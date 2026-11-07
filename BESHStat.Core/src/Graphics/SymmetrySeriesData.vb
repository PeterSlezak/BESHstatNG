Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic


''' <summary>Historical upper-tail X / lower-tail Y coordinates. Axis captions remain host-owned.</summary>
Public NotInheritable Class SymmetrySeriesData
    Private ReadOnly _x As Double()
    Private ReadOnly _y As Double()
    Private ReadOnly _ref As Double()
    Public ReadOnly Property Axis As ChartAxisScaleResult
    Private Sub New(x As Double(), y As Double(), reference As Double(), axisValue As ChartAxisScaleResult)
        _x = DirectCast(x.Clone(), Double())
        _y = DirectCast(y.Clone(), Double())
        _ref = DirectCast(reference.Clone(), Double())
        Axis = axisValue
    End Sub
    ''' <summary>Snapshots already sorted observations; the caller owns sorting and median calculation.</summary>
    Public Shared Function FromSorted(values As Double(), medianValue As Double) As SymmetrySeriesData
        Dim sortedData As Double() = DirectCast(values.Clone(), Double())
        Dim n As Integer = sortedData.Length
        Dim Medn As Double = medianValue
        Dim Xref() As Double, Yref() As Double
            Dim x(n - 1) As Double
            For i = 0 To n - 1
                If sortedData(i) <= Medn Then
                    x(i) = Medn - sortedData(i)
                Else
                    x(i) = sortedData(i) - Medn
                End If
            Next

            'prepare arrays for plotting
            Dim ii As Integer = CInt(StatFunc.RoundDown(n / 2, 0))
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
            Dim udPlotAxis = ChartAxisScalingCore.Compute(0, x.Max())

        Return New SymmetrySeriesData(Xs, Ys, Xref, udPlotAxis)
    End Function
    Public ReadOnly Property XValues As Double()
        Get
            Return DirectCast(_x.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property YValues As Double()
        Get
            Return DirectCast(_y.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property ReferenceX As Double()
        Get
            Return DirectCast(_ref.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property ReferenceY As Double()
        Get
            Return DirectCast(_ref.Clone(), Double())
        End Get
    End Property
End Class
