package walmart

import (
  "errors"
  "fmt"
  "github.com/CenturionLabs/centurion/checkout-service/contracts/monitor"
  "github.com/CenturionLabs/centurion/checkout-service/core"
  "github.com/CenturionLabs/centurion/checkout-service/services"
  "github.com/CenturionLabs/centurion/checkout-service/sites/walmart/perimeterx"
  "github.com/CenturionLabs/centurion/checkout-service/util"
  jsoniter "github.com/json-iterator/go"
  log "github.com/sirupsen/logrus"
  http "github.com/useflyent/fhttp"
  "github.com/useflyent/fhttp/cookiejar"
  "go.elastic.co/apm"
  "io"
  "io/ioutil"
  "math/rand"
  "net/url"
  "strconv"
  "strings"
  "time"
)

type walmartMode int32

const (
	DESKTOP walmartMode = 0
	OTHER   walmartMode = iota
)

const (
	TracingSpanName = "checkout"
)

type walmartTask struct {
	f_String_1      string
	offerId         string
	keywords        []string
	cookies         *cookiejar.Jar
	isActive        bool
	isZipLocated    bool
	taskId          int32
	profile         *Profile
	account         *Account
	paymentToken    *PaymentToken
	paymentInstance *PaymentInstance

	// possibly random referer
	referer              string
	modifiedSince        *string
	f_String_c           *string
	possiblySomeProdPath string
	storeIds             []string
	perimeterX           perimeterx.PerimeterX
	mode                 walmartMode
	taskMode             string
	dstrLock             services.DistributedLockFactory
	payload              *core.CheckoutPayload
	http                 services.HttpClient
	step                 int

	monitorClient monitor.MonitorClient
	api           walmartApi
	timestamp     time.Time
	f_boolean_0   bool
	retryDelay    time.Duration
	monitorDelay  time.Duration

	USItemId string
}

type walmartApi interface {
	initializePxSessionReceiveCookies() error
	GetCookies() []*http.Cookie
	createCreateAccountPayload() interface{}
	createSignUpRequest() *http.Request
	createSignInAccountJson(account *Account) interface{}
	createSignInRequest() *http.Request
	createHomePageRequest() *http.Request
	createAddToCartRequestByMethod(method string) *http.Request
	createAddToCartJson() interface{}
	createInitCartRequest() *http.Request
	createEnsureInStockRequest(uxItemId string, fullProductRoute bool) *http.Request
	createCartCrtRequest(crt string) *http.Request
	createCheckoutStep1Json() interface{}
	createCheckoutStep1Request(token *PaymentToken) *http.Request
	createCheckoutStep2Json(itemId, shipMethod string) interface{}
	createCheckoutStep2Request() *http.Request
	createCheckoutStep3Json(step2 *selectShippingResult) interface{}
	createCheckoutStep3Request() *http.Request
	createCheckoutStep4Json() interface{}
	createCheckoutStep4Request() *http.Request
	createSubmitPaymentJson(token *PaymentToken) interface{}
	createPreloadCartJson(offerId string, quantity int) map[string]interface{}
	createProcessPaymentJson(token *PaymentToken) interface{}
	createOrderRequestForPayment(token *PaymentToken) *http.Request
}

func (t *walmartTask) getRootCookieHost() *url.URL {
	return util.MustParseUrl(".walmart.com")
}

func (t *walmartTask) hasCookie(key string, host *url.URL) bool {
	c := t.cookies.Cookies(host)
	for _, c := range c {
		if c.Name == key {
			return true
		}
	}

	return false
}

func (t *walmartTask) getCookiesStr() string {
	var cookies = t.cookies.Cookies(t.getRootCookieHost())
	b := strings.Builder{}
	for ix, _ := range cookies {
		c := cookies[ix]
		b.WriteString(fmt.Sprintf("%s=%s\n", c.Name, c.Value))
	}

	return b.String()
}

func (t *walmartTask) executeWmpieGetKeyAndSetPaymentToken() error {
	requestUrl := "https://securedataweb.walmart.com/pie/v1/wmcom_us_vtg_pie/getkey.js?bust=" + strconv.FormatInt(time.Now().Unix(), 10)
	// todo: retry 5 times
	// todo: timeout 10 sec

	req := &http.Request{
		URL: util.MustParseUrl(requestUrl),
	}
	resp, err := t.http.Do(req)
	if err != nil {
		t.payload.ReportUnexpectedFailure(err)
	}

	if resp.StatusCode != http.StatusOK {
		// todo: retry here
		return errors.New(resp.Status)
	}

	keyBytes, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return err
	}

	key := string(keyBytes)
	t.paymentToken = createPaymentToken(key, t.profile.CardNumber, t.profile.Cvv)
	t.paymentToken.encryptedPan = encryptPan(key, "4111111111111111", t.profile.Cvv)

	return nil
}

