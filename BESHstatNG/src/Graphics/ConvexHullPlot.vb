Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Controls which visual attributes vary automatically between grouping levels.
''' </summary>
<Flags>
Public Enum ConvexHullGroupStyleMode
    None = 0
    Color = 1
    Marker = 2
    LineStyle = 4
    ColorAndMarker = Color Or Marker
    ColorAndLineStyle = Color Or LineStyle
    MarkerAndLineStyle = Marker Or LineStyle
    ColorMarkerAndLineStyle = Color Or Marker Or LineStyle
End Enum

''' <summary>
''' Windows compatibility forwarding layer for the portable convex-hull calculations.
''' Geometry, options and result DTOs are owned by BESHStat.Core.
''' </summary>
Public Module ConvexHullPlot
    ''' <summary>Computes an ungrouped or optionally grouped convex hull.</summary>
    Public Function Compute(xValues As Double(),
                            yValues As Double(),
                            Optional options As ConvexHullPlotOptions = Nothing,
                            Optional groupingValues As Array = Nothing) As ConvexHullPlotResult
        Return ConvexHullPlotCore.Compute(xValues, yValues, options, groupingValues)
    End Function

    ''' <summary>Computes grouped convex hulls, preserving the original argument order.</summary>
    Public Function ComputeGrouped(xValues As Double(),
                                   yValues As Double(),
                                   groupingValues As Array,
                                   Optional options As ConvexHullPlotOptions = Nothing) As ConvexHullPlotResult
        Return ConvexHullPlotCore.ComputeGrouped(xValues, yValues, groupingValues, options)
    End Function
End Module

''' <summary>
''' Optional per-group display override. Set only the properties that should differ from the plot defaults.
''' </summary>
Public Class ConvexHullGroupAppearance
    Public Property GroupName As String
    Public Property MarkerStyle As Nullable(Of XlMarkerStyle)
    Public Property MarkerSize As Nullable(Of Integer)
    Public Property MarkerForegroundColor As Nullable(Of Integer)
    Public Property MarkerBackgroundColor As Nullable(Of Integer)
    Public Property HullLineColor As Nullable(Of Integer)
    Public Property HullLineWeight As Nullable(Of Single)
    Public Property HullLineStyle As Nullable(Of XlLineStyle)
End Class

