Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Controls how one or more SPC result panels are rendered as embedded Excel charts.
    ''' </summary>
    ''' <remarks>
    ''' The options contain appearance and layout settings only. Statistical values,
    ''' limits, stages, exclusions, and signals always come from the immutable
    ''' <see cref="SpcFitResult"/> snapshot.
    ''' </remarks>
    Public NotInheritable Class SpcControlChartAppearanceOptions

        Public Property Left As Double = 20.0R
        Public Property Top As Double = 20.0R
        Public Property ChartWidth As Double = 760.0R
        Public Property PanelHeight As Double = 300.0R
        Public Property PanelSpacing As Double = 18.0R

        Public Property ChartTitle As String = String.Empty
        Public Property HorizontalAxisTitle As String = "Sample"
        Public Property UseSequenceValuesForHorizontalAxis As Boolean = False
        Public Property ShowHorizontalAxisOnEveryPanel As Boolean = False
        Public Property HorizontalTickLabelOrientation As Integer = 0

        Public Property ShowLegend As Boolean = True
        Public Property ShowMajorGridlines As Boolean = True
        Public Property ShowPointLabels As Boolean = False
        Public Property ShowSignalLabels As Boolean = True
        Public Property ShowExclusionLabels As Boolean = False
        Public Property ShowLimitLabels As Boolean = True
        Public Property ShowExcludedPoints As Boolean = True
        Public Property ShowStageBoundaries As Boolean = True

        Public Property ZoneDisplay As SpcZoneDisplayMode = SpcZoneDisplayMode.Lines
        Public Property ShowZoneSeriesInLegend As Boolean = False
        Public Property ShowSpecificationLimits As Boolean = False
        Public Property ShowTargetLine As Boolean = False

        Public Property StatisticColor As Integer = Rgb(31, 119, 180)
        Public Property CenterLineColor As Integer = Rgb(44, 120, 60)
        Public Property ControlLimitColor As Integer = Rgb(192, 45, 45)
        Public Property ZoneLineColor As Integer = Rgb(145, 145, 145)
        Public Property SignalColor As Integer = Rgb(210, 25, 25)
        Public Property ExclusionColor As Integer = Rgb(230, 125, 20)
        Public Property StageBoundaryColor As Integer = Rgb(95, 95, 95)
        Public Property SpecificationColor As Integer = Rgb(125, 70, 155)
        Public Property TargetColor As Integer = Rgb(80, 80, 80)

        Public Property CenterBandColor As Integer = Rgb(210, 236, 210)
        Public Property MiddleBandColor As Integer = Rgb(249, 236, 184)
        Public Property OuterBandColor As Integer = Rgb(247, 209, 202)
        Public Property ZoneBandTransparency As Single = 0.35F

        Public Property StatisticLineWeight As Single = 1.5F
        Public Property CenterLineWeight As Single = 1.25F
        Public Property ControlLimitWeight As Single = 1.25F
        Public Property ZoneLineWeight As Single = 0.75F
        Public Property SpecificationLineWeight As Single = 1.25F
        Public Property StageBoundaryLineWeight As Single = 1.0F

        Public Property StatisticMarkerSize As Integer = 5
        Public Property SignalMarkerSize As Integer = 8
        Public Property ExclusionMarkerSize As Integer = 9
        Public Property TitleFontSize As Double = 12.0R
        Public Property AxisTitleFontSize As Double = 10.0R
        Public Property TickLabelFontSize As Double = 9.0R
        Public Property DataLabelFontSize As Double = 8.0R
        Public Property ValueNumberFormat As String = "0.####"

        Friend Function CopyValidated() As SpcControlChartAppearanceOptions
            ValidateFiniteNonnegative(Left, NameOf(Left))
            ValidateFiniteNonnegative(Top, NameOf(Top))
            ValidateFinitePositive(ChartWidth, NameOf(ChartWidth))
            ValidateFinitePositive(PanelHeight, NameOf(PanelHeight))
            ValidateFiniteNonnegative(PanelSpacing, NameOf(PanelSpacing))

            If Not [Enum].IsDefined(GetType(SpcZoneDisplayMode), ZoneDisplay) Then
                Throw New ArgumentOutOfRangeException(NameOf(ZoneDisplay))
            End If
            If HorizontalTickLabelOrientation < -90 OrElse
               HorizontalTickLabelOrientation > 90 Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(HorizontalTickLabelOrientation),
                    "The horizontal tick-label orientation must be between -90 and 90 degrees.")
            End If
            If ZoneBandTransparency < 0.0F OrElse ZoneBandTransparency > 1.0F Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(ZoneBandTransparency),
                    "Zone-band transparency must be between zero and one.")
            End If

            ValidateFinitePositive(CDbl(StatisticLineWeight), NameOf(StatisticLineWeight))
            ValidateFinitePositive(CDbl(CenterLineWeight), NameOf(CenterLineWeight))
            ValidateFinitePositive(CDbl(ControlLimitWeight), NameOf(ControlLimitWeight))
            ValidateFinitePositive(CDbl(ZoneLineWeight), NameOf(ZoneLineWeight))
            ValidateFinitePositive(CDbl(SpecificationLineWeight), NameOf(SpecificationLineWeight))
            ValidateFinitePositive(CDbl(StageBoundaryLineWeight), NameOf(StageBoundaryLineWeight))
            ValidatePositive(StatisticMarkerSize, NameOf(StatisticMarkerSize))
            ValidatePositive(SignalMarkerSize, NameOf(SignalMarkerSize))
            ValidatePositive(ExclusionMarkerSize, NameOf(ExclusionMarkerSize))
            ValidateFinitePositive(TitleFontSize, NameOf(TitleFontSize))
            ValidateFinitePositive(AxisTitleFontSize, NameOf(AxisTitleFontSize))
            ValidateFinitePositive(TickLabelFontSize, NameOf(TickLabelFontSize))
            ValidateFinitePositive(DataLabelFontSize, NameOf(DataLabelFontSize))

            Return New SpcControlChartAppearanceOptions With {
                .Left = Left,
                .Top = Top,
                .ChartWidth = ChartWidth,
                .PanelHeight = PanelHeight,
                .PanelSpacing = PanelSpacing,
                .ChartTitle = NormalizeText(ChartTitle),
                .HorizontalAxisTitle = NormalizeText(HorizontalAxisTitle),
                .UseSequenceValuesForHorizontalAxis = UseSequenceValuesForHorizontalAxis,
                .ShowHorizontalAxisOnEveryPanel = ShowHorizontalAxisOnEveryPanel,
                .HorizontalTickLabelOrientation = HorizontalTickLabelOrientation,
                .ShowLegend = ShowLegend,
                .ShowMajorGridlines = ShowMajorGridlines,
                .ShowPointLabels = ShowPointLabels,
                .ShowSignalLabels = ShowSignalLabels,
                .ShowExclusionLabels = ShowExclusionLabels,
                .ShowLimitLabels = ShowLimitLabels,
                .ShowExcludedPoints = ShowExcludedPoints,
                .ShowStageBoundaries = ShowStageBoundaries,
                .ZoneDisplay = ZoneDisplay,
                .ShowZoneSeriesInLegend = ShowZoneSeriesInLegend,
                .ShowSpecificationLimits = ShowSpecificationLimits,
                .ShowTargetLine = ShowTargetLine,
                .StatisticColor = StatisticColor,
                .CenterLineColor = CenterLineColor,
                .ControlLimitColor = ControlLimitColor,
                .ZoneLineColor = ZoneLineColor,
                .SignalColor = SignalColor,
                .ExclusionColor = ExclusionColor,
                .StageBoundaryColor = StageBoundaryColor,
                .SpecificationColor = SpecificationColor,
                .TargetColor = TargetColor,
                .CenterBandColor = CenterBandColor,
                .MiddleBandColor = MiddleBandColor,
                .OuterBandColor = OuterBandColor,
                .ZoneBandTransparency = ZoneBandTransparency,
                .StatisticLineWeight = StatisticLineWeight,
                .CenterLineWeight = CenterLineWeight,
                .ControlLimitWeight = ControlLimitWeight,
                .ZoneLineWeight = ZoneLineWeight,
                .SpecificationLineWeight = SpecificationLineWeight,
                .StageBoundaryLineWeight = StageBoundaryLineWeight,
                .StatisticMarkerSize = StatisticMarkerSize,
                .SignalMarkerSize = SignalMarkerSize,
                .ExclusionMarkerSize = ExclusionMarkerSize,
                .TitleFontSize = TitleFontSize,
                .AxisTitleFontSize = AxisTitleFontSize,
                .TickLabelFontSize = TickLabelFontSize,
                .DataLabelFontSize = DataLabelFontSize,
                .ValueNumberFormat = NormalizeNumberFormat(ValueNumberFormat)
            }
        End Function

        Private Shared Function Rgb(red As Integer,
                                    green As Integer,
                                    blue As Integer) As Integer
            Return Microsoft.VisualBasic.RGB(red, green, blue)
        End Function

        Private Shared Function NormalizeText(value As String) As String
        Return ExcelCommon.Chart34.SpcChartPreparation34.NormalizeText(value)
    End Function

        Private Shared Function NormalizeNumberFormat(value As String) As String
        Return ExcelCommon.Chart34.SpcChartPreparation34.NormalizeNumberFormat(value)
    End Function

        Private Shared Sub ValidateFinitePositive(value As Double,
                                                  parameterName As String)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value <= 0.0R Then
                Throw New ArgumentOutOfRangeException(parameterName,
                    "The value must be finite and greater than zero.")
            End If
        End Sub

        Private Shared Sub ValidateFiniteNonnegative(value As Double,
                                                     parameterName As String)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value < 0.0R Then
                Throw New ArgumentOutOfRangeException(parameterName,
                    "The value must be finite and nonnegative.")
            End If
        End Sub

        Private Shared Sub ValidatePositive(value As Integer,
                                            parameterName As String)
            If value <= 0 Then
                Throw New ArgumentOutOfRangeException(parameterName,
                    "The value must be greater than zero.")
            End If
        End Sub
    End Class

    ''' <summary>
    ''' Specifies whether one- and two-sigma zones are drawn as lines, bands, or both.
    ''' </summary>
    Public Enum SpcZoneDisplayMode
        None = 0
        Lines = 1
        ShadedBands = 2
        LinesAndShadedBands = 3
    End Enum

    ''' <summary>
    ''' Excel-specific renderer for immutable SPC fit results.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The renderer performs no SPC calculations. Every plotted statistic, centre
    ''' line, control limit, zone, signal, exclusion, and stage comes from
    ''' <see cref="SpcFitResult"/>. This keeps Excel COM dependencies out of the
    ''' statistical backend and prevents live chart formulas from becoming
    ''' inconsistent with static signal flags.
    ''' </para>
    ''' <para>
    ''' Use <see cref="SpcResultTables.BuildChartDataTables"/> with the existing
    ''' ResultTable/ExcelDnaResultWriter pipeline when a worksheet data table is also
    ''' required. This class only creates the embedded charts.
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SpcControlChartExcel

        'Microsoft.Office.Core is deliberately not a compile-time project reference.
        'These are the stable numeric values of the Office line-dash members used
        'through narrowly scoped late binding below.
        Private Const MsoLineSolid As Integer = 1
        Private Const MsoLineRoundDot As Integer = 3
        Private Const MsoLineDash As Integer = 4
        Private Const MsoLineDashDot As Integer = 5
        Private Const MsoLineLongDash As Integer = 6

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Adds one vertically aligned embedded chart for every result panel.
        ''' </summary>
        ''' <returns>The created embedded <see cref="Chart"/> objects in panel order.</returns>
        Public Shared Function AddCharts(
            worksheet As Worksheet,
            result As SpcFitResult,
            Optional appearance As SpcControlChartAppearanceOptions = Nothing) As Chart()

            Return AddCharts(worksheet, result, appearance, Nothing)
        End Function

        'The Ui21 caller passes its post-fit Common payload inside the existing
        'chart-warning boundary. Other callers retain the three-argument API.
        Friend Shared Function AddCharts(
            worksheet As Worksheet,
            result As SpcFitResult,
            appearance As SpcControlChartAppearanceOptions,
            projection As SpcUnivariateChartProjection) As Chart()

            If worksheet Is Nothing Then Throw New ArgumentNullException(NameOf(worksheet))
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))

            Dim options As SpcControlChartAppearanceOptions =
                If(appearance, New SpcControlChartAppearanceOptions()).CopyValidated()
            Dim panels As SpcPanelResult() = result.Panels
            If panels.Length = 0 Then Return Array.Empty(Of Chart)()

            If projection Is Nothing Then
                projection = SpcUnivariateChartProjectionPolicy.Project(result, options.UseSequenceValuesForHorizontalAxis)
            End If
            If projection.UseSequenceValues <> options.UseSequenceValuesForHorizontalAxis OrElse
               projection.Panels.Count <> panels.Length Then
                Throw New ArgumentException("The chart projection must match the result and axis options.", NameOf(projection))
            End If
            For i As Integer = 0 To panels.Length - 1
                If Not Object.ReferenceEquals(projection.Panels(i).Panel, panels(i)) Then
                    Throw New ArgumentException("The chart projection must belong to this result.", NameOf(projection))
                End If
            Next
            Dim axisContext As SharedAxisContext = MarshalAxis(projection)
            Dim createdCharts As New List(Of Chart)(panels.Length)

            Try
                For panelIndex As Integer = 0 To panels.Length - 1
                    Dim chartTop As Double = SpcAxisBoundsCore.SpcPanelTop(options.Top, panelIndex, options.PanelHeight, options.PanelSpacing)

                    Dim chartShape = worksheet.Shapes.AddChart(
                        Left:=options.Left,
                        Top:=chartTop,
                        Width:=options.ChartWidth,
                        Height:=options.PanelHeight)
                    Dim chart As Chart = CType(chartShape.Chart, Chart)
                    createdCharts.Add(chart)

                    RenderPanel(chart,
                                result,
                                panels(panelIndex),
                                projection.Panels(panelIndex),
                                axisContext,
                                options,
                                panelIndex,
                                panels.Length)
                Next
            Catch
                For i As Integer = createdCharts.Count - 1 To 0 Step -1
                    Try
                        createdCharts(i).Delete()
                    Catch
                        'Best-effort rollback of charts created by this call only.
                    End Try
                Next
                Throw
            End Try

            Return createdCharts.ToArray()
        End Function

        Private Shared Sub RenderPanel(chart As Chart,
                                       result As SpcFitResult,
                                       panel As SpcPanelResult,
                                       panelProjection As SpcPanelSeriesProjection,
                                       axisContext As SharedAxisContext,
                                       options As SpcControlChartAppearanceOptions,
                                       panelIndex As Integer,
                                       panelCount As Integer)
            ClearSeries(chart)
            chart.ChartType = XlChartType.xlLineMarkers
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim panelData As PanelChartData = MarshalPanel(panelProjection)
            Dim specifications As SpcSpecificationLimits = result.Request.SpecificationLimits
            Dim yBounds As AxisBounds = ComputeYAxisBounds(panel, panelData, specifications, options)
            Dim hiddenLegendEntries As New List(Of Integer)()

            Dim presentation = PreparePresentation36(panelProjection, specifications, options)
            If presentation.IncludesBands Then
                AddZoneBands(chart, axisContext.Categories,
                             New SpcZoneBands36(panelProjection.Points.ToArray(), yBounds.Minimum),
                             yBounds, options, hiddenLegendEntries)
            End If

            For Each intent In presentation.Series
                Dim values = MarshalChartValues(intent.Values)
                Dim series As Series
                Dim labelColor As Integer = options.ControlLimitColor
                Select Case intent.Role
                    Case SpcSeriesRole36.Statistic
                        series = AddLineSeries(chart, intent.Name, axisContext.Categories, values,
                            options.StatisticColor, options.StatisticLineWeight, MsoLineSolid,
                            XlMarkerStyle.xlMarkerStyleCircle, options.StatisticMarkerSize, True)
                        If options.ShowPointLabels Then AddPointLabels(series, panelProjection.PointLabels, options)
                    Case SpcSeriesRole36.Excluded
                        series = AddMarkerSeries(chart, intent.Name, axisContext.Categories, values,
                            options.ExclusionColor, XlMarkerStyle.xlMarkerStyleX, options.ExclusionMarkerSize)
                        If options.ShowExclusionLabels Then AddExclusionLabels(series, panelProjection.ExclusionLabels, options)
                    Case SpcSeriesRole36.Signal
                        series = AddMarkerSeries(chart, intent.Name, axisContext.Categories, values,
                            options.SignalColor, XlMarkerStyle.xlMarkerStyleDiamond, options.SignalMarkerSize)
                        If options.ShowSignalLabels Then AddSignalLabels(series, panelProjection.SignalLabels, options)
                    Case Else
                        Dim color As Integer = options.ControlLimitColor
                        Dim weight As Single = options.ControlLimitWeight
                        Dim dash As Integer = MsoLineDash
                        Select Case intent.Role
                            Case SpcSeriesRole36.Center
                                color = options.CenterLineColor : weight = options.CenterLineWeight : dash = MsoLineSolid
                            Case SpcSeriesRole36.Zone
                                color = options.ZoneLineColor : weight = options.ZoneLineWeight : dash = MsoLineRoundDot
                            Case SpcSeriesRole36.Specification
                                color = options.SpecificationColor : weight = options.SpecificationLineWeight : dash = MsoLineLongDash
                            Case SpcSeriesRole36.Target
                                color = options.TargetColor : weight = options.SpecificationLineWeight : dash = MsoLineDashDot
                        End Select
                        labelColor = color
                        series = AddLineSeries(chart, intent.Name, axisContext.Categories, values, color,
                            weight, dash, XlMarkerStyle.xlMarkerStyleNone, 2, True)
                End Select
                If intent.ShowLimitLabel AndAlso intent.LastValueOrdinal >= 0 Then
                    SetPointLabel(series, intent.LastValueOrdinal, intent.Name, labelColor,
                                  options.DataLabelFontSize, XlDataLabelPosition.xlLabelPositionRight)
                End If
                If intent.HideLegend Then hiddenLegendEntries.Add(GetSeriesCollection(chart).Count)
            Next

            ConfigureChart(chart,
                           result,
                           panel,
                           yBounds,
                           options,
                           panelIndex,
                           panelCount)

            If hiddenLegendEntries.Count > 0 Then
                RemoveLegendEntries(chart, hiddenLegendEntries)
            End If

            If options.ShowStageBoundaries Then
                AddStageBoundaries(chart, axisContext, options)
            End If
        End Sub

        'Same post-CopyValidated snapshot for both Ui21 and Ui22 shared-renderer paths.
        Friend Shared Function PreparePresentation36(panel As SpcPanelSeriesProjection,
                                                      specifications As SpcSpecificationLimits,
                                                      options As SpcControlChartAppearanceOptions) As SpcPanelPresentation36
            Return New SpcPanelPresentation36(panel, specifications, CInt(options.ZoneDisplay),
                options.ShowSpecificationLimits, options.ShowTargetLine, options.ShowExcludedPoints,
                options.ShowLimitLabels, options.ShowZoneSeriesInLegend)
        End Function

        Private Shared Function MarshalAxis(projection As SpcUnivariateChartProjection) As SharedAxisContext
            Return New SharedAxisContext(MarshalChartCategories(projection), projection)
        End Function

        Friend Shared Function MarshalChartCategories(projection As SpcUnivariateChartProjection) As Object()
            Dim categories(projection.Axis.Count - 1) As Object
            For i As Integer = 0 To categories.Length - 1
                Dim category = projection.Axis(i)
                If category.IsSequenceCategory Then
                    categories(i) = category.Point.SequenceValue.Value
                Else
                    categories(i) = category.Point.Label
                End If
            Next
            Return categories
        End Function

        Private Shared Function MarshalPanel(projection As SpcPanelSeriesProjection) As PanelChartData
            Dim points(projection.Points.Count - 1) As SpcPointResult
            For i As Integer = 0 To points.Length - 1
                points(i) = projection.Points(i)
            Next
            Return New PanelChartData(projection, points,
                MarshalChartValues(projection.Channels(CInt(SpcChartChannel.Statistic))),
                MarshalChartValues(projection.Channels(CInt(SpcChartChannel.CenterLine))),
                MarshalChartValues(projection.Channels(CInt(SpcChartChannel.LowerControlLimit))),
                MarshalChartValues(projection.Channels(CInt(SpcChartChannel.UpperControlLimit))),
                MarshalChartValues(projection.Channels(CInt(SpcChartChannel.LowerOneSigma))),
                MarshalChartValues(projection.Channels(CInt(SpcChartChannel.UpperOneSigma))),
                MarshalChartValues(projection.Channels(CInt(SpcChartChannel.LowerTwoSigma))),
                MarshalChartValues(projection.Channels(CInt(SpcChartChannel.UpperTwoSigma))),
                MarshalChartValues(projection.Channels(CInt(SpcChartChannel.Signal))),
                MarshalChartValues(projection.Channels(CInt(SpcChartChannel.Excluded))))
        End Function

        'Excel-specific blank VARIANT marshalling stays at the native boundary.
        Friend Shared Function MarshalChartValues(values As IReadOnlyList(Of Double?)) As Object()
            Dim marshalled(values.Count - 1) As Object
            For i As Integer = 0 To values.Count - 1
                If values(i).HasValue Then marshalled(i) = values(i).Value
            Next
            Return marshalled
        End Function

        Private Shared Sub AddZoneBands(chart As Chart,
                                        categories As Object(),
                                        bands As SpcZoneBands36,
                                        bounds As AxisBounds,
                                        options As SpcControlChartAppearanceOptions,
                                        hiddenLegendEntries As List(Of Integer))
            Dim vectors = bands.Values
            Dim baseValues = MarshalChartValues(vectors(0))
            Dim lowerOuter = MarshalChartValues(vectors(1))
            Dim lowerMiddle = MarshalChartValues(vectors(2))
            Dim lowerCenter = MarshalChartValues(vectors(3))
            Dim upperCenter = MarshalChartValues(vectors(4))
            Dim upperMiddle = MarshalChartValues(vectors(5))
            Dim upperOuter = MarshalChartValues(vectors(6))
            If bands.ValidPointCount = 0 Then Return

            hiddenLegendEntries.Add(AddAreaSeries(
                chart, "Zone offset", categories, baseValues, options.CenterBandColor,
                1.0F, visible:=False))

            Dim visibleBandIndices As Integer() = {
                AddAreaSeries(chart, "Lower zone A", categories, lowerOuter,
                              options.OuterBandColor, options.ZoneBandTransparency, True),
                AddAreaSeries(chart, "Lower zone B", categories, lowerMiddle,
                              options.MiddleBandColor, options.ZoneBandTransparency, True),
                AddAreaSeries(chart, "Lower zone C", categories, lowerCenter,
                              options.CenterBandColor, options.ZoneBandTransparency, True),
                AddAreaSeries(chart, "Upper zone C", categories, upperCenter,
                              options.CenterBandColor, options.ZoneBandTransparency, True),
                AddAreaSeries(chart, "Upper zone B", categories, upperMiddle,
                              options.MiddleBandColor, options.ZoneBandTransparency, True),
                AddAreaSeries(chart, "Upper zone A", categories, upperOuter,
                              options.OuterBandColor, options.ZoneBandTransparency, True)
            }
            If Not options.ShowZoneSeriesInLegend Then
                hiddenLegendEntries.AddRange(visibleBandIndices)
            End If

            ConfigureSecondaryZoneAxis(chart, bounds)
        End Sub

        Private Shared Function AddAreaSeries(chart As Chart,
                                              name As String,
                                              categories As Object(),
                                              values As Object(),
                                              color As Integer,
                                              transparency As Single,
                                              visible As Boolean) As Integer
            Dim collection As SeriesCollection = GetSeriesCollection(chart)
            Dim series As Series = collection.NewSeries()
            With series
                .Name = name
                .XValues = categories
                .Values = values
                .ChartType = XlChartType.xlAreaStacked
                .AxisGroup = XlAxisGroup.xlSecondary
            End With
            Dim seriesObject As Object = series
            seriesObject.Format.Line.Visible = False
            If visible Then
                seriesObject.Format.Fill.Visible = True
                seriesObject.Format.Fill.Solid()
                seriesObject.Format.Fill.ForeColor.RGB = color
                seriesObject.Format.Fill.Transparency = transparency
            Else
                seriesObject.Format.Fill.Visible = False
            End If
            Return collection.Count
        End Function

        Private Shared Function AddLineSeries(chart As Chart,
                                              name As String,
                                              categories As Object(),
                                              values As Object(),
                                              color As Integer,
                                              lineWeight As Single,
                                              dashStyle As Integer,
                                              markerStyle As XlMarkerStyle,
                                              markerSize As Integer,
                                              showLine As Boolean) As Series
            Dim collection As SeriesCollection = GetSeriesCollection(chart)
            Dim series As Series = collection.NewSeries()
            With series
                .Name = name
                .XValues = categories
                .Values = values
                .ChartType = XlChartType.xlLineMarkers
                .AxisGroup = XlAxisGroup.xlPrimary
                .MarkerStyle = markerStyle
                .MarkerSize = markerSize
                .MarkerForegroundColor = color
                .MarkerBackgroundColor = color
            End With
            Dim seriesObject As Object = series
            seriesObject.Format.Line.Visible = showLine
            If showLine Then
                seriesObject.Format.Line.ForeColor.RGB = color
                seriesObject.Format.Line.Weight = lineWeight
                seriesObject.Format.Line.DashStyle = dashStyle
            End If
            Return series
        End Function

        Private Shared Function AddMarkerSeries(chart As Chart,
                                                name As String,
                                                categories As Object(),
                                                values As Object(),
                                                color As Integer,
                                                markerStyle As XlMarkerStyle,
                                                markerSize As Integer) As Series
            Return AddLineSeries(chart,
                                 name,
                                 categories,
                                 values,
                                 color,
                                 1.0F,
                                 MsoLineSolid,
                                 markerStyle,
                                 markerSize,
                                 showLine:=False)
        End Function


        Private Shared Sub ConfigureChart(chart As Chart,
                                          result As SpcFitResult,
                                          panel As SpcPanelResult,
                                          bounds As AxisBounds,
                                          options As SpcControlChartAppearanceOptions,
                                          panelIndex As Integer,
                                          panelCount As Integer)
            chart.HasTitle = True
            chart.ChartTitle.Text = ResolveChartTitle(result, panel, options, panelCount)
            chart.ChartTitle.Font.Size = options.TitleFontSize
            chart.HasLegend = options.ShowLegend
            If chart.HasLegend Then
                chart.Legend.Position = XlLegendPosition.xlLegendPositionBottom
                chart.Legend.Font.Size = options.TickLabelFontSize
                chart.Legend.IncludeInLayout = True
            End If

            Dim valueAxis As Axis = CType(
                chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary), Axis)
            valueAxis.MinimumScale = bounds.Minimum
            valueAxis.MaximumScale = bounds.Maximum
            valueAxis.HasTitle = True
            valueAxis.AxisTitle.Text = ResolveValueAxisTitle(result, panel)
            valueAxis.AxisTitle.Font.Size = options.AxisTitleFontSize
            valueAxis.TickLabels.Font.Size = options.TickLabelFontSize
            valueAxis.TickLabels.NumberFormat = options.ValueNumberFormat

            If options.ShowMajorGridlines Then
                valueAxis.HasMajorGridlines = True
                Try
                    valueAxis.MajorGridlines.Border.Color = Rgb(225, 225, 225)
                Catch
                    'Gridline formatting is cosmetic and version-dependent.
                End Try
            Else
                valueAxis.HasMajorGridlines = False
            End If

            Dim categoryAxis As Axis = CType(
                chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary), Axis)
            categoryAxis.CategoryType = XlCategoryType.xlCategoryScale
            categoryAxis.TickLabels.Font.Size = options.TickLabelFontSize
            categoryAxis.TickLabels.Orientation = options.HorizontalTickLabelOrientation

            Dim showHorizontal As Boolean =
                options.ShowHorizontalAxisOnEveryPanel OrElse panelIndex = panelCount - 1
            If showHorizontal Then
                categoryAxis.TickLabelPosition =
                    XlTickLabelPosition.xlTickLabelPositionNextToAxis
                categoryAxis.HasTitle = options.HorizontalAxisTitle.Length > 0
                If categoryAxis.HasTitle Then
                    categoryAxis.AxisTitle.Text = options.HorizontalAxisTitle
                    categoryAxis.AxisTitle.Font.Size = options.AxisTitleFontSize
                End If
            Else
                categoryAxis.TickLabelPosition =
                    XlTickLabelPosition.xlTickLabelPositionNone
                categoryAxis.HasTitle = False
            End If
        End Sub

        Private Shared Sub ConfigureSecondaryZoneAxis(chart As Chart,
                                                      bounds As AxisBounds)
            Try
                chart.HasAxis(XlAxisType.xlValue, XlAxisGroup.xlSecondary) = True
                Dim axis As Axis = CType(
                    chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlSecondary), Axis)
                axis.MinimumScale = 0.0R
                axis.MaximumScale = bounds.Maximum - bounds.Minimum
                axis.TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                axis.HasTitle = False
                axis.HasMajorGridlines = False
                Dim axisObject As Object = axis
                axisObject.Format.Line.Visible = False
            Catch
                'The zone bands remain optional if a particular Excel build does not
                'expose the secondary-axis formatting surface consistently.
            End Try

            Try
                chart.HasAxis(XlAxisType.xlCategory, XlAxisGroup.xlSecondary) = True
                Dim axis As Axis = CType(
                    chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlSecondary), Axis)
                axis.TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                axis.HasTitle = False
                Dim axisObject As Object = axis
                axisObject.Format.Line.Visible = False
            Catch
            End Try
        End Sub

        Private Shared Sub AddStageBoundaries(
            chart As Chart,
            axisContext As SharedAxisContext,
            options As SpcControlChartAppearanceOptions)

            If axisContext.Projection.Axis.Count < 2 Then Return

            Try
                chart.Refresh()
                Dim plotLeft As Double = CDbl(chart.PlotArea.InsideLeft)
                Dim plotTop As Double = CDbl(chart.PlotArea.InsideTop)
                Dim plotWidth As Double = CDbl(chart.PlotArea.InsideWidth)
                Dim plotHeight As Double = CDbl(chart.PlotArea.InsideHeight)
                For Each boundary In axisContext.Projection.StageBoundaries
                    Dim x As Single = CSng(plotLeft + plotWidth * boundary.Fraction)
                    Dim boundaryObject As Object = chart.Shapes.AddLine(
                        x,
                        CSng(plotTop),
                        x,
                        CSng(plotTop + plotHeight))
                    boundaryObject.Line.ForeColor.RGB = options.StageBoundaryColor
                    boundaryObject.Line.DashStyle = MsoLineDash
                    boundaryObject.Line.Weight = options.StageBoundaryLineWeight
                    boundaryObject.AlternativeText = boundary.AlternativeText
                Next
            Catch
                'Stage boundaries are annotations. Do not discard an otherwise valid
                'control chart when a specific Excel version delays PlotArea geometry.
            End Try
        End Sub

        Private Shared Sub AddPointLabels(series As Series,
                                          labels As IReadOnlyList(Of SpcChartLabelIntent),
                                          options As SpcControlChartAppearanceOptions)
            For Each label In labels
                SetPointLabel(series,
                              label.SeriesIndex,
                              label.Text,
                              options.StatisticColor,
                              options.DataLabelFontSize,
                              XlDataLabelPosition.xlLabelPositionAbove)
            Next
        End Sub

        Private Shared Sub AddSignalLabels(series As Series,
                                          labels As IReadOnlyList(Of SpcChartLabelIntent),
                                          options As SpcControlChartAppearanceOptions)
            For Each label In labels
                SetPointLabel(series,
                              label.SeriesIndex,
                              label.Text,
                              options.SignalColor,
                              options.DataLabelFontSize,
                              XlDataLabelPosition.xlLabelPositionAbove)
            Next
        End Sub

        Private Shared Sub AddExclusionLabels(series As Series,
                                          labels As IReadOnlyList(Of SpcChartLabelIntent),
                                          options As SpcControlChartAppearanceOptions)
            For Each label In labels
                SetPointLabel(series,
                              label.SeriesIndex,
                              label.Text,
                              options.ExclusionColor,
                              options.DataLabelFontSize,
                              XlDataLabelPosition.xlLabelPositionBelow)
            Next
        End Sub

        Private Shared Sub SetPointLabel(series As Series,
                                         zeroBasedPointIndex As Integer,
                                         text As String,
                                         color As Integer,
                                         fontSize As Double,
                                         position As XlDataLabelPosition)
            If String.IsNullOrWhiteSpace(text) Then Return
            Try
                Dim chartPoint As Point = CType(series.Points(zeroBasedPointIndex + 1), Point)
                chartPoint.HasDataLabel = True
                chartPoint.DataLabel.Text = text
                chartPoint.DataLabel.Position = position
                chartPoint.DataLabel.Font.Size = fontSize
                chartPoint.DataLabel.Font.Color = color
            Catch
                'Data-label support differs slightly between Excel chart versions.
            End Try
        End Sub


        Private Shared Function ComputeYAxisBounds(
            panel As SpcPanelResult,
            data As PanelChartData,
            specifications As SpcSpecificationLimits,
            options As SpcControlChartAppearanceOptions) As AxisBounds

            Dim bounds=SpcPanelPresentation36.AxisBounds(data.Projection,specifications,options.ShowSpecificationLimits,options.ShowTargetLine)
            Return New AxisBounds(bounds.Min,bounds.Max)
        End Function


        Private Shared Function PanelSupportsSpecificationLimits(
            panelType As SpcPanelType) As Boolean
        Return ExcelCommon.Chart34.SpcChartPreparation34.PanelSupportsSpecificationLimits(panelType)
    End Function



        Private Shared Function ResolveChartTitle(
            result As SpcFitResult,
            panel As SpcPanelResult,
            options As SpcControlChartAppearanceOptions,
            panelCount As Integer) As String
        Return ExcelCommon.Chart34.SpcChartPreparation34.ResolveChartTitle(result, panel, SnapshotSpcSettings34(options), panelCount)
    End Function

        Private Shared Function ResolveValueAxisTitle(
            result As SpcFitResult,
            panel As SpcPanelResult) As String
        Return ExcelCommon.Chart34.SpcChartPreparation34.ResolveValueAxisTitle(result, panel)
    End Function






        Private Shared Function HasValidZones(point As SpcPointResult) As Boolean
            Return SpcZoneBands36.HasValidZones(point)
        End Function

        Private Shared Function MissingChartValue() As Object
            'Nothing marshals as a blank VARIANT and is accepted by Series.Values.
            'Chart.DisplayBlanksAs controls whether the point is plotted.
            Return Nothing
        End Function


        Private Shared Function Rgb(red As Integer,
                                    green As Integer,
                                    blue As Integer) As Integer
            Return Microsoft.VisualBasic.RGB(red, green, blue)
        End Function

        Private Shared Function GetSeriesCollection(chart As Chart) As SeriesCollection
            Return CType(chart.SeriesCollection(), SeriesCollection)
        End Function

        Private Shared Sub ClearSeries(chart As Chart)
            Dim collection As SeriesCollection = GetSeriesCollection(chart)
            Do While collection.Count > 0
                CType(collection.Item(1), Series).Delete()
            Loop
        End Sub

        Private Shared Sub RemoveLegendEntries(chart As Chart,
                                               seriesIndices As List(Of Integer))
            If Not chart.HasLegend OrElse seriesIndices Is Nothing Then Return
            seriesIndices.Sort()

            Try
                Dim entries As LegendEntries = CType(chart.Legend.LegendEntries(), LegendEntries)
                Dim previous As Integer = Integer.MaxValue
                For i As Integer = seriesIndices.Count - 1 To 0 Step -1
                    Dim index As Integer = seriesIndices(i)
                    If index <> previous AndAlso index >= 1 AndAlso index <= entries.Count Then
                        CType(entries.Item(index), LegendEntry).Delete()
                        previous = index
                    End If
                Next
            Catch
                'Legend-entry deletion is cosmetic; retain the chart if Excel declines it.
            End Try
        End Sub

        Private NotInheritable Class SharedAxisContext
            Public Sub New(categories As Object(), projection As SpcUnivariateChartProjection)
                Me.Categories = categories
                Me.Projection = projection
            End Sub
            Public ReadOnly Categories As Object()
            Public ReadOnly Projection As SpcUnivariateChartProjection
        End Class

        Private NotInheritable Class PanelChartData
            Public Sub New(projection As SpcPanelSeriesProjection, points As SpcPointResult(),
                           statisticValues As Object(),
                           centerLineValues As Object(),
                           lowerControlLimitValues As Object(),
                           upperControlLimitValues As Object(),
                           lowerOneSigmaValues As Object(),
                           upperOneSigmaValues As Object(),
                           lowerTwoSigmaValues As Object(),
                           upperTwoSigmaValues As Object(),
                           signalValues As Object(),
                           excludedValues As Object())
                Me.Projection = projection
                Me.Points = points
                Me.StatisticValues = statisticValues
                Me.CenterLineValues = centerLineValues
                Me.LowerControlLimitValues = lowerControlLimitValues
                Me.UpperControlLimitValues = upperControlLimitValues
                Me.LowerOneSigmaValues = lowerOneSigmaValues
                Me.UpperOneSigmaValues = upperOneSigmaValues
                Me.LowerTwoSigmaValues = lowerTwoSigmaValues
                Me.UpperTwoSigmaValues = upperTwoSigmaValues
                Me.SignalValues = signalValues
                Me.ExcludedValues = excludedValues
            End Sub

            Public ReadOnly Projection As SpcPanelSeriesProjection
            Public ReadOnly Points As SpcPointResult()
            Public ReadOnly StatisticValues As Object()
            Public ReadOnly CenterLineValues As Object()
            Public ReadOnly LowerControlLimitValues As Object()
            Public ReadOnly UpperControlLimitValues As Object()
            Public ReadOnly LowerOneSigmaValues As Object()
            Public ReadOnly UpperOneSigmaValues As Object()
            Public ReadOnly LowerTwoSigmaValues As Object()
            Public ReadOnly UpperTwoSigmaValues As Object()
            Public ReadOnly SignalValues As Object()
            Public ReadOnly ExcludedValues As Object()
        End Class

        Private NotInheritable Class AxisBounds
            Public Sub New(minimum As Double, maximum As Double)
                Me.Minimum = minimum
                Me.Maximum = maximum
            End Sub

            Public ReadOnly Minimum As Double
            Public ReadOnly Maximum As Double
        End Class

    Private Shared Function SnapshotSpcSettings34(appearance As SpcControlChartAppearanceOptions) As ExcelCommon.Chart34.SpcChartSettings34
        If appearance Is Nothing Then Return Nothing
        Return New ExcelCommon.Chart34.SpcChartSettings34(appearance.ChartTitle)
    End Function
End Class

End Namespace
