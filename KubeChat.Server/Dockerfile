#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src

COPY KubeChat.Server/ KubeChat.Server/

RUN dotnet restore KubeChat.Server/KubeChat.Server.csproj
RUN dotnet build KubeChat.Server/KubeChat.Server.csproj -c Release -o /app/KubeChat.Server/build

FROM build AS publish
RUN dotnet publish KubeChat.Server/KubeChat.Server.csproj -c Release -o /app/KubeChat.Client.ServerSide/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/KubeChat.Client.ServerSide/publish .
ENTRYPOINT ["dotnet", "KubeChat.Server.dll"]