''' <summary>
''' Excel chart appearance settings for <see cref="ConvexHullPlotExcel"/>.
''' </summary>
''' <remarks>Colors are OLE RGB integers used by the Excel object model.</remarks>
Public Class ConvexHullPlotAppearance
    Public Property ChartTitle As String = "2D convex hull plot"
    Public Property XAxisTitle As String = "X"
    Public Property YAxisTitle As String = "Y"
    Public Property SeriesName As String = "Data"

    Public Property ShowPoints As Boolean = True
    Public Property ShowHullLine As Boolean = True
    Public Property ShowLegend As Boolean = False
    Public Property ShowGroupLegend As Boolean = True
    Public Property ShowMajorGridlines As Boolean = True

    Public Property GroupStyleMode As ConvexHullGroupStyleMode = ConvexHullGroupStyleMode.ColorAndMarker

    Public Property MarkerStyle As XlMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
    Public Property MarkerSize As Integer = 6
    Public Property MarkerForegroundColor As Integer = &HB4771F
    Public Property MarkerBackgroundColor As Integer = &HB4771F

    Public Property HullLineColor As Integer = &HB4771F
    Public Property HullLineWeight As Single = 1.5F
    Public Property HullLineStyle As XlLineStyle = XlLineStyle.xlContinuous

    Public Property GroupColors As Integer() = {
        &HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794,
        &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17
    }

    Public Property GroupMarkerStyles As XlMarkerStyle() = {
        XlMarkerStyle.xlMarkerStyleCircle,
        XlMarkerStyle.xlMarkerStyleSquare,
        XlMarkerStyle.xlMarkerStyleTriangle,
        XlMarkerStyle.xlMarkerStyleDiamond,
        XlMarkerStyle.xlMarkerStyleX,
        XlMarkerStyle.xlMarkerStylePlus,
        XlMarkerStyle.xlMarkerStyleStar,
        XlMarkerStyle.xlMarkerStyleDash
    }

    Public Property GroupLineStyles As XlLineStyle() = {
        XlLineStyle.xlContinuous,
        XlLineStyle.xlDash,
        XlLineStyle.xlDot,
        XlLineStyle.xlDashDot,
        XlLineStyle.xlDashDotDot
    }

    ''' <summary>
    ''' Gets or sets optional group-specific overrides matched case-insensitively by displayed group name.
    ''' </summary>
    Public Property GroupOverrides As ConvexHullGroupAppearance() = New ConvexHullGroupAppearance() {}

    Public Property BackgroundColor As Integer = &HFFFFFF
    Public Property GridlineColor As Integer = &HE6E6E6

    Public Property XAxisMinimum As Nullable(Of Double)
    Public Property XAxisMaximum As Nullable(Of Double)
    Public Property YAxisMinimum As Nullable(Of Double)
    Public Property YAxisMaximum As Nullable(Of Double)
End Class

''' <summary>
''' Renders a <see cref="ConvexHullPlotResult"/> as an embedded Excel XY-scatter chart.
''' </summary>
Public NotInheritable Class ConvexHullPlotExcel
    Private Sub New()
    End Sub

    Private NotInheritable Class ResolvedGroupStyle
        Friend MarkerStyle As XlMarkerStyle
        Friend MarkerSize As Integer
        Friend MarkerForegroundColor As Integer
        Friend MarkerBackgroundColor As Integer
        Friend HullLineColor As Integer
        Friend HullLineWeight As Single
        Friend HullLineStyle As XlLineStyle
    End Class

    Public Shared Function AddChart(ws As Worksheet,
                                    result As ConvexHullPlotResult,
                                    Optional appearance As ConvexHullPlotAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 620.0R,
                                    Optional height As Double = 420.0R) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If Not IsFinite(left) OrElse Not IsFinite(top) Then
            Throw New ArgumentOutOfRangeException(NameOf(left), "Chart position must be finite.")
        End If
        If Not IsFinitePositive(width) OrElse Not IsFinitePositive(height) Then
            Throw New ArgumentOutOfRangeException(NameOf(width), "Chart width and height must be finite and positive.")
        End If

        Dim resolvedAppearance As ConvexHullPlotAppearance = If(appearance, New ConvexHullPlotAppearance())
        ValidateAppearance(resolvedAppearance, result.HasGrouping)

        Dim chartShape As Shape = Nothing
        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlXYScatter, left, top, width, height)
            Dim chart As Chart = chartShape.Chart
            chart.ChartType = XlChartType.xlXYScatter
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)
            ConfigureBackground(chart, resolvedAppearance)
            ConfigureTitle(chart, resolvedAppearance.ChartTitle)

            Dim legendSeriesIndices As New List(Of Integer)()
            Dim groups As ConvexHullPlotGroup() = result.Groups
            For groupIndex As Integer = 0 To groups.Length - 1
                Dim group As ConvexHullPlotGroup = groups(groupIndex)
                Dim style As ResolvedGroupStyle = ResolveStyle(group, groupIndex, result.HasGrouping, resolvedAppearance)
                Dim seriesName As String = ResolveSeriesName(group, groupIndex, result.HasGrouping, resolvedAppearance)

                If resolvedAppearance.ShowPoints Then
                    legendSeriesIndices.Add(AddMarkerSeries(seriesCollection, group, seriesName, style))
                End If

                If resolvedAppearance.ShowHullLine AndAlso group.HullVertices.Length >= 2 Then
                    Dim hullIndex As Integer = AddHullSeries(seriesCollection, group, seriesName & " hull", style)
                    If Not resolvedAppearance.ShowPoints Then legendSeriesIndices.Add(hullIndex)
                End If
            Next

            If seriesCollection.Count = 0 Then
                Throw New InvalidOperationException("No chart series can be rendered with the current data and display settings.")
            End If

            ConfigureAxes(chart, resolvedAppearance)
            ConfigureLegend(chart,
                            legendSeriesIndices,
                            resolvedAppearance.ShowLegend OrElse (result.HasGrouping AndAlso resolvedAppearance.ShowGroupLegend))
            chart.Refresh()
            Return chart
        Catch
            If chartShape IsNot Nothing Then
                Try
                    chartShape.Delete()
                Catch
                End Try
            End If
            Throw
        End Try
    End Function

    Private Shared Sub ValidateAppearance(appearance As ConvexHullPlotAppearance, hasGrouping As Boolean)
        If Not appearance.ShowPoints AndAlso Not appearance.ShowHullLine Then
            Throw New ArgumentException("At least one of ShowPoints or ShowHullLine must be enabled.", NameOf(appearance))
        End If
        If Not [Enum].IsDefined(GetType(ConvexHullGroupStyleMode), appearance.GroupStyleMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.GroupStyleMode), "The group style mode is not defined.")
        End If
        If appearance.MarkerSize < 2 OrElse appearance.MarkerSize > 72 Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.MarkerSize), "Marker size must be between 2 and 72 points.")
        End If
        If Not IsFinitePositive(appearance.HullLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.HullLineWeight), "Hull line weight must be finite and positive.")
        End If
        If appearance.ShowPoints AndAlso appearance.MarkerStyle = XlMarkerStyle.xlMarkerStyleNone Then
            Throw New ArgumentException("MarkerStyle must be visible when ShowPoints is enabled.", NameOf(appearance.MarkerStyle))
        End If
        If appearance.ShowHullLine AndAlso appearance.HullLineStyle = XlLineStyle.xlLineStyleNone Then
            Throw New ArgumentException("HullLineStyle must be visible when ShowHullLine is enabled.", NameOf(appearance.HullLineStyle))
        End If
        If hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.Color) <> 0 AndAlso
           (appearance.GroupColors Is Nothing OrElse appearance.GroupColors.Length = 0) Then
            Throw New ArgumentException("GroupColors must contain at least one color.", NameOf(appearance.GroupColors))
        End If
        If hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.Marker) <> 0 AndAlso
           (appearance.GroupMarkerStyles Is Nothing OrElse appearance.GroupMarkerStyles.Length = 0) Then
            Throw New ArgumentException("GroupMarkerStyles must contain at least one marker style.", NameOf(appearance.GroupMarkerStyles))
        End If
        If hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.LineStyle) <> 0 AndAlso
           (appearance.GroupLineStyles Is Nothing OrElse appearance.GroupLineStyles.Length = 0) Then
            Throw New ArgumentException("GroupLineStyles must contain at least one line style.", NameOf(appearance.GroupLineStyles))
        End If

        If hasGrouping AndAlso appearance.ShowPoints AndAlso
           (appearance.GroupStyleMode And ConvexHullGroupStyleMode.Marker) <> 0 Then
            For Each marker As XlMarkerStyle In appearance.GroupMarkerStyles
                If marker = XlMarkerStyle.xlMarkerStyleNone Then
                    Throw New ArgumentException("Grouped marker styles must be visible when ShowPoints is enabled.",
                                                NameOf(appearance.GroupMarkerStyles))
                End If
            Next
        End If
        If hasGrouping AndAlso appearance.ShowHullLine AndAlso
           (appearance.GroupStyleMode And ConvexHullGroupStyleMode.LineStyle) <> 0 Then
            For Each lineStyle As XlLineStyle In appearance.GroupLineStyles
                If lineStyle = XlLineStyle.xlLineStyleNone Then
                    Throw New ArgumentException("Grouped line styles must be visible when ShowHullLine is enabled.",
                                                NameOf(appearance.GroupLineStyles))
                End If
            Next
        End If

        ValidateAxisLimits(appearance.XAxisMinimum, appearance.XAxisMaximum, "X")
        ValidateAxisLimits(appearance.YAxisMinimum, appearance.YAxisMaximum, "Y")
    End Sub

    Private Shared Sub ValidateAxisLimits(minimum As Nullable(Of Double),
                                          maximum As Nullable(Of Double),
                                          axisName As String)
        If minimum.HasValue AndAlso Not IsFinite(minimum.Value) Then
            Throw New ArgumentOutOfRangeException(axisName & "AxisMinimum", "Axis limits must be finite.")
        End If
        If maximum.HasValue AndAlso Not IsFinite(maximum.Value) Then
            Throw New ArgumentOutOfRangeException(axisName & "AxisMaximum", "Axis limits must be finite.")
        End If
        If minimum.HasValue AndAlso maximum.HasValue AndAlso minimum.Value >= maximum.Value Then
            Throw New ArgumentException(axisName & " axis minimum must be smaller than its maximum.")
        End If
    End Sub

    Private Shared Sub DeleteAllSeries(seriesCollection As SeriesCollection)
        Do While seriesCollection.Count > 0
            DirectCast(seriesCollection.Item(1), Series).Delete()
        Loop
    End Sub

    Private Shared Function AddMarkerSeries(seriesCollection As SeriesCollection,
                                            group As ConvexHullPlotGroup,
                                            seriesName As String,
                                            style As ResolvedGroupStyle) As Integer
        Dim points As ConvexHullPoint2D() = group.Points
        seriesCollection.NewSeries()
        'Dim series As Series = DirectCast(seriesCollection.Item(seriesCollection.Count), Series)
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatter
            .XValues = points.Select(Function(p) p.X).ToArray()
            .Values = points.Select(Function(p) p.Y).ToArray()
            .MarkerStyle = style.MarkerStyle
            .MarkerSize = style.MarkerSize
            .MarkerForegroundColor = style.MarkerForegroundColor
            .MarkerBackgroundColor = style.MarkerBackgroundColor
            .Format.Line.Visible = False
        End With
        Return seriesCollection.Count
    End Function

    Private Shared Function AddHullSeries(seriesCollection As SeriesCollection,
                                          group As ConvexHullPlotGroup,
                                          seriesName As String,
                                          style As ResolvedGroupStyle) As Integer
        seriesCollection.NewSeries()
        Dim series As Series = DirectCast(seriesCollection.Item(seriesCollection.Count), Series)
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatterLinesNoMarkers
            .XValues = group.ClosedHullX
            .Values = group.ClosedHullY
            .Smooth = False
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Border.Color = style.HullLineColor
            .Border.LineStyle = style.HullLineStyle
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = style.HullLineColor
            .Format.Line.Weight = style.HullLineWeight
        End With

        Return seriesCollection.Count
    End Function

    Private Shared Function ResolveStyle(group As ConvexHullPlotGroup,
                                         groupIndex As Integer,
                                         hasGrouping As Boolean,
                                         appearance As ConvexHullPlotAppearance) As ResolvedGroupStyle
        Dim varyColor As Boolean = hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.Color) <> 0
        Dim varyMarker As Boolean = hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.Marker) <> 0
        Dim varyLine As Boolean = hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.LineStyle) <> 0

        Dim color As Integer = If(varyColor,
                                  appearance.GroupColors(groupIndex Mod appearance.GroupColors.Length),
                                  appearance.HullLineColor)
        Dim markerColor As Integer = If(varyColor,
                                        appearance.GroupColors(groupIndex Mod appearance.GroupColors.Length),
                                        appearance.MarkerForegroundColor)

        Dim result As New ResolvedGroupStyle With {
            .MarkerStyle = If(varyMarker,
                              appearance.GroupMarkerStyles(groupIndex Mod appearance.GroupMarkerStyles.Length),
                              appearance.MarkerStyle),
            .MarkerSize = appearance.MarkerSize,
            .MarkerForegroundColor = markerColor,
            .MarkerBackgroundColor = If(varyColor, markerColor, appearance.MarkerBackgroundColor),
            .HullLineColor = color,
            .HullLineWeight = appearance.HullLineWeight,
            .HullLineStyle = If(varyLine,
                                appearance.GroupLineStyles(groupIndex Mod appearance.GroupLineStyles.Length),
                                appearance.HullLineStyle)
        }

        Dim overrideStyle As ConvexHullGroupAppearance = FindOverride(group.Name, appearance.GroupOverrides)
        If overrideStyle IsNot Nothing Then
            If overrideStyle.MarkerStyle.HasValue Then result.MarkerStyle = overrideStyle.MarkerStyle.Value
            If overrideStyle.MarkerSize.HasValue Then result.MarkerSize = overrideStyle.MarkerSize.Value
            If overrideStyle.MarkerForegroundColor.HasValue Then result.MarkerForegroundColor = overrideStyle.MarkerForegroundColor.Value
            If overrideStyle.MarkerBackgroundColor.HasValue Then result.MarkerBackgroundColor = overrideStyle.MarkerBackgroundColor.Value
            If overrideStyle.HullLineColor.HasValue Then result.HullLineColor = overrideStyle.HullLineColor.Value
            If overrideStyle.HullLineWeight.HasValue Then result.HullLineWeight = overrideStyle.HullLineWeight.Value
            If overrideStyle.HullLineStyle.HasValue Then result.HullLineStyle = overrideStyle.HullLineStyle.Value
        End If

        If appearance.ShowPoints AndAlso result.MarkerStyle = XlMarkerStyle.xlMarkerStyleNone Then
            Throw New ArgumentException($"Marker style override for group '{group.Name}' must be visible.", "MarkerStyle")
        End If
        If appearance.ShowHullLine AndAlso result.HullLineStyle = XlLineStyle.xlLineStyleNone Then
            Throw New ArgumentException($"Hull line style override for group '{group.Name}' must be visible.", "HullLineStyle")
        End If
        If result.MarkerSize < 2 OrElse result.MarkerSize > 72 Then
            Throw New ArgumentOutOfRangeException("MarkerSize",
                                                  $"Marker size override for group '{group.Name}' must be between 2 and 72 points.")
        End If
        If Not IsFinitePositive(result.HullLineWeight) Then
            Throw New ArgumentOutOfRangeException("HullLineWeight",
                                                  $"Hull line weight override for group '{group.Name}' must be finite and positive.")
        End If
        Return result
    End Function

    Private Shared Function FindOverride(groupName As String,
                                         ovrrides As ConvexHullGroupAppearance()) As ConvexHullGroupAppearance
        If ovrrides Is Nothing Then Return Nothing
        For Each item As ConvexHullGroupAppearance In ovrrides
            If item IsNot Nothing AndAlso
               Not String.IsNullOrWhiteSpace(item.GroupName) AndAlso
               String.Equals(item.GroupName.Trim(), groupName, StringComparison.CurrentCultureIgnoreCase) Then
                Return item
            End If
        Next
        Return Nothing
    End Function

    Private Shared Function ResolveSeriesName(group As ConvexHullPlotGroup,
                                              groupIndex As Integer,
                                              hasGrouping As Boolean,
                                              appearance As ConvexHullPlotAppearance) As String
        If hasGrouping Then
            If Not String.IsNullOrWhiteSpace(group.Name) Then Return group.Name
            Return "Group " & (groupIndex + 1).ToString(CultureInfo.CurrentCulture)
        End If
        Return If(String.IsNullOrWhiteSpace(appearance.SeriesName), "Data", appearance.SeriesName.Trim())
    End Function

    Private Shared Sub ConfigureTitle(chart As Chart, title As String)
        If String.IsNullOrWhiteSpace(title) Then
            chart.HasTitle = False
        Else
            chart.HasTitle = True
            chart.ChartTitle.Text = title.Trim()
        End If
    End Sub

    Private Shared Sub ConfigureBackground(chart As Object, appearance As ConvexHullPlotAppearance)
        chart.ChartArea.Format.Fill.Visible = True
        chart.ChartArea.Format.Fill.Solid()
        chart.ChartArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
        chart.PlotArea.Format.Fill.Visible = True
        chart.PlotArea.Format.Fill.Solid()
        chart.PlotArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
    End Sub

    Private Shared Sub ConfigureAxes(chart As Chart, appearance As ConvexHullPlotAppearance)
        ConfigureAxis(DirectCast(chart.Axes(XlAxisType.xlCategory), Axis),
                      appearance.XAxisTitle,
                      appearance.XAxisMinimum,
                      appearance.XAxisMaximum,
                      appearance.ShowMajorGridlines,
                      appearance.GridlineColor)
        ConfigureAxis(DirectCast(chart.Axes(XlAxisType.xlValue), Axis),
                      appearance.YAxisTitle,
                      appearance.YAxisMinimum,
                      appearance.YAxisMaximum,
                      appearance.ShowMajorGridlines,
                      appearance.GridlineColor)
    End Sub

    Private Shared Sub ConfigureAxis(axis As Axis,
                                     title As String,
                                     minimum As Nullable(Of Double),
                                     maximum As Nullable(Of Double),
                                     showGridlines As Boolean,
                                     gridlineColor As Integer)
        axis.HasTitle = Not String.IsNullOrWhiteSpace(title)
        If axis.HasTitle Then axis.AxisTitle.Text = title.Trim()

        If minimum.HasValue Then
            axis.MinimumScale = minimum.Value
        Else
            axis.MinimumScaleIsAuto = True
        End If
        If maximum.HasValue Then
            axis.MaximumScale = maximum.Value
        Else
            axis.MaximumScaleIsAuto = True
        End If

        axis.HasMajorGridlines = showGridlines
        If showGridlines Then
            Try
                axis.MajorGridlines.Border.Color = gridlineColor
            Catch
            End Try
        End If
    End Sub

    Private Shared Sub ConfigureLegend(chart As Chart,
                                       keepSeriesIndices As IList(Of Integer),
                                       showLegend As Boolean)
        If Not showLegend OrElse keepSeriesIndices Is Nothing OrElse keepSeriesIndices.Count = 0 Then
            chart.HasLegend = False
            Return
        End If

        chart.HasLegend = True
        chart.Legend.Position = XlLegendPosition.xlLegendPositionBottom
        Dim keep As New HashSet(Of Integer)(keepSeriesIndices)
        Dim entries As LegendEntries = DirectCast(chart.Legend.LegendEntries(), LegendEntries)
        For i As Integer = entries.Count To 1 Step -1
            If Not keep.Contains(i) Then DirectCast(entries.Item(i), LegendEntry).Delete()
        Next
    End Sub

    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function

    Private Shared Function IsFinitePositive(value As Single) As Boolean
        Return Not Single.IsNaN(value) AndAlso Not Single.IsInfinity(value) AndAlso value > 0.0F
    End Function
End Class
