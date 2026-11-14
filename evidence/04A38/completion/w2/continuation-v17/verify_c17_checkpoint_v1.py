"""Read-only final source proposal; comprehensive final-source gaps stay explicit."""
import sys,json,hashlib,subprocess,runpy,os
from pathlib import Path
from collections import Counter
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d';DEFERRED='NOT_RUN_OWNER_DEFERRED_TO_04A38_FINALIZATION'
def validate_ledger(doc):
 rows=doc['entries'];assert len(rows)==1733 and len({(r['population'],r['identity']) for r in rows})==1733
 expected={'historical':(607,97),'current':(635,91),'support':(271,32)}
 for p,(retained,new) in expected.items():
  subset=[r for r in rows if r['population']==p]
  assert Counter(r['transition'] for r in subset)==Counter(RETAINED=retained,NEW=new)
  assert all(r['supported'] and r['prior_supported']==(r['transition']=='RETAINED') for r in subset)
  for r in subset:
   if p=='current' and r['transition']=='RETAINED':assert r['proof'] is None
   else:assert r['proof']
  # Retained current rows preserve the inherited schema (no separate proof
  # field). The full read-back independently requires every one of their635
  # exact complete body hashes to equal the supported v16 source record.
def validate_frozen(doc,prior):
 assert doc['all_exact'] and doc['new_original_queries']==0 and doc['totals_per_configuration']=={'Debug':29424}
 before={r['corpus']:r for r in prior['results']};rows=doc['results']
 assert len(rows)==len(before)==12 and {r['corpus'] for r in rows}==set(before)
 for r in rows:
  assert r['configuration']=='Debug' and r['exact'] and r['command']['exit_code']==0
  assert all(r[k]==before[r['corpus']][k] for k in ['original','inputs','recorder','cases'])
  assert (r['current']['bytes'],r['current']['sha256'])==(r['original']['bytes'],r['original']['sha256'])
 assert sum(r['cases'] for r in rows if r['corpus'] in 'ABCDEFG')==458
 assert sum(r['cases'] for r in rows if r['corpus'] not in 'ABCDEFG')==28966
