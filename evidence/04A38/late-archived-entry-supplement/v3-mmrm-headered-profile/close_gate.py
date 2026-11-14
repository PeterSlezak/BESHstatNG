"""Issue bounded gate receipt only after independent source/data/audit checks."""
from datetime import datetime,timezone
from common import *
from verify_supplement import inspect
from verify_packet import inspect as packet
from verify_original_source import inspect as source

def main():
 proof=inspect();publication=packet();original=source()
 sys.path.insert(0,str(ROOT/'evidence/04A38'))
 from baseline_report import python_receipt
 tests=[python_receipt('mmrm38-v3-'+name) for name in ['supplement-adverse','packet-adverse']]
 assert [(r['total'],r['exit_code']) for r in tests]==[(53,0),(16,0)]
 prior=load_prior=json.loads((HERE/'prior-validation.json').read_bytes());assert prior['all_exit_zero']
 old_tests=[python_receipt('mmrm38-v3-prior-'+name) for name in ['preedit-adverse','checkpoint-adverse','continuation-adverse','late432-adverse','late432-manifest-adverse','gee-adverse','protected-adverse','gee-manifest-adverse','old-gap-adverse']]
 assert [r['total'] for r in old_tests]==[28,20,12,24,14,52,6,20,9] and all(r['exit_code']==0 for r in old_tests)
 reader=json.loads((RUNROOT/'commands/mmrm38-v3-gate-independent-reader/command.json').read_bytes());assert reader['exit_code']==0
 save('gate-validation.json',dict(independent_original_source=original,publication=publication,independent_reader=clean(reader),new_tests=tests,old_unchanged_tests=old_tests,
      new_test_total=69,old_test_total=185,full_suites='NOT_RUN: evidence-only; no production/test/project source changes'))
 save('MMRM_HEADERED_PROFILE_GATE_PASS.json',dict(status='MMRM_HEADERED_PROFILE_GATE_PASS',recorded_utc=datetime.now(timezone.utc).isoformat(),sequence=3,
      reference_class=CLASS,approval=pin(HERE/'owner-authorization.json'),input_seal=pin(HERE/'input-seal.json'),original_output_seal=pin(HERE/'original-output-seal.json'),
      comparison=pin(HERE/'compatibility-comparison.json'),validation=pin(HERE/'gate-validation.json'),gate_manifest=pin(HERE/'publication-manifest.json'),
      cases_per_configuration=2,headered_branch_cases_per_configuration=2,reports_per_configuration=2,original_executions_per_configuration=6,current_executions_per_configuration=2,
      batch_status='NOT_READY_04A38',new_production_changes=0,scientific='NOT_GOLDEN',native='NOT_RUN',
      next_action='Resume existing04A38 source/ownership review;stop before any further essential unapproved reference capture'))
 print('MMRM_HEADERED_PROFILE_GATE_PASS;2/2 exact each configuration;all4 returned reports;69 new and185 older tests PASS;whole batch NOT_READY')
if __name__=='__main__':main()
