from common40_v3 import *
lock();path=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';original=path.read_bytes()
expected=next(r for r in load(HERE/'entry/source-preimages-v3.json')['files'] if r['path']==path.relative_to(ROOT).as_posix())
assert pin(path)==expected
snapshot=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.15.md'
with snapshot.open('xb') as f:f.write(original)
text=original.decode('utf-8-sig')
edits={
 '**Revision 2.15 · 14 November 2026 · Accepted/merged04A.38;04A.39 in progress**':'**Revision 2.16 · 14 November 2026 · 04A.39 accepted/merged;04A.40 in progress**',
 '**Current state:**':'**Current owner authority (04A.40 entry):** Owner accepted04A.39 and its limited physical-derivative publication gate, then committed/merged39 at `'+MERGE39+'`. Updated main `'+MAIN+'` is its direct child, containing only the previously observed BuildDate metadata update. Actual39 source, canonical register and final witness are authenticated;1352 historical source pins remain exact and the one owner BuildDate transition is explicitly recorded. **04A.40 IN_PROGRESS**, not accepted;04A.41 is unauthorized. Repository privacy FAILs, W3 ERRORs and scientific/native gates remain unwaived. Exact Revision2.15 is retained separately.\n\n**Historical Revision2.15 state (retained for chronology):**',
 '| Accepted (Phases 00–04: 40; Phase 04A: 38) | **78** |':'| Accepted (Phases 00–04: 40; Phase 04A: 39) | **79** |',
 '| Not yet accepted (including in-progress 04A.39) | **148** |':'| Not yet accepted (including in-progress 04A.40) | **147** |',
 '| Phase 04A accepted / total | **38 / 75** |':'| Phase 04A accepted / total | **39 / 75** |',
 '| Phase 04A not yet accepted | **37** |':'| Phase 04A not yet accepted | **36** |',
 '| **04A.39** **IN PROGRESS** |':'| **04A.39** **ACCEPTED / MERGED** |',
 '| **04A.40** |':'| **04A.40** **IN PROGRESS** |'
}
for before,after in edits.items():
    assert text.count(before)==1,(before,text.count(before));text=text.replace(before,after)
with path.open('w',encoding='utf-8',newline='\n') as f:f.write(text)
save('planning/reconciliation-v1.json',dict(before=expected,historical_copy=pin(snapshot),after=pin(path),source_of_acceptance='Owner40 instruction plus actual committed source ancestry; not self-acceptance.',
    original_plan_revision='2.15_STALE_39_STATUS',current_revision='2.16_40_IN_PROGRESS',batch_count=226,scientific_charter_unchanged=True,gates=GATES))
