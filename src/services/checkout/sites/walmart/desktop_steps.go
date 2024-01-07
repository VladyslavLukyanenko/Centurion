package walmart

import (
  "github.com/CenturionLabs/centurion/checkout-service/core"
  "github.com/CenturionLabs/centurion/checkout-service/sites/walmart/perimeterx"
  "math/rand"
  "strings"
  "time"
)

func (t *walmartTask) startTask() *core.StepExecutionFailure {
	if t.mode == DESKTOP {
		t.perimeterX = perimeterx.Desktop()
	} else {
    t.perimeterX = perimeterx.Mobile()
  }


  if err := t.executeWmpieGetKeyAndSetPaymentToken(); err != nil {
    return t.payload.ReportUnexpectedFailure(err)
  }

  if err := t.api.initializePxSessionReceiveCookies(); err != nil {
    return t.payload.ReportUnexpectedFailure(err)
  }

  if err := t.setCollectedCookies(); err != nil {
    return t.payload.ReportUnexpectedFailure(err)
  }

  if strings.Contains(t.taskMode, "fast") {
    piHash, err := t.generateCheckout()
    if err != nil {
      return t.payload.ReportUnexpectedError(err)
    }

    t.paymentToken.piHash = piHash

    t.clearCookies()
    if strings.Contains(t.taskMode, "grief") {
      t.setRandomProxy()
    }
  }

  // delay 300-1500ms
  time.Sleep(time.Millisecond * time.Duration(rand.Int31n(1200) + 300))

  return t.checkoutTask()
}

func (t *walmartTask) setCollectedCookies() error {
  t.clearCookies()
  if t.mode == OTHER {
    if err := t.preloadSession(); err != nil {
      return err
    }
  }

  t.timestamp = time.Now().UTC()
  // grab cookies (but they may be already in httpclient cookieJar)
  return nil
}