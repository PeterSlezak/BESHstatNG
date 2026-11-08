"""Route A only: append-only source/permission/specification preparation, no product edit."""
from pathlib import Path
from datetime import datetime, timezone
import hashlib
import itertools
import json
import subprocess

ROOT=Path(__file__).resolve().parents[4]
HERE=Path(__file__).resolve().parent
BATCH=HERE.parents[1]
BASE='a0680f546c5130fa5143c0b0c7136f852d650cc5'
PROVENANCE='CAPTURED_AFTER_INITIAL_IMPLEMENTATION_FROM_PINNED_ARCHIVED_ENTRY'
QUOTE="I'm comfortable authorizing the late archived-entry supplement for 04A.37"

def sha(b):return hashlib.sha256(b).hexdigest()
def git(*a):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*a],cwd=ROOT)
def pin(p):
    p=Path(p);b=p.read_bytes();return dict(path=p.relative_to(ROOT).as_posix(),bytes=len(b),sha256=sha(b))
def save(name,v):
    p=HERE/name;p.parent.mkdir(parents=True,exist_ok=True)
    if p.exists():
        old=json.loads(p.read_bytes());a=dict(old);b=dict(v)
        for key in ['recorded_utc']:a.pop(key,None);b.pop(key,None)
        assert a==b, 'Refuse to replace prior receipt: '+name
        return
    with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(v,f,indent=2,ensure_ascii=False,allow_nan=False);f.write('\n')
def put(name,b):
    p=HERE/name;p.parent.mkdir(parents=True,exist_ok=True)
    if p.exists():assert p.read_bytes()==b;return
    with p.open('xb') as f:f.write(b)

def specifications():
    cases=[]
    def add(family,operation,culture,**parameters):
        d=dict(family=family,operation=operation,culture=culture,**parameters)
        d['case_id']=family+'.'+operation+'.'+sha(json.dumps(d,sort_keys=True,separators=(',',':')).encode())[:24]
        d['expected_stage']='PUBLIC_GUARD_OR_PRE_CHART_THEN_RECORDED_LATE_NATIVE_BOUNDARY'
        cases.append(d)
    cultures=['','sk-SK']
    scales=['0','0.25','0.5','0.75','1','-0.1','1.1','NaN','+Inf','-Inf']
    eigen=['positive_unequal','zero','negative','NaN','+Inf','missing','too_short']
    for culture,k,n,p,c,e in itertools.product(cultures,[0,1,2,3],[0,1,2,4],[1,2,4],scales,eigen):
        add('PCA','Biplot',culture,components=k,observations=n,variables=p,scale=c,eigen=e,row_ids='known',row_labels='mixed',groups='mixed',variable_labels='mixed',matrix='valid',cohort='biplot_full_cross_product')
    # Reachable ungrouped references for every finite metric/eigen state.
    for culture,k,c,e in itertools.product(cultures,[2,3],scales[:5],eigen):
        add('PCA','Biplot',culture,components=k,observations=4,variables=4,scale=c,eigen=e,row_ids='known',row_labels='none',groups='none',variable_labels='mixed',matrix='valid',cohort='biplot_ungrouped')
    for culture,op,k,r,l,g in itertools.product(cultures,['ScorePlot2D','ScorePlot3D'],[0,1,2,3],['known','missing','short','duplicate'],['none','blank','mixed','short','long'],['none','mixed','case_width','short','long']):
        add('PCA',op,culture,components=k,observations=4,variables=4,scale='1',eigen='positive_unequal',row_ids=r,row_labels=l,groups=g,variable_labels='mixed',matrix='valid',cohort='score_identity_full_cross_product')
    for culture,op,k,p,v,m in itertools.product(cultures,['LoadingPlot2D','LoadingPlot3D'],[0,1,2,3],[1,2,4],['mixed','missing','short'],['valid','missing_loadings','short_columns']):
        add('PCA',op,culture,components=k,observations=4,variables=p,scale='1',eigen='positive_unequal',row_ids='known',row_labels='none',groups='none',variable_labels=v,matrix=m,cohort='loading_shapes_labels')
    for culture,k,n,p,e in itertools.product(cultures,[0,1,2,3],[0,1,2,4],[1,2,4],eigen):
        add('PCA','ScreePlot',culture,components=k,observations=n,variables=p,scale='1',eigen=e,row_ids='known',row_labels='none',groups='none',variable_labels='mixed',matrix='valid',cohort='scree_cross_product')
    for culture,k,n,p,c,m in itertools.product(cultures,[2,3],[1,4],[2,4],scales[:5],['missing_scores','missing_loadings','short_columns','short_rows','signed_zero','constant']):
        add('PCA','Biplot',culture,components=k,observations=n,variables=p,scale=c,eigen='positive_unequal',row_ids='known',row_labels='mixed',groups='mixed',variable_labels='mixed',matrix=m,cohort='biplot_shape_edges')
    for culture,op,k,t,e,m in itertools.product(cultures,['ScreePlot','LoadingPlot2D','LoadingPlot3D'],[0,1,2,3,4],['positive','zero','negative','NaN','+Inf','missing'],['positive','missing','empty','nonfinite'],['valid','pattern_missing','structure_missing','pattern_short_columns','structure_short_rows','structure_short_columns']):
        add('FACTOR',op,culture,factors=k,trace=t,initial_eigen=e,matrices=m,profile='oblique_distinct_signs',variable_labels='mixed',cohort='factor_full_cross_product')
    for culture,op,k,t,profile,v in itertools.product(cultures,['LoadingPlot2D','LoadingPlot3D'],[2,3,4],['positive','zero','negative','NaN','+Inf','missing'],['orthogonal_signs','oblique_distinct_signs'],['mixed','missing','short']):
        add('FACTOR',op,culture,factors=k,trace=t,initial_eigen='positive',matrices='valid',profile=profile,variable_labels=v,cohort='factor_labels_profiles')
    for culture,op,k,rotation in itertools.product(cultures,['ScreePlot','LoadingPlot2D','LoadingPlot3D'],[2,3],['None','Varimax','Promax']):
        add('FACTOR',op,culture,factors=k,trace='fitted',initial_eigen='fitted',matrices='fitted',profile='fitted_'+rotation,rotation=rotation,variable_labels='mixed',cohort='entry_engine_fitted_rotation')
    structures=['valid','row_factors_missing','column_factors_missing','row_short_columns','column_short_columns','row_labels_missing','column_labels_missing','row_labels_short','column_labels_short','variable_labels_short','percent_missing','percent_short','row_contribution_missing','column_contribution_missing','contribution_short_columns','zero_contributors']
    for culture,multiple,k,structure in itertools.product(cultures,[False,True],[0,1,2,3,4],structures):
        common=dict(multiple=multiple,factors=k,structures=structure,bounds_profile='hidden_row_extremes',cohort='ca_mca_full_cross_product')
        add('CA','CorrespondencePlot',culture,**common,axis=0,row_side=False)
        for axis,row in itertools.product([-1,0,1,2,4],[False,True]):add('CA','ContributionPlot',culture,**common,axis=axis,row_side=row)
    for culture,multiple,k,rows,cols in itertools.product(cultures,[False,True],[2,3,4],[0,1,3],[0,1,4]):
        add('CA','CorrespondencePlot',culture,multiple=multiple,factors=k,structures='valid',bounds_profile='ordinary',rows=rows,columns=cols,cohort='ca_empty_singleton_axes',axis=0,row_side=False)
    for culture,family,op in itertools.product(cultures,['PCA','FACTOR','CA'],['null_model']):
        add(family,op,culture,cohort='null_guards')
    assert len(cases)==len({x['case_id'] for x in cases})
    return cases

