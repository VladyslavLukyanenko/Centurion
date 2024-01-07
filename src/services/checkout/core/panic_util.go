package core

import (
  "errors"
  log "github.com/sirupsen/logrus"
)

func ExecuteWithRecovery(err *error, fn func()) {
  defer func() {
    srcErr := recover()
    if srcErr == nil {
      return
    }

    if e, ok := srcErr.(error); ok {
      *err = e
    } else {
      e := errors.New("unexpected error")
      *err = e
    }

    log.Println(*err)
  }()

  fn()
}
