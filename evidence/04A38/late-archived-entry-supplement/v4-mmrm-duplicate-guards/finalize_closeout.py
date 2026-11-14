"""Append-only complete v4 NOT_READY dossier after the required reference stop."""
from datetime import datetime,timezone
from common import *
from verify_supplement import inspect as supplement
from verify_packet import inspect as packet
from verify_mask_gap import inspect as gap

def main():
 supplement();packet();missing=gap()
 result=command('preservation-closeout',[sys.executable,'-B',str(V2/'verify_preservation.py')]);assert result['exit_code']==0
 live=json.loads((RUNROOT/'commands/mmrm38-v4-preservation-closeout/stdout.log').read_bytes())
 save('preservation-closeout-v1.json',dict(live=live,v3_predecessor_external_payloads=1003,v4_gate_external_payloads=1095,
      all_predecessor_payloads='Independently rehashed through source/input/output/publication readers; old manifests unchanged',
      old_private_environment_control='Byte-preserved locally with its existing public derivative/exclusion; not republished',historical_TestResults='NOT_TRAVERSED_OR_REHASHED'))
 commands=[]
 for folder in sorted((RUNROOT/'commands').glob('mmrm38-v4-*')):
  if (folder/'command.json').is_file():commands.append(dict(lane=folder.name,receipt=clean(json.loads((folder/'command.json').read_bytes()))))
 save('command-receipts.json',dict(commands=commands,raw_logs='Retained privately; exact sanitized command vectors/cwd/exits and raw stdout/stderr digests above',
      attempt_records=['attempt-premature-build-sequence.json','attempt-adverse-v1.json'],
      command_aliases={'<V4>':HERE.relative_to(ROOT).as_posix(),'<PYTHON>':'Existing owner-specified validation executable'},
      reference_case_scope='Four approved identities/configuration only; read-only audits and private adverse mutations do not execute statistical variants'))
 save('toolchain.json',dict(python_version=sys.version,python_executable_name=Path(sys.executable).name,
      compiler_files=[library_pin(Path(compiler(n))) for n in ['vbc.exe','csc.exe']],runtime='Actual Framework/runtime/module SHA/MVID/size in each execution.json',
      projects='Original Core/Common netstandard2.0; Windows net48; no target changes',restore='NOT_RUN',installation='NOT_RUN',packaging='NOT_RUN'))
 sys.path.insert(0,str(ROOT/'evidence/04A38'))
 from baseline_report import python_receipt
 tests=python_receipt('mmrm38-v4-mask-gap-adverse');assert tests['total']==10 and tests['exit_code']==0
 reader=json.loads((RUNROOT/'commands/mmrm38-v4-mask-gap-reader/command.json').read_bytes());assert reader['exit_code']==0
 save('validation-closeout-v1.json',dict(gate_validation=pin(HERE/'gate-validation.json'),mask_gap_adverse=tests,mask_gap_reader=clean(reader),new_test_total=88,old_test_total=264,
      failed_first_supplement_attempt=pin(HERE/'attempt-adverse-v1.json'),failed_precompile_setup=pin(HERE/'attempt-premature-build-sequence.json'),
      full_suites='NOT_RUN: evidence-only; no production/existing test/project change; mandatory stop at new essential original reference gap',
      inherited_receipts_only=dict(managed_Debug='20556 PASS /14 inherited FAIL',managed_Release='20556 PASS /14 inherited FAIL',Python='767 PASS /178 FAIL /11 ERROR',Node='916 PASS',strict=1,profile=1,MkDocs=0),
      wrappers='Inspected;NOT_RUN due implicit restore/shared TestResults/Legacy packaging risk; safe isolated offline builds and focused readers ran',
      native_Excel_COM_UIA_RefEdit='NOT_RUN',independent_R='NOT_RUN',Office_Mac_manual='NOT_RUN',packaging_signing_installation_deployment='NOT_RUN',full_ready_verifier='NOT_COMPLETED_NOT_RUN'))
 save('readiness-closeout-v1.json',dict(status='NOT_READY_04A38',duplicate_guard_gate='MMRM_DUPLICATE_GUARD_GATE_PASS',
      earliest_failed_phase='POST_DUPLICATE_GATE_REMAINING_PROFILE_MASK_REFERENCE_COVERAGE',earliest_blocker_member_id=missing['member_id'],
      new_production_changes=0,extra_reference_executions=0,additional_case_packets='NOT_MATERIALIZED',extra_expected_original_output='UNKNOWN_NOT_CAPTURED',
      remaining_inventory=dict(historical=704,consumer_edge_positive_body_unreviewed=199,remaining_behavior_review=505,current=719,support=317,inline='UNFINISHED'),
      ownership_closed=False,exports=268,full_ready_verifier='NOT_COMPLETED_NOT_RUN',owner_decision_needed='owner-decision-needed.md; no automatic further reference',
      gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),scientific='NOT_GOLDEN',native='NOT_RUN',next_batch='04A39_NOT_STARTED',owner_acceptance=False,git_publication=False))
 save('source-transitions.json',dict(new_production_transitions=[],new_existing_test_project_transitions=[],new_portable_consumer_edges=[],
      existing_authorized_transitions=pin(ROOT/'evidence/04A38/source-transitions-validation-v2.json'),
      all_current_source_bytes='Preserved against1003-payload v3 predecessor; existing32 tracked transitions remain unchanged',
      new_files='Only v4 evidence and ignored isolated execution/build fixtures',owner_plan_READMEs_root_readiness_old_dossiers='BYTE_PRESERVED'))
 checked=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert checked.returncode==0 and not git('diff','--cached','--name-only')
 save('git-final.json',dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),
      merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=0,status=git('status','--porcelain=v1','--untracked-files=all').decode(),git_writes=False,recorded_utc=datetime.now(timezone.utc).isoformat()))
 paths={r['path'] for r in json.loads((ROOT/PREVIOUS).read_bytes())['entries']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file())
 manifest=(HERE/'publication-manifest-closeout-v1.json').relative_to(ROOT).as_posix();inventory=(HERE/'file-inventory-closeout-v1.json').relative_to(ROOT).as_posix();privacy=(HERE/'privacy.json').relative_to(ROOT).as_posix();paths.update([manifest,inventory,privacy])
 save('privacy.json',dict(status='PUBLIC_PATHS_SCRUBBED;INDEPENDENT_READER_SCANS_BYTES',prior_raw_environment_diagnostic='Not republished; exact old public derivative and disclosure retained',
      raw_logs_isolated_binaries='Retained privately outside inventory',statistical_source_input_output_trace_redactions=0,workbook_data='Original frozen synthetic fixture only',
      earlier_python_privacy_nonpasses='Preserved; not relabeled PASS'))
 save('file-inventory-closeout-v1.json',dict(paths=sorted(paths),private_exclusions='Existing raw owner diagnostic and isolated raw logs/binaries only; no previous intended public payload excluded',
      upload_zip='NOT_CREATED; no omission for upload-size constraints',authoritative_manifest=manifest))
 save('publication-manifest-closeout-v1.json',dict(status='NOT_READY_04A38',reference_class=CLASS,scope='COMPLETE_V4_DUPLICATE_GUARD_GATE_AND_POST_GATE_MASK_STOP_HANDOVER;LOCAL_ONLY',
      predecessor_gate_manifest=pin(HERE/'publication-manifest.json'),entries=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in sorted(paths)]))
 print(json.dumps(packet(ROOT,manifest,inventory)))
if __name__=='__main__':main()
