"""Bounded CDF/paired-chart evidence projection and independent C16 guards.

Reuses prior exact-text, portable and tracking primitives; never assigns reviews.
"""
from __future__ import annotations
import argparse,collections,json,sys
import catalog,semantic,continuation15_report as shared,continuation14_report as prior
from amendment import sha,read,git
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-16'
GROUPS=read(EVIDENCE/'targets.json')
SOURCE={s['symbol_id']:s for g in GROUPS for s in g['records']}
IDS=set(SOURCE);CHECK=False
PREFIX='docs/architecture/WindowsMigrationLedger/'

def write(name,value):
    raw=(value.rstrip()+'\n').encode('utf-8');p=OUT/name
    if CHECK:assert p.is_file() and p.read_bytes()==raw,'C16 report byte drift: '+name
    else:p.write_bytes(raw)

def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))

def scope_errors(symbols,decisions,complete=True):
    errors=[];ss={s['symbol_id']:s for s in symbols}
    selected=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-16']
    actual=[d['symbol_id'] for d in selected]
    if len(actual)!=len(set(actual)) or not set(actual)<=IDS or (complete and set(actual)!=IDS):return ['Missing/duplicate/out-of-scope C16 IDs']
    for d in selected:
        ident=d['symbol_id'];s=ss.get(ident);original=SOURCE[ident]
        if s is None:errors.append('Missing source '+ident);continue
        group=next(g for g in GROUPS if ident in {r['symbol_id'] for r in g['records']})
        if d.get('review_group')!=group['group']:errors.append('Group mismatch '+ident)
        if not semantic.qualified(d):errors.append('Unqualified '+ident)
        errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'C16')
        for k,f in [('kind','source_kind'),('profile','source_profile'),('span_start','source_span_start'),('span_length','source_span_length'),('body_sha256','body_sha256'),('enclosing_symbol_id','source_enclosing_symbol_id')]:
            if s[k]!=d.get(f) or s[k]!=original[k]:errors.append('Stale source '+k+' '+ident)
        if d.get('source_sha256')!=sha(ROOT/s['path']) or d.get('source_excerpt')!=shared.excerpt(s):errors.append('Stale exact source bytes '+ident)
        if d.get('equivalence_scope')=='whole-method':errors.append('Renderer cannot claim whole numeric forward '+ident)
        if d.get('registration_class')!='explicit non-export' or json.loads(d.get('registration_evidence','[]')):errors.append('False export '+ident)
        if d.get('disposition','').startswith('extract-to-') and not all(d.get(f) for f in ('candidate_operation_id','owner_role','future_batch')):errors.append('Unowned candidate '+ident)
    return errors

def snapshot_errors(decisions):
    current={d['symbol_id']:d for d in decisions};errors=[]
    refinements={r['symbol_id']:r for r in read(EVIDENCE/'source-refinements.json')}
    for g in GROUPS:
        snap=read(EVIDENCE/('group-'+g['group']+'-member-evidence.json'))
        if {d['symbol_id'] for d in snap['decisions']}!={s['symbol_id'] for s in g['records']}:errors.append('Snapshot IDs')
        for d in snap['decisions']:
            r=refinements.get(d['symbol_id'])
            if r and r['before']!=d:errors.append('Unbound refinement')
            if current.get(d['symbol_id'])!=(r['after'] if r else d):errors.append('Unrecorded C16 review mutation')
    return errors

