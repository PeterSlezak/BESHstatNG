"""04A39 reporting correction. Never overwrite an earlier checkpoint or raw result."""
from pathlib import Path
import datetime
import hashlib
import json
import os
import subprocess
import sys

ROOT=Path(__file__).resolve().parents[3]
HERE=Path(__file__).resolve().parent
PRIVATE=ROOT/'.test-rationalization-04A39-correction-v1.tmp'
PRIOR=ROOT/'evidence/04A39'
MAIN='22da2214da6bb1cad4f2934d0b36276324a1432f'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A40_NOT_AUTHORIZED')
os.environ.update(PYTHONDONTWRITEBYTECODE='1',GIT_OPTIONAL_LOCKS='0')

def utc():return datetime.datetime.now(datetime.timezone.utc).isoformat()
def sha(data):return hashlib.sha256(data).hexdigest()
def load(path):return json.loads(Path(path).read_bytes())
def digest(path):
    with Path(path).open('rb') as stream:return hashlib.file_digest(stream,'sha256').hexdigest()
def pin(path):
    path=Path(path)
    if not path.is_absolute():path=ROOT/path
    return dict(path=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=digest(path))
def save(path,data):
    path=Path(path)
    if not path.is_absolute():path=HERE/path
    path.parent.mkdir(parents=True,exist_ok=True)
    with path.open('x',encoding='utf-8',newline='\n') as stream:
        json.dump(data,stream,indent=2,ensure_ascii=False,allow_nan=False);stream.write('\n')
def git(*args):
    p=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args],cwd=ROOT,capture_output=True)
    assert p.returncode==0,(args,p.stderr)
    return p.stdout
def lock():
    assert git('branch','--show-current').decode().strip()=='codex/04A-39'
    assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
    assert not git('diff','--cached','--name-only')
    frozen=load(HERE/'entry/source-lock.json')
    for row in frozen['files']:assert pin(row['path'])==row,'SOURCE_CHANGED '+row['path']
    assert pin('.git/index')==frozen['index']

def command(name,args):
    folder=PRIVATE/'commands'/name;folder.mkdir(parents=True,exist_ok=False)
    start=utc()
    with (folder/'stdout.log').open('xb') as out,(folder/'stderr.log').open('xb') as err:
        p=subprocess.run(list(map(str,args)),cwd=ROOT,stdout=out,stderr=err,creationflags=subprocess.CREATE_NO_WINDOW)
    row=dict(args=list(map(str,args)),start_utc=start,end_utc=utc(),exit_code=p.returncode,
        stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'))
    save(folder/'command.json',row)
    return row
