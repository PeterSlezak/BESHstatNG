Option Explicit On
Imports Microsoft.Office.Interop.Excel

Namespace graphics


    ''' <summary>
    ''' Represents axis‑scaling parameters for Excel charts.
    ''' 
    ''' A <see cref="CHARTscale"/> object contains:
    ''' <list type="bullet">
    '''   <item><description><c>Min</c> — lower axis limit</description></item>
    '''   <item><description><c>Max</c> — upper axis limit</description></item>
    '''   <item><description><c>Scale</c> — major tick interval</description></item>
    ''' </list>
    ''' 
    ''' These values are typically computed using <c>ChartScaling</c>, which applies
    ''' the algorithm described in:
    ''' <para>
    ''' Rob Bovey, Dennis Wallentin, Stephen Bullen, John Green,  
    ''' <i>Professional Excel Development</i>, 2nd ed., Addison‑Wesley, 2009, p. 706.
    ''' </para>
    ''' </summary>
    Public Class CHARTscale
        Public Min As Double
        Public Max As Double
        Public Scale As Double
    End Class

    ''' <summary>
    ''' Provides helper functions for statistical charting in Excel, including:
    ''' <list type="bullet">
    '''   <item><description>Color selection for multiple series</description></item>
    '''   <item><description>General scatter‑plot creation</description></item>
    '''   <item><description>Axis scaling using the Bovey–Wallentin–Bullen–Green algorithm</description></item>
    '''   <item><description>Histogram bin computation (Sturges, Doane, Scott, Freedman–Diaconis)</description></item>
    '''   <item><description>Gaussian overlay curve computation</description></item>
    ''' </list>
    ''' 
    ''' These utilities are used throughout your plotting classes:
    ''' <list type="bullet">
    '''   <item><description><c>Histogram</c></description></item>
    '''   <item><description><c>NormalPlot</c></description></item>
    '''   <item><description><c>BoxPlot</c></description></item>
    '''   <item><description><c>Survival_KM_LR</c></description></item>
    ''' </list>
    ''' </summary>
    Module ChartingFunc

        ''' <summary>
        ''' Returns a predefined RGB color for the <paramref name="i"/>‑th series.
        ''' 
        ''' The function provides a palette of 16 distinct colors suitable for
        ''' multi‑series statistical plots.  
        ''' If <paramref name="i"/> exceeds the predefined range, a fallback color is
        ''' generated using <c>Timer * 60</c>.
        ''' </summary>
        ''' <param name="i">Series index (1‑based).</param>
        ''' <returns>An RGB color value.</returns>
        Public Function GetColor(i As Integer) As Integer

            If i = 1 Then
                GetColor = RGB(255, 0, 0)     'red
            ElseIf i = 2 Then
                GetColor = RGB(0, 0, 255)     'blue
            ElseIf i = 3 Then
                GetColor = RGB(0, 255, 0)     'green
            ElseIf i = 4 Then
                GetColor = RGB(255, 0, 255)   'purple
            ElseIf i = 5 Then
                GetColor = RGB(255, 250, 0)   'yellow
            ElseIf i = 6 Then
                GetColor = RGB(0, 250, 255)   'cyan
            ElseIf i = 7 Then
                GetColor = RGB(189, 183, 107) 'darkkhaki
            ElseIf i = 8 Then
                GetColor = RGB(255, 127, 0)   'darkorange
            ElseIf i = 9 Then
                GetColor = RGB(210, 105, 30)  'chocolate
            ElseIf i = 10 Then
                GetColor = RGB(238, 92, 66)   'tomato
            ElseIf i = 11 Then
                GetColor = RGB(0, 0, 128)     'navy
            ElseIf i = 12 Then
                GetColor = RGB(50, 50, 50)    'dark gray
            ElseIf i = 13 Then
                GetColor = RGB(100, 100, 100) 'medium gray
            ElseIf i = 14 Then
                GetColor = RGB(150, 150, 150) 'medium2 gray
            ElseIf i = 15 Then
                GetColor = RGB(200, 200, 200) 'light gray
            ElseIf i = 16 Then
                GetColor = RGB(0, 0, 0)       'black
            Else
                GetColor = Timer * 60
            End If

        End Function

        ''' <summary>
        ''' Creates a general‑purpose XY scatter plot in Excel for paired numeric data.
        ''' 
        ''' Features:
        ''' <list type="bullet">
        '''   <item><description>Automatic axis scaling using <c>ChartScaling</c></description></item>
        '''   <item><description>Single‑series scatter plot with customizable axis titles</description></item>
        '''   <item><description>Gridline removal and clean formatting</description></item>
        '''   <item><description>Used by regression and diagnostic plots (e.g., Theil–Sen, NormalPlot)</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>ChartScaling</c></description></item>
        '''   <item><description>Excel interop (<c>Worksheet</c>, <c>Chart</c>)</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="arX">X‑coordinates.</param>
        ''' <param name="arY">Y‑coordinates.</param>
        ''' <param name="strYname">Y‑axis label.</param>
        ''' <param name="strXname">X‑axis label.</param>
        ''' <param name="ws">Worksheet where the chart will be created.</param>
        ''' <returns>An Excel <see cref="Microsoft.Office.Interop.Excel.Chart"/> object.</returns>
        Public Function GeneralScatterPlot(arX() As Double, arY() As Double,
                                           strYname As String, strXname As String, ws As Worksheet,
                                           Optional strTitle As String = "Theil-Sen nonparametric regression plot") As Microsoft.Office.Interop.Excel.Chart
            'sub for ploting nonparametric simple linear regression plot of Theil-Sen regression
            'compute optimal scaling
            Dim udPlotAxis As CHARTscale = ChartScaling(arX.Min(), arX.Max())

            With ws.Shapes.AddChart
                With .Chart
                    .ChartType = XlChartType.xlXYScatter

                    'delete extra series
                    Do Until .SeriesCollection.Count = 0
                        .SeriesCollection(1).Delete
                    Loop

                    .Legend.Delete()
                    .Axes(XlAxisType.xlValue).MajorGridlines.Delete
                    .Axes(XlAxisType.xlCategory).MinimumScale = udPlotAxis.Min
                    .Axes(XlAxisType.xlCategory).MaximumScale = udPlotAxis.Max
                    .Axes(XlAxisType.xlCategory).MajorUnit = udPlotAxis.Scale
                    .Axes(XlAxisType.xlValue).CrossesAt = -1.0E+100
                    .Axes(XlAxisType.xlCategory).CrossesAt = -1.0E+100

                    .SeriesCollection.NewSeries
                    With .SeriesCollection(1)
                        .XValues = arX
                        .Values = arY
                        .Name = "Data"
                        .MarkerStyle = 8
                        .MarkerSize = 5
                        .MarkerForegroundColor = RGB(100, 100, 100)
                        .Format.Fill.Visible = False
                    End With

                    On Error Resume Next
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = strYname
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = strXname
                    .HasTitle = False
                    .HasTitle = True
                    .ChartTitle.Text = strTitle
                    On Error GoTo 0
                End With
                Return .Chart
            End With

        End Function

        ''' <summary>
        ''' Computes optimal axis limits and major tick spacing for Excel charts using
        ''' the algorithm from:
        ''' <para>
        ''' Rob Bovey et al., <i>Professional Excel Development</i>, 2nd ed., p. 706.
        ''' </para>
        ''' 
        ''' Steps:
        ''' <list type="number">
        '''   <item><description>Ensures <c>dMin</c> and <c>dMax</c> are ordered and non‑degenerate.</description></item>
        '''   <item><description>Expands the range slightly to avoid boundary clipping.</description></item>
        '''   <item><description>Computes the order of magnitude of the data range.</description></item>
        '''   <item><description>Selects a “nice” major unit (0.2, 0.5, 1.0, 2.0 × 10ᵏ).</description></item>
        '''   <item><description>Rounds axis limits to multiples of the major unit.</description></item>
        ''' </list>
        ''' 
        ''' Returns a <see cref="CHARTscale"/> object containing:
        ''' <list type="bullet">
        '''   <item><description><c>Min</c> — lower axis limit</description></item>
        '''   <item><description><c>Max</c> — upper axis limit</description></item>
        '''   <item><description><c>Scale</c> — major tick interval</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="dMin">Minimum data value.</param>
        ''' <param name="dMax">Maximum data value.</param>
        ''' <returns>A <see cref="CHARTscale"/> object.</returns>
        Function ChartScaling(dMin As Double, dMax As Double) As CHARTscale
            'Function to calculate optimal Chart Axes Scales
            'Taken from:    Rob Bovey, Dennis Wallentin, Stephen Bullen, John Green.:
            '               Professional Excel Development 2nd ed., Addison-Wesley Professional, 2009, page 706
            Dim dPower As Double, dScale As Double
            Dim out = New CHARTscale
            Dim localDMin As Double = dMin
            Dim localDMax As Double = dMax

            'Check if the max and min are the same
            If localDMax = localDMin Then
                dScale = localDMax
                localDMax = localDMax * 1.01
                localDMin = localDMin * 0.99
            End If

            'Check if dMax is bigger than dMin - swap them if not
            If localDMax < localDMin Then
                dScale = localDMax
                localDMax = localDMin
                localDMin = dScale
            End If

            'Make dMax a little bigger and dMin a little smaller
            If localDMax > 0 Then
                localDMax = localDMax + (localDMax - localDMin) * 0.01
            Else
                localDMax = localDMax - (localDMax - localDMin) * 0.01
            End If
            If localDMin > 0 Then
                localDMin = localDMin + (localDMax - localDMin) * 0.01
            Else
                localDMin = localDMin - (localDMax - localDMin) * 0.01
            End If

            'What if they are both 0?
            If localDMax = 0 And localDMin = 0 Then localDMax = 1

            'This bit rounds the maximum and minimum values to reasonable values to chart.
            'Find the range of values covered
            dPower = Math.Log(localDMax - localDMin) / Math.Log(10)
            dScale = 10 ^ (dPower - Int(dPower))

            'Find the scaling factor
            Select Case dScale
                Case 0.0 To 2.5
                    dScale = 0.2
                Case 2.5 To 5.0
                    dScale = 0.5
                Case 5.0 To 7.5
                    dScale = 1.0
                Case Else
                    dScale = 2.0
            End Select

            'Fit the scaling factor (major unit)
            dScale = dScale * 10 ^ Int(dPower)

            'Round the axis values to the nearest scaling factor
            out.Min = dScale * Int(localDMin / dScale)
            out.Max = dScale * (Int(localDMax / dScale) + 1)
            out.Scale = dScale
            Return out
        End Function

        ''' <summary>
        ''' Computes histogram bin midpoints and frequencies using one of four
        ''' standard bin–width selection rules.  
        ''' 
        ''' The function first determines an initial number of bins <c>k</c> or
        ''' bin width <c>h</c> according to the selected rule, and then snaps the
        ''' resulting breakpoints to visually clean values using <c>PrettyBreaks</c>.
        ''' 
        ''' <para><b>Supported rules:</b></para>
        ''' <list type="bullet">
        '''   <item>
        '''     <description>
        '''       <c>(Sturges)</c> — Classical rule for approximately normal data:  
        '''       <para>
        '''         k = 1 + log₂(n)
        '''       </para>
        '''       where <c>n</c> is the sample size.
        '''     </description>
        '''   </item>
        ''' 
        '''   <item>
        '''     <description>
        '''       <c>(Doane)</c> — Skewness‑adjusted Sturges rule.  
        '''       This increases the number of bins when the data exhibit skewness.  
        '''       <para>
        '''         k = 1 + log₂(n) + log₂(1 + |g₁| / σ<sub>g₁</sub>)
        '''       </para>
        '''       where:
        '''       <list type="bullet">
        '''         <item><description><c>g₁</c> = sample skewness (third standardized moment)</description></item>
        '''         <item><description>
        '''           σ<sub>g₁</sub> = sqrt( 6·(n−2) / ((n+1)(n+3)) ),  
        '''           the standard error of skewness
        '''         </description></item>
        '''       </list>
        '''       This implementation uses the population‑moment skewness estimator,
        '''       which converges to the sample estimator for large <c>n</c>.
        '''     </description>
        '''   </item>
        ''' 
        '''   <item>
        '''     <description>
        '''       <c>(Scott)</c> — Optimal for normally distributed data in the
        '''       mean‑squared‑error sense:
        '''       <para>
        '''         h = 3.5·σ / n^(1/3)
        '''       </para>
        '''       where <c>σ</c> is the sample standard deviation.
        '''     </description>
        '''   </item>
        ''' 
        '''   <item>
        '''     <description>
        '''       <c>(Freedman–Diaconis)</c> — Robust rule using the interquartile range:
        '''       <para>
        '''         h = 2·IQR / n^(1/3)
        '''       </para>
        '''       where <c>IQR = Q₃ − Q₁</c>.
        '''     </description>
        '''   </item>
        ''' </list>
        ''' 
        ''' <para>
        ''' After computing <c>k</c> or <c>h</c>, the function generates breakpoints
        ''' and then applies <c>PrettyBreaks</c> to snap the edges to rounded,
        ''' human‑friendly values (similar to R's <c>pretty()</c>).  
        ''' This may slightly adjust the final number of bins.
        ''' </para>
        ''' 
        ''' <para><b>Output:</b></para>
        ''' A 2‑column matrix <c>out(i, j)</c> where:
        ''' <list type="bullet">
        '''   <item><description><c>out(i, 0)</c> = midpoint of bin <c>i</c></description></item>
        '''   <item><description><c>out(i, 1)</c> = frequency count in bin <c>i</c></description></item>
        ''' </list>
        ''' 
        ''' <para><b>External dependencies:</b></para>
        ''' <list type="bullet">
        '''   <item><description><c>Skewness</c> — third standardized moment</description></item>
        '''   <item><description><c>stDev</c> — sample standard deviation</description></item>
        '''   <item><description><c>QuartilesComp</c> — computes Q₁ and Q₃</description></item>
        '''   <item><description><c>PrettyBreaks</c> — R‑style axis snapping</description></item>
        ''' </list>
        ''' </summary>
        ''' 
        ''' <param name="arInput">Numeric input data vector.</param>
        ''' <param name="strType">Binning rule: <c>(Sturges)</c>, <c>(Doane)</c>, <c>(Scott)</c>, or <c>(Freedman‑Diaconis)</c>.</param>
        ''' <returns>
        ''' A 2D array containing histogram bin midpoints and frequencies.
        ''' </returns>
        Public Function HistogramBinsComputation(arInput() As Double, strType As String) As Object(,)

            Dim iNoBins As Integer, sBinSize As Double, i As Integer
            Dim sDenominator As Double, out(,) As Object
            Dim Quartiles As udQuartiles
            Dim nn As Integer = arInput.Length

            'select method used for Bin number determination
            Dim min As Double = arInput.Min()
            Select Case strType
                Case Is = "(Sturges)"
                    iNoBins = Math.Round(1 + Math.Log(nn, 2))
                    sBinSize = (arInput.Max() - min) / iNoBins

                Case Is = "(Doane)"
                    sDenominator = Math.Sqrt(6 * (nn - 2) / ((nn + 1) * (nn + 3)))
                    iNoBins = Math.Round(1 + Math.Log(nn, 2) + Math.Log(1.0 + (Math.Abs(Skewness(arInput)) / sDenominator), 2))
                    sBinSize = (arInput.Max() - min) / iNoBins

                Case Is = "(Scott)"
                    sBinSize = 3.5 * stDev(arInput) / nn ^ (1 / 3)
                    iNoBins = Math.Round((arInput.Max() - min) / sBinSize)

                Case Is = "(Freedman-Diaconis)"
                    'compute quartiles with user defined function
                    Quartiles = QuartilesComp(arInput)

                    sBinSize = 2.0 * (Quartiles.Q3 - Quartiles.Q1) / nn ^ (1 / 3)
                    iNoBins = Math.Round((arInput.Max() - min) / sBinSize)

            End Select


            If iNoBins < 1 Then iNoBins = 1

            'Snap bin edges to "pretty" rounded numbers (R-style pretty())
            Dim dMax As Double = arInput.Max()
            Dim breaks() As Double = PrettyBreaks(min, dMax, iNoBins)

            'Update min/bin size/bin count to the snapped breaks
            min = breaks(0)
            iNoBins = breaks.Length - 1
            If iNoBins < 1 Then iNoBins = 1
            sBinSize = breaks(1) - breaks(0)

            ReDim out(iNoBins - 1, 1)
            'Dim digs As Integer = DecimalsForStep(sBinSize)

            ''Midpoints and initialize frequencies
            'For i = 0 To iNoBins - 1
            '    out(i, 0) = Math.Round((breaks(i) + breaks(i + 1)) / 2.0, digs)
            '    out(i, 1) = 0
            'Next
            Dim digs As Integer = DecimalsForStep(sBinSize)

            ' Midpoints may require more decimals than the bin width (e.g. step=1 => midpoint ends with .5)
            Dim midDigs As Integer = digs
            Dim halfStep As Double = sBinSize / 2.0
            If Math.Abs(halfStep - Math.Round(halfStep, midDigs)) > 0.000000000001 Then
                midDigs += 1
            End If

            'Midpoints and initialize frequencies
            For i = 0 To iNoBins - 1
                out(i, 0) = Math.Round((breaks(i) + breaks(i + 1)) / 2.0, midDigs)
                out(i, 1) = 0
            Next

            'Count frequencies (include the right-most edge in the last bin)
            For i = 0 To nn - 1
                Dim x As Double = arInput(i)
                Dim idx As Integer

                If x >= breaks(breaks.Length - 1) Then
                    idx = iNoBins - 1
                Else
                    idx = CInt(Math.Floor((x - min) / sBinSize))
                    If idx < 0 Then idx = 0
                    If idx > iNoBins - 1 Then idx = iNoBins - 1
                End If

                out(idx, 1) = CInt(out(idx, 1)) + 1
            Next i


            Return out
        End Function

        ''' <summary>
        ''' Returns a "nice" step size close to <paramref name="rawStep"/> using the common 1-2-5-10 rule.
        ''' </summary>
        ''' <param name="rawStep">
        ''' The raw (unrounded) step size, typically computed as (max - min) / targetBins.
        ''' The sign is ignored; the result is always positive.
        ''' </param>
        ''' <returns>
        ''' A positive "nice" step size of the form {1, 2, 5, 10} × 10^k that is greater than or equal to
        ''' the magnitude of <paramref name="rawStep"/> (except for 0, which yields 1).
        ''' </returns>
        ''' <remarks>
        ''' This mirrors the idea used by major statistical tools when producing human-friendly axis ticks
        ''' and histogram breaks. The chosen step is intentionally simple (powers of ten scaled by 1/2/5/10).
        ''' </remarks>
        Private Function NiceStep125(rawStep As Double) As Double
            rawStep = Math.Abs(rawStep)
            If rawStep = 0 Then Return 1.0

            Dim exponent As Double = Math.Floor(Math.Log10(rawStep))
            Dim fraction As Double = rawStep / (10.0 ^ exponent)

            Dim niceFraction As Double
            If fraction <= 1.0 Then
                niceFraction = 1.0
            ElseIf fraction <= 2.0 Then
                niceFraction = 2.0
            ElseIf fraction <= 5.0 Then
                niceFraction = 5.0
            Else
                niceFraction = 10.0
            End If

            Return niceFraction * (10.0 ^ exponent)
        End Function

        ''' <summary>
        ''' Determines a reasonable number of decimal digits for rounding values that lie on a given step size.
        ''' </summary>
        ''' <param name="stepSize">The step size used for breaks (bin width). The sign is ignored.</param>
        ''' <returns>
        ''' The number of decimal digits to use with <see cref="Math.Round(Double, Integer)"/>.
        ''' For steps &gt;= 1 returns 0; for steps like 0.1 returns 1; for 0.01 returns 2; etc.
        ''' </returns>
        ''' <remarks>
        ''' This is a pragmatic helper to keep computed breaks and midpoints visually clean.
        ''' It assumes the step is a decimal-friendly number produced by <see cref="NiceStep125(Double)"/>.
        ''' </remarks>
        Private Function DecimalsForStep(stepSize As Double) As Integer
            stepSize = Math.Abs(stepSize)
            If stepSize = 0 Then Return 0
            Dim exp As Integer = CInt(Math.Floor(Math.Log10(stepSize)))
            If exp >= 0 Then
                Return 0
            Else
                'e.g. step=0.01 -> exp=-2 -> 2 decimals
                Return -exp
            End If
        End Function

        ''' <summary>
        ''' Computes "pretty" histogram breakpoints that expand <paramref name="dMin"/> and <paramref name="dMax"/>
        ''' to clean rounded edges and use a human-friendly step size.
        ''' </summary>
        ''' <param name="dMin">Minimum data value (may be greater than <paramref name="dMax"/>; values will be swapped).</param>
        ''' <param name="dMax">Maximum data value.</param>
        ''' <param name="targetBins">Requested (approximate) number of bins. The returned number of bins may differ.</param>
        ''' <returns>
        ''' An array of breakpoints of length (numberOfBins + 1). The first element is the snapped minimum edge and the last
        ''' element is the snapped maximum edge. Consecutive differences define the bin width.
        ''' </returns>
        ''' <remarks>
        ''' The algorithm:
        ''' <list type="number">
        '''   <item><description>Compute a raw step ≈ (dMax - dMin) / targetBins.</description></item>
        '''   <item><description>Round the step to a "nice" value via the 1-2-5-10 rule.</description></item>
        '''   <item><description>Expand the min/max outward to multiples of the nice step.</description></item>
        '''   <item><description>Generate equally spaced breaks between the expanded bounds.</description></item>
        ''' </list>
        ''' This is similar in spirit to R's approach where a target bin count is computed first (e.g., Sturges/Scott/FD),
        ''' then breaks are snapped to "pretty" values for readability.
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' Dim br = PrettyBreaks(2.13, 9.92, 7)
        ''' ' Might yield breaks like: 2, 3, 4, 5, 6, 7, 8, 9, 10 (step = 1)
        ''' </code>
        ''' </example>
        Private Function PrettyBreaks(dMin As Double, dMax As Double, targetBins As Integer) As Double()
            If targetBins < 1 Then targetBins = 1
            If Double.IsNaN(dMin) OrElse Double.IsNaN(dMax) Then
                Return New Double() {0.0, 1.0}
            End If

            ' Ensure min <= max
            If dMax < dMin Then
                Dim tmp As Double = dMin : dMin = dMax : dMax = tmp
            End If

            Dim niceStep As Double
            Dim digs As Integer

            If dMax = dMin Then
                ' Degenerate range: build a tiny symmetric range
                niceStep = NiceStep125(Math.Abs(dMin))
                If niceStep = 0 Then niceStep = 1.0
                digs = DecimalsForStep(niceStep)
                Return New Double() {Math.Round(dMin - niceStep, digs), Math.Round(dMax + niceStep, digs)}
            End If

            Dim rawStep As Double = (dMax - dMin) / CDbl(targetBins)
            niceStep = NiceStep125(rawStep)
            If niceStep = 0 Then niceStep = 1.0

            Dim graphMin As Double = Math.Floor(dMin / niceStep) * niceStep
            Dim graphMax As Double = Math.Ceiling(dMax / niceStep) * niceStep

            ' Ensure at least 2 breaks (1 bin)
            Dim nBins As Integer = CInt(Math.Round((graphMax - graphMin) / niceStep))
            If nBins < 1 Then nBins = 1

            digs = DecimalsForStep(niceStep)
            Dim breaks(nBins) As Double
            For i As Integer = 0 To nBins
                breaks(i) = Math.Round(graphMin + i * niceStep, digs)
            Next

            Return breaks
        End Function



        ''' <summary>
        ''' Computes a Gaussian overlay curve for a histogram using robust estimates of
        ''' mean and standard deviation based on quartiles:
        ''' <code>
        ''' mean ≈ (Q1 + Q3) / 2  
        ''' sd   ≈ (Q3 − Q1) / 1.34898
        ''' </code>
        ''' 
        ''' The function generates 100 points spanning the data range and returns:
        ''' <list type="bullet">
        '''   <item><description>Column 0: X‑values</description></item>
        '''   <item><description>Column 1: Scaled Gaussian density values</description></item>
        ''' </list>
        ''' 
        ''' The scaling factor <c>n × binWidth</c> ensures the curve is comparable to
        ''' histogram frequencies.
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>QuartilesComp</c></description></item>
        '''   <item><description><c>DNorm</c> — normal density function</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="arData">Raw data values.</param>
        ''' <param name="arBinMidVal">Histogram bin midpoints.</param>
        ''' <returns>A 100×2 matrix of Gaussian overlay coordinates.</returns>
        Function GaussOverlayComputation(arData() As Double, arBinMidVal() As Double) As Double(,)

            Dim dBinSize As Double
            Dim out(99, 1) As Double, arNormInv(99) As Double
            Dim n As Integer = arData.Length
            Dim Quartiles As udQuartiles = QuartilesComp(arData)
            Dim dMean As Double = (Quartiles.Q3 + Quartiles.Q1) / 2.0
            'because gaussian dist has Q1 and Q2 at mean +/- 0.67449 so denominator is 2 * this value
            Dim dSD As Double = (Quartiles.Q3 - Quartiles.Q1) / 1.34898

            Dim mids = arBinMidVal.Distinct().OrderBy(Function(x) x).ToArray()
            If mids.Length >= 2 Then
                dBinSize = mids(1) - mids(0)
            Else
                dBinSize = 1.0
            End If
            'If UBound(arBinMidVal) >= 1 Then dBinSize = Math.Abs(arBinMidVal(0) - arBinMidVal(1))

            'calculate 100 values for superimposed gaussian curve
            Dim dMin As Double = arData.Min()
            Dim dStep As Double = (arData.Max() - dMin) / 99.0
            For i = 0 To 99
                out(i, 0) = dMin + (dStep * i)
                arNormInv(i) = distributions.DNorm(out(i, 0), dMean, dSD)
                out(i, 1) = arNormInv(i) * n * dBinSize
            Next

            Return out
        End Function

    End Module

End Namespace
