package web

import (
	"context"
	"errors"
	"fmt"
	contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
	contractgrpc "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout"
	integration2 "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/integration"
	"github.com/CenturionLabs/centurion/checkout-service/contracts/common"
	"github.com/CenturionLabs/centurion/checkout-service/core"
	"github.com/CenturionLabs/centurion/checkout-service/integration"
	"github.com/CenturionLabs/centurion/checkout-service/services"
	"github.com/CenturionLabs/centurion/checkout-service/util"
	log "github.com/sirupsen/logrus"
	"go.elastic.co/apm"
	"google.golang.org/grpc/metadata"
	status2 "google.golang.org/grpc/status"
	"google.golang.org/protobuf/proto"
	"google.golang.org/protobuf/reflect/protoreflect"
	"google.golang.org/protobuf/reflect/protoregistry"
	"google.golang.org/protobuf/types/known/emptypb"
	"google.golang.org/protobuf/types/known/timestamppb"
	"io"
	"runtime"
	"sync"
	"time"
)

var (
	HeaderNameUserId string
)

func init() {
	descriptor, err := protoregistry.GlobalFiles.FindDescriptorByName("checkout.Checkout")
	if err != nil {
		panic(err)
	}

	serviceDescriptor := descriptor.(protoreflect.ServiceDescriptor)
	extension := proto.GetExtension(serviceDescriptor.Options(), contractgrpc.E_CenturionHeaderUserId)
	HeaderNameUserId = extension.(string)
}

func NewCheckoutServer(checkoutSvc services.CheckoutService, eventDispatcher integration.EventsDispatcher,
	taskFactory services.CheckoutTaskFactory, rpcManagerFactory services.RpcManagerFactory) contractgrpc.CheckoutServer {
	return &checkoutServer{
		contexts: map[string]MutableCheckoutUserContext{},
		mu:       &sync.Mutex{},

		checkoutSvc:       checkoutSvc,
		taskFactory:       taskFactory,
		eventDispatcher:   eventDispatcher,
		rpcManagerFactory: rpcManagerFactory,
	}
}

type checkoutServer struct {
	contractgrpc.UnimplementedCheckoutServer

	contexts map[string]MutableCheckoutUserContext
	mu       *sync.Mutex

	checkoutSvc       services.CheckoutService
	taskFactory       services.CheckoutTaskFactory
	eventDispatcher   integration.EventsDispatcher
	rpcManagerFactory services.RpcManagerFactory
}

func (s *checkoutServer) ConnectCheckout(conn contractgrpc.Checkout_ConnectCheckoutServer) error {
	userId, err := extractUserId(conn.Context())
	if err != nil {
		return err
	}

	ctx := s.getOrCreateCtx(userId)
	ctx.SetCheckoutConn(conn)
	defer ctx.ResetCheckoutConn()

	return s.spawnCheckoutExecutor(ctx)
}

func extractUserId(grpcCtx context.Context) (string, error) {
	var userId string
	if meta, ok := metadata.FromIncomingContext(grpcCtx); !ok {
		return "", errors.New("No headers was sent")
	} else {
		if userIdHeaders := meta.Get(HeaderNameUserId); len(userIdHeaders) == 0 {
			return "", errors.New("No userId was provided")
		} else {
			userId = userIdHeaders[0]
		}
	}

	return userId, nil
}

func (s *checkoutServer) ConnectRpc(conn contractgrpc.Checkout_ConnectRpcServer) error {
	userId, err := extractUserId(conn.Context())
	if err != nil {
		return err
	}

	ctx := s.getOrCreateCtx(userId)
	ctx.SetRpcConn(conn)
	defer ctx.ResetRpcConn()

	rpc := s.rpcManagerFactory.Get(userId)

	_ = rpc.LoopMessages(conn, conn.Context())
	rpc.Reset()

	return nil
}

func (s *checkoutServer) ForceStop(_ context.Context, cmd *contractgrpc.ForceStopCheckoutCommand) (*emptypb.Empty, error) {
	checkoutCtx := s.getOrCreateCtx(cmd.UserId)
	for _, details := range cmd.Cmd.Tasks {
		s.cancelTask(checkoutCtx, details.Id)
	}

	return new(emptypb.Empty), nil
}

func (s *checkoutServer) GetSupportedModules(context.Context, *emptypb.Empty) (*contractgrpc.SupportedModuleList, error) {
	list := &contractgrpc.SupportedModuleList{
		Modules: s.taskFactory.SupportedModules(),
	}

	return list, nil
}

