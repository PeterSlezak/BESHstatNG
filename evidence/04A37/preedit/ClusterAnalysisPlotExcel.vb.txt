Option Explicit On

Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.Multivariate
Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Creates Excel dendrogram charts from a precomputed <see cref="DendrogramLayout"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The chart writer uses the Chapter-8 approach of drawing a dendrogram as an X/Y scatter chart with straight
    ''' lines and, optionally, a separate label series or x-axis title text for leaf labels.
    ''' </para>
    ''' <para>
    ''' All plotted series are supplied directly as in-memory VB.NET arrays. No worksheet helper ranges are needed
    ''' unless a caller deliberately exports the coordinates separately for debugging.
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class ClusterAnalysisPlotExcel

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Creates an Excel dendrogram chart from a fitted hierarchical clustering result.
        ''' </summary>
        Public Shared Function CreateDendrogramChart(result As Multivariate.HierarchicalClusterResult,
                                                     Optional figure As Chart = Nothing,
                                                     Optional ws As Worksheet = Nothing,
                                                     Optional topLeftCellAddress As String = "A1",
                                                     Optional chartWidth As Double = 480.0,
                                                     Optional chartHeight As Double = 320.0,
                                                     Optional heightMode As Multivariate.DendrogramHeightMode = Multivariate.DendrogramHeightMode.MergeDistance,
                                                     Optional orientation As Multivariate.DendrogramOrientation = Multivariate.DendrogramOrientation.Top,
                                                     Optional labelMode As Multivariate.DendrogramLabelMode = Multivariate.DendrogramLabelMode.DataLabels,
                                                     Optional chartTitle As String = "Dendrogram",
                                                     Optional distanceAxisTitle As String = Nothing,
                                                     Optional cutMode As Multivariate.HierarchicalMembershipDisplayMode = Multivariate.HierarchicalMembershipDisplayMode.ByClusterCount,
                                                     Optional membershipClusterCount As Integer = 3,
                                                     Optional membershipCutHeight As Double = 0.0) As Chart
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
            Dim layout As Multivariate.DendrogramLayout = result.CreateDendrogramLayout(heightMode, orientation, cutMode, membershipClusterCount, membershipCutHeight)
            Return CreateExcelChart(layout, figure, ws, topLeftCellAddress, chartWidth, chartHeight, labelMode, chartTitle, distanceAxisTitle)
        End Function

        ''' <summary>
        ''' Draws a prepared dendrogram layout into an Excel chart.
        ''' </summary>
        Public Shared Function Draw(layout As Multivariate.DendrogramLayout,
                                    Optional figure As Chart = Nothing,
                                    Optional ws As Worksheet = Nothing,
                                    Optional topLeftCellAddress As String = "A1",
                                    Optional chartWidth As Double = 480.0,
                                    Optional chartHeight As Double = 320.0,
                                    Optional labelMode As Multivariate.DendrogramLabelMode = Multivariate.DendrogramLabelMode.DataLabels,
                                    Optional chartTitle As String = "Dendrogram",
                                    Optional distanceAxisTitle As String = Nothing) As Chart
            Return CreateExcelChart(layout, figure, ws, topLeftCellAddress, chartWidth, chartHeight, labelMode, chartTitle, distanceAxisTitle)
        End Function


        ''' <summary>
        ''' Creates or redraws an Excel dendrogram chart from a prepared layout object.
        ''' </summary>
        ''' <param name="layout">Prepared dendrogram layout returned by HierarchicalClusterResult.CreateDendrogramLayout(DendrogramHeightMode, DendrogramOrientation).</param>
        ''' <param name="figure">
        ''' Optional existing chart object to reuse. When supplied, the dendrogram is rendered directly into this chart.
        ''' </param>
        ''' <param name="ws">
        ''' Optional worksheet used when a new embedded chart must be created. When both <paramref name="figure"/> and
        ''' <paramref name="ws"/> are omitted, a new chart sheet is created in the active workbook.
        ''' </param>
        ''' <param name="topLeftCellAddress">Worksheet address of the upper-left chart anchor when a new embedded chart is created.</param>
        ''' <param name="chartWidth">Chart width, in Excel points, for a newly created embedded chart.</param>
        ''' <param name="chartHeight">Chart height, in Excel points, for a newly created embedded chart.</param>
        ''' <param name="labelMode">Controls how leaf labels are rendered.</param>
        ''' <param name="chartTitle">Optional chart title text.</param>
        ''' <param name="distanceAxisTitle">Optional title for the distance axis.</param>
        ''' <returns>
        ''' The Excel <see cref="Chart"/> object that was created or updated. The chart series are populated directly
        ''' from the coordinate arrays stored in <paramref name="layout"/>.
        ''' </returns>
        Public Shared Function CreateExcelChart(layout As DendrogramLayout,
                                                Optional figure As Chart = Nothing,
                                                Optional ws As Worksheet = Nothing,
                                                Optional topLeftCellAddress As String = "A1",
                                                Optional chartWidth As Double = 480.0,
                                                Optional chartHeight As Double = 320.0,
                                                Optional labelMode As DendrogramLabelMode = DendrogramLabelMode.DataLabels,
                                                Optional chartTitle As String = "Dendrogram",
                                                Optional distanceAxisTitle As String = Nothing) As Chart

            If layout Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(layout)))
            If layout.PolylineX Is Nothing OrElse layout.PolylineY Is Nothing OrElse layout.PolylineX.Length = 0 Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("The supplied dendrogram layout does not contain any polyline coordinates."))
            End If

            If figure Is Nothing Then
                If ws Is Nothing Then
                    If AppGlobals.app Is Nothing OrElse AppGlobals.app.ActiveWorkbook Is Nothing Then
                        CoreServices.Errors.LogAndThrow(New InvalidOperationException("No active workbook is available for dendrogram chart creation."))
                    End If
                    AppGlobals.app.ActiveWorkbook.Charts.Add()
                    figure = CType(AppGlobals.app.ActiveChart, Chart)
                Else
                    Dim anchor As Range = ws.Range(topLeftCellAddress)
                    figure = ws.Shapes.AddChart(Left:=CDbl(anchor.Left), Top:=CDbl(anchor.Top), Width:=chartWidth, Height:=chartHeight).Chart
                End If
            End If

            With figure
                .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                .HasLegend = False
                .HasTitle = False
                If Not String.IsNullOrWhiteSpace(chartTitle) Then
                    .HasTitle = True
                    .ChartTitle.Text = chartTitle
                End If

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                .HasAxis(XlAxisType.xlCategory, XlAxisGroup.xlPrimary) = True
                .HasAxis(XlAxisType.xlValue, XlAxisGroup.xlPrimary) = True

                Dim nextSeriesIndex As Integer = 1

                .SeriesCollection.NewSeries()
                With .SeriesCollection(nextSeriesIndex)
                    .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                    .Name = "Dendrogram"
                    .XValues = layout.PolylineX
                    .Values = layout.PolylineY
                    .Border.LineStyle = XlLineStyle.xlContinuous
                    .Border.Weight = XlBorderWeight.xlThin
                    .Border.Color = RGB(0, 0, 0)
                    .Format.Line.Visible = True
                    .Format.Line.ForeColor.RGB = RGB(0, 0, 0)
                    .Format.Line.Weight = 1.25
                End With
                nextSeriesIndex += 1

                If layout.ClusterPolylineX IsNot Nothing AndAlso layout.ClusterPolylineY IsNot Nothing Then
                    Dim nColored As Integer = Math.Min(layout.ClusterPolylineX.Count, layout.ClusterPolylineY.Count)
                    For i As Integer = 0 To nColored - 1
                        If layout.ClusterPolylineX(i) Is Nothing OrElse layout.ClusterPolylineY(i) Is Nothing Then Continue For
                        If layout.ClusterPolylineX(i).Length = 0 OrElse layout.ClusterPolylineY(i).Length = 0 Then Continue For

                        Dim clr As Integer = ClusterAnalysisHelpers.GetClusterSeriesColor(i)
                        .SeriesCollection.NewSeries()
                        With .SeriesCollection(nextSeriesIndex)
                            .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                            .Name = $"Cluster {i + 1}"
                            .XValues = layout.ClusterPolylineX(i)
                            .Values = layout.ClusterPolylineY(i)
                            .Border.LineStyle = XlLineStyle.xlContinuous
                            .Border.Weight = XlBorderWeight.xlThin
                            .Border.Color = clr
                            .Format.Line.Visible = True
                            .Format.Line.ForeColor.RGB = clr
                            .Format.Line.Weight = 2.0
                        End With
                        nextSeriesIndex += 1
                    Next
                End If

                If layout.CutLineX IsNot Nothing AndAlso layout.CutLineY IsNot Nothing AndAlso layout.CutLineX.Length >= 2 AndAlso layout.CutLineY.Length >= 2 Then
                    .SeriesCollection.NewSeries()
                    With .SeriesCollection(nextSeriesIndex)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .Name = "Cut"
                        .XValues = layout.CutLineX
                        .Values = layout.CutLineY
                        .Border.LineStyle = XlLineStyle.xlContinuous
                        .Border.Weight = XlBorderWeight.xlThin
                        .Border.Color = RGB(90, 90, 90)
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(90, 90, 90)
                        .Format.Line.Weight = 1.25
                        .Format.Line.DashStyle = 4
                    End With
                    nextSeriesIndex += 1
                End If

                If labelMode = DendrogramLabelMode.DataLabels AndAlso layout.LeafX IsNot Nothing AndAlso layout.LeafLabels IsNot Nothing AndAlso layout.LeafLabels.Length > 0 Then
                    .SeriesCollection.NewSeries()
                    With .SeriesCollection(nextSeriesIndex)
                        .ChartType = XlChartType.xlXYScatter
                        .Name = "LeafLabels"
                        .XValues = layout.LeafX
                        .Values = layout.LeafY
                        .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                        For i As Integer = 1 To layout.LeafLabels.Length
                            .Points(i).HasDataLabel = True
                            .Points(i).DataLabel.Text = layout.LeafLabels(i - 1)
                            .Points(i).DataLabel.Font.Size = 9
                            Select Case layout.Orientation
                                Case DendrogramOrientation.Top
                                    .Points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionBelow
                                Case DendrogramOrientation.Bottom
                                    .Points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                                Case DendrogramOrientation.Left
                                    .Points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionRight
                                Case Else
                                    .Points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionLeft
                            End Select
                        Next
                    End With
                    nextSeriesIndex += 1
                End If

                Dim xMin As Double = layout.PolylineX.Min()
                Dim xMax As Double = layout.PolylineX.Max()
                Dim yMin As Double = layout.PolylineY.Min()
                Dim yMax As Double = layout.PolylineY.Max()

                If layout.CutLineX IsNot Nothing AndAlso layout.CutLineX.Length > 0 Then
                    xMin = Math.Min(xMin, layout.CutLineX.Min())
                    xMax = Math.Max(xMax, layout.CutLineX.Max())
                End If
                If layout.CutLineY IsNot Nothing AndAlso layout.CutLineY.Length > 0 Then
                    yMin = Math.Min(yMin, layout.CutLineY.Min())
                    yMax = Math.Max(yMax, layout.CutLineY.Max())
                End If

                Dim xReference As Double = If(layout.Orientation = DendrogramOrientation.Left OrElse layout.Orientation = DendrogramOrientation.Right,
                                              If(layout.MaximumHeight <= 0, 1.0, layout.MaximumHeight),
                                              layout.LeafCount)
                Dim yReference As Double = If(layout.Orientation = DendrogramOrientation.Left OrElse layout.Orientation = DendrogramOrientation.Right,
                                              layout.LeafCount,
                                              If(layout.MaximumHeight <= 0, 1.0, layout.MaximumHeight))
                Dim xPad As Double = ClusterAnalysisHelpers.AxisPadding(xMin, xMax, xReference)
                Dim yPad As Double = ClusterAnalysisHelpers.AxisPadding(yMin, yMax, yReference)

                With .Axes(XlAxisType.xlCategory)
                    .MinimumScaleIsAuto = False
                    .MaximumScaleIsAuto = False
                    .MinimumScale = xMin - xPad
                    .MaximumScale = xMax + xPad
                    .HasMajorGridlines = False
                    .HasMinorGridlines = False
                    Try
                        .MajorGridlines.Delete()
                    Catch
                    End Try
                End With

                With .Axes(XlAxisType.xlValue)
                    .MinimumScaleIsAuto = False
                    .MaximumScaleIsAuto = False
                    .MinimumScale = yMin - yPad
                    .MaximumScale = yMax + yPad
                    .HasMajorGridlines = False
                    .HasMinorGridlines = False
                    Try
                        .MajorGridlines.Delete()
                    Catch
                    End Try
                End With

                If layout.Orientation = DendrogramOrientation.Left OrElse layout.Orientation = DendrogramOrientation.Right Then
                    With .Axes(XlAxisType.xlCategory)
                        .MajorTickMark = XlTickMark.xlTickMarkOutside
                        .MinorTickMark = XlTickMark.xlTickMarkNone
                        .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNextToAxis
                        .Border.LineStyle = XlLineStyle.xlContinuous
                    End With
                    With .Axes(XlAxisType.xlValue)
                        .MajorTickMark = XlTickMark.xlTickMarkNone
                        .MinorTickMark = XlTickMark.xlTickMarkNone
                        .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                        .Border.LineStyle = XlLineStyle.xlLineStyleNone
                    End With
                Else
                    With .Axes(XlAxisType.xlCategory)
                        .MajorTickMark = XlTickMark.xlTickMarkNone
                        .MinorTickMark = XlTickMark.xlTickMarkNone
                        .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                        .Border.LineStyle = XlLineStyle.xlLineStyleNone
                    End With
                    With .Axes(XlAxisType.xlValue)
                        .MajorTickMark = XlTickMark.xlTickMarkOutside
                        .MinorTickMark = XlTickMark.xlTickMarkNone
                        .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNextToAxis
                        .Border.LineStyle = XlLineStyle.xlContinuous
                    End With
                End If

                ApplyDendrogramAxisTitles(figure, layout, labelMode, distanceAxisTitle)
            End With

            Return figure
        End Function
        Private Shared Sub ApplyDendrogramAxisTitles(figure As Chart,
                                             layout As DendrogramLayout,
                                             labelMode As DendrogramLabelMode,
                                             distanceAxisTitle As String)

            If figure Is Nothing OrElse layout Is Nothing Then Return

            Dim resolvedDistanceTitle As String = distanceAxisTitle
            If String.IsNullOrWhiteSpace(resolvedDistanceTitle) Then
                resolvedDistanceTitle = If(layout.HeightMode = DendrogramHeightMode.StepLevels,
                                           "Distance not in proportion",
                                           "Distance")
            End If

            Select Case layout.Orientation
                Case DendrogramOrientation.Top, DendrogramOrientation.Bottom
                    figure.Axes(XlAxisType.xlValue).HasTitle = True
                    figure.Axes(XlAxisType.xlValue).AxisTitle.Text = resolvedDistanceTitle
                    figure.Axes(XlAxisType.xlCategory).HasTitle = (labelMode = DendrogramLabelMode.AxisTitle)
                    If labelMode = DendrogramLabelMode.AxisTitle Then
                        figure.Axes(XlAxisType.xlCategory).AxisTitle.Text = layout.GetSuggestedAxisTitle()
                    End If
                Case Else
                    figure.Axes(XlAxisType.xlCategory).HasTitle = True
                    figure.Axes(XlAxisType.xlCategory).AxisTitle.Text = resolvedDistanceTitle
                    figure.Axes(XlAxisType.xlValue).HasTitle = (labelMode = DendrogramLabelMode.AxisTitle)
                    If labelMode = DendrogramLabelMode.AxisTitle Then
                        figure.Axes(XlAxisType.xlValue).AxisTitle.Text = layout.GetSuggestedAxisTitle()
                    End If
            End Select
        End Sub

        Public Shared Sub WriteObjectTable(ws As Worksheet,
                                    topLeftCellAddress As String,
                                    values As Object(,))
            If ws Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(ws)))
            If values Is Nothing Then Return
            Dim rows As Integer = values.GetUpperBound(0) + 1
            Dim cols As Integer = values.GetUpperBound(1) + 1
            Dim target As Range = ws.Range(topLeftCellAddress).Resize(rows, cols)
            target.Value = values
        End Sub
    End Class

End Namespace
