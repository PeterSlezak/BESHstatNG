"""Bounded one-time 04A32 implementation. Transform definitions also serve as the
reviewable source-delta specification. Never writes prior evidence/tests/plan.
"""
from audit import ROOT,HERE,RUN,BASE,WINDOWS,CORE_OLD,CORE_NEW,load,save,pin,sha
import re,sys
def replace(s,a,b):
 assert s.count(a)==1,(a[:90],s.count(a));return s.replace(a,b)
def method(s,name):
 m=re.search(r'(?im)^ *(?:(?:Public|Private|Friend|Shared) +)*(?:Sub|Function) '+re.escape(name)+r'\(',s)
 assert m,name
 end=re.search(r'(?im)^ *End (?:Sub|Function) *$',s[m.end():]);return s[m.start():m.end()+end.end()]
def body_replace(s,name,newbody):
 old=method(s,name);endheader=old.index('\n')
 # Only use for single-line headers.
 return replace(s,old,old[:endheader]+'\n'+newbody+'\n'+('        End Function' if 'Function '+name in old else '        End Sub'))
def transforms(original):
 out={};s=original[WINDOWS[0]]
 s=replace(s,'Dim arTemporal() As Double = DirectCast(pData(i).Clone(), Double())\n\n                \'Fit quantiles', 'Dim aggregate = BoxPlotDataCore.Aggregate(pData(i))\n                Dim arTemporal() As Double = aggregate.SortedValues\n                Dim smallFlags = aggregate.SmallOutlierFlags, largeFlags = aggregate.LargeOutlierFlags\n\n                \'Fit quantiles')
 s=replace(s,'Dim DescriptiveS As DescriptiveStat = New DescriptiveStat(arTemporal)\n                DescriptiveS.compute(False)','Dim DescriptiveS As BoxGroupAggregateData = aggregate')
 s=replace(s,'If arTemporal(j) < (.LQuartile - 1.5 * .IQR) Then','If smallFlags(CInt(j)) Then')
 s=replace(s,'If arTemporal(j) > (.UQuartile + 1.5 * .IQR) Then','If largeFlags(CInt(j)) Then')
 s=replace(s,'With Me.pDSCollection.Item(i + 1)','With DirectCast(Me.pDSCollection.Item(i + 1), BoxGroupAggregateData)')
 start=s.index("                    'Sort a clone for whisker lookup")
 end=s.index('                    pQ1(i) = .LQuartile',start)
 s=s[:start]+'''                    pWhiskerQ3(i) = BoxPlotDataCore.UpperWhisker(DirectCast(pDSCollection.Item(i + 1), BoxGroupAggregateData), pData(i), CInt(pNs(i)), pArNOutliersBig(i))
                    pWhiskerQ1(i) = BoxPlotDataCore.LowerWhisker(DirectCast(pDSCollection.Item(i + 1), BoxGroupAggregateData), pData(i), CInt(pNs(i)), pArNOutliersSmall(i))

'''+s[end:]
 s=replace(s,'udBoxPlotAxis = ChartScaling(pMins.Min(), pMaxs.Max())','Dim axis = BoxPlotDataCore.Axis(pMins, pMaxs)\n            udBoxPlotAxis = New CHARTscale With {.Min = axis.Min, .Max = axis.Max, .Scale = axis.Scale}')
 start=s.index('                Dim xValues(observations.Length - 1) As Double');end=s.index('                Dim observationSeries',start)
 s=s[:start]+'''                Dim xValues = DistributionPlotGeometryCore.ObservationX(groupIndex + 1.0, observations.Length, appearance.ObservationJitterHalfWidth)

'''+s[end:]
 old=method(s,'DeterministicJitter');header=old[:old.index('\n')];s=replace(s,old,header+'\n            Return DistributionPlotGeometryCore.DeterministicJitter(index, halfWidth)\n        End Function')
 out[WINDOWS[0]]=s
 s=original[WINDOWS[1]]
 s=replace(s,'Private data() As Double','Private data() As Double\n        Private pBins(,) As Object\n        Private pClassificationEdges() As Double')
 s=replace(s,'Dim bins = HistogramDataCore.BinsCopy(Me.data, strType)','Dim binned = HistogramDataCore.BinsAndEdgesCopy(Me.data, strType)\n            Dim bins = binned.Bins\n            pBins = DirectCast(bins.Clone(), Object(,))\n            pClassificationEdges = binned.Edges')
 start=s.index('                Dim sorted() As Double = DirectCast(Me.data.Clone(), Double())');end=s.index('                Me.pHasBoxSummary = True',start)
 s=s[:start]+'''                Dim summary As New HistogramTukeyData(Me.data)
                Me.pQ1 = summary.Q1
                Me.pMedian = summary.Median
                Me.pQ3 = summary.Q3
                Me.pWhiskerLow = summary.WhiskerLow
                Me.pWhiskerHigh = summary.WhiskerHigh
                Me.arOutliers = summary.Outliers
'''+s[end:]
 start=s.index("            'The column chart's categories");end=s.index('            Dim anchor As Range',start)
 s=s[:start]+'''            Dim numeric = PlotData(bBoxPlot).Axis
            Dim dXMin = numeric.XMinimum, dXMax = numeric.XMaximum
            Dim dYMin = numeric.YMinimum, dYMax = numeric.YMaximum, dYMajor = numeric.YMajorUnit
            Dim dBoxY = numeric.BoxCentreY, dBoxHalfHeight = numeric.BoxHalfHeight

'''+s[end:]
 marker='        Private Shared ReadOnly pBoxPlotOverlays As New List(Of HistogramBoxPlotOverlay)'
 s=replace(s,marker,'''        Public Function PlotData(Optional drawBox As Boolean = False) As HistogramPlotSeriesData
            Return New HistogramPlotSeriesData(pBins, pClassificationEdges, arBinMidVal, arFreq, arXi, arGauss, pHasBoxSummary,
                                               {pQ1, pMedian, pQ3, pWhiskerLow, pWhiskerHigh}, arOutliers, drawBox)
        End Function

'''+marker)
 out[WINDOWS[1]]=s
 s=original[WINDOWS[2]]
 s=replace(s,'Private NotInheritable Class AxisContext\n','Private NotInheritable Class AxisContext\n        Friend Domain As ViolinPlotDomainData\n')
 old=method(s,'ResolveAxisContext');header=old[:old.index(' As AxisContext')+len(' As AxisContext')]
 new=header+'''
        Dim domain As New ViolinPlotDomainData(result, appearance.YAxisMinimum, appearance.YAxisMaximum, appearance.YAxisMajorUnit, appearance.ObservationJitterHalfWidth)
        Return New AxisContext With {.XMinimum = domain.XMinimum, .XMaximum = domain.XMaximum, .YMinimum = domain.YMinimum, .YMaximum = domain.YMaximum, .Domain = domain}
    End Function'''
 s=replace(s,old,new)
 old=method(s,'ResolveAutomaticYLimits');header=old[:old.index('ByRef axisMaximum As Double)')+len('ByRef axisMaximum As Double)')]
 s=replace(s,old,header+'''
        Dim bounds = ViolinPlotDomainData.AutomaticLimits(dataMinimum, dataMaximum)
        axisMinimum = bounds(0) : axisMaximum = bounds(1)
    End Sub''')
 start=s.index('        Dim xValues(series.Length - 1) As Double');end=s.index('        seriesCollection.NewSeries()',start)
 s=s[:start]+'''        Dim xValues = axisContext.Domain.GroupPositions
        Dim yValues = axisContext.Domain.CategoryLabelY

'''+s[end:]
 s=replace(s,'Dim groupX As Double = groupIndex + 1.0R','Dim groupX As Double = axisContext.Domain.GroupPositions(groupIndex)')
 start=s.index('        If evaluationPoints.Length <> halfWidths.Length');end=s.index('        Dim builder As Object',start)
 s=s[:start]+'''        Dim outline = DistributionPlotGeometryCore.ViolinOutline(groupX, evaluationPoints, halfWidths)
        Dim dataX = outline.X, dataY = outline.Y
        Dim pointCount = dataX.Length
        Dim xs(pointCount - 1), ys(pointCount - 1) As Double
        For i As Integer = 0 To pointCount - 1
            xs(i) = DataXToPlotX(dataX(i), axisContext, insideLeft, insideWidth)
            ys(i) = DataYToPlotY(dataY(i), axisContext, insideTop, insideHeight)
        Next

'''+s[end:]
 start=s.index('        Dim sourceHalfWidths() As Double = source.ScaledHalfWidths');end=s.index('        Dim q1Y As Double',start)
 s=s[:start]+'''        Dim innerX = DistributionPlotGeometryCore.ViolinInnerX(groupX, source.ScaledHalfWidths, appearance.InnerBoxHalfWidth, appearance.WhiskerCapHalfWidth)
'''+s[end:]
 for a,b in [('groupX - effectiveBoxHalfWidth','innerX(0)'),('groupX + effectiveBoxHalfWidth','innerX(1)'),('groupX - effectiveCapHalfWidth','innerX(2)'),('groupX + effectiveCapHalfWidth','innerX(3)')]:s=replace(s,a,b)
 start=s.index('        For i As Integer = 0 To observations.Length - 1');s=s[:start]+'''        Dim visible = DistributionPlotGeometryCore.VisibleObservations(observations, axisContext.YMinimum, axisContext.YMaximum)
        Dim observationX = axisContext.Domain.Series(groupIndex).ObservationX
'''+s[start:]
 s=replace(s,'If observation < axisContext.YMinimum OrElse observation > axisContext.YMaximum Then','If Not visible(i) Then')
 s=replace(s,'Dim jitter As Double = DeterministicJitter(i, appearance.ObservationJitterHalfWidth)','Dim dataX As Double = observationX(i)')
 s=replace(s,'DataXToPlotX(groupX + jitter,','DataXToPlotX(dataX,')
 old=method(s,'DeterministicJitter');header=old[:old.index('halfWidth As Double) As Double')+len('halfWidth As Double) As Double')]
 s=replace(s,old,header+'\n        Return DistributionPlotGeometryCore.DeterministicJitter(index, halfWidth)\n    End Function')
 out[WINDOWS[2]]=s
 s=original[CORE_OLD[0]];old=method(s,'HistogramBinsComputation')
 header=old[:old.index('\n')];body=old[old.index('\n'):]
 moved=body.replace('        Return out','        capturedEdges = DirectCast(breaks.Clone(), Double())\n        Return out')
 replacement=header+'''
            Dim edges() As Double = Nothing
            Return ComputeBins(arInput, strType, edges)
        End Function

        Public Shared Function ComputeWithEdges(arInput() As Double, strType As String) As HistogramBinningData
            Dim edges() As Double = Nothing
            Dim bins = ComputeBins(DirectCast(arInput.Clone(), Double()), strType, edges)
            Return New HistogramBinningData(bins, edges)
        End Function

        Private Shared Function ComputeBins(arInput() As Double, strType As String, ByRef capturedEdges() As Double) As Object(,)'''+moved
 s=replace(s,old,replacement)
 s+='''
Public NotInheritable Class HistogramBinningData
    Private ReadOnly binData(,) As Object, edgeData() As Double
    Friend Sub New(bins(,) As Object, edges() As Double)
        binData = DirectCast(bins.Clone(), Object(,)) : edgeData = DirectCast(edges.Clone(), Double())
    End Sub
    Public ReadOnly Property Bins As Object(,)
        Get
            Return DirectCast(binData.Clone(), Object(,))
        End Get
    End Property
    Public ReadOnly Property Edges As Double()
        Get
            Return DirectCast(edgeData.Clone(), Double())
        End Get
    End Property
End Class
'''
 out[CORE_OLD[0]]=s
 s=original[CORE_OLD[1]];marker='    Public Function OverlayCopy('
 pos=s.index(marker);s=s[:pos]+'''    Public Function BinsAndEdgesCopy(values As Double(), rule As String) As HistogramBinningData
        Return HistogramBinningCore.ComputeWithEdges(DirectCast(values.Clone(), Double()), rule)
    End Function

'''+s[pos:];out[CORE_OLD[1]]=s
 s=original[CORE_OLD[2]]
 s=replace(s,'Private ReadOnly _observations As Double()','Private ReadOnly _sourceOrdinals As Integer()\n    Private ReadOnly _observations As Double()')
 old=method(s,'New');sig=old[:old.index('        _name = name')].replace('whiskerHigh As Double)','whiskerHigh As Double, sourceOrdinals As Integer())')
 ctor=sig+'''        Me.New(name, groupValue, observations, evaluationPoints, density, scaledHalfWidths, bandwidth, minimum, q1, median, q3, maximum, mean, whiskerLow, whiskerHigh)
        _sourceOrdinals = DirectCast(sourceOrdinals.Clone(), Integer())
    End Sub

    Public ReadOnly Property SourceObservationOrdinals As Integer()
        Get
            Return If(_sourceOrdinals Is Nothing, Nothing, DirectCast(_sourceOrdinals.Clone(), Integer()))
        End Get
    End Property
'''
 s=replace(s,old,old+'\n'+ctor)
 s=replace(s,'Friend Values As New List(Of Double)()','Friend Values As New List(Of Double)()\n        Friend SourceOrdinals As New List(Of Integer)()')
 s=replace(s,'Friend Observations As Double()','Friend Observations As Double()\n        Friend SourceOrdinals As Integer()')
 s=replace(s,'working.Values.Add(x)','working.Values.Add(x)\n            working.SourceOrdinals.Add(i)')
 s=replace(s,'.Observations = observations,','.Observations = observations,\n                .SourceOrdinals = group.SourceOrdinals.ToArray(),')
 s=replace(s,'ws.WhiskerHigh)','ws.WhiskerHigh,\n                                                      ws.SourceOrdinals)')
 out[CORE_OLD[2]]=s
 return out
def install():
 assert load(RUN/'before-Debug-matrix.json')['source_unchanged']
 expected={r['path']:r for r in load(HERE/'entry-pins.json')['files']}
 original={p:(ROOT/p).read_text(encoding='utf-8-sig') for p in WINDOWS+CORE_OLD}
 for p in original:assert pin(ROOT/p)==expected[p]
 for p in CORE_NEW:
  assert not (ROOT/p).exists();(ROOT/p).write_text((RUN/(p.split('/')[-1]+'.txt')).read_text(),encoding='utf-8',newline='\n')
 for p,text in transforms(original).items():(ROOT/p).write_text(text,encoding='utf-8',newline='\n')
 target=ROOT/'BESHStat.Core.Tests/src/Graphics/LegacyGraphics32.vb';assert not target.exists();target.write_bytes((HERE/'LegacyGraphics32.vb.txt').read_bytes())
 save('implementation-receipt.json',dict(schema='04A32-implementation-v1',base=BASE,authorized=WINDOWS+CORE_OLD+CORE_NEW,fixture=pin(target),scope='Numeric preparation,identity only;no downstream batch/native corrections'))
 print('Installed bounded Core/Windows extraction and frozen compatibility fixture')
if __name__=='__main__':install()
