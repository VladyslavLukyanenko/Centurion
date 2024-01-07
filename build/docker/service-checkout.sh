#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)

cd ./src/services/checkout-service

docker build \
  -t docker.pkg.github.com/centurionlabs/centurion/checkout:$IMG_TAG \
  -t docker.pkg.github.com/centurionlabs/centurion/checkout:latest \
  -f ./Dockerfile \
  .

docker push docker.pkg.github.com/centurionlabs/centurion/checkout:$IMG_TAG
docker push docker.pkg.github.com/centurionlabs/centurion/checkout:latest
