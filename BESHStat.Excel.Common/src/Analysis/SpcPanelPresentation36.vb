Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.StatisticalProcessControl
Namespace ExcelCommon
Public Enum SpcSeriesRole36
    Statistic
    Center
    Control
    Zone
    Specification
    Target
    Excluded
    Signal
End Enum

Public NotInheritable Class SpcSeriesIntent36
    Private ReadOnly _values As Double?()
    Public ReadOnly Property Role As SpcSeriesRole36
    Public ReadOnly Property Name As String
    Public ReadOnly Property LastValueOrdinal As Integer
    Public ReadOnly Property ShowLimitLabel As Boolean
    Public ReadOnly Property HideLegend As Boolean
    Friend Sub New(role As SpcSeriesRole36, name As String, values As IEnumerable(Of Double?), showLabel As Boolean, hide As Boolean)
        Me.Role=role : Me.Name=name : _values=values.ToArray()
        LastValueOrdinal=LastFiniteOrdinal(_values) : ShowLimitLabel=showLabel : HideLegend=hide
    End Sub
    Public ReadOnly Property Values As Double?()
        Get
            Return DirectCast(_values.Clone(),Double?())
        End Get
    End Property
    Public Shared Function LastFiniteOrdinal(values As IReadOnlyList(Of Double?)) As Integer
        For i=values.Count-1 To 0 Step -1
            If values(i).HasValue AndAlso Not Double.IsNaN(values(i).Value) AndAlso Not Double.IsInfinity(values(i).Value) Then Return i
        Next
        Return -1
    End Function
End Class

''' <summary>Ordered rendering intent for an accepted aligned panel. Never fits or recomputes limits/signals.</summary>
Public NotInheritable Class SpcPanelPresentation36
    Public ReadOnly Property Series As IReadOnlyList(Of SpcSeriesIntent36)
    Public ReadOnly Property IncludesBands As Boolean
    Public Shared Function AxisBounds(panel As SpcPanelSeriesProjection, specifications As SpcSpecificationLimits, showSpecs As Boolean, showTarget As Boolean) As ChartAxisScaleResult
        Dim minimum=Double.PositiveInfinity, maximum=Double.NegativeInfinity
        For channel=0 To 7
            For Each sample As Double? In panel.Channels(channel)
                Dim nextBounds=SpcAxisBoundsCore.SpcOptionalExtent(sample,minimum,maximum)
                minimum=nextBounds.Min : maximum=nextBounds.Max
            Next
        Next
        If Chart34.SpcChartPreparation34.PanelSupportsSpecificationLimits(panel.Panel.PanelType) Then
            If showSpecs Then
                Dim lower=SpcAxisBoundsCore.SpcOptionalExtent(specifications.LowerSpecificationLimit,minimum,maximum)
                minimum=lower.Min : maximum=lower.Max
                Dim upper=SpcAxisBoundsCore.SpcOptionalExtent(specifications.UpperSpecificationLimit,minimum,maximum)
                minimum=upper.Min : maximum=upper.Max
            End If
            If showTarget Then
                Dim target=SpcAxisBoundsCore.SpcOptionalExtent(specifications.Target,minimum,maximum)
                minimum=target.Min : maximum=target.Max
            End If
        End If
        Return SpcAxisBoundsCore.FromExtents(minimum,maximum,panel.Panel.PanelType)
    End Function
    Public Sub New(panel As SpcPanelSeriesProjection, specifications As SpcSpecificationLimits, zoneMode As Integer, showSpecs As Boolean, showTarget As Boolean, showExcluded As Boolean, showLabels As Boolean, showZoneLegend As Boolean)
        Dim result As New List(Of SpcSeriesIntent36)
        IncludesBands=zoneMode=2 OrElse zoneMode=3
        Dim add As Action(Of SpcSeriesRole36,String,IEnumerable(Of Double?),Boolean,Boolean)=Sub(role,name,values,label,hide)
            Dim intent As New SpcSeriesIntent36(role,name,values,label,hide)
            If role=SpcSeriesRole36.Statistic OrElse intent.LastValueOrdinal>=0 Then result.Add(intent)
        End Sub
        Dim c=panel.Channels
        add(SpcSeriesRole36.Statistic,panel.Panel.DisplayName,c(CInt(SpcChartChannel.Statistic)),False,False)
        add(SpcSeriesRole36.Center,"CL",c(CInt(SpcChartChannel.CenterLine)),showLabels,False)
        add(SpcSeriesRole36.Control,"UCL",c(CInt(SpcChartChannel.UpperControlLimit)),showLabels,False)
        add(SpcSeriesRole36.Control,"LCL",c(CInt(SpcChartChannel.LowerControlLimit)),showLabels,False)
        If zoneMode=1 OrElse zoneMode=3 Then
            add(SpcSeriesRole36.Zone,"-2 sigma",c(CInt(SpcChartChannel.LowerTwoSigma)),False,Not showZoneLegend)
            add(SpcSeriesRole36.Zone,"-1 sigma",c(CInt(SpcChartChannel.LowerOneSigma)),False,Not showZoneLegend)
            add(SpcSeriesRole36.Zone,"+1 sigma",c(CInt(SpcChartChannel.UpperOneSigma)),False,Not showZoneLegend)
            add(SpcSeriesRole36.Zone,"+2 sigma",c(CInt(SpcChartChannel.UpperTwoSigma)),False,Not showZoneLegend)
        End If
        If Chart34.SpcChartPreparation34.PanelSupportsSpecificationLimits(panel.Panel.PanelType) Then
            Dim count=c(0).Count
            Dim constant As Func(Of Double,Double?())=Function(v) SpcAxisBoundsCore.SpcConstantReference(count,v).Select(Function(x) CType(x,Double?)).ToArray()
            If showSpecs Then
                If specifications.LowerSpecificationLimit.HasValue Then add(SpcSeriesRole36.Specification,"LSL",constant(specifications.LowerSpecificationLimit.Value),showLabels,False)
                If specifications.UpperSpecificationLimit.HasValue Then add(SpcSeriesRole36.Specification,"USL",constant(specifications.UpperSpecificationLimit.Value),showLabels,False)
            End If
            If showTarget AndAlso specifications.Target.HasValue Then add(SpcSeriesRole36.Target,"Target",constant(specifications.Target.Value),showLabels,False)
        End If
        If showExcluded Then add(SpcSeriesRole36.Excluded,"Excluded",c(CInt(SpcChartChannel.Excluded)),False,False)
        add(SpcSeriesRole36.Signal,"Signal",c(CInt(SpcChartChannel.Signal)),False,False)
        Series=result.AsReadOnly()
    End Sub
End Class
End Namespace
