"""Bind the concrete final v16 review, live audits and final Git state additively."""
import sys,json,datetime
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent/'continuation-v16'))
from common import *
load=lambda p:json.loads(Path(p).read_bytes())
def external(name,doc):
 p=W2/name
 with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(doc,f,indent=2,ensure_ascii=False);f.write('\n')
 return pin(p)
indexpath=W2/'continuation-v16-review-parts-index-v3.json';index=load(indexpath)
copyreceipt=load(W2/'continuation-v16-copied-review-reader-v2.json');assert copyreceipt['status']=='PASS'
physical=load(W2/'continuation-v16-physical-review-verification-v2.json');assert physical['result']['parts']==len(index['parts'])
private=load(W2/'continuation-v16-private-reader-verification-v1.json');assert private['status']=='ACTUAL_PRIVATE_FILE_IDENTITIES_BYTES_AND_PHYSICAL_ARCHIVE_HASHES_PASS'
live=load(W2/'continuation-v16-live-preservation-v1.json');assert live['result']['Git']=='LIVE_PASS'
checkpoint=load(HERE/'checkpoint-verification-v3.json');assert checkpoint['result']['historical_supported']==607
for row in load(HERE/'production-test-freeze-v3.json')['sources']:assert pin(ROOT/row['path'])==row
for part in index['parts']:assert 0<part['bytes']<180*1024**2
failed=[]
for folder in sorted((RUNROOT/'commands').glob('w2c16-*')):
 p=folder/'command.json'
 if p.is_file() and load(p).get('exit_code',0):failed.append(dict(label=folder.name,command=pin(p),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log')))
external('continuation-v16-final-failed-attempts-v1.json',dict(status='ALL_REAL_NONZERO_COMMAND_RECEIPTS_RETAINED_NO_RELABEL',rows=failed,original_source_and_test_campaigns_changed=False,original_queries=0))
raw=[]
for folder in sorted((RUNROOT/'commands').glob('w2c16-postseal-*')):
 if not (folder/'command.json').is_file():continue
 raw.extend(pin(p) for p in sorted(folder.rglob('*')) if p.is_file())
external('continuation-v16-postseal-private-addendum-v1.json',dict(status='ADDITIVE_COMPLETED_POSTSEAL_RAW_PINS',base=pin(W2/'continuation-v16-retained-private-preservation-v2.json'),postseal_private_pins=raw,count=len(raw),scope='Completed current postseal command roots only. The enclosing finalizer receipt is bound after completion by the final additive artifacts record. No historical TestResults traversal.',original_queries=0))
parts='\n'.join('- `'+p['name']+'`: '+str(p['bytes'])+' bytes; SHA-256 `'+p['sha256']+'`.' for p in index['parts'])
handover=f'''# Final additive v16 handover — canonical authority

This is the final additive authority for the v16 partial W2 checkpoint. It supersedes nonfinal v16 review/seal/publication dispositions where explicitly corrected. Every earlier file, failed attempt, stopped manifest, reader result, ZIP and seal remains immutable.

Canonical review: [final/owner-review.md](continuation-v16/final/owner-review.md), also preserved as [owner-review-v2.md](continuation-v16/owner-review-v2.md). The initially sealed top-level `owner-review.md` remains a nonfinal artifact; its bytes were not rewritten. Canonical checkpoint: `verify_c16_checkpoint_v3.py` / `review-seal-v3.json`. Canonical historical/identity/fragment/queue ledgers use version2; current ownership remains version1. Canonical publication: manifest3, inventory3, parts index3, publication receipt3, publication reader4 and safe copy reader4. The actual fresh copied-reader receipt is version2. Older index2/eight ZIPs and its positive but nonfinal copy/physical receipts remain preserved.

## Exact outcome

Historical **607/704 supported, 97 OPEN**; current **635/726 supported, 91 OPEN**; distinct support **271/303 supported, 32 OPEN**. New closures **70 historical / 69 current / 9 support**; all prior **537/566/262** retained. **57/58** selected targets close. `LMM_FIT` and its existing `NormalizeMixedModelResidualCovarianceName` conversion dependency remain OPEN. Two additional separately owned original-only character lambdas received exact source, actual typed delegate and existing Debug CIL proofs after the parent/nested-identity audit. No source identity was invented or merged. Original fragment coverage is **16,465/20,683**; current coverage is **9,973/13,695**. Exact remaining ordered complements and next24 are in `remaining-work-queue-v2.json`. No MMRM packet was started.

Both substantive implementation packets completed before the combined production/test freeze3. No production/test source changed afterward. Debug offline compilation, actual shipping Debug signatures/CIL/268 exports and **29,424 exact** current-only frozen comparisons are complete. **28,966 PRE_EDIT_FROZEN** and **458 LATE_ARCHIVED_ENTRY_SUPPLEMENT** cases retain their original, separate provenance. No archived original execution or new oracle occurred.

Exactly one comprehensive managed Debug campaign: **21,039 PASS / 14 inherited FAIL among 21,053**, with **49 new PASS** and all **21,004 inherited identities/failure messages** preserved. Exactly one ordinary Python campaign: **956 parent starts/completions once**, **830 PASS / 113 FAIL / 13 ERROR**; subtest events **183 FAIL / 13 ERROR**, including the additional GLMNB source-pin failure in an already failing parent. Unicode privacy errors, earlier JSON/AssertionError chronology and separate unordered-SPC ERROR remain unwaived. Node once: **916 PASS**. Strict/profile/MkDocs once: **1/1/0**. Original assertions were not weakened; no replacement campaign was run. General verification wrappers were not run because the explicit v16 workflow required controlled offline Debug gates and prohibited repeat/Release/native campaigns.

## Explicit intermediate Release deferral

| Category | v16 status |
|---|---|
| Release compilation | {DEFERRED} |
| Release full/selected/focused unit tests | {DEFERRED} |
| Release ABI/CIL/signatures/export verification | {DEFERRED} |
| Release frozen compatibility | {DEFERRED} |

Prior Release bytes remain protected and are not fresh validation of this source freeze. Separately authorized finalization must restore fresh comprehensive Debug and Release gates. W3/finalization/04A.39 were not launched.

## Real-byte preservation and review transport

The final live private audit checked **{private['private_pin_count']} file pins / {private['physical_archives']} physical archives**, retaining all **21,208 inherited private pins / 101 inherited archives / 31 inherited postseal pins**. Real filesystem identities and actual physical hashes are preserved in raw streams. All 278 lineage pins, 100 mixed-model captures, 16 owner documents, 41 original UDF snapshots and every earlier failed attempt remain protected. The malformed v9 artifact stays **4,213,129,746 bytes**, SHA-256 `3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb`, and is not serialized into any derivative ZIP. Ignored historical TestResults was not bulk-enumerated. Master Plan2.14 and its separately preserved2.13 projection remain unchanged.

**{len(index['parts'])} independent complementary ZIPs**, **{len(index['files'])} unique logical paths**, each below180MiB, with actual local SHA/CRC, safe-path/collision controls and second physical serialization verified. These are not binary `.zip.001/.002` volumes: extract all complementary archives into one empty repository-root directory; no concatenation or reassembly is needed. Supply the external index3 alongside them. `verify_c16_publication_v4.py --index <index3> --parts --parts-dir <ZIP directory>` verifies actual supplied physical bytes. The safe copy reader validates paths, uniqueness, sizes, SHA/CRC, file types and the final copied checkpoint/predecessor chain. The independently extracted copy is explicitly .git-free, not a full standalone checkout or private/Git proof. Actual local ZIP bytes are hashed; **NOT_UPLOADED_BY_AGENT**. Independent external upload review requires the exact uploaded bytes.

{parts}

Final Git anchors/index/status/diff and additive artifact hashes have separate final external receipts. No stage, commit, reset, rebase, merge, push, restore, installer, deployment, scientific/R, native Excel/COM/UIA/RefEdit/manual or production packaging/Office/Mac/browser validation occurred.

`NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED`
'''
with (W2/'continuation-v16-postseal-handover-addendum-v1.md').open('x',encoding='utf-8',newline='\n') as f:f.write(handover)
# Reserve the final receipt paths before status capture. They are each written
# exactly once; no prior evidence or seal is overwritten.
gitfile=W2/'continuation-v16-final-git-preservation-v1.json';verification=W2/'continuation-v16-final-verification-v1.json';artifacts=W2/'continuation-v16-post-publication-artifacts-v1.json'
with gitfile.open('x',encoding='utf-8') as gout,verification.open('x',encoding='utf-8') as vout,artifacts.open('x',encoding='utf-8') as aout:
 receipts=[]
 for label,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('staged',['diff','--cached','--name-only']),('diff-check',['diff','--check']),('status-short',['status','--short'])]:
  r=command('postseal-final-git-'+label,['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args]);assert r['exit_code']==0
  folder=RUNROOT/'commands'/('w2c16-postseal-final-git-'+label);value=(folder/'stdout.log').read_bytes()
  if label in ['HEAD','main','merge-base']:assert value.decode().strip()==BASE
  if label=='branch':assert value.decode().strip()=='codex/04A-38'
  if label=='staged':assert not value
  receipts.append(dict(label=label,command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log')))
 idx=pin(ROOT/'.git/index');assert (idx['bytes'],idx['sha256'])==(620185,'2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c')
 json.dump(dict(status='FINAL_LIVE_GIT_ANCHORS_EXACT_INDEX_EMPTY_STAGED_AND_DIFF_ZERO',branch='codex/04A-38',HEAD=BASE,main=BASE,merge_base=BASE,index=idx,staged='EMPTY',diff_check_exit=0,raw=receipts,original_queries=0),gout,indent=2);gout.flush()
 json.dump(dict(status='FINAL_PARTIAL_W2_REVIEW_AND_REAL_BYTE_AUDITS_COMPLETE_NOT_ACCEPTED',checkpoint=pin(HERE/'checkpoint-verification-v3.json'),seal=pin(HERE/'review-seal-v3.json'),review=pin(HERE/'final/owner-review.md'),index=pin(indexpath),copy=pin(W2/'continuation-v16-copied-review-reader-v2.json'),physical=pin(W2/'continuation-v16-physical-review-verification-v2.json'),private=pin(W2/'continuation-v16-private-reader-verification-v1.json'),live=pin(W2/'continuation-v16-live-preservation-v1.json'),Git=pin(gitfile),counts=dict(historical_supported=607,historical_OPEN=97,current_supported=635,current_OPEN=91,support_supported=271,support_OPEN=32),Release={k:DEFERRED for k in ['compilation','full_selected_focused_unit_tests','ABI_CIL_signatures_exports','frozen_compatibility']},original_queries=0),vout,indent=2);vout.flush()
 pins=[pin(p) for p in sorted(W2.glob('continuation-v16-*')) if p.is_file() and p!=artifacts]
 json.dump(dict(status='FINAL_ADDITIVE_POST_PUBLICATION_ARTIFACT_PINS',files=pins,self_entry='Nonrecursive external artifact manifest; no self SHA assertion.',scope='Final additive authority and complete previous attempts are preserved; the enclosing finalizer raw command is bound additively after it exits.',original_queries=0),aout,indent=2)
print(json.dumps(dict(status='FINAL_V16_PARTIAL_CHECKPOINT_COMPLETE',handover=pin(W2/'continuation-v16-postseal-handover-addendum-v1.md'),review=pin(HERE/'final/owner-review.md'),index=pin(indexpath),private_pins=private['private_pin_count'],physical_archives=private['physical_archives'],parts=len(index['parts']),files=len(index['files']))),flush=True)
