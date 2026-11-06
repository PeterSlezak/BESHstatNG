Option Explicit On
Option Strict On

Imports System

''' <summary>Normalize one aligned raw axis to the legacy centered cube.
''' Explicit count preserves Windows' X-derived count and ragged-axis exception behavior.
''' Bounds expansion, manual bounds, nonzero-range policy, rotations, clipping and drawing remain Windows-owned.</summary>
Public Module XyzDataCore

    Public Function NormalizeAxis(values As Double(), minimum As Double, range As Double, direction As Double, count As Integer) As Double()
        Dim input = If(values Is Nothing, Nothing, DirectCast(values.Clone(), Double()))
        Dim result(count - 1) As Double
        For i As Integer = 0 To count - 1
            result(i) =((input(i) - minimum) / range - 0.5) * direction
        Next

        Return result
    End Function
End Module

