Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace graphics


    ''' <summary>
    ''' Creates a symmetry plot (Lovie plot) for assessing whether a univariate
    ''' distribution is symmetric about its median.
    ''' 
    ''' A symmetry plot compares:
    ''' <list type="bullet">
    '''   <item><description>**[Upper distance to median](guide://action?prefill=Tell%20me%20more%20about%3A%20Upper%20distance%20to%20median)**: Y(n−i+1) − median</description></item>
    '''   <item><description>**[Lower distance to median](guide://action?prefill=Tell%20me%20more%20about%3A%20Lower%20distance%20to%20median)**: median − Y(i)</description></item>
    ''' </list>
    ''' for i = 1 … ⌊n/2⌋, where Y is the sorted data.
    ''' 
    ''' Interpretation:
    ''' <list type="bullet">
    '''   <item><description>Points lying close to the **[45° reference line](guide://action?prefill=Tell%20me%20more%20about%3A%2045%C2%B0%20reference%20line)** indicate symmetry.</description></item>
    '''   <item><description>Points **above** the line suggest **left‑skewness**.</description></item>
    '''   <item><description>Points **below** the line suggest **right‑skewness**.</description></item>
    ''' </list>
    ''' 
    ''' Reference:
    ''' <para>
    ''' Lovie, S. (2005). “Symmetry Plot.” In *Encyclopedia of Statistics in Behavioral Science*,
    ''' Wiley, Vol. 4, pp. 1989–1990.
    ''' </para>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>Median</c> — computes sample median</description></item>
    '''   <item><description><c>RoundDown</c> — integer floor</description></item>
    '''   <item><description><c>ChartScaling</c> — axis scaling helper</description></item>
    '''   <item><description>Excel interop (<c>Worksheet</c>, <c>Chart</c>)</description></item>
    ''' </list>
    ''' </summary>
    Public Class SymetryPlot

        ''' <summary>Input data vector.</summary>
        Private pData() As Double

        ''' <summary>Number of observations.</summary>
        Private n As Integer


        ''' <summary>
        ''' Initializes the symmetry‑plot object with the supplied data.
        ''' </summary>
        ''' <param name="data">Numeric vector to be analyzed.</param>
        Sub New(data() As Double)
            Me.pData = data
            Me.n = data.Length
        End Sub

        ''' <summary>
        ''' Generates a symmetry plot in Excel for visually assessing distributional
        ''' symmetry about the sample median.
        ''' 
        ''' Steps:
        ''' <list type="number">
        '''   <item><description>Sorts the data and computes the **[median](guide://action?prefill=Tell%20me%20more%20about%3A%20median)**.</description></item>
        '''   <item><description>Computes absolute distances from the median for values
        '''     above and below it.</description></item>
        '''   <item><description>Pairs the largest upper distances with the largest lower
        '''     distances.</description></item>
        '''   <item><description>Constructs a 45° reference line for comparison.</description></item>
        '''   <item><description>Plots the paired distances in an XY scatter plot.</description></item>
        '''   <item><description>Applies axis scaling using <c>ChartScaling</c>.</description></item>
        ''' </list>
        ''' 
        ''' Interpretation:
        ''' <list type="bullet">
        '''   <item><description>Points near the diagonal → **[symmetric distribution](guide://action?prefill=Tell%20me%20more%20about%3A%20symmetric%20distribution)**</description></item>
        '''   <item><description>Points above diagonal → **left‑skewed**</description></item>
        '''   <item><description>Points below diagonal → **right‑skewed**</description></item>
        ''' </list>
        ''' 
        ''' External dependency:
        ''' <list type="bullet">
        '''   <item><description>Excel interop (<c>Worksheet</c>, <c>Chart</c>)</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="strTitle">Chart title (typically the variable name).</param>
        ''' <param name="lTop">Optional top coordinate for chart placement.</param>
        ''' <param name="lLeft">Optional left coordinate for chart placement.</param>
        Sub AsymmetryPlot(strTitle As String, Optional lTop As Integer = -1, Optional lLeft As Integer = -1)

            'Symmetry Plot provides a graphical test of whether a sample is symmetrically distributed about a measure of location;
            'in this case, the median. Having such information about a sample is useful in that just about all tests of significance
            'assume that the parent population from which the sample came is at least symmetrical about some location parameter and,
            'in effect, that the sample should not markedly violate this condition either.
            'It consists of:    Vertical axis = Y(n-i+1) - median;
            '                   Horizontal axis = median - Y(i);
            'where median is the sample median, Y is sample variable, and i goes from 1 to the index of the median point. This plot
            'graphs the distance from the median of points above the median against the corresponding points below the median. The
            'interpertation of this plot is that the closer these points lie to the 45 degree line, the more symmetric the data is.
            'When data are skewed to the left than data on the symmetry plot lying above the reference comparison line and
            'increasingly diverge from it, as one moves to the right. If the data are skewed to the right, then the plotted data
            'appeared below the comparison line.
            'Lovie S. Symmetry Plot. In Encyclopedia of Statistics in Behavioral Science, John Wiley & Sons, 2005, Vol 4, 1989–1990

            Dim Xref() As Double, Yref() As Double

            'prepare data for plotting
            'compute differences below and above median
            Dim Medn As Double = Median(Me.pData)
            Array.Sort(Me.pData)
            Dim x(n - 1) As Double
            For i = 0 To n - 1
                If pData(i) <= Medn Then
                    x(i) = Medn - pData(i)
                Else
                    x(i) = pData(i) - Medn
                End If
            Next

            'prepare arrays for plotting
            Dim ii As Integer = RoundDown(n / 2, 0)
            Dim Xs(ii - 1) As Double, Ys(ii - 1) As Double
            For i = 0 To ii - 1
                Xs(i) = x(n - 1 - i)
                Ys(i) = x(i)
            Next

            'reference line data
            If x(0) >= x(n - 1) Then
                Xref = {x(0), x(ii - 1)}
                Yref = Xref
            Else
                Xref = {x(n - 1), x(n - 1 - ii)}
                Yref = Xref
            End If

            'compute optimal scaling
            Dim udPlotAxis = ChartScaling(0, x.Max())

            If lLeft = -1 Then lLeft = 100
            If lTop = -1 Then lTop = 100
            With AppGlobals.app.ActiveSheet.Shapes.AddChart(Left:=lLeft, Top:=lTop, Width:=300, Height:=270)
                With .Chart
                    .ChartType = XlChartType.xlXYScatter

                    'delete extra series
                    Do Until .SeriesCollection.Count = 0
                        .SeriesCollection(1).Delete
                    Loop

                    With .Axes(XlAxisType.xlValue)
                        .MinimumScale = 0
                        .MaximumScale = udPlotAxis.Max
                        .MajorUnit = udPlotAxis.Scale
                        .MajorGridlines.Delete
                    End With
                    .Axes(XlAxisType.xlCategory).MinimumScale = 0
                    .Axes(XlAxisType.xlCategory).MaximumScale = udPlotAxis.Max

                    .SeriesCollection.NewSeries
                    With .SeriesCollection(1)
                        .XValues = Xs
                        .Values = Ys
                        .MarkerStyle = 2
                        .MarkerSize = 3
                        .MarkerForegroundColor = RGB(0, 0, 0)
                        .MarkerBackgroundColor = RGB(0, 0, 0)
                        .Name = "symmetry data"
                    End With

                    'plot reference line
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(2)
                        .XValues = Xref
                        .Values = Yref
                        .MarkerStyle = -4142
                        .Border.Color = RGB(0, 0, 0)
                        .Name = "45° reference line"
                        With .Format.Line
                            .Visible = True
                            .Weight = 1.5
                        End With
                    End With

                    Try
                        .Legend.Delete()
                        .HasTitle = False
                        .HasTitle = True
                        .ChartTitle.text = "Symmetry plot - " & strTitle
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = "Upper distance to median"
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = "Lower distance to median"
                    Catch
                    End Try
                End With
            End With
        End Sub
    End Class
End Namespace