package core

import (
	"container/list"
	"fmt"
	"time"
)

const defaultMaxSize = 500

type ActivityLogger interface {
	Log(message string)
	ToMessagesSlice() []string
}

func NewActivityLogger() ActivityLogger {
	return &limitingInMemoryLogger{
		linkedList: nil,
		slice:      []*logEntry{},
		maxSize:    defaultMaxSize,
	}
}

type logEntry struct {
	message   string
	timestamp time.Time
}

func (l *logEntry) String() string {
	return fmt.Sprintf("[%v] %s", l.timestamp, l.message)
}

type limitingInMemoryLogger struct {
	linkedList *list.List
	slice      []*logEntry
	maxSize    int
}

func (l *limitingInMemoryLogger) Log(message string) {
	e := &logEntry{
		message:   message,
		timestamp: time.Now().UTC(),
	}

	if l.reachedMaxSize() {
		if l.linkedList == nil {
			l.linkedList = list.New()
			for i := range l.slice {
				l.linkedList.PushBack(l.slice[i])
			}

			l.slice = nil
		}

		linkedList := l.linkedList
		linkedList.PushBack(e)
		linkedList.Remove(linkedList.Front())
	} else {
		l.slice = append(l.slice, e)
	}
}

func (l *limitingInMemoryLogger) ToMessagesSlice() []string {
	if l.reachedMaxSize() {
		s := make([]string, 0, l.linkedList.Len())
		h := l.linkedList.Back()
		for {
			e, _ := h.Value.(logEntry)
      s = append(s, e.String())
			h := h.Next()
			if h == nil {
				break
			}
		}

		return s
	}

	s := make([]string, 0, len(l.slice))
	for i := range l.slice {
		s = append(s, l.slice[i].String())
	}

	return s
}

func (l *limitingInMemoryLogger) reachedMaxSize() bool {
	var size int
	if l.slice != nil {
		size = len(l.slice)
	} else {
		size = l.linkedList.Len()
	}

	return size == l.maxSize
}
