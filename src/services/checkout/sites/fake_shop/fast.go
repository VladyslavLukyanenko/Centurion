package fake_shop

import (
  contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
  "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/config/fakeshop"
  integContracts "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/integration"
  "github.com/CenturionLabs/centurion/checkout-service/core"
  "github.com/CenturionLabs/centurion/checkout-service/services"
  "github.com/CenturionLabs/centurion/checkout-service/sites/common"
  "github.com/golang/protobuf/proto"
  jsoniter "github.com/json-iterator/go"
  log "github.com/sirupsen/logrus"
  http "github.com/useflyent/fhttp"
  "go.elastic.co/apm"
  "io/ioutil"
  "math/rand"
  "os"
  "strconv"
  "time"
)

const TracingSpanName = "checkout"

func getProductFetchUrl() string {
  env := os.Getenv("APPLICATION_ENV")
  if env == "Development" {
    return "https://localhost:5100/fakeproduct"
  }

  return "https://fakeshop.centurion.gg/fakeproduct"
}

type fakeShopResponse struct {
  Id          string `json:"id"`
  Name        string `json:"name"`
  IsAvailable bool   `json:"isAvailable"`
}

type fakeShopFast struct {
  step              int
  ctx               *core.CheckoutPayload
  http              services.HttpClient
  config            *fakeshop.FakeShopConfig
  rpcManagerFactory services.RpcManagerFactory
  delay             time.Duration
  currentProfile    *contract.ProfileData
}

func newFakeShopFast(ctx *core.CheckoutPayload, http services.HttpClient,
  rpcManagerFactory services.RpcManagerFactory) core.CheckoutTask {
  return &fakeShopFast{
    ctx:               ctx,
    http:              http,
    rpcManagerFactory: rpcManagerFactory,
  }
}

func (f *fakeShopFast) FetchProduct() (*contract.ProductData, error) {
  price := float64(int((rand.Float64() * 100 + 20) * 100)) / 100 // truncating to 2 digits
  return &contract.ProductData{
    Sku:    f.ctx.Id,
    Name:   "FakeShopProduct." + f.ctx.Id,
    Image:  "https://accounts-api.centurion.gg/CenturionLogo__small.png",
    Link:   getProductFetchUrl(),
    Module: f.ctx.Module,
    Price:  &price,
  }, nil
}

func (f *fakeShopFast) GetCheckoutSteps() ([]core.CheckoutStep, error) {
  if err := f.initialize(); err != nil {
    return nil, err
  }

  return []core.CheckoutStep{
    f.initSession,
    f.solveProtection,
    f.checkInStock,
    f.cardProduct,
    f.placeOrder,
    f.purchase,
  }, nil
}

func (f *fakeShopFast) GetUsedProfile() *contract.ProfileData {
  return f.currentProfile
}

func (f *fakeShopFast) ProcessProductCheckedOutEvent(e *integContracts.ProductCheckedOut) {
  proxyUrlStr := "http://fakedomain.com:12345"
  e.Proxy = &proxyUrlStr
  price := float64(int((rand.Float64() * 100 + 20) * 100)) / 100 // truncating to 2 digits
  e.FormattedPrice = "$" + strconv.FormatFloat(price, 'f', 2, 64)
  e.Price = price
}

func (f *fakeShopFast) Step() int {
  return f.step
}

func (f *fakeShopFast) checkInStock() *core.StepExecutionFailure {
  span, _ := apm.StartSpan(f.ctx.Context, "checkInStock", TracingSpanName)
  defer span.End()
  f.ctx.ProgressConsumer <- taskStatus__Monitoring
  //f.step++
  //return nil

  req, _ := http.NewRequestWithContext(f.ctx.Context, "GET", getProductFetchUrl(), nil)
  for f.ctx.Context.Err() == nil {
    resp, err := f.http.Do(req)
    if err != nil {
      return f.ctx.ReportUnexpectedFailure(err)
    }

    bytes, err := ioutil.ReadAll(resp.Body)
    if err != nil {
      return f.ctx.ReportUnexpectedFailure(err)
    }

    json := new(fakeShopResponse)
    if err := jsoniter.Unmarshal(bytes, json); err != nil {
      return f.ctx.ReportUnexpectedFailure(err)
    }

    if json.IsAvailable {
      f.ctx.ProgressConsumer <- common.TaskStatus_ProductInStock
      f.step++
      return nil
    }

    f.ctx.ProgressConsumer <- common.TaskStatus_OutOfStock
    time.Sleep(2 * time.Second)
  }

  return nil
}

