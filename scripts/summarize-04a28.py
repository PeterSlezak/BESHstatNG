"""Batch-qualified receipts and owner dossier; never rewrites an earlier authority."""
from collections import Counter
import copy
import importlib.util
import json
from pathlib import Path
import re
import sys

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    spec=importlib.util.spec_from_file_location(name,ROOT/path)
    m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
c=module('capture28','scripts/capture-04a28-contract.py')
r=module('runner28','scripts/test_suite_rationalization.py')
RUN,OUT=c.RUN,c.OUT
def load(path):return json.loads(Path(path).read_bytes())
def put(name,data):
    path=OUT/name
    assert path.parent==OUT
    path.write_text(json.dumps(c.clean(data),ensure_ascii=False,allow_nan=False,separators=(',',':'))+'\n',encoding='utf-8')
def log(name,stream='stdout'):return (RUN/'commands'/name/(stream+'.log')).read_text(encoding='utf-8',errors='replace')
NEWFILES=['BESHStat.Excel.Common.Tests/src/Output28PresentationTests.vb','BESHStat.Excel.Common.Tests/src/Output28SafetyAndAxisTests.vb','BESHStatNG.Test/src/DataManagement/Output28AdapterTests.vb','BESHStat.OfficeAddin/tests/Output28ContractTests.cs']
def prepare():
    methods=[]
    for path in NEWFILES:
        for name in re.findall(r'(?:Public Sub|public void) (\w+)\(', (ROOT/path).read_text()):
            if name=='New':continue
            methods.append(dict(source=path,method=name,owner='04A28 software output compatibility',behavior=re.sub(r'([a-z])([A-Z])',r'\1 \2',name),source_sha256=c.sha(ROOT/path),runtimes=['net10.0','net48'] if 'Common.Tests' in path else ['net10.0'] if path.endswith('.cs') else ['net48']))
    put('new-test-inventory.json',dict(schema_version=1,status='DECLARED_CASE_GROUPS_NOT_RUNTIME_COUNTS',managed_methods=methods,proof='NOT_GOLDEN / SAME_CORE_OUTPUT_PRESENTATION_COMPATIBILITY_ONLY'))

def identities(suites):
    return Counter((s['suite'],row['class_name'],row['method'],row['test_name']) for s in suites for row in s['records'])
