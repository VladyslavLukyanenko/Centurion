package perimeterx

import (
  http "github.com/useflyent/fhttp"
)

type PerimeterX interface {
  GetUserAgent() string
  GetAcceptLanguage() string
  GetAcceptEncoding() string
  Reset()
  EnsureResponsePxProtectionSolved(response *http.Response) (bool, error)
}

func Desktop() PerimeterX {
	return nil
}

func Mobile() PerimeterX {
	return nil
}
