"""C19 shared-dialog evidence projection. Does not author/qualify reviews or run historical writers."""
from __future__ import annotations
import argparse,collections,datetime,hashlib,json
import catalog,semantic,amendment as a,continuation19_controls as controls_guard
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-19'
GROUPS=a.read(EVIDENCE/'targets.json')
SOURCE={s['symbol_id']:s for g in GROUPS for s in g['records']}
CONTROL_SCOPE={c['control_id']:c for g in GROUPS for c in g['controls']}
PREFIX='docs/architecture/WindowsMigrationLedger/'
NEW_OPS={'C19-A01-update-version-policy','C19-A02-settings-form-request','C19-B01-shared-dialog-acquisition','C19-B02-one-input-request-projection','C19-C01-paired-option-state','C19-D01-export-format-size-policy'}
CHECK=False
def write(name,value):
    raw=(value.rstrip()+'\n').encode('utf-8');p=OUT/name
    if CHECK:assert p.is_file() and p.read_bytes()==raw,'C19 report byte drift: '+name
    else:p.write_bytes(raw)
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def literal_objects(text):
    decoder=json.JSONDecoder();pos=text.index('[')+1;out=[]
    while True:
        while text[pos] in ' \t\r\n,':pos+=1
        if text[pos]==']':return out
        start=pos;_,pos=decoder.raw_decode(text,pos);out.append(text[start:pos])
def prefix_errors(current,original):
    before=literal_objects(original);after=literal_objects(current)
    return [] if after[:len(before)]==before else ['Prior literal objects/order changed']
def excerpt(s):
    raw=(ROOT/s['path']).read_bytes().decode('utf-8-sig',errors='replace').encode('utf-16-le');i=int(s['span_start']);n=int(s['span_length']);return raw[2*i:2*(i+n)].decode('utf-16-le')
def scope_errors(symbols,decisions,complete=True):
    ss={s['symbol_id']:s for s in symbols};selected=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-19'];ids=[d['symbol_id'] for d in selected];errors=[]
    if len(ids)!=len(set(ids)) or not set(ids)<=set(SOURCE) or (complete and set(ids)!=set(SOURCE)):return ['Missing/duplicate/foreign C19 member IDs']
    for d in selected:
        ident=d['symbol_id'];s=ss.get(ident);frozen=SOURCE[ident]
        if s is None:errors.append('Missing C19 source '+ident);continue
        if not semantic.qualified(d):errors.append('Unqualified C19 member '+ident)
        errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'C19')
        for key,field in [('kind','source_kind'),('enclosing_symbol_id','source_enclosing_symbol_id'),('profile','source_profile'),('span_start','source_span_start'),('span_length','source_span_length'),('body_sha256','body_sha256'),('start_line','source_declared_start_line'),('end_line','source_declared_end_line'),('body_start_line','source_body_start_line'),('body_end_line','source_body_end_line')]:
            if s[key]!=frozen[key] or d.get(field)!=s[key]:errors.append('Stale C19 '+key+' '+ident)
        if d.get('source_sha256')!=a.sha(ROOT/s['path']) or d.get('source_excerpt')!=excerpt(s):errors.append('Stale C19 raw/UTF16 excerpt '+ident)
        if d.get('equivalence_scope')!='none':errors.append('Unverified whole/helper GUI forwarding claim '+ident)
        if d.get('registration_class')!='explicit non-export' or json.loads(d.get('registration_evidence','[]')):errors.append('False C19 UDF registration '+ident)
        if d['disposition'].startswith('extract-to-') and not all(d.get(k) for k in ('candidate_operation_id','owner_role','future_batch')):errors.append('Unowned C19 operation '+ident)
    return errors
