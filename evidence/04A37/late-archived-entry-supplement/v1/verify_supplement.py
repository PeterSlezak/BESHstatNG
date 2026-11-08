"""Independent read-only Route A reader; stdlib only, imports no capture/packet producer.

GEOMETRY means the recorded pure preparation reached its final source boundary.
It never asserts successful COM rendering. Original and current run in separate
processes against different Core assemblies; all returned/partial data and errors
are compared, including negative cases. This is NOT_GOLDEN compatibility evidence.
"""
from pathlib import Path
from collections import Counter, defaultdict
from datetime import datetime
import csv
import hashlib
import io
import itertools
import json
import re

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
BASE = 'a0680f546c5130fa5143c0b0c7136f852d650cc5'
PROVENANCE = 'CAPTURED_AFTER_INITIAL_IMPLEMENTATION_FROM_PINNED_ARCHIVED_ENTRY'
QUOTE = "I'm comfortable authorizing the late archived-entry supplement for 04A.37"
GATES = dict(P='NO', N1='NO GO', N2='NO', C2='UNMET', C3='UNAPPROVED')
CULTURES = ['', 'sk-SK']
SCALES = ['0', '0.25', '0.5', '0.75', '1']
EIGEN = ['positive_unequal', 'zero', 'negative', 'NaN', '+Inf', 'missing', 'too_short']
STRUCTURES = ['valid','row_factors_missing','column_factors_missing','row_short_columns','column_short_columns','row_labels_missing','column_labels_missing','row_labels_short','column_labels_short','variable_labels_short','percent_missing','percent_short','row_contribution_missing','column_contribution_missing','contribution_short_columns','zero_contributors']
TRACES = ['positive','zero','negative','NaN','+Inf','missing']
SHAPES = ['valid','pattern_missing','structure_missing','pattern_short_columns','structure_short_rows','structure_short_columns']
ADAPTER_HASHES = {
 'Pca': '3a156c106a3e969957b31b253e7ac1196a92fdfee08cddcb76ea4fcb9486f26b',
 'FactorAnalysis': '8164bad9ff52f224a87aa2ede85c51d37e58482ae95745859a5aa81de2f09c9e',
 'CorrespondenceAnalysis': '04790b798dfeb283bbd4360efe4cd87789d1d05df436400490e10469b843ef2f',
 'ClusterAnalysis': 'e7f450f123f7e84ee2fd45ee45fc4395f78278b6777fd80c9aea419cda27da7f'}


def digest(b): return hashlib.sha256(b).hexdigest()
def load(p): return json.loads(Path(p).read_bytes())
def pin(p, root=ROOT):
    p = Path(p).resolve(); assert p.is_relative_to(root.resolve())
    b = p.read_bytes()
    return dict(path=p.relative_to(root).as_posix(), bytes=len(b), sha256=digest(b))
def check_pin(row, root=ROOT):
    assert isinstance(row['path'], str) and '\\' not in row['path']
    assert not any(x in ['', '.', '..'] for x in row['path'].split('/'))
    assert row == pin(root / row['path'], root), 'Supplement pin changed: '+row['path']
def stamp(s): return datetime.fromisoformat(re.sub(r'(\.\d{6})\d+', r'\1', s).replace('Z', '+00:00'))


def approval(a):
    assert a['owner_quote'] == QUOTE and a['owner_decision_date'] == '2026-11-08'
    assert a['route'] == 'A' and a['batch'] == '04A.37'
    assert a['capture_provenance'] == PROVENANCE and a['gates'] == GATES
    assert 'late-executed archived-entry original-source' in a['exception']
    assert {'RouteB','statistical/renderer behavior changes','native/R/manual execution','test weakening or historical rebaseline','scientific-golden/native release','commit/merge/push/publication/install/sign/deploy','04A38'} <= set(a['excludes'])


def product(rows, cohort, keys, axes):
    selected = [r for r in rows if r['cohort'] == cohort]
    actual = [tuple(r[k] for k in keys) for r in selected]
    expected = set(itertools.product(*axes))
    assert len(actual) == len(set(actual)) and set(actual) == expected, 'Missing/duplicate Cartesian cell: '+cohort


