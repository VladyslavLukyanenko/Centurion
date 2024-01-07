package fake_shop

import (
  contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
  "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout/config/fakeshop"
  "github.com/CenturionLabs/centurion/checkout-service/core"
  "github.com/CenturionLabs/centurion/checkout-service/services"
  "github.com/CenturionLabs/centurion/checkout-service/utls_presets"
  "reflect"
)

func Register(taskFactory services.CheckoutTaskFactory, rpcManagerFactory services.RpcManagerFactory) {
  cfgType := reflect.TypeOf((*fakeshop.FakeShopConfig)(nil)).Elem()
  taskFactory.RegisterModule(contract.Module_FAKE_SHOP, cfgType, func(ctx *core.CheckoutPayload) (core.CheckoutTask, error) {
    client := services.NewHttpClient(utls_presets.TlsIdChrome_95, nil)
    return newFakeShopFast(ctx, client, rpcManagerFactory), nil
  })
}