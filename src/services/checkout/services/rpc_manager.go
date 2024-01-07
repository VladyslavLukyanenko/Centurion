package services

import (
  "context"
  "errors"
  "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout"
  "github.com/google/uuid"
  log "github.com/sirupsen/logrus"
  "io"
  "sync"
  "time"
)

var (
	RpcTimeoutError      = errors.New("HarvesterInProgress: Waiting...")
	RpcCancelledError    = errors.New("rpc context cancelled")
	RpcNoConnectionError = errors.New("HarvesterError: No connection")
)

type RpcManagerFactory interface {
	Get(userId string) RpcManager
}

type RpcManager interface {
	Reset()
	LoopMessages(stream RpcMessageStream, ctx context.Context) error
	InitializeHarvester(userId string) (string, error)
	SolveCaptcha(userId, harvesterId, productUrl string) (*checkout.ReCaptchaToken, error)
	RequestSmsConfirmationCode(userId, phoneNumber, taskDisplayId string) (*string, error)
	AckSmsConfirmationCode(userId, phoneNumber, taskDisplayId string) error
	Solve3DS2(userId, userAgent, formMethod, formAction, encodedData, termUrl, proxyUrl string, formFields map[string]string) (map[string]string, error)
}

func NewRpcManager() RpcManager {
	return &rpcManager{
		awaiters: &sync.Map{},
	}
}

type rpcManagerFactory struct {
	mu       *sync.Mutex
	managers *sync.Map
}

func (r *rpcManagerFactory) Get(userId string) RpcManager {
	data, ok := r.managers.Load(userId)
	if !ok {
		r.mu.Lock()
		defer r.mu.Unlock()
		data, _ = r.managers.LoadOrStore(userId, NewRpcManager())
	}

	return data.(RpcManager)
}

func NewRpcManagerFactory() RpcManagerFactory {
	return &rpcManagerFactory{
		managers: &sync.Map{},
		mu:       &sync.Mutex{},
	}
}

type rpcReplyAwaiter struct {
	userId string
	out    chan *checkout.RpcMessage
	cancel chan *struct{}
}

type rpcManager struct {
	awaiters *sync.Map
	stream   RpcMessageStream
}

type RpcMessageStream interface {
	Recv() (*checkout.RpcMessage, error)
	Send(message *checkout.RpcMessage) error
}

func (m *rpcManager) Reset() {
	m.awaiters.Range(func(key, value interface{}) bool {
		value.(*rpcReplyAwaiter).cancel <- &struct{}{}
		return true
	})

	m.awaiters = &sync.Map{}
}

func (m *rpcManager) LoopMessages(stream RpcMessageStream, ctx context.Context) error {
	m.stream = stream
	for ctx.Err() == nil {
		for {
			//log.Debugln("Waiting for message")
			in, err := stream.Recv()
			if err == io.EOF {
				log.Println(err)
				time.Sleep(1 * time.Second)
				return nil
			}

			if err != nil {
				log.Println(err)
				time.Sleep(1 * time.Second)
				return err
			}

			awaiterBoxed, ok := m.awaiters.LoadAndDelete(in.SessionId)
			if !ok {
				// todo: log, entry not found
				//log.Debugln("Entry not found for session " + in.SessionId)
				continue
			}

			awaiter := awaiterBoxed.(*rpcReplyAwaiter)
			//log.Debugln("Sending reply for session " + in.SessionId)
			awaiter.out <- in
			//log.Debugln("Reply sent for session " + in.SessionId)
		}
	}

	return nil
}

func (m *rpcManager) InitializeHarvester(userId string) (string, error) {
	//siteKey := "6Lf34M8ZAAAAANgE72rhfideXH21Lab333mdd2d-"
	//action := "yzysply_wr_pageview"

	r, err := m.sendWithDefaultTimeout(userId, &checkout.RpcMessage{
		Payload: &checkout.RpcMessage_Init{
			Init: &checkout.InitHarvesterCommand{
				//SiteKey: &siteKey,
				//Action:  &action,
			},
		},
	})

  if failure := r.GetActionError(); failure != nil {
    return "", errors.New("Harvesters: Init Failed")
  }

	return r.GetInitReply().HarvesterId, err
}

