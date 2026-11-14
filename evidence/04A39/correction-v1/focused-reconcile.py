from common import *
import collections, re

focused=load(PRIVATE/'focused-raw.json');previous=load(PRIOR/'python-node-scanners/python-identities-v1.json')
crosswalk=load(HERE/'diagnostics/changed-54-crosswalk.json')
byid={r['id']:r for r in focused['records']}
assert len(byid)==26 and focused['outcomes']==dict(PASS=3,FAIL=23)
for row in previous['parents']:
    if row['id'] in byid:assert row['outcome']==byid[row['id']]['outcome']
proof=[]
for row in crosswalk['changes']:
    events=byid[row['identity']]['events']
    matches=[e for e in events if sha(e['message'].encode())==row['current_message_sha256']]
    assert len(matches)==1,'FOCUSED_CHANGED_DIAGNOSTIC '+row['identity']
    proof.append(dict(change_id=row['change_id'],identity=row['identity'],outcome=matches[0]['outcome'],message_sha256=matches[0]['message_sha256'],message_exact=True,
        raw_trace_sha256=matches[0]['traceback_sha256'],trace_disposition='FOCUSED_RUNNER_FRAME_DIFFERS;ORIGINAL_RAW_TRACE_RETAINED;ASSERTION_MESSAGE_EXACT'))
cache=load(PRIVATE/'focused-cache.json')
findings=[f for r in cache['rows'] for f in r['findings']]
original=load(PRIVATE/'privacy-13-replay-v2.json')['findings']
key=lambda r:json.dumps(r,sort_keys=True)
old=collections.Counter(map(key,original));now=collections.Counter(map(key,findings))
assert not (old-now) and len(findings)==1941
left=old.copy();rows=[];private=[]
pins={r['path']:pin(r['path']) for r in findings}
for i,f in enumerate(findings):
    k=key(f);classification='RETAINED_FROM_1599_SNAPSHOT' if left[k] else 'NEW_SINCE_PYTHON_SNAPSHOT'
    if left[k]:left[k]-=1
    identity='FOCUSED-PRIVACY-'+str(i+1).zfill(4)
    private.append(dict(finding_id=identity,**f,source=pins[f['path']],classification=classification))
    rows.append(dict(finding_id=identity,source_id='SOURCE-'+sha(f['path'].encode())[:16],source_sha256=pins[f['path']]['sha256'],category=f['category'],origin=f['origin'],line=f.get('line'),
        classification=classification,area='CORRECTION_WORKFLOW_PREIMAGE' if '/correction-v1/' in f['path'] else 'IMMUTABLE_04A39_EVIDENCE',
        severity='RESTRICTED_DIAGNOSTIC',authorization='SANITIZED_PUBLICATION_AUTHORIZED;ORIGINAL_RECORD_CHANGE_UNAUTHORIZED',action='Exclude protected raw upload; preserve originals; provide sanitized rendering',status='ROOT_CANDIDATE_FAIL_UNWAIVED'))
save(PRIVATE/'focused-privacy-findings-restricted.json',dict(findings=private,files=pins,source_command=pin(PRIVATE/'focused-raw.json')))
save('privacy/focused-finding-reconciliation.json',dict(per_parent_findings=1941,overlap=1941,parents=2,scanner_occurrences=3882,original_snapshot_retained=1599,new_since_snapshot=342,
    source_files=len(pins),findings=rows,categories=dict(collections.Counter(r['category'] for r in findings)),protected_mapping=pin(PRIVATE/'focused-privacy-findings-restricted.json'),
    domain_note='Original1599 domain was reconstructed from the frozen pre-campaign input manifest. Live focused domain also contains subsequent original39 final/publication records plus the correction authentication preimage. No findings have been suppressed. Historical privacy parents remain FAIL.'))
save('diagnostics/focused-reconciliation.json',dict(executed=26,outcomes=focused['outcomes'],command_exit=focused['exit_code'],changed_diagnostics=proof,exact_changed_messages=54,
    raw_receipt=pin(PRIVATE/'focused-raw.json'),cache=pin(PRIVATE/'focused-cache.json'),changed_parent_outcomes_vs_original39=[],new_functional_failure=False,full956_rerun=False,
    three_pass_parents=[r['id'] for r in focused['records'] if r['outcome']=='PASS'],
    privacy_note='Two original ERROR parents now reproducibly FAIL; W3 UnicodeDecodeErrors remain unwaived. No successful repository privacy result.'))
print('54 focused assertion messages exact; 1599 retained +342 subsequent =1941 per privacy parent',flush=True)
