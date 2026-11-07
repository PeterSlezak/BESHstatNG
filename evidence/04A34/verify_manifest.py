"""Independent read-only packet verifier. Imports no producer or build runner."""
from pathlib import Path
import hashlib,json,re
import csv,collections
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
SELF='evidence/04A34/publication-manifest.json'
SOURCES=['BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj', 'BESHStat.Core.Tests/src/Graphics/Completion34Cases.vb', 'BESHStat.Core.Tests/src/Graphics/CompletionPolicyCases34.vb', 'BESHStat.Core.Tests/src/Graphics/Graphics34CompletionBoundsTests.vb', 'BESHStat.Core.Tests/src/Graphics/Graphics34CompletionGeometryTests.vb', 'BESHStat.Core.Tests/src/Graphics/Graphics34Tests.vb', 'BESHStat.Core.Tests/src/Graphics/LegacyCompletionExtras34.vb', 'BESHStat.Core.Tests/src/Graphics/LegacyCompletionPolicies34.vb', 'BESHStat.Core.Tests/src/Graphics/LegacyReference34.vb', 'BESHStat.Core.Tests/src/Graphics/LegacySpcBasis34.vb', 'BESHStat.Core.Tests/src/Graphics/LegacySpecialty34.vb', 'BESHStat.Core/src/Graphics/AnimationFrameTimingCore.vb', 'BESHStat.Core/src/Graphics/BulletMeasuredGeometryCore.vb', 'BESHStat.Core/src/Graphics/CalibrationSeriesData.vb', 'BESHStat.Core/src/Graphics/ChartReferenceSeries.vb', 'BESHStat.Core/src/Graphics/ComparisonCoordinatesCore.vb', 'BESHStat.Core/src/Graphics/CumulativeStepSeriesData.vb', 'BESHStat.Core/src/Graphics/LadderLabelGeometryCore.vb', 'BESHStat.Core/src/Graphics/PolarMeasuredLayoutCore.vb', 'BESHStat.Core/src/Graphics/SankeyMeasuredGeometryCore.vb', 'BESHStat.Core/src/Graphics/SpcAxisBoundsCore.vb', 'BESHStat.Core/src/Graphics/SymmetrySeriesData.vb', 'BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj', 'BESHStat.Excel.Common.Tests/src/Graphics34CompletionLegendTests.vb', 'BESHStat.Excel.Common.Tests/src/Graphics34CompletionPolicyTests.vb', 'BESHStat.Excel.Common.Tests/src/Graphics34CompletionSeriesTests.vb', 'BESHStat.Excel.Common.Tests/src/Graphics34LoveTests.vb', 'BESHStat.Excel.Common/src/Analysis/AgreementSeriesPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/BulletChartPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/CdfChartPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/CdfSeriesPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/ComparisonSeriesPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/DumbbellChartPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/DumbbellSeriesPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/HullChartPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/LadderChartPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/LadderSeriesPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/PolarChartPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/PsmLoveSeriesPolicy.vb', 'BESHStat.Excel.Common/src/Analysis/SankeyChartPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/SpcChartPreparation34.vb', 'BESHStat.Excel.Common/src/Analysis/SpecialtyLegendPolicy34.vb', 'BESHStatNG.Test/BESHStatNG.Tests.vbproj', 'BESHStatNG.Test/src/Charts/CompletionPreparationCases34.vb', 'BESHStatNG.Test/src/Charts/CompletionSeriesCases34.vb', 'BESHStatNG.Test/src/Charts/Graphics34AdapterTests.vb', 'BESHStatNG.Test/src/Charts/Graphics34CompletionNativeStateTests.vb', 'BESHStatNG.Test/src/Charts/Graphics34CompletionPreparationTests.vb', 'BESHStatNG.Test/src/Charts/LegacyCompletionNativeState34.vb', 'BESHStatNG.Test/src/Charts/LegacyCompletionPreparation34.vb', 'BESHStatNG.Test/src/Charts/LegacyCompletionSeries34.vb', 'BESHStatNG.Test/src/Charts/LegacyCompletionShapes34.vb', 'BESHStatNG.Test/src/Charts/LegacyLayouts34.vb', 'BESHstatNG/src/Graphics/AgreementPlotExcel.vb', 'BESHstatNG/src/Graphics/BulletChartExcel.vb', 'BESHstatNG/src/Graphics/CalibrationPlot.vb', 'BESHstatNG/src/Graphics/ConvexHullPlot.vb', 'BESHstatNG/src/Graphics/CumulativeDistributionPlotExcel.vb', 'BESHstatNG/src/Graphics/DumbbellPlotExcel.vb', 'BESHstatNG/src/Graphics/GifAnimator.vb', 'BESHstatNG/src/Graphics/LadderPlotExcel.vb', 'BESHstatNG/src/Graphics/MultipleComparisonPlotExcel.vb', 'BESHstatNG/src/Graphics/PolarPlot.vb', 'BESHstatNG/src/Graphics/PsmLovePlotExcel.vb', 'BESHstatNG/src/Graphics/SankeyPlotExcel.vb', 'BESHstatNG/src/Graphics/SpcControlChartExcel.vb', 'BESHstatNG/src/Graphics/SymetryPlot.vb', 'docs/architecture/batch-04A-34-owner-review.md']

