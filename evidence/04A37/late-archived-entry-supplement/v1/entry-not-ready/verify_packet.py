"""Independent, read-only stdlib verifier. Imports no packet producer or prior verifier."""
from pathlib import Path
from collections import Counter
import csv
import hashlib
import json
import re
import os
import struct
import math

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
SELF = 'evidence/04A37/publication-manifest.json'
READY = 'READY_FOR_OWNER_04A37_ACCEPTANCE_REVIEW'
SOURCES = {
 'BESHStat.Core/src/Graphics/MultivariatePlotGeometry37.vb',
 'BESHStat.Excel.Common/src/Results/MultivariateChartPresentation37.vb',
 'BESHstatNG/src/Graphics/PcaPlotExcel.vb',
 'BESHstatNG/src/Graphics/FactorAnalysisPlotExcel.vb',
 'BESHstatNG/src/Graphics/ClusterAnalysisPlotExcel.vb',
 'BESHstatNG/src/Graphics/CorrespondenceAnalysisPlotExcel.vb',
 'BESHStat.Core.Tests/src/Graphics/Graphics37Cases.vb',
 'BESHStat.Core.Tests/src/Graphics/Graphics37GeometryTests.vb',
 'BESHStat.Core.Tests/src/Graphics/Graphics37Original.vb',
 'BESHStat.Core.Tests/src/Graphics/Graphics37OriginalSeries.vb',
 'BESHStat.Excel.Common.Tests/src/Graphics37CurrentPrefixes.vb',
 'BESHStat.Excel.Common.Tests/src/Graphics37PresentationTests.vb',
 'BESHStatNG.Test/src/Charts/Graphics37AdapterTests.vb',
 'BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj',
 'BESHStatNG.Test/BESHStatNG.Tests.vbproj',
 'docs/architecture/batch-04A-37-owner-review.md'}
GATES = dict(P='NO', N1='NO GO', N2='NO', C2='UNMET', C3='UNAPPROVED')


def load(name):
    return json.loads((HERE / name).read_bytes())


def digest(data):
    return hashlib.sha256(data).hexdigest()


def canonical(path):
    assert isinstance(path, str) and path and '\\' not in path and ':' not in path and not path.startswith('/')
    assert not any(ord(c) < 32 for c in path)
    assert all(p not in ['', '.', '..'] and not p.endswith((' ', '.')) and not re.match(r'^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)', p, re.I) for p in path.split('/'))


def pin(path, root=ROOT):
    canonical(path)
    p = (root / path).resolve()
    assert p.is_relative_to(root.resolve()) and p.is_file()
    b = p.read_bytes()
    return dict(path=path, bytes=len(b), sha256=digest(b))


def scope():
    return SOURCES | {p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and not set(p.parts) & {'bin','obj','__pycache__'}} | {SELF}


def validate_manifest(rows, expected, root=ROOT):
    assert isinstance(rows, list)
    paths = [r['path'] for r in rows]
    assert paths == sorted(paths) and len(paths) == len(set(paths)) == len({p.casefold() for p in paths})
    assert set(paths) == set(expected) and paths.count(SELF) == 1
    for r in rows:
        assert set(r) == {'path','bytes','sha256'}
        canonical(r['path'])
        assert not re.search(r'\.(dll|exe|pdb|xll|msi|trx|log|pyc)$',r['path'],re.I)
        if r['path'] == SELF:
            assert r['bytes'] is r['sha256'] is None
        else:
            assert type(r['bytes']) is int and r['bytes'] >= 0
            assert isinstance(r['sha256'],str) and re.fullmatch(r'[0-9a-f]{64}',r['sha256'])
            assert r == pin(r['path'],root), 'Modified/missing artifact: ' + r['path']


