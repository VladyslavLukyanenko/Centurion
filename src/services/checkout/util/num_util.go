package util

import (
  "fmt"
)

func PadInt64Hex(n int64, zerosCount int) string {
  return fmt.Sprintf("%0*x", zerosCount, n)
}