func (t *walmartTask) clearCookies() {
	jar, _ := cookiejar.New(nil)
	t.cookies = jar
	t.http.UseCookieJar(jar)
}

func (t *walmartTask) setRandomProxy() error {
	span, _ := apm.StartSpan(t.payload.Context, "setRandomProxy", TracingSpanName)
	defer span.End()
	if t.payload.Task.ProxyPool == nil {
		return nil
	}

	proxyList, err := util.GetProxyUrls(t.payload.Task.ProxyPool)
	if err != nil {
		return err
	}

	rndProxy, err := util.RandomUrl(proxyList)
	if err != nil {
		return err
	}

	t.http.ChangeProxy(rndProxy)
	return nil
}

func (t *walmartTask) checkoutTask() *core.StepExecutionFailure {
	if strings.Contains(t.taskMode, "login") {
		if err := t.createAccount(); err != nil {
			return t.payload.ReportUnexpectedError(err)
		}
	} else if strings.Contains(t.taskMode, "account") {
		count, err := t.loginToAccount()
		if err != nil {
			return t.payload.ReportUnexpectedFailure(err)
		}

		if count == 0 {
			//this.logger.info("No accounts available in storage. Creating new...");
			err = t.createAccount()
			if err != nil {
				return t.payload.ReportUnexpectedError(err)
			}
		}
	}

	if t.isActive {
		diff := int64(time.Now().UTC().Sub(t.timestamp))
		bounds := rand.Int63n(300_000-120_000) + 120_000
		if diff >= bounds {
			// todo: double check how works `setCollectedCookies`
			if err := t.setCollectedCookies(); err != nil {
				return t.payload.ReportUnexpectedFailure(err)
			}
		}
	}

	step := 0
	addToCartMode := 0
	int3 := 0
	if step == 0 {
		if t.mode == DESKTOP {
			if err := t.checkHomePage(true); err != nil {
				return t.payload.ReportUnexpectedFailure(err)
			}
		}

		if addToCartMode != 0 {
			if err := t.addToCart(); err != nil {
				return t.payload.ReportUnexpectedFailure(err)
			}
		} else {
			if err := t.addToCartAlternate(); err != nil {
				return t.payload.ReportUnexpectedFailure(err)
			}
		}
	} else if int3 != 0 {
		isStock, err := t.ensureInStock()
		if err != nil {
			return t.payload.ReportUnexpectedFailure(err)
		}

		if !isStock {
			// todo: retry
		}
	}

	if err := t.checkoutAndGetPCID(); err != nil {
		return t.payload.ReportUnexpectedFailure(err)
	}

	return nil
}

func (t *walmartTask) createAccount() error {
	createAccPayload := t.api.createCreateAccountPayload()
	createAccPayloadJson, err := jsoniter.Marshal(createAccPayload)
	if err != nil {
		return err
	}

	ix := 0
	for {
		if t.isActive && ix <= 500 {
			ix++
			createAccRequest := t.api.createSignUpRequest()
			createAccRequest.Body = ioutil.NopCloser(strings.NewReader(string(createAccPayloadJson)))
			signUpResponse, err := t.http.Do(createAccRequest)
			if err != nil {
				return err
			}

			if signUpResponse.StatusCode != http.StatusOK &&
				signUpResponse.StatusCode != http.StatusCreated &&
				signUpResponse.StatusCode != http.StatusPartialContent {
				log.Printf("Creating account: status: '%v'\n", signUpResponse.StatusCode)

				if signUpResponse.StatusCode != http.StatusPreconditionFailed {
					t.f_boolean_0 = false
				} else if ix%10 == 0 && t.mode == DESKTOP && strings.Contains(t.taskMode, "skip") {
					t.perimeterX.Reset()
				}

				solved, err := t.perimeterX.EnsureResponsePxProtectionSolved(signUpResponse)
				if err != nil {
					return err
				}

				if !solved || t.f_boolean_0 {
					time.Sleep(t.retryDelay)
					// retry
				}

				t.f_boolean_0 = signUpResponse.StatusCode == http.StatusPreconditionFailed
			}

			rawAccountJson, err := ioutil.ReadAll(signUpResponse.Body)
			if err != nil {
				return err
			}

			email := jsoniter.Get(rawAccountJson, "email").ToString()
			t.account.Email = email
			return nil
		}

		return errors.New("FATAL: Failed")
	}
}

