package core

import (
  "context"
  "errors"
  contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
  "github.com/CenturionLabs/centurion/checkout-service/contracts/checkout"
  "github.com/CenturionLabs/centurion/checkout-service/util"
  "github.com/sirupsen/logrus"
  "go.elastic.co/apm"
)

type CheckoutPayload struct {
	*checkout.InitializedCheckoutTaskData
	Context          context.Context
	ProgressConsumer chan *contract.TaskStatusData
}

type StepExecutionFailure struct {
	Error         error
	HasFatalError bool
}

type CheckoutStep func() *StepExecutionFailure

type CheckoutTask interface {
	FetchProduct() (*contract.ProductData, error)
	GetCheckoutSteps() ([]CheckoutStep, error)
	Step() int
	GetUsedProfile() *contract.ProfileData
}

func (c *CheckoutPayload) ReportError(s *contract.TaskStatusData) *StepExecutionFailure {
	defer func() {
		e := recover()
		if e != nil {
			logrus.Errorln(e)
		}
	}()

	c.ProgressConsumer <- s
	err := s.Title
	if s.Description != nil && len(*s.Description) > 0 {
		err = *s.Description
	}

	reportedError := errors.New(err)
	if s.Category == contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR {
		return stepExecutionFatal(reportedError)
	}

	if span := apm.SpanFromContext(c.Context); span != nil {
		util.ReportErrorWithSpan(c.Context, reportedError, span)
	} else if tx := apm.TransactionFromContext(c.Context); tx != nil {
		util.ReportErrorWithTransaction(c.Context, reportedError, tx)
	}

	return stepExecutionFail(reportedError)
}

func (c *CheckoutPayload) ReportUnexpectedError(err error) *StepExecutionFailure {
	s := NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FATAL_ERROR, err.Error(), contract.TaskStage_TASK_STAGE_IDLE)

	return c.ReportError(s)
}

func (c *CheckoutPayload) ReportUnexpectedFailure(err error) *StepExecutionFailure {
	s := NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_FAILED, err.Error(), contract.TaskStage_TASK_STAGE_ERROR)

	return c.ReportError(s)
}

func (c *CheckoutPayload) ReportInProgress(message string) *StepExecutionFailure {
	s := NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED, message, contract.TaskStage_TASK_STAGE_RUNNING)
	c.ProgressConsumer <- s

	return nil
}

func (c *CheckoutPayload) ReportCarted(message string) *StepExecutionFailure {
  s := NewTaskStatus(contract.TaskCategory_TASK_CATEGORY_CARTED, message, contract.TaskStage_TASK_STAGE_CHECKING_OUT)
	c.ProgressConsumer <- s

	return nil
}

func (c *CheckoutPayload) ReportCheckingOutUnspecified(message string) *StepExecutionFailure {
  return c.ReportCheckingOut(message, contract.TaskCategory_TASK_CATEGORY_UNSPECIFIED)
}

func (c *CheckoutPayload) ReportCheckingOut(message string, category contract.TaskCategory) *StepExecutionFailure {
  s := NewTaskStatus(category, message, contract.TaskStage_TASK_STAGE_CHECKING_OUT)
	c.ProgressConsumer <- s

	return nil
}

func stepExecutionFatal(err error) *StepExecutionFailure {
	return &StepExecutionFailure{Error: err, HasFatalError: true}
}

func stepExecutionFail(err error) *StepExecutionFailure {
	return &StepExecutionFailure{Error: err}
}
