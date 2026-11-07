Option Strict On
Option Explicit On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Namespace Global.Legacy36
' Source-slice transcription of the untouched RenderPanel/AddReferenceLine/
' AddZoneLine/AddConstantReferenceLine admission and native call order.
' Native drawing is replaced by records, never a claim of Excel execution.
Public Module OriginalPresentation36V2
Public Function Panel(index As Integer) As SpcPanelSeriesProjection
Dim kinds={SpcPanelType.IndividualValue,SpcPanelType.HotellingT2,SpcPanelType.SubgroupMean,SpcPanelType.PcaQ,SpcPanelType.Ewma,SpcPanelType.Mcusum}
Dim points=Cases36.Points(index).Where(Function(p) p IsNot Nothing).ToArray()
Dim signalValues As New List(Of SpcSignalResult)
For Each point As SpcPointResult In points
For Each number As Integer In point.SignalRuleNumbers
Dim rule=SpcRuleCatalog.GetRules(SpcRulePreset.Nelson).Single(Function(r) r.RuleNumber=number)
signalValues.Add(New SpcSignalResult(kinds(index Mod kinds.Length),point.StageId,rule,point.PointIndex,point.PointIndex,point.PointIndex))
Next:Next
Dim panelValue As New SpcPanelResult(kinds(index Mod kinds.Length),"Statistic",points,signals:=signalValues.ToArray())
Dim fit As New SpcFitResult(New SpcFitRequest(SpcChartType.Individuals,SpcInputData.FromIndividualSequence(New Double(){1,2,3})),New SpcPanelResult(){panelValue})
Return SpcUnivariateChartProjectionPolicy.Project(fit,index Mod 2=0).Panels(0)
End Function
Public Function Specifications(index As Integer) As SpcSpecificationLimits
Return New SpcSpecificationLimits(If(index Mod 3=0,CType(Nothing,Double?),-2.0),If(index Mod 4=0,CType(Nothing,Double?),1.0),If(index Mod 5=0,CType(Nothing,Double?),7.0))
End Function
Public Function Series(panel As SpcPanelSeriesProjection,spec As SpcSpecificationLimits,zone As Integer,showSpecs As Boolean,showTarget As Boolean,showExcluded As Boolean,showLabels As Boolean,showZoneLegend As Boolean) As Object()
Dim result As New List(Of Object)
Dim values=panel.Channels.Select(Function(c) c.Select(Function(v) If(v.HasValue,CType(v.Value,Object),Nothing)).ToArray()).ToArray()
Dim add As Action(Of String,String,Object(),Boolean,Boolean)=Sub(role,name,channel,label,hide)
If role<>"Statistic" AndAlso Not Original36.HasAnyValue(channel) Then Return
result.Add(New Object(){role,name,channel,Original36.LastIndex(channel),label,hide})
End Sub
add("Statistic",panel.Panel.DisplayName,values(0),False,False)
add("Center","CL",values(1),showLabels,False)
add("Control","UCL",values(3),showLabels,False)
add("Control","LCL",values(2),showLabels,False)
If zone=1 OrElse zone=3 Then
add("Zone","-2 sigma",values(6),False,Not showZoneLegend)
add("Zone","-1 sigma",values(4),False,Not showZoneLegend)
add("Zone","+1 sigma",values(5),False,Not showZoneLegend)
add("Zone","+2 sigma",values(7),False,Not showZoneLegend)
End If
Dim supported=False
Select Case panel.Panel.PanelType
Case SpcPanelType.Run,SpcPanelType.IndividualValue,SpcPanelType.SubgroupMean,SpcPanelType.Ewma,SpcPanelType.MovingAverage:supported=True
End Select
If supported Then
If showSpecs Then
If spec.LowerSpecificationLimit.HasValue Then add("Specification","LSL",Enumerable.Repeat(CType(spec.LowerSpecificationLimit.Value,Object),values(0).Length).ToArray(),showLabels,False)
If spec.UpperSpecificationLimit.HasValue Then add("Specification","USL",Enumerable.Repeat(CType(spec.UpperSpecificationLimit.Value,Object),values(0).Length).ToArray(),showLabels,False)
End If
If showTarget AndAlso spec.Target.HasValue Then add("Target","Target",Enumerable.Repeat(CType(spec.Target.Value,Object),values(0).Length).ToArray(),showLabels,False)
End If
If showExcluded Then add("Excluded","Excluded",values(9),False,False)
add("Signal","Signal",values(8),False,False)
Return result.ToArray()
End Function
End Module
End Namespace