def preservation(history=True):
    b=read(EVIDENCE/'before.json')
    assert git('branch','--show-current')==b['branch'] and git('rev-parse','HEAD')==git('rev-parse','main')==b['head']
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    old=(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
    now=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
    assert len(json.loads(old))==1894 and not prior.prior_decision_errors(now,old)
    for label,records,base in [('tracked',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('compiled',b['compiled'],ROOT),('start snapshots',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/r['path']).is_file() and sha(base/r['path'])==r['sha256'] for r in records),label
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    historical=[a for a in b['original_artifacts'] if '/baseline/' in a['path'] or a['path'].rsplit('/',1)[-1].startswith(tuple('continuation'+str(i).zfill(2) for i in range(2,16)))]
    assert all(sha(ROOT/a['path'])==a['sha256'] for a in historical),'C01-C15 evidence drift'
    raw=read(EVIDENCE/'prior-raw-evidence-hashes.json')['files']
    if history:assert not prior.raw_history_errors(ROOT,raw)
    manifests={name:dict(path=(EVIDENCE/name).relative_to(ROOT).as_posix(),sha256=sha(EVIDENCE/name),bytes=(EVIDENCE/name).stat().st_size) for name in ('before.json','targets.json','prior-raw-evidence-hashes.json')}
    return dict(branch=b['branch'],head=b['head'],prior_ordered_literal_decisions=1894,tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(b['compiled']),baseline=len(b['baseline']['entries']),start_indexed_paths=len(b['original_artifacts']),prior_raw_files=len(raw),raw_history_checked=history,immutable_preflight_manifests=manifests,status='exact bytes preserved')

def operations(selected):
    old={r['operation_id'] for n in (14,15) for r in catalog.load(OUT,'continuation'+str(n)+'-candidate-operations.csv')}
    groups=collections.defaultdict(list);members=[]
    for d in selected:
        op=d.get('candidate_operation_id','');candidate=d['disposition'].startswith('extract-to-')
        assert candidate==bool(op)
        members.append(dict(symbol_id=d['symbol_id'],operation_id=op,owner=d['owner_role'],destination=d['destination'],future_batch=d['future_batch'],disposition=d['disposition'],member_class='candidate' if candidate else 'retained host',related_prior_operation_ids=d.get('related_operation_ids','')))
        if op:groups[op].append(d)
    rows=[]
    for op,ds in sorted(groups.items()):
        assert op in old or op in ('C16-A01-cdf-series-specification','C16-A02-graphics-label-format-policy','C16-C01-ladder-label-layout'),'Unknown or duplicate generic XY operation'
        rows.append(dict(operation_id=op,member_rows=len(ds),destination='; '.join(sorted({d['destination'] for d in ds})),approved_batch='; '.join(sorted({d['future_batch'] for d in ds})),owner='; '.join(sorted({d['owner_role'] for d in ds})),source_ids='; '.join(d['symbol_id'] for d in ds),required_tests='; '.join(d['proposed_test'] for d in ds),status='EXTENSION OF EXISTING C14/C15 OPERATION' if op in old else 'NEW PROVISIONAL GROUP',freeze='NOT FROZEN; no project-wide operation count'))
    return rows,members

def fixture_errors(rows):
    errors=[]
    if not rows or len({r['fixture_id'] for r in rows})!=len(rows):return ['Missing/duplicate fixtures']
    for r in rows:
        if not all(r.get(k) for k in ('owner','approved_batch','scope','status','fixture_id')):errors.append('Incomplete fixture')
        if r.get('source'):
            p=ROOT/r['source'];raw=p.read_bytes().decode('utf-8-sig').splitlines(keepends=True)
            body=''.join(raw[int(r['start_line'])-1:int(r['end_line'])])
            if sha(p)!=r['source_sha256'] or body!=r['declaration_and_body'] or __import__('hashlib').sha256(body.encode()).hexdigest()!=r['body_excerpt_sha256']:errors.append('Stale fixture body/source')
        elif r['status']!='NOT RUN':errors.append('False future fixture execution')
    return errors

def comparison_rows(selected):
    portable=semantic.portable_sources();rows=[]
    for d in selected:
        for field in ('portable_equivalent_pins','compared_portable_declarations'):
            for target in json.loads(d[field]):
                p=portable[target['symbol_id']]
                rows.append({**p,'source_symbol_id':d['symbol_id'],'equivalence_scope':d['equivalence_scope'] if field=='portable_equivalent_pins' else 'none verified; comparison only','body_excerpt':shared.excerpt(p),'source_sha256':sha(ROOT/p['path']),'comparison':d['equivalent_evidence'] if field=='portable_equivalent_pins' else d['source_purpose']})
    common=next(p for p in portable.values() if p['containing_type'].endswith('.OutputPlotArtifact') and p['name']=='.ctor')
    for d in selected:
        if SOURCE[d['symbol_id']]['name'] in ('AddChart','AddChartCore','AddOverlayChart','AddSeparateCharts'):
            rows.append({**common,'source_symbol_id':d['symbol_id'],'equivalence_scope':'none verified; comparison only','body_excerpt':shared.excerpt(common),'source_sha256':sha(ROOT/common['path']),'comparison':'OutputPlotArtifact only validates/stores logical referenceKey/purpose/description/caption, no coordinates, chart-series/appearance document, host renderer or executable family handler. Common registry still ANOVA/Kruskal-Wallis only.'})
    return rows

def validation():
    # Reuse the established TRX/log identity reader; discard C15 authoring metadata.
    previous=shared.EVIDENCE
    try:shared.EVIDENCE=EVIDENCE;v=shared.validation()
    finally:shared.EVIDENCE=previous
    v.pop('source_refinements',None)
    v['source_refinements']=dict(path=(EVIDENCE/'source-refinements.json').relative_to(ROOT).as_posix(),sha256=sha(EVIDENCE/'source-refinements.json'),records=len(read(EVIDENCE/'source-refinements.json')))
    v['group_checks']={g['group']:read(EVIDENCE/('group-'+g['group']+'-checks.json')) for g in GROUPS}
    v['corrections']=read(EVIDENCE/'corrections.json')
    return v

def main():
    ss=catalog.load(OUT,'symbols.csv');ds=semantic.read_store('review-decisions.json')
    errors=scope_errors(ss,ds)+snapshot_errors(ds);assert not errors,errors
    protection=preservation(history=False)
    checks=read(EVIDENCE/'final-checks.json');record=next(r for r in checks if r['id']=='continuation16-preservation')
    assert record['exit_code']==0 and sha(ROOT/record['log'])==record['sha256']
    import continuation03_report as logs
    full=json.loads(logs.text(ROOT/record['log']));assert full['raw_history_checked']
    protection.update(raw_history_checked=True,full_history_evidence=record)
    selected=[{**SOURCE[d['symbol_id']],**d} for d in ds if d.get('checkpoint_id')=='04A.01-continuation-16']
    sem=read(OUT/'semantic-review-coverage.json');coverage=read(OUT/'review-coverage.json')
    assert sem['qualified_source_reviews']==2144 and sem['unreviewed_for_acceptance']==1722
    assert len(catalog.load(OUT,'udf-exports.csv'))==268
    write('continuation16-member-reviews.csv',catalog.render(selected))
    ops,members=operations(selected)
    write('continuation16-candidate-operations.csv',catalog.render(ops));write('continuation16-operation-members.csv',catalog.render(members))
    compared=comparison_rows(selected);assert not shared.comparison_errors(compared)
    write('continuation16-portable-comparisons.csv',catalog.render(compared))
    alignment=read(EVIDENCE/'alignment-reviewed.json');assert not shared.alignment_errors(alignment)
    write('continuation16-gui-udf-alignment.csv',catalog.render(alignment))
    fixtures=read(EVIDENCE/'fixtures-reviewed.json');assert not fixture_errors(fixtures)
    write('continuation16-fixture-crosswalk.csv',catalog.render(fixtures))
    for g in GROUPS:
        group=g['group'];p=EVIDENCE/('group-'+group+'-member-evidence.json')
        protection_path=EVIDENCE/('group-'+group+('-recheck' if group=='E' else '')+'-protection.json')
        emit('continuation16-group-'+group+'.json',dict(group=group,exact_target_ids=[s['symbol_id'] for s in g['records']],source_records=g['records'],immutable_snapshot=dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p)),original_decisions=read(p)['decisions'],final_decisions=[d for d in selected if d['review_group']==group],progress=read(EVIDENCE/('progress-'+group+'.json')),checks=read(EVIDENCE/('group-'+group+'-checks.json')),corrected_checks=read(EVIDENCE/'group-E-recheck-checks.json') if group=='E' else [],protection=read(protection_path),corrections=[r for r in read(EVIDENCE/'source-refinements.json') if r['before']['review_group']==group]))
    emit('continuation16-count-deltas.json',dict(before=read(EVIDENCE/'before.json')['starting_review'],after=sem,ownership_investigations=coverage['member_investigations'],unresolved_edges=coverage['unresolved_edge_rows'],complete_controls=sem['qualified_full_control_contracts'],candidate_member_rows=sum(m['member_class']=='candidate' for m in members),deduplicated_operations=len(ops)))
    emit('continuation16-preservation.json',protection);emit('continuation16-validation.json',validation())
    # Current aggregate tables only; historical reports are never rerendered.
    current={d['symbol_id']:d for d in ds}
    candidates=[{**s,**current[s['symbol_id']]} for s in ss if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in prior.FIELDS} for s in candidates]))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in ss)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=b,allocation_evidence=e,records=n) for (f,r,d,b,e),n in sorted(reasons.items())]))
    write('continuation16.md',(EVIDENCE/'report-text.md').read_text(encoding='utf-8'))
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}|{p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}|{'docs/architecture/WindowsMigrationLedger.md',PREFIX+'continuation16-artifacts.csv'}
    assert not any('__pycache__' in p or p.endswith('.pyc') or '/bin/' in p or '/obj/' in p for p in paths)
    old={r['path']:r['sha256'] for r in read(EVIDENCE/'before.json')['original_artifacts']}
    write('continuation16-artifacts.csv',catalog.render([dict(path=p,change_from_continuation16_start='unchanged' if p in old and sha(ROOT/p)==old[p] else 'modified' if p in old else 'added',tracking='ignored; future individual force-add' if p.startswith(PREFIX) else 'visible audit artifact',hash_authority='index.json exact bytes/SHA; self excluded') for p in sorted(paths)]))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith(PREFIX) else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    if CHECK:
        import continuation03_report as ic
        assert not ic.verify_index(ROOT,read(OUT/'index.json'),sorted(paths))
        assert not prior.tracking_errors(read(OUT/'index.json'),sorted(paths),(OUT/'future-staging.txt').read_text(encoding='utf-8'))
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=len(selected),overall=2144,remaining=1722,candidate_rows=sum(m['member_class']=='candidate' for m in members),operations=len(ops),paths=len(paths))))

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--guard',action='store_true');p.add_argument('--check',action='store_true');a=p.parse_args();CHECK=a.check
    if a.guard:print(json.dumps(preservation()))
    else:main()
