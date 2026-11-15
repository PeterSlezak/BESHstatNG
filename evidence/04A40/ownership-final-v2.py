from common40_v3 import *
import collections,re
lock()
prior=load(HERE/'scope/source-freeze-v1.json')
paths={r['path'] for r in prior['paths']}
paths|={p.relative_to(ROOT).as_posix() for p in (ROOT/'BESHStat.Core/src/RegModels').rglob('*.vb')}
paths|={p.relative_to(ROOT).as_posix() for p in (ROOT/'BESHStat.Core/src/Survival').glob('*.vb')}
paths.add('BESHStat.Core/src/StatTests/Survival.vb')
lst=PRIVATE/'source-list-final-v2.txt'
with lst.open('x',encoding='utf-8',newline='\n') as f:f.write('\n'.join(sorted(paths))+'\n')
tool=ROOT/'.test-rationalization-04A39.tmp/map-tool/Inventory39.dll'
result=command('source-map-final-v2',['dotnet',str(tool),str(PRIVATE/'source-map-final-v2.json'),str(lst)])
save(PRIVATE/'source-map-final-command-v1.json',result);assert result['exit_code']==0
syntax=load(PRIVATE/'source-map-final-v2.json')
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
save('ownership/method-parity-v2.json',dict(methods=rows,method_count=len(rows),family_counts=dict(collections.Counter(r['family'] for r in rows)),sources=[pin(p) for p in sorted(paths)],
    syntax_reader=pin(tool),syntax=pin(PRIVATE/'source-map-final-v2.json'),exports=268,consumer_edges='Source-syntax edges plus final compiled call evidence; no claim that identifier spelling alone proves whole-body equivalence.',
    no_new_parser_or_estimator=True,Windows_only=['ExcelDnaUtil.IsInFunctionWizard','ExcelArgument/reference coercion','RefEdit/range reading','error/spill transport','session handle caches','WinForms/writers/chart COM'],gates=GATES))
print('Final source ownership map refreshed without register rewrite',flush=True)
