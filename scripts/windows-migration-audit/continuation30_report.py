"""C30 dossier integrity/projection only. This implements no acceptance profile."""
from __future__ import annotations
import argparse,collections,csv,hashlib,io,json,pathlib,re,sys
R=pathlib.Path.cwd();T=R/'scripts/windows-migration-audit';sys.path.insert(0,str(T))
import amendment as a,catalog,semantic
L=catalog.DEFAULT;H=R/'TestResults/04A-01-continuation-30'
CURRENT={'docs/architecture/WindowsMigrationLedger/'+n for n in ('README.md','reconciliation.md','index.json','proposed-tracked-files.json','future-staging.txt')}
EXCEPTIONS={
 'BESHstatNG/src/UI/Ui4Cox.Designer.vb|WithEvents|ckResidualPlots@873':('P0','04A.47','owner-decision-required'),
 'BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb|WithEvents|xlApp@34':('UNKNOWN_HIGH','04A.27','insufficient-evidence-blocker'),
 'BESHstatNG/src/UI/Ui0OneRefeditMulticol.Designer.vb|WithEvents|progressBarExactCalc@597':('P0','04A.46','insufficient-evidence-blocker')}
def read(p):return json.loads(p.read_bytes())
def sha(p):
    with p.open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()
def digest(v):return hashlib.sha256(json.dumps(v,sort_keys=True,ensure_ascii=False).encode()).hexdigest()
def pin(p):return dict(path=p.relative_to(R).as_posix(),bytes=p.stat().st_size,sha256=sha(p))
def old(n):return catalog.load(H/'original/docs/architecture/WindowsMigrationLedger',n)
def cell(v):return json.loads(v) if v else None
def csv_bytes(rs):
    s=io.StringIO(newline='');w=csv.DictWriter(s,fieldnames=list(rs[0]),lineterminator='\n');w.writeheader()
    for r in rs:w.writerow({k:json.dumps(v,sort_keys=True,ensure_ascii=False) if isinstance(v,(dict,list,bool)) or v is None else v for k,v in r.items()})
    return s.getvalue().encode()
def allowed(path):
    p=pathlib.PurePosixPath(path)
    if p.is_absolute() or '..' in p.parts:return False
    return path in CURRENT or path.startswith('TestResults/04A-01-continuation-30/') or bool(re.fullmatch(r'docs/architecture/WindowsMigrationLedger/continuation30[.-][^/]+',path)) or path in ('scripts/windows-migration-audit/continuation30_report.py','scripts/windows-migration-audit/test_continuation30.py')
def artifact_errors(es,root=R):return ['Missing/size/hash drift '+e['path'] for e in es if not (root/e['path']).is_file() or (root/e['path']).stat().st_size!=e['bytes'] or sha(root/e['path'])!=e['sha256']]
def history_continuity(entries,metadata,root=R):
    """Reuse the fresh full hash seal only for identical file identity/metadata.

    Changed metadata triggers a real content hash, never an assumed pass. This
    is not represented as a second full byte scan or an archive authorization.
    """
    meta={m['path']:m for m in metadata};errors=[];rehashed=[]
    for e in entries:
        p=root/e['path'];m=meta[e['path']]
        if not p.is_file():errors.append('Historical deletion '+e['path']);continue
        s=p.stat()
        if (s.st_size,s.st_mtime_ns,s.st_ctime_ns,s.st_ino,s.st_dev)!=(m['bytes'],m['mtime_ns'],m['ctime_ns'],m['ino'],m['dev']):
            rehashed.append(e['path'])
            if s.st_size!=e['bytes'] or sha(p)!=e['sha256']:errors.append('Historical content drift '+e['path'])
    return errors,rehashed
def preservation_claim_errors(proof):
    errors=[]
    if proof.get('rehashed_changed_metadata') and proof.get('historical_write_protocol')=='NO_HISTORY_WRITES':errors.append('Changed history metadata cannot support a no-writer claim')
    if proof.get('historical_rewrite_authorized') is not False:errors.append('No C30 authorization for historical rewrites')
    return errors
def plan():
    text=(R/'planning/BESHStatNG_CrossPlatform_MasterPlan.md').read_text(encoding='utf-8-sig')
    rs=[dict(batch_id=m[0],approved_objective=m[1],dependencies=m[2],implementation_authorized=False,status='PLANNED; no C30 approval') for m in re.findall(r'^\| \*\*(04A\.\d{2})\*\* \| (.*?) \| (.*?) \|',text,re.M)]
    assert {r['batch_id'] for r in rs}=={'04A.'+str(n).zfill(2) for n in range(1,76)} and len(rs)==75
    return rs
