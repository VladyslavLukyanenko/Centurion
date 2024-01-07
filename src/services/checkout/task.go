package main
//
//import (
//	"github.com/ProjectIndustries/tasks-service/src/core"
//	"github.com/shopspring/decimal"
//	"time"
//)
//
//var (
//	TaskMap map[string]*Task
//	Sessions *ManagedSessionGroup
//	SubscribeMap *SubscribeMap
//	GroupStatisticMap *GroupStatisticMap
//	CaptchaManager interface{}
//)
//
//type Task struct {
//	ID           string
//	GroupID      string
//	Site         string
//	Mode         string
//	Window       interface{} // puppeteer browser window
//	ProfileSetID string
//
//	ProfileId     string
//	Profile       *core.ManagedProfile
//	MonitorSetId  string
//	MonitorSet    ItemIterator
//	CheckoutSetId string
//	CheckoutSet   ItemIterator
//	Delay         int32
//	Sku           interface{}
//	Watchers      []interface{}
//	Identifier    string
//	StartTime     time.Time
//	EndTime       time.Time
//	//startInterval ReturnType<typeof setInterval> | undefined
//	//endTimeout ReturnType<typeof setTimeout> | undefined
//	JaClient       Client
//	Ua             string
//	CookieJar      CookieJar
//	CartJar        CookieJar
//	PrevStatus     TaskStatus
//	Status         TaskStatus
//	Step           int32
//	Sleeping       bool
//	HasFatalError  bool
//	StepFinished   bool
//	PxConfig       interface{}
//	Started        bool
//	TaskLog        []string
//	Steps          interface{}
//	CheckoutProxy  string
//	MonitorProxy   string
//	ProductTitle   string
//	ProductImage   string
//	ProductPrice   decimal.Decimal
//	CaptchaManager interface{}
//	Details        interface{}
//
//	/** GLOBAL CONSTANTS */
//	PresetMap map[string]string
//	Http2Map  map[string]interface{}
//	ChromeUa  string
//	SecUa     string
//}
