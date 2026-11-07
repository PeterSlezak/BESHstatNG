"""Apply bounded extraction after frozen pre-edit execution; no statistical edits."""
from work import *
from freeze import method
HEADER='Option Explicit On\nOption Strict On\nOption Infer On\nImports System\nImports System.Linq\nImports System.Collections.Generic\n'
if __name__=='__main__':
    assert not git('diff','--name-only')
    for config in ['Debug','Release']:
        assert (ROOT/('.test-rationalization-04A36.tmp/entry-'+config+'-managed.json')).is_file()
    assert (ROOT/'.test-rationalization-04A36.tmp/entry-source.json').is_file()
    assert (HERE/'pre-edit-capture-authority.json').is_file()
    original=(HERE/'Legacy36.vb').read_text()
    core=HEADER+'''Public Enum KaplanMeierSeriesRole36
    Survival = 0
    Censor = 1
    UpperConfidence = 2
    LowerConfidence = 3
End Enum

'''+'\'\'\' <summary>Detached original renderer vectors. Padding slot -2 is unassigned; origin -1 is synthetic. No worksheet-row inference.</summary>\n'+'''Public NotInheritable Class KaplanMeierSeries36
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

'''+'\'\'\' <summary>Per-phase preparation at the original host read boundary, including legacy sparse-array exceptions.</summary>\n'+'''Public NotInheritable Class KaplanMeierSeriesGeometry36
    Private Sub New()
    End Sub
'''
    for role,name in enumerate(['Survival','Censor','Confidence','UnusedLower']):
        if role==3:continue
        raw=method(original,'KM'+str(role)).replace('Public Function KM'+str(role),'Public Shared Function '+name).replace('As Object()','As KaplanMeierSeries36',1)
        if role==2:
            raw=raw.replace('group As Integer)','group As Integer, upper As Boolean)')
            raw=raw.replace('plotData.UpperConfidenceLimit(j, groupIndex)','If(upper, plotData.UpperConfidenceLimit(j, groupIndex), plotData.LowerConfidenceLimit(j, groupIndex))')
            # VB If short-circuits: the unrequested matrix is never read.
        raw=raw.replace('Dim bCen As Boolean','Dim bCen As Boolean\nDim written As Integer = '+('0' if role==1 else '1'))
        raw=raw.replace('time(j + 1) = plotData.SurvivalTime(j)','time(j + 1) = plotData.SurvivalTime(j)\nwritten += 1').replace('cenTimes(j) = plotData.CensorMarkerTime(j, groupIndex)','cenTimes(j) = plotData.CensorMarkerTime(j, groupIndex)\nwritten += 1')
        pair=['time,probability,group,KaplanMeierSeriesRole36.Survival,True','cenTimes,cenProbability,group,KaplanMeierSeriesRole36.Censor,bCen','time,upCI,group,If(upper,KaplanMeierSeriesRole36.UpperConfidence,KaplanMeierSeriesRole36.LowerConfidence),True'][role]
        raw=re.sub(r'Return New Object\(\)\{[^\n]+\}', 'Return New KaplanMeierSeries36('+pair+',written)',raw)
        core+=raw+'\n'
    core+='End Class\n'
    write('BESHStat.Core/src/Graphics/KaplanMeierSeriesGeometry36.vb',core)
    write('BESHStat.Core/src/Graphics/CoxCurveGeometry36.vb',HEADER+'''Public NotInheritable Class CoxCurveGeometry36
    Private Sub New()
    End Sub
    Public Shared Sub Validate(survTime As Double(), survProb As Double())
        If survTime Is Nothing Then Throw New ArgumentNullException(NameOf(survTime))
        If survProb Is Nothing Then Throw New ArgumentNullException(NameOf(survProb))
        If survTime.Length <> survProb.Length Then Throw New ArgumentException("survTime and survProb must have the same length.")
    End Sub
    Public Shared Function Axis(survTime As Double()) As ChartAxisScaleResult
        Return ChartAxisScalingCore.Compute(0,survTime.Max())
    End Function
    'Read at each original native assignment, never an earlier constructor snapshot.
    Public Shared Function Snapshot(values As Double()) As Double()
        Return DirectCast(values.Clone(),Double())
    End Function
End Class
''')
    k=(HERE/'entry-source/BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb.txt').read_text(encoding='utf-8-sig')
    k=k.replace('Imports System.Runtime.CompilerServices','Imports System.Runtime.CompilerServices\nImports BESHStatNG.ExcelCommon')
    for role,start in enumerate(['Dim plotUpper As Integer = plotData.SurvivalProbability.GetUpperBound(0)','Dim groupIndex As Integer = i - noGroups','Dim groupIndex As Integer = i - 2 * noGroups','Dim groupIndex As Integer = i - 3 * noGroups']):
        begin=k.index(start);end=k.index('                        .SeriesCollection.NewSeries',begin)
        call=['Survival(plotData, i)','Censor(plotData, groupIndex)','Confidence(plotData, groupIndex, True)','Confidence(plotData, groupIndex, False)'][role]
        group='' if role==0 else 'Dim groupIndex As Integer = i - '+['','noGroups','2 * noGroups','3 * noGroups'][role]+'\n                        '
        arrays=['time = prepared.XValues\n                        probability = prepared.YValues','cenTimes = prepared.XValues\n                        cenProbability = prepared.YValues\n                        bCen = prepared.AssignValues','time = prepared.XValues\n                            upCI = prepared.YValues','time = prepared.XValues\n                            lowCI = prepared.YValues'][role]
        k=k[:begin]+group+'Dim prepared = KaplanMeierSeriesGeometry36.'+call+'\n                        '+arrays+'\n\n'+k[end:]
    k=k.replace('.Name = plotData.GroupLabels(i)','.Name = SurvivalChartPresentation36.SeriesName(plotData, i, KaplanMeierSeriesRole36.Survival)')
    k=k.replace('"Censored " + plotData.GroupLabels(groupIndex)','SurvivalChartPresentation36.SeriesName(plotData, groupIndex, KaplanMeierSeriesRole36.Censor)')
    k=k.replace('$"{100.0 * (1.0 - plotData.Alpha):0.##}% CI " + plotData.GroupLabels(groupIndex)','SurvivalChartPresentation36.SeriesName(plotData, groupIndex, KaplanMeierSeriesRole36.UpperConfidence)')
    k=k.replace('"Survival Probability"','SurvivalChartPresentation36.ProbabilityTitle').replace('$"Time ({sXaxisUnit})"','SurvivalChartPresentation36.KmTimeTitle(sXaxisUnit)')
    k=k.replace('.MaximumScale = 1','.MaximumScale = SurvivalChartPresentation36.MaximumProbability').replace('.MajorUnit = 0.2','.MajorUnit = SurvivalChartPresentation36.ProbabilityMajorUnit')
    k=k.replace('If noGroups = 1 OrElse Not bLegend Then','If SurvivalChartPresentation36.DeleteLegend(noGroups, bLegend) Then')
    write('BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb',k)
    c=(HERE/'entry-source/BESHstatNG/src/Graphics/CoxSurvivalPlotExcel.vb.txt').read_text(encoding='utf-8-sig').replace('Imports Microsoft.Office.Interop.Excel','Imports Microsoft.Office.Interop.Excel\nImports BESHStatNG.ExcelCommon')
    begin=c.index('            If survTime Is Nothing');end=c.index('            If survTime.Length = 0 Then Exit Sub',begin)
    c=c[:begin]+'            CoxCurveGeometry36.Validate(survTime, survProb)\n'+c[end:]
    c=c.replace('ChartScaling(0, survTime.Max())','CoxCurveGeometry36.Axis(survTime)').replace('Dim yMax As Double = 1.0','Dim yMax As Double = SurvivalChartPresentation36.MaximumProbability').replace('Dim majorUnit As Double = 0.2','Dim majorUnit As Double = SurvivalChartPresentation36.ProbabilityMajorUnit')
    for value,member in [('"Baseline"','BaselineName'),('"Cox - Survival plot"','CoxTitle'),('"Survival Probability"','ProbabilityTitle'),('"Time"','CoxTimeTitle')]:c=c.replace(value,'SurvivalChartPresentation36.'+member)
    c=c.replace('.XValues = survTime','.XValues = CoxCurveGeometry36.Snapshot(survTime)').replace('.Values = survProb','.Values = CoxCurveGeometry36.Snapshot(survProb)')
    write('BESHstatNG/src/Graphics/CoxSurvivalPlotExcel.vb',c)
    print('Applied KM/Cox per-phase numeric delegation; all native tails retained')
