"""Final additive authority; never rewrites a prior source, seal, receipt or archive."""
from common import *
import stat
load=lambda p:json.loads(Path(p).read_bytes())
def external(name,doc):
 p=W2/name
 with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(doc,f,indent=2,ensure_ascii=False);f.write('\n')
 return pin(p)
audit=load(HERE/'actual-private-preservation-receipt-v1.json');assert audit['private_pins']==29489 and audit['physical_archives']==141 and audit['status'].endswith('_PASS')
transport=load(HERE/'publication-physical-reader-receipt-v1.json');assert transport['status']=='ALL_EIGHT_PHYSICAL_PART_AND_PAYLOAD_BYTES_VERIFIED'
index=W2/'continuation-v17-review-parts-index-v1.json';parts=load(index)['parts'];assert len(parts)==8 and all(r['bytes']<180*1024**2 for r in parts)
for r in load(HERE/'review-seal-v2.json')['files']:assert pin(ROOT/r['path'])==r,'Final sealed input drift '+r['path']
gitrows=[]
for label,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('staged',['diff','--cached','--name-only']),('diff-check',['diff','--check']),('status',['status','--short','--untracked-files=normal'])]:
 r=command('postseal-terminal-git-'+label,['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args]);assert r['exit_code']==0
 p=RUNROOT/'commands'/('w2c17-postseal-terminal-git-'+label);out=(p/'stdout.log').read_bytes()
 if label=='branch':assert out.decode().strip()=='codex/04A-38'
 elif label in ['HEAD','main','merge-base']:assert out.decode().strip()==BASE
 elif label in ['staged','diff-check']:assert out==b''
 gitrows.append(dict(label=label,command=pin(p/'command.json'),stdout=pin(p/'stdout.log'),stderr=pin(p/'stderr.log')))
idx=pin(ROOT/'.git/index');assert idx['bytes']==620185 and idx['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
gitreceipt=external('continuation-v17-final-git-preservation-v1.json',dict(branch='codex/04A-38',HEAD=BASE,main=BASE,merge_base=BASE,index=idx,staged_empty=True,diff_check_exit=0,worktree='EXPECTED_DIRTY_UNSTAGED',raw=gitrows,forbidden_git_operations='NOT_RUN',original_queries=0))
# These completed postseal command roots were deliberately outside the main
# nonrecursive private freeze. Hash their actual final files and bind them here.
late=[]
for folder in sorted((RUNROOT/'commands').iterdir(),key=lambda p:p.name):
 if not folder.is_dir() or not folder.name.startswith('w2c17-postseal-') or not (folder/'command.json').is_file():continue
 for path in sorted(folder.iterdir(),key=lambda p:p.name):
  if path.is_file():late.append(pin(path))
late.append(pin(PRIVATE/'final-actual-private-preservation-v1.json'))
assert len(late)==len({r['path'] for r in late})
privateadd=external('continuation-v17-postseal-private-addendum-v1.json',dict(status='ACTUAL_COMPLETED_POSTSEAL_RAW_BYTES_PINNED',base_manifest=pin(HERE/'retained-private-preservation-v2.json'),base_actual_audit=audit,postseal_private_pins=late,count=len(late),currently_running_finalizer='Own command/stdout/stderr are bound separately after completion; no recursive self-pin is manufactured.',original_queries=0))
files=[HERE/n for n in ['final/owner-review.md','review-seal-v2.json','checkpoint-verification-v2.json','actual-private-preservation-receipt-v1.json','publication-physical-reader-receipt-v1.json','retained-private-preservation-v2.json','final-validation-qualification-v2.json','adverse-reader-controls-v2.json','transport-path-controls-v2.json','verify_c17_checkpoint_v2.py','verify_c17_transport_v2.py','private_preservation_v1.py','TRANSPORT_README-v2.md','source-transitions-final-v2.json','ownership-summary-v4.json','canonical-identity-status-v4.json','remaining-work-queue-v4.json','whitespace-compiled-equivalence-v1.json']]
files += [HERE/'finalize_postseal_v1.py',index,W2/'continuation-v17-publication-receipt-v1.json',W2/'continuation-v17-final-git-preservation-v1.json',W2/'continuation-v17-postseal-private-addendum-v1.json']
artifacts=external('continuation-v17-post-publication-artifacts-v1.json',dict(status='FINAL_ADDITIVE_AUTHORITY_FILES_PINNED',files=[pin(p) for p in files],source_archive_payloads=11522,physical_parts=8,prior_v16_physical_eight='NOT_UPLOADED;LOCALLY_PRESERVED',upload='NOT_PERFORMED_BY_AGENT',scope='Several final postseal receipts are explicitly additive and outside the earlier eight-part payload union. The index and this authority identify them; no claim that a pre-existing ZIP contains later files.',original_queries=0))
verification=external('continuation-v17-final-verification-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',source_proposal=dict(historical_supported=704,historical_OPEN=0,current_supported=726,current_OPEN=0,support_supported=303,support_OPEN=0,new_historical=97,new_current=91,new_support=32),checkpoint=pin(HERE/'checkpoint-verification-v2.json'),private_actual=audit,postseal_private_addendum=privateadd,physical_transport=transport,index=pin(index),final_git=gitreceipt,artifacts=artifacts,validation=pin(HERE/'final-validation-qualification-v2.json'),additional_comprehensive_final_source_managed_Python='NOT_RUN_REQUIRES_SPECIFIC_OWNER_REPLACEMENT_AUTHORITY',Release_compilation=DEFERRED,Release_full_selected_focused_unit_tests=DEFERRED,Release_ABI_CIL_signatures_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,gate='P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED',original_queries=0))
text='''# Final additive continuation-v17 authority and handover

This is the final additive authority after all preserved corrections. It supersedes provisional v17 review/ledger/seal/reader choices without rewriting any predecessor evidence. The source remains NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED.

Read `continuation-v17/final/owner-review.md`, seal v2 / checkpoint reader v2, ownership ledgers v4, final byte source freeze8, source transitions v2, final verification/Git/publication/private records identified below. Original `owner-review.md`, ledgers v1/v2/v3, the failed seal/reader v1, failed compiles/binders, two fixture failures, two current-only box-alias counterexamples, overbroad MVID equality assertions and the rejected one-ACTIVE packing attempt remain immutable history.

The source-bound proposal is704/704 historical,726/726 logical current,303/303 distinct support (325raw):97/91/32NEW and607/635/271RETAINED, with0/0/0OPEN. Physical Windows bodies are723 plus three exact moved Common lambda counterparts. Original fragments20683 and current physical Native fragments12659 are fully reconciled; the previous13695 fragment identities remain in the crosswalk. Independent owner acceptance has not occurred.

Debug build/signature/CIL/268-export evidence is current. All29424 Debug current-only comparisons matched original frozen bytes at functional freeze7 (28966PRE_EDIT_FROZEN +458LATE_ARCHIVED_ENTRY_SUPPLEMENT). Byte freeze8 removes exactly two accidental extra CR bytes; every measured method/type/instruction/exception/signature and export is exact to functional7, while rebuilt assembly hashes/MVIDs differ. No fourth compatibility campaign ran.

One comprehensive managed Debug run produced21076PASS/16FAIL among21092 identities before final correction. After bounded rechecks, the effective identity record is21080PASS/14inheritedFAIL among21094, including41new passing identities. This is not a comprehensive final-source campaign. One Python campaign ran every956parent once:830PASS/113FAIL/13ERROR; events185FAIL/13ERROR, two new survival source-pin subtests and14changed raw messages. It predates the final production boxing correction. Node916PASS; strict/profile/MkDocs exits1/1/0. Every failure/error and raw trace remains unwaived. No replacement full managed/Python/scanner campaign was silently launched.

The final actual private audit checked29489file pins and141physical archives by full bytes and live filesystem identity, retaining all23711v16private pins,102v16postseal raw files and133inherited archives. Completed postseal raw files have a separate additive pin list. The4213129746-byte malformed v9 aggregation remains SHA2563ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb; stream-hash only, never deserialized or placed in the derivative ZIPs. Ignored historical TestResults was not enumerated/deleted. Master Plan2.14 remains0ee45eae70aa35d6c87c636d82c3f33e105b8c12e6e5a4badb69e38fe7ad69dd and the separate2.13projection remains protected.

Transport:11522unique logical payloads in eight independently valid complementary ZIPs, **two ACTIVE and six REFERENCE**, each below180MiB. Every actual local physical ZIP SHA and every payload SHA/size/CRC passed verification with identical embedded manifests. Use transport reader v2 and TRANSPORT_README-v2.md; the original one-ACTIVE README describes the rejected attempt. These are not .zip.001/.002 binary volumes. No upload was performed. The prior v16eight physical ZIPs are NOT_UPLOADED in this derivative. External physical verification requires actual uploaded bytes. A copied Git-free review tree cannot attest live Git/private files/execution.

No staging/commit/reset/restore/merge/rebase/push, archived-original execution/capture, statistical-engine changes, scientific/R/native Excel/COM/UIA/RefEdit/manual, production Office/Mac/browser, restore, production packaging/signing/install/deploy, W3 or04A.39 occurred. Release compilation, Release full/selected/focused tests, Release ABI/CIL/signatures/exports and Release frozen compatibility each remain `NOT_RUN_OWNER_DEFERRED_TO_04A38_FINALIZATION`.

The specific owner decision is whether to authorize one replacement comprehensive Debug and one complete Python/scanner campaign to certify final functional7/byte8. This is required by the owner's post-correction campaign policy before any additional full runs; it is not a request to accept/merge04A.38 or begin Release/W3/04A.39.

P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED.

Authoritative final verification: VERIFICATION
Final additive artifact pins: ARTIFACTS
Final Git preservation: GITRECEIPT
Final postseal private pins: PRIVATEADD
'''.replace('VERIFICATION',verification['path']).replace('ARTIFACTS',artifacts['path']).replace('GITRECEIPT',gitreceipt['path']).replace('PRIVATEADD',privateadd['path'])
p=W2/'continuation-v17-postseal-handover-addendum-v1.md'
with p.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
external('continuation-v17-final-authority-binding-v1.json',dict(handover=pin(p),verification=verification,artifacts=artifacts,owner_review=pin(HERE/'final/owner-review.md'),status='NOT_READY_04A38',original_queries=0))
print('Final additive handover and receipts written; source proposal complete, owner review/replacement campaign decision pending',flush=True)
