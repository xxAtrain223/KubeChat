#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
# Install libc6-dev for AgonesSDK
#RUN apt-get update && \
#    apt-get install -y --allow-unauthenticated libc6-dev && \
#    rm -rf /var/lib/apt/lists/*
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src

COPY KubeChat.Gateway/ KubeChat.Gateway/

RUN dotnet restore KubeChat.Gateway/KubeChat.Gateway.csproj
RUN dotnet build KubeChat.Gateway/KubeChat.Gateway.csproj -c Release -o /app/KubeChat.Gateway/build

FROM build AS publish
RUN dotnet publish KubeChat.Gateway/KubeChat.Gateway.csproj -c Release -o /app/KubeChat.Gateway/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/KubeChat.Gateway/publish .
ENTRYPOINT ["dotnet", "KubeChat.Gateway.dll"]
