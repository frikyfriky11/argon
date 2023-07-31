FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build-env

# the source code will be copied in /src
WORKDIR /src

# by copying only the csproj files we can leverage build layer caching.
# this means that this layer of the image will be rebuilt only if the csproj file change, such as new packages added.
COPY ["src/Argon.WebApi/Argon.WebApi.csproj", "Argon.WebApi/"]
COPY ["src/Argon.Application/Argon.Application.csproj", "Argon.Application/"]
COPY ["src/Argon.Domain/Argon.Domain.csproj", "Argon.Domain/"]
COPY ["src/Argon.Infrastructure/Argon.Infrastructure.csproj", "Argon.Infrastructure/"]
RUN dotnet restore "Argon.WebApi/Argon.WebApi.csproj"

# after restoring all packages, we can copy the whole source code.
# please note that the files copied are relative to the build context, and the .dockerignore file tells Docker which
# files to copy and which to not copy.
COPY ["src/Argon.WebApi/.", "Argon.WebApi/"]
COPY ["src/Argon.Domain/.", "Argon.Domain/"]
COPY ["src/Argon.Application/.", "Argon.Application/"]
COPY ["src/Argon.Infrastructure/.", "Argon.Infrastructure/"]

# at this point we can build and publish the app to the /out dir
RUN dotnet publish --no-restore -c Release -o out Argon.WebApi/Argon.WebApi.csproj

# this is a multi stage build, so the build process happens on the sdk base image and the final image
# uses only the runtime/aspnet image, to minimize the size of the resulting image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview

# we need to install curl to make the healthchecks work
RUN apt update -y && apt install curl -y

# the final app will run inside the /app directory
WORKDIR /app

# we can copy the build result from the previous build stage into this one
COPY --from=build-env ["/src/out", "."]

# make the app listen on all ip addresses on port 80.
# we are avoiding https because this app should run behind a reverse proxy.
ENV HTTP_PORTS=80

# the entrypoint of the application is our dll file run by the dotnet CLI
ENTRYPOINT ["dotnet", "Argon.WebApi.dll"]

# this healthcheck reports the healthy/unhealthy status to the Docker engine by contacting the API at the healthz endpoint.
# if the healthcheck returns HTTP 200 OK, then it is reported as healthy.
# if the healthcheck returns HTTP 503 Service Unavailable, then it is reported as unhealthy.
HEALTHCHECK --interval=3s --timeout=2s --start-period=10s --retries=3 CMD curl --fail http://localhost/healthz || exit 1

# we instruct the consumers of this image that it exposes the port 80 on TCP
EXPOSE 80/tcp