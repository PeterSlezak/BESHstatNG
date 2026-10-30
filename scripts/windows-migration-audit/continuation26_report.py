"""C26 exact source/control evidence guards and current-only projections. Checks do not write."""
from __future__ import annotations
import argparse,collections,csv,hashlib,json,pathlib,sys
import amendment as a,catalog,semantic
from continuation25_report import literals,checked_files,exact_ids,prefix_errors,schema_errors,member_errors,control_errors,IDENTITY,CONTROL,EDGE
R=catalog.ROOT;L=catalog.DEFAULT;T=semantic.TOOLING;H=R/'TestResults/04A-01-continuation-26'
def snapshots(group=None):
    m=[];c=[]
    for g in a.read(H/'targets.json'):
        if group and g['group']!=group:continue
        p=H/('group-'+g['group']+'-member-evidence.json')
        if p.exists():m+=a.read(p)['decisions']
        p=H/('group-'+g['group']+'-control-evidence.json')
        if p.exists():c+=a.read(p)['controls']
    return m,c
def target_errors(groups):
    errors=[]
    if [(g['group'],len(g['records']),len(g['controls'])) for g in groups]!=[(x,35,n) for x,n in zip('ABCDE',[37,37,36,36,36])]:errors.append('Incorrect five checkpoint quotas')
    m=[s for g in groups for s in g['records']];c=[s for g in groups for s in g['controls']]
    if len(m)!=175 or len({s['symbol_id'] for s in m})!=175:errors.append('Missing/duplicate 175 source IDs')
    if len(c)!=182 or len({s['control_id'] for s in c})!=182:errors.append('Missing/duplicate 182 control IDs')
    if any('Ui22MultivariateControlCharts' not in s['path'] for s in m+c):errors.append('Foreign form target')
    return errors
def control_admission(rows,expected,contexts,frozen):
    errors=control_errors(rows,expected,contexts,frozen)
    for d in rows:
        if d.get('intent_completeness')!='deferred' or d.get('review_contract')!='unreviewed' or semantic.control_qualified(d):errors.append('Fabricated qualified C26 contract; individual missing field unresolved')
    return errors
def wiring_errors(edges,full,rows):
    errors=exact_ids(rows,{e['edge_id'] for e in edges},'edge_id')
    source={s['symbol_id']:s for s in full}
    for e in rows:
        if e.get('kind') in ('AddHandler','Handles'):
            ids=json.loads(e.get('handler_source_ids','[]'))
            if not ids or any(i not in source for i in ids):errors.append('Missing dynamic/direct handler source')
    return errors
def verify_current(symbols,controls,edges,decisions):
    errors=[];groups=a.read(H/'targets.json');errors+=target_errors(groups)
    m,c=snapshots();expected={d['symbol_id']:d for d in m};actual=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-26']
    errors+=exact_ids(actual,expected,'symbol_id')
    if any(expected.get(d['symbol_id'])!=d for d in actual):errors.append('C26 authored/store drift')
    si={s['symbol_id']:s for s in symbols};ci={s['control_id']:s for s in controls};ei={s['edge_id']:s for s in edges}
    for g in groups:
        f=a.read(H/('group-'+g['group']+'-frozen-source.json'))
        for s in g['records']:
            if s['symbol_id'] not in si or any(s[k]!=si[s['symbol_id']].get(k) for k in IDENTITY+('profile','span_start','span_length')):errors.append('Stale C26 source ID/body/span')
        for s in g['controls']:
            if s['control_id'] not in ci or any(s[k]!=ci[s['control_id']].get(k) for k in CONTROL):errors.append('Erroneous C26 control ID/default')
            if ci.get(s['control_id'],{}).get('review_contract')=='04A.01-control-v1':errors.append('Unbacked C26 qualified control')
        if any(any(ei.get(e['edge_id'],{}).get(k)!=e[k] for k in EDGE) for e in f['wiring']):errors.append('C26 edge drift')
    return errors
