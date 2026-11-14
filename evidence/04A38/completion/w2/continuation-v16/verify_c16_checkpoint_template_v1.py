"""Read-only v16 Debug-only partial checkpoint; no build, test or original execution."""
import sys,json,hashlib,runpy,subprocess,posixpath
from pathlib import Path
from collections import Counter
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
sys.path.insert(0,str(HERE))
AUTH=__AUTH__
DEFERRED='NOT_RUN_OWNER_DEFERRED_TO_04A38_FINALIZATION'
def need(value,message):
 if not value:raise ValueError(message)
def compute(root=ROOT,overrides=None,predecessor=True):
 root=Path(root);overrides=overrides or {};cache={};prefix=HERE.relative_to(ROOT).as_posix()+'/'
 def raw(path):
  path=posixpath.normpath(path)
  if path not in cache:
   if path in overrides and overrides[path] is None:raise FileNotFoundError(path)
   cache[path]=overrides[path] if path in overrides else (root/path).read_bytes()
  return cache[path]
 load=lambda path:json.loads(raw(path))
 def check(row):need((len(raw(row['path'])),hashlib.sha256(raw(row['path'])).hexdigest())==(row['bytes'],row['sha256']),'Changed source/evidence '+row['path'])
 for path,digest in AUTH.items():need(hashlib.sha256(raw(path)).hexdigest()==digest,'Sealed v16 input changed '+path)
 for t in load(prefix+'source-transitions-final-v1.json')['files']:
  check(t['after']);check(t['snapshot']);need(raw(t['path'])==raw(t['snapshot']['path']),'Source final snapshot differs')
  if t['before']:
   check(t['preimage']);need((t['before']['bytes'],t['before']['sha256'])==(t['preimage']['bytes'],t['preimage']['sha256']),'Former source not preserved exactly')
 for row in load(prefix+'production-test-freeze-v3.json')['sources']:check(row)
 need(hashlib.sha256(raw('planning/BESHStatNG_CrossPlatform_MasterPlan.md')).hexdigest()=='0ee45eae70aa35d6c87c636d82c3f33e105b8c12e6e5a4badb69e38fe7ad69dd','Unauthorized plan revision')
 validation=load(prefix+'validation-v1.json')
 for category in ['Release_compilation','Release_full_selected_focused_unit_tests','Release_ABI_CIL_signatures_exports','Release_frozen_compatibility']:need(validation[category]==DEFERRED,'Release owner deferral misrepresented '+category)
 need(validation['Debug']['outcomes']==dict(Passed=21039,Failed=14),'Managed outcomes differ')
 tests=load(prefix+'managed-Debug-identities-v1.json');prior=load('evidence/04A38/completion/w2/continuation-v15/managed-Debug-identities-v2.json')
 def testkey(row):return row['suite'],row['identity_sha256'],row['occurrence']
 before={testkey(r):r for r in prior['tests']};after={testkey(r):r for r in tests['tests']}
 need(len(before)==21004 and len(after)==21053 and set(before).issubset(after),'Managed identity universe changed')
 for key,row in before.items():
  need(after[key]['outcome']==row['outcome'],'Inherited managed outcome changed')
 need(len(tests['new_tests'])==49 and all(r['outcome']=='Passed' for r in tests['new_tests']),'New managed test outcome differs')
 managed=load(prefix+'managed-validation-v1.json');need(managed['comparison_C15']['all_inherited_outcomes_and_nonpass_messages_unchanged'],'Inherited managed messages changed')
 need(len(tests['nonpasses'])==14 and managed['campaigns']==1,'Wrong managed campaign or nonpass count')
 prior_nonpasses={testkey(r):r for r in prior['nonpasses']}
 need(set(prior_nonpasses)=={testkey(r) for r in tests['nonpasses']},'Inherited managed failure identity lost')
 for r in tests['nonpasses']:need(r['message_sha256']==prior_nonpasses[testkey(r)]['message_sha256'],'Inherited failure message changed')
 python=load(prefix+'python-effective-identities-v1.json');oldpython=load('evidence/04A38/completion/w2/continuation-v15/python-effective-identities-v1.json')
 need(Counter(r['id'] for r in python['entries'])==Counter(r['id'] for r in oldpython['entries']) and len(python['entries'])==956,'Python identity multiset differs')
 need(python['raw_attempt_count']==956 and python['explicit_corrective_attempts']==0 and python['missing']==python['duplicated']==python['skipped']==[],'Python duplicated/interrupted/unexecuted parent')
 need(Counter(r['outcome'] for r in python['entries'])==Counter(PASS=830,FAIL=113,ERROR=13),'Python actual outcomes differ')
 need(python['nonpass_events']==dict(FAIL=183,ERROR=13),'Python parent/subtest accounting differs')
 need(validation['Node_total']==916 and validation['strict_profile_MkDocs']==dict(strict=1,profile=1,docs=0),'Auxiliary actual results differ')
 oldnode=load('evidence/04A38/completion/w2/continuation-v15/node-identities-v1.json')['suites'];node=load(prefix+'node-identities-v1.json')['suites']
 need(node==oldnode,'Node identity population differs')
 compatibility=load(prefix+'compatibility-combined-v3.json');oldcompat=load('evidence/04A38/completion/w2/continuation-v15/compatibility-packet-v3.json')
 previous={r['corpus']:r for r in oldcompat['results'] if r['configuration']=='Debug'}
 need(compatibility['all_exact'] and compatibility['new_original_queries']==0 and len(compatibility['results'])==12,'Frozen Debug evidence incomplete')
 for r in compatibility['results']:
  need(r['configuration']=='Debug' and r['exact'] and r['command']['exit_code']==0,'Unexpected configuration/current comparison failure')
  need(all(r[k]==previous[r['corpus']][k] for k in ['original','inputs','recorder','cases']),'Frozen provenance changed')
  for k in ['original','inputs','recorder','current']:check(r[k])
  need((r['current']['bytes'],r['current']['sha256'])==(r['original']['bytes'],r['original']['sha256']),'Frozen current output differs')
 need(sum(r['cases'] for r in compatibility['results'])==29424 and sum(r['cases'] for r in compatibility['results'] if r['corpus'] in list('ABCDEFG'))==458,'Original/late case population conflated')
 exports=load(prefix+'final-v4/exports-Debug.json')['exports'];original_exports=load('evidence/04A38/preedit/compiled-Debug.json')['exports']
 need(len(exports)==268 and sorted(exports,key=lambda r:r['effective_excel_name'])==sorted(original_exports,key=lambda r:r['effective_excel_name']),'Debug shipping public ABI/defaults/help changed')
 # These independent read-only tools recompute full original/current source,
 # generic/ByRef signatures, compiled ownership, native state and every ledger.
 need(root.resolve()==ROOT.resolve(),'Use the reader from the intended extracted repository root')
 for name in ['build_contracts_v17.py','reconcile_source_v16.py','reconcile_contexts_v16.py','build_ownership_v18.py']:
  runpy.run_path(str(root/(prefix+name)),run_name='__c16_readonly_proof__')
 ledger=load(prefix+'canonical-identity-status-v1.json')['entries']
 need(len(ledger)==1733 and len({(r['population'],r['identity']) for r in ledger})==1733,'Duplicate/missing canonical identity')
 need(all(r['supported']==(r['transition']!='OPEN') and (r['transition']=='RETAINED')==r['prior_supported'] for r in ledger),'Unsupported canonical closure')
 summary=load(prefix+'ownership-summary-v1.json')
 need(summary['historical']==dict(total=704,closed=605,OPEN=99) and summary['current']==dict(total=726,closed=635,OPEN=91) and summary['support']['distinct_closed']==271 and summary['support']['distinct_OPEN']==32,'Wrong canonical complements')
 obligations=load(prefix+'open-obligations-v1.json');need(sum(r['supported'] for r in obligations['selected58'])==57,'Selected starting queue changed')
 need({r['name'] for r in obligations['source_obligations']}=={'NormalizeMixedModelResidualCovarianceName'},'Unresolved source branch omitted')
 need(len(obligations['blocked'])==2,'Unresolved dependency parent not retained OPEN')
 fragments=load(prefix+'new-parent-fragment-crosswalk-v1.json');need(not fragments['unreconciled_branch_inline'],'Unproved branch/inline closure')
 actual=load(prefix+'new-closures-final-v1.json');need((actual['historical_new'],actual['current_new'],actual['distinct_support_new'])==(68,69,9),'Wrong actual closure delta')
 predecessor_result='SKIPPED_ONLY_FOR_EXPLICIT_ADVERSARIAL_CONTROL'
 if predecessor:
  r=subprocess.run([sys.executable,'-I','-B',str(root/(prefix+'replay_prior_v15_v1.py'))],cwd=root,capture_output=True)
  need(r.returncode==0,'Unchanged final v15 predecessor reader failed: '+r.stderr.decode('utf-8','replace')[-1000:])
  predecessor_result=json.loads(r.stdout)['status']
 readiness=load(prefix+'readiness-v1.json');need(readiness['W2']=='IN_PROGRESS' and readiness['W3']=='NOT_STARTED' and readiness['science']=='NOT_GOLDEN' and readiness['phase04A39']=='BLOCKED','Readiness promoted')
 return dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical_supported=605,historical_OPEN=99,current_supported=635,current_OPEN=91,support_supported=271,support_OPEN=32,selected58_closed=57,Debug=dict(PASS=21039,FAIL=14,new_PASS=49),Debug_frozen=29424,exports=268,Python=dict(PASS=830,FAIL=113,ERROR=13),Python_subtests=dict(FAIL=183,ERROR=13),Node_PASS=916,strict_profile_MkDocs=dict(strict=1,profile=1,docs=0),Release_compilation=DEFERRED,Release_full_selected_focused_unit_tests=DEFERRED,Release_ABI_CIL_signatures_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,predecessor=predecessor_result,original_queries=0,phase04A39='BLOCKED')
if __name__=='__main__':
 try:print(json.dumps(compute()),flush=True)
 except (AssertionError,ValueError,OSError,KeyError,TypeError) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))),flush=True);raise SystemExit(1)
