#!/bin/sh
cd ../../src/contracts || ('contracts dir not found. possibly it was moved'; exit;)

protoc \
  --go_out=../services/checkout/contracts \
  --go_opt=paths=source_relative \
  --go-grpc_out=../services/checkout/contracts \
  --go-grpc_opt=paths=source_relative \
  -I=. \
  ./*.proto \
  ./common/*.proto \
  ./checkout/**/*.proto \
  ./checkout/**/**/*.proto \
  ./checkout/*.proto \
  ./monitor/integration/events.proto \
  ./monitor/monitor_service.proto

