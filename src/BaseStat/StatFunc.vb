Option Explicit On
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.Office.Interop.Excel

Public Class udQuartiles
    Public Q1 As Double
    Public Median As Double
    Public Q3 As Double
End Class



Public Module StatFunc

    ''' <summary>
    ''' Computes log(y!) in a numerically stable way. For small y (0–20),
    ''' exact precomputed values are used. For larger y, a Stirling-based
    ''' approximation with correction terms is applied. This matches the
    ''' numerical behavior of R's lfactorial().
    ''' </summary>
    ''' <param name="y">A non-negative integer.</param>
    ''' <returns>The natural logarithm of y! (log-factorial).</returns>
    ''' <remarks>
    ''' <para>
    ''' Direct computation of factorials quickly overflows even for moderate
    ''' values (e.g., 20! ≈ 2.4e18). This function avoids overflow by computing
    ''' log(y!) directly.
    ''' </para>
    ''' 
    ''' <para>
    ''' For y ≤ 20, exact values are returned from a lookup table.
    ''' For y > 20, the following approximation is used:
    ''' 
    '''   log(y!) ≈ y*log(y) - y + 0.5*log(2πy) + 1/(12y) - 1/(360y³)
    ''' 
    ''' which is accurate to machine precision for all practical y.
    ''' </para>
    ''' </remarks>
    Public Function LogFactorial(y As Integer) As Double
        If y < 0 Then Return Double.NaN

        ' Exact values for 0–20
        Dim exact() As Double = {
        0.0,
        0.0,
        0.69314718055994529,
        1.791759469228055,
        3.1780538303479458,
        4.7874917427820458,
        6.5792512120101012,
        8.5251613610654147,
        10.604602902745251,
        12.801827480081471,
        15.104412573075519,
        17.502307845873887,
        19.987214495661888,
        22.552163853123425,
        25.191221182738683,
        27.899271383840894,
        30.671860106080675,
        33.505073450136891,
        36.395445208033053,
        39.339884187199495,
        42.335616460753485
    }

        If y <= 20 Then
            Return exact(y)
        End If

        ' Stirling approximation with correction terms
        Dim yD As Double = CDbl(y)
        Return yD * Math.Log(yD) - yD +
           0.5 * Math.Log(2 * Math.PI * yD) +
           1.0 / (12 * yD) -
           1.0 / (360 * yD * yD * yD)
    End Function

    ''' <summary>
    ''' Returns the smallest value from a variable-length list of inputs,
    ''' using generic comparison semantics. This method provides a flexible,
    ''' type-safe alternative to Math.Min by accepting any type that implements
    ''' <see cref="IComparable(Of T)"/> and by supporting an arbitrary number
    ''' of arguments through a <c>ParamArray</c>.
    ''' </summary>
    ''' 
    ''' <typeparam name="T">
    ''' A type that implements <see cref="IComparable(Of T)"/>. This allows the
    ''' function to determine ordering for numeric types, dates, strings, and
    ''' any user-defined types with a natural ordering.
    ''' </typeparam>
    ''' 
    ''' <param name="values">
    ''' A variable-length list of values from which the minimum is selected.
    ''' At least one value must be supplied. If no values are provided, an
    ''' <see cref="ArgumentException"/> is thrown.
    ''' </param>
    ''' 
    ''' <returns>
    ''' The smallest element in <paramref name="values"/> according to the
    ''' type's <see cref="IComparable(Of T).CompareTo"/> implementation.
    ''' </returns>
    ''' 
    ''' <remarks>
    ''' <para>
    ''' This function generalizes the behavior of <c>Math.Min</c> by allowing:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>**[More than two arguments](guide://action?prefill=Tell%20me%20more%20about%3A%20More%20than%20two%20arguments)** — any number of inputs may be supplied.</description></item>
    '''   <item><description>**[Any comparable type](guide://action?prefill=Tell%20me%20more%20about%3A%20Any%20comparable%20type)** — not limited to numeric primitives.</description></item>
    '''   <item><description>**[Custom ordering rules](guide://action?prefill=Tell%20me%20more%20about%3A%20Custom%20ordering%20rules)** — user-defined types can control comparison logic.</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' The algorithm performs a simple linear scan:
    ''' </para>
    ''' 
    ''' <code>
    ''' m = values(0)
    ''' For each element v in values:
    '''     If v &lt; m Then m = v
    ''' Next
    ''' </code>
    ''' 
    ''' <para>
    ''' This ensures:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>**[O(n) time complexity](guide://action?prefill=Tell%20me%20more%20about%3A%20O(n)%20time%20complexity)** — optimal for unsorted input.</description></item>
    '''   <item><description>**[Stable behavior](guide://action?prefill=Tell%20me%20more%20about%3A%20Stable%20behavior)** — the first occurrence of the minimum is returned.</description></item>
    '''   <item><description>**[No allocations beyond the ParamArray](guide://action?prefill=Tell%20me%20more%20about%3A%20No%20allocations%20beyond%20the%20ParamArray)** — efficient for tight loops.</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' <b>Examples:</b>
    ''' </para>
    ''' 
    ''' <code>
    ''' Minimum(5, 2, 9, -3)        ' returns -3
    ''' Minimum("pear", "apple")    ' returns "apple" (lexicographic)
    ''' Minimum(Date1, Date2, Date3)
    ''' </code>
    ''' 
    ''' <para>
    ''' This helper is especially useful in statistical code where minimum
    ''' extraction is required for:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>**[Range calculations](guide://action?prefill=Tell%20me%20more%20about%3A%20Range%20calculations)**</description></item>
    '''   <item><description>**[Robust parameter scanning](guide://action?prefill=Tell%20me%20more%20about%3A%20Robust%20parameter%20scanning)**</description></item>
    '''   <item><description>**[Generic numeric algorithms](guide://action?prefill=Tell%20me%20more%20about%3A%20Generic%20numeric%20algorithms)**</description></item>
    ''' </list>
    ''' 
    ''' </remarks>
    Public Function Minimum(Of T As IComparable(Of T))(ParamArray values() As T) As T
        If values Is Nothing OrElse values.Length = 0 Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("At least one value is required."))
        End If

        Dim m As T = values(0)
        For i As Integer = 1 To values.Length - 1
            If values(i).CompareTo(m) < 0 Then m = values(i)
        Next

        Return m
    End Function


    ''' <summary>
    ''' Computes the two-tailed F-test p-value comparing the variances of two samples,
    ''' matching the behavior of Excel's F.TEST function.
    ''' </summary>
    ''' <param name="array1">
    ''' The first sample of numeric values.
    ''' </param>
    ''' <param name="array2">
    ''' The second sample of numeric values.
    ''' </param>
    ''' <returns>
    ''' The two-tailed p-value for the F-test of equal variances, equivalent to Excel's F.TEST.
    ''' Returns <see cref="Double.NaN"/> for invalid inputs.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' Excel's F.TEST computes:
    ''' 
    '''     F = var1 / var2
    ''' 
    ''' where var1 and var2 are sample variances. The p-value is:
    ''' 
    '''     p = 2 * min( CDF(F), 1 - CDF(F) )
    ''' 
    ''' using the F-distribution with df1 = n1 - 1 and df2 = n2 - 1.
    ''' </para>
    ''' 
    ''' <para>
    ''' Requirements:
    ''' <list type="bullet">
    '''   <item><description>Each array must contain at least 2 values.</description></item>
    '''   <item><description>Variances must be non-zero.</description></item>
    '''   <item><description>Uses the F-distribution CDF for p-value calculation.</description></item>
    ''' </list>
    ''' </para>
    ''' </remarks>
    Public Function FTest(array1() As Double, array2() As Double) As Double
        If array1 Is Nothing OrElse array2 Is Nothing Then Return Double.NaN
        If array1.Length < 2 OrElse array2.Length < 2 Then Return Double.NaN

        Dim n1 As Integer = array1.Length
        Dim n2 As Integer = array2.Length
        Dim var1 As Double = variance(array1)
        Dim var2 As Double = variance(array2)

        If var1 = 0 OrElse var2 = 0 Then Return Double.NaN

        ' Compute F statistic
        Dim F As Double = var1 / var2
        Dim df1 As Integer = n1 - 1
        Dim df2 As Integer = n2 - 1

        ' Compute CDF of F-distribution
        Dim cdf As Double = distributions.F_CDF(F, df1, df2)

        ' Excel uses two-tailed p-value:
        Dim pRight As Double = 1 - cdf
        Dim pLeft As Double = cdf
        Dim p As Double = 2.0 * Math.Min(pLeft, pRight)

        ' Clamp to [0,1]
        If p < 0 Then p = 0
        If p > 1 Then p = 1

        Return p
    End Function


    ''' <summary>
    ''' Computes the exclusive percentile of a numeric data set, matching the
    ''' behavior of Excel's PERCENTILE.EXC function.
    ''' </summary>
    ''' <param name="data">
    ''' The array of numeric values from which the percentile is calculated.
    ''' </param>
    ''' <param name="k">
    ''' The percentile value between 0 and 1 (exclusive). Excel requires
    ''' 0 &lt; k &lt; 1 for PERCENTILE.EXC.
    ''' </param>
    ''' <returns>
    ''' The k-th exclusive percentile of the data, equivalent to Excel's
    ''' PERCENTILE.EXC. Returns <see cref="Double.NaN"/> for invalid inputs.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' Excel's PERCENTILE.EXC uses the exclusive percentile definition:
    ''' 
    '''     h = (n + 1) * k
    ''' 
    ''' where n is the number of data points. If h is an integer, the value at
    ''' that rank is returned. Otherwise, linear interpolation is performed
    ''' between the surrounding ranked values.
    ''' </para>
    ''' 
    ''' <para>
    ''' Requirements:
    ''' <list type="bullet">
    '''   <item><description>Data array must contain at least 2 values.</description></item>
    '''   <item><description>k must satisfy 0 &lt; k &lt; 1.</description></item>
    '''   <item><description>Returns NaN if k is outside the valid range.</description></item>
    ''' </list>
    ''' </para>
    ''' </remarks>
    Public Function Percentile_Exc(data() As Double, k As Double) As Double
        If data Is Nothing OrElse data.Length < 2 Then Return Double.NaN
        If k <= 0 OrElse k >= 1 Then Return Double.NaN

        ' Sort the data (Excel sorts ascending)
        Dim sorted = CType(data.Clone(), Double())
        Array.Sort(sorted)

        Dim n As Integer = sorted.Length
        Dim h As Double = (n + 1) * k

        ' Excel uses 1-based indexing for rank positions
        Dim hFloor As Integer = CInt(Math.Floor(h))
        Dim hCeil As Integer = CInt(Math.Ceiling(h))

        ' If h is an integer, return that element
        If hFloor = hCeil Then
            Dim idx As Integer = hFloor - 1
            If idx >= 0 AndAlso idx < n Then
                Return sorted(idx)
            Else
                Return Double.NaN
            End If
        End If

        ' Linear interpolation between surrounding ranks
        Dim lowerIndex As Integer = hFloor - 1
        Dim upperIndex As Integer = hCeil - 1

        If lowerIndex < 0 OrElse upperIndex >= n Then
            Return Double.NaN
        End If

        Dim lowerValue As Double = sorted(lowerIndex)
        Dim upperValue As Double = sorted(upperIndex)

        Dim fraction As Double = h - hFloor

        Return lowerValue + fraction * (upperValue - lowerValue)
    End Function


    ''' <summary>
    ''' Computes the number of combinations of <paramref name="n"/> items taken
    ''' <paramref name="k"/> at a time, matching the behavior of Excel's COMBIN function.
    ''' </summary>
    ''' <param name="n">
    ''' The total number of items. Excel truncates this value to an integer and
    ''' requires it to be non-negative.
    ''' </param>
    ''' <param name="k">
    ''' The number of items to choose. Excel truncates this value to an integer and
    ''' requires it to be non-negative.
    ''' </param>
    ''' <returns>
    ''' The number of combinations (n choose k), equivalent to Excel's COMBIN.
    ''' Returns 0 if <paramref name="k"/> is greater than <paramref name="n"/>.
    ''' Returns <see cref="Double.NaN"/> if either argument is negative.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' Excel's COMBIN function computes:
    ''' 
    '''     COMBIN(n, k) = n! / (k! * (n - k)!)
    ''' 
    ''' after truncating both arguments to integers.
    ''' </para>
    ''' 
    ''' <para>
    ''' This implementation uses a multiplicative formula that avoids overflow
    ''' and maintains full double-precision accuracy for all values supported by Excel.
    ''' </para>
    ''' 
    ''' <para>
    ''' Special cases:
    ''' <list type="bullet">
    '''   <item><description>If n &lt; 0 or k &lt; 0 → NaN</description></item>
    '''   <item><description>If k &gt; n → 0</description></item>
    '''   <item><description>If k = 0 or k = n → 1</description></item>
    ''' </list>
    ''' </para>
    ''' </remarks>
    Public Function Combin(n As Double, k As Double) As Double
        ' Excel truncates inputs to integers
        Dim nn As Integer = CInt(Math.Floor(n))
        Dim kk As Integer = CInt(Math.Floor(k))

        If nn < 0 OrElse kk < 0 Then Return Double.NaN
        If kk > nn Then Return 0
        If kk = 0 OrElse kk = nn Then Return 1

        ' Use symmetry: C(n, k) = C(n, n-k)
        If kk > nn \ 2 Then kk = nn - kk

        ' Multiplicative formula to avoid overflow:
        ' C(n, k) = product(i=1..k) (n - k + i) / i
        Dim result As Double = 1.0

        For i As Integer = 1 To kk
            result *= (nn - kk + i) / i
        Next

        Return result
    End Function

    ''' <summary>
    ''' Computes the natural logarithm of the binomial coefficient C(n, k)
    ''' using a numerically stable log‑space formulation. This method is
    ''' mathematically equivalent to Excel's COMBIN(n, k) but avoids overflow
    ''' and loss of precision by working entirely in log-space. The algorithm
    ''' matches the numerical behavior of R's <c>lchoose()</c> function.
    ''' </summary>
    ''' 
    ''' <param name="n">
    ''' The total number of items. Must be a non-negative integer.
    ''' </param>
    ''' 
    ''' <param name="k">
    ''' The number of selected items. Must satisfy 0 ≤ k ≤ n.
    ''' </param>
    ''' 
    ''' <returns>
    ''' The natural logarithm of the binomial coefficient C(n, k). Returns
    ''' <see cref="Double.NegativeInfinity"/> when k is outside the valid
    ''' range, consistent with log(0).
    ''' </returns>
    ''' 
    ''' <remarks>
    ''' <para>
    ''' The binomial coefficient is defined as:
    ''' 
    '''     C(n, k) = n! / (k! (n − k)!)
    ''' 
    ''' Direct computation using factorials is numerically unstable for even
    ''' moderate values of n (e.g., n ≥ 60), because factorials grow faster
    ''' than floating‑point numbers can represent. Excel's COMBIN silently
    ''' overflows to <c>Infinity</c> for large n, while R avoids this by
    ''' computing the logarithm of the coefficient directly.
    ''' </para>
    ''' 
    ''' <para>
    ''' This implementation uses the stable summation identity:
    ''' 
    '''     log(C(n, k)) = Σ[i = 1..k] log(n − k + i) − log(i)
    ''' 
    ''' which avoids factorials entirely. The summation is symmetric in k and
    ''' n − k, so the algorithm first applies:
    ''' 
    '''     k = min(k, n − k)
    ''' 
    ''' to reduce the number of iterations and improve numerical stability.
    ''' </para>
    ''' 
    ''' <para>
    ''' <b>Why log-space?</b><br/>
    ''' Computing C(n, k) directly and then taking log(C(n, k)) is unsafe:
    ''' <list type="bullet">
    '''   <item><description>For n ≥ 60, C(n, k) may overflow to +∞.</description></item>
    '''   <item><description>For extreme k, C(n, k) may underflow to 0.</description></item>
    '''   <item><description>Intermediate factorials (n!, k!) exceed 1E300 quickly.</description></item>
    '''   <item><description>Loss of precision occurs when subtracting large logs.</description></item>
    ''' </list>
    ''' 
    ''' Working directly in log-space avoids all of these issues and matches
    ''' the numerical behavior of R's <c>lchoose()</c>, which is the gold
    ''' standard for statistical computing.
    ''' </para>
    ''' 
    ''' <para>
    ''' <b>Special cases:</b>
    ''' <list type="bullet">
    '''   <item><description>If k &lt; 0 or k &gt; n, the result is log(0) = −∞.</description></item>
    '''   <item><description>If k = 0 or k = n, the result is log(1) = 0.</description></item>
    '''   <item><description>If n = 0, the only valid k is 0, returning 0.</description></item>
    ''' </list>
    ''' </para>
    ''' 
    ''' <para>
    ''' <b>Example:</b><br/>
    ''' For n = 10, k = 4:
    ''' 
    '''     C(10, 4) = 210
    '''     log(C(10, 4)) = log(210) ≈ 5.34710753071747
    ''' 
    ''' This function returns the same value as:
    ''' 
    '''     Excel:  LOG(COMBIN(10, 4))
    '''     R:      lchoose(10, 4)
    ''' </para>
    ''' 
    ''' <para>
    ''' <b>Usage:</b><br/>
    ''' This function is intended for use in:
    ''' <list type="bullet">
    '''   <item><description>Binomial PMF and CDF calculations</description></item>
    '''   <item><description>Likelihood computations</description></item>
    '''   <item><description>Log‑probability models</description></item>
    '''   <item><description>Any statistical routine requiring stable combinatorics</description></item>
    ''' </list>
    ''' 
    ''' It is especially important when computing probabilities for large n,
    ''' where direct combinatorial evaluation would overflow or lose precision.
    ''' </para>
    ''' </remarks>
    Public Function LogCombin(n As Integer, k As Integer) As Double
        If k < 0 OrElse k > n Then Return Double.NegativeInfinity
        If k = 0 OrElse k = n Then Return 0.0

        ' Use symmetry to reduce computation
        If k > n \ 2 Then k = n - k

        Dim sum As Double = 0.0
        For i As Integer = 1 To k
            sum += Math.Log(n - k + i) - Math.Log(i)
        Next

        Return sum
    End Function


    ''' <summary>
    ''' Computes the Pearson correlation coefficient between two numeric arrays,
    ''' matching the behavior of Excel's CORREL function.
    ''' </summary>
    ''' <param name="x">
    ''' The first data array (X values). Must be the same length as <paramref name="y"/>.
    ''' </param>
    ''' <param name="y">
    ''' The second data array (Y values). Must be the same length as <paramref name="x"/>.
    ''' </param>
    ''' <returns>
    ''' The Pearson correlation coefficient between <paramref name="x"/> and
    ''' <paramref name="y"/>, equivalent to Excel's CORREL function.
    ''' Returns <see cref="Double.NaN"/> if the variance of either array is zero.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The correlation coefficient is computed using the standard formula:
    ''' 
    '''     r = Σ[(x - mean(x)) * (y - mean(y))] /
    '''         sqrt( Σ[(x - mean(x))²] * Σ[(y - mean(y))²] )
    ''' 
    ''' This matches Excel's CORREL exactly for numeric arrays without missing values.
    ''' </para>
    ''' 
    ''' <para>
    ''' Throws an exception if the input arrays differ in length or are empty.
    ''' Returns <see cref="Double.NaN"/> if either array has zero variance,
    ''' consistent with Excel's behavior.
    ''' </para>
    ''' </remarks>
    Public Function Correl(x() As Double, y() As Double) As Double
        If x Is Nothing OrElse y Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentNullException())
        If x.Length <> y.Length Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Arrays must have the same length."))
        If x.Length = 0 Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Arrays must not be empty."))

        Dim n As Integer = x.Length
        Dim meanX As Double = x.Average()
        Dim meanY As Double = y.Average()
        Dim sumXY As Double = 0
        Dim sumXX As Double = 0
        Dim sumYY As Double = 0

        For i As Integer = 0 To n - 1
            Dim dx As Double = x(i) - meanX
            Dim dy As Double = y(i) - meanY

            sumXY += dx * dy
            sumXX += dx * dx
            sumYY += dy * dy
        Next

        If sumXX = 0 OrElse sumYY = 0 Then Return Double.NaN   ' Excel returns #DIV/0! → NaN is the closest .NET equivalent

        Return sumXY / Math.Sqrt(sumXX * sumYY)
    End Function


    ''' <summary>
    ''' Converts an angle measured in degrees to radians, matching the behavior
    ''' of Excel's RADIANS function.
    ''' </summary>
    ''' <param name="degrees">
    ''' The angle in degrees to be converted.
    ''' </param>
    ''' <returns>
    ''' The angle converted to radians, equivalent to Excel's RADIANS(degrees).
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' Excel's RADIANS function performs a simple unit conversion:
    ''' 
    '''     radians = degrees * π / 180
    ''' 
    ''' This implementation uses <see cref="Math.PI"/> for full double‑precision
    ''' accuracy and matches Excel's output exactly.
    ''' </para>
    ''' 
    ''' <para>
    ''' Examples:
    ''' <code>
    ''' Radians(180) = 3.14159265358979
    ''' Radians(90)  = 1.5707963267949
    ''' Radians(45)  = 0.785398163397448
    ''' </code>
    ''' </para>
    ''' </remarks>
    Public Function Radians(degrees As Double) As Double
        Return degrees * (Math.PI / 180.0)
    End Function


    ''' <summary>
    ''' Computes the inverse hyperbolic tangent of a number, matching the behavior
    ''' of Excel's ATANH function. This implementation is compatible with .NET
    ''' Framework versions that do not provide Math.Atanh.
    ''' </summary>
    ''' <param name="number">
    ''' The numeric value for which the inverse hyperbolic tangent is to be computed.
    ''' Must lie strictly within the open interval (-1, 1).
    ''' </param>
    ''' <returns>
    ''' The inverse hyperbolic tangent of <paramref name="number"/>, equivalent to
    ''' Excel's ATANH for all valid inputs. Returns <see cref="Double.NaN"/> if
    ''' the input is outside the interval (-1, 1).
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The inverse hyperbolic tangent is defined as:
    ''' 
    '''     atanh(x) = 0.5 * ln((1 + x) / (1 - x))
    ''' 
    ''' and is only defined for |x| &lt; 1.
    ''' </para>
    ''' 
    ''' <para>
    ''' This implementation uses the logarithmic definition directly instead of
    ''' relying on <c>Math.Atanh</c>, making it suitable for .NET Framework  (Core/5+)
    ''' versions that do not expose that method.
    ''' </para>
    ''' 
    ''' <para>
    ''' For inputs with |x| ≥ 1, this function returns <see cref="Double.NaN"/>,
    ''' similar to Excel returning a numeric error.
    ''' </para>
    ''' </remarks>
    Public Function Atanh(number As Double) As Double
        If number <= -1.0 OrElse number >= 1.0 Then
            Return Double.NaN
        End If

        ' atanh(x) = 0.5 * ln((1 + x) / (1 - x))
        Return 0.5 * Math.Log((1.0 + number) / (1.0 - number))
    End Function


    ''' <summary>
    ''' Computes the intercept of the linear regression line (least squares fit)
    ''' matching the behavior of Excel's INTERCEPT(y, x) function.
    ''' </summary>
    ''' <param name="y">
    ''' The dependent variable values (Y data). Must be the same length as <paramref name="x"/>.
    ''' </param>
    ''' <param name="x">
    ''' The independent variable values (X data). Must be the same length as <paramref name="y"/>.
    ''' </param>
    ''' <returns>
    ''' The intercept of the least-squares regression line, equivalent to Excel's INTERCEPT.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The intercept is computed using:
    ''' 
    '''     intercept = mean(y) - slope * mean(x)
    ''' 
    ''' where <c>slope</c> is computed using the same formula as Excel's SLOPE.
    ''' </para>
    ''' 
    ''' <para>
    ''' Throws an exception if the input arrays differ in length or if the variance
    ''' of <paramref name="x"/> is zero.
    ''' </para>
    ''' </remarks>
    Public Function Intercept(y() As Double, x() As Double) As Double
        Dim slp As Double = Slope(y, x)
        Dim meanX As Double = x.Average()
        Dim meanY As Double = y.Average()

        Return meanY - slp * meanX
    End Function


    ''' <summary>
    ''' Computes the slope of the linear regression line (least squares fit)
    ''' matching the behavior of Excel's SLOPE(y, x) function.
    ''' </summary>
    ''' <param name="y">
    ''' The dependent variable values (Y data). Must be the same length as <paramref name="x"/>.
    ''' </param>
    ''' <param name="x">
    ''' The independent variable values (X data). Must be the same length as <paramref name="y"/>.
    ''' </param>
    ''' <returns>
    ''' The slope of the least-squares regression line, equivalent to Excel's SLOPE.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The slope is computed using the standard least-squares formula:
    ''' 
    '''     slope = Σ[(x - mean(x)) * (y - mean(y))] / Σ[(x - mean(x))²]
    ''' 
    ''' This matches Excel's SLOPE exactly for numeric arrays without missing values.
    ''' </para>
    ''' 
    ''' <para>
    ''' Throws an exception if the input arrays differ in length or if the variance
    ''' of <paramref name="x"/> is zero.
    ''' </para>
    ''' </remarks>
    Public Function Slope(y() As Double, x() As Double) As Double
        If y Is Nothing OrElse x Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentNullException())
        If y.Length <> x.Length Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Arrays must have the same length."))
        If y.Length = 0 Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Arrays must not be empty."))

        Dim n As Integer = y.Length

        Dim meanX As Double = x.Average()
        Dim meanY As Double = y.Average()

        Dim num As Double = 0
        Dim den As Double = 0

        For i As Integer = 0 To n - 1
            Dim dx As Double = x(i) - meanX
            num += dx * (y(i) - meanY)
            den += dx * dx
        Next

        If den = 0 Then BESHstatGlobals.BSerr.LogAndThrow(New DivideByZeroException("Variance of X is zero."))

        Return num / den
    End Function

    ''' <summary>
    ''' Mimics Excel's ROUNDDOWN function by rounding a number toward zero
    ''' to a specified number of digits.
    ''' </summary>
    ''' <param name="number">
    ''' The numeric value to be rounded.
    ''' </param>
    ''' <param name="digits">
    ''' The number of digits to round to. Positive values round to decimal
    ''' places, zero rounds to the nearest integer, and negative values
    ''' round to tens, hundreds, thousands, etc.
    ''' </param>
    ''' <returns>
    ''' The value of <paramref name="number"/> rounded toward zero to the
    ''' specified number of digits, matching the behavior of Excel's ROUNDDOWN.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This method reproduces the exact semantics of Excel's ROUNDDOWN:
    ''' it always rounds toward zero, regardless of the sign of the input.
    ''' </para>
    ''' 
    ''' <para>
    ''' Examples:
    ''' <code>
    ''' RoundDown(3.14159, 2)   = 3.14
    ''' RoundDown(-3.14159, 2)  = -3.14
    ''' RoundDown(1234.56, -2)  = 1200
    ''' RoundDown(-1234.56, -2) = -1200
    ''' </code>
    ''' </para>
    ''' 
    ''' <para>
    ''' Internally, the function scales the input by 10^digits, applies
    ''' <see cref="Math.Floor"/> or <see cref="Math.Ceiling"/> depending on the
    ''' sign, and rescales the result.
    ''' </para>
    ''' </remarks>
    Public Function RoundDown(number As Double, Optional digits As Integer = 0) As Double
        Dim factor As Double = Math.Pow(10, digits)

        ' Scale the number
        Dim scaled As Double = number * factor

        ' Excel ROUNDDOWN always moves toward zero
        If scaled > 0 Then
            scaled = Math.Floor(scaled)
        Else
            scaled = Math.Ceiling(scaled)
        End If

        ' Rescale back
        Return scaled / factor
    End Function

    ''' <summary>
    ''' Mimics Excel's ROUNDUP function by rounding a number away from zero
    ''' to a specified number of digits.
    ''' </summary>
    ''' <param name="number">
    ''' The numeric value to be rounded.
    ''' </param>
    ''' <param name="digits">
    ''' The number of digits to round to. Positive values round to decimal
    ''' places, zero rounds to the nearest integer, and negative values
    ''' round to tens, hundreds, thousands, etc.
    ''' </param>
    ''' <returns>
    ''' The value of <paramref name="number"/> rounded away from zero to the
    ''' specified number of digits, matching the behavior of Excel's ROUNDUP.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This method reproduces the exact semantics of Excel's ROUNDUP:
    ''' it always rounds away from zero, regardless of the sign of the input.
    ''' </para>
    ''' 
    ''' <para>
    ''' Examples:
    ''' <code>
    ''' RoundUp(3.14159, 2)   = 3.15
    ''' RoundUp(-3.14159, 2)  = -3.15
    ''' RoundUp(1234.56, -2)  = 1300
    ''' RoundUp(-1234.56, -2) = -1300
    ''' </code>
    ''' </para>
    ''' 
    ''' <para>
    ''' Internally, the function scales the input by 10^digits, applies
    ''' <see cref="Math.Ceiling"/> or <see cref="Math.Floor"/> depending on the
    ''' sign, and rescales the result.
    ''' </para>
    ''' </remarks>
    Public Function RoundUp(number As Double, Optional digits As Integer = 0) As Double
        Dim factor As Double = Math.Pow(10, digits)

        ' Scale the number
        Dim scaled As Double = number * factor

        ' Excel ROUNDUP always moves away from zero
        If scaled > 0 Then
            scaled = Math.Ceiling(scaled)
        Else
            scaled = Math.Floor(scaled)
        End If

        ' Rescale back
        Return scaled / factor
    End Function


    ''' <summary>
    ''' Computes the sum of all elements in a two‑dimensional array of numeric values.
    ''' </summary>
    ''' <typeparam name="T">
    ''' A value type that implements <see cref="IConvertible"/>, allowing safe
    ''' conversion to <see cref="Double"/> for accumulation.
    ''' </typeparam>
    ''' <param name="source">
    ''' The 2D array whose elements are to be summed. The array may contain any
    ''' numeric type (Integer, Double, Decimal, Single, etc.) as long as it
    ''' implements <see cref="IConvertible"/>.
    ''' </param>
    ''' <returns>
    ''' The total sum of all elements in the array, returned as a <see cref="Double"/>.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This method provides a convenient and type‑agnostic way to sum the contents
    ''' of a rectangular 2D array. Since .NET's built‑in LINQ <c>Sum()</c> extension
    ''' methods do not operate on multidimensional arrays, this helper fills that gap
    ''' with a simple and efficient nested‑loop implementation.
    ''' </para>
    ''' 
    ''' <para>
    ''' Each element is converted to <see cref="Double"/> using
    ''' <see cref="Convert.ToDouble(Object)"/>. This ensures consistent numerical
    ''' behavior across all supported numeric types, but extremely large integer or
    ''' decimal values may lose precision when converted to double.
    ''' </para>
    ''' 
    ''' <para>
    ''' The method performs no allocations and runs in O(n·m) time, where n and m are
    ''' the dimensions of the array.
    ''' </para>
    ''' </remarks>
    <Extension>
    Public Function Sum2D(Of T As IConvertible)(source As T(,)) As Double
        Dim total As Double = 0
        For i = 0 To source.GetLength(0) - 1
            For j = 0 To source.GetLength(1) - 1
                total += Convert.ToDouble(source(i, j))
            Next
        Next
        Return total
    End Function

    ''' <summary>
    ''' Computes the arithmetic average of all elements in a two‑dimensional
    ''' array of numeric values.
    ''' </summary>
    ''' <typeparam name="T">
    ''' A value type that implements <see cref="IConvertible"/>, allowing safe
    ''' conversion to <see cref="Double"/> for accumulation.
    ''' </typeparam>
    ''' <param name="source">
    ''' The 2D array whose elements are to be averaged. The array may contain any
    ''' numeric type (Integer, Double, Decimal, Single, etc.) as long as it
    ''' implements <see cref="IConvertible"/>.
    ''' </param>
    ''' <returns>
    ''' The arithmetic mean of all elements in the array, returned as a
    ''' <see cref="Double"/>. Returns <c>NaN</c> if the array has zero length.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This method provides a convenient and type‑agnostic way to compute the
    ''' average of a rectangular 2D array. Since .NET's built‑in LINQ
    ''' <c>Average()</c> extension methods do not operate on multidimensional
    ''' arrays, this helper fills that gap with a simple and efficient nested‑loop
    ''' implementation.
    ''' </para>
    ''' 
    ''' <para>
    ''' Each element is converted to <see cref="Double"/> using
    ''' <see cref="Convert.ToDouble(Object)"/>. This ensures consistent numerical
    ''' behavior across all supported numeric types, but extremely large integer or
    ''' decimal values may lose precision when converted to double.
    ''' </para>
    ''' 
    ''' <para>
    ''' The method performs no allocations and runs in O(n·m) time, where n and m
    ''' are the dimensions of the array.
    ''' </para>
    ''' </remarks>
    <Extension>
    Public Function Average2D(Of T As IConvertible)(source As T(,)) As Double
        Dim rows As Integer = source.GetLength(0)
        Dim cols As Integer = source.GetLength(1)
        Dim count As Integer = rows * cols

        If count = 0 Then Return Double.NaN

        Dim total As Double = 0
        For i = 0 To rows - 1
            For j = 0 To cols - 1
                total += Convert.ToDouble(source(i, j))
            Next
        Next

        Return total / count
    End Function

    ''' <summary>
    ''' Computes the minimum value in a two‑dimensional array of Double.
    ''' This function scans all elements in row‑major order and returns the
    ''' smallest numeric value encountered.
    ''' </summary>
    ''' <param name="x">
    ''' A two‑dimensional array of Double values. The array must contain at
    ''' least one element; otherwise an <see cref="ArgumentException"/> is thrown.
    ''' </param>
    ''' <returns>
    ''' The minimum value contained in <paramref name="x"/>.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This function behaves similarly to Excel's MIN across a rectangular
    ''' range and is intended as the counterpart to Average2D.
    ''' </para>
    ''' 
    ''' <para>
    ''' The algorithm performs a single pass through the array:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>**O(n·m) time complexity** for an n×m array.</description></item>
    '''   <item><description>**No additional allocations** beyond loop variables.</description></item>
    '''   <item><description>**Stable for all finite Double values**, including negatives.</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' This helper is useful for:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>**Range checks** in statistical routines.</description></item>
    '''   <item><description>**Diagnostics** for EM or NR iterations.</description></item>
    '''   <item><description>**MatrixType preprocessing** before normalization.</description></item>
    ''' </list>
    ''' </remarks>
    Public Function Minimum2D(x(,) As Double) As Double
        If x Is Nothing OrElse x.Length = 0 Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Array must contain at least one element."))
        End If

        Dim r As Integer = x.GetLength(0)
        Dim c As Integer = x.GetLength(1)

        Dim m As Double = x(0, 0)

        For i As Integer = 0 To r - 1
            For j As Integer = 0 To c - 1
                If x(i, j) < m Then m = x(i, j)
            Next
        Next

        Return m
    End Function

    ''' <summary>
    ''' Computes the maximum value in a two‑dimensional array of Double.
    ''' This function scans all elements in row‑major order and returns the
    ''' largest numeric value encountered.
    ''' </summary>
    ''' <param name="x">
    ''' A two‑dimensional array of Double values. The array must contain at
    ''' least one element; otherwise an <see cref="ArgumentException"/> is thrown.
    ''' </param>
    ''' <returns>
    ''' The maximum value contained in <paramref name="x"/>.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This function behaves similarly to Excel's MAX across a rectangular
    ''' range and is intended as the counterpart to Minimum2D and Average2D.
    ''' </para>
    ''' 
    ''' <para>
    ''' The algorithm performs a single pass through the array:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>**[O(n·m) time complexity](guide://action?prefill=Tell%20me%20more%20about%3A%20O(n%C2%B7m)%20time%20complexity)** for an n×m array.</description></item>
    '''   <item><description>**[No additional allocations](guide://action?prefill=Tell%20me%20more%20about%3A%20No%20additional%20allocations)** beyond loop variables.</description></item>
    '''   <item><description>**[Stable for all finite Double values](guide://action?prefill=Tell%20me%20more%20about%3A%20Stable%20for%20all%20finite%20Double%20values)**, including negatives.</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' This helper is useful for:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>**[Range checks](guide://action?prefill=Tell%20me%20more%20about%3A%20Range%20checks)** in statistical routines.</description></item>
    '''   <item><description>**[Diagnostics](guide://action?prefill=Tell%20me%20more%20about%3A%20Diagnostics)** for EM or NR iterations.</description></item>
    '''   <item><description>**[MatrixType preprocessing](guide://action?prefill=Tell%20me%20more%20about%3A%20Matrix%20preprocessing)** before normalization.</description></item>
    ''' </list>
    ''' </remarks>
    Public Function Maximum2D(x(,) As Double) As Double
        If x Is Nothing OrElse x.Length = 0 Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Array must contain at least one element."))
        End If

        Dim r As Integer = x.GetLength(0)
        Dim c As Integer = x.GetLength(1)

        Dim m As Double = x(0, 0)

        For i As Integer = 0 To r - 1
            For j As Integer = 0 To c - 1
                If x(i, j) > m Then m = x(i, j)
            Next
        Next

        Return m
    End Function


    ''' <summary>
    ''' Computes the nearest positive (semi-)definite covariance matrix to the input
    ''' by converting to a correlation matrix, clipping eigenvalues, and restoring
    ''' the original variances. The diagonal of the covariance matrix is preserved.
    ''' </summary>
    ''' <param name="cov">
    ''' The input covariance matrix. Must be square and symmetric.
    ''' </param>
    ''' <param name="threshold">
    ''' Minimum allowed eigenvalue when clipping.  
    ''' Eigenvalues smaller than this value are replaced by <paramref name="threshold"/>.
    ''' Defaults to 1e‑15.
    ''' </param>
    ''' <returns>
    ''' A covariance matrix that is positive (semi-)definite and has the same diagonal
    ''' (variances) as <paramref name="cov"/>.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The algorithm proceeds as follows:
    ''' </para>
    ''' <list type="number">
    '''   <item><description>Convert covariance to correlation using <see cref="cov2corr"/>.</description></item>
    '''   <item><description>Clip eigenvalues of the correlation matrix using <see cref="corrClipped"/>.</description></item>
    '''   <item><description>Convert the clipped correlation matrix back to covariance using <see cref="corr2cov"/>.</description></item>
    ''' </list>
    ''' <para>
    ''' This method is fast and ensures numerical stability while preserving variances.
    ''' </para>
    ''' </remarks>
    Function CovNearest(cov(,) As Double, Optional threshold As Double = 0.000000000000001) As Double(,)
        Dim std() As Double = Nothing
        Dim corr(,) As Double = cov2corr(cov, std)              ' convert to correlation
        Dim corrFixed(,) As Double = corrClipped(corr, threshold) ' clip correlation
        Return corr2cov(corrFixed, std)                         ' back to covariance (preserve variances)
    End Function


    ''' <summary>
    ''' Converts a correlation matrix into a covariance matrix using the supplied
    ''' vector of standard deviations.
    ''' </summary>
    ''' <param name="corr">
    ''' A correlation matrix. Must be square with ones on the diagonal.
    ''' </param>
    ''' <param name="std">
    ''' A vector of standard deviations corresponding to each variable.
    ''' </param>
    ''' <returns>
    ''' A covariance matrix computed as:
    ''' <code>
    ''' cov(i, j) = corr(i, j) * std(i) * std(j)
    ''' </code>
    ''' </returns>
    ''' <remarks>
    ''' Internally computes the outer product of <paramref name="std"/> with itself
    ''' and multiplies element‑wise with <paramref name="corr"/>.
    ''' </remarks>
    Function corr2cov(corr(,) As Double, std() As Double) As Double(,)
        'convert correlation matrix to covariance matrix

        Dim cov(UBound(corr), UBound(corr, 2)) As Double
        Dim std2(,) As Double = Matrix.M_OUTERPRODUCT(std, std)
        For i = 0 To UBound(corr)
            For j = 0 To UBound(corr, 2)
                cov(i, j) = corr(i, j) * std2(i, j)
            Next
        Next
        Return cov
    End Function

    ''' <summary>
    ''' Computes a positive semi-definite approximation of a correlation matrix by
    ''' clipping eigenvalues below a threshold and renormalizing the result so that
    ''' the diagonal elements equal one.
    ''' </summary>
    ''' <param name="corr">
    ''' The input correlation matrix. Must be square and symmetric.
    ''' </param>
    ''' <param name="threshold">
    ''' Minimum allowed eigenvalue. Eigenvalues smaller than this value are replaced
    ''' by <paramref name="threshold"/>. Defaults to 1e‑15.
    ''' </param>
    ''' <returns>
    ''' A correlation matrix that is positive semi-definite.  
    ''' If no eigenvalues are clipped, the original matrix is returned unchanged.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This method is faster than full nearest‑correlation algorithms because it
    ''' performs only a single eigenvalue decomposition.
    ''' </para>
    ''' <para>
    ''' After clipping, the matrix is rescaled using <see cref="cov2corr"/> to ensure
    ''' unit diagonal.
    ''' </para>
    ''' </remarks>
    Function corrClipped(corr(,) As Double, Optional threshold As Double = 0.000000000000001) As Double(,)
        Dim xStd() As Double
        Dim bClipped As Boolean = False
        Dim xNew(,) As Double = clipEvals(corr, bClipped, threshold) 'bClipped is result

        If Not bClipped Then
            corrClipped = corr
        Else
            ReDim xStd(UBound(corr))
            For i = 0 To UBound(corr)
                xStd(i) = Math.Sqrt(corr(i, i))
            Next i
            corrClipped = cov2corr(xNew, xStd)
        End If
    End Function

    ''' <summary>
    ''' Clips the eigenvalues of a symmetric matrix by replacing values smaller than
    ''' <paramref name="value"/> with <paramref name="value"/> and reconstructs the
    ''' matrix using the original eigenvectors.
    ''' </summary>
    ''' <param name="corr">
    ''' The input symmetric matrix (typically a correlation matrix).
    ''' </param>
    ''' <param name="bClipped">
    ''' Output flag indicating whether any eigenvalues were clipped.
    ''' </param>
    ''' <param name="value">
    ''' Minimum allowed eigenvalue. Defaults to 0.
    ''' </param>
    ''' <returns>
    ''' A matrix reconstructed from the clipped eigenvalues and original eigenvectors.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' Uses <see cref="matrix.EIGEN_JK"/> to obtain eigenvalues and eigenvectors.  
    ''' The first column contains eigenvalues; remaining columns contain eigenvectors.
    ''' </para>
    ''' <para>
    ''' The reconstructed matrix is:
    ''' </para>
    ''' <code>
    ''' M = V * diag(max(eigenvalues, value)) * Vᵀ
    ''' </code>
    ''' </remarks>
    Function clipEvals(corr(,) As Double, ByRef bClipped As Boolean, Optional value As Double = 0#) As Double(,)
        Dim ei = Matrix.EIGEN_JK(corr)

        Dim n As Integer = UBound(corr, 1)
        Dim evecs(,) As Double = ei.Item2
        Dim evals() As Double = ei.Item1
        Dim maxs(n) As Double

        ' Compute signed eigenvalues using Rayleigh quotient: v^T * A * v
        For j = 0 To n
            Dim lam As Double = 0#
            For r = 0 To n
                Dim tmp As Double = 0#
                For c = 0 To n
                    tmp += corr(r, c) * evecs(c, j)
                Next
                lam += evecs(r, j) * tmp
            Next
            evals(j) = lam
        Next

        ' Clip
        bClipped = False
        For j = 0 To n
            If evals(j) < value Then
                bClipped = True
                maxs(j) = value
            Else
                maxs(j) = evals(j)
            End If
        Next

        ' Reconstruct: V * diag(maxs) * V^T  (scale columns by eigenvalues)
        Dim tmpM(n, n) As Double
        For i = 0 To n
            For j = 0 To n
                tmpM(i, j) = evecs(i, j) * maxs(j)
            Next
        Next

        Return Matrix.MatrixMult(tmpM, Matrix.trans(evecs))
    End Function


    ''' <summary>
    ''' Converts a covariance matrix into a correlation matrix and returns the
    ''' corresponding vector of standard deviations.
    ''' </summary>
    ''' <param name="cov">
    ''' The input covariance matrix. Must be square and symmetric.
    ''' </param>
    ''' <param name="std">
    ''' Output vector of standard deviations, computed as the square root of the
    ''' diagonal elements of <paramref name="cov"/>.
    ''' </param>
    ''' <returns>
    ''' A correlation matrix computed as:
    ''' <code>
    ''' corr(i, j) = cov(i, j) / (std(i) * std(j))
    ''' </code>
    ''' </returns>
    ''' <remarks>
    ''' Internally computes the outer product of <paramref name="std"/> with itself
    ''' and divides <paramref name="cov"/> element‑wise by this matrix.
    ''' </remarks>
    Function cov2corr(cov(,) As Double, ByRef std() As Double) As Double(,)
        'convert covariance matrix to correlation matrix
        'std is an output
        ReDim std(UBound(cov, 1))
        For i = 0 To UBound(cov)
            std(i) = Math.Sqrt(cov(i, i))
        Next
        Dim std2(,) As Double = Matrix.M_OUTERPRODUCT(std, std)
        Return Matrix.M_DIV(cov, std2)
    End Function

    ''' <summary>
    ''' Computes the digamma function ψ(x), the logarithmic derivative of the Gamma function.
    ''' </summary>
    ''' <param name="x">
    ''' The input value for which ψ(x) is evaluated.
    ''' Note: The digamma function has poles at non-positive integers.
    ''' </param>
    ''' <returns>
    ''' The value of the digamma function ψ(x) at the specified input.
    ''' </returns>
    ''' <remarks>
    ''' Uses an asymptotic expansion for large arguments and recurrence relations to reduce smaller x.
    ''' See the "Asymptotic expansion" section in 
    ''' https://en.wikipedia.org/wiki/Digamma_function for details.
    ''' </remarks>
    ''' <example>
    ''' Dim result As Double = digamma(5.0)
    ''' ' result ≈ 1.506 (since ψ(5) = H₄ - γ, where H₄ is the 4th harmonic number)
    ''' </example>
    Public Function digamma(x As Double) As Double
        Dim z As Double, d As Double

        If x >= 3 Then
            z = x + 1
            d = Math.Log(z) - 1 / (2 * z) - 1 / (12 * z ^ 2) + 1 / (120 * z ^ 4) - 1 / (252 * z ^ 6) + 1 / (240 * z ^ 8) - 1 / (132 * z ^ 10) + 691 / (32760 * z ^ 12) - 1 / (12 * z ^ 14)
            digamma = d - 1 / x
        ElseIf x >= 2 Then
            z = x + 2
            d = Math.Log(z) - 1 / (2 * z) - 1 / (12 * z ^ 2) + 1 / (120 * z ^ 4) - 1 / (252 * z ^ 6) + 1 / (240 * z ^ 8) - 1 / (132 * z ^ 10) + 691 / (32760 * z ^ 12) - 1 / (12 * z ^ 14)
            digamma = d - 1 / x - 1 / (x + 1)
        ElseIf x >= 1 Then
            z = x + 3
            d = Math.Log(z) - 1 / (2 * z) - 1 / (12 * z ^ 2) + 1 / (120 * z ^ 4) - 1 / (252 * z ^ 6) + 1 / (240 * z ^ 8) - 1 / (132 * z ^ 10) + 691 / (32760 * z ^ 12) - 1 / (12 * z ^ 14)
            digamma = d - 1 / x - 1 / (x + 1) - 1 / (x + 2)
        Else
            z = x + 4
            d = Math.Log(z) - 1 / (2 * z) - 1 / (12 * z ^ 2) + 1 / (120 * z ^ 4) - 1 / (252 * z ^ 6) + 1 / (240 * z ^ 8) - 1 / (132 * z ^ 10) + 691 / (32760 * z ^ 12) - 1 / (12 * z ^ 14)
            digamma = d - 1 / x - 1 / (x + 1) - 1 / (x + 2) - 1 / (x + 3)
        End If
    End Function


    ''' <summary>
    ''' Computes the trigamma function ψ₁(x), i.e., the first derivative of the digamma function.
    ''' </summary>
    ''' <param name="x">
    ''' Input value at which to evaluate ψ₁(x). Must be positive.
    ''' </param>
    ''' <returns>
    ''' The trigamma value ψ₁(x) for x &gt; 0. Returns <c>Double.NaN</c> for non-positive or NaN input.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The trigamma function is defined as the derivative of digamma:
    ''' ψ₁(x) = d/dx ψ(x).
    ''' It can also be expressed as the series ψ₁(x) = Σₖ₌₀^∞ 1 / (x + k)² for x &gt; 0.
    ''' </para>
    ''' <para>
    ''' Implementation details:
    ''' - Uses the recurrence ψ₁(x) = ψ₁(x + 1) + 1/x² to shift small x upward until x ≥ 8,
    '''   which improves accuracy of the asymptotic approximation.
    ''' - Evaluates an asymptotic expansion in inverse powers of x (Bernoulli-number series),
    '''   providing near double-precision accuracy for typical statistical inputs.
    ''' </para>
    ''' <para>
    ''' Edge cases:
    ''' - x ≤ 0 → NaN
    ''' - NaN → NaN
    ''' - For large x, ψ₁(x) → 0.
    ''' </para>
    ''' <example>
    ''' Example usage:
    ''' <code>
    ''' Dim t1 As Double = trigamma(1)     ' ~ 1.644934066848226 (π²/6)
    ''' Dim t2 As Double = trigamma(0.5)   ' ~ 4.934802200544679
    ''' Dim t3 As Double = trigamma(10)    ' ~ 0.105166335681686
    ''' </code>
    ''' </example>
    ''' </remarks>
    Public Function trigamma(x As Double) As Double
        If Double.IsNaN(x) Then Return Double.NaN
        If x <= 0# Then Return Double.NaN  ' keep consistent with your current behavior if different

        Dim acc As Double = 0#

        ' Recurrence: ψ1(x) = ψ1(x+1) + 1/x^2
        ' Shift upward until asymptotic series is very accurate
        While x < 8.0#
            acc += 1.0# / (x * x)
            x += 1.0#
        End While

        ' Asymptotic expansion for large x:
        ' ψ1(x) ~ 1/x + 1/(2x^2) + 1/(6x^3) - 1/(30x^5) + 1/(42x^7) - 1/(30x^9)
        '        + 5/(66x^11) - 691/(2730x^13) + 7/(6x^15) - 3617/(510x^17) + ...
        Dim inv As Double = 1.0 / x
        Dim inv2 As Double = inv * inv

        Dim inv2_2 As Double = inv2 * inv2        ' 1/x^4
        Dim inv2_3 As Double = inv2_2 * inv2      ' 1/x^6
        Dim inv2_4 As Double = inv2_3 * inv2      ' 1/x^8
        Dim inv2_5 As Double = inv2_4 * inv2      ' 1/x^10
        Dim inv2_6 As Double = inv2_5 * inv2      ' 1/x^12
        Dim inv2_7 As Double = inv2_6 * inv2      ' 1/x^14
        Dim inv2_8 As Double = inv2_7 * inv2      ' 1/x^16

        Dim series As Double =
        inv + 0.5 * inv2 + (inv2 * inv) / 6.0 _
        - (inv2_2 * inv) / 30.0 _
        + (inv2_3 * inv) / 42.0 _
        - (inv2_4 * inv) / 30.0 _
        + (5.0 * inv2_5 * inv) / 66.0 _
        - (691.0 * inv2_6 * inv) / 2730.0 _
        + (7.0 * inv2_7 * inv) / 6.0 _
        - (3617.0 * inv2_8 * inv) / 510.0

        Return acc + series
    End Function



    ''' <summary>
    ''' Computes the natural logarithm of the Gamma function using the Lanczos approximation.
    ''' </summary>
    ''' <param name="z">
    ''' The input value for which ln(Gamma(z)) is evaluated.
    ''' Must not be a non-positive integer (Gamma has poles there).
    ''' </param>
    ''' <returns>
    ''' The natural logarithm of the Gamma function at the specified value.
    ''' </returns>
    ''' <remarks>
    ''' Uses g=7, n=9 Lanczos coefficients. For z .lt. 0.5, applies the reflection formula
    ''' to improve numerical stability. For z ≥ 0.5, applies the standard Lanczos approximation.
    ''' </remarks>
    Public Function LogGamma(z As Double) As Double
        ' Lanczos coefficients, g=7, n=9
        Dim p() As Double = {0.99999999999980993,
                             676.5203681218851,
                             -1259.1392167224028,
                             771.32342877765313,
                             -176.61502916214059,
                             12.507343278686905,
                             -0.13857109526572012,
                             0.0000099843695780195716,
                             0.00000015056327351493116}

        If z <= 0 AndAlso z = Math.Floor(z) Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Gamma function is undefined for non-positive integers."))
        End If

        If z < 0.5 Then
            ' Reflection formula
            Return Math.Log(Math.PI) - Math.Log(Math.Sin(Math.PI * z)) - LogGamma(1.0 - z)
        End If

        z -= 1.0
        Dim x As Double = p(0)
        For i As Integer = 1 To p.Length - 1
            x += p(i) / (z + i)
        Next

        Dim t As Double = z + 7.5
        Return 0.5 * Math.Log(2.0 * Math.PI) + (z + 0.5) * Math.Log(t) - t + Math.Log(x)
    End Function

    ''' <summary>
    ''' Computes the regularized lower incomplete gamma function P(a, x),
    ''' defined as γ(a, x) / Γ(a), where γ(a, x) is the lower incomplete gamma integral.
    ''' This function is a core building block of the chi-square CDF.
    ''' </summary>
    ''' <param name="a">
    ''' Shape parameter of the gamma function. Must be strictly positive.
    ''' </param>
    ''' <param name="x">
    ''' Upper limit of integration. Must be non-negative.
    ''' </param>
    ''' <returns>
    ''' The regularized lower incomplete gamma value P(a, x), satisfying:
    ''' 0 ≤ P(a, x) ≤ 1.
    ''' Returns NaN if inputs are invalid.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' <b>Mathematical definition:</b><br/>
    ''' The regularized lower incomplete gamma function is defined as:
    ''' P(a, x) = γ(a, x) / Γ(a)
    ''' where γ(a, x) = ∫₀ˣ t^(a−1) e^(−t) dt.
    ''' </para>
    ''' 
    ''' <para>
    ''' <b>Numerical method:</b><br/>
    ''' This implementation follows the classical approach used in R and AS239:
    ''' <list type="bullet">
    '''   <item><description>For x &lt; a + 1: a rapidly convergent <b>series expansion</b> is used.</description></item>
    '''   <item><description>For x ≥ a + 1: a stable <b>continued fraction expansion</b> (Lentz’s method) is used.</description></item>
    ''' </list>
    ''' This split ensures numerical stability across the entire domain.
    ''' </para>
    ''' 
    ''' <para>
    ''' <b>Relation to chi-square distribution:</b><br/>
    ''' The chi-square CDF is computed as:
    ''' P(X ≤ x) = P(df/2, x/2)
    ''' Therefore, this function is the mathematical core of <c>ChiSquareCDF</c>.
    ''' </para>
    ''' 
    ''' <para>
    ''' <b>Relation to R:</b><br/>
    ''' - R computes the chi-square CDF using <c>pgamma(x/2, df/2)</c>.<br/>
    ''' - This function matches R's <c>pgamma</c> behavior to approximately 1e-14.
    ''' </para>
    ''' 
    ''' <para>
    ''' <b>Relation to Excel:</b><br/>
    ''' Excel does not expose the incomplete gamma function directly, but uses it internally for:<br/>
    ''' - <c>CHISQ.DIST(x, df, TRUE)</c> (lower tail)<br/>
    ''' - <c>CHISQ.DIST.RT(x, df)</c> (upper tail)<br/>
    ''' Excel’s implementation is less precise in extreme tails; this function matches R instead.
    ''' </para>
    ''' 
    ''' <para>
    ''' <b>Edge cases:</b>
    ''' <list type="bullet">
    '''   <item><description>If x = 0, returns 0.</description></item>
    '''   <item><description>If x → ∞, returns 1.</description></item>
    '''   <item><description>If a ≤ 0 or x &lt; 0, returns NaN.</description></item>
    '''   <item><description>Handles extremely small or large x using stable logarithmic forms.</description></item>
    ''' </list>
    ''' </para>
    ''' 
    ''' <example>
    ''' Example usage:
    ''' <code>
    ''' ' Compute P(a, x) for gamma distribution
    ''' Dim g1 = LowerIncompleteGamma(2, 3)     ' ~0.8008517
    ''' 
    ''' ' Chi-square CDF example: df = 4, x = 6
    ''' ' P(X ≤ 6) = P(4/2, 6/2) = P(2, 3)
    ''' Dim chi = LowerIncompleteGamma(2, 3)     ' matches R pchisq(6, 4)
    ''' </code>
    ''' </example>
    ''' </remarks>
    Public Function LowerIncompleteGamma(a As Double, x As Double) As Double
        If Double.IsNaN(a) OrElse Double.IsNaN(x) Then Return Double.NaN
        If x < 0 OrElse a <= 0 Then Return Double.NaN
        If x = 0 Then Return 0
        If Double.IsInfinity(x) Then Return 1.0

        Dim logPref As Double = -x + a * Math.Log(x) - LogGamma(a)

        ' If logPref is huge positive, exp(logPref) would overflow: clamp to 1
        If logPref > 709.0 Then
            ' This only happens in pathological numeric regions; return a bounded result.
            ' For lower regularized gamma P(a,x), when exp factor would overflow,
            ' we fall back based on relative position of x vs a.
            Return If(x >= a, 1.0, 0.0)
        End If

        If x < a + 1 Then
            ' Series expansion for P(a,x)
            Dim sum As Double = 1.0 / a
            Dim term As Double = sum
            Dim n As Integer = 1
            Dim maxIter As Integer = 200000

            While Math.Abs(term) > 0.000000000000001 AndAlso n < maxIter
                term *= x / (a + n)
                sum += term
                n += 1
                If Double.IsNaN(sum) OrElse Double.IsInfinity(sum) Then Exit While
            End While

            Dim res As Double = sum * Math.Exp(logPref)
            If Double.IsNaN(res) Then Return Double.NaN
            If res < 0.0 Then Return 0.0
            If res > 1.0 Then Return 1.0
            Return res
        Else
            ' Continued fraction for Q(a,x), then P = 1 - Q
            Dim b As Double = x + 1 - a
            Dim c As Double = 1 / 1.0E-30
            Dim d As Double = 1 / b
            Dim h As Double = d
            Dim i As Integer = 1
            Dim maxIter As Integer = 100000

            While i < maxIter
                Dim an As Double = -CDbl(i) * (CDbl(i) - a)
                b += 2.0

                d = an * d + b
                If Math.Abs(d) < 1.0E-30 Then d = 1.0E-30

                c = b + an / c
                If Math.Abs(c) < 1.0E-30 Then c = 1.0E-30

                d = 1.0 / d
                Dim delta As Double = d * c

                If Double.IsNaN(delta) OrElse Double.IsInfinity(delta) Then Exit While

                h *= delta

                If Double.IsNaN(h) OrElse Double.IsInfinity(h) Then Exit While
                If Math.Abs(delta - 1.0) < 0.00000000000001 Then Exit While

                i += 1
            End While

            ' If CF failed, fall back safely (avoid throwing/overflowing)
            If Double.IsNaN(h) OrElse Double.IsInfinity(h) OrElse i >= maxIter Then
                ' For large x, P(a,x) ~ 1
                If x > a Then Return 1.0
                ' Otherwise return bounded NaN-safe approximation
                Return Double.NaN
            End If

            Dim q As Double = h * Math.Exp(logPref)   ' Q(a,x)
            Dim p As Double = 1.0 - q                 ' P(a,x)

            If Double.IsNaN(p) Then Return Double.NaN
            If p < 0.0 Then Return 0.0
            If p > 1.0 Then Return 1.0
            Return p
        End If
    End Function



    ''' <summary>
    ''' Computes the skewness of a dataset using the standard moment-based definition.
    ''' </summary>
    ''' <param name="arInput">
    ''' An array of numeric values for which skewness is to be calculated.
    ''' </param>
    ''' <returns>
    ''' The skewness value. Returns 0 if both the second and third central moments are zero.
    ''' </returns>
    ''' <remarks>
    ''' Skewness is computed using normalized central moments:
    ''' <para>m₂ = E[(x - μ)²]</para>
    ''' <para>m₃ = E[(x - μ)³]</para>
    ''' <para>Skewness = m₃ / (m₂^(3/2))</para>
    ''' Reference definitions:
    ''' <list type="bullet">
    '''   <item><description>NIST: https://www.itl.nist.gov/div898/handbook/eda/section3/eda35b.htm</description></item>
    '''   <item><description>StatsDirect: https://www.statsdirect.com/help/#basic_descriptive_statistics/univariate_summary.htm</description></item>
    ''' </list>
    ''' </remarks>
    Function Skewness(arInput() As Double) As Double
        Dim m2 As Double, m3 As Double

        Dim Mean As Double = arInput.Average()
        Dim n As Integer = arInput.Length

        For i = 0 To n - 1
            m2 += ((arInput(i) - Mean) ^ 2) / n
            m3 += ((arInput(i) - Mean) ^ 3) / n
        Next

        If m3 = 0 And m2 = 0 Then
            Return 0
        Else
            Return m3 / m2 ^ 1.5
        End If
    End Function

    ''' <summary>
    ''' Computes the kurtosis of a dataset using the standard moment-based definition.
    ''' </summary>
    ''' <param name="arInput">
    ''' The array of numeric values for which kurtosis is to be calculated.
    ''' </param>
    ''' <returns>
    ''' The kurtosis value. Returns 0 if both the second and fourth central moments are zero.
    ''' </returns>
    ''' <remarks>
    ''' Kurtosis is computed using normalized central moments:
    ''' <para>m₂ = E[(x - μ)²]</para>
    ''' <para>m₄ = E[(x - μ)⁴]</para>
    ''' <para>Kurtosis = m₄ / (m₂²)</para>
    ''' The kurtosis of a standard normal distribution is 3.
    ''' Reference definitions:
    ''' <list type="bullet">
    '''   <item><description>NIST: https://www.itl.nist.gov/div898/handbook/eda/section3/eda35b.htm</description></item>
    '''   <item><description>StatsDirect: https://www.statsdirect.com/help/#basic_descriptive_statistics/univariate_summary.htm</description></item>
    ''' </list>
    ''' </remarks>
    Function Kurtosis(arInput() As Double) As Double
        Dim m2 As Double, m4 As Double

        Dim Mean As Double = arInput.Average()
        Dim n As Integer = arInput.Length

        For i = 0 To n - 1
            m2 += ((arInput(i) - Mean) ^ 2) / n
            m4 += ((arInput(i) - Mean) ^ 4) / n
        Next

        If m4 = 0 And m2 = 0 Then
            Return 0
        Else
            Return m4 / m2 ^ 2
        End If
    End Function

    ''' <summary>
    ''' Computes the sample standard deviation of a one-dimensional numeric array.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the array (e.g., Double, Integer, Decimal).
    ''' </typeparam>
    ''' <param name="data">
    ''' A one-dimensional array of type <typeparamref name="T"/> containing numeric values.
    ''' </param>
    ''' <returns>
    ''' The sample standard deviation of the values in <paramref name="data"/>.
    ''' Returns <see cref="Double.NaN"/> if the array has fewer than two elements.
    ''' </returns>
    ''' <remarks>
    ''' - Uses the formula: sqrt(sum((x - mean)^2) / (n - 1)).  
    ''' - For population standard deviation, replace denominator with <c>n</c> in the variance function.  
    ''' - Throws <see cref="InvalidCastException"/> if elements cannot be converted to Double.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: compute standard deviation of an integer array
    ''' Dim arr() As Integer = {2, 4, 6, 8}
    ''' Dim sd As Double = stDev(Of Integer)(arr)
    ''' ' sd ≈ 2.58
    ''' Console.WriteLine(sd)
    ''' </example>
    Public Function stDev(Of T)(data() As T) As Double
        Dim n As Integer = data.Length
        If n <= 1 Then
            BESHstatGlobals.BSlogg.Log("N<=1 for sample standard deviation computation.")
            Return Double.NaN
        End If

        ' Reuse the generic variance function
        Return Math.Sqrt(variance(Of T)(data))
    End Function


    ''' <summary>
    ''' Computes the sample variance of a one-dimensional numeric array.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the array (e.g., Double, Integer, Decimal).
    ''' </typeparam>
    ''' <param name="data">
    ''' A one-dimensional array of type <typeparamref name="T"/> containing numeric values.
    ''' </param>
    ''' <returns>
    ''' The sample variance of the values in <paramref name="data"/>.
    ''' Returns <see cref="Double.NaN"/> if the array has fewer than two elements.
    ''' </returns>
    ''' <remarks>
    ''' - Uses the formula: sum((x - mean)^2) / (n - 1).  
    ''' - For population variance, replace denominator with <c>n</c>.  
    ''' - Throws <see cref="InvalidCastException"/> if elements cannot be converted to Double.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: compute variance of an integer array
    ''' Dim arr() As Integer = {2, 4, 6, 8}
    ''' Dim v As Double = variance(Of Integer)(arr)
    ''' ' v = 6.666...
    ''' Console.WriteLine(v)
    ''' </example>
    Public Function variance(Of T)(data() As T) As Double
        Dim n As Integer = data.Length
        If n <= 1 Then
            BESHstatGlobals.BSlogg.Log("N<=1 for sample variance computation.")
            Return Double.NaN
        End If

        ' Convert all values to Double for computation
        Dim dblData = data.Select(Function(x) Convert.ToDouble(x)).ToArray()
        Dim mean As Double = dblData.Average()
        Dim sum As Double = 0

        For i = 0 To n - 1
            sum += (dblData(i) - mean) ^ 2
        Next

        Return sum / (n - 1)
    End Function


    ''' <summary>
    ''' Computes the sum of squared deviations from the mean,
    ''' equivalent to Excel's DEVSQ, without using LINQ.
    ''' </summary>
    Public Function DevSq(values As IEnumerable(Of Double)) As Double
        If values Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(values)))

        ' Materialize once
        Dim arr() As Double = values.ToArray()
        Dim n As Integer = arr.Length

        If n = 0 Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Sequence contains no elements.", NameOf(values)))

        Dim mean As Double = arr.Average()

        ' Compute sum of squared deviations
        Dim ssd As Double = 0.0
        For i As Integer = 0 To n - 1
            Dim d As Double = arr(i) - mean
            ssd += d * d
        Next

        Return ssd
    End Function

    ''' <summary>
    ''' ParamArray convenience overload.
    ''' </summary>
    Public Function DevSq(ParamArray values() As Double) As Double
        Return DevSq(CType(values, IEnumerable(Of Double)))
    End Function


    ''' <summary>
    ''' Computes the median (second quartile, Q2) of a numeric dataset.
    ''' </summary>
    ''' <param name="data">
    ''' The array of numeric values for which the median is to be calculated.
    ''' </param>
    ''' <returns>
    ''' The median value of the dataset.
    ''' </returns>
    ''' <remarks>
    ''' This function uses <see cref="QuartilesComp"/> to compute quartiles and 
    ''' returns the median component of the resulting <c>udQuartiles</c> structure.
    ''' </remarks>
    Public Function Median(data() As Double) As Double
        Dim q = QuartilesComp(data)
        Return q.Median
    End Function

    ''' <summary>
    ''' Computes the first quartile (Q1), median (Q2), and third quartile (Q3)
    ''' of a dataset using the CDF method (SAS Method 5).
    ''' </summary>
    ''' <param name="arInput">
    ''' An array of numeric values for which quartiles are to be calculated.
    ''' The array is internally sorted before quartile computation.
    ''' </param>
    ''' <returns>
    ''' A <see cref="udQuartiles"/> structure containing Q1, Median, and Q3.
    ''' </returns>
    ''' <remarks>
    ''' Quartiles are computed using the **CDF method**, which corresponds to 
    ''' **SAS default Method 5**.  
    ''' <para>
    ''' Q1 and Q3 are computed based on rank positions using interpolation rules:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>If rank is an integer, the quartile is the mean of the values at ranks r and r+1.</description></item>
    '''   <item><description>If rank is non-integer, the next higher rank (ceiling) is used.</description></item>
    ''' </list>
    ''' </remarks>
    Public Function QuartilesComp(arInput() As Double) As udQuartiles
        'quartiles are computed according "CDF" function, which is SAS default method (method 5)

        Dim dMedian As Double, dQ1 As Double, dQ3 As Double, dRank As Double, xi As Double, XiPlus1 As Double
        Dim out = New udQuartiles

        Dim n As Integer = arInput.Length

        'try to compute quartiles using excel .small function
        'it does not work for the large sample size. Therefore
        'we use QuickSort for quartiles determination in this case.
        Array.Sort(arInput)

        'compute median
        If n = 1 Then
            dMedian = arInput(0)
        Else
            dRank = n / 2
            If n Mod 2 = 0 Then 'even number sample size
                dMedian = (arInput(Int(dRank) - 1) + arInput(Int(dRank))) / 2.0
            Else 'odd number
                dMedian = arInput(Int(dRank))
            End If
        End If

        'compute Q1
        dRank = 0.25 * n
        Dim lRank As Integer = 0.25 * n

        If Math.Abs(dRank - lRank) < 0.00000000000001 Then 'it's integer
            xi = arInput(lRank - 1)
            XiPlus1 = arInput(lRank)
            dQ1 = (xi + XiPlus1) / 2.0
        Else 'it's not and integer
            dQ1 = arInput(RoundUp(dRank, 0) - 1)
        End If

        ' Q3
        dRank = 0.75 * n : lRank = 0.75 * n
        If Math.Abs(dRank - lRank) < 0.00000000000001 Then
            xi = arInput(lRank - 1)
            XiPlus1 = arInput(lRank)
            dQ3 = (xi + XiPlus1) / 2.0
        Else
            dQ3 = arInput(RoundUp(dRank, 0) - 1)
        End If

        out.Q1 = dQ1
        out.Q3 = dQ3
        out.Median = dMedian
        Return out
    End Function




    ''' <summary>
    ''' Computes the sum of squares of all elements in a 1-dimensional array (Excel-like <c>SUMSQ</c>).
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type. Must be a value type (structure) and a supported numeric primitive
    ''' (e.g., <see cref="Double"/>, <see cref="Integer"/>, <see cref="Long"/>).
    ''' </typeparam>
    ''' <param name="values">The input 1D array of numeric values.</param>
    ''' <returns>
    ''' The sum of each element squared, accumulated as a <see cref="Double"/>.
    ''' Returns 0 if <paramref name="values"/> is <c>Nothing</c> or empty.
    ''' </returns>
    ''' <exception cref="ArgumentException">
    ''' Thrown if <typeparamref name="T"/> is not a supported numeric primitive type.
    ''' </exception>
    Public Function SumSq(Of T As Structure)(values As T()) As Double
        If values Is Nothing OrElse values.Length = 0 Then Return 0.0

        Dim sum As Double = 0.0
        For i As Integer = 0 To values.Length - 1
            Dim x As Double = ToDoubleFast(values(i))
            sum += x * x
        Next
        Return sum
    End Function

    ''' <summary>
    ''' Computes the sum of squares of all elements in a 2-dimensional array (Excel-like <c>SUMSQ</c>).
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type. Must be a value type (structure) and a supported numeric primitive
    ''' (e.g., <see cref="Double"/>, <see cref="Integer"/>, <see cref="Long"/>).
    ''' </typeparam>
    ''' <param name="values">The input 2D array of numeric values.</param>
    ''' <returns>
    ''' The sum of each element squared, accumulated as a <see cref="Double"/>.
    ''' Returns 0 if <paramref name="values"/> is <c>Nothing</c>.
    ''' </returns>
    ''' <exception cref="ArgumentException">
    ''' Thrown if <typeparamref name="T"/> is not a supported numeric primitive type.
    ''' </exception>
    Public Function SumSq(Of T As Structure)(values As T(,)) As Double
        If values Is Nothing Then Return 0.0

        Dim sum As Double = 0.0
        Dim r1 As Integer = values.GetUpperBound(0)
        Dim c1 As Integer = values.GetUpperBound(1)

        For r As Integer = 0 To r1
            For c As Integer = 0 To c1
                Dim x As Double = ToDoubleFast(values(r, c))
                sum += x * x
            Next
        Next
        Return sum
    End Function

    ''' <summary>
    ''' Converts a numeric value type to <see cref="Double"/> using fast type checks.
    ''' </summary>
    ''' <typeparam name="T">The value type to convert.</typeparam>
    ''' <param name="value">The value to convert.</param>
    ''' <returns>The value converted to <see cref="Double"/>.</returns>
    ''' <remarks>
    ''' This method is optimized for <see cref="Double"/>, <see cref="Integer"/>, and <see cref="Long"/>.
    ''' Other common numeric primitives are also supported.
    ''' </remarks>
    ''' <exception cref="ArgumentException">
    ''' Thrown if <typeparamref name="T"/> is not a supported numeric primitive type.
    ''' </exception>
    Private Function ToDoubleFast(Of T As Structure)(value As T) As Double
        Dim boxed As Object = value

        ' Primary targets
        If TypeOf boxed Is Double Then
            Return DirectCast(boxed, Double)
        ElseIf TypeOf boxed Is Integer Then
            Return CDbl(DirectCast(boxed, Integer))
        ElseIf TypeOf boxed Is Long Then
            Return CDbl(DirectCast(boxed, Long))

            ' Additional numeric primitives (optional)
        ElseIf TypeOf boxed Is Single Then
            Return CDbl(DirectCast(boxed, Single))
        ElseIf TypeOf boxed Is Decimal Then
            Return CDbl(DirectCast(boxed, Decimal))
        ElseIf TypeOf boxed Is Short Then
            Return CDbl(DirectCast(boxed, Short))
        ElseIf TypeOf boxed Is UShort Then
            Return CDbl(DirectCast(boxed, UShort))
        ElseIf TypeOf boxed Is Byte Then
            Return CDbl(DirectCast(boxed, Byte))
        ElseIf TypeOf boxed Is SByte Then
            Return CDbl(DirectCast(boxed, SByte))
        ElseIf TypeOf boxed Is UInteger Then
            Return CDbl(DirectCast(boxed, UInteger))
        ElseIf TypeOf boxed Is ULong Then
            Return CDbl(DirectCast(boxed, ULong))
        End If
        BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException($"Unsupported element type: {GetType(T).FullName}. Expected Double/Integer/Long."))
        Return Nothing
    End Function

End Module



Public Class DescriptiveStat

    Private pData() As Double
    Private pClean As Boolean
    Private pVariableName As String
    Private pValidN As Integer
    Private pMean As Double
    Private pMedian As Double
    Private pSD As Double
    Private pSEM As Double
    Private pVariance As Double
    Private pCoefficientofVariation As Double
    Private pSkewness As Double
    Private pKurtosis As Double
    Private pLQuartile As Double
    Private pUQuartile As Double
    Private pIQR As Double
    Private pMinimum As Double
    Private pMaximum As Double
    Private pRange As Double
    Private pSWstat As Double = Nothing
    Private pSWPvalue As Double = Nothing
    Sub New(x() As Double)
        pData = x
    End Sub

    Public ReadOnly Property LQuartile() As Double
        Get
            Return pLQuartile
        End Get
    End Property

    Public ReadOnly Property UQuartile() As Double
        Get
            Return pUQuartile
        End Get
    End Property

    Public ReadOnly Property Median() As Double
        Get
            Return pMedian
        End Get
    End Property

    Public ReadOnly Property IQR() As Double
        Get
            Return pIQR
        End Get
    End Property

    Public ReadOnly Property Mean() As Double
        Get
            Return pMean
        End Get
    End Property

    Public ReadOnly Property Maximum() As Double
        Get
            Return pMaximum
        End Get
    End Property

    Public ReadOnly Property Minimum() As Double
        Get
            Return pMinimum
        End Get
    End Property

    Public Function wrapSelf(bLabels As Boolean, Optional statsToReturn As List(Of String) = Nothing) As Object(,)
        Dim out(,) As Object, i As Integer

        If statsToReturn Is Nothing Then 'provide full statistic
            If bLabels Then
                out = {{"Valid Data", Me.pValidN}, {"Mean", Me.pMean}, {"Median", Me.pMedian}, {"SD", Me.pSD}, {"SEM", Me.pSEM},
                       {"Variance", Me.pVariance}, {"Coefficient of Variation", Me.pCoefficientofVariation},
                       {"Skewness", Me.pSkewness}, {"Kurtosis", Me.pKurtosis}, {"Q1", Me.pLQuartile}, {"Q3", Me.pUQuartile},
                       {"IQR", Me.pIQR}, {"Minimum", Me.pMinimum}, {"Maximum", Me.pMaximum}, {"Range", Me.pRange},
                       {"Shapiro-Wilk W", Me.pSWstat}, {"Two-sided p-value", Me.pSWPvalue}}
            Else
                out = {{Me.pValidN}, {Me.pMean}, {Me.pMedian}, {Me.pSD}, {Me.pSEM}, {Me.pVariance}, {Me.pCoefficientofVariation},
                   {Me.pSkewness}, {Me.pKurtosis}, {Me.pLQuartile}, {Me.pUQuartile}, {Me.pIQR}, {Me.pMinimum}, {Me.pMaximum},
                   {Me.pRange}, {Me.pSWstat}, {Me.pSWPvalue}}
            End If
        Else
            If bLabels Then
                ReDim out(statsToReturn.Count - 1, 1)
            Else
                ReDim out(statsToReturn.Count - 1, 0)
            End If

            For i = 0 To statsToReturn.Count - 1
                If statsToReturn(i).ToLower = "mean" Or statsToReturn(i).ToLower = "avg" Or statsToReturn(i).ToLower = "average" Then
                    If bLabels Then
                        out(i, 0) = "Mean"
                        out(i, 1) = Me.pMean
                    Else
                        out(i, 0) = Me.pMean
                    End If
                ElseIf statsToReturn(i).ToLower = "n" Then
                    If bLabels Then
                        out(i, 0) = "Valid Data"
                        out(i, 1) = Me.pValidN
                    Else
                        out(i, 0) = Me.pValidN
                    End If
                ElseIf statsToReturn(i).ToLower = "median" Or statsToReturn(i).ToLower = "q2" Then
                    If bLabels Then
                        out(i, 0) = "Median"
                        out(i, 1) = Me.pMedian
                    Else
                        out(i, 0) = Me.pMedian
                    End If
                ElseIf statsToReturn(i).ToLower = "sd" Or statsToReturn(i).ToLower = "standard deviation" Or statsToReturn(i).ToLower = "std" Then
                    If bLabels Then
                        out(i, 0) = "SD"
                        out(i, 1) = Me.pSD
                    Else
                        out(i, 0) = Me.pSD
                    End If
                ElseIf statsToReturn(i).ToLower = "sem" Then
                    If bLabels Then
                        out(i, 0) = "SEM"
                        out(i, 1) = Me.pSEM
                    Else
                        out(i, 0) = Me.pSEM
                    End If
                ElseIf statsToReturn(i).ToLower = "var" Or statsToReturn(i).ToLower = "variance" Then
                    If bLabels Then
                        out(i, 0) = "Variance"
                        out(i, 1) = Me.pVariance
                    Else
                        out(i, 0) = Me.pVariance
                    End If
                ElseIf statsToReturn(i).ToLower = "cv" Then
                    If bLabels Then
                        out(i, 0) = "Coefficient of Variation"
                        out(i, 1) = Me.pCoefficientofVariation
                    Else
                        out(i, 0) = Me.pCoefficientofVariation
                    End If
                ElseIf statsToReturn(i).ToLower = "skew" Or statsToReturn(i).ToLower = "skewness" Then
                    If bLabels Then
                        out(i, 0) = "Skewness"
                        out(i, 1) = Me.pSkewness
                    Else
                        out(i, 0) = Me.pSkewness
                    End If
                ElseIf statsToReturn(i).ToLower = "kurt" Or statsToReturn(i).ToLower = "kurtosis" Then
                    If bLabels Then
                        out(i, 0) = "Kurtosis"
                        out(i, 1) = Me.pKurtosis
                    Else
                        out(i, 0) = Me.pKurtosis
                    End If
                ElseIf statsToReturn(i).ToLower = "q1" Then
                    If bLabels Then
                        out(i, 0) = "Q1"
                        out(i, 1) = Me.pLQuartile
                    Else
                        out(i, 0) = Me.pLQuartile
                    End If
                ElseIf statsToReturn(i).ToLower = "q3" Then
                    If bLabels Then
                        out(i, 0) = "Q3"
                        out(i, 1) = Me.pUQuartile
                    Else
                        out(i, 0) = Me.pUQuartile
                    End If
                ElseIf statsToReturn(i).ToLower = "min" Or statsToReturn(i).ToLower = "minimum" Then
                    If bLabels Then
                        out(i, 0) = "Minimum"
                        out(i, 1) = Me.pMinimum
                    Else
                        out(i, 0) = Me.pMinimum
                    End If
                ElseIf statsToReturn(i).ToLower = "max" Or statsToReturn(i).ToLower = "maximum" Then
                    If bLabels Then
                        out(i, 0) = "Maximum"
                        out(i, 1) = Me.pMaximum
                    Else
                        out(i, 0) = Me.pMaximum
                    End If
                ElseIf statsToReturn(i).ToLower = "range" Then
                    If bLabels Then
                        out(i, 0) = "Range"
                        out(i, 1) = Me.pRange
                    Else
                        out(i, 0) = Me.pRange
                    End If
                ElseIf statsToReturn(i).ToLower = "iqr" Then
                    If bLabels Then
                        out(i, 0) = "IQR"
                        out(i, 1) = Me.pIQR
                    Else
                        out(i, 0) = Me.pIQR
                    End If
                ElseIf statsToReturn(i).ToLower = "swstat" Then
                    If bLabels Then
                        out(i, 0) = "Shapiro-Wilk W"
                        out(i, 1) = Me.pSWstat
                    Else
                        out(i, 0) = Me.pSWstat
                    End If
                ElseIf statsToReturn(i).ToLower = "swpvalue" Then
                    If bLabels Then
                        out(i, 0) = "Two-sided p-value"
                        out(i, 1) = Me.pSWPvalue
                    Else
                        out(i, 0) = Me.pSWPvalue
                    End If
                Else
                    BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Unrecognized statistic"))
                End If
            Next
        End If
        Return out
    End Function

    Sub compute(Optional bShapiroWilk As Boolean = True)

        Dim Quartiles As udQuartiles, arData() As Double
        Dim strErrTmp As String = String.Empty, SWout = New TestResult

        arData = pData

        'Quartiles computes using user defined subs
        Quartiles = QuartilesComp(arData)
        pLQuartile = Quartiles.Q1
        pMedian = Quartiles.Median
        pUQuartile = Quartiles.Q3
        pIQR = pUQuartile - pLQuartile
        'Skewness and kurtosis computed using standard definitions
        pSkewness = Skewness(arData)
        pKurtosis = Kurtosis(arData)


        pValidN = pData.Length
        pMean = pData.Average()
        pMinimum = pData.Min()
        pMaximum = pData.Max()
        pRange = pMaximum - pMinimum
        If pValidN > 1 Then pVariance = variance(pData)
        If pValidN > 1 Then pSD = stDev(pData)
        If pValidN > 0 Then pSEM = pSD / Math.Sqrt(CDbl(pValidN))
        If pMean <> 0 Then pCoefficientofVariation = pSD / pMean

        'Compute Shapiro-Wilk test
        If pValidN > 3 And pValidN < 5000 And bShapiroWilk = True Then
            SWout = assumptions.ShapiroWilk(arData, strErrTmp)
            pSWstat = SWout.TestStatistics1
            pSWPvalue = SWout.Pvalue
        Else
            pSWstat = -1 : pSWPvalue = -1
        End If
    End Sub

End Class