func (t *walmartTask) loginToAccount() (int32, error) {
	if t.account == nil {
		return 0, errors.New("No accounts available...")
	}

	ix := 0
	for {
		ix++
		if !t.isActive || ix > 500 {
			return -1, errors.New("Failed to login to account. Max retries exceeded...")
		}

		signInJson := t.api.createSignInAccountJson(t.account)
		signInRequest := t.api.createSignInRequest()
		signInRequest.GetBody = func() (io.ReadCloser, error) {
			json, err := jsoniter.MarshalToString(signInJson)
			if err != nil {
				return nil, err
			}

			return ioutil.NopCloser(strings.NewReader(json)), nil
		}

		resp, err := t.http.Do(signInRequest)
		if err != nil {
			return -1, err
		}

		if resp.StatusCode != http.StatusOK &&
			resp.StatusCode != http.StatusPartialContent &&
			resp.StatusCode != http.StatusCreated {
			log.Printf("Account login: status: '%v'\n", resp.StatusCode)

			if resp.StatusCode != http.StatusPreconditionFailed {
				t.f_boolean_0 = false
			} else if ix%10 == 0 && t.mode == DESKTOP && strings.Contains(t.taskMode, "skip") {
				t.perimeterX.Reset()
			}

			solved, err := t.perimeterX.EnsureResponsePxProtectionSolved(resp)
			if err != nil {
				return -1, err
			}

			if !solved || t.f_boolean_0 {
				time.Sleep(t.retryDelay)
			}

			t.f_boolean_0 = resp.StatusCode == http.StatusPreconditionFailed
		}

		// todo: why do we do this?
		t.profile.Email = t.account.Email
		return 1, nil
	}
}

func (t *walmartTask) checkHomePage(boolean1 bool) error {
	ix := 0
	for ix++; t.isActive && ix < 100; {
		homePageRequest := t.api.createHomePageRequest()
		resp, err := t.http.Do(homePageRequest)
		if err != nil {
			return err
		}

		if resp.StatusCode == http.StatusTemporaryRedirect {
			if _, err := t.perimeterX.EnsureResponsePxProtectionSolved(resp); err != nil {
				return err
			}
		} else {
			if resp.StatusCode != http.StatusOK {
				log.Printf("Failed visiting homepage: status: '%v'\n", resp.StatusCode)
				time.Sleep(t.monitorDelay)
				continue
			}

			host := util.MustParseUrl("www.walmart.com")

			prodId := strconv.FormatInt(int64(mustConvInt(t.keywords[0])), 16)
			t.cookies.SetCookies(host, []*http.Cookie{
				{Name: "viq", Value: "walmart"},
				{Name: "cart-item-count", Value: "0"},
				{Name: "_uetsid", Value: utils_getStringc()},
				{Name: "_uetvid", Value: utils_getStringc()},
				{Name: "s_sess_2", Value: "prop32%3D"},
				{Name: "TBV", Value: "7"},
				{Name: "TB_DC_Flap_Test", Value: "0"},
				{Name: "TB_SFOU-10", Value: "0"},
				{Name: "athrvi", Value: "RVI~h" + prodId},
				{Name: "_gcl_au", Value: "1.1.1957667684." + strconv.FormatInt(time.Now().Unix()/1000, 10)},
			})

			return nil
		}
	}

	return errors.New("failed to visit home page")
}

func (t *walmartTask) addToCart() error {
	addToCartMethod := "addToCart"
	if rand.Uint32()%2 == 0 {
		addToCartMethod = "buynow"
	}

	ix := 0
	for ix++; t.isActive && ix <= 50; {
		req := t.api.createAddToCartRequestByMethod(addToCartMethod)
		resp, err := t.http.Do(req)
		if err != nil {
			return err
		}

		if resp.StatusCode == http.StatusFound {
			crt := ""
			err = errors.New("crt not found")
			for i := range resp.Cookies() {
				c := resp.Cookies()[i]
				if c.Name == "CRT" {
					err = nil
					crt = c.Value
				}
			}

			if err != nil {
				return err
			}

			// todo: share via distributed lock CRT value
			log.Printf("CRT: %s\n", crt)
			return nil
		}

		if _, err := t.perimeterX.EnsureResponsePxProtectionSolved(resp); err != nil {
			return err
		}
	}

	return errors.New("failed to add to cart")
}

