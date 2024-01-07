package integration

import (
	"fmt"
	"github.com/CenturionLabs/centurion/checkout-service/contracts"
	"github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/integration"
	"github.com/CenturionLabs/centurion/checkout-service/contracts/common"
  "github.com/CenturionLabs/centurion/checkout-service/core"
  "github.com/avast/retry-go/v3"
	"github.com/golang/protobuf/proto"
	"github.com/isayme/go-amqp-reconnect/rabbitmq"
	log "github.com/sirupsen/logrus"
	"github.com/streadway/amqp"
	"google.golang.org/protobuf/types/known/timestamppb"
	"time"
)

const (
	exchangeName = "events"
)

type EventsDispatcher interface {
	PublishCheckedOut(e *integration.ProductCheckedOut) error
	//PublishTaskStatusChange(userId, taskId, correlationId string, status *contracts.TaskStatusData) error
	PublishTerminated(userId string, taskId string) error
}

type rmqEventsDispatcher struct {
	connStr string
	channel *rabbitmq.Channel
	conn    *rabbitmq.Connection
}

func (r *rmqEventsDispatcher) PublishCheckedOut(e *integration.ProductCheckedOut) error {
	eventName := string(e.ProtoReflect().Descriptor().Name())
	routingKey := fmt.Sprintf("%s.%s", eventName, e.UserId)
	payload, err := proto.Marshal(e)
	if err != nil {
		return err
	}

	return r.publishEvent(routingKey, eventName, payload)
}

func (r *rmqEventsDispatcher) PublishTaskStatusChange(userId, taskId, correlationId string, status *contracts.TaskStatusData) error {
	event := &integration.CheckoutStatusChanged{
		Status: status,
		Meta: &common.EventMetadata{
			Timestamp: timestamppb.New(time.Now().UTC()),
			TaskId:    taskId,
			UserId:    &userId,
		},
	}
	payload, err := proto.Marshal(event)
	eventName := string(event.ProtoReflect().Descriptor().Name())
	routingKey := fmt.Sprintf("%s.%s.%s", eventName, userId, status.Category.String())

	if err != nil {
		return err
	}

	return r.publishEvent(routingKey, eventName, payload)
}

func (r *rmqEventsDispatcher) PublishTerminated(userId, taskId string) error {
	event := core.CreateTerminatedCheckoutStatus(taskId, userId)
	payload, err := proto.Marshal(event)
	eventName := string(event.ProtoReflect().Descriptor().Name())
	routingKey := fmt.Sprintf("%s.%s.%s", eventName, userId, event.Status.Category.String())

	if err != nil {
		return err
	}

	return r.publishEvent(routingKey, eventName, payload)
}

const retryCount = 5
const retryTimeout = time.Millisecond * 500

func (r *rmqEventsDispatcher) publishEvent(routingKey string, eventName string, payload []byte) error {
	attemptNo := 1
	for {
		err := r.channel.Publish(exchangeName, routingKey, false, false, amqp.Publishing{
			ContentType: "application/protobuf+" + eventName,
			Body:        payload,
		})

		if err == nil {
			return nil
		}

		if attemptNo >= retryCount {
			return err
		}

		_ = r.conn.Close()
		if r.conn.IsClosed() {
			conn, channel, err := establishAmqpConn(r.connStr)
			if err != nil {
				return err
			}

			r.conn = conn
			r.channel = channel
		}

		time.Sleep(retryTimeout * time.Duration(attemptNo))
		attemptNo++
	}
}

func NewRmqEventsDispatcher(amqpConnStr string) (EventsDispatcher, error) {
	conn, channel, err := establishAmqpConn(amqpConnStr)
	if err != nil {
		return nil, err
	}

	return &rmqEventsDispatcher{
		connStr: amqpConnStr,
		channel: channel,
		conn:    conn,
	}, nil
}

func establishAmqpConn(amqpConnStr string) (*rabbitmq.Connection, *rabbitmq.Channel, error) {
	var conn *rabbitmq.Connection
	err := retry.Do(
		func() error {
			var err error
			conn, err = rabbitmq.Dial(amqpConnStr)
			return err
		},
		retry.OnRetry(func(n uint, err error) {
			log.Printf("Failed to connect to rmq. Attempt: %d, cause: %s", n, err.Error())
		}),
	)

	if err != nil {
		return nil, nil, err
	}

	channel, err := conn.Channel()
	if err != nil {
		return nil, nil, err
	}

	err = channel.ExchangeDeclare(
		exchangeName,
		"topic",
		true,
		false,
		false,
		false,
		nil)
	if err != nil {
		return nil, nil, err
	}

	return conn, channel, nil
}
