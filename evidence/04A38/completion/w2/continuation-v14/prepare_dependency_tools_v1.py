from common import *
kernel='''"""C13 exact binder plus source-authenticated existing PSM private handle."""
from proof_kernel_v6 import *
NESTED_CACHE_DECLARATIONS['BESHStatNG.WorksheetFunctions.PropensityScoreUDFs']={'PsmUdfHandle'}
'''
with (HERE/'proof_kernel_v7.py').open('x',encoding='utf-8',newline='\n') as f:f.write(kernel)
s=PREVIOUS/'dependency_syntax_v1.py';v=s.read_text().replace('from proof_kernel_v4 import','from proof_kernel_v7 import')
a=v.index('portable=[');b=v.index('\nresult=',a)
paths=['BESHStat.Excel.Common/src/Analysis/UdfClassLabelPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfPsmDataPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfPsmPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfPlotDataPreparation38.vb','BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb']
for p in (ROOT/'BESHStat.Excel.Common/src').rglob('*.vb'):
 if 'Class PropensityScoreUDFsOptions38' in p.read_text(encoding='utf-8-sig'):paths.append(p.relative_to(ROOT).as_posix())
assert len(paths)==6
v=v[:a]+'portable='+repr(paths)+v[b:]
with (HERE/'dependency_syntax_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
for name in ['run_auxiliary_v1.py','report_managed_v1.py']:
 text=(PREVIOUS/name).read_text().replace('w2c13','w2c14')
 if name=='report_managed_v1.py':
  text=text.replace('continuation-v12','continuation-v13').replace('len(added)==40','len(added)==52').replace('Passed=20874','Passed=20926').replace('Passed=20834','Passed=20874').replace('new_passes=40','new_passes=52').replace('new_PASS=40','new_PASS=52').replace('40 genuinely','52 genuinely').replace('C12','C13').replace('NEW_C13','NEW_C14')
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
save('dependency-tools-source-crosswalk-v1.json',dict(original=pin(s),current=pin(HERE/'dependency_syntax_v1.py'),binder=pin(HERE/'proof_kernel_v7.py'),PSM_handle_source=json.loads((HERE/'entry-queue-receipt-v1.json').read_bytes())['property_context'],portable_sources=paths,original_queries=0))
save('managed-precondition-failure-v1.json',dict(failed_driver=pin(HERE/'full-managed-Debug-driver-v1-execution.json'),reason='Missing fresh compatibility receipt was rejected before creating source-freeze-managed receipt or starting any build/test suite. Wait for current-only comparisons, then invoke unchanged driver under a new outer command label.',full_campaigns_started=0,managed_tests_executed=0,original_queries=0))
