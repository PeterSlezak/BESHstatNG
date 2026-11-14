"""Append-only complete v5 dossier after the mandatory next-reference stop."""
from datetime import datetime,timezone
from common import *
from verify_supplement import inspect as supplement
from verify_packet import inspect as packet
from verify_group_gap import inspect as gap
def main():
 supplement();packet();missing=gap()
 result=command('preservation-closeout',[sys.executable,'-B',str(V2/'verify_preservation.py')]);assert result['exit_code']==0
 live=json.loads((RUNROOT/'commands/mmrm38-v5-preservation-closeout/stdout.log').read_bytes())
 save('preservation-closeout-v1.json',dict(live=live,v4_predecessor_external_payloads=1118,v5_gate_external_payloads=1212,
  all_predecessor_payloads='Independently rehashed; all old manifests, failed attempts and NOT_READY records byte-preserved',old_private_environment_control='Preserved locally with its existing exclusion/public derivative; not republished',historical_TestResults='NOT_TRAVERSED_OR_REHASHED'))
 commands=[]
 for folder in sorted((RUNROOT/'commands').glob('mmrm38-v5-*')):
  if (folder/'command.json').is_file():commands.append(dict(lane=folder.name,receipt=clean(json.loads((folder/'command.json').read_bytes()))))
 save('command-receipts.json',dict(commands=commands,raw_logs='Retained privately; exact sanitized vectors/cwd/exits and raw stdout/stderr digests above',
  direct_tool_attempts=[pin(HERE/'source-review-console-attempt.json')],post_seal_commands='Final closeout reader/adverse/public-copy commands recorded in closeout-verification.json under the outer v2 manifest',
  command_aliases={'<V5>':HERE.relative_to(ROOT).as_posix(),'<PYTHON>':'Existing owner-specified Windows validation executable','<REPO_ROOT>':'Repository root','<ISOLATED_RUN>':RUNROOT.relative_to(ROOT).as_posix()},
  reference_scope='Exactly2 approved identities/configuration; original plain/repeat/trace and current only. Corpus reanalysis/adverse controls do not execute new statistical variants'))
 save('toolchain.json',dict(python_version=sys.version,python_executable_name=Path(sys.executable).name,compiler_files=[library_pin(Path(compiler(n))) for n in ['vbc.exe','csc.exe']],
  runtime='Actual Framework/runtime/module SHA/MVID/size in every execution.json',projects='Original Core/Common netstandard2.0; Windows net48; no target changes',restore='NOT_RUN',installation='NOT_RUN',packaging='NOT_RUN'))
 from baseline_report import python_receipt
 tests=python_receipt('mmrm38-v5-group-gap-adverse');assert tests['total']==12 and tests['exit_code']==0
 reader=json.loads((RUNROOT/'commands/mmrm38-v5-group-gap-reader/command.json').read_bytes());assert reader['exit_code']==0
 save('validation-closeout-v1.json',dict(gate_validation=pin(HERE/'gate-validation.json'),group_gap_adverse=tests,group_gap_reader=clean(reader),new_test_total=93,old_test_total=352,
  failed_read_only_presentation=pin(HERE/'source-review-console-attempt.json'),gate_compiler_materialization_capture_test_nonpasses=[],
  full_suites='NOT_RUN: evidence-only; no production/existing test/project change; mandatory stop at newly established essential scalar-group reference gap',
  inherited_receipts_only=dict(managed_Debug='20556 PASS /14 inherited FAIL',managed_Release='20556 PASS /14 inherited FAIL',Python='767 PASS /178 FAIL /11 ERROR',Node='916 PASS',strict=1,profile=1,MkDocs=0),
  wrappers='Inspected;NOT_RUN: implicit restore/shared TestResults/Legacy packaging; safe isolated offline equivalents ran',native_Excel_COM_UIA_RefEdit='NOT_RUN',independent_R='NOT_RUN',Office_Mac_manual='NOT_RUN',packaging_signing_installation_deployment='NOT_RUN',full_ready_verifier='NOT_COMPLETED_NOT_RUN'))
 save('readiness-closeout-v1.json',dict(status='NOT_READY_04A38',unmatched_profile_gate='MMRM_UNMATCHED_PROFILE_GATE_PASS',earliest_failed_phase='POST_UNMATCHED_GATE_SCALAR_GROUP_INTERCEPT_REFERENCE_COVERAGE',earliest_blocker_member_id=missing['member_id'],
  new_production_changes=0,extra_reference_executions=0,additional_case_packets='NOT_MATERIALIZED',extra_expected_original_output='UNKNOWN_NOT_CAPTURED',
  inherited_remaining_inventory=dict(historical=704,consumer_edge_positive_body_unreviewed=199,remaining_behavior_review=505,current=719,support=317,inline='UNFINISHED'),ownership_closed=False,exports=268,full_ready_verifier='NOT_COMPLETED_NOT_RUN',
  owner_decision_needed='owner-decision-needed.md; separate authority required by owner continuation Section4.5',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),scientific='NOT_GOLDEN',native='NOT_RUN',next_batch='04A39_NOT_STARTED',owner_acceptance=False,git_publication=False))
 save('source-transitions.json',dict(new_production_transitions=[],new_existing_test_project_transitions=[],new_portable_consumer_edges=[],existing_authorized_transitions=pin(ROOT/'evidence/04A38/source-transitions-validation-v2.json'),
  current_source_bytes='All v4 predecessor source/metadata/assertions remain exact; existing32 tracked transitions unchanged',new_files='v5 evidence and ignored isolated build/capture/adverse fixtures only',owner_plan_READMEs_root_readiness_old_dossiers='BYTE_PRESERVED'))
 ignored=git('check-ignore','.test-rationalization-04A38.tmp','evidence/04A38/late-archived-entry-supplement/v5-mmrm-unmatched-profile/original-results/Debug/results.tsv').decode().splitlines()
 save('ignored-capture-paths.json',dict(ignored_roots=ignored,isolated_v5_root=PRIVATE.relative_to(ROOT).as_posix(),original_source_tree=TREE.relative_to(ROOT).as_posix(),
  prior_isolated_top_level_directories=[p.name for p in sorted(RUNROOT.iterdir()) if p.is_dir()],historical_TestResults='Explicitly excluded from enumeration, traversal, hashing and cleanup',captures='Debug/Release captures listed individually in publication manifest even where Git-ignored'))
 checked=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert checked.returncode==0 and not git('diff','--cached','--name-only')
 save('git-final.json',dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=0,status=git('status','--porcelain=v1','--untracked-files=all').decode(),git_writes=False,recorded_utc=datetime.now(timezone.utc).isoformat()))
 save('privacy.json',dict(status='PUBLIC_PATHS_SCRUBBED;INDEPENDENT_READER_SCANS_BYTES',prior_raw_environment_diagnostic='Not republished; existing public derivative and exclusion retained',raw_logs_isolated_binaries='Preserved privately outside publication inventory',statistical_source_input_output_trace_redactions=0,workbook_data='Original frozen synthetic fixture only',earlier_python_privacy_nonpasses='Preserved, not relabeled PASS',upload_zip='NOT_CREATED'))
 paths={r['path'] for r in json.loads((ROOT/PREVIOUS).read_bytes())['entries']};paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file())
 manifest=(HERE/'publication-manifest-closeout-v1.json').relative_to(ROOT).as_posix();inventory=(HERE/'file-inventory-closeout-v1.json').relative_to(ROOT).as_posix();paths.update([manifest,inventory])
 save('file-inventory-closeout-v1.json',dict(paths=sorted(paths),private_exclusions='Existing raw owner diagnostic and isolated raw logs/binaries only; all prior intended public payloads retained',upload_zip='NOT_CREATED',authoritative_manifest=manifest))
 save('publication-manifest-closeout-v1.json',dict(status='NOT_READY_04A38',reference_class=CLASS,scope='V5_UNMATCHED_PROFILE_GATE_AND_REQUIRED_POST_GATE_SCALAR_GROUP_STOP;LOCAL_ONLY',predecessor_gate_manifest=pin(HERE/'publication-manifest.json'),entries=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in sorted(paths)]))
 print(json.dumps(packet(ROOT,manifest,inventory)))
if __name__=='__main__':main()
