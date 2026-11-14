"""Reconcile every raw parent/event occurrence; preserve all first failures."""
from common import *
from collections import Counter,defaultdict
import re
inventory=json.loads((HERE/'python-discovery-inventory-v1.json').read_bytes());expected=Counter(r['id'] for r in inventory['entries'])
receipts=json.loads((HERE/'python-worker-receipts-v1.json').read_bytes());records=[];reports=[];executions=[]
for receipt in receipts['receipts']:
    assert receipt['raw'] is not None and pin(ROOT/receipt['raw']['path'])==receipt['raw']
    report=json.loads((ROOT/receipt['raw']['path']).read_bytes());reports.append(report)
    assert report['exit_code']==receipt['command']['exit_code'] and report['testsRun']==sum(report['outcomes'].values())
    command_folder=ROOT/receipt['command_folder']
    stderr=(command_folder/'stderr.log').read_bytes();matches=re.findall(rb'Ran (\d+) tests?',stderr);assert matches and int(matches[-1])==report['testsRun']
    effective_ids=set(receipt.get('effective_ids',[r['id'] for r in report['records']]))
    effective=[r for r in report['records'] if r['id'] in effective_ids];assert len(effective)==len(effective_ids)
    executions.append(dict(lane=report['lane'],phase=receipt['phase'],methods=len(effective),raw_methods=report['testsRun'],method_outcomes=dict(Counter(r['outcome'] for r in effective)),raw_method_outcomes=report['outcomes'],failure_events=report['unittest_failures'],error_events=report['unittest_errors'],seconds=report['seconds'],exit_code=report['exit_code'],pid=report['pid'],start_utc=report['start_utc'],end_utc=report['end_utc'],raw=receipt['raw'],stdout=pin(command_folder/'stdout.log'),stderr=pin(command_folder/'stderr.log'),command=pin(command_folder/'command.json'),copy_provenance=receipt.get('copy_provenance')))
    records.extend(dict(row,lane=report['lane'],phase=receipt['phase']) for row in effective)
assert Counter(r['id'] for r in records)==expected and len(records)==len(expected)==956
events=[];occurrences=Counter()
for record in records:
    for event in record['events']:
        assert event['outcome'] in ['FAIL','ERROR'] and sha(event['traceback'].encode())==event['traceback_sha256']
        key=record['id'],event.get('subtest'),event['outcome'];occurrence=occurrences[key];occurrences[key]+=1
        events.append(dict(event,identity=record['id'],occurrence=occurrence,lane=record['lane'],phase=record['phase']))
parents=Counter(r['outcome'] for r in records);counts=Counter(r['outcome'] for r in events)
old=json.loads((PREVIOUS/'python-first-failures-v2.json').read_bytes())['records']
def key(r):
    sub=r.get('subtest');suffix=clean(sub[len(r['identity']):].strip()) if sub is not None else ''
    return r['identity'],r['outcome'],suffix,r['occurrence']
newkeys=Counter(map(key,events));oldkeys=Counter(map(key,old))
delta=dict(extra=[dict(identity=k[0],outcome=k[1],subtest=k[2],occurrence=k[3],count=v) for k,v in (newkeys-oldkeys).items()],lost=[dict(identity=k[0],outcome=k[1],subtest=k[2],occurrence=k[3],count=v) for k,v in (oldkeys-newkeys).items()])
save('python-nonpass-delta-v2.json',delta)
previous_failure_pin=pin(PREVIOUS/'python-first-failures-v2.json')
prior_by_id=defaultdict(list)
for row in old:prior_by_id[row['identity']].append(row)
details=[]
source_pin='test_04a27_transport_successor.Transport27SuccessorTests.test_original_member_ids_and_body_pins_not_fabricated';spc='test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact'
for row in events:
    classification='INHERITED_C14_NONPASS_EVENT_IDENTITY_OUTCOME_OCCURRENCE_NOT_WAIVED' if key(row) in oldkeys else 'NEW_C15_NONPASS_REQUIRES_EXPLICIT_INVESTIGATION'
    if row['identity']==source_pin:classification='RETAINED_C6_INTRODUCED_04A27_SOURCE_PIN_FAIL_NOT_INHERITED_FROM_V5_NOT_WAIVED'
    if row['identity']==spc:classification='RETAINED_UNORDERED_SPC_ERROR_DISTINCT_IDENTITY_FIRST_KEY_VARIANT_NOT_RELABELLED'
    details.append(dict(clean(row),raw_message_sha256=sha(row.get('message','').encode()),classification=classification,previous_first_failure_links=[dict(source=previous_failure_pin,event_key=list(key(p)),message_sha256=p.get('message_sha256',sha(p.get('message','').encode())),traceback_sha256=p['traceback_sha256']) for p in prior_by_id[row['identity']] if key(p)==key(row)]))
