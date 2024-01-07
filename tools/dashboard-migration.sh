#!/usr/bin/env sh

cd ../src/accounts/Centurion.Accounts.Infra || ('Can`t find accounts service sources to generate migration';exit;)
MIGRATION_NAME="$1"
if [ -z "$1" ]; then
  MIGRATION_NAME='Initial'
fi

dotnet ef migrations add $MIGRATION_NAME -o ./Infrastructure/Migrations --startup-project ../Centurion.Accounts/