Option Explicit On
Option Strict On

Imports System
Imports ExcelDna.Integration
Imports BESHStatNG.contingencytable
Imports BESHStatNG.AppInfrastructure

Namespace WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for categorical-data and contingency-table analysis.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These worksheet functions analyze count tables whose cells contain observed frequencies.
    ''' A contingency table cross-classifies two categorical variables, so cell <c>(i,j)</c>
    ''' represents the number of observations falling simultaneously in row category <c>i</c>
    ''' and column category <c>j</c>.
    ''' </para>
    ''' <para>
    ''' Depending on the scientific question, the add-in exposes several complementary procedures:
    ''' goodness-of-fit style association tests for general <c>r×c</c> tables,
    ''' exact procedures for small or sparse tables,
    ''' effect-size summaries such as odds ratios, risk ratios, Cramér's <c>V</c>, and ordinal association coefficients,
    ''' linear-trend tests for ordered rows, and common-effect estimation across multiple stratified <c>2×2</c> tables.
    ''' </para>
    ''' <para>
    ''' All worksheet functions in this module accept a numeric matrix of non-negative counts.
    ''' A single top header row containing non-numeric labels is skipped automatically when present.
    ''' Embedded row-label columns are not supported; the body of the supplied range must contain only cell counts.
    ''' </para>
    ''' </remarks>
    Public Module ContingencyTableUDFs

        ''' <summary>
        ''' Pearson chi-square test of independence for an <c>r×c</c> contingency table.
        ''' </summary>
        ''' <param name="table">
        ''' A numeric matrix of non-negative cell counts.
        ''' Rows represent categories of one variable and columns represent categories of the second variable.
        ''' An optional single header row containing non-numeric labels is allowed and will be ignored.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the Pearson chi-square statistic,
        ''' the associated degrees of freedom,
        ''' and the upper-tail p-value from the chi-square distribution.
        ''' Returns <c>#VALUE!</c> when the input is not a valid non-negative integer matrix.
        ''' Returns <c>#NUM!</c> when the table has fewer than two rows or fewer than two columns,
        ''' when the total count is zero,
        ''' or when the statistic cannot be evaluated numerically.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function tests the null hypothesis that row membership and column membership are statistically independent.
        ''' Let <c>O_ij</c> denote the observed count in cell <c>(i,j)</c>,
        ''' let <c>R_i</c> and <c>C_j</c> denote the row and column totals,
        ''' and let <c>N</c> denote the grand total.
        ''' Under independence the expected count is
        ''' <c>E_ij = R_i C_j / N</c>.
        ''' The test statistic is
        ''' <c>X² = Σ_ij (O_ij - E_ij)² / E_ij</c>.
        ''' </para>
        ''' <para>
        ''' The reported p-value is based on the asymptotic chi-square reference distribution with
        ''' <c>(r-1)(c-1)</c> degrees of freedom after excluding structurally empty all-zero rows or columns.
        ''' The approximation is most reliable when expected counts are not too small.
        ''' For sparse tables or very small samples, consider the exact Fisher-Freeman-Halton procedure instead.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.CHI2(A1:C4)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.CHI2",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Pearson chi-square test of independence for an r×c contingency table.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency-tables/")>
        Public Function CT_CHI2(
            <ExcelArgument(Name:="table", Description:="Numeric contingency table of non-negative counts. A single top header row may be included.")> table As Object
        ) As Object
            Try
                Dim tab(,) As Integer = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetContingencyTable(table, tab) Then Return ExcelError.ExcelErrorValue
                If tab.GetLength(0) < 2 OrElse tab.GetLength(1) < 2 Then Return ExcelError.ExcelErrorNum
                If TableTotal(tab) <= 0 Then Return ExcelError.ExcelErrorNum

                Dim res = contingencytable.Chi2TESTindependence(tab)
                If res.Item1 Is Nothing OrElse Not IsFinite(res.Item1.TestStatistics1) OrElse Not IsFinite(res.Item1.Pvalue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim df As Integer = EffectiveDf(tab)
                Dim body As Object(,) = {
                    {"Chi-square", res.Item1.TestStatistics1},
                    {"Degrees of freedom", df},
                    {"Two-sided p-value", res.Item1.Pvalue}
                }
                Return BuildResultTable("Pearson's Chi-squared Test", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.CHI2", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Measures of nominal association for an <c>r×c</c> contingency table.
        ''' </summary>
        ''' <param name="table">
        ''' A numeric matrix of non-negative cell counts.
        ''' Rows and columns are treated as nominal categories, so only the pattern of cell frequencies matters and no ordering is assumed.
        ''' An optional single top header row containing non-numeric labels is allowed and will be ignored.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing Cramér's <c>V</c>, Pearson's contingency coefficient,
        ''' and the Phi coefficient.
        ''' Returns <c>#VALUE!</c> when the input is not a valid non-negative integer matrix.
        ''' Returns <c>#NUM!</c> when the table is too small or has zero total count.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' These quantities are effect-size summaries derived from the Pearson chi-square statistic.
        ''' If <c>X²</c> is the chi-square statistic and <c>N</c> is the grand total, then
        ''' <c>Phi = √(X²/N)</c>,
        ''' <c>V = √(X² / [N·min(r-1,c-1)])</c>,
        ''' and
        ''' Pearson's contingency coefficient is <c>C = √(X² / (X² + N))</c>.
        ''' </para>
        ''' <para>
        ''' Cramér's <c>V</c> is usually preferred for general rectangular tables because it rescales the chi-square statistic to the unit interval.
        ''' The Phi coefficient equals the absolute value of the correlation coefficient only for <c>2×2</c> tables.
        ''' These measures summarize association strength but do not indicate direction for nominal variables.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.NOMINAL_ASSOC(A1:C4)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.NOMINAL_ASSOC",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Cramér's V, Pearson's contingency coefficient, and Phi for an r×c table.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency-tables/")>
        Public Function CT_NOMINAL_ASSOC(
            <ExcelArgument(Name:="table", Description:="Numeric contingency table of non-negative counts. A single top header row may be included.")> table As Object
        ) As Object
            Try
                Dim tab(,) As Integer = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetContingencyTable(table, tab) Then Return ExcelError.ExcelErrorValue
                If tab.GetLength(0) < 2 OrElse tab.GetLength(1) < 2 Then Return ExcelError.ExcelErrorNum
                If TableTotal(tab) <= 0 Then Return ExcelError.ExcelErrorNum

                Dim res = contingencytable.Chi2TESTindependence(tab)
                If res.Item1 Is Nothing Then Return ExcelError.ExcelErrorNum

                Dim body As Object(,) = {
                    {"Cramer's V", res.Item2},
                    {"Pearson's contingency coefficient", res.Item3},
                    {"Phi", res.Item4}
                }
                Return BuildResultTable("Measures of Nominal Association", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.NOMINAL_ASSOC", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Fisher's exact test for a <c>2×2</c> contingency table.
        ''' </summary>
        ''' <param name="table">
        ''' A <c>2×2</c> matrix of non-negative counts.
        ''' The cells are interpreted as
        ''' <c>a = table(1,1)</c>, <c>b = table(1,2)</c>, <c>c = table(2,1)</c>, and <c>d = table(2,2)</c>.
        ''' An optional single top header row containing non-numeric labels is allowed and will be ignored.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing one-sided and two-sided exact p-values,
        ''' together with the corresponding mid-p versions.
        ''' Returns <c>#VALUE!</c> when the supplied range is not a valid <c>2×2</c> count table.
        ''' Returns <c>#NUM!</c> when the exact probabilities cannot be evaluated numerically.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Conditional on the fixed row and column totals,
        ''' the upper-left cell of a <c>2×2</c> table follows a hypergeometric distribution under the null hypothesis of independence.
        ''' Fisher's exact procedure sums the probabilities of all tables at least as extreme as the observed table,
        ''' thereby avoiding the large-sample chi-square approximation.
        ''' </para>
        ''' <para>
        ''' The mid-p values subtract one half of the observed-table probability before doubling or tail summation.
        ''' Mid-p procedures are often less conservative than fully exact p-values,
        ''' but they are no longer guaranteed to control the type-I error rate in the strict conditional-exact sense.
        ''' </para>
        ''' <para>
        ''' This function is appropriate when sample sizes are small,
        ''' expected counts are sparse,
        ''' or exact conditional inference is preferred for a <c>2×2</c> design.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.FISHER_2X2(A1:B3)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.FISHER_2X2",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Fisher's exact test for a 2×2 contingency table, including mid-p values.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency-tables/")>
        Public Function CT_FISHER_2X2(
            <ExcelArgument(Name:="table", Description:="2×2 contingency table of non-negative counts. A single top header row may be included.")> table As Object
        ) As Object
            Try
                Dim tab(,) As Integer = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetContingencyTable(table, tab) Then Return ExcelError.ExcelErrorValue
                If Not Is2x2(tab) Then Return ExcelError.ExcelErrorValue

                Dim res As TestResult = contingencytable.FisherExact2x2(tab(0, 0), tab(0, 1), tab(1, 0), tab(1, 1))
                If res Is Nothing OrElse Not IsFinite(res.Pvalue) Then Return ExcelError.ExcelErrorNum

                Dim body As Object(,) = {
                    {"One-sided exact p-value", res.PvalueLowerSide},
                    {"Two-sided exact p-value", res.Pvalue},
                    {"One-sided mid-p value", res.pValueExactLowerSide},
                    {"Two-sided mid-p value", res.Pvalue2}
                }
                Return BuildResultTable("Fisher's Exact Test", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.FISHER_2X2", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Fisher-Freeman-Halton exact test for a general <c>r×c</c> contingency table.
        ''' </summary>
        ''' <param name="table">
        ''' A numeric matrix of non-negative cell counts with at least two rows and two columns.
        ''' An optional single top header row containing non-numeric labels is allowed and will be ignored.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the observed-table probability under the conditional null distribution
        ''' and the exact two-sided p-value.
        ''' Returns <c>#VALUE!</c> when the input is not a valid count matrix.
        ''' Returns <c>#NUM!</c> when the exact network calculation fails because the table is too large or too sparse for the available workspace.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This procedure generalizes Fisher's exact test from <c>2×2</c> tables to arbitrary fixed-margin <c>r×c</c> tables.
        ''' Conditional on the observed row and column margins,
        ''' the null distribution assigns probability proportional to
        ''' <c>1 / Π_ij O_ij!</c>
        ''' up to a margin-dependent normalizing constant.
        ''' The exact p-value sums the probabilities of all feasible tables whose conditional probability is no greater than that of the observed table.
        ''' </para>
        ''' <para>
        ''' Because the number of feasible tables can be very large,
        ''' the calculation uses a network-style enumeration algorithm rather than naive brute-force generation.
        ''' Exact inference is particularly useful when asymptotic chi-square approximations are doubtful because expected counts are small.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.FFH_EXACT(A1:D5)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.FFH_EXACT",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Fisher-Freeman-Halton exact test for a general r×c contingency table.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency-tables/")>
        Public Function CT_FFH_EXACT(
            <ExcelArgument(Name:="table", Description:="r×c contingency table of non-negative counts. A single top header row may be included.")> table As Object
        ) As Object
            Try
                Dim tab(,) As Integer = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetContingencyTable(table, tab) Then Return ExcelError.ExcelErrorValue
                If tab.GetLength(0) < 2 OrElse tab.GetLength(1) < 2 Then Return ExcelError.ExcelErrorNum
                If TableTotal(tab) <= 0 Then Return ExcelError.ExcelErrorNum

                Dim exact As New FisherExactEngine(tab)
                exact.Run()

                If Not IsFinite(exact.PObserved) OrElse Not IsFinite(exact.PValue) Then Return ExcelError.ExcelErrorNum

                Dim body As Object(,) = {
                    {"Observed-table probability", exact.PObserved},
                    {"Two-sided exact p-value", exact.PValue}
                }
                Return BuildResultTable("Fisher-Freeman-Halton Exact Test", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.FFH_EXACT", ex, ExcelError.ExcelErrorNum)
            End Try
        End Function

        ''' <summary>
        ''' Exact paired <c>2×2</c> analysis using the McNemar/Liddell framework.
        ''' </summary>
        ''' <param name="table">
        ''' A <c>2×2</c> matched-pairs table of non-negative counts.
        ''' The off-diagonal cells are the discordant pairs and drive both the exact p-value and the matched-pairs odds ratio.
        ''' An optional single top header row containing non-numeric labels is allowed and will be ignored.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level for the confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% interval.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the exact two-sided p-value,
        ''' the matched-pairs odds-ratio estimate,
        ''' and its confidence interval.
        ''' Returns <c>#VALUE!</c> when the supplied range is not a valid <c>2×2</c> count table.
        ''' Returns <c>#NUM!</c> when <paramref name="alpha"/> is not strictly between 0 and 1,
        ''' or when the quantities cannot be evaluated numerically.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' For paired binary outcomes, only the discordant counts contribute information about marginal change.
        ''' If the table is written as
        ''' <c>[[a,b],[c,d]]</c>,
        ''' then the exact paired null hypothesis is assessed through the discordant counts <c>b</c> and <c>c</c>.
        ''' The exact p-value is a McNemar-type conditional test,
        ''' while the reported effect estimate is the matched-pairs odds ratio <c>b/c</c>.
        ''' </para>
        ''' <para>
        ''' The confidence interval is derived from exact finite-sample arguments based on <c>F</c>-distribution quantiles.
        ''' When one discordant cell is zero,
        ''' the interval may become one-sided with an infinite upper or lower bound,
        ''' reflecting the fact that the matched-pairs odds ratio is not bounded on both sides by the data.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.MCNEMAR_EXACT(A1:B3)
        ''' =BESH.CT.MCNEMAR_EXACT(A1:B3, 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.MCNEMAR_EXACT",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Exact paired 2×2 analysis: McNemar/Liddell p-value and matched-pairs odds-ratio interval.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency-tables/")>
        Public Function CT_MCNEMAR_EXACT(
            <ExcelArgument(Name:="table", Description:="Paired 2×2 contingency table of non-negative counts. A single top header row may be included.")> table As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level for the confidence interval. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim tab(,) As Integer = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetContingencyTable(table, tab) Then Return ExcelError.ExcelErrorValue
                If Not Is2x2(tab) Then Return ExcelError.ExcelErrorValue

                Dim alphaValue As Double = GetOptionalDouble(alpha, 0.05R)
                If alphaValue <= 0.0R OrElse alphaValue >= 1.0R Then Return ExcelError.ExcelErrorNum

                Dim res = contingencytable.Liddell_McNemar(tab, alphaValue)
                If res.Item1 Is Nothing OrElse res.Item2 Is Nothing OrElse Not IsFinite(res.Item1.Pvalue) Then Return ExcelError.ExcelErrorNum

                Dim ciText As String = If(String.IsNullOrWhiteSpace(res.Item2.strConfidenceInterval(CIformat.LL_to_UL)), "", res.Item2.strConfidenceInterval(CIformat.LL_to_UL))
                Dim body As Object(,) = {
                    {"Two-sided exact p-value", res.Item1.Pvalue},
                    {"Matched-pairs odds ratio", res.Item2.Estimate},
                    {res.Item2.CIlabel, ciText}
                }
                Return BuildResultTable("Exact Paired 2×2 Analysis", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.MCNEMAR_EXACT", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Odds ratio for an independent <c>2×2</c> contingency table,
        ''' with both large-sample and exact-style confidence intervals.
        ''' </summary>
        ''' <param name="table">
        ''' A <c>2×2</c> matrix of non-negative counts in the layout
        ''' <c>[[a,b],[c,d]]</c>.
        ''' An optional single top header row containing non-numeric labels is allowed and will be ignored.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level for the confidence intervals.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the odds-ratio estimate,
        ''' a Woolf log-normal confidence interval,
        ''' and a Cornfield confidence interval.
        ''' Returns <c>#VALUE!</c> when the supplied range is not a valid <c>2×2</c> count table.
        ''' Returns <c>#NUM!</c> when <paramref name="alpha"/> is invalid or when the estimates are not numerically defined.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' For a <c>2×2</c> table written as
        ''' <c>[[a,b],[c,d]]</c>,
        ''' the odds ratio is
        ''' <c>OR = ad/(bc)</c>.
        ''' It compares the odds of the row-1 outcome across the two column groups,
        ''' or equivalently the odds of the column-1 outcome across the two row groups.
        ''' Values greater than 1 indicate positive association and values smaller than 1 indicate negative association.
        ''' </para>
        ''' <para>
        ''' The Woolf interval uses the normal approximation on the log scale:
        ''' <c>log(OR) ± z_{1-α/2} · √(1/a + 1/b + 1/c + 1/d)</c>.
        ''' The Cornfield interval is more exact in spirit and is obtained by inverting the conditional test using iterative calculations.
        ''' </para>
        ''' <para>
        ''' The odds ratio is the natural effect measure for case-control studies and for logistic-type modeling,
        ''' whereas the risk ratio is usually more interpretable in prospective cohort settings.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.ODDS_RATIO(A1:B3)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.ODDS_RATIO",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Odds ratio for a 2×2 table with Woolf and Cornfield confidence intervals.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency-tables/")>
        Public Function CT_ODDS_RATIO(
            <ExcelArgument(Name:="table", Description:="2×2 contingency table of non-negative counts. A single top header row may be included.")> table As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level for the confidence intervals. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim tab(,) As Integer = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetContingencyTable(table, tab) Then Return ExcelError.ExcelErrorValue
                If Not Is2x2(tab) Then Return ExcelError.ExcelErrorValue

                Dim alphaValue As Double = GetOptionalDouble(alpha, 0.05R)
                If alphaValue <= 0.0R OrElse alphaValue >= 1.0R Then Return ExcelError.ExcelErrorNum

                Dim res = contingencytable.OddsRatio(tab, alphaValue)
                If res.Item1 Is Nothing OrElse res.Item2 Is Nothing OrElse Not IsFinite(res.Item1.Estimate) Then Return ExcelError.ExcelErrorNum

                Dim body As Object(,) = {
                    {"Odds ratio", res.Item1.Estimate},
                    {res.Item1.CIlabel & " (Woolf)", res.Item1.strConfidenceInterval(CIformat.LL_to_UL)},
                    {res.Item2.CIlabel & " (Cornfield)", res.Item2.strConfidenceInterval(CIformat.LL_to_UL)}
                }
                Return BuildResultTable("Odds Ratio", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.ODDS_RATIO", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Risk ratio (relative risk) for an independent <c>2×2</c> contingency table.
        ''' </summary>
        ''' <param name="table">
        ''' A <c>2×2</c> matrix of non-negative counts in the layout
        ''' <c>[[a,b],[c,d]]</c>,
        ''' where the first column corresponds to event counts and the second column to non-event counts.
        ''' An optional single top header row containing non-numeric labels is allowed and will be ignored.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level for the confidence interval.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the risk-ratio estimate and its confidence interval.
        ''' Returns <c>#VALUE!</c> when the supplied range is not a valid <c>2×2</c> count table.
        ''' Returns <c>#NUM!</c> when <paramref name="alpha"/> is invalid or when the estimate is not numerically defined.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' For a table written as
        ''' <c>[[a,b],[c,d]]</c>,
        ''' the reported quantity is
        ''' <c>RR = [a/(a+c)] / [b/(b+d)]</c>
        ''' according to the orientation used by the current add-in implementation.
        ''' Therefore the interpretation depends on how event and comparison groups are laid out in the worksheet.
        ''' </para>
        ''' <para>
        ''' Confidence limits are computed on the log scale using the large-sample approximation
        ''' <c>log(RR) ± z_{1-α/2}·SE</c>,
        ''' with
        ''' <c>SE = √[ c/(a(a+c)) + d/(b(b+d)) ]</c>
        ''' under the implemented cell ordering.
        ''' </para>
        ''' <para>
        ''' The risk ratio is often easier to communicate than the odds ratio in cohort or prospective settings,
        ''' but unlike the odds ratio it is not invariant to transposing the event/non-event orientation of the table.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.RISK_RATIO(A1:B3)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.RISK_RATIO",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Risk ratio (relative risk) for a 2×2 contingency table.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency-tables/")>
        Public Function CT_RISK_RATIO(
            <ExcelArgument(Name:="table", Description:="2×2 contingency table of non-negative counts. A single top header row may be included.")> table As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level for the confidence interval. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim tab(,) As Integer = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetContingencyTable(table, tab) Then Return ExcelError.ExcelErrorValue
                If Not Is2x2(tab) Then Return ExcelError.ExcelErrorValue

                Dim alphaValue As Double = GetOptionalDouble(alpha, 0.05R)
                If alphaValue <= 0.0R OrElse alphaValue >= 1.0R Then Return ExcelError.ExcelErrorNum

                Dim res As ConfidenceIntervalResult = contingencytable.RiskRatio(tab, alphaValue)
                If res Is Nothing OrElse Not IsFinite(res.Estimate) Then Return ExcelError.ExcelErrorNum

                Dim body As Object(,) = {
                    {"Risk ratio", res.Estimate},
                    {res.CIlabel, res.strConfidenceInterval(CIformat.LL_to_UL)}
                }
                Return BuildResultTable("Risk Ratio", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.RISK_RATIO", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Cochran-Armitage test for linear trend in proportions across ordered groups.
        ''' </summary>
        ''' <param name="table">
        ''' A count table with one dimension of length 2.
        ''' The function accepts either an <c>r×2</c> table or a <c>2×c</c> table;
        ''' if two rows are supplied the table is transposed automatically so that ordered groups run down the rows.
        ''' An optional single top header row containing non-numeric labels is allowed and will be ignored.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the chi-square statistic for linear trend,
        ''' its p-value,
        ''' the residual chi-square for departure from linearity,
        ''' and the corresponding p-value.
        ''' Returns <c>#VALUE!</c> when the input is not a valid count matrix.
        ''' Returns <c>#NUM!</c> when neither dimension equals 2,
        ''' when the table is only <c>2×2</c>,
        ''' or when the statistic cannot be evaluated numerically.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This test is designed for binary outcomes observed across ordered exposure groups,
        ''' such as increasing dose levels or ordered severity categories.
        ''' It tests whether the event probability changes linearly with the ordered group score.
        ''' </para>
        ''' <para>
        ''' If the rows are indexed by scores <c>w_i</c> (here taken as <c>0,1,2,…</c>),
        ''' the trend component is a one-degree-of-freedom quadratic form based on the covariance between the group scores and the observed successes.
        ''' The remaining lack-of-fit against a purely linear trend is reported as a second chi-square statistic with
        ''' <c>r-2</c> degrees of freedom.
        ''' </para>
        ''' <para>
        ''' The procedure is more powerful than the full Pearson chi-square test when the scientific alternative is specifically monotone or approximately linear across ordered groups.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.TREND(A1:B5)
        ''' =BESH.CT.TREND(A1:E2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.TREND",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Cochran-Armitage test for linear trend in proportions across ordered groups.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency-tables/")>
        Public Function CT_TREND(
            <ExcelArgument(Name:="table", Description:="Count table with one dimension equal to 2. A single top header row may be included.")> table As Object
        ) As Object
            Try
                Dim tab(,) As Integer = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetContingencyTable(table, tab) Then Return ExcelError.ExcelErrorValue

                Dim rows As Integer = tab.GetLength(0)
                Dim cols As Integer = tab.GetLength(1)
                If rows < 2 OrElse cols < 2 Then Return ExcelError.ExcelErrorNum
                If Not ((rows = 2) OrElse (cols = 2)) OrElse (rows + cols < 5) Then Return ExcelError.ExcelErrorNum

                Dim trendTable(,) As Integer = tab
                If rows = 2 AndAlso cols > 2 Then
                    trendTable = TransposeIntMatrix(tab)
                End If

                Dim res As TestResult = contingencytable.CochranArmitage(trendTable)
                If res Is Nothing OrElse Not IsFinite(res.TestStatistics1) OrElse Not IsFinite(res.Pvalue) Then Return ExcelError.ExcelErrorNum

                Dim body As Object(,) = {
                    {"Chi-square for linear trend", res.TestStatistics1},
                    {"Two-sided p-value for linear trend", res.Pvalue},
                    {"Chi-square for departure from linear trend", res.TestStatistics2},
                    {"Two-sided p-value for departure", res.Pvalue2}
                }
                Return BuildResultTable("Cochran-Armitage Test for Linear Trend", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.TREND", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Ordinal association measures for an ordered contingency table.
        ''' </summary>
        ''' <param name="table">
        ''' A numeric matrix of non-negative cell counts whose row and column categories are both intrinsically ordered.
        ''' An optional single top header row containing non-numeric labels is allowed and will be ignored.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level used for the reported confidence intervals.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing Kendall's tau-b,
        ''' Kendall's tau-c,
        ''' Goodman-Kruskal's gamma,
        ''' and Somers' <c>D</c> (columns treated as the dependent ordering),
        ''' together with their standard errors,
        ''' confidence intervals,
        ''' and two-sided p-values.
        ''' Returns <c>#VALUE!</c> when the input is not a valid count matrix.
        ''' Returns <c>#NUM!</c> when <paramref name="alpha"/> is invalid or when the statistics cannot be evaluated numerically.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' These measures compare concordant and discordant pairs of observations extracted from the ordered table.
        ''' A pair is concordant when the observation that is higher on the row ordering is also higher on the column ordering;
        ''' it is discordant when the two orderings disagree.
        ''' Tied pairs may be handled differently depending on the measure.
        ''' </para>
        ''' <para>
        ''' Kendall's tau-b adjusts for ties in both margins,
        ''' tau-c rescales association for rectangular tables,
        ''' Goodman-Kruskal's gamma ignores ties entirely,
        ''' and Somers' <c>D</c> is asymmetric because it conditions on one variable being treated as the response ordering.
        ''' </para>
        ''' <para>
        ''' The reported confidence intervals use normal approximations of the form
        ''' <c>estimate ± z_{1-α/2}·SE</c>.
        ''' These summaries are meaningful only when the category order is substantively important;
        ''' for purely nominal tables use the nominal-association UDF instead.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.ORDINAL_ASSOC(A1:D4)
        ''' =BESH.CT.ORDINAL_ASSOC(A1:D4, 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.ORDINAL_ASSOC",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Ordinal association measures: Kendall tau-b, tau-c, gamma, and Somers' D.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency-tables/")>
        Public Function CT_ORDINAL_ASSOC(
            <ExcelArgument(Name:="table", Description:="Ordered contingency table of non-negative counts. A single top header row may be included.")> table As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level for the confidence intervals. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim tab(,) As Integer = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetContingencyTable(table, tab) Then Return ExcelError.ExcelErrorValue
                If tab.GetLength(0) < 2 OrElse tab.GetLength(1) < 2 Then Return ExcelError.ExcelErrorNum

                Dim alphaValue As Double = GetOptionalDouble(alpha, 0.05R)
                If alphaValue <= 0.0R OrElse alphaValue >= 1.0R Then Return ExcelError.ExcelErrorNum

                Dim res = contingencytable.cTableORDINALassoc(tab, alphaValue)
                If res.Item1 Is Nothing OrElse Not IsFinite(res.Item1.TestStatistics1) OrElse Not IsFinite(res.Item1.Pvalue) Then Return ExcelError.ExcelErrorNum

                Dim ciLabel As String = $"{(1.0R - alphaValue) * 100.0R:0.##}% CI"
                Dim body As Object(,) = {
                    {"Kendall's tau-b", res.Item1.TestStatistics1},
                    {"Std.Err.", res.Item1.DF1},
                    {ciLabel, res.Item1.strSpecialInformation},
                    {"Two-sided p-value", res.Item1.Pvalue},
                    {"Kendall's tau-c", res.Item2.TestStatistics1},
                    {"Std.Err.", res.Item2.DF1},
                    {ciLabel, res.Item2.strSpecialInformation},
                    {"Two-sided p-value", res.Item2.Pvalue},
                    {"Goodman-Kruskal's gamma", res.Item3.TestStatistics1},
                    {"Std.Err.", res.Item3.DF1},
                    {ciLabel, res.Item3.strSpecialInformation},
                    {"Two-sided p-value", res.Item3.Pvalue},
                    {"Somers' D (columns as dependent variable)", res.Item4.TestStatistics1},
                    {"Std.Err.", res.Item4.DF1},
                    {ciLabel, res.Item4.strSpecialInformation},
                    {"Two-sided p-value", res.Item4.Pvalue}
                }
                Return BuildResultTable("Measures of Ordinal Association", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.ORDINAL_ASSOC", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Mantel-Haenszel pooled analysis across multiple stratified <c>2×2</c> tables.
        ''' </summary>
        ''' <param name="stackedTables">
        ''' A numeric matrix with exactly two columns and an even number of rows.
        ''' Every consecutive pair of rows represents one stratum-specific <c>2×2</c> table in the form
        ''' <c>[a,b]</c> on the first row and <c>[c,d]</c> on the second row.
        ''' An optional single top header row containing non-numeric labels is allowed and will be ignored.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level for the pooled common-odds-ratio confidence interval.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the Mantel-Haenszel chi-square statistic,
        ''' its p-value,
        ''' the pooled common odds ratio,
        ''' and a confidence interval for that pooled effect.
        ''' Returns <c>#VALUE!</c> when the input does not have the required stacked two-column layout.
        ''' Returns <c>#NUM!</c> when <paramref name="alpha"/> is invalid or when the quantities cannot be evaluated numerically.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Suppose there are strata <c>k = 1,…,K</c>, each contributing a <c>2×2</c> table with cells <c>a_k,b_k,c_k,d_k</c> and total <c>n_k</c>.
        ''' The Mantel-Haenszel approach combines the stratum-specific information while conditioning on the stratum margins.
        ''' The common odds-ratio estimator is
        ''' <c>OR_MH = [Σ_k a_k d_k / n_k] / [Σ_k b_k c_k / n_k]</c>.
        ''' </para>
        ''' <para>
        ''' The accompanying chi-square statistic is a one-degree-of-freedom test of the null hypothesis that the common odds ratio equals 1 across strata.
        ''' It is commonly used for stratified case-control data or when combining several matched <c>2×2</c> tables while adjusting for a categorical confounder.
        ''' </para>
        ''' <para>
        ''' If any stratum contains a zero cell,
        ''' a small continuity adjustment is applied internally before effect-size estimation to stabilize the pooled odds-ratio calculations.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.MANTEL_HAENSZEL(A1:B7)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.MANTEL_HAENSZEL",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Mantel-Haenszel pooled test and common odds ratio across stacked 2×2 strata.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency-tables/")>
        Public Function CT_MANTEL_HAENSZEL(
            <ExcelArgument(Name:="stackedTables", Description:="Two-column stacked 2×2 tables: each pair of rows is one stratum [a,b] / [c,d].")> stackedTables As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level for the pooled odds-ratio interval. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim rows As Integer = 0
                Dim cols As Integer = 0
                If Not Global.BESHStatNG.UdfDataImport.TryGetNumericMatrix(stackedTables, mat, rows, cols) Then Return ExcelError.ExcelErrorValue
                If cols <> 2 OrElse rows < 2 OrElse (rows Mod 2) <> 0 Then Return ExcelError.ExcelErrorValue

                For i As Integer = 0 To rows - 1
                    For j As Integer = 0 To cols - 1
                        Dim x As Double = mat(i, j)
                        If Double.IsNaN(x) OrElse Double.IsInfinity(x) OrElse x < 0.0R Then Return ExcelError.ExcelErrorValue
                        If Math.Abs(x - Math.Round(x)) > 0.0000001R Then Return ExcelError.ExcelErrorValue
                    Next
                Next

                Dim alphaValue As Double = GetOptionalDouble(alpha, 0.05R)
                If alphaValue <= 0.0R OrElse alphaValue >= 1.0R Then Return ExcelError.ExcelErrorNum

                Dim res = contingencytable.MantelHaenszel(mat, alphaValue)
                If res.Item1 Is Nothing OrElse res.Item2 Is Nothing OrElse Not IsFinite(res.Item1.TestStatistics1) Then Return ExcelError.ExcelErrorNum

                Dim body As Object(,) = {
                    {"Chi-square", res.Item1.TestStatistics1},
                    {"Degrees of freedom", 1},
                    {"Two-sided p-value", res.Item1.Pvalue},
                    {"Common odds ratio", res.Item2.Estimate},
                    {res.Item2.CIlabel, res.Item2.strConfidenceInterval(CIformat.LL_to_UL)}
                }
                Return BuildResultTable("Mantel-Haenszel Stratified 2×2 Analysis", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.MANTEL_HAENSZEL", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Proportions
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Estimates a single proportion and returns a score-based confidence interval.
        ''' </summary>
        ''' <param name="responders">
        ''' Number of observations classified as responders, successes, or events.
        ''' This is the numerator of the observed proportion and must satisfy
        ''' <c>0 ≤ responders ≤ totalN</c>.
        ''' </param>
        ''' <param name="totalN">
        ''' Total number of observations or Bernoulli trials.
        ''' The observed proportion is
        ''' <c>p̂ = responders / totalN</c>.
        ''' The value must be a positive integer.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level used to construct the confidence interval.
        ''' The returned confidence level is <c>1 - alpha</c>.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the sample size,
        ''' the number of responders,
        ''' the observed proportion,
        ''' and the lower and upper confidence limits.
        ''' Returns <c>#VALUE!</c> when one or more inputs are missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the arguments are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function estimates a binomial proportion from a sample of size <c>n</c>
        ''' with <c>x</c> responders, so the point estimate is
        ''' <c>p̂ = x / n</c>.
        ''' </para>
        ''' <para>
        ''' The confidence interval is based on the Wilson score method rather than the simple
        ''' Wald interval <c>p̂ ± z √(p̂(1-p̂)/n)</c>.
        ''' The Wilson interval is generally preferred because it has better coverage properties,
        ''' especially when the sample size is small or when the proportion is close to 0 or 1.
        ''' </para>
        ''' <para>
        ''' If <c>z = Φ⁻¹(1 - alpha/2)</c>, one form of the Wilson limits is
        ''' </para>
        ''' <para>
        ''' <c>
        ''' L,U = (2x + z² ± z √(z² + 4x(1 - x/n))) / (2(n + z²)).
        ''' </c>
        ''' </para>
        ''' <para>
        ''' The interval is bounded inside the unit interval and should be interpreted on the probability scale.
        ''' For example, a returned estimate of 0.32 means that about 32% of the observed subjects were responders.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.SINGLE_PROPORTION(18,50)
        ''' =BESH.CT.SINGLE_PROPORTION(18,50,0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.SINGLE_PROPORTION",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Estimate a single proportion and return a Wilson score confidence interval.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency/")>
        Public Function CT_SINGLE_PROPORTION(
            <ExcelArgument(Name:="responders", Description:="Number of responders, successes, or events. Must satisfy 0 ≤ responders ≤ totalN.")> responders As Object,
            <ExcelArgument(Name:="totalN", Description:="Total number of observations or trials. Must be a positive integer.")> totalN As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level for the confidence interval. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim x As Integer
                Dim n As Integer

                If Not TryGetWholeNumber(responders, x) OrElse Not TryGetWholeNumber(totalN, n) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim alphaValue As Double = GetOptionalDouble(alpha, 0.05R)
                If n <= 0 OrElse x < 0 OrElse x > n OrElse Not IsOpenUnitInterval(alphaValue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim ci As ConfidenceIntervalResult = contingencytable.SingleProportion(x, n, alphaValue)
                If ci Is Nothing OrElse Not IsFinite(ci.Estimate) OrElse Not IsFinite(ci.LowerLimit) OrElse Not IsFinite(ci.UpperLimit) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim body As Object(,) = {
                    {"Total number of observations", n},
                    {"Number of responders", x},
                    {"Estimated proportion", ci.Estimate},
                    {"Lower confidence limit", ci.LowerLimit},
                    {"Upper confidence limit", ci.UpperLimit},
                    {ci.CIlabel, ci.strConfidenceInterval(CIformat.LL_to_UL)}
                }

                Return BuildResultTable("Single Proportion", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.SINGLE_PROPORTION", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Estimates the difference between two independent proportions and returns a confidence interval.
        ''' </summary>
        ''' <param name="responders1">
        ''' Number of responders, successes, or events in the first sample.
        ''' Must satisfy <c>0 ≤ responders1 ≤ totalN1</c>.
        ''' </param>
        ''' <param name="totalN1">
        ''' Total number of observations in the first sample.
        ''' The first sample proportion is <c>p̂₁ = responders1 / totalN1</c>.
        ''' </param>
        ''' <param name="responders2">
        ''' Number of responders, successes, or events in the second sample.
        ''' Must satisfy <c>0 ≤ responders2 ≤ totalN2</c>.
        ''' </param>
        ''' <param name="totalN2">
        ''' Total number of observations in the second sample.
        ''' The second sample proportion is <c>p̂₂ = responders2 / totalN2</c>.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level used to construct the confidence interval.
        ''' The returned confidence level is <c>1 - alpha</c>.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing both sample proportions,
        ''' the estimated difference <c>p̂₁ - p̂₂</c>,
        ''' and the corresponding lower and upper confidence limits.
        ''' Returns <c>#VALUE!</c> when one or more inputs are missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the arguments are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function compares two independent binomial proportions,
        ''' such as a response rate in a treatment group versus a control group.
        ''' The point estimate is
        ''' <c>Δ̂ = p̂₁ - p̂₂</c>.
        ''' A positive value means the first sample has the higher observed proportion,
        ''' and a negative value means the second sample has the higher observed proportion.
        ''' </para>
        ''' <para>
        ''' The confidence limits are constructed from Wilson-score limits for the two marginal proportions
        ''' and then combined into a score-type interval for the difference.
        ''' This approach is more stable than the elementary normal approximation,
        ''' particularly when one or both sample proportions are near the boundaries 0 or 1.
        ''' </para>
        ''' <para>
        ''' The returned interval is for the absolute risk difference on the probability scale.
        ''' For example, an estimate of <c>0.12</c> means that the first sample proportion exceeds
        ''' the second sample proportion by 12 percentage points.
        ''' </para>
        ''' <para>
        ''' The procedure assumes that the two samples are statistically independent.
        ''' If the same subjects contribute to both measurements, use the paired-proportions function instead.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.TWO_INDEPENDENT_PROPORTIONS(18,50,10,45)
        ''' =BESH.CT.TWO_INDEPENDENT_PROPORTIONS(18,50,10,45,0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.TWO_INDEPENDENT_PROPORTIONS",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Estimate the difference between two independent proportions and return a confidence interval.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency/")>
        Public Function CT_TWO_INDEPENDENT_PROPORTIONS(
            <ExcelArgument(Name:="responders1", Description:="Number of responders in the first sample. Must satisfy 0 ≤ responders1 ≤ totalN1.")> responders1 As Object,
            <ExcelArgument(Name:="totalN1", Description:="Total number of observations in the first sample.")> totalN1 As Object,
            <ExcelArgument(Name:="responders2", Description:="Number of responders in the second sample. Must satisfy 0 ≤ responders2 ≤ totalN2.")> responders2 As Object,
            <ExcelArgument(Name:="totalN2", Description:="Total number of observations in the second sample.")> totalN2 As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level for the confidence interval. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim x1 As Integer
                Dim n1 As Integer
                Dim x2 As Integer
                Dim n2 As Integer

                If Not TryGetWholeNumber(responders1, x1) OrElse
                   Not TryGetWholeNumber(totalN1, n1) OrElse
                   Not TryGetWholeNumber(responders2, x2) OrElse
                   Not TryGetWholeNumber(totalN2, n2) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim alphaValue As Double = GetOptionalDouble(alpha, 0.05R)
                If n1 <= 0 OrElse n2 <= 0 OrElse x1 < 0 OrElse x1 > n1 OrElse x2 < 0 OrElse x2 > n2 OrElse Not IsOpenUnitInterval(alphaValue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim ci As ConfidenceIntervalResult = contingencytable.TwoIndependentProportions(x1, n1, x2, n2, alphaValue)
                If ci Is Nothing OrElse Not IsFinite(ci.Estimate) OrElse Not IsFinite(ci.LowerLimit) OrElse Not IsFinite(ci.UpperLimit) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim p1 As Double = CDbl(x1) / CDbl(n1)
                Dim p2 As Double = CDbl(x2) / CDbl(n2)

                Dim body As Object(,) = {
                    {"Total observations in sample 1", n1},
                    {"Responders in sample 1", x1},
                    {"Estimated proportion in sample 1", p1},
                    {"Total observations in sample 2", n2},
                    {"Responders in sample 2", x2},
                    {"Estimated proportion in sample 2", p2},
                    {"Estimated difference (p1 - p2)", ci.Estimate},
                    {"Lower confidence limit", ci.LowerLimit},
                    {"Upper confidence limit", ci.UpperLimit},
                    {ci.CIlabel, ci.strConfidenceInterval(CIformat.LL_to_UL)}
                }

                Return BuildResultTable("Two Independent Proportions", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.TWO_INDEPENDENT_PROPORTIONS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Estimates the difference between two paired proportions and returns a confidence interval.
        ''' </summary>
        ''' <param name="totalN">
        ''' Total number of paired observations.
        ''' Each observational unit contributes one paired binary outcome,
        ''' such as before/after response for the same subject or two ratings on the same subject.
        ''' The value must be a positive integer.
        ''' </param>
        ''' <param name="respondersOnly1">
        ''' Number of pairs that are positive only in the first condition and negative in the second condition.
        ''' In a paired <c>2×2</c> table this is one of the discordant cells.
        ''' </param>
        ''' <param name="respondersOnly2">
        ''' Number of pairs that are positive only in the second condition and negative in the first condition.
        ''' This is the other discordant cell.
        ''' </param>
        ''' <param name="respondersBoth">
        ''' Number of pairs that are positive in both conditions simultaneously.
        ''' This is one of the concordant cells.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level used to construct the confidence interval.
        ''' The returned confidence level is <c>1 - alpha</c>.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' A labeled result table containing the marginal paired proportions,
        ''' the estimated difference,
        ''' and the lower and upper confidence limits.
        ''' Returns <c>#VALUE!</c> when one or more inputs are missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the arguments are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function is for matched or repeated-measures binary data.
        ''' If the paired <c>2×2</c> table is written as
        ''' </para>
        ''' <para>
        ''' <c>
        ''' [ negative in both, respondersOnly1 ;
        '''   respondersOnly2, respondersBoth ]
        ''' </c>
        ''' </para>
        ''' <para>
        ''' then the two marginal proportions are
        ''' <c>p̂₁ = (respondersOnly1 + respondersBoth) / totalN</c>
        ''' and
        ''' <c>p̂₂ = (respondersOnly2 + respondersBoth) / totalN</c>.
        ''' The reported effect estimate is
        ''' <c>Δ̂ = p̂₁ - p̂₂</c>.
        ''' </para>
        ''' <para>
        ''' The confidence interval is based on Wilson-score limits for the two marginal proportions
        ''' together with a dependence adjustment derived from the paired binary association.
        ''' This is important because the two marginal proportions are computed from the same observational units
        ''' and are therefore not statistically independent.
        ''' </para>
        ''' <para>
        ''' A positive estimate means the first condition has the higher observed marginal proportion.
        ''' A negative estimate means the second condition has the higher observed marginal proportion.
        ''' This function focuses on estimation of the paired proportion difference rather than on hypothesis testing.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.PAIRED_PROPORTIONS(80,12,7,20)
        ''' =BESH.CT.PAIRED_PROPORTIONS(80,12,7,20,0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.PAIRED_PROPORTIONS",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Estimate the difference between two paired proportions and return a confidence interval.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/contingency/")>
        Public Function CT_PAIRED_PROPORTIONS(
            <ExcelArgument(Name:="totalN", Description:="Total number of paired observations.")> totalN As Object,
            <ExcelArgument(Name:="respondersOnly1", Description:="Number positive only in the first condition.")> respondersOnly1 As Object,
            <ExcelArgument(Name:="respondersOnly2", Description:="Number positive only in the second condition.")> respondersOnly2 As Object,
            <ExcelArgument(Name:="respondersBoth", Description:="Number positive in both conditions.")> respondersBoth As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level for the confidence interval. Default 0.05.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim n As Integer
                Dim only1 As Integer
                Dim only2 As Integer
                Dim both As Integer

                If Not TryGetWholeNumber(totalN, n) OrElse
                   Not TryGetWholeNumber(respondersOnly1, only1) OrElse
                   Not TryGetWholeNumber(respondersOnly2, only2) OrElse
                   Not TryGetWholeNumber(respondersBoth, both) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim alphaValue As Double = GetOptionalDouble(alpha, 0.05R)
                If n <= 0 OrElse only1 < 0 OrElse only2 < 0 OrElse both < 0 OrElse
                   (only1 + both) > n OrElse (only2 + both) > n OrElse
                   Not IsOpenUnitInterval(alphaValue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim ci As ConfidenceIntervalResult = contingencytable.PairedProportions(n, only1, only2, both, alphaValue)
                If ci Is Nothing OrElse Not IsFinite(ci.Estimate) OrElse Not IsFinite(ci.LowerLimit) OrElse Not IsFinite(ci.UpperLimit) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim p1 As Double = CDbl(only1 + both) / CDbl(n)
                Dim p2 As Double = CDbl(only2 + both) / CDbl(n)

                Dim body As Object(,) = {
                    {"Total paired observations", n},
                    {"Responders only in the first condition", only1},
                    {"Responders only in the second condition", only2},
                    {"Responders in both conditions", both},
                    {"Estimated proportion in the first condition", p1},
                    {"Estimated proportion in the second condition", p2},
                    {"Estimated difference (p1 - p2)", ci.Estimate},
                    {"Lower confidence limit", ci.LowerLimit},
                    {"Upper confidence limit", ci.UpperLimit},
                    {ci.CIlabel, ci.strConfidenceInterval(CIformat.LL_to_UL)}
                }

                Return BuildResultTable("Paired Proportions", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.PAIRED_PROPORTIONS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' =============================================================================================================
        ' Two independent proportions: non-inferiority and TOST equivalence
        ' =============================================================================================================

        ''' <summary>
        ''' One-sided non-inferiority comparison for two independent proportions.
        ''' </summary>
        ''' <param name="controlResponders">Number of responders in the control or reference group.</param>
        ''' <param name="controlTotal">Total number of observations in the control or reference group.</param>
        ''' <param name="experimentalResponders">Number of responders in the experimental or test group.</param>
        ''' <param name="experimentalTotal">Total number of observations in the experimental or test group.</param>
        ''' <param name="margin">
        ''' Positive non-inferiority margin magnitude <c>M</c> on the absolute risk-difference scale.
        ''' The comparison is performed on <c>Δ = p(experimental) - p(control)</c>, so the null boundary is <c>-M</c>.
        ''' </param>
        ''' <param name="alpha">Optional <b>one-sided</b> significance level. Default <c>0.025</c>.</param>
        ''' <returns>
        ''' A labeled spill table containing observed proportions, the difference <c>p(experimental) - p(control)</c>,
        ''' the one-sided z test, the matching two-sided confidence interval, and the interval-based decision summary.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The non-inferiority hypotheses are
        ''' <c>H0: Δ ≤ -M</c> versus <c>H1: Δ &gt; -M</c>,
        ''' where <c>M &gt; 0</c> is the largest acceptable loss in the experimental response probability.
        ''' </para>
        ''' <para>
        ''' The function reports the usual risk-difference estimate together with a Wilson/Newcombe-style two-sided confidence interval.
        ''' Non-inferiority is supported when the lower confidence bound exceeds <c>-M</c>, which corresponds to a one-sided p-value at most <c>α</c>.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.TWO_INDEPENDENT_PROPORTIONS_NI(18,50,16,48,0.1)
        ''' =BESH.CT.TWO_INDEPENDENT_PROPORTIONS_NI(18,50,16,48,0.08,0.025)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.TWO_INDEPENDENT_PROPORTIONS_NI",
            Category:="BESHStatNG - Contingency Tables",
            Description:="Non-inferiority comparison for two independent proportions with CI-based decision reporting.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/contingency-tables/")>
        Public Function CT_TWO_INDEPENDENT_PROPORTIONS_NI(
            <ExcelArgument(Name:="controlResponders", Description:="Number of responders in the control/reference group.")> controlResponders As Object,
            <ExcelArgument(Name:="controlTotal", Description:="Total number of observations in the control/reference group.")> controlTotal As Object,
            <ExcelArgument(Name:="experimentalResponders", Description:="Number of responders in the experimental/test group.")> experimentalResponders As Object,
            <ExcelArgument(Name:="experimentalTotal", Description:="Total number of observations in the experimental/test group.")> experimentalTotal As Object,
            <ExcelArgument(Name:="margin", Description:="Positive non-inferiority margin magnitude M on the experimental-minus-control proportion scale.")> margin As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional one-sided alpha. Default 0.025.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim x0 As Integer, n0 As Integer, x1 As Integer, n1 As Integer
                If Not TryGetWholeNumber(controlResponders, x0) OrElse
                   Not TryGetWholeNumber(controlTotal, n0) OrElse
                   Not TryGetWholeNumber(experimentalResponders, x1) OrElse
                   Not TryGetWholeNumber(experimentalTotal, n1) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim marginValue As Double
                If Not TryGetFiniteDouble(margin, marginValue) Then Return ExcelError.ExcelErrorValue
                If marginValue <= 0.0 Then Return ExcelError.ExcelErrorNum

                Dim alphaValue As Double = 0.025
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                If n0 <= 0 OrElse n1 <= 0 OrElse x0 < 0 OrElse x0 > n0 OrElse x1 < 0 OrElse x1 > n1 Then Return ExcelError.ExcelErrorNum

                Dim result As equivalencetests.ProportionNonInferiorityResult = equivalencetests.EquivalenceNonInferiorityMethods.TestIndependentProportionsNonInferiority(
                    x0, n0, x1, n1, marginValue, alphaValue)

                Dim body As Object(,) = {
                    {"Control observations", result.NumberOfControls},
                    {"Control responders", result.ControlResponders},
                    {"Control proportion", result.ControlProportion},
                    {"Experimental observations", result.NumberOfExperimental},
                    {"Experimental responders", result.ExperimentalResponders},
                    {"Experimental proportion", result.ExperimentalProportion},
                    {"Difference (experimental - control)", result.DifferenceExperimentalMinusControl},
                    {"Standard error of the difference", result.StandardError},
                    {"Non-inferiority margin magnitude", result.NonInferiorityMargin},
                    {"Null boundary on difference scale", result.NonInferiorityLimit},
                    {"One-sided alpha", result.AlphaOneSided},
                    {"Z statistic", result.ZStatistic},
                    {"One-sided p-value", result.PValue},
                    {"Lower one-sided confidence limit", result.LowerOneSidedConfidenceLimit},
                    {SafeCiLabel(result.TwoSidedEquivalentConfidenceInterval), SafeCiText(result.TwoSidedEquivalentConfidenceInterval)},
                    {"Point estimate within stated limits", result.CiAssessment.IsPointEstimateWithinMargins},
                    {"Confidence interval within stated limits", result.CiAssessment.IsConfidenceIntervalWithinMargins},
                    {"Lower-bound non-inferiority supported by CI", result.CiAssessment.SupportsLowerNonInferiority},
                    {"Upper-bound non-inferiority supported by CI", result.CiAssessment.SupportsUpperNonInferiority},
                    {"Conclusion", result.Conclusion}
                }

                Return BuildResultTable("Two Independent Proportions Non-Inferiority", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.TWO_INDEPENDENT_PROPORTIONS_NI", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' TOST-style equivalence comparison for two independent proportions.
        ''' </summary>
        ''' <param name="controlResponders">Number of responders in the control or reference group.</param>
        ''' <param name="controlTotal">Total number of observations in the control or reference group.</param>
        ''' <param name="experimentalResponders">Number of responders in the experimental or test group.</param>
        ''' <param name="experimentalTotal">Total number of observations in the experimental or test group.</param>
        ''' <param name="lowerMargin">
        ''' Lower equivalence margin on the risk-difference scale <c>p(experimental) - p(control)</c>.
        ''' If <paramref name="upperMargin"/> is omitted, this argument is interpreted as a positive symmetric margin magnitude <c>M</c>
        ''' and the function uses margins <c>-M</c> and <c>+M</c>.
        ''' </param>
        ''' <param name="upperMargin">Optional upper equivalence margin. When omitted, ±lowerMargin is used.</param>
        ''' <param name="alpha">Optional one-sided alpha for each TOST component. Default <c>0.025</c>.</param>
        ''' <returns>
        ''' A labeled spill table containing the two one-sided proportion tests, the combined TOST p-value,
        ''' the matched two-sided confidence interval, and the interval-based decision summary.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Equivalence is evaluated on the absolute risk-difference scale using the Two One-Sided Tests principle.
        ''' Let <c>Δ = p(experimental) - p(control)</c>. The function evaluates
        ''' <c>H0,lower: Δ ≤ L</c> versus <c>H1,lower: Δ &gt; L</c>
        ''' and
        ''' <c>H0,upper: Δ ≥ U</c> versus <c>H1,upper: Δ &lt; U</c>.
        ''' Both components must be significant at the supplied one-sided α level for equivalence to be supported.
        ''' </para>
        ''' <para>
        ''' The reported confidence interval is a Wilson/Newcombe-style interval for the difference in two independent proportions.
        ''' Equivalence is supported when that interval lies completely inside the stated equivalence margins.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.CT.TWO_INDEPENDENT_PROPORTIONS_EQUIV(18,50,16,48,0.1)
        ''' =BESH.CT.TWO_INDEPENDENT_PROPORTIONS_EQUIV(18,50,16,48,-0.08,0.08,0.025)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.CT.TWO_INDEPENDENT_PROPORTIONS_EQUIV",
            Category:="BESHStatNG - Contingency Tables",
            Description:="TOST-style equivalence comparison for two independent proportions with interval-based decision reporting.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/contingency-tables/")>
        Public Function CT_TWO_INDEPENDENT_PROPORTIONS_EQUIV(
            <ExcelArgument(Name:="controlResponders", Description:="Number of responders in the control/reference group.")> controlResponders As Object,
            <ExcelArgument(Name:="controlTotal", Description:="Total number of observations in the control/reference group.")> controlTotal As Object,
            <ExcelArgument(Name:="experimentalResponders", Description:="Number of responders in the experimental/test group.")> experimentalResponders As Object,
            <ExcelArgument(Name:="experimentalTotal", Description:="Total number of observations in the experimental/test group.")> experimentalTotal As Object,
            <ExcelArgument(Name:="lowerMargin", Description:="Lower equivalence margin, or a positive symmetric margin magnitude if upperMargin is omitted.")> lowerMargin As Object,
            <ExcelArgument(Name:="upperMargin", Description:="Optional upper equivalence margin. When omitted, ±lowerMargin is used.")> Optional upperMargin As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional one-sided alpha for each TOST component. Default 0.025.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Dim x0 As Integer, n0 As Integer, x1 As Integer, n1 As Integer
                If Not TryGetWholeNumber(controlResponders, x0) OrElse
                   Not TryGetWholeNumber(controlTotal, n0) OrElse
                   Not TryGetWholeNumber(experimentalResponders, x1) OrElse
                   Not TryGetWholeNumber(experimentalTotal, n1) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim lowerValue As Double
                Dim upperValue As Double
                If Not TryGetEquivalenceMargins(lowerMargin, upperMargin, lowerValue, upperValue) Then Return ExcelError.ExcelErrorValue
                If lowerValue >= upperValue Then Return ExcelError.ExcelErrorNum

                Dim alphaValue As Double = 0.025
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                If n0 <= 0 OrElse n1 <= 0 OrElse x0 < 0 OrElse x0 > n0 OrElse x1 < 0 OrElse x1 > n1 Then Return ExcelError.ExcelErrorNum

                Dim result As equivalencetests.ProportionEquivalenceResult = equivalencetests.EquivalenceNonInferiorityMethods.TestIndependentProportionsEquivalence(
                    x0, n0, x1, n1, lowerValue, upperValue, alphaValue)

                Dim body As Object(,) = {
                    {"Control observations", result.NumberOfControls},
                    {"Control responders", result.ControlResponders},
                    {"Control proportion", result.ControlProportion},
                    {"Experimental observations", result.NumberOfExperimental},
                    {"Experimental responders", result.ExperimentalResponders},
                    {"Experimental proportion", result.ExperimentalProportion},
                    {"Difference (experimental - control)", result.DifferenceExperimentalMinusControl},
                    {"Standard error of the difference", result.StandardError},
                    {"Lower equivalence margin", result.LowerMargin},
                    {"Upper equivalence margin", result.UpperMargin},
                    {"One-sided alpha", result.AlphaOneSided},
                    {"Lower TOST z statistic", result.LowerComponentStatistic},
                    {"Lower TOST p-value", result.LowerComponentPValue},
                    {"Upper TOST z statistic", result.UpperComponentStatistic},
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

                Return BuildResultTable("Two Independent Proportions Equivalence (TOST)", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.CT.TWO_INDEPENDENT_PROPORTIONS_EQUIV", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Helpers
        ' -------------------------------------------------------------------------------------------------------------

        Private Function Is2x2(table(,) As Integer) As Boolean
            If table Is Nothing Then Return False
            Return table.GetLength(0) = 2 AndAlso table.GetLength(1) = 2
        End Function

        Private Function TableTotal(table(,) As Integer) As Integer
            Dim total As Integer = 0
            For i As Integer = 0 To table.GetLength(0) - 1
                For j As Integer = 0 To table.GetLength(1) - 1
                    total += table(i, j)
                Next
            Next
            Return total
        End Function

        Private Function EffectiveDf(table(,) As Integer) As Integer
            Dim rows As Integer = table.GetLength(0)
            Dim cols As Integer = table.GetLength(1)
            Dim activeRows As Integer = rows
            Dim activeCols As Integer = cols

            For i As Integer = 0 To rows - 1
                Dim s As Integer = 0
                For j As Integer = 0 To cols - 1
                    s += table(i, j)
                Next
                If s = 0 Then activeRows -= 1
            Next

            For j As Integer = 0 To cols - 1
                Dim s As Integer = 0
                For i As Integer = 0 To rows - 1
                    s += table(i, j)
                Next
                If s = 0 Then activeCols -= 1
            Next

            Return Math.Max(0, activeRows * activeCols - activeRows - activeCols + 1)
        End Function

        Private Function TransposeIntMatrix(table(,) As Integer) As Integer(,)
            Dim rows As Integer = table.GetLength(0)
            Dim cols As Integer = table.GetLength(1)
            Dim out(cols - 1, rows - 1) As Integer

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    out(j, i) = table(i, j)
                Next
            Next

            Return out
        End Function



    End Module
End Namespace
