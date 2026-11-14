"""Reconcile the single real v16 campaign, every identity/event and deferred Release gate."""
from common import *
from collections import Counter,defaultdict
_save_new=save
def save(name,value):
 path=HERE/name
 if path.exists():assert json.loads(path.read_bytes())==value, 'Existing append-only validation differs '+name
 else:_save_new(name,value)

import re
load=lambda p:json.loads(Path(p).read_bytes())
inventory=load(HERE/'python-discovery-inventory-v1.json');expected=Counter(r['id'] for r in inventory['entries'])
receipts=load(HERE/'python-worker-receipts-v1.json');records=[];executions=[];reports=[]
for receipt in receipts['receipts']:
 assert receipt['raw'] and pin(ROOT/receipt['raw']['path'])==receipt['raw']
 report=load(ROOT/receipt['raw']['path']);reports.append(report)
 assert report['exit_code']==receipt['command']['exit_code']
 assert report['testsRun']==len(report['records'])==sum(report['outcomes'].values())
 folder=RUNROOT/'commands'/('w2c17-python-worker-v1-'+str(receipt['lane'])+'-'+receipt['phase'].lower())
 stderr=(folder/'stderr.log').read_bytes();matches=re.findall(rb'Ran (\d+) tests?',stderr)
 assert matches and int(matches[-1])==report['testsRun']
 journal=ROOT/report['parent_journal']['path'];assert pin(journal)==report['parent_journal']
 entries=[json.loads(line) for line in journal.read_text(encoding='utf-8').splitlines()]
 for kind in ['START','COMPLETE']:
  assert Counter(r['value']['id'] for r in entries if r['kind']==kind)==Counter(r['id'] for r in report['records'])
 assert [r['value'] for r in entries if r['kind']=='COMPLETE']==report['records']
 executions.append(dict(lane=report['lane'],phase=receipt['phase'],methods=report['testsRun'],method_outcomes=report['outcomes'],failure_events=report['unittest_failures'],error_events=report['unittest_errors'],seconds=report['seconds'],exit_code=report['exit_code'],pid=report['pid'],start_utc=report['start_utc'],end_utc=report['end_utc'],raw=receipt['raw'],stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),command=pin(folder/'command.json'),journal=report['parent_journal'],copy_provenance=receipt.get('copy_provenance')))
 records.extend(dict(r,lane=report['lane'],phase=receipt['phase']) for r in report['records'])
assert Counter(r['id'] for r in records)==expected and len(records)==len(expected)==956
events=[];occurrences=Counter()
for r in records:
 assert r['occurrence']==0
 for event in r['events']:
  assert event['outcome'] in ['FAIL','ERROR'] and sha(event['traceback'].encode())==event['traceback_sha256']
  key=(r['id'],event.get('subtest'),event['outcome']);occurrence=occurrences[key];occurrences[key]+=1
  events.append(dict(event,identity=r['id'],occurrence=occurrence,lane=r['lane'],phase=r['phase']))
parents=Counter(r['outcome'] for r in records);counts=Counter(r['outcome'] for r in events)
prior_ids=load(PREVIOUS/'python-effective-identities-v1.json')['entries'];prior_parents={r['id']:r['outcome'] for r in prior_ids}
assert Counter(prior_parents.keys())==expected
parent_delta=[dict(identity=r['id'],before=prior_parents[r['id']],after=r['outcome']) for r in records if prior_parents[r['id']]!=r['outcome']]
prior_events=load(PREVIOUS/'python-first-failures-v1.json')['records'];oldpin=pin(PREVIOUS/'python-first-failures-v1.json')
def key(r):
 sub=r.get('subtest');suffix=clean(sub[len(r['identity']):].strip()) if sub is not None else ''
 return r['identity'],r['outcome'],suffix,r['occurrence']
oldkeys=Counter(map(key,prior_events));newkeys=Counter(map(key,events))
delta=dict(extra=[dict(identity=k[0],outcome=k[1],subtest=k[2],occurrence=k[3],count=v) for k,v in (newkeys-oldkeys).items()],lost=[dict(identity=k[0],outcome=k[1],subtest=k[2],occurrence=k[3],count=v) for k,v in (oldkeys-newkeys).items()],parent_outcome_changes=parent_delta)
oldbykey={key(r):r for r in prior_events};details=[];changed_messages=[]
for r in events:
 prior=oldbykey.get(key(r));digest=sha(r.get('message','').encode())
 if prior:
  previous_digest=prior.get('raw_message_sha256',prior.get('message_sha256',sha(prior.get('message','').encode())))
  previous=dict(source=oldpin,event_key=list(key(prior)),message_sha256=previous_digest,traceback_sha256=prior['traceback_sha256'])
  if previous_digest!=digest:changed_messages.append(dict(identity=r['identity'],event_key=list(key(r)),previous=previous,current_raw_message_sha256=digest,current_raw_traceback_sha256=r['traceback_sha256'],current_message=clean(r.get('message','')),classification='CHANGED_RAW_MESSAGE_RETAINED_NOT_WAIVED'))
 else:previous=None
 classification='INHERITED_NONPASS_IDENTITY_OUTCOME_OCCURRENCE_NOT_WAIVED' if prior else 'NEW_NONPASS_EVENT_REQUIRES_RECORDED_INVESTIGATION'
 if r['identity'].startswith('test_04a22_spc.') and r['outcome']=='ERROR':classification='UNORDERED_SPC_ERROR_SEPARATE_IDENTITY_NOT_RELABELLED'
 if 'UnicodeDecodeError' in r['traceback']:classification='ACTUAL_UNICODE_DECODE_ERROR;C15_FOCUSED_AND_OLDER_JSON_OR_ASSERTION_ATTEMPTS_SEPARATELY_RETAINED'
 details.append(dict(clean(r),raw_message_sha256=digest,classification=classification,previous=previous))
