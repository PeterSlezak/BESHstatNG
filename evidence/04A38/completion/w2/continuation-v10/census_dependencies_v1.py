"""Source declaration/fragment census for the actual C10 portable/dependency frontier."""
from common import *
tool=RUNROOT/'completion-w2-cont8/portable-census-v1/bin/Census.dll';assert tool.is_file()
portable=['BESHStat.Excel.Common/src/Analysis/UdfContingencyPreparation38.vb','BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb']
dependencies=['BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb','BESHStat.Core/src/StatTests/StatisticalResultContracts.vb','BESHStat.Core/src/StatTests/ContingencyTable.vb','BESHStat.Core/src/RegModels/Multivariate/PCA.vb','BESHStat.Core/src/RegModels/Multivariate/FactorAnalysis.vb','BESHStat.Core/src/LinearAlgebra/MatrixArithmeticCore.vb']
for p in dependencies:assert (ROOT/p).is_file(),p
for name,files in [('portable-universe-v1.json',portable),('dependency-source-universe-v1.json',dependencies)]:
    r=command(name[:-5]+'-syntax',['dotnet',str(tool),str(ROOT),str(HERE/name),*files]);assert r['exit_code']==0
save('dependency-census-authority-v1.json',dict(tool=pin(tool),source=pin(W2/'continuation-v8/PortableCensus-v1.cs'),syntax_only=True,portable=portable,dependencies=dependencies,original_source_compiler_runs=0))
print('C10 full portable/dependency source/fragment declarations parsed')
