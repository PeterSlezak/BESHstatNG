"""Bind every historical candidate to live Roslyn spans, with explicit relocations."""
from work import *
import collections
sys.path.insert(0,str(HERE.parent))
from ownership import TARGETS
P=HERE.parent
def readcsv(p):return list(csv.DictReader(p.open(encoding='utf-8-sig')))
def csvout(name,rows):
    with (P/name).open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
def out(name,v):write('evidence/04A34/'+name,json.dumps(v,ensure_ascii=False,indent=2)+'\n')
data=json.loads((HERE/'member-source.json').read_bytes());nodes=data['current'];checkpoint=data['checkpoint']
old=readcsv(P/'operation-map-entry.csv');partial=readcsv(HERE/'checkpoint/operation-map-final.csv');stable=readcsv(HERE/'checkpoint/family-ownership-map.csv')
by=collections.defaultdict(list)
for n in nodes:by[n['key']].append(n)
def uid(n):return n['key']+'@'+str(n['source_span_start'])+':'+str(n['source_span_length'])
def select(path,name,token=None):
    hits=[n for n in nodes if n['path'].endswith('/'+path+'.vb') and n['name']==name and (token is None or token in n['tokens'])]
    assert len(hits)==1,(path,name,token,len(hits))
    return hits[0]
relocations={}
for r in old:
    id=r['member_id'];family=r['family'];name=r['name']
    target=None;reason=None;site=None
    if family=='LadderPlot' and name=='lambda' and 'c2396206bed850c5' in id:
        first=id.endswith('@30089:21')
        target=select('LadderLabelGeometryCore','lambda','values ( i )' if first else 'Function ( i ) i')
        reason='OrderBy value selector' if first else 'ThenBy original ordinal tie-break selector'
        site=select('LadderPlotExcel','SpreadLabelPositions')
    elif family=='LadderPlot' and name=='Clamp':
        target=select('LadderLabelGeometryCore','Clamp');reason='Removed unused duplicate private Windows clamp; the accepted partial portable spreading implementation already owns and calls this exact clamp.';site=select('LadderPlotExcel','SpreadLabelPositions')
    elif family=='PsmLovePlot' and name=='lambda':
        target=select('PsmLoveSeriesPolicy','lambda');reason='Finite admission Any predicate; original AppInfrastructure.IsFinite bound to explicit NaN/infinity predicate. Ordering/missing policy unchanged.';site=select('PsmLovePlotExcel','HasAnyFinite')
    elif family=='SpcControlChart' and name=='HasNaturalZero':
        target=select('SpcAxisBoundsCore','HasNaturalZero');reason='Private function relocated, consumed by FromExtents called by ComputeYAxisBounds; original same-file member genuinely absent.';site=select('SpcControlChartExcel','ComputeYAxisBounds')
    elif family=='SankeyPlot' and name=='lambda':
        original=[n for n in checkpoint if n['key']==r['signature_key'] and n['enclosing_name']=='ResolveCategoryStyles']
        if original:
            nodeSort='Function ( n )' in original[0]['tokens']
            target=select('SpecialtyLegendPolicy34','lambda','n . FirstAppearanceIndex' if nodeSort else 'firstIndices ( i )')
            reason='Stable OrderBy FirstAppearanceIndex moved to '+('SankeyNodeOrder' if nodeSort else 'SankeyLegendOrder')+'; actual host palette/override application and source ordinal materialization remain native.';site=select('SankeyPlotExcel','ResolveCategoryStyles')
    if target:
        relocations[id]=dict(historical_member_id=id,source_path=r['path'],historical_body_sha256=r['body_sha256'],historical_span=r['span_start']+':'+r['span_length'],successor=uid(target),target_path=target['path'],target_name=target['name'],target_span=[target['start'],target['end']],target_tokens_sha256=target['token_sha256'],reason=reason,windows_call_site=uid(site),same_file_header_match=bool(by[r['signature_key']]),binding='Explicit source/operator relocation; not an invented same-file match',scientific='NOT_GOLDEN')

RESERVED={
 'MultipleComparisonPlot':{'BuildTitle','ValidatePlotData','AddComparisonLabels'},
 'SpcControlChart':{'AddZoneBands','AddZoneLine','AddStageBoundaries','AddPointLabels','AddSignalLabels','AddExclusionLabels','AddLastValueLabel','IncludesLines','IncludesBands','HasValidZones'}}
