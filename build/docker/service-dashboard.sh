#!/usr/bin/env bash
IMG_TAG=$(date +%Y%m%d%H%M%S)

docker build \
  -t docker.pkg.github.com/centurionlabs/centurion/dashboard:$IMG_TAG \
  -t docker.pkg.github.com/centurionlabs/centurion/dashboard:latest \
  -f ./src/services/accounts/Centurion.Accounts.DashboardSpa/Dockerfile \
  .

docker push docker.pkg.github.com/centurionlabs/centurion/dashboard:$IMG_TAG
docker push docker.pkg.github.com/centurionlabs/centurion/dashboard:latest
