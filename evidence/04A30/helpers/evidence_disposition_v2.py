"""04A.30 final original-byte proof. Exact trusted ZIP root; no extraction.

All v1 files remain immutable. No product/existing-test/history writes.
Actions: freeze, qualify --archive ABSOLUTE_PATH, transition, tests, old-test,
guard, publish, verify, package. Full product/scientific/native suites are not run.
"""
from pathlib import Path
import argparse,collections,csv,hashlib,importlib.util,json,os,re,subprocess,sys,time,unittest,zipfile,warnings,tempfile

ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'evidence/04A30'
LOCAL=ROOT/'.04a30-disposition-v2.tmp'
PRIOR=OUT/'closeout-v2-prior'
BASE='45466a88e7567c9f4711ba4fbbe295a7a2e69bb4'
ARCHIVE_NAME='BESHStatNG_04A29_Pristine_Publication_Bytes_For_04A30_20261106.zip'
ARCHIVE_SHA='d14b960f974a10831474892c60bee63824d02eb864fb58b93aefdc41dcfaec66'
ARCHIVE_SIZE=3474560
SOURCE_ZIP_SHA='270f188b79f1ce339696d1439ac44ef77454e44d8b2beccfb6e1a00230a0c4d6'
ZIP_ROOT='PRISTINE_04A29_ORIGINALS/'
MAN29='evidence/04A29/publication-manifest.json'
DOSSIER='docs/architecture/batch-04A-30-owner-review.md'
REVISE=('validation.json','nonpasses.json','source-preservation.json','privacy.json','schema-validation.json','final-git-state.json','publication-manifest.json','README.json','source-ownership-guard.json')
spec=importlib.util.spec_from_file_location('retained_v1',Path(__file__).with_name('evidence_disposition_v1.py'))
v1=importlib.util.module_from_spec(spec);spec.loader.exec_module(v1)
sha=v1.sha;digest=v1.digest;pin=v1.pin;load=v1.load;git=v1.git;state=v1.state
def save(name,value):
    p=OUT/name;p.parent.mkdir(parents=True,exist_ok=True)
    p.write_text(json.dumps(value,ensure_ascii=False,allow_nan=False,indent=2)+'\n',encoding='utf8')
def safe_pin(path):
    p=Path(path);return dict(basename=p.name,bytes=p.stat().st_size,sha256=sha(p))

def normalized_member(name):
    """Canonical relative member name, with duplicate detection after slash normalization."""
    assert isinstance(name,str) and name and '\x00' not in name,'Invalid ZIP member name'
    name=name.replace('\\','/')
    assert not name.startswith('/') and ':' not in name,'Absolute/drive ZIP path rejected'
    directory=name.endswith('/')
    body=name[:-1] if directory else name
    assert body and all(part not in ('','.','..') for part in body.split('/')),'Traversal/noncanonical ZIP path rejected'
    return body+('/' if directory else '')
def trusted_root(root):
    if root=='':return root
    assert root.endswith('/') and '\\' not in root,'ZIP root must be explicit canonical prefix ending in slash'
    assert normalized_member(root)==root
    return root
def archive_entries(path,manifest,root):
    """No suffix search, discovery, first-match fallback or extraction.

    All names are validated, including auxiliary members. Duplicates after slash
    normalization, directory/file collisions and case ambiguity are rejected.
    The supplied frozen manifest path is matched only at root+relative exactly.
    """
    root=trusted_root(root)
    with zipfile.ZipFile(path) as archive:
        members={};casefold={}
        for info in archive.infolist():
            canonical=normalized_member(info.filename);key=canonical.rstrip('/')
            assert key not in members,'Duplicate normalized exact ZIP member'
            assert key.casefold() not in casefold,'Case-ambiguous ZIP members rejected'
            members[key]=(canonical,info);casefold[key.casefold()]=key
        originals={};seen=set()
        for row in manifest['files']:
            relative=row['path']
            assert normalized_member(relative)==relative and not relative.endswith('/'),'Noncanonical manifest path'
            assert relative not in seen,'Duplicate manifest path';seen.add(relative)
            expected=root+relative
            assert expected in members,'Missing exact trusted-root ZIP member: '+relative
            canonical,info=members[expected]
            assert canonical==expected and not info.is_dir(),'Expected archive file, not directory'
            original=archive.read(info)
            if row['sha256'] is not None:
                assert len(original)==row['bytes'] and digest(original)==row['sha256'],'Original manifest SHA/size mismatch: '+relative
            else:assert relative==MAN29,'Unexpected null historical manifest entry'
            originals[relative]=original
        auxiliary=[]
        for canonical,info in members.values():
            if canonical.startswith('__handoff__/') and not info.is_dir():
                b=archive.read(info)
                # Auxiliary provenance is inspected but never substituted for
                # frozen per-file hashes or original source-ZIP inspection.
                text=b.decode('utf-8-sig',errors='replace')
                auxiliary.append(dict(path=canonical,bytes=len(b),sha256=digest(b),accepted_source_zip_hash_reference_present=SOURCE_ZIP_SHA in text))
        return originals,auxiliary

