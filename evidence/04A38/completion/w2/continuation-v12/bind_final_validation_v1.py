"""Bind final actual identity receipts to the unchanged production/test freeze."""
from common import *

freeze=json.loads((HERE/'production-test-freeze-v1.json').read_bytes())
for row in freeze['sources']:assert pin(ROOT/row['path'])==row
post=json.loads((HERE/'python-frozen-input-postcheck-v1.json').read_bytes())
assert post['status']=='EXACT_FROZEN_INPUTS_UNCHANGED'
names=['production-test-freeze-v1.json','source-transitions-final-v1.json',
    'actual-managed-shipping-binding-v1.json','managed-Debug-identities-v1.json',
    'python-scan-input-freeze-v1.json','python-frozen-input-postcheck-v1.json',
    'python-effective-identities-v1.json','python-nonpass-investigation-v1.json',
    'validation-v1.json','validation-independent-replay-v1.json',
    'compatibility-packet-v1.json','frozen-provenance-v1.json',
    'release-execution-receipt-v1.json','compatibility-fixture-crosswalk-v1.json']
save('final-source-validation-binding-v1.json',dict(status='FINAL_FROZEN_SOURCES_ACTUAL_IDENTITY_CIL_AND_CURRENT_COMPATIBILITY_BOUND_NOT_READY',
    authorities=[pin(HERE/name) for name in names],sources=freeze['sources'],
    Debug_comprehensive_campaigns=1,Python_comprehensive_campaigns=1,
    Release_full_unit_suites='NOT_RUN_OWNER_PREFERENCE',Release_selected_unit_suites='NOT_RUN_OWNER_PREFERENCE',original_queries=0))
proofs=['entry-authority-pins-v1.json','entry-executable-authentication-v1.json',
    'entry-queue-receipt-v1.json','entry-original-dependency-addendum-v2.json',
    'whole-body-contracts-v1.json','callback-proofs-v1.json','portable-api-contracts-v1.json',
    'host-cache-contexts-v1.json','constructed-owner-proofs-v1.json',
    'compiled-method-deltas-v1.json','new-parent-fragment-crosswalk-v1.json',
    'historical-ownership-v2.json','current-ownership-v2.json','ownership-summary-v2.json',
    'new-closures-final-v1.json','remaining-work-queue-v1.json',
    'source-transitions-final-v1.json','independent-static-reader-v1.json',
    'final-source-validation-binding-v1.json']
save('proof-authority-crosswalk-v1.json',dict(status='C12_COMPLETE_REFERENCED_AUTHORITY_CROSSWALK_NOT_READY',
    authorities=[pin(HERE/name) for name in proofs],historical_universe=704,current_universe=726,
    support_universe=303,new_historical=33,new_current=33,new_support=2,
    original_queries=0,scope='Exact byte authorities; support requires complete body/dependency/callback/source/CIL/fragment and behavioral reconciliation, with unexecuted dynamic arms explicitly retained.'))
print('Final frozen source and proof authorities bound')
