Option Explicit On
Option Strict Off
Imports System
Imports System.Collections.Generic
Imports BESHStatNG
Namespace BESHStatNG.TestFixtures34
Public Module LegacyBulletLayout34
    Public NotInheritable Class DrawingLayout
        Public ChartWidth As Double
        Public ChartHeight As Double
        Public PlotLeft As Double
        Public PlotTop As Double
        Public PlotWidth As Double
        Public PlotHeight As Double
        Public RowHeight As Double
        Public BandHeight As Double
        Public LeftLabelsVisible As Boolean
    End Class
    Public NotInheritable Class RowLayout
        Public RowTop As Double
        Public RowBottom As Double
        Public BandTop As Double
        Public BandBottom As Double
        Public BandCentreY As Double
        Public ScaleLabelTop As Double
        Public TargetValueLabelTop As Double
    End Class
    Private Const MinimumPlotWidth As Double = 120.0R
    Private Const MinimumBandHeight As Double = 8.0R
    Private Const MinimumRowHeight As Double = 20.0R
Public Function BuildDrawingLayout(chartWidth As Double, chartHeight As Double, result As BulletChartResult, appearance As BulletChartAppearance) As DrawingLayout
        Dim padding As Double = appearance.ChartPadding
        Dim titleHeight As Double = If(String.IsNullOrWhiteSpace(appearance.ChartTitle), 0.0R, appearance.ChartTitleHeight)
        Dim leftLabelsVisible As Boolean = appearance.ShowMeasureLabels OrElse
                                           (appearance.ShowSubtitles AndAlso result.HasSubtitles)
        Dim labelReserve As Double = If(leftLabelsVisible,
                                        appearance.LabelAreaWidth + appearance.LabelGap,
                                        0.0R)

        Dim plotLeft As Double = padding + labelReserve
        Dim plotTop As Double = padding + titleHeight
        Dim plotWidth As Double = chartWidth - padding - plotLeft
        Dim plotHeight As Double = chartHeight - padding - plotTop

        If plotWidth < MinimumPlotWidth Then
            Throw New InvalidOperationException("The embedded chart is too narrow to draw the bullet chart. Increase its width or reduce the label area.")
        End If
        If plotHeight <= 0.0R Then
            Throw New InvalidOperationException("The embedded chart is too short to draw the bullet chart. Increase its height.")
        End If

        Dim rowHeight As Double = plotHeight / CDbl(result.ItemCount)
        If rowHeight < MinimumRowHeight Then
            Throw New InvalidOperationException("The embedded chart is too short for the number of bullet-chart items. Increase its height.")
        End If

        Dim scaleReserve As Double = 0.0R
        If appearance.ShowScaleLabels OrElse appearance.ShowScaleTickMarks Then
            scaleReserve = appearance.ScaleLabelGap
            If appearance.ShowScaleTickMarks Then scaleReserve += appearance.ScaleTickLength
            If appearance.ShowScaleLabels Then scaleReserve += appearance.ScaleLabelHeight
        End If
        Dim targetLabelReserve As Double = If(appearance.ShowTargetValueLabels,
                                              appearance.ValueLabelHeight + appearance.ValueLabelGap,
                                              0.0R)

        Dim bandAvailable As Double = rowHeight - appearance.RowGap - scaleReserve - targetLabelReserve
        If bandAvailable < MinimumBandHeight Then
            Throw New InvalidOperationException("The embedded chart is too short for the selected bullet-chart labels and spacing. Increase its height or hide value/scale labels.")
        End If

        Return New DrawingLayout With {
            .ChartWidth = chartWidth,
            .ChartHeight = chartHeight,
            .PlotLeft = plotLeft,
            .PlotTop = plotTop,
            .PlotWidth = plotWidth,
            .PlotHeight = plotHeight,
            .RowHeight = rowHeight,
            .BandHeight = Math.Min(appearance.QualitativeBandHeight, bandAvailable),
            .LeftLabelsVisible = leftLabelsVisible
        }
End Function
Public Function BuildRowLayout(layout As DrawingLayout,
                                           appearance As BulletChartAppearance,
                                           rowIndex As Integer) As RowLayout
        Dim rowTop As Double = layout.PlotTop + CDbl(rowIndex) * layout.RowHeight
        Dim rowBottom As Double = rowTop + layout.RowHeight

        Dim scaleReserve As Double = 0.0R
        If appearance.ShowScaleLabels OrElse appearance.ShowScaleTickMarks Then
            scaleReserve = appearance.ScaleLabelGap
            If appearance.ShowScaleTickMarks Then scaleReserve += appearance.ScaleTickLength
            If appearance.ShowScaleLabels Then scaleReserve += appearance.ScaleLabelHeight
        End If
        Dim targetLabelReserve As Double = If(appearance.ShowTargetValueLabels,
                                              appearance.ValueLabelHeight + appearance.ValueLabelGap,
                                              0.0R)
        Dim contentHeight As Double = targetLabelReserve + layout.BandHeight + scaleReserve
        Dim contentTop As Double = rowTop + (layout.RowHeight - contentHeight) / 2.0R

        Dim targetValueTop As Double = contentTop
        Dim bandTop As Double = contentTop + targetLabelReserve
        Dim bandBottom As Double = bandTop + layout.BandHeight
        Dim scaleTop As Double = bandBottom + appearance.ScaleLabelGap +
                                 If(appearance.ShowScaleTickMarks, appearance.ScaleTickLength, 0.0R)

        Return New RowLayout With {
            .RowTop = rowTop,
            .RowBottom = rowBottom,
            .BandTop = bandTop,
            .BandBottom = bandBottom,
            .BandCentreY = (bandTop + bandBottom) / 2.0R,
            .ScaleLabelTop = scaleTop,
            .TargetValueLabelTop = targetValueTop
        }
    End Function

