"""Regression UDF export checkpoint09; historical reports stay immutable."""
from __future__ import annotations
import argparse,collections,hashlib,json,re
import catalog,semantic,continuation03_report as logs,continuation07_report as prior
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-09'
BASE='BESHstatNG/src/ExcelUDFs/Regression/'
TARGETS={BASE+p:(g,n) for p,g,n in (
 ('RegressionFormulaUDFs.vb','A',12),('UdfRegressionOutput.vb','A',6),('LinearModelUDFs.vb','A',14),
 ('GlmUDFs.vb','B',15),('GlmNbUDFs.vb','B',8),('ZipUDFs.vb','B',16),('CoxUDFs.vb','C',13))}
EXPECTED_EXPORTS=dict(zip(TARGETS,(1,0,8,10,6,6,7)))
REG_FIELDS=('export_id','path','attribute_line','attribute_end_line','attribute','effective_name','symbol_id','implementing_name','signature','start_line','end_line')
FIELDS=prior.FIELDS+('registration_evidence',);CHECK=False
def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Checkpoint09 drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def historical_errors(root,records):
    errors=prior.historical_errors(root,records)
    for a in records:
        if a['path'].rsplit('/',1)[-1].startswith(('continuation07','continuation08')):
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
    reviewed=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-09']
    if len(reviewed)!=84 or len({d['symbol_id'] for d in reviewed})!=84:errors.append('checkpoint09 missing/duplicate or out-of-scope decision count')
    for d in reviewed:
        if json.loads(d.get('source_identity','{}')).get('path') not in TARGETS:errors.append('checkpoint09 outside scope ID: '+d['symbol_id'])
    for p,(group,count) in TARGETS.items():
        ids={s['symbol_id'] for s in queue if s['path']==p}
        if len(ids)!=count or sum(s['path']==p for s in symbols)!=count:errors.append('checkpoint09 source/queue denominator: '+p)
        for ident in ids:
            s=source.get(ident);d=pins.get(ident)
            if s is None or d is None or not semantic.qualified(s) or not semantic.qualified(d):errors.append('checkpoint09 missing/unqualified ID: '+ident);continue
            if d.get('checkpoint_id')!='04A.01-continuation-09' or d.get('review_group')!=group:errors.append('checkpoint09 incorrect group: '+ident)
            if d.get('body_sha256')!=s['body_sha256'] or d.get('signature_key')!=s['signature_key']:errors.append('checkpoint09 top-level source pin drift: '+ident)
            if d.get('source_kind')!=s['kind'] or d.get('source_enclosing_symbol_id')!=s.get('enclosing_symbol_id',''):errors.append('checkpoint09 kind/enclosing drift: '+ident)
            recorded=json.loads(d.get('registration_evidence','[]'))
            project=lambda rows:[{k:r.get(k,'') for k in REG_FIELDS} for r in rows]
            if project(recorded)!=project([e for e in exports if e['symbol_id']==ident]):errors.append('checkpoint09 member annotation evidence drift: '+ident)
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'checkpoint09')
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
    errors=[] if export_rows(selected,exports)==rows else ['checkpoint09 export/helper mapping drift']
    if sum(e['symbol_id'] in {s['symbol_id'] for s in selected} for e in exports)!=38:errors.append('checkpoint09 requires actual38 exported target IDs')
    if len(rows)!=84 or len({r['symbol_id'] for r in rows})!=84:errors.append('checkpoint09 export/helper member omission or duplicate')
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
        v['status']='NEW PROVISIONAL GROUP' if v['operation_id'].startswith('C09-') else 'EXTENSION OF EXISTING GROUP; NOT A NEW OPERATION'
        members=[s for s in selected if s.get('candidate_operation_id')==v['operation_id'] and s['disposition'].startswith('extract-to-')]
        v['owner']='; '.join(sorted({s.get('owner_role','') for s in members}))
        v['freeze_status']='NOT FROZEN; member rows and deduplicated operations are distinct'
    return values
def operation_errors(selected,operations):
    errors=[] if operation_rows(selected)==operations else ['checkpoint09 operation mapping drift']
    existing={r['operation_id'] for n in ('03','04','05','06','07','08') for r in catalog.load(OUT,'continuation'+n+'-candidate-operations.csv')}
    for o in operations:
        if not o['operation_id'].startswith('C09-') and o['operation_id'] not in existing:errors.append('Unknown claimed prior operation: '+o['operation_id'])
        if not o.get('owner') or not o['operation_id']:errors.append('Candidate operation lacks declared owner or identity')
    for s in selected:
        if s['disposition'].startswith('extract-to-') and (not s.get('owner_role') or not s.get('candidate_operation_id')):errors.append('Candidate member lacks declared operation owner')
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
    if len(ids)!=16 or len(set(ids))!=16:errors.append('alignment missing/duplicate candidate')
    return errors

