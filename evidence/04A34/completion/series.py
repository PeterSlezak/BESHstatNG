"""Freeze and delegate remaining series formation and measured label seams."""
from work import *
F=[]
def entry(file):return (HERE/'entry-source'/(file+'.vb.txt')).read_text(encoding='utf-8')
def vector_type():
    return '''Public NotInheritable Class SeriesPayload34
Private ReadOnly _x As Double(), _y As Double(), _amounts As Double()
Private ReadOnly _labels As String(), _source As Integer()
Public ReadOnly Property Visible As Boolean
Public Sub New(x As Double(), y As Double(), Optional amounts As Double() = Nothing, Optional labels As String() = Nothing, Optional source As Integer() = Nothing, Optional isVisible As Boolean = True)
_x = DirectCast(x.Clone(), Double())
_y = DirectCast(y.Clone(), Double())
_amounts = If(amounts Is Nothing, New Double(){}, DirectCast(amounts.Clone(), Double()))
_labels = If(labels Is Nothing, New String(){}, DirectCast(labels.Clone(), String()))
_source = If(source Is Nothing, New Integer(){}, DirectCast(source.Clone(), Integer()))
Visible = isVisible
End Sub
Public ReadOnly Property XValues As Double()
Get
Return DirectCast(_x.Clone(), Double())
End Get
End Property
Public ReadOnly Property YValues As Double()
Get
Return DirectCast(_y.Clone(), Double())
End Get
End Property
Public ReadOnly Property Amounts As Double()
Get
Return DirectCast(_amounts.Clone(), Double())
End Get
End Property
Public ReadOnly Property Labels As String()
Get
Return DirectCast(_labels.Clone(), String())
End Get
End Property
Public ReadOnly Property SourceOrdinals As Integer()
Get
Return DirectCast(_source.Clone(), Integer())
End Get
End Property
End Class
'''
def add(family,name,file,member,start,end,parameters,substitutions,returns):
    raw=method(entry(file),member);fragment=raw[raw.index(start):raw.index(end,raw.index(start))]
    b=fragment
    for a,z in sorted(substitutions.items(),key=lambda x:-len(x[0])):b=b.replace(a,z)
    signature='Public Function '+name+'('+parameters+') As '+returns[0]
    fn=signature+'\n'+b+returns[1]+'\nEnd Function\n'
    F.append(dict(family=family,name=name,file=file,member=member,original=fragment,original_sha256=sha(fragment.encode()),function=fn))
