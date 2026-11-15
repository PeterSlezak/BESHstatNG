from common42 import *
def write(p,s):(ROOT/p).write_bytes(s.replace('\r\n','\n').replace('\n','\r\n').encode())
p='BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb';s=(ROOT/p).read_text(encoding='utf-8-sig');new='''                Dim tables As List(Of ResultTable) = Global.BESHStatNG.ExcelCommon.UdfPlotDataPreparation38.ComputeRocTables(rocInput, markerName, alphaValue)'''
old='''                Dim mdl As New graphics.ROC(rocInput, New String() {If(String.IsNullOrWhiteSpace(markerName), "Marker", markerName)})
                mdl.compute(alphaValue)
                Dim tables As List(Of ResultTable) = mdl.wrapResults()'''
assert s.count(new)==2;s=s.replace(new,old);write(p,s)
p='BESHStat.Excel.Common/src/Analysis/UdfPlotDataPreparation38.vb';s=(ROOT/p).read_text(encoding='utf-8-sig');a=s.index('        Public Shared Function ComputeRocTables(');b=s.index('        Public Shared Function ComputeKaplanMeierCurves(',a);s=s[:a]+s[b:];s=s.replace('Of regression.CalibrationBinSummary','Of Global.BESHStatNG.regression.CalibrationBinSummary').replace('            regression.BinaryClassificationCore.','            Global.BESHStatNG.regression.BinaryClassificationCore.').replace('Return regression.BinaryClassificationCore.','Return Global.BESHStatNG.regression.BinaryClassificationCore.');write(p,s)
print('Retained already-correct ROC Core/Windows orchestration;no duplicate ROC builder or native adapter dependency introduced',flush=True)
