"""Append-only C11 entry authentication. Never executes original model code."""
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import time
import traceback

ROOT = Path(__file__).resolve().parents[5]
HERE = Path(__file__).resolve().parent
W2 = HERE.parent
OLD = W2 / 'continuation-v10'
BASE = 'ba1636f5356ac761509c2cfa16fc66b92430935d'
os.environ['GIT_OPTIONAL_LOCKS'] = '0'
os.environ['PYTHONDONTWRITEBYTECODE'] = '1'


def sha(data):
    return hashlib.sha256(data).hexdigest()


def pin(path):
    path = Path(path)
    with path.open('rb') as stream:
        digest = hashlib.file_digest(stream, 'sha256').hexdigest()
    return dict(path=path.relative_to(ROOT).as_posix(), bytes=path.stat().st_size, sha256=digest)


def save(name, value):
    with (HERE / name).open('x', encoding='utf-8', newline='\n') as stream:
        json.dump(value, stream, indent=2, ensure_ascii=False, allow_nan=False)
        stream.write('\n')


def command(label, args):
    start = time.monotonic()
    result = subprocess.run(args, cwd=ROOT, capture_output=True)
    streams = {}
    for name in ['stdout', 'stderr']:
        target = HERE / (label + '-' + name + '.log')
        with target.open('xb') as stream:
            stream.write(getattr(result, name))
        streams[name] = pin(target)
    receipt = dict(command=args, cwd=str(ROOT), exit_code=result.returncode,
                   seconds=time.monotonic()-start, **streams)
    save(label + '.json', receipt)
    return result