def inventory(cases, extension=False):
    assert len(cases) == (3584 if extension else 18466)
    ids=[]
    for row in cases:
        assert row['expected_stage'] == 'PUBLIC_GUARD_OR_PRE_CHART_THEN_RECORDED_LATE_NATIVE_BOUNDARY'
        original={k:v for k,v in row.items() if k not in ['case_id','expected_stage']}
        key=row['family']+'.'+row['operation']+'.'+digest(json.dumps(original,sort_keys=True,separators=(',',':')).encode())[:24]
        assert row['case_id'] == key and row['culture'] in CULTURES
        ids.append(key)
    assert len(ids)==len(set(ids))
    if extension:
        assert {r['cohort'] for r in cases} == {'explicit_width_and_custom_identity'}
        for op in ['ScorePlot2D','ScorePlot3D','Biplot']:
            part=[r for r in cases if r['operation']==op]
            product(part,'explicit_width_and_custom_identity', ['culture','components','row_ids','row_labels','groups','scale'],
                    [CULTURES,range(4),['known','missing','short','duplicate'],['none','blank','mixed','custom_duplicates'],['none','case_width','short','long'],SCALES if op=='Biplot' else ['1']])
            assert all(r['observations']==7 and r['variables']==4 and r['matrix']=='valid' and r['eigen']=='positive_unequal' for r in part)
        return
    product(cases,'biplot_full_cross_product', ['culture','components','observations','variables','scale','eigen'],
            [CULTURES,range(4),[0,1,2,4],[1,2,4],SCALES+['-0.1','1.1','NaN','+Inf','-Inf'],EIGEN])
    product(cases,'biplot_ungrouped',['culture','components','scale','eigen'],[CULTURES,[2,3],SCALES,EIGEN])
    product(cases,'score_identity_full_cross_product',['culture','operation','components','row_ids','row_labels','groups'],
            [CULTURES,['ScorePlot2D','ScorePlot3D'],range(4),['known','missing','short','duplicate'],['none','blank','mixed','short','long'],['none','mixed','case_width','short','long']])
    product(cases,'loading_shapes_labels',['culture','operation','components','variables','variable_labels','matrix'],
            [CULTURES,['LoadingPlot2D','LoadingPlot3D'],range(4),[1,2,4],['mixed','missing','short'],['valid','missing_loadings','short_columns']])
    product(cases,'scree_cross_product',['culture','components','observations','variables','eigen'],[CULTURES,range(4),[0,1,2,4],[1,2,4],EIGEN])
    product(cases,'biplot_shape_edges',['culture','components','observations','variables','scale','matrix'],
            [CULTURES,[2,3],[1,4],[2,4],SCALES,['missing_scores','missing_loadings','short_columns','short_rows','signed_zero','constant']])
    product(cases,'factor_full_cross_product',['culture','operation','factors','trace','initial_eigen','matrices'],
            [CULTURES,['ScreePlot','LoadingPlot2D','LoadingPlot3D'],range(5),TRACES,['positive','missing','empty','nonfinite'],SHAPES])
    product(cases,'factor_labels_profiles',['culture','operation','factors','trace','profile','variable_labels'],
            [CULTURES,['LoadingPlot2D','LoadingPlot3D'],[2,3,4],TRACES,['orthogonal_signs','oblique_distinct_signs'],['mixed','missing','short']])
    product(cases,'entry_engine_fitted_rotation',['culture','operation','factors','rotation'],
            [CULTURES,['ScreePlot','LoadingPlot2D','LoadingPlot3D'],[2,3],['None','Varimax','Promax']])
    ca=[r for r in cases if r['cohort']=='ca_mca_full_cross_product']
    product([r for r in ca if r['operation']=='CorrespondencePlot'],'ca_mca_full_cross_product',['culture','multiple','factors','structures'],[CULTURES,[False,True],range(5),STRUCTURES])
    product([r for r in ca if r['operation']=='ContributionPlot'],'ca_mca_full_cross_product',['culture','multiple','factors','structures','axis','row_side'],[CULTURES,[False,True],range(5),STRUCTURES,[-1,0,1,2,4],[False,True]])
    product(cases,'ca_empty_singleton_axes',['culture','multiple','factors','rows','columns'],[CULTURES,[False,True],[2,3,4],[0,1,3],[0,1,4]])
    product(cases,'null_guards',['culture','family'],[CULTURES,['PCA','FACTOR','CA']])
    assert set(r['cohort'] for r in cases)=={'biplot_full_cross_product','biplot_ungrouped','score_identity_full_cross_product','loading_shapes_labels','scree_cross_product','biplot_shape_edges','factor_full_cross_product','factor_labels_profiles','entry_engine_fitted_rotation','ca_mca_full_cross_product','ca_empty_singleton_axes','null_guards'}


