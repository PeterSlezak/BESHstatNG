"""C23 audit evidence checks/projections. --check and --guard never write files."""
from __future__ import annotations
import argparse, collections, functools, hashlib, json, pathlib, sys
import amendment as a, catalog, semantic
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-23'
GROUPS=a.read(EVIDENCE/'targets.json')
SOURCE={s['symbol_id']:s for g in GROUPS for s in g['records']}
CONTROLS={c['control_id']:c for g in GROUPS for c in g['controls']}
IDENTITY=('symbol_id','path','start_line','end_line','signature','containing_type','body_sha256')
EDGE=('edge_id','path','kind','event_or_control','target','start_line','end_line','owner_id')
CONTROL=('control_id','path','line','type_declaration','initialization_defaults','handles_edges')

@functools.lru_cache(maxsize=3)
def frozen_group(group):
    return a.read(EVIDENCE/('group-'+group+'-frozen-source.json'))

def excerpt(s):
    raw=(ROOT/s['path']).read_bytes().decode('utf-8-sig',errors='replace').encode('utf-16-le')
    i,n=int(s['span_start']),int(s['span_length'])
    return raw[i*2:(i+n)*2].decode('utf-16-le')

def literals(raw):
    text=raw.decode('utf-8-sig'); dec=json.JSONDecoder();pos=text.index('[')+1;out=[]
    while True:
        while text[pos] in ' \t\r\n,':pos+=1
        if text[pos]==']':return out
        begin=pos;_,pos=dec.raw_decode(text,pos);out.append(text[begin:pos].encode())

def checked_files(records,base=ROOT):
    errors=[]
    for r in records:
        p=base/r['path']
        if not p.is_file() or p.stat().st_size!=r.get('bytes',p.stat().st_size) or a.sha(p)!=r['sha256']:
            errors.append('Missing/size/hash drift: '+r['path'])
    return errors

def member_errors(rows,expected=None,live=None):
    expected=SOURCE if expected is None else expected
    live={s['symbol_id']:s for s in catalog.load(OUT,'symbols.csv')} if live is None else live
    ids=[d.get('symbol_id') for d in rows];errors=[]
    if len(ids)!=len(set(ids)) or set(ids)!=set(expected):return ['Missing/duplicate/foreign C23 member IDs']
    for d in rows:
        s=live.get(d['symbol_id']);f=expected[d['symbol_id']]
        if s is None:errors.append('Missing live source');continue
        if not semantic.qualified(d):errors.append('Unqualified member-v1')
        errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'C23')
        for k in ('kind','enclosing_symbol_id','profile','span_start','span_length','body_sha256','body_start_line','body_end_line'):
            dk={'kind':'source_kind','enclosing_symbol_id':'source_enclosing_symbol_id','profile':'source_profile','span_start':'source_span_start','span_length':'source_span_length','body_start_line':'source_body_start_line','body_end_line':'source_body_end_line'}.get(k,k)
            if d.get(dk)!=s.get(k) or f.get(k)!=s.get(k):errors.append('Stale source boundary/body '+k)
        if d.get('source_sha256')!=a.sha(ROOT/s['path']) or d.get('source_excerpt')!=excerpt(s):errors.append('Stale raw file/UTF16 excerpt')
        comp=json.loads(d.get('compared_portable_declarations','[]'))
        if not comp:errors.append('Missing exact Core/Common comparison declaration')
        group=next(g['group'] for g in GROUPS if s['symbol_id'] in {x['symbol_id'] for x in g['records']})
        if d.get('review_group')!=group:errors.append('Wrong source group')
        if d.get('future_batch') not in {'04A.'+str(i).zfill(2) for i in range(2,49)}:errors.append('Unapproved extraction/follow-up batch')
        if d.get('equivalence_scope')!='none' or json.loads(d.get('portable_equivalent_pins','[]')):errors.append('Unsupported whole/helper equivalence')
        if d.get('registration_class')!='explicit non-export' or json.loads(d.get('registration_evidence','[]')):errors.append('Invented export')
        if d.get('disposition','').startswith('extract-to-') and not all(d.get(k) for k in ('candidate_operation_id','owner_role','future_batch')):errors.append('Unowned extraction operation')
    return errors

