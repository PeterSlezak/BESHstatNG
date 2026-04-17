Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for the overdispersion-estimating Negative Binomial generalized linear model implemented by <see cref="GLM_NB"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These worksheet functions expose a handle-based Excel interface to the NB2 Negative Binomial regression model for count data.
    ''' The conditional mean is linked to predictors through a linear predictor
    ''' <c>η_i = β_0 + x_i'β + o_i</c>, where <c>o_i</c> is an optional offset, and the response mean satisfies
    ''' <c>μ_i = g^{-1}(η_i)</c>.
    ''' </para>
    ''' <para>
    ''' The fitted count response is assumed to follow the NB2 mean-variance relationship
    ''' <c>E[Y_i | x_i] = μ_i</c> and
    ''' <c>Var(Y_i | x_i) = μ_i + α μ_i^2</c>,
    ''' where <c>α &gt; 0</c> is the overdispersion parameter. Equivalently, with <c>θ = 1/α</c>,
    ''' <c>Var(Y_i | x_i) = μ_i + μ_i^2 / θ</c>.
    ''' </para>
    ''' <para>
    ''' Unlike the fixed-dispersion Negative Binomial family exposed through the general GLM worksheet functions,
    ''' this module uses <see cref="GLM_NB"/>, which estimates the dispersion parameter from the data by alternating between:
    ''' fitting the mean model for a fixed dispersion value and re-estimating dispersion from the current fitted means.
    ''' That design mirrors the <c>glm.nb</c>-style outer loop documented in <see cref="GLM_NB.Fit(Integer, Boolean, System.Windows.Forms.ProgressBar, System.Windows.Forms.Label)"/>.
    ''' </para>
    ''' <para>
    ''' The fit function stores the estimated model in an in-memory cache for the current Excel session and returns a text handle.
    ''' The remaining worksheet functions reuse that handle to return coefficient tables, model diagnostics, residuals, predictions,
    ''' and explicit cache cleanup without refitting the model.
    ''' </para>
    ''' <para>
    ''' Predictor formulas reuse the same regression-formula infrastructure used by the other model UDF modules.
    ''' This allows additive terms, polynomial terms, continuous interactions, and categorical main effects to be defined from the raw predictor matrix.
    ''' </para>
    ''' </remarks>
    Public Module GLMNbUDFs

        Private ReadOnly _glmNbCache As New ConcurrentDictionary(Of String, GlmNbHandle)(StringComparer.OrdinalIgnoreCase)

        Private Class GlmNbHandle
            Public Property Handle As String
            Public Property Model As GLM_NB
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
            Public Property LinkName As String
            Public Property ConfidenceAlpha As Double
        End Class

        ''' <summary>
        ''' Fits a Negative Binomial regression model with estimated overdispersion and returns a reusable handle.
        ''' </summary>
        ''' <param name="y">
        ''' Numeric response vector (single column) containing nonnegative count outcomes.
        ''' Each row corresponds to one observation.
        ''' </param>
        ''' <param name="x">
        ''' Raw predictor matrix with one row per observation and one column per raw predictor.
        ''' The rows must align with <paramref name="y"/>, <paramref name="offset"/>, and <paramref name="weights"/> when supplied.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional raw predictor names supplied either as a comma-separated string or as a one-row/one-column range.
        ''' If omitted, fallback names such as X1, X2, … are assigned internally.
        ''' </param>
        ''' <param name="link">
        ''' Optional link function for the mean model.
        ''' The default is <c>log</c>, which yields <c>μ_i = exp(η_i)</c>.
        ''' Accepted values follow the underlying Negative Binomial family implementation and include <c>log</c>, <c>identity</c>, and <c>power</c>.
        ''' </param>
        ''' <param name="offset">
        ''' Optional numeric offset vector (single column).
        ''' The offset enters additively on the link scale:
        ''' <c>η_i = β_0 + x_i'β + o_i</c>.
        ''' For rate models with exposure <c>t_i</c>, a common choice under the log link is <c>o_i = log(t_i)</c>.
        ''' </param>
        ''' <param name="weights">
        ''' Optional nonnegative case weights (single column).
        ''' These weights are passed into the fitting engine and act multiplicatively in the IRLS working weights and in the dispersion update objective.
        ''' </param>
        ''' <param name="includeIntercept">
        ''' TRUE to include an intercept term (default TRUE).
        ''' When FALSE, the fitted linear predictor omits <c>β_0</c>.
        ''' </param>
        ''' <param name="formula">
        ''' Optional right-hand-side formula used to expand the raw predictor matrix before fitting.
        ''' If omitted or blank, all raw predictor columns are included as continuous main effects.
        ''' </param>
        ''' <param name="formulaAddressing">
        ''' Formula-addressing mode: <c>relative</c> (default), <c>absolute</c>, or <c>names</c>.
        ''' This controls how bare column tokens are interpreted inside <paramref name="formula"/>.
        ''' </param>
        ''' <param name="power">
        ''' Optional power parameter used only when <paramref name="link"/> is <c>power</c>.
        ''' If the power link is selected, this parameter is required and must be finite and nonzero.
        ''' </param>
        ''' <param name="maxIter">
        ''' Maximum number of fitting iterations for the outer alternating optimization procedure (default 20).
        ''' </param>
        ''' <param name="tol">
        ''' Positive convergence tolerance for the alternating optimization procedure (default 1E-8).
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level used for confidence intervals stored in the fitted result object (default 0.05).
        ''' This parameter does not control the Negative Binomial dispersion; it controls reporting intervals only.
        ''' </param>
        ''' <returns>
        ''' A text handle identifying the fitted Negative Binomial model within the current Excel session.
        ''' The handle can be passed to the other <c>GLMNB_*</c> worksheet functions to retrieve summaries, tests, residuals, and predictions without refitting.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function fits the NB2 Negative Binomial regression model
        ''' <c>Y_i | x_i ~ NB(μ_i, α)</c>
        ''' with
        ''' <c>E[Y_i | x_i] = μ_i</c>
        ''' and
        ''' <c>Var(Y_i | x_i) = μ_i + α μ_i^2</c>.
        ''' Under the default log link,
        ''' <c>log(μ_i) = β_0 + x_i'β + o_i</c>,
        ''' so exponentiated coefficients represent multiplicative effects on the conditional mean count.
        ''' </para>
        ''' <para>
        ''' The underlying <see cref="GLM_NB"/> implementation uses an alternating procedure:
        ''' </para>
        ''' <list type="number">
        ''' <item><description>Fit an initial Poisson GLM to obtain starting mean-model coefficients and fitted means.</description></item>
        ''' <item><description>Estimate an initial overdispersion value.</description></item>
        ''' <item><description>Repeatedly refit the Negative Binomial mean model for the current dispersion and then update the dispersion from the current fitted means.</description></item>
        ''' </list>
        ''' <para>
        ''' Internally the model reports overdispersion in the NB2 parameterization <c>α</c>, while some software instead reports
        ''' <c>θ = 1/α</c>. Both quantities are made available through the test/diagnostic output returned by <c>BESH.REGR.GLMNB_TESTS</c>.
        ''' </para>
        ''' <para>
        ''' Rows with invalid values in the response, predictors, offset, or weights are excluded before fitting.
        ''' If too few valid rows remain, or if the resulting design has no estimable parameters, the function returns an Excel error.
        ''' </para>
        ''' <para>
        ''' If <c>formulaAddressing="absolute"</c> is used, the <paramref name="x"/> argument should be supplied as a direct worksheet range so that absolute worksheet column letters can be resolved.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.GLMNB_FIT(A2:A101,B2:D101,"Age,BMI,Treat")
        ''' =BESH.REGR.GLMNB_FIT(A2:A101,B2:E101,"Dose,Age,Stage,Center","log",F2:F101,,TRUE,"A + B + factor(D)","relative")
        ''' =BESH.REGR.GLMNB_FIT(A2:A101,B2:C101,"X1,X2","power",,,,TRUE,,,0.5)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.GLMNB_FIT",
            Category:="BESHStatNG - Regression Models",
            Description:="Fits a Negative Binomial regression model with estimated overdispersion and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLMNB_FIT(
            <ExcelArgument(Name:="y", Description:="Numeric response vector (single column) of nonnegative count outcomes.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Raw predictor matrix with one row per observation.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional raw predictor names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="link", Description:="Optional link function: ""log"" (default), ""identity"", or ""power"" where supported by the family.")> Optional link As Object = Nothing,
            <ExcelArgument(Name:="offset", Description:="Optional numeric offset vector (single column).")> Optional offset As Object = Nothing,
            <ExcelArgument(Name:="weights", Description:="Optional nonnegative case weights (single column).")> Optional weights As Object = Nothing,
            <ExcelArgument(Name:="includeIntercept", Description:="TRUE to include an intercept term (default TRUE).")> Optional includeIntercept As Object = Nothing,
            <ExcelArgument(Name:="formula", Description:="Optional RHS formula used to expand the raw predictor matrix.")> Optional formula As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Formula-addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing,
            <ExcelArgument(Name:="power", Description:="Optional power parameter used only when link=""power"".")> Optional power As Object = Nothing,
            <ExcelArgument(Name:="maxIter", Description:="Maximum number of fitting iterations (default 20).")> Optional maxIter As Object = Nothing,
            <ExcelArgument(Name:="tol", Description:="Convergence tolerance (default 1E-8).")> Optional tol As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha used internally for confidence intervals (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "GLMNB_FIT (editing...)"

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

                If fitData Is Nothing OrElse fitVarNames Is Nothing OrElse fitVarNames.Length < 1 Then
                    Return ExcelError.ExcelErrorValue
                End If
                If Not UDFhelpers.HasOnlyFinite(fitOffset) Then Return ExcelError.ExcelErrorValue
                If Not UDFhelpers.HasOnlyFinite(fitWeights, True) Then Return ExcelError.ExcelErrorValue

                Dim interceptFlag As Boolean = UDFhelpers.GetOptionalBool(includeIntercept, True)
                If Not interceptFlag AndAlso fitVarNames.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim ciAlpha As Double = 0.05R
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, ciAlpha) Then Return ExcelError.ExcelErrorNum
                End If

                Dim maxIterValue As Integer = UDFhelpers.GetOptionalInt(maxIter, 20)
                Dim tolValue As Double = UDFhelpers.GetOptionalDouble(tol, 0.00000001R)
                If maxIterValue < 1 Then Return ExcelError.ExcelErrorNum
                If Double.IsNaN(tolValue) OrElse Double.IsInfinity(tolValue) OrElse tolValue <= 0.0R Then Return ExcelError.ExcelErrorNum

                Dim linkName As String = ParseLinkName(link, "Negative Binomial")
                If String.IsNullOrWhiteSpace(linkName) Then Return ExcelError.ExcelErrorValue

                Dim family As New regression.NegativeBinomial()
                If Not family.testLink(linkName) Then Return ExcelError.ExcelErrorValue

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

                Dim mdl As New GLM_NB(lnk)
                mdl.bComputeResiduals = False
                mdl.bReturnCov = False
                mdl.bIterationDetails = False
                mdl.settingInputs(ciAlpha, maxIterValue, tolValue)
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

                Dim handleKey As String = "GLMNB:" & Guid.NewGuid().ToString("N")
                Dim h As New GlmNbHandle With {
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
                    .LinkName = lnk.ToString(),
                    .ConfidenceAlpha = ciAlpha
                }

                _glmNbCache(handleKey) = h
                Return handleKey

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.GLMNB_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the coefficient summary table for a fitted Negative Binomial regression handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GLMNB_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used to construct the displayed Wald confidence intervals.
        ''' This argument controls only interval reporting and does not refit the model.
        ''' </param>
        ''' <returns>
        ''' A rectangular coefficient table containing parameter labels, standard errors, Wald z statistics, p-values, and confidence limits.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The coefficient table is built from the fitted mean-model parameters <c>β</c> and their standard errors.
        ''' For coefficient <c>β_j</c> with standard error <c>SE(β_j)</c>, the worksheet output reports the Wald statistic
        ''' <c>z_j = β_j / SE(β_j)</c> and the two-sided p-value
        ''' <c>2 Φ(-|z_j|)</c>, where <c>Φ</c> is the standard normal CDF.
        ''' </para>
        ''' <para>
        ''' A <c>(1-α)</c> Wald confidence interval is displayed as
        ''' <c>β_j ± z_{1-α/2} SE(β_j)</c>.
        ''' Under the log link, exponentiating a slope coefficient yields the estimated multiplicative change in the conditional mean count for a one-unit increase in the predictor, holding other predictors fixed.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLMNB_SUMMARY",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the coefficient summary table for a fitted Negative Binomial regression handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLMNB_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLMNB_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for the displayed confidence intervals.")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As GlmNbHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim ciAlpha As Double = h.ConfidenceAlpha
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, ciAlpha) Then Return ExcelError.ExcelErrorNum
                End If

                Dim beta() As Double = h.Model.results.Coeffs_est
                Dim se() As Double = h.Model.results.Coeffs_SEs
                If beta Is Nothing OrElse se Is Nothing Then Return ExcelError.ExcelErrorNA
                If beta.Length <> se.Length Then Return ExcelError.ExcelErrorNA

                Dim names() As String = BuildParameterNames(h, beta.Length)
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, beta.Length + 1, beta.Length)
                Dim out(outRows - 1, 7) As Object
                Dim zCrit As Double = distributions.ZCritTwoSided(ciAlpha)
                Dim ciLabel As String = (100.0R * (1.0R - ciAlpha)).ToString("0.##", CultureInfo.InvariantCulture) & "%"
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
                Return LoggedUdfExceptionText("BESH.REGR.GLMNB_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns model-level diagnostics and fit statistics for a fitted Negative Binomial regression handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GLMNB_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A rectangular table containing family/link information, deviance diagnostics, information criteria, convergence information,
        ''' the estimated NB2 dispersion parameter <c>α</c>, its reciprocal <c>θ = 1/α</c>, and computation metadata.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The returned table is based primarily on the model summary table produced by <see cref="GLM_NB"/> after fitting.
        ''' In addition, this worksheet function explicitly reports the NB2 overdispersion estimate <c>α</c> and the reciprocal form
        ''' <c>θ = 1/α</c>, because different software packages report one or the other.
        ''' </para>
        ''' <para>
        ''' The NB2 variance function is
        ''' <c>V(μ) = μ + α μ^2</c>.
        ''' When <c>α</c> is close to 0, the model approaches a Poisson mean-variance relationship.
        ''' Larger values of <c>α</c> indicate stronger overdispersion relative to Poisson.
        ''' </para>
        ''' <para>
        ''' Information-criterion rows already account for the estimated dispersion parameter inside the underlying <see cref="GLM_NB"/> implementation.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLMNB_TESTS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns model-level diagnostics and fit statistics for a fitted Negative Binomial regression handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLMNB_TESTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLMNB_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GlmNbHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim labels() As String = h.Model.results.ModelTableLabels
                Dim vals(,) As Object = h.Model.results.ModelTableVals
                If labels Is Nothing OrElse vals Is Nothing Then Return ExcelError.ExcelErrorNA
                If vals.GetLength(0) <> labels.Length Then Return ExcelError.ExcelErrorNA

                Dim extraRows As Integer = 3
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

                Dim nbAlpha As Double = h.Model.NBalpha
                out(r, 0) = "NB2 dispersion alpha"
                out(r, 1) = nbAlpha
                out(r, 2) = ""
                out(r, 3) = ""
                r += 1

                out(r, 0) = "Theta = 1/alpha"
                out(r, 1) = If(nbAlpha > 0.0R, CType(1.0R / nbAlpha, Object), CType("Inf", Object))
                out(r, 2) = ""
                out(r, 3) = ""
                r += 1

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
                Return LoggedUdfExceptionText("BESH.REGR.GLMNB_TESTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns residual diagnostics for a fitted Negative Binomial regression handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GLMNB_FIT</c>.</param>
        ''' <param name="residType">
        ''' Residual block to return: <c>all</c> (default), <c>raw</c>, <c>deviance</c>, <c>pearson</c>,
        ''' <c>stdpearson</c>, <c>stddeviance</c>, <c>leverage</c>, or <c>cook</c>.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>A residual matrix or vector, depending on <paramref name="residType"/>.</returns>
        ''' <remarks>
        ''' <para>
        ''' Residual diagnostics are based on the fitted Negative Binomial mean model and include several commonly used quantities.
        ''' The raw residual is <c>y_i - μ_i</c>.
        ''' The Pearson residual rescales that difference by the model-implied standard deviation,
        ''' while the deviance residual is based on the signed square root of the per-observation deviance contribution.
        ''' </para>
        ''' <para>
        ''' Standardized residuals adjust for leverage, and the leverage/Cook's-distance columns are useful for influence screening.
        ''' The <c>all</c> option returns the full seven-column block used by the internal GLM diagnostics.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLMNB_RESID",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns residual diagnostics for a fitted Negative Binomial regression handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLMNB_RESID(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLMNB_FIT.")> handle As Object,
            <ExcelArgument(Name:="residType", Description:="Residual block: ""all"" (default), ""raw"", ""deviance"", ""pearson"", ""stdpearson"", ""stddeviance"", ""leverage"", or ""cook"".")> Optional residType As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GlmNbHandle = Nothing
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
                Return LoggedUdfExceptionText("BESH.REGR.GLMNB_RESID", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns predicted means and linear predictors for new data under a fitted Negative Binomial regression model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GLMNB_FIT</c>.</param>
        ''' <param name="newX">
        ''' New raw predictor matrix in the same raw-column order used at fitting time.
        ''' If the fitted model used a formula, the same stored formula design is reapplied to this raw matrix.
        ''' </param>
        ''' <param name="newOffset">
        ''' Optional offset vector for the new observations.
        ''' If the fitted model included an offset, this argument is required and is added on the link scale.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A two-column table containing the predicted conditional mean response and the corresponding linear predictor.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' For each new row, the worksheet function reconstructs the fitted design columns, evaluates the linear predictor
        ''' <c>η_new = β_0 + x_new'β + o_new</c>, and then returns the mean prediction
        ''' <c>μ_new = g^{-1}(η_new)</c>.
        ''' </para>
        ''' <para>
        ''' Under the default log link, the output therefore satisfies
        ''' <c>μ_new = exp(η_new)</c>,
        ''' which is the fitted conditional mean count for the supplied covariate pattern and offset.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLMNB_PRED",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns predicted means and linear predictors for new data under a fitted Negative Binomial regression model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLMNB_PRED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLMNB_FIT.")> handle As Object,
            <ExcelArgument(AllowReference:=True, Name:="newX", Description:="New raw predictor matrix in the same raw-column order used at fitting time.")> Optional newX As Object = Nothing,
            <ExcelArgument(Name:="newOffset", Description:="Optional offset vector for the new observations.")> Optional newOffset As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As GlmNbHandle = Nothing
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
                Return LoggedUdfExceptionText("BESH.REGR.GLMNB_PRED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a fitted Negative Binomial regression handle from the in-memory cache.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.GLMNB_FIT</c>.</param>
        ''' <returns>
        ''' TRUE when the handle was found and removed; otherwise FALSE.
        ''' </returns>
        ''' <remarks>
        ''' Handles are session-scoped identifiers for cached fitted models.
        ''' Removing a handle frees the corresponding in-memory model object for the current Excel session and invalidates subsequent lookups using that handle.
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.REGR.GLMNB_DROP",
            Category:="BESHStatNG - Regression Models",
            Description:="Removes a fitted Negative Binomial regression handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function GLMNB_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.GLMNB_FIT.")> handle As Object
        ) As Object
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return ExcelError.ExcelErrorValue
            Dim removed As GlmNbHandle = Nothing
            Return _glmNbCache.TryRemove(key, removed)
        End Function

        Private Function TryGetHandle(handle As Object, ByRef h As GlmNbHandle) As Boolean
            h = Nothing
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _glmNbCache.TryGetValue(key, h)
        End Function

        Private Function BuildParameterNames(h As GlmNbHandle, coefficientCount As Integer) As String()
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

    End Module

End Namespace