def obligations():
    risks=old('continuation28-control-triage.csv');intents=old('continuation29-wiring-intent-triage.csv');controls={r['control_id']:r for r in old('ui-controls.csv')};out=[]
    risk_fields='risk_tier risk_justification affected_behavior mechanism review_confidence unresolved_missing_fact mitigation_or_safe_capture_constraint'.split()
    for s in risks:
        ident=s['control_id'];linked=[e['edge_id'] for e in intents if ident in cell(e['control_ids'])]
        assert controls[ident]['review_contract']=='unreviewed'
        out.append(dict(obligation_id='control:'+ident,id_kind='control',original_id=ident,source_record_path='docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv',source_record_sha256=digest(s),source_evidence=cell(s['source_pin']),linked_control_ids=[ident],linked_edge_ids=linked,original_review_contract=controls[ident]['review_contract'],unsupported_contract=s['prior_unsupported_contract_field'],risk_tiers=[s['risk_tier']],risk_basis={k:s[k] for k in risk_fields},source_intent_outcome='C28 risk triage; incomplete full control contract',source_intent_classification=s['evidence_level'],owner_roles=[s['owner_role']],earliest_approved_batches=[s['target_approved_batch']],dependent_batches=cell(s['downstream_dependencies']),named_fixture=s['named_test_fixture'],blocked_gate=s['target_gate'],affected_live_golden_blocked=s['is_04A02_goldens_blocker']=='true',safe_capture_preconditions=s['mitigation_or_safe_capture_constraint'],acceptance_condition='Exact bounded per-ID restrictions, named owner and fixture; P0/UNKNOWN_HIGH require mitigation + executed regression + explicit project-owner sign-off before affected live work. P1 validation before dependent capture/extraction. P2/P3 retain Gate C2.',verification_status='NOT RUN',owner_decision_status='PENDING_OWNER_DECISION',project_owner_signatory=None,approved_at=None,deferral_accepted=False,full_contract_newly_qualified=False,capture_authorized=False))
    for s in intents:
        ident=s['edge_id'];out.append(dict(obligation_id='edge:'+ident,id_kind='edge',original_id=ident,source_record_path='docs/architecture/WindowsMigrationLedger/continuation29-wiring-intent-triage.csv',source_record_sha256=digest(s),source_evidence={k:cell(s[k]) if k=='member_pins' else s[k] for k in ('path','declaration_line','declaration_text','source_sha256','original_row_sha256','member_pins')},linked_control_ids=cell(s['control_ids']),linked_edge_ids=[ident],original_review_contract='unqualified original edge; '+s['original_target_status'],unsupported_contract=s['unresolved_mechanism'],risk_tiers=cell(s['risk_tiers']),risk_basis=dict(C28_risks=cell(s['C28_risks']),C28_risk_explanations=cell(s['C28_risk_explanations']),independent_edge_risk_justification=s['independent_edge_risk_justification']),source_intent_outcome=s['review_outcome'],source_intent_classification=s['source_intent_classification'],owner_roles=cell(s['owner_roles']),earliest_approved_batches=cell(s['approved_target_batches']),dependent_batches=['04A.27 source/selection identity','04A.28 output mutation','04A.45–47 host wiring/lifetime','04A.48 / Gate C2 extraction/parity closure'],named_fixture=s['named_test_fixture'],blocked_gate=s['target_gate'],affected_live_golden_blocked=cell(s['affected_live_golden_blocked']),safe_capture_preconditions=s['safe_capture_preconditions'],acceptance_condition=s['owner_action']+' Source-intent accounting is not a full edge contract.',verification_status='NOT RUN',owner_decision_status='PENDING_OWNER_DECISION',project_owner_signatory=None,approved_at=None,deferral_accepted=False,full_contract_newly_qualified=False,capture_authorized=False))
    return out
