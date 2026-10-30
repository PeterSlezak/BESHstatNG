"""Regression UDF export checkpoint11; historical reports stay immutable."""
from __future__ import annotations
import argparse,collections,hashlib,json,re
import catalog,semantic,continuation03_report as logs,continuation07_report as prior
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-11'
BASE='BESHstatNG/src/ExcelUDFs/Regression/'
TARGETS={BASE+'MixedModelUDFs.vb':('A',38),BASE+'LmmUDFs.vb':('B',25)}
EXPECTED_EXPORTS=dict(zip(TARGETS,(15,15)))
REG_FIELDS=('export_id','path','attribute_line','attribute_end_line','attribute','effective_name','symbol_id','implementing_name','signature','start_line','end_line')
FIELDS=prior.FIELDS+('registration_evidence',);CHECK=False
def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Checkpoint11 drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def historical_errors(root,records):
    errors=prior.historical_errors(root,records)
    for a in records:
        if a['path'].rsplit('/',1)[-1].startswith(('continuation07','continuation08','continuation09','continuation10')):
            p=root/a['path']
            if not p.is_file() or sha(p)!=a['sha256']:errors.append('Historical07/08 evidence changed: '+a['path'])
    return errors
def raw_history_errors(root,records):
    errors=[]
    for a in records:
        p=root/a['path']
        if not p.is_file() or p.stat().st_size!=a['bytes']:errors.append('Prior raw evidence missing/size changed: '+a['path']);continue
        h=hashlib.sha256()
        with p.open('rb') as f:
            for block in iter(lambda:f.read(1024*1024),b''):h.update(block)
        if h.hexdigest()!=a['sha256']:errors.append('Prior raw evidence hash changed: '+a['path'])
    return errors
