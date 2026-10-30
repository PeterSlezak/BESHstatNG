"""Bounded Sankey/XYZ evidence projection; reuses established byte/pin guards.

Never infers or assigns source reviews. Historical reporters remain immutable.
"""
from __future__ import annotations
import argparse, collections, json, datetime
import catalog, semantic, continuation16_report as c16
from amendment import sha, read, git
ROOT, OUT = catalog.ROOT, catalog.DEFAULT
EVIDENCE = ROOT/'TestResults/04A-01-continuation-18'
GROUPS = read(EVIDENCE/'targets.json')
SOURCE = {s['symbol_id']:s for g in GROUPS for s in g['records']}
IDS = set(SOURCE)
PREFIX = 'docs/architecture/WindowsMigrationLedger/'
CHECK = False
NEW_OPERATIONS = {'C18-A01-sankey-physical-layout','C18-A02-sankey-category-style-resolution','C18-B01-xyz-bounds-policy','C18-B02-xyz-projected-series-geometry','C18-B03-xyz-ordered-presentation-input'}

def write(name, value):
    raw=(value.rstrip()+'\n').encode('utf-8');p=OUT/name
    if CHECK: assert p.is_file() and p.read_bytes()==raw, 'C18 byte drift: '+name
    else: p.write_bytes(raw)

def emit(name, value): write(name,json.dumps(value,indent=2,ensure_ascii=False))

def scope_errors(symbols, decisions, complete=True):
    errors=[];ss={s['symbol_id']:s for s in symbols}
    ds=[d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-18']
    actual=[d['symbol_id'] for d in ds]
    if len(actual)!=len(set(actual)) or not set(actual)<=IDS or (complete and set(actual)!=IDS): return ['Missing/duplicate/foreign C18 IDs']
    for d in ds:
        ident=d['symbol_id'];s=ss.get(ident);original=SOURCE[ident]
        if not s: errors.append('Missing source '+ident);continue
        group=next(g['group'] for g in GROUPS if ident in {r['symbol_id'] for r in g['records']})
        if d.get('review_group')!=group: errors.append('Group mismatch '+ident)
        if not semantic.qualified(d): errors.append('Unqualified '+ident)
        errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'C18')
        for k,f in [('kind','source_kind'),('profile','source_profile'),('span_start','source_span_start'),('span_length','source_span_length'),('body_sha256','body_sha256'),('enclosing_symbol_id','source_enclosing_symbol_id')]:
            if s[k]!=d.get(f) or s[k]!=original[k]:errors.append('Stale source '+k+' '+ident)
        for k,f in [('start_line','source_declared_start_line'),('end_line','source_declared_end_line'),('body_start_line','source_body_start_line'),('body_end_line','source_body_end_line')]:
            if s[k]!=d.get(f):errors.append('Stale line/body pin '+ident)
        if d.get('source_sha256')!=sha(ROOT/s['path']) or d.get('source_excerpt')!=c16.shared.excerpt(s):errors.append('Stale exact bytes '+ident)
        if SOURCE[ident]['path'].endswith('XYZscatter.vb') and SOURCE[ident]['start_line']=='9' and d.get('disposition')!='retain-Windows-host-only':errors.append('COM interface falsely portable '+ident)
        if d.get('equivalence_scope')=='whole-method':errors.append('No whole portable forwarding in target renderers '+ident)
        if d.get('registration_class')!='explicit non-export' or json.loads(d.get('registration_evidence','[]')):errors.append('False export '+ident)
        if d.get('future_crosscutting_batches')!=d.get('future_batch','')+'; proposed responsibility only, no implementation authorization':errors.append('Unscoped future-batch context '+ident)
        if d['disposition'].startswith('extract-to-') and not all(d.get(k) for k in ('candidate_operation_id','owner_role','future_batch')):errors.append('Unowned candidate '+ident)
    return errors

def readiness_errors(coverage):
    if coverage.get('status')=='READY' or coverage.get('status')=='READY FOR ACCEPTANCE':
        if coverage.get('unreviewed_for_acceptance',0) or coverage.get('unresolved_edge_rows',0) or coverage.get('qualified_full_control_contracts',0)<2056:return ['Incomplete UI/wiring/control audit falsely READY']
    return []

def snapshot_errors(decisions):
    current={d['symbol_id']:d for d in decisions};errors=[]
    changes={r['symbol_id']:r for r in read(EVIDENCE/'source-refinements.json')}
    for g in GROUPS:
        snap=read(EVIDENCE/('group-'+g['group']+'-member-evidence.json'))
        if {d['symbol_id'] for d in snap['decisions']}!={s['symbol_id'] for s in g['records']}:errors.append('Snapshot IDs')
        for d in snap['decisions']:
            change=changes.get(d['symbol_id'])
            if change and change['before']!=d:errors.append('Unbound refinement')
            if current.get(d['symbol_id'])!=(change['after'] if change else d):errors.append('Unrecorded review mutation')
    return errors

