"""Freeze numerical prefixes, then separately apply their portable extraction."""
from work import *
SPECS=[]
def add(family,member,start,end,name,args,subs,fields,returns,guard=False):
    text=(HERE/'entry-source'/((family+'ChartExcel' if family=='Bullet' else family+'PlotExcel')+'.vb.txt')).read_text(encoding='utf-8')
    raw=method(text,member);a=raw.index(start);z=raw.index(end,a);fragment=raw[a:z]
    b=fragment
    for old,new in sorted(subs.items(),key=lambda x:-len(x[0])):b=b.replace(old,new)
    if guard:
        b=re.sub(r'(?m)^(\s*)Return\s*$',r'\1Return Nothing',b)
        b=re.sub(r'(?m)Then Return\s*$',r'Then Return Nothing',b)
    for call in ['NormalizedToChartX','NormalizedXToChartX','NormalizedYToChartY']:
        if call in b:
            suffix=', plotLeft, plotWidth' if family=='Bullet' else ', stageLeft, stageRight' if call=='NormalizedXToChartX' else ', plotTop, plotHeight'
            # Arguments here are simple identifiers/arithmetic, without nested calls.
            b=re.sub(call+r'\(([^,]+), layout\)',('MapX' if call!='NormalizedYToChartY' else 'MapY')+r'(\1'+suffix+')',b)
    typeName=family+name+'Geometry'
    function='    Public Function '+name+'('+', '.join(n+' As '+t for n,t in args)+') As '+typeName+'\n'+b+'        Return New '+typeName+'('+', '.join(returns)+')\n    End Function\n'
    SPECS.append(dict(family=family,member=member,name=name,args=args,fields=fields,returns=returns,type=typeName,function=function,original_fragment=fragment,original_lf_sha256=sha(fragment.encode()),adapter_original_prefix=fragment))

