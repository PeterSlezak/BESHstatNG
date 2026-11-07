"""Move the frozen explicit-measurement layout bodies into family-specific Core contracts."""
from entry import ROOT,HERE,save
from integrate import header,write,replace_member
import json,re

def immutable(name,fields):
    declarations='\n'.join('    Public ReadOnly Property '+n+' As '+t for n,t in fields)
    args=', '.join('value'+n+' As '+t for n,t in fields)
    assignments='\n'.join('        Me.'+n+' = value'+n for n,t in fields)
    return 'Public NotInheritable Class '+name+'\n'+declarations+'\n    Public Sub New('+args+')\n'+assignments+'\n    End Sub\nEnd Class\n'
def construct(b,old,new,fields):
    pattern=r'(?:Return New |Dim layout As New )'+old+r' With \{(.*?)\n        \}'
    m=re.search(pattern,b,re.S);assert m,old
    values={n:v.strip() for n,v in re.findall(r'\.(\w+) = (.*?)(?=,\n|$)',m.group(1),re.S)}
    assert set(values)=={n for n,t in fields},(values,fields)
    expression='New '+new+'('+', '.join(values[n] for n,t in fields)+')'
    prefix='Return ' if m.group().startswith('Return') else 'Dim layout As '+new+' = '
    return b[:m.start()]+prefix+expression+b[m.end():]
if __name__=='__main__':
    assert (HERE/'pre-edit-layout-capture.csv').exists()
    specs=json.loads((HERE/'pre-edit-layout-specification.json').read_bytes())['slices']
    original=(HERE/'LegacyLayouts34.vb').read_text()
    for family,record in zip(['Bullet','Sankey'],specs):
        file=record['path'];s=(ROOT/file).read_text();core=family+'MeasuredGeometryCore'
        module=original.split('Public Module Legacy'+family+'Layout34')[1].split('End Module')[0]
        fields=re.findall(r'Public (\w+) As (Double|Boolean|Integer)',re.search(r'Public NotInheritable Class DrawingLayout(.*?)End Class',module,re.S).group(1))
        layoutName=family+'DrawingLayout'
        raw=record['original_lf'];b=raw[raw.index('        Dim padding As Double'):raw.rindex('    End Function')]
        props=list(dict.fromkeys(re.findall(r'appearance\.(\w+)',b)))
        typed=[]
        for prop in props:
            kind='Boolean' if prop.startswith('Show') else 'String' if prop in ('ChartTitle','LegendTitle') else 'Double'
            typed.append((prop,kind))
            b=b.replace('appearance.'+prop,'input'+prop)
        b=b.replace('result.HasSubtitles','hasSubtitles').replace('result.ItemCount','itemCount').replace('result.StageCount','stageCount').replace('legendItems.Count','legendItemCount')
        b=construct(b,'DrawingLayout',layoutName,fields)
        fixed=[('chartWidth','Double'),('chartHeight','Double')]+([('itemCount','Integer'),('hasSubtitles','Boolean')] if family=='Bullet' else [('stageCount','Integer'),('legendItemCount','Integer')])
        args=fixed+[('input'+n,t) for n,t in typed]
        function='    Public Function ComputeLayout('+', '.join(n+' As '+t for n,t in args)+') As '+layoutName+'\n'+b+'    End Function\n'
        text=(ROOT/('BESHStat.Core/src/Graphics/'+core+'.vb')).read_text()
        constants='\n'.join(re.findall(r'^    Private Const Minimum\w+ As Double = .+$',module,re.M))
        text=text.replace('Public Module '+core,'Public Module '+core+'\n'+constants+'\n'+function)
        text+='\n'+immutable(layoutName,fields)
        # Native drawing retains exactly the original measurements and private types.
        calls=['chartWidth','chartHeight']+(['result.ItemCount','result.HasSubtitles'] if family=='Bullet' else ['result.StageCount','legendItems.Count'])+['appearance.'+n for n,t in typed]
        new='    '+raw[:raw.index('        Dim chartWidth As Double')].strip()+'\n        Dim chartWidth As Double\n        Dim chartHeight As Double\n        GetChartSize(chart, chartWidth, chartHeight)\n        Dim geometry As '+layoutName+' = '+core+'.ComputeLayout('+', '.join(calls)+')\n        Return New DrawingLayout With {\n'+',\n'.join('            .'+n+' = geometry.'+n for n,t in fields)+'\n        }\n    End Function'
        s=replace_member(s,'BuildDrawingLayout',new)
        if family=='Bullet':
            rawrow=re.search(r'Public Function BuildRowLayout\b.*?End Function',module,re.S).group()
            rb=rawrow[rawrow.index('        Dim rowTop'):rawrow.rindex('    End Function')]
            rf=re.findall(r'Public (\w+) As (Double|Boolean|Integer)',re.search(r'Public NotInheritable Class RowLayout(.*?)End Class',module,re.S).group(1))
            rowprops=list(dict.fromkeys(re.findall(r'appearance\.(\w+)',rb)))
            for prop in rowprops:rb=rb.replace('appearance.'+prop,'input'+prop)
            rb=construct(rb,'RowLayout','BulletRowLayout',rf)
            rowargs=[('rowIndex','Integer')]+[('input'+n,dict(typed).get(n,'Double')) for n in rowprops]
            rowfunction='    Public Function ComputeRow(layout As BulletDrawingLayout, '+', '.join(n+' As '+t for n,t in rowargs)+') As BulletRowLayout\n'+rb+'    End Function\n'
            text=text.replace('Public Module '+core,'Public Module '+core+'\n'+rowfunction)+immutable('BulletRowLayout',rf)
            layoutArgs=', '.join('layout.'+n for n,t in fields)
            rowCalls=', '.join(['rowIndex']+['appearance.'+n for n in rowprops])
            newrow='    Private Shared Function BuildRowLayout(layout As DrawingLayout, appearance As BulletChartAppearance, rowIndex As Integer) As RowLayout\n        Dim measured As New BulletDrawingLayout('+layoutArgs+')\n        Dim geometry As BulletRowLayout = BulletMeasuredGeometryCore.ComputeRow(measured, '+rowCalls+')\n        Return New RowLayout With {\n'+',\n'.join('            .'+n+' = geometry.'+n for n,t in rf)+'\n        }\n    End Function'
            s=replace_member(s,'BuildRowLayout',newrow)
        write('BESHStat.Core/src/Graphics/'+core+'.vb',text);write(file,s)
    print('Delegated measured Bullet/Sankey layouts and Bullet row geometry')
