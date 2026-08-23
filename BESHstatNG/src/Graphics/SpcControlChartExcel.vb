Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
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
            Return If(value, String.Empty).Trim()
        End Function

        Private Shared Function NormalizeNumberFormat(value As String) As String
            Dim normalized As String = NormalizeText(value)
            If normalized.Length = 0 Then Return "0.####"
            Return normalized
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

            If worksheet Is Nothing Then Throw New ArgumentNullException(NameOf(worksheet))
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))

            Dim options As SpcControlChartAppearanceOptions =
                If(appearance, New SpcControlChartAppearanceOptions()).CopyValidated()
            Dim panels As SpcPanelResult() = result.Panels
            If panels.Length = 0 Then Return Array.Empty(Of Chart)()

            Dim axisContext As SharedAxisContext = BuildSharedAxisContext(result, options)
            Dim createdCharts As New List(Of Chart)(panels.Length)

            Try
                For panelIndex As Integer = 0 To panels.Length - 1
                    Dim chartTop As Double = options.Top +
                        panelIndex * (options.PanelHeight + options.PanelSpacing)

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
                                       axisContext As SharedAxisContext,
                                       options As SpcControlChartAppearanceOptions,
                                       panelIndex As Integer,
                                       panelCount As Integer)
            ClearSeries(chart)
            chart.ChartType = XlChartType.xlLineMarkers
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim panelData As PanelChartData = BuildPanelChartData(panel, axisContext)
            Dim specifications As SpcSpecificationLimits = result.Request.SpecificationLimits
            Dim yBounds As AxisBounds = ComputeYAxisBounds(panel, panelData, specifications, options)
            Dim hiddenLegendEntries As New List(Of Integer)()

            If IncludesBands(options.ZoneDisplay) Then
                AddZoneBands(chart,
                             axisContext.Categories,
                             panelData,
                             yBounds,
                             options,
                             hiddenLegendEntries)
            End If

            Dim statisticSeries As Series = AddLineSeries(
                chart,
                panel.DisplayName,
                axisContext.Categories,
                panelData.StatisticValues,
                options.StatisticColor,
                options.StatisticLineWeight,
                MsoLineSolid,
                XlMarkerStyle.xlMarkerStyleCircle,
                options.StatisticMarkerSize,
                showLine:=True)

            If options.ShowPointLabels Then
                AddPointLabels(statisticSeries, panelData.Points, options)
            End If

            AddReferenceLine(chart,
                             "CL",
                             axisContext.Categories,
                             panelData.CenterLineValues,
                             options.CenterLineColor,
                             options.CenterLineWeight,
                             MsoLineSolid,
                             options.ShowLimitLabels,
                             options)
            AddReferenceLine(chart,
                             "UCL",
                             axisContext.Categories,
                             panelData.UpperControlLimitValues,
                             options.ControlLimitColor,
                             options.ControlLimitWeight,
                             MsoLineDash,
                             options.ShowLimitLabels,
                             options)
            AddReferenceLine(chart,
                             "LCL",
                             axisContext.Categories,
                             panelData.LowerControlLimitValues,
                             options.ControlLimitColor,
                             options.ControlLimitWeight,
                             MsoLineDash,
                             options.ShowLimitLabels,
                             options)

            If IncludesLines(options.ZoneDisplay) Then
                AddZoneLine(chart, "-2 sigma", axisContext.Categories,
                            panelData.LowerTwoSigmaValues, options, hiddenLegendEntries)
                AddZoneLine(chart, "-1 sigma", axisContext.Categories,
                            panelData.LowerOneSigmaValues, options, hiddenLegendEntries)
                AddZoneLine(chart, "+1 sigma", axisContext.Categories,
                            panelData.UpperOneSigmaValues, options, hiddenLegendEntries)
                AddZoneLine(chart, "+2 sigma", axisContext.Categories,
                            panelData.UpperTwoSigmaValues, options, hiddenLegendEntries)
            End If

            If PanelSupportsSpecificationLimits(panel.PanelType) Then
                If options.ShowSpecificationLimits Then
                    If specifications.LowerSpecificationLimit.HasValue Then
                        AddConstantReferenceLine(chart,
                                                 "LSL",
                                                 axisContext.Categories,
                                                 specifications.LowerSpecificationLimit.Value,
                                                 options.SpecificationColor,
                                                 options.SpecificationLineWeight,
                                                 MsoLineLongDash,
                                                 options)
                    End If
                    If specifications.UpperSpecificationLimit.HasValue Then
                        AddConstantReferenceLine(chart,
                                                 "USL",
                                                 axisContext.Categories,
                                                 specifications.UpperSpecificationLimit.Value,
                                                 options.SpecificationColor,
                                                 options.SpecificationLineWeight,
                                                 MsoLineLongDash,
                                                 options)
                    End If
                End If
                If options.ShowTargetLine AndAlso specifications.Target.HasValue Then
                    AddConstantReferenceLine(chart,
                                             "Target",
                                             axisContext.Categories,
                                             specifications.Target.Value,
                                             options.TargetColor,
                                             options.SpecificationLineWeight,
                                             MsoLineDashDot,
                                             options)
                End If
            End If

            If options.ShowExcludedPoints AndAlso HasAnyValue(panelData.ExcludedValues) Then
                Dim excludedSeries As Series = AddMarkerSeries(
                    chart,
                    "Excluded",
                    axisContext.Categories,
                    panelData.ExcludedValues,
                    options.ExclusionColor,
                    XlMarkerStyle.xlMarkerStyleX,
                    options.ExclusionMarkerSize)
                If options.ShowExclusionLabels Then
                    AddExclusionLabels(excludedSeries, panelData.Points, options)
                End If
            End If

            If HasAnyValue(panelData.SignalValues) Then
                Dim signalSeries As Series = AddMarkerSeries(
                    chart,
                    "Signal",
                    axisContext.Categories,
                    panelData.SignalValues,
                    options.SignalColor,
                    XlMarkerStyle.xlMarkerStyleDiamond,
                    options.SignalMarkerSize)
                If options.ShowSignalLabels Then
                    AddSignalLabels(signalSeries, panelData.Points, options)
                End If
            End If

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

        Private Shared Function BuildSharedAxisContext(
            result As SpcFitResult,
            options As SpcControlChartAppearanceOptions) As SharedAxisContext

            Dim representativePoints As New Dictionary(Of Integer, SpcPointResult)()
            Dim panels As SpcPanelResult() = result.Panels

            For panelIndex As Integer = 0 To panels.Length - 1
                Dim points As SpcPointResult() = panels(panelIndex).Points
                For pointIndex As Integer = 0 To points.Length - 1
                    Dim point As SpcPointResult = points(pointIndex)
                    If Not representativePoints.ContainsKey(point.PointIndex) Then
                        representativePoints.Add(point.PointIndex, point)
                    End If
                Next
            Next

            Dim orderedIndices As New List(Of Integer)(representativePoints.Keys)
            orderedIndices.Sort()
            Dim categories(orderedIndices.Count - 1) As Object
            Dim stageIds(orderedIndices.Count - 1) As String

            For i As Integer = 0 To orderedIndices.Count - 1
                Dim point As SpcPointResult = representativePoints(orderedIndices(i))
                If options.UseSequenceValuesForHorizontalAxis AndAlso
                   point.SequenceValue.HasValue Then
                    categories(i) = point.SequenceValue.Value
                Else
                    categories(i) = point.Label
                End If
                stageIds(i) = point.StageId
            Next

            Return New SharedAxisContext(orderedIndices.ToArray(), categories, stageIds)
        End Function

        Private Shared Function BuildPanelChartData(
            panel As SpcPanelResult,
            axisContext As SharedAxisContext) As PanelChartData

            Dim pointByIndex As New Dictionary(Of Integer, SpcPointResult)()
            Dim sourcePoints As SpcPointResult() = panel.Points
            For i As Integer = 0 To sourcePoints.Length - 1
                pointByIndex.Add(sourcePoints(i).PointIndex, sourcePoints(i))
            Next

            Dim n As Integer = axisContext.PointIndices.Length
            Dim points(n - 1) As SpcPointResult
            Dim statistic(n - 1) As Object
            Dim center(n - 1) As Object
            Dim lcl(n - 1) As Object
            Dim ucl(n - 1) As Object
            Dim lowerOne(n - 1) As Object
            Dim upperOne(n - 1) As Object
            Dim lowerTwo(n - 1) As Object
            Dim upperTwo(n - 1) As Object
            Dim signals(n - 1) As Object
            Dim exclusions(n - 1) As Object

            For i As Integer = 0 To n - 1
                Dim point As SpcPointResult = Nothing
                If pointByIndex.TryGetValue(axisContext.PointIndices(i), point) Then
                    points(i) = point
                    statistic(i) = ChartValue(point.Value)
                    center(i) = ChartValue(point.CenterLine)
                    lcl(i) = ChartValue(point.LowerControlLimit)
                    ucl(i) = ChartValue(point.UpperControlLimit)
                    lowerOne(i) = ChartValue(point.LowerOneSigmaLimit)
                    upperOne(i) = ChartValue(point.UpperOneSigmaLimit)
                    lowerTwo(i) = ChartValue(point.LowerTwoSigmaLimit)
                    upperTwo(i) = ChartValue(point.UpperTwoSigmaLimit)
                    signals(i) = If(point.IsSignalled,
                                    ChartValue(point.Value),
                                    MissingChartValue())
                    exclusions(i) = If(point.IsExplicitlyExcluded,
                                       ChartValue(point.Value),
                                       MissingChartValue())
                Else
                    statistic(i) = MissingChartValue()
                    center(i) = MissingChartValue()
                    lcl(i) = MissingChartValue()
                    ucl(i) = MissingChartValue()
                    lowerOne(i) = MissingChartValue()
                    upperOne(i) = MissingChartValue()
                    lowerTwo(i) = MissingChartValue()
                    upperTwo(i) = MissingChartValue()
                    signals(i) = MissingChartValue()
                    exclusions(i) = MissingChartValue()
                End If
            Next

            Return New PanelChartData(points,
                                      statistic,
                                      center,
                                      lcl,
                                      ucl,
                                      lowerOne,
                                      upperOne,
                                      lowerTwo,
                                      upperTwo,
                                      signals,
                                      exclusions)
        End Function

        Private Shared Sub AddZoneBands(chart As Chart,
                                        categories As Object(),
                                        data As PanelChartData,
                                        bounds As AxisBounds,
                                        options As SpcControlChartAppearanceOptions,
                                        hiddenLegendEntries As List(Of Integer))
            Dim baseValues(categories.Length - 1) As Object
            Dim lowerOuter(categories.Length - 1) As Object
            Dim lowerMiddle(categories.Length - 1) As Object
            Dim lowerCenter(categories.Length - 1) As Object
            Dim upperCenter(categories.Length - 1) As Object
            Dim upperMiddle(categories.Length - 1) As Object
            Dim upperOuter(categories.Length - 1) As Object
            Dim validBandCount As Integer = 0

            For i As Integer = 0 To categories.Length - 1
                Dim point As SpcPointResult = data.Points(i)
                If point IsNot Nothing AndAlso HasValidZones(point) Then
                    baseValues(i) = point.LowerControlLimit - bounds.Minimum
                    lowerOuter(i) = point.LowerTwoSigmaLimit - point.LowerControlLimit
                    lowerMiddle(i) = point.LowerOneSigmaLimit - point.LowerTwoSigmaLimit
                    lowerCenter(i) = point.CenterLine - point.LowerOneSigmaLimit
                    upperCenter(i) = point.UpperOneSigmaLimit - point.CenterLine
                    upperMiddle(i) = point.UpperTwoSigmaLimit - point.UpperOneSigmaLimit
                    upperOuter(i) = point.UpperControlLimit - point.UpperTwoSigmaLimit
                    validBandCount += 1
                Else
                    baseValues(i) = MissingChartValue()
                    lowerOuter(i) = MissingChartValue()
                    lowerMiddle(i) = MissingChartValue()
                    lowerCenter(i) = MissingChartValue()
                    upperCenter(i) = MissingChartValue()
                    upperMiddle(i) = MissingChartValue()
                    upperOuter(i) = MissingChartValue()
                End If
            Next

            If validBandCount = 0 Then Return

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

        Private Shared Sub AddReferenceLine(chart As Chart,
                                            name As String,
                                            categories As Object(),
                                            values As Object(),
                                            color As Integer,
                                            lineWeight As Single,
                                            dashStyle As Integer,
                                            showLabel As Boolean,
                                            options As SpcControlChartAppearanceOptions)
            If Not HasAnyValue(values) Then Return
            Dim series As Series = AddLineSeries(
                chart,
                name,
                categories,
                values,
                color,
                lineWeight,
                dashStyle,
                XlMarkerStyle.xlMarkerStyleNone,
                2,
                showLine:=True)
            If showLabel Then AddLastValueLabel(series, values, name, color, options)
        End Sub

        Private Shared Sub AddConstantReferenceLine(
            chart As Chart,
            name As String,
            categories As Object(),
            value As Double,
            color As Integer,
            lineWeight As Single,
            dashStyle As Integer,
            options As SpcControlChartAppearanceOptions)

            Dim values(categories.Length - 1) As Object
            For i As Integer = 0 To values.Length - 1
                values(i) = value
            Next
            AddReferenceLine(chart,
                             name,
                             categories,
                             values,
                             color,
                             lineWeight,
                             dashStyle,
                             options.ShowLimitLabels,
                             options)
        End Sub

        Private Shared Sub AddZoneLine(chart As Chart,
                                       name As String,
                                       categories As Object(),
                                       values As Object(),
                                       options As SpcControlChartAppearanceOptions,
                                       hiddenLegendEntries As List(Of Integer))
            If Not HasAnyValue(values) Then Return
            AddReferenceLine(chart,
                             name,
                             categories,
                             values,
                             options.ZoneLineColor,
                             options.ZoneLineWeight,
                             MsoLineRoundDot,
                             showLabel:=False,
                             options:=options)
            If Not options.ShowZoneSeriesInLegend Then
                hiddenLegendEntries.Add(GetSeriesCollection(chart).Count)
            End If
        End Sub

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

            If axisContext.StageIds.Length < 2 Then Return

            Try
                chart.Refresh()
                Dim plotLeft As Double = CDbl(chart.PlotArea.InsideLeft)
                Dim plotTop As Double = CDbl(chart.PlotArea.InsideTop)
                Dim plotWidth As Double = CDbl(chart.PlotArea.InsideWidth)
                Dim plotHeight As Double = CDbl(chart.PlotArea.InsideHeight)
                Dim n As Integer = axisContext.StageIds.Length

                For i As Integer = 1 To n - 1
                    If Not String.Equals(axisContext.StageIds(i - 1),
                                         axisContext.StageIds(i),
                                         StringComparison.OrdinalIgnoreCase) Then
                        Dim x As Single = CSng(plotLeft + plotWidth * (CDbl(i) / CDbl(n)))
                        Dim boundaryObject As Object = chart.Shapes.AddLine(
                            x,
                            CSng(plotTop),
                            x,
                            CSng(plotTop + plotHeight))
                        boundaryObject.Line.ForeColor.RGB = options.StageBoundaryColor
                        boundaryObject.Line.DashStyle = MsoLineDash
                        boundaryObject.Line.Weight = options.StageBoundaryLineWeight
                        boundaryObject.AlternativeText = "Stage boundary: " &
                            axisContext.StageIds(i - 1) & " to " & axisContext.StageIds(i)
                    End If
                Next
            Catch
                'Stage boundaries are annotations. Do not discard an otherwise valid
                'control chart when a specific Excel version delays PlotArea geometry.
            End Try
        End Sub

        Private Shared Sub AddPointLabels(series As Series,
                                          points As SpcPointResult(),
                                          options As SpcControlChartAppearanceOptions)
            For i As Integer = 0 To points.Length - 1
                Dim pointResult As SpcPointResult = points(i)
                If pointResult IsNot Nothing AndAlso pointResult.HasFiniteValue Then
                    SetPointLabel(series,
                                  i,
                                  pointResult.Label,
                                  options.StatisticColor,
                                  options.DataLabelFontSize,
                                  XlDataLabelPosition.xlLabelPositionAbove)
                End If
            Next
        End Sub

        Private Shared Sub AddSignalLabels(series As Series,
                                           points As SpcPointResult(),
                                           options As SpcControlChartAppearanceOptions)
            For i As Integer = 0 To points.Length - 1
                Dim pointResult As SpcPointResult = points(i)
                If pointResult IsNot Nothing AndAlso
                   pointResult.HasFiniteValue AndAlso
                   pointResult.IsSignalled Then
                    SetPointLabel(series,
                                  i,
                                  FormatRuleNumbers(pointResult.SignalRuleNumbers),
                                  options.SignalColor,
                                  options.DataLabelFontSize,
                                  XlDataLabelPosition.xlLabelPositionAbove)
                End If
            Next
        End Sub

        Private Shared Sub AddExclusionLabels(series As Series,
                                              points As SpcPointResult(),
                                              options As SpcControlChartAppearanceOptions)
            For i As Integer = 0 To points.Length - 1
                Dim pointResult As SpcPointResult = points(i)
                If pointResult IsNot Nothing AndAlso
                   pointResult.HasFiniteValue AndAlso
                   pointResult.IsExplicitlyExcluded Then
                    Dim text As String = pointResult.ExclusionReason
                    If text.Length = 0 Then text = "Excluded"
                    SetPointLabel(series,
                                  i,
                                  text,
                                  options.ExclusionColor,
                                  options.DataLabelFontSize,
                                  XlDataLabelPosition.xlLabelPositionBelow)
                End If
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

        Private Shared Sub AddLastValueLabel(
            series As Series,
            values As Object(),
            text As String,
            color As Integer,
            options As SpcControlChartAppearanceOptions)

            For i As Integer = values.Length - 1 To 0 Step -1
                If IsNumericChartValue(values(i)) Then
                    SetPointLabel(series,
                                  i,
                                  text,
                                  color,
                                  options.DataLabelFontSize,
                                  XlDataLabelPosition.xlLabelPositionRight)
                    Exit For
                End If
            Next
        End Sub

        Private Shared Function ComputeYAxisBounds(
            panel As SpcPanelResult,
            data As PanelChartData,
            specifications As SpcSpecificationLimits,
            options As SpcControlChartAppearanceOptions) As AxisBounds

            Dim minimum As Double = Double.PositiveInfinity
            Dim maximum As Double = Double.NegativeInfinity
            Dim valueSets As Object()() = {
                data.StatisticValues,
                data.CenterLineValues,
                data.LowerControlLimitValues,
                data.UpperControlLimitValues,
                data.LowerOneSigmaValues,
                data.UpperOneSigmaValues,
                data.LowerTwoSigmaValues,
                data.UpperTwoSigmaValues
            }

            For setIndex As Integer = 0 To valueSets.Length - 1
                IncludeValuesInBounds(valueSets(setIndex), minimum, maximum)
            Next

            If PanelSupportsSpecificationLimits(panel.PanelType) Then
                If options.ShowSpecificationLimits Then
                    IncludeNullableInBounds(specifications.LowerSpecificationLimit,
                                            minimum,
                                            maximum)
                    IncludeNullableInBounds(specifications.UpperSpecificationLimit,
                                            minimum,
                                            maximum)
                End If
                If options.ShowTargetLine Then
                    IncludeNullableInBounds(specifications.Target, minimum, maximum)
                End If
            End If

            If Double.IsPositiveInfinity(minimum) OrElse
               Double.IsNegativeInfinity(maximum) Then
                Return New AxisBounds(0.0R, 1.0R)
            End If

            If minimum = maximum Then
                Dim scale As Double = Math.Max(1.0R, Math.Abs(minimum))
                minimum -= 0.1R * scale
                maximum += 0.1R * scale
            Else
                Dim padding As Double = 0.07R * (maximum - minimum)
                minimum -= padding
                maximum += padding
            End If

            If HasNaturalZero(panel.PanelType) AndAlso minimum >= -0.15R * Math.Max(1.0R, maximum) Then
                minimum = 0.0R
            End If
            If panel.PanelType = SpcPanelType.Proportion AndAlso
               maximum > 1.0R AndAlso maximum <= 1.1R Then
                maximum = 1.0R
            End If
            If maximum <= minimum Then maximum = minimum + 1.0R

            Return New AxisBounds(minimum, maximum)
        End Function

        Private Shared Sub IncludeValuesInBounds(values As Object(),
                                                 ByRef minimum As Double,
                                                 ByRef maximum As Double)
            For i As Integer = 0 To values.Length - 1
                If IsNumericChartValue(values(i)) Then
                    Dim value As Double = Convert.ToDouble(values(i), CultureInfo.InvariantCulture)
                    If value < minimum Then minimum = value
                    If value > maximum Then maximum = value
                End If
            Next
        End Sub

        Private Shared Sub IncludeNullableInBounds(value As Nullable(Of Double),
                                                   ByRef minimum As Double,
                                                   ByRef maximum As Double)
            If Not value.HasValue Then Return
            If value.Value < minimum Then minimum = value.Value
            If value.Value > maximum Then maximum = value.Value
        End Sub

        Private Shared Function PanelSupportsSpecificationLimits(
            panelType As SpcPanelType) As Boolean
            Select Case panelType
                Case SpcPanelType.Run,
                     SpcPanelType.IndividualValue,
                     SpcPanelType.SubgroupMean,
                     SpcPanelType.Ewma,
                     SpcPanelType.MovingAverage
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Private Shared Function HasNaturalZero(panelType As SpcPanelType) As Boolean
            Select Case panelType
                Case SpcPanelType.MovingRange,
                     SpcPanelType.SubgroupRange,
                     SpcPanelType.SubgroupStandardDeviation,
                     SpcPanelType.Proportion,
                     SpcPanelType.NumberNonconforming,
                     SpcPanelType.DefectCount,
                     SpcPanelType.DefectRate,
                     SpcPanelType.EventsBetweenOccurrences,
                     SpcPanelType.TimeBetweenOccurrences,
                     SpcPanelType.UpperCusum,
                     SpcPanelType.LowerCusum,
                     SpcPanelType.HotellingT2,
                     SpcPanelType.GeneralizedVariance,
                     SpcPanelType.PcaT2,
                     SpcPanelType.PcaQ,
                     SpcPanelType.Mewma,
                     SpcPanelType.Mcusum
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Private Shared Function ResolveChartTitle(
            result As SpcFitResult,
            panel As SpcPanelResult,
            options As SpcControlChartAppearanceOptions,
            panelCount As Integer) As String

            Dim baseTitle As String = options.ChartTitle
            If baseTitle.Length = 0 Then baseTitle = result.ChartTitle
            If panelCount <= 1 Then Return baseTitle
            Return baseTitle & " - " & panel.DisplayName
        End Function

        Private Shared Function ResolveValueAxisTitle(
            result As SpcFitResult,
            panel As SpcPanelResult) As String
            If panel.ValueAxisTitle.Length > 0 Then Return panel.ValueAxisTitle
            If result.Request.ValueAxisTitle.Length > 0 Then Return result.Request.ValueAxisTitle
            Return panel.DisplayName
        End Function

        Private Shared Function IncludesLines(mode As SpcZoneDisplayMode) As Boolean
            Return mode = SpcZoneDisplayMode.Lines OrElse
                   mode = SpcZoneDisplayMode.LinesAndShadedBands
        End Function

        Private Shared Function IncludesBands(mode As SpcZoneDisplayMode) As Boolean
            Return mode = SpcZoneDisplayMode.ShadedBands OrElse
                   mode = SpcZoneDisplayMode.LinesAndShadedBands
        End Function

        Private Shared Function HasValidZones(point As SpcPointResult) As Boolean
            If point Is Nothing Then Return False
            Dim values As Double() = {
                point.LowerControlLimit,
                point.LowerTwoSigmaLimit,
                point.LowerOneSigmaLimit,
                point.CenterLine,
                point.UpperOneSigmaLimit,
                point.UpperTwoSigmaLimit,
                point.UpperControlLimit
            }
            For i As Integer = 0 To values.Length - 1
                If Not IsFinite(values(i)) Then Return False
                If i > 0 AndAlso values(i) < values(i - 1) Then Return False
            Next
            Return True
        End Function

        Private Shared Function FormatRuleNumbers(ruleNumbers As Integer()) As String
            If ruleNumbers Is Nothing OrElse ruleNumbers.Length = 0 Then Return String.Empty
            Dim parts(ruleNumbers.Length - 1) As String
            For i As Integer = 0 To ruleNumbers.Length - 1
                parts(i) = ruleNumbers(i).ToString(CultureInfo.InvariantCulture)
            Next
            Return "R" & String.Join(",", parts)
        End Function

        Private Shared Function ChartValue(value As Double) As Object
            If IsFinite(value) Then Return value
            Return MissingChartValue()
        End Function

        Private Shared Function MissingChartValue() As Object
            'Nothing marshals as a blank VARIANT and is accepted by Series.Values.
            'Chart.DisplayBlanksAs controls whether the point is plotted.
            Return Nothing
        End Function

        Private Shared Function HasAnyValue(values As Object()) As Boolean
            For i As Integer = 0 To values.Length - 1
                If IsNumericChartValue(values(i)) Then Return True
            Next
            Return False
        End Function

        Private Shared Function IsNumericChartValue(value As Object) As Boolean
            If value Is Nothing OrElse Not IsNumeric(value) Then Return False
            Dim numericValue As Double
            Try
                numericValue = Convert.ToDouble(value, CultureInfo.InvariantCulture)
            Catch
                Return False
            End Try
            Return IsFinite(numericValue)
        End Function

        Private Shared Function IsFinite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
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
            Public Sub New(pointIndices As Integer(),
                           categories As Object(),
                           stageIds As String())
                Me.PointIndices = pointIndices
                Me.Categories = categories
                Me.StageIds = stageIds
            End Sub

            Public ReadOnly PointIndices As Integer()
            Public ReadOnly Categories As Object()
            Public ReadOnly StageIds As String()
        End Class

        Private NotInheritable Class PanelChartData
            Public Sub New(points As SpcPointResult(),
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
    End Class

End Namespace