def main():
    checks = []
    for label, args in [('branch', ['branch', '--show-current']),
                        ('HEAD', ['rev-parse', 'HEAD']), ('main', ['rev-parse', 'main']),
                        ('merge-base', ['merge-base', 'HEAD', 'main']),
                        ('index', ['diff', '--cached', '--name-status']),
                        ('status', ['status', '--porcelain=v1']),
                        ('diff-check', ['diff', '--check'])]:
        argv = ['git', '-c', 'safe.directory=' + ROOT.as_posix(), '--no-optional-locks', *args]
        result = command('entry-git-' + label + '-v1', argv)
        checks.append(dict(check=label, exit_code=result.returncode,
                           stdout=result.stdout.decode(), stderr=result.stderr.decode()))
        assert result.returncode == 0, label
    assert checks[0]['stdout'].strip() == 'codex/04A-38'
    assert all(row['stdout'].strip() == BASE for row in checks[1:4]), 'BASE_MISMATCH_STOP'
    assert not checks[4]['stdout'], 'NONEMPTY_INDEX_STOP'
    save('git-entry-v1.json', dict(checks=checks, index_file=pin(ROOT/'.git/index'), baseline=BASE,
         initial_tool_attempt='Initial plain Git commands rejected dubious ownership for sandbox SID ending 1012 versus owner SID ending 1002. Process-local safe.directory repeat succeeded. No global configuration changed.',
         initial_read_attempts='rg reported absent .agents/.codex; optional continuation-v10/README.md absent. No evidence or source was changed by these reads.'))
    git = ['git', '-c', 'safe.directory=' + ROOT.as_posix(), '--no-optional-locks']
    modified = subprocess.check_output([*git, 'diff', '--name-only', '-z'], cwd=ROOT).decode().split('\0')
    untracked = subprocess.check_output([*git, 'ls-files', '--others', '--exclude-standard', '-z', '--',
        'BESHStat.Core', 'BESHStat.Excel.Common', 'BESHStat.Core.Tests',
        'BESHStat.Excel.Common.Tests', 'BESHstatNG', 'BESHStatNG.Test'], cwd=ROOT).decode().split('\0')
    source_rows = []
    for name in sorted(set(modified + untracked) - {''}):
        path = ROOT/name
        data = path.read_bytes()
        snapshot = HERE/'source-before'/name
        snapshot = snapshot.with_name(snapshot.name+'.txt')
        snapshot.parent.mkdir(parents=True, exist_ok=True)
        with snapshot.open('xb') as stream:
            stream.write(data)
        source_rows.append(dict(live=pin(path), preimage=pin(snapshot)))
    save('all-edited-source-preimages-v1.json', dict(files=source_rows, count=len(source_rows),
        scope='Every tracked diff path plus every untracked path in the six production/test project trees; no TestResults traversal.'))
    authority_names = ['continuation-v10-final-git-preservation-v1.json',
        'continuation-v10-retained-private-preservation-v2.json',
        'continuation-v10-retained-private-reader-v2.py',
        'continuation-v10-post-publication-artifacts-v1.json',
        'continuation-v10-publication-receipt-v1.json',
        'continuation-v10-review-parts-index-v1.json',
        'continuation-v10-postseal-handover-addendum-v1.md']
    authorities = [pin(W2/name) for name in authority_names]
    authorities += [pin(OLD/name) for name in ['owner-review.md', 'review-seal-v1.json',
        'remaining-work-queue-v1.json', 'ownership-summary-v2.json']]
    authorities += [pin(ROOT/name) for name in ['AGENTS.md',
        'planning/BESHStatNG_CrossPlatform_MasterPlan.md',
        'planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md']]
    save('entry-authority-pins-v1.json', dict(authorities=authorities, original_queries=0))
    seal = json.loads((OLD/'review-seal-v1.json').read_bytes())
    assert pin(ROOT/seal['reader']['path']) == seal['reader']
    # -I excludes ambient Python paths; explicitly inject only the documented reader directory.
    bootstrap = 'import sys,runpy; sys.path.insert(0,sys.argv[1]); runpy.run_path(sys.argv[2],run_name="__main__")'
    result = command('prior-sealed-reader-v1', [sys.executable, '-I', '-B', '-c', bootstrap,
        str(OLD), str(ROOT/seal['reader']['path'])])
    save('entry-reader-outcome-v1.json', dict(reader=seal['reader'], exit_code=result.returncode,
        original_queries=0, original_model_execution=False))
    # Do not call run(): the immutable v2 generator writes its existing receipt with mode x.
    private_bootstrap = '''import sys,json,importlib.util
from pathlib import Path
p=Path(sys.argv[1]); spec=importlib.util.spec_from_file_location('prior_private_v2',p)
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
d=json.loads((p.parent/'continuation-v10-retained-private-preservation-v2.json').read_bytes())
assert m.pin(p)==d['reader']
assert len(d['private_pins'])==d['private_pin_count']==2545
for row in d['private_pins']+d['retained_prior_physical_archives']+[d['retained_interrupted_aggregation']]:m.check(row)
assert (d['retained_interrupted_aggregation']['bytes'],d['retained_interrupted_aggregation']['sha256'])==(4213129746,'3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb')
print(json.dumps(dict(private_pins=2545,archives=len(d['retained_prior_physical_archives']),protected_giant='EXACT',status='AUTHENTICATED')))
'''
    private = command('prior-private-reader-v2', [sys.executable, '-I', '-B', '-c', private_bootstrap,
        str(W2/'continuation-v10-retained-private-reader-v2.py')])
    preservation = command('prior-live-preservation-v1', [sys.executable, '-I', '-B', '-c', bootstrap,
        str(OLD), str(OLD/'verify_c10_preservation_v1.py')])
    save('entry-outcome-v1.json', dict(status='AUTHENTICATED' if all(x.returncode == 0 for x in [result, private, preservation]) else 'ENTRY_AUTHENTICATION_FAILED_STOP',
        sealed_reader_exit=result.returncode, private_reader_exit=private.returncode,
        live_preservation_exit=preservation.returncode, original_queries=0,
        production_edits=0, gates='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED'))
    assert all(x.returncode == 0 for x in [result, private, preservation]), 'ENTRY_AUTHENTICATION_FAILED_STOP'


if __name__ == '__main__':
    try:
        main()
    except Exception:
        traceback.print_exc()
        raise SystemExit(1)
