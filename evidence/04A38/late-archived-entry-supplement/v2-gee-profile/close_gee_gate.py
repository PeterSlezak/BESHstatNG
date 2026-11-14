"""Receipt writer, gated solely by independent readers and raw test receipts."""
import json,sys
from datetime import datetime,timezone
from inspect_entry import HERE,ROOT,pin,save
from verify_gee import inspect,verify_publication
from verify_preservation import inspect as preserve
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from baseline_report import python_receipt,clean
from run import PRIVATE

def main():
 proof=inspect();publication=verify_publication(ROOT);preservation=preserve()
 tests=[python_receipt(name) for name in ['gee38-v2-adverse-corrected','gee38-v2-protected-adverse','gee38-v2-manifest-adverse']]
 assert [(r['total'],r['failed'],r['errors'],r['exit_code']) for r in tests]==[(52,0,0,0),(6,0,0,0),(20,0,0,0)]
 command=json.loads((PRIVATE/'commands/gee38-v2-final-independent-gate/command.json').read_bytes());assert command['exit_code']==0
 prior=json.loads((HERE/'prior-validation.json').read_bytes());assert prior['all_exit_zero']
 old_tests=[python_receipt('gee38-v2-prior-'+name) for name in ['preedit-adverse','checkpoint-adverse','continuation-adverse','late432-adverse','late432-manifest-adverse']]
 assert [r['total'] for r in old_tests]==[28,20,12,24,14] and all(r['exit_code']==0 for r in old_tests)
 save('gee-gate-validation.json',dict(new_tests=tests,unchanged_older_tests=old_tests,independent_reader=clean(command),publication=publication,
      preserved_failed_attempts=['attempt-trace-preflight-v1.json','attempt-recorder-compile-v1.txt','attempt-adverse-v1.json'],
      raw_commands='Private isolated runner receipts; exact sanitized commands and output digests published',whole_batch_ready=False))
 save('GEE_SUPPLEMENT_GATE_PASS.json',dict(status='GEE_SUPPLEMENT_GATE_PASS',sequence=3,recorded_utc=datetime.now(timezone.utc).isoformat(),
      owner_decision=pin(HERE/'owner-authorization.json'),input_seal=pin(HERE/'input-seal.json'),original_output_seal=pin(HERE/'original-output-seal.json'),
      proof=pin(HERE/'compatibility-comparison.json'),validation=pin(HERE/'gee-gate-validation.json'),
      manifest=pin(HERE/'publication-manifest.json'),privacy=pin(HERE/'privacy.json'),preservation=preservation,
      cases_per_configuration=12,positive_profile_selections_per_configuration=6,intended_guards_per_configuration=6,
      original_executions_per_configuration=36,current_executions_per_configuration=12,returned_reports=0,
      reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',whole_batch_status='NOT_READY_04A38',
      next_allowed_action='Existing04A38 source/coverage/ownership reconciliation;stop for owner decision if another essential independent reference gap is found',
      native='NOT_RUN',scientific='NOT_GOLDEN'))
 print('GEE_SUPPLEMENT_GATE_PASS; new78 tests and unchanged98 older tests PASS; whole batch NOT_READY_04A38')
if __name__=='__main__':main()
