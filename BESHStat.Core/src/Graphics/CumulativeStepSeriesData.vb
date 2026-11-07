Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic


Public NotInheritable Class CumulativeStepSeriesData
    Private ReadOnly _x As Double()
    Private ReadOnly _y As Double()
    Public Sub New(sourceX As Double(), sourceY As Double(), minimum As Double, maximum As Double)
        Dim x As Double() = If(sourceX Is Nothing, Nothing, DirectCast(sourceX.Clone(), Double()))
        Dim y As Double() = If(sourceY Is Nothing, Nothing, DirectCast(sourceY.Clone(), Double()))
        ExtendStepToAxisBounds(x, y, minimum, maximum, _x, _y)
    End Sub
    Public ReadOnly Property XValues As Double()
        Get
            Return If(_x Is Nothing, Nothing, DirectCast(_x.Clone(), Double()))
        End Get
    End Property
    Public ReadOnly Property YValues As Double()
        Get
            Return If(_y Is Nothing, Nothing, DirectCast(_y.Clone(), Double()))
        End Get
    End Property
Private Sub ExtendStepToAxisBounds(sourceX() As Double,
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
End Class
