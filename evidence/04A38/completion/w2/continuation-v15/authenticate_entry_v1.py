"""Read-only v14 entry gates; write only new v15 command evidence, fail closed."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[5]
HERE = Path(__file__).resolve().parent
W2 = HERE.parent
C14 = W2 / 'continuation-v14'
BASE = 'ba1636f5356ac761509c2cfa16fc66b92430935d'
os.environ['GIT_OPTIONAL_LOCKS'] = '0'
os.environ['PYTHONDONTWRITEBYTECODE'] = '1'


def pin(path):
    path = Path(path)
    with path.open('rb') as stream:
        digest = hashlib.file_digest(stream, 'sha256').hexdigest()
    return dict(path=path.relative_to(ROOT).as_posix(), bytes=path.stat().st_size, sha256=digest)


def save(path, value):
    with path.open('x', encoding='utf-8', newline='\n') as stream:
        json.dump(value, stream, indent=2, ensure_ascii=False)
        stream.write('\n')


def check(row):
    actual = pin(ROOT / row['path'])
    expected = {key: row[key] for key in ('path', 'bytes', 'sha256')}
    if actual != expected:
        raise ValueError('ENTRY_PIN_MISMATCH_STOP ' + json.dumps(dict(expected=expected, actual=actual)))
    return actual


def command(label, args):
    folder = HERE / 'entry-commands-v1' / label
    folder.mkdir(parents=True, exist_ok=False)
    begin = time.monotonic()
    result = subprocess.run(args, cwd=ROOT, capture_output=True)
    for name in ('stdout', 'stderr'):
        with (folder / (name + '.log')).open('xb') as stream:
            stream.write(getattr(result, name))
    record = dict(command=args, cwd=str(ROOT), exit_code=result.returncode,
                  seconds=time.monotonic() - begin,
                  stdout=pin(folder / 'stdout.log'), stderr=pin(folder / 'stderr.log'))
    save(folder / 'command.json', record)
    executions.append(dict(label=label, receipt=pin(folder / 'command.json'), **record))
    print(json.dumps(dict(label=label, exit_code=result.returncode,
                         seconds=record['seconds'])), flush=True)
    if result.returncode:
        raise ValueError('ENTRY_READER_OR_GIT_FAILED_STOP ' + label)
    return result.stdout


executions = []
authorities = []
try:
    for name in ('continuation-v14-postseal-handover-addendum-v1.md',
                 'continuation-v14-final-git-preservation-v1.json',
                 'continuation-v14-post-publication-artifacts-v1.json'):
        authorities.append(pin(W2 / name))
    final = json.loads((W2 / 'continuation-v14-final-git-preservation-v1.json').read_bytes())
    art = json.loads((W2 / 'continuation-v14-post-publication-artifacts-v1.json').read_bytes())
    for doc in (final, art):
        for row in doc.values():
            if isinstance(row, dict) and {'path', 'bytes', 'sha256'} <= row.keys():
                authorities.append(check(row))
    for row in art['source_after_images'] + art['current_compiled_artifacts']:
        authorities.append(check(row))
    for name, args in [('branch', ['branch', '--show-current']), ('HEAD', ['rev-parse', 'HEAD']),
                       ('main', ['rev-parse', 'main']), ('merge-base', ['merge-base', 'HEAD', 'main']),
                       ('index', ['diff', '--cached', '--name-status']),
                       ('status', ['status', '--short']), ('diff-check', ['diff', '--check'])]:
        out = command('git-' + name, ['git', '-c', 'safe.directory=' + ROOT.as_posix(),
                                    '--no-optional-locks', *args])
        if name == 'branch' and out.decode().strip() != 'codex/04A-38':
            raise ValueError('BRANCH_MISMATCH_STOP')
        if name in ('HEAD', 'main', 'merge-base') and out.decode().strip() != BASE:
            raise ValueError('ANCHOR_MISMATCH_STOP ' + name)
        if name == 'index' and out:
            raise ValueError('NONEMPTY_INDEX_STOP')
    authorities.append(check(final['index_file']))
    bootstrap = ('import sys,runpy;sys.path.insert(0,sys.argv[1]);'
                 'sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")')
    tasks = [
        ('c14-checkpoint-v2', C14, C14 / 'verify_c14_checkpoint_v2.py', []),
        ('c14-publication-v3', C14, C14 / 'verify_c14_publication_v3.py', []),
        ('c14-preservation-v3', C14, C14 / 'verify_c14_preservation_v3.py', []),
        ('c14-private-v2', C14, W2 / 'continuation-v14-retained-private-reader-v2.py', ['--verify']),
        ('c13-private-v2', W2 / 'continuation-v13', W2 / 'continuation-v13-retained-private-reader-v2.py', ['--verify']),
        ('c14-physical-parts-v3', C14, C14 / 'verify_c14_publication_v3.py',
         ['--parts', str(W2 / 'continuation-v14-review-parts-index-v2.json')]),
    ]
    for label, directory, script, args in tasks:
        authorities.append(pin(script))
        command(label, [sys.executable, '-I', '-B', '-c', bootstrap, str(directory), str(script), *args])
    save(HERE / 'entry-authentication-v1.json', dict(status='ENTRY_GATES_PASS', authorities=authorities,
         executions=executions, implementation_edits=0, original_queries=0))
except Exception as error:
    save(HERE / 'entry-authentication-v1.json', dict(status='ENTRY_AUTHENTICATION_FAILED_STOP',
         error=type(error).__name__ + ': ' + str(error), authorities=authorities,
         executions=executions, implementation_edits=0, original_queries=0))
    print(type(error).__name__ + ': ' + str(error), flush=True)
    raise SystemExit(1)
