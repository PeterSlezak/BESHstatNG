Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq


Public Module XyzGeometryCore
    ' Matches legacy tick arithmetic, including Math.Round digit exceptions.
    Public Function TickAxis(minimum As Double, range As Double, direction As Double) As Double()
        Dim fract As Double = range / 5.0
        Dim nDig As Integer = CInt(1 - Math.Floor(Math.Log10(Math.Abs(fract)) + 1))
        Dim ts As Double = Math.Round(fract, nDig)
        Dim ft As Double = ts * (Math.Floor(minimum / ts) + 1)
        Dim tx As Double = Math.Floor((range + minimum - ft) / ts)
        Return {ts, ft, tx, ((ft - minimum) / range - 0.5) * direction, (ts / range) * direction}
    End Function
    Public Function ProjectNormalized(context As XyzProjectionContext, x() As Double, y() As Double, z() As Double, count As Integer) As XyzGeometryData
        Dim work As New XyzGeometryBuilder(context)
        work.x_norm = DirectCast(x.Clone(), Double()) : work.y_norm = DirectCast(y.Clone(), Double()) : work.z_norm = DirectCast(z.Clone(), Double()) : work.n = count
        Return work.ProjectNormalized()
    End Function
    Public Function CutPlanes(context As XyzProjectionContext, x() As Double, y() As Double, z() As Double, count As Integer, planes() As Double) As XyzGeometryData
        Dim work As New XyzGeometryBuilder(context)
        work.x_norm = DirectCast(x.Clone(), Double()) : work.y_norm = DirectCast(y.Clone(), Double()) : work.z_norm = DirectCast(z.Clone(), Double()) : work.n = count
        work.cp_normalized_pos = DirectCast(planes.Clone(), Double())
        Return work.CutPlanes()
    End Function
    Public Function Cage(context As XyzProjectionContext) As XyzGeometryData
        Dim work As New XyzGeometryBuilder(context)
        Return work.Cage()
    End Function
    Public Function Ticks(context As XyzProjectionContext, first() As Double, stepSize() As Double, counts() As Double) As XyzGeometryData
        Dim work As New XyzGeometryBuilder(context)
        work.ftn = DirectCast(first.Clone(), Double()) : work.tsn = DirectCast(stepSize.Clone(), Double()) : work.tx = DirectCast(counts.Clone(), Double())
        Return work.Ticks()
    End Function
    Public Function GridLines(context As XyzProjectionContext, first() As Double, stepSize() As Double, counts() As Double) As XyzGeometryData
        Dim work As New XyzGeometryBuilder(context)
        work.ftn = DirectCast(first.Clone(), Double()) : work.tsn = DirectCast(stepSize.Clone(), Double()) : work.tx = DirectCast(counts.Clone(), Double())
        Return work.GridLines()
    End Function
End Module

