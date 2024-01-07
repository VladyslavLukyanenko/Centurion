package web

import (
	"context"
	"errors"
	"github.com/CenturionLabs/centurion/checkout-service/contracts"
	contractgrpc "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout"
	integration2 "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/integration"
	"github.com/CenturionLabs/centurion/checkout-service/core"
	"github.com/CenturionLabs/centurion/checkout-service/services"
	"github.com/reactivex/rxgo/v2"
	log "github.com/sirupsen/logrus"
	"runtime"
	"sync"
	"time"
)

type CheckoutCommandStream interface {
	Send(*contractgrpc.CheckoutStatusChangedBatch) error
	Recv() (*contractgrpc.CheckoutCommand, error)
}

type MutableCheckoutUserContext interface {
	CheckoutUserContext

	GetRunningTasks() *sync.Map

	SetCheckoutConn(stream CheckoutCommandStream)
	ResetCheckoutConn()

	SetRpcConn(stream services.RpcMessageStream)
	ResetRpcConn()
}

type CheckoutUserContext interface {
	GetUserId() string
	RecvCommand() (*contractgrpc.CheckoutCommand, error)

	SendEvent(e *integration2.CheckoutStatusChanged)

	StoreTask(t *core.SpawnedTask)
	DeleteTask(taskId string)
	HasTask(taskId string) bool
	GetTask(taskId string) *core.SpawnedTask
}

type checkoutUserContext struct {
	userId       string
	checkoutConn CheckoutCommandStream
	rpcConn      services.RpcMessageStream
	runningTasks *sync.Map
	eventsChan   chan rxgo.Item
	eventsStream rxgo.Observable
	disposeFn    context.CancelFunc
}

func newCheckoutContext(userId string) MutableCheckoutUserContext {
	eventsChan := make(chan rxgo.Item)
	var eventsStream = rxgo.FromChannel(eventsChan).
		OnErrorResumeNext(func(err error) rxgo.Observable {
			log.Errorln(err)
			return rxgo.Empty()
		}).
		BufferWithTimeOrCount(rxgo.WithDuration(time.Millisecond*200), 1000, rxgo.WithBackPressureStrategy(rxgo.Drop)).
		Filter(func(i interface{}) bool {
			items := i.([]interface{})
			return len(items) > 0
		})

	lifetime, disposeFn := context.WithCancel(context.Background())

	checkoutCtx := &checkoutUserContext{
		eventsChan:   eventsChan,
		eventsStream: eventsStream,
		disposeFn:    disposeFn,
		userId:       userId,
		runningTasks: &sync.Map{},
	}

	go func() {
		for lifetime.Err() == nil {
			checkoutCtx.processEventStream()
		}
	}()

	return checkoutCtx
}

func (c *checkoutUserContext) GetRunningTasks() *sync.Map {
	return c.runningTasks
}

func (c *checkoutUserContext) SetCheckoutConn(stream CheckoutCommandStream) {
	c.checkoutConn = stream
}
func (c *checkoutUserContext) ResetCheckoutConn() {
	c.checkoutConn = nil
}

func (c *checkoutUserContext) SetRpcConn(stream services.RpcMessageStream) {
	c.rpcConn = stream
}
func (c *checkoutUserContext) ResetRpcConn() {
	c.rpcConn = nil
}

func (c *checkoutUserContext) RecvCommand() (*contractgrpc.CheckoutCommand, error) {
	return c.checkoutConn.Recv()
}

func (c *checkoutUserContext) GetUserId() string {
	return c.userId
}

func (c *checkoutUserContext) StoreTask(t *core.SpawnedTask) {
	c.runningTasks.Store(t.Payload.Id, t)
}

func (c *checkoutUserContext) DeleteTask(taskId string) {
	c.runningTasks.Delete(taskId)
}

func (c *checkoutUserContext) HasTask(taskId string) bool {
	_, has := c.runningTasks.Load(taskId)
	return has
}

func (c *checkoutUserContext) GetTask(taskId string) *core.SpawnedTask {
	running, exists := c.runningTasks.Load(taskId)
	if exists {
		return running.(*core.SpawnedTask)
	}

	return nil
}

func (c *checkoutUserContext) processEventStream() {
	defer func() {
		e := recover()
		if e != nil {
			log.Errorln(e)
		}
	}()

	for e := range c.eventsStream.Observe() {
		if c.checkoutConn == nil {
			runtime.Gosched()
			log.WithField("userId", c.userId).Errorln(errors.New("no active connection to send status change event"))
			time.Sleep(time.Millisecond * 500)
			continue
		}

		go c.sendBatch(e)
	}
}

func (c *checkoutUserContext) sendBatch(e rxgo.Item) {
	if c.checkoutConn == nil {
		runtime.Gosched()
		log.WithField("userId", c.userId).Errorln(errors.New("no active connection to send status change event"))
		return
	}

	events := e.V.([]interface{})
	response := groupChangesByUserAndTask(events)

	//eventWriterMu.Lock()
	//defer eventWriterMu.Unlock()
	err := c.checkoutConn.Send(response)
	if err != nil {
		log.Errorln(err)
	}
}

func groupChangesByUserAndTask(events []interface{}) *contractgrpc.CheckoutStatusChangedBatch {
	list := map[string]*integration2.CheckoutStatusChanged{}

	var totalBatchSize = 0
	for ix := range events {
		evt := events[ix].(*integration2.CheckoutStatusChanged)
		//println(evt.Status.Title + " " + evt.Meta.Timestamp.AsTime().String())

		if existing, found := list[evt.Meta.TaskId]; found {
			if evt.Status.Category > existing.Status.Category ||
				!existing.Meta.Timestamp.AsTime().After(evt.Meta.Timestamp.AsTime()) {
				list[evt.Meta.TaskId] = evt
			}
		} else {
			list[evt.Meta.TaskId] = evt
			totalBatchSize++
		}
	}

	changes := make(map[string]*contracts.TaskStatusData, len(list))
	for k, v := range list {
		changes[k] = v.Status
	}

	return &contractgrpc.CheckoutStatusChangedBatch{
		Changes: changes,
	}
}

func (c *checkoutUserContext) SendEvent(e *integration2.CheckoutStatusChanged) {
	c.eventsChan <- rxgo.Of(e)
}