def preservation(history=True):
    scan_start=datetime.datetime.now(datetime.timezone.utc).isoformat()
    b=read(EVIDENCE/'before.json')
    assert git('branch','--show-current')==b['branch'] and git('rev-parse','HEAD')==git('rev-parse','main')==b['head']
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    old=(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json').read_bytes().decode('utf-8-sig')
    now=(semantic.TOOLING/'review-decisions.json').read_bytes().decode('utf-8-sig')
    assert len(json.loads(old))==2350 and not c16.prior.prior_decision_errors(now,old)
    frozen=read(EVIDENCE/'decision-freeze.json')
    assert sha(semantic.TOOLING/'review-decisions.json')==frozen['decision_store_sha256'],'Frozen decision edits drift'
    for a in frozen['group_files']:
        assert (ROOT/a['path']).stat().st_size==a['bytes'] and sha(ROOT/a['path'])==a['sha256'],'Frozen group drift'
    for label,records,base in [('tracked',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('compiled',b['compiled'],ROOT),('original snapshots',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/r['path']).is_file() and sha(base/r['path'])==r['sha256'] for r in records),label
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    historical=[a for a in b['original_artifacts'] if '/baseline/' in a['path'] or a['path'].rsplit('/',1)[-1].startswith(tuple('continuation'+str(i).zfill(2) for i in range(2,18)))]
    assert all(sha(ROOT/a['path'])==a['sha256'] for a in historical),'Historical checkpoint drift'
    raw=read(EVIDENCE/'prior-raw-evidence-hashes.json')['files']
    if history:assert not c16.prior.raw_history_errors(ROOT,raw)
    manifests={n:dict(path=(EVIDENCE/n).relative_to(ROOT).as_posix(),sha256=sha(EVIDENCE/n),bytes=(EVIDENCE/n).stat().st_size) for n in ('before.json','targets.json','prior-raw-evidence-hashes.json','prior-ordered-literal-decisions.json','start-integrity.json')}
    return dict(scan_started_utc=scan_start,scan_completed_utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),full_scan_expected_raw_files=len(raw),full_scan_checked_raw_files=len(raw) if history else 0,branch=b['branch'],head=b['head'],prior_ordered_literal_decisions=2350,tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(b['compiled']),baseline=len(b['baseline']['entries']),start_indexed_paths=len(b['original_artifacts']),prior_raw_files=len(raw),raw_history_checked=history,immutable_preflight_manifests=manifests,status='exact bytes preserved')

def operations(selected):
    old={r['operation_id'] for n in (14,15,16,17) for r in catalog.load(OUT,'continuation'+str(n)+'-candidate-operations.csv')}
    groups=collections.defaultdict(list);members=[]
    for d in selected:
        op=d.get('candidate_operation_id','');candidate=d['disposition'].startswith('extract-to-')
        assert candidate==bool(op),'Candidate/operation mismatch'
        if candidate: assert op in old|NEW_OPERATIONS,'Unknown/duplicate operation';groups[op].append(d)
        label='candidate' if candidate else 'compatibility facade' if d['disposition']=='compatibility-facade-to-preserve' else 'retained host'
        members.append(dict(symbol_id=d['symbol_id'],operation_id=op,owner=d['owner_role'],destination=d['destination'],future_batch=d['future_batch'],disposition=d['disposition'],member_class=label,related_prior_operation_ids=d.get('related_operation_ids','')))
    rows=[dict(operation_id=op,member_rows=len(ds),destination='; '.join(sorted({d['destination'] for d in ds})),approved_batch='; '.join(sorted({d['future_batch'] for d in ds})),owner='; '.join(sorted({d['owner_role'] for d in ds})),source_ids='; '.join(d['symbol_id'] for d in ds),required_tests='; '.join(d['proposed_test'] for d in ds),status='EXTENSION OF EXISTING OPERATION' if op in old else 'NEW PROVISIONAL GROUP',freeze='NOT FROZEN; no project-wide operation count') for op,ds in sorted(groups.items())]
    return rows,members

def comparison_rows(selected):
    portable=semantic.portable_sources();rows=[]
    for d in selected:
        for field in ('portable_equivalent_pins','compared_portable_declarations'):
            for t in json.loads(d[field]):
                p=portable[t['symbol_id']]
                rows.append({**p,'source_symbol_id':d['symbol_id'],'equivalence_scope':d['equivalence_scope'] if field=='portable_equivalent_pins' else 'none verified; comparison only','body_excerpt':c16.shared.excerpt(p),'source_sha256':sha(ROOT/p['path']),'comparison':d['equivalent_evidence'] if field=='portable_equivalent_pins' else d['source_purpose']})
    # Comparison declarations and bodies are exact portable catalog spans. No whole renderer equivalent.
    for path,typ,name in [('BESHStat.Core/src/Graphics/SankeyPlotCore.vb','SankeyPlotOptions','Copy'),('BESHStat.Core/src/Graphics/SankeyPlotCore.vb','SankeyPlotCore','ComputeNormalizedLayout'),('BESHStat.Core/src/Graphics/SankeyPlotCore.vb','SankeyPlotCore','ComputeLinkOffsets'),('BESHStat.Core/src/Graphics/SankeyPlotCore.vb','SankeyPlotCore','OrderNodes'),('BESHStat.Core/src/Graphics/SankeyPlotCore.vb','SankeyPlotCore','BuildCategoryKey'),('BESHStat.Core/src/Graphics/SankeyPlotCore.vb','SankeyPlotCore','FormatCategoryName'),('BESHStat.Core/src/Graphics/SankeyPlotCore.vb','SankeyPlotCore','ValidateOptions'),('BESHStat.Core/src/Graphics/SankeyPlotCore.vb','SankeyNode','.ctor'),('BESHStat.Core/src/Graphics/SankeyPlotCore.vb','SankeyLink','.ctor'),('BESHStat.Core/src/Graphics/SankeyPlotCore.vb','SankeyPlotResult','.ctor'),('BESHStat.Excel.Common/src/Output/OutputDocument.vb','OutputPlotArtifact','.ctor'),('BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb','OutputLayoutPlanner','Create')]:
        matches=[p for p in portable.values() if p['path']==path and p['containing_type'].endswith('.'+typ) and p['name']==name]
        assert len(matches)==1,(typ,name)
        p=matches[0];d=next(d for d in selected if SOURCE[d['symbol_id']]['name']=='AddChart')
        rows.append({**p,'source_symbol_id':d['symbol_id'],'equivalence_scope':'none verified; comparison only','body_excerpt':c16.shared.excerpt(p),'source_sha256':sha(ROOT/p['path']),'comparison':'Core copied options/immutable flow models and Common opaque plot reference/table occupancy are existing owners only. None implements Sankey physical layout, native vector renderer, XYZ projection or a production Office family handler.'})
    return rows

def fixture_errors(rows):
    errors=c16.fixture_errors(rows)
    if {r.get('fixture_id') for r in rows}!={r['fixture_id'] for r in read(EVIDENCE/'fixtures-reviewed.json')}:errors.append('Missing/foreign fixture')
    return errors

def alignment_errors(rows):
    errors=c16.shared.alignment_errors(rows)
    if {r.get('alignment_id') for r in rows}!={r['alignment_id'] for r in read(EVIDENCE/'alignment-reviewed.json')}:errors.append('Missing/foreign alignment')
    return errors

def comparison_errors(rows):
    errors=c16.shared.comparison_errors(rows)
    ds=[d for d in semantic.read_store('review-decisions.json') if d.get('checkpoint_id')=='04A.01-continuation-18']
    expected=comparison_rows(ds)
    keys=lambda rs:collections.Counter((r['source_symbol_id'],r['symbol_id'],r['equivalence_scope']) for r in rs)
    if keys(rows)!=keys(expected):errors.append('Missing/duplicate/foreign comparison link')
    return errors

def validation():
    old=c16.shared.EVIDENCE
    try:c16.shared.EVIDENCE=EVIDENCE;v=c16.shared.validation()
    finally:c16.shared.EVIDENCE=old
    v['source_refinements']=dict(path=(EVIDENCE/'source-refinements.json').relative_to(ROOT).as_posix(),sha256=sha(EVIDENCE/'source-refinements.json'),records=len(read(EVIDENCE/'source-refinements.json')))
    v['group_checks']={g['group']:read(EVIDENCE/('group-'+g['group']+'-checks.json')) for g in GROUPS}
    v['corrections']=read(EVIDENCE/'corrections.json')
    return v

def main():
    ss=catalog.load(OUT,'symbols.csv');ds=semantic.read_store('review-decisions.json')
    errors=scope_errors(ss,ds)+snapshot_errors(ds);assert not errors,errors
    protection=preservation(history=False)
    protection.pop('scan_started_utc',None);protection.pop('scan_completed_utc',None)
    checks=read(EVIDENCE/'final-checks.json');record=next(r for r in checks if r['id']=='continuation18-preservation')
    assert record['exit_code']==0 and sha(ROOT/record['log'])==record['sha256']
    import continuation03_report as ic
    full=json.loads(ic.text(ROOT/record['log']));assert full['raw_history_checked']
    protection.update(raw_history_checked=True,full_history_evidence=record)
    protection.update(full_scan_checked_raw_files=full['full_scan_checked_raw_files'],full_scan_started_utc=full['scan_started_utc'],full_scan_completed_utc=full['scan_completed_utc'])
    selected=[{**SOURCE[d['symbol_id']],**d} for d in ds if d.get('checkpoint_id')=='04A.01-continuation-18']
    sem=read(OUT/'semantic-review-coverage.json');coverage=read(OUT/'review-coverage.json')
    assert sem['qualified_source_reviews']==2494 and sem['unreviewed_for_acceptance']==1372
    assert not readiness_errors({**sem,**coverage})
    assert not [s for s in catalog.load(OUT,'remaining-member-review.csv') if s['family']!='UI']
    assert len(catalog.load(OUT,'udf-exports.csv'))==268
    write('continuation18-member-reviews.csv',catalog.render(selected))
    ops,members=operations(selected)
    write('continuation18-candidate-operations.csv',catalog.render(ops));write('continuation18-operation-members.csv',catalog.render(members))
    comparisons=comparison_rows(selected);assert not comparison_errors(comparisons)
    write('continuation18-portable-comparisons.csv',catalog.render(comparisons))
    alignment=read(EVIDENCE/'alignment-reviewed.json');assert not alignment_errors(alignment)
    write('continuation18-gui-core-alignment.csv',catalog.render(alignment))
    fixtures=read(EVIDENCE/'fixtures-reviewed.json');assert not fixture_errors(fixtures)
    write('continuation18-fixture-crosswalk.csv',catalog.render(fixtures))
    for g in GROUPS:
        group=g['group'];p=EVIDENCE/('group-'+group+'-member-evidence.json')
        emit('continuation18-group-'+group+'.json',dict(group=group,exact_target_ids=[s['symbol_id'] for s in g['records']],source_records=g['records'],immutable_snapshot=dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p)),original_decisions=read(p)['decisions'],final_decisions=[d for d in selected if d['review_group']==group],progress=read(EVIDENCE/('progress-'+group+'.json')),checks=read(EVIDENCE/('group-'+group+'-checks.json')),protection=read(EVIDENCE/('group-'+group+'-protection.json'))))
    emit('continuation18-count-deltas.json',dict(before=read(EVIDENCE/'before.json')['starting_review'],after=sem,ownership_investigations=coverage['member_investigations'],unresolved_edges=coverage['unresolved_edge_rows'],complete_controls=sem['qualified_full_control_contracts'],candidate_member_rows=sum(m['member_class']=='candidate' for m in members),deduplicated_operations=len(ops),distinct_new_provisional_operations=sum(o['status']=='NEW PROVISIONAL GROUP' for o in ops),extensions_of_existing_operations=sum(o['status'].startswith('EXTENSION') for o in ops),host_only_rows=sum(m['member_class']=='retained host' for m in members),compatibility_facade_rows=sum(m['member_class']=='compatibility facade' for m in members)))
    emit('continuation18-preservation.json',protection);emit('continuation18-validation.json',validation())
    current={d['symbol_id']:d for d in ds}
    candidates=[{**s,**current[s['symbol_id']]} for s in ss if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in c16.prior.FIELDS} for s in candidates]))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in ss)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=b,allocation_evidence=e,records=n) for (f,r,d,b,e),n in sorted(reasons.items())]))
    write('continuation18.md',(EVIDENCE/'report-text.md').read_text(encoding='utf-8'))
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}|{p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()}|{'docs/architecture/WindowsMigrationLedger.md',PREFIX+'continuation18-artifacts.csv'}
    assert not any('__pycache__' in p or p.endswith('.pyc') or '/bin/' in p or '/obj/' in p for p in paths)
    old={r['path']:r['sha256'] for r in read(EVIDENCE/'before.json')['original_artifacts']}
    write('continuation18-artifacts.csv',catalog.render([dict(path=p,change_from_continuation18_start='unchanged' if p in old and sha(ROOT/p)==old[p] else 'modified' if p in old else 'added',tracking='ignored; future individual force-add' if p.startswith(PREFIX) else 'visible audit artifact',hash_authority='index.json exact bytes/SHA; self excluded') for p in sorted(paths)]))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith(PREFIX) else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    if CHECK:
        assert not ic.verify_index(ROOT,read(OUT/'index.json'),sorted(paths))
        assert not c16.prior.tracking_errors(read(OUT/'index.json'),sorted(paths),(OUT/'future-staging.txt').read_text())
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=len(selected),overall=2494,remaining=1372,candidate_rows=sum(m['member_class']=='candidate' for m in members),operations=len(ops),paths=len(paths))))

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--guard',action='store_true');parser.add_argument('--check',action='store_true');args=parser.parse_args();CHECK=args.check
    if args.guard: print(json.dumps(preservation()))
    else: main()
