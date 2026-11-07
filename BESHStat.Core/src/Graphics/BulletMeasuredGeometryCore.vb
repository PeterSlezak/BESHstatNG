Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic

Public Module BulletMeasuredGeometryCore
Public Function BulletScaleWidth(tickCount As Integer, plotWidth As Double) As BulletScaleWidthGeometry
Dim nominalWidth As Double
If tickCount <= 1 Then
nominalWidth = 48.0R
Else
nominalWidth = Math.Max(24.0R, Math.Min(64.0R, plotWidth / CDbl(tickCount - 1) * 0.92R))
End If
Return New BulletScaleWidthGeometry(nominalWidth)
End Function

Public Function BulletTick(tickValue As Double, scaleMaximum As Double, plotLeft As Double, plotWidth As Double, bandBottom As Double, tickLength As Double) As BulletTickGeometry
Dim normalized As Double = If(scaleMaximum <= 0.0R, 0.0R, tickValue / scaleMaximum)
Dim x As Double = BulletMeasuredGeometryCore.MapX(normalized, plotLeft, plotWidth)
Return New BulletTickGeometry(x, bandBottom + tickLength)
End Function

Public Function BulletScaleLabel(tickIndex As Integer, tickCount As Integer, x As Double, plotLeft As Double, plotWidth As Double, nominalWidth As Double) As BulletScaleLabelGeometry
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

Public Function BulletLeftArea(padding As Double, plotLeft As Double, labelGap As Double) As BulletLeftAreaGeometry
Dim labelRight As Double = plotLeft - labelGap
Return New BulletLeftAreaGeometry(padding, Math.Max(1.0R, labelRight - padding))
End Function

Public Function BulletPairedLabels(bandCentreY As Double, mainFontSize As Double, subtitleFontSize As Double) As BulletPairedLabelsGeometry
Dim mainHeight As Double = Math.Max(12.0R, mainFontSize * 1.35R)
Dim subtitleHeight As Double = Math.Max(10.0R, subtitleFontSize * 1.35R)
Dim totalHeight As Double = mainHeight + subtitleHeight
Dim top As Double = bandCentreY - totalHeight / 2.0R
Return New BulletPairedLabelsGeometry(mainHeight, subtitleHeight, top)
End Function

Public Function BulletSingleLabel(bandCentreY As Double, fontSize As Double) As BulletSingleLabelGeometry
Dim boxHeight As Double = Math.Max(12.0R, fontSize * 1.45R)
Dim top As Double = bandCentreY - boxHeight / 2.0R
Return New BulletSingleLabelGeometry(boxHeight, top)
End Function

Public Function BulletCenteredLabel(centreY As Double, height As Double) As BulletCenteredLabelGeometry

Return New BulletCenteredLabelGeometry(centreY - height / 2.0R)
End Function

Public Function BulletTitle(chartWidth As Double, padding As Double) As BulletTitleGeometry

Return New BulletTitleGeometry(Math.Max(1.0R, chartWidth - 2.0R * padding))
End Function

