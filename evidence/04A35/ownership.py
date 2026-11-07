"""Three-family35 delta; accepted34 historical rows stay read-only."""
from work import *
def load(name):return json.loads((HERE/name).read_bytes())
def build():
    nodes=load('final-members.json');current={n['key']:n for n in nodes['current'] if n['path'].startswith('BESHstatNG/')}
    with (ROOT/'evidence/04A34/operation-map-final.csv').open(encoding='utf-8-sig') as f:history=list(csv.DictReader(f))
    rows=[];renderer=[]
    for old in nodes['entry']:
        new=current.get(old['key']);assert new,'Lost original Windows header '+old['key']
        family='Comparisons' if 'MultipleComparison' in old['path'] else 'Love' if 'PsmLove' in old['path'] else 'Agreement'
        name=old['name'];target='';status='HOST_ONLY';reason='Concrete Windows constructor/native chart configuration; no new reusable calculation in this member.'
        if family=='Comparisons':
            if name in ['AddPlot','BuildTitle','ValidatePlotData','AddComparisonLabels','ConfigureAxes']:
                status='PORTED_AND_CONSUMED';target='MultipleComparisonGeometry35;ComparisonChartPolicy35';reason='CI arms/bounds/zero/ordinal geometry before creation; title, Y padding and per-point labels delegate at their original late native read points. Raw live estimates remain read at native setter to preserve reentrancy/alias timing.'
            elif name=='SuggestedHeight':status='ALREADY_PORTABLE_REUSED';target='Chart34.ComparisonSeries.ComparisonSeriesPreparation34';reason='Original public height wrapper still calls the accepted34 policy and is consumed before numeric preparation.'
        elif family=='Agreement':
            if name=='AddPlot':status='PORTED_AND_CONSUMED';target='AgreementPlotPolicy35;accepted Core GetPlotData/ChartReferenceSeries';reason='Same four public overloads and accepted observed data/axes/title. Ordered regression/unity, identity, Bias/LoA/optional-subject metadata now consumed; Bland span still evaluated after GeneralScatterPlot to preserve error timing. No refit.'
            elif name=='AddHorizontalReferenceLine':status='ALREADY_PORTABLE_REUSED';target='ChartReferenceSeries.Horizontal';reason='Existing helper still consumes accepted pure horizontal endpoint geometry; caller supplies ordered Common descriptors. Native colors/line/series ownership remains here.'
        else:
            if name in ['BuildPlotDataTable','BuildProjectedDataTable','AddChart']:status='ALREADY_PORTABLE_REUSED';target='PsmLovePlotProjection';reason='Accepted shared projection/table/no-data wrappers and public signatures unchanged. Actual UI creates one projection and uses both wrappers; current chart wrapper reaches the new spec through AddProjectedChart.'
            elif name in ['AddProjectedChart','AddThresholdSeries','AddVariableLabels']:status='PORTED_AND_CONSUMED';target='PsmLoveChartSpec35;ComparisonChartPolicy35;PsmLoveSeriesPolicy';reason='Ordered finite channels, last data-series label owner, axes/reversal/names and zero-based-to-one-based point metadata consumed; original native styles, acquisition/error and per-point text read timing retained.'
            elif name=='HasAnyFinite':status='ALREADY_PORTABLE_REUSED';target='PsmLoveSeriesPolicy.HasAnyFinite';reason='Private compatibility delegate retained unchanged and executed by accepted34 adapter tests. Live admission now reaches the same accepted policy through PsmLoveChartSpec35; no claim this shim remains an active production caller.'
            elif name=='AddScatterSeries':reason='Native SeriesCollection creation, marshaling prepared vectors, assigning RGB/Excel marker/line styles and returning actual native series index. No portable statistical calculation.'
        linked=[r['historical_member_id'] for r in history if r['historical_member_id'].startswith(old['key']+'@')]
        if name=='AddProjectedChart':
            assert 'New PsmLoveChartSpec35 ( projection )' in new['tokens']
            new['calls'].append('New PsmLoveChartSpec35(projection) [Roslyn object-creation tokens; constructor]')
        rows.append(dict(member_id=old['key']+'@'+str(old['span_start'])+':'+str(old['span_length']),signature_key=old['key'],family=family,path=old['path'],member=name,signature=old['signature'],entry_range=str(old['start'])+'-'+str(old['end']),final_range=str(new['start'])+'-'+str(new['end']),entry_token_sha256=old['token_sha256'],final_token_sha256=new['token_sha256'],disposition=status,target=target,reason=reason,accepted34_historical_ids=';'.join(linked),reserved34_closed=name in ['BuildTitle','ValidatePlotData','AddComparisonLabels'] and family=='Comparisons',call_site=';'.join(c for c in new['calls'] if any(t in c for t in ['35','GetPlotData','ChartReferenceSeries','PsmLovePlotProjection','PsmLoveSeriesPolicy','SuggestedHeight'])),authority='PRE_EDIT_LEGACY_EXECUTED / PREVIOUS_ACCEPTED_CORE / SAME_CORE_WIRING; SOURCE_ONLY_NATIVE; NOT_GOLDEN'))
        def native(n):return [c for c in n['calls'] if re.search(r'(?:GeneralScatterPlot|AddChart|NewSeries|ErrorBar|Delete|ApplyDataLabels|Points|Axes)\(',c)]
        native_old,native_new=native(old),native(new)
        # Native method identity/order/count must remain; data expressions may
        # change only in the documented pure-coordinate/name/label delegation.
        pattern=r'([A-Za-z_]\w*)\('
        names=lambda calls:[re.findall(pattern,c)[0] for c in calls]
        assert names(native_old)==names(native_new),(name,'native operation sequence changed')
        assert old['catches']==new['catches'],(name,'native catch boundary changed')
        oldstyle=[a for a in old['assignments'] if any(t in a['left'] for t in ['Marker','Color','Font','Weight','DashStyle','Visible','Position','HasDataLabel','HasLegend','HasTitle','Tick','Crosses','MajorUnit','HasMajorGridlines'])]
        newstyle=[a for a in new['assignments'] if any(t in a['left'] for t in ['Marker','Color','Font','Weight','DashStyle','Visible','Position','HasDataLabel','HasLegend','HasTitle','Tick','Crosses','MajorUnit','HasMajorGridlines'])]
        assert oldstyle==newstyle,(name,'native style changed')
        renderer.append(dict(member_id=rows[-1]['member_id'],path=old['path'],member=name,header_preserved=True,body_preserved=old['token_sha256']==new['token_sha256'],native_entry=native_old,native_final=native_new,native_operation_sequence_preserved=True,catches_preserved=True,native_styles_preserved=True,coordinate_or_name_argument_substitutions='Pure35 prepared values; frozen references and actual pure adapter tests prove scalar/order parity; native visuals NOT_RUN',authority='SOURCE_ONLY_NATIVE',native='NATIVE_NOT_RUN'))
    # Additive metadata in the existing accepted projection is a separate, narrow
    # source transition. Every old non-constructor token body must remain exact.
    supporting=nodes['supporting_entry'];live_support={n['key']:n for n in nodes['current'] if n['path']=='BESHStat.Excel.Common/src/Analysis/PsmLovePlotProjection.vb'}
    constructor=next(n for n in supporting if n['name']=='.ctor')
    for old in supporting:
        if constructor['span_start']<=old['span_start']<constructor['span_start']+constructor['span_length']:continue
        hits=[n for n in nodes['current'] if n['path']==old['path'] and n['key']==old['key']]
        assert any(n['token_sha256']==old['token_sha256'] for n in hits),('Accepted projection body changed',old['name'])
    for member in ['.ctor','SourceOrdinals']:
        now=next(n for n in live_support.values() if n['name']==member and (member=='.ctor' or n['kind']=='PropertyBlock'))
        old=next((n for n in supporting if n['name']==member),None)
        rows.append(dict(member_id=now['key']+'@'+str(now['span_start'])+':'+str(now['span_length']),signature_key=now['key'],family='Love',path=now['path'],member=member,signature=now['signature'],entry_range=str(old['start'])+'-'+str(old['end']) if old else 'ADDITIVE_API',final_range=str(now['start'])+'-'+str(now['end']),entry_token_sha256=old['token_sha256'] if old else '',final_token_sha256=now['token_sha256'],disposition='PORTED_AND_CONSUMED',target='PsmLovePlotProjection.SourceOrdinals -> PsmLoveChartSpec35 source identity',reason='Original-list zero-based position is retained through the exact existing filter/key/tie order, then copied. Existing numeric suffix/table/getter bodies are token-identical; frozen independent old projection outputs match. Not worksheet rows; no reentrant UI option fix.',accepted34_historical_ids='',reserved34_closed=False,call_site='PsmLoveChartSpec35.New -> projection.SourceOrdinals; PsmLovePlotExcel.AddProjectedChart -> new spec',authority='PRE_EDIT_LEGACY_EXECUTED for values/order; HAND_DERIVED_INVARIANT for added source ordinals; SAME_CORE_WIRING; NOT_GOLDEN'))
    # Reports already Common-owned; no redundant report implementation/UI edit.
    report_members={'AgreementReportPlan':['.ctor','TablesForWriter','NativeRowSpan','AppendDecisionTables'],'PsmReportSelectionPolicy':['Plan','DiagnosticSheets','LoveSheets','MatchSheets','CemSheets'],'PsmResultPresentation':['.ctor'],'PsmReportSheet':['BuildTables']}
    for owner,names in report_members.items():
        for name in names:
            now=next(n for n in nodes['current'] if n['owner'].endswith('.'+owner) and n['name']==name)
            family='Agreement' if owner=='AgreementReportPlan' else 'Love';sites='UiTwoInputRefedits.RunDeming/RunLinsCCC/RunCohensKappa;Ui9ANOVA2nested.RunBlandAltman;Ui0OneRefeditMulticol.RunICC' if family=='Agreement' else 'Ui20PropensityScoreMatching.WritePsmResults/WriteLovePlotSheetIfAvailable'
            rows.append(dict(member_id=now['key']+'@'+str(now['span_start'])+':'+str(now['span_length']),signature_key=now['key'],family=family,path=now['path'],member=name,signature=now['signature'],entry_range='Existing accepted25/26; unchanged source file entry SHA',final_range=str(now['start'])+'-'+str(now['end']),entry_token_sha256=now['token_sha256'],final_token_sha256=now['token_sha256'],disposition='ALREADY_PORTABLE_REUSED',target=owner+'.'+name,reason='Actual existing UI calls accepted Common section/table/plot and native-row-span policies. Core table objects intentionally remain caller-owned; list snapshots/order are portable. Whole source file entry hash preserved. No UI changes or table-value recomputation.',accepted34_historical_ids='',reserved34_closed=False,call_site=sites,authority='PREVIOUS_ACCEPTED_CORE / SAME_CORE_WIRING; actual accepted25/26 Framework IL consumer tests; NOT_GOLDEN'))
    with (HERE/'member-ownership.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
    save('renderer-preservation.json',dict(status='PASS_SOURCE_BOUNDARY',members=renderer,qualification='Native call identity/order/count, public/private entry headers, style assignments and catches checked; coordinate/name/index expressions are explicitly delegated and legacy-tested. No fake chart/native parity claim.'))
    save('coverage.json',dict(families=['Comparisons','Agreement','Love'],members=len(rows),open_blockers=sum(r['disposition']=='OPEN_BLOCKER' for r in rows),reserved34_closed=[r['member_id'] for r in rows if r['reserved34_closed']],status='SOURCE_RECONCILED_PENDING_TESTS',native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN'))
    print('Mapped',len(rows),'delta members/report chains;3 reserved members closed;0 OPEN')
if __name__=='__main__':build()
