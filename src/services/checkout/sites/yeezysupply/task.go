package yeezysupply

import (
	"context"
	"crypto/aes"
	crand "crypto/rand"
	"crypto/rsa"
	"encoding/base64"
	"encoding/hex"
	"errors"
	"fmt"
	"github.com/CenturionLabs/centurion/checkout-service/contracts"
	checkout "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout"
	"github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/config/yeezysupply"
	"github.com/CenturionLabs/centurion/checkout-service/core"
	"github.com/CenturionLabs/centurion/checkout-service/module_reflection"
	"github.com/CenturionLabs/centurion/checkout-service/services"
	"github.com/CenturionLabs/centurion/checkout-service/sites/common"
	"github.com/CenturionLabs/centurion/checkout-service/sites/yeezysupply/pixel"
	"github.com/CenturionLabs/centurion/checkout-service/util"
	"github.com/CrimsonAIO/aesccm"
	"github.com/anaskhan96/soup"
	"github.com/dlclark/regexp2"
	descriptor2 "github.com/golang/protobuf/descriptor"
	"github.com/golang/protobuf/proto"
	jsoniter "github.com/json-iterator/go"
	log "github.com/sirupsen/logrus"
	http "github.com/useflyent/fhttp"
	"github.com/useflyent/fhttp/cookiejar"
	"go.elastic.co/apm"
	"io/ioutil"
	"math/big"
	"math/rand"
	"net"
	net_http "net/http"
	"net/url"
	"regexp"
	"sort"
	"strconv"
	"strings"
	"sync"
	"time"
)

const (
	TracingSpanName = "checkout"
)

var (
	sensorUrlPattern             = regexp.MustCompile("<script type=\"text/javascript\"\\s\\ssrc=\"(.*?)\"")
	hawkApiUaMajorVersionPattern = regexp.MustCompile("Chrome/([0-9][0-9])")
	powPagePattern               = regexp.MustCompile("branding_url_content\":\"(.*?)\"")
	signupIdPattern              = regexp.MustCompile("yeezySupplySignupFormComponentId\\\\\":\\\\\"(.*?)\\\\\"")

	taskCounters = &sync.Map{}

	_, sizesEnumDescr = descriptor2.EnumDescriptorProto(yeezysupply.YeezySupplySize_YEEZYSUPPLY_SIZE_RANDOM)
	sizesMapping, _   = module_reflection.GetAllowedValuesFromEnumMap(sizesEnumDescr)
)

const (
	YeezySupplyDomainName              = "yeezysupply.com"
	YeezySupplyWWWDomainUrl            = "https://www." + YeezySupplyDomainName
	YeezySupplyWWWDomainUrlSlashEnding = "https://www." + YeezySupplyDomainName + "/"

	//YeezySupplyDomainName              = "ysstaging.andoverphilo.com"
	//YeezySupplyWWWDomainUrl            = "https://" + YeezySupplyDomainName
	//YeezySupplyWWWDomainUrlSlashEnding = "https://" + YeezySupplyDomainName + "/"

	YeezySupplyDomainUrl       = "https://" + YeezySupplyDomainName
	YeezySupplyQueueDefaultUrl = YeezySupplyWWWDomainUrl + "/__queue/yzysply"
)

type YeezySupplyTaskConfig struct {
	// default: xhwUqgFqfW88H50
	CookieV3Name        string
	YeezySupplyQueueUrl string
	QueuePollTime       time.Duration
	ConfigPollTime      time.Duration
	CaptchaSolveDelay   time.Duration
}

type displayTaskId struct {
	value uint64
}

type yeezySupplyTask struct {
	displayTaskId uint64
	sku           string
	offerId       string
	keywords      []string
	cookies       *cookiejar.Jar
	logger        *log.Logger

	dstrLock              services.DistributedLockFactory
	rpcManager            services.RpcManager
	payload               *core.CheckoutPayload
	captchaSolverProvider services.ReCaptchaSolverProvider
	http                  services.HttpClient
	hawkHttp              services.HttpClient
	step                  int

	utag     *Utag
	bmak     *Bmak
	pixelApi pixel.PixelAPI

	sensorUrl             *string
	fetchedSizes          []*ProductVariation
	checkoutAuthorization string
	f_int_c               int32
	f_int_2               int32
	basketId              string
	userAgent             string
	queueUrl              string
	harvesterId           string

	taskConfig        *YeezySupplyTaskConfig
	modeConfig        *yeezysupply.YeezySupplyDefaultConfig
	retryDelay        time.Duration
	monitorDelay      time.Duration
	availableProfiles []*contracts.ProfileData
	currentProfile    *contracts.ProfileData

	captchaToken        *services.CaptchaToken
	isFirstIteration    bool
	visitedBaskets      bool
	cartSelectedSkuSize string
	cartSelectedSize    string
}

func (t *yeezySupplyTask) GetCheckoutSteps() ([]core.CheckoutStep, error) {
	//switch supportedModes(strings.ToLower(t.payload.Mode)) {
	//case mode2:
	err := initializeMode2(t, false)
	if err != nil {
		t.payload.ReportUnexpectedError(err)
		return nil, err
	}

	return []core.CheckoutStep{
		t.configureProxy,
		t.initializeHarvester,
		t.init,
		t.prepareSplash,
		t.queue,
		t.prepareCart,
		t.cart,
		t.checkout,
	}, nil
	//default:
	//	return nil, errors.New("Not supported mode - " + t.payload.Mode)
	//}
}

type yeezySupplyProduct struct {
	Id                 string                       `json:"id"`
	Name               string                       `json:"name"`
	ViewList           []*ysProductViewItem         `json:"view_list"`
	ProductDescription *ysProductDescription        `json:"product_description"`
	PricingInformation *ysProductPricingInformation `json:"pricing_information"`
}

type ysProductViewItem struct {
	Type     string `json:"type"`
	ImageUrl string `json:"image_url"`
	Source   string `json:"source"`
}

type ysProductDescription struct {
	Title string `json:"title"`
}

type ysProductPricingInformation struct {
	CurrentPrice int64 `json:"currentPrice"`
}

func (t *yeezySupplyTask) FetchProduct() (*contracts.ProductData, error) {
	err := initializeMode2(t, true)
	if err != nil {
		t.payload.ReportUnexpectedError(err)
		return nil, err
	}

	if err := t.configureProxy(); err != nil {
		return nil, err.Error
	}

	if r := t.init(); r != nil {
		return nil, r.Error
	}

	req := t.createHomePageRequest()
	rtHeaderValue, err := GenRTValue()
	if err != nil {
		return nil, err
	}

	t.cookies.SetCookies(t.baseDomainUrl(), []*http.Cookie{
		{
			Name:  "RT",
			Value: rtHeaderValue,
		},
	})

	t.logger.
		WithFields(log.Fields{
			"RTCookie": rtHeaderValue,
		}).
		Debugln("Visiting home page")
	resp, err := t.http.Do(req)
	if err != nil {
		if err == services.HttpClient407ProxyAuthFailure {
			t.logger.WithField("proxy", t.http.GetUsedProxy().String()).Errorln("proxy auth failure")
			return nil, err
		}

		t.logger.Errorln(err)
		return nil, errors.New("Can't get homepage")
	}

	bodyBytes, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return nil, err
	}

	bodyStr := string(bodyBytes)

	isForbid := strings.Contains(bodyStr, "HTTP 403 - Forbidden")
	hasScripts := strings.Contains(bodyStr, "text/javascript") || strings.Contains(bodyStr, ".js\">")
	if isForbid && !hasScripts {
		time.Sleep(t.monitorDelay)
		t.step = 0 // restart task
		t.clearCookies()
		if r := t.rotateProxy(); r != nil {
			return nil, r.Error
		}

		return nil, errors.New(taskStatus_ProxyBanRestart.Title)
	}

	t.logger.
		WithFields(log.Fields{
			"RTCookie": rtHeaderValue,
			"Proxy":    t.http.GetUsedProxy().String(),
		}).
		Debugln("Fetching akamai info")
	if resp.StatusCode == http.StatusOK {
		//t.payload. ProgressConsumer <- taskStatus_HomePageLoaded
		akInfo, err := t.fetchAkamProtectionDetails(bodyStr)
		if err != nil {
			return nil, err
		}

		t.logger.Debugln("Fetching bloom")
		if r := t.fetchBloom(); r != nil {
			return nil, r.Error
		}

		t.logger.Debugln("Sending pixel")
		if r := t.sendPixel(akInfo.pixelSubmitUrl, akInfo.pixelId, akInfo.tval, akInfo.scriptVal); r != nil {
			return nil, r.Error
		}

		cookies := GenPrepareSplashCookies(t.utag)
		t.cookies.SetCookies(t.baseDomainUrl(), cookies)

		t.logger.Debugln("Fetching product info")
		req := t.createProdPageRequest()
		resp, err := t.http.Do(req)
		if err != nil {
			t.logger.Errorln(err)
			return nil, errors.New("Can't get product")
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			return nil, err
		}

		body := string(b)
		isSuccess := resp.StatusCode < 300
		notProtected := !strings.Contains(body, "HTTP 403 - Forbidden")
		if isSuccess && notProtected {
			t.logger.
				WithField("response", body).
				Debugln("Product fetched")

			prod := new(yeezySupplyProduct)
			if err := jsoniter.Unmarshal(b, prod); err != nil {
				return nil, err
			}

			price := float64(prod.PricingInformation.CurrentPrice)
			return &contracts.ProductData{
				Sku:    t.sku,
				Name:   prod.ProductDescription.Title,
				Image:  prod.ViewList[0].ImageUrl,
				Link:   YeezySupplyWWWDomainUrl + "/product/" + t.sku,
				Module: t.payload.Module,
				Price:  &price,
			}, nil
		}

		if !notProtected {
			t.payload.ReportError(taskStatus_BanUserInfoNo1)
			return nil, errors.New(taskStatus_BanUserInfoNo1.Title)
		}

		if resp.StatusCode == http.StatusNotFound {
			return nil, errors.New(taskStatus_PageNotFound.Title)
		}
	}

	return nil, errors.New("can't fetch product")
}

var baseDomainUrl, _ = url.Parse(YeezySupplyWWWDomainUrl)

func (t *yeezySupplyTask) baseDomainUrl() *url.URL {
	return baseDomainUrl
}

func (t *yeezySupplyTask) configureProxy() *core.StepExecutionFailure {
	//println("Configuring proxies")
	t.payload.ReportInProgress("Setup Proxy")
	span, _ := apm.StartSpan(t.payload.Context, "configureProxy", TracingSpanName)
	defer span.End()
	//println("Rotating proxies")
	if r := t.rotateProxy(); r != nil {
		return r
	}

	//println("Rotated proxies")
	t.step++
	return nil
}

func (t *yeezySupplyTask) GetUsedProfile() *contracts.ProfileData {
	return t.currentProfile
}

func (t *yeezySupplyTask) rotateProxy() *core.StepExecutionFailure {
	//println("if t.payload.ProxyPool == nil ")
	if t.payload.ProxyPool == nil {
		return t.payload.ReportUnexpectedError(errors.New("Proxy is required"))
	}

	//println("proxyList, err := util.GetProxyUrls(t.payload.ProxyPool)")
	proxyList, err := util.GetProxyUrls(t.payload.ProxyPool)
	if err != nil {
		return t.payload.ReportUnexpectedError(err)
	}

	//println("rndProxy, err := util.RandomUrl(proxyList)")
	rndProxy, err := util.RandomUrl(proxyList)
	if err != nil {
		return t.payload.ReportUnexpectedError(err)
	}

	//println("t.http.ChangeProxy(rndProxy)")
	t.http.ChangeProxy(rndProxy)
	//println("t.logger.WithField(\"proxy\", rndProxy.String()).Debugln(\"Proxy rotated\")")
	t.logger.WithField("proxy", rndProxy.String()).Infoln("Proxy rotated")
	return nil
}

func (t *yeezySupplyTask) Step() int {
	return t.step
}

func initializeMode2(t *yeezySupplyTask, partialInit bool) error {
	t.payload.ReportInProgress("Starting Task")
	//println("Initializing mode")
	t.logger.Debugln("Initializing mode")
	span, _ := apm.StartSpan(t.payload.Context, "initializeMode2", TracingSpanName)
	defer span.End()
	//println("Unmarshaling taskConfig")
	t.keywords = []string{
		t.payload.Product.Sku,
	}

	t.sku = t.payload.Product.Sku
	cookies, _ := cookiejar.New(nil)
	t.cookies = cookies
	t.http.UseCookieJar(t.cookies)
	t.isFirstIteration = true
	//println("Configured profiles")
	t.taskConfig = &YeezySupplyTaskConfig{
		CookieV3Name:        "xhwUqgFqfW88H50",
		YeezySupplyQueueUrl: YeezySupplyQueueDefaultUrl,
		QueuePollTime:       time.Millisecond * 3_000, /*1250*/
		ConfigPollTime:      time.Millisecond * 5_000,
		CaptchaSolveDelay:   time.Millisecond * 1_000, // let's check each sec if captcha expired
	}

	if partialInit {
		return nil
	}

	config := new(yeezysupply.YeezySupplyConfig)
	err := proto.Unmarshal(t.payload.Config, config)
	if err != nil {
		return err
	}

	if config.Mode == nil {
		return errors.New("task invalid: mode not configured")
	}

	//println("Unmarshaled taskConfig")

	modeCfg := config.Mode.(*yeezysupply.YeezySupplyConfig_Default).Default
	t.modeConfig = modeCfg
	//println("Setting taskConfig")
	t.retryDelay = modeCfg.RetryDelay.AsDuration()
	t.monitorDelay = modeCfg.MonitorDelay.AsDuration()

	//println("Configuring profiles")
	//profilesJson, _ := jsoniter.MarshalToString(t.payload.ProfileList)
	//println("Profile JSON")
	//println(profilesJson)
	srcProfiles := t.payload.ProfileList
	t.availableProfiles = make([]*contracts.ProfileData, 0, len(srcProfiles))

	for ix := range srcProfiles {
		profile := srcProfiles[ix]
		if isProfileValid(profile) {
			t.availableProfiles = append(t.availableProfiles, profile)
		}
	}

	if len(t.availableProfiles) == 0 {
		return errors.New("No Valid Profiles Found")
	}

	r := t.rotateProfile()
	if r != nil {
		return r
	}

	t.logger.Debugln("Initialized mode")
	//println("Initialized mode")

	return nil
}