func (t *walmartTask) addToCartAlternate() error {
	addToCartJson := t.api.createAddToCartJson()
	getBodyFn := func() (io.ReadCloser, error) {
		json, err := jsoniter.MarshalToString(addToCartJson)
		if err != nil {
			return nil, err
		}

		return ioutil.NopCloser(strings.NewReader(json)), nil
	}
	ix := 0
	for ix++; t.isActive && ix <= 50; {
		req := t.api.createInitCartRequest()
		req.GetBody = getBodyFn
		resp, err := t.http.Do(req)
		if err != nil {
			return err
		}

		if resp.StatusCode != http.StatusCreated &&
			resp.StatusCode != http.StatusOK &&
			resp.StatusCode != http.StatusPartialContent {

			if resp.StatusCode != http.StatusPreconditionFailed {
				t.f_boolean_0 = false
			} else if ix%10 == 0 && t.mode == DESKTOP && strings.Contains(t.taskMode, "skip") {
				t.perimeterX.Reset()
			}

			if _, err = ioutil.ReadAll(resp.Body); err != nil {
				return err
			}

			solved, err := t.perimeterX.EnsureResponsePxProtectionSolved(resp)
			if err != nil {
				return err
			}

			if !solved || t.f_boolean_0 {
				time.Sleep(t.monitorDelay)
			}

			t.f_boolean_0 = resp.StatusCode == http.StatusPreconditionFailed
		}

		crt := ""
		err = errors.New("crt not found")
		for i := range resp.Cookies() {
			c := resp.Cookies()[i]
			if c.Name == "CRT" {
				err = nil
				crt = c.Value
			}
		}

		if err != nil {
			return err
		}

		// todo: share via distributed lock CRT value
		log.Printf("CRT: %s\n", crt)
		return nil
	}

	return errors.New("failed to add to cart")
}

func (t *walmartTask) ensureInStock() (bool, error) {
	crt, err := t.getCookieValue("CRT")
	if err != nil {
		return false, err
	}

	ix := 0
	for ix++; t.isActive && ix <= 30; {
		isInStock := false
		if ix%2 == 0 && len(t.USItemId) > 0 {
			isInStock, err = t.ensureInStockViaUSItemId(t.USItemId)
			if err != nil {
				return false, err
			}
		} else {
			isInStock, err = t.ensureInStockViaAddToCart(crt)
			if err != nil {
				return false, err
			}
		}

		if isInStock {
			return true, nil
		}
	}

	return false, errors.New("OOS")
}

func (t *walmartTask) checkoutAndGetPCID() error {
	r, err := t.paymentCheckoutAndGetPCID()
	if err != nil {
		return err
	}

	if r == -1 {
		return errors.New("FATAL: failed to process")
	}

	return nil
}

func (t *walmartTask) getCookieValue(name string) (string, error) {
	cookies := t.cookies.Cookies(t.getRootCookieHost())
	for i := range cookies {
		c := cookies[i]
		if c.Name == name {
			return c.Value, nil
		}
	}

	return "", errors.New("Can't find cookie " + name)
}