save('python-first-failures-v2.json',dict(records=details,events=len(details),method_nonpasses=sum(r['outcome']!='PASS' for r in records),raw_tracebacks_retained=True,scope='Raw exact traceback/message/first-frame hashes retained per worker. Public copies clean machine paths only; old and current outcomes remain distinct.'))
recovery=json.loads((HERE/'python-interruption-recovery-v1.json').read_bytes());started=set(recovery['partial_started_ids']);focused=set(recovery['focused_ids'])
save('python-effective-identities-v1.json',dict(entries=[dict(id=r['id'],outcome=r['outcome'],lane=r['lane'],phase=r['phase'],occurrence=r['occurrence'],attempt=int(r['id'] in started)+int(r['id'] in focused),replacement_lane=r['lane']==1) for r in records],method_outcomes=dict(parents),nonpass_events=dict(counts),missing=[],duplicated=[],skipped=[],raw_attempt_count=972,complete_current_records=956,explicit_corrective_attempts=16,owner_authorized_replacement_lanes=1,owner_focused_privacy_parents=2,authority=pin(HERE/'owner-replacement-python-lane-authorization-v1.json'),focused_authority=pin(HERE/'owner-focused-privacy-authorization-v1.json'),interrupted=pin(HERE/'python-interruption-recovery-v1.json')))
scanner=next(r for r in reports if r['lane']==1)
save('python-worker-accounting-v1.json',dict(workers_at_peak=6,executions=executions,raw_attempts=972,corrective_repeats=16,effective_complete_records=956,owner_authorized_replacement_lanes=1,owner_focused_privacy_parents=2,replacement_authority=pin(HERE/'owner-replacement-python-lane-authorization-v1.json'),focused_authority=pin(HERE/'owner-focused-privacy-authorization-v1.json'),interrupted=pin(HERE/'python-interruption-recovery-v1.json'),effective_method_outcomes=dict(parents),effective_nonpass_events=dict(counts),nonpass_event_delta=delta,scanner=scanner.get('scanner'),parallel_scanner=scanner.get('parallel_scanner'),focused_scanner=[r.get('parallel_scanner') for r in reports if r['testsRun']==2],tools=[r['tools'] for r in reports if 'tools' in r],schedule=receipts['schedule'],historical_C6_mixed_arithmetic=dict(total=956,passed=764,failed_events=181,error_events=11),C14_actual_parent_baseline=dict(PASS=830,FAIL=113,ERROR=13),accounting='972 known parent starts:916 complete originals,14 interrupted-lane starts,40 owner-approved replacement parents,2 separately approved focused privacy corrections.971 completed outcomes include13 partial headers; one interrupted outcome remains unknown.956 unique effective records. The2 accelerator AssertionError FAILs remain raw diagnostics; focused unchanged original scans report actual Unicode ERRORs. Prior JSON ERROR chronology remains immutable. No original assertion changed.'))
node_ids=[];nodes=[];old_nodes={r['suite']:r for r in json.loads((PREVIOUS/'node-identities-v1.json').read_bytes())['suites']}
for suffix in ['office-node','resolver','wasm-node']:
    folder=RUNROOT/'commands'/('w2c15-checkpoint-'+suffix);text=(folder/'stdout.log').read_bytes().decode('utf-8-sig')
    names=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)\r?$',text,re.M|re.S);hashes=[sha(v.encode()) for v in names]
    assert Counter(hashes)==Counter(old_nodes[suffix]['identities'])
    count=lambda label:int(re.findall(r'(?:# |\u2139 )'+label+r' (\d+)',text)[-1])
    command_record=json.loads((folder/'command.json').read_bytes());assert len(hashes)==count('tests')==count('pass') and count('fail')==0 and command_record['exit_code']==0
    node_ids.append(dict(suite=suffix,identities=hashes));nodes.append(dict(suite=suffix,total=len(hashes),passed=len(hashes),failed=0,command=clean(command_record),stdout=pin(folder/'stdout.log')))
assert sum(r['passed'] for r in nodes)==916
save('node-identities-v1.json',dict(suites=node_ids,complete_including_embedded_newline=True,old_identity_multiset_delta=0))
serial={key:json.loads((RUNROOT/'commands'/('w2c15-checkpoint-v1-'+key)/'command.json').read_bytes())['exit_code'] for key in ['strict','profile','docs']}
save('validation-v1.json',dict(Debug=json.loads((HERE/'managed-validation-v2.json').read_bytes())['Debug'],Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',Release_compile='PASS_COMPILE_ONLY',Python=dict(methods=dict(parents),nonpass_events=dict(counts),workers_at_peak=6,executed=956,delta=delta),Node=nodes,Node_total=916,strict_profile_MkDocs=serial,original_queries=0,native='NOT_RUN',science='NOT_GOLDEN'))
save('nonpasses-v1.json',dict(managed=json.loads((HERE/'managed-Debug-identities-v2.json').read_bytes())['nonpasses'],python=details,auxiliary=serial,first_attempts='All focused/new-test, reader/tooling and older C7 first attempts retained; no historical assertion changed.'))
print(json.dumps(dict(Python_methods=dict(parents),Python_events=dict(counts),delta=delta,Node=916,strict_profile_MkDocs=serial)),flush=True)
save('python-investigation-required-v1.json',dict(status='PENDING_EXACT_NONPASS_INVESTIGATION',delta=delta,scope='Aggregation is not test acceptance. Any new/lost event or changed message requires the separate source/trace investigation before sealing; no assertion or historical failure is waived.'))