func (t *yeezySupplyTask) prepareSplash() *core.StepExecutionFailure {
	for attempt := 0; attempt < 10 && t.payload.Context.Err() == nil; attempt++ {
		if attempt != 0 {
			time.Sleep(t.retryDelay)
		}

		t.payload.ReportInProgress("Preparing for Splash")
		req := t.createHomePageRequest()
		rtHeaderValue, err := GenRTValue()
		if err != nil {
			return t.payload.ReportUnexpectedError(err)
		}

		t.cookies.SetCookies(t.baseDomainUrl(), []*http.Cookie{
			{
				Name:  "RT",
				Value: rtHeaderValue,
			},
		})

		t.logger.
			WithFields(log.Fields{
				"RTCookie": rtHeaderValue,
				"attempt":  attempt,
			}).
			Debugln("Visiting home page")
		resp, err := t.http.Do(req)
		if err != nil {
			if isTimeout(err, resp) {
				return t.payload.ReportError(taskStatus_NoReply)
			}

			if err == services.HttpClient407ProxyAuthFailure {
				t.logger.WithField("proxy", t.http.GetUsedProxy().String()).Errorln("proxy auth failure")
				return t.payload.ReportUnexpectedError(err)
			}

			t.logger.Errorln(err)
			return t.payload.ReportUnexpectedFailure(errors.New("Can't get homepage"))
		}

		bodyBytes, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			return t.payload.ReportUnexpectedFailure(err)
		}

		bodyStr := string(bodyBytes)

		isForbid := strings.Contains(bodyStr, "HTTP 403 - Forbidden")
		hasScripts := strings.Contains(bodyStr, "text/javascript") || strings.Contains(bodyStr, ".js\">")
		if isForbid && !hasScripts {
			time.Sleep(t.monitorDelay)
			t.step = 0 // restart task
			t.clearCookies()
			if r := t.rotateProxy(); r != nil {
				return r
			}

			failure := t.payload.ReportError(taskStatus_ProxyBanRestart)
			time.Sleep(t.retryDelay)

			return failure
		}

		if resp.StatusCode == http.StatusOK {
			//t.payload. ProgressConsumer <- taskStatus_HomePageLoaded
			akInfo, err := t.fetchAkamProtectionDetails(bodyStr)
			if err != nil {
				return t.payload.ReportUnexpectedFailure(err)
			}

			if r := t.fetchBloom(); r != nil {
				return r
			}

			if r := t.sendPixel(akInfo.pixelSubmitUrl, akInfo.pixelId, akInfo.tval, akInfo.scriptVal); r != nil {
				return r
			}

			cookies := GenPrepareSplashCookies(t.utag)
			t.cookies.SetCookies(t.baseDomainUrl(), cookies)

			if _, r := t.visitProductPage(false); r != nil {
				return r
			}

			//t.payload. ReportInProgress("Fetching Assets")
			sharedJsonReq := t.createDownloadSharedJsonReq()
			if r := t.execReqWithoutProc(sharedJsonReq); r != nil {
				return r
			}

			downloadedEnUsJson := t.createDownloadEnUsJsonReq()
			if r := t.execReqWithoutProc(downloadedEnUsJson); r != nil {
				return r
			}

			if r := t.fetchSaleStatus(); r != nil {
				return r
			}

			t.step++
			return nil
		}
	}

	return t.payload.ReportError(taskStatus_CantVisitHomePage)
}

type akamInfo struct {
	pixelFetchUrl  string
	pixelSubmitUrl string
	tval           string
	pixelId        string
	scriptVal      string
}

func (t *yeezySupplyTask) fetchAkamProtectionDetails(homePageHtml string) (*akamInfo, error) {
	sensorUrl, r := t.grabSensorUrl(homePageHtml)
	if r != nil {
		return nil, r
	}

	t.sensorUrl = &sensorUrl
	//t.payload. ProgressConsumer <- taskStatus_FoundSensor

	r, akamBaza := t.grabAKAMAndBAZA(homePageHtml)
	if r != nil {
		return nil, r
	}

	r = t.fetchAkamaiScript()
	if r != nil {
		return nil, r
	}

	akamUrl1 := akamBaza[1]
	req := t.createPixelRequest(akamUrl1)
	// set timeout for 3_000ms
	akamResp, err := t.http.Do(req)
	if err != nil {
		t.logger.Errorln(err)
		return nil, errors.New("FW error")
	}

	akamBody, err := ioutil.ReadAll(akamResp.Body)
	if err != nil {
		return nil, err
	}

	akamBodyStr := string(akamBody)

	hexArr, err := pixel.GetHexArr(akamBodyStr)
	if err != nil {
		return nil, err
	}

	gindex, err := pixel.ParseGIndex(akamBodyStr)
	if err != nil {
		return nil, err
	}

	tval, err := pixel.ParseTVal(akamBaza[2])
	if err != nil {
		return nil, err
	}

	scriptVal := hexArr[gindex]
	pixelId := akamBaza[0]
	akamUrl2 := strings.Split(akamBaza[2], "?")[0] // probably this is just an url path without a search part

	return &akamInfo{
		pixelFetchUrl:  akamUrl1,
		pixelSubmitUrl: akamUrl2,
		tval:           tval,
		pixelId:        pixelId,
		scriptVal:      scriptVal,
	}, nil
}

func GenPrepareSplashCookies(utag *Utag) []*http.Cookie {
	cookiesMap := GenSplashCookies(utag)
	cookies := make([]*http.Cookie, len(cookiesMap), len(cookiesMap))
	i := 0
	for name, val := range cookiesMap {
		cookies[i] = &http.Cookie{
			Name:  name,
			Value: val,
		}
		i++
	}

	return cookies
}

func GenSplashCookies(utag *Utag) map[string]string {
	a5daysAhead := time.Now().UTC().Unix() + 7200
	timeInFuture := a5daysAhead + 597600

	amcvVal := "-227196251%7CMCIDTS%7C" + strconv.Itoa(GetIntH()) + "%7CMCMID%7C" + UtagGetRndDigitLongStr() +
		"%7CMCAAMLH-" + strconv.FormatInt(timeInFuture, 10) + "%7C7%7CMCAAMB-" + strconv.FormatInt(timeInFuture, 10) +
		"%7CRKhpRz8krg2tLO6pguXWp5olkAcUniQYPHaMWWgdJ3xzPWQmdj0y%7CMCOPTOUT-" + strconv.FormatInt(a5daysAhead, 10) + "s%7CNONE%7CMCAID%7CNONE"

	timefuture2 := strconv.Itoa(util.RandInt(1625112000771, 1625112000876))
	sPersLastToken := strconv.FormatInt(util.NowUtcMillis()+1_800_000, 10)
	sPersValue := "%20s_vnum%3D" + timefuture2 + "%2526vn%253D1%7C" + timefuture2 + "%3B%20s_invisit%3Dtrue%7C" + sPersLastToken + "%3B"
	utagMain := utag.GetMain()
	h := map[string]string{
		"geo_country":    "US",
		"utag_main":      utagMain,
		"_ga":            "GA1.2." + strconv.Itoa(util.RandInt(1207338862, 1992599043)) + "." + util.NowMillisStr(),
		"_gid":           "GA1.2." + strconv.Itoa(util.RandInt(120016221, 190016221)) + "." + util.NowMillisStr(),
		"_gat_tealium_0": "1",
		"_fbp":           "fb.1." + util.NowMillisStr() + strconv.Itoa(rand.Intn(1000)) + "." + util.NowMillisStr(),
		"_gcl_au":        "1.1." + util.NowMillisStr() + "." + util.NowMillisStr(),
		"AMCVS_7ADA401053CCF9130A490D4C%40AdobeOrg": "1",
		"AMCV_7ADA401053CCF9130A490D4C%40AdobeOrg":  amcvVal,
		"s_cc":   "true",
		"s_pers": sPersValue,
	}

	return h
}

// todo: remove it. it handled in httpService
func isTimeout(err error, resp *http.Response) bool {
	if resp == nil {
		return false
	}

	timeoutErr, ok := err.(net.Error)
	return ok && timeoutErr.Timeout()
}

func (t *yeezySupplyTask) execReqWithoutProc(req *http.Request) *core.StepExecutionFailure {
	resp, err := t.http.Do(req)
	if err != nil {
		t.logger.Errorln(err)
		return t.payload.ReportUnexpectedFailure(errors.New("Can't execute request. Cause: " + err.Error()))
	}

	if resp.StatusCode != http.StatusOK {
		// to do: maybe do something?
	}
	return nil
}

func (t *yeezySupplyTask) clearCookies() {
	jar, _ := cookiejar.New(nil)
	t.cookies = jar
	t.http.UseCookieJar(jar)
}

func (t *yeezySupplyTask) grabAKAMAndBAZA(body string) (error, []string) {
	res := make([]string, 3, 3)
	baza, err := pixel.ExtractBaza(body)
	if err != nil {
		return err, nil
	}

	res[0] = baza
	akam, err := pixel.ExtractAkam(body)
	if err != nil {
		return err, nil
	}

	res[1] = akam[0]
	res[2] = akam[1]

	return nil, res
}

func (t *yeezySupplyTask) fetchAkamaiScript() error {
	att := 0
	for t.payload.Context.Err() == nil && att < 50 {
		if att != 0 {
			time.Sleep(t.monitorDelay)
		}

		//t.payload. ReportInProgress("Interacting with FW")
		att++
		req, err := t.createFetchAkamaiScriptRequest(false)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
		}

		resp, err := t.http.Do(req)
		if err == nil && resp.StatusCode == http.StatusOK {
			return nil
		}

		if err != nil {
			t.logger.Errorln(err)
			return errors.New("FW fetch forbid")
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			continue
		}

		if strings.Contains(string(b), "HTTP 403 - Forbidden") {
			t.payload.ReportUnexpectedFailure(errors.New("FW failure"))
		}
	}

	return errors.New(taskStatus_CantVisitFW.Title)
}

func (t *yeezySupplyTask) fetchBloom() *core.StepExecutionFailure {
	retryDelay := t.retryDelay
	for att := 0; t.payload.Context.Err() == nil && att < 20; att++ {
		if att != 0 {
			time.Sleep(retryDelay)
		}

		retryDelay = t.retryDelay
		//t.payload. ReportInProgress("Fetching bloom")
		req := t.createBloomRequest()
		resp, err := t.http.Do(req)
		if err != nil {
			t.logger.Errorln(err)
			t.payload.ReportUnexpectedFailure(errors.New("Queue prepation error"))
			continue
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		body := string(b)
		if strings.Contains(body, "HTTP 403 - Forbidden") {
			t.payload.ReportError(taskStatus_ProxyBanCantFetchBloom)
			if r := t.rotateProxy(); r != nil {
				return r
			}

			retryDelay = time.Millisecond * time.Duration(util.RandInt(3000, 4500))

			continue
		}

		if resp.StatusCode != http.StatusOK {
			t.payload.ReportError(taskStatus_CantFetchBloomDueToUnexpectedError)
			continue
		}

		type bloomItem struct {
			ProductId   string `json:"product_id"`
			ProductName string `json:"product_name"`
		}

		items := []*bloomItem{}
		err = jsoniter.UnmarshalFromString(body, &items)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		for _, v := range items {
			if v.ProductId == t.sku {

				t.setUtagProductName(v.ProductName)
				t.cookies.SetCookies(t.baseDomainUrl(), []*http.Cookie{
					{Name: "UserSignUpAndSave", Value: "1"},
					{Name: "UserSignUpAndSaveOverlay", Value: "0"},
					{Name: "default_searchTerms_CustomizeSearch", Value: "%5B%5D"},
					{Name: "geoRedirectionAlreadySuggested", Value: "false"},
					{Name: "wishlist", Value: "%5B%5D"},
					{Name: "persistentBasketCount", Value: "0"},
					{Name: "userBasketCount", Value: "0"},
				})

				return nil
			}
		}
	}

	return t.payload.ReportError(taskStatus_CantFetchBloomDueToUnexpectedError)
}

func (t *yeezySupplyTask) prepareCart() *core.StepExecutionFailure {
	return t.executePrepareCart(true)
}

func (t *yeezySupplyTask) executePrepareCart(nextStepOnSuccess bool) *core.StepExecutionFailure {
	for att := 0; att < 50 && t.payload.Context.Err() == nil; att++ {
		if att != 0 {
			time.Sleep(time.Millisecond * time.Duration(util.RandInt(500, 1000)))
		}

		t.payload.ReportCheckingOutUnspecified("Cart Preparation")
		prodInfo, r := t.visitProductPage(true)
		if r != nil {
			continue
		}

		//t.payload. ReportCheckingOutUnspecified("Grabbing additional data")
		if sensorUrl, err := t.grabSensorUrl(prodInfo); err == nil {
			t.sensorUrl = &sensorUrl
			//t.payload. ProgressConsumer <- taskStatus_FoundSensor
		}

		t.utag.documentUrl = YeezySupplyWWWDomainUrl + "/product/" + t.sku
		t.cookies.SetCookies(t.baseDomainUrl(), []*http.Cookie{
			{Name: "utag_main", Value: t.utag.GetMain()},
		})

		t.payload.ReportCheckingOutUnspecified("Solving Protection")
		sensorReq, err := t.createFetchAkamaiScriptRequest(true)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}
		if _ /*sensorResp*/, err = t.http.Do(sensorReq); err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		if _, r = t.sendBasic("Prod Info #1"); r != nil {
			continue
		}

		if t.isFirstIteration {
			matches := signupIdPattern.FindAllStringSubmatch(prodInfo, -1)
			if matches != nil && len(matches) > 0 {
				newsletterPath := matches[0][1]
				newsletterReq := t.createNewsletterRequest(newsletterPath)
				_, err := t.http.Do(newsletterReq)
				if err != nil {
					t.logger.Errorln(err)
					continue
				}
			} else {
				t.logger.Errorln("Failed to signup to newsletters")
				return t.payload.ReportUnexpectedFailure(errors.New("Can't prepare splash"))
			}
		}

		//alreadyHasSomeSizes := len(t.fetchedSizes) > 0
		sizesRaw, r := t.fetchSizes(false /*alreadyHasSomeSizes*/)
		if r != nil {
			continue
		}

		//if alreadyHasSomeSizes && sizesRaw == nil {
		var info *ProductSize
		if err := jsoniter.Unmarshal(sizesRaw, &info); err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		t.fetchedSizes = info.VariationList
		sort.Slice(t.fetchedSizes, func(i, j int) bool {
			left := t.fetchedSizes[i]
			right := t.fetchedSizes[j]
			return left.Availability > right.Availability
		})
		//}

		//t.payload. ReportCheckingOutUnspecified("Posting sensor")
		if r = t.postSensors(contracts.TaskCategory_TASK_CATEGORY_UNSPECIFIED); r != nil {
			continue
		}

		if !t.isFirstIteration {
			basketReq := t.createCustomerBasketsRequest(true, t.checkoutAuthorization)
			//t.payload. ReportCheckingOutUnspecified("Visiting Customer Cart")
			if _ /*basketResp*/, err := t.http.Do(basketReq); err != nil {
				t.logger.Errorln(err)
				t.payload.ReportUnexpectedFailure(err)
				continue
			}
			t.visitedBaskets = true
		}

		t.isFirstIteration = false

		if nextStepOnSuccess {
			t.step++
		}

		return nil
	}

	return t.payload.ReportUnexpectedFailure(errors.New("Failed prepare for cart"))
}

