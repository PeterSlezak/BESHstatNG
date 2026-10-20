Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq

''' <summary>
''' Portable histogram binning and Gaussian-overlay calculations formerly housed in
''' the Windows ChartingFunc module. Return array shapes and legacy bin rules are preserved.
''' These helpers intentionally retain the existing input-array mutation by QuartilesComp
''' for Freedman-Diaconis binning and Gaussian overlays.
''' </summary>
Public NotInheritable Class HistogramBinningCore
    Private Sub New()
    End Sub

        Public Shared Function HistogramBinsComputation(arInput() As Double, strType As String) As Object(,)

        Dim iNoBins As Integer, sBinSize As Double, i As Integer
        Dim sDenominator As Double, out(,) As Object
        Dim Quartiles As udQuartiles
        Dim nn As Integer = arInput.Length

        'select method used for Bin number determination
        Dim min As Double = arInput.Min()
        Select Case strType
            Case Is = "(Sturges)"
                iNoBins = Convert.ToInt32(Math.Ceiling(1 + Math.Log(nn, 2)))
                sBinSize = (arInput.Max() - min) / iNoBins

            Case Is = "(Doane)"
                sDenominator = Math.Sqrt(6 * (nn - 2) / ((nn + 1) * (nn + 3)))
                iNoBins = Convert.ToInt32(Math.Ceiling(1 + Math.Log(nn, 2) + Math.Log(1.0 + (Math.Abs(StatFunc.Skewness(arInput)) / sDenominator), 2)))
                sBinSize = (arInput.Max() - min) / iNoBins

            Case Is = "(Scott)"
                sBinSize = 3.5 * StatFunc.stDev(arInput) / Math.Pow(nn, 1.0R / 3.0R)
                iNoBins = Convert.ToInt32(Math.Ceiling((arInput.Max() - min) / sBinSize))

            Case Is = "(Freedman-Diaconis)"
                'compute quartiles with user defined function
                Quartiles = StatFunc.QuartilesComp(arInput)

                sBinSize = 2.0 * (Quartiles.Q3 - Quartiles.Q1) / Math.Pow(nn, 1.0R / 3.0R)
                iNoBins = Convert.ToInt32(Math.Ceiling((arInput.Max() - min) / sBinSize))
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
                idx = Convert.ToInt32(Math.Floor((x - min) / sBinSize))
                If idx < 0 Then idx = 0
                If idx > iNoBins - 1 Then idx = iNoBins - 1
            End If

            out(idx, 1) = Convert.ToInt32(out(idx, 1)) + 1
        Next

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
    Private Shared Function NiceStep125(rawStep As Double) As Double
            rawStep = Math.Abs(rawStep)
            If rawStep = 0 Then Return 1.0

            Dim exponent As Double = Math.Floor(Math.Log10(rawStep))
            Dim fraction As Double = rawStep / (Math.Pow(10.0R, exponent))

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

            Return niceFraction * (Math.Pow(10.0R, exponent))
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
        Private Shared Function DecimalsForStep(stepSize As Double) As Integer
            stepSize = Math.Abs(stepSize)
            If stepSize = 0 Then Return 0
            Dim exp As Integer = Convert.ToInt32(Math.Floor(Math.Log10(stepSize)))
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
        Private Shared Function PrettyBreaks(dMin As Double, dMax As Double, targetBins As Integer) As Double()
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

            Dim rawStep As Double = (dMax - dMin) / Convert.ToDouble(targetBins)
            niceStep = NiceStep125(rawStep)
            If niceStep = 0 Then niceStep = 1.0

            Dim graphMin As Double = Math.Floor(dMin / niceStep) * niceStep
            Dim graphMax As Double = Math.Ceiling(dMax / niceStep) * niceStep

            ' Ensure at least 2 breaks (1 bin)
            Dim nBins As Integer = Convert.ToInt32(Math.Round((graphMax - graphMin) / niceStep))
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
        Public Shared Function GaussOverlayComputation(arData() As Double, arBinMidVal() As Double) As Double(,)

            Dim dBinSize As Double
            Dim out(99, 1) As Double, arNormInv(99) As Double
            Dim n As Integer = arData.Length
            Dim Quartiles As udQuartiles = StatFunc.QuartilesComp(arData)
            Dim dMean As Double = (Quartiles.Q3 + Quartiles.Q1) / 2.0
            'because gaussian dist has Q1 and Q2 at mean +/- 0.67449 so denominator is 2 * this value
            Dim dSD As Double = (Quartiles.Q3 - Quartiles.Q1) / 1.34898

            Dim mids = arBinMidVal.Distinct().OrderBy(Function(x) x).ToArray()
            If mids.Length >= 2 Then
                dBinSize = mids(1) - mids(0)
            Else
                dBinSize = 1.0
            End If

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

End Class
