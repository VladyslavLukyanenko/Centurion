package walmart

import (
  "github.com/CenturionLabs/centurion/checkout-service/util"
  http "github.com/useflyent/fhttp"
)

func (t *walmartTask) httpRequestString4(baseUrl string) *http.Request {
	h := http.Header{
		"pragma":                    {"no-cache"},
		"cache-control":             {"no-cache"},
		"upgrade-insecure-requests": {"1"},
		"user-agent":                {t.perimeterX.GetUserAgent()},
		"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
		"sec-fetch-site":            {"cross-site"},
		"sec-fetch-mode":            {"navigate"},
		"sec-fetch-user":            {"?1"},
		"sec-fetch-dest":            {"document"},
		"accept-encoding":           {"gzip, deflate, br"},
		"accept-language":           {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		Method: "POST",
		URL:    util.MustParseUrl(baseUrl + "?items=" + t.keywords[0] + "|" + t.offerId),
	}
}

func (t *walmartTask) createSubmitPaymentRequest() *http.Request {
	h := http.Header{
		//"content-length": {"DEFAULT_VALUE"},
		"accept":            {"application/json, text/javascript, */*; q=0.01"},
		"inkiru_precedence": {"false"},
		"wm_cvv_in_session": {"true"},
		"user-agent":        {t.perimeterX.GetUserAgent()},
		"wm_vertical_id":    {"0"},
		"content-type":      {"application/json"},
		"origin":            {"https://www.walmart.com"},
		"sec-fetch-site":    {"same-origin"},
		"sec-fetch-mode":    {"cors"},
		"sec-fetch-dest":    {"empty"},
		"referer":           {"https://www.walmart.com/checkout/"},
		"accept-encoding":   {"gzip, deflate, br"},
		"accept-language":   {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		Method: "POST",
		URL:    util.MustParseUrl("https://www.walmart.com/api/checkout/v3/contract/:PCID/payment"),
	}
}

func (t *walmartTask) createCartRequest() *http.Request {
	h := http.Header{
		"accept":          {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
		"user-agent":      {t.perimeterX.GetUserAgent()},
		"origin":          {"https://www.walmart.com"},
		"referer":         {"https://www.walmart.com/"},
		"accept-encoding": {"gzip, deflate, br"},
		"accept-language": {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		URL:    util.MustParseUrl("https://www.walmart.com/cart"),
	}
}

func (t *walmartTask) createHomeRequest() *http.Request {
	h := http.Header{
		"user-agent":                {t.perimeterX.GetUserAgent()},
		"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
		"purpose":                   {"prefetch"},
		"upgrade-insecure-requests": {"1"},
		"sec-fetch-site":            {"cross-site"},
		"sec-fetch-mode":            {"navigate"},
		"sec-fetch-dest":            {"document"},
		"referer":                   {"https://www.google.com/"},
		"accept-encoding":           {"gzip, deflate, br"},
		"accept-language":           {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		URL:    util.MustParseUrl("https://www.walmart.com/"),
	}
}

func (t *walmartTask) createCheckoutCustomerCreditCardRequest() *http.Request {
	h := http.Header{
		"user-agent":      {t.perimeterX.GetUserAgent()},
		"accept":          {"application/json"},
		"content-type":    {"application/json"},
		"origin":          {"https://www.walmart.com"},
		"sec-fetch-site":  {"same-origin"},
		"accept-language": {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		Method: "POST",
		URL:    util.MustParseUrl("https://www.walmart.com/api/checkout-customer/:CID/credit-card"),
	}
}

func (t *walmartTask) getHttpRequest8() *http.Request {
	h := http.Header{
		//"content-length": {"DEFAULT_VALUE"},
		"accept":          {"application/json"},
		"user-agent":      {t.perimeterX.GetUserAgent()},
		"content-type":    {"application/json"},
		"origin":          {"https://www.walmart.com"},
		"sec-fetch-site":  {"same-origin"},
		"sec-fetch-mode":  {"cors"},
		"sec-fetch-dest":  {"empty"},
		"referer":         {"https://www.walmart.com/checkout/"},
		"accept-encoding": {"gzip, deflate, br"},
		"accept-language": {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		Method: "POST",
		URL:    util.MustParseUrl("https://www.walmart.com/api/v3/saved/:CID/items"),
	}
}

func (t *walmartTask) httpRequestString3(string1 string) *http.Request {
	requestUrl := "https://affil.walmart.com/cart/" + string1 + "?items=" + t.keywords[0] + "|" + t.offerId

	h := http.Header{
		"cache-control":             {"max-age=0"},
		"upgrade-insecure-requests": {"1"},
		"user-agent":                {t.perimeterX.GetUserAgent()},
		"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
		"sec-fetch-site":            {"cross-site"},
		"sec-fetch-mode":            {"navigate"},
		"sec-fetch-user":            {"?1"},
		"sec-fetch-dest":            {"document"},
		"accept-encoding":           {"gzip, deflate, br"},
		"accept-language":           {t.perimeterX.GetAcceptLanguage()},
		//"cookie": {"DEFAULT_VALUE"},
	}

	if len(*t.modifiedSince) > 0 {
		h.Set("if-modified-since", *t.modifiedSince)
	}

	t.modifiedSince = getDateTimeNowWeb()

	return &http.Request{
		Header: h,
		URL:    util.MustParseUrl(requestUrl),
	}
}

func (t *walmartTask) createCartCtrRequest(crt string) *http.Request {
	requestUrl := "https://www.walmart.com/api/v3/cart/" + crt
	h := http.Header{
		//"content-length": {"DEFAULT_VALUE"},
		"user-agent":        {t.perimeterX.GetUserAgent()},
		"credentials":       {"include"},
		"omitcorrelationid": {"true"},
		"content-type":      {"application/json"},
		"accept":            {"application/json, text/javascript, */*; q=0.01"},
		"omitcsrfjwt":       {"true"},
		"origin":            {"https://www.walmart.com"},
		"referer":           {"https://www.walmart.com/search/?query=" + t.possiblySomeProdPath},
		"accept-encoding":   {"gzip, deflate, br"},
		"accept-language":   {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		URL:    util.MustParseUrl(requestUrl),
	}
}

func (t *walmartTask) getHttpRequestc() *http.Request {
	h := http.Header{
		"upgrade-insecure-requests": {"1"},
		"user-agent":                {t.perimeterX.GetUserAgent()},
		"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
		"sec-fetch-site":            {"cross-site"},
		"sec-fetch-mode":            {"navigate"},
		"sec-fetch-user":            {"?1"},
		"sec-fetch-dest":            {"document"},
		"accept-encoding":           {"gzip, deflate, br"},
		"accept-language":           {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		URL:    util.MustParseUrl(t.referer),
	}
}

func (t *walmartTask) getHttpRequesth() *http.Request {
	h := http.Header{
		//"content-length": {"DEFAULT_VALUE"},
		"user-agent":            {t.perimeterX.GetUserAgent()},
		"credentials":           {"include"},
		"content-type":          {"application/json"},
		"accept":                {"application/json, text/javascript, */*; q=0.01"},
		"omitcsrfjwt":           {"true"},
		"wm_qos.correlation_id": {"df2291f5-73f5-494a-bfa8-dd0dbdc3f585"},
		"origin":                {"https://www.walmart.com"},
		"sec-fetch-site":        {"same-origin"},
		"sec-fetch-mode":        {"cors"},
		"sec-fetch-dest":        {"empty"},
		"referer":               {"https://www.walmart.com/cart"},
		"accept-encoding":       {"gzip, deflate, br"},
		"accept-language":       {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		Method: "POST",
		URL:    util.MustParseUrl("https://www.walmart.com/api/v3/cart/:CRT/items"),
	}
}

func (t *walmartTask) getHttpRequest6() *http.Request {
	h := http.Header{
		"user-agent":      {t.perimeterX.GetUserAgent()},
		"accept":          {"application/signed-exchange;v=b3;q=0.9,*/*;q=0.8"},
		"purpose":         {"prefetch"},
		"sec-fetch-site":  {"same-origin"},
		"sec-fetch-mode":  {"no-cors"},
		"sec-fetch-dest":  {"script"},
		"referer":         {"https://www.walmart.com/"},
		"accept-encoding": {"gzip, deflate, br"},
		"accept-language": {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		URL:    util.MustParseUrl("https://www.walmart.com/perimeterX/PXu6b0qd2S/init.js"),
	}
}

func (t *walmartTask) httpRequestPaymentToken3(paymentToken1 PaymentToken) *http.Request {
	h := http.Header{
		//"content-length": {"DEFAULT_VALUE"},
		"accept":            {"application/json, text/javascript, */*; q=0.01"},
		"inkiru_precedence": {"false"},
		"wm_cvv_in_session": {"true"},
		"user-agent":        {t.perimeterX.GetUserAgent()},
		"wm_vertical_id":    {"0"},
		"content-type":      {"application/json"},
		"origin":            {"https://www.walmart.com"},
		"sec-fetch-site":    {"same-origin"},
		"sec-fetch-mode":    {"cors"},
		"sec-fetch-dest":    {"empty"},
		"referer":           {"https://www.walmart.com/checkout/"},
		"accept-encoding":   {"gzip, deflate, br"},
		"accept-language":   {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		Method: "PUT",
		URL:    util.MustParseUrl("https://www.walmart.com/api/checkout/v3/contract/:PCID/order"),
	}
}

func (t *walmartTask) getHttpRequestk() *http.Request {
	h := http.Header{
		"upgrade-insecure-requests":         {"1"},
		"user-agent":                        {t.perimeterX.GetUserAgent()},
		"accept":                            {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
		"service-worker-navigation-preload": {"true"},
		"sec-fetch-site":                    {"same-origin"},
		"sec-fetch-mode":                    {"navigate"},
		"sec-fetch-user":                    {"?1"},
		"sec-fetch-dest":                    {"document"},
		"referer":                           {"https://www.walmart.com/"},
		"accept-encoding":                   {"gzip, deflate, br"},
		"accept-language":                   {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		URL:    util.MustParseUrl("https://www.walmart.com/cart"),
	}
}

func (t *walmartTask) httpRequestStringc(string1 string) *http.Request {
	h := http.Header{
		"upgrade-insecure-requests": {"1"},
		"user-agent":                {t.perimeterX.GetUserAgent()},
		"accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
		"sec-fetch-site":            {"none"},
		"sec-fetch-mode":            {"navigate"},
		"sec-fetch-user":            {"?1"},
		"sec-fetch-dest":            {"document"},
		"accept-encoding":           {"gzip, deflate, br"},
		"accept-language":           {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		URL:    util.MustParseUrl(string1),
	}
}

func (t *walmartTask) getHttpRequestb() *http.Request {
	h := http.Header{
		"content-length":  {"DEFAULT_VALUE"},
		"accept":          {"application/json"},
		"user-agent":      {t.perimeterX.GetUserAgent()},
		"content-type":    {"application/json"},
		"origin":          {"https://www.walmart.com"},
		"sec-fetch-site":  {"same-origin"},
		"sec-fetch-mode":  {"cors"},
		"sec-fetch-dest":  {"empty"},
		"referer":         {"https://www.walmart.com/ip/2020-21-Panini-Hoops-NBA-Basketball-Trading-Cards-Holiday-Blaster-Box-88-Cards-Retail-Exclusives/377461077"},
		"accept-encoding": {"gzip, deflate, br"},
		"accept-language": {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		Method: "POST",
		URL:    util.MustParseUrl("https://www.walmart.com/api/v3/cart/:CRT/items"),
	}
}

func (t *walmartTask) httpRequestString1(string1 string) *http.Request {
	h := http.Header{
		"content-length":  {"DEFAULT_VALUE"},
		"accept":          {"application/json"},
		"user-agent":      {t.perimeterX.GetUserAgent()},
		"content-type":    {"application/json"},
		"origin":          {"https://www.walmart.com"},
		"sec-fetch-site":  {"same-origin"},
		"sec-fetch-mode":  {"cors"},
		"sec-fetch-dest":  {"empty"},
		"referer":         {"https://www.walmart.com/checkout/"},
		"accept-encoding": {"gzip, deflate, br"},
		"accept-language": {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		Method: "POST",
		URL:    util.MustParseUrl("https://www.walmart.com/api//saved/:CID/items/" + string1 + "/transfer"),
	}
}

func (t *walmartTask) getHttpRequest9() *http.Request {
	h := http.Header{
		"content-length":    {"DEFAULT_VALUE"},
		"accept":            {"application/json, text/javascript, */*; q=0.01"},
		"omitcsrfjwt":       {"true"},
		"user-agent":        {t.perimeterX.GetUserAgent()},
		"credentials":       {"include"},
		"omitcorrelationid": {"true"},
		"content-type":      {"application/json"},
		"origin":            {"https://www.walmart.com"},
		"sec-fetch-site":    {"same-origin"},
		"sec-fetch-mode":    {"cors"},
		"sec-fetch-dest":    {"empty"},
		"referer":           {"https://www.walmart.com/search/?query=panini"},
		"accept-encoding":   {"gzip, deflate, br"},
		"accept-language":   {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		Method: "POST",
		URL:    util.MustParseUrl("https://www.walmart.com/api/v3/cart/lite/:CRT/items"),
	}
}

func (t *walmartTask) getHttpRequesti() *http.Request {
	h := http.Header{
		//"content-length": {"DEFAULT_VALUE"},
		"accept":          {"application/json"},
		"user-agent":      {t.perimeterX.GetUserAgent()},
		"content-type":    {"application/json"},
		"origin":          {"https://www.walmart.com"},
		"sec-fetch-site":  {"same-origin"},
		"sec-fetch-mode":  {"cors"},
		"sec-fetch-dest":  {"empty"},
		"referer":         {"https://www.walmart.com/"},
		"accept-encoding": {"gzip, deflate, br"},
		"accept-language": {t.perimeterX.GetAcceptLanguage()},
	}

	return &http.Request{
		Header: h,
		Method: "POST",
		URL:    util.MustParseUrl("https://www.walmart.com/electrode/api/logger"),
	}
}

