FROM ubuntu:18.04 as tar_files
WORKDIR /tar
COPY . .
RUN find . -name "*.csproj" -print0 | tar -cvf projectfiles.tar --null -T -

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY ./*.sln  ./

COPY --from=tar_files /tar/projectfiles.tar .
RUN tar -xvf projectfiles.tar
RUN rm projectfiles.tar

RUN dotnet restore src/services/task-manager/

COPY . .
WORKDIR src/services/task-manager/
RUN dotnet publish -c Release --self-contained=false -o /app/dist

FROM mcr.microsoft.com/dotnet/aspnet:6.0

# Disable AOT
ENV DotNet_readytorun=0

# Turn on the hierarchical PGO
ENV dotnet_tieredpgo=1

# Enables Quick JIT for loop
ENV DotNet_TC_QUICKJITFORLOOPS=1
WORKDIR /app
COPY --from=build /app/dist .
ENTRYPOINT ["dotnet", "Centurion.TaskManager.dll"]
EXPOSE 443
EXPOSE 81
EXPOSE 80
