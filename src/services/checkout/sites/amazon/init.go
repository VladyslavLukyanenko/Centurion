package amazon

import (
  contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
  "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/config/amazon"
  "github.com/CenturionLabs/centurion/checkout-service/contracts/monitor"
	"github.com/CenturionLabs/centurion/checkout-service/core"
	"github.com/CenturionLabs/centurion/checkout-service/services"
  "github.com/CenturionLabs/centurion/checkout-service/utls_presets"
  "net/url"
  "reflect"
)

func Register(taskFactory services.CheckoutTaskFactory, dstrLock services.DistributedLockFactory, monitorClient monitor.MonitorClient) {

	inStockItemsProvider := NewInStockItemsProvider()
	factoryImpl := func(ctx *core.CheckoutPayload) (core.CheckoutTask, error) {
		var proxyUrl *url.URL

		client := services.NewHttpClient(utls_presets.TlsIdChrome_95, proxyUrl)
		return newAmazonTask(client, inStockItemsProvider, ctx, dstrLock, monitorClient), nil
	}

  cfgType := reflect.TypeOf((*amazon.AmazonConfig)(nil)).Elem()
	taskFactory.RegisterModule(contract.Module_AMAZON, cfgType, factoryImpl)
}
