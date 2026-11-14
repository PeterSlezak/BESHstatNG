"""Append-only 04A.39 evidence utilities. Historical evidence is read-only."""
from pathlib import Path
import datetime
import hashlib
import importlib.util
import json
import os
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
PRIVATE = ROOT / '.test-rationalization-04A39.tmp'
MAIN = '22da2214da6bb1cad4f2934d0b36276324a1432f'
W3 = ROOT / 'evidence/04A38/completion/w3'
GATES = dict(P='NO', N1='NO GO', N2='NO', C2='UNMET', C3='UNAPPROVED', scientific='NOT_GOLDEN')
os.environ.update(GIT_OPTIONAL_LOCKS='0', PYTHONDONTWRITEBYTECODE='1')


def utc(): return datetime.datetime.now(datetime.timezone.utc).isoformat()
def sha(b): return hashlib.sha256(b).hexdigest()
def load(p): return json.loads(Path(p).read_bytes())
def pin(p):
    p = Path(p)
    if not p.is_absolute(): p = ROOT / p
    with p.open('rb') as f: digest = hashlib.file_digest(f, 'sha256').hexdigest()
    return dict(path=p.relative_to(ROOT).as_posix(), bytes=p.stat().st_size, sha256=digest)


def save(name, value):
    p = HERE / name
    p.parent.mkdir(parents=True, exist_ok=True)
    with p.open('x', encoding='utf-8', newline='\n') as f:
        json.dump(value, f, indent=2, ensure_ascii=False, allow_nan=False)
        f.write('\n')


def git(*args):
    p = subprocess.run(['git', '-c', 'safe.directory='+ROOT.as_posix(), '--no-optional-locks', *args],
                       cwd=ROOT, capture_output=True)
    if p.returncode: raise RuntimeError((args, p.returncode, p.stderr.decode()))
    return p.stdout


def source_lock():
    assert git('rev-parse', 'HEAD').decode().strip() == MAIN
    assert git('rev-parse', 'main').decode().strip() == MAIN
    assert git('branch', '--show-current').decode().strip() == 'codex/04A-39'
    assert not git('diff', '--cached', '--name-only')
    for row in load(HERE/'entry/source-preimages-v1.json')['files']:
        if row['path'].startswith(('BESHstatNG/', 'BESHStat.Core/', 'BESHStat.Excel.Common/')):
            assert pin(row['path']) == row, 'PRODUCTION_SOURCE_CHANGED '+row['path']


def runner():
    spec = importlib.util.spec_from_file_location('accepted_isolated_runner39', ROOT/'scripts/test_suite_rationalization.py')
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module.Runner(PRIVATE)


def command(label, args, **kwargs):
    return runner().command('a39v1-'+label, args, **kwargs)


def bootstrap(script, *args):
    return [sys.executable, '-I', '-B', '-c',
            'import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")',
            str(HERE), str(HERE/script), *map(str, args)]
