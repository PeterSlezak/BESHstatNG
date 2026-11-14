"""Seal concrete final proof inputs after source/test/exception reconciliation."""
from common import *
load=lambda p:json.loads(p.read_bytes())
assert load(HERE/'python-nonpass-investigation-v1.json')['status']=='EXACT_MESSAGE_AND_EVENT_CHANGES_RECONCILED_NO_ASSERTION_WAIVER'
assert load(HERE/'managed-validation-v2.json')['Debug']['outcomes']==dict(Passed=20990,Failed=14)
assert load(HERE/'python-effective-identities-v1.json')['complete_current_records']==956
assert load(HERE/'support-child-identity-reconciliation-v1.json')['supported_identity_delta']==0
for row in load(HERE/'production-test-freeze-v3.json')['sources']:assert pin(ROOT/row['path'])==row
exclusions={r['path'] for r in load(HERE/'review-exclusions-v1.json')['excluded']}
from publish_parts_v15_v2 import selected
selected_paths=selected()-exclusions
auth={}
for path in sorted(HERE.rglob('*')):
 if not path.is_file() or '__pycache__' in path.parts:continue
 name=path.relative_to(ROOT).as_posix()
 if name in exclusions:continue
 auth[name]=pin(path)['sha256']
for row in load(HERE/'production-test-freeze-v3.json')['sources']:auth[row['path']]=row['sha256']
transitions=load(HERE/'source-transitions-final-v1.json')['files'];changed={r['path'] for r in transitions}
for row in load(HERE/'entry-authentication-v2.json')['authorities']:
 if row['path'] in selected_paths and row['path'] not in changed:
  assert pin(ROOT/row['path'])==row
  assert auth.setdefault(row['path'],row['sha256'])==row['sha256']
template=HERE/'verify_c15_checkpoint_template_v6.py';source=template.read_text(encoding='utf-8');assert source.count('__AUTH__')==1
reader=HERE/'verify_c15_checkpoint_v1.py'
with reader.open('x',encoding='utf-8',newline='\n') as f:f.write(source.replace('__AUTH__',repr(dict(sorted(auth.items())))))
save('review-seal-v1.json',dict(status='PARTIAL_W2_FROZEN_PROOF_SEAL_NOT_PHASE_ACCEPTANCE',reader=pin(reader),template=pin(template),sealed_inputs=len(auth),source_freeze=pin(HERE/'production-test-freeze-v3.json'),historical_supported=537,current_supported=566,support_supported=262,
 Debug_full_campaigns=2,owner_replacement_Debug_campaigns=1,Python_complete_unique=956,Python_known_parent_starts=972,owner_replacement_Python_lanes=1,owner_focused_privacy_parents=2,Node_campaigns=1,original_queries=0,
 scope='Immutable proof seal. Later positive/adverse-reader, inventory/manifest and external publication/private/Git bookkeeping is additive and separately bound; no historical or current seal is rewritten.'))
print('Final actual C15 reader sealed',len(auth),'inputs',pin(reader))
