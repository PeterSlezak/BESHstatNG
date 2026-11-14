"""Exact checkpoint accounting; preserve every failed and historical identity."""
import csv,re
from collections import Counter
from capture_current import *
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from baseline_report import index,python_receipt
from validation_report import safe_label

def save(n,v):
 with (HERE/n).open('x',encoding='utf-8') as f:json.dump(v,f,indent=2,ensure_ascii=False)
def main():
 with (ROOT/'evidence/04A38/managed-test-identities-validation-v2.csv').open(encoding='utf-8',newline='') as f:old=list(csv.DictReader(f))
 totals=Counter();identities=[];nonpasses=[];new=[];suites=[]
 for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
  data=json.loads((RUNROOT/'executions'/('w2-validation-v1-Debug-serial-'+suite)/'cases.json').read_bytes());slots=index(data['records']);totals.update(data['outcomes'])
  previous={(r['raw_identity_sha256'],int(r['raw_occurrence_ordinal'])):r for r in old if r['suite']==suite and r['configuration']=='Debug'}
  assert all(k in slots and slots[k]['outcome']==r['outcome'] for k,r in previous.items()),suite
  for k,r in slots.items():
   row=dict(suite=suite,identity_sha256=k[0],occurrence=k[1],class_name=r['class_name'],method=r['method'],test_name=safe_label(r['test_name']),outcome=r['outcome'],population='NEW_W2' if k not in previous else 'INHERITED_CHECKPOINT')
   identities.append(row)
   if k not in previous:new.append(row);assert r['outcome']=='Passed'
   if r['outcome']!='Passed':nonpasses.append(dict(row,message_sha256=sha(r['message'].encode())))
  suites.append(dict(suite=suite,outcomes=data['outcomes'],exit_code=data['command']['exit_code'],trx_sha256=data['sha256']))
 assert totals==Counter(Passed=20583,Failed=14) and len(new)==27
 save('managed-Debug-identities.json',dict(tests=identities,new_tests=new,nonpasses=nonpasses))
 pys=[python_receipt('w2-validation-v1-'+s) for s in ['python-tools','python-help','python-scripts','common-python']]
 reruns=[python_receipt('w2-privacy-artifact-correction-'+s) for s in ['13','14']]
 assert all(r['total']==r['failed']==1 and r['errors']==0 for r in reruns)
 previous=json.loads((ROOT/'evidence/04A38/nonpasses-validation-v2.json').read_bytes())['python'];oldmap={r['identity']:r for group in previous for r in group['nonpasses']}
 now={r['identity']:r for group in pys for r in group['nonpasses']}
 changes=[dict(identity=k,before=oldmap[k]['outcome'],first_full=v['outcome']) for k,v in now.items() if k in oldmap and oldmap[k]['outcome']!=v['outcome']]
 assert len(changes)==2 and all(c['before']=='FAIL' and c['first_full']=='ERROR' for c in changes)
 for r in reruns:
  for n in r['nonpasses']:now[n['identity']]=n
 assert set(now)==set(oldmap) and all(r['outcome']==oldmap[k]['outcome'] for k,r in now.items())
 nodes=[]
 for name in ['office-node','resolver','wasm-node']:
  p=RUNROOT/'commands'/('w2-validation-v1-'+name);data=(p/'stdout.log').read_text(encoding='utf-8-sig')
  count=lambda k:int(re.findall(r'(?:# |\u2139 )'+k+r' (\d+)',data)[-1])
  nodes.append(dict(suite=name,total=count('tests'),passed=count('pass'),failed=count('fail'),exit_code=json.loads((p/'command.json').read_bytes())['exit_code']))
 assert sum(r['passed'] for r in nodes)==916 and all(r['failed']==0 for r in nodes)
 extra=[python_receipt('w2-validation-v1-accepted'+b+'-audits') for b in ['34','35','36','37']]+[python_receipt('w2-validation-v1-accepted37-supplement')]
 for row in extra:
  if row['passed']<0:row['passed']=0;row['qualification']='setUpClass error before any test executed'
 save('nonpasses.json',dict(managed=nonpasses,python_first_full=pys,python_targeted_reruns=reruns,python_first_full_changed_identities=changes,python_reconciled_nonpasses=list(now.values()),historical_packet_audits=extra,history_changed=False))
 selected=json.loads((HERE/'selected-Release.json').read_bytes());release=sum(sum(r['tests']['outcomes'].values()) for r in selected['results']);assert release==49
 adverse=python_receipt('w2-independent-slice-adverse-v3');assert adverse['total']==17 and adverse['exit_code']==0
 serial={n:json.loads((RUNROOT/'commands'/('w2-validation-v1-'+n)/'command.json').read_bytes())['exit_code'] for n in ['strict','profile','docs']}
 save('validation.json',dict(status='EXECUTED_W2_CHECKPOINT_NOT_BATCH_READY',managed_Debug=dict(outcomes=dict(totals),new_passes=27,inherited_nonpasses=14,suites=suites,identity_outcome_delta=0),
  selected_Release=dict(existing_cases_pass=49,new_profile_cases_pass=27,unique_selected_pass=76,full_suite='NOT_RUN_OWNER_PREFERENCE',full_suite_historical_only='20556 PASS/14 inherited FAIL'),
  Python_first_full={k:sum(r[k] for r in pys) for k in ['total','passed','failed','errors']},Python_after_two_targeted_reruns=dict(total=956,passed=767,failed=178,errors=11,old_nonpass_identity_outcome_delta=0),
  artifact_encoding_correction='First full run767 PASS/176 FAIL/13 ERROR retained. Two inherited privacy FAIL identities had become ERROR due unrelated copied legacy-encoded source. Exact-byte preimages relocated privately; each targeted rerun FAIL, no ERROR. No test expectations or source bytes changed.',
  Node=nodes,Node_pass=916,strict_profile_MkDocs=serial,independent_slice_controls=adverse,failed_adverse_attempts=['adverse-fixture-attempt-v1.json','adverse-fixture-attempt-v2.json'],
  compatibility_per_configuration=dict(genuine_preedit=28966,late_A=432,late_B=12,late_C=2,late_D=4,late_E=2,late_F=2,late_G=4),
  wrappers='NOT_RUN: inspected original wrapper implicitly restores/writes shared TestResults; Legacy host Build may package. Isolated offline compile/test equivalents executed.',
  NOT_RUN=['Full managed Release','Native Excel/COM/UIA/RefEdit','Independent R scientific oracle','Office/Mac manual smoke','MSI/signing/install/deploy','W3 whole-batch READY verifier'],
  scientific=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN')))
 commands=[]
 for folder in sorted((RUNROOT/'commands').glob('w2-*')):
  if (folder/'command.json').exists():commands.append(dict(label=folder.name,receipt=clean(json.loads((folder/'command.json').read_bytes()))))
 save('command-receipts.json',dict(commands=commands,raw_logs='Preserved privately; source/statistical/branch bytes not redacted. Public labels/path diagnostics are separately sanitized.',all_attempts='Includes failed source build, recorder selection, path lookup, summary script, initial copy and adverse fixtures; no attempted run silently promoted'))
 print('Debug20583PASS/14 inheritedFAIL; selectedRelease76PASS; Node916PASS; Python767/178/11 after retained targeted reruns; strict/profile/MkDocs1/1/0')
if __name__=='__main__':main()
