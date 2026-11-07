"""Consume prepared SPC intent in existing univariate/multivariate renderer path."""
from work import *
from freeze import method
if __name__=='__main__':
    assert (ROOT/'.test-rationalization-04A36.tmp/entry-source.json').is_file()
    p='BESHstatNG/src/Graphics/SpcControlChartExcel.vb'
    s=(HERE/('entry-source/'+p+'.txt')).read_text(encoding='utf-8-sig')
    begin=s.index('            If IncludesBands(options.ZoneDisplay) Then');end=s.index('            ConfigureChart(chart,',begin)
    s=s[:begin]+'''            Dim presentation = PreparePresentation36(panelProjection, specifications, options)
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

'''+s[end:]
    pos=s.index('        Private Shared Function MarshalAxis(')
    s=s[:pos]+'''        'Same post-CopyValidated snapshot for both Ui21 and Ui22 shared-renderer paths.
        Friend Shared Function PreparePresentation36(panel As SpcPanelSeriesProjection,
                                                      specifications As SpcSpecificationLimits,
                                                      options As SpcControlChartAppearanceOptions) As SpcPanelPresentation36
            Return New SpcPanelPresentation36(panel, specifications, CInt(options.ZoneDisplay),
                options.ShowSpecificationLimits, options.ShowTargetLine, options.ShowExcludedPoints,
                options.ShowLimitLabels, options.ShowZoneSeriesInLegend)
        End Function

'''+s[pos:]
    s=s.replace('data As PanelChartData,\n            bounds As AxisBounds,','bands As SpcZoneBands36,\n            bounds As AxisBounds,')
    begin=s.index('            Dim baseValues',s.index('Private Shared Sub AddZoneBands'));end=s.index('            hiddenLegendEntries.Add(AddAreaSeries',begin)
    s=s[:begin]+'''            Dim vectors = bands.Values
            Dim baseValues = MarshalChartValues(vectors(0))
            Dim lowerOuter = MarshalChartValues(vectors(1))
            Dim lowerMiddle = MarshalChartValues(vectors(2))
            Dim lowerCenter = MarshalChartValues(vectors(3))
            Dim upperCenter = MarshalChartValues(vectors(4))
            Dim upperMiddle = MarshalChartValues(vectors(5))
            Dim upperOuter = MarshalChartValues(vectors(6))
            If bands.ValidPointCount = 0 Then Return

'''+s[end:]
    # These mixed private helpers are superseded by the typed plan and the
    # native translation loop; no dead duplicate admission/last-point algorithm.
    for name in ['AddReferenceLine','AddConstantReferenceLine','AddZoneLine','AddLastValueLabel','IncludesLines','IncludesBands','HasValidZones']:
        s=s.replace(method(s,name),'')
    write(p,s)
    print('SPC shared renderer consumes portable ordered series, finite admission, labels and zone geometry')
