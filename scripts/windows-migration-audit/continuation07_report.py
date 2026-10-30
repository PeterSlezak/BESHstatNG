"""Source-only specialized UDF import checkpoint07; historical reports stay immutable."""
from __future__ import annotations
import argparse,collections,hashlib,json,re
import catalog,semantic,continuation03_report as logs,continuation06_report as prior
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-07'
BASE='BESHstatNG/src/ExcelUDFs/UDFdataImport/'
TARGETS={BASE+p: (g,n) for p,g,n in (
 ('UdfDataImportOptions.vb','A',8),('UdfDataImportRegressionOptions.vb','A',5),('UdfDataImportFormula.vb','A',5),
 ('UdfDataImportMixedModel.vb','B',18),('UdfDataImportAgreement.vb','C',22),('UdfDataImportContingency.vb','C',1),
 ('UdfDataImportMultivariate.vb','C',5),('UdfDataImportPlot.vb','D',5),('UdfDataImportPsm.vb','D',3),('UdfDataImportSurvival.vb','D',5))}
FIELDS=prior.FIELDS+('source_kind','source_enclosing_symbol_id');CHECK=False
def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Checkpoint07 drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def historical_errors(root,records):
    errors=prior.historical_errors(root,records)
    for a in records:
        if a['path'].rsplit('/',1)[-1].startswith('continuation06'):
            p=root/a['path']
            if not p.is_file() or sha(p)!=a['sha256']:errors.append('Historical06 evidence changed: '+a['path'])
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
    for label,records,base in [('protected',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('snapshot07',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/a['path']).is_file() and sha(base/a['path'])==a['sha256'] for a in records),label+' mismatch'
    errors=historical_errors(ROOT,b['original_artifacts']);assert not errors,errors
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    files=catalog.load(OUT,'files.csv');assert len(files)==171 and all(sha(ROOT/f['path'])==f['sha256'] for f in files)
    raw=read(EVIDENCE/'prior-raw-evidence-hashes.json')['files'];errors=raw_history_errors(ROOT,raw);assert not errors,errors
    return dict(branch=b['branch'],head=b['head'],tracked=len(b['tracked']),gate_c=len(b['gate']),compiled=len(files),snapshot=len(b['original_artifacts']),baseline=len(b['baseline']['entries']),prior_raw_files=len(raw),historical='02/03/04/05/06 reports, snapshots and all recorded raw files unchanged')
def scope_errors(symbols,decisions,queue):
    source={s['symbol_id']:s for s in symbols};pins={d.get('symbol_id'):d for d in decisions};errors=[]
    for p,(group,count) in TARGETS.items():
        ids={s['symbol_id'] for s in queue if s['path']==p}
        if len(ids)!=count or sum(s['path']==p for s in symbols)!=count:errors.append('checkpoint07 source/queue denominator: '+p)
        for ident in ids:
            s=source.get(ident);d=pins.get(ident)
            if s is None or d is None or not semantic.qualified(s) or not semantic.qualified(d):errors.append('checkpoint07 missing/unqualified ID: '+ident);continue
            if d.get('checkpoint_id')!='04A.01-continuation-07' or d.get('review_group')!=group:errors.append('checkpoint07 incorrect group: '+ident)
            if d.get('body_sha256')!=s['body_sha256'] or d.get('signature_key')!=s['signature_key']:errors.append('checkpoint07 top-level source pin drift: '+ident)
            if d.get('source_kind')!=s['kind'] or d.get('source_enclosing_symbol_id')!=s.get('enclosing_symbol_id',''):errors.append('checkpoint07 kind/enclosing drift: '+ident)
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'checkpoint07')
    return errors
export_rows=prior.export_rows
def export_errors(selected,exports,rows):
    errors=[] if export_rows(selected,exports)==rows else ['checkpoint07 export/helper mapping drift']
    # This closed scope was independently checked against real declarations; it contains no exports.
    if any(e['symbol_id'] in {s['symbol_id'] for s in selected} for e in exports):errors.append('checkpoint07 helper unexpectedly declared exported; investigate source drift')
    return errors
def operation_rows(selected):
    values=prior.operation_rows(selected)
    for v in values:v['status']='NEW PROVISIONAL GROUP' if v['operation_id'].startswith('C07-') else 'EXTENSION OF EXISTING GROUP; NOT A NEW OPERATION'
    return values
