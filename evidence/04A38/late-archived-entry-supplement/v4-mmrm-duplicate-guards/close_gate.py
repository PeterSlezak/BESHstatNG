"""Gate receipt only after independent source, byte, branch and adverse checks."""
from datetime import datetime,timezone
from common import *
from verify_supplement import inspect
from verify_packet import inspect as packet
from verify_original_source import inspect as source

def main():
 proof=inspect();publication=packet();original=source()
 sys.path.insert(0,str(ROOT/'evidence/04A38'))
 from baseline_report import python_receipt
 tests=[python_receipt('mmrm38-v4-'+n) for n in ['supplement-adverse-corrected','packet-adverse']]
 assert [(r['total'],r['exit_code']) for r in tests]==[(62,0),(16,0)]
 prior=json.loads((HERE/'prior-validation.json').read_bytes());assert prior['all_exit_zero']
 labels=['preedit-adverse','checkpoint-adverse','continuation-adverse','late432-adverse','late432-manifest-adverse','gee-adverse','protected-adverse','gee-manifest-adverse','old-header-gap-adverse','headered-adverse','headered-manifest-adverse','old-guard-gap-adverse']
 old=[python_receipt('mmrm38-v4-prior-'+n) for n in labels]
 assert [r['total'] for r in old]==[28,20,12,24,14,52,6,20,9,53,16,10] and all(r['exit_code']==0 for r in old)
 reader=json.loads((RUNROOT/'commands/mmrm38-v4-gate-independent-reader/command.json').read_bytes());assert reader['exit_code']==0
 save('gate-validation.json',dict(independent_original_source=original,publication=publication,independent_reader=clean(reader),new_tests=tests,unchanged_old_tests=old,
      new_test_total=78,old_test_total=264,failed_attempts=['attempt-premature-build-sequence.json','attempt-adverse-v1.json'],full_suites='NOT_RUN: evidence-only; no production/test/project edit'))
 save('MMRM_DUPLICATE_GUARD_GATE_PASS.json',dict(status='MMRM_DUPLICATE_GUARD_GATE_PASS',sequence=3,recorded_utc=datetime.now(timezone.utc).isoformat(),reference_class=CLASS,
      approval=pin(HERE/'owner-authorization.json'),input_seal=pin(HERE/'input-seal.json'),original_output_seal=pin(HERE/'original-output-seal.json'),
      comparison=pin(HERE/'compatibility-comparison.json'),validation=pin(HERE/'gate-validation.json'),gate_manifest=pin(HERE/'publication-manifest.json'),
      cases_per_configuration=4,original_executions_per_configuration=12,current_executions_per_configuration=4,guard_kinds_per_configuration=dict(duplicate_visit_time=2,duplicate_fitted_column=2),
      returned_reports=0,observable_query_outcome='RETURN_TEXT for all8 configuration/culture/kind results; no exception or ByRef change',
      batch_status='NOT_READY_04A38',new_production_changes=0,scientific='NOT_GOLDEN',native='NOT_RUN',
      next_action='Resume existing04A38 whole-body/ownership review;stop before any further essential original reference outside approvals A/B/C/D'))
 print('MMRM_DUPLICATE_GUARD_GATE_PASS;4/4 exact Debug andRelease;78 new+264 unchanged older tests PASS;whole batch NOT_READY')
if __name__=='__main__':main()
