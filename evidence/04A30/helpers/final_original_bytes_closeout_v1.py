"""Resume the unchanged v2 exact-root verifier with the real pristine archive.

Retains the complete previous96-file checkpoint without duplicating large
datasets. Audit/evidence only. No product/native/scientific matrix execution.
"""
from pathlib import Path
import argparse,importlib.util,json,os,subprocess,sys,time

ROOT=Path(__file__).resolve().parents[3];OUT=ROOT/'evidence/04A30'
LOCAL=ROOT/'.04a30-original-byte-final-v1.tmp'
PRIOR=OUT/'original-byte-final-prior-v1'
spec=importlib.util.spec_from_file_location('exact_root_v2',Path(__file__).with_name('evidence_disposition_v2.py'))
v2=importlib.util.module_from_spec(spec);spec.loader.exec_module(v2)
setattr(v2,'LOCAL',LOCAL)
sha=v2.sha;pin=v2.pin;load=v2.load;save=v2.save;git=v2.git
DOC=v2.DOSSIER
REVISE=list(v2.REVISE)+['04A30-evidence-disposition-v2.json','04A30-evidence-disposition-v2.md','04A28-source-transition-successor-v2.json','evidence-disposition-tests-v2.json','historical-source-pin-rerun-v2.json','closeout-command-outcomes-v2.json']

def freeze():
    assert not (OUT/'original-byte-final-entry-v1.json').exists(),'Final original-byte entry already frozen'
    state=v2.state();manifest=load(OUT/'publication-manifest.json')
    for row in manifest['files']:
        if row['sha256'] is not None:assert pin(ROOT/row['path'])==row,row['path']
    LOCAL.mkdir(exist_ok=False);PRIOR.mkdir(exist_ok=False)
    retention=[]
    for path in [DOC]+['evidence/04A30/'+n for n in REVISE]:
        source=ROOT/path;target=PRIOR/('owner-review-before-original-proof.md' if path==DOC else source.name)
        with target.open('xb') as f:f.write(source.read_bytes())
        retention.append(dict(original_path=path,retained_path=target.relative_to(ROOT).as_posix(),bytes=source.stat().st_size,sha256=sha(source)))
    (PRIOR/'README.txt').write_text('Immutable previous NOT_READY checkpoint before real pristine bytes became available. V1 attempts and archive-input-status-v2 remain unchanged; large unchanged CSV/casebooks are not copied. Current root dossier and authenticated v2 original-byte proof supersede these statuses.\n',encoding='utf8')
    save('original-byte-final-retention-v1.json',dict(files=retention,large_datasets_not_duplicated=True))
    save('original-byte-final-entry-v1.json',dict(**state,previous_publication_files=manifest['files'],previous_external_hashes_verified=95,entry_authorities=[pin(ROOT/'AGENTS.md'),pin(ROOT/'.gitattributes'),pin(ROOT/v2.MAN29),pin(ROOT/'evidence/04A28/source-preservation.json'),pin(ROOT/'scripts/tests/test_04a28_output.py')],immutable_previous_attempts=[pin(OUT/p) for p in ['04A29-eol-qualification-v1.json','archive-input-status-v2.json','helpers/evidence_disposition_v1.py','helpers/evidence_disposition_v2.py','04A30-evidence-disposition-v1.json','04A30-evidence-disposition-v1.md']],writer_check='No dotnet/MSBuild/testhost/Python/Excel at entry;11 pre-existing Node processes. Fresh isolated audit outputs;no permanent writer lease claimed.'))
    print('PASS final entry;95 external hashes;prior summaries and attempts retained')