def control_errors(rows,expected=None):
    expected=CONTROLS if expected is None else expected
    ids=[d.get('control_id') for d in rows];errors=[]
    if len(ids)!=len(set(ids)) or set(ids)!=set(expected):return ['Missing/duplicate/foreign C23 control IDs']
    for d in rows:
        ident=d['control_id'];c=expected[ident]
        if json.loads(d.get('source_control_pin','{}'))!={k:c[k] for k in CONTROL}:errors.append('Stale declaration/default/Handles pin')
        group=next(g for g in GROUPS if ident in {c['control_id'] for c in g['controls']})
        f=frozen_group(group['group'])
        witnesses=json.loads(d.get('source_witnesses','[]'))
        if {s['symbol_id'] for s in witnesses}!={s['symbol_id'] for s in group['records']}:errors.append('Missing constructor/handler/outcome/lifetime source')
        for w in witnesses:
            actual=SOURCE.get(w['symbol_id'])
            if actual is None or any(w[k]!=actual[k] for k in w):errors.append('Stale source witness')
        if json.loads(d.get('binding_edge_pins','[]'))!=[{k:e[k] for k in EDGE} for e in f['wiring']]:errors.append('Missing/stale Handles/AddHandler/delegate/edge evidence')
        required={s['symbol_id'] for s in group['records'] if s['name'] in ('.ctor','InitializeComponent','Dispose')}
        if set(json.loads(d.get('required_constructor_initialization_ids','[]')))!=required:errors.append('Missing constructor/Designer/Dispose')
        if set(json.loads(d.get('control_flow_pins','[]')))!={s['symbol_id'] for s in group['records']}:errors.append('Missing validation/output/control flow')
        lifecycle=[s['symbol_id'] for s in group['records'] if 'FormClosing' in s['name'] or s['name'] in ('BeginRegressionComputation','EndRegressionComputation','BeginPsmComputation','EndPsmComputation')]
        if json.loads(d.get('form_lifecycle_method_ids','[]'))!=lifecycle:errors.append('Missing cancel/close/release lifecycle')
        if not all(d.get(k) for k in ('reviewed_intent','qualification_reason','lifetime_contract','owner_role','future_batch','verification')):errors.append('Incomplete owned decision')
        status=d.get('intent_completeness')
        if status not in ('qualified','deferred'):errors.append('Control has no individual complete/deferred decision')
        if status=='qualified' and (not semantic.control_qualified(d) or d.get('unsupported_contract_field')):errors.append('Unsupported complete-control claim')
        if status=='deferred' and (semantic.control_qualified(d) or not d.get('unsupported_contract_field')):errors.append('Unowned/false-credit deferred control')
        errors+=semantic.verify_pins([d],'C23-control')
    return errors

def snapshots(group=None,step=None):
    members=[];controls=[];wiring=[]
    for g in GROUPS:
        if group and g['group']!=group:continue
        for n in (1,2):
            if step and n!=step:continue
            for kind,key,target in [('member','decisions',members),('control','controls',controls)]:
                p=EVIDENCE/('group-'+g['group']+'-step-'+str(n)+'-'+kind+'-evidence.json')
                if not p.exists():continue
                payload=a.read(p);target.extend(payload[key])
                if kind=='control':wiring.extend(payload['new_wiring'])
    refinements=EVIDENCE/'control-dependency-refinements.json'
    if refinements.exists():
        changes={r['control_id']:r for r in a.read(refinements)}
        for d in controls:
            change=changes[d['control_id']]
            assert change['before_sha256']==hashlib.sha256(json.dumps(d,sort_keys=True,ensure_ascii=False).encode()).hexdigest(),'Unbound control refinement'
            assert set(change['changed_fields'])=={'dependency_pins','external_dependency_manifest','external_dependency_ids','review_step','form_source_snapshot'}
            assert all(change['after'].get(k)==v for k,v in d.items() if k not in change['changed_fields']),'Unapproved control content refinement'
        controls=[changes[d['control_id']]['after'] if d['control_id'] in changes else d for d in controls]
    refinements=EVIDENCE/'member-source-refinements.json'
    if refinements.exists():
        changes={r['symbol_id']:r for r in a.read(refinements)}
        for d in members:assert changes[d['symbol_id']]['before']==d,'Unbound member refinement'
        members=[changes[d['symbol_id']]['after'] for d in members]
    return members,controls,wiring

