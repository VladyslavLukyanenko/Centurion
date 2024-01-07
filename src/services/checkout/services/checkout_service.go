package services

import (
  "fmt"
  "github.com/CenturionLabs/centurion/checkout-service/core"
  "github.com/CenturionLabs/centurion/checkout-service/sites/common"
  "github.com/CenturionLabs/centurion/checkout-service/util"
  "go.elastic.co/apm"
)

type CheckoutService interface {
	Execute(ctx *core.CheckoutPayload, task core.CheckoutTask) error
}

type genericCheckoutService struct {
}

func NewCheckoutService() CheckoutService {
	return &genericCheckoutService{}
}

func (g *genericCheckoutService) Execute(payload *core.CheckoutPayload, task core.CheckoutTask) error {
	steps, err := task.GetCheckoutSteps()
	tx := apm.TransactionFromContext(payload.Context)
	if err != nil {
    payload.ReportUnexpectedError(err)
		util.ReportErrorWithTransaction(payload.Context, err, tx)
		return err
	}

	for len(steps) > task.Step() {
		span, ctx := apm.StartSpan(payload.Context, fmt.Sprintf("step_%d", task.Step()), "step")
		step := steps[task.Step()]

		if cancelErr := payload.Context.Err(); cancelErr != nil {
			util.ReportErrorWithSpan(ctx, cancelErr, span)
			span.End()
			return cancelErr
		}

		var result *core.StepExecutionFailure
		core.ExecuteWithRecovery(&err, func() {
			result = step()
		})
		if err != nil {
			payload.ReportError(common.TaskStatus_PanicFatal(err))
			util.ReportErrorWithSpan(ctx, err, span)
			span.End()
			return err
		}

		if result != nil && result.HasFatalError {
			util.ReportErrorWithSpan(ctx, result.Error, span)
			span.End()
			return result.Error
		}

		if cancelErr := payload.Context.Err(); cancelErr != nil {
			util.ReportErrorWithSpan(ctx, cancelErr, span)
			span.End()
			return cancelErr
		}
    //
		//if result != nil && result.Error != nil && payload.Task.Delay.AsDuration() > 0 {
		//	time.Sleep(payload.Task.Delay.AsDuration())
		//}
		span.End()
	}

	return nil
}
