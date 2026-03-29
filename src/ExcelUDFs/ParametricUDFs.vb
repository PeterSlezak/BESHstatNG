Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports ExcelDna.Integration
Imports BESHStatNG.Matrix

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions exposing selected parametric ANOVA procedures.
    ''' </summary>
    Public Module ParametricUDFs

        ' -------------------------------------------------------------------------------------------------------------
        ' One-way ANOVA
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Classical one-way ANOVA for comparing means across two or more independent groups.
        ''' </summary>
        ''' <param name="groups">
        ''' Multi-column Excel range where each column represents one group.
        ''' Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
        ''' If the first row contains non-numeric labels, it is treated as a header row and used as group names.
        ''' </param>
        ''' <param name="groupNames">
        ''' Optional group names supplied as a comma-separated string or as a one-row/one-column range.
        ''' When omitted, names are taken from the first row of <paramref name="groups"/> when it looks like a header;
        ''' otherwise default names such as Group 1, Group 2, … are used.
        ''' </param>
        ''' <returns>
        ''' A complete ANOVA table with row and column headers, including between-group, within-group, and total sums of squares,
        ''' degrees of freedom, mean squares, F statistic, and p-value.
        ''' Returns <c>#VALUE!</c> if the input is not a valid grouped range.
        ''' Returns <c>#NUM!</c> if fewer than two non-empty groups remain after filtering.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' One-way ANOVA tests whether the population means of several independent groups are equal.
        ''' The total variability is partitioned into variability explained by differences between group means and residual variability within groups.
        ''' The test statistic is
        ''' <c>F = MS_between / MS_within</c>,
        ''' where <c>MS</c> denotes a mean square.
        ''' </para>
        ''' <para>
        ''' A small p-value indicates evidence that at least one group mean differs from the others.
        ''' The test assumes independent observations, approximate normality within groups, and equal variances across groups.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.PAR.ANOVA1(A1:C20)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.PAR.ANOVA1",
            Category:="BESHStatNG - Parametric",
            Description:="One-way ANOVA table. Input: one column per group.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/parametric/")>
        Public Function ANOVA1(
            <ExcelArgument(Name:="groups", Description:="Multi-column range; one column per group. First row may contain headers.")> groups As Object,
            <ExcelArgument(Name:="groupNames", Description:="Optional group names as comma-separated text or 1-row/1-column range.")> Optional groupNames As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadGroupedNumericColumns(groups, data, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(groupNames, detectedNames, data.Length, "Group")
                Dim mdl As New parametric.OneWayANOVA(data, names)
                mdl.compute()
                Dim tables = mdl.wrapResults()
                Return PrepareResultTableForUdf(tables(0).returnSelf())
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ''' <summary>
        ''' Welch heteroscedastic one-way ANOVA for comparing means when group variances may differ.
        ''' </summary>
        ''' <param name="groups">
        ''' Multi-column Excel range where each column represents one group.
        ''' Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
        ''' If the first row contains non-numeric labels, it is treated as a header row and used as group names.
        ''' </param>
        ''' <param name="groupNames">
        ''' Optional group names supplied as a comma-separated string or as a one-row/one-column range.
        ''' </param>
        ''' <returns>
        ''' A Welch ANOVA summary table showing numerator and denominator degrees of freedom, F statistic, and p-value.
        ''' Returns <c>#VALUE!</c> if the input is not a valid grouped range.
        ''' Returns <c>#NUM!</c> if fewer than two non-empty groups remain after filtering.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Welch's ANOVA is a heteroscedastic alternative to classical one-way ANOVA.
        ''' It tests equality of group means without requiring equal variances and adjusts the denominator degrees of freedom
        ''' using a Satterthwaite-type approximation.
        ''' </para>
        ''' <para>
        ''' This procedure is often preferred when group sizes and variances are notably unequal.
        ''' The null hypothesis remains that all group means are equal.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.PAR.ANOVA1_WELCH(A1:C20)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.PAR.ANOVA1_WELCH",
            Category:="BESHStatNG - Parametric",
            Description:="Welch one-way ANOVA summary. Input: one column per group.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/parametric/")>
        Public Function ANOVA1_WELCH(
            <ExcelArgument(Name:="groups", Description:="Multi-column range; one column per group. First row may contain headers.")> groups As Object,
            <ExcelArgument(Name:="groupNames", Description:="Optional group names as comma-separated text or 1-row/1-column range.")> Optional groupNames As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadGroupedNumericColumns(groups, data, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(groupNames, detectedNames, data.Length, "Group")
                Dim mdl As New parametric.OneWayANOVA(data, names)
                mdl.compute()
                Dim w = mdl.WelshANOVA()

                Dim body(0, 3) As Object
                body(0, 0) = data.Length - 1
                body(0, 1) = w.DF1
                body(0, 2) = w.TestStatistics1
                body(0, 3) = w.Pvalue

                Dim t As New ResultTable
                t.SetBody(body)
                t.AddHeaderLeftRow({"Welch ANOVA"})
                t.AddHeaderTopRow({"Source", "df numerator", "df denominator", "F", "P-value"})
                Return PrepareResultTableForUdf(t.returnSelf())
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Repeated-measures ANOVA
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' One-way repeated-measures ANOVA for comparing several conditions measured on the same subjects or blocks.
        ''' </summary>
        ''' <param name="data">
        ''' Numeric matrix where rows are subjects/blocks and columns are repeated-measure conditions.
        ''' Rows containing any missing or non-numeric value are excluded so that the remaining matrix is complete.
        ''' If the first row contains non-numeric labels, it is treated as a header row and used as condition names.
        ''' </param>
        ''' <param name="conditionNames">
        ''' Optional condition names supplied as a comma-separated string or as a one-row/one-column range.
        ''' </param>
        ''' <param name="correction">
        ''' Optional sphericity-correction setting:
        ''' <list type="bullet">
        ''' <item><description><c>"none"</c> — classical RM-ANOVA table only (default)</description></item>
        ''' <item><description><c>"gg"</c> — append Greenhouse–Geisser epsilon and corrected p-value</description></item>
        ''' <item><description><c>"hf"</c> — append Huynh–Feldt epsilon and corrected p-value</description></item>
        ''' <item><description><c>"both"</c> — append both corrections</description></item>
        ''' </list>
        ''' </param>
        ''' <returns>
        ''' A complete repeated-measures ANOVA table with row and column headers.
        ''' Depending on <paramref name="correction"/>, the table may include Greenhouse–Geisser and/or Huynh–Feldt corrections.
        ''' Returns <c>#VALUE!</c> if the input is not a valid repeated-measures matrix.
        ''' Returns <c>#NUM!</c> if too few complete rows remain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' One-way repeated-measures ANOVA partitions variation into treatment (between conditions), subject, and residual components.
        ''' It tests whether the mean response differs across repeated conditions while accounting for subject-level dependence.
        ''' </para>
        ''' <para>
        ''' The usual F test assumes sphericity, meaning that the variances of all pairwise differences between conditions are equal.
        ''' Greenhouse–Geisser and Huynh–Feldt corrections relax this assumption by shrinking the effective degrees of freedom,
        ''' which typically increases the p-value when sphericity is violated.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.PAR.RMANOVA1(A1:D25,,"both")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.PAR.RMANOVA1",
            Category:="BESHStatNG - Parametric",
            Description:="One-way repeated-measures ANOVA table. Input: rows=subjects, cols=conditions.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/parametric/")>
        Public Function RMANOVA1(
            <ExcelArgument(Name:="data", Description:="Numeric matrix; rows=subjects, columns=conditions. First row may contain headers.")> data As Object,
            <ExcelArgument(Name:="conditionNames", Description:="Optional condition names as comma-separated text or 1-row/1-column range.")> Optional conditionNames As Object = Nothing,
            <ExcelArgument(Name:="correction", Description:="Optional: none/GG/HF/both.")> Optional correction As Object = Nothing
        ) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadCompleteNumericMatrixWithHeaders(data, mat, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If mat.GetLength(0) < 2 OrElse mat.GetLength(1) < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(conditionNames, detectedNames, mat.GetLength(1), "Condition")
                Dim mdl As New parametric.OneWayRmANOVA(mat, names)
                mdl.compute()

                Dim which As String = NormalizeText(correction)
                Select Case which
                    Case "", "NONE"
                        ' no correction
                    Case "GG", "GREENHOUSE", "GREENHOUSE-GEISSER", "GREENHOUSEGEISSER"
                        mdl.GreenhouseGeisser()
                    Case "HF", "HUYNH", "HUYNH-FELDT", "HUYNHFELDT"
                        mdl.HuyhnFeldt()
                    Case "BOTH", "ALL"
                        mdl.GreenhouseGeisser()
                        mdl.HuyhnFeldt()
                    Case Else
                        Return ExcelError.ExcelErrorValue
                End Select

                Dim tables = mdl.wrapResults()
                Return PrepareResultTableForUdf(tables(0).returnSelf())
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Two-way nested ANOVA
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Two-way nested ANOVA for designs where one factor is nested within another.
        ''' </summary>
        ''' <param name="data">
        ''' Three-column Excel range containing:
        ''' <list type="bullet">
        ''' <item><description>Column 1: higher-level group factor</description></item>
        ''' <item><description>Column 2: subgroup factor nested within the group factor</description></item>
        ''' <item><description>Column 3: numeric response variable</description></item>
        ''' </list>
        ''' The first row may contain headers. Rows with blank factor labels or non-numeric responses are ignored.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional variable names as a comma-separated string or a one-row/one-column range.
        ''' Three names are expected: group factor, subgroup factor, and response variable.
        ''' </param>
        ''' <param name="outputType">
        ''' Optional output selection:
        ''' <list type="bullet">
        ''' <item><description><c>"both"</c> — return the main ANOVA table followed by the Satterthwaite-adjusted table (default)</description></item>
        ''' <item><description><c>"main"</c> — return the main ANOVA table only</description></item>
        ''' <item><description><c>"satterthwaite"</c> or <c>"sw"</c> — return only the Satterthwaite-adjusted table</description></item>
        ''' </list>
        ''' </param>
        ''' <returns>
        ''' A labeled ANOVA table, or two stacked tables when <paramref name="outputType"/> is <c>"both"</c>.
        ''' The main table includes variance-component percentages. The Satterthwaite table is returned when applicable.
        ''' Returns <c>#VALUE!</c> for invalid input shape and <c>#NUM!</c> when too few valid observations remain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' In a nested ANOVA, the levels of one factor occur only within a single level of another factor.
        ''' This differs from a crossed two-way design, where all combinations of factor levels may occur.
        ''' </para>
        ''' <para>
        ''' The procedure partitions variation into between-group, between-subgroup-within-group, and residual components.
        ''' When the design is unbalanced, Satterthwaite-style approximations may be used for selected F tests.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.PAR.ANOVA2_NESTED(A1:C100,,"both")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.PAR.ANOVA2_NESTED",
            Category:="BESHStatNG - Parametric",
            Description:="Two-way nested ANOVA. Input: 3 columns = group, subgroup, response.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/parametric/")>
        Public Function ANOVA2_NESTED(
            <ExcelArgument(Name:="data", Description:="Three columns: group, subgroup (nested), response. First row may contain headers.")> data As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional variable names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="output", Description:="Optional: both/main/satterthwaite.")> Optional outputType As Object = Nothing
        ) As Object
            Try
                Dim mat(,) As Object = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadNestedThreeColumnData(data, mat, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If mat.GetLength(0) < 3 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(varNames, detectedNames, 3, "Var")
                Dim mdl As New parametric.TwoWayNestedANOVA(mat, names)
                mdl.compute()

                Dim tables = mdl.wrapResults()
                Dim which As String = NormalizeText(outputType)

                If which = "" OrElse which = "BOTH" OrElse which = "ALL" Then
                    Return PrepareResultTableForUdf(StackWithBlankRow(tables(0).returnSelf(), tables(1).returnSelf()))
                ElseIf which = "MAIN" OrElse which = "CLASSICAL" Then
                    Return PrepareResultTableForUdf(tables(0).returnSelf())
                ElseIf which = "SATTERTHWAITE" OrElse which = "SW" OrElse which = "ADJUSTED" Then
                    Return PrepareResultTableForUdf(tables(1).returnSelf())
                Else
                    Return ExcelError.ExcelErrorValue
                End If
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' One-way ANOVA multiple comparisons
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' One-way ANOVA post-hoc multiple comparisons for grouped data.
        ''' </summary>
        ''' <param name="groups">
        ''' Multi-column Excel range where each column represents one group.
        ''' Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
        ''' If the first row contains non-numeric labels, it is treated as a header row and used as group names.
        ''' </param>
        ''' <param name="groupNames">
        ''' Optional group names supplied as a comma-separated string or as a one-row/one-column range.
        ''' When omitted, names are taken from the first row of <paramref name="groups"/> when it looks like a header;
        ''' otherwise default names such as Group 1, Group 2, … are used.
        ''' </param>
        ''' <param name="method">
        ''' Post-hoc method to return:
        ''' <list type="bullet">
        ''' <item><description><c>"tukey"</c> / <c>"tukey-kramer"</c> (default)</description></item>
        ''' <item><description><c>"games-howell"</c></description></item>
        ''' <item><description><c>"lsd"</c> / <c>"fisher"</c></description></item>
        ''' <item><description><c>"bonferroni"</c></description></item>
        ''' <item><description><c>"all"</c> — stack all four MCP tables</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="alpha">
        ''' Significance level used for confidence intervals in the returned MCP table(s).
        ''' The default is `0.05`, corresponding to 95% confidence intervals.
        ''' </param>
        ''' <returns>
        ''' A labeled multiple-comparison table as a dynamic array.
        ''' Returns <c>#VALUE!</c> for invalid input or unknown method.
        ''' Returns <c>#NUM!</c> for invalid alpha or too few groups.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.PAR.ANOVA1_MCP",
            Category:="BESHStatNG - Parametric",
            Description:="One-way ANOVA multiple comparisons: Tukey-Kramer, Games-Howell, Fisher LSD, or Bonferroni.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/parametric/")>
        Public Function ANOVA1_MCP(
            <ExcelArgument(Name:="groups", Description:="Multi-column range; one column per group. First row may contain headers.")> groups As Object,
            <ExcelArgument(Name:="groupNames", Description:="Optional group names as comma-separated text or 1-row/1-column range.")> Optional groupNames As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional: tukey/tukey-kramer (default), games-howell, lsd/fisher, bonferroni, or all.")> Optional method As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided significance level for confidence intervals. Default 0.05 (95% confidence interval).")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadGroupedNumericColumns(groups, data, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(groupNames, detectedNames, data.Length, "Group")

                Dim alphaValue As Double = 0.05
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim which As String = NormalizeText(method)
                Dim mdl As New parametric.OneWayANOVA(data, names)
                mdl.compute()

                Select Case which
                    Case "", "TUKEY", "TUKEYKRAMER", "TUKEY-KRAMER", "TK"
                        mdl.TukeyKramer(alphaValue)
                        Dim tables = mdl.wrapResults()
                        Return PrepareResultTableForUdf(tables(1).returnSelf())

                    Case "GAMESHOWELL", "GAMES-HOWELL", "GH", "GAMES"
                        mdl.GamesHowell(alphaValue)
                        Dim tables = mdl.wrapResults()
                        Return PrepareResultTableForUdf(tables(1).returnSelf())

                    Case "LSD", "FISHER", "FISHERLSD", "FISHER-LSD"
                        mdl.FisherLSD(False, alphaValue)
                        Dim tables = mdl.wrapResults()
                        Return PrepareResultTableForUdf(tables(1).returnSelf())

                    Case "BONF", "BONFERRONI"
                        mdl.FisherLSD(True, alphaValue)
                        Dim tables = mdl.wrapResults()
                        Return PrepareResultTableForUdf(tables(1).returnSelf())

                    Case "ALL", "BOTH"
                        mdl.FisherLSD(False, alphaValue)
                        mdl.FisherLSD(True, alphaValue)
                        mdl.TukeyKramer(alphaValue)
                        mdl.GamesHowell(alphaValue)

                        Dim tables = mdl.wrapResults()
                        Dim stacked As Object(,) = TryCast(tables(1).returnSelf(), Object(,))
                        For i As Integer = 2 To tables.Count - 1
                            stacked = StackWithBlankRow(stacked, TryCast(tables(i).returnSelf(), Object(,)))
                        Next
                        Return PrepareResultTableForUdf(stacked)

                    Case Else
                        Return ExcelError.ExcelErrorValue
                End Select
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Repeated-measures ANOVA multiple comparisons
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' One-way repeated-measures ANOVA post-hoc multiple comparisons.
        ''' </summary>
        ''' <param name="data">
        ''' Numeric matrix where rows are subjects/blocks and columns are repeated-measure conditions.
        ''' Rows containing any missing or non-numeric value are excluded so that the remaining matrix is complete.
        ''' If the first row contains non-numeric labels, it is treated as a header row and used as condition names.
        ''' </param>
        ''' <param name="conditionNames">
        ''' Optional condition names supplied as a comma-separated string or as a one-row/one-column range.
        ''' </param>
        ''' <param name="method">
        ''' Post-hoc method to return:
        ''' <list type="bullet">
        ''' <item><description><c>"rm2"</c> / <c>"tukeyrm2"</c> (default; does not assume sphericity)</description></item>
        ''' <item><description><c>"tukey"</c> / <c>"sphericity"</c> (assumes sphericity)</description></item>
        ''' <item><description><c>"all"</c> — stack both MCP tables</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="alpha">
        ''' Significance level used for confidence intervals in the returned MCP table(s).
        ''' The default is `0.05`, corresponding to 95% confidence intervals.
        ''' </param>
        ''' <returns>
        ''' A labeled multiple-comparison table as a dynamic array.
        ''' Returns <c>#VALUE!</c> for invalid input or unknown method.
        ''' Returns <c>#NUM!</c> for invalid alpha or too few complete rows/conditions.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.PAR.RMANOVA1_MCP",
            Category:="BESHStatNG - Parametric",
            Description:="Repeated-measures ANOVA multiple comparisons: TukeyKramerRM2 (default) or Tukey assuming sphericity.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/parametric/")>
        Public Function RMANOVA1_MCP(
            <ExcelArgument(Name:="data", Description:="Numeric matrix; rows=subjects, columns=conditions. First row may contain headers.")> data As Object,
            <ExcelArgument(Name:="conditionNames", Description:="Optional condition names as comma-separated text or 1-row/1-column range.")> Optional conditionNames As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional: rm2/tukeyrm2 (default), tukey/sphericity, or all.")> Optional method As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided significance level for confidence intervals. Default 0.05 (95% confidence interval).")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadCompleteNumericMatrixWithHeaders(data, mat, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If mat.GetLength(0) < 2 OrElse mat.GetLength(1) < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(conditionNames, detectedNames, mat.GetLength(1), "Condition")

                Dim alphaValue As Double = 0.05
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim which As String = NormalizeText(method)
                Dim mdl As New parametric.OneWayRmANOVA(mat, names)
                mdl.compute()

                Select Case which
                    Case "", "RM2", "TUKEYRM2", "TUKEY-RM2", "RECOMMENDED", "NOSPHERICITY", "NO-SPHERICITY"
                        mdl.TukeyKramerRM2(alphaValue)
                        Dim tables = mdl.wrapResults()
                        Return PrepareResultTableForUdf(tables(1).returnSelf())

                    Case "TUKEY", "SPHERICITY", "ASSUME-SPHERICITY", "ASSUMESPHERICITY", "CLASSICAL"
                        mdl.Tukey(alphaValue)
                        Dim tables = mdl.wrapResults()
                        Return PrepareResultTableForUdf(tables(1).returnSelf())

                    Case "ALL", "BOTH"
                        mdl.TukeyKramerRM2(alphaValue)
                        mdl.Tukey(alphaValue)

                        Dim tables = mdl.wrapResults()
                        Dim stacked As Object(,) = TryCast(tables(1).returnSelf(), Object(,))
                        For i As Integer = 2 To tables.Count - 1
                            stacked = StackWithBlankRow(stacked, TryCast(tables(i).returnSelf(), Object(,)))
                        Next
                        Return PrepareResultTableForUdf(stacked)

                    Case Else
                        Return ExcelError.ExcelErrorValue
                End Select
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' T-tests
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Two-sample unpaired t-test for comparing the means of two independent groups.
        ''' </summary>
        ''' <param name="x">
        ''' First group as a single-column Excel range.
        ''' Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
        ''' it is treated as a header and may be used as the default name of the first group.
        ''' </param>
        ''' <param name="y">
        ''' Second group as a single-column Excel range.
        ''' Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
        ''' it is treated as a header and may be used as the default name of the second group.
        ''' </param>
        ''' <param name="groupNames">
        ''' Optional group names supplied as a comma-separated string or as a one-row/one-column range.
        ''' Two names are expected. When omitted, names are taken from header cells when available;
        ''' otherwise default names such as Group 1 and Group 2 are used.
        ''' </param>
        ''' <param name="outputType">
        ''' Optional output selection:
        ''' <list type="bullet">
        ''' <item><description><c>"both"</c> — return the pooled-variance table followed by the Welch table (default)</description></item>
        ''' <item><description><c>"equal"</c>, <c>"pooled"</c>, or <c>"student"</c> — return only the equal-variance table</description></item>
        ''' <item><description><c>"unequal"</c> or <c>"welch"</c> — return only the unequal-variance table</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the mean-difference confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A labeled result table, or two stacked labeled tables when <paramref name="outputType"/> is <c>"both"</c>.
        ''' The equal-variance output reports the pooled standard error, t statistic, degrees of freedom, two-sided p-value,
        ''' and confidence interval for the mean difference. The unequal-variance output reports the Welch standard error,
        ''' Welch degrees of freedom, two-sided p-value, confidence interval, and the p-value of the variance-comparison F test.
        ''' Returns <c>#VALUE!</c> if either input is not a single column or if <paramref name="outputType"/> is not recognized.
        ''' Returns <c>#NUM!</c> if either group has fewer than two usable numeric observations or if <paramref name="alpha"/> is invalid.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This worksheet function compares two <b>independent</b> samples.
        ''' By default it returns both the classical pooled-variance t-test and Welch’s unequal-variance alternative,
        ''' which is often preferred when sample sizes or variances differ noticeably.
        ''' </para>
        ''' <para>
        ''' The test statistic is based on the difference between the sample means.
        ''' The pooled version assumes equal population variances, while the Welch version does not and uses an adjusted
        ''' degrees-of-freedom approximation.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.PAR.TTEST_UNPAIRED(A2:A21, B2:B19)
        ''' =BESH.PAR.TTEST_UNPAIRED(A1:A21, B1:B19, "Control,Treatment", "welch", 0.01)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.PAR.TTEST_UNPAIRED",
            Category:="BESHStatNG - Parametric",
            Description:="Two-sample unpaired t-test. Returns pooled, Welch, or both result tables.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/parametric/")>
        Public Function TTEST_UNPAIRED(
            <ExcelArgument(Name:="x", Description:="First independent group as a single-column range. Non-numeric cells ignored; first cell may be a header.")> x As Object,
            <ExcelArgument(Name:="y", Description:="Second independent group as a single-column range. Non-numeric cells ignored; first cell may be a header.")> y As Object,
            <ExcelArgument(Name:="groupNames", Description:="Optional group names as comma-separated text or 1-row/1-column range.")> Optional groupNames As Object = Nothing,
            <ExcelArgument(Name:="output", Description:="Optional: both (default), equal/pooled/student, or unequal/welch.")> Optional outputType As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided significance level for the confidence interval. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadIndependentNumericColumns(x, y, data, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If data Is Nothing OrElse data.Length <> 2 Then Return ExcelError.ExcelErrorValue
                If data(0) Is Nothing OrElse data(1) Is Nothing Then Return ExcelError.ExcelErrorNum
                If data(0).Length < 2 OrElse data(1).Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(groupNames, detectedNames, 2, "Group")

                Dim alphaValue As Double = 0.05
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim mdl As New parametric.UnpairedTtest(data, names)
                mdl.compute(alphaValue)
                Dim tables = mdl.wrapResults()

                Dim which As String = NormalizeText(outputType)
                Select Case which
                    Case "", "BOTH", "ALL"
                        Return PrepareResultTableForUdf(StackWithBlankRow(tables(0).returnSelf(), tables(1).returnSelf()))
                    Case "EQUAL", "POOLED", "STUDENT", "ASSUME-EQUAL", "ASSUMEEQUAL"
                        Return PrepareResultTableForUdf(tables(0).returnSelf())
                    Case "UNEQUAL", "WELCH", "ASSUME-UNEQUAL", "ASSUMEUNEQUAL"
                        Return PrepareResultTableForUdf(tables(1).returnSelf())
                    Case Else
                        Return ExcelError.ExcelErrorValue
                End Select
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ''' <summary>
        ''' Paired t-test for comparing the mean of within-row differences between two matched measurements.
        ''' </summary>
        ''' <param name="x">
        ''' First measurement as a single-column Excel range.
        ''' Values are paired by row with the second input. If the first cell is a non-numeric label and numeric values follow,
        ''' it is treated as a header and may be used as the default name of the first measurement.
        ''' </param>
        ''' <param name="y">
        ''' Second measurement as a single-column Excel range.
        ''' Values are paired by row with the first input. If the first cell is a non-numeric label and numeric values follow,
        ''' it is treated as a header and may be used as the default name of the second measurement.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional names supplied as a comma-separated string or as a one-row/one-column range.
        ''' Two names are expected. When omitted, names are taken from header cells when available;
        ''' otherwise default names such as Sample 1 and Sample 2 are used.
        ''' </param>
        ''' <returns>
        ''' A labeled result table showing the number of usable pairs, the mean of differences,
        ''' the standard deviation and standard error of the differences, the degrees of freedom,
        ''' the t statistic, and the two-sided p-value.
        ''' Returns <c>#VALUE!</c> if either input is not a single column or if the two inputs have different row counts.
        ''' Returns <c>#NUM!</c> if fewer than two usable numeric pairs remain after filtering.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This worksheet function compares two <b>paired</b> measurements, such as before/after values,
        ''' left/right measurements, or matched observations from the same subjects.
        ''' Rows are matched strictly by position.
        ''' </para>
        ''' <para>
        ''' The test is carried out on the within-row differences <c>x - y</c>.
        ''' It tests whether the mean difference equals zero while accounting for the pairing structure.
        ''' Rows where either entry is non-numeric are discarded before the calculation.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.PAR.TTEST_PAIRED(A2:A21, B2:B21)
        ''' =BESH.PAR.TTEST_PAIRED(A1:A21, B1:B21, "Before,After")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.PAR.TTEST_PAIRED",
            Category:="BESHStatNG - Parametric",
            Description:="Paired t-test for two matched samples. Returns a labeled result table.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/parametric/")>
        Public Function TTEST_PAIRED(
            <ExcelArgument(Name:="x", Description:="First paired sample as a single-column range. Values are paired by row; first cell may be a header.")> x As Object,
            <ExcelArgument(Name:="y", Description:="Second paired sample as a single-column range. Values are paired by row; first cell may be a header.")> y As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing
        ) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not TryReadPairedNumericColumns(x, y, mat, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If mat Is Nothing OrElse mat.GetLength(0) < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(varNames, detectedNames, 2, "Sample")
                Dim mdl As New parametric.PairedTtest(mat, names)
                mdl.compute()
                Dim tables = mdl.wrapResults()
                Return PrepareResultTableForUdf(tables(0).returnSelf())
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Private/Friend helpers
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Converts a result table object into a 2D object array suitable for returning
        ''' from an Excel-DNA UDF.
        ''' </summary>
        ''' <param name="table">
        ''' The source table object, expected to be a two-dimensional <see cref="Object"/> array.
        ''' </param>
        ''' <returns>
        ''' A two-dimensional <see cref="Object"/> array with <c>Nothing</c> and
        ''' <see cref="DBNull"/> values converted to empty strings.
        ''' Returns <c>Nothing</c> if <paramref name="table"/> cannot be cast to
        ''' a two-dimensional object array.
        ''' </returns>
        Friend Function PrepareResultTableForUdf(table As Object) As Object(,)
            Dim arr As Object(,) = TryCast(table, Object(,))
            If arr Is Nothing Then Return Nothing

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            Dim out(rows - 1, cols - 1) As Object

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    Dim v As Object = arr(r, c)

                    If v Is Nothing Then
                        out(r, c) = String.Empty ' ExcelEmpty.Value
                    ElseIf TypeOf v Is DBNull Then
                        out(r, c) = String.Empty ' ExcelEmpty.Value
                    Else
                        out(r, c) = v
                    End If
                Next
            Next

            Return out
        End Function

        Private Function IsMissingArg(v As Object) As Boolean
            Return v Is Nothing OrElse TypeOf v Is ExcelMissing OrElse TypeOf v Is ExcelEmpty
        End Function

        ''' <summary>
        ''' Normalizes an optional text argument for case-insensitive method matching.
        ''' </summary>
        ''' <param name="v">The input value to normalize.</param>
        ''' <returns>
        ''' An upper-case, trimmed string representation of <paramref name="v"/>.
        ''' Returns an empty string for missing, empty, or null-like Excel arguments.
        ''' </returns>
        Friend Function NormalizeText(v As Object) As String
            If IsMissingArg(v) Then Return ""
            Dim s As String = Convert.ToString(v)
            If s Is Nothing Then Return ""
            Return s.Trim().ToUpperInvariant()
        End Function

        ''' <summary>
        ''' Resolves a final set of display names for groups or conditions.
        ''' </summary>
        ''' <param name="explicitNames">
        ''' Optional user-supplied names, typically as a comma-separated string or a one-row/one-column range.
        ''' </param>
        ''' <param name="detectedNames">
        ''' Names detected from the input range, typically from a header row.
        ''' </param>
        ''' <param name="expectedCount">The required number of names.</param>
        ''' <param name="prefix">
        ''' Fallback prefix used to generate default names such as
        ''' <c>Group 1</c>, <c>Group 2</c>, or <c>Condition 1</c>.
        ''' </param>
        ''' <returns>
        ''' A string array of length <paramref name="expectedCount"/> containing the resolved names.
        ''' User-supplied names take priority, then detected names, then generated fallback names.
        ''' Blank resolved entries are replaced with generated fallback names.
        ''' </returns>
        Friend Function ResolveNames(explicitNames As Object, detectedNames() As String, expectedCount As Integer, prefix As String) As String()
            If Not IsMissingArg(explicitNames) Then
                Dim names = UDFhelpers.GetVarNames(explicitNames, expectedCount)
                For i As Integer = 0 To names.Length - 1
                    If String.IsNullOrWhiteSpace(names(i)) Then names(i) = prefix & " " & (i + 1).ToString()
                Next
                Return names
            End If

            If detectedNames IsNot Nothing AndAlso detectedNames.Length = expectedCount Then
                Dim names(expectedCount - 1) As String
                For i As Integer = 0 To expectedCount - 1
                    names(i) = detectedNames(i)
                    If String.IsNullOrWhiteSpace(names(i)) Then names(i) = prefix & " " & (i + 1).ToString()
                Next
                Return names
            End If

            Dim fallback(expectedCount - 1) As String
            For i As Integer = 0 To expectedCount - 1
                fallback(i) = prefix & " " & (i + 1).ToString()
            Next
            Return fallback
        End Function

        Private Function LooksLikeHeaderRow(arr As Object(,), numericCols As Integer()) As Boolean
            Dim rows As Integer = arr.GetLength(0)
            If rows < 2 Then Return False

            Dim anyNonNumeric As Boolean = False
            For Each c In numericCols
                If Not TryGetDouble(arr(0, c)).HasValue Then
                    anyNonNumeric = True
                    Exit For
                End If
            Next
            If Not anyNonNumeric Then Return False

            For Each c In numericCols
                Dim foundNumericBelow As Boolean = False
                For r As Integer = 1 To rows - 1
                    If TryGetDouble(arr(r, c)).HasValue Then
                        foundNumericBelow = True
                        Exit For
                    End If
                Next
                If Not foundNumericBelow Then Return False
            Next

            Return True
        End Function

        Private Function LooksLikeSingleColumnHeader(arr As Object(,)) As Boolean
            If arr Is Nothing Then Return False
            If arr.GetLength(1) <> 1 Then Return False

            Dim rows As Integer = arr.GetLength(0)
            If rows < 2 Then Return False
            If TryGetDouble(arr(0, 0)).HasValue Then Return False

            For r As Integer = 1 To rows - 1
                If TryGetDouble(arr(r, 0)).HasValue Then Return True
            Next

            Return False
        End Function

        Private Function TryReadIndependentNumericColumns(x As Object, y As Object, ByRef groups()() As Double, ByRef names() As String) As Boolean
            groups = Nothing
            names = Nothing

            Dim ax As Object(,) = UDFhelpers.Get2D(x)
            Dim ay As Object(,) = UDFhelpers.Get2D(y)
            If ax Is Nothing OrElse ay Is Nothing Then Return False
            If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return False

            Dim hasHeaderX As Boolean = LooksLikeSingleColumnHeader(ax)
            Dim hasHeaderY As Boolean = LooksLikeSingleColumnHeader(ay)
            Dim startRowX As Integer = If(hasHeaderX, 1, 0)
            Dim startRowY As Integer = If(hasHeaderY, 1, 0)

            Dim gx As New List(Of Double)
            For r As Integer = startRowX To ax.GetLength(0) - 1
                Dim d = TryGetDouble(ax(r, 0))
                If d.HasValue Then gx.Add(d.Value)
            Next

            Dim gy As New List(Of Double)
            For r As Integer = startRowY To ay.GetLength(0) - 1
                Dim d = TryGetDouble(ay(r, 0))
                If d.HasValue Then gy.Add(d.Value)
            Next

            groups = New Double()() {gx.ToArray(), gy.ToArray()}
            names = New String() {
                If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Group 1"),
                If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Group 2")
            }
            Return True
        End Function

        Private Function TryReadPairedNumericColumns(x As Object, y As Object, ByRef mat As Double(,), ByRef names() As String) As Boolean
            mat = Nothing
            names = Nothing

            Dim ax As Object(,) = UDFhelpers.Get2D(x)
            Dim ay As Object(,) = UDFhelpers.Get2D(y)
            If ax Is Nothing OrElse ay Is Nothing Then Return False
            If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return False
            If ax.GetLength(0) <> ay.GetLength(0) Then Return False

            Dim hasHeaderX As Boolean = LooksLikeSingleColumnHeader(ax)
            Dim hasHeaderY As Boolean = LooksLikeSingleColumnHeader(ay)
            If hasHeaderX <> hasHeaderY Then Return False

            names = New String() {
                If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Sample 1"),
                If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Sample 2")
            }

            Dim pairs As New List(Of Double())
            For r As Integer = 0 To ax.GetLength(0) - 1
                Dim dx = TryGetDouble(ax(r, 0))
                Dim dy = TryGetDouble(ay(r, 0))
                If dx.HasValue AndAlso dy.HasValue Then
                    pairs.Add(New Double() {dx.Value, dy.Value})
                End If
            Next

            If pairs.Count = 0 Then Return True

            mat = New Double(pairs.Count - 1, 1) {}
            For r As Integer = 0 To pairs.Count - 1
                mat(r, 0) = pairs(r)(0)
                mat(r, 1) = pairs(r)(1)
            Next
            Return True
        End Function
        Private Function TryReadGroupedNumericColumns(input As Object, ByRef groups()() As Double, ByRef names() As String) As Boolean
            groups = Nothing
            names = Nothing

            Dim arr As Object(,) = TryCast(input, Object(,))
            If arr Is Nothing Then Return False

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            If cols < 1 OrElse rows < 1 Then Return False

            Dim hasHeader As Boolean = LooksLikeHeaderRow(arr, Enumerable.Range(0, cols).ToArray())
            Dim startRow As Integer = If(hasHeader, 1, 0)

            Dim groupList As New List(Of Double())
            Dim nameList As New List(Of String)

            For c As Integer = 0 To cols - 1
                Dim vals As New List(Of Double)
                For r As Integer = startRow To rows - 1
                    Dim d = TryGetDouble(arr(r, c))
                    If d.HasValue Then vals.Add(d.Value)
                Next
                If vals.Count > 0 Then
                    groupList.Add(vals.ToArray())
                    If hasHeader Then
                        nameList.Add(Convert.ToString(arr(0, c)).Trim())
                    Else
                        nameList.Add("Group " & (groupList.Count).ToString())
                    End If
                End If
            Next

            If groupList.Count < 2 Then Return False
            groups = groupList.ToArray()
            names = nameList.ToArray()
            Return True
        End Function

        ''' <summary>
        ''' Attempts to read a rectangular Excel input range as a complete numeric matrix,
        ''' optionally using the first row as column headers.
        ''' </summary>
        ''' <param name="input">The Excel input value to parse.</param>
        ''' <param name="mat">
        ''' When this method returns <c>True</c>, contains the parsed numeric matrix.
        ''' Rows containing any non-numeric or missing value are excluded.
        ''' </param>
        ''' <param name="names">
        ''' When this method returns <c>True</c>, contains the resolved column names.
        ''' If a header row is detected, its values are used; otherwise default condition names are generated.
        ''' </param>
        ''' <returns>
        ''' <c>True</c> if a valid numeric matrix could be parsed; otherwise <c>False</c>.
        ''' </returns>
        Friend Function TryReadCompleteNumericMatrixWithHeaders(input As Object, ByRef mat As Double(,), ByRef names() As String) As Boolean
            mat = Nothing
            names = Nothing

            Dim arr As Object(,) = TryCast(input, Object(,))
            If arr Is Nothing Then Return False

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            If rows < 1 OrElse cols < 2 Then Return False

            Dim numericCols As Integer() = Enumerable.Range(0, cols).ToArray()
            Dim hasHeader As Boolean = LooksLikeHeaderRow(arr, numericCols)
            Dim startRow As Integer = If(hasHeader, 1, 0)

            names = New String(cols - 1) {}
            For c As Integer = 0 To cols - 1
                names(c) = If(hasHeader, Convert.ToString(arr(0, c)).Trim(), "Condition " & (c + 1).ToString())
            Next

            Dim keepRows As New List(Of Double())
            For r As Integer = startRow To rows - 1
                Dim row(cols - 1) As Double
                Dim ok As Boolean = True
                For c As Integer = 0 To cols - 1
                    Dim d = TryGetDouble(arr(r, c))
                    If Not d.HasValue Then
                        ok = False
                        Exit For
                    End If
                    row(c) = d.Value
                Next
                If ok Then keepRows.Add(row)
            Next

            If keepRows.Count < 1 Then Return False
            mat = New Double(keepRows.Count - 1, cols - 1) {}
            For r As Integer = 0 To keepRows.Count - 1
                For c As Integer = 0 To cols - 1
                    mat(r, c) = keepRows(r)(c)
                Next
            Next
            Return True
        End Function

        Private Function TryReadNestedThreeColumnData(input As Object, ByRef data(,) As Object, ByRef names() As String) As Boolean
            data = Nothing
            names = Nothing

            Dim arr As Object(,) = TryCast(input, Object(,))
            If arr Is Nothing Then Return False

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            If cols <> 3 OrElse rows < 1 Then Return False

            Dim hasHeader As Boolean = False
            If rows >= 2 Then
                Dim firstRespNumeric As Boolean = TryGetDouble(arr(0, 2)).HasValue
                Dim belowRespNumeric As Boolean = False
                For r As Integer = 1 To rows - 1
                    If TryGetDouble(arr(r, 2)).HasValue Then
                        belowRespNumeric = True
                        Exit For
                    End If
                Next
                hasHeader = (Not firstRespNumeric) AndAlso belowRespNumeric
            End If
            Dim startRow As Integer = If(hasHeader, 1, 0)

            names = New String() {
                    If(hasHeader, Convert.ToString(arr(0, 0)).Trim(), "Group"),
                    If(hasHeader, Convert.ToString(arr(0, 1)).Trim(), "Subgroup"),
                    If(hasHeader, Convert.ToString(arr(0, 2)).Trim(), "Response")
                }

            Dim rowsOut As New List(Of Object())
            For r As Integer = startRow To rows - 1
                Dim g As String = Convert.ToString(arr(r, 0)).Trim()
                Dim sg As String = Convert.ToString(arr(r, 1)).Trim()
                Dim y = TryGetDouble(arr(r, 2))
                If g <> "" AndAlso sg <> "" AndAlso y.HasValue Then
                    rowsOut.Add(New Object() {g, sg, y.Value})
                End If
            Next

            If rowsOut.Count < 1 Then Return False
            data = New Object(rowsOut.Count - 1, 2) {}
            For r As Integer = 0 To rowsOut.Count - 1
                data(r, 0) = rowsOut(r)(0)
                data(r, 1) = rowsOut(r)(1)
                data(r, 2) = rowsOut(r)(2)
            Next
            Return True
        End Function

        ''' <summary>
        ''' Vertically stacks two 2D object arrays with a blank separator row between them.
        ''' </summary>
        ''' <param name="a">The first table.</param>
        ''' <param name="b">The second table.</param>
        ''' <returns>
        ''' A new 2D object array containing <paramref name="a"/>, followed by one blank row,
        ''' followed by <paramref name="b"/>.
        ''' If either input is <c>Nothing</c>, the other input is returned unchanged.
        ''' </returns>
        Friend Function StackWithBlankRow(a As Object(,), b As Object(,)) As Object(,)
            If a Is Nothing Then Return b
            If b Is Nothing Then Return a

            Dim rowsA As Integer = a.GetLength(0)
            Dim colsA As Integer = a.GetLength(1)
            Dim rowsB As Integer = b.GetLength(0)
            Dim colsB As Integer = b.GetLength(1)
            Dim cols As Integer = Math.Max(colsA, colsB)

            Dim out(rowsA + 1 + rowsB - 1, cols - 1) As Object

            For i As Integer = 0 To rowsA - 1
                For j As Integer = 0 To colsA - 1
                    out(i, j) = a(i, j)
                Next
            Next

            For i As Integer = 0 To rowsB - 1
                For j As Integer = 0 To colsB - 1
                    out(rowsA + 1 + i, j) = b(i, j)
                Next
            Next

            Return out
        End Function

        ''' <summary>
        ''' Attempts to parse and validate an alpha value from an optional Excel argument.
        ''' </summary>
        ''' <param name="arg">
        ''' The Excel argument to parse. May be missing, numeric, or a string representation of a number.
        ''' </param>
        ''' <param name="alpha">
        ''' When this method returns <c>True</c>, contains the parsed alpha value.
        ''' Defaults to <c>0.05</c> when the argument is missing.
        ''' </param>
        ''' <returns>
        ''' <c>True</c> if a valid alpha in the open interval <c>(0, 1)</c> could be obtained;
        ''' otherwise <c>False</c>.
        ''' </returns>
        Friend Function TryParseAlpha(arg As Object, ByRef alpha As Double) As Boolean
            alpha = 0.05

            If IsMissingArg(arg) Then Return True

            Try
                If TypeOf arg Is String Then
                    Dim s As String = Convert.ToString(arg).Trim()
                    If Not Double.TryParse(s, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, alpha) AndAlso
                       Not Double.TryParse(s, alpha) Then
                        Return False
                    End If
                Else
                    alpha = Convert.ToDouble(arg)
                End If
            Catch
                Return False
            End Try

            If Double.IsNaN(alpha) OrElse Double.IsInfinity(alpha) Then Return False
            If alpha <= 0.0 OrElse alpha >= 1.0 Then Return False

            Return True
        End Function

    End Module
End Namespace
