Option Explicit On
Imports System.Xml
Imports BESHStatNG.AppInfrastructure
Imports ExcelDna.Integration
Imports Microsoft.Office.Interop.Excel

Namespace graphics
    Public Interface IXYZDrawable3D
        Sub Draw(owner As XYZscatter, figure As Microsoft.Office.Interop.Excel.Chart)

    End Interface

    Public Interface IXYZHasBounds
        ''' <summary>
        ''' Returns the raw (data-space) axis-aligned bounds of the object.
        ''' </summary>
        Sub GetRawBounds(ByRef minX As Double, ByRef maxX As Double,
                         ByRef minY As Double, ByRef maxY As Double,
                         ByRef minZ As Double, ByRef maxZ As Double)
    End Interface

    ''' <summary>
    ''' Implements a full 3D scatter‑plot engine rendered in 2D Excel charts using
    ''' perspective projection, axis scaling, rotation matrices, cut‑planes,
    ''' gridlines, tick‑mark geometry, and optional grouping.
    ''' 
    ''' The class supports:
    ''' <list type="bullet">
    '''   <item><description>3D → 2D projection using user‑defined rotation angles</description></item>
    '''   <item><description>Independent axis scaling or proportional scaling</description></item>
    '''   <item><description>Zooming and XY shifting</description></item>
    '''   <item><description>Axis labels, tick marks, and tick‑label placement</description></item>
    '''   <item><description>XY, YZ, XZ plane projections with adjustable point sizes</description></item>
    '''   <item><description>Optional Z‑drop lines for depth perception</description></item>
    '''   <item><description>Optional gridlines on all three coordinate planes</description></item>
    '''   <item><description>Optional data labels and grouping</description></item>
    '''   <item><description>Cut‑plane visualization at arbitrary normalized positions</description></item>
    ''' </list>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>ChartScaling</c> — axis scaling helper</description></item>
    '''   <item><description><c>Median</c>, <c>RoundDown</c>, <c>RoundUp</c></description></item>
    '''   <item><description><c>GetColumnFrom2Darray</c></description></item>
    '''   <item><description>Excel interop (<c>Chart</c>, <c>SeriesCollection</c>)</description></item>
    ''' </list>
    ''' </summary>
    Public Class XYZscatter

        Private Const gToDeleteGridLineValue As Double = -100000000000000.0

        ''' <summary>X‑axis label for the rendered chart.</summary>
        Private pXlabel As String

        ''' <summary>Y‑axis label for the rendered chart.</summary>
        Private pYlabel As String

        ''' <summary>Z‑axis label for the rendered chart.</summary>
        Private pZlabel As String

        ''' <summary>Flags controlling whether XY, YZ, XZ plane projections are shown.</summary>
        Private pbShowXYplanePoints As Boolean
        Private pbShowYZplanePoints As Boolean
        Private pbShowXZplanePoints As Boolean

        ''' <summary>Point sizes for XY, YZ, XZ plane projections.</summary>
        Private pXYplanePointSize As Integer
        Private pYZplanePointSize As Integer
        Private pXZplanePointSize As Integer

        ''' <summary>Controls whether data labels are shown.</summary>
        Private pbDataLabels As Boolean

        ''' <summary>Controls whether Z‑drop lines are drawn from each point.</summary>
        Private pbZdropLines As Boolean

        ''' <summary>Controls whether gridlines are drawn on XY, YZ, XZ planes.</summary>
        Private pbShowGridlines As Boolean

        ''' <summary>Font size for data‑point labels.</summary>
        Private pPointLabelFontSize As Integer

        ''' <summary>Position of data labels (Excel constant).</summary>
        Private pDataLabelPosition As Integer

        ''' <summary>Marker size for 3D data points.</summary>
        Private pDataMarakerSize As Integer

        ''' <summary>Marker style for 3D data points.</summary>
        Private pDataMarkerStyle As XlMarkerStyle

        ''' <summary>Name of the chart object created in Excel.</summary>
        Private pChartName As String

        ' Raw data
        Private x_raw() As Double
        Private y_raw() As Double
        Private z_raw() As Double

        ''' <summary>Manual axis minimum bounds (Nothing means auto)</summary>
        Private manualMin_(2) As Nullable(Of Double)

        ''' <summary>Manual axis maximum bounds (Nothing means auto)</summary>
        Private manualMax_(2) As Nullable(Of Double)

        ''' <summary>Minimum values of raw X, Y, Z.</summary>
        Private raw_mins_(2) As Double

        ''' <summary>Ranges of raw X, Y, Z.</summary>
        Private raw_ranges_(2) As Double

        ''' <summary>Number of observations.</summary>
        Private n As Integer

        ''' <summary>Optional data‑point labels.</summary>
        Private DataLabels_() As String

        ''' <summary>Optional group identifiers for each point.</summary>
        Private Groups() As String

        ''' <summary>Unique group IDs.</summary>
        Private pgrpIds() As String

        ''' <summary>Counts of points in each group.</summary>
        Private pgrpCounts() As Integer

        ' Normalized 3D → 2D projected coordinates
        Private x_norm() As Double
        Private y_norm() As Double
        Private z_norm() As Double
        Private xs_norm_data() As Double
        Private ys_norm_data() As Double
        Private error_bars() As Double

        ' Scaling factors
        Private x_scale_ratio As Double
        Private y_scale_ratio As Double
        Private z_scale_ratio As Double
        Private zoom As Double
        Private x_shift As Double
        Private y_shift As Double
        Private zoom_internal As Double
        Private x_shift_internal As Double
        Private y_shift_internal As Double

        ''' <summary>If True, axes are scaled proportionally to their ranges.</summary>
        Private bScaleAxes As Boolean

        ''' <summary>True if more than one group is present.</summary>
        Private bGroups As Boolean

        ' Rotation angles
        Private x_rotate As Double
        Private y_rotate As Double
        Private z_rotate As Double

        ' Rotation vectors
        Private rot_1(2) As Double
        Private rot_2(2) As Double

        ' Axis direction (display) control.
        ' Each component is either +1 (normal) or -1 (reversed).
        ' Reversal is applied in normalized space ([-0.5, +0.5]) by multiplying
        ' the corresponding normalized coordinate by this factor.
        Private axis_dir_(2) As Double


        ' Cage (3D bounding box) projected coordinates
        Private xs_cage(10) As Double
        Private ys_cage(10) As Double

        ' Tick‑mark geometry
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

        ' Gridline geometry
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

        ' Cut-plane geometry
        Private cp_normalized_pos(2) As Double
        Private xs_cutplane_x() As Double
        Private ys_cutplane_x() As Double
        Private xs_cutplane_y() As Double
        Private ys_cutplane_y() As Double
        Private xs_cutplane_z() As Double
        Private ys_cutplane_z() As Double

        '3D drawable objects (wire sphere, ellipsoid, prism, etc.)
        Private ReadOnly pObjects As New List(Of IXYZDrawable3D)

        Public Sub ClearObjects()
            pObjects.Clear()

            'Revert to data-only bounds, then reapply any manual bounds
            RecomputeRawBoundsFromDataOnly()
            ApplyManualAxisBoundsIfAny()
        End Sub

        Public Sub AddObject(obj As IXYZDrawable3D)
            If obj Is Nothing Then Exit Sub
            pObjects.Add(obj)

            'If data is already loaded, include this object in axis bounds
            RecomputeAxisBoundsIncludingObjects()
        End Sub

        Public Sub SetObjects(objs As IEnumerable(Of IXYZDrawable3D))
            pObjects.Clear()
            If objs IsNot Nothing Then
                pObjects.AddRange(objs)
            End If

            'If data is already loaded, include all objects in axis bounds
            RecomputeAxisBoundsIncludingObjects()
        End Sub


        ' Set Values------------------------------------------------------------
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

        'Convenience: set all at once
        Public Sub SetAxisLimits(Optional xmin As Double? = Nothing, Optional xmax As Double? = Nothing,
                                 Optional ymin As Double? = Nothing, Optional ymax As Double? = Nothing,
                                 Optional zmin As Double? = Nothing, Optional zmax As Double? = Nothing)

            'X
            If xmin.HasValue Xor xmax.HasValue Then
                Throw New ArgumentException("X axis manual limits require both xmin and xmax.")
            ElseIf xmin.HasValue AndAlso xmax.HasValue Then
                SetAxisLimitsX(xmin.Value, xmax.Value)
            Else
                ClearAxisLimitsX()
            End If

            'Y
            If ymin.HasValue Xor ymax.HasValue Then
                Throw New ArgumentException("Y axis manual limits require both ymin and ymax.")
            ElseIf ymin.HasValue AndAlso ymax.HasValue Then
                SetAxisLimitsY(ymin.Value, ymax.Value)
            Else
                ClearAxisLimitsY()
            End If

            'Z
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


        ''' <summary>
        ''' Sets the name of the Excel chart object created by <c>draw()</c>.
        ''' </summary>
        Public WriteOnly Property ChartName() As String
            Set(value As String)
                pChartName = value
            End Set
        End Property

        ''' <summary>
        ''' Enables or disables proportional axis scaling.  
        ''' 
        ''' If enabled, each axis is scaled by:
        ''' <code>
        ''' scale_ratio = axis_range / max(range_x, range_y, range_z)
        ''' </code>
        ''' ensuring that the 3D geometry is not distorted.
        ''' </summary>
        ''' <param name="bScale">True to scale axes proportionally.</param>
        Public Sub ScaleAxis(bScale As Boolean)
            bScaleAxes = bScale

            RefreshScaleRatios()
        End Sub

        ''' <summary>
        ''' Supplies raw X, Y, Z data and computes minima and ranges for each axis.
        ''' </summary>
        ''' <param name="arXdata">X‑coordinates.</param>
        ''' <param name="arYdata">Y‑coordinates.</param>
        ''' <param name="arZdata">Z‑coordinates.</param>
        Public Sub dataInputs(arXdata() As Double, arYdata() As Double, arZdata() As Double)
            x_raw = arXdata
            y_raw = arYdata
            z_raw = arZdata

            If x_raw Is Nothing OrElse y_raw Is Nothing OrElse z_raw Is Nothing Then Exit Sub
            n = x_raw.Length

            'Compute bounds from data, then expand to include any attached 3D objects
            RecomputeAxisBoundsIncludingObjects()
        End Sub

        ''' <summary>
        ''' Sets zoom, XY shifts, and rotation angles for the 3D → 2D projection.
        ''' 
        ''' Rotation angles:
        ''' <list type="bullet">
        '''   <item><description><c>dXrot</c> — rotation around X‑axis</description></item>
        '''   <item><description><c>dZrot</c> — rotation around Z‑axis</description></item>
        ''' </list>
        ''' </summary>
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

        ''' <summary>
        ''' Sets axis labels for X, Y, and Z axes.
        ''' </summary>
        Public Sub axesLabelInputs(Xlabel As String, Ylabel As String, Zlabel As String)
            pXlabel = Xlabel
            pYlabel = Ylabel
            pZlabel = Zlabel
        End Sub

        ''' <summary>
        ''' Controls whether XY, YZ, and XZ plane projections are shown, and sets their
        ''' marker sizes.
        ''' </summary>
        Public Sub showPlanePointInputs(ShowXYplanePoints As Boolean, ShowYZplanePoints As Boolean, ShowXZplanePoints As Boolean,
                             XYplanePointSize As Integer, YZplanePointSize As Integer, XZplanePointSize As Integer)
            pbShowXYplanePoints = ShowXYplanePoints
            pbShowYZplanePoints = ShowYZplanePoints
            pbShowXZplanePoints = ShowXZplanePoints
            pXYplanePointSize = XYplanePointSize
            pYZplanePointSize = YZplanePointSize
            pXZplanePointSize = XZplanePointSize
        End Sub

        ''' <summary>
        ''' Sets optional display settings:
        ''' <list type="bullet">
        '''   <item><description>Data labels</description></item>
        '''   <item><description>Z‑drop lines</description></item>
        '''   <item><description>Gridlines</description></item>
        '''   <item><description>Label font size</description></item>
        '''   <item><description>Label position</description></item>
        '''   <item><description>Marker size</description></item>
        ''' </list>
        ''' </summary>
        Public Sub settingsInputs(Optional bDataLabels As Boolean = False, Optional bZdropLines As Boolean = True, Optional bShowGridlines As Boolean = True,
                       Optional PointLabelFontSize As Integer = 9, Optional DataLabelPosition As Integer = XlDataLabelPosition.xlLabelPositionRight,
                       Optional DataMarakerSize As Integer = 6, Optional DataMarkerStyle As XlMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle)
            pbDataLabels = bDataLabels
            pbZdropLines = bZdropLines
            pbShowGridlines = bShowGridlines
            pPointLabelFontSize = PointLabelFontSize
            pDataLabelPosition = DataLabelPosition
            pDataMarakerSize = DataMarakerSize
            pDataMarkerStyle = DataMarkerStyle
        End Sub

        ''' <summary>
        ''' Assigns text labels to each data point.
        ''' </summary>
        Public Sub SetDataLabels(x() As String)
            DataLabels_ = x
        End Sub

        ''' <summary>
        ''' Assigns group identifiers to each point and computes group counts.
        ''' Enables grouped plotting if more than one group is present.
        ''' </summary>
        Sub SetGroups(x() As String)
            Groups = x
            'get unique values
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

            'Axis direction defaults (normal): +1 for all axes
            axis_dir_(0) = 1.0
            axis_dir_(1) = 1.0
            axis_dir_(2) = 1.0

            'Cut planes normalized position. Default is on the "minimum" faces of the cube.
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
            pDataMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle

            pbDataLabels = False
            pPointLabelFontSize = 9
            pDataLabelPosition = XlDataLabelPosition.xlLabelPositionRight
        End Sub

        ''' <summary>
        ''' Controls the direction in which each axis increases when displayed.
        '''
        ''' Setting an axis to <c>True</c> reverses its display direction (min ↔ max).
        ''' Internally this is implemented by flipping the corresponding normalized
        ''' coordinate (multiplying by -1 in the [-0.5, +0.5] cube).
        '''
        ''' Note: This affects data points, tick marks/labels, cage, gridlines, cut-planes,
        ''' and the optional wire sphere.
        ''' </summary>
        Public Sub AxisDirectionInputs(Optional flipX As Boolean = False,
                                       Optional flipY As Boolean = False,
                                       Optional flipZ As Boolean = False)
            axis_dir_(0) = If(flipX, -1.0, 1.0)
            axis_dir_(1) = If(flipY, -1.0, 1.0)
            axis_dir_(2) = If(flipZ, -1.0, 1.0)

            'Keep default cut planes on the "minimum" raw side by tying them to axis direction.
            cp_normalized_pos(0) = -0.5 * axis_dir_(0)
            cp_normalized_pos(1) = -0.5 * axis_dir_(1)
            cp_normalized_pos(2) = -0.5 * axis_dir_(2)
        End Sub

        ''' <summary>
        ''' Renders the full 3D scatter plot into an Excel chart.
        ''' 
        ''' The method:
        ''' <list type="number">
        '''   <item><description>Computes rotation matrices and cut‑plane geometry.</description></item>
        '''   <item><description>Creates or reuses an Excel chart.</description></item>
        '''   <item><description>Draws the 3D cage (bounding box).</description></item>
        '''   <item><description>Draws tick marks and tick labels for all axes.</description></item>
        '''   <item><description>Draws gridlines on XY, YZ, XZ planes (optional).</description></item>
        '''   <item><description>Plots cut‑plane projections (optional).</description></item>
        '''   <item><description>Plots grouped or ungrouped 3D data points.</description></item>
        '''   <item><description>Optionally draws Z‑drop lines for depth perception.</description></item>
        '''   <item><description>Applies axis labels and chart title.</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="figure">Optional existing chart; otherwise a new chart is created.</param>
        Public Sub draw(Optional figure As Chart = Nothing)

            Dim i As Integer, j As Integer, grpID As Integer, zeros() As Double
            'Arrays for By Group display
            Dim ByGroupX() As Double, ByGroupY() As Double, ByGroupDataPointLabel() As String, ByGroupError_bar() As Double

            'Ensure axis bounds reflect current data + objects + manual limits
            If x_raw IsNot Nothing AndAlso y_raw IsNot Nothing AndAlso z_raw IsNot Nothing AndAlso x_raw.Length > 0 Then
                RecomputeAxisBoundsIncludingObjects()
            End If

            Call get_rotations()
            Call cut_planes()
            'Call wire_sphere_data()


            If figure Is Nothing Then
                AppGlobals.app.ActiveWorkbook.Charts.Add()
                figure = CType(AppGlobals.app.ActiveWorkbook.ActiveChart, Microsoft.Office.Interop.Excel.Chart)
            End If

            With figure
                Try
                    'Reset LegendEntries by setting it to false and true afterwars
                    .HasLegend = False
                    .HasLegend = True
                    .HasLegend = bGroups
                    .Name = pChartName
                    .HasTitle = False
                    .HasTitle = True
                    .HasTitle = False
                Catch
                End Try
                .ChartType = XlChartType.xlXYScatter

                '*** IMPORTANT RESET FOR REUSE ***
                'When reusing the same chart object, Excel can carry over formatting
                'that makes new line series (like the Cage) render invisible.
                Try
                    .ChartArea.ClearFormats()
                    .PlotArea.ClearFormats()
                Catch ex As Exception
                    CoreServices.Logger.Debug("ClearFormats failed: " & ex.Message)
                End Try

                'delete extra series
                Do While .SeriesCollection.Count > 0
                    .SeriesCollection(1).Delete
                Loop

                '------------------------------------------------------------
                ' FIX: Do NOT delete axes. Keep them, lock scales, and hide them.
                ' Deleting axes breaks redraw/reuse because the second draw may
                ' not have axes to configure, and the Try/Catch skips scale locks.
                '------------------------------------------------------------
                Try
                    .HasAxis(XlAxisType.xlCategory, XlAxisGroup.xlPrimary) = False
                    .HasAxis(XlAxisType.xlValue, XlAxisGroup.xlPrimary) = False
                    .HasAxis(XlAxisType.xlCategory, XlAxisGroup.xlPrimary) = True
                    .HasAxis(XlAxisType.xlValue, XlAxisGroup.xlPrimary) = True

                    'X axis
                    Dim axX As Axis = .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary)
                    axX.MinimumScaleIsAuto = False
                    axX.MaximumScaleIsAuto = False
                    axX.MinimumScale = -1
                    axX.MaximumScale = 1
                    axX.HasMajorGridlines = False
                    axX.HasMinorGridlines = False
                    axX.MajorTickMark = XlTickMark.xlTickMarkNone
                    axX.MinorTickMark = XlTickMark.xlTickMarkNone
                    axX.TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                    axX.Border.LineStyle = XlLineStyle.xlLineStyleNone   'hide axis line (Excel-only)

                    'Y axis
                    Dim axY As Axis = .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary)
                    axY.MinimumScaleIsAuto = False
                    axY.MaximumScaleIsAuto = False
                    axY.MinimumScale = 0
                    axY.MaximumScale = 2
                    axY.HasMajorGridlines = False
                    axY.HasMinorGridlines = False
                    axY.MajorTickMark = XlTickMark.xlTickMarkNone
                    axY.MinorTickMark = XlTickMark.xlTickMarkNone
                    axY.TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                    axY.Border.LineStyle = XlLineStyle.xlLineStyleNone   'hide axis line (Excel-only)

                Catch ex As Exception
                    CoreServices.Logger.Debug("Axis setup failed: " & ex.Message)
                End Try


                'plot cage-------------------------------------------------------------
                Call cage_data()
                Dim seriesID As Integer = 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                    .XValues = xs_cage
                    .Values = ys_cage
                    .Name = "Cage"
                    'Force a real line even after redraw/reuse
                    .Border.LineStyle = XlLineStyle.xlContinuous
                    .Border.Weight = XlBorderWeight.xlThin
                    .Border.Color = RGB(100, 100, 100)

                    .Format.Line.Weight = 1
                    .Format.Line.Visible = True
                    .Format.Line.ForeColor.RGB = RGB(100, 100, 100)

                    'Axes labels
                    .points(1).HasDataLabel = True
                    .points(1).DataLabel.text = pXlabel
                    .points(1).DataLabel.Position = XlDataLabelPosition.xlLabelPositionLeft
                    .points(1).DataLabel.Font.Size = 13
                    .points(1).DataLabel.Font.Bold = True

                    .points(7).HasDataLabel = True
                    .points(7).DataLabel.text = pYlabel
                    .points(7).DataLabel.Position = XlDataLabelPosition.xlLabelPositionRight
                    .points(7).DataLabel.Font.Size = 13
                    .points(7).DataLabel.Font.Bold = True

                    .points(9).HasDataLabel = True
                    .points(9).DataLabel.text = pZlabel
                    .points(9).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                    .points(9).DataLabel.Font.Size = 13
                    .points(9).DataLabel.Font.Bold = True
                End With

                'plot tick marks for all axes------------------------------------------
                Call tick_marks_data()
                seriesID += 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .ClearFormats()
                    .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                    .XValues = xs_x_axisticks
                    .Values = ys_x_axisticks
                    .Name = "x_ticks"
                    'Force a real line even after redraw/reuse
                    .Border.LineStyle = XlLineStyle.xlContinuous
                    .Border.Weight = XlBorderWeight.xlThin
                    .Border.Color = RGB(0, 0, 0)

                    .Format.Line.Weight = 2
                    .Format.Line.Visible = True
                    .Format.Line.ForeColor.RGB = RGB(0, 0, 0)

                    'Attach a label to each data point in the chart.
                    For t As Integer = 0 To x_tick_labels.GetUpperBound(0)
                        Dim pt As Integer = t * 3 + 2
                        .Points(pt).HasDataLabel = True
                        .Points(pt).DataLabel.Text = x_tick_labels(t)
                        .Points(pt).DataLabel.Position = XlDataLabelPosition.xlLabelPositionRight
                        .Points(pt).DataLabel.Font.Size = 10
                    Next

                    For i = 0 To xs_x_axisticks.Length - 1
                        If xs_x_axisticks(i) = gToDeleteGridLineValue Then
                            .Points(i + 1).Format.Line.Visible = False
                            If i + 1 <= xs_x_axisticks.GetUpperBound(0) Then .Points(i + 2).Format.Line.Visible = False
                            i += 1
                        End If
                    Next
                End With

                seriesID += 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                    .XValues = xs_y_axisticks
                    .Values = ys_y_axisticks
                    .Name = "y_ticks"
                    'Force a real line even after redraw/reuse
                    .Border.LineStyle = XlLineStyle.xlContinuous
                    .Border.Weight = XlBorderWeight.xlThin
                    .Border.Color = RGB(0, 0, 0)

                    .Format.Line.Weight = 2
                    .Format.Line.Visible = True
                    .Format.Line.ForeColor.RGB = RGB(0, 0, 0)

                    'Attach a label to each data point in the chart.
                    For t As Integer = 0 To y_tick_labels.GetUpperBound(0)
                        Dim pt As Integer = t * 3 + 2
                        .Points(pt).HasDataLabel = True
                        .Points(pt).DataLabel.Text = y_tick_labels(t)
                        .Points(pt).DataLabel.Position = XlDataLabelPosition.xlLabelPositionBelow
                        .Points(pt).DataLabel.Font.Size = 10
                    Next

                    For i = 0 To xs_y_axisticks.Length - 1
                        If xs_y_axisticks(i) = gToDeleteGridLineValue Then
                            .Points(i + 1).Format.Line.Visible = False
                            If i + 1 <= xs_y_axisticks.GetUpperBound(0) Then .Points(i + 2).Format.Line.Visible = False
                            i += 1
                        End If
                    Next
                End With

                seriesID += 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                    .XValues = xs_z_axisticks
                    .Values = ys_z_axisticks
                    .Name = "z_ticks"
                    'Force a real line even after redraw/reuse
                    .Border.LineStyle = XlLineStyle.xlContinuous
                    .Border.Weight = XlBorderWeight.xlThin
                    .Border.Color = RGB(0, 0, 0)

                    .Format.Line.Weight = 2
                    .Format.Line.Visible = True
                    .Format.Line.ForeColor.RGB = RGB(0, 0, 0)

                    'Attach a label to each data point in the chart.
                    For t As Integer = 0 To z_tick_labels.GetUpperBound(0)
                        Dim pt As Integer = t * 3 + 2
                        .Points(pt).HasDataLabel = True
                        .Points(pt).DataLabel.Text = z_tick_labels(t)
                        .Points(pt).DataLabel.Position = XlDataLabelPosition.xlLabelPositionRight
                        .Points(pt).DataLabel.Font.Size = 10
                    Next

                    For i = 0 To xs_z_axisticks.Length - 1
                        If xs_z_axisticks(i) = gToDeleteGridLineValue Then
                            .Points(i + 1).Format.Line.Visible = False
                            If i + 1 <= xs_z_axisticks.GetUpperBound(0) Then .Points(i + 2).Format.Line.Visible = False
                            i += 1
                        End If
                    Next
                End With

                If pbShowGridlines Then
                    Call get_gridlines()
                    'XY plane grid lines-----------------------------------------------
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = xs_xy_x_gridline
                        .Values = ys_xy_x_gridline
                        .Name = "XY_X_gridlines"
                        .Format.Line.Weight = 0.5
                        .Format.Line.Visible = True
                        .Format.Line.DashStyle = 7 'msoLineLongDash 'msoLineSolid
                        .Format.Line.ForeColor.RGB = RGB(200, 200, 200)

                        'Make redundant line segments invisible
                        For i = 0 To xs_xy_x_gridline.Length - 1
                            If xs_xy_x_gridline(i) = gToDeleteGridLineValue Then
                                .points(i + 1).format.line.visible = False
                                If i + 1 <= UBound(xs_xy_x_gridline) Then .points(i + 2).format.line.visible = False
                                i += 1
                            End If
                        Next
                    End With
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = xs_xy_y_gridline
                        .Values = ys_xy_y_gridline
                        .Name = "XY_Y_gridlines"
                        .Format.Line.Weight = 0.5
                        .Format.Line.Visible = True
                        .Format.Line.DashStyle = 7 'msoLineLongDash 'msoLineSolid
                        .Format.Line.ForeColor.RGB = RGB(200, 200, 200)

                        'Make redundant line segments invisible
                        For i = 0 To xs_xy_y_gridline.Length - 1
                            If xs_xy_y_gridline(i) = gToDeleteGridLineValue Then
                                .points(i + 1).Format.Line.Visible = False
                                If i + 1 <= UBound(xs_xy_y_gridline) Then .points(i + 2).Format.Line.Visible = False
                                i += 1
                            End If
                        Next
                    End With
                    'XZ plane grid lines-----------------------------------------------
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = xs_xz_x_gridline
                        .Values = ys_xz_x_gridline
                        .Name = "XZ_X_gridlines"
                        .Format.Line.Weight = 0.5
                        .Format.Line.Visible = True
                        .Format.Line.DashStyle = 7 'msoLineLongDash 'msoLineSolid
                        .Format.Line.ForeColor.RGB = RGB(200, 200, 200)

                        'Make redundant line segments invisible
                        For i = 0 To xs_xz_x_gridline.Length - 1
                            If xs_xz_x_gridline(i) = gToDeleteGridLineValue Then
                                .points(i + 1).Format.Line.Visible = False
                                If i + 1 <= UBound(xs_xz_x_gridline) Then .points(i + 2).Format.Line.Visible = False
                                i += 1
                            End If
                        Next
                    End With
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = xs_xz_z_gridline
                        .Values = ys_xz_z_gridline
                        .Name = "XZ_Y_gridlines"
                        .Format.Line.Weight = 0.5
                        .Format.Line.Visible = True
                        .Format.Line.DashStyle = 7 'msoLineLongDash 'msoLineSolid
                        .Format.Line.ForeColor.RGB = RGB(200, 200, 200)

                        'Make redundant line segments invisible
                        For i = 0 To xs_xz_z_gridline.Length - 1
                            If xs_xz_z_gridline(i) = gToDeleteGridLineValue Then
                                .points(i + 1).Format.Line.Visible = False
                                If i + 1 <= UBound(xs_xz_z_gridline) Then .points(i + 2).Format.Line.Visible = False
                                i += 1
                            End If
                        Next
                    End With
                    'YZ plane grid lines-----------------------------------------------
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = xs_yz_y_gridline
                        .Values = ys_yz_y_gridline
                        .Name = "YZ_Y_gridlines"
                        .Format.Line.Weight = 0.5
                        .Format.Line.Visible = True
                        .Format.Line.DashStyle = 7 'msoLineLongDash 'msoLineSolid
                        .Format.Line.ForeColor.RGB = RGB(200, 200, 200)

                        'Make redundant line segments invisible
                        For i = 0 To xs_yz_y_gridline.Length - 1
                            If xs_yz_y_gridline(i) = gToDeleteGridLineValue Then
                                .points(i + 1).Format.Line.Visible = False
                                If i + 1 <= UBound(xs_yz_y_gridline) Then .points(i + 2).Format.Line.Visible = False
                                i += 1
                            End If
                        Next
                    End With
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = xs_yz_z_gridline
                        .Values = ys_yz_z_gridline
                        .Name = "YZ_Z_gridlines"
                        .Format.Line.Weight = 0.5
                        .Format.Line.Visible = True
                        .Format.Line.DashStyle = 7 'msoLineLongDash 'msoLineSolid
                        .Format.Line.ForeColor.RGB = RGB(200, 200, 200)

                        'Make redundant line segments invisible
                        For i = 0 To xs_yz_z_gridline.Length - 1
                            If xs_yz_z_gridline(i) = gToDeleteGridLineValue Then
                                .points(i + 1).Format.Line.Visible = False
                                If i + 1 <= UBound(xs_yz_z_gridline) Then .points(i + 2).Format.Line.Visible = False
                                i += 1
                            End If
                        Next
                    End With
                End If

                'Cut plane data--------------------------------------------------------
                If pbShowYZplanePoints Then
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatter
                        .XValues = xs_cutplane_x
                        .Values = ys_cutplane_x
                        .Name = "x_plane"
                        .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                        .MarkerSize = pYZplanePointSize
                        .Format.Fill.Visible = True
                        .MarkerForegroundColor = RGB(0, 0, 0)
                        .MarkerBackgroundColor = RGB(0, 0, 0)
                    End With
                End If
                If pbShowXZplanePoints Then
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatter
                        .XValues = xs_cutplane_y
                        .Values = ys_cutplane_y
                        .Name = "y_plane"
                        .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                        .MarkerSize = pXZplanePointSize
                        .Format.Fill.Visible = True
                        .MarkerForegroundColor = RGB(0, 0, 0)
                        .MarkerBackgroundColor = RGB(0, 0, 0)
                    End With
                End If
                If pbShowXYplanePoints Then
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatter
                        .XValues = xs_cutplane_z
                        .Values = ys_cutplane_z
                        .Name = "z_plane"
                        .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                        .MarkerSize = pXYplanePointSize
                        .Format.Fill.Visible = True
                        .MarkerForegroundColor = RGB(0, 0, 0)
                        .MarkerBackgroundColor = RGB(0, 0, 0)
                    End With
                End If



                '----------------------------------------------------------------------
                'Plot data points
                Call normalizedData()

                'Draw additional 3D objects (wire sphere, ellipsoid, prism, etc.)
                For Each obj In pObjects
                    obj.Draw(Me, figure)
                Next
                'Keep seriesID aligned even if objects add 1+ series
                seriesID = figure.SeriesCollection.Count

                If bGroups Then
                    For grpID = 0 To pgrpCounts.Length - 1
                        ReDim ByGroupX(pgrpCounts(grpID) - 1), ByGroupY(pgrpCounts(grpID) - 1)
                        ReDim ByGroupDataPointLabel(pgrpCounts(grpID) - 1), ByGroupError_bar(pgrpCounts(grpID) - 1)

                        'populate arrays with data for this group
                        j = 0
                        For i = 1 To n
                            If Groups(i - 1) = pgrpIds(grpID) Then
                                ByGroupX(j) = xs_norm_data(i - 1)
                                ByGroupY(j) = ys_norm_data(i - 1)
                                If pbDataLabels AndAlso DataLabels_ IsNot Nothing Then ByGroupDataPointLabel(j) = DataLabels_(i - 1)
                                ByGroupError_bar(j) = error_bars(i - 1)
                                j += 1
                            End If
                        Next

                        'display this group
                        seriesID += 1
                        .SeriesCollection.NewSeries
                        With .SeriesCollection(seriesID)
                            .ChartType = XlChartType.xlXYScatter
                            .XValues = ByGroupX
                            .Values = ByGroupY
                            .Name = "Group_" & CStr(pgrpIds(grpID))
                            '.MarkerStyle = XlMarkerStyle.xlMarkerStyleAutomatic 'xlMarkerStyleCircle
                            .MarkerStyle = pDataMarkerStyle
                            .Format.Line.Weight = 2
                            .MarkerSize = pDataMarakerSize
                            .Border.LineStyle = XlLineStyle.xlLineStyleNone

                            'Attach a label to each data point in the chart.
                            If pbDataLabels AndAlso DataLabels_ IsNot Nothing Then
                                For i = 1 To pgrpCounts(grpID)
                                    If ByGroupDataPointLabel(i - 1) <> String.Empty Then
                                        .points(i).HasDataLabel = True
                                        .points(i).DataLabel.text = CStr(ByGroupDataPointLabel(i - 1))
                                        .points(i).DataLabel.Position = pDataLabelPosition 'xlLabelPositionRight
                                        .points(i).DataLabel.Font.Size = pPointLabelFontSize
                                    End If
                                Next
                            End If

                            If pbZdropLines Then
                                ReDim zeros(pgrpCounts(grpID) - 1)
                                .HasErrorBars = True
                                .ErrorBars.EndStyle = XlEndStyleCap.xlNoCap
                                .ErrorBars.Format.Line.DashStyle = 4 'msoLineDash
                                .ErrorBar(Direction:=XlErrorBarDirection.xlY, Include:=Constants.xlMinusValues, Type:=XlErrorBarType.xlErrorBarTypeCustom, amount:=zeros, MinusValues:=ByGroupError_bar)
                                .ErrorBar(Direction:=XlErrorBarDirection.xlX, Include:=Constants.xlNone, Type:=XlErrorBarType.xlErrorBarTypeFixedValue, amount:=0, MinusValues:=0)
                            End If
                        End With
                    Next grpID

                    Try
                        For i = (.Legend.LegendEntries.Count - pgrpCounts.Length) To 1 Step -1
                            .Legend.LegendEntries(i).Delete
                        Next
                    Catch
                    End Try
                Else
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .ChartType = XlChartType.xlXYScatter
                        .XValues = xs_norm_data
                        .Values = ys_norm_data
                        .Name = "Data"
                        '.MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                        .MarkerStyle = pDataMarkerStyle
                        '.Format.Line.Weight = 2
                        .MarkerSize = pDataMarakerSize
                        '.MarkerForegroundColor = RGB(100, 100, 100)
                        .Format.Fill.Visible = True
                        .Border.LineStyle = XlLineStyle.xlLineStyleNone

                        'Attach a label to each data point in the chart.
                        If pbDataLabels AndAlso DataLabels_ IsNot Nothing Then
                            For i = 1 To DataLabels_.Length
                                If DataLabels_(i - 1) <> String.Empty Then
                                    .points(i).HasDataLabel = True
                                    .points(i).DataLabel.text = CStr(DataLabels_(i - 1))
                                    .points(i).DataLabel.Position = pDataLabelPosition 'xlLabelPositionRight
                                    .points(i).DataLabel.Font.Size = pPointLabelFontSize
                                End If
                            Next
                        End If

                        If pbZdropLines Then
                            ReDim zeros(n - 1)
                            .HasErrorBars = True
                            .ErrorBars.EndStyle = XlEndStyleCap.xlNoCap
                            .ErrorBars.Format.Line.DashStyle = 4 'msoLineDash
                            .ErrorBar(Direction:=XlErrorBarDirection.xlY, Include:=Constants.xlMinusValues, Type:=XlErrorBarType.xlErrorBarTypeCustom, amount:=zeros, MinusValues:=error_bars)
                            .ErrorBar(Direction:=XlErrorBarDirection.xlX, Include:=Constants.xlNone, Type:=XlErrorBarType.xlErrorBarTypeFixedValue, amount:=0, MinusValues:=0)
                        End If
                    End With
                End If
            End With

        End Sub


        ''' <summary>
        ''' Computes all gridline coordinates for the XY, XZ, and YZ planes after
        ''' 3D → 2D projection.  
        ''' 
        ''' Gridlines are generated in normalized 3D space, rotated using the current
        ''' rotation matrices, scaled, shifted, and finally stored as 2D coordinates
        ''' for Excel plotting.
        ''' 
        ''' The method populates:
        ''' <list type="bullet">
        '''   <item><description>xs_xy_x_gridline / ys_xy_x_gridline — gridlines on XY plane ⟂ X‑axis</description></item>
        '''   <item><description>xs_xy_y_gridline / ys_xy_y_gridline — gridlines on XY plane ⟂ Y‑axis</description></item>
        '''   <item><description>xs_xz_x_gridline / ys_xz_x_gridline — gridlines on XZ plane ⟂ X‑axis</description></item>
        '''   <item><description>xs_xz_z_gridline / ys_xz_z_gridline — gridlines on XZ plane ⟂ Z‑axis</description></item>
        '''   <item><description>xs_yz_y_gridline / ys_yz_y_gridline — gridlines on YZ plane ⟂ Y‑axis</description></item>
        '''   <item><description>xs_yz_z_gridline / ys_yz_z_gridline — gridlines on YZ plane ⟂ Z‑axis</description></item>
        ''' </list>
        ''' 
        ''' Gridlines that fall outside the visible region or cross panel boundaries
        ''' are marked with <c>gToDeleteGridLineValue</c> so the Excel renderer can
        ''' hide them.
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>rot_1</c>, <c>rot_2</c> — rotation vectors</description></item>
        '''   <item><description><c>zoom_internal</c>, <c>x_shift_internal</c>, <c>y_shift_internal</c></description></item>
        ''' </list>
        ''' </summary>
        Private Sub get_gridlines()
            Dim x As Double, y As Double, z As Double

            'Keep gridlines on the original cage faces (do not move when axis is flipped)
            Dim xH As Double = 0.5
            Dim xL As Double = -0.5
            Dim yH As Double = 0.5
            Dim yL As Double = -0.5
            Dim zH As Double = 0.5
            Dim zL As Double = -0.5

            'XY plane gridlines
            ReDim xs_xy_x_gridline(3 * (tx(0) + 1) - 1), ys_xy_x_gridline(3 * (tx(0) + 1) - 1)
            ReDim xs_xy_y_gridline(3 * (tx(1) + 1) - 1), ys_xy_y_gridline(3 * (tx(1) + 1) - 1)
            'XZ plane gridlines
            ReDim xs_xz_x_gridline(3 * (tx(0) + 1) - 1), ys_xz_x_gridline(3 * (tx(0) + 1) - 1)
            ReDim xs_xz_z_gridline(3 * (tx(2) + 1) - 1), ys_xz_z_gridline(3 * (tx(2) + 1) - 1)
            'YZ plane gridlines
            ReDim xs_yz_y_gridline(3 * (tx(1) + 1) - 1), ys_yz_y_gridline(3 * (tx(1) + 1) - 1)
            ReDim xs_yz_z_gridline(3 * (tx(2) + 1) - 1), ys_yz_z_gridline(3 * (tx(2) + 1) - 1)

            For i = 0 To tx(0)
                'XY plane gridlines - perpendicular to X axis
                x = ftn(0) + i * tsn(0) 'x coordinate of tickmark start of the line
                xs_xy_x_gridline(i * 3) = (x * rot_1(0) + yH * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_x_gridline(i * 3) = (x * rot_2(0) + yH * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_x_gridline(1 + i * 3) = (x * rot_1(0) + yL * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_x_gridline(1 + i * 3) = (x * rot_2(0) + yL * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_xy_x_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xy_x_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i = 0 To tx(1)
                'XY plane gridlines - perpendicular to Y axis
                y = ftn(1) + i * tsn(1) 'pY coordinate of tickmark start of the line
                xs_xy_y_gridline(i * 3) = (xH * rot_1(0) + y * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_y_gridline(i * 3) = (xH * rot_2(0) + y * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_y_gridline(1 + i * 3) = (xL * rot_1(0) + y * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_y_gridline(1 + i * 3) = (xL * rot_2(0) + y * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_xy_y_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xy_y_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            '---------------------------------------------------------------------------------------
            For i = 0 To tx(0)
                'XZ plane gridlines - perpendicular to X axis
                x = ftn(0) + i * tsn(0) 'x coordinate of tickmark start of the line
                xs_xz_x_gridline(i * 3) = (x * rot_1(0) + yL * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_x_gridline(i * 3) = (x * rot_2(0) + yL * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_x_gridline(1 + i * 3) = (x * rot_1(0) + yL * rot_1(1) + zH * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_x_gridline(1 + i * 3) = (x * rot_2(0) + yL * rot_2(1) + zH * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_xz_x_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xz_x_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i = 0 To tx(2)
                'YZ plane gridlines - perpendicular to Z axis
                z = ftn(2) + i * tsn(2) 'z coordinate of tickmark start of the line
                xs_xz_z_gridline(i * 3) = (xL * rot_1(0) + yL * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_z_gridline(i * 3) = (xL * rot_2(0) + yL * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_z_gridline(1 + i * 3) = (xH * rot_1(0) + yL * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_z_gridline(1 + i * 3) = (xH * rot_2(0) + yL * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_xz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            '---------------------------------------------------------------------------------------
            For i = 0 To tx(1)
                'YZ plane gridlines - perpendicular to Y axis
                y = ftn(1) + i * tsn(1) 'pY coordinate of tickmark start of the line
                xs_yz_y_gridline(i * 3) = (xL * rot_1(0) + y * rot_1(1) + zH * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_y_gridline(i * 3) = (xL * rot_2(0) + y * rot_2(1) + zH * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_y_gridline(1 + i * 3) = (xL * rot_1(0) + y * rot_1(1) + zL * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_y_gridline(1 + i * 3) = (xL * rot_2(0) + y * rot_2(1) + zL * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_yz_y_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_yz_y_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
            For i = 0 To tx(2)
                'YZ plane gridlines - perpendicular to Z axis
                z = ftn(2) + i * tsn(2) 'z coordinate of tickmark start of the line
                xs_yz_z_gridline(i * 3) = (xL * rot_1(0) + yL * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_z_gridline(i * 3) = (xL * rot_2(0) + yL * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_z_gridline(1 + i * 3) = (xL * rot_1(0) + yH * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_z_gridline(1 + i * 3) = (xL * rot_2(0) + yH * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_yz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_yz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next
        End Sub

        ''' <summary>
        ''' Computes the intersection of the dataset with three orthogonal cut‑planes:
        ''' <list type="bullet">
        '''   <item><description>X = constant</description></item>
        '''   <item><description>Y = constant</description></item>
        '''   <item><description>Z = constant</description></item>
        ''' </list>
        ''' 
        ''' The cut‑plane positions are stored in <c>cp_normalized_pos()</c> and are
        ''' expressed in normalized 3D coordinates.
        ''' 
        ''' For each plane, the method:
        ''' <list type="number">
        '''   <item><description>Identifies points lying on the plane (within tolerance).</description></item>
        '''   <item><description>Projects them into 2D using rotation + scaling.</description></item>
        '''   <item><description>Stores results in:
        '''     <list type="bullet">
        '''       <item><description>xs_cutplane_x / ys_cutplane_x</description></item>
        '''       <item><description>xs_cutplane_y / ys_cutplane_y</description></item>
        '''       <item><description>xs_cutplane_z / ys_cutplane_z</description></item>
        '''     </list>
        '''   </description></item>
        ''' </list>
        ''' 
        ''' These points are plotted as XY, YZ, or XZ plane projections depending on
        ''' user settings.
        ''' </summary>
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

        ''' <summary>
        ''' Converts raw X, Y, Z data into normalized 3D coordinates, applies axis
        ''' scaling, rotation, zoom, and XY shifts, and stores the final 2D projected
        ''' coordinates for plotting.
        ''' 
        ''' The method:
        ''' <list type="number">
        '''   <item><description>Normalizes raw data into [0,1]³ using <c>raw_mins_</c> and <c>raw_ranges_</c>.</description></item>
        '''   <item><description>Applies axis scaling ratios (if enabled).</description></item>
        '''   <item><description>Applies rotation matrices <c>rot_1</c> and <c>rot_2</c>.</description></item>
        '''   <item><description>Applies zoom and XY shifts.</description></item>
        '''   <item><description>Stores results in <c>xs_norm_data</c>, <c>ys_norm_data</c>.</description></item>
        '''   <item><description>Computes <c>error_bars</c> for Z‑drop lines.</description></item>
        ''' </list>
        ''' 
        ''' If grouping is enabled, the grouped plotting logic in <c>draw()</c> uses
        ''' these arrays to build group‑specific series.
        ''' </summary>
        Public Sub normalizedData()
            Call getnorm()
            ReDim xs_norm_data(n - 1), ys_norm_data(n - 1), error_bars(n - 1)
            For i = 0 To n - 1
                xs_norm_data(i) = (x_norm(i) * rot_1(0) + y_norm(i) * rot_1(1) + z_norm(i) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_norm_data(i) = (x_norm(i) * rot_2(0) + y_norm(i) * rot_2(1) + z_norm(i) * rot_2(2) + y_shift_internal) * zoom_internal
                'error_bars(i) = ys_norm_data(i) - ys_cutplane_z(i)
                'Z-drop lines should always drop to the fixed "bottom" cage face (z = -0.5),
                'regardless of Z-axis flip direction.
                Dim ys_zdrop_base As Double = (x_norm(i) * rot_2(0) + y_norm(i) * rot_2(1) + (-0.5) * rot_2(2) + y_shift_internal) * zoom_internal
                error_bars(i) = ys_norm_data(i) - ys_zdrop_base
            Next
        End Sub

        ''' <summary>
        ''' Normalizes raw X, Y, Z data into the unit cube [0,1]³ prior to 3D → 2D
        ''' projection.  
        ''' 
        ''' For each axis k ∈ {X,Y,Z}, the method computes:
        ''' <code>
        ''' norm_k(i) = (raw_k(i) − raw_mins_(k)) / raw_ranges_(k)
        ''' </code>
        ''' 
        ''' The normalized coordinates are stored in:
        ''' <list type="bullet">
        '''   <item><description><c>x_norm()</c></description></item>
        '''   <item><description><c>y_norm()</c></description></item>
        '''   <item><description><c>z_norm()</c></description></item>
        ''' </list>
        ''' 
        ''' These values are later scaled, rotated, shifted, and projected by
        ''' <c>normalizedData()</c>.
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>raw_mins_()</c>, <c>raw_ranges_()</c></description></item>
        '''   <item><description><c>x_raw()</c>, <c>y_raw()</c>, <c>z_raw()</c></description></item>
        ''' </list>
        ''' </summary>
        Private Sub getnorm()
            ReDim x_norm(n - 1), y_norm(n - 1), z_norm(n - 1)
            For i = 0 To n - 1
                x_norm(i) = ((x_raw(i) - raw_mins_(0)) / raw_ranges_(0) - 0.5) * axis_dir_(0)
                y_norm(i) = ((y_raw(i) - raw_mins_(1)) / raw_ranges_(1) - 0.5) * axis_dir_(1)
                z_norm(i) = ((z_raw(i) - raw_mins_(2)) / raw_ranges_(2) - 0.5) * axis_dir_(2)
            Next
        End Sub

        ''' <summary>
        ''' Computes tick‑mark geometry for all three axes (X, Y, Z) in normalized
        ''' 3D space and projects them into 2D coordinates for Excel plotting.
        ''' 
        ''' The method:
        ''' <list type="number">
        '''   <item><description>Determines tick spacing <c>ts()</c> based on raw ranges.</description></item>
        '''   <item><description>Computes first tick location <c>ft()</c> and number of ticks <c>tx()</c>.</description></item>
        '''   <item><description>Normalizes tick positions into [0,1] cube.</description></item>
        '''   <item><description>Applies 3D rotation and 2D projection.</description></item>
        '''   <item><description>Stores results in:
        '''     <list type="bullet">
        '''       <item><description>xs_x_axisticks / ys_x_axisticks</description></item>
        '''       <item><description>xs_y_axisticks / ys_y_axisticks</description></item>
        '''       <item><description>xs_z_axisticks / ys_z_axisticks</description></item>
        '''     </list>
        '''   </description></item>
        '''   <item><description>Generates tick‑label arrays:
        '''     <c>x_tick_labels</c>, <c>y_tick_labels</c>, <c>z_tick_labels</c>.</description></item>
        ''' </list>
        ''' 
        ''' Tick marks that fall outside the visible region are flagged for deletion
        ''' using <c>gToDeleteGridLineValue</c>.
        ''' </summary>
        Private Sub tick_marks_data()
            Call tick_marks_prerequisites()

            'Labels: one per tick (0..tx)
            ReDim x_tick_labels(CInt(tx(0))), y_tick_labels(CInt(tx(1))), z_tick_labels(CInt(tx(2)))

            'Allocate ticks like gridlines: (base, tip, dummy) per tick
            ReDim xs_x_axisticks(3 * (CInt(tx(0)) + 1) - 1), ys_x_axisticks(3 * (CInt(tx(0)) + 1) - 1)
            ReDim xs_y_axisticks(3 * (CInt(tx(1)) + 1) - 1), ys_y_axisticks(3 * (CInt(tx(1)) + 1) - 1)
            ReDim xs_z_axisticks(3 * (CInt(tx(2)) + 1) - 1), ys_z_axisticks(3 * (CInt(tx(2)) + 1) - 1)

            'Axis-face constants FIXED to the original cage faces (do not move when axis is flipped)
            Dim xBase As Double = 0.5
            Dim xTip As Double = 0.55
            Dim xL As Double = -0.5
            Dim yBase As Double = 0.5
            Dim yTip As Double = 0.55
            Dim zBack As Double = -0.5

            'X axis ticks: (x varies), tick goes in +Y direction at Z = -0.5
            For t As Integer = 0 To CInt(tx(0))
                Dim x As Double = ftn(0) + t * tsn(0)

                'base
                xs_x_axisticks(t * 3) = (x * rot_1(0) + yBase * rot_1(1) + zBack * rot_1(2) + x_shift_internal) * zoom_internal
                ys_x_axisticks(t * 3) = (x * rot_2(0) + yBase * rot_2(1) + zBack * rot_2(2) + y_shift_internal) * zoom_internal

                'tip
                xs_x_axisticks(t * 3 + 1) = (x * rot_1(0) + yTip * rot_1(1) + zBack * rot_1(2) + x_shift_internal) * zoom_internal
                ys_x_axisticks(t * 3 + 1) = (x * rot_2(0) + yTip * rot_2(1) + zBack * rot_2(2) + y_shift_internal) * zoom_internal

                'dummy separator (hidden later)
                xs_x_axisticks(t * 3 + 2) = gToDeleteGridLineValue
                ys_x_axisticks(t * 3 + 2) = gToDeleteGridLineValue

                x_tick_labels(t) = CStr(ft(0) + t * ts(0))
            Next

            'Y axis ticks: (y varies), tick goes in +X direction at Z = -0.5
            For t As Integer = 0 To CInt(tx(1))
                Dim y As Double = ftn(1) + t * tsn(1)

                'base
                xs_y_axisticks(t * 3) = (xBase * rot_1(0) + y * rot_1(1) + zBack * rot_1(2) + x_shift_internal) * zoom_internal
                ys_y_axisticks(t * 3) = (xBase * rot_2(0) + y * rot_2(1) + zBack * rot_2(2) + y_shift_internal) * zoom_internal

                'tip
                xs_y_axisticks(t * 3 + 1) = (xTip * rot_1(0) + y * rot_1(1) + zBack * rot_1(2) + x_shift_internal) * zoom_internal
                ys_y_axisticks(t * 3 + 1) = (xTip * rot_2(0) + y * rot_2(1) + zBack * rot_2(2) + y_shift_internal) * zoom_internal

                'dummy separator
                xs_y_axisticks(t * 3 + 2) = gToDeleteGridLineValue
                ys_y_axisticks(t * 3 + 2) = gToDeleteGridLineValue

                y_tick_labels(t) = CStr(ft(1) + t * ts(1))
            Next

            'Z axis ticks: (z varies), tick goes in +Y direction at X = -0.5
            For t As Integer = 0 To CInt(tx(2))
                Dim z As Double = ftn(2) + t * tsn(2)

                'base
                xs_z_axisticks(t * 3) = (xL * rot_1(0) + yBase * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_z_axisticks(t * 3) = (xL * rot_2(0) + yBase * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                'tip
                xs_z_axisticks(t * 3 + 1) = (xL * rot_1(0) + yTip * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_z_axisticks(t * 3 + 1) = (xL * rot_2(0) + yTip * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                'dummy separator
                xs_z_axisticks(t * 3 + 2) = gToDeleteGridLineValue
                ys_z_axisticks(t * 3 + 2) = gToDeleteGridLineValue

                z_tick_labels(t) = CStr(ft(2) + t * ts(2))
            Next
        End Sub

        ''' <summary>
        ''' Computes all prerequisite quantities needed for tick‑mark generation on
        ''' the X, Y, and Z axes.  
        ''' 
        ''' For each axis k ∈ {X,Y,Z}, the method determines:
        ''' <list type="bullet">
        '''   <item><description><c>ts(k)</c> — tick‑mark spacing in raw units</description></item>
        '''   <item><description><c>ft(k)</c> — location of the first tick mark</description></item>
        '''   <item><description><c>tx(k)</c> — number of tick marks</description></item>
        ''' </list>
        ''' 
        ''' It then converts these into normalized coordinates:
        ''' <list type="bullet">
        '''   <item><description><c>ftn(k)</c> — normalized first tick location</description></item>
        '''   <item><description><c>tsn(k)</c> — normalized tick spacing</description></item>
        ''' </list>
        ''' 
        ''' These values are used by <c>tick_marks_data()</c> to compute the final
        ''' 2D projected tick‑mark coordinates.
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>raw_mins_()</c>, <c>raw_ranges_()</c></description></item>
        '''   <item><description><c>ChartScaling</c> (indirectly, for axis range logic)</description></item>
        ''' </list>
        ''' </summary>
        Private Sub tick_marks_prerequisites()
            Dim fract() As Double = {raw_ranges_(0) / 5.0, raw_ranges_(1) / 5.0, raw_ranges_(2) / 5.0}

            For i = 0 To 2
                Dim nDig As Integer = 1 - Int(Math.Log10(Math.Abs(fract(i))) + 1)
                ts(i) = Math.Round(fract(i), nDig)      'ts tick mark step width
                ft(i) = ts(i) * (Int(raw_mins_(i) / ts(i)) + 1)                 'ft location first tick mark
                tx(i) = Int((raw_ranges_(i) + raw_mins_(i) - ft(i)) / ts(i))    'tx number of tick marks
                'Normalize to [-0.5, +0.5] and apply axis direction.
                ftn(i) = ((ft(i) - raw_mins_(i)) / raw_ranges_(i) - 0.5) * axis_dir_(i)  'first tick (normalized)
                tsn(i) = (ts(i) / raw_ranges_(i)) * axis_dir_(i)                         'tick step (normalized)
            Next
        End Sub

        ''' <summary>
        ''' Computes the rotation vectors used for 3D → 2D projection.
        ''' 
        ''' The method:
        ''' <list type="number">
        '''   <item><description>Constructs rotation matrices for X‑rotation and Z‑rotation.</description></item>
        '''   <item><description>Combines them into two projection vectors <c>rot_1</c> and <c>rot_2</c>.</description></item>
        '''   <item><description>These vectors define the mapping:
        '''     <code>
        '''     x2D = rot_1 ⋅ (x, y, z)
        '''     y2D = rot_2 ⋅ (x, y, z)
        '''     </code>
        '''   </description></item>
        ''' </list>
        ''' 
        ''' The resulting vectors are used by all geometry‑generating methods
        ''' (gridlines, cage, tick marks, cut‑planes, and data projection).
        ''' </summary>
        Private Sub get_rotations()
            'rotation matrix
            Dim sin_a() As Double = {Math.Sin(Radians(x_rotate)), Math.Sin(Radians(y_rotate)), Math.Sin(Radians(z_rotate))}
            Dim cos_a() As Double = {Math.Cos(Radians(x_rotate)), Math.Cos(Radians(y_rotate)), Math.Cos(Radians(z_rotate))}

            rot_1(0) = x_scale_ratio * cos_a(1) * cos_a(2)
            rot_1(1) = -y_scale_ratio * cos_a(1) * sin_a(2)
            rot_1(2) = z_scale_ratio * sin_a(1)
            rot_2(0) = x_scale_ratio * (cos_a(2) * sin_a(1) * sin_a(0) + sin_a(2) * cos_a(0))
            rot_2(1) = y_scale_ratio * (-sin_a(2) * sin_a(1) * sin_a(0) + cos_a(2) * cos_a(0))
            rot_2(2) = -z_scale_ratio * sin_a(0) * cos_a(1)
        End Sub

        ''' <summary>
        ''' Computes the 3D bounding‑box (“cage”) geometry and projects it into 2D
        ''' coordinates for Excel plotting.
        ''' 
        ''' The cage consists of:
        ''' <list type="bullet">
        '''   <item><description>12 edges of the unit cube</description></item>
        '''   <item><description>Axis endpoints used for axis labeling</description></item>
        ''' </list>
        ''' 
        ''' The method:
        ''' <list type="number">
        '''   <item><description>Defines cube vertices in normalized 3D space.</description></item>
        '''   <item><description>Applies axis scaling (if enabled).</description></item>
        '''   <item><description>Applies rotation matrices.</description></item>
        '''   <item><description>Applies zoom and XY shifts.</description></item>
        '''   <item><description>Stores final 2D coordinates in <c>xs_cage</c> and <c>ys_cage</c>.</description></item>
        ''' </list>
        ''' 
        ''' Axis labels (X, Y, Z) are attached to specific cage vertices during
        ''' chart rendering.
        ''' </summary>
        Private Sub cage_data()
            Dim unscaled_x(10) As Double, unscaled_y(10) As Double, unscaled_z(10) As Double

            'Keep cage fixed (do not move when axis is flipped)
            Dim xH As Double = 0.5
            Dim xL As Double = -0.5
            Dim yH As Double = 0.5
            Dim yL As Double = -0.5
            Dim zH As Double = 0.5
            Dim zL As Double = -0.5

            'constants based on 0/0/0 coordinate and unity scaling
            unscaled_x = {xH, xL, xL, xH, xH, xH, xL, xL, xL, xL, xL}
            unscaled_y = {yL, yL, yL, yL, yL, yH, yH, yH, yL, yL, yH}
            unscaled_z = {zL, zL, zH, zH, zL, zL, zL, zH, zH, zL, zL}

            For i = 0 To 10
                xs_cage(i) = (unscaled_x(i) * rot_1(0) + unscaled_y(i) * rot_1(1) + unscaled_z(i) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_cage(i) = (unscaled_x(i) * rot_2(0) + unscaled_y(i) * rot_2(1) + unscaled_z(i) * rot_2(2) + y_shift_internal) * zoom_internal
            Next
        End Sub

        ''' <summary>
        ''' Computes the internal zoom factor used during 3D → 2D projection.  
        ''' 
        ''' The external zoom value <paramref name="z"/> is mapped to an internal
        ''' scaling coefficient that controls how strongly projected coordinates are
        ''' expanded or contracted around the chart center.
        ''' 
        ''' The transformation is nonlinear, providing smooth zoom behavior even for
        ''' large zoom values.  
        ''' 
        ''' The returned value is used by:
        ''' <list type="bullet">
        '''   <item><description><c>normalizedData()</c> — to scale projected points</description></item>
        '''   <item><description><c>cage_data()</c> — to scale bounding‑box edges</description></item>
        '''   <item><description><c>get_gridlines()</c> — to scale gridline geometry</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="z">User‑specified zoom level.</param>
        ''' <returns>Internal zoom coefficient used for projection scaling.</returns>
        Private Function comp_zoom_inter(z As Double) As Double
            Return (1.0 + z / 50.0) ^ 2
        End Function

        ''' <summary>
        ''' Computes the internal X‑shift coefficient used during 3D → 2D projection.
        ''' 
        ''' The user‑supplied horizontal shift <paramref name="x"/> is transformed into
        ''' an internal offset that is applied after rotation and zoom, ensuring that
        ''' the apparent movement of the 3D scene is smooth and visually consistent
        ''' across different zoom levels.
        ''' 
        ''' The transformation is nonlinear, preventing excessive drift at large shift
        ''' values and maintaining stable behavior when zoom is small.
        ''' 
        ''' The returned value is used by:
        ''' <list type="bullet">
        '''   <item><description><c>normalizedData()</c> — to shift projected data points</description></item>
        '''   <item><description><c>cage_data()</c> — to shift the bounding box</description></item>
        '''   <item><description><c>get_gridlines()</c> — to shift gridline geometry</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="x">User‑specified horizontal shift.</param>
        ''' <returns>Internal X‑shift coefficient used in projection.</returns>
        Private Function comp_x_shift_inter(x As Double) As Double
            Return x / 100.0 - 0.5
        End Function

        ''' <summary>
        ''' Computes the internal Y‑shift coefficient used during 3D → 2D projection.
        ''' 
        ''' The user‑supplied vertical shift <paramref name="y"/> is converted into an
        ''' internal offset that is applied after rotation and zoom.  
        ''' 
        ''' This mapping is nonlinear to ensure:
        ''' <list type="bullet">
        '''   <item><description>Stable movement at low zoom levels</description></item>
        '''   <item><description>Controlled drift at high zoom levels</description></item>
        '''   <item><description>Consistent visual behavior across different axis scales</description></item>
        ''' </list>
        ''' 
        ''' The returned value is used by:
        ''' <list type="bullet">
        '''   <item><description><c>normalizedData()</c> — to shift projected points vertically</description></item>
        '''   <item><description><c>cage_data()</c> — to shift the bounding box</description></item>
        '''   <item><description><c>get_gridlines()</c> — to shift gridline geometry</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="y">User‑specified vertical shift.</param>
        ''' <returns>Internal Y‑shift coefficient used in projection.</returns>
        Private Function comp_y_shift_inter(y As Double) As Double
            Return y / 100.0 + 0.5
        End Function

        ' ---------------------------------------------------------------------
        ' Projection helpers
        ' ---------------------------------------------------------------------
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

        '==================== Axis bounds helpers (include 3D objects) ====================

        ''' <summary>
        ''' Recomputes raw_mins_ and raw_ranges_ from the raw XYZ data only.
        ''' </summary>
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

        ''' <summary>
        ''' Expands the current raw axis bounds to include any attached 3D objects (sphere / ellipsoid).
        ''' </summary>
        Private Sub ExpandRawBoundsToIncludeObjects()
            'Need data bounds first
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

        ''' <summary>
        ''' Recomputes axis bounds from data and then expands them to include all objects.
        ''' Call this after data inputs change or object list changes.
        ''' </summary>
        Private Sub RecomputeAxisBoundsIncludingObjects()
            RecomputeRawBoundsFromDataOnly()
            ExpandRawBoundsToIncludeObjects()
            ApplyManualAxisBoundsIfAny()
        End Sub

        Private Sub ApplyManualAxisBoundsIfAny()
            'Only apply if we have any axis limits set; otherwise do nothing.
            For axis As Integer = 0 To 2
                If manualMin_(axis).HasValue OrElse manualMax_(axis).HasValue Then
                    'Require both min+max if either is provided
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

        ''' <summary>
        ''' Validates a numeric axis min/max pair.
        ''' </summary>
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
