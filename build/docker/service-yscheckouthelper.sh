#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)

docker build \
  -t docker.pkg.github.com/centurionlabs/centurion/yscheckouthelper$IMG_TAG \
  -t docker.pkg.github.com/centurionlabs/centurion/yscheckouthelper:latest \
  -f ./src/services/yscheckouthelper/Dockerfile \
  .

docker push docker.pkg.github.com/centurionlabs/centurion/yscheckouthelper:$IMG_TAG
docker push docker.pkg.github.com/centurionlabs/centurion/yscheckouthelper:latest
