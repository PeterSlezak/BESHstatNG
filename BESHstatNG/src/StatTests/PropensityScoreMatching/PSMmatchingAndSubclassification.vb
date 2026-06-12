Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.Multivariate

Namespace CausalInference

    ''' <summary>
    ''' Greedy nearest-neighbour matching backend with 1:k ratios, optional replacement, exact strata,
    ''' common-support trimming, raw/logit calipers, and Mahalanobis distance.
    ''' </summary>
    Public NotInheritable Class PsmMatchingEngine
        Private Sub New()
        End Sub

        Public Shared Function BuildObservations(input As PsmInputData, scoreModel As PsmScoreModelResult, options As PsmOptions) As List(Of PsmObservation)
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If scoreModel Is Nothing OrElse scoreModel.Scores Is Nothing Then Throw New ArgumentNullException("scoreModel")
            If scoreModel.Scores.Length <> input.RowCount Then Throw New ArgumentException("Score count must match input row count.")

            Dim observations As New List(Of PsmObservation)()
            For i As Integer = 0 To input.RowCount - 1
                Dim cov(input.CovariateCount - 1) As Double
                For j As Integer = 0 To input.CovariateCount - 1
                    cov(j) = input.Covariates(i, j)
                Next
                Dim p As Double = PsmMath.Clamp(scoreModel.Scores(i), 0.00000001, 0.99999999)
                observations.Add(New PsmObservation With {
                    .RowIndex = i,
                    .Id = input.GetId(i),
                    .Treated = input.Treatment(i) >= 0.5,
                    .Outcome = If(input.Outcome Is Nothing, Double.NaN, input.Outcome(i)),
                    .Covariates = cov,
                    .PropensityScore = p,
                    .LogitPropensityScore = If(scoreModel.LinearPredictor IsNot Nothing AndAlso scoreModel.LinearPredictor.Length = input.RowCount, scoreModel.LinearPredictor(i), PsmMath.SafeLogit(p)),
                    .ExactGroupLabel = If(input.ExactGroupLabels Is Nothing, "", If(input.ExactGroupLabels(i), ""))
                })
            Next

            ApplyTrimming(observations, options)
            ApplyCommonSupport(observations, options)
            Return observations
        End Function

        Public Shared Function MatchNearestNeighbors(input As PsmInputData, observations As List(Of PsmObservation), options As PsmOptions) As List(Of PsmMatchLink)
            If observations Is Nothing Then Throw New ArgumentNullException("observations")
            If options Is Nothing Then Throw New ArgumentNullException("options")
            If options.Estimand <> PsmEstimand.ATT AndAlso options.Estimand <> PsmEstimand.ATC Then Throw New ArgumentException("Nearest-neighbour matching currently supports ATT and ATC. Use weighting or subclassification for ATE/ATO.")

            Dim focalIsTreated As Boolean = (options.Estimand = PsmEstimand.ATT)
            Dim focal As List(Of PsmObservation) = observations.Where(Function(o) o.Treated = focalIsTreated AndAlso o.IncludedByCommonSupport AndAlso o.IncludedByTrimming).ToList()
            Dim candidates As List(Of PsmObservation) = observations.Where(Function(o) o.Treated <> focalIsTreated AndAlso o.IncludedByCommonSupport AndAlso o.IncludedByTrimming).ToList()
            If focal.Count = 0 OrElse candidates.Count = 0 Then Return New List(Of PsmMatchLink)()

            focal = OrderFocalObservations(focal, candidates, options)
            Dim available As New HashSet(Of Integer)(candidates.Select(Function(o) o.RowIndex))
            Dim matches As New List(Of PsmMatchLink)()
            Dim setId As Integer = 0
            Dim invCov As Double(,) = Nothing
            If options.DistanceMetric = PsmDistanceMetric.Mahalanobis OrElse options.DistanceMetric = PsmDistanceMetric.MahalanobisWithinPropensityCaliper Then invCov = BuildInverseCovariance(observations, input.CovariateCount)

            Dim caliperDenom As Double = CaliperDenominator(observations, options)

            For Each f In focal
                Dim eligible As New List(Of Tuple(Of PsmObservation, Double, Double, Double))()
                For Each c In candidates
                    If Not options.WithReplacement AndAlso Not available.Contains(c.RowIndex) Then Continue For
                    If Not String.Equals(f.ExactGroupLabel, c.ExactGroupLabel, StringComparison.Ordinal) Then Continue For
                    Dim psDistance As Double = PropensityDistance(f, c, options)
                    If Not PassesCaliper(f, c, options, caliperDenom) Then Continue For
                    Dim mahal As Double = Double.NaN
                    Dim distance As Double
                    If options.DistanceMetric = PsmDistanceMetric.Mahalanobis OrElse options.DistanceMetric = PsmDistanceMetric.MahalanobisWithinPropensityCaliper Then
                        mahal = MahalanobisDistance(f, c, invCov)
                        distance = mahal
                    Else
                        distance = psDistance
                    End If
                    eligible.Add(Tuple.Create(c, distance, psDistance, mahal))
                Next

                Dim selected = eligible.OrderBy(Function(t) t.Item2).ThenBy(Function(t) t.Item3).Take(options.MatchingRatio).ToList()
                If selected.Count = 0 Then Continue For
                setId += 1
                For Each s In selected
                    Dim treatedIndex As Integer = If(f.Treated, f.RowIndex, s.Item1.RowIndex)
                    Dim controlIndex As Integer = If(f.Treated, s.Item1.RowIndex, f.RowIndex)
                    matches.Add(New PsmMatchLink With {
                        .SetId = setId,
                        .FocalRowIndex = f.RowIndex,
                        .MatchedRowIndex = s.Item1.RowIndex,
                        .TreatedRowIndex = treatedIndex,
                        .ControlRowIndex = controlIndex,
                        .Distance = s.Item2,
                        .PropensityDistance = s.Item3,
                        .MahalanobisDistance = s.Item4,
                        .ExactGroupLabel = f.ExactGroupLabel,
                        .MatchedWeight = 1.0 / CDbl(selected.Count)
                    })
                    If Not options.WithReplacement Then available.Remove(s.Item1.RowIndex)
                Next
            Next

            Return matches
        End Function

        Public Shared Function ComputeMatchingWeights(input As PsmInputData, matches As List(Of PsmMatchLink), options As PsmOptions) As Double()
            Dim n As Integer = input.RowCount
            Dim weights(n - 1) As Double
            If matches Is Nothing Then Return weights

            If options.Estimand = PsmEstimand.ATT Then
                For Each setGroup In matches.GroupBy(Function(m) m.SetId)
                    Dim first As PsmMatchLink = setGroup.First()
                    weights(first.TreatedRowIndex) = 1.0
                    Dim k As Integer = setGroup.Count()
                    For Each m In setGroup
                        weights(m.ControlRowIndex) += 1.0 / CDbl(k)
                    Next
                Next
            ElseIf options.Estimand = PsmEstimand.ATC Then
                For Each setGroup In matches.GroupBy(Function(m) m.SetId)
                    Dim first As PsmMatchLink = setGroup.First()
                    weights(first.ControlRowIndex) = 1.0
                    Dim k As Integer = setGroup.Count()
                    For Each m In setGroup
                        weights(m.TreatedRowIndex) += 1.0 / CDbl(k)
                    Next
                Next
            End If

            Return weights
        End Function

        Private Shared Function OrderFocalObservations(focal As List(Of PsmObservation), candidates As List(Of PsmObservation), options As PsmOptions) As List(Of PsmObservation)
            Select Case options.MatchingOrder
                Case PsmMatchingOrder.PropensityAscending
                    Return focal.OrderBy(Function(o) o.PropensityScore).ToList()
                Case PsmMatchingOrder.PropensityDescending
                    Return focal.OrderByDescending(Function(o) o.PropensityScore).ToList()
                Case PsmMatchingOrder.Random
                    Dim rng As New Random(options.RandomSeed)
                    Return focal.OrderBy(Function(o) rng.NextDouble()).ToList()
                Case PsmMatchingOrder.HardestFirst
                    Return focal.OrderByDescending(Function(o) NearestAvailableDistance(o, candidates, options)).ToList()
                Case Else
                    Return focal.OrderBy(Function(o) o.RowIndex).ToList()
            End Select
        End Function

        Private Shared Function NearestAvailableDistance(f As PsmObservation, candidates As List(Of PsmObservation), options As PsmOptions) As Double
            Dim best As Double = Double.PositiveInfinity
            For Each c In candidates
                If Not String.Equals(f.ExactGroupLabel, c.ExactGroupLabel, StringComparison.Ordinal) Then Continue For
                Dim d As Double = PropensityDistance(f, c, options)
                If d < best Then best = d
            Next
            Return best
        End Function

        Private Shared Function PropensityDistance(a As PsmObservation, b As PsmObservation, options As PsmOptions) As Double
            If options.DistanceMetric = PsmDistanceMetric.LogitPropensityScore Then Return Math.Abs(a.LogitPropensityScore - b.LogitPropensityScore)
            Return Math.Abs(a.PropensityScore - b.PropensityScore)
        End Function

        Private Shared Function PassesCaliper(a As PsmObservation, b As PsmObservation, options As PsmOptions, denom As Double) As Boolean
            If options.CaliperScale = PsmCaliperScale.None Then Return True
            Dim d As Double
            Select Case options.CaliperScale
                Case PsmCaliperScale.RawPropensityScore
                    d = Math.Abs(a.PropensityScore - b.PropensityScore)
                Case PsmCaliperScale.StandardizedPropensityScore
                    d = Math.Abs(a.PropensityScore - b.PropensityScore) / denom
                Case PsmCaliperScale.LogitPropensityScore
                    d = Math.Abs(a.LogitPropensityScore - b.LogitPropensityScore)
                Case PsmCaliperScale.StandardizedLogitPropensityScore
                    d = Math.Abs(a.LogitPropensityScore - b.LogitPropensityScore) / denom
                Case Else
                    d = 0.0
            End Select
            Return d <= options.Caliper
        End Function

        Private Shared Function CaliperDenominator(observations As List(Of PsmObservation), options As PsmOptions) As Double
            Dim values As IEnumerable(Of Double)
            If options.CaliperScale = PsmCaliperScale.StandardizedLogitPropensityScore Then
                values = observations.Select(Function(o) o.LogitPropensityScore)
            Else
                values = observations.Select(Function(o) o.PropensityScore)
            End If
            Dim sd As Double = PsmMath.StandardDeviation(values)
            If Not AppInfrastructure.IsFinite(sd) OrElse sd <= 0 Then Return 1.0
            Return sd
        End Function

        Private Shared Function BuildInverseCovariance(observations As List(Of PsmObservation), p As Integer) As Double(,)
            Dim rows As New List(Of Double())()
            For Each o In observations
                If o.IncludedByCommonSupport AndAlso o.IncludedByTrimming Then rows.Add(o.Covariates)
            Next
            If rows.Count < 2 Then Throw New ArgumentException("At least two eligible observations are required to compute the Mahalanobis covariance matrix.")

            Dim covariateMatrix(rows.Count - 1, p - 1) As Double
            For i As Integer = 0 To rows.Count - 1
                If rows(i) Is Nothing OrElse rows(i).Length <> p Then Throw New ArgumentException("All covariate rows must have the same length.")
                For j As Integer = 0 To p - 1
                    covariateMatrix(i, j) = rows(i)(j)
                Next
            Next

            Dim cov As Double(,) = Matrix.MatCovar(covariateMatrix)

            For j As Integer = 0 To p - 1
                cov(j, j) += 0.0000001
            Next
            Return Matrix.MatInv(cov, "CHOL")
        End Function

        Private Shared Function MahalanobisDistance(a As PsmObservation, b As PsmObservation, invCov As Double(,)) As Double
            Dim p As Integer = a.Covariates.Length
            Dim diff(p - 1) As Double
            For j As Integer = 0 To p - 1
                diff(j) = a.Covariates(j) - b.Covariates(j)
            Next
            Dim q As Double = Matrix.QuadraticForm(diff, invCov)
            If Not AppInfrastructure.IsFinite(q) Then Return Double.PositiveInfinity
            Return Math.Sqrt(Math.Max(0.0, q))
        End Function

        Private Shared Sub ApplyTrimming(observations As List(Of PsmObservation), options As PsmOptions)
            For Each o In observations
                o.IncludedByTrimming = (o.PropensityScore >= options.TrimPropensityLower AndAlso o.PropensityScore <= options.TrimPropensityUpper)
            Next
        End Sub

        Private Shared Sub ApplyCommonSupport(observations As List(Of PsmObservation), options As PsmOptions)
            If options.CommonSupport = PsmCommonSupportMode.None Then Return
            Dim treated = observations.Where(Function(o) o.Treated AndAlso o.IncludedByTrimming).Select(Function(o) o.PropensityScore).ToList()
            Dim controls = observations.Where(Function(o) Not o.Treated AndAlso o.IncludedByTrimming).Select(Function(o) o.PropensityScore).ToList()
            If treated.Count = 0 OrElse controls.Count = 0 Then Return
            Dim minT As Double = treated.Min()
            Dim maxT As Double = treated.Max()
            Dim minC As Double = controls.Min()
            Dim maxC As Double = controls.Max()
            Dim low As Double = Math.Max(minT, minC)
            Dim high As Double = Math.Min(maxT, maxC)

            For Each o In observations
                Select Case options.CommonSupport
                    Case PsmCommonSupportMode.DropOutsideOverlapRange
                        o.IncludedByCommonSupport = (o.PropensityScore >= low AndAlso o.PropensityScore <= high)
                    Case PsmCommonSupportMode.DropTreatedOutsideControlRange
                        If o.Treated Then o.IncludedByCommonSupport = (o.PropensityScore >= minC AndAlso o.PropensityScore <= maxC)
                    Case PsmCommonSupportMode.DropControlsOutsideTreatedRange
                        If Not o.Treated Then o.IncludedByCommonSupport = (o.PropensityScore >= minT AndAlso o.PropensityScore <= maxT)
                End Select
            Next
        End Sub
    End Class

    Public NotInheritable Class PsmSubclassificationEngine
        Private Sub New()
        End Sub

        Public Shared Function BuildSubclasses(input As PsmInputData, scores As Double(), options As PsmOptions) As List(Of PsmSubclassRow)
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If scores Is Nothing OrElse scores.Length <> input.RowCount Then Throw New ArgumentException("Scores must match input row count.")
            If input.Outcome Is Nothing Then Return New List(Of PsmSubclassRow)()

            Dim cuts As New List(Of Double)()
            For s As Integer = 0 To options.SubclassificationStrata
                cuts.Add(PsmMath.Quantile(scores, CDbl(s) / CDbl(options.SubclassificationStrata)))
            Next
            cuts(0) = Math.Min(cuts(0), scores.Min()) - 0.000000000001
            cuts(cuts.Count - 1) = Math.Max(cuts(cuts.Count - 1), scores.Max()) + 0.000000000001

            Dim rows As New List(Of PsmSubclassRow)()
            For s As Integer = 1 To options.SubclassificationStrata
                Dim lo As Double = cuts(s - 1)
                Dim hi As Double = cuts(s)
                Dim treatedOutcomes As New List(Of Double)()
                Dim controlOutcomes As New List(Of Double)()
                For i As Integer = 0 To input.RowCount - 1
                    If scores(i) > lo AndAlso scores(i) <= hi Then
                        If input.Treatment(i) >= 0.5 Then
                            treatedOutcomes.Add(input.Outcome(i))
                        Else
                            controlOutcomes.Add(input.Outcome(i))
                        End If
                    End If
                Next
                Dim mt As Double = If(treatedOutcomes.Count > 0, treatedOutcomes.Average(), Double.NaN)
                Dim mc As Double = If(controlOutcomes.Count > 0, controlOutcomes.Average(), Double.NaN)
                rows.Add(New PsmSubclassRow With {
                    .Stratum = s,
                    .LowerScore = lo,
                    .UpperScore = hi,
                    .TreatedN = treatedOutcomes.Count,
                    .ControlN = controlOutcomes.Count,
                    .TreatedOutcomeMean = mt,
                    .ControlOutcomeMean = mc,
                    .Effect = mt - mc,
                    .Weight = CDbl(treatedOutcomes.Count + controlOutcomes.Count) / CDbl(input.RowCount)
                })
            Next
            Return rows
        End Function

        Public Shared Function EstimateEffect(rows As List(Of PsmSubclassRow), options As PsmOptions) As PsmEffectResult
            Dim usable = rows.Where(Function(r) r.TreatedN > 0 AndAlso r.ControlN > 0 AndAlso AppInfrastructure.IsFinite(r.Effect)).ToList()
            If usable.Count = 0 Then Return Nothing
            Dim sw As Double = usable.Sum(Function(r) r.Weight)
            Dim est As Double = usable.Sum(Function(r) r.Weight * r.Effect) / sw
            Return New PsmEffectResult With {
                .Estimand = options.Estimand,
                .Method = "Subclassification",
                .OutcomeType = PsmOutcomeType.Continuous,
                .Estimate = est,
                .StandardError = Double.NaN,
                .LowerConfidenceLimit = Double.NaN,
                .UpperConfidenceLimit = Double.NaN,
                .MatchedSets = usable.Count
            }
        End Function
    End Class

End Namespace
