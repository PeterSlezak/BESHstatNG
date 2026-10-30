"""C25 source/prefix/checkpoint proofs and current-only projections. Checks are read-only."""
from __future__ import annotations
import argparse,collections,csv,hashlib,json,pathlib,sys
import amendment as a,catalog,semantic
R=catalog.ROOT;H=R/'TestResults/04A-01-continuation-25';L=catalog.DEFAULT;T=semantic.TOOLING
IDENTITY=('symbol_id','path','start_line','end_line','signature','containing_type','body_sha256')
CONTROL=('control_id','path','line','type_declaration','initialization_defaults','handles_edges')
EDGE=('edge_id','path','kind','event_or_control','target','start_line','end_line','owner_id')
def literals(raw):
    txt=raw.decode('utf-8-sig');dec=json.JSONDecoder();pos=txt.index('[')+1;out=[]
    while True:
        while txt[pos] in ' \t\r\n,':pos+=1
        if txt[pos]==']':return out
        start=pos;_,pos=dec.raw_decode(txt,pos);out.append(txt[start:pos].encode())
def checked_files(rows,base=R):
    errors=[]
    for row in rows:
        p=base/row['path']
        if not p.is_file() or p.stat().st_size!=row.get('bytes',p.stat().st_size) or a.sha(p)!=row['sha256']:errors.append('Missing/size/hash drift: '+row['path'])
    return errors
def exact_ids(rows,expected,key):
    ids=[r.get(key) for r in rows]
    return [] if len(ids)==len(set(ids)) and set(ids)==set(expected) else ['Missing/duplicate/foreign '+key]
def prefix_errors(old,new,count):
    before=literals(old);after=literals(new)
    return [] if len(before)==count and after[:count]==before else ['Prior ordered literal objects changed']
def schema_errors(before,after):return [] if before==after and len(after)==81 else ['Established 81-column source schema/order drift']
def excerpt(s):
    raw=(R/s['path']).read_bytes().decode('utf-8-sig').encode('utf-16-le');i,n=int(s['span_start']),int(s['span_length'])
    return raw[i*2:(i+n)*2].decode('utf-16-le')
def member_errors(rows,expected,frozen,entries):
    errors=exact_ids(rows,expected,'symbol_id')
    if errors:return errors
    entrymap={e['symbol_id']:e for e in entries}
    for d in rows:
        s=expected[d['symbol_id']];f=frozen[d['symbol_id']]
        if not semantic.qualified(d):errors.append('Unqualified member-v1')
        errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'C25')
        for key in ('kind','enclosing_symbol_id','profile','span_start','span_length','body_sha256','body_start_line','body_end_line'):
            dk={'kind':'source_kind','enclosing_symbol_id':'source_enclosing_symbol_id','profile':'source_profile','span_start':'source_span_start','span_length':'source_span_length','body_start_line':'source_body_start_line','body_end_line':'source_body_end_line'}.get(key,key)
            if d.get(dk)!=s.get(key):errors.append('Stale source identity '+key)
        if d.get('source_sha256')!=a.sha(R/s['path']) or f['source_excerpt']!=excerpt(s):errors.append('Stale exact UTF16 source')
        if len(f['source_excerpt'])<=120000:
            if d.get('source_excerpt')!=f['source_excerpt']:errors.append('Stale literal excerpt')
        else:
            try:
                p=json.loads(d['source_excerpt']);path=R/p['path']
                if p['symbol_id']!=s['symbol_id'] or a.sha(path)!=p['sha256']:errors.append('Unbound oversized excerpt sidecar')
            except (ValueError,KeyError):errors.append('Missing oversized excerpt sidecar')
        ep=json.loads(d.get('observed_entry_references','{}'));entry=entrymap.get(d['symbol_id'])
        if entry is None or hashlib.sha256(json.dumps(entry,sort_keys=True).encode()).hexdigest()!=ep.get('entry_sha256'):errors.append('Missing/stale caller evidence')
        if d.get('equivalence_scope')!='none' or json.loads(d.get('portable_equivalent_pins','[]')):errors.append('Unsupported whole/helper equivalence')
        if not json.loads(d.get('compared_portable_declarations','[]')):errors.append('Missing checked portable declaration')
        if d.get('future_batch') not in {'04A.'+str(i).zfill(2) for i in range(2,49)}:errors.append('Unapproved batch')
        if d.get('registration_class')!='explicit non-export' or json.loads(d.get('registration_evidence','[]')):errors.append('Invented export')
    return errors
