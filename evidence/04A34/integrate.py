"""Deterministic 04A34 extraction from frozen entry source and numerical slices.

Run only after entry compatibility captures have been executed. Source guard
rejects any unexpected production edits. Historical source is never rewritten.
"""
from entry import ROOT,HERE,BASE,git,save,pin
from freeze_slices import member,source,body
import re

def write(path,text):
    p=ROOT/path
    p.write_text(text,encoding='utf-8',newline='\n')
def header(imports=''):
    return 'Option Explicit On\nOption Strict On\nOption Infer On\nImports System\nImports System.Linq\nImports System.Collections.Generic\n'+imports+'\n'
def replace_member(s,name,new):
    m=re.search(r'^[ \t]+(?:Private|Public|Friend) (?:Shared )?(?:Function|Sub) '+name+r'\b.*?^[ \t]+End (?:Function|Sub)',s,re.M|re.S)
    assert m,name
    return s[:m.start()]+new+s[m.end():]
def single_function(file,name,signature,subs=None):
    b=body(member(file,name))
    for a,z in (subs or {}).items():b=b.replace(a,z)
    return signature+'\n'+b+'End Function\n'

def extract():
    assert git('rev-parse','HEAD')==BASE
    assert (HERE/'pre-edit-compatibility-capture.csv').is_file()
    assert not git('diff','--name-only'),'Unexpected product edit before extraction'
    # One shared primitive removes repeated two-point array construction. All
    # endpoints are explicit; this does not fit or infer any statistical model.
    write('BESHStat.Core/src/Graphics/ChartReferenceSeries.vb',header()+'\n'.join([
        "''' <summary>Copy-safe two-point reference coordinates; supplied endpoints retain their order.</summary>",
        'Public NotInheritable Class ChartReferenceSeries',
        '    Private ReadOnly _x As Double()', '    Private ReadOnly _y As Double()',
        '    Private Sub New(x0 As Double, x1 As Double, y0 As Double, y1 As Double)',
        '        _x = New Double() {x0,x1}', '        _y = New Double() {y0,y1}', '    End Sub',
        '    Public ReadOnly Property XValues As Double()', '        Get', '            Return DirectCast(_x.Clone(), Double())', '        End Get', '    End Property',
        '    Public ReadOnly Property YValues As Double()', '        Get', '            Return DirectCast(_y.Clone(), Double())', '        End Get', '    End Property',
        '    Public Shared Function Identity(minimum As Double, maximum As Double) As ChartReferenceSeries',
        '        Return New ChartReferenceSeries(minimum,maximum,minimum,maximum)', '    End Function',
        '    Public Shared Function Affine(minimum As Double, maximum As Double, intercept As Double, slope As Double) As ChartReferenceSeries',
        '        Return New ChartReferenceSeries(minimum,maximum,intercept + slope * minimum,intercept + slope * maximum)', '    End Function',
        '    Public Shared Function Horizontal(x0 As Double, x1 As Double, value As Double) As ChartReferenceSeries',
        '        Return New ChartReferenceSeries(x0,x1,value,value)', '    End Function',
        '    Public Shared Function Vertical(value As Double, y0 As Double, y1 As Double) As ChartReferenceSeries',
        '        Return New ChartReferenceSeries(value,value,y0,y1)', '    End Function', 'End Class','']))
    write('BESHStat.Core/src/Graphics/CalibrationSeriesData.vb',header('Imports BESHStatNG.regression\n')+"""
''' <summary>Ordered calibration bins. Historical admission rejects NaN only; infinities are retained.</summary>
Public NotInheritable Class CalibrationSeriesData
    Private ReadOnly _x As Double()
    Private ReadOnly _y As Double()
    Private ReadOnly _ordinals As Integer()
    Public Sub New(rows As IList(Of CalibrationBinSummary))
        Dim xs As New List(Of Double)()
        Dim ys As New List(Of Double)()
        Dim ordinals As New List(Of Integer)()
        If rows IsNot Nothing Then
            Dim inputOrdinal As Integer = 0
            For Each row As CalibrationBinSummary In rows
                If Not Double.IsNaN(row.MeanPredicted) AndAlso Not Double.IsNaN(row.ObservedRate) Then
                    xs.Add(row.MeanPredicted)
                    ys.Add(row.ObservedRate)
                    ordinals.Add(inputOrdinal)
                End If
                inputOrdinal += 1
            Next
        End If
        _x = xs.ToArray()
        _y = ys.ToArray()
        _ordinals = ordinals.ToArray()
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
    ''' <summary>Zero-based input-bin ordinals, without worksheet-row provenance.</summary>
    Public ReadOnly Property SourceOrdinals As Integer()
        Get
            Return DirectCast(_ordinals.Clone(), Integer())
        End Get
    End Property
    Public ReadOnly Property Count As Integer
        Get
            Return _x.Length
        End Get
    End Property
    Public ReadOnly Property Reference As ChartReferenceSeries
        Get
            Return ChartReferenceSeries.Identity(0.0R, 1.0R)
        End Get
    End Property
    Public ReadOnly Property Axis As ChartAxisScaleResult
        Get
            Return New ChartAxisScaleResult(0.0R, 1.0R, 0.2R)
        End Get
    End Property
End Class
""")
    # Calibration snapshots at the original render-time loop, preserving the
    # constructor alias and worksheet/null/count guards exactly.
    s=source('CalibrationPlot');start=s.index('            Dim xs As New List(Of Double)()');end=s.index('            Dim shp =',start)
    s=s[:start]+"""            Dim projection As New CalibrationSeriesData(Me.pRows)
            If projection.Count = 0 Then Return Nothing
            Dim reference As ChartReferenceSeries = projection.Reference
            Dim axis As ChartAxisScaleResult = projection.Axis

"""+s[end:]
    s=s.replace('xs.ToArray()','projection.XValues').replace('ys.ToArray()','projection.YValues').replace('New Double() {0.0R, 1.0R}','reference.XValues',1).replace('New Double() {0.0R, 1.0R}','reference.YValues',1)
    s=s.replace('.MinimumScale = 0','.MinimumScale = axis.Min').replace('.MaximumScale = 1','.MaximumScale = axis.Max').replace('.MajorUnit = 0.2','.MajorUnit = axis.Scale')
    write('BESHstatNG/src/Graphics/CalibrationPlot.vb',s)
    # Exact sorted-tail loop, with the accepted historical orientation. Windows
    # retains its alias-sort and median evaluation before the new snapshot.
    s=source('SymetryPlot');start=s.index('            Dim Xref() As Double, Yref() As Double');end=s.index('            If lLeft =',start)
    original=s[start:end];b=original[original.index('            Dim x(n - 1) As Double'):original.index('            If lLeft =') if '            If lLeft =' in original else len(original)]
    b=b.replace('pData','sortedData').replace('Dim ii As Integer = RoundDown(n / 2, 0)','Dim ii As Integer = CInt(StatFunc.RoundDown(n / 2, 0))').replace('ChartScaling(0, x.Max())','ChartAxisScalingCore.Compute(0, x.Max())')
    b=b.replace('            Dim udPlotAxis =','            Dim udPlotAxis =')
    properties=''
    for name,field in [('XValues','_x'),('YValues','_y'),('ReferenceX','_ref'),('ReferenceY','_ref')]:
        properties+=f'    Public ReadOnly Property {name} As Double()\n        Get\n            Return DirectCast({field}.Clone(), Double())\n        End Get\n    End Property\n'
    write('BESHStat.Core/src/Graphics/SymmetrySeriesData.vb',header()+"""
''' <summary>Historical upper-tail X / lower-tail Y coordinates. Axis captions remain host-owned.</summary>
Public NotInheritable Class SymmetrySeriesData
    Private ReadOnly _x As Double()
    Private ReadOnly _y As Double()
    Private ReadOnly _ref As Double()
    Public ReadOnly Property Axis As ChartAxisScaleResult
    Private Sub New(x As Double(), y As Double(), reference As Double(), axisValue As ChartAxisScaleResult)
        _x = DirectCast(x.Clone(), Double())
        _y = DirectCast(y.Clone(), Double())
        _ref = DirectCast(reference.Clone(), Double())
        Axis = axisValue
    End Sub
    ''' <summary>Snapshots already sorted observations; the caller owns sorting and median calculation.</summary>
    Public Shared Function FromSorted(values As Double(), medianValue As Double) As SymmetrySeriesData
        Dim sortedData As Double() = DirectCast(values.Clone(), Double())
        Dim n As Integer = sortedData.Length
        Dim Medn As Double = medianValue
        Dim Xref() As Double, Yref() As Double
"""+b+"""        Return New SymmetrySeriesData(Xs, Ys, Xref, udPlotAxis)
    End Function
"""+properties+'End Class\n')
    s=s[:start]+"""            'Retain the historical caller-array sort and its evaluation timing.
            Dim Medn As Double = Median(Me.pData)
            Array.Sort(Me.pData)
            Dim projection As SymmetrySeriesData = SymmetrySeriesData.FromSorted(Me.pData, Medn)
            Dim Xs As Double() = projection.XValues
            Dim Ys As Double() = projection.YValues
            Dim Xref As Double() = projection.ReferenceX
            Dim Yref As Double() = projection.ReferenceY
            Dim udPlotAxis As ChartAxisScaleResult = projection.Axis

"""+s[end:]
    write('BESHstatNG/src/Graphics/SymetryPlot.vb',s)
    # Direct numerical helper extractions. Every returned array is new, or a
    # defensive copy at the public snapshot seam.
    for file,core,names in [
        ('LadderPlotExcel','LadderLabelGeometryCore',['SpreadLabelPositions','Clamp','IsFinite','IsFinitePositive']),
        ('GifAnimator','AnimationFrameTimingCore',['MsToCentiseconds'])]:
        pieces=[member(file,n).replace('Private Shared ','Public ').replace('Private Function ','Public Function ') for n in names]
        write('BESHStat.Core/src/Graphics/'+core+'.vb',header()+'Public Module '+core+'\n'+'\n'.join(pieces)+'End Module\n')
        s=source(file)
        for n in names[:1]:
            old=member(file,n);sig=old[:old.index('\n',old.index(') As '))] if ') As ' in old else ''
            if n=='SpreadLabelPositions':new='    Private Shared Function SpreadLabelPositions(values As Double(), minimum As Double, maximum As Double, requestedSeparation As Double) As Double()\n        Return LadderLabelGeometryCore.SpreadLabelPositions(values, minimum, maximum, requestedSeparation)\n    End Function'
            else:new='    Private Function MsToCentiseconds(ms As Integer) As Integer\n        Return AnimationFrameTimingCore.MsToCentiseconds(ms)\n    End Function'
            s=replace_member(s,n,new)
        write('BESHstatNG/src/Graphics/'+file+'.vb',s)
    # Preserve private ECDF alias behavior for rejected/no-extension input; the
    # public portable snapshot always owns its arrays.
    legacy=member('CumulativeDistributionPlotExcel','ExtendStepToAxisBounds').replace('Private Shared ','Private ')
    write('BESHStat.Core/src/Graphics/CumulativeStepSeriesData.vb',header()+"""
Public NotInheritable Class CumulativeStepSeriesData
    Private ReadOnly _x As Double()
    Private ReadOnly _y As Double()
    Public Sub New(sourceX As Double(), sourceY As Double(), minimum As Double, maximum As Double)
        Dim x As Double() = If(sourceX Is Nothing, Nothing, DirectCast(sourceX.Clone(), Double()))
        Dim y As Double() = If(sourceY Is Nothing, Nothing, DirectCast(sourceY.Clone(), Double()))
        ExtendStepToAxisBounds(x, y, minimum, maximum, _x, _y)
    End Sub
    Public ReadOnly Property XValues As Double()
        Get
            Return If(_x Is Nothing, Nothing, DirectCast(_x.Clone(), Double()))
        End Get
    End Property
    Public ReadOnly Property YValues As Double()
        Get
            Return If(_y Is Nothing, Nothing, DirectCast(_y.Clone(), Double()))
        End Get
    End Property
"""+legacy+'End Class\n')
    s=source('CumulativeDistributionPlotExcel')
    raw=member('CumulativeDistributionPlotExcel','ExtendStepToAxisBounds');b=body(raw);prefix=b[:b.index('        Dim prepend As Boolean')]
    # The original no-extension path returns the input arrays to its private caller.
    new="""    Private Shared Sub ExtendStepToAxisBounds(sourceX() As Double, sourceY() As Double, xMinimum As Double, xMaximum As Double, ByRef xOut() As Double, ByRef yOut() As Double)
"""+prefix+"""        If Not (xMinimum < sourceX(0)) AndAlso Not (xMaximum > sourceX(sourceX.Length - 1)) Then
            xOut = sourceX
            yOut = sourceY
            Return
        End If
        Dim projection As New CumulativeStepSeriesData(sourceX, sourceY, xMinimum, xMaximum)
        xOut = projection.XValues
        yOut = projection.YValues
    End Sub"""
    s=replace_member(s,'ExtendStepToAxisBounds',new);write('BESHstatNG/src/Graphics/CumulativeDistributionPlotExcel.vb',s)
    # Explicit measured coordinates use each family's original clipping rules;
    # notably Bullet comparisons and Sankey Math.Min/Max differ for NaN.
    for file,core,defs in [
        ('BulletChartExcel','BulletMeasuredGeometryCore',[
            ('NormalizedToChartX','Public Function MapX(value As Double, plotLeft As Double, plotWidth As Double) As Double',{'layout.PlotLeft':'plotLeft','layout.PlotWidth':'plotWidth'}),
            ('Clip01','Public Function Clip01(value As Double) As Double',{})]),
        ('SankeyPlotExcel','SankeyMeasuredGeometryCore',[
            ('NormalizedXToChartX','Public Function MapX(value As Double, stageLeft As Double, stageRight As Double) As Double',{'layout.StageCentreLeft':'stageLeft','layout.StageCentreRight':'stageRight'}),
            ('NormalizedYToChartY','Public Function MapY(value As Double, plotTop As Double, plotHeight As Double) As Double',{'layout.PlotTop':'plotTop','layout.PlotHeight':'plotHeight'}),
            ('BuildStageCentres','Public Function StageCentres(stageCount As Integer, stageLeft As Double, stageRight As Double) As Double()',{'layout.StageCentreLeft':'stageLeft','layout.StageCentreRight':'stageRight'})])]:
        write('BESHStat.Core/src/Graphics/'+core+'.vb',header()+'Public Module '+core+'\n'+'\n'.join(single_function(file,n,sig,subs) for n,sig,subs in defs)+'End Module\n')
        s=source(file)
        for n,sig,subs in defs:
            original=member(file,n);orig_sig=original[:len(original)-len(body(original))-len('End Function\n')].strip()
            if n=='Clip01':call='BulletMeasuredGeometryCore.Clip01(value)'
            elif n=='NormalizedToChartX':call='BulletMeasuredGeometryCore.MapX(value, layout.PlotLeft, layout.PlotWidth)'
            elif n=='NormalizedXToChartX':call='SankeyMeasuredGeometryCore.MapX(value, layout.StageCentreLeft, layout.StageCentreRight)'
            elif n=='NormalizedYToChartY':call='SankeyMeasuredGeometryCore.MapY(value, layout.PlotTop, layout.PlotHeight)'
            else:call='SankeyMeasuredGeometryCore.StageCentres(stageCount, layout.StageCentreLeft, layout.StageCentreRight)'
            s=replace_member(s,n,'    '+orig_sig+'\n        Return '+call+'\n    End Function')
        write('BESHstatNG/src/Graphics/'+file+'.vb',s)
    save('implemented-paths.json',dict(status='PARTIAL_EXTRACTION_PENDING_REMAINING_OPERATIONS',paths=git('diff','--name-only').splitlines(),limitations=['No claim of family completion until layout,axis,reference and audit work is complete.']))

if __name__=='__main__':extract()
