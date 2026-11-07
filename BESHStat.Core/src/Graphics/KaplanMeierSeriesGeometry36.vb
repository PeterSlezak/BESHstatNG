Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Public Enum KaplanMeierSeriesRole36
    Survival = 0
    Censor = 1
    UpperConfidence = 2
    LowerConfidence = 3
End Enum

''' <summary>Detached original renderer vectors. Padding slot -2 is unassigned; origin -1 is synthetic. No worksheet-row inference.</summary>
Public NotInheritable Class KaplanMeierSeries36
    Private ReadOnly _x As Double(), _y As Double()
    Private ReadOnly _written As Integer
    Public ReadOnly Property SourceGroupOrdinal As Integer
    Public ReadOnly Property Role As KaplanMeierSeriesRole36
    Public ReadOnly Property AssignValues As Boolean
    Friend Sub New(x As Double(), y As Double(), group As Integer, role As KaplanMeierSeriesRole36, assign As Boolean, written As Integer)
        _x=DirectCast(x.Clone(),Double()) : _y=DirectCast(y.Clone(),Double())
        SourceGroupOrdinal=group : Me.Role=role : AssignValues=assign : _written=written
    End Sub
    Public ReadOnly Property XValues As Double()
        Get
            Return DirectCast(_x.Clone(),Double())
        End Get
    End Property
    Public ReadOnly Property YValues As Double()
        Get
            Return DirectCast(_y.Clone(),Double())
        End Get
    End Property
    Private Function Slots(count As Integer) As Integer()
        Dim result=Enumerable.Repeat(-2,count).ToArray()
        For i=0 To Math.Min(count,_written)-1
            result(i)=If(Role=KaplanMeierSeriesRole36.Censor,i,i-1)
        Next
        Return result
    End Function
    Public ReadOnly Property XSourceSlots As Integer()
        Get
            Return Slots(_x.Length)
        End Get
    End Property
    Public ReadOnly Property YSourceSlots As Integer()
        Get
            Return Slots(_y.Length)
        End Get
    End Property
End Class

''' <summary>Per-phase preparation at the original host read boundary, including legacy sparse-array exceptions.</summary>
Public NotInheritable Class KaplanMeierSeriesGeometry36
    Private Sub New()
    End Sub
Public Shared Function Survival(plotData As survival.KaplanMeierPlotData, group As Integer) As KaplanMeierSeries36
Dim probability(),time(),upCI(),lowCI(),cenProbability(),cenTimes() As Double
Dim bCen As Boolean
Dim written As Integer = 1
Dim plotUpper As Integer = plotData.SurvivalProbability.GetUpperBound(0)
                        ReDim probability(plotUpper + 1)
                        ReDim time(plotUpper + 1)
                        probability(0) = 1
                        time(0) = 0

                        For j As Integer = 0 To plotUpper
                            If plotData.SurvivalTime(j) <= plotData.MaximumTimeByGroup(group) Then
                                probability(j + 1) = plotData.SurvivalProbability(j, group)
                                time(j + 1) = plotData.SurvivalTime(j)
written += 1
                            Else
                                ReDim Preserve probability(j)
                                ReDim Preserve time(j)
                                Exit For
                            End If
                        Next

Return New KaplanMeierSeries36(time,probability,group,KaplanMeierSeriesRole36.Survival,True,written)
End Function
Public Shared Function Censor(plotData As survival.KaplanMeierPlotData, group As Integer) As KaplanMeierSeries36
Dim probability(),time(),upCI(),lowCI(),cenProbability(),cenTimes() As Double
Dim bCen As Boolean
Dim written As Integer = 0
Dim groupIndex As Integer = group
                        If plotData.CensoredCountByGroup(groupIndex) > 0 Then
                            ReDim cenProbability(plotData.CensoredCountByGroup(groupIndex) - 1)
                            ReDim cenTimes(plotData.CensoredCountByGroup(groupIndex) - 1)
                            bCen = True
                        Else
                            ReDim cenProbability(0)
                            ReDim cenTimes(0)
                            bCen = False
                        End If

                        For j As Integer = 0 To plotData.CensoredCountByGroup(groupIndex) - 1
                            cenProbability(j) = plotData.CensorMarkerProbability(j, groupIndex)
                            cenTimes(j) = plotData.CensorMarkerTime(j, groupIndex)
written += 1
                        Next

Return New KaplanMeierSeries36(cenTimes,cenProbability,group,KaplanMeierSeriesRole36.Censor,bCen,written)
End Function
Public Shared Function Confidence(plotData As survival.KaplanMeierPlotData, group As Integer, upper As Boolean) As KaplanMeierSeries36
Dim probability(),time(),upCI(),lowCI(),cenProbability(),cenTimes() As Double
Dim bCen As Boolean
Dim written As Integer = 1
Dim groupIndex As Integer = group
                            Dim plotUpper As Integer = plotData.SurvivalProbability.GetUpperBound(0)
                            ReDim upCI(plotUpper + 1)
                            ReDim time(plotData.SurvivalTime.Length)
                            upCI(0) = 1
                            time(0) = 0

                            For j As Integer = 0 To plotUpper
                                If plotData.SurvivalTime(j) <= plotData.MaximumTimeByGroup(groupIndex) Then
                                    upCI(j + 1) = If(upper, plotData.UpperConfidenceLimit(j, groupIndex), plotData.LowerConfidenceLimit(j, groupIndex))
                                    time(j + 1) = plotData.SurvivalTime(j)
written += 1
                                Else
                                    ReDim Preserve upCI(j)
                                    ReDim Preserve time(j)
                                    Exit For
                                End If
                            Next

    Return New KaplanMeierSeries36(time,upCI,group,If(upper,KaplanMeierSeriesRole36.UpperConfidence,KaplanMeierSeriesRole36.LowerConfidence),True,written)
End Function
End Class
