"""Actual source-bound final C13 reader and complete preservation authentication."""
from common import *
from concurrent.futures import ThreadPoolExecutor
def check(r):assert pin(ROOT/r['path'])=={k:r[k] for k in ['path','bytes','sha256']},'ENTRY_MISMATCH_STOP '+r['path']
finalpath=W2/'continuation-v13-final-git-preservation-v1.json'
assert pin(finalpath)['sha256']=='f3cb0c1a3e4ab0d098b9530b81f1294457b6d91d96521b4fb5c7d64e4f19c61f','FINAL_C13_AUTHORITY_MISMATCH_STOP'
final=json.loads(finalpath.read_bytes());authority=[pin(finalpath)]
for r in final.values():
 if isinstance(r,dict) and {'path','bytes','sha256'}<=r.keys():check(r);authority.append(r)
art=json.loads((ROOT/final['post_publication_artifacts']['path']).read_bytes())
for r in art.values():
 if isinstance(r,dict) and {'path','bytes','sha256'}<=r.keys():check(r);authority.append(r)
for r in art['source_after_images']+art['current_compiled_artifacts']:check(r)
assert art['authoritative_private_reader']['path'].endswith('continuation-v13-retained-private-reader-v2.py')
checks=[]
argslist=[('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-status']),('status',['status','--porcelain=v1']),('diff-check',['diff','--check'])]
for name,args in argslist:
 r=command('entry-git-'+name+'-v1',['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args]);folder=RUNROOT/('commands/w2c14-entry-git-'+name+'-v1');out=(folder/'stdout.log').read_bytes();assert r['exit_code']==0
 prior=next(p for p in final['Git'] if p['check']==name)
 for key in ['exact_raw_predecessor_command','exact_raw_predecessor_stdout']:check(prior[key]);authority.append(prior[key])
 assert sha(out)==prior['stdout_sha256']==prior['exact_raw_predecessor_stdout']['sha256'],'GIT_STDOUT_MISMATCH_STOP '+name
 if name=='branch':assert out.decode().strip()=='codex/04A-38'
 if name in ['HEAD','main','merge-base']:assert out.decode().strip()==BASE
 if name=='index':assert not out
 checks.append(dict(check=name,execution=clean(r),raw_command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),actual_C13_predecessor=prior['exact_raw_predecessor_stdout']))
index=pin(ROOT/'.git/index');assert index==final['index_file'] and index['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
save('git-entry-v1.json',dict(checks=checks,index_file=index,base=BASE,production_edits=0,source_bound_predecessor=pin(PREVIOUS/'capture_exit_git_v1.py')))
modified=git('diff','--name-only','-z').decode().split('\0');untracked=git('ls-files','--others','--exclude-standard','-z','--','BESHStat.Core','BESHStat.Excel.Common','BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHstatNG','BESHStatNG.Test').decode().split('\0');rows=[]
for name in sorted(set(modified+untracked)-{''}):
 source=ROOT/name;target=HERE/'source-before'/(name+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
 with target.open('xb') as f:f.write(source.read_bytes())
 rows.append(dict(live=pin(source),preimage=pin(target)))
save('all-edited-source-preimages-v1.json',dict(files=rows,count=len(rows),scope='All tracked diffs and untracked production/test/project files; no TestResults enumeration.'))
for name in ['AGENTS.md','planning/BESHStatNG_CrossPlatform_MasterPlan.md','planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md']:
 (ROOT/name).read_bytes().decode('utf-8-sig');authority.append(pin(ROOT/name))
names=['source-transitions-final-v1.json','canonical-identity-status-v1.json','new-closures-final-v1.json','remaining-work-queue-v1.json','historical-ownership-v2.json','current-ownership-v2.json','ownership-summary-v2.json','current-semantic-position-crosswalk-v1.json','original-fragment-parent-crosswalk-v2.json','new-parent-fragment-crosswalk-v2.json','support-fragment-crosswalk-v1.json','whole-body-contracts-v1.json','callback-proofs-v1.json','portable-api-contracts-v1.json','host-cache-contexts-v1.json','constructed-owner-proofs-v1.json','compiled-method-deltas-v1.json','managed-Debug-identities-v1.json','python-first-failures-v2.json','python-nonpass-investigation-v1.json','node-identities-v1.json','validation-v1.json','compatibility-packet-v1.json','frozen-provenance-v1.json','build-packet-v1-Debug.json','build-packet-v1-Release.json','open-byref-alias-defect-v1.json','inherited-failure-mutation-ledger-v2.json','independent-controls-v1.json']
for name in names:json.loads((PREVIOUS/name).read_bytes());authority.append(pin(PREVIOUS/name))
private=json.loads((ROOT/final['private_preservation']['path']).read_bytes());assert len(private['private_pins'])==private['private_pin_count']==11875 and len(private['retained_prior_physical_archives'])==57
g=private['retained_interrupted_aggregation'];assert (g['bytes'],g['sha256'])==(4213129746,'3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb')
copyroot=ROOT/'.c13p.tmp';copyreader=copyroot/PREVIOUS.relative_to(ROOT)/'verify_c13_publication_v1.py';assert sha(copyreader.read_bytes())==art['publication_reader']['sha256']
save('entry-authority-pins-v1.json',dict(authorities=authority,source_after_images=art['source_after_images'],current_compiled_artifacts=art['current_compiled_artifacts'],original_queries=0))
bootstrap='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
tasks=[('prior-sealed-reader-v2',PREVIOUS,PREVIOUS/'verify_c13_checkpoint_v2.py',[],ROOT),('prior-private-reader-v2',PREVIOUS,W2/'continuation-v13-retained-private-reader-v2.py',['--verify'],ROOT),('prior-live-preservation-v1',PREVIOUS,PREVIOUS/'verify_c13_preservation_v1.py',[],ROOT),('prior-physical-union-v1',PREVIOUS,PREVIOUS/'verify_c13_publication_v1.py',['--parts',str(W2/'continuation-v13-review-parts-index-v1.json')],ROOT),('prior-copied-default-reader-v1',copyreader.parent,copyreader,[],copyroot)]
def execute(t):
 label,directory,script,args,cwd=t
 return label,command(label,[sys.executable,'-I','-B','-c',bootstrap,str(directory),str(script),*args],cwd=cwd)
with ThreadPoolExecutor(max_workers=3) as pool:results=list(pool.map(execute,tasks))
outcomes={n:r['exit_code'] for n,r in results};ok=all(v==0 for v in outcomes.values())
save('entry-executable-authentication-v1.json',dict(status='AUTHENTICATED' if ok else 'ENTRY_AUTHENTICATION_FAILED_STOP',executions=[dict(label=n,result=clean(r)) for n,r in results],production_edits=0,original_queries=0))
assert ok,'ENTRY_AUTHENTICATION_FAILED_STOP '+str(outcomes)
print(json.dumps(dict(status='AUTHENTICATED_PRE_IMPLEMENTATION',source_preimages=len(rows),outcomes=outcomes,original_queries=0)))