Friend NotInheritable Class XyzGeometryBuilder
    Private Const gToDeleteGridLineValue As Double = -100000000000000.0
    Friend n As Integer
    Private x_shift_internal As Double, y_shift_internal As Double, zoom_internal As Double
    Friend rot_1() As Double
    Friend rot_2() As Double
    Friend ftn() As Double
    Friend tsn() As Double
    Friend tx() As Double
    Friend x_norm() As Double
    Friend y_norm() As Double
    Friend z_norm() As Double
    Friend cp_normalized_pos() As Double
    Friend xs_norm_data() As Double
    Friend ys_norm_data() As Double
    Friend error_bars() As Double
    Friend xs_cutplane_x() As Double
    Friend ys_cutplane_x() As Double
    Friend xs_cutplane_y() As Double
    Friend ys_cutplane_y() As Double
    Friend xs_cutplane_z() As Double
    Friend ys_cutplane_z() As Double
    Friend xs_cage() As Double
    Friend ys_cage() As Double
    Friend xs_x_axisticks() As Double
    Friend ys_x_axisticks() As Double
    Friend xs_y_axisticks() As Double
    Friend ys_y_axisticks() As Double
    Friend xs_z_axisticks() As Double
    Friend ys_z_axisticks() As Double
    Friend xs_xy_x_gridline() As Double
    Friend ys_xy_x_gridline() As Double
    Friend xs_xy_y_gridline() As Double
    Friend ys_xy_y_gridline() As Double
    Friend xs_xz_x_gridline() As Double
    Friend ys_xz_x_gridline() As Double
    Friend xs_xz_z_gridline() As Double
    Friend ys_xz_z_gridline() As Double
    Friend xs_yz_y_gridline() As Double
    Friend ys_yz_y_gridline() As Double
    Friend xs_yz_z_gridline() As Double
    Friend ys_yz_z_gridline() As Double
    Friend Sub New(context As XyzProjectionContext)
        rot_1 = context.RotationX : rot_2 = context.RotationY
        x_shift_internal = context.XShift : y_shift_internal = context.YShift : zoom_internal = context.Zoom
        xs_cage = New Double(10) {} : ys_cage = New Double(10) {}
    End Sub
    Friend Function ProjectNormalized() As XyzGeometryData

            ReDim xs_norm_data(n - 1), ys_norm_data(n - 1), error_bars(n - 1)
            For i = 0 To n - 1
                xs_norm_data(i) = (x_norm(i) * rot_1(0) + y_norm(i) * rot_1(1) + z_norm(i) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_norm_data(i) = (x_norm(i) * rot_2(0) + y_norm(i) * rot_2(1) + z_norm(i) * rot_2(2) + y_shift_internal) * zoom_internal
                Dim ys_zdrop_base As Double = (x_norm(i) * rot_2(0) + y_norm(i) * rot_2(1) + (-0.5) * rot_2(2) + y_shift_internal) * zoom_internal
                error_bars(i) = ys_norm_data(i) - ys_zdrop_base
            Next
        Return New XyzGeometryData(New Dictionary(Of String, Double()) From {{"xs_norm_data", xs_norm_data}, {"ys_norm_data", ys_norm_data}, {"error_bars", error_bars}})
    End Function
    Friend Function CutPlanes() As XyzGeometryData

            ReDim xs_cutplane_x(n - 1), ys_cutplane_x(n - 1), xs_cutplane_y(n - 1), ys_cutplane_y(n - 1), xs_cutplane_z(n - 1), ys_cutplane_z(n - 1)

            For i = 0 To n - 1
                xs_cutplane_x(i) = (cp_normalized_pos(0) * rot_1(0) + y_norm(i) * rot_1(1) + z_norm(i) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_cutplane_x(i) = (cp_normalized_pos(0) * rot_2(0) + y_norm(i) * rot_2(1) + z_norm(i) * rot_2(2) + y_shift_internal) * zoom_internal

                xs_cutplane_y(i) = (x_norm(i) * rot_1(0) + cp_normalized_pos(1) * rot_1(1) + z_norm(i) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_cutplane_y(i) = (x_norm(i) * rot_2(0) + cp_normalized_pos(1) * rot_2(1) + z_norm(i) * rot_2(2) + y_shift_internal) * zoom_internal

                xs_cutplane_z(i) = (x_norm(i) * rot_1(0) + y_norm(i) * rot_1(1) + cp_normalized_pos(2) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_cutplane_z(i) = (x_norm(i) * rot_2(0) + y_norm(i) * rot_2(1) + cp_normalized_pos(2) * rot_2(2) + y_shift_internal) * zoom_internal
            Next
        Return New XyzGeometryData(New Dictionary(Of String, Double()) From {{"xs_cutplane_x", xs_cutplane_x}, {"ys_cutplane_x", ys_cutplane_x}, {"xs_cutplane_y", xs_cutplane_y}, {"ys_cutplane_y", ys_cutplane_y}, {"xs_cutplane_z", xs_cutplane_z}, {"ys_cutplane_z", ys_cutplane_z}})
    End Function
    Friend Function Cage() As XyzGeometryData
            Dim unscaled_x(10) As Double, unscaled_y(10) As Double, unscaled_z(10) As Double

            Dim xH As Double = 0.5
            Dim xL As Double = -0.5
            Dim yH As Double = 0.5
            Dim yL As Double = -0.5
            Dim zH As Double = 0.5
            Dim zL As Double = -0.5

            unscaled_x = {xH, xL, xL, xH, xH, xH, xL, xL, xL, xL, xL}
            unscaled_y = {yL, yL, yL, yL, yL, yH, yH, yH, yL, yL, yH}
            unscaled_z = {zL, zL, zH, zH, zL, zL, zL, zH, zH, zL, zL}

            For i = 0 To 10
                xs_cage(i) = (unscaled_x(i) * rot_1(0) + unscaled_y(i) * rot_1(1) + unscaled_z(i) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_cage(i) = (unscaled_x(i) * rot_2(0) + unscaled_y(i) * rot_2(1) + unscaled_z(i) * rot_2(2) + y_shift_internal) * zoom_internal
            Next
        Return New XyzGeometryData(New Dictionary(Of String, Double()) From {{"xs_cage", xs_cage}, {"ys_cage", ys_cage}})
    End Function
    Friend Function Ticks() As XyzGeometryData


            ReDim xs_x_axisticks(3 * (CInt(tx(0)) + 1) - 1), ys_x_axisticks(3 * (CInt(tx(0)) + 1) - 1)
            ReDim xs_y_axisticks(3 * (CInt(tx(1)) + 1) - 1), ys_y_axisticks(3 * (CInt(tx(1)) + 1) - 1)
            ReDim xs_z_axisticks(3 * (CInt(tx(2)) + 1) - 1), ys_z_axisticks(3 * (CInt(tx(2)) + 1) - 1)

            Dim xBase As Double = 0.5
            Dim xTip As Double = 0.55
            Dim xL As Double = -0.5
            Dim yBase As Double = 0.5
            Dim yTip As Double = 0.55
            Dim zBack As Double = -0.5

            For t As Integer = 0 To CInt(tx(0))
                Dim x As Double = ftn(0) + t * tsn(0)

                xs_x_axisticks(t * 3) = (x * rot_1(0) + yBase * rot_1(1) + zBack * rot_1(2) + x_shift_internal) * zoom_internal
                ys_x_axisticks(t * 3) = (x * rot_2(0) + yBase * rot_2(1) + zBack * rot_2(2) + y_shift_internal) * zoom_internal

                xs_x_axisticks(t * 3 + 1) = (x * rot_1(0) + yTip * rot_1(1) + zBack * rot_1(2) + x_shift_internal) * zoom_internal
                ys_x_axisticks(t * 3 + 1) = (x * rot_2(0) + yTip * rot_2(1) + zBack * rot_2(2) + y_shift_internal) * zoom_internal

                xs_x_axisticks(t * 3 + 2) = gToDeleteGridLineValue
                ys_x_axisticks(t * 3 + 2) = gToDeleteGridLineValue

            Next

            For t As Integer = 0 To CInt(tx(1))
                Dim y As Double = ftn(1) + t * tsn(1)

                xs_y_axisticks(t * 3) = (xBase * rot_1(0) + y * rot_1(1) + zBack * rot_1(2) + x_shift_internal) * zoom_internal
                ys_y_axisticks(t * 3) = (xBase * rot_2(0) + y * rot_2(1) + zBack * rot_2(2) + y_shift_internal) * zoom_internal

                xs_y_axisticks(t * 3 + 1) = (xTip * rot_1(0) + y * rot_1(1) + zBack * rot_1(2) + x_shift_internal) * zoom_internal
                ys_y_axisticks(t * 3 + 1) = (xTip * rot_2(0) + y * rot_2(1) + zBack * rot_2(2) + y_shift_internal) * zoom_internal

                xs_y_axisticks(t * 3 + 2) = gToDeleteGridLineValue
                ys_y_axisticks(t * 3 + 2) = gToDeleteGridLineValue

            Next

            For t As Integer = 0 To CInt(tx(2))
                Dim z As Double = ftn(2) + t * tsn(2)

                xs_z_axisticks(t * 3) = (xL * rot_1(0) + yBase * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_z_axisticks(t * 3) = (xL * rot_2(0) + yBase * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_z_axisticks(t * 3 + 1) = (xL * rot_1(0) + yTip * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_z_axisticks(t * 3 + 1) = (xL * rot_2(0) + yTip * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_z_axisticks(t * 3 + 2) = gToDeleteGridLineValue
                ys_z_axisticks(t * 3 + 2) = gToDeleteGridLineValue

            Next
        Return New XyzGeometryData(New Dictionary(Of String, Double()) From {{"xs_x_axisticks", xs_x_axisticks}, {"ys_x_axisticks", ys_x_axisticks}, {"xs_y_axisticks", xs_y_axisticks}, {"ys_y_axisticks", ys_y_axisticks}, {"xs_z_axisticks", xs_z_axisticks}, {"ys_z_axisticks", ys_z_axisticks}})
    End Function
    Friend Function GridLines() As XyzGeometryData
            Dim x As Double, y As Double, z As Double

            Dim xH As Double = 0.5
            Dim xL As Double = -0.5
            Dim yH As Double = 0.5
            Dim yL As Double = -0.5
            Dim zH As Double = 0.5
            Dim zL As Double = -0.5

            ReDim xs_xy_x_gridline(CInt(3 * (tx(0) + 1) - 1)), ys_xy_x_gridline(CInt(3 * (tx(0) + 1) - 1))
            ReDim xs_xy_y_gridline(CInt(3 * (tx(1) + 1) - 1)), ys_xy_y_gridline(CInt(3 * (tx(1) + 1) - 1))
            ReDim xs_xz_x_gridline(CInt(3 * (tx(0) + 1) - 1)), ys_xz_x_gridline(CInt(3 * (tx(0) + 1) - 1))
            ReDim xs_xz_z_gridline(CInt(3 * (tx(2) + 1) - 1)), ys_xz_z_gridline(CInt(3 * (tx(2) + 1) - 1))
            ReDim xs_yz_y_gridline(CInt(3 * (tx(1) + 1) - 1)), ys_yz_y_gridline(CInt(3 * (tx(1) + 1) - 1))
            ReDim xs_yz_z_gridline(CInt(3 * (tx(2) + 1) - 1)), ys_yz_z_gridline(CInt(3 * (tx(2) + 1) - 1))

            For i As Integer = 0 To CInt(tx(0))
                x = ftn(0) + i * tsn(0) 'x coordinate of tickmark start of the line
                xs_xy_x_gridline(i * 3) = (x * rot_1(0) + yH * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_x_gridline(i * 3) = (x * rot_2(0) + yH * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_x_gridline(1 + i * 3) = (x * rot_1(0) + yL * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_x_gridline(1 + i * 3) = (x * rot_2(0) + yL * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_x_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xy_x_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i As Integer = 0 To CInt(tx(1))
                y = ftn(1) + i * tsn(1) 'pY coordinate of tickmark start of the line
                xs_xy_y_gridline(i * 3) = (xH * rot_1(0) + y * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_y_gridline(i * 3) = (xH * rot_2(0) + y * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_y_gridline(1 + i * 3) = (xL * rot_1(0) + y * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_y_gridline(1 + i * 3) = (xL * rot_2(0) + y * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_y_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xy_y_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i As Integer = 0 To CInt(tx(0))
                x = ftn(0) + i * tsn(0) 'x coordinate of tickmark start of the line
                xs_xz_x_gridline(i * 3) = (x * rot_1(0) + yL * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_x_gridline(i * 3) = (x * rot_2(0) + yL * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_x_gridline(1 + i * 3) = (x * rot_1(0) + yL * rot_1(1) + zH * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_x_gridline(1 + i * 3) = (x * rot_2(0) + yL * rot_2(1) + zH * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_x_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xz_x_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i As Integer = 0 To CInt(tx(2))
                z = ftn(2) + i * tsn(2) 'z coordinate of tickmark start of the line
                xs_xz_z_gridline(i * 3) = (xL * rot_1(0) + yL * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_z_gridline(i * 3) = (xL * rot_2(0) + yL * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_z_gridline(1 + i * 3) = (xH * rot_1(0) + yL * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_z_gridline(1 + i * 3) = (xH * rot_2(0) + yL * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i As Integer = 0 To CInt(tx(1))
                y = ftn(1) + i * tsn(1) 'pY coordinate of tickmark start of the line
                xs_yz_y_gridline(i * 3) = (xL * rot_1(0) + y * rot_1(1) + zH * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_y_gridline(i * 3) = (xL * rot_2(0) + y * rot_2(1) + zH * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_y_gridline(1 + i * 3) = (xL * rot_1(0) + y * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_y_gridline(1 + i * 3) = (xL * rot_2(0) + y * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_y_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_yz_y_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i As Integer = 0 To CInt(tx(2))
                z = ftn(2) + i * tsn(2) 'z coordinate of tickmark start of the line
                xs_yz_z_gridline(i * 3) = (xL * rot_1(0) + yL * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_z_gridline(i * 3) = (xL * rot_2(0) + yL * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_z_gridline(1 + i * 3) = (xL * rot_1(0) + yH * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_z_gridline(1 + i * 3) = (xL * rot_2(0) + yH * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_yz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
        Return New XyzGeometryData(New Dictionary(Of String, Double()) From {{"xs_xy_x_gridline", xs_xy_x_gridline}, {"ys_xy_x_gridline", ys_xy_x_gridline}, {"xs_xy_y_gridline", xs_xy_y_gridline}, {"ys_xy_y_gridline", ys_xy_y_gridline}, {"xs_xz_x_gridline", xs_xz_x_gridline}, {"ys_xz_x_gridline", ys_xz_x_gridline}, {"xs_xz_z_gridline", xs_xz_z_gridline}, {"ys_xz_z_gridline", ys_xz_z_gridline}, {"xs_yz_y_gridline", xs_yz_y_gridline}, {"ys_yz_y_gridline", ys_yz_y_gridline}, {"xs_yz_z_gridline", xs_yz_z_gridline}, {"ys_yz_z_gridline", ys_yz_z_gridline}})
    End Function
End Class
