Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Windows compatibility façade for the portable polar geometry engine.
''' Keeps the existing constructor, grouped factory, and Compute API for the Excel UI.
''' The result DTOs and all calculations now belong to BESHStat.Core.
''' </summary>
Public Class PolarPlot
    Private ReadOnly _core As PolarPlotCore

    Public Sub New(radius As Double(),
                   angle As Double(),
                   Optional options As PolarPlotOptions = Nothing,
                   Optional groupValues As Array = Nothing)
        _core = New PolarPlotCore(radius, angle, options, groupValues)
    End Sub

    Public Shared Function CreateGrouped(radius As Double(),
                                         angle As Double(),
                                         groupValues As Array,
                                         Optional options As PolarPlotOptions = Nothing) As PolarPlot
        If groupValues Is Nothing Then Throw New ArgumentNullException(NameOf(groupValues))
        Return New PolarPlot(radius, angle, options, groupValues)
    End Function

    Public Function Compute() As PolarPlotResult
        Return _core.Compute()
    End Function
End Class

''' <summary>
''' Specifies how grouped polar-plot series are distinguished in the Excel renderer.
''' </summary>
Public Enum PolarGroupStyleMode
    ''' <summary>
    ''' Every group uses a different color while retaining the configured marker shape.
    ''' </summary>
    Color

    ''' <summary>
    ''' Every group uses a different marker shape while retaining the configured data color.
    ''' </summary>
    Marker

    ''' <summary>
    ''' Every group uses both a different color and a different marker shape.
    ''' </summary>
    ColorAndMarker
End Enum

''' <summary>
''' Contains Excel-rendering appearance settings for <see cref="PolarPlotExcel"/>.
''' </summary>
''' <remarks>
''' Colors are OLE RGB integers, as expected by the Excel object model. These
''' settings do not affect the numerical geometry stored in <see cref="PolarPlotResult"/>.
''' </remarks>
Public Class PolarPlotAppearance
    ''' <summary>
    ''' Gets or sets the chart title. Set an empty string to suppress the title.
    ''' </summary>
    Public Property ChartTitle As String = "Polar plot"

    ''' <summary>
    ''' Gets or sets the name of the observation series.
    ''' </summary>
    Public Property SeriesName As String = "Data"

    ''' <summary>
    ''' Gets or sets the OLE RGB color used for the data line and markers.
    ''' </summary>
    Public Property DataColor As Integer = &HB4771F

    ''' <summary>
    ''' Gets or sets whether grouped data are distinguished by color, marker, or both.
    ''' </summary>
    Public Property GroupStyleMode As PolarGroupStyleMode = PolarGroupStyleMode.ColorAndMarker

    ''' <summary>
    ''' Gets or sets the OLE RGB palette cycled across grouping levels.
    ''' </summary>
    ''' <remarks>
    ''' The default is the ten-color Tableau palette expressed in Excel's OLE RGB
    ''' byte order. At least one color is required when <see cref="GroupStyleMode"/>
    ''' uses color. When there are more groups than colors, the palette repeats.
    ''' </remarks>
    Public Property GroupColors As Integer() = {
        &HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794,
        &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17
    }

    ''' <summary>
    ''' Gets or sets the OLE RGB color used for radial grid circles.
    ''' </summary>
    Public Property GridColor As Integer = &HDCDCDC

    ''' <summary>
    ''' Gets or sets the OLE RGB color used for angular spokes.
    ''' </summary>
    Public Property SpokeColor As Integer = &HBEBEBE

    ''' <summary>
    ''' Gets or sets the OLE RGB color used for angular and radial labels.
    ''' </summary>
    Public Property TextColor As Integer = &H505050

    ''' <summary>
    ''' Gets or sets the OLE RGB color used for the chart and plot backgrounds.
    ''' </summary>
    Public Property BackgroundColor As Integer = &HFFFFFF

    ''' <summary>
    ''' Gets or sets the Excel marker style used for observations.
    ''' </summary>
    Public Property MarkerStyle As XlMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle

    ''' <summary>
    ''' Gets or sets the Excel marker-style palette cycled across grouping levels.
    ''' </summary>
    ''' <remarks>
    ''' At least one marker is required when <see cref="GroupStyleMode"/> uses
    ''' marker shapes. When there are more groups than marker styles, the palette repeats.
    ''' </remarks>
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

    ''' <summary>
    ''' Gets or sets the marker size in points.
    ''' </summary>
    Public Property MarkerSize As Integer = 6

    ''' <summary>
    ''' Gets or sets the data-line width in points.
    ''' </summary>
    Public Property DataLineWeight As Single = 1.5F

    ''' <summary>
    ''' Gets or sets the circular-gridline width in points.
    ''' </summary>
    Public Property GridLineWeight As Single = 0.75F

    ''' <summary>
    ''' Gets or sets the angular-spoke width in points.
    ''' </summary>
    Public Property SpokeLineWeight As Single = 0.75F

    ''' <summary>
    ''' Gets or sets the label font size in points.
    ''' </summary>
    Public Property LabelFontSize As Single = 9.0F

    ''' <summary>
    ''' Gets or sets whether angular labels are drawn.
    ''' </summary>
    Public Property ShowAngularLabels As Boolean = True

    ''' <summary>
    ''' Gets or sets whether radial labels are drawn.
    ''' </summary>
    Public Property ShowRadialLabels As Boolean = True

    ''' <summary>
    ''' Gets or sets whether a legend containing the data-series name is displayed.
    ''' </summary>
    Public Property ShowLegend As Boolean = False

    ''' <summary>
    ''' Gets or sets whether a legend is shown automatically when a grouping array was supplied.
    ''' </summary>
    ''' <remarks>
    ''' This setting is independent of <see cref="ShowLegend"/>, which controls the
    ''' single-series legend. The default makes group colors and shapes interpretable
    ''' without changing the original ungrouped chart appearance.
    ''' </remarks>
    Public Property ShowGroupLegend As Boolean = True