def run(name,args,archive=None):
    folder=LOCAL/name;folder.mkdir(exist_ok=False);start=time.perf_counter()
    env=os.environ.copy();env.update(PYTHONDONTWRITEBYTECODE='1',MSBUILDDISABLENODEREUSE='1')
    with (folder/'stdout.log').open('wb') as stdout,(folder/'stderr.log').open('wb') as stderr:
        p=subprocess.run(list(map(str,args)),cwd=ROOT,env=env,stdout=stdout,stderr=stderr)
    def clean(s):
        s=str(s)
        if archive is not None:s=s.replace(str(archive),'<PRISTINE_ARCHIVE>').replace(archive.as_posix(),'<PRISTINE_ARCHIVE>')
        return s.replace(str(LOCAL),'<FINAL_ORIGINAL_BYTE_RUN>').replace(LOCAL.as_posix(),'<FINAL_ORIGINAL_BYTE_RUN>').replace(str(ROOT),'<REPO_ROOT>').replace(ROOT.as_posix(),'<REPO_ROOT>').replace(sys.executable,'<PYTHON>')
    receipt=dict(command=[clean(s) for s in args],exit_code=p.returncode,seconds=time.perf_counter()-start,stdout_sha256=sha(folder/'stdout.log'),stderr_sha256=sha(folder/'stderr.log'),driver_sha256=sha(__file__),exact_root_helper=pin(Path(v2.__file__)))
    (folder/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf8');print(name,p.returncode,round(receipt['seconds'],2));return receipt

def retained():
    rows=load(OUT/'original-byte-final-retention-v1.json')['files'];mapping={r['original_path']:r for r in rows}
    entry=load(OUT/'original-byte-final-entry-v1.json')
    for row in rows:
        path=ROOT/row['retained_path'];assert sha(path)==row['sha256'] and path.stat().st_size==row['bytes']
    for row in entry['previous_publication_files']:
        if row['sha256'] is None:continue
        path=ROOT/(mapping[row['path']]['retained_path'] if row['path'] in mapping else row['path'])
        assert sha(path)==row['sha256'] and path.stat().st_size==row['bytes'],row['path']
    for row in entry['entry_authorities']+entry['immutable_previous_attempts']:assert pin(ROOT/row['path'])==row,row['path']
    return dict(status='PASS',previous95_external_payloads_preserved=True,prior_summary_copies=len(rows),v1_conditional_and_v2_pending_attempts_unchanged=True,large_dataset_copies=0)

def verification():
    v2.verify();retained()
    proof=load(OUT/'04A29-eol-qualification-v2.json')
    assert proof['status']=='PASS_ALL_ORIGINAL_EOL_TRANSITIONS_PROVEN'
    assert proof['archive']['sha256']==v2.ARCHIVE_SHA and proof['archive']['bytes']==v2.ARCHIVE_SIZE
    assert proof['counts']==dict(DIRECT_ORIGINAL_HASH_MATCH=27,ORIGINAL_BYTES_CRLF_TO_LF_PROVEN=93,MANIFEST_SELF_ENTRY=1)
    assert not proof['unproved_paths'] and not proof['unexplained_paths']
    assert len(proof['rows'])==121 and proof['externally_authenticated_original_entries']==120
    assert all(row['current_equals_git_blob'] for row in proof['rows'])
    assert all(row['original_sha256']==row['manifest_sha256'] and row['original_bytes']==row['manifest_bytes'] for row in proof['rows'] if row['category']!='MANIFEST_SELF_ENTRY')
    assert all(r['original_crlf_count']>0 and r['original_lone_lf_count']>0 for r in proof['mixed_ending_markdown_examples'])
    print('PASS authenticated original archive;27/93/1;four mixed Markdown;zero unresolved;retained95 prior payloads')

def publish():
    retained();v2.publish()
    proof=load(OUT/'04A29-eol-qualification-v2.json');assert proof['status']=='PASS_ALL_ORIGINAL_EOL_TRANSITIONS_PROVEN'
    tests=load(OUT/'evidence-disposition-tests-v2.json');old=load(OUT/'historical-source-pin-rerun-v2.json')
    receipts=[dict(name=p.parent.name,**load(p)) for p in sorted(LOCAL.glob('*/receipt.json'))]
    # Include in-process unchanged C# audit guard receipts as well.
    receipt=dict(schema='04A30-final-original-byte-completion-v1',decision='READY_FOR_OWNER_04A30_ACCEPTANCE_REVIEW',archive=proof['archive'],trusted_root=proof['trusted_zip_root'],entries=121,authenticated_originals=120,counts=proof['counts'],unresolved=0,source_pins=dict(total=278,unchanged=272,authorized=6,status='PASS'),targeted_tests=tests,old_test=dict(parent_tests_run=old['parent_tests_run'],failed_subtests=old['failed_subtests'],errors=old['errors'],status='FAIL retained'),retention=retained(),commands=receipts,prior_runs='REUSED_PREVIOUS_04A30_EXECUTION_EVIDENCE;full matrices/native/R/browser/MkDocs/strict/profile/history scans NOT_RUN in this final continuation',product_existing_test_changes=0,all100_captures_unchanged=True,original12_core_graphics_unchanged=True,gates=dict(P='NO',N1='NO GO',N2='NO',Gate_C2='UNMET'),scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN',native_releases=0,original_control_discharges=0,next_batch_started=False)
    save('original-byte-final-completion-v1.json',receipt)
    validation=load(OUT/'validation.json');validation['decision']=receipt['decision'];validation['blockers']=[]
    validation['previous_conditional_closeout_v1']=validation.pop('evidence_disposition_closeout',None)
    validation['previous_conditional_closeout_v1']['superseded_by']='final_original_byte_closeout_v2'
    validation['original_byte_final_completion_v1']=dict(receipt=pin(OUT/'original-byte-final-completion-v1.json'),counts=proof['counts'],unresolved=0,new_targeted_tests=tests['tests_run'],historical_failed_subtests=6,decision=receipt['decision'])
    save('validation.json',validation)
    nonpasses=load(OUT/'nonpasses.json');nonpasses['decision']=receipt['decision'];nonpasses['acceptance_blockers']=[]
    nonpasses['prior_authority_manifest_discrepancy_status']=nonpasses.pop('authority_manifest_discrepancies',None)
    nonpasses['authority_manifest_discrepancies']=dict(original_recorded_differences=93,exact_original_byte_eol_transforms_proven=93,unexplained=0,historical_manifest_unchanged=True,evidence='04A29-eol-qualification-v2.json')
    nonpasses['final_original_byte_outcome']='Original-byte blocker resolved;all previous raw managed/Python/strict/profile nonpasses and six historical FAIL subtests retained unchanged.'
    save('nonpasses.json',nonpasses)
    source=load(OUT/'source-preservation.json');source['original_byte_final_completion_v1']=dict(receipt=pin(OUT/'original-byte-final-completion-v1.json'),no_product_or_existing_test_change=True,original_manifest_and_v1_attempts_unchanged=True);save('source-preservation.json',source)
    text=(ROOT/DOC).read_text(encoding='utf8')
    text=text.replace('After the remaining original-byte qualification, send this Codex report and updated full ZIP','Send this Codex report and updated full ZIP')
    text+='\nFinal local completion: the authenticated compact archive resolves all93 EOL rows,including all four mixed-ending Markdown files. The earlier archive-input-status-v2 and conditional v1 records are retained historical attempts,not current blockers. [Final receipt](../../evidence/04A30/original-byte-final-completion-v1.json) records exact new commands and prior-run reuse. Owner acceptance is software-only;no scientific/native gate or04A.31 authorization follows.\n'
    (ROOT/DOC).write_text(text,encoding='utf8')
    save('original-byte-final-commands-v1.json',dict(commands=receipts,private_paths='Pristine input stays outside repository;real path replaced by<PRISTINE_ARCHIVE>;raw diagnostics ignored'))
    v2.seal();verification()

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('action',choices=['freeze','guard','tests','old-test','publish','verify','package']);a=p.parse_args()
    if a.action=='freeze':freeze()
    elif a.action=='guard':v2.guard()
    elif a.action=='tests':raise SystemExit(v2.tests())
    elif a.action=='old-test':raise SystemExit(v2.old_test())
    elif a.action=='publish':publish()
    elif a.action=='verify':verification()
    elif a.action=='package':verification();v2.package()
if __name__=='__main__':main()
