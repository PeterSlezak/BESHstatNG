"""Owner-authorized documentation-only closeout; no production/test implementation edits."""
from work import *
from summarize import clean
from verify_packet import SOURCES, SELF, READY, validate_manifest
from datetime import datetime, timezone

VERSION=HERE/'closeout/v1'
PLAN='planning/BESHStatNG_CrossPlatform_MasterPlan.md'
ORIGINAL='55662468c2b756cca7ea1e880b60bdc2c7f2f94db4c024d677c6cfce6eff91be'
CORRECTED='436b66c4307bd1571649c2604f8d90b5f0b5d843df3567b918698392fe5d6e7c'
SNAPSHOTS=['readiness.json','external-changes.json','external-owner-plan-change.json','git-final.json','validation.json','publication-manifest.json','privacy-check.json','source-transitions.json','preservation.json','audit-results.json','command-receipts.json','coverage.json']
def load(name):return json.loads((HERE/name).read_bytes())
def append(name,value):
    path=VERSION/name
    with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,ensure_ascii=False,indent=2,allow_nan=False);f.write('\n')
def check_git():
    assert git('branch','--show-current')=='codex/04A-36'
    assert git('rev-parse','HEAD')==git('rev-parse','main')==git('merge-base','HEAD','main')==BASE
    assert not git('diff','--cached','--name-only')
def command(args):
    r=subprocess.run(args,cwd=ROOT,capture_output=True,text=True)
    return dict(command=clean(args),exit_code=r.returncode,stdout=clean(r.stdout),stderr=clean(r.stderr))
def git_check(*args):return command(['git','-c','safe.directory='+ROOT.as_posix(),*args])
def prepare():
    check_git();assert not VERSION.exists()
    manifest=load('publication-manifest.json')
    validate_manifest(manifest['files'],{r['path'] for r in manifest['files']})
    old={r['path']:r for r in manifest['files']}
    for p in SOURCES:assert pin(p)==old[p]
    external=load('external-changes.json');assert len(external['files'])==16
    for row in external['files']:
        if row['path']!=PLAN:assert pin(row['path'])==row['current']
    observed=(ROOT/PLAN).read_bytes();assert sha(observed) in [ORIGINAL,CORRECTED]
    original=observed
    if sha(observed)==CORRECTED:
        lines=observed.splitlines(keepends=True);assert lines[2].endswith(b'**\n');lines[2]=lines[2][:-1]+b'  \n';original=b''.join(lines)
    assert sha(original)==ORIGINAL and len(original)==78247
    assert original.splitlines(keepends=True)[2]==b'**Revision 2.13 \xc2\xb7 7 November 2026 \xc2\xb7 concise status/roadmap edition**  \n'
    VERSION.mkdir(parents=True)
    history=[]
    for name in SNAPSHOTS:
        raw=(HERE/name).read_bytes();dest=VERSION/'pre-closeout'/name;dest.parent.mkdir(parents=True,exist_ok=True)
        with dest.open('xb') as f:f.write(raw)
        history.append(dict(original_path='evidence/04A36/'+name,snapshot=dest.relative_to(ROOT).as_posix(),bytes=len(raw),sha256=sha(raw)))
    dossier='docs/architecture/batch-04A-36-owner-review.md';raw=(ROOT/dossier).read_bytes()
    with (VERSION/'pre-closeout/owner-review.md').open('xb') as f:f.write(raw)
    history.append(dict(original_path=dossier,snapshot='evidence/04A36/closeout/v1/pre-closeout/owner-review.md',bytes=len(raw),sha256=sha(raw)))
    private=ROOT/'.test-rationalization-04A36.tmp/closeout-v1';private.mkdir(exist_ok=False)
    with (private/'original-owner-plan-v213.md').open('xb') as f:f.write(original)
    append('entry.json',dict(time_utc=datetime.now(timezone.utc).isoformat(),branch='codex/04A-36',head=BASE,main=BASE,merge_base=BASE,index='EMPTY',original_plan=dict(path=PLAN,bytes=len(original),sha256=sha(original)),observed_live_plan=pin(PLAN),already_corrected_on_observation=sha(observed)==CORRECTED,original_private_copy_basis='Exact inverse reconstruction from supplied corrected bytes' if sha(observed)==CORRECTED else 'Observed original bytes',original_manifest_rows=len(manifest['files']),snapshots=history,production_test_project_pins=[pin(p) for p in SOURCES if p!=dossier],external_files=external['files'],live_entry_full_diff_check=git_check('diff','--check'),authority='Owner continuation prompt: sixteen documents are owner-requested ChatGPT deliverable content; only two spaces on Plan line3 authorized for removal. Operator who previously applied documents/correction is not observed or asserted.'))
    note='''# 04A.36 closeout v1 — append-only owner authorization note

The owner confirmed content provenance of fifteen README updates from `BESHStatNG_README_updates_20261107.zip` and Master Plan Revision2.13 from `BESHStatNG_CrossPlatform_MasterPlan_v2.13_20261107.md`, based on independent exact-byte delivery comparison. This confirms owner-requested content, not the identity of the operator who copied it into the worktree. That operator remains unobserved; no filesystem-operator attestation is required or invented.

The owner expressly authorizes removing only the two trailing ASCII spaces on Plan line3. Original:78,247 bytes/SHA256 `55662468c2b756cca7ea1e880b60bdc2c7f2f94db4c024d677c6cfce6eff91be`. Expected corrected:78,245 bytes/SHA256 `436b66c4307bd1571649c2604f8d90b5f0b5d843df3567b918698392fe5d6e7c`. On live observation the Plan already had the exact corrected bytes; Codex does not overwrite it or claim to have applied the correction. Binary inverse reconstruction verifies the two-byte difference and every other byte/line ending; the actor who applied the correction remains unobserved. The initial guard stopped on this already-corrected state before any write; the subsequent path explicitly accepts only these two owner-supplied hashes.

The pre-closeout NOT_READY dossier, both open holds, full diff-check failure, first manifest and all listed current receipts are retained byte-for-byte under `pre-closeout/`, with original sizes/hashes in `entry.json`. Original batch-entry Plan/source/fixtures and all earlier test outcomes remain unchanged. Original Revision2.13 bytes are retained only in the ignored private execution directory; neither the Plan nor any of the fifteen README files is a batch publication payload.

Subsequent numbered JSON receipts append the applied correction, live Git checks, focused audit and independent verifier outcomes. No full managed/Node/Python suites are rerun: no production/test/build behavior changes. Counts/nonpasses, V1/V2 fixture qualifications and strict/profile1/1 remain historical facts. Native/R/Mac/Office manual and packaging checks remain NOT_RUN; all software comparisons NOT_GOLDEN. P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED. No staging/commit/merge/push/deployment or04A.37 work.
'''
    with (VERSION/'closeout-note.md').open('x',encoding='utf-8',newline='\n') as f:f.write(note)
    print('Preserved pre-closeout receipts and exact original Plan; authorized boundary verified')
