Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic

Public Module SankeyMeasuredGeometryCore
Public Function SankeyTitle(chartWidth As Double, padding As Double, legendWidth As Double, legendLeft As Double) As SankeyTitleGeometry
Dim titleWidth As Double = chartWidth - 2.0R * padding
If legendWidth > 0.0R Then titleWidth = Math.Max(30.0R, legendLeft - padding)
Return New SankeyTitleGeometry(titleWidth)
End Function

Public Function SankeyStageLabel(stageCount As Integer, plotWidth As Double, stageLeft As Double, stageRight As Double, plotTop As Double, plotHeight As Double) As SankeyStageLabelGeometry
Dim stepWidth As Double = If(stageCount <= 1, plotWidth, (stageRight - stageLeft) / CDbl(stageCount - 1))
Dim labelWidth As Double = Math.Max(50.0R, Math.Min(150.0R, stepWidth * 0.9R))
Dim top As Double = plotTop + plotHeight + 1.0R
Return New SankeyStageLabelGeometry(labelWidth, top)
End Function

Public Function SankeyStageLabelLeft(centre As Double, width As Double) As SankeyStageLabelLeftGeometry

Return New SankeyStageLabelLeftGeometry(centre - width / 2.0R)
End Function

Public Function SankeyLegendItem(itemIndex As Integer, rowsPerColumn As Integer, legendLeft As Double, legendTop As Double, titleOffset As Double, columnWidth As Double, rowHeight As Double, swatchSize As Double) As SankeyLegendItemGeometry
Dim column As Integer = itemIndex \ rowsPerColumn
Dim row As Integer = itemIndex Mod rowsPerColumn
Dim itemLeft As Double = legendLeft + column * columnWidth
Dim itemTop As Double = legendTop + titleOffset + row * rowHeight
Dim swatchTop As Double = itemTop + (rowHeight - swatchSize) / 2.0R
Return New SankeyLegendItemGeometry(itemLeft, itemTop, swatchTop)
End Function

