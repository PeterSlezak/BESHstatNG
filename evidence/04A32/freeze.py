"""Freeze numerical source slices BEFORE product changes. No oracle/new engine.
LegacyBox30 whisker/segment fixture is reused;only missing Calculate is sliced.
"""
from audit import ROOT,HERE,BASE,save,pin,sha
import re,hashlib
def member(path,name,occurrence=0):
 text=(ROOT/path).read_text(encoding='utf-8-sig')
 pattern=r'(?im)^ *\b(?:(?:Public|Private|Friend|Protected|Shared|Overrides) +)*(?:Sub|Function) '+re.escape(name)+r'\('
 matches=list(re.finditer(pattern,text));m=matches[occurrence]
 end=re.search(r'(?im)^ *End (?:Sub|Function)\s*$',text[m.end():]);body=text[m.start():m.end()+end.end()]
 return body
def strip(text):return '\n'.join(l for l in text.splitlines() if not l.lstrip().startswith("'"))
box='BESHstatNG/src/Graphics/BoxPlot.vb';hist='BESHstatNG/src/Graphics/Histogram.vb';violin='BESHstatNG/src/Graphics/ViolinPlot.vb'
assert not (HERE/'LegacyGraphics32.vb.txt').exists()
calculate=member(box,'Calculate')
histcompute=member(hist,'compute').replace('Public Function compute','Public Function Compute')
histcompute=histcompute.replace('Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(bins, 0))','DataManagement.ArrayUtilities.GetColumn(bins, 0).Select(Function(x) Convert.ToDouble(x)).ToArray()').replace('Matrix.Array2intArray(Matrix.GetColumnFrom2Darray(bins, 1))','DataManagement.ArrayUtilities.GetColumn(bins, 1).Select(Function(x) Convert.ToInt32(x)).ToArray()').replace('Matrix.GetColumnFrom2Darray','DataManagement.ArrayUtilities.GetColumn')
axes=member(hist,'addChart');axes=axes[axes.index('            Dim bOverlay'):axes.index('            Dim anchor As Range')]
axes=axes.replace('If iNoBins = 0 Then Exit Sub','If iNoBins = 0 Then Return Nothing').replace('CHARTscale','ChartAxisScaleResult').replace('ChartScaling(','ChartAxisScalingCore.Compute(')
resolve=member(violin,'ResolveAxisContext').replace('Private Shared','Public Shared').replace('ViolinPlotAppearance','LegacyViolinAppearance32').replace('AxisContext','LegacyAxis32')
automatic=member(violin,'ResolveAutomaticYLimits').replace('Private Shared','Public Shared').replace('graphics.CHARTscale','ChartAxisScaleResult').replace('graphics.ChartingFunc.ChartScaling','ChartAxisScalingCore.Compute')
jitter=member(box,'DeterministicJitter').replace('Private Shared','Public Shared')
text='''Option Explicit On
Option Strict Off
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG
Namespace Global.Batch32Fixtures
Public Class LegacyBox32
    Inherits Batch30Fixtures.LegacyBox30
    Friend pArOutliersSmall(,),pArOutliersBig(,) As Double
    Public Sub New(groups As Double()())
        pData=groups : pNoGroups=groups.Length
        pNs=groups.Select(Function(g) g.Length).ToArray()
    End Sub
'''+strip(calculate)+'''
End Class
Public Class LegacyHistogram32
    Friend data(), arBinMidVal(), arXi(), arGauss(), arOutliers() As Double
    Friend arFreq() As Integer
    Friend pQ1,pMedian,pQ3,pWhiskerLow,pWhiskerHigh As Double
    Friend pHasBoxSummary As Boolean
    Public Sub New(values As Double())
        data=values
    End Sub
'''+strip(histcompute)+'''
    Public Function Axes(bBoxPlot As Boolean) As Double()
'''+strip(axes)+'''
        Return {dXMin,dXMax,dYMin,dYMax,dYMajor,dBoxY,dBoxHalfHeight}
    End Function
End Class
Public Class LegacyAxis32
    Public XMinimum,XMaximum,YMinimum,YMaximum As Double
End Class
Public Class LegacyViolinAppearance32
    Public YAxisMinimum,YAxisMaximum As Double?
End Class
Public Class LegacyDomain32
    Private Shared Function IsFinite(x As Double) As Boolean
        Return Not Double.IsNaN(x) AndAlso Not Double.IsInfinity(x)
    End Function
'''+strip(resolve)+'\n'+strip(automatic)+'\n'+strip(jitter)+'''
End Class
End Namespace
'''
(HERE/'LegacyGraphics32.vb.txt').write_text(text,encoding='utf-8')
sources=[]
for path,name in [(box,'Calculate'),(hist,'compute'),(hist,'addChart'),(violin,'ResolveAxisContext'),(violin,'ResolveAutomaticYLimits'),(box,'DeterministicJitter')]:
 b=member(path,name);source=(ROOT/path).read_text(encoding='utf-8-sig');start=source.index(b);sources.append(dict(source=pin(ROOT/path),member=name,start_line=source[:start].count('\n')+1,end_line=source[:start+len(b)].count('\n')+1,body_sha256=hashlib.sha256(b.encode()).hexdigest()))
save('pre-edit-compatibility-authority.json',dict(schema='04A32-frozen-source-v1',base=BASE,slices=sources,fixture=pin(HERE/'LegacyGraphics32.vb.txt'),reused='LegacyBoxBounds30.vb supplies unchanged source-derived CalcForPlotting;not copied again',transformations=['Comments removed;host fields replaced with typed numeric fields','Histogram array-conversion facades replaced by their exact typed Convert targets','Histogram chart numeric prefix returned as seven scalars;no COM copied','Violin axis appearance/context replaced by equivalent numeric fields;shared scale forwards same Core engine'],quartile_identity='All three live paths ultimately use StatFunc.QuartilesComp;separate route policy and whisker implementation retained. No fictitious disagreement expected values.',scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN'))
print('Frozen source fixture and precise current source member ranges')
