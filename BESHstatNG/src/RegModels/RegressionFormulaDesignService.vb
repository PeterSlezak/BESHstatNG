Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

''' <summary>
''' Contains the intermediate and final artifacts produced when a predictor matrix is expanded from a formula.
''' </summary>
Public Class RegressionFormulaMatrixBuildResult
    Public Property VariableCatalog As RegressionVariableCatalog
    Public Property DesignSpec As RegressionFormulaDesignSpec
    Public Property FullRawPredictorKeys As String()
    Public Property FullRawPredictorNames As String()
    Public Property FullRawPredictorAbsoluteLetters As String()
    Public Property RequiredRawPredictorKeys As String()
    Public Property RequiredRawPredictorNames As String()
    Public Property RequiredRawPredictorMatrix As Double(,)
    Public Property ExpandedPredictorMatrix As Double(,)
    Public Property ExpandedPredictorNames As String()
End Class

''' <summary>
''' Contains the intermediate and final artifacts produced when a regression-style data matrix is expanded from a formula.
''' </summary>
Public Class RegressionFormulaRegressionDataBuildResult
    Inherits RegressionFormulaMatrixBuildResult

    Public Property ResponseKey As String
    Public Property RegressionDataMatrix As Double(,)
    Public Property RegressionDataVarNames As String()
End Class

