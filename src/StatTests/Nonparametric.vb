Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.Linq
Imports System.Security.Cryptography.X509Certificates
Imports System.Security.Policy
Imports System.ServiceModel.Security
Imports System.Text
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports BESHStatNG.Matrix
Imports Microsoft.Office.Interop.Excel




''' <summary>
''' Represents the collection of test statistics, p-values, and auxiliary
''' information returned by a statistical hypothesis test.
''' 
''' This structure is used across multiple procedures (e.g., Mann–Whitney,
''' Wilcoxon, Grubbs, GLM/GEE diagnostics) and supports both exact and
''' asymptotic inference.
''' </summary>
Public Class TestResult

    ''' <summary>
    ''' Two‑sided p‑value based on the primary test statistic
    ''' (<see cref="TestStatistics1"/>).  
    ''' For symmetric distributions, this is typically:
    ''' <code>
    ''' p = 2 * min( P(T ≤ t_obs), P(T ≥ t_obs) )
    ''' </code>
    ''' </summary>
    Public Pvalue As Double

    ''' <summary>
    ''' One‑sided lower‑tail p‑value:
    ''' <c>P(T ≤ t_obs)</c>.
    ''' </summary>
    Public PvalueLowerSide As Double

    ''' <summary>
    ''' One‑sided upper‑tail p‑value:
    ''' <c>P(T ≥ t_obs)</c>.
    ''' </summary>
    Public PvalueUpperSide As Double

    ''' <summary>
    ''' Optional second two‑sided p‑value associated with
    ''' <see cref="TestStatistics2"/> when a test produces two
    ''' related statistics (e.g., U₁ and U₂ in Mann–Whitney).
    ''' </summary>
    Public Pvalue2 As Double

    ''' <summary>
    ''' Exact two‑sided p‑value computed from the exact sampling
    ''' distribution of the statistic (when available).  
    ''' Used for small samples or discrete distributions.
    ''' </summary>
    Public PvalueExact As Double

    ''' <summary>
    ''' Exact lower‑tail p‑value:
    ''' <c>P(T ≤ t_obs)</c> from the exact distribution.
    ''' </summary>
    Public pValueExactLowerSide As Double

    ''' <summary>
    ''' Exact upper‑tail p‑value:
    ''' <c>P(T ≥ t_obs)</c> from the exact distribution.
    ''' </summary>
    Public pValueExactUpperSide As Double

    ''' <summary>
    ''' Primary test statistic.  
    ''' Examples:
    ''' <list type="bullet">
    '''   <item><description>Mann–Whitney U₁</description></item>
    '''   <item><description>t‑statistic</description></item>
    '''   <item><description>Z‑score</description></item>
    '''   <item><description>χ² statistic</description></item>
    ''' </list>
    ''' </summary>
    Public TestStatistics1 As Double

    ''' <summary>
    ''' Secondary test statistic when applicable.  
    ''' For example, Mann–Whitney U₂ = n₁n₂ − U₁.
    ''' </summary>
    Public TestStatistics2 As Double

    ''' <summary>
    ''' Degrees of freedom associated with <see cref="TestStatistics1"/>.
    ''' </summary>
    Public DF1 As Double

    ''' <summary>
    ''' Degrees of freedom associated with <see cref="TestStatistics2"/>,
    ''' if the test produces a second statistic requiring its own df.
    ''' </summary>
    Public DF2 As Double

    ''' <summary>
    ''' Optional textual information returned by the test.  
    ''' Used for procedures that require additional interpretation,
    ''' such as:
    ''' <list type="bullet">
    '''   <item><description>Grubbs outlier test (identifies which point is an outlier)</description></item>
    '''   <item><description>Normality tests (notes on ties or continuity corrections)</description></item>
    '''   <item><description>Warnings about small‑sample adjustments</description></item>
    ''' </list>
    ''' </summary>
    Public strSpecialInformation As String

    ''' <summary>
    ''' Indicates whether exact p‑values were computed and are available.
    ''' </summary>
    Public bExactAvailable As Boolean = False
End Class

Public Enum CIformat
    E_p_LL_to_UL_p = 0
    LL_to_UL = 1
    p_LL_to_UL_p = 2
End Enum

