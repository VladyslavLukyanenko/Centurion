package services

import (
	"context"
  "github.com/sirupsen/logrus"
  "golang.org/x/sync/semaphore"
	"sync"
	"time"
)

const inMemCacheLifetime = time.Millisecond * 50

type inMemBasedDistributedLockFactory struct {
	handles *sync.Map
	mutexes *sync.Map
}

func NewInMemLock() DistributedLockFactory {
  return &inMemBasedDistributedLockFactory{
    handles: &sync.Map{},
    mutexes: &sync.Map{},
  }
}

func (i *inMemBasedDistributedLockFactory) AcquireLockTimeout(mutexName string, ctx context.Context, duration time.Duration) (DistributedLockHandle, error) {
	withTimeout, cancel := context.WithTimeout(ctx, duration)
  defer cancel()
	return i.AcquireLock(mutexName, withTimeout)
}

func (i *inMemBasedDistributedLockFactory) AcquireLock(mutexName string, ctx context.Context) (DistributedLockHandle, error) {
	muBoxed, _ := i.mutexes.LoadOrStore(mutexName, semaphore.NewWeighted(1))
	sem := muBoxed.(*semaphore.Weighted)
	if err := sem.Acquire(ctx, 1); err != nil {
		return nil, err
	}

	handleBoxed, ok := i.handles.Load(mutexName)
	var handle *inMemLockHandle
	if ok {
		handle = handleBoxed.(*inMemLockHandle)
	} else {
		handle = &inMemLockHandle{
			lk:        i,
			mutexName: mutexName,
		}

    i.handles.Store(mutexName, handle)
	}

	handle.sem = sem // it will be null after each "Release"
	return handle, nil
}

type inMemLockHandle struct {
	lk             *inMemBasedDistributedLockFactory
	sem            *semaphore.Weighted
	mutexName      string
	clearScheduled bool
	sharedVal      []byte
}

func (i *inMemLockHandle) UnlockAndShare(val []byte, context context.Context) error {
	i.sharedVal = val[:]
	return i.Unlock(context)
}

func (i *inMemLockHandle) SharedValue() []byte {
	return i.sharedVal
}

func (i *inMemLockHandle) Unlock(_ context.Context) error {
  defer func() {
    e := recover()
    if e != nil {
      logrus.Errorln(e)
    }
  }()

	sem := i.sem
	if sem == nil {
		return nil
	}

  if !i.clearScheduled {
    go func() {
      time.Sleep(inMemCacheLifetime)
      i.lk.handles.Delete(i.mutexName)
      logrus.Debugln("Cleared cache for distr lock: " + i.mutexName)
    }()
    i.clearScheduled = true
  }

	i.sem = nil // NOTICE: to not release more than we acquired
	sem.Release(1)
	return nil
}