def guard(group=None,history=False):
    before=a.read(H/'before.json');errors=[];groups=a.read(H/'targets.json');errors+=target_errors(groups)
    assert a.git('branch','--show-current')==before['branch'] and a.git('rev-parse','HEAD')==a.git('rev-parse','main')==before['head']
    assert not a.git('diff','--name-only') and not a.git('diff','--cached','--name-only')
    for p in before['decision_prefixes']:
        name=p['store']+'.json';errors+=prefix_errors((H/'original/scripts/windows-migration-audit'/name).read_bytes(),(T/name).read_bytes(),p['objects'])
        if p['store']!='review-decisions' and (T/name).read_bytes()!=(H/'original/scripts/windows-migration-audit'/name).read_bytes():errors.append('Unexpected control/wiring store change: all C26 contracts deferred')
    with (H/'original/docs/architecture/WindowsMigrationLedger/symbols.csv').open(encoding='utf-8-sig',newline='') as stream:old=next(csv.reader(stream))
    with (L/'symbols.csv').open(encoding='utf-8-sig',newline='') as stream:new=next(csv.reader(stream))
    errors+=schema_errors(old,new);errors+=checked_files(a.read(H/'scope-freeze-manifest.json'))
    m,c=snapshots(group)
    store={d['symbol_id']:d for d in semantic.read_store('review-decisions.json')}
    if any(store.get(d['symbol_id'])!=d for d in m):errors.append('C26 authored/store drift')
    if len(store)!=3689+len(snapshots()[0]):errors.append('Unexpected extra/missing decision outside C26')
    for g in groups:
        if group and g['group']!=group:continue
        name=g['group'];f=a.read(H/('group-'+name+'-frozen-source.json'))
        errors+=member_errors([d for d in m if d['review_group']==name],{s['symbol_id']:s for s in g['records']},{s['symbol_id']:s for s in f['members']},a.read(H/('group-'+name+'-entry-evidence.json')))
        errors+=control_admission([d for d in c if d['review_group']==name],{s['control_id']:s for s in g['controls']},a.read(H/('group-'+name+'-control-context.json')),f)
        errors+=checked_files(a.read(H/('checkpoint-'+name+'.json'))['immutable_evidence'])
        errors+=checked_files(a.read(H/('checkpoint-'+name+'-start.json'))['decision_prefixes'])
    for label in ('tracked','gate','compiled'):errors+=checked_files(before[label])
    for e in before['baseline']['entries']:
        if a.sha(R/e['retained_path'])!=e['sha256']:errors.append('Original baseline drift')
    errors+=checked_files(before['original_artifacts'],H/'original')
    for e in before['original_artifacts']:
        name=pathlib.PurePosixPath(e['path']).name
        if '/baseline/' in e['path'] or any(name.startswith('continuation'+str(n).zfill(2)) for n in range(1,26)):errors+=checked_files([e])
    if history:
        manifest=a.read(H/'prior-raw-evidence-hashes.json')['files'];errors+=checked_files(manifest)
        roots=[p for p in (R/'TestResults').glob('04A-01*') if p!=H]
        actual={p.relative_to(R).as_posix() for root in roots for p in root.rglob('*') if p.is_file()}
        expected={e['path'] for e in manifest if e['path'].split('/')[1].startswith('04A-01')}
        if actual!=expected:errors.append('Historical raw path membership drift')
    assert not errors,errors
    return dict(status='PASS',group=group,members=len(m),controls=len(c),qualified=0,deferred=len(c),history_checked=history,historical_paths=before['historical_paths'],prefix_objects=[p['objects'] for p in before['decision_prefixes']])
def index_errors(index,paths,base=R):
    es=index['artifacts'];ids=[e['path'] for e in es]
    errors=[] if len(ids)==len(set(ids)) and set(ids)==set(paths) else ['Index membership drift']
    return errors+checked_files(es,base)
