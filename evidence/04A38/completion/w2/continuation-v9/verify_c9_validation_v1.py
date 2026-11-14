"""Independent complete Debug/Python-method/event/worker/Node recount; never READY."""
import json,hashlib
from collections import Counter
def need(value,message):
    if not value:raise ValueError(message)
def inspect(load,prefix,raw):
    oldprefix='evidence/04A38/completion/w2/continuation-v8/'
    validation=load(prefix+'validation-v1.json');managed=load(prefix+'managed-Debug-identities-v1.json');old=load(oldprefix+'managed-Debug-identities-v1.json')
    key=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'])
    now={key(r):r for r in managed['tests']}
    need(len(now)==len(managed['tests'])==20758,'Managed identity missing/duplicated')
    need(all(key(r) in now and now[key(r)]['outcome']==r['outcome'] for r in old['tests']),'Managed inherited outcome drift')
    outcomes=Counter(r['outcome'] for r in now.values());need(outcomes==Counter(Passed=20744,Failed=14),'Managed actual outcome recount')
    fields=lambda r:(*key(r),r['outcome'],r['message_sha256'])
    need(Counter(map(fields,managed['nonpasses']))==Counter(map(fields,old['nonpasses'])),'Managed nonpass identity/occurrence/message drift')
    need(len(managed['new_tests'])==24 and all(r['outcome']=='Passed' for r in managed['new_tests']),'New managed population mismatch')
    initial=Counter(r['first_full_outcome'] for r in managed['tests']);need(initial==outcomes,'Concealed corrective full-run stitching')
    need(all(r['correction'] is None for r in managed['tests']),'Unexpected C8 full-run correction')
    execution=load(prefix+'managed-validation-v1.json');need(execution['full_Debug_runs']==1 and execution['Debug']['first_full_outcomes']==dict(outcomes),'Full Debug campaign accounting')
    need(validation['Release_full']==validation['Release_selected']=='NOT_RUN_OWNER_PREFERENCE','Release unit evidence falsely claimed')
    inventory=load(prefix+'python-discovery-inventory-v1.json');python=load(prefix+'python-effective-identities-v1.json')
    need(len(inventory['entries'])==956 and Counter(r['id'] for r in inventory['entries'])==Counter(r['id'] for r in python['entries']),'Python parent missing/duplicated')
    need(python['missing']==python['duplicated']==python['skipped']==[] and python['raw_attempt_count']==956,'Hidden Python repeats/skips/stubs')
    parents=Counter(r['outcome'] for r in python['entries']);need(parents==Counter(python['method_outcomes']) and set(parents)<={'PASS','FAIL','ERROR'},'Parent/event units conflated')
    workers=load(prefix+'python-worker-accounting-v1.json');receipts=load(prefix+'python-worker-receipts-v1.json')
    need(workers['workers_at_peak']==6 and workers['raw_attempts']==956 and workers['corrective_repeats']==0,'Worker/attempt accounting')
    expected={(n,'MAIN') for n in range(1,7)}|{(5,'DISPOSABLE')}
    need({(r['lane'],r['phase']) for r in workers['executions']}==expected and len(workers['executions'])==7,'Dropped worker phase')
    need(sum(r['methods'] for r in workers['executions'])==956 and all(r['exit_code'] in [0,1] and r['seconds']>=0 for r in workers['executions']),'Abnormal/missing worker execution')
    schedule=receipts['schedule'];need(schedule['other_lanes_completed_before_scanner'] and schedule['non_scanner_end_seconds']<=schedule['disposable_end_seconds']<=schedule['scanner_start_seconds']<=schedule['scanner_end_seconds'],'Unsafe scanner/test overlap')
    need(schedule['max_non_scanner_workers']==5 and schedule['max_scanner_helpers']==5 and schedule['max_scanner_parent_plus_helpers']==6,'Worker ceiling exceeded')
    events=load(prefix+'python-first-failures-v1.json')['records'];before=load(oldprefix+'python-first-failures-v1.json')['records']
    def event_key(r):
        sub=r.get('subtest');suffix=sub[len(r['identity']):].strip() if sub is not None else ''
        return r['identity'],r['outcome'],suffix,r['occurrence']
    need(Counter(map(event_key,events))==Counter(map(event_key,before)),'New/changed Python nonpass requires explicit investigation')
    counts=Counter(r['outcome'] for r in events);need(counts==Counter(python['nonpass_events']) and counts==Counter(FAIL=181,ERROR=11),'Failure-event recount')
    need(parents==Counter(PASS=830,FAIL=115,ERROR=11),'Actual parent-method baseline changed')
    source_pin=[r for r in events if r['identity']=='test_04a27_transport_successor.Transport27SuccessorTests.test_original_member_ids_and_body_pins_not_fabricated']
    need(len(source_pin)==1 and source_pin[0]['outcome']=='FAIL' and source_pin[0]['classification']=='RETAINED_C6_INTRODUCED_04A27_SOURCE_PIN_FAIL_NOT_INHERITED_FROM_V5_NOT_WAIVED','Source-pin failure relabelled or waived')
    spc=[r for r in events if r['identity']=='test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact'];need(len(spc)==1 and spc[0]['outcome']=='ERROR','Unordered-SPC error lost')
    nodes=load(prefix+'node-identities-v1.json')['suites'];old_nodes={r['suite']:r for r in load(oldprefix+'node-identities-v1.json')['suites']}
    need(sum(len(r['identities']) for r in nodes)==916 and all(Counter(r['identities'])==Counter(old_nodes[r['suite']]['identities']) for r in nodes),'Node identity population changed')
    need(validation['strict_profile_MkDocs']==dict(strict=1,profile=1,docs=0),'Auxiliary failures concealed')
    return dict(status='INDEPENDENT_C9_VALIDATION_RECOUNT_PASS_NOT_READY',managed_first_full=dict(initial),managed_effective=dict(outcomes),Python_methods=dict(parents),Python_nonpass_events=dict(counts),workers_at_peak=6,Node=916,Release_unit='NOT_RUN_OWNER_PREFERENCE')
