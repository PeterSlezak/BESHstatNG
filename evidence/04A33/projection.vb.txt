Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Collections.Generic

''' <summary>Frozen legacy orthographic projection. Raw units, not workbook/pixel coordinates.</summary>
Public NotInheritable Class XyzProjectionContext
    Private ReadOnly minimum(), span(), direction(), rowX(), rowY() As Double
    Public ReadOnly Property XShift As Double
    Public ReadOnly Property YShift As Double
    Public ReadOnly Property Zoom As Double
    Public Sub New(minima() As Double, ranges() As Double, directions() As Double,
                   rotationX() As Double, rotationY() As Double, shiftX As Double, shiftY As Double, zoomFactor As Double)
        minimum = DirectCast(minima.Clone(), Double()) : span = DirectCast(ranges.Clone(), Double())
        direction = DirectCast(directions.Clone(), Double())
        rowX = DirectCast(rotationX.Clone(), Double()) : rowY = DirectCast(rotationY.Clone(), Double())
        XShift = shiftX : YShift = shiftY : Zoom = zoomFactor
    End Sub
    Public ReadOnly Property Minima As Double()
        Get
            Return DirectCast(minimum.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property Ranges As Double()
        Get
            Return DirectCast(span.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property Directions As Double()
        Get
            Return DirectCast(direction.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property RotationX As Double()
        Get
            Return DirectCast(rowX.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property RotationY As Double()
        Get
            Return DirectCast(rowY.Clone(), Double())
        End Get
    End Property
    Public Shared Function RotationRows(xRotate As Double, yRotate As Double, zRotate As Double,
                                        xScale As Double, yScale As Double, zScale As Double) As Double()()
        Dim sin_a() As Double = {Math.Sin(StatFunc.Radians(xRotate)), Math.Sin(StatFunc.Radians(yRotate)), Math.Sin(StatFunc.Radians(zRotate))}
        Dim cos_a() As Double = {Math.Cos(StatFunc.Radians(xRotate)), Math.Cos(StatFunc.Radians(yRotate)), Math.Cos(StatFunc.Radians(zRotate))}
        Dim a(2) As Double, b(2) As Double
        a(0) = xScale * cos_a(1) * cos_a(2)
        a(1) = -yScale * cos_a(1) * sin_a(2)
        a(2) = zScale * sin_a(1)
        b(0) = xScale * (cos_a(2) * sin_a(1) * sin_a(0) + sin_a(2) * cos_a(0))
        b(1) = yScale * (-sin_a(2) * sin_a(1) * sin_a(0) + cos_a(2) * cos_a(0))
        b(2) = -zScale * sin_a(0) * cos_a(1)
        Return {a, b}
    End Function
    Public Shared Function ZoomFactor(value As Double) As Double
        Return (1.0 + value / 50.0) ^ 2
    End Function
    Public Shared Function ShiftX(value As Double) As Double
        Return value / 100.0 - 0.5
    End Function
    Public Shared Function ShiftY(value As Double) As Double
        Return value / 100.0 + 0.5
    End Function
    Public Sub ProjectRawPoint(x As Double, y As Double, z As Double, ByRef projectedX As Double, ByRef projectedY As Double)
        Dim nx = XyzDataCore.NormalizeAxis({x}, minimum(0), span(0), direction(0), 1)(0)
        Dim ny = XyzDataCore.NormalizeAxis({y}, minimum(1), span(1), direction(1), 1)(0)
        Dim nz = XyzDataCore.NormalizeAxis({z}, minimum(2), span(2), direction(2), 1)(0)
        projectedX = (nx * rowX(0) + ny * rowX(1) + nz * rowX(2) + XShift) * Zoom
        projectedY = (nx * rowY(0) + ny * rowY(1) + nz * rowY(2) + YShift) * Zoom
    End Sub
End Class

''' <summary>Named legacy numeric vectors. Names preserve the source operation map.
''' Vectors and names are independent copies. Geometry ordinals are zero based;
''' no worksheet row identity is implied. Sentinel equality retains legacy collisions.</summary>
Public NotInheritable Class XyzGeometryData
    Private ReadOnly vectors As Dictionary(Of String, Double())
    Friend Sub New(values As Dictionary(Of String, Double()))
        vectors = values.ToDictionary(Function(p) p.Key, Function(p) DirectCast(p.Value.Clone(), Double()))
    End Sub
    Public ReadOnly Property VectorNames As String()
        Get
            Return vectors.Keys.ToArray()
        End Get
    End Property
    Public Function Vector(name As String) As Double()
        Return DirectCast(vectors(name).Clone(), Double())
    End Function
    Public Function BreakMask(name As String) As Boolean()
        Return vectors(name).Select(Function(x) x = -100000000000000.0).ToArray()
    End Function
    Public ReadOnly Property SourceOrdinals As Integer()
        Get
            If Not vectors.ContainsKey("xs_norm_data") Then Return Array.Empty(Of Integer)()
            Return Enumerable.Range(0, vectors("xs_norm_data").Length).ToArray()
        End Get
    End Property
End Class

''' <summary>One wire-object series; terminal break after each ring is preserved.</summary>
Public NotInheritable Class WireSeriesData
    Private ReadOnly xs(), ys() As Double
    Private ReadOnly stride As Integer
    Friend Sub New(x() As Double, y() As Double, ringStride As Integer)
        xs = DirectCast(x.Clone(), Double()) : ys = DirectCast(y.Clone(), Double()) : stride = ringStride
    End Sub
    Public ReadOnly Property X As Double()
        Get
            Return DirectCast(xs.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property Y As Double()
        Get
            Return DirectCast(ys.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property BreakMask As Boolean()
        Get
            Return Enumerable.Range(0, xs.Length).Select(Function(i) (i + 1) Mod stride = 0).ToArray()
        End Get
    End Property
    Public ReadOnly Property RingOrdinals As Integer()
        Get
            Return Enumerable.Range(0, xs.Length).Select(Function(i) i \ stride).ToArray()
        End Get
    End Property
    Public ReadOnly Property SampleOrdinals As Integer()
        Get
            Return Enumerable.Range(0, xs.Length).Select(Function(i) If((i + 1) Mod stride = 0, -1, i Mod stride)).ToArray()
        End Get
    End Property
End Class
