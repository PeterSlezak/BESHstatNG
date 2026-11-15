from common40_v3 import *
import collections,re
lock()
prior=load(HERE/'scope/source-freeze-v1.json')
paths={r['path'] for r in prior['paths']}
paths|={p.relative_to(ROOT).as_posix() for p in (ROOT/'BESHStat.Core/src/RegModels').rglob('*.vb')}
paths|={p.relative_to(ROOT).as_posix() for p in (ROOT/'BESHStat.Core/src/Survival').glob('*.vb')}
paths.add('BESHStat.Core/src/StatTests/Survival.vb')
lst=PRIVATE/'source-list-final-v1.txt'
with lst.open('x',encoding='utf-8',newline='\n') as f:f.write('\n'.join(sorted(paths))+'\n')
tool=ROOT/'.test-rationalization-04A39.tmp/map-tool/Inventory39.dll'
result=command('source-map-final-v1',['dotnet',str(tool),str(PRIVATE/'source-map-final-v1.json'),str(lst)])
save(PRIVATE/'source-map-final-command-v1.json',result);assert result['exit_code']==0
syntax=load(PRIVATE/'source-map-final-v1.json')
family_files=dict(LM='LinearModelUDFs.vb',GLM='GlmUDFs.vb',NB2='GlmNbUDFs.vb',ZIP='ZipUDFs.vb',GEE='GeeUDFs.vb',Cox='CoxUDFs.vb',LMM='LmmUDFs.vb',MMRM='MixedModelUDFs.vb',Ordinal='OrdinalLogitUDFs.vb',Multinomial='MultinomialLogitUDFs.vb',Formula='RegressionFormulaUDFs.vb')
gui=dict(LM='UiGLM.vb',GLM='UiGLM.vb',NB2='UiGLM.vb',ZIP='UiGLM.vb',Ordinal='UiGLM.vb',Multinomial='UiGLM.vb',GEE='Ui13GEE.vb',Cox='Ui4Cox.vb',LMM='Ui19LMM.vb',MMRM='Ui18MMRM.vb',Formula='UiGLM.vb')
policies=dict(LM=['RegressionRequestPolicy','LinearBinomialRegressionPolicy','UdfRegressionFamilyReportPreparation38'],GLM=['RegressionRequestPolicy','LinearBinomialRegressionPolicy','UdfRegressionFamilyReportPreparation38'],
 NB2=['RegressionRequestPolicy','ExtendedRegressionFamilyPolicy','UdfRegressionFamilyReportPreparation38'],ZIP=['ExtendedRegressionFamilyPolicy','UdfW2CompletionPreparation38'],
 GEE=['GeeWorkflowPolicy','GeeReportPolicy','GeePredictionOutputAxis','UdfRegressionFamilyReportPreparation38'],Cox=['CoxWorkflowPolicy','UdfCoxInputPreparation38','CoxResidualPlotAvailability36'],
 LMM=['MixedModelWorkflowPolicy','MixedModelReportPolicy','UdfLmmInputPreparation38','UdfLmmReportPreparation38'],MMRM=['MixedModelWorkflowPolicy','UdfW2CompletionPreparation38','UdfModelProfilePreparation38'],
 Ordinal=['RegressionRequestPolicy','ExtendedRegressionFamilyPolicy','UdfRegressionOutputReports38'],Multinomial=['RegressionRequestPolicy','ExtendedRegressionFamilyPolicy','UdfRegressionOutputReports38'],Formula=['RegressionEffectAuthoringPolicy','UdfW2CompletionPreparation38','RegressionFormulaDesignService'])
rows=[]
for family,file in family_files.items():
    members=[m for m in syntax['members'] if m['path'].endswith('/Regression/'+file)]
    for m in members:
        rows.append(dict(family=family,source=pin(m['path']),member=m['symbol_id'],signature=m['signature'],kind=m['kind'],start_line=m['start_line'],end_line=m['end_line'],
            body_sha256=m['body_sha256'],tokens_sha256=m['body_token_sha256'],calls=m['direct_calls'],
            branch_statement_count=len(m['statements']),existing_Common_owners=policies[family],GUI_consumer='BESHstatNG/src/UI/'+gui[family],
            boundary='Windows retains acquisition/sentinels/registration/handles; Common typed settings and report policies; Core owns model/design/inference.',
            row_order='Existing acquired RowIds, cluster/visit and result axes retained; no physical coordinates invented.',
            action='EXISTING_SHARED_PREPARATION_RETAINED' if family in ['Formula','MMRM'] else 'TYPED_FIT_OPTIONS_OR_REQUEST_PREPARATION_SHARED_WITHOUT_BEHAVIOR_CHANGE'))
survival=[m for m in syntax['members'] if '/ExcelUDFs/' in m['path'] and 'Survival' in m['path']]
for m in survival:rows.append(dict(family='Survival/KM/Logrank',source=pin(m['path']),member=m['symbol_id'],signature=m['signature'],kind=m['kind'],start_line=m['start_line'],end_line=m['end_line'],body_sha256=m['body_sha256'],calls=m['direct_calls'],action='EXISTING_UdfW2CompletionPreparation38_SHARED;SCIENTIFIC_FIX_DORMANT'))
save('ownership/method-parity-v1.json',dict(methods=rows,method_count=len(rows),family_counts=dict(collections.Counter(r['family'] for r in rows)),sources=[pin(p) for p in sorted(paths)],
    syntax_reader=pin(tool),syntax=pin(PRIVATE/'source-map-final-v1.json'),exports=268,consumer_edges='Source-syntax edges plus final compiled call evidence; no claim that identifier spelling alone proves whole-body equivalence.',
    no_new_parser_or_estimator=True,Windows_only=['ExcelDnaUtil.IsInFunctionWizard','ExcelArgument/reference coercion','RefEdit/range reading','error/spill transport','session handle caches','WinForms/writers/chart COM'],gates=GATES))
