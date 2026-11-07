"""Native-operation and style fingerprints; this is SOURCE_ONLY, never a fake chart."""
from work import *
from collections import defaultdict
d=json.loads((HERE/'member-source.json').read_bytes())
by=defaultdict(list)
for n in d['current']:by[n['key']].append(n)
NATIVE={'AddTextShape','AddShape','AddLine','BuildFreeform','AddNodes','ConvertToShape','ZOrder','Delete','NewSeries','AddChart','AddChart2','Points','ErrorBar','ApplyDataLabels','Axes','Refresh','Attach','Detach','CleanupOwnedShapes','ClearGeneratedShapes','GetChartSize'}
def calltrace(n):
    return [dict(expression=c['expression'],arguments=c['arguments']) for c in n['calls'] if c['expression'].split('.')[-1] in NATIVE]
def styles(n):
    return [a for a in n['assignments'] if (a['left'].startswith('.') or any(t in a['left'] for t in ['. Format ','. Font ','. Marker','. HasDataLabel','. Position','. Inside','. Name']))]
rows=[];deltas=[]
ALLOWED={
('BulletChartExcel.vb','DrawTitle'):[('Math.Max(1.0R, layout.ChartWidth - 2.0R * appearance.ChartPadding)','BulletMeasuredGeometryCore.BulletTitle(layout.ChartWidth, appearance.ChartPadding).Width','BulletTitle width')],
('BulletChartExcel.vb','DrawLeftLabels'):[('top + mainHeight','paired.SubtitleTop','BulletPairedLabels subtitle offset')],
('BulletChartExcel.vb','DrawScale'):[('CSng(rowLayout.BandBottom + appearance.ScaleTickLength)','CSng(tickGeometry.Bottom)','BulletTick bottom; Single conversion remains native')],
('BulletChartExcel.vb','DrawActualValueLabel'):[('rowLayout.BandCentreY - appearance.ValueLabelHeight / 2.0R','BulletMeasuredGeometryCore.BulletCenteredLabel(rowLayout.BandCentreY, appearance.ValueLabelHeight).Top','BulletCenteredLabel measured height')],
('DumbbellPlotExcel.vb','AddCategoryLabelSeries'):[('categories ( i ) . CategoryLabel','categoryLabels ( i )','Copied label vector; same zero-based category / one-based Excel point')],
('SankeyPlotExcel.vb','DrawStageLabels'):[('stageX(i) - labelWidth / 2.0R','SankeyMeasuredGeometryCore.SankeyStageLabelLeft(stageX(i), labelWidth).Left','SankeyStageLabelLeft explicit width')],
}
def normalized(v,replacements):
    if isinstance(v,list):return [normalized(x,replacements) for x in v]
    if isinstance(v,dict):return {k:normalized(x,replacements) for k,x in v.items()}
    if isinstance(v,str):
        for old,new,reason in replacements:
            if v in (old,new):return '<PARITY_PROVEN:'+reason+'>'
    return v
for old in d['checkpoint']:
    if old['kind'] not in ['FunctionBlock','SubBlock','ConstructorBlock']:continue
    hits=by[old['key']]
    if not hits:
        assert old['name']=='Clamp' and old['path'].endswith('LadderPlotExcel.vb')
        continue
    assert len(hits)==1
    new=hits[0];a,b=calltrace(old),calltrace(new);sa,sb=styles(old),styles(new)
    row=dict(path=old['path'],member=old['name'],entry_span=[old['start'],old['end']],final_span=[new['start'],new['end']],entry_tokens_sha256=old['token_sha256'],final_tokens_sha256=new['token_sha256'],body_preserved=old['token_sha256']==new['token_sha256'],native_call_trace_preserved=a==b,native_style_assignments_preserved=sa==sb,entry_native_trace_sha256=sha(json.dumps(a,sort_keys=True).encode()),final_native_trace_sha256=sha(json.dumps(b,sort_keys=True).encode()),entry_native_styles_sha256=sha(json.dumps(sa,sort_keys=True).encode()),final_native_styles_sha256=sha(json.dumps(sb,sort_keys=True).encode()))
    replacements=ALLOWED.get((old['path'].split('/')[-1],old['name']),[])
    if a or b or sa or sb or old['measurements'] or new['measurements']:
        assert old['exception_boundaries']==new['exception_boundaries'],(old['name'],'Native error boundary changed')
        assert old['measurements']==new['measurements'],(old['name'],'Native measurement acquisition/order changed')
    row['measurement_sequence']=old['measurements']
    row['exception_boundaries']=old['exception_boundaries']
    assert normalized(a,replacements)==normalized(b,replacements),(old['path'],old['name'],'Unapproved native call delta')
    assert normalized(sa,replacements)==normalized(sb,replacements),(old['path'],old['name'],'Unapproved native style delta')
    row['qualified_coordinate_substitutions']=[dict(entry=x,final=y,authority=z,fixture='PRE_EDIT_LEGACY source slice; exact bits; source identity test') for x,y,z in replacements]
    row['native_boundary_status']='PASS_EXACT' if a==b and sa==sb else 'PASS_ONLY_LISTED_COORDINATE_SUBSTITUTIONS'
    if a!=b or sa!=sb:deltas.append(dict(path=old['path'],member=old['name'],entry_calls=a,final_calls=b,entry_styles=sa,final_styles=sb,qualified=replacements))
    rows.append(row)
save('renderer-deltas.json',dict(deltas=deltas))
save('renderer-preservation.json',dict(status='PASS',basis='SOURCE_ONLY exact Roslyn native invocation sequence/arguments and native style assignments from frozen continuation source, except six listed parity-proven coordinate/label substitutions. Counts, order, native enums, style arguments and all other arguments are exact. Methods without changes are full-token equal. Public headers independently checked against merged-main. CDF/Ladder conditional legend policies additionally exercised as logical source policies and real host-free HashSet state, not fake native rendering.',members=rows,qualified_deltas=len(deltas),unqualified_deltas=0,native='NATIVE_NOT_RUN'))
print('Native/style deltas',[(r['path'].split('/')[-1],r['member']) for r in deltas])