def snapshot_errors(decisions):
    current={d['symbol_id']:d for d in decisions};changes={r['symbol_id']:r for r in a.read(EVIDENCE/'member-source-refinements.json')};errors=[]
    for g in GROUPS:
        snap=a.read(EVIDENCE/('group-'+g['group']+'-member-evidence.json'))['decisions']
        if {d['symbol_id'] for d in snap}!={s['symbol_id'] for s in g['records']}:errors.append('C19 member snapshot IDs')
        for old in snap:
            change=changes.get(old['symbol_id'])
            if change and change['before']!=old:errors.append('Unbound C19 member refinement')
            if current.get(old['symbol_id'])!=(change['after'] if change else old):errors.append('Unrecorded C19 member edit')
    return errors
def operations(selected):
    old={r['operation_id'] for r in catalog.load(OUT,'continuation07-candidate-operations.csv')};grouped=collections.defaultdict(list);members=[]
    for d in selected:
        op=d.get('candidate_operation_id','');candidate=d['disposition'].startswith('extract-to-')
        assert candidate==bool(op),'Candidate/operation mismatch'
        if candidate:assert op in NEW_OPS|old,'Unknown C19 operation';grouped[op].append(d)
        members.append(dict(symbol_id=d['symbol_id'],operation_id=op,disposition=d['disposition'],destination=d['destination'],owner=d['owner_role'],approved_batch=d['future_batch'],member_class='candidate' if candidate else 'host-only'))
    rows=[dict(operation_id=op,member_rows=len(ds),destination='; '.join(sorted({d['destination'] for d in ds})),approved_batch='; '.join(sorted({d['future_batch'] for d in ds})),owner='; '.join(sorted({d['owner_role'] for d in ds})),status='EXTENSION OF EXISTING OPERATION' if op in old else 'NEW PROVISIONAL OPERATION',freeze='NOT FROZEN; no project-wide total',source_ids='; '.join(d['symbol_id'] for d in ds)) for op,ds in sorted(grouped.items())]
    # Existing agreement output projection retains its approved owner batch.
    assert all(o['approved_batch']=='04A.28' for o in rows if o['operation_id']=='C07-C05-agreement-output-projection')
    return rows,members
def readiness_errors(value):
    if value.get('status') in ('READY','READY FOR ACCEPTANCE') and (value.get('unreviewed_for_acceptance') or value.get('unresolved_edge_rows') or value.get('qualified_full_control_contracts',0)<2056):return ['Incomplete UI/control/wiring falsely READY']
    return []
