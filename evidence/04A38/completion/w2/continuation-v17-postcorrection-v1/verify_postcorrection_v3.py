"""Deferral-aware post-correction reader. Prior reader and all its assertions remain immutable."""
from common import *
from collections import Counter
load=lambda p:json.loads(Path(p).read_bytes())
def exact_bytes(row,data):
 assert data is not None and len(data)==row['bytes'] and sha(data)==row['sha256'],'MISSING_OR_MODIFIED_BYTES '+row['path']
def canonical(d):
 expected=load(PREVIOUS/'canonical-identity-status-v4.json');rows=d['entries']
 assert Counter(r['population'] for r in rows)==dict(historical=704,current=726,support=303)
 keys=lambda rows:Counter((r['population'],r['identity']) for r in rows)
 assert keys(rows)==keys(expected['entries']) and all(r['supported'] and r['transition']!='OPEN' for r in rows)
 bound={(r['population'],r['identity']):r for r in expected['entries']}
 for row in rows:
  assert row.get('proof')==bound[row['population'],row['identity']].get('proof'),'Preserved entire-body proof removed or changed'
  assert row.get('proof') is not None or (row['population']=='current' and row['transition']=='RETAINED')
 return dict(historical=704,current=726,support=303,OPEN=0)
def frozen(d):
 prior=load(PREVIOUS/'compatibility-combined-v7.json')
 old={(r['configuration'],r['corpus']):r for r in prior['results']}
 assert d['totals_per_configuration']=={'Debug':29424} and d['all_exact'] and len(d['results'])==12
 late=0;pre=0
 for row in d['results']:
  assert row['configuration']=='Debug' and row['exact'] and row['original_queries']==0
  prior=old[row['configuration'],row['corpus']]
  for k in ['original','inputs','recorder']:assert row[k]==prior[k] and pin(ROOT/row[k]['path'])==row[k]
  assert row['cases']==prior['cases'];assert pin(ROOT/row['current']['path'])==row['current']
  assert (ROOT/row['current']['path']).read_bytes()==(ROOT/row['original']['path']).read_bytes()
  if row['corpus'] in list('ABCDEFG'):late+=row['cases']
  else:pre+=row['cases']
 assert (pre,late)==(28966,458)
 return dict(PRE_EDIT_FROZEN=pre,LATE_ARCHIVED_ENTRY_SUPPLEMENT=late,original_queries=0)
def compute():
 for r in load(HERE/'production-test-freeze-v1.json')['sources']:assert pin(ROOT/r['path'])==r
 assert load(HERE/'entry-final-reader-authentication-v1.json')['all_exit_zero']
 own=canonical(load(HERE/'canonical-identity-status-v1.json'))
 comp=frozen(load(HERE/'compatibility-replacement-v1.json'))
 for name in ['whole-body-dependency-fragment-reconciliation-v1.json','managed-validation-v1.json','validation-v1.json','python-frozen-input-postcheck-v1.json']:
  assert (HERE/name).is_file()
 managed=load(HERE/'managed-Debug-identities-v1.json');assert not managed['missing']
 py=load(HERE/'python-effective-identities-v1.json');assert len(py['entries'])==956 and not py['missing'] and not py['duplicated'] and not py['skipped']
 for r in load(HERE/'whole-body-dependency-fragment-reconciliation-v1.json')['proofs']:assert pin(ROOT/r['path'])==r
 for r in load(HERE/'review-seal-v1.json')['files']:assert pin(ROOT/r['path'])==r
 assert git('branch','--show-current').decode().strip()=='codex/04A-38'
 for args in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')]:assert git(*args).decode().strip()==BASE
 assert not git('diff','--cached','--name-only') and not git('diff','--check')
 assert pin(ROOT/'.git/index')['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
 return dict(status='POST_CORRECTION_FROZEN_SOURCE_TEST_IDENTITIES_COMPATIBILITY_OWNERSHIP_AND_GIT_READBACK_PASS',ownership=own,compatibility=comp,managed=load(HERE/'managed-validation-v1.json')['Debug'],Python=load(HERE/'validation-v1.json')['Python'],Release_compilation=DEFERRED,Release_full_selected_focused_unit_tests=DEFERRED,Release_ABI_CIL_signatures_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,W2_acceptance='NOT_GRANTED',W3='NOT_STARTED',original_queries=0)
if __name__=='__main__':print(json.dumps(compute()),flush=True)
