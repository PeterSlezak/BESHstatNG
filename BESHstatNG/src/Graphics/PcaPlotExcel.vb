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
        Public Shared Sub ScorePlot3D(model As Multivariate.PCA)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 3 Then Exit Sub

            Dim scores As Double(,) = model.ReducedDataset
            Dim pc1() As Double = Matrix.GetColumnFrom2Darray(scores, 0)
            Dim pc2() As Double = Matrix.GetColumnFrom2Darray(scores, 1)
            Dim pc3() As Double = Matrix.GetColumnFrom2Darray(scores, 2)
            Dim pct() As Double = model.PercentExpl
            Dim rowIds() As Integer = model.RowIds
            Dim n As Integer = model.ObservationCount

            Dim rownums_str(n - 1) As String
            For i = 0 To n - 1
                rownums_str(i) = CStr(rowIds(i))
            Next

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
                .SetDataLabels(rownums_str)
                .draw()
            End With
        End Sub

        ''' <summary>
        ''' Creates a 2D scatter plot of observation scores on PC1 vs PC2.
        ''' </summary>
        Public Shared Sub ScorePlot2D(model As Multivariate.PCA)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 2 Then Exit Sub

            Dim scores As Double(,) = model.ReducedDataset
            Dim rowIds() As Integer = model.RowIds
            Dim n As Integer = model.ObservationCount
            Dim pct() As Double = model.PercentExpl

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

                .SeriesCollection.NewSeries
                With .SeriesCollection(1)
                    .XValues = pc1
                    .Values = pc2
                    .Name = "Score plot"
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .Format.Fill.Visible = False

                    For i = 1 To n
                        .points(i).HasDataLabel = True
                        .points(i).DataLabel.text = CStr(rowIds(i - 1))
                        .points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(i).DataLabel.Font.Size = 8
                    Next
                End With

                Dim seriesId As Integer = 1
                AddZeroLines(.SeriesCollection, seriesId, udAxisX, udAxisY)
                DeleteLegendIfPresent(AppGlobals.app.ActiveWorkbook.ActiveChart)
                SetAxisTitles(AppGlobals.app.ActiveWorkbook.ActiveChart,
                              $"1St Component Scores [{ Format$(pct(0), "#0.0#") }%]",
                              $"2nd Component Scores [{ Format$(pct(1), "#0.0#") }%]",
                              "Scores Plot")
            End With
        End Sub

        ''' <summary>
        ''' Creates a PCA biplot (scores + loading vectors) in the PC1/PC2 plane.
        ''' </summary>
        Public Shared Sub Biplot(model As Multivariate.PCA, Optional c As Double = 1.0)
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
            Dim rowIds() As Integer = model.RowIds
            Dim varNames() As String = model.VariableNames
            Dim eigenvalues() As Double = model.Eigenvalues
            Dim pct() As Double = model.PercentExpl
            Dim n As Integer = model.ObservationCount
            Dim p As Integer = model.VariableCount

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

                .SeriesCollection.NewSeries
                Dim series_id As Integer = 1
                With .SeriesCollection(series_id)
                    .XValues = pc1
                    .Values = pc2
                    .Name = "Biplot: " & titl
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .Format.Fill.Visible = False

                    For i = 1 To n
                        .points(i).HasDataLabel = True
                        .points(i).DataLabel.text = CStr(rowIds(i - 1))
                        .points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .points(i).DataLabel.Font.Size = 8
                    Next i
                End With

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
                DeleteLegendIfPresent(AppGlobals.app.ActiveWorkbook.ActiveChart)
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