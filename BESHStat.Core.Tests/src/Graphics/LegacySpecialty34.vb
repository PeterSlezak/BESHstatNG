Option Explicit On
Option Strict Off
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.regression
Imports BESHStatNG.StatFunc
Namespace Global.BESHStatNG.TestFixtures34
Public Module LegacySpecialty34
Public Function SpreadLabelPositions(values As Double(),
                                                 minimum As Double,
                                                 maximum As Double,
                                                 requestedSeparation As Double) As Double()
        Dim result As Double() = DirectCast(values.Clone(), Double())
        If values.Length < 2 OrElse Not IsFinitePositive(requestedSeparation) Then Return result

        Dim span As Double = maximum - minimum
        If Not IsFinitePositive(span) Then Return result

        Dim orderedIndices As Integer() = Enumerable.Range(0, values.Length).
                                                   OrderBy(Function(i) values(i)).
                                                   ThenBy(Function(i) i).
                                                   ToArray()

        'If the requested physical spacing cannot fit all labels, use the largest feasible
        'spacing instead of forcing anchors outside the plot.
        Dim separation As Double = requestedSeparation
        If values.Length > 1 Then
            separation = Math.Min(separation, span / CDbl(values.Length - 1))
        End If
        If separation <= 0.0R Then Return result

        Dim lowerBound As Double = minimum
        Dim upperBound As Double = maximum
        If separation < span Then
            lowerBound = minimum + 0.5R * separation
            upperBound = maximum - 0.5R * separation
        End If

        Dim adjusted(values.Length - 1) As Double
        adjusted(0) = Clamp(values(orderedIndices(0)), lowerBound, upperBound)
        For j As Integer = 1 To orderedIndices.Length - 1
            Dim desired As Double = Clamp(values(orderedIndices(j)), lowerBound, upperBound)
            adjusted(j) = Math.Max(desired, adjusted(j - 1) + separation)
        Next

        If adjusted(adjusted.Length - 1) > upperBound Then
            adjusted(adjusted.Length - 1) = upperBound
            For j As Integer = adjusted.Length - 2 To 0 Step -1
                Dim desired As Double = Clamp(values(orderedIndices(j)), lowerBound, upperBound)
                adjusted(j) = Math.Min(desired, adjusted(j + 1) - separation)
            Next
        End If

        If adjusted(0) < lowerBound Then
            Dim shift As Double = lowerBound - adjusted(0)
            For j As Integer = 0 To adjusted.Length - 1
                adjusted(j) += shift
            Next
        End If

        'One last backward pass protects the upper edge after a possible bottom shift.
        If adjusted(adjusted.Length - 1) > upperBound Then
            adjusted(adjusted.Length - 1) = upperBound
            For j As Integer = adjusted.Length - 2 To 0 Step -1
                adjusted(j) = Math.Min(adjusted(j), adjusted(j + 1) - separation)
            Next
        End If

        For j As Integer = 0 To orderedIndices.Length - 1
            result(orderedIndices(j)) = Clamp(adjusted(j), minimum, maximum)
        Next
        Return result
    End Function

Public Function Clamp(value As Double, minimum As Double, maximum As Double) As Double
        If value < minimum Then Return minimum
        If value > maximum Then Return maximum
        Return value
    End Function

Public Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

Public Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function

Public Sub ExtendStepToAxisBounds(sourceX() As Double,
                                              sourceY() As Double,
                                              xMinimum As Double,
                                              xMaximum As Double,
                                              ByRef xOut() As Double,
                                              ByRef yOut() As Double)
        If sourceX Is Nothing OrElse sourceY Is Nothing OrElse sourceX.Length = 0 OrElse
           sourceX.Length <> sourceY.Length Then
            xOut = sourceX
            yOut = sourceY
            Return
        End If

        Dim prepend As Boolean = xMinimum < sourceX(0)
        Dim append As Boolean = xMaximum > sourceX(sourceX.Length - 1)
        Dim extra As Integer = If(prepend, 1, 0) + If(append, 1, 0)

        If extra = 0 Then
            xOut = sourceX
            yOut = sourceY
            Return
        End If

        ReDim xOut(sourceX.Length + extra - 1)
        ReDim yOut(sourceY.Length + extra - 1)

        Dim targetIndex As Integer = 0
        If prepend Then
            xOut(targetIndex) = xMinimum
            yOut(targetIndex) = sourceY(0)
            targetIndex += 1
        End If

        For i As Integer = 0 To sourceX.Length - 1
            xOut(targetIndex) = sourceX(i)
            yOut(targetIndex) = sourceY(i)
            targetIndex += 1
        Next

        If append Then
            xOut(targetIndex) = xMaximum
            yOut(targetIndex) = sourceY(sourceY.Length - 1)
        End If
    End Sub

