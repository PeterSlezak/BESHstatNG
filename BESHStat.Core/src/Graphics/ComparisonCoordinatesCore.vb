Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic


Public Module ComparisonCoordinatesCore
    Public Function OrdinalY(count As Integer) As Double()
        Dim y(count - 1) As Double
        For i As Integer = 0 To count - 1
            y(i) = count - i
        Next
        Return y
    End Function
    Public Function IncludeZeroBounds(minimum As Double, maximum As Double) As ChartAxisScaleResult
        Return New ChartAxisScaleResult(Math.Min(minimum, 0.0R), Math.Max(maximum, 0.0R), 0.0R)
    End Function
End Module
