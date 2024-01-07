package services

import (
  "context"
  "github.com/CenturionLabs/centurion/checkout-service/core"
  "github.com/go-redis/redis/v8"
  "github.com/go-redsync/redsync/v4"
  "github.com/go-redsync/redsync/v4/redis/goredis/v8"
  "github.com/sirupsen/logrus"
  "time"
)

const (
	distributedMutexExp   = time.Minute * 5 // math.MaxInt64
	distributedSharingExp = time.Millisecond * 500
)

type DistributedLockFactory interface {
	// todo: add ability to specify expiration time

	AcquireLockTimeout(mutexName string, context context.Context, duration time.Duration) (DistributedLockHandle, error)
	AcquireLock(mutexName string, context context.Context) (DistributedLockHandle, error)
}

type DistributedLockHandle interface {
	UnlockAndShare(val []byte, context context.Context) error
	SharedValue() []byte
	Unlock(ctx context.Context) error
}

func NewDistributedLockFactory(client *redis.Client) DistributedLockFactory {
	pool := goredis.NewPool(client)
	return &redisBasedDistributedLockFactory{
		client: client,
		rs:     redsync.New(pool),
	}
}

type redisBasedDistributedLockFactory struct {
	client *redis.Client
	rs     *redsync.Redsync
}

type redsyncLockHandle struct {
	mutex     *redsync.Mutex
	client    *redis.Client
	sharedVal []byte
}

func (r *redsyncLockHandle) SharedValue() []byte {
	return r.sharedVal
}

func (r *redsyncLockHandle) UnlockAndShare(val []byte, ctx context.Context) error {
	if r.sharedVal == nil {
		r.sharedVal = val
		r.client.Set(ctx, createSharedResultKey(r.mutex), val, distributedSharingExp)
	}

	return r.Unlock(ctx)
}

func (r *redsyncLockHandle) Unlock(ctx context.Context) error {
	_, err := r.mutex.UnlockContext(ctx)
	return err
}

func (r *redisBasedDistributedLockFactory) AcquireLock(mutexName string, context context.Context) (DistributedLockHandle, error) {
	return r.AcquireLockTimeout(mutexName, context, distributedMutexExp)
}

func (r *redisBasedDistributedLockFactory) AcquireLockTimeout(mutexName string, context context.Context, duration time.Duration) (DistributedLockHandle, error) {
	mutex := r.rs.NewMutex(mutexName, redsync.WithExpiry(duration))
	for {
		err := mutex.LockContext(context)
		if err == redsync.ErrFailed {
			continue
		}

		if err != nil {
			return nil, err
		}

		break
	}

	var val []byte
	resultKey := createSharedResultKey(mutex)
	if shared := r.client.Get(context, resultKey); shared != nil {
		v, err := shared.Bytes()
		if err == nil {
			val = v
		}
	}

	return &redsyncLockHandle{
		mutex:     mutex,
		client:    r.client,
		sharedVal: val,
	}, nil
}

func createSharedResultKey(mutex *redsync.Mutex) string {
	return "sharedResult__" + mutex.Name()
}

type ExecuteOnceAndShareConfig struct {
	MutexNameFactory func() string
	RawValueReceiver func(rawValue []byte) *core.StepExecutionFailure
	CheckoutPayload  *core.CheckoutPayload
	ValueProducer    func(consumer func(rawValue []byte) *core.StepExecutionFailure) *core.StepExecutionFailure
	DstrLock         DistributedLockFactory
	Timeout          *time.Duration
}

func (c *ExecuteOnceAndShareConfig) ExecuteOnceAndShare() *core.StepExecutionFailure {
	mutexName := c.MutexNameFactory()

	ctx := c.CheckoutPayload.Context
	var dHandle DistributedLockHandle

  done := make(chan interface{})
  defer func() {
    close(done)
  }()
	var err error
	if c.Timeout != nil {
		dHandle, err = c.DstrLock.AcquireLockTimeout(mutexName, ctx, *c.Timeout)
	} else {
		dHandle, err = c.DstrLock.AcquireLock(mutexName, ctx)
	}

  go func() {
    select {
    case <-done:
      if dHandle == nil {
        return
      }

      _ = dHandle.Unlock(context.Background()/* it shouldn't be cancelled */)
    case <-ctx.Done():
      if dHandle == nil {
        return
      }

      _ = dHandle.Unlock(context.Background()/* it shouldn't be cancelled */)
    }
  }()

	if err != nil {
		return c.CheckoutPayload.ReportUnexpectedFailure(err)
	}

	defer func() {
		perr := recover()
		if perr != nil {
			_ = dHandle.Unlock(ctx)
			//panic(perr)
      logrus.Errorln(perr)
		}
	}()

	if dHandle.SharedValue() != nil {
		r := c.RawValueReceiver(dHandle.SharedValue())
		_ = dHandle.Unlock(ctx)
		return r
	}

	return c.ValueProducer(func(rawValue []byte) *core.StepExecutionFailure {
		_ = dHandle.UnlockAndShare(rawValue, ctx)
		r := c.RawValueReceiver(dHandle.SharedValue())

		return r
	})
}
