Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch33Fixtures

<TestClass>
Public Class Graphics33Tests
    Public Shared Function Field(Of T)(value As Object, name As String) As T
        Dim f = value.GetType().GetField(name, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.IgnoreCase)
        If f IsNot Nothing Then Return DirectCast(f.GetValue(value), T)
        Return DirectCast(value.GetType().GetProperty(name, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic).GetValue(value), T)
    End Function
    Public Shared Sub Invoke(value As Object, name As String, ParamArray args() As Object)
        Try
            value.GetType().GetMethod(name, BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic).Invoke(value, args)
        Catch ex As TargetInvocationException
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw()
        End Try
    End Sub
    Public Shared Function Inputs(index As Integer) As Double()
        Return New Double()() {New Double() {-3, 0, 2, 8}, New Double() {1}, New Double() {0, 0, 0}, New Double() {}, New Double() {-0.0, 0, 1, 1}, New Double() {Double.NaN, 1, Double.PositiveInfinity}, New Double() {-1.0E+150, 1.0E+150}, New Double() {1.0E-150, 2.0E-150}, New Double() {8, 2, 0, -3}, New Double() {2, -3, 8, 0}}(index)
    End Function
    Public Shared Function Context(old As Object, Optional recompute As Boolean = True) As XyzProjectionContext
        Dim a = XyzProjectionContext.RotationRows(Field(Of Double)(old, "x_rotate"), Field(Of Double)(old, "y_rotate"), Field(Of Double)(old, "z_rotate"), Field(Of Double)(old, "x_scale_ratio"), Field(Of Double)(old, "y_scale_ratio"), Field(Of Double)(old, "z_scale_ratio"))
        If recompute Then
            Invoke(old, "get_rotations")
            Graphics31Evidence.EqualBits(Field(Of Double())(old, "rot_1"), a(0))
            Graphics31Evidence.EqualBits(Field(Of Double())(old, "rot_2"), a(1))
        End If
        Return New XyzProjectionContext(Field(Of Double())(old, "raw_mins_"), Field(Of Double())(old, "raw_ranges_"), Field(Of Double())(old, "axis_dir_"), a(0), a(1), Field(Of Double)(old, "x_shift_internal"), Field(Of Double)(old, "y_shift_internal"), Field(Of Double)(old, "zoom_internal"))
    End Function
    Public Shared Sub Compare(old As Object, actual As XyzGeometryData, id As String)
        For Each name In actual.VectorNames
            Dim expected = Field(Of Double())(old, name), obtained = actual.Vector(name)
            Graphics31Evidence.EqualBits(expected, obtained)
            Graphics31Evidence.Record(id & "-" & name, "frozen source fixture", "legacy exact arithmetic", expected.Length.ToString(), expected, obtained)
            If obtained.Length > 0 Then
                obtained(0) = 12345
                Assert.AreEqual(Graphics31Evidence.Bits(expected), Graphics31Evidence.Bits(actual.Vector(name)))
            End If
        Next
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)>
    Public Sub XyzFrozenCoordinates(fixture As Integer)
        For flip As Integer = 0 To 7
            For mode As Integer = 0 To 3
                Dim raw = Inputs(fixture), before = Graphics31Evidence.Bits(raw), old As New LegacyXyz33
                old.dataInputs(raw, raw.Reverse().ToArray(), raw.Select(Function(v) v * 2).ToArray())
                old.ScaleAxis(mode = 1) : old.AxisDirectionInputs((flip And 1) <> 0, (flip And 2) <> 0, (flip And 4) <> 0)
                old.rotationAndZoomInputs(If(mode = 2, -50.0, If(mode = 3, 150.0, 0.0)), If(mode = 3, -100.0, 50.0), If(mode = 3, 200.0, 50.0), If(mode = 0, 120.0, 90.0), If(mode = 0, 60.0, 0.0))
                Dim c = Context(old), id = "XYZ33-" & fixture & "-" & flip & "-" & mode
                old.normalizedData()
                Dim nx = Field(Of Double())(old, "x_norm"), ny = Field(Of Double())(old, "y_norm"), nz = Field(Of Double())(old, "z_norm")
                Dim result = XyzGeometryCore.ProjectNormalized(c, nx, ny, nz, raw.Length)
                Compare(old, result, id & "-data")
                CollectionAssert.AreEqual(Enumerable.Range(0, raw.Length).ToArray(), result.SourceOrdinals)
                Invoke(old, "cut_planes") : Compare(old, XyzGeometryCore.CutPlanes(c, nx, ny, nz, raw.Length, Field(Of Double())(old, "cp_normalized_pos")), id & "-planes")
                Invoke(old, "cage_data") : Compare(old, XyzGeometryCore.Cage(c), id & "-cage")
                Dim tickError = Graphics30Tests.ExceptionType(Sub() Invoke(old, "tick_marks_data"))
                Dim first(2), stepSize(2), count(2) As Double
                Dim newError = Graphics30Tests.ExceptionType(Sub()
                                                               For axis As Integer = 0 To 2
                                                                   Dim t = XyzGeometryCore.TickAxis(c.Minima(axis), c.Ranges(axis), c.Directions(axis))
                                                                   first(axis) = t(3) : stepSize(axis) = t(4) : count(axis) = t(2)
                                                               Next
                                                           End Sub)
                Assert.AreEqual(tickError, newError)
                If tickError Is Nothing Then
                    Compare(old, XyzGeometryCore.Ticks(c, first, stepSize, count), id & "-ticks")
                    Invoke(old, "get_gridlines") : Compare(old, XyzGeometryCore.GridLines(c, first, stepSize, count), id & "-grids")
                End If
                Assert.AreEqual(before, Graphics31Evidence.Bits(raw))
            Next
        Next
    End Sub
    <DataTestMethod><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)>
    Public Sub ScatterFrozenPanels(p As Integer)
        For edge As Integer = 0 To 4
            For mode As Integer = 0 To 3
                Dim data(If(edge = 1, 0, 4), p - 1) As Double
                For i As Integer = 0 To data.GetLength(0) - 1
                    For j As Integer = 0 To p - 1
                        data(i, j) = If(edge = 2, 2.0, If(edge = 3 AndAlso i = 0, Double.NaN, If(edge = 4, 1.0E-15 * i + j, i * i + 2 * j * i - j)))
                    Next
                Next
                Dim before = DirectCast(data.Clone(), Double(,)), old As New LegacyScatter33(data, Enumerable.Repeat("same", p).ToArray(), Nothing, 0.37)
                old.settingInputs((mode And 1) <> 0, (mode And 2) <> 0)
                For repeat As Integer = 0 To 1
                    Dim e = Graphics30Tests.ExceptionType(Sub() old.transformData()), actual As ScatterMatrixPanelData = Nothing
                    Dim a = Graphics30Tests.ExceptionType(Sub()
                                                            Dim normalized = ScatterMatrixDataCore.Normalize(data, 0.1)
                                                            If (mode And 1) <> 0 Then Graphics31Evidence.EqualBits(Field(Of Double(,))(old, "CorrMatrix").Cast(Of Double)().ToArray(), ScatterMatrixSeriesCore.Correlations(data).Cast(Of Double)().ToArray())
                                                            actual = ScatterMatrixSeriesCore.PanelCoordinates(normalized.Values, 0.1, (mode And 2) <> 0)
                                                        End Sub)
                    Assert.AreEqual(e, a)
                    If e Is Nothing Then
                        Graphics31Evidence.EqualBits(Field(Of Double(,))(old, "ScaledData").Cast(Of Double)().ToArray(), actual.Values.Cast(Of Double)().ToArray())
                        For Each pair In {Tuple.Create("x_corr_labels", actual.CorrelationX), Tuple.Create("y_corr_labels", actual.CorrelationY), Tuple.Create("x_regline", actual.RegressionX), Tuple.Create("y_regline", actual.RegressionY)}
                            Dim expected = Field(Of Double())(old, pair.Item1)
                            Graphics31Evidence.EqualBits(expected, pair.Item2)
                            Graphics31Evidence.Record("SCATTER33-" & p & "-" & mode & "-" & edge & "-" & repeat & "-" & pair.Item1, "frozen source fixture", "margin .37 overwritten .1", pair.Item2.Length.ToString(), expected, pair.Item2)
                        Next
                        Assert.AreEqual(p * p, actual.PanelRows.Length)
                        Assert.AreEqual(p * p, actual.PanelColumns.Length)
                        CollectionAssert.AreEqual(Enumerable.Range(0, data.GetLength(0)).ToArray(), actual.SourceOrdinals)
                        For i As Integer = 0 To p - 1
                            Graphics31Evidence.EqualBits(ScatterMatrixSeriesCore.PanelValues(actual.Values, i), actual.PanelX(i))
                        Next
                        Graphics32Tests.MutateArrayGetters(actual)
                    End If
                Next
                Graphics31Evidence.EqualBits(before.Cast(Of Double)().ToArray(), data.Cast(Of Double)().ToArray())
            Next
        Next
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)>
    Public Sub WireFrozenSampling(options As Integer)
        Dim owner As New LegacyXyz33 : owner.dataInputs({-5.0, 5}, {-4.0, 4}, {-3.0, 3})
        owner.AxisDirectionInputs(True, options Mod 2 = 0, True)
        Dim c = Context(owner), lat = If(options = 6, 8, If(options = 1, -2, 2)), lon = If(options = 6, 12, If(options = 2, 0, 3)), pts = If(options = 6, 120, If(options = 3, 1, 16)), diameter = If(options = 4, 0.0, If(options = 5, -2.0, If(options = 7, Double.PositiveInfinity, If(options = 8, Double.NaN, 4.0))))
        Dim sphere As New LegacySphere33(1, -1, 0, diameter) With {.LatitudeRings = lat, .LongitudeRings = lon, .PointsPerRing = pts}
        Dim ellipsoid As New LegacyEllipsoid33(1, -1, 0, diameter, 6, 2) With {.LatitudeRings = lat, .LongitudeRings = lon, .PointsPerRing = pts}
        For Each old As Object In New Object() {sphere, ellipsoid}
            Invoke(old, "Draw", owner, Nothing)
            Dim actual = If(TypeOf old Is LegacySphere33, WireObjectGeometryCore.Sphere(c, 1, -1, 0, diameter, lat, lon, pts), WireObjectGeometryCore.Ellipsoid(c, 1, -1, 0, diameter, 6, 2, lat, lon, pts))
            If diameter <= 0 Then
                Assert.AreEqual(0, actual.X.Length) : Continue For
            End If
            Dim expectedX = Field(Of Double())(old, "Xs"), expectedY = Field(Of Double())(old, "ys")
            Graphics31Evidence.EqualBits(expectedX, actual.X) : Graphics31Evidence.EqualBits(expectedY, actual.Y)
            Graphics31Evidence.Record("WIRE33-" & options & "-" & old.GetType().Name, "frozen sampling source", "current context;floor12 points", expectedX.Length.ToString(), expectedX.Concat(expectedY).ToArray(), actual.X.Concat(actual.Y).ToArray())
            Assert.AreEqual((Math.Max(0, lat) + Math.Max(1, lon)) * (Math.Max(12, pts) + 2), actual.X.Length)
            Assert.AreEqual(Math.Max(0, lat) + Math.Max(1, lon), actual.BreakMask.Count(Function(x) x))
            Graphics32Tests.MutateArrayGetters(actual)
            Graphics31Evidence.EqualBits(expectedX, actual.X)
        Next
    End Sub
    <TestMethod>
    Public Sub IndependentGeometryAndContextSnapshots()
        Dim minimum = {0.0, 0, 0}, ranges = {2.0, 2, 2}, dirs = {1.0, 1, 1}, rows = XyzProjectionContext.RotationRows(0, 0, 0, 1, 1, 1)
        Graphics31Evidence.EqualBits({1.0, -0.0, 0}, rows(0))
        Dim c As New XyzProjectionContext(minimum, ranges, dirs, rows(0), rows(1), 0, 0, 1), x, y As Double
        minimum(0) = 999 : ranges(0) = 999 : rows(0)(0) = 999 : dirs(0) = -1
        c.ProjectRawPoint(2, 0, 1, x, y) : Graphics31Evidence.EqualBits({0.5, -0.5}, {x, y})
        Graphics32Tests.MutateArrayGetters(c) : c.ProjectRawPoint(2, 0, 1, x, y)
        Graphics31Evidence.EqualBits({0.5, -0.5}, {x, y})
        Graphics31Evidence.EqualBits({0.0, 1, 9, -0.5, 1.5}, {XyzProjectionContext.ZoomFactor(-50), XyzProjectionContext.ZoomFactor(0), XyzProjectionContext.ZoomFactor(100), XyzProjectionContext.ShiftX(0), XyzProjectionContext.ShiftY(100)})
        For Each flip In {1.0, -1.0}
            Dim r = XyzProjectionContext.RotationRows(90, 0, 0, 1, 1, 1), d As New XyzProjectionContext({0.0, 0, 0}, {1.0, 1, 1}, {1.0, 1, flip}, r(0), r(1), 0, 0, 1)
            Dim z = XyzDataCore.NormalizeAxis({1.0}, 0, 1, flip, 1), projected = XyzGeometryCore.ProjectNormalized(d, {0.0}, {0.0}, z, 1)
            Dim expected = (z(0) * r(1)(2)) - (-0.5 * r(1)(2))
            Graphics31Evidence.EqualBits({expected}, projected.Vector("error_bars"))
        Next
        Graphics31Evidence.EqualBits({2.0, 0, 2, 0, 2, 0}, WireObjectGeometryCore.SphereBounds(1, 1, 1, -2))
    End Sub
    <TestMethod>
    Public Sub ProjectionSnapshotsRaggedMissingAndPanelMutation()
        Dim rows = XyzProjectionContext.RotationRows(37, -21, 83, 1, 0.75, 0.25)
        Dim c As New XyzProjectionContext({-2.0, -2, -2}, {4.0, 4, 4}, {1.0, -1, 1}, rows(0), rows(1), 0, 1, 1)
        Dim nx = {-0.5, 0.0, 0.5}, ny = DirectCast(nx.Clone(), Double()), nz = DirectCast(nx.Clone(), Double())
        Dim p = XyzGeometryCore.ProjectNormalized(c, nx, ny, nz, 3), before = p.Vector("xs_norm_data")
        nx(0) = 99 : ny(0) = 99 : nz(0) = 99
        Graphics31Evidence.EqualBits(before, p.Vector("xs_norm_data"))
        p.VectorNames(0) = "changed" : p.SourceOrdinals(0) = 99
        CollectionAssert.AreEqual({0, 1, 2}, p.SourceOrdinals)
        Dim raw(,) As Double = {{-2, 5}, {0, 3}, {2, 1}}, snapshot = ScatterMatrixDataCore.Normalize(raw, 0.1)
        Dim panels = ScatterMatrixSeriesCore.PanelCoordinates(snapshot.Values, 0.1, True), x = panels.RegressionX, allValues = panels.Values
        raw(0, 0) = 999 : snapshot.Values(0, 0) = 999
        Graphics32Tests.MutateArrayGetters(panels)
        Graphics31Evidence.EqualBits(x, panels.RegressionX)
        Graphics31Evidence.EqualBits(allValues.Cast(Of Double)().ToArray(), panels.Values.Cast(Of Double)().ToArray())
        For Each input As Double() In New Double()() {Nothing, New Double() {}, New Double() {1}}
            Dim expected = Graphics30Tests.ExceptionType(Sub()
                                                            Dim legacy As New LegacyXyz33
                                                            legacy.dataInputs({1.0, 2}, {1.0, 2}, {1.0, 2})
                                                            legacy.GetType().GetField("y_raw", BindingFlags.Instance Or BindingFlags.NonPublic).SetValue(legacy, input)
                                                            legacy.normalizedData()
                                                        End Sub)
            Dim actual = Graphics30Tests.ExceptionType(Sub()
                                                          XyzDataCore.NormalizeAxis(input, 0, 1, 1, 2)
                                                      End Sub)
            Assert.AreEqual(expected, actual)
        Next
    End Sub
End Class