def external_errors(rows):
    all_sources={s['symbol_id']:s for s in catalog.load(OUT,'symbols.csv')}
    all_sources.update(semantic.portable_sources());errors=[];seen=set()
    for r in rows:
        sid=r['symbol_id'];s=all_sources.get(sid)
        if sid in seen or s is None:errors.append('Missing/duplicate external declaration');continue
        seen.add(sid)
        if any(r.get(k)!=s.get(k) for k in IDENTITY+('profile','span_start','span_length')):errors.append('Stale exact external declaration/body')
        if r.get('source_sha256')!=a.sha(ROOT/s['path']) or r.get('source_excerpt')!=excerpt(s):errors.append('Stale external source/default/handler body bytes')
    return errors

def prefix_errors():
    errors=[]
    for name,n in [('review-decisions',3017),('control-review-decisions',750),('wiring-review-decisions',934)]:
        old=(EVIDENCE/'original/scripts/windows-migration-audit'/(name+'.json')).read_bytes()
        new=(semantic.TOOLING/(name+'.json')).read_bytes()
        if len(literals(old))!=n or literals(new)[:n]!=literals(old):errors.append('Previous ordered literal objects changed: '+name)
    return errors

def guard(group=None,step=None,history=False):
    b=a.read(EVIDENCE/'before.json'); errors=prefix_errors()
    assert a.git('branch','--show-current')==b['branch'] and a.git('rev-parse','HEAD')==a.git('rev-parse','main')==b['head']
    assert not a.git('diff','--name-only') and not a.git('diff','--cached','--name-only')
    if not group:
        for label,rows,base in [('tracked',b['tracked'],ROOT),('gate',b['gate'],ROOT),('compiled',b['compiled'],ROOT),('start copies',b['original_artifacts'],EVIDENCE/'original')]:
            errors+=checked_files(rows,base)
    errors+=checked_files(a.read(EVIDENCE/'scope-freeze-manifest.json'))
    if (EVIDENCE/'external-dependencies.json').exists():errors+=external_errors(a.read(EVIDENCE/'external-dependencies.json'))
    for r in b['baseline']['entries'] if not group else []:
        if a.sha(ROOT/r['retained_path'])!=r['sha256']:errors.append('Baseline drift '+r['retained_path'])
    # Immutable historical bodies are compared with new start copies; current projections may change.
    for r in b['original_artifacts'] if not group else []:
        name=pathlib.PurePosixPath(r['path']).name
        if '/baseline/' in r['path'] or any(name.startswith('continuation'+str(n).zfill(2)) for n in range(1,23)):
            errors+=checked_files([r])
    m,c,w=snapshots(group,step)
    scope={s['symbol_id']:s for g in GROUPS if not group or g['group']==group for s in g['records']}
    if step:scope={d['symbol_id']:SOURCE[d['symbol_id']] for d in m}
    errors+=member_errors(m,scope)
    controls={x['control_id']:x for g in GROUPS if not group or g['group']==group for x in g['controls']}
    if step:controls={d['control_id']:CONTROLS[d['control_id']] for d in c}
    errors+=control_errors(c,controls)
    current={d['symbol_id']:d for d in semantic.read_store('review-decisions.json')}
    if any(current.get(d['symbol_id'])!=d for d in m):errors.append('Authored member/store drift')
    intents={d['control_id']:d for d in semantic.read_store('control-review-decisions.json')}
    if any(intents.get(d['control_id'])!=d for d in c if d['intent_completeness']=='qualified'):errors.append('Authored complete-control/store drift')
    wires={d['edge_id']:d for d in semantic.read_store('wiring-review-decisions.json')}
    if any(wires.get(d['edge_id'])!=d for d in w):errors.append('Authored wiring/store drift')
    if history:errors+=checked_files(a.read(EVIDENCE/'prior-raw-evidence-hashes.json')['files'])
    assert not errors,errors
    return dict(status='PASS',group=group,step=step,members=len(m),controls=len(c),qualified=sum(d['intent_completeness']=='qualified' for d in c),deferred=sum(d['intent_completeness']=='deferred' for d in c),new_wiring=len(w),prior_literal_objects=dict(members=3017,controls=750,wiring=934),protected_tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(b['compiled']),baseline=52,start_indexed_plus_index=len(b['original_artifacts']),raw_history_checked=history,raw_history_paths=len(a.read(EVIDENCE/'prior-raw-evidence-hashes.json')['files']))

