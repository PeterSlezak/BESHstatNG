"""04A.01 approved inventory profile v1; never full-contract or final owner acceptance.

Exit 0: approved inventory criteria evidenced. Exit 1: integrity failure.
The independent, unchanged original Roslyn --check --strict remains authoritative
for full contracts and returns 2 while controls/edges are incomplete.
"""
from __future__ import annotations
import argparse,collections,copy,json,pathlib,sys
import continuation30_report as c
R=c.R;L=c.L;T=c.T;H=R/'TestResults/04A-01-owner-closeout-20261030'
PLAN='planning/BESHStatNG_CrossPlatform_MasterPlan.md'
OWNER=L/'04A01-owner-approvals-20261030.md'
CURRENT=c.CURRENT
def read(p):return c.read(p)
def emit(p,v):
    p.parent.mkdir(parents=True,exist_ok=True)
    with p.open('x',encoding='utf-8') as f:json.dump(v,f,indent=2,ensure_ascii=False);f.write('\n')
def metadata(p):
    s=p.stat();return dict(path=p.relative_to(R).as_posix(),bytes=s.st_size,mtime_ns=s.st_mtime_ns,ctime_ns=s.st_ctime_ns,ino=s.st_ino,dev=s.st_dev)
def metadata_errors(before,root=R):
    out=[]
    for e in before:
        p=root/e['path']
        if not p.is_file():out.append('Historical deletion '+e['path']);continue
        s=p.stat()
        if (s.st_size,s.st_mtime_ns,s.st_ctime_ns,s.st_ino,s.st_dev)!=tuple(e[k] for k in ('bytes','mtime_ns','ctime_ns','ino','dev')):out.append('Historical metadata/write drift '+e['path'])
    return out
def policy():return dict(schema='04A.01-owner-approved-inventory-profile-v1',version='1.0',profile='inventory-complete-with-approved-deferrals',criteria_approval='OWNER_APPROVED_2026-10-30',approval_record=OWNER.relative_to(R).as_posix(),implemented=True,inventory_profile_enabled=True,official_batch_accepted=False,final_owner_acceptance='PENDING_DISTINCT_OWNER_SIGN_OFF',original_strict_profile='strict-full-contract',original_strict_exit=2,integrity_failure_exit=1,**{'04A02_authorized':False},native_capture_authorized=False,blanket_P0_waiver=False,retention='ALL_HISTORICAL_TESTRESULTS_IN_PLACE_THROUGH_END_OF_PHASE_04A',cleanup_authorized=False,gate_c2='UNMET',E1_E7='OPEN',GC3_GC8='DEFERRED')
def dispositions():return dict(Cox=dict(edge_id=list(c.EXCEPTIONS)[0],decision='unimplemented; removal approved; implementation not performed',risk='P0',batch='04A.47'),RefEdit=dict(edge_id=list(c.EXCEPTIONS)[1],decision='investigation deferred with restrictions; no source-only safety waiver',risk='UNKNOWN_HIGH',batch='04A.27',dependencies=['04A.45','04A.46','04A.47']),Progress=dict(edge_id=list(c.EXCEPTIONS)[2],decision='investigation deferred with restrictions; consumer/lifetime remains unverified',risk='P0',batch='04A.46'),Preservation=dict(issue='C30-PRES-001',decision='owner accepts disclosed historical overwrite-protocol exception; no assertion of no-overwrite compliance',future_overwrite_authorized=False),R=dict(decision='retain all historical TestResults in existing paths through end of Phase 04A',cleanup_authorized=False,provisional_04A76='future proposal only; not added to approved 75-batch roadmap'))
def approved_obligations():
    rs=copy.deepcopy(c.obligations())
    for r in rs:
        r.update(owner_decision_status='CLASSIFICATION_AND_RESTRICTED_DEFERRAL_APPROVED',project_owner_signatory=None,approved_at='2026-10-30 (owner-stated date; time unspecified)',deferral_accepted=True,approval_authority='project_owner',approval_record=OWNER.relative_to(R).as_posix(),signature_basis='explicit owner instructions supplied in this session; no agent signature',final_batch_acceptance=False)
    return rs
def policy_errors(v):return [] if v==policy() else ['Unapproved profile/acceptance/native/cleanup or altered criteria']
def disposition_errors(v):return [] if v==dispositions() else ['Missing/changed exceptional disposition or retention decision']
def record_errors(rs,expected=None):
    expected=approved_obligations() if expected is None else expected;by={r['obligation_id']:r for r in expected};ids=[r.get('obligation_id') for r in rs];errors=[]
    if len(ids)!=len(set(ids)) or set(ids)!=set(by):errors.append('Missing/foreign/duplicate obligation ID')
    for r in rs:
        if r!=by.get(r.get('obligation_id')):errors.append('Source/risk/block/qualification/owner/batch/fixture/approval drift '+str(r.get('obligation_id')))
    return errors
