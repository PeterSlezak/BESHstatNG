"""Bounded35 source extraction, installed only after fresh-main baseline completes."""
from work import *
HEADER='Option Explicit On\nOption Strict On\nOption Infer On\nImports System\nImports System.Linq\nImports System.Collections.Generic\n'
CORE=HEADER+'''Public NotInheritable Class MultipleComparisonGeometry35
Private ReadOnly estimatesData As Double(), lowerData As Double(), upperData As Double(), yData As Double(), plusData As Double(), minusData As Double()
Public Sub New(estimates As Double(),lower As Double(),upper As Double())
If estimates Is Nothing Then Throw New ArgumentNullException(NameOf(estimates))
If lower Is Nothing Then Throw New ArgumentNullException(NameOf(lower))
If upper Is Nothing Then Throw New ArgumentNullException(NameOf(upper))
Dim n=estimates.Length
If n=0 OrElse lower.Length<>n OrElse upper.Length<>n Then Throw New ArgumentException("Comparison arrays must have equal non-zero lengths.",NameOf(estimates))
estimatesData=DirectCast(estimates.Clone(),Double()):lowerData=DirectCast(lower.Clone(),Double()):upperData=DirectCast(upper.Clone(),Double())
yData=ComparisonCoordinatesCore.OrdinalY(n)
ReDim plusData(n-1):ReDim minusData(n-1)
Dim minCI=lowerData(0), maxCI=upperData(0)
For i As Integer=0 To n-1
plusData(i)=Math.Max(0.0,upperData(i)-estimatesData(i))
minusData(i)=Math.Max(0.0,estimatesData(i)-lowerData(i))
minCI=Math.Min(minCI,lowerData(i)):maxCI=Math.Max(maxCI,upperData(i))
Next
Dim bounds=ComparisonCoordinatesCore.IncludeZeroBounds(minCI,maxCI)
MinimumX=bounds.Min:MaximumX=bounds.Max
ZeroReference=ChartReferenceSeries.Vertical(0.0R,0.5R,n+0.5R)
End Sub
Public ReadOnly Property MinimumX As Double
Public ReadOnly Property MaximumX As Double
Public ReadOnly Property ZeroReference As ChartReferenceSeries
'''
for prop,field in [('Estimates','estimatesData'),('LowerLimits','lowerData'),('UpperLimits','upperData'),('YValues','yData'),('PlusError','plusData'),('MinusError','minusData')]:
    CORE+='Public ReadOnly Property '+prop+' As Double()\nGet\nReturn DirectCast('+field+'.Clone(),Double())\nEnd Get\nEnd Property\n'
CORE+='End Class\n'

COMMON=HEADER.replace('Option Infer On','Option Infer On\nOption Compare Text')+'''Imports BESHStatNG
Namespace ExcelCommon
Public NotInheritable Class PlotPointLabel35
Public Sub New(orderedOrdinal As Integer,text As String)
Me.OrderedOrdinal=orderedOrdinal:ExcelPointIndex=ComparisonChartPolicy35.PointIndex(orderedOrdinal):Me.Text=text
End Sub
Public ReadOnly Property OrderedOrdinal As Integer
Public ReadOnly Property ExcelPointIndex As Integer
Public ReadOnly Property Text As String
End Class
Public Module ComparisonChartPolicy35
'''
multi=(HERE/'entry-source/MultipleComparisonPlotExcel.vb.txt').read_text(encoding='utf-8-sig')
for name in ['ValidatePlotData','BuildTitle']:
    COMMON+=method(multi,name).replace('Private Shared ','Public ').replace('vbLf','Convert.ToChar(10).ToString()')+'\n'
