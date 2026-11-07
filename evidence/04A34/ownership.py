"""Conservative member census. OPEN_BLOCKER is deliberately retained for unfinished work."""
from entry import ROOT,HERE,save,pin,git
import csv,json,re,collections

TARGETS={
'3D wire objects':('WireObjectGeometryCore;XyzProjectionContext','3Dobjects.vb:Draw/GetRawBounds','ALREADY_PORTABLE_REUSED'),
'AgreementPlot':('ChartReferenceSeries','AgreementPlotExcel.vb:AddPlot/AddHorizontalReferenceLine','PARTIAL'),
'BulletChart':('BulletMeasuredGeometryCore;BulletDrawingLayout;BulletRowLayout','BulletChartExcel.vb:BuildDrawingLayout/BuildRowLayout/NormalizedToChartX/Clip01','PARTIAL'),
'CalibrationPlot':('CalibrationSeriesData;ChartReferenceSeries','CalibrationPlot.vb:addCalibrationPlot','PORTED_AND_CONSUMED'),
'ConvexHullPlot':('ConvexHullPlotCore;ConvexHullPlotGroup','ConvexHullPlot.vb:Compute/AddChart/AddMarkerSeries/AddHullSeries','ALREADY_PORTABLE_REUSED'),
'CumulativeDistributionPlot':('CumulativeDistributionPlotCore;CumulativeStepSeriesData','CumulativeDistributionPlotExcel.vb:ExtendStepToAxisBounds','PARTIAL'),
'DumbbellPlot':('DumbbellPlotCore;DumbbellPlotCategory','DumbbellPlot.vb:Compute;DumbbellPlotExcel.vb:AddConnectorSeries/AddEndpointSeries','PARTIAL'),
'GifAnimator':('AnimationFrameTimingCore','GifAnimator.vb:MsToCentiseconds','PARTIAL_HOST_BOUNDARY'),
'KiteChart':('KiteChartCore;KiteChartSeries','KiteChart.vb:Compute/AddChart/AddCenterLineSeries','ALREADY_PORTABLE_REUSED'),
'LadderPlot':('LadderPlotCore;LadderLabelGeometryCore','LadderPlotExcel.vb:SpreadLabelPositions','PARTIAL'),
'MultipleComparisonPlot':('ComparisonCoordinatesCore;ChartReferenceSeries','MultipleComparisonPlotExcel.vb:AddPlot','FOUNDATION_PORTED_DETAILED_35_RESERVED'),
'PolarPlot':('PolarPlotCore;PolarPlotResult','PolarPlot.vb:Compute/AddChart/ConfigureAxes','ALREADY_PORTABLE_REUSED'),
'PsmLovePlot':('PsmLovePlotProjection;PsmLoveSeriesPolicy','PsmLovePlotExcel.vb:HasAnyFinite/AddThresholdSeries','FOUNDATION_PORTED_DETAILED_35_RESERVED'),
'SankeyPlot':('SankeyPlotCore;SankeyMeasuredGeometryCore;SankeyDrawingLayout','SankeyPlotExcel.vb:BuildDrawingLayout/BuildStageCentres/NormalizedXToChartX/NormalizedYToChartY','PARTIAL'),
'SpcControlChart':('SpcUnivariateChartProjectionPolicy;SpcAxisBoundsCore','SpcControlChartExcel.vb:ComputeYAxisBounds','FOUNDATION_PARTIAL_DETAILED_36_RESERVED'),
'Symmetry':('SymmetrySeriesData','SymetryPlot.vb:AsymmetryPlot','PORTED_AND_CONSUMED'),
'TheilSenPlot':('TheilSenPlotData;TheilSen.GetPlotData','TheilSenPlotExcel.vb:AddPlot','ALREADY_PORTABLE_REUSED')}
PORTED={
'CalibrationPlot':{'addCalibrationPlot'},'Symmetry':{'AsymmetryPlot'},
'AgreementPlot':{'AddHorizontalReferenceLine'},
'MultipleComparisonPlot':{'AddPlot'},'PsmLovePlot':{'HasAnyFinite','AddThresholdSeries'},
'SpcControlChart':{'ComputeYAxisBounds','HasNaturalZero'},
'BulletChart':{'BuildDrawingLayout','BuildRowLayout','NormalizedToChartX','Clip01'},
'SankeyPlot':{'BuildDrawingLayout','BuildStageCentres','NormalizedXToChartX','NormalizedYToChartY'},
'LadderPlot':{'SpreadLabelPositions'},'CumulativeDistributionPlot':{'ExtendStepToAxisBounds'},'GifAnimator':{'MsToCentiseconds'}}
OPEN={
'AgreementPlot':{'AddPlot'},
'BulletChart':{'DrawQualitativeBands','DrawActualBar','DrawTargetMarker','ResolveBandColor','BuildAutomaticNumberFormat','RequiredDecimalPlaces','FormatNumeric','DrawScale','DrawActualValueLabel','DrawTargetValueLabel'},
'CumulativeDistributionPlot':{'ResolveAxisContext','FormatPercentileLabel','ResolveChartTitle','ResolveEmpiricalSeriesName','ResolveFittedSeriesName','ResolveGroupLegendName','AutomaticNumberFormat'},
'DumbbellPlot':{'ResolveAxisContext','NiceStep125','FloorToStep','CeilingToStep','ResolveXAxisNumberFormat','FormatEndpointValue'},
'LadderPlot':{'ResolveAxisContext','NiceStep125','FloorToStep','CeilingToStep','ResolveYAxisNumberFormat','ResolveMinimumLabelSeparation','AddEndpointLabelSeries','BuildEndpointLabelText'},
'SankeyPlot':{'DrawRibbon','DrawNode','DrawNodeLabel','FormatNodeLabel','DrawStageLabels','DrawLegend'}}
RESERVED={'MultipleComparisonPlot':{'BuildTitle','ValidatePlotData','ConfigureAxes','AddComparisonLabels'},'SpcControlChart':{'AddZoneBands','AddZoneLine','AddStageBoundaries','AddPointLabels','AddSignalLabels','AddExclusionLabels','AddLastValueLabel'}}
def csvwrite(name,rows):
    with (HERE/name).open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
