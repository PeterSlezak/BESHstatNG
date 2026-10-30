"""Regression UDF export checkpoint10; historical reports stay immutable."""
from __future__ import annotations
import argparse,collections,hashlib,json,re
import catalog,semantic,continuation03_report as logs,continuation07_report as prior
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-10'
BASE='BESHstatNG/src/ExcelUDFs/Regression/'
TARGETS={BASE+'GeeUDFs.vb':('A',39),BASE+'OrdinalLogitUDFs.vb':('B',11),BASE+'MultinomialLogitUDFs.vb':('C',13)}
EXPECTED_EXPORTS=dict(zip(TARGETS,(13,7,7)))
REG_FIELDS=('export_id','path','attribute_line','attribute_end_line','attribute','effective_name','symbol_id','implementing_name','signature','start_line','end_line')
FIELDS=prior.FIELDS+('registration_evidence',);CHECK=False
def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Checkpoint10 drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def historical_errors(root,records):
    errors=prior.historical_errors(root,records)
    for a in records:
        if a['path'].rsplit('/',1)[-1].startswith(('continuation07','continuation08','continuation09')):
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
    for label,records,base in [('protected',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('snapshot09',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/a['path']).is_file() and sha(base/a['path'])==a['sha256'] for a in records),label+' mismatch'
    errors=historical_errors(ROOT,b['original_artifacts']);assert not errors,errors
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    files=catalog.load(OUT,'files.csv');assert len(files)==171 and all(sha(ROOT/f['path'])==f['sha256'] for f in files)
    raw=read(EVIDENCE/'prior-raw-evidence-hashes.json')['files'];errors=raw_history_errors(ROOT,raw);assert not errors,errors
    return dict(branch=b['branch'],head=b['head'],tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(files),snapshot=len(b['original_artifacts']),baseline=len(b['baseline']['entries']),prior_raw_files=len(raw),historical='02/03/04/05/06/07/08 reports, snapshots and all recorded raw files unchanged')
def scope_errors(symbols,decisions,queue):
    source={s['symbol_id']:s for s in symbols};pins={d.get('symbol_id'):d for d in decisions};errors=[]
    exports=catalog.load(OUT,'udf-exports.csv')
    reviewed=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-10']
    if len(reviewed)!=63 or len({d['symbol_id'] for d in reviewed})!=63:errors.append('checkpoint10 missing/duplicate or out-of-scope decision count')
    for d in reviewed:
        if json.loads(d.get('source_identity','{}')).get('path') not in TARGETS:errors.append('checkpoint10 outside scope ID: '+d['symbol_id'])
    for p,(group,count) in TARGETS.items():
        ids={s['symbol_id'] for s in queue if s['path']==p}
        if len(ids)!=count or sum(s['path']==p for s in symbols)!=count:errors.append('checkpoint10 source/queue denominator: '+p)
        for ident in ids:
            s=source.get(ident);d=pins.get(ident)
            if s is None or d is None or not semantic.qualified(s) or not semantic.qualified(d):errors.append('checkpoint10 missing/unqualified ID: '+ident);continue
            if d.get('checkpoint_id')!='04A.01-continuation-10' or d.get('review_group')!=group:errors.append('checkpoint10 incorrect group: '+ident)
            if d.get('body_sha256')!=s['body_sha256'] or d.get('signature_key')!=s['signature_key']:errors.append('checkpoint10 top-level source pin drift: '+ident)
            if d.get('source_kind')!=s['kind'] or d.get('source_enclosing_symbol_id')!=s.get('enclosing_symbol_id',''):errors.append('checkpoint10 kind/enclosing drift: '+ident)
            recorded=json.loads(d.get('registration_evidence','[]'))
            project=lambda rows:[{k:r.get(k,'') for k in REG_FIELDS} for r in rows]
            if project(recorded)!=project([e for e in exports if e['symbol_id']==ident]):errors.append('checkpoint10 member annotation evidence drift: '+ident)
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'checkpoint10')
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
    errors=[] if export_rows(selected,exports)==rows else ['checkpoint10 export/helper mapping drift']
    if sum(e['symbol_id'] in {s['symbol_id'] for s in selected} for e in exports)!=27:errors.append('checkpoint10 requires actual27 exported target IDs')
    if len(rows)!=63 or len({r['symbol_id'] for r in rows})!=63:errors.append('checkpoint10 export/helper member omission or duplicate')
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
        v['status']='NEW PROVISIONAL GROUP' if v['operation_id'].startswith('C10-') else 'EXTENSION OF EXISTING GROUP; NOT A NEW OPERATION'
        members=[s for s in selected if s.get('candidate_operation_id')==v['operation_id'] and s['disposition'].startswith('extract-to-')]
        v['owner']='; '.join(sorted({s.get('owner_role','') for s in members}))
        v['freeze_status']='NOT FROZEN; member rows and deduplicated operations are distinct'
    return values
