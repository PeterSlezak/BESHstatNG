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
    ''' Worksheet functions for fitting and summarizing Mixed Models for Repeated Measures (MMRM).
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These functions expose MMRM analysis directly to worksheet formulas. The fit function reads a
    ''' response range, a raw numeric predictor matrix or already-expanded fixed-effect design matrix,
    ''' subject identifiers, and optional visit values, then returns a short text handle. The remaining
    ''' functions use that handle to return
    ''' model summaries, fixed-effect inference, covariance estimates, fit statistics, LS-means,
    ''' contrasts, fitted values, and residuals.
    ''' </para>
    ''' <para>
    ''' The fit function can be used in either matrix-oriented mode or formula mode. Leave the
    ''' formula argument blank when parameter "x" already contains the desired fixed-effect
    ''' design columns. Provide a right-hand-side formula to expand the raw predictor columns using the
    ''' same regression-formula parser used by the other regression FIT UDFs.
    ''' </para>
    ''' <para>
    ''' Kenward-Roger inference is available for REML fits and is the default inference method for
    ''' this MMRM worksheet surface. If maximum likelihood is requested, choose a non-Kenward-Roger
    ''' inference method.
    ''' </para>
    ''' </remarks>
    Public Module MixedModelUDFs

        Private ReadOnly _mmrmCache As New ConcurrentDictionary(Of String, MmrmHandle)(StringComparer.OrdinalIgnoreCase)

        Friend Class MmrmHandle
            Public Property Handle As String
            Public Property Result As regression.MixedModelResult
            Public Property Alpha As Double
            Public Property InferenceLabel As String
            Public Property CovarianceStructure As String
            Public Property FitMethod As regression.MixedModelFitMethod
            Public Property IncludeIntercept As Boolean
            Public Property SourceRowsUsed As Integer
            Public Property SourceRowsDropped As Integer
            Public Property FixedEffectNames As String()
            Public Property FittedDesign As Double(,)
            Public Property VisitValues As Double()
        End Class

        Friend Class MmrmLsmEstimateProfileValue
            Public Property Name As String
            Public Property ColumnIndex As Integer
            Public Property Value As Double
        End Class

        Friend Class MmrmLsmEstimateComponent
            Public Property Label As String
            Public Property Weight As Double
            Public Property VisitSpecified As Boolean
            Public Property VisitValue As Double
            Public Property ProfileValues As New List(Of MmrmLsmEstimateProfileValue)()
        End Class

        Friend Class MmrmLsmEstimateAtProfile
            Public Property VisitSpecified As Boolean = False
            Public Property VisitValue As Double = Double.NaN
            Public Property ProfileValues As New List(Of MmrmLsmEstimateProfileValue)()
        End Class

        ''' <summary>
        ''' Fits a Mixed Model for Repeated Measures and returns a reusable worksheet-session handle.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Use this function when observations are grouped by subject and the within-subject
        ''' covariance is modeled directly. The response and every predictor column must be numeric.
        ''' Subject identifiers may be text or numeric. A visit/time column is optional but recommended
        ''' whenever repeated measurements have a meaningful order or when rows are not already sorted
        ''' within each subject.
        ''' </para>
        ''' <para>
        ''' Rows with missing or invalid response, predictor, subject, or supplied visit values are
        ''' excluded before fitting. The returned handle stores the fitted analysis for the current
        ''' Excel session and can be passed to the extractor functions listed below.
        ''' </para>
        ''' <para>
        ''' The default fit uses REML, an unstructured within-subject covariance matrix, an intercept
        ''' column, and Kenward-Roger fixed-effect inference. Kenward-Roger inference requires REML.
        ''' To use maximum likelihood, set <paramref name="fitMethod"/> to <c>"ML"</c> and choose a
        ''' non-Kenward-Roger inference method.
        ''' </para>
        ''' <para>
        ''' Covariance optimization can be controlled from the worksheet. By default the fit uses a
        ''' SAS PROC MIXED-style Average Information / Fisher-scoring REML covariance optimizer when
        ''' it is applicable. If that optimizer cannot provide a usable solution, the fit falls back
        ''' to the projected BFGS covariance optimizer. The gradient mode controls the derivative
        ''' source used by projected BFGS: automatic analytic scores for validated covariance
        ''' structures, fully numerical finite differences, analytic scores only, or analytic scores
        ''' with finite-difference validation diagnostics.
        ''' </para>
        ''' <para>
        ''' The optimizer setting is most useful for reproducibility and troubleshooting. Use
        ''' <c>AI</c> or <c>AverageInformation</c> for the default Average Information / Fisher-scoring
        ''' optimizer, <c>BFGS</c> for projected BFGS with the selected gradient mode,
        ''' <c>BFGS_ANALYTIC</c> for projected BFGS with analytic scores, or <c>BFGS_NUMERICAL</c> for
        ''' projected BFGS with finite-difference gradients. Use the numerical option when comparing
        ''' against older workbooks or when diagnosing a suspected analytic derivative issue.
        ''' </para>
        ''' <para>
        ''' Common follow-up functions are <c>BESH.REGR.MMRM_COEF</c>,
        ''' <c>BESH.REGR.MMRM_TYPE3</c>, <c>BESH.REGR.MMRM_COVPARMS</c>,
        ''' <c>BESH.REGR.MMRM_FITSTATS</c>, <c>BESH.REGR.MMRM_LSMEANS</c>,
        ''' <c>BESH.REGR.MMRM_CONTRASTS</c>, <c>BESH.REGR.MMRM_FITTED</c>, and
        ''' <c>BESH.REGR.MMRM_RESID</c>.
        ''' </para>
        ''' </remarks>
        ''' <param name="y">Single-column range containing the continuous response values.</param>
        ''' <param name="x">Numeric matrix containing raw fixed-effect predictors or already-coded design columns. Each row must correspond to the same row in <paramref name="y"/>.</param>
        ''' <param name="subject">Single-column range identifying the subject for each observation. Repeated rows with the same identifier are treated as belonging to the same subject.</param>
        ''' <param name="visit">Optional single-column numeric visit/time range. When supplied, observations are sorted within each subject by this value before the covariance structure is evaluated.</param>
        ''' <param name="varNames">Optional row or column range containing predictor names for the columns of <paramref name="x"/>. If omitted, generic names are used.</param>
        ''' <param name="covariance">Within-subject covariance structure. Accepted values include <c>ID</c>, <c>Diagonal</c>, <c>CS</c>, <c>HCS</c>, <c>AR(1)</c>, <c>HAR(1)</c>, <c>TOEP</c>, <c>TOEPH</c> and <c>UN</c>. The default is <c>UN</c>.</param>
        ''' <param name="fitMethod">Likelihood method. Use <c>REML</c> for restricted maximum likelihood or <c>ML</c> for maximum likelihood. The default is <c>REML</c>.</param>
        ''' <param name="inference">Fixed-effect inference method. Accepted values include <c>KR</c>, <c>Satterthwaite</c>, <c>BetweenWithin</c>, <c>ResidualDF</c>, and <c>Wald</c>. The default is <c>KR</c>.</param>
        ''' <param name="includeIntercept">TRUE to add an intercept column before fitting; FALSE when <paramref name="x"/> already contains all desired columns. The default is TRUE.</param>
        ''' <param name="formula">Optional right-hand-side formula used to expand the raw predictor matrix before fitting. Leave blank to use the columns of <paramref name="x"/> as supplied.</param>
        ''' <param name="formulaAddressing">Formula-addressing mode: <c>relative</c> (default), <c>absolute</c>, or <c>names</c>.</param>
        ''' <param name="alpha">Two-sided alpha level for confidence intervals returned by extractor functions. The default is 0.05.</param>
        ''' <param name="maxIter">Optional maximum number of optimizer iterations. Leave blank to use the standard setting.</param>
        ''' <param name="trace">
        ''' TRUE to store detailed optimizer trace text in the fitted handle. Stored trace text can
        ''' later be included by <c>BESH.REGR.MMRM_RESULTS</c> when its
        ''' <c>includeOptimizerTrace</c> argument is TRUE. The default is FALSE.
        ''' </param>
        ''' <param name="covOptimizerMode">
        ''' Optional covariance-optimizer mode. Blank uses the default SAS PROC MIXED-style
        ''' Average Information / Fisher-scoring REML optimizer with safe fallback. Accepted values
        ''' include <c>AI</c>, <c>AverageInformation</c>, <c>FisherScoring</c>, or <c>SAS</c> for
        ''' Average Information / Fisher scoring; <c>BFGS</c> or <c>ProjectedBFGS</c> for projected
        ''' BFGS using the selected gradient mode; <c>BFGS_ANALYTIC</c> for projected BFGS with
        ''' analytic scores; and <c>BFGS_NUMERICAL</c> for projected BFGS with finite-difference
        ''' gradients. Average Information is REML-oriented; if it is not applicable or does not
        ''' produce a usable covariance solution, the fit records diagnostics and falls back to
        ''' projected BFGS.
        ''' </param>
        ''' <param name="covGradientMode">
        ''' Optional covariance-gradient mode used by projected BFGS and by fallback paths. Blank
        ''' uses <c>Auto</c>. Accepted values include <c>Auto</c>, <c>Analytic</c>,
        ''' <c>AnalyticValidation</c>, <c>Validate</c>, <c>Numerical</c>, and
        ''' <c>FiniteDifference</c>. Auto uses analytic covariance scores for validated residual
        ''' covariance structures and numerical finite differences otherwise. AnalyticValidation
        ''' runs the analytic score path and records a finite-difference comparison in diagnostics;
        ''' it is intended for validation and troubleshooting rather than routine production use.
        ''' </param>''' <returns>A text handle that identifies the fitted model in the current Excel session, or an error message if the fit cannot be created.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_FIT",
            Category:="BESHStatNG - Regression Models",
            Description:="Fits an MMRM and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_FIT(
            <ExcelArgument(Name:="y", Description:="Continuous response vector.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Numeric fixed-effect predictor/design matrix.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="subject", Description:="Subject identifier vector.")> subject As Object,
            <ExcelArgument(AllowReference:=True, Name:="visit", Description:="Optional numeric visit/time vector.")> Optional visit As Object = Nothing,
            <ExcelArgument(Name:="varNames", Description:="Optional predictor names for columns of x.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="covariance", Description:="R-side covariance: ID, Diagonal, CS, HCS, AR(1), HAR(1), TOEP, TOEPH, or UN. Default UN.")> Optional covariance As Object = Nothing,
            <ExcelArgument(Name:="fitMethod", Description:="REML or ML. Default REML.")> Optional fitMethod As Object = Nothing,
            <ExcelArgument(Name:="inference", Description:="KR, Satterthwaite, BetweenWithin, ResidualDF, or Wald. Default KR.")> Optional inference As Object = Nothing,
            <ExcelArgument(Name:="includeIntercept", Description:="TRUE to prepend an intercept column to x. Default TRUE.")> Optional includeIntercept As Object = Nothing,
            <ExcelArgument(Name:="formula", Description:="Optional RHS formula used to expand the raw predictor matrix.")> Optional formula As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Formula-addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha for confidence intervals. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="maxIter", Description:="Optional optimizer maximum iterations. Default engine setting.")> Optional maxIter As Object = Nothing,
            <ExcelArgument(Name:="trace", Description:="TRUE to store optimizer trace text for later MMRM_RESULTS output. Default FALSE.")> Optional trace As Object = Nothing,
            <ExcelArgument(Name:="covOptimizerMode", Description:="Covariance optimizer: AI/AverageInformation/FisherScoring/SAS (default), BFGS, BFGS_ANALYTIC, or BFGS_NUMERICAL.")> Optional covOptimizerMode As Object = Nothing,
            <ExcelArgument(Name:="covGradientMode", Description:="Covariance gradient for BFGS/fallback: Auto (default), Analytic, AnalyticValidation/Validate, Numerical/FiniteDifference.")> Optional covGradientMode As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "MMRM_FIT (editing...)"

            Try
                Dim alphaValue As Double = 0.05
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim includeInterceptValue As Boolean = ExcelArgNumeric.GetOptionalBool(includeIntercept, True)
                Dim traceValue As Boolean = ExcelArgNumeric.GetOptionalBool(trace, False)

                Dim fit As regression.MixedModelFitMethod = ParseMixedModelFitMethodStrict(fitMethod, "MMRM")
                Dim fixedInference As regression.MixedModelFixedInferenceMethod = ParseMixedModelInferenceMethodStrict(inference, "MMRM")
                Dim optimizerMode As regression.MixedModelCovarianceOptimizerMode = ParseMixedModelCovarianceOptimizerModeStrict(covOptimizerMode, "MMRM")
                Dim gradientMode As regression.MixedModelCovarianceGradientMode = ParseMixedModelCovarianceGradientModeStrict(covGradientMode, "MMRM")
                ApplyMixedModelOptimizerShortcutToGradient(covOptimizerMode, gradientMode)

                If fixedInference = regression.MixedModelFixedInferenceMethod.KenwardRoger AndAlso fit <> regression.MixedModelFitMethod.REML Then
                    Return "BESH.REGR.MMRM_FIT error: Kenward-Roger inference requires REML. Set fitMethod to ""REML"" or choose another inference method."
                End If

                Dim formulaText As String = ExcelArgReaders.AsString(formula)
                If String.IsNullOrWhiteSpace(formulaText) Then formulaText = Nothing

                Dim addressingMode As String = Global.BESHStatNG.UdfDataImport.GetFormulaAddressingMode(formulaAddressing, "relative")
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

                Dim yValues() As Double = Nothing
                Dim xValues(,) As Double = Nothing
                Dim subjectValues() As Object = Nothing
                Dim visitValues() As Double = Nothing
                Dim inferredXNames() As String = Nothing
                Dim inferredXAbsoluteLetters() As String = Nothing
                Dim dropped As Integer = 0
                Dim errorMessage As String = Nothing

                If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmAlignedInputs(y:=y,
                                                                      x:=x,
                                                                      subject:=subject,
                                                                      visit:=visit,
                                                                      yValues:=yValues,
                                                                      xValues:=xValues,
                                                                      subjectValues:=subjectValues,
                                                                      visitValues:=visitValues,
                                                                      inferredXNames:=inferredXNames,
                                                                      inferredXAbsoluteLetters:=inferredXAbsoluteLetters,
                                                                      droppedRows:=dropped,
                                                                      errorMessage:=errorMessage,
                                                                      formulaText:=formulaText,
                                                                      formulaAddressingMode:=addressingMode,
                                                                      varNames:=varNames) Then
                    Return "BESH.REGR.MMRM_FIT error: " & errorMessage
                End If

                Dim rawNames() As String = ResolveMmrmImportedPredictorNames(varNames, inferredXNames, inferredXAbsoluteLetters)
                If rawNames Is Nothing OrElse rawNames.Length <> xValues.GetLength(1) Then rawNames = DefaultNames(xValues.GetLength(1), "X")

                Dim absoluteColumnLetters As String() = Nothing
                If allowAbsoluteColumnLetters AndAlso Not String.IsNullOrWhiteSpace(formulaText) Then
                    absoluteColumnLetters = inferredXAbsoluteLetters
                    If absoluteColumnLetters Is Nothing OrElse absoluteColumnLetters.Length <> xValues.GetLength(1) Then
                        Return "BESH.REGR.MMRM_FIT error: formulaAddressing='absolute' requires x to be passed as a direct worksheet range so absolute worksheet column letters can be determined for the formula terms."
                    End If
                End If

                Dim designBuild As RegressionFormulaMatrixBuildResult = Nothing
                Dim designErr As String = Nothing

                If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromFormula(rawX:=xValues,
                                                                                                result:=designBuild,
                                                                                                errorMessage:=designErr,
                                                                                                predictorNames:=rawNames,
                                                                                                formulaText:=formulaText,
                                                                                                absoluteColumnLetters:=absoluteColumnLetters,
                                                                                                allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                                                                allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                                                                allowQuotedVariableNames:=allowQuotedVariableNames,
                                                                                                omitCategoricalReference:=True) Then
                    Return "BESH.REGR.MMRM_FIT error: formula could not be parsed or expanded: " & If(designErr, String.Empty)
                End If

                If designBuild Is Nothing OrElse designBuild.ExpandedPredictorMatrix Is Nothing Then
                    Return "BESH.REGR.MMRM_FIT error: formula expansion did not produce a fixed-effect design matrix."
                End If

                Dim designX(,) As Double = designBuild.ExpandedPredictorMatrix
                Dim designNames() As String = If(designBuild.ExpandedPredictorNames, New String() {})
                If designNames.Length <> designX.GetLength(1) Then designNames = DefaultNames(designX.GetLength(1), "X")

                Dim fitX(,) As Double = AddInterceptIfRequested(designX, includeInterceptValue)
                Dim fitNames() As String = AddInterceptNameIfRequested(designNames, includeInterceptValue)

                Dim data As regression.MixedModelBlockData = regression.MixedModelBlockData.FromArrays(y:=yValues,
                                                                                                        x:=fitX,
                                                                                                        subjectId:=subjectValues,
                                                                                                        z:=Nothing,
                                                                                                        visit:=visitValues,
                                                                                                        sortWithinSubjectByVisit:=True)

                Dim covName As String = NormalizeMixedModelResidualCovarianceName(covariance, "UN")
                Dim rStruct As regression.MixedModelRStruct = regression.MixedModelRStructUtils.createMixedModelRStruct(covName)
                Dim req As regression.MixedModelFitRequest = regression.MixedModelFitRequest.CreateMMRM(data, rStruct, fit)
                req.FixedEffectNames = fitNames
                req.FixedInferenceMethod = fixedInference
                req.ResponseVarName = "y"
                req.SubjectVarName = "subject"
                req.VisitVarName = If(visitValues Is Nothing, String.Empty, "visit")
                req.RequestLabel = "MMRM UDF"

                Dim control As regression.MixedModelControl = req.Control
                control.Trace = traceValue
                control.CovarianceOptimizerMode = optimizerMode
                control.CovarianceGradientMode = gradientMode

                If Not ExcelArgPredicates.IsMissingArg(maxIter) Then
                    Dim maxIterValue As Integer = ExcelArgNumeric.GetOptionalInt(maxIter, control.MaxIter)
                    If maxIterValue <= 0 Then Return ExcelError.ExcelErrorNum
                    control.MaxIter = maxIterValue
                End If
                req.Control = control

                Select Case fixedInference
                    Case regression.MixedModelFixedInferenceMethod.KenwardRoger
                        req.EnableFullKenwardRogerForMmrm()
                    Case regression.MixedModelFixedInferenceMethod.Satterthwaite
                        req.UseSatterthwaite = True
                        req.UseKenwardRoger = False
                    Case Else
                        req.UseSatterthwaite = False
                        req.UseKenwardRoger = False
                End Select

                req.Validate()

                Dim model As New regression.MMRM(req)
                Dim result As regression.MixedModelResult = model.Fit()
                If result IsNot Nothing AndAlso fitNames IsNot Nothing AndAlso result.Beta IsNot Nothing AndAlso result.Beta.Length = fitNames.Length Then
                    result.FixedEffectNames = CType(fitNames.Clone(), String())
                End If

                Dim handleKey As String = "MMRM:" & Guid.NewGuid().ToString("N")
                Dim h As New MmrmHandle With {
                    .Handle = handleKey,
                    .Result = result,
                    .Alpha = alphaValue,
                    .InferenceLabel = fixedInference.ToString(),
                    .CovarianceStructure = covName,
                    .FitMethod = fit,
                    .IncludeIntercept = includeInterceptValue,
                    .SourceRowsUsed = yValues.Length,
                    .SourceRowsDropped = dropped,
                    .FixedEffectNames = fitNames,
                    .FittedDesign = fitX,
                    .VisitValues = visitValues
                }

                _mmrmCache(handleKey) = h
                Return handleKey

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.MMRM_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns all result tables, or one selected result table, from a fitted MMRM handle.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Leave <paramref name="table"/> blank to return the complete set of available tables stacked
        ''' vertically. Provide a table title to return only that table. Common table titles include
        ''' <c>Fixed effects</c>, <c>Kenward-Roger term-level F tests</c>,
        ''' <c>Covariance parameters</c>, <c>Estimated R covariance matrix</c>,
        ''' <c>Estimated R correlation matrix</c>, <c>Fit statistics</c>, and <c>Convergence</c>.
        ''' </para>
        ''' <para>
        ''' If trace output was requested when the model was fitted, set
        ''' <paramref name="includeOptimizerTrace"/> to TRUE to include the iteration history in the
        ''' returned result set.
        ''' </para>
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <param name="table">Optional result-table title to return. Leave blank to return all available tables.</param>
        ''' <param name="includeOptimizerTrace">TRUE to include stored optimizer trace information when returning all tables. The default is FALSE.</param>
        ''' <param name="alpha">Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.</param>
        ''' <returns>A dynamic array containing the requested result table or tables.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_RESULTS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns all MMRM result tables or one named table for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_RESULTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object,
            <ExcelArgument(Name:="table", Description:="Optional table name, e.g. Fixed effects, Kenward-Roger term-level F tests, Covariance parameters, Estimated R covariance matrix, Estimated R correlation matrix, Fit statistics, Convergence.")> Optional table As Object = Nothing,
            <ExcelArgument(Name:="includeOptimizerTrace", Description:="TRUE to include optimizer trace if stored. Default FALSE.")> Optional includeOptimizerTrace As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha for confidence intervals. Default is the fit alpha.")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As MmrmHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _mmrmCache, h) Then Return ExcelError.ExcelErrorNA
                If h.Result Is Nothing Then Return ExcelError.ExcelErrorNA
                EnsureMmrmResultUsesHandleFixedEffectNames(h)

                Dim alphaValue As Double = h.Alpha
                If Not ExcelArgPredicates.IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim includeTrace As Boolean = ExcelArgNumeric.GetOptionalBool(includeOptimizerTrace, False)
                Dim tables As List(Of ResultTable) = h.Result.wrapResults(alpha:=alphaValue, includeOptimizerTrace:=includeTrace)
                Dim tableName As String = ExcelArgReaders.AsString(table)

                If String.IsNullOrWhiteSpace(tableName) Then
                    Return PrepareResultTableForUdf(StackResultTables(tables))
                End If

                Dim selected As Object(,) = FindResultTableByTitle(tables, tableName)
                If selected Is Nothing Then Return ExcelError.ExcelErrorNA
                Return PrepareResultTableForUdf(selected)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.MMRM_RESULTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the fixed-effect coefficient table for a fitted MMRM handle.
        ''' </summary>
        ''' <remarks>
        ''' The table contains the fixed-effect estimates and the inferential columns associated with
        ''' the inference method chosen during fitting. For Kenward-Roger fits, the standard errors,
        ''' degrees of freedom, test statistics, p-values, and confidence intervals are reported using
        ''' the Kenward-Roger adjustment.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <param name="alpha">Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.</param>
        ''' <returns>A dynamic array containing the fixed-effect coefficient table.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_COEF",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the fixed-effect coefficient table for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_COEF(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha for confidence intervals. Default is the fit alpha.")> Optional alpha As Object = Nothing
        ) As Object
            Return MMRM_RESULTS(handle, "Fixed effects", False, alpha)
        End Function

        ''' <summary>
        ''' Returns the term-level fixed-effect F-test table for a fitted MMRM handle.
        ''' </summary>
        ''' <remarks>
        ''' For Kenward-Roger fits, the table reports term-level F tests using Kenward-Roger
        ''' denominator degrees of freedom and F scaling. This is the worksheet equivalent of the
        ''' model-wide fixed-effect tests shown in the graphical MMRM output.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <param name="alpha">Reserved for consistency with other extractors. Leave blank unless a future version documents alpha-dependent columns for this table.</param>
        ''' <returns>A dynamic array containing the term-level fixed-effect F-test table.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_TYPE3",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the Kenward-Roger term-level F-test table for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_TYPE3(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha. Default is the fit alpha.")> Optional alpha As Object = Nothing
        ) As Object
            Try
                Return MMRM_RESULTS(handle, "Kenward-Roger term-level F tests", False, alpha)
            Catch
                Return "Kenward-Roger term-level F tests table not available"
            End Try

        End Function

        ''' <summary>
        ''' Returns estimated within-subject covariance parameters for a fitted MMRM handle.
        ''' </summary>
        ''' <remarks>
        ''' The returned rows describe the fitted covariance parameters for the selected
        ''' within-subject covariance structure. The exact parameter labels depend on the covariance
        ''' structure used in <c>BESH.REGR.MMRM_FIT</c>.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <returns>A dynamic array containing covariance-parameter estimates and related columns.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_COVPARMS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns covariance-parameter estimates for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_COVPARMS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object
        ) As Object
            Return MMRM_RESULTS(handle, "Covariance parameters", False, Nothing)
        End Function

        ''' <summary>
        ''' Returns the fitted within-subject covariance matrix for a fitted MMRM handle.
        ''' </summary>
        ''' <remarks>
        ''' The matrix is reported on the response scale for the visit levels represented in the
        ''' fitted analysis. For unstructured covariance, this table is often the easiest way to review
        ''' the estimated variances and covariances by visit.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <returns>A dynamic array containing the estimated within-subject covariance matrix.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_R_COV",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the fitted R-side covariance matrix for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_R_COV(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object
        ) As Object
            Return MMRM_RESULTS(handle, "Estimated R covariance matrix", False, Nothing)
        End Function

        ''' <summary>
        ''' Returns the fitted within-subject correlation matrix for a fitted MMRM handle.
        ''' </summary>
        ''' <remarks>
        ''' This table converts the fitted within-subject covariance matrix to correlations, making it
        ''' easier to compare the strength of association between visits or time points.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <returns>A dynamic array containing the estimated within-subject correlation matrix.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_R_CORR",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the fitted R-side correlation matrix for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_R_CORR(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object
        ) As Object
            Return MMRM_RESULTS(handle, "Estimated R correlation matrix", False, Nothing)
        End Function

        ''' <summary>
        ''' Returns likelihood, information-criterion, convergence, and model-size statistics.
        ''' </summary>
        ''' <remarks>
        ''' Use this function to review model fit, compare alternative covariance structures fitted
        ''' with the same method, and check high-level convergence information.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <returns>A dynamic array containing model fit statistics.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_FITSTATS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns fit statistics for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_FITSTATS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object
        ) As Object
            Return MMRM_RESULTS(handle, "Fit statistics", False, Nothing)
        End Function


        ''' <summary>
        ''' Returns observed-design-grid estimated marginal means for a fitted MMRM handle.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This extractor computes LS-means from the fitted fixed-effect design rows retained during
        ''' the model fit. When <paramref name="group"/> is blank, the table contains one estimated
        ''' marginal mean for each visit/time value. When <paramref name="group"/> names a numeric
        ''' design column, the table contains means for each visit-by-group profile. When
        ''' <paramref name="group"/> is a worksheet range, it is interpreted as an AT/profile setting
        ''' range and the visit means are restricted to observed design rows matching that profile.
        ''' </para>
        ''' <para>
        ''' The estimates use the same inference method saved with the fit. For Kenward-Roger fits,
        ''' standard errors, denominator degrees of freedom, test statistics, p-values, and confidence
        ''' limits use the Kenward-Roger adjustment.
        ''' </para>
        ''' <para>
        ''' This function uses the observed design grid. Covariate/reference-grid helper functions can
        ''' be added separately if a workbook needs SAS/R-style equal-cell marginalization over class
        ''' factors and user-specified covariate values.
        ''' </para>
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <param name="group">Optional fitted design column to use as a grouping factor, for example <c>treatment_active</c>. Alternatively, provide an AT/profile range in name/value or wide form to compute visit means at a specified observed profile. Leave blank for visit-only means.</param>
        ''' <param name="alpha">Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.</param>
        ''' <returns>A dynamic array containing estimated marginal means.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_LSMEANS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns observed-design-grid LS-means for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_LSMEANS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object,
            <ExcelArgument(Name:="group", Description:="Optional fitted design column name, or an AT/profile range. Leave blank for visit-only means.")> Optional group As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha for confidence intervals. Default is the fit alpha.")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As MmrmHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _mmrmCache, h) Then Return ExcelError.ExcelErrorNA
                If h.Result Is Nothing Then Return ExcelError.ExcelErrorNA
                EnsureMmrmResultUsesHandleFixedEffectNames(h)

                Dim alphaValue As Double = h.Alpha
                If Not ExcelArgPredicates.IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                If h.FittedDesign Is Nothing OrElse h.VisitValues Is Nothing OrElse h.VisitValues.Length = 0 Then
                    Return "BESH.REGR.MMRM_LSMEANS error: the fit does not contain visit/time values required for LS-means."
                End If

                Dim table As ResultTable = Nothing

                If ExcelArgPredicates.IsMissingArg(group) Then
                    table = regression.MMRMPostEstimation.BuildEstimatedMeansByVisitTable(
                        result:=h.Result,
                        x:=h.FittedDesign,
                        visit:=h.VisitValues,
                        alpha:=alphaValue)
                ElseIf IsMmrmWorksheetRangeArgument(group) Then
                    Dim atProfile As MmrmLsmEstimateAtProfile = Nothing
                    Dim errorMessage As String = Nothing

                    If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmLsmEstimateAtSpec(group, h, atProfile, errorMessage) Then
                        Return "BESH.REGR.MMRM_LSMEANS error: " & errorMessage
                    End If

                    Dim rowMask() As Boolean = Nothing
                    Dim profileDescription As String = Nothing
                    If Not TryBuildMmrmAtProfileRowMask(h, atProfile, rowMask, profileDescription, errorMessage) Then
                        Return "BESH.REGR.MMRM_LSMEANS error: " & errorMessage
                    End If

                    table = regression.MMRMPostEstimation.BuildEstimatedMeansByVisitTable(
                        result:=h.Result,
                        x:=h.FittedDesign,
                        visit:=h.VisitValues,
                        alpha:=alphaValue,
                        rowMask:=rowMask,
                        profileDescription:=profileDescription)
                Else
                    Dim groupName As String = ExcelArgReaders.AsString(group)
                    Dim groupValues() As Double = Nothing
                    Dim resolvedGroupName As String = Nothing
                    Dim errorMessage As String = Nothing

                    If String.IsNullOrWhiteSpace(groupName) Then
                        table = regression.MMRMPostEstimation.BuildEstimatedMeansByVisitTable(
                            result:=h.Result,
                            x:=h.FittedDesign,
                            visit:=h.VisitValues,
                            alpha:=alphaValue)
                    ElseIf Not TryGetDesignColumnValues(h, groupName, resolvedGroupName, groupValues, errorMessage) Then
                        Return "BESH.REGR.MMRM_LSMEANS error: " & errorMessage
                    Else
                        table = regression.MMRMPostEstimation.BuildEstimatedMeansByVisitAndGroupTable(
                            result:=h.Result,
                            x:=h.FittedDesign,
                            visit:=h.VisitValues,
                            groupValues:=groupValues,
                            groupName:=resolvedGroupName,
                            alpha:=alphaValue)
                    End If
                End If

                If table Is Nothing Then Return ExcelError.ExcelErrorNA
                Return PrepareResultTableForUdf(table.returnSelf())

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.MMRM_LSMEANS", ex)
            End Try
        End Function


        ''' <summary>
        ''' Returns observed-design-grid contrasts between group levels for a fitted MMRM handle.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This extractor compares levels of one numeric fitted design column within each visit/time
        ''' value. It is intended for common treatment-difference workflows where the design matrix
        ''' contains a treatment indicator or other coded grouping column.
        ''' </para>
        ''' <para>
        ''' The default contrast mode returns all pairwise group differences within each visit. To
        ''' compare each group against a selected control level, set <paramref name="contrastMode"/> to
        ''' <c>"Each group vs control"</c> and provide <paramref name="controlLevel"/>. To request one
        ''' selected comparison, set <paramref name="contrastMode"/> to <c>"Selected comparison only"</c>
        ''' and provide both <paramref name="controlLevel"/> and <paramref name="comparisonLevel"/>.
        ''' </para>
        ''' <para>
        ''' For Kenward-Roger fits, the returned standard errors, denominator degrees of freedom, test
        ''' statistics, p-values, and confidence intervals use the Kenward-Roger adjustment.
        ''' </para>
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <param name="group">Fitted design column used as the grouping factor, for example <c>treatment_active</c>.</param>
        ''' <param name="contrastMode">Contrast mode: <c>Pairwise among group levels</c>, <c>Each group vs control</c>, or <c>Selected comparison only</c>. Default is pairwise.</param>
        ''' <param name="controlLevel">Optional numeric control/reference level. When omitted, the lowest observed group level is used.</param>
        ''' <param name="comparisonLevel">Optional numeric comparison level for selected-comparison mode.</param>
        ''' <param name="direction">Contrast direction: <c>Higher level - lower level</c>, <c>Treatment - control</c>, or <c>Control - treatment</c>. Default is treatment minus control for control-based contrasts and higher minus lower for pairwise contrasts.</param>
        ''' <param name="alpha">Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.</param>
        ''' <returns>A dynamic array containing group contrasts by visit.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_CONTRASTS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns observed-design-grid group contrasts for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_CONTRASTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object,
            <ExcelArgument(Name:="group", Description:="Fitted design column used as a grouping factor.")> group As Object,
            <ExcelArgument(Name:="contrastMode", Description:="Pairwise among group levels, Each group vs control, or Selected comparison only. Default pairwise.")> Optional contrastMode As Object = Nothing,
            <ExcelArgument(Name:="controlLevel", Description:="Optional numeric control/reference level. Default lowest observed group level.")> Optional controlLevel As Object = Nothing,
            <ExcelArgument(Name:="comparisonLevel", Description:="Optional numeric comparison level for selected-comparison mode.")> Optional comparisonLevel As Object = Nothing,
            <ExcelArgument(Name:="direction", Description:="Higher level - lower level, Treatment - control, or Control - treatment.")> Optional direction As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha for confidence intervals. Default is the fit alpha.")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As MmrmHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _mmrmCache, h) Then Return ExcelError.ExcelErrorNA
                If h.Result Is Nothing Then Return ExcelError.ExcelErrorNA
                EnsureMmrmResultUsesHandleFixedEffectNames(h)

                Dim alphaValue As Double = h.Alpha
                If Not ExcelArgPredicates.IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                If h.FittedDesign Is Nothing OrElse h.VisitValues Is Nothing OrElse h.VisitValues.Length = 0 Then
                    Return "BESH.REGR.MMRM_CONTRASTS error: the fit does not contain visit/time values required for contrasts."
                End If

                Dim groupName As String = ExcelArgReaders.AsString(group)
                If String.IsNullOrWhiteSpace(groupName) Then
                    Return "BESH.REGR.MMRM_CONTRASTS error: group must name a fitted design column."
                End If

                Dim groupValues() As Double = Nothing
                Dim resolvedGroupName As String = Nothing
                Dim errorMessage As String = Nothing

                If Not TryGetDesignColumnValues(h, groupName, resolvedGroupName, groupValues, errorMessage) Then
                    Return "BESH.REGR.MMRM_CONTRASTS error: " & errorMessage
                End If

                Dim mode As String = ExcelArgReaders.AsString(contrastMode)
                If String.IsNullOrWhiteSpace(mode) Then mode = regression.MMRMPostEstimation.MODE_PAIRWISE

                Dim dir As String = ExcelArgReaders.AsString(direction)
                If String.IsNullOrWhiteSpace(dir) Then
                    If String.Equals(mode, regression.MMRMPostEstimation.MODE_PAIRWISE, StringComparison.OrdinalIgnoreCase) Then
                        dir = regression.MMRMPostEstimation.DIR_HIGHER_MINUS_LOWER
                    Else
                        dir = regression.MMRMPostEstimation.DIR_TREATMENT_MINUS_CONTROL
                    End If
                End If

                Dim controlValue As Double = Double.NaN
                If Not TryParseOptionalFiniteDouble(controlLevel, controlValue) Then Return ExcelError.ExcelErrorNum

                Dim comparisonValue As Double = Double.NaN
                If Not TryParseOptionalFiniteDouble(comparisonLevel, comparisonValue) Then Return ExcelError.ExcelErrorNum

                Dim table As ResultTable =
                    regression.MMRMPostEstimation.BuildVisitGroupDifferencesTableControlled(
                        result:=h.Result,
                        x:=h.FittedDesign,
                        visit:=h.VisitValues,
                        groupValues:=groupValues,
                        groupName:=resolvedGroupName,
                        alpha:=alphaValue,
                        contrastMode:=mode,
                        controlLevel:=controlValue,
                        comparisonLevel:=comparisonValue,
                        direction:=dir)

                If table Is Nothing Then Return ExcelError.ExcelErrorNA
                Return PrepareResultTableForUdf(table.returnSelf())

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.MMRM_CONTRASTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns custom MMRM LS-mean estimates or contrasts from a fitted MMRM handle, using
        ''' a worksheet specification that is intentionally similar to the coefficient rows supplied
        ''' to the SAS <c>PROC MIXED</c> <c>LSMESTIMATE</c> statement.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' <c>BESH.REGR.MMRM_LSMESTIMATE</c> evaluates one or more user-defined linear functions
        ''' of the fitted fixed-effect coefficients from a previously fitted MMRM model. The function
        ''' is intended for custom estimands that cannot be expressed conveniently by the simpler
        ''' pairwise/control LS-mean contrast extractors, for example contrasts involving several
        ''' factors at once, weighted averages over selected profiles, custom change-from-baseline
        ''' combinations, or an average of several treatment/visit profiles.
        ''' </para>
        ''' <para>
        ''' The first argument must be a handle returned by <c>BESH.REGR.MMRM_FIT</c>. The second
        ''' argument, <paramref name="spec"/>, is a worksheet range with a header row and one or more
        ''' data rows. Each nonblank data row describes one LS-mean profile contribution. Rows with
        ''' the same label are accumulated into one final estimate as
        ''' <c>sum(weight * L(profile)) * beta</c>, where <c>L(profile)</c> is the average fixed-effect
        ''' design row among observations matching the requested profile. The resulting dynamic array
        ''' contains the estimate, standard error, confidence interval, test statistic, degrees of
        ''' freedom when available, and p-value using the variance-covariance information stored in
        ''' the fitted MMRM result.
        ''' </para>
        ''' <para>
        ''' The <paramref name="spec"/> range must contain a column named <c>weight</c>. Accepted
        ''' aliases are <c>coef</c>, <c>coefficient</c>, and <c>contrastweight</c>. The weights are
        ''' analogous to the numbers written in a SAS <c>LSMESTIMATE</c> coefficient row. Use values
        ''' such as <c>1</c>, <c>0</c>, and <c>-1</c> for simple differences, or fractional values such
        ''' as <c>0.5</c> for averages. Rows with a zero weight are ignored after validation.
        ''' </para>
        ''' <para>
        ''' The optional <c>label</c> column groups rows into estimates. Accepted aliases are
        ''' <c>contrast</c>, <c>estimate</c>, and <c>name</c>. If no label is supplied for a row,
        ''' labels are generated as <c>Estimate 1</c>, <c>Estimate 2</c>, and so on. To build a
        ''' contrast that uses multiple profile rows, give those rows exactly the same label.
        ''' </para>
        ''' <para>
        ''' The optional <c>visit</c> column restricts a profile contribution to a visit/time value
        ''' saved in the MMRM handle. Accepted alias: <c>time</c>. Any additional nonblank column in
        ''' <paramref name="spec"/> must match a fitted fixed-effect design column name, using either
        ''' the exact name or a punctuation-insensitive/case-insensitive version of the name. For
        ''' example, if the model handle stores design columns named <c>treatment_active</c>,
        ''' <c>sex_code</c>, and <c>treatment_active:sex_code</c>, those headers may be used as profile
        ''' columns. The numeric value in each cell is matched against the corresponding saved design
        ''' row value. This makes the function work with model-matrix columns already created by the
        ''' BESHStatNG formula/design machinery.
        ''' </para>
        ''' <para>
        ''' The optional <paramref name="at"/> argument implements an Excel range analogue of the SAS
        ''' <c>AT</c> option. It supplies profile values that are applied to every row in
        ''' <paramref name="spec"/> unless that row explicitly overrides the same visit/design column.
        ''' Use it for settings that are common to the whole custom estimand, such as holding a
        ''' covariate at its mean or evaluating all rows at a particular factor level. The
        ''' <paramref name="at"/> range can be supplied in either of two forms:
        ''' </para>
        ''' <list type="bullet">
        ''' <item>
        ''' <description><b>Name/value form</b>: two columns with headers such as <c>name</c> and
        ''' <c>value</c>. The first column contains <c>visit</c> or a fitted design column name; the
        ''' second column contains the numeric value to use.</description>
        ''' </item>
        ''' <item>
        ''' <description><b>Wide form</b>: a header row containing <c>visit</c> and/or fitted design
        ''' column names, followed by one nonblank data row containing the numeric values.</description>
        ''' </item>
        ''' </list>
        ''' <para>
        ''' Values supplied in <paramref name="spec"/> take precedence over values supplied in
        ''' <paramref name="at"/>. For example, if <paramref name="at"/> sets <c>visit=4</c> but one
        ''' row of <paramref name="spec"/> supplies <c>visit=2</c>, that row is evaluated at visit 2.
        ''' If <paramref name="at"/> sets <c>age=65</c> and the <paramref name="spec"/> range does not
        ''' contain an <c>age</c> column, every profile contribution is evaluated among observed design
        ''' rows with <c>age=65</c>.
        ''' </para>
        ''' <para>
        ''' This worksheet UDF uses the observed fitted design rows stored in the handle. It therefore
        ''' evaluates profiles by averaging rows that actually occur in the retained analysis data and
        ''' match the requested profile/AT conditions. It does not synthesize new design rows that were
        ''' absent from the observed design. If no row matches a requested profile, the function returns
        ''' a descriptive error identifying the affected label. When interaction terms are present,
        ''' provide any required interaction design columns explicitly in <paramref name="spec"/> or
        ''' <paramref name="at"/> if those columns are needed to identify the intended profile.
        ''' </para>
        ''' <para>
        ''' Blank cells in profile columns are treated as unspecified. Numeric cells must be finite;
        ''' missing, text, Boolean, error, nonnumeric, infinite, or NaN values in required numeric
        ''' positions produce an error. The intercept column should normally be left unspecified.
        ''' </para>
        ''' <para>
        ''' Example <paramref name="spec"/> for a treatment difference at visit 2 among male subjects,
        ''' with a design column named <c>sex_code</c> coded as 1 for male:
        ''' </para>
        ''' <code>
        ''' label              weight   visit   treatment_active   sex_code
        ''' Active-Control V2   1        2       1                  1
        ''' Active-Control V2  -1        2       0                  1
        ''' </code>
        ''' <para>
        ''' The same example using <paramref name="at"/> to avoid repeating the common visit and sex
        ''' settings:
        ''' </para>
        ''' <code>
        ''' spec:
        ''' label              weight   treatment_active
        ''' Active-Control V2   1        1
        ''' Active-Control V2  -1        0
        '''
        ''' at:
        ''' name       value
        ''' visit      2
        ''' sex_code   1
        ''' </code>
        ''' </remarks>
        ''' <param name="handle">A worksheet-session handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <param name="spec">
        ''' A worksheet range containing a header row and one or more profile/contrast rows. Required
        ''' column: <c>weight</c>. Optional columns: <c>label</c> and <c>visit</c>. Any other nonblank
        ''' header must name a fitted fixed-effect design column stored in the MMRM handle.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for confidence intervals. If omitted, the alpha
        ''' stored when the MMRM handle was created is used. The value must be finite and between 0 and 1
        ''' according to the shared alpha parser used by the MMRM extractor UDFs.
        ''' </param>
        ''' <param name="at">
        ''' Optional SAS <c>AT</c>-style worksheet range of common profile settings. The range may be a
        ''' two-column name/value table or a wide single-row table. Names may be <c>visit</c> or fitted
        ''' fixed-effect design column names. Values in <paramref name="spec"/> override values in
        ''' <paramref name="at"/> for the same visit/design column.
        ''' </param>
        ''' <returns>
        ''' A dynamic array containing one row per custom estimate/contrast. The columns are produced
        ''' by the common mixed-model linear contrast formatter and include the label, estimate, standard
        ''' error, confidence limits, test statistic, denominator degrees of freedom when available, and
        ''' p-value. On validation failure, the function returns a descriptive text error; if the handle
        ''' is not found, it returns <c>#N/A</c>.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_LSMESTIMATE",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns custom SAS-style LS-mean estimates/contrasts for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_LSMESTIMATE(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object,
            <ExcelArgument(Name:="spec", Description:="Range with headers: label(optional), weight(required), visit(optional), and fitted design profile columns.")> spec As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha for confidence intervals. Default is the fit alpha.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="at", Description:="Optional SAS AT-style range of common profile settings. Use name/value columns or one wide row of visit/design-column values.")> Optional at As Object = Nothing
        ) As Object

            Try
                Dim h As MmrmHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _mmrmCache, h) Then Return ExcelError.ExcelErrorNA
                If h.Result Is Nothing Then Return ExcelError.ExcelErrorNA
                EnsureMmrmResultUsesHandleFixedEffectNames(h)

                Dim alphaValue As Double = h.Alpha
                If Not ExcelArgPredicates.IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                If h.FittedDesign Is Nothing OrElse h.FixedEffectNames Is Nothing OrElse
                   h.FixedEffectNames.Length <> h.FittedDesign.GetLength(1) Then
                    Return "BESH.REGR.MMRM_LSMESTIMATE error: the fit does not contain usable fixed-effect design rows and names."
                End If

                Dim components As List(Of MmrmLsmEstimateComponent) = Nothing
                Dim errorMessage As String = Nothing

                If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmLsmEstimateSpec(spec, h, components, errorMessage) Then
                    Return "BESH.REGR.MMRM_LSMESTIMATE error: " & errorMessage
                End If

                Dim atProfile As MmrmLsmEstimateAtProfile = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmLsmEstimateAtSpec(at, h, atProfile, errorMessage) Then
                    Return "BESH.REGR.MMRM_LSMESTIMATE error: " & errorMessage
                End If

                If atProfile IsNot Nothing Then ApplyMmrmLsmEstimateAtProfile(components, atProfile)

                Dim labels As New List(Of String)()
                Dim lRows As New List(Of Double())()
                Dim lByLabel As New Dictionary(Of String, Double())(StringComparer.OrdinalIgnoreCase)
                Dim p As Integer = h.FittedDesign.GetLength(1)

                For Each component As MmrmLsmEstimateComponent In components
                    If Math.Abs(component.Weight) <= 0.0R Then Continue For

                    Dim matchedCount As Integer = 0
                    Dim lProfile() As Double = AverageFittedDesignRowForLsmProfile(h, component, matchedCount)
                    If lProfile Is Nothing Then
                        Return "BESH.REGR.MMRM_LSMESTIMATE error: no observed design rows matched profile for label '" &
                               component.Label & "'."
                    End If

                    If Not lByLabel.ContainsKey(component.Label) Then
                        Dim lZero(p - 1) As Double
                        lByLabel(component.Label) = lZero
                        labels.Add(component.Label)
                    End If

                    Dim lTarget() As Double = lByLabel(component.Label)
                    For j As Integer = 0 To p - 1
                        lTarget(j) += component.Weight * lProfile(j)
                    Next
                Next

                For Each label As String In labels
                    lRows.Add(lByLabel(label))
                Next

                If lRows.Count = 0 Then
                    Return "BESH.REGR.MMRM_LSMESTIMATE error: no non-zero custom contrast rows were produced."
                End If

                Dim table As ResultTable = regression.MixedModelPostEstimation.BuildLinearContrastResultTable(
                    title:="MMRM custom LS-mean estimates",
                    rowLabels:=labels.ToArray(),
                    lRows:=lRows,
                    result:=h.Result,
                    alpha:=alphaValue,
                    footnote:="Custom LS-mean estimates are computed as sum(weight * L(profile))*beta. L(profile) is the observed-design-grid average fixed-effect row among observations matching each profile row in the spec range after applying any AT settings.")

                If table Is Nothing Then Return ExcelError.ExcelErrorNA
                Return PrepareResultTableForUdf(table.returnSelf())

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.MMRM_LSMESTIMATE", ex)
            End Try
        End Function


        ''' <summary>
        ''' Returns row-level marginal fitted values for a fitted MMRM handle.
        ''' </summary>
        ''' <remarks>
        ''' The returned rows correspond to the valid rows that remained after input screening in
        ''' <c>BESH.REGR.MMRM_FIT</c>. This is a convenience extractor for workbooks that need fitted
        ''' values without the residual column returned by <c>BESH.REGR.MMRM_RESID</c>.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. The default is TRUE.</param>
        ''' <returns>A dynamic array with row number and fitted value columns.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_FITTED",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns marginal fitted values for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_FITTED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row. Default TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As MmrmHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _mmrmCache, h) Then Return ExcelError.ExcelErrorNA
                If h.Result Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim fitted() As Double = If(h.Result.FittedMarginal, Array.Empty(Of Double)())
                If fitted.Length <= 0 Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = ExcelArgNumeric.GetOptionalBool(includeHeader, True)
                Dim out(fitted.Length - 1 + If(hdr, 1, 0), 1) As Object
                Dim r As Integer = 0

                If hdr Then
                    out(0, 0) = "Row"
                    out(0, 1) = "Fitted"
                    r = 1
                End If

                For i As Integer = 0 To fitted.Length - 1
                    out(r + i, 0) = i + 1
                    out(r + i, 1) = fitted(i)
                Next

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.MMRM_FITTED", ex)
            End Try
        End Function


        ''' <summary>
        ''' Returns row-level fitted values and raw marginal residuals for a fitted MMRM handle.
        ''' </summary>
        ''' <remarks>
        ''' The returned rows correspond to the valid rows that remained after input screening in
        ''' <c>BESH.REGR.MMRM_FIT</c>. The residual is the observed response minus the marginal fitted
        ''' value for the fixed-effect part of the model.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. The default is TRUE.</param>
        ''' <returns>A dynamic array with row number, fitted value, and residual columns.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_RESID",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns fitted values and raw marginal residuals for a fitted MMRM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_RESID(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row. Default TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As MmrmHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _mmrmCache, h) Then Return ExcelError.ExcelErrorNA
                If h.Result Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim fitted() As Double = If(h.Result.FittedMarginal, Array.Empty(Of Double)())
                Dim resid() As Double = If(h.Result.ResidualRaw, Array.Empty(Of Double)())
                Dim n As Integer = Math.Max(fitted.Length, resid.Length)
                If n <= 0 Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = ExcelArgNumeric.GetOptionalBool(includeHeader, True)
                Dim out(n - 1 + If(hdr, 1, 0), 2) As Object
                Dim r As Integer = 0
                If hdr Then
                    out(0, 0) = "Row"
                    out(0, 1) = "Fitted"
                    out(0, 2) = "Residual"
                    r = 1
                End If

                For i As Integer = 0 To n - 1
                    out(r + i, 0) = i + 1
                    out(r + i, 1) = If(i < fitted.Length, CType(fitted(i), Object), String.Empty)
                    out(r + i, 2) = If(i < resid.Length, CType(resid(i), Object), String.Empty)
                Next

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.MMRM_RESID", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a fitted MMRM handle from the current worksheet-session cache.
        ''' </summary>
        ''' <remarks>
        ''' Use this function when a saved handle is no longer needed. Removing unused handles can
        ''' reduce memory use in long Excel sessions. Recalculate the original fit function to create
        ''' a new handle.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.MMRM_FIT</c>.</param>
        ''' <returns>TRUE if the handle was found and removed; otherwise FALSE.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_DROP",
            Category:="BESHStatNG - Regression Models",
            Description:="Drops a fitted MMRM handle from the session cache.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.MMRM_FIT.")> handle As Object
        ) As Object
            Dim key As String = Nothing
            If Not UdfCacheHelpers.TryGetHandleKey(handle, key) Then Return False

            Dim removed As MmrmHandle = Nothing
            Return _mmrmCache.TryRemove(key, removed)
        End Function


        ''' <summary>
        ''' Removes all fitted MMRM handles from the current worksheet-session cache.
        ''' </summary>
        ''' <remarks>
        ''' Use this function to clear all MMRM handles created during the current Excel session. It is
        ''' useful before rerunning a large workbook or after exploratory analyses that created many
        ''' temporary model handles. Recalculate any <c>BESH.REGR.MMRM_FIT</c> formulas to recreate
        ''' handles that are still needed.
        ''' </remarks>
        ''' <returns>The number of handles removed from the current session cache.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.MMRM_CLEAR_ALL",
            Category:="BESHStatNG - Regression Models",
            Description:="Drops all fitted MMRM handles from the session cache.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function MMRM_CLEAR_ALL() As Object
            Dim count As Integer = _mmrmCache.Count
            _mmrmCache.Clear()
            Return count
        End Function

        Private Function DefaultNames(count As Integer, prefix As String) As String()
            Dim names(Math.Max(0, count) - 1) As String
            For i As Integer = 0 To names.Length - 1
                names(i) = prefix & (i + 1).ToString(CultureInfo.InvariantCulture)
            Next
            Return names
        End Function


        Private Function AddInterceptIfRequested(x(,) As Double, includeIntercept As Boolean) As Double(,)
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
            If Not includeIntercept Then Return x

            Dim n As Integer = x.GetLength(0)
            Dim p As Integer = x.GetLength(1)
            Dim out(n - 1, p) As Double
            For i As Integer = 0 To n - 1
                out(i, 0) = 1.0
                For j As Integer = 0 To p - 1
                    out(i, j + 1) = x(i, j)
                Next
            Next
            Return out
        End Function

        Private Function ResolveMmrmImportedPredictorNames(varNames As Object,
                                                           inferredSelectedNames() As String,
                                                           selectedAbsoluteLetters() As String) As String()
            Dim selectedCount As Integer = If(inferredSelectedNames Is Nothing, 0, inferredSelectedNames.Length)
            If selectedCount <= 0 Then Return New String() {}

            Dim inferred() As String = NormalizeMmrmNameList(inferredSelectedNames, selectedCount, "X")
            If ExcelArgPredicates.IsMissingArg(varNames) Then Return inferred

            Dim suppliedNames() As String = Nothing
            If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmNameList(varNames, suppliedNames) Then Return inferred
            If suppliedNames Is Nothing OrElse suppliedNames.Length = 0 Then Return inferred

            If suppliedNames.Length = selectedCount Then
                Return NormalizeMmrmNameList(suppliedNames, selectedCount, "X")
            End If

            If selectedAbsoluteLetters IsNot Nothing AndAlso selectedAbsoluteLetters.Length = selectedCount Then
                Dim suppliedLetters() As String = Nothing
                If Global.BESHStatNG.UdfDataImport.TryGetAbsoluteColumnLetters(varNames, suppliedNames.Length, suppliedLetters) Then
                    Dim byLetter As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                    For i As Integer = 0 To Math.Min(suppliedNames.Length, suppliedLetters.Length) - 1
                        Dim letter As String = If(suppliedLetters(i), String.Empty).Trim()
                        If letter = String.Empty Then Continue For
                        If Not byLetter.ContainsKey(letter) Then byLetter(letter) = If(suppliedNames(i), String.Empty).Trim()
                    Next

                    Dim mapped(selectedCount - 1) As String
                    Dim anyMapped As Boolean = False
                    For j As Integer = 0 To selectedCount - 1
                        Dim selectedLetter As String = If(selectedAbsoluteLetters(j), String.Empty).Trim()
                        Dim mappedName As String = Nothing
                        If selectedLetter <> String.Empty AndAlso byLetter.TryGetValue(selectedLetter, mappedName) Then
                            mapped(j) = mappedName
                            anyMapped = True
                        Else
                            mapped(j) = inferred(j)
                        End If
                    Next

                    If anyMapped Then Return NormalizeMmrmNameList(mapped, selectedCount, "X")
                End If
            End If

            Return inferred
        End Function

        Private Function NormalizeMmrmNameList(inputNames() As String, expectedCount As Integer, fallbackPrefix As String) As String()
            If expectedCount <= 0 Then Return New String() {}
            Dim out(expectedCount - 1) As String
            For i As Integer = 0 To expectedCount - 1
                Dim nm As String = Nothing
                If inputNames IsNot Nothing AndAlso i < inputNames.Length Then nm = inputNames(i)
                nm = If(nm, String.Empty).Trim()
                If nm = String.Empty Then nm = fallbackPrefix & (i + 1).ToString(CultureInfo.InvariantCulture)
                out(i) = nm
            Next
            Return out
        End Function

        Private Sub EnsureMmrmResultUsesHandleFixedEffectNames(h As MmrmHandle)
            If h Is Nothing OrElse h.Result Is Nothing Then Exit Sub
            If h.FixedEffectNames Is Nothing OrElse h.FixedEffectNames.Length = 0 Then Exit Sub
            If h.Result.Beta Is Nothing OrElse h.Result.Beta.Length <> h.FixedEffectNames.Length Then Exit Sub
            h.Result.FixedEffectNames = CType(h.FixedEffectNames.Clone(), String())
        End Sub

        Private Function AddInterceptNameIfRequested(names() As String, includeIntercept As Boolean) As String()
            If names Is Nothing Then names = Array.Empty(Of String)()
            If Not includeIntercept Then Return DirectCast(names.Clone(), String())

            Dim out(names.Length) As String
            out(0) = "Intercept"
            For i As Integer = 0 To names.Length - 1
                out(i + 1) = If(String.IsNullOrWhiteSpace(names(i)), "X" & (i + 1).ToString(CultureInfo.InvariantCulture), names(i))
            Next
            Return out
        End Function

        Private Sub ApplyMmrmLsmEstimateAtProfile(components As List(Of MmrmLsmEstimateComponent),
                                                  atProfile As MmrmLsmEstimateAtProfile)
            If components Is Nothing OrElse atProfile Is Nothing Then Exit Sub

            For Each component As MmrmLsmEstimateComponent In components
                If component Is Nothing Then Continue For

                If Not component.VisitSpecified AndAlso atProfile.VisitSpecified Then
                    component.VisitSpecified = True
                    component.VisitValue = atProfile.VisitValue
                End If

                For Each atValue As MmrmLsmEstimateProfileValue In atProfile.ProfileValues
                    If Not ComponentHasProfileColumn(component, atValue.ColumnIndex) Then
                        component.ProfileValues.Add(New MmrmLsmEstimateProfileValue With {
                            .Name = atValue.Name,
                            .ColumnIndex = atValue.ColumnIndex,
                            .Value = atValue.Value
                        })
                    End If
                Next
            Next
        End Sub


        Private Function ComponentHasProfileColumn(component As MmrmLsmEstimateComponent, columnIndex As Integer) As Boolean
            If component Is Nothing OrElse component.ProfileValues Is Nothing Then Return False

            For Each value As MmrmLsmEstimateProfileValue In component.ProfileValues
                If value.ColumnIndex = columnIndex Then Return True
            Next

            Return False
        End Function

        Private Function AverageFittedDesignRowForLsmProfile(h As MmrmHandle, component As MmrmLsmEstimateComponent, ByRef matchedCount As Integer) As Double()
            matchedCount = 0
            If h Is Nothing OrElse h.FittedDesign Is Nothing OrElse component Is Nothing Then Return Nothing

            Dim n As Integer = h.FittedDesign.GetLength(0)
            Dim p As Integer = h.FittedDesign.GetLength(1)
            Dim sums(p - 1) As Double

            For i As Integer = 0 To n - 1
                If component.VisitSpecified Then
                    If h.VisitValues Is Nothing OrElse h.VisitValues.Length <> n Then Continue For
                    If Not regression.MixedModelPostEstimation.NearlyEqual(h.VisitValues(i), component.VisitValue) Then Continue For
                End If

                Dim match As Boolean = True
                For Each profileValue As MmrmLsmEstimateProfileValue In component.ProfileValues
                    If profileValue.ColumnIndex < 0 OrElse profileValue.ColumnIndex >= p Then
                        match = False
                        Exit For
                    End If

                    If Not regression.MixedModelPostEstimation.NearlyEqual(h.FittedDesign(i, profileValue.ColumnIndex), profileValue.Value) Then
                        match = False
                        Exit For
                    End If
                Next

                If Not match Then Continue For

                matchedCount += 1
                For j As Integer = 0 To p - 1
                    sums(j) += h.FittedDesign(i, j)
                Next
            Next

            If matchedCount <= 0 Then Return Nothing

            For j As Integer = 0 To p - 1
                sums(j) /= CDbl(matchedCount)
            Next

            Return sums
        End Function

        Private Function IsMmrmWorksheetRangeArgument(arg As Object) As Boolean
            If ExcelArgPredicates.IsMissingArg(arg) Then Return False
            Return TypeOf arg Is Object(,)
        End Function

        Private Function TryBuildMmrmAtProfileRowMask(h As MmrmHandle,
                                                       atProfile As MmrmLsmEstimateAtProfile,
                                                       ByRef rowMask() As Boolean,
                                                       ByRef profileDescription As String,
                                                       ByRef errorMessage As String) As Boolean
            rowMask = Nothing
            profileDescription = Nothing
            errorMessage = Nothing

            If h Is Nothing OrElse h.FittedDesign Is Nothing OrElse h.VisitValues Is Nothing Then
                errorMessage = "the fit does not contain the design rows and visit values required for LS-means profiles."
                Return False
            End If

            If atProfile Is Nothing OrElse (Not atProfile.VisitSpecified AndAlso atProfile.ProfileValues.Count = 0) Then
                errorMessage = "the supplied profile range does not contain any visit or fitted design-column settings."
                Return False
            End If

            Dim n As Integer = h.FittedDesign.GetLength(0)
            If h.VisitValues.Length <> n Then
                errorMessage = "the fit does not contain usable visit values for the saved design rows."
                Return False
            End If

            rowMask = New Boolean(n - 1) {}
            Dim matchedCount As Integer = 0

            For i As Integer = 0 To n - 1
                Dim keep As Boolean = True

                If atProfile.VisitSpecified AndAlso
                   Not regression.MixedModelPostEstimation.NearlyEqual(h.VisitValues(i), atProfile.VisitValue) Then
                    keep = False
                End If

                If keep Then
                    For Each profileValue As MmrmLsmEstimateProfileValue In atProfile.ProfileValues
                        If profileValue.ColumnIndex < 0 OrElse profileValue.ColumnIndex >= h.FittedDesign.GetLength(1) Then
                            keep = False
                            Exit For
                        End If

                        If Not regression.MixedModelPostEstimation.NearlyEqual(h.FittedDesign(i, profileValue.ColumnIndex), profileValue.Value) Then
                            keep = False
                            Exit For
                        End If
                    Next
                End If

                rowMask(i) = keep
                If keep Then matchedCount += 1
            Next

            If matchedCount <= 0 Then
                errorMessage = "no observed design rows matched the supplied LS-means profile."
                Return False
            End If

            profileDescription = FormatMmrmAtProfileDescription(atProfile)
            Return True
        End Function


        Private Function FormatMmrmAtProfileDescription(atProfile As MmrmLsmEstimateAtProfile) As String
            If atProfile Is Nothing Then Return String.Empty

            Dim parts As New List(Of String)()
            If atProfile.VisitSpecified Then
                parts.Add("visit=" & regression.MixedModelPostEstimation.FormatProfileValue(atProfile.VisitValue))
            End If

            For Each profileValue As MmrmLsmEstimateProfileValue In atProfile.ProfileValues
                parts.Add(profileValue.Name & "=" & regression.MixedModelPostEstimation.FormatProfileValue(profileValue.Value))
            Next

            Return String.Join(", ", parts)
        End Function

        Private Function TryGetDesignColumnValues(h As MmrmHandle,
                                                  requestedName As String,
                                                  ByRef resolvedName As String,
                                                  ByRef values() As Double,
                                                  ByRef errorMessage As String) As Boolean
            resolvedName = Nothing
            values = Nothing
            errorMessage = Nothing

            If h Is Nothing OrElse h.FittedDesign Is Nothing Then
                errorMessage = "the fit does not contain the fixed-effect design rows required for this extractor."
                Return False
            End If

            If h.FixedEffectNames Is Nothing OrElse h.FixedEffectNames.Length <> h.FittedDesign.GetLength(1) Then
                errorMessage = "the fit does not contain usable fixed-effect column names."
                Return False
            End If

            Dim idx As Integer = FindDesignColumnIndex(h.FixedEffectNames, requestedName)
            If idx < 0 Then
                errorMessage = "column """ & requestedName & """ was not found among fitted design columns: " &
                               String.Join(", ", h.FixedEffectNames)
                Return False
            End If

            If String.Equals(h.FixedEffectNames(idx), "Intercept", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(h.FixedEffectNames(idx), "(Intercept)", StringComparison.OrdinalIgnoreCase) Then
                errorMessage = "the intercept column cannot be used as a grouping factor."
                Return False
            End If

            Dim n As Integer = h.FittedDesign.GetLength(0)
            ReDim values(n - 1)

            For i As Integer = 0 To n - 1
                values(i) = h.FittedDesign(i, idx)
            Next

            resolvedName = h.FixedEffectNames(idx)
            Return True
        End Function


        Private Function FindDesignColumnIndex(names() As String, requestedName As String) As Integer
            If names Is Nothing OrElse String.IsNullOrWhiteSpace(requestedName) Then Return -1

            For i As Integer = 0 To names.Length - 1
                If String.Equals(names(i), requestedName, StringComparison.OrdinalIgnoreCase) Then Return i
            Next

            Dim wanted As String = NormalizeDesignColumnName(requestedName)
            For i As Integer = 0 To names.Length - 1
                If String.Equals(NormalizeDesignColumnName(names(i)), wanted, StringComparison.OrdinalIgnoreCase) Then Return i
            Next

            Return -1
        End Function


        Private Function NormalizeDesignColumnName(s As String) As String
            If s Is Nothing Then Return String.Empty
            Return New String(s.Trim().ToLowerInvariant().Where(Function(ch) Char.IsLetterOrDigit(ch)).ToArray())
        End Function


        Private Function TryParseOptionalFiniteDouble(arg As Object, ByRef value As Double) As Boolean
            value = Double.NaN
            If ExcelArgPredicates.IsMissingArg(arg) Then Return True

            Dim parsed As Double? = ExcelArgNumeric.TryGetDouble(arg)
            If Not parsed.HasValue Then Return False

            value = parsed.Value
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function


        Private Function StackResultTables(tables As List(Of ResultTable)) As Object(,)
            If tables Is Nothing OrElse tables.Count = 0 Then
                Dim empty(0, 0) As Object
                empty(0, 0) = "No result tables available."
                Return empty
            End If

            Dim rendered As New List(Of Object(,))()
            Dim totalRows As Integer = 0
            Dim maxCols As Integer = 1

            For Each t As ResultTable In tables
                If t Is Nothing Then Continue For
                Dim arr As Object(,) = t.returnSelf()
                If arr Is Nothing Then Continue For
                rendered.Add(arr)
                totalRows += arr.GetLength(0)
                maxCols = Math.Max(maxCols, arr.GetLength(1))
            Next

            If rendered.Count = 0 Then
                Dim empty(0, 0) As Object
                empty(0, 0) = "No result tables available."
                Return empty
            End If

            totalRows += rendered.Count - 1
            Dim out(totalRows - 1, maxCols - 1) As Object
            Dim r As Integer = 0

            For k As Integer = 0 To rendered.Count - 1
                If k > 0 Then r += 1
                Dim arr As Object(,) = rendered(k)
                For i As Integer = 0 To arr.GetLength(0) - 1
                    For j As Integer = 0 To arr.GetLength(1) - 1
                        out(r + i, j) = arr(i, j)
                    Next
                Next
                r += arr.GetLength(0)
            Next

            Return out
        End Function

    End Module

End Namespace
