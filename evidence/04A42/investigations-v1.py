from common42 import *
import re,collections
scope=module('scope42',HERE/'scope-v1.py');lock();inventory=load(HERE/'scope/UDF-inventory-v1.json');freeze=load(HERE/'matrix/source-freeze-v2.json');before=load(PRIVATE/'fixtures-before-v1/observations.json');compare=module('cmp42',HERE/'verify-wave-v1.py')
after=load(PRIVATE/'fixture-final3-Debug-42/observations.json');assert compare.canonical(before)==compare.canonical(after)
register=load(ROOT/'evidence/04A39/register/defects-v7.json');canonical={i['canonical_id']:i for i in register['issues']}
mapped=load(ROOT/'evidence/04A39/map/method-policy-map-v1.json');assert mapped['focus_members']==len(mapped['methods'])==192
matrix=[];changed=[];holes=[]
for row in inventory['functions']:
    current=next(m for m in scope.methods(ROOT/row['source']['path']) if m['name']==row['function']);altered=sha(current['text'].encode())!=row['method_sha256'];cases=[r for r in before['rows'] if r['method'].endswith('.'+row['module']+'.'+row['function'])]
    successes=[r['id'] for r in cases if r['id'].endswith('/ordinary') and (r.get('output') or {}).get('type')=='System.Object[,]']
    if altered:changed.append(row['function']);assert successes,'CHANGED_PARENT_WITHOUT_EXECUTED_SUCCESS'
    if not successes and not row['function'].endswith(('_FIT','_DROP','CLEANUP')):holes.append(dict(function=row['function'],unchanged=not altered,reason='Retained original execution error;not mislabeled as successful fit/output.'))
    policy=[]
    for m in mapped['methods']:
        name=m['member'].split('(')[0].rsplit('.',1)[-1]
        if re.search(r'\b'+re.escape(name)+r'\s*\(',current['text']):policy.append(dict(identity=m['identity'],source_path=m['path'],original_source_sha256=m['source_sha256'],original_body_sha256=m['body_sha256']))
    matrix.append(dict(function=row['function'],excel_name=row['excel_name'],module=row['module'],signature_before=row['signature'],before_source=row['source'],after_source=pin(row['source']['path']),before_method_sha256=row['method_sha256'],after_method_sha256=sha(current['text'].encode()),lines=[current['start'],current['end']],parent_changed=altered,Common_calls=sorted(set(re.findall(r'ExcelCommon\.([\w.]+)',current['text']))),materialized_cases=[r['id'] for r in cases],success_cases=successes,actual_policy_lexical_edges=policy,policy_edge_scope='Exact member-name invocation references,not invented semantic/transitive reachability.',ownership=row['ownership'],host_residual='Excel optional/error/range conversion;all model caches/FIT/DROP/CLEANUP unchanged;no native/Office UI execution'))
assert len(matrix)==90 and len(changed)==14
save('scope/ownership-parity-v1.json',dict(scoped_exports=90,total_exports=268,function_matrix=matrix,changed_public_parents=changed,changed_parent_count=len(changed),ordinary_coverage_holes=holes,changed_parents_have_success_and_adverse=True,existing_portable_paths_retained=['PCA','CA/MCA','KMeans','histogram','ROC projections'],row_policy='No invented physical IDs;model-local,physical AnalysisRowIds,source arrays and permutations retained exactly as observed.',policy_map=pin(ROOT/'evidence/04A39/map/method-policy-map-v1.json'),policy_members=192,extraction_ledger=pin(ROOT/'evidence/04A38/entry-members.json'),gates=GATES))
# Lock actual source methods, including every session/cache producer and original Common member.
members=[];new=[];old_changed=[];lifecycle=[]
for r in freeze['source_delta']:
    path=r['after']['path'];old=scope.methods(PRIVATE/'source-preimages'/path);now=scope.methods(ROOT/path);prior={m['name']+'|'+m['text'].split(') As ')[0]:m for m in old}
    if path.startswith('BESHStat.Excel.Common/'):
        for m in old:
            assert m['text'] in (ROOT/path).read_text(encoding='utf-8-sig'),'EXISTING_COMMON_METHOD_CHANGED'
        # Signature/body matching handles overloads without confusing original members.
        for m in now:
            if not any(m['text']==a['text'] for a in old):new.append(dict(path=path,name=m['name'],method_sha256=sha(m['text'].encode()),lines=[m['start'],m['end']]))
    else:
        for m in old:
            if m['name'].endswith(('_FIT','_DROP','CLEANUP')) or m['name'].startswith('TryGet'):
                n=next(x for x in now if x['name']==m['name']);assert n['text']==m['text'];lifecycle.append(dict(path=path,method=m['name'],sha256=sha(m['text'].encode()),exact=True))
