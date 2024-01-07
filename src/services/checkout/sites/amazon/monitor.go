package amazon

import (
	"context"
	"errors"
	"github.com/CenturionLabs/centurion/checkout-service/contracts"
	"github.com/CenturionLabs/centurion/checkout-service/core"
	"github.com/CenturionLabs/centurion/checkout-service/services"
	"github.com/CenturionLabs/centurion/checkout-service/util"
	jsoniter "github.com/json-iterator/go"
	"github.com/shopspring/decimal"
	http "github.com/useflyent/fhttp"
	"io/ioutil"
	"net/url"
	"strconv"
	"strings"
	"time"
)

type monitorState int32

const (
	monitorStateSetProxy        monitorState = 0
	monitorStateGenerateSession              = 1
	monitorStateMonitor                      = 2
	monitorStateReset                        = 3
)

type statusType string

const (
	statusNormal statusType = "normal"
)

type statusChanges struct {
	Message string
	Type    statusType
}

func statusMessage(message string) *statusChanges {
	return &statusChanges{
		Message: message,
		Type:    statusNormal,
	}
}

type amazonMonitor struct {
	monitorMethod string
	sessionId     string
	resetDelay    time.Duration
	maxPrice      decimal.Decimal
	proxy         string
	delayCancelFn context.CancelFunc
	steps         map[monitorState]func()
	logger        core.ActivityLogger
	statusChanged chan *statusChanges
	http          services.HttpClient
	step          int32

	item *monitorDetails
}

func getAddToCartUrl(region amazonRegion) *url.URL {
	addToCardUrl, _ := resolveAbsoluteUrl("/gp/add-to-cart/json", region)
	return addToCardUrl
}

type cardInfo struct {
	IsOK  bool          `json:"isOK"`
	Items []interface{} `json:"items"`
}

type monitorFastArguments struct {
	ProxySet *contracts.ProxyPoolData
	SecUa    string
}

func (m *amazonMonitor) monitorFast(ctx context.Context, t *monitorFastArguments) error {
	m.statusChanged <- statusMessage("Monitoring")
	proxies, err := util.GetProxyUrls(t.ProxySet)
	if err != nil {
		return err
	}

	proxy, err := util.RandomUrl(proxies)
	if err == nil {
		m.http.ChangeProxy(proxy)
	}

	addToCartUrl := getAddToCartUrl(m.item.Region)
	headers := http.Header{
		"rtt":                       {"0"},
		"downlink":                  {"10"},
		"ect":                       {"4g"},
		"sec-ch-ua":                 {t.SecUa},
		"sec-ch-ua-mobile":          {"?0"},
		"upgrade-insecure-requests": {"1"},
		"origin":                    {getRawBaseUrl(m.item.Region)},
		"content-type":              {"application/x-www-form-urlencoded"},
		"user-agent":                {m.item.Ua},
		"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
		"sec-fetch-site":            {"same-origin"},
		"sec-fetch-mode":            {"navigate"},
		"sec-fetch-user":            {"?1"},
		"sec-fetch-dest":            {"document"},
		"referer":                   {getRawBaseUrl(m.item.Region)},
		"accept-encoding":           {"gzip, deflate, br"},
		"accept-language":           {"en-US,en;q=0.9"},
	}

	util.HeaderShuffle(headers)

	payload := url.Values{
		"session-id":     {m.sessionId},
		"clientName":     {"retailwebsite"},
		"nextPage":       {"cartitems"},
		"ASIN":           {m.item.Sku},
		"offerListingID": {m.item.OfferId},
		"quantity":       {"1"},
	}

	req := &http.Request{
		Method: "POST",
		URL:    addToCartUrl,
		Host:   getBaseUrl(m.item.Region).Hostname(),
		Header: headers,
		Body:   ioutil.NopCloser(strings.NewReader(payload.Encode())),
	}

	response, err := m.http.Do(req)
	if err != nil {
		return err
	}

	if response.StatusCode != 200 {
		if response.StatusCode == 503 || response.StatusCode == 403 {
			return errors.New("Proxy Banned | Rotating Proxy")
		} else {
			return errors.New("Unknown Monitor Error - " + strconv.Itoa(response.StatusCode))
		}
	}

	cartInfoRaw, err := ioutil.ReadAll(response.Body)
	if err != nil {
		return err
	}

	if strings.Contains(string(cartInfoRaw), "/errors/validateCaptcha") {
		return errors.New("Captcha Detected | Rotating Proxy")
	}

	cartInfo := new(cardInfo)
	err = jsoniter.Unmarshal(cartInfoRaw, cartInfo)
	if err != nil {
		return err
	}

	if len(cartInfo.Items) == 0 {
		return errors.New("OOS")
	} else if len(cartInfo.Items) > 0 {
		m.logger.Log("Picked Up Restock - offerId Fast")

		// todo: alertWatchers
		m.step += 1
	} else {
		return errors.New("Unknown Error Monitoring")
	}

	return nil
}
