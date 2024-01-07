#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)

docker build \
  -t docker.pkg.github.com/centurionlabs/centurion/fakeshop:$IMG_TAG \
  -t docker.pkg.github.com/centurionlabs/centurion/fakeshop:latest \
  -f ./src/services/playground/MonitoringFakeShop/Dockerfile \
  .

docker push docker.pkg.github.com/centurionlabs/centurion/fakeshop:$IMG_TAG
docker push docker.pkg.github.com/centurionlabs/centurion/fakeshop:latest