def expected_scope():
    return set(SOURCES+[p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts]+[SELF])
def file_pin(path):
    p=(ROOT/path).resolve()
    assert p.is_relative_to(ROOT.resolve()) and p.is_file(),'Missing/escaping payload'
    b=p.read_bytes()
    return dict(path=path,bytes=len(b),sha256=hashlib.sha256(b).hexdigest())
def validate_rows(rows,expected):
    assert isinstance(rows,list) and rows
    paths=[r['path'] for r in rows]
    assert all(isinstance(p,str) and p and '\\' not in p and ':' not in p and not p.startswith('/') and all(x not in ('','.','..') for x in p.split('/')) for p in paths),'Noncanonical path'
    assert len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Duplicate/case-ambiguous path'
    assert set(paths)==set(expected),'Missing/foreign entry'
    assert paths.count(SELF)==1,'Self entry required'
    for row in rows:
        assert set(row)=={'path','bytes','sha256'}
        if row['path']==SELF:
            assert row['bytes'] is None and row['sha256'] is None,'Recursive self entry'
        else:
            assert type(row['bytes']) is int and row['bytes']>=0
            assert isinstance(row['sha256'],str) and re.fullmatch('[0-9a-f]{64}',row['sha256'])
            assert row==file_pin(row['path']),'Payload size/hash mismatch: '+row['path']
def verify_manifest():
    v=json.loads((HERE/'publication-manifest.json').read_bytes())
    assert v['schema']=='04A34-publication-v1' and v['decision']=='READY_FOR_OWNER_04A34_ACCEPTANCE_REVIEW'
    validate_rows(v['files'],expected_scope())
    return len(v['files'])
def validate_lineage(value):
    assert value['status']=='PASS_SUCCESSOR_MAP'
    rows=value['pins'];assert len(rows)==278 and len({r['path'] for r in rows})==278
    prior=json.loads((ROOT/'evidence/04A33/source-transition-successor.json').read_bytes())
    old={r['path']:r for r in prior['pins']}
    entry={r['path']:r for r in json.loads((HERE/'entry-pins.json').read_bytes())['files']}
    for r in rows:
        p=r['path']
        assert r['original_04A28_sha256']==old[p]['original_04A28_sha256']
        assert r['accepted_04A33_sha256']==old[p]['final_04A33_sha256']
        assert r['merged_main_sha256']==entry[p]['sha256']
        assert r['final_04A34_sha256']==file_pin(p)['sha256']

def crosswalk_inputs():
    def csvrows(name):
        with (HERE/name).open(encoding='utf-8-sig') as f:return list(csv.DictReader(f))
    return (csvrows('operation-map-entry.csv'),csvrows('operation-map-final.csv'),csvrows('family-ownership-map.csv'),csvrows('completion/checkpoint/family-ownership-map.csv'),json.loads((HERE/'completion/member-source.json').read_bytes()),json.loads((HERE/'completion/member-consumption.json').read_bytes()))

