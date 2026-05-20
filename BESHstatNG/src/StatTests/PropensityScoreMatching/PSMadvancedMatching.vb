Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace CausalInference

    ''' <summary>
    ''' Optional advanced matching modes that can be called by GUI/UDF layers without changing the main PsmBackend facade yet.
    ''' These methods intentionally return the same PsmMatchLink / weight structures used by the existing backend.
    ''' </summary>
    Public Enum PsmAdvancedMatchingMethod
        OptimalPair = 0
        CoarsenedExact = 1
    End Enum

    ''' <summary>
    ''' Coarsening rules for coarsened exact matching. If CutPoints(j) is supplied, those cut points are used for covariate j.
    ''' Otherwise BinCounts(j) quantile-based bins are used. BinCounts defaults to 4 for unspecified covariates.
    ''' </summary>
    Public Class PsmCoarseningSpec
        Public Property BinCounts As Integer()
        Public Property CutPoints As Double()()
        Public Property IncludePropensityScore As Boolean = False
        Public Property PropensityScoreBins As Integer = 5
        Public Property DefaultCovariateBins As Integer = 4
        Public Property Estimand As PsmEstimand = PsmEstimand.ATT
        Public Property NormalizeWeightsToSampleSize As Boolean = True
        Public Property DropUnmatchedStrata As Boolean = True
    End Class

    Public Class PsmCemStratumRow
        Public Property StratumId As Integer
        Public Property Key As String
        Public Property TreatedN As Integer
        Public Property ControlN As Integer
        Public Property TotalN As Integer
        Public Property Retained As Boolean
        Public Property WeightTreated As Double
        Public Property WeightControl As Double
    End Class

    Public Class PsmCoarsenedExactResult
        Public Property Weights As Double()
        Public Property RowStratumIds As Integer()
        Public Property Strata As New List(Of PsmCemStratumRow)()
        Public Property RetainedRows As Integer
        Public Property DroppedRows As Integer
        Public Property Warnings As New List(Of String)()
    End Class

    ''' <summary>
    ''' Additional backend matching routines commonly available in mature PSM tools.
    ''' Batch 07 focuses on optimal pair matching and coarsened exact matching. These are additive helpers;
    ''' the main Fit facade can be wired to them later when GUI/UDF options are introduced.
    ''' </summary>
    Public NotInheritable Class PsmAdvancedMatching
        Private Const LargeCost As Double = 1.0E+100

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Optimal 1:1 pair matching without replacement. This minimizes the total selected-pair distance within each exact group.
        ''' ATT matches treated focal units to controls. ATC matches control focal units to treated units.
        ''' ATE/ATO should use weighting, subclassification, or CEM weights instead.
        ''' </summary>
        Public Shared Function MatchOptimalPairs(input As PsmInputData, observations As List(Of PsmObservation), options As PsmOptions) As List(Of PsmMatchLink)
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If observations Is Nothing Then Throw New ArgumentNullException("observations")
            If options Is Nothing Then Throw New ArgumentNullException("options")
            If options.Estimand <> PsmEstimand.ATT AndAlso options.Estimand <> PsmEstimand.ATC Then Throw New ArgumentException("Optimal pair matching supports ATT and ATC.")

            Dim focalIsTreated As Boolean = (options.Estimand = PsmEstimand.ATT)
            Dim eligible As List(Of PsmObservation) = observations.Where(Function(o) o.IncludedByCommonSupport AndAlso o.IncludedByTrimming).ToList()
            If eligible.Count = 0 Then Return New List(Of PsmMatchLink)()

            Dim invCov As Double(,) = Nothing
            If options.DistanceMetric = PsmDistanceMetric.Mahalanobis OrElse options.DistanceMetric = PsmDistanceMetric.MahalanobisWithinPropensityCaliper Then
                invCov = BuildInverseCovariance(eligible, input.CovariateCount)
            End If

            Dim caliperDenom As Double = CaliperDenominator(eligible, options)
            Dim matches As New List(Of PsmMatchLink)()
            Dim nextSetId As Integer = 0

            For Each exactGroup In eligible.GroupBy(Function(o) If(o.ExactGroupLabel, ""))
                Dim focal As List(Of PsmObservation) = exactGroup.Where(Function(o) o.Treated = focalIsTreated).OrderBy(Function(o) o.RowIndex).ToList()
                Dim candidates As List(Of PsmObservation) = exactGroup.Where(Function(o) o.Treated <> focalIsTreated).OrderBy(Function(o) o.RowIndex).ToList()
                If focal.Count = 0 OrElse candidates.Count = 0 Then Continue For

                Dim groupMatches As List(Of PsmMatchLink) = MatchOptimalPairsWithinGroup(focal, candidates, options, invCov, caliperDenom, nextSetId)
                matches.AddRange(groupMatches)
                If groupMatches.Count > 0 Then nextSetId = matches.Max(Function(m) m.SetId)
            Next

            Return matches
        End Function

        ''' <summary>
        ''' Build coarsened exact matching weights. The returned weights can be used with PsmOutcomeAnalysis.EstimateWeightedEffect
        ''' and PsmBalanceDiagnostics.ComputeBalance. Unmatched strata receive weight 0 when DropUnmatchedStrata is true.
        ''' </summary>
        Public Shared Function BuildCoarsenedExactWeights(input As PsmInputData, Optional scores As Double() = Nothing, Optional spec As PsmCoarseningSpec = Nothing) As PsmCoarsenedExactResult
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If input.Covariates Is Nothing Then Throw New ArgumentException("Covariate matrix is required.")
            If spec Is Nothing Then spec = New PsmCoarseningSpec()
            If spec.IncludePropensityScore AndAlso (scores Is Nothing OrElse scores.Length <> input.RowCount) Then Throw New ArgumentException("Propensity scores are required when IncludePropensityScore is true.")

            Dim n As Integer = input.RowCount
            Dim p As Integer = input.CovariateCount
            Dim rowKeys(n - 1) As String
            Dim strataByKey As New Dictionary(Of String, List(Of Integer))(StringComparer.Ordinal)
            Dim cutPoints As Double()() = BuildCutPoints(input, scores, spec)

            For i As Integer = 0 To n - 1
                Dim key As String = BuildCemKey(input, scores, cutPoints, spec, i)
                rowKeys(i) = key
                If Not strataByKey.ContainsKey(key) Then strataByKey.Add(key, New List(Of Integer)())
                strataByKey(key).Add(i)
            Next

            Dim result As New PsmCoarsenedExactResult()
            Dim weights(n - 1) As Double
            Dim rowStrata(n - 1) As Integer
            For i As Integer = 0 To n - 1
                rowStrata(i) = -1
            Next

            Dim stratumId As Integer = 0
            For Each kvp In strataByKey.OrderBy(Function(k) k.Key)
                stratumId += 1
                Dim rows As List(Of Integer) = kvp.Value
                Dim treatedRows As List(Of Integer) = rows.Where(Function(r) input.Treatment(r) >= 0.5).ToList()
                Dim controlRows As List(Of Integer) = rows.Where(Function(r) input.Treatment(r) < 0.5).ToList()
                Dim retained As Boolean = (treatedRows.Count > 0 AndAlso controlRows.Count > 0)
                Dim wtT As Double = 0.0
                Dim wtC As Double = 0.0

                If retained OrElse Not spec.DropUnmatchedStrata Then
                    Dim cellWeights As Tuple(Of Double, Double) = ComputeCemCellWeights(treatedRows.Count, controlRows.Count, spec.Estimand)
                    wtT = cellWeights.Item1
                    wtC = cellWeights.Item2
                End If

                For Each r In treatedRows
                    weights(r) = If(retained OrElse Not spec.DropUnmatchedStrata, wtT, 0.0)
                    rowStrata(r) = stratumId
                Next
                For Each r In controlRows
                    weights(r) = If(retained OrElse Not spec.DropUnmatchedStrata, wtC, 0.0)
                    rowStrata(r) = stratumId
                Next

                result.Strata.Add(New PsmCemStratumRow With {
                    .StratumId = stratumId,
                    .Key = kvp.Key,
                    .TreatedN = treatedRows.Count,
                    .ControlN = controlRows.Count,
                    .TotalN = rows.Count,
                    .Retained = retained,
                    .WeightTreated = wtT,
                    .WeightControl = wtC
                })
            Next

            If spec.NormalizeWeightsToSampleSize Then NormalizeWeightsByTreatment(input, weights)

            result.Weights = weights
            result.RowStratumIds = rowStrata
            result.RetainedRows = System.Linq.Enumerable.Count(weights, Function(w) AppInfrastructure.IsFinite(w) AndAlso w > 0.0)
            result.DroppedRows = n - result.RetainedRows
            If result.RetainedRows = 0 Then result.Warnings.Add("Coarsened exact matching retained no rows. Use fewer bins or wider user-specified cut points.")
            If result.Strata.Any(Function(s) Not s.Retained) Then result.Warnings.Add("One or more coarsened strata contained only treated or only control observations and were assigned zero weight.")
            Return result
        End Function

        Public Shared Function EstimateCoarsenedExactEffect(input As PsmInputData, cemResult As PsmCoarsenedExactResult, estimand As PsmEstimand) As PsmEffectResult
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If cemResult Is Nothing OrElse cemResult.Weights Is Nothing Then Throw New ArgumentNullException("cemResult")
            Dim options As New PsmOptions With {.Estimand = estimand}
            Dim effect As PsmEffectResult = PsmOutcomeAnalysis.EstimateWeightedEffect(input, cemResult.Weights, options)
            If effect IsNot Nothing Then effect.Method = "Coarsened Exact Matching"
            Return effect
        End Function

        Private Shared Function MatchOptimalPairsWithinGroup(focal As List(Of PsmObservation), candidates As List(Of PsmObservation), options As PsmOptions, invCov As Double(,), caliperDenom As Double, setIdOffset As Integer) As List(Of PsmMatchLink)
            Dim matches As New List(Of PsmMatchLink)()
            Dim fCount As Integer = focal.Count
            Dim cCount As Integer = candidates.Count
            Dim pairCost(fCount - 1, cCount - 1) As Double
            For i As Integer = 0 To fCount - 1
                For j As Integer = 0 To cCount - 1
                    Dim distanceInfo As Tuple(Of Double, Double, Double, Boolean) = ComputeDistance(focal(i), candidates(j), options, invCov, caliperDenom)
                    pairCost(i, j) = If(distanceInfo.Item4, distanceInfo.Item1, LargeCost)
                Next
            Next

            If fCount <= cCount Then
                Dim assignment As Integer() = SolveRectangularAssignment(pairCost)
                For i As Integer = 0 To assignment.Length - 1
                    Dim j As Integer = assignment(i)
                    If j < 0 OrElse j >= cCount Then Continue For
                    Dim distanceInfo As Tuple(Of Double, Double, Double, Boolean) = ComputeDistance(focal(i), candidates(j), options, invCov, caliperDenom)
                    If Not distanceInfo.Item4 Then Continue For
                    matches.Add(BuildMatchLink(focal(i), candidates(j), options, setIdOffset + matches.Count + 1, distanceInfo))
                Next
            Else
                Dim transposed(cCount - 1, fCount - 1) As Double
                For i As Integer = 0 To fCount - 1
                    For j As Integer = 0 To cCount - 1
                        transposed(j, i) = pairCost(i, j)
                    Next
                Next
                Dim assignment As Integer() = SolveRectangularAssignment(transposed)
                For j As Integer = 0 To assignment.Length - 1
                    Dim i As Integer = assignment(j)
                    If i < 0 OrElse i >= fCount Then Continue For
                    Dim distanceInfo As Tuple(Of Double, Double, Double, Boolean) = ComputeDistance(focal(i), candidates(j), options, invCov, caliperDenom)
                    If Not distanceInfo.Item4 Then Continue For
                    matches.Add(BuildMatchLink(focal(i), candidates(j), options, setIdOffset + matches.Count + 1, distanceInfo))
                Next
            End If

            Return matches
        End Function

        Private Shared Function BuildMatchLink(focal As PsmObservation, candidate As PsmObservation, options As PsmOptions, setId As Integer, distanceInfo As Tuple(Of Double, Double, Double, Boolean)) As PsmMatchLink
            Dim treatedIndex As Integer = If(focal.Treated, focal.RowIndex, candidate.RowIndex)
            Dim controlIndex As Integer = If(focal.Treated, candidate.RowIndex, focal.RowIndex)
            Return New PsmMatchLink With {
                .SetId = setId,
                .FocalRowIndex = focal.RowIndex,
                .MatchedRowIndex = candidate.RowIndex,
                .TreatedRowIndex = treatedIndex,
                .ControlRowIndex = controlIndex,
                .Distance = distanceInfo.Item1,
                .PropensityDistance = distanceInfo.Item2,
                .MahalanobisDistance = distanceInfo.Item3,
                .ExactGroupLabel = If(focal.ExactGroupLabel, ""),
                .MatchedWeight = 1.0
            }
        End Function

        ''' <summary>
        ''' Returns (overall distance, propensity-score distance, Mahalanobis distance, eligible).
        ''' </summary>
        Private Shared Function ComputeDistance(a As PsmObservation, b As PsmObservation, options As PsmOptions, invCov As Double(,), caliperDenom As Double) As Tuple(Of Double, Double, Double, Boolean)
            Dim psDistance As Double = If(options.DistanceMetric = PsmDistanceMetric.LogitPropensityScore, Math.Abs(a.LogitPropensityScore - b.LogitPropensityScore), Math.Abs(a.PropensityScore - b.PropensityScore))
            Dim eligible As Boolean = PassesCaliper(a, b, options, caliperDenom)
            Dim mahal As Double = Double.NaN
            Dim distance As Double = psDistance
            If options.DistanceMetric = PsmDistanceMetric.Mahalanobis OrElse options.DistanceMetric = PsmDistanceMetric.MahalanobisWithinPropensityCaliper Then
                If invCov Is Nothing Then
                    eligible = False
                    distance = LargeCost
                Else
                    mahal = MahalanobisDistance(a, b, invCov)
                    distance = mahal
                End If
            End If
            If Not AppInfrastructure.IsFinite(distance) Then eligible = False
            Return Tuple.Create(distance, psDistance, mahal, eligible)
        End Function

        Private Shared Function PassesCaliper(a As PsmObservation, b As PsmObservation, options As PsmOptions, denom As Double) As Boolean
            If options.CaliperScale = PsmCaliperScale.None Then Return True
            Dim d As Double
            Select Case options.CaliperScale
                Case PsmCaliperScale.RawPropensityScore
                    d = Math.Abs(a.PropensityScore - b.PropensityScore)
                Case PsmCaliperScale.StandardizedPropensityScore
                    d = Math.Abs(a.PropensityScore - b.PropensityScore) / Math.Max(denom, 0.000000000001)
                Case PsmCaliperScale.LogitPropensityScore
                    d = Math.Abs(a.LogitPropensityScore - b.LogitPropensityScore)
                Case PsmCaliperScale.StandardizedLogitPropensityScore
                    d = Math.Abs(a.LogitPropensityScore - b.LogitPropensityScore) / Math.Max(denom, 0.000000000001)
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
            If Not AppInfrastructure.IsFinite(sd) OrElse sd <= 0.0 Then Return 1.0
            Return sd
        End Function

        Private Shared Function BuildInverseCovariance(observations As List(Of PsmObservation), p As Integer) As Double(,)
            Dim rows As List(Of PsmObservation) = observations.Where(Function(o) o.IncludedByCommonSupport AndAlso o.IncludedByTrimming).ToList()
            If rows.Count < 2 Then Return Nothing
            Dim x(rows.Count - 1, p - 1) As Double
            For i As Integer = 0 To rows.Count - 1
                For j As Integer = 0 To p - 1
                    x(i, j) = rows(i).Covariates(j)
                Next
            Next
            Dim cov As Double(,) = Matrix.MatCovar(x)
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
            Dim q As Double = 0.0
            For r As Integer = 0 To p - 1
                Dim inner As Double = 0.0
                For c As Integer = 0 To p - 1
                    inner += invCov(r, c) * diff(c)
                Next
                q += diff(r) * inner
            Next
            If Not AppInfrastructure.IsFinite(q) Then Return Double.PositiveInfinity
            Return Math.Sqrt(Math.Max(0.0, q))
        End Function

        ''' <summary>
        ''' Hungarian assignment solver for a rows .le. columns rectangular cost matrix.
        ''' Returns the selected column index for each row, or -1 if no column was selected.
        ''' </summary>
        Private Shared Function SolveRectangularAssignment(cost As Double(,)) As Integer()
            Dim n As Integer = cost.GetLength(0)
            Dim m As Integer = cost.GetLength(1)
            If n = 0 Then Return New Integer() {}
            If m < n Then Throw New ArgumentException("Assignment solver requires rows <= columns.")

            Dim u(n) As Double
            Dim v(m) As Double
            Dim p(m) As Integer
            Dim way(m) As Integer

            For i As Integer = 1 To n
                p(0) = i
                Dim j0 As Integer = 0
                Dim minv(m) As Double
                Dim used(m) As Boolean
                For j As Integer = 0 To m
                    minv(j) = Double.PositiveInfinity
                    used(j) = False
                Next

                Do
                    used(j0) = True
                    Dim i0 As Integer = p(j0)
                    Dim delta As Double = Double.PositiveInfinity
                    Dim j1 As Integer = 0
                    For j As Integer = 1 To m
                        If Not used(j) Then
                            Dim cur As Double = cost(i0 - 1, j - 1) - u(i0) - v(j)
                            If cur < minv(j) Then
                                minv(j) = cur
                                way(j) = j0
                            End If
                            If minv(j) < delta Then
                                delta = minv(j)
                                j1 = j
                            End If
                        End If
                    Next
                    If Double.IsPositiveInfinity(delta) Then Exit Do
                    For j As Integer = 0 To m
                        If used(j) Then
                            u(p(j)) += delta
                            v(j) -= delta
                        Else
                            minv(j) -= delta
                        End If
                    Next
                    j0 = j1
                Loop While p(j0) <> 0

                Do
                    Dim j1 As Integer = way(j0)
                    p(j0) = p(j1)
                    j0 = j1
                Loop While j0 <> 0
            Next

            Dim assignment(n - 1) As Integer
            For i As Integer = 0 To n - 1
                assignment(i) = -1
            Next
            For j As Integer = 1 To m
                If p(j) > 0 AndAlso p(j) <= n Then assignment(p(j) - 1) = j - 1
            Next
            Return assignment
        End Function

        Private Shared Function BuildCutPoints(input As PsmInputData, scores As Double(), spec As PsmCoarseningSpec) As Double()()
            Dim p As Integer = input.CovariateCount
            Dim totalColumns As Integer = p + If(spec.IncludePropensityScore, 1, 0)
            Dim cutPoints(totalColumns - 1)() As Double

            For j As Integer = 0 To totalColumns - 1
                Dim supplied As Double() = Nothing
                If spec.CutPoints IsNot Nothing AndAlso j < spec.CutPoints.Length Then supplied = spec.CutPoints(j)
                If supplied IsNot Nothing AndAlso supplied.Length > 0 Then
                    cutPoints(j) = supplied.Where(Function(v) AppInfrastructure.IsFinite(v)).OrderBy(Function(v) v).Distinct().ToArray()
                Else
                    Dim bins As Integer = If(j < p, GetBinCount(spec, j), Math.Max(2, spec.PropensityScoreBins))
                    cutPoints(j) = BuildQuantileCutPoints(GetColumnValues(input, scores, j), bins)
                End If
            Next

            Return cutPoints
        End Function

        Private Shared Function GetBinCount(spec As PsmCoarseningSpec, columnIndex As Integer) As Integer
            If spec.BinCounts IsNot Nothing AndAlso columnIndex < spec.BinCounts.Length AndAlso spec.BinCounts(columnIndex) > 0 Then Return spec.BinCounts(columnIndex)
            Return Math.Max(2, spec.DefaultCovariateBins)
        End Function

        Private Shared Function BuildQuantileCutPoints(values As IEnumerable(Of Double), bins As Integer) As Double()
            Dim k As Integer = Math.Max(1, bins)
            If k <= 1 Then Return New Double() {}
            Dim cuts As New List(Of Double)()
            For b As Integer = 1 To k - 1
                Dim q As Double = PsmMath.Quantile(values, CDbl(b) / CDbl(k))
                If AppInfrastructure.IsFinite(q) AndAlso Not cuts.Contains(q) Then cuts.Add(q)
            Next
            cuts.Sort()
            Return cuts.ToArray()
        End Function

        Private Shared Function GetColumnValues(input As PsmInputData, scores As Double(), columnIndex As Integer) As IEnumerable(Of Double)
            If columnIndex < input.CovariateCount Then
                Dim vals As New List(Of Double)()
                For i As Integer = 0 To input.RowCount - 1
                    vals.Add(input.Covariates(i, columnIndex))
                Next
                Return vals
            End If
            Return scores
        End Function

        Private Shared Function BuildCemKey(input As PsmInputData, scores As Double(), cutPoints As Double()(), spec As PsmCoarseningSpec, rowIndex As Integer) As String
            Dim parts As New List(Of String)()
            If input.ExactGroupLabels IsNot Nothing Then parts.Add("E=" & If(input.ExactGroupLabels(rowIndex), ""))
            For j As Integer = 0 To input.CovariateCount - 1
                parts.Add("X" & j.ToString() & "=" & BinIndex(input.Covariates(rowIndex, j), cutPoints(j)).ToString())
            Next
            If spec.IncludePropensityScore Then
                parts.Add("PS=" & BinIndex(scores(rowIndex), cutPoints(input.CovariateCount)).ToString())
            End If
            Return String.Join("|", parts.ToArray())
        End Function

        Private Shared Function BinIndex(value As Double, cuts As Double()) As Integer
            If cuts Is Nothing OrElse cuts.Length = 0 Then Return 0
            For i As Integer = 0 To cuts.Length - 1
                If value <= cuts(i) Then Return i
            Next
            Return cuts.Length
        End Function

        Private Shared Function ComputeCemCellWeights(nT As Integer, nC As Integer, estimand As PsmEstimand) As Tuple(Of Double, Double)
            If nT <= 0 OrElse nC <= 0 Then Return Tuple.Create(0.0, 0.0)
            Select Case estimand
                Case PsmEstimand.ATT
                    Return Tuple.Create(1.0, CDbl(nT) / CDbl(nC))
                Case PsmEstimand.ATC
                    Return Tuple.Create(CDbl(nC) / CDbl(nT), 1.0)
                Case PsmEstimand.ATE
                    Dim total As Double = CDbl(nT + nC)
                    Return Tuple.Create(total / CDbl(2 * nT), total / CDbl(2 * nC))
                Case PsmEstimand.ATO
                    Dim minCell As Double = CDbl(Math.Min(nT, nC))
                    Return Tuple.Create(minCell / CDbl(nT), minCell / CDbl(nC))
                Case Else
                    Return Tuple.Create(1.0, 1.0)
            End Select
        End Function

        Private Shared Sub NormalizeWeightsByTreatment(input As PsmInputData, weights As Double())
            Dim sumT As Double = 0.0
            Dim sumC As Double = 0.0
            Dim nT As Integer = 0
            Dim nC As Integer = 0
            For i As Integer = 0 To input.RowCount - 1
                If input.Treatment(i) >= 0.5 Then
                    nT += 1
                    sumT += weights(i)
                Else
                    nC += 1
                    sumC += weights(i)
                End If
            Next
            Dim scaleT As Double = If(sumT > 0.0, CDbl(nT) / sumT, 1.0)
            Dim scaleC As Double = If(sumC > 0.0, CDbl(nC) / sumC, 1.0)
            For i As Integer = 0 To input.RowCount - 1
                If input.Treatment(i) >= 0.5 Then
                    weights(i) *= scaleT
                Else
                    weights(i) *= scaleC
                End If
            Next
        End Sub
    End Class

    Public NotInheritable Class PsmAdvancedMatchingTables
        Private Sub New()
        End Sub

        Public Shared Function CoarsenedExactStrataTable(result As PsmCoarsenedExactResult) As Object(,)
            If result Is Nothing OrElse result.Strata Is Nothing OrElse result.Strata.Count = 0 Then Return PsmResult.EmptyTable("No coarsened exact strata available")
            Dim rows As List(Of PsmCemStratumRow) = result.Strata
            Dim table(rows.Count, 7) As Object
            table(0, 0) = "Stratum" : table(0, 1) = "Key" : table(0, 2) = "Treated N" : table(0, 3) = "Control N" : table(0, 4) = "Total N" : table(0, 5) = "Retained" : table(0, 6) = "Treated Weight" : table(0, 7) = "Control Weight"
            For i As Integer = 0 To rows.Count - 1
                Dim r As PsmCemStratumRow = rows(i)
                table(i + 1, 0) = r.StratumId
                table(i + 1, 1) = r.Key
                table(i + 1, 2) = r.TreatedN
                table(i + 1, 3) = r.ControlN
                table(i + 1, 4) = r.TotalN
                table(i + 1, 5) = r.Retained
                table(i + 1, 6) = r.WeightTreated
                table(i + 1, 7) = r.WeightControl
            Next
            Return table
        End Function

        Public Shared Function CoarsenedExactWeightsTable(input As PsmInputData, result As PsmCoarsenedExactResult) As Object(,)
            If input Is Nothing OrElse result Is Nothing OrElse result.Weights Is Nothing Then Return PsmResult.EmptyTable("No coarsened exact weights available")
            Dim table(input.RowCount, 5) As Object
            table(0, 0) = "Row" : table(0, 1) = "ID" : table(0, 2) = "Treatment" : table(0, 3) = "Stratum" : table(0, 4) = "Weight" : table(0, 5) = "Retained"
            For i As Integer = 0 To input.RowCount - 1
                table(i + 1, 0) = i + 1
                table(i + 1, 1) = input.GetId(i)
                table(i + 1, 2) = input.Treatment(i)
                table(i + 1, 3) = If(result.RowStratumIds Is Nothing OrElse i >= result.RowStratumIds.Length, -1, result.RowStratumIds(i))
                table(i + 1, 4) = result.Weights(i)
                table(i + 1, 5) = result.Weights(i) > 0.0
            Next
            Return table
        End Function

    End Class

End Namespace
