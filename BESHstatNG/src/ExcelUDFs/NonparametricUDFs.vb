Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports BESHStatNG.nonparametric
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions


    Public Module NonparametricUDFs
        ' -------------------------------------------------------------------------------------------------------------
        ' Mann–Whitney U (Wilcoxon rank-sum) test
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Mann–Whitney (Wilcoxon rank-sum) test — two-sided p-value using the normal approximation.
        ''' </summary>
        ''' <param name="group1">
        ''' Group 1 data (a single-column Excel range).
        ''' Non-numeric cells (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <param name="group2">
        ''' Group 2 data (a single-column Excel range).
        ''' Non-numeric cells (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <returns>
        ''' Two-sided p-value computed via the continuity-corrected, tie-corrected normal approximation.
        ''' Returns <c>#VALUE!</c> if either input range is not a single column.
        ''' Returns <c>#NUM!</c> if there are insufficient numeric observations.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The Mann–Whitney U test (also called the Wilcoxon rank‑sum test) compares two independent groups.
        ''' It tests whether one group tends to have larger values than the other without assuming normality.
        ''' <code>
        ''' p = 2 * P( Z ≤ -|z| )
        ''' </code>
        ''' where <c>z</c> is a continuity-corrected normal statistic with tie correction.
        ''' </para>
        ''' <para>
        ''' In Excel terminology, this is an <b>asymptotic</b> p-value (normal approximation),
        ''' suitable for moderate-to-large sample sizes.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.MW_P_NORM(A2:A21, B2:B16)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.MW_P_NORM",
            Category:="BESHStatNG - Nonparametric",
            Description:="Mann–Whitney test: two-sided p-value (normal approximation with ties & continuity correction).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/")>
        Public Function MW_P_NORM(
            <ExcelArgument(Name:="group1", Description:="Group 1 data (single-column range; non-numeric ignored).")> group1 As Object,
            <ExcelArgument(Name:="group2", Description:="Group 2 data (single-column range; non-numeric ignored).")> group2 As Object
        ) As Object

            Dim x1 As Double(), x2 As Double()
            Dim err As ExcelError? = Nothing

            x1 = UDFhelpers.ExtractNumericColumnIgnoringNonNumeric(group1, err)
            If err.HasValue Then Return err.Value

            x2 = UDFhelpers.ExtractNumericColumnIgnoringNonNumeric(group2, err)
            If err.HasValue Then Return err.Value

            If x1.Length < 1 OrElse x2.Length < 1 Then
                Return ExcelError.ExcelErrorNum
            End If

            Dim data As Double()() = {x1, x2}
            Dim mw As New MannWhitney(data, "group1", "group2")
            Dim res As TestResult = mw.Compute()

            Dim p As Double = res.Pvalue
            If Double.IsNaN(p) OrElse Double.IsInfinity(p) OrElse p < 0.0 OrElse p > 1.0 Then
                Return ExcelError.ExcelErrorNum
            End If
            Return p
        End Function


        ''' <summary>
        ''' Mann–Whitney (Wilcoxon rank-sum) test — exact p-value (available only for total n ≤ 50).
        ''' </summary>
        ''' <param name="group1">
        ''' Group 1 data (a single-column Excel range).
        ''' Non-numeric cells (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <param name="group2">
        ''' Group 2 data (a single-column Excel range).
        ''' Non-numeric cells (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <param name="side">
        ''' Specifies which exact p-value to return:
        ''' <list type="bullet">
        ''' <item><description><c>"two"</c> / <c>"two-sided"</c> / <c>"2"</c> — two-sided exact p-value</description></item>
        ''' <item><description><c>"lower"</c> / <c>"less"</c> — lower-tail exact p-value</description></item>
        ''' <item><description><c>"upper"</c> / <c>"greater"</c> — upper-tail exact p-value</description></item>
        ''' </list>
        ''' The comparison direction matches the internal implementation: lower-tail corresponds to smaller U
        ''' values (group 1 tends to have smaller values than group 2).
        ''' </param>
        ''' <returns>
        ''' Exact p-value requested by <paramref name="side"/> when the exact distribution is available.
        ''' Returns <c>#VALUE!</c> if either input range is not a single column, or if <paramref name="side"/>
        ''' is not recognized.
        ''' Returns <c>#NUM!</c> if there are insufficient numeric observations, or if the exact p-value is not
        ''' available (total sample size &gt; 50).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This worksheet function computes an <b>exact</b> p‑value for the Mann–Whitney U statistic by enumerating
        ''' its sampling distribution using a dynamic‑programming approach. Exact p‑values are most useful for small
        ''' sample sizes and discrete data.
        ''' </para>
        ''' <para>
        ''' Exact computation is performed only when <c>n = n1 + n2 ≤ 50</c>
        ''' For larger samples use <see cref="MW_P_NORM"/>.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.MW_P_EXACT(A2:A10, B2:B12, "two")
        ''' =BESH.NP.MW_P_EXACT(A2:A10, B2:B12, "lower")
        ''' =BESH.NP.MW_P_EXACT(A2:A10, B2:B12, "upper")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.MW_P_EXACT",
            Category:="BESHStatNG - Nonparametric",
            Description:="Mann–Whitney test: exact p-value (n ≤ 50). side: two/lower/upper.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/")>
        Public Function MW_P_EXACT(
            <ExcelArgument(Name:="group1", Description:="Group 1 data (single-column range; non-numeric ignored).")> group1 As Object,
            <ExcelArgument(Name:="group2", Description:="Group 2 data (single-column range; non-numeric ignored).")> group2 As Object,
            <ExcelArgument(Name:="side", Description:="Which exact p-value to return: two/lower/upper.")> side As String
        ) As Object

            Dim x1 As Double(), x2 As Double()
            Dim err As ExcelError? = Nothing

            x1 = UDFhelpers.ExtractNumericColumnIgnoringNonNumeric(group1, err)
            If err.HasValue Then Return err.Value

            x2 = UDFhelpers.ExtractNumericColumnIgnoringNonNumeric(group2, err)
            If err.HasValue Then Return err.Value

            If x1.Length < 1 OrElse x2.Length < 1 Then
                Return ExcelError.ExcelErrorNum
            End If

            Dim n As Integer = x1.Length + x2.Length
            If n > 50 Then
                Return ExcelError.ExcelErrorNum
            End If

            Dim data As Double()() = {x1, x2}
            Dim mw As New MannWhitney(data, "group1", "group2")
            Dim res As TestResult = mw.Compute()

            If Not res.bExactAvailable Then
                Return ExcelError.ExcelErrorNum
            End If

            Dim which As String = If(side, "").Trim().ToLowerInvariant()
            Select Case which
                Case "two", "two-sided", "twosided", "2"
                    Return ClampProb(res.PvalueExact)
                Case "lower", "less", "left", "l"
                    Return ClampProb(res.pValueExactLowerSide)
                Case "upper", "greater", "right", "u"
                    Return ClampProb(res.pValueExactUpperSide)
                Case Else
                    Return ExcelError.ExcelErrorValue
            End Select
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Wilcoxon signed-rank test (paired samples)
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Wilcoxon signed-rank test — two-sided p-value using the normal approximation (paired samples).
        ''' </summary>
        ''' <param name="x">
        ''' First set of paired observations (a single-column Excel range).
        ''' Values are paired by row with <paramref name="y"/>.
        ''' Rows where either cell is non-numeric (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <param name="y">
        ''' Second set of paired observations (a single-column Excel range).
        ''' Values are paired by row with <paramref name="x"/>.
        ''' Rows where either cell is non-numeric (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <returns>
        ''' Two-sided p-value based on the continuity-corrected, tie-corrected normal approximation.
        ''' Returns <c>#VALUE!</c> if either input is not a single column or if the input ranges have different row counts.
        ''' Returns <c>#NUM!</c> if there are insufficient usable pairs.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The Wilcoxon signed-rank test compares two <b>paired</b> samples (e.g., before/after measurements on the same subjects).
        ''' It tests whether the median of the paired differences (<c>x - y</c>) is zero without assuming normality.
        ''' </para>
        ''' <para>
        ''' The test forms paired differences, discards zero differences, ranks the absolute differences (averaging tied ranks),
        ''' and computes <c>W</c>, the sum of ranks for positive differences. This function returns a two-sided p-value
        ''' using a normal approximation with tie correction and a continuity correction.
        ''' </para>
        ''' <para>
        ''' For small samples, you may prefer the exact p-value returned by <see cref="WILCOX_P_EXACT"/>.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.WILCOX_P_NORM(A2:A21, B2:B21)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.WILCOX_P_NORM",
            Category:="BESHStatNG - Nonparametric",
            Description:="Wilcoxon signed-rank test: two-sided p-value (normal approximation; paired samples).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/")>
        Public Function WILCOX_P_NORM(
            <ExcelArgument(Name:="x", Description:="First paired sample (single-column; paired by row; non-numeric rows ignored).")> x As Object,
            <ExcelArgument(Name:="y", Description:="Second paired sample (single-column; paired by row; non-numeric rows ignored).")> y As Object) As Object

            Dim pairs As Double(,)
            Dim err As ExcelError? = Nothing

            pairs = UDFhelpers.ExtractPairedNumericColumnsIgnoringNonNumeric(x, y, err)
            If err.HasValue Then Return err.Value

            If pairs Is Nothing OrElse pairs.GetLength(0) < 1 Then
                Return ExcelError.ExcelErrorNum
            End If

            Dim w As New WilcoxonTest(pairs, "x", "y")
            Dim res As TestResult = w.Compute()

            Dim p As Double = res.Pvalue
            Return ClampProb(p)
        End Function

        ''' <summary>
        ''' Wilcoxon signed-rank test — exact p-value (paired samples; available only for up to 60 non-zero differences).
        ''' </summary>
        ''' <param name="x">
        ''' First set of paired observations (a single-column Excel range).
        ''' Values are paired by row with <paramref name="y"/>.
        ''' Rows where either cell is non-numeric (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <param name="y">
        ''' Second set of paired observations (a single-column Excel range).
        ''' Values are paired by row with <paramref name="x"/>.
        ''' Rows where either cell is non-numeric (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <param name="side">
        ''' Specifies which exact p-value to return:
        ''' <list type="bullet">
        ''' <item><description><c>"two"</c> / <c>"two-sided"</c> / <c>"2"</c> — two-sided exact p-value</description></item>
        ''' <item><description><c>"lower"</c> / <c>"less"</c> — lower-tail exact p-value</description></item>
        ''' <item><description><c>"upper"</c> / <c>"greater"</c> — upper-tail exact p-value</description></item>
        ''' </list>
        ''' Lower-tail corresponds to unusually small <c>W</c> (more negative differences), upper-tail to unusually large <c>W</c>.
        ''' </param>
        ''' <returns>
        ''' Exact p-value requested by <paramref name="side"/> when exact computation is available.
        ''' Returns <c>#VALUE!</c> if either input is not a single column, if the input ranges have different row counts,
        ''' or if <paramref name="side"/> is not recognized.
        ''' Returns <c>#NUM!</c> if there are insufficient usable pairs or if the exact p-value is not available.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This worksheet function computes an <b>exact</b> p-value for the Wilcoxon signed-rank statistic by constructing
        ''' the exact sampling distribution via dynamic programming. Exact p-values are most useful for small samples.
        ''' </para>
        ''' <para>
        ''' Exact computation is performed only when the number of <b>non-zero</b> paired differences is at most 60.
        ''' If there are more non-zero differences, the function returns <c>#NUM!</c>. In that case use <see cref="WILCOX_P_NORM"/>.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.WILCOX_P_EXACT(A2:A10, B2:B10, "two")
        ''' =BESH.NP.WILCOX_P_EXACT(A2:A10, B2:B10, "lower")
        ''' =BESH.NP.WILCOX_P_EXACT(A2:A10, B2:B10, "upper")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.WILCOX_P_EXACT",
            Category:="BESHStatNG - Nonparametric",
            Description:="Wilcoxon signed-rank test: exact p-value (paired samples; up to 60 non-zero diffs). side: two/lower/upper.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/")>
        Public Function WILCOX_P_EXACT(
            <ExcelArgument(Name:="x", Description:="First paired sample (single-column; paired by row; non-numeric rows ignored).")> x As Object,
            <ExcelArgument(Name:="y", Description:="Second paired sample (single-column; paired by row; non-numeric rows ignored).")> y As Object,
            <ExcelArgument(Name:="side", Description:="Which exact p-value to return: two/lower/upper.")> side As String) As Object

            Dim pairs As Double(,)
            Dim err As ExcelError? = Nothing

            pairs = UDFhelpers.ExtractPairedNumericColumnsIgnoringNonNumeric(x, y, err)
            If err.HasValue Then Return err.Value

            If pairs Is Nothing OrElse pairs.GetLength(0) < 1 Then
                Return ExcelError.ExcelErrorNum
            End If

            Dim w As New WilcoxonTest(pairs, "x", "y")
            Dim res As TestResult = w.Compute()

            If Not res.bExactAvailable Then
                Return ExcelError.ExcelErrorNum
            End If

            Dim which As String = If(side, "").Trim().ToLowerInvariant()
            Select Case which
                Case "two", "two-sided", "twosided", "2"
                    Return ClampProb(res.PvalueExact)
                Case "lower", "less", "left", "l"
                    Return ClampProb(res.pValueExactLowerSide)
                Case "upper", "greater", "right", "u"
                    Return ClampProb(res.pValueExactUpperSide)
                Case Else
                    Return ExcelError.ExcelErrorValue
            End Select
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Spearman rank correlation (paired samples)
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Returns Spearman's rank correlation coefficient (ρ) for two paired samples.
        ''' </summary>
        ''' <param name="xRange">
        ''' One-column range containing the first variable (X). Values are paired by row with <paramref name="yRange"/>.
        ''' Non-numeric cells are ignored together with the corresponding row in <paramref name="yRange"/>.
        ''' </param>
        ''' <param name="yRange">
        ''' One-column range containing the second variable (Y). Values are paired by row with <paramref name="xRange"/>.
        ''' Non-numeric cells are ignored together with the corresponding row in <paramref name="xRange"/>.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the internal confidence-interval metadata
        ''' computed by the underlying Spearman procedure.
        ''' The returned coefficient itself does not depend on <c>alpha</c>.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' Spearman's ρ in the range [-1, 1], or an Excel error code if inputs are invalid.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Spearman's ρ is the Pearson correlation computed on the ranks of the data (average ranks are used for ties).
        ''' The result measures the strength of a monotonic association between X and Y.
        ''' </para>
        ''' <para>
        ''' Input requirements:
        ''' <list type="bullet">
        '''   <item><description>Each input must be a single-column range.</description></item>
        '''   <item><description>The two ranges must have the same number of rows (paired by row).</description></item>
        '''   <item><description>At least 3 valid numeric pairs are required.</description></item>
        '''   <item><description><c>alpha</c> must satisfy 0 &lt; alpha &lt; 1.</description></item>
        ''' </list>
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.SPEARMAN_RHO(A2:A51, B2:B51)
        ''' =BESH.NP.SPEARMAN_RHO(A2:A51, B2:B51, 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.SPEARMAN_RHO",
            Category:="BESHStatNG - Nonparametric",
            Description:="Spearman rank correlation coefficient (ρ) for two paired samples.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric.md"
        )>
        Public Function SPEARMAN_RHO(
            <ExcelArgument(Name:="x", Description:="One-column range for X (paired by row).")> xRange As Object,
            <ExcelArgument(Name:="y", Description:="One-column range for Y (paired by row).")> yRange As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha for internal CI metadata; coefficient output is unchanged.")> Optional alpha As Object = Nothing) As Object

            Dim err? As ExcelError = Nothing
            Dim pairs = UDFhelpers.ExtractPairedNumericColumnsIgnoringNonNumeric(xRange, yRange, err)
            If err.HasValue Then Return err.Value
            If pairs Is Nothing OrElse pairs.GetLength(0) < 3 Then Return ExcelError.ExcelErrorNum

            Dim alphaValue As Double = 0.05
            If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

            Dim n As Integer = pairs.GetLength(0)
            Dim x(n - 1) As Double
            Dim y(n - 1) As Double
            For i = 0 To n - 1
                x(i) = pairs(i, 0)
                y(i) = pairs(i, 1)
            Next

            Dim test = New SpearmanRho(x, y, "X", "Y")
            test.Compute(Nothing, alphaValue)
            Return test.correlCoef
        End Function

        ''' <summary>
        ''' Returns the p-value for Spearman's rank correlation test for two paired samples.
        ''' </summary>
        ''' <param name="xRange">
        ''' One-column range containing the first variable (X). Values are paired by row with <paramref name="yRange"/>.
        ''' Non-numeric cells are ignored together with the corresponding row in <paramref name="yRange"/>.
        ''' </param>
        ''' <param name="yRange">
        ''' One-column range containing the second variable (Y). Values are paired by row with <paramref name="xRange"/>.
        ''' Non-numeric cells are ignored together with the corresponding row in <paramref name="xRange"/>.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level passed through to the underlying Spearman procedure for API consistency.
        ''' The returned p-value itself does not depend on <c>alpha</c>.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' Two-sided p-value for testing the null hypothesis of no monotonic association (ρ = 0),
        ''' or an Excel error code if inputs are invalid.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The test computes Spearman's ρ from ranked data (average ranks for ties), then produces a p-value using:
        ''' </para>
        ''' <list type="bullet">
        '''   <item><description>Exact permutation p-values for small samples when feasible.</description></item>
        '''   <item><description>An accurate approximation for moderate sample sizes without ties.</description></item>
        '''   <item><description>A large-sample approximation based on a t-statistic for general cases.</description></item>
        ''' </list>
        ''' <para>
        ''' Input requirements match <see cref="SPEARMAN_RHO"/>. At least 3 valid numeric pairs are required.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.SPEARMAN_P(A2:A51, B2:B51)
        ''' =BESH.NP.SPEARMAN_P(A2:A51, B2:B51, 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.SPEARMAN_P",
            Category:="BESHStatNG - Nonparametric",
            Description:="Two-sided p-value for Spearman rank correlation test (paired samples).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/"
        )>
        Public Function SPEARMAN_P(
            <ExcelArgument(Name:="x", Description:="One-column range for X (paired by row).")> xRange As Object,
            <ExcelArgument(Name:="y", Description:="One-column range for Y (paired by row).")> yRange As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha passed through for API consistency; p-value output is unchanged.")> Optional alpha As Object = Nothing) As Object

            Dim err? As ExcelError = Nothing
            Dim pairs = UDFhelpers.ExtractPairedNumericColumnsIgnoringNonNumeric(xRange, yRange, err)
            If err.HasValue Then Return err.Value
            If pairs Is Nothing OrElse pairs.GetLength(0) < 3 Then Return ExcelError.ExcelErrorNum

            Dim alphaValue As Double = 0.05
            If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

            Dim n As Integer = pairs.GetLength(0)
            Dim x(n - 1) As Double
            Dim y(n - 1) As Double
            For i = 0 To n - 1
                x(i) = pairs(i, 0)
                y(i) = pairs(i, 1)
            Next

            Dim test = New SpearmanRho(x, y, "X", "Y")
            test.Compute(Nothing, alphaValue)
            Return test.pvalue
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Kendall rank correlation (paired samples)
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Returns Kendall’s rank correlation coefficient (τ<sub>b</sub>) for two paired samples.
        ''' </summary>
        ''' <param name="xRange">
        ''' One-column range containing the first variable (X). Values are paired by row with <paramref name="yRange"/>.
        ''' Rows where either X or Y is non-numeric (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <param name="yRange">
        ''' One-column range containing the second variable (Y). Values are paired by row with <paramref name="xRange"/>.
        ''' Rows where either X or Y is non-numeric (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the internal confidence-interval metadata
        ''' computed by the underlying Kendall procedure.
        ''' The returned coefficient itself does not depend on <c>alpha</c>.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' Kendall’s τ<sub>b</sub> in the range [-1, 1], or an Excel error code if inputs are invalid.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Kendall’s τ<sub>b</sub> is a nonparametric measure of monotonic association based on the number of
        ''' concordant and discordant pairs among the observations. The τ<sub>b</sub> variant adjusts for ties
        ''' in X and/or Y, so it remains well-defined when there are repeated values.
        ''' </para>
        ''' <para>
        ''' Input requirements:
        ''' <list type="bullet">
        '''   <item><description>Each input must be a single-column range.</description></item>
        '''   <item><description>The two ranges must have the same number of rows (paired by row).</description></item>
        '''   <item><description>At least 4 valid numeric pairs are required for the associated significance test.</description></item>
        '''   <item><description><c>alpha</c> must satisfy 0 &lt; alpha &lt; 1.</description></item>
        ''' </list>
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.KENDALL_TAU(A2:A51, B2:B51)
        ''' =BESH.NP.KENDALL_TAU(A2:A51, B2:B51, 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.KENDALL_TAU",
            Category:="BESHStatNG - Nonparametric",
            Description:="Kendall rank correlation coefficient (τb) for two paired samples.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/"
        )>
        Public Function KENDALL_TAU(
            <ExcelArgument(Name:="x", Description:="One-column range for X (paired by row).")> xRange As Object,
            <ExcelArgument(Name:="y", Description:="One-column range for Y (paired by row).")> yRange As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha for internal CI metadata; coefficient output is unchanged.")> Optional alpha As Object = Nothing) As Object

            Dim err? As ExcelError = Nothing
            Dim pairs = UDFhelpers.ExtractPairedNumericColumnsIgnoringNonNumeric(xRange, yRange, err)
            If err.HasValue Then Return err.Value
            If pairs Is Nothing OrElse pairs.GetLength(0) < 3 Then Return ExcelError.ExcelErrorNum

            Dim alphaValue As Double = 0.05
            If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

            Dim n As Integer = pairs.GetLength(0)
            Dim x(n - 1) As Double
            Dim y(n - 1) As Double
            For i = 0 To n - 1
                x(i) = pairs(i, 0)
                y(i) = pairs(i, 1)
            Next

            Dim test = New KendallsTau(x, y, "X", "Y")
            test.compute(Nothing, alphaValue)
            Return test.correlCoef
        End Function

        ''' <summary>
        ''' Returns the p-value for Kendall’s rank correlation test (τ<sub>b</sub>) for two paired samples.
        ''' </summary>
        ''' <param name="xRange">
        ''' One-column range containing the first variable (X). Values are paired by row with <paramref name="yRange"/>.
        ''' Rows where either X or Y is non-numeric (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <param name="yRange">
        ''' One-column range containing the second variable (Y). Values are paired by row with <paramref name="xRange"/>.
        ''' Rows where either X or Y is non-numeric (empty, text, logical, error) are ignored.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level passed through to the underlying Kendall procedure for API consistency.
        ''' The returned p-value itself does not depend on <c>alpha</c>.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' Two-sided p-value for testing the null hypothesis of no association (τ = 0).
        ''' Returns an Excel error code if inputs are invalid.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The test is based on Kendall’s τ<sub>b</sub> and uses an exact permutation distribution for very small samples
        ''' when feasible; otherwise it uses an accurate approximation that accounts for ties.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.KENDALL_P(A2:A51, B2:B51)
        ''' =BESH.NP.KENDALL_P(A2:A51, B2:B51, 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.KENDALL_P",
            Category:="BESHStatNG - Nonparametric",
            Description:="P-value for Kendall rank correlation test (τb) for two paired samples.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/"
        )>
        Public Function KENDALL_P(
            <ExcelArgument(Name:="x", Description:="One-column range for X (paired by row).")> xRange As Object,
            <ExcelArgument(Name:="y", Description:="One-column range for Y (paired by row).")> yRange As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha passed through for API consistency; p-value output is unchanged.")> Optional alpha As Object = Nothing) As Object

            Dim err? As ExcelError = Nothing
            Dim pairs = UDFhelpers.ExtractPairedNumericColumnsIgnoringNonNumeric(xRange, yRange, err)
            If err.HasValue Then Return err.Value
            If pairs Is Nothing OrElse pairs.GetLength(0) < 4 Then Return ExcelError.ExcelErrorNum

            Dim alphaValue As Double = 0.05
            If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

            Dim n As Integer = pairs.GetLength(0)
            Dim x(n - 1) As Double
            Dim y(n - 1) As Double
            For i = 0 To n - 1
                x(i) = pairs(i, 0)
                y(i) = pairs(i, 1)
            Next

            Dim test = New KendallsTau(x, y, "X", "Y")
            test.compute(Nothing, alphaValue)
            Return test.pvalue
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Kruskal-Wallis H test (independent samples, 2+ groups)
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Kruskal-Wallis test statistic (H) for comparing 2 or more independent groups.
        ''' </summary>
        ''' <param name="groups">
        ''' Input data arranged as one column per group (a multi-column Excel range).
        ''' Each column represents an independent group of observations.
        ''' Non-numeric cells (empty, text, logical, error) are ignored within each column.
        ''' </param>
        ''' <param name="statType">
        ''' Select which statistic to return:
        ''' <list type="bullet">
        ''' <item><description><c>"H"</c> - the uncorrected Kruskal-Wallis H statistic</description></item>
        ''' <item><description><c>"Hcor"</c> - tie-corrected H (recommended when there are ties)</description></item>
        ''' </list>
        ''' The comparison is case-insensitive. If omitted, <c>"Hcor"</c> is used.
        ''' </param>
        ''' <returns>
        ''' The Kruskal-Wallis test statistic (H or H<sub>cor</sub>) as a number.
        ''' Returns an Excel error code if inputs are invalid.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The Kruskal-Wallis test is a nonparametric alternative to one-way ANOVA.
        ''' It tests whether multiple independent groups come from the same distribution,
        ''' by ranking all observations together and comparing the sums of ranks between groups.
        ''' </para>
        ''' <para>
        ''' When there are tied values, a tie correction can be applied to the H statistic.
        ''' The tie-corrected version (H<sub>cor</sub>) is usually preferred in real data.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.KW_STAT(A2:C20, "Hcor")         ' 3 groups stored in columns A..C
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.KW_STAT",
            Category:="BESHStatNG - Nonparametric",
            Description:="Kruskal-Wallis test statistic H (or tie-corrected Hcor) for 2+ independent groups.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/"
        )>
        Public Function KW_STAT(
            <ExcelArgument(Name:="groups", Description:="Multi-column range: one column per group (independent samples).")> groups As Object,
            <ExcelArgument(Name:="type", Description:="""H"" for uncorrected H; ""Hcor"" for tie-corrected H (default).")> Optional statType As Object = Nothing
        ) As Object

            Dim err? As ExcelError = Nothing
            Dim data = UDFhelpers.ExtractNumericGroupsFromColumnsIgnoringNonNumeric(groups, err)
            If err.HasValue Then Return err.Value
            If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum

            Dim names(data.Length - 1) As String
            For i As Integer = 0 To data.Length - 1
                names(i) = "G" & (i + 1).ToString()
            Next

            Dim which As Integer = ParseKwType(statType, defaultTieCorrected:=True, err:=err)
            If err.HasValue Then Return err.Value

            Dim test = New KruskallWalis(data, names)
            Dim res = test.compute()

            If which = 0 Then
                Return res.TestStatistics1   ' H
            Else
                Return res.TestStatistics2   ' Hcor
            End If
        End Function

        ''' <summary>
        ''' Kruskal-Wallis p-value for comparing 2 or more independent groups.
        ''' </summary>
        ''' <param name="groups">
        ''' Input data arranged as one column per group (a multi-column Excel range).
        ''' Each column represents an independent group of observations.
        ''' Non-numeric cells (empty, text, logical, error) are ignored within each column.
        ''' </param>
        ''' <param name="pType">
        ''' Select which p-value to return:
        ''' <list type="bullet">
        ''' <item><description><c>"H"</c> - p-value based on the uncorrected H statistic</description></item>
        ''' <item><description><c>"Hcor"</c> - p-value based on the tie-corrected H<sub>cor</sub> statistic</description></item>
        ''' </list>
        ''' The comparison is case-insensitive. If omitted, <c>"Hcor"</c> is used.
        ''' </param>
        ''' <returns>
        ''' A p-value in the range [0, 1]. Returns an Excel error code if inputs are invalid.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The p-value is obtained from the chi-square distribution with <c>k-1</c> degrees of freedom,
        ''' where <c>k</c> is the number of non-empty groups (columns).
        ''' </para>
        ''' <para>
        ''' When there are tied values, the tie-corrected p-value (based on H<sub>cor</sub>) is recommended.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.KW_P(A2:C20, "Hcor")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.KW_P",
            Category:="BESHStatNG - Nonparametric",
            Description:="P-value for Kruskal-Wallis test (based on H or tie-corrected Hcor) for 2+ independent groups.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/"
        )>
        Public Function KW_P(
            <ExcelArgument(Name:="groups", Description:="Multi-column range: one column per group (independent samples).")> groups As Object,
            <ExcelArgument(Name:="type", Description:="""H"" for p-value from H; ""Hcor"" for p-value from Hcor (default).")> Optional pType As Object = Nothing
        ) As Object

            Dim err? As ExcelError = Nothing
            Dim data = UDFhelpers.ExtractNumericGroupsFromColumnsIgnoringNonNumeric(groups, err)
            If err.HasValue Then Return err.Value
            If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum

            Dim names(data.Length - 1) As String
            For i As Integer = 0 To data.Length - 1
                names(i) = "G" & (i + 1).ToString()
            Next

            Dim which As Integer = ParseKwType(pType, defaultTieCorrected:=True, err:=err)
            If err.HasValue Then Return err.Value

            Dim test = New KruskallWalis(data, names)
            Dim res = test.compute()

            Dim p As Double = If(which = 0, res.Pvalue, res.Pvalue2)
            Return ClampProb(p)
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Friedman test (nonparametric repeated-measures / randomized-blocks ANOVA)
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Returns the Friedman test statistic for repeated-measures / blocked designs (k related samples).
        ''' </summary>
        ''' <param name="data">
        ''' A multi-column range where each column is a treatment/condition and each row is a block/subject.
        ''' <para>
        ''' The test is computed on complete blocks only: rows with any non-numeric or missing value in any column
        ''' are ignored so that the remaining rows contain paired observations across all treatments.
        ''' </para>
        ''' </param>
        ''' <param name="statType">
        ''' Selects which Friedman statistic to return:
        ''' <list type="bullet">
        ''' <item><description><c>"T1"</c> (or <c>"CHI"</c>): chi-square approximation statistic (classic Friedman χ²).</description></item>
        ''' <item><description><c>"T2"</c> (or <c>"F"</c>): Iman–Davenport F-approximation statistic (often better for small samples).</description></item>
        ''' </list>
        ''' If omitted or empty, <c>"T1"</c> is used.
        ''' </param>
        ''' <returns>
        ''' The requested Friedman statistic (T1 or T2).
        ''' Returns <c>#VALUE!</c> if <paramref name="data"/> is not a 2+ column range.
        ''' Returns <c>#NUM!</c> if there are fewer than 2 complete blocks after filtering.
        ''' </returns>
        ''' <remarks>
        ''' Use this test when the same subjects (or blocks) are measured under <c>k</c> different conditions
        ''' and you want a distribution-free alternative to repeated-measures ANOVA.
        ''' <para>
        ''' Interpretation: larger statistics indicate stronger evidence that at least one condition tends to produce
        ''' systematically different values (in terms of ranks) compared with the others.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' ' Data layout: columns = conditions A..C, rows = subjects
        ''' =BESH.NP.FRIEDMAN_STAT(A2:C21,"T1")
        ''' =BESH.NP.FRIEDMAN_STAT(A2:C21,"F")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.FRIEDMAN_STAT",
            Category:="BESHStatNG - Nonparametric",
            Description:="Friedman test statistic for repeated-measures/blocked designs (T1 chi-square or T2 F-approximation).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/udf/nonparametric/"
        )>
        Public Function FRIEDMAN_STAT(
            <ExcelArgument(Name:="data", Description:="Multi-column range: columns=treatments, rows=blocks/subjects (complete rows only).")> data As Object,
            <ExcelArgument(Name:="statType", Description:="Statistic type: ""T1""/""CHI"" (chi-square) or ""T2""/""F"" (F-approx). Default ""T1"".")> Optional statType As Object = Nothing
        ) As Object

            Try
                Dim mat As Double(,) = Nothing
                Dim k As Integer = 0, b As Integer = 0
                If Not UDFhelpers.ExtractCompleteNumericMatrixCompleteCases(data, mat, b, k) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If b < 2 OrElse k < 2 Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim names(k - 1) As String
                For j = 0 To k - 1
                    names(j) = $"G{j + 1}"
                Next

                Dim test = New Friedman(mat, names)
                Dim res = test.compute()

                Dim which As Integer = ParseFriedmanType(statType, defaultToT1:=True)
                If which = 2 Then
                    Return res.TestStatistics2
                End If
                Return res.TestStatistics1

            Catch
                Return ExcelError.ExcelErrorValue
            End Try

        End Function

        ''' <summary>
        ''' Returns the p-value for the Friedman test for repeated-measures / blocked designs (k related samples).
        ''' </summary>
        ''' <param name="data">
        ''' A multi-column range where each column is a treatment/condition and each row is a block/subject.
        ''' <para>
        ''' The p-value is computed on complete blocks only: rows with any non-numeric or missing value in any column
        ''' are ignored so that the remaining rows contain paired observations across all treatments.
        ''' </para>
        ''' </param>
        ''' <param name="pType">
        ''' Selects which p-value approximation to return:
        ''' <list type="bullet">
        ''' <item><description><c>"T1"</c> (or <c>"CHI"</c>): p-value from the chi-square approximation (df = k − 1).</description></item>
        ''' <item><description><c>"T2"</c> (or <c>"F"</c>): p-value from the Iman–Davenport F-approximation.</description></item>
        ''' </list>
        ''' If omitted or empty, <c>"T1"</c> is used.
        ''' </param>
        ''' <returns>
        ''' The requested p-value in the range [0, 1].
        ''' Returns <c>#VALUE!</c> if <paramref name="data"/> is not a 2+ column range.
        ''' Returns <c>#NUM!</c> if there are fewer than 2 complete blocks after filtering.
        ''' </returns>
        ''' <remarks>
        ''' The null hypothesis is that all <c>k</c> conditions have the same distribution (no systematic rank differences).
        ''' A small p-value indicates evidence that at least one condition tends to be higher/lower than another.
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.NP.FRIEDMAN_P(A2:C21,"T1")
        ''' =BESH.NP.FRIEDMAN_P(A2:C21,"F")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.NP.FRIEDMAN_P",
            Category:="BESHStatNG - Nonparametric",
            Description:="Friedman test p-value for repeated-measures/blocked designs (chi-square or F-approximation).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/udf/nonparametric/"
        )>
        Public Function FRIEDMAN_P(
            <ExcelArgument(Name:="data", Description:="Multi-column range: columns=treatments, rows=blocks/subjects (complete rows only).")> data As Object,
            <ExcelArgument(Name:="pType", Description:="P-value type: ""T1""/""CHI"" (chi-square) or ""T2""/""F"" (F-approx). Default ""T1"".")> Optional pType As Object = Nothing
        ) As Object

            Try
                Dim mat As Double(,) = Nothing
                Dim k As Integer = 0, b As Integer = 0
                If Not UDFhelpers.ExtractCompleteNumericMatrixCompleteCases(data, mat, b, k) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If b < 2 OrElse k < 2 Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim names(k - 1) As String
                For j = 0 To k - 1
                    names(j) = $"G{j + 1}"
                Next

                Dim test = New Friedman(mat, names)
                Dim res = test.compute()

                Dim which As Integer = ParseFriedmanType(pType, defaultToT1:=True)
                If which = 2 Then
                    Dim pv As Double = res.Pvalue2
                    If Double.IsNaN(pv) OrElse Double.IsInfinity(pv) Then Return ExcelError.ExcelErrorNum
                    Return pv
                End If

                Dim pv1 As Double = res.Pvalue
                If Double.IsNaN(pv1) OrElse Double.IsInfinity(pv1) Then Return ExcelError.ExcelErrorNum
                Return pv1

            Catch
                Return ExcelError.ExcelErrorValue
            End Try

        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Kruskal-Wallis multiple comparisons
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Dunn's post-hoc multiple comparisons following a Kruskal-Wallis test.
        ''' </summary>
        ''' <param name="groups">
        ''' Multi-column Excel range where each column represents one independent group.
        ''' Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
        ''' If the first row contains non-numeric labels, it is treated as a header row and used as group names.
        ''' </param>
        ''' <param name="groupNames">
        ''' Optional group names supplied as a comma-separated string or as a one-row/one-column range.
        ''' When omitted, names are taken from the first row of <paramref name="groups"/> when it looks like a header;
        ''' otherwise default names such as Group 1, Group 2, … are used.
        ''' </param>
        ''' <param name="alpha">
        ''' Reserved for API consistency with other MCP UDFs.
        ''' The current Dunn implementation reports adjusted p-values only and does not compute confidence intervals.
        ''' </param>
        ''' <returns>
        ''' A labeled Dunn multiple-comparison table as a dynamic array.
        ''' Returns <c>#VALUE!</c> for invalid input.
        ''' Returns <c>#NUM!</c> for invalid alpha or too few groups.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.NP.KW_MCP",
            Category:="BESHStatNG - Nonparametric",
            Description:="Kruskal-Wallis post-hoc multiple comparisons (Dunn test).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/")>
        Public Function KW_MCP(
            <ExcelArgument(Name:="groups", Description:="Multi-column range; one column per group. First row may contain headers.")> groups As Object,
            <ExcelArgument(Name:="groupNames", Description:="Optional group names as comma-separated text or 1-row/1-column range.")> Optional groupNames As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha parameter reserved for API consistency; currently no CI is reported.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not UDFhelpers.TryReadGroupedNumericColumns(groups, data, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(groupNames, detectedNames, data.Length, "Group")

                Dim alphaValue As Double = 0.05
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim test = New KruskallWalis(data, names)
                test.compute()
                test.MCP(alphaValue)

                Dim tables = test.wrapResults()
                Return PrepareResultTableForUdf(tables(1).returnSelf())
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Friedman multiple comparisons
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Post-hoc multiple comparisons following a Friedman test.
        ''' </summary>
        ''' <param name="data">
        ''' Numeric matrix where rows are blocks/subjects and columns are treatments/conditions.
        ''' Rows containing any missing or non-numeric value are excluded so that the remaining matrix is complete.
        ''' If the first row contains non-numeric labels, it is treated as a header row and used as condition names.
        ''' </param>
        ''' <param name="conditionNames">
        ''' Optional condition names supplied as a comma-separated string or as a one-row/one-column range.
        ''' </param>
        ''' <param name="method">
        ''' Post-hoc method to return:
        ''' <list type="bullet">
        ''' <item><description><c>"dunn"</c> / <c>"spss"</c> (default)</description></item>
        ''' <item><description><c>"conover"</c></description></item>
        ''' <item><description><c>"all"</c> — stack both MCP tables</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="alpha">
        ''' Reserved for API consistency with other MCP UDFs.
        ''' The current Friedman MCP implementation reports adjusted p-values only and does not compute confidence intervals.
        ''' </param>
        ''' <returns>
        ''' A labeled multiple-comparison table as a dynamic array.
        ''' Returns <c>#VALUE!</c> for invalid input or unknown method.
        ''' Returns <c>#NUM!</c> for invalid alpha or too few complete rows/conditions.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.NP.FRIEDMAN_MCP",
            Category:="BESHStatNG - Nonparametric",
            Description:="Friedman post-hoc multiple comparisons: Dunn (default) or Conover.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/nonparametric/")>
        Public Function FRIEDMAN_MCP(
            <ExcelArgument(Name:="data", Description:="Numeric matrix; rows=blocks/subjects, columns=conditions. First row may contain headers.")> data As Object,
            <ExcelArgument(Name:="conditionNames", Description:="Optional condition names as comma-separated text or 1-row/1-column range.")> Optional conditionNames As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional: dunn/spss (default), conover, or all.")> Optional method As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha parameter reserved for API consistency; currently no CI is reported.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not UDFhelpers.TryReadCompleteNumericMatrixWithHeaders(data, mat, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If mat.GetLength(0) < 2 OrElse mat.GetLength(1) < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(conditionNames, detectedNames, mat.GetLength(1), "Condition")

                Dim alphaValue As Double = 0.05
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim which As String = NormalizeText(method)
                Dim test = New Friedman(mat, names)
                test.compute()
                test.MCP(alphaValue)

                Dim tables = test.wrapResults()

                Select Case which
                    Case "", "DUNN", "SPSS"
                        Return PrepareResultTableForUdf(tables(3).returnSelf())

                    Case "CONOVER", "CON"
                        Return PrepareResultTableForUdf(tables(2).returnSelf())

                    Case "ALL", "BOTH"
                        Dim stacked As Object(,) = TryCast(tables(2).returnSelf(), Object(,))
                        stacked = ParametricUDFs.StackWithBlankRow(stacked, TryCast(tables(3).returnSelf(), Object(,)))
                        Return PrepareResultTableForUdf(stacked)

                    Case Else
                        Return ExcelError.ExcelErrorValue
                End Select
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Helpers
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Parses the Kruskal-Wallis output selection (H vs Hcor).
        ''' Returns 0 for H, 1 for Hcor.
        ''' </summary>
        Private Function ParseKwType(arg As Object, defaultTieCorrected As Boolean, ByRef err As ExcelError?) As Integer
            err = Nothing

            If arg Is Nothing OrElse TypeOf arg Is ExcelMissing OrElse TypeOf arg Is ExcelEmpty Then
                Return If(defaultTieCorrected, 1, 0)
            End If

            If TypeOf arg Is ExcelError Then
                err = DirectCast(arg, ExcelError)
                Return 0
            End If

            Dim s As String = Nothing
            Try
                s = Convert.ToString(arg)
            Catch
                s = Nothing
            End Try

            If String.IsNullOrWhiteSpace(s) Then
                Return If(defaultTieCorrected, 1, 0)
            End If

            s = s.Trim().ToUpperInvariant()
            If s = "H" OrElse s = "UNCOR" OrElse s = "UNCORRECTED" OrElse s = "RAW" Then
                Return 0
            End If
            If s = "HCOR" OrElse s = "H_COR" OrElse s = "TIE" OrElse s = "TIES" OrElse s = "COR" OrElse s = "CORR" OrElse s = "CORRECTED" Then
                Return 1
            End If

            err = ExcelError.ExcelErrorValue
            Return 0
        End Function

        ' Helper: parse Friedman statistic/p-value selector. Returns 1 for T1 (chi-square), 2 for T2 (F-approx).
        Private Function ParseFriedmanType(selector As Object, Optional defaultToT1 As Boolean = True) As Integer
            Dim s As String = ""
            If selector IsNot Nothing AndAlso Not TypeOf selector Is ExcelMissing AndAlso Not TypeOf selector Is ExcelEmpty Then
                s = Convert.ToString(selector).Trim().ToUpperInvariant()
            End If

            If String.IsNullOrWhiteSpace(s) Then
                Return If(defaultToT1, 1, 2)
            End If

            If s = "T2" OrElse s = "F" OrElse s = "FAPPROX" OrElse s = "IMAN" OrElse s = "IMAN-DAVENPORT" OrElse s = "DAVENPORT" Then
                Return 2
            End If

            ' default to T1
            Return 1
        End Function

    End Module
End Namespace