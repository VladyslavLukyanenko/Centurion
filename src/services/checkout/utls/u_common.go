// Copyright 2017 Google Inc. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

package tls

import (
	"crypto/hmac"
	"crypto/sha512"
	"fmt"
  "hash"
)

// Naming convention:
// Unsupported things are prefixed with "Fake"
// Things, supported by utls, but not crypto/tls' are prefixed with "utls"
// Supported things, that have changed their ID are prefixed with "Old"
// Supported but disabled things are prefixed with "Disabled". We will _enable_ them.
const (
	utlsExtensionPadding              uint16 = 21
	utlsExtensionExtendedMasterSecret uint16 = 23 // https://tools.ietf.org/html/rfc7627

  // https://datatracker.ietf.org/doc/html/rfc8879#section-7.1
  utlsExtensionCompressCertificate uint16 = 27

	// extensions with 'fake' prefix break connection, if server echoes them back
	fakeExtensionChannelID uint16 = 30032 // not IANA assigned

	fakeRecordSizeLimit     uint16 = 0x001c
  fakeExtensionTokenBinding uint16 = 24
  fakeExtensionChannelIDOld uint16 = 30031 // not IANA assigned
  //fakeExtensionALPS         uint16 = 17513 // not IANA assigned
  extensionApplicationSettings        uint16 = 17513 // not IANA assigned
  extensionCustom                     uint16 = 1234  // not IANA assigned

  // https://datatracker.ietf.org/doc/html/rfc8879#section-7.2
  typeCompressedCertificate uint8 = 25
)

const (
	OLD_TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256   = uint16(0xcc13)
	OLD_TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256 = uint16(0xcc14)

	DISABLED_TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384 = uint16(0xc024)
	DISABLED_TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384   = uint16(0xc028)
	DISABLED_TLS_RSA_WITH_AES_256_CBC_SHA256         = uint16(0x003d)

	FAKE_OLD_TLS_DHE_RSA_WITH_CHACHA20_POLY1305_SHA256 = uint16(0xcc15) // we can try to craft these ciphersuites
	FAKE_TLS_DHE_RSA_WITH_AES_128_GCM_SHA256           = uint16(0x009e) // from existing pieces, if needed

	FAKE_TLS_DHE_RSA_WITH_AES_128_CBC_SHA  = uint16(0x0033)
	FAKE_TLS_DHE_RSA_WITH_AES_256_CBC_SHA  = uint16(0x0039)
	FAKE_TLS_DHE_RSA_WITH_AES_256_GCM_SHA384  = uint16(0x009f)
	FAKE_TLS_RSA_WITH_RC4_128_MD5          = uint16(0x0004)
	FAKE_TLS_EMPTY_RENEGOTIATION_INFO_SCSV = uint16(0x00ff)
)

// newest signatures
var (
	FakePKCS1WithSHA224 SignatureScheme = 0x0301
	FakeECDSAWithSHA224 SignatureScheme = 0x0303

	// fakeEd25519 = SignatureAndHash{0x08, 0x07}
	// fakeEd448 = SignatureAndHash{0x08, 0x08}
)

// fake curves(groups)
var (
	FakeFFDHE2048 = uint16(0x0100)
	FakeFFDHE3072 = uint16(0x0101)
)

// https://tools.ietf.org/html/draft-ietf-tls-certificate-compression-04
type CertCompressionAlgo uint16

const (
	CertCompressionZlib   CertCompressionAlgo = 0x0001
	CertCompressionBrotli CertCompressionAlgo = 0x0002
  CertCompressionZstd   CertCompressionAlgo = 0x0003
)

const (
	PskModePlain uint8 = pskModePlain
	PskModeDHE   uint8 = pskModeDHE
)

type ClientHelloID struct {
	Client string

	// Version specifies version of a mimicked clients (e.g. browsers).
	// Not used in randomized, custom handshake, and default Go.
	Version string

	// Seed is only used for randomized fingerprints to seed PRNG.
	// Must not be modified once set.
	Seed *PRNGSeed
}

func (p *ClientHelloID) Str() string {
	return fmt.Sprintf("%s-%s", p.Client, p.Version)
}

func (p *ClientHelloID) IsSet() bool {
	return (p.Client == "") && (p.Version == "")
}

const (
	// clients
	helloGolang           = "Golang"
	helloRandomized       = "Randomized"
	helloRandomizedALPN   = "Randomized-ALPN"
	helloRandomizedNoALPN = "Randomized-NoALPN"
	helloCustom           = "Custom"
	helloFirefox          = "Firefox"
	helloChrome           = "Chrome"
	helloIOS              = "iOS"
	helloAndroid          = "Android"

	// versions
	helloAutoVers = "0"
)

type ClientHelloSpec struct {
	CipherSuites       []uint16       // nil => default
	CompressionMethods []uint8        // nil => no compression
	Extensions         []TLSExtension // nil => no extensions

	TLSVersMin uint16 // [1.0-1.3] default: parse from .Extensions, if SupportedVersions ext is not present => 1.0
	TLSVersMax uint16 // [1.2-1.3] default: parse from .Extensions, if SupportedVersions ext is not present => 1.2

	// GreaseStyle: currently only random
	// sessionID may or may not depend on ticket; nil => random
	GetSessionID func(ticket []byte) [32]byte

	// TLSFingerprintLink string // ?? link to tlsfingerprint.io for informational purposes
}