func (s *checkoutServer) FetchProduct(ctx context.Context, cmd *contract.FetchProductCommand) (*contract.ProductData, error) {
	progress := make(chan *contract.TaskStatusData, 1)
	completed := make(chan bool)
	go func() {
		for {
			select {
			case <-completed:
				return
			case <-progress:
				runtime.Gosched()
				continue

			}
		}
	}()

	payload := &core.CheckoutPayload{
		InitializedCheckoutTaskData: &contractgrpc.InitializedCheckoutTaskData{
			Id:          cmd.Sku,
			Module:      cmd.Module,
			ProfileList: nil,
			ProxyPool: &contract.ProxyPoolData{
				Id:      "default",
				Name:    "default",
				Proxies: cmd.Proxies,
			},
			Product: &contract.ProductData{
				Sku:    cmd.Sku,
				Name:   cmd.Sku,
				Image:  "",
				Link:   "",
				Module: cmd.Module,
				Price:  nil,
			},
			Config: nil,
			UserId: "",
		},
		Context:          ctx,
		ProgressConsumer: progress,
	}
	defer func() {
		completed <- true
	}()
	task, err := s.taskFactory.Create(payload)
	if err != nil {
		return nil, err
	}

	p, err := task.FetchProduct()
	if err != nil {
		log.WithFields(log.Fields{
			"sku":    cmd.Sku,
			"module": cmd.Module,
		}).Errorln(err)
	}

	return p, err
}

func (s *checkoutServer) GetTasksStats(context.Context, *emptypb.Empty) (*contractgrpc.TasksExecutingStats, error) {
	stats := &contractgrpc.TasksExecutingStats{
		PerUserCount: map[string]int32{},
		TotalCount:   0,
	}

	s.mu.Lock()
	defer s.mu.Unlock()
	for _, c := range s.contexts {
		c.GetRunningTasks().Range(func(key, value interface{}) bool {
			t := value.(*core.SpawnedTask)
			stats.TotalCount++
			if perUserCount, ok := stats.PerUserCount[t.UserId]; ok {
				stats.PerUserCount[t.UserId] = perUserCount + 1
			} else {
				stats.PerUserCount[t.UserId] = 1
			}

			return true
		})
	}

	return stats, nil
}

func (s *checkoutServer) getOrCreateCtx(userId string) MutableCheckoutUserContext {
	s.mu.Lock()
	defer s.mu.Unlock()
	if _, ok := s.contexts[userId]; !ok {
		s.contexts[userId] = newCheckoutContext(userId)
	}

	return s.contexts[userId]
}

func (s *checkoutServer) spawnCheckoutExecutor(checkoutCtx CheckoutUserContext) error {
	for {
		cmd, err := checkoutCtx.RecvCommand()
		if err == io.EOF {
			return nil
		}

		if err != nil {
			if status, ok := status2.FromError(err); ok {
				log.Printf("Error received: %s\n", status.Err().Error())
				return status.Err()
			}

			log.Println(err.Error())
			return err
		}
		if startCmd, ok := cmd.Command.(*contractgrpc.CheckoutCommand_Start); ok {
			start := startCmd.Start
			for ix := range start.Tasks {
				go s.startTask(checkoutCtx, start.Tasks[ix])
			}
		} else {
			var stopCmd = cmd.GetStop()
			for ix := range stopCmd.Tasks {
				stoppingTaskId := stopCmd.Tasks[ix].Id
				s.cancelTask(checkoutCtx, stoppingTaskId)
			}
		}
	}
}

func (s *checkoutServer) cancelTask(checkoutCtx CheckoutUserContext, stoppingTaskId string) {
	running := checkoutCtx.GetTask(stoppingTaskId)
	if running != nil {
		running.CancelFn()
	} else {
		checkoutCtx.DeleteTask(stoppingTaskId)
		event := core.CreateTerminatedCheckoutStatus(stoppingTaskId, checkoutCtx.GetUserId())
		checkoutCtx.SendEvent(event)
	}
}

