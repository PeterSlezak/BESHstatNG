"""Bounded RegModels/Ribbon/generated checkpoint05; no historical renderer writes."""
from __future__ import annotations
import argparse,collections,json,xml.etree.ElementTree as ET
import catalog,semantic,continuation03_report as logs,continuation04_report as prior
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-05'
TARGETS={
 'BESHstatNG/src/RegModels/BinaryClassificationReporting.vb':('A',19),
 'BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb':('A',28),
 'BESHstatNG/BESHStatAddIn.vb':('B',90),
 'BESHstatNG/My Project/Resources.Designer.vb':('C',11),
 'BESHstatNG/My Project/Settings.Designer.vb':('C',7)}
CHECK=False
FIELDS=prior.FIELDS+('generated_provenance','ribbon_routing_contract')
def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Current checkpoint05 drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))
def historical_errors(root,records):
    errors=prior.historical_errors(root,records)
    for a in records:
        if a['path'].rsplit('/',1)[-1].startswith('continuation04'):
            p=root/a['path']
            if not p.is_file() or sha(p)!=a['sha256']:errors.append('Immutable historical04 evidence changed: '+a['path'])
    return errors
def preservation():
    b=read(EVIDENCE/'before.json')
    assert git('branch','--show-current')==b['branch'] and git('rev-parse','HEAD')==b['head']
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    for label,records,base in [('protected',b['tracked'],ROOT),('GateC',b['gate'],ROOT),('snapshot05',b['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/a['path']).is_file() and sha(base/a['path'])==a['sha256'] for a in records),label+' mismatch'
    errors=historical_errors(ROOT,b['original_artifacts']);assert not errors,errors
    for n in ('02','03','04'):
        base=ROOT/('TestResults/04A-01-continuation-'+n)
        assert all(sha(base/'original'/a['path'])==a['sha256'] for a in read(base/'before.json')['original_artifacts']),'Historical snapshot'+n
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in b['baseline']['entries'])
    return dict(branch=b['branch'],head=b['head'],tracked=len(b['tracked']),gate_c=len(b['gate']),
                snapshot=len(b['original_artifacts']),baseline=len(b['baseline']['entries']),historical='02/03/04 reports and original snapshots unchanged')
def scope_errors(symbols,decisions,queue):
    errors=[];by_id={d.get('symbol_id'):d for d in decisions};source={s['symbol_id']:s for s in symbols}
    for path,(group,count) in TARGETS.items():
        ids={s['symbol_id'] for s in queue if s['path']==path}
        if len(ids)!=count or sum(s['path']==path for s in symbols)!=count:errors.append('checkpoint05 source/queue denominator: '+path)
        for ident in ids:
            s=source.get(ident);d=by_id.get(ident)
            if s is None or d is None or not semantic.qualified(s) or not semantic.qualified(d):errors.append('checkpoint05 missing/unqualified ID: '+ident);continue
            if d.get('checkpoint_id')!='04A.01-continuation-05' or d.get('review_group')!=group:errors.append('checkpoint05 incorrect A/B/C: '+ident)
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'checkpoint05')
            if path=='BESHstatNG/BESHStatAddIn.vb' and s['name']=='AutoOpen':
                if 'BESHStat.Core/src/AppInfrastructure/AppSettings.vb' not in json.loads(d.get('required_dependency_paths','[]')):
                    errors.append('AutoOpen missing file-backed Core settings dependency: '+ident)
    return errors
def generated_errors(symbols,decisions):
    errors=[];by_id={d['symbol_id']:d for d in decisions}
    for s in symbols:
        if s['path'] not in TARGETS or TARGETS[s['path']][0]!='C':continue
        d=by_id.get(s['symbol_id'],{})
        if (d.get('disposition')!='retain-Windows-host-only' or 'generated-by-tool:' not in d.get('generated_provenance','') or
            'BESHstatNG/BESHstatNG.vbproj' not in json.loads(d.get('required_dependency_paths','[]'))):
            errors.append('invalid generated resource/settings ownership: '+s['symbol_id'])
    return errors
def ribbon_rows(symbols,decisions,canonical):
    by_id={s['symbol_id']:s for s in symbols};reviews={d['symbol_id']:d for d in decisions};result=[]
    for row in canonical:
        s=by_id[row['handler_ids']];d=reviews[s['symbol_id']];route=json.loads(d['ribbon_routing_contract'])
        result.append(dict(ribbon_id=row['ribbon_id'],action=row['onAction'],xml_line=row['xml_line'],
            symbol_id=s['symbol_id'],body_sha256=s['body_sha256'],source_path=s['path'],source_range=s['start_line']+'-'+s['end_line'],
            target_type=route['target_type'],target_source_path=route['target_source_path'],target_entry_ids='; '.join(route['target_entry_ids']),
            title_literal=route.get('title_literal',''),help_topic=route['help_topic'],startup=route['startup'],
            owner=d['owner_role'],future_batch=d['future_batch'],related_batches=d.get('related_operation_ids',''),required_fixture=d['proposed_test'],
            evidence='source-resolved declaration/entry reference; no native activation/default/control-contract proof'))
    return result
def ribbon_errors(rows,symbols,decisions,canonical):
    ids=[r['ribbon_id'] for r in rows];expected={r['ribbon_id']:r for r in canonical}
    if len(ids)!=len(set(ids)) or set(ids)!=set(expected):return ['omitted/doubly counted Ribbon callbacks']
    source={s['symbol_id']:s for s in symbols};by_id={d['symbol_id']:d for d in decisions};errors=[]
    for row in rows:
        c=expected[row['ribbon_id']];s=source.get(row['symbol_id']);d=by_id.get(row['symbol_id'])
        if s is None or d is None or row['action']!=c['onAction'] or row['symbol_id']!=c['handler_ids'] or row['body_sha256']!=s['body_sha256']:
            errors.append('Ribbon source declaration drift: '+row['ribbon_id']);continue
        route=json.loads(d.get('ribbon_routing_contract') or '{}')
        if (route.get('action')!=row['action'] or row['ribbon_id'] not in route.get('xml_ids',[]) or
            route.get('target_type')!=row['target_type'] or route.get('target_source_path')!=row['target_source_path'] or
            route.get('startup')!=row['startup'] or any(i not in source for i in route.get('target_entry_ids',[]))):
            errors.append('Ribbon target/startup evidence drift: '+row['ribbon_id'])
    return errors
def operation_rows(selected):
    groups=collections.defaultdict(list)
    for s in selected:
        if s['disposition'].startswith('extract-to-'):groups[s['candidate_operation_id']].append(s)
    return [dict(operation_id=k,member_rows=len(v),destination='; '.join(sorted({s['destination'] for s in v})),
        approved_batch='; '.join(sorted({s['future_batch'] for s in v})),source_ids='; '.join(sorted(s['symbol_id'] for s in v)),
        existing_equivalent_or_helper='; '.join(sorted({s['core_common_equivalent'] for s in v})),
        required_tests='; '.join(s['proposed_test'] for s in v),status='PROVISIONAL; not a frozen global migration count') for k,v in sorted(groups.items())]
def operation_errors(selected,operations):
    return [] if operation_rows(selected)==operations else ['checkpoint05 operation mapping drift']
def validation():
    old_e,old_p=logs.EVIDENCE,logs.preservation
    try:
        logs.EVIDENCE=EVIDENCE;logs.preservation=preservation;value=logs.validation()
    finally:logs.EVIDENCE,logs.preservation=old_e,old_p
    value['scope']='Fresh continuation05 commands; historical evidence not counted as fresh'
    final_negative=EVIDENCE/'negative-reconciliation-final.log'
    if final_negative.exists():
        import re
        value['superseded_initial_negative_test_counts']=value['negative_test_counts']
        value['negative_test_counts']=re.findall(r'Ran (\d+) tests? in',logs.text(final_negative))
        value['final_negative_log']=dict(path=final_negative.relative_to(ROOT).as_posix(),sha256=sha(final_negative),note='Full rerun after directly pinning Core file-backed settings dependency and adding its removal fixture; repeats excluded from distinct totals')
    for c in value['additional_commands']:c['command']=c['command'].replace('continuation-03','continuation-05')
    value['final_current_state_commands']='TestResults/04A-01-continuation-05/final-current-checks.json; post-manifest log outside indexed ledger avoids self-reference'
    value['command_records']=read(EVIDENCE/'commands.json') if (EVIDENCE/'commands.json').exists() else []
    value['development_notes']=[
        '155 individually authored source reviews; old618 decisions unchanged. Source reading did not confer reviews on caller dependencies.',
        'Related route slices corrected against Revision2.1 before C checkpoint; raw batch-map-corrections.json preserves before/after16 allocations. No primary .45 native callback ownership changed.',
        'Final dependency review additionally pins Core AppSettings.vb in AutoOpen and tests its removal; raw settings-pin-strengthening.json preserves store hashes. Full final negative rerun supersedes initial52-test run, not added to it.',
        'Read-only shell searches with wildcards/quoted paths required correction; no product/validation failure or data mutation resulted.',
        'No family-generated manual promotion or denominator reduction. Existing continuation03 source/dependency/comparison guards retained.',
        'Generated resources/settings individually read; two alternative-profile IDs retained. Zero-record support files receive file notes only.',
        'Transient generated VersionAttributes copies retained before exact original restoration; any certificate messages collected explicitly.',
        'No new native, scientific/numerical golden or owner acceptance proof.']
    return value
def main():
    b=read(EVIDENCE/'before.json');oldroot=EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
    if CHECK:
        errors=logs.verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths']);assert not errors,errors
    old=catalog.load(oldroot,'symbols.csv');symbols=catalog.load(OUT,'symbols.csv');decisions=semantic.read_store('review-decisions.json')
    queue=[s for s in catalog.load(oldroot,'remaining-member-review.csv') if s['path'] in TARGETS]
    errors=scope_errors(symbols,decisions,queue)+generated_errors(symbols,decisions);assert not errors,errors
    ids={s['symbol_id'] for s in queue};selected=sorted((s for s in symbols if s['symbol_id'] in ids),key=lambda s:(s['path'],int(s['span_start'])))
    assert len(selected)==len(ids)==155
    by_id={d['symbol_id']:d for d in decisions}
    for d in read(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json'):assert by_id.get(d['symbol_id'])==d,'Prior review altered'
    assert len(symbols)==3866 and len(catalog.load(OUT,'files.csv'))==171
    assert all(sha(ROOT/f['path'])==f['sha256'] for f in catalog.load(OUT,'files.csv'))
    errors=semantic.verify(symbols,catalog.load(OUT,'ui-wiring.csv'),catalog.load(OUT,'ui-controls.csv'));assert not errors,errors
    sem=read(OUT/'semantic-review-coverage.json');review=read(OUT/'review-coverage.json')
    per_file=[dict(path=p,group=g,records=sum(s['path']==p for s in symbols),before_qualified=sum(semantic.qualified(s) for s in old if s['path']==p),
        after_qualified=sum(semantic.qualified(s) for s in symbols if s['path']==p),newly_qualified=sum(s['path']==p for s in selected),
        remaining=sum(not semantic.qualified(s) for s in symbols if s['path']==p),dispositions=dict(collections.Counter(s['disposition'] for s in selected if s['path']==p))) for p,(g,n) in TARGETS.items()]
    write('continuation05-member-reviews.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in selected]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in candidates]))
    operations=operation_rows(selected);write('continuation05-candidate-operations.csv',catalog.render(operations))
    canonical=catalog.load(OUT,'ribbon-crosswalk.csv');routes=ribbon_rows(symbols,decisions,canonical)
    errors=ribbon_errors(routes,symbols,decisions,canonical);assert not errors,errors
    xml=ET.parse(ROOT/'BESHstatNG/Resources/RibbonXML.xml')
    buttons=[e.attrib for e in xml.iter() if e.tag.endswith('}button') and e.get('onAction')]
    assert {(e['id'],e['onAction']) for e in buttons}=={(r['ribbon_id'],r['action']) for r in routes}
    write('continuation05-ribbon-targets.csv',catalog.render(routes))
    notes=[]
    for p,producer,consumer in [
        ('BESHstatNG/Generated/VersionAttributes.vb','BESHstatNG.vbproj GenerateVersionAttributes using Directory.Build.props BeshVersion/build metadata','Assembly version/file/informational/build date consumers; generated transients separately preserved then original restored'),
        ('BESHstatNG/My Project/Application.Designer.vb','Visual Studio generated checked-in header; empty body in current source','Compiled project support; no executable routines; no inferred WindowsForms lifecycle in active MyType Windows'),
        ('BESHstatNG/My Project/AssemblyInfo.vb','Checked-in assembly declarations, not claimed newly tool-generated','Windows COM GUID/visibility, identity/resource metadata and test InternalsVisibleTo; version declarations supplied separately')]:
        assert not any(s['path']==p for s in symbols)
        notes.append(dict(path=p,parsed_records=0,sha256=sha(ROOT/p),owner='Windows project/build maintainer',producer=producer,consumer=consumer,
            disposition='retain-Windows-host-only',future_batch='04A.44',fixture='Assembly/resource/version and XLL bitness packaging; no installer/native execution in audit'))
    write('continuation05-zero-record-support.csv',catalog.render(notes))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=x,allocation_evidence=e,records=n) for (f,r,d,x,e),n in sorted(reasons.items())]))
    deltas={}
    for key,a,z in [('member_v1',b['starting_review']['qualified_source_reviews'],sem['qualified_source_reviews']),('unreviewed',b['starting_review']['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),
        ('unpinned',b['starting_coverage']['unreviewed_static_records'],review['unreviewed_static_records']),('ownership_investigations',b['starting_coverage']['member_investigations'],review['member_investigations']),
        ('unresolved_edges',b['starting_coverage']['unresolved_edge_rows'],review['unresolved_edge_rows']),('full_controls',b['starting_review']['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),
        ('legacy_detail_gaps',b['starting_review']['legacy_pinned_reviews_requiring_details'],sem['legacy_pinned_reviews_requiring_details']),
        ('files_without_qualified_review',len(b['starting_review']['files_without_qualified_review']),len(sem['files_without_qualified_review']))]:deltas[key]=dict(before=a,after=z,delta=z-a)
    old_candidates=sum(semantic.qualified(s) and s['disposition'].startswith('extract-to-') for s in old)
    old_families={f['family']:f for f in b['starting_review']['families']}
    emit('continuation05-count-deltas.json',dict(counters=deltas,files=per_file,families=[dict(before=old_families[f['family']],after=f) for f in sem['families']],
        batch_allocations=sem['batch_allocations'],candidate_member_rows=dict(before=old_candidates,after=len(candidates),delta=len(candidates)-old_candidates),
        new_candidate_rows=sum(int(o['member_rows']) for o in operations),new_operation_groups=len(operations),candidate_freeze='NOT FROZEN',
        initially_noninvestigation_new_reviews=[{k:s[k] for k in ('symbol_id','path','name','disposition')} for s in old if s['symbol_id'] in ids and s['disposition']!='needs-investigation']))
    emit('continuation05-start.json',{k:b[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    val=validation();emit('continuation05-validation.json',val)
    ft='\n'.join('| '+f['group']+' | '+f['path']+' | '+str(f['before_qualified'])+' | '+str(f['after_qualified'])+'/'+str(f['records'])+' |' for f in per_file)
    ct='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    fam='\n'.join('| '+f['family']+' | '+str(f['qualified_source_reviews'])+'/'+str(f['parsed_records'])+' | '+str(f['needs_investigation'])+' |' for f in sem['families'])
    ot='\n'.join('| '+o['operation_id']+' | '+str(o['member_rows'])+' | '+o['approved_batch']+' |' for o in operations)
    distinct={c:sum(int(r.get('passed',0)) for r in val['managed'] if r['configuration']==c and r['distinct_full_suite']) for c in ('Debug','Release')}
    write('continuation05.md',f'''# 04A.01 Continuation05: RegModels/Ribbon/project support

**PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE.** All155 bounded source records reviewed, not the full04A.01 audit. No functional implementation, migration, source changes or04A.02. Gate C2 not met; E1-E7 open and GC3-GC8 deferred,40/223 accepted and75 approved Phase04A batches unchanged.

Branch `{b['branch']}`, HEAD/main `{b['head']}`. Tracked worktree/staging clean, main ancestry verified. Start index SHA acb685a72e958ead8dd6508bc316750b6d07b149e9d0941d03e7e97d9e53081a matched all152 artifact sizes/hashes; new153-path original snapshot under ignored TestResults/04A-01-continuation-05. No earlier snapshot recaptured.2,109 tracked-protected,1,543 Gate C,171 compiled input hashes,52 original baseline paths and all02/03/04 reports/original snapshots preserved. No local ZIP comparison rerun: owner archive is historical evidence; no inferred missing ZIP paths replace actual checkout.

| Group | Source | Detailed before | Detailed after |
|---|---|---:|---:|
{ft}

Progress A47/B90/C18, all155 IDs durable with source/profile/span/signature/body/dependency/portable declaration pins; before/after store/source hashes in raw progress-A/B/C.json. Prior618 detailed decisions including Matrix56, DM196 and Infrastructure/Help/Update unchanged. Two alternative Settings records retained. Zero-record VersionAttributes/Application.Designer/AssemblyInfo notes in continuation05-zero-record-support.csv, no invented IDs. Generated wrappers individually read and explicitly retain host/tool provenance.

| Counter | Before | After |
|---|---:|---:|
{ct}

| Family | Detailed / population | Investigations |
|---|---:|---:|
{fam}

The132 investigation reduction differs from155 new reviews because23 previously pending records already had provisional non-investigation labels: five binary forwards and18 generated resources/settings; exact prior IDs in count-deltas.json. No residual targeted member IDs remain. Census3866 (3864 active/two alternative),171 files, zero diagnostics;268 UDF exports/23 exporting+18 helper files;84 Ribbon buttons/81 distinct actions plus inherited LoadImage;76 methods/5 commands/2 pages;2056 controls/19701 wiring rows unchanged. methods.yml, RibbonXML, ExcelFunction and Common registry remain separate; Office runnable analyses are ANOVA/Kruskal-Wallis only. One full control contract,54 traced routing intents,1604 unresolved edges and2055 incomplete control contracts remain; callback review adds no WinForms closure.

## A: source-specific equivalence and risks

BinaryClassificationReporting.ComputeBinarySummary, BuildThresholdTable, ComputeBrierScore, BuildCalibrationBins and WrapResults directly and wholly forward identical arguments/defaults/results to Core `BESHStat.Core/src/RegModels/BinaryClassificationCore.vb` methods of the same names. Core owns weighted counts, p>=threshold ties, finite/probability/binary checks, calibration and zero-denominator NaN. These are compatibility facades, not duplicate algorithm extractions. Exact declaration signatures/ranges/body hashes appear in continuation05-member-reviews.csv.

Spill builders have different shapes/policies from Core ResultTable/Common OutputDocument: crosstab4x4 plus optional header, threshold14 columns, calibration6, calibration points8 with always-header, Brier2 columns/optional rows. Percent helper handles nonfinite before100x but other fields can retain NaN/Infinity. Private identity sentinel is not native error or text N/A; UDFhelpers.ConvertBinaryClassificationOutputForExcel maps only that singleton to ExcelErrorNA at scalar/cell positions. PrepareResultTableForUdfLocal maps Nothing/DBNull to empty string only. Calibration argument aliases/default-on-unknown are broader/different than Core bin-method validation and native missing recognition stays Windows. Candidate policy batch04A.39/42, related classification report04A.09/output04A.28. No invented general NaN-to-error conversion or native spill proof.

ROC partition classifies y>=0.5 versus other values, preserving probability order, rejects null/length/empty/single-class only; no finite/probability/binary validation locally. BuildRocFromBinaryProbabilities still constructs Windows Graphics.ROC and calls compute(alpha); current inspected Core census has no complete ROC equivalent. Windows compute still owns AUC/Hanley-McNeil/DeLong and cutoff computation, with transitive Core utilities only. Planned04A.31 numerical extraction/reuse is distinct from existing BinaryClassificationCore. AddRocResultsAndPlot shifts cursor/writes tables then native chart, no local safe-write/rollback; late failure can leave output. Reading ROC/writer dependencies does not qualify their own remaining IDs. Named future fixtures cover classes/ties/weights, invalid/single-class/one-observation ROC, exact labels/shape, alpha and native placement.

MixedModelFrontEndHelpers is typed policy/projection, not a second fitter. ParseFit/Inference/Optimizer/Gradient preserve exact synonyms, Unicode alphanumeric normalization, invariant Object conversion and caller-selected defaults on unknown; AUTO gradient returns caller default rather than always Auto. Residual structure factory/UsesVisitIndex are Core helpers but caught errors default False after possible logging. DataObj projections retain FinalData row order and aliases; CDbl conversion/finite/error/source-row provenance differs from Common typed parser. Required-key/effect expansion reuses Core RegressionDesignCore.GetCoefBaseName/GetRequiredRawVarKeys/BuildExpandedPredictorMatrix, always omitCategoricalReference=True; Core algorithm not re-extracted. Intercept/name helpers preserve alias-versus-copy and mismatch behavior. Exact dictionary finite dedup differs from Core MixedModelPostEstimation.UniqueSortedFiniteValues tolerant dedup; class labels mix invariant/current-culture parse, near-equality, G6 Core FormatProfileValue and case-insensitive sorting. Trace preserves empty lines; random covariance admission uses exact legacy labels/optional UI enforcement and unused randomNames. Whole forward equivalence not asserted for mixed helpers. Ownership04A.14-16, input04A.27 and effect04A.08 dependencies; exact fixtures per ID include locale, invalid/default aliases, source rows, no-intercept, numeric/text levels and random design counts. Numerical goldens deferred to separately authorized04A.02.

## B: host lifecycle and Ribbon targets

continuation05-ribbon-targets.csv is a derived crosswalk, not another authority:84 actual XML IDs map81 source actions and individually pinned target/constructor/Populate/Help contracts. Three reused callbacks retained. Native handlers remain Windows, primary04A.45; specialized related form/model/graphics batches follow actual routes. Literal legacy strings remain including Descriptive Statistcs, Scatter Plot MatrixType and Liner Mixed Models. RegStart maps six exact titles to HelpTopic; unknown leaves0. Workbook/ActiveSheet paths can create workbook and assume Worksheet even if chart sheet active. PCA family passes sh in constructor; MMRM/LMM/GEE/PSM/Cox explicitly Populate, SPC handlers assign but do not pass sh or call Populate. Form defaults/control contracts are still separate unfinished work, not silently reviewed through click methods.

AutoOpen orders native exception registration/app/XLL/path, WinForms Core services, quasi-separation delegate, Core SimpleFileLogger, diagnostics, settings/default fallback and background update. Logger setup occurs outside settings Try; failure can leave partial initialization. SimpleFileLogger still creates directories/writers/timer in Core (GC8), not filesystem-free. Actual Core IAppLogger/NullLogger and Common preference/diagnostic contracts exist; full lifecycle remains native, no duplicate policy extraction proposed. Quasi-separation lambda MsgBox has no buttons style (default OK-only), returns<>vbNo; source does not prove an effective cancellation choice. AutoClose disposes logger then clears diagnostics/callback/app, but does not visibly cancel update task, queued export, dispose forms/RefEdit or unregister exception handler; earlier Finally failure can interrupt later cleanup. UnhandledExceptionHandler logs before UI Try, tries log form, fallback MsgBox and #VALUE; logging/fallback itself may fail. Core error reporters can transitively log via file sink; exceptions remain open.

GetCustomUI ignores RibbonID and returns generated My.Resources.RibbonXML. Resources.resx references the actual UTF8 RibbonXML.xml and RefEdit0/1 bitmaps; old truncated generated XML comment is not the value authority. Inherited LoadImage remains external ExcelRibbon.LoadImage(String), installed assembly metadata/hash evidence retained separately, never a fabricated local callback or native proof. Export is queued Excel macro then reads current app/chart availability, creates new form and native-window owner; shared _exportChartForm is unused, no single-instance protection. Related route batches corrected against Revision2.1 in raw batch-map-corrections.json before C checkpoint; primary callback ownership unchanged.

## C: generated support

Resources StronglyTypedResourceBuilder17.0.0.0 wrappers cache ResourceManager(BESHStatNG.Resources,assembly), expose mutable culture override and GetObject bitmap/GetString XML. No custom equivalent in Core/Common; generated ownership retained with all11 records. SettingsSingleFileGenerator17.14.0.0 wraps ApplicationSettingsBase.Synchronized singleton. Active MyType=Windows getters do not attach Shutdown; WindowsForms alternative lock initializer/AutoSaveSettings and conditional Default accessor remain union-pinned. No detach is visible; application-lifetime auto-save only in alternative profile. Settings.settings is empty and differs from custom preference store. Embedded-resource/Compile producer alignment is source-checked; runtime assembly manifest/culture/bitness load requires later smoke, not a hidden static acceptance blocker or fake native evidence.

## Provisional operation crosswalk

Inspected candidate rows {old_candidates}->{len(candidates)}, new rows{sum(int(o['member_rows']) for o in operations)} grouped into{len(operations)} provisional operations; host callbacks/generated entries/whole forwards excluded. Prior C03/C04 groups/history retained. Counts are member rows versus deduplicated proposed operations, never frozen global migrations.

| Operation | Member rows | Approved batch |
|---|---:|---|
{ot}

## Validation, residuals and tracking

Fresh distinct managed passes Debug{distinct['Debug']}/Release{distinct['Release']}; foundation reruns separate and excluded. Actual commands/exits/log/TRX hashes, Python/Node counts/skips, parser/negative tests and transient generated file hashes are in continuation05-validation.json. Final post-freeze report/check/tracking/Git records stay in fresh ignored evidence to avoid hash recursion. Integrity corruption exits1; full acceptance remains strict2 because{sem['unreviewed_for_acceptance']} source records and mandatory control/wiring work remain. E1 output, E3 provenance, E5 guards, E7 family contracts, GC6 native qualification and GC8 file/injection lifetimes explicit; all E1-E7/GC3-GC8 unchanged.

No product, test-behavior, project/framework/API/official README/resource/generated/settings/authority/installer/ignore-policy edit. Build-generated VersionAttributes transients preserved and original bytes restored exactly; certificate-message collection included, no assumed normalization. No staging/commit/merge/push. All indexed paths plus index.json equal proposed tracking; ignored ledger requires individual future git add -f commands, listed but never executed. continuation05-artifacts.csv reconciles every added/modified/unchanged proposed path; raw evidence under TestResults/04A-01-continuation-05 is ignored and separate.

NOT RUN: native Excel/COM/XLL or live Workbook; Ribbon first launch with missing/unwritable settings/logs, no/active/chart-sheet workbook, double click/reopen, quasi-separation choice, log fallback, update-start failure/queued unload, Help/export, all model-family defaults/help; bitness/resource manifests; Mac/authenticated Office; installers/sign/publish/deploy/network installation;04A.02 golden reference collection. Static declarations and green managed/fake fixtures do not prove numerical/native UX equivalence.

RegModels103/103 and project-support108/108 source reviews are complete, but UI/Designer/control contracts, Graphics and UDF semantic/host analysis remain mandatory. Exact3093 pending IDs/current owners/batches are retained in global remaining-member-review.csv. Next coherent same-batch review: UI model/dialog source and full option/default/reset/validation/output contracts, preserving the remaining Graphics/UDF backlog. Owner alone decides acceptance of the eventual full04A.01, not this partial checkpoint.

Continuation 05 is a source-verified partial 04A.01 checkpoint only. Await owner review; nothing staged/committed, Gate C2 and exceptions unchanged, 04A.02 not started.
''')
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}
    names=('continuation05-artifacts.csv','proposed-tracked-files.json','future-staging.txt');paths|={(OUT/n).relative_to(ROOT).as_posix() for n in names}
    paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()};paths.add('docs/architecture/WindowsMigrationLedger.md')
    previous={a['path']:a['sha256'] for a in b['original_artifacts']};artifacts=[]
    for p in sorted(paths):
        change='added' if p not in previous else 'modified' if not (ROOT/p).exists() or sha(ROOT/p)!=previous[p] else 'unchanged'
        if p in {(OUT/n).relative_to(ROOT).as_posix() for n in names}|{'docs/architecture/WindowsMigrationLedger/index.json'}:change='modified' if p in previous else 'added'
        artifacts.append(dict(path=p,change_from_continuation05_start=change,tracking='ignored; future individual force-add' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'visible audit tooling/index',hash_authority='index.json; self excluded'))
    write('continuation05-artifacts.csv',catalog.render(artifacts));emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact indexed artifacts plus self-excluded index.json'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY',new_reviews=len(selected),overall=sem['qualified_source_reviews'],ribbon=len(routes),proposed_paths=len(paths))))
if __name__=='__main__':
    p=argparse.ArgumentParser(__doc__);p.add_argument('--check',action='store_true');p.add_argument('--guard',action='store_true');args=p.parse_args();CHECK=args.check
    try:
        if args.guard:print(json.dumps(preservation()))
        else:main()
    except (AssertionError,ValueError,KeyError,OSError) as e:print('INTEGRITY FAILURE: '+str(e));raise SystemExit(1)
