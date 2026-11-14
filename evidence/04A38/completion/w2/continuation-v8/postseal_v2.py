"""Separate live Git/index/protection receipt after physical publication succeeds."""
from common import *
from verify_c8_preservation_v1 import inspect
os.environ['GIT_OPTIONAL_LOCKS']='0'
publication=W2/'continuation-v8-publication-receipt-v1.json';receipt=json.loads(publication.read_bytes())
archive=ROOT/receipt['archive']['path'];assert archive.stat().st_size==receipt['archive']['bytes']
with archive.open('rb') as stream:assert hashlib.file_digest(stream,'sha256').hexdigest()==receipt['archive']['sha256']
protected=inspect();checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('status',['status','--short']),('diff-check',['diff','--check'])]:
    result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args],cwd=ROOT,capture_output=True);assert result.returncode==0
    checks.append(dict(check=name,exit_code=result.returncode,stdout=result.stdout.decode(),stderr=result.stderr.decode()))
assert checks[0]['stdout'].strip()=='codex/04A-38' and all(r['stdout'].strip()==BASE for r in checks[1:4]) and checks[4]['stdout']==''
index=ROOT/'.git/index';assert index.is_file()
value=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',Git=checks,index='EMPTY',index_file=pin(index),preservation=protected,publication_receipt=pin(publication),standalone_archive=receipt['archive'],original_queries=0,scope='Live authoritative repository and explicitly protected/transitioned files, all16 owner documents, retained private diagnostics plus external ZIP. No ignored TestResults traversal or deletion; no staging, commit, merge, rebase, push, reset, clean or remote changes.')
path=W2/'continuation-v8-final-git-preservation-v1.json'
with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(value,stream,ensure_ascii=False,indent=2);stream.write('\n')
print(json.dumps(dict(receipt=pin(path),branch='codex/04A-38',HEAD=BASE,index='EMPTY',index_sha256=value['index_file']['sha256'],diff_check_exit=0,protected_tracked=4695,original_queries=0)),flush=True)
