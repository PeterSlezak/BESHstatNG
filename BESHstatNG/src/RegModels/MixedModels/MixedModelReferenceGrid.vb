Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

Namespace regression

    ''' <summary>
    ''' Weighting rule used when marginalizing over class-factor combinations in a
    ''' mixed-model reference grid.
    ''' </summary>
    Public Enum MixedModelReferenceGridWeighting
        ''' <summary>
        ''' Weight each marginal cell by its observed frequency within the current by-profile.
        ''' </summary>
        ObservedCellFrequency = 0

        ''' <summary>
        ''' Weight each non-empty marginal cell equally.
        ''' </summary>
        EqualCells = 1
    End Enum


    ''' <summary>
    ''' How a continuous covariate should be represented in a reference grid.
    ''' </summary>
    Public Enum MixedModelCovariateReferenceMode
        ''' <summary>Use the arithmetic mean of the observed non-missing values.</summary>
        ObservedMean = 0

        ''' <summary>Use a caller-specified value.</summary>
        UserSpecified = 1
    End Enum


    ''' <summary>
    ''' Multiple-comparison adjustment applied to a family of p-values.
    ''' </summary>
    Public Enum MixedModelMultiplicityAdjustment
        None = 0
        Bonferroni = 1
        Holm = 2
        Sidak = 3
    End Enum


    ''' <summary>
    ''' One numerical class/factor variable used in a reference grid.
    ''' </summary>
    ''' <remarks>
    ''' The current implementation is intentionally numerical-code based.  This is a
    ''' good fit for BESHStatNG's current mixed-model matrix engine and for UDF use.
    ''' For example, use variables such as <c>treatment_active</c>,
    ''' <c>sex_code</c>, <c>visit</c>, or <c>site_code</c>.
    ''' </remarks>
    Public Class MixedModelReferenceGridFactor

        Public Property Name As String = String.Empty
        Public Property Values As Double() = Nothing
        Public Property ObservedValues As Double() = Nothing

        Public Sub New()
        End Sub

        Public Sub New(name As String,
                       values() As Double,
                       observedValues() As Double)
            Me.Name = If(name, String.Empty)
            Me.Values = If(values Is Nothing, Nothing, CType(values.Clone(), Double()))
            Me.ObservedValues = If(observedValues Is Nothing, Nothing, CType(observedValues.Clone(), Double()))
        End Sub

    End Class


    ''' <summary>
    ''' One continuous covariate setting used while constructing a reference grid.
    ''' </summary>
    Public Class MixedModelReferenceGridCovariate

        Public Property Name As String = String.Empty
        Public Property ObservedValues As Double() = Nothing
        Public Property Mode As MixedModelCovariateReferenceMode = MixedModelCovariateReferenceMode.ObservedMean
        Public Property UserValue As Double = Double.NaN

        Public Sub New()
        End Sub

        Public Sub New(name As String,
                       observedValues() As Double,
                       mode As MixedModelCovariateReferenceMode,
                       Optional userValue As Double = Double.NaN)
            Me.Name = If(name, String.Empty)
            Me.ObservedValues = If(observedValues Is Nothing, Nothing, CType(observedValues.Clone(), Double()))
            Me.Mode = mode
            Me.UserValue = userValue
        End Sub

        Public Function ReferenceValue() As Double
            If Mode = MixedModelCovariateReferenceMode.UserSpecified Then Return UserValue
            Return ObservedValues.Average()
        End Function

    End Class


    ''' <summary>
    ''' Reference-grid specification for mixed-model LS-means and contrasts.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The reference-grid service constructs model rows <c>L</c> and evaluates
    ''' <c>L * beta</c>.  It is deliberately independent of the MMRM GUI so it can be
    ''' reused by UDFs and tests.
    ''' </para>
    ''' <para>
    ''' The first implementation supports numerical fixed-effect columns and
    ''' numerical class-factor codes.  Fixed-effect names are matched by name, and
    ''' interactions are recognized with the <c>:</c> separator, for example
    ''' <c>treatment_active:age_centered_8</c>.
    ''' </para>
    ''' </remarks>
    Public Class MixedModelReferenceGridSpec

        Public Property FixedEffectNames As String() = Nothing
        Public Property ByFactors As New List(Of MixedModelReferenceGridFactor)()
        Public Property MarginalFactors As New List(Of MixedModelReferenceGridFactor)()
        Public Property Covariates As New List(Of MixedModelReferenceGridCovariate)()

        Public Property Weighting As MixedModelReferenceGridWeighting = MixedModelReferenceGridWeighting.EqualCells
        Public Property MultiplicityAdjustment As MixedModelMultiplicityAdjustment = MixedModelMultiplicityAdjustment.None
        Public Property Alpha As Double = 0.05
        Public Property IncludeIntercept As Boolean = True

        Public Function AddByFactor(name As String,
                                    values() As Double,
                                    observedValues() As Double) As MixedModelReferenceGridSpec
            Me.ByFactors.Add(New MixedModelReferenceGridFactor(name, values, observedValues))
            Return Me
        End Function

        Public Function AddMarginalFactor(name As String,
                                          values() As Double,
                                          observedValues() As Double) As MixedModelReferenceGridSpec
            Me.MarginalFactors.Add(New MixedModelReferenceGridFactor(name, values, observedValues))
            Return Me
        End Function

        Public Function AddCovariateMean(name As String,
                                         observedValues() As Double) As MixedModelReferenceGridSpec
            Me.Covariates.Add(New MixedModelReferenceGridCovariate(name,
                                                                   observedValues,
                                                                   MixedModelCovariateReferenceMode.ObservedMean))
            Return Me
        End Function

        Public Function AddCovariateValue(name As String,
                                          observedValues() As Double,
                                          value As Double) As MixedModelReferenceGridSpec
            Me.Covariates.Add(New MixedModelReferenceGridCovariate(name,
                                                                   observedValues,
                                                                   MixedModelCovariateReferenceMode.UserSpecified,
                                                                   value))
            Return Me
        End Function

    End Class


    ''' <summary>
    ''' One row of a reference grid, represented as an L vector aligned with beta.
    ''' </summary>
    Public Class MixedModelReferenceGridRow

        Public Property Label As String = String.Empty
        Public Property L As Double() = Nothing
        Public Property Count As Integer = 0
        Public Property WeightSum As Double = 0.0

        Public Property Profile As Dictionary(Of String, Double) =
            New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)

    End Class


    ''' <summary>
    ''' Reference-grid construction and post-estimation utilities for mixed models.
    ''' </summary>
    Public Module MixedModelReferenceGridService

        ''' <summary>
        ''' Builds reference-grid rows for the supplied specification.
        ''' </summary>
        Public Function BuildReferenceGridRows(spec As MixedModelReferenceGridSpec) As List(Of MixedModelReferenceGridRow)
            ValidateSpec(spec)

            Dim byProfiles As List(Of Dictionary(Of String, Double)) =
                BuildProfileCombinations(spec.ByFactors)

            If byProfiles.Count = 0 Then
                byProfiles.Add(New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase))
            End If

            Dim marginalProfiles As List(Of Dictionary(Of String, Double)) =
                BuildProfileCombinations(spec.MarginalFactors)

            If marginalProfiles.Count = 0 Then
                marginalProfiles.Add(New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase))
            End If

            Dim rows As New List(Of MixedModelReferenceGridRow)()

            For Each byProfile As Dictionary(Of String, Double) In byProfiles
                Dim sumRow(spec.FixedEffectNames.Length - 1) As Double
                Dim totalWeight As Double = 0.0
                Dim totalCount As Integer = 0

                For Each marginalProfile As Dictionary(Of String, Double) In marginalProfiles
                    Dim profile As Dictionary(Of String, Double) = MergeProfiles(byProfile, marginalProfile)

                    For Each cov As MixedModelReferenceGridCovariate In spec.Covariates
                        profile(cov.Name) = cov.ReferenceValue()
                    Next

                    Dim cellCount As Integer = CountObservedRows(spec, profile)
                    Dim weight As Double = ResolveCellWeight(spec.Weighting, cellCount)

                    If weight <= 0.0 Then Continue For

                    Dim l() As Double = BuildDesignRow(spec.FixedEffectNames,
                                                       profile,
                                                       spec.IncludeIntercept)

                    For j As Integer = 0 To l.Length - 1
                        sumRow(j) += weight * l(j)
                    Next

                    totalWeight += weight
                    totalCount += cellCount
                Next

                If totalWeight <= 0.0 Then Continue For

                For j As Integer = 0 To sumRow.Length - 1
                    sumRow(j) /= totalWeight
                Next

                rows.Add(New MixedModelReferenceGridRow With {
                        .Label = BuildProfileLabel(byProfile),
                        .L = sumRow,
                        .Count = totalCount,
                        .WeightSum = totalWeight,
                        .Profile = New Dictionary(Of String, Double)(byProfile, StringComparer.OrdinalIgnoreCase)
                    })
            Next

            Return rows
        End Function


        ''' <summary>
        ''' Builds an estimated-marginal-means table for reference-grid rows.
        ''' </summary>
        Public Function BuildEstimatedMeansTable(title As String,
                                                 rows As List(Of MixedModelReferenceGridRow),
                                                 result As MixedModelResult,
                                                 spec As MixedModelReferenceGridSpec) As Global.BESHStatNG.ResultTable
            If rows Is Nothing OrElse rows.Count = 0 OrElse result Is Nothing OrElse spec Is Nothing Then Return Nothing

            Dim labels() As String = rows.Select(Function(r) r.Label).ToArray()
            Dim lRows As New List(Of Double())()
            Dim counts As New List(Of Integer)()

            For Each r As MixedModelReferenceGridRow In rows
                lRows.Add(r.L)
                counts.Add(r.Count)
            Next

            Dim t As Global.BESHStatNG.ResultTable =
                MixedModelPostEstimation.BuildLinearEstimateResultTable(
                    title:=If(String.IsNullOrWhiteSpace(title), "Reference-grid estimated marginal means", title),
                    rowLabels:=labels,
                    lRows:=lRows,
                    counts:=counts,
                    result:=result,
                    alpha:=spec.Alpha,
                    footnote:=BuildReferenceGridFootnote(spec))

            Return t
        End Function


        ''' <summary>
        ''' Builds all pairwise contrasts for one by-factor while holding the other
        ''' by-factors fixed.
        ''' </summary>
        Public Function BuildPairwiseContrastsByFactor(rows As List(Of MixedModelReferenceGridRow),
                                                       result As MixedModelResult,
                                                       spec As MixedModelReferenceGridSpec,
                                                       factorName As String,
                                                       Optional title As String = "") As Global.BESHStatNG.ResultTable
            If rows Is Nothing OrElse rows.Count = 0 OrElse result Is Nothing OrElse spec Is Nothing Then Return Nothing
            If String.IsNullOrWhiteSpace(factorName) Then Return Nothing

            Dim labels As New List(Of String)()
            Dim lRows As New List(Of Double())()

            For i As Integer = 0 To rows.Count - 2
                For j As Integer = i + 1 To rows.Count - 1
                    If Not SameProfileExcept(rows(i).Profile, rows(j).Profile, factorName) Then Continue For
                    If Not rows(i).Profile.ContainsKey(factorName) OrElse Not rows(j).Profile.ContainsKey(factorName) Then Continue For

                    Dim a As Double = rows(i).Profile(factorName)
                    Dim b As Double = rows(j).Profile(factorName)

                    Dim first As MixedModelReferenceGridRow = rows(i)
                    Dim second As MixedModelReferenceGridRow = rows(j)

                    If a > b Then
                        first = rows(j)
                        second = rows(i)
                    End If

                    labels.Add(factorName & "=" & MixedModelPostEstimation.FormatProfileValue(second.Profile(factorName)) &
                               " - " & factorName & "=" & MixedModelPostEstimation.FormatProfileValue(first.Profile(factorName)) &
                               OtherProfileSuffix(first.Profile, factorName))

                    lRows.Add(Matrix.M_SUB(second.L, first.L))
                Next
            Next

            If lRows.Count = 0 Then Return Nothing

            Return BuildLinearContrastTableWithMultiplicity(
                title:=If(String.IsNullOrWhiteSpace(title), "Reference-grid pairwise contrasts", title),
                rowLabels:=labels.ToArray(),
                lRows:=lRows,
                result:=result,
                alpha:=spec.Alpha,
                adjustment:=spec.MultiplicityAdjustment,
                footnote:=BuildReferenceGridFootnote(spec))
        End Function


        ''' <summary>
        ''' Builds a contrast table with raw and multiplicity-adjusted p-values.
        ''' </summary>
        Public Function BuildLinearContrastTableWithMultiplicity(title As String,
                                                         rowLabels() As String,
                                                         lRows As List(Of Double()),
                                                         result As MixedModelResult,
                                                         alpha As Double,
                                                         adjustment As MixedModelMultiplicityAdjustment,
                                                         footnote As String) As Global.BESHStatNG.ResultTable
            If rowLabels Is Nothing OrElse lRows Is Nothing OrElse result Is Nothing Then Return Nothing
            If rowLabels.Length <> lRows.Count Then Return Nothing

            Dim alphaUse As Double = AppInfrastructure.NormalizeAlpha(alpha)
            Dim n As Integer = lRows.Count
            Dim rawP(n - 1) As Double
            Dim estimates(n - 1) As Double
            Dim ses(n - 1) As Double
            Dim dfs(n - 1) As Double
            Dim stats(n - 1) As Double
            Dim lo(n - 1) As Double
            Dim hi(n - 1) As Double

            For i As Integer = 0 To n - 1
                Dim diag As String = Nothing

                MixedModelPostEstimation.TryLinearInference(result,
                                                    rowLabels(i),
                                                    lRows(i),
                                                    alphaUse,
                                                    estimates(i),
                                                    ses(i),
                                                    dfs(i),
                                                    stats(i),
                                                    rawP(i),
                                                    lo(i),
                                                    hi(i),
                                                    diag)
            Next

            Dim adjP() As Double = AdjustPValues(rawP, adjustment)
            Dim body(n - 1, 7) As Object

            For i As Integer = 0 To n - 1
                body(i, 0) = estimates(i)
                body(i, 1) = ses(i)
                body(i, 2) = If(AppInfrastructure.IsFinite(dfs(i)) AndAlso dfs(i) > 0.0, CType(dfs(i), Object), String.Empty)
                body(i, 3) = stats(i)
                body(i, 4) = rawP(i)
                body(i, 5) = adjP(i)
                body(i, 6) = lo(i)
                body(i, 7) = hi(i)
            Next

            Dim statLabel As String = If(String.IsNullOrWhiteSpace(result.BetaStatisticLabel), "z", result.BetaStatisticLabel)
            Dim pLabel As String = If(String.IsNullOrWhiteSpace(result.BetaPValueLabel), "Pr(>|z|)", result.BetaPValueLabel)
            Dim ciLabel As String = Format((1.0 - alphaUse) * 100.0, "0.###") & "% CI"

            Dim t As New Global.BESHStatNG.ResultTable
            t.AddTitle(title)
            t.SetBody(body)
            t.AddHeaderTopRow({"Estimate", "Std. Error", "DF", statLabel, pLabel,
                       "Adjusted p (" & MultiplicityLabel(adjustment) & ")",
                       "Lower " & ciLabel, "Upper " & ciLabel})
            t.AddHeaderLeftRow(rowLabels)

            ' P-value columns are final-table column indices with one left-header column.
            t.AddPvalueToFormat(5)
            t.AddPvalueToFormat(6)

            If Not String.IsNullOrWhiteSpace(footnote) Then t.AddFootnote(footnote)
            t.AddFootnote("Multiplicity adjustment: " & MultiplicityLabel(adjustment) & ". Confidence limits are unadjusted.")

            If result.FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger Then
                t.AddFootnote("Reference-grid contrasts use KR-adjusted standard errors and R mmrm-style one-dimensional Kenward-Roger denominator DF.")
            End If

            Return t
        End Function


        ''' <summary>
        ''' Applies a multiplicity adjustment to a vector of p-values.
        ''' </summary>
        Public Function AdjustPValues(pValues() As Double, adjustment As MixedModelMultiplicityAdjustment) As Double()
            If pValues Is Nothing Then Return Nothing

            Dim n As Integer = pValues.Length
            Dim out(n - 1) As Double

            Select Case adjustment
                Case MixedModelMultiplicityAdjustment.None
                    For i As Integer = 0 To n - 1
                        out(i) = AppInfrastructure.ClampProbability(pValues(i))
                    Next

                Case MixedModelMultiplicityAdjustment.Bonferroni
                    For i As Integer = 0 To n - 1
                        out(i) = AppInfrastructure.ClampProbability(pValues(i) * CDbl(n))
                    Next

                Case MixedModelMultiplicityAdjustment.Sidak
                    For i As Integer = 0 To n - 1
                        Dim p As Double = AppInfrastructure.ClampProbability(pValues(i))
                        out(i) = AppInfrastructure.ClampProbability(1.0 - Math.Pow(1.0 - p, CDbl(n)))
                    Next

                Case MixedModelMultiplicityAdjustment.Holm
                    Dim idx As Integer() = Enumerable.Range(0, n).OrderBy(Function(i) AppInfrastructure.ClampProbability(pValues(i))).ToArray()
                    Dim adjustedSorted(n - 1) As Double
                    Dim runningMax As Double = 0.0

                    For rank As Integer = 0 To n - 1
                        Dim originalIndex As Integer = idx(rank)
                        Dim candidate As Double = AppInfrastructure.ClampProbability(CDbl(n - rank) * AppInfrastructure.ClampProbability(pValues(originalIndex)))

                        If candidate < runningMax Then candidate = runningMax
                        runningMax = candidate

                        adjustedSorted(rank) = AppInfrastructure.ClampProbability(candidate)
                    Next

                    For rank As Integer = 0 To n - 1
                        out(idx(rank)) = adjustedSorted(rank)
                    Next

                Case Else
                    For i As Integer = 0 To n - 1
                        out(i) = AppInfrastructure.ClampProbability(pValues(i))
                    Next
            End Select

            Return out
        End Function


        ''' <summary>
        ''' Builds one design row from fixed-effect names and profile values.
        ''' </summary>
        Public Function BuildDesignRow(fixedEffectNames() As String,
                                       profile As Dictionary(Of String, Double),
                                       Optional includeIntercept As Boolean = True) As Double()
            If fixedEffectNames Is Nothing Then Return Nothing
            If profile Is Nothing Then profile = New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)

            Dim l(fixedEffectNames.Length - 1) As Double

            For j As Integer = 0 To fixedEffectNames.Length - 1
                Dim name As String = If(fixedEffectNames(j), String.Empty).Trim()

                If IsInterceptName(name) Then
                    l(j) = If(includeIntercept, 1.0, 0.0)
                Else
                    l(j) = EvaluateTerm(name, profile)
                End If
            Next

            Return l
        End Function

        ''' <summary>
        ''' Normalizes a raw variable key to the name used in reference-grid profile
        ''' dictionaries and fixed-effect column matching.
        ''' </summary>
        Public Function NormalizeProfileName(rawKey As String) As String
            Return RegressionDesignCore.GetCoefBaseName(If(rawKey, String.Empty)).Trim()
        End Function


        ''' <summary>
        ''' Returns sorted unique finite levels for a factor contained in reference-grid rows.
        ''' </summary>
        Public Function GetLevelsFromRows(rows As List(Of MixedModelReferenceGridRow),
                                  factorName As String) As Double()
            If rows Is Nothing OrElse String.IsNullOrWhiteSpace(factorName) Then Return Array.Empty(Of Double)()

            Dim vals As New List(Of Double)()

            For Each row As MixedModelReferenceGridRow In rows
                If row Is Nothing OrElse row.Profile Is Nothing Then Continue For
                If Not row.Profile.ContainsKey(factorName) Then Continue For

                Dim v As Double = row.Profile(factorName)
                If Not AppInfrastructure.IsFinite(v) Then Continue For

                Dim found As Boolean = False
                For Each existing As Double In vals
                    If MixedModelPostEstimation.NearlyEqual(existing, v) Then
                        found = True
                        Exit For
                    End If
                Next

                If Not found Then vals.Add(v)
            Next

            vals.Sort()
            Return vals.ToArray()
        End Function


        ''' <summary>
        ''' Finds the row whose factor level equals <paramref name="targetFactorLevel"/>
        ''' and whose other profile variables match <paramref name="profile"/>.
        ''' </summary>
        Public Function FindMatchingReferenceGridRow(rows As List(Of MixedModelReferenceGridRow),
                                             profile As Dictionary(Of String, Double),
                                             factorName As String,
                                             targetFactorLevel As Double) As MixedModelReferenceGridRow
            If rows Is Nothing OrElse profile Is Nothing OrElse String.IsNullOrWhiteSpace(factorName) Then Return Nothing
            If Not AppInfrastructure.IsFinite(targetFactorLevel) Then Return Nothing

            For Each row As MixedModelReferenceGridRow In rows
                If row Is Nothing OrElse row.Profile Is Nothing Then Continue For
                If Not row.Profile.ContainsKey(factorName) Then Continue For
                If Not MixedModelPostEstimation.NearlyEqual(row.Profile(factorName), targetFactorLevel) Then Continue For

                Dim same As Boolean = True

                For Each kvp As KeyValuePair(Of String, Double) In profile
                    If String.Equals(kvp.Key, factorName, StringComparison.OrdinalIgnoreCase) Then Continue For

                    If Not row.Profile.ContainsKey(kvp.Key) Then
                        same = False
                        Exit For
                    End If

                    If Not MixedModelPostEstimation.NearlyEqual(row.Profile(kvp.Key), kvp.Value) Then
                        same = False
                        Exit For
                    End If
                Next

                If same Then Return row
            Next

            Return Nothing
        End Function


        ''' <summary>
        ''' Builds controlled contrasts for a reference-grid factor.
        ''' </summary>
        ''' <remarks>
        ''' This function is intentionally UI-agnostic.  The caller decides the control
        ''' level, whether only a single comparison level should be used, and the direction
        ''' labels.  It can be reused by Ui18MMRM, UDFs, and tests.
        ''' </remarks>
        Public Function BuildContrastsAgainstControlByFactor(rows As List(Of MixedModelReferenceGridRow),
                                                     result As MixedModelResult,
                                                     spec As MixedModelReferenceGridSpec,
                                                     factorName As String,
                                                     controlLevel As Double,
                                                     Optional comparisonLevel As Double = Double.NaN,
                                                     Optional useSingleComparison As Boolean = False,
                                                     Optional direction As String = "",
                                                     Optional treatmentMinusControlText As String = "Treatment - Control",
                                                     Optional controlMinusTreatmentText As String = "Control - Treatment",
                                                     Optional title As String = "Reference-grid contrasts") As Global.BESHStatNG.ResultTable
            If rows Is Nothing OrElse rows.Count = 0 OrElse result Is Nothing OrElse spec Is Nothing Then Return Nothing
            If String.IsNullOrWhiteSpace(factorName) Then Return Nothing
            If Not AppInfrastructure.IsFinite(controlLevel) Then Return Nothing
            If useSingleComparison AndAlso Not AppInfrastructure.IsFinite(comparisonLevel) Then Return Nothing

            Dim labels As New List(Of String)()
            Dim lRows As New List(Of Double())()

            For Each candidate As MixedModelReferenceGridRow In rows
                If candidate Is Nothing OrElse candidate.Profile Is Nothing OrElse candidate.L Is Nothing Then Continue For
                If Not candidate.Profile.ContainsKey(factorName) Then Continue For

                Dim candidateLevel As Double = candidate.Profile(factorName)

                If Not AppInfrastructure.IsFinite(candidateLevel) Then Continue For
                If MixedModelPostEstimation.NearlyEqual(candidateLevel, controlLevel) Then Continue For

                If useSingleComparison AndAlso Not MixedModelPostEstimation.NearlyEqual(candidateLevel, comparisonLevel) Then Continue For

                Dim controlRow As MixedModelReferenceGridRow =
            FindMatchingReferenceGridRow(rows, candidate.Profile, factorName, controlLevel)

                If controlRow Is Nothing OrElse controlRow.L Is Nothing Then Continue For

                Dim lDiff() As Double =
            MixedModelPostEstimation.MakeDirectedDifference(candidate.L,
                                                            controlRow.L,
                                                            direction,
                                                            treatmentMinusControlText,
                                                            controlMinusTreatmentText)

                labels.Add(MixedModelPostEstimation.DirectedComparisonLabel(factorName,
                                                                    candidateLevel,
                                                                    controlLevel,
                                                                    direction,
                                                                    treatmentMinusControlText,
                                                                    controlMinusTreatmentText:=controlMinusTreatmentText) &
                   OtherProfileSuffix(candidate.Profile, factorName))

                lRows.Add(lDiff)
            Next

            If lRows.Count = 0 Then Return Nothing

            Return BuildLinearContrastTableWithMultiplicity(
                        title:=If(String.IsNullOrWhiteSpace(title), "Reference-grid contrasts", title),
                        rowLabels:=labels.ToArray(),
                        lRows:=lRows,
                        result:=result,
                        alpha:=spec.Alpha,
                        adjustment:=spec.MultiplicityAdjustment,
                        footnote:=BuildReferenceGridFootnote(spec))
        End Function

        Private Function EvaluateTerm(term As String, profile As Dictionary(Of String, Double)) As Double
            If String.IsNullOrWhiteSpace(term) Then Return 0.0

            Dim parts() As String = term.Split(":"c)
            Dim value As Double = 1.0

            For Each raw As String In parts
                Dim part As String = raw.Trim()
                If part.Length = 0 Then Return 0.0

                value *= EvaluateAtomicTerm(part, profile)
            Next

            Return value
        End Function


        Private Function EvaluateAtomicTerm(part As String,
                                    profile As Dictionary(Of String, Double)) As Double
            Dim v As Double = 0.0

            If profile.TryGetValue(part, v) Then Return v

            Dim eqPos As Integer = part.IndexOf("="c)
            If eqPos > 0 Then
                Dim variableName As String = part.Substring(0, eqPos).Trim()
                Dim levelText As String = part.Substring(eqPos + 1).Trim()

                If profile.TryGetValue(variableName, v) Then
                    Dim levelValue As Double
                    If Double.TryParse(levelText, NumberStyles.Float, CultureInfo.InvariantCulture, levelValue) Then
                        Return If(MixedModelPostEstimation.NearlyEqual(v, levelValue), 1.0, 0.0)
                    End If
                End If
            End If

            Dim lb As Integer = part.LastIndexOf("["c)
            Dim rb As Integer = part.LastIndexOf("]"c)

            If lb > 0 AndAlso rb > lb Then
                Dim variableName As String = part.Substring(0, lb).Trim()
                Dim levelText As String = part.Substring(lb + 1, rb - lb - 1).Trim()

                If profile.TryGetValue(variableName, v) Then
                    Dim levelValue As Double
                    If Double.TryParse(levelText, NumberStyles.Float, CultureInfo.InvariantCulture, levelValue) Then
                        Return If(MixedModelPostEstimation.NearlyEqual(v, levelValue), 1.0, 0.0)
                    End If
                End If
            End If

            ' Unknown columns are treated as zero.  This is the correct behavior for
            ' dummy columns not active in the current profile.  Tests should cover the
            ' fixed-effect names used by each model.
            Return 0.0
        End Function


        Private Function BuildProfileCombinations(factors As List(Of MixedModelReferenceGridFactor)) As List(Of Dictionary(Of String, Double))
            Dim out As New List(Of Dictionary(Of String, Double))()
            BuildProfileCombinationsRecursive(factors, 0, New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase), out)
            Return out
        End Function


        Private Sub BuildProfileCombinationsRecursive(factors As List(Of MixedModelReferenceGridFactor),
                                                      index As Integer,
                                                      current As Dictionary(Of String, Double),
                                                      out As List(Of Dictionary(Of String, Double)))
            If factors Is Nothing OrElse index >= factors.Count Then
                out.Add(New Dictionary(Of String, Double)(current, StringComparer.OrdinalIgnoreCase))
                Exit Sub
            End If

            Dim f As MixedModelReferenceGridFactor = factors(index)
            If f.Values Is Nothing OrElse f.Values.Length = 0 Then Exit Sub

            For Each v As Double In f.Values
                current(f.Name) = v
                BuildProfileCombinationsRecursive(factors, index + 1, current, out)
            Next

            current.Remove(f.Name)
        End Sub


        Private Function MergeProfiles(a As Dictionary(Of String, Double), b As Dictionary(Of String, Double)) As Dictionary(Of String, Double)
            Dim out As New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)

            If a IsNot Nothing Then
                For Each kvp As KeyValuePair(Of String, Double) In a
                    out(kvp.Key) = kvp.Value
                Next
            End If

            If b IsNot Nothing Then
                For Each kvp As KeyValuePair(Of String, Double) In b
                    out(kvp.Key) = kvp.Value
                Next
            End If

            Return out
        End Function


        Private Function CountObservedRows(spec As MixedModelReferenceGridSpec, profile As Dictionary(Of String, Double)) As Integer
            Dim allFactors As New List(Of MixedModelReferenceGridFactor)()
            allFactors.AddRange(spec.ByFactors)
            allFactors.AddRange(spec.MarginalFactors)

            If allFactors.Count = 0 Then Return 0

            Dim n As Integer = -1
            For Each f As MixedModelReferenceGridFactor In allFactors
                If f.ObservedValues Is Nothing Then Continue For
                If n < 0 Then n = f.ObservedValues.Length
                If f.ObservedValues.Length <> n Then
                    Throw New ArgumentException("All observed factor arrays must have the same length.")
                End If
            Next

            If n < 0 Then Return 0

            Dim count As Integer = 0

            For i As Integer = 0 To n - 1
                Dim ok As Boolean = True

                For Each f As MixedModelReferenceGridFactor In allFactors
                    If f.ObservedValues Is Nothing Then Continue For
                    If Not profile.ContainsKey(f.Name) Then Continue For

                    If Not MixedModelPostEstimation.NearlyEqual(f.ObservedValues(i), profile(f.Name)) Then
                        ok = False
                        Exit For
                    End If
                Next

                If ok Then count += 1
            Next

            Return count
        End Function


        Private Function ResolveCellWeight(weighting As MixedModelReferenceGridWeighting, observedCount As Integer) As Double
            If weighting = MixedModelReferenceGridWeighting.EqualCells Then
                Return If(observedCount > 0, 1.0, 0.0)
            End If

            Return CDbl(Math.Max(0, observedCount))
        End Function


        Private Function SameProfileExcept(a As Dictionary(Of String, Double), b As Dictionary(Of String, Double), exceptName As String) As Boolean
            If a Is Nothing OrElse b Is Nothing Then Return False

            For Each kvp As KeyValuePair(Of String, Double) In a
                If String.Equals(kvp.Key, exceptName, StringComparison.OrdinalIgnoreCase) Then Continue For
                If Not b.ContainsKey(kvp.Key) Then Return False
                If Not MixedModelPostEstimation.NearlyEqual(kvp.Value, b(kvp.Key)) Then Return False
            Next

            For Each kvp As KeyValuePair(Of String, Double) In b
                If String.Equals(kvp.Key, exceptName, StringComparison.OrdinalIgnoreCase) Then Continue For
                If Not a.ContainsKey(kvp.Key) Then Return False
            Next

            Return True
        End Function


        Private Function BuildProfileLabel(profile As Dictionary(Of String, Double)) As String
            If profile Is Nothing OrElse profile.Count = 0 Then Return "Reference grid mean"

            Dim parts As New List(Of String)()
            For Each kvp As KeyValuePair(Of String, Double) In profile
                parts.Add(kvp.Key & "=" & MixedModelPostEstimation.FormatProfileValue(kvp.Value))
            Next

            Return String.Join(", ", parts)
        End Function


        Private Function OtherProfileSuffix(profile As Dictionary(Of String, Double), factorName As String) As String
            If profile Is Nothing Then Return String.Empty

            Dim parts As New List(Of String)()

            For Each kvp As KeyValuePair(Of String, Double) In profile
                If String.Equals(kvp.Key, factorName, StringComparison.OrdinalIgnoreCase) Then Continue For
                parts.Add(kvp.Key & "=" & MixedModelPostEstimation.FormatProfileValue(kvp.Value))
            Next

            If parts.Count = 0 Then Return String.Empty
            Return " | " & String.Join(", ", parts)
        End Function


        Private Function MultiplicityLabel(adjustment As MixedModelMultiplicityAdjustment) As String
            Select Case adjustment
                Case MixedModelMultiplicityAdjustment.Bonferroni
                    Return "Bonferroni"
                Case MixedModelMultiplicityAdjustment.Holm
                    Return "Holm"
                Case MixedModelMultiplicityAdjustment.Sidak
                    Return "Sidak"
                Case Else
                    Return "none"
            End Select
        End Function


        Private Function BuildReferenceGridFootnote(spec As MixedModelReferenceGridSpec) As String
            Dim covParts As New List(Of String)()

            For Each cov As MixedModelReferenceGridCovariate In spec.Covariates
                Dim modeText As String = If(cov.Mode = MixedModelCovariateReferenceMode.UserSpecified,
                                            "user value",
                                            "observed mean")
                covParts.Add(cov.Name & "=" & MixedModelPostEstimation.FormatProfileValue(cov.ReferenceValue()) & " (" & modeText & ")")
            Next

            Dim covText As String = If(covParts.Count = 0,
                                       "No continuous covariates were set by the reference-grid specification.",
                                       "Continuous covariates: " & String.Join("; ", covParts) & ".")

            Dim weightText As String = If(spec.Weighting = MixedModelReferenceGridWeighting.EqualCells,
                                          "Equal weighting over non-empty marginal class cells.",
                                          "Observed-cell-frequency weighting over marginal class cells.")

            Return "Reference-grid LS-means use numerical fixed-effect columns and L*beta. " &
                   weightText & " " & covText
        End Function

        Private Function IsInterceptName(name As String) As Boolean
            If String.IsNullOrWhiteSpace(name) Then Return False

            Dim s As String = name.Trim()
            Return String.Equals(s, "Intercept", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(s, "(Intercept)", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(s, "Constant", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(s, "Const", StringComparison.OrdinalIgnoreCase)
        End Function


        Private Sub ValidateSpec(spec As MixedModelReferenceGridSpec)
            If spec Is Nothing Then Throw New ArgumentNullException(NameOf(spec))
            If spec.FixedEffectNames Is Nothing OrElse spec.FixedEffectNames.Length = 0 Then
                Throw New ArgumentException("Reference-grid specification requires FixedEffectNames.")
            End If

            Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            For Each f As MixedModelReferenceGridFactor In spec.ByFactors
                If f Is Nothing OrElse String.IsNullOrWhiteSpace(f.Name) Then Throw New ArgumentException("All by-factors require names.")
                If f.Values Is Nothing OrElse f.Values.Length = 0 Then Throw New ArgumentException("By-factor '" & f.Name & "' has no values.")
                If names.Contains(f.Name) Then Throw New ArgumentException("Duplicate reference-grid variable '" & f.Name & "'.")
                names.Add(f.Name)
            Next

            For Each f As MixedModelReferenceGridFactor In spec.MarginalFactors
                If f Is Nothing OrElse String.IsNullOrWhiteSpace(f.Name) Then Throw New ArgumentException("All marginal factors require names.")
                If f.Values Is Nothing OrElse f.Values.Length = 0 Then Throw New ArgumentException("Marginal factor '" & f.Name & "' has no values.")
                If names.Contains(f.Name) Then Throw New ArgumentException("Duplicate reference-grid variable '" & f.Name & "'.")
                names.Add(f.Name)
            Next

            For Each c As MixedModelReferenceGridCovariate In spec.Covariates
                If c Is Nothing OrElse String.IsNullOrWhiteSpace(c.Name) Then Throw New ArgumentException("All covariates require names.")
                If names.Contains(c.Name) Then Throw New ArgumentException("Duplicate reference-grid variable '" & c.Name & "'.")
                names.Add(c.Name)

                Dim v As Double = c.ReferenceValue()
                If Not AppInfrastructure.IsFinite(v) Then Throw New ArgumentException("Covariate '" & c.Name & "' reference value is not finite.")
            Next
        End Sub

    End Module

End Namespace
