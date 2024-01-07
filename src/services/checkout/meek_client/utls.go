// Support code for TLS camouflage using uTLS.
//
// The goal is: provide an http.RoundTripper abstraction that retains the
// features of http.Transport (e.g., persistent connections and HTTP/2 support),
// while making TLS connections using uTLS in place of crypto/tls. The challenge
// is: while http.Transport provides a DialTLS hook, setting it to non-nil
// disables automatic HTTP/2 support in the client. Most of the uTLS
// fingerprints contain an ALPN extension containing "h2"; i.e., they declare
// support for HTTP/2. If the server also supports HTTP/2, then uTLS may
// negotiate an HTTP/2 connection without the http.Transport knowing it, which
// leads to an HTTP/1.1 client speaking to an HTTP/2 server, a protocol error.
//
// The code here uses an idea adapted from meek_lite in obfs4proxy:
// https://gitlab.com/yawning/obfs4/commit/4d453dab2120082b00bf6e63ab4aaeeda6b8d8a3
// Instead of setting DialTLS on an http.Transport and exposing it directly, we
// expose a wrapper type, UTLSRoundTripper, that contains within it either an
// http.Transport or an http2.Transport. The first time a caller calls RoundTrip
// on the wrapper, we initiate a uTLS connection (bootstrapConn), then peek at
// the ALPN-negotiated protocol: if "h2", create an internal http2.Transport;
// otherwise, create an internal http.Transport. In either case, set DialTLS on
// the created Transport to a function that dials using uTLS. As a special case,
// the first time the DialTLS callback is called, it reuses bootstrapConn (the
// one made to peek at the ALPN), rather than make a new connection.
//
// Subsequent calls to RoundTripper on the wrapper just pass the requests though
// the previously created http.Transport or http2.Transport. We assume that in
// future RoundTrips, the ALPN-negotiated protocol will remain the same as it
// was in the initial RoundTrip. At this point it is the http.Transport or
// http2.Transport calling DialTLS, not us, so we can't dynamically swap the
// underlying transport based on the ALPN.
//
// https://bugs.torproject.org/29077
// https://github.com/CenturionLabs/centurion/checkout-service/utls/issues/16
package meek_client

import (
	"crypto/tls"
	"fmt"
	"net"
	//"net/http"
	http "github.com/useflyent/fhttp"
	"net/url"
	"sync"

	utls "github.com/CenturionLabs/centurion/checkout-service/utls"
	//"golang.org/x/net/http2"
	"github.com/useflyent/fhttp/http2"
	"golang.org/x/net/proxy"
)

// Extract a host:port address from a URL, suitable for passing to net.Dial.
func addrForDial(url *url.URL) (string, error) {
	host := url.Hostname()
	// net/http would use golang.org/x/net/idna here, to convert a possible
	// internationalized domain name to ASCII.
	port := url.Port()
	if port == "" {
		// No port? Use the default for the scheme.
		switch url.Scheme {
		case "http":
			port = "80"
		case "https":
			port = "443"
		default:
			return "", fmt.Errorf("unsupported URL scheme %q", url.Scheme)
		}
	}
	return net.JoinHostPort(host, port), nil
}

// Analogous to tls.Dial. Connect to the given address and initiate a TLS
// handshake using the given ClientHelloID, returning the resulting connection.
func dialUTLS(network, addr string, cfg *utls.Config, spec *utls.ClientHelloSpec, forward proxy.Dialer) (*utls.UConn, error) {
	conn, err := forward.Dial(network, addr)
	if err != nil {
		return nil, err
	}
	uconn, err := createUTLSClient(conn, cfg, spec)
	if err != nil {
		return nil, err
	}

	if cfg == nil || cfg.ServerName == "" {
		serverName, _, err := net.SplitHostPort(addr)
		if err != nil {
			return nil, err
		}
		uconn.SetSNI(serverName)
	}
	err = uconn.Handshake()
	if err != nil {
		return nil, err
	}
	return uconn, nil
}

func createUTLSClient(conn net.Conn, config *utls.Config, spec *utls.ClientHelloSpec) (*utls.UConn, error) {
	clientHello := utls.HelloGolang
	if spec != nil {
		clientHello = utls.HelloCustom
		config.CipherSuites = spec.CipherSuites
	}

	uTlsConn := utls.UClient(conn, config, clientHello)

	if spec != nil {
		//	log.Println("Applying spec")
		//	for _, e := range spec.Extensions {
		//		switch ext := e.(type) {
		//		case *utls.UtlsGREASEExtension:
		//			log.Printf("BEFORE: UtlsGREASEExtension. Value %v Body %#v\n", ext.Value, ext.Body)
		//		case *utls.KeyShareExtension:
		//			for _, share := range ext.KeyShares {
		//				log.Printf("BEFORE: KeyShare applying Group %v Data len %v", share.Group, len(share.Data))
		//			}
		//		}
		//	}
		err := uTlsConn.ApplyPreset(spec)
		if err != nil {
			return nil, fmt.Errorf("failed to apply spec: %+v", err)
		}

		//for _, e := range spec.Extensions {
		//	switch ext := e.(type) {
		//	case *utls.UtlsGREASEExtension:
		//		log.Printf("AFTER: UtlsGREASEExtension. Value %v Body %#v\n", ext.Value, ext.Body)
		//	case *utls.KeyShareExtension:
		//		for _, share := range ext.KeyShares {
		//			log.Printf("AFTER: KeyShare value Group %v Data len %v", share.Group, len(share.Data))
		//		}
		//	}
		//}
	}

	return uTlsConn, nil
}

