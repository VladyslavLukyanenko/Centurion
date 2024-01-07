package services

import (
	"context"
	"github.com/go-redis/redis/v8"
  log "github.com/sirupsen/logrus"
	"strconv"
)

type Subscription interface {
	Dispose()
	EventSupplier() <-chan MonitoringStatus
}

type pubSubService struct {
	client *redis.Client
}
//
//func (p *pubSubService) Publish(nextStatus monitoring.STATUS, channelName string) {
//	p.client.Publish(context.Background(), channelName, nextStatus)
//}

type subscription struct {
	ctx      context.Context
	cancelFn context.CancelFunc
	supplier <-chan MonitoringStatus
}

func (s *subscription) Dispose() {
	s.cancelFn()
}

func (s *subscription) EventSupplier() <-chan MonitoringStatus {
	return s.supplier
}

func (p *pubSubService) Subscribe(channelName string) (Subscription, error) {
	ctx, cancelFn := context.WithCancel(context.Background())
	pubsub := p.client.Subscribe(ctx, channelName)

	_, err := pubsub.Receive(ctx)
	if err != nil {
		cancelFn()
		return nil, err
	}

	supplier := make(chan MonitoringStatus)
	ch := pubsub.Channel()
	go func() {
		for {
			select {
			case <-ctx.Done():
				_ = pubsub.Close()
				return
			case evt := <-ch:
				status, err := strconv.Atoi(evt.Payload)
				if err != nil {
					log.Fatal(err)
				}

				supplier <- MonitoringStatus(status)

			}
		}
	}()

	subscr := &subscription{
		ctx:      ctx,
		cancelFn: cancelFn,
		supplier: supplier,
	}
	return subscr, nil
}

type MonitoringStatus int32

const (
	Available MonitoringStatus = 0
	Unavailable = 1
)

type PubSubService interface {
	Subscribe(channelName string) (Subscription, error)
}

func NewPubSubService(client *redis.Client) PubSubService {
	return &pubSubService{client: client}
}
