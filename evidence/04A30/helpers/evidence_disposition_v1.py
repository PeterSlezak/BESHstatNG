"""04A.30 read-only evidence successor. No product or historical writes.

Commands: freeze, qualify [--archive PATH], transition, tests, old-test,
guard, publish, verify. Raw command logs stay in a fresh ignored local root.
The optional pristine archive is read only after its entries authenticate
against the frozen 04A.29 manifest. No archive path is presumed.
"""
from pathlib import Path
import argparse,copy,csv,hashlib,importlib.util,json,os,re,subprocess,sys,time,unittest,zipfile
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'evidence/04A30'
LOCAL=ROOT/'.04a30-disposition-v1.tmp'
BASE='45466a88e7567c9f4711ba4fbbe295a7a2e69bb4'
MAN29='evidence/04A29/publication-manifest.json'
PINS28='evidence/04A28/source-preservation.json'
DOSSIER='docs/architecture/batch-04A-30-owner-review.md'
APPROVED=tuple('BESHstatNG/src/Graphics/'+n+'.vb' for n in ('ROC','NormalPlot','BoxPlot','Histogram','ScatterPlotMatrix','XYZscatter'))
PRIOR=OUT/'closeout-v1-prior'
REVISE=('validation.json','nonpasses.json','source-preservation.json','privacy.json','schema-validation.json',
        'final-git-state.json','publication-manifest.json','README.json','source-ownership-guard.json')
def digest(b):return hashlib.sha256(b).hexdigest()
def sha(path):
    h=hashlib.sha256()
    with Path(path).open('rb') as f:
        for block in iter(lambda:f.read(1024*1024),b''):h.update(block)
    return h.hexdigest()
def load(path):return json.loads(Path(path).read_bytes())
def save(name,value):
    p=OUT/name;p.parent.mkdir(parents=True,exist_ok=True)
    p.write_text(json.dumps(value,ensure_ascii=False,allow_nan=False,indent=2)+'\n',encoding='utf-8')
def pin(path):
    path=Path(path);return dict(path=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=sha(path))
def git(*args,raw=False):
    b=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT)
    return b if raw else b.decode('utf-8-sig').strip()
def state():
    assert git('branch','--show-current')=='codex/04A-30'
    assert git('rev-parse','HEAD','main').splitlines()==[BASE,BASE]
    assert not git('diff','--cached','--name-only')
    for commit in ['02a6d01cd3520116539fcefdd77612bbd9550129','ed94086e449f22ee1a0fc3888d6209ecba85a8b5','033cbe9033cc10bce3e19322e5582e5ea389fe22']:
        subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'merge-base','--is-ancestor',commit,BASE],cwd=ROOT,check=True)
    return dict(branch=git('branch','--show-current'),base=BASE,head=BASE,index='EMPTY',tracked_paths=git('diff','--name-only').splitlines())
