package amazon

import (
	"bytes"
	"errors"
	"github.com/CenturionLabs/centurion/checkout-service/contracts"
	"github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/config/amazon"
	integContracts "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/integration"
	"github.com/CenturionLabs/centurion/checkout-service/contracts/monitor"
	"github.com/CenturionLabs/centurion/checkout-service/contracts/monitor/integration"
	"github.com/CenturionLabs/centurion/checkout-service/core"
	"github.com/CenturionLabs/centurion/checkout-service/services"
	"github.com/CenturionLabs/centurion/checkout-service/util"
  "github.com/CenturionLabs/centurion/checkout-service/utls_presets"
  "github.com/PuerkitoBio/goquery"
	"github.com/golang/protobuf/proto"
	jsoniter "github.com/json-iterator/go"
	http "github.com/useflyent/fhttp"
	"github.com/useflyent/fhttp/cookiejar"
	"go.elastic.co/apm"
	"io"
	"io/ioutil"
	"math/rand"
	"net/url"
	"strconv"
	"strings"
)

const (
	sessionCookieRegion  = "region"
	sessionCookieLoginIp = "loginIp"
	sessionIdCookieName  = "session-id"
)

type amazonRegion string

const (
	regionUSA amazonRegion = "USA"
)

type supportedModes string

const (
	modeCheckout supportedModes = "checkout"
	modeTurbo    supportedModes = "turbo"
)

func (s supportedModes) String() string {
	return string(s)
}

const (
	TracingSpanName = "checkout"
)

var (
	DefaultUa          = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36"
	DefaultSecUa       = `" Not A;Brand";v="99", "Chromium";v="90", "Google Chrome";v="90"`
	sessionCookieNames = []string{"session-id", "ubid-main", "at-main"}
)

type loadSessionResult struct {
	session        *contracts.SessionData
	cookieJar      *cookiejar.Jar
	checkoutCookie *http.Cookie
	region         amazonRegion
}

type loginValidationResult struct {
	isValid   bool
	addressId string
}

type weblabTokenData struct {
	TurboWeblab string `json:"turboWeblab"`
}

type checkoutTask struct {
	http         services.HttpClient
	inStockItems InStockItemsProvider
	dstrLock     services.DistributedLockFactory
	payload      *core.CheckoutPayload

	useSmile   bool
	purchaseId string
	csrf       *string

	offerId           string
	sendAllCookies    bool
	checkoutSessionId string

	checkoutAttempts  int32
	proxyRotation     services.ProxyRotationPolicy
	siteConfig        *amazon.AmazonConfig
	loadSessionResult *loadSessionResult
	validationResult  *loginValidationResult
	weblab            *weblabTokenData
	cartSessionId     string
	step              int

	monitorClient monitor.MonitorClient

	//checkoutCookieJar *cookiejar.Jar
	//checkoutConfig    *amazon.AmazonCheckoutConfig
}

func (a *checkoutTask) ProcessProductCheckedOutEvent(e *integContracts.ProductCheckedOut) {
	p := a.http.GetUsedProxy()
	if p != nil {
		proxyUrlStr := p.String()
		e.Proxy = &proxyUrlStr
	}
}

func newAmazonTask(httpClient services.HttpClient, inStockItems InStockItemsProvider, ctx *core.CheckoutPayload,
	dstrLock services.DistributedLockFactory, monitorClient monitor.MonitorClient) core.CheckoutTask {
	return &checkoutTask{
		http:          httpClient,
		inStockItems:  inStockItems,
		dstrLock:      dstrLock,
		payload:       ctx,
		monitorClient: monitorClient,
	}
}

func (a *checkoutTask) FetchProduct() (*contracts.ProductData, error) {
  return nil, errors.New("not implemented")
}

func NewInStockItemsProvider() InStockItemsProvider {
	return new(inMemoryInStockItemProvider)
}

type inMemoryInStockItemProvider struct {
}

func (i *inMemoryInStockItemProvider) GetRandomSKU(region amazonRegion) string {
	items := inStockItems[region][:]
	rand.Shuffle(len(items), func(i, j int) {
		items[i], items[j] = items[j], items[i]
	})

	return items[0]
}

func (a *checkoutTask) Step() int {
	return a.step
}

func (a *checkoutTask) GetCheckoutSteps() ([]core.CheckoutStep, error) {
  cfg := new(amazon.AmazonConfig)
  if err := proto.Unmarshal(a.payload.Config, cfg); err != nil {
    return nil, err
  }

  a.siteConfig = cfg

	switch cfg.Mode.(type) {
	case *amazon.AmazonConfig_Checkout:
		initializeCheckoutMode(a, cfg.GetCheckout())
		return []core.CheckoutStep{
      a.decodeDetailsAndSetCheckoutProxy,
      a.loadSession,
      a.checkLoginValid,
      a.fetchWebLab,
      a.placeOrder,
		}, nil
	case *amazon.AmazonConfig_Turbo:
		initializeTurboMode(a, cfg.GetTurbo())
		return []core.CheckoutStep{
      a.decodeDetailsAndSetCheckoutProxy,
      a.loadSession,
      a.checkLoginValid,
      a.fetchWebLab,
      a.ensureInStock,
      a.initTurbo,
      a.placeOrder,
		}, nil
	default:
		return nil, errors.New("Not supported mode")
	}
}