add('Bullet','DrawQualitativeBands','        Dim x1 As Double','            Dim bandShape', 'Band',[('normalizedStart','Double'),('normalizedEnd','Double'),('plotLeft','Double'),('plotWidth','Double')],{'band.NormalizedStart':'normalizedStart','band.NormalizedEnd':'normalizedEnd'},[('X1','Double'),('X2','Double'),('Width','Double')],['x1','x2','bandWidth'])
add('Bullet','DrawActualBar','        Dim normalized As Double','        Dim barShape','Actual',[('actualNormalized','Double'),('bandHeight','Double'),('barHeightFraction','Double'),('bandCentreY','Double'),('plotWidth','Double')],{'item.ActualNormalized':'actualNormalized','layout.BandHeight':'bandHeight','appearance.ActualBarHeightFraction':'barHeightFraction','rowLayout.BandCentreY':'bandCentreY','layout.PlotWidth':'plotWidth'},[('Height','Double'),('Top','Double'),('Width','Double')],['barHeight','barTop','barWidth'],True)
add('Bullet','DrawTargetMarker','        Dim x As Double','        Dim marker As Object','Target',[('targetNormalized','Double'),('plotLeft','Double'),('plotWidth','Double'),('bandHeight','Double'),('heightFraction','Double'),('bandCentreY','Double')],{'item.TargetNormalized':'targetNormalized','layout.BandHeight':'bandHeight','appearance.TargetMarkerHeightFraction':'heightFraction','rowLayout.BandCentreY':'bandCentreY'},[('X','Double'),('Y1','Double'),('Y2','Double')],['x','y1','y2'])
add('Bullet','DrawActualValueLabel','        Dim actualX As Double','        Dim numberFormat','ActualLabel',[('actualNormalized','Double'),('plotLeft','Double'),('plotWidth','Double'),('valueLabelWidth','Double'),('valueLabelGap','Double')],{'item.ActualNormalized':'actualNormalized','appearance.ValueLabelWidth':'valueLabelWidth','appearance.ValueLabelGap':'valueLabelGap','layout.PlotLeft':'plotLeft','layout.PlotWidth':'plotWidth','Dim alignment As Integer':'Dim alignment As Integer','MsoAlignLeft':'0','MsoAlignRight':'1'},[('Left','Double'),('Width','Double'),('RightAligned','Boolean')],['left','labelWidth','alignment = 1'])
add('Bullet','DrawTargetValueLabel','        Dim targetX As Double','        Dim numberFormat','TargetLabel',[('targetNormalized','Double'),('plotLeft','Double'),('plotWidth','Double'),('valueLabelWidth','Double')],{'item.TargetNormalized':'targetNormalized','appearance.ValueLabelWidth':'valueLabelWidth','layout.PlotLeft':'plotLeft','layout.PlotWidth':'plotWidth'},[('Left','Double'),('Width','Double')],['left','labelWidth'])
add('Sankey','DrawRibbon','        Dim sourceCentreX As Double','        Dim builder As Object','Ribbon',[('sourceNodeX','Double'),('targetNodeX','Double'),('nodeWidth','Double'),('sourceTopValue','Double'),('targetTopValue','Double'),('thickness','Double'),('curvature','Double'),('stageLeft','Double'),('stageRight','Double'),('plotTop','Double'),('plotHeight','Double')],{'sourceNode.X':'sourceNodeX','targetNode.X':'targetNodeX','appearance.NodeWidth':'nodeWidth','link.SourceTop':'sourceTopValue','link.TargetTop':'targetTopValue','link.Thickness':'thickness','appearance.RibbonCurvature':'curvature'},[(n,'Double') for n in ['SourceX','TargetX','SourceTop','SourceBottom','TargetTop','TargetBottom','C1X','C2X']],['sourceX','targetX','sourceTop','sourceBottom','targetTop','targetBottom','c1x','c2x'],True)
add('Sankey','DrawNode','        Dim centreX As Double','        Dim nodeShape As Object','Node',[('nodeX','Double'),('nodeY','Double'),('nodeHeightValue','Double'),('nodeWidth','Double'),('stageLeft','Double'),('stageRight','Double'),('plotTop','Double'),('plotHeight','Double')],{'node.X':'nodeX','node.Y':'nodeY','node.Height':'nodeHeightValue','appearance.NodeWidth':'nodeWidth','layout.PlotHeight':'plotHeight'},[(n,'Double') for n in ['Left','Top','Height']],['nodeLeft','nodeTop','nodeHeight'])
# Labels take an explicit copied stage-centre array; all identity/index decisions stay in order.
add('Sankey','DrawNodeLabel','        Dim centreX As Double','        AddTextShape','NodeLabel',[('nodeX','Double'),('nodeY','Double'),('nodeHeightValue','Double'),('nodeStageIndex','Integer'),('stageX','Double()'),('nodeWidth','Double'),('labelGap','Double'),('labelWidthValue','Double'),('fontSize','Double'),('stageLeft','Double'),('stageRight','Double'),('plotTop','Double'),('plotHeight','Double')],{'node.X':'nodeX','node.Y':'nodeY','node.Height':'nodeHeightValue','node.StageIndex':'nodeStageIndex','appearance.NodeWidth':'nodeWidth','appearance.NodeLabelGap':'labelGap','appearance.NodeLabelWidth':'labelWidthValue','appearance.NodeLabelFontSize':'fontSize','layout.PlotHeight':'plotHeight','layout.PlotTop':'plotTop'},[(n,'Double') for n in ['Left','Top','Width','Height']],['labelLeft','labelTop','labelWidth','labelHeight'])

def freeze():
    assert not (HERE/'pre-edit-geometry.csv').exists()
    # No remaining numeric member has changed since the continuation entry.
    for family in ['Bullet','Sankey']:
        file=family+('ChartExcel' if family=='Bullet' else 'PlotExcel')
        current=src(file);old=(HERE/'entry-source'/(file+'.vb.txt')).read_text(encoding='utf-8')
        for r in SPECS:
            if r['family']==family:assert method(current,r['member'])==method(old,r['member'])
    save('geometry-spec.json',dict(authority='PRE_EDIT_LEGACY',scientific='NOT_GOLDEN',functions=SPECS,adapter_prefixes='Copied verbatim; only parameter bindings, explicit measurements, Sub Return->Nothing and test namespace adapted. Existing accepted MapX/MapY/Clip01 dependencies retained.'))
    text=HEADER+'Namespace Global.Batch34CompletionFixtures\n'
    for family in ['Bullet','Sankey']:
        module=family+'MeasuredGeometryCore'
        text+='Public Module Legacy'+family+'Shapes34\n'
        for methodName,args in ([('MapX','value As Double, plotLeft As Double, plotWidth As Double'),('Clip01','value As Double')] if family=='Bullet' else [('MapX','value As Double, stageLeft As Double, stageRight As Double'),('MapY','value As Double, plotTop As Double, plotHeight As Double')]):
            vals=', '.join(a.split(' As ')[0] for a in args.split(', '))
            text+='Private Function '+methodName+'('+args+') As Double\nReturn BESHStatNG.'+module+'.'+methodName+'('+vals+')\nEnd Function\n'
        text+='\n'.join(r['function'] for r in SPECS if r['family']==family)+'End Module\n'
        text+='\n'.join(dto(r['type'],r['fields']) for r in SPECS if r['family']==family)
    text+='End Namespace\n'
    write('BESHStatNG.Test/src/Charts/LegacyCompletionShapes34.vb',text)
    print('Frozen',len(SPECS),'original geometry prefixes; compile/capture before apply')
