Option Explicit On

Imports System.Linq
Imports BESHStatNG.ExcelCommon
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

            contrib = MultivariatePlotGeometry37.Contribution(model, lAxis, bRow)

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
                .ChartTitle.Text = MultivariateChartPresentation37.ContributionTitle(lAxis)
                If model.IsMultiple Then .Name = MultivariateChartPresentation37.ContributionName(lAxis)
                .HasLegend = False

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                seriesID = 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .XValues = MultivariateChartPresentation37.ContributionLabels(model, bRow)
                    .Values = contrib
                End With

                Try
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = MultivariateChartPresentation37.ContributionAxisTitle
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

            Dim axes = MultivariatePlotGeometry37.CorrespondenceAxes(model)
            Dim udPlotAxisY As ChartAxisScaleResult = axes.Y
            Dim udPlotAxisX As ChartAxisScaleResult = axes.X

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
                If model.IsMultiple Then .Name = MultivariateChartPresentation37.CorrespondenceName

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
                Dim columns = MultivariateChartPresentation37.CorrespondenceSeries(model, False)
                With .SeriesCollection(seriesID)
                    .XValues = columns.XValues
                    .Values = columns.YValues
                    .Name = columns.Name
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                    .MarkerSize = 6
                    .MarkerForegroundColor = GetColor(10)
                    .MarkerBackgroundColor = GetColor(10)

                    For i As Integer = 1 To model.ColumNames.Length
                        .Points(i).HasDataLabel = True
                        .Points(i).DataLabel.Text = MultivariateChartPresentation37.CorrespondenceLabel(model, False, i - 1)
                    Next
                End With

                If MultivariateChartPresentation37.ShowCorrespondenceRows(model) Then
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    Dim rows = MultivariateChartPresentation37.CorrespondenceSeries(model, True)
                    With .SeriesCollection(seriesID)
                        .XValues = rows.XValues
                        .Values = rows.YValues
                        .Name = rows.Name
                        .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                        .MarkerSize = 6
                        .MarkerForegroundColor = RGB(0, 0, 150)
                        .MarkerBackgroundColor = RGB(0, 0, 150)

                        For i As Integer = 1 To model.rowNames.Length
                            .Points(i).HasDataLabel = True
                            .Points(i).DataLabel.Text = MultivariateChartPresentation37.CorrespondenceLabel(model, True, i - 1)
                        Next
                    End With
                End If

                .SeriesCollection.NewSeries
                seriesID += 1
                With .SeriesCollection(seriesID)
                    .XValues = ChartReferenceSeries.Horizontal(udPlotAxisX.Min, udPlotAxisX.Max, 0).XValues
                    .Values = MultivariatePlotGeometry37.ZeroPair()
                    .Name = MultivariateChartPresentation37.YZeroName
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
                    .XValues = MultivariatePlotGeometry37.ZeroPair()
                    .Values = ChartReferenceSeries.Vertical(0, udPlotAxisY.Min, udPlotAxisY.Max).YValues
                    .Name = MultivariateChartPresentation37.XZeroName
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 0.5
                    End With
                End With

                Try
                    If Not MultivariateChartPresentation37.ShowCorrespondenceRows(model) Then
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
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = MultivariateChartPresentation37.CorrespondenceCaption(pct(2, 1), 2)
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = MultivariateChartPresentation37.CorrespondenceCaption(pct(1, 1), 1)
                    .ChartTitle.Text = MultivariateChartPresentation37.CorrespondenceTitle
                    .ChartTitle.Font.Bold = True
                Catch
                End Try
            End With
        End Sub

    End Class

End Namespace