def profile():return dict(schema='04A.01-C30-acceptance-proposal-v1',version='C30.1',proposed_profile='inventory-complete-with-approved-deferrals',approval_status='PENDING',approval_authority='project_owner',approved_at=None,signatory=None,decision_status='PENDING_OWNER_DECISION',effective_profile='original_strict',implemented=False,effective=False,implementation_authorized=False,**{'04A02_authorized':False},original_profile='strict-full-contract',original_strict_exit=2,integrity_failure_exit=1,gate_c2='UNMET',E1_E7='OPEN',GC3_GC8='DEFERRED',full_control_qualified=1164,incomplete_controls=892,full_edge_additions=0,original_unresolved_edges=622,source_intent_accounted=622,blanket_P0_waiver=False,live_golden_blockers=802,owner_choices=['accept complete source inventory under separately amended criteria with specified restrictions','return for further evidence','reject amendment'],selected_choice=None,subsequent_steps=['Separately approve and authorize versioned policy/checker implementation and explicit04A.01 acceptance record','Separately authorize official04A.02 and exact safe case scope; no implication from amendment decision'],cleanup_authorized=False)
def policy_errors(p):return ['Unapproved/altered effective profile or blanket waiver'] if p!=profile() else []
def record_errors(rs,expected=None):
    expected={r['obligation_id']:r for r in obligations()} if expected is None else expected;errors=[];ids=[r.get('obligation_id') for r in rs]
    if len(ids)!=len(set(ids)) or set(ids)!=set(expected):errors.append('Missing/duplicate/foreign control or edge composite ID')
    for r in rs:
        ident=r.get('obligation_id');e=expected.get(ident)
        if e is None:continue
        if r!=e:errors.append('Original record/risk/block/owner/batch/fixture/unsigned qualification drift '+str(ident))
        if any(not r.get(k) for k in ('owner_roles','earliest_approved_batches','named_fixture','risk_tiers','unsupported_contract','source_evidence','acceptance_condition','safe_capture_preconditions')):errors.append('Unowned or unbounded obligation '+ident)
        if any(not re.fullmatch(r'04A\.(?:0[3-9]|1[0-9]|2[0-8]|3[1-6]|4[4-7])',b) for b in r.get('earliest_approved_batches',[])):errors.append('Invented batch/closure catch-all '+ident)
    return errors
def exception_entries():
    by={r['original_id']:r for r in obligations() if r['id_kind']=='edge'};out=[]
    for ident,(tier,batch,outcome) in EXCEPTIONS.items():
        r=by[ident];assert r['risk_tiers']==[tier] and r['earliest_approved_batches']==[batch] and r['source_intent_outcome']==outcome
        out.append(dict(edge_id=ident,risk=tier,source_outcome=outcome,owner_roles=r['owner_roles'],primary_batch=batch,dependencies=['04A.45','04A.46','04A.47','04A.48 / Gate C2'] if tier=='UNKNOWN_HIGH' else ['04A.48 / Gate C2'],named_fixture=r['named_fixture'],decision='PENDING_OWNER_DECISION',signatory=None,approved_at=None,mitigation_executed=False,source_resolved=False,live_use_authorized=False))
    return out
def reconciliation():
    ss=old('symbols.csv');cs=old('ui-controls.csv');es=old('ui-wiring.csv');risks=old('continuation28-control-triage.csv');ints=old('continuation29-wiring-intent-triage.csv');sem=read(H/'original/docs/architecture/WindowsMigrationLedger/semantic-review-coverage.json');cat=read(H/'original/docs/architecture/WindowsMigrationLedger/catalog-coverage.json')
    byfile=old('review-by-file.csv');ribbon=old('ribbon-crosswalk.csv')
    return dict(source_files=len(old('files.csv')),source_member_ids=len(ss),reviewed_member_ids=sem['qualified_source_reviews'],UI_member_ids=sum(s['family']=='UI' for s in ss),active_member_ids=sum(s['profile']=='project MyType=Windows' for s in ss),alternative_member_ids=sum(s['profile']!='project MyType=Windows' for s in ss),structural_investigations=sum(s['disposition']=='needs-investigation' for s in ss),review_by_file=dict(files=len(byfile),parsed=sum(int(s['parsed_records']) for s in byfile),reviewed=sum(int(s['manually_reviewed_static_records']) for s in byfile),runtime_or_semantic_parity_proven=sum(int(s['runtime_or_semantic_parity_proven']) for s in byfile)),ribbon=dict(buttons=len(ribbon),unique_ids=len({s['ribbon_id'] for s in ribbon}),distinct_actions=len({s['onAction'] for s in ribbon})),qualified_controls=sum(s['review_contract']=='04A.01-control-v1' for s in cs),controls=len(cs),incomplete_controls=len(risks),risk_distribution=dict(collections.Counter(s['risk_tier'] for s in risks)),live_control_golden_blockers=sum(s['is_04A02_goldens_blocker']=='true' for s in risks),existing_behavior_groups=len({s['risk_operation_id'] for s in risks}),wiring_rows=len(es),original_unresolved_edges=sum(s['target_status']=='no-Handles-found; decorative/default/indirect intent unresolved' for s in es),intent_sidecar_rows=len(ints),intent_outcomes=dict(collections.Counter(s['review_outcome'] for s in ints)),intent_categories=dict(collections.Counter(s['intent_category'] for s in ints)),intent_source_status=dict(collections.Counter(s['source_intent_classification'] for s in ints)),new_full_control_contracts=0,new_full_edge_contracts=0,exports=len(old('udf-exports.csv')),catalog=cat,obligations=1514,obligation_kinds={'control':892,'edge':622},plan_batches=len(plan()),member_dispositions=dict(collections.Counter(s['disposition'] for s in ss)),runtime_parity='NOT ESTABLISHED by source ownership; native/GUI/scientific fixture obligations remain open',strict_exit=2,gate_c2='UNMET',approval_status='PENDING_OWNER_DECISION')
