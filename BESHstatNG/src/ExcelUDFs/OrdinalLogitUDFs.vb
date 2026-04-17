Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for proportional-odds ordinal logistic regression models.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These functions fit and interrogate cumulative-logit ordinal regression models of the form
    ''' <c>logit(P(Y ≤ c_k)) = α_k - η</c>, where <c>η = x'β + offset</c>, the outcome has an intrinsic ordering,
    ''' and the slope vector <c>β</c> is assumed to be common across all cumulative logits (the proportional-odds assumption).
    ''' </para>
    ''' <para>
    ''' The fitted model is identified by a handle returned by <c>BESH.REGR.ORDLOGIT_FIT</c>. The handle can then be reused
    ''' by the other worksheet functions in this module to return coefficient tables, global tests, classification summaries,
    ''' residual diagnostics, and predictions without refitting the model.
    ''' </para>
    ''' <para>
    ''' Predictor formulas reuse the same formula infrastructure that is currently available for the Cox UDFs. This means
    ''' additive terms, polynomial terms, continuous-variable interactions, and categorical main effects can be defined from the raw
    ''' predictor matrix supplied to the fit function.
    ''' </para>
    ''' </remarks>
    Public Module OrdinalLogitUDFs

        ''' <summary>
        ''' In-memory cache of fitted ordinal-logit handles for the current Excel session.
        ''' </summary>
        Private ReadOnly _ordCache As New ConcurrentDictionary(Of String, OrdinalLogitHandle)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>
        ''' Stores a fitted ordinal-logit model together with the metadata required for summaries and prediction.
        ''' </summary>
        Private Class OrdinalLogitHandle
            Public Property Handle As String
            Public Property Model As regression.OrdinalLogitModel
            Public Property VarNames As String()
            Public Property ExpandedPredictorNames As String()
            Public Property RawVarNames As String()
            Public Property RawPredictorKeys As String()
            Public Property RawPredictorAbsoluteLetters As String()
            Public Property DesignSpec As RegressionFormulaDesignSpec
            Public Property OmitCategoricalReference As Boolean
            Public Property Reference As regression.ReferenceCategory
            Public Property CategoriesInModelOrder As Integer()
            Public Property PredictorCount As Integer
            Public Property HasOffset As Boolean
            Public Property Alpha As Double
        End Class

        ''' <summary>
        ''' Fits a proportional-odds ordinal logistic regression model and returns a reusable model handle.
        ''' </summary>
        ''' <param name="y">
        ''' A single-column numeric range containing the ordinal outcome.
        ''' Values must be finite integers representing the ordered response categories.
        ''' The observed categories are sorted and used as the ordinal scale of the model.
        ''' </param>
        ''' <param name="x">
        ''' A numeric predictor matrix with one row per observation and one column per raw predictor.
        ''' The predictor matrix can be used directly or transformed internally by the optional <paramref name="formula"/>.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional raw predictor names.
        ''' This may be supplied as a comma-separated text string or as a one-row or one-column range containing one name per raw predictor column.
        ''' If omitted, default names such as X1, X2, … are assigned automatically.
        ''' </param>
        ''' <param name="offset">
        ''' Optional numeric offset vector with one value per observation.
        ''' When supplied, the offset is added to the linear predictor and treated as known rather than estimated.
        ''' </param>
        ''' <param name="weights">
        ''' Optional nonnegative case weights.
        ''' Positive weights act like replicate or importance weights in the log-likelihood. Rows with nonpositive or invalid weights are excluded before fitting.
        ''' </param>
        ''' <param name="reference">
        ''' Optional direction / reference choice for the ordered outcome scale.
        ''' Accepted values are <c>last</c> (default) and <c>first</c>.
        ''' The choice changes the internal ordering used by the cumulative logits and therefore changes the interpretation of the thresholds.
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
        ''' A text handle identifying the fitted ordinal-logit model within the current Excel session.
        ''' The handle can be passed to the other <c>ORDLOGIT_*</c> worksheet functions to obtain summaries, diagnostics, and predictions without refitting the model.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The proportional-odds ordinal logistic model uses one common slope vector for all cumulative splits of the ordered response,
        ''' while estimating a separate threshold (cutpoint) for each adjacent outcome boundary.
        ''' Unlike ordinary binary logistic regression, the thresholds play the role of intercept terms and a separate free intercept column in <paramref name="x"/> is not identifiable.
        ''' </para>
        ''' <para>
        ''' Rows containing invalid values in the response, predictors, offset, or weights are excluded before fitting. At least two distinct ordered outcome categories must remain.
        ''' </para>
        ''' <para>
        ''' If <c>formulaAddressing="absolute"</c> is used, the <paramref name="x"/> argument should be passed as a direct worksheet range so that absolute worksheet column letters can be determined.
        ''' </para>
        ''' <para>
        ''' Residual diagnostics are computed during fitting so that <c>BESH.REGR.ORDLOGIT_RESID</c> can reuse the fitted object without forcing an additional refit.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.ORDLOGIT_FIT(A2:A101,B2:D101,"dose,age,prison")
        ''' =BESH.REGR.ORDLOGIT_FIT(A2:A101,B2:E101,"dose,age,prison,stage",,,"last","A + B + factor(D, ref=1) + 'dose':'age'","names",100,1E-8,0.05)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.ORDLOGIT_FIT",
            Category:="BESHStatNG - Regression Models",
            Description:="Fits a proportional-odds ordinal logistic regression model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ORDLOGIT_FIT(
            <ExcelArgument(Name:="y", Description:="Ordinal outcome (single numeric column of ordered category codes).")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Raw predictor matrix with one row per observation and one column per raw predictor.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional raw predictor names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="offset", Description:="Optional numeric offset vector (one column).")> Optional offset As Object = Nothing,
            <ExcelArgument(Name:="weights", Description:="Optional nonnegative case weights (one column).")> Optional weights As Object = Nothing,
            <ExcelArgument(Name:="reference", Description:="Outcome ordering reference: ""last"" (default) or ""first"".")> Optional reference As Object = Nothing,
            <ExcelArgument(Name:="formula", Description:="Optional RHS model formula built from the raw predictor matrix.")> Optional formula As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Formula-addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing,
            <ExcelArgument(Name:="maxIter", Description:="Maximum number of fitting iterations (default 50).")> Optional maxIter As Object = Nothing,
            <ExcelArgument(Name:="tol", Description:="Convergence tolerance (default 1E-10).")> Optional tol As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha used for internal confidence intervals (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "ORDLOGIT_FIT (editing...)"

            Try

                Dim imported As glmData = Nothing
                If Not UDFhelpers.TryBuildGlmDataFromUdfArgs(y, x, varNames, offset, weights, imported) Then
                    Return ExcelError.ExcelErrorValue
                End If

                If imported.nCols < 2 Then Return ExcelError.ExcelErrorNum

                Dim yVals As List(Of Integer) = Nothing
                If Not UDFhelpers.TryExtractIntegerOutcomeColumn(imported, yVals) Then
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

                If fitData Is Nothing OrElse fitVarNames Is Nothing OrElse fitVarNames.Length < 2 Then
                    Return ExcelError.ExcelErrorValue
                End If
                If Not UDFhelpers.HasOnlyFinite(fitOffset) Then Return ExcelError.ExcelErrorValue
                If Not UDFhelpers.HasOnlyFinite(fitWeights, True) Then Return ExcelError.ExcelErrorValue

                Dim distinctCats As Integer = CountDistinctOutcomeCategories(fitData)
                If distinctCats < 2 Then Return ExcelError.ExcelErrorNum

                Dim alphaValue As Double = 0.05
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim maxIterValue As Integer = UDFhelpers.GetOptionalInt(maxIter, 50)
                Dim tolValue As Double = UDFhelpers.GetOptionalDouble(tol, 0.0000000001R)
                If maxIterValue < 1 Then Return ExcelError.ExcelErrorNum
                If Double.IsNaN(tolValue) OrElse Double.IsInfinity(tolValue) OrElse tolValue <= 0 Then Return ExcelError.ExcelErrorNum

                Dim refCat As regression.ReferenceCategory = ParseReferenceCategory(reference)

                Dim ord As New regression.OrdinalLogitModel()
                ord.bComputeResiduals = True
                ord.bReturnCov = False
                ord.bIterationDetails = False
                ord.SettingInputs(alphaValue, maxIterValue, tolValue)
                ord.Data(fitData,
                         fitVarNames,
                         rowIds,
                         fitOffset,
                         fitWeights)
                ord.Fit(refCat, False)

                Dim cats() As Integer = Nothing
                If ord.Classification IsNot Nothing AndAlso ord.Classification.Categories IsNot Nothing Then
                    cats = DirectCast(ord.Classification.Categories.Clone(), Integer())
                End If

                Dim handleKey As String = "ORDLOGIT:" & Guid.NewGuid().ToString("N")
                Dim h As New OrdinalLogitHandle With {
                    .Handle = handleKey,
                    .Model = ord,
                    .VarNames = DirectCast(ord.results.varNames.Clone(), String()),
                    .ExpandedPredictorNames = DirectCast(fitPredictorNames.Clone(), String()),
                    .RawVarNames = If(designBuild.FullRawPredictorNames, New String() {}),
                    .RawPredictorKeys = If(designBuild.FullRawPredictorKeys, New String() {}),
                    .RawPredictorAbsoluteLetters = If(designBuild.FullRawPredictorAbsoluteLetters, New String() {}),
                    .DesignSpec = designBuild.DesignSpec,
                    .OmitCategoricalReference = True,
                    .Reference = refCat,
                    .CategoriesInModelOrder = cats,
                    .PredictorCount = fitPredictorNames.Length,
                    .HasOffset = (fitOffset IsNot Nothing),
                    .Alpha = alphaValue
                }

                _ordCache(handleKey) = h
                Return handleKey

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.ORDLOGIT_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a coefficient table for a fitted ordinal-logit model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ORDLOGIT_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <param name="alpha">Optional two-sided alpha for confidence intervals and odds-ratio confidence limits (default 0.05).</param>
        ''' <returns>
        ''' A spilled array containing one row per estimated parameter. Slope parameters are accompanied by odds ratios and odds-ratio confidence limits;
        ''' threshold parameters leave the odds-ratio columns blank because exponentiated thresholds are generally not interpreted as odds ratios for a predictor effect.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The first block of parameters corresponds to the common slope vector for the predictors. The remaining parameters are threshold (cutpoint) terms
        ''' that separate adjacent levels of the ordered response scale.
        ''' </para>
        ''' <para>
        ''' For slope parameters, exponentiating the coefficient gives the proportional-odds ratio associated with a one-unit increase in the predictor while the other predictors are held fixed.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.ORDLOGIT_SUMMARY(F2)
        ''' =BESH.REGR.ORDLOGIT_SUMMARY(F2,TRUE,0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.ORDLOGIT_SUMMARY",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the parameter summary table for a fitted ordinal-logit model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ORDLOGIT_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ORDLOGIT_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for confidence intervals (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As OrdinalLogitHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim alphaValue As Double = 0.05
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
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

                    Dim isSlope As Boolean = (i < h.PredictorCount)

                    out(r0 + i, 0) = h.VarNames(i)
                    out(r0 + i, 1) = If(isSlope, "Slope", "Threshold")
                    out(r0 + i, 2) = beta
                    out(r0 + i, 3) = se
                    out(r0 + i, 4) = z
                    out(r0 + i, 5) = pv

                    If isSlope Then
                        out(r0 + i, 6) = ExpForDisplay(beta)
                        out(r0 + i, 7) = ExpForDisplay(beta - zCrit * se)
                        out(r0 + i, 8) = ExpForDisplay(beta + zCrit * se)
                    Else
                        out(r0 + i, 6) = ""
                        out(r0 + i, 7) = ""
                        out(r0 + i, 8) = ""
                    End If
                Next

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.ORDLOGIT_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns global model tests and fit statistics for a fitted ordinal-logit model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ORDLOGIT_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing model-level diagnostics such as log-likelihoods, likelihood-ratio and goodness-of-fit tests,
        ''' pseudo-R² measures, information criteria, iteration count, and convergence information.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' These diagnostics are taken from the fitted model object without refitting the model. The exact rows mirror the diagnostic quantities stored
        ''' by <see cref="regression.OrdinalLogitModel"/> in its shared regression-results container.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.ORDLOGIT_TESTS(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.ORDLOGIT_TESTS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns model-level diagnostics and tests for a fitted ordinal-logit model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ORDLOGIT_TESTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ORDLOGIT_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As OrdinalLogitHandle = Nothing
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
                Return LoggedUdfExceptionText("BESH.REGR.ORDLOGIT_TESTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the observed-versus-predicted classification table for a fitted ordinal-logit model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ORDLOGIT_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include header rows and labels (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing the weighted or unweighted confusion matrix, per-row recall percentages,
        ''' per-column precision percentages, and overall classification accuracy.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The classification table is based on assigning each observation to the category with the largest fitted probability.
        ''' The category columns are shown in the model's internal category order, which depends on the reference-direction choice used during fitting.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.ORDLOGIT_CLASS(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.ORDLOGIT_CLASS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the classification confusion matrix for a fitted ordinal-logit model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ORDLOGIT_CLASS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ORDLOGIT_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include header rows and labels (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As OrdinalLogitHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim cls = h.Model.Classification
                If cls Is Nothing OrElse cls.Categories Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim k As Integer = cls.Categories.Length
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, k + 3, k + 2)
                Dim outCols As Integer = k + 2
                Dim out(outRows - 1, outCols - 1) As Object

                Dim r As Integer = 0
                If hdr Then
                    out(0, 0) = "Observed \ Predicted"
                    For j As Integer = 0 To k - 1
                        out(0, j + 1) = cls.Categories(j)
                    Next
                    out(0, outCols - 1) = "Recall %"
                    r = 1
                End If

                For i As Integer = 0 To k - 1
                    out(r + i, 0) = cls.Categories(i)
                    For j As Integer = 0 To k - 1
                        out(r + i, j + 1) = cls.Counts(i, j)
                    Next
                    out(r + i, outCols - 1) = cls.RecallPct(i)
                Next

                out(r + k, 0) = "Precision %"
                For j As Integer = 0 To k - 1
                    out(r + k, j + 1) = cls.PrecisionPct(j)
                Next
                out(r + k, outCols - 1) = ""

                out(r + k + 1, 0) = "Overall accuracy %"
                out(r + k + 1, 1) = cls.OverallAccuracyPct
                For j As Integer = 2 To outCols - 1
                    out(r + k + 1, j) = ""
                Next

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.ORDLOGIT_CLASS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns residual diagnostics for a fitted ordinal-logit model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ORDLOGIT_FIT</c>.</param>
        ''' <param name="residType">
        ''' Residual block to return.
        ''' Accepted values are <c>all</c> (default), <c>fittedmean</c>, <c>prob</c>, <c>response</c>, <c>pearson</c>, <c>stdpearson</c>,
        ''' <c>deviance</c>, <c>stddeviance</c>, and <c>leverage</c>.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing the requested residual-related output.
        ''' Category-specific outputs return one column per outcome category in the model's internal category order.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The <c>all</c> view reproduces the main residual blocks computed by the fitted ordinal model:
        ''' fitted means, fitted probabilities, raw response residuals, Pearson residuals, standardized Pearson residuals,
        ''' deviance residuals, standardized deviance residuals, and leverage.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.ORDLOGIT_RESID(F2)
        ''' =BESH.REGR.ORDLOGIT_RESID(F2,"pearson")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.ORDLOGIT_RESID",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns residual diagnostics for a fitted ordinal-logit model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ORDLOGIT_RESID(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ORDLOGIT_FIT.")> handle As Object,
            <ExcelArgument(Name:="residType", Description:="Residual output block: ""all"", ""fittedmean"", ""prob"", ""response"", ""pearson"", ""stdpearson"", ""deviance"", ""stddeviance"", or ""leverage"".")> Optional residType As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As OrdinalLogitHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim res = h.Model.Residuals
                If res Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim key As String = ParseOrdinalResidualType(residType)
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)

                Select Case key
                    Case "all"
                        Return BuildAllResidualOutput(res, hdr)

                    Case "fittedmean"
                        Return BuildResidualMatrixOutput(res.FittedMeans, CategoryHeaders("FittedMean", res.Categories), hdr)

                    Case "prob"
                        Return BuildResidualMatrixOutput(res.Probabilities, CategoryHeaders("Prob", res.Categories), hdr)

                    Case "response"
                        Return BuildResidualMatrixOutput(res.ResponseResiduals, CategoryHeaders("Response", res.Categories), hdr)

                    Case "pearson"
                        Return BuildResidualMatrixOutput(res.PearsonResiduals, CategoryHeaders("Pearson", res.Categories), hdr)

                    Case "stdpearson"
                        Return BuildResidualMatrixOutput(res.StdPearsonResiduals, CategoryHeaders("StdPearson", res.Categories), hdr)

                    Case "deviance"
                        Return BuildResidualVectorOutput(res.DevianceResiduals, "DevianceResidual", hdr)

                    Case "stddeviance"
                        Return BuildResidualVectorOutput(res.StdDevianceResiduals, "StdDevianceResidual", hdr)

                    Case "leverage"
                        Return BuildResidualVectorOutput(res.Leverage, "Leverage", hdr)

                    Case Else
                        Return BuildAllResidualOutput(res, hdr)
                End Select

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.ORDLOGIT_RESID", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns fitted probabilities and the most likely category for new predictor values under a fitted ordinal-logit model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ORDLOGIT_FIT</c>.</param>
        ''' <param name="newX">
        ''' New raw predictor matrix with the same raw-column structure used at model fitting time.
        ''' If the fitted model used a formula, the required expanded design matrix is rebuilt internally from this raw matrix.
        ''' </param>
        ''' <param name="newOffset">
        ''' Optional offset vector for the new observations.
        ''' If the fitted model used an offset, a matching new offset must be supplied here.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing the linear predictor, the most likely predicted category, and one fitted probability column per outcome category.
        ''' Probability columns are returned in the model's internal category order.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The probability columns sum to 1 across each row, up to normal floating-point rounding error. The predicted category is the category whose fitted
        ''' probability is largest in the returned probability vector.
        ''' </para>
        ''' <para>
        ''' When the model was fitted with an offset, <paramref name="newOffset"/> must be supplied and aligned with the rows of <paramref name="newX"/>.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.ORDLOGIT_PRED(F2,H2:J10)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.ORDLOGIT_PRED",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns fitted probabilities and predicted categories for new data under a fitted ordinal-logit model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ORDLOGIT_PRED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ORDLOGIT_FIT.")> handle As Object,
            <ExcelArgument(AllowReference:=True, Name:="newX", Description:="New raw predictor matrix in the same raw-column order used at fitting time.")> newX As Object,
            <ExcelArgument(Name:="newOffset", Description:="Optional offset vector for the new observations.")> Optional newOffset As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As OrdinalLogitHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim rawPredictorKeys As String() = If(h.RawPredictorKeys, h.RawVarNames)
                If rawPredictorKeys Is Nothing OrElse rawPredictorKeys.Length < 1 Then Return ExcelError.ExcelErrorValue

                Dim imported As glmData = Nothing
                If Not UDFhelpers.TryBuildPredictorDataFromUdfArgs(newX, rawPredictorKeys, newOffset, h.HasOffset, imported) Then
                    Return ExcelError.ExcelErrorValue
                End If

                If imported.nCols <> rawPredictorKeys.Length Then Return ExcelError.ExcelErrorValue

                Dim expandedX As Double(,) = Nothing
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

                If expandedX Is Nothing OrElse expandedNames Is Nothing Then Return ExcelError.ExcelErrorValue
                If expandedNames.Length <> h.PredictorCount Then Return ExcelError.ExcelErrorValue

                Dim offsetVals() As Double = If(imported.bOffset, imported.OffsetData, Nothing)
                If Not UDFhelpers.HasOnlyFinite(offsetVals) Then Return ExcelError.ExcelErrorValue

                Dim b() As Double = h.Model.results.Coeffs_est
                Dim cats() As Integer = h.CategoriesInModelOrder
                If b Is Nothing OrElse cats Is Nothing OrElse cats.Length < 2 Then Return ExcelError.ExcelErrorNA

                Dim nRows As Integer = imported.nRows
                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, nRows + 1, nRows)
                Dim outCols As Integer = cats.Length + 2
                Dim out(outRows - 1, outCols - 1) As Object

                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "PredictedCategory"
                    out(0, 1) = "LinearPredictor"
                    For k As Integer = 0 To cats.Length - 1
                        out(0, k + 2) = "P(Y=" & cats(k).ToString(CultureInfo.InvariantCulture) & ")"
                    Next
                    r0 = 1
                End If

                For i As Integer = 0 To nRows - 1
                    Dim eta As Double = 0.0
                    For j As Integer = 0 To h.PredictorCount - 1
                        Dim xv As Double = expandedX(i, j)
                        If Double.IsNaN(xv) OrElse Double.IsInfinity(xv) Then Return ExcelError.ExcelErrorValue
                        eta += xv * b(j)
                    Next
                    If offsetVals IsNot Nothing Then
                        eta += offsetVals(i)
                    End If

                    Dim probs() As Double = ComputeOrdinalProbabilities(b, h.PredictorCount, cats.Length, eta)
                    Dim predIdx As Integer = regression.CategoricalLogitUtils.ArgMax(probs, True)

                    out(r0 + i, 0) = cats(predIdx)
                    out(r0 + i, 1) = eta
                    For k As Integer = 0 To cats.Length - 1
                        out(r0 + i, k + 2) = probs(k)
                    Next
                Next

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.ORDLOGIT_PRED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a fitted ordinal-logit handle from the in-memory cache.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.ORDLOGIT_FIT</c>.</param>
        ''' <returns>TRUE if the handle existed and was removed; otherwise FALSE.</returns>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.ORDLOGIT_DROP(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.ORDLOGIT_DROP",
            Category:="BESHStatNG - Regression Models",
            Description:="Removes a fitted ordinal-logit model handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function ORDLOGIT_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.ORDLOGIT_FIT.")> handle As Object
        ) As Object
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return ExcelError.ExcelErrorValue
            Dim removed As OrdinalLogitHandle = Nothing
            Return _ordCache.TryRemove(key, removed)
        End Function

        ''' <summary>
        ''' Attempts to resolve a cached ordinal-logit handle.
        ''' </summary>
        ''' <param name="handle">Worksheet handle argument.</param>
        ''' <param name="h">On success, receives the cached handle object.</param>
        ''' <returns>True when the handle exists in the cache; otherwise, False.</returns>
        Private Function TryGetHandle(handle As Object, ByRef h As OrdinalLogitHandle) As Boolean
            h = Nothing
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _ordCache.TryGetValue(key, h)
        End Function

        ''' <summary>
        ''' Computes fitted category probabilities for a single row from the estimated slope and threshold parameters.
        ''' </summary>
        ''' <param name="b">Full parameter vector [β, α].</param>
        ''' <param name="p">Number of slope parameters.</param>
        ''' <param name="k">Number of ordered outcome categories.</param>
        ''' <param name="eta">Linear predictor value for the row.</param>
        ''' <returns>A probability vector of length <paramref name="k"/> in the model's internal category order.</returns>
        Private Function ComputeOrdinalProbabilities(b() As Double,
                                                     p As Integer,
                                                     k As Integer,
                                                     eta As Double) As Double()
            Dim probs(k - 1) As Double
            Dim prev As Double = 0.0R

            For thr As Integer = 0 To k - 2
                Dim alpha As Double = b(p + thr)
                Dim Fk As Double = regression.Logit.LogisticStable(alpha - eta)
                Dim pk As Double = If(thr = 0, Fk, Fk - prev)
                probs(thr) = Math.Max(0.0R, pk)
                prev = Fk
            Next

            probs(k - 1) = Math.Max(0.0R, 1.0R - prev)

            Dim s As Double = 0.0R
            For i As Integer = 0 To k - 1
                s += probs(i)
            Next

            If s > 0.0R Then
                For i As Integer = 0 To k - 1
                    probs(i) /= s
                Next
            End If

            Return probs
        End Function

        ''' <summary>
        ''' Parses the requested residual-output block for the ordinal residual worksheet function.
        ''' </summary>
        ''' <param name="v">Worksheet residual-type argument.</param>
        ''' <returns>A canonical residual-output key.</returns>
        Private Function ParseOrdinalResidualType(v As Object) As String
            Dim s As String = UDFhelpers.AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case s.Trim().ToLowerInvariant()
                Case "all"
                    Return "all"
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
        ''' Builds the full residual-output table used for <c>residType="all"</c>.
        ''' </summary>
        ''' <param name="res">Residual container from the fitted ordinal-logit model.</param>
        ''' <param name="includeHeader">Whether to include a header row.</param>
        ''' <returns>A spilled-object array containing the full residual output.</returns>
        Private Function BuildAllResidualOutput(res As regression.MultinomialResiduals,
                                                includeHeader As Boolean) As Object
            If res Is Nothing Then Return ExcelError.ExcelErrorNA

            Dim cats() As Integer = res.Categories
            Dim n As Integer = res.FittedMeans.GetLength(0)
            Dim k As Integer = cats.Length

            Dim headers As New List(Of String)()
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

    End Module

End Namespace