func convertToAmazonSmile(url string) string {
	return strings.ReplaceAll(url, "www", "smile")
}

func initializeCheckoutMode(a *checkoutTask, checkoutConfig *amazon.AmazonCheckoutConfig) *core.StepExecutionFailure {
	span, _ := apm.StartSpan(a.payload.Context, "initializeCheckoutMode", TracingSpanName)
	defer span.End()

	a.purchaseId = checkoutConfig.PurchaseId
	a.csrf = &checkoutConfig.Csrf
	a.useSmile = checkoutConfig.UseSmile

	return nil
}

// todo: think on better approach instead of storing all possible data from all modes in single struct
func initializeTurboMode(a *checkoutTask, config *amazon.AmazonTurboConfig) *core.StepExecutionFailure {
	span, _ := apm.StartSpan(a.payload.Context, "initializeTurboMode", TracingSpanName)
	defer span.End()

	a.offerId = config.OfferId
	a.useSmile = config.UseSmile
	a.sendAllCookies = config.SendAllCookies

	return nil
}

func (a *checkoutTask) GetUsedProfile() *contracts.ProfileData {
  // todo: implement it
  return nil
}

func (a *checkoutTask) decodeDetailsAndSetCheckoutProxy() *core.StepExecutionFailure {
	span, _ := apm.StartSpan(a.payload.Context, "decodeDetailsAndSetCheckoutProxy", TracingSpanName)
	defer span.End()

	// todo: it's required until we add support on frontend for it
	a.siteConfig.Ua = &DefaultUa
	a.siteConfig.SecUa = &DefaultSecUa

	err := a.setRandomProxy()
	if err != nil {
		return a.payload.ReportUnexpectedError(err)
	}

	a.http.ChangeTlsSettings(utls_presets.TlsIdChrome_95)
	a.http.SetFollowRedirects(true)
	a.step++
	return nil
}

func (a *checkoutTask) placeOrder() *core.StepExecutionFailure {
	span, _ := apm.StartSpan(a.payload.Context, "placeOrder", TracingSpanName)
	defer span.End()
	if a.proxyRotation == services.ProxyRotationSwitch {
		err := a.setRandomProxy()
		if err != nil {
			return a.payload.ReportUnexpectedError(err)
		}
	}

	if a.siteConfig.MaxCheckoutAttempts != nil &&
		*a.siteConfig.MaxCheckoutAttempts > 0 &&
		*a.siteConfig.MaxCheckoutAttempts != -1 &&
		*a.siteConfig.MaxCheckoutAttempts == a.checkoutAttempts {
		a.checkoutAttempts = 0
		return a.payload.ReportError(taskStatus_PlaceOrderOOS)
	}

	a.checkoutAttempts++
	a.payload.ProgressConsumer <- taskStatus_PlaceOrderPlacing
	startUrl := getRawBaseUrl(a.loadSessionResult.region)
	if a.loadSessionResult.region == regionUSA && a.useSmile {
		startUrl = convertToAmazonSmile(startUrl)
	}

	query := url.Values{
		"ref_":               {"chk_spc_placeOrder"},
		"referrer":           {"spc"},
		"pid":                {a.purchaseId},
		"pipelineType":       {"turbo"},
		"clientId":           {"retailwebsite"},
		"forcePlaceOrder":    {"Place this duplicate order"},
		"temporaryAddToCart": {"1"},
		"hostPage":           {"detail"},
		"weblab":             {a.weblab.TurboWeblab},
		"isClientTimeBased":  {"1"},
	}

	checkoutUrl, err := url.Parse(startUrl + "/checkout/spc/place-order?" + query.Encode())
	if err != nil {
		return a.payload.ReportUnexpectedError(err)
	}

	headers := http.Header{
		"sec-ch-ua":        {*a.siteConfig.SecUa},
		"sec-ch-ua-mobile": {"?0"},
		"user-agent":       {*a.siteConfig.Ua},
		"accept":           {"*/*"},
		"x-requested-with": {"XMLHttpRequest"},
		"origin":           {startUrl},
		"sec-fetch-site":   {"same-origin"},
		"sec-fetch-mode":   {"cors"},
		"sec-fetch-dest":   {"empty"},
		"referer":          {startUrl + "/checkout/spc?pid=" + a.purchaseId + "&pipelineType=turbo&clientId=retailwebsite&temporaryAddToCart=1&hostPage=detail&weblab=RCX_CHECKOUT_TURBO_DESKTOP_PRIME_87783"},
		"accept-encoding":  {"gzip, deflate, br"},
		"accept-language":  {"en-US,en;q=0.9"},
		"cookie":           {a.loadSessionResult.checkoutCookie.String()},
	}

	if a.loadSessionResult.region != regionUSA {
		a.http.UseCookieJar(a.loadSessionResult.cookieJar)
	}

	req := &http.Request{
		URL:    checkoutUrl,
		Header: headers,
	}
	resp, err := a.http.Do(req)
	if err != nil {
		return a.payload.ReportUnexpectedError(err)
	}

	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return a.payload.ReportUnexpectedError(err)
	}

	bodyStr := string(body)
	if resp.StatusCode != http.StatusOK {
		if strings.Contains(bodyStr, "An error occurred when we tried to process your request.") {
			a.step--
			return a.payload.ReportError(taskStatus_PlaceOrderCheckoutExpired)
		}

		if resp.StatusCode == http.StatusForbidden {
			_ = a.setRandomProxy()
			return a.payload.ReportError(taskStatus_AmazonAntiBot)
		}

		if resp.StatusCode == http.StatusServiceUnavailable {
			_ = a.setRandomProxy()
			return a.payload.ReportError(taskStatus_PotentialAccountClip)
		}

		return a.payload.ReportError(taskStatus_PlaceOrderFailedToPlaceOrder(resp.StatusCode))
	}

	if strings.Contains(bodyStr, "/errors/validateCaptcha") {
		_ = a.setRandomProxy()
		return a.payload.ReportError(taskStatus_CaptchaDetected)
	}

	locationUrl, err := resp.Location()
	if err != nil {
		if strings.Contains(bodyStr, "<meta http-equiv=\"refresh\" content=\"0; url=/gp/cart/view.html\">") {
			a.step--
			return a.payload.ReportError(taskStatus_PlaceOrderCheckoutExpired)
		}

		return a.payload.ReportError(taskStatus_FailedToFetchOrderStatus)
	}

	if !strings.Contains(locationUrl.String(), "thankyou") {
		return a.payload.ReportError(taskStatus_PlaceOrderUnknownError(locationUrl))
	}

	a.payload.ProgressConsumer <- taskStatus_PlaceOrderPlaced
	a.step++

	return nil
}