def apply():
    check_git();entry=json.loads((VERSION/'entry.json').read_bytes())
    observed=(ROOT/PLAN).read_bytes();assert sha(observed) in [ORIGINAL,CORRECTED]
    original=observed
    if sha(observed)==CORRECTED:
        restored=observed.splitlines(keepends=True);restored[2]=restored[2][:-1]+b'  \n';original=b''.join(restored)
    assert sha(original)==ORIGINAL and len(original)==78247
    for row in entry['external_files']:
        if row['path']!=PLAN:assert pin(row['path'])==row['current']
    lines=original.splitlines(keepends=True);assert lines[2].endswith(b'**  \n')
    lines[2]=lines[2][:-3]+b'\n';corrected=b''.join(lines)
    assert len(corrected)==78245 and sha(corrected)==CORRECTED
    restored=corrected.splitlines(keepends=True);restored[2]=restored[2][:-1]+b'  \n';assert b''.join(restored)==original
    applied_by_this_execution=sha(observed)==ORIGINAL
    if applied_by_this_execution:(ROOT/PLAN).write_bytes(corrected)
    else:assert observed==corrected
    for row in entry['external_files']:
        if row['path']!=PLAN:assert pin(row['path'])==row['current']
    checks=dict(full_diff_check=git_check('diff','--check'),status_short=git_check('status','--short'),index=git_check('diff','--cached','--name-only'))
    assert checks['full_diff_check']['exit_code']==0 and checks['index']['exit_code']==0 and not checks['index']['stdout']
    append('01-correction.json',dict(original_plan=entry['original_plan'],corrected_plan=pin(PLAN),removed_bytes_hex='2020',line=3,all_other_bytes_and_line_endings_preserved=True,inverse_reconstruction_sha256=ORIGINAL,readmes_unchanged=15,applied_by_this_execution=applied_by_this_execution,correction_operator='Codex under explicit owner authorization' if applied_by_this_execution else 'NOT_OBSERVED_NOT_ASSERTED; exact correction already present',checks=checks))
    print('Verified exact two-space correction; Plan78245 bytes; full diff-check0; index empty; applied now:',applied_by_this_execution)