func (t *walmartTask) ensureInStockViaUSItemId(usItemId string) (bool, error) {
	type instockInfo struct {
		data *struct {
			productByProductId *struct {
				offerList []*struct {
					id                  string
					productAvailability string
					offerInfo           *struct {
						offerType string
					}
				}
			}
		}
	}

	json := "{\"variables\":\"{\\\"casperSlots\\\":{\\\"fulfillmentType\\\":\\\"ACC\\\",\\\"reservationType\\\":\\\"SLOTS\\\"}," +
		"\\\"postalAddress\\\":{\\\"addressType\\\":\\\"RESIDENTIAL\\\",\\\"countryCode\\\":\\\"USA\\\",\\\"postalCode\\\":\\\"" + t.profile.PostalCode +
		"\\\",\\\"stateOrProvinceCode\\\":\\\"" + t.profile.State + "\\\",\\\"zipLocated\\\":true}," +
		"\\\"storeFrontIds\\\":[{\\\"distance\\\":2.24,\\\"inStore\\\":false,\\\"preferred\\\":false,\\\"storeId\\\":\\\"91672\\\"," +
		"\\\"storeUUID\\\":null,\\\"usStoreId\\\":91672},{\\\"distance\\\":3.04,\\\"inStore\\\":false,\\\"preferred\\\":false," +
		"\\\"storeId\\\":\\\"5936\\\",\\\"storeUUID\\\":null,\\\"usStoreId\\\":5936},{\\\"distance\\\":3.31,\\\"inStore\\\":false," +
		"\\\"preferred\\\":false,\\\"storeId\\\":\\\"90563\\\",\\\"storeUUID\\\":null,\\\"usStoreId\\\":90563}," +
		"{\\\"distance\\\":3.41,\\\"inStore\\\":false,\\\"preferred\\\":false,\\\"storeId\\\":\\\"91675\\\",\\\"storeUUID\\\":null," +
		"\\\"usStoreId\\\":91675},{\\\"distance\\\":5.58,\\\"inStore\\\":false,\\\"preferred\\\":false,\\\"storeId\\\":\\\"91121\\\"," +
		"\\\"storeUUID\\\":null,\\\"usStoreId\\\":91121}],\\\"productId\\\":\\\"" + usItemId + "\\\",\\\"selected\\\":false}\"}"
	var getReqBody = func() (io.ReadCloser, error) {
		return ioutil.NopCloser(strings.NewReader(json)), nil
	}

	var attempt = 0
	for attempt++; attempt < 5; {
		var req = t.api.createEnsureInStockRequest(usItemId, false)
		req.GetBody = getReqBody
		var resp, err = t.http.Do(req)
		if err != nil {
			return false, err
		}

		if resp.StatusCode != http.StatusOK &&
			resp.StatusCode != http.StatusCreated &&
			resp.StatusCode != http.StatusPartialContent {

			_, err := t.perimeterX.EnsureResponsePxProtectionSolved(resp)
			if err != nil {
				return false, err
			}

			continue
		}

		rawJson, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			return false, err
		}

		info := new(instockInfo)
		err = jsoniter.Unmarshal(rawJson, info)
		if err != nil {
			return false, nil
		}

		if info.data == nil || info.data.productByProductId == nil || len(info.data.productByProductId.offerList) == 0 {
			continue
		}

		offerList := info.data.productByProductId.offerList
		for i := range offerList {
			item := offerList[i]
			if item.id != t.payload.Task.Product.Sku {
				continue
			}

			if strings.ToUpper(item.productAvailability) != "IN_STOCK" {
				// todo: share CRT
				crt := ""
				if crt, err = t.getCookieValue("CRT"); err != nil {
					return false, err
				}

				log.Printf("CRT: %s\n", crt)
				return true, nil
			}

			if item.offerInfo == nil {
				continue
			}

			if strings.Contains(strings.ToUpper(item.offerInfo.offerType), "ONLINE") {
				return false, nil
			}

			// todo: share CRT
			crt := ""
			if crt, err = t.getCookieValue("CRT"); err != nil {
				return false, err
			}

			log.Printf("CRT: %s\n", crt)
			return true, nil
		}
	}

	return false, errors.New("OOS")
}

func (t *walmartTask) ensureInStockViaAddToCart(crt string) (bool, error) {
	type cartResp struct {
		checkoutable bool
		items        []*struct {
			offerId  string
			USItemId string
		}
	}
	var attempt = 0
	for attempt++; attempt < 5; {
		cartCrtRequest := t.api.createCartCrtRequest(crt)
		resp, err := t.http.Do(cartCrtRequest)
		if err != nil {
			return false, nil
		}

		if resp.StatusCode != http.StatusOK &&
			resp.StatusCode != http.StatusCreated &&
			resp.StatusCode != http.StatusPartialContent {
			_, err := t.perimeterX.EnsureResponsePxProtectionSolved(resp)
			if err != nil {
				return false, err
			}

			continue
		}

		rawjson, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			return false, err
		}

		json := new(cartResp)
		err = jsoniter.Unmarshal(rawjson, json)
		if err != nil {
			return false, err
		}

		if json.checkoutable {
			return true, nil
		}

		if len(json.items) == 0 {
			t.removeCookieByName("hasCRT")
			t.removeCookieByName("CRT")
			return false, errors.New("FATAL: CRT expired or empty")
		}

		for i := range json.items {
			item := json.items[i]
			if item.offerId == t.offerId {
				t.USItemId = item.USItemId
				break
			}
		}
	}

	return false, errors.New("OOS")
}

func (t *walmartTask) paymentCheckoutAndGetPCID() (int32, error) {
	t.offerId = t.keywords[0]
	priceLimit, err := strconv.Atoi(t.keywords[1])
	if err != nil {
		return -1, err
	}

	step1Json, err := t.checkoutStep1()
	if err != nil {
		return -1, err
	}

	if step1Json.items[0].unitPrice > float64(priceLimit) {
		return -1, errors.New("FATAL: price exceeds limit of $" + strconv.Itoa(priceLimit))
	}

	if t.paymentInstance.mobilegrief {
	}

	item := step1Json.items[0]
	shippingResult, err := t.selectShipping(item.id, item.fulfillmentSelection.shipMethod)
	if err != nil {
		return -1, err
	}

	/*step3Result*/
	_, err = t.submitShipping(shippingResult)
	if err != nil {
		return -1, err
	}

	_ /*paymentResult*/, err = t.submitPayment()
	if err != nil {
		return -1, err
	}

	return t.processPaymentWithDefaults()
}

type step1Result struct {
	items []*struct {
		id                   string
		unitPrice            float64
		fulfillmentSelection *struct {
			shipMethod string
		}
	}
}

