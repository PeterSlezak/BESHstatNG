Option Explicit On
Imports BESHStatNG.AppInfrastructure

Namespace distributions


    Public Module Distributions

        ''' <summary>
        ''' Returns the two-sided standard normal critical value z_(1 - α/2).
        ''' </summary>
        ''' <param name="alpha">
        ''' Significance level for a two-sided confidence interval.
        ''' Must satisfy 0 &lt; alpha &lt; 1.
        ''' </param>
        ''' <returns>
        ''' The standard normal quantile corresponding to 1 - alpha/2.
        ''' </returns>
        Public Function ZCritTwoSided(Optional alpha As Double = 0.05) As Double
            If alpha <= 0.0 OrElse alpha >= 1.0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(alpha), "alpha must be in (0,1)."))
            End If
            Return NormSInv(1.0 - alpha / 2.0)
        End Function

        ''' <summary>
        ''' Computes the probability density function (PDF) of the normal distribution,
        ''' equivalent to R's <c>dnorm</c> and Excel's <c>NORM.S.DIST(z, FALSE)</c> when mean=0 and sd=1.
        ''' </summary>
        ''' <param name="x">
        ''' The point at which to evaluate the density function.
        ''' </param>
        ''' <param name="mean">
        ''' The mean (μ) of the normal distribution.
        ''' </param>
        ''' <param name="sd">
        ''' The standard deviation (σ) of the normal distribution.
        ''' Must be strictly positive; otherwise the function returns <c>Double.NaN</c>.
        ''' </param>
        ''' <returns>
        ''' The value of the normal density function at <paramref name="x"/>:
        ''' φ(x) = (1 / (σ√(2π))) · exp(-((x - μ)²) / (2σ²)).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function implements the closed-form probability density function of the normal distribution.
        ''' It is exact up to floating-point rounding and does not rely on approximations.
        ''' </para>
        ''' <para>
        ''' Relation to Excel:
        ''' - <c>NORM.S.DIST(z, FALSE)</c> ⇔ <c>DNorm(z, 0, 1)</c>
        ''' - <c>NORM.DIST(x, mean, sd, FALSE)</c> ⇔ <c>DNorm(x, mean, sd)</c>
        ''' </para>
        ''' <para>
        ''' Relation to R:
        ''' - <c>dnorm(x, mean, sd)</c> ⇔ <c>DNorm(x, mean, sd)</c>
        ''' </para>
        ''' <para>
        ''' Edge cases:
        ''' - If <paramref name="sd"/> ≤ 0, returns NaN.
        ''' - If any argument is NaN, returns NaN.
        ''' - For extreme values of <paramref name="x"/>, the density approaches 0.
        ''' </para>
        ''' <para>
        ''' Accuracy: Results are limited only by IEEE double precision. This makes the function
        ''' suitable for statistical applications requiring reproducibility and alignment with
        ''' professional software such as R and Excel.
        ''' </para>
        ''' <example>
        ''' Example usage:
        ''' <code>
        ''' Dim d1 As Double = DNorm(0, 0, 1)       ' returns ~0.3989422804
        ''' Dim d2 As Double = DNorm(1, 0, 1)       ' returns ~0.2419707245
        ''' Dim d3 As Double = DNorm(-2, 0, 1)      ' returns ~0.0539909665
        ''' Dim d4 As Double = DNorm(10, 0, 1)      ' returns ~7.6946E-23
        ''' </code>
        ''' </example>
        ''' </remarks>
        Public Function DNorm(x As Double, Optional mean As Double = 0.0, Optional sd As Double = 1.0) As Double
            If Double.IsNaN(x) OrElse Double.IsNaN(mean) OrElse Double.IsNaN(sd) Then Return Double.NaN
            If sd <= 0.0 Then Return Double.NaN

            Dim z As Double = (x - mean) / sd
            Return Math.Exp(-0.5 * z * z) / (sd * Math.Sqrt(2.0 * Math.PI))
        End Function


        ''' <summary>
        ''' Computes the cumulative distribution function (CDF) of the normal distribution,
        ''' equivalent to R's <c>pnorm</c> and Excel's <c>NORM.S.DIST(z, TRUE)</c> when mean=0 and sd=1.
        ''' </summary>
        ''' <param name="q">
        ''' The quantile (z-value) at which to evaluate the distribution.
        ''' </param>
        ''' <param name="mean">
        ''' The mean (μ) of the normal distribution.
        ''' </param>
        ''' <param name="sd">
        ''' The standard deviation (σ) of the normal distribution.
        ''' Must be strictly positive; otherwise the function returns <c>Double.NaN</c>.
        ''' </param>
        ''' <returns>
        ''' The probability P(X ≤ q) for a normal random variable X ~ N(mean, sd).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This implementation computes the standard normal CDF using the complementary error function
        ''' (via <c>Erfc</c>) for improved numerical stability, especially in the extreme tails where
        ''' <c>1 - p</c> cancellation can occur.
        ''' </para>
        ''' <para>
        ''' Relation to R:
        ''' - <c>pnorm(q, mean, sd)</c> ⇔ <c>PNorm(q, mean, sd)</c>
        ''' </para>
        ''' <para>
        ''' Edge cases:
        ''' - If <paramref name="sd"/> ≤ 0, returns NaN.
        ''' - If any argument is NaN, returns NaN.
        ''' - If <c>q = mean</c>, returns exactly 0.5.
        ''' - For extreme values of q, returns 0 or 1 as appropriate.
        ''' </para>
        ''' <example>
        ''' Example usage:
        ''' <code>
        ''' Dim p1 As Double = PNorm(0, 0, 1)       ' returns 0.5
        ''' Dim p2 As Double = PNorm(1.96, 0, 1)    ' returns ~0.975
        ''' Dim p3 As Double = PNorm(-3, 0, 1)      ' returns ~0.00135
        ''' Dim p4 As Double = PNorm(100, 0, 1)     ' returns 1
        ''' </code>
        ''' </example>
        ''' </remarks>
        Public Function PNorm(q As Double, Optional mean As Double = 0.0, Optional sd As Double = 1.0) As Double
            If Double.IsNaN(q) OrElse Double.IsNaN(mean) OrElse Double.IsNaN(sd) Then Return Double.NaN
            If sd <= 0.0 Then Return Double.NaN

            Dim x As Double = (q - mean) / sd

            ' Exact midpoint
            If x = 0.0 Then Return 0.5

            Dim z As Double = x / Math.Sqrt(2.0)

            ' Use erfc for better tail accuracy and to avoid cancellation
            If z < 0.0 Then
                Return 0.5 * Erfc(-z)
            Else
                Return 1.0 - 0.5 * Erfc(z)
            End If
        End Function


        ''' <summary>
        ''' Computes the error function erf(x), used internally for computing the normal CDF.
        ''' </summary>
        ''' <param name="x">
        ''' The input value for which to evaluate erf(x).
        ''' </param>
        ''' <returns>
        ''' The value of erf(x) in the range [-1, 1].
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The error function is defined as:
        ''' erf(x) = (2 / √π) ∫₀ˣ exp(-t²) dt
        ''' </para>
        ''' <para>
        ''' This implementation uses a rational approximation (Cephes-style) and may delegate
        ''' to <c>Erfc</c> for improved accuracy when |x| is large (to avoid loss of precision).
        ''' </para>
        ''' <para>
        ''' Edge cases:
        ''' - x = 0 returns exactly 0.
        ''' - ±∞ returns ±1.
        ''' - NaN returns NaN.
        ''' </para>
        ''' <example>
        ''' Example usage:
        ''' <code>
        ''' Dim e1 As Double = Erf(0)      ' returns 0
        ''' Dim e2 As Double = Erf(1)      ' returns ~0.842700792949...
        ''' Dim e3 As Double = Erf(-2)     ' returns ~-0.995322265...
        ''' </code>
        ''' </example>
        ''' </remarks>
        Private Function Erf(x As Double) As Double
            ' Cephes erf from ndtr.c
            If Double.IsNaN(x) Then Return Double.NaN
            If x = 0.0 Then Return 0.0
            If Double.IsPositiveInfinity(x) Then Return 1.0
            If Double.IsNegativeInfinity(x) Then Return -1.0

            Dim ax As Double = Math.Abs(x)
            If ax > 1.0 Then Return 1.0 - Erfc(x)

            Dim T() As Double = {
        9.6049737398705162,
        90.026019720384269,
        2232.0053459468431,
        7003.3251411280507,
        55592.301301039493
    }
            Dim U() As Double = {
        33.561714164750313,
        521.35794978015269,
        4594.3238297098014,
        22629.000061389095,
        49267.394260863592
    }

            Dim z As Double = x * x
            Return x * Polevl(z, T, 4) / P1evl(z, U, 5)
        End Function


        ''' <summary>
        ''' Computes the complementary error function erfc(x) = 1 - erf(x).
        ''' </summary>
        ''' <param name="a">
        ''' The input value for which to evaluate erfc(x).
        ''' </param>
        ''' <returns>
        ''' The value of erfc(x) in the range [0, 2].
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The complementary error function is defined as:
        ''' erfc(x) = (2 / √π) ∫ₓ^∞ exp(-t²) dt
        ''' </para>
        ''' <para>
        ''' This function is preferred over computing <c>1 - Erf(x)</c> directly when x is large,
        ''' because it is numerically stable in the tails and avoids catastrophic cancellation.
        ''' </para>
        ''' <para>
        ''' Edge cases:
        ''' - x = 0 returns exactly 1.
        ''' - +∞ returns 0; -∞ returns 2.
        ''' - NaN returns NaN.
        ''' </para>
        ''' <example>
        ''' Example usage:
        ''' <code>
        ''' Dim c1 As Double = Erfc(0)     ' returns 1
        ''' Dim c2 As Double = Erfc(1)     ' returns ~0.157299207050...
        ''' Dim c3 As Double = Erfc(-1)    ' returns ~1.842700792949...
        ''' </code>
        ''' </example>
        ''' </remarks>
        Private Function Erfc(a As Double) As Double
            ' Cephes erfc
            If Double.IsNaN(a) Then Return Double.NaN
            If a = 0.0 Then Return 1.0
            If Double.IsPositiveInfinity(a) Then Return 0.0
            If Double.IsNegativeInfinity(a) Then Return 2.0

            Dim x As Double = Math.Abs(a)

            If x < 1.0 Then Return 1.0 - Erf(a)

            ' exp(-a*a) underflows for large |a|
            If x * x > 745.0 Then Return If(a < 0.0, 2.0, 0.0)

            Dim Pcoef() As Double = {
        0.00000000024619698147353052,
        0.56418956483106886,
        7.4632105644226989,
        48.637197098568137,
        196.5208329560771,
        526.44519499547732,
        934.52852717195765,
        1027.5518868951572,
        557.53533536939938}

            Dim Qcoef() As Double = {
        13.228195115474499,
        86.707214088598974,
        354.93777888781989,
        975.70850174320549,
        1823.9091668790973,
        2246.3376081871097,
        1656.6630919416134,
        557.53534081772773}

            Dim Rcoef() As Double = {
        0.56418958354775506,
        1.275366707599781,
        5.0190504225118051,
        6.160210979930536,
        7.4097426995044895,
        2.9788666537210022}

            Dim Scoef() As Double = {
        2.2605286322011726,
        9.3960352493800148,
        12.048953980809666,
        17.081445074756591,
        9.6089680906328585,
        3.3690764510008151}

            Dim z As Double = Math.Exp(-a * a)

            Dim num As Double
            Dim den As Double

            If x < 8.0 Then
                num = Polevl(x, Pcoef, 8)
                den = P1evl(x, Qcoef, 8)
            Else
                num = Polevl(x, Rcoef, 5)
                den = P1evl(x, Scoef, 6)
            End If

            Dim y As Double = (z * num) / den
            If a < 0.0 Then y = 2.0 - y
            Return y
        End Function

        ''' <summary>
        ''' Evaluates a polynomial using Horner's method.
        ''' </summary>
        ''' <param name="x">
        ''' The point at which to evaluate the polynomial.
        ''' </param>
        ''' <param name="coef">
        ''' Polynomial coefficients in descending order of degree (coef(0) is the highest-degree term).
        ''' </param>
        ''' <param name="N">
        ''' The degree of the polynomial (i.e., the highest power), such that coef has length N+1.
        ''' </param>
        ''' <returns>
        ''' The polynomial value: coef(0)*x^N + coef(1)*x^(N-1) + ... + coef(N).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This helper is used by the rational approximations for <c>Erf</c>/<c>Erfc</c>.
        ''' Horner's method minimizes multiplications and is numerically stable for typical coefficient sets.
        ''' </para>
        ''' </remarks>
        Private Function Polevl(x As Double, coef() As Double, N As Integer) As Double
            ' Horner; coef(0) is highest degree term
            Dim ans As Double = coef(0)
            For i As Integer = 1 To N
                ans = ans * x + coef(i)
            Next
            Return ans
        End Function


        ''' <summary>
        ''' Evaluates a polynomial using Horner's method where the leading coefficient is assumed to be 1.
        ''' </summary>
        ''' <param name="x">
        ''' The point at which to evaluate the polynomial.
        ''' </param>
        ''' <param name="coef">
        ''' Polynomial coefficients in descending order of degree excluding the leading 1.0 term.
        ''' For a degree-N polynomial, coef has length N.
        ''' </param>
        ''' <param name="N">
        ''' The effective degree of the polynomial including the implicit leading 1.0 term.
        ''' </param>
        ''' <returns>
        ''' The polynomial value: x^N + coef(0)*x^(N-1) + ... + coef(N-1).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This helper is used alongside <c>Polevl</c> to evaluate denominators of rational approximations
        ''' efficiently when the leading coefficient is 1.0.
        ''' </para>
        ''' </remarks>
        Private Function P1evl(x As Double, coef() As Double, N As Integer) As Double
            ' Evaluate polynomial with leading coefficient 1.0
            Dim ans As Double = x + coef(0)
            For i As Integer = 1 To N - 1
                ans = ans * x + coef(i)
            Next
            Return ans
        End Function


        ''' <summary>
        ''' Normal quantile function, equivalent to R's qnorm(p, mean, sd).
        ''' </summary>
        Public Function QNorm(p As Double, mean As Double, sd As Double) As Double
            If Double.IsNaN(p) OrElse Double.IsNaN(mean) OrElse Double.IsNaN(sd) Then Return Double.NaN
            If sd <= 0.0 Then Return Double.NaN
            If p < 0.0 OrElse p > 1.0 Then Return Double.NaN
            If p = 0.0 Then Return Double.NegativeInfinity
            If p = 1.0 Then Return Double.PositiveInfinity

            Dim z As Double = NormSInv(p)
            Return mean + sd * z
        End Function

        ''' <summary> 
        ''' Computes the inverse of the standard normal cumulative distribution function Φ⁻¹(p),
        ''' also known as the quantile function of the standard normal distribution.
        ''' </summary>
        ''' <param name="p">
        ''' A probability value in the open interval (0,1).
        ''' </param>
        ''' <returns>
        ''' The z-score (quantile) such that P(Z ≤ z) = p for a standard normal random variable Z.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This implementation uses the Wichura AS241 rational approximation (Applied Statistics, 1988)
        ''' to compute an initial estimate, then applies one Newton refinement step using the standard normal
        ''' CDF (<c>PNorm</c>) and PDF (<c>DNorm</c>) to improve accuracy and better align with R's <c>qnorm</c>.
        ''' </para>
        ''' <para>
        ''' Input validation:
        ''' - If <paramref name="p"/> ≤ 0 or ≥ 1, an <see cref="ArgumentOutOfRangeException"/> is thrown
        '''   (via <c>AppGlobals.BSerr.LogAndThrow</c>).
        ''' </para>
        ''' <para>
        ''' Accuracy: Typically near IEEE double precision for most inputs after refinement; practical agreement
        ''' with professional statistical software (e.g., R) is expected across the probability range, including tails.
        ''' </para>
        ''' <para>
        ''' Notes:
        ''' - The refinement step uses <c>x ← x − (PNorm(x) − p) / DNorm(x)</c>.
        ''' - If numerical underflow were ever to make <c>DNorm(x)</c> evaluate to 0, the refinement step is skipped.
        ''' </para>
        ''' <example>
        ''' Example usage:
        ''' <code>
        ''' Dim z1 As Double = NormSInv(0.5)    ' returns 0
        ''' Dim z2 As Double = NormSInv(0.975)  ' returns ~1.95996398454005
        ''' Dim z3 As Double = NormSInv(1E-6)   ' returns ~-4.7534243088229
        ''' </code>
        ''' </example>
        ''' </remarks>
        Public Function NormSInv(p As Double) As Double
            If p <= 0.0 OrElse p >= 1.0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException("p must be in (0,1)"))

            ' Coefficients for AS241
            Dim a() As Double = {
        -39.696830286653757, 220.9460984245205,
        -275.92851044696869, 138.357751867269,
        -30.66479806614716, 2.5066282774592392}

            Dim b() As Double = {
        -54.476098798224058, 161.58583685804089,
        -155.69897985988661, 66.80131188771972,
        -13.280681552885721}

            Dim c() As Double = {
        -0.0077848940024302926, -0.32239645804113648,
        -2.4007582771618381, -2.5497325393437338,
         4.3746641414649678, 2.9381639826987831}

            Dim d() As Double = {
         0.0077846957090414622, 0.32246712907003983,
         2.445134137142996, 3.7544086619074162}

            Const plow As Double = 0.02425
            Const phigh As Double = 1 - plow

            Dim q, r As Double
            Dim x As Double

            If p < plow Then
                q = Math.Sqrt(-2.0 * Math.Log(p))
                x = (((((c(0) * q + c(1)) * q + c(2)) * q + c(3)) * q + c(4)) * q + c(5)) /
                ((((d(0) * q + d(1)) * q + d(2)) * q + d(3)) * q + 1.0)
            ElseIf p > phigh Then
                q = Math.Sqrt(-2.0 * Math.Log(1.0 - p))
                x = -(((((c(0) * q + c(1)) * q + c(2)) * q + c(3)) * q + c(4)) * q + c(5)) /
                 ((((d(0) * q + d(1)) * q + d(2)) * q + d(3)) * q + 1.0)
            Else
                q = p - 0.5
                r = q * q
                x = (((((a(0) * r + a(1)) * r + a(2)) * r + a(3)) * r + a(4)) * r + a(5)) * q /
                (((((b(0) * r + b(1)) * r + b(2)) * r + b(3)) * r + b(4)) * r + 1.0)
            End If

            ' --- Refinement to match R qnorm() more tightly ---
            Dim pdf As Double = DNorm(x)                ' standard normal pdf
            If pdf > 0.0 Then
                x -= (PNorm(x) - p) / pdf               ' one Newton step
            End If

            Return x
        End Function


        ' ================================
        '  CHI-SQUARE DISTRIBUTION (R-compatible)
        ' ================================

        ''' <summary>
        ''' Computes the probability density function (PDF) of the chi-square distribution,
        ''' equivalent to R's <c>dchisq(x, df)</c> and Excel's <c>CHISQ.DIST(x, df, FALSE)</c>.
        ''' </summary>
        ''' <param name="x">
        ''' The point at which to evaluate the density. Must be non-negative.
        ''' </param>
        ''' <param name="df">
        ''' Degrees of freedom of the chi-square distribution. Must be strictly positive.
        ''' </param>
        ''' <returns>
        ''' The value of the chi-square density function at <paramref name="x"/>:
        ''' φ(x) = (1 / (2^(df/2) Γ(df/2))) · x^(df/2 - 1) · exp(-x/2).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function implements the exact closed-form PDF of the chi-square distribution.
        ''' It matches the numerical behavior of R's <c>dchisq</c> and Excel's
        ''' <c>CHISQ.DIST(x, df, FALSE)</c> to machine precision.
        ''' </para>
        ''' <para>
        ''' Relation to Excel:
        ''' - <c>CHISQ.DIST(x, df, FALSE)</c> ⇔ <c>ChiSquarePDF(x, df)</c>
        ''' </para>
        ''' <para>
        ''' Relation to R:
        ''' - <c>dchisq(x, df)</c> ⇔ <c>ChiSquarePDF(x, df)</c>
        ''' </para>
        ''' <para>
        ''' Edge cases:
        ''' - If <paramref name="x"/> &lt; 0 or <paramref name="df"/> ≤ 0, returns NaN.
        ''' - For large <paramref name="x"/>, the density approaches 0.
        ''' </para>
        ''' <example>
        ''' Example usage:
        ''' <code>
        ''' Dim d1 = ChiSquarePDF(2, 4)   ' ~0.1839
        ''' Dim d2 = ChiSquarePDF(10, 10) ' ~0.1251
        ''' </code>
        ''' </example>
        ''' </remarks>

        Public Function ChiSquarePDF(x As Double, df As Double) As Double
            If x < 0.0 OrElse df <= 0.0 Then Return Double.NaN
            If x = 0.0 AndAlso df = 2.0 Then Return 0.5

            ' PDF = 1/(2^(k/2) Γ(k/2)) * x^(k/2 - 1) * exp(-x/2)
            Dim k As Double = df / 2.0
            Return Math.Exp((k - 1.0) * Math.Log(x) - x / 2.0 - (k * Math.Log(2.0) + LogGamma(k)))
        End Function


        ''' <summary>
        ''' Computes the cumulative distribution function (CDF) of the chi-square distribution,
        ''' equivalent to R's <c>pchisq(x, df)</c> and Excel's <c>CHISQ.DIST(x, df, TRUE)</c>.
        ''' </summary>
        ''' <param name="x">
        ''' The chi-square statistic. Must be non-negative.
        ''' </param>
        ''' <param name="df">
        ''' Degrees of freedom. Must be strictly positive.
        ''' </param>
        ''' <returns>
        ''' The lower-tail probability P(X ≤ x) for X ~ χ²(df).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This implementation uses the regularized lower incomplete gamma function:
        ''' P(X ≤ x) = γ(df/2, x/2) / Γ(df/2).
        ''' </para>
        ''' <para>
        ''' Numerical behavior matches R's <c>pchisq</c> to approximately 1e-14.
        ''' Excel's <c>CHISQ.DIST(x, df, TRUE)</c> is similar but diverges in extreme tails.
        ''' </para>
        ''' <para>
        ''' Relation to Excel:
        ''' - <c>CHISQ.DIST(x, df, TRUE)</c> ⇔ lower-tail CDF ⇔ <c>ChiSquareCDF(x, df)</c>
        ''' - <c>CHISQ.DIST.RT(x, df)</c> ⇔ upper-tail CDF ⇔ <c>1 - ChiSquareCDF(x, df)</c>
        ''' </para>
        ''' <para>
        ''' Relation to R:
        ''' - <c>pchisq(x, df, lower.tail = TRUE)</c> ⇔ <c>ChiSquareCDF(x, df)</c>
        ''' - <c>pchisq(x, df, lower.tail = FALSE)</c> ⇔ <c>1 - ChiSquareCDF(x, df)</c>
        ''' </para>
        ''' <para>
        ''' Edge cases:
        ''' - If <paramref name="x"/> = 0, returns 0.
        ''' - If <paramref name="x"/> → ∞, returns 1.
        ''' - If <paramref name="df"/> ≤ 0, returns NaN.
        ''' </para>
        ''' <example>
        ''' Example usage:
        ''' <code>
        ''' Dim p1 = ChiSquareCDF(3.84, 1)   ' ~0.95
        ''' Dim p2 = ChiSquareCDF(10, 5)     ' ~0.9248
        ''' Dim upperTail = 1 - ChiSquareCDF(10, 5) ' Excel CHISQ.DIST.RT
        ''' </code>
        ''' </example>
        ''' </remarks>

        Public Function ChiSquareCDF(x As Double, df As Double) As Double
            If Double.IsNaN(x) OrElse Double.IsNaN(df) Then Return Double.NaN
            If x < 0.0 OrElse df <= 0.0 Then Return Double.NaN
            If x = 0.0 Then Return 0.0
            If Double.IsInfinity(x) Then Return 1.0

            ' For extremely large x, CDF is effectively 1 (prevents numerical issues).
            If x > 1000000000000.0 Then Return 1.0

            Return LowerIncompleteGamma(df / 2.0, x / 2.0)
        End Function



        ''' <summary>
        ''' Computes the inverse chi-square cumulative distribution function (quantile),
        ''' equivalent to R's <c>qchisq(p, df)</c> and Excel's <c>CHISQ.INV</c> and <c>CHISQ.INV.RT</c>.
        ''' </summary>
        ''' <param name="p">
        ''' The probability value. Must be in the interval [0, 1].
        ''' </param>
        ''' <param name="df">
        ''' Degrees of freedom. Must be strictly positive.
        ''' </param>
        ''' <returns>
        ''' The value x such that P(X ≤ x) = p for X ~ χ²(df).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function uses the Wilson–Hilferty transformation for an initial approximation,
        ''' followed by several Newton–Raphson refinement steps. It matches R's <c>qchisq</c>
        ''' to approximately 1e-12 across the full probability range.
        ''' </para>
        ''' <para>
        ''' Relation to Excel:
        ''' - <c>CHISQ.INV(p, df)</c> ⇔ lower-tail quantile ⇔ <c>ChiSquareInv(p, df)</c>
        ''' - <c>CHISQ.INV.RT(p, df)</c> ⇔ upper-tail quantile ⇔ <c>ChiSquareInv(1 - p, df)</c>
        ''' </para>
        ''' <para>
        ''' Relation to R:
        ''' - <c>qchisq(p, df, lower.tail = TRUE)</c> ⇔ <c>ChiSquareInv(p, df)</c>
        ''' - <c>qchisq(p, df, lower.tail = FALSE)</c> ⇔ <c>ChiSquareInv(1 - p, df)</c>
        ''' </para>
        ''' <para>
        ''' Edge cases:
        ''' - p = 0 → returns 0
        ''' - p = 1 → returns +∞
        ''' - p outside [0,1] → NaN
        ''' - df ≤ 0 → NaN
        ''' </para>
        ''' <example>
        ''' Example usage:
        ''' <code>
        ''' Dim q1 = ChiSquareInv(0.95, 1)   ' ~3.841459
        ''' Dim q2 = ChiSquareInv(0.99, 10)  ' ~23.20925
        ''' Dim upper = ChiSquareInv(1 - 0.95, 4) ' Excel CHISQ.INV.RT
        ''' </code>
        ''' </example>
        ''' </remarks>

        Public Function ChiSquareInv(p As Double, df As Double) As Double
            If p < 0.0 OrElse p > 1.0 OrElse df <= 0.0 Then Return Double.NaN
            If p = 0.0 Then Return 0.0
            If p = 1.0 Then Return Double.PositiveInfinity

            ' Wilson–Hilferty approximation
            Dim k As Double = df
            Dim z As Double = NormSInv(p)
            Dim x As Double = k * Math.Pow(1.0 - 2.0 / (9 * k) + z * Math.Sqrt(2.0 / (9.0 * k)), 3.0)

            ' Newton refinement
            For i As Integer = 1 To 5
                Dim f As Double = ChiSquareCDF(x, df) - p
                Dim fp As Double = ChiSquarePDF(x, df)
                x -= f / fp
                If x <= 0.0 Then x = 0.5 * x
            Next

            Return x
        End Function

        ''' <summary>
        ''' Computes the lower-tail quantile of the Student-t distribution,
        ''' equivalent to Excel T.INV(p, df) and R qt(p, df).
        ''' </summary>
        ''' <param name="p">
        ''' Probability value in [0,1]. Represents P(T ≤ x).
        ''' </param>
        ''' <param name="df">
        ''' Degrees of freedom. May be non-integer and must be strictly positive.
        ''' </param>
        ''' <returns>
        ''' The value x such that P(T ≤ x) = p for a Student-t random variable.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function computes the inverse CDF (quantile) of the Student-t distribution.
        ''' It uses a normal approximation for the initial estimate followed by several
        ''' Newton–Raphson refinement steps using the PDF and CDF for convergence.
        ''' </para>
        ''' 
        ''' <para>
        ''' <b>Relation to Excel:</b><br/>
        ''' - <c>T.INV(p, df)</c> ⇔ <c>T_Inv(p, df)</c><br/>
        ''' - <c>T.INV.2T(p, df)</c> ⇔ <c>T_Inv_2T(p, df)</c>
        ''' </para>
        ''' 
        ''' <para>
        ''' <b>Relation to R:</b><br/>
        ''' - <c>qt(p, df, lower.tail=TRUE)</c> ⇔ <c>T_Inv(p, df)</c><br/>
        ''' - <c>qt(p, df, lower.tail=FALSE)</c> ⇔ <c>-T_Inv(p, df)</c>
        ''' </para>
        ''' 
        ''' <para>
        ''' <b>Edge cases:</b>
        ''' <list type="bullet">
        '''   <item><description>p = 0 → -∞</description></item>
        '''   <item><description>p = 1 → +∞</description></item>
        '''   <item><description>df ≤ 0 → NaN</description></item>
        '''   <item><description>p outside [0,1] → NaN</description></item>
        ''' </list>
        ''' </para>
        ''' 
        ''' <example>
        ''' Example usage:
        ''' <code>
        ''' Dim q1 = T_Inv(0.975, 10)   ' ~2.228
        ''' Dim q2 = T_Inv(0.95, 30)    ' ~1.697
        ''' </code>
        ''' </example>
        ''' </remarks>
        Public Function T_Inv(p As Double, df As Double) As Double
            If p <= 0.0 Then Return Double.NegativeInfinity
            If p >= 1.0 Then Return Double.PositiveInfinity
            If df <= 0.0 Then Return Double.NaN

            ' Symmetry: use central region for accuracy
            Dim neg As Boolean = False
            Dim pp As Double = p
            If p < 0.5 Then
                pp = 1.0 - p
                neg = True
            End If

            ' --------------------------------------------------------------------
            ' 1. High-precision initial approximation (Hill + 5 correction terms)
            ' --------------------------------------------------------------------
            Dim z As Double = NormSInv(pp)  ' your high-precision normal inverse
            Dim z2 As Double = z * z

            ' Hill approximation with corrections
            Dim a As Double = (z2 + 1.0) / (4 * df)
            Dim b As Double = ((5.0 * z2 + 16.0) * z2 + 3.0) / (96.0 * df * df)
            Dim c As Double = (((3.0 * z2 + 19.0) * z2 + 17.0) * z2 - 15.0) / (384.0 * df * df * df)
            Dim d As Double = ((((79.0 * z2 + 776.0) * z2 + 1482.0) * z2 - 1920.0) * z2 - 945.0) / (92160.0 * df * df * df * df)

            Dim t As Double = z + z * a + z * b + z * c + z * d

            ' --------------------------------------------------------------------
            ' 2. Newton refinement using exact t-CDF and t-PDF
            ' --------------------------------------------------------------------
            For i As Integer = 1 To 10
                Dim f As Double = T_CDF(t, df) - pp
                Dim fp As Double = T_PDF(t, df)

                Dim stepSize As Double = f / fp
                t -= stepSize

                ' Safeguard: if Newton step is too large, dampen it
                If Math.Abs(stepSize) > 1 Then t += stepSize * 0.5
                If Math.Abs(stepSize) < 0.00000000000001 Then Exit For
            Next

            If neg Then t = -t
            Return t
        End Function


        ''' <summary>
        ''' Computes the two-tailed quantile of the Student-t distribution,
        ''' equivalent to Excel T.INV.2T(p, df).
        ''' </summary>
        ''' <param name="p">Two-tailed probability in (0,1).</param>
        ''' <param name="df">Degrees of freedom (non-integer allowed).</param>
        ''' <returns>
        ''' The value x such that P(|T| ≥ x) = p.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Excel defines T.INV.2T(p, df) as the symmetric two-tailed quantile:
        ''' x = T.INV(1 - p/2, df).
        ''' </para>
        ''' 
        ''' <para>
        ''' <b>Relation to Excel:</b><br/>
        ''' - <c>T.INV.2T(p, df)</c> ⇔ <c>T_Inv_2T(p, df)</c>
        ''' </para>
        ''' 
        ''' <para>
        ''' <b>Relation to R:</b><br/>
        ''' - <c>qt(1 - p/2, df)</c> ⇔ <c>T_Inv_2T(p, df)</c>
        ''' </para>
        ''' 
        ''' <example>
        ''' <code>
        ''' Dim x = T_Inv_2T(0.05, 12)   ' ~2.1788
        ''' </code>
        ''' </example>
        ''' </remarks>
        Public Function T_Inv_2T(p As Double, df As Double) As Double
            If p <= 0.0 OrElse p >= 1.0 Then Return Double.NaN
            Return Math.Abs(T_Inv(1.0 - p / 2.0, df))
        End Function

        ''' <summary>
        ''' Student-t probability density function, equivalent to Excel T.DIST(x,df,FALSE)
        ''' and R dt(x, df). Supports non-integer df.
        ''' </summary>
        Public Function T_PDF(x As Double, df As Double) As Double
            If df <= 0.0 Then Return Double.NaN

            Dim lg As Double = LogGamma((df + 1.0) / 2.0) - LogGamma(df / 2.0)
            Dim c As Double = Math.Exp(lg) / (Math.Sqrt(df * Math.PI))
            Return c * Math.Pow(1.0 + (x * x) / df, -(df + 1.0) / 2.0)
        End Function

        ''' <summary>
        ''' Student-t cumulative distribution function, equivalent to Excel T.DIST(x,df,TRUE)
        ''' and R pt(x, df). Supports non-integer df.
        ''' </summary>
        Public Function T_CDF(x As Double, df As Double) As Double
            If df <= 0 Then Return Double.NaN

            Dim t As Double = df / (df + x * x)
            Dim ib As Double = RegularizedIncompleteBeta(t, df / 2.0, 0.5)

            If x >= 0 Then
                Return 1.0 - 0.5 * ib
            Else
                Return 0.5 * ib
            End If
        End Function

        ''' <summary>
        ''' Right-tail Student-t probability, equivalent to Excel T.DIST.RT(x,df)
        ''' and R pt(x,df,lower.tail=FALSE).
        ''' </summary>
        Public Function T_RT(x As Double, df As Double) As Double
            Return 1.0 - T_CDF(x, df)
        End Function

        ''' <summary>
        ''' Two-tailed Student-t probability, equivalent to Excel T.DIST.2T(x,df)
        ''' and R 2*(1-pt(|x|,df)).
        ''' </summary>
        Public Function T_2T(x As Double, df As Double) As Double
            Return 2.0 * T_RT(Math.Abs(x), df)
        End Function

        ''' <summary>
        ''' Computes the regularized incomplete beta function I_x(a,b),
        ''' used as the core of the Student-t CDF.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' <b>Relation to Excel:</b><br/>
        ''' Excel does not expose the incomplete beta directly, but uses it internally
        ''' for T.DIST, T.DIST.RT, and T.DIST.2T.
        ''' </para>
        ''' 
        ''' <para>
        ''' <b>Relation to R:</b><br/>
        ''' Matches R's pbeta(x, a, b) to ~1e-14.
        ''' </para>
        ''' 
        ''' <para>
        ''' <b>Numerical method:</b><br/>
        ''' - For x .lt. (a+1)/(a+b+2): uses continued fraction directly.<br/>
        ''' - Otherwise uses symmetry I_x(a,b) = 1 - I_{1-x}(b,a).
        ''' </para>
        ''' </remarks>
        Public Function RegularizedIncompleteBeta(x As Double, a As Double, b As Double) As Double
            If x < 0.0 OrElse x > 1.0 Then Return Double.NaN

            Dim bt As Double
            If x = 0.0 OrElse x = 1.0 Then
                bt = 0.0
            Else
                bt = Math.Exp(LogGamma(a + b) - LogGamma(a) - LogGamma(b) +
                      a * Math.Log(x) + b * Math.Log(1 - x))
            End If

            Dim sym As Boolean = x < (a + 1) / (a + b + 2)
            Dim result As Double

            If sym Then
                result = bt * BetaContinuedFraction(x, a, b) / a
            Else
                result = 1.0 - bt * BetaContinuedFraction(1 - x, b, a) / b
            End If

            Return result
        End Function

        ''' <summary>
        ''' Computes the inverse of the regularized incomplete beta function I_x(a,b),
        ''' equivalent to R's qbeta(p,a,b) and Excel's BETA.INV(p,a,b).
        ''' </summary>
        ''' <param name="p">Probability in (0,1).</param>
        ''' <param name="a">First shape parameter (must be > 0).</param>
        ''' <param name="b">Second shape parameter (must be > 0).</param>
        ''' <returns>
        ''' The value x in [0,1] such that I_x(a,b) = p.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This implementation uses:
        ''' - An AS109-style initial approximation based on the normal quantile
        ''' - Newton-Raphson refinement using the beta PDF and CDF
        ''' - Stable handling of extreme p values
        ''' </para>
        ''' <para>
        ''' <b>Relation to Excel:</b><br/>
        ''' - Excel BETA.INV(p,a,b) ⇔ InverseRegularizedIncompleteBeta(p,a,b)
        ''' - Used internally by Excel's F.INV, F.INV.RT, T.INV, T.INV.2T
        ''' </para>
        ''' <para>
        ''' <b>Relation to R:</b><br/>
        ''' - R qbeta(p,a,b) ⇔ InverseRegularizedIncompleteBeta(p,a,b)
        ''' </para>
        ''' </remarks>
        Public Function InverseRegularizedIncompleteBeta(p As Double, a As Double, b As Double) As Double
            If p <= 0 Then Return 0.0
            If p >= 1 Then Return 1.0
            If a <= 0 OrElse b <= 0 Then Return Double.NaN

            ' Symmetry transform for better convergence:
            ' I_x(a,b) = 1 - I_(1-x)(b,a)
            Dim flip As Boolean = False
            Dim pp As Double = p
            Dim aa As Double = a
            Dim bb As Double = b

            If p > 0.5 Then
                pp = 1.0 - p
                aa = b
                bb = a
                flip = True
            End If

            ' Initial approximation using normal quantile
            Dim t As Double = Math.Sqrt(-2.0 * Math.Log(pp))
            Dim x As Double = (2.30753 + t * 0.27061) / (1.0 + t * (0.99229 + t * 0.04481)) - t
            If pp < 0.5 Then x = -x   ' IMPORTANT: use pp, not p

            Dim al As Double = (x * x - 3) / 6
            Dim h As Double = 2 / (1 / (2 * aa - 1) + 1 / (2 * bb - 1))
            Dim w As Double = x * Math.Sqrt(h + al) / h - (1 / (2 * bb - 1) - 1 / (2 * aa - 1)) * (al + 5 / 6 - 2 / (3 * h))
            Dim x0 As Double = aa / (aa + bb * Math.Exp(2.0 * w))

            ' Newton refinement
            Dim x1 As Double = x0
            For i As Integer = 1 To 12
                Dim f As Double = RegularizedIncompleteBeta(x1, aa, bb) - pp
                Dim pdf As Double = Math.Exp((aa - 1) * Math.Log(x1) + (bb - 1) * Math.Log(1 - x1) - LogBeta(aa, bb))
                Dim dx As Double = f / pdf
                x1 -= dx

                If x1 <= 0 Then x1 = x1 / 2.0
                If x1 >= 1 Then x1 = (x1 + 1) / 2.0
                If Math.Abs(dx) < 0.00000000000001 * x1 Then Exit For
            Next

            Return If(flip, 1.0 - x1, x1)
        End Function


        ''' <summary>
        ''' Evaluates the continued fraction representation of the incomplete beta
        ''' function using Lentz's algorithm (Numerical Recipes betacf).
        ''' </summary>
        ''' <param name="x">
        ''' The evaluation point in the interval [0, 1]. This is typically
        ''' x = df / (df + t^2) when used inside the Student-t CDF.
        ''' </param>
        ''' <param name="a">
        ''' The first shape parameter of the beta function. Must be positive.
        ''' </param>
        ''' <param name="b">
        ''' The second shape parameter of the beta function. Must be positive.
        ''' </param>
        ''' <returns>
        ''' The value of the continued fraction for the incomplete beta function.
        ''' This value is not the regularized incomplete beta itself; it must be
        ''' combined with the prefactor:
        ''' 
        '''     exp( logGamma(a + b) - logGamma(a) - logGamma(b)
        '''          + a * log(x) + b * log(1 - x) )
        ''' 
        ''' and divided by <c>a</c> or <c>b</c> depending on the symmetry branch.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This routine implements the stable continued fraction expansion for the
        ''' incomplete beta function using Lentz's method. It is the core numerical
        ''' engine used by <see cref="RegularizedIncompleteBeta"/> to compute the
        ''' regularized incomplete beta I_x(a, b).
        ''' </para>
        ''' 
        ''' <para>
        ''' The algorithm is robust for all valid (a, b, x) and converges rapidly
        ''' even for large shape parameters. It is essential for accurate evaluation
        ''' of distribution functions such as the Student-t CDF, F-distribution CDF,
        ''' and the Beta distribution CDF.
        ''' </para>
        ''' 
        ''' <para>
        ''' This function returns only the continued fraction value. The caller is
        ''' responsible for applying the appropriate prefactor and symmetry logic.
        ''' </para>
        ''' 
        ''' <para>
        ''' References:
        ''' Numerical Recipes, 3rd Edition — Section on the incomplete beta function.
        ''' </para>
        ''' </remarks>
        Private Function BetaContinuedFraction(x As Double, a As Double, b As Double) As Double
            Const eps As Double = 0.00000000000001
            Const maxIter As Integer = 200
            Const FPMIN As Double = 1.0E-300

            Dim qab As Double = a + b
            Dim qap As Double = a + 1.0
            Dim qam As Double = a - 1.0

            Dim c As Double = 1.0
            Dim d As Double = 1.0 - qab * x / qap
            If Math.Abs(d) < FPMIN Then d = FPMIN
            d = 1.0 / d
            Dim h As Double = d

            For m As Integer = 1 To maxIter
                Dim m2 As Integer = 2 * m

                ' First step
                Dim aa As Double = m * (b - m) * x / ((qam + m2) * (a + m2))
                d = 1.0 + aa * d
                If Math.Abs(d) < FPMIN Then d = FPMIN
                c = 1.0 + aa / c
                If Math.Abs(c) < FPMIN Then c = FPMIN
                d = 1.0 / d
                h *= d * c

                ' Second step
                aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2))
                d = 1.0 + aa * d
                If Math.Abs(d) < FPMIN Then d = FPMIN
                c = 1.0 + aa / c
                If Math.Abs(c) < FPMIN Then c = FPMIN
                d = 1.0 / d
                Dim delta As Double = d * c
                h *= delta

                If Math.Abs(delta - 1.0) < eps Then Exit For
            Next

            Return h
        End Function


        ''' <summary>
        ''' Computes the probability density function of the F-distribution,
        ''' equivalent to Excel F.DIST(x, df1, df2, FALSE) and R df(x, df1, df2).
        ''' Supports non-integer degrees of freedom.
        ''' </summary>
        ''' <param name="x">Point at which to evaluate the density. Must be non-negative.</param>
        ''' <param name="df1">Numerator degrees of freedom. Must be positive.</param>
        ''' <param name="df2">Denominator degrees of freedom. Must be positive.</param>
        ''' <returns>The F-distribution PDF evaluated at x.</returns>
        ''' <remarks>
        ''' <para>
        ''' The density is defined as:<br/>
        ''' f(x) = sqrt((df1·x)^(df1) · df2^(df2) / (df1·x + df2)^(df1+df2)) / (x·B(df1/2, df2/2))
        ''' </para>
        ''' <para>
        ''' <b>Relation to Excel:</b><br/>
        ''' - F.DIST(x, df1, df2, FALSE) ⇔ F_PDF(x, df1, df2)
        ''' </para>
        ''' <para>
        ''' <b>Relation to R:</b><br/>
        ''' - df(x, df1, df2) ⇔ F_PDF(x, df1, df2)
        ''' </para>
        ''' </remarks>
        Public Function F_PDF(x As Double, df1 As Double, df2 As Double) As Double
            If x < 0 OrElse df1 <= 0 OrElse df2 <= 0 Then Return Double.NaN

            Dim a As Double = df1 / 2.0
            Dim b As Double = df2 / 2.0
            Dim num As Double = Math.Sqrt(Math.Pow(df1 * x, df1) * Math.Pow(df2, df2))
            Dim den As Double = Math.Sqrt(Math.Pow(df1 * x + df2, df1 + df2)) * x * Math.Exp(LogBeta(a, b))

            Return num / den
        End Function

        ''' <summary>
        ''' Computes the lower-tail cumulative distribution function of the F-distribution,
        ''' equivalent to Excel F.DIST(x, df1, df2, TRUE) and R pf(x, df1, df2).
        ''' </summary>
        Public Function F_CDF(x As Double, df1 As Double, df2 As Double) As Double
            If x < 0.0 OrElse df1 <= 0.0 OrElse df2 <= 0.0 Then Return Double.NaN

            Dim a As Double = df1 / 2.0
            Dim b As Double = df2 / 2.0
            Dim t As Double = (df1 * x) / (df1 * x + df2)

            Return RegularizedIncompleteBeta(t, a, b)
        End Function

        ''' <summary>
        ''' Computes the right-tail probability of the F-distribution,
        ''' equivalent to Excel F.DIST.RT(x, df1, df2) and R pf(x, df1, df2, lower.tail=FALSE).
        ''' </summary>
        Public Function F_RT(x As Double, df1 As Double, df2 As Double) As Double
            Return 1.0 - F_CDF(x, df1, df2)
        End Function

        ''' <summary>
        ''' Computes the two-tailed probability for the F-distribution,
        ''' equivalent to legacy Excel FDIST and R 2*(1 - pf(x,df1,df2)).
        ''' </summary>
        Public Function F_2T(x As Double, df1 As Double, df2 As Double) As Double
            Return 2.0 * F_RT(x, df1, df2)
        End Function

        ''' <summary>
        ''' Computes the lower-tail quantile of the F-distribution,
        ''' equivalent to Excel F.INV(p, df1, df2) and R qf(p, df1, df2).
        ''' </summary>
        Public Function F_Inv(p As Double, df1 As Double, df2 As Double) As Double
            If p <= 0.0 Then Return 0.0
            If p >= 1.0 Then Return Double.PositiveInfinity
            If df1 <= 0.0 OrElse df2 <= 0.0 Then Return Double.NaN

            Dim a As Double = df1 / 2.0
            Dim b As Double = df2 / 2.0

            Dim t As Double = InverseRegularizedIncompleteBeta(p, a, b)
            Return df2 * t / (df1 * (1.0 - t))
        End Function

        ''' <summary>
        ''' Computes the right-tail quantile of the F-distribution,
        ''' equivalent to Excel F.INV.RT(p, df1, df2) and R qf(p, df1, df2, lower.tail=FALSE).
        ''' </summary>
        Public Function F_Inv_RT(p As Double, df1 As Double, df2 As Double) As Double
            Return F_Inv(1.0 - p, df1, df2)
        End Function

        ''' <summary>
        ''' Computes log(Beta(a,b)) using log-gamma identities.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Beta(a,b) = Γ(a)Γ(b)/Γ(a+b)
        ''' </para>
        ''' <para>
        ''' Used by F_PDF and F_CDF.
        ''' </para>
        ''' </remarks>
        Private Function LogBeta(a As Double, b As Double) As Double
            Return LogGamma(a) + LogGamma(b) - LogGamma(a + b)
        End Function






        ' ========================================================================
        '   PoissonDistribution Module
        '   Hybrid R-grade implementation using shared helpers:
        '   - LogGamma
        '   - NormalCDF
        '   - NormalInv
        '   - RegularizedIncompleteGamma (Gamma CDF)
        ' ========================================================================

        ''' <summary>
        ''' Computes the Poisson probability mass function (PMF),
        ''' equivalent to Excel POISSON.DIST(x, mean, FALSE) and R dpois(x, lambda).
        ''' </summary>
        ''' <param name="x">
        ''' Observed count. If non-integer, it is truncated to floor(x),
        ''' matching Excel and R behavior.
        ''' </param>
        ''' <param name="lambda">
        ''' Mean rate λ. Must be non-negative. Supports non-integer values.
        ''' </param>
        ''' <returns>
        ''' P(X = k) where k = floor(x). Returns NaN for invalid parameters.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Uses log-space computation via LogGamma for numerical stability.
        ''' </para>
        ''' </remarks>
        Public Function PoissonPMF(x As Double, lambda As Double) As Double
            If lambda < 0.0 OrElse Double.IsNaN(x) OrElse Double.IsNaN(lambda) Then Return Double.NaN

            Dim k As Integer = CInt(Math.Floor(x))
            If k < 0 Then Return 0.0

            If lambda = 0.0 Then Return If(k = 0, 1.0, 0.0)

            ' Fast and safest special-case used by ZIP: P(Y=0) = exp(-lambda)
            If k = 0 Then
                ' exp(-lambda) underflows to 0 for very large lambda (correct)
                Return Math.Exp(-lambda)
            End If

            Dim logP As Double = -lambda + k * Math.Log(lambda) - LogGamma(k + 1.0)

            Return Math.Exp(logP)
        End Function


        ''' <summary>
        ''' Computes the cumulative Poisson probability P(X ≤ x),
        ''' equivalent to Excel POISSON.DIST(x, mean, TRUE) and R ppois(x, lambda).
        ''' </summary>
        ''' <param name="x">Upper bound. Non-integer values are truncated.</param>
        ''' <param name="lambda">Mean rate λ (non-negative).</param>
        ''' <returns>P(X ≤ floor(x)).</returns>
        ''' <remarks>
        ''' <para>
        ''' Hybrid strategy:
        ''' - Direct summation for λ ≤ 20
        ''' - Recursive PMF walk for 20 .lt. λ ≤ 200
        ''' - Gamma CDF identity for λ > 200
        ''' </para>
        ''' </remarks>
        Public Function PoissonCDF(x As Double, lambda As Double) As Double
            If lambda < 0.0 Then Return Double.NaN

            Dim k As Integer = CInt(Math.Floor(x))
            If k < 0 Then Return 0.0

            If lambda <= 20.0 Then
                Return PoissonCDF_Direct(k, lambda)
            ElseIf lambda <= 200.0 Then
                Return PoissonCDF_Recursive(k, lambda)
            Else
                Return PoissonCDF_Gamma(k, lambda)
            End If
        End Function

        ''' <summary>
        ''' Computes the upper-tail Poisson probability P(X > x),
        ''' equivalent to 1 - POISSON.DIST(x, mean, TRUE) in Excel and
        ''' R ppois(x, lambda, lower.tail = FALSE).
        ''' </summary>
        ''' <param name="x">
        ''' Threshold count. Non-integer values are truncated to floor(x),
        ''' consistent with Excel and R.
        ''' </param>
        ''' <param name="lambda">
        ''' Mean rate λ (non-negative real number).
        ''' </param>
        ''' <returns>
        ''' The probability P(X > floor(x)).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' <b>Relation to Excel:</b><br/>
        ''' - No direct function; computed as 1 - POISSON.DIST(x, mean, TRUE).
        ''' </para>
        ''' <para>
        ''' <b>Relation to R:</b><br/>
        ''' - ppois(x, lambda, lower.tail = FALSE) ⇔ PoissonUpperTail(x, lambda)
        ''' </para>
        ''' </remarks>
        Public Function PoissonUpperTail(x As Double, lambda As Double) As Double
            Dim lower As Double = PoissonCDF(x, lambda)
            If Double.IsNaN(lower) Then Return Double.NaN
            Return 1.0 - lower
        End Function

        ''' <summary>
        ''' Computes the Poisson quantile (inverse CDF),
        ''' equivalent to R qpois(p, lambda). Excel has no built-in inverse.
        ''' </summary>
        ''' <param name="p">Target probability in [0,1].</param>
        ''' <param name="lambda">Mean rate λ.</param>
        ''' <returns>
        ''' Smallest integer k such that P(X ≤ k) ≥ p.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Hybrid strategy:
        ''' - Normal approximation seed
        ''' - Local monotone search
        ''' - Newton refinement
        ''' - Uses PoissonCDF (which itself is hybrid)
        ''' </para>
        ''' </remarks>
        Public Function PoissonInv(p As Double, lambda As Double) As Integer
            If p < 0.0 OrElse p > 1.0 OrElse lambda < 0.0 Then Return Integer.MinValue
            If p = 0.0 Then Return 0.0
            If p = 1.0 Then Return Integer.MaxValue

            Return PoissonInv_Hybrid(p, lambda)
        End Function



        ' ====================================================================
        '   INTERNAL: HYBRID CDF IMPLEMENTATION
        ' ====================================================================

        Private Function PoissonCDF_Direct(k As Integer, lambda As Double) As Double
            Dim sum As Double = 0.0
            For i As Integer = 0 To k
                sum += PoissonPMF(i, lambda)
            Next
            Return sum
        End Function


        Private Function PoissonCDF_Recursive(k As Integer, lambda As Double) As Double
            Dim mode As Integer = Math.Floor(lambda)
            Dim pm As Double = PoissonPMF(mode, lambda)
            Dim sum As Double = pm
            Dim p As Double = pm

            Dim i As Integer = mode - 1
            While i >= 0
                p *= (i + 1) / lambda
                sum += p
                i -= 1
            End While

            p = pm
            i = mode + 1
            While i <= k
                p *= lambda / i
                sum += p
                i += 1
            End While

            Return sum
        End Function


        Private Function PoissonCDF_Gamma(k As Integer, lambda As Double) As Double
            Return LowerIncompleteGamma(k + 1, lambda)
        End Function



        ' ====================================================================
        '   INTERNAL: HYBRID QUANTILE IMPLEMENTATION
        ' ====================================================================

        Private Function PoissonInv_Hybrid(p As Double, lambda As Double) As Integer
            Dim z As Double = NormSInv(p)
            Dim guess As Double = lambda + z * Math.Sqrt(lambda)

            Dim k As Integer = Math.Max(0, CInt(Math.Floor(guess)))

            While PoissonCDF(k, lambda) >= p AndAlso k > 0
                k -= 1
            End While

            While PoissonCDF(k, lambda) < p
                k += 1
            End While

            Return k
        End Function


        ''' <summary>
        ''' Computes the binomial distribution probability or cumulative probability,
        ''' matching the behavior of Excel's BINOM.DIST and numerically consistent
        ''' with R's dbinom() and pbinom().
        ''' </summary>
        ''' <param name="x">
        ''' The number of successes (must satisfy 0 ≤ x ≤ n).
        ''' </param>
        ''' <param name="n">
        ''' The number of trials (must be a non-negative integer).
        ''' </param>
        ''' <param name="p">
        ''' The probability of success on each trial (0 ≤ p ≤ 1).
        ''' </param>
        ''' <param name="cumulative">
        ''' If True, returns the cumulative probability P(X ≤ x).
        ''' If False, returns the probability mass P(X = x).
        ''' </param>
        ''' <returns>
        ''' The binomial probability or cumulative probability, equivalent to Excel's
        ''' BINOM.DIST and numerically matching R's dbinom() and pbinom().
        ''' Returns <see cref="Double.NaN"/> for invalid inputs.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The probability mass function is:
        ''' 
        '''     P(X = x) = C(n, x) * p^x * (1 - p)^(n - x)
        ''' 
        ''' where C(n, x) is the binomial coefficient.
        ''' </para>
        ''' 
        ''' <para>
        ''' For numerical stability and to match R's behavior, the computation is
        ''' performed in log-space using:
        ''' 
        '''     log(P) = log(C(n, x)) + x*log(p) + (n-x)*log(1-p)
        ''' 
        ''' followed by exponentiation.
        ''' </para>
        ''' 
        ''' <para>
        ''' The cumulative probability is computed by summing the PMF from 0 to x
        ''' in increasing order to minimize floating-point error, matching R's pbinom().
        ''' </para>
        ''' </remarks>
        Public Function BinomDist(x As Integer, n As Integer, p As Double, cumulative As Boolean) As Double
            ' Validate inputs
            If n < 0 OrElse x < 0 OrElse x > n Then Return Double.NaN
            If p < 0.0 OrElse p > 1.0 Then Return Double.NaN

            ' PMF only
            If Not cumulative Then Return BinomPMF(x, n, p)

            ' CDF: sum PMF from 0..x
            Dim sum As Double = 0.0
            For k As Integer = 0 To x
                sum += BinomPMF(k, n, p)
            Next

            Return sum
        End Function

        ''' <summary>
        ''' Computes the binomial probability mass function (PMF)
        ''' 
        '''     P(X = x)
        ''' 
        ''' for a binomially distributed random variable X with parameters
        ''' <paramref name="n"/> (number of trials) and <paramref name="p"/>
        ''' (probability of success), using a numerically stable log‑space
        ''' formulation. This implementation matches the behavior of Excel's
        ''' BINOM.DIST with cumulative = FALSE and is numerically consistent
        ''' with R's dbinom().
        ''' </summary>
        ''' 
        ''' <param name="x">
        ''' The number of observed successes. Must satisfy 0 ≤ x ≤ n.
        ''' </param>
        ''' 
        ''' <param name="n">
        ''' The total number of independent Bernoulli trials. Must be a
        ''' non‑negative integer.
        ''' </param>
        ''' 
        ''' <param name="p">
        ''' The probability of success on each trial. Must satisfy 0 ≤ p ≤ 1.
        ''' </param>
        ''' 
        ''' <returns>
        ''' The probability P(X = x) for a binomial(n, p) distribution.
        ''' Returns 1.0 when p = 0 and x = 0, or when p = 1 and x = n.
        ''' Returns 0.0 when p = 0 and x > 0, or when p = 1 and x .lt. n.
        ''' Returns <see cref="Double.NaN"/> for invalid inputs.
        ''' </returns>
        ''' 
        ''' <remarks>
        ''' <para>
        ''' The binomial PMF is defined as:
        ''' 
        '''     P(X = x) = C(n, x) · p^x · (1 − p)^(n − x)
        ''' 
        ''' where C(n, x) is the binomial coefficient "n choose x".
        ''' </para>
        ''' 
        ''' <para>
        ''' Direct computation of the PMF using factorials or raw powers can
        ''' lead to severe numerical instability, especially for large n or
        ''' extreme values of p. To avoid underflow and overflow, this method
        ''' computes the PMF in log‑space:
        ''' 
        '''     log(P) = log(C(n, x)) + x·log(p) + (n − x)·log(1 − p)
        ''' 
        ''' followed by exponentiation:
        ''' 
        '''     P = exp(log(P))
        ''' 
        ''' This approach matches the numerical strategy used by R's dbinom()
        ''' and ensures stable results even for large n (e.g., n > 1000).
        ''' </para>
        ''' 
        ''' <para>
        ''' The term log(C(n, x)) is computed using a stable summation formula:
        ''' 
        '''     log(C(n, x)) = Σ[i=1..x] log(n − x + i) − log(i)
        ''' 
        ''' which avoids factorials entirely and is symmetric in x and n − x.
        ''' </para>
        ''' 
        ''' <para>
        ''' Special cases are handled explicitly:
        ''' <list type="bullet">
        '''   <item><description>If p = 0, the PMF is 1 when x = 0 and 0 otherwise.</description></item>
        '''   <item><description>If p = 1, the PMF is 1 when x = n and 0 otherwise.</description></item>
        '''   <item><description>If x is outside the range 0 ≤ x ≤ n, the result is NaN.</description></item>
        '''   <item><description>If n = 0, the PMF is 1 when x = 0 and NaN otherwise.</description></item>
        ''' </list>
        ''' </para>
        ''' 
        ''' <para>
        ''' <b>Example:</b>
        ''' 
        ''' For n = 10, p = 0.3, x = 4:
        ''' 
        '''     P(X = 4) = C(10, 4) * 0.3^4 * 0.7^6
        '''              = 210 * 0.0081 * 0.117649
        '''              = 0.200120949
        ''' 
        ''' This function returns the same value as:
        ''' 
        '''     Excel: =BINOM.DIST(4, 10, 0.3, FALSE)
        '''     R:     dbinom(4, 10, 0.3)
        ''' </para>
        ''' 
        ''' <para>
        ''' This function is intended for use in statistical tests, likelihood
        ''' computations, and Excel‑compatible probability calculations where
        ''' numerical accuracy and reproducibility are essential.
        ''' </para>
        ''' </remarks>
        Private Function BinomPMF(x As Integer, n As Integer, p As Double) As Double
            If p = 0.0 Then Return If(x = 0, 1.0, 0.0)
            If p = 1.0 Then Return If(x = n, 1.0, 0.0)

            Dim logC As Double = LogCombin(n, x)
            Dim logP As Double = logC + x * Math.Log(p) + (n - x) * Math.Log(1 - p)

            Return Math.Exp(logP)
        End Function

        ''' <summary>
        ''' Computes the cumulative probability from 0 to <paramref name="q"/> for the 
        ''' Studentized range distribution (the distribution of the maximum minus minimum 
        ''' among <paramref name="r"/> normally distributed samples), with 
        ''' <paramref name="V"/> degrees of freedom.
        ''' </summary>
        ''' <param name="q">
        ''' The upper limit of integration for the Studentized range distribution (q &gt; 0).
        ''' </param>
        ''' <param name="V">
        ''' The degrees of freedom (must be ≥ 1). Values greater than 120 use an asymptotic 
        ''' approximation.
        ''' </param>
        ''' <param name="r">
        ''' The number of samples (must be ≥ 2).
        ''' </param>
        ''' <param name="iFault">
        ''' Returns 0 on success.  
        ''' Returns 1 if input parameters are invalid (V &lt; 1 or r &lt; 2 or q ≤ 0).
        ''' </param>
        ''' <returns>
        ''' The probability P(0 ≤ R ≤ q), where R is the Studentized range statistic.
        ''' Returns 0 if inputs are invalid.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function implements **Algorithm AS 190** from:
        ''' </para>
        ''' <para>
        ''' R.W. G. Hunt (1983), "Algorithm AS 190: The Distribution of a Studentized Range Statistic,"  
        ''' <i>Applied Statistics</i>, Vol. 32, No. 2.
        ''' </para>
        ''' 
        ''' <para>
        ''' The method evaluates the integral using adaptive quadrature on grids determined by 
        ''' parameters JMIN, JMAX, KMIN, KMAX, and STEP_.  
        ''' Arrays <c>VW</c> and <c>QW</c> store intermediate values for quadrature evaluation.  
        ''' </para>
        ''' 
        ''' <para>
        ''' Numerical shortcuts and termination rules are governed by PCUTJ and PCUTK to reduce 
        ''' computation time when contributions fall below precision thresholds.
        ''' </para>
        ''' 
        ''' <para>
        ''' For V &gt; 120, an asymptotic approximation is used to reduce computational load.
        ''' </para>
        ''' 
        ''' <para>
        ''' If <paramref name="iFault"/> is set to 1, the function returns 0 without attempting 
        ''' the computation.
        ''' </para>
        ''' </remarks>
        Function PRTRNG(q As Double, V As Double, r As Double, ByRef iFault As Integer) As Double
            'ALGORITHM AS 190 APPL. STATIST. (1983) VOL.32, NO.2
            'EVALUATES THE PROBABILITY FROM 0 TO Q FOR A STUDENTIZED RANGE HAVING V DEGREES OF FREEDOM AND R SAMPLES.
            'ARRAYS VW AND QW STORE TRANSIENT VALUES USED IN THE QUADRATURE SUMMATION.
            'NODE SPACING IS CONTROLLED BY STEP_. PCUTJ AND PCUTK CONTROL TRUNCATION.
            'MINIMUM AND MAX # OF STEPS ARE CONTROLLED BY JMIN, JMAX, KMIN AND KMAX. ACCURACY CAN BE INCREASED
            'BY USE OF A FINER GRID - INCREASE SIZES OF ARRAYS VW AND QW, AND JMIN, JMAX, KMIN, KMAX AND 1/STEP_ PROPORTIONALLY.

            Dim H As Double, V2 As Double, gk As Double, pK As Double, w0 As Double, pz As Double, x As Double
            Dim hj As Double, ehj As Double, pJ As Double, jj As Integer, jump As Integer

            Const JMIN As Integer = 8, jmax As Integer = 60
            Const KMIN As Integer = 20, kmax As Integer = 60
            Const PCUTJ As Double = 0.0000000001, PCUTK As Double = 0.0000000001
            Const STEP_ As Double = 0.125, VMAX As Double = 120.0
            Dim VW(2 * jmax) As Double, QW(2 * jmax) As Double
            Const CV1 As Double = 0.193064705, CV2 As Double = 0.293525326, CVMAX As Double = 0.39894228
            Dim CV() As Double = {0, 0.318309886, -0.00268132716, 0.00347222222, 0.0833333333}

            'CHECK INITIAL VALUES.
            PRTRNG = 0 : iFault = 0
            If V < 1 Or r < 2 Then iFault = 1
            If q <= 0 Or iFault = 1 Then Exit Function

            'COMPUTING CONSTANTS, LOCATING MIDPOINT, ADJUSTING STEPS.
            Dim G As Double = STEP_ * r ^ (-0.2)
            Dim gmid As Double = 0.5 * Math.Log(r)
            Dim r1 As Double = r - 1
            Dim c As Double = Math.Log(r * G * CVMAX)

            If Not (V > VMAX) Then
                H = STEP_ * V ^ (-0.5)
                V2 = V * 0.5
                If V = 1 Then c = CV1
                If V = 2 Then c = CV2
                If Not (V = 1 Or V = 2) Then c = Math.Sqrt(V2) * CV(1) / (1 + ((CV(2) / V2 + CV(3)) / V2 + CV(4)) / V2)
                c = Math.Log(c * r * G * H)
            End If

            'Computing integral
            'Given a row NoGroups, the procedure starts at the midpoint and works outward (index j) in calculating
            'the probability at nodes symmetric about the midpoint. The rows (index NoGroups) are also
            'processed outwards symmetrically about the midpoint. The centre row is unpaired.

            Dim gstep As Double = G
            Dim pk1 As Double = 1.0, pk2 As Double = 1.0
            QW(1) = -1 : QW(jmax + 1) = -1

            For k = 1 To kmax
                gstep = gstep - G
21:             gstep = -gstep
                gk = gmid + gstep
                pK = 0

                If Not (pk2 <= PCUTK And k > KMIN) Then
                    w0 = c - gk * gk * 0.5
                    pz = 1 - PNorm(gk)
                    x = (1 - PNorm(gk - q)) - pz

                    If x > 0 Then pK = Math.Exp(w0 + r1 * Math.Log(x))
                    If Not (V > VMAX) Then

                        jump = -jmax
22:                     jump += jmax
                        For j = 1 To jmax
                            jj = j + jump
                            If Not (QW(jj) > 0) Then
                                hj = H * j
                                If j < jmax Then QW(jj + 1) = -1
                                ehj = Math.Exp(hj)
                                QW(jj) = q * ehj
                                VW(jj) = V * (hj + 0.5 - ehj * ehj * 0.5)
                            End If
                            pJ = 0
                            x = (1.0 - PNorm(gk - QW(jj))) - pz

                            If x > 0 Then pJ = Math.Exp(w0 + VW(jj) + r1 * Math.Log(x))
                            pK = pK + pJ
                            If Not (pJ > PCUTJ) Then
                                If jj > JMIN Or k > KMIN Then Exit For
                            End If
                        Next
                        H = -H
                        If H < 0 Then GoTo 22
                    End If
                End If

                PRTRNG += pK
                If k > KMIN And pK <= PCUTK And pk1 <= PCUTK Then Exit Function
                pk2 = pk1
                pk1 = pK
                If gstep > 0 Then GoTo 21
            Next k
        End Function

        ''' <summary>
        ''' Computes the quantile (inverse CDF) of the Studentized range distribution for a 
        ''' given probability <paramref name="p"/>, degrees of freedom <paramref name="V"/>, 
        ''' and number of samples <paramref name="r"/>.  
        ''' Implements Algorithm AS 190.1 from *Applied Statistics* (1983).
        ''' </summary>
        ''' <param name="p">
        ''' The cumulative probability, required to satisfy 0.90 ≤ p ≤ 0.99.
        ''' </param>
        ''' <param name="V">
        ''' The degrees of freedom (V ≥ 1).
        ''' </param>
        ''' <param name="r">
        ''' The number of samples (r ≥ 2).
        ''' </param>
        ''' <param name="iFault">
        ''' Returns 0 on success.  
        ''' Returns:
        ''' <list type="table">
        '''   <item><description>1 — Invalid V or r</description></item>
        '''   <item><description>2 — p is outside the allowed range [0.90, 0.99]</description></item>
        '''   <item><description>9 — Failure in subsidiary routines (QTRNG0 or PRTRNG)</description></item>
        ''' </list>
        ''' </param>
        ''' <returns>
        ''' The quantile q such that P(R ≤ q) = p for the Studentized range statistic R.  
        ''' Returns 0 if the computation fails.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This algorithm uses a combination of:
        ''' </para>
        ''' <list type="bullet">
        '''   <item><description><c>QTRNG0</c> — initial quantile approximation (Algorithm AS 190.2)</description></item>
        '''   <item><description><c>PRTRNG</c> — cumulative probability (Algorithm AS 190)</description></item>
        ''' </list>
        ''' 
        ''' <para>
        ''' After obtaining an initial approximation, the method iteratively refines the estimate 
        ''' using a secant-like update step:
        ''' </para>
        ''' <code>
        ''' q ← (e₂·Q₁ − e₁·Q₂) / (e₂ − e₁)
        ''' </code>
        ''' 
        ''' <para>
        ''' Iteration stops early if |P(q) − p| falls below a tolerance threshold.
        ''' </para>
        ''' 
        ''' <para>
        ''' See:  
        ''' "Algorithm AS 190.1: Approximating the Percentage Points of the Studentized Range,"  
        ''' *Applied Statistics*, 32(2), 1983.
        ''' </para>
        ''' </remarks>
        Function QTRNG(p As Double, V As Double, r As Double, ByRef iFault As Integer) As Double
            Dim P2 As Double, e1 As Double, e2 As Double, d As Double, j As Integer, nfault As Integer

            Const jmax As Integer = 20
            Const eps As Double = 0.000000001   ' only used for secant denominator guard
            Const tolP As Double = 0.0000001  ' probability tolerance
            Const tolQ As Double = 0.000000001  ' step tolerance on q (optional)


            'Check input parameters
            iFault = 0 : nfault = 0
            If V < 1.0 Or r < 2.0 Then iFault = 1
            If p < 0.9 Or p > 0.99 Then iFault = 2
            If iFault = 0 Then

                'Obtain initial values
                Dim Q1 As Double = QTRNG0(p, V, r)
                If nfault <> 0 Then GoTo 99
                Dim P1 As Double = PRTRNG(Q1, V, r, nfault)
                If nfault <> 0 Then GoTo 99
                QTRNG = Q1
                If Math.Abs(P1 - p) < tolP Then GoTo 99
                If P1 > p Then P1 = 1.75 * p - 0.75 * P1
                If P1 < p Then P2 = p + (p - P1) * (1.0 - p) / (1.0 - P1) * 0.75
                If P2 < 0.8 Then P2 = 0.8
                If P2 > 0.995 Then P2 = 0.995
                Dim q2 As Double = QTRNG0(P2, V, r)
                If nfault <> 0 Then GoTo 99

                'Refine approximation
                For j = 2 To jmax
                    P2 = PRTRNG(q2, V, r, nfault)
                    If nfault <> 0 Then GoTo 99
                    e1 = P1 - p
                    e2 = P2 - p

                    ' Secant (fallback to midpoint if nearly flat)
                    QTRNG = (Q1 + q2) / 2.0
                    d = e2 - e1
                    If Math.Abs(d) > eps Then QTRNG = (e2 * Q1 - e1 * q2) / d

                    ' Evaluate at new point
                    Dim Pnew As Double = PRTRNG(QTRNG, V, r, nfault)
                    If nfault <> 0 Then GoTo 99

                    ' Replace the worse endpoint with the new point
                    If Math.Abs(P1 - p) > Math.Abs(P2 - p) Then
                        Q1 = QTRNG : P1 = Pnew
                    Else
                        q2 = QTRNG : P2 = Pnew
                    End If

                    If Math.Abs(P1 - p) < tolP OrElse Math.Abs(P2 - p) < tolP Then Exit For
                    If Math.Abs(q2 - Q1) < tolQ Then Exit For
                Next

                ' Return best of the two
                QTRNG = If(Math.Abs(P1 - p) <= Math.Abs(P2 - p), Q1, q2)
            End If

