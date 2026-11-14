"""Final live receipts outside the sealed C6 directory; never edit the packet."""
from common import *
from verify_preservation_v1 import inspect
receipt=W2/'continuation-v6-publication-receipt-v1.json'
publication=json.loads(receipt.read_bytes())
assert publication['status']=='C6_PARTIAL_W2_NOT_READY_LIVE_COPY_PUBLICATION_VERIFIED'
assert publication['copied_executable']['exit_code']==0
assert publication['manifest']==pin(ROOT/publication['manifest']['path'])
archive=PRIVATE/'publication-v1'/publication['archive']['name']
assert archive.stat().st_size==publication['archive']['bytes'] and sha(archive.read_bytes())==publication['archive']['sha256']
preserved=inspect();checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('status',['status','--short']),('diff-check',['diff','--check'])]:
 r=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True)
 assert r.returncode==0
 checks.append(dict(check=name,exit_code=r.returncode,stdout=clean(r.stdout.decode()),stderr=clean(r.stderr.decode())))
assert checks[0]['stdout'].strip()=='codex/04A-38' and all(r['stdout'].strip()==BASE for r in checks[1:4]) and checks[4]['stdout']==''
dest=W2/'continuation-v6-final-git-preservation-v1.json'
with dest.open('x',encoding='utf-8',newline='\n') as f:
 json.dump(dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',preservation=preserved,Git=checks,index='EMPTY',publication_receipt=pin(receipt),original_queries=0),f,ensure_ascii=False,indent=2);f.write('\n')
print('Final live Git/index/diff and protected evidence PASS; W2 partial, W3 NOT_STARTED')
