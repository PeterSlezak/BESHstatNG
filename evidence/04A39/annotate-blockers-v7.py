"""Make actual changed validation/privacy findings canonical; no waiver or rebaseline."""
from common39 import *
from collections import Counter

d=load(HERE/'register/defects-v6.json');delta=load(HERE/'python-node-scanners/python-delta-v2.json')
issues={x['canonical_id']:x for x in d['issues']}
for change in delta['changes']:
    row=issues['VALIDATION:PYTHON_PARENT:'+change['id']]
    row['current_outcome_delta']=change
    row['status']='CHANGED_NONPASS_BLOCKING' if change['after']=='FAIL' else 'CURRENT_SCOPE_PASS_HISTORICAL_FAIL_UNWAIVED'
    row['current_behavior']=change['reason']
for change in delta['diagnostic_drifts']:
    row=issues['VALIDATION:PYTHON_PARENT:'+change['identity']]
    row.setdefault('current_diagnostic_drifts',[]).append(change)
    if row['status']=='INHERITED_VALIDATION_NONPASS_NOT_WAIVED':row['status']='RETAINED_NONPASS_CHANGED_DIAGNOSTIC_UNWAIVED'
identity='ISSUE:A39-RAW-REVIEW-PRIVACY-01'
row=json.loads(json.dumps(issues['ISSUE:A39-PAIRED-SOURCE-POSITION-01']))
for key in ['aliases','original_obligations','original_controls','obligation_facets','report_occurrences','current_declaration_candidates','original_linked_member_signatures']:
    row[key]=[]
row.update(canonical_id=identity,aliases=['A39-RAW-REVIEW-PRIVACY-01'],record_kind='CURRENT_REVIEW_ARTIFACT_PRIVACY_FINDING',
    status='CHANGED_NONPASS_BLOCKING',description='Each existing13/14 privacy parent now reports1599 findings in new raw39 candidate artifacts; historical Unicode errors remain unwaived.',
    discovery_provenance=[pin(HERE/'python-node-scanners/python-delta-v2.json')],
    source_locations=[dict(preimage=pin('scripts/verify-04a13-publication.py'),member_signature='scan(candidates)',line=31,
        branch='Actual candidate domain is tracked diff plus untracked files; names/absolute machine paths in raw authority are findings.')],
    reproducer=dict(current=pin(HERE/'python-node-scanners/python-identities-v1.json'),delta=pin(HERE/'python-node-scanners/python-delta-v2.json'),
        note='Completed current Python/scanner campaign; do not rerun to erase failure history.'),
    original_current_evidence=[pin(HERE/'python-node-scanners/python-delta-v2.json')],
    host_owners=['Software evidence/publication owner'],risk=dict(scientific='NO_NUMERICAL_CHANGE',numerical='NO_PRODUCTION_CHANGE',provenance='RAW_AND_DERIVATIVE_MUST_REMAIN_DISTINCT',native_COM='NOT_AUTHORIZED',privacy='BLOCKING_FINDINGS'),
    observed_vs_suspected='ACTUAL_SCANNER_FAIL',current_behavior='Authoritative local raw reports retain machine-bound paths. They are not upload-ready raw source; independently sanitize/check the derivative.',
    proposed_action='Owner review of changed privacy/nonpass scope; authorize exact raw-to-private/report-only re-freeze/targeted validation plan before READY. Do not issue a privacy waiver or silently alter frozen attempts.',
    authorization='SAFE_DERIVATIVE_PREPARATION_ALLOWED;RAW_FAILURE_NOT_WAIVED',affected_tests=[x['id'] for x in delta['changes'] if x['after']=='FAIL'],
    dependency_phase=['04A.39 report/privacy repair and owner decision'],responsible_future_owner=['Software verification/publication maintainer; project owner'],
    unknown_blocked_reason=None)
issues[identity]=row
d['crosswalk'].append(dict(kind='NEW_04A39_OBSERVATION',original_id='A39-RAW-REVIEW-PRIVACY-01',canonical_id=identity))
d['issues']=[issues[k] for k in sorted(issues)];d['version']=7;d['schema']='BESHSTAT_CANONICAL_DEFECT_OBLIGATION_REGISTER_04A39_V7'
d['previous']=pin(HERE/'register/defects-v6.json')
d['counts'].update(canonical_entries=len(issues),new_observations=4,statuses=dict(Counter(x['status'] for x in issues.values())))
save('register/defects-v7.json',d)
save('register/current-v5.json',dict(canonical=pin(HERE/'register/defects-v7.json'),version=7,supersedes=pin(HERE/'register/current-v4.json'),
    reason='Actual changed privacy/nonpass/diagnostic results are blockers, not baseline normalization. Original IDs/facets/events retained.'))
text=(HERE/'register/index-v6.md').read_text().split('| Canonical ID')[0].replace('current-v4.json','current-v5.json').replace('defects-v6.json','defects-v7.json')
lines=[text,'| Canonical ID | Status | Original obligations | Aliases |','|---|---|---:|---|']
for r in d['issues']:lines.append('| '+r['canonical_id']+' | '+r['status']+' | '+str(len(r['original_obligations']))+' | '+', '.join(r['aliases'])+' |')
(HERE/'register/index-v7.md').write_text('\n'.join(lines)+'\n',encoding='utf-8',newline='\n')
print(d['counts'],flush=True)
