package core

import (
	"context"
	"time"
)

type SpawnedTask struct {
	Context      context.Context
	CancelFn     context.CancelFunc
	Payload      *CheckoutPayload
	CheckoutTask CheckoutTask
	Logger       ActivityLogger
	UserId       string
	StartTime    time.Time
	EndTime      *time.Time
}
