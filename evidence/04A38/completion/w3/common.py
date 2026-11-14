"""W3 append-only tooling; no production/test or historical evidence writes."""
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
W2 = HERE.parent/'w2'
ACCEPTED = W2/'continuation-v17-postcorrection-v1'
RUNROOT = ROOT/'.test-rationalization-04A38.tmp'
PRIVATE = RUNROOT/'w3v1'
LABEL = 'w3v1'
os.environ.update(GIT_OPTIONAL_LOCKS='0', PYTHONDONTWRITEBYTECODE='1')
spec = importlib.util.spec_from_file_location('w3_accepted_runner', ROOT/'scripts/test_suite_rationalization.py')
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)


def load(p): return json.loads(Path(p).read_bytes())


def sha(data): return hashlib.sha256(data).hexdigest()


def pin(p):
    p = Path(p)
    with p.open('rb') as f: digest=hashlib.file_digest(f,'sha256').hexdigest()
    return dict(path=p.relative_to(ROOT).as_posix(), bytes=p.stat().st_size, sha256=digest)


def save(p, value):
    p = HERE/p
    p.parent.mkdir(parents=True, exist_ok=True)
    with p.open('x', encoding='utf-8', newline='\n') as f:
        json.dump(value,f,indent=2,ensure_ascii=False,allow_nan=False)
        f.write('\n')


def lock():
    assert load(HERE/'entry/entry-manifest-v2.json')['status']=='ENTRY_AUTHENTICATED', 'ENTRY_STOP'
    for r in load(ACCEPTED/'production-test-freeze-v1.json')['sources']:
        assert pin(ROOT/r['path'])==r, 'SOURCE_LOCK_CHANGED '+r['path']


def compiler(name):
    locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    return subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/'+name],text=True).splitlines()[0]


def command(label, args, **kwargs):
    return m.Runner(RUNROOT).command('w3v1-'+label,args,**kwargs)


def bootstrap(script,*args):
    return [sys.executable,'-I','-B','-c',
            'import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")',
            str(HERE),str(script),*map(str,args)]
