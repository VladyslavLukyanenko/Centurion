package util

import (
	http "github.com/useflyent/fhttp"
	"math/rand"
)

func HeaderShuffle(headers http.Header) {
	headersOrder := headers[http.PHeaderOrderKey]
	rand.Shuffle(len(headersOrder), func(i, j int) {
		headersOrder[i], headersOrder[j] = headersOrder[j], headersOrder[i]
	})
}


func AddHeaders(target http.Header, copyFrom http.Header) {
  for k, values := range copyFrom {
    for ix, _ := range values {
      target.Add(k, values[ix])
    }
  }
}