"""Capture untouched Windows arithmetic as independently compiled test-only slices."""
from work import *
def method(source,name):
    m=re.search(r'^\s*(?:Public|Private|Friend) (?:Shared )?(?:Function|Sub) '+name+r'\b.*?^\s*End (?:Function|Sub)\b',source,re.M|re.S);assert m,name;return m.group()
if __name__=='__main__':
    assert not (HERE/'Legacy36.vb').exists()
    k=(HERE/'entry-source/BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb.txt').read_text(encoding='utf-8-sig')
    s=(HERE/'entry-source/BESHstatNG/src/Graphics/SpcControlChartExcel.vb.txt').read_text(encoding='utf-8-sig')
    slices=[]
    body='''Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl
Namespace Global.Legacy36
Public Module Original36
'''
    starts=['Dim plotUpper As Integer = plotData.SurvivalProbability.GetUpperBound(0)','Dim groupIndex As Integer = i - noGroups','Dim groupIndex As Integer = i - 2 * noGroups','Dim groupIndex As Integer = i - 3 * noGroups']
    for role,start in enumerate(starts):
        begin=k.index(start);end=k.index('                        .SeriesCollection.NewSeries',begin)
        raw=k[begin:end];slices.append(dict(id='KM-'+str(role),source='BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb',body=raw,sha256=sha(raw.encode())))
        block=raw.replace('Dim groupIndex As Integer = i - noGroups','Dim groupIndex As Integer = group').replace('Dim groupIndex As Integer = i - 2 * noGroups','Dim groupIndex As Integer = group').replace('Dim groupIndex As Integer = i - 3 * noGroups','Dim groupIndex As Integer = group')
        if role==0:block=block.replace('MaximumTimeByGroup(i)','MaximumTimeByGroup(group)').replace('SurvivalProbability(j, i)','SurvivalProbability(j, group)')
        body+='Public Function KM'+str(role)+'(plotData As survival.KaplanMeierPlotData, group As Integer) As Object()\nDim probability(),time(),upCI(),lowCI(),cenProbability(),cenTimes() As Double\nDim bCen As Boolean\n'+block
        pair=['time,probability,True','cenTimes,cenProbability,bCen','time,upCI,True','time,lowCI,True'][role]
        body+='Return New Object(){'+pair+'}\nEnd Function\n'
    zone=method(s,'AddZoneBands');raw=zone[zone.index('            Dim baseValues'):zone.index('            If validBandCount = 0 Then Return')]
    slices.append(dict(id='SPC-BANDS',source='BESHstatNG/src/Graphics/SpcControlChartExcel.vb',body=raw,sha256=sha(raw.encode())))
    body+='Public Function Bands(points As SpcPointResult(), minimum As Double) As Object()\nDim categories(points.Length-1) As Object\n'+raw.replace('data.Points(i)','points(i)').replace('bounds.Minimum','minimum')+'\nReturn New Object(){validBandCount,baseValues,lowerOuter,lowerMiddle,lowerCenter,upperCenter,upperMiddle,upperOuter}\nEnd Function\n'
    for name in ['HasValidZones','IsFinite','MissingChartValue','IsNumericChartValue','HasAnyValue']:
        raw=method(s,name);slices.append(dict(id=name,source='BESHstatNG/src/Graphics/SpcControlChartExcel.vb',body=raw,sha256=sha(raw.encode())))
        body+=raw.replace('Private Shared ','Public ')+'\n'
    body+='''Public Function LastIndex(values As Object()) As Integer
For i As Integer=values.Length-1 To 0 Step -1
If IsNumericChartValue(values(i)) Then Return i
Next
Return -1
End Function
Public Function Cox(time As Double(), probability As Double()) As Object()
If time Is Nothing Then Throw New ArgumentNullException("survTime")
If probability Is Nothing Then Throw New ArgumentNullException("survProb")
If time.Length<>probability.Length Then Throw New ArgumentException("survTime and survProb must have the same length.")
If time.Length=0 Then Return Nothing
Dim scale=ChartAxisScalingCore.Compute(0,time.Max())
Return New Object(){time,probability,scale.Min,scale.Max,scale.Scale,0.0,1.0,0.2,"Baseline","Cox - Survival plot","Survival Probability","Time"}
End Function
End Module
End Namespace
'''
    body=body.replace('Imports System.Linq','Imports System.Linq\nImports System.Globalization')
    write('evidence/04A36/Legacy36.vb',body)
    save('pre-edit-capture-spec.json',dict(authority='PRE_EDIT_LEGACY_EXECUTED',scientific='NOT_GOLDEN',slices=slices,adaptations=['KM numeric blocks copied before native NewSeries; only loop group binding changed; locals hoisted into independent functions. No missing-censor repair.','SPC band prefix copied until native early-return; substitutes explicit points/minimum. Validation/numeric admission/missing helpers copied verbatim. LastIndex is original reverse loop with native SetPointLabel replaced by returned ordinal.','Cox original pre-chart guard/scaling plus raw vectors and literal labels; existing accepted ChartScaling engine reused (ACCEPTED_CORE, not independent science).','All portable comparison outputs IEEE bit serialized; NaN payload bits compared exactly, signed zero retained. CurrentCulture captions separate from invariant numerical serialization.','No native calls executed. Original rendering and failure timing are separately source audited.']))
    print('Frozen',len(slices),'original source blocks; no product edited')
