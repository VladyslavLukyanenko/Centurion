package util

import (
	"net/url"
)

func MustParseUrl(rawUrl string) *url.URL {
	u, err := url.Parse(rawUrl)
	if err != nil {
		panic(err)
	}

	return u
}