def control_errors(rows,expected,contexts,frozen):
    errors=exact_ids(rows,expected,'control_id')
    if errors:return errors
    cm={c['control_id']:c for c in contexts};full=frozen['full_form_members'];fullids={s['symbol_id'] for s in full}
    for d in rows:
        ident=d['control_id'];c=expected[ident];ctx=cm.get(ident)
        if ctx is None:errors.append('Missing individual control context');continue
        if json.loads(d.get('source_control_pin','{}'))!={k:c[k] for k in CONTROL}:errors.append('Stale default/declaration pin')
        p=json.loads(d['source_control_references'])
        if hashlib.sha256(json.dumps(ctx,sort_keys=True,ensure_ascii=False).encode()).hexdigest()!=p.get('entry_sha256'):errors.append('Stale control context/default/consumer/parent')
        fp=json.loads(d['form_source_snapshot'])
        if a.sha(R/fp['path'])!=fp['sha256']:errors.append('Stale full form dependency snapshot')
        if set(json.loads(d.get('control_flow_pins','[]')))!=fullids:errors.append('Missing cross-group source dependencies')
        required={s['symbol_id'] for s in full if s['name'] in ('.ctor','InitializeComponent','Dispose')}
        if set(json.loads(d.get('required_constructor_initialization_ids','[]')))!=required:errors.append('Missing constructor/Designer/Dispose')
        if frozen['full_form_members'][0]['path'].endswith('Ui21ControlCharts.Designer.vb'):
            life={s['symbol_id'] for s in full if s['name'] in ('Ui21ControlCharts_FormClosing','CancellationRequested','EndComputation')}
            if not life<=set(json.loads(d.get('form_lifecycle_method_ids','[]'))):errors.append('Missing cancel/close/lifetime')
        if not all(d.get(k) for k in ('reviewed_intent','qualification_reason','owner_role','future_batch','verification','lifetime_contract')):errors.append('Incomplete owned control decision')
        status=d.get('intent_completeness')
        if status=='qualified':
            if not semantic.control_qualified(d) or d.get('unsupported_contract_field'):errors.append('Unsupported complete control')
            if len(ctx['parent_invocations'])!=1:errors.append('Unproven containment')
        elif status=='deferred':
            if semantic.control_qualified(d) or not d.get('unsupported_contract_field'):errors.append('False credit/unowned deferral')
        else:errors.append('Missing qualified/deferred status')
        errors+=semantic.verify_pins([d],'C25-control')
    return errors
def readiness_exit(m,c,i,e,errors=()):return 1 if errors else 2 if any((m,c,i,e)) else 0
def adjudication_errors(rows,decisions):
    expected={d['control_id']:d for d in decisions if d['intent_completeness']=='qualified'}
    errors=exact_ids(rows,expected,'control_id')
    if errors:return errors
    for row in rows:
        d=expected[row['control_id']]
        if hashlib.sha256(json.dumps(d,sort_keys=True,ensure_ascii=False).encode()).hexdigest()!=row.get('decision_sha256'):errors.append('Unbound individual display decision')
        if row.get('context_pin')!=json.loads(d['source_control_references']):errors.append('Unbound individual default/parent/state context')
        if not all(row.get(k) for k in ('individual_contract','display_and_state_source','parent','owner','future_batch')):errors.append('Incomplete individual display adjudication')
    return errors
