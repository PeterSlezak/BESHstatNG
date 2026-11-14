"""Append-only physical ZIP witness; old evidence, product and tests are read-only."""
from pathlib import Path
import datetime,hashlib,json,os,subprocess,sys,importlib.util

ROOT=Path(__file__).resolve().parents[3]
HERE=Path(__file__).resolve().parent
OLD=ROOT/'evidence/04A39/correction-v1'
PRIVATE=ROOT/'.test-rationalization-04A39-physical-witness-v1.tmp'
MAIN='22da2214da6bb1cad4f2934d0b36276324a1432f'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A40_NOT_AUTHORIZED')
os.environ.update(PYTHONDONTWRITEBYTECODE='1',GIT_OPTIONAL_LOCKS='0')
def utc():return datetime.datetime.now(datetime.timezone.utc).isoformat()
def sha(data):return hashlib.sha256(data).hexdigest()
def digest(path):
    with Path(path).open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()
def load(path):return json.loads(Path(path).read_bytes())
def pin(path):
    path=Path(path)
    if not path.is_absolute():path=ROOT/path
    return dict(path=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=digest(path))
def save(path,value):
    path=Path(path)
    if not path.is_absolute():path=HERE/path
    path.parent.mkdir(parents=True,exist_ok=True)
    with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,ensure_ascii=False,indent=2,allow_nan=False);f.write('\n')
def text(path,value):
    path=HERE/path;path.parent.mkdir(parents=True,exist_ok=True)
    with path.open('x',encoding='utf-8',newline='\n') as f:f.write(value)
def git(*args):
    command=['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args]
    p=subprocess.run(command,cwd=ROOT,capture_output=True)
    assert p.returncode==0, 'READ_ONLY_GIT_FAILED'
    return p.stdout
def imported(name,path):
    spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
def legacy():
    if str(OLD) not in sys.path:sys.path.insert(0,str(OLD))
    import common
    return common
def lock():
    legacy().lock()
    assert git('merge-base','HEAD','main').decode().strip()==MAIN
    assert not git('diff','--check')
def verify_frozen():
    lock()
    for row in load(HERE/'entry.json')['old_correction_files']:assert pin(row['path'])==row,'OLD_CORRECTION_CHANGED'
    for row in load(HERE/'entry.json')['input_archives']:assert pin(row['path'])==row,'INPUT_ZIP_CHANGED'
