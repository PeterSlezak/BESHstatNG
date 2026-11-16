from common47 import *
import re
lock();plan=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';before=plan.read_bytes();snapshot=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.23.md'
with snapshot.open('xb') as f:f.write(before)
text=before.decode('utf-8-sig');head='''
**Revision 2.24 · 16 November 2026 · 04A.46 safe software milestone accepted/merged; 04A.47 safe long-running GUI wiring in progress**
**Current owner authority:** Authenticated accepted46 merged main `'''+MAIN+'''`, direct child of `'''+PRIOR+'''`. Owner authorizes04A.47 SAFE SOFTWARE ONLY. All shipping handlers/designers/Core/Common remain unchanged; no deferred scientific/native activation. Live Excel/COM/UIA/RefEdit NOT_RUN; actual HWND/PID/loaded XLL NOT_VERIFIED; Windows/COM risk NOT_APPROVED; MayRunNative=False. AutoOpen/AutoUpdate conflict remains unresolved before any future launch. No original-control discharge or04A.48 authorization. All226 IDs and immutable2.12/2.23 chronology retained.

**Historical Revision2.23 entry (retained verbatim below):**
'''
text=text[:text.index('\n')+1]+head+text[text.index('\n')+1:];plan.write_bytes(text.encode())
ids=lambda t:list(dict.fromkeys(re.findall(r'^\|\s*\*\*(\d{2}A?\.\d{2}[A-Z]?)\*\*',t,re.M)))
assert ids(text)==ids(before.decode('utf-8-sig')) and len(ids(text))==226
save('planning/authority-v1.json',dict(entry=pin(snapshot),current=pin(plan),identities=ids(text),revision='2.24',original_charter='2.12',native_authorization=False,gates=GATES))
pins=runner().__class__.build.__globals__['source_pins']();entry=load(HERE/'entry/source-preimages-v1.json')['source_pins'];changed=[p for p in entry if pins.get(p)!=entry[p]];added=[p for p in pins if p not in entry]
assert changed==['BESHStatNG.Test/BESHStatNG.Tests.vbproj'] and len(added)==6 and all('/GuiWiring47' in p for p in added)
for r in load(HERE/'fixtures/projections-a-v2.json')['members']+load(HERE/'fixtures/projections-b-v2.json')['members']:
 exact(r['source']);body=r['original_text'];assert sha(body.encode())==r['source_text_sha256']
 assert body in (ROOT/r['source']['path']).read_text(encoding='utf-8-sig')
 for a,b in r['transforms']:body=body.replace(a,b)
 assert any(body in (ROOT/p).read_text() for p in added if 'Projection' in p),r['method']
save('matrix/source-freeze-v1.json',dict(kind='04A47_POST_WAVE_AB_SINGLE_SOURCE_TEST_FREEZE',utc=utc(),source_pins=pins,source_delta=[dict(before=dict(path=p,sha256=entry[p]),after=pin(p)) for p in changed],new_tests=[pin(p) for p in added],production_changed=[],plan=pin(plan),plan_snapshot=pin(snapshot),focused_final=pin(HERE/'waves/b5.json'),new_passing_identities=141,native='NOT_RUN',gates=GATES))
save('fixtures/projection-final-v1.json',dict(label='CURRENT_MAIN_PRE_EDIT_04A47_SOURCE_BACKED_FAKE',projections=[pin(ROOT/p) for p in added],source_manifests=[pin(HERE/'fixtures/projections-a-v2.json'),pin(HERE/'fixtures/projections-b-v2.json'),pin(HERE/'fixtures/integer-source-v1.json')],shipping_source_bodies_exact=True,live_native='NOT_RUN',scientific='NOT_GOLDEN'))
print('Source/test freeze:6 new Windows test sources;141 focused PASS;shipping bytes unchanged',flush=True)