Public Function BulletPaletteIndex(rank As Integer, rangeCount As Integer, paletteLength As Integer) As BulletPaletteIndexGeometry
If paletteLength = 1 OrElse rangeCount <= 1 Then Return New BulletPaletteIndexGeometry(0)
Dim fraction As Double = CDbl(rank) / CDbl(rangeCount - 1)
Dim selectionIndex As Integer = CInt(Math.Round(fraction * CDbl(paletteLength - 1), MidpointRounding.AwayFromZero))
selectionIndex = Math.Max(0, Math.Min(paletteLength - 1, selectionIndex))
Return New BulletPaletteIndexGeometry(selectionIndex)
End Function

    Public Function BulletBand(normalizedStart As Double, normalizedEnd As Double, plotLeft As Double, plotWidth As Double) As BulletBandGeometry
        Dim x1 As Double = MapX(normalizedStart, plotLeft, plotWidth)
            Dim x2 As Double = MapX(normalizedEnd, plotLeft, plotWidth)
            Dim bandWidth As Double = Math.Max(0.5R, x2 - x1)

        Return New BulletBandGeometry(x1, x2, bandWidth)
    End Function

    Public Function BulletActual(actualNormalized As Double, bandHeight As Double, barHeightFraction As Double, bandCentreY As Double, plotWidth As Double) As BulletActualGeometry
        Dim normalized As Double = Clip01(actualNormalized)
        If normalized <= 0.0R Then Return Nothing
        Dim barHeight As Double = Math.Max(1.0R, bandHeight * barHeightFraction)
        Dim barTop As Double = bandCentreY - barHeight / 2.0R
        Dim barWidth As Double = Math.Max(0.5R, normalized * plotWidth)

        Return New BulletActualGeometry(barHeight, barTop, barWidth)
    End Function

    Public Function BulletTarget(targetNormalized As Double, plotLeft As Double, plotWidth As Double, bandHeight As Double, heightFraction As Double, bandCentreY As Double) As BulletTargetGeometry
        Dim x As Double = MapX(targetNormalized, plotLeft, plotWidth)
        Dim markerHeight As Double = Math.Max(2.0R,
                                             bandHeight * heightFraction)
        Dim y1 As Double = bandCentreY - markerHeight / 2.0R
        Dim y2 As Double = bandCentreY + markerHeight / 2.0R

        Return New BulletTargetGeometry(x, y1, y2)
    End Function

    Public Function BulletActualLabel(actualNormalized As Double, plotLeft As Double, plotWidth As Double, valueLabelWidth As Double, valueLabelGap As Double) As BulletActualLabelGeometry
        Dim actualX As Double = MapX(actualNormalized, plotLeft, plotWidth)
        Dim labelWidth As Double = Math.Min(valueLabelWidth, plotWidth)
        Dim left As Double
        Dim alignment As Integer

        If actualX + valueLabelGap + labelWidth <= plotLeft + plotWidth Then
            left = actualX + valueLabelGap
            alignment = 0
        Else
            left = Math.Max(plotLeft, actualX - valueLabelGap - labelWidth)
            alignment = 1
        End If

        Return New BulletActualLabelGeometry(left, labelWidth, alignment = 1)
    End Function

    Public Function BulletTargetLabel(targetNormalized As Double, plotLeft As Double, plotWidth As Double, valueLabelWidth As Double) As BulletTargetLabelGeometry
        Dim targetX As Double = MapX(targetNormalized, plotLeft, plotWidth)
        Dim labelWidth As Double = Math.Min(valueLabelWidth, plotWidth)
        Dim left As Double = targetX - labelWidth / 2.0R
        left = Math.Max(plotLeft,
                        Math.Min(plotLeft + plotWidth - labelWidth, left))

        Return New BulletTargetLabelGeometry(left, labelWidth)
    End Function

    Public Function ComputeRow(layout As BulletDrawingLayout, rowIndex As Integer, ShowScaleLabels As Boolean, ShowScaleTickMarks As Boolean, ScaleLabelGap As Double, ScaleTickLength As Double, ScaleLabelHeight As Double, ShowTargetValueLabels As Boolean, ValueLabelHeight As Double, ValueLabelGap As Double) As BulletRowLayout
        Dim rowTop As Double = layout.PlotTop + CDbl(rowIndex) * layout.RowHeight
        Dim rowBottom As Double = rowTop + layout.RowHeight

        Dim scaleReserve As Double = 0.0R
        If ShowScaleLabels OrElse ShowScaleTickMarks Then
            scaleReserve = ScaleLabelGap
            If ShowScaleTickMarks Then scaleReserve += ScaleTickLength
            If ShowScaleLabels Then scaleReserve += ScaleLabelHeight
        End If
        Dim targetLabelReserve As Double = If(ShowTargetValueLabels,
                                              ValueLabelHeight + ValueLabelGap,
                                              0.0R)
        Dim contentHeight As Double = targetLabelReserve + layout.BandHeight + scaleReserve
        Dim contentTop As Double = rowTop + (layout.RowHeight - contentHeight) / 2.0R

        Dim targetValueTop As Double = contentTop
        Dim bandTop As Double = contentTop + targetLabelReserve
        Dim bandBottom As Double = bandTop + layout.BandHeight
        Dim scaleTop As Double = bandBottom + ScaleLabelGap +
                                 If(ShowScaleTickMarks, ScaleTickLength, 0.0R)

        Return New BulletRowLayout(rowTop, rowBottom, bandTop, bandBottom, (bandTop + bandBottom) / 2.0R, scaleTop, targetValueTop)
    End Function

    Private Const MinimumPlotWidth As Double = 120.0R
    Private Const MinimumBandHeight As Double = 8.0R
    Private Const MinimumRowHeight As Double = 20.0R
    Public Function ComputeLayout(chartWidth As Double, chartHeight As Double, itemCount As Integer, hasSubtitles As Boolean, ChartPadding As Double, ChartTitle As String, ChartTitleHeight As Double, ShowMeasureLabels As Boolean, ShowSubtitles As Boolean, LabelAreaWidth As Double, LabelGap As Double, ShowScaleLabels As Boolean, ShowScaleTickMarks As Boolean, ScaleLabelGap As Double, ScaleTickLength As Double, ScaleLabelHeight As Double, ShowTargetValueLabels As Boolean, ValueLabelHeight As Double, ValueLabelGap As Double, RowGap As Double, QualitativeBandHeight As Double) As BulletDrawingLayout
        Dim padding As Double = ChartPadding
        Dim titleHeight As Double = If(String.IsNullOrWhiteSpace(ChartTitle), 0.0R, ChartTitleHeight)
        Dim leftLabelsVisible As Boolean = ShowMeasureLabels OrElse
                                           (ShowSubtitles AndAlso hasSubtitles)
        Dim labelReserve As Double = If(leftLabelsVisible,
                                        LabelAreaWidth + LabelGap,
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

        Dim rowHeight As Double = plotHeight / CDbl(itemCount)
        If rowHeight < MinimumRowHeight Then
            Throw New InvalidOperationException("The embedded chart is too short for the number of bullet-chart items. Increase its height.")
        End If

        Dim scaleReserve As Double = 0.0R
        If ShowScaleLabels OrElse ShowScaleTickMarks Then
            scaleReserve = ScaleLabelGap
            If ShowScaleTickMarks Then scaleReserve += ScaleTickLength
            If ShowScaleLabels Then scaleReserve += ScaleLabelHeight
        End If
        Dim targetLabelReserve As Double = If(ShowTargetValueLabels,
                                              ValueLabelHeight + ValueLabelGap,
                                              0.0R)

        Dim bandAvailable As Double = rowHeight - RowGap - scaleReserve - targetLabelReserve
        If bandAvailable < MinimumBandHeight Then
            Throw New InvalidOperationException("The embedded chart is too short for the selected bullet-chart labels and spacing. Increase its height or hide value/scale labels.")
        End If

        Return New BulletDrawingLayout(chartWidth, chartHeight, plotLeft, plotTop, plotWidth, plotHeight, rowHeight, Math.Min(QualitativeBandHeight, bandAvailable), leftLabelsVisible)
    End Function

Public Function MapX(value As Double, plotLeft As Double, plotWidth As Double) As Double
        Return plotLeft + Clip01(value) * plotWidth
End Function

Public Function Clip01(value As Double) As Double
        If value < 0.0R Then Return 0.0R
        If value > 1.0R Then Return 1.0R
        Return value
End Function
End Module

Public NotInheritable Class BulletDrawingLayout
    Public ReadOnly Property ChartWidth As Double
    Public ReadOnly Property ChartHeight As Double
    Public ReadOnly Property PlotLeft As Double
    Public ReadOnly Property PlotTop As Double
    Public ReadOnly Property PlotWidth As Double
    Public ReadOnly Property PlotHeight As Double
    Public ReadOnly Property RowHeight As Double
    Public ReadOnly Property BandHeight As Double
    Public ReadOnly Property LeftLabelsVisible As Boolean
    Public Sub New(valueChartWidth As Double, valueChartHeight As Double, valuePlotLeft As Double, valuePlotTop As Double, valuePlotWidth As Double, valuePlotHeight As Double, valueRowHeight As Double, valueBandHeight As Double, valueLeftLabelsVisible As Boolean)
        Me.ChartWidth = valueChartWidth
        Me.ChartHeight = valueChartHeight
        Me.PlotLeft = valuePlotLeft
        Me.PlotTop = valuePlotTop
        Me.PlotWidth = valuePlotWidth
        Me.PlotHeight = valuePlotHeight
        Me.RowHeight = valueRowHeight
        Me.BandHeight = valueBandHeight
        Me.LeftLabelsVisible = valueLeftLabelsVisible
    End Sub
End Class
Public NotInheritable Class BulletRowLayout
    Public ReadOnly Property RowTop As Double
    Public ReadOnly Property RowBottom As Double
    Public ReadOnly Property BandTop As Double
    Public ReadOnly Property BandBottom As Double
    Public ReadOnly Property BandCentreY As Double
    Public ReadOnly Property ScaleLabelTop As Double
    Public ReadOnly Property TargetValueLabelTop As Double
    Public Sub New(valueRowTop As Double, valueRowBottom As Double, valueBandTop As Double, valueBandBottom As Double, valueBandCentreY As Double, valueScaleLabelTop As Double, valueTargetValueLabelTop As Double)
        Me.RowTop = valueRowTop
        Me.RowBottom = valueRowBottom
        Me.BandTop = valueBandTop
        Me.BandBottom = valueBandBottom
        Me.BandCentreY = valueBandCentreY
        Me.ScaleLabelTop = valueScaleLabelTop
        Me.TargetValueLabelTop = valueTargetValueLabelTop
    End Sub
End Class

Public NotInheritable Class BulletBandGeometry
    Public ReadOnly Property X1 As Double
    Public ReadOnly Property X2 As Double
    Public ReadOnly Property Width As Double
    Public Sub New(inputX1 As Double, inputX2 As Double, inputWidth As Double)
        Me.X1 = inputX1
        Me.X2 = inputX2
        Me.Width = inputWidth
    End Sub
End Class

Public NotInheritable Class BulletActualGeometry
    Public ReadOnly Property Height As Double
    Public ReadOnly Property Top As Double
    Public ReadOnly Property Width As Double
    Public Sub New(inputHeight As Double, inputTop As Double, inputWidth As Double)
        Me.Height = inputHeight
        Me.Top = inputTop
        Me.Width = inputWidth
    End Sub
End Class

Public NotInheritable Class BulletTargetGeometry
    Public ReadOnly Property X As Double
    Public ReadOnly Property Y1 As Double
    Public ReadOnly Property Y2 As Double
    Public Sub New(inputX As Double, inputY1 As Double, inputY2 As Double)
        Me.X = inputX
        Me.Y1 = inputY1
        Me.Y2 = inputY2
    End Sub
End Class

Public NotInheritable Class BulletActualLabelGeometry
    Public ReadOnly Property Left As Double
    Public ReadOnly Property Width As Double
    Public ReadOnly Property RightAligned As Boolean
    Public Sub New(inputLeft As Double, inputWidth As Double, inputRightAligned As Boolean)
        Me.Left = inputLeft
        Me.Width = inputWidth
        Me.RightAligned = inputRightAligned
    End Sub
End Class

Public NotInheritable Class BulletTargetLabelGeometry
    Public ReadOnly Property Left As Double
    Public ReadOnly Property Width As Double
    Public Sub New(inputLeft As Double, inputWidth As Double)
        Me.Left = inputLeft
        Me.Width = inputWidth
    End Sub
End Class

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
    Public ReadOnly Property SubtitleTop As Double
        Get
            Return Top + MainHeight
        End Get
    End Property
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
