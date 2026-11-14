"""Read-only final source-bound audit after all private streams have been sealed."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from common import *
def check(row):
 assert pin(ROOT/row['path'])=={k:row[k] for k in ['path','bytes','sha256']},'Final changed bytes '+row['path']
load=lambda p:json.loads(p.read_bytes())
finalpath=W2/'continuation-v14-final-git-preservation-v1.json';final=load(finalpath)
assert (final['status'],final['W2'],final['W3'],final['P'],final['N1'],final['N2'],final['C2'],final['C3'],final['science'],final['phase04A39'])==('NOT_READY_04A38','IN_PROGRESS','NOT_STARTED','NO','NO GO','NO','UNMET','UNAPPROVED','NOT_GOLDEN','BLOCKED')
for key in ['private_preservation','private_verification','live_preservation','publication','privacy_checkpoint','parts_index','copied_reader','post_publication_artifacts','postseal_addendum','postseal_tool']:check(final[key])
private=load(ROOT/final['private_preservation']['path']);old=load(W2/'continuation-v13-retained-private-preservation-v1.json');prior={r['path']:r for r in old['private_pins']};now={r['path']:r for r in private['private_pins']}
assert len(prior)==11875 and len(now)==private['private_pin_count'] and all(now.get(k)==v for k,v in prior.items())
archives={r['path']:r for r in private['retained_prior_physical_archives']};assert len(old['retained_prior_physical_archives'])==57 and all(archives.get(r['path'])==r for r in old['retained_prior_physical_archives'])
verification=load(ROOT/final['private_verification']['path']);assert verification['exit_code']==0 and json.loads(verification['stdout'])['pins']==len(now)
parts=load(ROOT/final['parts_index']['path']);paths=[r['path'] for r in parts['files']];assert len(paths)==len(set(paths))==len({p.casefold() for p in paths})
for r in parts['parts']:
 check(r);check(r['deterministic_reproduction']);assert r['bytes']<180*1024**2 and r['sha256']==r['deterministic_reproduction']['sha256']
artifacts=load(ROOT/final['post_publication_artifacts']['path'])
for r in artifacts['current_compiled_artifacts']+artifacts['source_after_images']:check(r)
for r in load(HERE/'production-test-freeze-v1.json')['sources']:check(r)
plan=load(HERE/'owner-plan-authorized-transition-v1.json')
for key in ['after','preimage','snapshot']:check(plan[key])
checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-status']),('status',['status','--porcelain=v1']),('diff-check',['diff','--check'])]:
 command=['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args];r=subprocess.run(command,cwd=ROOT,capture_output=True);oldcheck=next(c for c in final['Git'] if c['check']==name);assert r.returncode==0 and sha(r.stdout)==oldcheck['stdout_sha256'];checks.append(dict(check=name,command=clean(command),exit_code=r.returncode,stdout=r.stdout.decode(),stderr=clean(r.stderr.decode()),stdout_sha256=sha(r.stdout),stderr_sha256=sha(r.stderr)))
check(final['index_file'])
out=W2/'continuation-v14-final-audit-v1.json'
result=dict(status='LOCAL_SOURCE_BOUND_FINAL_AUDIT_PASS_NOT_READY',tool=pin(Path(__file__)),final_git=pin(finalpath),Git=checks,private_pins=len(now),inherited_private=11875,new_private=private['new_current_private_count'],archives=len(archives),parts=len(parts['parts']),logical_files=len(paths),all_parts_below180MiB=True,original_queries=0,upload='NOT_UPLOADED_BY_AGENT',external_reviewer_physical_verification='NOT_CLAIMED',no_private_files_written_after_seal=True)
with out.open('x',encoding='utf-8',newline='\n') as f:json.dump(result,f,indent=2);f.write('\n')
print(json.dumps({k:result[k] for k in ['status','private_pins','archives','parts','logical_files']}))