def preservation():
    b=read(EVIDENCE/'before.json')
    assert git('branch','--show-current')==b['branch'] and git('rev-parse','HEAD')==b['head']
    assert git('rev-parse','main')==b['head'] and not git('diff','--name-only') and not git('diff','--cached','--name-only')
    for label,records,base in [('protected',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('snapshot10',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/a['path']).is_file() and sha(base/a['path'])==a['sha256'] for a in records),label+' mismatch'
    errors=historical_errors(ROOT,b['original_artifacts']);assert not errors,errors
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    files=catalog.load(OUT,'files.csv');assert len(files)==171 and all(sha(ROOT/f['path'])==f['sha256'] for f in files)
    raw=read(EVIDENCE/'prior-raw-evidence-hashes.json')['files'];errors=raw_history_errors(ROOT,raw);assert not errors,errors
    old=(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
    assert not prior_decision_errors((semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig'),old)
    return dict(branch=b['branch'],head=b['head'],tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(files),snapshot=len(b['original_artifacts']),baseline=len(b['baseline']['entries']),prior_raw_files=len(raw),prior_decision_objects=1199,historical='02-10 reports, snapshots and all recorded raw files unchanged;1199 original decision objects exact raw UTF8 bytes')
def scope_errors(symbols,decisions,queue):
    source={s['symbol_id']:s for s in symbols};pins={d.get('symbol_id'):d for d in decisions};errors=[]
    exports=catalog.load(OUT,'udf-exports.csv')
    reviewed=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-11']
    if len(reviewed)!=63 or len({d['symbol_id'] for d in reviewed})!=63:errors.append('checkpoint11 missing/duplicate or out-of-scope decision count')
    for d in reviewed:
        if json.loads(d.get('source_identity','{}')).get('path') not in TARGETS:errors.append('checkpoint11 outside scope ID: '+d['symbol_id'])
    for p,(group,count) in TARGETS.items():
        ids={s['symbol_id'] for s in queue if s['path']==p}
        if len(ids)!=count or sum(s['path']==p for s in symbols)!=count:errors.append('checkpoint11 source/queue denominator: '+p)
        for ident in ids:
            s=source.get(ident);d=pins.get(ident)
            if s is None or d is None or not semantic.qualified(s) or not semantic.qualified(d):errors.append('checkpoint11 missing/unqualified ID: '+ident);continue
            if d.get('checkpoint_id')!='04A.01-continuation-11' or d.get('review_group')!=group:errors.append('checkpoint11 incorrect group: '+ident)
            if d.get('body_sha256')!=s['body_sha256'] or d.get('signature_key')!=s['signature_key']:errors.append('checkpoint11 top-level source pin drift: '+ident)
            if d.get('source_kind')!=s['kind'] or d.get('source_enclosing_symbol_id')!=s.get('enclosing_symbol_id',''):errors.append('checkpoint11 kind/enclosing drift: '+ident)
            recorded=json.loads(d.get('registration_evidence','[]'))
            project=lambda rows:[{k:r.get(k,'') for k in REG_FIELDS} for r in rows]
            if project(recorded)!=project([e for e in exports if e['symbol_id']==ident]):errors.append('checkpoint11 member annotation evidence drift: '+ident)
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'checkpoint11')
            if d.get('equivalence_scope')=='whole-method' or d.get('whole_wrapper_equivalence')!='not asserted; host registration/marshalling/cache and specified policy remain separate':errors.append('checkpoint11 false whole-Core wrapper equivalence')
            if d.get('source_sha256')!=sha(ROOT/s['path']):errors.append('checkpoint11 source file pin drift')
    return errors
def export_rows(selected,exports):
    values=prior.export_rows(selected,exports)
    for value in values:
        records=[{k:e[k] for k in REG_FIELDS} for e in exports if e['symbol_id']==value['symbol_id']]
        value['registration_source_evidence']=json.dumps(records,sort_keys=True)
        value['attribute_evidence_status']='Actual parsed ExcelFunction annotations; public visibility alone is not registration'
    return values
def registration_errors(exports,original):
    project=lambda rows:sorted(({k:r.get(k,'') for k in REG_FIELDS} for r in rows),key=lambda r:r['export_id'])
    errors=[]
    if len(exports)!=268 or len({e['export_id'] for e in exports})!=268:errors.append('Global export count/duplicate drift')
    if project(exports)!=project(original):errors.append('Export annotation/name/signature/source identity changed')
    for path,count in EXPECTED_EXPORTS.items():
        if sum(e['path']==path for e in exports)!=count:errors.append('Per-file registration denominator drift: '+path)
    return errors
def export_errors(selected,exports,rows):
    errors=[] if export_rows(selected,exports)==rows else ['checkpoint11 export/helper mapping drift']
    if sum(e['symbol_id'] in {s['symbol_id'] for s in selected} for e in exports)!=30:errors.append('checkpoint11 requires actual30 exported target IDs')
    if len(rows)!=63 or len({r['symbol_id'] for r in rows})!=63:errors.append('checkpoint11 export/helper member omission or duplicate')
    return errors
def prior_decision_errors(current_text,original_text):
    def objects(text):
        decoder=json.JSONDecoder();pos=text.index('[')+1;out={}
        while True:
            while pos<len(text) and (text[pos].isspace() or text[pos]==','):pos+=1
            if text[pos]==']':return out
            start=pos;value,pos=decoder.raw_decode(text,pos);key=value.get('symbol_id') or value['signature_key']
            if key in out:raise ValueError('Duplicate ownership decision: '+key)
            out[key]=text[start:pos].encode('utf-8')
    now,old=objects(current_text),objects(original_text)
    return ['Prior decision bytes altered: '+key for key,value in old.items() if now.get(key)!=value]
def operation_rows(selected):
    values=prior.operation_rows(selected)
    for v in values:
        v['status']='NEW PROVISIONAL GROUP' if v['operation_id'].startswith('C11-') else 'EXTENSION OF EXISTING GROUP; NOT A NEW OPERATION'
        members=[s for s in selected if s.get('candidate_operation_id')==v['operation_id'] and s['disposition'].startswith('extract-to-')]
        v['owner']='; '.join(sorted({s.get('owner_role','') for s in members}))
        v['freeze_status']='NOT FROZEN; member rows and deduplicated operations are distinct'
    return values
def operation_errors(selected,operations):
    errors=[] if operation_rows(selected)==operations else ['checkpoint11 operation mapping drift']
    existing={r['operation_id'] for n in ('03','04','05','06','07','08','09','10') for r in catalog.load(OUT,'continuation'+n+'-candidate-operations.csv')}
    for o in operations:
        if not o['operation_id'].startswith('C11-') and o['operation_id'] not in existing:errors.append('Unknown claimed prior operation: '+o['operation_id'])
        if not o.get('owner') or not o['operation_id']:errors.append('Candidate operation lacks declared owner or identity')
    for s in selected:
        if s['disposition'].startswith('extract-to-') and (not s.get('owner_role') or not s.get('candidate_operation_id') or not s.get('future_batch')):errors.append('Candidate member lacks declared operation owner')
    return errors

prior_decision_errors=__import__("continuation08_report").prior_decision_errors

EXTRA=('whole_wrapper_equivalence','scientific_parity_status','reversibility','prior_review_dependencies','member_identity_key','registration_class','portability_classification','future_crosscutting_batches','review_contract','review_status','evidence_status','confidence','source_sha256')
FIELDS=FIELDS+EXTRA

def alignment_errors(rows):
    errors=[]
    source={s['symbol_id']:s for s in catalog.load(OUT,'symbols.csv')}
    portable=semantic.portable_sources()
    ids=[]
    for row in rows:
        ids.append(row.get('alignment_id'))
        if any(not row.get(k) for k in ('gui_entry','future_batch','candidate_shared_policy','current_udf_behavior','current_gui_behavior','current_core_behavior')):errors.append('alignment missing policy/GUI/Core evidence')
        if row.get('udf_id') not in source or source[row['udf_id']]['path'] not in TARGETS:errors.append('alignment UDF outside scope')
        if not row.get('owner') or not row.get('future_fixture') or row.get('runtime_status','').startswith('PASS'):errors.append('alignment missing owner/fixture or false runtime pass')
        try:
            if not json.loads(row['gui_pins']):errors.append('alignment missing GUI pin')
            for recorded,authority in [(json.loads(row['udf_pin']),source),(json.loads(row['core_pin']),portable)]+[(p,source) for p in json.loads(row['gui_pins'])]:
                if not all(recorded.get(k) for k in ('symbol_id','path','containing_type','signature','start_line','end_line','body_sha256')):errors.append('alignment incomplete declaration pin');continue
                actual=authority.get(recorded['symbol_id'])
                if actual is None or any(actual.get(k)!=v for k,v in recorded.items()):errors.append('alignment stale declaration pin')
            for path,digest in json.loads(row['dependency_file_pins']).items():
                if sha(ROOT/path)!=digest:errors.append('alignment stale dependency')
        except (KeyError,ValueError,TypeError):errors.append('alignment invalid pin')
    if len(ids)!=14 or len(set(ids))!=14:errors.append('alignment missing/duplicate candidate')
    return errors

def acceptance_errors(summary,index):
    open_work=summary.get('unreviewed_for_acceptance',0)>0 or summary.get('qualified_full_control_contracts',0)<summary.get('control_review_denominator',0)
    if open_work and (summary.get('status')!='NOT READY FOR ACCEPTANCE' or index.get('acceptance_status')!='NOT READY FOR ACCEPTANCE'):
        return ['Misleading READY status while member/control obligations remain']
    return []

def comparison_errors(rows):
    errors=[];portable=semantic.portable_sources()
    if len(rows)!=60 or len({r.get('symbol_id') for r in rows})!=60:errors.append('Missing/duplicate mixed-model comparison declaration')
    for row in rows:
        actual=portable.get(row.get('symbol_id'))
        if actual is None or any(row.get(k)!=actual.get(k) for k in ('path','containing_type','signature','start_line','end_line','body_sha256','span_start','span_length')):
            errors.append('Stale mixed-model comparison declaration');continue
        p=ROOT/row['path'];raw=p.read_bytes().decode('utf-8-sig')
        if sha(p)!=row.get('source_sha256') or raw[int(actual['span_start']):int(actual['span_start'])+int(actual['span_length'])]!=row.get('body_excerpt'):
            errors.append('Stale mixed-model comparison body/file pin')
    return errors

def fixture_errors(rows):
    errors=[]
    if len(rows)!=9 or len({r.get('fixture_id') for r in rows})!=9:errors.append('Missing/duplicate existing fixture pin')
    for row in rows:
        try:
            lines=(ROOT/row['source']).read_text(encoding='utf-8-sig').splitlines()
            if sha(ROOT/row['source'])!=row['source_sha256'] or '\n'.join(lines[int(row['start_line'])-1:int(row['end_line'])])!=row['declaration_and_body']:errors.append('Stale existing test declaration/body')
            for path,digest in json.loads(row['dependency_file_pins']).items():
                if sha(ROOT/path)!=digest:errors.append('Stale existing fixture input/reference')
        except (ValueError,KeyError,FileNotFoundError):errors.append('Invalid existing fixture pin')
    return errors

def validation():
    import xml.etree.ElementTree as ET
    runs=read(EVIDENCE/'validation-runs.json')
    managed=[];other=[]
    for config in ('Debug','Release'):
        for name in ('Core.trx','Common.trx','OfficeAddin.trx','OfficeExecution.trx','Legacy.trx',f'OfficeAddin-{config}.trx'):
            p=EVIDENCE/config/name
            if p.exists():
                doc=ET.parse(p)
                identities=[{k:r.get(k) for k in ('testId','testName','outcome')} for r in doc.findall('.//{*}UnitTestResult')]
                managed.append(dict(configuration=config,suite=name,**doc.find('.//{*}Counters').attrib,execution_times=doc.find('.//{*}Times').attrib,distinct_full_suite=name!=f'OfficeAddin-{config}.trx',identities=identities,path=p.relative_to(ROOT).as_posix(),sha256=sha(p)))
        for kind in ('full','foundation'):
            p=EVIDENCE/config/(kind+'.log');contents=logs.text(p)
            other.append(dict(configuration=config,kind=kind,path=p.relative_to(ROOT).as_posix(),sha256=sha(p),python_suites=re.findall(r'Ran (\d+) tests? in',contents),node_test_counts=re.findall(r'(?:# |\u2139 )tests (\d+)',contents),node_pass_counts=re.findall(r'(?:# |\u2139 )pass (\d+)',contents),node_fail_counts=re.findall(r'(?:# |\u2139 )fail (\d+)',contents),node_skipped_counts=re.findall(r'(?:# |\u2139 )skipped (\d+)',contents),node_test_identities=re.findall(r'^\s*\u2714 (.+?) \([\d.]+ms\)',contents,re.M|re.S),python_test_identities=[dict(name=name,qualified=''.join(qualified.split())) for name,qualified in re.findall(r'\b(test_\w+)\s+\(([^)]+)\)\s+\.\.\.',contents)]))
    checks=read(EVIDENCE/'final-checks.json') if (EVIDENCE/'final-checks.json').exists() else read(EVIDENCE/'checks.json') if (EVIDENCE/'checks.json').exists() else []
    neg=EVIDENCE/('negative-reconciliation-final.log' if (EVIDENCE/'negative-reconciliation-final.log').exists() else 'negative-reconciliation.log');negative={}
    if neg.exists():
        contents=logs.text(neg);names=sorted(set(re.findall(r'(?:^|\n|python\.exe : )(test_\w+)',contents)))
        counts=re.findall(r'Ran (\d+) tests? in',contents)
        assert counts and len(names)==int(counts[-1]) and re.search(r'\nOK\s*$',contents),'entire final negative suite did not pass'
        negative=dict(distinct_tests=len(names),identities=names,log=neg.relative_to(ROOT).as_posix(),sha256=sha(neg))
    generated=[dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size,sha256=sha(p)) for config in ('Debug','Release') for p in (EVIDENCE/config).rglob('*.vb')]
    runtime=read(OUT/'continuation11-runtime.json')
    errors=runtime_errors(runtime,runs+checks);assert not errors,errors
    first=read(EVIDENCE/'first-checks.json') if (EVIDENCE/'first-checks.json').exists() else []
    runtime['initial_failed_checks']=first
    runtime['initial_authoring_corrections']=[
        'Before save: corrected nonexistent importer filename and Core contrast helper name using actual source; no product edits or decision writes from failed author invocations.',
        'Group A initial snapshot had31 exception_tags empty: schema requires nonblank even when none apply. Explicit none correction preserved original group evidence; all38 now qualify.',
        'Clarified LMM stack dispatch to existing UdfOutputTables helper, not MMRM Private duplicate; new11 objects only.',
        'Detected Windows newline translation of old object formatting during new-only contract correction. Parsed old values unchanged; exact1199 original objects restored from lossless snapshot and raw-byte verified. Drift copy and correction evidence retained.'
    ]
    return dict(scope='Fresh continuation11, approved WinPython3.14 only; static assertions do not establish native/scientific parity',runs=runs,checks=checks,initial_commands=read(EVIDENCE/'initial-commands.json'),managed=managed,python_node=other,negative=negative,transient_generated_copies=generated,protection=read(EVIDENCE/'protection.json') if (EVIDENCE/'protection.json').exists() else 'Pending final guard',runtime=runtime,not_run=['native Excel/COM/XLL/Windows UI/workbook recalc and lifetime','Mac/authenticated Office','R scripts/new numerical golden generation','installer/signing/deployment/network installation','04A.02 or later extraction'],development_notes=['A38/B25 checkpoints retained; original1199 decision objects literal bytes checked.','Whole wrappers are not already portable; existing Core inference reused. No product fixtures changed.'])


APPROVED_PYTHON='C:/Users/peter/Desktop/WPy64-31470/python/python.exe'
def runtime_errors(runtime,commands):
    errors=[]
    if runtime.get('executable','').replace('\\','/').lower()!=APPROVED_PYTHON.lower():errors.append('Unapproved fresh Python runtime')
    for command in commands:
        text=json.dumps(command.get('command',[]))+' '+str(command.get('exe',''))+' '+str(command.get('arguments',[]))
        if 'WPy64-31150' in text or 'python-3.11.5' in text:errors.append('Fresh command selects retired Python')
    return errors

def main():
    b=read(EVIDENCE/'before.json'); oldroot=EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
    symbols=catalog.load(OUT,'symbols.csv');decisions=semantic.read_store('review-decisions.json')
    queue=[s for s in catalog.load(oldroot,'remaining-member-review.csv') if s['path'] in TARGETS]
    errors=scope_errors(symbols,decisions,queue);assert not errors,errors
    oldtext=(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_text(encoding='utf-8-sig')
    assert not prior_decision_errors((semantic.TOOLING/'review-decisions.json').read_text(encoding='utf-8-sig'),oldtext)
    assert len(json.loads(oldtext))==1199
    ids={s['symbol_id'] for s in queue};selected=sorted((s for s in symbols if s['symbol_id'] in ids),key=lambda s:(s['path'],int(s['span_start'])))
    assert len(selected)==63
    current={d['symbol_id']:d for d in decisions}
    assert not semantic.verify(symbols,catalog.load(OUT,'ui-wiring.csv'),catalog.load(OUT,'ui-controls.csv'))
    sem=read(OUT/'semantic-review-coverage.json');coverage=read(OUT/'review-coverage.json')
    assert not acceptance_errors(sem,read(OUT/'index.json'))
    exports=catalog.load(OUT,'udf-exports.csv');old_exports=catalog.load(oldroot,'udf-exports.csv')
    assert not registration_errors(exports,old_exports)
    mapped=export_rows(selected,exports);assert not export_errors(selected,exports,mapped)
    operations=operation_rows(selected);assert not operation_errors(selected,operations)
    align=catalog.load(OUT,'continuation11-gui-udf-alignment.csv');assert not alignment_errors(align)
    assert not comparison_errors(catalog.load(OUT,'continuation11-portable-comparisons.csv'))
    assert not fixture_errors(catalog.load(OUT,'continuation11-fixture-crosswalk.csv'))
    reviewed_alignment=read(EVIDENCE/'alignment-reviewed.json')
    assert align==reviewed_alignment,'Reviewed alignment text/pins changed'
    rich=[{**s,**current[s['symbol_id']]} for s in selected]
    write('continuation11-member-reviews.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in rich]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:({**s,**current.get(s['symbol_id'],{})}).get(k,'') for k in FIELDS} for s in candidates]))
    write('continuation11-export-helper-map.csv',catalog.render(mapped))
    filecounts=[dict(path=p,group=g,records=n,reviewed=sum(semantic.qualified(s) for s in selected if s['path']==p),exports_before=sum(e['path']==p for e in old_exports),exports_after=sum(e['path']==p for e in exports),nonexports=sum(r['path']==p and r['excel_function_export']=='no' for r in mapped)) for p,(g,n) in TARGETS.items()]
    write('continuation11-export-counts.csv',catalog.render(filecounts+[dict(path='ALL WINDOWS EXPORTS',group='global',records=3866,reviewed=sem['qualified_source_reviews'],exports_before=268,exports_after=268,nonexports='outside scope')]))
    write('continuation11-candidate-operations.csv',catalog.render(operations))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=x,allocation_evidence=e,records=n) for (f,r,d,x,e),n in sorted(reasons.items())]))
    memberops=[dict(symbol_id=s['symbol_id'],operation_id=s.get('candidate_operation_id',''),owner=s['owner_role'],destination=s['destination'],future_batch=s['future_batch'],disposition=s['disposition']) for s in selected]
    write('continuation11-operation-members.csv',catalog.render(memberops))
    state=[]
    for row in catalog.load(OUT,'state.csv'):
        if row['path'] not in TARGETS:continue
        members=[s for s in selected if s['path']==row['path']]
        state.append(dict(**row,source_sha256=sha(ROOT/row['path']),initializer_review_ids='; '.join(s['symbol_id'] for s in members if s['kind']=='VariableDeclarator'),assignment_review_ids='; '.join(s['symbol_id'] for s in members if s['name'].endswith('_FIT')),classification='Windows session handle container; model numeric state remains existing Core. Uninitialized property has no additional executable census record.',future_owner='Excel-DNA lifecycle maintainer / regression request maintainer',future_batch='04A.43; 04A.40',runtime_status='Lifetime/recalculation/concurrency NOT ESTABLISHED; see named member fixtures'))
    write('continuation11-handle-state.csv',catalog.render(state))
    for group in ('A','B'):
        snapshot_path=EVIDENCE/('group-'+group+'-member-evidence.json')
        snap=read(snapshot_path)
        snap['original_snapshot']=dict(path=snapshot_path.relative_to(ROOT).as_posix(),sha256=sha(snapshot_path),initial_claimed_qualified=snap['qualified'],initial_contract_qualified=sum(semantic.qualified(d) for d in snap['decisions']),status='Immutable historical group draft; explicit refinements retained separately')
        snap['final_qualified_ids']=[s['symbol_id'] for s in selected if current[s['symbol_id']]['review_group']==group]
        snap['decisions']=[current[ident] for ident in snap['final_qualified_ids']]
        assert all(semantic.qualified(d) for d in snap['decisions'])
        snap['qualified']=len(snap['decisions'])
        snap['refinement_evidence']='Original group evidence retained; see group-A-contract-correction.json, source-refinements.json, operation-refinement.json and newline-correction.json in raw11 evidence. Final exact qualified IDs below are authoritative.'
        emit('continuation11-group-'+group+'.json',snap)
    emit('continuation11-start.json',{k:b[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    deltas={key:dict(before=before,after=after,delta=after-before) for key,before,after in (
      ('qualified_member_reviews',b['starting_review']['qualified_source_reviews'],sem['qualified_source_reviews']),
      ('remaining_member_reviews',b['starting_review']['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),
      ('ownership_investigations',b['starting_coverage']['member_investigations'],coverage['member_investigations']),
      ('unresolved_edges',b['starting_coverage']['unresolved_edge_rows'],coverage['unresolved_edge_rows']),
      ('full_controls',b['starting_review']['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),
      ('candidate_members',len(catalog.load(oldroot,'inspected-candidates.csv')),len(candidates)))}
    emit('continuation11-count-deltas.json',dict(counters=deltas,files=filecounts,families=sem['families'],new_operation_groups=sum(o['operation_id'].startswith('C11-') for o in operations),existing_extensions=[o['operation_id'] for o in operations if not o['operation_id'].startswith('C11-')],candidate_freeze='NOT FROZEN; member records are not deduplicated migration algorithms'))
    protection=[]
    for label,records in [('tracked2109',b['tracked']),('GateC1543',b['gate']),('compiled171',catalog.load(oldroot,'files.csv'))]:
        for row in records:
            after=sha(ROOT/row['path']);assert after==row['sha256'],row['path']
            protection.append(dict(protection_set=label,path=row['path'],before_sha256=row['sha256'],after_sha256=after,status='exact bytes preserved',current_bytes=(ROOT/row['path']).stat().st_size))
    for row in b['baseline']['entries']:
        path=row['retained_path'];after=sha(ROOT/path);assert after==row['sha256']
        protection.append(dict(protection_set='original52',path=path,before_sha256=row['sha256'],after_sha256=after,status='exact bytes preserved',current_bytes=(ROOT/path).stat().st_size))
    write('continuation11-protection.csv',catalog.render(protection))
    emit('continuation11-validation.json',validation())
    write('continuation11.md',review_text(b,filecounts,deltas,operations))
    # Complete path manifest includes every historical/current indexed artifact and self-excluded index.
    prefix='docs/architecture/WindowsMigrationLedger/'
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}
    paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}
    paths|={'docs/architecture/WindowsMigrationLedger.md',prefix+'continuation11-artifacts.csv'}
    old={r['path']:r['sha256'] for r in b['original_artifacts']}
    manifest=[dict(path=p,change_from_continuation11_start='unchanged' if p in old and sha(ROOT/p)==old[p] else 'modified' if p in old else 'added',tracking='ignored; future individual force-add' if p.startswith(prefix) else 'visible audit artifact',hash_authority='index.json exact SHA256/bytes; index self excluded') for p in sorted(paths)]
    write('continuation11-artifacts.csv',catalog.render(manifest))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith(prefix) else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    if CHECK:
        errors=logs.verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths']);assert not errors,errors
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=63,exports=30,overall=sem['qualified_source_reviews'],remaining=sem['unreviewed_for_acceptance'],candidate_rows=len(candidates),paths=len(paths))))


def review_text(b,files,deltas,operations):
    table='\n'.join('| '+f['group']+' | '+f['path'].rsplit('/',1)[-1]+' | '+str(f['reviewed'])+'/'+str(f['records'])+' | '+str(f['exports_after'])+' | '+str(f['nonexports'])+' |' for f in files)
    counts='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    return f'''# 04A.01 Continuation 11 — MMRM and LMM worksheet UDF audit

**CONTINUATION 11 COMPLETE — PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE.** Audit only; no product source/tests/fixtures, export annotations, framework/catalog/UI/metadata/installer/ignore changes. This is continuation11 within04A.01, not batch04A.11. Gate C2 not passed; E1–E7 open and GC3–GC8 deferred. No later continuation or04A.02 implementation authorized.

Verified branch `{b['branch']}`, HEAD/main `{b['head']}`. Start index SHA256 `c194bb60ce1c27efd15d3c888a60693c69b8b6357fc58c64f5a8557f0ac90187`; all230 artifacts matched sizes/hashes. Snapshot231 paths including index plus extra ignored ledger/tooling bytes under TestResults/04A-01-continuation-11/original. Prior1199 individual decision objects restored/verified literal-byte-identical after a caught newline-only audit-helper mistake; parsed decisions never changed. Original52 baseline,2109 tracked/1543 Gate C/171 compiled sources and26832 prior02–10 raw evidence files preserved. The owner ZIP is historical evidence; no separate ZIP reopening claimed.

| Group | Module | Qualified | Actual exports | Nonexports |
|---|---|---:|---:|---:|
{table}

All63 exact originally pending IDs now qualify,30 actual parsed ExcelFunction annotations and33 nonexports. Four initialized MMRM profile properties, cache initializers, nested name predicate and LMM N property/getter are independently span-pinned. Uninitialized state declarations receive no method credit; handle-state inventory is separate. A38/B25 immutable snapshots remain raw evidence; explicit new-only contract/source refinements are retained separately. No outside/UI/Graphics member received credit. Global268 registrations and authored catalog unchanged. Common typed mixed input/compiler and result contracts exist; executable Common registry remains ANOVA/Kruskal–Wallis only, no Office mixed-model capability enabled.

| Metric | Before | After |
|---|---:|---:|
{counts}

Overall1262/3866;2604 pending. UDF519/703;184 pending. Ownership counts above are computed from live ledger, not imposed arithmetic. UI1372/Graphics1048 pending,1604 unresolved wiring edges and1/2056 full controls unchanged; zero legacy-detail gaps. remaining-member-review.csv contains exact unfinished IDs; no unfinished11 target IDs. Strict must remain exit2/NOT READY.

MMRM: existing Core MMRM/FitRequest/BlockData/Engine own R-only likelihood and inference. UDF wizard/import/strict options, .05 alpha, formula/raw design, names/intercept, request orchestration, GUID cache and spills remain mixed host/portable policy. Prior C06/C07 importer and option decisions are referenced, not newly reviewed. R labels ID/Diagonal/CS/HCS/AR1/HAR1/TOEP/TOEPH/UN normalize into existing Core factories. REML/KR validation and full workspace are Core; AI is REML-only and engine can fall back to projected BFGS when usable-state conditions fail and fallback flag permits. Trace, optimizer, gradient and convergence evidence are not proof of native recalc safety.

MMRM LS-means/contrasts/custom estimates use observed fitted design profiles. AT only fills unspecified restrictions, visit/design equality uses Core relative1e-9 NearlyEqual, matching rows average equally (no subject/sampling weights; no weights argument at FIT). Labelled signed contrast weights accumulate case-insensitively; exactly zero skipped before matching; missing profile/no nonzero rows returns descriptive text. Core MixedModelPostEstimation computes KR/Satterthwaite/ordinary Lbeta inference and formatted tables; general profile selection is not fully equivalent to constructed reference grids. GUI can use equal/observed class-cell grids, covariates and multiplicity; future intended estimand needs approval.

LMM: independent fixed/random formula designs and names, missing z permitted for intercept-only random design, nonempty Z required, Windows frontend validation and default RI versus VC. Windows MixedModelFrontEndHelpers is not a Core/Common equivalent merely because it lacks Forms. G-side random coefficient covariance differs from visit-indexed R and full marginal V. Core owns fit, G/R user-scale tables and BLUPs; RANEF table sorts subject strings ordinal and names q random-effect columns. Only marginal fitted values and raw marginal residuals are exposed; no conditional/standardized selector/new-subject prediction is invented.

Concrete alignment candidates: GUIs coerce ML to REML for KR while UDFs reject; settings alpha versus .05 UDF default; GUI random-slope authoring can promote RI to correlated RI+S whereas omitted UDF G defaults VC; GUI retains raw.RowIds while UDF drops original coordinates. Core FromArrays defaults1..n RowIndices, but CanUseOriginalRowOrder requires0..n-1: fallback produces sorted block traversal. Consequently source comments promising retained original row order are not accepted as proof. Both GUI data-sheet and UDF sequential outputs need a future value/identity comparison; no fix or choice of canonical convention here. Core methods and source bodies are pinned in continuation11-portable-comparisons.csv.

Negative findings: no duplicated numerical likelihood/optimizer or user-scale covariance algorithm was proven; both hosts call existing Core wrappers. Same alpha/options/estimand still require pinned comparison. Core wrapResults, term/CI/p metadata, covparms internal theta and user-scale G/R remain reused. Host spills, p-display/cell-error formatting, output titles/separators and GUI worksheet writer legitimately differ. MMRM private stack and LMM shared UdfOutputTables stack are separately pinned, not assumed identical.

continuation11-gui-udf-alignment.csv records14 source-backed divergences/unknowns/negative findings with UDF/GUI/Core declarations, body/file pins, owners and fixtures. Proposed shared rules are provisional pending statistical and owner approval. continuation11-candidate-operations.csv deduplicates3 new groups and3 existing extensions (including the C10 observed-profile group extended for MMRM); member rows in continuation11-operation-members.csv are separate. No global extraction count frozen. .40 request/Core reuse, .27/.39 input/identity, .28 output, .43 native cache, .44 actual settings/fault dependencies and .47 concrete GUI controls only. GC8 only actual logging bodies, explicit none elsewhere; gate statuses unchanged.

Both static caches use case-insensitive keys, GUID values and mutable results/workspaces; DROP invalid key false, CLEAR_ALL count then clear non-atomic under concurrency, no workbook scope/leases/disposal shown. Core result wrapping/name restoration also mutates result state. Existing managed extractor/drop tests do not establish workbook lifetime, multiworkbook ownership, concurrency or recalculation. Future .43 native fixtures remain open.

Fresh commands/interpreter/version/exits, distinct test identities and hashed logs/TRX are in continuation11-validation.json. Approved WinPython3.14.7/PyYAML6.0.3 only with -B; no new cache artifacts tracked. Existing offline validation runner preserves/restores generated attributes byte-identically; transient copies retained. Full audit negatives rerun after corrections; strict2 is expected acceptance failure, not a pass. No native Excel/COM/XLL/UI, workbook recalc, authenticated Office, Mac, R scripts or new numerical goldens run. Later owner checks: small Orthodont-like KR MMRM plus visit/contrast profile; unbalanced sleepstudy LMM slope G/R/RANEF and row-aligned marginal residuals; stale/case-mixed handles and DROP/CLEAR_ALL; GUI/UDF alpha and order. No staging/commit/merge/push/reset/clean or next workstream.
'''

if __name__=='__main__':
    parser=argparse.ArgumentParser(__doc__);parser.add_argument('--check',action='store_true');parser.add_argument('--guard',action='store_true');args=parser.parse_args();CHECK=args.check
    if args.guard:print(json.dumps(preservation()))
    else:main()