End Class

''' <summary>
''' Renders a computed <see cref="PolarPlotResult"/> as an embedded Excel XY-scatter chart.
''' </summary>
''' <remarks>
''' The renderer writes no helper cells. Circular rings, spokes, labels, and data
''' are assigned directly to Excel series. Ordinary Cartesian axes are hidden and
''' given identical symmetric limits so the chart behaves as a polar coordinate system.
''' </remarks>
Public NotInheritable Class PolarPlotExcel
    Private Const MaximumExcelSeries As Integer = 255

    ''' <summary>
    ''' Prevents construction of this shared utility class.
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Adds a square embedded polar chart to a worksheet.
    ''' </summary>
    ''' <param name="ws">Worksheet that receives the chart.</param>
    ''' <param name="result">Host-independent geometry returned by <see cref="PolarPlot.Compute"/>.</param>
    ''' <param name="appearance">Optional appearance settings; <see langword="Nothing"/> uses defaults.</param>
    ''' <param name="left">Horizontal chart position in points.</param>
    ''' <param name="top">Vertical chart position in points.</param>
    ''' <param name="width">Requested chart width in points.</param>
    ''' <param name="height">Requested chart height in points.</param>
    ''' <returns>The created embedded Excel <see cref="Chart"/>.</returns>
    ''' <remarks>
    ''' The smaller of <paramref name="width"/> and <paramref name="height"/> is
    ''' used for both dimensions. This preserves the equal physical X/Y scale
    ''' required for circular rings.
    ''' </remarks>
    ''' <exception cref="ArgumentNullException"><paramref name="ws"/> or <paramref name="result"/> is <see langword="Nothing"/>.</exception>
    ''' <exception cref="ArgumentOutOfRangeException">A chart dimension or an appearance size is invalid.</exception>
    ''' <exception cref="InvalidOperationException">The missing-value pattern would exceed Excel's series limit.</exception>
    Public Shared Function AddChart(ws As Worksheet,
                                    result As PolarPlotResult,
                                    appearance As PolarPlotAppearance,
                                    left As Double,
                                    top As Double,
                                    width As Double,
                                    height As Double) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If Not IsFinite(left) OrElse Not IsFinite(top) Then
            Throw New ArgumentOutOfRangeException(NameOf(left), "Chart position must be finite.")
        End If
        If Not IsFinitePositive(width) OrElse Not IsFinitePositive(height) Then
            Throw New ArgumentOutOfRangeException(NameOf(width), "Chart width and height must be finite and positive.")
        End If

        Dim resolvedAppearance As PolarPlotAppearance = If(appearance, New PolarPlotAppearance())
        ValidateAppearance(resolvedAppearance, result.HasGrouping)
        ValidateSeriesCount(result, resolvedAppearance)

        Dim side As Double = Math.Min(width, height)
        Dim chartShape As Shape = Nothing

        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlXYScatter,
                                            left,
                                            top,
                                            side,
                                            side)
            'Use late binding here because LockAspectRatio is typed as Office.MsoTriState,
            'while the project intentionally references only the Excel interop assembly.
            Dim chartShapeObject As Object = chartShape
            chartShapeObject.LockAspectRatio = True

            Dim chart As Chart = chartShape.Chart
            chart.ChartType = XlChartType.xlXYScatter
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)
            ConfigureChartBackground(chart, resolvedAppearance)
            ConfigureAxes(chart, result)
            ConfigureTitle(chart, resolvedAppearance)

            For Each circle As PolarGridCircle In result.GridCircles
                AddLineSeries(seriesCollection,
                              circle.Coordinates,
                              "Polar grid",
                              resolvedAppearance.GridColor,
                              resolvedAppearance.GridLineWeight)
            Next

            For Each spoke As PolarSpoke In result.Spokes
                AddLineSeries(seriesCollection,
                              spoke.Coordinates,
                              "Polar spoke",
                              resolvedAppearance.SpokeColor,
                              resolvedAppearance.SpokeLineWeight)
            Next

            If resolvedAppearance.ShowAngularLabels Then
                AddLabelSeries(seriesCollection,
                               result.AngularLabels,
                               "Angular labels",
                               resolvedAppearance)
            End If

            If resolvedAppearance.ShowRadialLabels Then
                AddLabelSeries(seriesCollection,
                               result.RadialLabels,
                               "Radial labels",
                               resolvedAppearance)
            End If

            Dim legendSeriesIndices As New List(Of Integer)()
            Dim groups As PolarPlotGroupSeries() = result.GroupSeries
            For groupIndex As Integer = 0 To groups.Length - 1
                Dim group As PolarPlotGroupSeries = groups(groupIndex)
                Dim seriesName As String = ResolveSeriesName(group, groupIndex, result, resolvedAppearance)
                Dim seriesColor As Integer = ResolveGroupColor(groupIndex, result, resolvedAppearance)
                Dim markerStyle As XlMarkerStyle = ResolveGroupMarkerStyle(groupIndex,
                                                                          result,
                                                                          resolvedAppearance)

                If result.ConnectPoints Then
                    Dim segments As PolarPlotSeries() = group.DataSegments
                    For segmentIndex As Integer = 0 To segments.Length - 1
                        Dim newIndex As Integer = AddDataSeries(seriesCollection,
                                                               segments(segmentIndex),
                                                               seriesName,
                                                               seriesColor,
                                                               markerStyle,
                                                               resolvedAppearance,
                                                               True,
                                                               segmentIndex + 1)
                        If segmentIndex = 0 Then legendSeriesIndices.Add(newIndex)
                    Next
                Else
                    legendSeriesIndices.Add(AddDataSeries(seriesCollection,
                                                          group.MarkerSeries,
                                                          seriesName,
                                                          seriesColor,
                                                          markerStyle,
                                                          resolvedAppearance,
                                                          False,
                                                          1))
                End If
            Next

            Dim showLegend As Boolean = resolvedAppearance.ShowLegend OrElse
                                        (result.HasGrouping AndAlso resolvedAppearance.ShowGroupLegend)
            ConfigureLegend(chart, legendSeriesIndices, showLegend)
            MakePlotAreaSquare(chart)
            chart.Refresh()
            Return chart
        Catch
            If chartShape IsNot Nothing Then
                Try
                    chartShape.Delete()
                Catch
                    'Retain the original rendering exception if chart cleanup fails.
                End Try
            End If
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Validates appearance dimensions and marker settings before creating a worksheet object.
    ''' </summary>
    ''' <param name="appearance">Appearance settings to validate.</param>
    ''' <param name="hasGrouping">Whether grouped palettes will be consumed.</param>
    Private Shared Sub ValidateAppearance(appearance As PolarPlotAppearance,
                                         hasGrouping As Boolean)
        If Not [Enum].IsDefined(GetType(PolarGroupStyleMode), appearance.GroupStyleMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.GroupStyleMode),
                                                  appearance.GroupStyleMode,
                                                  "Unknown grouped-series style mode.")
        End If
        If Not [Enum].IsDefined(GetType(XlMarkerStyle), appearance.MarkerStyle) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.MarkerStyle),
                                                  appearance.MarkerStyle,
                                                  "Unknown Excel marker style.")
        End If

        Dim usesGroupColor As Boolean = hasGrouping AndAlso
                                        (appearance.GroupStyleMode = PolarGroupStyleMode.Color OrElse
                                         appearance.GroupStyleMode = PolarGroupStyleMode.ColorAndMarker)
        If usesGroupColor AndAlso
           (appearance.GroupColors Is Nothing OrElse appearance.GroupColors.Length = 0) Then
            Throw New ArgumentException("GroupColors must contain at least one color when grouped series use color.",
                                        NameOf(appearance.GroupColors))
        End If

        Dim usesGroupMarker As Boolean = hasGrouping AndAlso
                                         (appearance.GroupStyleMode = PolarGroupStyleMode.Marker OrElse
                                          appearance.GroupStyleMode = PolarGroupStyleMode.ColorAndMarker)
        If usesGroupMarker AndAlso
           (appearance.GroupMarkerStyles Is Nothing OrElse appearance.GroupMarkerStyles.Length = 0) Then
            Throw New ArgumentException("GroupMarkerStyles must contain at least one marker when grouped series use marker shapes.",
                                        NameOf(appearance.GroupMarkerStyles))
        End If
        If usesGroupMarker Then
            For Each marker As XlMarkerStyle In appearance.GroupMarkerStyles
                If Not [Enum].IsDefined(GetType(XlMarkerStyle), marker) OrElse
                   marker = XlMarkerStyle.xlMarkerStyleNone Then
                    Throw New ArgumentOutOfRangeException(NameOf(appearance.GroupMarkerStyles),
                                                          marker,
                                                          "Every grouped marker style must be a visible Excel marker.")
                End If
            Next
        End If
        If appearance.MarkerSize < 2 OrElse appearance.MarkerSize > 72 Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.MarkerSize),
                                                  "Marker size must be between 2 and 72 points.")
        End If
        If Not IsFinitePositive(appearance.DataLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.DataLineWeight),
                                                  "Data-line weight must be positive and finite.")
        End If
        If Not IsFinitePositive(appearance.GridLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.GridLineWeight),
                                                  "Gridline weight must be positive and finite.")
        End If
        If Not IsFinitePositive(appearance.SpokeLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.SpokeLineWeight),
                                                  "Spoke-line weight must be positive and finite.")
        End If
        If Not IsFinitePositive(appearance.LabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.LabelFontSize),
                                                  "Label font size must be positive and finite.")
        End If
    End Sub

    ''' <summary>
    ''' Checks that grid, label, and data sections fit within Excel's 255-series chart limit.
    ''' </summary>
    ''' <param name="result">Computed plot result.</param>
    ''' <param name="appearance">Resolved appearance settings.</param>
    Private Shared Sub ValidateSeriesCount(result As PolarPlotResult,
                                           appearance As PolarPlotAppearance)
        Dim required As Integer = result.GridCircles.Length + result.Spokes.Length
        If appearance.ShowAngularLabels Then required += 1
        If appearance.ShowRadialLabels Then required += 1
        If result.ConnectPoints Then
            required += result.DataSegments.Length
        Else
            required += result.GroupSeries.Length
        End If

        If required > MaximumExcelSeries Then
            Throw New InvalidOperationException(
                $"The polar plot requires {required} Excel series, exceeding Excel's {MaximumExcelSeries}-series limit. " &
                "Increase the radial or angular tick interval, clear 'Connect points', or reduce missing-value gaps.")
        End If
    End Sub

    ''' <summary>
    ''' Removes all automatically generated series from a newly created chart.
    ''' </summary>
    ''' <param name="seriesCollection">Target Excel series collection.</param>
    Private Shared Sub DeleteAllSeries(seriesCollection As SeriesCollection)
        Do While seriesCollection.Count > 0
            DirectCast(seriesCollection.Item(1), Series).Delete()
        Loop
    End Sub

    ''' <summary>
    ''' Configures chart-area and plot-area fills and removes their borders.
    ''' </summary>
    ''' <param name="chart">Chart to format.</param>
    ''' <param name="appearance">Resolved appearance settings.</param>
    Private Shared Sub ConfigureChartBackground(chart As Object, appearance As PolarPlotAppearance)
        'chart is a "Chart" object
        With chart
            .ChartArea.Format.Fill.Visible = True
            .ChartArea.Format.Fill.Solid()
            .ChartArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
            .ChartArea.Format.Line.Visible = False

            .PlotArea.Format.Fill.Visible = True
            .PlotArea.Format.Fill.Solid()
            .PlotArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
            .PlotArea.Format.Line.Visible = False
        End With
    End Sub

    ''' <summary>
    ''' Applies equal symmetric scales to both Cartesian axes and hides their visual elements.
    ''' </summary>
    ''' <param name="chart">Chart to configure.</param>
    ''' <param name="result">Computed plot geometry containing the recommended extent.</param>
    Private Shared Sub ConfigureAxes(chart As Chart, result As PolarPlotResult)
        Dim xAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlCategory,
                                                  XlAxisGroup.xlPrimary), Axis)
        Dim yAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlValue,
                                                  XlAxisGroup.xlPrimary), Axis)

        ConfigureHiddenAxis(xAxis, result.CartesianExtent, result.RadialMajorInterval)
        ConfigureHiddenAxis(yAxis, result.CartesianExtent, result.RadialMajorInterval)
    End Sub

    ''' <summary>
    ''' Configures one hidden Cartesian axis used only to position polar geometry.
    ''' </summary>
    ''' <param name="axis">Excel axis to configure.</param>
    ''' <param name="extent">Positive symmetric axis extent.</param>
    ''' <param name="majorInterval">Major interval used internally by Excel.</param>
    Private Shared Sub ConfigureHiddenAxis(axis As Axis,
                                           extent As Double,
                                           majorInterval As Double)
        With axis
            .MinimumScale = -extent
            .MaximumScale = extent
            .MajorUnit = majorInterval
            .CrossesAt = 0.0R
            .HasTitle = False
            .HasMajorGridlines = False
            .HasMinorGridlines = False
            .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
            .MajorTickMark = XlTickMark.xlTickMarkNone
            .MinorTickMark = XlTickMark.xlTickMarkNone
        End With

        'Avoid a visible Cartesian cross while keeping Microsoft.Office.Core out
        'of the project's compile-time references.
        Dim axisObject As Object = axis
        axisObject.Format.Line.Visible = False
    End Sub

    ''' <summary>
    ''' Applies or suppresses the chart title.
    ''' </summary>
    ''' <param name="chart">Chart to configure.</param>
    ''' <param name="appearance">Resolved appearance settings.</param>
    Private Shared Sub ConfigureTitle(chart As Chart,
                                      appearance As PolarPlotAppearance)
        If String.IsNullOrWhiteSpace(appearance.ChartTitle) Then
            chart.HasTitle = False
        Else
            chart.HasTitle = True
            chart.ChartTitle.Text = appearance.ChartTitle
            chart.ChartTitle.Font.Size = 12
        End If
    End Sub

    ''' <summary>
    ''' Adds one marker-free line series for a grid circle or angular spoke.
    ''' </summary>
    ''' <param name="seriesCollection">Target Excel series collection.</param>
    ''' <param name="coordinates">Cartesian coordinates to draw.</param>
    ''' <param name="seriesName">Internal descriptive series name.</param>
    ''' <param name="color">OLE RGB line color.</param>
    ''' <param name="weight">Line width in points.</param>
    ''' <returns>The one-based index of the new Excel series.</returns>
    Private Shared Function AddLineSeries(seriesCollection As Object,
                                          coordinates As PolarPlotSeries,
                                          seriesName As String,
                                          color As Integer,
                                          weight As Single) As Integer
        seriesCollection.NewSeries()
        With seriesCollection.Item(seriesCollection.Count)
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatterLinesNoMarkers
            .XValues = coordinates.XValues
            .Values = coordinates.YValues
            .Smooth = False
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = color
            .Format.Line.Weight = weight
        End With

        Return seriesCollection.Count
    End Function

    ''' <summary>
    ''' Adds an invisible scatter series and uses its points as anchors for custom text labels.
    ''' </summary>
    ''' <param name="seriesCollection">Target Excel series collection.</param>
    ''' <param name="labels">Text and anchor coordinates.</param>
    ''' <param name="seriesName">Internal descriptive series name.</param>
    ''' <param name="appearance">Resolved label appearance.</param>
    ''' <returns>The one-based index of the new Excel series, or zero when no labels were supplied.</returns>
    Private Shared Function AddLabelSeries(seriesCollection As Object,
                                           labels As PolarPlotLabel(),
                                           seriesName As String,
                                           appearance As PolarPlotAppearance) As Integer
        If labels Is Nothing OrElse labels.Length = 0 Then Return 0

        Dim xValues(labels.Length - 1) As Double
        Dim yValues(labels.Length - 1) As Double
        For i As Integer = 0 To labels.Length - 1
            xValues(i) = labels(i).X
            yValues(i) = labels(i).Y
        Next

        seriesCollection.NewSeries()
        With seriesCollection.Item(seriesCollection.Count)
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatter
            .XValues = xValues
            .Values = yValues
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = False
            .ApplyDataLabels()

            For i As Integer = 1 To labels.Length
                Dim point As Point = DirectCast(.Points(i), Point)
                point.HasDataLabel = True
                point.DataLabel.Text = labels(i - 1).Text
                point.DataLabel.Position = XlDataLabelPosition.xlLabelPositionCenter
                point.DataLabel.Font.Size = appearance.LabelFontSize
                point.DataLabel.Font.Color = appearance.TextColor
            Next
        End With

        Return seriesCollection.Count
    End Function

    ''' <summary>
    ''' Resolves the visible series name for grouped or ungrouped data.
    ''' </summary>
    Private Shared Function ResolveSeriesName(group As PolarPlotGroupSeries,
                                              groupIndex As Integer,
                                              result As PolarPlotResult,
                                              appearance As PolarPlotAppearance) As String
        If result.HasGrouping Then
            If Not String.IsNullOrWhiteSpace(group.Name) Then Return group.Name
            Return "Group " & (groupIndex + 1).ToString(Globalization.CultureInfo.CurrentCulture)
        End If
        Return If(String.IsNullOrWhiteSpace(appearance.SeriesName),
                  "Data",
                  appearance.SeriesName.Trim())
    End Function

    ''' <summary>
    ''' Resolves a grouped-series color, cycling the configured palette when necessary.
    ''' </summary>
    Private Shared Function ResolveGroupColor(groupIndex As Integer,
                                              result As PolarPlotResult,
                                              appearance As PolarPlotAppearance) As Integer
        Dim varyColor As Boolean = result.HasGrouping AndAlso
                                  (appearance.GroupStyleMode = PolarGroupStyleMode.Color OrElse
                                   appearance.GroupStyleMode = PolarGroupStyleMode.ColorAndMarker)
        If Not varyColor Then Return appearance.DataColor
        Return appearance.GroupColors(groupIndex Mod appearance.GroupColors.Length)
    End Function

    ''' <summary>
    ''' Resolves a grouped-series marker, cycling the configured palette when necessary.
    ''' </summary>
    Private Shared Function ResolveGroupMarkerStyle(groupIndex As Integer,
                                                    result As PolarPlotResult,
                                                    appearance As PolarPlotAppearance) As XlMarkerStyle
        Dim varyMarker As Boolean = result.HasGrouping AndAlso
                                   (appearance.GroupStyleMode = PolarGroupStyleMode.Marker OrElse
                                    appearance.GroupStyleMode = PolarGroupStyleMode.ColorAndMarker)
        If Not varyMarker Then Return appearance.MarkerStyle
        Return appearance.GroupMarkerStyles(groupIndex Mod appearance.GroupMarkerStyles.Length)
    End Function

    ''' <summary>
    ''' Adds one data section with visible markers and an optional connecting line.
    ''' </summary>
    ''' <param name="seriesCollection">Target Excel series collection.</param>
    ''' <param name="coordinates">Data coordinates to plot.</param>
    ''' <param name="seriesName">Legend name shared by every section of one group.</param>
    ''' <param name="seriesColor">OLE RGB line and marker color.</param>
    ''' <param name="markerStyle">Visible Excel marker style.</param>
    ''' <param name="appearance">Resolved series appearance.</param>
    ''' <param name="connectPoints">Whether the section uses a connecting line.</param>
    ''' <param name="segmentNumber">One-based section number used to create a unique internal name.</param>
    ''' <returns>The one-based index of the new Excel series.</returns>
    Private Shared Function AddDataSeries(seriesCollection As Object,
                                          coordinates As PolarPlotSeries,
                                          seriesName As String,
                                          seriesColor As Integer,
                                          markerStyle As XlMarkerStyle,
                                          appearance As PolarPlotAppearance,
                                          connectPoints As Boolean,
                                          segmentNumber As Integer) As Integer
        seriesCollection.NewSeries()
        With seriesCollection.Item(seriesCollection.Count)
            Dim baseName As String = If(String.IsNullOrWhiteSpace(seriesName), "Data", seriesName)
            .Name = If(segmentNumber = 1, baseName, $"{baseName} {segmentNumber}")
            .ChartType = If(connectPoints,
                                  XlChartType.xlXYScatterLines,
                                  XlChartType.xlXYScatter)
            .XValues = coordinates.XValues
            .Values = coordinates.YValues
            .Smooth = False
            .MarkerStyle = markerStyle
            .MarkerSize = appearance.MarkerSize
            .MarkerForegroundColor = seriesColor
            .MarkerBackgroundColor = seriesColor

            If connectPoints Then
                .Format.Line.Visible = True
                .Format.Line.ForeColor.RGB = seriesColor
                .Format.Line.Weight = appearance.DataLineWeight
            Else
                .Format.Line.Visible = False
            End If
        End With

        Return seriesCollection.Count
    End Function

    ''' <summary>
    ''' Keeps one legend entry per rendered group and removes grid/label/extra-section entries.
    ''' </summary>
    ''' <param name="chart">Chart to configure.</param>
    ''' <param name="dataSeriesIndices">One-based chart-series indices for observation sections.</param>
    ''' <param name="showLegend">Whether the filtered legend should be displayed.</param>
    Private Shared Sub ConfigureLegend(chart As Chart,
                                       dataSeriesIndices As IList(Of Integer),
                                       showLegend As Boolean)
        If Not showLegend OrElse dataSeriesIndices Is Nothing OrElse dataSeriesIndices.Count = 0 Then
            chart.HasLegend = False
            Return
        End If

        chart.HasLegend = True
        chart.Legend.Position = XlLegendPosition.xlLegendPositionBottom
        Dim keepIndices As New HashSet(Of Integer)(dataSeriesIndices)
        Dim entries As LegendEntries = DirectCast(chart.Legend.LegendEntries(), LegendEntries)

        For i As Integer = entries.Count To 1 Step -1
            If Not keepIndices.Contains(i) Then
                DirectCast(entries.Item(i), LegendEntry).Delete()
            End If
        Next
    End Sub

    ''' <summary>
    ''' Makes the internal plot area square after Excel has laid out the title and legend.
    ''' </summary>
    ''' <param name="chart">Chart whose plot area is adjusted.</param>
    ''' <remarks>
    ''' Some Excel versions postpone plot-area measurements until the chart is
    ''' painted. Failure to apply the refinement is therefore nonfatal because the
    ''' enclosing chart shape is already square and both axes use identical limits.
    ''' </remarks>
    Private Shared Sub MakePlotAreaSquare(chart As Chart)
        Try
            Dim plotArea As PlotArea = chart.PlotArea
            Dim currentWidth As Double = plotArea.InsideWidth
            Dim currentHeight As Double = plotArea.InsideHeight
            Dim side As Double = Math.Min(currentWidth, currentHeight)
            Dim adjustedLeft As Double = plotArea.InsideLeft + (currentWidth - side) / 2.0R
            Dim adjustedTop As Double = plotArea.InsideTop + (currentHeight - side) / 2.0R

            plotArea.InsideLeft = adjustedLeft
            plotArea.InsideTop = adjustedTop
            plotArea.InsideWidth = side
            plotArea.InsideHeight = side
        Catch
            'The square chart shape and equal axes remain a safe fallback.
        End Try
    End Sub

    ''' <summary>
    ''' Tests whether a number is neither NaN nor infinity.
    ''' </summary>
    ''' <param name="value">Number to test.</param>
    ''' <returns><see langword="True"/> when the number is finite.</returns>
    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    ''' <summary>
    ''' Tests whether a number is finite and strictly positive.
    ''' </summary>
    ''' <param name="value">Number to test.</param>
    ''' <returns><see langword="True"/> when the number is finite and greater than zero.</returns>
    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function

    ''' <summary>
    ''' Tests whether a single-precision number is finite and strictly positive.
    ''' </summary>
    ''' <param name="value">Number to test.</param>
    ''' <returns><see langword="True"/> when the number is finite and greater than zero.</returns>
    Private Shared Function IsFinitePositive(value As Single) As Boolean
        Return Not Single.IsNaN(value) AndAlso Not Single.IsInfinity(value) AndAlso value > 0.0F
    End Function
End Class