func (t *yeezySupplyTask) grabSensorUrl(body string) (string, error) {
	matches := sensorUrlPattern.FindAllStringSubmatch(body, -1)
	sensorUrl := YeezySupplyWWWDomainUrl + "/staticweb/a187244de83ti185702c1d719b8369c09"
	if matches != nil && len(matches) > 0 {
		sensorUrl = matches[0][1]
	}

	if !strings.HasPrefix(sensorUrl, "http") {
		sensorUrl = YeezySupplyWWWDomainUrl + sensorUrl
	}

	return sensorUrl, nil
}

func (t *yeezySupplyTask) setQueueUrl(queueUrl string) {
	if t.bmak != nil {
		t.bmak.domain = queueUrl
	}

	t.utag.documentUrl = queueUrl
	t.queueUrl = queueUrl
}

func (t *yeezySupplyTask) visitProductPage(afterSplash bool) (string, *core.StepExecutionFailure) {
	stage := contracts.TaskStage_TASK_STAGE_RUNNING
	if afterSplash {
		stage = contracts.TaskStage_TASK_STAGE_CHECKING_OUT
	}

	for t.payload.Context.Err() == nil {
		respBytes, r := t.fetchProductPage(afterSplash, stage)
		if r != nil && r.HasFatalError {
			return "", r
		}

		if len(respBytes) > 0 {
			t.payload.ProgressConsumer <- common.TaskStatus_InStock(stage)
			//t.step++
			return string(respBytes), nil
		} else {
			t.payload.ProgressConsumer <- common.TaskStatus_OutOfStock
		}
	}

	return "", t.payload.ReportError(taskStatus_NoReply)
}

func (t *yeezySupplyTask) fetchProductPage(afterSplash bool, stage contracts.TaskStage) ([]byte, *core.StepExecutionFailure) {
	//t.payload. ProgressConsumer <- core.NewTaskStatus(contracts.TaskCategory_TASK_CATEGORY_UNSPECIFIED, "Checking Product Page", stage)
	req := t.createProductOrArchiveRequest(afterSplash)
	resp, err := t.http.Do(req)
	if err != nil {
		t.logger.Errorln(err)
		return nil, t.payload.ReportUnexpectedFailure(errors.New("Can't fetch product page"))
	}

	b, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return nil, t.payload.ReportUnexpectedFailure(err)
	}

	body := string(b)

	isForbid := strings.Contains(body, "HTTP 403 - Forbidden")
	hasScript := strings.Contains(body, "text/javascript") || strings.Contains(body, ".js\">")
	if isForbid && !hasScript {
		reportError := t.payload.ReportError(taskStatus_ProxyBanCantVisitProdPage)
		if r := t.rotateProxy(); r != nil {
			return nil, r
		}

		time.Sleep(t.monitorDelay)
		return nil, reportError
	}

	if resp.StatusCode == http.StatusOK {
		if afterSplash && strings.Contains(body, "wrgen_orig_assets/") {
			// TO DO: could be wrong origin
		}

		if !t.modeConfig.SkipQueue && !afterSplash && !strings.Contains(body, "wrgen_orig_assets/") {
			t.payload.ProgressConsumer <- common.TaskStatus_OutOfStock
			time.Sleep(time.Millisecond * 8000)
			return nil, nil
		}

		return b, nil
	}

	return nil, t.payload.ReportError(taskStatus_NoReply)
}

func (t *yeezySupplyTask) sendBasic(string2 string) (string, *core.StepExecutionFailure) {
	for att := 0; att < 100 && t.payload.Context.Err() == nil; att++ {
		//t.payload. ReportCheckingOutUnspecified("Fetching prod info")
		req := t.createProdPageRequest()
		resp, err := t.http.Do(req)
		if err != nil {
			t.logger.Errorln(err)
			t.payload.ReportUnexpectedFailure(errors.New("Can't get product"))
			continue
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			if isTimeout(err, resp) {
				time.Sleep(t.monitorDelay)
			}

			continue
		}

		body := string(b)
		isSuccess := resp.StatusCode < 300
		notProtected := !strings.Contains(body, "HTTP 403 - Forbidden")
		if isSuccess && notProtected {
			return body, nil
		}

		if !notProtected {
			t.payload.ReportError(taskStatus_BanUserInfoNo1)
			return "", nil
		}

		if resp.StatusCode == http.StatusNotFound {
			t.payload.ReportError(taskStatus_PageNotFound)
			return "", nil
		}

		time.Sleep(time.Millisecond * time.Duration(util.RandInt(3_000, 4_500)))
	}

	return "", t.payload.ReportError(taskStatus_FailedUserInfoNo1)
}

func (t *yeezySupplyTask) fetchSizes(alreadyHasSomeSizes bool) ([]byte, *core.StepExecutionFailure) {
	attemptsCount := util.RandInt(4, 10)
	for attempt := 0; attempt < attemptsCount && t.payload.Context.Err() == nil; attempt++ {
		//t.payload. ReportCheckingOutUnspecified("Fetching Sizes #" + strconv.Itoa(attempt+1))
		availabReq := t.createAvailabilityRequest()
		resp, err := t.http.Do(availabReq)
		if err != nil {
			t.logger.Errorln(err)
			t.payload.ReportUnexpectedFailure(errors.New("Can't fetch sizes"))
			continue
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			_ = t.payload.ReportUnexpectedFailure(err)
			if isTimeout(err, resp) {
				time.Sleep(t.monitorDelay)
			}

			continue
		}

		body := string(b)
		if alreadyHasSomeSizes {
			return nil, nil
		}

		notForbid := !strings.Contains(body, "HTTP 403 - Forbidden")
		if notForbid {
			if resp.StatusCode == http.StatusOK {
				return b, nil
			}

			t.payload.ReportUnexpectedFailure(errors.New("Failed to fetch sizes"))
		} else {
			t.payload.ReportUnexpectedFailure(errors.New("Failed to fetch sizes - banned"))
		}

		time.Sleep(time.Millisecond * 1000)
	}

	return nil, t.payload.ReportError(taskStatus_FailedFetchSizes)
}

func (t *yeezySupplyTask) postSensors(category contracts.TaskCategory) *core.StepExecutionFailure {
	t.payload.ReportCheckingOut("Sending Sensors", category)
	for att := 0; att < 3 && t.payload.Context.Err() == nil; att++ {
		if _, r := t.sendSensorData(false); r != nil && r.HasFatalError {
			return r
		}

		time.Sleep(time.Millisecond * time.Duration(util.RandInt(750, 750*1.5)))
	}

	maxAttempts := util.RandInt(4, 6)
	for att := 0; att <= maxAttempts && t.payload.Context.Err() == nil; att++ {
		time.Sleep(time.Millisecond * time.Duration(util.RandInt(1000, 1500)))

		sensorRespBytes, r := t.sendSensorData(true)
		if r != nil && r.HasFatalError {
			return r
		}

		sensorRespString := ""
		if sensorRespBytes != nil {
			sensorRespString = string(sensorRespBytes)
		}

		abck, _ := t.getCookieValue("_abck")
		if (len(sensorRespString) == 0 || !strings.Contains(sensorRespString, "false")) &&
			(len(abck) <= 530 || strings.Contains(abck, "||")) {
			//t.payload. ReportCheckingOut(fmt.Sprintf("Sensor Handling: %d/%d", att, maxAttempts), category)
			att--
			continue
		} /* else {
			t.payload. ReportCheckingOut(fmt.Sprintf("Sensor Pass: %d/%d", att, maxAttempts), category)
		}*/

		if len(sensorRespString) > 0 {
			// sensor posted
			//int int10001 = this.sensorPosts;
			//this.sensorPosts = (1 + (int10001 & -2) | (1 | ~int10001) - ~int10001) * 2 - (1 + (int10001 & -2) ^ (1 | ~int10001) - ~int10001);
			//this.logger.info("Posted sensor: [{}][{}][{}]", step, this.api.getCookies().getCookieValue("_abck").length(), string5.replace("\n", " "));
		} else {
			//this.logger.info("Sensor not posted properly. Continuing.");
		}
	}

	return nil
}

func (t *yeezySupplyTask) sendSensorData(isUppedBound bool) ([]byte, *core.StepExecutionFailure) {
	for att := 0; att < 20 && t.payload.Context.Err() == nil; att++ {
		if att != 0 {
			time.Sleep(t.retryDelay)
		}

		var sensorPayload *SensorPayload
		abck, err := t.getCookieValue("_abck")
		if err != nil {
			return nil, t.payload.ReportUnexpectedFailure(err)
		}

		hawkApi, _ := t.pixelApi.(*pixel.HawkAPI)
		var sensorJson string
		if sensorJson, err = hawkApi.GenAkamaiSensorData(abck, strings.ToUpper(t.keywords[0]), isUppedBound); err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		sensorPayload = &SensorPayload{SensorData: sensorJson}
		/*} else if strings.Contains(t.taskMode, "3") {
		    panic("Not support mode")
		    //var bmakJson string
		    //var err error
		    //if bmakJson, err = t.generateBmakSensorDataJsonStr(); err != nil {
		    //	return nil, t.payload.ReportUnexpectedFailure(err)
		    //}
		    //
		    //sensorPayload = new(SensorPayload)
		    //if err := jsoniter.UnmarshalFromString(bmakJson, sensorPayload); err != nil {
		    //	return nil, t.payload.ReportUnexpectedFailure(err)
		    //}
		  } else {
		    panic("Not support mode")
		    //abck, err := t.getCookieValue("_abck")
		    //if err != nil {
		    //	return nil, t.payload.ReportUnexpectedFailure(err)
		    //}
		    //
		    //ganeshApi, _ := t.pixelApi.(*pixel.GaneshAPI)
		    //var sensorJson string
		    //if sensorJson, err = ganeshApi.FetchAkamaiSensor(abck, strings.ToUpper(t.keywords[0])); err != nil {
		    //	return nil, t.payload.ReportUnexpectedFailure(err)
		    //}
		    //
		    //sensorPayload = &SensorPayload{SensorData: sensorJson}
		  }
		*/
		sensorJson, err = jsoniter.MarshalToString(sensorPayload)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		request, err := t.createSendSensorRequest(true, sensorJson)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		resp, err := t.http.Do(request)
		if err != nil {
			t.logger.Errorln(err)
			t.payload.ReportUnexpectedFailure(errors.New("Can't post sensors"))
			continue
		}

		if resp.StatusCode != http.StatusCreated {
			return nil, t.payload.ReportError(taskStatus_FailedSendSensorWithStatus(resp.StatusCode))
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			continue
		}

		return b, nil
	}

	return nil, t.payload.ReportError(taskStatus_FailedSendSensor)
}

func (t *yeezySupplyTask) getCookieValue(name string) (string, error) {
	cookies := t.cookies.Cookies(t.baseDomainUrl())
	for i := range cookies {
		c := cookies[i]
		if c.Name == name {
			return c.Value, nil
		}
	}

	return "", errors.New("Can't find cookie " + name)
}

func (t *yeezySupplyTask) setUtagProductName(bloom string) {
	t.utag.setProductName(bloom)
}

func (t *yeezySupplyTask) fetchSaleStatus() *core.StepExecutionFailure {
	if t.modeConfig.SkipQueue {
		return nil
	}
	for t.payload.Context.Err() == nil {
		statusCode, body := t.fetchWaitingRoomConfig()
		if statusCode == http.StatusOK {
			if strings.Contains(strings.ToLower(body), "sale_started") {
				t.payload.ReportInProgress("Sale Started")
				return nil
			}

			t.payload.ReportInProgress("Sale: OOS. Waiting...")
			time.Sleep(time.Millisecond * time.Duration(util.RandInt(8_000, 12_000)))
		} else {
			time.Sleep(t.monitorDelay)
		}
	}

	return t.payload.ReportError(taskStatus_Forbid)
}

func (t *yeezySupplyTask) fetchWaitingRoomConfig() (int, string) {
	//t.payload. ReportInProgress("Fetching sale status")
	jsonReq := t.createFetchWaitingRoomConfigJsonRequest()
	resp, err := t.http.Do(jsonReq)
	if err != nil {
		if isTimeout(err, resp) {
			t.payload.ReportError(taskStatus_NoReply)
		} else {
			t.logger.Errorln(err)
			t.payload.ReportUnexpectedFailure(errors.New("Unable to get sale status"))
		}

		if resp == nil {
			return 0, ""
		}

		return resp.StatusCode, ""
	}

	b, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		if isTimeout(err, resp) {
			t.payload.ReportError(taskStatus_NoReply)
		} else {
			t.payload.ReportUnexpectedFailure(err)
		}

		return resp.StatusCode, ""
	}

	body := string(b)
	if strings.Contains(body, "HTTP 403 - Forbidden") {
		_ = t.payload.ReportUnexpectedFailure(errors.New("Sale Status: proxy banned"))
		t.rotateProxy()
		return resp.StatusCode, ""
	}

	return resp.StatusCode, body
}

