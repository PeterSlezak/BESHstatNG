"""04A40 additive evidence; raw process diagnostics remain task-private."""
from pathlib import Path
import datetime,hashlib,importlib.util,json,os,subprocess,sys
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
PRIVATE=ROOT/'.test-rationalization-04A40.tmp'
MAIN='f2eb060c9c422c1fec4e82989d07907dbd4b9337'
PREVIOUS='22da2214da6bb1cad4f2934d0b36276324a1432f'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A41_NOT_AUTHORIZED')
os.environ.update(GIT_OPTIONAL_LOCKS='0',PYTHONDONTWRITEBYTECODE='1')
def utc():return datetime.datetime.now(datetime.timezone.utc).isoformat()
def sha(data):return hashlib.sha256(data).hexdigest()
def load(path):return json.loads(Path(path).read_bytes())
def pin(path):
    path=Path(path)
    if not path.is_absolute():path=ROOT/path
    with path.open('rb') as f:digest=hashlib.file_digest(f,'sha256').hexdigest()
    return dict(path=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=digest)
def save(path,value):
    path=Path(path)
    if not path.is_absolute():path=HERE/path
    path.parent.mkdir(parents=True,exist_ok=True)
    with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,ensure_ascii=False,indent=2,allow_nan=False);f.write('\n')
def git(*args):
    p=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args],cwd=ROOT,capture_output=True)
    assert p.returncode==0,'READ_ONLY_GIT_FAILED'
    return p.stdout
def module(name,path):
    spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
def runner():return module('isolated_runner40',ROOT/'scripts/test_suite_rationalization.py').Runner(PRIVATE)
def command(name,args,**kwargs):return runner().command('a40-'+name,args,**kwargs)
def lock():
    assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
    assert git('branch','--show-current').decode().strip()=='codex/04A-40'
    assert not git('diff','--cached','--name-only')
    assert pin('BESHstatNG/Generated/VersionAttributes.vb')==load(HERE/'entry/authentication-v1.json')['preexisting_owner_edit']