func (a *checkoutTask) fetchWebLab() *core.StepExecutionFailure {
	span, _ := apm.StartSpan(a.payload.Context, "fetchWebLab", TracingSpanName)
	defer span.End()
	a.payload.ProgressConsumer <- taskStatus_FetchWebLabWaitingForPending
	config := &services.ExecuteOnceAndShareConfig{
		CheckoutPayload: a.payload,
		DstrLock:        a.dstrLock,
	}

	config.MutexNameFactory = func() string {
		return contracts.Module_AMAZON.String() + ":fetchWebLab:" + a.siteConfig.Session.Id
	}

	config.RawValueReceiver = func(token []byte) *core.StepExecutionFailure {
		a.weblab = &weblabTokenData{
			TurboWeblab: string(token),
		}

		a.payload.ProgressConsumer <- taskStatus_FetchWebLabFetched
		a.step++
		return nil
	}

	config.ValueProducer = func(consumer func(rawValue []byte) *core.StepExecutionFailure) *core.StepExecutionFailure {
		a.payload.ProgressConsumer <- taskStatus_FetchWebLabGetToken
		itemSku := a.inStockItems.GetRandomSKU(a.loadSessionResult.region)
		fetchUrl, err := resolveAbsoluteUrl("/gp/product/"+itemSku, a.loadSessionResult.region)
		if err != nil {
			return a.payload.ReportUnexpectedError(err)
		}

		headers := http.Header{
			"rtt":                       {"50"},
			"downlink":                  {"10"},
			"ect":                       {"4g"},
			"sec-ch-ua":                 {*a.siteConfig.SecUa},
			"sec-ch-ua-mobile":          {"?0"},
			"upgrade-insecure-requests": {"1"},
			"user-agent":                {*a.siteConfig.Ua},
			"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
			"sec-fetch-site":            {", same-origin"},
			"sec-fetch-mode":            {"navigate"},
			"sec-fetch-user":            {"?1"},
			"sec-fetch-dest":            {"document"},
			"accept-encoding":           {"gzip, deflate, br"},
			"accept-language":           {"en-US, en; q = 0.9"},
			http.HeaderOrderKey: {
				"rtt",
				"downlink",
				"ect",
				"sec-ch-ua",
				"sec-ch-ua-mobile",
				"upgrade-insecure-requests",
				"user-agent",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-user",
				"sec-fetch-dest",
				"accept-encoding",
				"accept-language",
			},
		}

		util.HeaderShuffle(headers)
		req := &http.Request{
			URL:    fetchUrl,
			Header: headers,
		}

		resp, err := a.http.Do(req)
		if err != nil {
			return a.payload.ReportUnexpectedError(err)
		}

		body, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			return a.payload.ReportUnexpectedError(err)
		}

		bodyStr := string(body)
		if resp.StatusCode == http.StatusCreated ||
			resp.StatusCode == http.StatusServiceUnavailable ||
			strings.Contains(bodyStr, "http-equiv=\"refresh\"") {
			_ = a.setRandomProxy()
			return a.payload.ReportError(taskStatus_AmazonAntiBot)
		}

		if strings.Contains(bodyStr, "/errors/validateCaptcha") {
			_ = a.setRandomProxy()
			return a.payload.ReportError(taskStatus_FetchWebLabCaptchaDetected)
		}

		if resp.StatusCode != http.StatusOK {
			return a.payload.ReportError(taskStatus_FetchWebLabFailedToFetch(resp.StatusCode))
		}

		startTurboWeblabIx := strings.Index(bodyStr, "{\"turboWeblab\"")
		turboInfo := new(weblabTokenData)
		if startTurboWeblabIx > -1 {
			tail := bodyStr[startTurboWeblabIx:]
			scriptCloseTag := "</script>"
			endIx := strings.Index(tail, scriptCloseTag)
			rawTurboInfoJson := tail[:endIx]
			err = jsoniter.Unmarshal([]byte(rawTurboInfoJson), turboInfo)
			if err != nil {
				return a.payload.ReportUnexpectedError(err)
			}
		} else {
			turboInfo.TurboWeblab = "RCX_CHECKOUT_TURBO_MWEB_126825"
		}

		return consumer([]byte(turboInfo.TurboWeblab))
	}

	return config.ExecuteOnceAndShare()
}