func (t *yeezySupplyTask) generateBmakSensorDataJsonStr() (string, error) {
	t.bmak.domain = YeezySupplyWWWDomainUrl + "/product/" + t.sku
	abck, err := t.getCookieValue("_abck")
	if err != nil {
		return "", err
	}

	prev := t.f_int_c
	t.f_int_c += 1
	switch prev {
	case 0:
		return t.bmak.GenerateAndGetSensorJsonStr(abck)
	case 1:
		return t.bmak.GenerateAndGetSensorJsonStr2(abck)
	default:
		if _, err = t.refreshMobileDeviceStateJson(); err != nil {
			return "", err
		}

		t.f_int_c = 1
		return t.bmak.GenerateAndGetSensorJsonStr(abck)
	}
}

func (t *yeezySupplyTask) refreshMobileDeviceStateJson() (bool, error) {
	/*for att := 0; att < 350000 && t.payload.Context.Err() == nil; att++ {
	  if strings.Contains(t.taskMode, "2") || strings.Contains(t.taskMode, "3") {*/
	return true, nil
	/*}

	  panic("other mode not supported")*/

	/*
	      var fetchDeviceState = VertxSingleton.INSTANCE.getLocalClient()
	           .fetchJson("https://loudounchris.xyz/api/device/browserDeviceQueried.json?mobile=false&ak=true", this.userAgent);
	       if (!fetchDeviceState.isDone()) {
	         return fetchDeviceState.exceptionally(Function.identity()).thenCompose(YeezyAPI::completableFutureYeezyAPIIntYeezyAPICompletableFutureThrowableIntObjectc);
	       }

	       this.modileDeviceStateJson = fetchDeviceState.join();
	       if (this.modileDeviceStateJson == null || this.modileDeviceStateJson.isEmpty()) {
	         continue;
	       }

	       this.bmak = new Bmak(this.modileDeviceStateJson);
	       this.f_Pixel_c = new TrickleAPI(this.modileDeviceStateJson);
	       if (this.bmak == null) {
	         continue;
	       }

	       if (this.userAgent != null) {
	         return CompletableFuture.completedFuture(Boolean.TRUE);
	       }
	     }
	     return CompletableFuture.completedFuture(Boolean.FALSE);

	}*/

	return false, errors.New("Failed to refresh sensor")
}

type addToCartPayload struct {
	ProductId           string `json:"product_id"`
	ProductVariationSku string `json:"product_variation_sku"`
	ProductSku          string `json:"productId"`
	Quantity            int32  `json:"quantity"`
	Size                string `json:"size"`
	DisplaySize         string `json:"displaySize"`
}

func (t *yeezySupplyTask) cart() *core.StepExecutionFailure {
	t.payload.ReportCheckingOutUnspecified("Adding to Cart")
	cartPayload, err := t.getAddToCartPayload()
	if err != nil {
		return t.payload.ReportUnexpectedError(err)
	}

	for t.payload.Context.Err() == nil {
		req, err := t.createAddToCartRequest(cartPayload)
		if err != nil {
			return t.payload.ReportUnexpectedFailure(err)
		}

		/*sendSensorAtt := 0
		  for sendSensorAtt > 5 && t.payload.Context.Err() == nil {
		  	sensorRespBytes, r := t.sendSensorData(true)
		  	if r != nil && r.HasFatalError {
		  		return r
		  	}

		  	sensorRespString := ""
		  	if sensorRespBytes != nil {
		  		sensorRespString = string(sensorRespBytes)
		  	}

		  	abck, _ := t.getCookieValue("_abck")
		  	if (len(sensorRespString) == 0 || !strings.Contains(sensorRespString, "false")) &&
		  		(len(abck) <= 530 || strings.Contains(abck, "||")) {
		  		sendSensorAtt++
		  		continue
		  	}

		  	break
		  }*/

		resp, err := t.http.Do(req)
		if err != nil {
			t.logger.Errorln(err)
			return t.payload.ReportUnexpectedFailure(errors.New("Failed to ATC. FW"))
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			return t.payload.ReportUnexpectedFailure(err)
		}

		var failure = t.payload.ReportUnexpectedFailure(errors.New("Unexpected failure ATC"))
		rawJson := string(b)
		t.logger.WithField("response", rawJson).Infoln("Atc response")
		if resp.StatusCode == http.StatusForbidden {
			failure = t.payload.ReportUnexpectedFailure(errors.New("ATC - bad cookies"))
			/*t.GenPrepareSplashCookies()

			  t.visitProductPage(true)

			  t.payload. ReportInProgress("Fetching Assets")
			  sharedJsonReq := t.createDownloadSharedJsonReq()
			  t.execReqWithoutProc(sharedJsonReq)

			  downloadedEnUsJson := t.createDownloadEnUsJsonReq()
			  t.execReqWithoutProc(downloadedEnUsJson)
			  t.fetchSizes(false)
			  t.postSensors()
			  t.createCustomerBasketsRequest(true, t.checkoutAuthorization)*/
		} else {
			rawJson = strings.Replace(rawJson, "\n", "", -1)
			t.logger.Errorln("ATC: " + rawJson)
			if strings.Contains(rawJson, "branding_url_content") {
				if r := t.handlePOW(rawJson); r != nil && r.HasFatalError {
					return r
				}
			}

			if strings.Contains(rawJson, "basketId") {
				// to do: enable it
				t.checkoutAuthorization = resp.Header.Get("Authorization")
				if len(t.checkoutAuthorization) == 0 {
					failure = t.payload.ReportUnexpectedError(errors.New("No authorization header"))
				} else if !strings.Contains(rawJson, "\"total\":0") {
					//prev := t.f_int_2
					t.f_int_2 += 1
					//this.logger.info("ATC SUCCESSFUL[{}]: {}", int10002, rawJson);
					t.payload.ReportCarted("Successful ATC")
					t.basketId = jsoniter.Get(b, "basketId").ToString()

					restoreBasketUrl := "%2Fon%2Fdemandware.store%2FSites-ys-US-Site%2Fen_US%2FCart-UpdateItems%3Fpid_0%3D" + t.cartSelectedSkuSize + "%26qty_0%3D1%26"

					t.logger.WithFields(log.Fields{
						"basketId":         t.basketId,
						"restoreBasketUrl": restoreBasketUrl,
					}).Infoln("Successful ATC")
					t.cookies.SetCookies(baseDomainUrl, []*http.Cookie{
						{Name: "userBasketCount", Value: "1"},
						{Name: "persistentBasketCount", Value: "1"},
						{Name: "restoreBasketUrl", Value: restoreBasketUrl},
					})

					t.step++

					return nil
				} else {
					//this.logger.info("EMPTY_CART (OOS): {}", rawJson);

					failure = t.payload.ReportUnexpectedFailure(errors.New("Empty cart"))
				}
			} else if len(rawJson) == 0 {
				failure = t.payload.ReportUnexpectedFailure(errors.New("ATC: Blocked by FW"))
			} else if !strings.Contains(rawJson, "HTTP 403 - Forbidden") {
				if resp.StatusCode == http.StatusBadRequest {
					failure = t.payload.ReportUnexpectedFailure(errors.New("ATC: bad session"))
				} else {
					failure = t.payload.ReportUnexpectedFailure(errors.New("ATC: No basket"))
				}
			} else {
				failure = t.payload.ReportUnexpectedFailure(errors.New("ATC: forbid"))
			}
		}

		//failure := t.payload.ReportUnexpectedFailure(errors.New("Failed ATC"))
		t.step--
		time.Sleep(t.retryDelay)
		return failure
	}

	return t.payload.ReportError(taskStatus_FailedAddToCart)
}

// todo: test it
func getSize(fetchedSizes []*ProductVariation, allowedSizes []yeezysupply.YeezySupplySize, sku string) (*ProductVariation, error) {
	//if len(fetchedSizes) == 0 || len(allowedSizes) == 0 {
	//  rndSize := getRandomSize()
	//  return createProductVariation(rndSize, sku)
	//}
	//
	//if len(fetchedSizes) > 0 {
	//  if isRndSizeSelected(allowedSizes) {
	//    return selectRandom(fetchedSizes), nil
	//  }
	//}

	if randomSizeSelectedAndSizesFetched(fetchedSizes, allowedSizes) {
		return selectRandom(fetchedSizes), nil
	} else if notRandomAndSizesFetched(fetchedSizes, allowedSizes) /* not random and fetched */ {
		filtered := filterSizesByStrList(fetchedSizes, allowedSizes)
		return selectRandom(filtered), nil
	} else if randomSizeSelectedOrNoFetchedSizes(fetchedSizes, allowedSizes) {
		rndSize := getRandomSize()
		return createProductVariation(rndSize, sku)
	} else {
		rnd := int32(allowedSizes[rand.Intn(len(allowedSizes))])
		value := sizesMapping[rnd]
		return createProductVariation(value.Value.DisplayName, sku)
	}
}

func isRndSizeSelected(allowedSizes []yeezysupply.YeezySupplySize) bool {
	for _, s := range allowedSizes {
		if s == yeezysupply.YeezySupplySize_YEEZYSUPPLY_SIZE_RANDOM {
			return true
		}
	}

	return false
}

func notRandomAndSizesFetched(fetchedSizes []*ProductVariation, allowedSizes []yeezysupply.YeezySupplySize) bool {
	return len(allowedSizes) > 0 && allowedSizes[0] != yeezysupply.YeezySupplySize_YEEZYSUPPLY_SIZE_RANDOM &&
		len(fetchedSizes) > 0
}

func randomSizeSelectedAndSizesFetched(availableSizes []*ProductVariation, allowedSizes []yeezysupply.YeezySupplySize) bool {
	return len(allowedSizes) == 1 && allowedSizes[0] == yeezysupply.YeezySupplySize_YEEZYSUPPLY_SIZE_RANDOM && len(availableSizes) > 0
}

func randomSizeSelectedOrNoFetchedSizes(availableSizes []*ProductVariation, allowedSizes []yeezysupply.YeezySupplySize) bool {
	return len(allowedSizes) == 1 && allowedSizes[0] == yeezysupply.YeezySupplySize_YEEZYSUPPLY_SIZE_RANDOM || len(availableSizes) == 0
}

func createProductVariation(size string, sku string) (*ProductVariation, error) {
	parsedSize := parseSize(size)
	idWithSize := sku + "_" + parsedSize
	return &ProductVariation{
		Availability: 1,
		Sku:          idWithSize,
		Size:         size,
	}, nil
}

func (t *yeezySupplyTask) getAddToCartPayload() ([]*addToCartPayload, error) {
	var cartPayload *addToCartPayload
	selected, err := getSize(t.fetchedSizes, t.modeConfig.Sizes, t.sku)
	if err != nil {
		return nil, err
	}

	t.cartSelectedSkuSize = selected.Sku
	t.cartSelectedSize = selected.Size
	cartPayload = mapToCartPayload(t.sku, selected)
	return []*addToCartPayload{cartPayload}, nil
}

func getRandomSize() string {
	return sizes[rand.Intn(len(sizes))]
}

func filterSizesByStrList(sizes []*ProductVariation, allowedSizes []yeezysupply.YeezySupplySize) []*ProductVariation {
	var filtered []*ProductVariation
	for ix, _ := range sizes {
		cs := sizes[ix]
		for six, _ := range allowedSizes {
			selectedSize := int32(allowedSizes[six])
			value := sizesMapping[selectedSize]
			if cs.Size == value.Value.DisplayName {
				filtered = append(filtered, cs)
			}
		}
	}

	return filtered
}

func mapStrSizeToCartPayload(productId string, sizeStr string) *addToCartPayload {
	parsedSize := parseSize(sizeStr)
	idWithSize := productId + parsedSize
	return &addToCartPayload{
		ProductId:           productId,
		ProductVariationSku: idWithSize,
		ProductSku:          idWithSize,
		Quantity:            1,
		Size:                sizeStr,
		DisplaySize:         sizeStr,
	}
}

//var sizes = map[float32]string {
//  4.0: "530",
//  4.5: "540",
//  5.0: "550",
//  5.5: "560",
//  6.0: "570",
//  6.5: "580",
//  7.0: "590",
//  7.5: "600",
//  8.0: "610",
//  8.5: "620",
//  9.0: "630",
//  9.5: "640",
//  10: "650",
//  10.5: "660",
//  11.0: "670",
//  11.5: "680",
//  12.0: "690",
//  12.5: "700",
//  13.0: "710",
//  13.5: "720",
//  14.5: "740",
//  15.0: "750",
//  16.0: "760",
//  17.0: "780",
//}
var sizes = []string{}

const (
	startYSSize = 530
	minSize     = 4
	maxSize     = 17
)

func init() {
	for i := float32(minSize); i < maxSize; i = i + .5 {
		sizes = append(sizes, parseSize(strconv.FormatFloat(float64(i), 'f', 1, 64)))
	}
}

func parseSize(sizeStr string) string {
	fsize, err := strconv.ParseFloat(sizeStr, 0)
	if err != nil {
		panic(err)
	}

	return strconv.Itoa(int(startYSSize + ((fsize-minSize)/.5)*10))
}

func mapToCartPayload(productId string, selected *ProductVariation) *addToCartPayload {
	return &addToCartPayload{
		ProductId:           productId,
		ProductVariationSku: selected.Sku,
		ProductSku:          selected.Sku,
		Quantity:            1,
		Size:                selected.Size,
		DisplaySize:         selected.Size,
	}
}

func selectRandom(sizes []*ProductVariation) *ProductVariation {
	ix := rand.Intn(len(sizes))
	return sizes[ix]
}

func (t *yeezySupplyTask) getProductUrl() string {
	return YeezySupplyWWWDomainUrl + "/product/" + t.sku
}

