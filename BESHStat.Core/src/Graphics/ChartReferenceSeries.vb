Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic

''' <summary>Copy-safe two-point reference coordinates; supplied endpoints retain their order.</summary>
Public NotInheritable Class ChartReferenceSeries
    Private ReadOnly _x As Double()
    Private ReadOnly _y As Double()
    Private Sub New(x0 As Double, x1 As Double, y0 As Double, y1 As Double)
        _x = New Double() {x0,x1}
        _y = New Double() {y0,y1}
    End Sub
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
    Public Shared Function Identity(minimum As Double, maximum As Double) As ChartReferenceSeries
        Return New ChartReferenceSeries(minimum,maximum,minimum,maximum)
    End Function
    Public Shared Function Affine(minimum As Double, maximum As Double, intercept As Double, slope As Double) As ChartReferenceSeries
        Return New ChartReferenceSeries(minimum,maximum,intercept + slope * minimum,intercept + slope * maximum)
    End Function
    Public Shared Function Horizontal(x0 As Double, x1 As Double, value As Double) As ChartReferenceSeries
        Return New ChartReferenceSeries(x0,x1,value,value)
    End Function
    Public Shared Function Vertical(value As Double, y0 As Double, y1 As Double) As ChartReferenceSeries
        Return New ChartReferenceSeries(value,value,y0,y1)
    End Function
End Class