def group_errors(g):
    rec=reconciliation();errors=[]
    if g=='A':
        for key,value in dict(source_files=171,source_member_ids=3866,reviewed_member_ids=3866,UI_member_ids=1547,active_member_ids=3864,alternative_member_ids=2,structural_investigations=0,exports=268,plan_batches=75).items():
            if rec[key]!=value:errors.append('Source/authority census drift '+key)
        if any(rec['catalog'][k]!=v for k,v in dict(methods=76,commands=5,pages=2,registry_handler_instances=4).items()) or rec['catalog']['registered_analysis_ids']!=['anova.oneway','nonparametric.kruskal-wallis']:errors.append('Catalog/registry boundary drift')
        if rec['review_by_file']!=dict(files=171,parsed=3866,reviewed=3866,runtime_or_semantic_parity_proven=0) or rec['ribbon']!=dict(buttons=84,unique_ids=84,distinct_actions=81):errors.append('By-file/Ribbon census or false runtime parity')
        ss=old('symbols.csv')
        if len(ss[0])!=81 or len({s['symbol_id'] for s in ss})!=3866 or not all(semantic.qualified(s) for s in ss):errors.append('Member schema/ownership qualification drift')
    if g in ('B','C'):
        rs=[r for r in obligations() if r['id_kind']==('control' if g=='B' else 'edge')];errors+=record_errors(rs,{r['obligation_id']:r for r in rs})
        if g=='B' and (rec['risk_distribution']!=dict(P0=385,P1=416,P2=78,P3=12,UNKNOWN_HIGH=1) or rec['live_control_golden_blockers']!=802 or rec['existing_behavior_groups']!=124):errors.append('C28 risk/group/blocker drift')
        if g=='C':
            if rec['intent_outcomes']!={'source-resolved-intent':458,'documented-deferred':161,'owner-decision-required':1,'insufficient-evidence-blocker':2} or rec['original_unresolved_edges']!=622:errors.append('C29 intent/full-contract conflation')
            if rec['intent_categories']!={'container/parent-managed':71,'designer-or-runtime-bound-state':84,'host-refedit/selection-owned':67,'value-input-consumed-by-compute':351,'display-only/inert':36,'delegate-dynamically-wired':11,'unbounded/unknown':2} or rec['intent_source_status']!={'source-supported':551,'partially-source-supported':68,'unknown':3} or rec['wiring_rows']!=19701:errors.append('C29 category/source-status census drift')
            exception_entries()
    if g=='D':
        errors+=policy_errors(read(L/'continuation30-acceptance-profile-proposal.json'));errors+=record_errors(read(H/'obligations.json'))
        errors+=safe_scope_errors(read(L/'continuation30-safe-fixture-candidates.json'))
    return errors
def safe_scope_errors(v):
    errors=[]
    if v.get('04A02_authorized') is not False or v.get('status')!='PROPOSAL ONLY; NOT RUN' or v.get('live_capture_authorized') is not False:errors.append('Invented golden/04A.02 authorization')
    cases=v.get('cases',[])
    if {r.get('case_id') for r in cases}!={'C30-SYN-ANOVA','C30-SYN-KW'} or len(cases)!=2:errors.append('Missing/duplicate/invented synthetic case')
    for r in cases:
        if r.get('executed') is not False or r.get('reference_output') is not None or r.get('capture_authorized') is not False or r.get('scientific_admission')!='PENDING_DOMAIN_OWNER_AND_INDEPENDENT_ORACLE':errors.append('Invented native/R/scientific golden or universal Core safety')
        if not r.get('owner_role') or not r.get('source_pins') or not r.get('scientific_missing_fact') or not r.get('source_row_ids'):errors.append('Unowned/unpinned scientific candidate')
    return errors
