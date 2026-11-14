Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.CausalInference
Imports ExcelDna.Integration

Namespace WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for propensity-score matching, weighting, subclassification, optimal pair matching,
    ''' and coarsened exact matching from Excel ranges.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These functions use a handle-based workflow. <c>BESH.PS.FIT</c> performs the statistical analysis once
    ''' and returns a text handle. The remaining functions reuse that handle to return the requested output tables
    ''' such as summaries, propensity scores, matched pairs, weights, balance diagnostics, effect estimates,
    ''' Love-plot data, and warnings. This avoids recalculating the same matching or weighting operation for every
    ''' worksheet output.
    ''' </para>
    ''' <para>
    ''' The fitted analysis supports either estimated propensity scores or user-supplied propensity scores.
    ''' When scores are estimated, the treatment indicator is modeled from the supplied covariates using a
    ''' binomial-logit propensity model. The covariate matrix can be used directly or expanded by a right-hand-side
    ''' formula, including main effects, categorical terms, polynomial terms, and interactions supported by the add-in's
    ''' regression formula syntax.
    ''' </para>
    ''' <para>
    ''' Adjustment methods include nearest-neighbor matching, optimal pair matching, propensity-score weighting,
    ''' subclassification, and coarsened exact matching. Common output diagnostics include standardized mean
    ''' differences, variance ratios, empirical-distribution differences, overlap summaries, effective sample size,
    ''' matched-pair tables, row-level inclusion flags, and chart-ready Love-plot data.
    ''' </para>
    ''' <para>
    ''' Propensity-score methods adjust only for measured covariates included in the analysis. They do not remove
    ''' bias due to unmeasured confounding, post-treatment variables, misspecified treatment timing, or poor overlap
    ''' between treatment groups. Always inspect balance and overlap diagnostics before interpreting treatment-effect
    ''' estimates.
    ''' </para>
    ''' </remarks>
    Public Module PropensityScoreUDFs

        Private ReadOnly _psmCache As New ConcurrentDictionary(Of String, PsmUdfHandle)(StringComparer.OrdinalIgnoreCase)

        Private Class PsmUdfHandle
            Public Property Handle As String
            Public Property Data As psmData
            Public Property FitOptions As PsmComprehensiveFitOptions
            Public Property Result As PsmComprehensiveResult
            Public Property Alpha As Double = 0.05R
            Public Property CreatedUtc As DateTime
        End Class

        ''' <summary>
        ''' Fits a propensity-score analysis and returns a reusable handle for subsequent table functions.
        ''' </summary>
        ''' <param name="id">
        ''' Optional ID column aligned with the treatment, outcome, and covariate rows. If omitted or blank, worksheet
        ''' source row numbers are used as row identifiers in output tables. IDs are used only for reporting and matching
        ''' audit tables; they do not affect the statistical adjustment.
        ''' </param>
        ''' <param name="treatment">
        ''' Required treatment indicator column. Values must identify two groups, normally coded as <c>0</c> for control
        ''' and <c>1</c> for treated. Rows with invalid, missing, or non-finite treatment values cannot be analyzed.
        ''' </param>
        ''' <param name="outcome">
        ''' Required outcome column aligned with <paramref name="treatment"/>. The treatment-effect tables compare
        ''' the treated and control outcomes after the selected propensity-score adjustment. The current worksheet
        ''' functions are primarily intended for numeric continuous outcomes.
        ''' </param>
        ''' <param name="covariates">
        ''' Required raw covariate matrix with one row per subject and one or more columns of pre-treatment covariates.
        ''' These variables are used to estimate propensity scores, to calculate balance diagnostics, and, when requested,
        ''' to define matching or coarsening distances. Covariates should be measured before treatment assignment.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional covariate names supplied as a one-row range, one-column range, or comma-separated text. Names are
        ''' displayed in output tables and can be referenced by the formula argument when formula addressing is set to
        ''' <c>names</c>. If omitted, generic covariate names are created from the input columns.
        ''' </param>
        ''' <param name="method">
        ''' Adjustment method. Accepted values include <c>matching</c> or <c>nearest</c> for nearest-neighbor matching,
        ''' <c>optimal</c> for optimal pair matching, <c>weighting</c> or <c>iptw</c> for propensity-score weighting,
        ''' <c>subclassification</c> or <c>subclass</c> for propensity-score strata, and <c>cem</c> for coarsened exact matching.
        ''' The default is nearest-neighbor matching.
        ''' </param>
        ''' <param name="estimand">
        ''' Target estimand. Accepted values are <c>ATT</c> for the average treatment effect among treated subjects,
        ''' <c>ATC</c> for the average effect among controls, <c>ATE</c> for the full-sample average effect, and <c>ATO</c>
        ''' for the overlap-population effect. Not every method supports every estimand; unsupported combinations return
        ''' an error instead of silently changing the requested analysis. The default is <c>ATT</c>.
        ''' </param>
        ''' <param name="scoreMethod">
        ''' Propensity-score source. Use <c>logit</c>, <c>logistic</c>, or <c>glm</c> to estimate scores from the treatment
        ''' indicator and covariates. Use <c>supplied</c> or <c>existing</c> when propensity scores are already available
        ''' in <paramref name="existingScore"/>. The default is estimated logistic propensity scores.
        ''' </param>
        ''' <param name="existingScore">
        ''' Optional supplied propensity-score column. This argument is required when <paramref name="scoreMethod"/>
        ''' is <c>supplied</c> or <c>existing</c>. Values must be finite probabilities strictly between 0 and 1. Supplied
        ''' scores are used directly for matching, weighting, subclassification, overlap diagnostics, and Love-plot data.
        ''' </param>
        ''' <param name="exactGroups">
        ''' Optional exact-matching or grouping matrix aligned with the analysis rows. When used with matching methods,
        ''' treated and control subjects are matched only within the same exact-group combination. With coarsened exact
        ''' matching, these columns can be used as additional exact grouping dimensions. Multiple columns are combined
        ''' row by row into a joint group label.
        ''' </param>
        ''' <param name="formula">
        ''' Optional right-hand-side formula that selects and expands covariates for the propensity model. If omitted,
        ''' all raw covariate columns are used as main effects. The formula can reference supplied variable names,
        ''' relative column letters, or absolute worksheet column letters depending on <paramref name="formulaAddressing"/>.
        ''' </param>
        ''' <param name="matchingOptions">
        ''' Optional semicolon-separated option string controlling the adjustment method. Common keys include
        ''' <c>ratio</c>, <c>replacement</c>, <c>caliper</c>, <c>caliperScale</c>, <c>distance</c>, <c>order</c>,
        ''' <c>seed</c>, <c>support</c>, <c>trimLower</c>, <c>trimUpper</c>, <c>subclasses</c>, <c>cemBins</c>,
        ''' <c>normalizeWeights</c>, <c>stabilizedWeights</c>, <c>ridge</c>, <c>maxIter</c>, and <c>tol</c>. For example:
        ''' <c>ratio=2; replacement=false; caliper=0.2; caliperScale=sd_logit; distance=mahalanobis_with_ps_caliper</c>.
        ''' </param>
        ''' <param name="diagnosticOptions">
        ''' Optional semicolon-separated option string controlling diagnostics and reporting thresholds. Common keys include
        ''' <c>smd</c> for the standardized-mean-difference threshold, <c>vrLower</c> and <c>vrUpper</c> for variance-ratio
        ''' thresholds, <c>overlapBins</c> for propensity-score histogram bins, and <c>lovePlot</c> to request Love-plot
        ''' data preparation.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level used for confidence intervals and hypothesis-test summaries where supported.
        ''' The default is <c>0.05</c>. This setting affects reporting only; it does not change the matched sets, weights,
        ''' propensity scores, or balance diagnostics.
        ''' </param>
        ''' <param name="formulaAddressing">
        ''' Formula-addressing mode. Use <c>relative</c> to refer to covariate columns as A, B, C relative to the supplied
        ''' covariate range, <c>absolute</c> to refer to worksheet column letters, or <c>names</c> to refer to values supplied
        ''' in <paramref name="varNames"/>. The default is <c>relative</c>.
        ''' </param>
        ''' <returns>
        ''' A text handle identifying the fitted propensity-score analysis in the current Excel session. Pass this handle
        ''' to the companion functions to retrieve output tables without refitting. If the function cannot fit the analysis,
        ''' it returns an Excel error or an explanatory error message.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function aligns the supplied columns, removes rows that cannot be analyzed, estimates or validates
        ''' propensity scores, applies the requested adjustment method, computes balance diagnostics, and stores the
        ''' complete result for later retrieval. The returned handle is session-local and is not saved permanently in the
        ''' workbook. Recalculate <c>BESH.PS.FIT</c> to create a fresh result after changing the input data or options.
        ''' </para>
        ''' <para>
        ''' Typical workflow: first call <c>BESH.PS.FIT</c>; then use <c>BESH.PS.SUMMARY</c>, <c>BESH.PS.SCORES</c>,
        ''' <c>BESH.PSM.MATCHES</c>, <c>BESH.PS.BALANCE</c>, <c>BESH.PS.EFFECT</c>, and related table functions with
        ''' the returned handle. Use <c>BESH.PS.CLEANUP</c> to remove stored results that are no longer needed.
        ''' </para>
        ''' <para>
        ''' Example: <c>=BESH.PS.FIT(A2:A101,B2:B101,C2:C101,D2:H101,D1:H1,&quot;matching&quot;,&quot;ATT&quot;,&quot;logit&quot;,,,&quot;age + sex + baseline&quot;, &quot;ratio=1; replacement=false; caliper=0.2; caliperScale=sd_logit&quot;, &quot;smd=0.1; lovePlot=true&quot;)</c>.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.PS.FIT",
            Category:="BESHStatNG - Causal Inference",
            Description:="Fits a propensity-score analysis and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/propensity-score-matching/"
        )>
        Public Function PS_FIT(
            <ExcelArgument(Name:="id", Description:="Optional ID column. Leave blank to use source row numbers.")> Optional id As Object = Nothing,
            <ExcelArgument(Name:="treatment", Description:="0/1 treatment indicator column.")> Optional treatment As Object = Nothing,
            <ExcelArgument(Name:="outcome", Description:="Outcome column aligned with treatment and covariates.")> Optional outcome As Object = Nothing,
            <ExcelArgument(Name:="covariates", Description:="Raw covariate matrix, with one row per subject.")> Optional covariates As Object = Nothing,
            <ExcelArgument(Name:="varNames", Description:="Optional covariate names as comma-separated text or range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="matching, weighting, subclassification, optimal, or cem.")> Optional method As Object = Nothing,
            <ExcelArgument(Name:="estimand", Description:="ATT, ATC, ATE, or ATO, depending on method.")> Optional estimand As Object = Nothing,
            <ExcelArgument(Name:="scoreMethod", Description:="logit/logistic or supplied.")> Optional scoreMethod As Object = Nothing,
            <ExcelArgument(Name:="existingScore", Description:="Optional supplied propensity-score column, required when scoreMethod=supplied.")> Optional existingScore As Object = Nothing,
            <ExcelArgument(Name:="exactGroups", Description:="Optional exact-matching group column or matrix.")> Optional exactGroups As Object = Nothing,
            <ExcelArgument(Name:="formula", Description:="Optional RHS propensity model formula using covariate columns/names.")> Optional formula As Object = Nothing,
            <ExcelArgument(Name:="matchingOptions", Description:="Semicolon-separated options, e.g. ratio=2; replacement=true; caliper=0.2; caliperScale=sd_logit; distance=mahalanobis_with_ps_caliper.")> Optional matchingOptions As Object = Nothing,
            <ExcelArgument(Name:="diagnosticOptions", Description:="Semicolon-separated diagnostics/options, e.g. smd=0.1; vrLower=0.5; vrUpper=2; lovePlot=true; overlapBins=20.")> Optional diagnosticOptions As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha for downstream reporting where supported. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Formula addressing: relative, absolute, or names. Default relative.")> Optional formulaAddressing As Object = Nothing
        ) As Object

            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|aef59c2e00938b4e@11274:5235")
        If ExcelDnaUtil.IsInFunctionWizard() Then Return "PS_FIT (editing...)"

            Try
                Dim fitOptions As PsmComprehensiveFitOptions = BuildFitOptions(method, estimand, scoreMethod, matchingOptions, diagnosticOptions)
                Dim alphaValue As Double = 0.05R
                If Not Global.BESHStatNG.UdfDataImport.TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim imported As psmData = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetPsmData(id:=id,
                                                                      treatment:=treatment,
                                                                      outcome:=outcome,
                                                                      covariates:=covariates,
                                                                      varNames:=varNames,
                                                                      scoreMethod:=fitOptions.StandardOptions.ScoreMethod,
                                                                      suppliedScore:=existingScore,
                                                                      exactGroups:=exactGroups,
                                                                      formula:=formula,
                                                                      formulaAddressing:=formulaAddressing,
                                                                      data:=imported) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim result As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(imported.Input, fitOptions)
                Dim handleKey As String = "PSM:" & Guid.NewGuid().ToString("N")
                _psmCache(handleKey) = New PsmUdfHandle With {
                    .Handle = handleKey,
                    .Data = imported,
                    .FitOptions = fitOptions,
                    .Result = result,
                    .Alpha = alphaValue,
                    .CreatedUtc = DateTime.UtcNow
                }

                Return handleKey

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.PS.FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a stacked summary report for a fitted propensity-score analysis.
        ''' </summary>
        ''' <param name="handle">
        ''' Text handle returned by <c>BESH.PS.FIT</c>. The handle identifies the fitted analysis to summarize.
        ''' </param>
        ''' <returns>
        ''' A spill table containing run settings, imported-data information, sample-size summaries, propensity-score
        ''' model information when available, treatment-effect summaries, and warnings. The exact sections depend on
        ''' the selected adjustment method and available diagnostics.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function as the first review table after fitting. It echoes key options such as adjustment method,
        ''' estimand, score source, matching ratio, replacement choice, caliper settings, common-support handling,
        ''' diagnostic thresholds, and any rows dropped during data preparation.
        ''' </para>
        ''' <para>
        ''' The summary is intended for auditability and interpretation. It does not show every row-level score or
        ''' matched pair; use the dedicated score, match, balance, and effect functions for those details.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(Name:="BESH.PS.SUMMARY", Category:="BESHStatNG - Causal Inference", Description:="Returns stacked summary tables for a fitted propensity-score handle.", HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/propensity-score-matching/")>
        Public Function PS_SUMMARY(<ExcelArgument(Name:="handle", Description:="Handle returned by BESH.PS.FIT.")> handle As Object) As Object
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|4bf78642c7f8d435@17851:918")
        If ExcelDnaUtil.IsInFunctionWizard() Then Return "PS_SUMMARY (editing...)"
            Try
                Dim h As PsmUdfHandle = Nothing
                If Not TryGetPsmHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return StackResultTables(PsmFormattedResultTables.GeneralResultTables(h.Result, h.FitOptions, DataImportSummaryTable(h)))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.PS.SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns row-level propensity scores, transformed scores, inclusion flags, weights, and match status.
        ''' </summary>
        ''' <param name="handle">
        ''' Text handle returned by <c>BESH.PS.FIT</c>.
        ''' </param>
        ''' <param name="includeHeader">
        ''' Optional TRUE/FALSE value indicating whether to include the header row. The default is TRUE.
        ''' </param>
        ''' <returns>
        ''' A spill table with one row per analyzed subject, including identifiers, source row numbers, treatment and
        ''' outcome values, propensity scores, logit-transformed scores, common-support and trimming flags, matching
        ''' status, balancing weights, and matching weights where relevant.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This table is the main row-level audit output. It is useful for verifying which rows were included in the
        ''' final analysis, which rows were removed by common-support or trimming rules, and how much each row
        ''' contributes to matched or weighted estimates.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(Name:="BESH.PS.SCORES", Category:="BESHStatNG - Causal Inference", Description:="Returns row-level propensity scores, logits, inclusion flags, weights and match status.", HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/propensity-score-matching/")>
        Public Function PS_SCORES(<ExcelArgument(Name:="handle", Description:="Handle returned by BESH.PS.FIT.")> handle As Object,
                                  <ExcelArgument(Name:="includeHeader", Description:="TRUE to include the header row. Default TRUE.")> Optional includeHeader As Object = Nothing) As Object
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|10fcfdbbf4a84769@19965:1153")
        If ExcelDnaUtil.IsInFunctionWizard() Then Return "PS_SCORES (editing...)"
            Try
                Dim h As PsmUdfHandle = Nothing
                If Not TryGetPsmHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(PsmFrontEndTables.RowLevelAuditTable(h.Data.Input, h.Result.Result, h.Data.RowIds), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.PS.SCORES", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the matched-pair or matched-set table for matching analyses.
        ''' </summary>
        ''' <param name="handle">
        ''' Text handle returned by <c>BESH.PS.FIT</c>. The referenced fit should use nearest-neighbor or optimal pair
        ''' matching.
        ''' </param>
        ''' <param name="includeHeader">
        ''' Optional TRUE/FALSE value indicating whether to include the header row. The default is TRUE.
        ''' </param>
        ''' <returns>
        ''' A spill table describing matched treated-control links, including set identifiers, row identifiers, propensity
        ''' scores, matching distance, exact group labels where present, outcome values, and reuse information when
        ''' matching with replacement is used. If the selected method does not produce matched pairs, the returned table
        ''' indicates that no matched-pair output is available.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function to inspect the actual matches behind the effect estimate. For 1:k nearest-neighbor matching,
        ''' a treated subject can appear in a matched set with multiple controls for ATT analyses, or a control subject can
        ''' appear with multiple treated subjects for ATC analyses. Exact-group and caliper restrictions are reflected in
        ''' the returned matched links.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(Name:="BESH.PSM.MATCHES", Category:="BESHStatNG - Causal Inference", Description:="Returns the matched-pair/set table for a fitted propensity-score handle.", HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/propensity-score-matching/")>
        Public Function PSM_MATCHES(<ExcelArgument(Name:="handle", Description:="Handle returned by BESH.PS.FIT.")> handle As Object,
                                    <ExcelArgument(Name:="includeHeader", Description:="TRUE to include the header row. Default TRUE.")> Optional includeHeader As Object = Nothing) As Object
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|a7a539470a75a65c@22605:1126")
        If ExcelDnaUtil.IsInFunctionWizard() Then Return "PSM_MATCHES (editing...)"
            Try
                Dim h As PsmUdfHandle = Nothing
                If Not TryGetPsmHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(PsmBackendTables.MatchesTable(h.Result.Result, h.Data.Input), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.PSM.MATCHES", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns row-level weights used by matching, weighting, subclassification, or coarsened exact matching.
        ''' </summary>
        ''' <param name="handle">
        ''' Text handle returned by <c>BESH.PS.FIT</c>.
        ''' </param>
        ''' <param name="includeHeader">
        ''' Optional TRUE/FALSE value indicating whether to include the header row. The default is TRUE.
        ''' </param>
        ''' <returns>
        ''' A spill table with one row per analyzed subject and columns for propensity scores, inclusion flags, matching
        ''' weights, balancing weights, normalized or stabilized weights where applicable, and indicators showing whether
        ''' the row contributed to the adjusted comparison.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The meaning of the weight columns depends on the adjustment method. Matching weights describe contribution
        ''' to matched comparisons. Balancing weights describe contribution to inverse-probability, overlap, subclassified,
        ''' or coarsened-exact weighted estimates. Very large weights can indicate poor overlap and should be interpreted
        ''' together with the balance and overlap diagnostics.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(Name:="BESH.PS.WEIGHTS", Category:="BESHStatNG - Causal Inference", Description:="Returns row-level matching and balancing weights for a fitted propensity-score handle.", HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/propensity-score-matching/")>
        Public Function PS_WEIGHTS(<ExcelArgument(Name:="handle", Description:="Handle returned by BESH.PS.FIT.")> handle As Object,
                                   <ExcelArgument(Name:="includeHeader", Description:="TRUE to include the header row. Default TRUE.")> Optional includeHeader As Object = Nothing) As Object
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|8a243b2fc69fd693@25063:1157")
        If ExcelDnaUtil.IsInFunctionWizard() Then Return "PS_WEIGHTS (editing...)"
            Try
                Dim h As PsmUdfHandle = Nothing
                If Not TryGetPsmHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(PsmFrontEndTables.RowLevelAuditTable(h.Data.Input, h.Result.Result, h.Data.RowIds), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.PS.WEIGHTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns covariate balance diagnostics before and after the selected propensity-score adjustment.
        ''' </summary>
        ''' <param name="handle">
        ''' Text handle returned by <c>BESH.PS.FIT</c>.
        ''' </param>
        ''' <param name="includeHeader">
        ''' Optional TRUE/FALSE value indicating whether to include the header row. The default is TRUE.
        ''' </param>
        ''' <returns>
        ''' A spill table with covariate-level balance statistics, including treatment and control summaries before and
        ''' after adjustment, standardized mean differences, absolute standardized mean differences, variance ratios,
        ''' empirical-distribution diagnostics, and threshold flags.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Balance diagnostics are central to propensity-score analysis. A small treatment-effect p-value is not meaningful
        ''' if the adjustment fails to balance important pre-treatment covariates. Standardized mean differences are often
        ''' reviewed against thresholds such as 0.1 or 0.2, while variance ratios and empirical-distribution differences
        ''' provide additional checks of covariate comparability.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(Name:="BESH.PS.BALANCE", Category:="BESHStatNG - Causal Inference", Description:="Returns balance diagnostics before and after matching/weighting.", HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/propensity-score-matching/")>
        Public Function PS_BALANCE(<ExcelArgument(Name:="handle", Description:="Handle returned by BESH.PS.FIT.")> handle As Object,
                                   <ExcelArgument(Name:="includeHeader", Description:="TRUE to include the header row. Default TRUE.")> Optional includeHeader As Object = Nothing) As Object
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|dec5336f087dfd1e@27557:1099")
        If ExcelDnaUtil.IsInFunctionWizard() Then Return "PS_BALANCE (editing...)"
            Try
                Dim h As PsmUdfHandle = Nothing
                If Not TryGetPsmHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(PsmBackendTables.BalanceTable(h.Result.Result), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.PS.BALANCE", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns treatment-effect estimates for the fitted propensity-score analysis.
        ''' </summary>
        ''' <param name="handle">
        ''' Text handle returned by <c>BESH.PS.FIT</c>.
        ''' </param>
        ''' <param name="includeHeader">
        ''' Optional TRUE/FALSE value indicating whether to include the header row. The default is TRUE.
        ''' </param>
        ''' <returns>
        ''' A spill table containing the requested treatment-effect estimate, the adjustment method used for the estimate,
        ''' the number of contributing rows or matched sets, standard error and confidence interval where available,
        ''' test statistic, p-value, and notes about the inference method.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Effect estimates should be interpreted for the selected target population: treated subjects for ATT, controls
        ''' for ATC, the full input population for ATE, or the overlap population for ATO. The estimate is conditional on
        ''' measured covariates and depends on the quality of overlap and balance after adjustment.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(Name:="BESH.PS.EFFECT", Category:="BESHStatNG - Causal Inference", Description:="Returns treatment-effect estimates for a fitted propensity-score handle.", HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/propensity-score-matching/")>
        Public Function PS_EFFECT(<ExcelArgument(Name:="handle", Description:="Handle returned by BESH.PS.FIT.")> handle As Object,
                                  <ExcelArgument(Name:="includeHeader", Description:="TRUE to include the header row. Default TRUE.")> Optional includeHeader As Object = Nothing) As Object
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|6b30c11ec79337d7@29889:1101")
        If ExcelDnaUtil.IsInFunctionWizard() Then Return "PS_EFFECT (editing...)"
            Try
                Dim h As PsmUdfHandle = Nothing
                If Not TryGetPsmHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(PsmBackendTables.EffectTable(h.Result.Result), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.PS.EFFECT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns chart-ready data for a Love plot of covariate imbalance before and after adjustment.
        ''' </summary>
        ''' <param name="handle">
        ''' Text handle returned by <c>BESH.PS.FIT</c>.
        ''' </param>
        ''' <param name="includeHeader">
        ''' Optional TRUE/FALSE value indicating whether to include the header row. The default is TRUE.
        ''' </param>
        ''' <returns>
        ''' A spill table containing covariate names, absolute standardized mean differences before adjustment, absolute
        ''' standardized mean differences after matching or weighting, threshold values, variable grouping information,
        ''' and display-order fields suitable for creating an Excel scatter or dot plot.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' A Love plot visualizes whether covariate imbalance has been reduced by the adjustment. Values farther to the
        ''' right indicate larger imbalance. The threshold column can be used to add a reference line, commonly at an
        ''' absolute standardized mean difference of 0.1.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(Name:="BESH.PS.LOVEPLOT_DATA", Category:="BESHStatNG - Causal Inference", Description:="Returns chart-ready Love plot data for a fitted propensity-score handle.", HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/propensity-score-matching/")>
        Public Function PS_LOVEPLOT_DATA(<ExcelArgument(Name:="handle", Description:="Handle returned by BESH.PS.FIT.")> handle As Object,
                                         <ExcelArgument(Name:="includeHeader", Description:="TRUE to include the header row. Default TRUE.")> Optional includeHeader As Object = Nothing) As Object
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|ce4731997b4341b0@32207:1145")
        If ExcelDnaUtil.IsInFunctionWizard() Then Return "PS_LOVEPLOT_DATA (editing...)"
            Try
                Dim h As PsmUdfHandle = Nothing
                If Not TryGetPsmHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(PsmAdvancedTables.LovePlotTable(h.Result.LovePlotRows), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.PS.LOVEPLOT_DATA", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a named output table from a fitted propensity-score analysis.
        ''' </summary>
        ''' <param name="handle">
        ''' Text handle returned by <c>BESH.PS.FIT</c>.
        ''' </param>
        ''' <param name="table">
        ''' Name of the requested table. Accepted names include <c>summary</c>, <c>options</c>, <c>import</c>,
        ''' <c>sample</c>, <c>scoremodel</c>, <c>scores</c>, <c>weights</c>, <c>matches</c>, <c>matcheddata</c>,
        ''' <c>balance</c>, <c>effect</c>, <c>aipw</c>, <c>weightdiagnostics</c>, <c>overlap</c>,
        ''' <c>overlapbins</c>, <c>loveplot</c>, <c>subclasses</c>, <c>cemstrata</c>, <c>cemweights</c>, and
        ''' <c>warnings</c>. Several aliases are accepted for common names.
        ''' </param>
        ''' <param name="includeHeader">
        ''' Optional TRUE/FALSE value indicating whether to include the header row. The default is TRUE.
        ''' </param>
        ''' <returns>
        ''' The requested spill table. If the table name is not recognized, a one-cell explanatory table is returned.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This convenience function is useful when building dashboards or documentation examples where the desired
        ''' output table is selected from a cell. It can return all major outputs available through the dedicated worksheet
        ''' functions, plus additional diagnostic and method-specific tables.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(Name:="BESH.PS.TABLE", Category:="BESHStatNG - Causal Inference", Description:="Returns a named PSM output table such as summary, scores, matches, weights, balance, effect, loveplot, overlap, diagnostics, matcheddata, subclasses, cemstrata, cemweights, or warnings.", HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/propensity-score-matching/")>
        Public Function PS_TABLE(<ExcelArgument(Name:="handle", Description:="Handle returned by BESH.PS.FIT.")> handle As Object,
                                 <ExcelArgument(Name:="table", Description:="Requested table name.")> table As Object,
                                 <ExcelArgument(Name:="includeHeader", Description:="TRUE to include the header row. Default TRUE.")> Optional includeHeader As Object = Nothing) As Object
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|8f97597c7bf11b57@34922:1381")
        If ExcelDnaUtil.IsInFunctionWizard() Then Return "PS_TABLE (editing...)"
            Try
                Dim h As PsmUdfHandle = Nothing
                If Not TryGetPsmHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim out As Object(,) = ResolveNamedTable(h, ExcelArgReaders.AsString(table))
                Return PrepareExistingObjectTableForUdf(out, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.PS.TABLE", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes fitted propensity-score analysis handles from the current Excel session.
        ''' </summary>
        ''' <param name="handle">
        ''' Optional handle returned by <c>BESH.PS.FIT</c>. Provide a specific handle to remove one stored result. Leave
        ''' blank or pass <c>ALL</c> to remove all stored propensity-score results from the current session.
        ''' </param>
        ''' <returns>
        ''' When clearing all results, returns the number of removed handles. When clearing one result, returns TRUE if
        ''' the handle was found and removed, and FALSE otherwise.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Handles are stored in memory for the current Excel session so that output functions can reuse a fitted result.
        ''' Cleanup is optional but useful in long sessions, large workbooks, or automated examples that fit many analyses.
        ''' Closing Excel also clears the stored results.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(Name:="BESH.PS.CLEANUP", Category:="BESHStatNG - Causal Inference", Description:="Removes one PSM handle from the session cache, or all PSM handles when handle is blank or ALL.", HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/propensity-score-matching/")>
        Public Function PS_CLEANUP(<ExcelArgument(Name:="handle", Description:="Optional handle returned by BESH.PS.FIT. Blank or ALL clears all PSM handles.")> Optional handle As Object = Nothing) As Object
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|47635a72e02747b3@37377:1105")
        Try
                Dim key As String = ExcelArgReaders.AsString(handle)
                If String.IsNullOrWhiteSpace(key) OrElse String.Equals(key, "ALL", StringComparison.OrdinalIgnoreCase) Then
                    Dim n As Integer = _psmCache.Count
                    _psmCache.Clear()
                    Return n
                End If

                Dim removed As PsmUdfHandle = Nothing
                Return _psmCache.TryRemove(key, removed)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.PS.CLEANUP", ex)
            End Try
        End Function

        Private Function TryGetPsmHandle(handle As Object, ByRef value As PsmUdfHandle) As Boolean
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|aa60443d6cb0165b@38494:177")
        Return TryGetCachedHandle(handle, _psmCache, value)
        End Function

        Private Function BuildFitOptions(method As Object,
                                         estimand As Object,
                                         scoreMethod As Object,
                                         matchingOptions As Object,
                                         diagnosticOptions As Object) As PsmComprehensiveFitOptions
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|3f491d6b02116d15@38683:1197")
        Dim fitOptions As New PsmComprehensiveFitOptions()
            fitOptions.StandardOptions = New PsmOptions()
            fitOptions.RunMethod = ParseRunMethod(ExcelArgReaders.AsString(method), PsmBackendRunMethod.StandardNearestNeighbor)
            fitOptions.StandardOptions.Estimand = ParseEstimand(ExcelArgReaders.AsString(estimand), PsmEstimand.ATT)
            fitOptions.StandardOptions.ScoreMethod = ParseScoreMethod(ExcelArgReaders.AsString(scoreMethod), PsmScoreMethod.LogisticRegression)
            ApplyOptionText(fitOptions, ExcelArgReaders.AsString(matchingOptions))
            ApplyOptionText(fitOptions, ExcelArgReaders.AsString(diagnosticOptions))
            NormalizeFitOptions(fitOptions)
            PsmMethodCapabilities.ValidateFitOptions(fitOptions)
            Return fitOptions
        End Function

        Private Sub NormalizeFitOptions(fitOptions As PsmComprehensiveFitOptions)
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|SubBlock|38ce4e9fef3d8a4a@39892:1065")
        Dim o As PsmOptions = fitOptions.StandardOptions
            If fitOptions.RunMethod = PsmBackendRunMethod.OptimalPairMatching Then
                o.MatchingRatio = 1
                o.WithReplacement = False
            End If
            If Not PsmMethodCapabilities.UsesMatchingControls(fitOptions.RunMethod) Then
                o.MatchingRatio = 1
                o.WithReplacement = False
                o.DistanceMetric = PsmDistanceMetric.PropensityScore
                o.CaliperScale = PsmCaliperScale.None
                o.Caliper = Double.NaN
            End If
            If fitOptions.RunMethod = PsmBackendRunMethod.CoarsenedExactMatching AndAlso fitOptions.CoarseningSpec Is Nothing Then
                fitOptions.CoarseningSpec = New PsmCoarseningSpec With {
                    .Estimand = o.Estimand,
                    .NormalizeWeightsToSampleSize = o.NormalizeWeightsToSampleSize
                }
            End If
        End Sub

        Private Sub ApplyOptionText(fitOptions As PsmComprehensiveFitOptions, optionText As String)
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|SubBlock|ecf499688e2ec662@40969:740")
        If String.IsNullOrWhiteSpace(optionText) Then Return
            Dim parts As String() = optionText.Split({";"c, ChrW(10), ChrW(13)}, StringSplitOptions.RemoveEmptyEntries)
            For Each raw As String In parts
                Dim p As String = raw.Trim()
                If p.Length = 0 Then Continue For
                Dim eq As Integer = p.IndexOf("="c)
                Dim key As String = NormalizeOptionKey(If(eq >= 0, p.Substring(0, eq), p))
                Dim value As String = If(eq >= 0, p.Substring(eq + 1).Trim(), "true")
                ApplyOption(fitOptions, key, value)
            Next
        End Sub

        Private Sub ApplyOption(fitOptions As PsmComprehensiveFitOptions, key As String, value As String)
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|SubBlock|66c51bd9d385759b@41721:5411")
        Dim o As PsmOptions = fitOptions.StandardOptions
            Select Case key
                Case "method", "runmethod"
                    fitOptions.RunMethod = ParseRunMethod(value, fitOptions.RunMethod)
                Case "estimand"
                    o.Estimand = ParseEstimand(value, o.Estimand)
                Case "scoremethod", "score"
                    o.ScoreMethod = ParseScoreMethod(value, o.ScoreMethod)
                Case "distance", "distancemetric"
                    o.DistanceMetric = ParseDistance(value, o.DistanceMetric)
                Case "ratio", "matchingratio", "k"
                    o.MatchingRatio = Math.Max(1, ParseInteger(value, o.MatchingRatio))
                Case "replacement", "withreplacement"
                    o.WithReplacement = ParseBoolean(value, o.WithReplacement)
                Case "caliperscale", "calipertype"
                    o.CaliperScale = ParseCaliperScale(value, o.CaliperScale)
                Case "caliper"
                    o.Caliper = ParseDouble(value, o.Caliper)
                Case "order", "matchingorder"
                    o.MatchingOrder = ParseOrder(value, o.MatchingOrder)
                Case "support", "commonsupport"
                    o.CommonSupport = ParseSupport(value, o.CommonSupport)
                Case "seed", "randomseed"
                    o.RandomSeed = ParseInteger(value, o.RandomSeed)
                Case "intercept", "includeintercept"
                    o.IncludeIntercept = ParseBoolean(value, o.IncludeIntercept)
                Case "standardize", "standardizecovariates"
                    o.StandardizeCovariates = ParseBoolean(value, o.StandardizeCovariates)
                Case "maxiter", "logisticmaxiterations"
                    o.LogisticMaxIterations = Math.Max(1, ParseInteger(value, o.LogisticMaxIterations))
                Case "tol", "tolerance", "logistictolerance"
                    o.LogisticTolerance = ParseDouble(value, o.LogisticTolerance)
                Case "ridge", "ridgepenalty", "logisticridgepenalty"
                    o.LogisticRidgePenalty = ParseDouble(value, o.LogisticRidgePenalty)
                Case "smd", "smdthreshold", "balancesmdthreshold"
                    o.BalanceSmdThreshold = ParseDouble(value, o.BalanceSmdThreshold)
                    fitOptions.LovePlotThreshold = o.BalanceSmdThreshold
                Case "vrlower", "varianceratiolower"
                    o.BalanceVarianceRatioLower = ParseDouble(value, o.BalanceVarianceRatioLower)
                Case "vrupper", "varianceratioupper"
                    o.BalanceVarianceRatioUpper = ParseDouble(value, o.BalanceVarianceRatioUpper)
                Case "strata", "subclasses", "subclassificationstrata"
                    o.SubclassificationStrata = Math.Max(2, ParseInteger(value, o.SubclassificationStrata))
                Case "normalize", "normalizeweights", "normalizeweightstosamplesize"
                    o.NormalizeWeightsToSampleSize = ParseBoolean(value, o.NormalizeWeightsToSampleSize)
                Case "trimlower", "trimpropensitylower"
                    o.TrimPropensityLower = ParseDouble(value, o.TrimPropensityLower)
                Case "trimupper", "trimpropensityupper"
                    o.TrimPropensityUpper = ParseDouble(value, o.TrimPropensityUpper)
                Case "dr", "aipw", "doublyrobust"
                    fitOptions.IncludeDoublyRobustEstimate = ParseBoolean(value, fitOptions.IncludeDoublyRobustEstimate)
                Case "overlap", "overlapdiagnostics"
                    fitOptions.IncludeOverlapDiagnostics = ParseBoolean(value, fitOptions.IncludeOverlapDiagnostics)
                Case "weightdiagnostics"
                    fitOptions.IncludeWeightDiagnostics = ParseBoolean(value, fitOptions.IncludeWeightDiagnostics)
                Case "loveplot", "loveplotrows"
                    fitOptions.IncludeLovePlotRows = ParseBoolean(value, fitOptions.IncludeLovePlotRows)
                Case "overlapbins", "overlapbincount"
                    fitOptions.OverlapBinCount = Math.Max(2, ParseInteger(value, fitOptions.OverlapBinCount))
                Case "extremeweight", "extremeweightcutoff"
                    fitOptions.ExtremeWeightCutoff = ParseDouble(value, fitOptions.ExtremeWeightCutoff)
                Case "cembins", "bins", "defaultcovariatebins"
                    EnsureCoarseningSpec(fitOptions)
                    fitOptions.CoarseningSpec.DefaultCovariateBins = Math.Max(2, ParseInteger(value, fitOptions.CoarseningSpec.DefaultCovariateBins))
                    fitOptions.CoarseningSpec.PropensityScoreBins = fitOptions.CoarseningSpec.DefaultCovariateBins
                Case "psbins", "propensityscorebins"
                    EnsureCoarseningSpec(fitOptions)
                    fitOptions.CoarseningSpec.PropensityScoreBins = Math.Max(2, ParseInteger(value, fitOptions.CoarseningSpec.PropensityScoreBins))
                Case "includeps", "includepropensityscore"
                    EnsureCoarseningSpec(fitOptions)
                    fitOptions.CoarseningSpec.IncludePropensityScore = ParseBoolean(value, fitOptions.CoarseningSpec.IncludePropensityScore)
            End Select
        End Sub

        Private Sub EnsureCoarseningSpec(fitOptions As PsmComprehensiveFitOptions)
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|SubBlock|528895e7de53e619@47144:413")
        If fitOptions.CoarseningSpec Is Nothing Then fitOptions.CoarseningSpec = New PsmCoarseningSpec()
            fitOptions.CoarseningSpec.Estimand = fitOptions.StandardOptions.Estimand
            fitOptions.CoarseningSpec.NormalizeWeightsToSampleSize = fitOptions.StandardOptions.NormalizeWeightsToSampleSize
        End Sub

        Private Function ResolveNamedTable(h As PsmUdfHandle, requested As String) As Object(,)
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|a2bb4382b9645aeb@47569:2926")
        Dim name As String = NormalizeOptionKey(requested)
            Select Case name
                Case "summary", "runsummary"
                    Return PsmComprehensiveTables.RunSummaryTable(h.Result)
                Case "options"
                    Return PsmFrontEndTables.OptionsTable(h.FitOptions)
                Case "import", "dataimport"
                    Return DataImportSummaryTable(h)
                Case "sample", "samplesize", "samplesizesummary"
                    Return PsmFrontEndTables.SampleSizeTable(h.Result.Result)
                Case "scoremodel", "model"
                    Return PsmBackendTables.ScoreModelTable(h.Result.Result)
                Case "scores", "audit", "rowaudit", "weights"
                    Return PsmFrontEndTables.RowLevelAuditTable(h.Data.Input, h.Result.Result, h.Data.RowIds)
                Case "matches", "matchedpairs"
                    Return PsmBackendTables.MatchesTable(h.Result.Result, h.Data.Input)
                Case "matcheddata", "matcheddataset"
                    Return PsmFrontEndTables.MatchedDatasetTable(h.Data.Input, h.Result.Result, h.Data.RowIds)
                Case "balance", "diagnostics"
                    Return PsmBackendTables.BalanceTable(h.Result.Result)
                Case "effect", "effects"
                    Return PsmBackendTables.EffectTable(h.Result.Result)
                Case "dr", "aipw", "doublyrobust"
                    Return PsmAdvancedTables.DoublyRobustEffectTable(h.Result.DoublyRobustResult)
                Case "weightdiagnostics"
                    Return PsmAdvancedTables.WeightDiagnosticsTable(h.Result.WeightDiagnostics)
                Case "overlap", "overlapsummary"
                    Return PsmAdvancedTables.OverlapSummaryTable(h.Result.OverlapDiagnostics)
                Case "overlapbins", "overlaphistogram"
                    Return PsmAdvancedTables.OverlapBinsTable(h.Result.OverlapDiagnostics)
                Case "loveplot", "loveplotdata"
                    Return PsmAdvancedTables.LovePlotTable(h.Result.LovePlotRows)
                Case "subclasses", "subclassification"
                    Return PsmFrontEndTables.SubclassTable(h.Result.Result)
                Case "cemstrata"
                    Return PsmAdvancedMatchingTables.CoarsenedExactStrataTable(h.Result.CoarsenedExactResult)
                Case "cemweights"
                    Return PsmAdvancedMatchingTables.CoarsenedExactWeightsTable(h.Data.Input, h.Result.CoarsenedExactResult)
                Case "warnings"
                    Return PsmComprehensiveTables.WarningsTable(h.Result)
                Case Else
                    Return PsmResult.EmptyTable("Unknown PSM table: " & If(requested, String.Empty))
            End Select
        End Function

        Private Function DataImportSummaryTable(h As PsmUdfHandle) As Object(,)
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|0198aa4b3fc209b6@50507:1420")
        Dim rows As New List(Of Object()) From {
                New Object() {"Source", "Worksheet UDF"},
                New Object() {"Imported reference", If(h.Data Is Nothing, "", h.Data.ImportedReferenceSummary)},
                New Object() {"Rows used", If(h.Data Is Nothing OrElse h.Data.Input Is Nothing, 0, h.Data.Input.RowCount)},
                New Object() {"Rows dropped while aligning outcome/score/exact/ID", If(h.Data Is Nothing, 0, h.Data.DroppedRowsDuringAlignment)},
                New Object() {"Raw covariates", If(h.Data Is Nothing OrElse h.Data.RawCovariateKeys Is Nothing, "", String.Join(", ", h.Data.RawCovariateKeys.ToArray()))},
                New Object() {"Expanded covariates", If(h.Data Is Nothing OrElse h.Data.Input Is Nothing OrElse h.Data.Input.CovariateNames Is Nothing, "", String.Join(", ", h.Data.Input.CovariateNames))},
                New Object() {"Created UTC", If(h Is Nothing, "", h.CreatedUtc.ToString("u", CultureInfo.InvariantCulture))}
            }

            Dim table(rows.Count, 1) As Object
            table(0, 0) = "Item"
            table(0, 1) = "Value"
            For i As Integer = 0 To rows.Count - 1
                table(i + 1, 0) = rows(i)(0)
                table(i + 1, 1) = rows(i)(1)
            Next
            Return table
        End Function

        Private Function ParseRunMethod(value As String, defaultValue As PsmBackendRunMethod) As PsmBackendRunMethod
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|d521fef63ad9e043@51939:1065")
        Dim s As String = NormalizeOptionKey(value)
            Select Case s
                Case "matching", "nearest", "nearestneighbor", "nn", "standardnearestneighbor"
                    Return PsmBackendRunMethod.StandardNearestNeighbor
                Case "weighting", "weights", "iptw", "overlap", "weightingonly"
                    Return PsmBackendRunMethod.WeightingOnly
                Case "subclassification", "subclass", "strata", "standardsubclassification"
                    Return PsmBackendRunMethod.StandardSubclassification
                Case "optimal", "optimalpair", "optimalpairmatching"
                    Return PsmBackendRunMethod.OptimalPairMatching
                Case "cem", "coarsenedexact", "coarsenedexactmatching"
                    Return PsmBackendRunMethod.CoarsenedExactMatching
                Case Else
                    Return defaultValue
            End Select
        End Function

        Private Function ParseEstimand(value As String, defaultValue As PsmEstimand) As PsmEstimand
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|ddeb09debb5d238b@53016:211")
        Return CType(ParseEnumValue(GetType(PsmEstimand), value, defaultValue), PsmEstimand)
        End Function

        Private Function ParseScoreMethod(value As String, defaultValue As PsmScoreMethod) As PsmScoreMethod
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|6bbfbb3cd115c227@53239:556")
        Dim s As String = NormalizeOptionKey(value)
            Select Case s
                Case "supplied", "existing", "score", "propensityscore"
                    Return PsmScoreMethod.Supplied
                Case "logit", "logistic", "logisticregression", "glm"
                    Return PsmScoreMethod.LogisticRegression
                Case Else
                    Return defaultValue
            End Select
        End Function

        Private Function ParseDistance(value As String, defaultValue As PsmDistanceMetric) As PsmDistanceMetric
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|675a03c3687b0bda@53807:872")
        Dim s As String = NormalizeOptionKey(value)
            Select Case s
                Case "ps", "propensity", "propensityscore"
                    Return PsmDistanceMetric.PropensityScore
                Case "logit", "logitps", "logitpropensity", "logitpropensityscore"
                    Return PsmDistanceMetric.LogitPropensityScore
                Case "mahalanobis"
                    Return PsmDistanceMetric.Mahalanobis
                Case "mahalanobiswithpropensitycaliper", "mahalanobiswithpscaliper", "mahalanobiswithinpropensitycaliper"
                    Return PsmDistanceMetric.MahalanobisWithinPropensityCaliper
                Case Else
                    Return defaultValue
            End Select
        End Function

        Private Function ParseCaliperScale(value As String, defaultValue As PsmCaliperScale) As PsmCaliperScale
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|2485a76036aa8e22@54691:1012")
        Dim s As String = NormalizeOptionKey(value)
            Select Case s
                Case "none", "no", "false", "0"
                    Return PsmCaliperScale.None
                Case "raw", "ps", "rawps", "rawpropensity", "rawpropensityscore"
                    Return PsmCaliperScale.RawPropensityScore
                Case "sd", "sdps", "standardized", "standardizedps", "standardizedpropensityscore"
                    Return PsmCaliperScale.StandardizedPropensityScore
                Case "logit", "logitps", "logitpropensityscore"
                    Return PsmCaliperScale.LogitPropensityScore
                Case "sdlogit", "standardizedlogit", "standardizedlogitpropensityscore"
                    Return PsmCaliperScale.StandardizedLogitPropensityScore
                Case Else
                    Return defaultValue
            End Select
        End Function

        Private Function ParseOrder(value As String, defaultValue As PsmMatchingOrder) As PsmMatchingOrder
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|a4c067785b072089@55715:882")
        Dim s As String = NormalizeOptionKey(value)
            Select Case s
                Case "asinput", "input", "original"
                    Return PsmMatchingOrder.AsInput
                Case "ascending", "psascending", "propensityascending"
                    Return PsmMatchingOrder.PropensityAscending
                Case "descending", "psdescending", "propensitydescending"
                    Return PsmMatchingOrder.PropensityDescending
                Case "random"
                    Return PsmMatchingOrder.Random
                Case "hardest", "hardestfirst", "bestfirst"
                    Return PsmMatchingOrder.HardestFirst
                Case Else
                    Return defaultValue
            End Select
        End Function

        Private Function ParseSupport(value As String, defaultValue As PsmCommonSupportMode) As PsmCommonSupportMode
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|d05992834cd8c5aa@56609:900")
        Dim s As String = NormalizeOptionKey(value)
            Select Case s
                Case "none", "no", "false", "0"
                    Return PsmCommonSupportMode.None
                Case "overlap", "minmax", "dropoutsideoverlaprange"
                    Return PsmCommonSupportMode.DropOutsideOverlapRange
                Case "treated", "dropoutsidetreated", "dropcontrolsoutsidetreatedrange"
                    Return PsmCommonSupportMode.DropControlsOutsideTreatedRange
                Case "control", "controls", "dropoutsidcontrol", "droptreatedoutsidecontrolrange"
                    Return PsmCommonSupportMode.DropTreatedOutsideControlRange
                Case Else
                    Return defaultValue
            End Select
        End Function

        Private Function ParseEnumValue(enumType As Type, value As String, defaultValue As Object) As Object
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|e49a77ed490dd329@57521:357")
        If String.IsNullOrWhiteSpace(value) Then Return defaultValue
            Try
                Return [Enum].Parse(enumType, value.Trim(), True)
            Catch
                Return defaultValue
            End Try
        End Function

        Private Function ParseBoolean(value As String, defaultValue As Boolean) As Boolean
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|3838b7ccbc018d3d@57890:509")
        If String.IsNullOrWhiteSpace(value) Then Return defaultValue
            Select Case value.Trim().ToLowerInvariant()
                Case "true", "t", "yes", "y", "1", "on"
                    Return True
                Case "false", "f", "no", "n", "0", "off"
                    Return False
                Case Else
                    Return defaultValue
            End Select
        End Function

        Private Function ParseInteger(value As String, defaultValue As Integer) As Integer
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|264e91352870dabf@58411:291")
        Dim parsed As Integer
            If Integer.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, parsed) Then Return parsed
            Return defaultValue
        End Function

        Private Function ParseDouble(value As String, defaultValue As Double) As Double
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|267c69b4ffc64d5f@58714:460")
        Dim parsed As Double
            If Double.TryParse(value, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture, parsed) Then Return parsed
            If Double.TryParse(value, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, parsed) Then Return parsed
            Return defaultValue
        End Function

        Private Function NormalizeOptionKey(value As String) As String
            Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb|BESHStatNG.WorksheetFunctions.PropensityScoreUDFs|FunctionBlock|ff25a1167e816e49@59186:386")
        If value Is Nothing Then Return String.Empty
            Dim chars As New List(Of Char)()
            For Each ch As Char In value.Trim().ToLowerInvariant()
                If Char.IsLetterOrDigit(ch) Then chars.Add(ch)
            Next
            Return New String(chars.ToArray())
        End Function

    End Module

End Namespace