type selectShippingResult struct {
}

type submitShippingResult struct {
}

type submitBillingResult struct {
	piHash string
}

type submitPaymentResult struct {
}

func (t *walmartTask) generateCheckout() (string, error) {
	t.offerId = t.keywords[0]
	priceLimit, err := strconv.Atoi(t.keywords[1])
	if err != nil {
		return "", err
	}

	err = t.preloadCart()
	if err != nil {
		return "", err
	}

	step1Json, err := t.checkoutStep1()
	if err != nil {
		return "", err
	}

	if step1Json.items[0].unitPrice > float64(priceLimit) {
		return "", errors.New("FATAL: price exceeds limit of $" + strconv.Itoa(priceLimit))
	}

	if t.paymentInstance.mobilegrief {
	}

	item := step1Json.items[0]
	shippingResult, err := t.selectShipping(item.id, item.fulfillmentSelection.shipMethod)
	if err != nil {
		return "", err
	}

	/*step3Result*/
	_, err = t.submitShipping(shippingResult)
	if err != nil {
		return "", err
	}

	_ /*paymentResult*/, err = t.submitPayment()
	if err != nil {
		return "", err
	}

	billingResult, err := t.submitBilling()
	if err != nil {
		return "", err
	}

	return billingResult.piHash, nil
}

func (t *walmartTask) checkoutStep1() (*step1Result, error) {
	step1Json := t.api.createCheckoutStep1Json()
	var attempt = 0
	for attempt++; attempt < 5; {
		req := t.api.createCheckoutStep1Request(t.paymentToken)
		req.GetBody = func() (io.ReadCloser, error) {
			jsonStr, err := jsoniter.MarshalToString(step1Json)
			if err != nil {
				return nil, err
			}

			return ioutil.NopCloser(strings.NewReader(jsonStr)), nil
		}

		resp, err := t.http.Do(req)
		if err != nil {
			return nil, err
		}

		if resp.StatusCode == http.StatusCreated {
			// todo: share with distributed lock offerId

			rawbody, err := ioutil.ReadAll(resp.Body)
			if err != nil {
				return nil, err
			}

			result := new(step1Result)
			err = jsoniter.Unmarshal(rawbody, result)
			if err != nil {
				return nil, err
			}

			return result, nil
		}

		if resp.StatusCode == http.StatusMethodNotAllowed {
			return nil, errors.New("FATAL: 405 error. Restarting session...")
		}

		body, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			return nil, err
		}

		if t.paymentInstance.isFast__probably && !strings.Contains(string(body), "cart_empty") {
			_, err := t.perimeterX.EnsureResponsePxProtectionSolved(resp)
			if err != nil {
				return nil, err
			}

			if resp.StatusCode != http.StatusPreconditionFailed && resp.StatusCode != http.StatusTemporaryRedirect {
				time.Sleep(t.retryDelay)
			}

			continue
		}
	}

	return nil, errors.New("FATAL: Failed to generate checkout step #1")
}

func (t *walmartTask) selectShipping(itemId, shipMethod string) (*selectShippingResult, error) {
	step2Json := t.api.createCheckoutStep2Json(itemId, shipMethod)
	var attempt = 0
	for attempt++; attempt < 5; {
		req := t.api.createCheckoutStep2Request()
		req.GetBody = func() (io.ReadCloser, error) {
			jsonStr, err := jsoniter.MarshalToString(step2Json)
			if err != nil {
				return nil, err
			}

			return ioutil.NopCloser(strings.NewReader(jsonStr)), nil
		}

		resp, err := t.http.Do(req)
		if err != nil {
			return nil, err
		}

		if resp.StatusCode == http.StatusOK {
			b, err := ioutil.ReadAll(resp.Body)
			if err != nil {
				return nil, err
			}

			result := new(selectShippingResult)
			err = jsoniter.Unmarshal(b, result)
			if err != nil {
				return nil, err
			}

			return result, nil
		}

		if !t.paymentInstance.isFast__probably {
			return nil, errors.New("FATAL: Failed to generate checkout step #2: status: " + strconv.Itoa(resp.StatusCode))
		}

		_, err = t.perimeterX.EnsureResponsePxProtectionSolved(resp)
		if err != nil {
			return nil, err
		}

		if !t.paymentInstance.mobilegrief &&
			resp.StatusCode != http.StatusPreconditionFailed &&
			resp.StatusCode != http.StatusTemporaryRedirect {
			time.Sleep(t.retryDelay)
		}
	}

	return nil, errors.New("FATAL: Failed to generate checkout step #2")
}

