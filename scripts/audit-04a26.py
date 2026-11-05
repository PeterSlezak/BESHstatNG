"""04A.26 read-only retention, dependency, source and final receipt audits."""
import ast
from datetime import datetime, timezone
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('capture26_audit','scripts/capture-04a26-contract.py');RUN,OUT=c.RUN,c.OUT
FORMS=[c.FORM,'BESHstatNG/src/UI/UiTwoInputRefedits.vb','BESHstatNG/src/UI/Ui9ANOVA2nested.vb','BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb']
HELP=['sample-size-icc.md','sample-size-bland-altman.md']
ALLOWED=set(FORMS+['BESHStatNG.Test/BESHStatNG.Tests.vbproj']+['BESHStatNG_Help_MkDocs/docs/methods/'+p for p in HELP])
def baseline():return json.loads((RUN/'baseline.json').read_bytes())['pins']
def dependency():
    rows=[]
    roots=['scripts/tests','BESHstatNG/tools/tests','BESHStatNG_Help_MkDocs/tests','BESHStat.Excel.Common.Tests/src']
    for root in roots:
        for path in sorted((ROOT/root).rglob('test_*.py')):
            tree=ast.parse(path.read_text(encoding='utf-8-sig'));calls=[]
            for node in ast.walk(tree):
                if isinstance(node,ast.Call):calls.append(ast.unparse(node.func))
            writers=[v for v in calls if any(w in v.lower() for w in ['write','mkdir','unlink','rmtree','temporary','copy','subprocess'])]
            runtime=any(v in path.read_text(encoding='utf-8-sig') for v in ['subprocess','G3_CANDIDATES','configuration','Debug','Release'])
            kind='writing/integrity validator' if writers else 'config-dependent runtime check' if runtime else 'frozen/historical global audit' if any(v in path.name for v in ['04a01','04a02','publication','preserv']) else 'static source-only/configuration-independent'
            methods=[n.name for n in ast.walk(tree) if isinstance(n,(ast.FunctionDef,ast.AsyncFunctionDef)) and n.name.startswith('test_')]
            rows.append(dict(path=path.relative_to(ROOT).as_posix(),sha256=c.sha(path),classification=kind,methods=methods,potential_side_effect_calls=writers,dependency_scope='Full repository/source/evidence conservative scope; transitive imports may scan or create temporary files.',execution='Lane D sequential full discovery roots in Debug and Release; zero method verdict reuse.'))
    pins={p:c.sha(ROOT/p) for p in set(c.git('ls-files').splitlines())|set(c.git('ls-files','--others','--exclude-standard').splitlines()) if (ROOT/p).is_file() and p.endswith(('.vb','.vbproj','.py','.ps1','.cjs','.mjs','.yml','.cs','.csproj')) and not p.startswith(('evidence/','TestResults/'))}
    c.emit('python-source-dependency-audit.json',dict(version=1,modules=rows,input_source_hashes=pins,classification_basis='Conservative AST call/dependency inventory; possible writers serialized. No historical scan edited or cached.',method_executions_reused=0,optimization='Only completed immutable TRX parsing/hashing is cached in new accounting; content SHA reread at terminal audit. No fifth worker, split discovery or alternating-batch policy.'))
