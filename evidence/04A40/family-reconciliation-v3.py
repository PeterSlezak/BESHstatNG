"""Resolve actual GUI/member edges and retain issue-specific restricted contracts."""
from common40_v3 import *
import collections
lock();syntax=load(PRIVATE/'source-map-final-v2.json');old=load(HERE/'ownership/method-parity-v2.json')
gui=[m for m in syntax['members'] if '/UI/' in m['path'] and m['kind'] in ['SubBlock','FunctionBlock','ConstructorBlock','PropertyBlock']]
shared=[m for m in syntax['members'] if m['path'].startswith('BESHStat.Excel.Common/') and m['kind'] in ['SubBlock','FunctionBlock','ConstructorBlock','PropertyBlock']]
def view(m):return dict(path=m['path'],member=m['symbol_id'],name=m['name'],signature=m['signature'],lines=[m['start_line'],m['end_line']],body_sha256=m['body_sha256'],direct_calls=m['direct_calls'])
family=[]
for f in sorted({r['family'] for r in old['methods']}):
    rows=[r for r in old['methods'] if r['family']==f];names=set(n for r in rows for n in r.get('existing_Common_owners',[]))
    gui_paths={r['GUI_consumer'] for r in rows if 'GUI_consumer' in r}
    gm=[m for m in gui if m['path'] in gui_paths and any(any(n in call for n in names|{'WindowsRegressionRequestAdapter','Fit','Report','CreateCoreModel','MixedModel'}) for call in m['direct_calls'])]
    sm=[m for m in shared if any(n in m['containing_type'] for n in names)]
    family.append(dict(family=f,UDF_members=len(rows),GUI_member_edges=list(map(view,gm)),Common_member_edges=list(map(view,sm)),
       actual_UDF_calls=[dict(member=r['member'],calls=r['calls']) for r in rows],
       request_contract='UDF explicit parsed options retain worksheet defaults; GUI projected intent retains its own defaults. New typed setters do not invoke stricter Validate or change acquired input.',
       result_contract='Accepted38 report/preparation and original data/name/row axes retained; no result rounding, order, p-value, covariance headers or handle clones changed.',
       host_only='Reference/RefEdit acquisition, Excel sentinel transport, session cache, WinForms/writer/COM',
       no_new_parser=True,no_new_statistical_engine=True))
save('ownership/family-parity-v3.json',dict(base=pin(HERE/'ownership/method-parity-v2.json'),UDF_members=200,families=family,GUI_methods=len({m['member'] for f in family for m in f['GUI_member_edges']}),
    GUI_vs_UDF='Shared policy owners are mapped explicitly; native UI behavior is retained, not declared equal where historic discrepancy is restricted.',
    already_shared_families=['MMRM','Formula/effects','Survival/KM/logrank result preparation'],gates=GATES))
base=load(ROOT/'evidence/04A39/register/defects-v7.json');v1=load(HERE/'register/issue-delta-v1.json');delta={r['canonical_id']:r for r in v1['entries']}
for r in base['issues']:
    if any(a.startswith(('C08-','C09-','C10-','C11-','C12-','C13-','C14-')) for a in r['aliases']) and r['canonical_id'] not in delta:
        delta[r['canonical_id']]=dict(canonical_id=r['canonical_id'],aliases=r['aliases'],pre_status=r['status'],post_status='INVESTIGATED_UNAPPLIED_RESTRICTED',
          current_authorization='INVESTIGATION_ONLY',original_record_sha256=sha(json.dumps(r,sort_keys=True,ensure_ascii=False).encode()),
          original_report_occurrences=r['report_occurrences'],historical_source_locations_retained=r['source_locations'],original_obligations=r['original_obligations'],original_controls=r['original_controls'],
          observed_behavior=r['current_behavior'],implementation='NONE; existing behavior retained',next_owner=r['responsible_future_owner'],unblocking_criterion=r['remaining_gate'],exit_disposition_04A48=None)
