"""Bind final source/identity/provenance and actual compiled managed shipping artifacts."""
from common import *
from proof_kernel_v3 import definitions,semantic

freeze=json.loads((HERE/'production-test-freeze-v1.json').read_bytes())
for row in freeze['sources']:assert pin(ROOT/row['path'])==row
comparison=pin(HERE/'compatibility-packet-v2.json')
previous=json.loads((PREVIOUS/'frozen-provenance-v1.json').read_bytes())
save('frozen-provenance-v1.json',dict(configurations=previous['configurations'],comparison=comparison,
    original_authority=pin(PREVIOUS/'frozen-provenance-v1.json'),new_original_queries=0,scientific_truth='NOT_GOLDEN',
    scope='Same original/input/recorder and late/genuine classification, linked to fresh C11 current-only outputs in both configurations.'))
history=json.loads((HERE/'historical-ownership-v2.json').read_bytes())['members']
new={r['original_id'] for r in json.loads((HERE/'new-closures-final-v1.json').read_bytes())['rows']}
save('proof-authority-crosswalk-v1.json',dict(rows=[dict(original_id=h['canonical_original_id'],current_ids=h['current_ids'],
    authority=h['proof'] if h['canonical_original_id'] in new else dict(prior='../continuation-v10/proof-authority-crosswalk-v1.json',
        proof=h['proof'],current_position_mapping='current-semantic-position-crosswalk-v1.json'),
    kind='C11_COMPLETE_SOURCE_CONSTRUCTED_CIL_BRANCH_INLINE_CALLBACK_FIELD' if h['canonical_original_id'] in new else 'RETAINED_EXACT_V10_AUTHORITY',
    whole_parent_supported=True) for h in history if h['proof']],original_queries=0))
save('final-source-validation-binding-v1.json',dict(sources=freeze['sources'],freeze=pin(HERE/'production-test-freeze-v1.json'),
    managed=pin(HERE/'managed-validation-v1.json'),python_postcheck=pin(HERE/'python-frozen-input-postcheck-v1.json'),
    production_test_changes_after_full_validation=0,original_queries=0,
    scope='Only additive generated accounting/readers/review/publication follows the unchanged production/test freeze. No source/assertion changed during or after the full campaigns.'))
save('release-execution-receipt-v1.json',dict(Release_full_unit_suites='NOT_RUN_OWNER_PREFERENCE',
    Release_selected_unit_suites='NOT_RUN_OWNER_PREFERENCE',Debug_full_campaigns=1,
    Debug_shipping=pin(HERE/'build-packet-v2-Debug.json'),Release_shipping=pin(HERE/'build-packet-v2-Release.json'),
    Release_metadata=pin(HERE/'metadata-all/packet-v2-context-v5/current-Release.json'),
    Release_export_check=pin(HERE/'inspection/packet-v2/Inspect38-Release.json'),
    dual_configuration_fresh_compatibility=comparison,restore='NOT_RUN',install='NOT_RUN',
    native='NOT_RUN',scientific_oracle='NOT_RUN',packaging_signing_deployment='NOT_RUN',original_queries=0))
managed=RUNROOT/'builds/w2c11-checkpoint-v1/Debug/legacy'
shipping=PRIVATE/'packet-v2/Debug/lib'
rows=[]
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll']:
    a=pin(managed/name);b=pin(shipping/name);rows.append(dict(actual_managed=a,measured_shipping=b,physical_bytes_equal=(a['bytes'],a['sha256'])==(b['bytes'],b['sha256'])))
if not all(r['physical_bytes_equal'] for r in rows):
    output=HERE/'metadata-managed-Debug-v1.json'
    result=command('managed-current-metadata-binding-v1',[str(PRIVATE/'packet-v2/metadata-all-v5/MethodMetadata38.exe'),
        str(managed),str(output),str(HERE/'native-metadata-owners-v3.txt')]);assert result['exit_code']==0
    actual=definitions(json.loads(output.read_bytes()))
    expected=definitions(json.loads((HERE/'metadata-all/packet-v2-context-v5/current-Debug.json').read_bytes()))
    assert set(actual)==set(expected) and all(semantic(actual[k])==semantic(expected[k]) for k in expected)
    measured=dict(metadata=pin(output),command=clean(result),all_actual_method_semantics_equal=True)
else:measured=dict(all_actual_physical_bytes_equal=True)
save('actual-managed-shipping-binding-v1.json',dict(assemblies=rows,actual=measured,
    scope='Independent actual full Debug test-consumer production assemblies match the measured shipping method/parameter/local/exception/CIL context, with physical equality where present. No original assembly load/execution.',original_queries=0))
print('Source/frozen/release/whole-proof and actual managed-shipping bindings complete',flush=True)