def readiness_exit(remaining_members,incomplete_controls,investigations,unresolved_edges,integrity_errors=()):
    if integrity_errors:return 1
    return 2 if any((remaining_members,incomplete_controls,investigations,unresolved_edges)) else 0

@functools.lru_cache(maxsize=1)
def authored_controls():
    return {d['control_id']:d for d in snapshots()[1]}

def verify_current(intents,symbols,controls,edges):
    """Fast current catalog hook; expensive byte/history proofs run separately."""
    selected=[d for d in intents if d.get('checkpoint_id')=='04A.01-continuation-23']
    expected={i:d for i,d in authored_controls().items() if d['intent_completeness']=='qualified'}
    ids=[d['control_id'] for d in selected];errors=[]
    if len(ids)!=len(set(ids)) or set(ids)!=set(expected):errors.append('Missing/duplicate/foreign C23 qualified control')
    ss={s['symbol_id']:s for s in symbols}; cc={c['control_id']:c for c in controls};ee={e['edge_id']:e for e in edges}
    for d in selected:
        if expected.get(d['control_id'])!=d:errors.append('Changed authored C23 control contract')
        c=cc.get(d['control_id']);pin=json.loads(d['source_control_pin'])
        if c is None or any(pin[k]!=c.get(k) for k in CONTROL):errors.append('Stale C23 control declaration/default')
    for s in SOURCE.values():
        live=ss.get(s['symbol_id'])
        if live is None or any(live.get(k)!=s.get(k) for k in IDENTITY+('span_start','span_length','profile')):errors.append('Missing/stale C23 body/default/handler/outcome')
    for g in GROUPS:
        for e in frozen_group(g['group'])['wiring']:
            live=ee.get(e['edge_id'])
            if live is None or any(e[k]!=live.get(k) for k in EDGE):errors.append('Missing/stale C23 edge/delegate binding')
    return errors

def main():
    p=argparse.ArgumentParser(__doc__);p.add_argument('--check',action='store_true');p.add_argument('--guard',action='store_true');p.add_argument('--history',action='store_true');p.add_argument('--group',choices=list('ABC'));p.add_argument('--step',type=int,choices=[1,2]);args=p.parse_args()
    proof=guard(args.group,args.step,args.history)
    if args.guard or args.group:print(json.dumps(proof,indent=2));return
    render(args.check,proof)