// A http.RoundTripper that uses uTLS (with a specified Client Hello ID) to make
// TLS connections.
//
// Can only be reused among servers which negotiate the same ALPN.
type UTLSRoundTripper struct {
	sync.Mutex

	spec        *utls.ClientHelloSpec
	config      *utls.Config
	proxyDialer proxy.Dialer
	rt          http.RoundTripper

	// Transport for HTTP requests, which don't use uTLS.
	httpRT *http.Transport
}

func (rt *UTLSRoundTripper) RoundTrip(req *http.Request) (*http.Response, error) {
	switch req.URL.Scheme {
	case "http":
		// If http, we don't invoke uTLS; just pass it to an ordinary http.Transport.
		return rt.httpRT.RoundTrip(req)
	case "https":
	default:
		return nil, fmt.Errorf("unsupported URL scheme %q", req.URL.Scheme)
	}

	rt.Lock()
	defer rt.Unlock()

	if rt.rt == nil {
		// On the first call, make an http.Transport or http2.Transport
		// as appropriate.
		var err error
		rt.rt, err = makeRoundTripper(req.URL, rt.spec, rt.config, rt.proxyDialer)
		if err != nil {
			return nil, err
		}
	}
	// Forward the request to the internal http.Transport or http2.Transport.
	return rt.rt.RoundTrip(req)
}

// Unlike when using the native Go net/http (whose built-in proxy support we can
// use by setting Proxy on an http.Transport), and unlike when using the browser
// helper (the browser has its own proxy support), when using uTLS we have to
// craft our own proxy connections.
func makeProxyDialer(proxyURL *url.URL, cfg *utls.Config, spec *utls.ClientHelloSpec) (proxy.Dialer, error) {
	var proxyDialer proxy.Dialer = proxy.Direct
	if proxyURL == nil {
		return proxyDialer, nil
	}

	proxyAddr, err := addrForDial(proxyURL)
	if err != nil {
		return nil, err
	}

	var auth *proxy.Auth
	if userpass := proxyURL.User; userpass != nil {
		auth = &proxy.Auth{
			User: userpass.Username(),
		}
		if password, ok := userpass.Password(); ok {
			auth.Password = password
		}
	}

	switch proxyURL.Scheme {
	case "socks5":
		proxyDialer, err = proxy.SOCKS5("tcp", proxyAddr, auth, proxyDialer)
	case "http":
		proxyDialer, err = ProxyHTTP("tcp", proxyAddr, auth, proxyDialer)
	case "https":
		// We use the same uTLS Config for TLS to the HTTPS proxy, as we
		// use for HTTPS connections through the tunnel. We make a clone
		// of the Config to avoid concurrent modification as the two
		// layers set the ServerName value.
		var cfgClone *utls.Config
		if cfg != nil {
			cfgClone = cfg.Clone()
		}
		proxyDialer, err = ProxyHTTPS("tcp", proxyAddr, auth, proxyDialer, cfgClone, spec)
	default:
		return nil, fmt.Errorf("cannot use proxy scheme %q with uTLS", proxyURL.Scheme)
	}

	return proxyDialer, err
}

