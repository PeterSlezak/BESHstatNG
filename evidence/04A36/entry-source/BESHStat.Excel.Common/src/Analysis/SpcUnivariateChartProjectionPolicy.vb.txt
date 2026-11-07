Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    ''' <summary>Ordered existing Windows chart channels; values come solely from Core points.</summary>
    Public Enum SpcChartChannel
        Statistic
        CenterLine
        LowerControlLimit
        UpperControlLimit
        LowerOneSigma
        UpperOneSigma
        LowerTwoSigma
        UpperTwoSigma
        Signal
        Excluded
    End Enum

    ''' <summary>A typed category and its immutable, first-encountered Core identity.</summary>
    Public NotInheritable Class SpcChartAxisPoint
        Friend Sub New(point As SpcPointResult, useSequence As Boolean)
            Me.Point = point
            Me.IsSequenceCategory = useSequence AndAlso point.SequenceValue.HasValue
        End Sub
        Public ReadOnly Property Point As SpcPointResult
        Public ReadOnly Property IsSequenceCategory As Boolean
    End Class

    ''' <summary>Semantic stage geometry; native plot-area coordinates stay with the host.</summary>
    Public NotInheritable Class SpcStageBoundaryIntent
        Friend Sub New(ordinal As Integer, count As Integer, previousStage As String, nextStage As String)
            Me.Ordinal = ordinal
            Me.Count = count
            Me.Fraction = CDbl(ordinal) / CDbl(count)
            Me.PreviousStageId = previousStage
            Me.NextStageId = nextStage
            Me.AlternativeText = "Stage boundary: " & previousStage & " to " & nextStage
        End Sub
        Public ReadOnly Property Ordinal As Integer
        Public ReadOnly Property Count As Integer
        Public ReadOnly Property Fraction As Double
        Public ReadOnly Property PreviousStageId As String
        Public ReadOnly Property NextStageId As String
        Public ReadOnly Property AlternativeText As String
    End Class

    Public NotInheritable Class SpcChartLabelIntent
        Friend Sub New(seriesIndex As Integer, text As String)
            Me.SeriesIndex = seriesIndex
            Me.Text = text
        End Sub
        ''' <summary>Zero-based aligned series slot, distinct from Core index and physical rows.</summary>
        Public ReadOnly Property SeriesIndex As Integer
        Public ReadOnly Property Text As String
    End Class

    ''' <summary>Detached finite samples. Nothing means a blank; Points retain absence/NaN distinctions.</summary>
    Public NotInheritable Class SpcPanelSeriesProjection
        Friend Sub New(panel As SpcPanelResult, axis As IReadOnlyList(Of SpcChartAxisPoint))
            Me.Panel = panel
            Dim byIndex As New Dictionary(Of Integer, SpcPointResult)
            For Each point In panel.Points
                byIndex.Add(point.PointIndex, point)
            Next
            Dim aligned(axis.Count - 1) As SpcPointResult
            Dim channels As New List(Of IReadOnlyList(Of Double?))
            Dim samples(9)() As Double?
            For c As Integer = 0 To 9
                samples(c) = New Double?(axis.Count - 1) {}
                channels.Add(Array.AsReadOnly(samples(c)))
            Next
            Dim pointLabels As New List(Of SpcChartLabelIntent)
            Dim signalLabels As New List(Of SpcChartLabelIntent)
            Dim exclusionLabels As New List(Of SpcChartLabelIntent)
            For i As Integer = 0 To axis.Count - 1
                Dim point As SpcPointResult = Nothing
                If Not byIndex.TryGetValue(axis(i).Point.PointIndex, point) Then Continue For
                aligned(i) = point
                Dim values As Double() = {point.Value, point.CenterLine, point.LowerControlLimit,
                    point.UpperControlLimit, point.LowerOneSigmaLimit, point.UpperOneSigmaLimit,
                    point.LowerTwoSigmaLimit, point.UpperTwoSigmaLimit}
                For c As Integer = 0 To values.Length - 1
                    If IsFinite(values(c)) Then samples(c)(i) = values(c)
                Next
                If point.IsSignalled Then samples(CInt(SpcChartChannel.Signal))(i) = samples(0)(i)
                If point.IsExplicitlyExcluded Then samples(CInt(SpcChartChannel.Excluded))(i) = samples(0)(i)
                If point.HasFiniteValue Then
                    AddLabel(pointLabels, i, point.Label)
                    If point.IsSignalled Then AddLabel(signalLabels, i, RuleText(point.SignalRuleNumbers))
                    If point.IsExplicitlyExcluded Then AddLabel(exclusionLabels, i, If(point.ExclusionReason.Length = 0, "Excluded", point.ExclusionReason))
                End If
            Next
            Me.Points = Array.AsReadOnly(aligned)
            Me.Channels = channels.AsReadOnly()
            Me.PointLabels = pointLabels.AsReadOnly()
            Me.SignalLabels = signalLabels.AsReadOnly()
            Me.ExclusionLabels = exclusionLabels.AsReadOnly()
        End Sub
        ''' <summary>Core's immutable panel retains ordered occurrence/window/contributor/marked metadata.</summary>
        Public ReadOnly Property Panel As SpcPanelResult
        Public ReadOnly Property Points As IReadOnlyList(Of SpcPointResult)
        Public ReadOnly Property Channels As IReadOnlyList(Of IReadOnlyList(Of Double?))
        Public ReadOnly Property PointLabels As IReadOnlyList(Of SpcChartLabelIntent)
        Public ReadOnly Property SignalLabels As IReadOnlyList(Of SpcChartLabelIntent)
        Public ReadOnly Property ExclusionLabels As IReadOnlyList(Of SpcChartLabelIntent)

        Private Shared Sub AddLabel(labels As List(Of SpcChartLabelIntent), index As Integer, text As String)
            If Not String.IsNullOrWhiteSpace(text) Then labels.Add(New SpcChartLabelIntent(index, text))
        End Sub
        Private Shared Function RuleText(numbers As Integer()) As String
            If numbers Is Nothing OrElse numbers.Length = 0 Then Return String.Empty
            Dim parts(numbers.Length - 1) As String
            For i As Integer = 0 To numbers.Length - 1
                parts(i) = numbers(i).ToString(CultureInfo.InvariantCulture)
            Next
            Return "R" & String.Join(",", parts)
        End Function
        Private Shared Function IsFinite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function
    End Class

    ''' <summary>Actual Core family/panel order, shared axis, channels and stage annotation intent.</summary>
    Public NotInheritable Class SpcUnivariateChartProjection
        Friend Sub New(result As SpcFitResult, useSequence As Boolean)
            Me.ChartType = result.ChartType
            Me.ChartFamily = result.ChartFamily
            Me.UseSequenceValues = useSequence
            Dim representatives As New Dictionary(Of Integer, SpcPointResult)
            Dim sourcePanels = result.Panels
            For Each panel In sourcePanels
                For Each point In panel.Points
                    If Not representatives.ContainsKey(point.PointIndex) Then representatives.Add(point.PointIndex, point)
                Next
            Next
            Dim indices As New List(Of Integer)(representatives.Keys)
            indices.Sort()
            Dim axis As New List(Of SpcChartAxisPoint)(indices.Count)
            Dim boundaries As New List(Of SpcStageBoundaryIntent)
            For i As Integer = 0 To indices.Count - 1
                Dim point = representatives(indices(i))
                axis.Add(New SpcChartAxisPoint(point, useSequence))
                If i > 0 AndAlso Not String.Equals(axis(i - 1).Point.StageId, point.StageId, StringComparison.OrdinalIgnoreCase) Then
                    boundaries.Add(New SpcStageBoundaryIntent(i, indices.Count, axis(i - 1).Point.StageId, point.StageId))
                End If
            Next
            Me.Axis = axis.AsReadOnly()
            Me.StageBoundaries = boundaries.AsReadOnly()
            Dim panels As New List(Of SpcPanelSeriesProjection)(sourcePanels.Length)
            For Each panel In sourcePanels
                panels.Add(New SpcPanelSeriesProjection(panel, Me.Axis))
            Next
            Me.Panels = panels.AsReadOnly()
        End Sub
        Public ReadOnly Property ChartType As SpcChartType
        Public ReadOnly Property ChartFamily As SpcChartFamily
        Public ReadOnly Property UseSequenceValues As Boolean
        Public ReadOnly Property Axis As IReadOnlyList(Of SpcChartAxisPoint)
        Public ReadOnly Property Panels As IReadOnlyList(Of SpcPanelSeriesProjection)
        Public ReadOnly Property StageBoundaries As IReadOnlyList(Of SpcStageBoundaryIntent)
    End Class

    Public NotInheritable Class SpcUnivariateChartProjectionPolicy
        Private Sub New()
        End Sub
        ''' <summary>Project one existing fit. Never fits, evaluates rules or estimates limits.</summary>
        Public Shared Function Project(result As SpcFitResult, Optional useSequenceValues As Boolean = False) As SpcUnivariateChartProjection
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
            Return New SpcUnivariateChartProjection(result, useSequenceValues)
        End Function
    End Class
End Namespace
