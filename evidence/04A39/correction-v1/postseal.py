"""Additive final receipts and small complementary postseal transport, never product edits."""
from common import *
from verify_correction_v2 import local
from review_readback import verify as archive_verify
from diagnostic_producer import redactor
import collections,zipfile,zlib

verdict=local();lock()
archive=load(HERE/'publication/archive-readback.json')
physical=archive_verify(ROOT/archive['archive']['path'],archive['archive']['sha256'])
save('final/standalone-readback.json',dict(**physical,reader=pin(HERE/'review_readback.py'),archive=archive['archive'],command_exit=0,
    invocation='Owner Python -I -B review_readback.py <archive> --sha256 <external SHA>; also executed directly here to bind final machine receipt.'))
controls=[load(HERE/'validation'/name) for name in ['reporting-boundary-tests.json','evidence-reader-controls-v2.json','source-disposition-controls.json']]
assert sum(r['testsRun'] for r in controls)==24 and all(r['exit_code']==r['failures']==r['errors']==0 for r in controls)
gate=load(HERE/'publication/privacy-gate.json')
assert gate['unresolved']==0 and gate['scanner_findings']==gate['reviewed_exact_occurrences']==25
assert gate['repository_candidate_status']=='FAIL_UNWAIVED'
for path in HERE.rglob('*.py'):compile(path.read_bytes(),str(path),'exec')
raw_git=dict(utc=utc(),branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),
    parent=git('rev-parse','HEAD^').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),
    status_short=git('status','--short').decode(),status_all=git('status','--porcelain=v1','-uall').decode(),
    staged=git('diff','--cached','--name-only').decode(),diff_check=git('diff','--check').decode(),
    worktrees=git('worktree','list','--porcelain').decode(),index=pin('.git/index'))
assert raw_git['head']==raw_git['main']==raw_git['merge_base']==MAIN and not raw_git['staged'] and not raw_git['diff_check']
assert set(git('diff','--name-only').decode().splitlines())=={'BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan.md'}
save(PRIVATE/'git-final-raw.json',raw_git)
safe_git={k:v for k,v in raw_git.items() if k not in {'worktrees','status_all'}}
safe_git.update(raw=pin(PRIVATE/'git-final-raw.json'),staged_empty=True,diff_check_exit=0,original_source_unchanged=True,index_unchanged=True,
    tracked_changes='Two pre-existing39 tracked changes only; no tracked file edited by this continuation.',
    untracked_evidence='New correction namespace and existing39 evidence remain unstaged. Delivery/self metadata is additive after this timestamp; HEAD/main/index anchors rechecked below.')
save('final/git-final-v1.json',safe_git)
attempts=[
 dict(id='INVESTIGATE-V1',outcome='STOPPED',reason='Absent-in-main candidate reconstruction omitted ignore rules; stopped before continuing historical scans. No parent identity executed.'),
 dict(id='PRIVACY-REPLAY-V2',outcome='FIRST_SCANNER_COMPLETED_SECOND_INTERRUPTED',reason='1599 exact13 findings retained; unchanged-AST guarded14 replay subsequently completed1599. Original assertions unchanged.'),
 dict(id='DIAGNOSTIC-READER-V1',outcome='ERROR',reason='Duplicate dictionary keyword; original script retained.'),
 dict(id='DIAGNOSTIC-READER-V2',outcome='FAIL',reason='Delegated publication producer absent from direct test body; follow source-backed helper edge.'),
 dict(id='DIAGNOSTIC-READER-V3',outcome='FAIL',reason='Counter mapping values instead of identity keys. Exact-key correctionv4 completed; copies retained in distinct paths.'),
 dict(id='STAGE-UTF8-V1',outcome='ERROR',reason='Strict historical UTF8 reader encountered legacy source bytes; no ZIP created. Version2 scans both strict round-tripping views, preserving original bytes.'),
 dict(id='EVIDENCE-READER-CONTROLS-V1',outcome='7_PASS_1_FAIL',reason='Event key omitted subtest. Additive54-event crosswalk-v2 and reader-v2 preserve full subtest identities;8 controls pass.'),
 dict(id='INSPECTION-ONLY',outcome='RETAINED_TOOL_TRANSCRIPT',reason='Bounded path/shell inspection mistakes and a minified metadata line produced errors/oversized output; no source writes or campaign identities. No attempt used to overwrite a failure.')]
