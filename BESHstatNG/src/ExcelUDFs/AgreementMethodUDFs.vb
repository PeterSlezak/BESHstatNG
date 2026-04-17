Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.equivalencetests
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Excel-DNA worksheet functions for agreement and method-comparison procedures.
    ''' </summary>
    Public Module AgreementMethodUDFs

        ''' <summary>
        ''' Returns a spillable labeled result table for Passing–Bablok method-comparison regression.
        ''' </summary>
        ''' <param name="x">
        ''' One-column range containing the reference-method measurements. The values must be paired row-by-row with <paramref name="y"/>.
        ''' If the first cell looks like text, it is treated as a header rather than data.
        ''' </param>
        ''' <param name="y">
        ''' One-column range containing the test-method measurements. The values must be paired row-by-row with <paramref name="x"/>.
        ''' If the first cell looks like text, it is treated as a header rather than data.
        ''' </param>
        ''' <param name="groups">
        ''' Optional one-column grouping or subject range aligned row-by-row with <paramref name="x"/> and <paramref name="y"/>.
        ''' When supplied, the function performs grouped / block Passing–Bablok regression so that paired observations can be handled within subject or block structure.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for confidence intervals. The default is 0.05, corresponding to 95% confidence intervals.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional display names for the two methods. Supply either a comma-separated string such as <c>"Reference,Test"</c>
        ''' or a one-row / one-column range with two names.
        ''' </param>
        ''' <param name="groupName">
        ''' Optional display name for the grouping variable shown in the output when <paramref name="groups"/> is supplied.
        ''' </param>
        ''' <returns>
        ''' A two-dimensional spill range containing a labeled result table with the fitted slope and intercept, interval estimates,
        ''' and additional method-comparison diagnostics.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Passing–Bablok regression is a robust, non-parametric linear method-comparison procedure for paired measurements.
        ''' The fitted line has the form <c>y = a + b x</c>.
        ''' </para>
        ''' <para>
        ''' The slope is estimated from the median of all admissible pairwise slopes
        ''' <c>(y_j - y_i) / (x_j - x_i)</c> with <c>x_j &lt;&gt; x_i</c>, and the intercept is estimated as the median of
        ''' <c>y_i - b x_i</c>. This makes the method resistant to moderate outliers and removes the need to assume
        ''' a normal distribution of residuals.
        ''' </para>
        ''' <para>
        ''' Typical assumptions are:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description><paramref name="x"/> and <paramref name="y"/> measure the same items or subjects in the same row order.</description></item>
        ''' <item><description>The relation is approximately linear and monotone over the observed range.</description></item>
        ''' <item><description>Pairs are independent unless an explicit grouped / block analysis is requested through <paramref name="groups"/>.</description></item>
        ''' <item><description>Missing or non-numeric rows are removed pairwise; if <paramref name="groups"/> is supplied, a retained numeric pair must also have a non-empty group label.</description></item>
        ''' </list>
        ''' <para>
        ''' Use this function when you want the full formatted worksheet report. Use <c>BESH.AGREE.PASSINGBABLOK_COEF</c>
        ''' when you only want the fitted coefficients and their interval estimates.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.PASSINGBABLOK_FIT",
            Category:="BESHStatNG - Agreement",
            Description:="Passing–Bablok regression for two paired methods. Returns a labeled result table.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function PASSINGBABLOK_FIT(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range. First cell may be a header.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range. First cell may be a header.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="groups", Description:="Optional grouping / subject range for grouped Block–Passing–Bablok. Must align row-wise with x and y.")> Optional groups As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="varNames", Description:="Optional names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="groupName", Description:="Optional display name for the grouping variable.")> Optional groupName As Object = Nothing) As Object
            Try
                Dim input = ReadAlignedNumericWithOptionalCategory(x, y, groups, requireCategory:=False)
                If input.Error.HasValue Then Return input.Error.Value
                If input.X Is Nothing OrElse input.X.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ParametricUDFs.ResolveNames(varNames, input.DetectedNames, 2, "Method")
                Dim alphaValue As Double = 0.05
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim grp() As Object = input.Category
                Dim grpVarName As String = If(UDFhelpers.IsMissingArg(groupName), "Group", Convert.ToString(groupName).Trim())

                Dim mdl As New Agreement.PassinbBablok(input.X, input.Y, names(0), names(1), grp, grpVarName)
                mdl.alpha = alphaValue
                If grp Is Nothing Then
                    mdl.PassingBablokCI()
                Else
                    mdl.GroupedBlockPassingBablok()
                End If
                Return StackResultTables(mdl.wrapResults())
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.PASSINGBABLOK_FIT", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns the Passing–Bablok slope and intercept together with their confidence intervals.
        ''' </summary>
        ''' <param name="x">One-column range of reference-method values, paired row-by-row with <paramref name="y"/>.</param>
        ''' <param name="y">One-column range of test-method values, paired row-by-row with <paramref name="x"/>.</param>
        ''' <param name="groups">Optional one-column grouping / subject range aligned with <paramref name="x"/> and <paramref name="y"/> for grouped / block Passing–Bablok analysis.</param>
        ''' <param name="alpha">Optional two-sided significance level used for confidence intervals. Default 0.05.</param>
        ''' <returns>
        ''' A compact spill range containing slope and intercept estimates with lower and upper confidence limits.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Passing–Bablok regression fits the robust method-comparison line <c>y = a + b x</c> by using medians of pairwise slopes
        ''' rather than least squares. It is designed for paired measurements where both methods may contain measurement error.
        ''' </para>
        ''' <para>
        ''' The reported slope describes proportional differences between methods; the reported intercept describes systematic offset.
        ''' Confidence intervals are distribution-free in the sense that they do not rely on normal residual assumptions.
        ''' </para>
        ''' <para>
        ''' Missing or non-numeric rows are removed pairwise. If <paramref name="groups"/> is supplied, the group range must align row-by-row
        ''' with the paired numeric data.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.PASSINGBABLOK_COEF",
            Category:="BESHStatNG - Agreement",
            Description:="Passing–Bablok regression coefficients and confidence intervals for two paired methods.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function PASSINGBABLOK_COEF(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="groups", Description:="Optional grouping / subject range for grouped Block–Passing–Bablok.")> Optional groups As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing) As Object
            Try
                Dim input = ReadAlignedNumericWithOptionalCategory(x, y, groups, requireCategory:=False)
                If input.Error.HasValue Then Return input.Error.Value
                If input.X Is Nothing OrElse input.X.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim alphaValue As Double = 0.05
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim grp() As Object = input.Category
                Dim mdl As New Agreement.PassinbBablok(input.X, input.Y, "Reference", "Test", grp, "Group")
                mdl.alpha = alphaValue
                Dim fit As (InterceptCI As ConfidenceIntervalResult, SlopeCI As ConfidenceIntervalResult)
                If grp Is Nothing Then
                    fit = mdl.PassingBablokCI()
                Else
                    fit = mdl.GroupedBlockPassingBablok()
                End If

                Dim t As New ResultTable
                t.AddTitle("Passing–Bablok Coefficients")
                t.AddHeaderLeftRow({"Slope", "Intercept"})
                t.AddHeaderTopRow({"Estimate", fit.SlopeCI.CIlabel, "Lower", "Upper"})
                t.SetBody(New Object(,) {
                    {fit.SlopeCI.Estimate, fit.SlopeCI.strConfidenceInterval(CIformat.LL_to_UL), fit.SlopeCI.LowerLimit, fit.SlopeCI.UpperLimit},
                    {fit.InterceptCI.Estimate, fit.InterceptCI.strConfidenceInterval(CIformat.LL_to_UL), fit.InterceptCI.LowerLimit, fit.InterceptCI.UpperLimit}
                })
                Return PrepareResultTableForUdf(t.returnSelf())
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.PASSINGBABLOK_COEF", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns a spillable labeled result table for Deming or weighted Deming regression.
        ''' </summary>
        ''' <param name="x">One-column range containing the reference-method measurements. Values are paired row-by-row with <paramref name="y"/>.</param>
        ''' <param name="y">One-column range containing the test-method measurements. Values are paired row-by-row with <paramref name="x"/>.</param>
        ''' <param name="alpha">Optional two-sided significance level used for confidence intervals. Default 0.05.</param>
        ''' <param name="lambda">
        ''' Optional constant error ratio <c>lambda = sigma_x^2 / sigma_y^2</c> used when <paramref name="varianceModel"/> is <c>lambda</c>.
        ''' <c>lambda = 1</c> yields orthogonal regression. Larger values place relatively more error on the <c>x</c>-axis than on the <c>y</c>-axis.
        ''' </param>
        ''' <param name="ciMethod">Optional confidence-interval method. Accepted tokens are <c>analytical</c>, <c>jackknife</c>, <c>bootstrap</c>, and <c>bca</c>.</param>
        ''' <param name="varianceModel">
        ''' Optional variance model. Use:
        ''' <list type="bullet">
        ''' <item><description><c>lambda</c> for classical Deming regression with constant error ratio <c>lambda</c></description></item>
        ''' <item><description><c>pointwise</c> when you supply row-specific standard deviations through <paramref name="sdX"/> and <paramref name="sdY"/></description></item>
        ''' <item><description><c>cv</c> when measurement error is assumed proportional to magnitude and you supply constant coefficients of variation through <paramref name="cvX"/> and <paramref name="cvY"/></description></item>
        ''' </list>
        ''' </param>
        ''' <param name="fitIntercept">Optional TRUE/FALSE. If TRUE, fits <c>y = a + b x</c>. If FALSE, forces the line through the origin and fits <c>y = b x</c>.</param>
        ''' <param name="sdX">Optional one-column range of row-specific standard deviations for <c>x</c>. Required for <paramref name="varianceModel"/> = <c>pointwise</c>.</param>
        ''' <param name="sdY">Optional one-column range of row-specific standard deviations for <c>y</c>. Required for <paramref name="varianceModel"/> = <c>pointwise</c>.</param>
        ''' <param name="cvX">Optional coefficient of variation for <c>x</c>. Required for <paramref name="varianceModel"/> = <c>cv</c>.</param>
        ''' <param name="cvY">Optional coefficient of variation for <c>y</c>. Required for <paramref name="varianceModel"/> = <c>cv</c>.</param>
        ''' <param name="bootstrapReplicates">Optional bootstrap replicate count used by bootstrap-based intervals. Default 2000.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap resampling.</param>
        ''' <param name="varNames">Optional display names for the two methods as comma-separated text or a small range.</param>
        ''' <returns>A two-dimensional spill range containing the fitted coefficients, confidence intervals, diagnostics, and model settings.</returns>
        ''' <remarks>
        ''' <para>
        ''' Deming regression is an errors-in-variables method-comparison regression. Unlike ordinary least squares, it acknowledges
        ''' that both axes may be measured with error. The fitted line has the form <c>y = a + b x</c> unless the intercept is fixed at zero.
        ''' </para>
        ''' <para>
        ''' In the constant-ratio model, the procedure minimizes squared orthogonal deviations weighted according to the ratio
        ''' <c>lambda = sigma_x^2 / sigma_y^2</c>. In the pointwise-SD model, each row has its own measurement precision. In the constant-CV model,
        ''' the standard deviation is taken to be proportional to the absolute measurement magnitude, approximately
        ''' <c>sd_x,i = CV_x * |x_i|</c> and <c>sd_y,i = CV_y * |y_i|</c>.
        ''' </para>
        ''' <para>
        ''' Typical assumptions are:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description>The paired rows represent the same items or subjects in the same order.</description></item>
        ''' <item><description>The relation between methods is approximately linear over the observed range.</description></item>
        ''' <item><description>The chosen error model (<c>lambda</c>, <c>pointwise</c>, or <c>cv</c>) is a reasonable description of the measurement process.</description></item>
        ''' <item><description>Missing or non-numeric paired rows are removed pairwise; if pointwise standard deviations are supplied, they must be available for every retained row.</description></item>
        ''' </list>
        ''' <para>
        ''' Use this function when you want the full formatted worksheet report. Use <c>BESH.AGREE.DEMING_COEF</c>
        ''' when you only want slope and intercept estimates with confidence intervals.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.DEMING_FIT",
            Category:="BESHStatNG - Agreement",
            Description:="Weighted / generalized Deming regression for two paired methods. Returns a labeled result table.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function DEMING_FIT(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range. First cell may be a header.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range. First cell may be a header.")> y As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="lambda", Description:="Optional constant error ratio λ = σx²/σy². Used for varianceModel='lambda'. Default 1.")> Optional lambda As Object = Nothing,
            <ExcelArgument(Name:="ciMethod", Description:="Optional CI method: analytical | jackknife | bootstrap | bca.")> Optional ciMethod As Object = Nothing,
            <ExcelArgument(Name:="varianceModel", Description:="Optional variance model: lambda | pointwise | cv.")> Optional varianceModel As Object = Nothing,
            <ExcelArgument(Name:="fitIntercept", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional fitIntercept As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="sdX", Description:="Optional pointwise SD for x. Required for varianceModel='pointwise'. Must align row-wise with x and y.")> Optional sdX As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="sdY", Description:="Optional pointwise SD for y. Required for varianceModel='pointwise'. Must align row-wise with x and y.")> Optional sdY As Object = Nothing,
            <ExcelArgument(Name:="cvX", Description:="Optional coefficient of variation for x. Required for varianceModel='cv'.")> Optional cvX As Object = Nothing,
            <ExcelArgument(Name:="cvY", Description:="Optional coefficient of variation for y. Required for varianceModel='cv'.")> Optional cvY As Object = Nothing,
            <ExcelArgument(Name:="bootstrapReplicates", Description:="Optional bootstrap replicate count used by bootstrap CI methods. Default 2000.")> Optional bootstrapReplicates As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed used by bootstrap CI methods.")> Optional randomSeed As Object = Nothing,
            <ExcelArgument(Name:="varNames", Description:="Optional names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing) As Object
            Try
                Dim data = ReadAlignedDemingInputs(x, y, sdX, sdY)
                If data.Error.HasValue Then Return data.Error.Value
                If data.X Is Nothing OrElse data.X.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim names() As String = ParametricUDFs.ResolveNames(varNames, data.DetectedNames, 2, "Method")
                Dim opts As New Agreement.DemingOptions With {
                    .Alpha = ParseAlphaOrDefault(alpha, 0.05),
                    .Lambda = UDFhelpers.GetOptionalDouble(lambda, 1.0),
                    .CiMethod = ParseAgreementCiMethod(ciMethod, Agreement.AgreementCiMethod.Jackknife),
                    .VarianceModel = ParseDemingVarianceModel(varianceModel, Agreement.DemingVarianceModel.ConstantLambda),
                    .FitIntercept = UDFhelpers.GetOptionalBool(fitIntercept, True),
                    .CVx = ParseOptionalNullableDouble(cvX),
                    .CVy = ParseOptionalNullableDouble(cvY),
                    .BootstrapReplicates = Math.Max(200, UDFhelpers.GetOptionalInt(bootstrapReplicates, 2000))
                }
                If Not UDFhelpers.IsMissingArg(sdX) Then opts.SDx = data.SDx
                If Not UDFhelpers.IsMissingArg(sdY) Then opts.SDy = data.SDy

                Dim seed As Integer = ParseOptionalSeed(randomSeed)
                Dim mdl As New Agreement.WeightedDemingRegression(data.X, data.Y, names(0), names(1), opts)
                mdl.alpha = opts.Alpha
                If opts.VarianceModel = Agreement.DemingVarianceModel.ConstantLambda Then mdl.Lambda = opts.Lambda
                mdl.Fit(Nothing, seed)
                Return StackResultTables(mdl.wrapResults())
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.DEMING_FIT", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns Deming or weighted Deming regression coefficients together with their confidence intervals.
        ''' </summary>
        ''' <param name="x">One-column range of reference-method values.</param>
        ''' <param name="y">One-column range of test-method values.</param>
        ''' <param name="alpha">Optional two-sided significance level used for confidence intervals. Default 0.05.</param>
        ''' <param name="lambda">Optional constant error ratio <c>lambda = sigma_x^2 / sigma_y^2</c> for the classical Deming model.</param>
        ''' <param name="ciMethod">Optional confidence-interval method: <c>analytical</c>, <c>jackknife</c>, <c>bootstrap</c>, or <c>bca</c>.</param>
        ''' <param name="varianceModel">Optional variance model: <c>lambda</c>, <c>pointwise</c>, or <c>cv</c>.</param>
        ''' <param name="fitIntercept">Optional TRUE/FALSE. If FALSE, fits through the origin.</param>
        ''' <param name="sdX">Optional row-specific standard deviations for <c>x</c> when using the pointwise variance model.</param>
        ''' <param name="sdY">Optional row-specific standard deviations for <c>y</c> when using the pointwise variance model.</param>
        ''' <param name="cvX">Optional coefficient of variation for <c>x</c> when using the constant-CV variance model.</param>
        ''' <param name="cvY">Optional coefficient of variation for <c>y</c> when using the constant-CV variance model.</param>
        ''' <param name="bootstrapReplicates">Optional bootstrap replicate count.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap resampling.</param>
        ''' <returns>A compact spill range containing slope and intercept estimates with lower and upper confidence limits.</returns>
        ''' <remarks>
        ''' <para>
        ''' This is the coefficient-focused companion to <c>BESH.AGREE.DEMING_FIT</c>. It fits the same model family but returns only
        ''' the estimated slope and intercept together with interval estimates.
        ''' </para>
        ''' <para>
        ''' The slope quantifies proportional difference between methods; the intercept quantifies systematic offset.
        ''' When <paramref name="fitIntercept"/> is FALSE, the fitted line is constrained to pass through the origin.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.DEMING_COEF",
            Category:="BESHStatNG - Agreement",
            Description:="Weighted / generalized Deming regression coefficients and confidence intervals.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function DEMING_COEF(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range.")> y As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="lambda", Description:="Optional constant error ratio λ = σx²/σy².")> Optional lambda As Object = Nothing,
            <ExcelArgument(Name:="ciMethod", Description:="Optional CI method: analytical | jackknife | bootstrap | bca.")> Optional ciMethod As Object = Nothing,
            <ExcelArgument(Name:="varianceModel", Description:="Optional variance model: lambda | pointwise | cv.")> Optional varianceModel As Object = Nothing,
            <ExcelArgument(Name:="fitIntercept", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional fitIntercept As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="sdX", Description:="Optional pointwise SD for x.")> Optional sdX As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="sdY", Description:="Optional pointwise SD for y.")> Optional sdY As Object = Nothing,
            <ExcelArgument(Name:="cvX", Description:="Optional coefficient of variation for x.")> Optional cvX As Object = Nothing,
            <ExcelArgument(Name:="cvY", Description:="Optional coefficient of variation for y.")> Optional cvY As Object = Nothing,
            <ExcelArgument(Name:="bootstrapReplicates", Description:="Optional bootstrap replicate count used by bootstrap CI methods. Default 2000.")> Optional bootstrapReplicates As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed used by bootstrap CI methods.")> Optional randomSeed As Object = Nothing) As Object
            Try
                Dim data = ReadAlignedDemingInputs(x, y, sdX, sdY)
                If data.Error.HasValue Then Return data.Error.Value
                If data.X Is Nothing OrElse data.X.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim opts As New Agreement.DemingOptions With {
                    .Alpha = ParseAlphaOrDefault(alpha, 0.05),
                    .Lambda = UDFhelpers.GetOptionalDouble(lambda, 1.0),
                    .CiMethod = ParseAgreementCiMethod(ciMethod, Agreement.AgreementCiMethod.Jackknife),
                    .VarianceModel = ParseDemingVarianceModel(varianceModel, Agreement.DemingVarianceModel.ConstantLambda),
                    .FitIntercept = UDFhelpers.GetOptionalBool(fitIntercept, True),
                    .CVx = ParseOptionalNullableDouble(cvX),
                    .CVy = ParseOptionalNullableDouble(cvY),
                    .BootstrapReplicates = Math.Max(200, UDFhelpers.GetOptionalInt(bootstrapReplicates, 2000))
                }
                If Not UDFhelpers.IsMissingArg(sdX) Then opts.SDx = data.SDx
                If Not UDFhelpers.IsMissingArg(sdY) Then opts.SDy = data.SDy

                Dim seed As Integer = ParseOptionalSeed(randomSeed)
                Dim mdl As New Agreement.WeightedDemingRegression(data.X, data.Y, "Reference", "Test", opts)
                mdl.Fit(Nothing, seed)

                Dim t As New ResultTable
                t.AddTitle("Deming Regression Coefficients")
                t.AddHeaderLeftRow({"Slope", "Intercept"})
                t.AddHeaderTopRow({"Estimate", mdl.SlopeCI.CIlabel, "Lower", "Upper"})
                t.SetBody(New Object(,) {
                    {mdl.SlopeCI.Estimate, mdl.SlopeCI.strConfidenceInterval(CIformat.LL_to_UL), mdl.SlopeCI.LowerLimit, mdl.SlopeCI.UpperLimit},
                    {mdl.InterceptCI.Estimate, mdl.InterceptCI.strConfidenceInterval(CIformat.LL_to_UL), mdl.InterceptCI.LowerLimit, mdl.InterceptCI.UpperLimit}
                })
                Return PrepareResultTableForUdf(t.returnSelf())
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.DEMING_COEF", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns a spillable labeled result table for Bland–Altman agreement analysis.
        ''' </summary>
        ''' <param name="x">One-column range of reference-method values. Values are paired row-by-row with <paramref name="y"/>.</param>
        ''' <param name="y">One-column range of test-method values. Values are paired row-by-row with <paramref name="x"/>.</param>
        ''' <param name="subjectIds">
        ''' Optional one-column subject or sample identifiers aligned row-by-row with the paired measurements.
        ''' Supply this when repeated paired measurements exist for the same subject and repeated-measures Bland–Altman analysis is desired.
        ''' </param>
        ''' <param name="alpha">Optional two-sided significance level used for confidence intervals. Default 0.05.</param>
        ''' <param name="mode">Optional analysis mode: <c>auto</c>, <c>simple</c>, or <c>repeated</c>.</param>
        ''' <param name="scale">
        ''' Optional difference scale: <c>raw</c>, <c>meanpct</c>, <c>refpct</c>, <c>testpct</c>, or <c>logratio</c>.
        ''' These correspond respectively to
        ''' <c>d_i = y_i - x_i</c>,
        ''' <c>100*(y_i-x_i)/((x_i+y_i)/2)</c>,
        ''' <c>100*(y_i-x_i)/x_i</c>,
        ''' <c>100*(y_i-x_i)/y_i</c>, and
        ''' <c>ln(y_i/x_i)</c>.
        ''' </param>
        ''' <param name="xAxis">Optional x-axis convention: <c>mean</c>, <c>reference</c>, or <c>test</c>.</param>
        ''' <param name="ciMethod">Optional confidence-interval method: <c>analytical</c>, <c>jackknife</c>, <c>bootstrap</c>, or <c>bca</c>.</param>
        ''' <param name="bootstrapReplicates">Optional bootstrap replicate count. Default 2000.</param>
        ''' <param name="useT">Optional TRUE/FALSE. If TRUE, analytical and jackknife intervals use the Student-t critical value where applicable.</param>
        ''' <param name="minSubjects">Optional minimum subject count required for repeated-measures mode. Default 2.</param>
        ''' <param name="minPairsPerSubject">Optional minimum number of paired observations required for a subject to contribute to repeated-measures mode. Default 2.</param>
        ''' <param name="excludeSingletonSubjects">Optional TRUE/FALSE. If TRUE, singleton subjects are excluded from repeated-measures estimation.</param>
        ''' <param name="allowFallbackToSimple">Optional TRUE/FALSE. If TRUE, repeated-mode requests may fall back to ordinary paired Bland–Altman when repeated-data requirements are not met.</param>
        ''' <param name="checkProportionalBias">Optional TRUE/FALSE. If TRUE, tests for a linear trend of the differences against the chosen x-axis quantity.</param>
        ''' <param name="plotMode">Optional repeated-measures plot mode: <c>all</c>, <c>means</c>, or <c>both</c>.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap resampling.</param>
        ''' <param name="varNames">Optional display names for the two methods as comma-separated text or a small range.</param>
        ''' <returns>A two-dimensional spill range containing a full Bland–Altman worksheet-style report.</returns>
        ''' <remarks>
        ''' <para>
        ''' Bland–Altman analysis is an agreement method, not a correlation method. For ordinary paired data the key summaries are:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description><b>Bias</b> = mean of the paired differences <c>d_i</c></description></item>
        ''' <item><description><b>Standard deviation of differences</b> = sample standard deviation of <c>d_i</c></description></item>
        ''' <item><description><b>Limits of agreement</b> = <c>bias +/- 1.96 * SD(d)</c> on the chosen scale</description></item>
        ''' </list>
        ''' <para>
        ''' The function can also run a repeated-measures version when <paramref name="subjectIds"/> is supplied, allowing within-subject variation
        ''' to drive the agreement limits rather than treating every row as fully independent.
        ''' </para>
        ''' <para>
        ''' Important assumptions depend on the chosen scale:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description><c>raw</c>: disagreement is interpreted in the original measurement units.</description></item>
        ''' <item><description><c>meanpct</c>, <c>refpct</c>, and <c>testpct</c>: denominators must be non-zero for retained rows.</description></item>
        ''' <item><description><c>logratio</c>: both methods must be strictly positive for retained rows.</description></item>
        ''' <item><description>Rows must represent paired measurements on the same item in the same order.</description></item>
        ''' </list>
        ''' <para>
        ''' Missing or invalid paired rows are removed pairwise before analysis.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.BLANDALTMAN_FIT",
            Category:="BESHStatNG - Agreement",
            Description:="Bland–Altman analysis for two paired methods. Returns a labeled result table.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function BLANDALTMAN_FIT(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range. First cell may be a header.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range. First cell may be a header.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="subjectIds", Description:="Optional subject/sample IDs aligned row-wise with x and y for repeated-measures Bland–Altman.")> Optional subjectIds As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="mode", Description:="Optional mode: auto | simple | repeated.")> Optional mode As Object = Nothing,
            <ExcelArgument(Name:="scale", Description:="Optional difference scale: raw | meanpct | refpct | testpct | logratio.")> Optional scale As Object = Nothing,
            <ExcelArgument(Name:="xAxis", Description:="Optional x-axis: mean | reference | test.")> Optional xAxis As Object = Nothing,
            <ExcelArgument(Name:="ciMethod", Description:="Optional CI method: analytical | jackknife | bootstrap | bca.")> Optional ciMethod As Object = Nothing,
            <ExcelArgument(Name:="bootstrapReplicates", Description:="Optional bootstrap replicate count. Default 2000.")> Optional bootstrapReplicates As Object = Nothing,
            <ExcelArgument(Name:="useT", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional useT As Object = Nothing,
            <ExcelArgument(Name:="minSubjects", Description:="Optional minimum subject count for repeated mode. Default 2.")> Optional minSubjects As Object = Nothing,
            <ExcelArgument(Name:="minPairsPerSubject", Description:="Optional minimum pairs per subject for repeated mode. Default 2.")> Optional minPairsPerSubject As Object = Nothing,
            <ExcelArgument(Name:="excludeSingletonSubjects", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional excludeSingletonSubjects As Object = Nothing,
            <ExcelArgument(Name:="allowFallbackToSimple", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional allowFallbackToSimple As Object = Nothing,
            <ExcelArgument(Name:="checkProportionalBias", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional checkProportionalBias As Object = Nothing,
            <ExcelArgument(Name:="plotMode", Description:="Optional plot mode: all | means | both.")> Optional plotMode As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed used by bootstrap CI methods.")> Optional randomSeed As Object = Nothing,
            <ExcelArgument(Name:="varNames", Description:="Optional names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing) As Object
            Try
                Dim input = ReadAlignedNumericWithOptionalCategory(x, y, subjectIds, requireCategory:=False)
                If input.Error.HasValue Then Return input.Error.Value
                If input.X Is Nothing OrElse input.X.Length < 2 Then Return ExcelError.ExcelErrorNum
                Dim names() As String = ParametricUDFs.ResolveNames(varNames, input.DetectedNames, 2, "Method")

                Dim opts As New Agreement.BlandAltmanOptions With {
                    .Alpha = ParseAlphaOrDefault(alpha, 0.05),
                    .Mode = ParseBlandMode(mode, Agreement.RepeatedBlandAltmanMode.Auto),
                    .Scale = ParseBlandScale(scale, Agreement.BlandAltmanScale.RawDifference),
                    .XAxisMode = ParseBlandXAxisMode(xAxis, Agreement.BlandAltmanXAxisMode.MeanOfMethods),
                    .CiMethod = ParseAgreementCiMethod(ciMethod, Agreement.AgreementCiMethod.Analytical),
                    .BootstrapReplicates = Math.Max(200, UDFhelpers.GetOptionalInt(bootstrapReplicates, 2000)),
                    .UseTDistribution = UDFhelpers.GetOptionalBool(useT, True),
                    .MinSubjects = Math.Max(1, UDFhelpers.GetOptionalInt(minSubjects, 2)),
                    .MinPairsPerSubject = Math.Max(1, UDFhelpers.GetOptionalInt(minPairsPerSubject, 2)),
                    .ExcludeSingletonSubjects = UDFhelpers.GetOptionalBool(excludeSingletonSubjects, True),
                    .AllowFallbackToSimple = UDFhelpers.GetOptionalBool(allowFallbackToSimple, True),
                    .CheckProportionalBias = UDFhelpers.GetOptionalBool(checkProportionalBias, True),
                    .PlotMode = ParseBlandPlotMode(plotMode, Agreement.RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans)
                }

                If input.Category IsNot Nothing Then opts.SubjectIds = input.Category

                Dim seed As Integer = ParseOptionalSeed(randomSeed)
                Dim mdl As New Agreement.BlandAltmanAgreement(input.X, input.Y, names(0), names(1), opts)
                mdl.Fit(seed)
                Return StackResultTables(mdl.wrapResults())
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.BLANDALTMAN_FIT", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns a compact numerical summary of Bland–Altman statistics.
        ''' </summary>
        ''' <param name="x">One-column range of reference-method values.</param>
        ''' <param name="y">One-column range of test-method values.</param>
        ''' <param name="subjectIds">Optional one-column subject IDs aligned with <paramref name="x"/> and <paramref name="y"/> for repeated-measures Bland–Altman.</param>
        ''' <param name="alpha">Optional two-sided significance level. Default 0.05.</param>
        ''' <param name="mode">Optional analysis mode: <c>auto</c>, <c>simple</c>, or <c>repeated</c>.</param>
        ''' <param name="scale">Optional difference scale.</param>
        ''' <param name="xAxis">Optional x-axis convention.</param>
        ''' <param name="ciMethod">Optional confidence-interval method.</param>
        ''' <param name="bootstrapReplicates">Optional bootstrap replicate count.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap resampling.</param>
        ''' <returns>
        ''' A compact spill range containing the core agreement quantities such as bias, lower limit of agreement, upper limit of agreement,
        ''' standard deviation of differences, and corresponding confidence intervals.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This is the compact numeric companion to <c>BESH.AGREE.BLANDALTMAN_FIT</c>. It applies the same agreement model but returns a smaller
        ''' result intended for formulas and downstream worksheet calculations rather than a full report layout.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.BLANDALTMAN_STATS",
            Category:="BESHStatNG - Agreement",
            Description:="Bland–Altman bias and limits of agreement for two paired methods.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function BLANDALTMAN_STATS(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="subjectIds", Description:="Optional subject/sample IDs aligned row-wise with x and y.")> Optional subjectIds As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="mode", Description:="Optional mode: auto | simple | repeated.")> Optional mode As Object = Nothing,
            <ExcelArgument(Name:="scale", Description:="Optional difference scale.")> Optional scale As Object = Nothing,
            <ExcelArgument(Name:="xAxis", Description:="Optional x-axis.")> Optional xAxis As Object = Nothing,
            <ExcelArgument(Name:="ciMethod", Description:="Optional CI method.")> Optional ciMethod As Object = Nothing,
            <ExcelArgument(Name:="bootstrapReplicates", Description:="Optional bootstrap replicate count.")> Optional bootstrapReplicates As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed used by bootstrap CI methods.")> Optional randomSeed As Object = Nothing) As Object
            Try
                Dim input = ReadAlignedNumericWithOptionalCategory(x, y, subjectIds, requireCategory:=False)
                If input.Error.HasValue Then Return input.Error.Value
                If input.X Is Nothing OrElse input.X.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim opts As New Agreement.BlandAltmanOptions With {
                    .Alpha = ParseAlphaOrDefault(alpha, 0.05),
                    .Mode = ParseBlandMode(mode, Agreement.RepeatedBlandAltmanMode.Auto),
                    .Scale = ParseBlandScale(scale, Agreement.BlandAltmanScale.RawDifference),
                    .XAxisMode = ParseBlandXAxisMode(xAxis, Agreement.BlandAltmanXAxisMode.MeanOfMethods),
                    .CiMethod = ParseAgreementCiMethod(ciMethod, Agreement.AgreementCiMethod.Analytical),
                    .BootstrapReplicates = Math.Max(200, UDFhelpers.GetOptionalInt(bootstrapReplicates, 2000))
                }
                If input.Category IsNot Nothing Then opts.SubjectIds = input.Category

                Dim mdl As New Agreement.BlandAltmanAgreement(input.X, input.Y, "Reference", "Test", opts)
                Dim res = mdl.Fit(ParseOptionalSeed(randomSeed))

                Dim t As New ResultTable
                t.AddTitle("Bland–Altman Statistics")
                t.AddHeaderLeftRow({"Bias", "Lower LoA", "Upper LoA", "SD(diff)", "Repeatability coefficient"})
                t.AddHeaderTopRow({"Estimate", res.BiasCI.CIlabel, "Lower", "Upper"})
                t.SetBody(New Object(,) {
                    {res.BiasCI.Estimate, res.BiasCI.strConfidenceInterval(CIformat.LL_to_UL), res.BiasCI.LowerLimit, res.BiasCI.UpperLimit},
                    {res.LowerLoACI.Estimate, res.LowerLoACI.strConfidenceInterval(CIformat.LL_to_UL), res.LowerLoACI.LowerLimit, res.LowerLoACI.UpperLimit},
                    {res.UpperLoACI.Estimate, res.UpperLoACI.strConfidenceInterval(CIformat.LL_to_UL), res.UpperLoACI.LowerLimit, res.UpperLoACI.UpperLimit},
                    {res.SdDifference, "", "", ""},
                    {res.RepeatabilityCoefficient, "", "", ""}
                })
                Return PrepareResultTableForUdf(t.returnSelf())
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.BLANDALTMAN_STATS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns the x-y pairs required to draw a Bland–Altman plot in the worksheet.
        ''' </summary>
        ''' <param name="x">One-column range of reference-method values.</param>
        ''' <param name="y">One-column range of test-method values.</param>
        ''' <param name="subjectIds">Optional subject or sample IDs for repeated-measures mode.</param>
        ''' <param name="mode">Optional analysis mode: <c>auto</c>, <c>simple</c>, or <c>repeated</c>.</param>
        ''' <param name="scale">Optional difference scale.</param>
        ''' <param name="xAxis">Optional x-axis convention.</param>
        ''' <param name="plotMode">Optional repeated-measures plot mode: <c>all</c>, <c>means</c>, or <c>both</c>.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap-enabled workflows.</param>
        ''' <returns>
        ''' A spill range containing the selected x-axis values, plotted differences, and horizontal reference values needed to draw the bias and limits-of-agreement lines.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' In a classical Bland–Altman plot the x-axis is usually the mean of the two methods and the y-axis is the paired difference.
        ''' This function lets you request alternative x-axis conventions and repeated-measures subject-mean plotting modes while still returning worksheet-friendly plot coordinates.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.BLANDALTMAN_PLOTDATA",
            Category:="BESHStatNG - Agreement",
            Description:="Bland–Altman plot data (observation and subject-mean coordinates).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function BLANDALTMAN_PLOTDATA(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="subjectIds", Description:="Optional subject/sample IDs aligned row-wise with x and y.")> Optional subjectIds As Object = Nothing,
            <ExcelArgument(Name:="mode", Description:="Optional mode: auto | simple | repeated.")> Optional mode As Object = Nothing,
            <ExcelArgument(Name:="scale", Description:="Optional difference scale.")> Optional scale As Object = Nothing,
            <ExcelArgument(Name:="xAxis", Description:="Optional x-axis.")> Optional xAxis As Object = Nothing,
            <ExcelArgument(Name:="plotMode", Description:="Optional plot mode: all | means | both.")> Optional plotMode As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed for bootstrap-enabled modes.")> Optional randomSeed As Object = Nothing) As Object
            Try
                Dim input = ReadAlignedNumericWithOptionalCategory(x, y, subjectIds, requireCategory:=False)
                If input.Error.HasValue Then Return input.Error.Value
                If input.X Is Nothing OrElse input.X.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim opts As New Agreement.BlandAltmanOptions With {
                    .Mode = ParseBlandMode(mode, Agreement.RepeatedBlandAltmanMode.Auto),
                    .Scale = ParseBlandScale(scale, Agreement.BlandAltmanScale.RawDifference),
                    .XAxisMode = ParseBlandXAxisMode(xAxis, Agreement.BlandAltmanXAxisMode.MeanOfMethods),
                    .PlotMode = ParseBlandPlotMode(plotMode, Agreement.RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans)
                }
                If input.Category IsNot Nothing Then opts.SubjectIds = input.Category

                Dim mdl As New Agreement.BlandAltmanAgreement(input.X, input.Y, "Reference", "Test", opts)
                Dim res = mdl.Fit(ParseOptionalSeed(randomSeed))
                Return BuildBlandPlotDataTable(res)
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.BLANDALTMAN_PLOTDATA", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Assesses whether the Bland–Altman bias confidence interval is acceptable relative to prespecified allowable limits.
        ''' </summary>
        ''' <param name="x">
        ''' Reference-method values as a single-column range. Values must be paired row-by-row with <paramref name="y"/>.
        ''' If the first cell looks like text, it is treated as a header.
        ''' </param>
        ''' <param name="y">
        ''' Test-method values as a single-column range. Values must be paired row-by-row with <paramref name="x"/>.
        ''' If the first cell looks like text, it is treated as a header.
        ''' </param>
        ''' <param name="lowerAllowableBias">Lower acceptable bias on the active Bland–Altman analysis scale.</param>
        ''' <param name="upperAllowableBias">Upper acceptable bias on the active Bland–Altman analysis scale.</param>
        ''' <param name="subjectIds">
        ''' Optional subject identifiers aligned row-by-row with <paramref name="x"/> and <paramref name="y"/>.
        ''' Supply this to enable repeated-measures Bland–Altman assessment.
        ''' </param>
        ''' <param name="alpha">Optional two-sided alpha used for the bias confidence interval. Default <c>0.05</c>.</param>
        ''' <param name="mode">Optional Bland–Altman mode: <c>auto</c>, <c>simple</c>, or <c>repeated</c>.</param>
        ''' <param name="scale">Optional difference scale: <c>raw</c>, <c>meanpct</c>, <c>refpct</c>, <c>testpct</c>, or <c>logratio</c>.</param>
        ''' <param name="xAxis">Optional x-axis convention: <c>mean</c>, <c>reference</c>, or <c>test</c>.</param>
        ''' <param name="ciMethod">Optional confidence-interval method: <c>analytical</c>, <c>jackknife</c>, <c>bootstrap</c>, or <c>bca</c>.</param>
        ''' <param name="bootstrapReplicates">Optional bootstrap replicate count. Default <c>2000</c>.</param>
        ''' <param name="useT">Optional TRUE/FALSE. When TRUE, analytical intervals use the Student-t critical value. Default TRUE.</param>
        ''' <param name="minSubjects">Optional minimum number of distinct subjects required for repeated mode. Default <c>2</c>.</param>
        ''' <param name="minPairsPerSubject">Optional minimum usable pairs per subject for repeated mode. Default <c>2</c>.</param>
        ''' <param name="excludeSingletonSubjects">Optional TRUE/FALSE. Default TRUE.</param>
        ''' <param name="allowFallbackToSimple">Optional TRUE/FALSE. Default TRUE.</param>
        ''' <param name="checkProportionalBias">Optional TRUE/FALSE. Default TRUE.</param>
        ''' <param name="plotMode">Optional repeated-measures plot mode: <c>all</c>, <c>means</c>, or <c>both</c>.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap resampling.</param>
        ''' <param name="varNames">Optional method names as comma-separated text or a 1-row/1-column range.</param>
        ''' <returns>
        ''' A labeled spill table reporting the fitted bias estimate and confidence interval on the selected analysis scale,
        ''' together with the allowable bias limits and the interval-based decision.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function is a decision-support companion to the Bland–Altman agreement analysis.
        ''' It first fits Bland–Altman on the requested scale and then compares the confidence interval for the mean bias with the
        ''' supplied allowable bias region <c>[L, U]</c>.
        ''' </para>
        ''' <para>
        ''' When the analysis scale is transformed, the allowable limits must be expressed on that same transformed scale:
        ''' for example percent limits for percent-difference analyses, or log-ratio limits for log-ratio analysis.
        ''' </para>
        ''' <para>
        ''' The assessment distinguishes whether only the point estimate lies inside the allowable region or whether the full confidence interval does.
        ''' The latter is the stricter and usually more defensible criterion in method-comparison studies.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.AGREE.BLANDALTMAN_ALLOWABLE_BIAS(A2:A31,B2:B31,-2,2)
        ''' =BESH.AGREE.BLANDALTMAN_ALLOWABLE_BIAS(A2:A31,B2:B31,-10,10,C2:C31,0.05,"repeated","meanpct")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.AGREE.BLANDALTMAN_ALLOWABLE_BIAS",
            Category:="BESHStatNG - Agreement",
            Description:="Assess Bland–Altman bias against allowable limits on the active analysis scale.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function BLANDALTMAN_ALLOWABLE_BIAS(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range. First cell may be a header.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range. First cell may be a header.")> y As Object,
            <ExcelArgument(Name:="lowerAllowableBias", Description:="Lower allowable bias on the active analysis scale.")> lowerAllowableBias As Object,
            <ExcelArgument(Name:="upperAllowableBias", Description:="Upper allowable bias on the active analysis scale.")> upperAllowableBias As Object,
            <ExcelArgument(AllowReference:=True, Name:="subjectIds", Description:="Optional subject/sample IDs aligned row-wise with x and y for repeated-measures assessment.")> Optional subjectIds As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="mode", Description:="Optional mode: auto | simple | repeated.")> Optional mode As Object = Nothing,
            <ExcelArgument(Name:="scale", Description:="Optional difference scale: raw | meanpct | refpct | testpct | logratio.")> Optional scale As Object = Nothing,
            <ExcelArgument(Name:="xAxis", Description:="Optional x-axis: mean | reference | test.")> Optional xAxis As Object = Nothing,
            <ExcelArgument(Name:="ciMethod", Description:="Optional CI method: analytical | jackknife | bootstrap | bca.")> Optional ciMethod As Object = Nothing,
            <ExcelArgument(Name:="bootstrapReplicates", Description:="Optional bootstrap replicate count. Default 2000.")> Optional bootstrapReplicates As Object = Nothing,
            <ExcelArgument(Name:="useT", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional useT As Object = Nothing,
            <ExcelArgument(Name:="minSubjects", Description:="Optional minimum subject count for repeated mode. Default 2.")> Optional minSubjects As Object = Nothing,
            <ExcelArgument(Name:="minPairsPerSubject", Description:="Optional minimum usable pairs per subject for repeated mode. Default 2.")> Optional minPairsPerSubject As Object = Nothing,
            <ExcelArgument(Name:="excludeSingletonSubjects", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional excludeSingletonSubjects As Object = Nothing,
            <ExcelArgument(Name:="allowFallbackToSimple", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional allowFallbackToSimple As Object = Nothing,
            <ExcelArgument(Name:="checkProportionalBias", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional checkProportionalBias As Object = Nothing,
            <ExcelArgument(Name:="plotMode", Description:="Optional plot mode: all | means | both.")> Optional plotMode As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed used by bootstrap CI methods.")> Optional randomSeed As Object = Nothing,
            <ExcelArgument(Name:="varNames", Description:="Optional names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing) As Object
            Try
                Dim lowerValue As Double
                Dim upperValue As Double
                If Not TryGetFiniteDouble(lowerAllowableBias, lowerValue) OrElse Not TryGetFiniteDouble(upperAllowableBias, upperValue) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If lowerValue > upperValue Then Return ExcelError.ExcelErrorNum

                Dim names() As String = Nothing
                Dim fit As Agreement.BlandAltmanResult = FitBlandAltmanFromUdfArgs(x, y, subjectIds, alpha, mode, scale, xAxis, ciMethod,
                                                                                   bootstrapReplicates, useT, minSubjects, minPairsPerSubject,
                                                                                   excludeSingletonSubjects, allowFallbackToSimple, checkProportionalBias,
                                                                                   plotMode, randomSeed, varNames, names)
                If fit Is Nothing Then Return ExcelError.ExcelErrorValue

                Dim assessment As MarginCiAssessmentResult = EquivalenceNonInferiorityMethods.AssessAllowableBias(fit, lowerValue, upperValue)
                Dim body As Object(,) = {
                    {"Reference method", names(0)},
                    {"Test method", names(1)},
                    {"Model actually used", If(fit.UsedRepeatedModel, "Repeated-measures Bland–Altman", "Ordinary paired Bland–Altman")},
                    {"Difference scale", DescribeBlandScale(ParseBlandScale(scale, Agreement.BlandAltmanScale.RawDifference))},
                    {"Bias estimate", assessment.Estimate},
                    {SafeCiLabel(assessment.ConfidenceInterval), SafeCiText(assessment.ConfidenceInterval)},
                    {"Lower allowable bias", assessment.LowerMargin},
                    {"Upper allowable bias", assessment.UpperMargin},
                    {"Point estimate within allowable limits", assessment.IsPointEstimateWithinMargins},
                    {"Confidence interval within allowable limits", assessment.IsConfidenceIntervalWithinMargins},
                    {"Lower limit supported by CI", assessment.SupportsLowerNonInferiority},
                    {"Upper limit supported by CI", assessment.SupportsUpperNonInferiority},
                    {"Conclusion", assessment.Conclusion}
                }
                Return BuildResultTable("Bland–Altman Allowable-Bias Assessment", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.BLANDALTMAN_ALLOWABLE_BIAS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Assesses whether Bland–Altman bias and limits of agreement remain inside prespecified allowable limits.
        ''' </summary>
        ''' <param name="x">Reference-method values as a single-column range.</param>
        ''' <param name="y">Test-method values as a single-column range.</param>
        ''' <param name="lowerAllowableLimit">Lower acceptable limit on the active Bland–Altman analysis scale.</param>
        ''' <param name="upperAllowableLimit">Upper acceptable limit on the active Bland–Altman analysis scale.</param>
        ''' <param name="subjectIds">Optional subject identifiers aligned row-by-row with <paramref name="x"/> and <paramref name="y"/> for repeated-measures Bland–Altman.</param>
        ''' <param name="alpha">Optional two-sided alpha. Default 0.05.</param>
        ''' <param name="mode">Optional Bland–Altman mode: <c>auto</c>, <c>simple</c>, or <c>repeated</c>.</param>
        ''' <param name="scale">Optional difference scale.</param>
        ''' <param name="xAxis">Optional x-axis convention.</param>
        ''' <param name="ciMethod">Optional confidence-interval method.</param>
        ''' <param name="bootstrapReplicates">Optional bootstrap replicate count.</param>
        ''' <param name="useT">Optional TRUE/FALSE. Default TRUE.</param>
        ''' <param name="minSubjects">Optional minimum subject count for repeated mode.</param>
        ''' <param name="minPairsPerSubject">Optional minimum usable pairs per subject.</param>
        ''' <param name="excludeSingletonSubjects">Optional TRUE/FALSE. Default TRUE.</param>
        ''' <param name="allowFallbackToSimple">Optional TRUE/FALSE. Default TRUE.</param>
        ''' <param name="checkProportionalBias">Optional TRUE/FALSE. Default TRUE.</param>
        ''' <param name="plotMode">Optional repeated-measures plot mode.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap resampling.</param>
        ''' <param name="varNames">Optional method names.</param>
        ''' <returns>
        ''' A labeled spill table reporting the fitted bias, lower limit of agreement, and upper limit of agreement,
        ''' together with their confidence intervals and the corresponding allowable-limits decision.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function extends ordinary Bland–Altman reporting into a prespecified decision framework.
        ''' It asks whether the observed bias and the agreement limits are acceptably small for the intended use.
        ''' </para>
        ''' <para>
        ''' Let <c>[L, U]</c> be the acceptable region on the active analysis scale. The function reports:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description>whether the fitted bias confidence interval lies entirely inside <c>[L, U]</c></description></item>
        ''' <item><description>whether the observed lower and upper limits of agreement lie inside <c>[L, U]</c></description></item>
        ''' <item><description>whether the confidence interval for the lower limit stays above <c>L</c> and the confidence interval for the upper limit stays below <c>U</c></description></item>
        ''' </list>
        ''' <para>
        ''' When repeated-measures or transformed-scale Bland–Altman is requested, the allowable limits must be given on that same effective scale.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.AGREE.BLANDALTMAN_DECISION(A2:A31,B2:B31,-5,5)
        ''' =BESH.AGREE.BLANDALTMAN_DECISION(A2:A31,B2:B31,-15,15,C2:C31,0.05,"repeated","meanpct")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.AGREE.BLANDALTMAN_DECISION",
            Category:="BESHStatNG - Agreement",
            Description:="Assess Bland–Altman bias and limits of agreement against allowable decision limits.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function BLANDALTMAN_DECISION(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range. First cell may be a header.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range. First cell may be a header.")> y As Object,
            <ExcelArgument(Name:="lowerAllowableLimit", Description:="Lower allowable limit on the active analysis scale.")> lowerAllowableLimit As Object,
            <ExcelArgument(Name:="upperAllowableLimit", Description:="Upper allowable limit on the active analysis scale.")> upperAllowableLimit As Object,
            <ExcelArgument(AllowReference:=True, Name:="subjectIds", Description:="Optional subject/sample IDs aligned row-wise with x and y for repeated-measures assessment.")> Optional subjectIds As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="mode", Description:="Optional mode: auto | simple | repeated.")> Optional mode As Object = Nothing,
            <ExcelArgument(Name:="scale", Description:="Optional difference scale: raw | meanpct | refpct | testpct | logratio.")> Optional scale As Object = Nothing,
            <ExcelArgument(Name:="xAxis", Description:="Optional x-axis: mean | reference | test.")> Optional xAxis As Object = Nothing,
            <ExcelArgument(Name:="ciMethod", Description:="Optional CI method: analytical | jackknife | bootstrap | bca.")> Optional ciMethod As Object = Nothing,
            <ExcelArgument(Name:="bootstrapReplicates", Description:="Optional bootstrap replicate count. Default 2000.")> Optional bootstrapReplicates As Object = Nothing,
            <ExcelArgument(Name:="useT", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional useT As Object = Nothing,
            <ExcelArgument(Name:="minSubjects", Description:="Optional minimum subject count for repeated mode. Default 2.")> Optional minSubjects As Object = Nothing,
            <ExcelArgument(Name:="minPairsPerSubject", Description:="Optional minimum usable pairs per subject for repeated mode. Default 2.")> Optional minPairsPerSubject As Object = Nothing,
            <ExcelArgument(Name:="excludeSingletonSubjects", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional excludeSingletonSubjects As Object = Nothing,
            <ExcelArgument(Name:="allowFallbackToSimple", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional allowFallbackToSimple As Object = Nothing,
            <ExcelArgument(Name:="checkProportionalBias", Description:="Optional TRUE/FALSE. Default TRUE.")> Optional checkProportionalBias As Object = Nothing,
            <ExcelArgument(Name:="plotMode", Description:="Optional plot mode: all | means | both.")> Optional plotMode As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed used by bootstrap CI methods.")> Optional randomSeed As Object = Nothing,
            <ExcelArgument(Name:="varNames", Description:="Optional names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing) As Object
            Try
                Dim lowerValue As Double
                Dim upperValue As Double
                If Not TryGetFiniteDouble(lowerAllowableLimit, lowerValue) OrElse Not TryGetFiniteDouble(upperAllowableLimit, upperValue) Then
                    Return ExcelError.ExcelErrorValue
                End If
                If lowerValue > upperValue Then Return ExcelError.ExcelErrorNum

                Dim names() As String = Nothing
                Dim fit As Agreement.BlandAltmanResult = FitBlandAltmanFromUdfArgs(x, y, subjectIds, alpha, mode, scale, xAxis, ciMethod,
                                                                                   bootstrapReplicates, useT, minSubjects, minPairsPerSubject,
                                                                                   excludeSingletonSubjects, allowFallbackToSimple, checkProportionalBias,
                                                                                   plotMode, randomSeed, varNames, names)
                If fit Is Nothing Then Return ExcelError.ExcelErrorValue

                Dim assessment As BlandAltmanDecisionLimitAssessmentResult = EquivalenceNonInferiorityMethods.AssessBlandAltmanAgainstDecisionLimits(fit, lowerValue, upperValue)
                Dim body As Object(,) = {
                    {"Reference method", names(0)},
                    {"Test method", names(1)},
                    {"Model actually used", If(fit.UsedRepeatedModel, "Repeated-measures Bland–Altman", "Ordinary paired Bland–Altman")},
                    {"Difference scale", DescribeBlandScale(ParseBlandScale(scale, Agreement.BlandAltmanScale.RawDifference))},
                    {"Bias estimate", fit.BiasCI.Estimate},
                    {"Bias confidence interval", SafeCiText(fit.BiasCI)},
                    {"Lower limit of agreement", fit.LowerLoACI.Estimate},
                    {"Lower LoA confidence interval", SafeCiText(fit.LowerLoACI)},
                    {"Upper limit of agreement", fit.UpperLoACI.Estimate},
                    {"Upper LoA confidence interval", SafeCiText(fit.UpperLoACI)},
                    {"Lower allowable limit", assessment.LowerAllowableLimit},
                    {"Upper allowable limit", assessment.UpperAllowableLimit},
                    {"Bias confidence interval within allowable limits", assessment.BiasAssessment.IsConfidenceIntervalWithinMargins},
                    {"Observed limits of agreement within allowable limits", assessment.AreObservedLoAWithinAllowableLimits},
                    {"LoA confidence intervals within allowable limits", assessment.AreLoAConfidenceIntervalsWithinAllowableLimits},
                    {"Conclusion", assessment.Conclusion}
                }
                Return BuildResultTable("Bland–Altman Decision-Limit Assessment", body)
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.BLANDALTMAN_DECISION", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns a spillable labeled result table for Lin's concordance correlation coefficient (CCC).
        ''' </summary>
        ''' <param name="x">One-column range of reference-method values paired row-by-row with <paramref name="y"/>.</param>
        ''' <param name="y">One-column range of test-method values paired row-by-row with <paramref name="x"/>.</param>
        ''' <param name="alpha">Optional two-sided significance level used for confidence intervals. Default 0.05.</param>
        ''' <param name="ciMethod">Optional confidence-interval method: <c>analytical</c>, <c>jackknife</c>, <c>bootstrap</c>, or <c>bca</c>.</param>
        ''' <param name="nullConcordance">Optional null concordance value used for the hypothesis test. Default 0.</param>
        ''' <param name="bootstrapReplicates">Optional bootstrap replicate count. Default 2000.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap resampling.</param>
        ''' <param name="varNames">Optional display names for the two methods.</param>
        ''' <returns>A two-dimensional spill range containing a full Lin CCC report.</returns>
        ''' <remarks>
        ''' <para>
        ''' Lin's concordance correlation coefficient combines precision and accuracy into a single agreement statistic:
        ''' <c>rho_c = rho * C_b</c>, where <c>rho</c> is Pearson correlation and <c>C_b</c> is a bias-correction factor that shrinks the value when the fitted points deviate from the 45-degree line of equality.
        ''' </para>
        ''' <para>
        ''' An equivalent moment-based formula is
        ''' <c>rho_c = 2*s_xy / (s_x^2 + s_y^2 + (xbar - ybar)^2)</c>.
        ''' </para>
        ''' <para>
        ''' Use this function when you want a full worksheet report containing the concordance estimate, decomposition into precision and accuracy,
        ''' confidence interval, and hypothesis test. Missing or non-numeric pairs are removed pairwise before fitting.
        ''' </para>
        ''' <para>
        ''' The procedure is intended for paired continuous measurements on the same items. It does not replace a full repeated-measures concordance model.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.LINCCC_FIT",
            Category:="BESHStatNG - Agreement",
            Description:="Lin's concordance correlation coefficient for two paired methods. Returns a labeled result table.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function LINCCC_FIT(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range. First cell may be a header.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range. First cell may be a header.")> y As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="ciMethod", Description:="Optional CI method: analytical | jackknife | bootstrap | bca.")> Optional ciMethod As Object = Nothing,
            <ExcelArgument(Name:="nullConcordance", Description:="Optional null concordance for the hypothesis test. Default 0.")> Optional nullConcordance As Object = Nothing,
            <ExcelArgument(Name:="bootstrapReplicates", Description:="Optional bootstrap replicate count. Default 2000.")> Optional bootstrapReplicates As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed used by bootstrap CI methods.")> Optional randomSeed As Object = Nothing,
            <ExcelArgument(Name:="varNames", Description:="Optional names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not UDFhelpers.TryReadPairedNumericColumns(x, y, mat, detectedNames) Then Return ExcelError.ExcelErrorValue
                If mat Is Nothing OrElse mat.GetLength(0) < 3 Then Return ExcelError.ExcelErrorNum
                Dim names() As String = ParametricUDFs.ResolveNames(varNames, detectedNames, 2, "Method")

                Dim opts As New Agreement.LinConcordanceOptions With {
                    .Alpha = ParseAlphaOrDefault(alpha, 0.05),
                    .CiMethod = ParseAgreementCiMethod(ciMethod, Agreement.AgreementCiMethod.Analytical),
                    .NullConcordance = UDFhelpers.GetOptionalDouble(nullConcordance, 0.0),
                    .BootstrapReplicates = Math.Max(200, UDFhelpers.GetOptionalInt(bootstrapReplicates, 2000))
                }
                Dim mdl As New Agreement.LinConcordanceCorrelation(Matrix.GetColumnFrom2Darray(mat, 0), Matrix.GetColumnFrom2Darray(mat, 1), names(0), names(1), opts)
                mdl.Fit(Nothing, ParseOptionalSeed(randomSeed))
                Return StackResultTables(mdl.wrapResults())
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.LINCCC_FIT", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns the numerical value of Lin's concordance correlation coefficient together with interval information.
        ''' </summary>
        ''' <param name="x">One-column range of reference-method values.</param>
        ''' <param name="y">One-column range of test-method values.</param>
        ''' <param name="alpha">Optional two-sided significance level. Default 0.05.</param>
        ''' <param name="ciMethod">Optional confidence-interval method.</param>
        ''' <param name="nullConcordance">Optional null concordance value for the associated hypothesis test.</param>
        ''' <param name="bootstrapReplicates">Optional bootstrap replicate count.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap resampling.</param>
        ''' <returns>A compact spill range containing the concordance estimate, confidence interval, and related scalar summaries.</returns>
        ''' <remarks>
        ''' <para>
        ''' This is the compact numeric companion to <c>BESH.AGREE.LINCCC_FIT</c>. The main estimand is
        ''' <c>rho_c = rho * C_b</c>, where values close to 1 indicate strong agreement and values near 0 indicate poor concordance.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.LINCCC_VALUE",
            Category:="BESHStatNG - Agreement",
            Description:="Lin's concordance correlation coefficient value for two paired methods.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function LINCCC_VALUE(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Reference method values as a single-column range.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Test method values as a single-column range.")> y As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="ciMethod", Description:="Optional CI method.")> Optional ciMethod As Object = Nothing,
            <ExcelArgument(Name:="nullConcordance", Description:="Optional null concordance for the hypothesis test.")> Optional nullConcordance As Object = Nothing,
            <ExcelArgument(Name:="bootstrapReplicates", Description:="Optional bootstrap replicate count.")> Optional bootstrapReplicates As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed used by bootstrap CI methods.")> Optional randomSeed As Object = Nothing) As Object
            Try
                Dim mat(,) As Double = Nothing
                Dim detectedNames() As String = Nothing
                If Not UDFhelpers.TryReadPairedNumericColumns(x, y, mat, detectedNames) Then Return ExcelError.ExcelErrorValue
                If mat Is Nothing OrElse mat.GetLength(0) < 3 Then Return ExcelError.ExcelErrorNum

                Dim opts As New Agreement.LinConcordanceOptions With {
                    .Alpha = ParseAlphaOrDefault(alpha, 0.05),
                    .CiMethod = ParseAgreementCiMethod(ciMethod, Agreement.AgreementCiMethod.Analytical),
                    .NullConcordance = UDFhelpers.GetOptionalDouble(nullConcordance, 0.0),
                    .BootstrapReplicates = Math.Max(200, UDFhelpers.GetOptionalInt(bootstrapReplicates, 2000))
                }
                Dim mdl As New Agreement.LinConcordanceCorrelation(Matrix.GetColumnFrom2Darray(mat, 0), Matrix.GetColumnFrom2Darray(mat, 1), "Reference", "Test", opts)
                Dim res = mdl.Fit(Nothing, ParseOptionalSeed(randomSeed))
                Return res.ConcordanceCI.Estimate
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.LINCCC_VALUE", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function


        ''' <summary>
        ''' Returns a spillable labeled result table for Cohen's kappa or weighted kappa for paired ratings.
        ''' </summary>
        ''' <param name="rater1">One-column range containing the first set of paired ratings. The first cell may be a header.</param>
        ''' <param name="rater2">One-column range containing the second set of paired ratings. The first cell may be a header.</param>
        ''' <param name="alpha">Optional two-sided significance level used for confidence intervals. Default 0.05.</param>
        ''' <param name="weighting">
        ''' Optional weighting scheme. Common choices are <c>unweighted</c>, <c>linear</c>, <c>quadratic</c>, <c>cicchetti</c>, and <c>fleiss</c>.
        ''' In theory a <c>custom</c> scheme exists, but this UDF does not expose a custom weight matrix argument, so a user-specified custom matrix is not available here.
        ''' </param>
        ''' <param name="ciMethod">Optional confidence-interval method: <c>analytical</c>, <c>bootstrap</c>, or <c>bca</c>.</param>
        ''' <param name="categories">
        ''' Optional ordered category list supplied as comma-separated text or a small range. Use this when the category order matters for weighted kappa,
        ''' for example ordinal scales such as <c>Low,Medium,High</c>.
        ''' </param>
        ''' <param name="bootstrapReplicates">Optional bootstrap replicate count. Default 2000.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap resampling.</param>
        ''' <param name="varNames">Optional display names for the two raters or methods.</param>
        ''' <returns>A two-dimensional spill range containing a full kappa report.</returns>
        ''' <remarks>
        ''' <para>
        ''' Unweighted Cohen's kappa is
        ''' <c>kappa = (P_o - P_e) / (1 - P_e)</c>,
        ''' where <c>P_o</c> is the observed agreement proportion and <c>P_e</c> is the expected agreement under independence of the two raters.
        ''' </para>
        ''' <para>
        ''' Weighted kappa generalizes this to
        ''' <c>kappa_w = (P_o^w - P_e^w) / (1 - P_e^w)</c>,
        ''' where disagreements closer to the diagonal receive partial credit through a weight matrix.
        ''' </para>
        ''' <para>
        ''' Use weighted kappa for ordinal categories where near-disagreements are less severe than far-apart disagreements.
        ''' Missing or blank category pairs are removed pairwise before fitting.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.KAPPA_FIT",
            Category:="BESHStatNG - Agreement",
            Description:="Cohen's / weighted kappa for two paired rating columns. Returns a labeled result table.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function KAPPA_FIT(
            <ExcelArgument(AllowReference:=True, Name:="rater1", Description:="First rating column. First cell may be a header.")> rater1 As Object,
            <ExcelArgument(AllowReference:=True, Name:="rater2", Description:="Second rating column. First cell may be a header.")> rater2 As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="weighting", Description:="Optional weighting: unweighted | linear | quadratic | cicchetti | fleiss | custom.")> Optional weighting As Object = Nothing,
            <ExcelArgument(Name:="ciMethod", Description:="Optional CI method: analytical | bootstrap | bca.")> Optional ciMethod As Object = Nothing,
            <ExcelArgument(Name:="categories", Description:="Optional ordered categories as comma-separated text or 1-row/1-column range.")> Optional categories As Object = Nothing,
            <ExcelArgument(Name:="bootstrapReplicates", Description:="Optional bootstrap replicate count. Default 2000.")> Optional bootstrapReplicates As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed used by bootstrap CI methods.")> Optional randomSeed As Object = Nothing,
            <ExcelArgument(Name:="varNames", Description:="Optional names as comma-separated text or 1-row/1-column range.")> Optional varNames As Object = Nothing) As Object
            Try
                Dim cat = ReadPairedCategoricalColumns(rater1, rater2)
                If cat.Error.HasValue Then Return cat.Error.Value
                If cat.X Is Nothing OrElse cat.X.Length < 2 Then Return ExcelError.ExcelErrorNum
                Dim names() As String = ParametricUDFs.ResolveNames(varNames, cat.DetectedNames, 2, "Rater")

                Dim opts As New Agreement.KappaOptions With {
                    .Alpha = ParseAlphaOrDefault(alpha, 0.05),
                    .Weighting = ParseKappaWeighting(weighting, Agreement.KappaWeightingScheme.Quadratic),
                    .CiMethod = ParseAgreementCiMethod(ciMethod, Agreement.AgreementCiMethod.Analytical),
                    .BootstrapReplicates = Math.Max(200, UDFhelpers.GetOptionalInt(bootstrapReplicates, 2000))
                }
                Dim cats() As Object = Nothing
                If TryParseCategoryList(categories, cats) Then opts.Categories = cats

                Dim mdl As New Agreement.WeightedKappaAgreement(cat.X, cat.Y, names(0), names(1), opts)
                mdl.Fit(Nothing, ParseOptionalSeed(randomSeed))
                Return StackResultTables(mdl.wrapResults())
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.KAPPA_FIT", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns the numerical value of Cohen's kappa or weighted kappa together with interval information.
        ''' </summary>
        ''' <param name="rater1">One-column range containing the first set of paired ratings.</param>
        ''' <param name="rater2">One-column range containing the second set of paired ratings.</param>
        ''' <param name="alpha">Optional two-sided significance level used for confidence intervals. Default 0.05.</param>
        ''' <param name="weighting">Optional weighting scheme.</param>
        ''' <param name="ciMethod">Optional confidence-interval method.</param>
        ''' <param name="categories">Optional ordered category list for ordinal weighting.</param>
        ''' <param name="bootstrapReplicates">Optional bootstrap replicate count.</param>
        ''' <param name="randomSeed">Optional integer seed for reproducible bootstrap resampling.</param>
        ''' <returns>A compact spill range containing the kappa estimate, confidence interval, and key scalar summaries.</returns>
        ''' <remarks>
        ''' <para>
        ''' Kappa measures agreement beyond chance for paired categorical ratings on the same items.
        ''' Values near 1 indicate strong agreement, values near 0 indicate agreement comparable to chance, and negative values indicate worse-than-chance agreement.
        ''' </para>
        ''' <para>
        ''' Weighted versions are intended for ordinal ratings where disagreements have different severities.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.KAPPA_VALUE",
            Category:="BESHStatNG - Agreement",
            Description:="Cohen's / weighted kappa value for two paired rating columns.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function KAPPA_VALUE(
            <ExcelArgument(AllowReference:=True, Name:="rater1", Description:="First rating column.")> rater1 As Object,
            <ExcelArgument(AllowReference:=True, Name:="rater2", Description:="Second rating column.")> rater2 As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="weighting", Description:="Optional weighting scheme.")> Optional weighting As Object = Nothing,
            <ExcelArgument(Name:="ciMethod", Description:="Optional CI method.")> Optional ciMethod As Object = Nothing,
            <ExcelArgument(Name:="categories", Description:="Optional ordered categories.")> Optional categories As Object = Nothing,
            <ExcelArgument(Name:="bootstrapReplicates", Description:="Optional bootstrap replicate count.")> Optional bootstrapReplicates As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional random seed used by bootstrap CI methods.")> Optional randomSeed As Object = Nothing) As Object
            Try
                Dim cat = ReadPairedCategoricalColumns(rater1, rater2)
                If cat.Error.HasValue Then Return cat.Error.Value
                If cat.X Is Nothing OrElse cat.X.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim opts As New Agreement.KappaOptions With {
                    .Alpha = ParseAlphaOrDefault(alpha, 0.05),
                    .Weighting = ParseKappaWeighting(weighting, Agreement.KappaWeightingScheme.Quadratic),
                    .CiMethod = ParseAgreementCiMethod(ciMethod, Agreement.AgreementCiMethod.Analytical),
                    .BootstrapReplicates = Math.Max(200, UDFhelpers.GetOptionalInt(bootstrapReplicates, 2000))
                }
                Dim cats() As Object = Nothing
                If TryParseCategoryList(categories, cats) Then opts.Categories = cats

                Dim mdl As New Agreement.WeightedKappaAgreement(cat.X, cat.Y, "Rater 1", "Rater 2", opts)
                Dim res = mdl.Fit(Nothing, ParseOptionalSeed(randomSeed))
                Return res.KappaCI.Estimate
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.KAPPA_VALUE", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns a spillable labeled result table for an intraclass correlation coefficient (ICC) model.
        ''' </summary>
        ''' <param name="data">
        ''' Numeric matrix containing repeated measurements.
        ''' </param>
        ''' <param name="model">
        ''' ICC model identifier. Supported values are:
        ''' <list type="bullet">
        ''' <item><description><c>ICC11</c> or <c>ICC(1,1)</c> — one-way random effects, single measurement.</description></item>
        ''' <item><description><c>ICC1K</c> or <c>ICC(1,k)</c> — one-way random effects, mean of k measurements.</description></item>
        ''' <item><description><c>ICC21</c> or <c>ICC(2,1)</c> — two-way random effects, absolute agreement, single measurement.</description></item>
        ''' <item><description><c>ICC2K</c> or <c>ICC(2,k)</c> — two-way random effects, absolute agreement, mean of k measurements.</description></item>
        ''' <item><description><c>ICC31</c> or <c>ICC(3,1)</c> — two-way mixed effects, consistency, single measurement.</description></item>
        ''' <item><description><c>ICC3K</c> or <c>ICC(3,k)</c> — two-way mixed effects, consistency, mean of k measurements.</description></item>
        ''' </list>
        ''' The default is <c>ICC21</c>.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <param name="includeRepeatability">
        ''' TRUE to append the repeatability coefficient (RC) and SEM to the spill output.
        ''' FALSE returns the ICC estimate and its confidence interval only.
        ''' The default is FALSE.
        ''' </param>
        ''' <returns>
        ''' A two-dimensional spill range containing a labeled result table for the requested ICC model.
        ''' Returns <c>#VALUE!</c> when the input shape is invalid or contains incompatible non-numeric cells.
        ''' Returns <c>#NUM!</c> when the requested ICC cannot be estimated from the supplied data.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Intraclass correlation coefficients measure the reliability or agreement of repeated measurements made on the same targets.
        ''' The meaning of the coefficient depends on the selected model:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description><b>ICC(1,·)</b> assumes a one-way random-effects design where rows are targets and columns are exchangeable repeated measurements. Missing cells are allowed; each row may contain a different number of usable measurements.</description></item>
        ''' <item><description><b>ICC(2,·)</b> assumes a complete balanced two-way random-effects design with rows = targets and columns = raters/replicates. This is the usual choice for absolute agreement when raters are considered random.</description></item>
        ''' <item><description><b>ICC(3,·)</b> assumes a complete balanced two-way mixed-effects design with rows = targets and columns = fixed raters/replicates. This is the usual choice for consistency rather than absolute agreement.</description></item>
        ''' </list>
        ''' <para>
        ''' The one-way single-measure coefficient uses the form
        ''' <c>ICC(1,1) = (MSB - MSW) / (MSB + (n0 - 1) MSW)</c>,
        ''' where <c>MSB</c> is the between-target mean square, <c>MSW</c> is the within-target mean square,
        ''' and <c>n0</c> is the effective group size for unbalanced data.
        ''' </para>
        ''' <para>
        ''' The two-way random-effects single-measure coefficient uses
        ''' <c>ICC(2,1) = (MSR - MSE) / (MSR + (k-1)MSE + k(MSC - MSE)/n)</c>,
        ''' where <c>MSR</c> is the target mean square, <c>MSC</c> is the rater mean square,
        ''' <c>MSE</c> is the residual mean square, <c>n</c> is the number of targets, and <c>k</c> is the number of raters.
        ''' </para>
        ''' <para>
        ''' The two-way mixed-effects consistency single-measure coefficient uses
        ''' <c>ICC(3,1) = (MSR - MSE) / (MSR + (k-1)MSE)</c>.
        ''' The average-measure versions transform these models to the reliability of the mean of k measurements.
        ''' </para>
        ''' <para>
        ''' Confidence intervals are F-based and therefore generally asymmetric. Lower confidence limits may be negative.
        ''' </para>
        ''' <para>
        ''' Data layout:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description>For ICC(1,1) and ICC(1,k), each row is one target/subject and each column is a repeated measurement. Blank cells are allowed and are treated as missing.</description></item>
        ''' <item><description>For ICC(2,1), ICC(2,k), ICC(3,1), and ICC(3,k), the matrix must be complete and balanced: one numeric value in every row × column cell.</description></item>
        ''' <item><description>An optional single top header row containing text labels is allowed and is ignored for the calculation.</description></item>
        ''' </list>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.ICC_FIT",
            Category:="BESHStatNG - Agreement",
            Description:="Intraclass correlation coefficient (ICC) result table for a selected ICC model.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function ICC_FIT(
            <ExcelArgument(AllowReference:=True, Name:="data", Description:="Numeric matrix of repeated measurements. Optional top header row allowed.")> data As Object,
            <ExcelArgument(Name:="model", Description:="ICC model: ICC11, ICC1K, ICC21, ICC2K, ICC31, or ICC3K. Default ICC21.")> Optional model As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="includeRepeatability", Description:="TRUE to append repeatability coefficient and SEM to the output. Default FALSE.")> Optional includeRepeatability As Object = Nothing) As Object
            Try
                Dim modelCode As String = ParseIccModel(model, "ICC21")
                Dim alphaValue As Double = ParseAlphaOrDefault(alpha, 0.05R)
                Dim addRc As Boolean = UDFhelpers.GetOptionalBool(includeRepeatability, False)

                Dim icc As New Agreement.IntraclassCorrelation
                Dim result As ConfidenceIntervalResult = Nothing
                Dim typeLabel As String = String.Empty

                If IsOneWayIcc(modelCode) Then
                    Dim grouped()() As Double = Nothing
                    If Not TryReadOneWayIccMatrix(data, grouped) Then Return ExcelError.ExcelErrorValue
                    Select Case modelCode
                        Case "ICC11"
                            result = icc.ICC11(grouped, alphaValue)
                            If addRc Then icc.RepeatabilityCoefficient_OneWay(grouped, False, alphaValue)
                            typeLabel = "ICC(1,1)"
                        Case "ICC1K"
                            result = icc.ICC1k(grouped, alphaValue)
                            If addRc Then icc.RepeatabilityCoefficient_OneWay(grouped, True, alphaValue)
                            typeLabel = "ICC(1,k)"
                        Case Else
                            Return ExcelError.ExcelErrorValue
                    End Select
                Else
                    Dim mat(,) As Double = Nothing
                    Dim names() As String = Nothing
                    If Not UDFhelpers.TryReadCompleteNumericMatrixWithHeaders(data, mat, names) Then Return ExcelError.ExcelErrorValue
                    Select Case modelCode
                        Case "ICC21"
                            result = icc.ICC21(mat, alphaValue)
                            If addRc Then icc.RepeatabilityCoefficient_TwoWay(mat, True, False, alphaValue)
                            typeLabel = "ICC(2,1)"
                        Case "ICC2K"
                            result = icc.ICC2k(mat, alphaValue)
                            If addRc Then icc.RepeatabilityCoefficient_TwoWay(mat, True, True, alphaValue)
                            typeLabel = "ICC(2,k)"
                        Case "ICC31"
                            result = icc.ICC31(mat, alphaValue)
                            If addRc Then icc.RepeatabilityCoefficient_TwoWay(mat, False, False, alphaValue)
                            typeLabel = "ICC(3,1)"
                        Case "ICC3K"
                            result = icc.ICC3k(mat, alphaValue)
                            If addRc Then icc.RepeatabilityCoefficient_TwoWay(mat, False, True, alphaValue)
                            typeLabel = "ICC(3,k)"
                        Case Else
                            Return ExcelError.ExcelErrorValue
                    End Select
                End If

                Dim tables As List(Of ResultTable) = icc.wrapResults(result, typeLabel)
                If tables Is Nothing OrElse tables.Count = 0 Then Return ExcelError.ExcelErrorNum
                Return PrepareResultTableForUdf(tables(0).returnSelf())
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.ICC_FIT", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns only the numerical value of an intraclass correlation coefficient (ICC) model.
        ''' </summary>
        ''' <param name="data">
        ''' Numeric matrix of repeated measurements. For one-way ICC models, blank cells are allowed and are treated as missing.
        ''' For two-way ICC models, the matrix must be complete and balanced.
        ''' </param>
        ''' <param name="model">
        ''' ICC model identifier: <c>ICC11</c>, <c>ICC1K</c>, <c>ICC21</c>, <c>ICC2K</c>, <c>ICC31</c>, or <c>ICC3K</c>.
        ''' The default is <c>ICC21</c>.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level.
        ''' The alpha value affects the confidence interval used internally by the fit routine but this function returns only the point estimate.
        ''' The default is 0.05.
        ''' </param>
        ''' <returns>
        ''' The point estimate of the requested ICC model as a scalar numeric value.
        ''' Returns <c>#VALUE!</c> for invalid input and <c>#NUM!</c> when the model is not estimable from the supplied data.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function is intended for formulas that need only the ICC estimate itself. The underlying formulas depend on the selected ICC family:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description><c>ICC(1,1)</c> — one-way random-effects single-measure reliability.</description></item>
        ''' <item><description><c>ICC(1,k)</c> — one-way random-effects average-measure reliability.</description></item>
        ''' <item><description><c>ICC(2,1)</c> — two-way random-effects single-measure absolute agreement.</description></item>
        ''' <item><description><c>ICC(2,k)</c> — two-way random-effects average-measure absolute agreement.</description></item>
        ''' <item><description><c>ICC(3,1)</c> — two-way mixed-effects single-measure consistency.</description></item>
        ''' <item><description><c>ICC(3,k)</c> — two-way mixed-effects average-measure consistency.</description></item>
        ''' </list>
        ''' <para>
        ''' Values near 1 indicate strong reliability / agreement; values near 0 indicate weak reliability; negative values can occur when within-target variation dominates between-target variation.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.ICC_VALUE",
            Category:="BESHStatNG - Agreement",
            Description:="Point estimate of a selected intraclass correlation coefficient (ICC) model.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function ICC_VALUE(
            <ExcelArgument(AllowReference:=True, Name:="data", Description:="Numeric matrix of repeated measurements. Optional top header row allowed.")> data As Object,
            <ExcelArgument(Name:="model", Description:="ICC model: ICC11, ICC1K, ICC21, ICC2K, ICC31, or ICC3K. Default ICC21.")> Optional model As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing) As Object
            Try
                Dim modelCode As String = ParseIccModel(model, "ICC21")
                Dim alphaValue As Double = ParseAlphaOrDefault(alpha, 0.05R)
                Dim icc As New Agreement.IntraclassCorrelation
                Dim result As ConfidenceIntervalResult = Nothing

                If IsOneWayIcc(modelCode) Then
                    Dim grouped()() As Double = Nothing
                    If Not TryReadOneWayIccMatrix(data, grouped) Then Return ExcelError.ExcelErrorValue
                    Select Case modelCode
                        Case "ICC11" : result = icc.ICC11(grouped, alphaValue)
                        Case "ICC1K" : result = icc.ICC1k(grouped, alphaValue)
                        Case Else : Return ExcelError.ExcelErrorValue
                    End Select
                Else
                    Dim mat(,) As Double = Nothing
                    Dim names() As String = Nothing
                    If Not UDFhelpers.TryReadCompleteNumericMatrixWithHeaders(data, mat, names) Then Return ExcelError.ExcelErrorValue
                    Select Case modelCode
                        Case "ICC21" : result = icc.ICC21(mat, alphaValue)
                        Case "ICC2K" : result = icc.ICC2k(mat, alphaValue)
                        Case "ICC31" : result = icc.ICC31(mat, alphaValue)
                        Case "ICC3K" : result = icc.ICC3k(mat, alphaValue)
                        Case Else : Return ExcelError.ExcelErrorValue
                    End Select
                End If

                If result Is Nothing OrElse Double.IsNaN(result.Estimate) OrElse Double.IsInfinity(result.Estimate) Then Return ExcelError.ExcelErrorNum
                Return result.Estimate
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.ICC_VALUE", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns the repeatability coefficient (RC), its confidence interval, and the SEM for a selected ICC design.
        ''' </summary>
        ''' <param name="data">
        ''' Numeric matrix of repeated measurements. For one-way ICC models, blank cells are allowed and are treated as missing.
        ''' For two-way ICC models, the matrix must be complete and balanced.
        ''' </param>
        ''' <param name="model">
        ''' ICC model identifier. The repeatability calculation is mapped as follows:
        ''' <list type="bullet">
        ''' <item><description><c>ICC11</c> → one-way single-measure RC and SEM.</description></item>
        ''' <item><description><c>ICC1K</c> → one-way average-measure RC and SEM.</description></item>
        ''' <item><description><c>ICC21</c> → two-way absolute-agreement single-measure RC and SEM.</description></item>
        ''' <item><description><c>ICC2K</c> → two-way absolute-agreement average-measure RC and SEM.</description></item>
        ''' <item><description><c>ICC31</c> → two-way consistency single-measure RC and SEM.</description></item>
        ''' <item><description><c>ICC3K</c> → two-way consistency average-measure RC and SEM.</description></item>
        ''' </list>
        ''' The default is <c>ICC21</c>.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the confidence interval.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' A spillable labeled table containing the repeatability coefficient, its confidence interval, and the SEM.
        ''' Returns <c>#VALUE!</c> when the input is invalid and <c>#NUM!</c> when the repeatability quantity is not estimable from the supplied data.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The repeatability coefficient summarizes the expected absolute difference between repeated measurements on the same target.
        ''' It is defined as
        ''' <c>RC = z_(1-α/2) × √2 × SEM</c>,
        ''' where <c>SEM</c> is the standard error of measurement implied by the selected ICC design.
        ''' </para>
        ''' <para>
        ''' For one-way ICC models, <c>SEM</c> is based on the within-target variance component. For average-measure models the variance of the mean is used.
        ''' For two-way models, the repeatability calculation can either include rater variance (absolute agreement) or exclude it (consistency), again with optional averaging across k raters.
        ''' </para>
        ''' <para>
        ''' This function is useful when you need an agreement quantity in measurement units rather than a unitless reliability coefficient.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.AGREE.ICC_RC",
            Category:="BESHStatNG - Agreement",
            Description:="Repeatability coefficient (RC) and SEM for a selected ICC design.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/agreement/")>
        Public Function ICC_RC(
            <ExcelArgument(AllowReference:=True, Name:="data", Description:="Numeric matrix of repeated measurements. Optional top header row allowed.")> data As Object,
            <ExcelArgument(Name:="model", Description:="ICC model: ICC11, ICC1K, ICC21, ICC2K, ICC31, or ICC3K. Default ICC21.")> Optional model As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha. Default 0.05.")> Optional alpha As Object = Nothing) As Object
            Try
                Dim modelCode As String = ParseIccModel(model, "ICC21")
                Dim alphaValue As Double = ParseAlphaOrDefault(alpha, 0.05R)
                Dim icc As New Agreement.IntraclassCorrelation
                Dim rc As ConfidenceIntervalResult = Nothing
                Dim title As String = String.Empty

                If IsOneWayIcc(modelCode) Then
                    Dim grouped()() As Double = Nothing
                    If Not TryReadOneWayIccMatrix(data, grouped) Then Return ExcelError.ExcelErrorValue
                    Select Case modelCode
                        Case "ICC11"
                            rc = icc.RepeatabilityCoefficient_OneWay(grouped, False, alphaValue)
                            title = "Repeatability coefficient for ICC(1,1)"
                        Case "ICC1K"
                            rc = icc.RepeatabilityCoefficient_OneWay(grouped, True, alphaValue)
                            title = "Repeatability coefficient for ICC(1,k)"
                        Case Else
                            Return ExcelError.ExcelErrorValue
                    End Select
                Else
                    Dim mat(,) As Double = Nothing
                    Dim names() As String = Nothing
                    If Not UDFhelpers.TryReadCompleteNumericMatrixWithHeaders(data, mat, names) Then Return ExcelError.ExcelErrorValue
                    Select Case modelCode
                        Case "ICC21"
                            rc = icc.RepeatabilityCoefficient_TwoWay(mat, True, False, alphaValue)
                            title = "Repeatability coefficient for ICC(2,1)"
                        Case "ICC2K"
                            rc = icc.RepeatabilityCoefficient_TwoWay(mat, True, True, alphaValue)
                            title = "Repeatability coefficient for ICC(2,k)"
                        Case "ICC31"
                            rc = icc.RepeatabilityCoefficient_TwoWay(mat, False, False, alphaValue)
                            title = "Repeatability coefficient for ICC(3,1)"
                        Case "ICC3K"
                            rc = icc.RepeatabilityCoefficient_TwoWay(mat, False, True, alphaValue)
                            title = "Repeatability coefficient for ICC(3,k)"
                        Case Else
                            Return ExcelError.ExcelErrorValue
                    End Select
                End If

                If rc Is Nothing Then Return ExcelError.ExcelErrorNum
                Dim t As New ResultTable
                t.SetBody(New Object(,) {
                        {"Repeatability coefficient", rc.Estimate},
                        {rc.CIlabel, rc.strConfidenceInterval(CIformat.LL_to_UL)},
                        {"SEM (standard error of measurement)", rc.StdErr}
                    })
                t.AddHeaderTopRow({title, ""})
                Return PrepareResultTableForUdf(t.returnSelf())
            Catch ex As Exception
                Return LoggedUdfError("BESH.AGREE.ICC_RC", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function


        ' =============================================================================================================
        ' Helpers
        ' =============================================================================================================
        Private Function ParseAlphaOrDefault(arg As Object, defaultValue As Double) As Double
            Dim a As Double = defaultValue
            If TryParseAlpha(arg, a) Then Return a
            Throw New ArgumentException("alpha must be in the open interval (0, 1).")
        End Function

        Private Function ParseAgreementCiMethod(arg As Object, defaultValue As Agreement.AgreementCiMethod) As Agreement.AgreementCiMethod
            Dim s As String = NormalizeToken(arg)
            If s = "" Then Return defaultValue
            Select Case s
                Case "ANALYTICAL", "ANALYTIC"
                    Return Agreement.AgreementCiMethod.Analytical
                Case "JACKKNIFE", "JACK"
                    Return Agreement.AgreementCiMethod.Jackknife
                Case "BOOTSTRAP", "PERCENTILE", "BOOTSTRAPPERCENTILE", "BOOTSTRAP_PERCENTILE"
                    Return Agreement.AgreementCiMethod.BootstrapPercentile
                Case "BCA", "BOOTSTRAPBCA", "BOOTSTRAP_BCA"
                    Return Agreement.AgreementCiMethod.BootstrapBCa
                Case Else
                    Throw New ArgumentException("Unsupported ciMethod. Use analytical, jackknife, bootstrap, or bca.")
            End Select
        End Function

        Private Function ParseBlandMode(arg As Object, defaultValue As Agreement.RepeatedBlandAltmanMode) As Agreement.RepeatedBlandAltmanMode
            Dim s As String = NormalizeToken(arg)
            If s = "" Then Return defaultValue
            Select Case s
                Case "AUTO"
                    Return Agreement.RepeatedBlandAltmanMode.Auto
                Case "SIMPLE", "PAIRS", "SIMPLEPAIRS"
                    Return Agreement.RepeatedBlandAltmanMode.SimplePairs
                Case "REPEATED", "REPEATEDBYSUBJECT", "SUBJECT"
                    Return Agreement.RepeatedBlandAltmanMode.RepeatedBySubject
                Case Else
                    Throw New ArgumentException("Unsupported Bland–Altman mode. Use auto, simple, or repeated.")
            End Select
        End Function

        Private Function ParseBlandScale(arg As Object, defaultValue As Agreement.BlandAltmanScale) As Agreement.BlandAltmanScale
            Dim s As String = NormalizeToken(arg)
            If s = "" Then Return defaultValue
            Select Case s
                Case "RAW", "RAWDIFFERENCE", "DIFF", "DIFFERENCE"
                    Return Agreement.BlandAltmanScale.RawDifference
                Case "MEANPCT", "PERCENTOFMEAN", "PCTMEAN", "PERCENTMEAN"
                    Return Agreement.BlandAltmanScale.PercentOfMean
                Case "REFPCT", "PERCENTOFREFERENCE", "PCTREF", "PERCENTREF", "REFERENCE"
                    Return Agreement.BlandAltmanScale.PercentOfReference
                Case "TESTPCT", "PERCENTOFTEST", "PCTTEST", "PERCENTTEST"
                    Return Agreement.BlandAltmanScale.PercentOfTest
                Case "LOGRATIO", "LOG", "RATIO"
                    Return Agreement.BlandAltmanScale.LogRatio
                Case Else
                    Throw New ArgumentException("Unsupported Bland–Altman scale.")
            End Select
        End Function

        Private Function ParseBlandXAxisMode(arg As Object, defaultValue As Agreement.BlandAltmanXAxisMode) As Agreement.BlandAltmanXAxisMode
            Dim s As String = NormalizeToken(arg)
            If s = "" Then Return defaultValue
            Select Case s
                Case "MEAN", "MEANOFMETHODS"
                    Return Agreement.BlandAltmanXAxisMode.MeanOfMethods
                Case "REFERENCE", "REF", "X"
                    Return Agreement.BlandAltmanXAxisMode.ReferenceMethod
                Case "TEST", "Y"
                    Return Agreement.BlandAltmanXAxisMode.TestMethod
                Case Else
                    Throw New ArgumentException("Unsupported Bland–Altman xAxis.")
            End Select
        End Function

        Private Function ParseBlandPlotMode(arg As Object, defaultValue As Agreement.RepeatedBlandAltmanPlotMode) As Agreement.RepeatedBlandAltmanPlotMode
            Dim s As String = NormalizeToken(arg)
            If s = "" Then Return defaultValue
            Select Case s
                Case "ALL", "OBS", "ALLOBSERVATIONS"
                    Return Agreement.RepeatedBlandAltmanPlotMode.AllObservations
                Case "MEANS", "SUBJECTMEANS", "MEANSONLY"
                    Return Agreement.RepeatedBlandAltmanPlotMode.SubjectMeansOnly
                Case "BOTH", "ALLANDMEANS", "ALLOBSERVATIONSANDSUBJECTMEANS"
                    Return Agreement.RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans
                Case Else
                    Throw New ArgumentException("Unsupported Bland–Altman plotMode.")
            End Select
        End Function

        Private Function ParseKappaWeighting(arg As Object, defaultValue As Agreement.KappaWeightingScheme) As Agreement.KappaWeightingScheme
            Dim s As String = NormalizeToken(arg)
            If s = "" Then Return defaultValue
            Select Case s
                Case "UNWEIGHTED", "COHEN", "NONE"
                    Return Agreement.KappaWeightingScheme.Unweighted
                Case "LINEAR"
                    Return Agreement.KappaWeightingScheme.Linear
                Case "QUADRATIC"
                    Return Agreement.KappaWeightingScheme.Quadratic
                Case "CICCHETTI", "CICCHETTIALLISON", "CA"
                    Return Agreement.KappaWeightingScheme.CicchettiAllison
                Case "FLEISS", "FLEISSCOHEN", "FC"
                    Return Agreement.KappaWeightingScheme.FleissCohen
                Case "CUSTOM"
                    Return Agreement.KappaWeightingScheme.Custom
                Case Else
                    Throw New ArgumentException("Unsupported kappa weighting scheme.")
            End Select
        End Function

        Private Function ParseDemingVarianceModel(arg As Object, defaultValue As Agreement.DemingVarianceModel) As Agreement.DemingVarianceModel
            Dim s As String = NormalizeToken(arg)
            If s = "" Then Return defaultValue
            Select Case s
                Case "LAMBDA", "CONSTANTLAMBDA", "CONSTANT"
                    Return Agreement.DemingVarianceModel.ConstantLambda
                Case "POINTWISE", "KNOWNPOINTWISESD", "SD", "POINTWISESD"
                    Return Agreement.DemingVarianceModel.KnownPointwiseSD
                Case "CV", "CONSTANTCV"
                    Return Agreement.DemingVarianceModel.ConstantCV
                Case Else
                    Throw New ArgumentException("Unsupported Deming varianceModel.")
            End Select
        End Function

        Private Function ParseOptionalNullableDouble(arg As Object) As Double
            If UDFhelpers.IsMissingArg(arg) Then Return Double.NaN
            Return UDFhelpers.GetOptionalDouble(arg, Double.NaN)
        End Function

        Private Function ParseOptionalSeed(arg As Object) As Integer
            If UDFhelpers.IsMissingArg(arg) Then Return Integer.MinValue
            Return UDFhelpers.GetOptionalInt(arg, Integer.MinValue)
        End Function

        Private Function ReadAlignedNumericWithOptionalCategory(x As Object,
                                                               y As Object,
                                                               category As Object,
                                                               requireCategory As Boolean) As (X As Double(), Y As Double(), Category As Object(), DetectedNames As String(), [Error] As ExcelError?)
            Dim ax As Object(,) = UDFhelpers.Get2D(x)
            Dim ay As Object(,) = UDFhelpers.Get2D(y)
            If ax Is Nothing OrElse ay Is Nothing Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            If ax.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

            Dim hasHeaderX As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ax)
            Dim hasHeaderY As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ay)
            If hasHeaderX <> hasHeaderY Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            Dim startRow As Integer = If(hasHeaderX, 1, 0)
            Dim names() As String = {
                    If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Reference"),
                    If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Test")
                }

            Dim ac As Object(,) = Nothing
            Dim useCategory As Boolean = Not UDFhelpers.IsMissingArg(category)
            If useCategory Then
                ac = UDFhelpers.Get2D(category)
                If ac Is Nothing OrElse ac.GetLength(1) <> 1 OrElse ac.GetLength(0) <> ax.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                Dim hasHeaderC As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ac)
                If hasHeaderC <> hasHeaderX Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            End If

            Dim xv As New List(Of Double)
            Dim yv As New List(Of Double)
            Dim cv As New List(Of Object)
            For r As Integer = startRow To ax.GetLength(0) - 1
                Dim dx = UDFhelpers.TryGetDouble(ax(r, 0))
                Dim dy = UDFhelpers.TryGetDouble(ay(r, 0))
                If dx.HasValue AndAlso dy.HasValue Then
                    xv.Add(dx.Value)
                    yv.Add(dy.Value)
                    If useCategory Then
                        Dim s As String = UDFhelpers.CellToTrimmedText(ac(r, 0))
                        If s = "" Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                        cv.Add(s)
                    End If
                End If
            Next
            If requireCategory AndAlso Not useCategory Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            Dim catOut() As Object = If(useCategory, cv.ToArray(), Nothing)
            Return (xv.ToArray(), yv.ToArray(), catOut, names, Nothing)
        End Function

        Private Function TryParseCategoryList(arg As Object, ByRef categories() As Object) As Boolean
            categories = Nothing
            If UDFhelpers.IsMissingArg(arg) Then Return False

            If TypeOf arg Is String Then
                Dim s As String = Convert.ToString(arg).Trim()
                If s = "" Then Return False
                Dim parts = s.Split({","c}, StringSplitOptions.RemoveEmptyEntries).Select(Function(t) CType(t.Trim(), Object)).ToArray()
                If parts.Length = 0 Then Return False
                categories = parts
                Return True
            End If

            Dim arr As Object(,) = UDFhelpers.Get2D(arg)
            If arr Is Nothing Then Return False
            Dim vals As New List(Of Object)
            If arr.GetLength(0) = 1 Then
                For j As Integer = 0 To arr.GetLength(1) - 1
                    Dim s As String = UDFhelpers.CellToTrimmedText(arr(0, j))
                    If s <> "" Then vals.Add(s)
                Next
            ElseIf arr.GetLength(1) = 1 Then
                For i As Integer = 0 To arr.GetLength(0) - 1
                    Dim s As String = UDFhelpers.CellToTrimmedText(arr(i, 0))
                    If s <> "" Then vals.Add(s)
                Next
            Else
                Return False
            End If
            If vals.Count = 0 Then Return False
            categories = vals.ToArray()
            Return True
        End Function

        Private Function ReadPairedCategoricalColumns(x As Object, y As Object) As (X As Object(), Y As Object(), DetectedNames As String(), [Error] As ExcelError?)
            Dim err As ExcelError? = Nothing
            Dim ax As Object(,) = UDFhelpers.Get2D(x)
            Dim ay As Object(,) = UDFhelpers.Get2D(y)
            If ax Is Nothing OrElse ay Is Nothing Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            If ax.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

            Dim hasHeaderX As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ax)
            Dim hasHeaderY As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ay)
            If hasHeaderX <> hasHeaderY Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

            Dim names() As String = {
                If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Rater 1"),
                If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Rater 2")
            }
            Dim startRow As Integer = If(hasHeaderX, 1, 0)
            Dim xs As New List(Of Object)
            Dim ys As New List(Of Object)
            For r As Integer = startRow To ax.GetLength(0) - 1
                Dim sx As String = UDFhelpers.CellToTrimmedText(ax(r, 0))
                Dim sy As String = UDFhelpers.CellToTrimmedText(ay(r, 0))
                If sx <> "" AndAlso sy <> "" Then
                    xs.Add(sx)
                    ys.Add(sy)
                End If
            Next
            If xs.Count = 0 Then Return (Nothing, Nothing, names, ExcelError.ExcelErrorNum)
            Return (xs.ToArray(), ys.ToArray(), names, Nothing)
        End Function

        Private Function ReadAlignedDemingInputs(x As Object, y As Object, sdX As Object, sdY As Object) As (X As Double(), Y As Double(), SDx As Double(), SDy As Double(), DetectedNames As String(), [Error] As ExcelError?)
            Dim ax As Object(,) = UDFhelpers.Get2D(x)
            Dim ay As Object(,) = UDFhelpers.Get2D(y)
            If ax Is Nothing OrElse ay Is Nothing Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            If ax.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

            Dim hasHeaderX As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ax)
            Dim hasHeaderY As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ay)
            If hasHeaderX <> hasHeaderY Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            Dim startRow As Integer = If(hasHeaderX, 1, 0)
            Dim names() As String = {
                If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Reference"),
                If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Test")
            }

            Dim useSdx As Boolean = Not UDFhelpers.IsMissingArg(sdX)
            Dim useSdy As Boolean = Not UDFhelpers.IsMissingArg(sdY)
            Dim asx As Object(,) = Nothing
            Dim asy As Object(,) = Nothing
            If useSdx Then
                asx = UDFhelpers.Get2D(sdX)
                If asx Is Nothing OrElse asx.GetLength(1) <> 1 OrElse asx.GetLength(0) <> ax.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                Dim hasHeaderSdx As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(asx)
                If hasHeaderSdx <> hasHeaderX Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            End If
            If useSdy Then
                asy = UDFhelpers.Get2D(sdY)
                If asy Is Nothing OrElse asy.GetLength(1) <> 1 OrElse asy.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                Dim hasHeaderSdy As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(asy)
                If hasHeaderSdy <> hasHeaderY Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            End If

            Dim xv As New List(Of Double)
            Dim yv As New List(Of Double)
            Dim sdxv As New List(Of Double)
            Dim sdyv As New List(Of Double)

            For r As Integer = startRow To ax.GetLength(0) - 1
                Dim dx = UDFhelpers.TryGetDouble(ax(r, 0))
                Dim dy = UDFhelpers.TryGetDouble(ay(r, 0))
                If dx.HasValue AndAlso dy.HasValue Then
                    xv.Add(dx.Value)
                    yv.Add(dy.Value)
                    If useSdx Then
                        Dim sx = UDFhelpers.TryGetDouble(asx(r, 0))
                        If Not sx.HasValue Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                        sdxv.Add(sx.Value)
                    End If
                    If useSdy Then
                        Dim sy = UDFhelpers.TryGetDouble(asy(r, 0))
                        If Not sy.HasValue Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                        sdyv.Add(sy.Value)
                    End If
                End If
            Next

            Dim sdxOut() As Double = If(useSdx, sdxv.ToArray(), Nothing)
            Dim sdyOut() As Double = If(useSdy, sdyv.ToArray(), Nothing)
            Return (xv.ToArray(), yv.ToArray(), sdxOut, sdyOut, names, Nothing)
        End Function

        Private Function BuildBlandPlotDataTable(res As Agreement.BlandAltmanResult) As Object(,)
            Dim nObs As Integer = If(res.PlotX Is Nothing, 0, res.PlotX.Length)
            Dim nMeans As Integer = If(res.SubjectMeanPlotX Is Nothing, 0, res.SubjectMeanPlotX.Length)
            Dim rows As Integer = Math.Max(nObs, nMeans) + 1
            Dim out(rows - 1, 3) As Object
            out(0, 0) = "PlotX"
            out(0, 1) = "PlotY"
            out(0, 2) = "SubjectMeanX"
            out(0, 3) = "SubjectMeanY"
            For i As Integer = 0 To rows - 2
                If i < nObs Then
                    out(i + 1, 0) = res.PlotX(i)
                    out(i + 1, 1) = res.PlotY(i)
                Else
                    out(i + 1, 0) = ""
                    out(i + 1, 1) = ""
                End If
                If i < nMeans Then
                    out(i + 1, 2) = res.SubjectMeanPlotX(i)
                    out(i + 1, 3) = res.SubjectMeanPlotY(i)
                Else
                    out(i + 1, 2) = ""
                    out(i + 1, 3) = ""
                End If
            Next
            Return out
        End Function

        Private Function ParseIccModel(arg As Object, defaultValue As String) As String
            Dim s As String = NormalizeToken(arg)
            If s = "" Then Return defaultValue
            Select Case s
                Case "ICC11", "11"
                    Return "ICC11"
                Case "ICC1K", "1K"
                    Return "ICC1K"
                Case "ICC21", "21"
                    Return "ICC21"
                Case "ICC2K", "2K"
                    Return "ICC2K"
                Case "ICC31", "31"
                    Return "ICC31"
                Case "ICC3K", "3K"
                    Return "ICC3K"
                Case Else
                    Throw New ArgumentException("Unsupported ICC model. Use ICC11, ICC1K, ICC21, ICC2K, ICC31, or ICC3K.")
            End Select
        End Function

        Private Function IsOneWayIcc(modelCode As String) As Boolean
            Return modelCode = "ICC11" OrElse modelCode = "ICC1K"
        End Function

        Private Function TryReadOneWayIccMatrix(input As Object, ByRef groups()() As Double) As Boolean
            groups = Nothing
            Dim arr As Object(,) = UDFhelpers.Get2D(input)
            If arr Is Nothing Then Return False

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            If rows < 1 OrElse cols < 1 Then Return False

            Dim lastRow As Integer = UDFhelpers.FindLastNonBlankRow(arr)
            If lastRow < 0 Then Return False

            Dim lastCol As Integer = FindLastNonBlankCol(arr, lastRow)
            If lastCol < 0 Then Return False

            Dim numericCols As Integer() = Enumerable.Range(0, lastCol + 1).ToArray()
            Dim hasHeader As Boolean = UDFhelpers.LooksLikeHeaderRow(arr, numericCols)
            Dim startRow As Integer = If(hasHeader, 1, 0)
            If startRow > lastRow Then Return False

            Dim out As New List(Of Double())
            For r As Integer = startRow To lastRow
                Dim rowVals As New List(Of Double)
                Dim sawAnyCell As Boolean = False
                For c As Integer = 0 To lastCol
                    Dim cell As Object = arr(r, c)
                    If IsBlankCellLocal(cell) Then Continue For
                    sawAnyCell = True
                    Dim d As Double? = UDFhelpers.TryGetDouble(cell)
                    If Not d.HasValue Then Return False
                    rowVals.Add(d.Value)
                Next
                If sawAnyCell AndAlso rowVals.Count > 0 Then out.Add(rowVals.ToArray())
            Next

            If out.Count < 2 Then Return False
            groups = out.ToArray()
            Return True
        End Function

        Private Function FindLastNonBlankCol(arr As Object(,), lastRow As Integer) As Integer
            For c As Integer = arr.GetLength(1) - 1 To 0 Step -1
                For r As Integer = 0 To lastRow
                    If Not IsBlankCellLocal(arr(r, c)) Then Return c
                Next
            Next
            Return -1
        End Function

        Private Function IsBlankCellLocal(cell As Object) As Boolean
            If cell Is Nothing OrElse TypeOf cell Is ExcelEmpty OrElse TypeOf cell Is ExcelMissing Then Return True
            If TypeOf cell Is String Then Return String.IsNullOrWhiteSpace(CStr(cell))
            Return False
        End Function

        Private Function DescribeBlandScale(scale As Agreement.BlandAltmanScale) As String
            Select Case scale
                Case Agreement.BlandAltmanScale.RawDifference
                    Return "Raw difference"
                Case Agreement.BlandAltmanScale.PercentOfMean
                    Return "% of paired mean"
                Case Agreement.BlandAltmanScale.PercentOfReference
                    Return "% of reference method"
                Case Agreement.BlandAltmanScale.PercentOfTest
                    Return "% of test method"
                Case Agreement.BlandAltmanScale.LogRatio
                    Return "Log ratio"
                Case Else
                    Return scale.ToString()
            End Select
        End Function

        Private Function FitBlandAltmanFromUdfArgs(x As Object,
                                                   y As Object,
                                                   subjectIds As Object,
                                                   alpha As Object,
                                                   mode As Object,
                                                   scale As Object,
                                                   xAxis As Object,
                                                   ciMethod As Object,
                                                   bootstrapReplicates As Object,
                                                   useT As Object,
                                                   minSubjects As Object,
                                                   minPairsPerSubject As Object,
                                                   excludeSingletonSubjects As Object,
                                                   allowFallbackToSimple As Object,
                                                   checkProportionalBias As Object,
                                                   plotMode As Object,
                                                   randomSeed As Object,
                                                   varNames As Object,
                                                   ByRef names() As String) As Agreement.BlandAltmanResult

            Dim input = ReadAlignedNumericWithOptionalCategory(x, y, subjectIds, requireCategory:=False)
            If input.Error.HasValue Then Throw New ArgumentException("Inputs must be aligned one-column ranges with matching row counts.")
            If input.X Is Nothing OrElse input.X.Length < 2 Then Throw New ArgumentException("At least two usable numeric pairs are required.")

            names = ParametricUDFs.ResolveNames(varNames, input.DetectedNames, 2, "Method")

            Dim opts As New Agreement.BlandAltmanOptions With {
                .Alpha = ParseAlphaOrDefault(alpha, 0.05),
                .Mode = ParseBlandMode(mode, Agreement.RepeatedBlandAltmanMode.Auto),
                .Scale = ParseBlandScale(scale, Agreement.BlandAltmanScale.RawDifference),
                .XAxisMode = ParseBlandXAxisMode(xAxis, Agreement.BlandAltmanXAxisMode.MeanOfMethods),
                .CiMethod = ParseAgreementCiMethod(ciMethod, Agreement.AgreementCiMethod.Analytical),
                .BootstrapReplicates = Math.Max(200, UDFhelpers.GetOptionalInt(bootstrapReplicates, 2000)),
                .UseTDistribution = UDFhelpers.GetOptionalBool(useT, True),
                .MinSubjects = Math.Max(1, UDFhelpers.GetOptionalInt(minSubjects, 2)),
                .MinPairsPerSubject = Math.Max(1, UDFhelpers.GetOptionalInt(minPairsPerSubject, 2)),
                .ExcludeSingletonSubjects = UDFhelpers.GetOptionalBool(excludeSingletonSubjects, True),
                .AllowFallbackToSimple = UDFhelpers.GetOptionalBool(allowFallbackToSimple, True),
                .CheckProportionalBias = UDFhelpers.GetOptionalBool(checkProportionalBias, True),
                .PlotMode = ParseBlandPlotMode(plotMode, Agreement.RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans)
            }

            If input.Category IsNot Nothing Then opts.SubjectIds = input.Category

            Dim seed As Integer = ParseOptionalSeed(randomSeed)
            Dim mdl As New Agreement.BlandAltmanAgreement(input.X, input.Y, names(0), names(1), opts)
            Return mdl.Fit(seed)
        End Function

    End Module
End Namespace
