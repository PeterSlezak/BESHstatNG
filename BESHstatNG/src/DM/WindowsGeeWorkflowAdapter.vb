Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.ExcelCommon

''' <summary>Shipping host-free GEE bridge. The form keeps UI parsing, range acquisition,
''' progress/cancellation, the actual Fit call, workbook writers and every chart.</summary>
Public NotInheritable Class WindowsGeeWorkflowAdapter
    Private Sub New()
    End Sub
    Public Shared Function Capture(response As String, raw As geeData, expanded(,) As Double, names() As String,
                                   effects As RegressionEffectModel, meanOptions As RegressionOptions,
                                   options As GeeWorkflowOptions, Optional initial() As Double = Nothing) As GeeWorkflowIntent
        If raw Is Nothing Then Throw New ArgumentNullException(NameOf(raw))
        Dim keys As New List(Of String) From {response}
        If effects IsNot Nothing Then keys.AddRange(RegressionDesignCore.GetRequiredRawVarKeys(effects.Effects.Select(Function(e) e.Key).ToArray(), effects.ToTermSpecs()))
        Dim sources As List(Of SpreadsheetTransportCoordinate) = Nothing
        Dim evidence = ExcelDnaDataImporter.GetPhysicalImportEvidence(raw.CoreTable)
        If evidence IsNot Nothing AndAlso evidence.Data IsNot Nothing AndAlso raw.RowIds IsNot Nothing Then
            sources = New List(Of SpreadsheetTransportCoordinate)
            Dim indices = evidence.Data.RowIds.Select(Function(id, index) New With {.Id = id, .Index = index}).ToList()
            For Each id In raw.RowIds
                Dim matches = indices.Where(Function(v) v.Id = id).Take(2).ToArray()
                ' Preserve available coordinates; ambiguity stays a typed diagnostic,
                ' never a new strict Windows input-refusal path.
                sources.Add(If(matches.Length = 1, evidence.Data.SourceAt(matches(0).Index, 0), Nothing))
            Next
        End If
        Dim primary As New RegressionComponent(expanded, names, raw.RowIds, effects, keys.ToArray(), initial, sources)
        Dim mean As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, response, primary, meanOptions,
            If(raw.bOffset, New RegressionVector(raw.OffsetVarName, raw.OffsetData, raw.RowIds), Nothing),
            If(raw.bWeights, New RegressionVector(raw.WeightVarName, raw.WeightData, raw.RowIds), Nothing))
        Return GeeWorkflowPolicy.Project(mean, New GeeClusterVector(raw.ClusterIdVarName, raw.ClusterIdData, raw.RowIds),
            If(raw.bTime, New RegressionVector(raw.TimeVarName, raw.TimeData, raw.RowIds), Nothing), options)
    End Function
    Public Shared Function Report(intent As GeeWorkflowIntent, tables As List(Of ResultTable), corePredictions() As Double,
                                  residuals(,) As Object, observed() As Double, weights() As Double,
                                  Optional workingCorrelation(,) As Double = Nothing) As GeeReport
        Return GeeReportPolicy.Compose(intent, tables, corePredictions, residuals, observed, weights, workingCorrelation)
    End Function
    ''' <summary>C12-NATIVE-CLASSIFICATION-01: quarantine the original post-fit selected-family
    ''' behavior in Windows. Common GEE classification correctly uses the fitted family.
    ''' A source-pinned unapplied guard removes this mismatch path after native timing approval.</summary>
    Public Shared Function DeferredPostFitFamilyReport(report As GeeReport, postFit As RegressionOptions,
                                                      eventRate As Double) As RegressionClassificationReport
        Dim values = report.Rows.Fitted
        Dim predicted = Enumerable.Range(0, values.GetLength(0)).Select(Function(i) values(i, 0)).ToArray()
        Return RegressionClassificationPolicy.Compose(report.Observed, predicted, report.ObservationWeights, postFit, eventRate, "GEE Binary Classification")
    End Function
End Class
