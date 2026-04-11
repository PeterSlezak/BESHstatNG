Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports ExcelDna.Integration
Imports BESHStatNG.assumptions

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions exposing assumption and diagnostic tests used across the add-in.
    ''' </summary>
    Public Module AssumptionsUDFs

        ' -------------------------------------------------------------------------------------------------------------
        ' Normality tests
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Shapiro–Wilk test for assessing univariate normality.
        ''' </summary>
        ''' <param name="data">
        ''' A single-column Excel range containing the sample values.
        ''' Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
        ''' it is treated as a header and excluded from the calculation.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the Shapiro–Wilk W statistic and the corresponding two-sided p-value.
        ''' Returns <c>#VALUE!</c> if the input is not a single-column range.
        ''' Returns <c>#NUM!</c> if fewer than 3 usable observations remain, more than 5000 usable observations are supplied,
        ''' or the test cannot be evaluated because the data have zero range.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The Shapiro–Wilk test is one of the most widely used tests of normality for small to moderate sample sizes.
        ''' It compares the ordered sample values with the corresponding expected order statistics under a normal distribution.
        ''' </para>
        ''' <para>
        ''' Small p-values indicate evidence against the assumption of normality.
        ''' The implementation is intended for sample sizes from 3 to 5000 usable observations.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.SHAPIRO_WILK(A2:A51)
        ''' =BESH.ASM.SHAPIRO_WILK(A1:A51)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.SHAPIRO_WILK",
            Category:="BESHStatNG - Assumptions",
            Description:="Shapiro-Wilk normality test for a single sample.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function SHAPIRO_WILK(
            <ExcelArgument(Name:="data", Description:="Sample values in one column. Non-numeric cells ignored; first cell may be a header.")> data As Object
        ) As Object
            Try
                Dim x() As Double = Nothing
                Dim detectedName As String = Nothing
                If Not TryReadSingleNumericColumn(data, x, detectedName) Then Return ExcelError.ExcelErrorValue
                If x Is Nothing OrElse x.Length < 3 OrElse x.Length > 5000 Then Return ExcelError.ExcelErrorNum

                Dim errText As String = String.Empty
                Dim res As TestResult = Assumptions.ShapiroWilk(x, errText)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildTwoValueTable("Shapiro-Wilk Test", "W statistic", res.TestStatistics1, "Two-sided p-value", res.Pvalue)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.SHAPIRO_WILK", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' D'Agostino–Pearson omnibus normality test based on skewness and kurtosis.
        ''' </summary>
        ''' <param name="data">
        ''' A single-column Excel range containing the sample values.
        ''' Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
        ''' it is treated as a header and excluded from the calculation.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the K² statistic and its two-sided p-value.
        ''' Returns <c>#VALUE!</c> if the input is not a single-column range.
        ''' Returns <c>#NUM!</c> if fewer than 9 usable observations remain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This omnibus normality test combines evidence from sample skewness and sample kurtosis.
        ''' It is useful when departures from normality may arise through asymmetry, heavy tails, or both.
        ''' </para>
        ''' <para>
        ''' The reported statistic follows an approximate chi-square distribution with 2 degrees of freedom under the null hypothesis of normality.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.DAGOSTINO_PEARSON(A2:A100)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.DAGOSTINO_PEARSON",
            Category:="BESHStatNG - Assumptions",
            Description:="D'Agostino-Pearson K² normality test for a single sample.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function DAGOSTINO_PEARSON(
            <ExcelArgument(Name:="data", Description:="Sample values in one column. Non-numeric cells ignored; first cell may be a header.")> data As Object
        ) As Object
            Try
                Dim x() As Double = Nothing
                Dim detectedName As String = Nothing
                If Not TryReadSingleNumericColumn(data, x, detectedName) Then Return ExcelError.ExcelErrorValue
                If x Is Nothing OrElse x.Length < 9 Then Return ExcelError.ExcelErrorNum

                Dim errText As String = String.Empty
                Dim res As TestResult = Assumptions.DAgostino(x, errText)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildTwoValueTable("D'Agostino-Pearson K² Test", "K² statistic", res.TestStatistics1, "Two-sided p-value", res.Pvalue)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.DAGOSTINO_PEARSON", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Anderson–Darling normality test for a single sample.
        ''' </summary>
        ''' <param name="data">
        ''' A single-column Excel range containing the sample values.
        ''' Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
        ''' it is treated as a header and excluded from the calculation.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the adjusted Anderson–Darling statistic and its approximate p-value.
        ''' Returns <c>#VALUE!</c> if the input is not a single-column range.
        ''' Returns <c>#NUM!</c> if fewer than 2 usable observations remain or if the statistic cannot be computed.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The Anderson–Darling test compares the empirical distribution of the sample with the fitted normal distribution.
        ''' Relative to some other normality tests, it is especially sensitive to discrepancies in the tails.
        ''' </para>
        ''' <para>
        ''' The reported p-value is an approximation based on the adjusted statistic.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.ANDERSON_DARLING(A2:A100)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.ANDERSON_DARLING",
            Category:="BESHStatNG - Assumptions",
            Description:="Anderson-Darling normality test for a single sample.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function ANDERSON_DARLING(
            <ExcelArgument(Name:="data", Description:="Sample values in one column. Non-numeric cells ignored; first cell may be a header.")> data As Object
        ) As Object
            Try
                Dim x() As Double = Nothing
                Dim detectedName As String = Nothing
                If Not TryReadSingleNumericColumn(data, x, detectedName) Then Return ExcelError.ExcelErrorValue
                If x Is Nothing OrElse x.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim res As TestResult = Assumptions.AndersonDarlingTEST(x)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildTwoValueTable("Anderson-Darling Test", "Adjusted AD²", res.TestStatistics1, "Approximate p-value", res.Pvalue)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.ANDERSON_DARLING", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Homogeneity of variances and covariance matrices
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Box's M test for equality of covariance matrices across two or more groups.
        ''' </summary>
        ''' <param name="data">
        ''' Numeric data matrix with one row per observation and one column per variable.
        ''' The first row may contain variable headers. Rows with any non-numeric value are excluded.
        ''' </param>
        ''' <param name="groups">
        ''' A single-column range of group labels aligned row-for-row with <paramref name="data"/>.
        ''' Labels may be text or numbers. An optional header cell may be included above the labels
        ''' and is excluded automatically when present.
        ''' Blank labels are ignored.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing Box's M statistic and its p-value.
        ''' Returns <c>#VALUE!</c> if the inputs are not a valid matrix-plus-group specification.
        ''' Returns <c>#NUM!</c> if fewer than two groups remain or if any retained group has fewer than two complete observations.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Box's M test evaluates whether the within-group covariance matrices are equal across groups.
        ''' It is commonly used as an assumption check before multivariate procedures such as Hotelling's two-sample test or MANOVA.
        ''' </para>
        ''' <para>
        ''' Rows are filtered jointly: a row is used only when it has a nonblank group label and complete numeric data across all variables.
        ''' </para>
        ''' <para>
        ''' Each group must contain enough complete observations to produce a non-singular covariance matrix. In practice, the number 
        ''' of complete observations in every group should exceed the number of variables. If any within-group covariance matrix is 
        ''' singular, the function returns <c>#NUM!</c>.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.BOX_M(A1:C101, D1:D101)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.BOX_M",
            Category:="BESHStatNG - Assumptions",
            Description:="Box's M test for equality of covariance matrices across groups.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function BOX_M(
            <ExcelArgument(Name:="data", Description:="Numeric data matrix. Rows are observations; columns are variables. First row may be headers.")> data As Object,
            <ExcelArgument(Name:="groups", Description:="Single-column group labels aligned with the rows of the data matrix.")> groups As Object
        ) As Object
            Try
                Dim groupMats As List(Of Double(,)) = Nothing
                Dim groupNames As List(Of String) = Nothing
                If Not TryReadGroupedCompleteMatrix(data, groups, groupMats, groupNames) Then Return ExcelError.ExcelErrorValue
                If groupMats Is Nothing OrElse groupMats.Count < 2 Then Return ExcelError.ExcelErrorNum

                Dim g As Integer = groupMats.Count
                Dim p As Integer = groupMats(0).GetLength(1)
                Dim covCube(g - 1, p - 1, p - 1) As Double
                Dim sampleSizes(g - 1) As Integer

                For i As Integer = 0 To g - 1
                    Dim mat = groupMats(i)
                    Dim n As Integer = mat.GetLength(0)
                    If n < 2 Then Return ExcelError.ExcelErrorNum
                    sampleSizes(i) = n
                    Dim cov = SampleCovariance(mat)
                    For r As Integer = 0 To p - 1
                        For c As Integer = 0 To p - 1
                            covCube(i, r, c) = cov(r, c)
                        Next
                    Next
                Next

                Dim res As TestResult = Assumptions.BoxM(covCube, sampleSizes)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildTwoValueTable("Box's Test of Equality of Covariance Matrices", "M statistic", res.TestStatistics1, "P-value", res.Pvalue)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.BOX_M", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Fligner–Killeen test for equality of variances across grouped samples.
        ''' </summary>
        ''' <param name="groups">
        ''' Multi-column Excel range where each column represents one group.
        ''' Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
        ''' If the first row contains non-numeric labels, it is treated as a header row and excluded.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the chi-square statistic and p-value.
        ''' Returns <c>#VALUE!</c> if the input is not a valid grouped range.
        ''' Returns <c>#NUM!</c> if fewer than two non-empty groups remain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The Fligner–Killeen test is a robust, rank-based procedure for comparing group variances.
        ''' It is often preferred when the normality assumption is doubtful.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.FLIGNER_KILLEEN(A1:C25)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.FLIGNER_KILLEEN",
            Category:="BESHStatNG - Assumptions",
            Description:="Fligner-Killeen test for homogeneity of variances across groups.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function FLIGNER_KILLEEN(
            <ExcelArgument(Name:="groups", Description:="Multi-column grouped data. One column per group; first row may be headers.")> groups As Object
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadGroupedNumericColumns(groups, data, detectedNames) Then Return ExcelError.ExcelErrorValue
                If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim res As TestResult = Assumptions.FlignerKilleenTEST(data)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildTwoValueTable("Fligner-Killeen Test", "Chi-square statistic", res.TestStatistics1, "Two-sided p-value", res.Pvalue)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.FLIGNER_KILLEEN", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Levene's test or Brown–Forsythe modification for equality of variances across grouped samples.
        ''' </summary>
        ''' <param name="groups">
        ''' Multi-column Excel range where each column represents one group.
        ''' Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
        ''' If the first row contains non-numeric labels, it is treated as a header row and excluded.
        ''' </param>
        ''' <param name="center">
        ''' Optional centering rule:
        ''' <list type="bullet">
        ''' <item><description><c>"mean"</c> or <c>"levene"</c> — classical Levene test (default)</description></item>
        ''' <item><description><c>"median"</c>, <c>"brown-forsythe"</c>, or <c>"bf"</c> — Brown–Forsythe modification</description></item>
        ''' </list>
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the F statistic and p-value.
        ''' Returns <c>#VALUE!</c> if the input is not a valid grouped range or if <paramref name="center"/> is not recognized.
        ''' Returns <c>#NUM!</c> if fewer than two non-empty groups remain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Classical Levene's test centers observations around the group mean.
        ''' The Brown–Forsythe variant centers them around the group median and is more robust when group distributions are skewed or heavy-tailed.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.LEVENE(A1:C25)
        ''' =BESH.ASM.LEVENE(A1:C25, "brown-forsythe")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.LEVENE",
            Category:="BESHStatNG - Assumptions",
            Description:="Levene or Brown-Forsythe test for homogeneity of variances across groups.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function LEVENE(
            <ExcelArgument(Name:="groups", Description:="Multi-column grouped data. One column per group; first row may be headers.")> groups As Object,
            <ExcelArgument(Name:="center", Description:="Optional: mean/levene (default) or median/brown-forsythe.")> Optional center As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadGroupedNumericColumns(groups, data, detectedNames) Then Return ExcelError.ExcelErrorValue
                If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim useMedian As Boolean = False
                Dim label As String = Nothing
                If Not TryParseLeveneCenter(center, useMedian, label) Then Return ExcelError.ExcelErrorValue

                Dim res As TestResult = Assumptions.LeveneTEST(data, useMedian)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildTwoValueTable(label, "F statistic", res.TestStatistics1, "P-value", res.Pvalue)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.LEVENE", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Bartlett's test for equality of variances across grouped samples.
        ''' </summary>
        ''' <param name="groups">
        ''' Multi-column Excel range where each column represents one group.
        ''' Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
        ''' If the first row contains non-numeric labels, it is treated as a header row and excluded.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the chi-square statistic and p-value.
        ''' Returns <c>#VALUE!</c> if the input is not a valid grouped range.
        ''' Returns <c>#NUM!</c> if fewer than two usable groups remain or if any retained group has fewer than two observations.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Bartlett's test is powerful under normality but can be sensitive to non-normal data.
        ''' When normality is doubtful, more robust alternatives such as Fligner–Killeen or the Brown–Forsythe variant of Levene's test may be preferable.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.BARTLETT(A1:C25)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.BARTLETT",
            Category:="BESHStatNG - Assumptions",
            Description:="Bartlett test for homogeneity of variances across groups.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function BARTLETT(
            <ExcelArgument(Name:="groups", Description:="Multi-column grouped data. One column per group; first row may be headers.")> groups As Object
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadGroupedNumericColumns(groups, data, detectedNames) Then Return ExcelError.ExcelErrorValue
                If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum
                For Each g In data
                    If g Is Nothing OrElse g.Length < 2 Then Return ExcelError.ExcelErrorNum
                Next

                Dim res As TestResult = Assumptions.BartlettTEST(data)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildTwoValueTable("Bartlett Test", "Chi-square statistic", res.TestStatistics1, "P-value", res.Pvalue)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.BARTLETT", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Squared-ranks test for equality of variances across grouped samples.
        ''' </summary>
        ''' <param name="groups">
        ''' Multi-column Excel range where each column represents one group.
        ''' Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
        ''' If the first row contains non-numeric labels, it is treated as a header row and excluded.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the chi-square statistic and p-value.
        ''' Returns <c>#VALUE!</c> if the input is not a valid grouped range.
        ''' Returns <c>#NUM!</c> if fewer than two non-empty groups remain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The squared-ranks test is a nonparametric procedure for comparing variability across groups.
        ''' It is based on ranks of absolute deviations and provides a robust alternative when normality is questionable.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.SQUARED_RANKS(A1:C25)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.SQUARED_RANKS",
            Category:="BESHStatNG - Assumptions",
            Description:="Squared-ranks test for homogeneity of variances across groups.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function SQUARED_RANKS(
            <ExcelArgument(Name:="groups", Description:="Multi-column grouped data. One column per group; first row may be headers.")> groups As Object
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadGroupedNumericColumns(groups, data, detectedNames) Then Return ExcelError.ExcelErrorValue
                If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim res As TestResult = Assumptions.SquaredRanksTestVARIANCE(data)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildTwoValueTable("Squared Ranks Test", "Chi-square statistic", res.TestStatistics1, "P-value", res.Pvalue)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.SQUARED_RANKS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Repeated-measures and symmetry diagnostics
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Mauchly's test of sphericity for repeated-measures data.
        ''' </summary>
        ''' <param name="data">
        ''' Numeric matrix where rows are subjects and columns are repeated-measure conditions.
        ''' Rows containing any missing or non-numeric value are excluded so that the retained matrix is complete.
        ''' If the first row contains non-numeric labels, it is treated as a header row and excluded.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the chi-square statistic and p-value for the sphericity test.
        ''' Returns <c>#VALUE!</c> if the input is not a valid numeric matrix.
        ''' Returns <c>#NUM!</c> if too few complete rows remain or if fewer than three conditions are supplied.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Mauchly's test assesses the sphericity assumption used by classical repeated-measures ANOVA.
        ''' A small p-value indicates evidence that the covariance structure departs from sphericity,
        ''' in which case corrections such as Greenhouse–Geisser or Huynh–Feldt are commonly considered.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.MAUCHLY(A1:D25)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.MAUCHLY",
            Category:="BESHStatNG - Assumptions",
            Description:="Mauchly's test of sphericity for repeated-measures data.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function MAUCHLY(
            <ExcelArgument(Name:="data", Description:="Complete repeated-measures matrix. Rows are subjects; columns are conditions. First row may be headers.")> data As Object
        ) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim names() As String = Nothing
                If Not UDFhelpers.TryReadCompleteNumericMatrixWithHeaders(data, mat, names) Then Return ExcelError.ExcelErrorValue
                If mat Is Nothing OrElse mat.GetLength(0) < 2 OrElse mat.GetLength(1) < 3 Then Return ExcelError.ExcelErrorNum

                Dim res As TestResult = Assumptions.MauchlyTest(mat)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildTwoValueTable("Mauchly's Test of Sphericity", "Chi-square statistic", res.TestStatistics1, "P-value", res.Pvalue)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.MAUCHLY", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Symmetry test about an unknown median for a single sample.
        ''' </summary>
        ''' <param name="data">
        ''' A single-column Excel range containing the sample values.
        ''' Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
        ''' it is treated as a header and excluded from the calculation.
        ''' </param>
        ''' <param name="method">
        ''' Optional symmetry test to use:
        ''' <list type="bullet">
        ''' <item><description><c>"mgg"</c>, <c>"miao-gel-gastwirth"</c> — Miao–Gel–Gastwirth test (default)</description></item>
        ''' <item><description><c>"cm"</c> or <c>"cabilio-masaro"</c> — Cabilio–Masaro test</description></item>
        ''' </list>
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the test statistic and two-sided p-value.
        ''' Returns <c>#VALUE!</c> if the input is not a single-column range or if <paramref name="method"/> is not recognized.
        ''' Returns <c>#NUM!</c> if fewer than two usable observations remain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' These tests assess whether the sample distribution is symmetric around an unknown median.
        ''' The Miao–Gel–Gastwirth option uses a robust scale estimate, while the Cabilio–Masaro option is based on the difference between the mean and the median.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.SYMMETRY(A2:A51)
        ''' =BESH.ASM.SYMMETRY(A2:A51, "cm")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.SYMMETRY",
            Category:="BESHStatNG - Assumptions",
            Description:="Symmetry test about an unknown median: MGG (default) or Cabilio-Masaro.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function SYMMETRY(
            <ExcelArgument(Name:="data", Description:="Sample values in one column. Non-numeric cells ignored; first cell may be a header.")> data As Object,
            <ExcelArgument(Name:="method", Description:="Optional symmetry test: mgg (default) or cm.")> Optional method As Object = Nothing
        ) As Object
            Try
                Dim x() As Double = Nothing
                Dim detectedName As String = Nothing
                If Not TryReadSingleNumericColumn(data, x, detectedName) Then Return ExcelError.ExcelErrorValue
                If x Is Nothing OrElse x.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim methodInternal As String = Nothing
                Dim title As String = Nothing
                If Not TryParseSymmetryMethod(method, methodInternal, title) Then Return ExcelError.ExcelErrorValue

                Dim res As TestResult = Assumptions.SymmetryTest(x, methodInternal)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildTwoValueTable(title, "Test statistic", res.TestStatistics1, "Two-sided p-value", res.Pvalue)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.SYMMETRY", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Outlier diagnostics
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Grubbs' test for detecting a single outlying observation in a univariate sample.
        ''' </summary>
        ''' <param name="data">
        ''' A single-column Excel range containing the sample values.
        ''' Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
        ''' it is treated as a header and excluded from the calculation.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional significance level in the open interval <c>(0,1)</c>.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing alpha, the critical statistic, the observed statistic, and a textual conclusion.
        ''' Returns <c>#VALUE!</c> if the input is not a single-column range.
        ''' Returns <c>#NUM!</c> if too few usable observations remain or if <paramref name="alpha"/> is invalid.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Grubbs' test evaluates whether the most extreme observation is inconsistent with the remainder of the sample under approximate normality.
        ''' It is intended for detecting at most one outlier at a time.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.GRUBBS(A2:A30)
        ''' =BESH.ASM.GRUBBS(A2:A30, 0.01)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.GRUBBS",
            Category:="BESHStatNG - Assumptions",
            Description:="Grubbs test for a single outlier in a univariate sample.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function GRUBBS(
            <ExcelArgument(Name:="data", Description:="Sample values in one column. Non-numeric cells ignored; first cell may be a header.")> data As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional significance level. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim x() As Double = Nothing
                Dim detectedName As String = Nothing
                If Not TryReadSingleNumericColumn(data, x, detectedName) Then Return ExcelError.ExcelErrorValue
                If x Is Nothing OrElse x.Length < 3 Then Return ExcelError.ExcelErrorNum

                Dim alphaValue As Double = 0.05
                If Not ParametricUDFs.TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim res As TestResult = Assumptions.Grubbs(x, alphaValue)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.TestStatistics2) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Return BuildGrubbsTable(alphaValue, res)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.GRUBBS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Rosner generalized ESD test for detecting multiple outliers in a univariate sample.
        ''' </summary>
        ''' <param name="data">
        ''' A single-column Excel range containing the sample values.
        ''' Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
        ''' it is treated as a header and excluded from the calculation.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional significance level in the open interval <c>(0,1)</c>.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing alpha, the number of detected outliers, and the detected outlying values.
        ''' Returns <c>#VALUE!</c> if the input is not a single-column range.
        ''' Returns <c>#NUM!</c> if fewer than 15 usable observations remain or if <paramref name="alpha"/> is invalid.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Rosner's generalized ESD procedure iteratively checks for up to ten potential outliers and determines how many of the most extreme values should be flagged.
        ''' It is intended for larger samples than Grubbs' test.
        ''' </para>
        ''' <para>
        ''' For sample sizes below 25, the result can still be computed but should be interpreted with caution.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.ASM.ROSNER(A2:A50)
        ''' =BESH.ASM.ROSNER(A2:A50, 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.ASM.ROSNER",
            Category:="BESHStatNG - Assumptions",
            Description:="Rosner generalized ESD test for multiple outliers in a univariate sample.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/assumptions/")>
        Public Function ROSNER(
            <ExcelArgument(Name:="data", Description:="Sample values in one column. Non-numeric cells ignored; first cell may be a header.")> data As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional significance level. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim x() As Double = Nothing
                Dim detectedName As String = Nothing
                If Not TryReadSingleNumericColumn(data, x, detectedName) Then Return ExcelError.ExcelErrorValue
                If x Is Nothing OrElse x.Length < 15 Then Return ExcelError.ExcelErrorNum

                Dim alphaValue As Double = 0.05
                If Not ParametricUDFs.TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim outliers() As Double = Assumptions.Rosner(x, alphaValue)
                Return BuildRosnerTable(alphaValue, outliers, x.Length < 25)
            Catch ex As Exception
                Return LoggedUdfError("BESH.ASSUMP.ROSNER", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Local helpers
        ' -------------------------------------------------------------------------------------------------------------

        Private Function BuildTwoValueTable(title As String, label1 As String, value1 As Object, label2 As String, value2 As Object) As Object(,)
            Dim t As New ResultTable
            t.SetBody(New Object(,) {{label1, value1}, {label2, value2}})
            t.AddHeaderTopRow({title, ""})
            Return ParametricUDFs.PrepareResultTableForUdf(t.returnSelf())
        End Function

        Private Function BuildGrubbsTable(alpha As Double, res As TestResult) As Object(,)
            Dim t As New ResultTable
            t.SetBody(New Object(,) {
                {"Alpha", alpha},
                {"Critical statistic", res.TestStatistics1},
                {"Observed statistic", res.TestStatistics2},
                {"Result", If(String.IsNullOrWhiteSpace(res.strSpecialInformation), "", res.strSpecialInformation)}
            })
            t.AddHeaderTopRow({"Grubbs Test", ""})
            Return ParametricUDFs.PrepareResultTableForUdf(t.returnSelf())
        End Function

        Private Function BuildRosnerTable(alpha As Double, outliers() As Double, addCaution As Boolean) As Object(,)
            Dim nOut As Integer = If(outliers Is Nothing, 0, outliers.Length)
            Dim rows As Integer = 3 + Math.Max(1, nOut)
            Dim body(rows - 1, 1) As Object
            body(0, 0) = "Alpha"
            body(0, 1) = alpha
            body(1, 0) = "Number of outliers"
            body(1, 1) = nOut
            body(2, 0) = "List of outliers"
            If nOut > 0 Then
                body(2, 1) = outliers(0)
            Else
                body(2, 1) = ""
            End If
            For i As Integer = 1 To nOut - 1
                body(2 + i, 0) = ""
                body(2 + i, 1) = outliers(i)
            Next
            If nOut = 0 Then
                body(3, 0) = ""
                body(3, 1) = ""
            End If

            Dim t As New ResultTable
            t.SetBody(body)
            t.AddHeaderTopRow({"Rosner Generalized ESD Test", ""})
            If addCaution Then
                t.AddFootnote("Interpret with caution for sample sizes below 25.")
            End If
            Return ParametricUDFs.PrepareResultTableForUdf(t.returnSelf())
        End Function

        Private Function TryParseLeveneCenter(arg As Object, ByRef useMedian As Boolean, ByRef title As String) As Boolean
            useMedian = False
            title = "Levene Test"

            Dim s As String = NormalizeText(arg)
            If s = "" OrElse s = "MEAN" OrElse s = "LEVENE" OrElse s = "CLASSICAL" Then
                useMedian = False
                title = "Levene Test"
                Return True
            End If

            Select Case s
                Case "MEDIAN", "BROWN-FORSYTHE", "BROWNFORSYTHE", "BF"
                    useMedian = True
                    title = "Brown-Forsythe Test"
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Private Function TryParseSymmetryMethod(arg As Object, ByRef methodInternal As String, ByRef title As String) As Boolean
            Dim s As String = NormalizeText(arg)
            If s = "" OrElse s = "MGG" OrElse s = "MIAO-GEL-GASTWIRTH" OrElse s = "MIAO" Then
                methodInternal = "Miao-Gel-Gastwirth"
                title = "Miao-Gel-Gastwirth Symmetry Test"
                Return True
            End If

            Select Case s
                Case "CM", "CABILIO-MASARO", "CABILIO", "MASARO"
                    methodInternal = "Cabilio-Masaro"
                    title = "Cabilio-Masaro Symmetry Test"
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Private Function TryReadSingleNumericColumn(input As Object, ByRef values() As Double, ByRef detectedName As String) As Boolean
            values = Nothing
            detectedName = ""

            Dim arr As Object(,) = UDFhelpers.Get2D(input)
            If arr Is Nothing Then Return False
            If arr.GetLength(1) <> 1 Then Return False

            Dim hasHeader As Boolean = LooksLikeSingleColumnHeader(arr)
            If hasHeader Then detectedName = Convert.ToString(arr(0, 0)).Trim()

            Dim startRow As Integer = If(hasHeader, 1, 0)
            Dim list As New List(Of Double)
            For r As Integer = startRow To arr.GetLength(0) - 1
                Dim d = UDFhelpers.TryGetDouble(arr(r, 0))
                If d.HasValue AndAlso IsFinite(d.Value) Then list.Add(d.Value)
            Next

            values = list.ToArray()
            Return True
        End Function

        Private Function TryReadGroupedCompleteMatrix(data As Object, groups As Object, ByRef groupMatrices As List(Of Double(,)), ByRef groupNames As List(Of String)) As Boolean
            groupMatrices = Nothing
            groupNames = Nothing

            Dim dataArr As Object(,) = UDFhelpers.Get2D(data)
            Dim groupArr As Object(,) = UDFhelpers.Get2D(groups)
            If dataArr Is Nothing OrElse groupArr Is Nothing Then Return False
            If groupArr.GetLength(1) <> 1 Then Return False

            Dim dataRows As Integer = dataArr.GetLength(0)
            Dim dataCols As Integer = dataArr.GetLength(1)
            If dataRows < 1 OrElse dataCols < 1 Then Return False

            Dim dataHasHeader As Boolean = LooksLikeHeaderRow(dataArr, Enumerable.Range(0, dataCols).ToArray())
            Dim startData As Integer = If(dataHasHeader, 1, 0)

            Dim usableRows As Integer = dataRows - startData
            Dim groupRows As Integer = groupArr.GetLength(0)

            Dim startGroup As Integer
            If groupRows = usableRows Then
                startGroup = 0
            ElseIf groupRows = usableRows + 1 Then
                startGroup = 1
            Else
                Return False
            End If

            Dim buckets As New Dictionary(Of String, List(Of Double()))(StringComparer.OrdinalIgnoreCase)
            Dim order As New List(Of String)

            For i As Integer = 0 To usableRows - 1
                Dim label As String = Convert.ToString(groupArr(startGroup + i, 0)).Trim()
                If String.IsNullOrWhiteSpace(label) Then Continue For

                Dim row(dataCols - 1) As Double
                Dim ok As Boolean = True
                For c As Integer = 0 To dataCols - 1
                    Dim d = UDFhelpers.TryGetDouble(dataArr(startData + i, c))
                    If Not d.HasValue OrElse Not IsFinite(d.Value) Then
                        ok = False
                        Exit For
                    End If
                    row(c) = d.Value
                Next
                If Not ok Then Continue For

                If Not buckets.ContainsKey(label) Then
                    buckets(label) = New List(Of Double())
                    order.Add(label)
                End If
                buckets(label).Add(row)
            Next

            groupMatrices = New List(Of Double(,))
            groupNames = New List(Of String)(order)
            For Each label In order
                groupMatrices.Add(RowsToMatrix(buckets(label), dataCols))
            Next

            Return True
        End Function

        Private Function RowsToMatrix(rows As List(Of Double()), cols As Integer) As Double(,)
            Dim out(rows.Count - 1, cols - 1) As Double
            For r As Integer = 0 To rows.Count - 1
                For c As Integer = 0 To cols - 1
                    out(r, c) = rows(r)(c)
                Next
            Next
            Return out
        End Function

        Private Function SampleCovariance(mat As Double(,)) As Double(,)
            Dim n As Integer = mat.GetLength(0)
            Dim p As Integer = mat.GetLength(1)
            Dim means(p - 1) As Double
            For c As Integer = 0 To p - 1
                For r As Integer = 0 To n - 1
                    means(c) += mat(r, c)
                Next
                means(c) /= n
            Next

            Dim cov(p - 1, p - 1) As Double
            For i As Integer = 0 To p - 1
                For j As Integer = i To p - 1
                    Dim s As Double = 0.0
                    For r As Integer = 0 To n - 1
                        s += (mat(r, i) - means(i)) * (mat(r, j) - means(j))
                    Next
                    cov(i, j) = s / (n - 1)
                    cov(j, i) = cov(i, j)
                Next
            Next
            Return cov
        End Function

    End Module
End Namespace