type waitingRoomCfg struct {
	EnableAvailabilityGrid bool     `json:"enableAvailabilityGrid"`
	EnableRecaptchaV3      bool     `json:"enableRecaptchaV3"`
	UseWrSlim              bool     `json:"useWrSlim"`
	YeezySupplyWrMessage   []string `json:"yeezySupplyWrMessage"`
	YsStatusMessageKey     string   `json:"ysStatusMessageKey"`
}

func (t *yeezySupplyTask) pollWaitingRoomCfg() {
	wRoomCfgPollSpan, _ := apm.StartSpan(t.payload.Context, "WaitingRoomCfgPoll", "waiting_room_cfg")
	defer wRoomCfgPollSpan.End()
	//t.payload. ReportInProgress("Fetch Config")
	statusCode, cfgJson := t.fetchWaitingRoomConfig()
	wRoomCfgPollSpan.Context.SetHTTPStatusCode(statusCode)
	if statusCode != http.StatusOK {
		t.payload.ReportUnexpectedError(errors.New("OOS"))
		return
	}

	roomCfg := new(waitingRoomCfg)
	err := jsoniter.UnmarshalFromString(cfgJson, roomCfg)
	if err == nil {
		//t.payload. ReportInProgress(roomCfg.YeezySupplyWrMessage[rand.Intn(len(roomCfg.YeezySupplyWrMessage))])
	} else {
		t.payload.ReportUnexpectedFailure(errors.New("Not available yet"))
	}
}

func (t *yeezySupplyTask) pollQueue() bool {
	queuePollSpan, _ := apm.StartSpan(t.payload.Context, "QueuePoll", "queue_poll")
	defer queuePollSpan.End()

	// NOTICE: server will expire cookie by himself
	t.cookies.SetCookies(t.baseDomainUrl(), []*http.Cookie{
		{Name: "akavpwr_ys_us", Expires: time.Unix(-1, -1)},
		//{Name: "akavpfq_ys_us", Expires: time.Unix(-1, -1)},
	})

	if t.captchaToken.GetCookieV3() != nil {
		t.cookies.SetCookies(t.baseDomainUrl(), []*http.Cookie{
			{Name: t.taskConfig.CookieV3Name, Value: *t.captchaToken.GetCookieV3()},
			//{Name: "_GRECAPTCHA", Value: "09AINsHFe5yc-c6HBNlYi25YjsXg39yFRu45WJXFqCk1ccdUlpv1dEXcxFRA41PRO8VVXu5ZOZJB0yPttai5SrZU4"},
		})
	}

	req := t.createQueueRequest()
	resp, err := t.http.Do(req)
	if resp != nil {
		queuePollSpan.Context.SetHTTPStatusCode(resp.StatusCode)
		SetHTTPRequest(&queuePollSpan.Context, req)
	}

	var cookies = t.cookies.Cookies(t.baseDomainUrl())
	for ix := range cookies {
		c := cookies[ix]
		queuePollSpan.Context.SetLabel("cookies."+c.Name, c.Value)
	}

	if err != nil {
		t.logger.Errorln(err)
		_ = t.payload.ReportUnexpectedFailure(errors.New("Can't poll queue"))
		return false
	}

	serializedCookies := t.getCookiesStr()
	//isOk := resp.StatusCode == http.StatusOK
	hasCookieU := t.hasCookie(t.taskConfig.CookieV3Name+"_u", baseDomainUrl)
	hasHmac := strings.Contains(serializedCookies, "hmac")
	if /*isOk && */ hasCookieU && hasHmac {
		isFakePass := strings.Contains(serializedCookies, "data=1~")
		if isFakePass {
			t.captchaToken.Expire()
			t.payload.ReportError(taskStatus_FakeQueuePass)
			t.logger.Warnln("Fake queue pass!!!")
			t.cookies.SetCookies(t.baseDomainUrl(), []*http.Cookie{
				{Name: t.taskConfig.CookieV3Name + "_u", Expires: time.Unix(-1, -1)},
			})
			return false
		}

		t.cookies.SetCookies(t.baseDomainUrl(), []*http.Cookie{
			{Name: t.taskConfig.CookieV3Name, Expires: time.Unix(-1, -1)},
		})

		return true
	}

	if resp.StatusCode == http.StatusTemporaryRedirect {
		// todo: detect stackoverflow
		return t.pollQueue()
	}

	return false
}

func SetHTTPRequest(c *apm.SpanContext, req *http.Request) {
	if req.URL == nil {
		return
	}
	toSet := &net_http.Request{
		URL:    req.URL,
		Header: net_http.Header(req.Header),
		Method: req.Method,
	}

	c.SetHTTPRequest(toSet)
}

func (t *yeezySupplyTask) queue() *core.StepExecutionFailure {
	splashPassSpan, _ := apm.StartSpan(t.payload.Context, "PassSplash", "splash")
	defer func() {
		splashPassSpan.End()
		//t.http.SetFollowRedirects(false)
		//t.logger.Debugln("AutoRedirects disabled")
	}()

	//t.payload. ReportInProgress("In queue")
	t.setQueueUrl(t.taskConfig.YeezySupplyQueueUrl)

	//t.http.SetFollowRedirects(true)
	main := t.utag.GetMain()
	t.cookies.SetCookies(t.baseDomainUrl(), []*http.Cookie{
		{Name: "akavpwr_ys_us", Expires: time.Unix(-1, -1)},
		{Name: "akavpfq_ys_us", Expires: time.Unix(-1, -1)},
		{Name: "PH0ENIX", Value: "false"},
		{Name: "utag_main", Value: main},
	})

	if t.modeConfig.SkipQueue {
		t.step++
		t.payload.ReportInProgress("Splash Skipped")
		return nil
	}
	for t.payload.Context.Err() == nil {
		r := t.solveCaptcha()
		if r != nil {
			if r.HasFatalError {
				return r
			} else {
				continue
			}
		}

		break
	}

	wg := &sync.WaitGroup{}
	wg.Add(+1)
	queuePassCtx, queuePollCancelFn := context.WithCancel(t.payload.Context)

	// cfg fetch
	go func() {
		for queuePassCtx.Err() == nil {
			start := time.Now()
			t.pollWaitingRoomCfg()
			delta := time.Now().Sub(start)
			pollTime := t.taskConfig.ConfigPollTime - delta
			t.logger.Debugln("Waiting room poll time: " + strconv.FormatInt(int64(pollTime), 10))

			t.payload.ReportInProgress("In queue")
			time.Sleep(pollTime)
		}
	}()

	splashPassed := false
	// queue poll
	go func() {
		defer func() {
			queuePollCancelFn()
			wg.Done()
			t.logger.Debugln("Finished queue poll loop")
		}()

		for queuePassCtx.Err() == nil {
			start := time.Now()
			if splashPassed = t.pollQueue(); splashPassed {
				queuePollCancelFn()
				break
			}

			delta := time.Now().Sub(start)
			time.Sleep(t.taskConfig.QueuePollTime - delta)
			//t.payload. ReportInProgress("Not available yet")
		}
	}()

	// captcha solve
	go func() {
		for queuePassCtx.Err() == nil {
			// to do: enable it
			r := t.solveCaptcha()
			if r != nil {
				continue
			}

			time.Sleep(t.taskConfig.CaptchaSolveDelay)
		}
	}()

	wg.Wait()
	t.logger.Debugln("Finished queue poll method")
	if splashPassed {
		t.step++
		t.payload.ReportInProgress("Splash Passed")
		return nil
	}

	return t.payload.ReportError(taskStatusFailedToPassQueue)
}

func (t *yeezySupplyTask) checkout() *core.StepExecutionFailure {
	if t.visitedBaskets {
		_ = t.postSensors(contracts.TaskCategory_TASK_CATEGORY_CARTED)
	}

	t.payload.ReportCheckingOut("Shipping and Billing", contracts.TaskCategory_TASK_CATEGORY_CARTED)
	r := t.submitBillingAndShipping(t.basketId, t.checkoutAuthorization)
	if r != nil {
		return r
	}

	t.cookies.SetCookies(baseDomainUrl, []*http.Cookie{
		{Name: "UserSignUpAndSaveOverlay", Value: "1"},
		{Name: "pagecontext_cookies", Value: ""},
		{Name: "pagecontext_secure_cookies", Value: ""},
	})

	t.payload.ReportCheckingOut("Purchasing", contracts.TaskCategory_TASK_CATEGORY_CARTED)
	return t.processPayment(t.checkoutAuthorization, t.basketId, t.currentProfile)
}

func (t *yeezySupplyTask) rotateProfile() error {
	if len(t.availableProfiles) == 0 {
		t.availableProfiles = t.payload.ProfileList[:]
	}

	t.currentProfile = t.availableProfiles[0]
	for len(t.payload.ProfileList) > 1 {
		ix := rand.Intn(len(t.payload.ProfileList))
		t.currentProfile = t.payload.ProfileList[ix]

		// to skip used profiles
		removeFast(t.availableProfiles, ix)
		if isProfileValid(t.currentProfile) {
			break
		}
	}

	if len(t.payload.ProfileList) == 0 {
		return errors.New("No Valid Profile Found")
	}

	return nil
}

func (t *yeezySupplyTask) initializeHarvester() *core.StepExecutionFailure {
	task := t.payload
	//cfg := services.ExecuteOnceAndShareConfig{
	//	MutexNameFactory: func() string {
	//		return "HarvestersInit_" + task.Module.String() + "__" + task.UserId
	//	},
	//	RawValueReceiver: func(rawValue []byte) *core.StepExecutionFailure {
	//		if len(rawValue) == 0 {
	//			t.step++
	//			return nil
	//		}
	//
	//		return t.payload.ReportUnexpectedFailure(errors.New(string(rawValue)))
	//	},
	//	CheckoutPayload: t.payload,
	//	ValueProducer: func(consumer func(rawValue []byte) *core.StepExecutionFailure) *core.StepExecutionFailure {
	//		harvesterId, err := t.rpcManager.InitializeHarvester(task.UserId)
	//		var result []byte
	//		if err != nil {
	//			result = []byte(err.Error())
	//		}
	//
	//		consumer(result)
	//		return nil
	//	},
	//	DstrLock: t.dstrLock,
	//}

	for att := 0; att < 350000 && t.payload.Context.Err() == nil; att++ {
		t.payload.ReportInProgress("Checking for Harvesters")
		//if r := cfg.ExecuteOnceAndShare(); r == nil {
		//	return nil
		//}

		harvesterId, err := t.rpcManager.InitializeHarvester(task.UserId)
		if err == nil {
			t.harvesterId = harvesterId
			t.step++
			return nil
		}

		t.payload.ReportUnexpectedFailure(err)
		time.Sleep(t.retryDelay)
	}

	return t.payload.ReportUnexpectedFailure(errors.New("Failed start harvesters"))
}

func (t *yeezySupplyTask) init() *core.StepExecutionFailure {
	t.payload.ReportInProgress("Initialization")
	api := pixel.NewHawkAPI(t.hawkHttp)
	for t.payload.Context.Err() == nil {
		time.Sleep(300 * time.Millisecond)
		//t.hawkHttp = services.NewHttpClient(t.http.GetTlsIdName(), t.http.GetUsedProxy())
		//t.hawkHttp.SetTimeout(time.Second * 5)
		t.pixelApi = api
		ua, err := api.GetUserAgent()
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		if len(ua) == 0 {
			continue
		}

		matches := hawkApiUaMajorVersionPattern.FindAllStringSubmatch(ua, -1)
		if len(matches) == 0 || len(matches[0]) == 0 || !strings.HasPrefix(matches[0][1], "9") {
			continue
		}

		t.utag = CreateUtag(ua, t.sku, func() string {
			return t.queueUrl
		})

		t.userAgent = ua
		t.step++
		break
	}

	return nil
	/*if strings.Contains(t.taskMode, "3") {
	  	panic("Not supported mode")
	  	//api := &pixel.GaneshAPI{}
	  	//t.pixelApi = api
	  	//ua, err := api.GetUserAgent()
	  	//if err != nil {
	  	//	t.payload.ReportUnexpectedFailure(err)
	  	//	continue
	  	//}
	  	//
	  	//if len(ua) == 0 {
	  	//	continue
	  	//}
	  	//
	  	//t.utag = CreateUtag(ua, t.sku, func() string {
	  	//	return t.queueUrl
	  	//})
	  	//
	  	//t.step++
	  	//
	  	//return nil
	  }

	  panic("Not supported mode")*/

	/*

	   var completableFuture10 = VertxSingleton.INSTANCE.getLocalClient()
	       .fetchJsonWithDefaultUA("https://loudounchris.xyz/api/device/browserDevice.json?mobile=false&ak=true");
	   if (!completableFuture10.isDone()) {
	     return completableFuture10.exceptionally(Function.identity()).thenCompose(YeezyAPI::nonDeobf_completableFutureYeezyAPIIntHawkAPIYeezyAPICompletableFutureGaneshAPIThrowableIntObjectc);
	   }

	   this.modileDeviceStateJson = completableFuture10.join();
	   if (this.modileDeviceStateJson != null && !this.modileDeviceStateJson.isEmpty()) {
	     this.bmak = new Bmak(this.modileDeviceStateJson);
	     this.f_Utag_c = new Utag(this.bmak, this.getProductId());
	     this.userAgent = this.bmak.getUserAgent();
	     this.f_Pixel_c = new TrickleAPI(this.modileDeviceStateJson);
	     if (this.bmak != null && this.userAgent != null && this.f_Pixel_c != null) {
	       return CompletableFuture.completedFuture(Boolean.TRUE);
	     }
	   }
	*/
}

func removeFast(profiles []*contracts.ProfileData, i int) []*contracts.ProfileData {
	profiles[i] = profiles[len(profiles)-1]
	return profiles[:len(profiles)-1]
}