def metadata():
    check_git();assert pin(PLAN)['sha256']==CORRECTED
    entry=json.loads((VERSION/'entry.json').read_bytes());external=load('external-changes.json')
    external['status']='OWNER_CONFIRMED_PRESERVED'
    external['attribution']=dict(content_origin='OWNER_REQUESTED_CHATGPT_DOCUMENTATION_DELIVERABLES',readme_delivery='BESHStatNG_README_updates_20261107.zip',plan_delivery='BESHStatNG_CrossPlatform_MasterPlan_v2.13_20261107.md',evidence='Owner-reported independent exact-byte delivery comparison; all15 live README hashes match saved evidence, and live Plan matches exactly the supplied corrected hash.',operator_who_applied_deliverables='NOT_OBSERVED_NOT_ASSERTED',confirmation_scope='Content provenance, owner request and separate preservation; no inference about the earlier filesystem operator.',plan_line3_amendment='Explicitly authorized by owner; exact two-space correction already present on observation, not overwritten by Codex. Original/corrected hashes and inverse proof recorded in closeout/v1.')
    for row in external['files']:
        if row['path']==PLAN:
            row['before_authorized_line3_correction']=row['current'];row['current']=pin(PLAN)
            row['diff_sha256']=sha(git('diff','--',PLAN).encode())
        else:assert pin(row['path'])==row['current']
    external['qualification']='All16 external documents remain outside batch publication and unstaged. Fifteen READMEs byte-preserved; Plan differs from confirmed delivery by the expressly authorized two-space correction only. Prior pending attribution is retained in closeout/v1/pre-closeout.'
    save('external-changes.json',external)
    plan=load('external-owner-plan-change.json');plan['before_authorized_line3_correction']=plan['current'];plan['current']=pin(PLAN);plan['git_diff_sha256']=sha(git('diff','--',PLAN).encode());plan['disposition']='OWNER_CONFIRMED_EXTERNAL_CONTENT_WITH_AUTHORIZED_LINE3_CORRECTION';plan['reason']='Owner-requested Revision2.13 content provenance confirmed; copy/correction operators unobserved. Exact authorized two-space amendment was already present and verified without overwrite; Plan excluded from batch manifest.';save('external-owner-plan-change.json',plan)
    readiness=load('readiness.json');readiness['status']=READY;readiness['closed_holds']=[dict(r,status='CLOSED',resolution='Owner confirmed content provenance and separately authorized exact Plan line3 whitespace amendment; full live diff-check0 and index empty.',evidence='closeout/v1/01-correction.json') for r in readiness['blockers']];readiness['blockers']=[];readiness['closeout_note']='closeout/v1/closeout-note.md';save('readiness.json',readiness)
    validation=load('validation.json');validation['decision']=READY;save('validation.json',validation)
    coverage=load('coverage.json');coverage['status']=READY;save('coverage.json',coverage)
    preservation=load('preservation.json')
    for row in preservation['external_documentation_changes']:
        if row['path']==PLAN:row['final_sha256']=CORRECTED;row['final_bytes']=78245;row['disposition']='OWNER_EXTERNAL_CONTENT_AUTHORIZED_TWO_SPACE_CORRECTION'
    preservation['master_plan']='Owner-confirmed external Revision2.13; exact authorized line3 two-space correction only. All15 README delivery bytes preserved. Original entry/first-attempt receipts retained; Plan/READMEs excluded from publication.';save('preservation.json',preservation)
    dossier=ROOT/'docs/architecture/batch-04A-36-owner-review.md';text=dossier.read_text(encoding='utf-8')
    first='**NOT_READY_04A36 — software implementation and validation complete; external documentation closeout remains unresolved.**'
    assert first in text;text=text.replace(first,'**READY_FOR_OWNER_04A36_ACCEPTANCE_REVIEW — software implementation, validation and documentation closeout complete.**',1)
    start=text.index('The remaining holds are outside');end=text.index('\n\n## Verified base',start)
    text=text[:start]+'Both documentation holds are closed. The owner confirmed the sixteen files as owner-requested ChatGPT delivery content; the filesystem-copy/correction operators remain unobserved and are not asserted. The live Plan already matched the exact separately authorized line3 correction; Codex verified the two-space difference by inverse reconstruction and did not overwrite it. Every other byte/line ending and all fifteen READMEs are preserved. Full live `git diff --check` exits0 and the index is empty. The original NOT_READY dossier/holds/failure receipts remain byte-identical in the [versioned closeout history](../../evidence/04A36/closeout/v1/closeout-note.md). The Plan and fifteen READMEs remain outside batch publication and must be staged separately only at owner direction.'+text[end:]
    text=text.replace('1. Resolve attribution of concurrent README/plan changes and the Master Plan line-3 whitespace finding without overwriting owner work. Recheck actual Git provenance and reseal changed packet metadata.','1. Documentation holds are closed under the explicit owner closeout instruction. Review [closeout receipts](../../evidence/04A36/closeout/v1/01-correction.json): original Plan SHA256 `'+ORIGINAL+'` (78,247 bytes), corrected `'+CORRECTED+'` (78,245 bytes), full diff-check0, empty index. Content provenance is confirmed; the earlier copy operator is not inferred.')
    dossier.write_text(text,encoding='utf-8',newline='\n')
    transitions=load('source-transitions.json')
    for row in transitions['files']:
        current=pin(row['path'])
        if row['path']=='docs/architecture/batch-04A-36-owner-review.md':row.update(current)
        else:assert all(row[k]==current[k] for k in ['path','bytes','sha256'])
    save('source-transitions.json',transitions)
    for r in entry['production_test_project_pins']:assert pin(r['path'])==r
    git_state=load('git-final.json');git_state.update(status_short=git('status','--short').splitlines(),diff_stat=git('diff','--stat'),full_diff_check_exit=0,full_diff_check_output='',closeout='Owner content provenance confirmed; only authorized Plan line3 spaces removed; prior Git receipt retained in closeout/v1/pre-closeout.')
    assert git_check('diff','--check')['exit_code']==0;save('git-final.json',git_state)
    append('02-metadata.json',dict(status=READY,closed_holds=readiness['closed_holds'],production_test_build_files_unchanged=len(entry['production_test_project_pins']),validation_change='Decision only; all counts/nonpass identities/fixtures/gates/NOT_RUN lanes unchanged.',external_files_excluded=[r['path'] for r in external['files']],operator_attribution='Copy/correction operators not observed; exact authorized Plan amendment already present, verified by Codex without overwrite.'))
    print('Both holds closed; current metadata READY; historical receipts and19 production/test/project payloads unchanged')
