#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src

COPY KubeChat.Client.Common/ KubeChat.Client.Common/
COPY KubeChat.Client.ServerSide/ KubeChat.Client.ServerSide/

RUN dotnet restore KubeChat.Client.ServerSide/KubeChat.Client.ServerSide.csproj
RUN dotnet build KubeChat.Client.ServerSide/KubeChat.Client.ServerSide.csproj -c Release -o /app/KubeChat.Client.ServerSide/build

FROM build AS publish
RUN dotnet publish KubeChat.Client.ServerSide/KubeChat.Client.ServerSide.csproj -c Release -o /app/KubeChat.Client.ServerSide/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/KubeChat.Client.ServerSide/publish .
ENTRYPOINT ["dotnet", "KubeChat.Client.ServerSide.dll"]