func (t *walmartTask) submitShipping(prev *selectShippingResult) (*submitShippingResult, error) {
	step3Json := t.api.createCheckoutStep3Json(prev)
	var attempt = 0
	for attempt++; attempt < 5; {
		req := t.api.createCheckoutStep3Request()
		req.GetBody = func() (io.ReadCloser, error) {
			jsonStr, err := jsoniter.MarshalToString(step3Json)
			if err != nil {
				return nil, err
			}

			return ioutil.NopCloser(strings.NewReader(jsonStr)), nil
		}

		resp, err := t.http.Do(req)
		if err != nil {
			return nil, err
		}

		if resp.StatusCode == http.StatusOK {
			b, err := ioutil.ReadAll(resp.Body)
			if err != nil {
				return nil, err
			}

			result := new(submitShippingResult)
			err = jsoniter.Unmarshal(b, result)
			if err != nil {
				return nil, err
			}

			return result, nil
		}

		if !t.paymentInstance.isFast__probably {
			return nil, errors.New("FATAL: Failed to generate checkout step #3: status: " + strconv.Itoa(resp.StatusCode))
		}

		_, err = t.perimeterX.EnsureResponsePxProtectionSolved(resp)
		if err != nil {
			return nil, err
		}

		if !t.paymentInstance.mobilegrief &&
			resp.StatusCode != http.StatusPreconditionFailed &&
			resp.StatusCode != http.StatusTemporaryRedirect {
			time.Sleep(t.retryDelay)
		}
	}

	return nil, errors.New("FATAL: Failed to generate checkout step #3")
}

func (t *walmartTask) submitBilling() (*submitBillingResult, error) {
	step4Json := t.api.createCheckoutStep4Json()
	var attempt = 0
	for attempt++; attempt < 5; {
		req := t.api.createCheckoutStep4Request()
		req.GetBody = func() (io.ReadCloser, error) {
			jsonStr, err := jsoniter.MarshalToString(step4Json)
			if err != nil {
				return nil, err
			}

			return ioutil.NopCloser(strings.NewReader(jsonStr)), nil
		}

		resp, err := t.http.Do(req)
		if err != nil {
			return nil, err
		}

		if resp.StatusCode == http.StatusOK {
			b, err := ioutil.ReadAll(resp.Body)
			if err != nil {
				return nil, err
			}

			result := new(submitBillingResult)
			err = jsoniter.Unmarshal(b, result)
			if err != nil {
				return nil, err
			}

			return result, nil
		}

		if !t.paymentInstance.isFast__probably {
			return nil, errors.New("FATAL: Failed to generate checkout step #4: status: " + strconv.Itoa(resp.StatusCode))
		}

		_, err = t.perimeterX.EnsureResponsePxProtectionSolved(resp)
		if err != nil {
			return nil, err
		}

		if !t.paymentInstance.mobilegrief &&
			resp.StatusCode != http.StatusPreconditionFailed &&
			resp.StatusCode != http.StatusTemporaryRedirect {
			time.Sleep(t.retryDelay)
		}
	}

	return nil, errors.New("FATAL: Failed to generate checkout step #4")
}

func (t *walmartTask) submitPayment() (*submitPaymentResult, error) {
	paymentJson := t.api.createSubmitPaymentJson(t.paymentToken)
	var attempt = 0
	for attempt++; attempt < 5; {
		req := t.api.createCheckoutStep4Request()
		req.GetBody = func() (io.ReadCloser, error) {
			jsonStr, err := jsoniter.MarshalToString(paymentJson)
			if err != nil {
				return nil, err
			}

			return ioutil.NopCloser(strings.NewReader(jsonStr)), nil
		}

		resp, err := t.http.Do(req)
		if err != nil {
			return nil, err
		}

		if resp.StatusCode == http.StatusOK {
			b, err := ioutil.ReadAll(resp.Body)
			if err != nil {
				return nil, err
			}

			result := new(submitPaymentResult)
			err = jsoniter.Unmarshal(b, result)
			if err != nil {
				return nil, err
			}

			return result, nil
		}

		if !t.paymentInstance.isFast__probably {
			return nil, errors.New("Failed to submit payment: status: " + strconv.Itoa(resp.StatusCode))
		}

		_, err = t.perimeterX.EnsureResponsePxProtectionSolved(resp)
		if err != nil {
			return nil, err
		}

		if !t.paymentInstance.mobilegrief &&
			resp.StatusCode != http.StatusPreconditionFailed &&
			resp.StatusCode != http.StatusTemporaryRedirect {
			time.Sleep(t.retryDelay)
		}
	}

	return nil, errors.New("FATAL: Failed to submit payment")
}

