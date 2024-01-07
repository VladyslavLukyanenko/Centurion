package tls

import "encoding/binary"

func writeLen(buf []byte, v, size int) {
  for i := 0; i < size; i++ {
    buf[size-i-1] = byte(v)
    v >>= 8
  }
  if v != 0 {
    panic("length is too long")
  }
}

type byteBuilder struct {
  buf       *[]byte
  start     int
  prefixLen int
  child     *byteBuilder
}

func newByteBuilder() *byteBuilder {
  buf := make([]byte, 0, 32)
  return &byteBuilder{buf: &buf}
}

func (bb *byteBuilder) len() int {
  return len(*bb.buf) - bb.start - bb.prefixLen
}

func (bb *byteBuilder) data() []byte {
  bb.flush()
  return (*bb.buf)[bb.start+bb.prefixLen:]
}

func (bb *byteBuilder) flush() {
  if bb.child == nil {
    return
  }
  bb.child.flush()
  writeLen((*bb.buf)[bb.child.start:], bb.child.len(), bb.child.prefixLen)
  bb.child = nil
  return
}

func (bb *byteBuilder) finish() []byte {
  bb.flush()
  return *bb.buf
}

func (bb *byteBuilder) addU8(u uint8) {
  bb.flush()
  *bb.buf = append(*bb.buf, u)
}

func (bb *byteBuilder) addU16(u uint16) {
  bb.flush()
  *bb.buf = append(*bb.buf, byte(u>>8), byte(u))
}

func (bb *byteBuilder) addU24(u int) {
  bb.flush()
  *bb.buf = append(*bb.buf, byte(u>>16), byte(u>>8), byte(u))
}

func (bb *byteBuilder) addU32(u uint32) {
  bb.flush()
  *bb.buf = append(*bb.buf, byte(u>>24), byte(u>>16), byte(u>>8), byte(u))
}

func (bb *byteBuilder) addU64(u uint64) {
  bb.flush()
  var b [8]byte
  binary.BigEndian.PutUint64(b[:], u)
  *bb.buf = append(*bb.buf, b[:]...)
}

func (bb *byteBuilder) addU8LengthPrefixed() *byteBuilder {
  return bb.createChild(1)
}

func (bb *byteBuilder) addU16LengthPrefixed() *byteBuilder {
  return bb.createChild(2)
}

func (bb *byteBuilder) addU24LengthPrefixed() *byteBuilder {
  return bb.createChild(3)
}

func (bb *byteBuilder) addU32LengthPrefixed() *byteBuilder {
  return bb.createChild(4)
}

func (bb *byteBuilder) addBytes(b []byte) {
  bb.flush()
  *bb.buf = append(*bb.buf, b...)
}

func (bb *byteBuilder) createChild(lengthPrefixSize int) *byteBuilder {
  bb.flush()
  bb.child = &byteBuilder{
    buf:       bb.buf,
    start:     len(*bb.buf),
    prefixLen: lengthPrefixSize,
  }
  for i := 0; i < lengthPrefixSize; i++ {
    *bb.buf = append(*bb.buf, 0)
  }
  return bb.child
}

func (bb *byteBuilder) discardChild() {
  if bb.child == nil {
    return
  }
  *bb.buf = (*bb.buf)[:bb.child.start]
  bb.child = nil
}

type byteReader []byte

func (br *byteReader) readInternal(out *byteReader, n int) bool {
  if len(*br) < n {
    return false
  }
  *out = (*br)[:n]
  *br = (*br)[n:]
  return true
}

func (br *byteReader) readBytes(out *[]byte, n int) bool {
  var child byteReader
  if !br.readInternal(&child, n) {
    return false
  }
  *out = []byte(child)
  return true
}

func (br *byteReader) readUint(out *uint64, n int) bool {
  var b []byte
  if !br.readBytes(&b, n) {
    return false
  }
  *out = 0
  for _, v := range b {
    *out <<= 8
    *out |= uint64(v)
  }
  return true
}

func (br *byteReader) readU8(out *uint8) bool {
  var b []byte
  if !br.readBytes(&b, 1) {
    return false
  }
  *out = b[0]
  return true
}

func (br *byteReader) readU16(out *uint16) bool {
  var v uint64
  if !br.readUint(&v, 2) {
    return false
  }
  *out = uint16(v)
  return true
}

func (br *byteReader) readU24(out *uint32) bool {
  var v uint64
  if !br.readUint(&v, 3) {
    return false
  }
  *out = uint32(v)
  return true
}

func (br *byteReader) readU32(out *uint32) bool {
  var v uint64
  if !br.readUint(&v, 4) {
    return false
  }
  *out = uint32(v)
  return true
}

func (br *byteReader) readU64(out *uint64) bool {
  return br.readUint(out, 8)
}

func (br *byteReader) readLengthPrefixed(out *byteReader, n int) bool {
  var length uint64
  return br.readUint(&length, n) &&
    uint64(len(*br)) >= length &&
    br.readInternal(out, int(length))
}

func (br *byteReader) readLengthPrefixedBytes(out *[]byte, n int) bool {
  var length uint64
  return br.readUint(&length, n) &&
    uint64(len(*br)) >= length &&
    br.readBytes(out, int(length))
}

func (br *byteReader) readU8LengthPrefixed(out *byteReader) bool {
  return br.readLengthPrefixed(out, 1)
}
func (br *byteReader) readU8LengthPrefixedBytes(out *[]byte) bool {
  return br.readLengthPrefixedBytes(out, 1)
}

func (br *byteReader) readU16LengthPrefixed(out *byteReader) bool {
  return br.readLengthPrefixed(out, 2)
}
func (br *byteReader) readU16LengthPrefixedBytes(out *[]byte) bool {
  return br.readLengthPrefixedBytes(out, 2)
}

func (br *byteReader) readU24LengthPrefixed(out *byteReader) bool {
  return br.readLengthPrefixed(out, 3)
}
func (br *byteReader) readU24LengthPrefixedBytes(out *[]byte) bool {
  return br.readLengthPrefixedBytes(out, 3)
}

func (br *byteReader) readU32LengthPrefixed(out *byteReader) bool {
  return br.readLengthPrefixed(out, 4)
}
func (br *byteReader) readU32LengthPrefixedBytes(out *[]byte) bool {
  return br.readLengthPrefixedBytes(out, 4)
}


func checkDuplicateExtensions(extensions byteReader) bool {
  seen := make(map[uint16]struct{})
  for len(extensions) > 0 {
    var extension uint16
    var body byteReader
    if !extensions.readU16(&extension) ||
      !extensions.readU16LengthPrefixed(&body) {
      return false
    }
    if _, ok := seen[extension]; ok {
      return false
    }
    seen[extension] = struct{}{}
  }
  return true
}