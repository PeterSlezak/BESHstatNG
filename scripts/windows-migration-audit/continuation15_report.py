"""Bounded specialty Graphics source audit, no product/runtime parity inference."""
from __future__ import annotations
import argparse,collections,csv,hashlib,json,sys,subprocess,xml.etree.ElementTree as ET
from pathlib import Path
import catalog,semantic,continuation14_report as prior
from amendment import sha,read,git
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-15'
GROUPS=read(EVIDENCE/'targets.json')
IDS={s['symbol_id'] for g in GROUPS for s in g['records']}
SOURCE={s['symbol_id']:s for g in GROUPS for s in g['records']}
PREFIX='docs/architecture/WindowsMigrationLedger/'
CHECK=False

def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        assert p.is_file() and p.read_bytes()==value.encode('utf-8'),'C15 report drift: '+name
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def excerpt(s):
    raw=(ROOT/s['path']).read_bytes().decode('utf-8-sig',errors='replace').encode('utf-16-le')
    return raw[2*int(s['span_start']):2*(int(s['span_start'])+int(s['span_length']))].decode('utf-16-le')
def scope_errors(symbols,decisions):
    errors=[];ss={s['symbol_id']:s for s in symbols};ds=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-15']
    if len(ds)!=232 or {d['symbol_id'] for d in ds}!=IDS:return ['Missing/duplicate/out-of-scope C15 IDs']
    for g in GROUPS:
        for original in g['records']:
            ident=original['symbol_id'];s=ss.get(ident);d=next(v for v in ds if v['symbol_id']==ident)
            if s is None:errors.append('Missing source '+ident);continue
            if d.get('review_group')!=g['group']:errors.append('Group mismatch '+ident)
            if not semantic.qualified(d):errors.append('Unqualified '+ident)
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'C15')
            for key,field in [('kind','source_kind'),('profile','source_profile'),('span_start','source_span_start'),('span_length','source_span_length'),('body_sha256','body_sha256'),('enclosing_symbol_id','source_enclosing_symbol_id')]:
                if s[key]!=d.get(field):errors.append('Stale '+key+' '+ident)
            if d.get('source_sha256')!=sha(ROOT/s['path']):errors.append('Source hash '+ident)
            if d.get('registration_class')!='explicit non-export' or json.loads(d.get('registration_evidence','[]')):errors.append('False export '+ident)
            if d.get('equivalence_scope')=='whole-method' and (s['name'] not in ('Compute','ComputeGrouped') or 'Excel' in s['containing_type']):errors.append('False whole renderer equivalence '+ident)
            if d.get('disposition','').startswith('extract-to-') and (not d.get('candidate_operation_id') or not d.get('owner_role') or not d.get('future_batch')):errors.append('Unowned operation '+ident)
    return errors

