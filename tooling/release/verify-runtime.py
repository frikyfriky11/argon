#!/usr/bin/env python3
"""Exercise one frontend image in both environments without network or host ports."""
import hashlib
import json
import subprocess
import sys
import time

source = sys.argv[1]
image = 'local/argon-webgui:' + source
bundle_hashes = []


def output(*args):
    return subprocess.check_output(args, text=True).strip()


for environment, tenant in (('test', 'argon-test'), ('prod', 'argon')):
    name = 'argon-runtime-probe-' + environment + '-' + str(time.time_ns())
    origin = 'https://' + tenant + '.previatozanirato.family'
    client = 'isolated-"client\\with\nJSON-boundaries'
    expected = {
        'VITE_APP_BACKEND_API_URI': '/api',
        'VITE_APP_RUNNING_ENVIRONMENT': environment,
        'VITE_APP_AUTHORITY': 'https://auth.previatozanirato.family/application/o/' + tenant + '/',
        'VITE_APP_CLIENT_ID': client,
        'VITE_APP_REDIRECT_URI': origin + '/auth/post-sign-in',
        'VITE_APP_SCOPE': 'openid profile offline_access',
    }
    command = ['docker', 'run', '--detach', '--name', name, '--network', 'none',
               '--health-interval', '1s', '--health-start-period', '1s']
    for key, value in expected.items():
        command += ['--env', 'ARGON_' + key.removeprefix('VITE_APP_') + '=' + value]
    try:
        output(*command, image)
        for attempt in range(60):
            state = json.loads(output('docker', 'inspect', name, '--format', '{{json .State}}'))
            assert state['Running'], 'Frontend image stopped'
            if state.get('Health', {}).get('Status') == 'healthy':
                break
            time.sleep(1)
        else:
            raise AssertionError('Frontend image did not become healthy')
        runtime = output('docker', 'exec', name, 'curl', '--fail', '--silent', 'http://127.0.0.1/runtime-config.js')
        assert runtime.startswith('window.ARGON_CONFIG = ')
        actual = json.loads(runtime.split('=', 1)[1].strip().removesuffix(';'))
        assert actual == expected, 'Runtime environment or JSON encoding differs'
        html = output('docker', 'exec', name, 'curl', '--fail', '--silent', 'http://127.0.0.1/')
        # Module scripts defer execution until parsing completes; this classic script runs during parsing.
        assert '<script src="/runtime-config.js"></script>' in html
        assert 'type="module"' in html and 'async' not in html
        assets = output('docker', 'exec', name, 'sh', '-c', 'find /usr/share/nginx/html/assets -type f -exec sha256sum {} + | sort')
        bundle_hashes.append(hashlib.sha256(assets.encode()).hexdigest())
    finally:
        subprocess.run(['docker', 'rm', '--force', '--volumes', name], check=True, stdout=subprocess.DEVNULL)
assert bundle_hashes[0] == bundle_hashes[1], 'Promotion must serve the identical application bundle'
print('Same frontend image passed both runtime environments, JSON boundaries and immutable bundle check.')
