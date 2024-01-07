package yeezysupply

import (
  contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
  yscontract "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/config/yeezysupply"
  "github.com/CenturionLabs/centurion/checkout-service/core"
  "github.com/CenturionLabs/centurion/checkout-service/services"
  "github.com/CenturionLabs/centurion/checkout-service/util"
  "github.com/CenturionLabs/centurion/checkout-service/utls_presets"
  log "github.com/sirupsen/logrus"
  "net/url"
  "reflect"
  "sync/atomic"
  "time"
)

func Register(taskFactory services.CheckoutTaskFactory, dstrLock services.DistributedLockFactory,
	rpcManager services.RpcManagerFactory, captchaSolverProvider services.ReCaptchaSolverProvider) {
	factoryImpl := func(payload *core.CheckoutPayload) (core.CheckoutTask, error) {
		var proxyUrl *url.URL
		//if payload.Task.PreferredProxy != nil {
		//	var err error
		//	proxyUrl, err = url.Parse(*payload.Task.PreferredProxy)
		//	if err != nil {
		//		return nil, err
		//	}
		//}

		client := services.NewHttpClient(utls_presets.TlsIdChrome_95, proxyUrl)
		hawkClient := services.NewHttpClient(utls_presets.TlsIdChrome_95, proxyUrl)
		hawkClient.SetTimeout(time.Second * 5)

		logger := util.NewLogger(payload.Context, log.Fields{
			"task_id": payload.Id,
			"user_id": payload.UserId,
			"sku":     payload.Product.Sku,
			"site":    payload.Module.String(),
			//"mode":    payload.Task.Mode,
		})

		return newYeezySupplyTask(client, hawkClient, payload, dstrLock, rpcManager, logger, captchaSolverProvider), nil
	}

	cfgType := reflect.TypeOf((*yscontract.YeezySupplyConfig)(nil)).Elem()
	taskFactory.RegisterModule(contract.Module_YEEZY_SUPPLY, cfgType, factoryImpl)
}

func newYeezySupplyTask(httpClient services.HttpClient, hawkApiHttpClient services.HttpClient, ctx *core.CheckoutPayload, dstrLock services.DistributedLockFactory,
	rpcManager services.RpcManagerFactory, logger *log.Logger, captchaSolverProvider services.ReCaptchaSolverProvider) core.CheckoutTask {
	v, _ := taskCounters.LoadOrStore(ctx.UserId, new(displayTaskId))
	taskId := v.(*displayTaskId)
	nextId := atomic.AddUint64(&taskId.value, 1)

	return &yeezySupplyTask{
		displayTaskId:         nextId,
		captchaSolverProvider: captchaSolverProvider,
		http:                  httpClient,
		hawkHttp:              hawkApiHttpClient,
		dstrLock:              dstrLock,
		payload:               ctx,
		rpcManager:            rpcManager.Get(ctx.UserId),
		logger:                logger,
	}
}
