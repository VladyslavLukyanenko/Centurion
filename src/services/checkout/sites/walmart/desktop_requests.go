package walmart

import (
  "github.com/CenturionLabs/centurion/checkout-service/util"
  "github.com/google/uuid"
  http "github.com/useflyent/fhttp"
  "time"
)

func (w *walmartDesktopApi) createSignUpRequest() *http.Request {
  h := http.Header{
    "authority":       {"www.task.walmart.com"},
    "pragma":          {"no-cache"},
    "cache-control":   {"no-cache"},
    "content-length":  {"DEFAULT_VALUE"},
    "user-agent":      {AndroidVersShort},
    "content-type":    {"application/json"},
    "accept":          {"*/*"},
    "origin":          {"https://www.task.walmart.com"},
    "referer":         {"https://www.task.walmart.com/account/signup?ref=domain"},
    "accept-language": {w.task.perimeterX.GetAcceptLanguage()},
  }

  for ix, _ := range MASP {
    toAdd := MASP[ix]
    h.Set(toAdd.Name, toAdd.Value)
  }

  return &http.Request{
    Header: h,
    Method: "POST",
    URL:    util.MustParseUrl("https://www.task.walmart.com/account/electrode/api/identity/sign-up"),
  }
}

func (w *walmartDesktopApi) createSignInRequest() *http.Request {
  h := http.Header{
    "authority":     {"www.task.walmart.com"},
    "pragma":        {"no-cache"},
    "cache-control": {"no-cache"},
    //"content-length": {"DEFAULT_VALUE"},
    "user-agent":      {AndroidVersShort},
    "content-type":    {"application/json"},
    "accept":          {"*/*"},
    "origin":          {"https://www.task.walmart.com"},
    "referer":         {"https://www.task.walmart.com/account/login?tid=0&returnUrl=%2F"},
    "accept-language": {w.task.perimeterX.GetAcceptLanguage()},
  }

  for ix, _ := range MASP {
    toAdd := MASP[ix]
    h.Set(toAdd.Name, toAdd.Value)
  }

  return &http.Request{
    Header: h,
    URL:    util.MustParseUrl("https://www.task.walmart.com/account/electrode/api/signin?tid=0&returnUrl=/"),
  }
}

func (w *walmartDesktopApi) createHomePageRequest() *http.Request {
  h := http.Header{
    "upgrade-insecure-requests": {"1"},
    "user-agent":                {w.task.perimeterX.GetUserAgent()},
    "accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
    "sec-fetch-site":            {"cross-site"},
    "sec-fetch-mode":            {"navigate"},
    "sec-fetch-user":            {"?1"},
    "sec-fetch-dest":            {"document"},
    "referer":                   {"https://www.task.google.com/"},
    "accept-encoding":           {"gzip, deflate, br"},
    "accept-language":           {w.task.perimeterX.GetAcceptLanguage()},
  }

  return &http.Request{
    Header: h,
    URL:    util.MustParseUrl("https://www.task.walmart.com/"),
  }
}

func (w *walmartDesktopApi) createAddToCartRequestByMethod(method string) *http.Request {
  requestUrl := "https://affil.walmart.com/cart/" + method + "?items=" + w.task.keywords[0] + "|" + w.task.offerId
  expiredTime := time.Unix(0, 0)
  w.task.cookies.SetCookies(w.task.getRootCookieHost(), []*http.Cookie{
    {Name: "CRT", Expires: expiredTime},
    {Name: "cart-item-count", Expires: expiredTime},
    {Name: "hasCRT", Expires: expiredTime},
  })

  h := http.Header{
    "upgrade-insecure-requests": {"1"},
    "user-agent":                {w.task.perimeterX.GetUserAgent()},
    "accept":                    {"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"},
    "sec-fetch-site":            {"cross-site"},
    "sec-fetch-mode":            {"navigate"},
    "sec-fetch-user":            {"?1"},
    "sec-fetch-dest":            {"document"},

    "accept-encoding": {w.task.perimeterX.GetAcceptEncoding()},
    "accept-language": {w.task.perimeterX.GetAcceptLanguage()},
    //"cookie": {"DEFAULT_VALUE"},
  }

  if w.task.f_String_c != nil {
    h.Set("referer", "https://"+*w.task.f_String_c+"/")
    if *w.task.f_String_c == "w.task.co" {
      h.Del("sec-fetch-user")
    }
  }

  if w.task.modifiedSince != nil {
    h.Set("if-modified-since", *w.task.modifiedSince)
  }

  w.task.modifiedSince = getDateTimeNowWeb()

  return &http.Request{
    Header: h,
    URL:    util.MustParseUrl(requestUrl),
  }
}