func (t *walmartTask) processPaymentWithDefaults() (int32, error) {
	return t.processPayment(true)
}

func (t *walmartTask) preloadCart() error {
	json := t.api.createPreloadCartJson("47CDCD2F7ED24EC2BA9F74BEAE3C151B", 1)
	json["unitPrice"] = 5

	var attempt = 0
	for attempt++; attempt < 3; {
		req := t.api.createInitCartRequest()
		req.GetBody = func() (io.ReadCloser, error) {
			jsonStr, err := jsoniter.MarshalToString(json)
			if err != nil {
				return nil, err
			}

			return ioutil.NopCloser(strings.NewReader(jsonStr)), nil
		}

		resp, err := t.http.Do(req)
		if err != nil {
			return err
		}

		if resp.StatusCode == http.StatusOK {
			_, err := ioutil.ReadAll(resp.Body)
			if err != nil {
				return err
			}

			return nil
		}

		_, err = t.perimeterX.EnsureResponsePxProtectionSolved(resp)
		if err != nil {
			return err
		}

		time.Sleep(t.monitorDelay)
	}

	return errors.New("FATAL: Failed to cart preload")
}

func (t *walmartTask) processPayment(boolean1 bool) (int32, error) {
	attempt := 47
	if boolean1 {
		attempt = 0
	}

	int3 := 0
	json := t.api.createProcessPaymentJson(t.paymentToken)
	for attempt++; attempt < 50; {
		req := t.api.createOrderRequestForPayment(t.paymentToken)
		req.GetBody = func() (io.ReadCloser, error) {
			jsonStr, err := jsoniter.MarshalToString(json)
			if err != nil {
				return nil, err
			}

			return ioutil.NopCloser(strings.NewReader(jsonStr)), nil
		}

		resp, err := t.http.Do(req)
		if err != nil {
			return -1, err
		}

		b, err := ioutil.ReadAll(resp.Body)
		if err != nil {
			return -1, err
		}

		bodyStr := string(b)
		if resp.StatusCode == http.StatusOK {
			if strings.Contains(bodyStr, "orderid") {
				if int3 != 0 {
					// release shared lock
				}

				return 200, nil
			}

			return -1, errors.New("Something went wrong while processing: status - " + strconv.Itoa(resp.StatusCode))
		}

		if strings.Contains(bodyStr, "missing") {
			log.Println("Missing payment info. Re-submitting...")
			if t.paymentInstance.mobilegriefalt && attempt <= 15 || t.paymentInstance.mobilegrief && attempt <= 8 {
				continue
			}
		} else if strings.Contains(bodyStr, "different payment") {
			log.Printf("Card Decline[Invalid/FailedCharge] with status '%d'. Retrying...\n", resp.StatusCode)
			if t.paymentInstance.mobilegrief && attempt <= 3 {
				continue
			}

			log.Printf("Card decline (FailedCharge)")
		} else if strings.Contains(bodyStr, "different card") {
			log.Printf("Card Decline[FailedCharge] with status '%d'. Retrying...\n", resp.StatusCode)
			if t.paymentInstance.mobilegrief && attempt <= 3 {
				continue
			}

			log.Printf("Card decline (FailedCharge)")
		} else if strings.Contains(bodyStr, "stock") {
			int3 = 1
			log.Printf("OOS on checkout with status '%d'. Retrying...\n", resp.StatusCode)
			if t.paymentInstance.mobilegrief && attempt <= 3 {
				continue
			}

			log.Printf("Out Of Stock")
		} else if resp.StatusCode != http.StatusMethodNotAllowed {
			if !strings.Contains(bodyStr, "contract has expired") {
				logErrorStatus(resp.StatusCode)
				_, err = t.perimeterX.EnsureResponsePxProtectionSolved(resp)
				if err != nil {
					return -1, err
				}
			}

			return -1, errors.New(fmt.Sprintf("Cart has expired with status '%d'. Re-submitting.\n", resp.StatusCode))
		}

		if resp.StatusCode == http.StatusPreconditionFailed {
			continue
		}

		// todo: release distributed lock
		time.Sleep(t.retryDelay)
	}

	return -1, errors.New("FATAL: Failed to process")
}

func logErrorStatus(statusCode int) {
	switch statusCode {
	case 307:
		log.Println("PX Block with status:'307'. Retrying...")
		break
	case 412:
		log.Println("PX Block with status:'412'. Retrying...")
		break
	case 444:
		log.Println("Failed to execute due to status '444': PROXY_BAN")
		break
	default:
		log.Printf("Failed to execute due to status '%d'\n", statusCode)
	}
}

func (t *walmartTask) preloadSession() error {
	panic(" not implemented")
}