func (f *fakeShopFast) initSession() *core.StepExecutionFailure {
  span, _ := apm.StartSpan(f.ctx.Context, "initSession", TracingSpanName)
  defer span.End()
  f.ctx.ProgressConsumer <- taskStatus__SessionInitializing
  time.Sleep(f.delay)

  if shouldSucceed() {
    f.ctx.ProgressConsumer <- taskStatus__SessionInitialized
    f.step++
    return nil
  }

  return f.ctx.ReportError(taskStatus__FailedToSessionInitialize)
}

func (f *fakeShopFast) solveProtection() *core.StepExecutionFailure {
  span, _ := apm.StartSpan(f.ctx.Context, "solveProtection", TracingSpanName)
  defer span.End()
  f.ctx.ProgressConsumer <- taskStatus__SolvingProtection
  time.Sleep(f.delay * 4)

  if shouldSucceed() {
    f.ctx.ProgressConsumer <- taskStatus__SolvedProtection
    f.step++
    return nil
  }

  return f.ctx.ReportError(taskStatus__FailedToSolveProtection)
}

func shouldSucceed() bool {
  return rand.Int()%2 == 0
}

func (f *fakeShopFast) cardProduct() *core.StepExecutionFailure {
  span, _ := apm.StartSpan(f.ctx.Context, "cardProduct", TracingSpanName)
  defer span.End()
  f.ctx.ProgressConsumer <- taskStatus__CartingProduct
  time.Sleep(f.delay)

  if shouldSucceed() {
    f.ctx.ProgressConsumer <- taskStatus__CartedProduct
    f.step++
    return nil
  }

  return f.ctx.ReportError(taskStatus__FailedToCartProduct)
}

func (f *fakeShopFast) placeOrder() *core.StepExecutionFailure {
  span, _ := apm.StartSpan(f.ctx.Context, "placeOrder", TracingSpanName)
  defer span.End()
  f.ctx.ProgressConsumer <- taskStatus__PlacingOrder
  time.Sleep(f.delay * 2)

  if shouldSucceed() {
    f.ctx.ProgressConsumer <- taskStatus__PlacedOrder
    f.step++
    return nil
  }

  return f.ctx.ReportError(taskStatus__FailedToPlaceOrder)
}

func (f *fakeShopFast) purchase() *core.StepExecutionFailure {
  span, _ := apm.StartSpan(f.ctx.Context, "purchase", TracingSpanName)
  defer span.End()
  f.ctx.ProgressConsumer <- taskStatus__Purchasing
  time.Sleep(f.delay * 2)

  if f.config.GetRequiresSmsConfirmation() {
    for f.ctx.Context.Err() == nil {
      smsCode, err := f.promptUserSmsCode("0987654321", strconv.FormatInt(time.Now().UnixNano(), 36))
      if err != nil {
        log.Debugln("Error on sms confirmation " + err.Error())
        return f.ctx.ReportUnexpectedFailure(err)
      }

      code := "<No code. Cancelled>"
      if smsCode != nil {
        code = *smsCode
      }

      log.Debugln("Cms confirmation code received " + code)
      f.ctx.ReportInProgress("Code received: " + code)
      time.Sleep(f.delay * 2)
      break
    }
  }

  succeeded := f.config.GetShouldAlwaysSucceed() || shouldSucceed()
  if succeeded {
    f.ctx.ProgressConsumer <- common.TaskStatus_PurchaseSucceeded

    f.step++
  } else {
    f.ctx.ReportError(common.TaskStatus_PurchaseDeclined)
  }

  if f.config.GetShouldLoopForever() {
    f.step = 0
    time.Sleep(f.delay * 2)
    return nil
  }

  if succeeded {
    return nil
  }

  return f.ctx.ReportError(common.TaskStatus_PurchaseDeclined)
}

func (f *fakeShopFast) initialize() error {
  f.config = new(fakeshop.FakeShopConfig)
  err := proto.Unmarshal(f.ctx.Config, f.config)
  if err != nil {
    return err
  }

  if f.config.Delay != nil {
    f.delay = f.config.Delay.AsDuration()
  }

  if len(f.ctx.ProfileList) > 0 {
    f.currentProfile = f.ctx.ProfileList[0]
  }

  return nil
}

func (f *fakeShopFast) promptUserSmsCode(phoneNumber, taskDisplayId string) (*string, error) {
  f.ctx.ReportInProgress("Sms Code: task #" + taskDisplayId)
  for f.ctx.Context.Err() == nil {
    log.Debugln("Waiting for sms confirmation")
    code, err := f.rpcManagerFactory.
      Get(f.ctx.UserId).
      RequestSmsConfirmationCode(f.ctx.UserId, phoneNumber, taskDisplayId)

    if err != nil && err == services.RpcTimeoutError {
      log.Debugln("Sms confirmation TIMEOUT")
      continue
    }

    return code, err
  }

  return nil, f.ctx.Context.Err()
}