Public Function MsToCentiseconds(ms As Integer) As Integer
        Dim safeMs = If(ms < 0, 0, ms)
        Return Math.Max(1, CInt(Math.Round(safeMs / 10.0)))
    End Function

Public Function BulletX(value As Double, plotLeft As Double, plotWidth As Double) As Double
        Return plotLeft + BulletClip(value) * plotWidth
End Function

Public Function BulletClip(value As Double) As Double
        If value < 0.0R Then Return 0.0R
        If value > 1.0R Then Return 1.0R
        Return value
End Function

Public Function SankeyX(value As Double, stageLeft As Double, stageRight As Double) As Double
        Dim clipped As Double = Math.Max(0.0R, Math.Min(1.0R, value))
        Return stageLeft + clipped * (stageRight - stageLeft)
End Function

Public Function SankeyY(value As Double, plotTop As Double, plotHeight As Double) As Double
        Dim clipped As Double = Math.Max(0.0R, Math.Min(1.0R, value))
        Return plotTop + clipped * plotHeight
End Function

Public Function StageCentres(stageCount As Integer, stageLeft As Double, stageRight As Double) As Double()
        Dim result(stageCount - 1) As Double
        If stageCount = 1 Then
            result(0) = (stageLeft + stageRight) / 2.0R
            Return result
        End If

        For i As Integer = 0 To stageCount - 1
            result(i) = stageLeft +
                        CDbl(i) / CDbl(stageCount - 1) *
                        (stageRight - stageLeft)
        Next
        Return result
End Function

Public Function Calibration(rows As IList(Of CalibrationBinSummary)) As Double()()
            Dim xs As New List(Of Double)()
            Dim ys As New List(Of Double)()
            For Each r As CalibrationBinSummary In rows
                If Double.IsNaN(r.MeanPredicted) OrElse Double.IsNaN(r.ObservedRate) Then Continue For
                xs.Add(r.MeanPredicted)
                ys.Add(r.ObservedRate)
            Next
            If xs.Count = 0 Then Return Nothing

Return New Double()() {xs.ToArray(), ys.ToArray()}
End Function

Public Function Symmetry(data As Double()) As Double()()
Dim n As Integer = data.Length
            Dim Xref() As Double, Yref() As Double

            'prepare data for plotting
            'compute differences below and above median
            Dim Medn As Double = Median(data)
            Array.Sort(data)
            Dim x(n - 1) As Double
            For i = 0 To n - 1
                If data(i) <= Medn Then
                    x(i) = Medn - data(i)
                Else
                    x(i) = data(i) - Medn
                End If
            Next

            'prepare arrays for plotting
            Dim ii As Integer = CInt(RoundDown(n / 2, 0))
            Dim Xs(ii - 1) As Double, Ys(ii - 1) As Double
            For i = 0 To ii - 1
                Xs(i) = x(n - 1 - i)
                Ys(i) = x(i)
            Next

            'reference line data
            If x(0) >= x(n - 1) Then
                Xref = {x(0), x(ii - 1)}
                Yref = Xref
            Else
                Xref = {x(n - 1), x(n - 1 - ii)}
                Yref = Xref
            End If

            'compute optimal scaling
            Dim udPlotAxis = BESHStatNG.ChartAxisScalingCore.Compute(0, x.Max())

Return New Double()() {Xs,Ys,Xref,Yref,x,New Double(){udPlotAxis.Min,udPlotAxis.Max,udPlotAxis.Scale}}
End Function

End Module
End Namespace
