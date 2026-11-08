Option Explicit On

Imports System.Linq
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
            Dim pc1() As Double = Matrix.GetColumnFrom2Darray(loadings, 0)
            Dim pc2() As Double = Matrix.GetColumnFrom2Darray(loadings, 1)
            Dim pc3() As Double = Matrix.GetColumnFrom2Darray(loadings, 2)
            Dim pct() As Double = model.PercentExpl

            Dim XYZ As New XYZscatter
            With XYZ
                .ChartName = "Loadings Plot3D"
                .dataInputs(pc1, pc2, pc3)
                .axesLabelInputs($"1st Component Scores [{ Format$(pct(0), "#0.0#") }%]",
                                 $"2nd Component Scores [{ Format$(pct(1), "#0.0#") }%]",
                                 $"3rd Component Scores [{ Format$(pct(2), "#0.0#") }%]")
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

            Dim pc1() As Double = Matrix.GetColumnFrom2Darray(loadings, 0)
            Dim pc2() As Double = Matrix.GetColumnFrom2Darray(loadings, 1)
            Dim scl1 As Double = Math.Max(Math.Abs(pc1.Min()), Math.Abs(pc1.Max()))
            Dim scl2 As Double = Math.Max(Math.Abs(pc2.Min()), Math.Abs(pc2.Max()))
            Dim udAxisX As CHARTscale = ChartScaling(-scl1, scl1)
            Dim udAxisY As CHARTscale = ChartScaling(-scl2, scl2)

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = "Loadings Plot2D"
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
                        .XValues = {0, pc1(id)}
                        .Values = {0, pc2(id)}
                        .Name = "Loading_" & CStr(id)
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
                              $"1st Component Scores [{ Format$(pct(0), "#0.0#") }%]",
                              $"2nd Component Scores [{ Format$(pct(1), "#0.0#") }%]",
                              "Component Loadings Plot")
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
            Dim pc1() As Double = Matrix.GetColumnFrom2Darray(scores, 0)
            Dim pc2() As Double = Matrix.GetColumnFrom2Darray(scores, 1)
            Dim pc3() As Double = Matrix.GetColumnFrom2Darray(scores, 2)
            Dim pct() As Double = model.PercentExpl
            Dim n As Integer = model.ObservationCount

            Dim scoreLabels() As String = ResolveScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = ResolveGroupLabels(groupLabels, n)

            Dim XYZ As New XYZscatter
            With XYZ
                .ChartName = "Score Plot3D"
                .dataInputs(pc1, pc2, pc3)
                .axesLabelInputs($"1St Component Scores [{ Format$(pct(0), "#0.0#") }%]",
                                 $"2nd Component Scores [{ Format$(pct(1), "#0.0#") }%]",
                                 $"3Rd Component Scores [{ Format$(pct(2), "#0.0#") }%]")
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
            Dim scoreLabels() As String = ResolveScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = ResolveGroupLabels(groupLabels, n)

            Dim pc1() As Double = Matrix.GetColumnFrom2Darray(scores, 0)
            Dim pc2() As Double = Matrix.GetColumnFrom2Darray(scores, 1)
            Dim udAxisX As CHARTscale = ChartScaling(pc1.Min(), pc1.Max())
            Dim udAxisY As CHARTscale = ChartScaling(pc2.Min(), pc2.Max())

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = "Score Plot2D"
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
                                                                    seriesId)

                AddZeroLines(.SeriesCollection, seriesId, udAxisX, udAxisY)
                ConfigureScoreLegend(AppGlobals.app.ActiveWorkbook.ActiveChart, groupSeriesCount)
                SetAxisTitles(AppGlobals.app.ActiveWorkbook.ActiveChart,
                              $"1St Component Scores [{ Format$(pct(0), "#0.0#") }%]",
                              $"2nd Component Scores [{ Format$(pct(1), "#0.0#") }%]",
                              "Scores Plot")
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

            Dim titl As String = String.Empty
            If c = 0.0 Then
                titl = "GH, or column-metric preserving"
            ElseIf c = 1.0 Then
                titl = "JK, or row-metric preserving"
            ElseIf c = 0.5 Then
                titl = "SQ, or symmetric"
            End If

            Dim scores As Double(,) = model.ReducedDataset
            Dim loadings As Double(,) = model.GetLoadings
            Dim varNames() As String = model.VariableNames
            Dim eigenvalues() As Double = model.Eigenvalues
            Dim pct() As Double = model.PercentExpl
            Dim n As Integer = model.ObservationCount
            Dim p As Integer = model.VariableCount
            Dim scoreLabels() As String = ResolveScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = ResolveGroupLabels(groupLabels, n)

            Dim pc1() As Double = Matrix.GetColumnFrom2Darray(scores, 0)
            Dim pc2() As Double = Matrix.GetColumnFrom2Darray(scores, 1)
            Dim Load1() As Double = Matrix.GetColumnFrom2Darray(loadings, 0)
            Dim Load2() As Double = Matrix.GetColumnFrom2Darray(loadings, 1)

            Dim lam(1) As Double
            For i = 0 To 1
                lam(i) = Math.Sqrt(eigenvalues(i)) * Math.Sqrt(n)
                lam(i) = lam(i) ^ (1.0 - c)
            Next

            For i = 0 To n - 1
                pc1(i) /= lam(0)
                pc2(i) /= lam(1)
            Next

            For i = 0 To p - 1
                Load1(i) *= lam(0)
                Load2(i) *= lam(1)
            Next

            Dim udAxisX As CHARTscale = ChartScaling(Math.Min(pc1.Min(), Load1.Min()), Math.Max(pc1.Max(), Load1.Max()))
            Dim udAxisY As CHARTscale = ChartScaling(Math.Min(pc2.Min(), Load2.Min()), Math.Max(pc2.Max(), Load2.Max()))

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = "Biplot scale=" & CStr(c)
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
                                                                    "Biplot: " & titl)

                For id = 0 To p - 1
                    .SeriesCollection.NewSeries
                    series_id += 1
                    With .SeriesCollection(series_id)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = {0, Load1(id)}
                        .Values = {0, Load2(id)}
                        .Name = "Loading_" & CStr(id)
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
                              $"1st Component Scores [{ Format$(pct(0), "#0.0#")}%]",
                              $"2nd Component Scores [{ Format$(pct(1), "#0.0#")}%]",
                              "Biplot: " & titl)
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
                .Name = "Scree Plot"
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                .SeriesCollection.NewSeries
                With .SeriesCollection(1)
                    .XValues = model.XaxisComponents
                    .Values = pct
                    .Name = "Percent Explained"
                    .Format.Line.Weight = 1.5
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .Border.Color = RGB(100, 100, 100)
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .MarkerBackgroundColor = RGB(100, 100, 100)

                    For i = 0 To p - 1
                        .points(i + 1).HasDataLabel = True
                        .points(i + 1).DataLabel.text = Format$(pct(i), "#0.0#")
                        .points(i + 1).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(i + 1).DataLabel.Font.Size = 12
                    Next
                End With

                DeleteLegendIfPresent(AppGlobals.app.ActiveWorkbook.ActiveChart)
                SetAxisTitles(AppGlobals.app.ActiveWorkbook.ActiveChart,
                              "Principal Component",
                              "Variance explained [%]",
                              "Scree Plot")
            End With
        End Sub

        Private Shared Function ResolveScoreLabels(model As Multivariate.PCA, rowLabels() As String) As String()
            Dim n As Integer = model.ObservationCount
            Dim rowIds() As Integer = model.RowIds
            Dim out(n - 1) As String

            If rowLabels IsNot Nothing AndAlso rowLabels.Length <> n Then
                Throw New ArgumentException("PCA row-label count must match the number of observations.", NameOf(rowLabels))
            End If

            For i As Integer = 0 To n - 1
                Dim text As String = String.Empty
                If rowLabels IsNot Nothing AndAlso rowLabels(i) IsNot Nothing Then text = rowLabels(i).Trim()
                If text = String.Empty Then text = CStr(rowIds(i))
                out(i) = text
            Next

            Return out
        End Function

        Private Shared Function ResolveGroupLabels(groupLabels() As String, expectedCount As Integer) As String()
            If groupLabels Is Nothing Then Return Nothing
            If groupLabels.Length <> expectedCount Then
                Throw New ArgumentException("PCA grouping-label count must match the number of observations.", NameOf(groupLabels))
            End If

            Dim out(expectedCount - 1) As String
            For i As Integer = 0 To expectedCount - 1
                Dim text As String = If(groupLabels(i), String.Empty).Trim()
                If text = String.Empty Then text = "(missing)"
                out(i) = text
            Next
            Return out
        End Function

        Private Shared Function AddScoreSeries2D(seriesCollection As Object,
                                                 xValues() As Double,
                                                 yValues() As Double,
                                                 pointLabels() As String,
                                                 groupLabels() As String,
                                                 ByRef seriesId As Integer,
                                                 Optional ungroupedSeriesName As String = "Score plot") As Integer
            If groupLabels Is Nothing Then
                seriesCollection.NewSeries
                seriesId += 1
                With seriesCollection(seriesId)
                    .XValues = xValues
                    .Values = yValues
                    .Name = ungroupedSeriesName
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .Format.Fill.Visible = False
                    AddPointLabels(.points, pointLabels)
                End With
                Return 0
            End If

            Dim groupIds As New List(Of String)
            Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
            For Each groupId As String In groupLabels
                If seen.Add(groupId) Then groupIds.Add(groupId)
            Next

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

            For g As Integer = 0 To groupIds.Count - 1
                Dim xs As New List(Of Double)
                Dim ys As New List(Of Double)
                Dim labels As New List(Of String)

                For i As Integer = 0 To groupLabels.Length - 1
                    If groupLabels(i) = groupIds(g) Then
                        xs.Add(xValues(i))
                        ys.Add(yValues(i))
                        labels.Add(pointLabels(i))
                    End If
                Next

                seriesCollection.NewSeries
                seriesId += 1
                With seriesCollection(seriesId)
                    .ChartType = XlChartType.xlXYScatter
                    .XValues = xs.ToArray()
                    .Values = ys.ToArray()
                    .Name = groupIds(g)
                    .MarkerStyle = markers(g Mod markers.Length)
                    .MarkerSize = 6
                    .MarkerForegroundColor = colors(g Mod colors.Length)
                    .MarkerBackgroundColor = colors(g Mod colors.Length)
                    .Format.Fill.Visible = True
                    AddPointLabels(.points, labels.ToArray())
                End With
            Next

            Return groupIds.Count
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
            If groupSeriesCount <= 1 Then
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

        Private Shared Sub ConfigureScatterAxes(axis As Object, scale As CHARTscale)
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
                                        axisX As CHARTscale,
                                        axisY As CHARTscale)
            seriesCollection.NewSeries
            seriesId += 1
            With seriesCollection(seriesId)
                .XValues = {axisX.Min, axisX.Max}
                .Values = {0, 0}
                .Name = "Y Zero Line"
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
                .XValues = {0, 0}
                .Values = {axisY.Min, axisY.Max}
                .Name = "X Zero Line"
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