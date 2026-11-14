from common import *
freeze=json.loads((HERE/'production-test-freeze-v1.json').read_bytes())
for r in freeze['sources']:assert pin(ROOT/r['path'])==r
names=['production-test-freeze-v1.json','source-transitions-final-v1.json','source-transition-independent-proof-v1.json','source-composition-proof-v1.json',
 'actual-managed-shipping-binding-v1.json','managed-Debug-identities-v1.json','python-scan-input-freeze-v2.json','python-frozen-input-postcheck-v1.json',
 'python-effective-identities-v1.json','python-nonpass-investigation-v1.json','validation-v1.json','validation-independent-replay-v1.json',
 'compatibility-packet-v1.json','frozen-provenance-v1.json','release-execution-receipt-v1.json','compatibility-fixture-crosswalk-v1.json',
 'whole-body-contracts-v1.json','callback-proofs-v1.json','portable-api-contracts-v1.json','host-cache-contexts-v1.json','constructed-owner-proofs-v1.json',
 'compiled-method-deltas-v1.json','new-parent-fragment-crosswalk-v2.json','historical-ownership-v2.json','current-ownership-v2.json','ownership-summary-v2.json',
 'new-closures-final-v1.json','remaining-work-queue-v1.json','roc-compatibility-repair-final-v1.json','property-body-contracts-v1.json']
save('final-source-validation-binding-v1.json',dict(status='FROZEN_SOURCE_ACTUAL_IDENTITY_CIL_COMPATIBILITY_BOUND_NOT_READY',authorities=[pin(HERE/n) for n in names],sources=freeze['sources'],
 Debug_comprehensive_campaigns=1,Python_comprehensive_campaigns=1,Node_campaigns=1,Release_full_unit_suites='NOT_RUN_OWNER_PREFERENCE',Release_selected_unit_suites='NOT_RUN_OWNER_PREFERENCE',original_queries=0))
save('proof-authority-crosswalk-v1.json',dict(authorities=[pin(HERE/n) for n in names],historical_universe=704,current_universe=726,support_universe=303,
 new_historical=35,new_current=35,new_support=3,blocked=[],original_queries=0))