def preservation(history=True):
    b=read(EVIDENCE/'before.json')
    assert git('branch','--show-current')==b['branch'] and git('rev-parse','HEAD')==git('rev-parse','main')==b['head']
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    old=(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
    now=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
    assert len(json.loads(old))==1662 and not prior.prior_decision_errors(now,old)
    sets=[('tracked',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('compiled',catalog.load(OUT,'files.csv'),ROOT),('start snapshots',b['original_artifacts'],EVIDENCE/'original')]
    for label,records,base in sets:
        assert all((base/a['path']).is_file() and sha(base/a['path'])==a['sha256'] for a in records),label
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    historical=[a for a in b['original_artifacts'] if '/baseline/' in a['path'] or a['path'].rsplit('/',1)[-1].startswith(tuple('continuation'+str(i).zfill(2) for i in range(2,15)))]
    assert all(sha(ROOT/a['path'])==a['sha256'] for a in historical),'Historical checkpoint changed'
    raw=read(EVIDENCE/'prior-raw-evidence-hashes.json')['files']
    if history:assert not prior.raw_history_errors(ROOT,raw)
    return dict(branch=b['branch'],head=b['head'],prior_ordered_literal_decisions=1662,tracked=2109,gate_c=1543,compiled=171,baseline=52,start_indexed_paths=len(b['original_artifacts']),prior_raw_files=len(raw),raw_history_checked=history,status='exact bytes preserved')

def snapshot_errors(decisions):
    errors=[];current={d['symbol_id']:d for d in decisions};ref={r['symbol_id']:r for r in read(EVIDENCE/'source-refinements.json')}
    for g in GROUPS:
        snap=read(EVIDENCE/('group-'+g['group']+'-member-evidence.json'))
        if {d['symbol_id'] for d in snap['decisions']}!={s['symbol_id'] for s in g['records']}:errors.append('Snapshot ID drift')
        for d in snap['decisions']:
            r=ref.get(d['symbol_id']);expected=r['after'] if r else d
            if r and r['before']!=d:errors.append('Unbound source refinement')
            if current.get(d['symbol_id'])!=expected:errors.append('Unrecorded decision change')
    return errors

def comparison_errors(rows):
    errors=[];pp=semantic.portable_sources()
    for row in rows:
        p=pp.get(row.get('symbol_id'))
        if p is None:errors.append('Missing portable declaration');continue
        if any(row.get(k)!=p.get(k) for k in ('path','signature','containing_type','span_start','span_length','start_line','end_line','body_sha256')):errors.append('Stale portable identity')
        if row.get('source_sha256')!=sha(ROOT/p['path']) or row.get('body_excerpt')!=excerpt(p):errors.append('Stale portable body/source')
    return errors

def operations(selected):
    old={r['operation_id'] for r in catalog.load(OUT,'continuation14-candidate-operations.csv')}
    groups=collections.defaultdict(list);members=[]
    for d in selected:
        op=d.get('candidate_operation_id','');candidate=d['disposition'].startswith('extract-to-')
        assert candidate==bool(op)
        member_class='candidate' if candidate else 'already-ported whole numeric forwarder' if d['equivalence_scope']=='whole-method' else 'compatibility facade/accessor' if d['disposition']=='compatibility-facade-to-preserve' else 'retained host'
        members.append(dict(symbol_id=d['symbol_id'],operation_id=op,owner=d['owner_role'],destination=d['destination'],future_batch=d['future_batch'],disposition=d['disposition'],member_class=member_class,related_prior_operation_ids=d.get('related_operation_ids','')))
        if op:groups[op].append(d)
    rows=[]
    for op,ds in sorted(groups.items()):
        assert op in old or op.startswith('C15-'),'Unknown prior operation'
        rows.append(dict(operation_id=op,member_rows=len(ds),destination='; '.join(sorted({d['destination'] for d in ds})),approved_batch='; '.join(sorted({d['future_batch'] for d in ds})),source_ids='; '.join(d['symbol_id'] for d in ds),owner='; '.join(sorted({d['owner_role'] for d in ds})),required_tests='; '.join(d['proposed_test'] for d in ds),status='EXTENSION OF EXISTING C14 OPERATION' if op in old else 'NEW PROVISIONAL GROUP',freeze='NOT FROZEN; not a project-wide migration count'))
    return rows,members

def comparison_rows(selected):
    portable=semantic.portable_sources();values={}
    for d in selected:
        for field in ('portable_equivalent_pins','compared_portable_declarations'):
            for target in json.loads(d[field]):
                p=portable[target['symbol_id']];key=(d['symbol_id'],p['symbol_id'],field)
                values[key]={**p,'source_symbol_id':d['symbol_id'],'equivalence_scope':d['equivalence_scope'] if field=='portable_equivalent_pins' else 'none verified; comparison only','body_excerpt':excerpt(p),'source_sha256':sha(ROOT/p['path']),'comparison':d['equivalent_evidence'] if field=='portable_equivalent_pins' else d['source_purpose']}
    common=next(p for p in portable.values() if p['path']=='BESHStat.Excel.Common/src/Output/OutputDocument.vb' and p['containing_type'].endswith('.OutputPlotArtifact') and p['name']=='.ctor')
    for d in selected:
        s=SOURCE[d['symbol_id']]
        if s['name'] in ('AddChart','AddBoxPlot'):
            values[(d['symbol_id'],common['symbol_id'],'Common comparison only')]={**common,'source_symbol_id':d['symbol_id'],'equivalence_scope':'none verified; comparison only','body_excerpt':excerpt(common),'source_sha256':sha(ROOT/common['path']),'comparison':'OutputPlotArtifact validates logical reference key and purpose, stores description/default empty and caption. It contains no plot data, series geometry, axes, styles, executable handler or renderer; not an equivalent of this native chart. No existing Common whole rendering/request pipeline is claimed.'}
    return list(values.values())

def alignment_errors(rows):
    errors=[]
    if len(rows)<10 or len({r.get('alignment_id') for r in rows})!=len(rows):errors.append('Missing/duplicate alignment rows')
    for r in rows:
        if not all(r.get(k) for k in ('observed_behavior','future_question','owner','approved_batch','future_fixture','source_pins','classification')):errors.append('Incomplete alignment')
        for p,h in json.loads(r.get('source_pins','{}')).items():
            if sha(ROOT/p)!=h:errors.append('Stale alignment source')
        if r.get('runtime_status')!='NOT RUN; source comparison only':errors.append('False runtime parity')
    return errors

def fixture_errors(rows):
    errors=[]
    if len(rows)!=9 or len({r.get('fixture_id') for r in rows})!=9:errors.append('Missing/duplicate existing fixture')
    for r in rows:
        p=ROOT/r['source'];lines=p.read_text(encoding='utf-8-sig').splitlines()
        if sha(p)!=r.get('source_sha256') or '\n'.join(lines[int(r['start_line'])-1:int(r['end_line'])])!=r.get('declaration_and_body'):errors.append('Stale fixture declaration/body')
    return errors

def validation():
    runs=read(EVIDENCE/'validation-runs.json') if (EVIDENCE/'validation-runs.json').exists() else []
    checks=read(EVIDENCE/'final-checks.json') if (EVIDENCE/'final-checks.json').exists() else []
    managed=[];languages=[]
    import continuation03_report as logreader,re
    for config in ('Debug','Release'):
        for name in ('Core.trx','Common.trx','OfficeAddin.trx','OfficeExecution.trx','Legacy.trx','OfficeAddin-'+config+'.trx'):
            p=EVIDENCE/config/name
            if p.exists():
                doc=ET.parse(p);ids=[{k:r.get(k) for k in ('testId','testName','outcome')} for r in doc.findall('.//{*}UnitTestResult')]
                managed.append(dict(configuration=config,suite=name,distinct_full_suite=name!='OfficeAddin-'+config+'.trx',**doc.find('.//{*}Counters').attrib,identities=ids,sha256=sha(p),path=p.relative_to(ROOT).as_posix()))
        for kind in ('full','foundation'):
            p=EVIDENCE/config/(kind+'.log')
            if p.exists():
                text=logreader.text(p);languages.append(dict(configuration=config,kind=kind,path=p.relative_to(ROOT).as_posix(),sha256=sha(p),python_test_counts=re.findall(r'Ran (\d+) tests? in',text),python_outcome_summaries=re.findall(r'(?m)^(?:OK(?: \([^\n]*\))?|FAILED \([^\n]*\))\s*$',text),python_test_identities=[dict(name=n,qualified=''.join(q.split())) for n,q in re.findall(r'\b(test_\w+)\s+\(([^)]+)\)\s+\.\.\.',text)],node_tests=re.findall(r'(?:# |\u2139 )tests (\d+)',text),node_pass=re.findall(r'(?:# |\u2139 )pass (\d+)',text),node_fail=re.findall(r'(?:# |\u2139 )fail (\d+)',text),node_skipped=re.findall(r'(?:# |\u2139 )skipped (\d+)',text),node_test_identities=re.findall(r'^\s*\u2714 (.+?) \([\d.]+ms\)',text,re.M)))
    negative={}
    p=EVIDENCE/'negative-reconciliation.log'
    if p.exists():
        text=logreader.text(p);negative=dict(log=p.relative_to(ROOT).as_posix(),sha256=sha(p),reported_counts=re.findall(r'Ran (\d+) tests? in',text),failed=re.findall(r'(?:FAIL|ERROR): (test_\w+)',text),passed=bool(re.search(r'\nOK\s*$',text)),identities=sorted(set(re.findall(r'(test_\w+)\s+\(',text))))
    first=EVIDENCE/'first-attempt/negative-reconciliation.log'
    if first.exists():
        text=logreader.text(first);negative['initial_entire_suite']=dict(log=first.relative_to(ROOT).as_posix(),sha256=sha(first),reported_counts=re.findall(r'Ran (\d+) tests? in',text),passed=bool(re.search(r'\nOK\s*$',text)),status='Initial successful full suite retained; final entire suite rerun after operation-member label clarification. Repeated identities not summed.')
    second=EVIDENCE/'second-attempt/negative-reconciliation.log'
    if second.exists():
        text=logreader.text(second);negative['second_entire_suite']=dict(log=second.relative_to(ROOT).as_posix(),sha256=sha(second),reported_counts=re.findall(r'Ran (\d+) tests? in',text),passed=bool(re.search(r'\nOK\s*$',text)),status='Successful label-correction suite retained. Final entire suite also includes exact embedded-CRLF roundtrip/mutation test; do not sum repeated identities.')
    return dict(initial_commands=read(EVIDENCE/'initial-commands.json'),runs=runs,checks=checks,managed=managed,python_node=languages,negative=negative,managed_identity_analysis=read(EVIDENCE/'managed-identity-analysis.json') if (EVIDENCE/'managed-identity-analysis.json').exists() else 'Pending completed product suites',initial_authoring_failures=[dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p),details=read(p)) for p in EVIDENCE.glob('*failure.json')],source_refinements=dict(path='TestResults/04A-01-continuation-15/source-refinements.json',sha256=sha(EVIDENCE/'source-refinements.json'),records=len(read(EVIDENCE/'source-refinements.json'))),transient_generated_copies=[dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p)) for c in ('Debug','Release') for p in (EVIDENCE/c).rglob('*.vb')],python_executable=sys.executable,not_run=['native Excel/COM/XLL/GUI/workbooks/shape rendering','Mac/authenticated Office','new R/scientific goldens','network/install/sign/deploy','04A.02 or migration implementation'])

