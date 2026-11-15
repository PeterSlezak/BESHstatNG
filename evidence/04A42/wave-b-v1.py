from common42 import *
lock()
def read(p):return (ROOT/p).read_text(encoding='utf-8-sig')
def write(p,s):(ROOT/p).write_bytes(s.replace('\r\n','\n').replace('\n','\r\n').encode())
def extend(p,text):
    s=read(p);assert text.splitlines()[1].strip() not in s;s=s.replace('    End Class',text+'    End Class');write(p,s)
p='BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb';s=read(p)
a=s.index('            Dim name As ',s.index('Private Function ResolveNamedTable('));b=s.index('        End Function',a);body=s[a:b]
body=body.replace('DataImportSummaryTable(h)','readImportSummary()').replace('h.Data.Input','readInput()').replace('h.Data.RowIds','readRowIds()').replace('h.FitOptions','fitOptions').replace('h.Result','result')
extend('BESHStat.Excel.Common/src/Analysis/UdfPsmDataPreparation38.vb','''
        '''+"''' <summary>Selects existing Core table builders; never owns fitting, handles or caches.</summary>"+'''
        Public Shared Function BuildNamedTable(result As PsmComprehensiveResult, fitOptions As PsmComprehensiveFitOptions,
                                              readInput As Func(Of PsmInputData), readRowIds As Func(Of Integer()),
                                              requested As String, readImportSummary As Func(Of Object(,))) As Object(,)
'''+body+'''        End Function

''')
s=s[:a]+'''            Return Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.BuildNamedTable(h.Result, h.FitOptions, Function() h.Data.Input, Function() h.Data.RowIds, requested, Function() DataImportSummaryTable(h))
'''+s[b:]
calls={'PsmFrontEndTables.RowLevelAuditTable(h.Data.Input, h.Result.Result, h.Data.RowIds)':'scores','PsmBackendTables.MatchesTable(h.Result.Result, h.Data.Input)':'matches','PsmBackendTables.BalanceTable(h.Result.Result)':'balance','PsmBackendTables.EffectTable(h.Result.Result)':'effect','PsmAdvancedTables.LovePlotTable(h.Result.LovePlotRows)':'loveplot'}
for old,key in calls.items():assert old in s;s=s.replace(old,'ResolveNamedTable(h, "'+key+'")')
write(p,s)
extend('BESHStat.Excel.Common/src/Results/UdfClassificationReportPreparation38.vb','''
        Public Shared Function ComputeConfusionOutput(y() As Double, probabilities() As Double, threshold As Double,
                                                     weights() As Double, missingValue As Object, includeHeader As Boolean) As Object
            Dim summary = BinaryClassificationCore.ComputeBinarySummary(y, probabilities, threshold, weights)
            Return BuildBinaryCrosstabUdfOutput(summary, missingValue, includeHeader)
        End Function

        Public Shared Function ComputeThresholdOutput(y() As Double, probabilities() As Double, thresholds() As Double,
                                                     weights() As Double, missingValue As Object, includeHeader As Boolean) As Object
            Dim rows = BinaryClassificationCore.BuildThresholdTable(y, probabilities, thresholds, weights)
            Return BuildThresholdTableUdfOutput(rows, missingValue, includeHeader)
        End Function

        Public Shared Function ComputeCalibrationOutput(y() As Double, probabilities() As Double, bins As Integer,
                                                       weights() As Double, method As String, missingValue As Object, includeHeader As Boolean) As Object
            Dim rows = BinaryClassificationCore.BuildCalibrationBins(y, probabilities, bins, weights, method)
            Return BuildCalibrationTableUdfOutput(rows, missingValue, includeHeader)
        End Function

        '''+"''' <summary>Input validation/header acquisition remain at their original adapter positions.</summary>"+'''
        Public Shared Function ComputeBrierOutput(y() As Double, probabilities() As Double, weights() As Double,
                                                 missingValue As Object, includeHeader As Boolean) As Object
            Dim score As Double = BinaryClassificationCore.ComputeBrierScore(y, probabilities, weights)
            Dim eventRate As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ComputeWeightedEventRate(y, weights)
            Dim n As Double = If(weights Is Nothing, y.Length, System.Linq.Enumerable.Sum(weights))
            Return BuildBrierScoreUdfOutput(score, missingValue, n, eventRate, includeHeader)
        End Function
''')
p='BESHstatNG/src/ExcelUDFs/ClassificationReportUDFs.vb';s=read(p)
for name,start,call in [
('CLASS_CONFUSION','                Dim summary As regression.BinaryClassificationSummary =','ComputeConfusionOutput(observed, probs, cutoff, w, BinaryClassificationReporting.MissingOutputValue, hdr)'),
('CLASS_THRESH','                Dim rows As List(Of regression.BinaryThresholdRow) =','ComputeThresholdOutput(observed, probs, thresholdVector, w, BinaryClassificationReporting.MissingOutputValue, hdr)'),
('CLASS_CALIB','                Dim rows As List(Of regression.CalibrationBinSummary) =','ComputeCalibrationOutput(observed, probs, binCount, w, methodName, BinaryClassificationReporting.MissingOutputValue, hdr)'),
('CLASS_BRIER','                Dim brier As Double =','ComputeBrierOutput(observed, probs, w, BinaryClassificationReporting.MissingOutputValue, hdr)')]:
    a=s.index(start,s.index('Public Function '+name+'('));b=s.index('            Catch ex As Exception',a)
    s=s[:a]+'                Return ConvertBinaryClassificationOutputForExcel(Global.BESHStatNG.ExcelCommon.UdfClassificationReportPreparation38.'+call+')\n'+s[b:]
