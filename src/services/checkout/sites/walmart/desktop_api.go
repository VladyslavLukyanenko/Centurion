package walmart

import (
  "github.com/CenturionLabs/centurion/checkout-service/util"
  http "github.com/useflyent/fhttp"
  "math/rand"
  "strconv"
  "strings"
)

type walmartDesktopApi struct {
  task *walmartTask
}

func (w *walmartDesktopApi) createCheckoutStep4Request() *http.Request {
  h := http.Header{
    //"content-length": {"DEFAULT_VALUE"},
    "accept":          {"application/json"},
    "user-agent":      {w.task.perimeterX.GetUserAgent()},
    "content-type":    {"application/json"},
    "origin":          {"https://www.task.walmart.com"},
    "sec-fetch-site":  {"same-origin"},
    "sec-fetch-mode":  {"cors"},
    "sec-fetch-dest":  {"empty"},
    "referer":         {"https://www.task.walmart.com/checkout/"},
    "accept-encoding": {"gzip, deflate, br"},
    "accept-language": {w.task.perimeterX.GetAcceptLanguage()},
  }

  return &http.Request{
    Header: h,
    Method: "POST",
    URL:    util.MustParseUrl("https://www.task.walmart.com/api/checkout-customer/:CID/credit-card"),
  }
}

func (w *walmartDesktopApi) initializePxSessionReceiveCookies() error {
  panic("implement me")
}

func (w *walmartDesktopApi) GetCookies() []*http.Cookie {
  panic("implement me")
}

func (w *walmartDesktopApi) createCreateAccountPayload() interface{} {
  tokens := strings.Split(w.task.profile.Email, "@")
  return map[string]interface{}{
    "email":         tokens[0] + "+" + strconv.Itoa(rand.Intn(999999999)) + "@" + tokens[1],
    "emailAccepted": false,
    "firstName":     w.task.profile.FirstName,
    "lastName":      w.task.profile.LastName,
    "Password":      generatePassword(),
  }
}

func (w *walmartDesktopApi) createSignInAccountJson(account1 *Account) interface{} {
  return map[string]interface{}{
    "username":       account1.Email,
    "password":       account1.Password,
    "rememberme":     false,
    "showRememberme": true,
    "captcha": map[string]interface{}{
      "sensorData": "",
    },
  }
}

func (w *walmartDesktopApi) createAddToCartJson() interface{} {
  panic("implement me")
}

func (w *walmartDesktopApi) createCheckoutStep1Json() interface{} {
  return map[string]interface{}{
    "storeList":                      []string{},
    "postalCode":                     w.task.profile.PostalCode,
    "city":                           w.task.profile.City,
    "state":                          w.task.profile.State,
    "isZipLocated":                   w.task.isZipLocated,
    "crt:CRT":                        "",
    "customerId:CID":                 "",
    "customerType:type":              "",
    "affiliateInfo:com.wm.reflector": "",
  }
}

// (JsonObject jsonObject1)
func (w *walmartDesktopApi) createCheckoutStep2Json(itemId, shipMethod string) interface{} {
  return map[string]interface{}{
    "groups": []interface{}{
      map[string]interface{}{
        "fulfillmentOption": "S2H",
        "itemIds": []interface{}{
          itemId,
        },
        "shipMethod": shipMethod,
      },
    },
  }
}

func (w *walmartDesktopApi) createCheckoutStep3Json(step2 *selectShippingResult) interface{} {
  return map[string]interface{}{
    "addressLineOne":     w.task.profile.AddressLine1,
    "addressLineTwo":     w.task.profile.AddressLine2,
    "city":               w.task.profile.AddressLine1,
    "firstName":          w.task.profile.FirstName,
    "lastName":           w.task.profile.LastName,
    "phone":              w.task.profile.Phone,
    "email":              w.task.profile.Email,
    "marketingEmailPref": false,
    "postalCode":         w.task.profile.PostalCode,
    "state":              w.task.profile.State,
    "countryCode":        "USA",
    "changedFields":      []string{},
    "storeList":          []string{},
  }
}

func (w *walmartDesktopApi) createCheckoutStep4Json() interface{} {
  return map[string]interface{}{
    "encryptedPan":   w.task.paymentToken.encryptedPan,
    "encryptedCvv":   w.task.paymentToken.encryptedCvv,
    "integrityCheck": w.task.paymentToken.integrityCheck,
    "keyId":          w.task.paymentToken.keyId,
    "phase":          w.task.paymentToken.phase,

    "state":          w.task.profile.State,
    "postalCode":     w.task.profile.PostalCode,
    "addressLineOne": w.task.profile.AddressLine1,
    "addressLineTwo": w.task.profile.AddressLine2,
    "city":           strings.ToUpper(w.task.profile.City),
    "firstName":      w.task.profile.FirstName,
    "lastName":       w.task.profile.LastName,
    "expiryMonth":    w.task.profile.ExpiryMonth,
    "expiryYear":     w.task.profile.ExpiryYear,
    "phone":          w.task.profile.Phone,
    "cardType":       w.task.profile.PaymentMethod.CardType,
    "isGuest":        true,
  }
}

func (w *walmartDesktopApi) createSubmitPaymentJson(paymentToken1 *PaymentToken) interface{} {
  return map[string]interface{}{
    "payments": []interface{}{
      map[string]interface{}{
        "paymentType":    "CREDITCARD",
        "cardType":       w.task.profile.PaymentMethod.CardType,
        "firstName":      w.task.profile.FirstName,
        "lastName":       w.task.profile.LastName,
        "addressLineOne": w.task.profile.AddressLine1,
        "addressLineTwo": w.task.profile.AddressLine2,
        "city":           w.task.profile.City,
        "state":          w.task.profile.State,
        "postalCode":     w.task.profile.PostalCode,
        "expiryMonth":    w.task.profile.ExpiryMonth,
        "expiryYear":     w.task.profile.ExpiryYear,
        "email":          w.task.profile.Email,
        "phone":          w.task.profile.Phone,
        "encryptedPan":   paymentToken1.encryptedPan,
        "encryptedCvv":   paymentToken1.encryptedCvv,
        "integrityCheck": paymentToken1.integrityCheck,
        "keyId":          paymentToken1.keyId,
        "phase":          paymentToken1.phase,
        "piHash":         paymentToken1.piHash,
      },
    },
    "cvvInSession": true,
  }
}

func (w *walmartDesktopApi) createPreloadCartJson(offerId string, quantity int) map[string]interface{} {
  panic("implement me")
}

func (w *walmartDesktopApi) createProcessPaymentJson(paymentToken1 *PaymentToken) interface{} {
  return map[string]interface{}{
    "cvvInSession": false,
    "voltagePayments": []interface{}{
      map[string]interface{}{
        "paymentType":    "CREDITCARD",
        "encryptedCvv":   paymentToken1.encryptedCvv,
        "encryptedPan":   paymentToken1.encryptedPan,
        "integrityCheck": paymentToken1.integrityCheck,
        "keyId":          paymentToken1.keyId,
        "phase":          paymentToken1.phase,
      },
    },
  }
}