def main():
    ss=catalog.load(OUT,'symbols.csv');ds=semantic.read_store('review-decisions.json');errors=scope_errors(ss,ds)+snapshot_errors(ds);assert not errors,errors
    protection=preservation(history=False)
    import continuation03_report as logreader
    guard_log=EVIDENCE/'continuation15-preservation.log';guard=read(EVIDENCE/'final-checks.json')
    guard_record=next(r for r in guard if r['id']=='continuation15-preservation');assert guard_record['exit_code']==0 and guard_record['sha256']==sha(guard_log)
    full=json.loads(logreader.text(guard_log));assert full['raw_history_checked'] and full['prior_raw_files']==39704
    protection['raw_history_checked']=True;protection['full_history_evidence']=dict(path=guard_log.relative_to(ROOT).as_posix(),sha256=sha(guard_log),result=full)
    selected=[]
    for d in ds:
        if d.get('checkpoint_id')=='04A.01-continuation-15':s=next(s for s in ss if s['symbol_id']==d['symbol_id']);selected.append({**s,**d})
    sem=read(OUT/'semantic-review-coverage.json');review=read(OUT/'review-coverage.json')
    assert sem['qualified_source_reviews']==1894 and sem['unreviewed_for_acceptance']==1972
    write('continuation15-member-reviews.csv',catalog.render(selected))
    current={d['symbol_id']:d for d in ds}
    candidates=[{**s,**current[s['symbol_id']]} for s in ss if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in prior.FIELDS} for s in candidates]))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in ss)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=x,allocation_evidence=e,records=n) for (f,r,d,x,e),n in sorted(reasons.items())]))
    ops,members=operations(selected);write('continuation15-candidate-operations.csv',catalog.render(ops));write('continuation15-operation-members.csv',catalog.render(members))
    write('continuation15-portable-comparisons.csv',catalog.render(comparison_rows(selected)))
    aligns=read(EVIDENCE/'alignment-reviewed.json');assert not alignment_errors(aligns);write('continuation15-gui-udf-alignment.csv',catalog.render(aligns))
    fixtures=read(EVIDENCE/'fixtures-reviewed.json');assert not fixture_errors(fixtures);write('continuation15-fixture-crosswalk.csv',catalog.render(fixtures))
    refinements=read(EVIDENCE/'source-refinements.json') if (EVIDENCE/'source-refinements.json').exists() else []
    for g in GROUPS:
        group=g['group'];p=EVIDENCE/('group-'+group+'-member-evidence.json');snap=read(p)
        emit('continuation15-group-'+group+'.json',dict(group=group,exact_target_ids=[s['symbol_id'] for s in g['records']],source_records=g['records'],immutable_original_snapshot=dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p)),original_decisions=snap['decisions'],final_decisions=[d for d in selected if d['review_group']==group],refinements=[r for r in refinements if r['before']['review_group']==group],progress=read(EVIDENCE/('progress-'+group+'.json')),checks=read(EVIDENCE/('group-'+group+'-checks.json')),protection=read(EVIDENCE/('group-'+group+'-protection.json'))))
    emit('continuation15-count-deltas.json',dict(before=read(EVIDENCE/'before.json')['starting_review'],after=sem,current_investigations=review['member_investigations'],unresolved_edges=review['unresolved_edge_rows'],control_contracts=sem['qualified_full_control_contracts'],candidate_member_rows=sum(m['member_class']=='candidate' for m in members),deduplicated_operations=len(ops),existing_extensions=[o['operation_id'] for o in ops if o['status'].startswith('EXTENSION')]))
    emit('continuation15-preservation.json',protection);emit('continuation15-validation.json',validation())
    write('continuation15.md',(EVIDENCE/'report-text.md').read_text(encoding='utf-8'))
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}|{p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}|{'docs/architecture/WindowsMigrationLedger.md',PREFIX+'continuation15-artifacts.csv'}
    assert not any('__pycache__' in p or p.endswith('.pyc') for p in paths)
    old={r['path']:r['sha256'] for r in read(EVIDENCE/'before.json')['original_artifacts']}
    write('continuation15-artifacts.csv',catalog.render([dict(path=p,change_from_continuation15_start='unchanged' if p in old and sha(ROOT/p)==old[p] else 'modified' if p in old else 'added',tracking='ignored; future individual force-add' if p.startswith(PREFIX) else 'visible audit artifact',hash_authority='index.json exact SHA256/bytes; self excluded') for p in sorted(paths)]))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith(PREFIX) else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    if CHECK:
        import continuation03_report as indexcheck
        assert not indexcheck.verify_index(ROOT,read(OUT/'index.json'),sorted(paths))
        assert not prior.tracking_errors(read(OUT/'index.json'),sorted(paths),(OUT/'future-staging.txt').read_text(encoding='utf-8'))
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=len(selected),overall=sem['qualified_source_reviews'],remaining=sem['unreviewed_for_acceptance'],candidate_rows=sum(m['member_class']=='candidate' for m in members),operations=len(ops),paths=len(paths))))

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--guard',action='store_true');parser.add_argument('--check',action='store_true');args=parser.parse_args();CHECK=args.check
    if args.guard:print(json.dumps(preservation()))
    else:main()
