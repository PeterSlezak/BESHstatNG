Option Explicit On
Option Strict On
Imports System
Imports BESHStatNG.ExcelCommon

''' <summary>Post-acquisition bridge only. Original importers, controls, culture-aware
''' Excel numeric parsing, Core calculation and native writer lifetime remain Windows-owned.</summary>
Public NotInheritable Class WindowsMultivariateWorkflowAdapter
    Private Sub New()
    End Sub
    Public Shared Function Capture(kind As MultivariateAnalysisKind, data As DataObj,
                                   Optional rowLabels() As String = Nothing, Optional groups() As String = Nothing,
                                   Optional selectedKeys() As String = Nothing) As MultivariateInputIntent
        Return New MultivariateInputIntent(kind, data.nRows, data.nCols, data.varNames, data.RowIds, rowLabels, groups, selectedKeys)
    End Function
    Public Shared Sub RenderPca(report As MultivariateReport, model As Multivariate.PCA)
        For Each plot In report.Plots
            Select Case plot.Kind
                Case MultivariatePlotKind.Scree : graphics.PcaPlotExcel.ScreePlot(model)
                Case MultivariatePlotKind.Scores2D : graphics.PcaPlotExcel.ScorePlot2D(model, report.Input.RowLabels, report.Input.GroupLabels)
                Case MultivariatePlotKind.Loadings2D : graphics.PcaPlotExcel.LoadingPlot2D(model)
                Case MultivariatePlotKind.Biplot : graphics.PcaPlotExcel.Biplot(model, plot.Scaling, report.Input.RowLabels, report.Input.GroupLabels)
                Case MultivariatePlotKind.Scores3D : graphics.PcaPlotExcel.ScorePlot3D(model, report.Input.RowLabels, report.Input.GroupLabels)
                Case MultivariatePlotKind.Loadings3D : graphics.PcaPlotExcel.LoadingPlot3D(model)
            End Select
        Next
    End Sub
    Public Shared Sub RenderMca(report As MultivariateReport, model As Multivariate.CA)
        For Each plot In report.Plots
            Select Case plot.Kind
                Case MultivariatePlotKind.Correspondence : graphics.CorrespondenceAnalysisPlotExcel.CorrespondencePlot(model)
                Case MultivariatePlotKind.Contribution : graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(model, plot.Axis, plot.RowContribution)
            End Select
        Next
    End Sub
    Public Shared Sub RenderFactor(report As MultivariateReport, model As Multivariate.FactorAnalysis)
        For Each plot In report.Plots
            Select Case plot.Kind
                Case MultivariatePlotKind.Scree : graphics.FactorAnalysisPlotExcel.ScreePlot(model)
                Case MultivariatePlotKind.Loadings2D : graphics.FactorAnalysisPlotExcel.LoadingPlot2D(model)
                Case MultivariatePlotKind.Loadings3D : graphics.FactorAnalysisPlotExcel.LoadingPlot3D(model)
            End Select
        Next
    End Sub
End Class