def preservation(history=False):
    started=datetime.datetime.now(datetime.timezone.utc).isoformat();b=a.read(EVIDENCE/'before.json')
    assert a.git('branch','--show-current')==b['branch'] and a.git('rev-parse','HEAD')==a.git('rev-parse','main')==b['head']
    assert not a.git('diff','--name-only') and not a.git('diff','--cached','--name-only')
    counts={}
    for name,n in [('review-decisions',2494),('control-review-decisions',54),('wiring-review-decisions',134)]:
        old=(EVIDENCE/'original/scripts/windows-migration-audit'/(name+'.json')).read_bytes().decode('utf-8-sig');now=(semantic.TOOLING/(name+'.json')).read_bytes().decode('utf-8-sig')
        assert len(json.loads(old))==n and not prefix_errors(now,old),name;counts[name]=n
    for label,records,base in [('tracked',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('compiled',b['compiled'],ROOT),('original index/artifact copies',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/r['path']).is_file() and (base/r['path']).stat().st_size==r.get('bytes',(base/r['path']).stat().st_size) and a.sha(base/r['path'])==r['sha256'] for r in records),label
    assert all(a.sha(ROOT/r['retained_path'])==r['sha256'] for r in b['baseline']['entries'])
    historic=[r for r in b['original_artifacts'] if '/baseline/' in r['path'] or r['path'].rsplit('/',1)[-1].startswith(tuple('continuation'+str(i).zfill(2) for i in range(1,19)))]
    assert all(a.sha(ROOT/r['path'])==r['sha256'] for r in historic),'Historical C01-C18 report/tooling bytes'
    frozen=a.read(EVIDENCE/'decision-freeze.json')
    for name,digest in frozen['stores'].items():assert a.sha(semantic.TOOLING/name)==digest,'C19 frozen store drift'
    for r in frozen['group_files']:assert a.sha(ROOT/r['path'])==r['sha256'],'C19 immutable group drift'
    raw=a.read(EVIDENCE/'prior-raw-evidence-hashes.json')['files']
    if history:
        errors=[]
        for r in raw:
            p=ROOT/r['path']
            if not p.is_file() or p.stat().st_size!=r['bytes'] or a.sha(p)!=r['sha256']:errors.append(r['path'])
        assert not errors,errors
    return dict(status='PASS exact protected/source/history bytes',branch=b['branch'],head=b['head'],prior_literal_objects=counts,tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(b['compiled']),baseline=len(b['baseline']['entries']),start_indexed_plus_index=len(b['original_artifacts']),raw_history_checked=history,raw_expected=len(raw),raw_checked=len(raw) if history else 0,scan_started_utc=started,scan_completed_utc=datetime.datetime.now(datetime.timezone.utc).isoformat())
def validation():
    import re,sys,xml.etree.ElementTree as ET
    import continuation03_report as logreader
    managed=[];languages=[]
    for config in ('Debug','Release'):
        for p in sorted((EVIDENCE/config).glob('*.trx')):
            doc=ET.parse(p)
            managed.append(dict(configuration=config,suite=p.name,distinct_full_suite=p.name in ('Core.trx','Common.trx','OfficeAddin.trx','OfficeExecution.trx','Legacy.trx'),counters=doc.find('.//{*}Counters').attrib,path=p.relative_to(ROOT).as_posix(),sha256=a.sha(p)))
        for kind in ('full','foundation'):
            p=EVIDENCE/config/(kind+'.log');content=logreader.text(p)
            languages.append(dict(configuration=config,kind=kind,path=p.relative_to(ROOT).as_posix(),sha256=a.sha(p),python_test_counts=re.findall(r'Ran (\d+) tests? in',content),node_tests=re.findall(r'(?:# |\u2139 )tests (\d+)',content),node_pass=re.findall(r'(?:# |\u2139 )pass (\d+)',content),node_fail=re.findall(r'(?:# |\u2139 )fail (\d+)',content),node_skipped=re.findall(r'(?:# |\u2139 )skipped (\d+)',content)))
    p=EVIDENCE/'negative-reconciliation.log';content=logreader.text(p)
    negative=dict(path=p.relative_to(ROOT).as_posix(),sha256=a.sha(p),reported_counts=re.findall(r'Ran (\d+) tests? in',content),passed=bool(re.search(r'\nOK\s*$',content)),failed=re.findall(r'(?:FAIL|ERROR): (test_\w+)',content),identities=sorted(set(re.findall(r'(test_\w+)\s+\(',content))))
    v=dict(initial_commands=a.read(EVIDENCE/'initial-commands.json'),runs=a.read(EVIDENCE/'validation-runs.json'),checks=a.read(EVIDENCE/'final-checks.json'),managed=managed,managed_identity_analysis=a.read(EVIDENCE/'managed-identity-analysis.json'),python_node=languages,negative=negative,python_executable=sys.executable,transient_generated_copies=[dict(path=p.relative_to(ROOT).as_posix(),sha256=a.sha(p)) for config in ('Debug','Release') for p in (EVIDENCE/config).rglob('*.vb')],not_run=['native Excel/COM/XLL/GUI/workbooks/chart export','Mac/authenticated Office','HTTP/network/update links','user settings/logs/workbooks','new scientific or R golden capture','installer/signing/deployment','04A.02 or product implementation'])
    v['source_refinements']=dict(path=(EVIDENCE/'member-source-refinements.json').relative_to(ROOT).as_posix(),sha256=a.sha(EVIDENCE/'member-source-refinements.json'),records=len(a.read(EVIDENCE/'member-source-refinements.json')))
    v['authoring_corrections']=a.read(EVIDENCE/'authoring-corrections.json')
    v['group_checks']={g['group']:a.read(ROOT/a.read(EVIDENCE/('progress-'+g['group']+'.json'))['checks_manifest']) for g in GROUPS}
    return v
def main():
    ss=catalog.load(OUT,'symbols.csv');ds=semantic.read_store('review-decisions.json');cs=semantic.read_store('control-review-decisions.json');edges=catalog.load(OUT,'ui-wiring.csv');cc=catalog.load(OUT,'ui-controls.csv')
    assert not scope_errors(ss,ds)+snapshot_errors(ds)+controls_guard.verify(cs,ss,cc,edges)
    protection=preservation();protection.pop('scan_started_utc');protection.pop('scan_completed_utc')
    final=a.read(EVIDENCE/'final-checks.json');full=next(r for r in final if r['id']=='continuation19-preservation');assert full['exit_code']==0 and a.sha(ROOT/full['log'])==full['sha256']
    import continuation03_report as logreader
    guard=json.loads(logreader.text(ROOT/full['log']));assert guard['raw_history_checked'] and guard['raw_checked']==guard['raw_expected'];protection['full_historical_scan']=dict(command=full,result=guard)
    sem=a.read(OUT/'semantic-review-coverage.json');coverage=a.read(OUT/'review-coverage.json');assert sem['qualified_source_reviews']==2616 and sem['unreviewed_for_acceptance']==1250
    assert not readiness_errors({**sem,**coverage})
    selected=[{**SOURCE[d['symbol_id']],**d} for d in ds if d.get('checkpoint_id')=='04A.01-continuation-19'];ops,members=operations(selected)
    write('continuation19-member-reviews.csv',catalog.render(selected));write('continuation19-candidate-operations.csv',catalog.render(ops));write('continuation19-operation-members.csv',catalog.render(members))
    oldcontrols={d['control_id']:d for d in a.read(EVIDENCE/'original/scripts/windows-migration-audit/control-review-decisions.json')};control_rows=[]
    for g in GROUPS:
        for d in a.read(EVIDENCE/('group-'+g['group']+'-control-evidence.json'))['controls']:
            ident=d['control_id'];row={**CONTROL_SCOPE[ident],**d}
            if ident in oldcontrols:
                row.update(oldcontrols[ident]);row['intent_completeness']=d['intent_completeness'];row['qualification_reason']=d['qualification_reason'];row['review_group']=g['group']
            control_rows.append(row)
        progress=a.read(EVIDENCE/('progress-'+g['group']+'.json'))
        emit('continuation19-group-'+g['group']+'.json',dict(group=g['group'],exact_member_ids=[s['symbol_id'] for s in g['records']],exact_control_ids=[c['control_id'] for c in g['controls']],progress=progress,checks=a.read(ROOT/progress['checks_manifest']),member_snapshot_sha256=a.sha(EVIDENCE/('group-'+g['group']+'-member-evidence.json')),control_snapshot_sha256=a.sha(EVIDENCE/('group-'+g['group']+'-control-evidence.json'))))
    assert len(control_rows)==160 and sum(r.get('review_contract')=='04A.01-control-v1' for r in control_rows)==sem['qualified_full_control_contracts']
    write('continuation19-control-contracts.csv',catalog.render(control_rows))
    scope_paths={s['path'] for s in SOURCE.values()};wire=[{**e,'source_edge_sha256':hashlib.sha256('|'.join(e[k] for k in ('path','kind','event_or_control','target','start_line','end_line','owner_id')).encode()).hexdigest()} for e in edges if e['path'] in scope_paths]
    orphan=next(d for d in selected if d['path'].endswith('Ui99ExportChart.vb') and d['name']=='btRefresh_Click');wire.append(dict(edge_id='',path=orphan['path'],kind='source observation; NOT an invented census edge',target_status='UNPROVEN external/reflection wiring; no btRefresh declaration/Handles/AddHandler/caller located',source_symbol_id=orphan['symbol_id'],body_sha256=orphan['body_sha256'],verification=orphan['proposed_test']))
    write('continuation19-wiring-crosswalk.csv',catalog.render(wire))
    portable=semantic.portable_sources();comparisons=[]
    for d in selected:
        for p in json.loads(d['compared_portable_declarations']):
            actual=portable[p['symbol_id']];comparisons.append({**actual,'source_symbol_id':d['symbol_id'],'equivalence_scope':'comparison-only; no whole GUI wrapper equivalent','source_sha256':a.sha(ROOT/actual['path']),'source_excerpt':excerpt(actual),'comparison':d['source_purpose']})
    write('continuation19-portable-comparisons.csv',catalog.render(comparisons))
    aligns=a.read(EVIDENCE/'alignment-reviewed.json');write('continuation19-gui-udf-alignment.csv',catalog.render(aligns))
    fixture_rows=a.read(EVIDENCE/'fixtures-reviewed.json');write('continuation19-fixture-crosswalk.csv',catalog.render(fixture_rows))
    counts=dict(before=a.read(EVIDENCE/'before.json')['starting_review'],after=sem,before_remaining=a.read(EVIDENCE/'before.json')['starting_coverage'],after_remaining=coverage,scope_members=len(selected),scope_controls=len(control_rows),new_complete_controls=sum(r['intent_completeness']=='qualified' for r in control_rows),preserved_complete_controls=sum(r['intent_completeness']=='preserved-qualified' for r in control_rows),deferred_control_ids=[r['control_id'] for r in control_rows if r['intent_completeness']=='deferred'],candidate_member_rows=sum(m['member_class']=='candidate' for m in members),distinct_new_provisional_operations=sum(o['status']=='NEW PROVISIONAL OPERATION' for o in ops),existing_operation_extensions=sum(o['status'].startswith('EXTENSION') for o in ops),host_only_member_rows=sum(m['member_class']=='host-only' for m in members))
    emit('continuation19-count-deltas.json',counts);emit('continuation19-preservation.json',protection);emit('continuation19-validation.json',validation())
    write('continuation19.md',(EVIDENCE/'report-text.md').read_text(encoding='utf-8'))
    current={d['symbol_id']:d for d in ds};candidate_rows=[{**s,**current[s['symbol_id']]} for s in ss if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    import continuation14_report as field_authority
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in field_authority.FIELDS} for s in candidate_rows]))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in ss)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=b,allocation_evidence=e,records=n) for (f,r,d,b,e),n in sorted(reasons.items())]))
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}|{p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}|{'docs/architecture/WindowsMigrationLedger.md',PREFIX+'continuation19-artifacts.csv'}
    assert not any('__pycache__' in p or '/bin/' in p or '/obj/' in p or p.endswith('.pyc') for p in paths)
    before={r['path']:r['sha256'] for r in a.read(EVIDENCE/'before.json')['original_artifacts']}
    write('continuation19-artifacts.csv',catalog.render([dict(path=p,change_from_C18='unchanged' if p in before and a.sha(ROOT/p)==before[p] else 'modified' if p in before else 'added',tracking='ignored; individual future force-add' if p.startswith(PREFIX) else 'visible audit tooling',hash_authority='index.json SHA/size; index itself excluded') for p in sorted(paths)]))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith(PREFIX) else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    if CHECK:
        assert not logreader.verify_index(ROOT,a.read(OUT/'index.json'),sorted(paths))
    print(json.dumps(dict(status='C19 source checkpoint complete; 04A.01 NOT READY',members=len(selected),overall=2616,UI=297,remaining=1250,new_controls=counts['new_complete_controls'],complete_controls=sem['qualified_full_control_contracts'],deferred_controls=len(counts['deferred_control_ids']),unresolved_wiring=coverage['unresolved_edge_rows'],candidate_rows=counts['candidate_member_rows'],operations=len(ops),paths=len(paths))))
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--check',action='store_true');parser.add_argument('--guard',action='store_true');args=parser.parse_args();CHECK=args.check
    if args.guard:print(json.dumps(preservation(history=True)))
    else:main()