def protected_errors(before):
    errors=[]
    for label in ('tracked','gate','compiled'):errors+=c.artifact_errors(before[label])
    for e in before['baseline']['entries']:
        if c.sha(R/e['retained_path'])!=e['sha256']:errors.append('Original baseline drift '+e['retained_path'])
    for e in before['decision_prefixes']:
        if c.sha(T/(e['store']+'.json'))!=e['sha256']:errors.append('Literal decision prefix drift '+e['store'])
    errors+=c.artifact_errors(before['original_artifacts'],H/'original')
    errors+=c.artifact_errors([e for e in before['original_artifacts'] if e['path'] not in CURRENT])
    if c.a.git('branch','--show-current')!=before['branch'] or c.a.git('rev-parse','HEAD')!=before['head'] or c.a.git('rev-parse','main')!=before['head']:errors.append('Git branch/head drift')
    if c.a.git('diff','--name-only')!=PLAN or c.a.git('diff','--cached','--name-only'):errors.append('Unapproved tracked change or staging')
    return errors
def check(history=False,index=False):
    before=read(H/'before.json');errors=protected_errors(before)
    errors+=policy_errors(read(L/'04A01-inventory-profile-v1.json'))
    errors+=disposition_errors(read(L/'04A01-approved-dispositions.json'))
    rs=read(L/'04A01-approved-deferred-obligations.json');errors+=record_errors(rs)
    if c.sha(OWNER)!=read(H/'authority.json')['owner_record_sha256']:errors.append('Owner authority record drift')
    for g in 'ABC':errors+=c.group_errors(g)
    census=c.reconciliation()
    for key,count in dict(source_files=171,source_member_ids=3866,reviewed_member_ids=3866,UI_member_ids=1547,exports=268,controls=2056,qualified_controls=1164,incomplete_controls=892,original_unresolved_edges=622,intent_sidecar_rows=622,existing_behavior_groups=124,live_control_golden_blockers=802,structural_investigations=0).items():
        if census[key]!=count:errors.append('Inventory census drift '+key)
    for name,field,count in [('symbols.csv','symbol_id',3866),('ui-controls.csv','control_id',2056),('continuation28-control-triage.csv','control_id',892),('continuation29-wiring-intent-triage.csv','edge_id',622)]:
        values=c.old(name)
        if len(values)!=count or len({r[field] for r in values})!=count:errors.append('Missing/foreign/duplicate inventory '+name)
    actual_plan=c.plan();prior_plan=c.catalog.load(H/'original/docs/architecture/WindowsMigrationLedger','continuation30-implementation-batch-crosswalk.csv')
    if len(actual_plan)!=75 or [{k:r[k] for k in ('batch_id','approved_objective','dependencies')} for r in actual_plan]!=[{k:r[k] for k in ('batch_id','approved_objective','dependencies')} for r in prior_plan]:errors.append('Numbered roadmap/objectives/dependencies changed')
    if history:
        errors+=metadata_errors(read(H/'history-metadata.json'))
        actual={p.relative_to(R).as_posix() for p in (R/'TestResults').rglob('*') if p.is_file() and not p.is_relative_to(H)}
        if actual!={e['path'] for e in read(H/'history-hashes.json')}:errors.append('Historical membership drift')
    if index:
        errors+=c.artifact_errors(read(L/'index.json')['artifacts'])
        errors+=c.artifact_errors(read(H/'published-artifacts.json'))
    if errors:raise ValueError(errors)
    return dict(status='PASS',profile='inventory-complete-with-approved-deferrals',version='1.0',inventory_criteria_approved=True,official_batch_accepted=False,final_owner_acceptance='PENDING',source_members=3866,UI_members=1547,exports=268,qualified_controls=1164,incomplete_controls=892,full_unqualified_edges=622,intent_outcomes=[458,161,1,2],obligations=len(rs),behavior_groups=124,live_golden_blockers=802,new_full_qualifications=0,original_strict_exit=2,gate_c2='UNMET',**{'04A02_authorized':False})
def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--profile',choices=['inventory-complete-with-approved-deferrals'],required=True);p.add_argument('--history',action='store_true');p.add_argument('--index',action='store_true');a=p.parse_args()
    try:print(json.dumps(check(a.history,a.index),indent=2));return 0
    except (AssertionError,ValueError,KeyError,OSError) as e:print(json.dumps(dict(status='INTEGRITY_FAILURE',error=str(e))));return 1
if __name__=='__main__':sys.exit(main())
