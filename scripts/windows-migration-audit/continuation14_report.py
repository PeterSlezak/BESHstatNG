"""Graphics source checkpoint14; historical reports stay immutable."""
from __future__ import annotations
import argparse,collections,hashlib,json,re
import catalog,semantic,continuation03_report as logs,continuation07_report as prior
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-14'
BASE='BESHstatNG/src/Graphics/'
GROUPS=read(EVIDENCE/'targets.json')
TARGETS={BASE+f:(g['group'],n) for g in GROUPS for f,n in g['files'].items()}
EXPECTED_EXPORTS={p:0 for p in TARGETS}
GROUP_IDS={g['group']:{s['symbol_id'] for s in g['records']} for g in GROUPS}
TARGET_IDS=set().union(*GROUP_IDS.values())
def group_for(s):return next(g for g,ids in GROUP_IDS.items() if s['symbol_id'] in ids)
REG_FIELDS=('export_id','path','attribute_line','attribute_end_line','attribute','effective_name','symbol_id','implementing_name','signature','start_line','end_line')
FIELDS=prior.FIELDS+('registration_evidence',);CHECK=False
def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Checkpoint14 drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def historical_errors(root,records):
    errors=prior.historical_errors(root,records)
    for a in records:
        if a['path'].rsplit('/',1)[-1].startswith(('continuation07','continuation08','continuation09','continuation10','continuation11','continuation12','continuation13')):
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
    for label,records,base in [('protected',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('snapshot13',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/a['path']).is_file() and sha(base/a['path'])==a['sha256'] for a in records),label+' mismatch'
    errors=historical_errors(ROOT,b['original_artifacts']);assert not errors,errors
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    files=catalog.load(OUT,'files.csv');assert len(files)==171 and all(sha(ROOT/f['path'])==f['sha256'] for f in files)
    raw=read(EVIDENCE/'prior-raw-evidence-hashes.json')['files'];errors=raw_history_errors(ROOT,raw);assert not errors,errors
    old=(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
    assert not prior_decision_errors((semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig'),old)
    return dict(branch=b['branch'],head=b['head'],tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(files),snapshot=len(b['original_artifacts']),baseline=len(b['baseline']['entries']),prior_raw_files=len(raw),prior_decision_objects=1446,historical='02-13 reports, snapshots and all recorded raw files unchanged;1446 original decision objects exact raw UTF8 bytes')
def source_fault_errors(symbols,decisions):
    source={s['symbol_id']:s for s in symbols};errors=[];path='BESHStat.Core/src/AppInfrastructure/CoreInfrastructure.vb'
    for d in decisions:
        if d.get('checkpoint_id')!='04A.01-continuation-14':continue
        s=source.get(d['symbol_id'])
        if not s:continue
        raw=(ROOT/s['path']).read_bytes().decode('utf-8-sig',errors='replace');body=raw[int(s['span_start']):int(s['span_start'])+int(s['span_length'])]
        if 'CoreServices.Errors.LogAndThrow' not in body:continue
        if json.loads(d.get('dependency_pins','{}')).get(path)!=sha(ROOT/path):errors.append('Missing/stale direct fault dependency')
        if 'GC8' not in d.get('exception_tags','') or '04A.44' not in d.get('future_batch',''):errors.append('Direct fault lacks exception/policy owner')
        if not any(p['path']==path and p['containing_type'].endswith('.DefaultErrorReporter') and p['signature'].startswith('Public Sub LogAndThrow') for p in json.loads(d.get('compared_portable_declarations','[]'))):errors.append('Direct fault lacks actual declaration pin')
    return errors

def scope_errors(symbols,decisions,queue):
    errors=[];source={s['symbol_id']:s for s in symbols};reviewed=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-14'];by={d['symbol_id']:d for d in reviewed}
    if len(reviewed)!=216 or set(by)!=TARGET_IDS:errors.append('Missing/duplicate/fabricated/out-of-scope checkpoint14 IDs')
    if {s['symbol_id'] for s in queue}!=TARGET_IDS:errors.append('Original pending target set drift')
    for g in GROUPS:
        ids=GROUP_IDS[g['group']]
        if len(ids)!=sum(g['files'].values()):errors.append('Group denominator drift')
        for ident in ids:
            s=source.get(ident);d=by.get(ident)
            if s is None or d is None or not semantic.qualified(s) or not semantic.qualified(d):errors.append('Missing/unqualified ID: '+ident);continue
            if d.get('review_group')!=g['group']:errors.append('Incorrect group')
            for key,field in [('body_sha256','body_sha256'),('signature_key','signature_key'),('kind','source_kind'),('enclosing_symbol_id','source_enclosing_symbol_id'),('profile','source_profile'),('span_start','source_span_start'),('span_length','source_span_length')]:
                if s.get(key,'')!=d.get(field,''):errors.append('Stale source identity '+key)
            if d.get('source_sha256')!=sha(ROOT/s['path']):errors.append('Stale source SHA')
            if json.loads(d.get('registration_evidence','[]')) or d.get('registration_class')!='explicit non-export':errors.append('Graphics helper falsely registered')
            if d.get('whole_wrapper_equivalence')!='not asserted; host registration/marshalling/cache and specified policy remain separate' or d.get('equivalence_scope')=='whole-method':errors.append('False whole Core/native wrapper equivalence')
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'checkpoint14')
    return errors+source_fault_errors(symbols,decisions)

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
    errors=[] if export_rows(selected,exports)==rows else ['Export/helper map drift']
    if len(rows)!=216 or len({r['symbol_id'] for r in rows})!=216:errors.append('Missing/duplicate helper records')
    if any(e['symbol_id'] in TARGET_IDS for e in exports):errors.append('Graphics target falsely exported')
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
        v['status']='NEW PROVISIONAL GROUP' if v['operation_id'].startswith('C14-') else 'EXTENSION OF EXISTING GROUP; NOT A NEW OPERATION'
        members=[s for s in selected if s.get('candidate_operation_id')==v['operation_id'] and s['disposition'].startswith('extract-to-')]
        v['owner']='; '.join(sorted({s.get('owner_role','') for s in members}))
        v['freeze_status']='NOT FROZEN; member rows and deduplicated operations are distinct'
        v['related_prior_operation_ids']='; '.join({'C14-B01-histogram-specification': ['C12-B01-plot-data-projection', 'C07-A02-histogram-rule-admission'], 'C14-C01-reference-interval-specification': ['C07-C05-agreement-output-projection'], 'C14-C02-survival-series-specification': ['C12-B01-plot-data-projection', 'C09-C01-cox-request-result'], 'C14-C03-multivariate-chart-specification': ['C13-A01-pca-fa-request-projection'], 'C14-D01-loveplot-projection': ['C12-C02-psm-result-dispatch'], 'C14-D02-biplot-geometry': ['C13-A01-pca-fa-request-projection']}.get(v['operation_id'],[]))
        v['group_boundary']='Native chart specification/geometry is separate from prior UDF tabular parsing/projection; only the explicit existing ID is an extension, related IDs confer no new review credit.'
    return values
def operation_errors(selected,operations):
    errors=[] if operation_rows(selected)==operations else ['checkpoint14 operation mapping drift']
    existing={r['operation_id'] for n in ('03','04','05','06','07','08','09','10','11','12','13') for r in catalog.load(OUT,'continuation'+n+'-candidate-operations.csv')}
    for o in operations:
        if not o['operation_id'].startswith('C14-') and o['operation_id'] not in existing:errors.append('Unknown claimed prior operation: '+o['operation_id'])
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
    if len(ids)!=len(read(EVIDENCE/'alignment-reviewed.json')) or len(set(ids))!=len(ids):errors.append('alignment missing/duplicate candidate')
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
        p=ROOT/row['path'];raw=p.read_bytes().decode('utf-8-sig',errors='replace')
        if sha(p)!=row.get('source_sha256') or raw[int(actual['span_start']):int(actual['span_start'])+int(actual['span_length'])]!=row.get('body_excerpt'):
            errors.append('Stale portable comparison body/file pin')
    return errors

def fixture_errors(rows):
    errors=[]
    if len(rows)!=len(read(EVIDENCE/'fixtures-reviewed.json')) or len({r.get('fixture_id') for r in rows})!=len(rows):errors.append('Missing/duplicate existing fixture pin')
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
    runtime=read(OUT/'continuation14-runtime.json')
    errors=runtime_errors(runtime,runs+checks);assert not errors,errors
    first=read(EVIDENCE/'first-checks.json') if (EVIDENCE/'first-checks.json').exists() else []
    runtime['initial_check_attempts']=first
    runtime['initial_failed_checks']=[r for r in first if r.get('exit_code')!=r.get('expected_exit_code',0)]
    initial_negative=EVIDENCE/'first-attempt/negative-reconciliation.log'
    if initial_negative.exists():
        text=logs.text(initial_negative)
        names=sorted(set(re.findall(r'(test_\w+)\s+\(',text)))
        failed=sorted(set(re.findall(r'(?:FAIL|ERROR):\s+(test_\w+)',text)))
        runtime['initial_negative_attempt']=dict(log=initial_negative.relative_to(ROOT).as_posix(),sha256=sha(initial_negative),reported_totals=re.findall(r'Ran (\d+) tests? in',text),distinct_identities=names,failed_identities=failed,passed_identities=[n for n in names if n not in failed],status='Initial entire audit attempt retained verbatim; final entire suite rerun after source-backed fault-pin refinement. Failure list above is authoritative; initial success is not relabeled failure.')
    runtime['initial_authoring_corrections']=[dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p),details=read(p)) for p in sorted(EVIDENCE.glob('*failure.json'))]+[dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p),details=read(p)) for p in sorted(EVIDENCE.glob('*correction.json'))]
    runtime['source_refinements']=dict(path=(EVIDENCE/'source-refinements.json').relative_to(ROOT).as_posix(),sha256=sha(EVIDENCE/'source-refinements.json'),records=len(read(EVIDENCE/'source-refinements.json')),status='New-only direct fault declaration/dependency pins and one GC8/batch annotation clarified; all1446 prior decisions and original group snapshots immutable. No product/algorithm/expectation changes.')
    runtime['managed_case_identity_analysis']=dict(path=(EVIDENCE/'managed-identity-analysis.json').relative_to(ROOT).as_posix(),sha256=sha(EVIDENCE/'managed-identity-analysis.json'),details=read(EVIDENCE/'managed-identity-analysis.json'))

    return dict(scope='Fresh continuation14 approved WinPython3.14.7; static review is not native/statistical parity',runs=runs,checks=checks,initial_commands=read(EVIDENCE/'initial-commands.json'),managed=managed,python_node=other,negative=negative,transient_generated_copies=generated,protection=read(EVIDENCE/'protection.json') if (EVIDENCE/'protection.json').exists() else 'Pending final guard',runtime=runtime,not_run=['native Excel/COM/XLL/GUI/graphics/workbooks/recalc/lifetime','Mac/authenticated Office','R scripts/new scientific goldens or ad hoc statistical fitting','network/package installation/signing/deployment','04A.02/extraction or untargeted GUI/Graphics review credit'],development_notes=['A26/B55/C38/D41/E56 immutable source checkpoints;1446 prior decision objects retain literal ordered bytes.','Source refinements precede current analyzer/catalog generation to avoid stale derived pins.','Only existing managed/Python/Node suites run; no product test/fixture changes.','Current authority symbols.csv; no nonexistent members.csv used.','Final index/tracking/Git log records kept in raw14/final-current-checks.json to avoid recursive hashes.'])


