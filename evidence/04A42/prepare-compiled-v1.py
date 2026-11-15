from common42 import *
s=(ROOT/'evidence/04A41/compiled-and-fixtures-v1.py').read_text(encoding='utf-8-sig').replace('from common41','from common42').replace("'a41-wave2-v1'","'a42-wave-b2-v1'").replace("'a41-final-v1'","'a42-final-v1'")
s=s.replace('evidence/04A40/abi-cil/','evidence/04A41/abi-cil/').replace('-proof-v2.json','-proof-v1.json').replace('baseline40','baseline41')
a=s.index('owners=[');b=s.index('\nunexpected=',a)
s=s[:a]+"owners=['WorksheetFunctions.MultivariateAnalysisUDFs','WorksheetFunctions.ClassificationReportUDFs','WorksheetFunctions.PropensityScoreUDFs','WorksheetFunctions.PlotDataUDFs','ExcelCommon.UdfMultivariateReportPreparation38','ExcelCommon.UdfClassificationReportPreparation38','ExcelCommon.UdfPsmDataPreparation38','ExcelCommon.UdfPlotDataPreparation38']"+s[b:]
s=s.replace("PRIVATE/'fixtures-before-v1/Observe41.exe'","ROOT/'.test-rationalization-04A41.tmp/fixtures-before-v1/Observe41.exe'").replace("PRIVATE/'fixtures-before-v1/observations.json'","ROOT/'.test-rationalization-04A41.tmp/fixtures-before-v1/observations.json'")
s+='''
compare42=module('compare42',HERE/'verify-wave-v1.py')
a,b,current=execute_fixture('42',PRIVATE/'fixtures-before-v1/Observe42.exe',[],PRIVATE/'fixtures-before-v1/observations.json')
assert compare42.canonical(a)==compare42.canonical(b) and len(a['rows'])==1802
save('fixtures/'+config+'-42-v1.json',dict(cases=1802,typed_statistical_shape_error_order_exact=True,baseline=pin(PRIVATE/'fixtures-before-v1/observations.json'),current=current,only_excluded_runtime_fields=['Generated session GUID strings','Created UTC timestamp strings'],scientific='NOT_GOLDEN'))
'''
with (HERE/'compiled-and-fixtures-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Compiled/fixture final recipe prepared',flush=True)
