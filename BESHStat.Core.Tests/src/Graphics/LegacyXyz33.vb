Option Strict Off
Option Explicit On



Imports BESHStatNG
Imports System.Linq
Imports System.Collections.Generic

Namespace Global.Batch33Fixtures
    Public Interface IXYZDrawable3D
        Sub Draw(owner As LegacyXyz33, figure As Object)

    End Interface

    Public Interface IXYZHasBounds
        Sub GetRawBounds(ByRef minX As Double, ByRef maxX As Double,
                         ByRef minY As Double, ByRef maxY As Double,
                         ByRef minZ As Double, ByRef maxZ As Double)
    End Interface

    Public Class LegacyXyz33

        Private Const gToDeleteGridLineValue As Double = -100000000000000.0

        Private pXlabel As String

        Private pYlabel As String

        Private pZlabel As String

        Private pbShowXYplanePoints As Boolean
        Private pbShowYZplanePoints As Boolean
        Private pbShowXZplanePoints As Boolean

        Private pXYplanePointSize As Integer
        Private pYZplanePointSize As Integer
        Private pXZplanePointSize As Integer

        Private pbDataLabels As Boolean

        Private pbZdropLines As Boolean

        Private pbShowGridlines As Boolean

        Private pPointLabelFontSize As Integer

        Private pDataLabelPosition As Integer

        Private pDataMarakerSize As Integer

        Private pDataMarkerStyle As Integer

        Private pChartName As String

        Private x_raw() As Double
        Private y_raw() As Double
        Private z_raw() As Double

        Private manualMin_(2) As Nullable(Of Double)

        Private manualMax_(2) As Nullable(Of Double)

        Private raw_mins_(2) As Double

        Private raw_ranges_(2) As Double

        Private n As Integer

        Private DataLabels_() As String

        Private Groups() As String

        Private pgrpIds() As String

        Private pgrpCounts() As Integer

        Private x_norm() As Double
        Private y_norm() As Double
        Private z_norm() As Double
        Private xs_norm_data() As Double
        Private ys_norm_data() As Double
        Private error_bars() As Double

        Private x_scale_ratio As Double
        Private y_scale_ratio As Double
        Private z_scale_ratio As Double
        Private zoom As Double
        Private x_shift As Double
        Private y_shift As Double
        Private zoom_internal As Double
        Private x_shift_internal As Double
        Private y_shift_internal As Double

        Private bScaleAxes As Boolean

        Private bGroups As Boolean

        Private x_rotate As Double
        Private y_rotate As Double
        Private z_rotate As Double

        Private rot_1(2) As Double
        Private rot_2(2) As Double

        Private axis_dir_(2) As Double


        Private xs_cage(10) As Double
        Private ys_cage(10) As Double

        Private ts(2) As Double
        Private ft(2) As Double
        Private tx(2) As Double
        Private ftn(2) As Double
        Private tsn(2) As Double
        Private xs_x_axisticks() As Double
        Private ys_x_axisticks() As Double
        Private xs_y_axisticks() As Double
        Private ys_y_axisticks() As Double
        Private xs_z_axisticks() As Double
        Private ys_z_axisticks() As Double
        Private x_tick_labels() As String
        Private y_tick_labels() As String
        Private z_tick_labels() As String

        Private xs_xy_x_gridline() As Double
        Private ys_xy_x_gridline() As Double
        Private xs_xy_y_gridline() As Double
        Private ys_xy_y_gridline() As Double
        Private xs_xz_x_gridline() As Double
        Private ys_xz_x_gridline() As Double
        Private xs_xz_z_gridline() As Double
        Private ys_xz_z_gridline() As Double
        Private xs_yz_y_gridline() As Double
        Private ys_yz_y_gridline() As Double
        Private xs_yz_z_gridline() As Double
        Private ys_yz_z_gridline() As Double

        Private cp_normalized_pos(2) As Double
        Private xs_cutplane_x() As Double
        Private ys_cutplane_x() As Double
        Private xs_cutplane_y() As Double
        Private ys_cutplane_y() As Double
        Private xs_cutplane_z() As Double
        Private ys_cutplane_z() As Double

        Private ReadOnly pObjects As New List(Of IXYZDrawable3D)

        Public Sub ClearObjects()
            pObjects.Clear()

            RecomputeRawBoundsFromDataOnly()
            ApplyManualAxisBoundsIfAny()
        End Sub

        Public Sub AddObject(obj As IXYZDrawable3D)
            If obj Is Nothing Then Exit Sub
            pObjects.Add(obj)

            RecomputeAxisBoundsIncludingObjects()
        End Sub

        Public Sub SetObjects(objs As IEnumerable(Of IXYZDrawable3D))
            pObjects.Clear()
            If objs IsNot Nothing Then
                pObjects.AddRange(objs)
            End If

            RecomputeAxisBoundsIncludingObjects()
        End Sub


        Public Sub SetAxisLimitsX(minVal As Double, maxVal As Double)
            Dim err As String = ""
            If Not ValidateAxisMinMax("X axis", minVal, maxVal, err) Then
                Throw New ArgumentException(err)
            End If
            manualMin_(0) = minVal
            manualMax_(0) = maxVal
            ApplyManualAxisBoundsIfAny()
        End Sub

        Public Sub ClearAxisLimitsX()
            manualMin_(0) = Nothing
            manualMax_(0) = Nothing
            RecomputeAxisBoundsIncludingObjects()
        End Sub

        Public Sub SetAxisLimitsY(minVal As Double, maxVal As Double)
            Dim err As String = ""
            If Not ValidateAxisMinMax("Y axis", minVal, maxVal, err) Then
                Throw New ArgumentException(err)
            End If
            manualMin_(1) = minVal
            manualMax_(1) = maxVal
            ApplyManualAxisBoundsIfAny()
        End Sub

        Public Sub ClearAxisLimitsY()
            manualMin_(1) = Nothing
            manualMax_(1) = Nothing
            RecomputeAxisBoundsIncludingObjects()
        End Sub

        Public Sub SetAxisLimitsZ(minVal As Double, maxVal As Double)
            Dim err As String = ""
            If Not ValidateAxisMinMax("Z axis", minVal, maxVal, err) Then
                Throw New ArgumentException(err)
            End If
            manualMin_(2) = minVal
            manualMax_(2) = maxVal
            ApplyManualAxisBoundsIfAny()
        End Sub

        Public Sub ClearAxisLimitsZ()
            manualMin_(2) = Nothing
            manualMax_(2) = Nothing
            RecomputeAxisBoundsIncludingObjects()
        End Sub

        Public Sub SetAxisLimits(Optional xmin As Double? = Nothing, Optional xmax As Double? = Nothing,
                                 Optional ymin As Double? = Nothing, Optional ymax As Double? = Nothing,
                                 Optional zmin As Double? = Nothing, Optional zmax As Double? = Nothing)

            If xmin.HasValue Xor xmax.HasValue Then
                Throw New ArgumentException("X axis manual limits require both xmin and xmax.")
            ElseIf xmin.HasValue AndAlso xmax.HasValue Then
                SetAxisLimitsX(xmin.Value, xmax.Value)
            Else
                ClearAxisLimitsX()
            End If

            If ymin.HasValue Xor ymax.HasValue Then
                Throw New ArgumentException("Y axis manual limits require both ymin and ymax.")
            ElseIf ymin.HasValue AndAlso ymax.HasValue Then
                SetAxisLimitsY(ymin.Value, ymax.Value)
            Else
                ClearAxisLimitsY()
            End If

            If zmin.HasValue Xor zmax.HasValue Then
                Throw New ArgumentException("Z axis manual limits require both zmin and zmax.")
            ElseIf zmin.HasValue AndAlso zmax.HasValue Then
                SetAxisLimitsZ(zmin.Value, zmax.Value)
            Else
                ClearAxisLimitsZ()
            End If
        End Sub

        Public Sub ClearAllAxisLimits()
            ClearAxisLimitsX()
            ClearAxisLimitsY()
            ClearAxisLimitsZ()
        End Sub


        Public WriteOnly Property ChartName() As String
            Set(value As String)
                pChartName = value
            End Set
        End Property

        Public Sub ScaleAxis(bScale As Boolean)
            bScaleAxes = bScale

            RefreshScaleRatios()
        End Sub

        Public Sub dataInputs(arXdata() As Double, arYdata() As Double, arZdata() As Double)
            x_raw = arXdata
            y_raw = arYdata
            z_raw = arZdata

            If x_raw Is Nothing OrElse y_raw Is Nothing OrElse z_raw Is Nothing Then Exit Sub
            n = x_raw.Length

            RecomputeAxisBoundsIncludingObjects()
        End Sub

        Public Sub rotationAndZoomInputs(dZoom As Double, dYshift As Double, dXshift As Double, dXrot As Double, dZrot As Double)
            zoom = dZoom
            zoom_internal = comp_zoom_inter(zoom)
            x_shift = dXshift
            x_shift_internal = comp_x_shift_inter(x_shift)
            y_shift = dYshift
            y_shift_internal = comp_y_shift_inter(y_shift)
            x_rotate = dXrot
            z_rotate = dZrot
        End Sub

        Public Sub axesLabelInputs(Xlabel As String, Ylabel As String, Zlabel As String)
            pXlabel = Xlabel
            pYlabel = Ylabel
            pZlabel = Zlabel
        End Sub

        Public Sub showPlanePointInputs(ShowXYplanePoints As Boolean, ShowYZplanePoints As Boolean, ShowXZplanePoints As Boolean,
                             XYplanePointSize As Integer, YZplanePointSize As Integer, XZplanePointSize As Integer)
            pbShowXYplanePoints = ShowXYplanePoints
            pbShowYZplanePoints = ShowYZplanePoints
            pbShowXZplanePoints = ShowXZplanePoints
            pXYplanePointSize = XYplanePointSize
            pYZplanePointSize = YZplanePointSize
            pXZplanePointSize = XZplanePointSize
        End Sub

        Public Sub settingsInputs(Optional bDataLabels As Boolean = False, Optional bZdropLines As Boolean = True, Optional bShowGridlines As Boolean = True,
                       Optional PointLabelFontSize As Integer = 9, Optional DataLabelPosition As Integer = -4152,
                       Optional DataMarakerSize As Integer = 6, Optional DataMarkerStyle As Integer = 8)
            pbDataLabels = bDataLabels
            pbZdropLines = bZdropLines
            pbShowGridlines = bShowGridlines
            pPointLabelFontSize = PointLabelFontSize
            pDataLabelPosition = DataLabelPosition
            pDataMarakerSize = DataMarakerSize
            pDataMarkerStyle = DataMarkerStyle
        End Sub

        Public Sub SetDataLabels(x() As String)
            DataLabels_ = x
        End Sub

        Sub SetGroups(x() As String)
            Groups = x
            pgrpIds = Groups.Distinct().ToArray()
            ReDim pgrpCounts(pgrpIds.Length - 1)
            For i = 0 To pgrpIds.Length - 1
                Dim gg = pgrpIds(i)
                pgrpCounts(i) = Groups.Where(Function(xx) xx = gg).Count()
            Next
            If pgrpIds.Length > 1 Then bGroups = True
        End Sub

        Sub New()
            pXlabel = "x-value"
            pYlabel = "pY-value"
            pZlabel = "z-value"
            pChartName = "XYZ 3D"

            x_rotate = 120.0
            y_rotate = 180.0
            z_rotate = 60.0

            zoom = 0.0
            x_shift = 50.0
            y_shift = 50.0
            zoom_internal = comp_zoom_inter(zoom)
            x_shift_internal = comp_x_shift_inter(x_shift)
            y_shift_internal = comp_y_shift_inter(y_shift)

            bScaleAxes = False 'make absolute values of axes to be proportional
            x_scale_ratio = 1.0
            y_scale_ratio = 1.0
            z_scale_ratio = 1.0

            axis_dir_(0) = 1.0
            axis_dir_(1) = 1.0
            axis_dir_(2) = 1.0

            cp_normalized_pos(0) = -0.5 * axis_dir_(0)
            cp_normalized_pos(1) = -0.5 * axis_dir_(1)
            cp_normalized_pos(2) = -0.5 * axis_dir_(2)

            pbShowXYplanePoints = True
            pbShowYZplanePoints = True
            pbShowXZplanePoints = True
            pXYplanePointSize = 2
            pYZplanePointSize = 2
            pXZplanePointSize = 2

            pDataMarakerSize = 6
            pDataMarkerStyle = 8

            pbDataLabels = False
            pPointLabelFontSize = 9
            pDataLabelPosition = -4152
        End Sub

        Public Sub AxisDirectionInputs(Optional flipX As Boolean = False,
                                       Optional flipY As Boolean = False,
                                       Optional flipZ As Boolean = False)
            axis_dir_(0) = If(flipX, -1.0, 1.0)
            axis_dir_(1) = If(flipY, -1.0, 1.0)
            axis_dir_(2) = If(flipZ, -1.0, 1.0)

            cp_normalized_pos(0) = -0.5 * axis_dir_(0)
            cp_normalized_pos(1) = -0.5 * axis_dir_(1)
            cp_normalized_pos(2) = -0.5 * axis_dir_(2)
        End Sub

        Private Sub get_gridlines()
            Dim x As Double, y As Double, z As Double

            Dim xH As Double = 0.5
            Dim xL As Double = -0.5
            Dim yH As Double = 0.5
            Dim yL As Double = -0.5
            Dim zH As Double = 0.5
            Dim zL As Double = -0.5

            ReDim xs_xy_x_gridline(3 * (tx(0) + 1) - 1), ys_xy_x_gridline(3 * (tx(0) + 1) - 1)
            ReDim xs_xy_y_gridline(3 * (tx(1) + 1) - 1), ys_xy_y_gridline(3 * (tx(1) + 1) - 1)
            ReDim xs_xz_x_gridline(3 * (tx(0) + 1) - 1), ys_xz_x_gridline(3 * (tx(0) + 1) - 1)
            ReDim xs_xz_z_gridline(3 * (tx(2) + 1) - 1), ys_xz_z_gridline(3 * (tx(2) + 1) - 1)
            ReDim xs_yz_y_gridline(3 * (tx(1) + 1) - 1), ys_yz_y_gridline(3 * (tx(1) + 1) - 1)
            ReDim xs_yz_z_gridline(3 * (tx(2) + 1) - 1), ys_yz_z_gridline(3 * (tx(2) + 1) - 1)

            For i = 0 To tx(0)
                x = ftn(0) + i * tsn(0) 'x coordinate of tickmark start of the line
                xs_xy_x_gridline(i * 3) = (x * rot_1(0) + yH * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_x_gridline(i * 3) = (x * rot_2(0) + yH * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_x_gridline(1 + i * 3) = (x * rot_1(0) + yL * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_x_gridline(1 + i * 3) = (x * rot_2(0) + yL * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_x_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xy_x_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i = 0 To tx(1)
                y = ftn(1) + i * tsn(1) 'pY coordinate of tickmark start of the line
                xs_xy_y_gridline(i * 3) = (xH * rot_1(0) + y * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_y_gridline(i * 3) = (xH * rot_2(0) + y * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_y_gridline(1 + i * 3) = (xL * rot_1(0) + y * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_y_gridline(1 + i * 3) = (xL * rot_2(0) + y * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_y_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xy_y_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i = 0 To tx(0)
                x = ftn(0) + i * tsn(0) 'x coordinate of tickmark start of the line
                xs_xz_x_gridline(i * 3) = (x * rot_1(0) + yL * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_x_gridline(i * 3) = (x * rot_2(0) + yL * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_x_gridline(1 + i * 3) = (x * rot_1(0) + yL * rot_1(1) + zH * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_x_gridline(1 + i * 3) = (x * rot_2(0) + yL * rot_2(1) + zH * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_x_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xz_x_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i = 0 To tx(2)
                z = ftn(2) + i * tsn(2) 'z coordinate of tickmark start of the line
                xs_xz_z_gridline(i * 3) = (xL * rot_1(0) + yL * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_z_gridline(i * 3) = (xL * rot_2(0) + yL * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_z_gridline(1 + i * 3) = (xH * rot_1(0) + yL * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_z_gridline(1 + i * 3) = (xH * rot_2(0) + yL * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i = 0 To tx(1)
                y = ftn(1) + i * tsn(1) 'pY coordinate of tickmark start of the line
                xs_yz_y_gridline(i * 3) = (xL * rot_1(0) + y * rot_1(1) + zH * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_y_gridline(i * 3) = (xL * rot_2(0) + y * rot_2(1) + zH * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_y_gridline(1 + i * 3) = (xL * rot_1(0) + y * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_y_gridline(1 + i * 3) = (xL * rot_2(0) + y * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_y_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_yz_y_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i = 0 To tx(2)
                z = ftn(2) + i * tsn(2) 'z coordinate of tickmark start of the line
                xs_yz_z_gridline(i * 3) = (xL * rot_1(0) + yL * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_z_gridline(i * 3) = (xL * rot_2(0) + yL * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_z_gridline(1 + i * 3) = (xL * rot_1(0) + yH * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_z_gridline(1 + i * 3) = (xL * rot_2(0) + yH * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_yz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
        End Sub

        Private Sub cut_planes()
            Call getnorm()
            ReDim xs_cutplane_x(n - 1), ys_cutplane_x(n - 1), xs_cutplane_y(n - 1), ys_cutplane_y(n - 1), xs_cutplane_z(n - 1), ys_cutplane_z(n - 1)

            For i = 0 To n - 1
                xs_cutplane_x(i) = (cp_normalized_pos(0) * rot_1(0) + y_norm(i) * rot_1(1) + z_norm(i) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_cutplane_x(i) = (cp_normalized_pos(0) * rot_2(0) + y_norm(i) * rot_2(1) + z_norm(i) * rot_2(2) + y_shift_internal) * zoom_internal

                xs_cutplane_y(i) = (x_norm(i) * rot_1(0) + cp_normalized_pos(1) * rot_1(1) + z_norm(i) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_cutplane_y(i) = (x_norm(i) * rot_2(0) + cp_normalized_pos(1) * rot_2(1) + z_norm(i) * rot_2(2) + y_shift_internal) * zoom_internal

                xs_cutplane_z(i) = (x_norm(i) * rot_1(0) + y_norm(i) * rot_1(1) + cp_normalized_pos(2) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_cutplane_z(i) = (x_norm(i) * rot_2(0) + y_norm(i) * rot_2(1) + cp_normalized_pos(2) * rot_2(2) + y_shift_internal) * zoom_internal
            Next
        End Sub

        Public Sub normalizedData()
            Call getnorm()
            ReDim xs_norm_data(n - 1), ys_norm_data(n - 1), error_bars(n - 1)
            For i = 0 To n - 1
                xs_norm_data(i) = (x_norm(i) * rot_1(0) + y_norm(i) * rot_1(1) + z_norm(i) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_norm_data(i) = (x_norm(i) * rot_2(0) + y_norm(i) * rot_2(1) + z_norm(i) * rot_2(2) + y_shift_internal) * zoom_internal
                Dim ys_zdrop_base As Double = (x_norm(i) * rot_2(0) + y_norm(i) * rot_2(1) + (-0.5) * rot_2(2) + y_shift_internal) * zoom_internal
                error_bars(i) = ys_norm_data(i) - ys_zdrop_base
            Next
        End Sub

        Private Sub getnorm()
            x_norm = XyzDataCore.NormalizeAxis(x_raw, raw_mins_(0), raw_ranges_(0), axis_dir_(0), n)
            y_norm = XyzDataCore.NormalizeAxis(y_raw, raw_mins_(1), raw_ranges_(1), axis_dir_(1), n)
            z_norm = XyzDataCore.NormalizeAxis(z_raw, raw_mins_(2), raw_ranges_(2), axis_dir_(2), n)
        End Sub

        Private Sub tick_marks_data()
            Call tick_marks_prerequisites()

            ReDim x_tick_labels(CInt(tx(0))), y_tick_labels(CInt(tx(1))), z_tick_labels(CInt(tx(2)))

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

                x_tick_labels(t) = CStr(ft(0) + t * ts(0))
            Next

            For t As Integer = 0 To CInt(tx(1))
                Dim y As Double = ftn(1) + t * tsn(1)

                xs_y_axisticks(t * 3) = (xBase * rot_1(0) + y * rot_1(1) + zBack * rot_1(2) + x_shift_internal) * zoom_internal
                ys_y_axisticks(t * 3) = (xBase * rot_2(0) + y * rot_2(1) + zBack * rot_2(2) + y_shift_internal) * zoom_internal

                xs_y_axisticks(t * 3 + 1) = (xTip * rot_1(0) + y * rot_1(1) + zBack * rot_1(2) + x_shift_internal) * zoom_internal
                ys_y_axisticks(t * 3 + 1) = (xTip * rot_2(0) + y * rot_2(1) + zBack * rot_2(2) + y_shift_internal) * zoom_internal

                xs_y_axisticks(t * 3 + 2) = gToDeleteGridLineValue
                ys_y_axisticks(t * 3 + 2) = gToDeleteGridLineValue

                y_tick_labels(t) = CStr(ft(1) + t * ts(1))
            Next

            For t As Integer = 0 To CInt(tx(2))
                Dim z As Double = ftn(2) + t * tsn(2)

                xs_z_axisticks(t * 3) = (xL * rot_1(0) + yBase * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_z_axisticks(t * 3) = (xL * rot_2(0) + yBase * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_z_axisticks(t * 3 + 1) = (xL * rot_1(0) + yTip * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_z_axisticks(t * 3 + 1) = (xL * rot_2(0) + yTip * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_z_axisticks(t * 3 + 2) = gToDeleteGridLineValue
                ys_z_axisticks(t * 3 + 2) = gToDeleteGridLineValue

                z_tick_labels(t) = CStr(ft(2) + t * ts(2))
            Next
        End Sub

        Private Sub tick_marks_prerequisites()
            Dim fract() As Double = {raw_ranges_(0) / 5.0, raw_ranges_(1) / 5.0, raw_ranges_(2) / 5.0}

            For i = 0 To 2
                Dim nDig As Integer = 1 - Int(Math.Log10(Math.Abs(fract(i))) + 1)
                ts(i) = Math.Round(fract(i), nDig)      'ts tick mark step width
                ft(i) = ts(i) * (Int(raw_mins_(i) / ts(i)) + 1)                 'ft location first tick mark
                tx(i) = Int((raw_ranges_(i) + raw_mins_(i) - ft(i)) / ts(i))    'tx number of tick marks
                ftn(i) = ((ft(i) - raw_mins_(i)) / raw_ranges_(i) - 0.5) * axis_dir_(i)  'first tick (normalized)
                tsn(i) = (ts(i) / raw_ranges_(i)) * axis_dir_(i)                         'tick step (normalized)
            Next
        End Sub

        Private Sub get_rotations()
            Dim sin_a() As Double = {Math.Sin(Radians(x_rotate)), Math.Sin(Radians(y_rotate)), Math.Sin(Radians(z_rotate))}
            Dim cos_a() As Double = {Math.Cos(Radians(x_rotate)), Math.Cos(Radians(y_rotate)), Math.Cos(Radians(z_rotate))}

            rot_1(0) = x_scale_ratio * cos_a(1) * cos_a(2)
            rot_1(1) = -y_scale_ratio * cos_a(1) * sin_a(2)
            rot_1(2) = z_scale_ratio * sin_a(1)
            rot_2(0) = x_scale_ratio * (cos_a(2) * sin_a(1) * sin_a(0) + sin_a(2) * cos_a(0))
            rot_2(1) = y_scale_ratio * (-sin_a(2) * sin_a(1) * sin_a(0) + cos_a(2) * cos_a(0))
            rot_2(2) = -z_scale_ratio * sin_a(0) * cos_a(1)
        End Sub

        Private Sub cage_data()
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
        End Sub

        Private Function comp_zoom_inter(z As Double) As Double
            Return (1.0 + z / 50.0) ^ 2
        End Function

        Private Function comp_x_shift_inter(x As Double) As Double
            Return x / 100.0 - 0.5
        End Function

        Private Function comp_y_shift_inter(y As Double) As Double
            Return y / 100.0 + 0.5
        End Function

        Friend ReadOnly Property BreakValue As Double
            Get
                Return gToDeleteGridLineValue
            End Get
        End Property

        Friend Sub ProjectRawPoint(xRaw As Double, yRaw As Double, zRaw As Double,
                                   ByRef x2d As Double, ByRef y2d As Double)

            Dim nx As Double = ((xRaw - raw_mins_(0)) / raw_ranges_(0) - 0.5) * axis_dir_(0)
            Dim ny As Double = ((yRaw - raw_mins_(1)) / raw_ranges_(1) - 0.5) * axis_dir_(1)
            Dim nz As Double = ((zRaw - raw_mins_(2)) / raw_ranges_(2) - 0.5) * axis_dir_(2)

            x2d = (nx * rot_1(0) + ny * rot_1(1) + nz * rot_1(2) + x_shift_internal) * zoom_internal
            y2d = (nx * rot_2(0) + ny * rot_2(1) + nz * rot_2(2) + y_shift_internal) * zoom_internal
        End Sub


        Private Sub RecomputeRawBoundsFromDataOnly()
            If x_raw Is Nothing OrElse y_raw Is Nothing OrElse z_raw Is Nothing Then Exit Sub
            If x_raw.Length = 0 Then Exit Sub

            raw_mins_(0) = x_raw.Min()
            raw_ranges_(0) = x_raw.Max() - raw_mins_(0)

            raw_mins_(1) = y_raw.Min()
            raw_ranges_(1) = y_raw.Max() - raw_mins_(1)

            raw_mins_(2) = z_raw.Min()
            raw_ranges_(2) = z_raw.Max() - raw_mins_(2)
            EnsureNonZeroRanges()
        End Sub

        Private Sub ExpandRawBoundsToIncludeObjects()
            If x_raw Is Nothing OrElse y_raw Is Nothing OrElse z_raw Is Nothing Then Exit Sub
            If x_raw.Length = 0 Then Exit Sub

            Dim minX As Double = raw_mins_(0)
            Dim minY As Double = raw_mins_(1)
            Dim minZ As Double = raw_mins_(2)

            Dim maxX As Double = raw_mins_(0) + raw_ranges_(0)
            Dim maxY As Double = raw_mins_(1) + raw_ranges_(1)
            Dim maxZ As Double = raw_mins_(2) + raw_ranges_(2)

            For Each obj In pObjects
                If obj Is Nothing Then Continue For

                Dim hb As IXYZHasBounds = TryCast(obj, IXYZHasBounds)
                If hb Is Nothing Then Continue For

                Dim ominX As Double, omaxX As Double
                Dim ominY As Double, omaxY As Double
                Dim ominZ As Double, omaxZ As Double

                hb.GetRawBounds(ominX, omaxX, ominY, omaxY, ominZ, omaxZ)

                minX = Math.Min(minX, ominX)
                maxX = Math.Max(maxX, omaxX)

                minY = Math.Min(minY, ominY)
                maxY = Math.Max(maxY, omaxY)

                minZ = Math.Min(minZ, ominZ)
                maxZ = Math.Max(maxZ, omaxZ)
            Next

            raw_mins_(0) = minX
            raw_ranges_(0) = maxX - minX

            raw_mins_(1) = minY
            raw_ranges_(1) = maxY - minY

            raw_mins_(2) = minZ
            raw_ranges_(2) = maxZ - minZ

            EnsureNonZeroRanges()
        End Sub

        Private Sub RecomputeAxisBoundsIncludingObjects()
            RecomputeRawBoundsFromDataOnly()
            ExpandRawBoundsToIncludeObjects()
            ApplyManualAxisBoundsIfAny()
        End Sub

        Private Sub ApplyManualAxisBoundsIfAny()
            For axis As Integer = 0 To 2
                If manualMin_(axis).HasValue OrElse manualMax_(axis).HasValue Then
                    If Not manualMin_(axis).HasValue OrElse Not manualMax_(axis).HasValue Then
                        Throw New ArgumentException($"Axis {axis} manual limits require both min and max.")
                    End If

                    Dim mn = manualMin_(axis).Value
                    Dim mx = manualMax_(axis).Value

                    Dim err As String = ""
                    Dim axisName As String = If(axis = 0, "X axis", If(axis = 1, "Y axis", "Z axis"))
                    If Not ValidateAxisMinMax(axisName, mn, mx, err) Then
                        Throw New ArgumentException(err)
                    End If

                    raw_mins_(axis) = mn
                    raw_ranges_(axis) = mx - mn
                End If
            Next

            RefreshScaleRatios()
        End Sub

        Private Sub RefreshScaleRatios()
            Dim maxRange As Double = raw_ranges_.Max()
            If maxRange <= 0 Then maxRange = 1.0

            If bScaleAxes Then
                x_scale_ratio = raw_ranges_(0) / maxRange
                y_scale_ratio = raw_ranges_(1) / maxRange
                z_scale_ratio = raw_ranges_(2) / maxRange
            Else
                x_scale_ratio = 1.0
                y_scale_ratio = 1.0
                z_scale_ratio = 1.0
            End If
        End Sub

        Private Sub EnsureNonZeroRanges()
            Const eps As Double = 0.000000000001
            For i As Integer = 0 To 2
                If raw_ranges_(i) <= eps Then raw_ranges_(i) = 1.0 'fallback to avoid div/0
            Next
        End Sub

        Friend Shared Function ValidateAxisMinMax(axisName As String,
                                                  minVal As Double,
                                                  maxVal As Double,
                                                  ByRef errText As String,
                                                  Optional allowEqual As Boolean = False) As Boolean
            errText = ""

            If Double.IsNaN(minVal) OrElse Double.IsInfinity(minVal) Then
                errText = $"{axisName} min must be a finite number."
                Return False
            End If

            If Double.IsNaN(maxVal) OrElse Double.IsInfinity(maxVal) Then
                errText = $"{axisName} max must be a finite number."
                Return False
            End If

            If allowEqual Then
                If minVal > maxVal Then
                    errText = $"{axisName} min must be <= max."
                    Return False
                End If
            Else
                If minVal >= maxVal Then
                    errText = $"{axisName} min must be < max."
                    Return False
                End If
            End If

            Return True
        End Function

    End Class
End Namespace
