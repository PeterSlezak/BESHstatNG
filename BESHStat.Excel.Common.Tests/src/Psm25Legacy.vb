Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.CausalInference
Public NotInheritable Class Psm25Legacy
    Public Shared Sub NormalizeFitOptionsForRunMethod(fitOptions As PsmComprehensiveFitOptions)
        Dim runMethod As PsmBackendRunMethod = fitOptions.RunMethod
        If fitOptions.StandardOptions Is Nothing Then Return

        If runMethod = PsmBackendRunMethod.OptimalPairMatching Then
            fitOptions.StandardOptions.MatchingRatio = 1
            fitOptions.StandardOptions.WithReplacement = False
            fitOptions.StandardOptions.MatchingOrder = PsmMatchingOrder.AsInput
        End If

        If Not PsmMethodCapabilities.UsesMatchingControls(runMethod) Then
            fitOptions.StandardOptions.DistanceMetric = PsmDistanceMetric.PropensityScore
            fitOptions.StandardOptions.CaliperScale = PsmCaliperScale.None
            fitOptions.StandardOptions.Caliper = Double.NaN
            fitOptions.StandardOptions.MatchingRatio = 1
            fitOptions.StandardOptions.WithReplacement = False
            fitOptions.StandardOptions.MatchingOrder = PsmMatchingOrder.AsInput
        End If
    End Sub
        Public Shared Function BuildPlotDataTable(rows As IList(Of PsmLovePlotRow)) As Object(,)
            If rows Is Nothing OrElse rows.Count = 0 Then Return PsmResult.EmptyTable("No Love plot data available")

            Dim cleanRows As List(Of PsmLovePlotRow) = rows.
                Where(Function(r) r IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(r.VariableName)).
                OrderByDescending(Function(r) MaxFiniteAbs(r.SmdBefore, r.SmdAfterMatching, r.SmdAfterWeighting)).
                ThenBy(Function(r) r.VariableName, StringComparer.OrdinalIgnoreCase).
                ToList()

            If cleanRows.Count = 0 Then Return PsmResult.EmptyTable("No Love plot data available")

            Dim table(cleanRows.Count, 6) As Object
            table(0, 0) = "Plot Row"
            table(0, 1) = "Variable"
            table(0, 2) = "|SMD| before"
            table(0, 3) = "|SMD| after matching"
            table(0, 4) = "|SMD| after weighting"
            table(0, 5) = "Threshold"
            table(0, 6) = "Flag"

            For i As Integer = 0 To cleanRows.Count - 1
                Dim r As PsmLovePlotRow = cleanRows(i)
                table(i + 1, 0) = i + 1
                table(i + 1, 1) = r.VariableName
                table(i + 1, 2) = AbsOrMissing(r.SmdBefore)
                table(i + 1, 3) = AbsOrMissing(r.SmdAfterMatching)
                table(i + 1, 4) = AbsOrMissing(r.SmdAfterWeighting)
                table(i + 1, 5) = If(Finite(r.Threshold), r.Threshold, Nothing)
                table(i + 1, 6) = r.Flag
            Next

            Return table
        End Function
        Public Shared Function Plan(rows As IList(Of PsmLovePlotRow)) As Object()
            If rows Is Nothing OrElse rows.Count = 0 Then Return EmptyPlan()
            Dim cleanRows As List(Of PsmLovePlotRow) = rows.
                Where(Function(r) r IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(r.VariableName)).
                OrderByDescending(Function(r) MaxFiniteAbs(r.SmdBefore, r.SmdAfterMatching, r.SmdAfterWeighting)).
                ThenBy(Function(r) r.VariableName, StringComparer.OrdinalIgnoreCase).
                ToList()

            If cleanRows.Count = 0 Then Return EmptyPlan()

            Dim yValues As Double() = Enumerable.Range(1, cleanRows.Count).Select(Function(i) CDbl(i)).ToArray()
            Dim xBefore As Double() = cleanRows.Select(Function(r) AbsForChart(r.SmdBefore)).ToArray()
            Dim xAfterMatching As Double() = cleanRows.Select(Function(r) AbsForChart(r.SmdAfterMatching)).ToArray()
            Dim xAfterWeighting As Double() = cleanRows.Select(Function(r) AbsForChart(r.SmdAfterWeighting)).ToArray()
            Dim labels As String() = cleanRows.Select(Function(r) r.VariableName).ToArray()
            Dim threshold As Double = cleanRows.Select(Function(r) r.Threshold).FirstOrDefault(Function(v) Finite(v) AndAlso v > 0)
            If Not Finite(threshold) OrElse threshold <= 0 Then threshold = 0.1R

            Dim maxX As Double = Math.Max(threshold * 1.25R, MaxFinite(xBefore.Concat(xAfterMatching).Concat(xAfterWeighting)) * 1.15R)
            If Not Finite(maxX) OrElse maxX <= 0 Then maxX = 0.25R
            If maxX < 0.25R Then maxX = 0.25R

            Return New Object() {yValues, xBefore, xAfterMatching, xAfterWeighting, labels, threshold, maxX}
        End Function
        Private Shared Function EmptyPlan() As Object()
            Return New Object() {New Double() {}, New Double() {}, New Double() {}, New Double() {}, New String() {}, 0.1R, 0.25R}
        End Function

        Private Shared Function AbsForChart(value As Double) As Double
            If Not Finite(value) Then Return Double.NaN
            Return Math.Abs(value)
        End Function
        Private Shared Function AbsOrMissing(value As Double) As Object
            If Not Finite(value) Then Return Nothing
            Return Math.Abs(value)
        End Function
        Private Shared Function MaxFiniteAbs(ParamArray values As Double()) As Double
            Dim maxValue As Double = Double.NaN
            If values Is Nothing Then Return maxValue
            For Each v As Double In values
                If Finite(v) Then
                    Dim av As Double = Math.Abs(v)
                    If Not Finite(maxValue) OrElse av > maxValue Then maxValue = av
                End If
            Next
            Return maxValue
        End Function
        Private Shared Function MaxFinite(values As IEnumerable(Of Double)) As Double
            Dim maxValue As Double = Double.NaN
            If values Is Nothing Then Return maxValue
            For Each v As Double In values
                If Finite(v) Then
                    If Not Finite(maxValue) OrElse v > maxValue Then maxValue = v
                End If
            Next
            Return maxValue
        End Function
        Private Shared Function Finite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function
End Class
