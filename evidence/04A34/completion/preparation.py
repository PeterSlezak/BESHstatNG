"""Freeze unchanged pure member bodies, then wire typed family-specific preparation.

Settings contain only the scalar/enum/string properties actually read by these
methods. Numeric/date axis hints are presentation intent; OADate conversion stays
in the Windows Dumbbell formatter.
"""
from work import *
CONFIG={
 'Bullet':dict(file='BulletChartExcel',appearance='BulletChartAppearance',methods=['ResolveNumberFormat','BuildAutomaticNumberFormat','RequiredDecimalPlaces','FormatNumeric'],helpers=['IsFinite']),
 'Sankey':dict(file='SankeyPlotExcel',appearance='SankeyPlotAppearance',methods=['FormatNodeLabel'],helpers=[]),
 'Cdf':dict(file='CumulativeDistributionPlotExcel',appearance='CumulativeDistributionPlotAppearance',methods=['ResolveAxisContext','FormatPercentileLabel','ResolveChartTitle','ResolveEmpiricalSeriesName','ResolveFittedSeriesName','ResolveGroupLegendName','DistributionShortName','AutomaticNumberFormat'],helpers=['IsFinite','IsFinitePositive']),
 'Dumbbell':dict(file='DumbbellPlotExcel',appearance='DumbbellPlotAppearance',methods=['ResolveAxisContext','NiceStep125','FloorToStep','CeilingToStep','ResolveXAxisNumberFormat','ResolveSeriesName','SuggestedHeight'],helpers=['IsFinite','IsFinitePositive']),
 'Ladder':dict(file='LadderPlotExcel',appearance='LadderPlotAppearance',methods=['ResolveAxisContext','ResolveYAxisNumberFormat','NiceStep125','FloorToStep','CeilingToStep','BuildEndpointLabel','FormatEndpointValue','ResolveObservationSeriesName','SuggestedWidth','SuggestedHeight'],helpers=['IsFinite','IsFinitePositive']),
 'Hull':dict(file='ConvexHullPlot',appearance='ConvexHullPlotAppearance',methods=['ResolveSeriesName'],helpers=[]),
 'Polar':dict(file='PolarPlot',appearance='PolarPlotAppearance',methods=['ResolveSeriesName'],helpers=[]),
 'Spc':dict(file='SpcControlChartExcel',appearance='SpcControlChartAppearanceOptions',methods=['NormalizeText','NormalizeNumberFormat','ResolveChartTitle','ResolveValueAxisTitle','PanelSupportsSpecificationLimits'],helpers=[])}
def fields_settings(text,appearance,methods):
    props=[]
    for n in methods:
        h,b=header_body(method(text,n))
        aliases=re.findall(r'(\w+) As '+appearance+r'\b',h+b)
        for alias in aliases:
            props+=re.findall(r'\b'+alias+r'\.(\w+)',b)
    fields=[]
    for name in dict.fromkeys(props):
        m=re.search(r'Public Property '+name+r' As ([^\n=]+?)(?:\s*=\s*([^\n]+))?\s*$',text,re.M)
        assert m,(appearance,name)
        kind=m[1].strip();default=(m[2] or ('Nothing' if kind in ('String',) or 'Nullable' in kind else 'False' if kind=='Boolean' else '0')).strip()
        fields.append((name,kind,default.replace('String.Empty','""')))
    return fields
def settings(name,fields):
    return 'Public NotInheritable Class '+name+'\n'+''.join('Public ReadOnly Property '+n+' As '+t+'\n' for n,t,d in fields)+'Public Sub New('+', '.join('Optional input'+n+' As '+t+' = '+d for n,t,d in fields)+')\n'+''.join('Me.'+n+' = input'+n+'\n' for n,t,d in fields)+'End Sub\nEnd Class\n'
def axis_type(text,immutable):
    m=re.search(r'Private NotInheritable Class AxisContext\b.*?End Class',text,re.S)
    if not m:return '',[]
    fields=re.findall(r'Friend (\w+) As (Double|String)',m.group())
    if not immutable:return m.group().replace('Private NotInheritable','Public NotInheritable').replace('Friend ','Public '),fields
    s='Public NotInheritable Class AxisContext\nFriend Sub New()\nEnd Sub\n'
    for n,t in fields:
        s+='Private _'+n+' As '+t+'\nPublic Property '+n+' As '+t+'\nGet\nReturn _'+n+'\nEnd Get\nFriend Set(value As '+t+')\n_'+n+' = value\nEnd Set\nEnd Property\n'
    return s+'End Class\n',fields
