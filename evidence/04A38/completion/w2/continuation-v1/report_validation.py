"""Account for every actual test identity; no inherited nonpass is removed."""
from collections import Counter
from common import *
from baseline_report import index,python_receipt
from validation_report import safe_label
def main():
 old=json.loads((W2/'managed-Debug-identities.json').read_bytes())['tests'];totals=Counter();identities=[];nonpasses=[];added=[];suites=[]
 for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
  data=json.loads((RUNROOT/'executions'/('w2c1-validation-v2-Debug-serial-'+suite)/'cases.json').read_bytes());now=index(data['records'])
  previous={(r['identity_sha256'],r['occurrence']):r for r in old if r['suite']==suite}
  assert all(k in now and now[k]['outcome']==r['outcome'] for k,r in previous.items()),'Inherited managed identity/outcome drift '+suite
  totals.update(data['outcomes']);suites.append(dict(suite=suite,outcomes=data['outcomes'],exit_code=data['command']['exit_code'],trx_sha256=data['sha256']))
  for k,r in now.items():
   item=dict(suite=suite,identity_sha256=k[0],occurrence=k[1],class_name=r['class_name'],method=r['method'],test_name=safe_label(r['test_name']),outcome=r['outcome'],population='NEW_CONTINUATION' if k not in previous else 'INHERITED_W2_CHECKPOINT')
   identities.append(item)
   if k not in previous:added.append(item)
   if r['outcome']!='Passed':nonpasses.append(dict(item,message_sha256=sha(r['message'].encode())))
 save('managed-Debug-identities.json',dict(tests=identities,new_tests=added,nonpasses=nonpasses))
 # Historical failed/invalid fixtures remain in separate raw attempts; only final state is summarized here.
 pys=[python_receipt('w2c1-validation-v2-'+s) for s in ['python-tools','python-help','python-scripts','common-python']]
 old_python=json.loads((W2/'nonpasses.json').read_bytes())['python_reconciled_nonpasses'];oldmap={r['identity']:r for r in old_python};nowmap={r['identity']:r for g in pys for r in g['nonpasses']}
 delta=[dict(identity=k,before=oldmap[k]['outcome'] if k in oldmap else 'ABSENT',after=v['outcome']) for k,v in nowmap.items() if k not in oldmap or v['outcome']!=oldmap[k]['outcome']]
 delta += [dict(identity=k,before=v['outcome'],after='ABSENT') for k,v in oldmap.items() if k not in nowmap]
 old_raw=[r for s in ['python-tools','python-help','python-scripts','common-python'] for r in python_receipt('w2-validation-v1-'+s)['nonpasses']]
 for label in ['13','14']:
  for record in python_receipt('w2-privacy-artifact-correction-'+label)['nonpasses']:
   old_raw=[r for r in old_raw if r['identity']!=record['identity']]+[record]
 occurrence=lambda r:(r['outcome'],r['identity'],r['subtest'])
 old_counts=Counter(occurrence(r) for r in old_raw);new_counts=Counter(occurrence(r) for g in pys for r in g['nonpasses'])
 extra=[dict(outcome=k[0],identity=k[1],subtest=k[2],occurrences=n) for k,n in (new_counts-old_counts).items()]
 lost=[dict(outcome=k[0],identity=k[1],subtest=k[2],occurrences=n) for k,n in (old_counts-new_counts).items()]
 occurrence_delta=dict(extra=extra,lost=lost,classification='The added PlotDataUDFs source-pin subtest is an authorized-source-transition assertion, not a waived PASS. Reconstruct old repeated subtests from immutable raw logs, not the old deduplicated dictionary.')
 nodes=[]
 import re
 for name in ['office-node','resolver','wasm-node']:
  p=RUNROOT/'commands'/('w2c1-validation-v2-'+name);text=(p/'stdout.log').read_text(encoding='utf-8-sig');count=lambda key:int(re.findall(r'(?:# |\u2139 )'+key+r' (\d+)',text)[-1])
  nodes.append(dict(suite=name,total=count('tests'),passed=count('pass'),failed=count('fail'),exit_code=json.loads((p/'command.json').read_bytes())['exit_code']))
 selected=json.loads((HERE/'selected-Release.json').read_bytes());focus=json.loads((HERE/'focused-p1-v5-Release.json').read_bytes());release={}
 for suite,data in [(r['suite'],r['tests']) for r in selected['results']]+[(r['suite'],r['tests']) for r in focus['results'] if r['kind']=='TEST']:
  for k,r in index(data['records']).items():release[(suite,*k)]=r['outcome']
 serial={n:json.loads((RUNROOT/'commands'/('w2c1-validation-v2-'+n)/'command.json').read_bytes())['exit_code'] for n in ['strict','profile','docs']}
 early=[]
 labels=['early-reader-preedit','early-reader-checkpoint','early-reader-continuation','early-reader-A-longpath-v2','early-reader-B','early-reader-C','early-reader-D-longpath-v2','early-reader-E-longpath-v2','early-reader-F-longpath-v2','early-reader-G-longpath-v2','early-reader-W1-longpath-v2','predecessor-old-W2-bodies','predecessor-old-W2-publication']
 for label in labels:
  if (RUNROOT/'commands'/('w2c1-'+label)/'command.json').is_file():early.append(dict(label=label,receipt=python_receipt('w2c1-'+label)))
 save('nonpasses.json',dict(managed=nonpasses,python=pys,python_inherited_identity_delta=delta,python_inherited_occurrence_delta=occurrence_delta,historical_reader_replays=early,investigative_current_text_policy=json.loads((HERE/'options-policy-diagnostic.json').read_bytes()),invalid_fixtures='Initial shipping nonfinite/calibration assumptions and P1-v4 actual calibration regression retained. Reader-v1/v2/v3 setup bugs and adverse-v1 missing-preimage check retained. No inherited identity edited.'))
 validation=dict(status='EXECUTED_PARTIAL_W2_NOT_BATCH_READY',managed_Debug=dict(outcomes=dict(totals),new_passes=len(added),new_nonpasses=[r for r in added if r['outcome']!='Passed'],inherited_nonpasses=len(nonpasses),suites=suites,old_identity_outcome_delta=0),selected_Release=dict(outcomes=dict(Counter(release.values())),distinct_identities=len(release),full_suite='NOT_RUN_OWNER_PREFERENCE'),Node=nodes,Python={k:sum(r[k] for r in pys) for k in ['total','passed','failed','errors']},Python_inherited_delta=delta,strict_profile_MkDocs=serial,current_frozen_comparison_per_configuration=dict(preedit=28966,late_A_to_G=458,total=29424,source_categories_separate=True),new_original_queries=0,investigative_policy_probe='FAIL:8 null-versus-ignorable differences/1352 runtime pairs/config;0 nonnull differences; production reachability/normalization remains OPEN; not a new original reference',historical_reader_replays=early,wrappers='NOT_RUN: implicit restore/shared outputs/Legacy packaging; safe isolated offline equivalents executed',NOT_RUN=['Full managed Release','W3 whole-batch READY verifier','Native Excel/COM/UIA/RefEdit','Independent R/scientific oracle','Office/Mac/manual GUI smoke','Package restore/install/MSI/signing/deployment','Phase04A39'],gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN'))
 validation['Python_occurrence_delta']=occurrence_delta
 save('validation.json',validation);print(json.dumps({k:validation[k] for k in ['managed_Debug','selected_Release','Node','Python','strict_profile_MkDocs']},ensure_ascii=False))
if __name__=='__main__':main()
