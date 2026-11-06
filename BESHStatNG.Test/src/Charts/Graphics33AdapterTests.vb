Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG
Imports Batch33Fixtures
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Graphics33AdapterTests
    Private Shared Sub EqualFields(old As Object, actual As Object, ParamArray names() As String)
        For Each name In names
            Dim a = Graphics33Tests.Field(Of Object)(old, name), b = Graphics33Tests.Field(Of Object)(actual, name)
            If TypeOf a Is Double() Then
                Graphics31Evidence.EqualBits(DirectCast(a, Double()), DirectCast(b, Double()))
            ElseIf TypeOf a Is Double(,) Then
                Graphics31Evidence.EqualBits(DirectCast(a, Double(,)).Cast(Of Double)().ToArray(), DirectCast(b, Double(,)).Cast(Of Double)().ToArray())
            ElseIf TypeOf a Is String() Then
                CollectionAssert.AreEqual(DirectCast(a, String()), DirectCast(b, String()))
            ElseIf TypeOf a Is Integer() Then
                CollectionAssert.AreEqual(DirectCast(a, Integer()), DirectCast(b, Integer()))
            Else
                Assert.AreEqual(a, b, name)
            End If
        Next
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)>
    Public Sub ActualXyzNumericsAndPartialState(fixture As Integer)
        For flip As Integer = 0 To 7
            Dim raw = Graphics33Tests.Inputs(fixture), old As New LegacyXyz33, actual As New graphics.XYZscatter
            For Each obj As Object In New Object() {old, actual}
                Graphics33Tests.Invoke(obj, "dataInputs", raw, raw.Reverse().ToArray(), raw.Select(Function(v) 2 * v).ToArray())
                Graphics33Tests.Invoke(obj, "AxisDirectionInputs", (flip And 1) <> 0, (flip And 2) <> 0, (flip And 4) <> 0)
                Graphics33Tests.Invoke(obj, "ScaleAxis", flip Mod 2 = 0)
            Next
            For repeat As Integer = 0 To 2
                For Each obj As Object In New Object() {old, actual}
                    Graphics33Tests.Invoke(obj, "rotationAndZoomInputs", CDbl(repeat * 25), CDbl(50 - repeat * 40), CDbl(50 + repeat * 30), CDbl(repeat * 90), CDbl(repeat * 45))
                Next
                For Each method In {"get_rotations", "cut_planes", "cage_data", "tick_marks_data", "get_gridlines", "normalizedData"}
                    Dim e = Graphics30Tests.ExceptionType(Sub() Graphics33Tests.Invoke(old, method))
                    Dim a = Graphics30Tests.ExceptionType(Sub() Graphics33Tests.Invoke(actual, method))
                    Assert.AreEqual(e, a, method)
                    EqualFields(old, actual, "raw_mins_", "raw_ranges_", "rot_1", "rot_2", "x_norm", "y_norm", "z_norm", "xs_norm_data", "ys_norm_data", "error_bars", "xs_cage", "ys_cage", "ts", "ft", "tx", "ftn", "tsn", "xs_cutplane_x", "ys_cutplane_x", "xs_cutplane_y", "ys_cutplane_y", "xs_cutplane_z", "ys_cutplane_z", "xs_x_axisticks", "ys_x_axisticks", "xs_y_axisticks", "ys_y_axisticks", "xs_z_axisticks", "ys_z_axisticks", "x_tick_labels", "y_tick_labels", "z_tick_labels", "xs_xy_x_gridline", "ys_xy_x_gridline", "xs_xy_y_gridline", "ys_xy_y_gridline", "xs_xz_x_gridline", "ys_xz_x_gridline", "xs_xz_z_gridline", "ys_xz_z_gridline", "xs_yz_y_gridline", "ys_yz_y_gridline", "xs_yz_z_gridline", "ys_yz_z_gridline")
                    If e IsNot Nothing Then Exit For
                Next
            Next
        Next
    End Sub
    <TestMethod>
    Public Sub ActualXyzObjectBoundsManualLimitsMutationAndGroups()
        Dim old As New LegacyXyz33, actual As New graphics.XYZscatter, raw = {-1.0, 0, 2}, labels = {"z", "a", "z"}
        Dim legacySphere As New LegacySphere33(2, 3, 4, 8), sphere As New graphics.WireSphere3D(2, 3, 4, 8)
        Dim legacyEllipsoid As New LegacyEllipsoid33(-5, -3, 0, 2, 8, 4), ellipsoid As New graphics.WireEllipsoid3D(-5, -3, 0, 2, 8, 4)
        old.AddObject(legacySphere) : old.AddObject(legacyEllipsoid)
        Graphics33Tests.Invoke(actual, "AddObject", sphere) : Graphics33Tests.Invoke(actual, "AddObject", ellipsoid)
        For repeat As Integer = 0 To 2
            legacySphere.Diameter += 1 : sphere.Diameter += 1
            For Each obj As Object In New Object() {old, actual}
                Graphics33Tests.Invoke(obj, "dataInputs", raw, raw, raw)
                Graphics33Tests.Invoke(obj, "ScaleAxis", True)
                Graphics33Tests.Invoke(obj, "SetGroups", DirectCast(labels, Object))
                Graphics33Tests.Invoke(obj, "SetDataLabels", DirectCast(labels, Object))
                Graphics33Tests.Invoke(obj, "SetAxisLimitsX", -10.0, 10.0)
                Graphics33Tests.Invoke(obj, "get_rotations")
            Next
            EqualFields(old, actual, "raw_mins_", "raw_ranges_", "x_scale_ratio", "y_scale_ratio", "z_scale_ratio", "DataLabels_", "Groups", "pgrpIds", "pgrpCounts", "bGroups")
            raw(0) -= 1
            Dim a() As Object = {0.0, 0.0, 0.0, 0.0, 0.0, 0.0}, b = DirectCast(a.Clone(), Object())
            Graphics33Tests.Invoke(sphere, "GetRawBounds", a) : Graphics33Tests.Invoke(legacySphere, "GetRawBounds", b)
            CollectionAssert.AreEqual(a, b)
        Next
        old.ClearObjects() : actual.ClearObjects() : old.ClearAllAxisLimits() : actual.ClearAllAxisLimits()
        For Each obj As Object In New Object() {old, actual}
            Graphics33Tests.Invoke(obj, "RecomputeAxisBoundsIncludingObjects")
        Next
        EqualFields(old, actual, "raw_mins_", "raw_ranges_")
        Assert.AreEqual(Graphics30Tests.ExceptionType(Sub() old.SetAxisLimitsX(2, 1)), Graphics30Tests.ExceptionType(Sub() actual.SetAxisLimitsX(2, 1)))
        For Each x As Double() In New Double()() {Nothing, New Double() {1, 2}}
            Dim e = Graphics30Tests.ExceptionType(Sub()
                                                     old.dataInputs(x, {1.0}, {1.0}) : old.normalizedData()
                                                 End Sub)
            Dim a = Graphics30Tests.ExceptionType(Sub()
                                                     actual.dataInputs(x, {1.0}, {1.0}) : actual.normalizedData()
                                                 End Sub)
            Assert.AreEqual(e, a)
        Next
    End Sub
    <DataTestMethod><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)>
    Public Sub ActualScatterForcedMarginAndFeatureState(p As Integer)
        Dim data(4, p - 1) As Double
        For i As Integer = 0 To 4
            For j As Integer = 0 To p - 1
                data(i, j) = i * i - j * i
            Next
        Next
        Dim names = Enumerable.Repeat("same", p).ToArray(), old As New LegacyScatter33(data, names, Nothing, 0.37)
        Dim actual = Activator.CreateInstance(GetType(graphics.ScatterPlotMatrix), BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, New Object() {data, names, Nothing, 0.37}, Nothing)
        For Each corr In {True, False, True}
            For Each showRegression As Boolean In {True, False}
                For Each obj As Object In New Object() {old, actual}
                    Graphics33Tests.Invoke(obj, "settingInputs", corr, showRegression)
                    Graphics33Tests.Invoke(obj, "transformData")
                Next
                EqualFields(old, actual, "pMargin", "ScaledData", "CorrMatrix", "x_var_labels", "x_corr_labels", "y_corr_labels", "x_regline", "y_regline")
            Next
        Next
        Assert.AreEqual(0.1, Graphics33Tests.Field(Of Double)(actual, "pMargin"))
    End Sub
End Class
