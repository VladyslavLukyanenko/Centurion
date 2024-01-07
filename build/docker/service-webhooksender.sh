#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)

docker build \
  -t docker.pkg.github.com/centurionlabs/centurion/webhook-sender:$IMG_TAG \
  -t docker.pkg.github.com/centurionlabs/centurion/webhook-sender:latest \
  -f ./src/services/webhook-service/Dockerfile \
  .

docker push docker.pkg.github.com/centurionlabs/centurion/webhook-sender:$IMG_TAG
docker push docker.pkg.github.com/centurionlabs/centurion/webhook-sender:latest
