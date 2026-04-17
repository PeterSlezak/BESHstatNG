Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for Zero-Inflated Poisson (ZIP) regression fitted through <see cref="ZeroInflatedPoisson"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These worksheet functions expose a handle-based Excel interface to the Zero-Inflated Poisson model for count outcomes
    ''' with excess zeros. The model combines two linked submodels:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>A Poisson count component with mean <c>λ_i = exp(η_{c,i})</c>.</description></item>
    ''' <item><description>A logistic zero-inflation component with structural-zero probability <c>π_i = logistic(η_{z,i})</c>.</description></item>
    ''' </list>
    ''' <para>
    ''' For observation <c>i</c>, with count-part linear predictor <c>η_{c,i} = β_0 + x_i'β + o_i</c>
    ''' and zero-part linear predictor <c>η_{z,i} = γ_0 + z_i'γ</c>, the fitted ZIP mean is
    ''' <c>E[Y_i | x_i, z_i] = (1 - π_i) λ_i</c>.
    ''' </para>
    ''' <para>
    ''' The ZIP probability mass function is
    ''' <c>P(Y_i = 0) = π_i + (1 - π_i) exp(-λ_i)</c>
    ''' and
    ''' <c>P(Y_i = k &gt; 0) = (1 - π_i) exp(-λ_i) λ_i^k / k!</c>.
    ''' </para>
    ''' <para>
    ''' Fitting uses the project&apos;s <see cref="ZeroInflatedPoisson"/> implementation, which applies an EM algorithm.
    ''' In the E-step, the model computes the posterior probability that an observed zero is structural.
    ''' In the M-step, it updates the Poisson mean model and the logistic zero model via GLM fits, with a monotone
    ''' over-relaxation fallback to preserve nondecreasing observed-data log-likelihood.
    ''' </para>
    ''' <para>
    ''' The fit function stores the fitted model in an in-memory cache for the current Excel session and returns a text handle.
    ''' Other worksheet functions reuse that handle to retrieve coefficient tables, diagnostics, residuals, predictions,
    ''' and explicit cache cleanup without refitting.
    ''' </para>
    ''' <para>
    ''' The count and zero components each use the same regression-formula infrastructure already used by the other regression UDF modules,
    ''' so continuous effects, authored categorical terms, polynomial terms, and interactions can be defined independently for each component.
    ''' </para>
    ''' </remarks>
    Public Module ZIPUDFs

        Private ReadOnly _zipCache As New ConcurrentDictionary(Of String, ZipHandle)(StringComparer.OrdinalIgnoreCase)

        Private Class ZipHandle
            Public Property Handle As String
            Public Property Model As ZeroInflatedPoisson
            Public Property CountVarNames As String()
            Public Property ZeroVarNames As String()
            Public Property CountExpandedPredictorNames As String()
            Public Property ZeroExpandedPredictorNames As String()
            Public Property RawCountVarNames As String()
            Public Property RawZeroVarNames As String()
            Public Property RawCountPredictorKeys As String()
            Public Property RawZeroPredictorKeys As String()
            Public Property RawCountPredictorAbsoluteLetters As String()
            Public Property RawZeroPredictorAbsoluteLetters As String()
            Public Property CountDesignSpec As RegressionFormulaDesignSpec
            Public Property ZeroDesignSpec As RegressionFormulaDesignSpec
            Public Property CountOmitCategoricalReference As Boolean
            Public Property ZeroOmitCategoricalReference As Boolean
            Public Property HasOffset As Boolean
            Public Property CountInterceptIncluded As Boolean
            Public Property ZeroInterceptIncluded As Boolean
            Public Property ConfidenceAlpha As Double
            Public Property Response As Integer()
        End Class

        ''' <summary>
        ''' Fits a Zero-Inflated Poisson regression model and returns a reusable handle.
        ''' </summary>
        ''' <param name="y">
        ''' Integer-valued nonnegative response vector (single column) containing observed counts.
        ''' Each row corresponds to one observation.
        ''' </param>
        ''' <param name="xCount">
        ''' Raw predictor matrix for the Poisson count component, with one row per observation.
        ''' Rows must align with <paramref name="y"/>, <paramref name="xZero"/>, and <paramref name="offset"/> when supplied.
        ''' </param>
        ''' <param name="xZero">
        ''' Optional raw predictor matrix for the logistic zero-inflation component.
        ''' When omitted, the function reuses <paramref name="xCount"/> as the raw zero-component predictor matrix.
        ''' </param>
        ''' <param name="countVarNames">
        ''' Optional raw predictor names for the count component, supplied either as a comma-separated string
        ''' or as a one-row/one-column range.
        ''' </param>
        ''' <param name="zeroVarNames">
        ''' Optional raw predictor names for the zero component, supplied either as a comma-separated string
        ''' or as a one-row/one-column range. When omitted and <paramref name="xZero"/> is omitted, the function reuses
        ''' the count-component raw predictor names.
        ''' </param>
        ''' <param name="offset">
        ''' Optional numeric offset vector for the Poisson count component only.
        ''' The offset enters additively on the log-mean scale:
        ''' <c>log(λ_i) = β_0 + x_i'β + o_i</c>.
        ''' A common rate-model choice is <c>o_i = log(t_i)</c> for exposure <c>t_i</c>.
        ''' </param>
        ''' <param name="includeCountIntercept">
        ''' TRUE to include an intercept in the Poisson count component (default TRUE).
        ''' </param>
        ''' <param name="includeZeroIntercept">
        ''' TRUE to include an intercept in the logistic zero component (default TRUE).
        ''' </param>
        ''' <param name="countFormula">
        ''' Optional right-hand-side formula used to expand the raw count-component predictor matrix before fitting.
        ''' If omitted or blank, all raw count predictors enter as continuous main effects.
        ''' </param>
        ''' <param name="zeroFormula">
        ''' Optional right-hand-side formula used to expand the raw zero-component predictor matrix before fitting.
        ''' If omitted or blank, all raw zero predictors enter as continuous main effects.
        ''' </param>
        ''' <param name="formulaAddressing">
        ''' Formula-addressing mode shared by both formulas: <c>relative</c> (default), <c>absolute</c>, or <c>names</c>.
        ''' </param>
        ''' <param name="maxEmIter">
        ''' Maximum number of EM iterations (default 200).
        ''' </param>
        ''' <param name="maxIrlsIter">
        ''' Maximum number of IRLS iterations used inside each M-step GLM fit (default 25).
        ''' </param>
        ''' <param name="tol">
        ''' Positive convergence tolerance for the absolute observed-data log-likelihood change (default 1E-9).
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level used for Wald confidence intervals stored in the fitted result objects (default 0.05).
        ''' </param>
        ''' <returns>
        ''' A text handle identifying the fitted Zero-Inflated Poisson model in the current Excel session.
        ''' The handle can be passed to the other <c>ZIP_*</c> worksheet functions to retrieve summaries, diagnostics,
        ''' residuals, and predictions without refitting.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function fits the model
        ''' <c>Y_i ~ ZIP(λ_i, π_i)</c>
        ''' with
        ''' <c>λ_i = exp(β_0 + x_i'β + o_i)</c>
        ''' and
        ''' <c>π_i = logistic(γ_0 + z_i'γ)</c>.
        ''' The unconditional ZIP mean and variance are
        ''' <c>E[Y_i] = (1 - π_i) λ_i</c>
        ''' and
        ''' <c>Var(Y_i) = (1 - π_i) λ_i (1 + π_i λ_i)</c>.
        ''' </para>
        ''' <para>
        ''' The implemented EM algorithm alternates between:
        ''' </para>
        ''' <list type="number">
        ''' <item><description>E-step: for zeros, compute the posterior structural-zero probability <c>τ_i = P(S_i = 1 | Y_i = 0)</c>.</description></item>
        ''' <item><description>M-step count update: fit a Poisson/log GLM to the observed counts with weights <c>1 - τ_i</c>.</description></item>
        ''' <item><description>M-step zero update: fit a Binomial/logit GLM to the fractional response <c>τ_i</c>.</description></item>
        ''' </list>
        ''' <para>
        ''' After the plain EM update, the underlying engine attempts an over-relaxed step and falls back monotonically when needed,
        ''' so the observed-data log-likelihood does not decrease.
        ''' </para>
        ''' <para>
        ''' The count and zero components may use different raw predictor matrices and different formulas. Rows containing invalid or
        ''' missing values in any required argument are excluded jointly, so both submodels remain aligned on the same retained observations.
        ''' </para>
        ''' <para>
        ''' If <c>formulaAddressing=&quot;absolute&quot;</c> is used, the relevant predictor arguments should be direct worksheet ranges so that absolute
        ''' worksheet column letters can be resolved for formula parsing.
        ''' </para>
        ''' <para>
        ''' Unlike the GLM and GLM_NB worksheet functions, this model does not accept case weights because the underlying
        ''' <see cref="ZeroInflatedPoisson"/> implementation exposes a Poisson-part offset but not a user-facing case-weight argument.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.ZIP_FIT(A2:A201,B2:D201)
        ''' =BESH.REGR.ZIP_FIT(A2:A201,B2:D201,E2:G201,"Age,BMI,Treat","Age,Stage,Smoker",H2:H201,TRUE,TRUE,"factor(C)+A","factor(B)+C")
        ''' =BESH.REGR.ZIP_FIT(A2:A201,B2:E201,,"Dose,Age,Stage,Center",,TRUE,FALSE,"A + factor(C) + A*B","factor(D)")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.ZIP_FIT",
            Category:="BESHStatNG - Regression Models",
            Description:="Fits a Zero-Inflated Poisson regression model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ZIP_FIT(
            <ExcelArgument(Name:="y", Description:="Integer-valued nonnegative response vector (single column) of observed counts.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="xCount", Description:="Raw predictor matrix for the Poisson count component.")> xCount As Object,
            <ExcelArgument(AllowReference:=True, Name:="xZero", Description:="Optional raw predictor matrix for the logistic zero component. Defaults to xCount.")> Optional xZero As Object = Nothing,
            <ExcelArgument(Name:="countVarNames", Description:="Optional raw predictor names for the count component as a comma-separated list or a one-row/one-column range.")> Optional countVarNames As Object = Nothing,
            <ExcelArgument(Name:="zeroVarNames", Description:="Optional raw predictor names for the zero component as a comma-separated list or a one-row/one-column range.")> Optional zeroVarNames As Object = Nothing,
            <ExcelArgument(Name:="offset", Description:="Optional numeric offset vector for the Poisson count component only.")> Optional offset As Object = Nothing,
            <ExcelArgument(Name:="includeCountIntercept", Description:="TRUE to include an intercept in the count component (default TRUE).")> Optional includeCountIntercept As Object = Nothing,
            <ExcelArgument(Name:="includeZeroIntercept", Description:="TRUE to include an intercept in the zero component (default TRUE).")> Optional includeZeroIntercept As Object = Nothing,
            <ExcelArgument(Name:="countFormula", Description:="Optional RHS formula used to expand the raw count-component predictor matrix.")> Optional countFormula As Object = Nothing,
            <ExcelArgument(Name:="zeroFormula", Description:="Optional RHS formula used to expand the raw zero-component predictor matrix.")> Optional zeroFormula As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Shared formula-addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing,
            <ExcelArgument(Name:="maxEmIter", Description:="Maximum number of EM iterations (default 200).")> Optional maxEmIter As Object = Nothing,
            <ExcelArgument(Name:="maxIrlsIter", Description:="Maximum number of IRLS iterations inside each M-step GLM fit (default 25).")> Optional maxIrlsIter As Object = Nothing,
            <ExcelArgument(Name:="tol", Description:="Convergence tolerance for the absolute observed-data log-likelihood change (default 1E-9).")> Optional tol As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha used internally for confidence intervals (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "ZIP_FIT (editing...)"

            Try
                Dim effectiveZeroX As Object = If(Not IsMissingArg(xZero), xZero, xCount)
                Dim effectiveZeroVarNames As Object = If(Not IsMissingArg(zeroVarNames), zeroVarNames, countVarNames)

                Dim countData As glmData = Nothing
                Dim zeroData As glmData = Nothing
                If Not TryBuildZipDataFromUdfArgs(y, xCount, effectiveZeroX, countVarNames, effectiveZeroVarNames, offset, countData, zeroData) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim countFormulaText As String = UDFhelpers.AsString(countFormula)
                If String.IsNullOrWhiteSpace(countFormulaText) Then countFormulaText = Nothing

                Dim zeroFormulaText As String = UDFhelpers.AsString(zeroFormula)
                If String.IsNullOrWhiteSpace(zeroFormulaText) Then zeroFormulaText = Nothing

                Dim addressingMode As String = UDFhelpers.ParseFormulaAddressingMode(formulaAddressing, "relative")
                Dim allowRelativeColumnLetters As Boolean = False
                Dim allowAbsoluteColumnLetters As Boolean = False
                Dim allowQuotedVariableNames As Boolean = True

                Select Case addressingMode
                    Case "absolute"
                        allowAbsoluteColumnLetters = True
                    Case "names"
                        allowQuotedVariableNames = True
                    Case Else
                        allowRelativeColumnLetters = True
                        addressingMode = "relative"
                End Select

                Dim countAbsoluteLetters As String() = Nothing
                If allowAbsoluteColumnLetters AndAlso Not String.IsNullOrWhiteSpace(countFormulaText) Then
                    If Not UDFhelpers.TryGetAbsoluteColumnLettersFromRange(xCount, countData.nCols - 1, countAbsoluteLetters) Then
                        Return ExcelError.ExcelErrorValue
                    End If
                End If

                Dim zeroAbsoluteLetters As String() = Nothing
                If allowAbsoluteColumnLetters AndAlso Not String.IsNullOrWhiteSpace(zeroFormulaText) Then
                    If Not UDFhelpers.TryGetAbsoluteColumnLettersFromRange(effectiveZeroX, zeroData.nCols - 1, zeroAbsoluteLetters) Then
                        Return ExcelError.ExcelErrorValue
                    End If
                End If

                Dim countInterceptFlag As Boolean = UDFhelpers.GetOptionalBool(includeCountIntercept, True)
                Dim zeroInterceptFlag As Boolean = UDFhelpers.GetOptionalBool(includeZeroIntercept, True)

                Dim countDesignBuild As RegressionFormulaRegressionDataBuildResult = Nothing
                Dim countDesignErr As String = Nothing
                If Not RegressionFormulaDesignService.TryBuildExpandedRegressionDataMatrixFromFormula(raw:=countData,
                                                                                                     yKey:=countData.varNames(0),
                                                                                                     result:=countDesignBuild,
                                                                                                     errorMessage:=countDesignErr,
                                                                                                     formulaText:=countFormulaText,
                                                                                                     absoluteColumnLetters:=countAbsoluteLetters,
                                                                                                     allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                                                                     allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                                                                     allowQuotedVariableNames:=allowQuotedVariableNames,
                                                                                                     omitCategoricalReference:=countInterceptFlag) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim zeroDesignBuild As RegressionFormulaRegressionDataBuildResult = Nothing
                Dim zeroDesignErr As String = Nothing
                If Not RegressionFormulaDesignService.TryBuildExpandedRegressionDataMatrixFromFormula(raw:=zeroData,
                                                                                                     yKey:=zeroData.varNames(0),
                                                                                                     result:=zeroDesignBuild,
                                                                                                     errorMessage:=zeroDesignErr,
                                                                                                     formulaText:=zeroFormulaText,
                                                                                                     absoluteColumnLetters:=zeroAbsoluteLetters,
                                                                                                     allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                                                                     allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                                                                     allowQuotedVariableNames:=allowQuotedVariableNames,
                                                                                                     omitCategoricalReference:=zeroInterceptFlag) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim fitCountData As Double(,) = countDesignBuild.RegressionDataMatrix
                Dim fitCountVarNames As String() = countDesignBuild.RegressionDataVarNames
                Dim fitCountPredictorNames As String() = If(countDesignBuild.ExpandedPredictorNames, New String() {})

                Dim fitZeroData As Double(,) = zeroDesignBuild.RegressionDataMatrix
                Dim fitZeroVarNames As String() = zeroDesignBuild.RegressionDataVarNames
                Dim fitZeroPredictorNames As String() = If(zeroDesignBuild.ExpandedPredictorNames, New String() {})

                Dim fitOffset() As Double = If(countData.bOffset, countData.OffsetData, Nothing)
                Dim rowIds() As Integer = countData.RowIds
                If fitCountData Is Nothing OrElse fitCountVarNames Is Nothing OrElse fitCountVarNames.Length < 1 Then Return ExcelError.ExcelErrorValue
                If fitZeroData Is Nothing OrElse fitZeroVarNames Is Nothing OrElse fitZeroVarNames.Length < 1 Then Return ExcelError.ExcelErrorValue
                If Not UDFhelpers.HasOnlyFinite(fitOffset) Then Return ExcelError.ExcelErrorValue
                If fitCountData.GetLength(0) <> fitZeroData.GetLength(0) Then Return ExcelError.ExcelErrorValue
                If rowIds Is Nothing OrElse rowIds.Length <> fitCountData.GetLength(0) Then Return ExcelError.ExcelErrorValue

                If Not countInterceptFlag AndAlso fitCountVarNames.Length < 2 Then Return ExcelError.ExcelErrorNum
                If Not zeroInterceptFlag AndAlso fitZeroVarNames.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim response() As Integer = Nothing
                If Not TryExtractNonnegativeIntegerResponse(fitCountData, response) Then Return ExcelError.ExcelErrorNum
                If Not ResponseColumnsMatch(fitCountData, fitZeroData) Then Return ExcelError.ExcelErrorValue

                Dim ciAlpha As Double = 0.05R
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, ciAlpha) Then Return ExcelError.ExcelErrorNum
                End If

                Dim maxEmValue As Integer = UDFhelpers.GetOptionalInt(maxEmIter, 200)
                Dim maxIrlsValue As Integer = UDFhelpers.GetOptionalInt(maxIrlsIter, 25)
                Dim tolValue As Double = UDFhelpers.GetOptionalDouble(tol, 0.000000001R)
                If maxEmValue < 1 Then Return ExcelError.ExcelErrorNum
                If maxIrlsValue < 1 Then Return ExcelError.ExcelErrorNum
                If Double.IsNaN(tolValue) OrElse Double.IsInfinity(tolValue) OrElse tolValue <= 0.0R Then Return ExcelError.ExcelErrorNum

                Dim mdl As New ZeroInflatedPoisson()
                mdl.bComputeResiduals = True
                mdl.bIterationDetails = False
                mdl.bReturnCov = False
                mdl.dataInputs(fitCountData,
                               fitZeroData,
                               fitCountVarNames,
                               fitZeroVarNames,
                               rowIds,
                               fitOffset,
                               If(countData.bOffset, countData.OffsetVarName, Nothing))
                mdl.settingInputs(ciAlpha, maxIrlsValue, maxEmValue, tolValue)
                mdl.Fit(If(countInterceptFlag, 1, 0), If(zeroInterceptFlag, 1, 0), False, False)

                If mdl.resultsPoisson Is Nothing OrElse mdl.resultsPoisson.Coeffs_est Is Nothing OrElse mdl.resultsPoisson.Coeffs_SEs Is Nothing Then
                    Return ExcelError.ExcelErrorValue
                End If
                If mdl.resultsLogistic Is Nothing OrElse mdl.resultsLogistic.Coeffs_est Is Nothing OrElse mdl.resultsLogistic.Coeffs_SEs Is Nothing Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim handleKey As String = "ZIP:" & Guid.NewGuid().ToString("N")
                Dim h As New ZipHandle With {
                    .Handle = handleKey,
                    .Model = mdl,
                    .CountVarNames = CloneStringArray(If(mdl.resultsPoisson.varNames, New String() {})),
                    .ZeroVarNames = CloneStringArray(If(mdl.resultsLogistic.varNames, New String() {})),
                    .CountExpandedPredictorNames = CloneStringArray(fitCountPredictorNames),
                    .ZeroExpandedPredictorNames = CloneStringArray(fitZeroPredictorNames),
                    .RawCountVarNames = CloneStringArray(If(countDesignBuild.FullRawPredictorNames, New String() {})),
                    .RawZeroVarNames = CloneStringArray(If(zeroDesignBuild.FullRawPredictorNames, New String() {})),
                    .RawCountPredictorKeys = CloneStringArray(If(countDesignBuild.FullRawPredictorKeys, New String() {})),
                    .RawZeroPredictorKeys = CloneStringArray(If(zeroDesignBuild.FullRawPredictorKeys, New String() {})),
                    .RawCountPredictorAbsoluteLetters = CloneStringArray(If(countDesignBuild.FullRawPredictorAbsoluteLetters, New String() {})),
                    .RawZeroPredictorAbsoluteLetters = CloneStringArray(If(zeroDesignBuild.FullRawPredictorAbsoluteLetters, New String() {})),
                    .CountDesignSpec = countDesignBuild.DesignSpec,
                    .ZeroDesignSpec = zeroDesignBuild.DesignSpec,
                    .CountOmitCategoricalReference = countInterceptFlag,
                    .ZeroOmitCategoricalReference = zeroInterceptFlag,
                    .HasOffset = (fitOffset IsNot Nothing),
                    .CountInterceptIncluded = countInterceptFlag,
                    .ZeroInterceptIncluded = zeroInterceptFlag,
                    .ConfidenceAlpha = ciAlpha,
                    .Response = CType(response.Clone(), Integer())
                }

                _zipCache(handleKey) = h
                Return handleKey

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.ZIP_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns coefficient summaries for the count and/or zero component of a fitted ZIP model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ZIP_FIT</c>.</param>
        ''' <param name="component">
        ''' Component selector: <c>all</c> (default), <c>count</c> / <c>poisson</c>, or <c>zero</c> / <c>logistic</c>.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used to construct the displayed Wald confidence intervals.
        ''' This argument affects only interval reporting and does not refit the model.
        ''' </param>
        ''' <returns>
        ''' A rectangular coefficient table containing the selected component(s), parameter labels, standard errors,
        ''' Wald z statistics, p-values, and confidence limits.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' For either component, the table reports Wald inference based on
        ''' <c>z_j = \hat θ_j / SE(\hat θ_j)</c>
        ''' with two-sided p-value
        ''' <c>2 Φ(-|z_j|)</c>.
        ''' A <c>(1-α)</c> confidence interval is reported as
        ''' <c>\hat θ_j ± z_{1-α/2} SE(\hat θ_j)</c>.
        ''' </para>
        ''' <para>
        ''' In the Poisson count component, coefficients live on the log-mean scale, so exponentiating a slope coefficient yields the
        ''' multiplicative change in <c>λ_i</c> associated with a one-unit change in the predictor, holding other component-specific predictors fixed.
        ''' </para>
        ''' <para>
        ''' In the logistic zero component, coefficients live on the log-odds scale for structural-zero membership.
        ''' Exponentiating a slope coefficient yields the multiplicative change in the structural-zero odds.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.ZIP_SUMMARY",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns coefficient summaries for the count and/or zero component of a fitted Zero-Inflated Poisson model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ZIP_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ZIP_FIT.")> handle As Object,
            <ExcelArgument(Name:="component", Description:="Component selector: ""all"" (default), ""count""/""poisson"", or ""zero""/""logistic"".")> Optional component As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for the displayed confidence intervals.")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As ZipHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim ciAlpha As Double = h.ConfidenceAlpha
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, ciAlpha) Then Return ExcelError.ExcelErrorNum
                End If

                Dim componentKey As String = ParseZipComponent(component)
                If String.IsNullOrWhiteSpace(componentKey) Then Return ExcelError.ExcelErrorValue

                Dim rows As New List(Of Object())
                If componentKey = "all" OrElse componentKey = "count" Then
                    AppendZipSummaryRows(rows,
                                         componentLabel:="Count",
                                         predictorNames:=BuildParameterNames(h.CountVarNames, h.CountInterceptIncluded, h.Model.resultsPoisson.Coeffs_est.Length),
                                         coeffs:=h.Model.resultsPoisson.Coeffs_est,
                                         ses:=h.Model.resultsPoisson.Coeffs_SEs,
                                         interceptIncluded:=h.CountInterceptIncluded,
                                         ciAlpha:=ciAlpha)
                End If
                If componentKey = "all" OrElse componentKey = "zero" Then
                    AppendZipSummaryRows(rows,
                                         componentLabel:="Zero",
                                         predictorNames:=BuildParameterNames(h.ZeroVarNames, h.ZeroInterceptIncluded, h.Model.resultsLogistic.Coeffs_est.Length),
                                         coeffs:=h.Model.resultsLogistic.Coeffs_est,
                                         ses:=h.Model.resultsLogistic.Coeffs_SEs,
                                         interceptIncluded:=h.ZeroInterceptIncluded,
                                         ciAlpha:=ciAlpha)
                End If

                Return MaterializeRows(rows,
                                       UDFhelpers.GetOptionalBool(includeHeader, True),
                                       New String() {"Component", "Parameter", "Type", "Coef", "SE", "Z", "P-value", "CI Lower", "CI Upper"})

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.ZIP_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns model-level diagnostics and fit statistics for a fitted ZIP model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ZIP_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A rectangular table containing model type, component link functions, likelihood-based diagnostics,
        ''' sample-size metadata, EM convergence information, information criteria, and selected fitting warnings.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The reported information criteria are those of the fitted ZIP model, not of either submodel taken separately.
        ''' They are based on the observed-data log-likelihood
        ''' <c>ℓ(β,γ) = Σ_i log P(Y_i = y_i | x_i, z_i)</c>.
        ''' </para>
        ''' <para>
        ''' The residual deviance reported by the underlying engine is <c>-2 ℓ(β,γ)</c>.
        ''' The AIC, AICc, and BIC values summarize overall model tradeoffs using the full ZIP parameter count
        ''' from both the Poisson and logistic components.
        ''' </para>
        ''' <para>
        ''' The convergence rows describe the EM outer loop. The relative log-likelihood-change row is the final absolute
        ''' change used by the implementation&apos;s stopping rule.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.ZIP_TESTS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns model-level diagnostics and fit statistics for a fitted Zero-Inflated Poisson model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ZIP_TESTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ZIP_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As ZipHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim rows As New List(Of Object())
                rows.Add(New Object() {"Model", "Zero-Inflated Poisson", "", ""})
                rows.Add(New Object() {"Poisson count link", "Log", "", ""})
                rows.Add(New Object() {"Logistic zero link", "Logit", "", ""})
                rows.Add(New Object() {"Count parameters", h.Model.resultsPoisson.Coeffs_est.Length, "", ""})
                rows.Add(New Object() {"Zero parameters", h.Model.resultsLogistic.Coeffs_est.Length, "", ""})
                rows.Add(New Object() {"Log Likelihood", SafeExcelNumber(h.Model.LogLikelihood), "", ""})
                rows.Add(New Object() {"Residual deviance", SafeExcelNumber(h.Model.ResidualDeviance), "", ""})
                rows.Add(New Object() {"# observations", h.Response.Length, "", ""})
                rows.Add(New Object() {"Observations with Y = 0", CountZeros(h.Response), "", ""})
                rows.Add(New Object() {"AIC", SafeExcelNumber(h.Model.AIC), "", ""})
                rows.Add(New Object() {"AICc", SafeExcelNumber(h.Model.AICc), "", ""})
                rows.Add(New Object() {"BIC", SafeExcelNumber(h.Model.BIC), "", ""})
                rows.Add(New Object() {"Number of EM iterations", h.Model.EMiterations, "", ""})
                rows.Add(New Object() {"Relative Log-Likelihood Change", SafeExcelNumber(h.Model.LastRelativeLogLikelihoodChange), "", ""})
                rows.Add(New Object() {"Converged?", h.Model.Converged, "", ""})
                rows.Add(New Object() {"Computational time (s)", SafeExcelNumber(h.Model.ComputationalTimeSeconds), "", ""})

                Dim zeroModel As GLM = h.Model.FinalZeroComponentModel
                If zeroModel IsNot Nothing Then
                    If zeroModel.bSeparation Then
                        rows.Add(New Object() {"Warnings", "Complete separation in the logistic zero component. Maximum-likelihood estimates may not exist.", "", ""})
                    ElseIf zeroModel.bQuasiSeparation Then
                        rows.Add(New Object() {"Warnings", "Quasi-separation detected in the logistic zero component. Inference may be unstable.", "", ""})
                    End If
                End If

                Return MaterializeRows(rows,
                                       UDFhelpers.GetOptionalBool(includeHeader, True),
                                       New String() {"Item", "Value", "df", "P-value"})

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.ZIP_TESTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns residual diagnostics for a fitted ZIP model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ZIP_FIT</c>.</param>
        ''' <param name="residType">
        ''' Residual block to return: <c>all</c> (default), <c>raw</c>, or <c>pearson</c>.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' Either a two-column table of raw and Pearson residuals, or a single residual vector for the selected type.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The raw residual is
        ''' <c>r_i = y_i - μ_i</c>
        ''' where <c>μ_i = (1 - π_i) λ_i</c> is the fitted ZIP mean.
        ''' </para>
        ''' <para>
        ''' The Pearson residual uses the ZIP variance
        ''' <c>Var(Y_i) = (1 - π_i) λ_i (1 + π_i λ_i)</c>
        ''' and is reported as
        ''' <c>r_i^P = (y_i - μ_i) / sqrt(Var(Y_i))</c>.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.ZIP_RESID",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns residual diagnostics for a fitted Zero-Inflated Poisson model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ZIP_RESID(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ZIP_FIT.")> handle As Object,
            <ExcelArgument(Name:="residType", Description:="Residual selector: ""all"" (default), ""raw"", or ""pearson"".")> Optional residType As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As ZipHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim raw() As Double = h.Model.RawResiduals
                Dim pearson() As Double = h.Model.PearsonResiduals
                If raw Is Nothing OrElse pearson Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim key As String = ParseZipResidualType(residType)
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)

                Select Case key
                    Case "raw"
                        Return BuildResidualVectorOutput(raw, "RawResidual", hdr)
                    Case "pearson"
                        Return BuildResidualVectorOutput(pearson, "PearsonResidual", hdr)
                    Case Else
                        Dim rows As New List(Of Object())
                        For i As Integer = 0 To raw.Length - 1
                            rows.Add(New Object() {raw(i), pearson(i)})
                        Next
                        Return MaterializeRows(rows, hdr, New String() {"RawResidual", "PearsonResidual"})
                End Select

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.ZIP_RESID", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns predicted ZIP means and component-level predictions for new data.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ZIP_FIT</c>.</param>
        ''' <param name="newCountX">
        ''' New raw predictor matrix for the Poisson count component in the same raw-column order used at fitting time.
        ''' </param>
        ''' <param name="newZeroX">
        ''' Optional new raw predictor matrix for the logistic zero component in the same raw-column order used at fitting time.
        ''' When omitted, the function reuses <paramref name="newCountX"/>.
        ''' </param>
        ''' <param name="newOffset">
        ''' Optional new offset vector for the Poisson count component.
        ''' It is required when the fitted model used an offset.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A rectangular table containing the ZIP mean prediction, the Poisson count mean, the structural-zero probability,
        ''' and the two component linear predictors.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' For each new observation, the function reconstructs the expanded design matrices from the stored formula specifications,
        ''' then computes
        ''' <c>η_c = β_0 + x'β + o</c>,
        ''' <c>λ = exp(η_c)</c>,
        ''' <c>η_z = γ_0 + z'γ</c>,
        ''' and
        ''' <c>π = logistic(η_z)</c>.
        ''' The returned ZIP mean prediction is
        ''' <c>μ = (1 - π) λ</c>.
        ''' </para>
        ''' <para>
        ''' This function performs deterministic plug-in prediction from the stored fitted coefficients.
        ''' It does not refit the model and does not compute prediction intervals.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.ZIP_PRED",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns predicted means and component predictions for new data under a fitted Zero-Inflated Poisson model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ZIP_PRED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ZIP_FIT.")> handle As Object,
            <ExcelArgument(AllowReference:=True, Name:="newCountX", Description:="New raw predictor matrix for the Poisson count component.")> Optional newCountX As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="newZeroX", Description:="Optional new raw predictor matrix for the logistic zero component. Defaults to newCountX.")> Optional newZeroX As Object = Nothing,
            <ExcelArgument(Name:="newOffset", Description:="Optional new offset vector for the Poisson count component. Required if the fitted model used an offset.")> Optional newOffset As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As ZipHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim effectiveZeroX As Object = If(Not IsMissingArg(newZeroX), newZeroX, newCountX)

                Dim countExpandedX(,) As Double = Nothing
                Dim zeroExpandedX(,) As Double = Nothing
                Dim offsetVals() As Double = Nothing
                Dim countRows As Integer = 0
                Dim zeroRows As Integer = 0

                If Not TryPrepareZipCountPredictionInputs(h, newCountX, newOffset, countExpandedX, offsetVals, countRows) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If Not TryPrepareZipZeroPredictionInputs(h, effectiveZeroX, zeroExpandedX, zeroRows) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim nRows As Integer = Math.Max(countRows, zeroRows)
                If nRows < 1 Then
                    If offsetVals IsNot Nothing Then
                        nRows = offsetVals.Length
                    Else
                        nRows = 1
                    End If
                End If

                If countRows > 0 AndAlso countRows <> nRows Then Return ExcelError.ExcelErrorValue
                If zeroRows > 0 AndAlso zeroRows <> nRows Then Return ExcelError.ExcelErrorValue
                If offsetVals IsNot Nothing AndAlso offsetVals.Length <> nRows Then Return ExcelError.ExcelErrorValue

                Dim betaCount() As Double = h.Model.resultsPoisson.Coeffs_est
                Dim betaZero() As Double = h.Model.resultsLogistic.Coeffs_est
                If betaCount Is Nothing OrElse betaZero Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim rows As New List(Of Object())
                For i As Integer = 0 To nRows - 1
                    Dim etaCount As Double = ComputeLinearPredictor(countExpandedX, i, betaCount, h.CountInterceptIncluded, offsetVals)
                    Dim lambda As Double = Math.Exp(etaCount)
                    Dim etaZero As Double = ComputeLinearPredictor(zeroExpandedX, i, betaZero, h.ZeroInterceptIncluded, Nothing)
                    Dim pi As Double = regression.Logit.LogisticStable(etaZero)
                    Dim mu As Double = (1.0R - pi) * lambda

                    rows.Add(New Object() {
                        SafeExcelNumber(mu),
                        SafeExcelNumber(lambda),
                        SafeExcelNumber(pi),
                        SafeExcelNumber(etaCount),
                        SafeExcelNumber(etaZero)
                    })
                Next

                Return MaterializeRows(rows,
                                       hdr,
                                       New String() {"PredictedResponse", "CountMean", "ZeroInflationProb", "CountLinearPredictor", "ZeroLinearPredictor"})

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.ZIP_PRED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a fitted ZIP model handle from the in-memory cache.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ZIP_FIT</c>.</param>
        ''' <returns>
        ''' TRUE when the handle was found and removed; otherwise FALSE.
        ''' </returns>
        ''' <remarks>
        ''' Handles are session-scoped identifiers for cached fitted models.
        ''' Removing a handle frees the corresponding in-memory model object for the current Excel session
        ''' and invalidates subsequent lookups using that handle.
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.ZIP_DROP",
            Category:="BESHStatNG - Regression Models",
            Description:="Removes a fitted Zero-Inflated Poisson model handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ZIP_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ZIP_FIT.")> handle As Object
        ) As Object
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return ExcelError.ExcelErrorValue
            Dim removed As ZipHandle = Nothing
            Return _zipCache.TryRemove(key, removed)
        End Function

        Private Sub AppendZipSummaryRows(rows As List(Of Object()),
                                         componentLabel As String,
                                         predictorNames() As String,
                                         coeffs() As Double,
                                         ses() As Double,
                                         interceptIncluded As Boolean,
                                         ciAlpha As Double)
            If rows Is Nothing OrElse coeffs Is Nothing OrElse ses Is Nothing Then Exit Sub
            If coeffs.Length <> ses.Length Then Exit Sub

            Dim zCrit As Double = distributions.ZCritTwoSided(ciAlpha)
            For i As Integer = 0 To coeffs.Length - 1
                Dim z As Double = If(ses(i) > 0.0R, coeffs(i) / ses(i), Double.NaN)
                Dim pv As Double = If(ses(i) > 0.0R, 2.0R * distributions.PNorm(-Math.Abs(z)), Double.NaN)
                rows.Add(New Object() {
                    componentLabel,
                    predictorNames(i),
                    If(interceptIncluded AndAlso i = 0, "Intercept", "Slope"),
                    coeffs(i),
                    ses(i),
                    z,
                    pv,
                    coeffs(i) - zCrit * ses(i),
                    coeffs(i) + zCrit * ses(i)
                })
            Next
        End Sub

        Private Function MaterializeRows(rows As List(Of Object()), includeHeader As Boolean, headers() As String) As Object
            Dim bodyCols As Integer = If(headers Is Nothing, If(rows.Count > 0, rows(0).Length, 1), headers.Length)
            Dim outRows As Integer = rows.Count + If(includeHeader, 1, 0)
            If outRows < 1 Then outRows = 1
            Dim out(outRows - 1, bodyCols - 1) As Object
            Dim r0 As Integer = 0

            If includeHeader AndAlso headers IsNot Nothing Then
                For j As Integer = 0 To headers.Length - 1
                    out(0, j) = headers(j)
                Next
                r0 = 1
            End If

            For i As Integer = 0 To rows.Count - 1
                Dim rowVals() As Object = rows(i)
                For j As Integer = 0 To Math.Min(bodyCols, rowVals.Length) - 1
                    out(r0 + i, j) = rowVals(j)
                Next
            Next

            Return out
        End Function

        Private Function TryPrepareZipCountPredictionInputs(h As ZipHandle,
                                                            newCountX As Object,
                                                            newOffset As Object,
                                                            ByRef expandedX(,) As Double,
                                                            ByRef offsetVals() As Double,
                                                            ByRef nRows As Integer) As Boolean
            expandedX = Nothing
            offsetVals = Nothing
            nRows = 0

            Dim rawPredictorKeys As String() = If(h.RawCountPredictorKeys, h.RawCountVarNames)
            If rawPredictorKeys Is Nothing Then rawPredictorKeys = New String() {}

            If rawPredictorKeys.Length < 1 Then
                If h.HasOffset Then
                    If Not TryPrepareOffsetOnlyPredictionInputs(newOffset, offsetVals, nRows) Then Return False
                ElseIf Not IsMissingArg(newOffset) Then
                    If Not TryPrepareOffsetOnlyPredictionInputs(newOffset, offsetVals, nRows) Then Return False
                End If
                Return True
            End If

            Dim imported As glmData = Nothing
            If Not UDFhelpers.TryBuildPredictorDataFromUdfArgs(newCountX, rawPredictorKeys, newOffset, h.HasOffset, imported) Then
                Return False
            End If
            If imported.nCols <> rawPredictorKeys.Length Then Return False

            Dim expandedNames() As String = Nothing
            Dim designErr As String = Nothing
            If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromDesignSpec(rawX:=imported.DataDbl,
                                                                                                fullRawPredictorKeys:=rawPredictorKeys,
                                                                                                designSpec:=h.CountDesignSpec,
                                                                                                expandedX:=expandedX,
                                                                                                expandedPredictorNames:=expandedNames,
                                                                                                errorMessage:=designErr,
                                                                                                omitCategoricalReference:=h.CountOmitCategoricalReference) Then
                Return False
            End If

            If expandedNames Is Nothing Then expandedNames = New String() {}
            If expandedNames.Length <> h.CountExpandedPredictorNames.Length Then Return False

            nRows = imported.nRows
            offsetVals = If(imported.bOffset, imported.OffsetData, Nothing)
            Return UDFhelpers.HasOnlyFinite(offsetVals)
        End Function

        Private Function TryPrepareZipZeroPredictionInputs(h As ZipHandle,
                                                           newZeroX As Object,
                                                           ByRef expandedX(,) As Double,
                                                           ByRef nRows As Integer) As Boolean
            expandedX = Nothing
            nRows = 0

            Dim rawPredictorKeys As String() = If(h.RawZeroPredictorKeys, h.RawZeroVarNames)
            If rawPredictorKeys Is Nothing Then rawPredictorKeys = New String() {}
            If rawPredictorKeys.Length < 1 Then Return True

            Dim imported As glmData = Nothing
            If Not UDFhelpers.TryBuildPredictorDataFromUdfArgs(newZeroX, rawPredictorKeys, Nothing, False, imported) Then
                Return False
            End If
            If imported.nCols <> rawPredictorKeys.Length Then Return False

            Dim expandedNames() As String = Nothing
            Dim designErr As String = Nothing
            If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromDesignSpec(rawX:=imported.DataDbl,
                                                                                                fullRawPredictorKeys:=rawPredictorKeys,
                                                                                                designSpec:=h.ZeroDesignSpec,
                                                                                                expandedX:=expandedX,
                                                                                                expandedPredictorNames:=expandedNames,
                                                                                                errorMessage:=designErr,
                                                                                                omitCategoricalReference:=h.ZeroOmitCategoricalReference) Then
                Return False
            End If

            If expandedNames Is Nothing Then expandedNames = New String() {}
            If expandedNames.Length <> h.ZeroExpandedPredictorNames.Length Then Return False

            nRows = imported.nRows
            Return True
        End Function

        Private Function TryPrepareOffsetOnlyPredictionInputs(newOffset As Object, ByRef offsetVals() As Double,
                                                              ByRef nRows As Integer) As Boolean
            offsetVals = Nothing
            nRows = 0
            If Not Not IsMissingArg(newOffset) Then Return False

            Dim values As List(Of Double) = Nothing
            If Not UDFhelpers.TryReadNumericColumn(newOffset, values) Then Return False
            If values Is Nothing OrElse values.Count < 1 Then Return False

            offsetVals = values.ToArray()
            If Not UDFhelpers.HasOnlyFinite(offsetVals) Then Return False
            nRows = offsetVals.Length
            Return True
        End Function

        Private Function TryBuildZipDataFromUdfArgs(y As Object,
                                                    xCount As Object,
                                                    xZero As Object,
                                                    countVarNames As Object,
                                                    zeroVarNames As Object,
                                                    offset As Object,
                                                    ByRef countData As glmData,
                                                    ByRef zeroData As glmData) As Boolean
            countData = Nothing
            zeroData = Nothing

            Dim yCol(,) As Object = Nothing
            Dim xCountMat(,) As Object = Nothing
            Dim xZeroMat(,) As Object = Nothing
            Dim offsetCol(,) As Object = Nothing

            Dim yName As String = Nothing
            Dim offsetName As String = Nothing
            Dim inferredCountNames() As String = Nothing
            Dim inferredZeroNames() As String = Nothing

            If Not UDFhelpers.TryGetTrimmedColumnObject(y, yCol, yName, "numeric") Then Return False
            If Not UDFhelpers.TryGetTrimmedNumericMatrixObject(xCount, xCountMat, inferredCountNames) Then Return False
            If Not UDFhelpers.TryGetTrimmedNumericMatrixObject(xZero, xZeroMat, inferredZeroNames) Then Return False

            Dim rowCount As Integer = yCol.GetLength(0)
            If xCountMat.GetLength(0) <> rowCount Then Return False
            If xZeroMat.GetLength(0) <> rowCount Then Return False

            Dim hasOffset As Boolean = Not IsMissingArg(offset)
            If hasOffset Then
                If Not UDFhelpers.TryGetTrimmedColumnObject(offset, offsetCol, offsetName, "numeric") Then Return False
                If offsetCol.GetLength(0) <> rowCount Then Return False
            End If

            Dim countPredictorNames As String() = UDFhelpers.ResolveImportedPredictorNames(countVarNames, inferredCountNames)
            Dim zeroPredictorNames As String() = UDFhelpers.ResolveImportedPredictorNames(zeroVarNames, inferredZeroNames)

            Dim countCols As Integer = xCountMat.GetLength(1)
            Dim zeroCols As Integer = xZeroMat.GetLength(1)

            Dim rawCount(rowCount - 1, countCols + If(hasOffset, 1, 0)) As Object
            Dim countNames(countCols + If(hasOffset, 1, 0)) As String
            countNames(0) = If(String.IsNullOrWhiteSpace(yName), "Y", yName)

            For i As Integer = 0 To rowCount - 1
                rawCount(i, 0) = yCol(i, 0)
            Next
            For j As Integer = 0 To countCols - 1
                countNames(j + 1) = countPredictorNames(j)
                For i As Integer = 0 To rowCount - 1
                    rawCount(i, j + 1) = xCountMat(i, j)
                Next
            Next
            If hasOffset Then
                countNames(countCols + 1) = If(String.IsNullOrWhiteSpace(offsetName), "Offset", offsetName)
                For i As Integer = 0 To rowCount - 1
                    rawCount(i, countCols + 1) = offsetCol(i, 0)
                Next
            End If

            Dim rawZero(rowCount - 1, zeroCols) As Object
            Dim zeroNames(zeroCols) As String
            zeroNames(0) = If(String.IsNullOrWhiteSpace(yName), "Y", yName)

            For i As Integer = 0 To rowCount - 1
                rawZero(i, 0) = yCol(i, 0)
            Next
            For j As Integer = 0 To zeroCols - 1
                zeroNames(j + 1) = zeroPredictorNames(j)
                For i As Integer = 0 To rowCount - 1
                    rawZero(i, j + 1) = xZeroMat(i, j)
                Next
            Next

            Dim countOut As New glmData With {.bOffset = hasOffset, .bWeights = False}
            countOut.DataImportRawMatrix(rawCount, countNames)
            If countOut.bZeroValid OrElse countOut.nRows < 1 Then Return False

            Dim zeroOut As New glmData With {.bOffset = False, .bWeights = False}
            zeroOut.DataImportRawMatrix(rawZero, zeroNames)
            If zeroOut.bZeroValid OrElse zeroOut.nRows < 1 Then Return False

            Dim keepCount As Dictionary(Of Integer, Integer) = CommonItems(countOut.RowIds, zeroOut.RowIds)
            Dim keepZero As Dictionary(Of Integer, Integer) = CommonItems(zeroOut.RowIds, countOut.RowIds)
            If keepCount Is Nothing OrElse keepZero Is Nothing Then Return False
            If keepCount.Count < 1 OrElse keepZero.Count < 1 Then Return False
            If keepCount.Count <> keepZero.Count Then Return False

            countOut.SubsetByRowIdValues(keepCount)
            zeroOut.SubsetByRowIdValues(keepZero)
            If countOut.nRows <> zeroOut.nRows Then Return False
            If countOut.nRows < 1 Then Return False

            If Not ResponseColumnsMatchObjects(countOut.FinalData, zeroOut.FinalData) Then Return False
            Dim response() As Integer = Nothing
            If Not TryExtractNonnegativeIntegerResponseObjects(countOut.FinalData, response) Then Return False
            If hasOffset AndAlso Not UDFhelpers.HasOnlyFinite(countOut.OffsetData) Then Return False

            countData = countOut
            zeroData = zeroOut
            Return True
        End Function

        Private Function ResponseColumnsMatchObjects(countData(,) As Object, zeroData(,) As Object) As Boolean
            If countData Is Nothing OrElse zeroData Is Nothing Then Return False
            If countData.GetLength(0) <> zeroData.GetLength(0) Then Return False

            For i As Integer = 0 To countData.GetLength(0) - 1
                Dim yc As Double? = UDFhelpers.TryGetDouble(countData(i, 0))
                Dim yz As Double? = UDFhelpers.TryGetDouble(zeroData(i, 0))
                If Not yc.HasValue OrElse Not yz.HasValue Then Return False
                If Math.Abs(yc.Value - yz.Value) > 0.0000001R Then Return False
            Next

            Return True
        End Function

        Private Function TryExtractNonnegativeIntegerResponseObjects(data(,) As Object, ByRef response() As Integer) As Boolean
            response = Nothing
            If data Is Nothing Then Return False
            Dim n As Integer = data.GetLength(0)
            If n < 1 Then Return False

            ReDim response(n - 1)
            For i As Integer = 0 To n - 1
                Dim yi As Double? = UDFhelpers.TryGetDouble(data(i, 0))
                If Not yi.HasValue Then Return False
                Dim yr As Double = Math.Round(yi.Value)
                If yi.Value < 0.0R OrElse Math.Abs(yi.Value - yr) > 0.0000001R Then Return False
                response(i) = CInt(yr)
            Next

            Return True
        End Function

        Private Function ResponseColumnsMatch(countData(,) As Double, zeroData(,) As Double) As Boolean
            If countData Is Nothing OrElse zeroData Is Nothing Then Return False
            If countData.GetLength(0) <> zeroData.GetLength(0) Then Return False

            For i As Integer = 0 To countData.GetLength(0) - 1
                Dim yc As Double = countData(i, 0)
                Dim yz As Double = zeroData(i, 0)
                If Double.IsNaN(yc) OrElse Double.IsInfinity(yc) OrElse Double.IsNaN(yz) OrElse Double.IsInfinity(yz) Then Return False
                If Math.Abs(yc - yz) > 0.0000001R Then Return False
            Next

            Return True
        End Function

        Private Function TryExtractNonnegativeIntegerResponse(data(,) As Double, ByRef response() As Integer) As Boolean
            response = Nothing
            If data Is Nothing Then Return False
            Dim n As Integer = data.GetLength(0)
            If n < 1 Then Return False

            ReDim response(n - 1)
            For i As Integer = 0 To n - 1
                Dim yi As Double = data(i, 0)
                If Double.IsNaN(yi) OrElse Double.IsInfinity(yi) Then Return False
                Dim yr As Double = Math.Round(yi)
                If yi < 0.0R OrElse Math.Abs(yi - yr) > 0.0000001R Then Return False
                response(i) = CInt(yr)
            Next

            Return True
        End Function

        Private Function ParseZipComponent(v As Object) As String
            Dim s As String = UDFhelpers.AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case NormalizeKey(s)
                Case "all"
                    Return "all"
                Case "count", "poisson", "countpart", "poissonpart"
                    Return "count"
                Case "zero", "logistic", "zeropart", "inflation", "zeroinflation"
                    Return "zero"
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function ParseZipResidualType(v As Object) As String
            Dim s As String = UDFhelpers.AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case NormalizeKey(s)
                Case "all"
                    Return "all"
                Case "raw", "response"
                    Return "raw"
                Case "pearson"
                    Return "pearson"
                Case Else
                    Return "all"
            End Select
        End Function

        Private Function TryGetHandle(handle As Object, ByRef h As ZipHandle) As Boolean
            h = Nothing
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _zipCache.TryGetValue(key, h)
        End Function

        Private Function BuildParameterNames(varNames() As String, interceptIncluded As Boolean, coefficientCount As Integer) As String()
            Dim out(coefficientCount - 1) As String
            Dim nextIndex As Integer = 0

            If interceptIncluded AndAlso coefficientCount > 0 Then
                out(0) = "Intercept"
                nextIndex = 1
            End If

            Dim predictorNames() As String = If(varNames, New String() {})
            For i As Integer = 0 To predictorNames.Length - 1
                If nextIndex + i <= UBound(out) Then
                    out(nextIndex + i) = predictorNames(i)
                End If
            Next

            For i As Integer = 0 To UBound(out)
                If String.IsNullOrWhiteSpace(out(i)) Then
                    out(i) = "Param" & (i + 1).ToString(CultureInfo.InvariantCulture)
                End If
            Next

            Return out
        End Function

        Private Function CountZeros(values() As Integer) As Integer
            If values Is Nothing Then Return 0
            Dim n As Integer = 0
            For Each v As Integer In values
                If v = 0 Then n += 1
            Next
            Return n
        End Function

    End Module

End Namespace
