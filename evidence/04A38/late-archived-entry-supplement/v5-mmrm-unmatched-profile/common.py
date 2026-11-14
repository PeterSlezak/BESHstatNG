"""Append-only v5 evidence utilities; no statistical logic or Git mutation."""
import hashlib,json,os,subprocess,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4]
HERE=Path(__file__).resolve().parent
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
CLASS='LATE_ARCHIVED_ENTRY_SUPPLEMENT'
RUNROOT=ROOT/'.test-rationalization-04A38.tmp'
PRIVATE=RUNROOT/'mmrm38-v5'
TREE=RUNROOT/'late38-v1/original-tree'
V2=ROOT/'evidence/04A38/late-archived-entry-supplement/v2-gee-profile'
V4=ROOT/'evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards'
V3=ROOT/'evidence/04A38/late-archived-entry-supplement/v3-mmrm-headered-profile'
PREVIOUS='evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards/publication-manifest-closeout-v1.json'
def sha(data):return hashlib.sha256(data).hexdigest()
def pin(path):
 data=path.read_bytes();return dict(path=path.relative_to(ROOT).as_posix(),bytes=len(data),sha256=sha(data))
def save(name,obj):
 target=HERE/name;target.parent.mkdir(parents=True,exist_ok=True)
 with target.open('x',encoding='utf-8',newline='\n') as stream:json.dump(obj,stream,indent=2,ensure_ascii=False,allow_nan=False);stream.write('\n')
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT)
def compiler(name):
 locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
 return subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/'+name],text=True).splitlines()[0]
def library_pin(p):
 data=p.read_bytes();return dict(name=p.name,bytes=len(data),sha256=sha(data))
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import m
from baseline_report import clean
def command(label,args,**kwargs):return m.Runner(RUNROOT).command('mmrm38-v5-'+label,args,**kwargs)
