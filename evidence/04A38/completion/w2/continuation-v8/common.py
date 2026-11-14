"""C8 current-only append-only work; no original source execution or restore."""
import hashlib, json, os, subprocess, sys, time
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5]
HERE=Path(__file__).resolve().parent
W2=HERE.parent
PREVIOUS=W2/'continuation-v7'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import PRIVATE as RUNROOT,m
from baseline_report import clean
PRIVATE=RUNROOT/'completion-w2-cont8'
def sha(data):return hashlib.sha256(data).hexdigest()
def pin(path):
    path=Path(path);data=path.read_bytes()
    return dict(path=path.relative_to(ROOT).as_posix(),bytes=len(data),sha256=sha(data))
def library_pin(path):
    path=Path(path);data=path.read_bytes()
    return dict(name=path.name,bytes=len(data),sha256=sha(data))
def save(name,value):
    path=HERE/name;path.parent.mkdir(parents=True,exist_ok=True)
    with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,ensure_ascii=False,indent=2,allow_nan=False);f.write('\n')
def command(label,args,**kwargs):return m.Runner(RUNROOT).command('w2c8-'+label,args,**kwargs)
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args],cwd=ROOT)
def compiler(name):
    locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    return subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/'+name],text=True).splitlines()[0]
def write_source(path,text):
    target=ROOT/path;before=target.read_bytes();newline='\r\n' if b'\r\n' in before else '\n'
    target.write_bytes((b'\xef\xbb\xbf' if before.startswith(b'\xef\xbb\xbf') else b'')+text.replace('\r\n','\n').replace('\n',newline).encode('utf-8'))
