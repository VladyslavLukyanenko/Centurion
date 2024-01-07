#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)

docker build \
  -t docker.pkg.github.com/centurionlabs/centurion/monitor:$IMG_TAG \
  -t docker.pkg.github.com/centurionlabs/centurion/monitor:latest \
  -f ./src/services/monitor/Centurion.Monitor/Dockerfile \
  --build-arg NUGETUSER=$1 \
  --build-arg NUGETPASS=$2 \
  .

docker push docker.pkg.github.com/centurionlabs/centurion/monitor:$IMG_TAG
docker push docker.pkg.github.com/centurionlabs/centurion/monitor:latest
