package util

import (
  contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
  log "github.com/sirupsen/logrus"
  "net/url"
)

func GetProxyURL(proxy *contract.ProxyData) (*url.URL, error) {
  return url.Parse(proxy.Value)
}

func GetProxyUrls(pool *contract.ProxyPoolData) ([]*url.URL, error) {
  urls := make([]*url.URL, len(pool.Proxies), len(pool.Proxies))
  for ix := range pool.Proxies {
    proxy := pool.Proxies[ix]
    proxyURL, err := GetProxyURL(proxy)
    if err != nil {
      log.Println("failed to parse proxy url " + proxy.Value)
      continue
    }

    urls[ix] = proxyURL
  }

  return urls, nil
}