"""Append-only offline 04A.46 evidence helpers; raw diagnostics stay owner-local."""
from pathlib import Path
import datetime,hashlib,importlib.util,json,os,subprocess,sys
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
PRIVATE=ROOT/'.test-rationalization-04A46.tmp'
MAIN='ebc4a3e9df41ef8588aa7f6b25363f6d044c41a7'
PRIOR='350ffbc0924b09199249316a3ee2f31bc3dd98e8'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A47_NOT_AUTHORIZED')
os.environ.update(PYTHONDONTWRITEBYTECODE='1',GIT_OPTIONAL_LOCKS='0')
sys.dont_write_bytecode=True
def utc():return datetime.datetime.now(datetime.timezone.utc).isoformat()
def sha(b):return hashlib.sha256(b).hexdigest()
def load(p):return json.loads(Path(p).read_bytes())
def pin(p):
 p=Path(p);p=p if p.is_absolute() else ROOT/p
 with p.open('rb') as f:h=hashlib.file_digest(f,'sha256').hexdigest()
 return dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size,sha256=h)
def exact(r):assert pin(r['path'])=={k:r[k] for k in ['path','bytes','sha256']},'PIN_CHANGED:'+r['path']
def save(p,d):
 p=Path(p);p=p if p.is_absolute() else HERE/p;p.parent.mkdir(parents=True,exist_ok=True)
 with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(d,f,indent=2,ensure_ascii=False,allow_nan=False);f.write('\n')
def git(*a):
 p=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*a],cwd=ROOT,capture_output=True)
 assert p.returncode==0,'GIT_READ_FAILED'
 return p.stdout
def module(name,p):
 spec=importlib.util.spec_from_file_location(name,p);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
def lock():
 assert git('branch','--show-current').decode().strip()=='codex/04A-46'
 assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
 assert git('merge-base','HEAD','main').decode().strip()==MAIN
 assert not git('diff','--cached','--name-only')
 assert pin('.git/index')['sha256']==load(HERE/'entry/authentication-v1.json')['index']['sha256']
def runner():return module('isolated_runner46',ROOT/'scripts/test_suite_rationalization.py').Runner(PRIVATE)
def command(name,args,**kw):return runner().command('a46-'+name,args,**kw)
