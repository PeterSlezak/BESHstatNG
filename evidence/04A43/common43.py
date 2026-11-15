"""Append-only 04A.43 evidence; execution output is protected, never a public transcript."""
from pathlib import Path
import datetime,hashlib,importlib.util,json,os,subprocess,sys
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
PRIVATE=ROOT/'.test-rationalization-04A43.tmp'
MAIN='3955a6e9142256df6bc1fd53a924010c58367b51'
PRIOR='1df856c47f2342e26de1115f1621722b7c0ac23e'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A44_NOT_AUTHORIZED')
os.environ.update(PYTHONDONTWRITEBYTECODE='1',GIT_OPTIONAL_LOCKS='0')
sys.dont_write_bytecode=True
def utc():return datetime.datetime.now(datetime.timezone.utc).isoformat()
def sha(b):return hashlib.sha256(b).hexdigest()
def load(p):return json.loads(Path(p).read_bytes())
def pin(p):
    p=Path(p)
    if not p.is_absolute():p=ROOT/p
    with p.open('rb') as f:h=hashlib.file_digest(f,'sha256').hexdigest()
    return dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size,sha256=h)
def exact(r):assert pin(r['path'])=={k:r[k] for k in ['path','bytes','sha256']},'PIN_CHANGED:'+r['path']
def save(p,d):
    p=Path(p)
    if not p.is_absolute():p=HERE/p
    p.parent.mkdir(parents=True,exist_ok=True)
    with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(d,f,indent=2,ensure_ascii=False,allow_nan=False);f.write('\n')
def git(*a):
    p=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*a],cwd=ROOT,capture_output=True)
    assert p.returncode==0,'GIT_READ_FAILED'
    return p.stdout
def module(name,p):
    spec=importlib.util.spec_from_file_location(name,p);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
def lock():
    assert git('branch','--show-current').decode().strip()=='codex/04A-43'
    assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
    assert git('merge-base','HEAD','main').decode().strip()==MAIN
    assert not git('diff','--cached','--name-only')
    assert pin('.git/index')['sha256']==load(HERE/'entry/authentication-v1.json')['index']['sha256']
def runner():return module('isolated_runner43',ROOT/'scripts/test_suite_rationalization.py').Runner(PRIVATE)
def command(name,args,**kw):return runner().command('a43-'+name,args,**kw)
