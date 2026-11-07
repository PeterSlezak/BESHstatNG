"""Independent read-only stdlib verifier; imports no producer or legacy packet code."""
from pathlib import Path
import json,csv,hashlib,collections,re,ast as pyast
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
SELF='evidence/04A36/publication-manifest.json';READY='READY_FOR_OWNER_04A36_ACCEPTANCE_REVIEW'
SOURCES=['BESHStat.Core.Tests/src/Graphics/Cases36.vb','BESHStat.Core.Tests/src/Graphics/Graphics36GeometryTests.vb','BESHStat.Core.Tests/src/Graphics/Legacy36.vb','BESHStat.Core/src/Graphics/CoxCurveGeometry36.vb','BESHStat.Core/src/Graphics/KaplanMeierSeriesGeometry36.vb','BESHStat.Core/src/Graphics/SpcZoneBands36.vb','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','BESHStat.Excel.Common.Tests/src/Graphics36PresentationTests.vb','BESHStat.Excel.Common.Tests/src/LegacyPresentation36.vb','BESHStat.Excel.Common.Tests/src/LegacyPresentation36V2.vb','BESHStat.Excel.Common/src/Analysis/SpcPanelPresentation36.vb','BESHStat.Excel.Common/src/Results/CoxResidualPlotAvailability36.vb','BESHStat.Excel.Common/src/Results/SurvivalChartPresentation36.vb','BESHStat.Excel.Common/src/Results/SurvivalReportPolicy.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG.Test/src/Charts/Graphics36AdapterTests.vb','BESHstatNG/src/Graphics/CoxSurvivalPlotExcel.vb','BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb','BESHstatNG/src/Graphics/SpcControlChartExcel.vb','docs/architecture/batch-04A-36-owner-review.md']
EXCLUDED={'evidence/04A36/entry-source/planning/BESHStatNG_CrossPlatform_MasterPlan.md.txt'}
def load(name):return json.loads((HERE/name).read_bytes())
def scope():return set(SOURCES+[p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and not any(x in p.parts for x in ['__pycache__','bin','obj']) and p.relative_to(ROOT).as_posix() not in EXCLUDED]+[SELF])
def pin(path,root=ROOT):
    p=(root/path).resolve();assert p.is_relative_to(root.resolve()) and p.is_file()
    b=p.read_bytes();return dict(path=path,bytes=len(b),sha256=hashlib.sha256(b).hexdigest())
def canonical(path):
    assert isinstance(path,str) and path and '\\' not in path and ':' not in path and not path.startswith('/') and not any(ord(c)<32 for c in path)
    assert all(x not in ('','.','..') and not x.endswith((' ','.')) and not re.match(r'^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)',x,re.I) for x in path.split('/'))
def validate_manifest(rows,expected,root=ROOT):
    paths=[r['path'] for r in rows];assert len(paths)==len(set(paths))==len({p.casefold() for p in paths})
    assert set(paths)==set(expected) and paths.count(SELF)==1
    for r in rows:
        assert set(r)=={'path','bytes','sha256'};canonical(r['path'])
        if r['path']==SELF:assert r['bytes'] is r['sha256'] is None
        else:assert isinstance(r['bytes'],int) and r['bytes']>=0 and isinstance(r['sha256'],str) and re.fullmatch('[0-9a-f]{64}',r['sha256'])
    for r in rows:
        if r['path']!=SELF:assert r==pin(r['path'],root),'Modified/missing payload: '+r['path']
