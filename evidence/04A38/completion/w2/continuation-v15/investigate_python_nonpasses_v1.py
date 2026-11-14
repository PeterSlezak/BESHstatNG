"""Explain exact message/event changes without changing or waiving assertions."""
from common import *
from collections import defaultdict,Counter
import re
load=lambda p:json.loads(p.read_bytes())
current=load(HERE/'python-first-failures-v2.json')['records'];old=load(PREVIOUS/'python-first-failures-v2.json')['records']
key=lambda r:(r['identity'],r['outcome'],clean((r.get('subtest') or '').split(r['identity'],1)[-1].strip()),r['occurrence'])
prior=defaultdict(list)
for row in old:prior[key(row)].append(row)
sources=load(HERE/'source-transitions-final-v1.json')['files'];changes=[];unexplained=[]
for row in current:
 before=prior[key(row)]
 if any(row['message']==r['message'] for r in before):continue
 paths=[r for r in sources if r['after']['sha256'] in row['message'] or r['path'] in (row.get('subtest') or '')]
 if paths:
  classification='CURRENT_AUTHORIZED_SOURCE_HASH_DIFF_IN_ORIGINAL_ASSERTION_REMAINS_FAIL'
  assert row['outcome']=='FAIL'
  for r in paths:assert pin(ROOT/r['path'])==r['after']
 elif row['identity'] in ['test_04a13_survival_workflow.Survival13SourceContracts.test_complete_existing_publication_set_has_no_unresolved_private_values','test_04a14_mixed_workflow.Mixed14SourceContracts.test_entire_proposed_publication_set_privacy'] and row['outcome']=='ERROR' and 'UnicodeDecodeError' in row['traceback']:
  classification='ORIGINAL_UTF8_EXCEPTION_AT_EARLIER_PROTECTED_C14_BINARY;PRIOR_JSON_ERROR_HISTORY_RETAINED'
  binary=load(PREVIOUS/'publication-git-index-binding-v1.json')['snapshot'];assert pin(ROOT/binary['path'])==binary
  paths=[dict(retained_binary=binary,source='UNCHANGED_ORIGINAL_SEQUENTIAL_SCANNER',two_focus_authority=pin(HERE/'owner-focused-privacy-authorization-v1.json'))]
 elif row['identity']=='test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact' and row['outcome']=='ERROR':
  classification='SEPARATE_UNORDERED_SPC_FIRST_KEY_ERROR_VARIANT_RETAINED_NOT_RELABELLED'
 else:
  classification='EXACT_MESSAGE_OR_EVENT_CHANGE_REQUIRES_MANUAL_INVESTIGATION';unexplained.append(key(row))
 changes.append(dict(identity=row['identity'],outcome=row['outcome'],subtest=row.get('subtest'),occurrence=row['occurrence'],classification=classification,current_message=row['message'],current_traceback=row['traceback'],current_raw_message_sha256=row.get('raw_message_sha256'),current_raw_traceback_sha256=row['traceback_sha256'],prior_messages=[r['message'] for r in before],prior_traceback_sha256=[r['traceback_sha256'] for r in before],authorized_source_transitions=paths))
delta=load(HERE/'python-nonpass-delta-v2.json')
save('python-nonpass-investigation-v1.json',dict(status='EXACT_MESSAGE_AND_EVENT_CHANGES_RECONCILED_NO_ASSERTION_WAIVER' if not unexplained else 'STOP_UNEXPLAINED_NONPASS_CHANGE',changes=changes,unexplained=unexplained,event_delta=delta,prior=pin(PREVIOUS/'python-first-failures-v2.json'),current=pin(HERE/'python-first-failures-v2.json'),interruption=pin(HERE/'python-interruption-recovery-v1.json'),original_queries=0,
 scope='Every old event/message remains in its immutable prior ledger. Current source-pin failures remain FAIL; both JSON privacy errors remain ERROR; the unordered-SPC ERROR is separately identified. Interrupted partial failure messages that were never emitted remain unknown. No original assertion changed.'))
assert not unexplained,'Preserve unexplained new/nonpass message and STOP before sealing'
print('Python message changes reconciled',len(changes),'event delta',delta)
