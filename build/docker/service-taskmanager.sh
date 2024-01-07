#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)

docker build \
  -t docker.pkg.github.com/centurionlabs/centurion/taskmanager:$IMG_TAG \
  -t docker.pkg.github.com/centurionlabs/centurion/taskmanager:latest \
  -f ./src/services/task-manager/Dockerfile \
  .

docker push docker.pkg.github.com/centurionlabs/centurion/taskmanager:$IMG_TAG
docker push docker.pkg.github.com/centurionlabs/centurion/taskmanager:latest
