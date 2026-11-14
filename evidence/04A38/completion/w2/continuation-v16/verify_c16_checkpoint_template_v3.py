"""Additive final v16 reader: unchanged v2 proofs plus two exact original-only identities."""
import sys,json,hashlib,runpy,importlib.util
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
sys.path.insert(0,str(HERE))
AUTH=__AUTH__
def compute(root=ROOT,overrides=None,predecessor=True):
 root=Path(root);overrides=overrides or {}
 for name,digest in AUTH.items():
  if name in overrides and overrides[name] is None:raise FileNotFoundError(name)
  data=overrides[name] if name in overrides else (root/name).read_bytes()
  assert hashlib.sha256(data).hexdigest()==digest,'Final additive v16 source/evidence changed '+name
 spec=importlib.util.spec_from_file_location('c16_immutable_sealed_base_v2',HERE/'verify_c16_checkpoint_v2.py');base=importlib.util.module_from_spec(spec);spec.loader.exec_module(base)
 result=base.compute(root,overrides,predecessor)
 for name in ['reconcile_original_only_callbacks_v1.py','reconcile_ownership_addendum_v1.py']:runpy.run_path(str(HERE/name),run_name='__c16_final_original_callback_proof__')
 load=lambda name:json.loads((HERE/name).read_bytes())
 history=load('historical-ownership-v2.json')['members'];new=load('new-closures-final-v2.json')
 assert len(history)==704 and sum(bool(h['proof']) for h in history)==607
 assert (new['historical_new'],new['current_new'],new['distinct_support_new'])==(70,69,9)
 assert len(load('remaining-work-queue-v2.json')['members'])==97
 assert load('ownership-summary-v2.json')['historical']==dict(total=704,closed=607,OPEN=97)
 ledger=load('canonical-identity-status-v2.json')['entries'];assert len(ledger)==1733
 assert sum(r['population']=='historical' and r['supported'] for r in ledger)==607
 assert all(r['supported']==(r['transition']!='OPEN') for r in ledger)
 result.update(historical_supported=607,historical_OPEN=97,historical_new=70,current_new=69,support_new=9,original_only_callbacks=3,final_review='final/owner-review.md',final_ledger='canonical-identity-status-v2.json',final_queue='remaining-work-queue-v2.json',qualification='All old seals/readers and nonfinal publication versions are preserved. Two independently proved original-only callbacks correct the final separate identity dispositions; no production/test source or campaign changed.')
 return result
if __name__=='__main__':
 try:print(json.dumps(compute()),flush=True)
 except (AssertionError,ValueError,OSError,KeyError,TypeError) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))),flush=True);raise SystemExit(1)
