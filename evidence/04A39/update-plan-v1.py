"""Preserve exact Revision2.14 and update the living status with verified merge facts."""
from common39 import *
source_lock()
path=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'
before=pin(path);old=path.read_bytes()
historical=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.14.md'
with historical.open('xb') as stream:stream.write(old)
s=old.decode('utf-8-sig')
def replace(a,b):
    global s
    assert s.count(a)==1,(a,s.count(a));s=s.replace(a,b)
replace('**Revision 2.14 · 12 November 2026 · Phase 04A.38 W2-v12 status refresh**','**Revision 2.15 · 14 November 2026 · Accepted/merged04A.38;04A.39 in progress**')
start=s.index('**Current owner-reported state:**');end=s.index('\n\n| Roadmap measure',start)
s=s[:start]+('**Current state:** Official batches **00.01–04.07 and04A.01–04A.38 accepted**. Owner accepted04A.38 W2 on13 November2026, qualified W3 software and overall04A.38 on14 November2026. '+
    '**04A.38 merged main independently verified at `'+MAIN+'`**, direct child of W3 baseline `ba1636f5356ac761509c2cfa16fc66b92430935d`. '+
    'All1350 source paths exist in that commit;1113 exact Git byte hashes and237 owner-authorized newline-only differences (live1311 exact/39 newline-only). '+
    '**04A.39 IN_PROGRESS**, on `codex/04A-39`, not accepted. No04A.40 authority follows. The exact previous living plan is retained as `BESHStatNG_CrossPlatform_MasterPlan2.14.md`.')+s[end:]
replace('| Accepted (Phases 00–04: 40; Phase 04A: 37) | **77** |','| Accepted (Phases 00–04: 40; Phase 04A: 38) | **78** |')
replace('| Not yet accepted (including in-progress 04A.38) | **149** |','| Not yet accepted (including in-progress 04A.39) | **148** |')
replace('| Phase 04A accepted / total | **37 / 75** |','| Phase 04A accepted / total | **38 / 75** |')
replace('| Phase 04A not yet accepted | **38** |','| Phase 04A not yet accepted | **37** |')
replace('**Verification boundary.**','**Historical Revision2.14 verification boundary (superseded by current entry above; preserved for chronology).**')
replace('NOT_GOLDEN; 04A39_BLOCKED','NOT_GOLDEN; 04A39_IN_PROGRESS')
replace('or Phase 04A.39 work follow from software-extraction progress. Phase 04A.38 requires completed W2, separately undertaken W3, independent readiness review and explicit owner acceptance/merge before downstream work.',
        'follow from software-extraction progress. The04A.39 prerequisite is satisfied by the separate owner acceptances and verified merge above; native/scientific gates remain unchanged.')
replace('### 04A.38 — latest partial W2 checkpoint (v12; not accepted)','### 04A.38 — retained historical v12 status (superseded; exact Revision2.14 preserved)')
replace('**Immediate work: 04A.38 continuation-v13 (W2 only).**','**Historical Revision2.14 immediate work:04A.38 continuation-v13 (W2 only; superseded).**')
replace('**04A.01–04A.37 accepted/merged by owner report; 04A.38 is W2_IN_PROGRESS / NOT_READY (v12 checkpoint); W3 NOT_STARTED; 04A.39 BLOCKED; 04A.39–04A.75 remain planned.**',
        '**04A.01–04A.38 owner-accepted;04A.38 merge independently verified;04A.39 IN_PROGRESS and not accepted;04A.40–04A.75 remain planned.**')
oldrow=next(x for x in s.splitlines() if x.startswith('| **04A.38**'))
replace(oldrow,'| **04A.38** **✓** | Accepted controlled extraction across268 Excel-DNA UDF exports; Windows registration/cache/native session ownership retained. | 04A.01,01.09 | W2 accepted13Nov; W3 software/overall accepted14Nov; verified main `'+MAIN+'`.704/726/303 closed,zeroOPEN; Debug/Release21080PASS/14 inheritedFAIL each;29424 exact/config. Native/scientific restrictions retained. | High |')
replace('| **04A.39** |','| **04A.39** **IN PROGRESS** |')
s+='\n**Revision2.15 change log (14November2026):** Records owner W2/W3/overall04A.38 acceptances, independently verified merged-main SHA and explicit newline reconciliation; starts04A.39 audit/fixtures/canonical issue register only. Preserves exact Revision2.14, original226 roadmap identities and04A.39–48 charter. New04A.39 evidence is an implementation/review candidate, never owner acceptance.\n'
path.write_text(s,encoding='utf-8',newline='\n')
save('entry/master-plan-crosswalk-v1.json',dict(before=before,historical=pin(historical),after=pin(path),main=MAIN,
    revision='2.15',original_roadmap_id_rows_preserved=True,accepted_batches=78,total=226,
    note='Historical v12 prose explicitly labelled superseded; immutable W2/W3 authority untouched.'))