def validate_crosswalk(entry,final,current,checkpoint,ast,consumption):
    assert len(entry)==len(final)==768
    assert len({r['member_id'] for r in entry})==768
    assert {r['member_id'] for r in entry}=={r['historical_member_id'] for r in final}
    assert len(current)==len(checkpoint)==764
    assert {r['current_member_id'] for r in checkpoint}=={r['preexisting_current_member_id'] for r in current}
    assert len({r['preexisting_current_member_id'] for r in current})==764
    def uid(n):return n['key']+'@'+str(n['source_span_start'])+':'+str(n['source_span_length'])
    live={uid(n):n for n in ast['current']}
    rows={r['historical_member_id']:r for r in consumption['members']}
    reloc={r['historical_member_id']:r for r in consumption['relocations']}
    assert len(rows)==768 and len(reloc)==7
    allowed={'PORTED_AND_CONSUMED','ALREADY_PORTABLE_REUSED','HOST_ONLY','LATER_BATCH_RESERVED'}
    e={r['member_id']:r for r in entry}
    for r in final:
        id=r['historical_member_id'];old=e[id];receipt=rows[id]
        assert r['disposition'] in allowed and r['reason'] and r['disposition']==receipt['disposition']
        assert r['entry_sha256']==old['entry_sha256']
        assert r['final_sha256']==file_pin(r['path'])['sha256']
        hits=[n for n in ast['current'] if n['key']==old['signature_key']]
        assert int(r['current_match_count'])==len(hits)
        if not hits:assert id in reloc and r['mapping']=='EXPLICIT_RELOCATION'
        assert all(t in live for t in r['successor_member_ids'].split(';'))
        if r['disposition']=='PORTED_AND_CONSUMED':assert receipt['portable_edges']
        for edge in receipt['portable_edges']:
            assert edge['site'] in live and edge['target'] in live
            assert live[edge['site']]['path'].startswith('BESHstatNG/')
            target=live[edge['target']]
            assert target['path']==edge['target_path'] and not target['path'].startswith('BESHstatNG/')
            assert [target['start'],target['end']]==edge['target_span']
            if edge['expression']=='EXPLICIT_RELOCATION':assert id in reloc and edge['target']==reloc[id]['successor']
            else:
                assert edge['expression'] in live[edge['site']]['tokens'].replace(' . ','.').replace(' ( ','(') or any(c['expression']==edge['expression'] for c in live[edge['site']]['calls']) or edge['expression'].startswith('New ')
                assert edge['expression'].startswith('New ') or target['qualified_type'].split('.')[-1]+'.'+target['name'] in edge['expression']
    for r in current:
        assert r['disposition'] in allowed
        assert all(t in live for t in r['final_successor_member_ids'].split(';'))
    # Specific old operator identities must be mapped, not forced to a header hit.
    ladder=[(i,r) for i,r in reloc.items() if 'c2396206bed850c5' in i]
    assert len(ladder)==2
    for id,r in ladder:
        target=live[r['successor']]
        assert target['enclosing_name']=='SpreadLabelPositions'
        assert target['path'].endswith('LadderLabelGeometryCore.vb')
        assert target['tokens']==('Function ( i ) values ( i )' if id.endswith('@30089:21') else 'Function ( i ) i')
        assert not r['same_file_header_match']
    love=next(r for i,r in reloc.items() if '53143a2ee7cb7aa2' in i)
    assert live[love['successor']]['enclosing_name']=='HasAnyFinite'
    assert 'Not Double . IsNaN' in live[love['successor']]['tokens'] and 'Not Double . IsInfinity' in live[love['successor']]['tokens']
    spc=next(r for i,r in reloc.items() if 'ca14220ee65e6934' in i)
    assert live[spc['successor']]['name']=='HasNaturalZero' and not spc['same_file_header_match']
    assert 'SpcAxisBoundsCore . FromExtents' in live[spc['windows_call_site']]['tokens']
    assert any(n['name']=='FromExtents' and 'HasNaturalZero' in n['tokens'] and n['path'].endswith('SpcAxisBoundsCore.vb') for n in live.values())
    assert len({r['family'] for r in final})==17
if __name__=='__main__':print('PASS independent local packet verifier;',verify_manifest(),'entries; null self; software review candidate only')