def operation_errors(selected,operations):
    errors=[] if operation_rows(selected)==operations else ['checkpoint07 operation mapping drift']
    existing={r['operation_id'] for n in ('03','04','05','06') for r in catalog.load(OUT,'continuation'+n+'-candidate-operations.csv')}
    for o in operations:
        if not o['operation_id'].startswith('C07-') and o['operation_id'] not in existing:errors.append('Unknown claimed prior operation: '+o['operation_id'])
    return errors
def validation():
    old_e,old_p=logs.EVIDENCE,logs.preservation
    try:logs.EVIDENCE=EVIDENCE;logs.preservation=preservation;value=logs.validation()
    finally:logs.EVIDENCE,logs.preservation=old_e,old_p
    value['scope']='Fresh continuation07 commands only; numerical/native parity not executed'
    value['development_notes']=[
      'All77 bodies including nested lambdas individually inspected; rich immutable A/B/C/D evidence plus explicit two-row C optional-parser source recheck retained.',
      'Tool development: first rich snapshot call used nonexistent catalog.rows; corrected to csv.DictReader before creating evidence. First C correction attempted legacy row identity access; corrected checkpoint filter before any store write.',
      'Two current C07 optional-parser rows corrected after exact GetOptionalDouble/Int reread: strict numeric, not permissive text; reference existing operation C06-A03. Original C snapshot preserved and correction before/after retained.',
      'Initial70 audit negatives:69 passed,1 new fixture failed because it called the detail validator for a top-level body pin checked separately by the analyzer. Strengthened07 reporter top-level body/signature check, corrected fixture to exercise that guard, then reran all8 affected07 fixtures; original failure retained.',
      'No product edits; portable equivalent pins assert source reuse, not numerical or native execution. No new whole-equivalence assertions.',
      'Generated transient VersionAttributes and any certificate output are explicitly listed; protected originals restored byte-for-byte.']
    for c in value['additional_commands']:c['command']=c['command'].replace('continuation-03','continuation-07')
    value['final_current_state_commands']='TestResults/04A-01-continuation-07/final-current-checks.json; raw post-index checks avoid recursive hashes'
    value['command_records']=read(EVIDENCE/'commands.json') if (EVIDENCE/'commands.json').exists() else []
    focused=EVIDENCE/'negative-focused07.log'
    if focused.exists():
        first=logs.text(EVIDENCE/'negative-reconciliation.log');second=logs.text(focused)
        names=lambda t:set(re.findall(r'(?:^|\n|python\.exe : )(test_\w+)',t))
        a,z=names(first),names(second);assert len(a)==70 and len(z)==8 and z<=a
        assert 'Ran 8 tests' in second and re.search(r'\nOK\s*$',second)
        value['negative_recheck']=dict(path=focused.relative_to(ROOT).as_posix(),sha256=sha(focused),passed=8,failed=0,repeated_identities=8,new_identities=0,initial_passed=69,initial_failed=1,corrected_fixture='test_stale_source_dependency_portable_and_enclosing_fail')
        value['distinct_negative_tests']=len(a|z)
    return value
