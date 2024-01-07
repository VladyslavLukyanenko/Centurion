package tls

import "errors"

func getFirstValidCurve(c []CurveID) (CurveID, error) {
  for ix, _ := range c {
    curr := c[ix]
    if int(curr) >= GREASE_PLACEHOLDER {
      continue
    }

    return curr, nil
  }

  return 0, errors.New("tls: no supported curves")
}