def snapshots(group=None):
    m=[];c=[];w=[]
    for g in a.read(H/'targets.json'):
        if group and group!=g['group']:continue
        name=g['group']
        mp=H/('group-'+name+'-member-evidence.json');cp=H/('group-'+name+'-control-evidence.json')
        if mp.exists():m+=a.read(mp)['decisions']
        if cp.exists():data=a.read(cp);c+=data['controls'];w+=data['new_wiring']
    return m,c,w
def guard(group=None,history=False):
    before=a.read(H/'before.json');errors=[];groups=a.read(H/'targets.json');source={s['symbol_id']:s for s in catalog.load(L,'symbols.csv')}
    assert a.git('branch','--show-current')==before['branch'] and a.git('rev-parse','HEAD')==a.git('rev-parse','main')==before['head']
    assert not a.git('diff','--name-only') and not a.git('diff','--cached','--name-only')
    for name,n in [('review-decisions',3438),('control-review-decisions',1039),('wiring-review-decisions',1260)]:
        errors+=prefix_errors((H/'original/scripts/windows-migration-audit'/(name+'.json')).read_bytes(),(T/(name+'.json')).read_bytes(),n)
    def header(p):
        with p.open(encoding='utf-8-sig',newline='') as stream:return next(csv.reader(stream))
    errors+=schema_errors(header(H/'original/docs/architecture/WindowsMigrationLedger/symbols.csv'),header(L/'symbols.csv'))
    errors+=checked_files(a.read(H/'scope-freeze-manifest.json'))
    m,c,w=snapshots(group)
    for g in groups:
        if group and group!=g['group']:continue
        name=g['group'];f=a.read(H/('group-'+name+'-frozen-source.json'))
        selected=[d for d in m if d['review_group']==name]
        errors+=member_errors(selected,{s['symbol_id']:source[s['symbol_id']] for s in g['records']},{s['symbol_id']:s for s in f['members']},a.read(H/('group-'+name+'-entry-evidence.json')))
        errors+=control_errors([d for d in c if d['review_group']==name],{c['control_id']:c for c in g['controls']},a.read(H/('group-'+name+'-control-context.json')),f)
        if set(g['edge_ids'])!={e['edge_id'] for e in f['wiring']}:errors.append('Scoped edge membership drift')
        if any(d['review_group']!=name for d in selected):errors.append('Wrong group')
        marker=H/('checkpoint-'+name+'.json')
        if marker.exists():errors+=checked_files(a.read(marker)['immutable_evidence'])
    for name,key,rows in [('review-decisions.json','symbol_id',m),('control-review-decisions.json','control_id',[d for d in c if d['intent_completeness']=='qualified']),('wiring-review-decisions.json','edge_id',w)]:
        store={d[key]:d for d in semantic.read_store(name)}
        if any(store.get(d[key])!=d for d in rows):errors.append('Authored/store drift '+name)
    if not group:
        if not (H/'qualified-control-adjudications.json').is_file():errors.append('Missing per-ID control qualification adjudications')
        else:errors+=adjudication_errors(a.read(H/'qualified-control-adjudications.json'),c)
        required=('review-input-manifest.json','external-dependencies.json','dependency-snapshots.json','portable-comparisons.json','alignment-reviewed.json','candidate-operations.json','candidate-members.json')
        for name in required:
            if not (H/name).is_file():errors.append('Missing C25 review input '+name)
        if (H/'review-input-manifest.json').exists():errors+=checked_files(a.read(H/'review-input-manifest.json'))
        if (H/'dependency-snapshots.json').exists():
            for dep in a.read(H/'dependency-snapshots.json'):
                errors+=checked_files([dict(path=dep['path'],sha256=dep['source_sha256'])])
                errors+=checked_files([dict(path=dep['snapshot'],bytes=dep['bytes'],sha256=dep['sha256'])])
        for label in ('tracked','gate','compiled'):errors+=checked_files(before[label])
        errors+=checked_files(before['original_artifacts'],H/'original')
        for r in before['baseline']['entries']:
            if a.sha(R/r['retained_path'])!=r['sha256']:errors.append('Baseline drift')
        for r in before['original_artifacts']:
            name=pathlib.PurePosixPath(r['path']).name
            if '/baseline/' in r['path'] or any(name.startswith('continuation'+str(n).zfill(2)) for n in range(1,25)):errors+=checked_files([r])
    if history:errors+=checked_files(a.read(H/'prior-raw-evidence-hashes.json')['files'])
    assert not errors,errors
    return dict(status='PASS',group=group,members=len(m),controls=len(c),qualified=sum(d['intent_completeness']=='qualified' for d in c),deferred=sum(d['intent_completeness']=='deferred' for d in c),new_wiring=len(w),history_checked=history,history_paths=len(a.read(H/'prior-raw-evidence-hashes.json')['files']),prefix_objects=[3438,1039,1260])
