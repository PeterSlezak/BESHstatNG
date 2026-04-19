Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for generalized linear models with reusable handle-based workflow from Excel.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These worksheet functions fit and reuse regression models of the form
    ''' <c>g(μ_i) = η_i = β_0 + x_i'β + o_i</c>,
    ''' where <c>μ_i = E[Y_i | x_i]</c>, <c>g(·)</c> is the chosen link function, and <c>o_i</c> is an optional offset.
    ''' The response distribution is chosen from common exponential-family models such as Gaussian, Binomial, Poisson,
    ''' Gamma, and a fixed-dispersion Negative Binomial variant.
    ''' </para>
    ''' <para>
    ''' The supported families imply different mean-variance relationships, including
    ''' <c>Var(Y_i|x_i)=σ^2</c> for Gaussian-type modeling,
    ''' <c>Var(Y_i|x_i)=μ_i(1-μ_i)</c> for Binomial responses,
    ''' <c>Var(Y_i|x_i)=μ_i</c> for Poisson counts,
    ''' <c>Var(Y_i|x_i)=φ μ_i^2</c> for Gamma responses, and
    ''' <c>Var(Y_i|x_i)=μ_i + α μ_i^2</c> for the fixed-dispersion Negative Binomial form.
    ''' The link determines how the conditional mean is mapped to the linear predictor, for example
    ''' <c>logit(μ_i)=η_i</c>, <c>log(μ_i)=η_i</c>, or <c>μ_i=η_i</c> under the identity link.
    ''' </para>
    ''' <para>
    ''' Estimation is carried out by iteratively reweighted least squares (IRLS), also viewed as Fisher scoring for many GLMs.
    ''' At iteration <c>t</c>, the method updates coefficients by solving a weighted least-squares problem for the working response
    ''' <c>z_i^{(t)} = η_i^{(t)} + (y_i-μ_i^{(t)}) (dη_i/dμ_i)</c>
    ''' with working weights proportional to
    ''' <c>[(dμ_i/dη_i)^2 / Var(Y_i|x_i)]</c>, optionally multiplied by user-supplied case weights.
    ''' The iteration stops when the requested tolerance is met or the maximum number of iterations is reached.
    ''' </para>
    ''' <para>
    ''' The fit function stores the estimated model in an in-memory cache for the current Excel session and returns a text handle.
    ''' The remaining worksheet functions reuse that handle to return coefficient summaries, model diagnostics, residuals,
    ''' predictions, and explicit cleanup without refitting.
    ''' </para>
    ''' <para>
    ''' Predictor formulas allow additive effects, polynomial terms, interactions, and categorical main effects to be built from
    ''' the raw predictor matrix supplied to the fit function. This lets the worksheet formula define the design matrix while
    ''' keeping prediction on new data consistent with the original model specification.
    ''' </para>
    ''' </remarks>
    Public Module GLMUDFs

        Private ReadOnly _glmCache As New ConcurrentDictionary(Of String, GlmHandle)(StringComparer.OrdinalIgnoreCase)

        Private Class GlmHandle
            Public Property Handle As String
            Public Property Model As GLM
            Public Property VarNames As String()
            Public Property ExpandedPredictorNames As String()
            Public Property RawVarNames As String()
            Public Property RawPredictorKeys As String()
            Public Property RawPredictorAbsoluteLetters As String()
            Public Property DesignSpec As RegressionFormulaDesignSpec
            Public Property OmitCategoricalReference As Boolean
            Public Property HasOffset As Boolean
            Public Property HasWeights As Boolean
            Public Property InterceptIncluded As Boolean
            Public Property FamilyName As String
            Public Property LinkName As String
            Public Property Alpha As Double
        End Class

        ''' <summary>
        ''' Fits a generalized linear model and returns a reusable handle.
        ''' </summary>
        ''' <param name="y">
        ''' Numeric response vector (single column).
        ''' Typical uses are continuous responses for Gaussian models, 0/1 outcomes for Binomial models,
        ''' nonnegative counts for Poisson or Negative Binomial models, and positive continuous responses for Gamma models.
        ''' Each row represents one observation.
        ''' </param>
        ''' <param name="x">
        ''' Raw predictor matrix with one row per observation.
        ''' Rows must align with <paramref name="y"/>, <paramref name="offset"/>, and <paramref name="weights"/> whenever those inputs are supplied.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional raw predictor names supplied as a comma-separated list or as a one-row/one-column range.
        ''' These names are used by the formula parser and by the returned coefficient table.
        ''' </param>
        ''' <param name="family">
        ''' Response family that determines the variance structure and likelihood contribution.
        ''' Accepted values include <c>gaussian</c>, <c>binomial</c>, <c>poisson</c>, <c>gamma</c>, and <c>negative binomial</c>/<c>nb</c>.
        ''' The default is <c>gaussian</c>.
        ''' Representative mean-variance relationships are
        ''' <c>Var(Y_i|x_i)=σ^2</c> for Gaussian,
        ''' <c>Var(Y_i|x_i)=μ_i(1-μ_i)</c> for Binomial,
        ''' <c>Var(Y_i|x_i)=μ_i</c> for Poisson,
        ''' <c>Var(Y_i|x_i)=φ μ_i^2</c> for Gamma, and
        ''' <c>Var(Y_i|x_i)=μ_i + α μ_i^2</c> for the fixed-dispersion Negative Binomial form.
        ''' </param>
        ''' <param name="link">
        ''' Optional link function <c>g(·)</c> used in <c>g(μ_i)=η_i</c>.
        ''' If omitted, the family's canonical or default link is used.
        ''' Accepted values include <c>logit</c>, <c>probit</c>, <c>log</c>, <c>identity</c>, <c>sqrt</c>, <c>inverse</c>, and <c>power</c>.
        ''' The link controls the interpretation of coefficients; for example, a log link yields multiplicative effects on the mean,
        ''' while a logit link yields additive effects on the log-odds scale.
        ''' </param>
        ''' <param name="offset">
        ''' Optional numeric offset vector (single column).
        ''' The offset enters additively on the linear-predictor scale:
        ''' <c>η_i = β_0 + x_i'β + o_i</c>.
        ''' Under a log link, a common choice for rate models is <c>o_i = log(t_i)</c>, where <c>t_i</c> is exposure or person-time.
        ''' </param>
        ''' <param name="weights">
        ''' Optional nonnegative case weights (single column).
        ''' These weights scale the contribution of each observation in the IRLS fitting equations and in the likelihood-based summaries.
        ''' </param>
        ''' <param name="includeIntercept">
        ''' TRUE to include an intercept term (default TRUE).
        ''' When FALSE, the fitted predictor is constrained to pass through the origin on the link scale.
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
        ''' Optional fixed dispersion parameter for the Negative Binomial family.
        ''' It is ignored by the other families.
        ''' In the NB2 parameterization the variance is <c>μ_i + α μ_i^2</c>, so this argument supplies the fixed value of <c>α</c>.
        ''' Larger values imply more overdispersion relative to the Poisson model.
        ''' </param>
        ''' <param name="power">
        ''' Optional power parameter used only when <paramref name="link"/> is <c>power</c>.
        ''' For a power link, the transformation is controlled by this exponent, and the value must be finite and nonzero.
        ''' </param>
        ''' <param name="maxIter">
        ''' Maximum number of IRLS iterations (default 20).
        ''' Larger values can help difficult models converge but may increase calculation time.
        ''' </param>
        ''' <param name="tol">
        ''' Positive convergence tolerance for IRLS (default 1E-8).
        ''' Smaller values request a stricter convergence check on successive updates.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level used for confidence intervals stored with the fitted result (default 0.05).
        ''' This controls inferential reporting only; it does not change the fitted coefficients.
        ''' </param>
        ''' <returns>
        ''' A text handle identifying the fitted model within the current Excel session.
        ''' The handle can be passed to the associated summary, diagnostics, residual, prediction, and cleanup worksheet functions without refitting.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function fits the generalized linear model defined by
        ''' <c>g(μ_i)=β_0+x_i'β+o_i</c>,
        ''' where <c>μ_i=E[Y_i|x_i]</c> and the conditional variance is determined by the chosen family.
        ''' Under canonical links, the score equations take their standard exponential-family form and the IRLS updates correspond to Fisher scoring.
        ''' </para>
        ''' <para>
        ''' At each iteration, the algorithm forms the working response
        ''' <c>z_i = η_i + (y_i-μ_i)(dη_i/dμ_i)</c>
        ''' and solves a weighted least-squares update using working weights proportional to
        ''' <c>[(dμ_i/dη_i)^2 / Var(Y_i|x_i)]</c>.
        ''' User-supplied case weights multiply these working weights.
        ''' This procedure is the standard numerical method used to maximize the GLM log-likelihood or quasi-likelihood criterion.
        ''' </para>
        ''' <para>
        ''' Coefficients are reported on the link scale.
        ''' For example, under a log link, <c>exp(β_j)</c> is the multiplicative change in the fitted mean associated with a one-unit increase in predictor <c>x_j</c> holding other terms fixed.
        ''' Under a logit link, <c>exp(β_j)</c> is an odds ratio for a one-unit change in <c>x_j</c>.
        ''' </para>
        ''' <para>
        ''' Rows containing invalid or non-finite values in the response, predictors, offset, or weights are removed before fitting.
        ''' If too few valid observations remain or the design matrix becomes non-estimable, the function returns an Excel error instead of a handle.
        ''' </para>
        ''' <para>
        ''' If <c>formulaAddressing="absolute"</c> is used, the predictor argument should be a direct worksheet range so absolute worksheet column letters can be resolved.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.GLM_FIT(A2:A101,B2:D101,"Age,BMI,Treat","binomial","logit")
        ''' =BESH.REGR.GLM_FIT(A2:A101,B2:E101,"Dose,Age,Stage,Center","poisson","log",F2:F101,,TRUE,"A + B + factor(D)","relative")
        ''' =BESH.REGR.GLM_FIT(A2:A101,B2:C101,"X1,X2","negative binomial","log",, ,TRUE,,,0.75)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.GLM_FIT",
            Category:="BESHStatNG - Regression Models",
            Description:="Fits a generalized linear model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLM_FIT(
            <ExcelArgument(Name:="y", Description:="Numeric response vector (single column).")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Raw predictor matrix with one row per observation.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional raw predictor names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="family", Description:="GLM family: ""gaussian"" (default), ""binomial"", ""poisson"", ""gamma"", or ""negative binomial"" / ""nb"".")> Optional family As Object = Nothing,
            <ExcelArgument(Name:="link", Description:="Optional link function. Defaults to the family's canonical/default link.")> Optional link As Object = Nothing,
            <ExcelArgument(Name:="offset", Description:="Optional numeric offset vector (single column).")> Optional offset As Object = Nothing,
            <ExcelArgument(Name:="weights", Description:="Optional nonnegative case weights (single column).")> Optional weights As Object = Nothing,
            <ExcelArgument(Name:="includeIntercept", Description:="TRUE to include an intercept term (default TRUE).")> Optional includeIntercept As Object = Nothing,
            <ExcelArgument(Name:="formula", Description:="Optional RHS formula used to expand the raw predictor matrix.")> Optional formula As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Formula-addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing,
            <ExcelArgument(Name:="dispersion", Description:="Optional fixed dispersion parameter for the negative-binomial family.")> Optional dispersion As Object = Nothing,
            <ExcelArgument(Name:="power", Description:="Optional power parameter used only when link=""power"".")> Optional power As Object = Nothing,
            <ExcelArgument(Name:="maxIter", Description:="Maximum IRLS iterations (default 20).")> Optional maxIter As Object = Nothing,
            <ExcelArgument(Name:="tol", Description:="Convergence tolerance (default 1E-8).")> Optional tol As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha used internally for confidence intervals (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "GLM_FIT (editing...)"

            Try
                Dim imported As glmData = Nothing
                If Not UDFhelpers.TryBuildGlmDataFromUdfArgs(y, x, varNames, offset, weights, imported) Then
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
                Dim rowIds() As Integer = imported.RowIds

                If fitData Is Nothing OrElse fitVarNames Is Nothing OrElse fitVarNames.Length < 1 Then Return ExcelError.ExcelErrorValue
                If Not UDFhelpers.HasOnlyFinite(fitOffset) Then Return ExcelError.ExcelErrorValue
                If Not UDFhelpers.HasOnlyFinite(fitWeights, True) Then Return ExcelError.ExcelErrorValue

                Dim interceptFlag As Boolean = UDFhelpers.GetOptionalBool(includeIntercept, True)
                If Not interceptFlag AndAlso fitVarNames.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim alphaValue As Double = 0.05R
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim maxIterValue As Integer = UDFhelpers.GetOptionalInt(maxIter, 20)
                Dim tolValue As Double = UDFhelpers.GetOptionalDouble(tol, 0.00000001R)
                If maxIterValue < 1 Then Return ExcelError.ExcelErrorNum
                If Double.IsNaN(tolValue) OrElse Double.IsInfinity(tolValue) OrElse tolValue <= 0.0R Then Return ExcelError.ExcelErrorNum

                Dim familyCode As String = ParseFamilyCode(family)
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
                    If IsMissingArg(power) Then Return ExcelError.ExcelErrorNum
                    Dim powerValue As Double = UDFhelpers.GetOptionalDouble(power, Double.NaN)
                    If Double.IsNaN(powerValue) OrElse Double.IsInfinity(powerValue) OrElse powerValue = 0.0R Then Return ExcelError.ExcelErrorNum
                    lnk = regression.createLink("Power", powerValue)
                Else
                    lnk = regression.createLink(linkName)
                End If
                If lnk Is Nothing Then Return ExcelError.ExcelErrorValue

                Dim mdl As New GLM(fam, lnk)
                mdl.bHosmerLemeshow = True
                mdl.bComputeResiduals = False
                mdl.bReturnCov = False
                mdl.bIterationDetails = False
                mdl.settingInputs(alphaValue, maxIterValue, tolValue)
                mdl.data(fitData,
                         rowIds,
                         fitOffset,
                         fitWeights)
                mdl.setVarNames(fitVarNames)
                mdl.Fit(If(interceptFlag, 1, 0), False)

                If mdl.results Is Nothing OrElse mdl.results.Coeffs_est Is Nothing OrElse mdl.results.Coeffs_SEs Is Nothing Then
                    If Not String.IsNullOrWhiteSpace(mdl.strError) Then Return mdl.strError.Trim()
                    Return ExcelError.ExcelErrorValue
                End If

                Dim handleKey As String = "GLM:" & Guid.NewGuid().ToString("N")
                Dim h As New GlmHandle With {
                    .Handle = handleKey,
                    .Model = mdl,
                    .VarNames = CloneStringArray(If(mdl.results.varNames, New String() {})),
                    .ExpandedPredictorNames = CloneStringArray(fitPredictorNames),
                    .RawVarNames = CloneStringArray(If(designBuild.FullRawPredictorNames, New String() {})),
                    .RawPredictorKeys = CloneStringArray(If(designBuild.FullRawPredictorKeys, New String() {})),
                    .RawPredictorAbsoluteLetters = CloneStringArray(If(designBuild.FullRawPredictorAbsoluteLetters, New String() {})),
                    .DesignSpec = designBuild.DesignSpec,
                    .OmitCategoricalReference = True,
                    .HasOffset = (fitOffset IsNot Nothing),
                    .HasWeights = (fitWeights IsNot Nothing),
                    .InterceptIncluded = interceptFlag,
                    .FamilyName = fam.ToString(),
                    .LinkName = lnk.ToString(),
                    .Alpha = alphaValue
                }

                _glmCache(handleKey) = h
                Return handleKey

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GLM_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the coefficient summary table for a fitted generalized linear model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GLM_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used to form the displayed Wald confidence intervals.
        ''' If omitted, the confidence level stored with the fitted model is used.
        ''' </param>
        ''' <returns>
        ''' A table with one row per estimated parameter.
        ''' The columns contain the parameter name, parameter type, estimated coefficient, standard error,
        ''' Wald <c>z</c> statistic, two-sided p-value, and lower/upper confidence limits.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The reported coefficients are on the link scale of the fitted model.
        ''' Thus the interpretation depends on the chosen link:
        ''' identity-link coefficients act directly on the mean,
        ''' log-link coefficients act on <c>log(μ)</c>, and
        ''' logit-link coefficients act on <c>log(μ/(1-μ))</c>.
        ''' </para>
        ''' <para>
        ''' Standard errors are derived from the estimated covariance matrix of the coefficients, and the table reports the Wald statistic
        ''' <c>z_j = β̂_j / SE(β̂_j)</c>
        ''' together with the usual two-sided large-sample p-value
        ''' <c>2 Φ(-|z_j|)</c>.
        ''' Confidence intervals are shown as
        ''' <c>β̂_j ± z_{1-α/2} SE(β̂_j)</c>.
        ''' </para>
        ''' <para>
        ''' No exponentiation is applied automatically.
        ''' When an odds-ratio or rate-ratio interpretation is desired, users can exponentiate the returned coefficients and confidence limits externally.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLM_SUMMARY",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the coefficient summary table for a fitted generalized linear model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLM_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLM_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for the displayed confidence intervals.")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As GlmHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim alphaValue As Double = h.Alpha
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim beta() As Double = h.Model.results.Coeffs_est
                Dim se() As Double = h.Model.results.Coeffs_SEs
                If beta Is Nothing OrElse se Is Nothing Then Return ExcelError.ExcelErrorNA
                If beta.Length <> se.Length Then Return ExcelError.ExcelErrorNA

                Dim names() As String = BuildParameterNames(h, beta.Length)
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
                    out(r0 + i, 1) = If(h.InterceptIncluded AndAlso i = 0, "Intercept", "Slope")
                    out(r0 + i, 2) = beta(i)
                    out(r0 + i, 3) = se(i)
                    out(r0 + i, 4) = z
                    out(r0 + i, 5) = pv
                    out(r0 + i, 6) = beta(i) - zCrit * se(i)
                    out(r0 + i, 7) = beta(i) + zCrit * se(i)
                Next

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GLM_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns model-level diagnostics and fit statistics for a fitted generalized linear model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GLM_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A table of model-level statistics such as likelihood-based fit summaries, deviance measures,
        ''' information criteria, degrees of freedom, p-values where available, computational time, and any warnings.
        ''' The exact row set depends on the selected family and the fitted model output.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Generalized linear models are commonly assessed with quantities such as residual deviance,
        ''' null deviance, likelihood-ratio style tests, Akaike information criterion (AIC), and associated degrees of freedom.
        ''' These are returned here in the order stored with the fitted model.
        ''' For many families, deviance is the summed contribution of the observation-wise log-likelihood ratio between the fitted model and the saturated model.
        ''' </para>
        ''' <para>
        ''' For Binomial models, this output also adds the numbers of observations with response greater than zero and equal to zero,
        ''' which helps document class balance in binary-response applications.
        ''' </para>
        ''' <para>
        ''' Any convergence messages or numerical warnings produced during fitting are returned as a final row so they can be surfaced in generated documentation or audit sheets.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLM_TESTS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns model-level diagnostics and fit statistics for a fitted generalized linear model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLM_TESTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLM_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GlmHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim labels() As String = h.Model.results.ModelTableLabels
                Dim vals(,) As Object = h.Model.results.ModelTableVals
                If labels Is Nothing OrElse vals Is Nothing Then Return ExcelError.ExcelErrorNA
                If vals.GetLength(0) <> labels.Length Then Return ExcelError.ExcelErrorNA

                Dim extraRows As Integer = 1
                If TypeOf h.Model.pFamily Is regression.Binomial Then extraRows += 2
                If Not String.IsNullOrWhiteSpace(h.Model.strError) Then extraRows += 1

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, labels.Length + extraRows + 1, labels.Length + extraRows)
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
                    out(r, 2) = vals(i, 1)
                    out(r, 3) = vals(i, 2)
                    r += 1
                Next

                If TypeOf h.Model.pFamily Is regression.Binomial Then
                    out(r, 0) = "Cases where Y>0"
                    out(r, 1) = h.Model.pSuccess
                    out(r, 2) = ""
                    out(r, 3) = ""
                    r += 1

                    out(r, 0) = "Cases where Y=0"
                    out(r, 1) = h.Model.pFail
                    out(r, 2) = ""
                    out(r, 3) = ""
                    r += 1
                End If

                out(r, 0) = "Computational time (s)"
                out(r, 1) = h.Model.CompTime
                out(r, 2) = ""
                out(r, 3) = ""
                r += 1

                If Not String.IsNullOrWhiteSpace(h.Model.strError) Then
                    out(r, 0) = "Warnings"
                    out(r, 1) = h.Model.strError.Trim()
                    out(r, 2) = ""
                    out(r, 3) = ""
                End If

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GLM_TESTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns residual diagnostics for a fitted generalized linear model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GLM_FIT</c>.</param>
        ''' <param name="residType">
        ''' Residual block to return: <c>all</c> (default), <c>raw</c>, <c>deviance</c>, <c>pearson</c>, <c>stdpearson</c>,
        ''' <c>stddeviance</c>, <c>leverage</c>, or <c>cook</c>.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' Either a single residual column or a multi-column diagnostic table, depending on <paramref name="residType"/>.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Residuals summarize different aspects of model misfit.
        ''' The raw or response residual is
        ''' <c>r_i = y_i - μ̂_i</c>.
        ''' The Pearson residual rescales this difference by the model-based standard deviation,
        ''' approximately
        ''' <c>r_{P,i} = (y_i-μ̂_i) / sqrt(Var(Y_i|x_i))</c>.
        ''' </para>
        ''' <para>
        ''' The deviance residual is the signed square root of the observation-wise contribution to model deviance,
        ''' <c>r_{D,i} = sign(y_i-μ̂_i) sqrt(d_i)</c>,
        ''' where <c>d_i</c> is the contribution to twice the log-likelihood ratio comparing the fitted model with the saturated model.
        ''' Deviance residuals are often more comparable across non-Gaussian families than raw residuals.
        ''' </para>
        ''' <para>
        ''' Standardized residuals account for leverage by dividing by approximately <c>sqrt(1-h_i)</c>,
        ''' where <c>h_i</c> is the diagonal element of the generalized hat matrix.
        ''' High leverage indicates observations with unusual predictor patterns, and Cook's distance combines residual size and leverage to measure potential influence on the fitted coefficients.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLM_RESID",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns residual diagnostics for a fitted generalized linear model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLM_RESID(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLM_FIT.")> handle As Object,
            <ExcelArgument(Name:="residType", Description:="Residual block: ""all"" (default), ""raw"", ""deviance"", ""pearson"", ""stdpearson"", ""stddeviance"", ""leverage"", or ""cook"".")> Optional residType As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GlmHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                h.Model.bComputeResiduals = True
                h.Model.Residuals()

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim key As String = ParseGlmResidualType(residType)

                Select Case key
                    Case "all"
                        Return BuildAllResidualOutput(h.Model, hdr)
                    Case "raw"
                        Return BuildResidualVectorOutput(h.Model.pRaw_res, "RawResidual", hdr)
                    Case "deviance"
                        Return BuildResidualVectorOutput(h.Model.pDeviance_res, "DevianceResidual", hdr)
                    Case "pearson"
                        Return BuildResidualVectorOutput(h.Model.pPearsChisq_res, "PearsonResidual", hdr)
                    Case "stdpearson"
                        Return BuildResidualVectorOutput(h.Model.pStPearsChisq_res, "StdPearsonResidual", hdr)
                    Case "stddeviance"
                        Return BuildResidualVectorOutput(h.Model.pStDeviance_res, "StdDevianceResidual", hdr)
                    Case "leverage"
                        Return BuildResidualVectorOutput(h.Model.pLeverage, "Leverage", hdr)
                    Case "cook"
                        Return BuildResidualVectorOutput(h.Model.pCookDistance, "CookDistance", hdr)
                    Case Else
                        Return BuildAllResidualOutput(h.Model, hdr)
                End Select

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GLM_RESID", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns predicted responses and linear predictors for new data under a fitted generalized linear model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GLM_FIT</c>.</param>
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
        ''' A two-column table containing the predicted mean response <c>μ̂_i</c> and the linear predictor <c>η̂_i</c> for each supplied observation.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Predictions are formed as
        ''' <c>η̂_i = β̂_0 + x_i'β̂ + o_i</c>
        ''' and
        ''' <c>μ̂_i = g^{-1}(η̂_i)</c>,
        ''' where <c>o_i</c> is the optional offset for the new observation.
        ''' The returned <c>PredictedResponse</c> column is therefore on the natural mean scale of the response,
        ''' while <c>LinearPredictor</c> remains on the link scale.
        ''' </para>
        ''' <para>
        ''' Under common links, this means the first column is a fitted probability for Binomial-logit models,
        ''' a fitted count or rate-scale mean for Poisson or Negative Binomial log-link models,
        ''' and a fitted mean outcome for Gaussian identity-link models.
        ''' </para>
        ''' <para>
        ''' Intercept-only models can be predicted without supplying <paramref name="newX"/>.
        ''' In that case, a single prediction row is returned unless a new offset vector is supplied, in which case one prediction is returned for each offset value.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLM_PRED",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns predicted responses and linear predictors for new data under a fitted generalized linear model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLM_PRED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLM_FIT.")> handle As Object,
            <ExcelArgument(AllowReference:=True, Name:="newX", Description:="New raw predictor matrix in the same raw-column order used at fitting time.")> Optional newX As Object = Nothing,
            <ExcelArgument(Name:="newOffset", Description:="Optional offset vector for the new observations.")> Optional newOffset As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GlmHandle = Nothing
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
                    Dim eta As Double = ComputeLinearPredictor(expandedX, i, beta, h.InterceptIncluded, offsetVals)
                    Dim mu As Double = h.Model.pLink.inverse(eta)

                    out(r0 + i, 0) = SafeExcelNumber(mu)
                    out(r0 + i, 1) = SafeExcelNumber(eta)
                Next

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GLM_PRED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a threshold-based classification report for a fitted binomial generalized linear model.
        ''' </summary>
        ''' <param name="handle">
        ''' Handle returned by <c>BESH.REGR.GLM_FIT</c> for a previously fitted generalized linear model.
        ''' The handle must refer to a <b>binomial</b> GLM because the report is based on fitted event probabilities.
        ''' </param>
        ''' <param name="threshold">
        ''' Optional single classification cutoff in the closed interval <c>[0,1]</c>.
        ''' The default is <c>0.5</c>.
        ''' Observations with fitted probability <c>p_i ≥ threshold</c> are classified as predicted positives and
        ''' observations with <c>p_i &lt; threshold</c> are classified as predicted negatives.
        ''' </param>
        ''' <param name="includeHeader">
        ''' TRUE to include a descriptive header row in the returned table (default TRUE).
        ''' </param>
        ''' <returns>
        ''' A 4-column worksheet table containing a binary confusion matrix and selected summary measures.
        ''' The returned layout is:
        ''' <list type="bullet">
        ''' <item><description>Observed vs predicted counts for classes 0 and 1.</description></item>
        ''' <item><description>Specificity and sensitivity (reported as percentages).</description></item>
        ''' <item><description>Negative predictive value, precision, and overall accuracy (reported as percentages).</description></item>
        ''' <item><description>The chosen threshold, balanced accuracy, and Youden's J statistic.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function is intended for fitted <b>binary-response</b> GLMs such as logistic regression.
        ''' It uses the model's fitted mean responses <c>μ_i</c> as estimated event probabilities and compares them with
        ''' the observed binary outcomes <c>y_i ∈ {0,1}</c>.
        ''' </para>
        ''' <para>
        ''' For a chosen threshold <c>c</c>, the predicted class is defined by
        ''' <c>ŷ_i = 1</c> when <c>p_i ≥ c</c> and <c>ŷ_i = 0</c> otherwise. The function then computes the usual
        ''' confusion-matrix counts <c>TP</c>, <c>FP</c>, <c>TN</c>, and <c>FN</c>, from which it derives
        ''' sensitivity, specificity, precision, negative predictive value, accuracy, balanced accuracy,
        ''' and Youden's J statistic.
        ''' </para>
        ''' <para>
        ''' This is a threshold-dependent report. Changing <paramref name="threshold"/> changes the predicted classes and therefore
        ''' the reported performance measures. For a threshold sweep across many cutoffs, use <c>BESH.REGR.GLM_THRESH</c>.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <c>=BESH.REGR.GLM_CLASS(A1)</c>
        ''' or
        ''' <c>=BESH.REGR.GLM_CLASS(A1,0.35,TRUE)</c>
        ''' where <c>A1</c> contains a handle returned by <c>BESH.REGR.GLM_FIT</c>.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLM_CLASS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns a threshold-based classification report for a fitted binomial generalized linear model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLM_CLASS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLM_FIT for a fitted binomial generalized linear model.")> handle As Object,
            <ExcelArgument(Name:="threshold", Description:="Optional single classification cutoff in [0,1]. Default = 0.5.")> Optional threshold As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row in the returned table. Default = TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As GlmHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                If h Is Nothing OrElse h.Model Is Nothing Then Return ExcelError.ExcelErrorNA
                If Not TypeOf h.Model.pFamily Is regression.Binomial Then Return ExcelError.ExcelErrorValue

                Dim cutoff As Double = 0.5R
                If Not TryGetSingleThresholdFromArg(threshold, cutoff, 0.5R) Then Return ExcelError.ExcelErrorValue

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim y() As Double = h.Model.ObservedResponses
                Dim p() As Double = h.Model.PredictedResponses
                Dim w() As Double = h.Model.ObservationWeights

                Dim summary As regression.BinaryClassificationSummary =
            regression.BinaryClassificationReporting.ComputeBinarySummary(y, p, cutoff, w)

                Return UDFhelpers.BuildBinaryCrosstabOutput(summary, hdr)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GLM_CLASS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a threshold table for a fitted binomial generalized linear model.
        ''' </summary>
        ''' <param name="handle">
        ''' Handle returned by <c>BESH.REGR.GLM_FIT</c> for a fitted binomial generalized linear model.
        ''' </param>
        ''' <param name="thresholds">
        ''' Optional vector of one or more thresholds in <c>[0,1]</c> supplied as a row range, column range, or a single scalar.
        ''' If omitted, the function builds a default threshold grid from the unique fitted probabilities generated by the model.
        ''' </param>
        ''' <param name="includeHeader">
        ''' TRUE to include a header row in the returned table (default TRUE).
        ''' </param>
        ''' <returns>
        ''' A worksheet table with one row per threshold and the following columns:
        ''' threshold, TP, FP, TN, FN, sensitivity, specificity, precision, recall, NPV,
        ''' accuracy, balanced accuracy, Youden's J, and F1.
        ''' Percentage-based metrics are returned on the 0-100 scale to match the style of the existing classification UDFs.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function evaluates the fitted binomial GLM across a sequence of classification cutoffs.
        ''' It is useful for selecting or comparing decision thresholds after the model has been fitted.
        ''' </para>
        ''' <para>
        ''' When <paramref name="thresholds"/> is omitted, the function uses the sorted unique fitted probabilities as the threshold grid.
        ''' This provides an exact threshold sweep over the observed fitted values and is often more informative than using only a small fixed set of cutoffs.
        ''' </para>
        ''' <para>
        ''' The returned table complements ROC analysis. ROC summarizes discrimination across cutoffs, whereas this function gives the
        ''' concrete confusion counts and predictive values at each threshold.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <c>=BESH.REGR.GLM_THRESH(A1)</c>
        ''' or
        ''' <c>=BESH.REGR.GLM_THRESH(A1,{0.1,0.2,0.3,0.4,0.5},TRUE)</c>
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLM_THRESH",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns a threshold table for a fitted binomial generalized linear model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLM_THRESH(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLM_FIT for a fitted binomial generalized linear model.")> handle As Object,
            <ExcelArgument(Name:="thresholds", Description:="Optional scalar or row/column vector of thresholds in [0,1]. If omitted, the default threshold grid from the fitted probabilities is used.")> Optional thresholds As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row in the returned table. Default = TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As GlmHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                If h Is Nothing OrElse h.Model Is Nothing Then Return ExcelError.ExcelErrorNA
                If Not TypeOf h.Model.pFamily Is regression.Binomial Then Return ExcelError.ExcelErrorValue

                Dim thresholdVector() As Double = Nothing
                If Not UDFhelpers.TryGetOptionalThresholdVector(thresholds, thresholdVector) Then Return ExcelError.ExcelErrorValue

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim y() As Double = h.Model.ObservedResponses
                Dim p() As Double = h.Model.PredictedResponses
                Dim w() As Double = h.Model.ObservationWeights

                Dim rows As List(Of regression.BinaryThresholdRow) =
            regression.BinaryClassificationReporting.BuildThresholdTable(y, p, thresholdVector, w)

                Return UDFhelpers.BuildThresholdTableOutput(rows, hdr)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GLM_THRESH", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns calibration-plot data for a fitted binomial generalized linear model.
        ''' </summary>
        ''' <param name="handle">
        ''' Handle returned by <c>BESH.REGR.GLM_FIT</c> for a fitted binomial generalized linear model.
        ''' </param>
        ''' <param name="bins">
        ''' Optional positive integer giving the number of calibration bins.
        ''' The default is 10. The current implementation requires at least 2 bins.
        ''' </param>
        ''' <param name="method">
        ''' Optional calibration binning method.
        ''' Accepted values are <c>"quantile"</c> (default) and <c>"equalwidth"</c>.
        ''' Quantile binning creates groups with approximately equal numbers of observations, while equal-width binning
        ''' partitions the probability scale into equal intervals.
        ''' </param>
        ''' <param name="includeHeader">
        ''' TRUE to include a header row in the returned table (default TRUE).
        ''' </param>
        ''' <returns>
        ''' A worksheet table with one row per calibration bin and the columns:
        ''' bin index, number of observations, mean predicted probability, observed event rate,
        ''' and lower/upper confidence limits for the observed event rate.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function is designed to support calibration plots for fitted binary GLMs.
        ''' A well-calibrated model should produce bins in which the observed event rate is close to the mean predicted probability.
        ''' </para>
        ''' <para>
        ''' The returned table can be plotted directly in Excel by using <c>MeanPredicted</c> on the x-axis and <c>ObservedRate</c> on the y-axis,
        ''' optionally adding the confidence interval columns as error bars.
        ''' </para>
        ''' <para>
        ''' Calibration describes how well predicted probabilities agree with empirical event frequencies.
        ''' It is different from discrimination measures such as AUC or from threshold-based summaries such as sensitivity and specificity.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <c>=BESH.REGR.GLM_CALIB(A1)</c>
        ''' or
        ''' <c>=BESH.REGR.GLM_CALIB(A1,10,"quantile",TRUE)</c>
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLM_CALIB",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns calibration-plot data for a fitted binomial generalized linear model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLM_CALIB(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLM_FIT for a fitted binomial generalized linear model.")> handle As Object,
            <ExcelArgument(Name:="bins", Description:="Optional positive integer specifying the number of calibration bins. Default = 10.")> Optional bins As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional calibration binning method: 'quantile' (default) or 'equalwidth'.")> Optional method As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row in the returned table. Default = TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As GlmHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                If h Is Nothing OrElse h.Model Is Nothing Then Return ExcelError.ExcelErrorNA
                If Not TypeOf h.Model.pFamily Is regression.Binomial Then Return ExcelError.ExcelErrorValue

                Dim binCount As Integer = 10
                If Not UDFhelpers.TryGetOptionalPositiveInteger(bins, binCount, 10, 2) Then Return ExcelError.ExcelErrorValue

                Dim methodName As String = UDFhelpers.ParseCalibrationMethod(method, "quantile")
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim y() As Double = h.Model.ObservedResponses
                Dim p() As Double = h.Model.PredictedResponses
                Dim w() As Double = h.Model.ObservationWeights

                Dim rows As List(Of regression.CalibrationBinSummary) =
            regression.BinaryClassificationReporting.BuildCalibrationBins(y, p, binCount, w, methodName)

                Return UDFhelpers.BuildCalibrationTableOutput(rows, hdr)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GLM_CALIB", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the Brier score for a fitted binomial generalized linear model.
        ''' </summary>
        ''' <param name="handle">
        ''' Handle returned by <c>BESH.REGR.GLM_FIT</c> for a fitted binomial generalized linear model.
        ''' </param>
        ''' <param name="includeHeader">
        ''' TRUE to include a header row in the returned table (default TRUE).
        ''' </param>
        ''' <returns>
        ''' A small worksheet table containing the Brier score and, for convenience, the sample size and event rate.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' For a binary outcome with observed values <c>y_i ∈ {0,1}</c> and fitted probabilities <c>p_i</c>, the Brier score is
        ''' <c>(1/n) Σ (y_i - p_i)^2</c> in the unweighted case, or the corresponding weighted mean when case weights were supplied.
        ''' Lower values indicate more accurate probabilistic predictions.
        ''' </para>
        ''' <para>
        ''' Unlike threshold-based metrics, the Brier score evaluates the full probability forecast and therefore does not depend on a chosen classification cutoff.
        ''' It combines aspects of calibration and discrimination into a single proper scoring rule.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <c>=BESH.REGR.GLM_BRIER(A1)</c>
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLM_BRIER",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the Brier score for a fitted binomial generalized linear model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLM_BRIER(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLM_FIT for a fitted binomial generalized linear model.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row in the returned table. Default = TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As GlmHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                If h Is Nothing OrElse h.Model Is Nothing Then Return ExcelError.ExcelErrorNA
                If Not TypeOf h.Model.pFamily Is regression.Binomial Then Return ExcelError.ExcelErrorValue

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim y() As Double = h.Model.ObservedResponses
                Dim p() As Double = h.Model.PredictedResponses
                Dim w() As Double = h.Model.ObservationWeights

                Dim score As Double = regression.BinaryClassificationReporting.ComputeBrierScore(y, p, w)
                Dim eventRate As Double = ComputeBinaryEventRate(y, w)

                Return UDFhelpers.BuildNamedScalarOutput("BrierScore", score, y.Length, eventRate, hdr)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GLM_BRIER", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a fitted generalized linear model handle from the in-memory cache.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GLM_FIT</c>.</param>
        ''' <returns>
        ''' TRUE if the handle was found and removed; otherwise FALSE.
        ''' </returns>
        ''' <remarks>
        ''' Handles persist only for the current Excel session and reference fitted models stored in memory.
        ''' This function explicitly releases one cached model so that long workbooks or repeated refits do not keep unnecessary objects alive.
        ''' Existing worksheet formulas that still reference a dropped handle will subsequently return a handle-not-found error until the model is refitted.
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLM_DROP",
            Category:="BESHStatNG - Regression Models",
            Description:="Removes a fitted generalized linear model handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLM_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLM_FIT.")> handle As Object
        ) As Object
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return ExcelError.ExcelErrorValue
            Dim removed As GlmHandle = Nothing
            Return _glmCache.TryRemove(key, removed)
        End Function

        ' ---------------------------------------------------------------------
        ' Helpers
        ' ---------------------------------------------------------------------
        Private Function TryGetHandle(handle As Object, ByRef h As GlmHandle) As Boolean
            h = Nothing
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _glmCache.TryGetValue(key, h)
        End Function

        ''' <summary>
        ''' Parses an optional single threshold argument for binary classification UDFs.
        ''' </summary>
        ''' <param name="arg">
        ''' Optional scalar Excel argument expected to represent a single probability cutoff in <c>[0,1]</c>.
        ''' Blank values use <paramref name="defaultValue"/>.
        ''' </param>
        ''' <param name="threshold">
        ''' On success, receives the parsed threshold.
        ''' </param>
        ''' <param name="defaultValue">
        ''' Default threshold returned when <paramref name="arg"/> is missing or blank.
        ''' </param>
        ''' <returns>
        ''' TRUE if parsing succeeds; otherwise FALSE.
        ''' </returns>
        Friend Function TryGetSingleThresholdFromArg(arg As Object, ByRef threshold As Double,
                                              Optional defaultValue As Double = 0.5R) As Boolean
            threshold = defaultValue
            Dim thresholds() As Double = Nothing
            If Not UDFhelpers.TryGetOptionalThresholdVector(arg, thresholds) Then Return False
            If thresholds Is Nothing OrElse thresholds.Length = 0 Then Return True
            If thresholds.Length <> 1 Then Return False

            threshold = thresholds(0)
            Return True
        End Function

        ''' <summary>
        ''' Computes the event rate for a binary outcome vector.
        ''' </summary>
        ''' <param name="y">Observed binary outcomes coded as 0/1.</param>
        ''' <param name="weights">Optional nonnegative observation weights.</param>
        ''' <returns>
        ''' The unweighted or weighted mean of <paramref name="y"/>. Returns <see cref="Double.NaN"/> when no valid denominator is available.
        ''' </returns>
        Friend Function ComputeBinaryEventRate(y() As Double, Optional weights() As Double = Nothing) As Double
            If y Is Nothing OrElse y.Length = 0 Then Return Double.NaN

            If weights Is Nothing OrElse weights.Length <> y.Length Then
                Dim sumY As Double = 0.0
                For i As Integer = 0 To y.Length - 1
                    sumY += y(i)
                Next
                Return sumY / y.Length
            End If

            Dim sumW As Double = 0.0
            Dim sumWY As Double = 0.0
            For i As Integer = 0 To y.Length - 1
                Dim wi As Double = weights(i)
                If wi <= 0.0 Then Continue For
                sumW += wi
                sumWY += wi * y(i)
            Next

            If sumW <= 0.0 Then Return Double.NaN
            Return sumWY / sumW
        End Function

        Private Function BuildParameterNames(h As GlmHandle, coefficientCount As Integer) As String()
            Dim out(coefficientCount - 1) As String
            Dim nextIndex As Integer = 0

            If h.InterceptIncluded AndAlso coefficientCount > 0 Then
                out(0) = "Intercept"
                nextIndex = 1
            End If

            Dim predictorNames() As String = If(h.VarNames, New String() {})
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

        Friend Function ParseGlmResidualType(v As Object) As String
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
                Case "stdpearson", "standardizedpearson"
                    Return "stdpearson"
                Case "stddeviance", "standardizeddeviance"
                    Return "stddeviance"
                Case "leverage", "hat"
                    Return "leverage"
                Case "cook", "cooksdistance", "cookdistance"
                    Return "cook"
                Case Else
                    Return "all"
            End Select
        End Function

        Friend Function BuildAllResidualOutput(model As GLM, includeHeader As Boolean) As Object
            If model Is Nothing Then Return ExcelError.ExcelErrorNA
            If model.pRaw_res Is Nothing OrElse model.pDeviance_res Is Nothing OrElse model.pPearsChisq_res Is Nothing OrElse
               model.pLeverage Is Nothing OrElse model.pStDeviance_res Is Nothing OrElse model.pStPearsChisq_res Is Nothing OrElse
               model.pCookDistance Is Nothing Then
                Return ExcelError.ExcelErrorNA
            End If

            Dim n As Integer = model.pRaw_res.Length
            Dim outRows As Integer = If(includeHeader, n + 1, n)
            Dim out(outRows - 1, 6) As Object
            Dim r0 As Integer = 0

            If includeHeader Then
                out(0, 0) = "RawResidual"
                out(0, 1) = "DevianceResidual"
                out(0, 2) = "PearsonResidual"
                out(0, 3) = "Leverage"
                out(0, 4) = "StdDevianceResidual"
                out(0, 5) = "StdPearsonResidual"
                out(0, 6) = "CookDistance"
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                out(r0 + i, 0) = model.pRaw_res(i)
                out(r0 + i, 1) = model.pDeviance_res(i)
                out(r0 + i, 2) = model.pPearsChisq_res(i)
                out(r0 + i, 3) = model.pLeverage(i)
                out(r0 + i, 4) = model.pStDeviance_res(i)
                out(r0 + i, 5) = model.pStPearsChisq_res(i)
                out(r0 + i, 6) = model.pCookDistance(i)
            Next

            Return out
        End Function

    End Module

End Namespace
