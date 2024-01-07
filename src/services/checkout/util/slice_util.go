package util

import (
	"errors"
	"math/rand"
	"net/url"
	"strings"
	"time"
)

var sharedRnd = rand.New(rand.NewSource(time.Now().UnixNano()))

func RandomUrl(items []*url.URL) (*url.URL, error) {
	if len(items) == 0 {
		return nil, errors.New("no items")
	}

	ix := sharedRnd.Int31n(int32(len(items)))
	return items[ix], nil
}

func StrSliceContains(searchedValue string, s []string) bool {
	for i := range s {
		if strings.EqualFold(s[i], searchedValue) {
			return true
		}
	}

	return false
}