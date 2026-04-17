Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for generalized estimating equations with reusable handle-based workflow from Excel.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These worksheet functions expose marginal regression models for clustered, repeated-measures, and longitudinal data.
    ''' The marginal mean model is
    ''' <c>g(μ_ij) = η_ij = β_0 + x_ij'β + o_ij</c>,
    ''' where <c>μ_ij = E[Y_ij | x_ij]</c>, <c>g(·)</c> is the selected link, and <c>o_ij</c> is an optional offset.
    ''' Unlike subject-specific random-effects models, generalized estimating equations target the average population-level response.
    ''' </para>
    ''' <para>
    ''' Correlation within a cluster is represented through a working covariance matrix of the form
    ''' <c>V_i = φ A_i^{1/2} R_i(α) A_i^{1/2}</c>,
    ''' where <c>A_i</c> contains the marginal variances implied by the chosen family,
    ''' <c>R_i(α)</c> is the working correlation matrix, and <c>φ</c> is a scale parameter.
    ''' Supported working structures include independence, exchangeable correlation, autoregressive correlation, and an unstructured correlation matrix.
    ''' </para>
    ''' <para>
    ''' The fitted coefficients solve the estimating equations
    ''' <c>Σ_i D_i' V_i^{-1} (y_i - μ_i) = 0</c>,
    ''' where <c>D_i = ∂μ_i/∂β</c>.
    ''' Estimation alternates between updating the mean parameters and updating the working association parameters.
    ''' Large-sample inference can be based on a model-based covariance, a robust sandwich covariance,
    ''' or a bias-reduced sandwich correction.
    ''' </para>
    ''' <para>
    ''' As with the GLM worksheet functions, the fit function caches the fitted model in memory and returns a text handle.
    ''' The remaining worksheet functions reuse that handle to return coefficient summaries, diagnostics, residuals,
    ''' predictions, and explicit cleanup without refitting.
    ''' </para>
    ''' <para>
    ''' Predictor formulas reuse the same regression-formula infrastructure as the generalized linear-model UDFs.
    ''' The estimator always includes an intercept in the mean model, so categorical expansions omit a reference level accordingly.
    ''' </para>
    ''' </remarks>
    Public Module GEEUDFs

        Private ReadOnly _geeCache As New ConcurrentDictionary(Of String, GeeHandle)(StringComparer.OrdinalIgnoreCase)

        Private Class GeeHandle
            Public Property Handle As String
            Public Property Model As GEE
            Public Property LinkObject As regression.Link
            Public Property VarNames As String()
            Public Property ExpandedPredictorNames As String()
            Public Property RawVarNames As String()
            Public Property RawPredictorKeys As String()
            Public Property RawPredictorAbsoluteLetters As String()
            Public Property DesignSpec As RegressionFormulaDesignSpec
            Public Property OmitCategoricalReference As Boolean
            Public Property HasOffset As Boolean
            Public Property HasWeights As Boolean
            Public Property HasTime As Boolean
            Public Property ClusterVarName As String
            Public Property FamilyName As String
            Public Property LinkName As String
            Public Property CovarianceName As String
            Public Property StandardErrorType As String
            Public Property Alpha As Double
        End Class

        ''' <summary>
        ''' Fits a generalized estimating equation model and returns a reusable handle.
        ''' </summary>
        ''' <param name="y">
        ''' Numeric response vector (single column) with one observation per row.
        ''' Typical uses include repeated binary outcomes, repeated counts, and continuous outcomes observed within clusters.
        ''' </param>
        ''' <param name="x">
        ''' Raw predictor matrix with one row per observation.
        ''' Rows must align with <paramref name="y"/>, <paramref name="clusterId"/>, and the optional time, offset, and weight inputs.
        ''' </param>
        ''' <param name="clusterId">
        ''' Cluster or subject identifier (single column).
        ''' Observations with the same identifier are treated as belonging to the same marginal-response cluster.
        ''' The identifier may be numeric or text.
        ''' </param>
        ''' <param name="time">
        ''' Optional within-cluster ordering variable (single column).
        ''' When supplied, observations are ordered within each cluster by this variable before fitting.
        ''' When omitted, the current row order within each cluster is used.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional raw predictor names supplied as a comma-separated list or as a one-row/one-column range.
        ''' These names are used by the formula parser and by the returned coefficient table.
        ''' </param>
        ''' <param name="family">
        ''' Response family for the marginal variance structure.
        ''' Accepted values include <c>binomial</c> (default), <c>poisson</c>, <c>negative binomial</c>/<c>nb</c>, <c>gaussian</c>, and <c>gamma</c>.
        ''' Representative variance functions are
        ''' <c>μ(1-μ)</c> for Binomial,
        ''' <c>μ</c> for Poisson,
        ''' <c>μ + α μ²</c> for Negative Binomial,
        ''' constant variance for Gaussian, and
        ''' <c>φ μ²</c> for Gamma-type modeling.
        ''' </param>
        ''' <param name="link">
        ''' Optional link function <c>g(·)</c> in <c>g(μ_ij)=η_ij</c>.
        ''' If omitted, the family's canonical or default link is used.
        ''' Accepted values include <c>logit</c>, <c>probit</c>, <c>log</c>, <c>identity</c>, <c>sqrt</c>, <c>inverse</c>, and <c>power</c> when compatible with the chosen family.
        ''' </param>
        ''' <param name="covariance">
        ''' Working-correlation structure.
        ''' Accepted values include <c>independence</c> (default), <c>exchangeable</c>, <c>autoregressive</c>/<c>ar1</c>, and <c>unstructured</c>.
        ''' The working structure affects efficiency and covariance estimation but not the interpretation of the mean model itself.
        ''' </param>
        ''' <param name="stdErrType">
        ''' Covariance estimator used for coefficient standard errors.
        ''' Accepted values are <c>robust</c> (default), <c>naive</c>, and <c>bias reduced</c>.
        ''' The robust option returns the sandwich covariance
        ''' <c>B^{-1} C B^{-1}</c>,
        ''' while the naive option returns the model-based covariance
        ''' <c>φ B^{-1}</c>.
        ''' </param>
        ''' <param name="offset">
        ''' Optional numeric offset vector (single column).
        ''' The offset enters additively on the link scale:
        ''' <c>η_ij = β_0 + x_ij'β + o_ij</c>.
        ''' Under a log link this is commonly used for log-exposure or log-person-time adjustment.
        ''' </param>
        ''' <param name="weights">
        ''' Optional nonnegative case weights (single column).
        ''' These weights enter the mean-estimating equations and residual calculations in the same row order as the response.
        ''' </param>
        ''' <param name="formula">
        ''' Optional right-hand-side formula used to expand the raw predictor matrix before fitting.
        ''' If omitted or blank, all raw predictor columns are included as continuous main effects.
        ''' Formula expansion can create transformed terms, interactions, and categorical indicators while preserving a consistent design for prediction.
        ''' </param>
        ''' <param name="formulaAddressing">
        ''' Formula-addressing mode: <c>relative</c> (default), <c>absolute</c>, or <c>names</c>.
        ''' This controls whether formula tokens refer to columns by relative worksheet letters, absolute worksheet letters, or supplied variable names.
        ''' </param>
        ''' <param name="dispersion">
        ''' Optional fixed NB2 dispersion parameter used only when <paramref name="family"/> is Negative Binomial.
        ''' In that parameterization the marginal variance is <c>μ + α μ²</c>, so this argument supplies the value of <c>α</c>.
        ''' </param>
        ''' <param name="power">
        ''' Optional power parameter used only when <paramref name="link"/> is <c>power</c>.
        ''' </param>
        ''' <param name="maxIter">
        ''' Maximum number of mean/correlation updating iterations (default 20).
        ''' </param>
        ''' <param name="tol">
        ''' Positive convergence tolerance for successive parameter updates (default 1E-8).
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level used for confidence intervals stored with the fitted result (default 0.05).
        ''' This affects inferential reporting only and does not change the fitted coefficients.
        ''' </param>
        ''' <param name="useP">
        ''' Optional logical flag controlling the denominator adjustments used in scale and association-parameter updates.
        ''' When TRUE, the fitting routine applies parameter-count adjustments analogous to small-sample corrections used in some GEE software.
        ''' </param>
        ''' <param name="startParams">
        ''' Optional starting values for the mean-model coefficients, supplied as a one-row/one-column range or a comma/space-separated text list.
        ''' The intercept starting value must be first, followed by the predictor coefficients in the expanded design-matrix order.
        ''' </param>
        ''' <returns>
        ''' A text handle identifying the fitted model within the current Excel session.
        ''' The handle can be passed to the associated summary, diagnostics, residual, prediction, and cleanup worksheet functions without refitting.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function fits the population-averaged model defined by
        ''' <c>g(μ_ij)=β_0+x_ij'β+o_ij</c>
        ''' together with the estimating equations
        ''' <c>Σ_i D_i'V_i^{-1}(y_i-μ_i)=0</c>.
        ''' The family determines <c>A_i</c>, the working-correlation structure determines <c>R_i(α)</c>, and the selected covariance type determines how standard errors are reported.
        ''' </para>
        ''' <para>
        ''' Estimation alternates between updating the mean coefficients using a Fisher-scoring-style linear solve
        ''' and updating the working association parameters from the current standardized residual pattern.
        ''' Convergence is judged from the largest absolute or relative coefficient change across iterations.
        ''' </para>
        ''' <para>
        ''' The returned coefficients are marginal, not cluster-specific.
        ''' For example, under a Binomial-logit GEE, exponentiating a slope yields a marginal odds ratio;
        ''' under a log link, exponentiating a slope yields a multiplicative effect on the marginal mean.
        ''' </para>
        ''' <para>
        ''' Rows containing invalid or non-finite values in the response, predictors, time variable, offset, or weights are removed before fitting.
        ''' Clusters are then sorted internally, and observations within each cluster are ordered by the supplied time variable when one is provided.
        ''' </para>
        ''' <para>
        ''' If <c>formulaAddressing="absolute"</c> is used, the predictor argument should be a direct worksheet range so absolute worksheet column letters can be resolved.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.GEE_FIT(A2:A101,B2:D101,E2:E101)
        ''' =BESH.REGR.GEE_FIT(A2:A101,B2:E101,F2:F101,G2:G101,"Age,BMI,Treat,Visit","binomial","logit","exchangeable","robust")
        ''' =BESH.REGR.GEE_FIT(A2:A101,B2:D101,E2:E101,,"Dose,Age,Stage","poisson","log","ar1","robust",H2:H101,,"A + B + factor(C)")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.GEE_FIT",
            Category:="BESHStatNG - Regression Models",
            Description:="Fits a generalized estimating equation model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GEE_FIT(
            <ExcelArgument(Name:="y", Description:="Numeric response vector (single column).")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Raw predictor matrix with one row per observation.")> x As Object,
            <ExcelArgument(Name:="clusterId", Description:="Cluster or subject identifier (single column).")> clusterId As Object,
            <ExcelArgument(Name:="time", Description:="Optional within-cluster ordering variable (single column).")> Optional time As Object = Nothing,
            <ExcelArgument(Name:="varNames", Description:="Optional raw predictor names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="family", Description:="Marginal family: ""binomial"" (default), ""poisson"", ""negative binomial"" / ""nb"", ""gaussian"", or ""gamma"".")> Optional family As Object = Nothing,
            <ExcelArgument(Name:="link", Description:="Optional link function. Defaults to the family's canonical/default link.")> Optional link As Object = Nothing,
            <ExcelArgument(Name:="covariance", Description:="Working-correlation structure: ""independence"" (default), ""exchangeable"", ""autoregressive"" / ""ar1"", or ""unstructured"".")> Optional covariance As Object = Nothing,
            <ExcelArgument(Name:="stdErrType", Description:="Coefficient covariance type: ""robust"" (default), ""naive"", or ""bias reduced"".")> Optional stdErrType As Object = Nothing,
            <ExcelArgument(Name:="offset", Description:="Optional numeric offset vector (single column).")> Optional offset As Object = Nothing,
            <ExcelArgument(Name:="weights", Description:="Optional nonnegative case weights (single column).")> Optional weights As Object = Nothing,
            <ExcelArgument(Name:="formula", Description:="Optional RHS formula used to expand the raw predictor matrix.")> Optional formula As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Formula-addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing,
            <ExcelArgument(Name:="dispersion", Description:="Optional fixed NB2 dispersion parameter used only for the negative-binomial family.")> Optional dispersion As Object = Nothing,
            <ExcelArgument(Name:="power", Description:="Optional power parameter used only when link=""power"".")> Optional power As Object = Nothing,
            <ExcelArgument(Name:="maxIter", Description:="Maximum mean/correlation updating iterations (default 20).")> Optional maxIter As Object = Nothing,
            <ExcelArgument(Name:="tol", Description:="Convergence tolerance (default 1E-8).")> Optional tol As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha used internally for confidence intervals (default 0.05).")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="useP", Description:="TRUE to use parameter-count adjustments in the scale and association updates.")> Optional useP As Object = Nothing,
            <ExcelArgument(Name:="startParams", Description:="Optional starting values for the mean-model coefficients.")> Optional startParams As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "GEE_FIT (editing...)"

            Try
                Dim imported As geeData = Nothing
                If Not UDFhelpers.TryBuildGeeDataFromUdfArgs(y, x, clusterId, time, varNames, offset, weights, imported) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim formulaText As String = UDFhelpers.AsString(formula)
                If String.IsNullOrWhiteSpace(formulaText) Then formulaText = Nothing

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

                Dim absoluteColumnLetters As String() = Nothing
                If allowAbsoluteColumnLetters AndAlso Not String.IsNullOrWhiteSpace(formulaText) Then
                    If Not UDFhelpers.TryGetAbsoluteColumnLettersFromRange(x, imported.nCols - 1, absoluteColumnLetters) Then
                        Return ExcelError.ExcelErrorValue
                    End If
                End If

                Dim designBuild As RegressionFormulaRegressionDataBuildResult = Nothing
                Dim designErr As String = Nothing
                If Not RegressionFormulaDesignService.TryBuildExpandedRegressionDataMatrixFromFormula(raw:=imported,
                                                                                                     yKey:=imported.varNames(0),
                                                                                                     result:=designBuild,
                                                                                                     errorMessage:=designErr,
                                                                                                     formulaText:=formulaText,
                                                                                                     absoluteColumnLetters:=absoluteColumnLetters,
                                                                                                     allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                                                                     allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                                                                     allowQuotedVariableNames:=allowQuotedVariableNames,
                                                                                                     omitCategoricalReference:=True) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim fitData As Double(,) = designBuild.RegressionDataMatrix
                Dim fitVarNames As String() = designBuild.RegressionDataVarNames
                Dim fitPredictorNames As String() = If(designBuild.ExpandedPredictorNames, New String() {})
                Dim fitOffset() As Double = If(imported.bOffset, imported.OffsetData, Nothing)
                Dim fitWeights() As Double = If(imported.bWeights, imported.WeightData, Nothing)
                Dim fitTime() As Double = If(imported.bTime, imported.TimeData, Nothing)
                Dim fitCluster() As Object = imported.ClusterIdData
                Dim rowIds() As Integer = imported.RowIds

                If fitData Is Nothing OrElse fitVarNames Is Nothing OrElse fitVarNames.Length < 1 Then Return ExcelError.ExcelErrorValue
                If fitCluster Is Nothing OrElse fitCluster.Length <> fitData.GetLength(0) Then Return ExcelError.ExcelErrorValue
                If Not UDFhelpers.HasOnlyFinite(fitOffset) Then Return ExcelError.ExcelErrorValue
                If Not UDFhelpers.HasOnlyFinite(fitWeights, True) Then Return ExcelError.ExcelErrorValue
                If Not UDFhelpers.HasOnlyFinite(fitTime) Then Return ExcelError.ExcelErrorValue

                Dim alphaValue As Double = 0.05R
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim maxIterValue As Integer = UDFhelpers.GetOptionalInt(maxIter, 20)
                Dim tolValue As Double = UDFhelpers.GetOptionalDouble(tol, 0.00000001R)
                Dim usePValue As Boolean = UDFhelpers.GetOptionalBool(useP, False)
                If maxIterValue < 1 Then Return ExcelError.ExcelErrorNum
                If Double.IsNaN(tolValue) OrElse Double.IsInfinity(tolValue) OrElse tolValue <= 0.0R Then Return ExcelError.ExcelErrorNum

                Dim familyCode As String = ParseFamilyCode(If(Not IsMissingArg(family), family, "binomial"))
                If String.IsNullOrWhiteSpace(familyCode) Then Return ExcelError.ExcelErrorValue

                Dim dispersionValue As Double = UDFhelpers.GetOptionalDouble(dispersion, 1.0R)
                If familyCode = "NegativeBinomial" Then
                    If Double.IsNaN(dispersionValue) OrElse Double.IsInfinity(dispersionValue) OrElse dispersionValue <= 0.0R Then Return ExcelError.ExcelErrorNum
                End If

                Dim fam As regression.Family = regression.createFamily(familyCode, dispersionValue)
                If fam Is Nothing Then Return ExcelError.ExcelErrorValue

                Dim linkName As String = ParseLinkName(link, fam.ToString())
                If String.IsNullOrWhiteSpace(linkName) Then Return ExcelError.ExcelErrorValue
                If Not fam.testLink(linkName) Then Return ExcelError.ExcelErrorValue

                Dim lnk As regression.Link = Nothing
                If String.Equals(linkName, "Power", StringComparison.OrdinalIgnoreCase) Then
                    If Not Not IsMissingArg(power) Then Return ExcelError.ExcelErrorNum
                    Dim powerValue As Double = UDFhelpers.GetOptionalDouble(power, Double.NaN)
                    If Double.IsNaN(powerValue) OrElse Double.IsInfinity(powerValue) OrElse powerValue = 0.0R Then Return ExcelError.ExcelErrorNum
                    lnk = regression.createLink("Power", powerValue)
                Else
                    lnk = regression.createLink(linkName)
                End If
                If lnk Is Nothing Then Return ExcelError.ExcelErrorValue

                Dim covName As String = ParseGeeCovarianceName(covariance)
                If String.IsNullOrWhiteSpace(covName) Then Return ExcelError.ExcelErrorValue
                Dim covStr As regression.GEEcovStruct = regression.createGEEcovMat(covName)
                If covStr Is Nothing Then Return ExcelError.ExcelErrorValue

                Dim seType As String = ParseGeeStandardErrorType(stdErrType)
                If String.IsNullOrWhiteSpace(seType) Then Return ExcelError.ExcelErrorValue

                Dim mdl As New GEE(fam, lnk, covStr, seType)
                mdl.bComputeResiduals = False
                mdl.bIterationDetails = False
                mdl.settingInputs(alphaValue, maxIterValue, tolValue, usePValue)
                mdl.data(fitData,
                         fitCluster,
                         rowIds,
                         fitOffset,
                         fitWeights,
                         fitTime)
                mdl.setVarNames(fitVarNames,
                                imported.ClusterIdVarName,
                                If(imported.bOffset, imported.OffsetVarName, Nothing),
                                If(imported.bWeights, imported.WeightVarName, Nothing),
                                If(imported.bTime, imported.TimeVarName, Nothing))

                Dim parsedStartParams() As Double = Nothing
                If Not IsMissingArg(startParams) Then
                    If Not TryParseNumericVector(startParams, parsedStartParams) Then Return ExcelError.ExcelErrorValue
                    If parsedStartParams Is Nothing OrElse parsedStartParams.Length <> fitVarNames.Length Then Return ExcelError.ExcelErrorNum
                    mdl.startParams = parsedStartParams
                End If

                mdl.Fit(parsedStartParams IsNot Nothing)

                If mdl.results Is Nothing OrElse mdl.results.Coeffs_est Is Nothing OrElse mdl.results.Coeffs_SEs Is Nothing Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim handleKey As String = "GEE:" & Guid.NewGuid().ToString("N")
                Dim h As New GeeHandle With {
                    .Handle = handleKey,
                    .Model = mdl,
                    .LinkObject = lnk,
                    .VarNames = CloneStringArray(If(mdl.results.varNames, New String() {})),
                    .ExpandedPredictorNames = CloneStringArray(fitPredictorNames),
                    .RawVarNames = CloneStringArray(If(designBuild.FullRawPredictorNames, New String() {})),
                    .RawPredictorKeys = CloneStringArray(If(designBuild.FullRawPredictorKeys, New String() {})),
                    .RawPredictorAbsoluteLetters = CloneStringArray(If(designBuild.FullRawPredictorAbsoluteLetters, New String() {})),
                    .DesignSpec = designBuild.DesignSpec,
                    .OmitCategoricalReference = True,
                    .HasOffset = (fitOffset IsNot Nothing),
                    .HasWeights = (fitWeights IsNot Nothing),
                    .HasTime = imported.bTime,
                    .ClusterVarName = imported.ClusterIdVarName,
                    .FamilyName = fam.ToString(),
                    .LinkName = lnk.ToString(),
                    .CovarianceName = covStr.ToString(),
                    .StandardErrorType = seType,
                    .Alpha = alphaValue
                }

                _geeCache(handleKey) = h
                Return handleKey

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GEE_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the coefficient summary table for a fitted generalized estimating equation handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GEE_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <param name="alpha">Optional two-sided alpha for the displayed confidence intervals.</param>
        ''' <returns>
        ''' A rectangular coefficient table with one row per estimated mean-model parameter.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The coefficient table is reported on the link scale.
        ''' For each parameter the function returns the estimate <c>β̂</c>, its selected standard error,
        ''' the Wald statistic <c>Z = β̂ / SE(β̂)</c>, the associated large-sample two-sided p-value,
        ''' and a two-sided confidence interval of the form
        ''' <c>β̂ ± z_{1-α/2} SE(β̂)</c>.
        ''' </para>
        ''' <para>
        ''' The standard-error column reflects the covariance estimator chosen at fit time:
        ''' model-based, robust sandwich, or bias-reduced sandwich.
        ''' This affects inference but not the coefficient estimates themselves.
        ''' </para>
        ''' <para>
        ''' No exponentiation is applied automatically.
        ''' When a marginal odds ratio or marginal rate ratio is desired, users can exponentiate the returned coefficients and confidence limits externally.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GEE_SUMMARY",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the coefficient summary table for a fitted generalized estimating equation handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GEE_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GEE_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for the displayed confidence intervals.")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As GeeHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim alphaValue As Double = h.Alpha
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim beta() As Double = h.Model.results.Coeffs_est
                Dim se() As Double = h.Model.results.Coeffs_SEs
                If beta Is Nothing OrElse se Is Nothing Then Return ExcelError.ExcelErrorNA
                If beta.Length <> se.Length Then Return ExcelError.ExcelErrorNA

                Dim names() As String = BuildParameterNames(h.VarNames, beta.Length)
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, beta.Length + 1, beta.Length)
                Dim out(outRows - 1, 7) As Object
                Dim zCrit As Double = distributions.ZCritTwoSided(alphaValue)
                Dim ciLabel As String = (100.0R * (1.0R - alphaValue)).ToString("0.##", CultureInfo.InvariantCulture) & "%"
                Dim r0 As Integer = 0

                If hdr Then
                    out(0, 0) = "Parameter"
                    out(0, 1) = "Type"
                    out(0, 2) = "Coef"
                    out(0, 3) = "SE"
                    out(0, 4) = "Z"
                    out(0, 5) = "P-value"
                    out(0, 6) = ciLabel & " CI Lower"
                    out(0, 7) = ciLabel & " CI Upper"
                    r0 = 1
                End If

                For i As Integer = 0 To beta.Length - 1
                    Dim z As Double = If(se(i) > 0.0R, beta(i) / se(i), Double.NaN)
                    Dim pv As Double = If(se(i) > 0.0R, 2.0R * distributions.PNorm(-Math.Abs(z)), Double.NaN)

                    out(r0 + i, 0) = names(i)
                    out(r0 + i, 1) = If(i = 0, "Intercept", "Slope")
                    out(r0 + i, 2) = beta(i)
                    out(r0 + i, 3) = se(i)
                    out(r0 + i, 4) = z
                    out(r0 + i, 5) = pv
                    out(r0 + i, 6) = beta(i) - zCrit * se(i)
                    out(r0 + i, 7) = beta(i) + zCrit * se(i)
                Next

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GEE_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns model-level diagnostics and fit statistics for a fitted generalized estimating equation handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GEE_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A table of model-level statistics such as family, link, working correlation structure,
        ''' numbers of observations and clusters, cluster-size summaries, scale, quasi-information criteria,
        ''' iteration counts, convergence indicators, and computational time.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Because GEE is based on estimating equations rather than a full likelihood in the general correlated-data setting,
        ''' model comparison is commonly summarized by quasi-likelihood information criteria rather than standard likelihood-ratio tests.
        ''' The reported QIC and QICu values are based on the fitted quasi-likelihood and the selected covariance structure.
        ''' </para>
        ''' <para>
        ''' The scale row summarizes the estimated overdispersion or residual scale parameter <c>φ</c>.
        ''' The cluster-size rows describe the replication pattern that underlies the sandwich covariance and working-correlation updates.
        ''' </para>
        ''' <para>
        ''' The convergence rows report the last relative coefficient-change criterion and whether the stopping rule was met before the iteration limit.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GEE_TESTS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns model-level diagnostics and fit statistics for a fitted generalized estimating equation handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GEE_TESTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GEE_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GeeHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim labels() As String = h.Model.results.ModelTableLabels
                Dim vals(,) As Object = h.Model.results.ModelTableVals
                If labels Is Nothing OrElse vals Is Nothing Then Return ExcelError.ExcelErrorNA
                If vals.GetLength(0) <> labels.Length Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, labels.Length + 2, labels.Length + 1)
                Dim out(outRows - 1, 3) As Object
                Dim r As Integer = 0

                If hdr Then
                    out(0, 0) = "Item"
                    out(0, 1) = "Value"
                    out(0, 2) = "df"
                    out(0, 3) = "P-value"
                    r = 1
                End If

                For i As Integer = 0 To labels.Length - 1
                    out(r, 0) = labels(i)
                    out(r, 1) = vals(i, 0)
                    out(r, 2) = If(vals.GetLength(1) > 1, vals(i, 1), "")
                    out(r, 3) = If(vals.GetLength(1) > 2, vals(i, 2), "")
                    r += 1
                Next

                out(r, 0) = "Computational time (s)"
                out(r, 1) = h.Model.ComputationalTimeSeconds
                out(r, 2) = ""
                out(r, 3) = ""

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GEE_TESTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the fitted working correlation matrix for a generalized estimating equation handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GEE_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include row and column labels (default TRUE).</param>
        ''' <returns>
        ''' A square matrix representing the fitted working correlation structure used inside the marginal estimating equations.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Generalized estimating equations model the within-cluster covariance through the decomposition
        ''' <c>V_i = φ A_i^{1/2} R_i(α) A_i^{1/2}</c>,
        ''' where <c>R_i(α)</c> is the working correlation matrix.
        ''' This worksheet function returns that fitted correlation matrix <c>R_i(α)</c>.
        ''' It is the association structure used by the algorithm, not the empirical sample correlation matrix of the observed responses.
        ''' </para>
        ''' <para>
        ''' The interpretation depends on the selected working structure:
        ''' independence returns an identity matrix,
        ''' exchangeable returns a matrix with common off-diagonal correlation,
        ''' autoregressive returns a banded-decay structure with entries of the form <c>ρ^{|t-s|}</c>,
        ''' and unstructured returns a fully estimated symmetric correlation matrix.
        ''' </para>
        ''' <para>
        ''' When within-cluster time was supplied at fitting, the row and column labels correspond to the ordered time values used internally by the model.
        ''' Otherwise a generic sequential labeling is returned.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GEE_WCORR",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the fitted working correlation matrix for a generalized estimating equation handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GEE_WCORR(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GEE_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include row and column labels (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GeeHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim mat(,) As Double = h.Model.WorkingCorrelationMatrix
                If mat Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim labels() As String = BuildGeeTimeLabels(h.Model, mat.GetLength(0))
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)

                Return BuildLabeledMatrixOutput(mat, labels, labels, hdr)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GEE_WCORR", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the covariance matrix of the estimated generalized estimating equation coefficients.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GEE_FIT</c>.</param>
        ''' <param name="covarianceType">
        ''' Covariance estimator to return.
        ''' Accepted values are <c>robust</c>, <c>naive</c>, and <c>bias reduced</c>.
        ''' If omitted, the covariance type selected at fit time is used.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include row and column labels (default TRUE).</param>
        ''' <returns>
        ''' A square parameter-covariance matrix whose diagonal entries are coefficient variances
        ''' and whose off-diagonal entries are coefficient covariances.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Let
        ''' <c>B = Σ_i D_i' V_i^{-1} D_i</c>
        ''' and
        ''' <c>u_i = D_i' V_i^{-1} (y_i - μ_i)</c>.
        ''' Then the main covariance estimators reported for generalized estimating equations are:
        ''' </para>
        ''' <para>
        ''' <b>Naive / model-based:</b>
        ''' <c>Var_naive(β̂) = φ B^{-1}</c>
        ''' </para>
        ''' <para>
        ''' <b>Robust / empirical sandwich:</b>
        ''' <c>Var_robust(β̂) = B^{-1} (Σ_i u_i u_i') B^{-1}</c>
        ''' </para>
        ''' <para>
        ''' <b>Bias-reduced sandwich:</b>
        ''' a leverage-adjusted sandwich estimator intended to improve finite-cluster performance when the ordinary robust covariance is downward biased.
        ''' </para>
        ''' <para>
        ''' This function returns the full matrix rather than only the standard errors.
        ''' Therefore:
        ''' the square root of each diagonal entry equals the corresponding coefficient standard error,
        ''' and the off-diagonal terms quantify the joint sampling dependence between coefficient estimators.
        ''' </para>
        ''' <para>
        ''' The matrix is on the linear-predictor coefficient scale.
        ''' For example, under a log link it is the covariance of log-rate or log-mean parameters,
        ''' and under a logit link it is the covariance of marginal log-odds parameters.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GEE_VCOV",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the covariance matrix of the estimated generalized estimating equation coefficients.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GEE_VCOV(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GEE_FIT.")> handle As Object,
            <ExcelArgument(Name:="covarianceType", Description:="Covariance estimator: ""robust"", ""naive"", or ""bias reduced"". If omitted, the fit-time choice is used.")> Optional covarianceType As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include row and column labels (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GeeHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim covName As String = h.StandardErrorType
                If Not IsMissingArg(covarianceType) Then
                    covName = ParseGeeStandardErrorType(covarianceType)
                    If String.IsNullOrWhiteSpace(covName) Then Return ExcelError.ExcelErrorValue
                End If

                Dim mat(,) As Double = h.Model.GetParameterCovarianceMatrix(covName)
                If mat Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim names() As String = BuildParameterNames(h.VarNames, mat.GetLength(0))
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)

                Return BuildLabeledMatrixOutput(mat, names, names, hdr)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GEE_VCOV", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns residual diagnostics for a fitted generalized estimating equation handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GEE_FIT</c>.</param>
        ''' <param name="residType">
        ''' Residual block to return: <c>all</c> (default), <c>raw</c>, <c>deviance</c>, <c>pearson</c>, <c>stdpearson</c>, <c>stddeviance</c>, or <c>working</c>.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' Either a single residual column or a multi-column diagnostic table, depending on <paramref name="residType"/>.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The returned residuals are marginal residual diagnostics built from the fitted mean model.
        ''' The raw residual is
        ''' <c>r_ij = y_ij - μ̂_ij</c>.
        ''' The Pearson residual rescales by the model-implied marginal standard deviation,
        ''' approximately
        ''' <c>r^P_ij = (y_ij - μ̂_ij) / sqrt(V(μ̂_ij))</c>.
        ''' </para>
        ''' <para>
        ''' The deviance residual is the signed square root of the observation-wise deviance contribution,
        ''' and the working residual is
        ''' <c>(y_ij - μ̂_ij) / (dμ_ij/dη_ij)</c>.
        ''' Scaled Pearson and scaled deviance residuals divide by <c>sqrt(φ)</c>, where <c>φ</c> is the fitted scale parameter.
        ''' </para>
        ''' <para>
        ''' These residuals diagnose the mean specification rather than the adequacy of the working-correlation structure itself.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GEE_RESID",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns residual diagnostics for a fitted generalized estimating equation handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GEE_RESID(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GEE_FIT.")> handle As Object,
            <ExcelArgument(Name:="residType", Description:="Residual block: ""all"" (default), ""raw"", ""deviance"", ""pearson"", ""stdpearson"", ""stddeviance"", or ""working"".")> Optional residType As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GeeHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                h.Model.Residuals()

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim key As String = ParseGeeResidualType(residType)

                Select Case key
                    Case "all"
                        Dim rows As New List(Of Object())
                        Dim raw() As Double = h.Model.RawResiduals
                        Dim dev() As Double = h.Model.DevianceResiduals
                        Dim pear() As Double = h.Model.PearsonResiduals
                        Dim sdev() As Double = h.Model.ScaledDevianceResiduals
                        Dim spear() As Double = h.Model.ScaledPearsonResiduals
                        Dim work() As Double = h.Model.WorkingResiduals
                        If raw Is Nothing OrElse dev Is Nothing OrElse pear Is Nothing OrElse sdev Is Nothing OrElse spear Is Nothing OrElse work Is Nothing Then
                            Return ExcelError.ExcelErrorNA
                        End If

                        For i As Integer = 0 To raw.Length - 1
                            rows.Add(New Object() {raw(i), dev(i), pear(i), sdev(i), spear(i), work(i)})
                        Next

                        Return MaterializeRows(rows,
                                               hdr,
                                               New String() {"RawResidual", "DevianceResidual", "PearsonResidual", "StdDevianceResidual", "StdPearsonResidual", "WorkingResidual"})
                    Case "raw"
                        Return BuildResidualVectorOutput(h.Model.RawResiduals, "RawResidual", hdr)
                    Case "deviance"
                        Return BuildResidualVectorOutput(h.Model.DevianceResiduals, "DevianceResidual", hdr)
                    Case "pearson"
                        Return BuildResidualVectorOutput(h.Model.PearsonResiduals, "PearsonResidual", hdr)
                    Case "stdpearson"
                        Return BuildResidualVectorOutput(h.Model.ScaledPearsonResiduals, "StdPearsonResidual", hdr)
                    Case "stddeviance"
                        Return BuildResidualVectorOutput(h.Model.ScaledDevianceResiduals, "StdDevianceResidual", hdr)
                    Case "working"
                        Return BuildResidualVectorOutput(h.Model.WorkingResiduals, "WorkingResidual", hdr)
                    Case Else
                        Return ExcelError.ExcelErrorValue
                End Select

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GEE_RESID", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns predicted marginal means and linear predictors for new data under a fitted generalized estimating equation model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GEE_FIT</c>.</param>
        ''' <param name="newX">
        ''' New raw predictor matrix in the same raw-column order used at fitting time.
        ''' When the fitted model contains transformed terms, interactions, or categorical encodings, those derived columns are rebuilt automatically from this raw matrix using the original model specification.
        ''' </param>
        ''' <param name="newOffset">
        ''' Optional offset vector for the new observations.
        ''' It is required when the fitted model used an offset and enters additively on the linear-predictor scale.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A two-column table containing the predicted marginal mean response <c>μ̂_i</c> and the linear predictor <c>η̂_i</c> for each supplied observation.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' For new observations the worksheet function reconstructs the expanded design columns from the stored predictor specification,
        ''' then evaluates
        ''' <c>η̂_i = β̂_0 + x_i'β̂ + o_i</c>
        ''' and
        ''' <c>μ̂_i = g^{-1}(η̂_i)</c>.
        ''' The returned mean is therefore on the natural response scale, while the second column remains on the link scale.
        ''' </para>
        ''' <para>
        ''' The prediction is marginal with respect to the working-correlation structure.
        ''' The cluster identifier and working covariance affect estimation efficiency and inference, but the fitted mean at a new covariate pattern is determined by the estimated regression coefficients and the chosen link.
        ''' </para>
        ''' <para>
        ''' Intercept-only models can be predicted without supplying <paramref name="newX"/>.
        ''' In that case, a single prediction row is returned unless a new offset vector is supplied, in which case one prediction is returned for each offset value.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GEE_PRED",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns predicted marginal means and linear predictors for new data under a fitted generalized estimating equation model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GEE_PRED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GEE_FIT.")> handle As Object,
            <ExcelArgument(AllowReference:=True, Name:="newX", Description:="New raw predictor matrix in the same raw-column order used at fitting time.")> Optional newX As Object = Nothing,
            <ExcelArgument(Name:="newOffset", Description:="Optional offset vector for the new observations.")> Optional newOffset As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GeeHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim rawPredictorKeys As String() = If(h.RawPredictorKeys, h.RawVarNames)
                If rawPredictorKeys Is Nothing Then rawPredictorKeys = New String() {}

                Dim nRows As Integer = 0
                Dim offsetVals() As Double = Nothing
                Dim expandedX(,) As Double = Nothing

                If rawPredictorKeys.Length < 1 Then
                    If Not TryPrepareInterceptOnlyPredictionInputs(newOffset, h.HasOffset, nRows, offsetVals) Then
                        Return ExcelError.ExcelErrorValue
                    End If
                Else
                    Dim imported As glmData = Nothing
                    If Not UDFhelpers.TryBuildPredictorDataFromUdfArgs(newX, rawPredictorKeys, newOffset, h.HasOffset, imported) Then
                        Return ExcelError.ExcelErrorValue
                    End If
                    If imported.nCols <> rawPredictorKeys.Length Then Return ExcelError.ExcelErrorValue

                    Dim expandedNames() As String = Nothing
                    Dim designErr As String = Nothing
                    If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromDesignSpec(rawX:=imported.DataDbl,
                                                                                                        fullRawPredictorKeys:=rawPredictorKeys,
                                                                                                        designSpec:=h.DesignSpec,
                                                                                                        expandedX:=expandedX,
                                                                                                        expandedPredictorNames:=expandedNames,
                                                                                                        errorMessage:=designErr,
                                                                                                        omitCategoricalReference:=h.OmitCategoricalReference) Then
                        Return ExcelError.ExcelErrorValue
                    End If

                    If expandedNames Is Nothing Then expandedNames = New String() {}
                    If expandedNames.Length <> h.ExpandedPredictorNames.Length Then Return ExcelError.ExcelErrorValue

                    nRows = imported.nRows
                    offsetVals = If(imported.bOffset, imported.OffsetData, Nothing)
                    If Not UDFhelpers.HasOnlyFinite(offsetVals) Then Return ExcelError.ExcelErrorValue
                End If

                Dim beta() As Double = h.Model.results.Coeffs_est
                If beta Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, nRows + 1, nRows)
                Dim out(outRows - 1, 1) As Object
                Dim r0 As Integer = 0

                If hdr Then
                    out(0, 0) = "PredictedResponse"
                    out(0, 1) = "LinearPredictor"
                    r0 = 1
                End If

                For i As Integer = 0 To nRows - 1
                    Dim eta As Double = ComputeLinearPredictor(expandedX, i, beta, True, offsetVals)
                    Dim mu As Double = h.LinkObject.inverse(eta)

                    out(r0 + i, 0) = SafeExcelNumber(mu)
                    out(r0 + i, 1) = SafeExcelNumber(eta)
                Next

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GEE_PRED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a fitted generalized estimating equation handle from the in-memory cache.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GEE_FIT</c>.</param>
        ''' <returns>
        ''' TRUE if the handle was found and removed; otherwise FALSE.
        ''' </returns>
        ''' <remarks>
        ''' Handles persist only for the current Excel session and reference fitted models stored in memory.
        ''' This function explicitly releases one cached model so that repeated refits do not keep unnecessary objects alive.
        ''' Existing worksheet formulas that still reference a dropped handle will subsequently return a handle-not-found error until the model is refitted.
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GEE_DROP",
            Category:="BESHStatNG - Regression Models",
            Description:="Removes a fitted generalized estimating equation handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GEE_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GEE_FIT.")> handle As Object
        ) As Object
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return ExcelError.ExcelErrorValue
            Dim removed As GeeHandle = Nothing
            Return _geeCache.TryRemove(key, removed)
        End Function

        Private Function BuildGeeTimeLabels(model As GEE, size As Integer) As String()
            Dim labels(size - 1) As String

            Dim dict = model.TimesDict
            If dict Is Nothing OrElse dict.Count <> size Then
                For i As Integer = 0 To size - 1
                    labels(i) = "t" & (i + 1).ToString(CultureInfo.InvariantCulture)
                Next
                Return labels
            End If

            For Each kvp In dict
                Dim idx As Integer = kvp.Value
                If idx >= 0 AndAlso idx < size Then
                    labels(idx) = "t=" & kvp.Key.ToString("0.###############", CultureInfo.InvariantCulture)
                End If
            Next

            For i As Integer = 0 To size - 1
                If String.IsNullOrWhiteSpace(labels(i)) Then
                    labels(i) = "t" & (i + 1).ToString(CultureInfo.InvariantCulture)
                End If
            Next

            Return labels
        End Function

        Private Function BuildLabeledMatrixOutput(matrix(,) As Double,
                                                  rowNames() As String,
                                                  colNames() As String,
                                                  includeHeader As Boolean) As Object

            If matrix Is Nothing Then Return ExcelError.ExcelErrorNA

            Dim nRows As Integer = matrix.GetLength(0)
            Dim nCols As Integer = matrix.GetLength(1)

            If rowNames Is Nothing OrElse rowNames.Length <> nRows Then
                ReDim rowNames(nRows - 1)
                For i As Integer = 0 To nRows - 1
                    rowNames(i) = "Row" & (i + 1).ToString(CultureInfo.InvariantCulture)
                Next
            End If

            If colNames Is Nothing OrElse colNames.Length <> nCols Then
                ReDim colNames(nCols - 1)
                For j As Integer = 0 To nCols - 1
                    colNames(j) = "Col" & (j + 1).ToString(CultureInfo.InvariantCulture)
                Next
            End If

            If Not includeHeader Then
                Dim outNoHdr(nRows - 1, nCols - 1) As Object
                For i As Integer = 0 To nRows - 1
                    For j As Integer = 0 To nCols - 1
                        outNoHdr(i, j) = SafeExcelNumber(matrix(i, j))
                    Next
                Next
                Return outNoHdr
            End If

            Dim out(nRows, nCols) As Object
            out(0, 0) = ""

            For j As Integer = 0 To nCols - 1
                out(0, j + 1) = colNames(j)
            Next

            For i As Integer = 0 To nRows - 1
                out(i + 1, 0) = rowNames(i)
                For j As Integer = 0 To nCols - 1
                    out(i + 1, j + 1) = SafeExcelNumber(matrix(i, j))
                Next
            Next

            Return out
        End Function

        Private Function ParseGeeCovarianceName(v As Object) As String
            Dim s As String = UDFhelpers.AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return "Independence"

            Select Case NormalizeKey(s)
                Case "independence", "independent"
                    Return "Independence"
                Case "exchangeable", "exchangable", "compoundsymmetry", "cs"
                    Return "Exchangable"
                Case "autoregressive", "ar1", "ar"
                    Return "Autoregressive"
                Case "unstructured", "uns"
                    Return "Unstructured"
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function ParseGeeStandardErrorType(v As Object) As String
            Dim s As String = UDFhelpers.AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return "Robust"

            Select Case NormalizeKey(s)
                Case "robust", "sandwich", "empirical"
                    Return "Robust"
                Case "naive", "modelbased", "model"
                    Return "Naive"
                Case "biasreduced", "biascorrected", "biascorr", "manclderouen"
                    Return "Bias Reduced"
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function ParseGeeResidualType(v As Object) As String
            Dim s As String = UDFhelpers.AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case NormalizeKey(s)
                Case "all"
                    Return "all"
                Case "raw", "response"
                    Return "raw"
                Case "deviance", "dev"
                    Return "deviance"
                Case "pearson"
                    Return "pearson"
                Case "stdpearson", "standardizedpearson", "scaledpearson"
                    Return "stdpearson"
                Case "stddeviance", "standardizeddeviance", "scaleddeviance"
                    Return "stddeviance"
                Case "working", "work"
                    Return "working"
                Case Else
                    Return "all"
            End Select
        End Function

        Private Function TryGetHandle(handle As Object, ByRef h As GeeHandle) As Boolean
            h = Nothing
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _geeCache.TryGetValue(key, h)
        End Function

        Private Function BuildParameterNames(predictorNames() As String, coefficientCount As Integer) As String()
            If coefficientCount < 1 Then Return New String() {}

            Dim out(coefficientCount - 1) As String
            out(0) = "Intercept"
            Dim source() As String = If(predictorNames, New String() {})
            For i As Integer = 1 To coefficientCount - 1
                Dim srcIndex As Integer = i - 1
                If srcIndex < source.Length Then
                    out(i) = source(srcIndex)
                Else
                    out(i) = "Param" & (i + 1).ToString(CultureInfo.InvariantCulture)
                End If
            Next
            Return out
        End Function

        Private Function MaterializeRows(rows As List(Of Object()), includeHeader As Boolean, headers() As String) As Object
            If rows Is Nothing Then Return ExcelError.ExcelErrorNA

            Dim colCount As Integer = If(headers Is Nothing, 0, headers.Length)
            If colCount = 0 AndAlso rows.Count > 0 Then
                colCount = rows(0).Length
            End If
            If colCount < 1 Then Return ExcelError.ExcelErrorNA

            Dim outRows As Integer = rows.Count + If(includeHeader, 1, 0)
            Dim out(outRows - 1, colCount - 1) As Object
            Dim r0 As Integer = 0

            If includeHeader Then
                For j As Integer = 0 To colCount - 1
                    out(0, j) = headers(j)
                Next
                r0 = 1
            End If

            For i As Integer = 0 To rows.Count - 1
                Dim row() As Object = rows(i)
                For j As Integer = 0 To colCount - 1
                    out(r0 + i, j) = If(j < row.Length, row(j), Nothing)
                Next
            Next

            Return out
        End Function

        Private Function TryParseNumericVector(v As Object, ByRef values() As Double) As Boolean
            values = Nothing

            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then
                Return False
            End If

            Dim s As String = TryCast(v, String)
            If s IsNot Nothing Then
                Dim parts = s.Split({","c, ";"c, " "c, ChrW(9)}, StringSplitOptions.RemoveEmptyEntries)
                If parts.Length < 1 Then Return False

                ReDim values(parts.Length - 1)
                For i As Integer = 0 To parts.Length - 1
                    Dim parsed As Double
                    If Not Double.TryParse(parts(i), NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture, parsed) AndAlso
                       Not Double.TryParse(parts(i), NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, parsed) Then
                        values = Nothing
                        Return False
                    End If
                    If Double.IsNaN(parsed) OrElse Double.IsInfinity(parsed) Then
                        values = Nothing
                        Return False
                    End If
                    values(i) = parsed
                Next
                Return True
            End If

            Dim arr As Object(,) = UDFhelpers.Get2D(v)
            If arr Is Nothing Then Return False

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            If rows < 1 OrElse cols < 1 Then Return False
            If rows <> 1 AndAlso cols <> 1 Then Return False

            Dim count As Integer = rows * cols
            ReDim values(count - 1)
            Dim k As Integer = 0
            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    Dim d As Double? = UDFhelpers.TryGetDouble(arr(i, j))
                    If Not d.HasValue Then
                        values = Nothing
                        Return False
                    End If
                    values(k) = d.Value
                    k += 1
                Next
            Next

            Return True
        End Function

    End Module

End Namespace
