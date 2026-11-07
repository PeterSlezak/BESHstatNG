Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace Global.Batch34CompletionFixtures
Public Module LegacyBulletShapes34
Private Function MapX(value As Double, plotLeft As Double, plotWidth As Double) As Double
Return BESHStatNG.BulletMeasuredGeometryCore.MapX(value, plotLeft, plotWidth)
End Function
Private Function Clip01(value As Double) As Double
Return BESHStatNG.BulletMeasuredGeometryCore.Clip01(value)
End Function
    Public Function Band(normalizedStart As Double, normalizedEnd As Double, plotLeft As Double, plotWidth As Double) As BulletBandGeometry
        Dim x1 As Double = MapX(normalizedStart, plotLeft, plotWidth)
            Dim x2 As Double = MapX(normalizedEnd, plotLeft, plotWidth)
            Dim bandWidth As Double = Math.Max(0.5R, x2 - x1)

        Return New BulletBandGeometry(x1, x2, bandWidth)
    End Function

    Public Function Actual(actualNormalized As Double, bandHeight As Double, barHeightFraction As Double, bandCentreY As Double, plotWidth As Double) As BulletActualGeometry
        Dim normalized As Double = Clip01(actualNormalized)
        If normalized <= 0.0R Then Return Nothing
        Dim barHeight As Double = Math.Max(1.0R, bandHeight * barHeightFraction)
        Dim barTop As Double = bandCentreY - barHeight / 2.0R
        Dim barWidth As Double = Math.Max(0.5R, normalized * plotWidth)

        Return New BulletActualGeometry(barHeight, barTop, barWidth)
    End Function

    Public Function Target(targetNormalized As Double, plotLeft As Double, plotWidth As Double, bandHeight As Double, heightFraction As Double, bandCentreY As Double) As BulletTargetGeometry
        Dim x As Double = MapX(targetNormalized, plotLeft, plotWidth)
        Dim markerHeight As Double = Math.Max(2.0R,
                                             bandHeight * heightFraction)
        Dim y1 As Double = bandCentreY - markerHeight / 2.0R
        Dim y2 As Double = bandCentreY + markerHeight / 2.0R

        Return New BulletTargetGeometry(x, y1, y2)
    End Function

    Public Function ActualLabel(actualNormalized As Double, plotLeft As Double, plotWidth As Double, valueLabelWidth As Double, valueLabelGap As Double) As BulletActualLabelGeometry
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

    Public Function TargetLabel(targetNormalized As Double, plotLeft As Double, plotWidth As Double, valueLabelWidth As Double) As BulletTargetLabelGeometry
        Dim targetX As Double = MapX(targetNormalized, plotLeft, plotWidth)
        Dim labelWidth As Double = Math.Min(valueLabelWidth, plotWidth)
        Dim left As Double = targetX - labelWidth / 2.0R
        left = Math.Max(plotLeft,
                        Math.Min(plotLeft + plotWidth - labelWidth, left))

        Return New BulletTargetLabelGeometry(left, labelWidth)
    End Function
End Module
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
Public Module LegacySankeyShapes34
Private Function MapX(value As Double, stageLeft As Double, stageRight As Double) As Double
Return BESHStatNG.SankeyMeasuredGeometryCore.MapX(value, stageLeft, stageRight)
End Function
Private Function MapY(value As Double, plotTop As Double, plotHeight As Double) As Double
Return BESHStatNG.SankeyMeasuredGeometryCore.MapY(value, plotTop, plotHeight)
End Function
    Public Function Ribbon(sourceNodeX As Double, targetNodeX As Double, nodeWidth As Double, sourceTopValue As Double, targetTopValue As Double, thickness As Double, curvature As Double, stageLeft As Double, stageRight As Double, plotTop As Double, plotHeight As Double) As SankeyRibbonGeometry
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

    Public Function Node(nodeX As Double, nodeY As Double, nodeHeightValue As Double, nodeWidth As Double, stageLeft As Double, stageRight As Double, plotTop As Double, plotHeight As Double) As SankeyNodeGeometry
        Dim centreX As Double = MapX(nodeX, stageLeft, stageRight)
        Dim nodeTop As Double = MapY(nodeY, plotTop, plotHeight)
        Dim nodeHeight As Double = Math.Max(0.5R, nodeHeightValue * plotHeight)
        Dim nodeLeft As Double = centreX - nodeWidth / 2.0R

        Return New SankeyNodeGeometry(nodeLeft, nodeTop, nodeHeight)
    End Function

    Public Function NodeLabel(nodeX As Double, nodeY As Double, nodeHeightValue As Double, nodeStageIndex As Integer, stageX As Double(), nodeWidth As Double, labelGap As Double, labelWidthValue As Double, fontSize As Double, stageLeft As Double, stageRight As Double, plotTop As Double, plotHeight As Double) As SankeyNodeLabelGeometry
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
End Module
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
End Namespace
