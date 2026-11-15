from common41 import *
import re
lock();path=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';original=path.read_bytes()
assert pin(path)==load(HERE/'entry/source-preimages-v1.json')['plan']
snapshot=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.17.md'
with snapshot.open('xb') as f:f.write(original)
text=original.decode('utf-8-sig')
lines=text.splitlines();assert 'Revision 2.17' in lines[2]
lines[2]='**Revision 2.18 · 15 November 2026 · 04A.40 owner-accepted/merged; 04A.41 in progress**'
text='\n'.join(lines)+'\n'
authority='**Current owner authority (04A.41 entry):** The owner explicitly accepted04A.40 with its documented restrictions and authorized04A.41 only. Actual clean merged main `'+MAIN+'` is the direct child of `'+PRIOR+'`; the accepted40 final witness, exact raw preservation boundary and sanitized successors are authenticated. **04A.41 IN_PROGRESS**, not accepted or merged; **04A.42_NOT_AUTHORIZED**. No scientific/native correction, historical waiver, staging, commit, merge or release authority follows. The original226 batch identities and deferred-defect charter remain binding. Exact Revision2.17 is retained separately.\n\n**Historical Revision2.17 entry (retained for chronology):** '
assert text.count('**Current owner authority (04A.40 entry):**')==1
text=text.replace('**Current owner authority (04A.40 entry):**',authority)
edits={
 '| Accepted (Phases 00–04: 40; Phase 04A: 39) | **79** |':'| Accepted (Phases 00–04: 40; Phase 04A: 40) | **80** |',
 '| Not yet accepted (including in-progress 04A.40) | **147** |':'| Not yet accepted (including in-progress 04A.41) | **146** |',
 '| Phase 04A accepted / total | **39 / 75** |':'| Phase 04A accepted / total | **40 / 75** |',
 '| Phase 04A not yet accepted | **36** |':'| Phase 04A not yet accepted | **35** |',
 '| **04A.40** **IN PROGRESS** |':'| **04A.40** **ACCEPTED / MERGED** |',
 '| **04A.41** |':'| **04A.41** **IN PROGRESS** |',
 '**04A.01–04A.39 owner-accepted;04A.39 merge independently verified at `f2eb060c9c422c1fec4e82989d07907dbd4b9337`;04A.40 IN_PROGRESS and not accepted;04A.41–04A.75 remain planned.**':'**04A.01–04A.40 owner-accepted;04A.40 merged main independently verified at `'+MAIN+'`;04A.41 IN_PROGRESS and not accepted;04A.42–04A.75 remain planned.**'
}
for before,after in edits.items():
    assert text.count(before)==1,'PLAN_REPLACEMENT_COUNT'
    text=text.replace(before,after)
beforeids=list(dict.fromkeys(re.findall(r'^\|\s*\*\*(\d{2}A?\.\d{2}[A-Z]?)\*\*',original.decode('utf-8-sig'),re.M)))
afterids=list(dict.fromkeys(re.findall(r'^\|\s*\*\*(\d{2}A?\.\d{2}[A-Z]?)\*\*',text,re.M)))
assert beforeids==afterids and len(afterids)==226
text+='\n**Revision2.18 change log (15 November2026):** Records explicit owner acceptance and actual merged main for04A.40, starts only04A.41 behavior-preserving basic/assumptions/contingency/agreement/sizing UDF reconciliation. Retains exact2.17, all226 IDs, original charter and all scientific/native/privacy failure history.04A.41 has not been owner-accepted or merged.\n'
with path.open('w',encoding='utf-8',newline='\n') as f:f.write(text)
save('planning/reconciliation-v1.json',dict(before=load(HERE/'entry/source-preimages-v1.json')['plan'],snapshot=pin(snapshot),after=pin(path),revision='2.18',identities=afterids,source='Current explicit owner acceptance/41 authority and authenticated actual Git ancestry; not self-acceptance.',native_scientific_charter_unchanged=True,gates=GATES))
print('Plan2.18: all226 IDs exact;40 accepted,41 in progress,42 unauthorized',flush=True)
