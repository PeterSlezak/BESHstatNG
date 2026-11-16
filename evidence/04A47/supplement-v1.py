from common47 import *
lock();d=load(HERE/'register/interim-delta-v1.json')
for r in d['restricted_issues']:
 if r['canonical_id']=='ISSUE:C21-STACKED-ROWCOUNT-VALIDATION-01':r.update(disposition='SOURCE_ONLY_RESTRICTED',software_evidence='Actual current Common logical-point policy and source-projected role/phase toggles tested;original Windows raw-count candidate itself not executed or corrected.')
save('register/interim-delta-v2.json',d)
paths=['evidence/04A45/native-risk/owner-decision-v1.md','evidence/04A46/native-risk/owner-decision-addendum-v1.md']
expected=['84a9c6c40e150ba5183590ba26b9165ce7bf87ea4d0f2162c8fe501df8921dc7','f6bac7506fd51c0414d4c4d85a6318228a26550d70a1a62cdfe167e478516135']
assert [pin(p)['sha256'] for p in paths]==expected
save('native-risk/dependency-v1.json',dict(authorities=[pin(p) for p in paths],approval='NOT_APPROVED',live_excel='NOT_RUN',actual_hwnd_pid_loaded_xll='NOT_VERIFIED',MayRunNative=False,original_control_discharge=0,AutoOpen_AutoUpdate='UNRESOLVED_STOP_BEFORE_ANY_LAUNCH',proposed_controls=pin(HERE/'register/interim-delta-v2.json'),gates=GATES))
