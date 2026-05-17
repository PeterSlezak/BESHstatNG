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
    ''' Worksheet functions for fitting and summarizing Gaussian linear mixed models.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These worksheet functions expose linear mixed models directly to Excel formulas. The fit
    ''' function reads a continuous response, fixed-effect predictors, optional random-effect
    ''' predictors, subject identifiers, and an optional visit/time variable. It returns a short
    ''' handle that can be passed to the extractor functions to retrieve coefficient tables,
    ''' covariance estimates, fit statistics, fitted values, residuals, and subject-specific random
    ''' effect predictions.
    ''' </para>
    ''' <para>
    ''' The fixed-effect and random-effect predictor ranges can be supplied as already-coded design
    ''' matrices, or they can be expanded by a right-hand-side formula. Formula mode follows the same
    ''' addressing conventions as the other regression worksheet functions: relative column names,
    ''' worksheet column letters, or explicit variable names.
    ''' </para>
    ''' </remarks>
    Public Module LmmUDFs

        Private ReadOnly _lmmCache As New ConcurrentDictionary(Of String, LmmHandle)(StringComparer.OrdinalIgnoreCase)

        Friend Class LmmHandle
            Public Property Handle As String
            Public Property Result As regression.MixedModelResult
            Public Property Alpha As Double
            Public Property FitMethod As regression.MixedModelFitMethod
            Public Property InferenceMethod As regression.MixedModelFixedInferenceMethod
            Public Property ResidualCovarianceStructure As String
            Public Property RandomCovarianceStructure As String
            Public Property IncludeFixedIntercept As Boolean
            Public Property IncludeRandomIntercept As Boolean
            Public Property SourceRowsUsed As Integer
            Public Property SourceRowsDropped As Integer
            Public Property FixedEffectNames As String()
            Public Property RandomEffectNames As String()
            Public Property FittedDesign As Double(,)
            Public Property RandomDesign As Double(,)
            Public Property VisitValues As Double()
        End Class

        Private Class LmmImportedInputs
            Public Property Y As Double()
            Public Property X As Double(,)
            Public Property Z As Double(,)
            Public Property Subject As Object()
            Public Property Visit As Double()
            Public Property InferredXNames As String()
            Public Property InferredZNames As String()
            Public Property InferredXAbsoluteLetters As String()
            Public Property InferredZAbsoluteLetters As String()
            Public Property DroppedRows As Integer
            Public ReadOnly Property N As Integer
                Get
                    Return If(Y Is Nothing, 0, Y.Length)
                End Get
            End Property
        End Class

        ''' <summary>
        ''' Fits a Gaussian linear mixed model and returns a reusable worksheet-session handle.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Use this function when observations are grouped by subject, site, cluster, or another
        ''' repeated-measure unit and the model contains one or more random effects. The response,
        ''' fixed-effect predictors, and random-effect predictors must be numeric. Subject identifiers
        ''' may be text or numeric. The visit/time input is optional and is used only by residual
        ''' covariance structures that depend on within-subject ordering.
        ''' </para>
        ''' <para>
        ''' Rows with missing or invalid response, fixed-effect predictor, random-effect predictor,
        ''' subject, or supplied visit values are excluded before fitting. The returned handle stores
        ''' the fitted analysis for the current Excel session and can be passed to the LMM extractor
        ''' functions.
        ''' </para>
        ''' <para>
        ''' The fixed-effect design is created from <paramref name="x"/> and optional
        ''' <paramref name="fixedFormula"/>. The random-effect design is created from optional
        ''' <paramref name="z"/> and optional <paramref name="randomFormula"/>. When
        ''' <paramref name="includeRandomIntercept"/> is TRUE, an intercept column is prepended to
        ''' the random-effect design. Therefore a random-intercept-only model can be fitted without
        ''' supplying <paramref name="z"/>.
        ''' </para>
        ''' <para>
        ''' The default fit uses REML, an identity residual covariance structure, a random intercept
        ''' when no random-effect predictors are supplied, variance-components random-effect
        ''' covariance when multiple random-effect columns are present, and Kenward-Roger fixed-effect
        ''' inference. Kenward-Roger inference requires REML. To use maximum likelihood, set
        ''' <paramref name="fitMethod"/> to <c>"ML"</c> and choose a non-Kenward-Roger inference
        ''' method.
        ''' </para>
        ''' <para>
        ''' Common follow-up functions are <c>BESH.REGR.LMM_RESULTS</c>, <c>BESH.REGR.LMM_COEF</c>,
        ''' <c>BESH.REGR.LMM_TYPE3</c>, <c>BESH.REGR.LMM_COVPARMS</c>,
        ''' <c>BESH.REGR.LMM_G_COV</c>, <c>BESH.REGR.LMM_G_CORR</c>,
        ''' <c>BESH.REGR.LMM_R_COV</c>, <c>BESH.REGR.LMM_R_CORR</c>,
        ''' <c>BESH.REGR.LMM_RANEF</c>, <c>BESH.REGR.LMM_FITSTATS</c>,
        ''' <c>BESH.REGR.LMM_FITTED</c>, and <c>BESH.REGR.LMM_RESID</c>.
        ''' </para>
        ''' </remarks>
        ''' <param name="y">Single-column range containing the continuous response values.</param>
        ''' <param name="x">Numeric matrix containing raw fixed-effect predictors or an already-coded fixed-effect design matrix. Each row must correspond to the same row in <paramref name="y"/>.</param>
        ''' <param name="subject">Single-column range identifying the subject, cluster, or repeated-measure unit for each observation.</param>
        ''' <param name="z">Optional numeric matrix containing raw random-effect predictors or an already-coded random-effect design matrix. Leave blank for a random-intercept-only model.</param>
        ''' <param name="visit">Optional single-column numeric visit/time range. Required for visit-indexed residual structures when row order is not sufficient.</param>
        ''' <param name="xVarNames">Optional row or column range containing predictor names for the columns of <paramref name="x"/>.</param>
        ''' <param name="zVarNames">Optional row or column range containing predictor names for the columns of <paramref name="z"/>.</param>
        ''' <param name="residualCovariance">Residual covariance structure. Accepted values include <c>ID</c>, <c>Diagonal</c>, <c>CS</c>, <c>HCS</c>, <c>AR(1)</c>, <c>HAR(1)</c>, <c>TOEP</c>, <c>TOEPH</c>, and <c>UN</c>. The default is <c>ID</c>.</param>
        ''' <param name="randomCovariance">Random-effect covariance structure. Accepted values include <c>RI</c>, <c>RI+S</c>, <c>ID</c>, <c>VC</c>, <c>CS</c>, <c>CSH</c>, <c>AR1</c>, <c>ARH1</c>, <c>TOEP</c>, <c>TOEPH</c>, and <c>UN</c>. If omitted, the function chooses a safe default from the random-effect design.</param>
        ''' <param name="fitMethod">Likelihood method. Use <c>REML</c> for restricted maximum likelihood or <c>ML</c> for maximum likelihood. The default is <c>REML</c>.</param>
        ''' <param name="inference">Fixed-effect inference method. Accepted values include <c>KR</c>, <c>Satterthwaite</c>, <c>BetweenWithin</c>, <c>ResidualDF</c>, and <c>Wald</c>. The default is <c>KR</c>.</param>
        ''' <param name="includeFixedIntercept">TRUE to add an intercept column to the fixed-effect design; FALSE when <paramref name="x"/> or <paramref name="fixedFormula"/> already contains all desired columns. The default is TRUE.</param>
        ''' <param name="includeRandomIntercept">TRUE to add an intercept column to the random-effect design. The default is TRUE.</param>
        ''' <param name="fixedFormula">Optional right-hand-side formula used to expand the raw fixed-effect predictor matrix before fitting. Leave blank to use the columns of <paramref name="x"/> as supplied.</param>
        ''' <param name="randomFormula">Optional right-hand-side formula used to expand the raw random-effect predictor matrix before fitting. Leave blank to use the columns of <paramref name="z"/> as supplied.</param>
        ''' <param name="formulaAddressing">Formula-addressing mode: <c>relative</c> (default), <c>absolute</c>, or <c>names</c>.</param>
        ''' <param name="alpha">Two-sided alpha level for confidence intervals returned by extractor functions. The default is 0.05.</param>
        ''' <param name="maxIter">Optional maximum number of optimizer iterations. Leave blank to use the standard setting.</param>
        ''' <param name="trace">TRUE to store optimizer trace text for later result output. The default is FALSE.</param>
        ''' <param name="covOptimizerMode">Optional covariance optimizer. Accepted values include <c>AI</c>, <c>AverageInformation</c>, <c>FisherScoring</c>, <c>SAS</c>, <c>BFGS</c>, <c>BFGS_ANALYTIC</c>, and <c>BFGS_NUMERICAL</c>.</param>
        ''' <param name="covGradientMode">Optional covariance-gradient mode for BFGS/fallback paths. Accepted values include <c>Auto</c>, <c>Analytic</c>, <c>AnalyticValidation</c>, <c>Validate</c>, <c>Numerical</c>, and <c>FiniteDifference</c>.</param>
        ''' <returns>A text handle that identifies the fitted model in the current Excel session, or a descriptive error message if the fit cannot be created.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_FIT",
            Category:="BESHStatNG - Regression Models",
            Description:="Fits a linear mixed model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_FIT(
            <ExcelArgument(Name:="y", Description:="Continuous response vector.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Numeric fixed-effect predictor/design matrix.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="subject", Description:="Subject/cluster identifier vector.")> subject As Object,
            <ExcelArgument(AllowReference:=True, Name:="z", Description:="Optional numeric random-effect predictor/design matrix. Leave blank for random intercept only.")> Optional z As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="visit", Description:="Optional numeric visit/time vector for visit-indexed residual covariance structures.")> Optional visit As Object = Nothing,
            <ExcelArgument(Name:="xVarNames", Description:="Optional predictor names for columns of x.")> Optional xVarNames As Object = Nothing,
            <ExcelArgument(Name:="zVarNames", Description:="Optional predictor names for columns of z.")> Optional zVarNames As Object = Nothing,
            <ExcelArgument(Name:="residualCovariance", Description:="R-side covariance: ID, Diagonal, CS, HCS, AR(1), HAR(1), TOEP, TOEPH, or UN. Default ID.")> Optional residualCovariance As Object = Nothing,
            <ExcelArgument(Name:="randomCovariance", Description:="G-side covariance: RI, RI+S, ID, VC, CS, CSH, AR1, ARH1, TOEP, TOEPH, or UN. Default chosen from random design.")> Optional randomCovariance As Object = Nothing,
            <ExcelArgument(Name:="fitMethod", Description:="REML or ML. Default REML.")> Optional fitMethod As Object = Nothing,
            <ExcelArgument(Name:="inference", Description:="KR, Satterthwaite, BetweenWithin, ResidualDF, or Wald. Default KR.")> Optional inference As Object = Nothing,
            <ExcelArgument(Name:="includeFixedIntercept", Description:="TRUE to prepend an intercept column to fixed effects. Default TRUE.")> Optional includeFixedIntercept As Object = Nothing,
            <ExcelArgument(Name:="includeRandomIntercept", Description:="TRUE to prepend an intercept column to random effects. Default TRUE.")> Optional includeRandomIntercept As Object = Nothing,
            <ExcelArgument(Name:="fixedFormula", Description:="Optional RHS formula used to expand fixed-effect predictors.")> Optional fixedFormula As Object = Nothing,
            <ExcelArgument(Name:="randomFormula", Description:="Optional RHS formula used to expand random-effect predictors.")> Optional randomFormula As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Formula-addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha for confidence intervals. Default 0.05.")> Optional alpha As Object = Nothing,
            <ExcelArgument(Name:="maxIter", Description:="Optional optimizer maximum iterations. Default engine setting.")> Optional maxIter As Object = Nothing,
            <ExcelArgument(Name:="trace", Description:="TRUE to store optimizer trace text for later LMM_RESULTS output. Default FALSE.")> Optional trace As Object = Nothing,
            <ExcelArgument(Name:="covOptimizerMode", Description:="Covariance optimizer: AI/AverageInformation/FisherScoring/SAS (default), BFGS, BFGS_ANALYTIC, or BFGS_NUMERICAL.")> Optional covOptimizerMode As Object = Nothing,
            <ExcelArgument(Name:="covGradientMode", Description:="Covariance gradient for BFGS/fallback: Auto (default), Analytic, AnalyticValidation/Validate, Numerical/FiniteDifference.")> Optional covGradientMode As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "LMM_FIT (editing...)"

            Try
                Dim alphaValue As Double = 0.05
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim includeFixedInterceptValue As Boolean = ExcelArgNumeric.GetOptionalBool(includeFixedIntercept, True)
                Dim includeRandomInterceptValue As Boolean = ExcelArgNumeric.GetOptionalBool(includeRandomIntercept, True)
                Dim traceValue As Boolean = ExcelArgNumeric.GetOptionalBool(trace, False)

                Dim fit As regression.MixedModelFitMethod = ParseMixedModelFitMethodStrict(fitMethod, "LMM")
                Dim fixedInference As regression.MixedModelFixedInferenceMethod = ParseMixedModelInferenceMethodStrict(inference, "LMM")
                Dim optimizerMode As regression.MixedModelCovarianceOptimizerMode = ParseMixedModelCovarianceOptimizerModeStrict(covOptimizerMode, "LMM")
                Dim gradientMode As regression.MixedModelCovarianceGradientMode = ParseMixedModelCovarianceGradientModeStrict(covGradientMode, "LMM")
                ApplyMixedModelOptimizerShortcutToGradient(covOptimizerMode, gradientMode)

                If fixedInference = regression.MixedModelFixedInferenceMethod.KenwardRoger AndAlso fit <> regression.MixedModelFitMethod.REML Then
                    Return "BESH.REGR.LMM_FIT error: Kenward-Roger inference requires REML. Set fitMethod to ""REML"" or choose another inference method."
                End If

                Dim fixedFormulaText As String = ExcelArgReaders.AsString(fixedFormula)
                If String.IsNullOrWhiteSpace(fixedFormulaText) Then fixedFormulaText = Nothing

                Dim randomFormulaText As String = ExcelArgReaders.AsString(randomFormula)
                If String.IsNullOrWhiteSpace(randomFormulaText) Then randomFormulaText = Nothing

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

                Dim imported As LmmImportedInputs = Nothing
                Dim errorMessage As String = Nothing
                If Not TryGetLmmAlignedInputs(y:=y,
                                               x:=x,
                                               z:=z,
                                               subject:=subject,
                                               visit:=visit,
                                               imported:=imported,
                                               errorMessage:=errorMessage) Then
                    Return "BESH.REGR.LMM_FIT error: " & errorMessage
                End If

                Dim xNames() As String = ResolveImportedNames(xVarNames, imported.InferredXNames, imported.X.GetLength(1), "X")
                Dim zNames() As String = ResolveImportedNames(zVarNames, imported.InferredZNames, MatrixColumnCount(imported.Z), "Z")

                Dim xAbsoluteColumnLetters() As String = Nothing
                If allowAbsoluteColumnLetters AndAlso Not String.IsNullOrWhiteSpace(fixedFormulaText) Then
                    xAbsoluteColumnLetters = imported.InferredXAbsoluteLetters
                    If xAbsoluteColumnLetters Is Nothing OrElse xAbsoluteColumnLetters.Length <> imported.X.GetLength(1) Then
                        Return "BESH.REGR.LMM_FIT error: formulaAddressing='absolute' requires x to be passed as a direct worksheet range so absolute worksheet column letters can be determined for the fixedFormula terms."
                    End If
                End If

                Dim zAbsoluteColumnLetters() As String = Nothing
                If allowAbsoluteColumnLetters AndAlso Not String.IsNullOrWhiteSpace(randomFormulaText) Then
                    zAbsoluteColumnLetters = imported.InferredZAbsoluteLetters
                    If zAbsoluteColumnLetters Is Nothing OrElse zAbsoluteColumnLetters.Length <> MatrixColumnCount(imported.Z) Then
                        Return "BESH.REGR.LMM_FIT error: formulaAddressing='absolute' requires z to be passed as a direct worksheet range so absolute worksheet column letters can be determined for the randomFormula terms."
                    End If
                End If

                Dim fixedDesign(,) As Double = Nothing
                Dim fixedNames() As String = Nothing
                If Not TryBuildLmmExpandedDesign(raw:=imported.X,
                                                 rawNames:=xNames,
                                                 formulaText:=fixedFormulaText,
                                                 absoluteColumnLetters:=xAbsoluteColumnLetters,
                                                 allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                 allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                 allowQuotedVariableNames:=allowQuotedVariableNames,
                                                 role:="fixedFormula",
                                                 fallbackPrefix:="X",
                                                 design:=fixedDesign,
                                                 designNames:=fixedNames,
                                                 errorMessage:=errorMessage) Then
                    Return "BESH.REGR.LMM_FIT error: " & errorMessage
                End If

                Dim randomExpanded(,) As Double = Nothing
                Dim randomExpandedNames() As String = New String() {}
                If imported.Z IsNot Nothing Then
                    If Not TryBuildLmmExpandedDesign(raw:=imported.Z,
                                                     rawNames:=zNames,
                                                     formulaText:=randomFormulaText,
                                                     absoluteColumnLetters:=zAbsoluteColumnLetters,
                                                     allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                     allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                     allowQuotedVariableNames:=allowQuotedVariableNames,
                                                     role:="randomFormula",
                                                     fallbackPrefix:="Z",
                                                     design:=randomExpanded,
                                                     designNames:=randomExpandedNames,
                                                     errorMessage:=errorMessage) Then
                        Return "BESH.REGR.LMM_FIT error: " & errorMessage
                    End If
                ElseIf Not String.IsNullOrWhiteSpace(randomFormulaText) Then
                    Return "BESH.REGR.LMM_FIT error: randomFormula was supplied, but z was not supplied. Provide z or leave randomFormula blank for a random-intercept-only model."
                End If

                Dim fitX(,) As Double = regression.MixedModelFrontEndHelpers.AddInterceptIfRequested(fixedDesign, includeFixedInterceptValue)
                Dim fitNames() As String = regression.MixedModelFrontEndHelpers.AddInterceptNameIfRequested(fixedNames, includeFixedInterceptValue)

                Dim fitZ(,) As Double = Nothing
                Dim randomNames() As String = Nothing
                If includeRandomInterceptValue Then
                    fitZ = regression.MixedModelFrontEndHelpers.AddInterceptColumn(randomExpanded, imported.N)
                    randomNames = regression.MixedModelFrontEndHelpers.AddInterceptName(randomExpandedNames)
                Else
                    If randomExpanded Is Nothing OrElse MatrixColumnCount(randomExpanded) <= 0 Then
                        Return "BESH.REGR.LMM_FIT error: the random-effects design contains no columns. Supply z/randomFormula or set includeRandomIntercept to TRUE."
                    End If
                    fitZ = randomExpanded
                    randomNames = If(randomExpandedNames, New String() {})
                End If

                If fitZ Is Nothing OrElse fitZ.GetLength(1) <= 0 Then
                    Return "BESH.REGR.LMM_FIT error: the random-effects design contains no columns. Supply z/randomFormula or set includeRandomIntercept to TRUE."
                End If

                Dim residualCovName As String = NormalizeMixedModelResidualCovarianceName(residualCovariance, "Identity")

                Dim randomCovName As String = ExcelArgReaders.AsString(randomCovariance)
                If String.IsNullOrWhiteSpace(randomCovName) Then
                    randomCovName = DefaultRandomCovarianceName(includeRandomInterceptValue, MatrixColumnCount(randomExpanded), fitZ.GetLength(1))
                End If

                regression.MixedModelFrontEndHelpers.ValidateRandomStructureAgainstDesign(randomCovName,
                                                                                         fitZ,
                                                                                         randomNames,
                                                                                         includeRandomInterceptValue,
                                                                                         MatrixColumnCount(randomExpanded),
                                                                                         True)

                Dim data As regression.MixedModelBlockData = regression.MixedModelBlockData.FromArrays(y:=imported.Y,
                                                                                                        x:=fitX,
                                                                                                        subjectId:=imported.Subject,
                                                                                                        z:=fitZ,
                                                                                                        visit:=imported.Visit,
                                                                                                        sortWithinSubjectByVisit:=True)

                Dim rStruct As regression.MixedModelRStruct = regression.MixedModelRStructUtils.createMixedModelRStruct(residualCovName)
                Dim gStruct As regression.MixedModelGStruct = regression.MixedModelGStructUtils.createMixedModelGStruct(randomCovName)
                Dim req As regression.MixedModelFitRequest = regression.MixedModelFitRequest.CreateLMM(data, rStruct, gStruct, fit)
                req.FixedEffectNames = fitNames
                req.RandomEffectNames = randomNames
                req.FixedInferenceMethod = fixedInference
                req.ResponseVarName = "y"
                req.SubjectVarName = "subject"
                req.VisitVarName = If(imported.Visit Is Nothing, String.Empty, "visit")
                req.FixedFormulaText = If(fixedFormulaText, String.Empty)
                req.RandomFormulaText = If(randomFormulaText, String.Empty)
                req.RequestLabel = "LMM UDF"

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
                        req.EnableFullKenwardRogerForLmm()
                    Case regression.MixedModelFixedInferenceMethod.Satterthwaite
                        req.UseSatterthwaite = True
                        req.UseKenwardRoger = False
                        req.FixedInferenceMethod = fixedInference
                    Case Else
                        req.UseSatterthwaite = False
                        req.UseKenwardRoger = False
                        req.FixedInferenceMethod = fixedInference
                End Select

                req.Validate()

                Dim model As New regression.LMM(req)
                Dim result As regression.MixedModelResult = model.Fit()
                If result IsNot Nothing Then
                    If fitNames IsNot Nothing AndAlso result.Beta IsNot Nothing AndAlso result.Beta.Length = fitNames.Length Then
                        result.FixedEffectNames = CType(fitNames.Clone(), String())
                    End If
                    If randomNames IsNot Nothing Then
                        result.RandomCovarianceLabels = CType(randomNames.Clone(), String())
                    End If
                End If

                Dim handleKey As String = "LMM:" & Guid.NewGuid().ToString("N")
                Dim h As New LmmHandle With {
                    .Handle = handleKey,
                    .Result = result,
                    .Alpha = alphaValue,
                    .FitMethod = fit,
                    .InferenceMethod = fixedInference,
                    .ResidualCovarianceStructure = residualCovName,
                    .RandomCovarianceStructure = randomCovName,
                    .IncludeFixedIntercept = includeFixedInterceptValue,
                    .IncludeRandomIntercept = includeRandomInterceptValue,
                    .SourceRowsUsed = imported.N,
                    .SourceRowsDropped = imported.DroppedRows,
                    .FixedEffectNames = fitNames,
                    .RandomEffectNames = randomNames,
                    .FittedDesign = fitX,
                    .RandomDesign = fitZ,
                    .VisitValues = imported.Visit
                }

                _lmmCache(handleKey) = h
                Return handleKey

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.LMM_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns all result tables, or one selected result table, from a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Leave <paramref name="table"/> blank to return the complete set of available tables stacked
        ''' vertically. Provide a table title to return only that table. Common table titles include
        ''' <c>Fixed effects</c>, <c>Kenward-Roger term-level F tests</c>,
        ''' <c>Covariance parameters</c>, <c>Estimated G covariance matrix</c>,
        ''' <c>Estimated G correlation matrix</c>, <c>Estimated R covariance matrix</c>,
        ''' <c>Estimated R correlation matrix</c>, <c>BLUPs / random effects</c>,
        ''' <c>Fit statistics</c>, and <c>Convergence</c>.
        ''' </para>
        ''' <para>
        ''' If trace output was requested when the model was fitted, set
        ''' <paramref name="includeOptimizerTrace"/> to TRUE to include the iteration history in the
        ''' returned result set.
        ''' </para>
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <param name="table">Optional result-table title to return. Leave blank to return all available tables.</param>
        ''' <param name="includeOptimizerTrace">TRUE to include stored optimizer trace information when returning all tables. The default is FALSE.</param>
        ''' <param name="alpha">Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.</param>
        ''' <returns>A dynamic array containing the requested result table or tables.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_RESULTS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns all LMM result tables or one named table for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_RESULTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object,
            <ExcelArgument(Name:="table", Description:="Optional table name, e.g. Fixed effects, Covariance parameters, Estimated G covariance matrix, BLUPs / random effects, Fit statistics, Convergence.")> Optional table As Object = Nothing,
            <ExcelArgument(Name:="includeOptimizerTrace", Description:="TRUE to include optimizer trace if stored. Default FALSE.")> Optional includeOptimizerTrace As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha for confidence intervals. Default is the fit alpha.")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim h As LmmHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _lmmCache, h) Then Return ExcelError.ExcelErrorNA
                If h.Result Is Nothing Then Return ExcelError.ExcelErrorNA
                EnsureLmmResultUsesHandleNames(h)

                Dim alphaValue As Double = h.Alpha
                If Not ExcelArgPredicates.IsMissingArg(alpha) Then
                    If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum
                End If

                Dim includeTrace As Boolean = ExcelArgNumeric.GetOptionalBool(includeOptimizerTrace, False)
                Dim tables As List(Of ResultTable) = h.Result.wrapResults(alpha:=alphaValue,
                                                                          includeOptimizerTrace:=includeTrace,
                                                                          includeKenwardRogerTermTests:=h.InferenceMethod = regression.MixedModelFixedInferenceMethod.KenwardRoger,
                                                                          includeDiagnostics:=True)
                Dim tableName As String = ExcelArgReaders.AsString(table)
                If String.IsNullOrWhiteSpace(tableName) Then
                    Return PrepareResultTableForUdf(StackResultTables(tables))
                End If

                Dim selected As Object(,) = FindResultTableByTitle(tables, tableName)
                If selected Is Nothing Then Return ExcelError.ExcelErrorNA
                Return PrepareResultTableForUdf(selected)

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.REGR.LMM_RESULTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the fixed-effect coefficient table for a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' The table contains estimates, standard errors, test statistics, p-values, and confidence
        ''' intervals for the fixed-effect coefficients saved with the fitted model. Denominator
        ''' degrees of freedom are included when the selected inference method provides them.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <param name="alpha">Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.</param>
        ''' <returns>A dynamic array containing the fixed-effect coefficient table.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_COEF",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the fixed-effect coefficient table for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_COEF(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha for confidence intervals. Default is the fit alpha.")> Optional alpha As Object = Nothing
        ) As Object
            Return LMM_RESULTS(handle, "Fixed effects", False, alpha)
        End Function

        ''' <summary>
        ''' Returns the term-level fixed-effect F-test table for a fitted LMM handle when available.
        ''' </summary>
        ''' <remarks>
        ''' This extractor is intended for fits that requested Kenward-Roger fixed-effect inference.
        ''' It returns multi-degree-of-freedom tests for fixed-effect terms when the necessary
        ''' information is available in the fitted handle. If the selected inference method does not
        ''' create term-level tests, the function returns <c>#N/A</c>.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <param name="alpha">Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.</param>
        ''' <returns>A dynamic array containing term-level fixed-effect tests, or <c>#N/A</c> when the table is not available.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_TYPE3",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns term-level fixed-effect tests for a fitted LMM handle when available.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_TYPE3(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object,
            <ExcelArgument(Name:="alpha", Description:="Optional alpha for confidence intervals. Default is the fit alpha.")> Optional alpha As Object = Nothing
        ) As Object
            Return LMM_RESULTS(handle, "Kenward-Roger term-level F tests", False, alpha)
        End Function

        ''' <summary>
        ''' Returns the covariance-parameter table for a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' The table lists optimized covariance parameters for the random-effect side and residual
        ''' side. These parameters are shown on the internal optimization scale; use the G-side and
        ''' R-side covariance/correlation extractors for user-scale covariance matrices.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <returns>A dynamic array containing covariance-parameter estimates.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_COVPARMS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns covariance-parameter estimates for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_COVPARMS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object
        ) As Object
            Return LMM_RESULTS(handle, "Covariance parameters", False, Nothing)
        End Function

        ''' <summary>
        ''' Returns the fitted random-effect covariance matrix for a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' The matrix is shown on the user/statistical scale and is aligned with the random-effect
        ''' columns used in the fitted model.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <returns>A dynamic array containing the fitted random-effect covariance matrix.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_G_COV",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the fitted G-side random-effect covariance matrix for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_G_COV(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object
        ) As Object
            Return LMM_RESULTS(handle, "Estimated G covariance matrix", False, Nothing)
        End Function

        ''' <summary>
        ''' Returns the fitted random-effect correlation matrix for a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' The matrix is derived from the fitted random-effect covariance matrix and is aligned with
        ''' the random-effect columns used in the fitted model.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <returns>A dynamic array containing the fitted random-effect correlation matrix.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_G_CORR",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the fitted G-side random-effect correlation matrix for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_G_CORR(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object
        ) As Object
            Return LMM_RESULTS(handle, "Estimated G correlation matrix", False, Nothing)
        End Function

        ''' <summary>
        ''' Returns the fitted residual covariance matrix for a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' The matrix is shown on the user/statistical scale. For visit-indexed residual covariance
        ''' structures, rows and columns follow the visit/time ordering used by the fitted model.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <returns>A dynamic array containing the fitted residual covariance matrix.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_R_COV",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the fitted R-side residual covariance matrix for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_R_COV(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object
        ) As Object
            Return LMM_RESULTS(handle, "Estimated R covariance matrix", False, Nothing)
        End Function

        ''' <summary>
        ''' Returns the fitted residual correlation matrix for a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' The matrix is derived from the fitted residual covariance matrix. For visit-indexed
        ''' residual covariance structures, rows and columns follow the visit/time ordering used by
        ''' the fitted model.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <returns>A dynamic array containing the fitted residual correlation matrix.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_R_CORR",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the fitted R-side residual correlation matrix for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_R_CORR(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object
        ) As Object
            Return LMM_RESULTS(handle, "Estimated R correlation matrix", False, Nothing)
        End Function

        ''' <summary>
        ''' Returns subject-specific random-effect predictions for a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' The returned table contains empirical Bayes predictions for each subject and random-effect
        ''' column. These values are conditional random-effect predictions, not additional
        ''' fixed-effect coefficients.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <returns>A dynamic array containing subject-specific random-effect predictions.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_RANEF",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns subject-specific random-effect predictions for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_RANEF(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object
        ) As Object
            Return LMM_RESULTS(handle, "BLUPs / random effects", False, Nothing)
        End Function

        ''' <summary>
        ''' Returns model-level fit statistics for a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' The table includes likelihood criterion, information criteria, model dimensions,
        ''' execution time when available, and related model-fit summaries.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <returns>A dynamic array containing model-level fit statistics.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_FITSTATS",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns fit statistics for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_FITSTATS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object
        ) As Object
            Return LMM_RESULTS(handle, "Fit statistics", False, Nothing)
        End Function

        ''' <summary>
        ''' Returns row-level marginal fitted values for a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' The returned rows correspond to the valid rows that remained after input screening in
        ''' <c>BESH.REGR.LMM_FIT</c>. The fitted value is the model-implied marginal mean for the
        ''' fixed-effect part of the model.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. The default is TRUE.</param>
        ''' <returns>A dynamic array with row number and fitted value columns.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_FITTED",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns marginal fitted values for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_FITTED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row. Default TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As LmmHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _lmmCache, h) Then Return ExcelError.ExcelErrorNA
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
                Return LoggedUdfExceptionText("BESH.REGR.LMM_FITTED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns row-level marginal fitted values and raw residuals for a fitted LMM handle.
        ''' </summary>
        ''' <remarks>
        ''' The returned rows correspond to the valid rows that remained after input screening in
        ''' <c>BESH.REGR.LMM_FIT</c>. The residual is the observed response minus the marginal fitted
        ''' value for the fixed-effect part of the model.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. The default is TRUE.</param>
        ''' <returns>A dynamic array with row number, fitted value, and residual columns.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_RESID",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns fitted values and raw marginal residuals for a fitted LMM handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_RESID(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row. Default TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim h As LmmHandle = Nothing
                If Not UdfCacheHelpers.TryGetCachedHandle(handle, _lmmCache, h) Then Return ExcelError.ExcelErrorNA
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
                Return LoggedUdfExceptionText("BESH.REGR.LMM_RESID", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a fitted LMM handle from the current worksheet-session cache.
        ''' </summary>
        ''' <remarks>
        ''' Use this function when a saved handle is no longer needed. Removing unused handles can
        ''' reduce memory use in long Excel sessions. Recalculate the original fit function to create
        ''' a new handle.
        ''' </remarks>
        ''' <param name="handle">Handle returned by <c>BESH.REGR.LMM_FIT</c>.</param>
        ''' <returns>TRUE if the handle was found and removed; otherwise FALSE.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_DROP",
            Category:="BESHStatNG - Regression Models",
            Description:="Drops a fitted LMM handle from the session cache.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.REGR.LMM_FIT.")> handle As Object
        ) As Object
            Dim key As String = Nothing
            If Not UdfCacheHelpers.TryGetHandleKey(handle, key) Then Return False

            Dim removed As LmmHandle = Nothing
            Return _lmmCache.TryRemove(key, removed)
        End Function

        ''' <summary>
        ''' Removes all fitted LMM handles from the current worksheet-session cache.
        ''' </summary>
        ''' <remarks>
        ''' Use this function to clear all LMM handles created during the current Excel session. It is
        ''' useful before rerunning a large workbook or after exploratory analyses that created many
        ''' temporary model handles. Recalculate any <c>BESH.REGR.LMM_FIT</c> formulas to recreate
        ''' handles that are still needed.
        ''' </remarks>
        ''' <returns>The number of handles removed from the current session cache.</returns>
        <ExcelFunction(
            Name:="BESH.REGR.LMM_CLEAR_ALL",
            Category:="BESHStatNG - Regression Models",
            Description:="Drops all fitted LMM handles from the session cache.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function LMM_CLEAR_ALL() As Object
            Dim count As Integer = _lmmCache.Count
            _lmmCache.Clear()
            Return count
        End Function

        Private Function TryGetLmmAlignedInputs(y As Object,
                                                x As Object,
                                                z As Object,
                                                subject As Object,
                                                visit As Object,
                                                ByRef imported As LmmImportedInputs,
                                                ByRef errorMessage As String) As Boolean
            imported = Nothing
            errorMessage = Nothing

            Dim yCol(,) As Object = Nothing
            Dim yName As String = Nothing
            If Not Global.BESHStatNG.UdfDataImport.TryGetTrimmedColumnObject(y, yCol, yName, "numeric") Then
                errorMessage = "y must be a non-empty single-column numeric range."
                Return False
            End If

            Dim xRaw(,) As Object = Nothing
            Dim inferredXNames() As String = Nothing
            If Not Global.BESHStatNG.UdfDataImport.TryGetTrimmedNumericMatrixObject(x, xRaw, inferredXNames) Then
                errorMessage = "x must be a non-empty numeric matrix."
                Return False
            End If

            Dim subjectCol(,) As Object = Nothing
            Dim subjectName As String = Nothing
            If Not Global.BESHStatNG.UdfDataImport.TryGetTrimmedColumnObject(subject, subjectCol, subjectName, "text") Then
                errorMessage = "subject must be a non-empty single-column range."
                Return False
            End If

            Dim zRaw(,) As Object = Nothing
            Dim inferredZNames() As String = New String() {}
            Dim zSupplied As Boolean = Not ExcelArgPredicates.IsMissingArg(z)
            If zSupplied Then
                If Not Global.BESHStatNG.UdfDataImport.TryGetTrimmedNumericMatrixObject(z, zRaw, inferredZNames) Then
                    errorMessage = "z must be a numeric matrix when supplied."
                    Return False
                End If
            End If

            Dim visitCol(,) As Object = Nothing
            Dim visitSupplied As Boolean = Not ExcelArgPredicates.IsMissingArg(visit)
            If visitSupplied Then
                Dim visitName As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetTrimmedColumnObject(visit, visitCol, visitName, "numeric") Then
                    errorMessage = "visit must be a numeric single-column range when supplied."
                    Return False
                End If
            End If

            Dim n As Integer = yCol.GetLength(0)
            If xRaw.GetLength(0) <> n OrElse subjectCol.GetLength(0) <> n OrElse
               (zSupplied AndAlso zRaw.GetLength(0) <> n) OrElse
               (visitSupplied AndAlso visitCol.GetLength(0) <> n) Then
                errorMessage = "y, x, z, subject, and visit inputs must have the same number of data rows after optional header trimming."
                Return False
            End If

            Dim pX As Integer = xRaw.GetLength(1)
            Dim pZ As Integer = If(zSupplied, zRaw.GetLength(1), 0)
            If pX <= 0 Then
                errorMessage = "x must contain at least one fixed-effect predictor column."
                Return False
            End If

            Dim xLetters() As String = Nothing
            Global.BESHStatNG.UdfDataImport.TryGetAbsoluteColumnLetters(x, pX, xLetters)
            Dim zLetters() As String = Nothing
            If zSupplied Then Global.BESHStatNG.UdfDataImport.TryGetAbsoluteColumnLetters(z, pZ, zLetters)

            Dim yList As New List(Of Double)()
            Dim xRows As New List(Of Double())()
            Dim zRows As New List(Of Double())()
            Dim subjectList As New List(Of Object)()
            Dim visitList As New List(Of Double)()
            Dim dropped As Integer = 0

            For i As Integer = 0 To n - 1
                Dim yi As Double? = ExcelArgNumeric.TryGetDouble(yCol(i, 0))
                Dim subjectText As String = ExcelArgReaders.CellToTrimmedText(subjectCol(i, 0))
                Dim valid As Boolean = yi.HasValue AndAlso Not String.IsNullOrWhiteSpace(subjectText)

                Dim xRow(pX - 1) As Double
                If valid Then
                    For j As Integer = 0 To pX - 1
                        Dim xij As Double? = ExcelArgNumeric.TryGetDouble(xRaw(i, j))
                        If Not xij.HasValue Then
                            valid = False
                            Exit For
                        End If
                        xRow(j) = xij.Value
                    Next
                End If

                Dim zRow() As Double = Nothing
                If valid AndAlso zSupplied Then
                    ReDim zRow(pZ - 1)
                    For j As Integer = 0 To pZ - 1
                        Dim zij As Double? = ExcelArgNumeric.TryGetDouble(zRaw(i, j))
                        If Not zij.HasValue Then
                            valid = False
                            Exit For
                        End If
                        zRow(j) = zij.Value
                    Next
                End If

                Dim vi As Double = Double.NaN
                If valid AndAlso visitSupplied Then
                    Dim parsedVisit As Double? = ExcelArgNumeric.TryGetDouble(visitCol(i, 0))
                    If Not parsedVisit.HasValue Then
                        valid = False
                    Else
                        vi = parsedVisit.Value
                    End If
                End If

                If valid Then
                    yList.Add(yi.Value)
                    xRows.Add(xRow)
                    If zSupplied Then zRows.Add(zRow)
                    subjectList.Add(subjectText)
                    If visitSupplied Then visitList.Add(vi)
                Else
                    dropped += 1
                End If
            Next

            If yList.Count = 0 Then
                errorMessage = "no valid complete rows remain after removing rows with missing/invalid y, x, z, subject, or visit values."
                Return False
            End If

            Dim xValues(yList.Count - 1, pX - 1) As Double
            For i As Integer = 0 To yList.Count - 1
                For j As Integer = 0 To pX - 1
                    xValues(i, j) = xRows(i)(j)
                Next
            Next

            Dim zValues(,) As Double = Nothing
            If zSupplied Then
                ReDim zValues(yList.Count - 1, pZ - 1)
                For i As Integer = 0 To yList.Count - 1
                    For j As Integer = 0 To pZ - 1
                        zValues(i, j) = zRows(i)(j)
                    Next
                Next
            End If

            Dim finalVisit() As Double = Nothing
            If visitSupplied Then finalVisit = visitList.ToArray()

            imported = New LmmImportedInputs With {
                .Y = yList.ToArray(),
                .X = xValues,
                .Z = zValues,
                .Subject = subjectList.ToArray(),
                .Visit = finalVisit,
                .InferredXNames = inferredXNames,
                .InferredZNames = If(zSupplied, inferredZNames, New String() {}),
                .InferredXAbsoluteLetters = xLetters,
                .InferredZAbsoluteLetters = zLetters,
                .DroppedRows = dropped
            }
            Return True
        End Function

        Private Function TryBuildLmmExpandedDesign(raw(,) As Double,
                                                   rawNames() As String,
                                                   formulaText As String,
                                                   absoluteColumnLetters() As String,
                                                   allowRelativeColumnLetters As Boolean,
                                                   allowAbsoluteColumnLetters As Boolean,
                                                   allowQuotedVariableNames As Boolean,
                                                   role As String,
                                                   fallbackPrefix As String,
                                                   ByRef design(,) As Double,
                                                   ByRef designNames() As String,
                                                   ByRef errorMessage As String) As Boolean
            design = Nothing
            designNames = New String() {}
            errorMessage = Nothing

            If raw Is Nothing OrElse raw.GetLength(0) <= 0 OrElse raw.GetLength(1) <= 0 Then
                errorMessage = role & " requires a non-empty predictor matrix."
                Return False
            End If

            Dim p As Integer = raw.GetLength(1)
            Dim prefix As String = If(String.IsNullOrWhiteSpace(fallbackPrefix), "X", fallbackPrefix.Trim())
            Dim names() As String = NormalizeNameList(rawNames, p, prefix)

            If String.IsNullOrWhiteSpace(formulaText) Then
                design = raw
                designNames = names
                Return True
            End If

            Dim designBuild As RegressionFormulaMatrixBuildResult = Nothing
            Dim designErr As String = Nothing

            If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromFormula(rawX:=raw,
                                                                                            result:=designBuild,
                                                                                            errorMessage:=designErr,
                                                                                            predictorNames:=names,
                                                                                            formulaText:=formulaText,
                                                                                            absoluteColumnLetters:=absoluteColumnLetters,
                                                                                            allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                                                            allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                                                            allowQuotedVariableNames:=allowQuotedVariableNames,
                                                                                            omitCategoricalReference:=True) Then
                errorMessage = role & " could not be parsed or expanded: " & If(designErr, String.Empty)
                Return False
            End If

            If designBuild Is Nothing OrElse designBuild.ExpandedPredictorMatrix Is Nothing OrElse designBuild.ExpandedPredictorMatrix.GetLength(1) <= 0 Then
                errorMessage = role & " expansion did not produce any design columns."
                Return False
            End If

            design = designBuild.ExpandedPredictorMatrix
            designNames = If(designBuild.ExpandedPredictorNames, New String() {})
            If designNames.Length <> design.GetLength(1) Then designNames = regression.MixedModelFrontEndHelpers.DefaultNames(design.GetLength(1), prefix)
            Return True
        End Function

        Private Function ResolveImportedNames(varNames As Object,
                                              inferredNames() As String,
                                              expectedCount As Integer,
                                              fallbackPrefix As String) As String()
            If expectedCount <= 0 Then Return New String() {}

            Dim suppliedNames() As String = Nothing
            If Not ExcelArgPredicates.IsMissingArg(varNames) AndAlso
               Global.BESHStatNG.UdfDataImport.TryGetMmrmNameList(varNames, suppliedNames) AndAlso
               suppliedNames IsNot Nothing AndAlso suppliedNames.Length = expectedCount Then
                Return NormalizeNameList(suppliedNames, expectedCount, fallbackPrefix)
            End If

            Return NormalizeNameList(inferredNames, expectedCount, fallbackPrefix)
        End Function

        Private Function NormalizeNameList(inputNames() As String, expectedCount As Integer, fallbackPrefix As String) As String()
            If expectedCount <= 0 Then Return New String() {}
            Dim out(expectedCount - 1) As String
            For i As Integer = 0 To expectedCount - 1
                Dim nm As String = Nothing
                If inputNames IsNot Nothing AndAlso i < inputNames.Length Then nm = inputNames(i)
                nm = If(nm, String.Empty).Trim()
                If nm = String.Empty OrElse String.Equals(nm, "X" & (i + 1).ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) Then
                    nm = fallbackPrefix & (i + 1).ToString(CultureInfo.InvariantCulture)
                End If
                out(i) = nm
            Next
            Return out
        End Function

        Private Function MatrixColumnCount(mat(,) As Double) As Integer
            If mat Is Nothing Then Return 0
            Return mat.GetLength(1)
        End Function

        Private Function DefaultRandomCovarianceName(includeRandomIntercept As Boolean,
                                                     authoredRandomColumnCount As Integer,
                                                     totalRandomColumnCount As Integer) As String
            If includeRandomIntercept AndAlso authoredRandomColumnCount = 0 AndAlso totalRandomColumnCount = 1 Then
                Return "Random Intercept"
            End If

            Return "Variance Components (VC/Diag)"
        End Function

        Private Sub EnsureLmmResultUsesHandleNames(h As LmmHandle)
            If h Is Nothing OrElse h.Result Is Nothing Then Exit Sub
            If h.FixedEffectNames IsNot Nothing AndAlso h.FixedEffectNames.Length > 0 Then
                h.Result.FixedEffectNames = CType(h.FixedEffectNames.Clone(), String())
            End If
            If h.RandomEffectNames IsNot Nothing AndAlso h.RandomEffectNames.Length > 0 Then
                h.Result.RandomCovarianceLabels = CType(h.RandomEffectNames.Clone(), String())
            End If
        End Sub

    End Module

End Namespace
