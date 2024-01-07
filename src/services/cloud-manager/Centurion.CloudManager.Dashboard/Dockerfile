# build backend
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS dotnet-build-env
WORKDIR /srv
COPY ./src ./

WORKDIR MonitorsPanel.Web.ManagerSpa
RUN dotnet restore && dotnet publish -c Release -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -r linux-x64 -o ../dist


# build client app
FROM node:12.18.3 AS angular-build-env
     
ENV NPM_CONFIG_LOGLEVEL warn
ARG app_env

WORKDIR /angular-tmp
COPY ./src/MonitorsPanel.Web.ManagerSpa/ClientApp .
RUN npm i && npm run build:prod

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

WORKDIR /app
COPY --from=dotnet-build-env /srv/dist .
COPY --from=angular-build-env /angular-tmp/dist ./ClientApp/dist

ENTRYPOINT ["dotnet", "MonitorsPanel.Web.ManagerSpa.dll"]
EXPOSE 443
EXPOSE 80
