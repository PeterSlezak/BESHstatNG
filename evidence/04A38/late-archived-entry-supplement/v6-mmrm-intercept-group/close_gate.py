"""Issue narrow compatibility gate only after independent and adverse verification."""
from datetime import datetime,timezone
from common import *
from verify_supplement import inspect
from verify_packet import inspect as packet
from verify_original_source import inspect as source
def main():
 proof=inspect();publication=packet();original=source();assert __import__('json').loads((HERE/'public-copy-reader.json').read_bytes())['command']['exit_code']==0
 from baseline_report import python_receipt
 tests=[python_receipt('mmrm38-v6-'+n) for n in ['supplement-adverse','packet-adverse']]
 assert [(r['total'],r['exit_code']) for r in tests]==[(54,0),(16,0)]
 prior=json.loads((HERE/'prior-validation.json').read_bytes());assert prior['all_exit_zero']
 labels=['preedit-adverse','checkpoint-adverse','continuation-adverse','late432-adverse','late432-manifest-adverse','gee-adverse','protected-adverse','gee-manifest-adverse','old-header-gap-adverse','headered-adverse','headered-manifest-adverse','old-guard-gap-adverse','duplicate-adverse','duplicate-manifest-adverse','old-mask-gap-adverse','unmatched-adverse','unmatched-manifest-adverse','old-group-gap-adverse','unmatched-closeout-adverse']
 old=[python_receipt('mmrm38-v6-prior-'+n) for n in labels]
 assert [r['total'] for r in old]==[28,20,12,24,14,52,6,20,9,53,16,10,62,16,10,65,16,12,12] and all(r['exit_code']==0 for r in old)
 reader=json.loads((RUNROOT/'commands/mmrm38-v6-gate-independent-reader/command.json').read_bytes());assert reader['exit_code']==0
 compile_times=[]
 sealed=datetime.fromisoformat(json.loads((HERE/'original-output-seal.json').read_bytes())['sealed_utc'])
 for c in ['Debug','Release']:
  for name in ['BESHStat.Core.Tests.dll','MmrmCurrentCapture38.exe']:
   p=PRIVATE/('current-'+c+'-trace-v2')/name;stat=p.stat();modified=datetime.fromtimestamp(stat.st_mtime,timezone.utc);assert modified>sealed
   compile_times.append(dict(configuration=c,artifact=library_pin(p),modified_utc=modified.isoformat(),birth_utc=datetime.fromtimestamp(stat.st_birthtime,timezone.utc).isoformat()))
 save('gate-validation.json',dict(independent_original_source=original,publication=publication,independent_reader=clean(reader),new_tests=tests,unchanged_old_tests=old,new_test_total=70,old_test_total=457,
  current_compiled_artifact_chronology=compile_times,failed_compile_capture_test_attempts=[],full_suites='NOT_RUN: evidence-only; no production/existing test/project edit'))
 save('MMRM_INTERCEPT_GROUP_GATE_PASS.json',dict(status='MMRM_INTERCEPT_GROUP_GATE_PASS',sequence=3,recorded_utc=datetime.now(timezone.utc).isoformat(),reference_class=CLASS,
  approval=pin(HERE/'owner-authorization.json'),input_seal=pin(HERE/'input-seal.json'),original_output_seal=pin(HERE/'original-output-seal.json'),comparison=pin(HERE/'compatibility-comparison.json'),validation=pin(HERE/'gate-validation.json'),gate_manifest=pin(HERE/'publication-manifest.json'),
  cases_per_configuration=2,original_executions_per_configuration=6,current_executions_per_configuration=2,original_true_branch_lines=[1477,1478,1479,1480],observed_index=0,resolved_fitted_name="Intercept",private_ByRef_resolvedName=None,private_ByRef_values=None,
  returned_reports=0,observable_query_outcome='All4 results are RETURN_TEXT; no public argument mutation/exception; private ByRef resolvedName/valuesNothing,error recorded through scalar public path',
  batch_status='NOT_READY_04A38',new_production_changes=0,scientific='NOT_GOLDEN',native='NOT_RUN',next_action='Resume existing04A38 whole-body ownership/import/profile/report work; stop before any further essential original reference outside approvals A-F'))
 print('MMRM_INTERCEPT_GROUP_GATE_PASS;2/2 exact Debug and Release;70 new+457 unchanged older focused tests PASS;batch NOT_READY')
if __name__=='__main__':main()