Public Function SankeyLegendText(itemLeft As Double, swatchSize As Double, columnWidth As Double) As SankeyLegendTextGeometry
Dim textLeft As Double = itemLeft + swatchSize + 4.0R
Dim textWidth As Double = Math.Max(20.0R, columnWidth - swatchSize - 6.0R)
Return New SankeyLegendTextGeometry(textLeft, textWidth)
End Function

    Public Function SankeyRibbon(sourceNodeX As Double, targetNodeX As Double, nodeWidth As Double, sourceTopValue As Double, targetTopValue As Double, thickness As Double, curvature As Double, stageLeft As Double, stageRight As Double, plotTop As Double, plotHeight As Double) As SankeyRibbonGeometry
        Dim sourceCentreX As Double = MapX(sourceNodeX, stageLeft, stageRight)
        Dim targetCentreX As Double = MapX(targetNodeX, stageLeft, stageRight)
        Dim sourceX As Double = sourceCentreX + nodeWidth / 2.0R
        Dim targetX As Double = targetCentreX - nodeWidth / 2.0R

        If targetX <= sourceX Then
            'The backend guarantees forward stages; this guard protects against an
            'impossibly narrow chart after aggressive manual resizing.
            Return Nothing
        End If

        Dim sourceTop As Double = MapY(sourceTopValue, plotTop, plotHeight)
        Dim sourceBottom As Double = MapY(sourceTopValue + thickness, plotTop, plotHeight)
        Dim targetTop As Double = MapY(targetTopValue, plotTop, plotHeight)
        Dim targetBottom As Double = MapY(targetTopValue + thickness, plotTop, plotHeight)

        Dim dx As Double = targetX - sourceX
        Dim controlOffset As Double = dx * curvature
        Dim c1x As Double = sourceX + controlOffset
        Dim c2x As Double = targetX - controlOffset

        Return New SankeyRibbonGeometry(sourceX, targetX, sourceTop, sourceBottom, targetTop, targetBottom, c1x, c2x)
    End Function

    Public Function SankeyNode(nodeX As Double, nodeY As Double, nodeHeightValue As Double, nodeWidth As Double, stageLeft As Double, stageRight As Double, plotTop As Double, plotHeight As Double) As SankeyNodeGeometry
        Dim centreX As Double = MapX(nodeX, stageLeft, stageRight)
        Dim nodeTop As Double = MapY(nodeY, plotTop, plotHeight)
        Dim nodeHeight As Double = Math.Max(0.5R, nodeHeightValue * plotHeight)
        Dim nodeLeft As Double = centreX - nodeWidth / 2.0R

        Return New SankeyNodeGeometry(nodeLeft, nodeTop, nodeHeight)
    End Function

    Public Function SankeyNodeLabel(nodeX As Double, nodeY As Double, nodeHeightValue As Double, nodeStageIndex As Integer, stageX As Double(), nodeWidth As Double, labelGap As Double, labelWidthValue As Double, fontSize As Double, stageLeft As Double, stageRight As Double, plotTop As Double, plotHeight As Double) As SankeyNodeLabelGeometry
        Dim centreX As Double = MapX(nodeX, stageLeft, stageRight)
        Dim nodeRight As Double = centreX + nodeWidth / 2.0R
        Dim labelLeft As Double = nodeRight + labelGap
        Dim labelWidth As Double = labelWidthValue

        If nodeStageIndex < stageX.Length - 1 Then
            Dim nextNodeLeft As Double = stageX(nodeStageIndex + 1) - nodeWidth / 2.0R
            Dim interStageWidth As Double = nextNodeLeft - labelLeft - 3.0R
            If interStageWidth > 20.0R Then
                labelWidth = Math.Min(labelWidth, interStageWidth)
            Else
                labelWidth = Math.Max(20.0R, interStageWidth)
            End If
        End If

        Dim nodeTop As Double = MapY(nodeY, plotTop, plotHeight)
        Dim nodeHeight As Double = Math.Max(0.5R, nodeHeightValue * plotHeight)
        Dim centreY As Double = nodeTop + nodeHeight / 2.0R
        Dim labelHeight As Double = Math.Max(18.0R, fontSize * 2.5R)
        Dim labelTop As Double = Math.Max(plotTop,
                                         Math.Min(plotTop + plotHeight - labelHeight,
                                                  centreY - labelHeight / 2.0R))

        Return New SankeyNodeLabelGeometry(labelLeft, labelTop, labelWidth, labelHeight)
    End Function


    Public Function ComputeLayout(chartWidth As Double, chartHeight As Double, stageCount As Integer, legendItemCount As Integer, ChartPadding As Double, ChartTitle As String, ChartTitleHeight As Double, ShowStageLabels As Boolean, inputStageLabelHeight As Double, ShowLegend As Boolean, LegendTitle As String, LegendRowHeight As Double, LegendGap As Double, LegendColumnWidth As Double, ShowNodeLabels As Boolean, NodeLabelGap As Double, NodeLabelWidth As Double, NodeWidth As Double) As SankeyDrawingLayout
        Dim padding As Double = ChartPadding
        Dim titleHeight As Double = If(String.IsNullOrWhiteSpace(ChartTitle), 0.0R, ChartTitleHeight)
        Dim stageLabelHeight As Double = If(ShowStageLabels, inputStageLabelHeight, 0.0R)

        Dim provisionalPlotHeight As Double = chartHeight - 2.0R * padding - titleHeight - stageLabelHeight
        If provisionalPlotHeight < 60.0R Then
            Throw New InvalidOperationException("The embedded chart is too short to draw the Sankey diagram. Increase its height.")
        End If

        Dim legendColumns As Integer = 0
        Dim legendRowsPerColumn As Integer = 0
        Dim legendWidth As Double = 0.0R
        If ShowLegend AndAlso legendItemCount > 0 Then
            Dim legendTitleRows As Integer = If(String.IsNullOrWhiteSpace(LegendTitle), 0, 1)
            Dim availableRowsHeight As Double = provisionalPlotHeight - legendTitleRows * LegendRowHeight
            legendRowsPerColumn = Math.Max(1, CInt(Math.Floor(availableRowsHeight / LegendRowHeight)))
            legendColumns = CInt(Math.Ceiling(CDbl(legendItemCount) / CDbl(legendRowsPerColumn)))
            legendWidth = LegendGap + legendColumns * LegendColumnWidth
        End If

        Dim finalLabelReserve As Double = If(ShowNodeLabels,
                                              NodeLabelGap + NodeLabelWidth,
                                              0.0R)

        Dim plotLeft As Double = padding
        Dim plotTop As Double = padding + titleHeight
        Dim plotRight As Double = chartWidth - padding - legendWidth - finalLabelReserve
        Dim plotBottom As Double = chartHeight - padding - stageLabelHeight
        Dim plotWidth As Double = plotRight - plotLeft
        Dim plotHeight As Double = plotBottom - plotTop

        If plotWidth < Math.Max(100.0R, NodeWidth * stageCount * 1.5R) Then
            Throw New InvalidOperationException("The embedded chart is too narrow to draw this Sankey diagram. Increase its width or hide node labels/legend.")
        End If
        If plotHeight < 60.0R Then
            Throw New InvalidOperationException("The embedded chart is too short to draw this Sankey diagram. Increase its height.")
        End If

        Dim layout As SankeyDrawingLayout = New SankeyDrawingLayout(chartWidth, chartHeight, plotLeft, plotTop, plotWidth, plotHeight, plotLeft + NodeWidth / 2.0R, plotRight - NodeWidth / 2.0R, chartWidth - padding - legendWidth + LegendGap, plotTop, Math.Max(0.0R, legendWidth - LegendGap), plotHeight, legendColumns, legendRowsPerColumn, finalLabelReserve)

        If layout.StageCentreRight <= layout.StageCentreLeft Then
            Throw New InvalidOperationException("The chart does not have enough horizontal space for the Sankey nodes.")
        End If
        Return layout
    End Function

