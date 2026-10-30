"""Source-only UDF helper checkpoint06; preserves all historical checkpoint renderers."""
from __future__ import annotations
import argparse,collections,json
import catalog,semantic,continuation03_report as logs,continuation05_report as prior
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-06'
TARGETS={
 'BESHstatNG/src/ExcelUDFs/ExcelArgModules.vb':('A',18),
 'BESHstatNG/src/ExcelUDFs/UDFhelpers.vb':('A',3),
 'BESHstatNG/src/ExcelUDFs/UdfOutputTables.vb':('A',21),
 'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImport.vb':('B',34),
 'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb':('C',32)}
FIELDS=prior.FIELDS+('cache_owner_sites',);CHECK=False
def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Checkpoint06 drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def historical_errors(root,records):
    errors=prior.historical_errors(root,records)
    for a in records:
        if a['path'].rsplit('/',1)[-1].startswith('continuation05'):
            p=root/a['path']
            if not p.is_file() or sha(p)!=a['sha256']:errors.append('Historical05 evidence changed: '+a['path'])
    return errors
def preservation():
    b=read(EVIDENCE/'before.json');assert git('branch','--show-current')==b['branch'] and git('rev-parse','HEAD')==b['head']
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    for label,records,base in [('protected',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('snapshot06',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/a['path']).is_file() and sha(base/a['path'])==a['sha256'] for a in records),label+' mismatch'
    errors=historical_errors(ROOT,b['original_artifacts']);assert not errors,errors
    for n in ('02','03','04','05'):
        base=ROOT/('TestResults/04A-01-continuation-'+n)
        assert all(sha(base/'original'/a['path'])==a['sha256'] for a in read(base/'before.json')['original_artifacts']),'Historical snapshot'+n
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    return dict(branch=b['branch'],head=b['head'],tracked=len(b['tracked']),gate_c=len(b['gate']),snapshot=len(b['original_artifacts']),
        baseline=len(b['baseline']['entries']),historical='02/03/04/05 reports and original snapshots unchanged')
def scope_errors(symbols,decisions,queue):
    source={s['symbol_id']:s for s in symbols};pins={d.get('symbol_id'):d for d in decisions};errors=[]
    for p,(group,count) in TARGETS.items():
        ids={s['symbol_id'] for s in queue if s['path']==p}
        if len(ids)!=count or sum(s['path']==p for s in symbols)!=count:errors.append('checkpoint06 source/queue denominator: '+p)
        for ident in ids:
            s=source.get(ident);d=pins.get(ident)
            if s is None or d is None or not semantic.qualified(s) or not semantic.qualified(d):errors.append('checkpoint06 missing/unqualified ID: '+ident);continue
            if d.get('checkpoint_id')!='04A.01-continuation-06' or d.get('review_group')!=group:errors.append('checkpoint06 incorrect group: '+ident)
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'checkpoint06')
    return errors
def export_rows(selected,exports):
    ids={r['symbol_id'] for r in exports};names=collections.defaultdict(list)
    for r in exports:names[r['symbol_id']].append(r['effective_name'])
    return [dict(symbol_id=s['symbol_id'],path=s['path'],name=s['name'],signature=s['signature'],body_sha256=s['body_sha256'],
        excel_function_export='yes' if s['symbol_id'] in ids else 'no',export_names='; '.join(names[s['symbol_id']]),
        source_role='annotated worksheet export' if s['symbol_id'] in ids else 'shared/private non-export UDF helper; public visibility is not registration') for s in selected]
def export_errors(selected,exports,rows):
    return [] if export_rows(selected,exports)==rows else ['checkpoint06 export/helper mapping drift']
