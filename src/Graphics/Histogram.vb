Option Explicit On
Imports System.Xml
Imports Microsoft.Office.Interop.Excel

Namespace graphics


    ''' <summary>
    ''' Implements histogram computation and Excel‑based visualization for a
    ''' univariate numeric dataset.  
    ''' 
    ''' The class supports:
    ''' <list type="bullet">
    '''   <item><description>Automatic bin computation using <c>HistogramBinsComputation</c></description></item>
    '''   <item><description>Extraction of bin midpoints and frequencies</description></item>
    '''   <item><description>Optional Gaussian overlay curve based on sample mean and variance</description></item>
    '''   <item><description>Excel chart generation with primary and secondary axes</description></item>
    ''' </list>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>HistogramBinsComputation</c> — computes histogram bins</description></item>
    '''   <item><description><c>GaussOverlayComputation</c> — computes normal density overlay</description></item>
    '''   <item><description><c>GetColumnFrom2Darray</c>, <c>Array2dblArray</c>, <c>Array2intArray</c></description></item>
    '''   <item><description>Excel interop (<c>Worksheet</c>, <c>Workbook</c>, <c>Chart</c>)</description></item>
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

        ''' <summary>X‑coordinates for Gaussian overlay curve.</summary>
        Private arXi() As Double

        ''' <summary>Gaussian density values for overlay curve.</summary>
        Private arGauss() As Double = Nothing


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
        ''' Computes histogram bins and optionally a Gaussian overlay curve.
        ''' 
        ''' Steps:
        ''' <list type="number">
        '''   <item><description>Calls <c>HistogramBinsComputation</c> to obtain bin midpoints and frequencies.</description></item>
        '''   <item><description>Extracts midpoints and frequencies into 1D arrays.</description></item>
        '''   <item><description>If <paramref name="bOveraly"/> is <c>True</c>, computes a normal curve overlay using
        '''     <c>GaussOverlayComputation</c>.</description></item>
        ''' </list>
        ''' 
        ''' The returned 2D array contains:
        ''' <list type="bullet">
        '''   <item><description>Column 0: Bin midpoints</description></item>
        '''   <item><description>Column 1: Bin frequencies</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="bOveraly">If <c>True</c>, computes Gaussian overlay curve.</param>
        ''' <param name="strType">Optional histogram type parameter passed to the binning routine.</param>
        ''' <returns>A 2D array of histogram bin midpoints and frequencies.</returns>
        Public Function compute(bOveraly As Boolean, Optional strType As String = "") As Object(,)

            Dim bins = HistogramBinsComputation(Me.data, strType)
            Me.arBinMidVal = Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(bins, 0))
            Me.arFreq = Matrix.Array2intArray(Matrix.GetColumnFrom2Darray(bins, 1))

            If bOveraly Then
                Dim overlayData = GaussOverlayComputation(Me.data, Me.arBinMidVal)
                Me.arXi = Matrix.GetColumnFrom2Darray(overlayData, 0)
                Me.arGauss = Matrix.GetColumnFrom2Darray(overlayData, 1)
            End If

            Return bins
        End Function

        ''' <summary>
        ''' Creates an Excel histogram chart at the specified worksheet location.
        ''' 
        ''' Features:
        ''' <list type="bullet">
        '''   <item><description>Clustered column histogram using bin midpoints and frequencies</description></item>
        '''   <item><description>Optional Gaussian overlay curve plotted on a secondary axis</description></item>
        '''   <item><description>Automatic axis scaling and formatting</description></item>
        '''   <item><description>Customizable chart title</description></item>
        ''' </list>
        ''' 
        ''' Overlay details:
        ''' <list type="bullet">
        '''   <item><description>Gaussian curve is drawn as a smooth scatter plot</description></item>
        '''   <item><description>Secondary X‑axis is scaled to the overlay domain</description></item>
        '''   <item><description>Histogram bars remain on the primary axis</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description>Excel interop (<c>Worksheet</c>, <c>Chart</c>, <c>SeriesCollection</c>)</description></item>
        '''   <item><description><c>arBinMidVal</c>, <c>arFreq</c>, <c>arXi</c>, <c>arGauss</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="ws">Worksheet where the chart will be created.</param>
        ''' <param name="r">Row offset for chart placement.</param>
        ''' <param name="c">Column offset for chart placement.</param>
        ''' <param name="strTitle">Chart title.</param>
        Sub addChart(ByRef ws As Worksheet, r As Integer, c As Integer, strTitle As String)
            Dim bOverlay As Boolean, dMin As Double, dMax As Double

            If Me.arGauss Is Nothing Then
                bOverlay = False
            Else
                bOverlay = True
                dMin = Me.arXi.Min()
                dMax = Me.arXi.Max()
            End If

            Dim iNoBins As Integer = Me.arBinMidVal.Length

            With Me.pWs.Shapes.AddChart
                With .Chart
                    .ChartType = XlChartType.xlColumnClustered

                    'delete extra series
                    Do Until .SeriesCollection.Count = 0
                        .SeriesCollection(1).Delete
                    Loop

                    '.SetSourceData(Source:=ws.Range(ws.Cells(r + 1, c + 1), ws.Cells(r + iNoBins, c + 1)))
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(1)
                        .Values = arFreq
                        .AxisGroup = 1
                        '.XValues = ws.Range(ws.Cells(r + 1, c), ws.Cells(r + iNoBins, c))
                        .XValues = arBinMidVal
                        .Border.Color = RGB(255, 255, 255)
                        With .Format.Line
                            .Visible = True
                            .ForeColor.RGB = RGB(255, 255, 255)
                            .Transparency = 0
                            .Weight = 1.5
                        End With
                        With .Format.Fill
                            .Visible = True
                            .ForeColor.RGB = RGB(128, 128, 128)
                            .Transparency = 0
                            .Solid
                        End With
                    End With
                    .ChartGroups(1).GapWidth = 0
                    .Legend.Delete()
                    .Axes(XlAxisType.xlValue).MajorGridlines.Delete
                    .Axes(XlAxisType.xlValue).MinimumScale = 0
                    .HasTitle = False
                    .HasTitle = True
                    .ChartTitle.Text = strTitle
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = "Frequency"

                    If bOverlay Then
                        'Set up the secondary axis
                        .SeriesCollection.NewSeries
                        With .SeriesCollection(2)
                            .ChartType = XlChartType.xlXYScatterSmoothNoMarkers
                            .XValues = arXi
                            .Values = arGauss
                            .AxisGroup = 2
                            .Name = "Normal Curve"
                        End With

                        'set secondary X axis
                        .HasAxis(XlAxisType.xlCategory, XlAxisGroup.xlSecondary) = True
                        With .Axes(XlAxisType.xlCategory, XlAxisGroup.xlSecondary)
                            .MinimumScale = dMin
                            .MaximumScale = dMax
                            .MajorTickMark = XlTickMark.xlTickMarkNone
                            .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                            .Format.Line.ForeColor.RGB = RGB(255, 255, 255)
                        End With
                        .HasAxis(XlAxisType.xlValue, XlAxisGroup.xlSecondary) = False
                    End If
                End With

            End With
        End Sub

    End Class
End Namespace