save('final/attempt-register.json',dict(attempts=attempts,original_dossier_unchanged=True,source_or_expected_result_repairs=0,prior_failing_receipts_retained=True))
facts=load(HERE/'validation/retained-evidence-readback.json')['retained_facts']
readiness=dict(**verdict,utc=utc(),main=MAIN,branch='codex/04A-39',tip=MAIN,
    active_local_reader=pin(HERE/'verify_correction_v2.py'),active_public_reader=pin(HERE/'review_readback.py'),
    original39=pin(PRIOR/'final/delivery-receipt-v1.json'),old_dossier_status='NOT_READY_04A39_UNCHANGED',
    source_lock=pin(HERE/'entry/source-lock.json'),source_paths=1353,production_or_existing_test_changes=0,
    retained_results=facts,fresh_python_full_campaign=False,focused=dict(parents=26,PASS=3,FAIL=23,ERROR=0,command_exit=1),
    changed_diagnostic_events=54,changed_diagnostics_explained=54,trace_differences_only_assertion_messages=True,
    original_privacy_per_parent=1599,focused_privacy_per_parent=1941,privacy_findings_unwaived=True,
    new_reporting_controls=dict(executed=24,PASS=24,FAIL=0,ERROR=0),
    staged_publication=gate,main_archive=archive['archive'],standalone_readback=pin(HERE/'final/standalone-readback.json'),
    preservation=pin(HERE/'preservation/postpublication.json'),Git=pin(HERE/'final/git-final-v1.json'),
    actions=[dict(id='A39-RAW-REVIEW-PRIVACY-01',status='BLOCKED_OWNER_DECISION',action='Decide whether the new physical derivative gate can satisfy39 publication while the two historical Git-candidate FAILs and immutable raw originals remain unwaived. No control/test/domain is silently redefined.'),
             dict(id='PYTHON_54_CHANGED_DIAGNOSTICS',status='EXPLANATION_VERIFIED_ORIGINAL_FAILURES_UNWAIVED',action='Review54 exact source-bound assertion differences; retain historical register facets and scientific restrictions.'),
             dict(id='V6_RAW_DERIVATIVE',status='WORKFLOW_REPLACED_OLD_ARCHIVE_PRESERVED',action='Use the gated correction derivative for review. External v6 workflow/archive were not changed or republished.'),
             dict(id='SCIENTIFIC_NATIVE_AND_04A40',status='NOT_AUTHORIZED',action='No statistical/native changes, acceptance, Git publication or next batch authorized.')],
    transport_self_policy='Main ZIP was frozen before archive-bound receipts. Small postseal companion transports these additive receipts and helper source; final delivery receipt is external to avoid circular archive hashes.')
save('final/readiness.json',readiness)
summary=f'''# 04A.39 corrected owner checkpoint

`BLOCKED_04A39_OWNER_DECISION_REQUIRED`

The replacement upload boundary passed: **2,058 payloads / 2,060 physical members**, **25 exact synthetic test-literal matches / zero unresolved sensitive findings**, and complete SHA/size/CRC plus standalone readback. Main ZIP: **74,763,995 bytes**, SHA-256 `{archive['archive']['sha256']}`. The main ZIP contains the full correction report,54-event crosswalk,956-parent reconciliation, register/source proofs and review reader. This additive postseal report and receipts are transported separately to avoid circular hashes.

All **54** changed diagnostics are source-explained and their current messages reproduce exactly. Focused Python: **26 parents =3 PASS /23 FAIL /0 ERROR**, command exit1. Both privacy parents still FAIL with **1,941 findings each**, retaining the complete1,599-finding earlier snapshot. Both W3 privacy ERRORs remain unwaived. **24 reporting/publication/reader controls PASS**; failed earlier report-only versions remain preserved.

The actual v6 archive contains102 known raw diagnostic files and1,934 source-bound finding occurrences. Its4,479 payloads are not equivalent to the older1,956-payload Codex ZIP. Neither old archive nor the external v6 script was modified. Use the new gated process; the old raw derivative is not approved for republication.

No production or existing test source changed. All1,353 source/test/plan pins and the original468-file NOT_READY checkpoint remain exact. The192-member map and1,169 canonical entries still cover1,514 obligations and802 restricted controls. Retained original39 results: Debug21,113 PASS/14 FAIL; targeted Release336 PASS;268 exports and77 fixtures per configuration;29,424 sealed compatibility cases/configuration; Python833/112/11; Node916 PASS; strict/profile/MkDocs1/1/0. These campaigns were not rerun here.

HEAD/main/merge-base remain `{MAIN}` on `codex/04A-39`; staged index is empty and unchanged; diff-check exits0. Only the pre-existing project/plan tracked changes remain. No commit, merge, push, native/scientific gate or04A.40 work occurred. W3 preservation37484/154 remains inherited with its qualified scope; this task's exact incremental readback is documented separately.

**Owner decision required:** whether the new physical derivative gate may satisfy39 publication while the historical repository-candidate FAILs remain unwaived and raw originals remain protected locally. Changing protected records, restricted controls or a privacy waiver requires separate explicit authority. No39 acceptance is inferred. See `readiness.json`, `git-final-v1.json` and the final external delivery receipt for exact pins and scope.

`P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A40_NOT_AUTHORIZED`
'''
with (HERE/'final/owner-review-v2.md').open('x',encoding='utf-8',newline='\n') as f:f.write(summary)
with (HERE/'final/next-gate-handover.md').open('x',encoding='utf-8',newline='\n') as f:f.write('Independent owner/ChatGPT review of the main correction ZIP and complementary postseal receipts is recommended. Resolve the exact publication-domain decision in readiness.json; preserve both historical privacy FAILs and all raw originals. Do not infer39 acceptance, stage/commit/merge or begin04A.40. All scientific/native gates remain unchanged.\n')
# Exact new local additions; self/index/delivery paths have explicit acyclic exclusions.
added=[pin(p) for p in sorted(HERE.rglob('*')) if p.is_file() and '__pycache__' not in p.parts]
save('final/added-files-v1.json',dict(files=added,modified_existing_files=[],scope='Correction namespace additions only; protected private artifacts have their separate exact inventory. Self and subsequent transport/delivery metadata are explicitly excluded from this acyclic snapshot.'))
post_paths=['postseal.py','publication/privacy-gate.json','publication/payload-index.json','publication/archive-readback.json','preservation/postpublication.json',
 'final/standalone-readback.json','final/git-final-v1.json','final/attempt-register.json','final/readiness.json','final/owner-review-v2.md','final/next-gate-handover.md','final/added-files-v1.json']
