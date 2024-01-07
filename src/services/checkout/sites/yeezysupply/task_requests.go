package yeezysupply

import (
	"github.com/CenturionLabs/centurion/checkout-service/util"
	jsoniter "github.com/json-iterator/go"
	http "github.com/useflyent/fhttp"
	"io/ioutil"
	"strings"
)

var pHeadersOrder = []string{
	":method",
	":authority",
	":scheme",
	":path",
}

func (t *yeezySupplyTask) createBloomRequest() *http.Request {
	instanaID := util.GetInstanaID()
	return &http.Request{
		URL: util.MustParseUrl(YeezySupplyWWWDomainUrl + "/api/yeezysupply/products/bloom"),
		Header: http.Header{
			"x-instana-t":        {instanaID},
			"sec-ch-ua-mobile":   {"?0"},
			"user-agent":         {t.userAgent},
			"x-instana-l":        {"1,correlationType=web;correlationId=" + instanaID},
			"x-instana-s":        {instanaID},
			"content-type":       {"application/json"},
			"sec-ch-ua-platform": {"\"Windows\""},
			"sec-ch-ua":          {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"accept":             {"*/*"},
			"sec-fetch-site":     {"same-origin"},
			"sec-fetch-mode":     {"cors"},
			"sec-fetch-dest":     {"empty"},
			"referer":            {YeezySupplyWWWDomainUrlSlashEnding},
			"accept-encoding":    {"gzip, deflate, br"},
			"accept-language":    {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"x-instana-t",
				"sec-ch-ua-mobile",
				"user-agent",
				"x-instana-l",
				"x-instana-s",
				"content-type",
				"sec-ch-ua-platform",
				"sec-ch-ua",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}
}

func (t *yeezySupplyTask) createProductOrArchiveRequest(afterSplash bool) *http.Request {
	var url string
	//if t.sku == "BY9611" {
	url = YeezySupplyWWWDomainUrl + "/product/" + t.sku
	/*} else {
		url = YeezySupplyWWWDomainUrl + "/archive/" + t.sku
	}*/

	req := &http.Request{
		URL: util.MustParseUrl(url),
		Header: http.Header{
			"cache-control":             {"max-age=0"},
			"sec-ch-ua":                 {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"sec-ch-ua-mobile":          {"?0"},
			"sec-ch-ua-platform":        {"\"Windows\""},
			"upgrade-insecure-requests": {"1"},
			"user-agent":                {t.userAgent},
			"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
			"sec-fetch-site":            {"same-origin"},
			"sec-fetch-mode":            {"navigate"},
			"sec-fetch-user":            {"?1"},
			"sec-fetch-dest":            {"document"},
			"referer":                   {YeezySupplyWWWDomainUrlSlashEnding},
			"accept-encoding":           {"gzip, deflate, br"},
			"accept-language":           {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"cache-control",
				"sec-ch-ua",
				"sec-ch-ua-mobile",
				"sec-ch-ua-platform",
				"upgrade-insecure-requests",
				"user-agent",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-user",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}

	if afterSplash {
		delete(req.Header, "sec-fetch-user")
		req.Header["referer"] = []string{YeezySupplyWWWDomainUrl + "/product/" + t.sku}
	} else {
		delete(req.Header, "cache-control")
	}

	return req
}

func (t *yeezySupplyTask) createProdPageRequest() *http.Request {
	rndStr := util.GetInstanaID()
	return &http.Request{
		URL: util.MustParseUrl(YeezySupplyWWWDomainUrl + "/api/products/" + t.sku),
		Header: http.Header{
			"x-instana-t":      {rndStr},
			"content-type":     {"application/json"},
			"x-instana-s":      {rndStr},
			"sec-ch-ua-mobile": {"?0"},
			"user-agent":       {t.userAgent},
			"sec-ch-ua":        {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"x-instana-l":      {"1,correlationType=web;correlationId=" + rndStr},
			"accept":           {"*/*"},
			"sec-fetch-site":   {"same-origin"},
			"sec-fetch-mode":   {"cors"},
			"sec-fetch-dest":   {"empty"},
			"referer":          {YeezySupplyWWWDomainUrl + "/product/" + t.sku},
			"accept-encoding":  {"gzip, deflate, br"},
			"accept-language":  {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"x-instana-t",
				"content-type",
				"x-instana-s",
				"sec-ch-ua-mobile",
				"user-agent",
				"sec-ch-ua",
				"x-instana-l",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}
}

func (t *yeezySupplyTask) createCustomerBasketsRequest(useProductPage bool, checkoutAuthorization string) *http.Request {
	referer := YeezySupplyWWWDomainUrlSlashEnding
	if useProductPage {
		referer = YeezySupplyWWWDomainUrl + "/product/" + t.sku
	}

	return &http.Request{
		URL: util.MustParseUrl(YeezySupplyWWWDomainUrl + "/api/checkout/customer/baskets"),
		Header: http.Header{
			"user-agent":             {t.userAgent},
			"content-type":           {"application/json"},
			"checkout-authorization": {checkoutAuthorization},
			"accept":                 {"*/*"},
			"sec-fetch-site":         {"same-origin"},
			"sec-fetch-mode":         {"cors"},
			"sec-fetch-dest":         {"empty"},
			"referer":                {referer},
			"accept-encoding":        {"gzip, deflate, br"},
			"accept-language":        {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"user-agent",
				"content-type",
				"checkout-authorization",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}
}

func (t *yeezySupplyTask) createAvailabilityRequest() *http.Request {
	string2 := util.GetInstanaID()
	return &http.Request{
		URL: util.MustParseUrl(YeezySupplyWWWDomainUrl + "/api/products/" + t.sku + "/availability"),
		Header: http.Header{
			"x-instana-t":      {string2},
			"content-type":     {"application/json"},
			"x-instana-s":      {string2},
			"sec-ch-ua-mobile": {"?0"},
			"user-agent":       {t.userAgent},
			"sec-ch-ua":        {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"x-instana-l":      {"1,correlationType=web;correlationId=" + string2},
			"accept":           {"*/*"},
			"sec-fetch-site":   {"same-origin"},
			"sec-fetch-mode":   {"cors"},
			"sec-fetch-dest":   {"empty"},
			"referer":          {YeezySupplyWWWDomainUrl + "/product/" + t.sku},
			"accept-encoding":  {"gzip, deflate, br"},
			"accept-language":  {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"x-instana-t",
				"content-type",
				"x-instana-s",
				"sec-ch-ua-mobile",
				"user-agent",
				"sec-ch-ua",
				"x-instana-l",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}
}

func (t *yeezySupplyTask) createDownloadSharedJsonReq() *http.Request {
	return &http.Request{
		URL: util.MustParseUrl(YeezySupplyWWWDomainUrl + "/hpl/content/yeezy-supply/releases/" + t.sku + "/shared.json"),
		Header: http.Header{
			"sec-ch-ua":        {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"accept":           {"application/json, text/plain, */*"},
			"sec-ch-ua-mobile": {"?0"},
			"user-agent":       {t.userAgent},
			"sec-fetch-site":   {"same-origin"},
			"sec-fetch-mode":   {"cors"},
			"sec-fetch-dest":   {"empty"},
			"referer":          {YeezySupplyWWWDomainUrl + "/product/" + t.sku},
			"accept-encoding":  {"gzip, deflate, br"},
			"accept-language":  {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"sec-ch-ua",
				"accept",
				"sec-ch-ua-mobile",
				"user-agent",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}
}

func (t *yeezySupplyTask) createDownloadEnUsJsonReq() *http.Request {
	return &http.Request{
		URL: util.MustParseUrl(YeezySupplyWWWDomainUrl + "/hpl/content/yeezy-supply/releases/" + t.sku + "/en_US.json"),
		Header: http.Header{
			"sec-ch-ua":        {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"accept":           {"application/json, text/plain, */*"},
			"sec-ch-ua-mobile": {"?0"},
			"user-agent":       {t.userAgent},
			"sec-fetch-site":   {"same-origin"},
			"sec-fetch-mode":   {"cors"},
			"sec-fetch-dest":   {"empty"},
			"referer":          {YeezySupplyWWWDomainUrl + "/product/" + t.sku},
			"accept-encoding":  {"gzip, deflate, br"},
			"accept-language":  {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"sec-ch-ua",
				"accept",
				"sec-ch-ua-mobile",
				"user-agent",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}
}

func (t *yeezySupplyTask) createPixelSendRequest(akamUrl, data string) *http.Request {
	instanaID := util.GetInstanaID()
	req, _ := http.NewRequest("POST", akamUrl, strings.NewReader(data))
	util.AddHeaders(req.Header, http.Header{
		//"content-length": {"DEFAULT_VALUE"},
		"x-instana-t":        {instanaID},
		"sec-ch-ua-mobile":   {"?0"},
		"user-agent":         {t.userAgent},
		"x-instana-l":        {"1,correlationType=web;correlationId=" + instanaID},
		"x-instana-s":        {instanaID},
		"content-type":       {"application/x-www-form-urlencoded"},
		"sec-ch-ua-platform": {"\"Windows\""},
		"sec-ch-ua":          {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
		"accept":             {"*/*"},
		"origin":             {YeezySupplyWWWDomainUrl},
		"sec-fetch-site":     {"same-origin"},
		"sec-fetch-mode":     {"cors"},
		"sec-fetch-dest":     {"empty"},
		"referer":            {YeezySupplyWWWDomainUrlSlashEnding},
		"accept-encoding":    {"gzip, deflate, br"},
		"accept-language":    {"en-US,en;q=0.9"},

		http.HeaderOrderKey: {
			"x-instana-t",
			"sec-ch-ua-mobile",
			"user-agent",
			"x-instana-l",
			"x-instana-s",
			"content-type",
			"sec-ch-ua-platform",
			"sec-ch-ua",
			"accept",
			"origin",
			"sec-fetch-site",
			"sec-fetch-mode",
			"sec-fetch-dest",
			"referer",
			"accept-encoding",
			"accept-language",
		},

		// http.PHeaderOrderKey: pHeadersOrder,
	})

	return req
}

func (t *yeezySupplyTask) createSendSensorRequest(useProductUrl bool, payload string) (*http.Request, error) {
	referer := YeezySupplyWWWDomainUrlSlashEnding
	if useProductUrl {
		referer = YeezySupplyWWWDomainUrl + "/product/" + t.sku
	}

	instanaId := util.GetInstanaID()
	req, err := http.NewRequest("POST", *t.sensorUrl, strings.NewReader(payload))
	if err != nil {
		return nil, err
	}
	util.AddHeaders(req.Header, http.Header{
		"x-instana-t":        {instanaId},
		"sec-ch-ua-mobile":   {"?0"},
		"user-agent":         {t.userAgent},
		"x-instana-l":        {"1,correlationType=web;correlationId=" + instanaId},
		"x-instana-s":        {instanaId},
		"content-type":       {"text/plain;charset=UTF-8"},
		"sec-ch-ua-platform": {"\"Windows\""},
		"sec-ch-ua":          {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
		"accept":             {"*/*"},
		"origin":             {YeezySupplyWWWDomainUrl},
		"sec-fetch-site":     {"same-origin"},
		"sec-fetch-mode":     {"cors"},
		"sec-fetch-dest":     {"empty"},
		"referer":            {referer},
		"accept-encoding":    {"gzip, deflate, br"},
		"accept-language":    {"en-US,en;q=0.9"},

		http.HeaderOrderKey: {
			"x-instana-t",
			"sec-ch-ua-mobile",
			"user-agent",
			"x-instana-l",
			"x-instana-s",
			"content-type",
			"sec-ch-ua-platform",
			"sec-ch-ua",
			"accept",
			"origin",
			"sec-fetch-site",
			"sec-fetch-mode",
			"sec-fetch-dest",
			"referer",
			"accept-encoding",
			"accept-language",
		},

		// http.PHeaderOrderKey: pHeadersOrder,
	})

	return req, nil
}

func (t *yeezySupplyTask) createFetchAkamaiScriptRequest(useProductUrl bool) (*http.Request, error) {
	referer := YeezySupplyWWWDomainUrlSlashEnding
	if useProductUrl {
		referer = YeezySupplyWWWDomainUrl + "/product/" + t.sku
	}

	req, err := http.NewRequest("GET", *t.sensorUrl, nil)
	if err != nil {
		return nil, err
	}
	util.AddHeaders(req.Header, http.Header{
		"sec-ch-ua":          {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
		"sec-ch-ua-mobile":   {"?0"},
		"user-agent":         {t.userAgent},
		"sec-ch-ua-platform": {"\"Windows\""},
		"accept":             {"*/*"},
		"sec-fetch-site":     {"same-origin"},
		"sec-fetch-mode":     {"no-cors"},
		"sec-fetch-dest":     {"script"},
		"referer":            {referer},
		"accept-encoding":    {"gzip, deflate, br"},
		"accept-language":    {"en-US,en;q=0.9"},

		http.HeaderOrderKey: {
			"sec-ch-ua",
			"sec-ch-ua-mobile",
			"user-agent",
			"sec-ch-ua-platform",
			"accept",
			"sec-fetch-site",
			"sec-fetch-mode",
			"sec-fetch-dest",
			"referer",
			"accept-encoding",
			"accept-language",
		},

		// http.PHeaderOrderKey: pHeadersOrder,
	})

	return req, nil
}

func (t *yeezySupplyTask) createPixelRequest(requestUrl string) *http.Request {
	return &http.Request{
		URL: util.MustParseUrl(requestUrl),
		Header: http.Header{
			"sec-ch-ua":          {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"sec-ch-ua-mobile":   {"?0"},
			"user-agent":         {t.userAgent},
			"sec-ch-ua-platform": {"\"Windows\""},
			"accept":             {"*/*"},
			"sec-fetch-site":     {"same-origin"},
			"sec-fetch-mode":     {"no-cors"},
			"sec-fetch-dest":     {"script"},
			"referer":            {YeezySupplyWWWDomainUrlSlashEnding},
			"accept-encoding":    {"gzip, deflate, br"},
			"accept-language":    {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"sec-ch-ua",
				"sec-ch-ua-mobile",
				"user-agent",
				"sec-ch-ua-platform",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}
}

func (t *yeezySupplyTask) createHomePageRequest() *http.Request {
	return &http.Request{
		URL: util.MustParseUrl(YeezySupplyWWWDomainUrlSlashEnding),
		Header: http.Header{
			"cache-control":             {"max-age=0"},
			"sec-ch-ua":                 {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"sec-ch-ua-mobile":          {"?0"},
			"sec-ch-ua-platform":        {"\"Windows\""},
			"upgrade-insecure-requests": {"1"},
			"user-agent":                {t.userAgent},
			"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
			"sec-fetch-site":            {"none"},
			"sec-fetch-mode":            {"navigate"},
			"sec-fetch-user":            {"?1"},
			"sec-fetch-dest":            {"document"},
			"accept-encoding":           {"gzip, deflate, br"},
			"accept-language":           {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"cache-control",
				"sec-ch-ua",
				"sec-ch-ua-mobile",
				"sec-ch-ua-platform",
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

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}
}

func (t *yeezySupplyTask) createFetchWaitingRoomConfigJsonRequest() *http.Request {
	return &http.Request{
		URL: util.MustParseUrl(YeezySupplyWWWDomainUrl + "/hpl/content/yeezy-supply/config/US/waitingRoomConfig.json"),
		Header: http.Header{
			"sec-ch-ua":        {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"accept":           {"application/json, text/plain, */*"},
			"sec-ch-ua-mobile": {"?0"},
			"user-agent":       {t.userAgent},
			"sec-fetch-site":   {"same-origin"},
			"sec-fetch-mode":   {"cors"},
			"sec-fetch-dest":   {"empty"},
			"referer":          {YeezySupplyWWWDomainUrl + "/product/" + t.sku},
			"accept-encoding":  {"gzip, deflate, br"},
			"accept-language":  {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"sec-ch-ua",
				"accept",
				"sec-ch-ua-mobile",
				"user-agent",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}
}

func (t *yeezySupplyTask) createAddToCartRequest(payload []*addToCartPayload) (*http.Request, error) {
	atcPayloadJson, err := jsoniter.MarshalToString(payload)
	if err != nil {
		return nil, err
	}

  t.logger.WithField("request", atcPayloadJson).Infoln("Atc request")
	rndStr := util.GetInstanaID()
	req, _ := http.NewRequest("POST", YeezySupplyWWWDomainUrl+"/api/checkout/baskets/-/items", strings.NewReader(atcPayloadJson))
	util.AddHeaders(req.Header, http.Header{
		//"content-length":       {"DEFAULT_VALUE"},
		"x-instana-t":            {rndStr},
		"sec-ch-ua-mobile":       {"?0"},
		"user-agent":             {t.userAgent},
		"x-instana-l":            {"1,correlationType=web;correlationId=" + rndStr},
		"x-instana-s":            {rndStr},
		"content-type":           {"application/json"},
		"checkout-authorization": {"null"},
		"sec-ch-ua":              {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
		"accept":                 {"*/*"},
		"origin":                 {YeezySupplyWWWDomainUrl},
		"sec-fetch-site":         {"same-origin"},
		"sec-fetch-mode":         {"cors"},
		"sec-fetch-dest":         {"empty"},
		"referer":                {YeezySupplyWWWDomainUrl + "/product/" + t.keywords[0]},
		"accept-encoding":        {"gzip, deflate, br"},
		"accept-language":        {"en-US,en;q=0.9"},

		http.HeaderOrderKey: {
			"content-length",
			"x-instana-t",
			"sec-ch-ua-mobile",
			"user-agent",
			"x-instana-l",
			"x-instana-s",
			"content-type",
			"checkout-authorization",
			"sec-ch-ua",
			"accept",
			"origin",
			"sec-fetch-site",
			"sec-fetch-mode",
			"sec-fetch-dest",
			"referer",
			"accept-encoding",
			"accept-language",
		},

		// http.PHeaderOrderKey: pHeadersOrder,
	})

	return req, nil
}

func (t *yeezySupplyTask) createQueueRequest() *http.Request {
	return &http.Request{
		URL: util.MustParseUrl(t.queueUrl),
		Header: http.Header{
			"sec-ch-ua":        {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"accept":           {"application/json, text/plain, */*"},
			"sec-ch-ua-mobile": {"?0"},
			"user-agent":       {t.userAgent},
			"sec-fetch-site":   {"same-origin"},
			"sec-fetch-mode":   {"cors"},
			"sec-fetch-dest":   {"empty"},
			"referer":          {YeezySupplyWWWDomainUrl + "/product/" + t.sku},
			"accept-encoding":  {"gzip, deflate, br"},
			"accept-language":  {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"sec-ch-ua",
				"accept",
				"sec-ch-ua-mobile",
				"user-agent",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},

			// http.PHeaderOrderKey: pHeadersOrder,
		},
	}
}

func (t *yeezySupplyTask) createCheckoutBasketRequest(basketId, checkoutAuthorization, payloadRawJson string) (*http.Request, error) {
	instanaID := util.GetInstanaID()
	req, err := http.NewRequest("PATCH", YeezySupplyWWWDomainUrl+"/api/checkout/baskets/"+basketId, strings.NewReader(payloadRawJson))
	if err != nil {
		return nil, err
	}

	util.AddHeaders(req.Header, http.Header{
		"x-instana-t": {instanaID},
		//"content-length":       {"DEFAULT_VALUE"},
		"sec-ch-ua-mobile":       {"?0"},
		"user-agent":             {t.userAgent},
		"x-instana-l":            {"1,correlationType=web;correlationId=" + instanaID},
		"x-instana-s":            {instanaID},
		"content-type":           {"application/json"},
		"checkout-authorization": {checkoutAuthorization},
		"sec-ch-ua":              {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
		"accept":                 {"*/*"},
		"origin":                 {YeezySupplyWWWDomainUrl},
		"sec-fetch-site":         {"same-origin"},
		"sec-fetch-mode":         {"cors"},
		"sec-fetch-dest":         {"empty"},
		"referer":                {YeezySupplyWWWDomainUrl + "/delivery"},
		"accept-encoding":        {"gzip, deflate, br"},
		"accept-language":        {"en-US,en;q=0.9"},

		http.HeaderOrderKey: {
			"x-instana-t",
			"content-length",
			"sec-ch-ua-mobile",
			"user-agent",
			"x-instana-l",
			"x-instana-s",
			"content-type",
			"checkout-authorization",
			"sec-ch-ua",
			"accept",
			"origin",
			"sec-fetch-site",
			"sec-fetch-mode",
			"sec-fetch-dest",
			"referer",
			"accept-encoding",
			"accept-language",
		},

		// http.PHeaderOrderKey: pHeadersOrder,
	})

	return req, nil
}

func (t *yeezySupplyTask) createProcessPaymentReq(checkoutAuthorization, payload string) (*http.Request, error) {
	rnd := util.GetInstanaID()

	req, err := http.NewRequest("POST", YeezySupplyWWWDomainUrl+"/api/checkout/orders", strings.NewReader(payload))
	if err != nil {
		return nil, err
	}

	util.AddHeaders(req.Header, http.Header{
		//"content-length":       {"DEFAULT_VALUE"},
		"x-instana-t":            {rnd},
		"sec-ch-ua-mobile":       {"?0"},
		"user-agent":             {t.userAgent},
		"x-instana-l":            {"1,correlationType=web;correlationId=" + rnd},
		"x-instana-s":            {rnd},
		"content-type":           {"application/json"},
		"checkout-authorization": {checkoutAuthorization},
		"sec-ch-ua":              {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
		"accept":                 {"*/*"},
		"origin":                 {YeezySupplyWWWDomainUrl},
		"sec-fetch-site":         {"same-origin"},
		"sec-fetch-mode":         {"cors"},
		"sec-fetch-dest":         {"empty"},
		"referer":                {YeezySupplyWWWDomainUrl + "/payment"},
		"accept-encoding":        {"gzip, deflate, br"},
		"accept-language":        {"en-US,en;q=0.9"},

		http.HeaderOrderKey: {
			"content-length",
			"x-instana-t",
			"sec-ch-ua-mobile",
			"user-agent",
			"x-instana-l",
			"x-instana-s",
			"content-type",
			"checkout-authorization",
			"sec-ch-ua",
			"accept",
			"origin",
			"sec-fetch-site",
			"sec-fetch-mode",
			"sec-fetch-dest",
			"referer",
			"accept-encoding",
			"accept-language",
		},

		// http.PHeaderOrderKey: pHeadersOrder,
	})

	return req, nil
}

func (t *yeezySupplyTask) createPOWRequest(powUrl string) *http.Request {
	return &http.Request{
		URL: util.MustParseUrl(powUrl),
		Header: http.Header{
			"sec-ch-ua":                 {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"sec-ch-ua-mobile":          {"?0"},
			"sec-ch-ua-platform":        {"\"Windows\""},
			"upgrade-insecure-requests": {"1"},
			"user-agent":                {t.userAgent},
			"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
			"sec-fetch-site":            {"same-origin"},
			"sec-fetch-mode":            {"navigate"},
			"sec-fetch-dest":            {"iframe"},
			"referer":                   {YeezySupplyWWWDomainUrl + "/product/" + t.sku},
			"accept-encoding":           {"gzip, deflate, br"},
			"accept-language":           {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"sec-ch-ua",
				"sec-ch-ua-mobile",
				"sec-ch-ua-platform",
				"upgrade-insecure-requests",
				"user-agent",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
				"cookie",
			},
		},
	}
}

func (t *yeezySupplyTask) createNewsletterRequest(signupPath string) *http.Request {
	instanaId := util.GetInstanaID()
	return &http.Request{
		URL: util.MustParseUrl(YeezySupplyWWWDomainUrl + "/api/signup/" + signupPath + "?trigger="),
		Header: http.Header{
			"x-instana-t":        {instanaId},
			"sec-ch-ua-mobile":   {"?0"},
			"user-agent":         {t.userAgent},
			"x-instana-l":        {"1,correlationType=web;correlationId=" + instanaId},
			"x-instana-s":        {instanaId},
			"content-type":       {"application/json"},
			"sec-ch-ua-platform": {"\"Windows\""},
			"sec-ch-ua":          {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"accept":             {"*/*"},
			"sec-fetch-site":     {"same-origin"},
			"sec-fetch-mode":     {"cors"},
			"sec-fetch-dest":     {"empty"},
			"referer":            {YeezySupplyWWWDomainUrl + "/product/" + t.sku},
			"accept-encoding":    {"gzip, deflate, br"},
			"accept-language":    {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"x-instana-t",
				"sec-ch-ua-mobile",
				"user-agent",
				"x-instana-l",
				"x-instana-s",
				"content-type",
				"sec-ch-ua-platform",
				"sec-ch-ua",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},
		},
	}
}

func (t *yeezySupplyTask) createPOWVerifyPageRequest() *http.Request {
	instanaID := util.GetInstanaID()
	return &http.Request{
		URL: util.MustParseUrl(YeezySupplyWWWDomainUrl + "/_sec/cp_challenge/verify"),
		Header: http.Header{
			"x-instana-t":         {instanaID},
			"sec-ch-ua-mobile":    {"?0"},
			"user-agent":          {t.userAgent},
			"x-instana-l":         {"1,correlationType=web;correlationId=" + instanaID},
			"x-sec-clge-req-type": {"ajax"},
			"x-instana-s":         {instanaID},
			"sec-ch-ua-platform":  {"\"Windows\""},
			"sec-ch-ua":           {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
			"accept":              {"*/*"},
			"sec-fetch-site":      {"same-origin"},
			"sec-fetch-mode":      {"cors"},
			"sec-fetch-dest":      {"empty"},
			"referer":             {YeezySupplyWWWDomainUrl + "/product/" + t.sku},
			"accept-encoding":     {"gzip, deflate, br"},
			"accept-language":     {"en-US,en;q=0.9"},

			http.HeaderOrderKey: {
				"x-instana-t",
				"sec-ch-ua-mobile",
				"user-agent",
				"x-instana-l",
				"x-sec-clge-req-type",
				"x-instana-s",
				"sec-ch-ua-platform",
				"sec-ch-ua",
				"accept",
				"sec-fetch-site",
				"sec-fetch-mode",
				"sec-fetch-dest",
				"referer",
				"accept-encoding",
				"accept-language",
			},
		},
	}
}

func (t *yeezySupplyTask) createSubmitCouponRequest(basketId string, checkoutAuthorization string, form string) (*http.Request, error) {
	req, err := http.NewRequest("POST", YeezySupplyWWWDomainUrl+"/api/checkout/baskets/"+basketId+"/coupons/", ioutil.NopCloser(strings.NewReader(form)))
	if err != nil {
		return nil, err
	}

	instanaId := util.GetInstanaID()
	util.AddHeaders(req.Header, http.Header{
		"x-instana-t":            {instanaId},
		"sec-ch-ua-mobile":       {"?0"},
		"x-instana-l":            {"1,correlationType=web;correlationId=" + instanaId},
		"content-type":           {"application/json"},
		"user-agent":             {t.userAgent},
		"x-instana-s":            {instanaId},
		"checkout-authorization": {checkoutAuthorization},
		"sec-ch-ua-platform":     {"\"Windows\""},
		"sec-ch-ua":              {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
		"accept":                 {"*/*"},
		"origin":                 {YeezySupplyWWWDomainUrl},
		"sec-fetch-site":         {"same-origin"},
		"sec-fetch-mode":         {"cors"},
		"sec-fetch-dest":         {"empty"},
		"referer":                {YeezySupplyWWWDomainUrl + "/payment"},
		"accept-encoding":        {"gzip, deflate, br"},
		"accept-language":        {"en-US,en;q=0.9"},

		http.HeaderOrderKey: {
			"content-length",
			"x-instana-t",
			"sec-ch-ua-mobile",
			"x-instana-l",
			"content-type",
			"user-agent",
			"x-instana-s",
			"checkout-authorization",
			"sec-ch-ua-platform",
			"sec-ch-ua",
			"accept",
			"origin",
			"sec-fetch-site",
			"sec-fetch-mode",
			"sec-fetch-dest",
			"referer",
			"accept-encoding",
			"accept-language",
		},
	})

	return req, nil
}

func (t *yeezySupplyTask) createConfirm3dsRequest(encodedData, checkoutAuthorization, json, termUrl string) (*http.Request, error) {
	req, err := http.NewRequest("POST", YeezySupplyWWWDomainUrl+"/api/checkout/payment-verification/"+encodedData, ioutil.NopCloser(strings.NewReader(json)))
	if err != nil {
		return nil, err
	}

	instanaId := util.GetInstanaID()
	util.AddHeaders(req.Header, http.Header{
		"x-instana-t":            {instanaId},
		"sec-ch-ua-mobile":       {"?0"},
		"user-agent":             {t.userAgent},
		"x-instana-l":            {"1,correlationType=web;correlationId=" + instanaId},
		"x-instana-s":            {instanaId},
		"content-type":           {"application/json"},
		"checkout-authorization": {checkoutAuthorization},
		"sec-ch-ua":              {"\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\""},
		"accept":                 {"*/*"},
		"origin":                 {YeezySupplyWWWDomainUrl},
		"sec-fetch-site":         {"same-origin"},
		"sec-fetch-mode":         {"cors"},
		"sec-fetch-dest":         {"empty"},
		"referer":                {termUrl},
		"accept-encoding":        {"gzip, deflate, br"},
		"accept-language":        {"en-US,en;q=0.9"},

		http.HeaderOrderKey: {
			"content-length",
      "x-instana-t",
      "sec-ch-ua-mobile",
      "user-agent",
      "x-instana-l",
      "x-instana-s",
      "content-type",
      "checkout-authorization",
      "sec-ch-ua",
      "accept",
      "origin",
      "sec-fetch-site",
      "sec-fetch-mode",
      "sec-fetch-dest",
      "referer",
      "accept-encoding",
      "accept-language",
    },
	})

	return req, nil
}
