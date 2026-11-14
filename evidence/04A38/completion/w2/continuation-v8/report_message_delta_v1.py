"""Post-scan accounting, without repeating or altering historical tests."""
from common import *
from collections import Counter
import re
old=json.loads((PREVIOUS/'python-first-failures-v1.json').read_bytes())['records'];now=json.loads((HERE/'python-first-failures-v1.json').read_bytes())['records']
def key(r):
    sub=r.get('subtest');return r['identity'],r['outcome'],sub[len(r['identity']):].strip() if sub else '',r['occurrence']
prior={key(r):r for r in old};changes=[];same=0
transitions=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']
for r in now:
    before=prior[key(r)]
    if r['message']==before['message']:same+=1;continue
    assert r['first_traceback']==before['first_traceback'],'Changed first assertion requires separate investigation'
    row=dict(identity=r['identity'],subtest=r.get('subtest'),occurrence=r['occurrence'],outcome=r['outcome'],old_message=before['message'],new_message=r['message'],old_clean_message_sha256=sha(before['message'].encode()),new_clean_message_sha256=sha(r['message'].encode()),first_traceback=r['first_traceback'],old_traceback_sha256=before['traceback_sha256'],new_traceback_sha256=r['traceback_sha256'])
    if r['identity'].startswith('test_04a28_output.'):
        t=next(t for t in transitions if t['after']['sha256'] in r['message'])
        assert t['before']['sha256'] in before['message']
        row.update(classification='RETAINED_ASSERTION_NEW_AUTHORIZED_SOURCE_PIN_ACTUAL_HASH_NOT_WAIVED',transition=t,explanation='Same immutable line29 subtest and original expected pin; actual source moved from authenticated C7 before bytes to the recorded C8 after bytes. Both failures remain FAIL.')
    else:
        assert r['identity'] in ['test_04a13_survival_workflow.Survival13SourceContracts.test_complete_existing_publication_set_has_no_unresolved_private_values','test_04a14_mixed_workflow.Mixed14SourceContracts.test_entire_proposed_publication_set_privacy']
        assert '172 additional' in before['message'] and '206 additional' in r['message']
        row.update(classification='RETAINED_PRIVACY_ASSERTION_NEW_FROZEN_CANDIDATE_FINDING_COUNT_NOT_WAIVED',explanation='Original assertions still FAIL. All206 actual tuples retained in five helper reports;26 are new C8 synthetic-control or public SDK-path source findings,8 are serialized relative traceback path separators in the C7 correction receipt generated after its historical scan. Prior172 full tuples are not reconstructed from truncated unittest messages.')
    changes.append(row)
assert len(changes)==4 and same==188
findings=[];helper_pins=[]
for f in sorted((PRIVATE/'python-v1/1/parallel-scan-1').glob('*/raw-result.json')):
    helper_pins.append(pin(f));report=json.loads(f.read_bytes())
    for r in report['rows']:findings.extend(r['findings'])
assert len(findings)==206 and len(helper_pins)==5
extra=[]
for r in findings:
    if '/continuation-v8/' in r['path']:
        category='PUBLIC_FIXED_REFERENCE_ASSEMBLY_PATH' if r['path'].endswith(('compatibility_v1.py','inspect_stage_v1.py')) else 'EXACT_SYNTHETIC_ADVERSE_CONTROL_SOURCE'
        extra.append(dict(r,classification=category,source=pin(ROOT/r['path'])))
    elif r['path'].endswith('continuation-v7/python-environment-corrections-v1.json'):
        assert r['category']=='unc_share';extra.append(dict(r,classification='JSON_ESCAPED_RELATIVE_TRACEBACK_SEPARATORS_IN_RETAINED_C7_POSTSCAN_RECEIPT',source=pin(ROOT/r['path'])))
assert len(extra)==34 and sum('/continuation-v8/' in r['path'] for r in extra)==26
save('python-message-delta-investigation-v1.json',dict(status='FOUR_CHANGED_MESSAGES_EXPLICITLY_RETAINED_NOT_WAIVED',same_clean_messages=same,changed_messages=changes,identity_outcome_occurrence_delta=0,actual_privacy_findings=206,historical_privacy_count=172,classified_current_additional_findings=extra,helper_raw=helper_pins,limitation='The old unittest privacy assertion printed a truncated list. No missing original tuple list is fabricated. Current exact tuples and source hashes are retained; immutable C7 publication/protection authority supports the unchanged historical inputs. This does not waive either historical privacy assertion or assert global absence of secrets.',postscan_bookkeeping=True))
print('Exact four message deltas and34 additional actual scanner tuples accounted')