def verify_current(intents,symbols,controls,edges):
    m,c,w=snapshots();expected={d['control_id']:d for d in c if d['intent_completeness']=='qualified'}
    selected=[d for d in intents if d.get('checkpoint_id')=='04A.01-continuation-25'];errors=exact_ids(selected,expected,'control_id')
    if any(expected.get(d['control_id'])!=d for d in selected):errors.append('Changed C25 authored contract')
    if not (H/'qualified-control-adjudications.json').is_file():errors.append('Missing C25 individual control adjudications')
    else:errors+=adjudication_errors(a.read(H/'qualified-control-adjudications.json'),c)
    si={s['symbol_id']:s for s in symbols};ei={e['edge_id']:e for e in edges};ci={c['control_id']:c for c in controls}
    for g in a.read(H/'targets.json'):
        f=a.read(H/('group-'+g['group']+'-frozen-source.json'))
        for s in g['records']:
            if s['symbol_id'] not in si or any(s[k]!=si[s['symbol_id']].get(k) for k in IDENTITY+('profile','span_start','span_length')):errors.append('Missing/stale C25 member')
        for c0 in g['controls']:
            if c0['control_id'] not in ci or any(c0[k]!=ci[c0['control_id']].get(k) for k in CONTROL):errors.append('Missing/stale C25 declaration/default')
        for e in f['wiring']:
            if e['edge_id'] not in ei or any(e[k]!=ei[e['edge_id']].get(k) for k in EDGE):errors.append('Missing/stale C25 scoped edge')
    return errors
def main():
    p=argparse.ArgumentParser(__doc__);p.add_argument('--guard',action='store_true');p.add_argument('--check',action='store_true');p.add_argument('--history',action='store_true');p.add_argument('--group',choices=list('ABCDE'));args=p.parse_args()
    proof=guard(args.group,args.history)
    if args.guard or args.group:print(json.dumps(proof,indent=2));return
    render(args.check,proof)