def compute(root=ROOT,overrides=None,proofs=True):
 root=Path(root);assert root.resolve()==ROOT.resolve(),'Live reader requires its real repository; use the transport reader for a Git-free copy'
 overrides=overrides or {};prefix=HERE.relative_to(ROOT).as_posix()+'/'
 def raw(name):
  if name in overrides:
   if overrides[name] is None:raise FileNotFoundError(name)
   return overrides[name]
  return (root/name).read_bytes()
 load=lambda name:json.loads(raw(prefix+name))
 seal=load('review-seal-v1.json')
 for row in seal['files']:
  data=raw(row['path']);assert (len(data),hashlib.sha256(data).hexdigest())==(row['bytes'],row['sha256']),'Sealed source/evidence drift '+row['path']
 assert hashlib.sha256(raw('planning/BESHStatNG_CrossPlatform_MasterPlan.md')).hexdigest()=='0ee45eae70aa35d6c87c636d82c3f33e105b8c12e6e5a4badb69e38fe7ad69dd'
 freeze=load('production-test-freeze-v7.json')
 for r in freeze['sources']:
  data=raw(r['path']);assert (len(data),hashlib.sha256(data).hexdigest())==(r['bytes'],r['sha256'])
 for t in load('source-transitions-final-v1.json')['files']:
  assert raw(t['path'])==raw(t['snapshot']['path'])
  if t['before'] and t['changed']:
   b=raw(t['preimage']['path']);assert (len(b),hashlib.sha256(b).hexdigest())==(t['before']['bytes'],t['before']['sha256'])
 validate_ledger(load('canonical-identity-status-v3.json'))
 history=load('historical-ownership-v3.json')['members'];current=load('current-ownership-v3.json');queue=load('remaining-work-queue-v3.json')
 assert len(history)==704 and all(h['proof'] for h in history)
 assert len(current['members'])==726 and all(not r['review_status'].startswith('OPEN') for r in current['members'])
 assert len(current['support'])==325 and sum(r['distinct'] for r in current['support'])==303 and all(r['fully_reconciled'] for r in current['support'])
 assert queue['members']==queue['current_OPEN']==queue['support_OPEN']==[]
 original=load('original-fragment-parent-crosswalk-v3.json')['rows'];assert len(original)==20683 and all(r['covered'] for r in original)
 assert len(current['fragments'])==12659 and all(not r['review_status'].startswith('OPEN') for r in current['fragments'])
 contracts=load('whole-body-contracts-v2.json')['contracts'];callbacks=load('callback-proofs-v2.json')['rows'];properties=load('original-only-property-proofs-v2.json')['rows']
 assert len(contracts)==82 and len(callbacks)==11 and len(properties)==4
 entry=load('entry-queue-receipt-v1.json')['rows'];assert {r['original_id'] for r in entry}=={r['original_id'] for r in contracts+callbacks+properties} and len(entry)==97
 assert not load('source-composition-proof-v3.json')['unresolved']
 compat=load('compatibility-combined-v7.json');prior=json.loads(raw('evidence/04A38/completion/w2/continuation-v16/compatibility-combined-v3.json'));validate_frozen(compat,prior)
 for r in compat['results']:
  for k in ['original','inputs','recorder','current']:
   b=raw(r[k]['path']);assert (len(b),hashlib.sha256(b).hexdigest())==(r[k]['bytes'],r[k]['sha256'])
 exports=load('final-v7/exports-Debug.json')['exports'];original_exports=json.loads(raw('evidence/04A38/preedit/compiled-Debug.json'))['exports']
 assert len(exports)==268 and sorted(exports,key=lambda r:r['effective_excel_name'])==sorted(original_exports,key=lambda r:r['effective_excel_name'])
 q=load('final-validation-qualification-v1.json')
 for category in ['Release_compilation','Release_full_selected_focused_unit_tests','Release_ABI_CIL_signatures_exports','Release_frozen_compatibility']:assert q[category]==DEFERRED
 assert q['managed_comprehensive']['campaigns']==1 and not q['managed_comprehensive']['final_source_comprehensive']
 assert q['managed_comprehensive']['PASS']==21076 and q['managed_comprehensive']['FAIL']==16
 assert q['managed_effective']['identities']==21094 and q['managed_effective']['PASS']==21080 and q['managed_effective']['inherited_FAIL']==14
 assert q['Python']['campaigns']==1 and q['Python']['parents']==956 and not q['Python']['final_source_comprehensive'] and q['Python']['replacement_campaigns']==0
 assert (q['Python']['PASS'],q['Python']['FAIL'],q['Python']['ERROR'])==(830,113,13)
 assert (q['Python']['subtest_FAIL'],q['Python']['subtest_ERROR'])==(185,13)
 assert q['Node']['PASS']==916 and q['strict_profile_MkDocs']==dict(strict=1,profile=1,docs=0)
 assert q['W3']=='NOT_STARTED' and q['phase04A39']=='BLOCKED' and q['original_queries']==0
 chain=load('final-prior-reader-chain-v1.json');assert chain['historical_assertions_changed'] is False and chain['projected_existing_sources']==13 and chain['status'].startswith('UNCHANGED_FINAL_V16_AND_PREDECESSORS_PASS')
 if proofs:
  sys.path.insert(0,str(HERE));import common
  oldsave=common.save
  def readback(name,value):assert json.loads((HERE/name).read_bytes())==value,'Independent proof read-back differs '+name
  common.save=readback
  try:
   for name in ['reconcile_complete_source_v4.py','build_complete_contracts_v6.py','reconcile_compiled_contexts_v3.py','reconcile_support_and_fragments_v3.py','build_fragment_transition_matrix_v1.py']:
    runpy.run_path(str(HERE/name),run_name='__c17_readonly_complete_proof__')
   args=sys.argv;sys.argv=['build_ownership_v3.py','--verify']
   try:runpy.run_path(str(HERE/'build_ownership_v3.py'),run_name='__c17_readonly_ledger__')
   finally:sys.argv=args
  finally:common.save=oldsave
 env=os.environ.copy();env['GIT_OPTIONAL_LOCKS']='0'
 def git(*args):return subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),'--no-optional-locks',*args],cwd=root,env=env)
 assert git('branch','--show-current').decode().strip()=='codex/04A-38'
 for args in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')]:assert git(*args).decode().strip()==BASE
 assert not git('diff','--cached','--name-only')
 subprocess.run(['git','-c','safe.directory='+root.as_posix(),'--no-optional-locks','diff','--check'],cwd=root,env=env,check=True,capture_output=True)
 index=raw('.git/index');assert len(index)==620185 and hashlib.sha256(index).hexdigest()=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
 return dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical=dict(supported=704,OPEN=0),current=dict(supported=726,OPEN=0,physical_Windows=723,moved_to_Common=3),support=dict(supported=303,OPEN=0,raw=325),Debug_frozen=29424,exports=268,final_source_comprehensive_managed='NOT_RUN_REQUIRES_OWNER_REPLACEMENT_AUTHORITY',final_source_comprehensive_Python='NOT_RUN_REQUIRES_OWNER_REPLACEMENT_AUTHORITY',Release_compilation=DEFERRED,Release_unit_tests=DEFERRED,Release_ABI_CIL_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,original_queries=0,gate='P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED')
if __name__=='__main__':
 try:print(json.dumps(compute()),flush=True)
 except (AssertionError,ValueError,OSError,KeyError,TypeError) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))),flush=True);raise SystemExit(1)
