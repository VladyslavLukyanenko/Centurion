#!/usr/bin/env bash

mv "../app/monitors-panel-api" "../app/monitors-panel-api_tmp"
java -jar ./openapi-generator-cli-5.0.0.jar generate -i "http://localhost:5000/swagger/v1/swagger.json" -g typescript-angular -c codegen-clients-config.json -o "../app/monitors-panel-api"
rm -r "../app/monitors-panel-api_tmp"
