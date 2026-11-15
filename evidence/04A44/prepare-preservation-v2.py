from common44 import *
old=load(PRIVATE/'tracked-entry-inventory-v1.json')['files'];initial={r['path']:r for r in old};a=load(ROOT/'evidence/04A43/entry/authentication-v1.json');b=load(HERE/'entry/authentication-v1.json');c=load(ROOT/'evidence/04A42/entry/authentication-v1.json');trans=[]
for group in [c['merged41_newline_transitions'],a['merged42_newline_transitions'],b['merged43_newline_transitions']]:
 for r in group:
  if initial[r['current']['path']]!=r['current']:trans.append(dict(historical_owner_merge=r['current'],current_entry=initial[r['current']['path']]))
s=(HERE/'preservation-v1.py').read_text().replace("for r in table.values():assert initial[r['current']['path']]==r['current'];exact(r['protected_exact'])","for r in table.values():\n        exact(r['protected_exact'])\n        if initial[r['current']['path']]!=r['current']:\n            assert r['current']['path'] in {p['path'] for p in old_history},'UNRECONCILED_NONHISTORY_TRANSITION'\n            # The full authorized40/41/42/43/owner successor chain above already proved initial[path].")
for batch in ['41','42','43']:s=s.replace('inherited'+batch+'-current-raw-inventory-v1','inherited'+batch+'-current-raw-inventory-v2')
with (HERE/'preservation-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('preservation/failed-readback-v1-disposition.json',dict(status='REPORT_ASSERTION_ASSUMED_HISTORICAL_OWNER_PREIMAGE_STILL_CURRENT;NO_RAW_BYTE_MISMATCH',transitions=trans,correction='Apply authenticated later accepted43 transitions after older41/42 newline receipts;the complete original1353 source chain remains independently validated before this redundant check. No original pin modified.',failed_recipe=pin(HERE/'preservation-v1.py'),gates=GATES))