def freeze():
    assert not (HERE/'pre-edit-preparation.csv').exists()
    recipes=[];legacy=HEADER+'Imports BESHStatNG.StatisticalProcessControl\nImports BESHStatNG.graphics\nNamespace Global.Batch34PreparationFixtures\n'
    for family,cfg in CONFIG.items():
        text=(HERE/'entry-source'/(cfg['file']+'.vb.txt')).read_text(encoding='utf-8');current=src(cfg['file'])
        for n in cfg['methods']+cfg['helpers']:assert method(text,n)==method(current,n),(cfg['file'],n)
        fs=fields_settings(text,cfg['appearance'],cfg['methods'])
        context,af=axis_type(text,False)
        constants='\n'.join(re.findall(r'^[ \t]*Private Const PlotX\w+ As Double = .+$',text,re.M))
        definitions=[]
        for n in cfg['methods']+cfg['helpers']:
            raw=method(text,n);definitions.append(raw.replace('Private Shared ','Public ').replace('Friend Shared ','Public ').replace('Public Shared ','Public ').replace('graphics.CHARTscale','ChartAxisScaleResult').replace('graphics.ChartingFunc.ChartScaling','ChartAxisScalingCore.Compute'))
        legacy+='Public Module Legacy'+family+'Preparation34\n'+constants+'\n'+context+'\n'+'\n'.join(definitions)+'\nEnd Module\n'
        enums={}
        for _,kind,_ in fs:
            e=re.search(r'Public Enum '+re.escape(kind)+r'\b.*?End Enum',text,re.S)
            if e:enums[kind]=e.group()
        recipes.append(dict(family=family,config=cfg,settings_fields=fs,enums=enums,axis_fields=af,constants=constants,members=[dict(name=n,original=method(text,n),original_sha256=sha(method(text,n).encode())) for n in cfg['methods']+cfg['helpers']]))
    legacy+='End Namespace\n'
    write('BESHStatNG.Test/src/Charts/LegacyCompletionPreparation34.vb',legacy)
    save('preparation-spec.json',dict(authority='PRE_EDIT_LEGACY',scientific='NOT_GOLDEN',families=recipes,qualification='Exact original member bodies, original Windows appearance/result types and namespace binding; no new portable preparation used as oracle. Accepted Core engines remain dependencies.'))
    print('Frozen pure axis/text members for',len(recipes),'families')
def apply():
    assert (HERE/'pre-edit-preparation.csv').exists()
    specs=json.loads((HERE/'preparation-spec.json').read_bytes())['families']
    enum_written=set()
    for r in specs:
        f=r['family'];cfg=r['config'];service=f+'ChartPreparation34';settingsname=f+'ChartSettings34'
        source=src(cfg['file']);entry=(HERE/'entry-source'/(cfg['file']+'.vb.txt')).read_text(encoding='utf-8')
        context,af=axis_type(entry,True)
        common=HEADER+'Imports BESHStatNG.StatisticalProcessControl\nNamespace ExcelCommon.Chart34\n'
        for key,enum in r['enums'].items():
            if key not in enum_written:common+=enum+'\n';enum_written.add(key)
        if r['settings_fields']:common+=settings(settingsname,r['settings_fields'])
        common+='Public Module '+service+'\n'+r['constants']+'\n'+context+'\n'
        for item in r['members']:
            raw=item['original'];header,b=header_body(raw)
            if item['name'] in cfg['methods']:
                new=raw.replace('Private Shared ','Public ').replace('Friend Shared ','Public ').replace('Public Shared ','Public ')
            else:new=raw.replace('Private Shared ','Private ')
            new=new.replace(cfg['appearance'],settingsname).replace('graphics.CHARTscale','ChartAxisScaleResult').replace('graphics.ChartingFunc.ChartScaling','ChartAxisScalingCore.Compute')
            common+=new+'\n'
            if item['name'] not in cfg['methods']:continue
            # Existing private/public Windows signatures are preserved exactly.
            parameters=re.findall(r'(\w+)(?:\(\))? As (?:[^,\n]+)',header)
            parameters=[n for n in parameters if n not in ('Function','Sub')]
            # Capture only identifiers in the parameter section (not the return type).
            param_text=header[header.index('(')+1:header.rfind(')')]
            parameters=re.findall(r'(?:Optional )?(\w+)(?:\(\))? As ',param_text)
            args=[]
            for name in parameters:
                if re.search(r'\b'+name+' As '+cfg['appearance']+r'\b',header):args.append('Snapshot'+f+'Settings34('+name+')')
                else:args.append(name)
            call='ExcelCommon.Chart34.'+service+'.'+item['name']+'('+', '.join(args)+')'
            if ' As AxisContext' in header:
                wrapper=header+'\n        Dim geometry = '+call+'\n        Return New AxisContext With {\n'+',\n'.join('            .'+n+' = geometry.'+n for n,t in af)+'\n        }\n    End Function'
            else:wrapper=header+'\n        Return '+call+'\n    End Function'
            source=replace(source,item['name'],wrapper)
        common+='End Module\nEnd Namespace\n'
        write('BESHStat.Excel.Common/src/Analysis/'+service+'.vb',common)
        if r['settings_fields']:
            args=[]
            for n,t,d in r['settings_fields']:
                value='appearance.'+n
                if t in r['enums']:value='CType(CInt('+value+'), ExcelCommon.Chart34.'+t+')'
                args.append(value)
            snap='\n    Private Shared Function Snapshot'+f+'Settings34(appearance As '+cfg['appearance']+') As ExcelCommon.Chart34.'+settingsname+'\n        If appearance Is Nothing Then Return Nothing\n        Return New ExcelCommon.Chart34.'+settingsname+'('+', '.join(args)+')\n    End Function\n'
            pos=source.rfind('End Class');assert pos>=0
            source=source[:pos]+snap+source[pos:]
        write('BESHstatNG/src/Graphics/'+cfg['file']+'.vb',source)
    print('Consumed original axis/text preparation through typed Common settings')
if __name__=='__main__':globals()[sys.argv[1]]()
