#!/usr/bin/env sh

cd ../src/services/cloud-manager/Centurion.CloudManager || ('Can`t find cloud-manager service sources to generate migration';exit;)
MIGRATION_NAME="$1"
if [ -z "$1" ]; then
  MIGRATION_NAME='Initial'
fi

dotnet ef migrations add $MIGRATION_NAME -o ./Infra/Migrations