def main():
    b=read(EVIDENCE/'before.json');oldroot=EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
    if CHECK:
        errors=logs.verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths']);assert not errors,errors
    old=catalog.load(oldroot,'symbols.csv');symbols=catalog.load(OUT,'symbols.csv');decisions=semantic.read_store('review-decisions.json')
    queue=[s for s in catalog.load(oldroot,'remaining-member-review.csv') if s['path'] in TARGETS]
    errors=scope_errors(symbols,decisions,queue);assert not errors,errors
    ids={s['symbol_id'] for s in queue};selected=sorted((s for s in symbols if s['symbol_id'] in ids),key=lambda s:(s['path'],int(s['span_start'])))
    assert len(selected)==len(ids)==77
    current={d['symbol_id']:d for d in decisions}
    for d in read(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json'):assert current.get(d['symbol_id'])==d,'Prior review altered'
    assert len(symbols)==3866 and len(catalog.load(OUT,'files.csv'))==171
    errors=semantic.verify(symbols,catalog.load(OUT,'ui-wiring.csv'),catalog.load(OUT,'ui-controls.csv'));assert not errors,errors
    sem=read(OUT/'semantic-review-coverage.json');review=read(OUT/'review-coverage.json')
    per_file=[dict(path=p,group=g,records=sum(s['path']==p for s in symbols),before_qualified=sum(semantic.qualified(s) for s in old if s['path']==p),after_qualified=sum(semantic.qualified(s) for s in symbols if s['path']==p),remaining=sum(not semantic.qualified(s) for s in symbols if s['path']==p),dispositions=dict(collections.Counter(s['disposition'] for s in selected if s['path']==p))) for p,(g,n) in TARGETS.items()]
    # Merge only extra audit fields; source CSV remains the source identity authority.
    rich=[{**s,**{k:v for k,v in current[s['symbol_id']].items() if k not in s}} for s in selected]
    write('continuation07-member-reviews.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in rich]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in candidates]))
    operations=operation_rows(selected);assert not operation_errors(selected,operations)
    write('continuation07-candidate-operations.csv',catalog.render(operations))
    exports=catalog.load(OUT,'udf-exports.csv');old_exports=catalog.load(oldroot,'udf-exports.csv')
    assert len(exports)==len(old_exports)==268 and {e['export_id'] for e in exports}=={e['export_id'] for e in old_exports}
    mapped=export_rows(selected,exports);assert not export_errors(selected,exports,mapped)
    write('continuation07-export-helper-map.csv',catalog.render(mapped))
    export_files=[dict(path=p,source_records=n,exports_before=sum(e['path']==p for e in old_exports),exports_after=sum(e['path']==p for e in exports),reviewed_helpers=sum(r['path']==p and r['excel_function_export']=='no' for r in mapped)) for p,(g,n) in TARGETS.items()]
    write('continuation07-export-counts.csv',catalog.render(export_files+[dict(path='ALL WINDOWS EXPORTS',source_records=3866,exports_before=len(old_exports),exports_after=len(exports),reviewed_helpers=len(mapped))]))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=x,allocation_evidence=e,records=n) for (f,r,d,x,e),n in sorted(reasons.items())]))
    deltas={}
    for key,a,z in [('member_v1',b['starting_review']['qualified_source_reviews'],sem['qualified_source_reviews']),('unreviewed',b['starting_review']['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),('unpinned',b['starting_coverage']['unreviewed_static_records'],review['unreviewed_static_records']),('ownership_investigations',b['starting_coverage']['member_investigations'],review['member_investigations']),('unresolved_edges',b['starting_coverage']['unresolved_edge_rows'],review['unresolved_edge_rows']),('full_controls',b['starting_review']['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),('legacy_detail_gaps',b['starting_review']['legacy_pinned_reviews_requiring_details'],sem['legacy_pinned_reviews_requiring_details']),('files_without_qualified_review',len(b['starting_review']['files_without_qualified_review']),len(sem['files_without_qualified_review']))]:deltas[key]=dict(before=a,after=z,delta=z-a)
    old_candidates=sum(semantic.qualified(s) and s['disposition'].startswith('extract-to-') for s in old);old_families={f['family']:f for f in b['starting_review']['families']}
    emit('continuation07-count-deltas.json',dict(counters=deltas,files=per_file,families=[dict(before=old_families[f['family']],after=f) for f in sem['families']],batch_allocations=sem['batch_allocations'],candidate_member_rows=dict(before=old_candidates,after=len(candidates),delta=len(candidates)-old_candidates),new_candidate_rows=sum(int(o['member_rows']) for o in operations),new_operation_groups=sum(o['operation_id'].startswith('C07-') for o in operations),existing_operation_extensions=[o['operation_id'] for o in operations if not o['operation_id'].startswith('C07-')],candidate_freeze='NOT FROZEN',export_counts=export_files,initially_noninvestigation_new_reviews=[{k:s[k] for k in ('symbol_id','path','name','disposition')} for s in old if s['symbol_id'] in ids and s['disposition']!='needs-investigation']))
    emit('continuation07-start.json',{k:b[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    val=validation();emit('continuation07-validation.json',val)
    ft='\n'.join('| '+f['group']+' | '+f['path']+' | '+str(f['before_qualified'])+' | '+str(f['after_qualified'])+'/'+str(f['records'])+' |' for f in per_file)
    ct='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    ot='\n'.join('| '+o['operation_id']+' | '+str(o['member_rows'])+' | '+o['approved_batch']+' | '+o['status']+' |' for o in operations)
    distinct={c:sum(int(r.get('passed',0)) for r in val['managed'] if r['configuration']==c and r['distinct_full_suite']) for c in ('Debug','Release')}
    write('continuation07.md',f'''# 04A.01 Continuation07 — specialized UDF import semantics

**CONTINUATION 07 COMPLETE — PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE.** All77 bounded source records are qualified; the whole04A.01 audit remains incomplete. No04A.02, refactor, extraction, product change, acceptance or Gate C2 closure. E1–E7 open, GC3–GC8 deferred,40/223 accepted and75 approved Phase04A batches unchanged.

Branch `{b['branch']}`, HEAD/main `{b['head']}`; no tracked changes or staged files. Original173 indexed artifacts plus index copied as174 paths to new ignored TestResults/04A-01-continuation-07/original, index SHA4041ec68e14f83c9af551e51369a616be48acf5e1a932ebe6a37e41f4a83044f. Guards check2109 protected tracked files,1543 Gate C files,171 compiled hashes,52 original baseline paths,02–06 indexed history and14554 prior raw evidence files. No local owner ZIP is available for an independent archive comparison. Raw A/B/C/D detailed decision snapshots and one explicit C optional-parser correction preserve review evolution rather than replacing history.

| Group | Source | Before | After |
|---|---|---:|---:|
{ft}

Exactly18/18/28/13 group records; source kind, enclosing ID, type/signature/span/profile/body/dependency and portable comparison pins retained per ID, including nested lambdas. Previous881 store decisions unchanged. No dependency read qualifies another source ID. Census3866=3864 active+2alternative,171 compiled files and zero parser diagnostics unchanged.

| Counter | Before | After |
|---|---:|---:|
{ct}

UDF215/703 detailed,488 remaining; UI1372 and Graphics1048 remain, total2908 pending. Exact source IDs, reason, owner and provisional batch queue: remaining-member-review.csv.1/2056 full controls and1604 unresolved wiring/intent edges unchanged; no routing/control closure inferred from helper review.268 ExcelFunction exports unchanged; all77 reviewed IDs are non-export helpers, independently mapped against actual annotations in continuation07-export-helper-map.csv (per-file before/after export totals in continuation07-export-counts.csv). Ribbon84buttons/81actions plus inherited LoadImage, authored76methods/5commands/2pages and Common registry remain separate authorities; Office runnable analyses remain ANOVA/Kruskal–Wallis only.

## A — option grammar and formula addressing

Histogram key removes ASCIIspace/hyphen/underscore; regression key keeps Unicode letters/digits only; agreement keys retain underscores. They are distinct legacy grammars. Histogram aliases select existing Core HistogramBinningCore labels, not new numerics or PrettyBreaks. Thresholds sort and neighbor-deduplicate1e-12; explicit margins need not be ordered. Alpha parser resets missing to0.05, tries invariant then current culture for strings, and can leave invalid ByRef state onFalse. Core NumericGuards comparisons do not establish whole forwards. Family/link aliases and silent defaults precede actual Core family-link validation; existing FamilyUtils canonical-link call is helper-only.

Formula absolute columns require direct ExcelReference/width and reuse Core RegressionVariableCatalog.NumberToLetters (RegressionFormulaParser.vb); neutral SpreadsheetRangeReference is not native provenance. CSV empty-name positions differ from range blanks; duplicate names retained. Prediction candidate keys reuse RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromDesignSpec, but check expanded-name COUNT only, not identity/order. Exact declarations/body hashes are in member CSV/store; source inspection is not scientific parity. .39/.40 own option/request policies, .27 native range boundary. GC8 applies to existing Core design/parser logging, not every string helper.

## B — MMRM and LS-mean profile contracts

Independent header trimming followed by length checks does not prove physical row identity. Formula selected columns are cleaned jointly with y/subject/optionalvisit; no original source-row IDs retained, only dropped count. Relative formulas parse but keep ALL x columns; names/absolute paths prune required keys and restore original column order. Direct reference is mandatory for absolute addressing. Core RegressionVariableCatalog.Build and RegressionFormulaParser.TryParseFormulaToDesignSpec are existing helper reuse; actual design/fit remains downstream. Common LongitudinalParsers.ParseMixedModel has explicit header choices, typed provenance, configurable duplicate-visit/order validation, so no whole equivalence.

CSV name holes versus range blanks, first ordinal BaseKey match, unique-key/order lambdas, fallback Xnames and two-pass fitted-column matching each have their own source decision. LS-mean spec requires finite weights, visits/profile values, skips allblank rows and retains previous components on late failure. AT parsing probes headerless name/value before headered name/value or single-data-row wide forms; attempted headerless failure blocks fallback. Visit/time reserved names and duplicate normalized design slots rejected, with local partial mutation. Fitted handle is runtime-owned; reading caller declarations does not certify cache lifetime. Future .40 fixtures cover alias collisions, duplicate visits, invalid unused x, header offsets, partial ByRef state and AT ambiguity; numerical goldens deferred.

## C — agreement, contingency and CA/MCA

Agreement pairs require matching numeric-header heuristics even for categorical inputs. Numeric pairs jointly drop invalids; blank category or SD errors are checked only on retained pairs. Empty numeric results can succeed, categorical empty returns Num versus dimension/header Value. ICC input is ragged DATA ROWS, not independent columns. Category strings/ranges differ on blank slots and preserve duplicates. Option enums use actual Core Agreement declarations, but alias/default/throw policies remain separate. ParseAlphaOrDefault supplied default is overwritten to0.05 by TryParseAlpha on missing. Optional double/int use strict numeric GetOptionalDouble/Int; integer truncates and CInt can overflow; rejected numeric strings/default seeds are preserved, corrected explicitly after reread.

BlandAltman request builder locally permits2 pairs but existing Core constructor requires3 and logs; option clamps/defaults and subject IDs precede existing BlandAltmanAgreement.Fit. No duplicate algorithm. Plot-data spill is header+4columns with literal empty-string padding, assumes matched X/Y array lengths, not a Common output-document whole equivalent.

Contingency/CA integer import accepts distance-to-bankers-round<=1e-7, CInt can overflow, lateFalse may leave partial allocated arrays and allzero is accepted here. Common RectangularCategoricalParsers.ParseContingencyCounts instead requires exact integers, Int64/total validation, explicit labels/diagnostics; policy mismatch pinned. MCA default assumes header when varNames is missing, can drop first ordinary category row; explicit bad names still change header default. Blank/error category text retained. Existing Core CA.data/DataMultiple remain numerical/data engine consumers, not worksheet policy equivalents. Multivariate numeric string vectors permit successful parsed NaN/Infinity despite finite comment, unlike strict range branch; invariant-before-current parsing and delimiter choice are fixture obligations. .39 owns agreement/count admission; .41 agreement requests; .42 CA/MCA; .28 projection. No repairs performed.

## D — ROC, PSM and survival

ROC can discard one extra status row, guesses positive class before invalid marker deletion, normalizes labels and treats all nonpositive nonblank labels as negative; explicit absent positive accepted until empty-class failure. Lower direction negates markers. PlotDataUDFs still invokes Windows graphics.ROC; no claim of wholesale Core migration. .42 fixtures cover class collisions, multiclass/default/absentpositive, order and direction.

PSM builds raw[treatment,covariates] plus separately trimmed outcome/score/ID/exact inputs. Existing DM psmData.DataImportFromRawMatrices performs child RowId intersection and actual formula expansion; worksheet path explicitly passes formulaNothing/relativeTrue/absoluteFalse instead. UDF names mode allows relative tokens unlike GLM/MMRM names-only flags. FirstSourceRow1/2 is relative to selected array, not physical Excel row/sheet identity. Exact-group header inferred by row COUNT only; existing joined exact labels use unescaped ` | ` so collisions remain a named fixture. No duplicate PSM matching/design extraction. .42 request/acquisition extends already inspected .25 DM operations; .27 physical input provenance remains pending.

KM_TABLE skips negative times and maps groups OrdinalIgnoreCase while preserving each spelling; plot KM rejects negative before status validation and uses Core case-sensitive groups. Median rejects negative after valid status, logrank only after valid status/nonblank group; blank strata ->ALL. Scalar coercion facade remains Windows. Existing Core Survival.CreatSurvivalData, Survival_KM_LR.BrookmeyerCrowleyMedianSurvivalCI and WeightedLogRankTest are helper-only reuse; Common LongitudinalParsers.ParseSurvival has different typed header/diagnostic policy. Median projection emits native NA for nonestimable cells and assumes group order matches Core. Error/throw/logger and first-seen group behavior need fixtures; GC8 logging remains open. No native result or numerical-equivalence claim.

## Provisional operations and validation

Candidate member rows {old_candidates}->{len(candidates)}; selected candidate rows {sum(int(o['member_rows']) for o in operations)}; new provisional groups {sum(o['operation_id'].startswith('C07-') for o in operations)}. Existing-group extensions shown separately; accessors/lambdas/overlapping orchestration do not become independent algorithms. Project-wide operation count NOT FROZEN.

| Operation | Member rows | Approved batch | Status |
|---|---:|---|---|
{ot}

Fresh full-suite managed passes Debug{distinct['Debug']}, Release{distinct['Release']}; Office foundation repeats excluded from distinct totals. Exact commands/exits/TRX/log hashes, Python/Node/fail/skip counts, parser assertions, negatives, source/strict/integrity checks and generated-transient/certificate observations: continuation07-validation.json. Final current index/SHA/tracking/branch/status checks are recorded separately under TestResults/04A-01-continuation-07/final-current-checks.json to avoid recursive hash dependencies. Strict remains2 for real incomplete source/control/wiring review; integrity drift remains1. Historical passing logs are not fresh test results.

NOT RUN: Excel/COM/XLL/native workbook/spill/recalculation/cache execution, Mac/authenticated Office, installer/sign/deploy/publish/network install/update,04A.02 numerical goldens. Future native smoke only: compare same array/direct-reference formula selection and headers; compare KM table/plot/logrank invalid rows/group case and PSM worksheet/raw formula requests. Fixtures named per ID are future tests, not executed parity.

continuation07-artifacts.csv lists complete added/modified/unchanged paths relative to start; index excludes itself, proposed tracking equals indexed paths+index. Ignored ledger requires individual future force-add commands in future-staging.txt; none executed. No source/framework/generated/resource/settings/Designer/Ribbon/catalog/installer/official README/ignore edits. Nothing staged/committed/merged/pushed. Owner acceptance remains separate.

04A.01 remains incomplete; Gate C2 and exceptions unchanged; 04A.02 was not started. Await owner review of this partial checkpoint.
''')
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()};names=('continuation07-artifacts.csv','proposed-tracked-files.json','future-staging.txt')
    paths|={(OUT/n).relative_to(ROOT).as_posix() for n in names};paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()};paths.add('docs/architecture/WindowsMigrationLedger.md')
    old_paths={a['path']:a['sha256'] for a in b['original_artifacts']};artifacts=[]
    for p in sorted(paths):
        change='added' if p not in old_paths else 'modified' if not (ROOT/p).exists() or sha(ROOT/p)!=old_paths[p] else 'unchanged'
        if p in {(OUT/n).relative_to(ROOT).as_posix() for n in names}|{'docs/architecture/WindowsMigrationLedger/index.json'}:change='modified' if p in old_paths else 'added'
        artifacts.append(dict(path=p,change_from_continuation07_start=change,tracking='ignored; future individual force-add' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'visible audit tooling/index',hash_authority='index.json; self excluded'))
    write('continuation07-artifacts.csv',catalog.render(artifacts));emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus self-excluded index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=len(selected),overall=sem['qualified_source_reviews'],selected_exports=sum(r['excel_function_export']=='yes' for r in mapped),proposed_paths=len(paths))))
if __name__=='__main__':
    p=argparse.ArgumentParser(__doc__);p.add_argument('--check',action='store_true');p.add_argument('--guard',action='store_true');args=p.parse_args();CHECK=args.check
    try:
        if args.guard:print(json.dumps(preservation()))
        else:main()
    except (AssertionError,ValueError,KeyError,OSError) as e:print('INTEGRITY FAILURE: '+str(e));raise SystemExit(1)