func (t *yeezySupplyTask) submitBillingAndShipping(basketId, checkoutAuthorization string) *core.StepExecutionFailure {
	billingShippingJsonStr, _ := CreateBillingShippingJsonAlter(t.currentProfile)
	req, err := t.createCheckoutBasketRequest(basketId, checkoutAuthorization, billingShippingJsonStr)
	t.logger.WithField("payload", billingShippingJsonStr).Infoln("Sending billing and shipping")
	if err != nil {
		return t.payload.ReportUnexpectedFailure(err)
	}

	t.utag.documentUrl = YeezySupplyWWWDomainUrl + "/delivery"
	t.cookies.SetCookies(baseDomainUrl, []*http.Cookie{
		{Name: "utag_main", Value: t.utag.GetMain()},
	})
	for attempt := 0; attempt < 250 && t.payload.Context.Err() == nil; attempt++ {
		if attempt != 0 {
			time.Sleep(t.retryDelay)
			t.postSensors(contracts.TaskCategory_TASK_CATEGORY_CARTED)
		}

		t.payload.ReportCheckingOut("Submitting billing", contracts.TaskCategory_TASK_CATEGORY_CARTED)

		resp, err := t.http.Do(req)
		if err != nil {
			t.logger.Errorln(err)
			t.payload.ReportUnexpectedFailure(errors.New("Billing & shipping post error"))
			continue
		}

		//this.logger.info("ShippingBilling: [{}]{}", response.statusCode(), response.body());
		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		bodyJson := string(b)
		t.logger.WithField("response", bodyJson).Infoln("ShippingBillingResponse")
		if resp.StatusCode != http.StatusOK {
			if strings.Contains(bodyJson, "The shipping address postal code is blacklisted") {
				err = errors.New("Shipping & billing: zip ban: " + t.currentProfile.BillingAddress.ZipCode)
				if len(t.availableProfiles) > 1 {
					if err = t.rotateProfile(); err != nil {
						return t.payload.ReportUnexpectedFailure(err)
					}
				}
				return t.payload.ReportUnexpectedError(err)
			}

			if t.isSmsCodeRequired(bodyJson) {
				if err = t.handleSmsCode(t.currentProfile); err != nil {
					t.payload.ReportUnexpectedFailure(err)
					continue
				}

				t.payload.ReportUnexpectedFailure(errors.New("Shipping & billing: Successful"))
				return nil
			} else {
				t.payload.ReportUnexpectedFailure(errors.New("Shipping & billing: failure"))
			}
		} else {
			if strings.Contains(bodyJson, "basketId") {
				t.payload.ReportUnexpectedFailure(errors.New("Shipping & billing: Successful"))
				return nil
			}

			t.payload.ReportUnexpectedFailure(errors.New("Shipping & billing: failed. FW: " + strconv.FormatBool(strings.Contains(bodyJson, "HTTP 403 - Forbidden"))))
		}
	}

	return t.payload.ReportUnexpectedError(errors.New("Shipping & billing: failed to process"))
}

func CreateBillingShippingJson(profile *contracts.ProfileData) string {
	billingAddress := profile.BillingAddress
	shippingAddress := getSafeShippingAddress(profile)

	shippingAddressLine2 := ""
	if shippingAddress.Line2 != nil {
		shippingAddressLine2 = *shippingAddress.Line2
	}

	billingAddressLine2 := ""
	if billingAddress.Line2 != nil {
		billingAddressLine2 = *billingAddress.Line2
	}

	billingShippingJsonStr := "{\"customer\":{\"email\":\"" + profile.Email + "\",\"receiveSmsUpdates\":false},\"shippingAddress\":{\"country\":\"US\"," +
		"\"firstName\":\"" + strings.Title(profile.FirstName) + "\",\"lastName\":\"" + strings.Title(profile.LastName) + "\",\"address1\":\"" + shippingAddress.Line1 +
		"\",\"address2\":\"" + shippingAddressLine2 + "\",\"city\":\"" + shippingAddress.City + "\",\"stateCode\":\"" + *shippingAddress.ProvinceCode +
		"\",\"zipcode\":\"" + shippingAddress.ZipCode + "\",\"phoneNumber\":\"" + *profile.PhoneNumber + "\"}," +
		"\"billingAddress\":{\"country\":\"US\",\"firstName\":\"" + strings.Title(profile.FirstName) + "\",\"lastName\":\"" + strings.Title(profile.LastName) +
		"\",\"address1\":\"" + billingAddress.Line1 + "\",\"address2\":\"" + billingAddressLine2 +
		"\",\"city\":\"" + billingAddress.City + "\",\"stateCode\":\"" + *billingAddress.ProvinceCode + "\",\"zipcode\":\"" + billingAddress.ZipCode +
		"\",\"phoneNumber\":\"" + *profile.PhoneNumber + "\"},\"methodList\":[{\"id\":\"2ndDay-1\",\"shipmentId\":\"me\"," +
		"\"collectionPeriod\":\"\",\"deliveryPeriod\":\"\"}],\"newsletterSubscription\":true}"
	return billingShippingJsonStr
}

func CreateBillingShippingJsonAlter(profile *contracts.ProfileData) (string, error) {
	billingAddress := profile.BillingAddress
	shippingAddress := getSafeShippingAddress(profile)

	shippingAddressLine2 := ""
	if shippingAddress.Line2 != nil {
		shippingAddressLine2 = *shippingAddress.Line2
	}

	billingAddressLine2 := ""
	if billingAddress.Line2 != nil {
		billingAddressLine2 = *billingAddress.Line2
	}

	payload := &ShippingBillingYSPayload{
		Customer: &CustomerYSPayload{
			Email:             profile.Email,
			ReceiveSmsUpdates: false,
		},
		ShippingAddress: &AddressYSPayload{
			Country:     "US",
			FirstName:   strings.Title(profile.FirstName),
			LastName:    strings.Title(profile.LastName),
			Address1:    shippingAddress.Line1,
			Address2:    shippingAddressLine2,
			City:        shippingAddress.City,
			StateCode:   *shippingAddress.ProvinceCode,
			Zipcode:     shippingAddress.ZipCode,
			PhoneNumber: *profile.PhoneNumber,
		},
		BillingAddress: &AddressYSPayload{
			Country:     "US",
			FirstName:   strings.Title(profile.FirstName),
			LastName:    strings.Title(profile.LastName),
			Address1:    billingAddress.Line1,
			Address2:    billingAddressLine2,
			City:        billingAddress.City,
			StateCode:   *billingAddress.ProvinceCode,
			Zipcode:     billingAddress.ZipCode,
			PhoneNumber: *profile.PhoneNumber,
		},
		MethodList: []*BillingMethodYSPayload{
			{
				Id:               "2ndDay-1",
				ShipmentId:       "me",
				CollectionPeriod: "",
				DeliveryPeriod:   "",
			},
		},
		NewsletterSubscription: false,
	}

	return jsoniter.MarshalToString(payload)
}

type BillingMethodYSPayload struct {
	Id               string `json:"id"`
	ShipmentId       string `json:"shipmentId"`
	CollectionPeriod string `json:"collectionPeriod"`
	DeliveryPeriod   string `json:"deliveryPeriod"`
}
type AddressYSPayload struct {
	Country     string `json:"country"`
	FirstName   string `json:"firstName"`
	LastName    string `json:"lastName"`
	Address1    string `json:"address1"`
	Address2    string `json:"address2,omitempty"`
	City        string `json:"city"`
	StateCode   string `json:"stateCode"`
	Zipcode     string `json:"zipcode"`
	PhoneNumber string `json:"phoneNumber"`
}

type CustomerYSPayload struct {
	Email             string `json:"email"`
	ReceiveSmsUpdates bool   `json:"receiveSmsUpdates"`
}

type ShippingBillingYSPayload struct {
	Customer               *CustomerYSPayload        `json:"customer"`
	ShippingAddress        *AddressYSPayload         `json:"shippingAddress"`
	BillingAddress         *AddressYSPayload         `json:"billingAddress"`
	MethodList             []*BillingMethodYSPayload `json:"methodList"`
	NewsletterSubscription bool                      `json:"newsletterSubscription"`
}

func isProfileValid(profile *contracts.ProfileData) bool {
	if !profile.BillingAsShipping && profile.ShippingAddress == nil {
		return false
	}

	return profile.PhoneNumber != nil &&
		profile.BillingAddress.ProvinceCode != nil &&
		getSafeShippingAddress(profile).ProvinceCode != nil &&
		profile.Billing != nil &&
		profile.Billing.HolderName != nil
}

func getSafeShippingAddress(profile *contracts.ProfileData) *contracts.AddressData {
	if profile.BillingAsShipping {
		return profile.BillingAddress
	}

	return profile.ShippingAddress
}

func (t *yeezySupplyTask) processPayment(checkoutAuthorization, basketId string, profile *contracts.ProfileData) *core.StepExecutionFailure {
	for attempt := 0; attempt < 250 && t.payload.Context.Err() == nil; attempt++ {
		if attempt != 0 {
			time.Sleep(t.retryDelay)
		}

		t.payload.ReportCheckingOut("Processing Payment", contracts.TaskCategory_TASK_CATEGORY_CARTED)

		t.utag.documentUrl = YeezySupplyWWWDomainUrl + "/payment"
		t.cookies.SetCookies(baseDomainUrl, []*http.Cookie{
			{Name: "utag_main", Value: t.utag.GetMain()},
		})

		cardType, err := DetectCartType(profile.Billing.CardNumber)
		if err != nil {
			return t.payload.ReportUnexpectedError(err)
		}

		expirationMonth := strconv.Itoa(int(profile.Billing.ExpirationMonth))
		//cryptoResult, err := t.cryptoClient.Encrypt(t.payload.Context, profile)
		cryptoResult, err := Encrypt(profile)
		if err != nil {
			return t.payload.ReportUnexpectedError(err)
		}

		// todo: title case for card holder
		holderName := TitleCaseHolderName(*profile.Billing.HolderName)
		// "DpqwU4zEdN0050000000000000CEENawQ48p0055821336cVB94iKzBGhyJQDr2zvIBix7RX3az8002w7hYyfhAaY00000qZkTE00000wr58nMb7jDEC4FlSABmQ:40"
		rawJsonPayload := "{\"basketId\":\"" + basketId + "\",\"encryptedInstrument\":\"" + cryptoResult.Encrypted +
			"\",\"paymentInstrument\":{\"holder\":\"" + holderName + "\",\"expirationMonth\":" + expirationMonth +
			",\"expirationYear\":" + strconv.Itoa(int(profile.Billing.ExpirationYear)) + ",\"lastFour\":\"" + profile.Billing.CardNumber[len(profile.Billing.CardNumber)-4:] +
			"\",\"paymentMethodId\":\"CREDIT_CARD\",\"cardType\":\"" + cardType +
			"\"},\"fingerprint\":\"ryEGX8eZpJ0030000000000000bsx09CX6tD0119707416cVB94iKzBGa0zW3qWYPi5S16Goh5Mk004NMWer10CgS00000YVxEr00000F2e2nkLQB0DGiMVSwn2S:40\"}"

		req, err := t.createProcessPaymentReq(checkoutAuthorization, rawJsonPayload)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		t.logger.WithField("payload", rawJsonPayload).Infoln("PaymentRequest")
		resp, err := t.http.Do(req)
		if err != nil {
			t.logger.Errorln(err)
			t.payload.ReportUnexpectedFailure(errors.New("Purchase error on submit"))
			continue
		}

		b, err := ioutil.ReadAll(resp.Body)
		respBody := string(b)
		//fmt.Println(respBody)
		//fmt.Println(rawJsonPayload)
		t.logger.WithField("response", respBody).Infoln("PaymentResponse")
		if resp.StatusCode != http.StatusCreated {
			t.processPaymentFailure(respBody, resp)
		} else {
			orderToken := jsoniter.Get(b, "orderToken").ToString()
			paymentStatus := jsoniter.Get(b, "paymentStatus").ToString()
			authorizationType := jsoniter.Get(b, "authorizationType").ToString()

			if len(orderToken) > 0 || !strings.EqualFold(paymentStatus, "not_paid") || !strings.EqualFold(authorizationType, "3ds") {
				t.step++

				t.payload.ProgressConsumer <- taskStatus_CheckedOut
				return nil
			}

			checkoutResp := new(Checkout3DSResponse)
			err := jsoniter.Unmarshal(b, checkoutResp)
			if err != nil {
				return t.payload.ReportUnexpectedError(err)
			}

			termUrl := checkoutResp.BuildTermUrl(t.basketId)
			fields := map[string]string{
				"PaReq":   checkoutResp.PaRedirectForm.FormFields.PaReq,
				"MD":      checkoutResp.PaRedirectForm.FormFields.MD,
				"TermUrl": termUrl,
			}

			t.payload.ReportCheckingOut("3DS2 Solving", contracts.TaskCategory_TASK_CATEGORY_CARTED)
			replay3dsFields, err := t.rpcManager.Solve3DS2(t.payload.UserId, t.userAgent,
				checkoutResp.PaRedirectForm.FormMethod, checkoutResp.PaRedirectForm.FormAction, checkoutResp.PaRedirectForm.FormFields.EncodedData,
				termUrl, t.http.GetUsedProxy().String(), fields)
			if err != nil {
				t.logger.Errorln(err)
				return t.payload.ReportUnexpectedError(errors.New("3DS2: Failure"))
			}

			replay3dsFields["orderId"] = checkoutResp.OrderId
			json, err := jsoniter.MarshalToString(replay3dsFields)
			t.logger.WithField("request", json).Infoln("3DS2ConfirmRequest")
			if err != nil {
				t.logger.Errorln(err)
				return t.payload.ReportUnexpectedError(errors.New("3DS2: Unexpected failure"))
			}

			confirmReq, err := t.createConfirm3dsRequest(checkoutResp.PaRedirectForm.FormFields.EncodedData, t.checkoutAuthorization, json, termUrl)
			if err != nil {
				t.logger.Errorln(err)
				return t.payload.ReportUnexpectedError(errors.New("3DS2: Unexpected failure"))
			}

			confResp, err := t.http.Do(confirmReq)
			if err != nil {
				t.logger.Errorln(err)
				return t.payload.ReportUnexpectedError(errors.New("3DS2: Unexpected failure"))
			}

			b, err := ioutil.ReadAll(confResp.Body)
			if err != nil {
				t.logger.Errorln(err)
				return t.payload.ReportUnexpectedError(errors.New("3DS2: Unexpected failure"))
			}

			confRespBody := string(b)
			t.logger.WithField("response", confRespBody).Infoln("3DS2ConfirmResponse")
			if strings.Contains(confRespBody, "orderToken") {
				t.step++

				t.payload.ProgressConsumer <- taskStatus_CheckedOut
				return nil
			}

			r := t.processPaymentFailure(confRespBody, confResp)
			/*r.HasFatalError = true*/ // todo: should we stop here?
			return r
		}

		time.Sleep(t.retryDelay)
		if r := t.executePrepareCart(false); r != nil {
			return r
		}

		_ = t.postSensors(contracts.TaskCategory_TASK_CATEGORY_CARTED)
	}

	return t.payload.ReportUnexpectedError(errors.New("Failed to process payment"))
}