COMMON+='''Public Function LabelOrdinals(pointCount As Integer,labelCount As Integer) As Integer()
Dim result As New List(Of Integer)
For i As Integer=1 To Math.Min(pointCount,labelCount)
result.Add(i-1)
Next
Return result.ToArray()
End Function
Public Function LabelAt(labels As String(),orderedOrdinal As Integer) As PlotPointLabel35
Return New PlotPointLabel35(orderedOrdinal,labels(orderedOrdinal))
End Function
Public Function PointIndex(orderedOrdinal As Integer) As Integer
Return orderedOrdinal+1
End Function
Public Function ValueAxisMaximum(count As Integer) As Double
Return count+0.8
End Function
End Module
End Namespace
'''
AGREEMENT=HEADER+'''Imports BESHStatNG
Namespace ExcelCommon
Public NotInheritable Class AgreementPlotSeries35
Private ReadOnly xData As Double(),yData As Double()
Public Sub New(name As String,x As Double(),y As Double())
Me.Name=name:xData=DirectCast(x.Clone(),Double()):yData=DirectCast(y.Clone(),Double())
End Sub
Public ReadOnly Property Name As String
Public ReadOnly Property XValues As Double()
Get
Return DirectCast(xData.Clone(),Double())
End Get
End Property
Public ReadOnly Property YValues As Double()
Get
Return DirectCast(yData.Clone(),Double())
End Get
End Property
End Class
Public Module AgreementPlotPolicy35
Public Function Regression(minimum As Double,maximum As Double,intercept As Double,slope As Double) As IReadOnlyList(Of AgreementPlotSeries35)
Dim fit=ChartReferenceSeries.Affine(minimum,maximum,intercept,slope),identity=ChartReferenceSeries.Identity(minimum,maximum)
Return Array.AsReadOnly(New AgreementPlotSeries35(){New AgreementPlotSeries35("Regression line",identity.XValues,fit.YValues),New AgreementPlotSeries35("Unity line (y = x)",identity.XValues,identity.XValues)})
End Function
Public Function Identity(minimum As Double,maximum As Double) As IReadOnlyList(Of AgreementPlotSeries35)
Dim line=ChartReferenceSeries.Identity(minimum,maximum)
Return Array.AsReadOnly(New AgreementPlotSeries35(){New AgreementPlotSeries35("Identity",line.XValues,line.XValues)})
End Function
Public Function Bland(x As Double(),bias As Double,lower As Double,upper As Double,show As Boolean,sx As Double(),sy As Double()) As IReadOnlyList(Of AgreementPlotSeries35)
Dim span=Chart34.AgreementSeries.AgreementSeriesPreparation34.BaseSpan(x)
Dim result As New List(Of AgreementPlotSeries35)
Dim values As Double()={bias,lower,upper},names As String()={"Bias","Lower LoA","Upper LoA"}
For i As Integer=0 To 2
Dim line=ChartReferenceSeries.Horizontal(span.Min,span.Max,values(i))
result.Add(New AgreementPlotSeries35(names(i),line.XValues,line.YValues))
Next
If show AndAlso sx IsNot Nothing AndAlso sy IsNot Nothing Then result.Add(New AgreementPlotSeries35("Subject means",sx,sy))
Return result.AsReadOnly()
End Function
End Module
End Namespace
'''
LOVE=HEADER+'''Imports BESHStatNG
Namespace ExcelCommon
Public NotInheritable Class PsmLoveChartSpec35
Private ReadOnly beforeData As Double(),matchingData As Double(),weightingData As Double(),yData As Double(),labelData As String(),sourceData As Integer()
Private ReadOnly namesData As String(),channelsData As Integer()
Public Sub New(projection As PsmLovePlotProjection)
yData=projection.YValues:beforeData=projection.Before:matchingData=projection.AfterMatching:weightingData=projection.AfterWeighting:labelData=projection.Labels:sourceData=projection.SourceOrdinals
Threshold=projection.Threshold:MaximumX=projection.MaximumX:MaximumY=projection.Count+0.5R
Dim names As New List(Of String),channels As New List(Of Integer)
If PsmLoveSeriesPolicy.HasAnyFinite(beforeData) Then
names.Add("Before"):channels.Add(0)
End If
If PsmLoveSeriesPolicy.HasAnyFinite(matchingData) Then
names.Add("After matching"):channels.Add(1)
End If
If PsmLoveSeriesPolicy.HasAnyFinite(weightingData) Then
names.Add("After weighting"):channels.Add(2)
End If
namesData=names.ToArray():channelsData=channels.ToArray():LabelSeriesOrdinal=names.Count-1
End Sub
Public ReadOnly Property Threshold As Double
Public ReadOnly Property MaximumX As Double
Public ReadOnly Property MaximumY As Double
Public ReadOnly Property MinimumX As Double=0.0R
Public ReadOnly Property MinimumY As Double=0.5R
Public ReadOnly Property ReverseY As Boolean=True
Public ReadOnly Property Title As String="Love plot: absolute standardized mean differences"
Public ReadOnly Property XAxisTitle As String="Absolute standardized mean difference"
Public Const ThresholdSeriesName As String="Threshold"
Public ReadOnly Property LabelSeriesOrdinal As Integer
Public Function IncludesChannel(channel As Integer) As Boolean
Return channelsData.Contains(channel)
End Function
Public Function NameForChannel(channel As Integer) As String
Select Case channel
Case 0:Return "Before"
Case 1:Return "After matching"
Case 2:Return "After weighting"
Case Else:Throw New ArgumentOutOfRangeException(NameOf(channel))
End Select
End Function
Public ReadOnly Property Names As String()
Get
Return DirectCast(namesData.Clone(),String())
End Get
End Property
Public ReadOnly Property Channels As Integer()
Get
Return DirectCast(channelsData.Clone(),Integer())
End Get
End Property
Public ReadOnly Property SourceOrdinals As Integer()
Get
Return DirectCast(sourceData.Clone(),Integer())
End Get
End Property
Public ReadOnly Property Labels As String()
Get
Return DirectCast(labelData.Clone(),String())
End Get
End Property
'''
for prop,field in [('Before','beforeData'),('AfterMatching','matchingData'),('AfterWeighting','weightingData'),('YValues','yData')]:LOVE+='Public ReadOnly Property '+prop+' As Double()\nGet\nReturn DirectCast('+field+'.Clone(),Double())\nEnd Get\nEnd Property\n'
LOVE+='End Class\nEnd Namespace\n'