def record():
    check_git();entry=json.loads((VERSION/'entry.json').read_bytes())
    for r in entry['snapshots']:
        actual=pin(r['snapshot']);assert actual['sha256']==r['sha256'] and actual['bytes']==r['bytes']
    for r in entry['production_test_project_pins']:assert pin(r['path'])==r
    old_validation=json.loads((VERSION/'pre-closeout/validation.json').read_bytes());current=load('validation.json')
    assert {k:v for k,v in old_validation.items() if k!='decision'}=={k:v for k,v in current.items() if k!='decision'}
    assert pin(PLAN)['sha256']==CORRECTED and pin(PLAN)['bytes']==78245
    for r in entry['external_files']:
        if r['path']!=PLAN:assert pin(r['path'])==r['current']
    folder=ROOT/'.test-rationalization-04A36.tmp/commands/doccloseout-v1-packet-audits'
    audit=json.loads((folder/'command.json').read_bytes());raw=(folder/'stderr.log').read_bytes();text=raw.decode('utf-8-sig')
    assert audit['exit_code']==0 and re.search(r'Ran 44 tests',text) and text.rstrip().endswith('OK')
    reader=command([sys.executable,'-B','evidence/04A36/verify_packet.py']);assert reader['exit_code']==0
    checks=dict(full_diff_check=git_check('diff','--check'),status=git_check('status','--short'),index=git_check('diff','--cached','--name-only'))
    assert checks['full_diff_check']['exit_code']==0 and checks['index']['exit_code']==0 and not checks['index']['stdout']
    forbidden={r['path'] for r in entry['external_files']};manifest=load('publication-manifest.json');assert not forbidden & {r['path'] for r in manifest['files']}
    append('03-verification.json',dict(focused_audits=dict(total=44,passed=44,failed=0,errors=0,command=clean(audit),stderr_sha256=sha(raw)),independent_reader=reader,verified_manifest_before_appending_this_receipt=pin(SELF),external_documents_excluded=16,readmes_byte_preserved=15,unchanged_production_test_build_payloads=19,unchanged_validation_fields_except_decision=True,pre_closeout_snapshot_files_verified=len(entry['snapshots']),checks=checks,full_suites='NOT_RERUN_DOCUMENTATION_ONLY; prior actual counts/nonpasses unchanged',final_seal='Manifest resealed after this append; final independent reader and same44 audits rerun without subsequent public mutations.'))
    results=load('audit-results.json');results['documentation_closeout']=dict(version='closeout/v1',total=44,passed=44,failed=0,errors=0,exit_code=0,command=clean(audit),independent_reader=reader,receipt='closeout/v1/03-verification.json');results['qualification']+=' Subsequent owner-authorized documentation closeout is recorded separately; current final readiness is READY and all prior attempts remain unchanged.';save('audit-results.json',results)
    print('Appended44-pass closeout audit, independent reader0, unchanged19 payloads/validation counters;16 external documents excluded')
if __name__=='__main__':
    {'prepare':prepare,'apply':apply,'metadata':metadata,'record':record}[sys.argv[1]]()
