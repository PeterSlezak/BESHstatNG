"""Entry-only authentication. Never execute original sources or native controls."""
from common import *
sys.path.insert(0,str(PREVIOUS))
from verify_ownership_final import inspect as ownership
from verify_preservation_v3 import inspect as preservation
from verify_derivative_v3 import inspect as derivative
def main():
 PRIVATE.mkdir(exist_ok=False)
 checks=[]
 for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('status',['status','--short']),('diff-check',['diff','--check'])]:
  r=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True);assert r.returncode==0
  checks.append(dict(check=name,exit_code=r.returncode,stdout=r.stdout.decode(),stderr=r.stderr.decode()))
 assert checks[0]['stdout'].strip()=='codex/04A-38' and all(r['stdout'].strip()==BASE for r in checks[1:4]) and checks[4]['stdout']==''
 save('git-entry.json',dict(records=checks,index='EMPTY',branch='codex/04A-38',baseline=BASE))
 own=ownership();save('v5-ownership-recomputed.json',own)
 protected=preservation();save('v5-preservation-recomputed.json',protected)
 save('v5-diagnostic-preservation.json',derivative())
 manifest=json.loads((PREVIOUS/'publication-manifest-v1.json').read_bytes());rows=manifest['entries']
 for row in rows:
  if row['sha256'] is None:assert row['path']==(PREVIOUS/'publication-manifest-v1.json').relative_to(ROOT).as_posix() and row['bytes'] is None;continue
  assert pin(ROOT/row['path'])==row,'V5 manifest payload changed '+row['path']
 receipt=json.loads((W2/'continuation-v5-publication-receipt-v1.json').read_bytes())
 assert pin(ROOT/receipt['manifest']['path'])==receipt['manifest']
 for name in ['failed_attempt','recovery_reader']:assert pin(ROOT/receipt[name]['path'])==receipt[name]
 archive=RUNROOT/'completion-w2-cont5/publication-v1'/receipt['archive']['name']
 assert archive.stat().st_size==receipt['archive']['bytes'] and sha(archive.read_bytes())==receipt['archive']['sha256']
 save('v5-publication-hashes-recomputed.json',dict(hashed_payloads=len(rows)-1,null_self_entries=sum(r['sha256'] is None for r in rows),manifest=pin(PREVIOUS/'publication-manifest-v1.json'),archive=receipt['archive'],receipt=pin(W2/'continuation-v5-publication-receipt-v1.json'),status='ALL_BYTES_EXACT'))
 save('entry.json',dict(status='ENTRY_VERIFIED_BEFORE_SOURCE_EDITS',ownership=own,protection=protected,original_queries=0,baseline=BASE,master_plan=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'),instructions=pin(ROOT/'AGENTS.md')))
 print(json.dumps(dict(ownership={k:own[k] for k in ['historical','current','support']},tracked=protected['tracked'],payloads=len(rows)-1)))
if __name__=='__main__':main()