write(p,s)
extend('BESHStat.Excel.Common/src/Analysis/UdfPlotDataPreparation38.vb','''
        Public Shared Function ComputeRocTables(rocInput()() As Double, markerName As String, alpha As Double) As List(Of ResultTable)
            Dim model As New graphics.ROC(rocInput, New String() {If(String.IsNullOrWhiteSpace(markerName), "Marker", markerName)})
            model.compute(alpha)
            Return model.wrapResults()
        End Function

        Public Shared Function ComputeKaplanMeierCurves(records As List(Of survival.SurvivalRecord), alpha As Double) As Object()
            Dim model As New survival.Survival_KM_LR(records)
            Return model.SurvivalCurveTabularOutput(alpha)
        End Function

        Public Shared Function ComputeCalibrationRows(y() As Double, probabilities() As Double, bins As Integer,
                                                     weights() As Double, method As String) As List(Of regression.CalibrationBinSummary)
            regression.BinaryClassificationCore.ValidateBinaryInputs(y, probabilities, weights)
            Return regression.BinaryClassificationCore.BuildCalibrationBins(y, probabilities, bins, weights, method)
        End Function
''')
p='BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb';s=read(p)
old='''                Dim mdl As New graphics.ROC(rocInput, New String() {If(String.IsNullOrWhiteSpace(markerName), "Marker", markerName)})
                mdl.compute(alphaValue)
                Dim tables As List(Of ResultTable) = mdl.wrapResults()'''
assert s.count(old)==2;s=s.replace(old,'''                Dim tables As List(Of ResultTable) = Global.BESHStatNG.ExcelCommon.UdfPlotDataPreparation38.ComputeRocTables(rocInput, markerName, alphaValue)''')
old='''                Dim km As New survival.Survival_KM_LR(records)
                Dim rawCurves As Object() = km.SurvivalCurveTabularOutput(alphaValue)''';assert old in s;s=s.replace(old,'''                Dim rawCurves As Object() = Global.BESHStatNG.ExcelCommon.UdfPlotDataPreparation38.ComputeKaplanMeierCurves(records, alphaValue)''')
old='''                Global.BESHStatNG.regression.BinaryClassificationCore.ValidateBinaryInputs(yy, pp, w)
                Dim rows As List(Of regression.CalibrationBinSummary) = regression.BinaryClassificationReporting.BuildCalibrationBins(yy, pp, binCount, w, methodName)''';assert old in s;s=s.replace(old,'''                Dim rows As List(Of regression.CalibrationBinSummary) = Global.BESHStatNG.ExcelCommon.UdfPlotDataPreparation38.ComputeCalibrationRows(yy, pp, binCount, w, methodName)''');write(p,s)
print('Wave B: shared existing PSM table dispatch,classification results and ROC/KM/calibration engine-backed payloads;algorithms/caches/defaults unchanged',flush=True)
