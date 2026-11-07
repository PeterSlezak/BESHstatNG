"""Avoid VB module-wide generic name injection; preserve frozen fixture names."""
from work import *
for family,names in [('Bullet',['Band','Actual','Target','ActualLabel','TargetLabel']),('Sankey',['Ribbon','Node','NodeLabel'])]:
    p='BESHStat.Core/src/Graphics/'+family+'MeasuredGeometryCore.vb';s=(ROOT/p).read_text(encoding='utf-8')
    w=family+('ChartExcel' if family=='Bullet' else 'PlotExcel');host=src(w)
    for n in names:
        s=s.replace('Public Function '+n+'(','Public Function '+family+n+'(')
        host=host.replace(family+'MeasuredGeometryCore.'+n+'(',family+'MeasuredGeometryCore.'+family+n+'(')
    write(p,s);write('BESHstatNG/src/Graphics/'+w+'.vb',host)
