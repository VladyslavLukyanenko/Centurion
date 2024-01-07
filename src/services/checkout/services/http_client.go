package services

import (
	"context"
	"errors"
	"github.com/CenturionLabs/centurion/checkout-service/meek_client"
	tls "github.com/CenturionLabs/centurion/checkout-service/utls"
	"github.com/CenturionLabs/centurion/checkout-service/utls_presets"
  "github.com/sirupsen/logrus"
  http "github.com/useflyent/fhttp"
	"github.com/useflyent/fhttp/cookiejar"
	"net"
	"net/url"
	"os"
  "strings"
  "sync/atomic"
  "time"
)

var (
  HttpClient407ProxyAuthFailure = errors.New("407 Proxy Auth Failed")
)

type HttpClient interface {
	ChangeProxy(*url.URL)
	GetUsedProxy() *url.URL
	Do(req *http.Request) (*http.Response, error)
	UseCookieJar(jar *cookiejar.Jar)
	SetFollowRedirects(follow bool)
	SetTimeout(timeout time.Duration)
	ChangeTlsSettings(tlsIdName utls_presets.TlsIdName)
	GetTlsIdName() utls_presets.TlsIdName
}

type utlsHttpClient struct {
	http             *http.Client
	followRedirects  bool
	proxyURL         *url.URL
	tlsIdName        utls_presets.TlsIdName
	tlsRetryAttempts uint32
}

func NewHttpClient(tlsIdName utls_presets.TlsIdName, proxyURL *url.URL) HttpClient {
	var client *utlsHttpClient
	client = &utlsHttpClient{
		followRedirects: false,
		proxyURL:        proxyURL,
		tlsIdName:       tlsIdName,
	}

	client.http = &http.Client{
		Transport: client.createHttpTransport(),
		CheckRedirect: func(req *http.Request, via []*http.Request) error {
			if client.followRedirects {
				return nil
			}
			return http.ErrUseLastResponse
		},
	}

	return client
}

func (u *utlsHttpClient) createHttpTransport() http.RoundTripper {
	tlsSpec, err := utls_presets.UtlsIdNameToSpec(u.tlsIdName)
	if err != nil {
		panic(err)
	}

	cfg := &tls.Config{InsecureSkipVerify: true}
	keyLogFile := os.Getenv("CENTURION_CHECKOUT_SSLKEYLOGFILE")
	if len(keyLogFile) > 0 {
		kl, err := os.OpenFile(keyLogFile, os.O_WRONLY|os.O_CREATE|os.O_APPEND, 0600)
		if err != nil {
			panic(err)
		}

		cfg.KeyLogWriter = kl
	}

	rt, _ := meek_client.NewUTLSRoundTripper(tlsSpec, cfg, u.proxyURL)

	return rt
}

func (u *utlsHttpClient) SetTimeout(timeout time.Duration) {
	u.http.Timeout = timeout
}

func (u *utlsHttpClient) SetFollowRedirects(follow bool) {
	u.followRedirects = follow
}

func (u *utlsHttpClient) ChangeTlsSettings(tlsIdName utls_presets.TlsIdName) {
	u.tlsIdName = tlsIdName
	u.http.Transport = u.createHttpTransport()
}
func (u *utlsHttpClient) GetTlsIdName() utls_presets.TlsIdName {
	return u.tlsIdName
}

func (u *utlsHttpClient) UseCookieJar(jar *cookiejar.Jar) {
	u.http.Jar = jar
}

func (u *utlsHttpClient) ChangeProxy(proxyUrl *url.URL) {
	if proxyUrl == u.proxyURL {
		return
	}

	u.proxyURL = proxyUrl
	u.http.Transport = u.createHttpTransport()
}

func (u *utlsHttpClient) GetUsedProxy() *url.URL {
	return u.proxyURL
}

func (u *utlsHttpClient) Do(req *http.Request) (*http.Response, error) {
	resp, err := u.http.Do(req)
	if err == nil && resp == nil {
		return nil, errors.New("no response")
	}

	if err != nil {
		if timeoutErr, ok := err.(net.Error); ok && timeoutErr.Timeout() {
			return nil, errors.New("no response")
		}

		if urlError, ok := err.(*url.Error); ok && tls.IsTlsError(urlError.Err) && u.tlsRetryAttempts < 10 {
      logrus.WithError(err).Errorln("TLS error")
			u.http.CloseIdleConnections()
			u.http.Transport = u.createHttpTransport()
      atomic.AddUint32(&u.tlsRetryAttempts, 1)
			return u.Do(req.Clone(context.Background()))
		}

    if isProxyAuthFailure(err) {
      return nil, HttpClient407ProxyAuthFailure
    }
	}

  u.tlsRetryAttempts = 0

	return resp, err
}

func isProxyAuthFailure(err error) bool {
  return strings.Contains(err.Error(), "407 Proxy")
}