def apply():
    assert (HERE/'pre-edit-capture.csv').is_file()
    assert all((ROOT/'.test-rationalization-04A35.tmp'/('baseline-'+c+('-matrix.json' if c=='Debug' else '-managed.json'))).exists() for c in ['Debug','Release'])
    for path,text in [('BESHStat.Core/src/Graphics/MultipleComparisonGeometry35.vb',CORE),('BESHStat.Excel.Common/src/Analysis/ComparisonChartPolicy35.vb',COMMON),('BESHStat.Excel.Common/src/Analysis/AgreementPlotPolicy35.vb',AGREEMENT),('BESHStat.Excel.Common/src/Analysis/PsmLoveChartSpec35.vb',LOVE)]:write(path,text)
    p='BESHstatNG/src/Graphics/MultipleComparisonPlotExcel.vb';s=(ROOT/p).read_text(encoding='utf-8-sig')
    a=s.index('            Dim y As Double()');b=s.index('            Dim xScale As CHARTscale',a)
    s=s[:a]+'''            Dim prepared As New MultipleComparisonGeometry35(data.Estimates, data.LowerLimits, data.UpperLimits)
            Dim y As Double() = prepared.YValues
            Dim plusError As Double() = prepared.PlusError
            Dim minusError As Double() = prepared.MinusError
            Dim minCI As Double = prepared.MinimumX
            Dim maxCI As Double = prepared.MaximumX
'''+s[b:]
    for name in ['BuildTitle','ValidatePlotData']:
        old=method(s,name);header=next(line for line in old.splitlines() if line.strip())
        body='\n            '+('Return ' if name=='BuildTitle' else '')+'ExcelCommon.ComparisonChartPolicy35.'+name+'(data)\n        End '+('Function' if name=='BuildTitle' else 'Sub')
        s=s.replace(old,header+body)
    s=s.replace('data.Estimates.Length + 0.8','ExcelCommon.ComparisonChartPolicy35.ValueAxisMaximum(data.Estimates.Length)')
    s=s.replace('For i As Integer = 1 To Math.Min(series.Points.Count, labels.Length)','For Each ordinal As Integer In ExcelCommon.ComparisonChartPolicy35.LabelOrdinals(series.Points.Count, labels.Length)\n                    Dim i As Integer = ExcelCommon.ComparisonChartPolicy35.PointIndex(ordinal)').replace('point.DataLabel.Text = labels(i - 1)','point.DataLabel.Text = ExcelCommon.ComparisonChartPolicy35.LabelAt(labels, ordinal).Text')
    # Keep original live estimate/title/labels read boundaries after native creation.
    s=s.replace('ChartReferenceSeries.Vertical(0.0R, 0.5R, n + 0.5R).XValues','prepared.ZeroReference.XValues').replace('ChartReferenceSeries.Vertical(0.0R, 0.5R, n + 0.5R).YValues','prepared.ZeroReference.YValues')
    write(p,s)
    p='BESHstatNG/src/Graphics/AgreementPlotExcel.vb';s=(ROOT/p).read_text(encoding='utf-8-sig')
    s=s.replace('        With ch\n            .HasLegend = True','        Dim references = ExcelCommon.AgreementPlotPolicy35.Regression(plotData.MinX, plotData.MaxX, plotData.Intercept, plotData.Slope)\n        With ch\n            .HasLegend = True')
    s=s.replace('ChartReferenceSeries.Identity(plotData.MinX, plotData.MaxX).XValues','references(0).XValues',2) # first regression and first unity X
    # Apply per series block, keeping native block/order/style exact.
    for start,stop in [('With .SeriesCollection(2)','With .SeriesCollection(3)')]:
        pass
    s=s.replace('ChartReferenceSeries.Affine(plotData.MinX, plotData.MaxX, plotData.Intercept, plotData.Slope).YValues','references(0).YValues').replace('.Name = "Regression line"','.Name = references(0).Name').replace('.Name = "Unity line (y = x)"','.Name = references(1).Name')
    # All remaining minX identity endpoints are the unity pair (or second overload fit X).
    # Resolve complete series blocks by their fixed native series ordinal.
    pattern=r'With \.SeriesCollection\(([23])\)(.*?)(?=\n            End With)'
    def endpoint(m):
        index=int(m[1])-2;body=m[2]
        body=re.sub(r'(?m)^(\s*\.XValues = ).*',lambda n:n[1]+f'references({index}).XValues',body)
        body=re.sub(r'(?m)^(\s*\.Values = ).*',lambda n:n[1]+f'references({index}).YValues',body)
        return 'With .SeriesCollection('+m[1]+')'+body
    s=re.sub(pattern,endpoint,s,flags=re.S)
    a=s.index('        Dim span =');b=s.index('            ch.SeriesCollection.NewSeries()',a)
    s=s[:a]+'''        Dim references = ExcelCommon.AgreementPlotPolicy35.Bland(plotData.XValues, plotData.Bias, plotData.LowerLoA, plotData.UpperLoA, plotData.ShowSubjectMeans, plotData.SubjectMeanX, plotData.SubjectMeanY)
        AddHorizontalReferenceLine(ch, references(0).XValues, references(0).YValues(0), references(0).Name, RGB(31, 119, 180))
        AddHorizontalReferenceLine(ch, references(1).XValues, references(1).YValues(0), references(1).Name, RGB(214, 39, 40))
        AddHorizontalReferenceLine(ch, references(2).XValues, references(2).YValues(0), references(2).Name, RGB(214, 39, 40))

        If references.Count > 3 Then
'''+s[b:]
    s=s.replace('.XValues = plotData.SubjectMeanX','.XValues = references(3).XValues').replace('.Values = plotData.SubjectMeanY','.Values = references(3).YValues').replace('.Name = "Subject means"','.Name = references(3).Name')
    s=s.replace('        With chartObj\n','        Dim references = ExcelCommon.AgreementPlotPolicy35.Identity(plotData.MinValue, plotData.MaxValue)\n        With chartObj\n')
    s=s.replace('.Name = "Identity"','.Name = references(0).Name').replace('ChartReferenceSeries.Identity(plotData.MinValue, plotData.MaxValue).XValues','references(0).XValues',1).replace('ChartReferenceSeries.Identity(plotData.MinValue, plotData.MaxValue).XValues','references(0).YValues')
    write(p,s)
    p='BESHStat.Excel.Common/src/Analysis/PsmLovePlotProjection.vb';s=(ROOT/p).read_text(encoding='utf-8-sig')
    s=s.replace('        Public Sub New(source','        Private ReadOnly sourceIndices As Integer()\n        Public Sub New(source')
    a=s.index('            rows =');b=s.index('            Dim value =',a)
    ordered='''            Dim ordered = If(source, New List(Of PsmLovePlotRow)()).
                Select(Function(r, i) New With {.Row = r, .Ordinal = i}).
                Where(Function(item) item.Row IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(item.Row.VariableName)).
                OrderByDescending(Function(item) MaxFiniteAbs(item.Row.SmdBefore, item.Row.SmdAfterMatching, item.Row.SmdAfterWeighting)).
                ThenBy(Function(item) item.Row.VariableName, StringComparer.OrdinalIgnoreCase).ToArray()
            sourceIndices = ordered.Select(Function(item) item.Ordinal).ToArray()
            rows = ordered.Select(Function(item) New PsmLovePlotRow With {.VariableName = item.Row.VariableName, .SmdBefore = item.Row.SmdBefore,
                .SmdAfterMatching = item.Row.SmdAfterMatching, .SmdAfterWeighting = item.Row.SmdAfterWeighting,
                .Threshold = item.Row.Threshold, .Flag = item.Row.Flag}).ToArray()
'''
    s=s[:a]+ordered+s[b:]
    pos=s.index('        Public ReadOnly Property Count')
    s=s[:pos]+'''        Public ReadOnly Property SourceOrdinals As Integer()
            Get
                Return DirectCast(sourceIndices.Clone(), Integer())
            End Get
        End Property
'''+s[pos:];write(p,s)
    p='BESHstatNG/src/Graphics/PsmLovePlotExcel.vb';s=(ROOT/p).read_text(encoding='utf-8-sig')
    a=s.index('            Dim yValues =');b=s.index('            Dim shp As Shape',a)
    s=s[:a]+'''            Dim spec As New PsmLoveChartSpec35(projection)
            Dim yValues = spec.YValues
            Dim xBefore = spec.Before
            Dim xAfterMatching = spec.AfterMatching
            Dim xAfterWeighting = spec.AfterWeighting
            Dim labels = spec.Labels
            Dim threshold = spec.Threshold
            Dim maxX = spec.MaximumX

'''+s[b:]
    for i,name in enumerate(['xBefore','xAfterMatching','xAfterWeighting']):s=s.replace('If HasAnyFinite('+name+') Then','If spec.IncludesChannel('+str(i)+') Then')
    for i,name in enumerate(['Before','After matching','After weighting']):s=s.replace('AddScatterSeries(ch, "'+name+'",','AddScatterSeries(ch, spec.NameForChannel('+str(i)+'),')
    s=s.replace('.Name = "Threshold"','.Name = PsmLoveChartSpec35.ThresholdSeriesName')
    s=s.replace('plottedSeries(plottedSeries.Count - 1)','plottedSeries(spec.LabelSeriesOrdinal)').replace('.ChartTitle.Text = "Love plot: absolute standardized mean differences"','.ChartTitle.Text = spec.Title').replace('.AxisTitle.Text = "Absolute standardized mean difference"','.AxisTitle.Text = spec.XAxisTitle').replace('.MinimumScale = 0\n','.MinimumScale = spec.MinimumX\n').replace('.MinimumScale = 0.5R','.MinimumScale = spec.MinimumY').replace('.MaximumScale = projection.Count + 0.5R','.MaximumScale = spec.MaximumY').replace('.ReversePlotOrder = True','.ReversePlotOrder = spec.ReverseY')
    s=s.replace('For i As Integer = 1 To Math.Min(labels.Length, series.Points.Count)','For Each ordinal As Integer In ComparisonChartPolicy35.LabelOrdinals(series.Points.Count, labels.Length)\n                    Dim i As Integer = ComparisonChartPolicy35.PointIndex(ordinal)').replace('pt.DataLabel.Text = labels(i - 1)','pt.DataLabel.Text = ComparisonChartPolicy35.LabelAt(labels, ordinal).Text')
    # Love originally evaluates labels.Length before the native point count.
    s=s.replace('ComparisonChartPolicy35.LabelOrdinals(series.Points.Count, labels.Length)','ComparisonChartPolicy35.LabelOrdinals(labels.Length, series.Points.Count)')
    write(p,s)
    print('Installed bounded35 Core/Common preparation and actual three adapters')
if __name__=='__main__':apply()