def preflight():
    dependency()
    c.emit('parallel-preflight.json',dict(worker_limit=4,fifth_worker=False,runner=dict(path='scripts/verify-04a26.py',sha256=c.sha(ROOT/'scripts/verify-04a26.py')),predecessors={p:c.sha(ROOT/p) for p in ['scripts/verify-04a25.py','scripts/verify-beshstatng.ps1']},builds='Offline --no-restore /m:4; Debug build/matrix before Release build/matrix. No overlapping MSBuild outputs.',lanes=dict(A='Core/Common managed',B='Windows/ControlMitigation managed',C='Office/WASM managed and Node',D='All Python roots/source validators, submitted first'),outputs='Fresh TestResults/04A-26/<run>/commands and executions; copied bins and hash-checked project TestData; unique TRX and exports.',settings='MaxCpuCount=1, MSTest Workers=1, Node concurrency=1; C02 full-suite optional writer disabled. Separate semantic-only probe supplies Common Python observations.',original_verifier='Read-only predecessor. Its fixed TestResults/Codex outputs and restore-capable build prevent direct safe execution; equivalent established portable+Legacy inventory is run through isolated offline helper.',serial='Strict/profile, strict MkDocs, preservation/privacy/publication scans after matrix; no live Excel/Office/browser/R.'))
    pins=baseline();pages=['sample-size-paired-t-test.md','sample-size-unpaired-t-test.md','sample-size-single-proportion.md','sample-size-independent-proportions.md','sample-size-log-rank.md','sample-size-cox-regression.md','sample-size-icc.md','sample-size-bland-altman.md','bland-altman.md','deming-regression.md','intraclass-correlation-coefficients.md','cohens-kappa.md']
    pages += [p.name for p in (ROOT/'BESHStatNG_Help_MkDocs/docs/methods').glob('*concordance*.md')]
    audit={}
    for name in pages:
        p='BESHStatNG_Help_MkDocs/docs/methods/'+name
        audit[p]=dict(entry_sha256=pins[p],final_sha256=c.sha(ROOT/p),changed=pins[p]!=c.sha(ROOT/p))
    metadata='BESHStatNG_Help_MkDocs/docs/_meta/methods.yml';assert c.sha(ROOT/metadata)==pins[metadata]
    assets={p:h for p,h in pins.items() if p.startswith('BESHStatNG_Help_MkDocs/') and p.endswith(('.png','.jpg','.jpeg','.xlsx','.xlsm'))}
    for p,h in assets.items():assert c.sha(ROOT/p)==h
    c.emit('documentation-audit.json',dict(pages=audit,authored_metadata=dict(path=metadata,sha256=pins[metadata],unchanged=True),changes='ICC/LoA sizing clarify achieved result metadata and absence of separate assurance solver; no scientific formulas or controls changed.',assets_preserved=assets,availability='Existing Windows dialogs only; Office/Mac panel unavailable. Native chart execution and independent scientific oracle NOT_RUN.',udf_audit={p:c.sha(ROOT/p) for p in ['BESHstatNG/src/ExcelUDFs/SampleSizeUDFs.vb','BESHstatNG/src/ExcelUDFs/AgreementMethodUDFs.vb']},udf_difference='GUI text/allocation/rounding reports versus UDF registered scalar/result contracts; Log-rank/Cox GUI two-sided=True versus UDF parameter choice. No ABI/default reconciliation authorized.'))
    c.emit('candidate-issues.json',dict(status='OPEN / CANDIDATE_ONLY / UNAPPLIED',activated_fixes=0,issues=[dict(id='C26-ICC-NONFINITE-COUNT-01',reproducer='Original Ui12 CheckData comparison integer guard accepts positive Infinity; subsequent original CInt in RunICC raises OverflowException. Planning26Tests characterizes guard and conversion separately.',actual='Original GUI guard and CInt timing retained; no new blocking Common validation.',proposed='Require finite integral subject observation count before conversion, subject to owner review.',owner='Windows validation owner / later 04A.47',activation_gate='Separate native/behavior authorization.'),dict(id='C26-INDEPENDENT-PROP-DEFAULTS-01',reproducer='Ui12 constructor independent-proportions final defaults p0=.5,p1=.4; ApplyIndependentProportionHypothesisLayout superiority resets .4,.5. SampleSizePlanningDefaults retains both contexts.',actual='Existing control initialization/toggling untouched.',proposed='Owner decides intended initial versus toggled default orientation.',owner='Windows/UDF planning owners / 04A.39–04A.42 and 04A.47',activation_gate='Separate default/output approval.'),dict(id='C26-GUI-UDF-CONTRACT-01',reproducer='SampleSizeUDFs exposes two-sided option for survival sizing; Ui12 passes True. GUI reports full allocation/text while scalar UDF contracts select different fields.',actual='All UDF source/registrations unchanged; GUI branches retain original arguments.',proposed='Inventory intentional differences before any reconciliation.',owner='UDF/scientific owner / 04A.39–04A.42',activation_gate='Separate scientific/public-contract review.')],predecessors='C09/C11/C12, C13 log-rank, C17–C25 including C21 stacked validation and C24 historic oracle scope remain unchanged/unapplied.',native_releases=0,discharges=0))
