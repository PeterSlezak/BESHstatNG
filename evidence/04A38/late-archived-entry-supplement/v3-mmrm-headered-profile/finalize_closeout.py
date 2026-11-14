"""Append-only complete NOT_READY packet after the mandatory reference-gap stop."""
from datetime import datetime,timezone
from common import *
from verify_supplement import inspect as supplement
from verify_packet import inspect as packet
from verify_guard_gap import inspect as gap

def main():
 supplement();packet();missing=gap()
 result=command('preservation-closeout',[sys.executable,'-B',str(V2/'verify_preservation.py')]);assert result['exit_code']==0
 preserved=json.loads((RUNROOT/'commands/mmrm38-v3-preservation-closeout/stdout.log').read_bytes())
 save('preservation-closeout-v1.json',dict(live=preserved,prior_v2_external_payloads=890,first_v3_gate_external_payloads=977,
      checked='Every predecessor manifest hash reverified by independent supplement/publication readers',
      source_changes_after_entry=0,old_private_environment_control='Byte-preserved locally, not republished',historical_TestResults='NOT_TRAVERSED_OR_REHASHED'))
 commands=[]
 for folder in sorted((RUNROOT/'commands').glob('mmrm38-v3-*')):
  if (folder/'command.json').is_file():commands.append(dict(lane=folder.name,receipt=clean(json.loads((folder/'command.json').read_bytes()))))
 save('command-receipts.json',dict(commands=commands,raw_logs='Retained privately; exact sanitized command vectors, cwd, exits and raw log digests are above',
      earlier_tooling_failures=['attempt-input-tooling-v1.json','attempt-input-reader-v1.json'],
      command_aliases={'<V3>':HERE.relative_to(ROOT).as_posix(),'<PYTHON>':'Existing owner-specified Python executable; identity/runtime in this packet'},
      non_execution='Read-only corpus audits create no reference cases; all original/current executions are the two approved identities/configuration'))
 save('toolchain.json',dict(python_version=sys.version,python_executable_name=Path(sys.executable).name,compiler_files=[library_pin(Path(compiler(name))) for name in ['vbc.exe','csc.exe']],
      runtime='Actual Framework runtime and loaded assembly MVID/SHA/size in every execution.json',
      projects='Original Core/Common netstandard2.0; Windows net48; targets unchanged',restore='NOT_RUN',installation='NOT_RUN',packaging='NOT_RUN'))
 sys.path.insert(0,str(ROOT/'evidence/04A38'))
 from baseline_report import python_receipt
 guard_tests=python_receipt('mmrm38-v3-guard-gap-adverse');assert guard_tests['total']==10 and guard_tests['exit_code']==0
 reader=json.loads((RUNROOT/'commands/mmrm38-v3-guard-gap-reader/command.json').read_bytes());assert reader['exit_code']==0
 save('validation-closeout-v1.json',dict(gate_validation=pin(HERE/'gate-validation.json'),guard_gap_adverse=guard_tests,guard_gap_reader=clean(reader),new_test_total=79,old_test_total=185,
      full_suites='NOT_RUN: evidence-only; no production/test/project edit; required stop on essential missing original reference',
      inherited_receipts_only=dict(managed_Debug='20556 PASS /14 inherited FAIL',managed_Release='20556 PASS /14 inherited FAIL',Node='916 PASS',Python='767 PASS /178 FAIL /11 ERROR',strict=1,profile=1,MkDocs=0),
      wrappers='Inspected;NOT_RUN due implicit restore/shared TestResults and Legacy dependency packaging;isolated offline original builds and focused lanes executed',
      native='NOT_RUN',independent_R='NOT_RUN',Office_Mac_manual='NOT_RUN',packaging_signing_installation_deployment='NOT_RUN',full_ready_verifier='NOT_COMPLETED_NOT_RUN'))
 save('readiness-closeout-v1.json',dict(status='NOT_READY_04A38',mmrm_headered_gate='MMRM_HEADERED_PROFILE_GATE_PASS',
      earliest_failed_phase='POST_MMRM_GATE_REMAINING_PROFILE_GUARD_REFERENCE_COVERAGE',earliest_blocker_member_id=missing['member_id'],
      new_production_changes=0,extra_invalid_profile_reference_executions=0,additional_invalid_profile_packets='NOT_MATERIALIZED',expected_extra_original_results='UNKNOWN_NOT_CAPTURED',
      remaining_inventory=dict(historical=704,consumer_edge_positive_body_unreviewed=199,remaining_behavior_review=505,current=719,support=317,inline='UNFINISHED'),
      ownership_closed=False,exports=268,full_ready_verifier='NOT_COMPLETED_NOT_RUN',
      owner_decision_needed='owner-decision-needed.md; no automatic next capture',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),
      scientific='NOT_GOLDEN',native='NOT_RUN',next_batch='04A39_NOT_STARTED',owner_acceptance=False,git_publication=False))
 save('source-transitions.json',dict(new_production_transitions=[],new_existing_test_project_transitions=[],new_consumer_edges=[],
      existing_authorized_transitions=pin(ROOT/'evidence/04A38/source-transitions-validation-v2.json'),
      existing_all_sources='Byte-preserved against predecessor890-payload manifest; existing32 tracked transitions unchanged',
      new_files='Only v3 versioned evidence plus ignored isolated execution/build outputs',root_readiness_owner_dossier_and_plan='BYTE_PRESERVED; new versioned dossier records current decision'))
 check=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert check.returncode==0 and not git('diff','--cached','--name-only')
 save('git-final.json',dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),
      merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=0,status=git('status','--short').decode(),git_writes=False,recorded_utc=datetime.now(timezone.utc).isoformat()))
 paths={r['path'] for r in json.loads((ROOT/PREVIOUS).read_bytes())['entries']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file())
 manifest=(HERE/'publication-manifest-closeout-v1.json').relative_to(ROOT).as_posix()
 inventory=(HERE/'file-inventory-closeout-v1.json').relative_to(ROOT).as_posix()
 privacy=(HERE/'privacy.json').relative_to(ROOT).as_posix();paths.update([manifest,inventory,privacy])
 # The independent publication reader also scans actual new public bytes.
 save('privacy.json',dict(status='PUBLIC_PATHS_SCRUBBED;INDEPENDENT_READER_REQUIRED',
      old_private_diagnostic='Not republished; predecessor public derivative/disclosure retained exactly',raw_logs_binaries='Retained privately outside inventory',
      statistical_source_input_output_trace_redactions=0,workbook_data='Synthetic original frozen fixture only',
      previous_python_privacy_nonpasses='Retained historical synthetic-fixture findings; not relabeled PASS'))
 save('file-inventory-closeout-v1.json',dict(paths=sorted(paths),private_exclusions='Prior raw environment diagnostic and raw isolated logs/binaries; no prior intended public payload excluded',
      upload_zip='NOT_CREATED; no payload omission for upload-size constraints',authoritative_manifest=manifest))
 save('publication-manifest-closeout-v1.json',dict(status='NOT_READY_04A38',reference_class=CLASS,scope='COMPLETE_V3_MMRM_GATE_AND_POST_GATE_GUARD_STOP_HANDOVER;LOCAL_ONLY',
      predecessor_gate_manifest=pin(HERE/'publication-manifest.json'),entries=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in sorted(paths)]))
 print(json.dumps(packet(ROOT,manifest,inventory)))
if __name__=='__main__':main()
