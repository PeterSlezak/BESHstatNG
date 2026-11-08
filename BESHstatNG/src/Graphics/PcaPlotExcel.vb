Option Explicit On

Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Excel-specific renderer for PCA plots.
    ''' </summary>
    ''' <remarks>
    ''' This adapter keeps worksheet/chart creation out of the PCA statistical model class.
    ''' The PCA class should compute scores, loadings, eigenvalues, and explained-variance
    ''' data only; Excel chart rendering belongs to the Excel-DNA graphics/front-end layer.
    ''' </remarks>
    Public NotInheritable Class PcaPlotExcel

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Creates a 3D scatter plot of variable loadings on PC1, PC2, and PC3.
        ''' </summary>
        Public Shared Sub LoadingPlot3D(model As Multivariate.PCA)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 3 Then Exit Sub

            Dim loadings As Double(,) = model.GetLoadings
            Dim pc1() As Double = MultivariatePlotGeometry37.Column(loadings, 0)
            Dim pc2() As Double = MultivariatePlotGeometry37.Column(loadings, 1)
            Dim pc3() As Double = MultivariatePlotGeometry37.Column(loadings, 2)
            Dim pct() As Double = model.PercentExpl

            Dim XYZ As New XYZscatter
            With XYZ
                .ChartName = MultivariateChartPresentation37.PcaLoading3DName
                .dataInputs(pc1, pc2, pc3)
                .axesLabelInputs(MultivariateChartPresentation37.ComponentCaption(pct(0), 0, False),
                                 MultivariateChartPresentation37.ComponentCaption(pct(1), 1, False),
                                 MultivariateChartPresentation37.ComponentCaption(pct(2), 2, False))
                .showPlanePointInputs(True, True, True, 3, 3, 3)
                .ScaleAxis(False)
                .settingsInputs(True, True, True)
                .SetDataLabels(model.VariableNames)
                .draw()
            End With
        End Sub

        ''' <summary>
        ''' Creates a 2D scatter plot of variable loadings on PC1 vs PC2.
        ''' </summary>
        Public Shared Sub LoadingPlot2D(model As Multivariate.PCA)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 2 Then Exit Sub

            Dim loadings As Double(,) = model.GetLoadings
            Dim varNames() As String = model.VariableNames
            Dim p As Integer = model.VariableCount
            Dim pct() As Double = model.PercentExpl

            Dim pc1() As Double = MultivariatePlotGeometry37.Column(loadings, 0)
            Dim pc2() As Double = MultivariatePlotGeometry37.Column(loadings, 1)
            Dim axes = MultivariatePlotGeometry37.SymmetricAxes(pc1, pc2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = MultivariateChartPresentation37.PcaLoading2DName
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                ConfigureScatterAxes(.Axes(XlAxisType.xlCategory), udAxisX)
                ConfigureScatterAxes(.Axes(XlAxisType.xlValue), udAxisY)

                Dim series_id As Integer = 0
                For id As Integer = 0 To p - 1
                    .SeriesCollection.NewSeries
                    series_id += 1
                    With .SeriesCollection(series_id)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = MultivariatePlotGeometry37.LoadingVector(pc1(id))
                        .Values = MultivariatePlotGeometry37.LoadingVector(pc2(id))
                        .Name = MultivariateChartPresentation37.LoadingSeriesName(id)
                        .Format.Line.Weight = 1
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(0, 0, 150)
                        .Format.Line.EndArrowheadStyle = 2 'msoArrowheadTriangle

                        .points(2).HasDataLabel = True
                        .points(2).DataLabel.text = CStr(varNames(id))
                        .points(2).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(2).DataLabel.Font.Size = 11
                        .points(2).DataLabel.Font.Color = RGB(0, 0, 150)
                    End With
                Next id

                AddZeroLines(.SeriesCollection, series_id, udAxisX, udAxisY)
                DeleteLegendIfPresent(AppGlobals.app.ActiveWorkbook.ActiveChart)
                SetAxisTitles(AppGlobals.app.ActiveWorkbook.ActiveChart,
                              MultivariateChartPresentation37.ComponentCaption(pct(0), 0, False),
                              MultivariateChartPresentation37.ComponentCaption(pct(1), 1, False),
                              MultivariateChartPresentation37.PcaLoadingTitle)
            End With
        End Sub

        ''' <summary>
        ''' Creates a 3D scatter plot of observation scores on PC1, PC2, and PC3.
        ''' </summary>
        Public Shared Sub ScorePlot3D(model As Multivariate.PCA,
                                      Optional rowLabels() As String = Nothing,
                                      Optional groupLabels() As String = Nothing)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 3 Then Exit Sub

            Dim scores As Double(,) = model.ReducedDataset
            Dim pc1() As Double = MultivariatePlotGeometry37.Column(scores, 0)
            Dim pc2() As Double = MultivariatePlotGeometry37.Column(scores, 1)
            Dim pc3() As Double = MultivariatePlotGeometry37.Column(scores, 2)
            Dim pct() As Double = model.PercentExpl
            Dim n As Integer = model.ObservationCount

            Dim scoreLabels() As String = MultivariateChartPresentation37.ScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = MultivariateChartPresentation37.NormalizeGroups(groupLabels, n)

            Dim XYZ As New XYZscatter
            With XYZ
                .ChartName = MultivariateChartPresentation37.PcaScore3DName
                .dataInputs(pc1, pc2, pc3)
                .axesLabelInputs(MultivariateChartPresentation37.ComponentCaption(pct(0), 0, True),
                                 MultivariateChartPresentation37.ComponentCaption(pct(1), 1, False),
                                 MultivariateChartPresentation37.ComponentCaption(pct(2), 2, True))
                .showPlanePointInputs(True, True, True, 3, 3, 3)
                .ScaleAxis(False)
                .settingsInputs(True, True, True)
                .SetDataLabels(scoreLabels)
                If normalizedGroups IsNot Nothing Then
                    .SetGroups(normalizedGroups)
                End If
                .draw()
            End With
        End Sub

        ''' <summary>
        ''' Creates a 2D scatter plot of observation scores on PC1 vs PC2.
        ''' </summary>
        Public Shared Sub ScorePlot2D(model As Multivariate.PCA,
                                      Optional rowLabels() As String = Nothing,
                                      Optional groupLabels() As String = Nothing)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 2 Then Exit Sub

            Dim scores As Double(,) = model.ReducedDataset
            Dim n As Integer = model.ObservationCount
            Dim pct() As Double = model.PercentExpl
            Dim scoreLabels() As String = MultivariateChartPresentation37.ScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = MultivariateChartPresentation37.NormalizeGroups(groupLabels, n)

            Dim pc1() As Double = MultivariatePlotGeometry37.Column(scores, 0)
            Dim pc2() As Double = MultivariatePlotGeometry37.Column(scores, 1)
            Dim axes = MultivariatePlotGeometry37.Axes(pc1, pc2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = MultivariateChartPresentation37.PcaScore2DName
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                ConfigureScatterAxes(.Axes(XlAxisType.xlCategory), udAxisX)
                ConfigureScatterAxes(.Axes(XlAxisType.xlValue), udAxisY)

                Dim seriesId As Integer = 0
                Dim groupSeriesCount As Integer = AddScoreSeries2D(.SeriesCollection,
                                                                    pc1,
                                                                    pc2,
                                                                    scoreLabels,
                                                                    normalizedGroups,
                                                                    seriesId,
                                                                    modelRowIds:=model.RowIds)

                AddZeroLines(.SeriesCollection, seriesId, udAxisX, udAxisY)
                ConfigureScoreLegend(AppGlobals.app.ActiveWorkbook.ActiveChart, groupSeriesCount)
                SetAxisTitles(AppGlobals.app.ActiveWorkbook.ActiveChart,
                              MultivariateChartPresentation37.ComponentCaption(pct(0), 0, True),
                              MultivariateChartPresentation37.ComponentCaption(pct(1), 1, False),
                              MultivariateChartPresentation37.PcaScoreTitle)
            End With
        End Sub

        ''' <summary>
        ''' Creates a PCA biplot (scores + loading vectors) in the PC1/PC2 plane.
        ''' </summary>
        Public Shared Sub Biplot(model As Multivariate.PCA,
                                 Optional c As Double = 1.0,
                                 Optional rowLabels() As String = Nothing,
                                 Optional groupLabels() As String = Nothing)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 2 Then Exit Sub
            If c < 0.0 Or c > 1.0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("biplot 'scale' is outside of range [0, 1]"))

            Dim titl As String = MultivariateChartPresentation37.BiplotDescription(c)

            Dim scores As Double(,) = model.ReducedDataset
            Dim loadings As Double(,) = model.GetLoadings
            Dim varNames() As String = model.VariableNames
            Dim eigenvalues() As Double = model.Eigenvalues
            Dim pct() As Double = model.PercentExpl
            Dim n As Integer = model.ObservationCount
            Dim p As Integer = model.VariableCount
            Dim scoreLabels() As String = MultivariateChartPresentation37.ScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = MultivariateChartPresentation37.NormalizeGroups(groupLabels, n)

            Dim coordinates = MultivariatePlotGeometry37.Biplot(scores, loadings, eigenvalues, n, p, c)
            Dim pc1() As Double = coordinates.ScoreX
            Dim pc2() As Double = coordinates.ScoreY
            Dim Load1() As Double = coordinates.LoadingX
            Dim Load2() As Double = coordinates.LoadingY
            Dim axes = MultivariatePlotGeometry37.CombinedAxes(pc1, pc2, Load1, Load2)
            Dim udAxisX As ChartAxisScaleResult = axes.X
            Dim udAxisY As ChartAxisScaleResult = axes.Y

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = MultivariateChartPresentation37.BiplotName(c)
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                ConfigureScatterAxes(.Axes(XlAxisType.xlCategory), udAxisX)
                ConfigureScatterAxes(.Axes(XlAxisType.xlValue), udAxisY)

                Dim series_id As Integer = 0
                Dim groupSeriesCount As Integer = AddScoreSeries2D(.SeriesCollection,
                                                                    pc1,
                                                                    pc2,
                                                                    scoreLabels,
                                                                    normalizedGroups,
                                                                    series_id,
                                                                    MultivariateChartPresentation37.BiplotTitlePrefix & titl,
                                                                    model.RowIds)

                For id = 0 To p - 1
                    .SeriesCollection.NewSeries
                    series_id += 1
                    With .SeriesCollection(series_id)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = MultivariatePlotGeometry37.LoadingVector(Load1(id))
                        .Values = MultivariatePlotGeometry37.LoadingVector(Load2(id))
                        .Name = MultivariateChartPresentation37.LoadingSeriesName(id)
                        .Format.Line.Weight = 1
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(0, 0, 150)
                        .Format.Line.EndArrowheadStyle = 2 'msoArrowheadTriangle

                        .points(2).HasDataLabel = True
                        .points(2).DataLabel.text = CStr(varNames(id))
                        .points(2).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(2).DataLabel.Font.Size = 11
                        .points(2).DataLabel.Font.Color = RGB(0, 0, 150)
                    End With
                Next id

                AddZeroLines(.SeriesCollection, series_id, udAxisX, udAxisY)
                ConfigureScoreLegend(AppGlobals.app.ActiveWorkbook.ActiveChart, groupSeriesCount)
                SetAxisTitles(AppGlobals.app.ActiveWorkbook.ActiveChart,
                              MultivariateChartPresentation37.ComponentCaption(pct(0), 0, False),
                              MultivariateChartPresentation37.ComponentCaption(pct(1), 1, False),
                              MultivariateChartPresentation37.BiplotTitlePrefix & titl)
            End With
        End Sub

        ''' <summary>
        ''' Creates a scree plot of eigenvalues versus component index.
        ''' </summary>
        Public Shared Sub ScreePlot(model As Multivariate.PCA)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            Dim pct() As Double = model.PercentExpl
            Dim p As Integer = model.VariableCount

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = MultivariateChartPresentation37.ScreeName
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                .SeriesCollection.NewSeries
                With .SeriesCollection(1)
                    .XValues = model.XaxisComponents
                    .Values = pct
                    .Name = MultivariateChartPresentation37.PercentSeriesName
                    .Format.Line.Weight = 1.5
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .Border.Color = RGB(100, 100, 100)
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .MarkerBackgroundColor = RGB(100, 100, 100)

                    For i = 0 To p - 1
                        .points(i + 1).HasDataLabel = True
                        .points(i + 1).DataLabel.text = MultivariateChartPresentation37.PercentLabel(pct(i))
                        .points(i + 1).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(i + 1).DataLabel.Font.Size = 12
                    Next
                End With

                DeleteLegendIfPresent(AppGlobals.app.ActiveWorkbook.ActiveChart)
                SetAxisTitles(AppGlobals.app.ActiveWorkbook.ActiveChart,
                              MultivariateChartPresentation37.ComponentAxisTitle,
                              MultivariateChartPresentation37.VarianceAxisTitle,
                              MultivariateChartPresentation37.ScreeName)
            End With
        End Sub

        Private Shared Function AddScoreSeries2D(seriesCollection As Object,
                                                 xValues() As Double,
                                                 yValues() As Double,
                                                 pointLabels() As String,
                                                 groupLabels() As String,
                                                 ByRef seriesId As Integer,
                                                 Optional ungroupedSeriesName As String = "Score plot",
                                                 Optional modelRowIds() As Integer = Nothing) As Integer
            Dim ungrouped = If(groupLabels Is Nothing, MultivariateChartPresentation37.ScoreSeries(xValues, yValues, pointLabels, Nothing, ungroupedSeriesName, modelRowIds)(0), Nothing)
            If groupLabels Is Nothing Then
                seriesCollection.NewSeries
                seriesId += 1
                With seriesCollection(seriesId)
                    .XValues = ungrouped.XValues
                    .Values = ungrouped.YValues
                    .Name = ungrouped.Name
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .Format.Fill.Visible = False
                    AddPointLabels(.points, ungrouped.Labels)
                End With
                Return 0
            End If

            Dim prepared = MultivariateChartPresentation37.ScoreSeries(xValues, yValues, pointLabels, groupLabels, ungroupedSeriesName, modelRowIds)

            'First four colors/styles follow the classic PCA grouping convention shown in
            'the UI example (blue circle, orange triangle, gray diamond, yellow square).
            'Additional groups continue with clearly separated colors.
            Dim colors() As Integer = {
                RGB(79, 129, 189), RGB(247, 150, 70), RGB(166, 166, 166), RGB(255, 192, 0), RGB(148, 103, 189),
                RGB(140, 86, 75), RGB(227, 119, 194), RGB(44, 160, 44), RGB(23, 190, 207), RGB(214, 39, 40)
            }
            Dim markers() As XlMarkerStyle = {
                XlMarkerStyle.xlMarkerStyleCircle,
                XlMarkerStyle.xlMarkerStyleTriangle,
                XlMarkerStyle.xlMarkerStyleDiamond,
                XlMarkerStyle.xlMarkerStyleSquare,
                XlMarkerStyle.xlMarkerStyleX,
                XlMarkerStyle.xlMarkerStyleStar,
                XlMarkerStyle.xlMarkerStylePlus,
                XlMarkerStyle.xlMarkerStyleDash
            }

            For g As Integer = 0 To prepared.Count - 1
                Dim pointSeries = prepared(g)

                seriesCollection.NewSeries
                seriesId += 1
                With seriesCollection(seriesId)
                    .ChartType = XlChartType.xlXYScatter
                    .XValues = pointSeries.XValues
                    .Values = pointSeries.YValues
                    .Name = pointSeries.Name
                    .MarkerStyle = markers(g Mod markers.Length)
                    .MarkerSize = 6
                    .MarkerForegroundColor = colors(g Mod colors.Length)
                    .MarkerBackgroundColor = colors(g Mod colors.Length)
                    .Format.Fill.Visible = True
                    AddPointLabels(.points, pointSeries.Labels)
                End With
            Next

            Return prepared.Count
        End Function

        Private Shared Sub AddPointLabels(points As Object, labels() As String)
            If labels Is Nothing Then Exit Sub

            For i As Integer = 1 To labels.Length
                If labels(i - 1) <> String.Empty Then
                    points(i).HasDataLabel = True
                    points(i).DataLabel.text = labels(i - 1)
                    points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                    points(i).DataLabel.Font.Size = 8
                End If
            Next
        End Sub

        Private Shared Sub ConfigureScoreLegend(chart As Chart, groupSeriesCount As Integer)
            If Not MultivariateChartPresentation37.ShowScoreLegend(groupSeriesCount) Then
                DeleteLegendIfPresent(chart)
                Exit Sub
            End If

            chart.HasLegend = True
            Try
                chart.Legend.Position = XlLegendPosition.xlLegendPositionRight
                Do While chart.Legend.LegendEntries.Count > groupSeriesCount
                    chart.Legend.LegendEntries(chart.Legend.LegendEntries.Count).Delete()
                Loop
            Catch
            End Try
        End Sub

        Private Shared Sub ConfigureScatterAxes(axis As Object, scale As ChartAxisScaleResult)
            With axis
                .MinimumScale = scale.Min
                .MaximumScale = scale.Max
                .MajorUnit = scale.Scale
                .CrossesAt = -1.0E+100
                .MajorTickMark = XlTickMark.xlTickMarkOutside
                .MajorGridlines.Delete
            End With
        End Sub

        Private Shared Sub AddZeroLines(seriesCollection As Object,
                                        ByRef seriesId As Integer,
                                        axisX As ChartAxisScaleResult,
                                        axisY As ChartAxisScaleResult)
            seriesCollection.NewSeries
            seriesId += 1
            With seriesCollection(seriesId)
                .XValues = ChartReferenceSeries.Horizontal(axisX.Min, axisX.Max, 0).XValues
                .Values = MultivariatePlotGeometry37.ZeroPair()
                .Name = MultivariateChartPresentation37.YZeroName
                .MarkerStyle = -4142
                .Border.Color = RGB(0, 0, 0)
                With .Format.Line
                    .Visible = True
                    .Weight = 1
                End With
            End With

            seriesCollection.NewSeries
            seriesId += 1
            With seriesCollection(seriesId)
                .XValues = MultivariatePlotGeometry37.ZeroPair()
                .Values = ChartReferenceSeries.Vertical(0, axisY.Min, axisY.Max).YValues
                .Name = MultivariateChartPresentation37.XZeroName
                .MarkerStyle = -4142
                .Border.Color = RGB(0, 0, 0)
                With .Format.Line
                    .Visible = True
                    .Weight = 1
                End With
            End With
        End Sub

        Private Shared Sub DeleteLegendIfPresent(chart As Chart)
            Try
                chart.Legend.Delete()
            Catch
            End Try
        End Sub

        Private Shared Sub SetAxisTitles(chart As Chart,
                                         xTitle As String,
                                         yTitle As String,
                                         chartTitle As String)
            With chart
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = yTitle
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = xTitle
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = chartTitle
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With
        End Sub

    End Class

End Namespace