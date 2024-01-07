package util

import (
  "strconv"
  "time"
)

func NowMillisStr() string {
  return strconv.FormatInt(NowUtcMillis(), 10)
}


func NowUtcMillis() int64 {
  return time.Now().UTC().UnixNano() / 1000_000
}