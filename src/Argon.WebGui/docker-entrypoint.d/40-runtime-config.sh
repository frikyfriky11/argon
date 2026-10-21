#!/bin/sh
set -eu
: "${ARGON_BACKEND_API_URI:=/api}"
: "${ARGON_RUNNING_ENVIRONMENT:=prod}"
: "${ARGON_AUTHORITY:?The environment authentication authority is required}"
: "${ARGON_CLIENT_ID:?The environment authentication client is required}"
: "${ARGON_REDIRECT_URI:?The environment authentication redirect is required}"
: "${ARGON_SCOPE:=openid profile offline_access}"
output=/usr/share/nginx/html/runtime-config.js
printf 'window.ARGON_CONFIG = ' >"${output}.new"
jq --null-input --compact-output \
  --arg backend "$ARGON_BACKEND_API_URI" \
  --arg environment "$ARGON_RUNNING_ENVIRONMENT" \
  --arg authority "$ARGON_AUTHORITY" \
  --arg client "$ARGON_CLIENT_ID" \
  --arg redirect "$ARGON_REDIRECT_URI" \
  --arg scope "$ARGON_SCOPE" \
  '{VITE_APP_BACKEND_API_URI:$backend,VITE_APP_RUNNING_ENVIRONMENT:$environment,VITE_APP_AUTHORITY:$authority,VITE_APP_CLIENT_ID:$client,VITE_APP_REDIRECT_URI:$redirect,VITE_APP_SCOPE:$scope}' >>"${output}.new"
printf ';\n' >>"${output}.new"
mv "${output}.new" "$output"
