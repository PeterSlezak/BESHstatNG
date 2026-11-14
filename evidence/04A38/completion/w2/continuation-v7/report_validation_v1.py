"""Raw attempts and corrected effective outcomes; distinguish methods/subtests."""
from common import *
from collections import Counter,defaultdict
import re
inventory=json.loads((HERE/'python-discovery-inventory-v1.json').read_bytes());expected=Counter(r['id'] for r in inventory['entries'])
receipts=json.loads((HERE/'python-worker-receipts-v1.json').read_bytes());reports=[];records=[];attempts=[]
for receipt in receipts['receipts']:
    assert receipt['raw'] is not None,'Dropped worker'
    result=json.loads((ROOT/receipt['raw']['path']).read_bytes());reports.append(result)
    assert result['exit_code']==receipt['command']['exit_code']
    records.extend(dict(r,lane=result['lane']) for r in result['records'])
    attempts.extend(dict(r,lane=result['lane'],attempt=0) for r in result['records'])
assert Counter(r['id'] for r in records)==expected and len(records)==956
correction_folder=PRIVATE/'python-correction-v1'
correction=json.loads((correction_folder/'result.json').read_bytes());provenance=json.loads((correction_folder/'copy-provenance.json').read_bytes())
assert correction['testsRun']==4 and correction['exit_code']==0 and all(r['outcome']=='PASS' for r in correction['records'])
for row in provenance['source_files']:
    assert (len((ROOT/row['path']).read_bytes()),sha((ROOT/row['path']).read_bytes()))==(row['bytes'],row['sha256'])
by_id={r['id']:r for r in records};corrected=[]
for row in correction['records']:
    first=by_id[row['id']]
    assert first['outcome']=='ERROR' and len(first['events'])==1 and 'C7 worker refused write outside disposable lane:' in first['events'][0]['message']
    corrected.append(dict(identity=row['id'],first=first,corrected=row,classification='INITIAL_WRITE_GUARD_ERROR_CORRECTED_BY_UNCHANGED_SERIAL_DISPOSABLE_COPY'))
    by_id[row['id']]=dict(row,lane='SERIAL_DISPOSABLE_COPY',first_lane=5,attempt=1)
    attempts.append(dict(row,lane='SERIAL_DISPOSABLE_COPY',attempt=1))
effective=[by_id[r['id']] for r in records];assert Counter(r['id'] for r in effective)==expected
method_outcomes=Counter(r['outcome'] for r in effective);events=[];counts=Counter()
for record in effective:
    for event in record['events']:
        key=record['id'],event.get('subtest'),event['outcome'];occurrence=counts[key];counts[key]+=1
        events.append(dict(event,identity=record['id'],occurrence=occurrence,lane=record['lane']))
event_outcomes=Counter(e['outcome'] for e in events)
baseline=json.loads((PREVIOUS/'nonpasses-v2.json').read_bytes())['python']
def display_id(identity):return identity.split('(',1)[1].split(')',1)[0]
old_events=[]
for group in baseline:
    for row in group['nonpasses']:old_events.append((display_id(row['identity']),row['outcome'],row['subtest']))
def subtest_suffix(event):
    value=event.get('subtest')
    if value is None:return ''
    assert value.startswith(event['identity'])
    return clean(value[len(event['identity']):].strip())