def cache_owner_errors(symbols,decisions):
    errors=[];source={s['symbol_id']:s for s in symbols}
    lookup=next((d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-06' and json.loads(d['source_identity'])['path'].endswith('ExcelArgModules.vb') and json.loads(d['source_identity'])['start_line']=='265'),{})
    sites=json.loads(lookup.get('cache_owner_sites','[]'));pins=json.loads(lookup.get('dependency_pins','{}'))
    if len(sites)!=11 or len({s['path'] for s in sites})!=11:errors.append('missing/duplicate UDF cache owner site')
    for s in sites:
        if pins.get(s['path'])!=s['source_sha256'] or sha(ROOT/s['path'])!=s['source_sha256'] or s['cleanup_symbol_id'] not in source:
            errors.append('stale/unpinned UDF cache ownership: '+s['path'])
    return errors
def operation_rows(selected):
    groups=collections.defaultdict(list)
    for s in selected:
        if s['disposition'].startswith('extract-to-'):groups[s['candidate_operation_id']].append(s)
    return [dict(operation_id=k,member_rows=len(v),destination='; '.join(sorted({s['destination'] for s in v})),
        approved_batch='; '.join(sorted({s['future_batch'] for s in v})),source_ids='; '.join(sorted(s['symbol_id'] for s in v)),
        existing_equivalent_or_helper='; '.join(sorted({s['core_common_equivalent'] for s in v})),required_tests='; '.join(s['proposed_test'] for s in v),
        status='NEW PROVISIONAL GROUP' if k.startswith('C06-') else 'EXTENSION OF EXISTING GROUP; NOT A NEW OPERATION',freeze='NOT FROZEN') for k,v in sorted(groups.items())]
def operation_errors(selected,operations):return [] if operation_rows(selected)==operations else ['checkpoint06 operation mapping drift']
def validation():
    old_e,old_p=logs.EVIDENCE,logs.preservation
    try:logs.EVIDENCE=EVIDENCE;logs.preservation=preservation;value=logs.validation()
    finally:logs.EVIDENCE,logs.preservation=old_e,old_p
    value['scope']='Fresh continuation06 commands only; historical checkpoint results not counted as fresh'
    for c in value['additional_commands']:c['command']=c['command'].replace('continuation-03','continuation-06')
    value['final_current_state_commands']='TestResults/04A-01-continuation-06/final-current-checks.json; post-manifest evidence avoids recursive hashes'
    value['command_records']=read(EVIDENCE/'commands.json') if (EVIDENCE/'commands.json').exists() else []
    focused=EVIDENCE/'negative-cache-recheck.log'
    if focused.exists():
        import re
        first=logs.text(EVIDENCE/'negative-reconciliation.log');second=logs.text(focused)
        # PowerShell prefixes the first native-stderr line with python.exe :.
        first_names=set(re.findall(r'(?:^|\n|python\.exe : )(test_\w+)',first));second_names=set(re.findall(r'(?:^|\n|python\.exe : )(test_\w+)',second))
        first_counts=re.findall(r'Ran (\d+) tests? in',first);second_counts=re.findall(r'Ran (\d+) tests? in',second)
        assert first_counts and second_counts and len(first_names)==int(first_counts[-1]) and len(second_names)==int(second_counts[-1]),'Negative test identity/count reconciliation'
        value['focused_recheck']=dict(counts=second_counts,path=focused.relative_to(ROOT).as_posix(),sha256=sha(focused),
            reason='Rerun Continuation06 fixtures after complete11-cache-owner dependency pinning, plus one new owner-removal case; initial fixture results retained')
        value['distinct_negative_tests']=len(first_names|second_names)
        value['repeated_negative_tests_excluded']=len(first_names&second_names)
    value['development_notes']=[
      '108 individually transcribed reviews in A42/B34/C32; prior773 decisions preserved exactly. No other dependency member promoted.',
      'Initial atomic authoring rejected guessed CI and Cox dependency paths; corrected from actual portable/source census to StatisticalResultContracts.vb and CoxPH.vb before group persistence. Failed authoring did not change product or partially save a group.',
      'Three GroupA evidence notes refined for cached mutable CI getter and numeric enum pvalue behavior; raw progress-A-refinement.json preserves affected IDs/store hashes.',
      'Final helper lookup review pins all11 actual caller maps and DROP/PS_CLEANUP contracts; raw cache-owner-strengthening.json records amendment. Focused recheck repeats are excluded from distinct counts.',
      'Managed DayOfWeek enum TypeCode check is a .NET behavior observation, not native Excel/ExcelError runtime proof; exact source TypeCode handling and future fixtures are recorded.',
      'Core logger/settings filesystem and Windows session/cache concerns remain open GC8; no native COM, workbook, Excel/XLL or numerical golden capture.',
      'Generated VersionAttributes transient bytes preserved before exact protected restoration; certificate messages collected explicitly.']
    return value
def main():
    b=read(EVIDENCE/'before.json');oldroot=EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
    if CHECK:
        errors=logs.verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths']);assert not errors,errors
    old=catalog.load(oldroot,'symbols.csv');symbols=catalog.load(OUT,'symbols.csv');decisions=semantic.read_store('review-decisions.json')
    queue=[s for s in catalog.load(oldroot,'remaining-member-review.csv') if s['path'] in TARGETS]
    errors=scope_errors(symbols,decisions,queue)+cache_owner_errors(symbols,decisions);assert not errors,errors
    ids={s['symbol_id'] for s in queue};selected=sorted((s for s in symbols if s['symbol_id'] in ids),key=lambda s:(s['path'],int(s['span_start'])))
    assert len(selected)==len(ids)==108
    current={d['symbol_id']:d for d in decisions}
    for d in read(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json'):assert current.get(d['symbol_id'])==d,'Prior review altered'
    assert len(symbols)==3866 and len(catalog.load(OUT,'files.csv'))==171
    assert all(sha(ROOT/f['path'])==f['sha256'] for f in catalog.load(OUT,'files.csv'))
    errors=semantic.verify(symbols,catalog.load(OUT,'ui-wiring.csv'),catalog.load(OUT,'ui-controls.csv'));assert not errors,errors
    sem=read(OUT/'semantic-review-coverage.json');review=read(OUT/'review-coverage.json')
    per_file=[dict(path=p,group=g,records=sum(s['path']==p for s in symbols),before_qualified=sum(semantic.qualified(s) for s in old if s['path']==p),
        after_qualified=sum(semantic.qualified(s) for s in symbols if s['path']==p),newly_qualified=sum(s['path']==p for s in selected),
        remaining=sum(not semantic.qualified(s) for s in symbols if s['path']==p),dispositions=dict(collections.Counter(s['disposition'] for s in selected if s['path']==p))) for p,(g,n) in TARGETS.items()]
    write('continuation06-member-reviews.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in selected]))
    lookup=next(d for d in decisions if d.get('checkpoint_id')=='04A.01-continuation-06' and json.loads(d['source_identity'])['path'].endswith('ExcelArgModules.vb') and json.loads(d['source_identity'])['start_line']=='265')
    write('continuation06-cache-owner-sites.csv',catalog.render([{**r,'reader_symbol_ids':'; '.join(r['reader_symbol_ids'])} for r in json.loads(lookup['cache_owner_sites'])]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in candidates]))
    operations=operation_rows(selected);write('continuation06-candidate-operations.csv',catalog.render(operations))
    exports=catalog.load(OUT,'udf-exports.csv');old_exports=catalog.load(oldroot,'udf-exports.csv')
    assert len(exports)==len(old_exports)==268 and {e['export_id'] for e in exports}=={e['export_id'] for e in old_exports}
    mapped=export_rows(selected,exports);assert not export_errors(selected,exports,mapped)
    write('continuation06-export-helper-map.csv',catalog.render(mapped))
    export_files=[dict(path=p,source_records=n,exports_before=sum(e['path']==p for e in old_exports),exports_after=sum(e['path']==p for e in exports),
        reviewed_helper_records=sum(r['path']==p and r['excel_function_export']=='no' for r in mapped)) for p,(g,n) in TARGETS.items()]
    write('continuation06-export-counts.csv',catalog.render(export_files+[dict(path='ALL WINDOWS EXPORTS',source_records=3866,exports_before=len(old_exports),exports_after=len(exports),reviewed_helper_records=sum(r['excel_function_export']=='no' for r in mapped))]))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=x,allocation_evidence=e,records=n) for (f,r,d,x,e),n in sorted(reasons.items())]))
    deltas={}
    for key,a,z in [('member_v1',b['starting_review']['qualified_source_reviews'],sem['qualified_source_reviews']),('unreviewed',b['starting_review']['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),
      ('unpinned',b['starting_coverage']['unreviewed_static_records'],review['unreviewed_static_records']),('ownership_investigations',b['starting_coverage']['member_investigations'],review['member_investigations']),
      ('unresolved_edges',b['starting_coverage']['unresolved_edge_rows'],review['unresolved_edge_rows']),('full_controls',b['starting_review']['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),
      ('legacy_detail_gaps',b['starting_review']['legacy_pinned_reviews_requiring_details'],sem['legacy_pinned_reviews_requiring_details']),
      ('files_without_qualified_review',len(b['starting_review']['files_without_qualified_review']),len(sem['files_without_qualified_review']))]:deltas[key]=dict(before=a,after=z,delta=z-a)
    old_candidates=sum(semantic.qualified(s) and s['disposition'].startswith('extract-to-') for s in old);old_families={f['family']:f for f in b['starting_review']['families']}
    emit('continuation06-count-deltas.json',dict(counters=deltas,files=per_file,families=[dict(before=old_families[f['family']],after=f) for f in sem['families']],batch_allocations=sem['batch_allocations'],
      candidate_member_rows=dict(before=old_candidates,after=len(candidates),delta=len(candidates)-old_candidates),new_candidate_rows=sum(int(o['member_rows']) for o in operations),
      new_operation_groups=sum(o['operation_id'].startswith('C06-') for o in operations),existing_operation_extensions=[o['operation_id'] for o in operations if not o['operation_id'].startswith('C06-')],candidate_freeze='NOT FROZEN',
      export_counts=export_files,initially_noninvestigation_new_reviews=[{k:s[k] for k in ('symbol_id','path','name','disposition')} for s in old if s['symbol_id'] in ids and s['disposition']!='needs-investigation']))
    emit('continuation06-start.json',{k:b[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    val=validation();emit('continuation06-validation.json',val)
    ft='\n'.join('| '+f['group']+' | '+f['path']+' | '+str(f['before_qualified'])+' | '+str(f['after_qualified'])+'/'+str(f['records'])+' |' for f in per_file)
    ct='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    ot='\n'.join('| '+o['operation_id']+' | '+str(o['member_rows'])+' | '+o['approved_batch']+' | '+o['status']+' |' for o in operations)
    et='\n'.join('| '+e['path']+' | '+str(e['exports_before'])+' | '+str(e['exports_after'])+' | '+str(e['reviewed_helper_records'])+' |' for e in export_files)
    distinct={c:sum(int(r.get('passed',0)) for r in val['managed'] if r['configuration']==c and r['distinct_full_suite']) for c in ('Debug','Release')}
    write('continuation06.md',f'''# 04A.01 Continuation06: UDF arguments, output and range/input helpers

**PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE.** This completes108 bounded helper reviews only, not04A.01 acceptance or Gate C2. No04A.02, functional changes, extraction or new exports. E1-E7 open/GC3-GC8 deferred;40/223 owner-accepted and75 Phase04A batch IDs unchanged.

Branch `{b['branch']}`, source/main HEAD `{b['head']}`; clean tracked worktree/index, main ancestry verified. Initial index162 artifacts plus index.json (SHA dae104f987b6c42a707a237c72aceb7a873aa0ac1a1d679048d6169e42bf7d24) verified and copied to new ignored TestResults/04A-01-continuation-06/original163-path snapshot. All2109 protected tracked hashes,1543 Gate C hashes,171 compiled-source hashes,52 original baseline paths and02/03/04/05 reports/snapshots remain unchanged. No local owner ZIP exists for independent archive inclusion/diff claims. No earlier evidence recaptured or overwritten.

| Group | Source | Detailed before | Detailed after |
|---|---|---:|---:|
{ft}

A42/B34/C32 exact IDs persisted independently with store/source hashes and named fixtures. Three additional GroupA notes preserve CI caching and enum pvalue semantics in raw progress-A-refinement.json. Prior773 decisions unchanged; dependency reads did not qualify any member in the other36 UDF files, UI or Graphics. No parser/profile reduction:3866 records (3864 active/two alternative),171 compiled sources, zero diagnostics. The two alternate settings IDs remain represented/reviewed as before.

| Counter | Before | After |
|---|---:|---:|
{ct}

Global member coverage{sem['qualified_source_reviews']}/3866, UDF138/703; remaining UI1372, Graphics1048, UDF565 =2985. Exact pending IDs/current owners/batches remain in remaining-member-review.csv. No new control/wiring reviews:1/2056 full contracts,54 routing intents,1604 unresolved wiring/intent edges and2055 incomplete controls retained. Ribbon84 buttons/81 actions plus inherited LoadImage; authored76 methods/5 commands/2 pages unchanged. Native exports/RibbonXML/methods.yml/Common registry remain four authorities; Office executable families remain ANOVA/Kruskal-Wallis only.

## Export versus helper reconciliation

| Target file | Exports before | Exports after | Newly reviewed helper records |
|---|---:|---:|---:|
{et}

All Windows exports268->268, exact export IDs unchanged. Each of108 selected symbols is mapped in continuation06-export-helper-map.csv from actual ExcelFunction records. Public/Friend visibility is not registration; all five target files are helper-only, zero selected exported functions. No claim that reading their callers reviews the remaining exports. Whole registration/census checks remain mandatory.

## A: arguments, caches, errors and result projection

Each row in continuation06-member-reviews.csv binds exact source identity/type/signature/range/profile/body/dependency manifests, actual purpose/caller, mixed responsibilities, portable declaration pin or none, equivalence scope, destination/batch/owner/risk/fixture. Source-only assertions are not native or scientific parity.

ExcelArgPredicates distinguishes missing Nothing/ExcelMissing/ExcelEmpty from whitespace blank and DBNull/error/False/zero. Strict TryGetDouble rejects String/Boolean/ExcelError and nonfinite, other convertible CLR inputs use Convert; flexible parsing instead uses invariant then current culture with broad NumberStyles. Optional ints truncate and may overflow rather than default; whole/positive use1e-7 tolerance; Binary01 truncates fractions (including negative fractions near zero) to0/1 and can overflow. Flexible status numeric and textual paths therefore differ, and failed output may be-1, not initial0. Text formatting has1e-12 integer rounding/CLng overflow. Threshold vector adapter sorts/deduplicates; single-threshold checks post-dedup cardinality and never validates absent default. Keep host marker gates and legacy culture/shape policy separate from Common TypedNumericRangeParsers/SpreadsheetCellValuePolicy, whose typed-cell finite/date/error/missing contracts differ. Most normalization ownership04A.39, input adaptation04A.27; no fabricated whole Common equivalent.

Handle key conversion has no prefix/GUID/workbook check. Generic lookup delegates caller ConcurrentDictionary.TryGetValue; no map null guard or object clone. Glm/LM FIT creates prefixed GUID keys in OrdinalIgnoreCase maps; DROP removes entries. Thread-safe lookup is not fitted-object lifetime, workbook invalidation or deterministic recalc. Retain Windows session/cache seam under04A.43; no cache acquisition or native recalculation was executed. Logger error/fallback methods can themselves throw; nonblank prefix changes return from fallback object to exception text. Diagnostics policy04A.44, CoreServices can reach existing file logger under GC8. Binary converter maps only reviewed Windows singleton identity to native ExcelErrorNA; it does not normalize Core NaN or all array/error kinds.

The complete source-visible caller-map crosswalk in continuation06-cache-owner-sites.csv pins11 owners: LM, GLM, GLMNB, GEE, Cox, LMM, MMRM, multinomial, ordinal, ZIP and PSM. Ten regression DROP routes remove one key with differing invalid-handle/fault returns. PS_CLEANUP accepts blank/ALL to Count then Clear (not atomic together), or removes one key; its Catch returns exception text. This is declaration/assignment/cleanup evidence only, not a completed member review of those files, model-object synchronization or proof of workbook/session teardown.

UdfOutputTables creates spill arrays, not worksheet writes. Named/case outputs always prepend ID column and have optional header; case IDs not bounds-checked. Wrapped versus existing table slicers remove different leading rows and preserve a final row on short inputs. Core ResultTable.ToOutputModel/ResultTableOutputModel supplies sections/annotations and copied metadata lists, but Values/SetBody have aliasing. Object matrix materializer maps only Nothing/DBNull to empty text and rejects non-Object2D shapes; it extends existing C05-A02 blank/spill operation rather than adding a duplicate. A ReturnSelf title lookup is exact-first then symmetric substring and can match normalized-empty table titles.

CI text uses existing Core StatisticalResultContracts.ConfidenceIntervalResult.strConfidenceInterval caching property: first requested/generated or explicit text may be returned for later LL_to_UL request; not a pure function. Pvalue projection targets body metadata, explicit columns(all rows), or label-inferred rows, with different bounds/overlap. Numeric TypeCode branch can include enum underlying integer, so a native error enum in targeted pvalue cells can become formatted code text; plain matrix copy preserves it. Core PValuePresentation.FormatForDisplay is reused, current AppGlobals settings/culture can produce strings, nonfinite/out-of-range formatting differs from Common OutputPValuePolicy.Preview finite-probability validation and explicit culture. No duplicate Core formatter proposed. Common OutputDocument/Layout/ResultTableWriteRequest/SafeWrite are inspected comparisons; arrays alone supply no occupancy, consent, guarded write, rollback or live destination proof (E1/E5). Future fixtures cover error enum versus sentinel, exact headers/offsets, CI cache history, alpha/settings/culture, title/pvalue labels, stacking widths and unchanged source matrices.

## B/C: acquisition, row shape and Core data coordination

Get2D accepts Object2D alias or native ExcelReference xlCoerce, wrapping only a coerced scalar; raw scalar/1D/jagged/typed numeric matrix is not the same contract. Scalar-enabled coercer instead wraps arbitrary unsupported objects as a single cell, always turns native missing into1x1 ExcelEmpty even when scalarFalse, and propagates only top-level ExcelError. Cell errors are often filtered/placeholdered, not returned. No budget/cancellation/identity verification before full reference coercion; whole-column memory/read cost precedes last-nonblank trimming. This differs from reviewed DM bounded acquisition. Native xlCoerce/reference geometry and marker/error adaptation remain Windows .27/.39, not Common APIs.

Header rules are deliberately separate: numeric column immediate-second numeric, numeric matrix all-second-row numeric/no first numeric, broad grouped/complete any-column numeric anywhere below, strict vector any first nonnumeric skipped, and text header only full1048576-row ExcelReference from row0 with two nonblank initial cells. Identical array versus reference can differ; XLS65536 does not satisfy text whole-column test. Last-nonblank includes error/False/DBNull, trims only tail. Inferred names/defaults and empty-column compression preserve source order but do not retain coordinates. Explicit bad variable-name count can fall back X1.. instead of inferred names.

Independent inputs filter separately, unequal lengths permitted. Pair helpers keep only same-index complete numeric pairs; header-aware route requires equal row counts/header flags, still scans original rows. Numeric-column/matrix conversion preserves invalid interior cells as NaN; other helpers discard them. Complete-case matrix with0/1 valid row may True with matNothing and counters; with-headers route materializes one valid row but rejects zero. Grouped buckets use case-insensitive first surviving labels and can return no groupsTrue; nested3col path infers from response only. Loose String vector tokenizes comma/semicolon/space/tab, so locale comma conflicts with delimiter; matrix vector is strict and has no header handling. Shared Common configurable/paired/matrix parsers are comparisons with different minima, diagnostics, typed cell policies and provenance, not whole forwards.

GLM/GEE/predictor/Cox wrappers in B directly forward to Windows sibling builders; these are compatibility facades, not whole Core/Common equivalence. Builders independently trim each role and compare lengths, not physical coordinate identity. GLM [y,x,offset?,weight?] uses existing Core DM/GlmData.DataImportRawMatrix then cleaning and split; GEE encodes trimmed current-culture cluster text Ordinal first-appearance IDs before cleaning, then Core GeeData sorts cluster/time or relative RowIds and splits extras. Duplicate/error/category collisions, no reverse label map and unsupplied finite/weight validation need fixtures. Cox [time,status,strata?,x] uses Core CoxPHData import, exact0/1 final validation and survival records; Binary01 header truncation is not final status validation. Numeric DataObj builder accounts for local removed header via firstSourceRow1/2 but not original reference row/sheet. CoreDataTable accepts numeric strings/underlying numeric enum paths and legacy missing conversion; strict UDF numeric helpers reject them. Common model/longitudinal typed input retains different identities/diagnostics. Existing Core cleaning/design/fitting must be reused, not copied; adapters coordinate requests .27/.40, no fitter migration in this audit.

ZIP builds count/zero Core glmData separately, intersects relative RowIds, subsets, checks response within1e-7 and nonnegative count, offset finite. Offset can influence count-side deletion; no identity restored from live ranges. Count conversion may overflow outside Int32 despite Try naming; failures may leave partial ByRef arrays in other count/vector helpers. These are source-backed compatibility fixtures, not repaired bugs. Reading Core GlmData/GeeData/CoxPHData/DataObj/RowArrayUtilities or caller regressions adds no detailed review outside the108 selected IDs. E3 provenance/missing/native errors and E7 typed family promotion remain open.

## Provisional operation crosswalk

Inspected global candidate member rows {old_candidates}->{len(candidates)}, new{sum(int(o['member_rows']) for o in operations)} candidate rows. New06 groups{sum(o['operation_id'].startswith('C06-') for o in operations)}, existing-group extension(s) shown separately. Forward facades, native coercion/cache/error mapping excluded; Core CI/pvalue/cleaning algorithms are existing helper reuse. Row/initializer/lambda overlaps are member crosswalks, not extra algorithms. Global count remains NOT FROZEN.

| Operation | New member rows | Approved batch | Group status |
|---|---:|---|---|
{ot}

## Validation and residual ownership

Fresh distinct managed passes Debug{distinct['Debug']}/Release{distinct['Release']}; foundation reruns listed separately/excluded. Exact commands/exits/log/TRX SHA, Python/JS/skips, parser and negative tests, source/index/protected guards and generated transients in continuation06-validation.json. Post-freeze checks recorded separately under ignored raw evidence to avoid self-reference. Integrity errors remain1; strict incomplete remains2, not waived by complete108/108 or green product tests. No live native/numerical equivalence claim. Previous snapshots and authority bytes preserved. Generated VersionAttributes transient outputs separately retained and original protected bytes restored; certificate messages explicitly collected.

NOT RUN: Excel/COM/XLL/workbook acquisition or mutation, native UDF recalculation/spill/cache lifetime, full-column reference/shape/locale/error observations, Mac/authenticated Office, installer/sign/deploy/publish, network/package installation, update operations,04A.02 numerical/table/chart golden captures. Named future fixtures are proposals, not executed native results. Suggested native smoke only: compare strict/loose/paired UDFs on mixed text/error/header rows; FIT/get/DROP handles across workbook close/reopen and pvalue spill under culture/settings. Both remain deferred to authorized parity/wiring work.

Complete path reconciliation in continuation06-artifacts.csv; index.json excludes itself and covers all audit artifacts; proposed-tracked-files.json equals those paths plus index. Ignored ledger requires individual future force-add commands in future-staging.txt, never executed. Raw evidence/progress/snapshots under new TestResults/04A-01-continuation-06. No product/portable/test-behavior/UI/graphics/generated/project/framework/official README/manifest/catalog/Ribbon/installer/ignore changes. No stage/commit/merge/push. Only owner can accept a continuation or full04A.01/Gate C2; next numbered batch is not started.

PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE. 04A.01 remains incomplete; Gate C2 and exceptions unchanged; 04A.02 not started. Await owner review.
''')
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()};names=('continuation06-artifacts.csv','proposed-tracked-files.json','future-staging.txt')
    paths|={(OUT/n).relative_to(ROOT).as_posix() for n in names};paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()};paths.add('docs/architecture/WindowsMigrationLedger.md')
    old_paths={a['path']:a['sha256'] for a in b['original_artifacts']};artifacts=[]
    for p in sorted(paths):
        change='added' if p not in old_paths else 'modified' if not (ROOT/p).exists() or sha(ROOT/p)!=old_paths[p] else 'unchanged'
        if p in {(OUT/n).relative_to(ROOT).as_posix() for n in names}|{'docs/architecture/WindowsMigrationLedger/index.json'}:change='modified' if p in old_paths else 'added'
        artifacts.append(dict(path=p,change_from_continuation06_start=change,tracking='ignored; future individual force-add' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'visible audit tooling/index',hash_authority='index.json; self excluded'))
    write('continuation06-artifacts.csv',catalog.render(artifacts));emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus self-excluded index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=len(selected),overall=sem['qualified_source_reviews'],selected_exports=sum(r['excel_function_export']=='yes' for r in mapped),proposed_paths=len(paths))))
if __name__=='__main__':
    p=argparse.ArgumentParser(__doc__);p.add_argument('--check',action='store_true');p.add_argument('--guard',action='store_true');args=p.parse_args();CHECK=args.check
    try:
        if args.guard:print(json.dumps(preservation()))
        else:main()
    except (AssertionError,ValueError,KeyError,OSError) as e:print('INTEGRITY FAILURE: '+str(e));raise SystemExit(1)
