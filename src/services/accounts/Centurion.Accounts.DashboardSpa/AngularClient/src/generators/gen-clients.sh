#!/usr/bin/env bash

mv "../dashboards-api" "../dashboards-api_tmp"
java -jar ./openapi-generator-cli-5.0.0.jar generate -i "http://localhost:5000/swagger/v1/swagger.json" -g typescript-angular -c codegen-clients-config.json -o "../dashboards-api"
rm -r "../dashboards-api_tmp"