post_rows=[]
for rel in post_paths:
    path=HERE/rel;name=path.relative_to(ROOT).as_posix();data=path.read_bytes()
    assert not redactor.scan(name,data),'POSTSEAL_METADATA_PRIVACY_FAILURE'
    post_rows.append(dict(**pin(path),crc32=f'{zlib.crc32(data):08x}'))
index=dict(schema='04A39-correction-postseal-v1',files=post_rows,main_archive=archive['archive'],gates=GATES,
    scope='Complementary additive postseal metadata and source only. Raw diagnostics excluded. This index excludes itself; external delivery receipt pins both ZIPs and all physical members.')
index_data=(json.dumps(index,ensure_ascii=False,indent=2)+'\n').encode()
assert not redactor.scan('.postseal-inventory.json',index_data)
post=PRIVATE/'BESHStatNG_04A39_CORRECTION_POSTSEAL_v1.zip'
with zipfile.ZipFile(post,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6) as z:
    for row in post_rows:
        data=(ROOT/row['path']).read_bytes();assert sha(data)==row['sha256']
        z.writestr(row['path'],data)
    z.writestr('.postseal-inventory.json',index_data)
with zipfile.ZipFile(post) as z:
    assert z.testzip() is None and len(z.namelist())==len(post_rows)+1
    assert len(z.namelist())==len({n.casefold() for n in z.namelist()})
    for row in post_rows:
        data=z.read(row['path']);assert sha(data)==row['sha256'] and len(data)==row['bytes'] and f'{z.getinfo(row["path"]).CRC:08x}'==row['crc32']
lock();assert not git('diff','--check')
save('final/delivery-receipt-v1.json',dict(status=verdict['status'],utc=utc(),main_archive=archive['archive'],postseal_archive=pin(post),
    main_members=2060,postseal_members=len(post_rows)+1,archive_crc_tests='PASS_BOTH',main_per_file_index=pin(HERE/'publication/payload-index.json'),postseal_files=post_rows,
    postseal_index_sha256=sha(index_data),postseal_privacy=dict(command_exit=0,unresolved=0,scanner_findings=0,scope='Actual12 postseal files and embedded index scanned before creating companion ZIP'),
    owner_review=pin(HERE/'final/owner-review-v2.md'),readiness=pin(HERE/'final/readiness.json'),Git=pin(HERE/'final/git-final-v1.json'),
    source_lock_unchanged=True,index_unchanged=True,staged_empty=True,diff_check_exit=0,gates=GATES,
    self_policy='External delivery receipt omitted from both ZIPs to avoid circular archive hashes; its fields bind every physical payload and both actual archives.',
    external_upload=False,owner39_acceptance=False,commit_merge_push=False))
print('BLOCKED_04A39_OWNER_DECISION_REQUIRED; both safe derivatives verified; no product changes or Git publication.',flush=True)
