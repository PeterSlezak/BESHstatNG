"""Final additive owner dossier and incremental privacy verification after immutable ZIP."""
from common40_v3 import *
import zipfile,collections
sys.path.insert(0,str(ROOT/'evidence/04A39/physical-witness-v1'));import physical
lock();published=load(HERE/'publication/physical-readback-v1.json');archive=published['archive'];assert pin(archive['path'])==archive
repo=load(HERE/'python/repository-privacy-witness-v2.json');assert repo['distinct_rule_occurrences']==8
counts=collections.Counter(r['source']['path'] for r in repo['findings']);assert counts=={'evidence/04A40/baseline/Debug-build-v1.json':6,'evidence/04A40/entry/tool-runtime-final-v1.json':2}
with zipfile.ZipFile(ROOT/archive['path']) as z:
    safe=z.read('evidence/04A40/entry/tool-runtime-final-v1.json');assert safe==(PRIVATE/'review-tool-runtime-v2.json').read_bytes()
    (HERE/'entry/tool-runtime-review-v2.json').open('xb').write(safe)
ready=load(HERE/'final/readiness-v2.json');ready.update(publication='PHYSICAL_DERIVATIVE_PUBLICATION_PASS',publication_receipt=pin(HERE/'publication/physical-readback-v1.json'),
    repository_current_rule_occurrences=8,repository_snapshot=pin(HERE/'python/repository-privacy-witness-v2.json'),Python_parent_privacy_snapshot='Six occurrences per privacy parent at full-campaign freeze; later focused candidate scan has eight. No parent rerun or historical result replacement.')
save('final/readiness-postseal-v3.json',ready)
text=f'''# Final owner review — NOT_READY_04A40

The behavior-preserving implementation is verified. Eight typed Common fit/request helpers serve nine Windows UDF parents;15 production files changed and two new test files add12 passing managed identities. Formula/effect, MMRM and accepted result policies were reused. No Core algorithm, existing test source, external UDF behavior, native activation or historical oracle changed.

| Final corrected-state check | Actual outcome |
|---|---|
| Debug / Release compilation | Both nine-step graphs exit0 |
| Full Debug |21,125 PASS /14 inherited FAIL;all21,127 existing identities retained;all14 messages/traces exact39 |
| Targeted Release |3,108 PASS /2 inherited FAIL;full Release NOT_RUN |
| Excel-DNA exports |268 exact per configuration |
| Frozen compatibility |29,424 exact per configuration:28,966 PRE_EDIT_FROZEN +458 approved LATE_ARCHIVED_ENTRY_SUPPLEMENT |
|40 current-main fixtures /39 fixtures |48 statistical/input/exception contracts exact and77 fully exact per configuration;four measured GEE elapsed fields disclosed |
| Python |830 PASS /115 FAIL /11 ERROR over956 parents;187 FAIL +11 ERROR events |
| Node |916 PASS, exact identities;retained unchanged JS inputs across two VB correction files |
| Strict/profile/MkDocs |1/1/0;nonzero results remain nonpasses |
| Publication |Both original physical scanners:zero unresolved;399 members,398 indexed;SHA/size/CRC and standalone typed-case reader PASS |

The ZIP is **{archive['bytes']} bytes**, SHA-256 **`{archive['sha256']}`**. It includes exact original/current synthetic compatibility TSVs, frozen inputs and recorders, source changes/preimages, method/CIL indices, retained268 exports, canonical register and deterministic review readers. Raw managed/Python diagnostics, owner tokens, build binaries, index bytes and ignored historical TestResults are excluded. The full raw CIL metadata remains owner-local; complete per-method hashes and changed records are supplied. Independent native/scientific execution is not implied.

The publication gate first stopped before ZIP creation on five matches. The corrected derivative redacts one noncanonical source-excerpt cell in the historical symbol ledger and formats aliased tool paths safely. All symbol IDs, row order and759 referenced signatures are exact; the original ledger is unchanged. No synthetic exception was added and neither original scanner was changed. The unsafe staging attempt is retained locally.

The repository candidate remains a **FAIL**, separate from the verified physical derivative. A focused final scan reports eight unique rule occurrences, shared by both scanners: six owner-path occurrences in the preserved entry build receipt, plus two UNC-pattern occurrences in tool metadata. The full Python campaign's privacy-parent snapshot remains six findings each; it predates the new tool receipt. These are distinct input/encoding snapshots, not edited results. The tool-path rendering is a reporting change, not changed test behavior. Safe counterparts are available; originals are preserved. No original repository-candidate rule or scope is narrowed.

The final32 diagnostic differences are source/context explained, with exact before/current fingerprints and assertion-argument proofs. All54 earlier differences remain unchanged and unwaived. Three historical UDF-diff guards reexpose FAIL under the explicitly authorized40 edits. Both historical39 privacy FAIL parents retain1941 findings each(1599 earlier +342 later), and W3's two privacy ERRORs remain unwaived.

The192-member39 input-policy map is retained;the200-member40 regression/survival inventory is a different scope. Twenty canonical issue investigations and the40 delta preserve1169 entries,1514 original obligations,802 restricted controls,2967 mappings and33 source pins with authorized transitions. No obligation/control is discharged. C08 labels/reference formatting,C10/C11 alpha/covariance/native patches,C12 supplied-start QIC/cluster/prediction/weight/AR(1),C13 earliest tied censor/event risk sets and mixed-model inference remain restricted. No exact C13 correction authorization/fresh independent R gate was established, so no inference change was activated.

Preservation:468 original checkpoint files/copies and8984 listed protected paths verified;four already-committed owner-authorized newline renderings have exact originals retained. Continuous precommit file IDs are NOT_VERIFIED and old ID receipts are unchanged. All1353 source/test/plan entry identities reconcile(1337 unchanged,15 production transitions plus plan);two new tests added. The37484/154 W3 union remains inherited qualified. The source/shipping/Git/archive locks were checked again after publication.

Branch `codex/04A-40`;HEAD/main/merge-base `{MAIN}`;accepted39 implementation `{load(HERE/'entry/authentication-v3.json')['merge39']}`. The owner BuildDate commit is preserved. Staging is empty,index bytes match entry,and `git diff --check` exits0. No commit,merge,rebase,push,release,installer,signing,live Excel/COM/UIA,Office/Mac/browser or fresh R execution occurred. All226 plan identities remain in Revision2.17 with prior2.15/2.16 snapshots preserved.

**Recommendation: do not accept04A.40 yet.** The smallest decision is an exact versioned Git-candidate representation repair for the two new reporting artifacts, retaining their protected originals and failed snapshots. The entry receipt's private output path must not be committed. Safe counterparts are `baseline/Debug-build-review-v2.json` and `entry/tool-runtime-review-v2.json`. Disposition the historical diff-scope guard applicability without calling their FAILs green. No broad code/scientific/native approval is requested.

See `owner-review-v1.md` for implementation detail,`readiness-postseal-v3.json` for the final gate,`../python/diagnostic-dispositions-v3.json` for all32 current changes,`../register/issue-delta-v4.json` for issue decisions,and the external final delivery receipt for ZIP/readback/source/Git binding. Use the standalone `review_readback.py` inside the ZIP for transport/case checks. A read-only local verification is `module('active40',HERE/'verify40-v3.py').verify(module('active40',HERE/'verify40-v3.py').documents(),live=True)` via the supplied common helper;do not rerun the append-only report writer CLI over existing reports.

`P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A41_NOT_AUTHORIZED`
'''
(HERE/'final/owner-review-v2.md').open('x',encoding='utf-8',newline='\n').write(text)
assert not git('diff','--check') and not git('diff','--cached','--name-only')
prior=load(HERE/'final/git-postpublication-v2.json');assert pin('.git/index')==prior['index']
save('final/git-final-v3.json',dict(prior,utc=utc(),untracked=git('ls-files','--others','--exclude-standard').decode().splitlines(),modified=git('diff','--name-only').decode().splitlines(),
    HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),staged_empty=True,diff_check_exit=0))
