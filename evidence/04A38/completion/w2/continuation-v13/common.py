"""Append-only C13 W2 tools; original code is never executed or captured."""
import hashlib,json,os,subprocess,sys,time
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5]
HERE=Path(__file__).resolve().parent
W2=HERE.parent
PREVIOUS=W2/'continuation-v12'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import PRIVATE as RUNROOT,m
from baseline_report import clean
PRIVATE=RUNROOT/'completion-w2-cont13'
os.environ['GIT_OPTIONAL_LOCKS']='0'
os.environ['PYTHONDONTWRITEBYTECODE']='1'
def sha(data):return hashlib.sha256(data).hexdigest()
def pin(path):
    path=Path(path)
    with path.open('rb') as stream:digest=hashlib.file_digest(stream,'sha256').hexdigest()
    return dict(path=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=digest)
def library_pin(path):
    path=Path(path)
    with path.open('rb') as stream:digest=hashlib.file_digest(stream,'sha256').hexdigest()
    return dict(name=path.name,bytes=path.stat().st_size,sha256=digest)
def save(name,value):
    target=HERE/name;target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('x',encoding='utf-8',newline='\n') as stream:json.dump(value,stream,indent=2,ensure_ascii=False,allow_nan=False);stream.write('\n')
def command(label,args,**kwargs):return m.Runner(RUNROOT).command('w2c13-'+label,args,**kwargs)
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args],cwd=ROOT)
def compiler(name):
    locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    return subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/'+name],text=True).splitlines()[0]
def write_source(path,text):
    target=ROOT/path;before=target.read_bytes();newline='\r\n' if b'\r\n' in before else '\n'
    target.write_bytes((b'\xef\xbb\xbf' if before.startswith(b'\xef\xbb\xbf') else b'')+text.replace('\r\n','\n').replace('\n',newline).encode('utf-8'))