notes={
 'C08-CORE-LABEL-01':'Interaction/polynomial label alignment remains RegressionDesignCore/RegressionEffectAuthoringPolicy-owned; no coefficient/data/label recoding. Existing Regression08 tests and all model-input corpora executed on final source.',
 'C08-FORMULA-REF-01':'Categorical reference string formatting remains original formula/design policy. Invariant/sk-SK and original frozen inputs retained; no high-precision level reformatting or new formula parser.',
 'C10-LM-ALPHA-01':'Requested UDF alpha0.10 versus0.05 real weighted fits materialized. GUI intent NativeLegacyDefault still0.05; no native patch. Shared setter takes explicit UDF options only.',
 'C10-COV-NAMES-01':'GUI DeferredCovarianceDataHeaders and clone behavior retained; UDF report shapes/headers remain accepted38. No covariance Data header patch activated.',
 'C12-GEE-START-QIC-01':'GetStartParams initializes pIndependenceNaiveVarCovar; supplied-start Fit bypasses GetStartParams, EstimateQIC consumes the field. Four actual supplied-start cases retain NullReferenceException; automatic-start fits return results. No QIC repair or new weight/AR(1) convention.',
 'C12-GEE-CLUSTER-TEXT-01':'UDF acquired cluster identity and GUI importer typed/text distinction remain unchanged; no source-row or cluster reinterpretation. Existing Gee12/UDF model-input cases executed.',
 'C12-GEE-PREDICTION-AXIS-01':'GeePredictionOutputAxis and original selected/observed/residual axes remain distinct. No physical coordinates invented for positional arrays or predictions; sparse/reordered row assertions remain in Gee12 tests.',
 'C12-NATIVE-CLASSIFICATION-01':'GUI selected-family versus effective model classification retained; no native classification patch. Model options now shared without activating acquisition/refusal.',
 'C13-LOGRANK-R-01':'Survival.WeightedLogRank removes earliest censor records before constructing per-stratum risk sets. A censor tied with earliest event is wrongly removed. Current canonical factory fixtures include grouped/stratified ties and no-event cases; raw pre/post results preserved. Historical R candidate is retained, not newly independently reproduced. No exact correction authority authenticated; correction remains dormant.',
 'C14-INPUT-FINITE-01':'LMM/MMRM finite checks and request validation preserved; no stricter rejection or numerical change. KR/Satterthwaite/LS-means/contrast/grid differences remain science/native restricted.'}
for r in delta.values():
    alias=next((a for a in r['aliases'] if a in notes),None)
    if alias:r['04A40_source_backed_investigation']=notes[alias]
    r['reference_status']='HISTORICAL_REFERENCE_RETAINED;FRESH_R_NOT_RUN;NOT_GOLDEN'
    r['native_status']='NOT_RUN_UNAUTHORIZED;NO_UI_PATCH_ACTIVATED'
    r['behavior_fix_activated']=False
historical=[]
for phase,paths in {'13':['issues.json','numerical-comparisons.json','R-candidate-reference-v2.csv'], '14':['source-inventory.json','source-delta.json'], '15':['source-contract.json'], '16':['source-contract.json']}.items():
    for name in paths:
        path='evidence/04A'+phase+'/'+name
        if (ROOT/path).is_file():historical.append(pin(path))
save('register/issue-delta-v3.json',dict(base=v1['base'],canonical_base_entries=1169,original_obligations=1514,restricted_controls=802,entries=list(delta.values()),
    mixed14_16=dict(status='INVESTIGATED_RETAINED_RESTRICTED_NOT_EXIT_DISPOSITION',immutable_named_alias='C14-INPUT-FINITE-01',
       note='No invented C15/C16 aliases. Their source/evidence/control obligations remain original canonical register entries; historical source contracts and final selected Mixed14/15/16 tests provide cross-reference.',
       request='Same MixedModelFitRequest data/name/visit/treatment order, optimizer/options, effective alpha and covariance choices.',
       inference='Same KR/Satterthwaite switches; same estimators/degrees of freedom/LS-means/contrast/reference grid. Twelve new tests include real LMM/Cox fits.',scientific_native='RESTRICTED'),
    history=historical,current_Core_sources=[pin(p) for p in ['BESHStat.Core/src/StatTests/Survival.vb','BESHStat.Core/src/RegModels/GEE.vb']],
    all_remaining_entries='UNCHANGED_NO_DISCHARGE',C13_authorization='WINDOW_ASSIGNMENT_FOUND;ISSUE_SPECIFIC_CORRECTION_AND_FRESH_INDEPENDENT_REFERENCE_NOT_AUTHENTICATED;HOLD',
    reserved_04A48_exit_dispositions=v1['reserved_exit_dispositions'],gates=GATES))
print('Family GUI/Common edges',len(family),'families; issue decisions',len(delta),'no inference/native activation',flush=True)
