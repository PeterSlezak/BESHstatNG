from common40_v3 import *
lock()
udfs=sorted((ROOT/'BESHstatNG/src/ExcelUDFs').rglob('*.vb'))
extra=[ROOT/'BESHstatNG/src/UI'/n for n in ['UiGLM.vb','Ui13GEE.vb','Ui4Cox.vb','Ui18MMRM.vb','Ui19LMM.vb']]
extra += list((ROOT/'BESHstatNG/src/DM').glob('Windows*Regression*.vb'))+list((ROOT/'BESHstatNG/src/DM').glob('Windows*WorkflowAdapter.vb'))
extra += list((ROOT/'BESHStat.Excel.Common/src').rglob('*Regression*.vb'))+list((ROOT/'BESHStat.Excel.Common/src').rglob('*Gee*.vb'))+list((ROOT/'BESHStat.Excel.Common/src').rglob('*Cox*.vb'))+list((ROOT/'BESHStat.Excel.Common/src').rglob('*MixedModel*.vb'))
paths=sorted({p.relative_to(ROOT).as_posix() for p in udfs+extra if p.is_file()})
listfile=PRIVATE/'source-list-v1.txt'
with listfile.open('x',encoding='utf-8',newline='\n') as f:f.write('\n'.join(paths)+'\n')
tool=ROOT/'.test-rationalization-04A39.tmp/map-tool/Inventory39.dll';assert tool.is_file()
record=command('source-map-before-v1',['dotnet',str(tool),str(PRIVATE/'source-map-before-v1.json'),str(listfile)])
save(PRIVATE/'source-map-before-command-v1.json',record);assert record['exit_code']==0
save('scope/source-freeze-v1.json',dict(paths=[pin(p) for p in paths],syntax_map=pin(PRIVATE/'source-map-before-v1.json'),reader=pin(tool),kind='04A40_CURRENT_PRE_CHANGE',
    expected_exports=268,plan_families=['LM','GLM','NB2','ZIP','GEE','Cox/survival','LMM','MMRM','ordinal','multinomial','formula/effects'],
    planned_production_edits=['RegressionRequestPolicy','LinearBinomialRegressionPolicy','ExtendedRegressionFamilyPolicy','GeeWorkflowPolicy','CoxWorkflowPolicy','UdfLmmInputPreparation38',
        'UDF LM/GLM/NB2/ZIP/ordinal/multinomial/GEE/Cox/LMM existing consumers'],
    policy='Existing typed options/policies, no new parser, validation, estimator, cache owner or GUI patch activation. Preserve numeric values, aliases, callback/failure order. Formula, MMRM and result helpers already shared unless concrete gap proved.'))
print('Bounded syntax/source freeze completed',len(paths),'paths;268 exports',flush=True)