def acceptance_errors(summary,index):
    open_work=summary.get('unreviewed_for_acceptance',0)>0 or summary.get('qualified_full_control_contracts',0)<summary.get('control_review_denominator',0)
    if open_work and (summary.get('status')!='NOT READY FOR ACCEPTANCE' or index.get('acceptance_status')!='NOT READY FOR ACCEPTANCE'):
        return ['Misleading READY status while member/control obligations remain']
    return []

def numeric_interface_errors(rows):
    errors=[];source=semantic.portable_sources()
    if len(rows)!=10 or len({r.get('symbol_id') for r in rows})!=10:errors.append('numeric comparison missing/duplicate declaration')
    for r in rows:
        actual=source.get(r.get('symbol_id'))
        if actual is None or any(r.get(k)!=actual.get(k) for k in ('path','signature','start_line','end_line','body_sha256','containing_type')):
            errors.append('numeric comparison stale declaration');continue
        data=(ROOT/r['path']).read_bytes().decode('utf-8-sig')
        if sha(ROOT/r['path'])!=r.get('source_sha256') or data[int(actual['span_start']):int(actual['span_start'])+int(actual['span_length'])]!=r.get('body_excerpt'):
            errors.append('numeric comparison stale body/file')
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
    first=EVIDENCE/'negative-reconciliation-first-pass.log'
    first_run=dict(log=first.relative_to(ROOT).as_posix(),sha256=sha(first),outcome='Initial89 tests:88passed/1failure; alignment fixture ran with old14-row expectation while final16-row comparisons were being added. Full final suite rerun after matching the reviewed16-row scope; no product test changed.') if first.exists() else None
    return dict(scope='Fresh09 offline commands only; static audits and automated tests do not establish native parity',runs=runs,checks=checks,initial_check_attempt=read(EVIDENCE/'checks-first-pass.json') if (EVIDENCE/'checks-first-pass.json').exists() else [],managed=managed,python_node=other,negative=negative,initial_negative_run=first_run,transient_generated_copies=generated,protection=read(EVIDENCE/'protection.json') if (EVIDENCE/'protection.json').exists() else 'Pending final guard',not_run=['native Excel/COM/XLL or workbook golden capture','authenticated Office or Mac','installer/signing/deployment/network installs','04A.02 or later extraction implementation'],development_notes=['Initial read-only Git calls rejected ownership; invocation-only safe.directory used, no global config change.','Initial command-string quoting for new snapshot support failed before writes; a Python source file was then used.','Snapshot reused08 capture code; its initial stdout retained historical counts. before.json independently records195 paths/1052 reviews; those real fields are authoritative.','A/B/C rich snapshots are immutable; new-only dependency/registration refinement has explicit before/after evidence. No older decisions normalized.','A renderer-maintenance command read UTF8 with Windows default codepage; its UnicodeDecodeError did not write that file. Original renderer/README UTF8 bytes were recovered from09 start,09 links appended with explicit UTF8, and final reports regenerated.'])

