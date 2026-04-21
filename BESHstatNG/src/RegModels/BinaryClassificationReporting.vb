Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports ExcelDna.Integration
Imports Microsoft.Office.Interop

Namespace regression

    ''' <summary>
    ''' Summary metrics for binary classification at a single threshold.
    ''' </summary>
    Public Structure BinaryClassificationSummary
        Public Threshold As Double
        Public TP As Double
        Public FP As Double
        Public TN As Double
        Public FN As Double
        Public Sensitivity As Double
        Public Specificity As Double
        Public Precision As Double
        Public Recall As Double
        Public NPV As Double
        Public Accuracy As Double
        Public BalancedAccuracy As Double
        Public YoudenJ As Double
        Public F1 As Double
        Public Prevalence As Double
        Public N As Double
    End Structure

    ''' <summary>
    ''' One row of a threshold-performance table.
    ''' </summary>
    Public Structure BinaryThresholdRow
        Public Threshold As Double
        Public TP As Double
        Public FP As Double
        Public TN As Double
        Public FN As Double
        Public Sensitivity As Double
        Public Specificity As Double
        Public Precision As Double
        Public Recall As Double
        Public NPV As Double
        Public Accuracy As Double
        Public BalancedAccuracy As Double
        Public YoudenJ As Double
        Public F1 As Double
        Public Prevalence As Double
        Public N As Double
    End Structure

    ''' <summary>
    ''' Summary information for a calibration bin.
    ''' </summary>
    Public Structure CalibrationBinSummary
        Public BinIndex As Integer
        Public N As Double
        Public MeanPredicted As Double
        Public ObservedRate As Double
        Public LowerCI As Double
        Public UpperCI As Double
        Public ThresholdLower As Double
        Public ThresholdUpper As Double
    End Structure

    ''' <summary>
    ''' Shared percentile-cut bin definition used by both Hosmer–Lemeshow
    ''' and quantile-based calibration summaries.
    ''' </summary>
    Public Structure ProbabilityCutpointBin
        Public BinIndex As Integer
        Public CutUpper As Double
        Public Indices As Integer()
    End Structure

    ''' <summary>
    ''' Shared engine for threshold-based reporting of binary probabilistic models.
    ''' </summary>
    Public Module BinaryClassificationReporting

        Private Const EPS As Double = 0.000000000001

        ''' <summary>
        ''' Validates binary outcomes, predicted probabilities, and optional weights.
        ''' </summary>
        Public Sub ValidateBinaryInputs(y() As Double,
                                        p() As Double,
                                        Optional weights() As Double = Nothing,
                                        Optional allowMissing As Boolean = False)

            If y Is Nothing OrElse p Is Nothing Then
                Throw New ArgumentNullException("y/p", "Observed outcomes and predicted probabilities must not be Nothing.")
            End If
            If y.Length = 0 OrElse p.Length = 0 Then
                Throw New ArgumentException("Observed outcomes and predicted probabilities must be non-empty.")
            End If
            If y.Length <> p.Length Then
                Throw New ArgumentException("Observed outcomes and predicted probabilities must have the same length.")
            End If
            If weights IsNot Nothing AndAlso weights.Length <> y.Length Then
                Throw New ArgumentException("Weights must have the same length as observed outcomes.")
            End If

            For i = 0 To y.Length - 1
                If Not allowMissing Then
                    If Double.IsNaN(y(i)) OrElse Double.IsInfinity(y(i)) Then
                        Throw New ArgumentException("Observed outcomes contain invalid numeric values.")
                    End If
                    If Double.IsNaN(p(i)) OrElse Double.IsInfinity(p(i)) Then
                        Throw New ArgumentException("Predicted probabilities contain invalid numeric values.")
                    End If
                End If

                If Not IsApproximatelyBinary(y(i)) Then
                    Throw New ArgumentException("Observed outcomes must be coded as 0/1.")
                End If

                If p(i) < -EPS OrElse p(i) > 1.0 + EPS Then
                    Throw New ArgumentException("Predicted probabilities must lie in [0,1].")
                End If

                If weights IsNot Nothing Then
                    If Double.IsNaN(weights(i)) OrElse Double.IsInfinity(weights(i)) OrElse weights(i) < 0 Then
                        Throw New ArgumentException("Weights must be finite and nonnegative.")
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' Computes a binary classification summary at a single threshold.
        ''' Positive is predicted when p &gt;= threshold.
        ''' </summary>
        Public Function ComputeBinarySummary(y() As Double,
                                             p() As Double,
                                             Optional threshold As Double = 0.5,
                                             Optional weights() As Double = Nothing) As BinaryClassificationSummary

            ValidateBinaryInputs(y, p, weights)
            AppInfrastructure.ValidateClosedUnitInterval(threshold, "threshold", "Threshold must lie in [0,1].")

            Dim w() As Double = NormalizeWeights(y.Length, weights)
            Dim out As New BinaryClassificationSummary With {.Threshold = threshold, .N = 0.0}

            For i = 0 To y.Length - 1
                Dim yi As Integer = If(y(i) >= 0.5, 1, 0)
                Dim predPos As Boolean = (p(i) >= threshold)
                Dim wi As Double = w(i)

                out.N += wi
                If yi = 1 Then
                    If predPos Then
                        out.TP += wi
                    Else
                        out.FN += wi
                    End If
                Else
                    If predPos Then
                        out.FP += wi
                    Else
                        out.TN += wi
                    End If
                End If
            Next

            PopulateDerivedMetrics(out)
            Return out
        End Function

        ''' <summary>
        ''' Builds a threshold table for binary classification.
        ''' If thresholds are not supplied, unique fitted probabilities are used.
        ''' </summary>
        Public Function BuildThresholdTable(y() As Double,
                                            p() As Double,
                                            Optional thresholds() As Double = Nothing,
                                            Optional weights() As Double = Nothing,
                                            Optional maxRows As Integer = 500) As List(Of BinaryThresholdRow)

            ValidateBinaryInputs(y, p, weights)

            Dim thr() As Double = If(thresholds IsNot Nothing AndAlso thresholds.Length > 0,
                                     NormalizeThresholds(thresholds),
                                     GetDefaultThresholds(p, maxRows))

            Dim rows As New List(Of BinaryThresholdRow)(thr.Length)
            For Each t In thr
                Dim s = ComputeBinarySummary(y, p, t, weights)
                rows.Add(ToThresholdRow(s))
            Next

            Return rows
        End Function

        ''' <summary>
        ''' Computes the Brier score for binary outcomes and predicted probabilities.
        ''' </summary>
        Public Function ComputeBrierScore(y() As Double, p() As Double, Optional weights() As Double = Nothing) As Double
            ValidateBinaryInputs(y, p, weights)
            Dim w() As Double = NormalizeWeights(y.Length, weights)
            Dim sumW As Double = 0.0
            Dim sumSq As Double = 0.0

            For i = 0 To y.Length - 1
                Dim wi As Double = w(i)
                Dim d As Double = y(i) - p(i)
                sumW += wi
                sumSq += wi * d * d
            Next

            If sumW <= 0 Then Return Double.NaN
            Return sumSq / sumW
        End Function

        ''' <summary>
        ''' Builds calibration bins from observed outcomes and predicted probabilities.
        ''' Supported methods: quantile (default), equalwidth.
        ''' </summary>
        Public Function BuildCalibrationBins(y() As Double,
                                             p() As Double,
                                             Optional bins As Integer = 10,
                                             Optional weights() As Double = Nothing,
                                             Optional method As String = "quantile") As List(Of CalibrationBinSummary)

            ValidateBinaryInputs(y, p, weights)
            If bins < 2 Then Throw New ArgumentOutOfRangeException("bins", "Number of calibration bins must be at least 2.")

            Dim w() As Double = NormalizeWeights(y.Length, weights)
            Dim cleanMethod As String = If(method, "quantile").Trim().ToLowerInvariant()

            Select Case cleanMethod
                Case "quantile"
                    Return BuildQuantileCalibrationBins(y, p, w, bins)
                Case "equalwidth", "equal-width", "equal_width"
                    Return BuildEqualWidthCalibrationBins(y, p, w, bins)
                Case Else
                    Throw New ArgumentException("Unsupported calibration binning method. Use 'quantile' or 'equalwidth'.")
            End Select
        End Function

        ''' <summary>
        ''' Parses and normalizes a calibration-binning method supplied from Excel.
        ''' Supported return values are <c>quantile</c> and <c>equalwidth</c>.
        ''' </summary>
        Public Function ParseCalibrationMethod(arg As Object,
                                               Optional defaultValue As String = "quantile") As String
            If BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return defaultValue

            Dim s As String = BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)
            If String.IsNullOrWhiteSpace(s) Then Return defaultValue

            Select Case s.Trim().ToLowerInvariant()
                Case "quantile", "quantiles", "decile", "deciles"
                    Return "quantile"
                Case "equalwidth", "equal-width", "equal_width", "equal width"
                    Return "equalwidth"
                Case Else
                    Return defaultValue
            End Select
        End Function

        ''' <summary>
        ''' Returns a default set of sorted thresholds based on the unique fitted probabilities.
        ''' </summary>
        Public Function GetDefaultThresholds(p() As Double, Optional maxRows As Integer = 500,
                                             Optional includeEndpoints As Boolean = True) As Double()
            If p Is Nothing OrElse p.Length = 0 Then Return New Double() {}

            Dim uniq = p.
                Where(Function(v) Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v)).
                Select(Function(v) ClampProbability(v)).
                Distinct().
                OrderBy(Function(v) v).
                ToList()

            If includeEndpoints Then
                If uniq.Count = 0 OrElse Math.Abs(uniq(0) - 0.0) > EPS Then uniq.Insert(0, 0.0)
                If Math.Abs(uniq(uniq.Count - 1) - 1.0) > EPS Then uniq.Add(1.0)
            End If

            If maxRows > 0 AndAlso uniq.Count > maxRows Then
                Return ThinSortedValues(uniq.ToArray(), maxRows)
            End If

            Return uniq.ToArray()
        End Function

        ''' <summary>
        ''' Builds percentile-cut bins using the same tie-collapsing logic as
        ''' GLM.HosmerLemeshowTest. The requested bin count is treated as a target;
        ''' identical percentile cutpoints are collapsed, so the returned bin count
        ''' can be smaller than requested.
        ''' </summary>
        Public Function BuildPercentileCutpointBins(p() As Double,
                                                  Optional requestedBins As Integer = 10) As List(Of ProbabilityCutpointBin)
            If p Is Nothing OrElse p.Length = 0 Then Return New List(Of ProbabilityCutpointBin)()
            If requestedBins < 2 Then Throw New ArgumentOutOfRangeException("requestedBins", "Number of requested bins must be at least 2.")

            Dim clean = p.
                Where(Function(v) Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v)).
                Select(Function(v) ClampProbability(v)).
                ToArray()
            If clean.Length = 0 Then Return New List(Of ProbabilityCutpointBin)()

            Dim sorted = DirectCast(clean.Clone(), Double())
            Array.Sort(sorted)

            Dim rawCuts(requestedBins - 1) As Double
            For i = 0 To requestedBins - 1
                rawCuts(i) = PercentileSortedInclusive(sorted, (i + 1) / CDbl(requestedBins))
            Next

            Dim cutpoints() As Double = rawCuts.Distinct().OrderBy(Function(v) v).ToArray()
            Dim byBin As New Dictionary(Of Integer, List(Of Integer))()

            For i = 0 To p.Length - 1
                Dim pi As Double = ClampProbability(p(i))
                Dim binKey As Integer = Array.FindIndex(cutpoints, Function(c) pi <= c)
                If binKey < 0 Then binKey = cutpoints.Length - 1
                If Not byBin.ContainsKey(binKey) Then byBin(binKey) = New List(Of Integer)()
                byBin(binKey).Add(i)
            Next

            Dim out As New List(Of ProbabilityCutpointBin)(byBin.Count)
            Dim orderedKeys = byBin.Keys.OrderBy(Function(k) k).ToArray()
            For j = 0 To orderedKeys.Length - 1
                Dim key = orderedKeys(j)
                out.Add(New ProbabilityCutpointBin With {
                    .BinIndex = j + 1,
                    .CutUpper = cutpoints(key),
                    .Indices = byBin(key).ToArray()
                })
            Next

            Return out
        End Function

        Private Function BuildQuantileCalibrationBins(y() As Double, p() As Double, weights() As Double,
                                                      bins As Integer) As List(Of CalibrationBinSummary)
            Dim idx = Enumerable.Range(0, p.Length).OrderBy(Function(i) p(i)).ThenBy(Function(i) y(i)).ToArray()

            Dim actualBins As Integer = Math.Min(Math.Max(2, bins), p.Length)
            Dim groups As New List(Of List(Of Integer))(actualBins)
            For b = 0 To actualBins - 1
                groups.Add(New List(Of Integer))
            Next

            For rank = 0 To idx.Length - 1
                Dim b As Integer = CInt(Math.Floor(rank * actualBins / CDbl(idx.Length)))
                If b < 0 Then b = 0
                If b >= actualBins Then b = actualBins - 1
                groups(b).Add(idx(rank))
            Next

            Dim out As New List(Of CalibrationBinSummary)
            For b = 0 To groups.Count - 1
                If groups(b).Count = 0 Then Continue For
                out.Add(SummarizeCalibrationBin(b + 1, groups(b), y, p, weights))
            Next
            Return out
        End Function

        Private Function BuildEqualWidthCalibrationBins(y() As Double, p() As Double, weights() As Double,
                                                        bins As Integer) As List(Of CalibrationBinSummary)
            Dim groups As New List(Of Integer)(p.Length)
            Dim byBin As New Dictionary(Of Integer, List(Of Integer))

            For i = 0 To p.Length - 1
                Dim scaled As Double = ClampProbability(p(i)) * bins
                Dim b As Integer = CInt(Math.Floor(scaled)) + 1
                If b > bins Then b = bins
                If b < 1 Then b = 1
                If Not byBin.ContainsKey(b) Then byBin(b) = New List(Of Integer)
                byBin(b).Add(i)
            Next

            Dim out As New List(Of CalibrationBinSummary)
            For b = 1 To bins
                If byBin.ContainsKey(b) AndAlso byBin(b).Count > 0 Then
                    out.Add(SummarizeCalibrationBin(b, byBin(b), y, p, weights))
                End If
            Next
            Return out
        End Function

        Private Function SummarizeCalibrationBin(binIndex As Integer, idx As List(Of Integer),
                                                 y() As Double, p() As Double,
                                                 weights() As Double) As CalibrationBinSummary
            Dim sumW As Double = 0.0
            Dim sumP As Double = 0.0
            Dim sumY As Double = 0.0
            Dim minP As Double = Double.PositiveInfinity
            Dim maxP As Double = Double.NegativeInfinity
            Dim sumW2 As Double = 0.0

            For Each i In idx
                Dim wi As Double = weights(i)
                Dim pi As Double = ClampProbability(p(i))
                Dim yi As Double = If(y(i) >= 0.5, 1.0, 0.0)
                sumW += wi
                sumW2 += wi * wi
                sumP += wi * pi
                sumY += wi * yi
                If pi < minP Then minP = pi
                If pi > maxP Then maxP = pi
            Next

            Dim meanP As Double = If(sumW > 0, sumP / sumW, Double.NaN)
            Dim obsRate As Double = If(sumW > 0, sumY / sumW, Double.NaN)
            Dim effN As Double = EffectiveSampleSize(sumW, sumW2)
            Dim ci = WilsonInterval(obsRate, effN)

            Return New CalibrationBinSummary With {
                .BinIndex = binIndex,
                .N = sumW,
                .MeanPredicted = meanP,
                .ObservedRate = obsRate,
                .LowerCI = ci.Item1,
                .UpperCI = ci.Item2,
                .ThresholdLower = minP,
                .ThresholdUpper = maxP
            }
        End Function

        Private Sub PopulateDerivedMetrics(ByRef out As BinaryClassificationSummary)
            Dim pos As Double = out.TP + out.FN
            Dim neg As Double = out.TN + out.FP
            Dim predPos As Double = out.TP + out.FP
            Dim predNeg As Double = out.TN + out.FN

            out.Sensitivity = SafeDivide(out.TP, pos)
            out.Recall = out.Sensitivity
            out.Specificity = SafeDivide(out.TN, neg)
            out.Precision = SafeDivide(out.TP, predPos)
            out.NPV = SafeDivide(out.TN, predNeg)
            out.Accuracy = SafeDivide(out.TP + out.TN, out.N)
            out.BalancedAccuracy = 0.5 * (Nz(out.Sensitivity) + Nz(out.Specificity))
            out.YoudenJ = Nz(out.Sensitivity) + Nz(out.Specificity) - 1.0
            out.F1 = SafeDivide(2.0 * out.TP, 2.0 * out.TP + out.FP + out.FN)
            out.Prevalence = SafeDivide(pos, out.N)
        End Sub

        Private Function ToThresholdRow(s As BinaryClassificationSummary) As BinaryThresholdRow
            Return New BinaryThresholdRow With {
                .Threshold = s.Threshold,
                .TP = s.TP,
                .FP = s.FP,
                .TN = s.TN,
                .FN = s.FN,
                .Sensitivity = s.Sensitivity,
                .Specificity = s.Specificity,
                .Precision = s.Precision,
                .Recall = s.Recall,
                .NPV = s.NPV,
                .Accuracy = s.Accuracy,
                .BalancedAccuracy = s.BalancedAccuracy,
                .YoudenJ = s.YoudenJ,
                .F1 = s.F1,
                .Prevalence = s.Prevalence,
                .N = s.N
            }
        End Function

        Private Function PercentileSortedInclusive(sortedValues() As Double, p As Double) As Double
            If sortedValues Is Nothing OrElse sortedValues.Length = 0 Then
                Throw New ArgumentException("At least one sorted value is required.", NameOf(sortedValues))
            End If

            Dim pp As Double = Math.Min(1.0, Math.Max(0.0, p))
            Dim h As Double = (sortedValues.Length - 1) * pp
            Dim lo As Integer = CInt(Math.Floor(h))
            Dim hi As Integer = CInt(Math.Ceiling(h))
            If lo = hi Then Return sortedValues(lo)

            Dim frac As Double = h - lo
            Return sortedValues(lo) + frac * (sortedValues(hi) - sortedValues(lo))
        End Function

        Private Function NormalizeWeights(n As Integer, weights() As Double) As Double()
            If weights Is Nothing Then
                Dim out(n - 1) As Double
                For i = 0 To n - 1
                    out(i) = 1.0
                Next
                Return out
            End If

            Dim copy(n - 1) As Double
            Array.Copy(weights, copy, n)
            Return copy
        End Function

        Private Function NormalizeThresholds(thresholds() As Double) As Double()
            If thresholds Is Nothing OrElse thresholds.Length = 0 Then Return New Double() {}
            Dim vals = thresholds.Where(Function(v) Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v)).
                                 Select(Function(v)
                                            AppInfrastructure.ValidateClosedUnitInterval(v, "threshold", "Threshold must lie in [0,1].")
                                            Return ClampProbability(v)
                                        End Function).
                                  Distinct().OrderBy(Function(v) v).ToArray()
            Return vals
        End Function

        Private Function ThinSortedValues(sortedValues() As Double, maxRows As Integer) As Double()
            If sortedValues Is Nothing OrElse sortedValues.Length <= maxRows OrElse maxRows < 2 Then Return sortedValues

            Dim out(maxRows - 1) As Double
            Dim last As Integer = sortedValues.Length - 1
            For i = 0 To maxRows - 1
                Dim idx As Integer = CInt(Math.Round(i * last / CDbl(maxRows - 1)))
                out(i) = sortedValues(idx)
            Next
            Return out.Distinct().OrderBy(Function(v) v).ToArray()
        End Function

        Private Function EffectiveSampleSize(sumW As Double, sumW2 As Double) As Double
            If sumW <= 0 OrElse sumW2 <= 0 Then Return 0.0
            Return (sumW * sumW) / sumW2
        End Function

        Private Function WilsonInterval(pHat As Double, nEff As Double,
                                        Optional z As Double = 1.959963984540054) As Tuple(Of Double, Double)
            If Double.IsNaN(pHat) OrElse nEff <= 0 Then Return Tuple.Create(Double.NaN, Double.NaN)

            pHat = ClampProbability(pHat)
            Dim z2 As Double = z * z
            Dim denom As Double = 1.0 + z2 / nEff
            Dim center As Double = (pHat + z2 / (2.0 * nEff)) / denom
            Dim halfWidth As Double = (z / denom) * Math.Sqrt(Math.Max(0.0, (pHat * (1.0 - pHat) / nEff) + z2 / (4.0 * nEff * nEff)))

            Return Tuple.Create(ClampProbability(center - halfWidth), ClampProbability(center + halfWidth))
        End Function

        ''' <summary>
        ''' Builds a list of <see cref="ResultTable"/> objects for binary-classification
        ''' reporting, analogous to model-level <c>wrapResults()</c> methods used elsewhere
        ''' in the application.
        ''' </summary>
        ''' <param name="summary">Single-threshold classification summary.</param>
        ''' <param name="thresholdRows">
        ''' Optional threshold-performance rows. Pass <c>Nothing</c> to omit the table.
        ''' </param>
        ''' <param name="calibrationRows">
        ''' Optional calibration-bin rows. Pass <c>Nothing</c> to omit the table.
        ''' </param>
        ''' <param name="brierScore">
        ''' Optional Brier score. Pass <see cref="Double.NaN"/> to omit the table.
        ''' </param>
        ''' <param name="eventRate">
        ''' Optional event rate displayed together with the Brier score.
        ''' Pass <see cref="Double.NaN"/> to omit.
        ''' </param>
        ''' <param name="analysisLabel">
        ''' Prefix used in table titles, for example <c>GLM Binary Classification</c>.
        ''' </param>
        ''' <returns>
        ''' A list of <see cref="ResultTable"/> instances ready to be written by
        ''' <see cref="ProcessListofResultTables"/>.
        ''' </returns>
        Public Function WrapResults(summary As BinaryClassificationSummary,
                                    Optional thresholdRows As IList(Of BinaryThresholdRow) = Nothing,
                                    Optional calibrationRows As IList(Of CalibrationBinSummary) = Nothing,
                                    Optional brierScore As Double = Double.NaN,
                                    Optional eventRate As Double = Double.NaN,
                                    Optional analysisLabel As String = "Binary Classification") As List(Of ResultTable)

            Dim out As New List(Of ResultTable)
            out.Add(BuildConfusionMatrixTable(summary, analysisLabel))
            out.Add(BuildClassificationSummaryTable(summary, analysisLabel))
            If calibrationRows IsNot Nothing AndAlso calibrationRows.Count > 0 Then out.Add(BuildCalibrationTable(calibrationRows, analysisLabel))
            If Not Double.IsNaN(brierScore) Then out.Add(BuildBrierScoreTable(brierScore, summary.N, eventRate, analysisLabel))
            If thresholdRows IsNot Nothing AndAlso thresholdRows.Count > 0 Then out.Add(BuildThresholdPerformanceTable(thresholdRows, analysisLabel))

            Return out
        End Function


        ''' <summary>
        ''' Builds an Excel spill range for a binary confusion-matrix style crosstab.
        ''' </summary>
        Public Function BuildBinaryCrosstabUdfOutput(summary As BinaryClassificationSummary,
                                                     Optional includeHeader As Boolean = True) As Object
            Dim rowOffset As Integer = If(includeHeader, 1, 0)
            Dim outRows As Integer = rowOffset + 4
            Dim out(outRows - 1, 3) As Object

            If includeHeader Then
                out(0, 0) = "Observed \ Predicted"
                out(0, 1) = 0
                out(0, 2) = 1
                out(0, 3) = "Recall %"
            End If

            out(rowOffset + 0, 0) = 0
            out(rowOffset + 0, 1) = summary.TN
            out(rowOffset + 0, 2) = summary.FP
            out(rowOffset + 0, 3) = PercentOrExcelNa(summary.Specificity)

            out(rowOffset + 1, 0) = 1
            out(rowOffset + 1, 1) = summary.FN
            out(rowOffset + 1, 2) = summary.TP
            out(rowOffset + 1, 3) = PercentOrExcelNa(summary.Sensitivity)

            out(rowOffset + 2, 0) = "Precision % / Overall"
            out(rowOffset + 2, 1) = PercentOrExcelNa(summary.NPV)
            out(rowOffset + 2, 2) = PercentOrExcelNa(summary.Precision)
            out(rowOffset + 2, 3) = PercentOrExcelNa(summary.Accuracy)

            out(rowOffset + 3, 0) = "Threshold / Balanced accuracy"
            out(rowOffset + 3, 1) = summary.Threshold
            out(rowOffset + 3, 2) = PercentOrExcelNa(summary.BalancedAccuracy)
            out(rowOffset + 3, 3) = PercentOrExcelNa(summary.YoudenJ)

            Return PrepareResultTableForUdfLocal(out)
        End Function

        ''' <summary>
        ''' Builds an Excel spill range for threshold-performance rows.
        ''' </summary>
        Public Function BuildThresholdTableUdfOutput(rows As IList(Of BinaryThresholdRow),
                                                     Optional includeHeader As Boolean = True) As Object
            If rows Is Nothing OrElse rows.Count = 0 Then Return ExcelError.ExcelErrorNA

            Dim rowOffset As Integer = If(includeHeader, 1, 0)
            Dim outRows As Integer = rowOffset + rows.Count
            Dim out(outRows - 1, 13) As Object

            If includeHeader Then
                out(0, 0) = "Threshold"
                out(0, 1) = "TP"
                out(0, 2) = "FP"
                out(0, 3) = "TN"
                out(0, 4) = "FN"
                out(0, 5) = "Sensitivity %"
                out(0, 6) = "Specificity %"
                out(0, 7) = "Precision %"
                out(0, 8) = "Recall %"
                out(0, 9) = "NPV %"
                out(0, 10) = "Accuracy %"
                out(0, 11) = "BalancedAccuracy %"
                out(0, 12) = "YoudenJ %"
                out(0, 13) = "F1 %"
            End If

            For i As Integer = 0 To rows.Count - 1
                Dim r As BinaryThresholdRow = rows(i)
                Dim rr As Integer = rowOffset + i
                out(rr, 0) = r.Threshold
                out(rr, 1) = r.TP
                out(rr, 2) = r.FP
                out(rr, 3) = r.TN
                out(rr, 4) = r.FN
                out(rr, 5) = PercentOrExcelNa(r.Sensitivity)
                out(rr, 6) = PercentOrExcelNa(r.Specificity)
                out(rr, 7) = PercentOrExcelNa(r.Precision)
                out(rr, 8) = PercentOrExcelNa(r.Recall)
                out(rr, 9) = PercentOrExcelNa(r.NPV)
                out(rr, 10) = PercentOrExcelNa(r.Accuracy)
                out(rr, 11) = PercentOrExcelNa(r.BalancedAccuracy)
                out(rr, 12) = PercentOrExcelNa(r.YoudenJ)
                out(rr, 13) = PercentOrExcelNa(r.F1)
            Next

            Return PrepareResultTableForUdfLocal(out)
        End Function

        ''' <summary>
        ''' Builds an Excel spill range for calibration-bin rows.
        ''' </summary>
        Public Function BuildCalibrationTableUdfOutput(rows As IList(Of CalibrationBinSummary),
                                                       Optional includeHeader As Boolean = True) As Object
            If rows Is Nothing OrElse rows.Count = 0 Then Return ExcelError.ExcelErrorNA

            Dim rowOffset As Integer = If(includeHeader, 1, 0)
            Dim outRows As Integer = rowOffset + rows.Count
            Dim out(outRows - 1, 5) As Object

            If includeHeader Then
                out(0, 0) = "Bin"
                out(0, 1) = "N"
                out(0, 2) = "MeanPredicted"
                out(0, 3) = "ObservedRate"
                out(0, 4) = "LowerCI"
                out(0, 5) = "UpperCI"
            End If

            For i As Integer = 0 To rows.Count - 1
                Dim r As CalibrationBinSummary = rows(i)
                Dim rr As Integer = rowOffset + i
                out(rr, 0) = r.BinIndex
                out(rr, 1) = r.N
                out(rr, 2) = r.MeanPredicted
                out(rr, 3) = r.ObservedRate
                out(rr, 4) = r.LowerCI
                out(rr, 5) = r.UpperCI
            Next

            Return PrepareResultTableForUdfLocal(out)
        End Function

        ''' <summary>
        ''' Builds an Excel spill range for chart-ready calibration points.
        ''' </summary>
        Public Function BuildCalibrationPointsUdfOutput(rows As IList(Of CalibrationBinSummary)) As Object
            If rows Is Nothing OrElse rows.Count = 0 Then Return ExcelError.ExcelErrorNA

            Dim out(rows.Count, 7) As Object
            out(0, 0) = "Bin"
            out(0, 1) = "N"
            out(0, 2) = "MeanPredicted"
            out(0, 3) = "ObservedRate"
            out(0, 4) = "LowerCI"
            out(0, 5) = "UpperCI"
            out(0, 6) = "ErrorMinus"
            out(0, 7) = "ErrorPlus"

            For i As Integer = 0 To rows.Count - 1
                Dim r As CalibrationBinSummary = rows(i)
                out(i + 1, 0) = r.BinIndex
                out(i + 1, 1) = r.N
                out(i + 1, 2) = r.MeanPredicted
                out(i + 1, 3) = r.ObservedRate
                out(i + 1, 4) = r.LowerCI
                out(i + 1, 5) = r.UpperCI
                out(i + 1, 6) = If(Double.IsNaN(r.LowerCI), ExcelError.ExcelErrorNA, r.ObservedRate - r.LowerCI)
                out(i + 1, 7) = If(Double.IsNaN(r.UpperCI), ExcelError.ExcelErrorNA, r.UpperCI - r.ObservedRate)
            Next

            Return PrepareResultTableForUdfLocal(out)
        End Function

        ''' <summary>
        ''' Builds an Excel spill range for the Brier score and optional supporting scalars.
        ''' </summary>
        Public Function BuildBrierScoreUdfOutput(score As Double,
                                                 Optional n As Double = Double.NaN,
                                                 Optional eventRate As Double = Double.NaN,
                                                 Optional includeHeader As Boolean = True) As Object
            Dim totalRows As Integer = 1
            If Not Double.IsNaN(n) Then totalRows += 1
            If Not Double.IsNaN(eventRate) Then totalRows += 1

            Dim rowOffset As Integer = If(includeHeader, 1, 0)
            Dim outRows As Integer = rowOffset + totalRows
            Dim out(outRows - 1, 1) As Object

            If includeHeader Then
                out(0, 0) = "Item"
                out(0, 1) = "Value"
            End If

            Dim r As Integer = rowOffset
            out(r, 0) = "BrierScore"
            out(r, 1) = score
            r += 1

            If Not Double.IsNaN(n) Then
                out(r, 0) = "N"
                out(r, 1) = n
                r += 1
            End If

            If Not Double.IsNaN(eventRate) Then
                out(r, 0) = "EventRate"
                out(r, 1) = eventRate
            End If

            Return PrepareResultTableForUdfLocal(out)
        End Function

        ''' <summary>
        ''' Builds the two-group numeric arrays required by <see cref="graphics.ROC"/> from
        ''' binary outcomes and fitted probabilities.
        '''
        ''' Group layout expected by <see cref="graphics.ROC"/>:
        ''' <list type="bullet">
        '''   <item><description><c>x(0)</c> = positive / event group (<c>y = 1</c>)</description></item>
        '''   <item><description><c>x(1)</c> = negative / non-event group (<c>y = 0</c>)</description></item>
        ''' </list>
        '''
        ''' Each group contains the predicted probabilities for the corresponding observations.
        ''' The ROC class then interprets those probabilities as marker values.
        '''
        ''' <para>
        ''' Returns <c>Nothing</c> when the input does not contain at least one event and one
        ''' non-event observation, because an ROC curve cannot be computed in that case.
        ''' </para>
        ''' </summary>
        ''' <param name="y">Observed binary outcomes encoded as 0/1.</param>
        ''' <param name="p">Predicted probabilities on the interval [0,1].</param>
        ''' <returns>
        ''' A jagged array with two elements:
        ''' <list type="bullet">
        '''   <item><description><c>result(0)</c> = probabilities for observations with <c>y = 1</c></description></item>
        '''   <item><description><c>result(1)</c> = probabilities for observations with <c>y = 0</c></description></item>
        ''' </list>
        ''' or <c>Nothing</c> if an ROC curve cannot be constructed.
        ''' </returns>
        Public Function BuildRocInputFromBinaryProbabilities(y() As Double, p() As Double) As Double()()
            If y Is Nothing OrElse p Is Nothing Then Return Nothing
            If y.Length <> p.Length OrElse y.Length = 0 Then Return Nothing

            Dim positive As New List(Of Double)()
            Dim negative As New List(Of Double)()

            For i As Integer = 0 To y.Length - 1
                If y(i) >= 0.5R Then
                    positive.Add(p(i))
                Else
                    negative.Add(p(i))
                End If
            Next

            If positive.Count = 0 OrElse negative.Count = 0 Then Return Nothing

            Return New Double()() {positive.ToArray(), negative.ToArray()}
        End Function

        ''' <summary>
        ''' Computes an ROC analysis object from observed binary outcomes and fitted probabilities.
        ''' </summary>
        ''' <param name="y">Observed binary outcomes encoded as 0/1.</param>
        ''' <param name="p">Predicted probabilities on the interval [0,1].</param>
        ''' <param name="alphaValue">
        ''' Two-sided significance level used for the ROC confidence intervals and AUC test.
        ''' </param>
        ''' <returns>
        ''' A computed <see cref="graphics.ROC"/> instance, or <c>Nothing</c> if an ROC curve
        ''' cannot be constructed from the supplied data.
        ''' </returns>
        Private Function BuildRocFromBinaryProbabilities(y() As Double,
                                                 p() As Double,
                                                 alphaValue As Double) As graphics.ROC
            Dim rocInput As Double()() = BuildRocInputFromBinaryProbabilities(y, p)
            If rocInput Is Nothing Then Return Nothing

            Dim varNames() As String = {"Event (y = 1)", "Non-event (y = 0)"}
            Dim roc As New graphics.ROC(rocInput, varNames)
            roc.compute(alphaValue)
            Return roc
        End Function

        ''' <summary>
        ''' Computes an ROC curve from observed binary outcomes and fitted probabilities,
        ''' writes the tabular ROC results to the current classification worksheet, and
        ''' adds the ROC chart below the written tables.
        '''
        ''' The tabular results are taken directly from <see cref="graphics.ROC.wrapResults"/>
        ''' and therefore include:
        ''' <list type="bullet">
        '''   <item><description>overall ROC summary (AUC, confidence intervals, standard errors, and p-value)</description></item>
        '''   <item><description>cut-off table (cut-off, sensitivity, specificity)</description></item>
        ''' </list>
        '''
        ''' <para>
        ''' The ROC chart is then added to the same worksheet and positioned below the last
        ''' written ROC table, using the writer’s updated row pointer.
        ''' </para>
        ''' </summary>
        ''' <param name="writer">
        ''' <see cref="WriteResults"/> instance already pointing at the classification worksheet.
        ''' The current row pointer is used as the insertion location for the ROC tables.
        ''' </param>
        ''' <param name="y">Observed binary outcomes encoded as 0/1.</param>
        ''' <param name="p">Predicted probabilities on the interval [0,1].</param>
        ''' <param name="alphaValue">
        ''' Two-sided significance level used for ROC confidence intervals and the AUC test.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' <c>True</c> if the ROC results were successfully computed and written; otherwise <c>False</c>.
        ''' </returns>
        Public Function AddRocResultsAndPlotToClassificationSheet(writer As WriteResults, y() As Double, p() As Double,
                                                                  Optional alphaValue As Double = 0.05R) As Boolean
            If writer Is Nothing OrElse writer.ws Is Nothing Then Return False

            Dim roc As graphics.ROC = BuildRocFromBinaryProbabilities(y, p, alphaValue)
            If roc Is Nothing Then Return False

            Dim rocRes As List(Of ResultTable) = roc.wrapResults()
            If rocRes IsNot Nothing AndAlso rocRes.Count > 0 Then
                writer.shiftRowPointer() 'blank row separator before ROC output
                Dim rrRoc As New ProcessListofResultTables(rocRes)
                rrRoc.writeToSheet(writer, True)
            End If

            roc.addROCplot(writer.ws)

            Return True
        End Function

        Private Function BuildConfusionMatrixTable(summary As BinaryClassificationSummary, analysisLabel As String) As ResultTable
            Dim t As New ResultTable
            t.AddTitle($"{analysisLabel} - Confusion Matrix")

            Dim body(2, 2) As Object
            body(0, 0) = summary.TN
            body(0, 1) = summary.FP
            body(0, 2) = DisplayPercent(summary.Specificity)

            body(1, 0) = summary.FN
            body(1, 1) = summary.TP
            body(1, 2) = DisplayPercent(summary.Sensitivity)

            body(2, 0) = DisplayPercent(summary.NPV)
            body(2, 1) = DisplayPercent(summary.Precision)
            body(2, 2) = DisplayPercent(summary.Accuracy)

            t.SetBody(body)
            t.AddHeaderTopRow({"Predicted 0", "Predicted 1", "Recall / Overall %"})
            t.AddHeaderLeftRow({"Observed 0", "Observed 1", "Precision / Overall"})
            t.AddFootnote("Positive classification is assigned when fitted probability >= threshold.")
            Return t
        End Function

        Private Function BuildClassificationSummaryTable(summary As BinaryClassificationSummary, analysisLabel As String) As ResultTable
            Dim t As New ResultTable
            t.AddTitle($"{analysisLabel} - Summary")
            t.SetBody(New Object(,) {
                {"Threshold", summary.Threshold},
                {"Weighted N", summary.N},
                {"Prevalence %", DisplayPercent(summary.Prevalence)},
                {"Sensitivity %", DisplayPercent(summary.Sensitivity)},
                {"Specificity %", DisplayPercent(summary.Specificity)},
                {"Precision %", DisplayPercent(summary.Precision)},
                {"NPV %", DisplayPercent(summary.NPV)},
                {"Accuracy %", DisplayPercent(summary.Accuracy)},
                {"Balanced accuracy %", DisplayPercent(summary.BalancedAccuracy)},
                {"Youden J %", DisplayPercent(summary.YoudenJ)},
                {"F1 %", DisplayPercent(summary.F1)},
                {"True positives", summary.TP},
                {"False positives", summary.FP},
                {"True negatives", summary.TN},
                {"False negatives", summary.FN}
            })
            Return t
        End Function

        Private Function BuildThresholdPerformanceTable(rows As IList(Of BinaryThresholdRow), analysisLabel As String) As ResultTable
            Dim t As New ResultTable
            t.AddTitle($"{analysisLabel} - Threshold Performance")

            Dim body(rows.Count - 1, 13) As Object
            For i As Integer = 0 To rows.Count - 1
                Dim r = rows(i)
                body(i, 0) = r.Threshold
                body(i, 1) = r.TP
                body(i, 2) = r.FP
                body(i, 3) = r.TN
                body(i, 4) = r.FN
                body(i, 5) = DisplayPercent(r.Sensitivity)
                body(i, 6) = DisplayPercent(r.Specificity)
                body(i, 7) = DisplayPercent(r.Precision)
                body(i, 8) = DisplayPercent(r.Recall)
                body(i, 9) = DisplayPercent(r.NPV)
                body(i, 10) = DisplayPercent(r.Accuracy)
                body(i, 11) = DisplayPercent(r.BalancedAccuracy)
                body(i, 12) = DisplayPercent(r.YoudenJ)
                body(i, 13) = DisplayPercent(r.F1)
            Next

            t.SetBody(body)
            t.AddHeaderTopRow({"Threshold", "TP", "FP", "TN", "FN",
                               "Sensitivity %", "Specificity %", "Precision %", "Recall %",
                               "NPV %", "Accuracy %", "Balanced Accuracy %", "Youden J %", "F1 %"})
            Return t
        End Function

        Private Function BuildCalibrationTable(rows As IList(Of CalibrationBinSummary), analysisLabel As String) As ResultTable
            Dim t As New ResultTable
            t.AddTitle($"{analysisLabel} - Calibration")

            Dim body(rows.Count - 1, 5) As Object
            For i As Integer = 0 To rows.Count - 1
                Dim r = rows(i)
                body(i, 0) = r.BinIndex
                body(i, 1) = r.N
                body(i, 2) = r.MeanPredicted
                body(i, 3) = r.ObservedRate
                body(i, 4) = r.LowerCI
                body(i, 5) = r.UpperCI
            Next

            t.SetBody(body)
            t.AddHeaderTopRow({"Bin", "N", "Mean Predicted", "Observed Rate", "Lower CI", "Upper CI"})
            t.AddFootnote("Calibration is reported on the probability scale [0,1].")
            Return t
        End Function

        Private Function BuildBrierScoreTable(brierScore As Double, n As Double,
                                              eventRate As Double, analysisLabel As String) As ResultTable
            Dim t As New ResultTable
            t.AddTitle($"{analysisLabel} - Brier Score")

            If Double.IsNaN(eventRate) Then
                t.SetBody(New Object(,) {{"Brier score", brierScore}, {"Weighted N", n}})
            Else
                t.SetBody(New Object(,) {{"Brier score", brierScore}, {"Weighted N", n}, {"Event rate", eventRate}})
            End If
            Return t
        End Function

        Private Function DisplayPercent(value As Double) As Object
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return "N/A"
            Return 100.0R * value
        End Function

        Private Function PercentOrExcelNa(value As Double) As Object
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return ExcelError.ExcelErrorNA
            Return 100.0R * value
        End Function

        Private Function ClampProbability(value As Double) As Double
            If Double.IsNaN(value) Then Return 0.0R
            If value <= 0.0R Then Return 0.0R
            If value >= 1.0R Then Return 1.0R
            Return value
        End Function

        Private Function PrepareResultTableForUdfLocal(table As Object(,)) As Object(,)
            If table Is Nothing Then Return Nothing

            Dim rows As Integer = table.GetLength(0)
            Dim cols As Integer = table.GetLength(1)
            Dim out(rows - 1, cols - 1) As Object

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    Dim v As Object = table(r, c)

                    If v Is Nothing OrElse TypeOf v Is DBNull Then
                        out(r, c) = String.Empty
                    Else
                        out(r, c) = v
                    End If
                Next
            Next

            Return out
        End Function

        Private Function IsApproximatelyBinary(value As Double) As Boolean
            Return Math.Abs(value) <= EPS OrElse Math.Abs(value - 1.0) <= EPS
        End Function

        Private Function SafeDivide(num As Double, den As Double) As Double
            If Math.Abs(den) <= EPS Then Return Double.NaN
            Return num / den
        End Function

        Private Function Nz(value As Double) As Double
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return 0.0
            Return value
        End Function

    End Module
End Namespace