Public Function MapX(value As Double, stageLeft As Double, stageRight As Double) As Double
        Dim clipped As Double = Math.Max(0.0R, Math.Min(1.0R, value))
        Return stageLeft + clipped * (stageRight - stageLeft)
End Function

Public Function MapY(value As Double, plotTop As Double, plotHeight As Double) As Double
        Dim clipped As Double = Math.Max(0.0R, Math.Min(1.0R, value))
        Return plotTop + clipped * plotHeight
End Function

Public Function StageCentres(stageCount As Integer, stageLeft As Double, stageRight As Double) As Double()
        Dim result(stageCount - 1) As Double
        If stageCount = 1 Then
            result(0) = (stageLeft + stageRight) / 2.0R
            Return result
        End If

        For i As Integer = 0 To stageCount - 1
            result(i) = stageLeft +
                        CDbl(i) / CDbl(stageCount - 1) *
                        (stageRight - stageLeft)
        Next
        Return result
End Function
End Module

Public NotInheritable Class SankeyDrawingLayout
    Public ReadOnly Property ChartWidth As Double
    Public ReadOnly Property ChartHeight As Double
    Public ReadOnly Property PlotLeft As Double
    Public ReadOnly Property PlotTop As Double
    Public ReadOnly Property PlotWidth As Double
    Public ReadOnly Property PlotHeight As Double
    Public ReadOnly Property StageCentreLeft As Double
    Public ReadOnly Property StageCentreRight As Double
    Public ReadOnly Property LegendLeft As Double
    Public ReadOnly Property LegendTop As Double
    Public ReadOnly Property LegendWidth As Double
    Public ReadOnly Property LegendHeight As Double
    Public ReadOnly Property LegendColumns As Integer
    Public ReadOnly Property LegendRowsPerColumn As Integer
    Public ReadOnly Property FinalLabelReserve As Double
    Public Sub New(valueChartWidth As Double, valueChartHeight As Double, valuePlotLeft As Double, valuePlotTop As Double, valuePlotWidth As Double, valuePlotHeight As Double, valueStageCentreLeft As Double, valueStageCentreRight As Double, valueLegendLeft As Double, valueLegendTop As Double, valueLegendWidth As Double, valueLegendHeight As Double, valueLegendColumns As Integer, valueLegendRowsPerColumn As Integer, valueFinalLabelReserve As Double)
        Me.ChartWidth = valueChartWidth
        Me.ChartHeight = valueChartHeight
        Me.PlotLeft = valuePlotLeft
        Me.PlotTop = valuePlotTop
        Me.PlotWidth = valuePlotWidth
        Me.PlotHeight = valuePlotHeight
        Me.StageCentreLeft = valueStageCentreLeft
        Me.StageCentreRight = valueStageCentreRight
        Me.LegendLeft = valueLegendLeft
        Me.LegendTop = valueLegendTop
        Me.LegendWidth = valueLegendWidth
        Me.LegendHeight = valueLegendHeight
        Me.LegendColumns = valueLegendColumns
        Me.LegendRowsPerColumn = valueLegendRowsPerColumn
        Me.FinalLabelReserve = valueFinalLabelReserve
    End Sub