save('scope/portable-member-lock-v1.json',dict(new_methods=new,added_public_methods=len(new),existing_Common_services_extended=4,new_Common_classes=0,existing_Common_method_bodies_changed=False,session_lifecycle=lifecycle,Core_algorithms_changed=False,Windows_GUI_native_renderers_changed=False,all_outside_authorized_compiled_owners_exact=load(HERE/'abi-cil/Debug-proof-v3.json')['all_other_compiled_methods_exact'],gates=GATES))
anchors={
'C17-CA-NOLABEL-INDEX-01':(['CA_FIT','CA_ROWS','CA_ROW_COORD'],'17','GUI no-label j-1 mapping stays dormant;UDF numeric count table route is distinct.','Separate exact dormant-patch/native timing authority and loaded-build capture.'),
'C18-PCA-THRESHOLD-CAPTION-01':(['PCA_FIT','PCA_SUMMARY','PCA_EIGEN'],'18','GUI descriptive > versus Core >= caption boundary retained.','Exact caption/Help decision plus applicable native evidence;no retention change.'),
'C18-MCA-CATEGORY-OWNER-01':(['MCA_CATEGORIES','MCA_COORD'],'18','Duplicate variable/category display names can remain ambiguous;ordered values unchanged.','Exact category identity/label policy and native/output review.'),
'C19-CLUSTER-ROWID-DISPLAY-01':(['KMEANS_ASSIGNMENTS','HCLUST_LEAFORDER','HCLUST_MEMBERSHIP'],'19','Input ordinals,physical source rows,active leaf indices and display permutation remain distinct.','Exact displayed row-ID contract and native before/after evidence.'),
'C20-DA-ROWID-DISPLAY-01':(['DA_CASEWISE','DA_PREDICT','DA_REMOVED'],'20','Casewise/removed display ordinals retained;labels not unique identity keys.','Exact physical-versus-local display decision/native evidence.'),
'C20-FA-ROWID-AMBIGUOUS-01':(['FA_SCORES','FA_SUMMARY'],'20','Physical AnalysisRowIds retained;duplicate supplied IDs cannot infer unique deletion position.','Exact provenance representation policy;do not invent removed ordinals.'),
'C20-DA-PRIOR-NONFINITE-01':(['DA_FIT','DA_SUMMARY','DA_CASEWISE'],'20','Legacy prior validation/model estimates unchanged;nonfinite candidate remains restricted.','Exact scientific/input authority and independent intended output/reference.'),
'C20-DA-FOLD-UPPER-BOUND-01':(['DA_FIT','DA_CONFUSION'],'20','Original lower-only fold validation remains;unbounded stress not executed.','Exact feasible upper-bound/domain authority;no max-integer performance experiment.'),
'C25-REENTRANT-REQUEST-SNAPSHOT-01':(['PS_FIT','PS_TABLE'],'25','GUI capture/DoEvents chronology retained;local typed selection is not a GUI reentrancy correction.','Native synchronous/reentrant execution gate and exact snapshot decision.'),
'C25-LOVE-THRESHOLD-MISSING-01':(['PS_LOVEPLOT_DATA','PS_TABLE'],'25','Legacy NaN-threshold zero cell/missing SMD policy unchanged.','Exact threshold/missing output contract,Help/native chart evidence.'),
'C25-LOVE-AXIS-NOFINITE-01':(['PS_LOVEPLOT_DATA'],'25','Legacy all-missing Love axis fallback retained.','Exact axis policy and native plot evidence.'),
'C25-RAW-TEXT-COVARIATE-IMPORT-01':(['PS_FIT'],'25','Current numeric UDF/formula/raw-text acquisition policies remain distinct.','Exact host input policy and original native/finite gates.'),
'C25-GUI-UDF-DEFAULT-INTENT-01':(['PS_FIT','PS_SUMMARY','PS_TABLE'],'25','GUI intent and UDF defaults are not silently unified.','Exact default-intent decision,Help and scientific/native evidence when applicable.'),
'C14-INPUT-FINITE-01':(['CLASS_CALIB','CALIB_POINTS','PS_FIT'],'14','Original finite rejection/filter/order semantics retained.','Exact input/diagnostic/native authority;no global finite rescue.'),
'UDF38-D42-FA-FACTORABILITY':(['FA_FACTORABILITY'],'38','Actual ordinary original/current IndexOutOfRangeException preserved;not a successful numeric output.','Separate exact row-bound correction decision with original/corrected payload and relevant report review.'),
'UDF38-D42-PLOT-MODEL':(['HIST_BINS','HIST_NORMAL','ROC_POINTS','ROC_STATS','KM_CURVE','CALIB_POINTS'],'38','Existing Core geometry,endpoint/tail/CI/bin defaults retained;only portable payload orchestration moved.','Further scientific/bin/plot method changes require exact separate authority;PrettyBreaks75 remains out of scope.')}
issues=[]
for alias,(names,batch,observation,gate) in anchors.items():
    c=next(i for i in register['issues'] if alias in i['aliases']);case_ids=[r['id'] for r in before['rows'] if r['method'].rsplit('.',1)[-1] in names];actual=[r for r in before['rows'] if r['id'] in case_ids]
    dossier='docs/architecture/batch-04A-'+batch+'-owner-review.md';originals=[r for r in c['discovery_provenance'] if (ROOT/r['path']).is_file()]
    proofs=[]
    for r in originals:
        # Pin live bytes and original authority separately;no historical record mutation.
        proofs.append(dict(original_record_pin=r,current_source=pin(r['path'])))
    issues.append(dict(canonical_id=c['canonical_id'],aliases=c['aliases'],canonical_record_sha256=sha(json.dumps(c,sort_keys=True).encode()),original_status=c['status'],original_provenance=proofs,owner_review=pin(dossier) if (ROOT/dossier).is_file() else None,original_control_count=len(c['original_controls']),original_obligation_count=len(c['original_obligations']),current_sources=[dict(function=m['function'],source=m['after_source'],method_sha256=m['after_method_sha256']) for m in matrix if m['function'] in names],materialized_input_and_output_cases=case_ids,original_current_pair_fingerprints=[dict(id=r['id'],before=sha(json.dumps(compare.canonical(r),sort_keys=True).encode()),after=sha(json.dumps(compare.canonical(next(a for a in after['rows'] if a['id']==r['id'])),sort_keys=True).encode())) for r in actual],observed_current=observation,selected_policy='PRESERVE_ACCEPTED_CORE_AND_HOST_BEHAVIOR',interim_decision='BEHAVIOR_PRESERVING_IMPLEMENTED' if alias=='UDF38-D42-PLOT-MODEL' else 'INVESTIGATED_RESTRICTED',authority='Owner42 permits portable payload extraction only;no correction/waiver/native or scientific promotion.',correction_authorized=False,required_exact_future_decision=gate,owners=['Core numerical/geometry','Common typed request/payload/provenance','Windows acquisition/session/native report'],original48_exit_status=None))
