"""Frozen provenance and actual full-test consumer linkage before Python freeze."""
from common import *
from proof_kernel_v4 import definitions,semantic
freeze=json.loads((HERE/'production-test-freeze-v1.json').read_bytes())
for row in freeze['sources']:assert pin(ROOT/row['path'])==row
comparison=pin(HERE/'compatibility-packet-v1.json');previous=json.loads((PREVIOUS/'frozen-provenance-v1.json').read_bytes())
save('frozen-provenance-v1.json',dict(configurations=previous['configurations'],comparison=comparison,original_authority=pin(PREVIOUS/'frozen-provenance-v1.json'),new_original_queries=0,scientific_truth='NOT_GOLDEN',scope='Identical frozen original/input/recorder authority and genuine/late classification; fresh C12 current-only outputs for both configurations.'))
save('release-execution-receipt-v1.json',dict(Release_full_unit_suites='NOT_RUN_OWNER_PREFERENCE',Release_selected_unit_suites='NOT_RUN_OWNER_PREFERENCE',Debug_full_campaigns=1,
    Debug_shipping=pin(HERE/'build-packet-v1-Debug.json'),Release_shipping=pin(HERE/'build-packet-v1-Release.json'),Release_metadata=pin(HERE/'metadata-all/packet-v1-context-v5/current-Release.json'),
    Release_export_check=pin(HERE/'inspection/packet-v1/Inspect38-Release.json'),dual_configuration_fresh_compatibility=comparison,restore='NOT_RUN',install='NOT_RUN',native='NOT_RUN',scientific_oracle='NOT_RUN',packaging_signing_deployment='NOT_RUN',original_queries=0))
managed=RUNROOT/'builds/w2c12-checkpoint-v1/Debug/legacy';shipping=PRIVATE/'packet-v1/Debug/lib';rows=[]
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll']:
    a=pin(managed/name);b=pin(shipping/name);rows.append(dict(actual_managed=a,measured_shipping=b,physical_bytes_equal=(a['bytes'],a['sha256'])==(b['bytes'],b['sha256'])))
if not all(row['physical_bytes_equal'] for row in rows):
    output=HERE/'metadata-managed-Debug-v1.json'
    result=command('managed-current-metadata-binding-v1',[str(PRIVATE/'packet-v1/metadata-all-v5/MethodMetadata38.exe'),str(managed),str(output),str(HERE/'native-metadata-owners-v3.txt')]);assert result['exit_code']==0
    actual=definitions(json.loads(output.read_bytes()));expected=definitions(json.loads((HERE/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes()))
    assert set(actual)==set(expected) and all(semantic(actual[key])==semantic(expected[key]) for key in expected)
    measured=dict(metadata=pin(output),command=clean(result),all_actual_method_semantics_equal=True)
else:measured=dict(all_actual_physical_bytes_equal=True)
save('actual-managed-shipping-binding-v1.json',dict(assemblies=rows,actual=measured,scope='Actual full Debug campaign production DLLs measured independently where physical bytes differ; every method/parameter/local/exception/CIL semantics matches shipping evidence. Original DLLs never loaded or executed.',original_queries=0))
print('Frozen original/late provenance and actual full Debug consumer CIL authenticated')