new_events=[(r['identity'],r['outcome'],subtest_suffix(r)) for r in events]
delta=dict(extra=[dict(identity=k[0],outcome=k[1],subtest=k[2],occurrences=v) for k,v in (Counter(new_events)-Counter(old_events)).items()],lost=[dict(identity=k[0],outcome=k[1],subtest=k[2],occurrences=v) for k,v in (Counter(old_events)-Counter(new_events)).items()])
assert not delta['extra'] and not delta['lost'],('New/changed Python nonpass identity requires investigation',delta)
assert event_outcomes==Counter(FAIL=181,ERROR=11)
assert method_outcomes==Counter(PASS=830,FAIL=115,ERROR=11)
details=[]
source_pin='test_04a27_transport_successor.Transport27SuccessorTests.test_original_member_ids_and_body_pins_not_fabricated'
spc='test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact'
old_details=json.loads((PREVIOUS/'python-first-failures-v2.json').read_bytes())['records']
old_by_parent=defaultdict(list)
for row in old_details:old_by_parent[display_id(row['identity'])].append(row)
for row in events:
    classification='INHERITED_C6_NONPASS_EVENT_IDENTITY_OUTCOME_OCCURRENCE_NOT_WAIVED'
    if row['identity']==source_pin:classification='RETAINED_C6_INTRODUCED_04A27_SOURCE_PIN_FAIL_NOT_INHERITED_FROM_V5_NOT_WAIVED'
    if row['identity']==spc:classification='RETAINED_UNORDERED_SPC_ERROR_DISTINCT_IDENTITY_FIRST_KEY_VARIANT_NOT_RELABELLED'
    details.append(dict(clean(row),classification=classification,raw_message_sha256=sha(row.get('message','').encode()),previous_first_failures=old_by_parent[row['identity']]))
save('python-environment-corrections-v1.json',dict(corrections=clean(corrected),raw_result=pin(correction_folder/'result.json'),raw_stdout=pin(correction_folder/'stdout.log'),raw_stderr=pin(correction_folder/'stderr.log'),copy_provenance=pin(correction_folder/'copy-provenance.json'),copied_runner=pin(correction_folder/'correct_python_shared_writer_v1.py'),assertions_changed=0,source_files_changed=0,scope='Exactly four original tests run serially in byte-identical disposable reader/test/fixture copy; no authoritative evidence scratch path was written. Initial errors remain raw attempts.'))
save('python-first-failures-v1.json',dict(records=details,events=192,method_nonpasses=126,raw_tracebacks_retained=True,block_hash_convention='Exact unittest formatted traceback strings in per-worker raw-result.json; exact raw stderr streams separately hashed. Public derivative cleans machine paths only; original trace/message hashes and first frames retained.'))
save('python-effective-identities-v1.json',dict(entries=[dict(id=r['id'],outcome=r['outcome'],lane=r['lane'],occurrence=r['occurrence'],attempt=r.get('attempt',0)) for r in effective],method_outcomes=dict(method_outcomes),nonpass_events=dict(event_outcomes),expected_unique_methods=956,missing=[],duplicated=[],skipped=[],raw_attempt_count=960,explicit_corrective_attempts=4))
save('python-worker-accounting-v1.json',dict(workers_at_peak=6,initial_worker_processes=6,additional_serial_corrective_processes=1,initial=[dict(lane=r['lane'],methods=r['testsRun'],method_outcomes=r['outcomes'],failure_events=r['unittest_failures'],error_events=r['unittest_errors'],seconds=r['seconds'],exit_code=r['exit_code']) for r in reports],effective_method_outcomes=dict(method_outcomes),effective_nonpass_events=dict(event_outcomes),receipt_compatible_arithmetic=dict(total=956,passed=764,failed=181,errors=11),accounting='V6 reports956 parent unittest methods, but subtracts181 failure events (including failed subtests) and11 error events to derive764. C7 preserves those raw192 nonpass events and that historical arithmetic, while separately reporting actual830 passing/115 failing/11 error parent methods.66 additional failure events are failed subtests within already failing methods; none are relabelled PASS.',nonpass_event_delta=delta,actual_executed_methods=956,initial_raw_attempts=956,corrective_raw_attempts=4,writer_guard_errors_retained=4,scanner=[r['scanner'] for r in reports if 'scanner' in r],tools=[r['tools'] for r in reports if 'tools' in r]))

