package core

import (
  "errors"
  contract "github.com/CenturionLabs/centurion/checkout-service/contracts"
  "github.com/CenturionLabs/centurion/checkout-service/util"
  "go.elastic.co/apm"
)

func ReportErrorForSpan(a *CheckoutPayload, s *contract.TaskStatusData)  *StepExecutionFailure {
  span := apm.SpanFromContext(a.Context)
  util.ReportErrorWithSpan(a.Context, errors.New(s.Title), span)

  return a.ReportError(s)
}
