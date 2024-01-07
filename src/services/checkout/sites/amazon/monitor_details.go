package amazon

import (
	"github.com/shopspring/decimal"
	"time"
)

type monitorDetails struct {
	Sku           string
	OfferId       string
	MaxPrice      decimal.Decimal
	Delay         time.Duration
	Region        amazonRegion
	MonitorMethod string
	TlsSetting    string
	Ua            string
	ResetDelay    time.Duration
}