func (w *walmartDesktopApi) createInitCartRequest() *http.Request {
  host := w.task.getRootCookieHost()
  if w.task.isActive && !w.task.hasCookie("CRT", host) {
    newUUID, _ := uuid.NewUUID()
    w.task.cookies.SetCookies(host, []*http.Cookie{
      {Name: "CRT", Value: newUUID.String()},
      {Name: "hasCRT", Value: "1"},
    })
  }

  switch w.task.taskId {
  case 0:
    return w.createAddToCartRequest()
  case 1:
    return w.createAddToCartLiteRequest()
  default:
    return w.createAddToCartLiteRequest()
  }
}

func (w *walmartDesktopApi) createAddToCartLiteRequest() *http.Request {
  h := http.Header{
    //"content-length", "DEFAULT_VALUE"},
    "accept":            {"application/json, text/javascript, */*; q=0.01"},
    "omitcsrfjwt":       {"true"},
    "user-agent":        {w.task.perimeterX.GetUserAgent()},
    "credentials":       {"include"},
    "omitcorrelationid": {"true"},
    "content-type":      {"application/json"},
    "origin":            {"https://www.task.walmart.com"},
    "sec-fetch-site":    {"same-origin"},
    "sec-fetch-mode":    {"cors"},
    "sec-fetch-dest":    {"empty"},
    "referer":           {"https://www.task.walmart.com/search/?query=" + w.task.possiblySomeProdPath},
    "accept-encoding":   {"gzip, deflate, br"},
    "accept-language":   {w.task.perimeterX.GetAcceptLanguage()},
  }

  rawUrl := "https://www.task.walmart.com/api/v3/cart/lite/"
  if w.task.isActive {
    rawUrl += "customer/:CRT/items"
  } else {
    rawUrl += "guest/:CID/items"
  }

  return &http.Request{
    Header: h,
    Method: "POST",
    URL:    util.MustParseUrl(rawUrl),
  }
}

func (w *walmartDesktopApi) createAddToCartRequest() *http.Request {
  h := http.Header{
    //"content-length", "DEFAULT_VALUE"},
    "accept":          {"application/json"},
    "user-agent":      {w.task.perimeterX.GetUserAgent()},
    "content-type":    {"application/json"},
    "origin":          {"https://www.task.walmart.com"},
    "sec-fetch-site":  {"same-origin"},
    "sec-fetch-mode":  {"cors"},
    "sec-fetch-dest":  {"empty"},
    "referer":         {w.task.referer},
    "accept-encoding": {"gzip, deflate, br"},
    "accept-language": {w.task.perimeterX.GetAcceptLanguage()},
  }

  rawUrl := "https://www.task.walmart.com/api/v3/cart/"
  if w.task.isActive {
    rawUrl += "customer/:CRT/items"
  } else {
    rawUrl += "guest/:CID/items"
  }

  return &http.Request{
    Header: h,
    Method: "POST",
    URL:    util.MustParseUrl(rawUrl),
  }
}

func (w *walmartDesktopApi) createEnsureInStockRequest(productId string, fullProductRoute bool) *http.Request {
  parseUrl := "https://www.task.walmart.com/terra-firma/graphql?v=2&options=timing%2Cnonnull%2Cerrors%2Ccontext&id=FullProductHolidaysRoute-web"
  if fullProductRoute {
    parseUrl = "https://www.task.walmart.com/terra-firma/graphql?options=timing,nonnull,context&v=2&id=FullProductRoute-web"
  }

  h := http.Header{
    //"content-length": "DEFAULT_VALUE"},
    "user-agent":        {w.task.perimeterX.GetUserAgent()},
    "credentials":       {"include"},
    "omitcorrelationid": {"true"},
    "content-type":      {"application/json; charset=utf-8"},
    "accept":            {"application/json, text/javascript, */*; q=0.01"},
    "omitcsrfjwt":       {"true"},
    "origin":            {"https://www.task.walmart.com"},
    "referer":           {"https://www.task.walmart.com/search/?query=" + w.task.f_String_1},
    "accept-encoding":   {"gzip, deflate, br"},
    "accept-language":   {w.task.perimeterX.GetAcceptLanguage()},
  }

  return &http.Request{
    Header: h,
    Method: "POST",
    URL:    util.MustParseUrl(parseUrl),
  }
}

