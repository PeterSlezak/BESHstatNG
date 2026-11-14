"""Last independent live Git/index/private/physical preservation seal."""
from common import *
from verify_c10_preservation_v1 import inspect
os.environ['GIT_OPTIONAL_LOCKS']='0'
pubpath=W2/'continuation-v10-publication-receipt-v1.json';pub=json.loads(pubpath.read_bytes());privatepath=W2/'continuation-v10-retained-private-preservation-v1.json';private=json.loads(privatepath.read_bytes());parts_path=W2/'continuation-v10-review-parts-index-v1.json';parts=json.loads(parts_path.read_bytes())
assert private['status']=='POST_PUBLICATION_EXACT_RETAINED_PRIVATE_HASH_PASS'
for row in private['private_pins']+private['retained_prior_physical_archives']+[private['retained_interrupted_aggregation']]:assert pin(ROOT/row['path'])=={k:row[k] for k in ['path','bytes','sha256']}
protected=inspect();checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('status',['status','--short']),('diff-check',['diff','--check'])]:
    r=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args],cwd=ROOT,capture_output=True);assert r.returncode==0;checks.append(dict(check=name,exit_code=r.returncode,stdout=r.stdout.decode(),stderr=r.stderr.decode(),stdout_sha256=sha(r.stdout),stderr_sha256=sha(r.stderr)))
assert checks[0]['stdout'].strip()=='codex/04A-38' and all(r['stdout'].strip()==BASE for r in checks[1:4]) and checks[4]['stdout']==''
assert pin(ROOT/'.git/index')==json.loads((HERE/'git-entry-v1.json').read_bytes())['index_file']
doc=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',Git=checks,index='EMPTY',index_file=pin(ROOT/'.git/index'),preservation=protected,publication_receipt=pin(pubpath),private_preservation_receipt=pin(privatepath),review_parts_index=pin(parts_path),standalone_archive=pub['archive'],original_queries=0,scope='Authoritative live repository: all16 owner documents, protected278/100/41 groups, tracked path census/preimages/successors and explicit retained private/physical artifacts. Selected ZIP integrity is independent. No historical TestResults traversal, source/reference regeneration, staging/commit/merge/rebase/push/reset/clean, native/scientific work or W3/04A39.')
target=W2/'continuation-v10-final-git-preservation-v1.json'
with target.open('x',encoding='utf-8',newline='\n') as f:json.dump(doc,f,ensure_ascii=False,indent=2);f.write('\n')
print(json.dumps(dict(receipt=pin(target),branch='codex/04A-38',HEAD=BASE,index='EMPTY',index_sha256=doc['index_file']['sha256'],diff_check_exit=0)))