HOST_REASONS={
 'Copy':'Copies mutable Windows appearance/style arrays and native option state at the original validation/draw point; retains accepted alias/copy contract. Numeric preparation snapshots only its used scalar/string fields per call.',
 'CopyValidated':'Validates and snapshots Windows chart options, native markers/colors/fonts and GUI presentation defaults. Portable ten-channel projection remains the accepted Common owner.',
 'ResolveStyle':'Resolves concrete OLE color, Excel marker/line enums, native font/line sizes and per-group overrides. No hull/polygon computation; accepted Core point/order/closure data is consumed separately.',
 'FindOverride':'Matches mutable native group-style override records using the original culture/trim/first-match rule; returns the actual Windows appearance object containing Excel enums/RGB/line weights.',
 'FindCategoryOverride':'Returns original mutable Windows category style override (RGB/native formatting), retaining first-match and current-culture matching. Flow/category numerical identity belongs to accepted Sankey Core.',
 'ResolveGroupColor':'Selects actual host RGB/OLE palette entry and native appearance override; group ordinal comes unchanged from accepted Core.',
 'ResolveGroupMarkerStyle':'Selects concrete Excel XlMarkerStyle by accepted Core group ordinal; no portable coordinate computation.',
 'ResolveSeriesColor':'Selects actual Windows OLE/RGB palette by current group ordinal; portable group/series naming is delegated independently.',
 'ResolveObservationColor':'Selects native OLE/RGB single/observation/group palette by accepted Core indices; palette ownership and mutable overrides remain Windows.',
 'BlendOleColor':'OLE ColorTranslator conversion and host RGB interpolation for marker color; System.Drawing remains Windows-only. No domain geometry.',
 'AddTextShape':'Creates native text boxes and applies current font/alignment/RGB, acquiring actual text measurements on the original draw path. Explicit numeric placement is supplied by portable measured geometry.',
 'AddReferenceLine':'Receives accepted projected channel categories/Object values, performs Excel-error admission and creates/formats a native line; optional specialized last-value annotation remains 04A.36.',
 'IncludeValuesInBounds':'Marshals actual Excel Object values (including ErrorWrapper and Excel errors) under original IsNumericChartValue/Convert policy. Numerical comparison delegates through IncludeNullableInBounds to portable SpcOptionalExtent.',
}
NEW_PORTED={
 'AgreementPlot':{'AddPlot','AddHorizontalReferenceLine'},
 'BulletChart':{'DrawBulletChart','DrawTitle','DrawQualitativeBands','ResolveBandColor','DrawActualBar','DrawTargetMarker','DrawLeftLabels','DrawScale','DrawActualValueLabel','DrawTargetValueLabel','ResolveNumberFormat','BuildAutomaticNumberFormat','RequiredDecimalPlaces','FormatNumeric','BuildDrawingLayout','BuildRowLayout','NormalizedToChartX','Clip01'},
 'CumulativeDistributionPlot':{'AddEmpiricalSeries','AddFittedSeries','AddPercentileReferenceSeries','FormatPercentileLabel','RegisterLegendSeries','ResolveAxisContext','ResolveChartTitle','ResolveEmpiricalSeriesName','ResolveFittedSeriesName','ResolveGroupLegendName','DistributionShortName','AutomaticNumberFormat','ExtendStepToAxisBounds'},
 'DumbbellPlot':{'SuggestedHeight','AddConnectorSeries','AddEndpointSeries','AddAuxiliarySeries','AddCategoryLabelSeries','AddEndpointValueLabels','ResolveAxisContext','NiceStep125','FloorToStep','CeilingToStep','ResolveXAxisNumberFormat','FormatEndpointValue','ResolveSeriesName'},
 'LadderPlot':{'SuggestedWidth','SuggestedHeight','AddObservationSeries','AddSideLabelSeries','AddEndpointLabelSeries','ResolveMinimumLabelSeparation','BuildEndpointLabel','FormatEndpointValue','ResolveObservationSeriesName','ShouldKeepLegendEntry','ResolveAxisContext','ResolveYAxisNumberFormat','NiceStep125','FloorToStep','CeilingToStep','Clamp','SpreadLabelPositions'},
 'MultipleComparisonPlot':{'SuggestedHeight','AddPlot'},
 'PsmLovePlot':{'HasAnyFinite','AddThresholdSeries'},
 'SankeyPlot':{'ResolveCategoryStyles','DrawTitle','DrawRibbon','DrawNode','DrawNodeLabel','FormatNodeLabel','DrawStageLabels','DrawLegend','BuildDrawingLayout','BuildStageCentres','NormalizedXToChartX','NormalizedYToChartY'},
 'SpcControlChart':{'NormalizeText','NormalizeNumberFormat','AddCharts','AddConstantReferenceLine','IncludeNullableInBounds','PanelSupportsSpecificationLimits','ResolveChartTitle','ResolveValueAxisTitle','ComputeYAxisBounds','HasNaturalZero'},
 'ConvexHullPlot':{'ResolveSeriesName'},'PolarPlot':{'ResolveSeriesName','MakePlotAreaSquare'},
 'CalibrationPlot':{'addCalibrationPlot'},'Symmetry':{'AsymmetryPlot'},'GifAnimator':{'MsToCentiseconds'}}

