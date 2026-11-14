"""Capture actual external owner-plan drift; no closure or publication promotion."""
from common import *
import difflib
plan='planning/BESHStatNG_CrossPlatform_MasterPlan.md'
authority=json.loads((HERE/'entry-authority-pins-v1.json').read_bytes())
def walk(x):
 if isinstance(x,dict):
  if x.get('path')==plan and 'sha256' in x:yield x
  for v in x.values():yield from walk(v)
 elif isinstance(x,list):
  for v in x:yield from walk(v)
entry=list(walk(authority));assert len(entry)==1;entry=entry[0]
current=pin(ROOT/plan);assert current!=entry
folder=HERE/'preservation-stop-v1';folder.mkdir(exist_ok=False)
current_snapshot=folder/'owner-plan-live-at-stop.md'
with current_snapshot.open('xb') as f:f.write((ROOT/plan).read_bytes())
assert (pin(current_snapshot)['bytes'],pin(current_snapshot)['sha256'])==(current['bytes'],current['sha256'])
prior=ROOT/'.c13p.tmp'/plan;assert (pin(prior)['bytes'],pin(prior)['sha256'])==(entry['bytes'],entry['sha256'])
entry_snapshot=folder/'owner-plan-authenticated-entry.md'
with entry_snapshot.open('xb') as f:f.write(prior.read_bytes())
diff=''.join(difflib.unified_diff(prior.read_text(encoding='utf-8-sig').splitlines(True),(ROOT/plan).read_text(encoding='utf-8-sig').splitlines(True),fromfile='authenticated-entry',tofile='live-at-stop'))
with (folder/'owner-plan-delta.diff').open('x',encoding='utf-8',newline='\n') as f:f.write(diff)
result=command('preservation-drift-check-v1',[sys.executable,'-B',str(HERE/'verify_c14_preservation_v1.py')])
checks=[]
for label,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-status']),('status',['status','--porcelain=v1']),('diff-check',['diff','--check'])]:
 r=command('stop-git-'+label+'-v1',['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args]);checks.append(dict(check=label,execution=clean(r)))
save('preservation-stop-v1.json',dict(status='STOP_REQUIRED_PRESERVATION_DRIFT_NO_FINAL_CLOSURE_PROMOTION',entry=entry,current=current,entry_snapshot=pin(entry_snapshot),current_snapshot=pin(current_snapshot),delta=pin(folder/'owner-plan-delta.diff'),actual_live_reader=clean(result),Git=checks,index_file=pin(ROOT/'.git/index'),production_freeze=pin(HERE/'production-test-freeze-v1.json'),Python_frozen_postcheck=pin(HERE/'python-frozen-input-postcheck-v1.json'),prior_failed_nonpass_reconciliation=dict(command=pin(RUNROOT/'commands/w2c14-nonpass-source-deltas-v1/command.json'),stderr=pin(RUNROOT/'commands/w2c14-nonpass-source-deltas-v1/stderr.log')),owner_decision='Confirm authority for the changed owner plan and specify how to preserve/reconcile the entry plan versus current plan. No reset or owner document overwrite. Existing source/proof/test work remains retained and provisional. Final v14 closure and publication work is stopped.',original_queries=0))
print(json.dumps(dict(entry=entry,current=current,live_reader_exit=result['exit_code'],status='STOP_REQUIRED_PRESERVATION_DRIFT')))