def guard(group=None,continuity=False):
    b=read(H/'before.json');errors=[]
    for label in ('tracked','gate','compiled'):errors+=artifact_errors(b[label])
    errors+=artifact_errors(b['original_artifacts'],H/'original')
    for e in b['original_artifacts']:
        if e['path'] not in CURRENT:errors+=artifact_errors([e])
    if a.git('branch','--show-current')!=b['branch'] or a.git('rev-parse','HEAD')!=b['head'] or a.git('rev-parse','main')!=b['main'] or a.git('diff','--name-only') or a.git('diff','--cached','--name-only'):errors.append('Branch/source/staging drift')
    for e in b['decision_prefixes']:
        if sha(T/(e['store']+'.json'))!=e['sha256']:errors.append('Literal decision bytes/order changed')
    groups=group or 'ABCD'
    for g in groups:
        if g!='E':errors+=group_errors(g)
        marker=H/('checkpoint-'+g+'.json')
        if marker.exists():errors+=artifact_errors(read(marker)['immutable_evidence'])
        else:errors.append('Missing C30 checkpoint marker '+g)
    proof=None
    if continuity:
        hs=read(H/'prior-raw-evidence-hashes.json')['files'];er,rehashed=history_continuity(hs,read(H/'preexisting-file-metadata.json'));errors+=er
        proof=dict(full_hash_scan='C30 preflight complete; every historical file SHA-256 recomputed',final_check='All original paths/file identities/sizes/write+creation metadata checked; changed metadata requires actual rehash. No claim of a second full byte scan.',files=len(hs),rehashed_changed_metadata=rehashed,errors=len(er),manifest=pin(H/'prior-raw-evidence-hashes.json'),metadata=pin(H/'preexisting-file-metadata.json'),historical_writers='No absence-of-writer claim. Inspect changed metadata and the preservation-protocol deviation record; TRX routing alone does not isolate test-internal fixture writers.',historical_write_protocol='DEVIATION_PENDING_OWNER_REVIEW' if rehashed else 'NO_METADATA_CHANGE_OBSERVED',historical_rewrite_authorized=False)
        errors+=preservation_claim_errors(proof)
    if errors:raise AssertionError(errors)
    return dict(status='PASS',scope='Dossier integrity only; not an acceptance profile',groups=groups,strict_exit=2,approval_status='PENDING_OWNER_DECISION',history_continuity=proof)
def write(name,raw,check=False):
    p=L/name
    if check:
        if not p.is_file() or p.read_bytes()!=raw:raise AssertionError('C30 projection drift '+name)
    else:p.write_bytes(raw)
def render(check=False):
    guard();rs=obligations();rec=reconciliation()
    b=read(H/'before.json');rec.update(protection=dict(before_counts={k:len(b[k]) for k in ('tracked','gate','compiled')},after_counts={k:len(b[k]) for k in ('tracked','gate','compiled')},source_hash_guard='PASS',initial_index_sha256=b['index_sha256'],historical_full_hash_manifest=pin(H/'prior-raw-evidence-hashes.json')),duplicated_or_lost_obligation_ids=0,original_decision_prefix_counts=[e['objects'] for e in b['decision_prefixes']],implemented_profile=False,effective_profile='original_strict',**{'04A02_authorized':False})
    values={'continuation30-reconciliation.json':(json.dumps(rec,indent=2,ensure_ascii=False)+'\n').encode(),'continuation30-deferred-obligation-crosswalk.csv':csv_bytes(rs),'continuation30-implementation-batch-crosswalk.csv':csv_bytes(plan()),'continuation30-three-exception-decisions.json':(json.dumps(exception_entries(),indent=2,ensure_ascii=False)+'\n').encode()}
    for n,v in values.items():write(n,v,check)
    if check:
        guard('E')
        errors=artifact_errors(read(H/'publication-artifacts.json'))
        if errors:raise AssertionError(errors)
    return dict(status='PASS; C30 EVIDENCE COHERENCE ONLY',control_obligations=892,edge_obligations=622,full_contract_qualified_additions=0,approval_status='PENDING_OWNER_DECISION',original_strict_exit=2)
if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--guard',action='store_true');p.add_argument('--group',choices=list('ABCDE'));p.add_argument('--continuity',action='store_true');p.add_argument('--check',action='store_true');args=p.parse_args();print(json.dumps(guard(args.group,args.continuity) if args.guard else render(args.check),indent=2))