func (a *checkoutTask) checkLoginValid() *core.StepExecutionFailure {
	span, _ := apm.StartSpan(a.payload.Context, "checkLoginValid", TracingSpanName)
	defer span.End()
	a.payload.ProgressConsumer <- taskStatus_ValidateAccWaitingForPending
	config := &services.ExecuteOnceAndShareConfig{
		CheckoutPayload: a.payload,
		DstrLock:        a.dstrLock,
	}

	config.MutexNameFactory = func() string {
		return contracts.Module_AMAZON.String() + ":validate_login:" + a.siteConfig.Session.Id
	}

	config.RawValueReceiver = func(addressId []byte) *core.StepExecutionFailure {
		a.payload.ProgressConsumer <- taskStatus_ValidateAccLoginValid
		a.validationResult = &loginValidationResult{
			isValid:   true,
			addressId: string(addressId),
		}

		a.step++
		return nil
	}

	config.ValueProducer = func(consumer func(rawValue []byte) *core.StepExecutionFailure) *core.StepExecutionFailure {
		a.payload.ProgressConsumer <- taskStatus_ValidateAccInProgress
		checkAccountUrl, err := resolveAbsoluteUrl("/cpe/yourpayments/wallet?ref_=ya_d_c_pmt_mpo", a.loadSessionResult.region)
		if err != nil {
			return a.payload.ReportUnexpectedError(err)
		}

		headers := http.Header{
			"rtt":                       {"50"},
			"downlink":                  {"10"},
			"ect":                       {"4g"},
			"sec-ch-ua":                 {*a.siteConfig.SecUa},
			"sec-ch-ua-mobile":          {"?0"},
			"upgrade-insecure-requests": {"1"},
			"user-agent":                {*a.siteConfig.Ua},
			"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
			"sec-fetch-site":            {"same-origin"},
			"sec-fetch-mode":            {"navigate"},
			"sec-fetch-user":            {"?1"},
			"sec-fetch-dest":            {"document"},
			"accept-encoding":           {"gzip, deflate, br"},
			"accept-language":           {"en-US,en;q=0.9"},
			http.HeaderOrderKey: {
				"rtt",
				"downlink",
				"ect",
				"sec-ch-ua",
				"sec-ch-ua-mobile",
				"upgrade-insecure-requests",
				"user-agent",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-user",
				"sec-fetch-dest",
				"accept-encoding",
				"accept-language",
			},
		}

		a.http.UseCookieJar(a.loadSessionResult.cookieJar)
		req := &http.Request{
			URL:    checkAccountUrl,
			Method: "GET",
			Header: headers,
		}

		a.http.SetFollowRedirects(false)
		resp, err := a.http.Do(req)
		if err != nil {
			return a.payload.ReportUnexpectedError(err)
		}

		loc, err := resp.Location()
		if err == nil || loc != nil && strings.Contains(loc.String(), "/ap/signin") {
			return a.payload.ReportError(taskStatus_ValidateAccSessionInvalid)
		}

		bodyBytes, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			return a.payload.ReportUnexpectedError(err)
		}

		body := string(bodyBytes)
		if resp.StatusCode != 200 {
			return a.payload.ReportError(taskStatus_ValidateAccFailedToValidate(resp.StatusCode))
		}

		if strings.Contains(body, "Business Prime") {
			a.http.SetFollowRedirects(true)

			a.payload.ProgressConsumer <- taskStatus_ValidateAccLoginValid
			a.validationResult = &loginValidationResult{
				isValid:   true,
				addressId: "",
			}

			a.step++
			return nil
		}

		doc, err := goquery.NewDocumentFromReader(bytes.NewReader(bodyBytes))
		if err != nil {
			return a.payload.ReportUnexpectedError(err)
		}

		addressId := doc.Find("input[name=\"dropdown-selection\"]").AttrOr("value", "")
		if addressId == "add-new" {
			// setSessionCookies(session, []);
			return a.payload.ReportError(taskStatus_ValidateAccReverifyAccountSettings)
		}

		return consumer([]byte(addressId))
	}

	return config.ExecuteOnceAndShare()
}

