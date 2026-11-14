"""Append-only C5 entry audit. Reads frozen evidence; never executes originals."""
import hashlib
import importlib
import json
import subprocess
import sys
import time
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[4]
PRIOR = HERE.parent / 'continuation-v4'
sys.dont_write_bytecode = True
sys.path.insert(0, str(PRIOR))


def sha(data):
    return hashlib.sha256(data).hexdigest()


def save(name, value):
    with (HERE / name).open('x', encoding='utf-8', newline='\n') as stream:
        json.dump(value, stream, ensure_ascii=False, indent=2)
        stream.write('\n')


def main():
    records = []
    for name in ['status', 'branch', 'HEAD', 'main', 'merge-base', 'index', 'diff-check']:
        args = {'status': ['status', '--porcelain=v1'], 'branch': ['branch', '--show-current'],
                'HEAD': ['rev-parse', 'HEAD'], 'main': ['rev-parse', 'main'],
                'merge-base': ['merge-base', 'HEAD', 'main'],
                'index': ['diff', '--cached', '--name-only'], 'diff-check': ['diff', '--check']}[name]
        run = subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *args],
                             cwd=ROOT, capture_output=True)
        records.append(dict(check=name, exit_code=run.returncode,
                            stdout=run.stdout.decode('utf-8'), stderr=run.stderr.decode('utf-8')))
    save('git-entry.json', records)
    manifest_path = PRIOR / 'publication-manifest-v3.json'
    manifest = json.loads(manifest_path.read_bytes())
    failures = []
    count = 0
    for row in manifest['entries']:
        if row['sha256'] is None:
            continue
        path = ROOT / row['path']
        if not path.is_file():
            failures.append(dict(path=row['path'], error='MISSING', expected=row))
        else:
            data = path.read_bytes()
            if (len(data), sha(data)) != (row['bytes'], row['sha256']):
                failures.append(dict(path=row['path'], error='BYTES_MISMATCH', expected=row,
                                     actual=dict(bytes=len(data), sha256=sha(data))))
        count += 1
    save('manifest-entry.json', dict(manifest_sha256=sha(manifest_path.read_bytes()),
                                   checked_payloads=count, failures=failures))
    print('manifest', count, 'failures', len(failures), flush=True)
    for module, kwargs in [('verify_preservation', {}), ('verify_slice_v5', {}),
                           ('verify_retained', {}), ('verify_ownership_v2', {}),
                           ('verify_fragment_scope', {}),
                           ('verify_publication', dict(manifest_path=manifest_path.relative_to(ROOT).as_posix()))]:
        start = time.monotonic()
        try:
            result = importlib.import_module(module).inspect(**kwargs)
            outcome = dict(passed=True, result=result)
        except Exception as error:
            outcome = dict(passed=False, error_type=type(error).__name__,
                           error=str(error).replace(str(ROOT), '<REPO_ROOT>'))
        outcome['seconds'] = time.monotonic() - start
        save(module + '-entry.json', outcome)
        print(module, outcome['passed'], outcome.get('error', ''), flush=True)


if __name__ == '__main__':
    main()
