package services

import (
	"errors"
	"fmt"
	contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
	"github.com/CenturionLabs/centurion/checkout-service/core"
	"reflect"
	"strings"
)

type CheckoutTaskFactoryImpl func(payload *core.CheckoutPayload) (core.CheckoutTask, error)
type CheckoutTaskFactory interface {
	Create(ctx *core.CheckoutPayload) (core.CheckoutTask, error)
	RegisterModule(moduleName contract.Module, configType reflect.Type, taskFactory CheckoutTaskFactoryImpl)
	SupportedModules() []*contract.ModuleMetadata
}

type checkoutTaskFactory struct {
	registry          map[string]*modeInfo
	moduleDataFactory ModuleMetadataFactory
}

type modeInfo struct {
	taskFactory CheckoutTaskFactoryImpl
	configType  reflect.Type
	metadata    *contract.ModuleMetadata
}

func NewCheckoutTaskFactory(metaFactory ModuleMetadataFactory) CheckoutTaskFactory {
	return &checkoutTaskFactory{
		registry:          make(map[string]*modeInfo),
		moduleDataFactory: metaFactory,
	}
}

func (c *checkoutTaskFactory) Create(ctx *core.CheckoutPayload) (core.CheckoutTask, error) {
	factoryKey := c.createFactoryKey(ctx.Module)
	if f, ok := c.registry[factoryKey]; ok {
		return f.taskFactory(ctx)
	}

	return nil, errors.New(fmt.Sprintf("Not supported module '%v'", ctx.Module))
}

func (c *checkoutTaskFactory) createFactoryKey(module contract.Module) string {
	return strings.ToLower(module.String())
}

func (c *checkoutTaskFactory) RegisterModule(moduleName contract.Module, configType reflect.Type, taskFactory CheckoutTaskFactoryImpl) {
	c.registry[c.createFactoryKey(moduleName)] = &modeInfo{
		taskFactory: taskFactory,
		configType:  configType,
		metadata:    c.moduleDataFactory.CreateFor(configType),
	}
}

func (c *checkoutTaskFactory) SupportedModules() []*contract.ModuleMetadata {
	data := make([]*contract.ModuleMetadata, 0, len(c.registry))
	for _, v := range c.registry {
		data = append(data, v.metadata)
	}

	return data
}
