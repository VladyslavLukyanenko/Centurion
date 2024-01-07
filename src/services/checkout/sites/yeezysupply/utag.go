package yeezysupply

import (
  "github.com/CenturionLabs/centurion/checkout-service/util"
  "math"
  "math/rand"
  "strconv"
  "strings"
)

const (
  Rnd2DigSymbols = "0123456789"
)

type Utag struct {
  documentUrl         string
  productName         string
  updateCookieCounter int32
  bmak                *Bmak
  userAgent string
  sku       string
  sesId     int64
  st                  int64
  exp                 int64
  vId                 string
  getQueueUrl         func() string
}

func (u *Utag) GetMain() string {
  u.generateVIdIfEmpty(util.NowUtcMillis())
  if u.sesId == -1 {
    u.sesId = util.NowUtcMillis()
  }

  u.st = util.NowUtcMillis() + 1_800_000
  u.exp = util.NowUtcMillis() + int64(util.RandInt(470, 480)) + 3_600_000
  u.updateCookieCounter += 1
  ss := 1
  if u.updateCookieCounter != 1 {
    ss = 0
  }

  prevPage := u.getPrevPage()
  return "v_id:" + u.vId +
    "$_se:" + strconv.FormatInt(int64(u.updateCookieCounter), 10) +
    "$_ss:" + strconv.Itoa(ss) +
    "$_st:" + strconv.FormatInt(u.st, 10) +
    "$ses_id:" + strconv.FormatInt(u.sesId, 10) +
    "%3Bexp-session$_pn:" + strconv.FormatInt(int64(u.updateCookieCounter), 10) +
    "%3Bexp-session$_prevpage:" + prevPage +
    "%3Bexp-" + strconv.FormatInt(u.exp, 10)
}


func (u *Utag) setProductName(val string) {
  u.productName = strings.Replace(val, " ", "%20", -1)
}

func (u *Utag) generateVIdIfEmpty(timestamp int64) {
  if u.bmak != nil && len(u.vId) == 0 {
    panic("Not supported")
  } else if len(u.userAgent) > 0 && len(u.vId) == 0 {
    b := strings.Builder{}
    b.WriteString(util.PadInt64Hex(timestamp, 12))

    parsed, _ := strconv.ParseInt(strconv.FormatFloat(rand.Float64(), 'f', -1, 64)[2:], 10, 64)
    rndFloat := util.PadInt64Hex(parsed, 16)
    b.WriteString(rndFloat)
    b.WriteString(util.PadInt64Hex(3, 2))
    b.WriteString(util.PadInt64Hex(int64(len(u.userAgent)), 3))
    b.WriteString(util.PadInt64Hex(int64(len(u.documentUrl)), 4))
    b.WriteString(util.PadInt64Hex(int64(len(u.userAgent)), 3))
    b.WriteString(util.PadInt64Hex(int64(int32(rand.Float64()*1761)+800)+int64(int32(rand.Float64()*841)+624), 5))
    u.vId = b.String()
  }
}

func (u *Utag) getPrevPage() string {
  if !strings.Contains(u.documentUrl, "product") && !strings.Contains(u.documentUrl, "archive") {
    if u.documentUrl == u.getQueueUrl() {
      return u.getWaitingRoomEncodedPath()
    }

    if strings.Contains(u.documentUrl, "/delivery") {
      return u.getCheckoutShippingEncodedPath()
    }

    if strings.Contains(u.documentUrl, "/payment") {
      return u.getCheckoutPaymentEncodedPath()
    }

    return "HOME"
  }

  return u.getProductEncodedPath()
}

func (u *Utag) getWaitingRoomEncodedPath() string {
  return "WAITING%20ROOM%7C" + u.productName + "%7C" + u.productName + "%20(" + u.sku + ")"
}

func (u *Utag) getCheckoutShippingEncodedPath() string {
  return "CHECKOUT%7CSHIPPING"
}

func (u *Utag) getCheckoutPaymentEncodedPath() string {
  return "CHECKOUT%7CPAYMENT"
}

func (u *Utag) getProductEncodedPath() string {
  return "PRODUCT%7C" + u.productName + "%20(" + u.sku + ")"
}

func GetIntH() int {
  return int(math.Ceil(float64(util.NowUtcMillis()) / 86400000.0))
}

func UtagGetRndDigitLongStr() string {
  left := strings.Builder{}
  right := strings.Builder{}
  max := 10
  max2 := 10
  curr := 0
  for 19 > curr {
    rnd := rand.Intn(max)
    left.WriteByte(Rnd2DigSymbols[rnd])
    max = getValidMaxForRnd2DigStr(max, curr, rnd)

    var rnd2 = rand.Intn(max2)
    right.WriteByte(Rnd2DigSymbols[rnd2])
    max2 = getValidMaxForRnd2DigStr(max2, curr, rnd2)
    curr++
  }

  return left.String() + right.String()
}

func getValidMaxForRnd2DigStr(max, curr, rnd int) int {
  if 0 == curr && 9 == rnd {
    max = 3
  } else if (1 == curr || 2 == curr) && 10 != max && 2 > rnd {
    max = 10
  } else if 2 < curr {
    max = 10
  }

  return max
}

func CreateUtag(ua string, productId string, getQueueUrl func() string) *Utag {
  return &Utag{
    sesId:               -1,
    updateCookieCounter: 0,
    bmak:                nil,
    userAgent:           ua,
    documentUrl:         YeezySupplyWWWDomainUrlSlashEnding,
    sku:                 productId,
    getQueueUrl:         getQueueUrl,
  }
}
