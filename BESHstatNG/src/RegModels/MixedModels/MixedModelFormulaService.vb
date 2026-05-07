Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' Stores the artifacts produced while converting worksheet-style data and formula text into a
    ''' <see cref="MixedModelFitRequest"/> for the shared LMM/MMRM engine.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The mixed-model engine itself is deliberately matrix based.  This class captures the bridge
    ''' artifacts produced by <see cref="MixedModelFormulaService"/> so that the future UI, UDF layer,
    ''' and unit tests can inspect how the original user-facing formula was expanded.
    ''' </para>
    ''' <para>
    ''' The fixed-effects matrix <c>X</c> and the random-effects matrix <c>Z</c> are built separately.
    ''' Both re-use the existing <c>RegressionFormulaDesignService</c> / <c>RegressionFormulaParser</c>
    ''' stack for main effects, polynomial effects, categorical effects, and supported interactions.
    ''' The mixed-model-specific service only adds the pieces that are not part of ordinary regression
    ''' formula expansion: separate response-vector validation plus subject/visit column extraction, intercept handling, and request
    ''' construction for LMM versus MMRM.
    ''' </para>
    ''' </remarks>
    Public Class MixedModelFormulaBuildResult
        ''' <summary>Prepared request ready for <see cref="MixedModelEngine"/>, <see cref="LMM"/>, or <see cref="MMRM"/>.</summary>
        Public Property Request As MixedModelFitRequest = Nothing

        ''' <summary>Blocked subject data generated from the original rows.</summary>
        Public Property BlockData As MixedModelBlockData = Nothing

        ''' <summary>Expanded fixed-effects design information returned by the existing regression formula service.</summary>
        Public Property FixedDesignResult As RegressionFormulaMatrixBuildResult = Nothing

        ''' <summary>Expanded random-effects design information returned by the existing regression formula service, when non-intercept random terms were requested.</summary>
        Public Property RandomDesignResult As RegressionFormulaMatrixBuildResult = Nothing

        ''' <summary>Original fixed-effects formula supplied by the caller.</summary>
        Public Property FixedFormulaText As String = String.Empty

        ''' <summary>Parser-ready fixed-effects formula after intercept-token removal, and <c>*</c> expansion.</summary>
        Public Property FixedParserFormulaText As String = String.Empty

        ''' <summary>Original random-effects formula supplied by the caller.</summary>
        Public Property RandomFormulaText As String = String.Empty

        ''' <summary>Parser-ready random-effects formula after optional <c>(... | subject)</c> stripping, intercept-token removal, and <c>*</c> expansion.</summary>
        Public Property RandomParserFormulaText As String = String.Empty

        ''' <summary>Names of fixed-effects columns in the final <c>X</c> matrix.</summary>
        Public Property FixedEffectNames As String() = Array.Empty(Of String)()

        ''' <summary>Names of random-effects columns in the final <c>Z</c> matrix.</summary>
        Public Property RandomEffectNames As String() = Array.Empty(Of String)()

        ''' <summary>Response vector used by the model.</summary>
        Public Property Response As Double() = Array.Empty(Of Double)()

        ''' <summary>Subject identifiers used for block construction.</summary>
        Public Property SubjectId As Object() = Array.Empty(Of Object)()

        ''' <summary>Visit/time values used for sorting and R-side covariance indexing, or <c>Nothing</c>.</summary>
        Public Property Visit As Double() = Nothing

        ''' <summary>Diagnostic trace accumulated by the formula service.</summary>
        Public Property strTrace As String = String.Empty
    End Class

    ''' <summary>
    ''' Formula/data bridge for the Gaussian mixed-model engine.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module is intentionally a bridge layer, not a second formula parser.  The existing
    ''' <c>RegressionFormulaDesignService</c> and <c>RegressionFormulaParser</c> are re-used for the
    ''' actual design-matrix expansion.  That means mixed models inherit the same semantics currently
    ''' used by LM/GLM/GEE formula workflows: quoted variable names, relative column letters, polynomial
    ''' terms, supported interactions, and <c>factor(...)</c> main effects.
    ''' </para>
    ''' <para>
    ''' The mixed-model-specific additions are:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>validation of a response vector supplied separately from the predictor catalog,</description></item>
    ''' <item><description>automatic fixed-effects intercept handling, because the existing formula service returns predictor columns but does not prepend an intercept,</description></item>
    ''' <item><description>simple random-effects formula handling for <c>1</c>, <c>1 + time</c>, <c>0 + time</c>, and optional lme4-like <c>(1 + time | subject)</c> text,</description></item>
    ''' <item><description>optional expansion of simple <c>*</c> shorthand into main effects and colon interactions before delegating to the existing parser,</description></item>
    ''' <item><description>construction of either an LMM request, <c>V_i = Z_i G Z_i' + R_i</c>, or an MMRM request, <c>V_i = R_i</c>.</description></item>
    ''' </list>
    ''' <para>
    ''' The first MMRM use case should call this service with a separate response vector, no random-effects structure, a subject
    ''' column, a visit column, and an R-side covariance such as unstructured, heterogeneous compound
    ''' symmetry, or heterogeneous AR(1).  The same <see cref="MixedModelBlockData"/> object is then
    ''' consumed by the shared likelihood engine.
    ''' </para>
    ''' </remarks>
    Public Module MixedModelFormulaService

        Private Const InterceptName As String = "Intercept"

        Private Class PredictorSource
            Public Matrix As Double(,) = Nothing
            Public Names As String() = Array.Empty(Of String)()
            Public BaseKeys As String() = Array.Empty(Of String)()
            Public OriginalColumnIndices As Integer() = Array.Empty(Of Integer)()
        End Class

        Private Class PreparedFormula
            Public OriginalText As String = String.Empty
            Public ParserText As String = String.Empty
            Public IncludeIntercept As Boolean = True
            Public IsDefaultMainEffects As Boolean = False
            Public HasPredictorTerms As Boolean = False
            Public Notes As String = String.Empty
        End Class

        ''' <summary>
        ''' Builds a <see cref="MixedModelFitRequest"/> from a <see cref="DataObj"/>, a separate response vector,
        ''' and right-hand-side-only formula settings, throwing an exception when construction fails.
        ''' </summary>
        ''' <param name="raw">Cleaned data object containing subject, optional visit, and predictor columns.</param>
        ''' <param name="response">Response vector aligned with the cleaned rows of <paramref name="raw"/>.</param>
        ''' <param name="fixedFormulaText">Right-hand-side fixed-effects formula only, for example <c>trt * visit + baseline</c>. Do not include <c>y ~</c>.</param>
        ''' <param name="subjectKey">Subject/cluster identifier column in <paramref name="raw"/>.</param>
        ''' <param name="responseName">Optional user-facing response name used only for labels/results.</param>
        ''' <param name="randomFormulaText">Random-effects formula. Examples: <c>1</c>, <c>1 + visit</c>, <c>0 + visit</c>, or <c>(1 + visit | subject)</c>. Blank defaults to random intercept when a non-degenerate G structure is requested.</param>
        ''' <param name="visitKey">Optional visit/time column used for sorting and visit-indexed R-side covariance structures. The visit column remains available to formulas.</param>
        ''' <param name="fitMethod">ML or REML.</param>
        ''' <param name="residualStructType">R-side covariance structure name.</param>
        ''' <param name="randomStructType">G-side covariance structure name. Use <c>None</c> for MMRM/R-side-only fits.</param>
        ''' <param name="includeFixedInterceptDefault">Default fixed-effects intercept setting when no explicit <c>0</c> or <c>-1</c> token is present.</param>
        ''' <returns>A prepared mixed-model fit request.</returns>
        Public Function BuildRequestFromFormula(raw As DataObj,
                                                response() As Double,
                                                fixedFormulaText As String,
                                                subjectKey As String,
                                                Optional responseName As String = "y",
                                                Optional randomFormulaText As String = Nothing,
                                                Optional visitKey As String = Nothing,
                                                Optional fitMethod As MixedModelFitMethod = MixedModelFitMethod.REML,
                                                Optional residualStructType As String = "Identity",
                                                Optional randomStructType As String = "Random Intercept",
                                                Optional includeFixedInterceptDefault As Boolean = True) As MixedModelFitRequest
            Dim build As MixedModelFormulaBuildResult = Nothing
            Dim err As String = Nothing
            If Not TryBuildRequestFromFormula(raw:=raw,
                                              response:=response,
                                              fixedFormulaText:=fixedFormulaText,
                                              subjectKey:=subjectKey,
                                              result:=build,
                                              errorMessage:=err,
                                              responseName:=responseName,
                                              randomFormulaText:=randomFormulaText,
                                              visitKey:=visitKey,
                                              fitMethod:=fitMethod,
                                              residualStructType:=residualStructType,
                                              randomStructType:=randomStructType,
                                              includeFixedInterceptDefault:=includeFixedInterceptDefault) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException(err))
            End If
            Return build.Request
        End Function

        ''' <summary>
        ''' Attempts to build a mixed-model request from a <see cref="DataObj"/>, a separate response vector,
        ''' and right-hand-side-only formula settings.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This is the preferred entry point for future UI/UDF layers because it returns the intermediate
        ''' build result.  The response is intentionally supplied separately from the predictor data so that
        ''' UDFs can expose <c>y</c> as its own argument and fixed/random formulas can remain RHS-only.
        ''' </para>
        ''' <para>
        ''' If a top-level <c>~</c> is present, this method fails with a clear message.  This avoids ambiguity
        ''' between a separate response argument and a formula-embedded response.
        ''' </para>
        ''' </remarks>
        Public Function TryBuildRequestFromFormula(raw As DataObj,
                                                   response() As Double,
                                                   fixedFormulaText As String,
                                                   subjectKey As String,
                                                   ByRef result As MixedModelFormulaBuildResult,
                                                   ByRef errorMessage As String,
                                                   Optional responseName As String = "y",
                                                   Optional randomFormulaText As String = Nothing,
                                                   Optional visitKey As String = Nothing,
                                                   Optional fitMethod As MixedModelFitMethod = MixedModelFitMethod.REML,
                                                   Optional residualStructType As String = "Identity",
                                                   Optional randomStructType As String = "Random Intercept",
                                                   Optional includeFixedInterceptDefault As Boolean = True,
                                                   Optional sortWithinSubjectByVisit As Boolean = True) As Boolean
            result = Nothing
            errorMessage = Nothing
            Dim strTrace As String = String.Empty

            Try
                AppendInfo(strTrace, "MixedModelFormulaService.TryBuildRequestFromFormula start. responseName='" & If(responseName, String.Empty) & "'; subject='" & If(subjectKey, String.Empty) & "'; visit='" & If(visitKey, String.Empty) & "'; fixedFormula='" & If(fixedFormulaText, String.Empty) & "'; randomFormula='" & If(randomFormulaText, String.Empty) & "'.")

                ValidateRawDataObj(raw)
                Dim y() As Double = ValidateAndCopyResponse(response, raw.nRows, If(responseName, "response"))

                Dim subjectCol As Integer = ResolveColumnIndex(raw.varNames, subjectKey, "subject")
                Dim visitCol As Integer = -1
                If Not String.IsNullOrWhiteSpace(visitKey) Then
                    visitCol = ResolveColumnIndex(raw.varNames, visitKey, "visit")
                End If

                Dim predictorSource As PredictorSource = BuildPredictorSource(raw, subjectCol, strTrace)
                Dim subjectId() As Object = ExtractObjectColumn(raw, subjectCol)
                Dim visit() As Double = Nothing
                If visitCol >= 0 Then visit = ExtractNumericColumn(raw, visitCol, "visit")

                Dim fixedPrep As PreparedFormula = PrepareFixedFormula(fixedFormulaText, includeFixedInterceptDefault, strTrace)
                fixedPrep.ParserText = RewriteBareVariableNamesForParser(fixedPrep.ParserText, predictorSource, strTrace)

                Dim fixedDesign As RegressionFormulaMatrixBuildResult = Nothing
                Dim x(,) As Double = BuildDesignMatrixFromPreparedFormula(predictorSource:=predictorSource,
                                                                           prepared:=fixedPrep,
                                                                           includeIntercept:=fixedPrep.IncludeIntercept,
                                                                           result:=fixedDesign,
                                                                           traceContext:="fixed",
                                                                           strTrace:=strTrace)
                Dim fixedNames() As String = BuildDesignNames(fixedPrep.IncludeIntercept,
                                                              If(fixedDesign Is Nothing, Array.Empty(Of String)(), fixedDesign.ExpandedPredictorNames))
                If x Is Nothing OrElse x.GetLength(1) < 1 Then
                    Throw New ApplicationException("The fixed-effects design matrix contains no columns. Include an intercept or at least one fixed-effect term.")
                End If

                Dim gStruct As MixedModelGStruct = MixedModelGStructUtils.createMixedModelGStruct(randomStructType)
                Dim rStruct As MixedModelRStruct = MixedModelRStructUtils.createMixedModelRStruct(residualStructType)

                Dim randomPrep As PreparedFormula = PrepareRandomFormula(randomFormulaText, gStruct, strTrace)
                randomPrep.ParserText = RewriteBareVariableNamesForParser(randomPrep.ParserText, predictorSource, strTrace)

                Dim randomDesign As RegressionFormulaMatrixBuildResult = Nothing
                Dim z(,) As Double = Nothing
                Dim randomNames() As String = Array.Empty(Of String)()

                If gStruct IsNot Nothing AndAlso Not gStruct.IsDegenerateZeroG() Then
                    z = BuildDesignMatrixFromPreparedFormula(predictorSource:=predictorSource,
                                                             prepared:=randomPrep,
                                                             includeIntercept:=randomPrep.IncludeIntercept,
                                                             result:=randomDesign,
                                                             traceContext:="random",
                                                             strTrace:=strTrace)
                    randomNames = BuildDesignNames(randomPrep.IncludeIntercept,
                                                   If(randomDesign Is Nothing, Array.Empty(Of String)(), randomDesign.ExpandedPredictorNames))
                    If z Is Nothing OrElse z.GetLength(1) < 1 Then
                        Throw New ApplicationException("A non-degenerate random-effects covariance structure was requested, but the random-effects formula produced no Z columns.")
                    End If
                Else
                    AppendTrace(strTrace, "Random structure is degenerate/None; no Z matrix will be constructed. Request will use the MMRM/R-side-only path.")
                    z = Nothing
                    randomNames = Array.Empty(Of String)()
                    randomDesign = Nothing
                    gStruct = Nothing
                End If

                Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                                       x:=x,
                                                                                       subjectId:=subjectId,
                                                                                       z:=z,
                                                                                       visit:=visit,
                                                                                       sortWithinSubjectByVisit:=sortWithinSubjectByVisit,
                                                                                       rowNumbers:=raw.RowIds)

                Dim req As MixedModelFitRequest
                If z Is Nothing Then
                    req = MixedModelFitRequest.CreateMMRM(blockData, rStruct, fitMethod)
                Else
                    req = MixedModelFitRequest.CreateLMM(blockData, rStruct, gStruct, fitMethod)
                End If

                req.ResponseVarName = If(String.IsNullOrWhiteSpace(responseName), "y", responseName.Trim())
                req.SubjectVarName = SafeColumnName(raw.varNames, subjectCol)
                req.VisitVarName = If(visitCol >= 0, SafeColumnName(raw.varNames, visitCol), String.Empty)
                req.FixedFormulaText = If(fixedFormulaText, String.Empty)
                req.RandomFormulaText = If(randomPrep.ParserText, String.Empty)
                req.FixedEffectNames = fixedNames
                req.RandomEffectNames = randomNames
                req.RequestLabel = BuildRequestLabel(req.ResponseVarName, req.SubjectVarName, req.VisitVarName, z Is Nothing)
                req.strTrace = MergeTrace(req.strTrace, strTrace)
                req.Validate()

                result = New MixedModelFormulaBuildResult With {
                    .Request = req,
                    .BlockData = blockData,
                    .FixedDesignResult = fixedDesign,
                    .RandomDesignResult = randomDesign,
                    .FixedFormulaText = If(fixedFormulaText, String.Empty),
                    .FixedParserFormulaText = fixedPrep.ParserText,
                    .RandomFormulaText = If(randomFormulaText, String.Empty),
                    .RandomParserFormulaText = randomPrep.ParserText,
                    .FixedEffectNames = fixedNames,
                    .RandomEffectNames = randomNames,
                    .Response = y,
                    .SubjectId = subjectId,
                    .Visit = visit,
                    .strTrace = MergeTrace(strTrace, req.strTrace)
                }

                AppendInfo(result.strTrace, "MixedModelFormulaService.TryBuildRequestFromFormula success. " & req.Describe())
                Return True

            Catch ex As Exception
                AppGlobals.BSlogg.Error(ex, "MixedModelFormulaService.TryBuildRequestFromFormula failed.")
                errorMessage = ex.Message
                result = Nothing
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Convenience helper that builds a request from an in-memory predictor matrix, a separate response
        ''' vector, and variable-name array.
        ''' </summary>
        ''' <remarks>
        ''' This helper is useful for unit tests because it avoids Excel interop.  The supplied predictor matrix
        ''' is loaded into a temporary <see cref="DataObj"/> using <c>DataImportRawMatrix</c>, then the normal
        ''' RHS-only formula-service path is used.
        ''' </remarks>
        Public Function BuildRequestFromRawMatrix(rawInput(,) As Object,
                                                  variableNames() As String,
                                                  response() As Double,
                                                  fixedFormulaText As String,
                                                  subjectKey As String,
                                                  Optional responseName As String = "y",
                                                  Optional randomFormulaText As String = Nothing,
                                                  Optional visitKey As String = Nothing,
                                                  Optional fitMethod As MixedModelFitMethod = MixedModelFitMethod.REML,
                                                  Optional residualStructType As String = "Identity",
                                                  Optional randomStructType As String = "Random Intercept") As MixedModelFitRequest
            Dim d As New DataObj()
            d.DataImportRawMatrix(rawInput, variableNames, CharCols:=-1)
            Return BuildRequestFromFormula(raw:=d,
                                           response:=response,
                                           fixedFormulaText:=fixedFormulaText,
                                           subjectKey:=subjectKey,
                                           responseName:=responseName,
                                           randomFormulaText:=randomFormulaText,
                                           visitKey:=visitKey,
                                           fitMethod:=fitMethod,
                                           residualStructType:=residualStructType,
                                           randomStructType:=randomStructType)
        End Function

        Private Function BuildDesignMatrixFromPreparedFormula(predictorSource As PredictorSource,
                                                              prepared As PreparedFormula,
                                                              includeIntercept As Boolean,
                                                              ByRef result As RegressionFormulaMatrixBuildResult,
                                                              traceContext As String,
                                                              ByRef strTrace As String) As Double(,)
            result = Nothing
            If predictorSource Is Nothing Then Throw New ArgumentNullException(NameOf(predictorSource))
            If prepared Is Nothing Then Throw New ArgumentNullException(NameOf(prepared))

            Dim n As Integer = If(predictorSource.Matrix Is Nothing, 0, predictorSource.Matrix.GetLength(0))
            If n <= 0 Then Throw New ApplicationException("Cannot build a design matrix because the predictor source has no rows.")

            Dim expanded(,) As Double = Nothing
            Dim expandedNames() As String = Array.Empty(Of String)()

            If prepared.IsDefaultMainEffects Then
                If predictorSource.Names.Length = 0 Then
                    AppendWarn(strTrace, traceContext & " formula requested default main effects, but no eligible predictor columns are available. Only the intercept will be used if requested.")
                    expanded = CreateZeroColumnMatrix(n)
                    expandedNames = Array.Empty(Of String)()
                Else
                    Dim err As String = Nothing
                    If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromFormula(rawX:=predictorSource.Matrix,
                                                                                                      result:=result,
                                                                                                      errorMessage:=err,
                                                                                                      predictorNames:=predictorSource.Names,
                                                                                                      formulaText:=Nothing,
                                                                                                      baseKeys:=predictorSource.BaseKeys,
                                                                                                      allowRelativeColumnLetters:=True,
                                                                                                      allowAbsoluteColumnLetters:=False,
                                                                                                      allowQuotedVariableNames:=True,
                                                                                                      omitCategoricalReference:=True) Then
                        Throw New ApplicationException("Failed to build " & traceContext & " default main-effects design matrix. " & err)
                    End If
                    expanded = result.ExpandedPredictorMatrix
                    expandedNames = If(result.ExpandedPredictorNames, Array.Empty(Of String)())
                End If
            ElseIf prepared.HasPredictorTerms Then
                Dim err As String = Nothing
                If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromFormula(rawX:=predictorSource.Matrix,
                                                                                                  result:=result,
                                                                                                  errorMessage:=err,
                                                                                                  predictorNames:=predictorSource.Names,
                                                                                                  formulaText:=prepared.ParserText,
                                                                                                  baseKeys:=predictorSource.BaseKeys,
                                                                                                  allowRelativeColumnLetters:=True,
                                                                                                  allowAbsoluteColumnLetters:=False,
                                                                                                  allowQuotedVariableNames:=True,
                                                                                                  omitCategoricalReference:=True) Then
                    Throw New ApplicationException("Failed to build " & traceContext & " design matrix from formula '" & prepared.ParserText & "'. " & err)
                End If
                expanded = result.ExpandedPredictorMatrix
                expandedNames = If(result.ExpandedPredictorNames, Array.Empty(Of String)())
            Else
                expanded = CreateZeroColumnMatrix(n)
                expandedNames = Array.Empty(Of String)()
            End If

            Dim out(,) As Double = AddInterceptIfRequested(expanded, n, includeIntercept)
            AppendTrace(strTrace, "Built " & traceContext & " design. includeIntercept=" & includeIntercept.ToString() & "; predictorTerms=" & expandedNames.Length.ToString(CultureInfo.InvariantCulture) & "; totalColumns=" & out.GetLength(1).ToString(CultureInfo.InvariantCulture) & "; parserFormula='" & If(prepared.ParserText, String.Empty) & "'.")
            Return out
        End Function

        Private Function PrepareFixedFormula(formulaText As String,
                                             includeInterceptDefault As Boolean,
                                             ByRef strTrace As String) As PreparedFormula
            Dim s As String = EnsureRightHandSideOnlyFormula(formulaText, "fixed-effects")
            Dim prep As PreparedFormula = PrepareFormulaCore(s, includeInterceptDefault, defaultBlankMeansMainEffects:=True, strTrace:=strTrace)
            prep.OriginalText = If(formulaText, String.Empty)
            Return prep
        End Function

        Private Function PrepareRandomFormula(formulaText As String,
                                              gStruct As MixedModelGStruct,
                                              ByRef strTrace As String) As PreparedFormula
            If gStruct Is Nothing OrElse gStruct.IsDegenerateZeroG() Then
                Return New PreparedFormula With {
                    .OriginalText = If(formulaText, String.Empty),
                    .ParserText = String.Empty,
                    .IncludeIntercept = False,
                    .IsDefaultMainEffects = False,
                    .HasPredictorTerms = False
                }
            End If

            Dim s As String = ExtractRandomFormulaLeftSide(formulaText, strTrace)
            If String.IsNullOrWhiteSpace(s) Then s = "1"
            Dim prep As PreparedFormula = PrepareFormulaCore(s, includeInterceptDefault:=True, defaultBlankMeansMainEffects:=False, strTrace:=strTrace)
            prep.OriginalText = If(formulaText, String.Empty)
            Return prep
        End Function

        Private Function PrepareFormulaCore(formulaText As String,
                                            includeInterceptDefault As Boolean,
                                            defaultBlankMeansMainEffects As Boolean,
                                            ByRef strTrace As String) As PreparedFormula
            Dim out As New PreparedFormula()
            Dim s As String = If(formulaText, String.Empty).Trim()
            out.OriginalText = s
            out.IncludeIntercept = includeInterceptDefault

            If s = String.Empty Then
                out.IsDefaultMainEffects = defaultBlankMeansMainEffects
                out.HasPredictorTerms = defaultBlankMeansMainEffects
                out.ParserText = String.Empty
                Return out
            End If

            s = ExpandStarShorthand(s, strTrace)

            Dim terms As List(Of String) = SplitTopLevel(s, "+"c)
            Dim kept As New List(Of String)()

            For Each term As String In terms
                Dim t As String = If(term, String.Empty).Trim()
                If t = String.Empty Then Continue For

                If t = "1" Then
                    out.IncludeIntercept = True
                    Continue For
                End If

                If t = "0" OrElse t = "-1" Then
                    out.IncludeIntercept = False
                    Continue For
                End If

                If t.StartsWith("-", StringComparison.Ordinal) Then
                    Dim minusTerm As String = t.Substring(1).Trim()
                    If minusTerm = "1" OrElse minusTerm = "0" Then
                        out.IncludeIntercept = False
                        Continue For
                    End If
                    Throw New ApplicationException("Formula term subtraction is not supported yet except for '-1' to remove the intercept. Unsupported term: '" & t & "'.")
                End If

                kept.Add(t)
            Next

            out.ParserText = String.Join(" + ", kept)
            out.HasPredictorTerms = kept.Count > 0
            out.IsDefaultMainEffects = False
            Return out
        End Function

        ''' <summary>
        ''' Rewrites user-friendly bare variable names into the single-quoted variable-name syntax
        ''' expected by <see cref="RegressionFormulaParser"/>.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' The shared regression parser treats bare tokens such as <c>A</c> and <c>B</c> as relative
        ''' predictor-column letters. Mixed-model UDF/UI formulas are friendlier when simple column
        ''' names such as <c>visit</c> can be typed directly. This helper rewrites bare identifiers
        ''' that match the predictor catalog into quoted variable-name references before delegation to
        ''' the shared parser; for example, <c>visit</c> becomes <c>'visit'</c>.
        ''' </para>
        ''' <para>
        ''' The rewrite is conservative: existing quoted names are preserved, function names such as
        ''' <c>factor(...)</c> and <c>poly(...)</c> are preserved, and valid relative column-letter
        ''' references for the current predictor source remain column-letter references. Users can
        ''' always force variable-name semantics with explicit single quotes.
        ''' </para>
        ''' </remarks>
        Private Function RewriteBareVariableNamesForParser(formulaText As String,
                                                           predictorSource As PredictorSource,
                                                           ByRef strTrace As String) As String
            Dim s As String = If(formulaText, String.Empty)
            If String.IsNullOrWhiteSpace(s) OrElse predictorSource Is Nothing OrElse predictorSource.Names Is Nothing Then Return s

            Dim tokenToDisplayName As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            AddBareVariableCandidates(tokenToDisplayName, predictorSource.Names)
            AddBareVariableCandidates(tokenToDisplayName, predictorSource.BaseKeys)

            If tokenToDisplayName.Count = 0 Then Return s

            Dim sb As New StringBuilder()
            Dim changed As Boolean = False
            Dim i As Integer = 0

            While i < s.Length
                Dim ch As Char = s.Chars(i)

                If ch = "'"c Then
                    Dim j As Integer = i + 1
                    sb.Append(ch)
                    While j < s.Length
                        sb.Append(s.Chars(j))
                        If s.Chars(j) = "'"c Then Exit While
                        j += 1
                    End While
                    i = j + 1
                    Continue While
                End If

                If IsBareIdentifierStart(ch) Then
                    Dim j As Integer = i + 1
                    While j < s.Length AndAlso IsBareIdentifierPart(s.Chars(j))
                        j += 1
                    End While

                    Dim token As String = s.Substring(i, j - i)
                    Dim nextNonSpace As Char = NextNonSpaceChar(s, j)
                    Dim displayName As String = Nothing

                    If tokenToDisplayName.TryGetValue(token, displayName) AndAlso
                       nextNonSpace <> "("c AndAlso
                       Not IsValidRelativeColumnLetter(token, predictorSource.Matrix) Then

                        sb.Append("'").Append(EscapeSingleQuotedVariableName(displayName)).Append("'")
                        changed = True
                    Else
                        sb.Append(token)
                    End If

                    i = j
                    Continue While
                End If

                sb.Append(ch)
                i += 1
            End While

            Dim out As String = sb.ToString()
            If changed Then AppendTrace(strTrace, "Rewrote bare mixed-model variable names for the shared regression parser. original='" & s & "'; parser='" & out & "'.")
            Return out
        End Function

        Private Sub AddBareVariableCandidates(target As Dictionary(Of String, String), values() As String)
            If target Is Nothing OrElse values Is Nothing Then Return

            For Each rawName As String In values
                Dim name As String = If(rawName, String.Empty).Trim()
                If name = String.Empty Then Continue For
                If name.Contains("'"c) Then Continue For
                If Not IsSimpleBareIdentifier(name) Then Continue For

                If Not target.ContainsKey(name) Then target.Add(name, name)
            Next
        End Sub

        Private Function IsSimpleBareIdentifier(text As String) As Boolean
            If String.IsNullOrWhiteSpace(text) Then Return False
            Dim s As String = text.Trim()
            If Not IsBareIdentifierStart(s.Chars(0)) Then Return False
            For i As Integer = 1 To s.Length - 1
                If Not IsBareIdentifierPart(s.Chars(i)) Then Return False
            Next
            Return True
        End Function

        Private Function IsBareIdentifierStart(ch As Char) As Boolean
            Return Char.IsLetter(ch) OrElse ch = "_"c
        End Function

        Private Function IsBareIdentifierPart(ch As Char) As Boolean
            Return Char.IsLetterOrDigit(ch) OrElse ch = "_"c
        End Function

        Private Function NextNonSpaceChar(text As String, startIndex As Integer) As Char
            If text Is Nothing Then Return ChrW(0)
            For i As Integer = Math.Max(0, startIndex) To text.Length - 1
                If Not Char.IsWhiteSpace(text.Chars(i)) Then Return text.Chars(i)
            Next
            Return ChrW(0)
        End Function

        Private Function IsValidRelativeColumnLetter(token As String, predictorMatrix(,) As Double) As Boolean
            If predictorMatrix Is Nothing OrElse String.IsNullOrWhiteSpace(token) Then Return False
            Dim idx As Integer = TryResolveColumnLetter(token.Trim())
            Return idx >= 0 AndAlso idx < predictorMatrix.GetLength(1)
        End Function

        Private Function EscapeSingleQuotedVariableName(name As String) As String
            Return If(name, String.Empty).Replace("'", "''")
        End Function

        Private Function BuildPredictorSource(raw As DataObj,
                                              subjectCol As Integer,
                                              ByRef strTrace As String) As PredictorSource
            Dim includeCols As New List(Of Integer)()
            For j As Integer = 0 To raw.nCols - 1
                If j = subjectCol Then Continue For
                includeCols.Add(j)
            Next

            Dim n As Integer = raw.nRows
            Dim p As Integer = includeCols.Count
            Dim mat(,) As Double
            If p = 0 Then
                mat = CreateZeroColumnMatrix(n)
            Else
                ReDim mat(n - 1, p - 1)
                For jj As Integer = 0 To p - 1
                    Dim srcCol As Integer = includeCols(jj)
                    For i As Integer = 0 To n - 1
                        mat(i, jj) = ToDoubleStrict(raw.FinalData(i, srcCol), SafeColumnName(raw.varNames, srcCol))
                    Next
                Next
            End If

            Dim names As String() = includeCols.Select(Function(ix) SafeColumnName(raw.varNames, ix)).ToArray()
            Dim baseKeys As String() = MakeUniqueBaseKeys(names)

            AppendTrace(strTrace, "Predictor source built. subjectCol=" & subjectCol.ToString(CultureInfo.InvariantCulture) & "; predictorCols=" & p.ToString(CultureInfo.InvariantCulture) & "; names='" & String.Join(", ", names) & "'.")

            Return New PredictorSource With {
                .Matrix = mat,
                .Names = names,
                .BaseKeys = baseKeys,
                .OriginalColumnIndices = includeCols.ToArray()
            }
        End Function

        Private Function ExtractNumericColumn(raw As DataObj, colIndex As Integer, role As String) As Double()
            Dim out(raw.nRows - 1) As Double
            Dim colName As String = SafeColumnName(raw.varNames, colIndex)
            For i As Integer = 0 To raw.nRows - 1
                out(i) = ToDoubleStrict(raw.FinalData(i, colIndex), role & " column '" & colName & "'")
            Next
            Return out
        End Function

        Private Function ExtractObjectColumn(raw As DataObj, colIndex As Integer) As Object()
            Dim out(raw.nRows - 1) As Object
            For i As Integer = 0 To raw.nRows - 1
                Dim v As Object = raw.FinalData(i, colIndex)
                If v Is Nothing OrElse Convert.IsDBNull(v) Then
                    Throw New ApplicationException("Subject column contains a missing value at cleaned row " & (i + 1).ToString(CultureInfo.InvariantCulture) & ".")
                End If
                out(i) = v
            Next
            Return out
        End Function

        Private Function CreateZeroColumnMatrix(nRows As Integer) As Double(,)
            If nRows < 0 Then Throw New ArgumentOutOfRangeException(NameOf(nRows))
            Return DirectCast(Array.CreateInstance(GetType(Double), nRows, 0), Double(,))
        End Function

        Private Function AddInterceptIfRequested(expanded(,) As Double,
                                                 nRows As Integer,
                                                 includeIntercept As Boolean) As Double(,)
            Dim pExpanded As Integer = If(expanded Is Nothing, 0, Math.Max(0, expanded.GetLength(1)))
            Dim totalCols As Integer = pExpanded + If(includeIntercept, 1, 0)
            If totalCols <= 0 Then
                Return CreateZeroColumnMatrix(nRows)
            End If

            Dim out(nRows - 1, totalCols - 1) As Double
            Dim offset As Integer = 0
            If includeIntercept Then
                For i As Integer = 0 To nRows - 1
                    out(i, 0) = 1.0
                Next
                offset = 1
            End If

            If pExpanded > 0 Then
                For j As Integer = 0 To pExpanded - 1
                    For i As Integer = 0 To nRows - 1
                        out(i, j + offset) = expanded(i, j)
                    Next
                Next
            End If

            Return out
        End Function

        Private Function BuildDesignNames(includeIntercept As Boolean, expandedNames() As String) As String()
            Dim names As New List(Of String)()
            If includeIntercept Then names.Add(InterceptName)
            If expandedNames IsNot Nothing Then
                For Each nm As String In expandedNames
                    names.Add(If(nm, String.Empty))
                Next
            End If
            Return names.ToArray()
        End Function

        Private Function EnsureRightHandSideOnlyFormula(formulaText As String,
                                                        formulaRole As String) As String
            Dim s As String = If(formulaText, String.Empty).Trim()
            If s = String.Empty Then Return String.Empty

            Dim parts As List(Of String) = SplitTopLevel(s, "~"c)
            If parts.Count > 1 Then
                Throw New ApplicationException("Mixed-model " & formulaRole & " formulas must be right-hand-side only because the response is supplied as a separate argument. Use '" & parts(parts.Count - 1).Trim() & "' instead of '" & s & "'.")
            End If

            Return s
        End Function

        Private Function ExtractRandomFormulaLeftSide(formulaText As String,
                                                      ByRef strTrace As String) As String
            Dim s As String = If(formulaText, String.Empty).Trim()
            If s = String.Empty Then Return String.Empty

            If s.StartsWith("(", StringComparison.Ordinal) AndAlso s.EndsWith(")", StringComparison.Ordinal) AndAlso s.Contains("|") Then
                Dim inner As String = s.Substring(1, s.Length - 2)
                Dim parts As List(Of String) = SplitTopLevel(inner, "|"c)
                If parts.Count >= 1 Then
                    AppendTrace(strTrace, "Random formula lme4-style text detected. Using left side '" & parts(0).Trim() & "' and ignoring grouping right side; subjectKey controls grouping in BESHStatNG.")
                    Return parts(0).Trim()
                End If
            End If

            If s.Contains("|") Then
                Dim parts As List(Of String) = SplitTopLevel(s, "|"c)
                If parts.Count >= 1 Then
                    AppendTrace(strTrace, "Random formula contains '|'. Using left side '" & parts(0).Trim() & "' and ignoring grouping right side; subjectKey controls grouping in BESHStatNG.")
                    Return parts(0).Trim().Trim("("c, ")"c)
                End If
            End If

            Return s
        End Function

        Private Function ExpandStarShorthand(formulaText As String,
                                             ByRef strTrace As String) As String
            Dim s As String = If(formulaText, String.Empty).Trim()
            If s = String.Empty OrElse Not s.Contains("*") Then Return s

            Dim additiveParts As List(Of String) = SplitTopLevel(s, "+"c)
            Dim outParts As New List(Of String)()
            Dim changed As Boolean = False

            For Each part As String In additiveParts
                Dim t As String = If(part, String.Empty).Trim()
                If t = String.Empty Then Continue For

                Dim starParts As List(Of String) = SplitTopLevel(t, "*"c)
                If starParts.Count <= 1 Then
                    outParts.Add(t)
                Else
                    changed = True
                    Dim clean As List(Of String) = starParts.Select(Function(x) x.Trim()).Where(Function(x) x <> String.Empty).ToList()
                    If clean.Count < 2 Then Throw New ApplicationException("Invalid '*' formula term: '" & t & "'.")
                    Dim expanded As List(Of String) = ExpandStarParts(clean)
                    outParts.AddRange(expanded)
                End If
            Next

            Dim out As String = String.Join(" + ", outParts)
            If changed Then AppendTrace(strTrace, "Expanded '*' shorthand. original='" & s & "'; expanded='" & out & "'.")
            Return out
        End Function

        Private Function ExpandStarParts(parts As List(Of String)) As List(Of String)
            Dim out As New List(Of String)()
            Dim n As Integer = parts.Count
            Dim maxMask As Integer = CInt(Math.Pow(2, n)) - 1

            For mask As Integer = 1 To maxMask
                Dim combo As New List(Of String)()
                For bit As Integer = 0 To n - 1
                    If (mask And (1 << bit)) <> 0 Then combo.Add(parts(bit))
                Next
                out.Add(String.Join(":", combo))
            Next

            Return out
        End Function

        Private Function SplitTopLevel(text As String, delimiter As Char) As List(Of String)
            Dim out As New List(Of String)()
            Dim sb As New StringBuilder()
            Dim depth As Integer = 0
            Dim inQuote As Boolean = False
            Dim s As String = If(text, String.Empty)

            For i As Integer = 0 To s.Length - 1
                Dim ch As Char = s.Chars(i)
                If ch = """"c Then
                    inQuote = Not inQuote
                    sb.Append(ch)
                ElseIf Not inQuote AndAlso ch = "("c Then
                    depth += 1
                    sb.Append(ch)
                ElseIf Not inQuote AndAlso ch = ")"c Then
                    depth -= 1
                    If depth < 0 Then Throw New ApplicationException("Formula contains unmatched ')'.")
                    sb.Append(ch)
                ElseIf Not inQuote AndAlso depth = 0 AndAlso ch = delimiter Then
                    out.Add(sb.ToString().Trim())
                    sb.Length = 0
                Else
                    sb.Append(ch)
                End If
            Next

            If depth <> 0 Then Throw New ApplicationException("Formula contains unmatched '('.")
            out.Add(sb.ToString().Trim())
            Return out
        End Function

        Private Sub ValidateRawDataObj(raw As DataObj)
            If raw Is Nothing Then Throw New ArgumentNullException(NameOf(raw))
            If raw.FinalData Is Nothing Then Throw New ApplicationException("DataObj.FinalData is Nothing. Import and clean the data before building a mixed-model request.")
            If raw.varNames Is Nothing Then Throw New ApplicationException("DataObj.varNames is Nothing.")
            If raw.nRows <= 0 Then Throw New ApplicationException("DataObj contains no rows.")
            If raw.nCols <= 0 Then Throw New ApplicationException("DataObj contains no columns.")
            If raw.varNames.Length <> raw.nCols Then Throw New ApplicationException("DataObj.varNames length does not match nCols.")
        End Sub



        Private Function ValidateAndCopyResponse(response() As Double,
                                                 expectedRows As Integer,
                                                 context As String) As Double()
            If response Is Nothing Then Throw New ArgumentNullException(NameOf(response))
            If expectedRows <= 0 Then Throw New ApplicationException("The predictor data contain no rows.")
            If response.Length <> expectedRows Then
                Throw New ApplicationException("Response vector length (" & response.Length.ToString(CultureInfo.InvariantCulture) & ") does not match the cleaned predictor row count (" & expectedRows.ToString(CultureInfo.InvariantCulture) & ").")
            End If

            Dim out(response.Length - 1) As Double
            For i As Integer = 0 To response.Length - 1
                Dim d As Double = response(i)
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then
                    Throw New ApplicationException(If(context, "response") & " contains a non-finite value at cleaned row " & (i + 1).ToString(CultureInfo.InvariantCulture) & ".")
                End If
                out(i) = d
            Next
            Return out
        End Function

        Private Function ResolveColumnIndex(varNames() As String, key As String, role As String) As Integer
            If varNames Is Nothing Then Throw New ArgumentNullException(NameOf(varNames))
            If String.IsNullOrWhiteSpace(key) Then Throw New ApplicationException("A " & role & " column key is required.")

            Dim k As String = key.Trim()
            For i As Integer = 0 To varNames.Length - 1
                If String.Equals(If(varNames(i), String.Empty).Trim(), k, StringComparison.OrdinalIgnoreCase) Then Return i
            Next

            Dim letterIndex As Integer = TryResolveColumnLetter(k)
            If letterIndex >= 0 AndAlso letterIndex < varNames.Length Then Return letterIndex

            Dim parsedOneBased As Integer
            If Integer.TryParse(k, NumberStyles.Integer, CultureInfo.InvariantCulture, parsedOneBased) AndAlso parsedOneBased >= 1 AndAlso parsedOneBased <= varNames.Length Then
                Return parsedOneBased - 1
            End If

            Throw New ApplicationException("Could not resolve " & role & " column key '" & key & "'. Available columns: " & String.Join(", ", varNames))
        End Function

        Private Function TryResolveColumnLetter(text As String) As Integer
            If String.IsNullOrWhiteSpace(text) Then Return -1
            Dim s As String = text.Trim().ToUpperInvariant()
            For Each ch As Char In s
                If ch < "A"c OrElse ch > "Z"c Then Return -1
            Next
            Dim value As Integer = 0
            For Each ch As Char In s
                value = value * 26 + (AscW(ch) - AscW("A"c) + 1)
            Next
            Return value - 1
        End Function

        Private Function ToDoubleStrict(value As Object, context As String) As Double
            If value Is Nothing OrElse Convert.IsDBNull(value) Then
                Throw New ApplicationException(context & " contains a missing value.")
            End If
            Try
                Dim d As Double = Convert.ToDouble(value, CultureInfo.InvariantCulture)
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Throw New ApplicationException(context & " contains a non-finite numeric value.")
                Return d
            Catch ex As Exception
                Throw New ApplicationException(context & " must be numeric for this mixed-model formula path. Value='" & Convert.ToString(value, CultureInfo.InvariantCulture) & "'.", ex)
            End Try
        End Function

        Private Function MakeUniqueBaseKeys(names() As String) As String()
            If names Is Nothing Then Return Array.Empty(Of String)()
            Dim seen As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Dim out(names.Length - 1) As String
            For i As Integer = 0 To names.Length - 1
                Dim baseName As String = If(names(i), String.Empty).Trim()
                If baseName = String.Empty Then baseName = "Var" & (i + 1).ToString(CultureInfo.InvariantCulture)
                If Not seen.ContainsKey(baseName) Then
                    seen(baseName) = 1
                    out(i) = baseName
                Else
                    seen(baseName) += 1
                    out(i) = baseName & "_" & seen(baseName).ToString(CultureInfo.InvariantCulture)
                End If
            Next
            Return out
        End Function

        Private Function SafeColumnName(varNames() As String, index As Integer) As String
            If varNames Is Nothing OrElse index < 0 OrElse index >= varNames.Length Then Return "Var" & (index + 1).ToString(CultureInfo.InvariantCulture)
            Dim s As String = If(varNames(index), String.Empty).Trim()
            If s = String.Empty Then s = "Var" & (index + 1).ToString(CultureInfo.InvariantCulture)
            Return s
        End Function

        Private Function BuildRequestLabel(responseName As String,
                                           subjectName As String,
                                           visitName As String,
                                           isMMRM As Boolean) As String
            Dim modelName As String = If(isMMRM, "MMRM", "LMM")
            Dim label As String = modelName & ": " & If(responseName, String.Empty) & " by subject " & If(subjectName, String.Empty)
            If Not String.IsNullOrWhiteSpace(visitName) Then label &= ", visit " & visitName
            Return label
        End Function

        Private Sub AppendInfo(ByRef strTrace As String, message As String)
            AppendLogCore(strTrace, "INFO", message)
            AppGlobals.BSlogg.Info(message)
        End Sub

        Private Sub AppendWarn(ByRef strTrace As String, message As String)
            AppendLogCore(strTrace, "WARN", message)
            AppGlobals.BSlogg.Warn(message)
        End Sub

        Private Sub AppendTrace(ByRef strTrace As String, message As String)
            AppendLogCore(strTrace, "TRACE", message)
            AppGlobals.BSlogg.Trace(message)
        End Sub

        Private Sub AppendLogCore(ByRef strTrace As String, level As String, message As String)
            Dim line As String = Date.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture) & "|" & level & "|MixedModelFormulaService|" & If(message, String.Empty)
            If String.IsNullOrEmpty(strTrace) Then
                strTrace = line
            Else
                strTrace &= vbNewLine & line
            End If
        End Sub

        Private Function MergeTrace(a As String, b As String) As String
            If String.IsNullOrEmpty(a) Then Return If(b, String.Empty)
            If String.IsNullOrEmpty(b) Then Return a
            If a.Contains(b) Then Return a
            Return a & vbNewLine & b
        End Function

    End Module

End Namespace
