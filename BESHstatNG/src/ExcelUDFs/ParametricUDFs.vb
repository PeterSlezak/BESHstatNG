Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.equivalencetests
Imports BESHStatNG.Matrix
Imports ExcelDna.Integration

Namespace WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions exposing selected parametric ANOVA procedures.
    ''' </summary>
    Public Module ParametricUDFs

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
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/parametric/")>
        Public Function ANOVA1(
            <ExcelArgument(Name:="groups", Description:="Multi-column range; one column per group. First row may contain headers.")> groups As Object,
            <ExcelArgument(Name:="groupNames", Description:="Optional group names as comma-separated text or 1-row/1-column range.")> Optional groupNames As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetGroupedNumericColumns(groups, data, detectedNames) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If data Is Nothing OrElse data.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(groupNames, detectedNames, data.Length, "Group")
                Dim mdl As New parametric.OneWayANOVA(data, names)
                mdl.compute()
                Dim tables = mdl.wrapResults()
                Return PrepareResultTableForUdf(tables(0).returnSelf())
            Catch ex As Exception
                Return LoggedUdfError("BESH.PAR.ANOVA1", ex, ExcelError.ExcelErrorValue)
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
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/parametric/")>
        Public Function ANOVA1_WELCH(
            <ExcelArgument(Name:="groups", Description:="Multi-column range; one column per group. First row may contain headers.")> groups As Object,
            <ExcelArgument(Name:="groupNames", Description:="Optional group names as comma-separated text or 1-row/1-column range.")> Optional groupNames As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetGroupedNumericColumns(groups, data, detectedNames) Then Return ExcelError.ExcelErrorValue
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
            Catch ex As Exception
                Return LoggedUdfError("BESH.PAR.ANOVA1_WELCH", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

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
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/parametric/")>
        Public Function RMANOVA1(
            <ExcelArgument(Name:="data", Description:="Numeric matrix; rows=subjects, columns=conditions. First row may contain headers.")> data As Object,
            <ExcelArgument(Name:="conditionNames", Description:="Optional condition names as comma-separated text or 1-row/1-column range.")> Optional conditionNames As Object = Nothing,
            <ExcelArgument(Name:="correction", Description:="Optional: none/GG/HF/both.")> Optional correction As Object = Nothing
        ) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetCompleteNumericMatrixWithHeaders(data, mat, detectedNames) Then
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
            Catch ex As Exception
                Return LoggedUdfError("BESH.PAR.RMANOVA1", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

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
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/parametric/")>
        Public Function ANOVA2_NESTED(
            <ExcelArgument(Name:="data", Description:="Three columns: group, subgroup (nested), response. First row may contain headers.")> data As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional variable names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="output", Description:="Optional: both/main/satterthwaite.")> Optional outputType As Object = Nothing
        ) As Object
            Try
                Dim mat(,) As Object = Nothing
                Dim detectedNames() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetNestedThreeColumnData(data, mat, detectedNames) Then Return ExcelError.ExcelErrorValue
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
            Catch ex As Exception
                Return LoggedUdfError("BESH.PAR.ANOVA2_NESTED", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

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
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/parametric/")>
        Public Function ANOVA1_MCP(
            <ExcelArgument(Name:="groups", Description:="Multi-column range; one column per group. First row may contain headers.")> groups As Object,
            <ExcelArgument(Name:="groupNames", Description:="Optional group names as comma-separated text or 1-row/1-column range.")> Optional groupNames As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional: tukey/tukey-kramer (default), games-howell, lsd/fisher, bonferroni, or all.")> Optional method As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided significance level for confidence intervals. Default 0.05 (95% confidence interval).")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetGroupedNumericColumns(groups, data, detectedNames) Then
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
            Catch ex As Exception
                Return LoggedUdfError("BESH.PAR.ANOVA1_MCP", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

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
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/parametric/")>
        Public Function RMANOVA1_MCP(
            <ExcelArgument(Name:="data", Description:="Numeric matrix; rows=subjects, columns=conditions. First row may contain headers.")> data As Object,
            <ExcelArgument(Name:="conditionNames", Description:="Optional condition names as comma-separated text or 1-row/1-column range.")> Optional conditionNames As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional: rm2/tukeyrm2 (default), tukey/sphericity, or all.")> Optional method As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided significance level for confidence intervals. Default 0.05 (95% confidence interval).")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetCompleteNumericMatrixWithHeaders(data, mat, detectedNames) Then
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
            Catch ex As Exception
                Return LoggedUdfError("BESH.PAR.RMANOVA1_MCP", ex, ExcelError.ExcelErrorValue)
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
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/parametric/")>
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
                If Not Global.BESHStatNG.UdfDataImport.TryGetIndependentNumericColumns(x, y, data, detectedNames) Then
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
            Catch ex As Exception
                Return LoggedUdfError("BESH.PAR.TTEST_UNPAIRED", ex, ExcelError.ExcelErrorValue)
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
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/parametric/")>
        Public Function TTEST_PAIRED(
            <ExcelArgument(Name:="x", Description:="First paired sample as a single-column range. Values are paired by row; first cell may be a header.")> x As Object,
            <ExcelArgument(Name:="y", Description:="Second paired sample as a single-column range. Values are paired by row; first cell may be a header.")> y As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing
        ) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetPairedNumericColumns(x, y, mat, detectedNames) Then Return ExcelError.ExcelErrorValue
                If mat Is Nothing OrElse mat.GetLength(0) < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ResolveNames(varNames, detectedNames, 2, "Sample")
                Dim mdl As New parametric.PairedTtest(mat, names)
                mdl.compute()
                Dim tables = mdl.wrapResults()
                Return PrepareResultTableForUdf(tables(0).returnSelf())
            Catch ex As Exception
                Return LoggedUdfError("BESH.PAR.TTEST_PAIRED", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' One-sided non-inferiority comparison for two independent means.
        ''' </summary>
        ''' <param name="control">
        ''' Control or reference sample as a single-column Excel range.
        ''' Non-numeric cells are ignored. If the first cell looks like text, it is treated as a header and may be used
        ''' as the default display name of the control group.
        ''' </param>
        ''' <param name="experimental">
        ''' Experimental or test sample as a single-column Excel range.
        ''' Non-numeric cells are ignored. If the first cell looks like text, it is treated as a header and may be used
        ''' as the default display name of the experimental group.
        ''' </param>
        ''' <param name="margin">
        ''' Positive non-inferiority margin magnitude <c>M</c> on the difference scale.
        ''' The comparison is performed on <c>Δ = mean(experimental) - mean(control)</c>.
        ''' The null boundary is therefore <c>Δ ≤ -M</c> and the alternative is <c>Δ &gt; -M</c>.
        ''' </param>
        ''' <param name="groupNames">
        ''' Optional display names for the two groups, supplied either as a comma-separated string such as
        ''' <c>"Control,Test"</c> or as a one-row / one-column range with two names.
        ''' When omitted, names are taken from header cells when available.
        ''' </param>
        ''' <param name="method">
        ''' Optional variance assumption:
        ''' <c>welch</c> (default, unequal variances) or <c>equal</c>/<c>pooled</c>/<c>student</c>.
        ''' Welch’s method is usually preferred when sample sizes or variances differ.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional <b>one-sided</b> significance level. The default is <c>0.025</c>.
        ''' The function also reports the matching two-sided confidence interval with confidence level <c>1 - 2α</c>.
        ''' </param>
        ''' <returns>
        ''' A labeled spill table containing sample sizes, sample means, the mean difference
        ''' <c>mean(experimental) - mean(control)</c>, the non-inferiority limit <c>-M</c>,
        ''' the test statistic, one-sided p-value, lower one-sided confidence limit, the matching two-sided confidence interval,
        ''' and an interval-based decision summary.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function tests whether the experimental mean is not worse than the control mean by more than a prespecified amount.
        ''' On the difference scale <c>Δ = mean(experimental) - mean(control)</c>, a positive value favors the experimental group.
        ''' </para>
        ''' <para>
        ''' The non-inferiority hypotheses are
        ''' <c>H0: Δ ≤ -M</c> versus <c>H1: Δ &gt; -M</c>,
        ''' where <c>M &gt; 0</c> is the clinically or scientifically acceptable loss.
        ''' </para>
        ''' <para>
        ''' The test statistic is
        ''' <c>t = (Δ̂ + M) / SE(Δ̂)</c>,
        ''' evaluated either with Welch’s unequal-variance degrees of freedom or the pooled-variance Student t distribution.
        ''' The reported two-sided confidence interval uses confidence level <c>1 - 2α</c>. Non-inferiority is supported when
        ''' the lower confidence bound exceeds <c>-M</c> and, equivalently, when the one-sided p-value is at most <c>α</c>.
        ''' </para>
        ''' <para>
        ''' Missing or non-numeric cells are removed independently within each supplied group before the comparison is formed.
        ''' Each retained sample must contribute at least two usable numeric observations.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.PAR.TTEST_UNPAIRED_NI(A2:A21,B2:B19,0.5)
        ''' =BESH.PAR.TTEST_UNPAIRED_NI(A1:A21,B1:B19,1.25,"Control,Treatment","welch",0.025)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.PAR.TTEST_UNPAIRED_NI",
            Category:="BESHStatNG - Parametric",
            Description:="Non-inferiority comparison for two independent means with CI-based decision reporting.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/parametric/")>
        Public Function TTEST_UNPAIRED_NI(
            <ExcelArgument(AllowReference:=True, Name:="control", Description:="Control/reference group as a single-column range. First cell may be a header.")> control As Object,
            <ExcelArgument(AllowReference:=True, Name:="experimental", Description:="Experimental/test group as a single-column range. First cell may be a header.")> experimental As Object,
            <ExcelArgument(Name:="margin", Description:="Positive non-inferiority margin magnitude M. The null limit is -M on the experimental-minus-control scale.")> margin As Object,
            <ExcelArgument(Name:="groupNames", Description:="Optional names as comma-separated text or a 1-row/1-column range.")> Optional groupNames As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional variance assumption: welch (default) or equal/pooled/student.")> Optional method As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional one-sided alpha. Default 0.025.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetIndependentNumericColumns(control, experimental, data, detectedNames) Then Return ExcelError.ExcelErrorValue
                If data Is Nothing OrElse data.Length <> 2 Then Return ExcelError.ExcelErrorValue
                If data(0) Is Nothing OrElse data(1) Is Nothing Then Return ExcelError.ExcelErrorNum
                If data(0).Length < 2 OrElse data(1).Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim marginValue As Double
                If Not TryGetFiniteDouble(margin, marginValue) Then Return ExcelError.ExcelErrorValue
                If marginValue <= 0.0 Then Return ExcelError.ExcelErrorNum

                Dim alphaValue As Double = 0.025
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim assumeEqual As Boolean = ParseEqualVarianceMode(method, False)
                Dim names() As String = ParametricUDFs.ResolveNames(groupNames, detectedNames, 2, "Group")

                Dim result As MeanNonInferiorityResult = EquivalenceNonInferiorityMethods.TestUnpairedMeansNonInferiority(
                    data(0), data(1), marginValue, alphaValue, assumeEqual)

                Dim body As Object(,) = {
                    {"Control group", names(0)},
                    {"Experimental group", names(1)},
                    {"Variance model", If(result.AssumeEqualVariances, "Equal variances (pooled Student t)", "Unequal variances (Welch)")},
                    {"Observations in control", result.NumberOfControls},
                    {"Observations in experimental", result.NumberOfExperimental},
                    {"Mean control", result.MeanControl},
                    {"Mean experimental", result.MeanExperimental},
                    {"Difference (experimental - control)", result.DifferenceExperimentalMinusControl},
                    {"Standard error of the difference", result.StandardError},
                    {"Degrees of freedom", result.DegreesOfFreedom},
                    {"Non-inferiority margin magnitude", result.NonInferiorityMargin},
                    {"Null boundary on difference scale", result.NonInferiorityLimit},
                    {"One-sided alpha", result.AlphaOneSided},
                    {"Test statistic", result.TestStatistic},
                    {"One-sided p-value", result.PValue},
                    {"Lower one-sided confidence limit", result.LowerOneSidedConfidenceLimit},
                    {SafeCiLabel(result.TwoSidedEquivalentConfidenceInterval), SafeCiText(result.TwoSidedEquivalentConfidenceInterval)},
                    {"Point estimate within stated limits", result.CiAssessment.IsPointEstimateWithinMargins},
                    {"Confidence interval within stated limits", result.CiAssessment.IsConfidenceIntervalWithinMargins},
                    {"Lower-bound non-inferiority supported by CI", result.CiAssessment.SupportsLowerNonInferiority},
                    {"Upper-bound non-inferiority supported by CI", result.CiAssessment.SupportsUpperNonInferiority},
                    {"Conclusion", result.Conclusion}
                }

                Return BuildResultTable("Unpaired Means Non-Inferiority", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.PAR.TTEST_UNPAIRED_NI", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' TOST-style equivalence comparison for two independent means.
        ''' </summary>
        ''' <param name="control">Control or reference sample as a single-column Excel range.</param>
        ''' <param name="experimental">Experimental or test sample as a single-column Excel range.</param>
        ''' <param name="lowerMargin">
        ''' Lower equivalence margin on the difference scale <c>mean(experimental) - mean(control)</c>.
        ''' If <paramref name="upperMargin"/> is omitted, this argument is interpreted as a positive symmetric margin magnitude <c>M</c>
        ''' and the function uses margins <c>-M</c> and <c>+M</c>.
        ''' </param>
        ''' <param name="upperMargin">
        ''' Optional upper equivalence margin on the difference scale.
        ''' When supplied, the function uses the exact interval <c>[lowerMargin, upperMargin]</c>.
        ''' </param>
        ''' <param name="groupNames">Optional display names for the two groups.</param>
        ''' <param name="method">Optional variance assumption: <c>welch</c> (default) or <c>equal</c>/<c>pooled</c>/<c>student</c>.</param>
        ''' <param name="alpha">
        ''' Optional <b>one-sided</b> significance level for each TOST component. Default <c>0.025</c>.
        ''' The matching confidence interval therefore has confidence level <c>1 - 2α</c>.
        ''' </param>
        ''' <returns>
        ''' A labeled spill table containing the two one-sided test components, the combined TOST p-value,
        ''' the equivalence confidence interval, and the interval-based decision summary.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The Two One-Sided Tests (TOST) procedure assesses whether the true mean difference lies entirely inside a prespecified equivalence region.
        ''' On the difference scale <c>Δ = mean(experimental) - mean(control)</c>, equivalence is supported when both hypotheses are rejected:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description><c>H0,lower: Δ ≤ L</c> versus <c>H1,lower: Δ &gt; L</c></description></item>
        ''' <item><description><c>H0,upper: Δ ≥ U</c> versus <c>H1,upper: Δ &lt; U</c></description></item>
        ''' </list>
        ''' <para>
        ''' The function reports the lower and upper component statistics and p-values separately,
        ''' together with the TOST p-value <c>max(p_lower, p_upper)</c>.
        ''' It also returns the matched two-sided confidence interval with confidence level <c>1 - 2α</c>.
        ''' Equivalence is supported when this interval lies completely inside <c>[L, U]</c>.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.PAR.TTEST_UNPAIRED_EQUIV(A2:A21,B2:B19,0.5)
        ''' =BESH.PAR.TTEST_UNPAIRED_EQUIV(A2:A21,B2:B19,-1,1,"Control,Treatment","welch",0.025)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.PAR.TTEST_UNPAIRED_EQUIV",
            Category:="BESHStatNG - Parametric",
            Description:="TOST-style equivalence comparison for two independent means with interval-based decision reporting.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/parametric/")>
        Public Function TTEST_UNPAIRED_EQUIV(
            <ExcelArgument(AllowReference:=True, Name:="control", Description:="Control/reference group as a single-column range. First cell may be a header.")> control As Object,
            <ExcelArgument(AllowReference:=True, Name:="experimental", Description:="Experimental/test group as a single-column range. First cell may be a header.")> experimental As Object,
            <ExcelArgument(Name:="lowerMargin", Description:="Lower equivalence margin, or a positive symmetric margin magnitude if upperMargin is omitted.")> lowerMargin As Object,
            <ExcelArgument(Name:="upperMargin", Description:="Optional upper equivalence margin. When omitted, ±lowerMargin is used.")> Optional upperMargin As Object = Nothing,
            <ExcelArgument(Name:="groupNames", Description:="Optional names as comma-separated text or a 1-row/1-column range.")> Optional groupNames As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional variance assumption: welch (default) or equal/pooled/student.")> Optional method As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional one-sided alpha for each TOST component. Default 0.025.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim data()() As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetIndependentNumericColumns(control, experimental, data, detectedNames) Then Return ExcelError.ExcelErrorValue
                If data Is Nothing OrElse data.Length <> 2 Then Return ExcelError.ExcelErrorValue
                If data(0) Is Nothing OrElse data(1) Is Nothing Then Return ExcelError.ExcelErrorNum
                If data(0).Length < 2 OrElse data(1).Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim lowerValue As Double
                Dim upperValue As Double
                If Not TryGetEquivalenceMargins(lowerMargin, upperMargin, lowerValue, upperValue) Then Return ExcelError.ExcelErrorValue
                If lowerValue >= upperValue Then Return ExcelError.ExcelErrorNum

                Dim alphaValue As Double = 0.025
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim assumeEqual As Boolean = ParseEqualVarianceMode(method, False)
                Dim names() As String = ParametricUDFs.ResolveNames(groupNames, detectedNames, 2, "Group")

                Dim result As MeanEquivalenceResult = EquivalenceNonInferiorityMethods.TestUnpairedMeansEquivalence(
                    data(0), data(1), lowerValue, upperValue, alphaValue, assumeEqual)

                Dim body As Object(,) = {
                    {"Control group", names(0)},
                    {"Experimental group", names(1)},
                    {"Variance model", If(result.AssumeEqualVariances, "Equal variances (pooled Student t)", "Unequal variances (Welch)")},
                    {"Observations in control", result.NumberOfControls},
                    {"Observations in experimental", result.NumberOfExperimental},
                    {"Mean control", result.MeanControl},
                    {"Mean experimental", result.MeanExperimental},
                    {"Difference (experimental - control)", result.DifferenceExperimentalMinusControl},
                    {"Standard error of the difference", result.StandardError},
                    {"Degrees of freedom", result.DegreesOfFreedom},
                    {"Lower equivalence margin", result.LowerMargin},
                    {"Upper equivalence margin", result.UpperMargin},
                    {"One-sided alpha", result.AlphaOneSided},
                    {"Lower TOST statistic", result.LowerComponentStatistic},
                    {"Lower TOST p-value", result.LowerComponentPValue},
                    {"Upper TOST statistic", result.UpperComponentStatistic},
                    {"Upper TOST p-value", result.UpperComponentPValue},
                    {"TOST p-value = max(component p-values)", result.TostPValue},
                    {SafeCiLabel(result.EquivalentConfidenceInterval), SafeCiText(result.EquivalentConfidenceInterval)},
                    {"Point estimate within margins", result.CiAssessment.IsPointEstimateWithinMargins},
                    {"Confidence interval within margins", result.CiAssessment.IsConfidenceIntervalWithinMargins},
                    {"Lower margin supported by CI", result.CiAssessment.SupportsLowerNonInferiority},
                    {"Upper margin supported by CI", result.CiAssessment.SupportsUpperNonInferiority},
                    {"Supports equivalence", result.SupportsEquivalence},
                    {"Conclusion", result.Conclusion}
                }

                Return BuildResultTable("Unpaired Means Equivalence (TOST)", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.PAR.TTEST_UNPAIRED_EQUIV", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Private/Friend helpers
        ' -------------------------------------------------------------------------------------------------------------

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
                Dim names = Global.BESHStatNG.UdfDataImport.GetVariableNames(explicitNames, expectedCount)
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

        Private Function ParseEqualVarianceMode(arg As Object, defaultEqual As Boolean) As Boolean
            Dim s As String = NormalizeToken(arg)
            If s = "" Then Return defaultEqual
            Select Case s
                Case "WELCH", "UNEQUAL", "ASSUMEUNEQUAL", "ASSUME-UNEQUAL"
                    Return False
                Case "EQUAL", "POOLED", "STUDENT", "ASSUMEEQUAL", "ASSUME-EQUAL"
                    Return True
                Case Else
                    Throw New ArgumentException("Unsupported variance method. Use welch or equal/pooled/student.")
            End Select
        End Function

    End Module
End Namespace
