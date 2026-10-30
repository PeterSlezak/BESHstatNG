"""Remaining-DM checkpoint04. All historical renderers/evidence remain immutable."""
from __future__ import annotations
import argparse,collections,json
import catalog,semantic,continuation03_report as previous_report
from amendment import git,read,sha
ROOT,OUT=catalog.ROOT,catalog.DEFAULT
EVIDENCE=ROOT/'TestResults/04A-01-continuation-04'
TARGETS={
 'BESHstatNG/src/DM/ExcelDnaSharedApplicationAdapter.vb':('A',23),
 'BESHstatNG/src/DM/ExcelDnaDataImporter.vb':('A',9),
 'BESHstatNG/src/DM/ExcelDnaHostCapabilities.vb':('A',3),
 'BESHstatNG/src/DM/Helpers.vb':('A',4),
 'BESHstatNG/src/DM/PSMdataImport.vb':('B',40),
 'BESHstatNG/src/DM/UIprocedures.vb':('C',25)}
CHECK=False
FIELDS=('symbol_id','signature_key','path','containing_type','signature','kind','profile','start_line','end_line','span_start','span_length','body_sha256',
 'review_group','checkpoint_id','review_revision','disposition','responsibility','source_purpose','separable_responsibilities','core_common_equivalent',
 'equivalence_scope','equivalent_evidence','equivalence_assessment','portable_equivalent_pins','compared_portable_declarations','candidate_operation_id',
 'related_operation_ids','reusable_operation','destination','future_batch','risk','exception_tags','exception_applicability','owner_role','proposed_test',
 'runtime_boundary','source_identity','source_profile','source_span_start','source_span_length','dependency_pins','required_dependency_paths')

def write(name,value):
    value=value.rstrip()+'\n';p=OUT/name
    if CHECK:
        if not p.is_file() or p.read_text(encoding='utf-8-sig')!=value:raise ValueError('Current checkpoint drift: '+name)
    else:p.write_text(value,encoding='utf-8',newline='')
def emit(name,value):write(name,json.dumps(value,indent=2,ensure_ascii=False))

def historical_errors(root,records):
    errors=[]
    for a in records:
        name=a['path'].rsplit('/',1)[-1]
        if ('/baseline/' in a['path'] or name.startswith(('continuation02','continuation03')) or
            name in ('completion-amendment.md','amendment-validation.json','amendment-count-deltas.json','baseline-manifest.json')):
            p=root/a['path']
            if not p.is_file() or sha(p)!=a['sha256']:errors.append('Immutable historical evidence changed: '+a['path'])
    return errors

def preservation():
    before=read(EVIDENCE/'before.json');assert git('branch','--show-current')==before['branch'] and git('rev-parse','HEAD')==before['head']
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    for label,records,base in [('protected',before['tracked'],ROOT),('GateC',before['gate'],ROOT),('start snapshot',before['original_artifacts'],EVIDENCE/'original')]:
        assert all((base/r['path']).is_file() and sha(base/r['path'])==r['sha256'] for r in records),label+' hash mismatch'
    errors=historical_errors(ROOT,before['original_artifacts']);assert not errors,errors
    for n in ('02','03'):
        oldroot=ROOT/('TestResults/04A-01-continuation-'+n);old=read(oldroot/'before.json')
        assert all(sha(oldroot/'original'/a['path'])==a['sha256'] for a in old['original_artifacts']),'Prior snapshot'+n
    assert all(sha(ROOT/a['retained_path'])==a['sha256'] for a in before['baseline']['entries'])
    return dict(branch=before['branch'],head=before['head'],tracked=len(before['tracked']),gate_c=len(before['gate']),
        start_snapshot=len(before['original_artifacts']),baseline=len(before['baseline']['entries']),
        historical='All continuation02/03 indexed artifacts and their original snapshots unchanged; original52 unchanged')

