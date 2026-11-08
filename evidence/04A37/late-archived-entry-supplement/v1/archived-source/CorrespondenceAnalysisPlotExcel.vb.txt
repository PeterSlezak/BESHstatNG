Option Explicit On

Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Excel-specific renderer for correspondence analysis charts.
    ''' </summary>
    ''' <remarks>
    ''' This adapter keeps Excel chart creation out of the CA/MCA statistical model class.
    ''' </remarks>
    Public NotInheritable Class CorrespondenceAnalysisPlotExcel

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Creates an Excel chart of contributions to a selected CA/MCA axis.
        ''' </summary>
        Public Shared Sub ContributionPlot(model As Multivariate.CA,
                                           lAxis As Integer,
                                           bRow As Boolean,
                                           Optional ws As Worksheet = Nothing)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            Dim figure As Chart
            Dim seriesID As Integer
            Dim contrib() As Double

            If bRow Then
                contrib = model.RowContribution(lAxis)
            Else
                contrib = model.ColContribution(lAxis)
            End If

            If ws Is Nothing Then
                AppGlobals.app.Charts.Add()
                figure = AppGlobals.app.ActiveWorkbook.ActiveChart
            Else
                figure = ws.Shapes.AddChart.Chart
            End If

            With figure
                .ChartType = XlChartType.xlColumnClustered
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Contribution Plot: Axis " & CStr(lAxis + 1)
                If model.IsMultiple Then .Name = "Contribution" & CStr(lAxis + 1)
                .HasLegend = False

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                seriesID = 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .XValues = model.PlotLabels(bRow, vbNewLine)
                    .Values = contrib
                End With

                Try
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = "Contribution"
                    .ChartTitle.Font.Bold = True
                Catch
                End Try
            End With
        End Sub

        ''' <summary>
        ''' Creates a 2D correspondence map (Factor 1 vs Factor 2) in Excel.
        ''' </summary>
        Public Shared Sub CorrespondencePlot(model As Multivariate.CA,
                                             Optional ws As Worksheet = Nothing)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.FactorCount < 2 Then Exit Sub

            Dim seriesID As Integer
            Dim figure As Chart

            Dim udPlotAxisY As CHARTscale = ChartScaling(Math.Min(model.RowFactors(1).Min(), model.ColFactors(1).Min()),
                                                         Math.Max(model.RowFactors(1).Max(), model.ColFactors(1).Max()))
            Dim udPlotAxisX As CHARTscale = ChartScaling(Math.Min(model.RowFactors(0).Min(), model.ColFactors(0).Min()),
                                                         Math.Max(model.RowFactors(0).Max(), model.ColFactors(0).Max()))

            If ws Is Nothing Then
                AppGlobals.app.Charts.Add()
                figure = AppGlobals.app.ActiveWorkbook.ActiveChart
            Else
                figure = ws.Shapes.AddChart.Chart
            End If

            With figure
                .ChartType = XlChartType.xlXYScatter
                .ChartStyle = 1
                .HasTitle = False
                .HasTitle = True
                If model.IsMultiple Then .Name = "CA plot"

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                .Axes(XlAxisType.xlValue).MajorGridlines.Delete
                .Axes(XlAxisType.xlCategory).MinimumScale = udPlotAxisX.Min
                .Axes(XlAxisType.xlCategory).MaximumScale = udPlotAxisX.Max
                .Axes(XlAxisType.xlCategory).MajorUnit = udPlotAxisX.Scale
                .Axes(XlAxisType.xlValue).CrossesAt = -1.0E+100
                .Axes(XlAxisType.xlCategory).CrossesAt = -1.0E+100
                .Axes(XlAxisType.xlValue).MinimumScale = udPlotAxisY.Min
                .Axes(XlAxisType.xlValue).MaximumScale = udPlotAxisY.Max
                .Axes(XlAxisType.xlValue).MajorUnit = udPlotAxisY.Scale

                seriesID = 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .XValues = model.ColFactors(0)
                    .Values = model.ColFactors(1)
                    .Name = "Columns"
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                    .MarkerSize = 6
                    .MarkerForegroundColor = GetColor(10)
                    .MarkerBackgroundColor = GetColor(10)

                    For i As Integer = 1 To model.ColumNames.Length
                        .Points(i).HasDataLabel = True
                        .Points(i).DataLabel.Text = model.PlotLabels(False, vbNullString)(i - 1)
                    Next
                End With

                If Not model.IsMultiple Then
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .XValues = model.RowFactors(0)
                        .Values = model.RowFactors(1)
                        .Name = "Rows"
                        .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                        .MarkerSize = 6
                        .MarkerForegroundColor = RGB(0, 0, 150)
                        .MarkerBackgroundColor = RGB(0, 0, 150)

                        For i As Integer = 1 To model.rowNames.Length
                            .Points(i).HasDataLabel = True
                            .Points(i).DataLabel.Text = model.PlotLabels(True, vbNullString)(i - 1)
                        Next
                    End With
                End If

                .SeriesCollection.NewSeries
                seriesID += 1
                With .SeriesCollection(seriesID)
                    .XValues = {udPlotAxisX.Min, udPlotAxisX.Max}
                    .Values = {0, 0}
                    .Name = "Y Zero Line"
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 0.5
                    End With
                End With

                .SeriesCollection.NewSeries
                seriesID += 1
                With .SeriesCollection(seriesID)
                    .XValues = {0, 0}
                    .Values = {udPlotAxisY.Min, udPlotAxisY.Max}
                    .Name = "X Zero Line"
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 0.5
                    End With
                End With

                Try
                    If model.IsMultiple Then
                        .HasLegend = False
                    Else
                        For i As Integer = 4 To 3 Step -1
                            .Legend.LegendEntries(i).Delete
                        Next
                    End If
                Catch
                End Try

                Try
                    Dim pct As Double(,) = model.Percents
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = $"Factor 2 [{ Format$(pct(2, 1), "#0.0#") }%]"
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = $"Factor 1 [{ Format$(pct(1, 1), "#0.0#") }%]"
                    .ChartTitle.Text = "Correspondence Plot"
                    .ChartTitle.Font.Bold = True
                Catch
                End Try
            End With
        End Sub

    End Class

End Namespace