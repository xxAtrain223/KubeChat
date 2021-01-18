#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
# Install libc6-dev for AgonesSDK
RUN apt-get update && \
    apt-get install -y --allow-unauthenticated libc6-dev && \
    rm -rf /var/lib/apt/lists/*
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src

COPY KubeChat.Server/ KubeChat.Server/

RUN dotnet restore KubeChat.Server/KubeChat.Server.csproj
RUN dotnet build KubeChat.Server/KubeChat.Server.csproj -c Release -o /app/KubeChat.Server/build

FROM build AS publish
RUN dotnet publish KubeChat.Server/KubeChat.Server.csproj -c Release -o /app/KubeChat.Server/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/KubeChat.Server/publish .
ENTRYPOINT ["dotnet", "KubeChat.Server.dll"]