save('python-first-failures-v1.json',dict(records=details,events=len(details),method_nonpasses=sum(r['outcome']!='PASS' for r in records),raw_tracebacks_retained=True,previous=oldpin))
save('python-nonpass-delta-v1.json',dict(**delta,changed_messages=changed_messages,previous_historical_attempts=['continuation-v15/python-interruption-recovery-v1.json','continuation-v15/owner-replacement-python-lane-authorization-v1.json','continuation-v15/owner-focused-privacy-authorization-v1.json','continuation-v15/python-first-failures-v1.json'],scope='Every old failed/interrupted/error attempt remains immutable. Current errors are not PASS and earlier AssertionError/JSON/Unicode chronology is not collapsed.'))
save('python-effective-identities-v1.json',dict(entries=[dict(id=r['id'],outcome=r['outcome'],lane=r['lane'],phase=r['phase'],occurrence=0,attempt=1) for r in records],method_outcomes=dict(parents),nonpass_events=dict(counts),missing=[],duplicated=[],skipped=[],raw_attempt_count=956,complete_current_records=956,explicit_corrective_attempts=0))
scanner=next(r for r in reports if r['lane']==1)
save('python-worker-accounting-v1.json',dict(workers_at_peak=5,executions=executions,raw_attempts=956,corrective_repeats=0,effective_complete_records=956,effective_method_outcomes=dict(parents),effective_nonpass_events=dict(counts),nonpass_event_delta=delta,scanner=scanner['scanner'],retained_scanner=scanner['retained_scanner'],tools=[r['tools'] for r in reports if 'tools' in r],schedule=receipts['schedule'],accounting='Exactly956 parent START and COMPLETE journal entries, seven disjoint execution parts across six deterministic logical lanes; lane5 has a disjoint four-parent relocation copy. Five concurrent non-scanner processes, then scanner alone, zero scanner helpers, zero corrective repetitions.'))
nodes=[];node_ids=[];old_nodes={r['suite']:r for r in load(PREVIOUS/'node-identities-v1.json')['suites']}
for suffix in ['office-node','resolver','wasm-node']:
 folder=RUNROOT/'commands'/('w2c17-checkpoint-'+suffix);text=(folder/'stdout.log').read_bytes().decode('utf-8-sig')
 names=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)\r?$',text,re.M|re.S);hashes=[sha(v.encode()) for v in names]
 assert Counter(hashes)==Counter(old_nodes[suffix]['identities'])
 count=lambda label:int(re.findall(r'(?:# |\u2139 )'+label+r' (\d+)',text)[-1])
 record=load(folder/'command.json');assert len(hashes)==count('tests')==count('pass') and count('fail')==0 and record['exit_code']==0
 node_ids.append(dict(suite=suffix,identities=hashes));nodes.append(dict(suite=suffix,total=len(hashes),passed=len(hashes),failed=0,command=clean(record),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log')))
assert sum(r['passed'] for r in nodes)==916
save('node-identities-v1.json',dict(suites=node_ids,complete_including_embedded_newline=True,old_identity_multiset_delta=0))
serial={k:load(RUNROOT/'commands'/('w2c17-checkpoint-v1-'+k)/'command.json')['exit_code'] for k in ['strict','profile','docs']}
save('validation-v1.json',dict(Debug=load(HERE/'managed-validation-v1.json')['Debug'],Release_compilation=DEFERRED,Release_full_selected_focused_unit_tests=DEFERRED,Release_ABI_CIL_signatures_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,Debug_frozen_cases=29424,Python=dict(methods=dict(parents),nonpass_events=dict(counts),workers_at_peak=5,executed=956,delta=delta),Node=nodes,Node_total=916,strict_profile_MkDocs=serial,original_queries=0,native='NOT_RUN',science='NOT_GOLDEN'))
save('nonpasses-v1.json',dict(managed=load(HERE/'managed-Debug-identities-v1.json')['nonpasses'],python=details,auxiliary=serial,first_attempts='All prior and current failed/interrupted attempts, compilation and evidence-tool failures retained. No historical assertion changed.'))
print(json.dumps(dict(Python_methods=dict(parents),Python_events=dict(counts),delta=delta,changed_messages=len(changed_messages),Node=916,strict_profile_MkDocs=serial)),flush=True)
