Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.StatisticalProcessControl
Public NotInheritable Class SpcZoneBands36
    Private ReadOnly _values As Double?()()
    Public ReadOnly Property ValidPointCount As Integer
    Public Sub New(points As SpcPointResult(), minimum As Double)
Dim categories(points.Length-1) As Double?
            Dim baseValues(categories.Length - 1) As Double?
            Dim lowerOuter(categories.Length - 1) As Double?
            Dim lowerMiddle(categories.Length - 1) As Double?
            Dim lowerCenter(categories.Length - 1) As Double?
            Dim upperCenter(categories.Length - 1) As Double?
            Dim upperMiddle(categories.Length - 1) As Double?
            Dim upperOuter(categories.Length - 1) As Double?
            Dim validBandCount As Integer = 0

            For i As Integer = 0 To categories.Length - 1
                Dim point As SpcPointResult = points(i)
                If point IsNot Nothing AndAlso HasValidZones(point) Then
                    baseValues(i) = point.LowerControlLimit - minimum
                    lowerOuter(i) = point.LowerTwoSigmaLimit - point.LowerControlLimit
                    lowerMiddle(i) = point.LowerOneSigmaLimit - point.LowerTwoSigmaLimit
                    lowerCenter(i) = point.CenterLine - point.LowerOneSigmaLimit
                    upperCenter(i) = point.UpperOneSigmaLimit - point.CenterLine
                    upperMiddle(i) = point.UpperTwoSigmaLimit - point.UpperOneSigmaLimit
                    upperOuter(i) = point.UpperControlLimit - point.UpperTwoSigmaLimit
                    validBandCount += 1
                Else
                    baseValues(i) = Nothing
                    lowerOuter(i) = Nothing
                    lowerMiddle(i) = Nothing
                    lowerCenter(i) = Nothing
                    upperCenter(i) = Nothing
                    upperMiddle(i) = Nothing
                    upperOuter(i) = Nothing
                End If
            Next


ValidPointCount=validBandCount
_values=New Double?()(){baseValues,lowerOuter,lowerMiddle,lowerCenter,upperCenter,upperMiddle,upperOuter}
End Sub
Public ReadOnly Property Values As Double?()()
Get
Return _values.Select(Function(v) DirectCast(v.Clone(),Double?())).ToArray()
End Get
End Property

        Public Shared Function HasValidZones(point As SpcPointResult) As Boolean
            If point Is Nothing Then Return False
            Dim values As Double() = {
                point.LowerControlLimit,
                point.LowerTwoSigmaLimit,
                point.LowerOneSigmaLimit,
                point.CenterLine,
                point.UpperOneSigmaLimit,
                point.UpperTwoSigmaLimit,
                point.UpperControlLimit
            }
            For i As Integer = 0 To values.Length - 1
                If Not IsFinite(values(i)) Then Return False
                If i > 0 AndAlso values(i) < values(i - 1) Then Return False
            Next
            Return True
        End Function

        Private Shared Function IsFinite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function
End Class
