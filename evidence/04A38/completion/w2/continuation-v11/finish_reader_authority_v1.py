"""Additive actual final reader/attempt receipts; never rewrite the failed seal."""
from common import *
folder=RUNROOT/'commands/w2c11-sealed-reader-positive-v2'
result=json.loads((folder/'stdout.log').read_bytes())
assert result['reader_sealed'] and result['status']=='NOT_READY_04A38'
assert json.loads((folder/'command.json').read_bytes())['exit_code']==0
save('independent-proof-final-v1.json',dict(actual_result=result,
    reader=pin(HERE/'verify_c11_checkpoint_v2.py'),execution=pin(HERE/'sealed-reader-positive-v2.json'),
    raw_command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),
    scope='Actual full sealed successor execution, not a rerun/stitch of managed/Python campaigns or a promotion of the retained failed v1.',original_queries=0))
save('final-reader-authority-v1.json',dict(status='AUTHORITATIVE_C11_SUCCESSOR_READERS_PASS_NOT_READY',
    checkpoint=pin(HERE/'verify_c11_checkpoint_v2.py'),publication=pin(HERE/'verify_c11_publication_v2.py'),
    seal=pin(HERE/'review-seal-v2.json'),positive=pin(HERE/'sealed-reader-positive-v2.json'),
    controls=pin(HERE/'independent-controls-v2.json'),review_addendum=pin(HERE/'owner-review-reader-addendum-v1.md'),
    publisher=pin(HERE/'publish_parts_v2.py'),copied_reader_tool=pin(HERE/'copy_review_v2.py'),
    failed_prior=dict(seal=pin(HERE/'review-seal-v1.json'),reader=pin(HERE/'verify_c11_checkpoint_v1.py'),
        outcome=pin(HERE/'sealed-reader-positive-v1.json')),original_queries=0))
attempts=[]
for path in sorted((RUNROOT/'commands').glob('w2c11-*/command.json')):
    doc=json.loads(path.read_bytes())
    if doc['exit_code']:
        attempts.append(dict(command=pin(path),stdout=pin(path.parent/'stdout.log'),stderr=pin(path.parent/'stderr.log'),
            exit_code=doc['exit_code'],args=clean(doc['command'])))
save('postseal-attempts-addendum-v1.json',dict(status='ALL_COMPLETED_NONZERO_COMMANDS_RETAINED_NOT_WAIVED',
    prior_ledger=pin(HERE/'inherited-failure-mutation-ledger-v1.json'),commands=attempts,
    failed_seal_correction=pin(HERE/'sealed-reader-first-failure-correction-v3.json'),
    actual_successor=pin(HERE/'final-reader-authority-v1.json'),production_test_changes=0,
    scope='Includes the late failed sealed v1 and rejected argument-order diagnosis in addition to all completed earlier nonzero Runner commands. Direct shell diagnostics are explicitly observational, not fabricated raw files.',original_queries=0))
source=(HERE/'finalize_live_v1.py').read_text(encoding='utf-8')
point="sealed_review=pin(HERE/'owner-review.md'),sealed_handover=pin(HERE/'NEXT_WAVE_HANDOVER.md'),"
replacement="checkpoint_reader=pin(HERE/'verify_c11_checkpoint_v2.py'),publication_reader=pin(HERE/'verify_c11_publication_v2.py'),checkpoint_seal=pin(HERE/'review-seal-v2.json'),reader_authority=pin(HERE/'final-reader-authority-v1.json'),owner_reader_addendum=pin(HERE/'owner-review-reader-addendum-v1.md'),"+point
assert point in source;source=source.replace(point,replacement)
point="'The sealed source/evidence/owner review/handover and manifest are unchanged. These are final external additive authorities, not W3, scientific/native acceptance or a merge.','',"
replacement=point+"\n        'Authoritative executable readers are verify_c11_checkpoint_v2.py and verify_c11_publication_v2.py, under review-seal-v2.json. The failed sealed v1 and rejected first diagnosis remain unchanged; owner-review-reader-addendum-v1.md and final-reader-authority-v1.json identify the exact module-isolated successor and actual positive/control receipts. No source/test or historical assertion changed to correct that reader-only collision.','',"
assert point in source;source=source.replace(point,replacement)
with (HERE/'finalize_live_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
save('finalizer-source-crosswalk-v2.json',dict(before=pin(HERE/'finalize_live_v1.py'),after=pin(HERE/'finalize_live_v2.py'),
    change='Name/pin the authoritative v2 reader and retained failed-v1 chronology in additive final external authorities. No earlier sealed source or receipt changed.',original_queries=0))
print('Actual full v2 result and complete late failed-attempt/source authority preserved')