def validate_privacy(text):
    assert '\x00' not in text
    assert not re.search(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+',text,re.I)
    assert not re.search(r'S-1-5-\d+(?:-\d+)*|AKIA[0-9A-Z]{16}|-----BEGIN [A-Z ]*PRIVATE KEY-----',text)
    for name in [os.environ.get('COMPUTERNAME','!unset!'),os.environ.get('USERNAME','!unset!')]:
        assert not re.search(r'(?<![A-Za-z0-9])'+re.escape(name)+r'(?![A-Za-z0-9])',text,re.I)


def validate_gates(value):
    assert value == GATES


def validate_validation(value, identities):
    assert value['managed'] == dict(Debug=dict(Passed=20507,Failed=14),Release=dict(Passed=20507,Failed=14))
    assert value['new_managed_cases_per_configuration'] == 50
    assert value['new_managed_by_suite'] == dict(Core=11,Common=8,Legacy=31)
    assert value['node'] == dict(total=916,passed=916,failed=0)
    assert value['strict_profile_mkdocs'] == dict(strict=1,profile=1,docs=0)
    assert value['native'] == 'NOT_RUN' and value['scientific'] == 'NOT_GOLDEN' and value['unclassified_python'] == 0
    assert len(identities) == 20521
    assert len({(r['suite'],r['raw_identity_sha256'],r['raw_occurrence_ordinal']) for r in identities}) == len(identities)
    for config in ['debug','release']:
        new = [r for r in identities if r['merged_main_'+config] == 'NEW_04A37']
        assert len(new) == 50 and all(r['final_'+config] == 'Passed' and 'Graphics37' in r['class_name'] for r in new)
        assert all(r['merged_main_'+config] == r['final_'+config] for r in identities if r['merged_main_'+config] != 'NEW_04A37')
        assert Counter(r['final_'+config] for r in identities) == Counter(Passed=20507,Failed=14)
    assert all(r['removed'] == r['changed_outcomes'] == 0 for r in value['identity_deltas'])


def validate_ownership(rows, ast, coverage):
    original = {n['key']+'@'+str(n['span_start'])+':'+str(n['span_length']):n for n in ast['entry']}
    assert len(rows) == len(original) == coverage['current_original_members'] == 32
    assert {r['member_id'] for r in rows} == set(original) and len({r['member_id'] for r in rows}) == 32
    assert coverage['historical_members'] == 32 and coverage['in_scope_open'] == 0
    for r in rows:
        old=original[r['member_id']]
        assert r['member'] == old['name'] and r['path'] == old['path']
        expected='HOST_ONLY' if old['name'] in {'.ctor','AddPointLabels','ConfigureScatterAxes','DeleteLegendIfPresent','SetAxisTitles','WriteObjectTable'} else 'EXISTING_PORTABLE_REUSED' if old['name'] in {'Draw','CreateDendrogramChart'} else 'PORTED_AND_CONSUMED'
        assert r['disposition'] == expected
        assert r['disposition'] in {'PORTED_AND_CONSUMED','EXISTING_PORTABLE_REUSED','HOST_ONLY','RESERVED_NEXT_AUTHORIZED_BATCH'}
        assert r['entry_signature'] == original[r['member_id']]['signature']
        assert r['entry_token_sha256'] == original[r['member_id']]['token_sha256']
        assert r['reason'] and r['dependency'] and r['consuming_edge'] and r['tests'] and r['successor']
        if r['disposition'] == 'PORTED_AND_CONSUMED':
            assert r['dependency'] != 'Retained Windows renderer' and r['mapping'] in {'EXACT_HEADER','EXPLICIT_SUPERSESSION','PRIVATE_TYPED_ARGUMENT_CHANGE'}
        if r['mapping'] == 'EXPLICIT_SUPERSESSION':
            assert r['member'] in {'ResolveScoreLabels','ResolveGroupLabels','ColumnSumsOfSquares','PercentOfTotal','TotalVariance'}
            target=dict(ResolveScoreLabels='MultivariateChartPresentation37.ScoreLabels',ResolveGroupLabels='MultivariateChartPresentation37.NormalizeGroups',ColumnSumsOfSquares='MultivariatePlotGeometry37.FactorPercent',PercentOfTotal='MultivariatePlotGeometry37.PercentOfTotal',TotalVariance='MultivariatePlotGeometry37.TotalVariance')[r['member']]
            assert r['successor'].startswith(target) and r['dependency'].startswith(target)
        else:
            current=[n for n in ast['current'] if n['path']==old['path'] and n['owner']==old['owner'] and n['name']==old['name'] and n['kind']==old['kind']]
            assert len(current)==1
            n=current[0];assert r['successor']==n['key']+'@'+str(n['span_start'])+':'+str(n['span_length'])
            if old['signature'].startswith('Public'): assert old['header_tokens']==n['header_tokens']


def validate_history(history, ownership):
    assert len(history) == len({r['historical_member_id'] for r in history}) == 32
    assert {r['historical_member_id'] for r in history} == {r['historical_member_id'] for r in ownership}
    assert all(r['successor'] and r['disposition'] != 'OPEN' for r in history)


def code_only(text):
    # Drop comments and string literals before checking executable call edges.
    return '\n'.join(re.sub(r'"(?:[^"]|"")*"', '""',line).split("'",1)[0] for line in text.splitlines())


def validate_consumption(texts):
    pca = code_only(texts['Pca']); factor = code_only(texts['Factor']); ca = code_only(texts['CA']); cluster = code_only(texts['Cluster'])
    for code in [pca,factor]:
        assert 'MultivariatePlotGeometry37.Column' in code
        assert 'MultivariateChartPresentation37.PercentLabel' in code
        assert 'MultivariatePlotGeometry37.LoadingVector' in code
    assert 'MultivariatePlotGeometry37.Biplot' in pca and 'MultivariateChartPresentation37.ScoreSeries' in pca
    assert 'MultivariateChartPresentation37.ScoreLabels' in pca and 'MultivariateChartPresentation37.NormalizeGroups' in pca
    assert 'MultivariatePlotGeometry37.FactorPercent' in factor and 'MultivariatePlotGeometry37.InitialFactorPercent' in factor
    assert 'MultivariatePlotGeometry37.CorrespondenceAxes' in ca and 'MultivariatePlotGeometry37.Contribution' in ca
    assert ca.count('MultivariateChartPresentation37.CorrespondenceSeries') == 2
    assert 'New DendrogramChartGeometry37' in cluster and 'New DendrogramChartPresentation37' in cluster
    assert 'geometry.Bounds()' in cluster
    for n in ['BaseSeries','Clusters','CutSeries','LeafSeries']: assert 'presentation.'+n+'()' in cluster
    for code in [pca,factor,ca,cluster]:
        assert not re.search(r'\bMath\.|\.Min\(|\.Max\(|ColumnSumsOfSquares|PercentOfTotal|TotalVariance|Matrix.GetColumnFrom2Darray',code)
    assert cluster.index('If layout.PolylineX Is Nothing') < cluster.index('If figure Is Nothing Then')
    assert cluster.index('geometry.Bounds()') > cluster.index('presentation.LeafSeries()')


def validate_comparison(value):
    assert value['status'] == 'PASS_EXACT_SOFTWARE_COMPATIBILITY' and value['scientific'] == 'NOT_GOLDEN'
    assert value['rows_per_configuration'] == 6912 and len(value['comparisons']) == 4
    for c in ['Debug','Release']: assert sum(r['rows'] for r in value['comparisons'] if r['configuration'] == c) == 6912
    for row in value['comparisons']:
        assert row['mismatches'] == 0 and row['pre']['sha256'] == row['post']['sha256']
        assert pin(row['pre']['path']) == row['pre'] and pin(row['post']['path']) == row['post']
        validate_capture(ROOT / row['post']['path'], row['family'])
    receipt = load('preedit/receipt.json')
    assert receipt['source_commit'] == 'a0680f546c5130fa5143c0b0c7136f852d650cc5' and receipt['production_diff'] == 'EMPTY'
    for row in receipt['files']: assert pin(row['path']) == row
    assert pin('evidence/04A37/preedit/capture-Debug.tsv')['sha256'] == '8e3f05bf83e270f9b02ee11f16c46e8322a4dd83cea20c463714fd809f82a745'
    assert pin('evidence/04A37/preedit/capture-Release.tsv')['sha256'] == '8e3f05bf83e270f9b02ee11f16c46e8322a4dd83cea20c463714fd809f82a745'
    assert pin('evidence/04A37/preedit/series.tsv')['sha256'] == '198a692567f8d79a48d6e6420ce7e68d54a430722019d23c6165dea265e0bdc7'


def validate_capture(path, family):
    with path.open(encoding='utf-8-sig',newline='') as f: rows=list(csv.DictReader(f,delimiter='\t'))
    if family == 'plot-prefixes':
        expected=[]
        for culture in ['', 'sk-SK']:
            for i in range(128):
                for name in ['PcaLoadingPlot2D','PcaLoadingPlot3D','PcaScreePlot','PcaScorePlot2D','PcaScorePlot3D','PcaBiplot','FactorLoadingPlot2D','FactorLoadingPlot3D','FactorScreePlot']:
                    expected.append((culture,name,str(i)))
                for role in range(2):
                    for name in ['CaContribution','CaPoints','CaLabels']: expected.append((culture,name,str(i*2+role)))
                for name in ['CaBounds','CaCaptions','DendrogramBounds','DendrogramTitles','Captions','Groups']:expected.append((culture,name,str(i)))
        actual=[(r['culture'],r['operation'],r['case']) for r in rows]
        assert actual == expected and len(actual) == len(set(actual)) == 5376
    else:
        assert family == 'dendrogram-series'
        expected=[(c,str(s),str(i),str(m)) for c in ['', 'sk-SK'] for s in range(2) for i in range(128) for m in range(3)]
        actual=[(r['culture'],r['source'],r['case'],r['labelMode']) for r in rows]
        assert actual == expected and len(actual) == len(set(actual)) == 1536


def validate_ready(decision, readiness, validation):
    if decision == READY:
        assert readiness['status'] == READY and not readiness['blockers'] and readiness['in_scope_open'] == 0
        assert validation['unclassified_python'] == 0 and readiness['manifest_verified_required'] is True
        assert required_preedit_coverage()['eligible'], 'Mandatory pre-edit option coverage is incomplete'
    else:
        assert decision == readiness['status'] == 'NOT_READY_04A37' and readiness['blockers']
    validate_gates(readiness['gates'])


def required_preedit_coverage():
    with (HERE/'preedit/capture-Debug.tsv').open(encoding='utf-8-sig',newline='') as f:
        rows=[r for r in csv.DictReader(f,delimiter='\t') if r['operation']=='PcaBiplot']
    scales={};negative_range={c:0 for c in ['', 'sk-SK']};eigen_edges={k:0 for k in ['zero','negative','NaN','positive_infinity']}
    for row in rows:
        scale=struct.unpack('>d',bytes.fromhex(row['input'].split('|')[1][1:]))[0]
        key='NaN' if math.isnan(scale) else str(scale)
        value=scales.setdefault(key,dict(rows=0,guard_returns=0,geometries=0,exceptions=0,geometry_by_culture={'':0,'sk-SK':0}))
        value['rows']+=1
        value['guard_returns']+=row['error']=='None' and row['output']=='N'
        geometry=row['error']=='None' and row['output']!='N'
        value['geometries']+=geometry
        value['geometry_by_culture'][row['culture']]+=geometry
        value['exceptions']+=row['error']!='None'
        if scale<0 and row['error'].startswith('System.ArgumentException::'):
            negative_range[row['culture']]+=1
        # These original case IDs are pinned by frozen Cases37.vb. Only finite,
        # in-range scales exercising the numerical prefix count as an eigen edge.
        edge={6:'zero',7:'negative',8:'NaN',9:'positive_infinity'}.get(int(row['case'])%32)
        if edge and math.isfinite(scale) and 0<=scale<=1 and int(row['case'])%4>=2:
            eigen_edges[edge]+=1
    required_modes={k:all(scales.get(k,{}).get('geometry_by_culture',{}).get(c,0)>0 for c in ['', 'sk-SK']) for k in ['0.0','0.5','1.0']}
    eligible=all(required_modes.values()) and all(negative_range.values()) and all(eigen_edges.values())
    return dict(eligible=eligible,scales=scales,required_metric_modes=required_modes,negative_scale_range_rejections=negative_range,finite_valid_scale_eigen_edge_executions=eigen_edges,
        cause='Original scale(id) uses id Mod8 while retained components use id Mod4. Required GH/SQ cases and several eigen/range cases are stopped by dimensional guards or paired with a nonfinite/out-of-range scale.')


def verify():
    manifest = load('publication-manifest.json')
    validate_manifest(manifest['files'],scope())
    for row in manifest['files']:
        if row['path'] != SELF: validate_privacy((ROOT / row['path']).read_text(encoding='utf-8-sig'))
    validate_gates(load('entry-authority.json')['gates'])
    with (HERE / 'managed-test-identities.csv').open(encoding='utf-8-sig') as f: identities=list(csv.DictReader(f))
    validation=load('validation.json'); validate_validation(validation,identities)
    with (HERE / 'member-ownership.csv').open(encoding='utf-8-sig') as f: ownership=list(csv.DictReader(f))
    validate_ownership(ownership,load('final-members.json'),load('coverage.json'))
    with (HERE / 'historical-crosswalk.csv').open(encoding='utf-8-sig') as f: history=list(csv.DictReader(f))
    validate_history(history,ownership)
    validate_consumption({k:(ROOT/p).read_text(encoding='utf-8-sig') for k,p in dict(Pca='BESHstatNG/src/Graphics/PcaPlotExcel.vb',Factor='BESHstatNG/src/Graphics/FactorAnalysisPlotExcel.vb',CA='BESHstatNG/src/Graphics/CorrespondenceAnalysisPlotExcel.vb',Cluster='BESHstatNG/src/Graphics/ClusterAnalysisPlotExcel.vb').items()})
    for p in ['BESHStat.Core/src/Graphics/MultivariatePlotGeometry37.vb','BESHStat.Excel.Common/src/Results/MultivariateChartPresentation37.vb']:
        text=(ROOT/p).read_text(encoding='utf-8-sig'); assert 'Option Strict On' in text
        assert not re.search(r'Imports.*(?:Office|ExcelDna|Windows.Forms|System.Drawing|Microsoft.VisualBasic|System.IO)',text,re.I)
    validate_comparison(load('compatibility-comparison.json'))
    assert load('preservation.json')['status'] == 'PASS_EXACT_ENTRY_BYTES'
    assert load('historic-census-metadata.json')['status'] == 'MATCH_ACCEPTED_METADATA'
    pins=load('lineage-check.json')['pins']; assert len(pins) == 278
    assert all(pin(r['path'])['sha256'] == r['final37_sha256'] for r in pins)
    captures=load('mixed-model-captures-check.json')['files']; assert len(captures) == 100
    assert all(pin(r['path']) == r for r in captures)
    transitions=load('source-transitions.json')['files']; assert {r['path'] for r in transitions} == SOURCES
    assert all(all(pin(r['path'])[k] == r[k] for k in ['path','bytes','sha256']) for r in transitions)
    builds=load('build-source-pins.json')['configurations']
    assert {r['configuration'] for r in builds} == {'Debug','Release'}
    for build in builds:
        assert build['build_source_unchanged'] and build['managed_source_unchanged'] and not any(build['build_exits'])
        assert {r['path'] for r in build['approved_source_files']} == SOURCES-{'docs/architecture/batch-04A-37-owner-review.md'}
        assert all(pin(r['path'])['sha256'] == r['sha256'] for r in build['approved_source_files'])
    for row in load('entry-authority.json')['owner_documents_excluded']:
        assert pin(row['path']) == row and row['path'] not in scope()
    state=load('git-final.json'); assert state['index'] == 'EMPTY' and state['full_diff_check_exit'] == 0
    assert state['head'] == state['main'] == state['merge_base'] == 'a0680f546c5130fa5143c0b0c7136f852d650cc5'
    assert state['branch'] == 'codex/04A-37' and state['next_batch'] == '04A38_NOT_STARTED'
    validate_ready(manifest['decision'],load('readiness.json'),validation)
    coverage=load('preedit-coverage-audit.json')
    assert coverage['observed']==required_preedit_coverage()
    if manifest['decision'] == READY:
        audits=load('audit-results.json')
        assert audits['status'] == 'PASS' and audits['total'] == audits['passed'] and audits['failed'] == audits['errors'] == 0
    return len(manifest['files'])


if __name__ == '__main__':
    print('PASS independent37 verifier;',verify(),'entries;',load('readiness.json')['status'],'; NOT_GOLDEN')