func (a *checkoutTask) loadSession() *core.StepExecutionFailure {
	span, _ := apm.StartSpan(a.payload.Context, "loadSession", TracingSpanName)
	defer span.End()
	session := a.siteConfig.Session
	if session == nil {
		return a.payload.ReportError(taskStatus_SessionNotSelected)
	}

	if session.Status == contracts.SessionStatus_NOT_READY {
		return a.payload.ReportError(taskStatus_SessionIsNotReady)
	}

	region := amazonRegion(session.Extra[sessionCookieRegion])
	if len(region) == 0 {
		return a.payload.ReportError(taskStatus_FailedSessionLoad)
	}

	if len(session.Cookies) == 0 {
		return a.payload.ReportError(taskStatus_SessionHasNoCookies)
	}

	cookieJar, err := cookiejar.New(nil)
	if err != nil {
		return a.payload.ReportUnexpectedError(err)
	}

	parsedCookies := util.ParseCookies(session.Cookies)
	cookiesToSet := []*http.Cookie{}
	var checkoutCookie *http.Cookie
	for ix := range parsedCookies {
		cookie := parsedCookies[ix]

		if util.StrSliceContains(cookie.Name, sessionCookieNames) {
			if strings.EqualFold(sessionIdCookieName, cookie.Name) {
				a.cartSessionId = cookie.Value
			}

			checkoutCookie = &http.Cookie{Name: cookie.Name, Value: cookie.Value}
		}

		if !strings.Contains(cookie.Raw, "apn-privacy") &&
			!strings.Contains(cookie.Raw, "amzn-ssnap-ctxt") &&
			!strings.Contains(cookie.Raw, "amzn-app-id") &&
			!strings.Contains(cookie.Raw, "mobile-device-info") {
			cookiesToSet = append(cookiesToSet, cookie)
		}
	}

	baseUrl := getBaseUrl(region)
	cookieJar.SetCookies(baseUrl, cookiesToSet)
	a.payload.ProgressConsumer <- taskStatus_SessionLoaded

	a.loadSessionResult = &loadSessionResult{
		session:        session,
		cookieJar:      cookieJar,
		checkoutCookie: checkoutCookie,
		region:         region,
	}

	a.step++

	return nil
}

func (a *checkoutTask) setRandomProxy() error {
	span, _ := apm.StartSpan(a.payload.Context, "setRandomProxy", TracingSpanName)
	defer span.End()
	if a.payload.ProxyPool == nil {
		return nil
	}

	proxyList, err := util.GetProxyUrls(a.payload.ProxyPool)
	if err != nil {
		return err
	}

	rndProxy, err := util.RandomUrl(proxyList)
	if err != nil {
		return err
	}

	a.http.ChangeProxy(rndProxy)
	return nil
}

type CartInfo struct {
	IsOk      bool          `json:"isOK"`
	Exception CartException `json:"exception"`
	Items     []CartItem    `json:"items"`
}

type CartException struct {
	Reason string `json:"reason"`
	Code   string `json:"code"`
}
type CartItem struct {
}

