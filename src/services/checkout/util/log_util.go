package util

import (
	"context"
	"github.com/sirupsen/logrus"
	"go.elastic.co/ecslogrus"
  "strings"
  "sync"
)

var logUtilMu = &sync.Mutex{}
type contextualLoggerFormatter struct {
	defaultFields logrus.Fields
	formatter     logrus.Formatter
  mu        *sync.Mutex
}

func (c *contextualLoggerFormatter) Format(entry *logrus.Entry) ([]byte, error) {
  data := logrus.Fields{}
  c.mu.Lock()
  defer c.mu.Unlock()
  for k, v := range c.defaultFields {
    data[k] = v
  }

  for k, v := range entry.Data {
    if !strings.HasPrefix(k, "labels.") {
      data["labels."+k] = v
    } else {
      data[k] = v
    }
  }

  entry.Data = data

	return c.formatter.Format(entry)
}

func NewLogger(ctx context.Context, fields logrus.Fields) *logrus.Logger {
	//traceContext := apmlogrus.TraceContext(ctx)
	entry := logrus.New().WithContext(ctx)

	if fields != nil {
		entry = entry.WithFields(fields)
	}

	logger := entry.
		Logger

	logger.SetFormatter(&contextualLoggerFormatter{
		defaultFields: fields,
    mu: &sync.Mutex{},
		formatter: &ecslogrus.Formatter{
			DataKey: "labels",
		},
	})

	logger.SetLevel(logrus.GetLevel())
  logUtilMu.Lock()
  defer logUtilMu.Unlock()
  levelHooks := logrus.StandardLogger().Hooks
  logger.Hooks = levelHooks
	//logger.AddHook(&apmlogrus.Hook{})

	return logger
}
