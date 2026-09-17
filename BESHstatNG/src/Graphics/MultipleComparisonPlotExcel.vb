Option Explicit On

Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Draws a forest-style confidence-interval chart for one-way ANOVA
    ''' multiple-comparison results. Each point is the estimated difference of means,
    ''' the horizontal error bar is its confidence interval, and the vertical reference
    ''' line at zero indicates no difference.
    ''' </summary>
    Public NotInheritable Class MultipleComparisonPlotExcel

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Adds a multiple-comparison confidence-interval chart to an Excel worksheet.
        ''' </summary>
        Public Shared Function AddPlot(ws As Worksheet,
                                       data As parametric.MultipleComparisonPlotData,
                                       left As Double,
                                       top As Double,
                                       Optional width As Double = 560.0) As Chart
            If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
            ValidatePlotData(data)

            Dim n As Integer = data.Estimates.Length
            Dim height As Double = SuggestedHeight(data)
            Dim y(n - 1) As Double
            Dim plusError(n - 1) As Double
            Dim minusError(n - 1) As Double
            Dim minCI As Double = data.LowerLimits(0)
            Dim maxCI As Double = data.UpperLimits(0)

            'Reverse the numeric y positions so the first comparison is shown at the top.
            For i As Integer = 0 To n - 1
                y(i) = n - i
                plusError(i) = Math.Max(0.0, data.UpperLimits(i) - data.Estimates(i))
                minusError(i) = Math.Max(0.0, data.Estimates(i) - data.LowerLimits(i))
                minCI = Math.Min(minCI, data.LowerLimits(i))
                maxCI = Math.Max(maxCI, data.UpperLimits(i))
            Next

            'Always include zero in the visible range because it is the null reference.
            minCI = Math.Min(minCI, 0.0)
            maxCI = Math.Max(maxCI, 0.0)
            Dim xScale As CHARTscale = ChartScaling(minCI, maxCI)

            Dim shape As Object = ws.Shapes.AddChart()
            shape.Left = CSng(left)
            shape.Top = CSng(top)
            shape.Width = CSng(width)
            shape.Height = CSng(height)

            Dim ch As Chart = shape.Chart
            ch.ChartType = XlChartType.xlXYScatter

            Do Until ch.SeriesCollection.Count = 0
                ch.SeriesCollection(1).Delete()
            Loop

            'Estimated mean differences with custom horizontal confidence intervals.
            ch.SeriesCollection.NewSeries()
            'Dim estimateSeries As Series = CType(ch.SeriesCollection(1), Series)
            With ch.SeriesCollection(1)
                .Name = "Difference of means"
                .XValues = data.Estimates
                .Values = y
                .ChartType = XlChartType.xlXYScatter
                .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                .MarkerSize = 7
                .MarkerForegroundColor = RGB(31, 78, 121)
                .MarkerBackgroundColor = RGB(31, 78, 121)
                .Format.Line.Visible = False
                .ErrorBar(Direction:=XlErrorBarDirection.xlX,
                          Include:=Constants.xlBoth,
                          Type:=XlErrorBarType.xlErrorBarTypeCustom,
                          Amount:=plusError,
                          MinusValues:=minusError)
                Try
                    .ErrorBars.Format.Line.Weight = 1.25F
                    .ErrorBars.Format.Line.ForeColor.RGB = RGB(31, 78, 121)
                Catch
                    'Formatting is cosmetic; retain the chart if a particular Excel version
                    'does not expose ErrorBars.Format through COM.
                End Try
            End With

            'Zero-difference reference line.
            ch.SeriesCollection.NewSeries()
            With ch.SeriesCollection(2)
                .Name = "No difference"
                .XValues = New Double() {0.0, 0.0}
                .Values = New Double() {0.5, n + 0.5}
                .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                .Format.Line.Visible = True
                .Format.Line.Weight = 1.0F
                .Format.Line.DashStyle = 4 'MsoLineDashStyle.msoLineDash
                .Format.Line.ForeColor.RGB = RGB(120, 120, 120)
            End With

            ConfigureAxes(ch, data, xScale)
            'Attach comparison labels to the actual mean-difference points, not to
            'the two-point zero-reference series. This keeps each comparison name
            'visually associated with its estimate and CI.
            AddComparisonLabels(ch.SeriesCollection(1), data.Labels)

            ch.HasLegend = False
            ch.HasTitle = True
            ch.ChartTitle.Text = BuildTitle(data)
            ch.ChartTitle.Font.Size = 12

            Return ch
        End Function

        Public Shared Function SuggestedHeight(data As parametric.MultipleComparisonPlotData) As Double
            If data Is Nothing OrElse data.Estimates Is Nothing Then Return 320.0
            Return Math.Max(320.0, Math.Min(720.0, 150.0 + 32.0 * data.Estimates.Length))
        End Function

        Private Shared Sub ConfigureAxes(ch As Chart,
                                         data As parametric.MultipleComparisonPlotData,
                                         xScale As CHARTscale)
            With ch.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary)
                .MinimumScale = xScale.Min
                .MaximumScale = xScale.Max
                .MajorUnit = xScale.Scale
                .CrossesAt = xScale.Min
                .HasTitle = True
                .AxisTitle.Text = "Difference of means"
            End With

            With ch.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary)
                .MinimumScale = 0.5
                'Leave a little extra room above the first point for its data label.
                .MaximumScale = data.Estimates.Length + 0.8
                .MajorUnit = 1.0
                .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                .HasMajorGridlines = False
                .CrossesAt = 0.5
            End With

            Try
                ch.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).MajorGridlines.Delete()
            Catch
            End Try
        End Sub

        Private Shared Sub AddComparisonLabels(series As Series, labels() As String)
            Try
                For i As Integer = 1 To Math.Min(series.Points.Count, labels.Length)
                    Dim point As Point = CType(series.Points(i), Point)
                    point.HasDataLabel = True
                    point.DataLabel.Text = labels(i - 1)
                    point.DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                    point.DataLabel.Font.Size = 8
                Next
            Catch
                'The chart remains useful even if an older Excel version rejects
                'individual data-label positioning.
            End Try
        End Sub

        Private Shared Function BuildTitle(data As parametric.MultipleComparisonPlotData) As String
            Dim confidenceLevel As Double = (1.0 - data.Alpha) * 100.0
            Dim qualifier As String
            Select Case data.ProcedureName
                Case "Tukey-Kramer", "Bonferroni", "Games-Howell"
                    qualifier = " Simultaneous "
                Case Else
                    qualifier = " "
            End Select
            Return data.ProcedureName & qualifier & confidenceLevel.ToString("0.##") & "% CIs" & vbLf &
                   "Differences of Means"
        End Function

        Private Shared Sub ValidatePlotData(data As parametric.MultipleComparisonPlotData)
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If data.Labels Is Nothing OrElse data.Estimates Is Nothing OrElse
               data.LowerLimits Is Nothing OrElse data.UpperLimits Is Nothing Then
                Throw New ArgumentException("Multiple-comparison plot data are incomplete.", NameOf(data))
            End If
            Dim n As Integer = data.Estimates.Length
            If n = 0 OrElse data.Labels.Length <> n OrElse data.LowerLimits.Length <> n OrElse
               data.UpperLimits.Length <> n Then
                Throw New ArgumentException("Multiple-comparison plot arrays must have equal non-zero lengths.", NameOf(data))
            End If
        End Sub

    End Class

End Namespace
