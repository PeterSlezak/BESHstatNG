"""Independent read-only verifier: stdlib only, no manifest producer imports."""
from pathlib import Path
import json,csv,hashlib,collections,re
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
SELF='evidence/04A35/publication-manifest.json';READY='READY_FOR_OWNER_04A35_ACCEPTANCE_REVIEW'
SOURCES=['BESHStat.Core/src/Graphics/MultipleComparisonGeometry35.vb','BESHStat.Excel.Common/src/Analysis/ComparisonChartPolicy35.vb','BESHStat.Excel.Common/src/Analysis/AgreementPlotPolicy35.vb','BESHStat.Excel.Common/src/Analysis/PsmLoveChartSpec35.vb','BESHStat.Excel.Common/src/Analysis/PsmLovePlotProjection.vb','BESHstatNG/src/Graphics/MultipleComparisonPlotExcel.vb','BESHstatNG/src/Graphics/AgreementPlotExcel.vb','BESHstatNG/src/Graphics/PsmLovePlotExcel.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStat.Core.Tests/src/Graphics/Graphics35GeometryTests.vb','BESHStat.Excel.Common.Tests/src/Cases35.vb','BESHStat.Excel.Common.Tests/src/Graphics35Tests.vb','BESHStat.Excel.Common.Tests/src/Legacy35.vb','BESHStat.Excel.Common.Tests/src/LegacyLoveProjection35.vb','BESHStatNG.Test/src/Charts/Graphics35AdapterTests.vb','docs/architecture/batch-04A-35-owner-review.md']
def load(n):return json.loads((HERE/n).read_bytes())
def scope():return set(SOURCES+[p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts]+[SELF])
def pin(path):
    p=(ROOT/path).resolve();assert p.is_relative_to(ROOT.resolve()) and p.is_file()
    b=p.read_bytes();return dict(path=path,bytes=len(b),sha256=hashlib.sha256(b).hexdigest())
def validate_manifest(rows,expected):
    paths=[r['path'] for r in rows]
    assert len(paths)==len(set(paths))==len({p.casefold() for p in paths})
    assert set(paths)==set(expected) and paths.count(SELF)==1
    for r in rows:
        p=r['path'];assert isinstance(p,str) and p and '\\' not in p and ':' not in p and not p.startswith('/') and all(x not in ('','.','..') for x in p.split('/'))
        assert set(r)=={'path','bytes','sha256'}
        if p==SELF:assert r['bytes'] is r['sha256'] is None
        else:assert r==pin(p),'Payload changed: '+p
def ownership_inputs():
    with (HERE/'member-ownership.csv').open(encoding='utf-8-sig') as f:rows=list(csv.DictReader(f))
    return rows,load('final-members.json'),load('coverage.json')
def validate_ownership(rows,ast,coverage):
    assert len(rows)==34 and len({r['member_id'] for r in rows})==34
    assert {r['family'] for r in rows}=={'Agreement','Comparisons','Love'}
    assert coverage['open_blockers']==0
    allowed={'PORTED_AND_CONSUMED','ALREADY_PORTABLE_REUSED','HOST_ONLY','LATER_BATCH_RESERVED'}
    for r in rows:
        assert r['disposition'] in allowed and r['reason']
        if r['disposition']=='PORTED_AND_CONSUMED':assert r['target'] and r['call_site']
        if r['path'].startswith('BESHstatNG/'):
            old=next(n for n in ast['entry'] if n['key']==r['signature_key'])
            new=next(n for n in ast['current'] if n['key']==r['signature_key'])
            assert r['entry_token_sha256']==old['token_sha256'] and r['final_token_sha256']==new['token_sha256']
            if r['disposition']=='PORTED_AND_CONSUMED':
                tokens=new['tokens']
                required={('Comparisons','AddPlot'):['New MultipleComparisonGeometry35','prepared . PlusError','prepared . MinusError'],('Comparisons','BuildTitle'):['ComparisonChartPolicy35 . BuildTitle'],('Comparisons','ValidatePlotData'):['ComparisonChartPolicy35 . ValidatePlotData'],('Comparisons','AddComparisonLabels'):['ComparisonChartPolicy35 . LabelOrdinals','ComparisonChartPolicy35 . PointIndex','ComparisonChartPolicy35 . LabelAt'],('Comparisons','ConfigureAxes'):['ComparisonChartPolicy35 . ValueAxisMaximum'],('Agreement','AddPlot'):['AgreementPlotPolicy35 .'],('Love','AddProjectedChart'):['New PsmLoveChartSpec35','spec . LabelSeriesOrdinal','spec . IncludesChannel','spec . ReverseY'],('Love','AddThresholdSeries'):['PsmLoveChartSpec35 . ThresholdSeriesName'],('Love','AddVariableLabels'):['ComparisonChartPolicy35 . LabelAt','ComparisonChartPolicy35 . PointIndex']}
                assert (r['family'],r['member']) in required
                assert all(needle in tokens for needle in required[(r['family'],r['member'])])
    for old in ast['entry']:
        assert any(r['signature_key']==old['key'] for r in rows),'Missing original member identity'
    closed=[r for r in rows if r['reserved34_closed']=='True']
    assert {r['member'] for r in closed}=={'BuildTitle','ValidatePlotData','AddComparisonLabels'}
    assert all(r['disposition']=='PORTED_AND_CONSUMED' and r['accepted34_historical_ids'] for r in closed)
    assert len(coverage['reserved34_closed'])==3
def validate_gates(value):assert value==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED')
def verify():
    m=load('publication-manifest.json');assert m['decision']==READY
    validate_manifest(m['files'],scope());validate_ownership(*ownership_inputs());validate_gates(load('entry-authority.json')['gates'])
    v=load('validation.json');assert v['decision']==READY and not v['unclassified_managed'] and not v['unclassified_python']
    assert load('preservation.json')['status']=='PASS'
    assert load('privacy-check.json')['status']=='PASS'
    return len(m['files'])
if __name__=='__main__':print('PASS independent35 verifier;',verify(),'entries; null self; software scope only')
