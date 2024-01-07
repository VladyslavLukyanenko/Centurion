FROM golang:latest as buildenv

WORKDIR /build

COPY go.mod go.sum ./

# Download all the dependencies
RUN go mod download

COPY . .

WORKDIR /build/


RUN GOOS=linux go build -a -installsuffix cgo -o checkout_service .

FROM ubuntu
MAINTAINER 0xFF <0xFF@centurion.gg>
WORKDIR /app
COPY --from=buildenv /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/
COPY --from=buildenv /build/checkout_service .

ENTRYPOINT ["./checkout_service"]
EXPOSE 5007