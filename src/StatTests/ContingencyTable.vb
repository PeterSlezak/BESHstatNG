Option Explicit On
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace contingencytable


    Public Module ContingencyTable

        ''' <summary>
        ''' Performs the Mantel–Haenszel test for stratified 2x2 contingency tables.
        ''' </summary>
        ''' <param name="data">
        ''' A two-dimensional array of doubles representing multiple 2x2 tables stacked vertically.
        ''' Each pair of consecutive rows corresponds to one 2x2 table:
        ''' <list type="bullet">
        ''' <item><description>Row i: [a, b]</description></item>
        ''' <item><description>Row i+1: [c, d]</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the pooled odds-ratio confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A tuple containing:
        ''' <list type="bullet">
        ''' <item><description><see cref="TestResult"/> with the Mantel–Haenszel chi-square statistic and p-value.</description></item>
        ''' <item><description><see cref="ConfidenceIntervalResult"/> with the pooled odds ratio estimate and a two-sided confidence interval at level <c>1 - alpha</c>.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Applies a continuity correction (0.5) when any cell count is zero.  
        ''' - Uses the chi-square distribution with 1 degree of freedom for the test statistic.  
        ''' - Confidence interval is computed on the log-odds scale and exponentiated back using
        '''   <c>z = NormSInv(1 - alpha / 2)</c>.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: two stratified 2x2 tables
        ''' Dim data(3,1) As Double
        ''' data(0,0) = 12 : data(0,1) = 8
        ''' data(1,0) = 5  : data(1,1) = 15
        ''' data(2,0) = 20 : data(2,1) = 10
        ''' data(3,0) = 7  : data(3,1) = 13
        '''
        ''' Dim result = MantelHaenszel(data)
        ''' Console.WriteLine("Chi-square: " result.Item1.TestStatistics1)
        ''' Console.WriteLine("p-value: "  result.Item1.Pvalue)
        ''' Console.WriteLine("Odds ratio: "  result.Item2.Estimate)
        ''' Console.WriteLine("CI: "  result.Item2.strConfidenceInterval)
        ''' </example>
        Public Function MantelHaenszel(data(,) As Double, Optional alpha As Double = 0.05) As (TestResult, ConfidenceIntervalResult)

            Dim a As Double, b As Double, c As Double, d As Double
            Dim SumNum As Double, SumDenom As Double, SumOR1 As Double, SumOR2 As Double, n As Double
            Dim sum1 As Double, sum2 As Double, Sum3 As Double
            Dim z As Double = distributions.ZCritTwoSided(alpha)
            Dim rowsNo As Integer = data.GetLength(0)

            If (rowsNo Mod 2 <> 0) Or (data.GetUpperBound(1) <> 1) Then 'check imput data dimension
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Wrong dimension of the input table! Mantel Haenszel test"))
            End If

            For i = 0 To rowsNo - 1
                For j = 0 To 1
                    data(i, j) = Int(data(i, j))
                Next
            Next

            For i = 1 To rowsNo Step 2
                a = data(i - 1, 0)
                b = data(i - 1, 1)
                c = data(i, 0)
                d = data(i, 1)
                If a = 0 Or b = 0 Or c = 0 Or d = 0 Then
                    a = a + 0.5 : b = b + 0.5 : c = c + 0.5 : d = d + 0.5
                End If
                n = a + b + c + d
                SumNum += (a - (a + b) * (a + c) / n)
                SumDenom += ((a + b) * (a + c) * (b + d) * (c + d) / (n ^ 3 - n ^ 2))
                SumOR1 += (a * d / n)
                SumOR2 += (c * b / n)
                sum1 += (((a + d) / n) * (a * d / n))
                sum2 += (((a + d) / n) * (c * b / n) + ((c + b) / n) * (a * d / n))
                Sum3 += (((c + b) / n) * (c * b / n))
            Next i

            Dim out = New TestResult
            out.TestStatistics1 = (Math.Abs(SumNum) - 0.5) ^ 2 / SumDenom
            out.Pvalue = 1.0 - distributions.ChiSquareCDF(out.TestStatistics1, 1)

            Dim ci As New ConfidenceIntervalResult
            ci.alpha = alpha
            ci.Estimate = SumOR1 / SumOR2
            Dim var As Double = 0.5 * ((sum1 / SumOR1 ^ 2) + (sum2 / (SumOR1 * SumOR2)) + (Sum3 / SumOR2 ^ 2))
            ci.LowerLimit = Math.Exp(Math.Log(ci.Estimate) - (z * Math.Sqrt(var)))
            ci.UpperLimit = Math.Exp(Math.Log(ci.Estimate) + (z * Math.Sqrt(var)))

            Return (out, ci)
        End Function


        ''' <summary>
        ''' Computes a single proportion estimate and its confidence interval
        ''' using the Wilson score interval method.
        ''' </summary>
        ''' <param name="NoResp">
        ''' The number of responses (successes) observed in the sample.
        ''' </param>
        ''' <param name="TotalN">
        ''' The total sample size (number of trials).
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the Wilson confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A <see cref="ConfidenceIntervalResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>Estimate</c>: the observed proportion (NoResp / TotalN).</description></item>
        ''' <item><description><c>LowerLimit</c>: the lower bound of the confidence interval.</description></item>
        ''' <item><description><c>UpperLimit</c>: the upper bound of the confidence interval.</description></item>
        ''' <item><description><c>strConfidenceInterval</c>: formatted string representation of the interval.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' This implementation uses the Wilson score interval with
        ''' <c>z = NormSInv(1 - alpha / 2)</c>.
        ''' It is more accurate than the normal approximation, especially for small samples or proportions near 0 or 1.
        ''' </remarks>
        ''' <example>
        ''' Dim result As ConfidenceIntervalResult = SingleProportion(45, 100)
        ''' Console.WriteLine("Estimate: " result.Estimate)
        ''' Console.WriteLine("CI: " result.strConfidenceInterval)
        ''' ' Output: Estimate ≈ 0.45, CI ≈ 0.352 to 0.552
        ''' </example>
        Public Function SingleProportion(NoResp As Integer, TotalN As Integer, Optional alpha As Double = 0.05) As ConfidenceIntervalResult

            Dim z As Double = distributions.ZCritTwoSided(alpha)
            Dim z2 As Double = z * z
            Dim a As Double = 2.0 * NoResp + z2
            Dim b As Double = z * Math.Sqrt(z2 + 4.0 * NoResp * (1 - NoResp / TotalN))
            Dim c As Double = 2.0 * (TotalN + z2)

            Dim out As New ConfidenceIntervalResult
            out.alpha = alpha
            out.LowerLimit = (a - b) / c
            out.UpperLimit = (a + b) / c
            out.Estimate = NoResp / TotalN

            Return out
        End Function

        ''' <summary>
        ''' Computes the difference between two independent proportions and its confidence interval.
        ''' </summary>
        ''' <param name="NoResp1">
        ''' The number of responses (successes) observed in the first sample.
        ''' </param>
        ''' <param name="TotalN1">
        ''' The total sample size of the first group.
        ''' </param>
        ''' <param name="NoResp2">
        ''' The number of responses (successes) observed in the second sample.
        ''' </param>
        ''' <param name="TotalN2">
        ''' The total sample size of the second group.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A <see cref="ConfidenceIntervalResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>Estimate</c>: the difference in proportions (p1 − p2).</description></item>
        ''' <item><description><c>LowerLimit</c>: the lower bound of the confidence interval.</description></item>
        ''' <item><description><c>UpperLimit</c>: the upper bound of the confidence interval.</description></item>
        ''' <item><description><c>strConfidenceInterval</c>: formatted string representation of the interval.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' This implementation uses Wilson score intervals for each proportion and combines them
        ''' to form a confidence interval for the difference. It is more accurate than the normal
        ''' approximation, especially for small samples or proportions near 0 or 1.
        ''' </remarks>
        ''' <example>
        ''' Dim result As ConfidenceIntervalResult = TwoIndependentProportions(45, 100, 30, 100)
        ''' Console.WriteLine("Estimate: "  result.Estimate)
        ''' Console.WriteLine("CI: "  result.strConfidenceInterval)
        ''' ' Output: Estimate ≈ 0.15, CI ≈ 0.037 to 0.263
        ''' </example>
        Public Function TwoIndependentProportions(NoResp1 As Integer, TotalN1 As Integer, NoResp2 As Integer, TotalN2 As Integer, Optional alpha As Double = 0.05) As ConfidenceIntervalResult
            Dim z As Double = distributions.ZCritTwoSided(alpha)
            Dim z2 As Double = z * z
            Dim out = New ConfidenceIntervalResult
            out.alpha = alpha
            out.Estimate = NoResp1 / TotalN1 - NoResp2 / TotalN2

            Dim a1 As Double = 2.0 * NoResp1 + z2
            Dim a2 As Double = 2.0 * NoResp2 + z2
            Dim b1 As Double = z * Math.Sqrt(z2 + 4.0 * NoResp1 * (1 - NoResp1 / TotalN1))
            Dim b2 As Double = z * Math.Sqrt(z2 + 4.0 * NoResp2 * (1 - NoResp2 / TotalN2))
            Dim c1 As Double = 2.0 * (TotalN1 + z2)
            Dim c2 As Double = 2.0 * (TotalN2 + z2)
            Dim L1 As Double = (a1 - b1) / c1
            Dim L2 As Double = (a2 - b2) / c2
            Dim U1 As Double = (a1 + b1) / c1
            Dim U2 As Double = (a2 + b2) / c2

            out.LowerLimit = out.Estimate - Math.Sqrt((NoResp1 / TotalN1 - L1) ^ 2 + (U2 - NoResp2 / TotalN2) ^ 2)
            out.UpperLimit = out.Estimate + Math.Sqrt((NoResp2 / TotalN2 - L2) ^ 2 + (U1 - NoResp1 / TotalN1) ^ 2)

            Return out
        End Function

        ''' <summary>
        ''' Performs Fisher's exact test for a 2x2 contingency table.
        ''' </summary>
        ''' <param name="a">
        ''' The count in the first row, first column cell.
        ''' </param>
        ''' <param name="b">
        ''' The count in the first row, second column cell.
        ''' </param>
        ''' <param name="c">
        ''' The count in the second row, first column cell.
        ''' </param>
        ''' <param name="d">
        ''' The count in the second row, second column cell.
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>PvalueLowerSide</c>: one-tailed p-value (lower side).</description></item>
        ''' <item><description><c>Pvalue</c>: two-tailed p-value.</description></item>
        ''' <item><description><c>pValueExactLowerSide</c>: mid-p one-tailed p-value.</description></item>
        ''' <item><description><c>Pvalue2</c>: mid-p two-tailed p-value.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Rotates the table so that the smallest cell is in the top-left position before computation.  
        ''' - Applies exact combinatorial probabilities for all possible tables with fixed margins.  
        ''' - Warns and aborts if the total sample size exceeds 1000 (to avoid excessive computation).  
        ''' </remarks>
        ''' <example>
        ''' ' Example: 2x2 table
        ''' '   a b
        ''' '   c d
        ''' Dim result As TestResult = FisherExact2x2(12, 8, 5, 15)
        ''' Console.WriteLine("One-tailed p-value: "  result.PvalueLowerSide)
        ''' Console.WriteLine("Two-tailed p-value: "  result.Pvalue)
        ''' Console.WriteLine("Mid-p one-tailed: "  result.pValueExactLowerSide)
        ''' Console.WriteLine("Mid-p two-tailed: "  result.Pvalue2)
        ''' </example>
        Public Function FisherExact2x2(a As Integer, b As Integer, c As Integer, d As Integer) As TestResult
            Dim buffer As Integer, p As Double, out = New TestResult

            Dim n As Double = a + b + c + d
            If n > 1000 Then
                AppGlobals.BSlogg.Log("Too large sample size for exact computation.", AppGlobals.LogMsgType.Warn)
                Return Nothing
            End If
            Dim min As Integer = Minimum(a, b, c, d)

            'find the min value and rotate the table until min value is in the cell (1,1) (i.e. a)
            Do Until a = min
                buffer = a : a = b : b = d : d = c : c = buffer
            Loop
            'store the current values
            Dim AA As Integer = a
            Dim BB As Integer = b
            Dim CC As Integer = c
            Dim dd As Integer = d

            Dim Ptreshold As Double = Combin(a + c, a) * Combin(b + d, b) / Combin(n, a + b)
            Dim marginR As Integer = a + b
            Dim marginC As Integer = a + c
            Dim minMARGIN As Integer = Math.Min(marginR, marginC)

            'calculate one-tail probability
            Dim p_oneTAIL1 As Double = Ptreshold
            Do Until a = 0
                a = a - 1 : b = b + 1 : c = c + 1 : d = d - 1
                p = Combin(a + c, a) * Combin(b + d, b) / Combin(n, a + b)
                If p <= Ptreshold Then p_oneTAIL1 += p
            Loop

            'calculate 2nd tail
            a = AA : b = BB : c = CC : d = dd
            Dim p_oneTAIL2 As Double = Ptreshold
            Do Until a = minMARGIN
                a = a + 1 : b = b - 1 : c = c - 1 : d = d + 1
                p = Combin(a + c, a) * Combin(b + d, b) / Combin(n, a + b)
                If p <= Ptreshold Then p_oneTAIL2 += p
            Loop

            'outputs
            out.PvalueLowerSide = Math.Min(p_oneTAIL1, p_oneTAIL2) 'one tail
            out.Pvalue = p_oneTAIL1 + p_oneTAIL2 - Ptreshold 'two tail
            out.pValueExactLowerSide = out.PvalueLowerSide - Ptreshold / 2.0 'mid one tail
            out.Pvalue2 = (out.PvalueLowerSide - Ptreshold / 2.0) * 2.0 'mid two tail

            Return out
        End Function

        ''' <summary>
        ''' Computes the difference between two paired proportions and its confidence interval.
        ''' </summary>
        ''' <param name="TotalN">
        ''' The total number of paired observations.
        ''' </param>
        ''' <param name="NoResp1">
        ''' The number of responses observed in the first condition only.
        ''' </param>
        ''' <param name="NoResp2">
        ''' The number of responses observed in the second condition only.
        ''' </param>
        ''' <param name="RespBoth">
        ''' The number of responses observed in both conditions simultaneously.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A <see cref="ConfidenceIntervalResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>Estimate</c>: the difference in paired proportions (P1 − P2).</description></item>
        ''' <item><description><c>LowerLimit</c>: the lower bound of the confidence interval.</description></item>
        ''' <item><description><c>UpperLimit</c>: the upper bound of the confidence interval.</description></item>
        ''' <item><description><c>strConfidenceInterval</c>: formatted string representation of the interval.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Uses Wilson score intervals for each proportion and adjusts for correlation between paired responses.  
        ''' - The correlation adjustment is based on the phi coefficient derived from the 2x2 table of paired outcomes.  
        ''' - Provides more accurate confidence intervals than treating the two proportions as independent.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: paired responses in two conditions
        ''' Dim result As ConfidenceIntervalResult = PairedProportions(100, 20, 15, 40)
        ''' Console.WriteLine("Estimate: "  result.Estimate)
        ''' Console.WriteLine("CI: "  result.strConfidenceInterval)
        ''' ' Output: Estimate ≈ 0.05, CI ≈ -0.083 to 0.183
        ''' </example>
        Public Function PairedProportions(TotalN As Integer, NoResp1 As Integer, NoResp2 As Integer, RespBoth As Integer, Optional alpha As Double = 0.05) As ConfidenceIntervalResult
            Dim phi As Double, a As Double, b As Double, c As Double
            Dim z As Double = distributions.ZCritTwoSided(alpha)
            Dim z2 As Double = z * z
            Dim out = New ConfidenceIntervalResult
            out.alpha = alpha

            Dim P1 As Double = (NoResp1 + RespBoth) / TotalN
            Dim P2 As Double = (NoResp2 + RespBoth) / TotalN
            out.Estimate = P1 - P2

            Dim a1 As Double = 2.0 * (NoResp1 + RespBoth) + z2
            Dim a2 As Double = 2.0 * (NoResp2 + RespBoth) + z2
            Dim b1 As Double = z * Math.Sqrt(z2 + 4.0 * (NoResp1 + RespBoth) * (1.0 - P1))
            Dim b2 As Double = z * Math.Sqrt(z2 + 4.0 * (NoResp2 + RespBoth) * (1.0 - P2))
            Dim c1 As Double = 2.0 * (TotalN + z2)
            Dim c2 As Double = 2.0 * (TotalN + z2)
            Dim L1 As Double = (a1 - b1) / c1
            Dim L2 As Double = (a2 - b2) / c2
            Dim U1 As Double = (a1 + b1) / c1
            Dim U2 As Double = (a2 + b2) / c2

            If (NoResp1 + RespBoth) = 0 Or ((TotalN - NoResp1 - RespBoth) = 0) Or (NoResp2 + RespBoth) = 0 Or ((TotalN - NoResp2 - RespBoth) = 0) Then
                phi = 0.0
            Else
                a = (NoResp1 + RespBoth) * (TotalN - NoResp1 - RespBoth) * (NoResp2 + RespBoth) * (TotalN - NoResp2 - RespBoth)
                b = (RespBoth * (TotalN - (NoResp1 + NoResp2 + RespBoth))) - (NoResp1 * NoResp2)
                If b > (TotalN / 2) Then
                    c = b - (TotalN / 2.0)
                ElseIf b >= 0.0 And b <= (TotalN / 2.0) Then
                    c = 0.0
                ElseIf b < 0.0 Then
                    c = b
                End If
                phi = c / Math.Sqrt(a)
            End If

            out.LowerLimit = out.Estimate - Math.Sqrt((P1 - L1) ^ 2 - 2.0 * phi * (P1 - L1) * (U2 - P2) + (U2 - P2) ^ 2)
            out.UpperLimit = out.Estimate + Math.Sqrt((P2 - L2) ^ 2 - 2.0 * phi * (P2 - L2) * (U1 - P1) + (U1 - P1) ^ 2)

            Return out
        End Function

        ''' <summary>
        ''' Performs Liddell's exact McNemar test for paired 2×2 tables and returns
        ''' an exact odds-ratio confidence interval.
        ''' </summary>
        ''' <param name="table">
        ''' A two-dimensional integer array representing a paired 2×2 table:
        ''' <list type="bullet">
        ''' <item><description>Row 0, Col 0: concordant negative pairs</description></item>
        ''' <item><description>Row 0, Col 1: discordant pairs of type r</description></item>
        ''' <item><description>Row 1, Col 0: discordant pairs of type s</description></item>
        ''' <item><description>Row 1, Col 1: concordant positive pairs</description></item>
        ''' </list>
        ''' If a larger matrix is provided, only the upper-left 2×2 subtable is analyzed.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the exact odds-ratio confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A tuple containing:
        ''' <list type="bullet">
        ''' <item><description><see cref="TestResult"/> with the exact p-value from Liddell's McNemar test.</description></item>
        ''' <item><description><see cref="ConfidenceIntervalResult"/> with the odds-ratio estimate and an exact two-sided confidence interval at level <c>1 - alpha</c>.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Based on F.D.K. Liddell (1983), <i>Simplified exact analysis of case-referent studies: matched pairs; dichotomous exposure</i>, Journal of Epidemiology and Community Health, 37, 82–84.  
        ''' - Uses upper-tail F quantiles with <c>alpha / 2</c> to compute exact confidence limits for the matched-pairs odds ratio.  
        ''' - Handles special cases when one discordant cell count is zero, producing one-sided or infinite bounds when appropriate.  
        ''' </remarks>
        ''' <example>
        ''' Dim table(1,1) As Integer
        ''' table(0,0) = 30 : table(0,1) = 10
        ''' table(1,0) = 5  : table(1,1) = 25
        '''
        ''' Dim result = Liddell_McNemar(table, 0.1)
        ''' Console.WriteLine("Exact p-value: " result.Item1.Pvalue)
        ''' Console.WriteLine("Odds ratio: " result.Item2.Estimate)
        ''' Console.WriteLine("CI: "  result.Item2.strConfidenceInterval)
        ''' </example>
        Public Function Liddell_McNemar(table(,) As Integer, Optional alpha As Double = 0.05) As (TestResult, ConfidenceIntervalResult)
            'input table have to be of size 2x2 else 2x2 subtable in upper left corner of the matrix is analyzed
            'function is based on paper by F.D.K. Liddell. Simplified exact analysis of case-referent studies:
            'matched pairs; dichotomous exposure. Journal of Epidemiology and Community Health, 1983, 37, 82-84.

            Dim tst = New TestResult, ci = New ConfidenceIntervalResult
            ci.alpha = alpha
            Dim r As Double = table(0, 1)
            Dim S As Double = table(1, 0)

            If S > 0 Then
                If r > 0 Then
                    ci.Estimate = r / S
                    ci.LowerLimit = r / ((S + 1) * distributions.F_Inv_RT(alpha / 2.0, 2.0 * (S + 1), 2.0 * r))
                    ci.UpperLimit = ((r + 1) * distributions.F_Inv_RT(alpha / 2.0, 2.0 * (r + 1), 2.0 * S)) / S

                ElseIf r = 0 Then
                    ci.Estimate = r / S
                    ci.LowerLimit = 0.0
                    ci.UpperLimit = ((r + 1) * distributions.F_Inv_RT(alpha / 2.0, 2.0 * (r + 1), 2 * S)) / S

                End If
            ElseIf S = 0 Then
                ci.Estimate = 1.0E+30 'infinity
                ci.LowerLimit = r / ((S + 1) * distributions.F_Inv_RT(alpha / 2.0, 2.0 * (S + 1), 2.0 * r))
                ci.strConfidenceInterval = CStr(CSng(ci.LowerLimit)) + " to infinity"
            End If

            If r > S Then
                tst.Pvalue = 2.0 * distributions.F_RT(r / (S + 1.0), 2.0 * (S + 1.0), 2.0 * r)
            ElseIf r < S Then
                tst.Pvalue = 2.0 * distributions.F_RT(S / (r + 1.0), 2.0 * (r + 1.0), 2.0 * S)
            Else
                tst.Pvalue = 2.0 * (1.0 - distributions.F_RT(S / (r + 1.0), 2.0 * (r + 1.0), 2.0 * S))
            End If
            Return (tst, ci)
        End Function

        ''' <summary>
        ''' Performs a chi-square test of independence on an r × s contingency table.
        ''' </summary>
        ''' <param name="table">
        ''' A two-dimensional integer array representing the contingency table.
        ''' Each row corresponds to a category of one variable, and each column corresponds
        ''' to a category of the other variable. Cell values are observed frequencies.
        ''' </param>
        ''' <returns>
        ''' A tuple containing:
        ''' <list type="bullet">
        ''' <item><description><see cref="TestResult"/> with the chi-square statistic and p-value.</description></item>
        ''' <item><description><c>Double</c>: Cramer's V (measure of association strength).</description></item>
        ''' <item><description><c>Double</c>: Pearson contingency coefficient.</description></item>
        ''' <item><description><c>Double</c>: Phi coefficient (for 2×2 tables, equivalent to correlation).</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Computes the uncorrected chi-square statistic and p-value using the chi-square distribution.  
        ''' - Degrees of freedom are adjusted for zero rows or columns.  
        ''' - Association measures (Cramer's V, Pearson coefficient, Phi) provide effect size interpretation.  
        ''' - Based on Press W.H. et al., *Numerical Recipes in Fortran 77: The Art of Scientific Computing*, 
        ''' Cambridge University Press, 1992.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: 3×2 contingency table
        ''' Dim table(2,1) As Integer
        ''' table(0,0) = 10 : table(0,1) = 20
        ''' table(1,0) = 15 : table(1,1) = 25
        ''' table(2,0) = 5  : table(2,1) = 30
        '''
        ''' Dim result = Chi2TESTindependence(table)
        ''' Console.WriteLine("Chi-square statistic: "  result.Item1.TestStatistics1)
        ''' Console.WriteLine("p-value: "  result.Item1.Pvalue)
        ''' Console.WriteLine("Cramer's V: "  result.Item2)
        ''' Console.WriteLine("Pearson coefficient: "  result.Item3)
        ''' Console.WriteLine("Phi coefficient: "  result.Item4)
        ''' </example>
        Public Function Chi2TESTindependence(table(,) As Integer) As (TestResult, Double, Double, Double)
            Dim sum As Double, expected As Double

            Dim rowsNo As Integer = table.GetLength(0)
            Dim columnsNo As Integer = table.GetLength(1)
            Dim SumRows(rowsNo - 1) As Double, SumColumns(columnsNo - 1) As Double
            Dim nrows As Integer = rowsNo
            Dim Ncolumns As Integer = columnsNo

            For i = 0 To rowsNo - 1 'Get the row totals
                SumRows(i) = 0
                For j = 0 To columnsNo - 1
                    SumRows(i) += table(i, j)
                    sum += table(i, j)
                Next
                If (SumRows(i) = 0) Then nrows -= 1 'Eliminate any zero rows by reducing the Number
            Next

            For j = 0 To columnsNo - 1 'Get the column totals
                SumColumns(j) = 0
                For i = 0 To rowsNo - 1
                    SumColumns(j) += table(i, j)
                Next
                If (SumColumns(j) = 0) Then Ncolumns -= 1 'Eliminate any zero columns
            Next

            Dim df As Integer = nrows * Ncolumns - nrows - Ncolumns + 1 'Corrected number of degrees of freedom
            Dim chisq As Double = 0
            For i = 0 To rowsNo - 1 'Do the chi-square sum
                For j = 0 To columnsNo - 1
                    expected = (SumColumns(j) * SumRows(i)) / sum
                    chisq += (table(i, j) - expected) ^ 2 / (expected + 1.0E-30) 'Here guarantees that any
                Next                                             'eliminated row or column will not contribute to the sum.
            Next

            Dim out = New TestResult
            out.TestStatistics1 = chisq
            out.Pvalue = 1.0 - distributions.ChiSquareCDF(chisq, CDbl(df))
            Dim cramerv As Double = Math.Sqrt(chisq / (sum * Math.Min(nrows - 1, Ncolumns - 1)))  'Cramer V
            Dim pearson As Double = Math.Sqrt(chisq / (chisq + sum))  'Pearson Contingency Coefficient
            Dim phi As Double = Math.Sqrt(chisq / sum)  'Phi

            Return (out, cramerv, pearson, phi)
        End Function

        ''' <summary>
        ''' Computes the odds ratio from a 2×2 contingency table and returns both Woolf and Cornfield confidence intervals.
        ''' </summary>
        ''' <param name="table">
        ''' A two-dimensional integer array representing a 2×2 contingency table:
        ''' <list type="bullet">
        ''' <item><description>Row 0, Col 0: cell a</description></item>
        ''' <item><description>Row 0, Col 1: cell b</description></item>
        ''' <item><description>Row 1, Col 0: cell c</description></item>
        ''' <item><description>Row 1, Col 1: cell d</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the Woolf and Cornfield confidence intervals.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A tuple containing:
        ''' <list type="bullet">
        ''' <item><description><see cref="ConfidenceIntervalResult"/> with the Woolf confidence interval for the odds ratio.</description></item>
        ''' <item><description><see cref="ConfidenceIntervalResult"/> with the Cornfield confidence interval for the odds ratio.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Woolf CI is based on the log odds ratio and its standard error.  
        ''' - Cornfield CI is computed iteratively using F-distribution quantiles for exact bounds.  
        ''' - Both intervals provide measures of uncertainty around the odds ratio estimate.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: 2×2 table
        ''' Dim table(1,1) As Integer
        ''' table(0,0) = 12 : table(0,1) = 8
        ''' table(1,0) = 5  : table(1,1) = 15
        '''
        ''' Dim result = OddsRatio(table)
        ''' Console.WriteLine("Odds ratio: "  result.Item1.Estimate)
        ''' Console.WriteLine("Woolf CI: "  result.Item1.strConfidenceInterval)
        ''' Console.WriteLine("Cornfield CI: "  result.Item2.strConfidenceInterval)
        ''' </example>
        Public Function OddsRatio(table(,) As Integer, Optional alpha As Double = 0.05) As (ConfidenceIntervalResult, ConfidenceIntervalResult)

            Dim t As Double, u As Double, V As Double

            Dim a As Integer = table(0, 0)
            Dim b As Integer = table(0, 1)
            Dim c As Integer = table(1, 0)
            Dim d As Integer = table(1, 1)
            Dim sum As Double = a + b + c + d
            Dim q = distributions.NormSInv(1.0 - alpha / 2.0)
            Dim out As New ConfidenceIntervalResult
            out.alpha = alpha
            'Woolf confidence interval
            out.Estimate = a * d / (b * c)
            Dim SE As Double = Math.Sqrt(1.0 / a + 1 / b + 1 / c + 1 / d)
            Dim y As Double = Math.Log(out.Estimate) - (q * SE)
            Dim z As Double = Math.Log(out.Estimate) + (q * SE)
            out.LowerLimit = Math.Exp(y)
            out.UpperLimit = Math.Exp(z)

            'Cornfield confidence intervals
            'lower confidence limit
            Dim OmegaL As Double = out.LowerLimit
            Dim AA As Double = OmegaL * (2 * a + b + c) + (c + d - a - c)
            Dim BB As Double = Math.Sqrt(AA ^ 2 - 4 * (a + b) * (a + c) * OmegaL * (OmegaL - 1))
            Dim n11 As Double = (AA - BB) / (2.0 * (OmegaL - 1.0))
            Dim n12 As Double = a + b - n11
            Dim N21 As Double = a + c - n11
            Dim N22 As Double = n11 - (a + b) - (a + c) + sum
            Dim W As Double = 1 / n11 + 1 / n12 + 1 / N21 + 1 / N22
            Dim Chi2L As Double = (a - n11 - 0.5) ^ 2 * W
            Dim F As Double = Chi2L - q ^ 2
            Do While Math.Abs(F) > 0.0001
                u = (1 / n12 ^ 2) + (1 / N21 ^ 2) - (1 / n11 ^ 2) - 1 / N22 ^ 2
                t = 1 / (2 * (OmegaL - 1) ^ 2) * (BB - sum - (OmegaL - 1) / BB *
                (AA * (2 * a + b + c) - 2 * (a + b) * (a + c) * (2 * OmegaL - 1)))
                V = t * (((a - n11 - 0.5) ^ 2) * u - 2 * W * (a - n11 - 0.5))
                OmegaL = OmegaL - F / V
                AA = OmegaL * (2 * a + b + c) + (c + d - a - c)
                BB = Math.Sqrt(AA ^ 2 - 4 * (a + b) * (a + c) * OmegaL * (OmegaL - 1))
                n11 = (AA - BB) / (2 * (OmegaL - 1))
                n12 = a + b - n11
                N21 = a + c - n11
                N22 = n11 - (a + b) - (a + c) + sum
                W = 1 / n11 + 1 / n12 + 1 / N21 + 1 / N22
                Chi2L = (a - n11 - 0.5) ^ 2 * W
                F = Chi2L - q ^ 2
            Loop

            'upper confidence limit
            Dim OmegaU As Double = out.UpperLimit
            AA = OmegaU * (2 * a + b + c) + (c + d - a - c)
            BB = Math.Sqrt(AA ^ 2 - 4 * (a + b) * (a + c) * OmegaU * (OmegaU - 1))
            n11 = (AA - BB) / (2 * (OmegaU - 1))
            n12 = a + b - n11
            N21 = a + c - n11
            N22 = n11 - (a + b) - (a + c) + sum
            W = 1.0 / n11 + 1 / n12 + 1 / N21 + 1 / N22
            Chi2L = (a - n11 + 0.5) ^ 2 * W
            F = Chi2L - q ^ 2
            Do While Math.Abs(F) > 0.0001
                u = (1.0 / n12 ^ 2) + (1.0 / N21 ^ 2) - (1.0 / n11 ^ 2) - 1 / N22 ^ 2
                t = 1 / (2 * (OmegaU - 1) ^ 2) * (BB - sum - (OmegaU - 1) / BB *
                (AA * (2 * a + b + c) - 2 * (a + b) * (a + c) * (2 * OmegaU - 1)))
                V = t * (((a - n11 + 0.5) ^ 2) * u - 2 * W * (a - n11 + 0.5))
                OmegaU = OmegaU - F / V
                AA = OmegaU * (2 * a + b + c) + (c + d - a - c)
                BB = Math.Sqrt(AA ^ 2 - 4 * (a + b) * (a + c) * OmegaU * (OmegaU - 1))
                n11 = (AA - BB) / (2 * (OmegaU - 1))
                n12 = a + b - n11
                N21 = a + c - n11
                N22 = n11 - (a + b) - (a + c) + sum
                W = 1 / n11 + 1 / n12 + 1 / N21 + 1 / N22
                Chi2L = (a - n11 + 0.5) ^ 2 * W
                F = Chi2L - q ^ 2
            Loop

            Dim out2 As New ConfidenceIntervalResult
            out2.alpha = alpha
            out2.Estimate = out.Estimate
            out2.LowerLimit = OmegaL
            out2.UpperLimit = OmegaU

            Return (out, out2)
        End Function

        ''' <summary>
        ''' Computes the risk ratio (relative risk) from a 2×2 contingency table and its confidence interval.
        ''' </summary>
        ''' <param name="table">
        ''' A two-dimensional integer array representing a 2×2 contingency table:
        ''' <list type="bullet">
        ''' <item><description>Row 0, Col 0: exposed cases (a)</description></item>
        ''' <item><description>Row 0, Col 1: exposed non-cases (b)</description></item>
        ''' <item><description>Row 1, Col 0: unexposed cases (c)</description></item>
        ''' <item><description>Row 1, Col 1: unexposed non-cases (d)</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A <see cref="ConfidenceIntervalResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>Estimate</c>: the risk ratio (relative risk).</description></item>
        ''' <item><description><c>LowerLimit</c>: the lower bound of the confidence interval.</description></item>
        ''' <item><description><c>UpperLimit</c>: the upper bound of the confidence interval.</description></item>
        ''' <item><description><c>strConfidenceInterval</c>: formatted string representation of the interval.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Risk ratio is computed as (a / (a + c)) ÷ (b / (b + d)).  
        ''' - Confidence interval is based on the log risk ratio and its standard error.  
        ''' - Uses the normal approximation with <c>z = NormSInv(1 − alpha / 2)</c>.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: 2×2 table
        ''' Dim table(1,1) As Integer
        ''' table(0,0) = 30 : table(0,1) = 70
        ''' table(1,0) = 10 : table(1,1) = 90
        '''
        ''' Dim result As ConfidenceIntervalResult = RiskRatio(table)
        ''' Console.WriteLine("Risk ratio: "  result.Estimate)
        ''' Console.WriteLine("CI: "  result.strConfidenceInterval)
        ''' ' Output: Risk ratio ≈ 3.86, CI ≈ 2.02 to 7.37
        ''' </example>
        Public Function RiskRatio(table(,) As Integer, Optional alpha As Double = 0.05) As ConfidenceIntervalResult
            Dim a As Double = CDbl(table(0, 0))
            Dim b As Double = CDbl(table(0, 1))
            Dim c As Double = CDbl(table(1, 0))
            Dim d As Double = CDbl(table(1, 1))
            Dim out As New ConfidenceIntervalResult
            out.alpha = alpha
            out.Estimate = (a / (a + c)) / (b / (b + d))
            Dim SE As Double = Math.Sqrt((c / (a * (a + c))) + (d / (b * (b + d))))
            Dim q = distributions.NormSInv(1.0 - alpha / 2.0)
            out.LowerLimit = Math.Exp(Math.Log(out.Estimate) - (q * SE))
            out.UpperLimit = Math.Exp(Math.Log(out.Estimate) + (q * SE))

            Return out
        End Function

        ''' <summary>
        ''' Performs the Cochran–Armitage test for linear trend in proportions across ordered groups.
        ''' </summary>
        ''' <param name="table">
        ''' A two-dimensional integer array representing an r × 2 contingency table:
        ''' <list type="bullet">
        ''' <item><description>Each row corresponds to an ordered group (e.g., dose level).</description></item>
        ''' <item><description>Column 0: number of "successes" in the group.</description></item>
        ''' <item><description>Column 1: number of "failures" in the group.</description></item>
        ''' </list>
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        ''' <item><description><c>TestStatistics1</c>: chi-square statistic for linear trend (Cochran–Armitage).</description></item>
        ''' <item><description><c>Pvalue</c>: p-value for the linear trend test.</description></item>
        ''' <item><description><c>TestStatistics2</c>: chi-square statistic for departure from linearity.</description></item>
        ''' <item><description><c>Pvalue2</c>: p-value for departure from linearity.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Assumes the input table has exactly 2 columns and r rows.  
        ''' - Trend scores are assigned as row indices (0, 1, 2, …).  
        ''' - The test decomposes the overall chi-square into a linear trend component and a residual (departure from linearity).  
        ''' - Based on the Cochran–Armitage method for detecting linear trends in proportions.  
        ''' </remarks>
        ''' <example>
        ''' ' Example: 3 dose levels with binary outcomes
        ''' Dim table(2,1) As Integer
        ''' table(0,0) = 5 : table(0,1) = 15
        ''' table(1,0) = 10 : table(1,1) = 10
        ''' table(2,0) = 20 : table(2,1) = 5
        '''
        ''' Dim result As TestResult = CochranArmitage(table)
        ''' Console.WriteLine("Trend chi-square: "  result.TestStatistics1)
        ''' Console.WriteLine("Trend p-value: "  result.Pvalue)
        ''' Console.WriteLine("Departure chi-square: "  result.TestStatistics2)
        ''' Console.WriteLine("Departure p-value: "  result.Pvalue2)
        ''' </example>
        Public Function CochranArmitage(table(,) As Integer) As TestResult
            'Sub assumes that input table have 2 columns and r rows

            Dim n As Double 'totoal sum
            Dim sum1 As Double, sum2 As Double, Sum3 As Double, expected As Double, r As Double 'R = sum of 1st row

            Dim rowsNo As Integer = table.GetLength(0)
            Dim columnsNo As Integer = table.GetLength(1)
            If columnsNo <> 2 Then Return Nothing 'unexpected table dimension

            Dim RowTot(rowsNo - 1) As Double, TrendScore(rowsNo - 1) As Double

            'rewrite contingencyTable range to table() array
            For i = 0 To rowsNo - 1
                For j = 0 To 1
                    n += table(i, j)
                    r += table(i, 0) / 2
                    RowTot(i) += table(i, j)
                    TrendScore(i) = i
                Next
            Next

            For i = 0 To rowsNo - 1
                sum1 += (table(i, 0) * TrendScore(i))
                sum2 += (TrendScore(i) * (table(i, 0) + table(i, 1)))
                Sum3 += (TrendScore(i) ^ 2 * (table(i, 0) + table(i, 1)))
            Next

            Dim Chi2CochranA As Double = (n * (n * sum1 - r * sum2) ^ 2) / (r * (n - r) * (n * Sum3 - sum2 ^ 2))

            'common chi2 test to calculate departure from linear trend
            Dim Chi2Departure As Double = 0
            For i = 0 To rowsNo - 1 'Do the chi-square sum
                For j = 0 To 1
                    If j = 0 Then
                        expected = (r * RowTot(i)) / n
                        Chi2Departure += (table(i, j) - expected) ^ 2 / expected
                    ElseIf j = 1 Then
                        expected = ((n - r) * RowTot(i)) / n
                        Chi2Departure += (table(i, j) - expected) ^ 2 / expected
                    End If
                Next
            Next

            Dim out = New TestResult
            out.TestStatistics1 = Chi2CochranA 'Cocharn Armitage test for linearity
            out.Pvalue = 1.0 - distributions.ChiSquareCDF(out.TestStatistics1, 1)
            out.TestStatistics2 = Chi2Departure - Chi2CochranA 'departure from linearity (i.e. remaininng non-linearity)
            out.Pvalue2 = 1.0 - distributions.ChiSquareCDF(out.TestStatistics2, rowsNo - 2)

            Return out
        End Function

        ''' <summary>
        ''' Computes ordinal association measures from a contingency table:
        ''' Kendall's Tau-b, Tau-c, Goodman–Kruskal Gamma, and Somers' D.
        ''' </summary>
        ''' <param name="table">
        ''' A two-dimensional integer array representing an r × c contingency table.
        ''' Rows correspond to ordered categories of the independent variable,
        ''' and columns correspond to ordered categories of the dependent variable.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the reported confidence intervals.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A tuple containing four <see cref="TestResult"/> objects:
        ''' <list type="bullet">
        ''' <item><description><c>taub</c>: Kendall's Tau-b statistic, SE, CI, and p-value.</description></item>
        ''' <item><description><c>tauC</c>: Kendall's Tau-c statistic, SE, CI, and p-value.</description></item>
        ''' <item><description><c>Gamma</c>: Goodman–Kruskal Gamma statistic, SE, CI, and p-value.</description></item>
        ''' <item><description><c>SomersD</c>: Somers' D statistic (columns dependent, rows independent), SE, CI, and p-value.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' - Tau-b adjusts for ties in both rows and columns.  
        ''' - Tau-c is suitable for rectangular tables (not just square).  
        ''' - Goodman–Kruskal Gamma ignores ties and measures concordance vs. discordance.  
        ''' - Somers' D is asymmetric: here columns are treated as dependent and rows as independent.  
        ''' - Confidence intervals are computed using normal approximation with z = NormSInv(1 − α/2).  
        ''' </remarks>
        ''' <example>
        ''' ' Example: 3×3 ordinal contingency table
        ''' Dim table(2,2) As Integer
        ''' table(0,0) = 10 : table(0,1) = 5  : table(0,2) = 0
        ''' table(1,0) = 3  : table(1,1) = 15 : table(1,2) = 2
        ''' table(2,0) = 0  : table(2,1) = 4  : table(2,2) = 20
        '''
        ''' Dim result = cTableORDINALassoc(table)
        ''' Console.WriteLine("Tau-b: "  result.Item1.TestStatistics1  " (p="  result.Item1.Pvalue  ")")
        ''' Console.WriteLine("Tau-c: "  result.Item2.TestStatistics1  " (p="  result.Item2.Pvalue  ")")
        ''' Console.WriteLine("Gamma: "  result.Item3.TestStatistics1  " (p="  result.Item3.Pvalue  ")")
        ''' Console.WriteLine("Somers' D: "  result.Item4.TestStatistics1  " (p="  result.Item4.Pvalue  ")")
        ''' </example>
        Public Function cTableORDINALassoc(table(,) As Integer, Optional alpha As Double = 0.05) As (TestResult, TestResult, TestResult, TestResult)
            Dim en1 As Double, en2 As Double, m1 As Integer, m2 As Integer, Mm As Integer
            Dim k As Integer, L As Integer, ki As Integer, kj As Integer, li As Integer, lj As Integer, pairs As Double
            Dim Cij As Double, Dij As Double, SumASE0 As Double
            Dim kk As Integer, LL As Integer, lll As Integer, sum1 As Double, sum2 As Double, p As Double, q As Double
            Dim Mpl As Double, Mmi As Double, Npl As Double, Nmi As Double, m As Double, Dm As Double, tmp As Double 'For Somers D SE
            Dim v_sas2 As Double 'For Somers D - SAS notation
            Dim taub = New TestResult, tauC = New TestResult, Gamma = New TestResult, SomersD = New TestResult

            Dim i As Integer = table.GetLength(0)
            Dim j As Integer = table.GetLength(1)
            Dim minIJ As Integer = Math.Min(i, j)
            Dim nidot_sas(i - 1) As Double

            Dim nn As Integer = i * j 'Total # of entries in contingency table
            Dim points As Double = table(i - 1, j - 1)

            For k = 0 To nn - 2                           'Loop over entries in table,
                ki = RoundDown(k / j, 0)                 'decoding a row index,
                kj = k - j * ki                           'and a column index.
                points += table(ki, kj)

                For L = k + 1 To nn - 1                   'Loop over other member of the pair,
                    li = RoundDown(L / j, 0)             'decoding its row
                    lj = L - j * li                       'and column.
                    m1 = li - ki
                    m2 = lj - kj
                    Mm = m1 * m2
                    pairs = table(ki, kj) * table(li, lj)

                    If Mm <> 0 Then                     'Not a tie.
                        en1 += pairs
                        en2 += pairs
                        If Mm > 0 Then                  'Concordant, or
                            p += pairs
                        Else                              'discordant.
                            q += pairs
                        End If
                    Else                                  'ties
                        If m1 <> 0 Then en1 += pairs
                        If m2 <> 0 Then en2 += pairs
                    End If
                Next
            Next

            'calculate test statistics
            taub.TestStatistics1 = (p - q) / Math.Sqrt(en1 * en2)
            Gamma.TestStatistics1 = (p - q) / (p + q)
            tauC.TestStatistics1 = (minIJ * (p * 2 - q * 2)) / (points ^ 2 * (minIJ - 1))

            'SomersD
            For k = 0 To i - 1
                tmp = 0
                For L = 0 To j - 1
                    Mpl = 0 : Mmi = 0 : Npl = 0 : Nmi = 0
                    tmp = tmp + table(k, L)

                    For kk = 0 To i - 1
                        For LL = 0 To j - 1
                            If kk < k And LL < L Then
                                Mpl += table(kk, LL)
                            ElseIf kk < k And LL > L Then
                                Mmi += table(kk, LL)
                            ElseIf kk > k And LL < L Then
                                Npl += table(kk, LL)
                            ElseIf kk > k And LL > L Then
                                Nmi += table(kk, LL)
                            End If
                        Next
                    Next kk
                    m += (table(k, L) * (Mpl + Nmi - Mmi - Npl) ^ 2)
                Next L
                Dm += tmp * tmp
                nidot_sas(k) = tmp
            Next k
            Dim n_sas As Double = table.Sum2D()
            SomersD.TestStatistics1 = 2.0 * (p - q) / (n_sas ^ 2 - Dm)

            Dim wr_sas As Double = n_sas * n_sas - Dm 'SAS notation

            'calculate Cij and Dij and sums for calculation of standard errors
            sum1 = 0 : sum2 = 0 : SumASE0 = 0
            For k = 0 To i - 1
                For L = 0 To j - 1
                    For kk = 0 To k - 1
                        For LL = 0 To L - 1
                            sum1 += table(kk, LL)
                        Next LL
                        For lll = L + 1 To j - 1
                            sum2 += table(kk, lll)
                        Next lll
                    Next kk
                    For kk = k + 1 To i - 1
                        For LL = L + 1 To j - 1
                            sum1 += table(kk, LL)
                        Next LL
                        For lll = 0 To L - 1
                            sum2 += table(kk, lll)
                        Next lll
                    Next kk
                    Cij = sum1 : Dij = sum2
                    SumASE0 += ((table(k, L) * (Cij - Dij) ^ 2))
                    sum1 = 0 : sum2 = 0

                    'Somers' D SE
                    v_sas2 += (CDbl(table(k, L)) * (wr_sas * (Cij - Dij) - 2 * (p - q) * (n_sas - nidot_sas(k))) ^ 2)
                Next L
            Next k

            'standard errors
            Gamma.DF1 = (2.0 / (p * 2 + q * 2)) * Math.Sqrt(SumASE0 - ((1.0 / points) * (p * 2 - q * 2) ^ 2))
            taub.DF1 = Math.Sqrt((SumASE0 - ((p * 2 - q * 2) ^ 2 / points)) / (en1 * en2))
            tauC.DF1 = ((2 * minIJ) / ((minIJ - 1) * points ^ 2)) * Math.Sqrt((SumASE0 - ((p * 2 - q * 2) ^ 2 / points)))
            SomersD.DF1 = (2.0 / wr_sas ^ 2) * Math.Sqrt(v_sas2)
            'confidence interval at the selected level
            Dim qq = distributions.NormSInv(1.0 - alpha / 2.0)
            taub.strSpecialInformation = $"{Format$(taub.TestStatistics1 - qq * taub.DF1, "0.#########")} to {Format$(taub.TestStatistics1 + qq * taub.DF1, "0.#########")}"
            tauC.strSpecialInformation = $"{Format$(tauC.TestStatistics1 - qq * tauC.DF1, "0.#########")} to {Format$(tauC.TestStatistics1 + qq * tauC.DF1, "0.#########")}"
            Gamma.strSpecialInformation = $"{Format$(Gamma.TestStatistics1 - qq * Gamma.DF1, "0.#########")} to {Format$(Gamma.TestStatistics1 + qq * Gamma.DF1, "0.#########")}"
            SomersD.strSpecialInformation = $"{Format$(SomersD.TestStatistics1 - qq * SomersD.DF1, "0.#########")} to {Format$(SomersD.TestStatistics1 + qq * SomersD.DF1, "0.#########")}"
            'two-sided P-values
            taub.Pvalue = (1.0 - distributions.PNorm(Math.Abs(taub.TestStatistics1 / taub.DF1))) * 2.0
            tauC.Pvalue = (1.0 - distributions.PNorm(Math.Abs(tauC.TestStatistics1 / tauC.DF1))) * 2.0
            Gamma.Pvalue = (1.0 - distributions.PNorm(Math.Abs(Gamma.TestStatistics1 / Gamma.DF1))) * 2.0
            SomersD.Pvalue = (1.0 - distributions.PNorm(Math.Abs(SomersD.TestStatistics1 / SomersD.DF1))) * 2.0

            Return (taub, tauC, Gamma, SomersD)
        End Function

    End Module
End Namespace