def render(check,proof):
    import re,xml.etree.ElementTree as ET
    import continuation03_report as reader
    m,controls,w=snapshots();groups=a.read(H/'targets.json');before=a.read(H/'before.json')
    assert (len(m),len(controls))==(251,355)
    markers=[a.read(H/('checkpoint-'+g['group']+'.json')) for g in groups];assert all(p['status']=='PASS' for p in markers)
    runs=a.read(H/'validation-runs.json');assert len(runs)==4 and all(r['exit_code']==0 for r in runs)
    checks=a.read(H/'final-checks.json');assert all(r['exit_code']==r['expected_exit_code'] for r in checks)
    history=next(r for r in checks if r['id']=='continuation25-preservation');assert history['exit_code']==0 and a.sha(R/history['log'])==history['sha256']
    history_proof=json.loads(reader.text(R/history['log']));assert history_proof['history_checked'] and history_proof['history_paths']==82474
    sem=a.read(L/'semantic-review-coverage.json');cov=a.read(L/'review-coverage.json')
    symbols=catalog.load(L,'symbols.csv');ss={s['symbol_id']:s for s in symbols};cs={d['control_id']:d for g in groups for d in g['controls']}
    assert len(catalog.load(L,'files.csv'))==171 and len(symbols)==3866 and len(catalog.load(L,'udf-exports.csv'))==268
    assert sem['qualified_source_reviews']==3689 and sem['qualified_full_control_contracts']==986+sum(d['intent_completeness']=='qualified' for d in controls)
    remaining=catalog.load(L,'remaining-member-review.csv');queue=collections.Counter(pathlib.PurePosixPath(s['path']).name for s in remaining)
    assert len(remaining)==177 and queue=={'Ui22MultivariateControlCharts.vb':173,'Ui22MultivariateControlCharts.Designer.vb':2,'UibyID.Designer.vb':2},queue
    code=readiness_exit(sem['unreviewed_for_acceptance'],2056-sem['qualified_full_control_contracts'],cov['member_investigations'],cov['unresolved_edge_rows']);assert code==2
    def write(name,raw):
        if isinstance(raw,str):raw=(raw.rstrip()+'\n').encode()
        p=L/name
        if check:assert p.is_file() and p.read_bytes()==raw,'C25 report drift: '+name
        else:p.write_bytes(raw)
    def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
    def csvout(name,rows):
        write(name,catalog.render([{k:json.dumps(v,sort_keys=True,ensure_ascii=False) if isinstance(v,(list,dict)) else v for k,v in row.items()} for row in rows]))
    csvout('continuation25-member-reviews.csv',[{**ss[d['symbol_id']],**d} for d in m])
    csvout('continuation25-control-contracts.csv',[{**cs[d['control_id']],**d} for d in controls])
    csvout('continuation25-control-adjudications.csv',a.read(H/'qualified-control-adjudications.json'))
    csvout('continuation25-portable-comparisons.csv',a.read(H/'portable-comparisons.json'))
    csvout('continuation25-external-dependencies.csv',[{**{k:s[k] for k in IDENTITY+('profile','span_start','span_length')},'source_sha256':s['source_sha256'],'full_utf16_evidence':'TestResults/04A-01-continuation-25/external-dependencies.json; exact symbol_id'} for s in a.read(H/'external-dependencies.json')])
    alignment=a.read(H/'alignment-reviewed.json');operations=a.read(H/'candidate-operations.json');operation_members=a.read(H/'candidate-members.json')
    csvout('continuation25-gui-udf-alignment.csv',alignment);csvout('continuation25-candidate-operations.csv',operations);csvout('continuation25-operation-members.csv',operation_members)
    association=collections.defaultdict(list)
    for d in controls:
        for eid in json.loads(d['associated_control_edge_ids']):association[eid].append(d['control_id'])
    edgegroups={eid:g['group'] for g in groups for eid in g['edge_ids']};crosswalk=[]
    for e in catalog.load(L,'ui-wiring.csv'):
        if e['edge_id'] not in edgegroups:continue
        associated=association[e['edge_id']]
        crosswalk.append({**e,'review_group':edgegroups[e['edge_id']],'associated_control_ids':associated,'association_basis':'Exact control token in event/target and source context' if associated else 'Owning member/form context only; no direct control target inferred','runtime_status':'NOT RUN; no native execution proof','unresolved_reason':'Preserve actual target/intent status; decorative no-event declarations are not defects. Active/delegate/grid/reentrant intent remains explicitly owned in control decisions.'})
    assert len(crosswalk)==len(edgegroups)==3284
    csvout('continuation25-wiring-crosswalk.csv',crosswalk)
    fixtures=[dict(fixture_id='C25-control-'+d['control_id'],source_id=d['control_id'],status=d['intent_completeness'],missing=d['unsupported_contract_field'],owner=d['owner_role'],future_batch=d['future_batch'],fixture=d['verification'],execution='NOT RUN') for d in controls]
    fixtures += [dict(fixture_id='C25-member-'+d['symbol_id'],source_id=d['symbol_id'],status='future source-policy fixture',missing='',owner=d['owner_role'],future_batch=d['future_batch'],fixture=d['proposed_test'],execution='NOT RUN') for d in m]
    fixtures += [dict(fixture_id=r['case_id'],source_id='; '.join(p['symbol_id'] for p in r['gui_declarations']),status='future alignment/correctness',missing='',owner=r['owner'],future_batch=r['future_batch'],fixture=r['fixture'],execution='NOT RUN') for r in alignment]
    csvout('continuation25-fixture-crosswalk.csv',fixtures)
    pergroup=[dict(group=p['group'],members=p['members'],controls=p['controls'],qualified=p['qualified'],deferred=p['deferred'],wiring=len(p['edge_ids']),checkpoint_sha256=a.sha(H/('checkpoint-'+p['group']+'.json'))) for p in markers]
    counts=dict(status='PARTIAL CHECKPOINT; NOT READY FOR ACCEPTANCE',strict_exit=code,before=before['starting_review'],after=sem,before_remaining=before['starting_coverage'],after_remaining=cov,new_member_ids=251,scoped_controls=355,new_complete_controls=sum(d['intent_completeness']=='qualified' for d in controls),new_deferred_controls=sum(d['intent_completeness']=='deferred' for d in controls),per_group=pergroup,remaining_queue=dict(queue),scope_wiring=3284,candidate_member_rows=len(operation_members),provisional_operation_labels=len({r['provisional_label'] for r in operation_members}),deduplicated_behavior_operations=len(operations),existing_behavior_extensions=sum(not r['operation_id'].startswith('C25-') for r in operations),new_provisional_behaviors=sum(r['operation_id'].startswith('C25-') for r in operations),prior_prefix_objects=[3438,1039,1260],historical_paths=82474,gate_b='E1-E7 OPEN',gate_c='GC3-GC8 DEFERRED',gate_c2='UNMET')
    emit('continuation25-count-deltas.json',counts)
    auditlog=H/'negative-reconciliation.log';audittext=reader.text(auditlog);auditids=[]
    for candidate in re.findall(r'\(([^()]*)\)',audittext):
        ident=re.sub(r'\s+','',candidate)
        if re.fullmatch(r'[\w.]+\.test_\w+',ident):auditids.append(ident)
    reported=re.findall(r'Ran (\d+) tests? in',audittext);assert len(reported)==1 and len(auditids)==len(set(auditids))==int(reported[0])
    assert re.search(r'\nOK\s*$',audittext)
    logs=[];trx=[]
    for config in ('Debug','Release'):
        for kind in ('full','foundation'):
            p=H/config/(kind+'.log');text=reader.text(p)
            logs.append(dict(configuration=config,kind=kind,path=p.relative_to(R).as_posix(),sha256=a.sha(p),python_counts=re.findall(r'Ran (\d+) tests? in',text),node_tests=re.findall(r'(?:# |\u2139 )tests (\d+)',text),node_pass=re.findall(r'(?:# |\u2139 )pass (\d+)',text),node_fail=re.findall(r'(?:# |\u2139 )fail (\d+)',text),node_skipped=re.findall(r'(?:# |\u2139 )skipped (\d+)',text)))
        for p in sorted((H/config).glob('*.trx')):
            trx.append(dict(configuration=config,path=p.relative_to(R).as_posix(),sha256=a.sha(p),counters=ET.parse(p).find('.//{*}Counters').attrib))
    version='BESHstatNG/Generated/VersionAttributes.vb';pre=next(r['sha256'] for r in before['tracked'] if r['path']==version);assert a.sha(R/version)==pre
    validation=dict(status=counts['status'],command_runs=runs,initial_commands=a.read(H/'initial-commands.json'),checks=checks,managed_identity_analysis=a.read(H/'managed-identity-analysis.json'),managed_trx=trx,python_node_logs=logs,audit=dict(path=auditlog.relative_to(R).as_posix(),sha256=a.sha(auditlog),passed=len(auditids),failed=0,skipped=0,identities=sorted(auditids),focused_c25_identities=len([i for i in auditids if '.Continuation25Tests.' in i]),rerun_credit=0),history=history_proof,history_log=history,generated_restoration=dict(path=version,before_sha256=pre,after_sha256=a.sha(R/version),original_snapshot_sha256=a.sha(H/'generated-originals/VersionAttributes.vb'),transient_copies=[dict(path=p.relative_to(R).as_posix(),sha256=a.sha(p)) for p in sorted(H.glob('*/generated-by-*/BESHstatNG/Generated/VersionAttributes.vb'))]),development_corrections=a.read(H/'authoring-corrections.json'),parallelism='Sequential. No parallel workers, orchestration benchmark or speedup claim.',not_run=['Native Excel/COM/XLL/GUI/workbook/chart execution','Mac/authenticated Office','R/new numerical goldens','Network/package installation, installers/signing/deployment','C26,04A.02 or product/statistical implementation'])
    emit('continuation25-validation.json',validation)
    prefixes=[]
    for name,n in [('review-decisions',3438),('control-review-decisions',1039),('wiring-review-decisions',1260)]:
        old=(H/'original/scripts/windows-migration-audit'/(name+'.json')).read_bytes();new=(T/(name+'.json')).read_bytes();oldhash=hashlib.sha256(b'\n'.join(literals(old))).hexdigest();newhash=hashlib.sha256(b'\n'.join(literals(new)[:n])).hexdigest();assert oldhash==newhash
        prefixes.append(dict(store=name,prior_objects=n,previous_store_sha256=hashlib.sha256(old).hexdigest(),before_literal_prefix_sha256=oldhash,after_literal_prefix_sha256=newhash,byte_and_order_identical=True))
    emit('continuation25-preservation.json',dict(proof=proof,previous_index_sha256=before['index_sha256'],previous_indexed_plus_index=506,history_paths=82474,prefixes=prefixes,current_index_self_hash='TestResults/04A-01-continuation-25/final-independent-preservation.json; separate from indexed artifacts to avoid hash cycle',protected_sets=dict(tracked=2109,gate_c=1543,compiled=171,baseline=52)))
    emit('continuation25-targets.json',groups)
    narrative=a.read(H/'report-narrative.json')['text']
    for token,value in [('{{MEMBERS}}',sem['qualified_source_reviews']),('{{CONTROLS}}',sem['qualified_full_control_contracts']),('{{INVESTIGATIONS}}',cov['member_investigations']),('{{WIRING}}',cov['unresolved_edge_rows']),('{{AUDIT}}',len(auditids)),('{{MANAGED}}',validation['managed_identity_analysis']['Debug']['passed'])]:narrative=narrative.replace(token,str(value))
    write('continuation25.md',narrative)
    proposed=a.read(L/'proposed-tracked-files.json');members={p.relative_to(R).as_posix() for p in L.rglob('*') if p.is_file()}|{p.relative_to(R).as_posix() for p in T.iterdir() if p.is_file()}|{'docs/architecture/WindowsMigrationLedger.md','docs/architecture/WindowsMigrationLedger/continuation25-artifacts.csv'}
    proposed['paths']=sorted(members);proposed['comparison']='Exact current indexed artifacts plus index.json; proposal only, no staging executed';emit('proposed-tracked-files.json',proposed)
    write('future-staging.txt','# PROPOSAL ONLY; DO NOT EXECUTE WITHOUT OWNER APPROVAL\n'+'\n'.join(('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(members)))
    files=[p for p in L.glob('continuation25*') if p.is_file() and p.name!='continuation25-artifacts.csv']+[T/'continuation25_report.py',T/'test_continuation25.py']
    csvout('continuation25-artifacts.csv',[dict(path=p.relative_to(R).as_posix(),bytes=p.stat().st_size,sha256=a.sha(p)) for p in sorted(files)])
    print(json.dumps(dict(status=counts['status'],strict_exit=code,members=251,controls=355,qualified=counts['new_complete_controls'],deferred=counts['new_deferred_controls'],operations=len(operations),proposed_paths=len(members))))
if __name__=='__main__':main()
