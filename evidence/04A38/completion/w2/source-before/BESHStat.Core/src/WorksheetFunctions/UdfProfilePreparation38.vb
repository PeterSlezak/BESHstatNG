Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace WorksheetCompatibility
    ''' <summary>
    ''' Legacy fitted-design averaging. Profile access is lazy so excluded rows
    ''' and empty designs do not acquire new validation or exception phases.
    ''' Inputs are read only; the returned numerical vector is newly allocated.
    ''' The caller supplies its established comparison rule (GEE and MMRM differ).
    ''' </summary>
    Public NotInheritable Class UdfProfilePreparation38
        Private Sub New()
        End Sub

        Public Shared Function AverageDesign(Of T)(design As Double(,), timeValues As Double(),
                                                   timeSpecified As Boolean, targetTime As Double,
                                                   profiles As IEnumerable(Of T), column As Func(Of T, Integer),
                                                   value As Func(Of T, Double), equal As Func(Of Double, Double, Boolean),
                                                   ByRef matchedCount As Integer) As Double()
            matchedCount = 0
            Dim n As Integer = design.GetLength(0)
            Dim p As Integer = design.GetLength(1)
            Dim sums(p - 1) As Double
            For i As Integer = 0 To n - 1
                If timeSpecified Then
                    If timeValues Is Nothing OrElse timeValues.Length <> n Then Continue For
                    If Not equal(timeValues(i), targetTime) Then Continue For
                End If
                Dim match As Boolean = True
                For Each profile As T In profiles
                    If column(profile) < 0 OrElse column(profile) >= p Then
                        match = False
                        Exit For
                    End If
                    If Not equal(design(i, column(profile)), value(profile)) Then
                        match = False
                        Exit For
                    End If
                Next
                If Not match Then Continue For
                matchedCount += 1
                For j As Integer = 0 To p - 1
                    sums(j) += design(i, j)
                Next
            Next
            If matchedCount <= 0 Then Return Nothing
            For j As Integer = 0 To p - 1
                sums(j) /= CDbl(matchedCount)
            Next
            Return sums
        End Function
    End Class
End Namespace
