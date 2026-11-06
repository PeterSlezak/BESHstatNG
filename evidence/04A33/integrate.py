"""Explicit source-backed extraction recipe; also a reproducible whole-file delta guard."""
from pathlib import Path
import re,subprocess
ROOT=Path(__file__).resolve().parents[2]
BASE='43d20f86b80934c49b2f3dec90824aa62af5aaee'
def original(p):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+p],cwd=ROOT).decode('utf-8')
def block(s,name):
 m=re.search(r'(?m)^        (?:Public |Private |Friend |Shared )*(?:Sub|Function) '+re.escape(name)+r'\b[\s\S]*?^        End (?:Sub|Function)',s);assert m,name;return m.group()
def body(s,name):return block(s,name).split('\n',1)[1].rsplit('\n',1)[0]
def strip(s):return '\n'.join(l for l in s.splitlines() if not l.lstrip().startswith("'"))
def write(p,s):(ROOT/p).write_text(s,encoding='utf-8',newline='\n')
def transforms():
 p='BESHstatNG/src/Graphics/XYZscatter.vb'; old=original(p);s=old
 ops={
 'normalizedData':('ProjectNormalized','Me.ProjectionContext(), x_norm, y_norm, z_norm, n'),
 'cut_planes':('CutPlanes','Me.ProjectionContext(), x_norm, y_norm, z_norm, n, cp_normalized_pos'),
 'cage_data':('Cage','Me.ProjectionContext()'),
 'tick_marks_data':('Ticks','Me.ProjectionContext(), ftn, tsn, tx'),
 'get_gridlines':('GridLines','Me.ProjectionContext(), ftn, tsn, tx')}
 outputs={}
 for name,(method,args) in ops.items():
  b=strip(body(old,name))
  names=list(dict.fromkeys(re.findall(r'\b((?:xs_|ys_|error_bars)[A-Za-z_]*)(?=\()',b)))
  outputs[name]=names
  pre='            Call getnorm()\n' if name in ['normalizedData','cut_planes'] else ''
  if name=='tick_marks_data':pre='            Call tick_marks_prerequisites()\n'
  # Preserve pre-existing allocation/partial-state sequence before portable arithmetic.
  allocations='\n'.join(l for l in b.splitlines() if l.strip().startswith('ReDim '))
  new=block(old,name).split('\n')[0]+'\n'+pre+allocations+'\n            Dim coordinates = XyzGeometryCore.'+method+'('+args+')\n'
  new+='\n'.join('            '+v+' = coordinates.Vector("'+v+'")' for v in names)+'\n'
  if name=='tick_marks_data':
   for axis,label in enumerate(['x','y','z']):new+=f'            For t As Integer = 0 To CInt(tx({axis}))\n                {label}_tick_labels(t) = CStr(ft({axis}) + t * ts({axis}))\n            Next\n'
  new+='        End Sub'
  s=s.replace(block(old,name),new)
 # Per-axis commits preserve original partial tick state if a later axis fails.
 new='''        Private Sub tick_marks_prerequisites()
            For i As Integer = 0 To 2
                Dim tick = XyzGeometryCore.TickAxis(raw_mins_(i), raw_ranges_(i), axis_dir_(i))
                ts(i) = tick(0) : ft(i) = tick(1) : tx(i) = tick(2)
                ftn(i) = tick(3) : tsn(i) = tick(4)
            Next
        End Sub'''
 s=s.replace(block(old,'tick_marks_prerequisites'),new)
 s=s.replace(block(old,'get_rotations'),'''        Private Sub get_rotations()
            Dim rows = XyzProjectionContext.RotationRows(x_rotate, y_rotate, z_rotate, x_scale_ratio, y_scale_ratio, z_scale_ratio)
            rot_1 = rows(0) : rot_2 = rows(1)
        End Sub''')
 for name,method,param in [('comp_zoom_inter','ZoomFactor','z'),('comp_x_shift_inter','ShiftX','x'),('comp_y_shift_inter','ShiftY','y')]:
  s=s.replace(block(old,name),block(old,name).split('\n')[0]+'\n            Return XyzProjectionContext.'+method+'('+param+')\n        End Function')
 s=s.replace(block(old,'ProjectRawPoint'),'''        Friend Sub ProjectRawPoint(xRaw As Double, yRaw As Double, zRaw As Double,
                                   ByRef x2d As Double, ByRef y2d As Double)
            Me.ProjectionContext().ProjectRawPoint(xRaw, yRaw, zRaw, x2d, y2d)
        End Sub

        Friend Function ProjectionContext() As XyzProjectionContext
            Return New XyzProjectionContext(raw_mins_, raw_ranges_, axis_dir_, rot_1, rot_2, x_shift_internal, y_shift_internal, zoom_internal)
        End Function''')
 result={p:s}
 # Extract numeric expressions verbatim into an ephemeral private builder.
 fields=['rot_1','rot_2','ftn','tsn','tx','x_norm','y_norm','z_norm','cp_normalized_pos']
 alloutputs=list(dict.fromkeys(sum(outputs.values(),[])))
 core='''Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

'''+'\nPublic Module XyzGeometryCore\n'
 core+='''    ' Matches legacy tick arithmetic, including Math.Round digit exceptions.
    Public Function TickAxis(minimum As Double, range As Double, direction As Double) As Double()
        Dim fract As Double = range / 5.0
        Dim nDig As Integer = CInt(1 - Math.Floor(Math.Log10(Math.Abs(fract)) + 1))
        Dim ts As Double = Math.Round(fract, nDig)
        Dim ft As Double = ts * (Math.Floor(minimum / ts) + 1)
        Dim tx As Double = Math.Floor((range + minimum - ft) / ts)
        Return {ts, ft, tx, ((ft - minimum) / range - 0.5) * direction, (ts / range) * direction}
    End Function
'''
 for name,(method,_) in ops.items():
  sig='context As XyzProjectionContext'
  if name in ['normalizedData','cut_planes']:sig+=', x() As Double, y() As Double, z() As Double, count As Integer'
  if name=='cut_planes':sig+=', planes() As Double'
  if name in ['tick_marks_data','get_gridlines']:sig+=', first() As Double, stepSize() As Double, counts() As Double'
  core+=f'    Public Function {method}({sig}) As XyzGeometryData\n        Dim work As New XyzGeometryBuilder(context)\n'
  if name in ['normalizedData','cut_planes']:core+='        work.x_norm = DirectCast(x.Clone(), Double()) : work.y_norm = DirectCast(y.Clone(), Double()) : work.z_norm = DirectCast(z.Clone(), Double()) : work.n = count\n'
  if name=='cut_planes':core+='        work.cp_normalized_pos = DirectCast(planes.Clone(), Double())\n'
  if name in ['tick_marks_data','get_gridlines']:core+='        work.ftn = DirectCast(first.Clone(), Double()) : work.tsn = DirectCast(stepSize.Clone(), Double()) : work.tx = DirectCast(counts.Clone(), Double())\n'
  core+=f'        Return work.{method}()\n    End Function\n'
 core+='End Module\n\nFriend NotInheritable Class XyzGeometryBuilder\n'
 core+='    Private Const gToDeleteGridLineValue As Double = -100000000000000.0\n    Friend n As Integer\n    Private x_shift_internal As Double, y_shift_internal As Double, zoom_internal As Double\n'
 core+='\n'.join('    Friend '+f+'() As Double' for f in fields+alloutputs)+'\n'
 core+='''    Friend Sub New(context As XyzProjectionContext)
        rot_1 = context.RotationX : rot_2 = context.RotationY
        x_shift_internal = context.XShift : y_shift_internal = context.YShift : zoom_internal = context.Zoom
        xs_cage = New Double(10) {} : ys_cage = New Double(10) {}
    End Sub
'''
 for name,(method,_) in ops.items():
  b=strip(body(old,name))
  b=re.sub(r'^\s*Call (?:getnorm|tick_marks_prerequisites)\(\)\s*$', '',b,flags=re.M)
  b='\n'.join(l for l in b.splitlines() if '_tick_labels' not in l)
  # Original Strict-Off array bounds/For limits converted with the SAME VB CInt conversion.
  b=re.sub(r'3 \* \(tx\((\d)\) \+ 1\) - 1',r'CInt(3 * (tx(\1) + 1) - 1)',b)
  b=re.sub(r'For i = 0 To tx\((\d)\)',r'For i As Integer = 0 To CInt(tx(\1))',b)
  entries=', '.join('{"'+v+'", '+v+'}' for v in outputs[name])
  core+=f'    Friend Function {method}() As XyzGeometryData\n'+b+'\n        Return New XyzGeometryData(New Dictionary(Of String, Double()) From {'+entries+'})\n    End Function\n'
 core+='End Class\n'
 result['BESHStat.Core/src/Graphics/XyzGeometryCore.vb']=core
 result['BESHStat.Core/src/Graphics/XyzProjectionContext.vb']=(ROOT/'evidence/04A33/projection.vb.txt').read_text(encoding='utf-8')
 p='BESHstatNG/src/Graphics/3Dobjects.vb';old=original(p);s=old
 wire='Option Explicit On\nOption Strict On\nImports System\nPublic Module WireObjectGeometryCore\n'
 for class_name,method,dims in [('WireSphere3D','Sphere',['Diameter']),('WireEllipsoid3D','Ellipsoid',['DiameterX','DiameterY','DiameterZ'])]:
  start=old.index('    Public Class '+class_name);end=old.index('    End Class',start)
  section=old[start:end]
  draw=block(section,'Draw');numeric=draw[draw.index('            If Diameter'):draw.index('            Dim s As Series')]
  numeric=strip(numeric).replace('Then Exit Sub','Then Return New WireSeriesData(Array.Empty(Of Double)(), Array.Empty(Of Double)(), 1)')
  numeric=numeric.replace('owner.ProjectRawPoint','context.ProjectRawPoint').replace('owner.BreakValue','breakValue')
  numeric='        Dim Xs() As Double = Nothing, ys() As Double = Nothing\n'+numeric
  params=', '.join(x+' As Double' for x in ['Cx','Cy','Cz']+dims)
  wire+=f'    Public Function {method}(context As XyzProjectionContext, {params}, LatitudeRings As Integer, LongitudeRings As Integer, PointsPerRing As Integer, Optional breakValue As Double = -100000000000000.0) As WireSeriesData\n'+numeric+'\n        Return New WireSeriesData(Xs, ys, stride)\n    End Function\n'
  # Original invalid-dimension check and null-chart guard remain at the same point.
  keep=draw[:draw.index('            Dim '+('r As Double' if method=='Sphere' else 'a As Double'))]
  new=keep+'            Dim geometry = WireObjectGeometryCore.'+method+'(owner.ProjectionContext(), '+', '.join(['Cx','Cy','Cz']+dims+['LatitudeRings','LongitudeRings','PointsPerRing','owner.BreakValue'])+')\n            Xs = geometry.X : ys = geometry.Y\n'+draw[draw.index('            Dim s As Series'):]
  s=s.replace(draw,new)
  bound=block(section,'GetRawBounds');b=body(section,'GetRawBounds')
  # Multi-line signature continues through Implements; retain only the numeric body.
  b=b[b.index('            Dim '):]
  wire+=f'    Public Function {method}Bounds({params}) As Double()\n        Dim minX, maxX, minY, maxY, minZ, maxZ As Double\n'+b+'\n        Return {minX, maxX, minY, maxY, minZ, maxZ}\n    End Function\n'
  prefix=bound[:bound.index('            Dim ')]
  new=prefix+'            Dim bounds = WireObjectGeometryCore.'+method+'Bounds('+', '.join(['Cx','Cy','Cz']+dims)+')\n            minX = bounds(0) : maxX = bounds(1) : minY = bounds(2) : maxY = bounds(3) : minZ = bounds(4) : maxZ = bounds(5)\n        End Sub'
  s=s.replace(bound,new)
 result[p]=s;result['BESHStat.Core/src/Graphics/WireObjectGeometryCore.vb']=wire+'End Module\n'
 p='BESHstatNG/src/Graphics/ScatterPlotMatrix.vb';old=original(p);s=old
 b=body(old,'transformData')
 # Reuse frozen normalized data and the original statistical helpers, in original order.
 b=b[b.index('            Dim ii As Integer'):]
 fits=body(old,'transformData');fits=fits[fits.index('            If pbDisplayRegLines Then'):fits.index('            Dim ii As Integer')]
 core='''Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Public Module ScatterMatrixSeriesCore
    Public Function Correlations(data(,) As Double) As Double(,)
        Return Matrix.CorrelationMatrixCore.Compute(DirectCast(data.Clone(), Double(,)), "r")
    End Function
    Public Function GridPosition(index As Integer, variables As Integer) As Double
        Return index * 1 / variables
    End Function
    Public Function PanelValues(data(,) As Double, variableOrdinal As Integer) As Double()
        Return DataManagement.ArrayUtilities.GetColumn(DirectCast(data.Clone(), Double(,)), variableOrdinal)
    End Function
    Public Function PanelCoordinates(input(,) As Double, margin As Double, regression As Boolean) As ScatterMatrixPanelData
        Dim ScaledData = DirectCast(input.Clone(), Double(,)), p = input.GetLength(1)
        Dim pMargin As Double = margin, pbDisplayRegLines As Boolean = regression
        Dim tmp(), tmp2() As Double
        Dim intercepts_scaled(p - 1, p - 1), slopes_scaled(p - 1, p - 1) As Double
        Dim x_corr_labels(p * p - 1), y_corr_labels(p * p - 1) As Double
        Dim x_regline(p * (p - 1) * 3 - 1), y_regline(p * (p - 1) * 3 - 1) As Double
        Const gToDeleteGridLineValue As Double = -100
'''
 core+=strip(fits+b).replace('Matrix.GetColumnFrom2Darray','DataManagement.ArrayUtilities.GetColumn')
 core+='\n        Return New ScatterMatrixPanelData(ScaledData, x_corr_labels, y_corr_labels, x_regline, y_regline)\n    End Function\nEnd Module\n'
 result['BESHStat.Core/src/Graphics/ScatterMatrixSeriesCore.vb']=core
 new=block(old,'transformData').split('\n')[0]+'\n'+body(old,'transformData').split('            If pbDisplayCorrCoef Then')[0]
 new+='''            If pbDisplayCorrCoef Then CorrMatrix = ScatterMatrixSeriesCore.Correlations(pData)
            Dim coordinates = ScatterMatrixSeriesCore.PanelCoordinates(ScaledData, pMargin, pbDisplayRegLines)
            x_corr_labels = coordinates.CorrelationX : y_corr_labels = coordinates.CorrelationY
            x_regline = coordinates.RegressionX : y_regline = coordinates.RegressionY
        End Sub'''
 s=s.replace(block(old,'transformData'),new)
 s=s.replace('{i * 1 / p, i * 1 / p}','{ScatterMatrixSeriesCore.GridPosition(i, p), ScatterMatrixSeriesCore.GridPosition(i, p)}')
 s=s.replace('Matrix.GetColumnFrom2Darray(ScaledData, i)','ScatterMatrixSeriesCore.PanelValues(ScaledData, i)').replace('Matrix.GetColumnFrom2Darray(ScaledData, j)','ScatterMatrixSeriesCore.PanelValues(ScaledData, j)')
 result[p]=s
 result['BESHStat.Core/src/Graphics/ScatterMatrixPanelData.vb']=(ROOT/'evidence/04A33/scatter.vb.txt').read_text(encoding='utf-8')
 return result
if __name__=='__main__':
 for p,s in transforms().items():write(p,s)
