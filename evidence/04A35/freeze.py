"""Original source slices; run only before production edits. No new35 oracle."""
from work import *
HEADER='Option Explicit On\nOption Strict On\nOption Infer On\nImports System\nImports System.Linq\nImports System.Collections.Generic\nImports BESHStatNG\n'
if __name__=='__main__':
    assert not (HERE/'pre-edit-capture.csv').exists()
    multi=(HERE/'entry-source/MultipleComparisonPlotExcel.vb.txt').read_text(encoding='utf-8-sig')
    functions=[]
    for name in ['ValidatePlotData','BuildTitle']:
        raw=method(multi,name);functions.append(dict(name=name,original=raw,body_sha256=sha(raw.encode()),adaptation='Only Private Shared becomes Public in a test-only standalone module. Original namespace/data owner retained.'))
    numeric=multi[multi.index('            Dim n As Integer = data.Estimates.Length'):multi.index('            Dim xScale As CHARTscale')]
    numeric=numeric.replace('SuggestedHeight(data)','ExcelCommon.Chart34.ComparisonSeries.ComparisonSeriesPreparation34.SuggestedHeight(data)')
    legacy=HEADER+'Namespace Global.Legacy35\nPublic Module Original35\n'
    for r in functions:legacy+=r['original'].replace('Private Shared ','Public ')+'\n'
    legacy+='''Public Function Comparison(data As parametric.MultipleComparisonPlotData) As Object()
ValidatePlotData(data)
'''+numeric+'''
Return New Object(){y,plusError,minusError,minCI,maxCI,BuildTitle(data),height}
End Function
Public Function LabelMap(labels As String(), pointCount As Integer) As Object()
Dim rows As New List(Of Object)
For i As Integer = 1 To Math.Min(pointCount,labels.Length)
rows.Add(New Object(){i-1,i,labels(i-1)})
Next
Return rows.ToArray()
End Function
Public Function Regression(minX As Double,maxX As Double,intercept As Double,slope As Double) As Object()
Return New Object(){New Object(){"Regression line",ChartReferenceSeries.Identity(minX,maxX).XValues,ChartReferenceSeries.Affine(minX,maxX,intercept,slope).YValues},New Object(){"Unity line (y = x)",ChartReferenceSeries.Identity(minX,maxX).XValues,ChartReferenceSeries.Identity(minX,maxX).XValues}}
End Function
Public Function Identity(minimum As Double, maximum As Double) As Object()
Return New Object(){New Object(){"Identity",ChartReferenceSeries.Identity(minimum,maximum).XValues,ChartReferenceSeries.Identity(minimum,maximum).XValues}}
End Function
Public Function Bland(x As Double(),bias As Double,lower As Double,upper As Double,show As Boolean,sx As Double(),sy As Double()) As Object()
Dim span=ExcelCommon.Chart34.AgreementSeries.AgreementSeriesPreparation34.BaseSpan(x)
Dim lineX As Double()={span.Min,span.Max}
Dim result As New List(Of Object) From {New Object(){"Bias",lineX,ChartReferenceSeries.Horizontal(lineX(0),lineX(1),bias).YValues},New Object(){"Lower LoA",lineX,ChartReferenceSeries.Horizontal(lineX(0),lineX(1),lower).YValues},New Object(){"Upper LoA",lineX,ChartReferenceSeries.Horizontal(lineX(0),lineX(1),upper).YValues}}
If show AndAlso sx IsNot Nothing AndAlso sy IsNot Nothing Then result.Add(New Object(){"Subject means",sx,sy})
Return result.ToArray()
End Function
Public Function Love(rows As IList(Of CausalInference.PsmLovePlotRow)) As Object()
Dim projection As New LegacyLoveProjection35(rows)
Dim y=projection.YValues,xBefore=projection.Before,xAfterMatching=projection.AfterMatching,xAfterWeighting=projection.AfterWeighting
Dim names As New List(Of String),values As New List(Of Object)
If xBefore.Any(Function(v) Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v)) Then
names.Add("Before"):values.Add(xBefore)
End If
If xAfterMatching.Any(Function(v) Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v)) Then
names.Add("After matching"):values.Add(xAfterMatching)
End If
If xAfterWeighting.Any(Function(v) Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v)) Then
names.Add("After weighting"):values.Add(xAfterWeighting)
End If
Return New Object(){names.ToArray(),values.ToArray(),y,projection.Labels,If(names.Count=0,-1,names.Count-1),projection.Threshold,projection.MaximumX,projection.BuildDataTable()}
End Function
End Module
End Namespace
'''
    old=(ROOT/'BESHStat.Excel.Common/src/Analysis/PsmLovePlotProjection.vb').read_text(encoding='utf-8-sig')
    clone=old.replace('Namespace ExcelCommon','Namespace Global.Legacy35').replace('PsmLovePlotProjection','LegacyLoveProjection35')
    write('BESHStat.Excel.Common.Tests/src/Legacy35.vb',legacy+'\n')
    write('BESHStat.Excel.Common.Tests/src/LegacyLoveProjection35.vb',clone)
    save('pre-edit-capture-spec.json',dict(authority='PRE_EDIT_LEGACY_EXECUTED',scientific='NOT_GOLDEN',source_methods=functions,numeric_prefix=numeric,numeric_prefix_sha256=sha(numeric.encode()),love_original_source=pin('BESHStat.Excel.Common/src/Analysis/PsmLovePlotProjection.vb'),bindings=['Accepted34 OrdinalY/IncludeZeroBounds/reference/base-span/height engines reused; not independent scientific references.','Agreement raw-input slices copy exact endpoint/order/subject-presence expressions from frozen adapter bodies; GetPlotData statistics remain accepted Core.','Love source class copied verbatim except namespace/type name; admission/order policy faithfully sliced from original adapter, no new35 implementation.','LabelMap replaces native Points.Count with explicit measured count and records the original one-based loop. Native assignment/style/error catch stays SOURCE_ONLY.']))
    print('Frozen independently compilable original bodies; no product edited')