nodes=[];node_ids=[];old_nodes={r['suite']:r for r in json.loads((PREVIOUS/'node-identities-v1.json').read_bytes())['suites']}
for suffix in ['office-node','resolver','wasm-node']:
    folder=RUNROOT/'commands'/('w2c7-checkpoint-'+suffix);raw=(folder/'stdout.log').read_bytes();text=raw.decode('utf-8-sig')
    full=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)\r?$',text,re.M|re.S);hashes=[sha(value.encode()) for value in full]
    assert Counter(hashes)==Counter(old_nodes[suffix]['identities'])
    count=lambda key:int(re.findall(r'(?:# |\u2139 )'+key+r' (\d+)',text)[-1])
    row=dict(suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),exit_code=json.loads((folder/'command.json').read_bytes())['exit_code'],raw_stdout=pin(folder/'stdout.log'),identity_multiset_sha256=sha(json.dumps(sorted(hashes)).encode()))
    assert len(hashes)==row['total']==row['passed'] and row['exit_code']==0
    nodes.append(row);node_ids.append(dict(suite=suffix,identities=hashes))
assert sum(r['passed'] for r in nodes)==916
save('node-identities-v1.json',dict(suites=node_ids,complete_including_embedded_newline=True,old_identity_multiset_delta=0))
serial={key:json.loads((RUNROOT/'commands'/('w2c7-checkpoint-v1-'+key)/'command.json').read_bytes())['exit_code'] for key in ['strict','profile','docs']}
assert serial==dict(strict=1,profile=1,docs=0)
save('nonpasses-v1.json',dict(managed=json.loads((HERE/'managed-Debug-identities-v1.json').read_bytes())['nonpasses'],python=details,python_event_delta=delta,source_pin_first_introduced='C6; NOT_INHERITED_FROM_V5',prior_unordered_SPC='Earlier FAIL-to-ERROR classification remains the same distinct inherited identity; C7 actual first KeyError is retained with its own hash and seed38. No test or expectation changed.',unresolved_source_and_policy_failures='All retained nonpasses remain nonpasses; no independent scientific/native obligation is discharged.'))
managed=json.loads((HERE/'managed-validation-v1.json').read_bytes())
validation=dict(status='C7_FRESH_PARTIAL_W2_VALIDATION_NOT_READY',managed_Debug=managed['Debug'],Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',Release_compile='PASS_NO_RESTORE_COMPILE_ONLY',Python_method_outcomes=dict(method_outcomes),Python_nonpass_events=dict(event_outcomes),Python_receipt_compatible_arithmetic=dict(passed=764,failed=181,errors=11),Python_workers=6,Python_identity_occurrence_delta=0,Python_event_delta=delta,Node=nodes,Node_total=916,strict_profile_MkDocs=serial,current_frozen_per_configuration=dict(preedit=28966,late=458,total=29424,exact=True),exports=dict(Debug=268,Release=268),new_original_queries=0,NOT_RUN=['Full and selected Release unit tests:NOT_RUN_OWNER_PREFERENCE','Canonical portable/Legacy PowerShell wrappers:implicit restore/shared outputs/XLL packaging; isolated offline equivalents used','Native Excel/COM/UIA/RefEdit/manual Office/Mac/WASM smoke','Independent scientific/R or new archived original reference execution','Restore/install/installer/XLL packaging/signing/deployment','W3 or Phase04A39'],gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN'))
save('validation-v1.json',validation)
summary=json.loads((HERE/'ownership-summary-v1.json').read_bytes())
save('readiness-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',mandatory_blockers=dict(historical_OPEN=summary['historical']['OPEN'],current_OPEN=summary['current']['OPEN'],distinct_support_OPEN=summary['support']['distinct_OPEN']),validation=validation,new_original_queries=0,owner_decision_needed=False))
print(json.dumps(dict(Python_methods=dict(method_outcomes),Python_nonpass_events=dict(event_outcomes),Node=916,managed_effective=managed['Debug']['outcomes'])))