def summarize(label):
    matrix=load(RUN/(label+'-Debug-matrix.json'))
    # Accepted accounting binds project/source lookup by this exact suite order.
    suites=[load(RUN/'executions'/(label+'-Debug-'+suite)/'cases.json') for suite in ['Core','OfficeAddin','OfficeExecution','Common','Legacy','ControlMitigation']]
    for suite in suites:
        suite['path']=(RUN/'executions'/(label+'-Debug-'+suite['suite'])/'results'/(suite['suite']+'.trx')).relative_to(ROOT).as_posix()
    accounting=module('accounting28','scripts/summarize-04a17.py')
    groups=accounting.typed_groups(copy.deepcopy(suites))
    weight=accounting.prior.weight
    logical=dict(total=sum(weight(k) for k in groups),passed=sum(weight(k) for k,v in groups.items() if all(row['outcome']=='Passed' for row in v)))
    logical['nonpassed']=logical['total']-logical['passed']
    raw=Counter(row['outcome'] for suite in suites for row in suite['records'])
    prior=load(ROOT/'evidence/TestRationalization/test-inventory-v1.json')
    phase=prior['phase_columns'].index('after-Debug');before=Counter()
    for suite,ci,mi,ni,oi in prior['managed_cases']:
        key=(suite,prior['managed_classes'][ci],prior['managed_methods'][mi],prior['managed_test_names'][ni])
        before[key]+=sum(prior['managed_outcome_patterns'][oi][phase].values())
    after=identities(suites);removed=before-after;added=after-before
    nonpasses=[dict(suite=suite['suite'],**row) for suite in suites for row in suite['records'] if row['outcome']!='Passed']
    old_validation=load(ROOT/'evidence/TestRationalization/validation-v1.json')['phase_results']['after-Debug']
    old_nonpass={(row['suite'],row['class_name'],row['method'],row['test_name']) for row in old_validation['raw_managed']['nonpasses']}
    new_nonpasses=[row for row in nonpasses if (row['suite'],row['class_name'],row['method'],row['test_name']) not in old_nonpass]
    python_total=0;python_nonpass={};python_events=Counter();python_ids={}
    for suffix in ['python-tools','python-help','python-scripts','common-python']:
        text=log(label+'-Debug-'+suffix,'stderr')
        counts=re.findall(r'^Ran (\d+) tests? in',text,re.M)
        if len(counts)!=1:raise ValueError('Unqualified Python receipt: '+suffix)
        python_total+=int(counts[0])
        for outcome,method,identity in re.findall(r'^(FAIL|ERROR): (\w+) \(([^()]+)\)',text,re.M):python_nonpass[identity]=outcome
        for method,identity,outcome in re.findall(r'^(test\w+) \(([^()]+)\) \.\.\. (ok|FAIL|ERROR|skipped[^\n]*)\s*$',text,re.M):python_ids[identity]=outcome
        python_events.update({k:int(v) for k,v in re.findall(r'(failures|errors|skipped)=(\d+)',text.split('\nRan ')[-1])})
    python_ids.update(python_nonpass)
    old_python={identity:outcomes[phase] for identity,outcomes in prior['python_methods']}
    inherited_py={key:value for key,value in python_nonpass.items() if old_python.get(key)==value}
    new_py={key:value for key,value in python_nonpass.items() if old_python.get(key)!=value}
    node=Counter()
    for suffix in ['office-node','resolver','wasm-node']:
        text=log(label+'-Debug-'+suffix)
        for field in ['tests','pass','fail','cancelled','skipped','todo']:
            matches=re.findall(r'\b'+field+r' (\d+)\s*$',text,re.M)
            if len(matches)!=1:raise ValueError('Unqualified Node receipt: '+suffix+' '+field)
            node[field]+=int(matches[0])
    release=load(RUN/(label+'-Release-selected.json'))
    new_managed=[dict(suite=s['suite'],**row) for s in suites for row in s['records'] if 'Output28' in row['class_name']]
    inventory=load(OUT/'new-test-inventory.json');inventory.update(status='EXECUTED',debug_cases=new_managed,release_cases=[dict(suite=s['suite'],**row) for s in release for row in s['records'] if 'Output28' in row['class_name']],python_methods={k:v for k,v in python_ids.items() if k.startswith('test_04a28_output.')})
    put('new-test-inventory.json',inventory)
    build_debug=load(RUN/'builds'/label/'Debug/build.json');build_release=load(RUN/'builds'/label/'Release/build.json')
    compact_matrix={k:v for k,v in matrix.items() if k!='lanes'}
    compact_matrix['lanes']=[dict(lane=lane['lane'],seconds=lane['seconds'],results=[{k:v for k,v in result.items() if k!='records'} for result in lane['results']]) for lane in matrix['lanes']]
    debug=dict(configuration='Debug',raw_managed=dict(total=sum(raw.values()),outcomes=dict(raw)),logical_shared_source=logical,node=dict(node),python=dict(total=python_total,passed=python_total-len(python_nonpass)-python_events.get('skipped',0),method_nonpasses=dict(Counter(python_nonpass.values())),failure_events=dict(python_events)),suites=[dict(suite=s['suite'],outcomes=s['outcomes'],counters=s['counters'],trx_sha256=s['sha256'],command=s['command']) for s in suites],matrix=compact_matrix,build=build_debug,aggregate_exit=1 if r.failed(matrix) else 0)
    put('debug-full-validation.json',debug)
    put('debug-entry-final-test-identities.json',dict(entry='Qualified accepted merged maintenance receipts, not a fresh full entry execution',entry_inventory_sha256=c.sha(ROOT/'evidence/TestRationalization/test-inventory-v1.json'),entry_counts=old_validation,entry_fresh_full='NOT_RUN: owner requests reuse of accepted receipts',final_counts={k:v for k,v in debug.items() if k in ['raw_managed','logical_shared_source','node','python']},retained_raw_executions=sum((before&after).values()),removed_raw_executions=[dict(identity=list(k),multiplicity=v) for k,v in removed.items()],added_raw_executions=[dict(identity=list(k),multiplicity=v) for k,v in added.items()],python_added=sorted(set(python_ids)-set(old_python)),python_removed=sorted(set(old_python)-set(python_ids)),node_identity_membership='Original Node sources and runner unchanged; full original selection executes once',logical_accounting='Accepted typed-null cohort/source-sharing algorithm retained; canonical source identity is distinct from raw runtime multiplicity.'))
    release_raw=Counter(row['outcome'] for suite in release for row in suite['records'])
    release_bad=[dict(suite=s['suite'],**row) for s in release for row in s['records'] if row['outcome']!='Passed']
    put('release-build-and-targeted-tests.json',dict(configuration='Release',build=build_release,application_builds=load(RUN/(label+'-Release-application-builds.json')),selected=dict(total=sum(release_raw.values()),outcomes=dict(release_raw)),selections=release,positive_discovery=all(s['records'] for s in release),rationale='All new compile-sensitive cases in both Common and Windows; existing Core ResultTable/p-value; Common styles/layout/targets/SafeWrite; full existing Windows shared/composite tests; all Office managed conditional paths and execution tests. Assemblies copied from isolated Release build only.'))
    put('release-full-not-run.json',dict(status='NOT_RUN',reason='OWNER_APPROVED_DEBUG_FIRST_04A28',node_python='Source-only independent checks executed once under Debug; not counted twice',release_selected_total=sum(release_raw.values()),unexecuted_managed_raw=sum(raw.values())-sum(release_raw.values()),gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET')))
    put('nonpasses.json',dict(debug_managed=nonpasses,new_managed=new_nonpasses,debug_python=python_nonpass,inherited_python=inherited_py,new_python=new_py,release_selected=release_bad,strict_profile='Actual original commands; nonzero remains UNMET',no_nonpass_suppression=True))
    commands=[dict(receipt=p.relative_to(ROOT).as_posix(),**load(p)) for p in sorted((RUN/'commands').glob('*/command.json'))]
    put('command-outcomes.json',dict(commands=commands,raw_private_logs_retained=True,official_verifier='NOT_RUN: original fixed TestResults/Codex receipts would be overwritten; equivalent full isolated selections executed without changing script'))
    strict={name:load(RUN/'commands'/(label+'-'+name)/'command.json') for name in ['strict','profile','docs']}
    put('strict-profile-results.json',strict)
    put('four-worker-timing.json',dict(matrix_seconds=matrix['seconds'],lanes=[dict(lane=l['lane'],seconds=l['seconds']) for l in matrix['lanes']],critical_path_seconds=max(l['seconds'] for l in matrix['lanes']),workers=4,inner_MaxCpuCount=1,build_phase_separate=True,cpu_peak_memory='NOT_MEASURED',matched_workload=load(RUN/(label+'-matched-timing.json')),qualification='No native speedup claim; historical four-lane times are context only. Fresh unique output roots; no shared writers.'))
    put('config-dependent-source-audit.json',dict(core_common_TFM='netstandard2.0 unchanged',windows_TFM='net48 unchanged',tests='net10.0 and net48 retained',debug_flags='Windows DefineDebug=true',release_flags='Windows DefineDebug=false; Optimize=true',office_conditional_sources=[dict(path=p,sha256=c.sha(ROOT/p)) for p in ['BESHStat.OfficeAddin/Services/WorkbookDiagnostics.cs','BESHStat.OfficeAddin/tests/CorrectiveWorkflowTests.cs','BESHStat.OfficeAddin/tests/WorkbookAdapterTests.cs']],new_28_conditional_branches=0,compiler_package_targets_changed=False))
    # Native rendering property statements/helpers are protected by executable source contracts.
    escalation=bool(release_bad or new_nonpasses or removed or new_py)
    put('release-escalation-decision.json',dict(escalate=escalation,reason='Investigate exact newly observed failures before qualification' if escalation else 'No configuration-dependent difference, Release-only failure, compiler/package change, numerical or native formatting behavior change observed. New/modified branches have positive focused coverage; full Release remains deliberately NOT_RUN.',native_property_statement_guard='test_native_formatting_operation_order_preserved',limitation='Recording/source parity does not prove native appearance, rollback or scientific truth.'))
    ready=not escalation and not python_events.get('skipped',0) and all(row['outcome']=='Passed' for row in new_managed) and not node['fail'] and all(not any(b['exit_codes']) and b['source_unchanged'] for b in [build_debug,build_release]) and matrix['source_unchanged'] and strict['docs']['exit_code']==0
    state=dict(ready=ready,label=label,debug={k:debug[k] for k in ['raw_managed','logical_shared_source','node','python','aggregate_exit']},release=dict(total=sum(release_raw.values()),outcomes=dict(release_raw)),new_raw=len(new_managed),new_python=len(inventory['python_methods']),nonpasses=nonpasses,new_py=new_py,strict={k:v['exit_code'] for k,v in strict.items()},timing=matrix['seconds'])
    (RUN/'review-summary.json').write_text(json.dumps(state),encoding='utf-8')
    print(json.dumps({k:v for k,v in state.items() if k not in ['nonpasses']},indent=2))

def architecture():
    proof='NOT_GOLDEN / SAME_CORE_OUTPUT_PRESENTATION_COMPATIBILITY_ONLY'
    put('actual-host-consumption-map-v1.json',dict(proof=proof,routes=[dict(route='Scalar/rank1/rank2 or ResultTable',path='ResultTableWriterBase.write -> BuildOutputBlock -> ExcelDnaResultWriter.WriteOutputBlockCore -> ResultTablePresentationIntent.Capture -> WriteNativeOutputBlock -> format/ApplyPValueFormatting',transferred='Shape, roles and lazy index offsets',retained='Core normalization and pointer increment; Windows Range values/property writes'),dict(route='Single shared or legacy multi-table',path='WriteSharedRequest / ProcessListofResultTables.writeToSheet -> real writer -> Common presentation',transferred='Per-table immutable axes',retained='Original list insertion/separators/below-beside measurements; raw scalar/empty semantics'),dict(route='UibyID shared tables + supplement + chart',path='ExcelDnaSharedOutputRun -> OutputDocument/OutputLayoutPlanner -> ExcelDnaSharedOutputPlanAdapter -> ResultTableSequencePolicy/command.Presentation -> WriteSharedCommand -> real writer',transferred='Envelope, ordered table/extra/chart regions, immutable presentation',retained='Native acquire/identity/inspection/consent/chart action/partial failure boundaries')],untransferred=['Excel-native formatting constants/property writes, COM observation, workbook/sheet creation and chart placement remain Windows-owned','Legacy below/beside measurement and list insertion remain Core-owned; no universal horizontal document layout introduced','Deferred reporting correction subpaths remain inactive'],office='Existing output schema/OfficeJS consumers unchanged; managed/Node contract regression only'))
    put('output-cell-and-pvalue-role-matrix-v1.json',dict(proof=proof,intent='ResultTablePresentationIntent: shape+metadata only; no dense value clone or second p-value mathematics',coordinates='One-based table/body/sheet axes retained separately',cases='81 title/top/left/footer combinations plus invalid legacy metadata, lazy overflow, scalar/nonzero-bound vectors/matrices, Boolean/text/nonfinite/null, exact alpha and sk-SK',legacy_difference='Windows Convert.ToDouble numeric-looking text remains eligible; typed OutputCell.TextCell is not a probability',settings='Core BuildExcelNumberFormat unchanged; shared requests snapshot settings at original factory point; legacy reads AppGlobals at original late formatting point',annotation_policy='Order, duplicates and out-of-range annotations retained; native ignored-index/error boundaries unchanged',native_quirk='Left-header bold loop remains headerLeftColumns-footerRows',immutable='ReadOnly metadata lists; source ordinal/artifact/start/shape captured; no cell values retained in presentation intent'))
    put('result-table-pointer-and-layout-parity-v1.json',dict(proof=proof,core='ResultTable/ResultTableWriterBase/ProcessListofResultTables byte unchanged',sequence='Exact former Windows envelope/region logic delegated to Common ResultTableSequencePolicy',ordering='Existing OutputDocument and layout placements, then supplementary-0..N, then native-chart-0..N',pointers='Legacy write advances model row count once; shared commands set endRow+1 once; supplement blank row unchanged',bounds='Canonical last valid grid cell allowed; Windows shared preflight still requires spare next-cursor row; no new GUI blocking for raw metadata',tests=['LegacyMultiTableInsertionKeepsTitlesFootersAndNullSeparators','LastValidCellAndOverflowRemainDifferentSharedAndLegacyContracts','ShippingSharedRunConsumesCommonCommandsSupplementAndChart']))
    put('three-destination-preflight-and-overwrite-v1.json',dict(destinations={'CurrentWorksheet':0,'NewWorksheet':1,'NewWorkbook':2},default='NewWorksheet; BESHStat_Result canonical default unchanged',facts='33 pure destination/fact cases plus 36 consent/stale/partial cases; all preexisting Windows shared/composite native-fake cases retained',native='ExcelDnaOutputTargetAdapter and SheetManagement unchanged; names never physical identity',no_write='Denial, changed observations, protected/merged/read-only/deleted facts preserve no-write',partial='MutationMayHaveOccurred/confirmed written regions truthful; RollbackAttempted=false and RollbackSucceeded=false',native_acceptance='NOT_RUN'))
    put('deferred-output-issues-disposition-v1.json',dict(issues=[dict(id='04A05-LIDDELL-LABEL',status='CANDIDATE_ONLY_UNAPPLIED',source='ContingencyReportPolicy.TwoByTwo / Ui82x2',location='Liddell table index 1 after analyzed-count table; top header R1C1; body estimate label R3C1 (body2); p R2C2; CI R4C2',observed='Risk ratio label for existing Liddell estimate; no changed CI/value/marker',candidate='Odds ratio label, subject to exact owner/native timing authority'),dict(id='04A06-EMPTY-HOMOGENEITY',status='REVIEWED_UNCHANGED',source='UibyID.RunHomogeneityVar / WindowsAssumptionReportAdapter.Prepare',observed='No selected method produces zero tables; TotRows=0; UI area measurement TotRows+Count-1=-1; no detached writer call/pointer movement',candidate='Explicit empty-report GUI disposition requires owner/native decision; no invented statistic'),dict(id='C10-COV-NAMES-01 / C11-NATIVE-CORRECTIONS-01',status='UNAPPLIED',source='UiGLM / WindowsRegressionRequestAdapter.DeferredCovarianceDataHeaders',observed='Cloned first Data heading becomes Intercept only for covariance+intercept GLM/NB2; duplicate/missing names retained; Core names not mutated',candidate='Remove quarantined writer substitutions only under original exact authority'),dict(id='C12-GEE-PREDICTION-AXIS-01',status='DESCRIPTOR_ONLY',source='Ui13GEE / GeeReportPolicy.Predictions / GeePredictionOutputAxis',observed='Core cluster-concatenated -> existing input-axis Predictions permutation; actual sorted native import may be identity, interleaved fixtures are not',candidate='Descriptor records prediction position/input position/retained Core row ID/cluster; no physical worksheet row invented; no native reorder activated')],other_candidates_unchanged=['C19-CLUSTER-ROWID-DISPLAY-01','C20-DA-ROWID-DISPLAY-01','C20-FA-ROWID-AMBIGUOUS-01','PSM Love plot candidates','C13-LOGRANK-R-01','04A21 stacked-count issue','earlier C09/C11/C12 native/scientific obligations']))
    put('same-core-output-presentation-comparisons-v1.json',dict(proof=proof,independent_scientific_authority=False,numerical_tolerance='Exact typed cell/type/coordinate and format equality for projection; existing statistical result values reused unchanged',culture=['Invariant','sk-SK'],fixtures=[dict(path=p,sha256=c.sha(ROOT/p),bounded=True) for p in NEWFILES],classification='Passing output compatibility; not independent numerical validation; expected old errors and deferred labels remain explicit',payloads='Synthetic deterministic bounded fixtures in test sources; no historical matrix/corpus copied'))
    cross=load(ROOT/'evidence/04A25/original-control-crosswalk.json')
    put('original-control-crosswalk.json',dict(authorities=cross['authorities'],inherited_counts=cross['counts'],population_rule='Overlapping original populations, never add as disjoint. Exact lineage remains in unchanged original crosswalks.',relevant_owners=['Result writer/SheetManagement native controls','Ui82x2 Liddell','UibyID empty homogeneity/shared output','UiGLM covariance','Ui13GEE prediction/classification','04A27 importer transport'],releases=0,discharges=0,gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET')))
    pages=['global-settings.md','methods/homogeneity-of-variance.md','methods/generalized-estimating-equations-gee.md','_meta/methods.yml']
    pages+=['methods/'+p.name for p in (ROOT/'BESHStatNG_Help_MkDocs/docs/methods').glob('*conting*')]
    put('documentation-audit.json',dict(changed=['BESHStatNG_Help_MkDocs/docs/global-settings.md'],purpose='Describe existing marked numeric-looking text distinction without changing calculated values',inspected=[dict(path='BESHStatNG_Help_MkDocs/docs/'+p,sha256=c.sha(ROOT/'BESHStatNG_Help_MkDocs/docs'/p)) for p in pages],dormant_corrections_documented_as_shipping=False,office_mac_live_availability_changed=False,strict='See strict-profile-results.json'))

def final(label):
    state=load(RUN/'review-summary.json')
    initial=load(RUN/'source-pins.json');frozen=load(RUN/'frozen-pins.json')
    source_bad=[p for p,h in initial.items() if c.sha(ROOT/p)!=h]
    allowed=['BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb','BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj']
    unexpected=[p for p in source_bad if p not in allowed]
    frozen_bad=[p for p,v in frozen.items() if c.sha(ROOT/p)!=v['sha256']]
    old=load(RUN/'historical-result-metadata.json');now=c.result_membership()
    oldmap={v[0]:v[1:] for v in old};newmap={v[0]:v[1:] for v in now}
    historic_bad=[p for p,v in oldmap.items() if newmap.get(p)!=v]
    diff=c.git('diff','--check');index=c.git('diff','--cached','--name-only')
    status=c.git('status','--short');changed=c.git('diff','--name-only').splitlines()
    protected_ok=not unexpected and not frozen_bad and not historic_bad and not index and not diff
    put('retention.json',dict(historical_evidence_files=len(frozen),hash_mismatches=frozen_bad,historical_TestResults_files=len(old),stat_or_membership_mismatches=historic_bad,new_TestResults_files=sorted(set(newmap)-set(oldmap)),limitation='Original TestResults stat/identity membership checked; not all original result bytes rehashed. All earlier authority/evidence files byte-hashed. No cleanup/archival/deletion executed.',original_100_capture_registry='evidence/04A14/baseline-registry.json',protected_source_mismatches=unexpected))
    pins=r.source_pins();build=load(RUN/'builds'/label/'Debug/build.json')
    post_changes={p:dict(before=build['source_pins'].get(p),after=h) for p,h in pins.items() if build['source_pins'].get(p)!=h}
    source_exact=not post_changes
    qualified=set(post_changes)<= {'scripts/summarize-04a28.py'}
    put('source-final-qualification.json',dict(base=c.BASE,head=c.git('rev-parse','HEAD'),main=c.git('rev-parse','main'),branch=c.git('branch','--show-current'),current_source_pins=pins,source_same_as_final_debug_build=source_exact,executed_production_test_runner_inputs_unchanged=qualified,post_matrix_report_only_changes=post_changes,report_only_reason='Fix missing receipt path adapter to accepted logical accounting; compact public metadata. No production/compiled test/helper/selection/runner change.',changed_preexisting_sources={p:dict(before=initial[p],after=c.sha(ROOT/p)) for p in source_bad},protected_ok=protected_ok,index_empty=not index,diff_check=diff,status=status,git_modified_paths=changed,no_staging_commit_merge_push=True,ignored_run_root=RUN.name))
    state['ready']=state['ready'] and protected_ok and qualified
    decision='READY_FOR_OWNER_04A28_ACCEPTANCE_REVIEW' if state['ready'] else 'NOT_READY'
    dossier=ROOT/'docs/architecture/batch-04A-28-owner-review.md'
    nonpass_table='\n'.join('| '+row['suite']+' | `'+row['class_name']+'.'+row['test_name']+'` | '+row['outcome']+' |' for row in state['nonpasses'])
    py=load(OUT/'nonpasses.json')['debug_python']
    py_table='\n'.join('| `'+key+'` | '+value+' |' for key,value in sorted(py.items()))
    command_table='\n'.join('| `'+Path(p).parent.name+'` | '+str(load(p)['exit_code'])+' | '+format(load(p)['seconds'],'.2f')+' |' for p in sorted((RUN/'commands').glob('*/command.json')))
    d=state['debug'];release=state['release'];timing=load(OUT/'four-worker-timing.json');matched=timing['matched_workload']['results']
    dossier.write_text(f'''# 04A.28 owner review

**{decision}** — software extraction only. Full Debug aggregate exit is **{d['aggregate_exit']}**; inherited nonpasses remain failures. Full Release is **NOT_RUN — OWNER_APPROVED_DEBUG_FIRST_04A28**. No native/scientific acceptance, release or obligation discharge is requested.

## Entry and authority

Branch `codex/04A-28`; actual main/base/HEAD `{c.BASE}`. Owner explicitly confirmed exclusive repository/shared-output writers for this new batch and accepted merges of 04A.26, out-of-sequence 04A.27 and test rationalization. All three branch tips are verified ancestors; see entry qualification JSON. Clean entry and index, no excluded owner edits. Plan revision 2.10's stale 04A.26 introductory status is superseded by explicit owner acceptance; plan bytes remain unchanged. AGENTS, required plan rows, accepted dossiers, live projects/runsettings/verifier inspected. No Git publication.

## Shipping implementation and boundary

Core `ResultTableWriterBase.write/BuildOutputBlock` → Windows `WriteOutputBlockCore` → Common `ResultTablePresentationIntent` → Windows `WriteNativeOutputBlock/format/ApplyPValueFormatting`. Shared commands carry captured immutable intent through the original virtual entry point. `WriteSharedRequest` and legacy `ProcessListofResultTables` consume the same metadata. The UibyID shared table/supplement/chart route also delegates bounds and ordered regions to Common `ResultTableSequencePolicy`, through existing `OutputDocument/OutputLayoutPlanner/WriteIntent/SafeWrite`. Actual integration is exercised by a native-recording seam below the shipping Core method; old recorder override identities remain intact.

Common owns detached shape, roles, table/body/sheet offsets, source ordinals, annotation ordering and pure envelope/region decisions. Windows retains Range/COM values, fonts/colors/borders/number formats, AutoFit, diagnostics, workbook creation, identity/occupancy/shape observation, confirmation and partial-failure boundaries. No new output schema, p-value mathematics, dense copied cell matrix or GUI rejection was introduced. Core conversion, list separators and below/beside measurements remain unchanged; a general horizontal document planner was not added. Chart geometry remains renderer-owned.

One-based coordinates, duplicate/invalid p-value annotations and body offsets are retained. P-value number format uses unchanged Core formula; numeric values remain numeric. Windows numeric-looking text coercion differs intentionally from typed probability cells, including `sk-SK`. Shared settings snapshot at the original request factory; legacy settings remain read at original formatting point. The old left-header/footer bold-loop coupling remains unchanged. Native property statement order and conversion/fallback helper bodies have executable source guards. Pure parity does not prove byte-identical Excel appearance. Cancellation/denial/stale observations produce no writes; partial failures retain confirmed regions and uncertainty. RollbackAttempted/Succeeded remain false.

## Deferred issue review

Liddell retains **Risk ratio** at Liddell table body row2/column1 (assembled R3C1), p-value R2C2, CI R4C2; candidate Odds ratio label remains unapplied. The analyzed count table precedes this table. No-selected homogeneity returns zero tables; existing UI area measurement remains -1 and detached writer pointer unchanged. Covariance Data heading quarantine remains under `C11-NATIVE-CORRECTIONS-01`, including duplicate/missing names and unmutated Core names. New GEE review descriptor distinguishes Core prediction ordinal, input ordinal, retained row ID and cluster; existing Predictions permutation is unchanged and no physical worksheet coordinates invented. No native reorder or correction is activated. Other C09/C11/C12, C13 log-rank, C19/C20 row displays, stacked counts and PSM Love plot candidates remain open; see exact disposition metadata and original dossiers.

## Validation and configuration evidence

| Qualification | Actual result |
|---|---|
| Debug managed canonical logical | {d['logical_shared_source']['passed']}/{d['logical_shared_source']['total']} |
| Debug managed raw runtime executions | {d['raw_managed']['outcomes'].get('Passed',0)}/{d['raw_managed']['total']} |
| Node once | {d['node']['pass']}/{d['node']['tests']} |
| Python once | {d['python']['passed']}/{d['python']['total']}; {d['python']['method_nonpasses']} |
| New managed raw Debug cases | {state['new_raw']}; exact positive identities/outcomes in new-test-inventory.json |
| New Python methods | {state['new_python']} |
| Release selected managed | {release['outcomes'].get('Passed',0)}/{release['total']} |
| Full Release managed | NOT_RUN; do not infer PASS for remaining cases |
| Original strict/profile/strict MkDocs exits | {state['strict']['strict']}/{state['strict']['profile']}/{state['strict']['docs']} |

Both configurations compiled Core/Common netstandard2.0, Windows net48 host/tests/control mitigation and Office managed projects into isolated paths; application build receipts separately cover OfficeAddin/WASM. Windows Debug DefineDebug=true, Release DefineDebug=false/Optimize=true retained; no TFM/package/conditional options changed. Release selections cover all new adapter/presentation branches, Core ResultTable/p-values, Common style/layout/target/SafeWrite, all Windows shared/composite contracts and all Office compiled conditional paths. Exact Release test IDs, actual Release assembly paths and per-command filter/hash/count are retained. Positive discovery required. Escalation decision is in release-escalation-decision.json; no inferred full Release equivalence.

Entry full rerun was intentionally NOT_RUN; accepted main-qualified maintenance receipts supply historical context, not a fabricated fresh baseline. Exact original raw membership retained/added/removed is reconciled against the compact accepted inventory. The 47 linked SPC cases still execute in each host. No existing test, DataRow, assertion, source pin or historical authority was edited/deleted/skipped. Accepted logical typed-null/source-sharing accounting retained. Same-Core output tests are **NOT_GOLDEN / SAME_CORE_OUTPUT_PRESENTATION_COMPATIBILITY_ONLY**, not independent scientific proof. Synthetic fixtures use exact type/cell/format/coordinate equality; existing statistical values are reused, not recalculated by a second engine.

Final Debug uses four serial-inner isolated lanes, separate from builds: overall {timing['matrix_seconds']:.2f}s; lane durations {[(v['lane'],round(v['seconds'],2)) for v in timing['lanes']]}; critical path {timing['critical_path_seconds']:.2f}s. CPU/peak memory NOT_MEASURED. Matched existing Windows orchestration workload: original {matched[0]['command']['seconds']:.2f}s versus current {matched[1]['command']['seconds']:.2f}s, {len(matched[0]['records'])} identical executions, exact original compiled-source qualification. One pair is not causal speedup evidence and excludes native Excel. No acceleration claim. All failed development attempts remain preserved; fixture/compiler issues were fixed before qualification, with no old-test weakening. The reporting assembler's missing-path adapter was corrected after the matrix; this report-only change does not alter compiled inputs, tests or selection.

Unmodified `verify-beshstatng.ps1`/`-Legacy` wrapper NOT_RUN because fixed historical TestResults/Codex receipts would be overwritten; equivalent original complete selections run through accepted opt-in infrastructure. Default/full maintenance runner unchanged. Exact complete build/current source-pin equality: {source_exact}; executed production/test/runner inputs unchanged: {qualified}. Only the new report assembler changed after the matrix; the final report assembly and focused Python contracts are reverified. No compiled/test selection changes after final matrix. Unique private receipts: `{RUN.name}`; no logs copied into older evidence. Exact commands, sanitized paths and real exits are in command-outcomes.json; source-only Python/Node/strict checks are counted once, not twice under Release.

## Exact managed nonpasses

| Suite | Identity | Outcome |
|---|---|---|
{nonpass_table}

## Exact Python nonpasses

| Identity | Outcome |
|---|---|
{py_table}

These real FAIL/ERROR outcomes remain visible; no denominator change hides them. Source/branch pins are historical authority failures, distinct from portable functional discrepancies. Exact traces are retained privately and sanitized terminal messages in machine-readable results. See nonpasses.json for inherited/new classifications.

## Protection, Help and owner decision

{len(frozen)} original evidence/architecture/planning files hash-checked; 100 original mixed-model captures preserved. {len(old)} historical TestResults stat/identity memberships unchanged: {not historic_bad}. Original TestResults bytes were not all rehashed; no such stronger claim is made. Core statistical sources, UDF exports, designers, chart renderers, 04A.27 importer and canonical SPC sources remain byte-identical. Original control populations overlap and are never summed; 2056 controls/1514 obligations and 44 importer obligations remain under their original authorities, zero releases/discharges. No cleanup, archive, relocation or historical transformation.

Help changed only global-settings.md to describe the existing locale-convertible text distinction; homogeneity/GEE/dormant corrections and Office/Mac availability remain unchanged. methods.yml unchanged; strict MkDocs runs on final Help bytes. Excel/manual smoke NOT_RUN (separate owner/native authorization): several tables plus chart in current/new sheet; verify significance and destination consent. Office/Mac/browser/R/COM/UIA/installation/signing NOT_RUN/unauthorized.

Final protected verification: {protected_ok}; `git diff --check` clean; empty index: {not index}. No staging/commit/merge/push. New public files are privacy-screened and trackable; manifest lists only this batch's candidates (self hash excluded explicitly). Changed/preexisting-source list and added test/helper/source hashes are in source-final-qualification.json and publication-manifest.json. Gates remain **P=NO; N1=NO GO; N2=NO; Gate C2=UNMET**.

Proposed owner decision: accept bounded 04A.28 software extraction and the explicit reduced Release evidence only if the above caveats are acceptable. All dormant scientific/native corrections and original obligations stay open; a future release/full matrix/native decision requires separate authority. No publication or next batch authorization is implied.

## Exact command outcomes

Full sanitized arguments/cwd/output hashes and test count receipts are in command-outcomes.json. Local command identifiers and wall seconds:

| Command | Exit | Seconds |
|---|---|---|
{command_table}
''',encoding='utf-8')
    candidates=sorted(set(c.git('diff','--name-only').splitlines()+c.git('ls-files','--others','--exclude-standard').splitlines()))
    entries=[];privacy=[]
    for p in candidates:
        if p=='evidence/04A28/publication-manifest.json':continue
        raw=(ROOT/p).read_bytes()
        # Source tools intentionally resolve local tools dynamically; public outputs contain no profile paths.
        if p.startswith(('evidence/04A28/','docs/architecture/batch-04A-28')):
            text=raw.decode('utf-8-sig')
            if re.search(r'(?i)[a-z]:[\\/]+Users[\\/]|\bS-1-\d+(?:-\d+)+\b',text):privacy.append(p)
        ignored=__import__('subprocess').run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','--quiet',p],cwd=ROOT).returncode==0
        entries.append(dict(path=p,sha256=c.sha(ROOT/p),bytes=len(raw),trackable=not ignored,privacy='PASS' if p not in privacy else 'FAIL'))
    put('privacy.json',dict(status='PASS' if not privacy else 'FAIL',violations=privacy,sanitizer='Stable <REPO_ROOT>, <PYTHON_EXE>, <MSBUILD_EXE>, <VSTEST_EXE>; no native pointer/workbook payload',public_evidence_only=True))
    # privacy file was generated after its first candidate scan: refresh its manifest row.
    privacy_path='evidence/04A28/privacy.json'
    entries=[v for v in entries if v['path']!=privacy_path]+[dict(path=privacy_path,sha256=c.sha(ROOT/privacy_path),bytes=(ROOT/privacy_path).stat().st_size,trackable=True,privacy='PASS')]
    put('publication-manifest.json',dict(schema_version=1,entries=sorted(entries,key=lambda v:v['path']),self_hash='Excluded to avoid recursive hash; file itself is trackable',native_releases=0,obligation_discharges=0,index_empty=not index))
    assert not privacy and all(v['trackable'] for v in entries)
    print(decision,dossier.relative_to(ROOT))

if __name__=='__main__':
    action=sys.argv[1];label=sys.argv[2] if len(sys.argv)>2 else 'final01'
    if action=='prepare':prepare()
    elif action=='summarize':summarize(label)
    elif action=='architecture':architecture()
    elif action=='final':final(label)
    else:raise ValueError(action)
