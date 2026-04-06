Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for baseline-category multinomial logistic regression models.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These functions fit and interrogate multinomial logistic regression models for categorical outcomes with more than two levels.
    ''' One category is treated as the reference (baseline) category, and the model estimates a separate linear predictor for each non-baseline category.
    ''' </para>
    ''' <para>
    ''' For a response category <c>k</c> relative to the baseline category <c>r</c>, the model has the form
    ''' <c>log(P(Y=k)/P(Y=r)) = η_k = α_k + x'β_k + offset</c>, where <c>α_k</c> is a category-specific intercept when enabled,
    ''' <c>β_k</c> is the slope vector for category <c>k</c>, and <c>x</c> is the row of predictors.
    ''' </para>
    ''' <para>
    ''' The fitted model is identified by a handle returned by <c>BESH.REGR.MNLOGIT_FIT</c>. The handle can then be reused by the other worksheet functions
    ''' in this module to obtain coefficient summaries, model-level tests, classification tables, residual diagnostics, predictions, and explicit cleanup
    ''' without refitting the model.
    ''' </para>
    ''' <para>
    ''' Predictor formulas reuse the same formula infrastructure that is currently available for the Cox and ordinal-logit UDFs.
    ''' This means additive terms, polynomial terms, continuous-variable interactions, and categorical main effects can be defined from the raw predictor matrix supplied to the fit function.
    ''' </para>
    ''' </remarks>
    Public Module MultinomialLogitUDFs

        ''' <summary>
        ''' In-memory cache of fitted multinomial-logit handles for the current Excel session.
        ''' </summary>
        Private ReadOnly _mnCache As New ConcurrentDictionary(Of String, MultinomialLogitHandle)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>Sentinel used internally for missing categorical outcomes before row filtering.</summary>
        Private Const MissingCategoricalOutcomeCode As Integer = Integer.MinValue

        ''' <summary>
        ''' Stores a fitted multinomial-logit model together with the metadata required for summaries and prediction.
        ''' </summary>
        Private Class MultinomialLogitHandle
            Public Property Handle As String
            Public Property Model As regression.MultinomialLogitModel
            Public Property VarNames As String()
            Public Property ExpandedPredictorNames As String()
            Public Property RawVarNames As String()
            Public Property RawPredictorKeys As String()
            Public Property RawPredictorAbsoluteLetters As String()
            Public Property DesignSpec As RegressionFormulaDesignSpec
            Public Property OmitCategoricalReference As Boolean
            Public Property Reference As regression.ReferenceCategory
            Public Property CategoriesInModelOrder As Integer()
            Public Property BaselineCategory As Integer
            Public Property PredictorCount As Integer
            Public Property EquationParameterCount As Integer
            Public Property InterceptIncluded As Boolean
            Public Property HasOffset As Boolean
            Public Property HasWeights As Boolean
            Public Property Alpha As Double
        End Class

        ''' <summary>
        ''' Fits a baseline-category multinomial logistic regression model and returns a reusable model handle.
        ''' </summary>
        ''' <param name="y">
        ''' A single-column numeric range containing the categorical outcome.
        ''' Values must be finite integers representing the observed response categories.
        ''' The distinct categories are sorted and the requested reference category is moved to the baseline position used internally by the model.
        ''' </param>
        ''' <param name="x">
        ''' A numeric predictor matrix with one row per observation and one column per raw predictor.
        ''' The raw predictor matrix can be used directly or transformed internally by the optional <paramref name="formula"/>.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional raw predictor names.
        ''' This may be supplied as a comma-separated text string or as a one-row or one-column range containing one name per raw predictor column.
        ''' If omitted, default names such as X1, X2, … are assigned automatically.
        ''' </param>
        ''' <param name="offset">
        ''' Optional numeric offset vector with one value per observation.
        ''' When supplied, the offset is added to each non-baseline linear predictor and treated as known rather than estimated.
        ''' </param>
        ''' <param name="weights">
        ''' Optional nonnegative case weights.
        ''' Positive weights act like replicate or importance weights in the log-likelihood. Rows with nonpositive or invalid weights are excluded before fitting.
        ''' </param>
        ''' <param name="reference">
        ''' Optional baseline-category choice for the response scale.
        ''' Accepted values are <c>last</c> (default) and <c>first</c>.
        ''' The selected category becomes the baseline category against which all other logits are formed.
        ''' </param>
        ''' <param name="includeIntercept">
        ''' TRUE to include one category-specific intercept for each non-baseline category (default TRUE).
        ''' This corresponds to the usual multinomial-logit formulation.
        ''' </param>
        ''' <param name="formula">
        ''' Optional right-hand-side model formula used to construct the design matrix from the raw predictor matrix <paramref name="x"/>.
        ''' Supported syntax currently includes additive terms (<c>A + B</c>), polynomial terms (<c>A^2</c>),
        ''' continuous-variable interactions (<c>A:B</c>, <c>A:B:C</c>), and categorical main effects such as
        ''' <c>factor(C)</c> or <c>factor(C, ref=2)</c>. If omitted or blank, all raw predictor columns are used as continuous main effects.
        ''' </param>
        ''' <param name="formulaAddressing">
        ''' Optional formula-addressing mode that controls how bare column-letter tokens are interpreted.
        ''' Accepted values are <c>relative</c> (default), <c>absolute</c>, and <c>names</c>.
        ''' In <c>relative</c> mode, <c>A</c>, <c>B</c>, <c>AA</c>, … refer to columns 1, 2, 27, … of <paramref name="x"/>.
        ''' In <c>absolute</c> mode, bare letters refer to worksheet columns of the supplied <paramref name="x"/> range.
        ''' In <c>names</c> mode, bare letters are disabled and variables should be referenced using single-quoted names such as <c>'dose'</c>.
        ''' Single quotes inside names are escaped by doubling them, e.g. <c>'Children''s dose'</c>.
        ''' </param>
        ''' <param name="maxIter">
        ''' Optional maximum number of Newton-style iterations used by the fitting procedure.
        ''' Increase this value when convergence is slow for more complex models.
        ''' </param>
        ''' <param name="tol">
        ''' Optional convergence tolerance controlling the stopping criteria for parameter changes and log-likelihood changes.
        ''' Smaller values demand tighter convergence but may increase runtime.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used internally for confidence intervals stored in the wrapped regression results.
        ''' This does not affect the maximum-likelihood estimates themselves.
        ''' </param>
        ''' <returns>
        ''' A text handle identifying the fitted multinomial-logit model within the current Excel session.
        ''' The handle can be passed to the other <c>MNLOGIT_*</c> worksheet functions to obtain summaries, diagnostics, and predictions without refitting the model.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Unlike ordinal logistic regression, multinomial logistic regression does not assume proportional odds or any intrinsic ordering of the response categories.
        ''' A separate slope vector is estimated for each non-baseline category, so predictor effects may differ across category comparisons.
        ''' </para>
        ''' <para>
        ''' Rows containing invalid values in the response, predictors, offset, or weights are excluded before fitting. At least two distinct response categories must remain.
        ''' </para>
        ''' <para>
        ''' If <c>formulaAddressing="absolute"</c> is used, the <paramref name="x"/> argument should be passed as a direct worksheet range so that absolute worksheet column letters can be determined.
        ''' </para>
        ''' <para>
        ''' Residual diagnostics are computed during fitting so that <c>BESH.REGR.MNLOGIT_RESID</c> can reuse the fitted object without forcing an additional refit.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.MNLOGIT_FIT(A2:A101,B2:D101,"dose,age,prison")
        ''' =BESH.REGR.MNLOGIT_FIT(A2:A101,B2:E101,"dose,age,prison,stage",,,"last",TRUE,"A + B + factor(D, ref=1) + 'dose':'age'","names",100,1E-8,0.05)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.MNLOGIT_FIT",
            Category:="BESHStatNG - Regression Models",
            Description:="Fits a baseline-category multinomial logistic regression model and returns a reusable handle.",
            HelpTopic:="udf/regression.md#beshregrmnlogit_fit"
        )>
        Public Function MNLOGIT_FIT(
            <ExcelArgument(Name:="y", Description:="Categorical outcome (single numeric column of category codes).")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Raw predictor matrix with one row per observation and one column per raw predictor.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional raw predictor names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="offset", Description:="Optional numeric offset vector (one column).")> Optional offset As Object = Nothing,
            <ExcelArgument(Name:="weights", Description:="Optional nonnegative case weights (one column).")> Optional weights As Object = Nothing,
            <ExcelArgument(Name:="reference", Description:="Baseline-category choice: ""last"" (default) or ""first"".")> Optional reference As Object = Nothing,
            <ExcelArgument(Name:="includeIntercept", Description:="TRUE to include category-specific intercepts (default TRUE).")> Optional includeIntercept As Object = Nothing,
            <ExcelArgument(Name:="formula", Description:="Optional RHS model formula built from the raw predictor matrix.")> Optional formula As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Formula-addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing,
            <ExcelArgument(Name:="maxIter", Description:="Maximum number of fitting iterations (default 50).")> Optional maxIter As Object = Nothing,
            <ExcelArgument(Name:="tol", Description:="Convergence tolerance (default 1E-10).")> Optional tol As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha used for internal confidence intervals (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "MNLOGIT_FIT (editing...)"

            Try
                Dim yVals As List(Of Integer) = Nothing
                If Not TryReadCategoricalOutcome(y, yVals) Then Return ExcelError.ExcelErrorValue

                Dim xMat As Double(,) = Nothing
                Dim rowCount As Integer = 0
                Dim colCount As Integer = 0
                If Not UDFhelpers.TryReadNumericMatrix(x, xMat, rowCount, colCount) Then Return ExcelError.ExcelErrorValue
                If rowCount <> yVals.Count Then Return ExcelError.ExcelErrorValue
                If colCount < 1 Then Return ExcelError.ExcelErrorNum

                Dim rawRowIds() As Integer = Nothing

                Dim offsetVals As List(Of Double) = Nothing
                Dim hasOffset As Boolean = HasUsableOptionalArgument(offset)
                If hasOffset Then
                    If Not UDFhelpers.TryReadNumericColumn(offset, offsetVals) Then Return ExcelError.ExcelErrorValue
                    If offsetVals.Count <> rowCount Then Return ExcelError.ExcelErrorValue
                End If

                Dim weightVals As List(Of Double) = Nothing
                Dim hasWeights As Boolean = HasUsableOptionalArgument(weights)
                If hasWeights Then
                    If Not UDFhelpers.TryReadNumericColumn(weights, weightVals) Then Return ExcelError.ExcelErrorValue
                    If weightVals.Count <> rowCount Then Return ExcelError.ExcelErrorValue
                End If

                If Not TryFilterRawMultinomialRegressionInputs(yVals:=yVals,
                                                               rawX:=xMat,
                                                               offsetVals:=offsetVals,
                                                               weightVals:=weightVals,
                                                               filteredY:=yVals,
                                                               filteredRawX:=xMat,
                                                               filteredOffset:=offsetVals,
                                                               filteredWeights:=weightVals,
                                                               originalRowIds:=rawRowIds) Then
                    Return ExcelError.ExcelErrorValue
                End If

                rowCount = xMat.GetLength(0)
                colCount = xMat.GetLength(1)
                If rowCount <> yVals.Count Then Return ExcelError.ExcelErrorValue
                If colCount < 1 Then Return ExcelError.ExcelErrorNum

                Dim rawVarNames As String() = UDFhelpers.GetVarNames(varNames, colCount)

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
                    If Not UDFhelpers.TryGetAbsoluteColumnLettersFromRange(x, colCount, absoluteColumnLetters) Then
                        Return ExcelError.ExcelErrorValue
                    End If
                End If

                Dim designBuild As RegressionFormulaMatrixBuildResult = Nothing
                Dim designErr As String = Nothing
                If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromFormula(rawX:=xMat,
                                                                                                result:=designBuild,
                                                                                                errorMessage:=designErr,
                                                                                                predictorNames:=rawVarNames,
                                                                                                formulaText:=formulaText,
                                                                                                absoluteColumnLetters:=absoluteColumnLetters,
                                                                                                allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                                                                allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                                                                allowQuotedVariableNames:=allowQuotedVariableNames,
                                                                                                omitCategoricalReference:=True) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim fitX As Double(,) = designBuild.ExpandedPredictorMatrix
                Dim fitPredictorNames As String() = designBuild.ExpandedPredictorNames
                If fitX Is Nothing OrElse fitPredictorNames Is Nothing OrElse fitPredictorNames.Length < 1 Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim fitData As Double(,) = Nothing
                Dim fitOffset() As Double = Nothing
                Dim fitWeights() As Double = Nothing
                Dim rowIds() As Integer = Nothing

                If Not BuildFilteredMultinomialRegressionInputs(yVals:=yVals,
                                                                expandedX:=fitX,
                                                                offsetVals:=offsetVals,
                                                                weightVals:=weightVals,
                                                                sourceRowIds:=rawRowIds,
                                                                fitData:=fitData,
                                                                rowIds:=rowIds,
                                                                fitOffset:=fitOffset,
                                                                fitWeights:=fitWeights) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim distinctCats As Integer = CountDistinctOutcomeCategories(fitData)
                If distinctCats < 2 Then Return ExcelError.ExcelErrorNum

                Dim fitVarNames(fitPredictorNames.Length) As String
                fitVarNames(0) = "Y"
                Array.Copy(fitPredictorNames, 0, fitVarNames, 1, fitPredictorNames.Length)

                Dim alphaValue As Double = 0.05
                If HasUsableOptionalArgument(alpha) Then
                    If Not ParametricUDFs.TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim maxIterValue As Integer = UDFhelpers.GetOptionalInt(maxIter, 50)
                Dim tolValue As Double = UDFhelpers.GetOptionalDouble(tol, 0.0000000001R)
                If maxIterValue < 1 Then Return ExcelError.ExcelErrorNum
                If Double.IsNaN(tolValue) OrElse Double.IsInfinity(tolValue) OrElse tolValue <= 0 Then Return ExcelError.ExcelErrorNum

                Dim refCat As regression.ReferenceCategory = ParseReferenceCategory(reference)
                Dim interceptFlag As Boolean = UDFhelpers.GetOptionalBool(includeIntercept, True)

                Dim mn As New regression.MultinomialLogitModel()
                mn.bComputeResiduals = True
                mn.bReturnCov = False
                mn.bIterationDetails = False
                mn.settingInputs(alphaValue, maxIterValue, tolValue)
                mn.data(fitData,
                        fitVarNames,
                        rowIds,
                        fitOffset,
                        fitWeights)
                mn.Fit(If(interceptFlag, 1, 0), refCat, False)

                Dim cats() As Integer = GetCategoriesInModelOrder(fitData, refCat)
                If cats Is Nothing OrElse cats.Length < 2 Then Return ExcelError.ExcelErrorNum

                Dim handleKey As String = "MNLOGIT:" & Guid.NewGuid().ToString("N")
                Dim h As New MultinomialLogitHandle With {
                    .Handle = handleKey,
                    .Model = mn,
                    .VarNames = DirectCast(mn.results.varNames.Clone(), String()),
                    .ExpandedPredictorNames = DirectCast(fitPredictorNames.Clone(), String()),
                    .RawVarNames = If(designBuild.FullRawPredictorNames, New String() {}),
                    .RawPredictorKeys = If(designBuild.FullRawPredictorKeys, New String() {}),
                    .RawPredictorAbsoluteLetters = If(designBuild.FullRawPredictorAbsoluteLetters, New String() {}),
                    .DesignSpec = designBuild.DesignSpec,
                    .OmitCategoricalReference = True,
                    .Reference = refCat,
                    .CategoriesInModelOrder = DirectCast(cats.Clone(), Integer()),
                    .BaselineCategory = cats(cats.Length - 1),
                    .PredictorCount = fitPredictorNames.Length,
                    .EquationParameterCount = fitPredictorNames.Length + If(interceptFlag, 1, 0),
                    .InterceptIncluded = interceptFlag,
                    .HasOffset = (fitOffset IsNot Nothing),
                    .HasWeights = (fitWeights IsNot Nothing),
                    .Alpha = alphaValue
                }

                _mnCache(handleKey) = h
                Return handleKey

            Catch ex As Exception
                Return ex.GetType().Name & ": " & ex.Message
            End Try
        End Function

        ''' <summary>
        ''' Returns the parameter summary table for a fitted multinomial-logit model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MNLOGIT_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <param name="alpha">Optional two-sided alpha for confidence intervals and odds-ratio confidence limits (default 0.05).</param>
        ''' <returns>
        ''' A spilled array containing one row per estimated parameter. Slope parameters are accompanied by odds ratios and odds-ratio confidence limits;
        ''' category-specific intercept parameters leave the odds-ratio columns blank because exponentiated intercepts are generally not interpreted as predictor-effect odds ratios.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Parameter names identify the compared category and the reference category. For example, a parameter name such as
        ''' <c>cat=2 (ref=4): dose</c> refers to the log-odds contrast between category 2 and the baseline category 4.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.MNLOGIT_SUMMARY(F2)
        ''' =BESH.REGR.MNLOGIT_SUMMARY(F2,TRUE,0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.MNLOGIT_SUMMARY",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the parameter summary table for a fitted multinomial-logit model handle.",
            HelpTopic:="udf/regression.md#beshregrmnlogit_summary"
        )>
        Public Function MNLOGIT_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MNLOGIT_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for confidence intervals (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As MultinomialLogitHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim alphaValue As Double = 0.05
                If HasUsableOptionalArgument(alpha) Then
                    If Not ParametricUDFs.TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim q As Integer = h.Model.results.Coeffs_est.Length
                Dim outRows As Integer = If(hdr, q + 1, q)
                Dim out(outRows - 1, 8) As Object

                Dim zCrit As Double = distributions.ZCritTwoSided(alphaValue)
                Dim r0 As Integer = 0

                If hdr Then
                    out(0, 0) = "Parameter"
                    out(0, 1) = "Type"
                    out(0, 2) = "Coef"
                    out(0, 3) = "SE"
                    out(0, 4) = "Z"
                    out(0, 5) = "P-value"
                    out(0, 6) = "OR"
                    out(0, 7) = "OR " & $"{100.0 * (1.0 - alphaValue):0.##}%" & " LCL"
                    out(0, 8) = "OR " & $"{100.0 * (1.0 - alphaValue):0.##}%" & " UCL"
                    r0 = 1
                End If

                For i As Integer = 0 To q - 1
                    Dim beta As Double = h.Model.results.Coeffs_est(i)
                    Dim se As Double = h.Model.results.Coeffs_SEs(i)
                    Dim z As Double = If(se > 0.0, beta / se, Double.NaN)
                    Dim pv As Double = If(se > 0.0, 2.0 * distributions.PNorm(-Math.Abs(z)), Double.NaN)
                    Dim pname As String = h.VarNames(i)
                    Dim isInterceptParam As Boolean = pname IsNot Nothing AndAlso pname.Trim().EndsWith(": Intercept", StringComparison.OrdinalIgnoreCase)

                    out(r0 + i, 0) = pname
                    out(r0 + i, 1) = If(isInterceptParam, "Intercept", "Slope")
                    out(r0 + i, 2) = beta
                    out(r0 + i, 3) = se
                    out(r0 + i, 4) = z
                    out(r0 + i, 5) = pv

                    If isInterceptParam Then
                        out(r0 + i, 6) = ""
                        out(r0 + i, 7) = ""
                        out(r0 + i, 8) = ""
                    Else
                        out(r0 + i, 6) = ExpForDisplay(beta)
                        out(r0 + i, 7) = ExpForDisplay(beta - zCrit * se)
                        out(r0 + i, 8) = ExpForDisplay(beta + zCrit * se)
                    End If
                Next

                Return out

            Catch ex As Exception
                Return ex.GetType().Name & ": " & ex.Message
            End Try
        End Function

        ''' <summary>
        ''' Returns global model tests and fit statistics for a fitted multinomial-logit model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MNLOGIT_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing model-level diagnostics such as log-likelihoods, likelihood-ratio and goodness-of-fit tests,
        ''' pseudo-R² measures, information criteria, iteration count, and convergence information.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.MNLOGIT_TESTS(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.MNLOGIT_TESTS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns model-level diagnostics and tests for a fitted multinomial-logit model handle.",
            HelpTopic:="udf/regression.md#beshregrmnlogit_tests"
        )>
        Public Function MNLOGIT_TESTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MNLOGIT_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As MultinomialLogitHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim labels() As String = h.Model.results.ModelTableLabels
                Dim vals(,) As Object = h.Model.results.ModelTableVals
                If labels Is Nothing OrElse vals Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim n As Integer = labels.Length
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, n + 1, n)
                Dim out(outRows - 1, 3) As Object

                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "Item"
                    out(0, 1) = "Value"
                    out(0, 2) = "df"
                    out(0, 3) = "P-value"
                    r0 = 1
                End If

                For i As Integer = 0 To n - 1
                    out(r0 + i, 0) = labels(i)
                    out(r0 + i, 1) = vals(i, 0)
                    out(r0 + i, 2) = vals(i, 1)
                    out(r0 + i, 3) = vals(i, 2)
                Next

                Return out

            Catch ex As Exception
                Return ex.GetType().Name & ": " & ex.Message
            End Try
        End Function

        ''' <summary>
        ''' Returns the observed-versus-predicted classification table for a fitted multinomial-logit model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MNLOGIT_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include header rows and labels (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing the weighted or unweighted confusion matrix, per-row recall percentages,
        ''' per-column precision percentages, and overall classification accuracy.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The classification table is based on assigning each observation to the category with the largest fitted probability.
        ''' The category columns are shown in the model's internal category order, which depends on the reference-category choice used during fitting.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.MNLOGIT_CLASS(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.MNLOGIT_CLASS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the classification confusion matrix for a fitted multinomial-logit model handle.",
            HelpTopic:="udf/regression.md#beshregrmnlogit_class"
        )>
        Public Function MNLOGIT_CLASS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MNLOGIT_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include header rows and labels (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As MultinomialLogitHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim cls As regression.ClassificationCrosstab = Nothing
                If Not TryGetPrivateField(h.Model, "pPredAccuary", cls) Then Return ExcelError.ExcelErrorNA
                If cls Is Nothing OrElse cls.Counts Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim cats() As Integer = h.CategoriesInModelOrder
                If cats Is Nothing OrElse cats.Length < 2 Then Return ExcelError.ExcelErrorNA

                Dim k As Integer = cats.Length
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, k + 2, k + 1)
                Dim outCols As Integer = k + 2
                Dim out(outRows - 1, outCols - 1) As Object
                Dim r0 As Integer = 0

                If hdr Then
                    out(0, 0) = "Observed \ Predicted"
                    For j As Integer = 0 To k - 1
                        out(0, j + 1) = cats(j)
                    Next
                    out(0, k + 1) = "Recall %"
                    r0 = 1
                End If

                For i As Integer = 0 To k - 1
                    out(r0 + i, 0) = cats(i)
                    For j As Integer = 0 To k - 1
                        out(r0 + i, j + 1) = cls.Counts(i, j)
                    Next
                    out(r0 + i, k + 1) = cls.RecallPct(i)
                Next

                out(r0 + k, 0) = "Precision % / Overall"
                For j As Integer = 0 To k - 1
                    out(r0 + k, j + 1) = cls.PrecisionPct(j)
                Next
                out(r0 + k, k + 1) = cls.OverallAccuracyPct

                Return out

            Catch ex As Exception
                Return ex.GetType().Name & ": " & ex.Message
            End Try
        End Function

        ''' <summary>
        ''' Returns residual diagnostics for a fitted multinomial-logit model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MNLOGIT_FIT</c>.</param>
        ''' <param name="residType">
        ''' Optional residual-output selector.
        ''' Accepted values are <c>all</c> (default), <c>observed</c>, <c>fittedmean</c>, <c>prob</c>, <c>response</c>,
        ''' <c>pearson</c>, <c>stdpearson</c>, <c>deviance</c>, <c>stddeviance</c>, and <c>leverage</c>.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing the requested residual block.
        ''' When <paramref name="residType"/> is <c>all</c>, the returned table contains all category-specific blocks plus the scalar residual diagnostics.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.MNLOGIT_RESID(F2)
        ''' =BESH.REGR.MNLOGIT_RESID(F2,"pearson")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.MNLOGIT_RESID",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns residual diagnostics for a fitted multinomial-logit model handle.",
            HelpTopic:="udf/regression.md#beshregrmnlogit_resid"
        )>
        Public Function MNLOGIT_RESID(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MNLOGIT_FIT.")> handle As Object,
            <ExcelArgument(Name:="residType", Description:="Residual output block: ""all"", ""observed"", ""fittedmean"", ""prob"", ""response"", ""pearson"", ""stdpearson"", ""deviance"", ""stddeviance"", or ""leverage"".")> Optional residType As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As MultinomialLogitHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim res As regression.MultinomialResiduals = Nothing
                If Not TryGetPrivateField(h.Model, "pResiduals", res) Then Return ExcelError.ExcelErrorNA
                If res Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim key As String = ParseMultinomialResidualType(residType)
                Dim cats() As Integer = h.CategoriesInModelOrder

                Select Case key
                    Case "all"
                        Return BuildAllResidualOutput(res, cats, hdr)
                    Case "observed"
                        Return BuildResidualMatrixOutput(res.Observed, CategoryHeaders("Observed", cats), hdr)
                    Case "fittedmean"
                        Return BuildResidualMatrixOutput(res.FittedMeans, CategoryHeaders("FittedMean", cats), hdr)
                    Case "prob"
                        Return BuildResidualMatrixOutput(res.Probabilities, CategoryHeaders("Prob", cats), hdr)
                    Case "response"
                        Return BuildResidualMatrixOutput(res.ResponseResiduals, CategoryHeaders("Response", cats), hdr)
                    Case "pearson"
                        Return BuildResidualMatrixOutput(res.PearsonResiduals, CategoryHeaders("Pearson", cats), hdr)
                    Case "stdpearson"
                        Return BuildResidualMatrixOutput(res.StdPearsonResiduals, CategoryHeaders("StdPearson", cats), hdr)
                    Case "deviance"
                        Return BuildResidualVectorOutput(res.DevianceResiduals, "DevianceResidual", hdr)
                    Case "stddeviance"
                        Return BuildResidualVectorOutput(res.StdDevianceResiduals, "StdDevianceResidual", hdr)
                    Case "leverage"
                        Return BuildResidualVectorOutput(res.Leverage, "Leverage", hdr)
                    Case Else
                        Return BuildAllResidualOutput(res, cats, hdr)
                End Select

            Catch ex As Exception
                Return ex.GetType().Name & ": " & ex.Message
            End Try
        End Function

        ''' <summary>
        ''' Returns fitted category probabilities and predicted categories for new data under a fitted multinomial-logit model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MNLOGIT_FIT</c>.</param>
        ''' <param name="newX">New raw predictor matrix in the same raw-column order used at fitting time.</param>
        ''' <param name="newOffset">Optional offset vector for the new observations.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing the most likely predicted category, one category-specific linear predictor column for each non-baseline category,
        ''' and one fitted probability column per outcome category in the model's internal category order.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The probability columns sum to 1 across each row, up to normal floating-point rounding error.
        ''' The predicted category is the category whose fitted probability is largest in the returned probability vector.
        ''' </para>
        ''' <para>
        ''' When the model was fitted with an offset, <paramref name="newOffset"/> must be supplied and aligned with the rows of <paramref name="newX"/>.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.MNLOGIT_PRED(F2,H2:J10)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.MNLOGIT_PRED",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns fitted probabilities and predicted categories for new data under a fitted multinomial-logit model.",
            HelpTopic:="udf/regression.md#beshregrmnlogit_pred"
        )>
        Public Function MNLOGIT_PRED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MNLOGIT_FIT.")> handle As Object,
            <ExcelArgument(AllowReference:=True, Name:="newX", Description:="New raw predictor matrix in the same raw-column order used at fitting time.")> newX As Object,
            <ExcelArgument(Name:="newOffset", Description:="Optional offset vector for the new observations.")> Optional newOffset As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As MultinomialLogitHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim rawX As Double(,) = Nothing
                Dim nRows As Integer = 0
                Dim rawCols As Integer = 0
                If Not UDFhelpers.TryReadNumericMatrix(newX, rawX, nRows, rawCols) Then Return ExcelError.ExcelErrorValue
                If h.RawPredictorKeys Is Nothing OrElse rawCols <> h.RawPredictorKeys.Length Then Return ExcelError.ExcelErrorValue

                Dim expandedX As Double(,) = Nothing
                Dim expandedNames() As String = Nothing
                Dim designErr As String = Nothing
                If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromDesignSpec(rawX:=rawX,
                                                                                                    fullRawPredictorKeys:=h.RawPredictorKeys,
                                                                                                    designSpec:=h.DesignSpec,
                                                                                                    expandedX:=expandedX,
                                                                                                    expandedPredictorNames:=expandedNames,
                                                                                                    errorMessage:=designErr,
                                                                                                    omitCategoricalReference:=h.OmitCategoricalReference) Then
                    Return ExcelError.ExcelErrorValue
                End If

                If expandedX Is Nothing OrElse expandedNames Is Nothing Then Return ExcelError.ExcelErrorValue
                If expandedNames.Length <> h.PredictorCount Then Return ExcelError.ExcelErrorValue

                Dim offsetVals As List(Of Double) = Nothing
                If h.HasOffset Then
                    If Not HasUsableOptionalArgument(newOffset) Then Return ExcelError.ExcelErrorValue
                    If Not UDFhelpers.TryReadNumericColumn(newOffset, offsetVals) Then Return ExcelError.ExcelErrorValue
                    If offsetVals.Count <> nRows Then Return ExcelError.ExcelErrorValue
                ElseIf HasUsableOptionalArgument(newOffset) Then
                    If Not UDFhelpers.TryReadNumericColumn(newOffset, offsetVals) Then Return ExcelError.ExcelErrorValue
                    If offsetVals.Count <> nRows Then Return ExcelError.ExcelErrorValue
                End If

                Dim b() As Double = h.Model.results.Coeffs_est
                Dim cats() As Integer = h.CategoriesInModelOrder
                If b Is Nothing OrElse cats Is Nothing OrElse cats.Length < 2 Then Return ExcelError.ExcelErrorNA

                Dim k As Integer = cats.Length
                Dim q As Integer = b.Length
                If (k - 1) < 1 Then Return ExcelError.ExcelErrorNA
                If q <> (k - 1) * h.EquationParameterCount Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, nRows + 1, nRows)
                Dim outCols As Integer = 1 + (k - 1) + k
                Dim out(outRows - 1, outCols - 1) As Object

                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "PredictedCategory"
                    Dim c As Integer = 1
                    For j As Integer = 0 To k - 2
                        out(0, c) = "Eta(" & cats(j).ToString(CultureInfo.InvariantCulture) & " vs " & h.BaselineCategory.ToString(CultureInfo.InvariantCulture) & ")"
                        c += 1
                    Next
                    For j As Integer = 0 To k - 1
                        out(0, c) = "P(Y=" & cats(j).ToString(CultureInfo.InvariantCulture) & ")"
                        c += 1
                    Next
                    r0 = 1
                End If

                For i As Integer = 0 To nRows - 1
                    Dim eta() As Double = ComputeMultinomialEtas(expandedX, i, b, h.EquationParameterCount, h.InterceptIncluded, offsetVals, cats.Length)
                    Dim probs() As Double = ComputeMultinomialProbabilities(eta)
                    Dim predIdx As Integer = regression.CategoricalLogitUtils.ArgMax(probs, True)

                    out(r0 + i, 0) = cats(predIdx)
                    Dim c As Integer = 1
                    For j As Integer = 0 To eta.Length - 1
                        out(r0 + i, c) = eta(j)
                        c += 1
                    Next
                    For j As Integer = 0 To probs.Length - 1
                        out(r0 + i, c) = probs(j)
                        c += 1
                    Next
                Next

                Return out

            Catch ex As Exception
                Return ex.GetType().Name & ": " & ex.Message
            End Try
        End Function

        ''' <summary>
        ''' Removes a fitted multinomial-logit handle from the in-memory cache.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MNLOGIT_FIT</c>.</param>
        ''' <returns>TRUE if the handle existed and was removed; otherwise FALSE.</returns>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.MNLOGIT_DROP(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.MNLOGIT_DROP",
            Category:="BESHStatNG - Regression Models",
            Description:="Removes a fitted multinomial-logit model handle from memory.",
            HelpTopic:="udf/regression.md#beshregrmnlogit_drop"
        )>
        Public Function MNLOGIT_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MNLOGIT_FIT.")> handle As Object
        ) As Object
            Try
                Dim key As String = UDFhelpers.AsString(handle)
                If String.IsNullOrWhiteSpace(key) Then Return False
                Dim removed As MultinomialLogitHandle = Nothing
                Return _mnCache.TryRemove(key, removed)
            Catch
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Attempts to read a categorical outcome column from Excel input.
        ''' </summary>
        ''' <param name="v">Worksheet argument containing the outcome column.</param>
        ''' <param name="values">On success, receives the category codes as integers.</param>
        ''' <returns>True when the input can be interpreted as a one-column categorical outcome; otherwise, False.</returns>
        Private Function TryReadCategoricalOutcome(v As Object, ByRef values As List(Of Integer)) As Boolean
            values = New List(Of Integer)()

            Dim dVals As List(Of Double) = Nothing
            If Not UDFhelpers.TryReadNumericColumn(v, dVals) Then Return False
            If dVals Is Nothing OrElse dVals.Count = 0 Then Return False

            For Each d As Double In dVals
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then
                    values.Add(MissingCategoricalOutcomeCode)
                    Continue For
                End If

                Dim rounded As Double = Math.Round(d)
                If Math.Abs(d - rounded) > 0.0000001R Then Return False

                values.Add(CInt(rounded))
            Next

            Return values.Count > 0
        End Function

        ''' <summary>
        ''' Filters the raw multinomial-regression inputs before formula expansion so rows with missing outcome/predictor values are ignored consistently.
        ''' </summary>
        Private Function TryFilterRawMultinomialRegressionInputs(yVals As IList(Of Integer),
                                                                 rawX(,) As Double,
                                                                 offsetVals As IList(Of Double),
                                                                 weightVals As IList(Of Double),
                                                                 ByRef filteredY As List(Of Integer),
                                                                 ByRef filteredRawX As Double(,),
                                                                 ByRef filteredOffset As List(Of Double),
                                                                 ByRef filteredWeights As List(Of Double),
                                                                 ByRef originalRowIds As Integer()) As Boolean

            filteredY = Nothing
            filteredRawX = Nothing
            filteredOffset = Nothing
            filteredWeights = Nothing
            originalRowIds = Nothing

            If yVals Is Nothing OrElse rawX Is Nothing Then Return False

            Dim n As Integer = rawX.GetLength(0)
            Dim p As Integer = rawX.GetLength(1)
            If yVals.Count <> n Then Return False
            If offsetVals IsNot Nothing AndAlso offsetVals.Count <> n Then Return False
            If weightVals IsNot Nothing AndAlso weightVals.Count <> n Then Return False

            Dim keep As New List(Of Integer)()

            For i As Integer = 0 To n - 1
                Dim ok As Boolean = (yVals(i) <> MissingCategoricalOutcomeCode)

                If ok Then
                    For j As Integer = 0 To p - 1
                        Dim xv As Double = rawX(i, j)
                        If Double.IsNaN(xv) OrElse Double.IsInfinity(xv) Then
                            ok = False
                            Exit For
                        End If
                    Next
                End If

                If ok AndAlso offsetVals IsNot Nothing Then
                    Dim ov As Double = offsetVals(i)
                    If Double.IsNaN(ov) OrElse Double.IsInfinity(ov) Then ok = False
                End If

                If ok AndAlso weightVals IsNot Nothing Then
                    Dim wv As Double = weightVals(i)
                    If Double.IsNaN(wv) OrElse Double.IsInfinity(wv) OrElse wv <= 0.0 Then ok = False
                End If

                If ok Then keep.Add(i)
            Next

            If keep.Count < 1 Then Return False

            filteredY = New List(Of Integer)(keep.Count)
            filteredRawX = New Double(keep.Count - 1, p - 1) {}
            ReDim originalRowIds(keep.Count - 1)

            If offsetVals IsNot Nothing Then filteredOffset = New List(Of Double)(keep.Count)
            If weightVals IsNot Nothing Then filteredWeights = New List(Of Double)(keep.Count)

            For r As Integer = 0 To keep.Count - 1
                Dim src As Integer = keep(r)
                filteredY.Add(yVals(src))
                originalRowIds(r) = src + 1

                For j As Integer = 0 To p - 1
                    filteredRawX(r, j) = rawX(src, j)
                Next

                If filteredOffset IsNot Nothing Then filteredOffset.Add(offsetVals(src))
                If filteredWeights IsNot Nothing Then filteredWeights.Add(weightVals(src))
            Next

            Return True
        End Function

        ''' <summary>
        ''' Builds the filtered fitting inputs for the multinomial-logit model by excluding rows containing invalid values.
        ''' </summary>
        ''' <param name="yVals">Outcome category codes.</param>
        ''' <param name="expandedX">Expanded predictor matrix aligned with <paramref name="yVals"/>.</param>
        ''' <param name="offsetVals">Optional offset vector aligned with <paramref name="yVals"/>.</param>
        ''' <param name="weightVals">Optional case-weight vector aligned with <paramref name="yVals"/>.</param>
        ''' <param name="fitData">On success, receives the final regression data matrix with outcome in column 0 and predictors in the remaining columns.</param>
        ''' <param name="rowIds">On success, receives the original 1-based row numbers retained after filtering.</param>
        ''' <param name="fitOffset">On success, receives the filtered offset vector or Nothing.</param>
        ''' <param name="fitWeights">On success, receives the filtered weight vector or Nothing.</param>
        ''' <returns>True when at least one valid row remains and the filtered structures are produced; otherwise, False.</returns>
        Private Function BuildFilteredMultinomialRegressionInputs(yVals As IList(Of Integer),
                                                                  expandedX(,) As Double,
                                                                  offsetVals As IList(Of Double),
                                                                  weightVals As IList(Of Double),
                                                                  sourceRowIds As IList(Of Integer),
                                                                  ByRef fitData As Double(,),
                                                                  ByRef rowIds As Integer(),
                                                                  ByRef fitOffset As Double(),
                                                                  ByRef fitWeights As Double()) As Boolean

            fitData = Nothing
            rowIds = Nothing
            fitOffset = Nothing
            fitWeights = Nothing

            If yVals Is Nothing OrElse expandedX Is Nothing Then Return False

            Dim n As Integer = expandedX.GetLength(0)
            Dim p As Integer = expandedX.GetLength(1)
            If yVals.Count <> n Then Return False
            If offsetVals IsNot Nothing AndAlso offsetVals.Count <> n Then Return False
            If weightVals IsNot Nothing AndAlso weightVals.Count <> n Then Return False
            If sourceRowIds IsNot Nothing AndAlso sourceRowIds.Count <> n Then Return False

            Dim keep As New List(Of Integer)()

            For i As Integer = 0 To n - 1
                Dim ok As Boolean = True

                For j As Integer = 0 To p - 1
                    Dim xv As Double = expandedX(i, j)
                    If Double.IsNaN(xv) OrElse Double.IsInfinity(xv) Then
                        ok = False
                        Exit For
                    End If
                Next

                If ok AndAlso offsetVals IsNot Nothing Then
                    Dim ov As Double = offsetVals(i)
                    If Double.IsNaN(ov) OrElse Double.IsInfinity(ov) Then ok = False
                End If

                If ok AndAlso weightVals IsNot Nothing Then
                    Dim wv As Double = weightVals(i)
                    If Double.IsNaN(wv) OrElse Double.IsInfinity(wv) OrElse wv <= 0.0 Then ok = False
                End If

                If ok Then keep.Add(i)
            Next

            If keep.Count < 1 Then Return False

            ReDim fitData(keep.Count - 1, p)
            ReDim rowIds(keep.Count - 1)

            If offsetVals IsNot Nothing Then ReDim fitOffset(keep.Count - 1)
            If weightVals IsNot Nothing Then ReDim fitWeights(keep.Count - 1)

            For r As Integer = 0 To keep.Count - 1
                Dim src As Integer = keep(r)
                fitData(r, 0) = yVals(src)
                rowIds(r) = If(sourceRowIds IsNot Nothing, sourceRowIds(src), src + 1)

                For j As Integer = 0 To p - 1
                    fitData(r, j + 1) = expandedX(src, j)
                Next

                If fitOffset IsNot Nothing Then fitOffset(r) = offsetVals(src)
                If fitWeights IsNot Nothing Then fitWeights(r) = weightVals(src)
            Next

            Return True
        End Function

        ''' <summary>
        ''' Counts the number of distinct outcome categories present in the filtered regression data matrix.
        ''' </summary>
        ''' <param name="fitData">Regression matrix whose first column contains the outcome.</param>
        ''' <returns>The number of distinct integer-valued response categories observed.</returns>
        Private Function CountDistinctOutcomeCategories(fitData(,) As Double) As Integer
            If fitData Is Nothing Then Return 0
            Dim n As Integer = fitData.GetLength(0)
            Dim cats As New HashSet(Of Integer)()
            For i As Integer = 0 To n - 1
                cats.Add(CInt(Math.Round(fitData(i, 0))))
            Next
            Return cats.Count
        End Function

        ''' <summary>
        ''' Returns the response categories in the model's internal order for the requested reference-category convention.
        ''' </summary>
        ''' <param name="fitData">Filtered regression matrix whose first column contains the outcome codes.</param>
        ''' <param name="reference">Requested baseline-category convention.</param>
        ''' <returns>The category labels in the same order used internally by the fitted model.</returns>
        Private Function GetCategoriesInModelOrder(fitData(,) As Double,
                                                   reference As regression.ReferenceCategory) As Integer()
            If fitData Is Nothing Then Return Nothing

            Dim n As Integer = fitData.GetLength(0)
            Dim setCat As New SortedSet(Of Integer)()
            For i As Integer = 0 To n - 1
                setCat.Add(CInt(Math.Round(fitData(i, 0))))
            Next

            Dim cats As Integer() = setCat.ToArray()
            If cats.Length < 2 Then Return cats

            If reference = regression.ReferenceCategory.First Then
                Dim out(cats.Length - 1) As Integer
                Array.Copy(cats, 1, out, 0, cats.Length - 1)
                out(out.Length - 1) = cats(0)
                Return out
            End If

            Return cats
        End Function

        ''' <summary>
        ''' Parses the reference-category option supplied to the multinomial-logit fit function.
        ''' </summary>
        ''' <param name="v">Worksheet argument containing the requested reference direction.</param>
        ''' <returns>The parsed reference-category choice, defaulting to <see cref="regression.ReferenceCategory.Last"/>.</returns>
        Private Function ParseReferenceCategory(v As Object) As regression.ReferenceCategory
            Dim s As String = UDFhelpers.AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return regression.ReferenceCategory.Last

            Select Case s.Trim().ToLowerInvariant()
                Case "first", "smallest", "min"
                    Return regression.ReferenceCategory.First
                Case Else
                    Return regression.ReferenceCategory.Last
            End Select
        End Function

        ''' <summary>
        ''' Attempts to resolve a cached multinomial-logit handle.
        ''' </summary>
        ''' <param name="handle">Worksheet handle argument.</param>
        ''' <param name="h">On success, receives the cached handle object.</param>
        ''' <returns>True when the handle exists in the cache; otherwise, False.</returns>
        Private Function TryGetHandle(handle As Object, ByRef h As MultinomialLogitHandle) As Boolean
            h = Nothing
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _mnCache.TryGetValue(key, h)
        End Function

        ''' <summary>
        ''' Returns True when an optional worksheet argument contains a usable value rather than Excel missing/empty markers.
        ''' </summary>
        ''' <param name="v">Worksheet argument to inspect.</param>
        ''' <returns>True when the argument is present; otherwise, False.</returns>
        Private Function HasUsableOptionalArgument(v As Object) As Boolean
            Return Not (v Is Nothing OrElse TypeOf v Is ExcelMissing OrElse TypeOf v Is ExcelEmpty)
        End Function

        ''' <summary>
        ''' Attempts to read a private instance field by name using reflection.
        ''' </summary>
        ''' <typeparam name="T">Expected field type.</typeparam>
        ''' <param name="instance">Object containing the field.</param>
        ''' <param name="fieldName">Exact private field name.</param>
        ''' <param name="value">On success, receives the field value.</param>
        ''' <returns>True when the field exists and can be cast to <typeparamref name="T"/>; otherwise, False.</returns>
        Private Function TryGetPrivateField(Of T)(instance As Object,
                                                  fieldName As String,
                                                  ByRef value As T) As Boolean
            value = Nothing
            If instance Is Nothing OrElse String.IsNullOrWhiteSpace(fieldName) Then Return False

            Dim fi As FieldInfo = instance.GetType().GetField(fieldName, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public)
            If fi Is Nothing Then Return False

            Dim raw As Object = fi.GetValue(instance)
            If raw Is Nothing Then Return False

            If GetType(T).IsAssignableFrom(raw.GetType()) Then
                value = CType(raw, T)
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' Computes the category-specific linear predictors for one new row.
        ''' </summary>
        ''' <param name="expandedX">Expanded predictor matrix.</param>
        ''' <param name="row">Row index to score.</param>
        ''' <param name="b">Full parameter vector.</param>
        ''' <param name="equationParameterCount">Number of parameters per non-baseline equation.</param>
        ''' <param name="interceptIncluded">Whether each non-baseline equation includes an intercept term.</param>
        ''' <param name="offsetVals">Optional new-data offset vector.</param>
        ''' <param name="categoryCount">Total number of response categories.</param>
        ''' <returns>A vector of length K-1 containing the category-specific linear predictors for the non-baseline categories.</returns>
        Private Function ComputeMultinomialEtas(expandedX(,) As Double,
                                                row As Integer,
                                                b() As Double,
                                                equationParameterCount As Integer,
                                                interceptIncluded As Boolean,
                                                offsetVals As IList(Of Double),
                                                categoryCount As Integer) As Double()

            Dim nonBase As Integer = categoryCount - 1
            Dim eta(nonBase - 1) As Double

            For cat As Integer = 0 To nonBase - 1
                Dim baseIdx As Integer = cat * equationParameterCount
                Dim s As Double = 0.0R
                Dim coeffOffset As Integer = 0

                If interceptIncluded Then
                    s += b(baseIdx)
                    coeffOffset = 1
                End If

                For j As Integer = 0 To equationParameterCount - coeffOffset - 1
                    Dim xv As Double = expandedX(row, j)
                    If Double.IsNaN(xv) OrElse Double.IsInfinity(xv) Then
                        Throw New ArgumentException("New predictor matrix contains invalid numeric values.")
                    End If
                    s += xv * b(baseIdx + coeffOffset + j)
                Next

                If offsetVals IsNot Nothing Then
                    Dim ov As Double = offsetVals(row)
                    If Double.IsNaN(ov) OrElse Double.IsInfinity(ov) Then
                        Throw New ArgumentException("New offset vector contains invalid numeric values.")
                    End If
                    s += ov
                End If

                eta(cat) = s
            Next

            Return eta
        End Function

        ''' <summary>
        ''' Converts category-specific linear predictors into full-category probabilities using the baseline-category softmax.
        ''' </summary>
        ''' <param name="eta">Linear predictors for the non-baseline categories.</param>
        ''' <returns>A probability vector of length K including the baseline category as the last element.</returns>
        Private Function ComputeMultinomialProbabilities(eta() As Double) As Double()
            Dim lse As Double = regression.CategoricalLogitUtils.LogSumExpBaselineZero(eta)
            Dim probs(eta.Length) As Double
            probs(eta.Length) = Math.Exp(-lse)

            For i As Integer = 0 To eta.Length - 1
                probs(i) = Math.Exp(eta(i) - lse)
            Next

            Return probs
        End Function

        ''' <summary>
        ''' Parses the requested residual-output block for the multinomial residual worksheet function.
        ''' </summary>
        ''' <param name="v">Worksheet residual-type argument.</param>
        ''' <returns>A canonical residual-output key.</returns>
        Private Function ParseMultinomialResidualType(v As Object) As String
            Dim s As String = UDFhelpers.AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case s.Trim().ToLowerInvariant()
                Case "all"
                    Return "all"
                Case "observed"
                    Return "observed"
                Case "fittedmean", "mean", "fittedmeans"
                    Return "fittedmean"
                Case "prob", "probability", "probabilities", "fittedprob", "fittedprobability"
                    Return "prob"
                Case "response", "raw"
                    Return "response"
                Case "pearson"
                    Return "pearson"
                Case "stdpearson", "standardizedpearson"
                    Return "stdpearson"
                Case "deviance", "dev"
                    Return "deviance"
                Case "stddeviance", "standardizeddeviance"
                    Return "stddeviance"
                Case "leverage", "hat"
                    Return "leverage"
                Case Else
                    Return "all"
            End Select
        End Function

        ''' <summary>
        ''' Builds category-specific column headers for residual or probability outputs.
        ''' </summary>
        ''' <param name="prefix">Prefix describing the quantity shown in each category column.</param>
        ''' <param name="categories">Outcome categories in model order.</param>
        ''' <returns>An array of column labels aligned with the category-specific matrix.</returns>
        Private Function CategoryHeaders(prefix As String, categories() As Integer) As String()
            If categories Is Nothing Then Return New String() {}
            Dim out(categories.Length - 1) As String
            For i As Integer = 0 To categories.Length - 1
                out(i) = prefix & "(" & categories(i).ToString(CultureInfo.InvariantCulture) & ")"
            Next
            Return out
        End Function

        ''' <summary>
        ''' Builds the full residual-output table used for <c>residType="all"</c>.
        ''' </summary>
        ''' <param name="res">Residual container from the fitted multinomial-logit model.</param>
        ''' <param name="categories">Outcome categories in model order.</param>
        ''' <param name="includeHeader">Whether to include a header row.</param>
        ''' <returns>A spilled-object array containing the full residual output.</returns>
        Private Function BuildAllResidualOutput(res As regression.MultinomialResiduals,
                                                categories() As Integer,
                                                includeHeader As Boolean) As Object
            If res Is Nothing Then Return ExcelError.ExcelErrorNA

            Dim cats() As Integer = categories
            Dim n As Integer = res.FittedMeans.GetLength(0)
            Dim k As Integer = cats.Length

            Dim headers As New List(Of String)()
            headers.AddRange(CategoryHeaders("Observed", cats))
            headers.AddRange(CategoryHeaders("FittedMean", cats))
            headers.AddRange(CategoryHeaders("Prob", cats))
            headers.AddRange(CategoryHeaders("Response", cats))
            headers.AddRange(CategoryHeaders("Pearson", cats))
            headers.AddRange(CategoryHeaders("StdPearson", cats))
            headers.Add("DevianceResidual")
            headers.Add("StdDevianceResidual")
            headers.Add("Leverage")

            Dim outRows As Integer = If(includeHeader, n + 1, n)
            Dim outCols As Integer = headers.Count
            Dim out(outRows - 1, outCols - 1) As Object
            Dim r0 As Integer = 0

            If includeHeader Then
                For j As Integer = 0 To outCols - 1
                    out(0, j) = headers(j)
                Next
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                Dim c As Integer = 0

                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.Observed(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.FittedMeans(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.Probabilities(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.ResponseResiduals(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.PearsonResiduals(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.StdPearsonResiduals(i, j) : c += 1
                Next

                out(r0 + i, c) = res.DevianceResiduals(i) : c += 1
                out(r0 + i, c) = res.StdDevianceResiduals(i) : c += 1
                out(r0 + i, c) = res.Leverage(i)
            Next

            Return out
        End Function

        ''' <summary>
        ''' Wraps a category-specific residual matrix in a spilled-object array.
        ''' </summary>
        ''' <param name="mat">Residual matrix with one row per observation and one column per category.</param>
        ''' <param name="headers">Column headers aligned with <paramref name="mat"/>.</param>
        ''' <param name="includeHeader">Whether to include a header row.</param>
        ''' <returns>A spilled-object array containing the requested residual matrix.</returns>
        Private Function BuildResidualMatrixOutput(mat(,) As Double,
                                                   headers() As String,
                                                   includeHeader As Boolean) As Object
            If mat Is Nothing Then Return ExcelError.ExcelErrorNA

            Dim n As Integer = mat.GetLength(0)
            Dim p As Integer = mat.GetLength(1)
            Dim outRows As Integer = If(includeHeader, n + 1, n)
            Dim out(outRows - 1, p - 1) As Object
            Dim r0 As Integer = 0

            If includeHeader Then
                For j As Integer = 0 To p - 1
                    out(0, j) = headers(j)
                Next
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To p - 1
                    out(r0 + i, j) = mat(i, j)
                Next
            Next

            Return out
        End Function

        ''' <summary>
        ''' Wraps a residual or leverage vector in a spilled-object array.
        ''' </summary>
        ''' <param name="vec">Vector of per-observation values.</param>
        ''' <param name="header">Column label to use when <paramref name="includeHeader"/> is True.</param>
        ''' <param name="includeHeader">Whether to include a header row.</param>
        ''' <returns>A spilled-object array containing the requested vector.</returns>
        Private Function BuildResidualVectorOutput(vec() As Double,
                                                   header As String,
                                                   includeHeader As Boolean) As Object
            If vec Is Nothing Then Return ExcelError.ExcelErrorNA

            Dim n As Integer = vec.Length
            Dim outRows As Integer = If(includeHeader, n + 1, n)
            Dim out(outRows - 1, 0) As Object
            Dim r0 As Integer = 0

            If includeHeader Then
                out(0, 0) = header
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                out(r0 + i, 0) = vec(i)
            Next

            Return out
        End Function

    End Module

End Namespace
