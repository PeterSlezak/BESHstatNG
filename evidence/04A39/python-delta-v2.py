"""Classify actual changed outcomes without rebaselining or waiving historical errors."""
from common39 import *
from collections import Counter

old=load(W3/'python-node-scanners/python-identities-v1.json')
now=load(HERE/'python-node-scanners/python-identities-v1.json')
previous={x['id']:x for x in old['parents']}
assert Counter(x['id'] for x in now['parents'])==Counter(previous.keys())
privacy_ids={x['identity'] for x in old['events'] if x['outcome']=='ERROR' and 'UnicodeDecodeError' in x.get('traceback','')}
assert len(privacy_ids)==2
changes=[]
for row in now['parents']:
    before=previous[row['id']]['outcome']
    if before==row['outcome']:continue
    if row['id'] in privacy_ids:
        assert (before,row['outcome'])==('ERROR','FAIL')
        category='CHANGED_NONPASS_PRIVACY_FINDINGS_BLOCKING'
        reason='Same test parent executed. Its scan candidate set uses git diff plus untracked files. Old38 files are now committed; new39 raw machine-bound reports are candidates. Current scanner completes and reports1599 findings instead of reaching the historical UTF8 decode error. Original ERROR remains unwaived.'
    else:
        assert (before,row['outcome'])==('FAIL','PASS')
        category='PASS_IN_CURRENT_TRACKED_DIFF_SCOPE;HISTORICAL_FAIL_NOT_DISCHARGED'
        reason='Guard inspects git diff names. Accepted38 UDF changes are committed, leaving only39 test-project/plan tracked modifications. Current PASS does not erase original FAIL or independently prove all untracked/private files.'
    changes.append(dict(id=row['id'],before=before,after=row['outcome'],classification=category,reason=reason))
historical=[x for x in old['events'] if x['identity'] in {c['id'] for c in changes}]
common={(x['identity'],x['outcome'],x.get('subtest'),x['occurrence']):x for x in old['events']}
drifts=[]
for event in now['events']:
    key=(event['identity'],event['outcome'],event.get('subtest'),event['occurrence'])
    before=common.get(key)
    if before and (event['message_sha256']!=before['message_sha256'] or event['traceback_sha256']!=before['traceback_sha256']):
        drifts.append(dict(identity=event['identity'],outcome=event['outcome'],subtest=event.get('subtest'),occurrence=event['occurrence'],
            before_message_sha256=before['message_sha256'],after_message_sha256=event['message_sha256'],
            before_trace_sha256=before['traceback_sha256'],after_trace_sha256=event['traceback_sha256'],
            message_changed=event['message_sha256']!=before['message_sha256'],trace_changed=event['traceback_sha256']!=before['traceback_sha256'],
            disposition='CHANGED_DIAGNOSTIC_NOT_WAIVED;EXACT_MESSAGES_RETAINED;OWNER_REVIEW_REQUIRED'))
save('python-node-scanners/python-delta-v2.json',dict(status='BLOCKING_CHANGED_PRIVACY_AND_DIAGNOSTICS_NOT_WAIVED',
    current=pin(HERE/'python-node-scanners/python-identities-v1.json'),baseline=pin(W3/'python-node-scanners/python-identities-v1.json'),
    parents=956,current_parent_outcomes=now['summary']['parent_outcomes'],baseline_parent_outcomes=dict(PASS=830,FAIL=113,ERROR=13),
    current_event_outcomes=now['summary']['event_outcomes'],baseline_event_outcomes=dict(FAIL=185,ERROR=13),
    changes=changes,diagnostic_drifts=drifts,diagnostic_drift_count=len(drifts),
    historical_events_not_reproduced_in_current_scope=historical,historical_privacy_UnicodeDecodeErrors=[x for x in old['events'] if x['identity'] in privacy_ids],
    historical_privacy_errors_waived=False,missing_parent_identities=[],
    scope_source=[pin('scripts/verify-04a13-publication.py'),pin('scripts/verify-04a14-publication.py'),
        pin('scripts/tests/test_04a04_alignment.py'),pin('scripts/tests/test_04a05_completion_successor.py'),pin('scripts/tests/test_04a05_contingency.py')],
    findings_per_current_privacy_parent=1599,repository_privacy='NOT_CLEAN;NEW_RAW39_ARTIFACT_FINDINGS_BLOCKING',
    campaign_repeats=0,action='Keep raw authority local; verify sanitized upload derivative independently. No source/test fixes or privacy waivers. Owner decision/re-freeze plan required before READY.'))
print('Current Python',now['summary']['parent_outcomes'],'changes',len(changes),'diagnostic drifts',len(drifts),'NOT_READY',flush=True)
