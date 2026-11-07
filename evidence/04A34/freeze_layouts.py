"""Supplementary pre-edit measured-layout slices. Compiled with accepted host types."""
from entry import ROOT,HERE,BASE,git,save,pin,sha
from freeze_slices import source,member,body
import re
if __name__=='__main__':
    assert git('rev-parse','HEAD')==BASE and not git('diff','--name-only')
    assert not (HERE/'LegacyLayouts34.vb').exists()
    fragments=[];receipts=[]
    for family in ['Bullet','Sankey']:
        file=family+('ChartExcel' if family=='Bullet' else 'PlotExcel')
        s=source(file);pieces=[]
        names=['DrawingLayout','RowLayout'] if family=='Bullet' else ['DrawingLayout','LegendItem']
        for name in names:
            raw=re.search(r'    Private NotInheritable Class '+name+r'\b.*?    End Class',s,re.S).group()
            pieces.append(raw.replace('Private NotInheritable','Public NotInheritable').replace('Friend ','Public '))
        constants=re.findall(r'^    Private Const Minimum\w+ As Double = .+$',s,re.M)
        pieces+=constants
        raw=member(file,'BuildDrawingLayout');b=body(raw)
        b=b[b.index('        Dim padding As Double'):]
        signature=('Public Function BuildDrawingLayout(chartWidth As Double, chartHeight As Double, result As '+family+('ChartResult, appearance As BulletChartAppearance)' if family=='Bullet' else 'PlotResult, appearance As SankeyPlotAppearance, legendItems As List(Of LegendItem))')+' As DrawingLayout')
        pieces.append(signature+'\n'+b+'End Function')
        receipts.append(dict(path='BESHstatNG/src/Graphics/'+file+'.vb',member='BuildDrawingLayout',source=pin('BESHstatNG/src/Graphics/'+file+'.vb'),original_lf=raw,original_lf_sha256=sha(raw.encode()),adaptation='Replace Chart/GetChartSize with explicit chartWidth/chartHeight; source body and accepted appearance/result types unchanged; copy numerical nested layout types.'))
        if family=='Bullet':pieces.append(member(file,'BuildRowLayout').replace('Private Shared','Public'))
        fragments.append('Public Module Legacy'+family+'Layout34\n'+'\n'.join(pieces)+'\nEnd Module')
    text='Option Explicit On\nOption Strict Off\nImports System\nImports System.Collections.Generic\nImports BESHStatNG\nNamespace BESHStatNG.TestFixtures34\n'+'\n'.join(fragments)+'\nEnd Namespace\n'
    (HERE/'LegacyLayouts34.vb').write_text(text,encoding='utf-8',newline='\n')
    save('pre-edit-layout-specification.json',dict(authority='PRE_EDIT_LEGACY',scientific='NOT_GOLDEN',slices=receipts,fixture=pin('evidence/04A34/LegacyLayouts34.vb')))
    print('Frozen explicit-measurement layout slices')