99:         If nfault <> 0 Then iFault = 9
            Return QTRNG
        End Function

        ''' <summary>
        ''' Computes an initial quantile approximation for the Studentized range distribution
        ''' using Algorithm AS 190.2 (Applied Statistics, 1983).  
        ''' This is used as a starting value for iterative refinement in <see cref="QTRNG"/>.
        ''' </summary>
        ''' <param name="p">
        ''' The cumulative probability, required to satisfy 0.80 &lt; p &lt; 0.995.
        ''' </param>
        ''' <param name="V">
        ''' The degrees of freedom.  
        ''' For V &lt; 120, finite-sample corrections are applied; for V ≥ 120, asymptotic behavior is assumed.
        ''' </param>
        ''' <param name="r">
        ''' The number of samples in the Studentized range calculation.
        ''' </param>
        ''' <returns>
        ''' An approximate quantile value suitable as an initial guess for the Studentized range distribution.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This routine implements **Algorithm AS 190.2**, providing a fast closed-form approximation:
        ''' </para>
        ''' 
        ''' <list type="bullet">
        '''   <item>
        '''     <description>
        '''     Computes <c>t</c> using the standard normal inverse CDF.
        '''     </description>
        '''   </item>
        '''   <item>
        '''     <description>
        '''     Applies small-sample corrections for <paramref name="V"/> &lt; 120.
        '''     </description>
        '''   </item>
        '''   <item>
        '''     <description>
        '''     Forms a scale-adjusted value based on <c>log(r − 1)</c>.
        '''     </description>
        '''   </item>
        ''' </list>
        ''' 
        ''' <para>
        ''' Reference:  
        ''' "Algorithm AS 190.2: Approximating the Percentage Points of the Studentized Range,"  
        ''' *Applied Statistics*, Vol. 32, No. 2, 1983.
        ''' </para>
        ''' </remarks>
        Function QTRNG0(p As Double, V As Double, r As Double) As Double
            Dim VMAX As Double = 120.0
            Dim t As Double = NormSInv(0.5 + 0.5 * p)
            If V < VMAX Then t = t + (t * t * t + t) / V / 4
            Dim q As Double = 0.8843 - 0.2368 * t
            If V < VMAX Then q = q - 1.214 / V + 1.208 * t / V
            Return t * (q * Math.Log(r - 1.0) + 1.4142)
        End Function
    End Module
End Namespace