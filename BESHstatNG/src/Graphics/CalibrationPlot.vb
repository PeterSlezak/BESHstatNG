Option Explicit On
Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Creates a calibration plot for binary probabilistic models.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A calibration plot compares the mean predicted probability in each bin with the
    ''' observed event rate in the same bin. Perfect calibration would place all points
    ''' on the diagonal reference line <c>y = x</c>.
    ''' </para>
    ''' <para>
    ''' This class is intentionally lightweight and mirrors the style of the existing
    ''' plotting helpers such as <see cref="ROC"/>. It expects pre-computed bin summaries,
    ''' typically produced by <c>regression.BinaryClassificationReporting.BuildCalibrationBins</c>.
    ''' </para>
    ''' <para>
    ''' The chart includes:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>Calibration points: x = mean predicted probability, y = observed event rate.</description></item>
    '''   <item><description>Diagonal reference line <c>y = x</c>.</description></item>
    '''   <item><description>Axis scaling fixed to the probability range [0,1].</description></item>
    '''   <item><description>Chart title and axis labels suitable for workbook output.</description></item>
    ''' </list>
    ''' <para>
    ''' Confidence limits are carried in the input rows and can be surfaced later through
    ''' manual Excel error bars if desired. The first implementation focuses on the core
    ''' point-vs-reference-line plot.
    ''' </para>
    ''' </remarks>
    Public Class CalibrationPlot

        ''' <summary>Calibration rows used for plotting.</summary>
        Private ReadOnly pRows As IList(Of regression.CalibrationBinSummary)

        ''' <summary>Optional chart title.</summary>
        Private ReadOnly pTitle As String

        ''' <summary>
        ''' Initializes a new calibration plot from pre-computed calibration-bin summaries.
        ''' </summary>
        ''' <param name="rows">
        ''' Calibration-bin summaries. Each row should contain at least:
        ''' <list type="bullet">
        '''   <item><description><c>MeanPredicted</c> — mean fitted probability in the bin.</description></item>
        '''   <item><description><c>ObservedRate</c> — empirical event rate in the bin.</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="title">Optional chart title. Default is <c>Calibration plot</c>.</param>
        Public Sub New(rows As IList(Of regression.CalibrationBinSummary), Optional title As String = "Calibration plot")
            Me.pRows = rows
            Me.pTitle = If(String.IsNullOrWhiteSpace(title), "Calibration plot", title)
        End Sub

        ''' <summary>
        ''' Adds a calibration plot to the supplied worksheet.
        ''' </summary>
        ''' <param name="ws">Worksheet that will receive the chart.</param>
        ''' <param name="left">Optional left position in points. Default 10.</param>
        ''' <param name="top">Optional top position in points. Default 10.</param>
        ''' <param name="width">Optional chart width in points. Default 320.</param>
        ''' <param name="height">Optional chart height in points. Default 270.</param>
        ''' <returns>
        ''' The created Excel chart, or <c>Nothing</c> when there are no plottable calibration points.
        ''' </returns>
        Public Function addCalibrationPlot(ws As Worksheet,
                                           Optional left As Double = 10,
                                           Optional top As Double = 10,
                                           Optional width As Double = 320,
                                           Optional height As Double = 270) As Chart
            If ws Is Nothing Then Return Nothing
            If Me.pRows Is Nothing OrElse Me.pRows.Count = 0 Then Return Nothing

            Dim xs As New List(Of Double)()
            Dim ys As New List(Of Double)()
            For Each r As regression.CalibrationBinSummary In Me.pRows
                If Double.IsNaN(r.MeanPredicted) OrElse Double.IsNaN(r.ObservedRate) Then Continue For
                xs.Add(r.MeanPredicted)
                ys.Add(r.ObservedRate)
            Next
            If xs.Count = 0 Then Return Nothing

            Dim shp = ws.Shapes.AddChart(Left:=left, Top:=top, Width:=width, Height:=height)
            With shp.Chart
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                ' Calibration points
                .SeriesCollection.NewSeries()
                With .SeriesCollection(1)
                    .Name = "Calibration"
                    .XValues = xs.ToArray()
                    .Values = ys.ToArray()
                    .ChartType = XlChartType.xlXYScatter
                    .MarkerStyle = 8
                    .MarkerSize = 6
                    .Format.Line.Visible = False
                    .MarkerForegroundColor = RGB(70, 70, 70)
                    .MarkerBackgroundColor = RGB(70, 70, 70)
                End With

                ' Reference line y = x
                .SeriesCollection.NewSeries()
                With .SeriesCollection(2)
                    .Name = "Reference line"
                    .XValues = New Double() {0.0R, 1.0R}
                    .Values = New Double() {0.0R, 1.0R}
                    .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1.25
                    End With
                End With

                With .Axes(XlAxisType.xlCategory)
                    .MinimumScale = 0
                    .MaximumScale = 1
                    .CrossesAt = 0
                    .MajorUnit = 0.2
                    .HasTitle = False
                    .HasTitle = True
                    .AxisTitle.Text = "Mean predicted probability"
                    Try
                        .MajorGridlines.Delete()
                    Catch
                    End Try
                End With

                With .Axes(XlAxisType.xlValue)
                    .MinimumScale = 0
                    .MaximumScale = 1
                    .CrossesAt = 0
                    .MajorUnit = 0.2
                    .HasTitle = False
                    .HasTitle = True
                    .AxisTitle.Text = "Observed event rate"
                    Try
                        .MajorGridlines.Delete()
                    Catch
                    End Try
                End With

                Try
                    .Legend.Position = XlLegendPosition.xlLegendPositionBottom
                Catch
                End Try

                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = Me.pTitle
            End With

            Return shp.Chart
        End Function
    End Class
End Namespace