func (a *checkoutTask) ensureInStock() *core.StepExecutionFailure {
	span, _ := apm.StartSpan(a.payload.Context, "ensureInStock", TracingSpanName)
	defer span.End()
	task := a.payload
	config := &services.ExecuteOnceAndShareConfig{
		CheckoutPayload: a.payload,
		DstrLock:        a.dstrLock,
	}

	config.MutexNameFactory = func() string {
		return task.Module.String() + "___" + task.Product.Sku
	}

	config.RawValueReceiver = func(rawValue []byte) *core.StepExecutionFailure {
		if strings.EqualFold(string(rawValue), taskStatus_MonitorInStock.Title) {
			a.payload.ProgressConsumer <- taskStatus_MonitorInStock
			a.step++
		} else {
			a.payload.ReportError(taskStatus_UnknownErrorMonitor)
		}

		return nil
	}

	config.ValueProducer = func(consumer func(rawValue []byte) *core.StepExecutionFailure) *core.StepExecutionFailure {
		a.payload.ProgressConsumer <- taskStatus_Monitoring
		var sessionCookies strings.Builder
		for _, s := range a.loadSessionResult.session.Cookies {
			sessionCookies.WriteString(s)
			sessionCookies.WriteString("; ")
		}
		//
		//watchCmd := &monitor.WatchCommand{
		//	Product:        task.Product,
		//	UserId:         task.UserId,
		//	ProxyPool:      task.ProxyPool,
		//	PreferredProxy: task.PreferredProxy,
		//	Site:           task.Site,
		//	CorrelationId:  task.CorrelationId,
		//	SiteConfig:     task.SiteConfig,
		//	MonitorConfig:  task.MonitorConfig,
		//	Extra: map[string]string{
		//		"cookie": sessionCookies.String(),
		//	},
		//}
		//
		//for {
		//	if a.payload.Context.Err() != nil {
		//		return a.payload.ReportUnexpectedError(a.payload.Context.Err())
		//	}
		//
		//	stream, err := a.monitorClient.WatchStreamed(a.payload.Context, watchCmd)
		//	if err != nil {
		//		return a.payload.ReportUnexpectedError(err)
		//	}
		//
		//	entry, err := a.receiveMonitoringResult(stream)
		//
		//	if err != io.EOF && err != nil {
		//		return a.payload.ReportUnexpectedError(err)
		//	}
		//
		//	if entry != nil && core.IsTaskCompleted(entry.Status) {
		//		a.step++
		//		return nil
		//	}
		//}

		headers := http.Header{
			/*	":authority":                {"www.amazon.com"},
				":method":                   {"GET"},
				":path":                     {"/cpe/yourpayments/wallet?ref_=ya_d_c_pmt_mpo"},
				":scheme":                   {"https"},*/
			"accept":                    {`text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9`},
			"accept-encoding":           {"gzip, deflate, br"},
			"accept-language":           {`en-US,en;q=0.9,uk;q=0.8,ru;q=0.7,fr;q=0.6`},
			"cache-control":             {`no-cache`},
			"cookie":                    {sessionCookies.String()},
			"downlink":                  {"10"},
			"ect":                       {"4g"},
			"pragma":                    {"no-cache"},
			"rtt":                       {"50"},
			"sec-ch-ua":                 {*a.siteConfig.SecUa},
			"sec-ch-ua-mobile":          {"?0"},
			"sec-fetch-dest":            {"document"},
			"sec-fetch-mode":            {"navigate"},
			"sec-fetch-site":            {"none"},
			"sec-fetch-user":            {"?1"},
			"upgrade-insecure-requests": {"1"},
			"content-type":              {"application/x-www-form-urlencoded"},
			"User-Agent":                {*a.siteConfig.Ua},

			http.HeaderOrderKey: {
				"accept",
				"accept-encoding",
				"accept-language",
				"cache-control",
				"cookie",
				"downlink",
				"ect",
				"pragma",
				"rtt",
				"sec-ch-ua",
				"sec-ch-ua-mobile",
				"sec-fetch-dest",
				"sec-fetch-mode",
				"sec-fetch-site",
				"sec-fetch-user",
				"upgrade-insecure-requests",
				"user-agent",
			},
			http.PHeaderOrderKey: {
				":authority",
				":method",
				":path",
				":scheme",
			},
			/*http.HeaderOrderKey: {
			  	"accept",
			  	"accept-encoding",
			  	"accept-language",
			  	"cache-control",
			  	"cookie",
			  	"pragma",
			  	"sec-ch-ua",
			  	"sec-ch-ua-mobile",
			  	"sec-fetch-dest",
			  	"sec-fetch-mode",
			  	"sec-fetch-site",
			  	"sec-fetch-user",
			  	"upgrade-insecure-requests",
			  	"user-agent",
			  },
			  http.PHeaderOrderKey: {
			  	":authority",
			  	":method",
			  	":path",
			  	":scheme",
			  },*/
		}

		targetUrl, err := url.Parse("https://www.amazon.com/gp/add-to-cart/json")
		if err != nil {
			return a.payload.ReportUnexpectedError(err)
		}

		payload := url.Values{
			"session-id":     {a.cartSessionId},
			"clientName":     {"retailwebsite"},
			"nextPage":       {"cartitems"},
			"ASIN":           {a.payload.Product.Sku},
			"offerListingID": {a.offerId},
			"quantity":       {"1"},
		}

		req := &http.Request{
			Method: "POST",
			URL:    targetUrl,
			Host:   targetUrl.Hostname(),
			Body:   ioutil.NopCloser(strings.NewReader(payload.Encode())),
			Header: headers,
		}

		//proxyUrl, _ := url.Parse("http://PgRSaDfIZv:xnQGbvAV7x@46.19.108.216:6236")
		//proxyUrl, _ := url.Parse("http://localhost:8888")
		//var rawCookies = map[string]string{
		//	`session-id`:      `133-2239167-9994034`,
		//	`ubid-main`:       `131-4633398-1159863`,
		//	`session-token`:   `"sfKluPjCjzb5s4UdMVGEGxS+oaKBYTKYfq2s4/XHhvNft45Eq8iLjiW+d5DXEPvjqehn8dXPMFmi2APc4MudpsuajbTA4pEhW4HHN2kIoCR5WghQ6cdvKG4EX2zOzOeg1Yrisin9pSL/RnTqSTgOZN3YtGFNUVTUrzA0NfqliGlVmvhVx9dfl8ZfR8x2QP7pKSA1OZF9+CNGYBL5JrDDx/lLYuSZ53IGzCEsXOTPbis="`,
		//	`x-main`:          `"jQLMuSVXzVbbMrzyv?Bi1PdZeKnxHKaMiyWCdZeUWeQn4BPVJf@jZJ210G6v1JLF"`,
		//	`at-main`:         `Atza|IwEBIN7Pzq7Fuo25xdtGtfgRE5w3F2G1UFXKO9uqnkgpxI02OUY8i025t2gLkM7-Tqhap2N4ksBnGtQV3mq3kKzaTuI6G7ivKQPlpzHyzTqZ1BG1u_U7kac5XMiA0J4xZTgFWYzbvfpqhPxz9h0Iv8Rn3dlZ4a9-8bypq2cKBDVB22PVgPlybNksHmdc5iu1ezx-y2TJexFa2G2RMB2tvImCyGfr`,
		//	`sess-at-main`:    `"T9fgDzPciD9yihIi8E0jN9NHIA41SdK2x15EuQPKnho="`,
		//	`sst-main`:        `Sst1|PQEbZmuXIX0u5IQrrb8Ltsx0CUNhMdvCEfnoPBoD7MxN_n_ZMVT0KGxuo7S8yppSo907XIoGgNOkvaSPStDiYkqqWLvV3aVV29qTTLltgsmYEVba6JKU7uxeT97QfPHh_JfZWR6ng4PpcYuQ54Gk2royHG0szRFWs8upoziypt9zEIMOPdXaDVb6D-Drpjzp0yIIBYKlJ0asNeswq6vGavjJXoPRWmCwBYNZPVvag1TauVFhqQcWDjAQ00m3AtF7z9T4rjJun1ZT5M5El19ndfqHxwhkjsLGysXMWl2ZYa0787w`,
		//	`lc-main`:         `en_US`,
		//	`session-id-time`: `2082787201l`,
		//	`i18n-prefs`:      `USD`,
		//	`csm-hit`:         `tb:WWF0B4BH9ESRKRQBDKHA+s-ZRH5A78SP7WD33ZF1ZFH|1628844051973&t:1628844051973&adb:adblk_yes`,
		//}
		//
		//cookies := []*http.Cookie{}
		//for ix, _ := range rawCookies {
		//	v := rawCookies[ix]
		//	cookies = append(cookies, &http.Cookie{Name: ix, Value: v})
		//	fmt.Println(ix + "=" + v)
		//}
		//
		//cookieJar, _ := cookiejar.New(nil)
		//domain, _ := url.Parse("https://www.amazon.com")
		//cookieJar.SetCookies(domain, cookies)
		//client.Jar = cookieJar
		resp, err := a.http.Do(req)
		if err != nil {
			return a.payload.ReportUnexpectedError(err)
		}

		if resp.StatusCode == http.StatusServiceUnavailable || resp.StatusCode == http.StatusForbidden {
			return a.payload.ReportError(taskStatus_ProxyBanned)
		}

		if resp.StatusCode != http.StatusOK {
			return a.payload.ReportError(taskStatus_UnknownHttpErrorMonitor(resp.StatusCode))
		}

		bodyBytes, err := ioutil.ReadAll(resp.Body)
		bodyStr := string(bodyBytes)
		if strings.Contains(bodyStr, "/errors/validateCaptcha") {
			return a.payload.ReportError(taskStatus_CaptchaDetected)
		}

		info := new(CartInfo)
		err = jsoniter.Unmarshal(bodyBytes, info)
		if err != nil || !info.IsOk {
			return a.payload.ReportError(taskStatus_UnknownErrorMonitor)
		}

		if len(info.Items) == 0 {
			return a.payload.ReportError(taskStatus_MonitorOutOfStock)
		}

		consumer([]byte(taskStatus_MonitorInStock.Title))
		return nil
	}

	//infiniteTimeout := time.Second * 3
	//infiniteTimeout := time.Duration(math.MaxInt64)
	//config.Timeout = &infiniteTimeout

	return config.ExecuteOnceAndShare()
}

