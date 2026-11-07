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
            Dim prepared As New MultipleComparisonGeometry35(data.Estimates, data.LowerLimits, data.UpperLimits)
            Dim y As Double() = prepared.YValues
            Dim plusError As Double() = prepared.PlusError
            Dim minusError As Double() = prepared.MinusError
            Dim minCI As Double = prepared.MinimumX
            Dim maxCI As Double = prepared.MaximumX
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
                .XValues = prepared.ZeroReference.XValues
                .Values = prepared.ZeroReference.YValues
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
            Return ExcelCommon.Chart34.ComparisonSeries.ComparisonSeriesPreparation34.SuggestedHeight(data)
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
                .MaximumScale = ExcelCommon.ComparisonChartPolicy35.ValueAxisMaximum(data.Estimates.Length)
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
                For Each ordinal As Integer In ExcelCommon.ComparisonChartPolicy35.LabelOrdinals(series.Points.Count, labels.Length)
                    Dim i As Integer = ExcelCommon.ComparisonChartPolicy35.PointIndex(ordinal)
                    Dim point As Point = CType(series.Points(i), Point)
                    point.HasDataLabel = True
                    point.DataLabel.Text = ExcelCommon.ComparisonChartPolicy35.LabelAt(labels, ordinal).Text
                    point.DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                    point.DataLabel.Font.Size = 8
                Next
            Catch
                'The chart remains useful even if an older Excel version rejects
                'individual data-label positioning.
            End Try
        End Sub
        Private Shared Function BuildTitle(data As parametric.MultipleComparisonPlotData) As String
            Return ExcelCommon.ComparisonChartPolicy35.BuildTitle(data)
        End Function
        Private Shared Sub ValidatePlotData(data As parametric.MultipleComparisonPlotData)
            ExcelCommon.ComparisonChartPolicy35.ValidatePlotData(data)
        End Sub

    End Class

End Namespace
