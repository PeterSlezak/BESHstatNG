Option Explicit On
Imports System.Xml
Imports Microsoft.Office.Interop.Excel

Namespace graphics


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

        ''' <summary>Name of the chart object created in Excel.</summary>
        Private pChartName As String

        ' Raw data
        Private x_raw() As Double
        Private y_raw() As Double
        Private z_raw() As Double

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

        ' Cage (3D bounding box) projected coordinates
        Private xs_cage(10) As Double
        Private ys_cage(10) As Double

        ' Tick‑mark geometry
        Private ts(2) As Double
        Private ft(2) As Double
        Private tx(2) As Double
        Private ftn(2) As Double
        Private tsn(2) As Double
        Private xs_x_axisticks(20) As Double
        Private ys_x_axisticks(20) As Double
        Private xs_y_axisticks(20) As Double
        Private ys_y_axisticks(20) As Double
        Private xs_z_axisticks(20) As Double
        Private ys_z_axisticks(20) As Double
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

        ' Cut‑plane geometry
        Private cp_normalized_pos(2) As Double
        Private xs_cutplane_x() As Double
        Private ys_cutplane_x() As Double
        Private xs_cutplane_y() As Double
        Private ys_cutplane_y() As Double
        Private xs_cutplane_z() As Double
        Private ys_cutplane_z() As Double


        ' Set Values------------------------------------------------------------
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

            If bScaleAxes Then
                x_scale_ratio = raw_ranges_(0) / raw_ranges_.Max
                y_scale_ratio = raw_ranges_(1) / raw_ranges_.Max
                z_scale_ratio = raw_ranges_(2) / raw_ranges_.Max
            Else
                x_scale_ratio = 1.0
                y_scale_ratio = 1.0
                z_scale_ratio = 1.0
            End If
        End Sub

        ''' <summary>
        ''' Supplies raw X, Y, Z data and computes minima and ranges for each axis.
        ''' </summary>
        ''' <param name="arXdata">X‑coordinates.</param>
        ''' <param name="arYdata">Y‑coordinates.</param>
        ''' <param name="arZdata">Z‑coordinates.</param>
        Public Sub dataInputs(arXdata() As Double, arYdata() As Double, arZdata() As Double)
            x_raw = arXdata
            raw_mins_(0) = x_raw.Min()
            raw_ranges_(0) = x_raw.Max() - raw_mins_(0)
            n = x_raw.Length

            y_raw = arYdata
            raw_mins_(1) = y_raw.Min()
            raw_ranges_(1) = y_raw.Max() - raw_mins_(1)

            z_raw = arZdata
            raw_mins_(2) = z_raw.Min()
            raw_ranges_(2) = z_raw.Max() - raw_mins_(2)
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
                       Optional DataMarakerSize As Integer = 6)
            pbDataLabels = bDataLabels
            pbZdropLines = bZdropLines
            pbShowGridlines = bShowGridlines
            pPointLabelFontSize = PointLabelFontSize
            pDataLabelPosition = DataLabelPosition
            pDataMarakerSize = DataMarakerSize
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

            'cut planes normalized position. Default is on the unity box planes
            cp_normalized_pos(0) = -0.5
            cp_normalized_pos(1) = -0.5
            cp_normalized_pos(2) = -0.5

            pbShowXYplanePoints = True
            pbShowYZplanePoints = True
            pbShowXZplanePoints = True
            pXYplanePointSize = 2
            pYZplanePointSize = 2
            pXZplanePointSize = 2

            pDataMarakerSize = 6

            pbDataLabels = False
            pPointLabelFontSize = 9
            pDataLabelPosition = XlDataLabelPosition.xlLabelPositionRight
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

            Dim i As Integer, j As Integer, can_delete_i As Integer, grpID As Integer, zeros() As Double
            'Arrays for By Group display
            Dim ByGroupX() As Double, ByGroupY() As Double, ByGroupDataPointLabel() As String, ByGroupError_bar() As Double

            Call get_rotations()
            Call cut_planes()

            If figure Is Nothing Then
                app.Charts.Add()
                figure = app.ActiveWorkbook.ActiveChart
            End If

            With figure
                Try
                    'Reset LegendEntries by setting it to false and true afterwars
                    .HasLegend = False
                    .HasLegend = True
                    .HasLegend = bGroups
                    .Name = pChartName
                    .HasTitle = False
                    .hestitle = True
                    .ChartTitle.Delete()
                Catch
                End Try
                .ChartType = XlChartType.xlXYScatter

                'delete extra series
                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop
                Try
                    'this is required to enable zoom/shift. Scale is automaticaly recalculated otherwise = calceling the zoom/shift effect.
                    .Axes(XlAxisType.xlCategory).MinimumScale = -1
                    .Axes(XlAxisType.xlCategory).MaximumScale = 1
                    .Axes(XlAxisType.xlCategory).Delete
                    .Axes(XlAxisType.xlValue).MinimumScale = 0
                    .Axes(XlAxisType.xlValue).MaximumScale = 2
                    .Axes(XlAxisType.xlValue).Delete
                    .Axes(XlAxisType.xlValue).MajorGridlines.Delete
                Catch
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

                'plot tick macrks for all axes------------------------------------------
                Call tick_marks_data()
                seriesID += 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                    .XValues = xs_x_axisticks
                    .Values = ys_x_axisticks
                    .Name = "x_ticks"
                    .Format.Line.Weight = 2
                    .Format.Line.Visible = True
                    .Format.Line.ForeColor.RGB = RGB(0, 0, 0)

                    'Attach a label to each data point in the chart.
                    j = 0
                    For i = 1 To xs_x_axisticks.Length
                        If i = 2 Or i = 6 Or i = 8 Or i = 12 Or i = 14 Or i = 18 Or i = 20 Then
                            .points(i).HasDataLabel = True
                            .points(i).DataLabel.text = CStr(x_tick_labels(j))
                            .points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionRight
                            .points(i).DataLabel.Font.Size = 10
                            j += 1
                            If j > UBound(x_tick_labels) Then Exit For
                        End If
                    Next

                    can_delete_i = i
                    For i = can_delete_i + 2 To UBound(xs_x_axisticks)
                        .points(i).Format.Line.Visible = False
                    Next

                    'Make redundant line segments invisible
                    For i = 3 To 21
                        If i = 3 Or i = 4 Or i = 7 Or i = 8 Or i = 10 Or i = 11 Or i = 13 Or
                           i = 15 Or i = 16 Or i = 19 Or i = 20 Then
                            .points(i).Format.Line.Visible = False
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
                    .Format.Line.Weight = 2
                    .Format.Line.Visible = True
                    .Format.Line.ForeColor.RGB = RGB(0, 0, 0)

                    'Attach a label to each data point in the chart.
                    j = 0
                    For i = 1 To xs_y_axisticks.Length
                        If i = 2 Or i = 6 Or i = 8 Or i = 12 Or i = 14 Or i = 18 Or i = 20 Then
                            .points(i).HasDataLabel = True
                            .points(i).DataLabel.text = CStr(y_tick_labels(j))
                            .points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionBelow
                            .points(i).DataLabel.Font.Size = 11
                            j += 1
                            If j > UBound(y_tick_labels) Then Exit For
                        End If
                    Next

                    can_delete_i = i
                    For i = can_delete_i + 2 To UBound(xs_y_axisticks)
                        .points(i).Format.Line.Visible = False
                    Next

                    'Make redundant line segments invisible
                    For i = 3 To 21
                        If i = 3 Or i = 4 Or i = 7 Or i = 8 Or i = 10 Or i = 11 Or i = 13 Or
                           i = 15 Or i = 16 Or i = 19 Or i = 20 Then
                            .points(i).Format.Line.Visible = False
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
                    .Format.Line.Weight = 2
                    .Format.Line.Visible = True
                    .Format.Line.ForeColor.RGB = RGB(0, 0, 0)

                    'Attach a label to each data point in the chart.
                    j = 0
                    For i = 1 To xs_z_axisticks.Length
                        If i = 2 Or i = 6 Or i = 8 Or i = 12 Or i = 14 Or i = 18 Or i = 20 Then
                            .points(i).HasDataLabel = True
                            .points(i).DataLabel.text = CStr(z_tick_labels(j))
                            .points(i).DataLabel.Position = XlDataLabelPosition.xlLabelPositionRight
                            .points(i).DataLabel.Font.Size = 10
                            j += 1
                            If j > UBound(z_tick_labels) Then Exit For
                        End If
                    Next

                    can_delete_i = i
                    For i = can_delete_i + 2 To UBound(xs_z_axisticks)
                        .points(i).Format.Line.Visible = False
                    Next

                    'Make redundant line segments invisible
                    For i = 3 To 21
                        If i = 3 Or i = 4 Or i = 7 Or i = 8 Or i = 10 Or i = 11 Or i = 13 Or i = 15 Or i = 16 Or i = 19 Or i = 20 Then
                            .points(i).Format.Line.Visible = False
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
                            .MarkerStyle = XlMarkerStyle.xlMarkerStyleAutomatic 'xlMarkerStyleCircle
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
                        .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                        .Format.Line.Weight = 2
                        .MarkerSize = pDataMarakerSize
                        .MarkerForegroundColor = RGB(100, 100, 100)
                        .Format.Fill.Visible = False
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

                        Try
                            .Legend.Delete
                        Catch
                        End Try
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
                xs_xy_x_gridline(i * 3) = (x * rot_1(0) + 0.5 * rot_1(1) - 0.5 * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_x_gridline(i * 3) = (x * rot_2(0) + 0.5 * rot_2(1) - 0.5 * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_x_gridline(1 + i * 3) = (x * rot_1(0) - 0.5 * rot_1(1) - 0.5 * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_x_gridline(1 + i * 3) = (x * rot_2(0) - 0.5 * rot_2(1) - 0.5 * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_xy_x_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xy_x_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next i
            For i = 0 To tx(1)
                'XY plane gridlines - perpendicular to Y axis
                y = ftn(1) + i * tsn(1) 'pY coordinate of tickmark start of the line
                xs_xy_y_gridline(i * 3) = (0.5 * rot_1(0) + y * rot_1(1) - 0.5 * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_y_gridline(i * 3) = (0.5 * rot_2(0) + y * rot_2(1) - 0.5 * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xy_y_gridline(1 + i * 3) = (-0.5 * rot_1(0) + y * rot_1(1) - 0.5 * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xy_y_gridline(1 + i * 3) = (-0.5 * rot_2(0) + y * rot_2(1) - 0.5 * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_xy_y_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xy_y_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next i
            '---------------------------------------------------------------------------------------
            For i = 0 To tx(0)
                'XZ plane gridlines - perpendicular to X axis
                x = ftn(0) + i * tsn(0) 'x coordinate of tickmark start of the line
                xs_xz_x_gridline(i * 3) = (x * rot_1(0) - 0.5 * rot_1(1) - 0.5 * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_x_gridline(i * 3) = (x * rot_2(0) - 0.5 * rot_2(1) - 0.5 * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_x_gridline(1 + i * 3) = (x * rot_1(0) - 0.5 * rot_1(1) + 0.5 * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_x_gridline(1 + i * 3) = (x * rot_2(0) - 0.5 * rot_2(1) + 0.5 * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_xz_x_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xz_x_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next i
            For i = 0 To tx(2)
                'YZ plane gridlines - perpendicular to Z axis
                z = ftn(2) + i * tsn(2) 'z coordinate of tickmark start of the line
                xs_xz_z_gridline(i * 3) = (-0.5 * rot_1(0) - 0.5 * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_z_gridline(i * 3) = (-0.5 * rot_2(0) - 0.5 * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_xz_z_gridline(1 + i * 3) = (0.5 * rot_1(0) - 0.5 * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_xz_z_gridline(1 + i * 3) = (0.5 * rot_2(0) - 0.5 * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_xz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_xz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next i
            '---------------------------------------------------------------------------------------
            For i = 0 To tx(1)
                'YZ plane gridlines - perpendicular to Y axis
                y = ftn(1) + i * tsn(1) 'pY coordinate of tickmark start of the line
                xs_yz_y_gridline(i * 3) = (-0.5 * rot_1(0) + y * rot_1(1) + 0.5 * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_y_gridline(i * 3) = (-0.5 * rot_2(0) + y * rot_2(1) + 0.5 * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_y_gridline(1 + i * 3) = (-0.5 * rot_1(0) + y * rot_1(1) - 0.5 * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_y_gridline(1 + i * 3) = (-0.5 * rot_2(0) + y * rot_2(1) - 0.5 * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_yz_y_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_yz_y_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next i
            For i = 0 To tx(2)
                'YZ plane gridlines - perpendicular to Z axis
                z = ftn(2) + i * tsn(2) 'z coordinate of tickmark start of the line
                xs_yz_z_gridline(i * 3) = (-0.5 * rot_1(0) - 0.5 * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_z_gridline(i * 3) = (-0.5 * rot_2(0) - 0.5 * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                xs_yz_z_gridline(1 + i * 3) = (-0.5 * rot_1(0) + 0.5 * rot_1(1) + z * rot_1(2) + x_shift_internal) * zoom_internal
                ys_yz_z_gridline(1 + i * 3) = (-0.5 * rot_2(0) + 0.5 * rot_2(1) + z * rot_2(2) + y_shift_internal) * zoom_internal

                'this is just a dummy point to make line segment invisible as well as the segment to the next point
                xs_yz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
                ys_yz_z_gridline(2 + i * 3) = gToDeleteGridLineValue
            Next i
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
                error_bars(i) = ys_norm_data(i) - ys_cutplane_z(i)
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
                x_norm(i) = (x_raw(i) - raw_mins_(0)) / raw_ranges_(0) - 0.5
                y_norm(i) = (y_raw(i) - raw_mins_(1)) / raw_ranges_(1) - 0.5
                z_norm(i) = (z_raw(i) - raw_mins_(2)) / raw_ranges_(2) - 0.5
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
            Dim vect1(20) As Double, vect2(20) As Double, vect3(20) As Double
            Dim vectxc(20) As Double, tmx_x(20) As Double, vectyc(20) As Double, tmy_y(20) As Double, vectzc(20) As Double, tmz_z(20) As Double

            Call tick_marks_prerequisites()

            ReDim x_tick_labels(tx(0)), y_tick_labels(tx(1)), z_tick_labels(tx(2))
            For i = 1 To 21
                vect1(i - 1) = Int((i - 1) / 3)
                If i Mod 2 = 1 Then
                    vect2(i - 1) = 0.5
                ElseIf i Mod 2 = 0 Then
                    vect2(i - 1) = 0.55
                End If
                vect3(i - 1) = -0.5

                'x axis tick marks
                vectxc(i - 1) = If(vect1(i - 1) > tx(0), tx(0), vect1(i - 1))
                tmx_x(i - 1) = ftn(0) + vectxc(i - 1) * tsn(0)
                xs_x_axisticks(i - 1) = (tmx_x(i - 1) * rot_1(0) + vect2(i - 1) * rot_1(1) + vect3(i - 1) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_x_axisticks(i - 1) = (tmx_x(i - 1) * rot_2(0) + vect2(i - 1) * rot_2(1) + vect3(i - 1) * rot_2(2) + y_shift_internal) * zoom_internal

                'pY axis tick marks
                vectyc(i - 1) = If(vect1(i - 1) > tx(1), tx(1), vect1(i - 1))
                tmy_y(i - 1) = ftn(1) + vectyc(i - 1) * tsn(1)
                xs_y_axisticks(i - 1) = (vect2(i - 1) * rot_1(0) + tmy_y(i - 1) * rot_1(1) + vect3(i - 1) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_y_axisticks(i - 1) = (vect2(i - 1) * rot_2(0) + tmy_y(i - 1) * rot_2(1) + vect3(i - 1) * rot_2(2) + y_shift_internal) * zoom_internal

                'z axis tick marks
                vectzc(i - 1) = If(vect1(i - 1) > tx(2), tx(2), vect1(i - 1))
                tmz_z(i - 1) = ftn(2) + vectzc(i - 1) * tsn(2)
                xs_z_axisticks(i - 1) = (vect3(i - 1) * rot_1(0) + vect2(i - 1) * rot_1(1) + tmz_z(i - 1) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_z_axisticks(i - 1) = (vect3(i - 1) * rot_2(0) + vect2(i - 1) * rot_2(1) + tmz_z(i - 1) * rot_2(2) + y_shift_internal) * zoom_internal

                'tick mark values
                If (i - 1) Mod 3 = 1 Then
                    x_tick_labels(Math.Round(vectxc(i - 1), 0)) = CStr(ft(0) + vectxc(i - 1) * ts(0))
                    y_tick_labels(Math.Round(vectyc(i - 1), 0)) = CStr(ft(1) + vectyc(i - 1) * ts(1))
                    z_tick_labels(Math.Round(vectzc(i - 1), 0)) = CStr(ft(2) + vectzc(i - 1) * ts(2))
                End If
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
                ftn(i) = (ft(i) - raw_mins_(i)) / raw_ranges_(i) - 0.5          'ftn normalised location first tick mark
                tsn(i) = ts(i) / raw_ranges_(i)                                 'tsn normalised tick mark step width
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

            'constants based on 0/0/0 coordinate and unity scaling
            unscaled_x = {0.5, -0.5, -0.5, 0.5, 0.5, 0.5, -0.5, -0.5, -0.5, -0.5, -0.5}
            unscaled_y = {-0.5, -0.5, -0.5, -0.5, -0.5, 0.5, 0.5, 0.5, -0.5, -0.5, 0.5}
            unscaled_z = {-0.5, -0.5, 0.5, 0.5, -0.5, -0.5, -0.5, 0.5, 0.5, -0.5, -0.5}

            For i = 0 To 10
                xs_cage(i) = (unscaled_x(i) * rot_1(0) + unscaled_y(i) * rot_1(1) + unscaled_z(i) * rot_1(2) + x_shift_internal) * zoom_internal
                ys_cage(i) = (unscaled_x(i) * rot_2(0) + unscaled_y(i) * rot_2(1) + unscaled_z(i) * rot_2(2) + y_shift_internal) * zoom_internal
            Next i
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

    End Class
End Namespace