def render(check,proof):
    import re,xml.etree.ElementTree as ET
    import continuation03_report as reader
    m,controls,_=snapshots()
    assert len(m)==179 and len(controls)==292
    runs=a.read(EVIDENCE/'validation-runs.json');assert len(runs)==4 and all(r['exit_code']==0 for r in runs)
    checks=a.read(EVIDENCE/'final-checks.json');assert all(r['exit_code']==r['expected_exit_code'] for r in checks)
    hist=next(r for r in checks if r['id']=='continuation23-preservation')
    assert hist['exit_code']==0 and a.sha(ROOT/hist['log'])==hist['sha256']
    history=json.loads(reader.text(ROOT/hist['log']));assert history['raw_history_checked'] and history['raw_history_paths']==75061
    sem=a.read(OUT/'semantic-review-coverage.json');cov=a.read(OUT/'review-coverage.json')
    assert len(catalog.load(OUT,'files.csv'))==171 and len(catalog.load(OUT,'symbols.csv'))==3866 and len(catalog.load(OUT,'udf-exports.csv'))==268
    assert sem['qualified_source_reviews']>=3017+179
    assert sem['qualified_full_control_contracts']>=697+sum(d['intent_completeness']=='qualified' for d in controls)
    code=readiness_exit(sem['unreviewed_for_acceptance'],2056-sem['qualified_full_control_contracts'],cov['member_investigations'],cov['unresolved_edge_rows'])
    status='READY FOR OWNER REVIEW' if code==0 else 'PARTIAL CHECKPOINT; NOT READY FOR ACCEPTANCE'
    def write(name,raw):
        p=OUT/name
        if isinstance(raw,str):raw=(raw.rstrip()+'\n').encode()
        if check:assert p.is_file() and p.read_bytes()==raw,'C23 report byte drift: '+name
        else:p.write_bytes(raw)
    def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
    def csv(name,rows):
        values=[{k:json.dumps(v,ensure_ascii=False,sort_keys=True) if isinstance(v,(dict,list)) else v for k,v in r.items()} for r in rows]
        write(name,catalog.render(values))
    before=a.read(EVIDENCE/'before.json')
    emit('continuation23-start.json',dict(before=before,scope_freeze_manifest=a.read(EVIDENCE/'scope-freeze-manifest.json'),historical_preflight=a.read(EVIDENCE/'history-preflight.json'),historical_manifest=dict(path=(EVIDENCE/'prior-raw-evidence-hashes.json').relative_to(ROOT).as_posix(),sha256=a.sha(EVIDENCE/'prior-raw-evidence-hashes.json')),review_credit_at_start='zero C23 credit; all scope IDs unqualified at start; nine earlier routing-only control objects retained'))
    emit('continuation23-targets.json',GROUPS)
    csv('continuation23-member-reviews.csv',[{**SOURCE[d['symbol_id']],**d} for d in m])
    csv('continuation23-control-contracts.csv',[{**CONTROLS[d['control_id']],**d} for d in controls])
    csv('continuation23-external-dependencies.csv',a.read(EVIDENCE/'external-dependencies.json'))
    # Reuse previously inspected Framework metadata without executing a control constructor.
    metadata=ROOT/'TestResults/04A-01-continuation-21/framework-defaults-final.json'
    emit('continuation23-framework-defaults.json',dict(source=metadata.relative_to(ROOT).as_posix(),sha256=a.sha(metadata),metadata=a.read(metadata),limit='Metadata attributes are not effective parent/resource/native states; absent attributes remain unknown. No new native control execution.'))
    edges=catalog.load(OUT,'ui-wiring.csv');scope_paths={s['path'] for s in SOURCE.values()}
    association=collections.defaultdict(list)
    for d in controls:
        for eid in json.loads(d['associated_control_edge_ids']):association[eid].append(d['control_id'])
    crosswalk=[]
    for e in edges:
        if e['path'] not in scope_paths:continue
        group=next(g for g in GROUPS if e['path'] in {s['path'] for s in g['records']})
        direct=association[e['edge_id']]
        crosswalk.append({**e,'review_group':group['group'],'associated_control_ids':direct,'form_context_control_ids':[c['control_id'] for c in group['controls']] if not direct else [],'association_basis':'exact declared control/AST event-or-control token' if direct else 'owning member/form context only; no direct control target inferred','runtime_status':'OUTSTANDING; native execution NOT RUN','unresolved_reason':'Retain current target/intent status; no runtime qualification from body or control credit. '+('Specific control association unresolved from this edge alone; exact owning source member remains pinned.' if not direct else 'Full/deferred decisions reside in exact linked control contracts; declaration does not imply a Click handler.')})
    assert len(crosswalk)==sum(len(frozen_group(g['group'])['wiring']) for g in GROUPS)==2691
    csv('continuation23-wiring-crosswalk.csv',crosswalk)
    byop=collections.defaultdict(list)
    for d in m:
        if d['candidate_operation_id']:byop[d['candidate_operation_id']].append(d)
    prior_ops={r['operation_id']:r for n in ('continuation09-candidate-operations.csv','continuation10-candidate-operations.csv','continuation12-candidate-operations.csv','continuation19-candidate-operations.csv') for r in catalog.load(EVIDENCE/'original/docs/architecture/WindowsMigrationLedger',n)}
    ops=[]
    for op,ds in sorted(byop.items()):
        ops.append(dict(operation_id=op,member_rows=len(ds),status='EXISTING BEHAVIOR OPERATION EXTENDED WITH GUI EVIDENCE' if op in prior_ops else 'NEW PROVISIONAL POLICY OPERATION',approved_batches='; '.join(sorted({d['future_batch'] for d in ds})),prior_approved_batches_unchanged=prior_ops.get(op,{}).get('approved_batch','none; new provisional operation'),destination='BESHStat.Excel.Common',owner='Common model request/report owner; Windows regression/PSM UI maintainer; Core statistical correctness owner',source_ids='; '.join(d['symbol_id'] for d in ds),behavior='Typed GUI option/acquisition/report policy; existing Core algorithms reused, not duplicated',freeze='NOT FROZEN; no final project-wide candidate total'))
    csv('continuation23-candidate-operations.csv',ops)
    csv('continuation23-operation-members.csv',[dict(symbol_id=d['symbol_id'],operation_id=d['candidate_operation_id'],disposition=d['disposition'],approved_batch=d['future_batch'],owner=d['owner_role']) for d in m])
    alignment=a.read(EVIDENCE/'alignment-reviewed.json')
    csv('continuation23-gui-udf-alignment.csv',alignment)
    fixtures=[dict(fixture_id='C23-control-'+d['control_id'],control_id=d['control_id'],status=d['intent_completeness'],unsupported_contract_field=d['unsupported_contract_field'],owner=d['owner_role'],approved_batch=d['future_batch'],fixture=d['verification'],execution='NOT RUN') for d in controls]
    fixtures += [dict(fixture_id=r['alignment_id'],control_id='',status='future alignment/correctness comparison',unsupported_contract_field='',owner=r['owner'],approved_batch=r['approved_batch'],fixture=r['future_fixture'],execution='NOT RUN') for r in alignment]
    csv('continuation23-fixture-crosswalk.csv',fixtures)
    pergroup=[dict(group=g['group'],members=sum(d['review_group']==g['group'] for d in m),controls=sum(d['review_group']==g['group'] for d in controls),qualified=sum(d['review_group']==g['group'] and d['intent_completeness']=='qualified' for d in controls),deferred=[d['control_id'] for d in controls if d['review_group']==g['group'] and d['intent_completeness']=='deferred'],wiring=len(frozen_group(g['group'])['wiring']),source_snapshot_sha256=a.sha(EVIDENCE/('group-'+g['group']+'-frozen-source.json'))) for g in GROUPS]
    counts=dict(status=status,strict_exit=code,before=before['starting_review'],after=sem,before_remaining=before['starting_coverage'],after_remaining=cov,new_member_ids=len(m),scoped_controls=len(controls),new_complete_controls=sum(d['intent_completeness']=='qualified' for d in controls),deferred_control_ids=[d['control_id'] for d in controls if d['intent_completeness']=='deferred'],scope_wiring=len(crosswalk),per_group=pergroup,candidate_member_rows=sum(len(ds) for ds in byop.values()),behavior_operations=len(ops),prior_decisions_preserved=dict(members=3017,control_objects=750,qualified_controls=697,wiring_objects=934),gate_b='E1-E7 OPEN',gate_c='GC3-GC8 DEFERRED',gate_c2='UNMET' if code else 'Owner review required; no automatic gate closure')
    emit('continuation23-count-deltas.json',counts)
    logs=[];trx=[]
    for config in ('Debug','Release'):
        for kind in ('full','foundation'):
            path=EVIDENCE/config/(kind+'.log');txt=reader.text(path)
            logs.append(dict(configuration=config,kind=kind,path=path.relative_to(ROOT).as_posix(),sha256=a.sha(path),python_counts=re.findall(r'Ran (\d+) tests? in',txt),node_tests=re.findall(r'(?:# |\u2139 )tests (\d+)',txt),node_pass=re.findall(r'(?:# |\u2139 )pass (\d+)',txt),node_fail=re.findall(r'(?:# |\u2139 )fail (\d+)',txt),node_skipped=re.findall(r'(?:# |\u2139 )skipped (\d+)',txt)))
        for path in sorted((EVIDENCE/config).glob('*.trx')):
            doc=ET.parse(path);trx.append(dict(configuration=config,path=path.relative_to(ROOT).as_posix(),sha256=a.sha(path),counters=doc.find('.//{*}Counters').attrib))
    auditlog=EVIDENCE/'negative-reconciliation.log';text=reader.text(auditlog)
    version='BESHstatNG/Generated/VersionAttributes.vb';pre=next(r['sha256'] for r in before['tracked'] if r['path']==version)
    assert a.sha(ROOT/version)==pre
    validation=dict(status=status,command_runs=runs,initial_commands=a.read(EVIDENCE/'initial-commands.json'),checks=checks,managed_identity_analysis=a.read(EVIDENCE/'managed-identity-analysis.json'),managed_trx=trx,python_node_logs=logs,audit=dict(path=auditlog.relative_to(ROOT).as_posix(),sha256=a.sha(auditlog),reported_counts=re.findall(r'Ran (\d+) tests? in',text),passed=bool(re.search(r'\nOK\s*$',text))),audit_identity_analysis=a.read(EVIDENCE/'audit-identity-analysis.json'),history=history,history_log=hist,history_manifest_sha256=a.sha(EVIDENCE/'prior-raw-evidence-hashes.json'),generated_restoration=dict(path=version,before_sha256=pre,after_sha256=a.sha(ROOT/version),byte_perfect=True,new_original_sha256=a.sha(EVIDENCE/'generated-originals/VersionAttributes.vb'),transient_copies=[dict(path=p.relative_to(ROOT).as_posix(),sha256=a.sha(p)) for p in sorted(EVIDENCE.glob('*/generated-by-*/BESHstatNG/Generated/VersionAttributes.vb'))]),development_corrections=a.read(EVIDENCE/'authoring-corrections.json'),parallelism='Sequential only; no benchmark, read-only validation workers or speedup claim.',not_run=['Native Excel/COM/XLL/GUI/workbook execution, live callbacks or chart lifecycle','Mac qualification or authenticated Office','R/new numerical goldens','Network/package installation, installers/signing/deployment','C24,04A.02 or any product/statistical implementation'])
    validation['corrective_commands']=a.read(EVIDENCE/'corrective-commands.json')
    emit('continuation23-validation.json',validation)
    emit('continuation23-preservation.json',dict(proof=proof,final_history=hist,start_index_sha256=before['index_sha256'],current_final_index_hash_location='TestResults/04A-01-continuation-23/final-independent-preservation.json (outside index to avoid self-hash cycle)',prior_prefixes='Exact literal UTF-8 objects and order checked; all 3017/750/934 prior objects unchanged',immutable_groups='All original two-step member/control/source snapshots preserved; refinements explicitly bind before/after',current_source_protection='171 compiled,2109 tracked,1543 GateC,52 baseline; generated originals restored exactly'))
    narrative=a.read(EVIDENCE/'report-narrative.json')
    write('continuation23.md',narrative['text'].replace('{{OVERALL}}',str(sem['qualified_source_reviews'])).replace('{{REMAINING}}',str(sem['unreviewed_for_acceptance'])).replace('{{CONTROLS}}',str(sem['qualified_full_control_contracts'])).replace('{{INVESTIGATIONS}}',str(cov['member_investigations'])).replace('{{WIRING}}',str(cov['unresolved_edge_rows'])).replace('{{MANAGED}}',str(validation['managed_identity_analysis']['Debug']['passed'])).replace('{{AUDIT}}',','.join(validation['audit']['reported_counts'])))
    # Current proposal, not staging. Historical checkpoint proposal bytes stay in start copies.
    proposed=a.read(OUT/'proposed-tracked-files.json')
    members={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}|{p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}|{'docs/architecture/WindowsMigrationLedger.md','docs/architecture/WindowsMigrationLedger/continuation23-artifacts.csv'}
    proposed['paths']=sorted(members);proposed['comparison']='Exact current indexed artifacts plus index.json; proposal only, no staging executed'
    emit('proposed-tracked-files.json',proposed)
    write('future-staging.txt','# PROPOSAL ONLY; DO NOT EXECUTE WITHOUT OWNER APPROVAL\n'+'\n'.join(('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(members)))
    files=[p for p in OUT.glob('continuation23*') if p.is_file() and p.name!='continuation23-artifacts.csv']+[semantic.TOOLING/'continuation23_report.py',semantic.TOOLING/'test_continuation23.py']
    csv('continuation23-artifacts.csv',[dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size,sha256=a.sha(p)) for p in sorted(files)])
    print(json.dumps(dict(status=status,strict_exit=code,members=len(m),controls=len(controls),qualified_controls=counts['new_complete_controls'],deferred_controls=len(counts['deferred_control_ids']),scope_wiring=len(crosswalk),proposed_paths=len(members))))

if __name__=='__main__':main()
