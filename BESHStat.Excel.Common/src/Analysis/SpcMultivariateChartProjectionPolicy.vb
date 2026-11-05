Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    ''' <summary>Detached presentation strings read by the native caller after Fit.</summary>
    Public NotInheritable Class SpcMultivariateChartIntent
        Public Sub New(chartTitle As String, valueAxisTitle As String, fallbackCaption As String)
            Me.ChartTitle = chartTitle : Me.ValueAxisTitle = valueAxisTitle : Me.FallbackCaption = fallbackCaption
        End Sub
        Public ReadOnly Property ChartTitle As String
        Public ReadOnly Property ValueAxisTitle As String
        Public ReadOnly Property FallbackCaption As String
    End Class

    ''' <summary>Original renderer-compatible Core wrapper; series preparation delegates to accepted 04A.22.
    ''' No chart collection, styling, marshalling, pixel geometry or statistical computation.</summary>
    Public NotInheritable Class SpcMultivariateChartPayload
        Friend Sub New(result As SpcFitResult, family As SpcMultivariateFamilyPresentation)
            RenderingResult = result : Me.Family = family
        End Sub
        Public ReadOnly Property RenderingResult As SpcFitResult
        Public ReadOnly Property Family As SpcMultivariateFamilyPresentation
        Public Function ProjectSeries(useSequence As Boolean) As SpcUnivariateChartProjection
            Return SpcUnivariateChartProjectionPolicy.Project(RenderingResult, useSequence)
        End Function
    End Class

    Public NotInheritable Class SpcMultivariateChartProjectionPolicy
        Private Sub New()
        End Sub
        Public Shared Function Prepare(report As SpcMultivariateReportProjection, intent As SpcMultivariateChartIntent) As SpcMultivariateChartPayload
            If report Is Nothing Then Throw New ArgumentNullException(NameOf(report))
            If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))
            Dim result = report.Result, context = report.Context
            Dim sourcePanels = context.Panels
            Dim panels(sourcePanels.Count - 1) As SpcPanelResult
            For i As Integer = 0 To sourcePanels.Count - 1
                Dim source = sourcePanels(i)
                panels(i) = New SpcPanelResult(source.PanelType, source.DisplayName, source.Points,
                    valueAxisTitle:=If(String.IsNullOrWhiteSpace(intent.ValueAxisTitle), source.ValueAxisTitle, intent.ValueAxisTitle.Trim()),
                    parameterEstimates:=source.ParameterEstimates, signals:=source.Signals, warnings:=source.Warnings)
            Next
            Dim input = SpcMultivariateResultAxisPolicy.RenderingInput(panels(0))
            Dim title = If(String.IsNullOrWhiteSpace(intent.ChartTitle), intent.FallbackCaption, intent.ChartTitle.Trim())
            Dim request As New SpcFitRequest(result.Request.ChartType, input, requestLabel:=result.Request.RequestLabel,
                chartTitle:=title, valueAxisTitle:=intent.ValueAxisTitle.Trim())
            Return New SpcMultivariateChartPayload(New SpcFitResult(request, panels, result.Warnings, result.ExecutionTimeMilliseconds), report.Family)
        End Function
    End Class
End Namespace
