Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.ExcelCommon

''' <summary>Host-free shipping seam. Controls/text parsing, importer, Core fitting/progress,
''' cancellation and every worksheet/chart operation remain in UiGLM/Windows.</summary>
Public NotInheritable Class WindowsRegressionRequestAdapter
    Private Sub New()
    End Sub
    Public Shared Function Capture(kind As RegressionAnalysisKind, responseKey As String, raw As glmData,
                                   expanded(,) As Double, names() As String, effects As RegressionEffectModel,
                                   options As RegressionOptions, Optional initial() As Double = Nothing,
                                   Optional logisticRaw As glmData = Nothing, Optional logisticExpanded(,) As Double = Nothing,
                                   Optional logisticNames() As String = Nothing, Optional logisticEffects As RegressionEffectModel = Nothing,
                                   Optional logisticInitial() As Double = Nothing) As RegressionRequest
        If raw Is Nothing Then Throw New ArgumentNullException(NameOf(raw))
        Dim primary = Component(responseKey, raw, expanded, names, effects, initial)
        Dim logistic As RegressionComponent = Nothing
        If logisticRaw IsNot Nothing Then logistic = Component(responseKey, logisticRaw, logisticExpanded, logisticNames, logisticEffects, logisticInitial)
        Return New RegressionRequest(kind, responseKey, primary, options,
            If(raw.bOffset, New RegressionVector(raw.OffsetVarName, raw.OffsetData, raw.RowIds), Nothing),
            If(raw.bWeights, New RegressionVector(raw.WeightVarName, raw.WeightData, raw.RowIds), Nothing), logistic)
    End Function

    Private Shared Function Component(response As String, raw As glmData, expanded(,) As Double, names() As String,
                                      effects As RegressionEffectModel, initial() As Double) As RegressionComponent
        Dim keys As New List(Of String) From {response}
        If effects IsNot Nothing Then keys.AddRange(RegressionDesignCore.GetRequiredRawVarKeys(effects.Effects.Select(Function(e) e.Key).ToArray(), effects.ToTermSpecs()))
        Dim sources As List(Of SpreadsheetTransportCoordinate) = Nothing
        Dim evidence = ExcelDnaDataImporter.GetPhysicalImportEvidence(raw.CoreTable)
        If evidence IsNot Nothing AndAlso evidence.Data IsNot Nothing AndAlso raw.RowIds IsNot Nothing Then
            sources = New List(Of SpreadsheetTransportCoordinate)
            Dim indices = evidence.Data.RowIds.Select(Function(id, index) New With {.Id = id, .Index = index}).ToDictionary(Function(v) v.Id, Function(v) v.Index)
            For Each id In raw.RowIds
                Dim index As Integer
                If Not indices.TryGetValue(id, index) Then Throw New ArgumentException("regression.primary.provenance: Selected row is absent from import transport.")
                sources.Add(evidence.Data.SourceAt(index, 0))
            Next
        End If
        Return New RegressionComponent(expanded, names, raw.RowIds, effects, keys.ToArray(), initial, sources)
    End Function

    Public Shared Function Validate(request As RegressionRequest) As RegressionValidation
        Return RegressionRequestPolicy.Validate(request)
    End Function

    Public Shared Function Report(request As RegressionRequest, tables As List(Of ResultTable), fitted(,) As Double,
                                  residuals(,) As Object, Optional converged As Nullable(Of Boolean) = Nothing,
                                  Optional categories() As Integer = Nothing, Optional fittedRowIds() As Integer = Nothing) As RegressionReportPlan
        Dim rows = If(fittedRowIds, request.Primary.RowIds)
        Return RegressionReportPolicy.Compose(request, tables, New RegressionRowOutput(rows, fitted, residuals, 1, categories), converged)
    End Function

    Public Shared Function FittedColumn(values() As Double) As Double(,)
        If values Is Nothing Then Return Nothing
        Dim result(values.Length - 1, 0) As Double
        For i = 0 To values.Length - 1 : result(i, 0) = values(i) : Next
        Return result
    End Function

    ''' <summary>04A.10 shipping response seam; Core wrapResults and fitted arrays are captured
    ''' before the existing native writer. The six-family overload remains compatible.</summary>
    Public Shared Function Report(intent As LinearBinomialFitIntent, tables As List(Of ResultTable),
                                  fitted() As Double, residuals(,) As Object,
                                  Optional converged As Nullable(Of Boolean) = Nothing,
                                  Optional actualFitAlpha As Nullable(Of Double) = Nothing) As LinearBinomialReport
        Return LinearBinomialRegressionPolicy.Compose(intent, tables, fitted, residuals, converged, actualFitAlpha)
    End Function

    Public Shared Function Report(intent As ExtendedRegressionIntent, tables As List(Of ResultTable),
                                  fitted(,) As Double, residuals(,) As Object,
                                  Optional converged As Nullable(Of Boolean) = Nothing,
                                  Optional categories() As Integer = Nothing,
                                  Optional estimatedDispersion As Nullable(Of Double) = Nothing) As ExtendedRegressionReport
        Return ExtendedRegressionFamilyPolicy.Compose(intent, tables,
            New RegressionRowOutput(intent.Primary.RowIds, fitted, residuals, 1, categories), converged, estimatedDispersion)
    End Function

    ''' <summary>Temporary native-only Data-header compatibility quarantine for C10-COV-NAMES-01.
    ''' Core/Common identifiers are fixed and never mutated. Remove these writer-call substitutions
    ''' only under C11-NATIVE-CORRECTIONS-01; the tested removal patch remains unapplied.</summary>
    Public Shared Function DeferredCovarianceDataHeaders(request As RegressionRequest, names() As String) As String()
        If names Is Nothing Then Return Nothing
        Dim result = DirectCast(names.Clone(), String())
        If (request.Kind = RegressionAnalysisKind.GeneralizedLinearModel OrElse request.Kind = RegressionAnalysisKind.NegativeBinomial2) AndAlso
            request.Options.Covariance AndAlso request.Primary.Effects.IncludeIntercept AndAlso result.Length > 0 Then result(0) = "Intercept"
        Return result
    End Function
End Class