# Lexical same-owner call graph, bounded and recorded. Tests compile actual adapters;
# these source edges are not represented as independent scientific/native execution.
def reachable(n,seen=None):
    seen=set() if seen is None else seen
    if uid(n) in seen:return []
    seen.add(uid(n));edges=[]
    for creation in n['creations']:
        for t in nodes:
            if not t['path'].startswith('BESHstatNG/') and t['name']=='.ctor' and t['qualified_type'].split('.')[-1]==creation['expression'].split('.')[-1]:edges.append(dict(site=uid(n),expression='New '+creation['expression'],target=uid(t),target_path=t['path'],target_span=[t['start'],t['end']]))
    for call in n['calls']:
        e=call['expression'];last=e.split('.')[-1]
        direct=[t for t in nodes if not t['path'].startswith('BESHstatNG/') and t['name']==last and t['qualified_type'].split('.')[-1]+'.'+last in e]
        if direct:
            for t in direct:edges.append(dict(site=uid(n),expression=e,target=uid(t),target_path=t['path'],target_span=[t['start'],t['end']]))
        elif '.' not in e:
            for t in nodes:
                if t['path']==n['path'] and t['qualified_type']==n['qualified_type'] and t['name']==last:edges+=reachable(t,seen)
    return edges
def classify(r,hits):
    f,name,kind=r['family'],r['name'],r['kind']
    if r['member_id'] in relocations:return 'PORTED_AND_CONSUMED',relocations[r['member_id']]['reason']
    if name in NEW_PORTED.get(f,set()):
        return 'PORTED_AND_CONSUMED','Basic 04A.34 numerical/series/label preparation delegates at the current Windows '+name+' call point; exact portable targets and source spans are in the consumption receipt. Native tail/options/exception timing preserved by frozen-source compatibility and renderer receipt. Specialized 04A.35/36 algorithms are excluded.'
    if name in RESERVED.get(f,set()):return 'LATER_BATCH_RESERVED','Master Plan '+('04A.36 detailed SPC channel/stage/limit/zone annotations' if f=='SpcControlChart' else '04A.35 detailed multiple-comparison CI/report/label/validation refinement')+'. Current specialized native behavior remains protected; 04A.34 basic coordinates/reference/axis foundation is consumed independently.'
    if f=='PsmLovePlot' and name in {'AddChart','AddProjectedChart','AddVariableLabels'}:
        return 'ALREADY_PORTABLE_REUSED','Actual call chain consumes accepted PsmLovePlotProjection before/after/masks/labels/MaximumX/Threshold unchanged; labels are mapped to one-based native points. Finite admission/threshold delegates to accepted-in-batch Common policy; Y reversal and deferred C25 defects unchanged. Detailed report geometry remains 04A.35.'
    if f=='ConvexHullPlot' and name=='lambda':return 'ALREADY_PORTABLE_REUSED','AddMarkerSeries Select extracts X/Y directly from accepted Core ConvexHullPlotGroup.Points, preserving order/cardinality; hull closure and source/group identity are Core-owned. No hull algorithm exists in this selector.'
    if f=='PolarPlot' and name=='CreateGrouped':return 'ALREADY_PORTABLE_REUSED','Constructor wrapper delegates directly to existing PolarPlotCore.CreateGrouped; no native transformation or new angular convention.'
    if name in HOST_REASONS:return 'HOST_ONLY',HOST_REASONS[name]
    if name.startswith('IsFinite'):
        return 'HOST_ONLY','Retained predicate for Windows appearance/measurement validation or Excel marshalling (font/line/size/alpha options), preserving original exception parameter names. Portable geometry has its own copied source admission at the explicit input boundary; no chart-data transformation is classified native solely for containing arithmetic.'
    base=next(p for p in partial if p['historical_member_id']==r['member_id'])
    if base['disposition']!='OPEN_BLOCKER':return base['disposition'],base['reason']+' Exact candidate: '+r['qualified_type']+'.'+name+'.'
    if f=='SankeyPlot' and name=='lambda':return 'HOST_ONLY','Native appearance Copy filters null override objects then invokes their native Copy; source category/flow ordering is prepared separately by consumed Common policy. No geometry in this selector.'
    raise AssertionError(('Unresolved member',f,name,kind,r['member_id']))

