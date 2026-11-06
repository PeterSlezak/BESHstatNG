Option Explicit On
Option Strict On

Imports System

''' <summary>Copy-safe entry to the existing histogram engine; PrettyBreaks remains always on.
''' Horizontal bounds retain the renderer's existing one-bin fallback and width guards.</summary>
Public Module HistogramDataCore

    Public Function BinsCopy(values As Double(), rule As String) As Object(,)
        Return HistogramBinningCore.HistogramBinsComputation(DirectCast(values.Clone(), Double()), rule)
    End Function

    Public Function OverlayCopy(values As Double(), midpoints As Double()) As Double(,)
        Return HistogramBinningCore.GaussOverlayComputation(DirectCast(values.Clone(), Double()), DirectCast(midpoints.Clone(), Double()))
    End Function

    Public Function HorizontalBounds(midpoints As Double()) As HistogramHorizontalBounds
        Dim iNoBins = midpoints.Length
        If iNoBins = 0 Then Throw New ArgumentException("At least one midpoint is required.", NameOf(midpoints))
        Dim dBinWidth As Double
        If iNoBins >= 2 Then
            dBinWidth = Math.Abs(midpoints(1) - midpoints(0))
        Else
            dBinWidth = Math.Max(1.0R, Math.Abs(midpoints(0))) * 2.0R
        End If

        If dBinWidth <= 0.0R OrElse Double.IsNaN(dBinWidth) OrElse Double.IsInfinity(dBinWidth) Then
            dBinWidth = 1.0R
        End If

        Dim minimum As Double = midpoints(0) - dBinWidth / 2.0R
        Dim maximum As Double = midpoints(iNoBins - 1) + dBinWidth / 2.0R
        Return New HistogramHorizontalBounds(minimum, maximum, dBinWidth)
    End Function
End Module

Public NotInheritable Class HistogramHorizontalBounds

    Friend Sub New(minimum As Double, maximum As Double, width As Double)
        Me.Minimum = minimum : Me.Maximum = maximum : Me.Width = width
    End Sub

    Public ReadOnly Property Minimum As Double

    Public ReadOnly Property Maximum As Double

    Public ReadOnly Property Width As Double
End Class