def operation_errors(selected,operations):
    errors=[] if operation_rows(selected)==operations else ['checkpoint10 operation mapping drift']
    existing={r['operation_id'] for n in ('03','04','05','06','07','08','09') for r in catalog.load(OUT,'continuation'+n+'-candidate-operations.csv')}
    for o in operations:
        if not o['operation_id'].startswith('C10-') and o['operation_id'] not in existing:errors.append('Unknown claimed prior operation: '+o['operation_id'])
        if not o.get('owner') or not o['operation_id']:errors.append('Candidate operation lacks declared owner or identity')
    for s in selected:
        if s['disposition'].startswith('extract-to-') and (not s.get('owner_role') or not s.get('candidate_operation_id') or not s.get('future_batch')):errors.append('Candidate member lacks declared operation owner')
    return errors

prior_decision_errors=__import__("continuation08_report").prior_decision_errors

EXTRA=('prior_review_dependencies','member_identity_key','registration_class','portability_classification','future_crosscutting_batches','review_contract','review_status','evidence_status','confidence','source_sha256')
FIELDS=FIELDS+EXTRA

def alignment_errors(rows):
    errors=[]
    source={s['symbol_id']:s for s in catalog.load(OUT,'symbols.csv')}
    portable=semantic.portable_sources()
    ids=[]
    for row in rows:
        ids.append(row.get('alignment_id'))
        if row.get('udf_id') not in source or source[row['udf_id']]['path'] not in TARGETS:errors.append('alignment UDF outside scope')
        if not row.get('owner') or not row.get('future_fixture') or row.get('runtime_status','').startswith('PASS'):errors.append('alignment missing owner/fixture or false runtime pass')
        try:
            for recorded,authority in [(json.loads(row['udf_pin']),source),(json.loads(row['core_pin']),portable)]+[(p,source) for p in json.loads(row['gui_pins'])]:
                actual=authority.get(recorded['symbol_id'])
                if actual is None or any(actual.get(k)!=v for k,v in recorded.items()):errors.append('alignment stale declaration pin')
            for path,digest in json.loads(row['dependency_file_pins']).items():
                if sha(ROOT/path)!=digest:errors.append('alignment stale dependency')
        except (KeyError,ValueError,TypeError):errors.append('alignment invalid pin')
    if len(ids)!=12 or len(set(ids))!=12:errors.append('alignment missing/duplicate candidate')
    return errors