APPROVED_PYTHON='C:/Users/peter/Desktop/WPy64-31470/python/python.exe'
def runtime_errors(runtime,commands):
    errors=[]
    if runtime.get('executable','').replace('\\','/').lower()!=APPROVED_PYTHON.lower():errors.append('Unapproved fresh Python runtime')
    for command in commands:
        values=command.get('command',[])
        if isinstance(values,str):values=re.findall(r'[^ ]*python(?:[.]exe)?',values,re.I)
        values=list(values)+[command.get('exe','')]+list(command.get('arguments',[]))
        for value in values:
            value=str(value).strip('"')
            if value.lower().endswith('/python.exe') or value.lower().endswith('\\python.exe'):
                if value.replace('\\','/').lower()!=APPROVED_PYTHON.lower():errors.append('Fresh command selects unapproved Python')
    return errors

def tracking_errors(index,paths,commands):
    errors=[];expected={a['path'] for a in index['artifacts']}|{'docs/architecture/WindowsMigrationLedger/index.json'}
    if set(paths)!=expected or len(paths)!=len(set(paths)):errors.append('Tracking/index missing/duplicate path')
    expected_lines=[('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(paths)]
    actual=[line for line in commands.splitlines() if line and not line.startswith('#')]
    if actual!=expected_lines:errors.append('Missing/extra/non-individual/incorrect force-add proposal')
    if any('__pycache__' in p or p.endswith('.pyc') for p in paths):errors.append('Generated cache proposed for tracking')
    return errors

def main():
    b=read(EVIDENCE/'before.json');oldroot=EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
    symbols=catalog.load(OUT,'symbols.csv');decisions=semantic.read_store('review-decisions.json');current={d['symbol_id']:d for d in decisions}
    queue=[s for s in catalog.load(oldroot,'remaining-member-review.csv') if s['path'] in TARGETS]
    errors=scope_errors(symbols,decisions,queue);assert not errors,errors
    oldtext=(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig');assert len(json.loads(oldtext))==1446
    assert not prior_decision_errors((semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig'),oldtext)
    selected=sorted((s for s in symbols if s['symbol_id'] in TARGET_IDS),key=lambda s:(s['path'],int(s['span_start'])))
    assert len(selected)==216 and len(symbols)==3866
    assert not semantic.verify(symbols,catalog.load(OUT,'ui-wiring.csv'),catalog.load(OUT,'ui-controls.csv'))
    sem=read(OUT/'semantic-review-coverage.json');coverage=read(OUT/'review-coverage.json');assert not acceptance_errors(sem,read(OUT/'index.json'))
    exports=catalog.load(OUT,'udf-exports.csv');original_exports=catalog.load(oldroot,'udf-exports.csv');assert not registration_errors(exports,original_exports)
    mapped=export_rows(selected,exports);assert not export_errors(selected,exports,mapped)
    operations=operation_rows(selected);assert not operation_errors(selected,operations)
    for name,check in [('gui-udf-alignment',alignment_errors),('portable-comparisons',comparison_errors),('fixture-crosswalk',fixture_errors)]:
        errors=check(catalog.load(OUT,'continuation14-'+name+'.csv'));assert not errors,errors
    assert catalog.load(OUT,'continuation14-gui-udf-alignment.csv')==read(EVIDENCE/'alignment-reviewed.json')
    rich=[{**s,**current[s['symbol_id']]} for s in selected]
    write('continuation14-member-reviews.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in rich]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:({**s,**current.get(s['symbol_id'],{})}).get(k,'') for k in FIELDS} for s in candidates]))
    write('continuation14-export-helper-map.csv',catalog.render(mapped))
    filecounts=[]
    for path,(group,n) in TARGETS.items():
        selected_file=[s for s in selected if s['path']==path]
        filecounts.append(dict(path=path,group=group,records=n,reviewed=sum(semantic.qualified(s) for s in selected_file),total_file_records=sum(s['path']==path for s in symbols),prior_reviewed_file_records=sum(s['path']==path and s['symbol_id'] not in TARGET_IDS and semantic.qualified(s) for s in symbols),exports_before=sum(x['path']==path for x in original_exports),exports_after=sum(x['path']==path for x in exports),nonexports=n))
    write('continuation14-export-counts.csv',catalog.render(filecounts+[dict(path='ALL WINDOWS EXPORTS',group='global',records=3866,reviewed=sem['qualified_source_reviews'],exports_before=268,exports_after=268,nonexports='outside target scope')]))
    write('continuation14-candidate-operations.csv',catalog.render(operations))
    write('continuation14-operation-members.csv',catalog.render([dict(symbol_id=s['symbol_id'],operation_id=s.get('candidate_operation_id',''),owner=s['owner_role'],destination=s['destination'],future_batch=s['future_batch'],disposition=s['disposition']) for s in selected]))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=x,allocation_evidence=e,records=n) for (f,r,d,x,e),n in sorted(reasons.items())]))
    for g in GROUPS:
        group=g['group'];p=EVIDENCE/('group-'+group+'-member-evidence.json');snap=read(p);ids=GROUP_IDS[group]
        refinements={r['symbol_id']:r for r in read(EVIDENCE/'source-refinements.json')}
        assert {d['symbol_id'] for d in snap['decisions']}==ids
        assert all(current[d['symbol_id']]==d or (refinements.get(d['symbol_id'],{}).get('before')==d and refinements[d['symbol_id']]['after']==current[d['symbol_id']]) for d in snap['decisions'])
        snap['final_qualified_decisions']=[current[d['symbol_id']] for d in snap['decisions']]
        snap['refinement_evidence']=dict(path=(EVIDENCE/'source-refinements.json').relative_to(ROOT).as_posix(),sha256=sha(EVIDENCE/'source-refinements.json'),original_group_unchanged=True)
        snap['original_snapshot']=dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p),immutable=True)
        snap['progress']=read(EVIDENCE/('progress-'+group+'.json'));snap['checks']=read(EVIDENCE/('group-'+group+'-checks.json'));snap['protection']=read(EVIDENCE/('group-'+group+'-protection.json'))
        emit('continuation14-group-'+group+'.json',snap)
    emit('continuation14-start.json',{k:b[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    deltas={key:dict(before=before,after=after,delta=after-before) for key,before,after in (
      ('qualified_member_reviews',b['starting_review']['qualified_source_reviews'],sem['qualified_source_reviews']),
      ('remaining_member_reviews',b['starting_review']['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),
      ('ownership_investigations',b['starting_coverage']['member_investigations'],coverage['member_investigations']),
      ('unresolved_edges',b['starting_coverage']['unresolved_edge_rows'],coverage['unresolved_edge_rows']),
      ('full_controls',b['starting_review']['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),
      ('candidate_members',len(catalog.load(oldroot,'inspected-candidates.csv')),len(candidates)))}
    emit('continuation14-count-deltas.json',dict(counters=deltas,files=filecounts,families=sem['families'],new_operation_groups=sum(o['operation_id'].startswith('C14-') for o in operations),existing_extensions=[o['operation_id'] for o in operations if not o['operation_id'].startswith('C14-')],candidate_freeze='NOT FROZEN; source-member candidates differ from deduplicated provisional operations'))
    protection=[]
    for label,records in [('tracked2109',b['tracked']),('GateC1543',b['gate']),('compiled171',catalog.load(oldroot,'files.csv'))]:
        for row in records:
            after=sha(ROOT/row['path']);assert after==row['sha256'],row['path'];protection.append(dict(protection_set=label,path=row['path'],before_sha256=row['sha256'],after_sha256=after,status='exact bytes preserved',current_bytes=(ROOT/row['path']).stat().st_size))
    for row in b['baseline']['entries']:
        path=row['retained_path'];after=sha(ROOT/path);assert after==row['sha256'];protection.append(dict(protection_set='original52',path=path,before_sha256=row['sha256'],after_sha256=after,status='exact bytes preserved',current_bytes=(ROOT/path).stat().st_size))
    write('continuation14-protection.csv',catalog.render(protection));emit('continuation14-validation.json',validation());write('continuation14.md',review_text(b,filecounts,deltas,operations))
    prefix='docs/architecture/WindowsMigrationLedger/';paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}
    paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()};paths|={'docs/architecture/WindowsMigrationLedger.md',prefix+'continuation14-artifacts.csv'}
    assert not any('__pycache__' in p or p.endswith('.pyc') for p in paths)
    old={r['path']:r['sha256'] for r in b['original_artifacts']}
    write('continuation14-artifacts.csv',catalog.render([dict(path=p,change_from_continuation14_start='unchanged' if p in old and sha(ROOT/p)==old[p] else 'modified' if p in old else 'added',tracking='ignored; future individual force-add' if p.startswith(prefix) else 'visible audit artifact',hash_authority='index.json exact SHA256/bytes; index self excluded') for p in sorted(paths)]))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith(prefix) else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    if CHECK:
        errors=logs.verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths'])+tracking_errors(read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths'],(OUT/'future-staging.txt').read_text(encoding='utf-8-sig'));assert not errors,errors
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=216,target_exports=0,global_exports=268,overall=sem['qualified_source_reviews'],remaining=sem['unreviewed_for_acceptance'],candidate_rows=len(candidates),target_candidate_rows=sum(s['disposition'].startswith('extract-to-') for s in selected),deduplicated_operations=len(operations),paths=len(paths))))

