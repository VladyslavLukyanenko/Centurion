package yeezysupply

import (
  crypto_rand "crypto/rand"
  "encoding/hex"
  "github.com/CenturionLabs/centurion/checkout-service/util"
  "github.com/google/uuid"
  "strconv"
)

func GenRTValue() (string, error) {
  sessionId := uuid.NewString()
  timestamp := strconv.FormatInt(util.NowUtcMillis(), 36)
  timeToLive := strconv.FormatInt(int64(util.RandInt(890, 5800)), 36)

  rnd := make([]byte, 4, 4)
  _, err := crypto_rand.Read(rnd)
  if err != nil {
    return "", err
  }

  akstatSubdomain := hex.EncodeToString(rnd)

  return "\"z=1&dm=yeezysupply.com&si=" + sessionId + "&ss=" + timestamp + "&sl=1&tt=" + timeToLive + "&bcn=%2F%2F" + akstatSubdomain + ".akstat.io%2F\"", nil;
}
