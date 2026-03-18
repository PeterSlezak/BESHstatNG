Imports System.Drawing
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace assumptions


    Public Module Assumptions

        ''' <summary>
        ''' Performs the Shapiro–Wilk test for normality on a univariate dataset.
        ''' </summary>
        ''' <param name="data">
        ''' A one-dimensional array of doubles containing the sample data.
        ''' </param>
        ''' <param name="strErr">
        ''' A string passed by reference that will contain error messages if the test cannot be performed
        ''' (e.g., sample size too small, zero range, or too large sample size).
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the Shapiro–Wilk W statistic.</description></item>
        ''' <item><description><c>Pvalue</c>: the p-value for the test of normality.</description></item>
        ''' </list>
        ''' Returns <c>Nothing</c> if the test cannot be performed due to invalid sample size or data conditions.
        ''' </returns>
        ''' <remarks>
        ''' - Based on Algorithm AS R94, *Applied Statistics* (1995), Vol. 44, No. 4.  
        ''' - Tests the null hypothesis that the data are normally distributed.  
        ''' - Valid for sample sizes 3 ≤ n ≤ 5000.  
        ''' - For n &lt; 3 or n &gt; 5000, the test is not computed.  
        ''' - For very small samples, exact p-values are used; for larger samples, approximations are applied.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: test normality of a dataset
        ''' Dim data() As Double = {1.2, 2.3, 2.1, 1.9, 2.0, 2.2}
        ''' Dim errMsg As String = ""
        ''' Dim result As TestResult = ShapiroWilk(data, errMsg)
        ''' If result Is Nothing Then
        '''     Console.WriteLine("Error: "  errMsg)
        ''' Else
        '''     Console.WriteLine("W statistic: "  result.TestStatistics1)
        '''     Console.WriteLine("p-value: "  result.Pvalue)
        ''' End If
        ''' </example>
        Public Function ShapiroWilk(data() As Double, ByRef strErr As String) As TestResult
            ' if Arg = 0 function return W statistics, if higher (> 0) return p-value
            ' Function use algorythm AS R94 APPL. STATIST. (1995) VOL.44, NO.4
            Dim out = New TestResult, x() As Double
            Dim i As Integer, i1 As Integer
            Dim a() As Double, W As Double 'the p-value of W and the  W-statistis
            Dim AN25 As Double, SUMM2 As Double, SSUMM2 As Double, RSN As Double, a1 As Double, a2 As Double, fac As Double
            Dim xi As Double, ASA As Double, XSX As Double, w1 As Double, y As Double, m As Double, S As Double, Gamma As Double

            strErr = String.Empty
            Dim n As Double = data.Length
            ReDim a(n), x(n)
            'original code use 1 based arrays. Shift data by 1
            Array.Sort(data)
            For i = 0 To n - 1
                x(i + 1) = data(i)
            Next

            ' Here start algorythm AS R94 APPL. STATIST. (1995) VOL.44, NO.4
            ' Calculates the Shapiro-Wilk W test and its significance level
            Dim PW As Double = 1.0
            If W >= 0 Then W = 1

            Dim an As Double = n
            Dim n2 As Integer = n / 2
            Dim nn2 As Integer = n2

            If n Mod 2 = 0 Then ' N2 = 1/2N if N is even, 1/2(N-1) if N is odd
                n2 = n / 2
            Else
                n2 = (n - 1) / 2
            End If

            ' IF N2 < NN2 Then RETURN
            If n < 3 Then
                strErr = "Small sample size (N < 3); For further information see Remark  AS R94."
                Return Nothing
            End If

            Select Case n
                Case Is = 3
                    a(1) = 0.70711
                Case Is < 5 ' statsdirect doesn't have this option
                    AN25 = an + 0.25
                    SUMM2 = 0.0
                    For i = 1 To n2
                        a(i) = distributions.NormSInv((i - 0.375) / AN25)
                        SUMM2 += a(i) * a(i)
                    Next

                    SUMM2 = SUMM2 * 2
                    SSUMM2 = Math.Sqrt(SUMM2)
                    RSN = 1 / (an ^ 0.5)
                    a1 = ((0.221157 * RSN) - 0.147981 * RSN ^ 2 - 2.07119 * RSN ^ 3 + 4.434685 * RSN ^ 4 - 2.706056 * RSN ^ 5) - (a(1) / SSUMM2)
                    a2 = -a(2) / SSUMM2 + (0.042981 * RSN - 0.293762 * RSN ^ 2 - 1.752461 * RSN ^ 3 + 5.682633 * RSN ^ 4 - 3.582633 * RSN ^ 5)
                    i1 = 3
                    fac = ((SUMM2 - 2.0 * a(1) ^ 2) / (1.0 - 2.0 * a1 ^ 2)) ^ 2
                    a(1) = a1 : a(2) = a2
                    For i = i1 To nn2
                        a(i) = -a(i) / fac
                    Next
                Case Else
                    AN25 = an + 0.25
                    SUMM2 = 0
                    For i = 1 To n2
                        a(i) = distributions.NormSInv((i - 0.375) / AN25)
                        SUMM2 += a(i) * a(i)
                    Next

                    SUMM2 = SUMM2 * 2
                    SSUMM2 = Math.Sqrt(SUMM2)
                    RSN = 1 / Math.Sqrt(an)
                    a1 = ((0.221157 * RSN) - 0.147981 * RSN ^ 2 - 2.07119 * RSN ^ 3 + 4.434685 * RSN ^ 4 - 2.706056 * RSN ^ 5) - (a(1) / SSUMM2)
                    a2 = -a(2) / SSUMM2 + (0.042981 * RSN - 0.293762 * RSN ^ 2 - 1.752461 * RSN ^ 3 + 5.682633 * RSN ^ 4 - 3.582633 * RSN ^ 5)
                    i1 = 3

                    fac = Math.Sqrt((SUMM2 - 2.0 * a(1) ^ 2 - 2.0 * a(2) ^ 2) / (1.0 - 2.0 * a1 ^ 2 - 2.0 * a2 ^ 2))
                    a(1) = a1 : a(2) = a2

                    For i = i1 To nn2
                        a(i) = -a(i) / fac
                    Next
            End Select

            ' If W input as negative, calculate significance level of -W
            If W < 0.0 Then w1 = 1.0 + W

            ' Check for zero range
            Dim Range As Double = x(n) - x(1)
            If Range < 1.0E-19 Then
                strErr = "The  data  have  zero  range. For more info see (Remark AS R94)."
                Return Nothing
            End If

            ' Check for correct sort order on range - scaled X
            Dim xx As Double = x(1) / Range
            Dim sx As Double = xx
            Dim SA As Double = -a(1)
            Dim j As Integer = n - 1
            For i = 2 To n
                xi = x(i) / Range
                sx += xi
                If i > j Then ' originaly: if i<> j then SA = SA + SIGN(1, I - J) * A(MIN(I, J))
                    SA = SA + 1.0 * a(Math.Min(i, j))
                ElseIf i < j Then
                    SA = SA - 1.0 * a(Math.Min(i, j))
                End If
                xx = xi
                j -= 1
            Next

            If n > 5000 Then
                strErr = "Shapiro: Sample size is too large (N > 5000)."
                Return Nothing
            End If

            ' Fit W statistic as squared correlation between data and coefficients
            SA = SA / n
            sx = sx / n
            Dim SSA As Double = 0.0
            Dim SSX As Double = 0.0
            Dim SAX As Double = 0.0
            j = n
            For i = 1 To n
                If i > j Then
                    ASA = 1 * a(Math.Min(i, j)) - SA
                ElseIf i < j Then
                    ASA = -1 * a(Math.Min(i, j)) - SA
                Else
                    ASA = -SA
                End If
                XSX = x(i) / Range - sx
                SSA += ASA * ASA
                SSX += XSX * XSX
                SAX += ASA * XSX
                j -= 1
            Next

            ' W1 equals (1-W) claculated to avoid excessive rounding error
            ' for W very near 1 (a potential problem in very large samples)
            Dim SSASSX As Double = Math.Sqrt(SSA * SSX)
            w1 = (SSASSX - SAX) * (SSASSX + SAX) / (SSA * SSX)
            W = 1.0 - w1

            ' Fit significance level for W (exact for N=3)
            Select Case n ' neriesi n<3 to treba vyriesit na zaciatku
                Case Is = 3
                    PW = 1.909859 * (Math.Asin(Math.Sqrt(W)) - 1.047198)

                    'outputs
                    out.Pvalue = PW
                    out.TestStatistics1 = W
                    Return out
                Case Is <= 11
                    y = Math.Log(w1)
                    xx = Math.Log(an)
                    m = 0.0
                    S = 1.0
                    Gamma = (-2.273 + 0.459 * an)
                    If y >= Gamma Then
                        PW = 1.0E-19

                        'outputs
                        out.Pvalue = PW
                        out.TestStatistics1 = W
                        Return out
                    End If
                    y = -Math.Log(Gamma - y)
                    m = (0.544 - 0.39978 * an + 0.025054 * an ^ 2 - 0.0006714 * an ^ 3)
                    S = Math.Exp(1.3822 - 0.77857 * an + 0.062767 * an ^ 2 - 0.0020322 * an ^ 3)
                Case Is > 11
                    y = Math.Log(w1)
                    xx = Math.Log(an)
                    m = 0.0 : S = 1.0
                    Gamma = (-2.273 + 0.459 * an)
                    m = (-1.5861 - 0.31082 * xx - 0.083751 * xx ^ 2 + 0.0038915 * xx ^ 3)
                    S = Math.Exp(-0.4803 - 0.082676 * xx + 0.0030302 * xx ^ 2)
            End Select

            PW = 1.0 - distributions.PNorm((y - m) / S)
            out.Pvalue = PW
            out.TestStatistics1 = W
            Return out
        End Function

        ''' <summary>
        ''' Performs the D'Agostino–Pearson K² test for normality on a univariate dataset.
        ''' </summary>
        ''' <param name="Values">
        ''' A one-dimensional array of doubles containing the sample data.
        ''' </param>
        ''' <param name="strErr">
        ''' A string passed by reference that will contain error messages if the test cannot be performed
        ''' (e.g., sample size too small).
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the K² test statistic (sum of squared z-scores for skewness and kurtosis).</description></item>
        ''' <item><description><c>Pvalue</c>: the p-value for the test of normality.</description></item>
        ''' </list>
        ''' Returns <c>Nothing</c> if the sample size is less than 9.
        ''' </returns>
        ''' <remarks>
        ''' - Based on D'Agostino and Pearson (1973), *Biometrika*, 60, 613–622.  
        ''' - Extended by D'Agostino, Belanger, and D'Agostino Jr. (1990), *American Statistician*, 44, 316–321.  
        ''' - Tests the null hypothesis that the data are normally distributed.  
        ''' - Valid only for sample sizes n ≥ 9.  
        ''' - Combines skewness and kurtosis into a single omnibus test statistic.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: test normality of a dataset
        ''' Dim data() As Double = {1.2, 2.3, 2.1, 1.9, 2.0, 2.2, 2.5, 2.4, 2.3}
        ''' Dim errMsg As String = ""
        ''' Dim result As TestResult = DAgostino(data, errMsg)
        ''' If result Is Nothing Then
        '''     Console.WriteLine("Error: "  errMsg)
        ''' Else
        '''     Console.WriteLine("K² statistic: "  result.TestStatistics1)
        '''     Console.WriteLine("p-value: "  result.Pvalue)
        ''' End If
        ''' </example>
        Public Function DAgostino(Values() As Double, strErr As String) As TestResult
            Dim out = New TestResult

            'get number of valid values
            'ignore - Empty cells, logical values, text, or error values

            Dim n As Integer = Values.Length

            ' According (American Statistician, 44,3 16-321) the hypothesis based on this test is justiafable only if n >= 9.
            If n < 9 Then
                strErr = "Function DAgostino: Sample size too small n < 9. Inference could be inappropriate. See (American Statistician, 44,3 16-321)."
                Return Nothing
            End If

            Dim Skew As Double = Skewness(Values)
            Dim Kurt As Double = Kurtosis(Values)

            ' compute test statistics for skewness
            Dim y As Double = Skew * Math.Sqrt(((n + 1) * (n + 3)) / (6 * (n - 2)))
            Dim Beta2 As Double = (3.0 * (n ^ 2 + 27 * n - 70) * (n + 1) * (n + 3)) / ((n - 2) * (n + 5) * (n + 7) * (n + 9))
            Dim w2 As Double = Math.Sqrt(2 * (Beta2 - 1)) - 1
            Dim W As Double = Math.Sqrt(w2)
            Dim Delta As Double = 1.0 / Math.Sqrt(Math.Log(W))
            Dim Alpha As Double = Math.Sqrt(2.0 / (w2 - 1.0))
            Dim Zskewness As Double = Delta * Math.Log(y / Alpha + Math.Sqrt((y / Alpha) ^ 2 + 1.0))

            ' compute test statistics for kurtosis
            Dim Eb2 As Double = (3.0 * (n - 1.0)) / (n + 1.0)
            Dim varb2 As Double = (24.0 * n * (n - 2.0) * (n - 3.0)) / (((n + 1.0) ^ 2) * (n + 3.0) * (n + 5.0))
            Dim x As Double = (Kurt - Eb2) / Math.Sqrt(varb2)
            Dim j As Double = (6 * (n ^ 2 - 5.0 * n + 2.0)) / ((n + 7.0) * (n + 9.0)) * Math.Sqrt((6.0 * (n + 3.0) * (n + 5.0)) / (n * (n - 2.0) * (n - 3.0)))
            Dim a As Double = 6.0 + (8.0 / j) * ((2.0 / j) + Math.Sqrt(1.0 + 4.0 / (j ^ 2)))
            Dim Zkurtosis As Double = (((1.0 - 2.0 / (9.0 * a)) - ((1.0 - 2.0 / a) / (1.0 + x * Math.Sqrt(2.0 / (a - 4.0)))) ^ (1 / 3))) / Math.Sqrt(2.0 / (9.0 * a))

            ' compute test statistics K2 and p-value
            out.TestStatistics1 = (Zskewness * Zskewness) + (Zkurtosis * Zkurtosis)
            out.Pvalue = 1.0 - distributions.ChiSquareCDF(out.TestStatistics1, 2) 'two sided test

            Return out
        End Function

        ''' <summary>
        ''' Performs the Anderson–Darling test for normality on a univariate dataset.
        ''' </summary>
        ''' <param name="x">
        ''' A one-dimensional array of doubles containing the sample data.
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the adjusted Anderson–Darling statistic (AD²).</description></item>
        ''' <item><description><c>Pvalue</c>: the p-value for the test of normality.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Tests the null hypothesis that the data come from a normal distribution.  
        ''' - The statistic AD² is adjusted for sample size.  
        ''' - P-values are approximated using piecewise exponential formulas depending on the range of AD².  
        ''' - More sensitive to deviations in the tails than Shapiro–Wilk or Kolmogorov–Smirnov tests.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: test normality of a dataset
        ''' Dim data() As Double = {1.2, 2.3, 2.1, 1.9, 2.0, 2.2}
        ''' Dim result As TestResult = AndersonDarlingTEST(data)
        ''' Console.WriteLine("AD² statistic: "  result.TestStatistics1)
        ''' Console.WriteLine("p-value: "  result.Pvalue)
        ''' </example>
        Public Function AndersonDarlingTEST(x() As Double) As TestResult
            Dim S As Double, P_value As Double
            Dim out = New TestResult
            Dim n As Integer = x.Length
            Dim F1(n - 1) As Double, F2(n - 1) As Double
            Dim Mean As Double = x.Average()
            Dim sd As Double = stDev(x)
            Array.Sort(x)

            For i = 0 To n - 1
                F1(i) = distributions.PNorm(x(i), Mean, sd)
            Next


            For i = 0 To n - 1
                F2(i) = 1 - F1(n - 1 - i)
                S += ((2.0 * (i + 1) - 1) * (Math.Log(F1(i)) + Math.Log(F2(i))))
            Next

            Dim AD As Double = -n - S / n
            Dim AD2 As Double = AD * (1.0 + 0.75 / n + 2.25 / n ^ 2)

            If AD2 >= 0.6 Then
                P_value = Math.Exp(1.2937 - 5.709 * AD2 + 0.0186 * AD2 ^ 2)
            ElseIf (AD2 < 0.6 And AD2 >= 0.34) Then
                P_value = Math.Exp(0.9177 - 4.279 * AD2 - 1.38 * AD2 ^ 2)
            ElseIf (AD2 < 0.34 And AD2 >= 0.2) Then
                P_value = 1 - Math.Exp(-8.318 + 42.796 * AD2 - 59.938 * AD2 ^ 2)
            ElseIf AD2 < 0.2 Then
                P_value = 1 - Math.Exp(-13.436 + 101.14 * AD2 - 223.73 * AD2 ^ 2)
            End If

            out.TestStatistics1 = AD2
            out.Pvalue = P_value
            Return out
        End Function

        ''' <summary>
        ''' Performs Box's M test for equality of covariance matrices across multiple groups.
        ''' </summary>
        ''' <param name="Cov_mat">
        ''' A three-dimensional array of doubles containing covariance matrices for each group:
        ''' <list type="bullet">
        ''' <item><description>Dimension 1: group index (0..g-1).</description></item>
        ''' <item><description>Dimension 2: row index of covariance matrix.</description></item>
        ''' <item><description>Dimension 3: column index of covariance matrix.</description></item>
        ''' </list>
        ''' Each covariance matrix must be square (p × p).
        ''' </param>
        ''' <param name="SampleSizes">
        ''' An integer array containing the sample size for each group.
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the Box's M test statistic.</description></item>
        ''' <item><description><c>Pvalue</c>: the p-value from the F-distribution approximation.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Tests the null hypothesis that all groups have equal covariance matrices.  
        ''' - Uses pooled covariance matrix and determinants of group covariance matrices.  
        ''' - Approximates significance using an F-distribution with degrees of freedom based on matrix dimension and group count.  
        ''' - Throws exceptions if input dimensions are inconsistent.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: Box's M test with 2 groups, each with 2×2 covariance matrix
        ''' Dim Cov_mat(1,1,1) As Double
        ''' ' Group 1 covariance matrix
        ''' Cov_mat(0,0,0) = 1.0 : Cov_mat(0,0,1) = 0.2
        ''' Cov_mat(0,1,0) = 0.2 : Cov_mat(0,1,1) = 0.9
        ''' ' Group 2 covariance matrix
        ''' Cov_mat(1,0,0) = 1.1 : Cov_mat(1,0,1) = 0.25
        ''' Cov_mat(1,1,0) = 0.25 : Cov_mat(1,1,1) = 1.0
        ''' Dim SampleSizes() As Integer = {30, 35}
        '''
        ''' Dim result As TestResult = BoxM(Cov_mat, SampleSizes)
        ''' Console.WriteLine("Box's M statistic: "  result.TestStatistics1)
        ''' Console.WriteLine("p-value: "  result.Pvalue)
        ''' </example>
        Public Function BoxM(Cov_mat(,,) As Double, SampleSizes() As Integer) As TestResult
            'Box test for equality of covariance matrix
            ' Cov_mat is 3D array - one covariance matrix (p x p) for each group (1st dimension)
            ' SampleSizes - sample size for each group
            Dim out = New TestResult, test_stat As Double, nn As Double, nn2 As Double

            Dim n_grp As Integer = Cov_mat.GetLength(0)
            Dim p As Integer = Cov_mat.GetLength(1)
            ' Cov_mat is (group, row, col)
            If Cov_mat.GetLength(1) <> Cov_mat.GetLength(2) Then
                AppGlobals.BSerr.LogAndThrow(New ApplicationException("Error: Box test require the same dimenstions of Covariance matrix for each group."))
            End If
            If n_grp <> SampleSizes.Length Then
                AppGlobals.BSerr.LogAndThrow(New ApplicationException("Error: Box test - incorrect input array dimenstions."))
            End If

            Dim cov_pooled(p - 1, p - 1) As Double, tmp(p - 1, p - 1) As Double
            Dim tot As Double = SampleSizes.Sum()
            For i = 0 To p - 1
                For j = 0 To p - 1
                    For k = 0 To n_grp - 1
                        cov_pooled(i, j) = cov_pooled(i, j) + Cov_mat(k, i, j) * (SampleSizes(k) - 1)
                    Next k
                    cov_pooled(i, j) = cov_pooled(i, j) * (1 / (tot - n_grp))
                Next
            Next

            For k = 0 To n_grp - 1
                ' get covariance matrix for the NoGroups-th group
                For i = 0 To p - 1
                    For j = 0 To p - 1
                        tmp(i, j) = Cov_mat(k, i, j)
                    Next
                Next
                test_stat += (Math.Log(Matrix.MDeterm(tmp)) * (SampleSizes(k) - 1))
                nn += (1.0 / (SampleSizes(k) - 1))
                nn2 += ((1.0 / (SampleSizes(k) - 1)) ^ 2)
            Next
            test_stat = (Math.Log(Matrix.MDeterm(cov_pooled)) * (tot - n_grp)) - test_stat
            nn -= (1.0 / (tot - n_grp))
            nn2 -= (1.0 / (tot - n_grp)) ^ 2

            'Compute p-value using F distribution
            Dim c As Double = (2.0 * p ^ 2 + 3.0 * p - 1.0) / (6.0 * (p + 1) * (n_grp - 1)) * nn
            Dim c2 As Double = (p - 1) * (p + 2) / (6 * (n_grp - 1)) * nn2
            Dim df As Double = p * (p + 1) * (n_grp - 1) / 2.0
            Dim df2 As Double = (df + 2) / Math.Abs(c2 - c ^ 2)
            Dim a1 As Double = df / (1 - c - df / df2)
            Dim a2 As Double = df2 / (1 - c + 2 / df2)
            Dim F1 As Double = test_stat / a1
            Dim F2 As Double = df2 * test_stat / (df * (a2 - test_stat))
            Dim F As Double = If(c2 > c ^ 2, F1, F2)

            out.TestStatistics1 = test_stat
            out.Pvalue = distributions.F_RT(F, df, df2)
            Return out
        End Function

        ''' <summary>
        ''' Performs the Fligner–Killeen test for homogeneity of variances across multiple groups.
        ''' </summary>
        ''' <param name="arDataColumn">
        ''' A jagged array of doubles, where each inner array represents the sample values for one group.
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the chi-square test statistic for variance equality.</description></item>
        ''' <item><description><c>Pvalue</c>: the p-value for the test of homogeneity of variances.</description></item>
        ''' </list>
        ''' Returns an empty <see cref="TestResult"/> if only one group is provided.
        ''' </returns>
        ''' <remarks>
        ''' - The Fligner–Killeen test is a nonparametric test for equality of variances.  
        ''' - It is robust against departures from normality.  
        ''' - Uses ranks of absolute deviations from group medians, transformed via the normal quantile function.  
        ''' - The test statistic is compared to a chi-square distribution with k − 1 degrees of freedom.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: compare variances across 3 groups
        ''' Dim group1() As Double = {5.1, 4.9, 5.0, 5.2}
        ''' Dim group2() As Double = {6.0, 6.1, 5.9, 6.2}
        ''' Dim group3() As Double = {5.5, 5.6, 5.4, 5.7}
        ''' Dim data()() As Double = {group1, group2, group3}
        '''
        ''' Dim result As TestResult = FlignerKilleenTEST(data)
        ''' Console.WriteLine("Chi-square statistic: " + result.TestStatistics1)
        ''' Console.WriteLine("p-value: " + result.Pvalue)
        ''' </example>
        Public Function FlignerKilleenTEST(arDataColumn()() As Double) As TestResult

            Dim out = New TestResult
            Dim Aktualna As Double, Poradie As Double, Numerator As Double
            Dim Medians() As Double, Temporal() As Double, diffs() As Double, Ranks() As Double
            Dim j As Integer, jj As Integer, a As Double, ai() As Double, V2 As Double

            Dim k As Integer = arDataColumn.Length 'find out # of groups
            Dim arNs(k - 1) As Integer
            For i = 0 To k - 1
                arNs(i) = arDataColumn(i).Length
            Next
            Dim n As Integer = arNs.Sum() 'total sample size
            ReDim Medians(k - 1), diffs(n - 1), Ranks(n - 1), ai(k - 1)

            'Check assumptions
            If k <= 1 Then Return out 'only one group

            'compute absolute differences between respective group and group median
            Dim ii As Integer = 0
            For i = 0 To k - 1
                Temporal = arDataColumn(i)
                Medians(i) = Median(Temporal) 'compute medians for i-th group
                For j = 0 To arNs(i) - 1
                    diffs(ii) = Math.Abs(arDataColumn(i)(j) - Medians(i))
                    ii += 1
                Next
            Next

            'compute ranks
            ii = 0 : jj = 0
            j = arNs(0)
            Do While ii <= n - 1
                Poradie = n
                Aktualna = diffs(ii) 'take a value
                For i = 0 To n - 1
                    If i <> ii Then
                        If Aktualna < diffs(i) Then
                            Poradie = Poradie - 1.0
                        ElseIf Aktualna = diffs(i) Then 'tied ranks
                            Poradie = Poradie - 0.5
                        End If
                    End If
                Next
                Ranks(ii) = distributions.NormSInv(0.5 + Poradie / (2.0 * (n + 1)))

                If ii < j Then
                    ai(jj) += Ranks(ii)
                Else
                    jj += 1
                    j += arNs(jj)
                    ai(jj) += Ranks(ii)
                End If
                ii += 1
            Loop
            a = Ranks.Average()
            V2 = variance(Ranks)


            For i = 0 To k - 1 'calculate mean ranks for respecitve groups
                ai(i) = ai(i) / arNs(i)
                Numerator += (arNs(i) * (a - ai(i)) ^ 2)
            Next

            out.TestStatistics1 = Numerator / V2
            out.Pvalue = 1.0 - distributions.ChiSquareCDF(out.TestStatistics1, k - 1)
            Return out
        End Function

        ''' <summary>
        ''' Performs Levene's test (or the Brown–Forsythe modification) for equality of variances across groups.
        ''' </summary>
        ''' <param name="arDataColumn">
        ''' A jagged array of doubles, where each inner array represents the sample values for one group.
        ''' </param>
        ''' <param name="bW50">
        ''' A Boolean flag specifying the test variant:
        ''' <list type="bullet">
        ''' <item><description><c>True</c>: Brown–Forsythe modification (uses group medians).</description></item>
        ''' <item><description><c>False</c>: Classical Levene's test (uses group means).</description></item>
        ''' </list>
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the F statistic from Levene's or Brown–Forsythe test.</description></item>
        ''' <item><description><c>Pvalue</c>: the p-value for the test of homogeneity of variances.</description></item>
        ''' </list>
        ''' Returns an empty <see cref="TestResult"/> if only one group is provided.
        ''' </returns>
        ''' <remarks>
        ''' - Levene's test evaluates the null hypothesis that all groups have equal variances.  
        ''' - The Brown–Forsythe modification uses medians instead of means, making the test more robust to non-normality.  
        ''' - The test statistic is compared to an F distribution with (k − 1, n − k) degrees of freedom.  
        ''' - References:  
        '''   • Levene, H. (1960), *Robust Tests for Equality of Variances*, in I. Olkin (ed.), *Contributions to Probability and Statistics*, Stanford University Press.  
        '''   • Brown, M.B., Forsythe, A.B. (1974), *Robust Tests for the Equality of Variances*, JASA, 69, 364–367.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: compare variances across 3 groups
        ''' Dim group1() As Double = {5.1, 4.9, 5.0, 5.2}
        ''' Dim group2() As Double = {6.0, 6.1, 5.9, 6.2}
        ''' Dim group3() As Double = {5.5, 5.6, 5.4, 5.7}
        ''' Dim data()() As Double = {group1, group2, group3}
        '''
        ''' ' Classical Levene's test (means)
        ''' Dim result1 As TestResult = LeveneTEST(data, False)
        ''' Console.WriteLine("Levene's F statistic: " + result1.TestStatistics1)
        ''' Console.WriteLine("p-value: " + result1.Pvalue)
        '''
        ''' ' Brown–Forsythe modification (medians)
        ''' Dim result2 As TestResult = LeveneTEST(data, True)
        ''' Console.WriteLine("Brown–Forsythe F statistic: " + result2.TestStatistics1)
        ''' Console.WriteLine("p-value: " + result2.Pvalue)
        ''' </example>
        Public Function LeveneTEST(arDataColumn()() As Double, bW50 As Boolean) As TestResult
            'bW50 = true    Brown and Forsythe modification of Levene's test
            'bW50 = false   W statistic and associated p-value of Levene's test
            'Levene, H., "Robust Tests for Equality of Variances," in I. Olkin, ed., Contributions to Probability and Statistics,
            'Palo Alto, Calif.: Stanford University Press , 1960, 278 - 92
            'Brown MB, Forsythe AB. Robust tests for the equality of variances. Journal of the American Statistical Association 1974;69:364-7.
            Dim out = New TestResult

            Dim sum1 As Double, sum2 As Double, Sum3 As Double, Sum4 As Double
            Dim SumNominator1 As Double, SumDenominator1 As Double, SumNominator2 As Double, SumDenominator2 As Double

            Dim k As Integer = arDataColumn.Length 'find out # of groups
            Dim arNs(k - 1) As Integer
            For i = 0 To k - 1
                arNs(i) = arDataColumn(i).Length
            Next
            Dim n As Integer = arNs.Sum() 'total sample size

            Dim means(k - 1) As Double, Medians(k - 1) As Double, Zi1AVG(k - 1) As Double, Zi2AVG(k - 1) As Double

            'Check assumptions
            If k <= 1 Then Return out

            Dim Zmean(arNs.Max() - 1, k - 1) As Double, Zmedian(arNs.Max() - 1, k - 1) As Double

            'calculate difference from groups means and medians and sums required for the test statistics computation
            For i = 0 To k - 1
                Dim Temporal() As Double = arDataColumn(i)
                Medians(i) = Median(Temporal) 'calculate groups medians
                means(i) = Temporal.Average() 'and means
                sum1 = 0 : sum2 = 0
                For j = 0 To arNs(i) - 1
                    Zmean(j, i) = Math.Abs(arDataColumn(i)(j) - means(i))
                    Zmedian(j, i) = Math.Abs(arDataColumn(i)(j) - Medians(i))
                    sum1 += Zmean(j, i)
                    sum2 += Zmedian(j, i)
                Next
                Zi1AVG(i) = sum1 / arNs(i)
                Zi2AVG(i) = sum2 / arNs(i)
                Sum3 += sum1
                Sum4 += sum2
            Next i
            Dim Z1avg As Double = Sum3 / n
            Dim Z2avg As Double = Sum4 / n

            For i = 0 To k - 1
                SumNominator1 += arNs(i) * (Zi1AVG(i) - Z1avg) ^ 2
                SumNominator2 += arNs(i) * (Zi2AVG(i) - Z2avg) ^ 2
                For j = 0 To arNs(i) - 1
                    SumDenominator1 += (Zmean(j, i) - Zi1AVG(i)) ^ 2
                    SumDenominator2 += (Zmedian(j, i) - Zi2AVG(i)) ^ 2
                Next
            Next

            'compute test statistics
            Dim Wmean As Double = (n - k) * SumNominator1 / ((k - 1) * SumDenominator1)
            Dim Wmedian As Double = (n - k) * SumNominator2 / ((k - 1) * SumDenominator2)

            If bW50 Then 'Brown and Forsythe modification of Levene's test
                out.Pvalue = distributions.F_RT(Wmedian, k - 1, n - k)
                out.TestStatistics1 = Wmedian
            Else 'Levene's test
                out.Pvalue = distributions.F_RT(Wmean, k - 1, n - k)
                out.TestStatistics1 = Wmean
            End If

            Return out
        End Function

        ''' <summary>
        ''' Performs Bartlett's test for homogeneity of variances across multiple groups.
        ''' </summary>
        ''' <param name="arDataColumn">
        ''' A jagged array of doubles, where each inner array represents the sample values for one group.
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the Bartlett chi-square test statistic.</description></item>
        ''' <item><description><c>Pvalue</c>: the p-value for the test of equal variances.</description></item>
        ''' </list>
        ''' Returns an empty <see cref="TestResult"/> if only one group is provided or if any group has fewer than 2 observations.
        ''' </returns>
        ''' <remarks>
        ''' - Bartlett's test evaluates the null hypothesis that all groups have equal variances.  
        ''' - It is sensitive to departures from normality; non-normal data may inflate Type I error rates.  
        ''' - The test statistic is compared to a chi-square distribution with (k − 1) degrees of freedom.  
        ''' - References: Bartlett, M.S. (1937), *Properties of Sufficiency and Statistical Tests*, Proc. Royal Soc. A, 160, 268–282.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: compare variances across 3 groups
        ''' Dim group1() As Double = {5.1, 4.9, 5.0, 5.2}
        ''' Dim group2() As Double = {6.0, 6.1, 5.9, 6.2}
        ''' Dim group3() As Double = {5.5, 5.6, 5.4, 5.7}
        ''' Dim data()() As Double = {group1, group2, group3}
        '''
        ''' Dim result As TestResult = BartlettTEST(data)
        ''' Console.WriteLine("Bartlett chi-square statistic: " + result.TestStatistics1)
        ''' Console.WriteLine("p-value: " + result.Pvalue)
        ''' </example>
        Public Function BartlettTEST(arDataColumn()() As Double) As TestResult
            Dim out = New TestResult, Sp As Double   'Sp - pooled variance
            Dim sum1 As Double, sum2 As Double
            Dim k As Integer = arDataColumn.Length 'find out # of groups
            Dim arNs(k - 1) As Integer
            For i = 0 To k - 1
                arNs(i) = arDataColumn(i).Length
            Next
            Dim n As Integer = arNs.Sum() 'total sample size

            'Check assumptions
            For i = 0 To k - 1
                If arNs(i) <= 1 Then Return out
            Next
            If k <= 1 Then Return out

            Dim Si(k - 1) As Double 'variances

            For i = 0 To k - 1
                Dim Temporal() As Double = arDataColumn(i)
                If arNs(i) > 1 Then Si(i) = variance(Temporal)
                sum1 += ((arNs(i) - 1) * Math.Log(Si(i)))
                sum2 += (1 / (arNs(i) - 1))
                Sp += ((arNs(i) - 1) * Si(i) / (n - k))
            Next

            out.TestStatistics1 = ((n - k) * Math.Log(Sp) - sum1) / (1 + (1.0 / (3 * (k - 1))) * (sum2 - 1.0 / (n - k)))
            out.Pvalue = 1.0 - distributions.ChiSquareCDF(out.TestStatistics1, k - 1)

            Return out
        End Function

        ''' <summary>
        ''' Performs the Squared Ranks Test for equality of variances across multiple groups.
        ''' </summary>
        ''' <param name="arDataColumn">
        ''' A jagged array of doubles, where each inner array represents the sample values for one group.
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the chi-square test statistic for variance equality.</description></item>
        ''' <item><description><c>Pvalue</c>: the p-value for the test of homogeneity of variances.</description></item>
        ''' </list>
        ''' Returns an empty <see cref="TestResult"/> if only one group is provided.
        ''' </returns>
        ''' <remarks>
        ''' - The Squared Ranks Test is a nonparametric test for equality of variances.  
        ''' - It is based on ranking absolute deviations from group means, squaring ranks, and comparing group sums.  
        ''' - The test statistic is compared to a chi-square distribution with k − 1 degrees of freedom.  
        ''' - Robust against departures from normality.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: compare variances across 3 groups
        ''' Dim group1() As Double = {5.1, 4.9, 5.0, 5.2}
        ''' Dim group2() As Double = {6.0, 6.1, 5.9, 6.2}
        ''' Dim group3() As Double = {5.5, 5.6, 5.4, 5.7}
        ''' Dim data()() As Double = {group1, group2, group3}
        '''
        ''' Dim result As TestResult = SquaredRanksTestVARIANCE(data)
        ''' Console.WriteLine("Chi-square statistic: " + result.TestStatistics1)
        ''' Console.WriteLine("p-value: " + result.Pvalue)
        ''' </example>
        Public Function SquaredRanksTestVARIANCE(arDataColumn()() As Double) As TestResult
            Dim out = New TestResult
            Dim sum1 As Double, sum2 As Double, Sum3 As Double

            Dim k As Integer = arDataColumn.Length 'find out # of groups
            Dim arNs(k - 1) As Integer
            For i = 0 To k - 1
                arNs(i) = arDataColumn(i).Length
            Next
            Dim n As Integer = arNs.Sum() 'total sample size
            Dim means(k - 1) As Double, diffs(n - 1) As Double

            'Check assumptions
            If k <= 1 Then Return out

            Dim ii As Integer = 0
            For i = 0 To k - 1
                Dim Temporal() As Double = arDataColumn(i)
                means(i) = Temporal.Average()
                For j = 0 To arNs(i) - 1
                    diffs(ii) = Math.Abs(arDataColumn(i)(j) - means(i))
                    ii += 1
                Next
            Next

            Dim Si(k - 1) As Double
            Dim Ranks() As Double = nonparametric.ComputeAvgRanks(diffs)
            ii = 0
            For i = 0 To k - 1
                For j = 0 To arNs(i) - 1
                    Si(i) += Ranks(ii) * Ranks(ii)
                    sum2 += Ranks(ii) ^ 4
                    Sum3 += Ranks(ii) * Ranks(ii)
                    ii += 1
                Next
                sum1 += ((Si(i) * Si(i)) / arNs(i))
            Next

            Dim Saverage As Double = Sum3 / n
            Dim d As Double = 1.0 / (n - 1) * (sum2 - n * Saverage * Saverage)
            out.TestStatistics1 = 1.0 / d * (sum1 - n * Saverage * Saverage)
            out.Pvalue = 1.0 - distributions.ChiSquareCDF(out.TestStatistics1, k - 1)

            Return out
        End Function

        ''' <summary>
        ''' Performs Mauchly's Test of Sphericity for a one-way repeated measures ANOVA.
        ''' </summary>
        ''' <param name="arData">
        ''' A two-dimensional array of doubles containing the repeated measures data.
        ''' Rows correspond to subjects, and columns correspond to groups (conditions).
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the chi-square test statistic for sphericity.</description></item>
        ''' <item><description><c>Pvalue</c>: the p-value for the test of sphericity.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Constructs the variance–covariance matrix of the data.  
        ''' - Applies double-centering to estimate the population variance–covariance matrix.  
        ''' - Computes eigenvalues and uses them to calculate Mauchly's W statistic.  
        ''' - Adjusts the test statistic with a correction factor and evaluates significance using the chi-square distribution.  
        ''' - A significant result (p &lt; α) indicates violation of the sphericity assumption.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: 5 subjects measured under 3 conditions
        ''' Dim arData(4,2) As Double
        ''' arData(0,0) = 12 : arData(0,1) = 15 : arData(0,2) = 14
        ''' arData(1,0) = 10 : arData(1,1) = 13 : arData(1,2) = 12
        ''' arData(2,0) = 11 : arData(2,1) = 14 : arData(2,2) = 13
        ''' arData(3,0) = 9  : arData(3,1) = 12 : arData(3,2) = 11
        ''' arData(4,0) = 13 : arData(4,1) = 16 : arData(4,2) = 15
        '''
        ''' Dim result As TestResult = MauchlyTest(arData)
        ''' Console.WriteLine("Chi-square statistic: "  result.TestStatistics1)
        ''' Console.WriteLine("p-value: "  result.Pvalue)
        ''' </example>
        Public Function MauchlyTest(arData(,) As Double) As TestResult
            Dim out = New TestResult, i As Integer, Den As Double

            Dim NoSub As Integer = arData.GetLength(0)
            Dim NoGroups As Integer = arData.GetLength(1)

            Dim VarCovar(,) As Double = Matrix.MatCovar(arData) 'create variance-covariance matrix
            'double center sample var-covar matrix to estimate population var-covar matrix
            Dim PopVarCovar(,) As Double = Matrix.MatDoubleCenter(VarCovar)
            Dim eig = Matrix.EIGEN_JK(PopVarCovar) 'calculate eigenvector and eigenvalues
            Dim Eigenval() As Double = eig.Item1

            Dim Num As Double = 1.0
            For i = 0 To Eigenval.GetUpperBound(0) - 1
                Num *= Eigenval(i) 'numerator
                Den += Eigenval(i) 'denominator
            Next
            Den = (Den / (NoGroups - 1)) ^ (NoGroups - 1)
            Dim W As Double = Num / Den
            Dim F As Double = (2.0 * ((NoGroups - 1) ^ 2) + NoGroups + 2) / (6.0 * (NoGroups - 1) * (NoSub - 1))
            out.TestStatistics1 = -(1.0 - F) * (NoSub - 1) * Math.Log(W)
            out.Pvalue = 1.0 - distributions.ChiSquareCDF(out.TestStatistics1, 0.5 * NoGroups * (NoGroups - 1))

            Return out
        End Function

        ''' <summary>
        ''' Performs a statistical test of symmetry about an unknown median.
        ''' </summary>
        ''' <param name="data">
        ''' A one-dimensional array of doubles containing the sample data.
        ''' </param>
        ''' <param name="strType">
        ''' Specifies the test type:
        ''' <list type="bullet">
        ''' <item><description><c>"Miao-Gel-Gastwirth"</c>: MGG test of symmetry (Miao, Gel, Gastwirth, 2006).</description></item>
        ''' <item><description><c>"Cabilio-Masaro"</c>: CM test of symmetry (Cabilio, Masaro, 1996).</description></item>
        ''' </list>
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the test statistic for the chosen symmetry test.</description></item>
        ''' <item><description><c>Pvalue</c>: the two-sided p-value for the test.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - The MGG test uses a robust scale estimator based on deviations from the median.  
        ''' - The CM test uses the difference between mean and median scaled by the sample standard deviation.  
        ''' - Both tests are distribution-free and test the null hypothesis of symmetry about the median.  
        ''' - If the robust scale estimator is zero, the test statistic is set to 0 and p-value to 1.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: test symmetry of a dataset
        ''' Dim data() As Double = {1.2, 2.3, 2.1, 1.9, 2.0, 2.2}
        ''' Dim result As TestResult = SymmetryTest(data, "Miao-Gel-Gastwirth")
        ''' Console.WriteLine("Test statistic: "  result.TestStatistics1)
        ''' Console.WriteLine("p-value: "  result.Pvalue)
        ''' </example>
        Public Function SymmetryTest(data() As Double, strType As String) As TestResult
            'MGG test
            'Miao W., Gel Y.R., Gastwirth J.L. A NEW TEST OF SYMMETRY ABOUT AN UNKNOWN MEDIAN. In Random Walk, Sequential Analysis and
            'Related Topics a Festschrift in Honor of Yuan-Shih Chow. By Yuan Shih Chow, Agnes Chao. Hsiung, Zhiliang Ying, and Cun-Hui Zhang
            'Singapore: World Scientific Pub., 2006. 199-214.
            'CM test: Cabilio P., Masaro J. (1996) A simple test of symmetry about an unknown median. The Canadian Journal of Statistics, 24, 349-361
            'M test:  Mira A. (1999) Distribution-free test for symmetry based on Bonferroni's measure. Journal of Applied Statistics, 26, 959-972

            Dim out = New TestResult
            Dim SDrobust As Double
            Dim n As Integer = data.Length

            Dim Medn As Double = Median(data)
            Dim Mean As Double = data.Average()
            Dim sd As Double = stDev(data)
            For i = 0 To n - 1
                SDrobust += Math.Abs(data(i) - Medn)
            Next
            Dim temp As Double = Math.Sqrt(Math.PI / 2.0) / n
            SDrobust = temp * SDrobust
            SDrobust = SDrobust / Math.Sqrt(n)

            If SDrobust = 0.0 Then
                out.TestStatistics1 = 0.0
                out.Pvalue = 1.0
                Return out
            End If

            'test statistic and two-sided P-value
            If strType = "Miao-Gel-Gastwirth" Then
                If SDrobust <> 0.0 Then out.TestStatistics1 = (Mean - Medn) / SDrobust
                out.TestStatistics1 = out.TestStatistics1 / Math.Sqrt(Math.PI / 2.0 - 1.0)
                out.Pvalue = 2.0 * (1.0 - distributions.PNorm(Math.Abs(out.TestStatistics1)))
            ElseIf strType = "Cabilio-Masaro" Then
                out.TestStatistics1 = (Mean - Medn) / (sd / Math.Sqrt(n))
                out.TestStatistics1 = out.TestStatistics1 / Math.Sqrt(Math.PI / 2.0 - 1.0)
                out.Pvalue = 2.0 * (1.0 - distributions.PNorm(Math.Abs(out.TestStatistics1)))
            End If

            Return out
        End Function

        ''' <summary>
        ''' Performs the two-sided Grubbs' Test for detecting a single outlier in a univariate dataset.
        ''' </summary>
        ''' <param name="x">
        ''' A one-dimensional array of doubles containing the sample data.
        ''' </param>
        ''' <param name="Alpha">
        ''' The significance level for the test (default = 0.05 for 5%).
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: the critical value Gcrit based on alpha.</description></item>
        ''' <item><description><c>TestStatistics2</c>: the observed Grubbs' test statistic G.</description></item>
        ''' <item><description><c>strSpecialInformation</c>: textual conclusion ("Maximum value (…) is an outlier.", "Minimum value (…) is an outlier.", or "No outlier present in the data.").</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Based on NIST/SEMATECH e-Handbook of Statistical Methods (2012).  
        ''' - Tests whether the maximum or minimum value in the dataset is an outlier.  
        ''' - Uses Student's t-distribution to compute the critical value.  
        ''' - Only valid for detecting one outlier at a time.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: dataset with a potential outlier
        ''' Dim data() As Double = {10, 12, 11, 13, 50}
        ''' Dim result As TestResult = Grubbs(data, 0.05)
        ''' Console.WriteLine("Critical value (Gcrit): "  result.TestStatistics1)
        ''' Console.WriteLine("Observed statistic (G): "  result.TestStatistics2)
        ''' Console.WriteLine("Conclusion: "  result.strSpecialInformation)
        ''' ' Output: Maximum value (50) is an outlier.
        ''' </example>
        Public Function Grubbs(x() As Double, Optional Alpha As Double = 0.05) As TestResult

            Dim out = New TestResult
            Dim G1 As Double
            Dim n As Integer = x.Length
            Dim Max As Double = x.Max()
            Dim min As Double = x.Min()
            Dim Avg As Double = x.Average()
            Dim sd As Double = stDev(x)
            Dim Gmax As Double = Math.Abs(Max - Avg) / sd 'important at the end when identifying whether
            Dim Gmin As Double = Math.Abs(min - Avg) / sd 'max or min value is an outlier

            Dim G As Double = Math.Abs(x(1) - Avg) / sd
            For i = 1 To n - 1  'find the value of Grubbs' test statistics
                G1 = Math.Abs(x(i) - Avg) / sd
                If G1 > G Then G = G1
            Next

            Dim Tcrit As Double = distributions.T_Inv((Alpha / (2.0 * n)), n - 2)
            Dim Gcrit As Double = ((n - 1) / Math.Sqrt(n)) * Math.Sqrt(Tcrit ^ 2 / (n - 2 + Tcrit * Tcrit))

            Select Case G 'compare G value with critical value and make conclusion
                Case Is > Gcrit
                    If G = Gmax Then
                        out.strSpecialInformation = $"Maximum value {(Max)} Is an outlier."
                    ElseIf G = Gmin Then
                        out.strSpecialInformation = $"Minimum value {(min)} Is an outlier."
                    End If
                Case Else
                    out.strSpecialInformation = "No outlier present in the data."
            End Select

            out.TestStatistics1 = Gcrit
            out.TestStatistics2 = G

            Return out
        End Function

        ''' <summary>
        ''' Performs the Rosner Generalized Extreme Studentized Deviate (ESD) test for detecting multiple outliers
        ''' in a univariate dataset (up to 10 outliers).
        ''' </summary>
        ''' <param name="x">
        ''' A one-dimensional array of doubles containing the sample data.
        ''' </param>
        ''' <param name="Alpha">
        ''' The significance level for the test (default = 0.05 for 5%).
        ''' </param>
        ''' <returns>
        ''' A <c>Double()</c> array containing the detected outliers (maximum 10 values).
        ''' If no outliers are detected or the sample size is too small, <c>Nothing</c> is returned.
        ''' </returns>
        ''' <remarks>
        ''' - Based on Rosner B. (1983), *Percentage Points for a Generalized ESD Many-Outlier Procedure*, 
        '''   Technometrics, 25(2), 165–172.  
        ''' - The test uses approximate percentiles based on the t-distribution.  
        ''' - Not recommended for small samples (n &lt; 25); fails entirely for n &lt; 15.  
        ''' - Iteratively removes the most extreme value and compares the test statistic R(i) to its critical value λ(i).  
        ''' - The number of outliers is determined by the largest i such that R(i) &gt; λ(i).  
        ''' </remarks>
        ''' <example>
        ''' ' Example: dataset with potential multiple outliers
        ''' Dim data() As Double = {10, 12, 11, 13, 50, 52, 9, 8, 7, 100}
        ''' Dim result() As Double = Rosner(data, 0.05)
        ''' If result Is Nothing Then
        '''     Console.WriteLine("No outliers detected.")
        ''' Else
        '''     Console.WriteLine("Outliers detected: "  String.Join(", ", result))
        ''' End If
        ''' ' Output: Outliers detected: 100, 52, 50
        ''' </example>
        Public Function Rosner(x() As Double, Optional Alpha As Double = 0.05#) As Double()

            Dim r(9) As Double, Outliers(9) As Double, Lambda(9) As Double, NoOutliers As Integer
            Dim Mean As Double, ss As Double, ibig As Double
            Dim sd As Double, a As Double, p As Double, Tcrit As Double, big As Double, i As Integer

            Dim n As Integer = x.Length

            If n < 15 Then
                AppGlobals.BSlogg.Log("Rosner: Sample size too small for calucalation (n < 15).", AppGlobals.LogMsgType.Warn)
                Return Nothing
            ElseIf n < 25 Then
                AppGlobals.BSlogg.Log("Rosner: Too small sample size for this test (n < 25). Inference done by the test could be incorect. For more information see (Technometrics, 25(2), 165-172).",
                        AppGlobals.LogMsgType.Warn)
            End If

            Dim fn = n
            Dim q(n - 1) As Double
            Dim sum As Double = x.Sum()
            Dim sums = SumSq(x) 'sum of squares

            Dim ii As Integer = 0
            Do While ii <= Math.Min(9, n - 1) '10 - max # of detected outliers
                ss = sums - (sum * sum) / fn
                sd = Math.Sqrt(ss / (fn - 1))
                Mean = sum / fn
                big = 0 : ibig = 0

                For i = 0 To n - 1
                    If q(i) <> 1 Then
                        a = Math.Abs(x(i) - Mean)
                        If a > big Then
                            big = a
                            ibig = i
                        End If
                    End If
                Next

                r(ii) = big / sd
                q(ibig) = 1
                Outliers(ii) = x(ibig)
                ii += 1
                sum -= x(ibig)
                sums -= x(ibig) * x(ibig)
                fn -= 1
            Loop

            'compute the 10 critical values, which coresponds to 10 Ri values
            For i = 1 To Math.Min(10, n)
                p = 1.0 - (Alpha / (2.0 * (n - i + 1)))
                Tcrit = distributions.T_Inv(p, (n - i - 1))
                Lambda(i - 1) = ((n - 1) * Tcrit) / Math.Sqrt((n - i - 1 + Tcrit ^ 2) * (n - i + 1))
            Next

            '# of outliers is determined by finding the largest i such that R(i) > lambda(i)
            NoOutliers = 0
            i = 0
            Do While r(i) > Lambda(i)
                NoOutliers += 1
                i += 1
                If i = Math.Min(10, n) Then Exit Do
            Loop

            If NoOutliers > 0 Then ReDim Preserve Outliers(NoOutliers - 1)
            If NoOutliers = 0 Then
                Return Nothing
            Else
                Return Outliers
            End If
        End Function

    End Module
End Namespace