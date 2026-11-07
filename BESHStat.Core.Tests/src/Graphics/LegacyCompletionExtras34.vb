Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace Global.Batch34CompletionFixtures
Public Module LegacyBulletExtras34
Public Function ScaleWidth(tickCount As Integer, plotWidth As Double) As BulletScaleWidthGeometry
Dim nominalWidth As Double
If tickCount <= 1 Then
nominalWidth = 48.0R
Else
nominalWidth = Math.Max(24.0R, Math.Min(64.0R, plotWidth / CDbl(tickCount - 1) * 0.92R))
End If
Return New BulletScaleWidthGeometry(nominalWidth)
End Function

Public Function Tick(tickValue As Double, scaleMaximum As Double, plotLeft As Double, plotWidth As Double, bandBottom As Double, tickLength As Double) As BulletTickGeometry
Dim normalized As Double = If(scaleMaximum <= 0.0R, 0.0R, tickValue / scaleMaximum)
Dim x As Double = BulletMeasuredGeometryCore.MapX(normalized, plotLeft, plotWidth)
Return New BulletTickGeometry(x, bandBottom + tickLength)
End Function

Public Function ScaleLabel(tickIndex As Integer, tickCount As Integer, x As Double, plotLeft As Double, plotWidth As Double, nominalWidth As Double) As BulletScaleLabelGeometry
Dim textLeft As Double
If tickIndex = 0 Then
textLeft = plotLeft
ElseIf tickIndex = tickCount - 1 Then
textLeft = plotLeft + plotWidth - nominalWidth
Else
textLeft = x - nominalWidth / 2.0R
End If
textLeft = Math.Max(plotLeft, Math.Min(plotLeft + plotWidth - nominalWidth, textLeft))
Return New BulletScaleLabelGeometry(textLeft)
End Function

Public Function LeftArea(padding As Double, plotLeft As Double, labelGap As Double) As BulletLeftAreaGeometry
Dim labelRight As Double = plotLeft - labelGap
Return New BulletLeftAreaGeometry(padding, Math.Max(1.0R, labelRight - padding))
End Function

Public Function PairedLabels(bandCentreY As Double, mainFontSize As Double, subtitleFontSize As Double) As BulletPairedLabelsGeometry
Dim mainHeight As Double = Math.Max(12.0R, mainFontSize * 1.35R)
Dim subtitleHeight As Double = Math.Max(10.0R, subtitleFontSize * 1.35R)
Dim totalHeight As Double = mainHeight + subtitleHeight
Dim top As Double = bandCentreY - totalHeight / 2.0R
Return New BulletPairedLabelsGeometry(mainHeight, subtitleHeight, top)
End Function

Public Function SingleLabel(bandCentreY As Double, fontSize As Double) As BulletSingleLabelGeometry
Dim boxHeight As Double = Math.Max(12.0R, fontSize * 1.45R)
Dim top As Double = bandCentreY - boxHeight / 2.0R
Return New BulletSingleLabelGeometry(boxHeight, top)
End Function

Public Function CenteredLabel(centreY As Double, height As Double) As BulletCenteredLabelGeometry

Return New BulletCenteredLabelGeometry(centreY - height / 2.0R)
End Function

Public Function Title(chartWidth As Double, padding As Double) As BulletTitleGeometry

Return New BulletTitleGeometry(Math.Max(1.0R, chartWidth - 2.0R * padding))
End Function

Public Function PaletteIndex(rank As Integer, rangeCount As Integer, paletteLength As Integer) As BulletPaletteIndexGeometry
If paletteLength = 1 OrElse rangeCount <= 1 Then Return New BulletPaletteIndexGeometry(0)
Dim fraction As Double = CDbl(rank) / CDbl(rangeCount - 1)
Dim selectionIndex As Integer = CInt(Math.Round(fraction * CDbl(paletteLength - 1), MidpointRounding.AwayFromZero))
selectionIndex = Math.Max(0, Math.Min(paletteLength - 1, selectionIndex))
Return New BulletPaletteIndexGeometry(selectionIndex)
End Function
End Module
Public NotInheritable Class BulletScaleWidthGeometry
    Public ReadOnly Property Width As Double
    Public Sub New(inputWidth As Double)
        Me.Width = inputWidth
    End Sub
End Class