add('Dumbbell','Connectors','DumbbellPlotExcel','AddConnectorSeries','        Dim n As Integer','        \'If every pair','categories As DumbbellPlotCategory()',{},('SeriesPayload34','Return New SeriesPayload34(xStarts, yValues, connectorLengths, source:=categories.Select(Function(c) c.SourceIndex).ToArray(), isVisible:=hasVisibleConnector)'))
add('Dumbbell','Endpoints','DumbbellPlotExcel','AddEndpointSeries','        Dim n As Integer','        Dim series As Object','categories As DumbbellPlotCategory(), useFirstEndpoint As Boolean',{},('SeriesPayload34','Return New SeriesPayload34(xValues, yValues, source:=categories.Select(Function(c) c.SourceIndex).ToArray())'))
add('Dumbbell','Auxiliary','DumbbellPlotExcel','AddAuxiliarySeries','        If observations Is Nothing','        Dim series As Object','observations As DumbbellAuxiliaryObservation()',{},('SeriesPayload34','Return New SeriesPayload34(xValues, yValues, source:=observations.Select(Function(c) c.SourceIndex).ToArray())'))
add('Dumbbell','CategoryLabels','DumbbellPlotExcel','AddCategoryLabelSeries','        Dim n As Integer','        Dim series As Object','categories As DumbbellPlotCategory(), minimum As Double',{'axisContext.XMinimum':'minimum'},('SeriesPayload34','Return New SeriesPayload34(xValues, yValues, labels:=categories.Select(Function(c) c.CategoryLabel).ToArray(), source:=categories.Select(Function(c) c.SourceIndex).ToArray())'))
add('Dumbbell','NumericEndpoint','DumbbellPlotExcel','FormatEndpointValue','        If Not String.IsNullOrWhiteSpace(requestedFormat)','    End Function','value As Double, requestedFormat As String',{},('String',''))
add('Ladder','ObservationCoordinates','LadderPlotExcel','AddObservationSeries','        Dim xValues() As Double','        Dim series As Object','observation As LadderPlotObservation',{},('SeriesPayload34','Return New SeriesPayload34(xValues, yValues, source:=New Integer(){observation.SourceIndex,observation.SourceIndex})'))
add('Ladder','EndpointValues','LadderPlotExcel','AddEndpointLabelSeries','        Dim actualValues','        Dim labelValues','observations As LadderPlotObservation(), useFirstSide As Boolean',{},('SeriesPayload34','Return New SeriesPayload34(New Double(){}, actualValues, source:=observations.Select(Function(c) c.SourceIndex).ToArray())'))
add('Ladder','EndpointAnchors','LadderPlotExcel','AddEndpointLabelSeries','        Dim maximumObservedJitter','        Dim series As Object','observations As LadderPlotObservation(), useFirstSide As Boolean, minimum As Double, maximum As Double, gap As Double',{'appearance.EndpointLabelAnchorGap':'gap','axisContext.XMinimum':'minimum','axisContext.XMaximum':'maximum','LadderPlot.FirstBaseXPosition':'LadderPlotCore.FirstBaseXPosition','LadderPlot.SecondBaseXPosition':'LadderPlotCore.SecondBaseXPosition'},('SeriesPayload34','Return New SeriesPayload34(xValues, New Double(){}, source:=observations.Select(Function(c) c.SourceIndex).ToArray())'))
add('Ladder','SideLabels','LadderPlotExcel','AddSideLabelSeries','        Dim xValues() As Double','        Dim series As Object','minimum As Double, firstLabel As String, secondLabel As String',{'axisContext.YMinimum':'minimum','appearance.FirstAxisLabel':'firstLabel','appearance.SecondAxisLabel':'secondLabel','LadderPlot.FirstBaseXPosition':'LadderPlotCore.FirstBaseXPosition','LadderPlot.SecondBaseXPosition':'LadderPlotCore.SecondBaseXPosition'},('SeriesPayload34','Return New SeriesPayload34(xValues, yValues, labels:=labels)'))
add('Ladder','MinimumSeparation','LadderPlotExcel','ResolveMinimumLabelSeparation','        If Not IsFinitePositive(plotHeight)','    End Function','plotHeight As Double, requestedChartHeight As Double, minimum As Double, maximum As Double, minimumPoints As Double, fontSize As Double',{'appearance.EndpointLabelMinimumSeparationPoints':'minimumPoints','appearance.EndpointLabelFontSize':'fontSize','axisContext.YMaximum':'maximum','axisContext.YMinimum':'minimum'},('Double',''))
raw=method(entry('AgreementPlotExcel'),'AddHorizontalReferenceLine') # pin horizontal renderer as context
F.append(dict(family='Agreement',name='BaseSpan',file='AgreementPlotExcel',member='AddPlot(BlandAltmanAgreement)',original=entry('AgreementPlotExcel'),original_sha256=sha(entry('AgreementPlotExcel').encode()),function='''Public Function BaseSpan(values As Double()) As ChartAxisScaleResult
Dim xMin As Double = values.Min()
Dim xMax As Double = values.Max()
If xMin = xMax Then
xMin -= 0.5
xMax += 0.5
End If
Return New ChartAxisScaleResult(xMin,xMax,0.0R)
End Function
'''))
raw=method(entry('MultipleComparisonPlotExcel'),'SuggestedHeight')
F.append(dict(family='Comparison',name='SuggestedHeight',file='MultipleComparisonPlotExcel',member='SuggestedHeight',original=raw,original_sha256=sha(raw.encode()),function=raw.replace('Public Shared ','Public ')))
F.append(dict(family='Cdf',name='PercentileReference',file='CumulativeDistributionPlotExcel',member='AddPercentileReferenceSeries',original=method(entry('CumulativeDistributionPlotExcel'),'AddPercentileReferenceSeries'),original_sha256=sha(method(entry('CumulativeDistributionPlotExcel'),'AddPercentileReferenceSeries').encode()),function='''Public Function PercentileReference(xCorner As Double, yCorner As Double, minimumX As Double, minimumY As Double) As SeriesPayload34
If Double.IsNaN(xCorner) OrElse Double.IsInfinity(xCorner) OrElse Double.IsNaN(yCorner) OrElse Double.IsInfinity(yCorner) Then Return Nothing
Dim xValues() As Double = {minimumX, xCorner, xCorner}
Dim yValues() As Double = {yCorner, yCorner, minimumY}
Return New SeriesPayload34(xValues,yValues)
End Function
'''))
def freeze():
    assert not (HERE/'pre-edit-series.csv').exists()
    # Each numerical prefix still equals its continuation-entry authority.
    for r in F:
        if r['family'] not in ('Agreement','Cdf'):
            assert r['original'] in src(r['file']),(r['file'],r['member'])
    save('series-spec.json',dict(authority='PRE_EDIT_LEGACY',scientific='NOT_GOLDEN',functions=F,identity_authority='SourceOrdinals are accepted Core SourceIndex getters, not worksheet rows. Additional identity vectors are ACCEPTED_CORE wiring and checked independently. Original coordinate loops are the compatibility oracle.'))
    legacy=HEADER+'Namespace Global.Batch34SeriesFixtures\n'+vector_type()
    for family in dict.fromkeys(r['family'] for r in F):
        legacy+='Public Module Legacy'+family+'Series34\n'+'\n'.join(r['function'] for r in F if r['family']==family)
        if family=='Ladder':legacy+='Private Function IsFinitePositive(value As Double) As Boolean\nReturn Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value) AndAlso value > 0.0R\nEnd Function\n'
        legacy+='\nEnd Module\n'
    write('BESHStatNG.Test/src/Charts/LegacyCompletionSeries34.vb',legacy+'End Namespace\n')
    print('Frozen remaining series, numeric/date-boundary and reference seams')
