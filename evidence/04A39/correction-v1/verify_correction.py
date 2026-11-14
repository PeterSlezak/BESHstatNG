"""Independent fail-closed evidence reader. A repaired derivative is not a repository PASS."""
from common import *
import collections

def verify_documents(parents,changes,focused,privacy,controls):
    rows=parents['parents'];ids=[r['id'] for r in rows]
    assert len(rows)==len(set(ids))==956,'PARENT_IDENTITY_LOSS_OR_DUPLICATE'
    assert collections.Counter(r['w3_outcome'] for r in rows)==dict(PASS=830,FAIL=113,ERROR=13),'W3_BASELINE_CHANGED'
    assert collections.Counter(r['original39_outcome'] for r in rows)==dict(PASS=833,FAIL=112,ERROR=11),'39_OUTCOME_CHANGED'
    assert not parents['missing'] and not parents['duplicate']
    drifts=changes['changes'];keys=[(r['identity'],r['occurrence']) for r in drifts]
    assert len(drifts)==len(set(keys))==54,'MISSING_CHANGED_DIAGNOSTIC'
    assert all(r['before_outcome']==r['current_outcome']=='FAIL' and r['trace_difference_only_assertion_message'] and not r['functional_behavior_change_detected'] for r in drifts),'SUBSTANTIVE_DIAGNOSTIC_DRIFT'
    assert set(r['change_id'] for r in focused['changed_diagnostics'])==set(r['change_id'] for r in drifts),'MISSING_FOCUSED_DIAGNOSTIC'
    assert focused['executed']==26 and focused['outcomes']==dict(PASS=3,FAIL=23) and focused['command_exit']==1,'FOCUSED_OUTCOME_DRIFT'
    assert all(r['message_exact'] and r['outcome']=='FAIL' for r in focused['changed_diagnostics'])
    assert privacy['per_parent_findings']==1941 and privacy['original_snapshot_retained']==1599 and privacy['new_since_snapshot']==342,'PRIVACY_POPULATION_DRIFT'
    assert len(privacy['findings'])==len({r['finding_id'] for r in privacy['findings']})==1941,'PRIVACY_FINDING_LOSS'
    assert controls['failures']==controls['errors']==controls['exit_code']==0 and controls['testsRun']==12,'BOUNDARY_CONTROLS_ABSENT'
    return dict(status='BLOCKED_04A39_OWNER_DECISION_REQUIRED',reason='Historical repository-candidate privacy FAILs remain unwaived while immutable raw records are preserved. New derivative safety is a separate gate.',
        diagnostics='54_EXPLAINED_UNWAIVED',privacy='ROOT_CANDIDATE_FAIL_UNWAIVED',gates=GATES)

def local():
    lock()
    result=verify_documents(load(HERE/'diagnostics/parent-956-reconciliation.json'),load(HERE/'diagnostics/changed-54-crosswalk.json'),load(HERE/'diagnostics/focused-reconciliation.json'),load(HERE/'privacy/focused-finding-reconciliation.json'),load(HERE/'validation/reporting-boundary-tests.json'))
    original=load(PRIOR/'python-node-scanners/python-identities-v1.json')
    byevent={(r['identity'],r['occurrence']):r for r in original['events']}
    for row in load(HERE/'diagnostics/changed-54-crosswalk.json')['changes']:
        event=byevent[(row['identity'],row['occurrence'])]
        assert sha(event['message'].encode())==row['current_message_sha256']
        assert sha(event['previous']['message'].encode())==row['before_message_sha256']
        assert sha(event['traceback'].encode())==row['current_trace_sha256']
        assert sha(event['previous']['traceback'].encode())==row['before_trace_sha256']
        assert event['previous']['traceback'].replace(event['previous']['message'],event['message'])==event['traceback']
        assert pin(row['test_source']['path'])==row['test_source']
    assert not git('diff','--check')
    return result

if __name__=='__main__':print(json.dumps(local(),sort_keys=True))
