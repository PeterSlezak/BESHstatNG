"""Read-only predecessor audits and new batch evidence; no native activation."""
import ast
import csv
import hashlib
import importlib.util
import json
from pathlib import Path
import sys

s=importlib.util.spec_from_file_location('c25',Path(__file__).with_name('capture-04a25-contract.py'));c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
ROOT,RUN,OUT=c.ROOT,c.RUN,c.OUT
def digest(v):return hashlib.sha256(json.dumps(v,sort_keys=True,separators=(',',':'),ensure_ascii=False).encode()).hexdigest()
def authority():
    control_path='docs/architecture/WindowsMigrationLedger/ui-controls.csv'
    ledger_path='docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json'
    native_path='docs/architecture/04A02/native-restriction-crosswalk-v1.json'
    controls=list(csv.DictReader((ROOT/control_path).read_text(encoding='utf-8-sig').splitlines()))
    ledger=json.loads((ROOT/ledger_path).read_bytes())
    selected=[r for r in controls if Path(r['path']).name=='Ui20PropensityScoreMatching.Designer.vb']
    ids={r['control_id'] for r in selected}
    obligations=[r for r in ledger if set(r.get('linked_control_ids',[]))&ids]
    related=[r for r in ledger if any(name in json.dumps(r.get('source_evidence',{})) for name in ['PSMdataImport','PsmLovePlotExcel'])]
    native=json.loads((ROOT/native_path).read_bytes())
    c.emit('original-control-crosswalk.json',dict(authorities={p:dict(sha256=c.sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in [control_path,ledger_path,native_path]},controls=[dict(control_id=r['control_id'],path=r['path'],source_row_sha256=digest(r)) for r in selected],obligations=[dict(obligation_id=r['obligation_id'],original_sha256=digest(r),linked_control_ids=r.get('linked_control_ids',[]),status='OPEN',source_restrictions={k:r.get(k) for k in ['blocked_gate','safe_capture_preconditions','acceptance_condition','affected_live_golden_blocked','capture_authorized','verification_status','owner_decision_status','approval_authority']}) for r in obligations],related_source_obligations=[dict(id=r['obligation_id'],sha256=digest(r)) for r in related],native_case_overlap={n:sorted(ids&set(r['blocked_control_ids'])) for n,r in native['forms'].items() if ids&set(r['blocked_control_ids'])},counts=dict(ui20_controls=len(ids),ui20_linked_open_obligations=len(obligations),overall_controls=len(controls),overall_obligations=len(ledger),original_source_members=3866,importer_04A27_obligations=44),populations='Overlapping; never additive. Original obligations remain OPEN; all native and scientific restrictions retained.',releases=0,discharges=0,gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET')))
    print('Ui20',len(ids),'controls',len(obligations),'linked OPEN obligations')
def dependency():
    rows=[]
    for root in ['scripts/tests','BESHstatNG/tools/tests','BESHStatNG_Help_MkDocs/tests','BESHStat.Excel.Common.Tests/src']:
        for path in sorted((ROOT/root).glob('test_*.py')):
            source=path.read_text(encoding='utf-8-sig');tree=ast.parse(source)
            calls=sorted({ast.unparse(n.func) for n in ast.walk(tree) if isinstance(n,ast.Call)})
            writer=any(any(token in n.lower() for token in ['write','mkdir','unlink','rmtree','temporary','copy','subprocess']) for n in calls)
            historical='04a' in path.name.lower() and any(t in source for t in ['sha256','frozen','ledger','TestResults','publication','git'])
            runtime='BESHSTAT_04A02_G3_CANDIDATES' in source or 'BESHSTAT_' in source or '.dll' in source or '.trx' in source
            kind='b_config_dependent_runtime' if runtime else 'd_writer_or_integrity_validator' if writer else 'c_frozen_historical_global_audit' if historical else 'a_static_source_only_configuration_independent'
            methods=[dict(identity=cls.name+'.'+node.name,line=node.lineno) for cls in tree.body if isinstance(cls,ast.ClassDef) for node in cls.body if isinstance(node,(ast.FunctionDef,ast.AsyncFunctionDef)) and node.name.startswith('test_')]
            rows.append(dict(path=path.relative_to(ROOT).as_posix(),sha256=c.sha(path),classification=kind,methods=methods,method_count=len(methods),potential_side_effect_calls=[n for n in calls if any(t in n.lower() for t in ['write','mkdir','unlink','rmtree','temporary','copy','subprocess'])],dependency_scope='Conservative full repository/source/evidence view; historical tests may import recursive scanners. No partial-input cache inferred from AST.',isolation='Lane D only, sequential unittest roots; temporary writers and validators not split.',executions='Full Debug and Release final Python execution required; zero method executions reused.'))
    pins={p:c.sha(ROOT/p) for p in set(c.git('ls-files').splitlines())|set(c.git('ls-files','--others','--exclude-standard').splitlines()) if (ROOT/p).is_file() and p.endswith(('.vb','.vbproj','.cs','.csproj','.py','.ps1','.cjs','.mjs','.yml','.yaml')) and not p.startswith(('evidence/','TestResults/'))}
    c.emit('python-source-dependency-audit.json',dict(version=1,modules=rows,input_source_hashes=pins,classification_basis='AST call inventory plus explicit runtime/environment and historical scanner flags; conservative: possible writers treated as serialized. Full transitive input scope retained, no historical assertion weakened.',reuse=dict(method_executions=0,immutable_inventory_artifact='prechange-source-inventory.json and final-source-qualification.json; result-bearing hashing only, not a duplicated method pass.'),optimization='Only accounting parses each immutable TRX once. Historical scanners/check membership unchanged. No additional Python workers or alternate-batch regression policy enabled.'))
    print('Python modules',len(rows),'AST method declarations',sum(r['method_count'] for r in rows))
def preflight():
    authority();dependency()
    c.emit('parallel-preflight.json',dict(worker_limit=4,fifth_worker=False,runner=dict(path='scripts/verify-04a25.py',sha256=c.sha(ROOT/'scripts/verify-04a25.py')),predecessors={p:c.sha(ROOT/p) for p in ['scripts/verify-04a24.py','scripts/verify-beshstatng.ps1']},builds='Sequential offline --no-restore /m:4; no concurrent MSBuild across configurations.',lanes=dict(A='Core then Common managed',B='Windows then ControlMitigation managed',C='Office/WASM managed then JS/resolver/WASM Node --test-concurrency=1',D='Python/source check commands sequential; D submitted first'),outputs='Unique TestResults/04A-25/run01/commands and executions per invocation. Copied prebuilt binary sandbox, MaxCpuCount=1 and MSTest Workers=1.',writers=['Project-relative TestData copied/hash checked; original bin-local priority retained.','Fixed ../../../../TestResults/Codex destinations resolve beneath per-invocation sandbox, asserted by runner.','BESHSTAT_KR_EXPORT_DIR unique; optional C02 full-suite writer unset.','Separate 15-case semantic-only observation probe feeds Common Python; no additional logical count.','Python Help temp clones serialized; strict/profile/MkDocs/hash/privacy/retention serial after matrix.'],original_verifier='Not executed in-place: its fixed TestResults/Codex output and restore-capable dotnet test would violate protected-history/offline constraints. Same commands adapted through batch-local offline isolated runner, including Legacy.',native='NOT_RUN / UNAUTHORIZED',resources='Root command PID/CPU/working set observed, not all testhost child aggregate metrics; max four command roots. No independent sequential baseline/speedup claim.'))
def provenance():
    files={}
    import re
    for path in [c.FORM,'BESHstatNG/src/DM/PSMdataImport.vb','BESHstatNG/src/Graphics/PsmLovePlotExcel.vb']:
        pre=RUN/'preimages'/Path(path).name;text=pre.read_text(encoding='utf-8-sig')
        indent=8 if 'Graphics' in path else 4
        pattern=r'^'+(' '*indent)+r'(?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^'+(' '*indent)+r'End \1'
        methods={m.group(2):m.group() for m in re.finditer(pattern,text,re.M|re.S)}
        files[path]=dict(sha256=c.sha(pre),methods=methods)
    c.emit('prechange-method-provenance.json',dict(files=files,source='Actual merged main preimages frozen before shipping edits; source LF normalized only in method bodies.'))
def sanitize():
    initial=OUT/'initial-full-matrix-validation.json'
    if not initial.exists():c.emit(initial.name,json.loads((OUT/'validation.json').read_bytes()))
    for path in sorted(OUT.glob('*.json')):
        data=json.loads(path.read_bytes());cleaned=c.clean(data)
        if cleaned!=data:c.emit(path.name,cleaned)
    (RUN/'accounting-attempt-01.json').write_text(json.dumps(dict(action='Stopped own accounting helper with tool-session Ctrl-C, exit 1; no test process stopped.',command='summarize-04a25.py final',pid=15548,reason='Repeated immutable large-TRX hashing; changed to metadata-keyed cache plus terminal SHA reread.',last_observed_root_cpu_seconds=183.578125,attempt_outputs='Initial validation/nonpasses files retained; raw test/TRX/command receipts remain immutable.',read_only_process_probe='Optional CIM metadata lookup denied; no elevation or global permissions changed.')),encoding='utf-8')
def publication_context():
    pre=json.loads((OUT/'parallel-preflight.json').read_bytes())
    if not (OUT/'parallel-preflight-initial-v1.json').exists():c.emit('parallel-preflight-initial-v1.json',pre)
    pre.update(final_runner=dict(path='scripts/verify-04a25.py',sha256=c.sha(ROOT/'scripts/verify-04a25.py')),post_matrix_helper_changes='Evidence privacy, stable source/case inventory fingerprints, immutable receipt-hash caching, run-ID selection and standalone source requalification action. Entry/final copied-bin routing is now explicit for reproducibility. No product/compiled test/Python test source changed. Actual executing runner hashes remain in every private command receipt.',fresh_run='Set BESHSTAT_04A25_RUN to a new alphanumeric/hyphen/underscore identifier; never overwrite an old run. Repeat build names and matrix names inside that new isolated root.')
    c.emit('parallel-preflight.json',pre);dependency()
    audit=json.loads((OUT/'documentation-audit.json').read_bytes());baseline=json.loads((RUN/'baseline.json').read_bytes())['pins']
    assets={p:dict(entry_sha256=h,final_sha256=c.sha(ROOT/p)) for p,h in baseline.items() if '/205propensityscorematching/' in p}
    assert len(assets)==7 and all(r['entry_sha256']==r['final_sha256'] for r in assets.values())
    audit['original_assets']=assets;c.emit('documentation-audit.json',audit)
    lanes=json.loads((OUT/'parallel-validation-audit.json').read_bytes());commands=[json.loads(p.read_bytes()) for p in (RUN/'commands').glob('*/command.json')]
    lanes['final_build_seconds']=sum(r['seconds'] for r in commands if r['name'].startswith('delivery-build-'))
    lanes['final_serial_gate_seconds']=sum(r['seconds'] for r in commands if r['name'].startswith('final-serial-'))
    lanes['source_requalification_seconds']=sum(r['seconds'] for r in commands if r['name'].startswith('requalified-'))
    lanes['accounting_attempt']=json.loads((RUN/'accounting-attempt-01.json').read_bytes())
    c.emit('command-outcomes.json',dict(commands=commands,all_attempts_retained=True,terminal_focused_checks='Additional terminal source guards are excluded from distinct final-suite totals.'))
    lanes['root_pid_records']=[dict(name=r['name'],pid=r['pid'],seconds=r['seconds'],metrics=r['metrics']) for r in commands]
    c.emit('parallel-validation-audit.json',lanes)
def closeout():
    import os
    import subprocess
    from datetime import datetime,timezone
    assert c.git('branch','--show-current')=='codex/04A-25'
    assert c.git('rev-parse','HEAD')==c.git('rev-parse','main')==c.BASE
    assert not c.git('diff','--cached','--name-only')
    changed=set(c.git('diff','--name-only').splitlines())
    allowed={c.FORM,'BESHstatNG/src/DM/PSMdataImport.vb','BESHstatNG/src/Graphics/PsmLovePlotExcel.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/propensity-score-matching.md'}
    assert changed<=allowed,changed-allowed
    baseline=json.loads((RUN/'baseline.json').read_bytes())['pins']
    for p,h in baseline.items():
        if p not in allowed:assert c.sha(ROOT/p)==h,p
    frozen=json.loads((OUT/'frozen-capture-hashes.json').read_bytes())['files']
    for p,r in frozen.items():assert c.sha(ROOT/p)==r['sha256'],p
    before=json.loads((OUT/'prechange-method-provenance.json').read_bytes())['files']
    delta={}
    for p,r in before.items():
        text=(ROOT/p).read_text(encoding='utf-8-sig')
        if 'Graphics' in p:
            import re
            now={m.group(2):m.group() for m in re.finditer(r'^        (?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^        End \1',text,re.M|re.S)}
        else:now=c.methods(text)
        delta[p]=dict(modified=[n for n,b in r['methods'].items() if now.get(n)!=b],added=sorted(set(now)-set(r['methods'])),unchanged=[n for n,b in r['methods'].items() if now.get(n)==b])
    p='BESHstatNG/src/Graphics/PsmLovePlotExcel.vb'
    legacy=before[p]['methods']['AddChart'];current=(ROOT/p).read_text(encoding='utf-8-sig')
    original_tail=legacy[legacy.index('            Dim shp'):].replace('cleanRows.Count','projection.Count')
    actual_tail=current[current.index('            Dim shp'):current.index('        End Sub',current.index('            Dim shp'))+len('        End Sub')]
    assert original_tail==actual_tail,'Native chart geometry/style tail changed beyond row count reference'
    state=json.loads((OUT/'source-preservation.json').read_bytes())
    state.update(final_changes={p:dict(entry_sha256=baseline[p],final_sha256=c.sha(ROOT/p)) for p in sorted(changed)},method_delta=delta,unchanged_tracked_files=len(baseline)-len(changed),native_chart_tail_equal_after_count_reference=True,frozen_hashes_verified=len(frozen))
    c.emit('source-preservation.json',state)
    history=json.loads((RUN/'historical-metadata.json').read_bytes());expected={r['path']:r for r in history};current_history=set();errors=[]
    for folder in [ROOT/'TestResults',*(ROOT/p/'TestResults' for p in ['BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHStatNG.Test'])]:
        for parent,dirs,files in os.walk(folder):
            dirs[:]=[d for d in dirs if not (Path(parent)/d).is_relative_to(ROOT/'TestResults/04A-25')]
            for name in files:
                path=Path(parent)/name;relative=path.relative_to(ROOT).as_posix();current_history.add(relative)
                if relative not in expected:errors.append(dict(path=relative,reason='new historical member'));continue
                st=path.stat();row=expected[relative]
                if (st.st_size,st.st_mtime_ns,st.st_ctime_ns,st.st_ino,st.st_dev)!=tuple(row[k] for k in ['bytes','mtime_ns','ctime_ns','ino','dev']):errors.append(dict(path=relative,reason='metadata changed'))
    errors += [dict(path=p,reason='missing') for p in expected.keys()-current_history]
    (RUN/'retention-final.json').write_text(json.dumps(dict(errors=errors,files=len(history),finished_utc=datetime.now(timezone.utc).isoformat())),encoding='utf-8')
    assert not errors,errors[:10]
    c.emit('retention.json',dict(status='PASS_HISTORICAL_MEMBERSHIP_METADATA',historical_files=len(history),membership_metadata_errors=0,frozen_byte_hashes=len(frozen),receipt=dict(alias='LOCAL_RETENTION:retention-final',sha256=c.sha(RUN/'retention-final.json')),scope='All initial historical result file identities/size/creation-write timestamps/membership. Authorities/corpora additionally SHA-256 checked, not an all-TestResults-byte reread.',releases=0,discharges=0))
    prior=json.loads((ROOT/'evidence/04A24/frozen-capture-hashes.json').read_bytes())['files'];comparisons=[]
    for p,row in prior.items():
        data=(ROOT/p).read_bytes();actual=c.sha(ROOT/p);normalized=data.replace(b'\r\n',b'\n')
        variants={__import__('hashlib').sha256(v).hexdigest() for v in [normalized,normalized.replace(b'\n',b'\r\n')]}
        kind='EXACT' if actual==row['sha256'] else 'LINE_ENDINGS_ONLY' if row['sha256'] in variants else 'ENTRY_DIVERGENCE_RETAINED'
        comparisons.append(dict(path=p,prior_sha256=row['sha256'],actual_entry_sha256=frozen[p]['sha256'],current_sha256=actual,classification=kind))
    from collections import Counter
    registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())['files']
    for p,row in registry.items():assert c.sha(ROOT/p)==row['sha256'],p
    assert len(registry)==100
    c.emit('historical-hash-reconciliation.json',dict(prior_04A24_hash_population=len(prior),classifications=dict(Counter(r['classification'] for r in comparisons)),comparisons=comparisons,actual_entry_frozen_hash_population=len(frozen),mixed_model_captures=100,mixed_model_original_hashes_verified=True,policy='No prior file or historical expected value repaired, normalized or rewritten. Every current byte equals this batch actual entry; historical source-record differences are recorded explicitly, never forced to old ZIP hashes.'))
    builds={}
    for config in ['Debug','Release']:
        build=json.loads((RUN/'builds'/('delivery-build-'+config+'.json')).read_bytes())
        for p,h in build['source_pins'].items():assert c.sha(ROOT/p)==h,p
        builds[config]=dict(receipt='LOCAL_BUILD:delivery-build-'+config,sha256=c.sha(RUN/'builds'/('delivery-build-'+config+'.json')),compiled_members=len(build['source_pins']))
    candidates=sorted(changed|set(c.git('ls-files','--others','--exclude-standard').splitlines()))
    final_inputs={p:c.sha(ROOT/p) for p in candidates if p.endswith(('.vb','.vbproj','.cs','.csproj')) or p.startswith('scripts/tests/')}
    c.emit('final-source-qualification.json',dict(configurations=builds,compiled_sources_unchanged_after_final_suites=True,final_input_pins=final_inputs,qualifications='Both final matrices use the pinned compiled/test bytes. Evidence/accounting-only edits require hash/privacy/manifest rechecks; no product or test-source edits after these final matrices.',native='NOT_RUN / UNAUTHORIZED'))
    page='BESHStatNG_Help_MkDocs/docs/methods/propensity-score-matching.md';meta='BESHStatNG_Help_MkDocs/docs/_meta/methods.yml'
    c.emit('documentation-audit.json',dict(page=dict(path=page,entry_sha256=baseline[page],final_sha256=c.sha(ROOT/page)),metadata_sha256=c.sha(ROOT/meta),metadata_unchanged=c.sha(ROOT/meta)==baseline[meta],original_screenshots_and_example_assets_unchanged=True,strict_mkdocs={phase:json.loads((RUN/'commands'/(phase+'-serial-mkdocs')/'command.json').read_bytes())['exit_code'] for phase in ['entry','final']},clarifications='Retained source-row audit versus dropped alignment count; duplicate labels versus match sets; optional source-table/chart warning and method-specific output availability. Windows only; no Office/Mac or chart appearance claim.'))
    commands={phase:{name:json.loads((RUN/'commands'/(phase+'-serial-'+name)/'command.json').read_bytes())['exit_code'] for name in ['strict','profile','mkdocs']} for phase in ['entry','final']}
    c.emit('gate-validation.json',dict(gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),serial_commands=commands,native_releases=0,deferred_discharges=0,native='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED',golden_promotion=False))
    c.emit('final-git-state.json',dict(head=c.git('rev-parse','HEAD'),main=c.git('rev-parse','main'),branch=c.git('branch','--show-current'),status=c.git('status','--short').splitlines(),index_empty=True,owner_plan_sha256=c.sha(ROOT/c.PLAN),diff_check_exit=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True).returncode,publication='UNSTAGED / UNCOMMITTED / NOT_PUSHED'))
    start=datetime.fromtimestamp(RUN.stat().st_ctime,timezone.utc);end=datetime.now(timezone.utc)
    c.emit('end-to-end-timing.json',dict(started_utc=start.isoformat(),finished_utc=end.isoformat(),seconds=(end-start).total_seconds(),scope='Measured from private entry-freeze directory creation through terminal audit; includes entry/final builds/matrices, development and serial gates. Earlier interactive Git/authority preflight was not independently timed. Final dossier/manifest sealing follows this receipt and is timed separately.'))
    print('PASS source/frozen/history/qualification',len(history),len(frozen))
if __name__=='__main__':globals()[sys.argv[1]]()