def apply():
    assert (HERE/'pre-edit-series.csv').exists()
    # Per-family types avoid a new general chart framework; common source class is
    # generated independently in each family namespace and owns all mutable arrays.
    for family in dict.fromkeys(r['family'] for r in F):
        text=HEADER+'Namespace ExcelCommon.Chart34.'+family+'Series\n'+(vector_type() if family in ('Dumbbell','Ladder','Cdf') else '')+'Public Module '+family+'SeriesPreparation34\n'+'\n'.join(r['function'] for r in F if r['family']==family)
        if family=='Ladder':text+='Private Function IsFinitePositive(value As Double) As Boolean\nReturn Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value) AndAlso value > 0.0R\nEnd Function\n'
        text+='\nEnd Module\nEnd Namespace\n'
        write('BESHStat.Excel.Common/src/Analysis/'+family+'SeriesPreparation34.vb',text)
    for family in ['Dumbbell','Ladder']:
        file=family+'PlotExcel';s=src(file)
        for r in F:
            if r['family']!=family:continue
            if r['name']=='NumericEndpoint':
                s=s.replace(r['original'],'        Return ExcelCommon.Chart34.DumbbellSeries.DumbbellSeriesPreparation34.NumericEndpoint(value, requestedFormat)\n')
                continue
            if r['name']=='MinimumSeparation':
                new='        Return ExcelCommon.Chart34.LadderSeries.LadderSeriesPreparation34.MinimumSeparation(plotHeight, requestedChartHeight, axisContext.YMinimum, axisContext.YMaximum, appearance.EndpointLabelMinimumSeparationPoints, appearance.EndpointLabelFontSize)\n'
                assert r['original'] in s;s=s.replace(r['original'],new);continue
            names={
              'Connectors':('categories',{'xStarts':'XValues','yValues':'YValues','connectorLengths':'Amounts'},'        Dim hasVisibleConnector As Boolean = prepared.Visible\n'),
              'Endpoints':('categories, useFirstEndpoint',{'xValues':'XValues','yValues':'YValues'},''),
              'Auxiliary':('observations',{'xValues':'XValues','yValues':'YValues'},'        If prepared Is Nothing Then Return Nothing\n'),
              'CategoryLabels':('categories, axisContext.XMinimum',{'xValues':'XValues','yValues':'YValues'},'        Dim n As Integer = categories.Length\n'),
              'ObservationCoordinates':('observation',{'xValues':'XValues','yValues':'YValues'},''),
              'EndpointValues':('observations, useFirstSide',{'actualValues':'YValues'},''),
              'EndpointAnchors':('observations, useFirstSide, axisContext.XMinimum, axisContext.XMaximum, appearance.EndpointLabelAnchorGap',{'xValues':'XValues'},''),
              'SideLabels':('axisContext.YMinimum, appearance.FirstAxisLabel, appearance.SecondAxisLabel',{'xValues':'XValues','yValues':'YValues','labels':'Labels'},'')}
            args,fields,extra=names[r['name']];variable='prepared'+r['name']
            b='        Dim '+variable+' = ExcelCommon.Chart34.'+family+'Series.'+family+'SeriesPreparation34.'+r['name']+'('+args+')\n'+extra.replace('prepared',variable)
            b+=''.join('        Dim '+n+' As '+('String()' if p=='Labels' else 'Double()')+' = '+variable+'.'+p+'\n' for n,p in fields.items())+'\n'
            assert r['original'] in s,r['name'];s=s.replace(r['original'],b)
        write('BESHstatNG/src/Graphics/'+file+'.vb',s)
    s=src('AgreementPlotExcel');start=s.index('        Dim xMin As Double = plotData.XValues.Min()');end=s.index('        Dim lineX',start)
    s=s[:start]+'''        Dim span = ExcelCommon.Chart34.AgreementSeries.AgreementSeriesPreparation34.BaseSpan(plotData.XValues)
        Dim xMin As Double = span.Min
        Dim xMax As Double = span.Max

'''+s[end:];write('BESHstatNG/src/Graphics/AgreementPlotExcel.vb',s)
    s=src('MultipleComparisonPlotExcel');h,b=header_body(method(s,'SuggestedHeight'));s=replace(s,'SuggestedHeight',h+'\n            Return ExcelCommon.Chart34.ComparisonSeries.ComparisonSeriesPreparation34.SuggestedHeight(data)\n        End Function');write('BESHstatNG/src/Graphics/MultipleComparisonPlotExcel.vb',s)
    s=src('CumulativeDistributionPlotExcel');start=s.index('            Dim xCorner As Double = percentile.XValue');end=s.index('            Dim series As Object',start)
    s=s[:start]+'''            Dim prepared = ExcelCommon.Chart34.CdfSeries.CdfSeriesPreparation34.PercentileReference(percentile.XValue, percentile.YValue, axisContext.XMinimum, axisContext.YMinimum)
            If prepared Is Nothing Then Continue For
            Dim xValues() As Double = prepared.XValues
            Dim yValues() As Double = prepared.YValues

'''+s[end:];write('BESHstatNG/src/Graphics/CumulativeDistributionPlotExcel.vb',s)
    print('Consumed series vectors, identity, label anchors, measured separation and base agreement span')
if __name__=='__main__':globals()[sys.argv[1]]()