def scope_errors(symbols,decisions,queue):
    errors=[];by_id={d.get('symbol_id'):d for d in decisions};source={s['symbol_id']:s for s in symbols}
    for path,(group,count) in TARGETS.items():
        ids={s['symbol_id'] for s in queue if s['path']==path}
        if len(ids)!=count:errors.append('checkpoint04 queue denominator: '+path)
        for ident in ids:
            s=source.get(ident);d=by_id.get(ident)
            if s is None or d is None or not semantic.qualified(s) or not semantic.qualified(d):
                errors.append('checkpoint04 missing/unqualified ID: '+ident);continue
            if d.get('checkpoint_id')!='04A.01-continuation-04' or d.get('review_group')!=group:
                errors.append('checkpoint04 incorrect group: '+ident)
            errors+=semantic.verify_member_contract(d,s)+semantic.verify_pins([d],'checkpoint04')
    return errors

def operation_rows(selected):
    groups=collections.defaultdict(list)
    for s in selected:
        if s['disposition'].startswith('extract-to-'):groups[s['candidate_operation_id']].append(s)
    return [dict(operation_id=k,status='NEW PROVISIONAL GROUP' if k.startswith('C04-') else 'EXTENSION OF EXISTING C03 GROUP; NOT A NEW OPERATION',
        member_rows=len(v),destination='; '.join(sorted({s['destination'] for s in v})),approved_batch='; '.join(sorted({s['future_batch'] for s in v})),
        source_ids='; '.join(sorted(s['symbol_id'] for s in v)),existing_equivalent_or_helper='; '.join(sorted({s['core_common_equivalent'] for s in v})),
        required_tests='; '.join(s['proposed_test'] for s in v),owner='; '.join(sorted({s['owner_role'] for s in v})),
        freeze_status='NOT FROZEN; group is not a unique global migration commitment') for k,v in sorted(groups.items())]

def operation_errors(selected,operations):
    expected={r['operation_id']:r for r in operation_rows(selected)};observed={r['operation_id']:r for r in operations}
    if expected!=observed or len(observed)!=len(operations):return ['checkpoint04 operation mapping drift']
    return []

def validation():
    # Reuse the historical tool's read-only log/TRX parser with a new evidence root.
    # Never call its main renderer or write any continuation03 path.
    old_evidence,old_preservation=previous_report.EVIDENCE,previous_report.preservation
    try:
        previous_report.EVIDENCE=EVIDENCE;previous_report.preservation=preservation
        val=previous_report.validation()
    finally:
        previous_report.EVIDENCE,previous_report.preservation=old_evidence,old_preservation
    val['scope']='Fresh continuation04 commands only; historical02/03 is preserved, not counted as fresh'
    for c in val['additional_commands']:c['command']=c['command'].replace('continuation-03','continuation-04')
    val['final_current_state_commands']='TestResults/04A-01-continuation-04/final-current-checks.json; run after manifest freeze, avoids self-reference'
    val['command_records']=read(EVIDENCE/'commands.json') if (EVIDENCE/'commands.json').exists() else []
    val['development_notes']=[
        '104 independently transcribed source-body reviews in three persisted groups; no generated family promotion.',
        'Uses existing continuation03 stronger schema plus checkpoint_id/review_group; all old514 detailed pins unchanged.',
        'Initial C transcription validation rejected ambiguous overloaded Evaluate reference; selected inspected exact line113 before persistence. A local Python invocation path typo was corrected. No product failure or source edit.',
        'Core private category formatter matches source body; consolidation needs deliberate API exposure, not a new algorithm or a runtime parity claim.',
        'Transient generated VersionAttributes copies retained before exact restoration; no historical evidence normalizing.',
        'No native Excel/COM/XLL or numerical-golden proof is inferred from static/fake-host suites.']
    return val