def receipts():
    commands=[json.loads(p.read_bytes()) for p in sorted((RUN/'commands').glob('*/command.json'))]
    c.emit('command-outcomes.json',dict(commands=commands,all_attempts_retained=True,setup_failures='Import ordering, VB shadowing, old-project XML insertion, UTF-8 reference decoding and typed-hash comparison setup attempts retained; corrected before final source freeze. Zero-test focused filter attempt remains a failed setup receipt.'))
    lanes={cfg:json.loads((RUN/'lanes'/('final-'+cfg+'-summary.json')).read_bytes()) for cfg in ['Debug','Release']}
    for cfg in lanes:
        rows=[r for r in commands if r['name'].startswith('final-'+cfg+'-')]
        lanes[cfg]['commands']=[dict(name=r['name'],started_utc=r['started_utc'],finished_utc=r['finished_utc'],pid=r['pid'],seconds=r['seconds'],exit_code=r['exit_code'],metrics=r['metrics']) for r in rows]
    repeats={phase+'-'+cfg:json.loads((RUN/'lanes'/(phase+'-'+cfg+'-qualification.json')).read_bytes()) for cfg in lanes for phase in ['requalified','qualified'] if (RUN/'lanes'/(phase+'-'+cfg+'-qualification.json')).exists()}
    repeat_commands=[r for r in commands if r['name'].startswith(('requalified-','qualified-Debug','qualified-Release','encoding-'))]
    repeat_start=min(datetime.fromisoformat(r['started_utc']) for r in repeat_commands);repeat_end=max(datetime.fromisoformat(r['finished_utc']) for r in repeat_commands)
    events=sorted([(datetime.fromisoformat(r['started_utc']),1) for r in commands]+[(datetime.fromisoformat(r['finished_utc']),-1) for r in commands]);active=peak=0
    for _,change in events:active+=change;peak=max(peak,active)
    assert peak<=4,peak
    c.emit('parallel-validation-audit.json',dict(configurations=lanes,worker_limit=4,global_measured_max_command_roots=peak,matrix_seconds=sum(v['seconds'] for v in lanes.values()),final_build_seconds=sum(r['seconds'] for r in commands if r['name'].startswith('delivery-build-')),serial_gate_seconds=sum(r['seconds'] for r in commands if r['name'].startswith(('final-serial-','qualified-serial-'))),requalification_command_seconds=sum(r['seconds'] for r in repeat_commands),requalification_seconds=(repeat_end-repeat_start).total_seconds(),requalifications=repeats,scope='Root PID CPU/peak working set only; child aggregates not claimed. Four roots globally verified from UTC intervals. Requalification wall is first-start through last-finish, including review gaps; command sum separate. Copied-bin encoding-only Debug Windows rerun safely overlapped Release qualification, at most four roots and no concurrent builds.',speedup='No comparable sequential baseline measured; no speedup claimed.'))
    start=datetime.fromtimestamp(RUN.stat().st_ctime,timezone.utc);end=datetime.now(timezone.utc)
    c.emit('end-to-end-timing.json',dict(started_utc=start.isoformat(),finished_utc=end.isoformat(),seconds=(end-start).total_seconds(),scope='Entry-freeze directory creation through terminal audit, including development, entry checks, builds, matrices and serial gates; earlier interactive Git/authority preflight was not independently timed. Dossier/manifest sealing follows.'))
    gates={n:json.loads((RUN/'commands'/('final-serial-'+n)/'command.json').read_bytes()) for n in ['strict','profile','mkdocs']}
    for n in gates:
        if (RUN/'commands'/('qualified-serial-'+n)/'command.json').exists():gates[n]=json.loads((RUN/'commands'/('qualified-serial-'+n)/'command.json').read_bytes())
    c.emit('gate-validation.json',dict(gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),serial_commands=gates,strict_reason='Stale review dependency UibyID.vb for graphics.NormalPlot ConstructorBlock; frozen authority not modified.',profile_reason='INTEGRITY_FAILURE, empty error field; retained real exit 1.',native_releases=0,deferred_discharges=0,native_Excel_COM_UIA='NOT_RUN / UNAUTHORIZED',Office_Mac_browser='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED',installer_signing='NOT_RUN',golden_promotion=False))