''' <summary>
''' Represents the result of a confidence interval computation, typically for
''' an effect size, parameter estimate, or distributional shift.
''' </summary>
Public Class ConfidenceIntervalResult

    Private pstrConfidenceInterval As String = String.Empty

    ''' <summary>
    ''' Point estimate of the parameter of interest (e.g., mean difference,
    ''' Hodges–Lehmann shift, effect size).
    ''' </summary>
    Public Estimate As Double = Nothing

    ''' <summary>
    ''' Upper bound of the confidence interval.
    ''' </summary>
    Public UpperLimit As Double = Nothing

    ''' <summary>
    ''' Lower bound of the confidence interval.
    ''' </summary>
    Public LowerLimit As Double = Nothing

    ''' <summary>
    ''' Standard error that is used to derive interval (used as applicable).
    ''' </summary>
    Public StdErr As Double = Nothing

    ''' <summary>
    ''' 100 * (1 - alpha) confidence interval is created.
    ''' </summary>
    Public alpha As Double = 0.05

    ''' <summary>
    ''' Preformatted textual representation of the confidence interval,
    ''' typically in the form "estimate (lower to upper)" for reporting.
    ''' </summary>
    Public Property strConfidenceInterval(Optional format As CIformat = CIformat.E_p_LL_to_UL_p) As String
        Get
            If pstrConfidenceInterval = String.Empty Then
                Dim estText As String = FormatDoubleForDisplay(Estimate)
                Dim llText As String = FormatDoubleForDisplay(LowerLimit)
                Dim ulText As String = FormatDoubleForDisplay(UpperLimit)

                If format = CIformat.E_p_LL_to_UL_p Then
                    pstrConfidenceInterval = $"{estText} ({llText} to {ulText})"
                ElseIf format = CIformat.LL_to_UL Then
                    pstrConfidenceInterval = $"{llText} to {ulText}"
                ElseIf format = CIformat.p_LL_to_UL_p Then
                    pstrConfidenceInterval = $"({llText} to {ulText})"
                End If
            End If
            Return pstrConfidenceInterval
        End Get
        Set(value As String)
            pstrConfidenceInterval = value
        End Set
    End Property

    Private Shared Function FormatDoubleForDisplay(x As Double) As String
        If Double.IsNaN(x) Then Return "#N/A"
        If Double.IsPositiveInfinity(x) Then Return "#Pinf"
        If Double.IsNegativeInfinity(x) Then Return "#Ninf"
        Return CStr(CSng(x))
    End Function

    Public ReadOnly Property CIlabel As String
        Get
            Return $"{100.0 * (1.0 - alpha)}% Confidence Interval"
        End Get
    End Property

End Class

Namespace nonparametric
    '------------------------------------------------------------------------------
    ' Mann-Whitney or Wilcoxon ranks sum test
    '------------------------------------------------------------------------------
    ''' <summary>
    ''' Implements the Mann–Whitney U test (also known as the Wilcoxon rank‑sum test),
    ''' including:
    ''' <list type="bullet">
    '''   <item><description>Exact p‑values via dynamic programming (for n ≤ 50)</description></item>
    '''   <item><description>Normal approximation with tie correction</description></item>
    '''   <item><description>Hodges–Lehmann estimator of shift</description></item>
    '''   <item><description>Confidence interval for the shift parameter</description></item>
    '''   <item><description>Summary tables for reporting</description></item>
    ''' </list>
    ''' 
    ''' The test compares two independent samples <c>X</c> and <c>Y</c> and evaluates
    ''' whether one distribution tends to yield larger values than the other.
    ''' 
    ''' Mathematically, the Mann–Whitney U statistic is:
    ''' <code>
    ''' U = Σ Σ I(Xᵢ &lt; Yⱼ)
    ''' </code>
    ''' or equivalently:
    ''' <code>
    ''' U₁ = R₁ − n₁(n₁ + 1)/2
    ''' U₂ = n₁n₂ − U₁
    ''' </code>
    ''' where R₁ is the sum of ranks for group 1.
    ''' 
    ''' Exact p‑values are computed using the distribution of U via dynamic programming.
    ''' For larger samples, a normal approximation with tie correction is used:
    ''' <code>
    ''' Z = (U − n₁n₂/2 + 0.5) / sqrt( n₁n₂(n+1)/12 × (1 − T) )
    ''' </code>
    ''' where T is the tie correction factor.
    ''' </summary>
    Public Class MannWhitney
        ''' <summary>Two‑sample input data: data(0) = group 1, data(1) = group 2.</summary>
        Private data()() As Double

        ''' <summary>Name of the first variable (group 1).</summary>
        Private var1 As String

        ''' <summary>Name of the second variable (group 2).</summary>
        Private var2 As String

        ''' <summary>Combined sample used for tie correction.</summary>
        Private G12() As Double

        ''' <summary>Stores test statistics and p‑values.</summary>
        Private MWresult As TestResult

        ''' <summary>Stores Hodges–Lehmann shift estimate and CI.</summary>
        Private Shift As ConfidenceIntervalResult

        ''' <summary>Indicates whether the shift estimate was computed.</summary>
        Private bShift As Boolean

        ''' <summary>Sample size of group 1.</summary>
        Private n1 As Integer

        ''' <summary>Sample size of group 2.</summary>
        Private n2 As Integer

        ''' <summary>Total sample size n = n1 + n2.</summary>
        Private n As Integer

        ''' <summary>Total number of ties in the combined sample.</summary>
        Private NumberOfTies As Integer

        'Private data()() As Double
        'Private var1 As String
        'Private var2 As String
        'Private G12() As Double
        'Private MWresult As TestResult
        'Private Shift As ConfidenceIntervalResult
        'Private bShift As Boolean
        'Private n1 As Integer 'size of group 1, 2, and totalComb respectively
        'Private n2 As Integer
        'Private n As Integer
        'Private NumberOfTies As Integer 'number of ties

        ''' <summary>
        ''' Initializes a new Mann–Whitney test instance.
        ''' </summary>
        ''' <param name="x">Two‑sample input array: x(0) = group 1, x(1) = group 2.</param>
        ''' <param name="x1name">Name of group 1.</param>
        ''' <param name="x2name">Name of group 2.</param>
        Sub New(x()() As Double, x1name As String, x2name As String)
            Me.data = x
            Me.var1 = x1name
            Me.var2 = x2name
        End Sub

        ''' <summary>
        ''' Produces formatted result tables for reporting the Mann–Whitney test,
        ''' including:
        ''' <list type="bullet">
        '''   <item><description>Sample sizes</description></item>
        '''   <item><description>Medians and quartiles</description></item>
        '''   <item><description>U statistics</description></item>
        '''   <item><description>Exact and asymptotic p‑values</description></item>
        '''   <item><description>Optional Hodges–Lehmann shift estimate</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>QuartilesComp</c> — computes Q1, median, Q3</description></item>
        '''   <item><description><c>HorizontalStackArrays</c> — merges tables</description></item>
        '''   <item><description><c>ResultTable</c> — table formatting class</description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>A list of formatted result tables.</returns>
        Public Function wrapResults() As List(Of ResultTable)
            Dim out = New List(Of ResultTable)
            Dim t = New ResultTable
            Dim MWout1(,) As Object, quartiles1 As udQuartiles, quartiles2 As udQuartiles
            Dim pexactOut(,) As Object, shiftOut(,) As Object = Nothing
            quartiles1 = QuartilesComp(Me.data(0))
            quartiles2 = QuartilesComp(Me.data(1))

            If Me.MWresult.bExactAvailable Then
                pexactOut = {{"Exact Two sided p-value", Me.MWresult.PvalueExact, Me.var1 & " tends to be distributed differently to " & Me.var2},
                      {"Exact Low-side p-value", Me.MWresult.pValueExactLowerSide, Me.var1 & " tends to have smaller values than " & Me.var2},
                      {"Exact Upper-side p-value", Me.MWresult.pValueExactUpperSide, Me.var1 & " tends to have larger values than " & Me.var2}
                     }
            Else
                pexactOut = {{"Exact Two sided p-value", "NE"}}
            End If

            MWout1 = {{"n", Me.n1, Me.n2},
                  {"Median", quartiles1.Median, quartiles2.Median},
                  {"Q1", quartiles1.Q1, quartiles2.Q1},
                  {"Q3", quartiles1.Q3, quartiles2.Q3},
                  {"Test statistic U", Me.MWresult.TestStatistics1, Me.MWresult.TestStatistics2},
                  {"Two sided p-value", Me.MWresult.Pvalue, "Normal approx. (ties, continuity corrected)"}
                 }

            'put all together
            t.AddHeaderTopRow({"Mann-Whitney test", Me.var1, Me.var2})
            t.SetBody(HorizontalStackArrays(MWout1, pexactOut, True))
            out.Add(t)

            If Me.bShift Then
                t = New ResultTable
                t.AddHeaderTopRow({"Hodges-Lehmann estimate of shift", ""})
                t.SetBody({{"mean/median diff (95%CI)", Me.Shift.strConfidenceInterval}})
                out.Add(t)
            End If

            Return out
        End Function

        ''' <summary>
        ''' Computes the Hodges–Lehmann estimator of shift between two distributions.
        ''' 
        ''' The estimator is the median of all pairwise differences:
        ''' <code>
        ''' Δ = median( Xᵢ − Yⱼ )
        ''' </code>
        ''' 
        ''' Confidence intervals are computed using:
        ''' <list type="bullet">
        '''   <item><description>Exact quantiles (n₁,n₂ ≤ 20)</description></item>
        '''   <item><description>Normal approximation for moderate sizes</description></item>
        '''   <item><description>Direct indexing for very large samples</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>Median</c> — computes sample median</description></item>
        '''   <item><description><c>Percentile_Exc</c> — Excel‑style percentile</description></item>
        '''   <item><description><c>MannWhitneyQuantiles</c> — exact quantile table</description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>A <see cref="ConfidenceIntervalResult"/> containing estimate and CI.</returns>
        Public Function ComputeShift() As ConfidenceIntervalResult
            'Hodges-Lehmann estimate of shift

            Dim diffs() As Double, Quantile As Integer, low_k As Double, up_k As Double, k As Integer, G As Integer
            Me.Shift = New ConfidenceIntervalResult
            ReDim diffs(Me.n1 * Me.n2 - 1)

            For i = 0 To Me.n1 - 1
                For j = 0 To Me.n2 - 1 'witin each i iteration the differences are already sorted because we have sorted input arrays
                    diffs(G) = Me.data(0)(i) - Me.data(1)(j)
                    G += 1
                Next
            Next

            Array.Sort(diffs)

            If Me.n1 <= 20 And Me.n2 <= 20 Then
                Me.Shift.Estimate = Median(diffs)
                Quantile = MannWhitneyQuantiles(Me.n1, Me.n2)
                k = Quantile - Me.n1 * (Me.n1 + 1) / 2
                Me.Shift.LowerLimit = diffs(k - 1)
                Me.Shift.UpperLimit = diffs(n1 * n2 - (k - 1) - 1)

            ElseIf Me.n1 * Me.n2 < 1048576 Then 'use build in excel functions to find median/quantiles
                Me.Shift.Estimate = Median(diffs)

                Quantile = CDbl(Me.n1) * (Me.n + 1.0) / 2.0 - 1.96 * Math.Sqrt(CDbl(Me.n1) * CDbl(Me.n2) * (Me.n + 1) / 12.0)
                k = Quantile - Me.n1 * (Me.n1 + 1) / 2
                low_k = CDbl(k) / (CDbl(Me.n1) * CDbl(Me.n2))
                up_k = 1.0# - low_k

                Me.Shift.LowerLimit = Percentile_Exc(diffs, low_k)
                Me.Shift.UpperLimit = Percentile_Exc(diffs, up_k)
            Else
                Me.Shift.Estimate = diffs(CLng((n1 * n2) / 2))
                Quantile = CDbl(Me.n1) * (Me.n + 1.0) / 2.0 - 1.96 * Math.Sqrt(CDbl(Me.n1) * CDbl(n2) * (Me.n + 1.0) / 12.0)
                k = Quantile - Me.n1 * (Me.n1 + 1) / 2
                Me.Shift.LowerLimit = diffs(k - 1)
                Me.Shift.UpperLimit = diffs(Me.n1 * Me.n2 - (k - 1) - 1)
            End If

            Me.bShift = True
            Return Me.Shift
        End Function

        ''' <summary>
        ''' Computes the Mann–Whitney U test, including:
        ''' <list type="bullet">
        '''   <item><description>Rank assignment with tie averaging</description></item>
        '''   <item><description>U statistics U₁ and U₂</description></item>
        '''   <item><description>Exact p‑values via dynamic programming (n ≤ 50)</description></item>
        '''   <item><description>Normal approximation with tie correction</description></item>
        ''' </list>
        ''' 
        ''' Exact p‑value computation:
        ''' <para>
        ''' Uses dynamic programming to enumerate all possible allocations of ranks
        ''' to the smaller group. This yields the exact distribution of U.
        ''' </para>
        ''' 
        ''' Normal approximation:
        ''' <code>
        ''' Z = (U − n₁n₂/2 + 0.5) / sqrt( n₁n₂(n+1)/12 × (1 − T) )
        ''' </code>
        ''' where T is the tie correction factor:
        ''' <code>
        ''' T = Σ (tᵢ³ − tᵢ) / (n³ − n)
        ''' </code>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>TiesCorrection</c> — computes tie correction factor</description></item>
        '''   <item><description><c>PNorm</c> — normal CDF</description></item>
        '''   <item><description><c>ChiSquareCDF</c> — chi‑square CDF</description></item>
        '''   <item><description><c>ConcatArrays</c> — merges samples</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="progressBar">Optional progress bar for exact computation.</param>
        ''' <returns>A <see cref="TestResult"/> containing U statistics and p‑values.</returns>
        Public Function Compute(Optional progressBar As System.Windows.Forms.ProgressBar = Nothing) As TestResult
            'calculate value of U statistics and p-values from group1 and group2 values

            Dim P_value As Double, u As Double
            Me.MWresult = New TestResult

            'get # of valid values
            'ignore - Empty cells, logical values, text, or error values
            n1 = Me.data(0).Length
            n2 = Me.data(1).Length
            n = n1 + n2

            ' Combine and iRank all observations
            Dim combined As New List(Of KeyValuePair(Of Double, Integer))
            For i = 0 To n1 - 1
                combined.Add(New KeyValuePair(Of Double, Integer)(Me.data(0)(i), 1))
            Next
            For i = 0 To n2 - 1
                combined.Add(New KeyValuePair(Of Double, Integer)(Me.data(1)(i), 2))
            Next
            combined.Sort(Function(a, b) a.Key.CompareTo(b.Key))

            ' Assign ranks (with ties averaged)
            Dim start As Integer, iRank As Integer = 1, endRank As Integer
            Dim avgRank As Double
            Dim ranks(n - 1) As Double
            While iRank <= n
                start = iRank - 1
                endRank = start
                While endRank + 1 < n AndAlso combined(endRank + 1).Key = combined(start).Key
                    endRank += 1
                End While
                avgRank = (iRank + endRank + 1) / 2.0
                For i = start To endRank
                    ranks(i) = avgRank
                Next
                iRank = endRank + 2
            End While

            ' Observed iRank sum for group1
            Dim rankSum1 As Double
            For i = 0 To n - 1
                If combined(i).Value = 1 Then rankSum1 += ranks(i)
            Next

            'Test statistics
            Dim U1 As Double = rankSum1 - n1 * (n1 + 1) / 2.0
            Dim U2 As Double = Me.n1 * Me.n2 - U1 'calculate U for 2nd group (i.e. U2)

            U1 = rankSum1 - n1 * (n1 + 1) / 2.0
            U2 = n1 * n2 - U1
            Dim Uobs As Double = Math.Min(U1, U2)

            ' Choose smaller group to assign dynamically
            Dim smallerGroupLabel As Integer = If(U1 <= U2, 1, 0)
            Dim smallerSize As Integer = If(smallerGroupLabel = 1, n1, n2)
            Dim largerSize As Integer = n - smallerSize

            If n <= 50 Then 'exact calculation
                ' --- Step 1:Build distribution using dynamic programming (dictionary) ---
                ' Dynamic programming: key = (count, U), value = ways
                Dim dp As New Dictionary(Of Tuple(Of Integer, Double), Long)
                dp(Tuple.Create(0, 0.0)) = 1
                Dim maxRankSum = smallerSize * n

                For k = 0 To n - 1
                    Dim newDp As New Dictionary(Of Tuple(Of Integer, Double), Long)
                    For Each kvp In dp
                        Dim count = kvp.Key.Item1
                        Dim u_ = kvp.Key.Item2
                        Dim ways = kvp.Value

                        ' Option 1: assign to smaller group
                        If count < smallerSize Then
                            Dim newU = u_ + ranks(k)
                            ' Prune: skip if newU exceeds max possible rank sum

                            If newU <= maxRankSum Then
                                Dim key1 = Tuple.Create(count + 1, newU)
                                If Not newDp.ContainsKey(key1) Then newDp(key1) = 0
                                newDp(key1) += ways
                            End If
                        End If

                        ' Option 2: assign to larger group (no U change)
                        Dim key2 = Tuple.Create(count, u_)
                        If Not newDp.ContainsKey(key2) Then newDp(key2) = 0
                        newDp(key2) += ways
                    Next
                    dp = newDp
                Next

                ' Compute U distribution for smaller group
                Dim totalComb As Long = 0L 'dp.Values.Sum()
                Dim pLower As Long = 0L
                Dim pUpper As Long = 0L, s As Long = 0L, iUpdate As Long
                iUpdate = 100L
                If iUpdate = 0 Then iUpdate = 1
                For Each kvp In dp
                    If kvp.Key.Item1 = smallerSize Then
                        totalComb += kvp.Value
                        Dim Uval As Double = If(smallerGroupLabel = 1,
                                        kvp.Key.Item2 - smallerSize * (smallerSize + 1) / 2.0,
                                        largerSize * smallerSize - (kvp.Key.Item2 - smallerSize * (smallerSize + 1) / 2.0))
                        If Uval <= Uobs Then pLower += kvp.Value
                        If Uval > Uobs Then pUpper += kvp.Value
                    End If

                    If progressBar IsNot Nothing Then
                        If s Mod iUpdate = 0 Then
                            progressBar.Invoke(Sub()
                                                   progressBar.Value = 100 * s / totalComb
                                               End Sub)
                            System.Windows.Forms.Application.DoEvents()
                        End If
                        s += 1
                    End If
                Next
                If progressBar IsNot Nothing Then progressBar.Invoke(Sub()
                                                                         progressBar.Value = 100
                                                                     End Sub)

                Me.MWresult.bExactAvailable = True
                Me.MWresult.pValueExactLowerSide = pLower / totalComb
                Me.MWresult.pValueExactUpperSide = pUpper / totalComb
                Me.MWresult.PvalueExact = Math.Min(1.0, 2.0 * Math.Min(Me.MWresult.pValueExactLowerSide, Me.MWresult.pValueExactUpperSide))
            End If

            'Normal approximation
            u = Math.Min(U1, U2) 'smaller value represent the U statistics
            'ties corrected normal approximation p-value (continutity corrected)
            Me.G12 = ConcatArrays(Me.data(0), Me.data(1))
            Dim Cties As Double = TiesCorrection(Me.G12)
            Cties /= (Me.n ^ 3 - Me.n)
            Dim sig As Double = ((CDbl(Me.n1) * CDbl(Me.n2) * (Me.n + 1.0)) / 12.0) * (1.0 - Cties) 'using CDbl because of othe verflow error with large sample sizes
            Dim z As Double = (u - (Me.n1 * Me.n2) / 2.0 + 0.5) / Math.Sqrt(sig)

            'we want negative Z, therefore -abs(Z) to obtain one side p-value from distribution function
            P_value = 2.0 * distributions.PNorm(-Math.Abs(z))

            'output
            Me.MWresult.Pvalue = P_value
            Me.MWresult.TestStatistics1 = U1
            Me.MWresult.TestStatistics2 = U2

            Return Me.MWresult
        End Function

        ' ---------------------------------------
        ' Helper functions
        ' ---------------------------------------
        ''' <summary>
        ''' Returns exact Mann–Whitney quantiles for small samples using
        ''' Conover (1999), Practical Nonparametric Statistics, Table A7.
        ''' 
        ''' Used for exact Hodges–Lehmann confidence intervals.
        ''' </summary>
        ''' <param name="n">Sample size of group 1.</param>
        ''' <param name="m">Sample size of group 2.</param>
        ''' <returns>The table lookup quantile.</returns>
        Private Function MannWhitneyQuantiles(n As Integer, m As Integer) As Integer
            'Quantiles of Mann-Whitney statistics, Table A7 (Conover, Practival Nonparametric Statistics 3rd ed., 1999)
            Dim W(,) As Integer = {{3, 3, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6},
            {6, 6, 6, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15},
            {10, 10, 11, 12, 13, 14, 15, 15, 16, 17, 18, 19, 20, 21, 22, 22, 23, 24, 25},
            {15, 16, 17, 18, 19, 21, 22, 23, 24, 25, 27, 28, 29, 30, 31, 33, 34, 35, 39},
            {21, 23, 24, 25, 27, 28, 30, 32, 33, 35, 36, 38, 39, 41, 43, 44, 46, 47, 49},
            {28, 30, 32, 34, 35, 37, 39, 41, 43, 45, 47, 49, 51, 53, 55, 57, 59, 61, 63},
            {37, 39, 41, 43, 45, 47, 50, 52, 54, 56, 59, 61, 63, 66, 68, 71, 73, 75, 78},
            {46, 48, 50, 53, 56, 58, 61, 63, 66, 69, 72, 74, 77, 80, 83, 85, 88, 91, 94},
            {56, 59, 61, 64, 67, 70, 73, 76, 79, 82, 85, 89, 92, 95, 98, 101, 104, 108, 111},
            {67, 70, 73, 76, 80, 83, 86, 90, 93, 97, 100, 104, 107, 111, 114, 118, 122, 125, 129},
            {80, 83, 86, 90, 93, 97, 101, 105, 108, 112, 116, 120, 124, 128, 132, 136, 140, 144, 148},
            {93, 96, 100, 104, 108, 112, 116, 120, 125, 129, 133, 137, 142, 146, 151, 155, 159, 164, 168},
            {107, 111, 115, 119, 123, 128, 132, 137, 142, 146, 151, 156, 161, 165, 170, 175, 180, 184, 189},
            {122, 126, 131, 135, 140, 145, 150, 155, 160, 165, 170, 175, 180, 185, 191, 196, 201, 206, 211},
            {138, 143, 148, 152, 158, 163, 168, 174, 179, 184, 190, 196, 201, 207, 212, 218, 223, 229, 235},
            {156, 160, 165, 171, 176, 182, 188, 193, 199, 205, 211, 217, 223, 229, 235, 241, 247, 253, 259},
            {174, 179, 184, 190, 196, 202, 208, 214, 220, 227, 233, 239, 246, 252, 258, 265, 271, 278, 284},
            {193, 198, 204, 211, 216, 223, 229, 236, 243, 249, 256, 263, 269, 276, 283, 290, 297, 304, 310},
            {213, 219, 225, 231, 238, 245, 251, 259, 266, 273, 280, 287, 294, 301, 309, 316, 323, 330, 338}
        }

            Return W(n - 2, m - 2) 'minus two because w is zero based and original table starts at (2,2)
        End Function
    End Class


    '------------------------------------------------------------------------------
    ' Wilcoxon signed ranks test
    '------------------------------------------------------------------------------
    ''' <summary>
    ''' Implements the Wilcoxon Signed-Rank Test for paired or matched samples.
    ''' 
    ''' This nonparametric test evaluates whether the median of paired differences
    ''' differs from zero. It is appropriate when:
    ''' <list type="bullet">
    '''   <item><description>The data consist of paired observations (Xᵢ, Yᵢ)</description></item>
    '''   <item><description>The distribution of differences is symmetric</description></item>
    '''   <item><description>The measurement scale is at least ordinal</description></item>
    ''' </list>
    ''' 
    ''' The test statistic W is the sum of ranks of positive differences:
    ''' <code>
    ''' dᵢ = Xᵢ − Yᵢ
    ''' W = Σ rank(|dᵢ|) for dᵢ > 0
    ''' </code>
    ''' 
    ''' Exact p-values are computed via dynamic programming for n ≤ 60.
    ''' For larger samples, a normal approximation with tie correction and
    ''' continuity correction is used:
    ''' <code>
    ''' Z = (W − 0.5 − n(n+1)/4) / sqrt( (n(n+1)(2n+1) − Σ(tᵢ³ − tᵢ)/2) / 24 )
    ''' </code>
    ''' 
    ''' The class also computes:
    ''' <list type="bullet">
    '''   <item><description>Hodges–Lehmann estimator of shift</description></item>
    '''   <item><description>Confidence interval for the shift</description></item>
    '''   <item><description>Optional Sign Test</description></item>
    ''' </list>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>ComputeAvgRanks</c> — computes average ranks with ties</description></item>
    '''   <item><description><c>TiesCorrection</c> — tie correction factor</description></item>
    '''   <item><description><c>PNorm</c> — normal CDF</description></item>
    '''   <item><description><c>BinomDist</c> — binomial distribution CDF</description></item>
    '''   <item><description><c>Median</c> — sample median</description></item>
    '''   <item><description><c>HorizontalStackArrays</c> — table formatting</description></item>
    '''   <item><description><c>ResultTable</c>, <c>TestResult</c>, <c>ConfidenceIntervalResult</c></description></item>
    ''' </list>
    ''' </summary>
    Public Class WilcoxonTest
        ''' <summary>Stores the Wilcoxon test results (statistics and p-values).</summary>
        Private WilcoxonTestresult As TestResult

        ''' <summary>Stores the Hodges–Lehmann shift estimate and confidence interval.</summary>
        Private Shift As ConfidenceIntervalResult

        ''' <summary>Input paired data matrix: arG12(i,0)=Xᵢ, arG12(i,1)=Yᵢ.</summary>
        Private arG12(,) As Double

        ''' <summary>Sum of ranks for positive differences (W statistic).</summary>
        Private pWpoz As Double

        ''' <summary>Number of non-zero paired differences.</summary>
        Private pNact As Integer

        ''' <summary>Vector of paired differences dᵢ = Xᵢ − Yᵢ.</summary>
        Private pDifferences() As Double

        ''' <summary>Name of the first variable.</summary>
        Private var1 As String

        ''' <summary>Name of the second variable.</summary>
        Private var2 As String

        ''' <summary>Indicates whether the Sign Test was computed.</summary>
        Private pbSignTest As Boolean = False

        ''' <summary>Stores results of the Sign Test.</summary>
        Private pSignTestResults As TestResult = Nothing


        ''' <summary>
        ''' Returns the vector of paired differences dᵢ = Xᵢ − Yᵢ.
        ''' Zero differences are included; they are removed internally for ranking.
        ''' </summary>
        Public ReadOnly Property Differences() As Double()
            Get
                Return pDifferences
            End Get
        End Property


        ''' <summary>
        ''' Initializes a new Wilcoxon Signed-Rank Test instance.
        ''' </summary>
        ''' <param name="data">Paired data matrix: data(i,0)=Xᵢ, data(i,1)=Yᵢ.</param>
        ''' <param name="x1name">Name of the first variable.</param>
        ''' <param name="x2name">Name of the second variable.</param>
        Public Sub New(data(,) As Double, x1name As String, x2name As String)
            Me.arG12 = data
            var1 = x1name
            var2 = x2name
        End Sub

        ''' <summary>
        ''' Produces formatted result tables summarizing:
        ''' <list type="bullet">
        '''   <item><description>Wilcoxon Signed-Rank Test results</description></item>
        '''   <item><description>Exact p-values (if available)</description></item>
        '''   <item><description>Hodges–Lehmann shift estimate</description></item>
        '''   <item><description>Optional Sign Test results</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>HorizontalStackArrays</c></description></item>
        '''   <item><description><c>ResultTable</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>A list of formatted <see cref="ResultTable"/> objects.</returns>
        Public Function wrapResults() As List(Of ResultTable)
            Dim out = New List(Of ResultTable), t = New ResultTable
            Dim signOut(,) As Object, wOut1(,) As Object, wOut2(,) As Object, pexactOut(,) As Object

            If Me.WilcoxonTestresult.bExactAvailable Then
                pexactOut = {{"Exact Two sided p-value", Me.WilcoxonTestresult.PvalueExact},
                      {"Exact Low-side p-value", Me.WilcoxonTestresult.pValueExactLowerSide},
                      {"Exact Upper-side p-value", Me.WilcoxonTestresult.pValueExactUpperSide}}
            Else
                pexactOut = {{"Exact Two sided p-value", "NE"}}
            End If

            If Me.pSignTestResults IsNot Nothing Then
                signOut = {{"Number of positive differences", Me.pSignTestResults.TestStatistics1},
                       {"Two sided p-value", Me.pSignTestResults.Pvalue}}
            Else
                signOut = Nothing
            End If

            'standard Wilcoxon test reustls
            wOut1 = {{"Number of valid data pairs", Me.pNact},
                 {"Sum of ranks (positive differences)", Me.pWpoz},
                 {"Z score", WilcoxonTestresult.TestStatistics1},
                 {"Two sided p-value (ties and continuity corrected)", WilcoxonTestresult.Pvalue}
                }
            wOut2 = {{"mean/median diff (95%CI)", Me.Shift.strConfidenceInterval}}

            'put all together
            t.SetBody(HorizontalStackArrays(wOut1, pexactOut))
            t.AddHeaderTopRow({"Wilcoxon Signed Rank Test", ""})
            out.Add(t)

            t = New ResultTable
            t.SetBody(wOut2)
            t.AddHeaderTopRow({"Hodges-Lehmann estimate of shift", ""})
            out.Add(t)
            If signOut IsNot Nothing Then
                t = New ResultTable
                t.SetBody(signOut)
                t.AddHeaderTopRow({"Sign Test", ""})
                out.Add(t)
            End If
            Return out
        End Function

        ''' <summary>
        ''' Computes the Hodges–Lehmann estimator of shift for paired data.
        ''' 
        ''' The estimator is the median of all Walsh averages:
        ''' <code>
        ''' wᵢⱼ = (dᵢ + dⱼ) / 2
        ''' HL = median( wᵢⱼ )
        ''' </code>
        ''' 
        ''' Confidence intervals:
        ''' <list type="bullet">
        '''   <item><description>Exact quantiles for n ≤ 50 (using table W25)</description></item>
        '''   <item><description>Normal approximation for n > 50</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>Median</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>A <see cref="ConfidenceIntervalResult"/> containing estimate and CI.</returns>
        Public Function ComputeShift() As ConfidenceIntervalResult
            ' Hodges-Lehmann estimate of shift
            Dim Wquantil As Integer, j As Integer
            Dim W25() As Integer = {Nothing, Nothing, Nothing, Nothing, 0, 0, 1, 3, 4, 6, 9, 11, 14, 18, 22, 26, 30, 35, 41,
                                47, 53, 59, 67, 74, 82, 90, 99, 108, 117, 127, 138, 148, 160, 171, 183, 196, 209, 222,
                                236, 250, 265, 280, 295, 311, 328, 344, 362, 379, 397, 416, 435}

            Me.Shift = New ConfidenceIntervalResult
            Dim n As Integer = UBound(arG12, 1) + 1

            'Fit Hodges-Lehmann estimate of shift
            Dim MeanOfDiffs(n * (n - 1) / 2 + n - 1) As Double

            For i = 0 To n - 1
                For ii = i To n - 1
                    MeanOfDiffs(j) = (Me.pDifferences(i) + Me.pDifferences(ii)) / 2.0
                    j += 1
                Next
            Next
            Array.Sort(MeanOfDiffs)

            Me.Shift.Estimate = Median(MeanOfDiffs)

            If n > 3 And n <= 50 Then 'exact quantiles
                Me.Shift.LowerLimit = MeanOfDiffs(W25(n) - 1)
                Me.Shift.UpperLimit = MeanOfDiffs(UBound(MeanOfDiffs) - W25(n))
            ElseIf n > 50 Then 'normal approximation
                Wquantil = (n * (n + 1) / 4) - 1.96 * Math.Sqrt(n * (n + 1) * (2 * n + 1) / 24)
                Me.Shift.LowerLimit = MeanOfDiffs(Wquantil - 1)
                Me.Shift.UpperLimit = MeanOfDiffs(UBound(MeanOfDiffs) - Wquantil - 1)
            End If

            Return Me.Shift
        End Function

        ''' <summary>
        ''' Computes the Wilcoxon Signed-Rank Test, including:
        ''' <list type="bullet">
        '''   <item><description>Removal of zero differences</description></item>
        '''   <item><description>Ranking of absolute differences</description></item>
        '''   <item><description>Computation of W statistic</description></item>
        '''   <item><description>Normal approximation with tie correction</description></item>
        '''   <item><description>Exact p-values via dynamic programming (n ≤ 60)</description></item>
        ''' </list>
        ''' 
        ''' Normal approximation:
        ''' <code>
        ''' Wmean = n(n+1)/4
        ''' Wsd = sqrt( (n(n+1)(2n+1) − Σ(tᵢ³ − tᵢ)/2) / 24 )
        ''' Z = (W − 0.5 − Wmean) / Wsd
        ''' </code>
        ''' 
        ''' Exact p-values:
        ''' <para>
        ''' Computed by enumerating all 2ⁿ sign assignments using dynamic programming
        ''' on scaled integer ranks.
        ''' </para>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>ComputeAvgRanks</c></description></item>
        '''   <item><description><c>TiesCorrection</c></description></item>
        '''   <item><description><c>PNorm</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="progressBar">Optional progress bar for exact computation.</param>
        ''' <returns>A <see cref="TestResult"/> containing W, Z, and p-values.</returns>
        Function Compute(Optional progressBar As System.Windows.Forms.ProgressBar = Nothing) As TestResult
            'pWpoz - sum of ranks for positive differences
            Dim arDiff() As Double, arSign() As Double, Ranks() As Double, nonZeroDiffs() As Double
            Dim SUMties As Double, Wmean As Double, WsdCor As Double, rr() As Double, z As Double
            Me.WilcoxonTestresult = New TestResult

            Dim n As Integer = UBound(arG12, 1) + 1
            ReDim arDiff(n - 1), arSign(n - 1), rr(n - 1), Me.pDifferences(n - 1), nonZeroDiffs(n - 1)

            'get absolute difference between groups and remember sign in 2nd range
            For i = 0 To n - 1
                Me.pDifferences(i) = arG12(i, 0) - arG12(i, 1)
                If Me.pDifferences(i) <> 0 Then
                    Me.pNact += 1
                    nonZeroDiffs(Me.pNact - 1) = Me.pDifferences(i)
                End If
            Next
            ReDim Preserve nonZeroDiffs(Me.pNact - 1) ' Remove zero differences

            If Me.pNact = 0 Then
                With Me.WilcoxonTestresult
                    .TestStatistics1 = 0
                    .Pvalue = 1
                    .PvalueExact = 1
                End With
                Me.pWpoz = 0
                Return Me.WilcoxonTestresult
            End If

            ' Step 2: Rank absolute difference
            Ranks = ComputeAvgRanks(nonZeroDiffs)

            ' Step 3: Compute observed W (sum of positive ranks)
            For i = 0 To Me.pNact - 1
                If nonZeroDiffs(i) > 0 Then Me.pWpoz += Ranks(i)
            Next

            ' Step 4: compute approximate p-value
            'calculate Wmean and Wsd
            SUMties = TiesCorrection(Ranks)
            Wmean = Me.pNact * (Me.pNact + 1) / 4
            WsdCor = Math.Sqrt(((Me.pNact * (Me.pNact + 1) * (2 * Me.pNact + 1)) - 0.5 * SUMties) / 24)
            If WsdCor <> 0 Then z = (Me.pWpoz - 0.5 - Wmean) / WsdCor 'continuity (-0.5), and ties corrected

            With Me.WilcoxonTestresult
                .TestStatistics1 = z
                .Pvalue = 2.0 * distributions.PNorm(-Math.Abs(z)) '*2 to obtain two-tail probability
            End With

            ' Exact pvalue using Dynamic programming distribution
            If Me.pNact <= 60 Then
                Dim totalComb As Long = 1L << CLng(Me.pNact)
                Dim iUpdate As Long

                ' Step 6: Scale ranks by 2 to handle fractional ranks as integers
                Dim scaledRanks(Me.pNact - 1) As Integer
                For i = 0 To Me.pNact - 1
                    scaledRanks(i) = CInt(Ranks(i) * 2)
                Next
                Dim maxRankSum As Long = scaledRanks.Sum()
                iUpdate = CLng(maxRankSum / 100L)
                Dim extremeCount As Long, extremeCountLower As Long, extremeCountUpper As Long
                Dim meanW = maxRankSum / 2.0
                ' Distribution array: counts of combinations yielding sum s
                Dim dist(maxRankSum) As Long
                dist(0) = 1  ' base case
                For Each r In scaledRanks
                    For s As Integer = maxRankSum - r To 0 Step -1
                        dist(s + r) += dist(s)
                    Next
                Next
                Dim distObs = Math.Abs(Me.pWpoz * 2.0 - meanW) ' scale observed W
                For s As Integer = 0 To maxRankSum
                    'Two sided test
                    If Math.Abs(s - meanW) >= distObs Then extremeCount += dist(s)
                    ' Left-tailed test: P(W ≤ Wobs)
                    If s <= (Me.pWpoz * 2.0) Then extremeCountLower += dist(s)
                    ' Right-tailed test: P(W ≥ Wobs)
                    If s >= (Me.pWpoz * 2.0) Then extremeCountUpper += dist(s)

                    If progressBar IsNot Nothing Then
                        If s Mod iUpdate = 0 Then
                            Dim k As Integer = s
                            progressBar.Invoke(Sub()
                                                   progressBar.Value = 100 * k / maxRankSum
                                               End Sub)
                            System.Windows.Forms.Application.DoEvents()
                        End If
                    End If
                Next
                If progressBar IsNot Nothing Then progressBar.Invoke(Sub()
                                                                         progressBar.Value = 100
                                                                     End Sub)

                Me.WilcoxonTestresult.bExactAvailable = True
                Me.WilcoxonTestresult.PvalueExact = extremeCount / CDbl(totalComb)
                Me.WilcoxonTestresult.pValueExactLowerSide = extremeCountLower / CDbl(totalComb)
                Me.WilcoxonTestresult.pValueExactUpperSide = extremeCountUpper / CDbl(totalComb)
            Else
                Me.WilcoxonTestresult.bExactAvailable = False
            End If

            Return Me.WilcoxonTestresult
        End Function

        ''' <summary>
        ''' Computes the Sign Test as a complementary nonparametric test.
        ''' 
        ''' The Sign Test counts positive and negative differences:
        ''' <code>
        ''' N₊ = #{dᵢ > 0}
        ''' N₋ = #{dᵢ .lt. 0}
        ''' </code>
        ''' 
        ''' Under the null hypothesis median(dᵢ) = 0:
        ''' <code>
        ''' N₊ ~ Binomial(N₊ + N₋, 0.5)
        ''' </code>
        ''' 
        ''' Two-sided p-value:
        ''' <code>
        ''' p = 2 * BinomCDF( min(N₊, N₋), N, 0.5 )
        ''' </code>
        ''' 
        ''' External dependency:
        ''' <list type="bullet">
        '''   <item><description><c>BinomDist</c> — binomial CDF</description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>A <see cref="TestResult"/> containing Sign Test results.</returns>
        Public Function signTest() As TestResult
            Dim Nneg As Integer, Npoz As Integer
            Me.pSignTestResults = New TestResult

            'count positive and negative differences
            For i = 0 To UBound(arG12, 1)
                If arG12(i, 0) - arG12(i, 1) > 0 Then
                    Npoz += 1
                ElseIf arG12(i, 0) - arG12(i, 1) < 0 Then
                    Nneg += 1
                End If
            Next

            Dim min As Double = Math.Min(Npoz, Nneg)

            Me.pSignTestResults.Pvalue = 2.0 * distributions.BinomDist(min, Npoz + Nneg, 0.5, True)
            Me.pSignTestResults.TestStatistics1 = Npoz

            Return Me.pSignTestResults
        End Function

    End Class



    '------------------------------------------------------------------------------
    ' Spearman rank correlation coneficient Rho
    '------------------------------------------------------------------------------
    ''' <summary>
    ''' Computes Spearman's rank correlation coefficient (ρ), including:
    ''' <list type="bullet">
    '''   <item><description>Rank transformation of X and Y</description></item>
    '''   <item><description>Exact permutation p-values for n ≤ 10</description></item>
    '''   <item><description>Edgeworth-series approximation for 4 ≤ n ≤ 50 (no ties)</description></item>
    '''   <item><description>t-distribution approximation for general n</description></item>
    '''   <item><description>Fisher z-transformation confidence interval at level <c>1 - alpha</c></description></item>
    ''' </list>
    ''' 
    ''' Spearman's ρ is defined as the Pearson correlation of the ranked variables:
    ''' <code>
    ''' ρ = cor(rank(X), rank(Y))
    ''' </code>
    ''' 
    ''' When no ties are present, the classical formula applies:
    ''' <code>
    ''' ρ = 1 − (6 Σ dᵢ²) / (n(n² − 1))
    ''' </code>
    ''' where <c>dᵢ = rank(Xᵢ) − rank(Yᵢ)</c>.
    ''' 
    ''' Exact p-values are computed by permutation enumeration.
    ''' For larger samples or when ties are present, an approximate t-based test is used:
    ''' <code>
    ''' t = ρ √((n − 2) / (1 − ρ²))
    ''' </code>
    ''' 
    ''' The confidence interval is based on the Fisher z transform:
    ''' <code>
    ''' z = atanh(ρ)
    ''' CI = tanh(z ± z_(1 − alpha/2) / √(n − 3))
    ''' </code>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>ComputeAvgRanks</c> — rank computation with ties</description></item>
    '''   <item><description><c>PNorm</c> — normal CDF</description></item>
    '''   <item><description><c>T_2T</c>, <c>T_RT</c>, <c>T_CDF</c> — t-distribution functions</description></item>
    '''   <item><description><c>Atanh</c> — inverse hyperbolic tangent</description></item>
    '''   <item><description><c>ZCritTwoSided</c> — two-sided normal critical value</description></item>
    '''   <item><description><c>HorizontalStackArrays</c> — table formatting</description></item>
    '''   <item><description><c>ResultTable</c>, <c>TestResult</c>, <c>ConfidenceIntervalResult</c></description></item>
    ''' </list>
    ''' </summary>
    Public Class SpearmanRho

        ''' <summary>Stores test statistics and p-values for Spearman's ρ.</summary>
        Private Protected CorrelationResult As TestResult

        ''' <summary>Input vector X.</summary>
        Private Protected X() As Double

        ''' <summary>Input vector Y.</summary>
        Private Protected Y() As Double

        ''' <summary>Name of variable X.</summary>
        Private Protected var1 As String

        ''' <summary>Name of variable Y.</summary>
        Private Protected var2 As String

        ''' <summary>Confidence interval for Spearman's ρ.</summary>
        Private Protected correlationCI As ConfidenceIntervalResult

        ''' <summary>Sample size.</summary>
        Private Protected n As Integer

        ''' <summary>
        ''' Initializes a new Spearman rank correlation test.
        ''' </summary>
        ''' <param name="x">Vector X.</param>
        ''' <param name="y">Vector Y.</param>
        ''' <param name="xname">Name of X.</param>
        ''' <param name="yname">Name of Y.</param>
        Sub New(x() As Double, y() As Double, xname As String, yname As String)
            Me.X = x
            Me.Y = y
            var1 = xname
            var2 = yname
        End Sub

        ''' <summary>
        ''' Returns the Spearman correlation coefficient ρ.
        ''' </summary>
        ReadOnly Property correlCoef() As Double
            Get
                Return CorrelationResult.TestStatistics1
            End Get
        End Property

        ''' <summary>
        ''' Returns the two‑sided p‑value (approximate or exact).
        ''' </summary>
        ReadOnly Property pvalue() As Double
            Get
                Return CorrelationResult.Pvalue
            End Get
        End Property

        ''' <summary>
        ''' Produces formatted result tables summarizing:
        ''' <list type="bullet">
        '''   <item><description>Sample size</description></item>
        '''   <item><description>Spearman's ρ</description></item>
        '''   <item><description>Approximate confidence interval at level <c>1 - alpha</c></description></item>
        '''   <item><description>Approximate p-values</description></item>
        '''   <item><description>Exact p-values (if available)</description></item>
        ''' </list>
        ''' 
        ''' External dependency:
        ''' <c>HorizontalStackArrays</c> for table formatting.
        ''' </summary>
        ''' <returns>A list of <see cref="ResultTable"/> objects.</returns>
        Public Function wrapResults() As List(Of ResultTable)
            Dim out = New List(Of ResultTable), t = New ResultTable, pexactOut(,) As Object

            If Me.CorrelationResult.bExactAvailable Then
                pexactOut = {{"Exact Two sided p-value", Me.CorrelationResult.PvalueExact},
                      {"Exact Low-side p-value", Me.CorrelationResult.pValueExactLowerSide},
                      {"Exact Upper-side p-value", Me.CorrelationResult.pValueExactUpperSide}
                     }
            Else
                pexactOut = {{"Exact Two sided p-value", "NE"}}
            End If

            Dim o = {
                {"Number of valid data pairs", Me.n},
                {"Rho", Me.CorrelationResult.TestStatistics1},
                {"Approximate " & Me.correlationCI.CIlabel, Me.correlationCI.strConfidenceInterval(CIformat.LL_to_UL)},
                {"Two-sided p-value (approx.)", Me.CorrelationResult.Pvalue},
                {"Low-side p-value (approx.)", Me.CorrelationResult.PvalueLowerSide},
                {"Upper-sid p-value (approx.)", Me.CorrelationResult.PvalueUpperSide}
            }
            t.SetBody(HorizontalStackArrays(o, pexactOut))
            t.AddHeaderTopRow({"Spearman rank correlation coefficient", ""})
            out.Add(t)
            Return out
        End Function

        ''' <summary>
        ''' Computes Spearman's rank correlation coefficient as the Pearson
        ''' correlation of ranked variables:
        ''' <code>
        ''' ρ = cov(rank(X), rank(Y)) / (sd(rank(X)) sd(rank(Y)))
        ''' </code>
        ''' 
        ''' This implementation uses centered ranks and avoids overflow for large n.
        ''' </summary>
        ''' <param name="xRanks">Rank-transformed X.</param>
        ''' <param name="yRanks">Rank-transformed Y.</param>
        ''' <returns>Spearman's ρ.</returns>
        Private Function correlationCoefficient(xRanks As Double(), yRanks As Double()) As Double
            'compute spearman Rho
            Dim n As Integer = xRanks.Length
            Dim meanX As Double = xRanks.Average()
            Dim meanY As Double = yRanks.Average()

            Dim numerator As Double = 0.0
            Dim sumSqX As Double = 0.0
            Dim sumSqY As Double = 0.0

            For i = 0 To n - 1
                Dim dx = xRanks(i) - meanX
                Dim dy = yRanks(i) - meanY
                numerator += dx * dy
                sumSqX += dx * dx
                sumSqY += dy * dy
            Next

            If sumSqX = 0 OrElse sumSqY = 0 Then Return 0.0 ' Avoid division by zero

            Return numerator / Math.Sqrt(sumSqX * sumSqY)
        End Function

        ''' <summary>
        ''' Computes Spearman's rank correlation test, including:
        ''' <list type="bullet">
        '''   <item><description>Rank transformation of X and Y</description></item>
        '''   <item><description>Exact permutation p-values for n ≤ 10</description></item>
        '''   <item><description>Edgeworth-series approximation (AS 89) for 4 ≤ n ≤ 50 without ties</description></item>
        '''   <item><description>t-distribution approximation for general n</description></item>
        '''   <item><description>Fisher z-transformation confidence interval at level <c>1 - alpha</c></description></item>
        ''' </list>
        ''' 
        ''' Exact permutation test:
        ''' <para>
        ''' Enumerates permutations of Y (or unique permutations when ties are present),
        ''' computes ρ for each permutation, and compares it with the observed value.
        ''' </para>
        ''' 
        ''' Approximate p-values:
        ''' <code>
        ''' t = ρ √((n − 2) / (1 − ρ²))
        ''' p = 2 * (1 − T_CDF(|t|))
        ''' </code>
        ''' 
        ''' Confidence interval:
        ''' <code>
        ''' z = atanh(ρ)
        ''' CI = tanh(z ± z_(1 − alpha/2) / √(n − 3))
        ''' </code>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>ComputeAvgRanks</c></description></item>
        '''   <item><description><c>PNorm</c></description></item>
        '''   <item><description><c>T_2T</c>, <c>T_RT</c>, <c>T_CDF</c></description></item>
        '''   <item><description><c>Atanh</c></description></item>
        '''   <item><description><c>ZCritTwoSided</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="progressBar">Optional progress bar for permutation enumeration.</param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the confidence interval.
        ''' For example, <c>alpha = 0.05</c> gives a 95% confidence interval.
        ''' </param>
        ''' <returns>A <see cref="TestResult"/> containing ρ and p-values.</returns>
        Function Compute(Optional progressBar As System.Windows.Forms.ProgressBar = Nothing, Optional alpha As Double = 0.05) As TestResult
            Me.CorrelationResult = New TestResult
            Me.n = Me.X.Length
            Dim xRanks = ComputeAvgRanks(X)
            Dim yRanks = ComputeAvgRanks(Y)
            Dim rhoObs As Double = correlationCoefficient(xRanks, yRanks)

            Dim total As Long = 0L
            Dim extremeTwoSided As Long = 0L
            Dim extremeGreater As Long = 0L
            Dim extremeLess As Long = 0L
            Dim hasTies = Me.HasTies(yRanks)

            If n <= 10 Then

                Dim expectedTotal = If(hasTies, ExpectedTotalPermutations(yRanks), Factorial(n))
                Dim s As Long = 0
                Dim iUpdate As Long
                iUpdate = expectedTotal / 100L
                If iUpdate = 0 Then iUpdate = 1

                ' Optional deduplication
                Dim seen As HashSet(Of String) = If(hasTies, New HashSet(Of String)(), Nothing)
                Dim c(n - 1) As Integer
                Dim yPerm = yRanks.ToArray()

                ' Evaluate initial permutation
                Dim key = If(hasTies, GetPermutationKey(yPerm), Nothing)
                If Not hasTies OrElse Not seen.Contains(key) Then
                    If hasTies Then seen.Add(key)
                    Dim r = correlationCoefficient(xRanks, yPerm)
                    total += 1
                    If Math.Abs(r) >= Math.Abs(rhoObs) Then extremeTwoSided += 1
                    If r >= rhoObs Then extremeGreater += 1
                    If r <= rhoObs Then extremeLess += 1
                End If

                Dim i = 0
                While i < n
                    If c(i) < i Then
                        Dim j = If(i Mod 2 = 0, 0, c(i))
                        ' Swap
                        Dim temp = yPerm(i)
                        yPerm(i) = yPerm(j)
                        yPerm(j) = temp

                        key = If(hasTies, GetPermutationKey(yPerm), Nothing)
                        If Not hasTies OrElse Not seen.Contains(key) Then
                            If hasTies Then seen.Add(key)
                            Dim r = correlationCoefficient(xRanks, yPerm)
                            total += 1
                            If Math.Abs(r) >= Math.Abs(rhoObs) Then extremeTwoSided += 1
                            If r >= rhoObs Then extremeGreater += 1
                            If r < rhoObs Then extremeLess += 1

                            If progressBar IsNot Nothing Then
                                If s Mod iUpdate = 0 Then
                                    progressBar.Invoke(Sub()
                                                           progressBar.Value = 100 * s / expectedTotal
                                                       End Sub)
                                    System.Windows.Forms.Application.DoEvents()
                                End If
                                s += 1
                            End If
                        End If
                        c(i) += 1
                        i = 0
                    Else
                        c(i) = 0
                        i += 1
                    End If
                End While
                If progressBar IsNot Nothing Then progressBar.Invoke(Sub()
                                                                         progressBar.Value = 100
                                                                     End Sub)

                Me.CorrelationResult.bExactAvailable = True
                Me.CorrelationResult.PvalueExact = extremeTwoSided / total
                Me.CorrelationResult.pValueExactUpperSide = extremeGreater / total
                Me.CorrelationResult.pValueExactLowerSide = extremeLess / total
            ElseIf Not hasTies And n < 30 Then
                Me.CorrelationResult.bExactAvailable = Me.SpearmanTailProbabilities(rhoObs)
            End If

            Me.CorrelationResult.TestStatistics1 = rhoObs
            Dim t = rhoObs * Math.Sqrt((n - 2) / (1.0 - rhoObs * rhoObs))
            'two-sided
            Me.CorrelationResult.Pvalue = distributions.T_2T(Math.Abs(t), n - 2)
            'upper side
            Me.CorrelationResult.PvalueUpperSide = distributions.T_RT(t, n - 2)
            'lower side
            Me.CorrelationResult.PvalueLowerSide = distributions.T_CDF(t, n - 2)

            Dim z As Double = distributions.ZCritTwoSided(alpha)

            Me.correlationCI = New ConfidenceIntervalResult
            Me.correlationCI.alpha = alpha
            Me.correlationCI.Estimate = rhoObs
            Dim SE As Double = Math.Sqrt(1.0 / (n - 3)) '(((1 + (AtanhRho ^ 2)) / 2) / (n - 3)) ^ 0.5 'BONETT and WRIGHT, PSYCHOMETRIKA-VOL. 65, NO. 1, 23-28, 2000
            Me.correlationCI.LowerLimit = Math.Tanh(Atanh(rhoObs) - SE * z)
            Me.correlationCI.UpperLimit = Math.Tanh(Atanh(rhoObs) + SE * z)

            Return Me.CorrelationResult
        End Function

        '--------------
        ' Helper functions
        '--------------
        ''' <summary>
        ''' Computes approximate tail probabilities for Spearman's ρ using the
        ''' Edgeworth-series expansion (Algorithm AS 89, Best and Roberts, 1975).
        ''' 
        ''' Valid for:
        ''' <code>
        ''' 4 ≤ n ≤ 50
        ''' </code>
        ''' and only when no ties are present.
        ''' 
        ''' External dependency:
        ''' <c>PNorm</c> — normal CDF.
        ''' </summary>
        ''' <param name="rho">Observed Spearman correlation.</param>
        ''' <returns><c>True</c> if approximation was applied; otherwise <c>False</c>.</returns>
        Private Function SpearmanTailProbabilities(rho As Double) As Boolean
            If n < 4 OrElse n > 50 Then Return False ' AS 89 is valid for 4 ≤ n ≤ 50

            Dim d As Double = (n ^ 3 - n) * (1 - rho) / 6
            Dim Js As Double = d
            If (Js <> 2 * (Js / 2)) Then Js += 1
            Js = Math.Round(Js, 12)

            If d > (n * (n * n - 1) / 3) Then
                Me.CorrelationResult.PvalueExact = 0.0
                Me.CorrelationResult.pValueExactUpperSide = 0.0
                Me.CorrelationResult.pValueExactLowerSide = 1.0
                Return True
            End If

            ' Coefficients from AS 89 (Best & Roberts, 1975)
            Dim b As Double = 1.0 / CDbl(n)
            Dim x As Double = (6 * (Js - 1) * b / (1 / (b * b) - 1) - 1) * Math.Sqrt(1 / b - 1)
            Dim Y As Double = x * x
            Dim T1 As Double = 0.2531 + 0.1745 * b
            Dim T2 As Double = 0.1033 + 0.3932 * b
            Dim t3 As Double = 0.0131 - 0.00046 * Y
            Dim t4 As Double = 0.0072 - 0.0831 * b + Y * b * t3
            Dim u As Double = x * b * (0.2274 + b * T1 + Y * (-0.0758 + b * T2 - Y * b * (0.0879 + 0.0151 * b - Y * t4)))
            Dim PRHO As Double = u / Math.Exp(Y / 2.0) + (1.0 - distributions.PNorm(x)) 'Fit probability

            If PRHO > 1.0 Then
                Me.CorrelationResult.PvalueExact = 0.5
                Me.CorrelationResult.pValueExactUpperSide = 0.5
                Me.CorrelationResult.pValueExactLowerSide = 1.0
            ElseIf PRHO < 0.0 Then
                Me.CorrelationResult.PvalueExact = 0.0
                Me.CorrelationResult.pValueExactUpperSide = 0.0
                Me.CorrelationResult.pValueExactLowerSide = 1.0
            ElseIf PRHO > 0.5 And PRHO < 1.0 Then 'return "smaller side" p-value (program originaly compute upper tail probability)
                Me.CorrelationResult.PvalueExact = 2.0 * (1.0 - PRHO)
                Me.CorrelationResult.pValueExactUpperSide = 1.0 - PRHO
                Me.CorrelationResult.pValueExactLowerSide = PRHO
            Else
                Me.CorrelationResult.PvalueExact = 2.0 * PRHO
                Me.CorrelationResult.pValueExactUpperSide = PRHO
                Me.CorrelationResult.pValueExactLowerSide = 1.0 - PRHO
            End If

            Return True
        End Function

        ''' <summary>
        ''' Computes n! for small n. Used in exact permutation enumeration.
        ''' </summary>
        Private Protected Function Factorial(n As Integer) As Long
            Dim result As Long = 1
            For i = 2 To n
                result *= i
            Next
            Return result
        End Function

        ''' <summary>
        ''' Computes the number of unique permutations when ties are present:
        ''' <code>
        ''' n! / Π (freqᵢ!)
        ''' </code>
        ''' </summary>
        Private Protected Function ExpectedTotalPermutations(data As Double()) As Long
            Dim n = data.Length
            Dim freq = data.GroupBy(Function(v) v).Select(Function(g) g.Count()).ToArray()
            Dim denominator As Long = 1
            For Each count In freq
                denominator *= Factorial(count)
            Next
            Return Factorial(n) \ denominator
        End Function

        ''' <summary>
        ''' Returns True if the data contain tied ranks.
        ''' </summary>
        Private Function HasTies(data As Double()) As Boolean
            Return data.GroupBy(Function(v) v).Any(Function(g) g.Count() > 1)
        End Function

        ''' <summary>
        ''' Generates a canonical string key for a permutation of ranks.
        ''' Used to avoid duplicate permutations when ties exist.
        ''' </summary>
        Private Protected Function GetPermutationKey(arr As Double()) As String
            Dim sb As New StringBuilder()
            For Each v In arr
                sb.AppendFormat("{0:G17};", v)
            Next
            Return sb.ToString()
        End Function

    End Class


    ''' <summary>
    ''' Implements Kendall's rank correlation coefficient τ<sub>b</sub>, including:
    ''' <list type="bullet">
    '''   <item><description>Computation of Kendall's τ<sub>b</sub> with tie adjustment</description></item>
    '''   <item><description>Exact permutation p-values for n ≤ 10</description></item>
    '''   <item><description>Edgeworth-series approximation for 4 ≤ n ≤ 50 (no ties)</description></item>
    '''   <item><description>Normal approximation for general n</description></item>
    '''   <item><description>Approximate confidence interval at level <c>1 - alpha</c></description></item>
    ''' </list>
    ''' 
    ''' Kendall's τ<sub>b</sub> measures the strength of monotonic association between two variables.
    ''' It is defined as:
    ''' <code>
    ''' τ = (C − D) / √((C + Tₓ)(C + Tᵧ))
    ''' </code>
    ''' where:
    ''' <list type="bullet">
    '''   <item><description>C = number of concordant pairs</description></item>
    '''   <item><description>D = number of discordant pairs</description></item>
    '''   <item><description>Tₓ = number of ties in X</description></item>
    '''   <item><description>Tᵧ = number of ties in Y</description></item>
    ''' </list>
    ''' 
    ''' Exact p-values are computed by permutation enumeration.
    ''' For larger samples, a normal approximation is used:
    ''' <code>
    ''' Z = τ / √((4n + 10) / (9n(n − 1)))
    ''' </code>
    ''' 
    ''' The approximate confidence interval is computed as:
    ''' <code>
    ''' CI = τ ± z_(1 − alpha/2) × SE(τ)
    ''' </code>
    ''' and truncated to the valid correlation range [-1, 1].
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>ComputeAvgRanks</c> — rank computation for inherited utilities</description></item>
    '''   <item><description><c>PNorm</c> — normal CDF</description></item>
    '''   <item><description><c>ExpectedTotalPermutations</c>, <c>Factorial</c> — permutation enumeration</description></item>
    '''   <item><description><c>GetPermutationKey</c> — tie-aware permutation deduplication</description></item>
    '''   <item><description><c>ZCritTwoSided</c> — two-sided normal critical value</description></item>
    '''   <item><description><c>HorizontalStackArrays</c>, <c>ResultTable</c>, <c>TestResult</c>, <c>ConfidenceIntervalResult</c></description></item>
    ''' </list>
    ''' </summary>
    Class KendallsTau
        Inherits SpearmanRho

        ''' <summary>
        ''' Standard error of Kendall’s τ used for confidence interval computation.
        ''' Computed using the variance estimator from Hollander and Wolfe (1999).
        ''' </summary>
        Private pSE As Double

        ''' <summary>
        ''' Initializes a new Kendall’s τ<sub>b</sub> test instance.
        ''' </summary>
        ''' <param name="x">Vector X.</param>
        ''' <param name="y">Vector Y.</param>
        ''' <param name="xname">Name of X.</param>
        ''' <param name="yname">Name of Y.</param>
        Sub New(x() As Double, y() As Double, xname As String, yname As String)
            MyBase.New(x, y, xname, yname)
        End Sub

        ''' <summary>
        ''' Produces formatted result tables summarizing:
        ''' <list type="bullet">
        '''   <item><description>Kendall's τ<sub>b</sub></description></item>
        '''   <item><description>Approximate confidence interval at level <c>1 - alpha</c></description></item>
        '''   <item><description>Approximate p-values</description></item>
        '''   <item><description>Exact p-values (if available)</description></item>
        ''' </list>
        ''' 
        ''' External dependency:
        ''' <c>HorizontalStackArrays</c> for table formatting.
        ''' </summary>
        ''' <returns>A list of <see cref="ResultTable"/> objects.</returns>
        Public Shadows Function wrapResults() As List(Of ResultTable)
            Dim out = New List(Of ResultTable), t = New ResultTable, pexactOut(,) As Object

            If Me.CorrelationResult.bExactAvailable Then
                pexactOut = {{"Exact Two sided p-value", Me.CorrelationResult.PvalueExact},
                      {"Exact Low-side p-value", Me.CorrelationResult.pValueExactLowerSide},
                      {"Exact Upper-side p-value", Me.CorrelationResult.pValueExactUpperSide}
                     }
            Else
                pexactOut = {{"Exact Two sided p-value", "NE"}}
            End If

            Dim o = {{"Number of valid data pairs", Me.n},
                 {"Tau-b", Me.CorrelationResult.TestStatistics1},
                 {"Approximate " & Me.correlationCI.CIlabel, Me.correlationCI.strConfidenceInterval(CIformat.LL_to_UL)},
                 {"Two-sided p-value (approx.)", Me.CorrelationResult.Pvalue},
                 {"Low-side p-value (approx.)", Me.CorrelationResult.PvalueLowerSide},
                 {"Upper-sid p-value (approx.)", Me.CorrelationResult.PvalueUpperSide}
                }

            t.SetBody(HorizontalStackArrays(o, pexactOut))
            t.AddHeaderTopRow({"Kendall's tau-b correlation coefficient", ""})
            out.Add(t)
            Return out
        End Function

        ''' <summary>
        ''' Computes Kendall’s τ<sub>b</sub> using the algorithm from
        ''' Numerical Recipes in Fortran 77, Chapter 14.
        ''' 
        ''' For each pair (i, j), determines whether the pair is:
        ''' <list type="bullet">
        '''   <item><description>Concordant (C)</description></item>
        '''   <item><description>Discordant (D)</description></item>
        '''   <item><description>Tied in X</description></item>
        '''   <item><description>Tied in Y</description></item>
        ''' </list>
        ''' 
        ''' τ<sub>b</sub> is computed as:
        ''' <code>
        ''' τ = S / √(n₁ n₂)
        ''' </code>
        ''' where:
        ''' <list type="bullet">
        '''   <item><description>S = C − D</description></item>
        '''   <item><description>n₁ = total non-tied comparisons in X</description></item>
        '''   <item><description>n₂ = total non-tied comparisons in Y</description></item>
        ''' </list>
        ''' 
        ''' When <paramref name="bComputeSE"/> is True, computes the standard error
        ''' using the variance estimator from Hollander and Wolfe (1999).
        ''' </summary>
        ''' <param name="x">Vector X.</param>
        ''' <param name="y">Vector Y.</param>
        ''' <param name="bComputeSE">If True, computes SE(τ) for CI.</param>
        ''' <returns>Kendall’s τ<sub>b</sub>.</returns>
        Public Shadows Function correlationCoefficient(x As Double(), y As Double(), Optional bComputeSE As Boolean = False) As Double
            'computes tau-b
            Dim Ci() As Double, Sc() As Double, sd() As Double, k As Integer
            Dim n1 As Integer, n2 As Integer, S As Integer, Sigma As Double, temp As Double

            Dim n = x.Length
            ReDim Sc(n - 1), sd(n - 1)

            'from NUMERICAL RECIPES IN FORTRAN 77: THE ART OF SCIENTIFIC COMPUTING chapter 14.
            For j = 0 To n - 2                      'Loop over 1st member of pair
                For k = j + 1 To n - 1                'and 2nd member
                    Dim a1 As Double = x(j) - x(k)
                    Dim a2 As Double = y(j) - y(k)
                    Dim AA As Double = a1 * a2
                    If AA <> 0 Then                 'Neither array has a tie
                        n1 += 1
                        n2 += 1
                        If AA > 0 Then
                            S += 1
                            Sc(j) += 1
                            Sc(k) += 1
                        Else
                            S = S - 1
                            sd(j) += 1
                            sd(k) += 1
                        End If
                    Else                            'One or both arrays have ties
                        If a1 <> 0 Then n1 += 1 'An "extra x" event
                        If a2 <> 0 Then n2 += 1 'An "extra pY" event
                    End If
                Next
            Next

            Dim tau As Double = S / Math.Sqrt(n1 * n2)

            'Confidence interval caluclation
            If bComputeSE Then
                Ci = M_SUB(Sc, sd)
                Dim Cbar As Double = Ci.Sum() / CDbl(n)

                For i = 0 To n - 1
                    Sigma += (Ci(i) - Cbar) ^ 2
                Next

                temp = (2.0 * (n - 2)) / (n * ((n - 1) ^ 2))
                Sigma = temp * Sigma + 1 - tau ^ 2
                Sigma = Math.Sqrt(Sigma * (2.0 / (n * (n - 1))))
                Me.pSE = Sigma
            End If

            Return tau
        End Function

        ''' <summary>
        ''' Computes Kendall's τ<sub>b</sub> correlation test, including:
        ''' <list type="bullet">
        '''   <item><description>Computation of τ<sub>b</sub> and SE(τ)</description></item>
        '''   <item><description>Exact permutation p-values for n ≤ 10</description></item>
        '''   <item><description>Edgeworth-series approximation for 4 ≤ n ≤ 50 (no ties)</description></item>
        '''   <item><description>Normal approximation for general n</description></item>
        '''   <item><description>Approximate confidence interval at level <c>1 - alpha</c></description></item>
        ''' </list>
        ''' 
        ''' Normal approximation:
        ''' <code>
        ''' Z = τ / √((4n + 10) / (9n(n − 1)))
        ''' </code>
        ''' 
        ''' Confidence interval:
        ''' <code>
        ''' CI = τ ± z_(1 − alpha/2) × SE(τ)
        ''' </code>
        ''' 
        ''' The interval is truncated to the valid correlation range [-1, 1].
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>PNorm</c> — normal CDF</description></item>
        '''   <item><description><c>ExpectedTotalPermutations</c>, <c>Factorial</c></description></item>
        '''   <item><description><c>GetPermutationKey</c> — tie-aware deduplication</description></item>
        '''   <item><description><c>ZCritTwoSided</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="progressBar">Optional progress bar for permutation enumeration.</param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the approximate confidence interval.
        ''' For example, <c>alpha = 0.05</c> gives a 95% confidence interval.
        ''' </param>
        ''' <returns>A <see cref="TestResult"/> containing τ<sub>b</sub> and p-values.</returns>
        Public Shadows Function compute(Optional progressBar As System.Windows.Forms.ProgressBar = Nothing, Optional alpha As Double = 0.05) As TestResult
            CorrelationResult = New TestResult
            Me.n = X.Length
            Dim tauObs As Double = correlationCoefficient(X, Y, True)
            Dim total As Long = 0L
            Dim extremeTwoSided As Long = 0L
            Dim extremeGreater As Long = 0L
            Dim extremeLess As Long = 0L
            Dim hasTies = Me.HasTies(Y)

            If n <= 10 Then

                Dim expectedTotal = If(hasTies, ExpectedTotalPermutations(Y), Factorial(n))
                Dim s As Long = 0L
                Dim iUpdate As Long
                iUpdate = expectedTotal / 100L
                If iUpdate = 0 Then iUpdate = 1

                Dim c(n - 1) As Integer
                Dim yPerm = Y.ToArray()

                '' Optional deduplication
                Dim seen As HashSet(Of String) = If(hasTies, New HashSet(Of String)(), Nothing)

                ' evaluate initial permutation
                Dim key = If(hasTies, GetPermutationKey(yPerm), Nothing)
                If Not hasTies OrElse Not seen.Contains(key) Then
                    If hasTies Then seen.Add(key)
                    Dim tau = correlationCoefficient(X, yPerm)
                    total += 1
                    If Math.Abs(tau) >= Math.Abs(tauObs) Then extremeTwoSided += 1
                    If tau >= tauObs Then extremeGreater += 1
                    If tau <= tauObs Then extremeLess += 1
                End If

                Dim i = 0
                While i < n
                    If c(i) < i Then
                        Dim j = If(i Mod 2 = 0, 0, c(i))
                        ' swap
                        Dim temp = yPerm(i)
                        yPerm(i) = yPerm(j)
                        yPerm(j) = temp

                        key = If(hasTies, GetPermutationKey(yPerm), Nothing)
                        If Not hasTies OrElse Not seen.Contains(key) Then
                            If hasTies Then seen.Add(key)
                            Dim tau = correlationCoefficient(X, yPerm)
                            total += 1
                            If Math.Abs(tau) >= Math.Abs(tauObs) Then extremeTwoSided += 1
                            If tau >= tauObs Then extremeGreater += 1
                            If tau < tauObs Then extremeLess += 1

                            If progressBar IsNot Nothing Then
                                If s Mod iUpdate = 0 Then
                                    progressBar.Invoke(Sub()
                                                           progressBar.Value = 100 * s / expectedTotal
                                                       End Sub)
                                    System.Windows.Forms.Application.DoEvents()
                                End If
                                s += 1
                            End If
                        End If
                        c(i) += 1
                        i = 0
                    Else
                        c(i) = 0
                        i += 1
                    End If
                End While
                If progressBar IsNot Nothing Then progressBar.Invoke(Sub()
                                                                         progressBar.Value = 100
                                                                     End Sub)

                Me.CorrelationResult.bExactAvailable = True
                Me.CorrelationResult.PvalueExact = extremeTwoSided / total
                Me.CorrelationResult.pValueExactUpperSide = extremeGreater / total
                Me.CorrelationResult.pValueExactLowerSide = extremeLess / total
            ElseIf Not hasTies And n < 30 Then
                Me.CorrelationResult.bExactAvailable = Me.KendallTailProbabilities(tauObs)
            End If

            Me.CorrelationResult.TestStatistics1 = tauObs
            Dim z = tauObs / Math.Sqrt((4 * n + 10) / (9 * n * (n - 1)))
            'two-sided
            Me.CorrelationResult.Pvalue = 2.0 * (1.0 - distributions.PNorm(Math.Abs(z)))
            'upper side
            Me.CorrelationResult.PvalueUpperSide = 1.0 - distributions.PNorm(z)
            'lower side
            Me.CorrelationResult.PvalueLowerSide = distributions.PNorm(z)


            'Kendall TAU Confidence Interval as described in
            'Hollander M, Wolfe DA. Non-parametric Statistical Methods (2nd edition). New York: Wiley 1999. page 384
            Me.correlationCI = New ConfidenceIntervalResult
            Me.correlationCI.alpha = alpha
            Me.correlationCI.Estimate = tauObs
            Dim zz As Double = distributions.ZCritTwoSided(alpha)
            Me.correlationCI.LowerLimit = tauObs - Me.pSE * zz
            If Me.correlationCI.LowerLimit < -1 Then Me.correlationCI.LowerLimit = -1
            Me.correlationCI.UpperLimit = tauObs + Me.pSE * zz
            If Me.correlationCI.UpperLimit > 1 Then Me.correlationCI.UpperLimit = 1

            Return Me.CorrelationResult
        End Function

        ''' <summary>
        ''' Computes approximate tail probabilities for Kendall’s τ using the
        ''' Edgeworth-series expansion (Algorithm AS 89, Best and Roberts, 1975).
        ''' 
        ''' Valid for:
        ''' <code>
        ''' 4 ≤ n ≤ 50
        ''' </code>
        ''' and only when no ties are present.
        ''' 
        ''' External dependency:
        ''' <c>PNorm</c> — normal CDF.
        ''' </summary>
        ''' <param name="tau">Observed Kendall τ.</param>
        ''' <returns><c>True</c> if approximation was applied; otherwise <c>False</c>.</returns>
        Private Function KendallTailProbabilities(tau As Double) As Boolean
            Dim H(15) As Double

            'computes p-value using Edgeworth series expansion based on Algorithm AS 89 Appl. Statist. (1975) Vol.24, No. 3, P377.
            If n < 4 OrElse n > 50 Then Return False ' AS 89 is valid for 4 ≤ n ≤ 50

            'calculate the s statistic from tau (s = n over two multiplied by tau)
            Dim S As Double = tau * n * (n - 1) / 2

            'CALCUIATION OF TCHEBYCHEFF-HERMITE POLYNOMIALS
            Dim X As Double = (S - 1.0) / Math.Sqrt((((6 + n * (5 - n * (3 + 2 * n)))) / (-18)))
            H(1) = X
            H(2) = X * X - 1
            For i = 3 To 15
                H(i) = X * H(i - 1) - CDbl(i - 1) * H(i - 2)
            Next

            'PROBABILITIES CALCULATED BY MODIFIED EDGEWORTH SERIES FOR N GREATER THAN 8
            Dim r As Double = 1.0 / CDbl(n)
            Dim c1 As Double = (-0.09 + r * (0.045 + r * (-0.5325 + r * 0.506)))
            Dim c2 As Double = (0.036735 + r * (-0.036735 + r * 0.3214))
            Dim c3 As Double = (0.00405 + r * (-0.023336 + r * 0.07787))
            Dim c4 As Double = (-0.0033061 - r * 0.0065166)
            Dim c5 As Double = (-0.0001215 + r * 0.0025927)
            Dim c6 As Double = (H(13) * 0.00014878 + H(15) * 0.0000027338)

            Dim Sc As Double = r * (H(3) * c1 + r * (H(5) * c2 + H(7) * c3 + r * (H(9) * c4 + H(11) * c5 + r * c6)))
            Dim P_value As Double = Sc * 0.398942 * Math.Exp(-0.3 * X * X) + (1.0 - distributions.PNorm(X))

            If P_value > 1.0 Then
                Me.CorrelationResult.PvalueExact = 0.5
                Me.CorrelationResult.pValueExactUpperSide = 0.5
                Me.CorrelationResult.pValueExactLowerSide = 1.0
            ElseIf P_value < 0.0 Then
                Me.CorrelationResult.PvalueExact = 0.0
                Me.CorrelationResult.pValueExactUpperSide = 0.0
                Me.CorrelationResult.pValueExactLowerSide = 1.0
            ElseIf P_value > 0.5 And P_value < 1.0 Then 'return "smaller side" p-value (program originaly compute upper tail probability)
                Me.CorrelationResult.PvalueExact = 2.0 * (1.0 - P_value)
                Me.CorrelationResult.pValueExactUpperSide = 1.0 - P_value
                Me.CorrelationResult.pValueExactLowerSide = P_value
            Else
                Me.CorrelationResult.PvalueExact = 2.0 * P_value
                Me.CorrelationResult.pValueExactUpperSide = P_value
                Me.CorrelationResult.pValueExactLowerSide = 1.0 - P_value
            End If

            Return True
        End Function

        ''' <summary>
        ''' Returns True if the data contain tied values.
        ''' Used to determine whether exact permutation enumeration
        ''' requires tie‑aware deduplication.
        ''' </summary>
        ''' <param name="data">Input vector.</param>
        ''' <returns>True if ties exist; otherwise False.</returns>
        Private Function HasTies(data As Double()) As Boolean
            Return data.Distinct().Count() < data.Length
        End Function
    End Class


    ''' <summary>
    ''' Implements the Kruskal–Wallis H test, a nonparametric alternative to
    ''' one‑way ANOVA for comparing k independent groups.
    ''' 
    ''' The test evaluates whether the distributions of the groups differ by
    ''' comparing their mean ranks. It is based on:
    ''' <code>
    ''' H = (12 / (N(N+1))) Σ (Rᵢ² / nᵢ) − 3(N+1)
    ''' </code>
    ''' where:
    ''' <list type="bullet">
    '''   <item><description>Rᵢ = sum of ranks in group i</description></item>
    '''   <item><description>nᵢ = sample size of group i</description></item>
    '''   <item><description>N = total sample size</description></item>
    ''' </list>
    ''' 
    ''' A tie‑corrected statistic is also computed:
    ''' <code>
    ''' H<sub>cor</sub> = H / (1 − T / (N³ − N))
    ''' </code>
    ''' where T = Σ(tⱼ³ − tⱼ) over all tied groups.
    ''' 
    ''' Post‑hoc pairwise comparisons are computed using **Dunn’s test**, with
    ''' Bonferroni correction:
    ''' <code>
    ''' Z = |mean rank diff| / √( a (1/nᵢ + 1/nⱼ) )
    ''' </code>
    ''' where:
    ''' <code>
    ''' a = N(N+1)/12 − T/(12(N−1))
    ''' </code>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>ComputeAvgRanks</c> — rank computation with ties</description></item>
    '''   <item><description><c>TiesCorrection</c> — tie correction factor</description></item>
    '''   <item><description><c>PNorm</c> — normal CDF</description></item>
    '''   <item><description><c>ChiSquareCDF</c> — chi‑square CDF</description></item>
    '''   <item><description><c>ResultTable</c>, <c>TestResult</c></description></item>
    ''' </list>
    ''' </summary>
    Class KruskallWalis
        ''' <summary>Input data grouped as data(g)(i).</summary>
        Private data()() As Double

        ''' <summary>Names of the groups.</summary>
        Private varNames() As String

        ''' <summary>Stores H, H<sub>cor</sub>, and p‑values.</summary>
        Private KWResult As TestResult

        ''' <summary>Number of groups.</summary>
        Private NoGroups As Integer

        ''' <summary>Total number of observations across all groups.</summary>
        Private N As Integer

        ''' <summary>
        ''' Dunn’s multiple comparison results:
        ''' columns = {contrast, mean rank diff, Z, p-value}.
        ''' </summary>
        Private pMCP(,) As Object = Nothing

        ''' <summary>Rank sums Rᵢ for each group.</summary>
        Private rankSums() As Double

        ''' <summary>
        ''' Initializes a new Kruskal–Wallis test instance.
        ''' </summary>
        ''' <param name="x">Array of groups, each containing numeric observations.</param>
        ''' <param name="strNames">Names of the groups.</param>
        Sub New(x()() As Double, strNames() As String)
            Me.data = x
            Me.varNames = strNames
            Me.NoGroups = varNames.Length
        End Sub

        ''' <summary>
        ''' Produces formatted result tables summarizing:
        ''' <list type="bullet">
        '''   <item><description>Total sample size</description></item>
        '''   <item><description>Number of groups</description></item>
        '''   <item><description>H statistic (uncorrected)</description></item>
        '''   <item><description>H statistic (tie‑corrected)</description></item>
        '''   <item><description>Corresponding chi‑square p‑values</description></item>
        '''   <item><description>Dunn’s post‑hoc comparisons (if computed)</description></item>
        ''' </list>
        ''' 
        ''' External dependency:
        ''' <c>ResultTable</c> for formatting.
        ''' </summary>
        ''' <returns>A list of <see cref="ResultTable"/> objects.</returns>
        Public Function wrapResults() As List(Of ResultTable)
            Dim out = New List(Of ResultTable)
            Dim t = New ResultTable

            t.SetBody({{"n", Me.N, ""},
                  {"Number of groups", Me.NoGroups, ""},
                  {"Test statistics H", Me.KWResult.TestStatistics1, ""},
                  {"Two sided P-value", Me.KWResult.Pvalue, "ties un-corrected"},
                  {"Test statistics Hcor", Me.KWResult.TestStatistics2, ""},
                  {"Two sided p-value", Me.KWResult.Pvalue2, "ties corrected"}
                 })
            t.AddHeaderTopRow({"Kruskal-Wallis Test", "", ""})
            out.Add(t)

            If Me.pMCP IsNot Nothing Then
                t = New ResultTable
                t.SetBody(Me.pMCP)
                t.AddHeaderTopRow({"Dunn's multiple comparison test", "Mean rank diff.", "Z", "Two sided P-value"})
                out.Add(t)
            End If

            Return out
        End Function

        ''' <summary>
        ''' Computes the Kruskal–Wallis H statistic and tie‑corrected H<sub>cor</sub>.
        ''' 
        ''' Steps:
        ''' <list type="number">
        '''   <item><description>Combine all values and compute average ranks.</description></item>
        '''   <item><description>Compute rank sums Rᵢ for each group.</description></item>
        '''   <item><description>Compute H using the standard formula.</description></item>
        '''   <item><description>Apply tie correction using T = Σ(tⱼ³ − tⱼ).</description></item>
        '''   <item><description>Compute chi‑square p-values with df = k − 1.</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>ComputeAvgRanks</c></description></item>
        '''   <item><description><c>TiesCorrection</c></description></item>
        '''   <item><description><c>ChiSquareCDF</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>A <see cref="TestResult"/> containing H, H<sub>cor</sub>, and p-values.</returns>
        Public Function compute() As TestResult
            KWResult = New TestResult
            Dim H As Double
            Dim allValues As New List(Of Double)
            Dim groupMap As New List(Of Integer)

            For g = 0 To Me.NoGroups - 1
                'For Each v In Me.data(g)
                For i = 0 To Me.data(g).Length - 1
                    allValues.Add(Me.data(g)(i))
                    groupMap.Add(g)
                Next
            Next
            Me.N = allValues.Count

            ' Compute ranks with tie handling
            Dim ranks = ComputeAvgRanks(allValues.ToArray())

            ' Sum ranks per group
            ReDim Me.rankSums(NoGroups - 1)
            For i = 0 To N - 1
                Dim g = groupMap(i)
                rankSums(g) += ranks(i)
            Next

            For g = 0 To NoGroups - 1
                Dim n_g = Me.data(g).Length
                H += (rankSums(g) ^ 2) / n_g
            Next
            H = (12.0 / (N * (N + 1))) * H - (3.0 * (N + 1))

            'ties correction
            Dim sumTIES3 As Double = TiesCorrection(allValues.ToArray())
            Dim Hcor As Double = H / (1 - (sumTIES3 / (N ^ 3 - N)))

            'output
            KWResult.TestStatistics1 = H
            KWResult.TestStatistics2 = Hcor
            KWResult.Pvalue = 1.0 - distributions.ChiSquareCDF(H, NoGroups - 1)
            KWResult.Pvalue2 = 1.0 - distributions.ChiSquareCDF(Hcor, NoGroups - 1)

            Return KWResult
        End Function

        ''' <summary>
        ''' Computes Dunn’s post‑hoc pairwise comparisons following a significant
        ''' Kruskal–Wallis test.
        ''' 
        ''' For each pair of groups (i, j), computes:
        ''' <list type="bullet">
        '''   <item><description>Mean rank difference</description></item>
        '''   <item><description>Z statistic</description></item>
        '''   <item><description>Bonferroni‑adjusted p-value</description></item>
        ''' </list>
        ''' 
        ''' Z statistic:
        ''' <code>
        ''' Z = |mean rank diff| / √( a (1/nᵢ + 1/nⱼ) )
        ''' </code>
        ''' where:
        ''' <code>
        ''' a = N(N+1)/12 − T/(12(N−1))
        ''' </code>
        ''' 
        ''' External dependency:
        ''' <c>PNorm</c> — normal CDF.
        ''' </summary>
        '''  <param name="alpha">
        ''' Reserved for API consistency with other multiple-comparison methods.
        ''' This method currently reports Bonferroni-adjusted p-values only and does
        ''' not compute confidence intervals, so <paramref name="alpha"/> is not yet used.
        ''' </param>
        ''' <returns>A matrix of contrasts and test results.</returns>
        Public Function MCP(Optional alpha As Double = 0.05) As Object(,)

            parametric.ValidateAlpha(alpha)
            Dim allValues As New List(Of Double)
            ' Compute ranks with tie handling
            For g = 0 To Me.NoGroups - 1
                For Each v In Me.data(g)
                    allValues.Add(v)
                Next
            Next

            'ties correction
            Dim sumTIES3 As Double = TiesCorrection(allValues.ToArray())
            Dim a As Double = (N * (N + 1) / 12) - (sumTIES3 / (12 * (N - 1)))
            ReDim Me.pMCP((NoGroups * (NoGroups - 1)) / 2 - 1, 3)
            Dim ii As Integer = 0
            For i = 0 To NoGroups - 1
                For j = i + 1 To NoGroups - 1
                    pMCP(ii, 0) = varNames(i) & " vs " & varNames(j)  'contrast name
                    pMCP(ii, 1) = (Me.rankSums(i) / data(i).Length) - (Me.rankSums(j) / data(j).Length) 'mean rank difference
                    pMCP(ii, 2) = Math.Abs(pMCP(ii, 1)) / Math.Sqrt(a * (1 / data(i).Length + 1 / data(j).Length))
                    pMCP(ii, 3) = 2.0 * (1.0 - distributions.PNorm(CDbl(pMCP(ii, 2)))) * (NoGroups * (NoGroups - 1) / 2)
                    If pMCP(ii, 3) >= 1 Then pMCP(ii, 3) = 1
                    ii += 1
                Next
            Next

            Return pMCP
        End Function

    End Class


    ''' <summary>
    ''' Implements the Friedman test for randomized block designs, a nonparametric
    ''' alternative to one‑way repeated‑measures ANOVA.
    ''' 
    ''' The test evaluates whether k treatments differ in central tendency across
    ''' b blocks by ranking treatments within each block and comparing mean ranks.
    ''' 
    ''' The Friedman chi‑square statistic is:
    ''' <code>
    ''' T₁ = (12 / (b k (k + 1))) Σ (Rⱼ²) − 3 b (k + 1)
    ''' </code>
    ''' where:
    ''' <list type="bullet">
    '''   <item><description>Rⱼ = sum of ranks for treatment j</description></item>
    '''   <item><description>b = number of blocks</description></item>
    '''   <item><description>k = number of treatments</description></item>
    ''' </list>
    ''' 
    ''' A second statistic T₂ provides an F‑approximation:
    ''' <code>
    ''' T₂ = ((b − 1) T₁) / (b (k − 1) − T₁)
    ''' </code>
    ''' 
    ''' Post‑hoc multiple comparisons include:
    ''' <list type="bullet">
    '''   <item><description>Conover’s test (t‑approximation)</description></item>
    '''   <item><description>Dunn’s test (normal approximation, SPSS style)</description></item>
    ''' </list>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>ComputeAvgRanks</c> — rank computation with ties</description></item>
    '''   <item><description><c>ChiSquareCDF</c> — chi‑square CDF</description></item>
    '''   <item><description><c>F_RT</c> — F‑distribution right‑tail probability</description></item>
    '''   <item><description><c>PNorm</c> — normal CDF</description></item>
    '''   <item><description><c>T_2T</c> — two‑tailed t‑distribution probability</description></item>
    '''   <item><description><c>HorizontalStackArrays</c>, <c>ResultTable</c>, <c>TestResult</c></description></item>
    ''' </list>
    ''' </summary>
    Public Class Friedman
        ''' <summary>Data matrix: rows = blocks, columns = treatments.</summary>
        Private data(,) As Double

        ''' <summary>Names of the treatments (columns).</summary>
        Private varNames() As String

        ''' <summary>Stores Friedman statistics and p-values.</summary>
        Private FriedmanResult As TestResult

        ''' <summary>Number of treatments (columns).</summary>
        Private NoGroups As Integer

        ''' <summary>Number of blocks (rows).</summary>
        Private NoBlocks As Integer

        ''' <summary>Total number of observations (b × k).</summary>
        Private N As Integer

        ''' <summary>Multiple comparison results (Conover + Dunn).</summary>
        Private pMCP(,) As Object = Nothing

        ''' <summary>Mean ranks for each treatment.</summary>
        Private MeanRanks() As Double

        ''' <summary>Conover post‑hoc comparison table.</summary>
        Private Conover(,) As Object = Nothing

        ''' <summary>Dunn (SPSS‑style) post‑hoc comparison table.</summary>
        Private SPSS(,) As Object = Nothing

        ''' <summary>
        ''' Initializes a Friedman test instance.
        ''' </summary>
        ''' <param name="x">Data matrix: rows = blocks, columns = treatments.</param>
        ''' <param name="strNames">Names of the treatments.</param>
        Sub New(x(,) As Double, strNames() As String)
            Me.data = x
            Me.varNames = strNames
            Me.NoBlocks = data.GetLength(0)
            Me.NoGroups = data.GetLength(1)
        End Sub

        ''' <summary>
        ''' Produces formatted result tables summarizing:
        ''' <list type="bullet">
        '''   <item><description>Friedman chi‑square statistic T₁</description></item>
        '''   <item><description>F‑approximation statistic T₂</description></item>
        '''   <item><description>Corresponding p‑values</description></item>
        '''   <item><description>Mean ranks for each treatment</description></item>
        '''   <item><description>Conover and Dunn post‑hoc comparisons (if computed)</description></item>
        ''' </list>
        ''' 
        ''' External dependency:
        ''' <c>ResultTable</c> for formatting.
        ''' </summary>
        ''' <returns>A list of <see cref="ResultTable"/> objects.</returns>
        Public Function wrapResults() As List(Of ResultTable)
            Dim out = New List(Of ResultTable)
            Dim t = New ResultTable

            'Friedman test output -------------------------------
            Dim Fout(,) As Object = {{"Number of blocks", Me.NoBlocks},
                {"Number of groups", Me.NoGroups},
                {"Test statistics T1(Chi-square)", Me.FriedmanResult.TestStatistics1},
                {"Two sided P-value", Me.FriedmanResult.Pvalue},
                {"Test statistics T2(F)", Me.FriedmanResult.TestStatistics2},
                {"Two sided p-value", Me.FriedmanResult.Pvalue2}
                }

            t.SetBody(Fout)
            t.AddHeaderTopRow({"Friedman Test", ""})
            out.Add(t)

            'Mean Rank output -------------------------------
            t = New ResultTable
            Dim Mranks(NoGroups - 1, 1) As Object
            For i = 0 To NoGroups - 1
                Mranks(i, 0) = Me.varNames(i)
                Mranks(i, 1) = Me.MeanRanks(i)
            Next
            t.SetBody(Mranks)
            t.AddHeaderTopRow({"Group", "Mean Rank"})
            out.Add(t)

            'Multiple comparisons-------------------------------
            If Me.Conover IsNot Nothing Then
                t = New ResultTable
                t.SetBody(Me.Conover)
                t.AddHeaderTopRow({"Conover multiple comparison test", "Mean rank diff.", "T", "Two sided P-value"})
                out.Add(t)
            End If
            If Me.SPSS IsNot Nothing Then
                t = New ResultTable
                t.SetBody(Me.SPSS)
                t.AddHeaderTopRow({"Dunn's multiple comparison test", "Mean rank diff.", "Z", "Two sided P-value"})
                out.Add(t)
            End If

            Return out
        End Function

        ''' <summary>
        ''' Computes post‑hoc multiple comparisons following a significant
        ''' Friedman test, including:
        ''' <list type="bullet">
        '''   <item><description>Conover’s test (t‑approximation)</description></item>
        '''   <item><description>Dunn’s test (normal approximation, SPSS style)</description></item>
        ''' </list>
        ''' 
        ''' Conover statistic:
        ''' <code>
        ''' T = |R̄ᵢ − R̄ⱼ| / √(CompCrit)
        ''' </code>
        ''' 
        ''' Dunn statistic:
        ''' <code>
        ''' Z = |mean rank diff| / √( SE )
        ''' </code>
        ''' 
        ''' Bonferroni correction is applied to Dunn’s p-values.
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>ComputeAvgRanks</c></description></item>
        '''   <item><description><c>T_2T</c> — t‑distribution p-value</description></item>
        '''   <item><description><c>PNorm</c> — normal CDF</description></item>
        '''   <item><description><c>HorizontalStackArrays</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="alpha">
        ''' Reserved for API consistency with other multiple-comparison methods.
        ''' This method currently reports adjusted p-values only and does not compute
        ''' confidence intervals, so <paramref name="alpha"/> is not yet used.
        ''' </param>
        ''' <returns>A matrix of contrasts and test results.</returns>
        Public Function MCP(Optional alpha As Double = 0.05) As Object(,)
            parametric.ValidateAlpha(alpha)
            Dim MrankSUM As Double, SumRanksSQ As Double
            Dim Ncontrast As Integer = NoGroups * (NoGroups - 1) / 2
            ReDim Conover(Ncontrast - 1, 3), SPSS(Ncontrast - 1, 3)

            ' Step 1: Rank treatments within each block
            Dim ranks = New Double(NoBlocks - 1, NoGroups - 1) {}
            For i = 0 To NoBlocks - 1
                Dim block(NoGroups - 1) As Double
                For j = 0 To NoGroups - 1
                    block(j) = data(i, j)
                Next
                Dim ranked = ComputeAvgRanks(block)
                For j = 0 To NoGroups - 1
                    ranks(i, j) = ranked(j)
                    SumRanksSQ += ranked(j) ^ 2
                Next
            Next

            ' Step 2: Sum ranks per treatment
            Dim rankSums(NoGroups - 1) As Double
            For j = 0 To NoGroups - 1
                For i = 0 To NoBlocks - 1
                    rankSums(j) += ranks(i, j)
                Next
                MrankSUM += ((rankSums(j) ^ 2) / NoBlocks)
            Next

            Dim CompCrit As Double = Math.Sqrt((2 * (NoBlocks * SumRanksSQ - MrankSUM * NoBlocks)) / ((NoBlocks - 1) * (NoGroups - 1)))
            Dim SE As Double = NoGroups * (NoGroups + 1) / (6 * NoBlocks) 'SE for Dunn's MCP

            Dim ii As Integer = 0
            For i = 0 To NoGroups - 1
                For j = i + 1 To NoGroups - 1
                    Conover(ii, 0) = varNames(i) & " vs " & varNames(j)  'contrast name
                    Conover(ii, 1) = MeanRanks(i) * NoBlocks - MeanRanks(j) * NoBlocks
                    Conover(ii, 2) = Math.Abs(Conover(ii, 1)) / CompCrit
                    Conover(ii, 3) = distributions.T_2T(CDbl(Conover(ii, 2)), (NoBlocks - 1) * (NoGroups - 1))

                    'MCP according the SPSS
                    SPSS(ii, 0) = Conover(ii, 0)  'contrast name
                    SPSS(ii, 1) = Conover(ii, 1)
                    SPSS(ii, 2) = Math.Abs(Conover(ii, 1) / NoBlocks) / Math.Sqrt(SE)
                    SPSS(ii, 3) = 2.0 * (1.0 - distributions.PNorm(CDbl(SPSS(ii, 2))))
                    SPSS(ii, 3) = SPSS(ii, 3) * Ncontrast 'now it is adjusted for MC
                    If SPSS(ii, 3) > 1 Then SPSS(ii, 3) = 1

                    ii += 1
                Next
            Next

            Me.pMCP = HorizontalStackArrays({{"Conover multiple comparison test", "Mean rank diff.", "T", "Two sided P-value"}}, Conover)
            Me.pMCP = HorizontalStackArrays(Me.pMCP,
                                        {{"", "", "", ""},
                                         {"Dunn's multiple comparison test", "Mean rank diff.", "Z", "Two sided P-value"}})
            Me.pMCP = HorizontalStackArrays(Me.pMCP, SPSS)
            Return Me.pMCP
        End Function

        ''' <summary>
        ''' Computes the Friedman test statistics T₁ (chi‑square) and T₂ (F‑approximation).
        ''' 
        ''' Steps:
        ''' <list type="number">
        '''   <item><description>Rank treatments within each block using average ranks.</description></item>
        '''   <item><description>Compute rank sums Rⱼ and mean ranks.</description></item>
        '''   <item><description>Compute T₁ using the Friedman formula.</description></item>
        '''   <item><description>Compute T₂ using the Iman–Davenport F‑approximation.</description></item>
        '''   <item><description>Compute chi‑square and F‑distribution p‑values.</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>ComputeAvgRanks</c></description></item>
        '''   <item><description><c>ChiSquareCDF</c></description></item>
        '''   <item><description><c>F_RT</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>A <see cref="TestResult"/> containing T₁, T₂, and p-values.</returns>
        Public Function compute() As TestResult
            Me.FriedmanResult = New TestResult
            Dim MrankSUM As Double, SumRanksSQ As Double

            ' Step 1: Rank treatments within each block
            Dim ranks = New Double(NoBlocks - 1, NoGroups - 1) {}
            For i = 0 To NoBlocks - 1
                Dim block(NoGroups - 1) As Double
                For j = 0 To NoGroups - 1
                    block(j) = data(i, j)
                Next
                Dim ranked = ComputeAvgRanks(block)
                For j = 0 To NoGroups - 1
                    ranks(i, j) = ranked(j)
                    SumRanksSQ += ranked(j) ^ 2
                Next
            Next

            ' Step 2: Sum ranks per treatment
            Dim rankSums(NoGroups - 1) As Double
            ReDim Me.MeanRanks(NoGroups - 1)
            For j = 0 To NoGroups - 1
                For i = 0 To NoBlocks - 1
                    rankSums(j) += ranks(i, j)
                Next
                MrankSUM += ((rankSums(j) ^ 2) / NoBlocks)
                Me.MeanRanks(j) = rankSums(j) / NoBlocks
            Next

            'compute test statistics
            Dim TiesC As Double = (NoBlocks * NoGroups * (NoGroups + 1) ^ 2) / 4.0
            Dim T1 As Double = (NoBlocks * (NoGroups - 1) * (MrankSUM - TiesC)) / (SumRanksSQ - TiesC)
            Dim T2 As Double = If(NoBlocks = T1, 10 ^ 30, ((NoBlocks - 1) * T1) / (NoBlocks * (NoGroups - 1) - T1))

            Me.FriedmanResult.TestStatistics1 = T1
            Me.FriedmanResult.TestStatistics2 = T2
            Me.FriedmanResult.Pvalue = 1.0 - distributions.ChiSquareCDF(T1, (NoGroups - 1))
            Me.FriedmanResult.Pvalue2 = distributions.F_RT(T2, NoGroups - 1, (NoBlocks - 1) * (NoGroups - 1))

            Return Me.FriedmanResult

        End Function

    End Class



    ''' <summary>
    ''' Represents the results of the Theil–Sen estimator for robust linear regression.
    ''' 
    ''' The Theil–Sen method estimates the slope as the median of all pairwise slopes:
    ''' <code>
    ''' slope = median( (yⱼ − yᵢ) / (xⱼ − xᵢ) )
    ''' </code>
    ''' 
    ''' It is highly robust to outliers and valid under minimal assumptions.
    ''' Confidence limits are typically computed using Kendall’s τ‑based variance
    ''' or via rank‑based inversion of the Sen slope distribution.
    ''' </summary>
    Public Class TheilSenResults

        ''' <summary>
        ''' Number of non‑tied slope pairs used in the computation.
        ''' 
        ''' This equals the number of (i, j) pairs where xⱼ ≠ xᵢ.
        ''' Ties reduce the effective sample size for the slope distribution.
        ''' </summary>
        Public lNoTies As Integer

        ''' <summary>
        ''' The Theil–Sen median slope estimate, defined as the median of all
        ''' pairwise slopes (yⱼ − yᵢ) / (xⱼ − xᵢ).
        ''' </summary>
        Public MedianSlope As Double

        ''' <summary>
        ''' Lower confidence limit for the slope estimate.
        ''' Typically computed using Kendall‑based variance or
        ''' distributional inversion of the ordered slopes.
        ''' </summary>
        Public LLslope As Double

        ''' <summary>
        ''' Upper confidence limit for the slope estimate.
        ''' </summary>
        Public ULslope As Double

        ''' <summary>
        ''' Intercept estimate for the Theil–Sen regression line.
        ''' Often computed as:
        ''' <code>
        ''' intercept = median( yᵢ − slope × xᵢ )
        ''' </code>
        ''' ensuring robustness to outliers in both variables.
        ''' </summary>
        Public Intercept As Double

    End Class


    ''' <summary>
    ''' Implements the Theil–Sen nonparametric simple linear regression estimator.
    ''' 
    ''' The Theil–Sen slope is defined as the median of all pairwise slopes:
    ''' <code>
    ''' slope = median( (yⱼ − yᵢ) / (xⱼ − xᵢ) ),  for all i .lt. j and xⱼ ≠ xᵢ
    ''' </code>
    ''' 
    ''' This estimator is:
    ''' <list type="bullet">
    '''   <item><description>Highly robust to outliers</description></item>
    '''   <item><description>Invariant to monotone transformations of X</description></item>
    '''   <item><description>Distribution‑free under minimal assumptions</description></item>
    ''' </list>
    ''' 
    ''' Confidence limits for the slope are computed using the large‑sample
    ''' approximation described in Sen (1968) and Conover (1980):
    ''' <code>
    ''' L = slope(rank_L),   U = slope(rank_U)
    ''' rank limits = (N ± z * sqrt( n(n−1)(2n+5)/18 )) / 2
    ''' </code>
    ''' 
    ''' The intercept is computed using the robust estimator:
    ''' <code>
    ''' intercept = median(Y) − slope × median(X)
    ''' </code>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>Median</c> — sample median</description></item>
    '''   <item><description><c>GetColumnFrom2Darray</c> — extracts X or Y column</description></item>
    '''   <item><description><c>NormSInv</c> — inverse standard normal CDF</description></item>
    '''   <item><description><c>GeneralScatterPlot</c> — Excel scatter plot generator</description></item>
    '''   <item><description><c>ResultTable</c>, <c>TheilSenResults</c></description></item>
    ''' </list>
    ''' </summary>
    Public Class TheilSen

        ''' <summary>Input data matrix: column 0 = Y, column 1 = X.</summary>
        Private data(,) As Double

        ''' <summary>Names of the variables (Y, X).</summary>
        Private varNames() As String

        ''' <summary>Stores slope, intercept, and confidence limits.</summary>
        Private TSresults As TheilSenResults


        ''' <summary>
        ''' Initializes a Theil–Sen regression instance.
        ''' </summary>
        ''' <param name="x">Data matrix with columns (Y, X).</param>
        ''' <param name="strNames">Variable names for Y and X.</param>
        Sub New(x(,) As Double, strNames() As String)
            Me.data = x
            Me.varNames = strNames
        End Sub

        ''' <summary>
        ''' Produces formatted result tables summarizing:
        ''' <list type="bullet">
        '''   <item><description>Number of observations</description></item>
        '''   <item><description>Number of tied X values</description></item>
        '''   <item><description>Theil–Sen median slope with 95% CI</description></item>
        '''   <item><description>Intercept estimate</description></item>
        '''   <item><description>Regression equation</description></item>
        ''' </list>
        ''' 
        ''' External dependency:
        ''' <c>ResultTable</c> for formatting.
        ''' </summary>
        ''' <returns>A list of <see cref="ResultTable"/> objects.</returns>
        Public Function wrapResults() As List(Of ResultTable)
            Dim out = New List(Of ResultTable)
            Dim t = New ResultTable
            Dim strYname As String = If(varNames(0) = String.Empty, "Y", varNames(0))
            Dim strXNameEq As String = If(varNames(1) = String.Empty, "X", varNames(0))
            t.SetBody({{"Number of data points", UBound(Me.data, 1) + 1},
                  {"Number of X-ties", Me.TSresults.lNoTies},
                  {"Median Slope(95%CI)", CStr(TSresults.MedianSlope) & " (" & CStr(TSresults.LLslope) & " to " & CStr(TSresults.ULslope) & ")"},
                  {"Intercept", Me.TSresults.Intercept},
                  {"Equation", strYname & " = " & CStr(TSresults.MedianSlope) & " " & strXNameEq & " + " & CStr(TSresults.Intercept)}
                 })
            t.AddHeaderTopRow({"Theil-Sen nonparametric linear regression", ""})
            out.Add(t)
            Return out
        End Function

        ''' <summary>
        ''' Computes the Theil–Sen slope, intercept, and confidence interval.
        ''' 
        ''' Steps:
        ''' <list type="number">
        '''   <item><description>Compute medians of X and Y.</description></item>
        '''   <item><description>Compute all pairwise slopes (yⱼ − yᵢ)/(xⱼ − xᵢ).</description></item>
        '''   <item><description>Remove undefined slopes where xⱼ = xᵢ (ties).</description></item>
        '''   <item><description>Median of slopes = Theil–Sen slope.</description></item>
        '''   <item><description>Compute confidence limits using Sen’s large‑sample approximation.</description></item>
        '''   <item><description>Compute intercept using Conover’s robust formula.</description></item>
        ''' </list>
        ''' 
        ''' Confidence interval:
        ''' <code>
        ''' rank_L = (N − z √(n(n−1)(2n+5)/18)) / 2
        ''' rank_U = (N + z √(n(n−1)(2n+5)/18)) / 2
        ''' </code>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>Median</c></description></item>
        '''   <item><description><c>GetColumnFrom2Darray</c></description></item>
        '''   <item><description><c>NormSInv</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="alpha">Significance level for CI (default 0.05).</param>
        ''' <returns>A <see cref="TheilSenResults"/> object containing slope, CI, and intercept.</returns>
        Public Function compute(Optional alpha As Double = 0.05) As TheilSenResults
            ' Theil-Sen nonparametric simple linear regression
            Dim jj As Long, arSlopes() As Double, dYmedian As Double, dXmedian As Double
            TSresults = New TheilSenResults

            'calculate nonparametric regression parameters
            dYmedian = Median(GetColumnFrom2Darray(Me.data, 0))
            dXmedian = Median(GetColumnFrom2Darray(Me.data, 1))
            Dim n As Long = UBound(Me.data, 1) + 1

            'calculate all posible slopes from data points with distinc X-axis coordinate
            ReDim arSlopes(n * (n - 1) / 2 - 1) 'all pairwise slopes between two data points will be caluculated

            Dim ii As Long = 0
            For i = 0 To n - 2
                For j = i + 1 To n - 1
                    If data(i, 1) <> data(j, 1) Then
                        arSlopes(ii) = (data(i, 0) - data(j, 0)) / (data(i, 1) - data(j, 1))
                        ii += 1
                    ElseIf data(j, 1) = data(i, 1) Then
                        jj += 1 'only slopes actualy calucalted are considered in subsequent median slope calculation
                    End If
                Next
            Next

            Me.TSresults.lNoTies = jj
            ReDim Preserve arSlopes(ii - 1)

            'Fit Median slope
            Array.Sort(arSlopes)
            If ii Mod 2 = 0 Then
                Me.TSresults.MedianSlope = (arSlopes(ii / 2 - 1) + arSlopes(ii / 2)) / 2
            Else
                Me.TSresults.MedianSlope = arSlopes((ii - 1) / 2)
            End If

            '95% CI for slope
            'are calculated according large-sample approximation equations. The large-sample approximation is appropriate
            'for samples that include at least 20 pairs of data. A sample size of five XY points is, algebraically,
            'the min sample size that will produce meaningful ranks. Use of eq. 4 and 5 with five XY points
            'will produce a 95% CI including all 10 pairwise slopes.

            'if there are less then 5 distinct points, then confidence limits are the min and max calculated slope
            Dim q As Double = distributions.NormSInv(1.0 - alpha / 2.0)
            Dim lLLrank As Long = (ii - q * Math.Sqrt((n * (n - 1) * (2 * n + 5)) / 18)) / 2
            If lLLrank < 1 Then lLLrank = 1
            Dim lULrank As Long = (ii + q * Math.Sqrt((n * (n - 1) * (2 * n + 5)) / 18)) / 2
            If lULrank > ii Then lULrank = ii

            Me.TSresults.LLslope = arSlopes(lLLrank - 1) 'zero based
            Me.TSresults.ULslope = arSlopes(lULrank - 1)

            'The estimate of the intercept is calculated by use of the Conover (1980) eq.
            Me.TSresults.Intercept = dYmedian - Me.TSresults.MedianSlope * dXmedian

            Return TSresults
        End Function

        ''' <summary>
        ''' Adds a scatter plot with the Theil–Sen regression line to an Excel worksheet.
        ''' 
        ''' The fitted line is drawn between:
        ''' <code>
        ''' (min(X), intercept + slope × min(X))
        ''' (max(X), intercept + slope × max(X))
        ''' </code>
        ''' 
        ''' External dependency:
        ''' <c>GeneralScatterPlot</c> — creates the base scatter plot.
        ''' </summary>
        ''' <param name="ws">Excel worksheet to receive the plot.</param>
        Sub AddPlot(ws As Worksheet)
            Dim ch = graphics.GeneralScatterPlot(GetColumnFrom2Darray(data, 1),
                                             GetColumnFrom2Darray(data, 0),
                                             varNames(0),
                                             varNames(1),
                                             ws)
            Dim dMinX As Double = GetColumnFrom2Darray(data, 1).Min()
            Dim dMaxX As Double = GetColumnFrom2Darray(data, 1).Max()

            With ch
                'add and plot nonparametric fit line
                .SeriesCollection.NewSeries
                With .SeriesCollection(2)
                    .XValues = {dMinX, dMaxX}
                    .Values = {TSresults.Intercept + TSresults.MedianSlope * dMinX,
                           TSresults.Intercept + TSresults.MedianSlope * dMaxX}
                    .Name = "Nonparametric Fit"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(255, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1.5
                    End With
                End With
            End With
        End Sub

    End Class



    Public Module Nonparametric

        ''' <summary>
        ''' Computes the tie‑correction term 
        ''' <c>T = Σ (tᵢ³ − tᵢ)</c> 
        ''' used in rank‑based nonparametric tests such as:
        ''' <list type="bullet">
        '''   <item><description>Kruskal–Wallis H test</description></item>
        '''   <item><description>Friedman test</description></item>
        '''   <item><description>Kendall’s τ and τ‑b</description></item>
        '''   <item><description>Wilcoxon and Mann–Whitney tie adjustments</description></item>
        ''' </list>
        ''' 
        ''' For each distinct value in <paramref name="x"/>, let tᵢ be the number of
        ''' tied observations. The tie‑correction factor is:
        ''' <code>
        ''' T = Σ (tᵢ³ − tᵢ)
        ''' </code>
        ''' 
        ''' This quantity is used to adjust the variance of rank‑based statistics
        ''' when ties are present. For example, in the Kruskal–Wallis test:
        ''' <code>
        ''' H_corrected = H / (1 − T / (N³ − N))
        ''' </code>
        ''' 
        ''' The function returns only the numerator T; callers apply the appropriate
        ''' scaling depending on the statistical test.
        ''' </summary>
        ''' <param name="x">Array of ranked or raw values from which tie groups are identified.</param>
        ''' <returns>
        ''' The tie‑correction sum <c>T = Σ (tᵢ³ − tᵢ)</c>.
        ''' </returns>
        Function TiesCorrection(x() As Double) As Double
            Dim c As Double, dict As New Dictionary(Of Double, Integer)
            dict.Add(x(0), 1)
            For i = 1 To UBound(x)
                If dict.ContainsKey(x(i)) Then
                    dict.Item(x(i)) += 1
                Else
                    dict.Add(x(i), 1)
                End If
            Next
            For Each key In dict.Keys
                c += (CDbl(dict.Item(key)) ^ 3 - CDbl(dict.Item(key)))
            Next key
            Return c
        End Function

        ''' <summary>
        ''' Computes average ranks for a numeric vector, with full tie handling.
        ''' 
        ''' This function assigns ranks to the values in <paramref name="arData"/> by:
        ''' <list type="number">
        '''   <item><description>Sorting the values while preserving original indices.</description></item>
        '''   <item><description>Identifying tied groups (equal values).</description></item>
        '''   <item><description>Assigning each tied group the average of their rank positions.</description></item>
        ''' </list>
        ''' 
        ''' Ranking convention:
        ''' <code>
        ''' If values at sorted positions i … j are tied,
        ''' average rank = (i + j + 2) / 2
        ''' </code>
        ''' 
        ''' This corresponds to the standard “midrank” method used in:
        ''' <list type="bullet">
        '''   <item><description>Mann–Whitney U test</description></item>
        '''   <item><description>Wilcoxon Signed‑Rank test</description></item>
        '''   <item><description>Kruskal–Wallis test</description></item>
        '''   <item><description>Friedman test</description></item>
        '''   <item><description>Spearman’s rank correlation</description></item>
        ''' </list>
        ''' 
        ''' The returned array preserves the original ordering of <paramref name="arData"/>.
        ''' </summary>
        ''' <param name="arData">A one‑dimensional array of numeric values to be ranked.</param>
        ''' <returns>
        ''' A Double() array of the same length as <paramref name="arData"/>,
        ''' containing the average ranks corresponding to each original element.
        ''' </returns>
        Function ComputeAvgRanks(arData() As Double) As Double()
            'arData  - 1D array of data from which ranks will be computed
            Dim n As Integer = arData.Length
            Dim indexed = arData.Select(Function(v, k) New With {.Value = v, .OriginalIndex = k}).ToList()
            indexed.Sort(Function(a, b) a.Value.CompareTo(b.Value))

            Dim ranks(n - 1) As Double
            Dim i = 0
            While i < n
                Dim j = i
                While j + 1 < n AndAlso indexed(j + 1).Value = indexed(i).Value
                    j += 1
                End While

                Dim avgRank = (i + j + 2) / 2.0
                For k = i To j
                    ranks(indexed(k).OriginalIndex) = avgRank
                Next

                i = j + 1
            End While

            Return ranks

        End Function

        ''' <summary>
        ''' Computes the Skillings–Mack test statistic for incomplete block designs.
        ''' 
        ''' The Skillings–Mack test is a generalization of the Friedman test that allows:
        ''' <list type="bullet">
        '''   <item><description>Unequal numbers of observations per block</description></item>
        '''   <item><description>Missing values within blocks</description></item>
        '''   <item><description>Arbitrary block sizes (≥ 2 non‑missing values)</description></item>
        ''' </list>
        ''' 
        ''' The test ranks treatments **within each block**, standardizes the ranks,
        ''' and forms a quadratic statistic:
        ''' <code>
        ''' T = Rᵀ Σ⁻¹ R
        ''' </code>
        ''' where:
        ''' <list type="bullet">
        '''   <item><description>R = vector of standardized rank sums</description></item>
        '''   <item><description>Σ = covariance matrix of standardized ranks</description></item>
        ''' </list>
        ''' 
        ''' Blocks with fewer than two non‑missing observations are removed, as they
        ''' contribute no ranking information.
        ''' 
        ''' The asymptotic distribution of T is chi‑square with (k − 1) degrees of freedom,
        ''' where k is the number of treatments.
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>ComputeAvgRanks</c> — computes average ranks with ties</description></item>
        '''   <item><description><c>MatInv</c> — matrix inversion (LU/Cholesky)</description></item>
        '''   <item><description><c>MatrixMult</c> — matrix multiplication</description></item>
        '''   <item><description><c>trans</c> — matrix transpose</description></item>
        '''   <item><description><c>ChiSquareCDF</c> — chi‑square CDF</description></item>
        '''   <item><description><c>TestResult</c> — container for test statistics</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="data">
        ''' A 2D array of observations where:
        ''' <list type="bullet">
        '''   <item><description>Rows = blocks</description></item>
        '''   <item><description>Columns = treatments</description></item>
        '''   <item><description>Missing values are represented as <c>Double.NaN</c></description></item>
        ''' </list>
        ''' </param>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        '''   <item><description><c>TestStatistics1</c> — Skillings–Mack statistic T</description></item>
        '''   <item><description><c>Pvalue</c> — asymptotic chi‑square p‑value</description></item>
        ''' </list>
        ''' </returns>
        Public Function SkillingsMack(data(,) As Double) As TestResult
            'bSimulatedPvalue = true if you want to calculate simpulated p-value
            'NoBlocks - output
            'SMoutcome user defined type (test statistic, asymptotic p-value and simulated p-value)

            'assumes that treatments are in columns and block are in rows
            Dim NoColumns As Integer = UBound(data, 2) + 1 '# of treatments
            Dim NoRows As Integer = UBound(data, 1) + 1
            Dim nanCount As Integer = data.Cast(Of Double)().Count(Function(x) Double.IsNaN(x)) 'data.Cast(Of Double)() flattens the 2D array into a sequence of doubles.
            Dim n As Integer = NoRows * NoColumns - nanCount

            'check for bloks with missing data. In each block, there have to be at least two valid data, otherwise the block is ommited
            'rewrite the "correct" blocks from range to the array
            ' Filter rows with at least 2 non-NaN values
            Dim validRows = Enumerable.Range(0, NoRows).
                                    Where(Function(i) Enumerable.Range(0, NoColumns).
                                        Count(Function(j) Not Double.IsNaN(data(i, j))) >= 2).
                                    ToList()

            ' Build new 2D array
            Dim arData(validRows.Count - 1, NoColumns - 1) As Double
            For r As Integer = 0 To validRows.Count - 1
                For c As Integer = 0 To NoColumns - 1
                    arData(r, c) = data(validRows(r), c)
                Next
            Next

            Dim NoBlocks As Integer = UBound(arData, 1) + 1
            'compute ranks
            Dim Ranks(NoBlocks - 1, NoColumns - 1) As Double, NonMiss(NoBlocks - 1, NoColumns - 1) As Boolean
            Dim ki(NoBlocks - 1) As Double 'reference array in the ranking when there are missing data
            Dim RanksSum(NoColumns - 1, 0) As Double, RanksSum2(NoColumns - 2, 0) As Double

            For i = 0 To NoBlocks - 1
                Dim arTempMis(NoColumns - 1) As Double
                Dim ii As Integer = 0
                For j = 0 To NoColumns - 1
                    If Not Double.IsNaN(arData(i, j)) Then
                        arTempMis(ii) = arData(i, j) 'reference array in the ranking when there are missing data
                        ii += 1
                        NonMiss(i, j) = True
                    Else
                        NonMiss(i, j) = False
                    End If
                Next
                ki(i) = ii
                ReDim Preserve arTempMis(ii - 1)

                'calculate ranks
                Dim arRnk = ComputeAvgRanks(arTempMis)
                ii = 0
                For j = 0 To NoColumns - 1
                    If NonMiss(i, j) Then
                        Ranks(i, j) = arRnk(ii)
                        ii += 1
                    Else
                        Ranks(i, j) = (ki(i) + 1) / 2
                    End If
                    RanksSum(j, 0) += (Math.Sqrt(12 / (ki(i) + 1))) * (Ranks(i, j) - (ki(i) + 1) / 2)
                    If j < NoColumns - 1 Then RanksSum2(j, 0) = RanksSum(j, 0)
                Next
            Next

            'compute covariance matrix
            Dim covarianceMatrix(NoColumns - 1, NoColumns - 1) As Double, CovMat2(NoColumns - 2, NoColumns - 2) As Double

            For i = 0 To NoColumns - 1
                For ii = 0 To NoColumns - 1
                    If i <> ii Then
                        For j = 0 To NoBlocks - 1
                            If NonMiss(j, i) And NonMiss(j, ii) Then
                                covarianceMatrix(i, i) += 1 'diagonal elements
                                If i < ii Then 'count it only once
                                    'off-diagonal elements
                                    covarianceMatrix(i, ii) -= 1
                                    covarianceMatrix(ii, i) = covarianceMatrix(i, ii)
                                End If
                            End If
                        Next
                    End If
                Next
            Next i

            'compute test statistic - in the form of quadratic sum
            For i = 0 To NoColumns - 2
                For j = 0 To NoColumns - 2
                    CovMat2(i, j) = covarianceMatrix(i, j)
                Next
            Next

            Dim CovInv = MatInv(CovMat2, method:="CHOL") 'find inverese using LU decomposition
            Dim TestStatistic = MatrixMult(MatrixMult(trans(RanksSum2), CovInv), RanksSum2)

            Dim P_value As Double = 1.0 - distributions.ChiSquareCDF(TestStatistic(0, 0), NoColumns - 1)

            'return outcomes-----------------------------------------------------
            Dim out = New TestResult
            out.TestStatistics1 = TestStatistic(0, 0)
            out.Pvalue = P_value

            Return out
        End Function
    End Module
End Namespace