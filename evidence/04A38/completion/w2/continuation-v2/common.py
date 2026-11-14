"""Append-only04A38 current-only continuation; no original execution route."""
import hashlib,json,subprocess,sys,os
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
W2=HERE.parent;W1=W2.parent/'w1';PREVIOUS=W2/'continuation-v1'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import PRIVATE as RUNROOT,m
from baseline_report import clean
PRIVATE=RUNROOT/'completion-w2-cont2'
def sha(b):return hashlib.sha256(b).hexdigest()
def pin(p):
 b=p.read_bytes();return dict(path=p.relative_to(ROOT).as_posix(),bytes=len(b),sha256=sha(b))
def library_pin(p):
 b=p.read_bytes();return dict(name=p.name,bytes=len(b),sha256=sha(b))
def save(name,obj):
 p=HERE/name;p.parent.mkdir(parents=True,exist_ok=True)
 with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(obj,f,indent=2,ensure_ascii=False,allow_nan=False);f.write('\n')
def command(label,args,**kwargs):return m.Runner(RUNROOT).command('w2c2-'+label,args,**kwargs)
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT)
def compiler(name):
 locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
 return subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/'+name],text=True).splitlines()[0]
def write_source(path,text):
 p=ROOT/path;old=p.read_bytes();nl='\r\n' if b'\r\n' in old else '\n'
 p.write_bytes((b'\xef\xbb\xbf' if old.startswith(b'\xef\xbb\xbf') else b'')+text.replace('\r\n','\n').replace('\n',nl).encode('utf-8'))
