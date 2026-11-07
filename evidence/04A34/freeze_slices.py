"""Freeze numerical slices from entry source, before any product edit.

Only signatures, explicit measurement arguments and enclosing state references
are adapted. The original fragments and their hashes are retained separately.
"""
from entry import ROOT,HERE,BASE,git,pin,sha,save
import re

def source(name): return (ROOT/('BESHstatNG/src/Graphics/'+name+'.vb')).read_text(encoding='utf-8-sig')
def member(name,method):
    s=source(name)
    match=re.search(r'^\s*(?:Private|Public|Friend) (?:Shared )?(?:Function|Sub) '+method+r'\b',s,re.M)
    assert match,(name,method)
    end=re.search(r'^\s*End (?:Function|Sub)\s*$',s[match.start():],re.M)
    return s[match.start():match.start()+end.end()].strip()+'\n'
def body(text):
    lines=text.splitlines();i=0
    balance=lines[0].count('(')-lines[0].count(')')
    while balance>0:
        i+=1
        balance+=lines[i].count('(')-lines[i].count(')')
    return '\n'.join(lines[i+1:-1])+'\n'

if __name__=='__main__':
    assert git('rev-parse','HEAD')==BASE
    assert not git('diff','--name-only')
    # Re-running before product edits is permitted to correct fixture-only bindings.
    pieces=[];receipts=[]
    def add(file,method,text,original=None):
        raw=original if original is not None else member(file,method)
        pieces.append(text)
        receipts.append(dict(file='BESHstatNG/src/Graphics/'+file+'.vb',member=method,source=pin('BESHstatNG/src/Graphics/'+file+'.vb'),original_lf_sha256=sha(raw.encode()),original_lf=raw,adapted_lf_sha256=sha(text.encode()),authority='PRE_EDIT_LEGACY',adaptation='Test-only signature/enclosing-state/explicit-measurement substitution; original numeric statement order retained. Accepted StatFunc remains a dependency; NOT_GOLDEN.'))
    for file,methods in [('LadderPlotExcel',['SpreadLabelPositions','Clamp','IsFinite','IsFinitePositive']),('CumulativeDistributionPlotExcel',['ExtendStepToAxisBounds']),('GifAnimator',['MsToCentiseconds'])]:
        for name in methods:
            raw=member(file,name)
            text=raw.replace('Private Shared ','Public ').replace('Private Function ','Public Function ')
            add(file,name,text)
    for file,name,signature,subs in [
        ('BulletChartExcel','NormalizedToChartX','Public Function BulletX(value As Double, plotLeft As Double, plotWidth As Double) As Double',{'layout.PlotLeft':'plotLeft','layout.PlotWidth':'plotWidth','Clip01':'BulletClip'}),
        ('BulletChartExcel','Clip01','Public Function BulletClip(value As Double) As Double',{}),
        ('SankeyPlotExcel','NormalizedXToChartX','Public Function SankeyX(value As Double, stageLeft As Double, stageRight As Double) As Double',{'layout.StageCentreLeft':'stageLeft','layout.StageCentreRight':'stageRight'}),
        ('SankeyPlotExcel','NormalizedYToChartY','Public Function SankeyY(value As Double, plotTop As Double, plotHeight As Double) As Double',{'layout.PlotTop':'plotTop','layout.PlotHeight':'plotHeight'}),
        ('SankeyPlotExcel','BuildStageCentres','Public Function StageCentres(stageCount As Integer, stageLeft As Double, stageRight As Double) As Double()',{'layout.StageCentreLeft':'stageLeft','layout.StageCentreRight':'stageRight'})]:
        raw=member(file,name);b=body(raw)
        for a,z in subs.items():b=b.replace(a,z)
        add(file,name,signature+'\n'+b+'End Function\n')
    raw=source('CalibrationPlot').split('            Dim xs As New List(Of Double)()')[1].split('            Dim shp =')[0]
    raw='            Dim xs As New List(Of Double)()'+raw
    text='Public Function Calibration(rows As IList(Of CalibrationBinSummary)) As Double()()\n'+raw.replace('Me.pRows','rows').replace('regression.CalibrationBinSummary','CalibrationBinSummary')+'Return New Double()() {xs.ToArray(), ys.ToArray()}\nEnd Function\n'
    add('CalibrationPlot','addCalibrationPlot.numeric-slice',text,raw)
    raw=source('SymetryPlot').split('            Dim Xref() As Double, Yref() As Double')[1].split('            If lLeft =')[0]
    raw='            Dim Xref() As Double, Yref() As Double'+raw
    text='Public Function Symmetry(data As Double()) As Double()()\nDim n As Integer = data.Length\n'+raw.replace('Me.pData','data').replace('pData','data').replace('Dim ii As Integer = RoundDown(n / 2, 0)','Dim ii As Integer = CInt(RoundDown(n / 2, 0))').replace('Dim udPlotAxis = ChartScaling(0, x.Max())','Dim udPlotAxis = BESHStatNG.ChartAxisScalingCore.Compute(0, x.Max())')+'Return New Double()() {Xs,Ys,Xref,Yref,x,New Double(){udPlotAxis.Min,udPlotAxis.Max,udPlotAxis.Scale}}\nEnd Function\n'
    add('SymetryPlot','AsymmetryPlot.numeric-slice',text,raw)
    output='Option Explicit On\nOption Strict Off\nImports System\nImports System.Linq\nImports System.Collections.Generic\nImports BESHStatNG\nImports BESHStatNG.regression\nImports BESHStatNG.StatFunc\nNamespace BESHStatNG.TestFixtures34\nPublic Module LegacySpecialty34\n'+ '\n'.join(pieces)+'\nEnd Module\nEnd Namespace\n'
    (HERE/'LegacySpecialty34.vb').write_text(output,encoding='utf-8',newline='\n')
    save('pre-edit-fixture-specification.json',dict(schema='04A34-slices-v1',scientific='NOT_GOLDEN',entry=BASE,slices=receipts,fixture=pin('evidence/04A34/LegacySpecialty34.vb'),native='NATIVE_NOT_RUN',r='R_NOT_RUN'))
    print('Frozen',len(receipts),'numeric slices; no new Core dependency')