def review_text(b,files,deltas,operations):
    groups='\n'.join('| '+g['group']+' | '+str(sum(g['files'].values()))+'/'+str(sum(g['files'].values()))+' | '+str(len(g['files']))+' |' for g in GROUPS)
    counters='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    val=validation();managed={c:sum(int(r['passed']) for r in val['managed'] if r['configuration']==c and r['distinct_full_suite']) for c in ('Debug','Release')}
    new=sum(o['operation_id'].startswith('C14-') for o in operations);ext=len(operations)-new
    return f'''# 04A.01 Continuation 14 — Graphics source audit

**CONTINUATION 14 COMPLETE AS A PARTIAL CHECKPOINT — 04A.01 NOT READY FOR ACCEPTANCE; Gate C2 unmet; E1–E7 open; GC3–GC8 deferred; 04A.02 not started.**

Branch `{b['branch']}`, HEAD/main `{b['head']}`. This is audit evidence inside04A.01, not numbered04A.14 or execution of04A.29. No product/source/test-fixture/statistical/chart/UI changes, extraction, new golden capture or native execution. All changes remain unstaged/uncommitted for owner review.

## Exact bounded source coverage

| Group | Qualified pending IDs | Modules |
|---|---:|---:|
{groups}

216 distinct original pending Roslyn IDs across24 modules are qualified; constructors, initialized properties/state, lambdas and Resize event records are included individually. Targets are explicit nonexports; global268 parsed ExcelFunction names/attributes/signatures unchanged and UDF703/703 unchanged. Two already-reviewed facade records in CumulativeDistributionPlot and CategoricalHistogram were excluded from the216 pending IDs and their old objects preserved. Total file source counts exceed pending counts for those files; no duplicate credit was awarded. No other Graphics or UI source member receives credit.

| Counter | Before | After |
|---|---:|---:|
{counters}

Overall1662/3866; Graphics230/1062 with832 remaining; UI175/1547 with1372 remaining; total2204 pending. Wiring1604 remains unresolved, full controls1/2056 so2055 incomplete. Investigations/candidate counts above derive from current ledger, not expected subtraction. Source171/3866, active3864+alternate2 and all268 registrations remain unchanged. No legacy-detail gaps. Strict full acceptance must remain exit2; integrity failures are not acceptance blockers to excuse.

## Individual boundaries and alignment findings

A: Bullet/CDF/Dumbbell/Ladder/Sankey numeric facades forward to their existing Core engines. Existing Core validation/order/copy policies remain authoritative source evidence, not GUI/runtime parity. ChartingFunc mixes Core histogram/axis forwarding with native scatter and Timer-dependent palette fallback. Neutral XY series/style policy is separable, native chart operations remain Windows.

B: Core histogram/categorical calculations remain Core. Windows histogram owns chart/box overlay and WithEvents Resize/registry lifetime. Categorical comparative mirroring acts on cloned PlotValues, so that helper does not mutate the Core payload. Native orientation/unit/percentage labels, symmetric nice-scale policy, box-to-pixel projection and cleanup require later comparison. Ordinary FD/normal-overlay source sorting may mutate caller data. No04A.75 raw-bin/PrettyBreaks enhancement or checkbox was implemented.

C: Agreement/Theil-Sen, survival, clustering, CA and FA payloads already exist in Core; each renderer still creates/mutates native charts. CA title code selects cumulative-percent cells with shifted axis indexes, including an out-of-range row for exactly2 axes hidden by catch: future source-backed label/index investigation. Symmetry X uses upper-tail distance while caption says lower (Y converse), and sorts aliased caller input: future scientific/label/immutability review. FA crossproduct/trace/percentage helpers duplicate private Core implementations; percentage finite/nonpositive policy differs from previously audited UDF helper, not proof which should be canonical. KM Core prep already duplicates step times; zero-censor/group-count behavior remains open. Cox renderer accepts supplied arrays without independently expanding steps, so payload semantics need a future pinned comparison.

D: PCA uses existing Core scores/loadings but local biplot eigen scaling, retained-row label lengths and grouped first-seen Ordinal category order need neutral projection contracts. Loading captions currently say Scores. Love-plot table/chart sort by maximum absolute SMD across before/matching/weighting, unlike Core diagnostics MaxAbsAfter order; signed diagnostic vs absolute plot schema is presentation, not automatically a scientific defect. Plot-wide threshold chooses first finite-positive threshold after sorting; per-row differences are not represented by multiple lines.

E: Clipboard/PInvoke/EMF/GDI+/native encoders/file streams/WinForms progress remain Windows. Numeric memory estimates do not make export portable; overflow, decoder-vs-encoder lookup, synchronous Invoke/DoEvents and cleanup need later native fixtures. Pure GIF centisecond rounding is a narrow policy candidate. Sphere/ellipsoid raw bounds/mesh sampling are separable from XYZ owner projection and COM series creation. Scatter-matrix transform has pure numeric layout plus existing Core correlation/slope helpers; constructor overwrites requested margin with.1, constant-column and nonfinite behavior need canonical specification. The50 pending XYZscatter records were inspected only as dependency, with no credit.

The source-pinned alignment register retains proven differences, legitimate host-only presentation, negative findings and unknowns. GUI/UDF should converge on one scientifically reviewed Core calculation authority wherever appropriate; neither current GUI nor UDF is declared universally correct. Common OutputPlotArtifact is only an opaque logical reference/purpose, not an existing geometry renderer. Actual registered Common executables remain ANOVA/Kruskal–Wallis; this audit enables no Office Graphics execution.

## Operations, evidence and preservation

`continuation14-member-reviews.csv` contains all216 rich member-v1 rows. Portable comparisons bind exact declaration spans/bodies/source SHA; export-helper/count tables prove0 target exports and268 global. `continuation14-operation-members.csv` distinguishes retained/native/facade members from candidate member rows; `continuation14-candidate-operations.csv` has{new} new provisional operation groups and{ext} existing-group extensions, not216 migrations. Future approved owners are04A.30–.37 by actual geometry/rendering scope,04A.27/.28 input/output,04A.44 only actual fault/style policy,04A.47 later native verification and04A.02 later references.04A.21–.24 are not generic graphics.04A.75 remains future optional enhancement. No roadmap or gate changes.

Five immutable raw group snapshots/progress/check logs and ledger group records preserve restart IDs. All1446 earlier decision objects retain literal UTF-8 object bytes and order; append never reformats the old entries. All292 starting indexed paths and extra ignored audit evidence were recoverably snapshotted; baseline52, protected2109, GateC1543 and compiled171 hashes remain exact. Historical02–13 reports/snapshots plus36377 recorded prior raw files are guarded. Legacy non-UTF8 source bytes are preserved; comparison excerpts use Roslyn File.ReadAllText-compatible replacement, while raw hashes remain authoritative. Initial scope-helper, authoring import/encoding, duplicate-keyword, alignment-name and template-BOM errors occurred before affected saves/rendering; failure/correction records are retained. A final five-record direct-fault reread adds exact Core error-reporter pins; one new-review GC8/batch annotation is clarified. Original group objects are retained separately and old1446 decisions unchanged. Entire audit suite rerun after this refinement.

## Actual validation and limitations

Fresh guarded Debug/Release distinct managed passes: {managed}. Office foundation duplicate identities are reported separately in validation JSON. Python/Node exact identities/counts, all commands/exit codes/log/TRX hashes, transient generated copies and byte-restoration evidence are in `continuation14-validation.json`; full corrected audit negatives: {val.get('negative',{}).get('distinct_tests','pending')}. Source/catalog/parser/report/tracking/protection checks and strict expected exit2 are recorded separately from green product suites. The complete added/modified/unchanged path manifest is `continuation14-artifacts.csv`; index.json lists exact sizes/SHA and excludes itself. Independent final index hash and proposed-path count are retained in raw final-facts.json to avoid recursive hashes. Individual future force-add commands are instructions only; no staging executed, caches excluded.

NOT RUN: native Excel/COM/XLL/clipboard/chart exports/GIF playback/WinForms/GUI rendering, real workbook recalc/lifetime/concurrency, Mac/authenticated Office, fresh R/statistical goldens, installers/signing/deployment/network installs. Static source qualification does not prove runtime/native/statistical parity. Later owner checks should separately pin histogram bin/resize behavior, CA/symmetry quantity labels, PCA retained rows/biplot axes, Love threshold/order, KM tied/censored steps, XYZ bounds and native export/progress/disposal. No continuation15 or later batch begins automatically.
'''

if __name__=='__main__':
    parser=argparse.ArgumentParser(__doc__);parser.add_argument('--check',action='store_true');parser.add_argument('--guard',action='store_true');args=parser.parse_args();CHECK=args.check
    if args.guard:print(json.dumps(preservation()))
    else:main()