func TitleCaseHolderName(sourceHolderName string) string {
	tokens := strings.Split(sourceHolderName, " ")
	for ix := range tokens {
		tokens[ix] = strings.ToUpper(string(tokens[ix][0])) + tokens[ix][1:]
	}

	return strings.Join(tokens, " ")
}

func (t *yeezySupplyTask) processPaymentFailure(respBody string, resp *http.Response) *core.StepExecutionFailure {
	if len(respBody) == 0 {
		return t.payload.ReportUnexpectedFailure(errors.New("Payment: Fw block"))
	} else if strings.Contains(respBody, "HTTP 403 - Forbidden") {
		return t.payload.ReportUnexpectedFailure(errors.New("Payment: Blocked by Forbidden resp"))
	} else if len(strings.Replace(respBody, "\n", "", -1)) == 0 {
		return t.payload.ReportUnexpectedFailure(errors.New("Payment: Blocked by FW"))
	} else if resp.StatusCode == http.StatusForbidden {
		return t.payload.ReportUnexpectedFailure(errors.New("Payment: Bad cookie"))
	} else if !strings.Contains(respBody, "confirm.error.paymentdeclined.fraud") && !strings.Contains(respBody, "hook_status_exception") {
		if strings.Contains(respBody, "basket_not_found_exception") || strings.Contains(respBody, "{\"invalidFields\":[\"Product items\"]") {
			t.step -= 2 // NOTICE: move to atc

			if strings.Contains(respBody, "basket_not_found_exception") {
				return t.payload.ReportUnexpectedFailure(errors.New("Payment: Expired Cart"))
			} else {
				return t.payload.ReportUnexpectedFailure(errors.New("Payment: Cart Jacked"))
			}
		} else if strings.Contains(respBody, "confirm.error.paymentdeclined.not_enough_balance") {
			return t.payload.ReportUnexpectedFailure(errors.New("Payment: Card Decline (balance)"))
		} else if strings.Contains(respBody, "paymentdeclined") {
			return t.payload.ReportUnexpectedFailure(errors.New("Payment: Card Decline"))
		} else if strings.Contains(respBody, "missing properties") {
			return t.payload.ReportUnexpectedFailure(errors.New("Payment: Invalid Shipping/Billing"))
		} else if strings.Contains(respBody, "<H1>Invalid URL</H1>") {
			return t.payload.ReportUnexpectedFailure(errors.New("Payment: Session Expired"))
		} else if resp.StatusCode == http.StatusOK && len(resp.Header.Get("authorization")) == 0 {
			return t.payload.ReportUnexpectedFailure(errors.New("Payment: Fake pass"))
		} else if !strings.Contains(respBody, "Product item not available") && !strings.Contains(respBody, "Basket has been removed") {
			// todo: parse response and read message
			return t.payload.ReportUnexpectedFailure(errors.New("Payment: Unknown Error"))
		} else if resp.StatusCode >= http.StatusInternalServerError {
			return t.payload.ReportUnexpectedFailure(errors.New("Payment: YS Site Down"))
		} else if strings.Contains(respBody, "invalidFields\":[\"Product items\"") {
			return t.payload.ReportUnexpectedFailure(errors.New("Payment: OOS"))
		} else if strings.Contains(respBody, "<H1>Invalid URL</H1>") {
			return t.payload.ReportUnexpectedFailure(errors.New("Payment: Blocked Cookies (URL)"))
		} else {
			return t.payload.ReportUnexpectedFailure(errors.New("Payment: Unknown Decline Error"))
		}
	} else {
		return t.payload.ReportUnexpectedFailure(errors.New("Payment: Fraud Decline"))
	}
}

type Checkout3DSResponse struct {
	OrderId           string `json:"orderId"`
	OrderToken        string `json:"orderToken"`
	ResourceState     string `json:"resourceState"`
	PaymentStatus     string `json:"paymentStatus"`
	Status            string `json:"status"`
	AuthorizationType string `json:"authorizationType"`
	PaRedirectForm    struct {
		FormMethod string `json:"formMethod"`
		FormAction string `json:"formAction"`
		FormFields struct {
			PaReq       string `json:"PaReq"`
			EncodedData string `json:"EncodedData"`
			MD          string `json:"MD"`
		} `json:"formFields"`
	} `json:"paRedirectForm"`
}

func (r *Checkout3DSResponse) BuildTermUrl(basketId string) string {
	return "https://www.yeezysupply.com/payment/callback/CREDIT_CARD/" + basketId + "/adyen?orderId=" + r.OrderId +
		"&encodedData=" + r.PaRedirectForm.FormFields.EncodedData + "&result=AUTHORISED"
}

/*func (t *yeezySupplyTask) solve3DS(checkoutResp *Checkout3DSResponse) (string, *core.StepExecutionFailure) {
	httpClient := services.NewHttpClient(t.http.GetTlsIdName(), t.http.GetUsedProxy())
	postUrl := checkoutResp.PaRedirectForm.FormAction
	paRes, err := t.postSecureCode(postUrl, checkoutResp, httpClient)
	if err != nil {
		return "", t.payload.ReportUnexpectedError(err)
	}

	if err = t.postEncryptedPayment(paRes, checkoutResp, httpClient); err != nil {
		return "", t.payload.ReportUnexpectedError(err)
	}

	orderId, r := t.verifyPayment(paRes, checkoutResp, httpClient)
	if r != nil {
		return "", r
	}

	return orderId, nil
}*/

//func (t *yeezySupplyTask) postSecureCode(postUrl string, checkout3ds *Checkout3DSResponse, httpClient services.HttpClient) (string, error) {
//	t.payload.ReportInProgress("Initiating Encrypted Checkout")
//	var paRes string
//
//	form := url.Values{
//		"PaReq":   {checkout3ds.PaRedirectForm.FormFields.PaReq},
//		"MD":      {checkout3ds.PaRedirectForm.FormFields.MD},
//		"TermUrl": {checkout3ds.BuildTermUrl(t.basketId)},
//	}
//
//	req, err := http.NewRequest("POST", postUrl, strings.NewReader(form.Encode()))
//	if err != nil {
//		return paRes, err
//	}
//
//	req.Header.Set("Pragma", "no-cache")
//	req.Header.Set("Cache-Control", "no-cache")
//	//req.Header.Set("sec-ch-ua", t.SecChUa)
//	//req.Header.Set("sec-ch-ua-mobile", "?0")
//	req.Header.Set("Upgrade-Insecure-Requests", "1")
//	req.Header.Set("Origin", "https://www.yeezysupply.com")
//	req.Header.Set("User-Agent", t.userAgent)
//	req.Header.Set("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9")
//	req.Header.Set("Sec-Fetch-Site", "cross-site")
//	req.Header.Set("Sec-Fetch-Mode", "navigate")
//	req.Header.Set("Sec-Fetch-User", "?1")
//	req.Header.Set("Sec-Fetch-Dest", "document")
//	req.Header.Set("Referer", "https://www.yeezysupply.com/")
//	req.Header.Set("Accept-Language", "en-US,en;q=0.9")
//	req.Header.Set("dnt", "1")
//
//	res, err := httpClient.Do(req)
//	if err != nil {
//		return paRes, err
//	}
//
//	defer res.Body.Close()
//
//	body, err := ioutil.ReadAll(res.Body)
//	if err != nil {
//		return paRes, err
//	}
//
//	paRes, err = t.extractPaRes(body)
//	return paRes, err
//}

func (t *yeezySupplyTask) extractPaRes(byteResponse []byte) (string, error) {
	var paRes string
	bodyHtml := string(byteResponse)
	doc := soup.HTMLParse(bodyHtml)

	inputs := doc.Find("input")
	println(inputs.Text())
	paResElem := doc.Find("input", "name", "PaRes")
	if paResElem.Pointer == nil {
		return paRes, errors.New("Failed to extract paRes")
	}

	return paResElem.Attrs()["value"], nil
}

/*func (t *yeezySupplyTask) postEncryptedPayment(paRes string, checkout3ds *Checkout3DSResponse, httpClient services.HttpClient) error {
	t.payload.ReportInProgress("Posting Encrypted Payment")
	form := url.Values{
		"PaReq":              {checkout3ds.PaRedirectForm.FormFields.PaReq},
		"EncodedData":        {checkout3ds.PaRedirectForm.FormFields.EncodedData},
		"MD":                 {checkout3ds.PaRedirectForm.FormFields.MD},
		"orderId":            {checkout3ds.OrderId},
		"PaRes":              {paRes},
		"ABSlog":             {"GPP"},
		"deviceDNA":          {""},
		"executionTime":      {""},
		"dnaError":           {""},
		"mesc":               {""},
		"mescIterationCount": {"0"},
		"desc":               {""},
		"isDNADone":          {"false"},
		"arcotFlashCookie":   {""},
	}

	termUrl := checkout3ds.BuildTermUrl(t.basketId)
	req, err := http.NewRequest("POST", termUrl, strings.NewReader(form.Encode()))
	if err != nil {
		return err
	}

	//t.PostEncryptedPaymentHeaders(req)

	resp, err := httpClient.Do(req)
	if err != nil {
		return err
	}

	if resp.StatusCode != http.StatusOK {
		return errors.New("Post Encrypted Payment: Failure")
	}

	return nil
}*/

/*func (t *yeezySupplyTask) verifyPayment(paRes string, checkout3ds *Checkout3DSResponse, httpClient services.HttpClient) (string, *core.StepExecutionFailure) {
	t.payload.ReportInProgress("Verifying Payment")
	//referer := checkout3ds.BuildTermUrl()
	verifyUrl := "https://www.yeezysupply.com/api/checkout/payment-verification/" + checkout3ds.PaRedirectForm.FormFields.EncodedData
	verifyPayload := map[string]string{
		"OrderID": checkout3ds.OrderId,
		"MD":      checkout3ds.PaRedirectForm.FormFields.MD,
		"PaRes":   paRes,
	}

	marshalledPayload, err := jsoniter.Marshal(&verifyPayload)
	if err != nil {
		return "", t.payload.ReportUnexpectedFailure(err)
	}

	req, err := http.NewRequest("POST", verifyUrl, bytes.NewReader(marshalledPayload))
	if err != nil {
		return "", t.payload.ReportUnexpectedFailure(err)
	}

	//t.PostVerifyPaymentHeaders(req, referer)

	resp, err := httpClient.Do(req)
	if err != nil {
		t.logger.Errorln(err)
		return "", t.payload.ReportUnexpectedFailure(errors.New("Failed to verify payment (HTTP)"))
	}

	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return "", t.payload.ReportUnexpectedFailure(err)
	}
	//if err := t.HandleStatus(status, 200, 201, 400); err != nil {
	//	return err
	//}

	bodyStr := string(body)

	if len(bodyStr) == 0 {
		return "", t.payload.ReportUnexpectedFailure(errors.New("Payment: Fw block"))
	}

	if strings.Contains(bodyStr, "confirm.error.paymentdeclined.fraud") {
		//this.handleFailureWebhooks("Fraud", bodyStr);
		//this.logger.warn("Failed to process payment: status: '{}' - '{}'", "Fraud Decline", httpResponse5.statusCode());
		return "", t.payload.ReportUnexpectedError(errors.New("Payment Decline: Fraud"))
	}

	if strings.Contains(bodyStr, "confirm.error.paymentdeclined.fraud") && !strings.Contains(bodyStr, "hook_status_exception") {
		if strings.Contains(bodyStr, "basket_not_found_exception") || strings.Contains(bodyStr, "{\"invalidFields\":[\"Product items\"]") {
			t.step -= 2

			if strings.Contains(bodyStr, "basket_not_found_exception") {
				t.payload.ReportUnexpectedFailure(errors.New("Payment: Expired Cart"))
			} else {
				t.payload.ReportUnexpectedFailure(errors.New("Payment: Cart Jacked"))
			}
		}
	}

	if strings.Contains(bodyStr, "confirm.error.paymentdeclined.not_enough_balance") {
		//this.handleFailureWebhooks("Card Decline (balance)", bodyStr);
		//this.logger.warn("Failed to process payment: status: '{}' - '{}'", "Card Decline (balance)", httpResponse5.statusCode());
		return "", t.payload.ReportUnexpectedError(errors.New("Payment Decline: Not Enough Balance"))
	}

	if strings.Contains(bodyStr, "paymentdeclined") {
		//this.logger.warn("Failed to process payment: status: '{}' - '{}'", "Card Decline", httpResponse5.statusCode());
		//this.handleFailureWebhooks("Card Decline", bodyStr);
		return "", t.payload.ReportUnexpectedError(errors.New("Card Decline"))
	}

	if strings.Contains(bodyStr, "missing properties") {
		//this.logger.warn("Failed to process payment: status: '{}' - '{}'", "Invalid shipping or billing", httpResponse5.statusCode());
		//this.handleFailureWebhooks("Invalid shipping or billing", bodyStr);
		return "", t.payload.ReportUnexpectedError(errors.New("Invalid Shipping/Billing"))
	}

	if strings.Contains(bodyStr, "Invalid payment verification request") && strings.Contains(bodyStr, "InvalidDataException") {
		//this.logger.warn("Failed to process payment: status: '{}' - '{}'", "3DS Error", bodyStr);
		//this.handleFailureWebhooks("3DS Auth Fail", bodyStr);
		return "", t.payload.ReportUnexpectedError(errors.New("3DS Error"))
	}

	if !strings.Contains(bodyStr, "Product item not available") &&
		!strings.Contains(bodyStr, "Basket has been removed") ||
		strings.Contains(bodyStr, "PaymentNotAuthorized") {
		//this.logger.warn("Failed to process payment: status: '{}' - '{}'", "Unknown payment error", bodyStr);
		return "", t.payload.ReportUnexpectedError(errors.New("Unknown Payment Error"))
	}

	orderId := jsoniter.Get(body, "orderId")
	if orderId.LastError() != nil {
		return "", t.payload.ReportUnexpectedError(orderId.LastError())
	}

	return orderId.ToString(), nil
}*/

