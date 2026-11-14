from common import *
focused=json.loads((HERE/'focused-bc-v1-packet-v1-Debug.json').read_bytes());tests={r['suite']:r['tests']['outcomes'] for r in focused['results'] if r['kind']=='TEST'}
assert tests==dict(Common={'Passed':24},Legacy={'Passed':16})
save('production-test-freeze-v1.json',dict(status='C13_FINAL_PRODUCTION_TEST_INPUTS_FROZEN',sources=[pin(ROOT/p) for p in json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths']],
    focused=pin(HERE/'focused-bc-v1-packet-v1-Debug.json'),Debug_production=pin(HERE/'build-packet-v1-Debug.json'),new_managed_tests=dict(Common=24,Windows=16,total=40),
    Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',original_queries=0))
rows=[]
names=['inspect_stage_v1.py','inspect_metadata_v5.py','compatibility_v1.py','MethodMetadata-v11.cs','native-metadata-owners-v3.txt','ReturnMetadata-v1.cs','DeclarationShapes-v1.cs','validate_managed_v1.py','report_managed_v1.py','run_auxiliary_v1.py']
for name in names:
    source=PREVIOUS/name;value=source.read_text(encoding='utf-8')
    if name in ['validate_managed_v1.py','run_auxiliary_v1.py','report_managed_v1.py']:value=value.replace('w2c12-checkpoint','w2c13-checkpoint')
    if name=='inspect_metadata_v5.py':value=value.replace('packet-v2-context-v5','packet-v1-context-v5')
    if name=='report_managed_v1.py':
        value=value.replace('v11','v12').replace('C11','C12').replace('NEW_C12','NEW_C13')
        value=value.replace('len(added)==31','len(added)==40').replace('Passed=20834','Passed=20874').replace('Passed=20803','Passed=20834')
        value=value.replace('new_passes=31','new_passes=40').replace('new_PASS=31','new_PASS=40').replace('31 genuinely','40 genuinely')
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
    rows.append(dict(before=pin(source),after=pin(HERE/name),scope='Current continuation paths,40 measured new test identities and exact C12 baseline; unchanged original recorders, offline build/source/capture logic and historical test assertions.'))
save('checkpoint-tools-source-crosswalk-v1.json',dict(rows=rows,original_queries=0))
print('Seven production/test sources frozen; isolated current-only compilation/ABI/CIL/compatibility and Debug recipes prepared')