func (m *rpcManager) SolveCaptcha(userId, harvesterId, productUrl string) (*checkout.ReCaptchaToken, error) {
	reply, err := m.sendWithDefaultTimeout(userId, &checkout.RpcMessage{
		Payload: &checkout.RpcMessage_SolveCaptcha{
			SolveCaptcha: &checkout.SolveCaptchaHarvesterCommand{
				HarvesterId: harvesterId,
				ProductUrl: productUrl,
			},
		},
	})

	if err != nil {
		return nil, err
	}

	return reply.GetSolveCaptchaReply().Token, nil
}

func (m *rpcManager) AckSmsConfirmationCode(userId, phoneNumber, taskDisplayId string) error {
	err := m.sendNoWait(userId, &checkout.RpcMessage{
		Payload: &checkout.RpcMessage_SmsConfirmationAck{
			SmsConfirmationAck: &checkout.SmsConfirmationCommandReplyAck{
				PhoneNumber:   phoneNumber,
				DisplayTaskId: taskDisplayId,
			},
		},
	})

	if err != nil {
		return err
	}

	return nil
}

func (m *rpcManager) Solve3DS2(userId, userAgent, formMethod, formAction, encodedData, termUrl, proxyUrl string, formFields map[string]string) (map[string]string, error) {
	reply, err := m.sendWithTimeout(userId, &checkout.RpcMessage{
		Payload: &checkout.RpcMessage_Solve_3Ds2{
			Solve_3Ds2: &checkout.Solve3DS2Command{
				UserAgent:   userAgent,
				FormMethod:  formMethod,
				FormAction:  formAction,
				FormFields:  formFields,
				EncodedData: encodedData,
				TermUrl:     termUrl,
				ProxyUrl:    proxyUrl,
			},
		},
	}, 2*time.Minute)

	if err != nil {
		return nil, err
	}

	if reply.GetSolve_3Ds2Reply().Payload == nil {
		return nil, errors.New("Failed to solve 3DS2")
	}

	return reply.GetSolve_3Ds2Reply().Payload, nil
}

func (m *rpcManager) RequestSmsConfirmationCode(userId, phoneNumber, taskDisplayId string) (*string, error) {
	reply, err := m.sendWithDefaultTimeout(userId, &checkout.RpcMessage{
		Payload: &checkout.RpcMessage_SmsConfirmation{
			SmsConfirmation: &checkout.SmsConfirmationCommand{
				PhoneNumber:   phoneNumber,
				DisplayTaskId: taskDisplayId,
			},
		},
	})

	if err != nil {
		return nil, err
	}

	_ = m.AckSmsConfirmationCode(userId, phoneNumber, taskDisplayId)
	log.Debugln("Sms confirmation ACK DONE")

	return &reply.GetSmsConfirmationReply().SmsCode, nil
}

func (m *rpcManager) sendWithDefaultTimeout(userId string, msg *checkout.RpcMessage) (*checkout.RpcMessage, error) {
	return m.sendWithTimeout(userId, msg, 15*time.Second)
}

func (m *rpcManager) sendWithTimeout(userId string, msg *checkout.RpcMessage, timeout time.Duration) (*checkout.RpcMessage, error) {
	awaiter := &rpcReplyAwaiter{
		out:    make(chan *checkout.RpcMessage),
		cancel: make(chan *struct{}),
		userId: userId,
	}

	sessId := uuid.NewString()
	m.awaiters.Store(sessId, awaiter)
	defer m.awaiters.Delete(sessId)

	msg.SessionId = sessId

	if m.stream == nil {
		return nil, RpcNoConnectionError
	}

	err := m.stream.Send(msg)
	if err != nil {
		return nil, err
	}

	select {
	case reply := <-awaiter.out:
		return reply, nil
	case <-awaiter.cancel:
		return nil, RpcCancelledError
	case <-time.After(timeout):
		m.awaiters.Delete(msg.SessionId)
		return nil, RpcTimeoutError
	}
}
func (m *rpcManager) sendNoWait(userId string, msg *checkout.RpcMessage) error {
	msg.SessionId = uuid.NewString()
	if m.stream == nil {
		return RpcNoConnectionError
	}

	return m.stream.Send(msg)
}