func (t *yeezySupplyTask) solveCaptcha() *core.StepExecutionFailure {
	if t.captchaToken != nil && !t.captchaToken.IsExpired() {
		return nil
	}

	queuePollSpan, _ := apm.StartSpan(t.payload.Context, "SolveCaptcha", "captcha")
	defer queuePollSpan.End()

	for att := 0; att < 10 && t.payload.Context.Err() == nil; att++ {
		t.payload.ReportInProgress("Solving Captcha")

		t.logger.Debugln("Solving captcha")
		r, err := t.captchaSolverProvider.
			Get(t.harvesterId, t.payload.UserId, t.getProductUrl()).
			Solve(t.payload.Context)

		if err != nil {
			t.logger.Errorln("Solve captcha FATAL")
			return t.payload.ReportUnexpectedFailure(err)
		}

		t.captchaToken = r
		t.logger.Debugln("Solved captcha")
		return nil
	}

	return t.payload.ReportUnexpectedFailure(errors.New("Can't solve captcha"))
}

var (
	VISA          = regexp.MustCompile("^4[0-9]{12}(?:[0-9]{3}){0,2}$")
	MASTERCARD    = regexp2.MustCompile("^(?:5[1-5]|2(?!2([01]|20)|7(2[1-9]|3))[2-7])\\d{14}$", 0)
	AMEX          = regexp.MustCompile("^3[47][0-9]{13}$")
	DinersClub    = regexp.MustCompile("^3(?:0[0-5]\\d|095|6\\d{0,2}|[89]\\d{2})\\d{12,15}$")
	DISCOVER      = regexp.MustCompile("^6(?:011|[45][0-9]{2})[0-9]{12}$")
	JCB           = regexp.MustCompile("^(?:2131|1800|35\\d{3})\\d{11}$")
	ChinaUnionPay = regexp.MustCompile("^62[0-9]{14,17}$")

	cardTypePatterns = map[string]func(s string) bool{
		"VISA": func(s string) bool {
			return VISA.MatchString(s)
		},
		"MASTERCARD": func(s string) bool {
			m, _ := MASTERCARD.MatchString(s)
			return m
		},
		"AMEX": func(s string) bool {
			return AMEX.MatchString(s)
		},
		"DINERS_CLUB": func(s string) bool {
			return DinersClub.MatchString(s)
		},
		"DISCOVER": func(s string) bool {
			return DISCOVER.MatchString(s)
		},
		"JCB": func(s string) bool {
			return JCB.MatchString(s)
		},
		"CHINA_UNION_PAY": func(s string) bool {
			return ChinaUnionPay.MatchString(s)
		},
	}
)

func DetectCartType(cardNumber string) (string, error) {
	for name, predicate := range cardTypePatterns {
		if predicate(cardNumber) {
			return strings.Replace(name, "MASTERCARD", "MASTER", -1), nil
		}
	}

	return "", errors.New("Unknown Card Type")
}

func (t *yeezySupplyTask) hasCookie(key string, host *url.URL) bool {
	cookies := t.cookies.Cookies(host)
	for _, c := range cookies {
		if c.Name == key {
			return true
		}
	}

	return false
}

func (t *yeezySupplyTask) getCookiesStr() string {
	var cookies = t.cookies.Cookies(t.baseDomainUrl())
	b := strings.Builder{}
	for ix, _ := range cookies {
		c := cookies[ix]
		b.WriteString(fmt.Sprintf("%s=%s\n", c.Name, c.Value))
	}

	return b.String()
}

func (t *yeezySupplyTask) sendPixel(akamUrl, pixelId, tval, scriptVal string) *core.StepExecutionFailure {
	//if strings.Contains(t.taskMode, "2") {

	for at := 0; at < 20 && t.payload.Context.Err() == nil; at++ {
		if at == 0 {
			time.Sleep(t.retryDelay)
		}

		pixelData, r := t.pixelApi.SendPixelData(t.payload.Context, pixelId, tval, scriptVal)
		if r != nil {
			t.payload.ReportUnexpectedFailure(r)
			continue
		}

		req := t.createPixelSendRequest(akamUrl, pixelData)
		//req.GetBody = func() (io.ReadCloser, error) {
		//	return ioutil.NopCloser(strings.NewReader(pixelData)), nil
		//}

		resp, err := t.http.Do(req)
		if err != nil {
			t.logger.Errorln(err)
			t.payload.ReportUnexpectedFailure(errors.New("Error on FW interaction"))
			continue
		}

		if resp.StatusCode != http.StatusOK {
			t.payload.ReportUnexpectedFailure(errors.New("Failed to send pixel"))
			continue
		}

		//cookies := resp.Header.Values("set-cookie")
		//t.logger.Println(cookies)
		return nil
	}
	return t.payload.ReportUnexpectedFailure(errors.New("Failed to send pixel"))
	/*} else {
		panic("Not supported")
	}*/
}

func (t *yeezySupplyTask) handlePOW(json string) *core.StepExecutionFailure {
	for t.payload.Context.Err() == nil {
		matches := powPagePattern.FindAllStringSubmatch(json, -1)
		powUrl := "https://www.yeezysupply.com" + matches[0][0]
		powRequest := t.createPOWRequest(powUrl)
		_, err := t.http.Do(powRequest)
		if err != nil {
			continue
		}

		initSecCpt, err := t.getCookieValue("sec_cpt")
		for secCpt := initSecCpt; err != nil && initSecCpt == secCpt; secCpt, err = t.getCookieValue("sec_cpt") {
			t.sendSensorData(false)
			time.Sleep(time.Millisecond * 2000)
		}

		verifyReq := t.createPOWVerifyPageRequest()
		verifyResp, err := t.http.Do(verifyReq)
		if err != nil {
			continue
		}

		if verifyResp.StatusCode == http.StatusOK {
			return nil
		}
	}

	return t.payload.ReportUnexpectedFailure(errors.New("POW failed"))
}

func (t *yeezySupplyTask) isSmsCodeRequired(shippingResponseJson string) bool {
	type shippingResponse struct {
		MetaShared bool `json:"metashared"`
	}

	r := new(shippingResponse)
	err := jsoniter.UnmarshalFromString(shippingResponseJson, r)
	if err != nil {
		t.logger.Warnln("failed to deserialize shipping response json: " + shippingResponseJson)
		return false
	}

	return r.MetaShared
}

var couponResponseRegexp = regexp2.MustCompile("\\\"valid\\\":\\s*true", regexp2.Compiled|regexp2.IgnoreCase|regexp2.Multiline)

const couponSendAttemptsLimit = 20

func (t *yeezySupplyTask) handleSmsCode(profile *contracts.ProfileData) error {
	couponSendAtt := 0
	var smsCode *string
	for att := 0; att < 1000; att++ {
		couponSendAtt++
		if att != 0 {
			time.Sleep(t.retryDelay)
		}

		if smsCode == nil {
			var err error
			smsCode, err = t.promptUserSmsCode(profile, strconv.FormatUint(t.displayTaskId, 10))
			if err != nil {
				t.payload.ReportUnexpectedFailure(err)
				continue
			}
		}

		form := t.createCouponForm(*smsCode)
		couponReq, err := t.createSubmitCouponRequest(t.basketId, t.checkoutAuthorization, form)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		t.utag.documentUrl = YeezySupplyWWWDomainUrl + "/payment"
		t.cookies.SetCookies(baseDomainUrl, []*http.Cookie{
			{Name: "utag_main", Value: t.utag.GetMain()},
		})

		resp, err := t.http.Do(couponReq)
		if err != nil {
			t.payload.ReportUnexpectedFailure(errors.New("Sms Code: Failed to send"))
			continue
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			t.payload.ReportUnexpectedFailure(err)
			continue
		}

		json := string(b)
		if resp.StatusCode == http.StatusOK {
			if len(resp.Header.Values("authorization")) > 0 {
				isValid, err := couponResponseRegexp.MatchString(json)
				if isValid && err == nil && (strings.Contains(json, "basketId")) {
					return nil
				}

				if strings.Contains(json, "invalid_coupon_code_exception") {
					t.payload.ReportUnexpectedFailure(errors.New("Sms Code: Invalid code"))
					if couponSendAtt > couponSendAttemptsLimit {
						smsCode = nil
						couponSendAtt = 0
						continue
					}
				} else {
					t.payload.ReportUnexpectedFailure(errors.New("Sms Code: FW block"))
				}
			}
		} else {
			t.payload.ReportUnexpectedFailure(errors.New("Sms Code: invalid: " + strconv.Itoa(resp.StatusCode)))
		}
	}

	return errors.New("Sms Code: failed to process")
}

type smsCodeVerificationPayload struct {
	Code string `json:"couponCode"`
}

func (t *yeezySupplyTask) createCouponForm(smsCode string) string {
	p := &smsCodeVerificationPayload{Code: smsCode}
	json, _ := jsoniter.MarshalToString(p)
	return json
}

func (t *yeezySupplyTask) promptUserSmsCode(profile *contracts.ProfileData, taskDisplayId string) (*string, error) {
	t.payload.ReportCheckingOutUnspecified("Sms Code: task #" + taskDisplayId)
	for t.payload.Context.Err() == nil {
		t.logger.Debugln("Waiting for sms confirmation")
		code, err := t.rpcManager.RequestSmsConfirmationCode(t.payload.UserId, *profile.PhoneNumber, taskDisplayId)
		if err != nil && err == services.RpcTimeoutError {
			t.logger.Debugln("Sms confirmation TIMEOUT")
			continue
		}

		return code, err
	}

	return nil, t.payload.Context.Err()
}

const ccmTagSize = 8
const adyenRawSignKey = "10001|C4F415A1A41A283417FAB7EF8580E077284BCC2B06F8A6C1785E31F5ABFD38A3E80760E0CA6437A8DC95BA4720A83203B99175889FA06FC6BABD4BF10EEEF0D73EF86DD336EBE68642AC15913B2FC24337BDEF52D2F5350224BD59F97C1B944BD03F0C3B4CA2E093A18507C349D68BE8BA54B458DB63D01377048F3E53C757F82B163A99A6A89AD0B969C0F745BB82DA7108B1D6FD74303711065B61009BC8011C27D1D1B5B9FC5378368F24DE03B582FE3490604F5803E805AEEA8B9EF86C54F27D9BD3FC4138B9DC30AF43A58CFF7C6ECEF68029C234BBC0816193DF9BD708D10AAFF6B10E38F0721CF422867C8CC5C554A357A8F51BA18153FB8A83CCBED1"

var adyenSignKey = getAdyenSignKey()

func Encrypt(profile *contracts.ProfileData) (*checkout.EncryptionResult, error) {
	aesKey := make([]byte, 32)
	if _, err := crand.Read(aesKey); err != nil {
		return nil, err
	}

	aesNonce := make([]byte, 12)
	if _, err := crand.Read(aesNonce); err != nil {
		return nil, err
	}

	block, err := aes.NewCipher(aesKey)
	if err != nil {
		return nil, err
	}

	ccm, err := aesccm.NewCCM(block, len(aesNonce), ccmTagSize)
	if err != nil {
		return nil, err
	}

	serializedProfile, err := SerializeProfileForPayment(profile)
	if err != nil {
		return nil, err
	}

	encryptedProfile := ccm.Seal(nil, aesNonce, serializedProfile, nil)
	nonceWithData := append(aesNonce[:], encryptedProfile...)

	encryptedKey, err := rsa.EncryptPKCS1v15(crand.Reader, adyenSignKey, aesKey)

	return &checkout.EncryptionResult{
		Encrypted: fmt.Sprintf("adyenjs_%s$%s$%s",
			"0_1_12",
			base64.StdEncoding.EncodeToString(encryptedKey),
			base64.StdEncoding.EncodeToString(nonceWithData),
		),
	}, nil
}

func SerializeProfileForPayment(profile *contracts.ProfileData) ([]byte, error) {
	json := map[string]interface{}{
		"cvc":         profile.Billing.Cvv,
		"dfValue":     nil,
		"expiryMonth": strconv.FormatUint(uint64(profile.Billing.ExpirationMonth), 10),
		"expiryYear":  strconv.FormatUint(uint64(profile.Billing.ExpirationYear), 10),
		//"dfValue": "",
		//"expiryMonth": profile.Billing.ExpirationMonth,
		//"expiryYear": profile.Billing.ExpirationYear,
		"generationtime":      time.Now().UTC().Format("2006-01-02T15:04:05.000Z07:00"),
		"holderName":          TitleCaseHolderName(*profile.Billing.HolderName),
		"initializeCount":     "1",
		"luhnCount":           "1",
		"luhnOkCount":         "1",
		"luhnSameLengthCount": "1",
		"number":              profile.Billing.CardNumber,
		"sjclStrength":        "10",
	}

	jsonBytes, err := jsoniter.Marshal(json)
	//println(string(jsonBytes))
	if err != nil {
		return nil, err
	}

	return jsonBytes, nil
}

func getAdyenSignKey() *rsa.PublicKey {
	keyParts := strings.Split(adyenRawSignKey, "|")
	exp := keyParts[0]
	mod := keyParts[1]
	parsedExp, err := strconv.ParseInt(exp, 16, 64)
	if err != nil {
		panic(err)
	}

	decodedMod, err := hex.DecodeString(mod)
	if err != nil {
		panic(err)
	}

	rsaKey := new(rsa.PublicKey)
	rsaKey.E = int(parsedExp)
	rsaKey.N = new(big.Int).SetBytes(decodedMod)

	return rsaKey
}