func (w *walmartDesktopApi) createCartCrtRequest(crt string) *http.Request {
  panic("implement me")
}

// $FF: renamed from: 1 (io.trickle.task.sites.walmart.util.PaymentToken) io.vertx.ext.web.client.HttpRequest
func (w *walmartDesktopApi) createCheckoutStep1Request(paymentToken1 *PaymentToken) *http.Request {
  h := http.Header{
    //"content-length": {"DEFAULT_VALUE"},
    "accept":            {"application/json, text/javascript, */*; q=0.01"},
    "wm_cvv_in_session": {"true"},
    "user-agent":        {w.task.perimeterX.GetUserAgent()},
    "wm_vertical_id":    {"0"},
    "content-type":      {"application/json"},
    "origin":            {"https://www.task.walmart.com"},
    "sec-fetch-site":    {"same-origin"},
    "sec-fetch-mode":    {"cors"},
    "sec-fetch-dest":    {"empty"},
    "referer":           {"https://www.task.walmart.com/checkout/"},
    "accept-encoding":   {"gzip, deflate, br"},
    "accept-language":   {w.task.perimeterX.GetAcceptLanguage()},
  }

  return &http.Request{
    Method: "POST",
    Header: h,
    URL:    util.MustParseUrl("https://www.task.walmart.com/api/checkout/v3/contract?page=CHECKOUT_VIEW"),
  }
}

func (w *walmartDesktopApi) createCheckoutStep2Request() *http.Request {
  h := http.Header{
    //"content-length": {"DEFAULT_VALUE"},
    "accept":            {"application/json, text/javascript, */*; q=0.01"},
    "inkiru_precedence": {"false"},
    "wm_cvv_in_session": {"true"},
    "user-agent":        {w.task.perimeterX.GetUserAgent()},
    "wm_vertical_id":    {"0"},
    "content-type":      {"application/json"},
    "origin":            {"https://www.task.walmart.com"},
    "sec-fetch-site":    {"same-origin"},
    "sec-fetch-mode":    {"cors"},
    "sec-fetch-dest":    {"empty"},
    "referer":           {"https://www.task.walmart.com/checkout/"},
    "accept-encoding":   {"gzip, deflate, br"},
    "accept-language":   {w.task.perimeterX.GetAcceptLanguage()},
  }

  return &http.Request{
    Header: h,
    Method: "POST",
    URL:    util.MustParseUrl("https://www.task.walmart.com/api/checkout/v3/contract/:PCID/fulfillment"),
  }
}

func (w *walmartDesktopApi) createCheckoutStep3Request() *http.Request {
  h := http.Header{
    "content-length":    {"DEFAULT_VALUE"},
    "accept":            {"application/json, text/javascript, */*; q=0.01"},
    "inkiru_precedence": {"false"},
    "wm_cvv_in_session": {"true"},
    "user-agent":        {w.task.perimeterX.GetUserAgent()},
    "wm_vertical_id":    {"0"},
    "content-type":      {"application/json"},
    "origin":            {"https://www.task.walmart.com"},
    "sec-fetch-site":    {"same-origin"},
    "sec-fetch-mode":    {"cors"},
    "sec-fetch-dest":    {"empty"},
    "referer":           {"https://www.task.walmart.com/checkout/"},
    "accept-encoding":   {"gzip, deflate, br"},
    "accept-language":   {w.task.perimeterX.GetAcceptLanguage()},
  }

  return &http.Request{
    Header: h,
    Method: "POST",
    URL:    util.MustParseUrl("https://www.task.walmart.com/api/checkout/v3/contract/:PCID/shipping-address"),
  }
}

func (w *walmartDesktopApi) createOrderRequestForPayment(token *PaymentToken) *http.Request {
  panic("implement me")
}