package services

type SessionStatus string

const (
	SessionNotReady SessionStatus = "NOT_READY"
	SessionReady                  = "READY"
	SessionExpired                = "EXPIRED"
)

type ManagedSession struct {
	id         string
	AccountId  *string
	Cookies    []string
	StatusType SessionStatus
	StatusText string
	Extra      map[string]string
}
