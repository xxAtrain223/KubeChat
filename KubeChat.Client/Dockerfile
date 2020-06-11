FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY KubeChat.Client.csproj .
RUN dotnet restore "KubeChat.Client.csproj"
COPY . .
RUN dotnet build "KubeChat.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KubeChat.Client.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY nginx.conf /etc/nginx/nginx.conf