func (a *checkoutTask) receiveMonitoringResult(stream monitor.Monitor_WatchStreamedClient) (*integration.MonitoringStatusChanged, error) {
	span, _ := apm.StartSpan(a.payload.Context, "receiveMonitoringResult", TracingSpanName)
	defer span.End()
	var entry *integration.MonitoringStatusChanged
	for {
		if a.payload.Context.Err() != nil {
			return nil, a.payload.Context.Err()
		}

		result, err := stream.Recv()
		if err != nil {
			entry = result
			if err != io.EOF {
				return nil, err
			}

			break
		}

		if core.IsTaskCompleted(result.Status) {
			entry = result
			break
		}
	}

	return entry, nil
}

func (a *checkoutTask) initTurbo() *core.StepExecutionFailure {
	span, _ := apm.StartSpan(a.payload.Context, "initTurbo", TracingSpanName)
	defer span.End()
	//if (proxyRotation === 'Switch') {
	//  await jaClient?.changeProxy(checkoutSet.random());
	//}
	//if (this.maxCartAttempts !== -1 && this.maxCartAttempts === this.cartAttempts) {
	//  this.cartAttempts = 0;
	//  this.step -= 1;
	//  return;
	//}
	//this.cartAttempts += 1;
	region := amazonRegion(a.siteConfig.Region)
	startUrl := getRawBaseUrl(region)
	if a.useSmile {
		startUrl = convertToAmazonSmile(startUrl)
	}

	query := url.Values{
		"ref_":               {"dp_start-bbf_1_glance_buyNow_2-1"},
		"referrer":           {"detail"},
		"pipelineType":       {"turbo"},
		"clientId":           {"retailwebsite"},
		"weblab":             {a.weblab.TurboWeblab},
		"temporaryAddToCart": {"1"},
		"isAsync":            {"1"},
		"addressID":          {a.validationResult.addressId},
		"offerListing.1":     {a.offerId},
		"quantity.1":         {strconv.Itoa(int(a.siteConfig.Qty))},
	}

	initTurboUrl, err := resolveAbsoluteUrl("/checkout/turbo-initiate?"+query.Encode(), region)
	if err != nil {
		return a.payload.ReportUnexpectedFailure(err)
	}

	headersOrder := []string{
		"sec-ch-ua",
		"x-amz-checkout-csrf-token",
		"rtt",
		"sec-ch-ua-mobile",
		"user-agent",
		"content-type",
		"x-amz-support-custom-signin",
		"accept",
		"X-Requested-With",
		"downlink",
		"ect",
		"origin",
		"sec-fetch-site",
		"sec-fetch-mode",
		"sec-fetch-dest",
		"accept-encoding",
		"accept-language",
	}
	headers := http.Header{
		// [
		//   'x-amz-checkout-entry-referer-url',
		//   `${startUrl}/gp/product/${randomSku}/ref=ppx_yo_dt_b_asin_title_o00?ie=UTF8&psc=1`,
		// ],
		// ['x-amz-turbo-checkout-dp-url', `${startUrl}/gp/product/${randomSku}/ref=ppx_yo_dt_b_asin_title_o00?ie=UTF8&psc=1`],
		// ["referer",	`${startUrl}/gp/product/${randomSku}/ref=ox_sc_saved_title_1?smid=ATVPDKIKX0DER&psc=1`],
		"sec-ch-ua":                   {*a.siteConfig.SecUa},
		"x-amz-checkout-csrf-token":   {a.cartSessionId},
		"rtt":                         {"0"},
		"sec-ch-ua-mobile":            {"?0"},
		"user-agent":                  {*a.siteConfig.Ua},
		"content-type":                {"application/x-www-form-urlencoded"},
		"x-amz-support-custom-signin": {"1"},
		"accept":                      {"*/*"},
		"X-Requested-With":            {"XMLHttpRequest"},
		"downlink":                    {"10"},
		"ect":                         {"4g"},
		"origin":                      {startUrl},
		"sec-fetch-site":              {"same-origin"},
		"sec-fetch-mode":              {"cors"},
		"sec-fetch-dest":              {"empty"},
		"accept-encoding":             {"gzip, deflate, br"},
		"accept-language":             {"en-US,en;q=0.9"},
		http.HeaderOrderKey:           headersOrder,
	}

	util.HeaderShuffle(headers)

	if region != regionUSA || a.sendAllCookies {
		a.http.UseCookieJar(a.loadSessionResult.cookieJar)
	} else {
		headers["cookie"] = []string{a.loadSessionResult.checkoutCookie.String()}
	}

	req := &http.Request{
		URL:    initTurboUrl,
		Header: headers,
	}

	response, err := a.http.Do(req)
	var bodyBytes []byte
	bodyBytes, err = ioutil.ReadAll(response.Body)
	if err != nil {
		return a.payload.ReportUnexpectedFailure(err)
	}

	body := string(bodyBytes)

	if response.StatusCode != http.StatusOK || len(body) <= 10 {
		var status *contracts.TaskStatusData
		switch response.StatusCode {
		case http.StatusForbidden:
			status = taskStatus_AmazonAntiBot
		case http.StatusServiceUnavailable:
			status = taskStatus_PotentialAccountClip
		case http.StatusNoContent:
			status = taskStatus_InvalidSettings
		default:
			status = taskStatus_FailedToATCError(response.StatusCode)
		}

		return a.payload.ReportError(status)
	}

	csrfHolderEntry := "anti-csrftoken-a2z' value='"
	if !strings.Contains(body, csrfHolderEntry) {
		if strings.Contains(body, "/errors/validateCaptcha") {
			return a.payload.ReportError(taskStatus_CaptchaDetected)
		}

		return a.payload.ReportError(taskStatus_UnknownCartingError)
	}

	if !strings.Contains(body, checkoutMap[region]) {
		return a.payload.ReportError(taskStatus_ItemNotAdded)
	}

	if len(response.Header.Get("X-Amz-Rid")) == 0 {
		return a.payload.ReportError(taskStatus_FailedToFetchCheckout)
	}

	a.checkoutSessionId = response.Header["X-Amz-Rid"][0]
	purchaseIdIx := strings.Index(body, csrfMap[region])
	a.purchaseId = body[purchaseIdIx : purchaseIdIx+19]
	csrfStartIx := strings.Index(body, csrfHolderEntry) + len(csrfHolderEntry) /* - 1*/
	csrfEndIx := strings.Index(body[csrfStartIx:], "'") + csrfStartIx
	csrf := body[csrfStartIx:csrfEndIx]
	a.csrf = &csrf

	a.http.SetFollowRedirects(false)
	a.step++

	a.payload.ProgressConsumer <- taskStatus_AddedToCart

	return nil
}
