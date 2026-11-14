"""Final external additive authority; never edits a previous seal or dossier."""
import sys,json,hashlib
from pathlib import Path
W2=Path(__file__).resolve().parent
HERE=W2/'continuation-v15'
ROOT=W2.parents[3]
sys.path.insert(0,str(HERE))
from common import pin,sha

def load(path):return json.loads(path.read_bytes())
def create(path,data):
 with path.open('x',encoding='utf-8',newline='\n') as f:f.write(data)

indexpath=W2/'continuation-v15-review-parts-index-v2.json'
index=load(indexpath)
copy=load(W2/'continuation-v15-copied-review-reader-v3.json')
privatepath=W2/'continuation-v15-retained-private-preservation-v2.json'
private=load(privatepath)
verificationpath=W2/'continuation-v15-final-verification-v1.json'
verification=load(verificationpath)
gitpath=W2/'continuation-v15-final-git-preservation-v1.json'
git=load(gitpath)
addendumpath=W2/'continuation-v15-postseal-private-addendum-v1.json'
addendum=load(addendumpath)
generationpath=W2/'continuation-v15-private-seal-generation-v1.json'
generation=load(generationpath)
assert generation['status']=='ACTUAL_PRIVATE_SEAL_GENERATION_PASS'
assert copy['status']=='PASS' and copy['copied_files']==len(index['files'])
assert verification['status']=='ACTUAL_FINAL_PRIVATE_PHYSICAL_AND_LIVE_READERS_PASS_NOT_READY'
assert git['HEAD']==git['main']==git['merge_base']=='ba1636f5356ac761509c2cfa16fc66b92430935d'
assert git['branch']=='codex/04A-38' and git['staged']=='EMPTY' and git['diff_check_exit']==0
assert private['inherited_private_count']==15166 and private['inherited_physical_archives']==85
freeze=load(HERE/'production-test-freeze-v3.json')
for row in freeze['sources']:assert pin(ROOT/row['path'])==row
for row in addendum['postseal_private_pins']:assert pin(ROOT/row['path'])==row
for row in generation['raw_private_pins']:assert pin(ROOT/row['path'])==row
transcription=load(W2/'continuation-v15-publication-failed-attempt-addendum-v3.json')['transcript']
assert pin(ROOT/transcription['path'])==transcription
all_postseal=addendum['postseal_private_pins']+generation['raw_private_pins']+[transcription]
assert len(all_postseal)==len({r['path'] for r in all_postseal})
extended=dict(status='ADDITIVE_EXPLICIT_POSTSEAL_AND_GENERATION_RAW_PINS_NO_PRIOR_REWRITE',base=pin(privatepath),previous_addendum=pin(addendumpath),generation=pin(generationpath),postseal_private_pins=all_postseal,count=len(all_postseal),tool_response_transcription=transcription,original_queries=0,scope='Exact unchanged v1 postseal raw pins plus three actual private-seal generation command streams and one explicitly labelled conversation tool-response transcription. The transcription is not an original Runner stderr file. No historical tree traversal.')
addendumpath=W2/'continuation-v15-postseal-private-addendum-v2.json'
create(addendumpath,json.dumps(extended,indent=2)+'\n')
addendum=extended
parts='\n'.join('| ['+p['name']+'](../../../../'+p['path']+') | '+str(p['bytes'])+' | `'+p['sha256']+'` |' for p in index['parts'])
queue=load(HERE/'remaining-work-queue-v2.json')
nextrow=queue['first_members'][0]
handover=f'''# Continuation v15 final additive authority and handover

NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED.

This is a partial W2 checkpoint, not Phase04A.38 acceptance or authority for W3/04A.39. This additive handover takes precedence over earlier v15 publication instructions and ambiguous nonpass descriptions only as stated below. Every previous seal, failed attempt, archive, original/reference snapshot and source identity remains preserved.

Historical supported537/704 (167OPEN); current supported566/726 (160OPEN); distinct support supported262/303 (41OPEN). NEW41/37/6; all prior496/529/256 supported identities remain supported, including the four C14 ROC bodies. Final ownership is historical-ownership-v2.json, current-ownership-v3.json and the canonical ledgers; the exact167/160/41 ordered OPEN complements and next24 are remaining-work-queue-v2.json. Its first next identity is `{nextrow['original_id']}`. No later queue work has begun.

The original selected24 Cox/GEE identities and coherent dependencies have full body, fragment, callback, constructed property, source movement and actual Debug/Release CIL ownership reconciliation. Four owner-distinct GEE properties have authentic Common counterparts without invented current Windows IDs. Core scientific engines are unchanged. Native import, caches/handles, Excel error materialization, workbook and exception boundaries remain host-owned. GuessPositiveClassAfterConversion alias behavior and the old resetting Common contract remain preserved. See [owner review](continuation-v15/owner-review.md), [immutable proof seal](continuation-v15/review-seal-v1.json), and the actual sealed checkpoint reader.

## Actual final validation

- Managed final Debug:20,990PASS /14 inheritedFAIL among21,004 identities,64 newPASS; every prior identity/outcome/nonpass message is reconciled. The first pre-correction campaign is immutable NONFINAL. Exactly one owner-approved replacement Debug campaign was used; no third campaign occurred.
- Debug/Release offline compilation succeeded. Each configuration passed all29,424 current-only frozen comparisons:28,966 genuine pre-edit plus458 separately approved A-G late-reference cases. No archived original implementation was executed/recompiled and no new oracle was captured. Public268 ABI/attributes/help/defaults and complete compiled signatures/CIL were checked in both configurations. Full and selected Release unit suites are NOT_RUN_OWNER_PREFERENCE.
- Python effective956 parent identities:830PASS /113FAIL /13ERROR; subtest events182FAIL /13ERROR. The interruption left14 known starts,13 emitted outcomes and one unknown in-flight outcome.916 completed parents were retained. The owner-approved replacement40-parent lane and separately approved two focused privacy parents were each consumed exactly once. Actual history is972 known starts,971 completed outcomes including13 partial emitted headers, one unknown. This is not an exactly-once execution claim after the explicitly approved interruption recovery.
- The two current focused privacy outcomes are original-scanner UnicodeDecodeError ERRORs on the retained C14 binary Git index snapshot. The earlier accelerator translated the helper exception into two AssertionError FAILs; those raw FAILs remain preserved. The earlier C14 JSON ERROR traces remain immutable historical outcomes. The sentence in owner-review.md referring to “both privacy JSON errors” must not be read as the current exception type. No ERROR became PASS and no assertion was waived. The separate unordered-SPC ERROR remains recorded. The focused correction reused4,105 authenticated per-file rows and made five fresh per-file evaluations, without another bulk batch/helper job; the unchanged sequential scanner reached the uncached binary error in original order.
- Node:916PASS (468Office /71resolver /377WASM-node). Strict/profile/MkDocs actual exits1/1/0. No further test campaign was run during publication.

## Final publication authority and preserved failures

Use [final v2 parts index](continuation-v15-review-parts-index-v2.json), publication-receipt-v2.json, continuation-v15/verify_c15_publication_v3.py, and continuation-v15/copy_c15_review_union_v3.py. The original v1 index/seven ZIPs remain immutable. The first copy helper failed on a Windows path-length limit before its Runner call; its process traceback was returned by the tool and its partial extraction is retained, but no durable Runner raw stream was produced for that initial extraction failure. A separately labelled transcription of that observed tool response is preserved and pinned in the v3 publication failed-attempt addendum; it is not represented as an original Runner stderr file. The shorter-path v2 helper extracted all9,724 original members, then failed because run.py imported the absent scripts/test_suite_rationalization.py. Its actual command/stdout/stderr and failed receipt remain preserved.

The additive eighth dependency ZIP supplies that unchanged runtime source, versioned tooling, v2 logical manifest/inventory and failed-attempt records. All9,724 original members and all seven original archive hashes remain exact; the final reader asserts this prior union. No prior reader/seal/assertion was weakened or overwritten. The final fresh short-path .git-free extraction contained{len(index['files']):,} unique normalized members. Its actual final checkpoint/publication/preservation readers passed with isolated copied imports: [copied-reader receipt](continuation-v15-copied-review-reader-v3.json). Standalone production validation was NOT_CREATED.

All eight archives are independent complementary ZIPs below180MiB, not .zip.001 binary volumes. Extract them into one fresh short destination. Unique path/size/SHA-256/member CRC, physical archive SHA-256 and deterministic second serializations were checked against actual local bytes. No external upload or independent reviewer execution is claimed; independently checking an uploaded archive requires those actual uploaded bytes.

| Physical archive | Bytes | SHA-256 |
| --- | ---: | --- |
{parts}

## Actual preservation and Git exit gates

Final versioned private reader is continuation-v15-retained-private-reader-v3.py, bound to [private preservation v2](continuation-v15-retained-private-preservation-v2.json). It preserves all15,166 inherited pins/85 inherited physical archives and adds bounded current evidence: total{private['private_pin_count']:,} private pins and{len(private['retained_prior_physical_archives'])} physical archives, plus{addendum['count']} explicitly pinned postseal evidence files (actual raw streams and one labelled tool-response transcription) in the additive postseal private receipt. Review copies are archive-derived evidence, not substitutes for raw private records. No ignored historical TestResults tree was walked or deleted. The4,213,129,746-byte malformed v9 aggregation remains SHA-256 `3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb`.

Actual private replay, physical eight-part reader and live preservation reader passed after publication. All41 original UDF snapshots,278 lineage pins,100 mixed-model captures and16 owner documents remain protected. Corrected final C14 v2 authority and actual final C14 readers/C13 private chain were authenticated; the C14 v1 index/failed .git/index attempts remain preserved. Master Plan2.14 is unchanged, SHA-256 `0ee45eae70aa35d6c87c636d82c3f33e105b8c12e6e5a4badb69e38fe7ad69dd`; historical2.13 remains `436b66c4307bd1571649c2604f8d90b5f0b5d843df3567b918698392fe5d6e7c` under its exact authorized projection.

Branch codex/04A-38; HEAD/main/merge-base all `ba1636f5356ac761509c2cfa16fc66b92430935d`. Index remains empty and unchanged:620,185bytes /`2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c`. git diff --check exit0. Dirty unstaged worktree is retained. No stage/commit/reset/merge/rebase/push occurred. Actual final command provenance is [final verification](continuation-v15-final-verification-v1.json), [Git/index receipt](continuation-v15-final-git-preservation-v1.json) and [postseal raw-pin addendum](continuation-v15-postseal-private-addendum-v2.json). The immutable v1 raw-pin addendum is retained and exactly extended by v2 to include the three actual private-seal generation streams and the separately identified tool-response transcription.

No scientific/native/manual/R/Excel/COM/UIA/RefEdit, production Office/Mac/WASM/browser gate, restore/install, production packaging, signing or deployment validation was performed. General verification wrappers and manual smoke checks remain NOT_RUN_OWNER_PROHIBITION because they would repeat campaigns or enter prohibited workflows. No next batch was started. Final terminal state remains exactly as printed at the beginning of this handover.
'''
handoverpath=W2/'continuation-v15-postseal-handover-addendum-v1.md'
create(handoverpath,handover)
artifacts=[pin(p) for p in sorted(W2.glob('continuation-v15-*')) if p.is_file() and p.suffix in ['.py','.json','.md']]
for name in ['verify_c15_publication_v3.py','postseal_verify_c15_v2.py','copy_c15_review_union_v3.py','publish_dependency_supplement_v1.py','REVIEW-ARCHIVES-addendum-v2.md','publication-manifest-v2.json','file-inventory-v2.json']:
 artifacts.append(pin(HERE/name))
doc=dict(status='FINAL_ADDITIVE_POST_PUBLICATION_ARTIFACTS_BOUND_NOT_READY',handover=pin(handoverpath),parts_index=pin(indexpath),proof_seal=pin(HERE/'review-seal-v1.json'),final_verification=pin(verificationpath),private=pin(privatepath),postseal_private=pin(addendumpath),artifacts=artifacts,physical_archives=index['parts'],production_test_sources_unchanged=len(freeze['sources']),original_queries=0,terminal='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED',self_entry='Nonrecursive: this receipt is verified by its actual printed SHA-256, not a manufactured self-hash.')
target=W2/'continuation-v15-post-publication-artifacts-v1.json'
create(target,json.dumps(doc,indent=2,ensure_ascii=False)+'\n')
for row in artifacts:assert pin(ROOT/row['path'])==row
print(json.dumps(dict(status='FINAL_DOSSIER_PINS_AND_SOURCE_FREEZE_PASS_NOT_READY',handover=pin(handoverpath),artifacts=pin(target),private_pins=private['private_pin_count'],physical_archives=len(private['retained_prior_physical_archives']),review_parts=len(index['parts']),review_files=len(index['files']),original_queries=0)),flush=True)