End Module
Public Module LegacySankeyLayout34
    Public NotInheritable Class DrawingLayout
        Public ChartWidth As Double
        Public ChartHeight As Double
        Public PlotLeft As Double
        Public PlotTop As Double
        Public PlotWidth As Double
        Public PlotHeight As Double
        Public StageCentreLeft As Double
        Public StageCentreRight As Double
        Public LegendLeft As Double
        Public LegendTop As Double
        Public LegendWidth As Double
        Public LegendHeight As Double
        Public LegendColumns As Integer
        Public LegendRowsPerColumn As Integer
        Public FinalLabelReserve As Double
    End Class
    Public NotInheritable Class LegendItem
        Public ColorKey As String
        Public Label As String
        Public Color As Integer
        Public FirstAppearanceIndex As Integer
    End Class
Public Function BuildDrawingLayout(chartWidth As Double, chartHeight As Double, result As SankeyPlotResult, appearance As SankeyPlotAppearance, legendItems As List(Of LegendItem)) As DrawingLayout
        Dim padding As Double = appearance.ChartPadding
        Dim titleHeight As Double = If(String.IsNullOrWhiteSpace(appearance.ChartTitle), 0.0R, appearance.ChartTitleHeight)
        Dim stageLabelHeight As Double = If(appearance.ShowStageLabels, appearance.StageLabelHeight, 0.0R)

        Dim provisionalPlotHeight As Double = chartHeight - 2.0R * padding - titleHeight - stageLabelHeight
        If provisionalPlotHeight < 60.0R Then
            Throw New InvalidOperationException("The embedded chart is too short to draw the Sankey diagram. Increase its height.")
        End If

        Dim legendColumns As Integer = 0
        Dim legendRowsPerColumn As Integer = 0
        Dim legendWidth As Double = 0.0R
        If appearance.ShowLegend AndAlso legendItems.Count > 0 Then
            Dim legendTitleRows As Integer = If(String.IsNullOrWhiteSpace(appearance.LegendTitle), 0, 1)
            Dim availableRowsHeight As Double = provisionalPlotHeight - legendTitleRows * appearance.LegendRowHeight
            legendRowsPerColumn = Math.Max(1, CInt(Math.Floor(availableRowsHeight / appearance.LegendRowHeight)))
            legendColumns = CInt(Math.Ceiling(CDbl(legendItems.Count) / CDbl(legendRowsPerColumn)))
            legendWidth = appearance.LegendGap + legendColumns * appearance.LegendColumnWidth
        End If

        Dim finalLabelReserve As Double = If(appearance.ShowNodeLabels,
                                              appearance.NodeLabelGap + appearance.NodeLabelWidth,
                                              0.0R)

        Dim plotLeft As Double = padding
        Dim plotTop As Double = padding + titleHeight
        Dim plotRight As Double = chartWidth - padding - legendWidth - finalLabelReserve
        Dim plotBottom As Double = chartHeight - padding - stageLabelHeight
        Dim plotWidth As Double = plotRight - plotLeft
        Dim plotHeight As Double = plotBottom - plotTop

        If plotWidth < Math.Max(100.0R, appearance.NodeWidth * result.StageCount * 1.5R) Then
            Throw New InvalidOperationException("The embedded chart is too narrow to draw this Sankey diagram. Increase its width or hide node labels/legend.")
        End If
        If plotHeight < 60.0R Then
            Throw New InvalidOperationException("The embedded chart is too short to draw this Sankey diagram. Increase its height.")
        End If

        Dim layout As New DrawingLayout With {
            .ChartWidth = chartWidth,
            .ChartHeight = chartHeight,
            .PlotLeft = plotLeft,
            .PlotTop = plotTop,
            .PlotWidth = plotWidth,
            .PlotHeight = plotHeight,
            .StageCentreLeft = plotLeft + appearance.NodeWidth / 2.0R,
            .StageCentreRight = plotRight - appearance.NodeWidth / 2.0R,
            .LegendLeft = chartWidth - padding - legendWidth + appearance.LegendGap,
            .LegendTop = plotTop,
            .LegendWidth = Math.Max(0.0R, legendWidth - appearance.LegendGap),
            .LegendHeight = plotHeight,
            .LegendColumns = legendColumns,
            .LegendRowsPerColumn = legendRowsPerColumn,
            .FinalLabelReserve = finalLabelReserve
        }

        If layout.StageCentreRight <= layout.StageCentreLeft Then
            Throw New InvalidOperationException("The chart does not have enough horizontal space for the Sankey nodes.")
        End If
        Return layout
End Function
End Module
End Namespace
