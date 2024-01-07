#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)

docker build \
  -t docker.pkg.github.com/centurionlabs/centurion/cloudmanager:$IMG_TAG \
  -t docker.pkg.github.com/centurionlabs/centurion/cloudmanager:latest \
  -f ./src/services/cloud-manager/Centurion.CloudManager/Dockerfile \
  .

docker push docker.pkg.github.com/centurionlabs/centurion/cloudmanager:$IMG_TAG
docker push docker.pkg.github.com/centurionlabs/centurion/cloudmanager:latest
