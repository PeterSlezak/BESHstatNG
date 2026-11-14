"""Independent exact managed/method/subtest/worker/Node recount. Never READY."""
from collections import Counter
import json,hashlib
def need(value,message):
    if not value:raise ValueError(message)
def inspect(load,prefix,raw):
    validation=load(prefix+'validation-v1.json');managed=load(prefix+'managed-Debug-identities-v1.json');old=load('evidence/04A38/completion/w2/continuation-v6/managed-Debug-identities-v1.json')
    key=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'])
    now={key(r):r for r in managed['tests']}
    need(len(now)==len(managed['tests'])==20718,'Missing/duplicate managed identity occurrence')
    need(all(key(r) in now and now[key(r)]['outcome']==r['outcome'] for r in old['tests']),'Managed inherited identity/outcome drift')
    need(Counter(r['outcome'] for r in now.values())==Counter(Passed=20704,Failed=14),'Managed effective outcomes')
    fields=lambda r:(*key(r),r['outcome'],r['message_sha256'])
    need(Counter(map(fields,managed['nonpasses']))==Counter(map(fields,old['nonpasses'])),'Managed inherited nonpass message/occurrence drift')
    need(len(managed['new_tests'])==5 and all(r['outcome']=='Passed' for r in managed['new_tests']),'Unsupported new managed passes')
    initial=Counter(r['first_full_outcome'] for r in managed['tests']);need(initial==Counter(Passed=20700,Failed=18),'First full Debug outcomes concealed')
    changes=[r for r in managed['tests'] if r['first_full_outcome']!=r['outcome']]
    need(len(changes)==4 and all(r['method']=='TypedService_CandidateObservationOnly' and r['first_full_outcome']=='Failed' and r['outcome']=='Passed' for r in changes),'Unjustified managed correction')
    correction=load(prefix+'managed-environment-correction-v1.json')
    need(correction['source_unchanged'] and correction['result']['outcomes']==dict(Passed=4),'Missing focused environment correction')
    need(validation['Release_full']==validation['Release_selected']=='NOT_RUN_OWNER_PREFERENCE','Release unit tests falsely claimed')
    inventory=load(prefix+'python-discovery-inventory-v1.json');python=load(prefix+'python-effective-identities-v1.json')
    need(len(inventory['entries'])==956 and Counter(r['id'] for r in inventory['entries'])==Counter(r['id'] for r in python['entries']),'Missing/duplicate executed Python method')
    need(Counter(r['outcome'] for r in python['entries'])==Counter(PASS=830,FAIL=115,ERROR=11),'Actual parent-method outcome recount')
    need(python['missing']==python['duplicated']==python['skipped']==[],'Skipped Python population')
    workers=load(prefix+'python-worker-accounting-v1.json')
    need(workers['workers_at_peak']==6 and len(workers['initial'])==6 and {r['lane'] for r in workers['initial']}==set(range(1,7)),'Dropped Python worker')
    need(sum(r['methods'] for r in workers['initial'])==956 and all(r['seconds']>=0 and r['exit_code'] in [0,1] for r in workers['initial']),'Abnormal/missing Python worker execution')
    need(workers['initial_raw_attempts']==956 and workers['corrective_raw_attempts']==4 and python['raw_attempt_count']==960,'Corrective Python attempts hidden')
    effective=load(prefix+'python-first-failures-v1.json')['records'];need(len(effective)==192 and Counter(r['outcome'] for r in effective)==Counter(FAIL=181,ERROR=11),'Python failure-event recount')
    before=load('evidence/04A38/completion/w2/continuation-v6/nonpasses-v2.json')['python']
    old_events=Counter((r['identity'].split('(',1)[1].split(')',1)[0],r['outcome'],r['subtest']) for group in before for r in group['nonpasses'])
    def event_key(row):
        subtest=row.get('subtest');suffix=subtest[len(row['identity']):].strip() if subtest is not None else ''
        return row['identity'],row['outcome'],suffix
    need(Counter(map(event_key,effective))==old_events,'Changed Python nonpass identity/classification/occurrence')
    source_pin=[r for r in effective if r['identity']=='test_04a27_transport_successor.Transport27SuccessorTests.test_original_member_ids_and_body_pins_not_fabricated']
    need(len(source_pin)==1 and source_pin[0]['outcome']=='FAIL' and source_pin[0]['classification']=='RETAINED_C6_INTRODUCED_04A27_SOURCE_PIN_FAIL_NOT_INHERITED_FROM_V5_NOT_WAIVED','C6 source-pin failure falsely waived/inherited')
    need(workers['effective_method_outcomes']==dict(PASS=830,FAIL=115,ERROR=11) and workers['effective_nonpass_events']==dict(FAIL=181,ERROR=11),'Conflated method/subtest accounting')
    nodes=load(prefix+'node-identities-v1.json')['suites'];old_nodes={r['suite']:r for r in load('evidence/04A38/completion/w2/continuation-v6/node-identities-v1.json')['suites']}
    need(sum(len(r['identities']) for r in nodes)==916 and all(Counter(r['identities'])==Counter(old_nodes[r['suite']]['identities']) for r in nodes),'Complete Node identity drift')
    need(validation['strict_profile_MkDocs']==dict(strict=1,profile=1,docs=0),'Strict/profile failure concealed')
    return dict(status='INDEPENDENT_C7_VALIDATION_METHOD_EVENT_WORKER_RECOUNT_PASS_NOT_READY',managed_first_full=dict(initial),managed_effective=dict(Passed=20704,Failed=14),Python_methods=dict(PASS=830,FAIL=115,ERROR=11),Python_nonpass_events=dict(FAIL=181,ERROR=11),Python_workers=6,Node=916,Release_unit='NOT_RUN_OWNER_PREFERENCE')
