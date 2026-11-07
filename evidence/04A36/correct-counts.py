"""Correct a new-packet count description; frozen sources/outputs remain unchanged."""
from work import *
if __name__=='__main__':
    paths=['compatibility-comparison.json','correct-fixture-v2.py','development-attempts.json','fixture-correction-v2.json','packet.py','README.md','reconcile.py','summarize.py','tooling-corrections.json']
    for name in paths:
        p=HERE/name;s=p.read_text(encoding='utf-8-sig')
        s=s.replace('all-rejected','mostly rejected (6 valid / 378 rejected)')
        s=s.replace('Every v1 synthetic panel carried unmatched point rule numbers or was empty, so all384 presentation/axis rows recorded Core-construction ArgumentException:points. Those rows remain immutable rejected-input evidence, NOT valid portable-series parity.','Most V1 synthetic panels carried unmatched point rule numbers or were empty:378 construction rejections and6 valid outputs per384 rows. All rows remain immutable; V1 provides insufficient valid-series coverage.')
        s=s.replace('SPC V1 all construction rejections','SPC V1 only6 valid outputs and378 construction rejections')
        write('evidence/04A36/'+name,s)
    save('audit-count-correction.json',dict(reason='First new audit expected zero V1 successes; actual immutable V1 has6 valid outputs and378 rejections. Descriptions/new count assertion corrected to executed corpus. No old test, fixture input, expected vector or capture bytes changed.',first_audit=dict(passed=40,failed=1,errors=0,exit_code=1),corrected_expected=dict(v1_valid=6,v1_rejected=378,v2_valid=330,v2_rejected=54)))
