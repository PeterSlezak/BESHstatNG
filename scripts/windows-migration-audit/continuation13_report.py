"""Classification, plot-data and PSM UDF checkpoint13; historical reports stay immutable."""
from __future__ import annotations
import argparse,collections,hashlib,json,re
import catalog,semantic,continuation03_report as logs,continuation07_report as prior
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-13'
BASE='BESHstatNG/src/ExcelUDFs/'
MV=BASE+'MultivariateAnalysisUDFs.vb'
TARGETS={MV:('ALL',134)}
EXPECTED_EXPORTS={MV:70}
GROUPS={'A':(1,1076,25,18),'B':(1077,1790,15,15),'C':(1791,2602,31,22),'D':(2603,3223,15,15),'E':(3224,9999,48,0)}
def group_for(s):return next(g for g,(lo,hi,n,e) in GROUPS.items() if lo<=int(s['start_line'])<=hi)
REG_FIELDS=('export_id','path','attribute_line','attribute_end_line','attribute','effective_name','symbol_id','implementing_name','signature','start_line','end_line')
FIELDS=prior.FIELDS+('registration_evidence',);CHECK=False
def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Checkpoint13 drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def historical_errors(root,records):
    errors=prior.historical_errors(root,records)
    for a in records:
        if a['path'].rsplit('/',1)[-1].startswith(('continuation07','continuation08','continuation09','continuation10','continuation11','continuation12')):
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
    for label,records,base in [('protected',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('snapshot12',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/a['path']).is_file() and sha(base/a['path'])==a['sha256'] for a in records),label+' mismatch'
    errors=historical_errors(ROOT,b['original_artifacts']);assert not errors,errors
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    files=catalog.load(OUT,'files.csv');assert len(files)==171 and all(sha(ROOT/f['path'])==f['sha256'] for f in files)
    raw=read(EVIDENCE/'prior-raw-evidence-hashes.json')['files'];errors=raw_history_errors(ROOT,raw);assert not errors,errors
    old=(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
    assert not prior_decision_errors((semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig'),old)
    return dict(branch=b['branch'],head=b['head'],tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(files),snapshot=len(b['original_artifacts']),baseline=len(b['baseline']['entries']),prior_raw_files=len(raw),prior_decision_objects=1312,historical='02-12 reports, snapshots and all recorded raw files unchanged;1312 original decision objects exact raw UTF8 bytes')
def scope_errors(symbols,decisions,queue):
    source={s['symbol_id']:s for s in symbols};pins={d.get('symbol_id'):d for d in decisions};errors=[]
    exports=catalog.load(OUT,'udf-exports.csv');reviewed=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-13']
    target=[s for s in symbols if s['path']==MV]
    if len(reviewed)!=134 or len({d['symbol_id'] for d in reviewed})!=134:errors.append('checkpoint13 missing/duplicate/out-of-scope decision count')
    if len(target)!=134 or collections.Counter(s['kind'] for s in target)!=dict(FunctionBlock=118,SingleLineFunctionLambdaExpression=9,VariableDeclarator=7):errors.append('checkpoint13 source kind/state/lambda denominator drift')
    for d in reviewed:
        if json.loads(d.get('source_identity','{}')).get('path')!=MV:errors.append('checkpoint13 outside source scope')
    for group,(lo,hi,count,expc) in GROUPS.items():
        ids={s['symbol_id'] for s in queue if lo<=int(s['start_line'])<=hi}
        if len(ids)!=count or sum(e['symbol_id'] in ids for e in exports)!=expc:errors.append('checkpoint13 exact group source/export denominator drift: '+group)
        for ident in ids:
            s=source.get(ident);d=pins.get(ident)
            if s is None or d is None or not semantic.qualified(s) or not semantic.qualified(d):errors.append('checkpoint13 missing/unqualified ID: '+ident);continue
            if d.get('checkpoint_id')!='04A.01-continuation-13' or d.get('review_group')!=group:errors.append('checkpoint13 incorrect group: '+ident)
            if d.get('body_sha256')!=s['body_sha256'] or d.get('signature_key')!=s['signature_key']:errors.append('checkpoint13 source pin drift: '+ident)
            if d.get('source_kind')!=s['kind'] or d.get('source_enclosing_symbol_id')!=s.get('enclosing_symbol_id',''):errors.append('checkpoint13 kind/enclosing drift: '+ident)
            project=lambda rows:[{k:r.get(k,'') for k in REG_FIELDS} for r in rows]
            if project(json.loads(d.get('registration_evidence','[]')))!=project([e for e in exports if e['symbol_id']==ident]):errors.append('checkpoint13 annotation evidence drift: '+ident)
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'checkpoint13')
            if d.get('equivalence_scope')=='whole-method' or d.get('whole_wrapper_equivalence')!='not asserted; host registration/marshalling/cache and specified policy remain separate':errors.append('checkpoint13 false whole-Core wrapper claim')
            if d.get('source_sha256')!=sha(ROOT/MV):errors.append('checkpoint13 source file hash drift')
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
    errors=[] if export_rows(selected,exports)==rows else ['checkpoint13 export/helper mapping drift']
    if sum(e['symbol_id'] in {s['symbol_id'] for s in selected} for e in exports)!=70:errors.append('checkpoint13 requires actual70 exported target IDs')
    if len(rows)!=134 or len({r['symbol_id'] for r in rows})!=134:errors.append('checkpoint13 export/helper member omission or duplicate')
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
        v['status']='NEW PROVISIONAL GROUP' if v['operation_id'].startswith('C13-') else 'EXTENSION OF EXISTING GROUP; NOT A NEW OPERATION'
        members=[s for s in selected if s.get('candidate_operation_id')==v['operation_id'] and s['disposition'].startswith('extract-to-')]
        v['owner']='; '.join(sorted({s.get('owner_role','') for s in members}))
        v['freeze_status']='NOT FROZEN; member rows and deduplicated operations are distinct'
    return values
def operation_errors(selected,operations):
    errors=[] if operation_rows(selected)==operations else ['checkpoint13 operation mapping drift']
    existing={r['operation_id'] for n in ('03','04','05','06','07','08','09','10','11','12') for r in catalog.load(OUT,'continuation'+n+'-candidate-operations.csv')}
    for o in operations:
        if not o['operation_id'].startswith('C13-') and o['operation_id'] not in existing:errors.append('Unknown claimed prior operation: '+o['operation_id'])
        if not o.get('owner') or not o['operation_id']:errors.append('Candidate operation lacks declared owner or identity')
    for s in selected:
        if s['disposition'].startswith('extract-to-') and (not s.get('owner_role') or not s.get('candidate_operation_id') or not s.get('future_batch')):errors.append('Candidate member lacks declared operation owner')
    return errors

def prior_decision_errors(current_text,original_text):
    errors=__import__('continuation08_report').prior_decision_errors(current_text,original_text)
    old=json.loads(original_text);now=json.loads(current_text)
    if [d.get('symbol_id') for d in now[:len(old)]]!=[d.get('symbol_id') for d in old]:errors.append('Prior decision order/prefix altered')
    return errors

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
    if len(ids)!=16 or len(set(ids))!=16:errors.append('alignment missing/duplicate candidate')
    return errors

def acceptance_errors(summary,index):
    open_work=summary.get('unreviewed_for_acceptance',0)>0 or summary.get('qualified_full_control_contracts',0)<summary.get('control_review_denominator',0)
    if open_work and (summary.get('status')!='NOT READY FOR ACCEPTANCE' or index.get('acceptance_status')!='NOT READY FOR ACCEPTANCE'):
        return ['Misleading READY status while member/control obligations remain']
    return []

def comparison_errors(rows):
    errors=[];portable=semantic.portable_sources()
    expected=read(EVIDENCE/'portable-comparisons-reviewed.json');expected_ids={r['symbol_id'] for r in expected}
    if len(rows)!=len(expected_ids) or {r.get('symbol_id') for r in rows}!=expected_ids:errors.append('Missing/duplicate bounded comparison declaration')
    for row in rows:
        actual=portable.get(row.get('symbol_id'))
        if actual is None or any(row.get(k)!=actual.get(k) for k in ('path','containing_type','signature','start_line','end_line','body_sha256','span_start','span_length')):
            errors.append('Stale portable comparison declaration');continue
        p=ROOT/row['path'];raw=p.read_bytes().decode('utf-8-sig')
        if sha(p)!=row.get('source_sha256') or raw[int(actual['span_start']):int(actual['span_start'])+int(actual['span_length'])]!=row.get('body_excerpt'):
            errors.append('Stale portable comparison body/file pin')
    return errors

def fixture_errors(rows):
    errors=[]
    if len(rows)!=8 or len({r.get('fixture_id') for r in rows})!=8:errors.append('Missing/duplicate existing fixture pin')
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
    runtime=read(OUT/'continuation13-runtime.json')
    errors=runtime_errors(runtime,runs+checks);assert not errors,errors
    first=read(EVIDENCE/'first-checks.json') if (EVIDENCE/'first-checks.json').exists() else []
    runtime['initial_failed_checks']=first
    initial_negative=EVIDENCE/'first-attempt/negative-reconciliation.log'
    if initial_negative.exists():
        text=logs.text(initial_negative)
        names=sorted(set(re.findall(r'(test_\w+)\s+\(',text)))
        failed=sorted(set(re.findall(r'(?:FAIL|ERROR):\s+(test_\w+)',text)))
        runtime['initial_negative_attempt']=dict(log=initial_negative.relative_to(ROOT).as_posix(),sha256=sha(initial_negative),reported_totals=re.findall(r'Ran (\d+) tests? in',text),distinct_identities=names,failed_identities=failed,passed_identities=[n for n in names if n not in failed],status='Initial stale derived-ledger failure retained; entire unchanged suite rerun after source/catalog refresh')
    runtime['initial_authoring_corrections']=[dict(path=(EVIDENCE/n).relative_to(ROOT).as_posix(),sha256=sha(EVIDENCE/n),details=read(EVIDENCE/n)) for n in ('initial-authoring-failure.json','alignment-initial-failure.json')]
    runtime['source_refinements']=dict(path=(EVIDENCE/'source-refinements.json').relative_to(ROOT).as_posix(),sha256=sha(EVIDENCE/'source-refinements.json'),members=len(read(EVIDENCE/'source-refinements.json')),status='New-only transitive normalization pins and two wording clarifications, before final derivation/full audit; original groups preserved')
    return dict(scope='Fresh continuation13 approved WinPython3.14.7; static review is not native/statistical parity',runs=runs,checks=checks,initial_commands=read(EVIDENCE/'initial-commands.json'),managed=managed,python_node=other,negative=negative,transient_generated_copies=generated,protection=read(EVIDENCE/'protection.json') if (EVIDENCE/'protection.json').exists() else 'Pending final guard',runtime=runtime,not_run=['native Excel/COM/XLL/GUI/graphics/workbooks/recalc/lifetime','Mac/authenticated Office','R scripts/new scientific goldens or ad hoc multivariate fitting','network/package installation/signing/deployment','04A.02/extraction or GUI/Graphics review credit'],development_notes=['A25/B15/C31/D15/E48 immutable source checkpoints;1312 prior decision objects retain literal ordered bytes.','Source refinements precede current analyzer/catalog generation to avoid stale derived pins.','Only existing managed/Python/Node suites run; no product test/fixture changes.','Current authority symbols.csv; no nonexistent members.csv used.','Final index/tracking/Git log records kept in raw13/final-current-checks.json to avoid recursive hashes.'])


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
    oldtext=(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
    assert not prior_decision_errors((semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig'),oldtext)
    assert len(json.loads(oldtext))==1312
    ids={s['symbol_id'] for s in queue};selected=sorted((s for s in symbols if s['symbol_id'] in ids),key=lambda s:(s['path'],int(s['span_start'])))
    assert len(selected)==134
    current={d['symbol_id']:d for d in decisions}
    assert not semantic.verify(symbols,catalog.load(OUT,'ui-wiring.csv'),catalog.load(OUT,'ui-controls.csv'))
    sem=read(OUT/'semantic-review-coverage.json');coverage=read(OUT/'review-coverage.json')
    assert not acceptance_errors(sem,read(OUT/'index.json'))
    exports=catalog.load(OUT,'udf-exports.csv');old_exports=catalog.load(oldroot,'udf-exports.csv')
    assert not registration_errors(exports,old_exports)
    mapped=export_rows(selected,exports);assert not export_errors(selected,exports,mapped)
    operations=operation_rows(selected);assert not operation_errors(selected,operations)
    align=catalog.load(OUT,'continuation13-gui-udf-alignment.csv');assert not alignment_errors(align)
    assert not comparison_errors(catalog.load(OUT,'continuation13-portable-comparisons.csv'))
    assert not fixture_errors(catalog.load(OUT,'continuation13-fixture-crosswalk.csv'))
    reviewed_alignment=read(EVIDENCE/'alignment-reviewed.json')
    assert align==reviewed_alignment,'Reviewed alignment text/pins changed'
    rich=[{**s,**current[s['symbol_id']]} for s in selected]
    write('continuation13-member-reviews.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in rich]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:({**s,**current.get(s['symbol_id'],{})}).get(k,'') for k in FIELDS} for s in candidates]))
    write('continuation13-export-helper-map.csv',catalog.render(mapped))
    filecounts=[]
    for g,(lo,hi,n,e) in GROUPS.items():
        ids={s['symbol_id'] for s in selected if group_for(s)==g}
        filecounts.append(dict(path=MV,group=g,records=n,reviewed=sum(semantic.qualified(s) for s in selected if s['symbol_id'] in ids),exports_before=sum(x['symbol_id'] in ids for x in old_exports),exports_after=sum(x['symbol_id'] in ids for x in exports),nonexports=n-e))
    write('continuation13-export-counts.csv',catalog.render(filecounts+[dict(path='ALL WINDOWS EXPORTS',group='global',records=3866,reviewed=sem['qualified_source_reviews'],exports_before=268,exports_after=268,nonexports='outside scope')]))
    write('continuation13-candidate-operations.csv',catalog.render(operations))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=x,allocation_evidence=e,records=n) for (f,r,d,x,e),n in sorted(reasons.items())]))
    memberops=[dict(symbol_id=s['symbol_id'],operation_id=s.get('candidate_operation_id',''),owner=s['owner_role'],destination=s['destination'],future_batch=s['future_batch'],disposition=s['disposition']) for s in selected]
    write('continuation13-operation-members.csv',catalog.render(memberops))
    state=[]
    for row in catalog.load(OUT,'state.csv'):
        if row['path'] not in TARGETS:continue
        members=[s for s in selected if s['path']==row['path']]
        state.append(dict(**row,source_sha256=sha(ROOT/row['path']),initializer_review_ids='; '.join(s['symbol_id'] for s in members if s['kind']=='VariableDeclarator'),assignment_review_ids='; '.join(s['symbol_id'] for s in members if s['name'].endswith('_FIT')),classification='Windows session handle container; model numeric state remains existing Core. Uninitialized property has no additional executable census record.',future_owner='Excel-DNA lifecycle maintainer / regression request maintainer',future_batch='04A.43; 04A.42',runtime_status='Lifetime/recalculation/concurrency NOT ESTABLISHED; see named member fixtures'))
    write('continuation13-handle-state.csv',catalog.render(state))
    for group in GROUPS:
        snapshot_path=EVIDENCE/('group-'+group+'-member-evidence.json')
        snap=read(snapshot_path)
        snap['original_snapshot']=dict(path=snapshot_path.relative_to(ROOT).as_posix(),sha256=sha(snapshot_path),initial_claimed_qualified=snap['qualified'],initial_contract_qualified=sum(semantic.qualified(d) for d in snap['decisions']),status='Immutable historical group draft; explicit refinements retained separately')
        snap['final_qualified_ids']=[s['symbol_id'] for s in selected if current[s['symbol_id']]['review_group']==group]
        snap['decisions']=[current[ident] for ident in snap['final_qualified_ids']]
        assert all(semantic.qualified(d) for d in snap['decisions'])
        snap['qualified']=len(snap['decisions'])
        snap['refinement_evidence']='Immutable original group saved only after all member contracts and raw prior-object byte checks passed. New-only source refinements retained in raw13/source-refinements.json; original groups immutable.'
        emit('continuation13-group-'+group+'.json',snap)
    emit('continuation13-start.json',{k:b[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    deltas={key:dict(before=before,after=after,delta=after-before) for key,before,after in (
      ('qualified_member_reviews',b['starting_review']['qualified_source_reviews'],sem['qualified_source_reviews']),
      ('remaining_member_reviews',b['starting_review']['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),
      ('ownership_investigations',b['starting_coverage']['member_investigations'],coverage['member_investigations']),
      ('unresolved_edges',b['starting_coverage']['unresolved_edge_rows'],coverage['unresolved_edge_rows']),
      ('full_controls',b['starting_review']['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),
      ('candidate_members',len(catalog.load(oldroot,'inspected-candidates.csv')),len(candidates)))}
    emit('continuation13-count-deltas.json',dict(counters=deltas,files=filecounts,families=sem['families'],new_operation_groups=sum(o['operation_id'].startswith('C13-') for o in operations),existing_extensions=[o['operation_id'] for o in operations if not o['operation_id'].startswith('C13-')],candidate_freeze='NOT FROZEN; member records are not deduplicated migration algorithms'))
    protection=[]
    for label,records in [('tracked2109',b['tracked']),('GateC1543',b['gate']),('compiled171',catalog.load(oldroot,'files.csv'))]:
        for row in records:
            after=sha(ROOT/row['path']);assert after==row['sha256'],row['path']
            protection.append(dict(protection_set=label,path=row['path'],before_sha256=row['sha256'],after_sha256=after,status='exact bytes preserved',current_bytes=(ROOT/row['path']).stat().st_size))
    for row in b['baseline']['entries']:
        path=row['retained_path'];after=sha(ROOT/path);assert after==row['sha256']
        protection.append(dict(protection_set='original52',path=path,before_sha256=row['sha256'],after_sha256=after,status='exact bytes preserved',current_bytes=(ROOT/path).stat().st_size))
    write('continuation13-protection.csv',catalog.render(protection))
    emit('continuation13-validation.json',validation())
    write('continuation13.md',review_text(b,filecounts,deltas,operations))
    # Complete path manifest includes every historical/current indexed artifact and self-excluded index.
    prefix='docs/architecture/WindowsMigrationLedger/'
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}
    paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}
    paths|={'docs/architecture/WindowsMigrationLedger.md',prefix+'continuation13-artifacts.csv'}
    old={r['path']:r['sha256'] for r in b['original_artifacts']}
    manifest=[dict(path=p,change_from_continuation13_start='unchanged' if p in old and sha(ROOT/p)==old[p] else 'modified' if p in old else 'added',tracking='ignored; future individual force-add' if p.startswith(prefix) else 'visible audit artifact',hash_authority='index.json exact SHA256/bytes; index self excluded') for p in sorted(paths)]
    write('continuation13-artifacts.csv',catalog.render(manifest))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith(prefix) else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    if CHECK:
        errors=logs.verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths']);assert not errors,errors
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=134,exports=70,overall=sem['qualified_source_reviews'],remaining=sem['unreviewed_for_acceptance'],candidate_rows=len(candidates),paths=len(paths))))




def review_text(b,files,deltas,operations):
    table='\n'.join('| '+f['group']+' | '+str(f['reviewed'])+'/'+str(f['records'])+' | '+str(f['exports_after'])+' | '+str(f['nonexports'])+' |' for f in files)
    counts='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    new=sum(o['operation_id'].startswith('C13-') for o in operations);ext=len(operations)-new
    return f'''# 04A.01 Continuation 13 — Complete multivariate UDF source audit

**CONTINUATION 13 COMPLETE AS A PARTIAL CHECKPOINT — 04A.01 NOT READY FOR ACCEPTANCE; Gate C2 unmet; E1–E7 open; GC3–GC8 deferred; 04A.02 not started.** This is continuation13 inside04A.01, not master-plan batch04A.13. No migration, product/test/fixture changes, algorithms, UI or graphics implementation, new goldens, native execution or owner acceptance.

Live branch `{b['branch']}`, HEAD/main `{b['head']}`. Starting index SHA256 `f4be3c75a09133a4e7d2b2ff6a9e5144ddd4cb6044b008da95ea838fda61a136`:269 indexed artifacts plus index, all bytes/hashes verified and losslessly snapshotted. Protected2109 tracked/GateC1543/compiled171/source census3866 remain unchanged. Original52 baseline paths,33158 prior02–12 raw evidence files and all1312 prior qualified decisions retain exact ordered literal object bytes, field order and historical line endings. Extra ignored ledger/tooling evidence preserved. No ZIP availability or independent archive validation claimed. `symbols.csv` is the real census; no nonexistent members.csv is used. Legacy top-level audit entry remains unchanged under the allowlist; current evidence is in this directory.

Single source `BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb`:134 exact original pending IDs,118 FunctionBlock/9 separate lambda/7 VariableDeclarator records.70 real parsed ExcelFunction registrations and64 explicit nonexports; global268 exports unchanged. Plain uninitialized handle properties receive no fabricated routine credit. Each body, source/profile/span/signature, dependency and actual portable declaration is pinned; a Core Fit call proves engine reuse only, never whole-wrapper equivalence.

| Group | Qualified records | Exports | Nonexports |
|---|---:|---:|---:|
{table}

Immutable A/B/C/D/E raw snapshots and progress JSON verify1337/1352/1383/1398/1446 cumulative reviews. Final current group projections preserve snapshot hashes and link17 new-only source refinements (transitive NormalizeToken pins and two wording clarifications). An authoring helper initially assumed historical checkpoint_id existed; another used constructor name New instead of actual Roslyn .ctor. Both failed before writes, were corrected and retained; the entire final audit suite is required after corrections. Prior1312 decisions are never normalized/reclassified. Reading GUI/Graphics/Core dependencies gives no additional Windows review credit.

| Metric | Before | After |
|---|---:|---:|
{counts}

Overall1446/3866,2420 pending (UI1372 + Graphics1048). UDF703/703 detailed source records complete, not native UDF qualification or04A.01 acceptance. Wiring1604 unresolved, full control contracts1/2056 (2055 incomplete), legacy detail gaps0 unchanged. Strict must return2/NOT READY. Exact derived ownership/candidate counts above are recomputed. Candidate operations are provisional, not final project extraction counts: {new} new groups and {ext} prior-group extensions, separate from member rows. Three private factor numerical helpers have Core candidates; other candidates are domain requests/outputs and native state/registration remains Windows. Existing C06 title and C09 spill operations are extended where source supports it.

Group A: seven independent OrdinalIgnoreCase static ConcurrentDictionary initializers retain typed mutable models and metadata. GUID handle maps are session-level; workbook isolation, automatic invalidation, safe model use, disposal and recalc concurrency NOT ESTABLISHED. Individual DROP and lookup members keep04A.43 ownership. PCA dataInputs aliases imported arrays, Calculate centers/standardizes, sorts eigenpairs, chooses loading-direction signs, clamps retained count1..p and projects scores. PCA loadings are retained DIRECTIONS, not automatically eigenvectors*sqrt(eigenvalues). GUI initial maxIter50 versus UDF250, both eps1e-6, is a future default/convergence candidate; both use the same Core algorithm. Constant-column validation applies even on covariance path because standardization is computed as well. Scores retain imported RowIds; GUI optional label/group/plot policies differ legitimately.

FA reuses Core dataInputs/settingsInputs/Calculate and diagnostic/rotation/score payloads. UDF omission chooses Eigenvalue1/None rotation; GUI initializes Fixed2/Varimax; Core settings defaults Fixed1/None. All are observed defaults, no canonical choice approved. Wrapper n>=2 reaches Core requirement>=3 complete rows; missing Error/Listwise and scoringNone->NA pathways pinned. Unrotated column squares and oblique Pattern*Structure sums are distinct. Local PercentOfTotal zeros zero/NaN/Inf total but divides negative total; private Core FA zeros total<=0. Null handling also differs in numeric duplicates. Future shared numeric policy needs scientific review. FA_FACTORABILITY has a concrete static array-bound defect: upper row5/4 with six data rows plus optional header writes last row6/5, caught and logged as text before p-value projection. Existing GUI/Core neutral tables do not share that manual bound. No UDF execution or fix occurred; future fixture must first preserve current failure evidence.

Group B: KMeans and HierarchicalClustering reuse existing Core preprocessing/Fit/results. Numeric import allowMissingTrue and label acquisition are separate prior policies. Imported DataObj.RowIds are NOT passed to cluster models; Core ActiveOriginalIndices=i+1 refers to input positions, not verified worksheet rows. Seed omitted delegates CoreServices.AnalysisDefaults; explicit seed and complete options required for deterministic comparison, cluster numbers/tied leaf order may permute. Supplied KMeans centers override initialization and force one start. Core owns standardization, empty-cluster strategy, multistart objective and retained rows; requested summary metadata versus actual starts/seed/convergence are distinct. Hierarchical Ward/Centroid/Median require Euclidean/squared distance; other supported linkages/distances are individually pinned. UDF cuts at accessor time versus GUI reportInputs/optional native dendrogram; exact-height equality, inversions, count3 on n2 and leaf-ID fallback need future fixtures.

Group C: CA importer enforces finite nonnegative near-integer counts/tolerance1e-7 and minimum2x2; Core chi-square/SVD owns axes, inertia and side-specific mass/coordinates/cos2/contributions. Zero grand total or margins have no additional UDF screening; finite/error outcome requires future reference. GUI Ui0 RunCA unlabeled branch stores TableData(i,j-1) from j0: static negative-index candidate, no GUI member credit/fix. MCA importer header default depends on explicit varNames; trimmed blank category versus omission is a policy issue. Core DataMultiple fits INDICATOR Z; Burt cross-product is reporting only. Category levels use default string ordering with Ordinal lookup, so culture/blank/repeated variable-category labels require comparison. MCA category metrics use column side, not observation row metrics. Nine captured selectors are individually span-pinned, call Core properties without independent numerical work, and share operation ownership. Eigen count, all stored axes and overview Quality(first min2 axes) are distinct. ResolveAvailableAxisCount stops on any exception/Nothing, then builders call selectors again; high-dimensional and short-vector behavior not proved equivalent by probing.

Group D: Core DA owns LDA/QDA, first-observed Ordinal group order, label-wise normalized priors, preprocessing, regularization, softmax/first-max classification and validation. No distinct GUI numerical engine demonstrated. UDF vector import/group alignment/source row provenance remain host/request policy. Core clamps folds>=2/ridge>=0 while cached summary can display requested values. Case-insensitive UDF covariance lookup can ambiguously select A/a groups that Core distinguishes Ordinally. Pooled/linear/canonical exports materialize Core wrapResults and find exact first-cell title strings; QDA/no-function output yields message tables, not universal NA. DA_PREDICT checks predictor COUNT and positional order, not name-based schema reorder, uses training transformation, and generates input-relative row indices; no verified equivalent GUI newX prediction route. Casewise table is5+2g columns, with posterior and squared distances, not raw scores. Native/error/spill outcomes unproved.

Group E: seven typed handle lookups reset ByRef output, normalize/trim keys and query their own map; no lease or disposal. Strict enum/string parsers use prior NormalizeToken (invariant uppercase, remove spaces/hyphens, retain underscores); omitted defaults differ from unknown ArgumentException. Optional numeric helpers instead truncate integers/fallback malformed, and finite validation is model-specific. The21 option parsers are policies, not21 independent extraction algorithms. Label helper uses imported count and generates Obs labels, not source IDs. Shared tables define fallback notes, variable/category/axis orientation, exact label lengths, raw nonfinite cells, current title strings and header extents. Most builders assume vector alignment; availability probing does not validate length. Common OutputCell.FromRaw has typed NonFinite errors and NumberCell rejects nonfinite, whereas current generic Windows array output preserves raw NaN/Inf and blank-converts Nothing/DBNull. No false whole-Common or universal numeric-error equivalence. ResultTableWriteRequestFactory is an inspected portable presentation contract, not multivariate workflow execution. Registry still constructs ANOVA/KW handlers only; Office capability not enabled.

`continuation13-member-reviews.csv` binds every ID; export/helper/count maps reconcile attributes. `continuation13-portable-comparisons.csv` pins109 actual Core/Common declarations and body/file hashes; eight existing tests are declaration-pinned in fixture-crosswalk, not new goldens. `continuation13-gui-udf-alignment.csv` has16 source-backed numerical, host or unknown/negative findings. Candidate policies are provisional pending scientific/owner approval; neither GUI nor UDF is universally canonical. `candidate-operations.csv` and `operation-members.csv` distinguish member rows, native state and deduplicated operations.04A.42 primary;04A.18 PCA/CA/MCA,04A.19 clustering,04A.20 FA/DA,04A.27/.39 acquisition,04A.28 output,04A.43 cache and04A.44 only actual logging/fault dependencies.04A.21–.24 are SPC batches in actual plan and are NOT assigned to generic multivariate extraction.04A.47 future native GUI tests,04A.02 future references; neither started. GC8 tags only actual logging/fault bodies; E1–E7 open/GC3–GC8 deferred unchanged.

Fresh commands/exits, WinPython3.14.7, managed/Python/Node distinct identities, parser/audit results and log/TRX hashes are in validation.json. Guarded Debug/Release full and Office validation restores Generated/VersionAttributes exact bytes and preserves transients separately. Protection CSV records2109+1543+171+52 before/after hashes; all historical raw evidence checked. The final index verifies all artifact bytes/SHA; independent index hash and final paths are in raw13/final-facts.json and final owner report (self-hash excluded). Artifact manifest covers added/modified/unchanged paths; future-staging.txt has individually specified proposed commands including ignored force-add, NEVER executed, no pyc/cache tracking. Git diff/staging/branch/status checks recorded.

NOT RUN: native Excel/COM/XLL, real workbook/GUI rendering, recalc/concurrency/lifetime, Mac/authenticated Office, new R/scientific goldens, ad hoc target model fits, network/install/sign/deploy. Existing managed reference tests are not new golden capture. Future owner scenarios: correlation/covariance PCA and rotated FA with singular/tied roots; factorability header bounds; seeded cluster/listwise row identity and dendrogram cuts; CA/MCA one/many axes and identical/blank categories; LDA/QDA unequal/case-sensitive groups with reordered priors and prediction columns; stale/padded/mixed-case handles and simultaneous FIT/DROP. Preserve today's baseline before any authorized behavior alignment. Stop for owner review; no UI/Graphics review or next continuation begins.
'''

if __name__=='__main__':
    parser=argparse.ArgumentParser(__doc__);parser.add_argument('--check',action='store_true');parser.add_argument('--guard',action='store_true');args=parser.parse_args();CHECK=args.check
    if args.guard:print(json.dumps(preservation()))
    else:main()