var (
	// HelloGolang will use default "crypto/tls" handshake marshaling codepath, which WILL
	// overwrite your changes to Hello(Config, Session are fine).
	// You might want to call BuildHandshakeState() before applying any changes.
	// UConn.Extensions will be completely ignored.
	HelloGolang = ClientHelloID{helloGolang, helloAutoVers, nil}

	// HelloCustom will prepare ClientHello with empty uconn.Extensions so you can fill it with
	// TLSExtensions manually or use ApplyPreset function
	HelloCustom = ClientHelloID{helloCustom, helloAutoVers, nil}

	// HelloRandomized* randomly adds/reorders extensions, ciphersuites, etc.
	HelloRandomized       = ClientHelloID{helloRandomized, helloAutoVers, nil}
	HelloRandomizedALPN   = ClientHelloID{helloRandomizedALPN, helloAutoVers, nil}
	HelloRandomizedNoALPN = ClientHelloID{helloRandomizedNoALPN, helloAutoVers, nil}

	// The rest will will parrot given browser.
	HelloFirefox_Auto = HelloFirefox_65
	HelloFirefox_55   = ClientHelloID{helloFirefox, "55", nil}
	HelloFirefox_56   = ClientHelloID{helloFirefox, "56", nil}
	HelloFirefox_63   = ClientHelloID{helloFirefox, "63", nil}
	HelloFirefox_65   = ClientHelloID{helloFirefox, "65", nil}

	HelloChrome_Auto = HelloChrome_83
	HelloChrome_58   = ClientHelloID{helloChrome, "58", nil}
	HelloChrome_62   = ClientHelloID{helloChrome, "62", nil}
	HelloChrome_70   = ClientHelloID{helloChrome, "70", nil}
	HelloChrome_72   = ClientHelloID{helloChrome, "72", nil}
	HelloChrome_83   = ClientHelloID{helloChrome, "83", nil}

	HelloIOS_Auto = HelloIOS_12_1
	HelloIOS_11_1 = ClientHelloID{helloIOS, "111", nil} // legacy "111" means 11.1
	HelloIOS_12_1 = ClientHelloID{helloIOS, "12.1", nil}
)

// based on spec's GreaseStyle, GREASE_PLACEHOLDER may be replaced by another GREASE value
// https://tools.ietf.org/html/draft-ietf-tls-grease-01
const GREASE_PLACEHOLDER = 0x0a0a

func isGREASEUint16(v uint16) bool {
	// First byte is same as second byte
	// and lowest nibble is 0xa
	return ((v >> 8) == v&0xff) && v&0xf == 0xa
}

func unGREASEUint16(v uint16) uint16 {
	if isGREASEUint16(v) {
		return GREASE_PLACEHOLDER
	} else {
		return v
	}
}


type macFunction interface {
  // Size returns the length of the MAC.
  Size() int
  // MAC appends the MAC of (seq, header, data) to out. The extra data is fed
  // into the MAC after obtaining the result to normalize timing. The result
  // is only valid until the next invocation of MAC as the buffer is reused.
  MAC(seq, header, data, extra []byte) []byte
}


// tls10MAC__t implements the TLS 1.0 MAC function. RFC 2246, Section 6.2.3.
type tls10MAC__t struct {
  h   hash.Hash
  buf []byte
}

func (s tls10MAC__t) Size() int {
  return s.h.Size()
}

// MAC is guaranteed to take constant time, as long as
// len(seq)+len(header)+len(data)+len(extra) is constant. extra is not fed into
// the MAC, but is only provided to make the timing profile constant.
func (s tls10MAC__t) MAC(seq, header, data, extra []byte) []byte {
  s.h.Reset()
  s.h.Write(seq)
  s.h.Write(header)
  s.h.Write(data)
  res := s.h.Sum(s.buf[:0])
  if extra != nil {
    s.h.Write(extra)
  }
  return res
}

// utlsMacSHA384 returns a SHA-384 based MAC. These are only supported in TLS 1.2
// so the given version is ignored.
func utlsMacSHA384(version uint16, key []byte) macFunction {
	return tls10MAC__t{h: hmac.New(sha512.New384, key)}
}

var utlsSupportedCipherSuites []*cipherSuite

func init() {
	utlsSupportedCipherSuites = append(cipherSuites, []*cipherSuite{
		{OLD_TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256, 32, 0, 12, ecdheRSAKA,
			suiteECDHE | suiteTLS12 | suiteDefaultOff, nil, nil, aeadChaCha20Poly1305},
		{OLD_TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256, 32, 0, 12, ecdheECDSAKA,
			suiteECDHE | suiteECSign | suiteTLS12 | suiteDefaultOff, nil, nil, aeadChaCha20Poly1305},
	}...)
}

// EnableWeakCiphers allows utls connections to continue in some cases, when weak cipher was chosen.
// This provides better compatibility with servers on the web, but weakens security. Feel free
// to use this option if you establish additional secure connection inside of utls connection.
// This option does not change the shape of parrots (i.e. same ciphers will be offered either way).
// Must be called before establishing any connections.
//func EnableWeakCiphers() {
//	utlsSupportedCipherSuites = append(cipherSuites, []*cipherSuite{
//		{DISABLED_TLS_RSA_WITH_AES_256_CBC_SHA256, 32, 32, 16, rsaKA,
//			suiteTLS12 | suiteDefaultOff, cipherAES, macSHA256, nil},
//
//		{DISABLED_TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384, 32, 48, 16, ecdheECDSAKA,
//			suiteECDHE | suiteECSign | suiteTLS12 | suiteDefaultOff | suiteSHA384, cipherAES, utlsMacSHA384, nil},
//		{DISABLED_TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384, 32, 48, 16, ecdheRSAKA,
//			suiteECDHE | suiteTLS12 | suiteDefaultOff | suiteSHA384, cipherAES, utlsMacSHA384, nil},
//	}...)
//}
