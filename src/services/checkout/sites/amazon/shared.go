package amazon

import (
	"net/url"
)

var regionMap = map[amazonRegion]string{
	"USA": ".com",
	"CA":  ".ca",
	"UK":  ".co.uk",
	"NL":  ".nl",
	"FR":  ".fr",
	"IT":  ".it",
	"DE":  ".de",
	"JP":  ".co.jp",
}

var csrfMap = map[amazonRegion]string{
	"USA": "106-",
	"CA":  "703-",
	"UK":  "204-",
	"NL":  "404-",
	"FR":  "404-",
	"IT":  "404-",
	"DE":  "304-",
}

var checkoutMap = map[amazonRegion]string{
	"USA": "By placing your order, you agree to Amazon",
	"CA":  "Place Your Order - Amazon.ca Checkout",
	"UK":  "By placing your order you agree to",
	"NL":  "Door je bestelling te plaatsen, ga je akkoord met de gebruiks- en verkoopvoorwaarden van Amazon",
	"FR":  "En passant votre commande, vous acceptez les",
	"IT":  "L'acquisto si ritiene completato solo con la ricezione dell'e-mail di conferma della spedizione. Effettuando l'ordine, accetti le",
	"DE":  "Indem Sie Ihre Bestellung aufgeben, stimmen Sie den",
}

var inStockItems = map[amazonRegion][]string{
	"USA": {"B07FZ8S74R", "B079QHML21", "B07KWNSTRR"},
	"CA":  {"B079QH9GG7", "B08C1TR9X6"},
	"UK":  {"B005EJFLEM", "B07ZZW3KJY", "B005KP747W"},
	"NL":  {"B08GY9NYRM", "B07FCMKK5X", "B07NQ5YGDW"},
	"FR":  {"B084DWG2VQ", "B07ZZVRWLK", "B07ZZVWB4L"},
	"IT":  {"B07ZZVWB4L", "B08C1KN5J2", "B089NS9JW2"},
	"DE":  {"B013ICNQLQ", "B00TYEL0YS", "B07ZZVWB4L"},
	"JP":  {"B079QRQTCR", "B007TFQ2QU", "B08344TKCX"},
}

func getBaseUrl(region amazonRegion) *url.URL {
	baseUrl, _ := url.Parse(getRawBaseUrl(region))
	return baseUrl
}

func getRawBaseUrl(region amazonRegion) string {
	rawBaseUrl := "https://www.amazon" + regionMap[region]
	return rawBaseUrl
}

func resolveAbsoluteUrlRaw(relativePath string, region amazonRegion) string {
	baseUrl := getRawBaseUrl(region)
	if relativePath[0] != '/' {
		relativePath = "/" + relativePath
	}

	absoluteUrl := baseUrl + relativePath
	return absoluteUrl
}

func resolveAbsoluteUrl(relativePath string, region amazonRegion) (*url.URL, error) {
	absoluteUrl := resolveAbsoluteUrlRaw(relativePath, region)
	return url.Parse(absoluteUrl)
}
