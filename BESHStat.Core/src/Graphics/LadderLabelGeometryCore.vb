Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic

Public Module LadderLabelGeometryCore
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

Private Function Clamp(value As Double, minimum As Double, maximum As Double) As Double
        If value < minimum Then Return minimum
        If value > maximum Then Return maximum
        Return value
    End Function

Private Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

Private Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Module