def closeout():
    assert c.git('branch','--show-current')=='codex/04A-26'
    assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE
    assert not c.git('diff','--cached','--name-only')
    provenance=json.loads((OUT/'sample-size-extraction-provenance.json').read_bytes())
    provenance['transformations']=[v.replace('vbNewLine becomes Environment.NewLine','vbNewLine becomes explicit CRLF through PlanningTextNewLine.Value') for v in provenance['transformations']]
    c.emit('sample-size-extraction-provenance.json',provenance)
    pins=baseline();changed=set(c.git('diff','--name-only').splitlines())
    assert changed<=ALLOWED|{c.PLAN},changed-ALLOWED-{c.PLAN}
    encoding={}
    for p in sorted(changed-{c.PLAN}):
        original=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',c.BASE+':'+p],cwd=ROOT)
        current=(ROOT/p).read_bytes()
        describe=lambda raw:dict(utf8_bom=raw.startswith(b'\xef\xbb\xbf'),crlf=raw.count(b'\r\n'),bare_lf=raw.count(b'\n')-raw.count(b'\r\n'),terminal_newline=raw.endswith(b'\n'))
        encoding[p]=dict(entry=describe(original),final=describe(current))
    c.emit('encoding-audit.json',dict(files=encoding,qualification='Authorized modified files retain entry BOM/LF/terminal-newline conventions after explicit restoration and qualified rebuild. No whole-tree normalization; Core/graphics/UDF/Designer remain byte-exact.'))
    for p,h in pins.items():
        if p not in ALLOWED|{c.PLAN}:assert c.sha(ROOT/p)==h,p
    frozen=json.loads((OUT/'frozen-capture-hashes.json').read_bytes())['files'];differences=[]
    for p,r in frozen.items():
        if c.sha(ROOT/p)!=r['sha256']:
            assert p==c.PLAN,p
            differences.append(dict(path=p,entry_sha256=r['sha256'],current_sha256=c.sha(ROOT/p),classification='OWNER_CONCURRENT_REVISION_2_10 / excluded publication edit; no agent plan write'))
    history=json.loads((RUN/'historical-metadata.json').read_bytes());known={r['path'] for r in history}
    for r in history:
        s=(ROOT/r['path']).stat()
        assert all(r[k]==v for k,v in [('bytes',s.st_size),('mtime_ns',s.st_mtime_ns),('ctime_ns',s.st_ctime_ns),('ino',s.st_ino),('dev',s.st_dev)]),r['path']
    actual=set()
    for folder in [ROOT/'TestResults',*(ROOT/p/'TestResults' for p in ['BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHStatNG.Test'])]:
        for parent,dirs,files in os.walk(folder):
            dirs[:]=[d for d in dirs if not (Path(parent)/d).is_relative_to(ROOT/'TestResults/04A-26')]
            actual.update((Path(parent)/n).relative_to(ROOT).as_posix() for n in files)
    assert known==actual, (len(known-actual),len(actual-known))
    c.emit('retention.json',dict(historical_result_files=len(history),membership_exact=True,metadata_exact=True,private_registry_sha256=c.sha(RUN/'historical-metadata.json'),frozen_authority_files=len(frozen),byte_exact_count=len(frozen)-len(differences),exceptions=differences,scope='Historical results membership+size/mtime/ctime/file identity preserved. Original authority/corpus hashes independently byte-verified; metadata preservation is not falsely described as hashing all large results.'))
    prior=json.loads((ROOT/'evidence/04A25/frozen-capture-hashes.json').read_bytes())['files'];reconciled=[]
    for p,r in prior.items():
        h=r['sha256'] if isinstance(r,dict) else r
        now=c.sha(ROOT/p)
        if now==h:kind='EXACT'
        else:
            raw=(ROOT/p).read_bytes().replace(b'\r\n',b'\n')
            import hashlib
            kind='LINE_ENDINGS_ONLY' if h in {hashlib.sha256(raw).hexdigest(),hashlib.sha256(raw.replace(b'\n',b'\r\n')).hexdigest()} else 'ENTRY_OWNER_AUTHORITY_CHANGE' if p==c.PLAN else 'PRIOR_BASELINE_DIVERGENCE'
        reconciled.append(dict(path=p,prior_sha256=h,current_sha256=now,classification=kind))
    registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())['files']
    for p,r in registry.items():assert c.sha(ROOT/p)==r['sha256'],p
    assert len(registry)==100
    psm=json.loads((ROOT/'evidence/04A25/psm-matching-weighting-balance-same-core-v1.json').read_bytes())
    for bucket in psm['payload_files']:assert c.sha(ROOT/bucket['path'])==bucket['sha256']
    c.emit('historical-hash-reconciliation.json',dict(prior_population=len(prior),records=reconciled,no_history_rewrite=True,mixed_model_original_captures=len(registry),mixed_model_original_hashes_verified=True,retained_psm_records=psm['record_count'],retained_psm_payloads=psm['payload_count'],qualification='Earlier accepted capture registry reconciled against current actual main; entry snapshot remains authority for later preservation.'))
    cross=json.loads((OUT/'original-control-crosswalk.json').read_bytes());native=json.loads((ROOT/'docs/architecture/04A02/native-restriction-crosswalk-v1.json').read_bytes())
    ids={r['id'] for r in cross['controls']}
    c.emit('restriction-retention.json',dict(native_case_overlap={n:sorted(ids&set(r['blocked_control_ids'])) for n,r in native['forms'].items() if ids&set(r['blocked_control_ids'])},controls='original-control-crosswalk.json',original_source_members=3866,overall_controls=2056,overall_open_obligations=1514,importer_04A27_open_obligations=44,populations='Different overlapping populations, not additive. Host control selection includes unrelated modes; no statistical/native release.',releases=0,discharges=0,gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET')))
    delta={}
    old=json.loads((OUT/'prechange-method-provenance.json').read_bytes())['files']
    import hashlib
    for p,record in old.items():
        now=c.methods((ROOT/p).read_text(encoding='utf-8-sig'))
        delta[p]=[n for n,h in record['method_sha256'].items() if hashlib.sha256(now[n].encode()).hexdigest()!=h]
    finalpins={}
    for cfg in ['Debug','Release']:
        build_path=RUN/'builds'/('requalified-build-'+cfg+'.json')
        if cfg=='Debug' and (RUN/'builds/encoding-build-Debug.json').exists():build_path=RUN/'builds/encoding-build-Debug.json'
        if (RUN/'builds'/('qualified-'+cfg+'-build.json')).exists():build_path=RUN/'builds'/('qualified-'+cfg+'-build.json')
        if not build_path.exists():build_path=RUN/'builds'/('delivery-build-'+cfg+'.json')
        build=json.loads(build_path.read_bytes())
        for p,h in build['source_pins'].items():assert c.sha(ROOT/p)==h,p
        initial=json.loads((RUN/'builds'/('delivery-build-'+cfg+'.json')).read_bytes())['source_pins']
        changed_inputs=[p for p,h in initial.items() if h!=build['source_pins'][p]]
        assert set(changed_inputs)<= {'BESHStat.Excel.Common.Tests/src/Agreement26Tests.vb','BESHStat.Excel.Common.Tests/src/Agreement26Reference.vb',c.FORM,'BESHStatNG.Test/BESHStatNG.Tests.vbproj'},changed_inputs
        finalpins[cfg]=dict(build_receipt_sha256=c.sha(build_path),source_pins=build['source_pins'],initial_matrix_changed_inputs=changed_inputs,qualification='Two new test fixtures requalified by full Common/Windows suites. Ui12/test-project entry encoding restored and Windows recompiled/retested. Unchanged Core/Office/Node/other Python already executed; no product semantic/Core/old test change.')
    assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True).returncode==0
    c.emit('final-source-qualification.json',dict(configurations=finalpins,method_deltas=delta,owner_excluded_edits=differences,git_status=c.git('status','--short').splitlines(),untracked=c.git('ls-files','--others','--exclude-standard').splitlines(),index_empty=True,diff_check_exit=0,core_udf_designer_graphics_unchanged=True,post_matrix='Two new test fixtures strengthened after first matrices; full affected Common/Windows and scripts discovery repeated. Entry encoding restored and Windows recompiled/retested. Final Help strict-built; every selected compiled pin matches. Product semantic source and old tests never changed during requalification.'))
    candidates=json.loads((OUT/'candidate-issues.json').read_bytes())
    if not any(r['id']=='C26-RESAMPLING-MINIMUM-01' for r in candidates['issues']):
        candidates['issues'].append(dict(id='C26-RESAMPLING-MINIMUM-01',reproducer='Initial profiles with BootstrapReplicates=30: Deming rejects below 50 even analytical paths; Lin/Bland bootstrap reject below 200; Kappa validation clamps to 200. Final positive fixtures use 200 and explicit Lin/Bland low-replicate rejection remains.',actual='Core policies and original GUI spinner/exception boundaries retained; Common echoes entered replicates without new guard.',proposed='Review whether host capability/help intent should expose method-specific lower bounds or clamp semantics.',owner='Scientific/agreement and Windows owners / later 04A.39–42 and 04A.47',activation_gate='Separate scientific/default and native validation approval.'))
    c.emit('candidate-issues.json',candidates)
    receipts();dependency()
    print('PASS frozen/history/final-source',len(frozen),len(history),'owner excluded',len(differences))
def schemas():
    required={'entry-state.json':['base','branch','gates'],'entry-validation.json':['configurations','qualification'],'validation.json':['configurations','status'],'new-test-inventory.json':['cases','python_cases'],'sample-agreement-same-core-v1.json':['record_count','payload_count','payload_files','status'],'gate-validation.json':['gates','serial_commands'],'retention.json':['historical_result_files','membership_exact'],'final-source-qualification.json':['configurations','index_empty']}
    checked={}
    for p in sorted(OUT.glob('*.json')):
        value=json.loads(p.read_bytes());assert isinstance(value,dict),p
        for key in required.get(p.name,[]):assert key in value,(p,key)
        checked[p.name]=dict(sha256=c.sha(p),bytes=p.stat().st_size,schema='object envelope plus required artifact fields' if p.name in required else 'versioned accepted-pattern object envelope')
    corpus=json.loads((OUT/'sample-agreement-same-core-v1.json').read_bytes());payloads={}
    for b in corpus['payload_files']:
        assert c.sha(ROOT/b['path'])==b['sha256'];payloads.update(json.loads((ROOT/b['path']).read_bytes())['payloads'])
    for r in corpus['records']:assert r['payload_sha256'] in payloads and r['test_outcome']=='Passed'
    for key,payload in payloads.items():assert __import__('hashlib').sha256(json.dumps(payload,ensure_ascii=False,sort_keys=True,separators=(',',':')).encode()).hexdigest()==key
    assert len(payloads)==corpus['payload_count'] and len(corpus['records'])==corpus['record_count']
    c.emit('schema-validation.json',dict(status='PASS',artifacts=checked,required_fields=required,corpus_references_verified=True,qualification='Structural artifact schema and content-address references; no external package installed. Publication manifest seals final bytes independently.'))
if __name__=='__main__':globals()[sys.argv[1]]()