def render(check=False):
    proof=guard();groups=a.read(H/'targets.json');m,c=snapshots();assert(len(m),len(c))==(175,182)
    symbols=catalog.load(L,'symbols.csv');ss={s['symbol_id']:s for s in symbols};cs={s['control_id']:s for g in groups for s in g['controls']}
    sem=a.read(L/'semantic-review-coverage.json');cov=a.read(L/'review-coverage.json');assert sem['qualified_source_reviews']==3864 and sem['qualified_full_control_contracts']==1129
    remaining=catalog.load(L,'remaining-member-review.csv');assert len(remaining)==2 and all(s['path'].endswith('UibyID.Designer.vb') for s in remaining)
    def write(name,raw):
        if isinstance(raw,str):raw=(raw.rstrip()+'\n').encode()
        p=L/name
        if check:assert p.read_bytes()==raw,'C26 report drift '+name
        else:p.write_bytes(raw)
    def emit(name,v):write(name,json.dumps(v,indent=2,ensure_ascii=False))
    def csvout(name,rows,fields=None):
        if fields:
            import io
            out=io.StringIO(newline='');writer=csv.DictWriter(out,fields,quoting=csv.QUOTE_ALL,lineterminator='\n');writer.writeheader();writer.writerows(rows);write(name,out.getvalue());return
        write(name,catalog.render([{k:json.dumps(v,sort_keys=True,ensure_ascii=False) if isinstance(v,(dict,list)) else v for k,v in row.items()} for row in rows]))
    with (L/'symbols.csv').open(encoding='utf-8-sig',newline='') as stream:header=next(csv.reader(stream))
    csvout('continuation26-member-reviews.csv',[{**ss[d['symbol_id']],**d} for d in m],header)
    csvout('continuation26-control-contracts.csv',[{**cs[d['control_id']],**d} for d in c])
    csvout('continuation26-control-adjudications.csv',[dict(control_id=d['control_id'],status='deferred',unsupported_contract_field=d['unsupported_contract_field'],qualification_reason=d['qualification_reason'],source_control_references=d['source_control_references'],owner=d['owner_role'],future_batch=d['future_batch'],fixture=d['verification'],confidence=d['confidence']) for d in c])
    allfull=a.read(H/'group-A-frozen-source.json');edges=allfull['full_form_wiring'];full=allfull['full_form_members'];crosswalk=[]
    for e in edges:
        src=[s['symbol_id'] for s in full if s['path']==e['path'] and int(s['start_line'])<=int(e['start_line'])<=int(s['end_line'])]
        associated=[d['control_id'] for d in c if e['edge_id'] in json.loads(d['associated_control_edge_ids'])]
        crosswalk.append({**e,'handler_source_ids':json.dumps(src),'associated_control_ids':associated,'resolution':'Source-pinned static association only; existing global unresolved status preserved. No runtime/complete contract credit.'})
    assert not wiring_errors(edges,full,crosswalk)
    csvout('continuation26-wiring-crosswalk.csv',crosswalk)
    comp=[dict(member_id=d['symbol_id'],comparison=json.loads(d['compared_portable_declarations']),scope='comparison only; no whole GUI replacement',observation=d['source_purpose'],owner=d['owner_role'],future_batch=d['future_batch']) for d in m]
    csvout('continuation26-portable-comparisons.csv',comp)
    candidates=[dict(operation_id=d['candidate_operation_id'],member_id=d['symbol_id'],responsibility=d['source_purpose'],owner=d['owner_role'],future_batch=d['future_batch'],fixture=d['proposed_test']) for d in m if d['candidate_operation_id']]
    csvout('continuation26-operation-members.csv',candidates)
    operations=[]
    for operation in sorted({d['operation_id'] for d in candidates}):
        selected=[d for d in candidates if d['operation_id']==operation]
        operations.append(dict(operation_id=operation,classification='new provisional multivariate request/result policy' if operation.startswith('C26-') else 'extension of existing operation; no extra global credit',member_ids=[d['member_id'] for d in selected],boundaries='Host COM/control bindings remain separate; shared string/table/acquisition policy requires compatibility fixtures; no whole workflow extraction authorized',owner='Common request/output owner; Core SPC maintainer; Windows UI maintainer',future_batches=sorted({d['future_batch'] for d in selected}),runtime='NOT RUN'))
    csvout('continuation26-candidate-operations.csv',operations)
    alignment=a.read(H/'alignment-reviewed.json');csvout('continuation26-gui-core-alignment.csv',alignment)
    fixtures=[dict(source_id=d['symbol_id'],fixture=d['proposed_test'],owner=d['owner_role'],future_batch=d['future_batch'],status='NOT RUN') for d in m]+[dict(source_id=d['control_id'],fixture=d['verification'],owner=d['owner_role'],future_batch=d['future_batch'],status='NOT RUN') for d in c]
    csvout('continuation26-fixture-crosswalk.csv',fixtures)
    emit('continuation26-targets.json',groups)
    emit('continuation26-count-deltas.json',dict(before=a.read(H/'original/docs/architecture/WindowsMigrationLedger/semantic-review-coverage.json'),after=sem,current_review=cov,new_members=175,scoped_controls=182,qualified_controls=0,deferred_controls=182,scope_edges=len(edges),remaining=remaining,gate_c2='UNMET',exceptions='E1-E7 OPEN; GC3-GC8 DEFERRED'))
    emit('continuation26-preservation.json',dict(proof=proof,start=a.read(H/'start-verification.json'),final_history='TestResults/04A-01-continuation-26/history-final.json',independent_index_hash='TestResults/04A-01-continuation-26/final-facts.json'))
    emit('continuation26-validation.json',a.read(H/'validation-summary.json'))
    write('continuation26.md',(H/'narrative.md').read_bytes())
    files=[p for p in L.glob('continuation26*') if p.name!='continuation26-artifacts.csv']+[T/'continuation26_report.py',T/'test_continuation26.py']
    csvout('continuation26-artifacts.csv',[dict(path=p.relative_to(R).as_posix(),bytes=p.stat().st_size,sha256=a.sha(p)) for p in sorted(files)])
    members={p.relative_to(R).as_posix() for p in L.rglob('*') if p.is_file()}|{p.relative_to(R).as_posix() for p in T.iterdir() if p.is_file()}|{'docs/architecture/WindowsMigrationLedger.md'}
    proposal=a.read(L/'proposed-tracked-files.json');proposal['paths']=sorted(members);proposal['comparison']='Exact current indexed artifacts plus index.json; proposal only, no staging executed';emit('proposed-tracked-files.json',proposal)
    write('future-staging.txt','# PROPOSAL ONLY; DO NOT EXECUTE WITHOUT OWNER APPROVAL\n'+'\n'.join(('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(members)))
    print(json.dumps(dict(status='PARTIAL CHECKPOINT; OFFICIAL 04A.01 NOT READY FOR ACCEPTANCE',members=175,controls=182,qualified=0,deferred=182,proposed_paths=len(members))))
def main():
    p=argparse.ArgumentParser(__doc__);p.add_argument('--guard',action='store_true');p.add_argument('--history',action='store_true');p.add_argument('--group',choices=list('ABCDE'));p.add_argument('--check',action='store_true');args=p.parse_args()
    if args.guard or args.group:print(json.dumps(guard(args.group,args.history),indent=2))
    else:render(args.check)
if __name__=='__main__':main()
