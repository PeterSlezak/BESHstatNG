Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports ExcelDna.Integration

Namespace WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for Gaussian linear regression models fitted by ordinary or weighted least squares.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These functions fit and interrogate linear regression models of the form
    ''' <c>Y = β0 + β1X1 + … + βpXp + offset + ε</c>, where the response is continuous,
    ''' the mean is modeled on the identity scale, and observation weights may optionally be supplied.
    ''' </para>
    ''' <para>
    ''' The fitted model is identified by a handle returned by <c>BESH.REGR.LM_FIT</c>. The handle can then be reused
    ''' by the other worksheet functions in this module to return coefficient summaries, model diagnostics, ANOVA tables,
    ''' variance-inflation factors, residual diagnostics, predictions, and explicit cleanup without refitting the model.
    ''' </para>
    ''' <para>
    ''' Predictor formulas reuse the same formula infrastructure that is currently available for the Cox, multinomial-logit,
    ''' and ordinal-logit UDFs. This means additive terms, polynomial terms, continuous-variable interactions, and categorical
    ''' main effects can be defined from the raw predictor matrix supplied to the fit function.
    ''' </para>
    ''' </remarks>
    Public Module LinearModelUDFs

        ''' <summary>
        ''' In-memory cache of fitted linear-model handles for the current Excel session.
        ''' </summary>
        Private ReadOnly _lmCache As New ConcurrentDictionary(Of String, LinearModelHandle)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>
        ''' Stores a fitted linear model together with the metadata required for summaries and prediction.
        ''' </summary>
        Private Class LinearModelHandle
            Public Property Handle As String
            Public Property Model As regression.LinearModel
            Public Property ExpandedPredictorNames As String()
            Public Property RawVarNames As String()
            Public Property RawPredictorKeys As String()
            Public Property RawPredictorAbsoluteLetters As String()
            Public Property DesignSpec As RegressionFormulaDesignSpec
            Public Property OmitCategoricalReference As Boolean
            Public Property IncludeIntercept As Boolean
            Public Property HasOffset As Boolean
            Public Property HasWeights As Boolean
            Public Property PredictorCount As Integer
            Public Property Alpha As Double
            Public Property PredictorCodingFootnotes As String()
            Public Property TypeIAnovaTable As Object(,)
            Public Property TypeIIIAnovaTable As Object(,)
        End Class

        ''' <summary>
        ''' Fits a Gaussian linear regression model and returns a reusable model handle.
        ''' </summary>
        ''' <param name="y">
        ''' A single-column numeric range containing the continuous response.
        ''' Non-numeric or invalid rows are excluded by the shared regression-data import machinery before fitting.
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
        ''' When supplied, the offset is added to the fitted mean and treated as known rather than estimated.
        ''' </param>
        ''' <param name="weights">
        ''' Optional positive case weights.
        ''' When supplied, the fitted coefficients minimize the weighted sum of squared residuals.
        ''' Rows with nonpositive or invalid weights are excluded by the shared regression-data import machinery before fitting.
        ''' </param>
        ''' <param name="includeIntercept">
        ''' TRUE to include an intercept term (default TRUE).
        ''' Set FALSE to fit a model through the origin after any formula-based predictor expansion.
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
        ''' <param name="computeResiduals">
        ''' TRUE to store observation-level residual diagnostics for later use by <c>BESH.REGR.LM_RESID</c> (default TRUE).
        ''' Set FALSE to reduce memory use when residual diagnostics will not be requested.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for confidence intervals stored with the fitted model (default 0.05).
        ''' This does not affect the estimated coefficients themselves.
        ''' </param>
        ''' <returns>
        ''' A text handle identifying the fitted linear model within the current Excel session.
        ''' The handle can be passed to the other <c>LM_*</c> worksheet functions to obtain summaries, diagnostics, and predictions without refitting the model.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' When an offset is supplied, the fitted mean is <c>offset + Xβ</c>. Internally this is implemented by fitting the adjusted response
        ''' <c>Y - offset</c> against the estimated terms, while predictions returned by <c>BESH.REGR.LM_PRED</c> add the offset back.
        ''' </para>
        ''' <para>
        ''' Rows containing invalid values in the response, predictors, offset, or weights are excluded before fitting.
        ''' If <c>formulaAddressing="absolute"</c> is used, the <paramref name="x"/> argument should be passed as a direct worksheet range
        ''' so that absolute worksheet column letters can be determined.
        ''' </para>
        ''' <para>
        ''' Term-wise ANOVA tables are prepared in both sequential (Type I) and partial (Type III) forms so that
        ''' <c>BESH.REGR.LM_ANOVA</c> can return either table without forcing an additional refit.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.LM_FIT(A2:A101,B2:D101,"dose,age,weight")
        ''' =BESH.REGR.LM_FIT(A2:A101,B2:E101,"dose,age,stage,treat",,F2:F101,TRUE,"A + B + factor(C, ref=1) + 'dose':'age'","names",TRUE,0.05)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.LM_FIT",
            Category:="BESHStatNG - Regression Models",
            Description:="Fits a Gaussian linear regression model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LM_FIT(
            <ExcelArgument(Name:="y", Description:="Continuous response (single numeric column).")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Raw predictor matrix with one row per observation and one column per raw predictor.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional raw predictor names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="offset", Description:="Optional numeric offset vector (one column).")> Optional offset As Object = Nothing,
            <ExcelArgument(Name:="weights", Description:="Optional positive case weights (one column).")> Optional weights As Object = Nothing,
            <ExcelArgument(Name:="includeIntercept", Description:="TRUE to include an intercept term (default TRUE).")> Optional includeIntercept As Object = Nothing,
            <ExcelArgument(Name:="formula", Description:="Optional RHS model formula built from the raw predictor matrix.")> Optional formula As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Formula-addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing,
            <ExcelArgument(Name:="computeResiduals", Description:="TRUE to store residual diagnostics for later use by LM_RESID (default TRUE).")> Optional computeResiduals As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha used for stored confidence intervals (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "LM_FIT (editing...)"

            Try
                Dim imported As glmData = Nothing
                If Not UDFhelpers.TryBuildGlmDataFromUdfArgs(y, x, varNames, offset, weights, imported) Then
                    Return ExcelError.ExcelErrorValue
                End If

                If imported.nCols < 2 Then Return ExcelError.ExcelErrorNum

                Dim formulaText As String = AsString(formula)
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

                Dim fitDataAdjusted(,) As Double = DirectCast(fitData.Clone(), Double(,))
                If fitOffset IsNot Nothing Then
                    For i As Integer = 0 To fitDataAdjusted.GetLength(0) - 1
                        fitDataAdjusted(i, 0) -= fitOffset(i)
                    Next
                End If

                Dim alphaValue As Double = 0.05
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim interceptFlag As Boolean = GetOptionalBool(includeIntercept, True)
                Dim residualFlag As Boolean = GetOptionalBool(computeResiduals, True)

                Dim rawBaseDisplayNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
                If designBuild.VariableCatalog IsNot Nothing AndAlso designBuild.VariableCatalog.Variables IsNot Nothing Then
                    For Each entry As RegressionVariableCatalogEntry In designBuild.VariableCatalog.Variables
                        If entry Is Nothing Then Continue For
                        Dim baseKey As String = If(entry.BaseKey, String.Empty).Trim()
                        If baseKey = String.Empty Then Continue For
                        Dim displayName As String = If(entry.DisplayName, String.Empty).Trim()
                        If displayName = String.Empty Then displayName = baseKey
                        rawBaseDisplayNames(baseKey) = displayName
                    Next
                End If

                Dim customGroups As Dictionary(Of String, Integer()) = RegressionDesignCore.BuildCustomTermGroupsForLm(
                    effectItems:=designBuild.DesignSpec.EffectItems,
                    termSpecs:=designBuild.DesignSpec.TermSpecs,
                    includeIntercept:=interceptFlag,
                    rawBaseDisplayNames:=rawBaseDisplayNames)

                Dim codingNotes As List(Of String) = RegressionDesignCore.BuildCategoricalReferenceFootnotesForLm(
                    raw:=imported,
                    effectItems:=designBuild.DesignSpec.EffectItems,
                    termSpecs:=designBuild.DesignSpec.TermSpecs,
                    includeIntercept:=interceptFlag,
                    rawBaseDisplayNames:=rawBaseDisplayNames)

                Dim lm As New regression.LinearModel()
                lm.Alpha = alphaValue
                lm.bComputeResiduals = residualFlag
                lm.bReturnCov = False
                lm.Data(fitDataAdjusted, fitVarNames, rowIds, fitWeights)
                lm.SetPredictorCodingFootnotes(codingNotes)
                lm.Fit(interceptFlag, customGroups, regression.TermSumOfSquaresType.TypeIII)

                Dim lmTypeI As New regression.LinearModel()
                lmTypeI.Alpha = alphaValue
                lmTypeI.bComputeResiduals = False
                lmTypeI.bReturnCov = False
                lmTypeI.Data(fitDataAdjusted, fitVarNames, rowIds, fitWeights)
                lmTypeI.SetPredictorCodingFootnotes(codingNotes)
                lmTypeI.Fit(interceptFlag, customGroups, regression.TermSumOfSquaresType.TypeI)

                Dim handleKey As String = "LM:" & Guid.NewGuid().ToString("N")
                Dim h As New LinearModelHandle With {
                    .Handle = handleKey,
                    .Model = lm,
                    .ExpandedPredictorNames = DirectCast(fitPredictorNames.Clone(), String()),
                    .RawVarNames = If(designBuild.FullRawPredictorNames, New String() {}),
                    .RawPredictorKeys = If(designBuild.FullRawPredictorKeys, New String() {}),
                    .RawPredictorAbsoluteLetters = If(designBuild.FullRawPredictorAbsoluteLetters, New String() {}),
                    .DesignSpec = designBuild.DesignSpec,
                    .OmitCategoricalReference = True,
                    .IncludeIntercept = interceptFlag,
                    .HasOffset = (fitOffset IsNot Nothing),
                    .HasWeights = (fitWeights IsNot Nothing),
                    .PredictorCount = fitPredictorNames.Length,
                    .Alpha = alphaValue,
                    .PredictorCodingFootnotes = If(codingNotes Is Nothing, New String() {}, codingNotes.ToArray()),
                    .TypeIAnovaTable = PrepareResultTableForUdf(lmTypeI.AnovaTypeI_toPrint.returnSelf()),
                    .TypeIIIAnovaTable = PrepareResultTableForUdf(lm.AnovaTypeIII_toPrint.returnSelf())
                }

                _lmCache(handleKey) = h
                Return handleKey

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.LM_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the coefficient summary table for a fitted linear-model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LM_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <param name="alpha">Optional two-sided alpha for confidence intervals (default 0.05).</param>
        ''' <returns>
        ''' A spilled array containing one row per estimated parameter with coefficient estimates,
        ''' standard errors, t statistics, p-values, and two-sided confidence limits.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' When an intercept is included, the intercept appears as its own parameter row.
        ''' For factor-coded predictors, each non-reference level contributes its own coefficient row.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.LM_SUMMARY(F2)
        ''' =BESH.REGR.LM_SUMMARY(F2,TRUE,0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.LM_SUMMARY",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the coefficient summary table for a fitted linear-model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LM_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LM_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for confidence intervals (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As LinearModelHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _lmCache, h) Then Return ExcelError.ExcelErrorNA

                Dim alphaValue As Double = h.Alpha
                If Not IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim coef As Double() = h.Model.results.Coeffs_est
                Dim se As Double() = h.Model.results.Coeffs_SEsT
                Dim dfResid As Double = h.Model.results.dfResid
                If coef Is Nothing OrElse se Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim q As Integer = coef.Length
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
                Dim noteCount As Integer = If(h.PredictorCodingFootnotes Is Nothing, 0, h.PredictorCodingFootnotes.Length)
                Dim outRows As Integer = q + If(hdr, 1, 0) + noteCount
                Dim out(outRows - 1, 7) As Object

                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "Parameter"
                    out(0, 1) = "Type"
                    out(0, 2) = "Coef"
                    out(0, 3) = "SE"
                    out(0, 4) = "T"
                    out(0, 5) = "P-value"
                    out(0, 6) = $"{100.0 * (1.0 - alphaValue):0.##}% CI LCL"
                    out(0, 7) = $"{100.0 * (1.0 - alphaValue):0.##}% CI UCL"
                    r0 = 1
                End If

                Dim tCrit As Double = distributions.T_Inv_2T(alphaValue, dfResid)
                For i As Integer = 0 To q - 1
                    Dim pname As String = If(h.Model.results.bIntercept AndAlso i = 0,
                                             "Intercept",
                                             h.Model.results.varNames(i - If(h.Model.results.bIntercept, 1, 0)))
                    Dim tVal As Double = If(se(i) > 0.0, coef(i) / se(i), Double.NaN)
                    Dim pVal As Double = If(se(i) > 0.0, distributions.T_2T(Math.Abs(tVal), dfResid), Double.NaN)

                    out(r0 + i, 0) = pname
                    out(r0 + i, 1) = If(h.Model.results.bIntercept AndAlso i = 0, "Intercept", "Slope")
                    out(r0 + i, 2) = coef(i)
                    out(r0 + i, 3) = se(i)
                    out(r0 + i, 4) = tVal
                    out(r0 + i, 5) = pVal
                    out(r0 + i, 6) = coef(i) - tCrit * se(i)
                    out(r0 + i, 7) = coef(i) + tCrit * se(i)
                Next

                AppendFootnotesInPlace(out, q + If(hdr, 1, 0), h.PredictorCodingFootnotes)
                Return PrepareResultTableForUdf(out)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.LM_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns model-level diagnostics and fit statistics for a fitted linear-model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LM_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing sample size, parameter count, degrees of freedom, R²,
        ''' adjusted R², the overall F test, log-likelihood, AIC, and BIC.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.LM_TESTS(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.LM_TESTS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns model-level diagnostics and fit statistics for a fitted linear-model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LM_TESTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LM_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As LinearModelHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _lmCache, h) Then Return ExcelError.ExcelErrorNA

                Dim labels() As String = h.Model.results.ModelTableLabels
                Dim vals(,) As Object = h.Model.results.ModelTableVals
                If labels Is Nothing OrElse vals Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim n As Integer = labels.Length
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
                Dim noteCount As Integer = If(h.PredictorCodingFootnotes Is Nothing, 0, h.PredictorCodingFootnotes.Length)
                Dim outRows As Integer = n + If(hdr, 1, 0) + noteCount
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

                AppendFootnotesInPlace(out, n + If(hdr, 1, 0), h.PredictorCodingFootnotes)
                Return PrepareResultTableForUdf(out)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.LM_TESTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns an overall, Type I, or Type III ANOVA table for a fitted linear-model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LM_FIT</c>.</param>
        ''' <param name="scope">
        ''' Optional ANOVA-table selector.
        ''' Accepted values are <c>overall</c> (default), <c>type1</c>, <c>typei</c>, <c>type3</c>, and <c>typeiii</c>.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include the title and header rows (default TRUE).</param>
        ''' <returns>A spilled array containing the requested ANOVA table.</returns>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.LM_ANOVA(F2)
        ''' =BESH.REGR.LM_ANOVA(F2,"type3")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.LM_ANOVA",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns an overall, Type I, or Type III ANOVA table for a fitted linear-model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LM_ANOVA(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LM_FIT.")> handle As Object,
            <ExcelArgument(Name:="scope", Description:="ANOVA selector: ""overall"" (default), ""type1"", or ""type3"".")> Optional scope As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include the title and header rows (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As LinearModelHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _lmCache, h) Then Return ExcelError.ExcelErrorNA

                Dim which As String = ParseLmAnovaScope(scope)
                Dim table As Object(,) = Nothing
                Dim titleRows As Integer = 1

                Select Case which
                    Case "type1"
                        table = h.TypeIAnovaTable
                    Case "type3"
                        table = h.TypeIIIAnovaTable
                    Case Else
                        table = PrepareResultTableForUdf(h.Model.AnovaOverall_toPrint.returnSelf())
                End Select

                If table Is Nothing Then Return ExcelError.ExcelErrorNA

                table = AppendFootnotes(table, h.PredictorCodingFootnotes)
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
                If Not hdr Then
                    table = DropTopRows(table, If(which = "overall" OrElse which = "type1" OrElse which = "type3", titleRows + 1, 1))
                End If

                Return PrepareResultTableForUdf(table)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.LM_ANOVA", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the variance-inflation-factor table for a fitted linear-model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LM_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include the title and header rows (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing one VIF value per modeled predictor column.
        ''' Intercept terms are omitted.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.LM_VIF(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.LM_VIF",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the variance-inflation-factor table for a fitted linear-model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LM_VIF(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LM_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include the title and header rows (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As LinearModelHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _lmCache, h) Then Return ExcelError.ExcelErrorNA

                Dim table As Object(,) = PrepareResultTableForUdf(h.Model.VIF_toPrint.returnSelf())
                If table Is Nothing Then Return ExcelError.ExcelErrorNA

                table = AppendFootnotes(table, h.PredictorCodingFootnotes)
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
                If Not hdr Then
                    table = DropTopRows(table, 2)
                End If

                Return PrepareResultTableForUdf(table)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.LM_VIF", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns residual diagnostics for a fitted linear-model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LM_FIT</c>.</param>
        ''' <param name="residType">
        ''' Optional residual-output selector.
        ''' Accepted values are <c>all</c> (default), <c>fitted</c>, <c>residual</c>, <c>leverage</c>,
        ''' <c>stdresid</c>, <c>cooksd</c>, and <c>jackknife</c>.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing the requested residual block.
        ''' When <paramref name="residType"/> is <c>all</c>, the returned table contains fitted values,
        ''' raw residuals, leverage, standardized residuals, Cook's distance, and jackknife residuals.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.LM_RESID(F2)
        ''' =BESH.REGR.LM_RESID(F2,"stdresid")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.LM_RESID",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns residual diagnostics for a fitted linear-model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LM_RESID(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LM_FIT.")> handle As Object,
            <ExcelArgument(Name:="residType", Description:="Residual selector: ""all"" (default), ""fitted"", ""residual"", ""leverage"", ""stdresid"", ""cooksd"", or ""jackknife"".")> Optional residType As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As LinearModelHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _lmCache, h) Then Return ExcelError.ExcelErrorNA

                Dim fullTable As Object(,) = PrepareResultTableForUdf(h.Model.AllResiduals_toPrint)
                If fullTable Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim selector As String = ParseLmResidualType(residType)
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)

                If selector = "all" Then
                    Return If(hdr, fullTable, DropTopRows(fullTable, 1))
                End If

                Dim colIndex As Integer
                Dim colName As String
                Select Case selector
                    Case "fitted"
                        colIndex = 0 : colName = "Fitted"
                    Case "residual"
                        colIndex = 1 : colName = "Residual"
                    Case "leverage"
                        colIndex = 2 : colName = "Leverage"
                    Case "stdresid"
                        colIndex = 3 : colName = "Std. Residual"
                    Case "cooksd"
                        colIndex = 4 : colName = "Cook's D"
                    Case Else
                        colIndex = 5 : colName = "Jackknife Residual"
                End Select

                Dim startRow As Integer = If(hdr, 0, 1)
                Dim rowCount As Integer = fullTable.GetLength(0) - startRow
                Dim out(rowCount - 1, 0) As Object

                Dim r As Integer = 0
                If hdr Then
                    out(0, 0) = colName
                    r = 1
                End If

                For i As Integer = 1 To fullTable.GetLength(0) - 1
                    out(r, 0) = fullTable(i, colIndex)
                    r += 1
                Next

                Return PrepareResultTableForUdf(out)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.LM_RESID", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns predicted mean responses for new observations from a fitted linear-model handle.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LM_FIT</c>.</param>
        ''' <param name="newX">New raw predictor matrix in the same raw-column order used at fitting time.</param>
        ''' <param name="newOffset">Optional offset vector for the new observations. Required when the fitted model used an offset.</param>
        ''' <param name="includeHeader">TRUE to include a header row (default TRUE).</param>
        ''' <returns>
        ''' A spilled array containing one predicted mean response per new observation.
        ''' When the fitted model used an offset, the returned predictions include that offset on the original response scale.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.LM_PRED(F2,B2:D11)
        ''' =BESH.REGR.LM_PRED(F2,B2:D11,E2:E11)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.LM_PRED",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns predicted mean responses for new observations from a fitted linear-model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LM_PRED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LM_FIT.")> handle As Object,
            <ExcelArgument(AllowReference:=True, Name:="newX", Description:="New raw predictor matrix in the same raw-column order used at fitting time.")> newX As Object,
            <ExcelArgument(Name:="newOffset", Description:="Optional offset vector for the new observations. Required when the fitted model used an offset.")> Optional newOffset As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As LinearModelHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _lmCache, h) Then Return ExcelError.ExcelErrorNA

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

                Dim beta() As Double = h.Model.results.Coeffs_est
                If beta Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim nRows As Integer = imported.nRows
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
                Dim outRows As Integer = If(hdr, nRows + 1, nRows)
                Dim out(outRows - 1, 0) As Object

                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "PredictedY"
                    r0 = 1
                End If

                For i As Integer = 0 To nRows - 1
                    Dim pred As Double = 0.0
                    If h.IncludeIntercept Then
                        pred = beta(0)
                    End If

                    For j As Integer = 0 To h.PredictorCount - 1
                        Dim xv As Double = expandedX(i, j)
                        If Double.IsNaN(xv) OrElse Double.IsInfinity(xv) Then Return ExcelError.ExcelErrorValue
                        pred += xv * beta(j + If(h.IncludeIntercept, 1, 0))
                    Next

                    If offsetVals IsNot Nothing Then
                        pred += offsetVals(i)
                    End If

                    out(r0 + i, 0) = pred
                Next

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.LM_PRED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a fitted linear-model handle from the in-memory cache.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LM_FIT</c>.</param>
        ''' <returns>TRUE if the handle existed and was removed; otherwise FALSE.</returns>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.LM_DROP(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.LM_DROP",
            Category:="BESHStatNG - Regression Models",
            Description:="Removes a fitted linear-model handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LM_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LM_FIT.")> handle As Object
        ) As Object
            Dim key As String = AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return ExcelError.ExcelErrorValue
            Dim removed As LinearModelHandle = Nothing
            Return _lmCache.TryRemove(key, removed)
        End Function

        ''' <summary>
        ''' Parses the requested ANOVA-table selector into a canonical value.
        ''' </summary>
        Private Function ParseLmAnovaScope(v As Object) As String
            Dim s As String = AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return "overall"

            Select Case s.Trim().ToLowerInvariant()
                Case "type1", "typei", "i", "seq", "sequential"
                    Return "type1"
                Case "type3", "typeiii", "iii", "partial"
                    Return "type3"
                Case Else
                    Return "overall"
            End Select
        End Function

        ''' <summary>
        ''' Parses the requested residual selector into a canonical value.
        ''' </summary>
        Private Function ParseLmResidualType(v As Object) As String
            Dim s As String = AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case s.Trim().ToLowerInvariant()
                Case "fitted", "fit", "mean", "pred", "prediction"
                    Return "fitted"
                Case "residual", "resid", "raw"
                    Return "residual"
                Case "leverage", "hat"
                    Return "leverage"
                Case "stdresid", "standardized", "standardizedresidual", "std", "studentized"
                    Return "stdresid"
                Case "cooksd", "cook", "cooks", "cooksdistance"
                    Return "cooksd"
                Case "jackknife", "press", "deleted"
                    Return "jackknife"
                Case Else
                    Return "all"
            End Select
        End Function

        ''' <summary>
        ''' Returns a copy of the supplied table with a specified number of top rows removed.
        ''' </summary>
        Private Function DropTopRows(arr As Object(,), rowsToDrop As Integer) As Object(,)
            If arr Is Nothing Then Return Nothing
            If rowsToDrop <= 0 Then Return arr

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            If rowsToDrop >= rows Then
                Dim emptyOut(0, Math.Max(0, cols - 1)) As Object
                Return emptyOut
            End If

            Dim out(rows - rowsToDrop - 1, cols - 1) As Object
            For i As Integer = rowsToDrop To rows - 1
                For j As Integer = 0 To cols - 1
                    out(i - rowsToDrop, j) = arr(i, j)
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Appends footnote rows to a table and returns the expanded array.
        ''' </summary>
        Private Function AppendFootnotes(arr As Object(,), notes As String()) As Object(,)
            If arr Is Nothing Then Return Nothing
            If notes Is Nothing OrElse notes.Length = 0 Then Return arr

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            Dim out(rows + notes.Length - 1, cols - 1) As Object

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    out(i, j) = arr(i, j)
                Next
            Next

            For i As Integer = 0 To notes.Length - 1
                out(rows + i, 0) = notes(i)
            Next

            Return out
        End Function

        ''' <summary>
        ''' Writes footnote rows into an already-sized output matrix starting at the supplied row index.
        ''' </summary>
        Private Sub AppendFootnotesInPlace(arr As Object(,), startRow As Integer, notes As String())
            If arr Is Nothing OrElse notes Is Nothing Then Exit Sub
            For i As Integer = 0 To notes.Length - 1
                arr(startRow + i, 0) = notes(i)
            Next
        End Sub

    End Module
End Namespace