historical=[];receipts=[]
for r in old:
    hits=by[r['signature_key']];status,reason=classify(r,hits)
    assert hits or r['member_id'] in relocations,r['member_id']
    portable=[e for n in hits for e in reachable(n)]
    if r['member_id'] in relocations:
        rel=relocations[r['member_id']];portable.append(dict(site=rel['windows_call_site'],expression='EXPLICIT_RELOCATION',target=rel['successor'],target_path=rel['target_path'],target_span=rel['target_span']))
    # Admission/axis wrappers in nested appearance types use a fully qualified
    # module path; reachable records the exact token/source edge, not just a name.
    if status=='PORTED_AND_CONSUMED':assert portable,('Disconnected claimed port',r['family'],r['name'])
    receipt=dict(historical_member_id=r['member_id'],disposition=status,windows_members=[uid(n) for n in hits],portable_edges=list({json.dumps(e,sort_keys=True):e for e in portable}.values()),relocation=relocations.get(r['member_id']),native_boundary=reason if status in ['HOST_ONLY','LATER_BATCH_RESERVED'] else 'Native chart/shape/style/measurement/lifetime tail remains Windows; SOURCE_ONLY/NATIVE_NOT_RUN',evidence=['completion/member-source.json','completion/renderer-preservation.json','compatibility-casebook.json'],scientific='NOT_GOLDEN')
    receipts.append(receipt)
    row=dict(historical_member_id=r['member_id'],operation_id=r['operation_id'],family=r['family'],path=r['path'],name=r['name'],kind=r['kind'],disposition=status,reason=reason,target_type=TARGETS[r['family']][0],windows_consumption=';'.join(sorted({e['site'] for e in portable})),entry_sha256=r['entry_sha256'],final_sha256=pin(r['path'])['sha256'],current_ranges=';'.join(str(n['start'])+'-'+str(n['end']) for n in hits),current_match_count=len(hits),successor_member_ids=';'.join([relocations[r['member_id']]['successor']] if r['member_id'] in relocations else [uid(n) for n in hits]),mapping='EXPLICIT_RELOCATION' if r['member_id'] in relocations else 'CURRENT_HEADER_IDENTITY',fixture_authority='PRE_EDIT_LEGACY; ACCEPTED_CORE/SAME_CORE_WIRING where earlier engines reused; SOURCE_ONLY native; NOT_GOLDEN')
    historical.append(row)
assert len(historical)==len({r['historical_member_id'] for r in historical})==768
hby={r['historical_member_id']:r for r in historical}
current=[]
for r in stable:
    ids=r['historical_member_ids'].split(';');decisions=[hby[i] for i in ids]
    assert len({d['disposition'] for d in decisions})==1
    current.append(dict(r,disposition=decisions[0]['disposition'],reason=decisions[0]['reason'],preexisting_current_member_id=r['current_member_id'],final_successor_member_ids=';'.join(sorted({i for d in decisions for i in d['successor_member_ids'].split(';')})),final_current_ranges=';'.join(sorted({d['current_ranges'] for d in decisions})),source_sha256=pin(r['path'])['sha256'],identity_basis='Stable continuation-checkpoint candidate identity; final successors/spans recorded separately. Historical offsets never rewritten.'))
assert len(current)==len({r['preexisting_current_member_id'] for r in current})==764
csvout('operation-map-final.csv',historical);csvout('family-ownership-map.csv',current)
families=[]
for f in TARGETS:
    h=[r for r in historical if r['family']==f];c=[r for r in current if r['family']==f]
    families.append(dict(family=f,historical_candidates=len(h),current_distinct_candidate_members=len(c),historical_dispositions=dict(collections.Counter(r['disposition'] for r in h)),current_dispositions=dict(collections.Counter(r['disposition'] for r in c)),complete=True,actually_changed=any(r['entry_sha256']!=r['final_sha256'] for r in h),current_open_blockers=0,native='NATIVE_NOT_RUN',later_scope='Detailed agreement/comparison/Love 04A.35; survival/SPC annotations 04A.36; multivariate 04A.37; not completed by this foundation'))
out('family-coverage.json',dict(schema='04A34-family-coverage-v2',status='RECONCILED_PENDING_FINAL_VALIDATION',families=families,historical_candidate_count=768,current_distinct_candidate_count=764,open_blockers=0,historical_open_blockers=0,unmatched_historical=sum(not r['current_match_count'] for r in historical),explicit_relocations=len(relocations),native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN',identity_basis='764 pre-existing continuation-entry candidate identities, 768 unique historical IDs; final successors may be portable or multiple source nodes; no forced exact same-file matches. New portable methods are separate census.'))
save('member-consumption.json',dict(status='PASS_SOURCE_CROSSWALK',members=receipts,relocations=list(relocations.values()),new_portable_members=[{k:n[k] for k in ['path','key','name','kind','start','end','source_span_start','source_span_length','token_sha256']} for n in nodes if not n['path'].startswith('BESHstatNG/')],qualification='Lexical edges are audited against compiled real-adapter tests, not semantic/native execution. HOST_ONLY retains concrete native option semantics. No open candidate is silently discarded.'))
print('Reconciled768 historical IDs/764 preexisting current identities; zero OPEN;',len(relocations),'explicit relocations')
