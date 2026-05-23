Option Explicit On
Option Strict On

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' UI/UDF-facing helper routines shared by mixed-model front ends.
    ''' The routines in this module intentionally avoid Windows Forms and Excel-DNA types so they
    ''' can be reused by Ui18MMRM/Ui19LMM and by future LMM worksheet functions.
    ''' </summary>
    Friend Module MixedModelFrontEndHelpers

        Friend Function ResidualStructureRequiresVisit(structureName As String) As Boolean
            Try
                Dim r As MixedModelRStruct = MixedModelRStructUtils.createMixedModelRStruct(structureName)
                Return r IsNot Nothing AndAlso r.UsesVisitIndex()
            Catch
                Return False
            End Try
        End Function

        Friend Function ParseFitMethod(arg As Object,
                                       Optional defaultValue As MixedModelFitMethod = MixedModelFitMethod.REML) As MixedModelFitMethod
            Dim token As String = NormalizeToken(AsString(arg))
            If token = "ML" OrElse token = "MAXIMUMLIKELIHOOD" Then Return MixedModelFitMethod.ML
            If token = "REML" OrElse token = "RESTRICTEDML" OrElse token = "RESTRICTEDMAXIMUMLIKELIHOOD" Then Return MixedModelFitMethod.REML
            Return defaultValue
        End Function

        Friend Function ParseInferenceMethod(arg As Object,
                                             Optional defaultValue As MixedModelFixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger) As MixedModelFixedInferenceMethod
            Dim token As String = NormalizeToken(AsString(arg))
            If token = "KR" OrElse token = "KENWARDROGER" Then Return MixedModelFixedInferenceMethod.KenwardRoger
            If token = "SATTERTHWAITE" OrElse token = "SATTERTHWAITEDF" Then Return MixedModelFixedInferenceMethod.Satterthwaite
            If token = "BETWEENWITHIN" OrElse token = "BW" Then Return MixedModelFixedInferenceMethod.BetweenWithin
            If token = "RESIDUALDF" OrElse token = "RESIDUAL" Then Return MixedModelFixedInferenceMethod.ResidualDF
            If token = "WALD" OrElse token = "WALDNORMAL" OrElse token = "LARGESAMPLENORMAL" OrElse token = "NORMAL" Then Return MixedModelFixedInferenceMethod.WaldNormal
            Return defaultValue
        End Function

        Friend Function ParseCovarianceOptimizerMode(arg As Object,
                                                      Optional defaultValue As MixedModelCovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.AverageInformationReml) As MixedModelCovarianceOptimizerMode
            Dim token As String = NormalizeToken(AsString(arg))
            If token = String.Empty Then Return defaultValue

            If token = "AI" OrElse token = "AVERAGEINFORMATION" OrElse token = "FISHERSCORING" OrElse token = "SAS" OrElse
               token = "AIFISHERSCORINGDEFAULT" OrElse token = "AVERAGEINFORMATIONREML" Then
                Return MixedModelCovarianceOptimizerMode.AverageInformationReml
            End If

            If token = "BFGSANALYTIC" OrElse token = "PROJECTEDBFGSANALYTIC" OrElse
               token = "PROJECTEDBFGSANALYTICGRADIENT" OrElse token = "ANALYTICBFGS" Then
                Return MixedModelCovarianceOptimizerMode.ProjectedBfgsAnalyticGradient
            End If

            If token = "BFGS" OrElse token = "PROJECTEDBFGS" OrElse token = "BFGSAUTOGRADIENT" OrElse
               token = "PROJECTEDBFGSAUTOGRADIENT" OrElse token = "BFGSNUMERICAL" OrElse
               token = "PROJECTEDBFGSNUMERICAL" OrElse token = "BFGSFINITE" OrElse
               token = "BFGSFINITEDIFFERENCE" OrElse token = "NUMERICALBFGS" Then
                Return MixedModelCovarianceOptimizerMode.ProjectedBfgs
            End If

            Return defaultValue
        End Function

        Friend Function ParseCovarianceGradientMode(arg As Object,
                                                     Optional defaultValue As MixedModelCovarianceGradientMode = MixedModelCovarianceGradientMode.Auto) As MixedModelCovarianceGradientMode
            Dim token As String = NormalizeToken(AsString(arg))
            If token = String.Empty OrElse token = "AUTO" OrElse token = "AUTOANALYTICWHEREAVAILABLE" Then Return defaultValue

            If token = "ANALYTIC" OrElse token = "ANALYTICSCORE" Then
                Return MixedModelCovarianceGradientMode.AnalyticScore
            End If

            If token = "ANALYTICVALIDATION" OrElse token = "VALIDATE" OrElse
               token = "ANALYTICSCOREFINITEDIFFERENCEVALIDATION" OrElse
               token = "ANALYTICSCOREWITHFINITEDIFFERENCEVALIDATION" Then
                Return MixedModelCovarianceGradientMode.AnalyticScoreWithFiniteDifferenceValidation
            End If

            If token = "NUMERICAL" OrElse token = "FINITE" OrElse token = "FINITEDIFFERENCE" OrElse
               token = "NUMERICALFINITEDIFFERENCE" Then
                Return MixedModelCovarianceGradientMode.NumericalFiniteDifference
            End If

            Return defaultValue
        End Function

        Friend Function NormalizeToken(s As String) As String
            If String.IsNullOrWhiteSpace(s) Then Return String.Empty
            Dim chars As IEnumerable(Of Char) = s.Where(Function(ch) Char.IsLetterOrDigit(ch))
            Return New String(chars.ToArray()).ToUpperInvariant()
        End Function

        Friend Function AsString(arg As Object) As String
            If arg Is Nothing Then Return String.Empty
            Return Convert.ToString(arg, CultureInfo.InvariantCulture)
        End Function

        Friend Function ResolveDataColumnIndex(raw As Global.BESHStatNG.DataObj,
                                               key As String,
                                               role As String,
                                               Optional analysisLabel As String = "mixed-model") As Integer
            If raw Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(raw)))
            If raw.varNames Is Nothing Then CoreServices.Errors.LogAndThrow(New ApplicationException("Data object has no variable names."))

            Dim targetKey As String = If(key, String.Empty).Trim()
            Dim targetBase As String = Global.BESHStatNG.RegressionDesignCore.GetCoefBaseName(targetKey).Trim()

            For j As Integer = 0 To raw.varNames.Length - 1
                Dim candidate As String = If(raw.varNames(j), String.Empty).Trim()
                If String.Equals(candidate, targetKey, StringComparison.Ordinal) Then Return j
                If String.Equals(candidate, targetBase, StringComparison.Ordinal) Then Return j
                If String.Equals(Global.BESHStatNG.RegressionDesignCore.GetCoefBaseName(candidate), targetBase, StringComparison.Ordinal) Then Return j
            Next

            CoreServices.Errors.LogAndThrow(New ApplicationException("Cannot resolve " & role & " variable '" & key & "' in imported " & analysisLabel & " data."))
            Return -1
        End Function

        Friend Function ExtractNumericColumnFromData(raw As Global.BESHStatNG.DataObj,
                                                     columnIndex As Integer,
                                                     Optional analysisLabel As String = "mixed-model") As Double()
            If raw Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(raw)))
            If raw.FinalData Is Nothing Then CoreServices.Errors.LogAndThrow(New ApplicationException("Data object has no FinalData matrix."))
            If columnIndex < 0 OrElse columnIndex >= raw.nCols Then CoreServices.Errors.LogAndThrow(New ArgumentOutOfRangeException(NameOf(columnIndex)))

            Dim out(raw.nRows - 1) As Double
            For i As Integer = 0 To raw.nRows - 1
                If raw.FinalData(i, columnIndex) Is Nothing Then
                    CoreServices.Errors.LogAndThrow(New ApplicationException("Missing numeric value encountered in " & analysisLabel & " data at row " & CStr(i + 1) & ", column " & CStr(columnIndex + 1) & "."))
                End If
                out(i) = CDbl(raw.FinalData(i, columnIndex))
            Next

            Return out
        End Function

        Friend Function ExtractObjectColumnFromData(raw As Global.BESHStatNG.DataObj,
                                                    columnIndex As Integer) As Object()
            If raw Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(raw)))
            If raw.FinalData Is Nothing Then CoreServices.Errors.LogAndThrow(New ApplicationException("Data object has no FinalData matrix."))
            If columnIndex < 0 OrElse columnIndex >= raw.nCols Then CoreServices.Errors.LogAndThrow(New ArgumentOutOfRangeException(NameOf(columnIndex)))

            Dim out(raw.nRows - 1) As Object
            For i As Integer = 0 To raw.nRows - 1
                out(i) = raw.FinalData(i, columnIndex)
            Next

            Return out
        End Function

        Friend Function ExtractRawNumericMatrix(raw As Global.BESHStatNG.DataObj,
                                                rawKeys As List(Of String),
                                                role As String,
                                                Optional analysisLabel As String = "mixed-model") As Double(,)
            If rawKeys Is Nothing OrElse rawKeys.Count = 0 Then Return Nothing

            Dim out(raw.nRows - 1, rawKeys.Count - 1) As Double

            For j As Integer = 0 To rawKeys.Count - 1
                Dim col As Integer = ResolveDataColumnIndex(raw, rawKeys(j), role & " source", analysisLabel)
                Dim tmp() As Double = ExtractNumericColumnFromData(raw, col, analysisLabel)

                For i As Integer = 0 To raw.nRows - 1
                    out(i, j) = tmp(i)
                Next
            Next

            Return out
        End Function

        Friend Sub BuildExpandedDesignFromEffectSpecs(raw As Global.BESHStatNG.DataObj,
                                                      effectItems As IEnumerable,
                                                      termSpecs As Dictionary(Of String, Global.BESHStatNG.TermSpec),
                                                      includeIntercept As Boolean,
                                                      role As String,
                                                      ByRef design(,) As Double,
                                                      ByRef designNames() As String,
                                                      Optional analysisLabel As String = "mixed-model")
            If raw Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(raw)))
            If effectItems Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(effectItems)))
            If termSpecs Is Nothing Then termSpecs = New Dictionary(Of String, Global.BESHStatNG.TermSpec)(StringComparer.Ordinal)

            Dim rawKeys As List(Of String) = Global.BESHStatNG.RegressionDesignCore.GetRequiredRawVarKeys(effectItems, termSpecs)
            Dim expanded(,) As Double = Nothing
            Dim expandedNames() As String = New String() {}

            If rawKeys.Count > 0 Then
                Dim rawMatrix(,) As Double = ExtractRawNumericMatrix(raw, rawKeys, role, analysisLabel)

                Global.BESHStatNG.RegressionDesignCore.BuildExpandedPredictorMatrix(rawX:=rawMatrix,
                                                                                    rawXKeys:=rawKeys,
                                                                                    effectItems:=effectItems,
                                                                                    termSpecs:=termSpecs,
                                                                                    omitCategoricalReference:=True,
                                                                                    outX:=expanded,
                                                                                    outPredictorNames:=expandedNames)
            End If

            If expandedNames Is Nothing Then expandedNames = New String() {}

            If includeIntercept Then
                design = AddInterceptColumn(expanded, raw.nRows)
                designNames = AddInterceptName(expandedNames)
            Else
                If expanded Is Nothing OrElse expandedNames.Length = 0 Then
                    CoreServices.Errors.LogAndThrow(New ApplicationException("The " & role & " design contains no columns. Add at least one " & role & " or enable its intercept."))
                End If

                design = expanded
                designNames = DirectCast(expandedNames.Clone(), String())
            End If
        End Sub

        Friend Function AddInterceptColumn(expandedX(,) As Double, nRows As Integer) As Double(,)
            Dim p As Integer = If(expandedX Is Nothing, 0, expandedX.GetLength(1))
            Dim out(nRows - 1, p) As Double

            For i As Integer = 0 To nRows - 1
                out(i, 0) = 1.0
            Next

            If p > 0 Then
                For i As Integer = 0 To nRows - 1
                    For j As Integer = 0 To p - 1
                        out(i, j + 1) = expandedX(i, j)
                    Next
                Next
            End If

            Return out
        End Function

        Friend Function AddInterceptName(expandedNames() As String) As String()
            Dim p As Integer = If(expandedNames Is Nothing, 0, expandedNames.Length)
            Dim out(p) As String
            out(0) = "Intercept"

            If p > 0 Then
                For j As Integer = 0 To p - 1
                    out(j + 1) = expandedNames(j)
                Next
            End If

            Return out
        End Function

        Friend Function AddInterceptIfRequested(x(,) As Double, includeIntercept As Boolean) As Double(,)
            If Not includeIntercept Then Return x
            If x Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(x)))
            Return AddInterceptColumn(x, x.GetLength(0))
        End Function

        Friend Function AddInterceptNameIfRequested(names() As String, includeIntercept As Boolean) As String()
            If Not includeIntercept Then Return If(names, New String() {})
            Return AddInterceptName(names)
        End Function

        Friend Function DefaultNames(count As Integer, prefix As String) As String()
            If count <= 0 Then Return New String() {}
            Dim names(count - 1) As String
            For i As Integer = 0 To count - 1
                names(i) = prefix & (i + 1).ToString(CultureInfo.InvariantCulture)
            Next
            Return names
        End Function

        Friend Function BuildEffectsText(effectItems As IEnumerable,
                                         includeIntercept As Boolean,
                                         interceptOnlyText As String) As String
            If effectItems Is Nothing Then
                If includeIntercept Then Return interceptOnlyText
                Return String.Empty
            End If

            Dim parts As New List(Of String)()
            For Each it As Object In effectItems
                parts.Add(CStr(it))
            Next

            If parts.Count = 0 Then
                If includeIntercept Then Return interceptOnlyText
                Return String.Empty
            End If

            If includeIntercept Then Return "Intercept + " & String.Join(" + ", parts.ToArray())
            Return String.Join(" + ", parts.ToArray())
        End Function

        Friend Function TraceTextToMatrix(traceText As String) As Object(,)
            If traceText Is Nothing Then traceText = String.Empty

            Dim lines() As String = traceText.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split({vbLf}, StringSplitOptions.None)
            If lines.Length = 0 Then
                Dim emptyOut(0, 0) As Object
                emptyOut(0, 0) = String.Empty
                Return emptyOut
            End If

            Dim out(lines.Length - 1, 0) As Object
            For i As Integer = 0 To lines.Length - 1
                out(i, 0) = lines(i)
            Next
            Return out
        End Function

        Friend Function UniqueSortedFiniteValues(values() As Double) As List(Of Double)
            Dim dict As New Dictionary(Of Double, Boolean)()
            If values IsNot Nothing Then
                For Each v As Double In values
                    If Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v) AndAlso Not dict.ContainsKey(v) Then dict(v) = True
                Next
            End If
            Dim out As New List(Of Double)(dict.Keys)
            out.Sort()
            Return out
        End Function

        ''' <summary>
        ''' Returns unique class values sorted numerically when all non-missing values are numeric;
        ''' otherwise sorts by display text. Missing/blank values are ignored.
        ''' </summary>
        Friend Function UniqueSortedClassValues(values() As Object) As Object()
            If values Is Nothing Then Return New Object() {}

            Dim numericValues As New List(Of Double)()
            Dim textValues As New List(Of String)()
            Dim anyText As Boolean = False

            For Each obj As Object In values
                If obj Is Nothing Then Continue For

                Dim s As String = Convert.ToString(obj, CultureInfo.InvariantCulture).Trim()
                If s.Length = 0 Then Continue For

                Dim d As Double
                If Double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, d) Then
                    AddUniqueNumeric(numericValues, d)
                ElseIf Double.TryParse(s, d) Then
                    AddUniqueNumeric(numericValues, d)
                Else
                    anyText = True
                    AddUniqueText(textValues, s)
                End If
            Next

            If Not anyText Then
                If numericValues.Count = 0 Then Return New Object() {}
                numericValues.Sort()
                Dim out(numericValues.Count - 1) As Object
                For i As Integer = 0 To numericValues.Count - 1
                    out(i) = numericValues(i)
                Next
                Return out
            End If

            For Each d As Double In numericValues
                AddUniqueText(textValues, MixedModelPostEstimation.FormatProfileValue(d))
            Next

            If textValues.Count = 0 Then Return New Object() {}
            textValues.Sort(StringComparer.OrdinalIgnoreCase)
            Dim outText(textValues.Count - 1) As Object
            For i As Integer = 0 To textValues.Count - 1
                outText(i) = textValues(i)
            Next
            Return outText
        End Function

        ''' <summary>Formats class values as a SAS-like space-separated list.</summary>
        Friend Function JoinClassValues(values() As Object) As String
            If values Is Nothing OrElse values.Length = 0 Then Return String.Empty

            Dim parts As New List(Of String)()
            For Each obj As Object In values
                If obj Is Nothing Then Continue For

                If TypeOf obj Is Double Then
                    parts.Add(MixedModelPostEstimation.FormatProfileValue(CDbl(obj)))
                ElseIf TypeOf obj Is Single OrElse TypeOf obj Is Decimal OrElse TypeOf obj Is Integer OrElse TypeOf obj Is Long OrElse TypeOf obj Is Short Then
                    parts.Add(MixedModelPostEstimation.FormatProfileValue(CDbl(obj)))
                Else
                    parts.Add(Convert.ToString(obj, CultureInfo.InvariantCulture))
                End If
            Next

            Return String.Join(" ", parts.ToArray())
        End Function

        Private Sub AddUniqueNumeric(values As List(Of Double), value As Double)
            If values Is Nothing Then Exit Sub
            If Not NumericGuards.IsFinite(value) Then Exit Sub

            For Each oldValue As Double In values
                If MixedModelPostEstimation.NearlyEqual(oldValue, value) Then Exit Sub
            Next

            values.Add(value)
        End Sub

        Private Sub AddUniqueText(values As List(Of String), value As String)
            If values Is Nothing Then Exit Sub
            If String.IsNullOrWhiteSpace(value) Then Exit Sub

            For Each oldValue As String In values
                If String.Equals(oldValue, value, StringComparison.OrdinalIgnoreCase) Then Exit Sub
            Next

            values.Add(value)
        End Sub

        Friend Function RandomAuthoringRequiresGeneralGSideStructure(termSpecs As Dictionary(Of String, Global.BESHStatNG.TermSpec),
                                                                      authoredRandomEffectCount As Integer,
                                                                      includeRandomIntercept As Boolean) As Boolean
            If authoredRandomEffectCount < 0 Then authoredRandomEffectCount = 0

            If Not includeRandomIntercept AndAlso authoredRandomEffectCount > 0 Then Return True
            If authoredRandomEffectCount + If(includeRandomIntercept, 1, 0) > 2 Then Return True

            If termSpecs IsNot Nothing Then
                For Each kvp As KeyValuePair(Of String, Global.BESHStatNG.TermSpec) In termSpecs
                    Dim spec As Global.BESHStatNG.TermSpec = kvp.Value
                    If spec Is Nothing Then Continue For
                    If spec.Scale = Global.BESHStatNG.PredictorScale.Categorical Then Return True
                    If Not String.Equals(spec.Kind, "MainEffect", StringComparison.OrdinalIgnoreCase) Then Return True
                    If spec.Degree > 1 Then Return True
                Next
            End If

            Return False
        End Function

        Friend Sub ValidateRandomStructureAgainstDesign(randomStructName As String,
                                                        z(,) As Double,
                                                        randomNames() As String,
                                                        Optional randomInterceptChecked As Boolean? = Nothing,
                                                        Optional authoredRandomEffectCount As Integer? = Nothing,
                                                        Optional enforceUiInterceptSemantics As Boolean = False)
            Dim q As Integer = If(z Is Nothing, 0, z.GetLength(1))
            ValidateRandomStructureAgainstColumnCount(randomStructName,
                                                      q,
                                                      randomNames,
                                                      randomInterceptChecked,
                                                      authoredRandomEffectCount,
                                                      enforceUiInterceptSemantics)
        End Sub

        Friend Sub ValidateRandomStructureAgainstColumnCount(randomStructName As String,
                                                            q As Integer,
                                                            randomNames() As String,
                                                            Optional randomInterceptChecked As Boolean? = Nothing,
                                                            Optional authoredRandomEffectCount As Integer? = Nothing,
                                                            Optional enforceUiInterceptSemantics As Boolean = False)
            If q <= 0 Then
                CoreServices.Errors.LogAndThrow(New ApplicationException("The random-effects design contains no columns. Add at least one random effect or enable Random Intercepts."))
            End If

            If String.Equals(randomStructName, "Random Intercept", StringComparison.OrdinalIgnoreCase) Then
                If q <> 1 Then
                    CoreServices.Errors.LogAndThrow(New ApplicationException("Random Intercept covariance requires exactly one expanded random-effect column, but the current random-effects design has " & q.ToString(CultureInfo.InvariantCulture) & ". Choose Identity, Variance Components (VC/Diag), CS, CSH, AR1, ARH1, TOEP, TOEPH, or Unstructured Random Effects for multiple random effects or interactions."))
                End If

                If enforceUiInterceptSemantics Then
                    Dim hasIntercept As Boolean = randomInterceptChecked.HasValue AndAlso randomInterceptChecked.Value
                    Dim authored As Integer = If(authoredRandomEffectCount.HasValue, authoredRandomEffectCount.Value, 0)
                    If Not hasIntercept OrElse authored <> 0 Then
                        CoreServices.Errors.LogAndThrow(New ApplicationException("Random Intercept covariance requires Random Intercepts enabled and no authored random slopes/effects. Choose Identity, Variance Components (VC/Diag), CS, CSH, AR1, ARH1, TOEP, TOEPH, or Unstructured Random Effects for slope-only, categorical, interaction, polynomial, or multiple random effects."))
                    End If
                End If
            End If

            If String.Equals(randomStructName, "Random Intercept + Slope", StringComparison.OrdinalIgnoreCase) Then
                If q <> 2 Then
                    CoreServices.Errors.LogAndThrow(New ApplicationException("Random Intercept + Slope covariance requires exactly two expanded random-effect columns, but the current random-effects design has " & q.ToString(CultureInfo.InvariantCulture) & ". Choose Identity, Variance Components (VC/Diag), CS, CSH, AR1, ARH1, TOEP, TOEPH, or Unstructured Random Effects for multiple random effects or interactions."))
                End If

                If enforceUiInterceptSemantics Then
                    Dim hasIntercept As Boolean = randomInterceptChecked.HasValue AndAlso randomInterceptChecked.Value
                    Dim authored As Integer = If(authoredRandomEffectCount.HasValue, authoredRandomEffectCount.Value, 0)
                    If Not hasIntercept OrElse authored <> 1 Then
                        CoreServices.Errors.LogAndThrow(New ApplicationException("Random Intercept + Slope covariance requires Random Intercepts enabled plus exactly one authored random slope/effect. Choose Identity, Variance Components (VC/Diag), CS, CSH, AR1, ARH1, TOEP, TOEPH, or Unstructured Random Effects for slope-only, categorical, interaction, polynomial, or multiple random effects."))
                    End If
                End If
            End If
        End Sub

    End Module

End Namespace