func (s *checkoutServer) startTask(checkoutCtx CheckoutUserContext, task *contractgrpc.InitializedCheckoutTaskData) {
	txname := fmt.Sprintf("Checkout %s - '%s', '%s'", task.Module.String(), task.Product.Name, task.Product.Sku)
	tx := apm.DefaultTracer.StartTransaction /*Options*/ (txname, "request" /*, opts*/)
	tx.Context.SetUserID(task.UserId)

	/*log.WithFields(log.Fields{
		"taskId": task.Id,
		"GetUserId": task.UserId,
		"sku":    task.Product.Sku,
	}).Debugln("Creating task")*/
	ctx := apm.ContextWithTransaction(context.Background(), tx)

	alreadyRunning := checkoutCtx.HasTask(task.Id)
	if alreadyRunning {
		status := &contract.TaskStatusData{
			Title:    "Already Running",
			Category: contract.TaskCategory_TASK_CATEGORY_FAILED,
			Stage:    contract.TaskStage_TASK_STAGE_ERROR,
		}
		event := &integration2.CheckoutStatusChanged{
			Status: status,
			Meta: &common.EventMetadata{
				Timestamp: timestamppb.New(time.Now().UTC()),
				TaskId:    task.Id,
				//CorrelationId: correlationId,
				UserId: &task.UserId,
			},
		}

		checkoutCtx.SendEvent(event)

		err := errors.New("task already running")
		util.ReportErrorWithTransaction(ctx, err, tx)
		log.WithFields(log.Fields{
			"taskId": task.Id,
			"userId": task.UserId,
			"sku":    task.Product.Sku,
		}).Println(err)
		tx.End()
		//conn.SendMsg(err)
		return
	}

	ctx, cancelFn := context.WithCancel(ctx)

	progress := make(chan *contract.TaskStatusData, 1)
	payload := &core.CheckoutPayload{
		InitializedCheckoutTaskData: task,
		Context:                     ctx,
		ProgressConsumer:            progress,
	}

	checkoutTask, err := s.taskFactory.Create(payload)
	if err != nil {
		cancelFn()

		log.Errorln(err)
		status := &contract.TaskStatusData{
			Title:    "Failed. Check Mode",
			Category: contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR,
			Stage:    contract.TaskStage_TASK_STAGE_IDLE,
		}
		event := &integration2.CheckoutStatusChanged{
			Status: status,
			Meta: &common.EventMetadata{
				Timestamp: timestamppb.New(time.Now().UTC()),
				TaskId:    task.Id,
				UserId:    &task.UserId,
			},
		}

		checkoutCtx.SendEvent(event)
		return
	}

	spawnedTask := &core.SpawnedTask{
		Context:      ctx,
		CancelFn:     cancelFn,
		Payload:      payload,
		CheckoutTask: checkoutTask,
		Logger:       core.NewActivityLogger(),
		UserId:       task.UserId,
		//TlsIdName:        utls_presets.TlsIdChrome_95,
		StartTime: time.Now().UTC(),
	}

	completed := make(chan bool)

	checkoutCtx.StoreTask(spawnedTask)

	go func() {
		select {
		case <-completed:
			return
		/*case <-terminationCtx.Done():
		  spawnedTask.CancelFn()*/
		case <-spawnedTask.Context.Done():
			spawnedTask.CancelFn()
		}
	}()

	var lastStatus *contract.TaskStatusData
	go func() {
		for {
			select {
			case <-completed:
				return
			case nextStatus := <-spawnedTask.Payload.ProgressConsumer:
				if nextStatus == nil && spawnedTask.EndTime != nil {
					return // chan just closed
				} else if nextStatus == nil {
					runtime.Gosched()
					continue
				}

				lastStatus = nextStatus
				spawnedTask.Logger.Log(nextStatus.Title)
				event := &integration2.CheckoutStatusChanged{
					Status: nextStatus,
					Meta: &common.EventMetadata{
						Timestamp: timestamppb.New(time.Now().UTC()),
						UserId:    &task.UserId,
						TaskId:    task.Id,
					},
				}

				checkoutCtx.SendEvent(event)
			}
		}
	}()

	defer func() {
		srcErr := recover()
		if srcErr == nil {
			return
		}

		if e, ok := srcErr.(error); ok {
			err = e
		} else {
			e := errors.New("unexpected error")
			err = e
		}

		log.Errorln(err)
	}()

	/*log.WithFields(log.Fields{
		"taskId": task.Id,
		"GetUserId": task.UserId,
		"sku":    task.Product.Sku,
	}).Debugln("Executing task")*/

	err = s.checkoutSvc.Execute(payload, checkoutTask)
	endTime := time.Now().UTC()
	spawnedTask.EndTime = &endTime

	if err == nil && payload.Context.Err() == nil {
		// todo: use lastStatus to check if finished as checkedOut/declined/otherError

		checkedOutEvent := core.CreateProductCheckedOutEvent(spawnedTask)
		if pcf, ok := spawnedTask.CheckoutTask.(integration.ProductCheckedOutEventProcessor); ok {
			pcf.ProcessProductCheckedOutEvent(checkedOutEvent)
		}

		checkedOutEvent.ProcessingLog = spawnedTask.Logger.ToMessagesSlice()

		_ = s.eventDispatcher.PublishCheckedOut(checkedOutEvent)

		for ix, logEntry := range checkedOutEvent.ProcessingLog {
			tx.Context.SetLabel(fmt.Sprintf("log_%03d", ix), logEntry)
		}

		tx.Result = "OK"
	} else if err != context.Canceled {
		util.ReportErrorWithTransaction(ctx, err, tx)
		log.Println(err)
	}

	completed <- true
	checkoutCtx.DeleteTask(task.Id)
	tx.End()
	close(spawnedTask.Payload.ProgressConsumer)
	if err == context.Canceled || payload.Context.Err() != nil {
		_ = s.eventDispatcher.PublishTerminated(task.UserId, task.Id)
		event := core.CreateTerminatedCheckoutStatus(task.Id, task.UserId)

		checkoutCtx.SendEvent(event)
	}

	log.WithFields(log.Fields{
		"taskId": task.Id,
		"userId": task.UserId,
		"sku":    task.Product.Sku,
	}).Debugln("Task finished executing")
}