Public NotInheritable Class BulletTickGeometry
    Public ReadOnly Property X As Double
    Public ReadOnly Property Bottom As Double
    Public Sub New(inputX As Double, inputBottom As Double)
        Me.X = inputX
        Me.Bottom = inputBottom
    End Sub
End Class

Public NotInheritable Class BulletScaleLabelGeometry
    Public ReadOnly Property Left As Double
    Public Sub New(inputLeft As Double)
        Me.Left = inputLeft
    End Sub
End Class

Public NotInheritable Class BulletLeftAreaGeometry
    Public ReadOnly Property Left As Double
    Public ReadOnly Property Width As Double
    Public Sub New(inputLeft As Double, inputWidth As Double)
        Me.Left = inputLeft
        Me.Width = inputWidth
    End Sub
End Class

Public NotInheritable Class BulletPairedLabelsGeometry
    Public ReadOnly Property MainHeight As Double
    Public ReadOnly Property SubtitleHeight As Double
    Public ReadOnly Property Top As Double
    Public Sub New(inputMainHeight As Double, inputSubtitleHeight As Double, inputTop As Double)
        Me.MainHeight = inputMainHeight
        Me.SubtitleHeight = inputSubtitleHeight
        Me.Top = inputTop
    End Sub
End Class

Public NotInheritable Class BulletSingleLabelGeometry
    Public ReadOnly Property Height As Double
    Public ReadOnly Property Top As Double
    Public Sub New(inputHeight As Double, inputTop As Double)
        Me.Height = inputHeight
        Me.Top = inputTop
    End Sub
End Class

Public NotInheritable Class BulletCenteredLabelGeometry
    Public ReadOnly Property Top As Double
    Public Sub New(inputTop As Double)
        Me.Top = inputTop
    End Sub
End Class

Public NotInheritable Class BulletTitleGeometry
    Public ReadOnly Property Width As Double
    Public Sub New(inputWidth As Double)
        Me.Width = inputWidth
    End Sub
End Class

Public NotInheritable Class BulletPaletteIndexGeometry
    Public ReadOnly Property Index As Integer
    Public Sub New(inputIndex As Integer)
        Me.Index = inputIndex
    End Sub
End Class
Public Module LegacySankeyExtras34
Public Function Title(chartWidth As Double, padding As Double, legendWidth As Double, legendLeft As Double) As SankeyTitleGeometry
Dim titleWidth As Double = chartWidth - 2.0R * padding
If legendWidth > 0.0R Then titleWidth = Math.Max(30.0R, legendLeft - padding)
Return New SankeyTitleGeometry(titleWidth)
End Function

Public Function StageLabel(stageCount As Integer, plotWidth As Double, stageLeft As Double, stageRight As Double, plotTop As Double, plotHeight As Double) As SankeyStageLabelGeometry
Dim stepWidth As Double = If(stageCount <= 1, plotWidth, (stageRight - stageLeft) / CDbl(stageCount - 1))
Dim labelWidth As Double = Math.Max(50.0R, Math.Min(150.0R, stepWidth * 0.9R))
Dim top As Double = plotTop + plotHeight + 1.0R
Return New SankeyStageLabelGeometry(labelWidth, top)
End Function

Public Function StageLabelLeft(centre As Double, width As Double) As SankeyStageLabelLeftGeometry

Return New SankeyStageLabelLeftGeometry(centre - width / 2.0R)
End Function

Public Function LegendItem(itemIndex As Integer, rowsPerColumn As Integer, legendLeft As Double, legendTop As Double, titleOffset As Double, columnWidth As Double, rowHeight As Double, swatchSize As Double) As SankeyLegendItemGeometry
Dim column As Integer = itemIndex \ rowsPerColumn
Dim row As Integer = itemIndex Mod rowsPerColumn
Dim itemLeft As Double = legendLeft + column * columnWidth
Dim itemTop As Double = legendTop + titleOffset + row * rowHeight
Dim swatchTop As Double = itemTop + (rowHeight - swatchSize) / 2.0R
Return New SankeyLegendItemGeometry(itemLeft, itemTop, swatchTop)
End Function

Public Function LegendText(itemLeft As Double, swatchSize As Double, columnWidth As Double) As SankeyLegendTextGeometry
Dim textLeft As Double = itemLeft + swatchSize + 4.0R
Dim textWidth As Double = Math.Max(20.0R, columnWidth - swatchSize - 6.0R)
Return New SankeyLegendTextGeometry(textLeft, textWidth)
End Function
End Module
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
End Namespace
