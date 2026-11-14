"""Reconcile exact scanner tuples; describe syntax without suppressing any assertion."""
from common_v2 import *
from collections import Counter
prior=json.loads((PREVIOUS/'python-message-delta-investigation-v1.json').read_bytes());current=json.loads((HERE/'python-message-delta-investigation-v1.json').read_bytes());before=[]
for r in prior['helper_raw']:
    assert pin(ROOT/r['path'])==r
    for row in json.loads((ROOT/r['path']).read_bytes())['rows']:before.extend(row['findings'])
key=lambda r:json.dumps(r,sort_keys=True);a=Counter(map(key,before));b=Counter(map(key,current['all_actual_privacy_findings']));assert len(before)==206 and len(current['all_actual_privacy_findings'])==232 and not a-b
extra=[json.loads(k) for k,count in (b-a).items() for _ in range(count)];assert len(extra)==26 and all('/continuation-v9/' in r['path'] for r in extra)
classes=Counter()
for r in extra:
    if r['path'].endswith('dependency-source-universe-v1.json'):
        assert r['category']=='appdata_or_drive_path';label='SERIALIZED_CRLF_AFTER_COLON_LABEL_NOT_REAL_DRIVE'
    elif r['path'].endswith('prove_parallel_scan_v1.py'):
        assert r['category'] in ['domain_sid','email','connection_secret','internal_hostname'];label='SYNTHETIC_SID_EMAIL_HOST_CONNECTION_ADVERSE_CONTROL'
    else:
        assert r['path'].endswith(('compatibility_v1.py','inspect_stage_v1.py')) and r['category']=='appdata_or_drive_path';label='FIXED_PUBLIC_TOOL_REFERENCE_SOURCE'
    classes[label]+=1
assert sorted(classes.values())==[2,6,18]
save('privacy-findings-delta-v1.json',dict(status='EXACT206_OLD_PLUS26_NEW_TUPLES_ALL_RETAINED_AS_SCANNER_FAILURES',prior_helpers=prior['helper_raw'],baseline_count=206,current_count=232,missing_old=[],additional=extra,classification_counts=dict(classes),scope='18 raw/decoded JSON source-fragment CRLF matches are c-colon-backslash-r/n-colon-backslash-r label syntax;6 new synthetic scanner controls;2 fixed public tool references. No scanner tuple or historical assertion is removed or waived. Final publication policy is checked separately.'))
source=(HERE/'seal_and_controls_v2.py').read_text(encoding='utf-8').replace("    names.extend(['publication-privacy-authority-v1.json'","    names.extend(['privacy-findings-delta-v1.json','python-additional-message-source-proof-v1.json','failed-aggregation-attempt-v1.json','aggregation-correction-crosswalk-v1.json','publication-privacy-authority-v1.json'")
with (HERE/'seal_and_controls_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
print('Exact privacy tuple delta',dict(classes))
