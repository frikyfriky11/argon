FROM node:20.3 as build-env

# this project uses pnpm as a package manager, so let's install it
RUN npm i -g pnpm@9.6.0

# the source code will be copied in /src
WORKDIR /src

# by copying only the package files we can leverage build layer caching.
# this means that this layer of the image will be rebuilt only if the packages file change, such as new packages added.
COPY ["src/Argon.WebGui/pnpm-lock.yaml", "."]

# pnpm fetch downloads the packages from the npm registry without looking at the package.json file
# but only using the pnpm-lock.yaml file, making the download process super fast.
# it also helps with build layer caching because we can change the scripts or other information in the package.json
# file without invalidating the cache layer built up until this point
RUN pnpm fetch

# after restoring all packages, we can copy the whole source code.
# please note that the files copied are relative to the build context, and the .dockerignore file tells Docker which
# files to copy and which to not copy.
COPY ["src/Argon.WebGui/.", "."]

# now we can actually install the packages, but without ever going to the network (by using --offline) because
# they are already placed in the pnpm folder and can be hard-linked from there.
RUN pnpm install -r --offline

# when building the image, we need a way to change the BASE_API_URL env variable
# because it is not injected in the code but it is statically replaced with its content when
# the project is built, so we don't have an appsettings file to configure after the build is done.
# the ARG command declares a variable that can come from the docker build command
ARG BACKEND_API_URI
ARG RUNNING_ENVIRONMENT
ARG BUILD_ID
ARG COMMIT_HASH
# the ENV command injects this variable as an environment variable so that Vite can pick it up when it runs
ENV VITE_APP_BACKEND_API_URI $BACKEND_API_URI
ENV VITE_APP_RUNNING_ENVIRONMENT $RUNNING_ENVIRONMENT
ENV VITE_APP_BUILD_ID $BUILD_ID
ENV VITE_APP_COMMIT_HASH $COMMIT_HASH

# run the Vite build and prepare the dist folder with the built project
RUN npm run build

# this is a multi stage build, so the build process happens on the node base image and the final image
# uses only the nginx image, to minimize the size of the resulting image
FROM nginx:1.25.1

# copy over the configuration of nginx and replace it inside the image
COPY ["src/Argon.WebGui/nginx.conf", "/etc/nginx/conf.d/default.conf"]

# we can copy the build result from the previous build stage into this one
COPY --from=build-env ["/src/dist", "/usr/share/nginx/html"]

# this healthcheck reports the healthy/unhealthy status to the Docker engine by checking if the nginx service is up.
# if the healthcheck returns HTTP 200 OK, then it is reported as healthy.
# if the healthcheck returns HTTP 503 Service Unavailable, then it is reported as unhealthy.
HEALTHCHECK --interval=3s --timeout=2s --start-period=10s --retries=3 CMD service nginx status || exit 1

# we instruct the consumers of this image that it exposes the port 80 on TCP
EXPOSE 80/tcp