def comparison(cases, reference, current):
    # Bytes enforce line order/encoding/complete error strings, array schema and bits.
    assert reference == current, 'Original/current bytes differ; no exception filtering permitted'
    rows=list(csv.DictReader(io.StringIO(reference.decode('utf-8')),delimiter='\t'))
    assert [r['case_id'] for r in rows] == [r['case_id'] for r in cases], 'Missing/reordered/foreign case'
    assert len(rows)==len({r['case_id'] for r in rows})
    for r in rows:
        assert set(r)=={'case_id','input_sha256','dispatch','phase','prefix_completed','usable_numeric_prefix','result','error'}
        assert re.fullmatch('[0-9a-f]{64}',r['input_sha256'])
        assert r['dispatch'] in ['GUARD','GEOMETRY','EXCEPTION']
        assert r['prefix_completed'] in ['true','false'] and r['usable_numeric_prefix'] in ['true','false']
        if r['dispatch']=='GUARD':
            assert r['result']=='System.Object[0:0]{}' and r['error']=='None' and r['prefix_completed']=='false'
        elif r['dispatch']=='EXCEPTION':
            assert r['error'].startswith('System.') and r['error']!='None'
        else:
            assert r['prefix_completed']=='true' and r['error']=='None' and r['result']!='System.Object[0:0]{}'
    return rows


