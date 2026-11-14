"""Freeze production/tests; syntax-only dependency census, with existing offline recorder."""
from common import *
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes())
save('production-test-freeze-v1.json',dict(status='C11_FINAL_PRODUCTION_TEST_INPUTS_FROZEN',
    sources=[pin(ROOT/p) for p in plan['source_paths']],
    focused=pin(HERE/'focused-bc-v1-packet-v2-Debug.json'),
    Debug_production=pin(HERE/'build-packet-v2-Debug.json'),
    Release_production=pin(HERE/'build-packet-v2-Release.json'),
    new_managed_tests=dict(Common=22,Windows=7,total=29),
    Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',original_queries=0))
tool=RUNROOT/'completion-w2-cont8/portable-census-v1/bin/Census.dll'
portable=['BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb',
          'BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb']
dependencies=['BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb',
 'BESHStat.Core/src/RegModels/Multivariate/FactorAnalysis.vb',
 'BESHStat.Core/src/RegModels/Multivariate/ClusterAnalysis.vb',
 'BESHStat.Core/src/RegModels/Multivariate/CA.vb',
 'BESHStat.Core/src/RegModels/Multivariate/MultivariateShared.vb',
 'BESHStat.Core/src/RegModels/Multivariate/MultivariateInputHelpers.vb',
 'BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb',
 'BESHStat.Core/src/LinearAlgebra/MatrixArithmeticCore.vb',
 'BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb',
 'BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb',
 'BESHStat.Excel.Common/src/Analysis/UdfRangePreparation38.vb',
 'BESHStat.Excel.Common/src/Results/UdfReportProjection38.vb']
for name,paths in [('portable-universe-v1.json',portable),('dependency-source-universe-v1.json',dependencies)]:
    assert all((ROOT/p).is_file() for p in paths)
    result=command(name[:-5]+'-syntax',['dotnet',str(tool),str(ROOT),str(HERE/name),*paths])
    assert result['exit_code']==0
save('dependency-census-authority-v1.json',dict(tool=pin(tool),
    source=pin(W2/'continuation-v8/PortableCensus-v1.cs'),portable=portable,dependencies=dependencies,
    syntax_only=True,original_source_compiler_runs=0,original_queries=0))
print('C11 source/tests frozen; current-only dependency source census complete')
