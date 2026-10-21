#!/usr/bin/env python3
"""Read-only live accounting/API/authentication configuration checks."""
import json
import sys
import time
import urllib.error
import urllib.request
origin, environment = sys.argv[1].rstrip('/'), sys.argv[2]
def get(path):
    request = urllib.request.Request(origin + path, headers={'Cache-Control': 'no-cache, no-store'})
    with urllib.request.urlopen(request, timeout=20) as response:
        assert response.status == 200
        return response.read().decode()
assert 'healthy' in get('/api/healthz').lower()
assert '<html' in get('/').lower()
runtime = get('/runtime-config.js?release_nonce=' + str(time.time_ns()))
assert runtime.startswith('window.ARGON_CONFIG = ')
config = json.loads(runtime.split('=', 1)[1].strip().removesuffix(';'))
assert config['VITE_APP_BACKEND_API_URI'] == '/api'
assert config['VITE_APP_RUNNING_ENVIRONMENT'] == ('test' if environment == 'staging' else 'prod')
assert config['VITE_APP_REDIRECT_URI'] == origin + '/auth/post-sign-in'
tenant = 'argon-test' if environment == 'staging' else 'argon'
assert config['VITE_APP_AUTHORITY'] == 'https://auth.previatozanirato.family/application/o/' + tenant + '/'
assert config['VITE_APP_CLIENT_ID']
assert config['VITE_APP_SCOPE'] == 'openid profile offline_access'
with urllib.request.urlopen(config['VITE_APP_AUTHORITY'] + '.well-known/openid-configuration', timeout=20) as response:
    discovery = json.load(response)
assert discovery['issuer'] == config['VITE_APP_AUTHORITY']
assert discovery['authorization_endpoint'].startswith('https://auth.previatozanirato.family/')
assert discovery['token_endpoint'].startswith('https://auth.previatozanirato.family/')
try:
    get('/api/Accounts')
    raise AssertionError('Accounting data accepted unauthenticated request')
except urllib.error.HTTPError as error:
    assert error.code == 401
print('Live health, independent authentication, runtime configuration and accounting access boundary passed.')