def reachability(cases, rows, extension=False):
    pairs=list(zip(cases,rows)); witnesses=[]
    def require(name, predicate, dispatch='GEOMETRY', count=None, error=None):
        found=[(c,r) for c,r in pairs if predicate(c)]
        assert found and (count is None or len(found)==count), 'No complete witness set: '+name
        assert all(r['dispatch']==dispatch for c,r in found), 'Unreachable required geometry: '+name
        if dispatch=='GEOMETRY': assert all(r['prefix_completed']=='true' for c,r in found)
        if error: assert all(r['error'].startswith(error) for c,r in found)
        witnesses.append(dict(name=name,count=len(found),dispatch=dispatch,case_ids=[c['case_id'] for c,r in found]))
    if extension:
        for culture,op in itertools.product(CULTURES,['ScorePlot2D','ScorePlot3D','Biplot']):
            require('custom-label-missing-rowids/'+culture+'/'+op,lambda c:c['culture']==culture and c['operation']==op and c['components']==3 and c['row_ids']=='missing' and c['row_labels']=='custom_duplicates' and c['groups'] in ['none','case_width'],count=10 if op=='Biplot' else 2)
        # The compiled fixture explicitly contains U+FF22; output stores strings UTF8/base64.
        import base64
        encoded='S'+base64.b64encode('\uff22'.encode()).decode()
        actual=[r for c,r in pairs if c['groups']=='case_width' and r['dispatch']=='GEOMETRY']
        assert actual and all(encoded in r['result'] for r in actual)
        return witnesses
    for culture,k,scale in itertools.product(CULTURES,[2,3],SCALES):
        anchor=lambda c:c['cohort']=='biplot_full_cross_product' and c['culture']==culture and c['components']==k and c['observations']==4 and c['variables']==4 and c['scale']==scale
        require('biplot-finite/'+culture+'/'+str(k)+'/'+scale,lambda c:anchor(c) and c['eigen']=='positive_unequal',count=1)
        for eigen in EIGEN:
            require('biplot-eigen/'+culture+'/'+str(k)+'/'+scale+'/'+eigen,lambda c:anchor(c) and c['eigen']==eigen,
                    dispatch='EXCEPTION' if eigen in ['missing','too_short'] else 'GEOMETRY',count=1)
    for culture,k,scale in itertools.product(CULTURES,[2,3],['-0.1','1.1','+Inf','-Inf']):
        require('biplot-range/'+culture+'/'+str(k)+'/'+scale,lambda c:c['cohort']=='biplot_full_cross_product' and c['culture']==culture and c['components']==k and c['observations']==4 and c['variables']==4 and c['scale']==scale and c['eigen']=='positive_unequal',dispatch='EXCEPTION',count=1,error='System.ArgumentException:')
    for culture,op,k,trace in itertools.product(CULTURES,['LoadingPlot2D','LoadingPlot3D'],[2,3,4],TRACES[:-1]):
        if op=='LoadingPlot3D' and k<3: continue
        require('factor-trace/'+culture+'/'+op+'/'+str(k)+'/'+trace,lambda c:c['cohort']=='factor_full_cross_product' and c['culture']==culture and c['operation']==op and c['factors']==k and c['trace']==trace and c['matrices']=='valid' and c['initial_eigen']=='positive',count=1)
    for culture,multiple,k in itertools.product(CULTURES,[False,True],[2,3,4]):
        require('ca-map/'+culture+'/'+str(multiple)+'/'+str(k),lambda c:c['cohort']=='ca_mca_full_cross_product' and c['culture']==culture and c['operation']=='CorrespondencePlot' and c['multiple']==multiple and c['factors']==k and c['structures']=='valid',count=1)
    import base64
    column='S'+base64.b64encode(b'Columns').decode(); row='S'+base64.b64encode(b'Rows').decode()
    for c,r in pairs:
        if c['family']=='CA' and c['operation']=='CorrespondencePlot' and r['dispatch']=='GEOMETRY':
            assert column in r['result']
            assert (row not in r['result']) if c['multiple'] else (r['result'].index(column)<r['result'].index(row))
    return witnesses


def source_authority(root=ROOT, here=HERE):
    approval(load(here/'supplemental-approval.json'))
    auth=load(here/'source-authenticity.json')
    assert auth['entry_commit']==BASE and auth['chronology']==PROVENANCE and auth['status']=='PASS'
    assert len(auth['adapters'])==4
    for name,expected in ADAPTER_HASHES.items():
        b=(here/'archived-source'/ (name+'PlotExcel.vb.txt')).read_bytes()
        assert digest(b)==expected and b==(root/'evidence/04A37/preedit'/ (name+'PlotExcel.vb.txt')).read_bytes()
        a=next(r for r in auth['adapters'] if r['path'].endswith('/'+name+'PlotExcel.vb'))
        assert a['sha256']==expected and a['commit']==BASE and a['matches_original_frozen_snapshot']
        assert digest(b.replace(b'\r\n',b'\n'))==a['git_blob_sha256']
        check_pin(a['archived'],root)
    entry={r['path']:r for r in load(root/'evidence/04A37/entry-tracked-pins.json')['files']}
    for r in auth['unchanged_helper_sources']:
        b=(root/r['path']).read_bytes()
        assert digest(b)==entry[r['path']]['sha256']==r['entry_raw_sha256']==r['live_sha256']
        assert digest(b.replace(b'\r\n',b'\n'))==r['sha256']
    for key in ['original_prefix','current_prefix','extraction_authority']: check_pin(auth[key],root)
    assert (here/'archived-source/Original37.vb').read_bytes()==(root/'evidence/04A37/preedit/Original37.vb').read_bytes()
    assert (here/'current-source/Current37.vb').read_bytes()==(root/'BESHStat.Excel.Common.Tests/src/Graphics37CurrentPrefixes.vb').read_bytes()
    sources={name:(here/'archived-source'/ (name+'PlotExcel.vb.txt')).read_text(encoding='utf-8-sig') for name in ADAPTER_HASHES}
    for binding in load(here/'late-expression-bindings.json')['bindings']:
        expr=binding['expression']; assert digest(expr.encode())==binding['sha256']
        assert expr in sources[binding['family']], 'Late expression not in original adapter'
    slices=load(root/'evidence/04A37/preedit/source-slices.json')
    for r in slices:
        assert digest(r['original'].encode())==r['original_sha256']
        if r['operation'] not in ['CaBounds','CaContribution','Captions']:
            assert r['original'] in (root/'evidence/04A37/preedit'/ (r['source']+'.txt')).read_text(encoding='utf-8-sig')
    # Explicit wrapper substitutions in frozen CA slices, never new statistical logic.
    for role in ['CaBounds','CaContribution']:
        part=next(r['original'] for r in slices if r['operation']==role)
        part=part.replace('ChartAxisScaleResult = ChartAxisScalingCore.Compute','CHARTscale = ChartScaling').replace('Then Return Nothing','Then Exit Sub')
        assert all(line.strip() in sources['CorrespondenceAnalysis'] for line in part.splitlines() if line.strip())