End Class

Public NotInheritable Class SankeyRibbonGeometry
    Public ReadOnly Property SourceX As Double
    Public ReadOnly Property TargetX As Double
    Public ReadOnly Property SourceTop As Double
    Public ReadOnly Property SourceBottom As Double
    Public ReadOnly Property TargetTop As Double
    Public ReadOnly Property TargetBottom As Double
    Public ReadOnly Property C1X As Double
    Public ReadOnly Property C2X As Double
    Public Sub New(inputSourceX As Double, inputTargetX As Double, inputSourceTop As Double, inputSourceBottom As Double, inputTargetTop As Double, inputTargetBottom As Double, inputC1X As Double, inputC2X As Double)
        Me.SourceX = inputSourceX
        Me.TargetX = inputTargetX
        Me.SourceTop = inputSourceTop
        Me.SourceBottom = inputSourceBottom
        Me.TargetTop = inputTargetTop
        Me.TargetBottom = inputTargetBottom
        Me.C1X = inputC1X
        Me.C2X = inputC2X
    End Sub
End Class

Public NotInheritable Class SankeyNodeGeometry
    Public ReadOnly Property Left As Double
    Public ReadOnly Property Top As Double
    Public ReadOnly Property Height As Double
    Public Sub New(inputLeft As Double, inputTop As Double, inputHeight As Double)
        Me.Left = inputLeft
        Me.Top = inputTop
        Me.Height = inputHeight
    End Sub
End Class

Public NotInheritable Class SankeyNodeLabelGeometry
    Public ReadOnly Property Left As Double
    Public ReadOnly Property Top As Double
    Public ReadOnly Property Width As Double
    Public ReadOnly Property Height As Double
    Public Sub New(inputLeft As Double, inputTop As Double, inputWidth As Double, inputHeight As Double)
        Me.Left = inputLeft
        Me.Top = inputTop
        Me.Width = inputWidth
        Me.Height = inputHeight
    End Sub
End Class

Public NotInheritable Class SankeyTitleGeometry
    Public ReadOnly Property Width As Double
    Public Sub New(inputWidth As Double)
        Me.Width = inputWidth
    End Sub
End Class

Public NotInheritable Class SankeyStageLabelGeometry
    Public ReadOnly Property Width As Double
    Public ReadOnly Property Top As Double
    Public Sub New(inputWidth As Double, inputTop As Double)
        Me.Width = inputWidth
        Me.Top = inputTop
    End Sub
End Class

Public NotInheritable Class SankeyStageLabelLeftGeometry
    Public ReadOnly Property Left As Double
    Public Sub New(inputLeft As Double)
        Me.Left = inputLeft
    End Sub
End Class

Public NotInheritable Class SankeyLegendItemGeometry
    Public ReadOnly Property Left As Double
    Public ReadOnly Property Top As Double
    Public ReadOnly Property SwatchTop As Double
    Public Sub New(inputLeft As Double, inputTop As Double, inputSwatchTop As Double)
        Me.Left = inputLeft
        Me.Top = inputTop
        Me.SwatchTop = inputSwatchTop
    End Sub
End Class

Public NotInheritable Class SankeyLegendTextGeometry
    Public ReadOnly Property Left As Double
    Public ReadOnly Property Width As Double
    Public Sub New(inputLeft As Double, inputWidth As Double)
        Me.Left = inputLeft
        Me.Width = inputWidth
    End Sub
End Class