def decision(family,name,kind,ownership):
    if name in OPEN.get(family,set()):return 'OPEN_BLOCKER','Residual source-independent geometry/axis/label preparation has not been fully extracted or qualified. Native tails are retained; no HOST_ONLY claim for its arithmetic.'
    if name in PORTED.get(family,set()):return 'PORTED_AND_CONSUMED','Actual named Windows member delegates to the typed portable service; frozen-source parity coverage is recorded separately. Renderer ownership stays native.'
    if name in RESERVED.get(family,set()):return 'LATER_BATCH_RESERVED','Detailed comparison/report/CI refinement is 04A.35; detailed SPC stage/channel/annotation work is 04A.36. Original source/member behavior is protected; no completion claim for those batches.'
    if kind in ('VariableDeclarator','PropertyStatement','PropertyBlock','GetAccessorBlock','SetAccessorBlock','EnumMemberDeclaration','ConstructorBlock'):
        return 'HOST_ONLY','Retained concrete Windows state/default/accessor/enum/constructor boundary. This record is not a new numerical algorithm; public mutable appearance/renderer configuration, native type and alias semantics remain in the original host class.'
    if family in ('3D wire objects','TheilSenPlot'):
        return 'ALREADY_PORTABLE_REUSED','Accepted portable plot-data/projection engine is already called by this Windows family. No numerical re-extraction or change to accepted engine; native drawing still belongs to Windows.'
    if name in ('Compute','ComputeGrouped','ComputeBasic','ComputeWithAuxiliary','ComputeStaged','EvaluateFittedCdf'):
        return 'ALREADY_PORTABLE_REUSED','Current wrapper directly forwards the existing typed Core engine; source/target and call-site are recorded. Statistical algorithm is unchanged.'
    if ownership in ('WINDOWS_NATIVE_ONLY','WINDOWS_BRIDGE_REQUIRED') or name.startswith(('Chart_','Validate','Delete','Hide','Configure','Attach','Prune','SetPoint','RemoveLegend','Suppress','SameChart','OwnsChart','Detach','GetChartSize','UpdateProgress','GetEncoder','RenderToCanvas','CreatePropertyItem','SetFrameDelayProperty','SetLoopCountProperty','CountAllFrames','GetFirstFrameSize','CreateAnimatedGif')):
        return 'HOST_ONLY','Exact retained native/member boundary: original '+name+' owns chart/shape/appearance validation, interop marshalling, style/event/cleanup, text measurement, encoding or progress lifetime. No native execution or portable renderer is claimed.'
    if family in ('ConvexHullPlot','KiteChart','PolarPlot') and name in ('AddChart','AddMarkerSeries','AddHullSeries','AddAreaSeries','AddMaskSeries','AddBoundaryLineSeries','AddCenterLineSeries','AddLineSeries','AddLabelSeries','AddDataSeries'):
        return 'ALREADY_PORTABLE_REUSED','Current renderer consumes accepted Core coordinate/category/grid/label vectors directly; no new statistical or transformation algorithm is introduced. Native series styles and point indexing remain host-owned.'
    return 'OPEN_BLOCKER','Candidate needs further member-specific reconciliation. No invented portable ownership or blanket host-only exception is asserted.'