def apply():
    assert (HERE/'pre-edit-geometry.csv').exists(),'Execute old-source capture first'
    data=json.loads((HERE/'geometry-spec.json').read_bytes())['functions']
    for family in ['Bullet','Sankey']:
        module=family+'MeasuredGeometryCore';p='BESHStat.Core/src/Graphics/'+module+'.vb';s=(ROOT/p).read_text(encoding='utf-8')
        assert 'Public Function Ribbon(' not in s and 'Public Function Band(' not in s
        functions='\n'.join(r['function'] for r in data if r['family']==family)
        s=s.replace('Public Module '+module,'Public Module '+module+'\n'+functions)
        s+='\n'+'\n'.join(dto(r['type'],r['fields']) for r in data if r['family']==family)
        # Stage-centre getters are not stored; parameters are read only during this
        # synchronous calculation and no input or returned mutable array is aliased.
        write(p,s)
    calls={
      'Band':('band.NormalizedStart, band.NormalizedEnd, layout.PlotLeft, layout.PlotWidth',{'x1':'X1','x2':'X2','bandWidth':'Width'}),
      'Actual':('item.ActualNormalized, layout.BandHeight, appearance.ActualBarHeightFraction, rowLayout.BandCentreY, layout.PlotWidth',{'barHeight':'Height','barTop':'Top','barWidth':'Width'}),
      'Target':('item.TargetNormalized, layout.PlotLeft, layout.PlotWidth, layout.BandHeight, appearance.TargetMarkerHeightFraction, rowLayout.BandCentreY',{'x':'X','y1':'Y1','y2':'Y2'}),
      'ActualLabel':('item.ActualNormalized, layout.PlotLeft, layout.PlotWidth, appearance.ValueLabelWidth, appearance.ValueLabelGap',{'left':'Left','labelWidth':'Width'}),
      'TargetLabel':('item.TargetNormalized, layout.PlotLeft, layout.PlotWidth, appearance.ValueLabelWidth',{'left':'Left','labelWidth':'Width'}),
      'Ribbon':('sourceNode.X, targetNode.X, appearance.NodeWidth, link.SourceTop, link.TargetTop, link.Thickness, appearance.RibbonCurvature, layout.StageCentreLeft, layout.StageCentreRight, layout.PlotTop, layout.PlotHeight',{n:n[0].upper()+n[1:] for n in ['sourceX','targetX','sourceTop','sourceBottom','targetTop','targetBottom','c1x','c2x']}),
      'Node':('node.X, node.Y, node.Height, appearance.NodeWidth, layout.StageCentreLeft, layout.StageCentreRight, layout.PlotTop, layout.PlotHeight',{'nodeLeft':'Left','nodeTop':'Top','nodeHeight':'Height'}),
      'NodeLabel':('node.X, node.Y, node.Height, node.StageIndex, stageX, appearance.NodeWidth, appearance.NodeLabelGap, appearance.NodeLabelWidth, appearance.NodeLabelFontSize, layout.StageCentreLeft, layout.StageCentreRight, layout.PlotTop, layout.PlotHeight',{'labelLeft':'Left','labelTop':'Top','labelWidth':'Width','labelHeight':'Height'})}
    for family in ['Bullet','Sankey']:
        file=family+('ChartExcel' if family=='Bullet' else 'PlotExcel');s=src(file)
        for r in data:
            if r['family']!=family:continue
            args,fields=calls[r['name']]
            b='        Dim geometry As '+r['type']+' = '+family+'MeasuredGeometryCore.'+r['name']+'('+args+')\n'
            if r['name'] in ('Actual','Ribbon'):b+='        If geometry Is Nothing Then Return\n'
            b+=''.join('        Dim '+n+' As Double = geometry.'+prop+'\n' for n,prop in fields.items())
            if r['name']=='ActualLabel':b+='        Dim alignment As Integer = If(geometry.RightAligned, MsoAlignRight, MsoAlignLeft)\n'
            original=r['adapter_original_prefix'];assert original in s,(file,r['member'])
            s=s.replace(original,b+'\n')
        write('BESHstatNG/src/Graphics/'+file+'.vb',s)
    print('Consumed frozen Bullet/Sankey shape geometry in actual native renderers')
if __name__=='__main__':globals()[sys.argv[1]]()
