Option Explicit On
Option Strict On

''' <summary>Five signed Peltier stacked segments only. Quartiles and whiskers remain with their existing owners.</summary>
Public Module BoxSegmentsCore

    Public Function Compute(lower As Double, median As Double, upper As Double) As BoxSegmentData
        Dim blank As Double, medianHeight As Double, q3Height As Double, q3Minus As Double, medianMinus As Double
        If lower > 0 Then
            blank = lower
        Else
            blank = If(upper < 0, upper, 0)
        End If

        If median > 0 Then
            If lower > 0 Then
                medianHeight = If(median > lower, median - lower, 0)
            Else
                medianHeight = median
            End If
        Else
            medianHeight = 0
        End If

        If upper > 0 Then
            If median > 0 Then
                q3Height = If(upper > median, upper - median, 0)
            Else
                q3Height = upper
            End If
        Else
            q3Height = 0
        End If

        If median < 0 Then
            If upper < 0 Then
                q3Minus = If(median < upper, median - upper, 0)
            Else
                q3Minus = median
            End If
        Else
            q3Minus = 0
        End If

        If lower < 0 Then
            If median < 0 Then
                medianMinus = If(lower < median, lower - median, 0)
            Else
                medianMinus = lower
            End If
        Else
            medianMinus = 0
        End If

        Return New BoxSegmentData(blank, medianHeight, q3Height, q3Minus, medianMinus)
    End Function
End Module

Public NotInheritable Class BoxSegmentData

    Friend Sub New(blank As Double, median As Double, q3 As Double, q3Minus As Double, medianMinus As Double)
        Me.Blank = blank : Me.Median = median : Me.Q3 = q3 : Me.Q3Minus = q3Minus : Me.MedianMinus = medianMinus
    End Sub

    Public ReadOnly Property Blank As Double

    Public ReadOnly Property Median As Double

    Public ReadOnly Property Q3 As Double

    Public ReadOnly Property Q3Minus As Double

    Public ReadOnly Property MedianMinus As Double
End Class