def validate_gates(gates):assert gates==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED')
def validate_public_text(text):
    assert not re.search(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+',text,re.I)
    assert not re.search(r'S-1-5-\d+(?:-\d+)*',text)
    assert not re.search(r'AKIA[0-9A-Z]{16}|-----BEGIN [A-Z ]*PRIVATE KEY-----',text)
def validate_ownership(rows,ast,coverage):
    original={n['key']+'@'+str(n['span_start'])+':'+str(n['span_length']):n for n in ast['entry']}
    assert len(rows)==len(original)==106 and len({r['member_id'] for r in rows})==106 and set(original)=={r['member_id'] for r in rows}
    assert {r['family'] for r in rows}=={'KM','Cox','SPC'} and coverage['open_blockers']==0
    for r in rows:
        assert r['disposition'] in {'PORTED_AND_CONSUMED','ALREADY_PORTABLE_REUSED','HOST_ONLY','DEFERRED_APPROVED'}
        assert r['reason'] and r['entry_token_sha256']==original[r['member_id']]['token_sha256']
        assert r['member']==original[r['member_id']]['name']
        if r['disposition']=='PORTED_AND_CONSUMED':
            allowed={('KM','Plot'),('Cox','PlotCox')}|{('SPC',n) for n in ['RenderPanel','AddZoneBands','ComputeYAxisBounds','HasValidZones','AddReferenceLine','AddConstantReferenceLine','AddZoneLine','AddLastValueLabel','IncludesLines','IncludesBands','IncludeValuesInBounds','IncludeNullableInBounds','HasAnyValue','IsNumericChartValue','IsFinite']}
            assert (r['family'],r['member']) in allowed and r['target'] and r['compiled_consumer']
        if r['current_member_id']:
            n=next(n for n in ast['current'] if n['key']+'@'+str(n['span_start'])+':'+str(n['span_length'])==r['current_member_id'])
            assert n['token_sha256']==r['final_token_sha256']
        else:assert r['mapping']=='EXPLICIT_SUPERSESSION' and r['disposition']=='PORTED_AND_CONSUMED'
    def tokens(owner,name):return [n['tokens'] for n in ast['current'] if n['owner'].endswith('.'+owner) and n['name']==name]
    assert any(all(x in t for x in ['KaplanMeierSeriesGeometry36 . Survival','KaplanMeierSeriesGeometry36 . Censor','KaplanMeierSeriesGeometry36 . Confidence']) for t in tokens('KaplanMeierPlotExcel','Plot'))
    assert any(all(x in t for x in ['CoxCurveGeometry36 . Validate','CoxCurveGeometry36 . Axis','CoxCurveGeometry36 . Snapshot']) for t in tokens('CoxSurvivalPlotExcel','PlotCox'))
    assert any('PreparePresentation36' in t and 'New SpcZoneBands36' in t and 'presentation . Series' in t for t in tokens('SpcControlChartExcel','RenderPanel'))
    assert any('New SpcPanelPresentation36' in t for t in tokens('SpcControlChartExcel','PreparePresentation36'))
    assert any('SpcPanelPresentation36 . AxisBounds' in t for t in tokens('SpcControlChartExcel','ComputeYAxisBounds'))
def validate_validation(v,identities):
    assert not v['unclassified_managed'] and not v['unclassified_python']
    assert v['native']=='NATIVE_NOT_RUN' and v['r']=='R_NOT_RUN' and v['scientific']=='NOT_GOLDEN'
    assert v['new_managed_cases_per_configuration']==83
    assert len(identities)==sum(v['managed_totals']['Debug'].values())==sum(v['managed_totals']['Release'].values())
    assert all(r['removed']==0 and r['changed_baseline_outcomes']==0 for r in v['identity_deltas'])
    assert len({(r['suite'],r['raw_identity_sha256'],r['raw_occurrence_ordinal']) for r in identities})==len(identities)
    for config in ['debug','release']:
        new=[r for r in identities if r['merged_main_'+config]=='NEW_04A36'];assert len(new)==83 and all(r['final_'+config]=='Passed' for r in new)
        assert all(r['merged_main_'+config]==r['final_'+config] for r in identities if r['merged_main_'+config]!='NEW_04A36')
        assert sum(r['final_'+config]=='Failed' for r in identities)==14
        assert dict(collections.Counter(r['final_'+config] for r in identities))==v['managed_totals'][config.capitalize()]
    assert v['node']==dict(total=916,passed=916,failed=0)
    assert v['strict_profile_mkdocs']['final']==dict(strict=1,profile=1,docs=0)
def validate_comparison(value):
    assert value['status']=='PASS_EXACT_SOFTWARE_COMPATIBILITY' and value['rows_per_configuration']==6240 and value['scientific']=='NOT_GOLDEN'
    rows=value['comparisons'];assert len(rows)==6
    for config in ['Debug','Release']:assert sum(r['rows'] for r in rows if r['configuration']==config)==6240
    for r in rows:
        assert r['byte_identical'] and r['mismatches']==0
        assert pin(r['pre_edit']['path'])==r['pre_edit'] and pin(r['post_edit']['path'])==r['post_edit']
        assert r['pre_edit']['sha256']==r['post_edit']['sha256']
    for r in load('pre-edit-capture-authority.json')['files']:assert pin(r['path'])==r
def validate_history(rows,coverage):
    with (ROOT/'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig') as f:old=[r for r in csv.DictReader(f) if r['path'].endswith(('/KaplanMeierPlotExcel.vb','/CoxSurvivalPlotExcel.vb','/SpcControlChartExcel.vb'))]
    assert len(rows)==len(old)==coverage['historical_members']==130
    assert len({r['historical_member_id'] for r in rows})==130 and {r['historical_member_id'] for r in rows}=={r['member_id'] for r in old}
    assert all(r['disposition']!='OPEN_BLOCKER' and r['current_successor'] for r in rows)
    h=next(r for r in rows if r['name']=='HasNaturalZero');assert h['disposition']=='ALREADY_PORTABLE_REUSED' and 'SpcAxisBoundsCore' in h['target']
def validate_readiness(decision,validation,readiness,git_state,external):
    if decision==READY:
        assert validation['decision']==READY and not readiness['blockers']
        assert git_state['full_diff_check_exit']==0 and external['status']=='OWNER_CONFIRMED_PRESERVED'
    else:assert decision==validation['decision']=='NOT_READY_04A36' and readiness['blockers']
def validate_transitions(rows):
    assert len(rows)==len(SOURCES) and len({r['path'] for r in rows})==len(SOURCES) and {r['path'] for r in rows}==set(SOURCES)
    for r in rows:
        actual=pin(r['path']);assert all(r[k]==actual[k] for k in ['path','bytes','sha256'])
def verify(require_ready=False):
    manifest=load('publication-manifest.json');assert manifest['decision'] in [READY,'NOT_READY_04A36']
    validate_manifest(manifest['files'],scope());validate_gates(load('entry-authority.json')['gates'])
    for name in ['audit-results.json','not-run.json','publication-exclusions.json','source-policy.json','project-includes.json','readiness.json']:assert (HERE/name).is_file(),name
    for p in HERE.rglob('*.py'):
        if not any(x in p.parts for x in ['bin','obj','__pycache__']):pyast.parse(p.read_text(encoding='utf-8-sig'),filename=p.name)
    with (HERE/'member-ownership.csv').open(encoding='utf-8-sig') as f:rows=list(csv.DictReader(f))
    ast=load('final-members.json');coverage=load('coverage.json');validate_ownership(rows,ast,coverage)
    with (HERE/'historical-crosswalk.csv').open(encoding='utf-8-sig') as f:validate_history(list(csv.DictReader(f)),coverage)
    with (HERE/'managed-test-identities.csv').open(encoding='utf-8-sig') as f:identities=list(csv.DictReader(f))
    v=load('validation.json');validate_validation(v,identities);validate_comparison(load('compatibility-comparison.json'))
    validate_transitions(load('source-transitions.json')['files'])
    assert load('privacy-check.json')['status']=='PASS' and load('preservation.json')['status']=='HASHES_VERIFIED'
    for r in load('lineage-check.json')['pins']:assert pin(r['path'])['sha256']==r['final36_sha256']
    registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())['files'];assert len(registry)==100
    assert all(pin(p)['sha256']==r['sha256'] for p,r in registry.items())
    assert len(load('lineage-check.json')['pins'])==278
    assert load('git-final.json')['index']=='EMPTY' and load('git-final.json')['batch_diff_check_exit']==0
    assert 'ckResidualPlots.Enabled = False' in (ROOT/'BESHstatNG/src/UI/Ui4Cox.Designer.vb').read_text(encoding='utf-8-sig')
    validate_readiness(manifest['decision'],v,load('readiness.json'),load('git-final.json'),load('external-changes.json'))
    if require_ready:assert manifest['decision']==READY
    return len(manifest['files'])
if __name__=='__main__':print('PASS independent36 packet verifier;',verify(),'entries; null self; software scope only')
