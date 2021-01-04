# Here, we include the dotnet core SDK as the base to build our app
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
# Setting the work directory for our app

WORKDIR /src

COPY KubeChat.Client.Common/ KubeChat.Client.Common/
COPY KubeChat.Client.WebAssembly/ KubeChat.Client.WebAssembly/ 

RUN dotnet restore KubeChat.Client.WebAssembly/KubeChat.Client.WebAssembly.csproj
RUN dotnet build KubeChat.Client.WebAssembly/KubeChat.Client.WebAssembly.csproj -c Release -o /app/KubeChat.Client.WebAssembly/build

# Once we're done building, we'll publish the project
# to the publish folder 
FROM build AS publish
RUN dotnet publish KubeChat.Client.WebAssembly/KubeChat.Client.WebAssembly.csproj -c Release -o /app/KubeChat.Client.WebAssembly/publish

# We then get the base image for Nginx and set the 
# work directory 
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# We'll copy all the contents from wwwroot in the publish
# folder into nginx/html for nginx to serve. The destination
# should be the same as what you set in the nginx.conf.
COPY --from=publish /app/KubeChat.Client.WebAssembly/publish/wwwroot /usr/share/nginx/html
#COPY KubeChat.Client.WebAssembly/nginx.conf /etc/nginx/nginx.conf
