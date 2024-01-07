#!/usr/bin/env sh

cd ../src/webhook-service || ('Can`t find webhook service sources to generate migration';exit;)
MIGRATION_NAME="$1"
if [ -z "$1" ]; then
  MIGRATION_NAME='Initial'
fi

dotnet ef migrations add $MIGRATION_NAME -o ./Infrastructure/Migrations