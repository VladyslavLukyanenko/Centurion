package walmart

import "regexp"

const (
	VersionWMTAPP     = "21.13.3"
	VersionAndroid    = "21.12"
	AppUserAgent      = "Walmart/2107091553 Walmart WMTAPP v" + VersionWMTAPP
	AndroidVersShort  = "Android v" + VersionAndroid
	AndroidVersWMTAPP = "Android v" + VersionAndroid + " WMTAPP"
)

var (
	f_Pattern_c = regexp.MustCompile("vid=(.*?)&")
  f_String_9 = ""

	DOMAIN_NAMES = []string{
		"www.reddit.com",
		"www.facebook.com",
		"t.co",
	}

	PRODUCT_CATEGORIES = []string{
		"ps5",
		"xbox",
		"cleaner",
		"clorox",
		"water",
		"toaster",
		"puzzle",
		"game",
		"table",
		"cards",
		"panini",
		"laundry",
		"pokemon",
		"watch",
		"keyboard",
		"phone",
		"apple",
		"laptop",
		"guitar",
		"shirt",
		"chair",
		"pan",
		"socks",
		"pool",
		"toothpaste",
		"lotion",
	}
)
