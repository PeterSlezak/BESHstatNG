from common46 import *
import re
lock();plan=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';before=plan.read_bytes();snapshot=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.22.md'
with snapshot.open('xb') as f:f.write(before)
text=before.decode('utf-8-sig');head='''
**Revision 2.23 · 16 November 2026 · 04A.45 safe software milestone accepted/merged; 04A.46 safe GUI wiring in progress**
**Current owner authority:** Authenticated accepted45 merged main `'''+MAIN+'''`, direct child of `'''+PRIOR+'''`. Owner authorizes04A.46 SAFE GUI-WIRING SOFTWARE WORK ONLY. Production behavior is unchanged. The45 Windows/COM packet remains NOT_APPROVED; actual HWND/PID/loaded XLL NOT_VERIFIED; live Excel/COM/UIA/RefEdit NOT_RUN. No native-control discharge or04A.47 authorization. All226 original IDs and immutable2.12/2.22 charters retained. Acceptance45 covers the safe software milestone only,not its native gate.

**Historical Revision2.22 entry (retained verbatim below):**
'''
text=text[:text.index('\n')+1]+head+text[text.index('\n')+1:];plan.write_bytes(text.encode('utf-8'))
ids=lambda t:list(dict.fromkeys(re.findall(r'^\|\s*\*\*(\d{2}A?\.\d{2}[A-Z]?)\*\*',t,re.M)))
assert ids(text)==ids(before.decode('utf-8-sig')) and len(ids(text))==226
save('planning/authority-v1.json',dict(entry=pin(snapshot),current=pin(plan),identities=ids(text),revision='2.23',historical_revision='2.22',original_charter='2.12',native_authorization=False,gates=GATES))
pins=runner().__class__.build.__globals__['source_pins']();entry=load(HERE/'entry/source-preimages-v1.json')['source_pins'];changed=[p for p in entry if pins.get(p)!=entry[p]];added=[p for p in pins if p not in entry]
assert changed==['BESHStatNG.Test/BESHStatNG.Tests.vbproj'] and len(added)==8 and all('/GuiWiring46' in p for p in added)
for name in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj','BESHstatNG/BESHstatNG.vbproj']:assert pins[name]==entry[name]
save('matrix/source-freeze-v1.json',dict(kind='04A46_POST_WAVE_AB_SINGLE_SOURCE_TEST_FREEZE',utc=utc(),source_pins=pins,source_delta=[dict(before=dict(path=p,sha256=entry[p]),after=pin(p)) for p in changed],new_tests=[pin(p) for p in added],production_changed=[],plan=pin(plan),plan_snapshot=pin(snapshot),focused_final=pin(HERE/'waves/b8.json'),new_passing_identities=343,native='NOT_RUN',gates=GATES))
save('fixtures/projection-final-v1.json',dict(wave_a=pin(HERE/'fixtures/projections-wave-a-v1.json'),wave_b=pin(HERE/'fixtures/projections-wave-b-v1.json'),type_aliases=load(HERE/'fixtures/host-aliases-wave-b-v4.json')['edits'],projections=[pin('BESHStatNG.Test/src/'+n) for n in ['GuiWiring46Projections.vb','GuiWiring46ExecutionProjections.vb','GuiWiring46Numbers.vb']],kind='CURRENT_MAIN_PRE_EDIT_04A46_SOURCE_DERIVED_FAKE_POST_TEST_SEAL',numerics=pin(HERE/'fixtures/numeric-source-v2.json'),production_preimages='Frozen before any new test source;shipping unchanged;no claim native constructor/default/acquisition was materialized',native='NOT_RUN'))
print('Single final freeze:8 Windows test sources/8 Compile entries;343 focused PASS;zero production changes',flush=True)