register=load(ROOT/'evidence/04A39/register/defects-v7.json')
wanted=['C08-CORE-LABEL-01','C08-FORMULA-REF-01','C10-LM-ALPHA-01','C10-COV-NAMES-01','C11-NATIVE-CORRECTIONS-01','C09-NATIVE-STRICT-01',
 'C12-GEE-START-QIC-01','C12-GEE-CLUSTER-TEXT-01','C12-GEE-PREDICTION-AXIS-01','C12-NATIVE-CLASSIFICATION-01','C13-LOGRANK-R-01','C14-INPUT-FINITE-01']
families={'C08':['Formula'],'C10':['LM','GLM'],'C11':['LM','GLM','NB2'],'C09':['LM','GLM','NB2','ZIP','Ordinal','Multinomial'],'C12':['GEE'],'C13':['Cox','Survival/KM/Logrank'],'C14':['LMM','MMRM']}
delta=[]
for issue in wanted:
    matches=[r for r in register['issues'] if issue in r['aliases']];assert len(matches)==1
    old=matches[0];fs=families[issue[:3]];selected=[r for r in rows if r['family'] in fs]
    source_links=[dict(path=p,sha256=pin(p)['sha256'],members=[dict(id=m['symbol_id'],name=m['name'],start_line=m['start_line'],end_line=m['end_line'],body_sha256=m['body_sha256']) for m in syntax['members'] if m['path']==p]) for p in sorted({r['source']['path'] for r in selected})]
    occurrence_pins=[]
    for hit in old['report_occurrences']:
        src=hit['source'];assert pin(src['path'])==src;occurrence_pins.append(hit)
    behavior='Current behavior retained; shared typed option mapping does not activate a correction.'
    if issue=='C12-GEE-START-QIC-01':behavior='Actual supplied-start Gaussian independence fits reproduce NullReferenceException in both cultures; automatic starts produce fitted results. QIC independence covariance initialization remains unmodified.'
    if issue=='C13-LOGRANK-R-01':behavior='Canonical shipping record-factory grouped/stratified earliest censor-event ties materialized before/after; existing risk-set implementation unchanged. Historical register location in ContingencyTable is unresolved/misattributed; current source must be linked to Survival.vb, not rewritten in39 register.'
    if issue=='C10-LM-ALPHA-01':behavior='UDF requested alpha0.10/0.05 preserved; GUI NativeLegacyDefault remains effective0.05. Point estimates unchanged. Windows activation remains unapplied.'
    delta.append(dict(canonical_id=old['canonical_id'],aliases=old['aliases'],pre_status=old['status'],post_status='INVESTIGATED_UNAPPLIED_SCIENTIFIC_NATIVE_RESTRICTED',
        original_record_sha256=sha(json.dumps(old,sort_keys=True,ensure_ascii=False).encode()),original_authorization=old['authorization'],current_authorization='BEHAVIOR_PRESERVING_CONFIGURATION_ONLY;NO_ISSUE_SPECIFIC_INFERENCE_OR_NATIVE_ACTIVATION_AUTHORITY',
        observed_behavior=behavior,source_links=source_links,original_report_occurrences=occurrence_pins,original_obligations=old['original_obligations'],original_controls=old['original_controls'],
        historical_source_locations_retained=old['source_locations'],reference_status='FRESH_INDEPENDENT_R_NOT_RUN;EXACT_CORRECTION_AUTHORIZATION_NOT_ESTABLISHED' if issue.startswith(('C12','C13')) else 'CURRENT_MAIN_FROZEN_COMPARISON;NOT_GOLDEN',
        implementation='FIT_SETTING_PLUMBING_SHARED_ONLY;CORRECTION_DORMANT',tests=['48 current-main before/after cases','Regression40FitPolicyTests','Regression40MixedSurvivalTests','existing family/reference tests in final managed census'],
        native_gate='04A.45–47 exact native attribution/approval required',scientific_gate='Issue-specific numerical/reference decision required',next_owner='Owner and independent scientific/native reviewer',
        unblocking_criterion='Exact issue-specific authorization and applicable independent numerical/native evidence; batch assignment alone is insufficient.',exit_disposition_04A48=None))
core_links=[]
for path in ['BESHStat.Core/src/RegModels/GEE.vb','BESHStat.Core/src/StatTests/Survival.vb','BESHStat.Core/src/RegModels/RegressionDesignCore.vb']:
    if (ROOT/path).exists():core_links.append(dict(source=pin(path),members=[dict(id=m['symbol_id'],name=m['name'],start_line=m['start_line'],end_line=m['end_line'],body_sha256=m['body_sha256']) for m in syntax['members'] if m['path']==path]))
save('register/issue-delta-v1.json',dict(schema='04A40_VERSIONED_SUCCESSOR_DELTA_NO_ORIGINAL_REWRITE',base=pin(ROOT/'evidence/04A39/register/defects-v7.json'),base_entries=1169,
    base_obligations=1514,base_restricted_controls=802,entries=delta,Core_source_reconciliation=core_links,
    all_other_canonical_entries='UNCHANGED;NO_DISCHARGE',reserved_exit_dispositions=['FIXED_AND_VERIFIED','RETAINED_RESTRICTED','DEFERRED_OWNER_APPROVED'],
    no_scientific_or_native_fix_activated=True,precise_pending_decisions=['C13 corrected tied risk-set inference plus independent R reference scope','C12 supplied-start/QIC numerical correction','C11 LM alpha/covariance native activation'],gates=GATES))
print('Measured parity map',len(rows),'members;',len(delta),'canonical issue decisions retained, no scientific/native activation',flush=True)