def freeze():
    assert not (OUT/'closeout-entry-v1.json').exists(),'Closeout entry already frozen'
    current=state();manifest=load(OUT/'publication-manifest.json')
    for row in manifest['files']:
        if row['sha256'] is not None:assert pin(ROOT/row['path'])==row,row['path']
    PRIOR.mkdir(exist_ok=False);LOCAL.mkdir(exist_ok=False)
    retention=[]
    for relative in [DOSSIER]+['evidence/04A30/'+name for name in REVISE]:
        source=ROOT/relative
        target=PRIOR/('owner-review-pre-closeout.md' if relative==DOSSIER else source.name)
        with target.open('xb') as f:f.write(source.read_bytes())
        retention.append(dict(original_path=relative,retained_path=target.relative_to(ROOT).as_posix(),bytes=source.stat().st_size,sha256=sha(source)))
    (PRIOR/'README.txt').write_text('Immutable pre-closeout 04A.30 summaries. Superseded decisions are historical, not current authority. Large unchanged CSV/casebooks remain at their original paths, authenticated by the retained manifest.\n',encoding='utf8')
    save('closeout-retention-v1.json',dict(files=retention,large_unchanged_artifacts='Not duplicated; retained manifest authenticates their original locations.'))
    save('closeout-entry-v1.json',dict(**current,prior_publication_files=len(manifest['files']),prior_external_hashes_verified=sum(r['sha256'] is not None for r in manifest['files']),publication_files=manifest['files'],historical_authority=[pin(ROOT/MAN29),pin(ROOT/PINS28),pin(ROOT/'scripts/tests/test_04a28_output.py'),pin(ROOT/'AGENTS.md'),pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'),pin(ROOT/'.gitattributes')],writer_check='No dotnet/MSBuild/testhost/python/Excel before closeout. Eleven pre-existing Node processes; command-line attribution not claimed. Isolated audit helper outputs only.',archive_discovery='Named archives not found at repository/Release_archive/repository-parent/Downloads/Desktop roots. No archive inspected at entry.',historical_results='No new traversal/payload rehash; accepted 04A30 metadata receipt preserved and reused.',gates=dict(P='NO',N1='NO GO',N2='NO',Gate_C2='UNMET')))
    print('PASS closeout freeze;53 external publication hashes;prior summaries retained')

def command(name,args):
    folder=LOCAL/name;folder.mkdir(exist_ok=False);start=time.perf_counter()
    env=os.environ.copy();env['PYTHONDONTWRITEBYTECODE']='1';env['MSBUILDDISABLENODEREUSE']='1'
    with (folder/'stdout.log').open('wb') as out,(folder/'stderr.log').open('wb') as err:
        p=subprocess.run([str(a) for a in args],cwd=ROOT,env=env,stdout=out,stderr=err)
    def clean(s):
        return str(s).replace(str(LOCAL),'<DISPOSITION_RUN>').replace(LOCAL.as_posix(),'<DISPOSITION_RUN>').replace(str(ROOT),'<REPO_ROOT>').replace(ROOT.as_posix(),'<REPO_ROOT>').replace(sys.executable,'<PYTHON>')
    receipt=dict(command=[clean(a) for a in args],exit_code=p.returncode,seconds=time.perf_counter()-start,stdout_sha256=sha(folder/'stdout.log'),stderr_sha256=sha(folder/'stderr.log'),helper_sha256=sha(__file__))
    (folder/'receipt.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf8')
    print(name,'exit',p.returncode,'seconds',round(receipt['seconds'],2));return receipt

def archive_entries(path,manifest):
    archive=zipfile.ZipFile(path);names=archive.namelist();data={}
    for entry in manifest['files']:
        relative=entry['path'];matches=[n for n in names if n.replace('\\','/').rstrip('/').endswith('/'+relative) or n.replace('\\','/')==relative]
        assert len(matches)==1,('Missing/ambiguous archive entry',relative)
        b=archive.read(matches[0]);data[relative]=b
        if entry['sha256'] is not None:assert len(b)==entry['bytes'] and digest(b)==entry['sha256'],('Pristine archive hash mismatch',relative)
    archive.close();return data
def qualify(archive_path=None):
    entry=load(OUT/'closeout-entry-v1.json');expected=next(x for x in entry['historical_authority'] if x['path']==MAN29)
    assert pin(ROOT/MAN29)==expected,'Frozen04A29 manifest altered'
    manifest=load(ROOT/MAN29);originals=archive_entries(archive_path,manifest) if archive_path else {}
    rows=[]
    for old in manifest['files']:
        relative=old['path'];current=(ROOT/relative).read_bytes();blob=git('show',BASE+':'+relative,raw=True)
        assert current==blob,('Working-tree drift',relative)
        row=dict(path=relative,original_bytes=old['bytes'],original_sha256=old['sha256'],current_bytes=len(current),current_sha256=digest(current),git_blob_oid=git('rev-parse',BASE+':'+relative),git_blob_sha256=digest(blob),matches_git_blob=True,original_bytes_inspected=bool(originals))
        if old['sha256'] is None:
            row.update(category='MANIFEST_SELF_ENTRY',relation='Self-hash exception retained; no recursive seal')
        elif len(current)==old['bytes'] and digest(current)==old['sha256']:
            row.update(category='DIRECT_ORIGINAL_HASH_MATCH',relation='Exact original manifest SHA/byte match')
        elif originals:
            original=originals[relative]
            assert original.replace(b'\r\n',b'\n')==blob,('Non-EOL byte difference',relative)
            row.update(category='ORIGINAL_BYTES_CRLF_TO_LF_PROVEN',relation='Inspected pristine original SHA/size authenticated, then exact original.replace(CRLF,LF)==committed/current bytes',original_crlf_count=original.count(b'\r\n'),original_lone_lf_count=original.count(b'\n')-original.count(b'\r\n'))
        else:
            # Diagnostic corroboration only. Neither candidate is promoted to
            # original-source inspection, and no mixed-ending mapping is guessed.
            uniform=current.replace(b'\n',b'\r\n')
            terminal_lf=uniform[:-2]+b'\n' if uniform.endswith(b'\r\n') else uniform
            candidates=[(name,b) for name,b in [('UNIFORM_CRLF',uniform),('CRLF_WITH_LITERAL_FINAL_LF',terminal_lf)] if len(b)==old['bytes'] and digest(b)==old['sha256']]
            row.update(category='ORIGINAL_BYTES_UNAVAILABLE_NOT_PROVEN',relation='Independent ZIP finding reported by owner; no local original inspected. No reverse mapping asserted as proof.',digest_corroboration=[name for name,b in candidates],corroboration_limit='Digest-matching inverse is a diagnostic, not a claimed inspected original. Mixed endings are not guessed.')
        rows.append(row)
    counts=dict(__import__('collections').Counter(r['category'] for r in rows));unresolved=[r['path'] for r in rows if r['category']=='ORIGINAL_BYTES_UNAVAILABLE_NOT_PROVEN']
    save('04A29-eol-qualification-v1.json',dict(schema='04A30-04A29-eol-qualification-v1',status='PASS_ALL_ORIGINAL_EOL_TRANSITIONS_PROVEN' if not unresolved else 'CONDITIONAL_ORIGINAL_BYTES_MISSING',manifest_pin=pin(ROOT/MAN29),entries=len(rows),counts=counts,archive_provenance=dict(inspected=True,archive_basename=Path(archive_path).name,archive_sha256=sha(archive_path),archive_bytes=Path(archive_path).stat().st_size,qualification='Every externally hashed original archive entry matched frozen manifest SHA/bytes before normalization comparison') if archive_path else dict(inspected=False,source='No original archive/local pre-normalization bytes available; owner ZIP-to-ZIP findings are external testimony, not local inspection'),gitattributes=pin(ROOT/'.gitattributes'),text_rule='* text=auto eol=lf; historical publication hashes describe original bytes, not normalized blobs',unresolved_paths=unresolved,rows=rows,scientific='NOT_GOLDEN; byte/EOL qualification only'))
    print('EOL categories',counts,'unresolved',len(unresolved));return rows

def check_pins(old,current,transitions):
    assert len(old)==278 and set(current)==set(old),'Missing/foreign frozen source pin'
    changed={p for p in old if current[p]!=old[p]}
    assert changed==set(APPROVED),'Unexpected changed-source population'
    assert len(old)-len(changed)==272
    assert set(transitions)==changed
    for path in changed:
        t=transitions[path]
        assert t['old_sha256']==old[path],'Legacy SHA mismatch/swapped transition'
        assert t['new_sha256']==current[path] and t['new_sha256']!=t['old_sha256'],'New SHA mismatch/swapped transition'
    return changed
def check_manifest(b):
    expected=next(x for x in load(OUT/'closeout-entry-v1.json')['historical_authority'] if x['path']==MAN29)
    assert digest(b)==expected['sha256'] and len(b)==expected['bytes'],'Frozen04A29 manifest altered'
def guard_contract(guard,transitions):
    assert guard['status']=='PASS' and guard['compiled_windows_graphics']==38 and guard['unchanged_windows_declarations'] is True
    assert guard['changed_member_count']==13 and guard['portable_dependency_guard']=='PASS'
    assert guard['native_chart_appearance']=='NATIVE_NOT_RUN'
    required={
        'ROC.vb': [('BESHStatNG.RocDataCore','ComputeCurve'),('BESHStatNG.RocDataCore','ComputeDeLong'),('BESHStatNG.RocDataCore','MidRanks')],
        'NormalPlot.vb':[('BESHStatNG.NormalPlotDataCore','SortedCopy'),('BESHStatNG.NormalPlotDataCore','Scores'),('BESHStatNG.NormalPlotDataCore','ReferenceLine')],
        'BoxPlot.vb':[('BESHStatNG.BoxSegmentsCore','Compute')],
        'Histogram.vb':[('BESHStatNG.HistogramDataCore','BinsCopy'),('BESHStatNG.HistogramDataCore','OverlayCopy'),('BESHStatNG.HistogramDataCore','HorizontalBounds')],
        'ScatterPlotMatrix.vb':[('BESHStatNG.ScatterMatrixDataCore','Normalize')],
        'XYZscatter.vb':[('BESHStatNG.XyzDataCore','NormalizeAxis')]}
    windows_calls={'ROC.vb':['RocDataCore.ComputeCurve','RocDataCore.ComputeDeLong','RocDataCore.MidRanks'],
        'NormalPlot.vb':['NormalPlotDataCore.SortedCopy','NormalPlotDataCore.Scores','NormalPlotDataCore.ReferenceLine'],
        'BoxPlot.vb':['BoxSegmentsCore.Compute','SnapshotGroupNames()'],
        'Histogram.vb':['HistogramDataCore.BinsCopy','HistogramDataCore.OverlayCopy','HistogramDataCore.HorizontalBounds'],
        'ScatterPlotMatrix.vb':['ScatterMatrixDataCore.Normalize'],'XYZscatter.vb':['XyzDataCore.NormalizeAxis']}
    members=guard['current_members']
    for path,t in transitions.items():
        assert sha(ROOT/path)==t['new_sha256']
        source=(ROOT/path).read_text(encoding='utf-8-sig')
        assert all(call in source for call in windows_calls[Path(path).name]),'Missing Windows Core delegation'
        changed=[r['current'] for r in guard['changed'] if r['current']['path']==path]
        assert changed and all(m['source_sha256']==t['new_sha256'] for m in changed)
        for typename,name in required[Path(path).name]:
            candidates=[m for m in members if m['qualified_type']==typename and m['name']==name]
            assert candidates,'Missing portable delegation declaration'
            assert all(sha(ROOT/m['path'])==m['source_sha256'] for m in candidates)
    assert len(guard['frozen_fixture_proofs'])==9 and all(p['original_statement_tokens_sha256']==p['fixture_statement_tokens_sha256'] for p in guard['frozen_fixture_proofs'])

def payload():
    old=load(ROOT/PINS28)['protected_compiled_sources'];current={p:sha(ROOT/p) for p in old}
    previous=load(PRIOR/'source-preservation.json');new_pins={r['path']:r for r in previous['changed']}
    graphics={r['path']:r for r in csv.DictReader((ROOT/'evidence/04A29/graphics-files-v1.csv').open(encoding='utf-8-sig',newline=''))}
    transitions={}
    for p in APPROVED:
        blob=git('show',BASE+':'+p,raw=True)
        exact=digest(blob)==old[p]
        candidate=blob if exact else blob.replace(b'\n',b'\r\n')
        assert digest(candidate)==old[p] and candidate.replace(b'\r\n',b'\n')==blob.replace(b'\r\n',b'\n')
        transitions[p]=dict(path=p,old_sha256=old[p],entry04A29_sha256=graphics[p]['sha256'],entry04A30_sha256=new_pins[p]['entry_sha256'],new_sha256=new_pins[p]['final_sha256'],old_git_blob_oid=git('rev-parse',BASE+':'+p),old_git_blob_sha256=digest(blob),old_git_blob_relation='EXACT_PIN_MATCH' if exact else 'PIN_AUTHENTICATED_CRLF_RECONSTITUTION_OF_LF_GIT_BLOB; original archive not claimed',cause='AUTHORIZED_VERSIONED_SOURCE_TRANSITION_04A30')
    return old,current,transitions
def protection():
    # Reuse the prior exact tracked-entry census plus the seven previously
    # approved final hashes. Never read historical TestResults payload bytes.
    baseline=load(OUT/'entry-pins.json')['files'];paths=git('ls-files').splitlines()
    alias={'<PRIVATE_PROTECTED_PATH:'+digest(p.encode())[:24]+'>':p for p in paths}
    allowed={r['path']:r['final_sha256'] for r in load(PRIOR/'source-preservation.json')['changed']}
    errors=[]
    for row in baseline:
        path=alias.get(row['path'],row['path']);expected=allowed.get(path,row['sha256'])
        if not (ROOT/path).is_file() or sha(ROOT/path)!=expected:errors.append(row['path'])
    assert not errors,errors
    for item in load(OUT/'closeout-entry-v1.json')['publication_files']:
        if item['path'] in APPROVED or item['path'].endswith(('.vb','.vbproj','.csproj')):
            assert pin(ROOT/item['path'])==item,item['path']
    for row in load(OUT/'closeout-entry-v1.json')['historical_authority']:assert pin(ROOT/row['path'])==row
    registry=load(ROOT/'evidence/04A14/baseline-registry.json')['files']
    assert len(registry)==100 and all(sha(ROOT/p)==v['sha256'] for p,v in registry.items())
    return dict(status='PASS',tracked_entry_files=len(baseline),existing_approved_tracked_changes=len(allowed),new_closeout_product_or_existing_test_changes=0,all_100_captures_match_registry=True,historical_TestResults='NOT_RESCANNED/NOT_REHASHED; prior matching metadata receipt reused; no historical writers invoked')

def transition():
    state();check_manifest((ROOT/MAN29).read_bytes());old,current,changes=payload()
    check_pins(old,current,changes)
    for path,row in changes.items():assert row['old_sha256']==row['entry04A29_sha256']==row['entry04A30_sha256']
    guard_contract(load(OUT/'source-ownership-guard.json'),changes)
    project=ET.parse(ROOT/'BESHstatNG/BESHstatNG.vbproj')
    compiled=sorted('BESHstatNG/'+n.attrib['Include'].replace('\\','/') for n in project.iter() if n.tag.split('}')[-1]=='Compile' and 'Graphics' in n.attrib.get('Include',''))
    assert len(compiled)==38 and compiled==sorted('BESHstatNG/src/Graphics/'+p.name for p in (ROOT/'BESHstatNG/src/Graphics').glob('*.vb'))
    core=load(ROOT/'evidence/04A29/graphics-existing-core-crosswalk-v1.json')['core']
    assert len(core)==12 and all(sha(ROOT/r['source']['path'])==r['source']['sha256'] for r in core)
    recon=load(ROOT/'evidence/04A29/04A01-graphics-reconciliation-v2.json')
    assert set(map(int,recon['uncertain_different_source_ordinals']))=={171,172} and recon['categories']['UNCERTAIN_DIFFERENT_SOURCE']==2
    historical=list(csv.DictReader((ROOT/'evidence/04A29/04A01-graphics-member-crosswalk-v2.csv').open(encoding='utf-8-sig',newline='')))
    assert len(historical)==1062 and {int(r['historical_ordinal']) for r in historical if r['category']=='UNCERTAIN_DIFFERENT_SOURCE'}=={171,172}
    field='BESHStatNG.Test/BESHStatNG.Tests.vbproj';before=git('show',BASE+':'+field,raw=True);after=(ROOT/field).read_bytes()
    prior=next(p for p in load(OUT/'closeout-entry-v1.json')['publication_files'] if p['path']==field)
    assert digest(after)==prior['sha256'] and digest(after)!=digest(before)
    tree=ET.fromstring(after);includes=[x.attrib.get('Include','') for x in tree.iter() if x.tag.split('}')[-1]=='Compile']
    assert all(any(name in path for path in includes) for name in ['Graphics30AdapterTests.vb','Graphics30Tests.vb','LegacyGraphics30.vb','LegacyBoxBounds30.vb','OrderedPlotGroups30Tests.vb'])
    result=dict(status='PASS',frozen_pin_source=pin(ROOT/PINS28),total_old_pins=278,unchanged=272,authorized_graphics_transitions=6,transitions=list(changes.values()),all_old_pins_accounted=True,compiled_windows_graphics=38,existing_core_graphics_unchanged=12,uncertain_historical_ordinals_retained=[171,172],ast_guard=pin(OUT/'source-ownership-guard.json'),native_exclusion='Re-executed AST guard limits13 old member deltas;original declarations/defaults retained;Histogram native tail token-identical;Box category argument snapshot only. Native execution NATIVE_NOT_RUN.',test_project=dict(path=field,entry_git_blob_sha256=digest(before),current_sha256=digest(after),classification='SEPARATE_FRAMEWORK_TEST_INCLUSION_CHANGE; not a seventh Graphics transition'),protections=protection(),historical_test_status='Six old subtests remain FAIL. Successor PASS does not change their outcomes.',scientific='NOT_GOLDEN')
    save('04A28-source-transition-successor-v1.json',result);print('PASS successor:278/272/6;38 Windows;12 existing Core;171/172 retained');return result

class SuccessorTests(unittest.TestCase):
    def test_historical_manifest_original_bytes_unchanged(self):
        check_manifest((ROOT/MAN29).read_bytes())
    def test_all_278_pins_and_six_transitions(self):
        old,current,changes=payload();self.assertEqual(set(APPROVED),check_pins(old,current,changes))
    def test_seventh_changed_source_is_rejected(self):
        old,current,changes=payload();other=next(p for p in old if p not in APPROVED);current[other]='0'*64
        with self.assertRaises(AssertionError):check_pins(old,current,changes)
    def test_swapped_legacy_and_new_sha_is_rejected(self):
        old,current,changes=payload();row=changes[APPROVED[0]];row['old_sha256'],row['new_sha256']=row['new_sha256'],row['old_sha256']
        with self.assertRaises(AssertionError):check_pins(old,current,changes)
    def test_missing_pin_is_rejected(self):
        old,current,changes=payload();current.pop(next(iter(current)))
        with self.assertRaises(AssertionError):check_pins(old,current,changes)
    def test_altered_historical_manifest_is_rejected(self):
        with self.assertRaises(AssertionError):check_manifest((ROOT/MAN29).read_bytes()+b'\n')
    def test_missing_portable_delegation_is_rejected(self):
        _,_,changes=payload();guard=load(OUT/'source-ownership-guard.json')
        guard['current_members']=[m for m in guard['current_members'] if m['qualified_type']!='BESHStatNG.RocDataCore']
        with self.assertRaises(AssertionError):guard_contract(guard,changes)
    def test_missing_windows_delegation_is_rejected(self):
        _,_,changes=payload();guard=load(OUT/'source-ownership-guard.json')
        changed=next(r for r in guard['changed'] if r['current']['path']==APPROVED[0]);changed['current']['source_sha256']='0'*64
        with self.assertRaises(AssertionError):guard_contract(guard,changes)
    def test_ast_membership_core_and_protected_sources(self):
        self.assertEqual('PASS',transition()['status'])
    def test_archive_proof_preserves_mixed_endings(self):
        original=b'first\r\nsecond\nthird\r\n';relative='fixture/mixed.txt'
        path=LOCAL/'mixed-ending-validator-fixture.zip'
        with zipfile.ZipFile(path,'w') as z:z.writestr(relative,original)
        manifest=dict(files=[dict(path=relative,bytes=len(original),sha256=digest(original))])
        actual=archive_entries(path,manifest)[relative]
        self.assertEqual(original,actual)
        self.assertEqual(b'first\nsecond\nthird\n',actual.replace(b'\r\n',b'\n'))
    def test_wrong_archive_original_hash_is_rejected(self):
        path=LOCAL/'wrong-original-validator-fixture.zip'
        with zipfile.ZipFile(path,'w') as z:z.writestr('fixture/wrong.txt',b'changed')
        with self.assertRaises(AssertionError):archive_entries(path,dict(files=[dict(path='fixture/wrong.txt',bytes=7,sha256='0'*64)]))

def run_tests():
    result=unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(SuccessorTests))
    receipt=dict(status='PASS' if result.wasSuccessful() else 'FAIL',tests_run=result.testsRun,failures=len(result.failures),errors=len(result.errors),skipped=len(result.skipped),test_identities=[__name__+'.SuccessorTests.'+name for name in unittest.defaultTestLoader.getTestCaseNames(SuccessorTests)],helper_pin=pin(Path(__file__)))
    save('source-transition-tests-v1.json',receipt)
    return 0 if result.wasSuccessful() else 1

def old_test():
    spec=importlib.util.spec_from_file_location('historical_output28',ROOT/'scripts/tests/test_04a28_output.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
    suite=unittest.defaultTestLoader.loadTestsFromName('Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved',m)
    result=unittest.TextTestRunner(verbosity=2).run(suite)
    self_pin=pin(ROOT/'scripts/tests/test_04a28_output.py')
    assert result.testsRun==1 and len(result.failures)==6 and not result.errors
    subtests=[]
    for case,trace in result.failures:
        text=str(case);match=re.search(r"path='([^']+)'",text);assert match,text
        subtests.append(dict(path=match.group(1),outcome='FAIL',cause='AUTHORIZED_VERSIONED_SOURCE_TRANSITION_04A30',failure_text_sha256=digest(trace.encode())))
    assert {r['path'] for r in subtests}==set(APPROVED)
    save('historical-source-pin-rerun-v1.json',dict(status='FAIL_EXPECTED_SIX_HISTORICAL_SUBTESTS',raw_parent_tests_run=1,raw_failed_subtests=6,raw_errors=0,parent_identity='scripts/tests/test_04a28_output.py::Output28SourceContracts::test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved',unchanged_test_pin=self_pin,subtests=subtests,outcomes_not_subtracted_or_promoted=True))
    return 1

def guard():
    # The accepted helper only parses source and writes the snapshotted current
    # audit guard receipt. No product compilation, original ledger or captures.
    assets=ROOT/'.04a29-audit.tmp/obj/project.assets.json'
    assert assets.is_file(),'Existing offline SDK assets required; no restore authorized'
    args=['dotnet','build','evidence/04A30/helpers/Graphics30Guard.csproj','--no-restore','-p:ProjectAssetsFile='+str(assets),'-p:BaseIntermediateOutputPath='+str(LOCAL/'guard-obj')+'/', '-p:OutputPath='+str(LOCAL/'guard-bin')+'/', '--nologo','--verbosity','quiet']
    assert command('ast-build',args)['exit_code']==0
    assert command('ast-guard',['dotnet',LOCAL/'guard-bin/Graphics30Guard.dll'])['exit_code']==0

def retained():
    receipt=load(OUT/'closeout-retention-v1.json');mapping={r['original_path']:r for r in receipt['files']}
    for row in receipt['files']:
        p=ROOT/row['retained_path'];assert p.stat().st_size==row['bytes'] and sha(p)==row['sha256']
    original=load(PRIOR/'publication-manifest.json')
    for row in original['files']:
        if row['sha256'] is None:continue
        path=ROOT/(mapping[row['path']]['retained_path'] if row['path'] in mapping else row['path'])
        assert path.stat().st_size==row['bytes'] and sha(path)==row['sha256'],row['path']
    return dict(status='PASS',retained_summary_copies=len(mapping),prior_external_manifest_entries_preserved=53,large_CSV_casebook_copies=0)

def current_paths():
    paths=set(git('diff','--name-only').splitlines())|set(git('ls-files','--others','--exclude-standard').splitlines())
    allowed={r['path'] for r in load(OUT/'closeout-entry-v1.json')['publication_files']}
    assert all(p in allowed or p.startswith('evidence/04A30/') for p in paths),'Unexpected closeout publication path'
    return sorted(paths)
def receipts():
    return [dict(name=p.parent.name,**load(p),raw_receipt_sha256=sha(p)) for p in sorted(LOCAL.glob('*/receipt.json'))]

def publish():
    state();preservation=protection();retention=retained()
    eol=load(OUT/'04A29-eol-qualification-v1.json');successor=load(OUT/'04A28-source-transition-successor-v1.json')
    tests=load(OUT/'source-transition-tests-v1.json');old_run=load(OUT/'historical-source-pin-rerun-v1.json')
    assert successor['status']==tests['status']=='PASS' and old_run['raw_failed_subtests']==6
    assert tests['helper_pin']==pin(Path(__file__)),'Tests must execute against final helper source'
    unresolved=eol['unresolved_paths']
    decision='NOT_READY' if unresolved else 'READY_FOR_OWNER_04A30_ACCEPTANCE_REVIEW'
    blockers=[dict(issue='04A29_ORIGINAL_BYTES_UNAVAILABLE',paths=unresolved,next_action='Supply a readable pristine pre-merge archive/original byte population; qualify --archive authenticates all120 old pins before comparing exact CRLF-to-LF bytes. No inverse EOL mapping is substituted for original inspection.')] if unresolved else []
    closeout=dict(schema='04A30-evidence-disposition-v1',decision=decision,branch='codex/04A-30',base=BASE,head=BASE,index='EMPTY',eol=dict(receipt=pin(OUT/'04A29-eol-qualification-v1.json'),counts=eol['counts'],unresolved_paths=unresolved),source_transition=dict(receipt=pin(OUT/'04A28-source-transition-successor-v1.json'),total=278,unchanged=272,transitions=6,status='PASS',historical_fail_subtests_retained=6,cause='AUTHORIZED_VERSIONED_SOURCE_TRANSITION_04A30'),blockers=blockers,new_executions=dict(successor_tests=tests,historical_test=old_run,ast='RUN PASS; existing helper rebuilt in isolated audit outputs; product NOT_REBUILT'),previous_product_runs='REUSED_PREVIOUS_04A30_EXECUTION_EVIDENCE, not rerun in this closeout',deliberately_not_run=['Managed Debug/Release full matrices','Node','Full Python745-source lane (helper not discovered there)','strict/profile/scientific controls','MkDocs','native Excel/COM/UIA/RefEdit','independent R','browser/Office/Mac E2E','historical138GB payload rehash/stat traversal'],source_and_history=preservation,retained_prior=retention,commands=receipts(),gates=dict(P='NO',N1='NO GO',N2='NO',Gate_C2='UNMET'),scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN',native_releases=0,original_control_discharges=0,office_gui_activated=False,next_batch_started=False)
    save('04A30-evidence-disposition-v1.json',closeout)
    diagnostic_counts={}
    for row in eol['rows']:
        for kind in row.get('digest_corroboration',[]):diagnostic_counts[kind]=diagnostic_counts.get(kind,0)+1
    description=f'''# 04A.30 targeted evidence disposition v1

**{decision} — software extraction only; no product changes in this closeout.**

Live branch `codex/04A-30`, merged-main entry/HEAD `{BASE}`, empty staged index. Original seven tracked modifications are unchanged from closeout entry. All previous publication external hashes passed53/53 at entry. Prior summaries/dossier/manifest are retained byte-for-byte under `closeout-v1-prior/`; large unchanged CSV/casebook data was not copied.

04A.29:121 manifest entries enumerated (120 externally hashed plus one self entry). All27 direct original SHA/size matches pass. All121 current files equal their committed Git blobs. **{len(unresolved)} EOL rows remain unproved locally** because the pristine pre-merge archive/original bytes were not found. The owner's independent ZIP-to-ZIP finding of93 exact CRLF-to-LF transitions is recorded as external evidence, not falsely claimed as local archive inspection. Digest-only inverse diagnostics are {diagnostic_counts}; they are expressly not promoted to original-byte inspection. Four mixed-ending Markdown cases cannot be settled by blind reversal; the receipt lists every unresolved path. `.gitattributes` has `* text=auto eol=lf`, consistent with the reported transition, but policy alone is not per-file byte proof. Frozen04A.29 manifest and v2 lineage remain unchanged.

04A.28 successor: **278 accounted /272 unchanged /six authorized Graphics transitions PASS**. Old hashes match both04A.29 graphics pins and04A.30 entry pins; new hashes match the pre-closeout protected-source receipt and current bytes. Accepted main Git blobs supply exact or digest-authenticated CRLF source correspondence. AST/declaration/call-through/native-tail guards pass for38 compiled Windows files;12 original Core graphic sources and171/172 uncertainty remain unchanged. The Framework test-project includes are a separate seventh tracked-file change, not another Graphics transition.

New narrowly scoped tests: **{tests['tests_run']} PASS,0 FAIL,0 ERROR**, including seventh-source drift, swapped SHA, missing pin, altered frozen manifest, missing Core delegation, mixed-ending archive parsing and bad-original-hash rejection. The unchanged historical test was actually rerun: **one parent test,six FAIL subtests,zero ERROR**. Each remains FAIL with cause `AUTHORIZED_VERSIONED_SOURCE_TRANSITION_04A30`; successor success does not subtract failures or turn that test green.

Prior software runs are reused, not re-executed: Debug and Release managed19,930 PASS/14 inherited FAIL each;Node916 PASS;original complete Python838/108/10,later qualified aggregate840/106/10 after scripts rerun;strict/profile1/1,MkDocs0. New closeout runs are separate: source/AST guard,11 targeted successor/validator tests and the historical six-failure test. No full matrix, native/R, scientific oracle or138GB history rescan was performed. All100 captures still match registry; all Core/Common/Office/UDF/Ribbon/Designer and prior product/test pins are preserved.

Current acceptance blocker is original-byte availability, with the exact93 paths in `04A29-eol-qualification-v1.json`. Supply a readable pristine archive/original population for local proof; no permission to edit history, old expected hashes or software is requested or inferred. If all93 exact transitions authenticate, this dossier may recommend software-only acceptance review; formal owner acceptance remains separate.

P=NO;N1=NO GO;N2=NO;Gate C2=UNMET. NOT_GOLDEN;NATIVE_NOT_RUN;zero native releases/discharges. No Office/Mac GUI activation,no04A.31 start,no stage/commit/merge/push/deploy/sign.
'''
    save('closeout-command-outcomes-v1.json',dict(commands=receipts(),raw_logs='Fresh ignored <DISPOSITION_RUN>; raw prior failures/receipts retained. No private paths published.'))
    (OUT/'04A30-evidence-disposition-v1.md').write_text(description,encoding='utf8')
    # Update only versioned current-batch summaries from their exact retained
    # originals. No original command receipts or large datasets are rewritten.
    validation=load(PRIOR/'validation.json');validation['decision']=decision;validation['blockers']=blockers
    validation['evidence_disposition_closeout']=dict(status=decision,new_tests_run=tests['tests_run'],new_tests_passed=tests['tests_run'],historical_parent_tests_run=1,historical_fail_subtests=6,previous_matrix_status='REUSED_PREVIOUS_04A30_EXECUTION_EVIDENCE',deliberately_not_run=closeout['deliberately_not_run'],source_transition='PASS AUTHORIZED_VERSIONED_SOURCE_TRANSITION_04A30',eol_unproved=len(unresolved),receipt='04A30-evidence-disposition-v1.json')
    save('validation.json',validation)
    nonpasses=load(PRIOR/'nonpasses.json');nonpasses['decision']=decision;nonpasses['acceptance_blockers']=blockers
    nonpasses['historical_six_source_pin_failures']=dict(status='FAIL',cause='AUTHORIZED_VERSIONED_SOURCE_TRANSITION_04A30',successor='PASS',receipt='historical-source-pin-rerun-v1.json',subtests=old_run['subtests'],raw_totals_unchanged=True)
    nonpasses['new_closeout_runs']=dict(successor_tests=tests['tests_run'],passed=tests['tests_run'],failed=0,errors=0,historical_parent_tests=1,historical_failed_subtests=6,aggregate_with_previous_suites='NOT_COMBINED; separate execution scopes')
    nonpasses['authority_manifest_discrepancies']=dict(original_frozen_entry_mismatches=93,eol_proven=93-len(unresolved),eol_unproved=len(unresolved),frozen_manifest_unchanged=True)
    save('nonpasses.json',nonpasses)
    source=load(PRIOR/'source-preservation.json');source['closeout_v1']=dict(**preservation,prior_receipt=pin(PRIOR/'source-preservation.json'),source_transition=pin(OUT/'04A28-source-transition-successor-v1.json'),historical_authority_unchanged=True);save('source-preservation.json',source)
    old=(PRIOR/'owner-review-pre-closeout.md').read_text(encoding='utf8')
    prefix='# Batch 04A.30 owner review\n\n'
    start=old.index('## Entry, authority and publication')
    intro=f'''**{decision} — targeted evidence disposition; software extraction only.**

The six historical04A.28 source-pin failures now have an executable verified successor:278/272/6 PASS,while the original six subtests remain FAIL. The remaining blocker is local proof for{len(unresolved)} original04A.29 EOL rows; no pristine archive/original bytes were available locally. All27 direct matches and all121 current-to-Git identities pass. The owner's ZIP findings are external testimony until originals can be inspected. No product or existing test was changed in this closeout. Prior complete summary is preserved byte-for-byte in `evidence/04A30/closeout-v1-prior/owner-review-pre-closeout.md`.

Canonical closeout: [disposition](../../evidence/04A30/04A30-evidence-disposition-v1.md), [machine receipt](../../evidence/04A30/04A30-evidence-disposition-v1.json), [EOL qualification](../../evidence/04A30/04A29-eol-qualification-v1.json), [source successor](../../evidence/04A30/04A28-source-transition-successor-v1.json), [new tests](../../evidence/04A30/source-transition-tests-v1.json), [historical FAIL rerun](../../evidence/04A30/historical-source-pin-rerun-v1.json). New source-helper tests/AST checks are RUN; prior managed/Node/Python/strict/MkDocs executions below are REUSED_PREVIOUS_04A30_EXECUTION_EVIDENCE,not reruns. Full matrices,native/R and historical payload scans are NOT_RUN in this closeout.

'''
    text=prefix+intro+old[start:]
    text=text.replace('The owner must disposition (1) the 93 inherited committed 04A.29 manifest mismatches and (2) the unchanged 04A.28 source-pin test\'s six failures on this batch\'s authorized Windows source changes.','The source transition is dispositioned by the passing successor; original-byte EOL proof remains open.')
    text=text.replace('After the two explicit evidence dispositions, send this Codex report and updated full ZIP','After the remaining original-byte qualification, send this Codex report and updated full ZIP')
    (ROOT/DOSSIER).write_text(text,encoding='utf8')
    readme=load(PRIOR/'README.json');readme['decision']=decision;readme['current_closeout']='04A30-evidence-disposition-v1.json';readme['verification']='helpers/evidence_disposition_v1.py verify (or unchanged finalize.py verify after final seal)';save('README.json',readme)
    seal()

def seal():
    state();paths=current_paths()
    # Reuse the accepted comprehensive content policy, including decoded JSON,
    # base64,NFKC,current owner/host,credential/UNC/SID/internal-host patterns.
    spec=importlib.util.spec_from_file_location('privacy14',ROOT/'scripts/verify-04a14-publication.py');policy=importlib.util.module_from_spec(spec);spec.loader.exec_module(policy)
    for path in paths:
        result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','--no-index',path],cwd=ROOT,stdout=subprocess.PIPE,stderr=subprocess.PIPE)
        assert result.returncode==1,('Ignored publication candidate',path)
    findings,reviewed=policy.scan(paths);assert not findings,findings
    save('privacy.json',dict(status='PASS',closeout='v1',candidate_count=len(paths),scope='Complete prior and new publication; accepted comprehensive14 scanner; no raw archive paths/host/SID/workbook/credential diagnostics',findings=0,private_historical_path_aliases=70,raw_logs='<DISPOSITION_RUN> ignored',reviewed_categories=sorted(set(x[1] for x in reviewed))))
    save('schema-validation.json',dict(status='PASS',closeout='v1',checks=['121 historical29 entries classified with self exception and unproved originals explicit','278/272/6 successor and exactly six historical FAIL subtests','11 positive/negative audit-helper tests PASS','Original38 compilation/12 existing Core/171-172 uncertainty retained','Every prior53 manifest payload retained via exact copy or unchanged original location','All product/existing tests/history/captures unchanged from closeout entry','Previous suites reused,not falsely rerun;no failures subtracted','Complete manifest/privacy/trackability/empty-index and diff checks']))
    save('final-git-state.json',dict(**state(),status_short=git('status','--short').splitlines(),publication_paths=current_paths(),git_publication_performed=False))
    paths=current_paths();manifest='evidence/04A30/publication-manifest.json'
    save('publication-manifest.json',dict(schema='04A30-publication-closeout-v1',self_rule='Self included;bytes/sha256 null;no recursive hash',files=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in paths]))
    findings,reviewed=policy.scan(current_paths());assert not findings,findings
    verify()

def verify():
    state();retained();protection()
    manifest=load(OUT/'publication-manifest.json');paths=current_paths()
    assert [r['path'] for r in manifest['files']]==paths and len(paths)==len(set(paths))
    for row in manifest['files']:
        if row['path']=='evidence/04A30/publication-manifest.json':assert row['sha256'] is row['bytes'] is None
        else:assert pin(ROOT/row['path'])==row,row['path']
    check_manifest((ROOT/MAN29).read_bytes());old,current,changes=payload();check_pins(old,current,changes);guard_contract(load(OUT/'source-ownership-guard.json'),changes)
    subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,check=True,stdout=subprocess.PIPE,stderr=subprocess.PIPE)
    print('PASS closeout publication SHA/bytes;prior receipts;278/272/6;source protection;empty index;files',len(paths))

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('action',choices=['freeze','qualify','transition','tests','old-test','guard','publish','verify']);p.add_argument('--archive',type=Path);a=p.parse_args()
    if a.action=='freeze':freeze()
    elif a.action=='qualify':qualify(a.archive)
    elif a.action=='transition':transition()
    elif a.action=='tests':raise SystemExit(run_tests())
    elif a.action=='old-test':raise SystemExit(old_test())
    elif a.action=='guard':guard()
    elif a.action=='publish':publish()
    elif a.action=='verify':verify()
if __name__=='__main__':main()