save('publication/delivery-final-v2.json',dict(archive=archive,physical_members=399,indexed=398,publication='PHYSICAL_DERIVATIVE_PUBLICATION_PASS',
    overall='NOT_READY_04A40',owner_review=pin(HERE/'final/owner-review-v2.md'),readiness=pin(HERE/'final/readiness-postseal-v3.json'),
    physical_scan=pin(HERE/'publication/physical-readback-v1.json'),Git=pin(HERE/'final/git-final-v3.json'),source_preservation=pin(HERE/'preservation/postpublication-lock-v1.json'),
    final_repository_privacy=pin(HERE/'python/repository-privacy-witness-v2.json'),accepted=False,committed=False,merged=False,next_phase_authorized=False,gates=GATES))
# Read-back and scan only newly appended artifacts; all previous full-scope candidate
# bytes are independently rehashed, so no earlier assertions or findings disappear.
results=[]
for phase in ['13','14']:
    old=load(PRIVATE/('repository-privacy-'+phase+'-v2.json'));m=physical.scanner(phase)
    assert pin(m.__file__)==old['scanner']
    previous={r['path']:r for r in old['candidate_inputs']}
    for r in previous.values():assert pin(r['path'])==r,'PRIOR_CANDIDATE_CHANGED_AFTER_FULL_SCAN'
    candidates=[p for p in m.paths() if (ROOT/p).is_file()];added=sorted(set(candidates)-set(previous));found,reviewed=m.scan(added)
    assert not found,'NEW_POSTSEAL_ARTIFACT_PRIVACY_FINDING'
    results.append(dict(scanner=phase,prior_full_scope_unchanged=len(previous),added_scanned=added,added_findings=0,prior_findings=len(old['findings']),combined_findings=len(old['findings']),original_scope_unchanged=True))
save('publication/postseal-new-artifact-privacy-v1.json',dict(results=results,combined_current_repo_findings=8,repository_status='FAIL_UNWAIVED',publication='PHYSICAL_DERIVATIVE_PUBLICATION_PASS',
    context='Same owner checkout/process environment; original source/guard/Git anchors unchanged. Prior candidate bytes rehashed; every new path scanned with unchanged originals. This is incremental evidence verification, not another956-parent campaign.'))
lock();print('Final postseal owner dossier NOT_READY; physical ZIP PASS; repository8 findings unwaived; new metadata privacy0',flush=True)
