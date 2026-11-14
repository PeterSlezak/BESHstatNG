"""Finish C7 entry: historical before-pins authenticate preimages, not live after-files."""
from common import *
anchors=['owner-review.md','NEXT_WAVE_HANDOVER.md','remaining-work-queue-v1.json','new-closures-v1.json','whole-body-contracts-v1.json','proof-authority-crosswalk-v1.json','original-fragment-parent-crosswalk-v1.json','current-ownership-v1.json','historical-ownership-v1.json','source-transitions-final-v1.json','readiness-v1.json','review-seal-v1.json','independent-controls-v1.json','command-receipts-v1.json']
transitions=json.loads((PREVIOUS/'source-transitions-final-v1.json').read_bytes())['files']
historical={(t['before']['path'],t['before']['bytes'],t['before']['sha256']):t for t in transitions}
refs={}
def walk(x):
    if isinstance(x,dict):
        if all(k in x for k in ['path','bytes','sha256']) and isinstance(x['path'],str) and x['sha256'] is not None:refs[x['path'],x['bytes'],x['sha256']]=x
        for v in x.values():walk(v)
    elif isinstance(x,list):
        for v in x:walk(v)
for name in anchors:
    if name.endswith('.json'):walk(json.loads((PREVIOUS/name).read_bytes()))
checked=[]
for key,row in refs.items():
    if key in historical:
        t=historical[key]
        assert pin(ROOT/t['preimage']['path'])==t['preimage'] and (t['preimage']['bytes'],t['preimage']['sha256'])==(row['bytes'],row['sha256'])
        assert pin(ROOT/t['after']['path'])==t['after'] and pin(ROOT/t['snapshot']['path'])==t['snapshot']
        checked.append(dict(expected=row,authority='AUTHENTICATED_C7_BEFORE_TO_PRESERVED_PREIMAGE',preimage=t['preimage'],live_after=t['after']))
    else:
        actual=pin(ROOT/row['path']);assert all(actual[k]==row[k] for k in actual),row['path']
        checked.append(dict(expected=row,authority='EXACT_LIVE_REFERENCED_PIN'))
assert git('branch','--show-current').decode().strip()=='codex/04A-38' and git('rev-parse','HEAD').decode().strip()==BASE and git('diff','--cached','--name-only')==b''
queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())
expected=['SSIZE_TTEST_UNPAIRED','SSIZE_PROP_SINGLE','SSIZE_PROP_INDEP','SSIZE_LOGRANK','SSIZE_COX_BINARY','SSIZE_COX_CONTINUOUS','SSIZE_ICC','SSIZE_BLANDALTMAN','MakeTwoGroupTable','MakeEquivalenceTwoGroupTable','MakeIndependentProportionsTable','MakeEquivalenceIndependentProportionsTable','ResolveSampleSizeHypothesisType','MakeLogRankPlanningTable','MakeCoxPlanningTable','MakeIccPlanningTable','MakeBlandAltmanPlanningTable','lambda','ResolveImportedPredictorNames','TryGetAlignedClusterIdColumnObject','TryBuildGeeDataFromUdfArgs','TryBuildGlmDataFromUdfArgs','TryBuildPredictorDataFromUdfArgs','TryBuildCoxDataFromUdfArgs']
assert [r['name'] for r in queue['first_members']]==expected
save('v7-evidence-pins-entry-v2.json',dict(anchors=[pin(PREVIOUS/n) for n in anchors],referenced=checked,all_exact_under_explicit_chronology=True,original_queries=0))
prior=[pin(HERE/n) for n in ['git-entry-v1.json','v7-source-proof-entry-v1.json','v7-live-preservation-entry-v1.json','v7-publication-entry-v1.json']]
save('entry-v2.json',dict(status='LIVE_C7_AUTHENTICATED_BEFORE_C8_SOURCE_EDITS',reader=pin(HERE/'entry_v2.py'),common=pin(HERE/'common.py'),prior_completed_live_checks=prior,instructions=pin(ROOT/'AGENTS.md'),master_plan=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'),queue=pin(PREVIOUS/'remaining-work-queue-v1.json'),first_members=queue['first_members'],pin_chronology='All7 apparent before/live differences are authorized C7 source transitions, independently hash-matched to immutable before/after images. No protected reference conflict.',retained_failed_attempts=[dict(reader=pin(HERE/'entry_v1.py'),exit_code=1,reason='Unconditional live lookup incorrectly applied historical before pins to current after paths'),dict(reader=pin(HERE/'inspect_entry_pins_v1.py'),exit_code=1,diagnostic=pin(HERE/'entry-pin-shape-diagnostic-v1.json'),reason='All7 differences identified; explicitly routed through authentic C7 transition records'),dict(kind='Read-only shell diagnostic',exit_code=1,reason='PowerShell quoting produced SyntaxError; no script or source executed')],negative_controls='C7 directed and publication controls source/receipt authenticated unchanged; no unit suite rerun',original_queries=0))
print('C8 live entry PASS: original/current pin chronology exact; no source changes or original queries',flush=True)