def main():
    before=read(EVIDENCE/'before.json');oldroot=EVIDENCE/'original/docs/architecture/WindowsMigrationLedger'
    if CHECK:
        errors=previous_report.verify_index(ROOT,read(OUT/'index.json'),read(OUT/'proposed-tracked-files.json')['paths']);assert not errors,errors
    queue=catalog.load(oldroot,'continuation03-remaining-dm.csv');symbols=catalog.load(OUT,'symbols.csv');old=catalog.load(oldroot,'symbols.csv')
    decisions=semantic.read_store('review-decisions.json');by_id={d['symbol_id']:d for d in decisions}
    errors=scope_errors(symbols,decisions,queue);assert not errors,errors
    for d in read(EVIDENCE/'original/scripts/windows-migration-audit/review-decisions.json'):
        assert by_id.get(d['symbol_id'])==d,'Prior detailed review altered: '+d['symbol_id']
    selected=sorted((s for s in symbols if s['symbol_id'] in {q['symbol_id'] for q in queue}),key=lambda s:(s['path'],int(s['span_start'])))
    assert len(selected)==len(queue)==104
    sem=read(OUT/'semantic-review-coverage.json');review=read(OUT/'review-coverage.json')
    assert len(symbols)==3866 and len(catalog.load(OUT,'files.csv'))==171
    assert all(sha(ROOT/f['path'])==f['sha256'] for f in catalog.load(OUT,'files.csv'))
    errors=semantic.verify(symbols,catalog.load(OUT,'ui-wiring.csv'),catalog.load(OUT,'ui-controls.csv'));assert not errors,errors
    per_file=[]
    for path,(group,count) in TARGETS.items():
        records=[s for s in symbols if s['path']==path];was=[s for s in old if s['path']==path]
        per_file.append(dict(path=path,group=group,records=len(records),newly_qualified=count,
            before_qualified=sum(semantic.qualified(s) for s in was),after_qualified=sum(semantic.qualified(s) for s in records),
            remaining_member_v1_gaps=sum(not semantic.qualified(s) for s in records),
            dispositions=dict(collections.Counter(s['disposition'] for s in records))))
    write('continuation04-member-reviews.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in selected]))
    candidates=[s for s in symbols if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    write('inspected-candidates.csv',catalog.render([{k:s.get(k,'') for k in FIELDS} for s in candidates]))
    operations=operation_rows(selected);assert not operation_errors(selected,operations)
    write('continuation04-candidate-operations.csv',catalog.render(operations))
    remaining=[s for s in symbols if s['family']=='DM' and not semantic.qualified(s)]
    remaining_fields=('symbol_id','path','start_line','end_line','signature','body_sha256','owner_role','future_batch','proposed_test','disposition')
    write('continuation04-remaining-dm.csv',catalog.render([{k:s[k] for k in remaining_fields} for s in remaining]) if remaining else ','.join('"'+k+'"' for k in remaining_fields))
    reasons=collections.Counter((s['family'],'detailed source review' if semantic.qualified(s) else 'no pinned source review',s['disposition'],s['future_batch'],'inspected' if semantic.qualified(s) else 'provisional; review owed in04A.01') for s in symbols)
    write('counts-by-reason-and-batch.csv',catalog.render([dict(family=f,review_reason=r,disposition=d,batch=b,allocation_evidence=e,records=n) for (f,r,d,b,e),n in sorted(reasons.items())]))
    deltas={}
    for k,a,b in [('member_v1',before['starting_review']['qualified_source_reviews'],sem['qualified_source_reviews']),
        ('unreviewed',before['starting_review']['unreviewed_for_acceptance'],sem['unreviewed_for_acceptance']),
        ('unpinned',before['starting_coverage']['unreviewed_static_records'],review['unreviewed_static_records']),
        ('ownership_investigations',before['starting_coverage']['member_investigations'],review['member_investigations']),
        ('unresolved_edges',before['starting_coverage']['unresolved_edge_rows'],review['unresolved_edge_rows']),
        ('full_controls',before['starting_review']['qualified_full_control_contracts'],sem['qualified_full_control_contracts']),
        ('legacy_detail_gaps',before['starting_review']['legacy_pinned_reviews_requiring_details'],sem['legacy_pinned_reviews_requiring_details']),
        ('files_without_qualified_review',len(before['starting_review']['files_without_qualified_review']),len(sem['files_without_qualified_review']))]:
        deltas[k]=dict(before=a,after=b,delta=b-a)
    old_candidates=[s for s in old if semantic.qualified(s) and s['disposition'].startswith('extract-to-')]
    prior_noninvestigations=[{k:s[k] for k in ('symbol_id','path','name','disposition')} for s in old if s['symbol_id'] in {r['symbol_id'] for r in selected} and s['disposition']!='needs-investigation']
    old_families={f['family']:f for f in before['starting_review']['families']}
    emit('continuation04-count-deltas.json',dict(counters=deltas,files=per_file,
        families=[dict(before=old_families[f['family']],after=f) for f in sem['families']],batch_allocations=sem['batch_allocations'],
        candidate_member_rows=dict(before=len(old_candidates),after=len(candidates),delta=len(candidates)-len(old_candidates)),
        initially_noninvestigation_new_reviews=prior_noninvestigations,
        new_candidate_rows=sum(int(o['member_rows']) for o in operations),new_operation_groups=sum(o['operation_id'].startswith('C04-') for o in operations),
        existing_operation_extensions=[o['operation_id'] for o in operations if not o['operation_id'].startswith('C04-')],candidate_freeze='NOT FROZEN'))
    emit('continuation04-start.json',{k:before[k] for k in ('captured_utc','branch','head','status','ignored_status','original_artifacts','starting_review','starting_coverage')})
    val=validation();emit('continuation04-validation.json',val)
    ft='\n'.join('| '+r['group']+' | '+r['path']+' | '+str(r['before_qualified'])+' | '+str(r['newly_qualified'])+' | '+str(r['after_qualified'])+'/'+str(r['records'])+' |' for r in per_file)
    ct='\n'.join('| '+k+' | '+str(v['before'])+' | '+str(v['after'])+' |' for k,v in deltas.items())
    ot='\n'.join('| '+r['operation_id']+' | '+str(r['member_rows'])+' | '+r['approved_batch']+' | '+r['status']+' |' for r in operations)
    fam='\n'.join('| '+f['family']+' | '+str(f['qualified_source_reviews'])+'/'+str(f['parsed_records'])+' | '+str(f['needs_investigation'])+' |' for f in sem['families'])
    distinct={c:sum(int(r.get('passed',0)) for r in val['managed'] if r['configuration']==c and r['distinct_full_suite']) for c in ('Debug','Release')}
    write('continuation04.md',f'''# 04A.01 continuation04: remaining DM partial checkpoint

**PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE.** This closes the104 queued DM syntax reviews only. The full04A.01 audit, owner acceptance and Gate C2 remain incomplete. E1-E7 open, GC3-GC8 deferred;40/223 accepted and75 approved Phase04A batches unchanged. No04A.02, extraction or product changes.

Branch `{before['branch']}`, source/main HEAD `{before['head']}`, clean tracked worktree and staging index. Preflight verified143 indexed artifacts plus index.json and copied144 paths to the new ignored `TestResults/04A-01-continuation-04/original/`; no prior snapshot was recaptured. All2,109 protected tracked hashes,1,543 Gate C hashes,171 compiled source hashes,52 original baseline paths, historical continuation02/03 reports and both older original snapshots remain byte-identical. No local ZIP exists; owner-supplied ZIP comparisons are historical, not rerun evidence.

## Derived per-file coverage

| Group | Source | Detailed before | Added | Detailed after |
|---|---|---:|---:|---:|
{ft}

All104 queued IDs have individual member-v1 source/signature/profile/span/body/dependency pins, using the existing stronger continuation03 schema with explicit checkpoint04/group markers. The prior514 detailed decisions, including the36 legacy upgrades and91/91 previous output-adapter records, are unchanged. Adapter has24 total records:23 newly reviewed plus previously reviewed ToGroupedNumericData. DM is196/196 across12 compiled files. `DataObjContracts.vb` contains field-only TwoGroups/MultiGroups/MmrmData DTOs, no executable initializer or method; its source hash is retained with zero invented member records. Micro-checkpoints A39/B40/C25 and exact IDs are persisted under ignored fresh evidence; they are progress records, not synthetic reviews.

| Counter | Before | After |
|---|---:|---:|
{ct}

| Family | Detailed / population | Ownership investigations |
|---|---:|---:|
{fam}

The100 investigation reduction differs from104 new reviews because GetSnapshot and three Helper forwarders (SubsetArrayByIds, CommonItems, Subset1DArrayByIds) already had provisional compatibility-facade labels; they still lacked detailed reviews. Exact prior non-investigation IDs are derived in count-deltas.json. QuickSort2D was previously an investigation. Census unchanged:171 files,3,866 source records (3,864 active/two alternative), zero diagnostics;268 UDF exports/23 exporting+18 helper files;84 Ribbon buttons/81 actions plus inherited external LoadImage;76 authored methods/5 commands/2 pages;2,056 controls and19,701 wiring/configuration rows. Four source authorities remain distinct; Office executes registered ANOVA/Kruskal-Wallis only. No control-v1 or wiring decision was added; one full contract,54 routing intents,1,604 unresolved edges and2,055 incomplete contracts remain.

## Source findings and exact portable ownership

`continuation04-member-reviews.csv` contains every new ID, actual caller/state purpose, mixed responsibilities, exact portable declaration ranges/signatures/body pins or explicit none, destination, risk, owner, exception applicability and future fixture. A dependency read is not counted as its own review. Whole-method is a source assertion; it is not execution proof.

**A: acquisition/import/host/utilities.** SharedApplicationAdapter owns borrowed native ranges, COM Areas enumeration order, separate Value2/NumberFormat/Text observations, Date1904 hint, pre/post budget/cancellation checks and exact identity/coordinate adaptation. Common `Contracts/SpreadsheetRangeReferences.vb:SpreadsheetRangeReference.FromAddress`, `Input/InputSizeLimits.vb:InputAcquisitionBudget` and cell factories are helpers, not atomic acquisition equivalence. No durable WorkbookId or selection revision is supplied; CodeName and display name checks do not prove physical same-name workbook identity. Delegate readers/selectors are injection seams; invocation does not identify live instance targets. SnapshotValues preserves exact array bounds/shape policies; ToSnapshotCell distinguishes blank/text/Boolean/error codes and rejects unsupported objects such as DateTime/ExcelMissing, while legacy Import converts ExcelEmpty/Missing/Error to missing and uses Range.Value. Cell policy Evaluate is a downstream comparison, not identical materialization. Import/CoreDataTable header/source-row/first-column semantics require captures. Four Acquire wrappers forward to a Windows adapter, not directly to portable Common.

HostCapabilities.GetSnapshot wholly forwards to Common `Host/HostCapabilities.vb:HostCapabilityProfiles.WindowsExcelDna`; EnsureCanWrite only reuses `HostCapabilityPreflight.CheckRunAndWrite` then EnsureSupported with a Windows profile. Static profile is not runtime capability certification. Helpers CommonItems and QuickSort2D wholly forward exact arguments to Core `DM/RowArrayUtilities.vb:FindCommonItems/SortRows`; SubsetArrayByIds/Subset1DArrayByIds additionally project Dictionary.Keys, so helper-only. Preserve key order, duplicates, optional comparer, in-place mutation, unstable sort and exception boundaries. No duplicate Core utility extraction.

**B: PSM.** Native Worksheet/VarColumnInfo selectors remain Windows. Common candidate is typed role/default/provenance/state coordination over existing Core `DM/DataObj.vb:DataImportRawMatrix/LoadCoreDataTable/SubsetByRowIdValues`, `RegModels/RegressionDesignCore.vb:GetRequiredRawVarKeys/BuildExpandedRegressionDataMatrix` and `RegressionFormulaDesignService.vb:TryBuildExpandedRegressionDataMatrixFromFormula`. Existing Core `StatTests/PropensityScoreMatching/PSMcore.vb:PsmInputData.Validate` and `PSMbackendOrchestrationTuning.vb:PsmComprehensiveBackend.Fit` own fitting validation/numerics. Common `Input/ModelInputCompiler.vb:SpreadsheetModelInputCompiler.Compile` is a different typed regression workflow, not an equivalent PSM importer. No matching, formula or contrast algorithm should be copied.

Worksheet import ignores its FormulaText/AbsoluteColumnLetters/allow fields and passes Nothing/Nothing/True/False/True, whereas raw import honors them. GUI GetData leaves them at effects/defaults; raw-UDF Build assigns formula/address flags. Public nondefault worksheet requests are therefore a compatibility discrepancy, not fixed here. FormulaAddressing is metadata unused directly in either importer. Validate-before-reset preserves previous state on failure; reset clears PSM state but not all inherited DataObj buffers. TermSpecs aliases caller dictionary and SourceInfo has Core copy/alias behavior. Alignment intersects row IDs but subsets children in their own existing order, not explicit model order; duplicate/reordered/orphan IDs need captures. Empty intersection leaves dropped count zero. Exact labels use culture-sensitive CStr and unescaped space-pipe-space, so delimiter collisions are possible. BuildPsmInput uses response/treatment position zero, existing Core design helpers and copies vectors; finite/binary/score validation occurs later in Core Fit. Owner: Windows PSM/Common model-input/Core PSM maintainer,04A.25 with04A.27 acquisition dependency. Named fixtures include worksheet/raw formula divergence, no-valid/duplicate-key/source-row alignment, categorical references, supplied scores, exact labels, culture and downstream Love-plot/report shapes.

**C: UI helper policy.** Windows retains actual ListBox/TextBox/MsgBox/focus/RefEdit/Excel locale and range operations. Typed selection/effect-removal/default/error/format policy is a04A.44 candidate, with04A.45 wiring fixtures and04A.27 input dependencies. Core `RegressionDesignCore.ExtractBaseKeysFromEffectText` is helper-only: UI ExtractBaseKeysForSubset has a different categorical/polynomial order and fallback. Reuse Core StripOuterDoubleQuotesPublic/RefreshTermSpecOrders rather than another formula grammar. List equality is not multiset equality (duplicate first keys throw; duplicate second keys can pass), multi-add can partially mutate before exclusion, removal cascades dependent effects and updates term order. TryParseUiDouble uses Excel separators then CurrentCulture then Invariant with Float|AllowThousands, no finite guard; empty/whitespace/ByRef output policies differ from Common `TypedNumericRangeParsers.ParseScalar/ParseVector` and finite `SpreadsheetCellValuePolicy.Evaluate`. Integer accepts integral exponent/decimal forms, list parser empty input returns one zero, ASCII spaces alone delimit tokens. CheckNumeric also colors controls and allows exact empty only. AreaCheck's current Or condition counts any non-Nothing Value, including sentinels; it is not SafeWrite/Formula-aware inspection. Preserve rather than silently repair. ConvertCategoriesToStrings has no other Windows call site; its body matches private Core `StatTests/Agreement/AgreementWeightedKappa.vb:WeightedKappaAgreement.ConvertCategoriesToStrings`. This is whole source comparison, not delegation; deliberate existing-operation exposure/consolidation is needed, not a duplicate algorithm.

E3 typed provenance/row/error/date preservation, E7 remaining-family contracts, E1 legacy output presentation and E5 exact guard/overwrite boundaries remain explicit per member. GC8 includes existing Core logger filesystem behavior under CoreServices; Core is not advertised as filesystem-free. GC6 native qualification and all other exceptions remain open/deferred. Native lifetime, delegate instance, live selection and RefEdit events are not proven by syntax.

## Provisional operations, not a frozen migration count

Inspected global candidate member rows: {len(old_candidates)} -> {len(candidates)}; newly inspected candidate rows:{sum(int(o['member_rows']) for o in operations)}. New04 groups:{sum(o['operation_id'].startswith('C04-') for o in operations)}; extensions to existing03 groups are labeled separately. C03 nine groups and119 original candidate rows remain preserved; property/initializer/accessor rows are crosswalks to operations, not additional algorithms. Four portable Helper algorithms and the private Core category formatter are reuse cases, excluded from new extraction operations. No master-plan ID or global count frozen.

| Operation ID | New member rows | Approved primary batch | Status |
|---|---:|---|---|
{ot}

## Validation, remaining queue and tracking

Fresh managed distinct passes: Debug{distinct['Debug']}, Release{distinct['Release']}; Office foundation reruns are reported separately, excluded from distinct totals. Actual commands/exits/log/TRX SHA-256, Python/Node counts/skips, negative fixtures and generated transients: `continuation04-validation.json`. Integrity failures exit1; strict remains2 because{sem['unreviewed_for_acceptance']} source reviews and the control/wiring obligations remain. No product source/test/project/framework/official README/manifest/authority/ignore-policy change. Generated VersionAttributes transient copies are retained and original bytes restored; dev-certificate messages are explicitly collected rather than assumed absent.

NOT RUN: live Excel/COM/XLL, authenticated Office/Mac, real workbook acquisition, duplicate workbook name/rename/CodeName/revision, AreaCheck sentinel/formula occupancy, RefEdit focus/teardown, locale round trips, worksheet/raw PSM reference captures, source-row reorder/zero-valid and downstream Love plots, output overwrite/chart consent/stale identity/partial writes, installer/sign/deploy and04A.02 goldens. Existing fake-host/managed passes are not native or numerical reference proof.

No DM member remains queued (`continuation04-remaining-dm.csv` header only). All non-DM missing IDs remain in global `remaining-member-review.csv`: UI/controls, Graphics, all remaining UDF semantics, RegModels and project support. Next coherent same-batch queue: model/UI caller contracts (Ui20PropensityScoreMatching and model dialogs) with defaults/reset/validation/output routing, then the other mandatory families; no implementation or next batch authorized.

`continuation04-artifacts.csv` gives every added/modified/unchanged proposed path relative to this start. `index.json` SHA/size covers all current audit artifacts except itself; `proposed-tracked-files.json` matches index paths plus index exactly. Ledger remains ignored. Individual future `git add -f -- <ledger-file>` and normal tooling commands are listed in `future-staging.txt` for owner review only; none executed. No stage/commit/merge/push.

04A.01 remains incomplete; Gate C2 and exceptions unchanged; 04A.02 was not started. Await owner review of this partial checkpoint.
''')
    paths={p.relative_to(ROOT).as_posix() for p in OUT.rglob('*') if p.is_file()}
    names=('continuation04-artifacts.csv','proposed-tracked-files.json','future-staging.txt')
    paths|={(OUT/n).relative_to(ROOT).as_posix() for n in names}
    paths|={p.relative_to(ROOT).as_posix() for p in semantic.TOOLING.iterdir() if p.is_file()};paths.add('docs/architecture/WindowsMigrationLedger.md')
    previous={a['path']:a['sha256'] for a in before['original_artifacts']};artifacts=[]
    for p in sorted(paths):
        change='added' if p not in previous else 'modified' if not (ROOT/p).exists() or sha(ROOT/p)!=previous[p] else 'unchanged'
        if p in {(OUT/n).relative_to(ROOT).as_posix() for n in names}|{'docs/architecture/WindowsMigrationLedger/index.json'}:change='modified' if p in previous else 'added'
        artifacts.append(dict(path=p,change_from_continuation04_start=change,tracking='ignored; future individual force-add' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'visible audit tooling/index',hash_authority='index.json; self excluded'))
    write('continuation04-artifacts.csv',catalog.render(artifacts))
    emit('proposed-tracked-files.json',dict(status='PROPOSED ONLY; nothing staged',paths=sorted(paths),comparison='Exact current index artifacts plus index.json; preserve ignore policy'))
    write('future-staging.txt','# Future owner-reviewed commands only; NOT EXECUTED.\n'+'\n'.join(('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(paths)))
    print(json.dumps(dict(status='PARTIAL CHECKPOINT / NOT READY FOR ACCEPTANCE',new_reviews=len(selected),overall=sem['qualified_source_reviews'],dm_remaining=len(remaining),proposed_paths=len(paths))))

if __name__=='__main__':
    parser=argparse.ArgumentParser(__doc__);parser.add_argument('--check',action='store_true');parser.add_argument('--guard',action='store_true')
    args=parser.parse_args();CHECK=args.check
    try:
        if args.guard:print(json.dumps(preservation()))
        else:main()
    except (AssertionError,ValueError,KeyError,OSError) as error:
        print('INTEGRITY FAILURE: '+str(error));raise SystemExit(1)