def preserved_packet(root=ROOT,here=HERE):
    prior=load(here/'entry-packet-pins.json')
    assert len(prior['files'])==120
    snapshots={r['original']:r['snapshot'] for r in prior['snapshots']}
    old=load(here/'entry-not-ready/publication-manifest.json')
    assert old['files']==prior['files'] and old['decision']=='NOT_READY_04A37'
    assert digest((here/'entry-not-ready/publication-manifest.json').read_bytes())==prior['manifest']['sha256']
    for r in prior['files']:
        if r['sha256'] is None: continue
        if r['path'] in snapshots:
            s=snapshots[r['path']];check_pin(s,root)
            assert (s['bytes'],s['sha256'])==(r['bytes'],r['sha256'])
        else: check_pin(r,root)
    check_pin(prior['preserved_original_audit_source'],root)
    assert load(root/'evidence/04A37/preedit-coverage-audit.json')['observed']['eligible'] is False


def verify(root=ROOT, here=HERE):
    source_authority(root,here);preserved_packet(root,here)
    summaries=[];all_witnesses=[];all_rows=[];all_cases=[]
    for extension,folder in [(False,here),(True,here/'label-extension-v1')]:
        seal=load(folder/'reference-seal.json')
        assert seal['provenance']==PROVENANCE and seal['current_comparisons_started'] is False
        sealed=[r['path'] for r in seal['files']]
        assert len(sealed)==len(set(sealed)) and len(sealed)>= (8 if extension else 23)
        for row in seal['files']:check_pin(row,root)
        for name in ['cases.jsonl','Harness37.cs','original-Debug.tsv','original-Release.tsv','original-Debug-execution.json','original-Release-execution.json']:
            assert (folder/name).relative_to(root).as_posix() in sealed
        spec=load(folder/('specification.json' if extension else 'matrix-specification.json'))
        assert spec['provenance']==PROVENANCE
        cases=[json.loads(line) for line in (folder/'cases.jsonl').read_text(encoding='utf-8').splitlines()]
        inventory(cases,extension)
        results=[]
        for config in ['Debug','Release']:
            original=(folder/('original-'+config+'.tsv')).read_bytes(); current=(folder/('current-'+config+'.tsv')).read_bytes()
            rows=comparison(cases,original,current)
            witnesses=reachability(cases,rows,extension)
            before=load(folder/('original-'+config+'-execution.json')); after=load(folder/('current-'+config+'-execution.json'))
            for receipt in [before,after]:
                assert receipt['provenance']==PROVENANCE and receipt['native']=='NOT_RUN' and receipt['scientific']=='NOT_GOLDEN'
                assert receipt['count']==len(cases) and receipt['guards']==sum(r['dispatch']=='GUARD' for r in rows)
                assert receipt['geometries']==sum(r['dispatch']=='GEOMETRY' for r in rows) and receipt['exceptions']==sum(r['dispatch']=='EXCEPTION' for r in rows)
            assert stamp(before['completed_utc'])<=stamp(seal['sealed_utc'])<stamp(after['started_utc'])
            assert before['core_mvid']!=after['core_mvid']
            results.append(dict(configuration=config,rows=len(rows),bytes=len(original),sha256=digest(original),mismatches=0,
                                outcomes=dict(Counter(r['dispatch'] for r in rows)),finite_numeric_prefix_geometry=sum(r['dispatch']=='GEOMETRY' and r['usable_numeric_prefix']=='true' for r in rows),
                                reference_execution=before,current_execution=after))
            if config=='Debug':all_cases.extend(cases);all_rows.extend(rows);all_witnesses.extend(witnesses)
        assert results[0]['sha256']==results[1]['sha256']
        summaries.append(dict(corpus='explicit-label-extension-v1' if extension else 'main-v1',configurations=results))
    return dict(status='PASS',eligible=True,route='A',entry_commit=BASE,provenance=PROVENANCE,old_preedit_coverage_eligible=False,
                supplemental_rows_per_configuration=len(all_cases),outcomes_per_configuration=dict(Counter(r['dispatch'] for r in all_rows)),
                finite_numeric_prefix_geometry=sum(r['dispatch']=='GEOMETRY' and r['usable_numeric_prefix']=='true' for r in all_rows),
                corpora=summaries,witnesses=all_witnesses,open_cases=[],mismatches=[],scientific='NOT_GOLDEN',native='NOT_RUN'),all_cases,all_rows