def main():
    assert git('branch','--show-current').decode().strip()=='codex/04A-37'
    assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==git('merge-base','HEAD','main').decode().strip()==BASE
    assert not git('diff','--cached','--name-only')
    manifest=json.loads((BATCH/'publication-manifest.json').read_bytes())
    assert len(manifest['files'])==120 and manifest['decision']=='NOT_READY_04A37'
    for r in manifest['files']:
        if r['sha256'] is not None:assert pin(ROOT/r['path'])==r
    mutable=['readiness.json','publication-manifest.json','validation.json','audit-results.json','source-transitions.json','privacy-check.json','command-receipts.json','git-final.json','verify_packet.py','packet.py','README.md']
    snapshots=[]
    for name in mutable:
        b=(BATCH/name).read_bytes();put('entry-not-ready/'+name,b);snapshots.append(dict(original='evidence/04A37/'+name,snapshot=pin(HERE/'entry-not-ready'/name)))
    p=ROOT/'docs/architecture/batch-04A-37-owner-review.md';put('entry-not-ready/owner-review.md',p.read_bytes());snapshots.append(dict(original=p.relative_to(ROOT).as_posix(),snapshot=pin(HERE/'entry-not-ready/owner-review.md')))
    save('entry-packet-pins.json',dict(manifest=pin(BATCH/'publication-manifest.json'),files=manifest['files'],snapshots=snapshots,
        preserved_original_audit_source=pin(BATCH/'test_audit.py'),scope='Original120-entry NOT_READY packet reverified before supplement. Mutable current records have exact versioned snapshots; all other prior artifacts must stay byte-identical.'))
    save('supplemental-approval.json',dict(owner_quote=QUOTE,owner_decision_date='2026-11-08',route='A',batch='04A.37',
        provenance='Direct owner ChatGPT project-conversation decision, quoted verbatim in continuation prompt sectionA; no external message identifier or independent ownership attestation invented.',
        exception='Versioned late-executed archived-entry original-source supplemental compatibility oracle only',capture_provenance=PROVENANCE,
        authorizes=['Pinned archived original-source references and current-code comparisons for missing orthogonal cases','Current04A37 dossier/receipt/manifest/readiness closeout'],
        excludes=['RouteB','statistical/renderer behavior changes','native/R/manual execution','test weakening or historical rebaseline','scientific-golden/native release','commit/merge/push/publication/install/sign/deploy','04A38'],
        gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),recorded_utc=datetime.now(timezone.utc).isoformat()))
    authentic=[]
    for name in ['Pca','FactorAnalysis','CorrespondenceAnalysis','ClusterAnalysis']:
        path='BESHstatNG/src/Graphics/'+name+'PlotExcel.vb';blob=git('show',BASE+':'+path);archived=BATCH/'preedit'/ (name+'PlotExcel.vb.txt')
        old=archived.read_bytes()
        expected=next(r for r in json.loads((BATCH/'preedit/receipt.json').read_bytes())['files'] if r['path']==archived.relative_to(ROOT).as_posix())
        assert pin(archived)==expected
        assert blob==old or blob.replace(b'\n',b'\r\n')==old
        put('archived-source/'+name+'PlotExcel.vb.txt',old)
        authentic.append(dict(path=path,commit=BASE,git_blob_sha256=sha(blob),git_blob_bytes=len(blob),sha256=sha(old),bytes=len(old),archived=pin(HERE/'archived-source'/archived.name),matches_original_frozen_snapshot=True,git_qualification='Exact Git canonical LF to frozen-entry CRLF reconstruction; no other byte difference.'))
    core_paths=['BESHStat.Core/src/RegModels/Multivariate/'+n+'.vb' for n in ['PCA','FactorAnalysis','CA','ClusterAnalysis']]+['BESHStat.Core/src/DM/ArrayUtilities.vb','BESHStat.Core/src/Graphics/ChartAxisScalingCore.vb','BESHstatNG/src/RegModels/LinearAlgebra/Matrix.vb','BESHstatNG/src/Graphics/ChartingFunc.vb']
    helpers=[]
    entry_pins={r['path']:r for r in json.loads((BATCH/'entry-tracked-pins.json').read_bytes())['files']}
    for path in core_paths:
        b=git('show',BASE+':'+path);live=(ROOT/path).read_bytes()
        assert sha(live)==entry_pins[path]['sha256']
        assert b.replace(b'\r\n',b'\n')==live.replace(b'\r\n',b'\n')
        helpers.append(dict(path=path,bytes=len(b),sha256=sha(b),live_sha256=sha(live),entry_raw_sha256=entry_pins[path]['sha256'],qualification='Live bytes exactly match pinned entry; Git canonical content matches after established LF/CRLF (including mixed final newline) normalization.'))
    # The independently compiled original prefixes already frozen before implementation
    # are reused unchanged; the supplemental case factory is entirely new and independent.
    for name in ['Original37.vb']:
        b=(BATCH/'preedit'/name).read_bytes();put('archived-source/'+name,b)
    current=ROOT/'BESHStat.Excel.Common.Tests/src/Graphics37CurrentPrefixes.vb'
    put('current-source/Current37.vb',current.read_bytes())
    save('source-authenticity.json',dict(status='PASS',entry_commit=BASE,adapters=authentic,unchanged_helper_sources=helpers,
        original_prefix=pin(HERE/'archived-source/Original37.vb'),current_prefix=pin(HERE/'current-source/Current37.vb'),
        binding='Original Matrix.GetColumnFrom2Darray delegates to existing ArrayUtilities.GetColumn; ChartScaling delegates to ChartAxisScalingCore.Compute; Friend AxisPadding exact body in frozen original prefix. No new37 Core/Common class in original oracle.',
        extraction_authority=pin(BATCH/'preedit/source-slices.json'),chronology=PROVENANCE))
    cases=specifications()
    with (HERE/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as f:
        for c in cases:f.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    save('matrix-specification.json',dict(version='routeA-v1',provenance=PROVENANCE,constructed_utc=datetime.now(timezone.utc).isoformat(),
        cases=pin(HERE/'cases.jsonl'),count=len(cases),generation='Named independent Cartesian parameters; IDs are content hashes, never modulo-derived parameter selectors.',
        mandatory_cohorts=['biplot_full_cross_product','biplot_ungrouped','score_identity_full_cross_product','factor_full_cross_product','ca_mca_full_cross_product'],
        guards='Public guard returns are classified separately and never satisfy reachable geometry cells.',
        native='No Chart/Series/XYZ draw/native API invoked; late pure expressions evaluated at explicitly recorded original boundaries. COM effects/source-only style invariants are not runtime equivalence.',
        scientific='NOT_GOLDEN',dendrogram='Reuse immutable original1536 rows; original Debug-only provenance remains unchanged.'))
    print('Prepared RouteA authority and',len(cases),'orthogonal cases; no production/test sources changed')

if __name__=='__main__':main()
