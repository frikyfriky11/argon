#!/usr/bin/env python3
"""Read-only live accounting/API/authentication configuration checks."""
import json
import sys
import urllib.error
import urllib.request
origin, environment = sys.argv[1].rstrip('/'), sys.argv[2]
def get(path):
    with urllib.request.urlopen(origin + path, timeout=20) as response:
        assert response.status == 200
        return response.read().decode()
assert 'healthy' in get('/api/healthz').lower()
assert '<html' in get('/').lower()
runtime = get('/runtime-config.js')
assert runtime.startswith('window.ARGON_CONFIG = ')
config = json.loads(runtime.split('=', 1)[1].strip().removesuffix(';'))
assert config['VITE_APP_BACKEND_API_URI'] == '/api'
assert config['VITE_APP_RUNNING_ENVIRONMENT'] == ('test' if environment == 'staging' else 'prod')
assert config['VITE_APP_REDIRECT_URI'] == origin + '/auth/post-sign-in'
assert config['VITE_APP_AUTHORITY'].startswith('https://auth.previatozanirato.family/')
assert config['VITE_APP_CLIENT_ID']
try:
    get('/api/Accounts')
    raise AssertionError('Accounting data accepted unauthenticated request')
except urllib.error.HTTPError as error:
    assert error.code == 401
print('Live health, independent authentication, runtime configuration and accounting access boundary passed.')
