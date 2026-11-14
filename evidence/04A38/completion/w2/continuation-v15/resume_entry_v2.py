"""Resume interrupted entry evidence, retaining the completed actual checkpoint replay."""
import ast
from concurrent.futures import ThreadPoolExecutor
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
os.environ['GIT_OPTIONAL_LOCKS'] = '0'
os.environ['PYTHONDONTWRITEBYTECODE'] = '1'


def pin(path):
    path = Path(path)
    with path.open('rb') as stream:
        digest = hashlib.file_digest(stream, 'sha256').hexdigest()
    return dict(path=path.relative_to(ROOT).as_posix(), bytes=path.stat().st_size, sha256=digest)


def save(path, value):
    with path.open('x', encoding='utf-8', newline='\n') as stream:
        json.dump(value, stream, indent=2)
        stream.write('\n')


def check(row):
    actual = pin(ROOT / row['path'])
    assert actual == {key: row[key] for key in ('path', 'bytes', 'sha256')}, 'ENTRY_PIN_MISMATCH_STOP ' + row['path']
    return actual


def command(task):
    label, directory, script, args = task
    folder = HERE / 'entry-commands-v2' / label
    folder.mkdir(parents=True, exist_ok=False)
    bootstrap = ('import sys,runpy;sys.path.insert(0,sys.argv[1]);'
                 'sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")')
    argv = [sys.executable, '-I', '-B', '-c', bootstrap, str(directory), str(script), *args]
    save(folder / 'launch.json', dict(command=argv, cwd=str(ROOT), reader=pin(script)))
    begin = time.monotonic()
    with (folder / 'stdout.log').open('xb') as out, (folder / 'stderr.log').open('xb') as err:
        result = subprocess.run(argv, cwd=ROOT, stdout=out, stderr=err)
    record = dict(command=argv, cwd=str(ROOT), reader=pin(script), exit_code=result.returncode,
                  seconds=time.monotonic() - begin,
                  stdout=pin(folder / 'stdout.log'), stderr=pin(folder / 'stderr.log'))
    save(folder / 'command.json', record)
    print(json.dumps(dict(label=label, exit_code=result.returncode, seconds=record['seconds'])), flush=True)
    return dict(label=label, receipt=pin(folder / 'command.json'), **record)


authorities = []
executions = []
try:
    final = json.loads((W2 / 'continuation-v14-final-git-preservation-v1.json').read_bytes())
    art = json.loads((W2 / 'continuation-v14-post-publication-artifacts-v1.json').read_bytes())
    for doc in (final, art):
        for row in doc.values():
            if isinstance(row, dict) and {'path', 'bytes', 'sha256'} <= row.keys():
                authorities.append(check(row))
    for row in art['source_after_images'] + art['current_compiled_artifacts']:
        authorities.append(check(row))
    tree = ast.parse((C14 / 'verify_c14_checkpoint_v2.py').read_bytes())
    sealed = next(ast.literal_eval(n.value) for n in tree.body if isinstance(n, ast.Assign)
                  and any(isinstance(t, ast.Name) and t.id == 'AUTH' for t in n.targets))
    for name, digest in sealed.items():
        row = pin(ROOT / name)
        assert row['sha256'] == digest, 'SEALED_INPUT_MISMATCH_STOP ' + name
        authorities.append(row)
    prior = HERE / 'entry-commands-v1' / 'c14-checkpoint-v2' / 'command.json'
    result = json.loads(prior.read_bytes())
    assert result['exit_code'] == 0
    check(result['stdout']); check(result['stderr'])
    summary = json.loads((ROOT / result['stdout']['path']).read_bytes())
    assert summary['historical'] == dict(total=704, closed=496, OPEN=208)
    assert summary['current'] == dict(total=726, closed=529, OPEN=197)
    executions.append(dict(label='c14-checkpoint-v2', receipt=pin(prior), **result))
    save(HERE / 'entry-interruption-v1.json', dict(status='INTERRUPTED_NO_RESULT_CAPTURED',
         completed_checkpoint=pin(prior), interrupted_reader=art['publication_reader'],
         interrupted_folder='entry-commands-v1/c14-publication-v3', observed_files=[],
         exit_code=None, implementation_edits=0, original_queries=0,
         reason='Conversation interruption; prior process/session no longer exists. Empty attempt folder retained; no exit code inferred.'))
    tasks = [
        ('c14-publication-v3', C14, C14 / 'verify_c14_publication_v3.py', []),
        ('c14-preservation-v3', C14, C14 / 'verify_c14_preservation_v3.py', []),
        ('c14-private-v2', C14, W2 / 'continuation-v14-retained-private-reader-v2.py', ['--verify']),
        ('c13-private-v2', W2 / 'continuation-v13', W2 / 'continuation-v13-retained-private-reader-v2.py', ['--verify']),
        ('c14-physical-parts-v3', C14, C14 / 'verify_c14_publication_v3.py',
         ['--parts', str(W2 / 'continuation-v14-review-parts-index-v2.json')]),
    ]
    with ThreadPoolExecutor(max_workers=3) as pool:
        executions.extend(pool.map(command, tasks))
    ok = all(row['exit_code'] == 0 for row in executions)
    save(HERE / 'entry-authentication-v2.json', dict(status='ENTRY_GATES_PASS' if ok else 'ENTRY_AUTHENTICATION_FAILED_STOP',
         authorities=authorities, executions=executions, implementation_edits=0, original_queries=0))
    raise SystemExit(0 if ok else 1)
except Exception as error:
    save(HERE / 'entry-authentication-v2.json', dict(status='ENTRY_AUTHENTICATION_FAILED_STOP',
         error=type(error).__name__ + ': ' + str(error), authorities=authorities,
         executions=executions, implementation_edits=0, original_queries=0))
    print(type(error).__name__ + ': ' + str(error), flush=True)
    raise SystemExit(1)