''' <summary>
''' Builds variable catalogs, parses formulas, and materializes expanded predictor matrices from raw inputs.
''' </summary>
Public Module RegressionFormulaDesignService

    ''' <summary>
    ''' Builds a formula variable catalog from a raw predictor matrix and associated metadata.
    ''' </summary>
    ''' <param name="rawX">The raw predictor matrix.</param>
    ''' <param name="predictorNames">Predictor display names in raw-column order.</param>
    ''' <param name="baseKeys">Internal base keys associated with each raw predictor column.</param>
    ''' <param name="absoluteColumnLetters">Absolute worksheet column letters aligned with the raw predictor columns.</param>
    ''' <param name="allowRelativeColumnLetters">Whether relative X-column letters such as A, B, or AA are allowed.</param>
    ''' <param name="allowAbsoluteColumnLetters">Whether absolute worksheet column letters are allowed.</param>
    ''' <param name="allowQuotedVariableNames">Whether quoted variable-name references are allowed.</param>
    ''' <returns>A variable catalog aligned with the supplied predictor matrix.</returns>
    Public Function BuildVariableCatalogFromRawPredictors(rawX(,) As Double,
                                                          Optional predictorNames As IEnumerable(Of String) = Nothing,
                                                          Optional baseKeys As IEnumerable(Of String) = Nothing,
                                                          Optional absoluteColumnLetters As IEnumerable(Of String) = Nothing,
                                                          Optional allowRelativeColumnLetters As Boolean = True,
                                                          Optional allowAbsoluteColumnLetters As Boolean = False,
                                                          Optional allowQuotedVariableNames As Boolean = True) As RegressionVariableCatalog

        Dim nRows As Integer = 0
        Dim p As Integer = 0
        ValidatePredictorMatrix(rawX, nRows, p)

        Dim names As List(Of String) = MaterializePredictorNames(p, predictorNames)
        Dim keys As List(Of String) = MaterializeBaseKeys(p, baseKeys)
        Dim absCols As List(Of String) = MaterializeAbsoluteColumnLetters(p, absoluteColumnLetters)

        Return RegressionVariableCatalog.Build(varNames:=names,
                                               baseKeys:=keys,
                                               absoluteColumnLetters:=absCols,
                                               allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                               allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                               allowQuotedVariableNames:=allowQuotedVariableNames)
    End Function

    ''' <summary>
    ''' Attempts to parse a formula and build the corresponding expanded predictor matrix.
    ''' </summary>
    ''' <param name="rawX">The raw predictor matrix.</param>
    ''' <param name="predictorNames">Predictor display names in raw-column order.</param>
    ''' <param name="formulaText">The formula text to parse. Blank text produces the default main-effects design.</param>
    ''' <param name="baseKeys">Internal base keys associated with each raw predictor column.</param>
    ''' <param name="absoluteColumnLetters">Absolute worksheet column letters aligned with the raw predictor columns.</param>
    ''' <param name="allowRelativeColumnLetters">Whether relative X-column letters such as A, B, or AA are allowed.</param>
    ''' <param name="allowAbsoluteColumnLetters">Whether absolute worksheet column letters are allowed.</param>
    ''' <param name="allowQuotedVariableNames">Whether quoted variable-name references are allowed.</param>
    ''' <param name="omitCategoricalReference">Whether the categorical reference level should be omitted from the expanded matrix.</param>
    ''' <param name="result">On success, receives the build result.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when parsing and matrix construction succeed; otherwise, False.</returns>
    Public Function TryBuildExpandedPredictorMatrixFromFormula(rawX(,) As Double,
                                                               ByRef result As RegressionFormulaMatrixBuildResult,
                                                               ByRef errorMessage As String,
                                                               Optional predictorNames As IEnumerable(Of String) = Nothing,
                                                               Optional formulaText As String = Nothing,
                                                               Optional baseKeys As IEnumerable(Of String) = Nothing,
                                                               Optional absoluteColumnLetters As IEnumerable(Of String) = Nothing,
                                                               Optional allowRelativeColumnLetters As Boolean = True,
                                                               Optional allowAbsoluteColumnLetters As Boolean = False,
                                                               Optional allowQuotedVariableNames As Boolean = True,
                                                               Optional omitCategoricalReference As Boolean = True) As Boolean

        result = Nothing
        errorMessage = Nothing
        Dim rawRows As Integer = If(rawX Is Nothing, 0, rawX.GetLength(0))
        Dim rawCols As Integer = If(rawX Is Nothing, 0, rawX.GetLength(1))
        CoreServices.Logger.Trace($"TryBuildExpandedPredictorMatrixFromFormula start. rawShape={rawRows}x{rawCols}; formula='{If(formulaText, String.Empty)}'; omitCategoricalReference={omitCategoricalReference}; allowRelative={allowRelativeColumnLetters}; allowAbsolute={allowAbsoluteColumnLetters}; allowQuoted={allowQuotedVariableNames}")

        Try
            Dim catalog As RegressionVariableCatalog = BuildVariableCatalogFromRawPredictors(rawX:=rawX,
                                                                                             predictorNames:=predictorNames,
                                                                                             baseKeys:=baseKeys,
                                                                                             absoluteColumnLetters:=absoluteColumnLetters,
                                                                                             allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                                                             allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                                                             allowQuotedVariableNames:=allowQuotedVariableNames)

            Dim spec As RegressionFormulaDesignSpec = Nothing
            If Not RegressionFormulaParser.TryParseFormulaToDesignSpec(formulaText:=formulaText,
                                                                      variableCatalog:=catalog,
                                                                      designSpec:=spec,
                                                                      errorMessage:=errorMessage) Then
                CoreServices.Logger.Debug($"TryBuildExpandedPredictorMatrixFromFormula parse failed. formula='{If(formulaText, String.Empty)}'; error='{errorMessage}'")
                Return False
            End If

            Dim orderedVars = catalog.Variables.OrderBy(Function(v) v.RelativeColumnIndex).ToList()
            Dim fullKeys() As String = orderedVars.Select(Function(v) v.BaseKey).ToArray()
            Dim fullNames() As String = orderedVars.Select(Function(v) v.DisplayName).ToArray()
            Dim fullAbs() As String = orderedVars.Select(Function(v) v.AbsoluteColumnLetter).ToArray()

            Dim requiredKeys As List(Of String) = If(spec.RequiredRawVarKeys, New List(Of String)())
            Dim requiredIndices As List(Of Integer) = ResolveRequiredColumnIndices(fullKeys, requiredKeys)
            Dim requiredX(,) As Double = SubsetPredictorMatrix(rawX, requiredIndices)
            Dim requiredKeyArr() As String = requiredIndices.Select(Function(ix) fullKeys(ix)).ToArray()
            Dim requiredNameArr() As String = requiredIndices.Select(Function(ix) fullNames(ix)).ToArray()

            Dim expandedX(,) As Double = Nothing
            Dim expandedNames() As String = Nothing

            RegressionDesignCore.BuildExpandedPredictorMatrix(rawX:=requiredX,
                                                             rawXKeys:=requiredKeyArr,
                                                             effectItems:=spec.EffectItems,
                                                             termSpecs:=spec.TermSpecs,
                                                             omitCategoricalReference:=omitCategoricalReference,
                                                             outX:=expandedX,
                                                             outPredictorNames:=expandedNames)

            result = New RegressionFormulaMatrixBuildResult With {
                .VariableCatalog = catalog,
                .DesignSpec = spec,
                .FullRawPredictorKeys = fullKeys,
                .FullRawPredictorNames = fullNames,
                .FullRawPredictorAbsoluteLetters = fullAbs,
                .RequiredRawPredictorKeys = requiredKeyArr,
                .RequiredRawPredictorNames = requiredNameArr,
                .RequiredRawPredictorMatrix = requiredX,
                .ExpandedPredictorMatrix = expandedX,
                .ExpandedPredictorNames = If(expandedNames, New String() {})
            }
            CoreServices.Logger.Trace($"TryBuildExpandedPredictorMatrixFromFormula success. normalizedFormula='{If(spec.NormalizedFormulaText, String.Empty)}'; requiredRaw={requiredKeyArr.Length}; expandedCols={If(expandedNames Is Nothing, 0, expandedNames.Length)}")
            Return True

        Catch ex As Exception
            CoreServices.Logger.Error(ex, "TryBuildExpandedPredictorMatrixFromFormula failed.")
            errorMessage = ex.Message
            result = Nothing
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Parses a formula and builds the corresponding expanded predictor matrix or throws an exception.
    ''' </summary>
    ''' <param name="rawX">The raw predictor matrix.</param>
    ''' <param name="predictorNames">Predictor display names in raw-column order.</param>
    ''' <param name="formulaText">The formula text to parse. Blank text produces the default main-effects design.</param>
    ''' <param name="baseKeys">Internal base keys associated with each raw predictor column.</param>
    ''' <param name="absoluteColumnLetters">Absolute worksheet column letters aligned with the raw predictor columns.</param>
    ''' <param name="allowRelativeColumnLetters">Whether relative X-column letters such as A, B, or AA are allowed.</param>
    ''' <param name="allowAbsoluteColumnLetters">Whether absolute worksheet column letters are allowed.</param>
    ''' <param name="allowQuotedVariableNames">Whether quoted variable-name references are allowed.</param>
    ''' <param name="omitCategoricalReference">Whether the categorical reference level should be omitted from the expanded matrix.</param>
    ''' <returns>The expanded predictor-matrix build result.</returns>
    Public Function BuildExpandedPredictorMatrixFromFormula(rawX(,) As Double,
                                                            Optional predictorNames As IEnumerable(Of String) = Nothing,
                                                            Optional formulaText As String = Nothing,
                                                            Optional baseKeys As IEnumerable(Of String) = Nothing,
                                                            Optional absoluteColumnLetters As IEnumerable(Of String) = Nothing,
                                                            Optional allowRelativeColumnLetters As Boolean = True,
                                                            Optional allowAbsoluteColumnLetters As Boolean = False,
                                                            Optional allowQuotedVariableNames As Boolean = True,
                                                            Optional omitCategoricalReference As Boolean = True) As RegressionFormulaMatrixBuildResult

        Dim result As RegressionFormulaMatrixBuildResult = Nothing
        Dim err As String = Nothing

        If Not TryBuildExpandedPredictorMatrixFromFormula(rawX:=rawX,
                                                          predictorNames:=predictorNames,
                                                          formulaText:=formulaText,
                                                          baseKeys:=baseKeys,
                                                          absoluteColumnLetters:=absoluteColumnLetters,
                                                          allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                          allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                          allowQuotedVariableNames:=allowQuotedVariableNames,
                                                          omitCategoricalReference:=omitCategoricalReference,
                                                          result:=result,
                                                          errorMessage:=err) Then
            CoreServices.Errors.LogAndThrow(New ArgumentException(err))
        End If

        Return result
    End Function

    ''' <summary>
    ''' Attempts to parse a formula and build a regression-style data matrix containing the response and expanded predictors.
    ''' </summary>
    ''' <param name="raw">The regression-style data object containing the response in the first column and raw predictors after it.</param>
    ''' <param name="yKey">The response-variable key used for the first column of the output matrix.</param>
    ''' <param name="formulaText">The formula text to parse. Blank text produces the default main-effects design.</param>
    ''' <param name="baseKeys">Internal base keys associated with each raw predictor column.</param>
    ''' <param name="absoluteColumnLetters">Absolute worksheet column letters aligned with the raw predictor columns.</param>
    ''' <param name="allowRelativeColumnLetters">Whether relative X-column letters such as A, B, or AA are allowed.</param>
    ''' <param name="allowAbsoluteColumnLetters">Whether absolute worksheet column letters are allowed.</param>
    ''' <param name="allowQuotedVariableNames">Whether quoted variable-name references are allowed.</param>
    ''' <param name="omitCategoricalReference">Whether the categorical reference level should be omitted from the expanded matrix.</param>
    ''' <param name="result">On success, receives the build result.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when parsing and matrix construction succeed; otherwise, False.</returns>
    Public Function TryBuildExpandedRegressionDataMatrixFromFormula(raw As DataObj,
                                                                    yKey As String,
                                                                    ByRef result As RegressionFormulaRegressionDataBuildResult,
                                                                    ByRef errorMessage As String,
                                                                    Optional formulaText As String = Nothing,
                                                                    Optional baseKeys As IEnumerable(Of String) = Nothing,
                                                                    Optional absoluteColumnLetters As IEnumerable(Of String) = Nothing,
                                                                    Optional allowRelativeColumnLetters As Boolean = True,
                                                                    Optional allowAbsoluteColumnLetters As Boolean = False,
                                                                    Optional allowQuotedVariableNames As Boolean = True,
                                                                    Optional omitCategoricalReference As Boolean = True) As Boolean
        result = Nothing
        errorMessage = Nothing

        Try
            If raw Is Nothing Then
                Throw New ArgumentNullException(NameOf(raw))
            End If
            If raw.nCols < 2 Then
                Throw New ArgumentException("Regression data object must contain a response column and at least one predictor column.")
            End If

            Dim allData(,) As Double = raw.DataDbl
            Dim nRows As Integer = raw.nRows
            Dim p As Integer = raw.nCols - 1

            Dim predictorNames As IEnumerable(Of String) = Enumerable.Empty(Of String)()
            If raw.varNames IsNot Nothing AndAlso raw.varNames.Length > 1 Then
                predictorNames = raw.varNames.Skip(1)
            End If

            Dim rawX(nRows - 1, p - 1) As Double
            For i As Integer = 0 To nRows - 1
                For j As Integer = 0 To p - 1
                    rawX(i, j) = allData(i, j + 1)
                Next
            Next

            Dim matrixResult As RegressionFormulaMatrixBuildResult = Nothing
            If Not TryBuildExpandedPredictorMatrixFromFormula(rawX:=rawX,
                                                              predictorNames:=predictorNames,
                                                              formulaText:=formulaText,
                                                              baseKeys:=baseKeys,
                                                              absoluteColumnLetters:=absoluteColumnLetters,
                                                              allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                              allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                              allowQuotedVariableNames:=allowQuotedVariableNames,
                                                              omitCategoricalReference:=omitCategoricalReference,
                                                              result:=matrixResult,
                                                              errorMessage:=errorMessage) Then
                Return False
            End If

            Dim expandedNames() As String = If(matrixResult.ExpandedPredictorNames, New String() {})
            Dim pExpanded As Integer = expandedNames.Length
            Dim outData(,) As Double = Nothing
            Dim outNames() As String = Nothing

            ReDim outData(nRows - 1, pExpanded)
            ReDim outNames(pExpanded)

            outNames(0) = RegressionDesignCore.GetCoefBaseName(yKey)

            For i As Integer = 0 To nRows - 1
                outData(i, 0) = allData(i, 0)
            Next

            If pExpanded > 0 AndAlso matrixResult.ExpandedPredictorMatrix IsNot Nothing Then
                For j As Integer = 0 To pExpanded - 1
                    outNames(j + 1) = expandedNames(j)
                    For i As Integer = 0 To nRows - 1
                        outData(i, j + 1) = matrixResult.ExpandedPredictorMatrix(i, j)
                    Next
                Next
            End If

            result = New RegressionFormulaRegressionDataBuildResult With {
                .VariableCatalog = matrixResult.VariableCatalog,
                .DesignSpec = matrixResult.DesignSpec,
                .FullRawPredictorKeys = matrixResult.FullRawPredictorKeys,
                .FullRawPredictorNames = matrixResult.FullRawPredictorNames,
                .FullRawPredictorAbsoluteLetters = matrixResult.FullRawPredictorAbsoluteLetters,
                .RequiredRawPredictorKeys = matrixResult.RequiredRawPredictorKeys,
                .RequiredRawPredictorNames = matrixResult.RequiredRawPredictorNames,
                .RequiredRawPredictorMatrix = matrixResult.RequiredRawPredictorMatrix,
                .ExpandedPredictorMatrix = matrixResult.ExpandedPredictorMatrix,
                .ExpandedPredictorNames = matrixResult.ExpandedPredictorNames,
                .ResponseKey = yKey,
                .RegressionDataMatrix = outData,
                .RegressionDataVarNames = outNames
            }

            Return True

        Catch ex As Exception
            CoreServices.Logger.Error(ex, "TryBuildExpandedRegressionDataMatrixFromFormula failed.")
            errorMessage = ex.Message
            result = Nothing
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Parses a formula and builds a regression-style data matrix containing the response and expanded predictors or throws an exception.
    ''' </summary>
    ''' <param name="raw">The regression-style data object containing the response in the first column and raw predictors after it.</param>
    ''' <param name="yKey">The response-variable key used for the first column of the output matrix.</param>
    ''' <param name="formulaText">The formula text to parse. Blank text produces the default main-effects design.</param>
    ''' <param name="baseKeys">Internal base keys associated with each raw predictor column.</param>
    ''' <param name="absoluteColumnLetters">Absolute worksheet column letters aligned with the raw predictor columns.</param>
    ''' <param name="allowRelativeColumnLetters">Whether relative X-column letters such as A, B, or AA are allowed.</param>
    ''' <param name="allowAbsoluteColumnLetters">Whether absolute worksheet column letters are allowed.</param>
    ''' <param name="allowQuotedVariableNames">Whether quoted variable-name references are allowed.</param>
    ''' <param name="omitCategoricalReference">Whether the categorical reference level should be omitted from the expanded matrix.</param>
    ''' <returns>The regression-data build result.</returns>
    Public Function BuildExpandedRegressionDataMatrixFromFormula(raw As DataObj,
                                                                 yKey As String,
                                                                 Optional formulaText As String = Nothing,
                                                                 Optional baseKeys As IEnumerable(Of String) = Nothing,
                                                                 Optional absoluteColumnLetters As IEnumerable(Of String) = Nothing,
                                                                 Optional allowRelativeColumnLetters As Boolean = True,
                                                                 Optional allowAbsoluteColumnLetters As Boolean = False,
                                                                 Optional allowQuotedVariableNames As Boolean = True,
                                                                 Optional omitCategoricalReference As Boolean = True) As RegressionFormulaRegressionDataBuildResult

        Dim result As RegressionFormulaRegressionDataBuildResult = Nothing
        Dim err As String = Nothing

        If Not TryBuildExpandedRegressionDataMatrixFromFormula(raw:=raw,
                                                               yKey:=yKey,
                                                               formulaText:=formulaText,
                                                               baseKeys:=baseKeys,
                                                               absoluteColumnLetters:=absoluteColumnLetters,
                                                               allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                               allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                               allowQuotedVariableNames:=allowQuotedVariableNames,
                                                               omitCategoricalReference:=omitCategoricalReference,
                                                               result:=result,
                                                               errorMessage:=err) Then
            CoreServices.Errors.LogAndThrow(New ArgumentException(err))
        End If

        Return result
    End Function

    ''' <summary>
    ''' Attempts to expand a raw predictor matrix by using an already prepared design specification.
    ''' </summary>
    ''' <param name="rawX">The full raw predictor matrix supplied for prediction or downstream scoring.</param>
    ''' <param name="fullRawPredictorKeys">The full raw predictor keys aligned with <paramref name="rawX"/> columns.</param>
    ''' <param name="designSpec">The parsed design specification created during model fitting.</param>
    ''' <param name="expandedX">On success, receives the expanded predictor matrix used by the fitted model.</param>
    ''' <param name="expandedPredictorNames">On success, receives the expanded predictor names.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <param name="omitCategoricalReference">Whether the categorical reference level should be omitted from the expanded matrix.</param>
    ''' <returns>True when expansion succeeds; otherwise, False.</returns>
    Public Function TryBuildExpandedPredictorMatrixFromDesignSpec(rawX(,) As Double,
                                                                  fullRawPredictorKeys As IEnumerable(Of String),
                                                                  designSpec As RegressionFormulaDesignSpec,
                                                                  ByRef expandedX As Double(,),
                                                                  ByRef expandedPredictorNames As String(),
                                                                  ByRef errorMessage As String,
                                                                  Optional omitCategoricalReference As Boolean = True) As Boolean

        expandedX = Nothing
        expandedPredictorNames = Nothing
        errorMessage = Nothing

        Try
            If designSpec Is Nothing Then
                Throw New ArgumentNullException(NameOf(designSpec))
            End If

            Dim nRows As Integer = 0
            Dim p As Integer = 0
            ValidatePredictorMatrix(rawX, nRows, p)

            Dim fullKeys As List(Of String) = If(fullRawPredictorKeys, Enumerable.Empty(Of String)()).Select(Function(x) If(x, String.Empty).Trim()).ToList()
            If fullKeys.Count <> p Then
                Throw New ArgumentException($"fullRawPredictorKeys count ({fullKeys.Count}) must match the number of predictor columns ({p}).")
            End If

            Dim requiredKeys As List(Of String) = If(designSpec.RequiredRawVarKeys, New List(Of String)())
            Dim requiredIndices As List(Of Integer) = ResolveRequiredColumnIndices(fullKeys, requiredKeys)
            Dim requiredX(,) As Double = SubsetPredictorMatrix(rawX, requiredIndices)
            Dim requiredKeyArr() As String = requiredIndices.Select(Function(ix) fullKeys(ix)).ToArray()

            RegressionDesignCore.BuildExpandedPredictorMatrix(rawX:=requiredX,
                                                             rawXKeys:=requiredKeyArr,
                                                             effectItems:=designSpec.EffectItems,
                                                             termSpecs:=designSpec.TermSpecs,
                                                             omitCategoricalReference:=omitCategoricalReference,
                                                             outX:=expandedX,
                                                             outPredictorNames:=expandedPredictorNames)

            Return True

        Catch ex As Exception
            CoreServices.Logger.Error(ex, "TryBuildExpandedPredictorMatrixFromDesignSpec failed.")
            errorMessage = ex.Message
            expandedX = Nothing
            expandedPredictorNames = Nothing
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Expands a raw predictor matrix by using an already prepared design specification.
    ''' </summary>
    ''' <param name="rawX">The full raw predictor matrix supplied for prediction or downstream scoring.</param>
    ''' <param name="fullRawPredictorKeys">The full raw predictor keys aligned with <paramref name="rawX"/> columns.</param>
    ''' <param name="designSpec">The parsed design specification created during model fitting.</param>
    ''' <param name="omitCategoricalReference">Whether the categorical reference level should be omitted from the expanded matrix.</param>
    ''' <returns>The expanded predictor matrix and predictor names.</returns>
    Public Function BuildExpandedPredictorMatrixFromDesignSpec(rawX(,) As Double,
                                                               fullRawPredictorKeys As IEnumerable(Of String),
                                                               designSpec As RegressionFormulaDesignSpec,
                                                               Optional omitCategoricalReference As Boolean = True) As RegressionFormulaMatrixBuildResult

        Dim expandedX(,) As Double = Nothing
        Dim expandedNames() As String = Nothing
        Dim err As String = Nothing

        If Not TryBuildExpandedPredictorMatrixFromDesignSpec(rawX:=rawX,
                                                             fullRawPredictorKeys:=fullRawPredictorKeys,
                                                             designSpec:=designSpec,
                                                             expandedX:=expandedX,
                                                             expandedPredictorNames:=expandedNames,
                                                             errorMessage:=err,
                                                             omitCategoricalReference:=omitCategoricalReference) Then
            CoreServices.Errors.LogAndThrow(New ArgumentException(err))
        End If

        Return New RegressionFormulaMatrixBuildResult With {
            .DesignSpec = designSpec,
            .FullRawPredictorKeys = If(fullRawPredictorKeys, Enumerable.Empty(Of String)()).ToArray(),
            .ExpandedPredictorMatrix = expandedX,
            .ExpandedPredictorNames = If(expandedNames, New String() {})
        }
    End Function

    Private Sub ValidatePredictorMatrix(rawX(,) As Double,
                                        ByRef nRows As Integer,
                                        ByRef p As Integer)

        If rawX Is Nothing Then
            Throw New ArgumentNullException(NameOf(rawX))
        End If

        nRows = UBound(rawX, 1) + 1
        p = UBound(rawX, 2) + 1

        If nRows < 1 Then
            Throw New ArgumentException("Predictor matrix must contain at least one row.")
        End If
        If p < 1 Then
            Throw New ArgumentException("Predictor matrix must contain at least one predictor column.")
        End If
    End Sub

    ''' <summary>
    ''' Materializes and validates predictor names for the supplied predictor count.
    ''' </summary>
    ''' <param name="p">Receives the number of predictor columns found in the predictor matrix.</param>
    ''' <param name="predictorNames">Predictor display names in raw-column order.</param>
    ''' <returns>A validated predictor-name list.</returns>
    Private Function MaterializePredictorNames(p As Integer,
                                               predictorNames As IEnumerable(Of String)) As List(Of String)

        Dim names As List(Of String)

        If predictorNames Is Nothing Then
            names = New List(Of String)()
        Else
            names = predictorNames.Select(Function(x) If(x, String.Empty)).ToList()
        End If

        If names.Count = 0 Then
            names = Enumerable.Range(1, p).Select(Function(i) "X" & i.ToString(CultureInfo.InvariantCulture)).ToList()
        ElseIf names.Count <> p Then
            Throw New ArgumentException($"predictorNames count ({names.Count}) must match the number of predictor columns ({p}).")
        End If

        For i As Integer = 0 To p - 1
            If names(i).Trim() = String.Empty Then
                names(i) = "X" & (i + 1).ToString(CultureInfo.InvariantCulture)
            Else
                names(i) = names(i).Trim()
            End If
        Next

        Return names
    End Function

    ''' <summary>
    ''' Materializes and validates base keys for the supplied predictor count.
    ''' </summary>
    ''' <param name="p">Receives the number of predictor columns found in the predictor matrix.</param>
    ''' <param name="baseKeys">Internal base keys associated with each raw predictor column.</param>
    ''' <returns>A validated base-key list.</returns>
    Private Function MaterializeBaseKeys(p As Integer,
                                         baseKeys As IEnumerable(Of String)) As List(Of String)

        Dim keys As List(Of String)

        If baseKeys Is Nothing Then
            keys = Enumerable.Range(1, p).Select(Function(i) RegressionVariableCatalog.NumberToLetters(i)).ToList()
        Else
            keys = baseKeys.Select(Function(x) If(x, String.Empty).Trim()).ToList()
            If keys.Count <> p Then
                Throw New ArgumentException($"baseKeys count ({keys.Count}) must match the number of predictor columns ({p}).")
            End If
        End If

        For i As Integer = 0 To keys.Count - 1
            If keys(i) = String.Empty Then
                Throw New ArgumentException($"baseKeys({i}) is blank.")
            End If
        Next

        Return keys
    End Function

    ''' <summary>
    ''' Materializes and validates absolute worksheet column letters for the supplied predictor count.
    ''' </summary>
    ''' <param name="p">Receives the number of predictor columns found in the predictor matrix.</param>
    ''' <param name="absoluteColumnLetters">Absolute worksheet column letters aligned with the raw predictor columns.</param>
    ''' <returns>A validated absolute-column-letter list.</returns>
    Private Function MaterializeAbsoluteColumnLetters(p As Integer,
                                                      absoluteColumnLetters As IEnumerable(Of String)) As List(Of String)

        If absoluteColumnLetters Is Nothing Then
            Return New List(Of String)()
        End If

        Dim absCols As List(Of String) = absoluteColumnLetters.Select(Function(x) If(x, String.Empty).Trim()).ToList()
        If absCols.Count <> p Then
            Throw New ArgumentException($"absoluteColumnLetters count ({absCols.Count}) must match the number of predictor columns ({p}).")
        End If

        Return absCols
    End Function

    ''' <summary>
    ''' Maps required raw predictor keys to their source column indices.
    ''' </summary>
    ''' <param name="fullRawKeys">All raw predictor keys in full-matrix order.</param>
    ''' <param name="requiredRawKeys">The subset of raw predictor keys needed by the design.</param>
    ''' <returns>The zero-based source-column indices corresponding to the requested raw keys.</returns>
    Private Function ResolveRequiredColumnIndices(fullRawKeys As IEnumerable(Of String),
                                                  requiredRawKeys As IEnumerable(Of String)) As List(Of Integer)

        Dim fullKeys As List(Of String) = If(fullRawKeys, Enumerable.Empty(Of String)()).ToList()
        Dim reqKeys As List(Of String) = If(requiredRawKeys, Enumerable.Empty(Of String)()).ToList()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

        For j As Integer = 0 To fullKeys.Count - 1
            idx(fullKeys(j)) = j
        Next

        Dim out As New List(Of Integer)()
        For Each key As String In reqKeys
            If Not idx.ContainsKey(key) Then
                Throw New ArgumentException("Missing raw predictor '" & key & "' required by the formula/design specification.")
            End If
            out.Add(idx(key))
        Next

        Return out
    End Function

    ''' <summary>
    ''' Builds a predictor-matrix subset using the requested source-column indices.
    ''' </summary>
    ''' <param name="rawX">The raw predictor matrix.</param>
    ''' <param name="requiredIndices">The source-column indices to copy into the subset matrix.</param>
    ''' <returns>The subset predictor matrix, or Nothing when no columns are requested.</returns>
    Private Function SubsetPredictorMatrix(rawX(,) As Double,
                                           requiredIndices As IList(Of Integer)) As Double(,)

        Dim nRows As Integer = 0
        Dim p As Integer = 0
        ValidatePredictorMatrix(rawX, nRows, p)

        If requiredIndices Is Nothing OrElse requiredIndices.Count = 0 Then
            Return Nothing
        End If

        Dim out(nRows - 1, requiredIndices.Count - 1) As Double

        For j As Integer = 0 To requiredIndices.Count - 1
            Dim srcCol As Integer = requiredIndices(j)
            If srcCol < 0 OrElse srcCol >= p Then
                Throw New ArgumentOutOfRangeException(NameOf(requiredIndices), "Required predictor index is out of range.")
            End If

            For i As Integer = 0 To nRows - 1
                out(i, j) = rawX(i, srcCol)
            Next
        Next

        Return out
    End Function

End Module