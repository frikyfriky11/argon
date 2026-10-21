#!/usr/bin/env python3
"""Build a coherent private image set and stream it to the restricted release receiver."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import time


def run(argv, **kwargs):
    return subprocess.run(argv, check=True, **kwargs)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['deploy', 'accept', 'promote'])
    parser.add_argument('--environment', required=True, choices=['staging', 'production'])
    args = parser.parse_args()
    config = json.loads(Path(__file__).with_name('release.json').read_text())
    project = config['project']
    assert re.fullmatch('[a-z0-9][a-z0-9-]{0,40}', project)
    assert os.environ['GITHUB_REPOSITORY'] == 'frikyfriky11/' + project
    assert os.environ['GITHUB_EVENT_NAME'] in ['push', 'workflow_dispatch']
    source = os.environ['GITHUB_SHA']
    assert re.fullmatch('[0-9a-f]{40}', source)
    assert subprocess.check_output(['git', 'rev-parse', 'HEAD'], text=True).strip() == source
    assert not subprocess.check_output(['git', 'status', '--porcelain', '--untracked-files=no'], text=True).strip(), 'Build source has tracked changes'
    header = {'project': project, 'action': args.action, 'environment': args.environment, 'source': source,
              'ref': os.environ['GITHUB_REF'], 'event': os.environ['GITHUB_EVENT_NAME'],
              'run_id': int(os.environ['GITHUB_RUN_ID']), 'attempt': int(os.environ['GITHUB_RUN_ATTEMPT']),
              'token': os.environ['GH_TOKEN']}
    with tempfile.TemporaryDirectory(prefix=project + '-private-release-') as temporary:
        root = Path(temporary)
        root.chmod(0o700)
        key = root / 'key'
        key.write_text(os.environ['HOMELAB_DEPLOY_SSH_KEY'].rstrip() + '\n')
        key.chmod(0o600)
        known = root / 'known_hosts'
        known.write_text(os.environ['HOMELAB_DEPLOY_KNOWN_HOSTS'].rstrip() + '\n')
        known.chmod(0o600)
        archive = None
        if args.action == 'deploy':
            driver = json.loads(subprocess.check_output(['docker', 'info', '--format', '{{json .DriverStatus}}'], text=True))
            assert ['driver-type', 'io.containerd.snapshotter.v1'] in driver, 'A containerd image store is required to preserve BuildKit provenance'
            builder = project + '-private-release'
            if subprocess.run(['docker', 'buildx', 'inspect', builder], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL).returncode:
                run(['docker', 'buildx', 'create', '--name', builder, '--driver', 'docker-container'])
            tags = []
            header['images'] = {}
            environment = {**os.environ, 'BUILDX_METADATA_PROVENANCE': 'max', 'DOCKER_BUILD_SUMMARY': 'false', 'DOCKER_BUILD_RECORD_UPLOAD': 'false'}
            for image in config['images']:
                service = image['service']
                assert re.fullmatch('[a-z][a-z0-9-]*', service)
                assert service not in header['images'], 'Duplicate application service'
                tag = 'local/' + project + '-' + service + ':' + source
                command = ['docker', 'buildx', 'build', '--builder', builder, '--platform', 'linux/amd64',
                           '--provenance=mode=max', '--load', '--metadata-file', str(root / (service + '-metadata.json')),
                           '--label', 'org.opencontainers.image.revision=' + source, '--tag', tag]
                if image.get('dockerfile'):
                    command.extend(['--file', image['dockerfile']])
                if image.get('target'):
                    command.extend(['--target', image['target']])
                for target in image.get('no_cache_filters', []):
                    command.extend(['--no-cache-filter', target])
                for name, value in image.get('build_args', {}).items():
                    if isinstance(value, dict):
                        value = os.environ[value['env']]
                    command.extend(['--build-arg', name + '=' + str(value).replace('{source}', source)])
                command.append(image.get('context', '.'))
                run(command, env=environment)
                identity = subprocess.check_output(['docker', 'image', 'inspect', tag, '--format', '{{.Id}}'], text=True).strip()
                assert re.fullmatch('sha256:[0-9a-f]{64}', identity)
                services = [service] + image.get('aliases', [])
                assert len(services) == len(set(services)), 'Duplicate application alias'
                for name in services:
                    assert re.fullmatch('[a-z][a-z0-9-]*', name)
                    assert name not in header['images'], 'Duplicate application service'
                    header['images'][name] = {'image_id': identity}
                tags.append(tag)
                if image.get('isolated_command'):
                    run(['docker', 'run', '--rm', '--network', 'none', tag] + image['isolated_command'])
                if image.get('isolated_health'):
                    probe = project + '-release-probe-' + service + '-' + str(os.getpid())
                    command = ['docker', 'run', '--detach', '--name', probe, '--network', 'none',
                               '--health-interval', '2s', '--health-start-period', '3s', '--health-retries', '20']
                    for name, value in image.get('isolated_environment', {}).items():
                        command.extend(['--env', name + '=' + str(value)])
                    command.append(tag)
                    try:
                        run(command)
                        deadline = time.monotonic() + 90
                        while time.monotonic() < deadline:
                            state = json.loads(subprocess.check_output(['docker', 'inspect', probe, '--format', '{{json .State}}'], text=True))
                            assert state['Running'], 'Isolated application image stopped'
                            if state.get('Health', {}).get('Status') == 'healthy':
                                break
                            time.sleep(2)
                        else:
                            raise AssertionError('Isolated application image did not become healthy')
                    finally:
                        subprocess.run(['docker', 'rm', '--force', '--volumes', probe], check=True, stdout=subprocess.DEVNULL)
            for command in config.get('verify_commands', []):
                assert isinstance(command, list) and command and all(isinstance(value, str) for value in command)
                run([value.replace('{source}', source) for value in command])
            archive = root / 'images.tar.gz'
            with archive.open('wb') as output:
                compressor = subprocess.Popen(['gzip', '-n', '-1'], stdin=subprocess.PIPE, stdout=output)
                saver = subprocess.run(['docker', 'image', 'save'] + tags, stdout=compressor.stdin)
                compressor.stdin.close()
                assert compressor.wait() == 0 and saver.returncode == 0
            with archive.open('rb') as stream:
                header['archive_sha256'] = hashlib.file_digest(stream, 'sha256').hexdigest()
            header['archive_bytes'] = archive.stat().st_size
        host = os.environ['HOMELAB_DEPLOY_HOST']
        assert re.fullmatch('[a-zA-Z0-9.-]+', host)
        command = ['ssh', '-oBatchMode=yes', '-oIdentitiesOnly=yes', '-oStrictHostKeyChecking=yes',
                   '-oUserKnownHostsFile=' + str(known), '-i', str(key), 'release-' + project + '@' + host, 'deploy']
        receiver = subprocess.Popen(command, stdin=subprocess.PIPE)
        try:
            receiver.stdin.write((json.dumps(header, separators=(',', ':')) + '\n').encode())
            receiver.stdin.flush()
            if archive:
                with archive.open('rb') as stream:
                    shutil.copyfileobj(stream, receiver.stdin, 1024**2)
        finally:
            receiver.stdin.close()
        assert receiver.wait() == 0, 'Restricted deployment or acceptance was refused'


if __name__ == '__main__':
    main()
