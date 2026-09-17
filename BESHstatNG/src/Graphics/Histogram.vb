Option Explicit On
Imports System.Collections.Generic
Imports Microsoft.Office.Interop.Excel

Namespace graphics


    ''' <summary>
    ''' Implements histogram computation and Excel-based visualization for a
    ''' univariate numeric dataset.
    ''' 
    ''' The class supports:
    ''' <list type="bullet">
    '''   <item><description>Automatic bin computation using <c>HistogramBinsComputation</c></description></item>
    '''   <item><description>Extraction of bin midpoints and frequencies</description></item>
    '''   <item><description>Optional Gaussian overlay curve using the robust quartile-based estimates in <c>GaussOverlayComputation</c></description></item>
    '''   <item><description>Optional horizontal Tukey box plot below the histogram</description></item>
    '''   <item><description>Excel chart generation with aligned primary and secondary axes</description></item>
    ''' </list>
    ''' </summary>
    Public Class Histogram
        ''' <summary>Input data vector.</summary>
        Private data() As Double

        ''' <summary>Worksheet used for chart output.</summary>
        Private pWs As Worksheet

        ''' <summary>Workbook containing the worksheet.</summary>
        Private pWb As Workbook

        ''' <summary>Histogram frequencies for each bin.</summary>
        Private arFreq() As Integer

        ''' <summary>Midpoint values of histogram bins.</summary>
        Private arBinMidVal() As Double

        ''' <summary>X-coordinates for Gaussian overlay curve.</summary>
        Private arXi() As Double

        ''' <summary>Gaussian density values for overlay curve.</summary>
        Private arGauss() As Double = Nothing

        ''' <summary>Tukey box-plot statistics.</summary>
        Private pQ1 As Double
        Private pMedian As Double
        Private pQ3 As Double
        Private pWhiskerLow As Double
        Private pWhiskerHigh As Double
        Private arOutliers() As Double = Nothing
        Private pHasBoxSummary As Boolean = False

        ''' <summary>
        ''' Initializes a histogram object with the supplied numeric data.
        ''' </summary>
        ''' <param name="x">Array of numeric observations.</param>
        Sub New(x() As Double)
            Me.data = x
        End Sub


        ''' <summary>
        ''' Assigns the worksheet used for chart output.
        ''' Also stores the parent workbook for convenience.
        ''' </summary>
        ''' <value>An Excel <see cref="Worksheet"/> object.</value>
        Public WriteOnly Property SetWs() As Worksheet
            Set(ws As Worksheet)
                pWs = ws
                pWb = ws.Parent
            End Set
        End Property

        ''' <summary>
        ''' Computes histogram bins, an optional Gaussian overlay and, when requested,
        ''' the quartiles/Tukey whiskers needed by the horizontal box plot.
        ''' </summary>
        ''' <param name="bOveraly">If <c>True</c>, computes the Gaussian overlay curve.</param>
        ''' <param name="strType">Optional histogram type parameter passed to the binning routine.</param>
        ''' <param name="bBoxPlot">If <c>True</c>, computes Tukey box-plot statistics and outliers.</param>
        ''' <returns>A 2D array of histogram bin midpoints and frequencies.</returns>
        Public Function compute(bOveraly As Boolean,
                                Optional strType As String = "",
                                Optional bBoxPlot As Boolean = False) As Object(,)

            Dim bins = HistogramBinsComputation(Me.data, strType)
            Me.arBinMidVal = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(bins, 0))
            Me.arFreq = Matrix.Array2intArray(Matrix.GetColumnFrom2Darray(bins, 1))

            Me.arXi = Nothing
            Me.arGauss = Nothing

            If bOveraly Then
                Dim overlayData = GaussOverlayComputation(Me.data, Me.arBinMidVal)
                Me.arXi = Matrix.GetColumnFrom2Darray(overlayData, 0)
                Me.arGauss = Matrix.GetColumnFrom2Darray(overlayData, 1)
            End If

            Me.pHasBoxSummary = False
            Me.arOutliers = Nothing
            If bBoxPlot AndAlso Me.data IsNot Nothing AndAlso Me.data.Length > 0 Then
                Dim sorted() As Double = DirectCast(Me.data.Clone(), Double())
                Array.Sort(sorted)

                Dim quartileInput() As Double = DirectCast(sorted.Clone(), Double())
                Dim quartiles As udQuartiles = StatFunc.QuartilesComp(quartileInput)

                Me.pQ1 = quartiles.Q1
                Me.pMedian = quartiles.Median
                Me.pQ3 = quartiles.Q3
                StatFunc.ResolveTukeyWhiskers(sorted,
                                              Me.pQ1,
                                              Me.pQ3,
                                              Me.pWhiskerLow,
                                              Me.pWhiskerHigh)

                Dim outlierList As New List(Of Double)
                For Each value As Double In sorted
                    If value < Me.pWhiskerLow OrElse value > Me.pWhiskerHigh Then
                        outlierList.Add(value)
                    End If
                Next
                Me.arOutliers = outlierList.ToArray()
                Me.pHasBoxSummary = True
            End If

            Return bins
        End Function

        ''' <summary>
        ''' Creates an Excel histogram chart at the specified worksheet location.
        ''' The optional normal curve uses secondary XY axes. The Tukey box plot is
        ''' drawn with chart-contained shapes projected onto the same numeric data scale.
        ''' 
        ''' </summary>
        ''' <param name="ws">Worksheet where the chart will be created.</param>
        ''' <param name="r">One-based row used as the chart anchor.</param>
        ''' <param name="c">One-based column used as the chart anchor.</param>
        ''' <param name="strTitle">Chart title.</param>
        ''' <param name="bBoxPlot">If <c>True</c>, draws the horizontal Tukey box plot.</param>
        Sub addChart(ByRef ws As Worksheet,
                     r As Integer,
                     c As Integer,
                     strTitle As String,
                     Optional bBoxPlot As Boolean = False)

            Dim bOverlay As Boolean = (Me.arGauss IsNot Nothing AndAlso Me.arXi IsNot Nothing)
            Dim bDrawBoxPlot As Boolean = bBoxPlot AndAlso Me.pHasBoxSummary
            Dim bNeedsSecondaryAxes As Boolean = bOverlay

            Dim iNoBins As Integer = Me.arBinMidVal.Length
            If iNoBins = 0 Then Exit Sub

            'The column chart's categories are bin midpoints.  The secondary XY axis
            'must instead span the outer bin edges so the normal curve and box plot
            'line up exactly with the bar geometry.
            Dim dBinWidth As Double
            If iNoBins >= 2 Then
                dBinWidth = Math.Abs(Me.arBinMidVal(1) - Me.arBinMidVal(0))
            Else
                'Degenerate/one-bin case.  Exact width is not recoverable from the
                'midpoint alone, so use a stable symmetric display range.
                dBinWidth = Math.Max(1.0R, Math.Abs(Me.arBinMidVal(0))) * 2.0R
            End If
            If dBinWidth <= 0.0R OrElse Double.IsNaN(dBinWidth) OrElse Double.IsInfinity(dBinWidth) Then
                dBinWidth = 1.0R
            End If

            Dim dXMin As Double = Me.arBinMidVal(0) - dBinWidth / 2.0R
            Dim dXMax As Double = Me.arBinMidVal(iNoBins - 1) + dBinWidth / 2.0R

            'Use one shared Y scale for histogram frequencies and the already
            'frequency-scaled normal curve.  This fixes the previous independent
            'secondary-Y autoscaling of the normal overlay.
            Dim dPositiveMax As Double = 1.0R
            For Each f As Integer In Me.arFreq
                If f > dPositiveMax Then dPositiveMax = f
            Next
            If bOverlay Then
                For Each y As Double In Me.arGauss
                    If Not Double.IsNaN(y) AndAlso Not Double.IsInfinity(y) AndAlso y > dPositiveMax Then
                        dPositiveMax = y
                    End If
                Next
            End If

            Dim yScale As CHARTscale = ChartScaling(0.0R, dPositiveMax)
            Dim dYMax As Double = yScale.Max
            If dYMax <= 0.0R OrElse Double.IsNaN(dYMax) OrElse Double.IsInfinity(dYMax) Then dYMax = 1.0R
            Dim dYMajor As Double = yScale.Scale
            If dYMajor <= 0.0R OrElse Double.IsNaN(dYMajor) OrElse Double.IsInfinity(dYMajor) Then dYMajor = 1.0R

            'Reserve a band below zero for the Tukey box plot.  Histogram bars still
            'start at zero, and negative value-axis labels are hidden by number format.
            Dim dYMin As Double = If(bDrawBoxPlot, -0.32R * dYMax, 0.0R)
            Dim dBoxY As Double = -0.18R * dYMax
            Dim dBoxHalfHeight As Double = 0.055R * dYMax

            Dim anchor As Range = DirectCast(ws.Cells(r, c), Range)
            Dim chartShape As Shape = ws.Shapes.AddChart()
            chartShape.Left = CSng(anchor.Left)
            chartShape.Top = CSng(anchor.Top)
            chartShape.Width = 480.0F
            chartShape.Height = If(bDrawBoxPlot, 340.0F, 300.0F)

            Dim ch As Chart = chartShape.Chart
            With ch
                .ChartType = XlChartType.xlColumnClustered

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete()
                Loop

                'Histogram bars.
                .SeriesCollection.NewSeries()
                With .SeriesCollection(1)
                    .Values = arFreq
                    .AxisGroup = XlAxisGroup.xlPrimary
                    .XValues = arBinMidVal
                    .Border.Color = RGB(255, 255, 255)
                    With .Format.Line
                        .Visible = True
                        .ForeColor.RGB = RGB(255, 255, 255)
                        .Transparency = 0
                        .Weight = 1.5F
                    End With
                    With .Format.Fill
                        .Visible = True
                        .ForeColor.RGB = RGB(128, 128, 128)
                        .Transparency = 0
                        .Solid()
                    End With
                End With
                .ChartGroups(1).GapWidth = 0

                .HasLegend = False
                .HasTitle = True
                .ChartTitle.Text = strTitle

                With .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary)
                    .HasTitle = True
                    .AxisTitle.Text = "Frequency"
                    .MinimumScale = dYMin
                    .MaximumScale = dYMax
                    .MajorUnit = dYMajor
                    .CrossesAt = 0.0R
                    .TickLabels.NumberFormat = "0.##;;0"
                    Try
                        .MajorGridlines.Delete()
                    Catch
                        'Gridlines are cosmetic; retain the chart if Excel has none.
                    End Try
                End With

                Dim nextSeriesIndex As Integer = 2

                If bOverlay Then
                    .SeriesCollection.NewSeries()
                    With .SeriesCollection(nextSeriesIndex)
                        .ChartType = XlChartType.xlXYScatterSmoothNoMarkers
                        .XValues = arXi
                        .Values = arGauss
                        .AxisGroup = XlAxisGroup.xlSecondary
                        .Name = "Normal Curve"
                    End With
                    nextSeriesIndex += 1
                End If

                If bNeedsSecondaryAxes Then
                    'Secondary horizontal value axis: use the *bin edges*, not the
                    'raw-data min/max, so XY series are aligned with the bars.
                    .HasAxis(XlAxisType.xlCategory, XlAxisGroup.xlSecondary) = True
                    With .Axes(XlAxisType.xlCategory, XlAxisGroup.xlSecondary)
                        .MinimumScale = dXMin
                        .MaximumScale = dXMax
                        .MajorTickMark = XlTickMark.xlTickMarkNone
                        .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                        .Format.Line.Visible = False
                    End With

                    'Keep the secondary Y scale numerically identical to the primary
                    'one.  The normal overlay is already scaled as n * binWidth * density.
                    .HasAxis(XlAxisType.xlValue, XlAxisGroup.xlSecondary) = True
                    With .Axes(XlAxisType.xlValue, XlAxisGroup.xlSecondary)
                        .MinimumScale = dYMin
                        .MaximumScale = dYMax
                        .MajorUnit = dYMajor
                        .MajorTickMark = XlTickMark.xlTickMarkNone
                        .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                        .Format.Line.Visible = False
                    End With
                End If

                'Draw the box plot only after Excel has finalized both axis scales.
                'Unlike the previous thick-line approximation, this uses chart-contained
                'rectangle/line/oval shapes. A chart-event helper reprojects those
                'shapes from data coordinates whenever the embedded chart is resized.
                'That keeps the grey fill exactly inside the Q1-Q3 outline and gives the
                'box genuinely square ends at every chart size.
                If bDrawBoxPlot Then
                    AttachBoxPlotOverlay(ch,
                                         Me.pQ1,
                                         Me.pMedian,
                                         Me.pQ3,
                                         Me.pWhiskerLow,
                                         Me.pWhiskerHigh,
                                         Me.arOutliers,
                                         dXMin,
                                         dXMax,
                                         dYMin,
                                         dYMax,
                                         dBoxY,
                                         dBoxHalfHeight)
                End If

            End With
        End Sub

        'Keep embedded-chart event sinks alive for as long as their charts are usable.
        'Closed/deleted charts are pruned whenever another histogram overlay is attached.
        Private Shared ReadOnly pBoxPlotOverlays As New List(Of HistogramBoxPlotOverlay)

        Private Shared Sub AttachBoxPlotOverlay(chart As Chart,
                                                q1 As Double,
                                                median As Double,
                                                q3 As Double,
                                                whiskerLow As Double,
                                                whiskerHigh As Double,
                                                outliers() As Double,
                                                xMinimum As Double,
                                                xMaximum As Double,
                                                yMinimum As Double,
                                                yMaximum As Double,
                                                boxY As Double,
                                                boxHalfHeight As Double)
            For i As Integer = pBoxPlotOverlays.Count - 1 To 0 Step -1
                If Not pBoxPlotOverlays(i).IsChartAvailable Then
                    pBoxPlotOverlays(i).Detach()
                    pBoxPlotOverlays.RemoveAt(i)
                End If
            Next

            Dim overlay As New HistogramBoxPlotOverlay(chart,
                                                       q1,
                                                       median,
                                                       q3,
                                                       whiskerLow,
                                                       whiskerHigh,
                                                       outliers,
                                                       xMinimum,
                                                       xMaximum,
                                                       yMinimum,
                                                       yMaximum,
                                                       boxY,
                                                       boxHalfHeight)
            overlay.Draw()
            pBoxPlotOverlays.Add(overlay)
        End Sub

        ''' <summary>
        ''' Owns the chart-contained shapes used for the horizontal Tukey box plot and
        ''' keeps them synchronized with the live Excel PlotArea after chart resizing.
        ''' </summary>
        Private NotInheritable Class HistogramBoxPlotOverlay
            'Stable numeric Office enum values. BESHStatNG intentionally avoids a
            'compile-time Microsoft.Office.Core reference; see ViolinPlotExcel.
            Private Const MsoShapeRectangle As Integer = 1
            Private Const MsoShapeOval As Integer = 9

            Private WithEvents pChart As Chart

            Private ReadOnly pQ1 As Double
            Private ReadOnly pMedian As Double
            Private ReadOnly pQ3 As Double
            Private ReadOnly pWhiskerLow As Double
            Private ReadOnly pWhiskerHigh As Double
            Private ReadOnly pOutliers() As Double
            Private ReadOnly pXMinimum As Double
            Private ReadOnly pXMaximum As Double
            Private ReadOnly pYMinimum As Double
            Private ReadOnly pYMaximum As Double
            Private ReadOnly pBoxY As Double
            Private ReadOnly pBoxHalfHeight As Double

            Private pWhiskerLine As Object
            Private pWhiskerLowCap As Object
            Private pWhiskerHighCap As Object
            Private pBox As Object
            Private pMedianLine As Object
            Private ReadOnly pOutlierShapes As New List(Of Object)

            Friend Sub New(chart As Chart,
                           q1 As Double,
                           median As Double,
                           q3 As Double,
                           whiskerLow As Double,
                           whiskerHigh As Double,
                           outliers() As Double,
                           xMinimum As Double,
                           xMaximum As Double,
                           yMinimum As Double,
                           yMaximum As Double,
                           boxY As Double,
                           boxHalfHeight As Double)
                pChart = chart
                pQ1 = q1
                pMedian = median
                pQ3 = q3
                pWhiskerLow = whiskerLow
                pWhiskerHigh = whiskerHigh
                pOutliers = If(outliers, New Double() {})
                pXMinimum = xMinimum
                pXMaximum = xMaximum
                pYMinimum = yMinimum
                pYMaximum = yMaximum
                pBoxY = boxY
                pBoxHalfHeight = boxHalfHeight
            End Sub

            Friend ReadOnly Property IsChartAvailable As Boolean
                Get
                    If pChart Is Nothing Then Return False
                    Try
                        Dim unused As String = pChart.Name
                        Return True
                    Catch
                        Return False
                    End Try
                End Get
            End Property

            Friend Sub Detach()
                pWhiskerLine = Nothing
                pWhiskerLowCap = Nothing
                pWhiskerHighCap = Nothing
                pBox = Nothing
                pMedianLine = Nothing
                pOutlierShapes.Clear()
                pChart = Nothing
            End Sub

            Friend Sub Draw()
                If pChart Is Nothing Then Exit Sub

                Dim chartObject As Object = pChart
                Dim chartShapes As Object = chartObject.Shapes

                pWhiskerLine = chartShapes.AddLine(0.0F, 0.0F, 1.0F, 0.0F)
                FormatLine(pWhiskerLine, 1.25F)

                pWhiskerLowCap = chartShapes.AddLine(0.0F, 0.0F, 0.0F, 1.0F)
                FormatLine(pWhiskerLowCap, 1.25F)

                pWhiskerHighCap = chartShapes.AddLine(0.0F, 0.0F, 0.0F, 1.0F)
                FormatLine(pWhiskerHighCap, 1.25F)

                pBox = chartShapes.AddShape(MsoShapeRectangle, 0.0F, 0.0F, 1.0F, 1.0F)
                With pBox.Fill
                    .Visible = True
                    .Solid()
                    .ForeColor.RGB = RGB(192, 192, 192)
                    .Transparency = 0.0F
                End With
                With pBox.Line
                    .Visible = True
                    .ForeColor.RGB = RGB(0, 0, 0)
                    .Transparency = 0.0F
                    .Weight = 1.0F
                End With

                pMedianLine = chartShapes.AddLine(0.0F, 0.0F, 0.0F, 1.0F)
                FormatLine(pMedianLine, 1.25F)

                For i As Integer = 0 To pOutliers.Length - 1
                    Dim pointShape As Object = chartShapes.AddShape(MsoShapeOval, 0.0F, 0.0F, 5.0F, 5.0F)
                    Try
                        pointShape.LockAspectRatio = True
                    Catch
                        'Some Excel versions expose this property inconsistently for chart shapes.
                    End Try
                    With pointShape.Fill
                        .Visible = True
                        .Solid()
                        .ForeColor.RGB = RGB(255, 255, 255)
                        .Transparency = 0.0F
                    End With
                    With pointShape.Line
                        .Visible = True
                        .ForeColor.RGB = RGB(0, 0, 0)
                        .Transparency = 0.0F
                        .Weight = 1.0F
                    End With
                    pOutlierShapes.Add(pointShape)
                Next

                PositionShapes()
            End Sub

            Private Shared Sub FormatLine(lineShape As Object, weight As Single)
                With lineShape.Line
                    .Visible = True
                    .ForeColor.RGB = RGB(0, 0, 0)
                    .Transparency = 0.0F
                    .Weight = weight
                End With
            End Sub

            Private Sub Chart_Resize() Handles pChart.Resize
                'Excel changes PlotArea.Inside* non-proportionally when the chart is
                'stretched because titles/labels keep approximately fixed point sizes.
                'Reproject from data coordinates instead of relying on shape scaling.
                PositionShapes()
            End Sub

            Private Sub PositionShapes()
                If pChart Is Nothing OrElse pBox Is Nothing Then Exit Sub

                Try
                    Dim plotArea As PlotArea = pChart.PlotArea
                    Dim insideLeft As Double = plotArea.InsideLeft
                    Dim insideTop As Double = plotArea.InsideTop
                    Dim insideWidth As Double = plotArea.InsideWidth
                    Dim insideHeight As Double = plotArea.InsideHeight

                    If insideWidth <= 0.0R OrElse insideHeight <= 0.0R Then Exit Sub

                    Dim whiskerLeft As Double = DataXToPlotX(pWhiskerLow, insideLeft, insideWidth)
                    Dim whiskerRight As Double = DataXToPlotX(pWhiskerHigh, insideLeft, insideWidth)
                    Dim q1X As Double = DataXToPlotX(pQ1, insideLeft, insideWidth)
                    Dim q3X As Double = DataXToPlotX(pQ3, insideLeft, insideWidth)
                    Dim medianX As Double = DataXToPlotX(pMedian, insideLeft, insideWidth)
                    Dim centreY As Double = DataYToPlotY(pBoxY, insideTop, insideHeight)
                    Dim topY As Double = DataYToPlotY(pBoxY + pBoxHalfHeight, insideTop, insideHeight)
                    Dim bottomY As Double = DataYToPlotY(pBoxY - pBoxHalfHeight, insideTop, insideHeight)

                    Dim capHalfHeight As Double = Math.Max(3.0R, Math.Abs(bottomY - topY) * 0.18R)

                    SetLineBounds(pWhiskerLine, whiskerLeft, centreY, whiskerRight, centreY)
                    SetLineBounds(pWhiskerLowCap,
                                  whiskerLeft,
                                  centreY - capHalfHeight,
                                  whiskerLeft,
                                  centreY + capHalfHeight)
                    SetLineBounds(pWhiskerHighCap,
                                  whiskerRight,
                                  centreY - capHalfHeight,
                                  whiskerRight,
                                  centreY + capHalfHeight)

                    pBox.Left = CSng(Math.Min(q1X, q3X))
                    pBox.Top = CSng(Math.Min(topY, bottomY))
                    pBox.Width = CSng(Math.Max(1.0R, Math.Abs(q3X - q1X)))
                    pBox.Height = CSng(Math.Max(1.0R, Math.Abs(bottomY - topY)))

                    SetLineBounds(pMedianLine, medianX, topY, medianX, bottomY)

                    Const markerSize As Double = 5.0R
                    For i As Integer = 0 To pOutlierShapes.Count - 1
                        Dim x As Double = DataXToPlotX(pOutliers(i), insideLeft, insideWidth)
                        Dim shp As Object = pOutlierShapes(i)
                        shp.Left = CSng(x - markerSize / 2.0R)
                        shp.Top = CSng(centreY - markerSize / 2.0R)
                        shp.Width = CSng(markerSize)
                        shp.Height = CSng(markerSize)
                    Next
                Catch
                    'A deleted chart/workbook can invalidate COM objects between Resize
                    'notifications. The registry will prune the handler on the next attach.
                End Try
            End Sub

            Private Function DataXToPlotX(value As Double,
                                          insideLeft As Double,
                                          insideWidth As Double) As Double
                Dim span As Double = pXMaximum - pXMinimum
                If span <= 0.0R Then Return insideLeft + insideWidth / 2.0R
                Return insideLeft + (value - pXMinimum) * insideWidth / span
            End Function

            Private Function DataYToPlotY(value As Double,
                                          insideTop As Double,
                                          insideHeight As Double) As Double
                Dim span As Double = pYMaximum - pYMinimum
                If span <= 0.0R Then Return insideTop + insideHeight / 2.0R
                Return insideTop + (pYMaximum - value) * insideHeight / span
            End Function

            Private Shared Sub SetLineBounds(lineShape As Object,
                                             x1 As Double,
                                             y1 As Double,
                                             x2 As Double,
                                             y2 As Double)
                lineShape.Left = CSng(Math.Min(x1, x2))
                lineShape.Top = CSng(Math.Min(y1, y2))
                lineShape.Width = CSng(Math.Max(0.01R, Math.Abs(x2 - x1)))
                lineShape.Height = CSng(Math.Max(0.01R, Math.Abs(y2 - y1)))
            End Sub
        End Class

    End Class

End Namespace