assert len(issues)==16
save('register/issue-delta-v1.json',dict(kind='04A42_INTERIM_DELTA;NOT_04A48_CLOSURE',canonical39=pin(ROOT/'evidence/04A39/register/defects-v7.json'),accepted40=pin(ROOT/'evidence/04A40/register/issue-delta-v4.json'),accepted41=pin(ROOT/'evidence/04A41/register/issue-delta-v2.json'),counts=dict(canonical_entries=1169,original_obligations=1514,restricted_controls=802,mappings=2967),issues=issues,original_controls_discharged=0,original_obligations_discharged=0,scientific_corrections_activated=0,native_changes=0,new_canonical_ids=0,original_fixture=pin(PRIVATE/'fixtures-before-v1/observations.json'),current_fixture=pin(PRIVATE/'fixture-final3-Debug-42/observations.json'),integrated_regressions_corrected=pin(HERE/'matrix/failed-freeze-v1-disposition.json'),gates=GATES))
save('fixtures/current-main-review-v1.json',dict(kind='SANITIZED_TYPED_COMPATIBILITY_REVIEW;NOT_RAW_TRANSCRIPT_OR_INDEPENDENT_GOLDEN',raw=pin(PRIVATE/'fixtures-before-v1/observations.json'),data=compare.canonical(before),normalization=['Session GUID identity text','Created UTC timestamp text'],statistical_values_shapes_errors_order_unchanged=True,gates=GATES))
print('90 functions,14 changed parents,192 policy map,source/cache locks and16 canonical interim investigations recorded',flush=True)
