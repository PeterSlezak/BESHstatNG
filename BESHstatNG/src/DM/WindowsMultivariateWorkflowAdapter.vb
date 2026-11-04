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
            RenderPcaRequest(plot, report.Input, model)
        Next
    End Sub
    Public Shared Sub RenderPca(projection As MultivariateComponentProjection, model As Multivariate.PCA)
        For Each binding In projection.PlotData
            RenderPcaRequest(binding.Request, projection.Input, model)
        Next
    End Sub
    Private Shared Sub RenderPcaRequest(plot As MultivariatePlotIntent, input As MultivariateInputIntent, model As Multivariate.PCA)
            Select Case plot.Kind
                Case MultivariatePlotKind.Scree : graphics.PcaPlotExcel.ScreePlot(model)
                Case MultivariatePlotKind.Scores2D : graphics.PcaPlotExcel.ScorePlot2D(model, input.RowLabels, input.GroupLabels)
                Case MultivariatePlotKind.Loadings2D : graphics.PcaPlotExcel.LoadingPlot2D(model)
                Case MultivariatePlotKind.Biplot : graphics.PcaPlotExcel.Biplot(model, plot.Scaling, input.RowLabels, input.GroupLabels)
                Case MultivariatePlotKind.Scores3D : graphics.PcaPlotExcel.ScorePlot3D(model, input.RowLabels, input.GroupLabels)
                Case MultivariatePlotKind.Loadings3D : graphics.PcaPlotExcel.LoadingPlot3D(model)
            End Select
    End Sub
    Public Shared Sub RenderMca(report As MultivariateReport, model As Multivariate.CA)
        For Each plot In report.Plots
            RenderMcaRequest(plot, model)
        Next
    End Sub
    Public Shared Sub RenderMca(projection As MultivariateComponentProjection, model As Multivariate.CA)
        For Each binding In projection.PlotData
            RenderMcaRequest(binding.Request, model)
        Next
    End Sub
    Private Shared Sub RenderMcaRequest(plot As MultivariatePlotIntent, model As Multivariate.CA)
            Select Case plot.Kind
                Case MultivariatePlotKind.Correspondence : graphics.CorrespondenceAnalysisPlotExcel.CorrespondencePlot(model)
                Case MultivariatePlotKind.Contribution : graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(model, plot.Axis, plot.RowContribution)
            End Select
    End Sub
    Private Shared Sub WriteSection(writer As ResultTableWriterBase, section As MultivariateResultSection, payload As Object, tall As Boolean)
        writer.setRowPointer(section.StartRow)
        writer.setColumnPointer(section.StartColumn)
        writer.write(payload, tall)
    End Sub
    Public Shared Sub WritePcaData(writer As ResultTableWriterBase, original(,) As Object, model As Multivariate.PCA, projection As MultivariateComponentProjection)
        MultivariateComponentProjectionPolicy.VisitPca(projection, original, model, Sub(section, payload, tall) WriteSection(writer, section, payload, tall))
    End Sub
    Public Shared Sub WriteMcaData(writer As ResultTableWriterBase, original(,) As Object, model As Multivariate.CA, projection As MultivariateComponentProjection)
        MultivariateComponentProjectionPolicy.VisitMca(projection, original, model, Sub(section, payload, tall) WriteSection(writer, section, payload, tall))
    End Sub
    Public Shared Function CaptureLabelledCa(data As DataObj, labels() As String) As MultivariateContingencyInput
        Return MultivariateCorrespondenceInputPolicy.Labelled(data.nRows, data.nCols, data.varNames, data.RowIds, labels)
    End Function
    Public Shared Function ConvertLabelledCaCounts(data As DataObj, input As MultivariateContingencyInput) As Integer(,)
        If input.CandidateOnly Then Throw New InvalidOperationException("Dormant unlabelled CA mapping cannot activate the Windows conversion path.")
        Dim counts(input.RowCount - 1, input.CountColumnCount - 1) As Integer
        For i = 0 To UBound(data.FinalData, 1)
            For j = input.FirstCountSourceColumn To UBound(data.FinalData, 2)
                counts(i, input.ResultColumn(j)) = CInt(data.FinalData(i, j))
            Next
        Next
        Return counts
    End Function
    Public Shared Sub RenderCa(projection As MultivariateComponentProjection, model As Multivariate.CA,
                               Optional ws As Microsoft.Office.Interop.Excel.Worksheet = Nothing)
        For Each binding In projection.PlotData
            Dim plot = binding.Request
            Select Case plot.Kind
                Case MultivariatePlotKind.Contribution : graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(model, plot.Axis, plot.RowContribution, ws)
                Case MultivariatePlotKind.Correspondence : graphics.CorrespondenceAnalysisPlotExcel.CorrespondencePlot(model, ws)
            End Select
        Next
    End Sub
    Public Shared Sub WriteClusterData(writer As ResultTableWriterBase, original(,) As Object, request As MultivariateKMeansRequest)
        writer.setRowPointer(request.DataSection.HeaderRow)
        writer.setColumnPointer(request.DataSection.FirstColumn)
        writer.write(request.DataTable(original))
    End Sub
    Public Shared Sub WriteClusterData(writer As ResultTableWriterBase, original(,) As Object, request As MultivariateHierarchicalRequest)
        writer.setRowPointer(request.DataSection.HeaderRow)
        writer.setColumnPointer(request.DataSection.FirstColumn)
        writer.write(request.DataTable(original))
    End Sub
    Public Shared Sub RenderDendrogram(result As Multivariate.HierarchicalClusterResult, ws As Microsoft.Office.Interop.Excel.Worksheet, request As MultivariateDendrogramRequest)
        graphics.ClusterAnalysisPlotExcel.CreateDendrogramChart(result, ws:=ws,
            topLeftCellAddress:=request.Anchor, chartWidth:=request.Width, chartHeight:=request.Height,
            heightMode:=request.HeightMode, orientation:=request.Orientation, labelMode:=request.LabelMode,
            chartTitle:=request.Title, cutMode:=request.Cut.Mode,
            membershipClusterCount:=request.Cut.Clusters, membershipCutHeight:=request.Cut.Height)
    End Sub
    Public Shared Sub WriteFactorData(writer As ResultTableWriterBase, original(,) As Object, projection As MultivariateFactorProjection)
        writer.setRowPointer(projection.Request.DataSection.HeaderRow)
        writer.setColumnPointer(projection.Request.DataSection.FirstColumn)
        writer.write(projection.Request.DataTable(original))
    End Sub
    Public Shared Sub WriteDiscriminantData(writer As ResultTableWriterBase, original(,) As Object, groups() As Object, projection As MultivariateDiscriminantProjection)
        writer.setRowPointer(projection.Request.DataSection.HeaderRow)
        writer.setColumnPointer(projection.Request.DataSection.FirstColumn)
        writer.write(projection.Request.DataTable(original, groups))
    End Sub
    Public Shared Sub RenderFactorProjection(projection As MultivariateFactorProjection, model As Multivariate.FactorAnalysis)
        For Each binding In projection.Plots
            Select Case binding.Request.Kind
                Case MultivariatePlotKind.Scree : graphics.FactorAnalysisPlotExcel.ScreePlot(model)
                Case MultivariatePlotKind.Loadings2D : graphics.FactorAnalysisPlotExcel.LoadingPlot2D(model)
                Case MultivariatePlotKind.Loadings3D : graphics.FactorAnalysisPlotExcel.LoadingPlot3D(model)
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