def authenticate_archive(path):
    assert path.is_absolute() and path.is_file(),'Actual local absolute archive path required'
    assert not path.resolve().is_relative_to(ROOT.resolve()),'Pristine archive must stay outside repository'
    assert path.name==ARCHIVE_NAME,'Unexpected supplied archive basename'
    check_archive_fingerprint(path.stat().st_size,sha(path))
    return safe_pin(path)
def check_archive_fingerprint(size,value):
    assert size==ARCHIVE_SIZE and value==ARCHIVE_SHA,'Supplied archive SHA/size mismatch'
def frozen_manifest():
    v1.check_manifest((ROOT/MAN29).read_bytes());return load(ROOT/MAN29)
def freeze():
    assert not (OUT/'closeout-entry-v2.json').exists(),'v2 entry already frozen'
    before=state();manifest=load(OUT/'publication-manifest.json')
    for row in manifest['files']:
        if row['sha256'] is not None:assert pin(ROOT/row['path'])==row,row['path']
    PRIOR.mkdir(exist_ok=False);LOCAL.mkdir(exist_ok=False)
    kept=[]
    for relative in [DOSSIER]+['evidence/04A30/'+n for n in REVISE]:
        source=ROOT/relative;target=PRIOR/('owner-review-pre-v2.md' if relative==DOSSIER else source.name)
        with target.open('xb') as f:f.write(source.read_bytes())
        kept.append(dict(original_path=relative,retained_path=target.relative_to(ROOT).as_posix(),bytes=source.stat().st_size,sha256=sha(source)))
    (PRIOR/'README.txt').write_text('Immutable pre-v2 summaries. Conditional v1 proof and helper remain at original paths unchanged. These retained decisions are historical; root dossier and v2 receipts are current. Large unchanged datasets are not duplicated.\n',encoding='utf8')
    save('closeout-retention-v2.json',dict(files=kept))
    save('closeout-entry-v2.json',dict(**before,publication_files=manifest['files'],prior_external_hashes_verified=74,historical_authority=[pin(ROOT/MAN29),pin(ROOT/'evidence/04A28/source-preservation.json'),pin(ROOT/'scripts/tests/test_04a28_output.py'),pin(ROOT/'AGENTS.md'),pin(ROOT/'.gitattributes'),pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md')],v1_immutable_files=[pin(OUT/n) for n in ['04A29-eol-qualification-v1.json','04A28-source-transition-successor-v1.json','04A30-evidence-disposition-v1.json','04A30-evidence-disposition-v1.md','historical-source-pin-rerun-v1.json','source-transition-tests-v1.json','helpers/evidence_disposition_v1.py']],writer_check='No build/test/Python/Excel processes at entry;11 pre-existing Node processes. Isolated audit helper outputs;no global writer lease assumed.',supplied_archive=dict(basename=ARCHIVE_NAME,expected_bytes=ARCHIVE_SIZE,expected_sha256=ARCHIVE_SHA,accepted_source_zip_sha256=SOURCE_ZIP_SHA),scope='Audit helper/evidence only;no product/existing test changes or full matrices'))
    print('PASS v2 entry freeze;74 prior payload hashes;10 exact retained summaries')

def qualify(path,root=ZIP_ROOT):
    state();manifest=frozen_manifest();archive_pin=authenticate_archive(path)
    originals,auxiliary=archive_entries(path,manifest,root)
    rows=[]
    for old in manifest['files']:
        relative=old['path'];original=originals[relative];current=(ROOT/relative).read_bytes();blob=git('show',BASE+':'+relative,raw=True)
        assert current==blob,'Historical worktree drift: '+relative
        if old['sha256'] is None:category='MANIFEST_SELF_ENTRY'
        elif current==original:category='DIRECT_ORIGINAL_HASH_MATCH'
        else:
            assert original.replace(b'\r\n',b'\n')==blob,'Unexplained non-EOL byte difference: '+relative
            category='ORIGINAL_BYTES_CRLF_TO_LF_PROVEN'
        rows.append(dict(path=relative,category=category,exact_archive_member=root+relative,original_bytes_inspected=True,original_bytes=len(original),original_sha256=digest(original),manifest_bytes=old['bytes'],manifest_sha256=old['sha256'],original_crlf_count=original.count(b'\r\n'),original_lone_lf_count=original.count(b'\n')-original.count(b'\r\n'),current_bytes=len(current),current_sha256=digest(current),git_blob_oid=git('rev-parse',BASE+':'+relative),git_blob_sha256=digest(blob),current_equals_git_blob=True,relation='original.replace(CRLF,LF)==current Git blob' if category=='ORIGINAL_BYTES_CRLF_TO_LF_PROVEN' else 'direct bytes' if category=='DIRECT_ORIGINAL_HASH_MATCH' else 'self entry;null frozen hash retained'))
    counts=dict(collections.Counter(r['category'] for r in rows))
    assert len(rows)==121 and counts==dict(DIRECT_ORIGINAL_HASH_MATCH=27,ORIGINAL_BYTES_CRLF_TO_LF_PROVEN=93,MANIFEST_SELF_ENTRY=1)
    mixed_paths=['evidence/04A29/04A29-lineage-correction.md','evidence/04A29/README.md','evidence/04A29/superseded/v1-attempt/evidence/04A29/README.md','evidence/04A29/superseded/v1-attempt/owner-dossier-v1.md']
    mixed=[next(r for r in rows if r['path']==p) for p in mixed_paths]
    assert all(r['category']=='ORIGINAL_BYTES_CRLF_TO_LF_PROVEN' and r['original_crlf_count'] and r['original_lone_lf_count'] for r in mixed)
    result=dict(schema='04A30-04A29-eol-qualification-v2',status='PASS_ALL_ORIGINAL_EOL_TRANSITIONS_PROVEN',archive=archive_pin,trusted_zip_root=root,accepted_full_source_zip_sha256=SOURCE_ZIP_SHA,full_source_zip_inspected=False,full_source_zip_provenance='Owner supplied accepted original full source ZIP hash;compact ZIP authenticated locally. Full source ZIP itself not claimed inspected.',auxiliary_metadata=auxiliary,manifest_pin=pin(ROOT/MAN29),entries=121,externally_authenticated_original_entries=120,counts=counts,unproved_paths=[],unexplained_paths=[],mixed_ending_markdown_examples=mixed,rows=rows,gitattributes=pin(ROOT/'.gitattributes'),scientific='NOT_GOLDEN;byte-only proof,no scientific equivalence claim',previous_conditional_attempt=pin(OUT/'04A29-eol-qualification-v1.json'))
    save('04A29-eol-qualification-v2.json',result);print('PASS pristine original bytes;27 direct/93 exact CRLF-to-LF/1 self;0 unresolved');return result

def unchanged_current_pinset(expected,actual):
    assert set(expected)==set(actual),'Missing/foreign current product/test file'
    assert expected==actual,'Unauthorized product/existing test/project change in final closeout'
def source_transition():
    state();old,current,changes=v1.payload();v1.check_pins(old,current,changes)
    v1.guard_contract(load(OUT/'source-ownership-guard.json'),changes)
    assert all(r['old_sha256']==r['entry04A29_sha256']==r['entry04A30_sha256'] for r in changes.values())
    protection=v1.protection()
    entry=load(OUT/'closeout-entry-v2.json')
    expected={r['path']:r['sha256'] for r in entry['publication_files'] if r['path'].endswith(('.vb','.vbproj','.cs','.csproj'))}
    actual={p:sha(ROOT/p) for p in expected};unchanged_current_pinset(expected,actual)
    for r in entry['historical_authority']+entry['v1_immutable_files']:assert pin(ROOT/r['path'])==r,r['path']
    original=v1.load(OUT/'04A28-source-transition-successor-v1.json')
    assert original['total_old_pins']==278 and original['unchanged']==272 and original['authorized_graphics_transitions']==6
    return dict(status='PASS',total_old_pins=278,unchanged=272,authorized_graphics_transitions=6,transitions=list(changes.values()),original38_compiled_graphics=True,original12_core_graphics_unchanged=True,historical171_172_uncertainties_retained=True,source_guard=pin(OUT/'source-ownership-guard.json'),v1_successor_preserved=pin(OUT/'04A28-source-transition-successor-v1.json'),framework_test_project='Separate previously accepted inclusion change;exact current hash frozen and protected,not seventh Graphics transition',protection=protection,final_closeout_product_existing_test_changes=0,native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN')

class ExactArchiveTests(unittest.TestCase):
    def make(self,entries):
        self.addCleanup(lambda:None)
        path=LOCAL/('fixture-'+self._testMethodName+'.zip')
        with warnings.catch_warnings():
            warnings.simplefilter('ignore',UserWarning)
            with zipfile.ZipFile(path,'w') as z:
                for name,data in entries:z.writestr(name,data)
        return path
    def manifest(self,entries):return dict(files=[dict(path=p,bytes=len(b),sha256=digest(b)) for p,b in entries])
    def test_duplicate_suffix_trap_is_resolved_by_exact_root(self):
        a='evidence/04A29/README.md';b='evidence/04A29/superseded/v1-attempt/evidence/04A29/README.md'
        pairs=[(a,b'current\r\n'),(b,b'original\n')]
        result,_=archive_entries(self.make([(ZIP_ROOT+p,b) for p,b in pairs]),self.manifest(pairs),ZIP_ROOT)
        self.assertEqual(dict(pairs),result)
    def test_bad_original_sha_rejected(self):
        m=self.manifest([('a.txt',b'correct')])
        with self.assertRaises(AssertionError):archive_entries(self.make([(ZIP_ROOT+'a.txt',b'changed')]),m,ZIP_ROOT)
    def test_missing_exact_entry_rejected(self):
        with self.assertRaises(AssertionError):archive_entries(self.make([(ZIP_ROOT+'other/a.txt',b'x')]),self.manifest([('a.txt',b'x')]),ZIP_ROOT)
    def test_duplicate_exact_path_rejected(self):
        with self.assertRaises(AssertionError):archive_entries(self.make([(ZIP_ROOT+'a.txt',b'x'),(ZIP_ROOT+'a.txt',b'x')]),self.manifest([('a.txt',b'x')]),ZIP_ROOT)
    def test_duplicate_normalized_path_rejected(self):
        with self.assertRaises(AssertionError):archive_entries(self.make([(ZIP_ROOT+'a.txt',b'x'),(ZIP_ROOT.replace('/','\\')+'a.txt',b'x')]),self.manifest([('a.txt',b'x')]),ZIP_ROOT)
    def test_case_ambiguity_rejected(self):
        with self.assertRaises(AssertionError):archive_entries(self.make([(ZIP_ROOT+'a.txt',b'x'),(ZIP_ROOT+'A.txt',b'x')]),self.manifest([('a.txt',b'x')]),ZIP_ROOT)
    def test_traversal_rejected(self):
        with self.assertRaises(AssertionError):archive_entries(self.make([('../a.txt',b'x')]),self.manifest([('a.txt',b'x')]),ZIP_ROOT)
    def test_absolute_path_rejected(self):
        with self.assertRaises(AssertionError):archive_entries(self.make([('/a.txt',b'x')]),self.manifest([('a.txt',b'x')]),ZIP_ROOT)
    def test_mixed_line_endings_forward_proof(self):
        original=b'first\r\nsecond\nthird\r\n'
        result,_=archive_entries(self.make([(ZIP_ROOT+'mixed.md',original)]),self.manifest([('mixed.md',original)]),ZIP_ROOT)
        self.assertEqual(original,result['mixed.md']);self.assertEqual(b'first\nsecond\nthird\n',result['mixed.md'].replace(b'\r\n',b'\n'))
    def test_no_root_discovery(self):
        with self.assertRaises(AssertionError):archive_entries(self.make([('other/a.txt',b'x')]),self.manifest([('a.txt',b'x')]),'')
    def test_explicit_empty_repository_root(self):
        result,_=archive_entries(self.make([('a.txt',b'x')]),self.manifest([('a.txt',b'x')]),'');self.assertEqual(b'x',result['a.txt'])
    def test_bad_archive_container_sha_rejected(self):
        with self.assertRaisesRegex(AssertionError,'SHA/size mismatch'):check_archive_fingerprint(ARCHIVE_SIZE,'0'*64)
    def test_278_272_6_and_all_protections(self):self.assertEqual('PASS',source_transition()['status'])
    def test_seventh_changed_source_rejected(self):
        old,current,changes=v1.payload();p=next(p for p in old if p not in v1.APPROVED);current[p]='0'*64
        with self.assertRaises(AssertionError):v1.check_pins(old,current,changes)
    def test_swapped_legacy_new_sha_rejected(self):
        old,current,changes=v1.payload();r=changes[v1.APPROVED[0]];r['old_sha256'],r['new_sha256']=r['new_sha256'],r['old_sha256']
        with self.assertRaises(AssertionError):v1.check_pins(old,current,changes)
    def test_altered_frozen_manifest_rejected(self):
        with self.assertRaises(AssertionError):v1.check_manifest((ROOT/MAN29).read_bytes()+b'\n')
    def test_missing_core_delegation_rejected(self):
        _,_,changes=v1.payload();guard=load(OUT/'source-ownership-guard.json');guard['current_members']=[r for r in guard['current_members'] if r['qualified_type']!='BESHStatNG.RocDataCore']
        with self.assertRaises(AssertionError):v1.guard_contract(guard,changes)
    def test_unauthorized_test_project_change_rejected(self):
        p='BESHStatNG.Test/BESHStatNG.Tests.vbproj';expected={p:sha(ROOT/p)}
        with self.assertRaises(AssertionError):unchanged_current_pinset(expected,{p:'0'*64})
    def test_unauthorized_existing_test_change_rejected(self):
        p='scripts/tests/test_04a28_output.py';expected={p:sha(ROOT/p)}
        with self.assertRaises(AssertionError):unchanged_current_pinset(expected,{p:'0'*64})

def tests():
    suite=unittest.defaultTestLoader.loadTestsFromTestCase(ExactArchiveTests);ids=[t.id() for t in suite]
    result=unittest.TextTestRunner(verbosity=2).run(suite)
    save('evidence-disposition-tests-v2.json',dict(status='PASS' if result.wasSuccessful() else 'FAIL',tests_run=result.testsRun,failures=len(result.failures),errors=len(result.errors),skipped=len(result.skipped),test_identities=ids,helper_pin=pin(Path(__file__))))
    return 0 if result.wasSuccessful() else 1
def old_test():
    spec=importlib.util.spec_from_file_location('historical_output28',ROOT/'scripts/tests/test_04a28_output.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
    suite=unittest.defaultTestLoader.loadTestsFromName('Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved',m)
    result=unittest.TextTestRunner(verbosity=2).run(suite)
    assert result.testsRun==1 and len(result.failures)==6 and not result.errors
    rows=[]
    for case,trace in result.failures:
        match=re.search(r"path='([^']+)'",str(case));assert match
        rows.append(dict(path=match.group(1),outcome='FAIL',cause='AUTHORIZED_VERSIONED_SOURCE_TRANSITION_04A30',failure_text_sha256=digest(trace.encode())))
    assert {r['path'] for r in rows}==set(v1.APPROVED)
    save('historical-source-pin-rerun-v2.json',dict(status='FAIL_EXPECTED_SIX_HISTORICAL_SUBTESTS',parent_tests_run=1,failed_subtests=6,errors=0,subtests=rows,unchanged_test_pin=pin(ROOT/'scripts/tests/test_04a28_output.py'),v1_rerun_preserved=pin(OUT/'historical-source-pin-rerun-v1.json'),failure_totals_not_subtracted=True));return 1

def command(name,args):
    folder=LOCAL/name;folder.mkdir(exist_ok=False);start=time.perf_counter()
    env=os.environ.copy();env.update(PYTHONDONTWRITEBYTECODE='1',MSBUILDDISABLENODEREUSE='1')
    with (folder/'stdout.log').open('wb') as out,(folder/'stderr.log').open('wb') as err:p=subprocess.run(list(map(str,args)),cwd=ROOT,env=env,stdout=out,stderr=err)
    def clean(s):return str(s).replace(str(LOCAL),'<FINAL_EVIDENCE_RUN>').replace(LOCAL.as_posix(),'<FINAL_EVIDENCE_RUN>').replace(str(ROOT),'<REPO_ROOT>').replace(ROOT.as_posix(),'<REPO_ROOT>').replace(sys.executable,'<PYTHON>')
    receipt=dict(command=[clean(s) for s in args],exit_code=p.returncode,seconds=time.perf_counter()-start,stdout_sha256=sha(folder/'stdout.log'),stderr_sha256=sha(folder/'stderr.log'),helper_sha256=sha(__file__))
    (folder/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf8');print(name,p.returncode,round(receipt['seconds'],2));return receipt
def guard():
    assets=ROOT/'.04a29-audit.tmp/obj/project.assets.json';assert assets.is_file()
    assert command('ast-build',['dotnet','build','evidence/04A30/helpers/Graphics30Guard.csproj','--no-restore','-p:ProjectAssetsFile='+str(assets),'-p:BaseIntermediateOutputPath='+str(LOCAL/'guard-obj')+'/', '-p:OutputPath='+str(LOCAL/'guard-bin')+'/', '--nologo','--verbosity','quiet'])['exit_code']==0
    assert command('ast-guard',['dotnet',LOCAL/'guard-bin/Graphics30Guard.dll'])['exit_code']==0

def retained():
    v1.retained()
    entry=load(OUT/'closeout-entry-v2.json');rows=load(OUT/'closeout-retention-v2.json')['files'];mapping={r['original_path']:r for r in rows}
    for r in rows:
        p=ROOT/r['retained_path'];assert p.stat().st_size==r['bytes'] and sha(p)==r['sha256']
    for r in entry['publication_files']:
        if r['sha256'] is None:continue
        p=ROOT/(mapping[r['path']]['retained_path'] if r['path'] in mapping else r['path'])
        assert p.stat().st_size==r['bytes'] and sha(p)==r['sha256'],r['path']
    for r in entry['v1_immutable_files']:assert pin(ROOT/r['path'])==r,r['path']
    return dict(status='PASS',prior_v2_summary_copies=10,prior74_external_payloads_preserved=True,v1_attempt_files_unchanged=True,large_CSV_casebook_copies=0)
def current_paths():
    paths=sorted(set(git('diff','--name-only').splitlines())|set(git('ls-files','--others','--exclude-standard').splitlines()))
    allowed={r['path'] for r in load(OUT/'closeout-entry-v2.json')['publication_files']}
    assert all(p in allowed or p.startswith('evidence/04A30/') for p in paths),'Unexpected publication path'
    return paths
def receipts():return [dict(name=p.parent.name,**load(p),raw_receipt_sha256=sha(p)) for p in sorted(LOCAL.glob('*/receipt.json'))]

def publish():
    state();retention=retained();protection=source_transition()
    tests=load(OUT/'evidence-disposition-tests-v2.json');old=load(OUT/'historical-source-pin-rerun-v2.json')
    assert tests['status']=='PASS' and tests['helper_pin']==pin(Path(__file__))
    assert old['failed_subtests']==6 and old['parent_tests_run']==1 and old['errors']==0
    proof_path=OUT/'04A29-eol-qualification-v2.json'
    if proof_path.exists():
        proof=load(proof_path);assert proof['status']=='PASS_ALL_ORIGINAL_EOL_TRANSITIONS_PROVEN'
        assert proof['counts']==dict(DIRECT_ORIGINAL_HASH_MATCH=27,ORIGINAL_BYTES_CRLF_TO_LF_PROVEN=93,MANIFEST_SELF_ENTRY=1)
        assert not proof['unproved_paths'] and not proof['unexplained_paths']
        decision='READY_FOR_OWNER_04A30_ACCEPTANCE_REVIEW';blockers=[]
    else:
        previous=load(OUT/'04A29-eol-qualification-v1.json')
        # Recheck current-to-main identities; no original inspection is implied.
        for r in previous['rows']:assert (ROOT/r['path']).read_bytes()==git('show',BASE+':'+r['path'],raw=True)
        proof=dict(status='NOT_RUN_INPUT_UNAVAILABLE',expected_archive=dict(basename=ARCHIVE_NAME,expected_bytes=ARCHIVE_SIZE,expected_sha256=ARCHIVE_SHA),archive_inspected=False,counts=dict(DIRECT_ORIGINAL_HASH_MATCH=27,ORIGINAL_BYTES_CRLF_TO_LF_PROVEN=0,MANIFEST_SELF_ENTRY=1,ORIGINAL_BYTES_UNAVAILABLE_NOT_PROVEN=93),unproved_paths=previous['unresolved_paths'],search_scope=['Repository/parent roots','Downloads tree','Desktop tree','Documents depth2','Codex temporary directories depth4'],source='Owner supplied expected archive identity; real local path/download URL not available. No fabricated archive authentication.')
        save('archive-input-status-v2.json',proof)
        decision='NOT_READY';blockers=[dict(issue='PRISTINE_ARCHIVE_NOT_AVAILABLE_LOCALLY',owner='Owner/input transfer',expected_basename=ARCHIVE_NAME,unproved_paths=proof['unproved_paths'],action='Provide actual absolute outside-repository archive path or download URL; then run v2 qualify against exact trusted root.')]
    save('04A28-source-transition-successor-v2.json',protection)
    reused=[]
    for name in ['managed-validation-v1.json','focused-validation-v1.json','source-and-node-validation-v1.json','command-outcomes.json','historical-results-preservation.json']:
        reused.append(dict(status='REUSED_PREVIOUS_04A30_EXECUTION_EVIDENCE',evidence=pin(OUT/name)))
    disposition=dict(schema='04A30-final-original-byte-disposition-v2',decision=decision,branch='codex/04A-30',base=BASE,head=BASE,index='EMPTY',archive_proof=proof,source_transition=pin(OUT/'04A28-source-transition-successor-v2.json'),new_targeted_tests=tests,historical_parent_rerun=old,source_ast_guard=pin(OUT/'source-ownership-guard.json'),prior_execution_evidence=reused,not_run_in_final_closeout=['Full Debug/Release managed','Node/full Python scripts','MkDocs/strict/profile/scientific tests','Native Excel/COM/UIA/RefEdit/charts','Independent R','Browser/Office/Mac E2E','Historical138GB stat/payload scan'],retention=retention,commands=receipts(),blockers=blockers,gates=dict(P='NO',N1='NO GO',N2='NO',Gate_C2='UNMET'),scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN',native_releases=0,original_control_discharges=0,product_or_existing_test_changes_in_final_closeout=0,office_gui_activation=False,next_batch_started=False)
    save('04A30-evidence-disposition-v2.json',disposition)
    save('closeout-command-outcomes-v2.json',dict(commands=receipts(),helper_pin=pin(Path(__file__)),raw_logs='Ignored <FINAL_EVIDENCE_RUN>; no private absolute paths published'))
    archive_line=(f"Supplied compact archive authenticated locally:SHA256 `{ARCHIVE_SHA}`,bytes {ARCHIVE_SIZE}. All120 original SHA/size pins authenticate before classification;27 direct/93 exact original.replace(CRLF,LF) transforms/1 self;zero unproved. Four mixed-ending Markdown examples are included. Full original ZIP hash `{SOURCE_ZIP_SHA}` is owner-provided provenance; the full ZIP itself was not claimed inspected." if decision.startswith('READY') else f"Pristine archive `{ARCHIVE_NAME}` is not available at a real local path or download URL. Expected SHA `{ARCHIVE_SHA}` and size {ARCHIVE_SIZE} are owner-supplied,**not locally verified**. All121 current entries still match accepted-main blobs and27 match original hashes directly;93 original-byte transforms remain unproved. No v2 original-byte proof is fabricated. V1 conditional evidence remains byte-identical.")
    text=f'''# 04A.30 final original-byte disposition v2

**{decision} — software extraction only.**

{archive_line}

The v1 suffix matcher is retained unchanged as historical evidence. V2 requires an explicit trusted root (`PRISTINE_04A29_ORIGINALS/` for the supplied compact archive) and exact canonical names. It never searches by suffix/filename or discovers a root. Traversal/absolute paths,duplicate exact or slash-normalized members,directory/file collisions and case ambiguity are rejected before reading selected members. Auxiliary handoff metadata is inspected only as provenance; frozen04A.29 SHA/byte pins remain authority.

New helper tests:{tests['tests_run']} PASS,0 FAIL,0 ERROR,0 SKIP. They cover the nested duplicate-suffix trap,bad original/container SHA,missing/exact duplicate/normalized duplicate/case/traversal/absolute members,mixed endings,explicit empty root,no root discovery,source/test-project mutation,278/272/6 and missing delegation. Source/AST/protection checks pass:38 Windows sources,12 original Core graphics files,100 captures and171/172 uncertainty preserved. No product/existing test/history change occurred.

The historical04A.28 test was rerun unchanged:one failing parent with **six FAIL subtests**. Cause remains `AUTHORIZED_VERSIONED_SOURCE_TRANSITION_04A30` with a passing278/272/6 successor; no raw failure was subtracted or turned green.

Previous product results are reused with exact evidence hashes:Debug/Release managed19,930 PASS/14 inherited FAIL each;Node916 PASS;original full Python838/108/10,later qualified840/106/10;strict/profile1/1,MkDocs0. None of those suites,native/R/browser tests or historical138GB payload scans were repeated. New execution is limited to audit helper/schema/source checks and the old six-failure parent. Source row/numerical/native policies remain untouched.

All previous attempt files,including v1 helper and conditional EOL receipt,remain unchanged. Ten pre-v2 summaries are retained exactly;large unchanged CSV/casebooks are not duplicated. Canonical owner report remains `docs/architecture/batch-04A-30-owner-review.md`;current publication is `evidence/04A30/publication-manifest.json`.

P=NO;N1=NO GO;N2=NO;Gate C2=UNMET;NOT_GOLDEN;NATIVE_NOT_RUN;zero native releases/discharges;no Office/Mac GUI;no04A.31;no stage/commit/merge/push/deploy/sign. Independent owner acceptance remains separate.
'''
    (OUT/'04A30-evidence-disposition-v2.md').write_text(text,encoding='utf8')
    validation=load(PRIOR/'validation.json');validation['decision']=decision;validation['blockers']=blockers
    validation['final_original_byte_closeout_v2']=dict(decision=decision,archive_proof_status=proof['status'],counts=proof['counts'],targeted_tests=tests['tests_run'],new_failed_tests=0,historical_parent_tests_run=1,historical_failed_subtests=6,reused_execution_evidence=reused,not_run=disposition['not_run_in_final_closeout'],receipt='04A30-evidence-disposition-v2.json')
    save('validation.json',validation)
    nonpasses=load(PRIOR/'nonpasses.json');nonpasses['decision']=decision;nonpasses['acceptance_blockers']=blockers
    nonpasses['final_v2']=dict(archive=proof['status'],counts=proof['counts'],new_targeted_tests_passed=tests['tests_run'],historical_failed_subtests=old['subtests'],old_raw_totals_preserved=True,prior_matrices='REUSED_PREVIOUS_04A30_EXECUTION_EVIDENCE;not rerun')
    save('nonpasses.json',nonpasses)
    source=load(PRIOR/'source-preservation.json');source['final_v2']=dict(prior=pin(PRIOR/'source-preservation.json'),current_successor=pin(OUT/'04A28-source-transition-successor-v2.json'),new_product_existing_test_changes=0,v1_and_historical_files_unchanged=True,all100_captures_unchanged=True,historical_results='Prior metadata proof reused; no new scan/rehash')
    save('source-preservation.json',source)
    previous=(PRIOR/'owner-review-pre-v2.md').read_text(encoding='utf8');body=previous[previous.index('## Entry, authority and publication'):]
    intro=f'''# Batch 04A.30 owner review

**{decision} — final original-byte evidence closeout;software extraction only.**

{archive_line}

The v2 exact-root helper corrects the suffix-matching defect;v1 is retained unchanged. New targeted tests:{tests['tests_run']} PASS. Source successor278/272/6 and AST/protection guards PASS. Historical six FAIL subtests remain FAIL. No product or existing test was changed. All previous product runs below are **REUSED_PREVIOUS_04A30_EXECUTION_EVIDENCE**,not new passes;full suites/native/R/browser/history scans are NOT_RUN here.

Current authority:[v2 disposition](../../evidence/04A30/04A30-evidence-disposition-v2.md),[machine receipt](../../evidence/04A30/04A30-evidence-disposition-v2.json),[v2 tests](../../evidence/04A30/evidence-disposition-tests-v2.json),[source successor](../../evidence/04A30/04A28-source-transition-successor-v2.json),[historical FAIL rerun](../../evidence/04A30/historical-source-pin-rerun-v2.json). Previous v1 conditional records remain historical and are not contradictory current decisions.

'''
    if decision.startswith('READY'):intro+='Exact original-byte proof:[04A29-eol-qualification-v2.json](../../evidence/04A30/04A29-eol-qualification-v2.json). No readiness blocker remains for software-only owner review. Scientific/native gates and full-matrix inherited failures remain unchanged.\n\n'
    else:intro+='Remaining blocker:[archive-input-status-v2.json](../../evidence/04A30/archive-input-status-v2.json);provide actual archive path/download URL to finish local proof.\n\n'
    (ROOT/DOSSIER).write_text(intro+body,encoding='utf8')
    readme=load(PRIOR/'README.json');readme['decision']=decision;readme['current_closeout']='04A30-evidence-disposition-v2.json';readme['verification']='helpers/evidence_disposition_v2.py verify;unchanged finalize.py verify supported after seal';save('README.json',readme)
    seal()

def seal():
    paths=current_paths()
    spec=importlib.util.spec_from_file_location('privacy14',ROOT/'scripts/verify-04a14-publication.py');policy=importlib.util.module_from_spec(spec);spec.loader.exec_module(policy)
    for p in paths:
        result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','--no-index',p],cwd=ROOT,stdout=subprocess.PIPE,stderr=subprocess.PIPE);assert result.returncode==1,('Ignored candidate',p)
    findings,reviewed=policy.scan(paths);assert not findings,findings
    save('privacy.json',dict(status='PASS',version=2,candidate_count=len(paths),findings=0,policy='Accepted comprehensive14 scanner;decoded JSON/base64/NFKC/owner/host/SID/credential/drive/UNC checks',v1_and_pristine_paths='No private absolute paths;original archive stays outside repo;no extraction',raw_logs='<FINAL_EVIDENCE_RUN> ignored'))
    save('schema-validation.json',dict(status='PASS',version=2,checks=['Exact trusted-root lookup;no suffix/root discovery;duplicates/traversal/case rejected','Archive authentication and120 original SHA/size checks required before transform classification','Expected27/93/1 accepted only after actual original bytes;missing input staysNOT_READY','278/272/6 and no source/test/project changes','All previous v1 attempt pins and74 prior publication payloads preserved','100 captures and all historical authority unchanged','Old six failures retained;prior full suites reused;current targeted tests explicit','Publication self rule/privacy/trackability/empty index/diff']))
    save('final-git-state.json',dict(**state(),status_short=git('status','--short').splitlines(),publication_paths=current_paths(),git_publication=False))
    paths=current_paths();name='evidence/04A30/publication-manifest.json'
    save('publication-manifest.json',dict(schema='04A30-publication-final-v2',self_rule='Self included withbytes/sha256 null;no recursive hash',files=[dict(path=p,bytes=None,sha256=None) if p==name else pin(ROOT/p) for p in paths]))
    findings,_=policy.scan(current_paths());assert not findings,findings
    verify()
def verify():
    state();retained();source_transition();manifest=load(OUT/'publication-manifest.json');paths=current_paths()
    assert [r['path'] for r in manifest['files']]==paths and len(paths)==len(set(paths))
    for r in manifest['files']:
        if r['path']=='evidence/04A30/publication-manifest.json':assert r['bytes'] is r['sha256'] is None
        else:assert pin(ROOT/r['path'])==r,r['path']
    subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE)
    print('PASS finalv2 manifest/source/history/retention/schema/index/diff;',len(paths),'files')

def package():
    """Full tracked solution plus every proposed changed/new review file.

    Ignored packages/builds/rawlogs/historical TestResults payloads are retained
    in place and excluded. This source/evidence snapshot is never a release.
    """
    verify();members=sorted(set(git('ls-files').splitlines())|set(current_paths()))
    # Never include signing material in a review delivery; fail visibly rather
    # than silently drop a tracked item from the requested solution snapshot.
    assert not any(Path(p).suffix.lower() in ('.pfx','.p12','.key') for p in members),'Tracked private signing material needs owner handling'
    destination=LOCAL/'BESHStatNG_04A30_FullSolution_Review.zip';assert not destination.exists()
    records=[];computer=os.environ.get('COMPUTERNAME','!unset!').casefold()
    def public_name(p):
        if computer in p.casefold() or re.search(r'DESKTOP-|LAPTOP-|S-1-5-',p,re.I):return '<PRIVATE_PROTECTED_PATH:'+digest(p.encode())[:24]+'>'
        return p
    with zipfile.ZipFile(destination,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6) as z:
        for p in members:
            b=(ROOT/p).read_bytes();z.writestr(p,b);records.append(dict(path=public_name(p),bytes=len(b),sha256=digest(b)))
        index=dict(scope='Full tracked working-tree solution and all proposed source/evidence;ignored dependency/build/rawlog/history payloads excluded unchanged;not a native release',decision=load(OUT/'04A30-evidence-disposition-v2.json')['decision'],base=BASE,publication_manifest_sha256=sha(OUT/'publication-manifest.json'),files=records,privacy='Historical private filenames represented by aliases in this new index;original frozen authority files remain byte-exact in the solution snapshot')
        z.writestr('__review_package__/INDEX.json',json.dumps(index,ensure_ascii=False,indent=2)+'\n')
    with zipfile.ZipFile(destination) as z:
        assert z.testzip() is None
        for p,r in zip(members,records):assert digest(z.read(p))==r['sha256'] and sha(ROOT/p)==r['sha256']
    receipt=dict(path=destination.relative_to(ROOT).as_posix(),bytes=destination.stat().st_size,sha256=sha(destination),solution_files=len(members),scope=index['scope'],decision=index['decision'],public_manifest_sha256=index['publication_manifest_sha256'])
    (LOCAL/'PACKAGE_RECEIPT.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf8');print(json.dumps(receipt))

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('action',choices=['freeze','qualify','transition','tests','old-test','guard','publish','verify','package']);p.add_argument('--archive',type=Path);p.add_argument('--root',default=ZIP_ROOT);a=p.parse_args()
    if a.action=='freeze':freeze()
    elif a.action=='qualify':assert a.archive is not None;qualify(a.archive,a.root)
    elif a.action=='transition':save('04A28-source-transition-successor-v2.json',source_transition())
    elif a.action=='tests':raise SystemExit(tests())
    elif a.action=='old-test':raise SystemExit(old_test())
    elif a.action=='guard':guard()
    elif a.action=='publish':publish()
    elif a.action=='verify':verify()
    elif a.action=='package':package()
if __name__=='__main__':main()
