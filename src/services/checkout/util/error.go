package util

import (
  "context"
  "go.elastic.co/apm"
)

func ReportErrorWithSpan(ctx context.Context, err error, span *apm.Span) {
  e := apm.CaptureError(ctx, err)
  e.SetSpan(span)
  e.Send()
}

func ReportErrorWithTransaction(ctx context.Context, err error, transaction *apm.Transaction) {
  e := apm.CaptureError(ctx, err)
  e.SetTransaction(transaction)
  e.Send()
}