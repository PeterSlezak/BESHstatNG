from common45 import *
import re
lock();plan=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';before=plan.read_bytes()
snapshot=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.21.md'
with snapshot.open('xb') as f:f.write(before)
text=before.decode('utf-8-sig')
line='**Revision 2.22 · 16 November 2026 · 04A.44 owner-accepted/merged; 04A.45 safe harness in progress**\n**Current owner authority:** Accepted44 merged main independently authenticated at `'+MAIN+'`, direct child of `'+PRIOR+'`. Owner authorizes04A.45 SAFE HARNESS DEVELOPMENT only. No live Excel/COM/UIA/RefEdit observation or XLL packaging is authorized. Independent exact Windows/COM risk decision remains NOT_APPROVED; N1=NO GO; N2=NO.04A.46 is not authorized. Exact Revision2.21 and all226 original roadmap IDs are preserved.\n\n**Historical Revision2.21 entry (retained verbatim below):**\n'
text=text[:text.index('\n')+1]+'\n'+line+text[text.index('\n')+1:]
plan.write_bytes(text.encode('utf-8'))
ids=lambda t:list(dict.fromkeys(re.findall(r'^\|\s*\*\*(\d{2}A?\.\d{2}[A-Z]?)\*\*',t,re.M)))
assert ids(text)==ids(before.decode('utf-8-sig')) and len(ids(text))==226
save('planning/authority-v1.json',dict(entry=pin(snapshot),current=pin(plan),identities=ids(text),revision='2.22',historical_revision='2.21',charter_revision='2.12',roadmap_rows_changed=False,owner_authority='Current prompt;safe45 only',native_authorization=False,gates=GATES))
