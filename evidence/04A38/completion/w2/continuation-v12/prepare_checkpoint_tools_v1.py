from common import *
import shutil
focused=json.loads((HERE/'focused-bc-v1-packet-v1-Debug.json').read_bytes())
tests={r['suite']:r['tests']['outcomes'] for r in focused['results'] if r['kind']=='TEST'}
assert tests['Common']=={'Passed':21} and tests['Legacy']=={'Passed':10}
save('production-test-freeze-v1.json',dict(status='C12_FINAL_PRODUCTION_TEST_INPUTS_FROZEN',sources=[pin(ROOT/p) for p in json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths']],
    focused=pin(HERE/'focused-bc-v1-packet-v1-Debug.json'),Debug_production=pin(HERE/'build-packet-v1-Debug.json'),
    new_managed_tests=dict(Common=21,Windows=10,total=31),Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',original_queries=0))
rows=[]
for name in ['inspect_stage_v1.py','inspect_metadata_v5.py','compatibility_v1.py','MethodMetadata-v11.cs','native-metadata-owners-v3.txt','ReturnMetadata-v1.cs','DeclarationShapes-v1.cs','validate_managed_v1.py']:
    source=PREVIOUS/name;value=source.read_bytes()
    if name=='validate_managed_v1.py':value=value.replace(b'w2c11-checkpoint-v1',b'w2c12-checkpoint-v1').replace(b'packet-v2',b'packet-v1')
    with (HERE/name).open('xb') as stream:stream.write(value)
    rows.append(dict(before=pin(source),after=pin(HERE/name),scope='Retained current-only compiler/recorder/source with C12 stage/label update only where needed. Original statistical sources/DLLs never executed or measured anew.'))
# New nested-type binding names were source/compiled-type authenticated in the entry queue.
source=PREVIOUS/'proof_kernel_v3.py';value=source.read_text().replace('"CorrespondenceHandle"}}','"CorrespondenceHandle", "MultipleCorrespondenceHandle", "DiscriminantHandle"}}')
assert value!=source.read_text()
with (HERE/'proof_kernel_v4.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
rows.append(dict(before=pin(source),after=pin(HERE/'proof_kernel_v4.py'),scope='Exact same source-to-constructed-CIL binder; adds two source-declared, measured nested handle types from entry proof.'))
save('checkpoint-tools-source-crosswalk-v1.json',dict(rows=rows,original_queries=0))
print(json.dumps(dict(production_test_sources=6,new_test_cases=31,focused=tests)))
