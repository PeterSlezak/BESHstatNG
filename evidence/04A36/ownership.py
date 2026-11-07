"""Current36 successor crosswalk; accepted13/21-24/29/34 maps stay immutable."""
from work import *
from collections import Counter
def load(name):return json.loads((HERE/name).read_bytes())
if __name__=='__main__':
    ast=load('final-members.json');live=ast['current'];old=ast['entry'];rows=[];preserved=[]
    with (ROOT/'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig') as f:g29=list(csv.DictReader(f))
    with (ROOT/'evidence/04A34/operation-map-final.csv').open(encoding='utf-8-sig') as f:g34=list(csv.DictReader(f))
    superseded={'AddReferenceLine':'SpcPanelPresentation36.New;RenderPanel','AddConstantReferenceLine':'SpcPanelPresentation36.New;RenderPanel','AddZoneLine':'SpcPanelPresentation36.New;RenderPanel','AddLastValueLabel':'SpcSeriesIntent36.LastFiniteOrdinal;RenderPanel','IncludesLines':'SpcPanelPresentation36.New','IncludesBands':'SpcPanelPresentation36.IncludesBands','IncludeValuesInBounds':'SpcPanelPresentation36.AxisBounds','IncludeNullableInBounds':'SpcPanelPresentation36.AxisBounds','HasAnyValue':'SpcSeriesIntent36.LastFiniteOrdinal','IsNumericChartValue':'SpcSeriesIntent36.LastFiniteOrdinal','IsFinite':'SpcSeriesIntent36.LastFiniteOrdinal'}
    spc_port={'RenderPanel':'SpcPanelPresentation36;SpcZoneBands36','AddZoneBands':'SpcZoneBands36','ComputeYAxisBounds':'SpcPanelPresentation36.AxisBounds','HasValidZones':'SpcZoneBands36.HasValidZones'}
    already={'NormalizeText','NormalizeNumberFormat','PanelSupportsSpecificationLimits','SnapshotSpcSettings34','ResolveChartTitle','ResolveValueAxisTitle','AddPointLabels','AddSignalLabels','AddExclusionLabels','AddStageBoundaries'}
    for n in old:
        matches=[v for v in live if v['key']==n['key']]
        if not matches and (n['name']=='AddZoneBands' or n['name']=='.ctor' and n['owner'].endswith('.PanelChartData')):
            matches=[v for v in live if v['owner']==n['owner'] and v['name']==n['name']]
        assert len(matches)<=1
        new=matches[0] if matches else None;name=n['name'];family='KM' if 'KaplanMeier' in n['path'] else 'Cox' if 'CoxSurvival' in n['path'] else 'SPC'
        status='HOST_ONLY';target='';reason='Native chart/series/shape lifetime, styles, font/point/pixel options or Excel blank VARIANT storage/marshalling; no missing data-space algorithm assigned to this operation.';site='';mapping='EXACT_HEADER' if new else 'EXPLICIT_SUPERSESSION'
        if family=='KM' and name=='Plot':
            status='PORTED_AND_CONSUMED';target='KaplanMeierSeriesGeometry36;SurvivalChartPresentation36';site='KaplanMeierPlotExcel.Plot(payload) -> Survival/Censor/Confidence; analysis overload -> payload overload'
            reason='Per-phase independent vectors, origin/padding/source-slot identity and captions at original read points. Empty native censor series and sparse-count exceptions preserved; no eager whole-chart validation.'
        elif family=='KM' and name=='AddKMplot':
            status='ALREADY_PORTABLE_REUSED';target='KaplanMeierPlotExcel.Plot';site='AddKMplot -> analysis Plot -> GetKaplanMeierPlotData -> payload Plot';reason='Historical extension header unchanged; actual shipping wrapper reaches the new projection without estimator changes.'
        elif family=='Cox' and name=='PlotCox':
            status='PORTED_AND_CONSUMED';target='CoxCurveGeometry36;SurvivalChartPresentation36';site='PlotCox -> Validate/Axis before AddChart; Snapshot at XValues and Values setters'
            reason='Original worksheet guard, null/mismatch/empty timing and accepted ChartScaling retained. Separate late copies preserve original time/probability read points.'
        elif family=='SPC' and (name in spc_port or name in superseded):
            status='PORTED_AND_CONSUMED';target=spc_port.get(name,superseded.get(name));site='SpcControlChartExcel.RenderPanel -> PreparePresentation36 -> SpcPanelPresentation36; AddZoneBands -> SpcZoneBands36; ComputeYAxisBounds -> AxisBounds'
            reason='Original typed ten channels/finite masks reused. Pure admission/order/reference/last-index/zone/extent work now portable; actual Excel construction and native style translation remain in RenderPanel/helper tails.'
        elif family=='SPC' and name in already:
            status='ALREADY_PORTABLE_REUSED';target='SpcUnivariateChartProjectionPolicy / Chart34.SpcChartPreparation34';site='Ui21 and Ui22 existing AddCharts overload -> shared RenderPanel; ConfigureChart and typed label/stage helpers'
            reason='Accepted channel/label/stage fraction, title/number-format/spec applicability policies remain consumed. Stage pixels/font acquisition/native point index application stay Windows. Retained private spec applicability shim has no new algorithm; live portable successor reuses the same accepted policy.'
        elif family=='SPC' and name=='AddCharts':
            status='ALREADY_PORTABLE_REUSED';target='SpcUnivariateChartProjectionPolicy;SpcAxisBoundsCore.SpcPanelTop;RenderPanel';site='Ui21 and Ui22 -> existing AddCharts overloads -> shared RenderPanel';reason='Existing post-fit typed projection and explicit-size panel top calculation already portable and consumed; this orchestration retains validated appearance snapshots, chart ownership, errors and rollback.'
        if new is None:assert name in superseded,('Unmapped source member',n['key'])
        if new and n['signature']!=new['signature']:
            assert n['owner'].endswith('.PanelChartData') or name=='AddZoneBands'
            mapping='PRIVATE_HEADER_CHANGED_EXPLICIT_TYPED_ARGUMENT'
        linked=[r['member_id'] for r in g29 if r['path']==n['path'] and r['qualified_type']==n['owner'] and r['name']==name and r['kind']==n['kind']]
        linked34=[r['historical_member_id'] for r in g34 if r['path']==n['path'] and r['name']==name and r['kind']==n['kind']]
        rows.append(dict(member_id=n['key']+'@'+str(n['span_start'])+':'+str(n['span_length']),signature_key=n['key'],family=family,path=n['path'],member=name,entry_signature=n['signature'],entry_lines=str(n['start'])+'-'+str(n['end']),entry_token_sha256=n['token_sha256'],current_member_id=(new['key']+'@'+str(new['span_start'])+':'+str(new['span_length'])) if new else '',final_signature=new['signature'] if new else 'SUPERSEDED; not falsely matched',final_lines=str(new['start'])+'-'+str(new['end']) if new else '',final_token_sha256=new['token_sha256'] if new else '',disposition=status,target=target,compiled_consumer=site,reason=reason,mapping=mapping,accepted29_ids=';'.join(linked),accepted34_ids=';'.join(linked34),authority='PRE_EDIT_LEGACY_EXECUTED / ACCEPTED_CORE / SAME_CORE_WIRING / SOURCE_ONLY_NATIVE; NOT_GOLDEN'))
        if new:
            header=n['header_tokens']==new['header_tokens'];equal=n['token_sha256']==new['token_sha256']
            # Every public header (including 49 options and all overloads) stays exact.
            if n['signature'].startswith('Public') and not n['owner'].endswith('.PanelChartData'):assert header,n['key']
            preserved.append(dict(member_id=rows[-1]['member_id'],member=name,path=n['path'],public_header_preserved=header,token_body_preserved=equal,entry_sha256=n['token_sha256'],final_sha256=new['token_sha256'],native='NATIVE_NOT_RUN'))
    for path in ['BESHStat.Core/src/Graphics/KaplanMeierSeriesGeometry36.vb','BESHStat.Core/src/Graphics/CoxCurveGeometry36.vb','BESHStat.Core/src/Graphics/SpcZoneBands36.vb','BESHStat.Excel.Common/src/Results/SurvivalChartPresentation36.vb','BESHStat.Excel.Common/src/Results/CoxResidualPlotAvailability36.vb','BESHStat.Excel.Common/src/Analysis/SpcPanelPresentation36.vb']:
        assert 'Option Strict On' in (ROOT/path).read_text()
    with (HERE/'member-ownership.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
    history=[]
    for h in g29:
        if h['path'] not in {n['path'] for n in old}:continue
        links=[r for r in rows if r['path']==h['path'] and r['member']==h['name'] and any(n['key']==r['signature_key'] and n['owner']==h['qualified_type'] and n['kind']==h['kind'] for n in old)]
        exact=[r for r in links if r['signature_key']==h['signature_key']]
        if exact:links=exact
        if links:
            assert len(links)==1
            r=links[0];status=r['disposition'];target=r['target'];successor=r['member_id'];reason='Actual35-entry member ->36 successor; '+r['mapping']
        elif h['name']=='HasNaturalZero':
            status='ALREADY_PORTABLE_REUSED';target='BESHStat.Core/src/Graphics/SpcAxisBoundsCore.vb::HasNaturalZero';successor='SpcControlChartExcel.ComputeYAxisBounds -> SpcPanelPresentation36.AxisBounds -> SpcAxisBoundsCore.FromExtents -> HasNaturalZero';reason='Accepted34 explicit relocation, not a false same-file header match. Old source-inspection KeyError/pin failures remain visible.'
            assert 'Private Function HasNaturalZero(' in (ROOT/'BESHStat.Core/src/Graphics/SpcAxisBoundsCore.vb').read_text()
        else:
            assert h['kind'] in ['VariableDeclarator','EnumMemberDeclaration'],h['member_id']
            assert h['name'] in (ROOT/h['path']).read_text()
            status='HOST_ONLY';target='';successor=h['path']+'::'+h['qualified_type']+'.'+h['name'];reason='Retained Windows public enum/native style constant or marshalled category/channel/axis buffer field; declaration stores native adapter state. No numerical algorithm is hidden by this field disposition; its calculating member has a separate successor row.'
        history.append(dict(historical_member_id=h['member_id'],family=h['family'],path=h['path'],name=h['name'],kind=h['kind'],original_signature=h['signature'],original_source_sha256=h['source_sha256'],original_body_sha256=h['body_sha256'],current_successor=successor,disposition=status,target=target,reason=reason))
    with (HERE/'historical-crosswalk.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(history[0]));w.writeheader();w.writerows(history)
    save('coverage.json',dict(status='SOURCE_RECONCILED_PENDING_FINAL_VALIDATION',families=['KM','Cox','SPC'],members=len(rows),historical_members=len(history),by_family=dict(Counter(r['family'] for r in rows)),dispositions=dict(Counter(r['disposition'] for r in rows)),open_blockers=0,explicit_supersessions=[r['member_id'] for r in rows if not r['current_member_id']],native_controls='OPEN_UNCHANGED',residuals='Explicit Score record.Index-to-input-RowId joins only; other blocks accurately unsupported; GUI disabled',scientific='NOT_GOLDEN'))
    save('renderer-preservation.json',dict(status='SOURCE_BOUNDARY_VERIFIED',members=preserved,qualifications=['All public headers exact; private PanelChartData constructor stores accepted immutable projection for the existing reflective adapter test seam; AddZoneBands now accepts pure geometry.','KM four loop/native NewSeries/style tails remain source-identical after removing numeric prefixes and substituting prepared names/arrays/axis constants. Count/order and empty censor slot behavior follow frozen source; no Excel execution.','Cox native clear/create/style/catch/placement tail unchanged except prepared array/scalar/text expressions. X and Y copies remain separate original late reads.','SPC native AddAreaSeries/AddLineSeries/AddMarkerSeries/ConfigureChart/ConfigureSecondaryZoneAxis/SetPointLabel/label helpers/AddStageBoundaries/cleanup remain token-identical. RenderPanel replaces selection arithmetic with prepared ordered roles; native color/weight/dash/marker mapping is explicit.','SPC style mapping: Statistic circle/statistic color+weight; Center solid; UCL/LCL dash/control; Zone round-dot/zone; Specs long-dash/spec; Target dash-dot/target; Excluded X/exclusion; Signal diamond/signal. Last-value labels use original right position/font/color; zone band native order/styles preserved.'],native='NATIVE_NOT_RUN'))
    exact_native={'AddAreaSeries','AddLineSeries','AddMarkerSeries','ConfigureChart','ConfigureSecondaryZoneAxis','SetPointLabel','AddPointLabels','AddSignalLabels','AddExclusionLabels','AddStageBoundaries','ClearSeries','RemoveLegendEntries'}
    assert all(r['token_body_preserved'] for r in preserved if r['member'] in exact_native)
    entry_spc=(HERE/'entry-source/BESHstatNG/src/Graphics/SpcControlChartExcel.vb.txt').read_text(encoding='utf-8-sig')
    final_spc=(ROOT/'BESHstatNG/src/Graphics/SpcControlChartExcel.vb').read_text(encoding='utf-8-sig')
    from freeze import method
    a=method(entry_spc,'AddZoneBands');b=method(final_spc,'AddZoneBands');marker='hiddenLegendEntries.Add(AddAreaSeries('
    assert re.sub(r'\s+',' ',a[a.index(marker):]).strip()==re.sub(r'\s+',' ',b[b.index(marker):]).strip()
    changed={n['key']:n for n in live if ('36.vb' in n['path']) and n['signature'].startswith('Public')}
    save('coordinate-and-chart-spec-contract.json',dict(public_members=[dict(path=n['path'],owner=n['owner'],signature=n['signature'],lines=[n['start'],n['end']],token_sha256=n['token_sha256']) for n in changed.values()],ownership=dict(Core='KM coordinates/source slots; Cox validation/scaling/late copy; SPC zone vectors',Common='KM/Cox captions; SPC ordered admission/legend/limit-label/axis intent; Cox residual data/availability',Windows='Chart/Series/Shape creation, VARIANT marshaling, style/pixel/font acquisition, COM indexes/catches/cleanup'),source_slots=dict(origin=-1,unassigned=-2,other='zero-based input array row/slot; never fabricated worksheet row'),spc_identity='Accepted PointIndex/source rows vs aligned plot ordinal vs one-based Excel point index retained; SetPointLabel alone adds one',copy_policy='Input snapshots and independent arrays/matrices/getters; nested residual arrays cloned recursively. Accepted source models and legacy report blocks unchanged.',exceptions='Legacy null/index/shape exceptions and native phase timing retained; malformed KM censor dimension not repaired',scientific='NOT_GOLDEN'))
    print('Mapped',len(rows),'entry members; explicit supersessions; public APIs retained;0 OPEN')