def acceptance_errors(summary,index):
    open_work=summary.get('unreviewed_for_acceptance',0)>0 or summary.get('qualified_full_control_contracts',0)<summary.get('control_review_denominator',0)
    if open_work and (summary.get('status')!='NOT READY FOR ACCEPTANCE' or index.get('acceptance_status')!='NOT READY FOR ACCEPTANCE'):
        return ['Misleading READY status while member/control obligations remain']
    return []

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
    runtime=read(OUT/'continuation10-runtime.json')
    errors=runtime_errors(runtime,runs+checks);assert not errors,errors
    first=read(EVIDENCE/'first-checks.json') if (EVIDENCE/'first-checks.json').exists() else []
    runtime['initial_failed_checks']=first
    runtime['audit_fixture_correction']='Initial104 tests:103pass/1failure; group-counter fixture filtered to exports. Corrected to all39/11/13 source members, then entire104-test suite rerun. No source decisions, product tests or golden expectations weakened.'
    return dict(scope='Fresh continuation10, approved WinPython3.14 only; source assertions are not native parity',runs=runs,checks=checks,initial_commands=read(EVIDENCE/'initial-commands.json'),managed=managed,python_node=other,negative=negative,transient_generated_copies=generated,protection=read(EVIDENCE/'protection.json') if (EVIDENCE/'protection.json').exists() else 'Pending final guard',runtime=runtime,not_run=['native Excel/COM/XLL/workbook golden capture','authenticated Office/Mac','installer/signing/deployment/network installation','04A.02 or later extraction'],development_notes=['A39/B11/C13 immutable source-review checkpoints; original1136 decision object bytes retained.','All fresh Python commands use approved3.14.7 executable with -B; historical command strings/defaults remain historical and were not executed.','Initial audit-only alignment.py authoring invocation exited1 for an unmatched parenthesis before writing files; corrected invocation exited0 and all final reconciliation fixtures ran after correction. No product test or golden expectation changed.','No product code, golden expectations or gate status changed.'])

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
    assert len(json.loads(oldtext))==1136
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
    align=catalog.load(OUT,'continuation10-gui-udf-alignment.csv');assert not alignment_errors(align)
    reviewed_alignment=read(EVIDENCE/'alignment-reviewed.json')
    assert align==reviewed_alignment,'Reviewed alignment text/pins changed'
    rich=[{**s,**current[s['symbol_id']]} for s in selected]
    write('continuation10-member-reviews.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in rich]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:({**s,**current.get(s['symbol_id'],{})}).get(k,'') for k in FIELDS} for s in candidates]))
    write('continuation10-export-helper-map.csv',catalog.render(mapped))
    filecounts=[dict(path=p,group=g,records=n,reviewed=sum(semantic.qualified(s) for s in selected if s['path']==p),exports_before=sum(e['path']==p for e in old_exports),exports_after=sum(e['path']==p for e in exports),nonexports=sum(r['path']==p and r['excel_function_export']=='no' for r in mapped)) for p,(g,n) in TARGETS.items()]
    write('continuation10-export-counts.csv',catalog.render(filecounts+[dict(path='ALL WINDOWS EXPORTS',group='global',records=3866,reviewed=sem['qualified_source_reviews'],exports_before=268,exports_after=268,nonexports='outside scope')]))
    write('continuation10-candidate-operations.csv',catalog.render(operations))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=x,allocation_evidence=e,records=n) for (f,r,d,x,e),n in sorted(reasons.items())]))
    memberops=[dict(symbol_id=s['symbol_id'],operation_id=s.get('candidate_operation_id',''),owner=s['owner_role'],destination=s['destination'],future_batch=s['future_batch'],disposition=s['disposition']) for s in selected]
    write('continuation10-operation-members.csv',catalog.render(memberops))
    state=[]
    for row in catalog.load(OUT,'state.csv'):
        if row['path'] not in TARGETS:continue
        members=[s for s in selected if s['path']==row['path']]
        state.append(dict(**row,source_sha256=sha(ROOT/row['path']),initializer_review_ids='; '.join(s['symbol_id'] for s in members if s['kind']=='VariableDeclarator'),assignment_review_ids='; '.join(s['symbol_id'] for s in members if s['name'].endswith('_FIT')),classification='Windows session handle container; model numeric state remains existing Core. Uninitialized property has no additional executable census record.',future_owner='Excel-DNA lifecycle maintainer / regression request maintainer',future_batch='04A.43; 04A.40',runtime_status='Lifetime/recalculation/concurrency NOT ESTABLISHED; see named member fixtures'))
    write('continuation10-handle-state.csv',catalog.render(state))
    for group in ('A','B','C'):
        snap=read(EVIDENCE/('group-'+group+'-member-evidence.json'))
        snap['final_qualified_ids']=[s['symbol_id'] for s in selected if current[s['symbol_id']]['review_group']==group]
        snap['refinement_evidence']='None required; registration/dependency references captured before immutable group save'
        emit('continuation10-group-'+group+'.json',snap)
    emit('continuation10-start.json',{k:b[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    deltas={key:dict(before=before,after=after,delta=after-before) for key,before,after in (
      ('qualified_member_reviews',b['starting_review']['qualified_source_reviews'],sem['qualified_source_reviews']),
      ('remaining_member_reviews',b['starting_review']['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),
      ('ownership_investigations',b['starting_coverage']['member_investigations'],coverage['member_investigations']),
      ('unresolved_edges',b['starting_coverage']['unresolved_edge_rows'],coverage['unresolved_edge_rows']),
      ('full_controls',b['starting_review']['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),
      ('candidate_members',len(catalog.load(oldroot,'inspected-candidates.csv')),len(candidates)))}
    emit('continuation10-count-deltas.json',dict(counters=deltas,files=filecounts,families=sem['families'],new_operation_groups=sum(o['operation_id'].startswith('C10-') for o in operations),existing_extensions=[o['operation_id'] for o in operations if not o['operation_id'].startswith('C10-')],candidate_freeze='NOT FROZEN; member records are not deduplicated migration algorithms'))
    emit('continuation10-validation.json',validation())
    write('continuation10.md',review_text(b,filecounts,deltas,operations))
    # Complete path manifest includes every historical/current indexed artifact and self-excluded index.
    prefix='docs/architecture/WindowsMigrationLedger/'
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}
    paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}
    paths|={'docs/architecture/WindowsMigrationLedger.md',prefix+'continuation10-artifacts.csv'}
    old={r['path']:r['sha256'] for r in b['original_artifacts']}
    manifest=[dict(path=p,change_from_continuation10_start='unchanged' if p in old and sha(ROOT/p)==old[p] else 'modified' if p in old else 'added',tracking='ignored; future individual force-add' if p.startswith(prefix) else 'visible audit artifact',hash_authority='index.json exact SHA256/bytes; index self excluded') for p in sorted(paths)]
    write('continuation10-artifacts.csv',catalog.render(manifest))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith(prefix) else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    if CHECK:
        errors=logs.verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths']);assert not errors,errors
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=63,exports=27,overall=sem['qualified_source_reviews'],remaining=sem['unreviewed_for_acceptance'],candidate_rows=len(candidates),paths=len(paths))))

def review_text(b,files,deltas,operations):
    table='\n'.join('| '+f['group']+' | '+f['path'].rsplit('/',1)[-1]+' | '+str(f['reviewed'])+'/'+str(f['records'])+' | '+str(f['exports_after'])+' | '+str(f['nonexports'])+' |' for f in files)
    counts='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    return f'''# 04A.01 Continuation 10 — GEE and categorical regression UDFs

**CONTINUATION 10 COMPLETE — PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE.** Audit only:63 exact source-qualified records, no product changes, extraction, scientific-golden capture, native execution or later batch implementation. This is continuation10 within04A.01, not batch04A.10. Gate C2 not passed; E1–E7 remain open and GC3–GC8 deferred;40/223 accepted unchanged.

Live branch `{b['branch']}`, HEAD/main `{b['head']}`. Start index SHA2561a78bbe4374c03e0567904612ff20cfb64c0326cd5dc100ae441c937d304e8f1 and all211 indexed artifact sizes/hashes matched;212 paths including index preserved under TestResults/04A-01-continuation-10/original. All1136 earlier decision entries retain literal object bytes. Extra ledger/tooling copies,52 baseline paths,2109 tracked/1543 Gate C/171 compiled-source hashes and23727 prior02–09 raw evidence hashes preserved. No independent ZIP reopening claimed. Incidental preexisting ignored bytecode is retained and excluded from proposed tracking; fresh Python uses -B and approved WinPython3.14.7/PyYAML6.0.3 only.

| Group | Source | Qualified | Actual exports | Nonexports |
|---|---|---:|---:|---:|
{table}

Immutable A39/B11/C13 rich snapshots contain source identities/profile/spans/body and dependency/portable declaration pins; four GEE initialized auto-property records and its nested normalization lambda are individually qualified. Uninitialized handle properties are derived separately in continuation10-handle-state.csv, without invented executable review credit. No excluded source member qualified by dependency reads. All27 export names/attribute ranges/signatures bind actual parsed ExcelFunction annotations,36 nonexports explicit; global268 registrations unchanged. Public helper visibility is not registration. Core and Common capability registration remain distinct: ANOVA/KW runnable Common handlers only; no GEE/ordinal/multinomial Office execution claimed.

| Metric | Before | After |
|---|---:|---:|
{counts}

Overall1199/3866,2667 pending; UDF456/703 and247 pending. UI1372/Graphics1048 pending unchanged;1604 wiring/intent edges and1/2056 complete control contracts unchanged; zero legacy-detail gaps. Remaining exact IDs are in remaining-member-review.csv; no unfinished10 target IDs. All other families remain outside this continuation. Strict must remain exit2/NOT READY.

## A — GEE

GEE_FIT forwards model/data/settings to existing Core GEE and GEEcovStruct; formula/argument/error/result handling is partial helper reuse, not whole-wrapper equivalence. geeData sorts cluster/time or cluster/source RowId; Core builds time-index covariance structures. Independence, Exchangable, Autoregressive, Toeplitz and Unstructured are actual supported factory names. Robust/Naive/Bias Reduced inference uses existing Core covariance; VCOV can compute bias covariance lazily and returns clone. Summary remains fit-selected SE when another VCOV type is requested. Residual requests recalculate mutable arrays using Core default weighting/scale; scaled residual labels are not proof of leverage-standardized diagnostics. Binary metrics already forward through Windows facade to BinaryClassificationCore; parser/default/spill/sentinel policy remains separate and extends existing C09 operations.

LSMESTIMATE is an observed fitted-design profile/contrast policy, not a proven reference-grid LS-means replacement. Finite spec weights, optional AT long/wide syntax and normalized fitted-column names resolve observed matches; explicit component constraints override AT. Every matched design row is averaged equally, independently of fit weights or cluster weighting. Contrast weights accumulate case-insensitive labels; zero contrast rows skip matching. No match returns descriptive text. Fitted design lacks offsets, while model means and PRED use offsets. Link-scale Lbeta/QuadraticForm reuse existing Core primitives; response transform applies inverse after contrast combination, delta SE and endpoint transforms. Tiny negative variance only in(-1e-10,0) clamps0; decreasing inverse links can reverse CI endpoints. No verified Core/GUI GEE facility owns this entire policy. Named ordinary/unobserved/zero-weight/time/near-negative-variance/extreme-link/robust-vs-naive fixtures remain future work. No native equivalence claim follows from Lbeta arithmetic.

Supplementary continuation10-ls-comparisons.csv pins existing MixedModelPostEstimation, MMRMPostEstimation and reference-grid services: MixedModelResult, denominator-DF/Kenward-Roger and grid construction do not replace GEE asymptotic/link/profile policy wholesale. continuation10-covariance-comparisons.csv pins20 actual/inherited Core covariance method bodies, including positional AR1 and time-indexed Toeplitz/unstructured paths. These dependency comparisons confer no additional Windows member review credit.

## B — ordinal logistic

Core OrdinalLogitModel owns likelihood, increasing thresholds, weighted residuals and category orientation. Last reference uses ascending observed integers; First reverses ordinal ordering and cutpoint interpretation. UDF requires raw and expanded predictor although Core permits threshold-only fitting. Summary defaults.05 independently of cached fit alpha; GUI wraps stored Core alpha. New prediction reconstructs stored design and required offset, computes slope eta and local cumulative probabilities. Its finite arithmetic matches inspected Core private PredictRowProbs but is not a public whole-equivalent API. Invalid cut ordering/nonfinite helper inputs can be masked or propagate; thresholds are validated by Core fitting, not local helper. ArgMax ties choose first internal index, not necessarily smallest original label. Residual all output5k+3columns, category order from Core payload. Ordinary lifecycle tests already exist; they are managed tests, not workbook recalculation proof.

## C — multinomial logistic

Core MultinomialLogitModel owns baseline-category fit, category/parameter naming, likelihood, weighted diagnostics and classification. First reference rotates smallest category to last internally; equation blocks have predictor+optional intercept width and countK-1. Worksheet eta adds offset to every nonbaseline equation. Shared Core LogSumExpBaselineZero gives stable finite softmax; final exponentials are repeated locally with no clipping/renormalization or nonfinite guard. Core fitted-row arithmetic is comparison-pinned, not whole worksheet reuse. Classification is reordered to ascending original categories, predictions retain baseline-last order, and residual payload Categories versus numerical column order requires pinned First-reference comparison. All residuals6k+3columns; raw arrays may contain nonfinite values or bounds failures. Summary omitted alpha again.05; DROP uniquely returnsFalse on blank/caught error, unlike other target DROP Value. These are baseline facts for later evaluation, not immutable correct specifications.

continuation10-gui-udf-alignment.csv contains12 individually pinned future candidates distinguishing statistical/numerical, host/output and unknown semantics. Every row binds actual UDF/GUI/Core declarations plus file hashes, owner, existing future batch and concrete normal/edge fixture. The intended direction is one Core calculation authority for GUI and UDF; proposed policies are provisional pending scientific/owner review. No permanent divergence or automatic GUI/UDF correctness is asserted.

Candidate member rows are separate from deduplicated provisional operations; existing C09 binary/spill groups extend rather than duplicate. No frozen global migration count. .40 owns regression request/presentation; .39 references prior argument/import reviews; .28 shared result semantics; .38 registration; .43 native cache/recalculation; .44 actual logging only. GC8 appears only for actual fault calls; no other blanket gate attribution. Source review responsibility/risk and named fixtures are per member in continuation10-member-reviews.csv.

Fresh exact commands/interpreter/version/exits, test identities and log/TRX hashes are in continuation10-validation.json; source, index and proposed-tracking guards are reproducible. Generated version copies are retained before restoring exact protected originals. Automated Python/.NET/Node tests and WASM builds do not establish native Excel/COM/XLL, workbook lifetime, concurrency, authenticated Office, Mac, GUI option parity or new scientific goldens. Those remain NOT RUN. Future manual checks: shuffled/time-ordered GEE with robust/naive LS profiles and DROP; First-reference ordinal/multinomial fit/prediction/residual labels with offsets and omitted summaryalpha. No product framework/catalog/UI/metadata/installer/ignore/signing changes, staging/commit/merge/push/reset/clean/network install or next continuation.
'''

if __name__=='__main__':
    parser=argparse.ArgumentParser(__doc__);parser.add_argument('--check',action='store_true');parser.add_argument('--guard',action='store_true');args=parser.parse_args();CHECK=args.check
    if args.guard:print(json.dumps(preservation()))
    else:main()

