# build client
FROM node:14.17.4 AS angular-build-env
     
ENV NPM_CONFIG_LOGLEVEL warn

WORKDIR /spa-client
COPY ./src/accounts/Centurion.Accounts.DashboardSpa/AngularClient .
RUN npm i && npm run build:prod

# build backend
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS dotnet-build-env
WORKDIR /spa-server
COPY ./src/accounts/Centurion.Accounts.DashboardSpa .

RUN dotnet publish -c Release -r linux-x64 -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -o /spa-server/dist

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim

WORKDIR /app
COPY --from=dotnet-build-env /spa-server/dist .
COPY --from=angular-build-env /spa-client/dist ./AngularClient/dist

ENTRYPOINT ["dotnet", "Centurion.Accounts.DashboardSpa.dll"]
EXPOSE 443
EXPOSE 80
