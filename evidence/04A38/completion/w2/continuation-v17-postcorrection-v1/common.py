"""Append-only authorized v17 post-correction validation; no archived engine execution."""
import hashlib,json,os,subprocess,sys,time
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5]
HERE=Path(__file__).resolve().parent
W2=HERE.parent
PREVIOUS=W2/'continuation-v17'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
RUNROOT=ROOT/'.test-rationalization-04A38.tmp'
PRIVATE=RUNROOT/'completion-w2-cont17-postcorrection-v1'
DEFERRED='NOT_RUN_OWNER_DEFERRED_TO_04A38_FINALIZATION'
os.environ.update(GIT_OPTIONAL_LOCKS='0',PYTHONDONTWRITEBYTECODE='1')
sys.path.append(str(ROOT/'evidence/04A38'))
from run import m
from baseline_report import clean

def sha(data):return hashlib.sha256(data).hexdigest()
def pin(path):
 path=Path(path).resolve()
 with path.open('rb') as f:digest=hashlib.file_digest(f,'sha256').hexdigest()
 return dict(path=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=digest)
def library_pin(path):
 path=Path(path)
 with path.open('rb') as f:digest=hashlib.file_digest(f,'sha256').hexdigest()
 return dict(name=path.name,bytes=path.stat().st_size,sha256=digest)
def save(name,value):
 path=HERE/name;path.parent.mkdir(parents=True,exist_ok=True)
 with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False,allow_nan=False);f.write('\n')
def command(label,args,**kwargs):return m.Runner(RUNROOT).command('w2c17pc1-'+label,args,**kwargs)
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args],cwd=ROOT)
def bootstrap(path,*args):
 code='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
 return [sys.executable,'-I','-B','-c',code,str(Path(path).parent),str(path),*map(str,args)]
def compiler(name):
 locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
 return subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/'+name],text=True).splitlines()[0]
