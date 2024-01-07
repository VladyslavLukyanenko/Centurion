#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)

docker build \
  -t docker.pkg.github.com/centurionlabs/centurion/accounts:$IMG_TAG \
  -t docker.pkg.github.com/centurionlabs/centurion/accounts:latest \
  -f ./src/services/accounts/Centurion.Accounts/Dockerfile \
  .

docker push docker.pkg.github.com/centurionlabs/centurion/accounts:$IMG_TAG
docker push docker.pkg.github.com/centurionlabs/centurion/accounts:latest