def verify_indexes(result,cases,rows,here=HERE):
    assert load(here/'compatibility-and-coverage.json')==result
    with (here/'reference-dispatch-expectations.csv').open(encoding='utf-8',newline='') as f:
        expectations=list(csv.DictReader(f))
    assert len(expectations)==len(rows)
    for i,(case,row,expected) in enumerate(zip(cases,rows,expectations)):
        assert expected==dict(case_id=case['case_id'],expected_dispatch=row['dispatch'],expected_source_phase=row['phase'],expected_prefix_completed=row['prefix_completed'],input_sha256=row['input_sha256'],expected_result_sha256=digest(row['result'].encode()),expected_error=row['error'],expectation_source=('label-extension-v1/' if i>=18466 else '')+'original-Debug.tsv')
    dimensions=['cohort','family','operation','culture','components','scale','eigen','factors','trace','initial_eigen','matrices','multiple','structures','axis','row_side','row_ids','row_labels','groups']
    counts=defaultdict(Counter)
    for case,row in zip(cases,rows):
        key=tuple(str(case.get(k,'')) for k in dimensions)
        counts[key][row['dispatch']]+=1
        counts[key]['finite_numeric_prefix_geometry']+=row['dispatch']=='GEOMETRY' and row['usable_numeric_prefix']=='true'
    with (here/'coverage-cells.csv').open(encoding='utf-8',newline='') as f:cells=list(csv.DictReader(f))
    actual={}
    for row in cells:
        key=tuple(row[k] for k in dimensions); assert key not in actual
        actual[key]=Counter({k:int(row[k]) for k in ['GUARD','GEOMETRY','EXCEPTION','finite_numeric_prefix_geometry']})
        assert int(row['total'])==sum(actual[key][k] for k in ['GUARD','GEOMETRY','EXCEPTION'])
    assert actual==counts


if __name__=='__main__':
    value,cases,rows=verify()
    verify_indexes(value,cases,rows)
    print('PASS independent RouteA reader;',value['supplemental_rows_per_configuration'],'exact rows/configuration;',value['outcomes_per_configuration'],'; NOT_GOLDEN; native NOT_RUN')