if __name__=='__main__':
    old=list(csv.DictReader((HERE/'operation-map-entry.csv').open(encoding='utf-8-sig')))
    current=json.loads((HERE/'source-ownership.json').read_bytes())['current_nodes']
    by=collections.defaultdict(list)
    for n in current:by[n['key']].append(n)
    historical=[];current_rows={}
    for m in old:
        status,reason=decision(m['family'],m['name'],m['kind'],m['ownership'])
        hits=by[m['signature_key']]
        target,site,outcome=TARGETS[m['family']]
        row=dict(historical_member_id=m['member_id'],operation_id=m['operation_id'],family=m['family'],path=m['path'],name=m['name'],kind=m['kind'],disposition=status,reason=reason,target_type=target,windows_consumption=site,entry_sha256=m['entry_sha256'],final_sha256=pin(m['path'])['sha256'],current_ranges=';'.join(str(h['start'])+'-'+str(h['end']) for h in hits),current_match_count=len(hits),fixture_authority='PRE_EDIT_LEGACY where new and tested; ACCEPTED_CORE/SAME_CORE_WIRING for prior engines; SOURCE_ONLY for native tails; NOT_GOLDEN')
        if not hits and status not in ('PORTED_AND_CONSUMED',):
            row['disposition']='OPEN_BLOCKER';row['reason']='Historical candidate has no exact current header match; further source reconciliation required. '+reason
        historical.append(row)
        for h in hits:
            key=h['key']+'@'+str(h['source_span_start'])+':'+str(h['source_span_length'])
            r=current_rows.setdefault(key,dict(current_member_id=key,historical_member_ids=[],family=m['family'],path=m['path'],name=h['name'],kind=h['kind'],qualified_type=h['qualified_type'],signature=h['signature'],start=h['start'],end=h['end'],disposition=row['disposition'],reason=row['reason'],target_type=target,windows_consumption=site,source_sha256=pin(m['path'])['sha256']))
            r['historical_member_ids'].append(m['member_id'])
    csvwrite('operation-map-final.csv',historical)
    rows=list(current_rows.values())
    for r in rows:r['historical_member_ids']=';'.join(r['historical_member_ids'])
    csvwrite('family-ownership-map.csv',rows)
    families=[]
    for f,(target,site,outcome) in TARGETS.items():
        h=[r for r in historical if r['family']==f];c=[r for r in rows if r['family']==f]
        families.append(dict(family=f,historical_candidates=len(h),current_distinct_candidate_members=len(c),historical_dispositions=dict(collections.Counter(r['disposition'] for r in h)),current_dispositions=dict(collections.Counter(r['disposition'] for r in c)),target_type=target,windows_consumption=site,implementation_outcome=outcome,complete=False if any(r['disposition']=='OPEN_BLOCKER' for r in h) else True,actually_changed=any(pin(r['path'])['sha256']!=r['entry_sha256'] for r in h),minimum_remaining_members=sorted(OPEN.get(f,set()))))
    save('family-coverage.json',dict(schema='04A34-family-coverage-v1',status='NOT_READY_04A34',families=families,historical_candidate_count=len(historical),current_distinct_candidate_count=len(rows),current_entire_22_file_census=len(current),scope='Current family candidate members are deduplicated by current Roslyn identity. Historical 768 declarations are leads, not 768 rewrites. Other members in the 22 source files are inventory context, not automatically assigned34.',open_blockers=sum(r['disposition']=='OPEN_BLOCKER' for r in rows),historical_open_blockers=sum(r['disposition']=='OPEN_BLOCKER' for r in historical),unmatched_historical=sum(r['current_match_count']==0 for r in historical),native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN'))
    print('17 families;',len(rows),'current candidate members;',sum(r['disposition']=='OPEN_BLOCKER' for r in rows),'OPEN_BLOCKER; NOT_READY_04A34')