func makeRoundTripper(url *url.URL, spec *utls.ClientHelloSpec, cfg *utls.Config, proxyDialer proxy.Dialer) (http.RoundTripper, error) {
	addr, err := addrForDial(url)
	if err != nil {
		return nil, err
	}

	// Connect to the given address, through a proxy if requested, and
	// initiate a TLS handshake using the given ClientHelloID. Return the
	// resulting connection.
	dial := func(network, addr string) (*utls.UConn, error) {
		return dialUTLS(network, addr, cfg, spec, proxyDialer)
	}

	bootstrapConn, err := dial("tcp", addr)
	if err != nil {
		return nil, err
	}

	// Peek at what protocol we negotiated.
	protocol := bootstrapConn.ConnectionState().NegotiatedProtocol

	// Protects bootstrapConn.
	var lock sync.Mutex
	// This is the callback for future dials done by the internal
	// http.Transport or http2.Transport.
	dialTLS := func(network, addr string) (net.Conn, error) {
		lock.Lock()
		defer lock.Unlock()

		// On the first dial, reuse bootstrapConn.
		if bootstrapConn != nil {
			uconn := bootstrapConn
			bootstrapConn = nil
			return uconn, nil
		}

		// Later dials make a new connection.
		uconn, err := dial(network, addr)
		if err != nil {
			return nil, err
		}
		if uconn.ConnectionState().NegotiatedProtocol != protocol {
			return nil, fmt.Errorf("unexpected switch from ALPN %q to %q",
				protocol, uconn.ConnectionState().NegotiatedProtocol)
		}

		return uconn, nil
	}

	// Construct an http.Transport or http2.Transport depending on ALPN.
	switch protocol {
	case http2.NextProtoTLS:
		// Unfortunately http2.Transport does not expose the same
		// configuration options as http.Transport with regard to
		// timeouts, etc., so we are at the mercy of the defaults.
		// https://github.com/golang/go/issues/16581
		return &http2.Transport{
			DialTLS: func(network, addr string, _ *tls.Config) (net.Conn, error) {
				// Ignore the *tls.Config parameter; use our
				// static cfg instead.
				return dialTLS(network, addr)
			},

			/* CHROME Settings*/
			MaxHeaderListSize: 262144,
			HeaderTableSize:   65536,
			InitialWindowSize: 6291456,
			Settings: []http2.Setting{
				{ID: http2.SettingMaxConcurrentStreams, Val: 1000},
				{ID: http2.SettingMaxFrameSize, Val: 16384},

        // todo: double check if it works correctly
				{ID: http2.SettingMaxHeaderListSize, Val: 262144},
			},
      // todo: double check if it works correctly
			StrictMaxConcurrentStreams: true,
      // todo: double check if it works correctly
			PushHandler:                &http2.DefaultPushHandler{},
		}, nil
	default:
		// With http.Transport, copy important default fields from
		// http.DefaultTransport, such as TLSHandshakeTimeout and
		// IdleConnTimeout, before overriding DialTLS.
		tr := httpRoundTripper.Clone()
		tr.DialTLS = dialTLS
		return tr, nil
	}
}

//
//// When you update this map, also update the man page in doc/meek-client.1.txt.
//var clientHelloIDMap = map[string]*utls.ClientHelloID{
//	// No HelloCustom: not useful for external configuration.
//	// No HelloRandomized: doesn't negotiate consistent ALPN.
//	"none":                  nil, // special case: disable uTLS
//	"hellogolang":           nil, // special case: disable uTLS
//	"hellorandomizedalpn":   &utls.HelloRandomizedALPN,
//	"hellorandomizednoalpn": &utls.HelloRandomizedNoALPN,
//	"hellofirefox_auto":     &utls.HelloFirefox_Auto,
//	"hellofirefox_55":       &utls.HelloFirefox_55,
//	"hellofirefox_56":       &utls.HelloFirefox_56,
//	"hellofirefox_63":       &utls.HelloFirefox_63,
//	"hellofirefox_65":       &utls.HelloFirefox_65,
//	"hellochrome_auto":      &utls.HelloChrome_Auto,
//	"hellochrome_58":        &utls.HelloChrome_58,
//	"hellochrome_62":        &utls.HelloChrome_62,
//	"hellochrome_70":        &utls.HelloChrome_70,
//	"hellochrome_72":        &utls.HelloChrome_72,
//	"helloios_auto":         &utls.HelloIOS_Auto,
//	"helloios_11_1":         &utls.HelloIOS_11_1,
//	"helloios_12_1":         &utls.HelloIOS_12_1,
//}

func NewUTLSRoundTripper(spec *utls.ClientHelloSpec, cfg *utls.Config, proxyURL *url.URL) (http.RoundTripper, error) {
	// Lookup is case-insensitive.
	//clientHelloID, ok := clientHelloIDMap[strings.ToLower(name)]
	//if !ok {
	//	return nil, fmt.Errorf("no uTLS Client Hello ID named %q", name)
	//}
	if spec == nil {
		// Special case for "none" and HelloGolang.
		return httpRoundTripper, nil
	}

	proxyDialer, err := makeProxyDialer(proxyURL, cfg, spec)
	if err != nil {
		return nil, err
	}

	// This special-case RoundTripper is used for HTTP requests, which don't
	// use uTLS but should use the specified proxy.
	httpRT := httpRoundTripper.Clone()
	httpRT.Proxy = http.ProxyURL(proxyURL)

	return &UTLSRoundTripper{
		spec:        spec,
		config:      cfg,
		proxyDialer: proxyDialer,
		// rt will be set in the first call to RoundTrip.
		httpRT: httpRT,
	}, nil
}