def main():
    b=read(EVIDENCE/'before.json'); oldroot=EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
    symbols=catalog.load(OUT,'symbols.csv');decisions=semantic.read_store('review-decisions.json')
    queue=[s for s in catalog.load(oldroot,'remaining-member-review.csv') if s['path'] in TARGETS]
    errors=scope_errors(symbols,decisions,queue);assert not errors,errors
    oldtext=(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_text(encoding='utf-8-sig')
    assert not prior_decision_errors((semantic.TOOLING/'review-decisions.json').read_text(encoding='utf-8-sig'),oldtext)
    assert len(json.loads(oldtext))==1052
    ids={s['symbol_id'] for s in queue};selected=sorted((s for s in symbols if s['symbol_id'] in ids),key=lambda s:(s['path'],int(s['span_start'])))
    assert len(selected)==84
    current={d['symbol_id']:d for d in decisions}
    assert not semantic.verify(symbols,catalog.load(OUT,'ui-wiring.csv'),catalog.load(OUT,'ui-controls.csv'))
    sem=read(OUT/'semantic-review-coverage.json');coverage=read(OUT/'review-coverage.json')
    assert not acceptance_errors(sem,read(OUT/'index.json'))
    exports=catalog.load(OUT,'udf-exports.csv');old_exports=catalog.load(oldroot,'udf-exports.csv')
    assert not registration_errors(exports,old_exports)
    mapped=export_rows(selected,exports);assert not export_errors(selected,exports,mapped)
    operations=operation_rows(selected);assert not operation_errors(selected,operations)
    align=catalog.load(OUT,'continuation09-gui-udf-alignment.csv');assert not alignment_errors(align)
    assert not numeric_interface_errors(catalog.load(OUT,'continuation09-numeric-interfaces.csv'))
    reviewed_alignment=read(EVIDENCE/'alignment-reviewed.json')
    assert align==reviewed_alignment,'Reviewed alignment text/pins changed'
    rich=[{**s,**current[s['symbol_id']]} for s in selected]
    write('continuation09-member-reviews.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in rich]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:({**s,**current.get(s['symbol_id'],{})}).get(k,'') for k in FIELDS} for s in candidates]))
    write('continuation09-export-helper-map.csv',catalog.render(mapped))
    filecounts=[dict(path=p,group=g,records=n,reviewed=sum(semantic.qualified(s) for s in selected if s['path']==p),exports_before=sum(e['path']==p for e in old_exports),exports_after=sum(e['path']==p for e in exports),nonexports=sum(r['path']==p and r['excel_function_export']=='no' for r in mapped)) for p,(g,n) in TARGETS.items()]
    write('continuation09-export-counts.csv',catalog.render(filecounts+[dict(path='ALL WINDOWS EXPORTS',group='global',records=3866,reviewed=sem['qualified_source_reviews'],exports_before=268,exports_after=268,nonexports='outside scope')]))
    write('continuation09-candidate-operations.csv',catalog.render(operations))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=x,allocation_evidence=e,records=n) for (f,r,d,x,e),n in sorted(reasons.items())]))
    memberops=[dict(symbol_id=s['symbol_id'],operation_id=s.get('candidate_operation_id',''),owner=s['owner_role'],destination=s['destination'],future_batch=s['future_batch'],disposition=s['disposition']) for s in selected]
    write('continuation09-operation-members.csv',catalog.render(memberops))
    state=[]
    for row in catalog.load(OUT,'state.csv'):
        if row['path'] not in TARGETS:continue
        members=[s for s in selected if s['path']==row['path']]
        state.append(dict(**row,source_sha256=sha(ROOT/row['path']),initializer_review_ids='; '.join(s['symbol_id'] for s in members if s['kind']=='VariableDeclarator'),assignment_review_ids='; '.join(s['symbol_id'] for s in members if s['name'].endswith('_FIT')),classification='Windows session handle container; model numeric state remains existing Core. Uninitialized property has no additional executable census record.',future_owner='Excel-DNA lifecycle maintainer / regression request maintainer',future_batch='04A.43; 04A.40',runtime_status='Lifetime/recalculation/concurrency NOT ESTABLISHED; see named member fixtures'))
    write('continuation09-handle-state.csv',catalog.render(state))
    for group in ('A','B','C'):
        snap=read(EVIDENCE/('group-'+group+'-member-evidence.json'))
        snap['final_qualified_ids']=[s['symbol_id'] for s in selected if current[s['symbol_id']]['review_group']==group]
        snap['refinement_evidence']='TestResults/04A-01-continuation-09/dependency-registration-refinement.json; original group decisions retained'
        emit('continuation09-group-'+group+'.json',snap)
    emit('continuation09-start.json',{k:b[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    deltas={key:dict(before=before,after=after,delta=after-before) for key,before,after in (
      ('qualified_member_reviews',b['starting_review']['qualified_source_reviews'],sem['qualified_source_reviews']),
      ('remaining_member_reviews',b['starting_review']['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),
      ('ownership_investigations',b['starting_coverage']['member_investigations'],coverage['member_investigations']),
      ('unresolved_edges',b['starting_coverage']['unresolved_edge_rows'],coverage['unresolved_edge_rows']),
      ('full_controls',b['starting_review']['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),
      ('candidate_members',len(catalog.load(oldroot,'inspected-candidates.csv')),len(candidates)))}
    emit('continuation09-count-deltas.json',dict(counters=deltas,files=filecounts,families=sem['families'],new_operation_groups=sum(o['operation_id'].startswith('C09-') for o in operations),existing_extensions=[o['operation_id'] for o in operations if not o['operation_id'].startswith('C09-')],candidate_freeze='NOT FROZEN; member records are not deduplicated migration algorithms'))
    emit('continuation09-validation.json',validation())
    write('continuation09.md',review_text(b,filecounts,deltas,operations))
    # Complete path manifest includes every historical/current indexed artifact and self-excluded index.
    prefix='docs/architecture/WindowsMigrationLedger/'
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}
    paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}
    paths|={'docs/architecture/WindowsMigrationLedger.md',prefix+'continuation09-artifacts.csv'}
    old={r['path']:r['sha256'] for r in b['original_artifacts']}
    manifest=[dict(path=p,change_from_continuation09_start='unchanged' if p in old and sha(ROOT/p)==old[p] else 'modified' if p in old else 'added',tracking='ignored; future individual force-add' if p.startswith(prefix) else 'visible audit artifact',hash_authority='index.json exact SHA256/bytes; index self excluded') for p in sorted(paths)]
    write('continuation09-artifacts.csv',catalog.render(manifest))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith(prefix) else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    if CHECK:
        errors=logs.verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths']);assert not errors,errors
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=84,exports=38,overall=sem['qualified_source_reviews'],remaining=sem['unreviewed_for_acceptance'],candidate_rows=len(candidates),paths=len(paths))))

def review_text(b,files,deltas,operations):
    table='\n'.join('| '+f['group']+' | '+f['path'].rsplit('/',1)[-1]+' | '+str(f['reviewed'])+'/'+str(f['records'])+' | '+str(f['exports_after'])+' | '+str(f['nonexports'])+' |' for f in files)
    counters='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    return f'''# 04A.01 Continuation09 â€” regression worksheet boundaries

**CONTINUATION 09 COMPLETE â€” PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE.** Exactly84 individually source-qualified records; no extraction, product change, new method, later batch execution, numerical-golden capture or native Excel run. Gate C2 not passed. E1â€“E7/GC3â€“GC8 and40/223 accepted batches unchanged.

Verified branch `{b['branch']}`, HEAD/main `{b['head']}`. Live start index SHA256 dc7385941f76ff91291fdf6d844884eec05cce63591f4e90b4d4a1f81e3a1ec1;194 indexed artifacts plus index preserved as195 exact files, all194 hashes/byte sizes matched. All ledger/tooling extras copied separately. Protected2109 tracked,1543 Gate C,171 compiled-source hashes and52 baseline paths verified. Previous02â€“08 raw evidence20660 files retained with byte/hash inventory. Prior1052 decision objects remain byte-equivalent individually; no rewrite/reclassification. The initial reused capture stdout mistakenly printed historical08 counts; before.json has actual195/1052 and is authoritative. No ZIP file was independently revalidated.

| Group | File | Qualified records | Exports | Non-exports |
|---|---|---:|---:|---:|
{table}

A32=9exports+23nonexports; B39=22+17; C13=7+6. 84 unique exact IDs from pre09 pending queue,38 actual parsed ExcelFunction annotations,46 explicit nonexports; global268 registrations unchanged. Formula nested lambdas have shared signature movement keys; symbol_id and added member_identity_key retain parser span to disambiguate them. We preserve true census signature_key instead of inventing unique signatures. Handle auto-properties without initializers are in state.csv, not extra executable member records. A/B/C snapshots and later explicit new-only dependency/registration refinement preserve resumability; reads of excluded files/GUI do not confer review credit.

| Metric | Before | After |
|---|---:|---:|
{counters}

Overall1136/3866;2730 pending; UDF393/703 and310 pending. UI1372/Graphics1048 unchanged;1/2056 full control contracts,1604 unresolved wiring edges, zero legacy-detail gaps. Strict acceptance must exit2. Current remaining-member-review.csv retains exact outstanding IDs; no unfinished09 target IDs. Actual export/Ribbon/catalog/Common registry are distinct:268 exports,84 buttons/81 actions,76 authored methods/5 commands/2 pages, ANOVA/KW executable handlers only.

Formula validator calls existing Core RegressionFormulaDesignService with reference omission; direct ExcelReference absolute address acquisition is host-specific. Diagnostics are English heuristics, not another formula grammar. Common SpreadsheetModelInputCompiler.Compile uses typed roles/required-column filtering and is a comparison, not whole wrapper equivalent. Numeric eta helper indexing is distinct from Excel Num/NA/Inf text and residual spill presentation. Empty/ragged arrays and unchecked beta/header bounds have named future cases, not native pass claims.

LM fits existing Core LinearModel twice for TypeIII/TypeI, subtracts offset before fit and adds it in prediction; adjusted fitted residual column and GUI RunOLS effective offset/alpha need pinned comparison. Summary uses t inference and cached coding notes. Core term groups/fit/Data declarations and GUI route pins are explicit. ANOVA tables cache fit-time p presentation; read/drop/keycase/parallel workbook lifetime is not established by ConcurrentDictionary.

GLM/NB use existing Core fit and family/link implementations; power is a link parameter, not a newly supported family. Positive-weight helper policy differs from argument wording; optional parser defaults/domain and model errors remain baseline. GLM_RESID mutates model and recalculates. NB inherited residual path differs from inner NB AllResiduals override; NB convergence equality at iteration cap is a shared-engine candidate. Binary classification metrics already forward to BinaryClassificationCore; threshold/calibration/Brier UDF shapes/missing sentinel remain Windows reporting policies, not a new Common GLM handler.

ZIP carries separate component designs/coding/intercepts; count-only offset; fit import intersects RowIds, whereas prediction independently filters then checks row counts. Equal-count different-row identity and unbounded exp versus Core +/-700 clipping need future comparisons. Summary helper can silently omit malformed component rows; raw/Pearson spill finite policy differs from prediction Num cells. Core fit/residual/prediction declarations are pinned individually.

Cox uses existing Core CoxPH ties/variance/baseline/residual algorithms. Raw source RowIds become SurvivalRecord.Index; residual output sorts IDs while prediction subjects are renumbered. Time positivity differs from configurable Common survival parser; no correctness ruling made. Core baseline for Exact uses Breslow increment; Efron has separate denominator. Empty event stratum/singular information and all three ties/robust need future scientific fixtures. Summary lacks catch/SE guard, tests use robust covariance dimension checks and NA fallbacks; headerFalse drops Converged data row. Baseline and prediction hazard aliases have quantity/header inconsistencies. Step lookup includes event time (right-continuous). EnsureResiduals probes on-demand calculation then may refit/replace Result; no native safety assertion.

continuation09-gui-udf-alignment.csv has16 source-backed future issue/candidate rows with exact GUI/UDF/Core declarations, body/file pins, category, owner, existing future batch and ordinary/boundary cases. Observed baseline is not automatically canonical: numerical specification, host presentation and unknowns are separate. Both GUI and UDF should share existing Core algorithms and appropriate statistical validation; future intended policies remain provisional for owner approval.

Candidate member rows and deduplicated provisional operations are separate; counts are not a frozen global extraction census. Existing C06 stack/title operation extended; new C09 groups cover formula diagnostics, spills, eta request policy, LM, GLM, binary reporting request, NB, ZIP and Cox. .40 owns portable regression orchestration; .39 references prior imports rather than re-review; .28 semantic projection; .38 registration; .43 Windows cache/lifecycle; .44 actual fault/settings dependencies only. GC8 tagged only where actual logging calls are present. Every member has owner/batch/fixtures and exact source/dependency/portable declaration evidence.

Fresh command exits, managed distinct identities/counters, Python/Node counts, negative identities and log/TRX hashes are in continuation09-validation.json. Protected generated version copies are retained and originals restored byte-identically. Static reconciliation does not establish native recalculation, COM reference context, concurrency, workbook lifetime, GUI UX or scientific parity. Future Excel checks: weighted/offset categorical LM TypeI/III across recalculation and DROP; NB residual/ZIP extreme prediction and Cox ties/strata/baseline step aliases. WASM build/tests are automated smoke only; no production regression handler enabled.

Complete path manifest/future individual force-add plan covers every ignored indexed artifact; index SHA/bytes independently verified, no staging/commit/push/merge/reset/install/deployment or ignore-rule/product/framework change. Full04A.01 and Gate C2 remain incomplete. No04A.02/04A.40 implementation or Continuation10 started.
'''

if __name__=='__main__':
    parser=argparse.ArgumentParser(__doc__);parser.add_argument('--check',action='store_true');parser.add_argument('--guard',action='store_true');args=parser.parse_args();CHECK=args.check
    if args.guard:print(json.dumps(preservation()))
    else:main()
