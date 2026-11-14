"""Append-only NOT_READY closeout after the mandatory new reference-gap stop."""
import json,re,subprocess,sys
from datetime import datetime,timezone
from inspect_entry import ROOT,HERE,BASE,CLASS,pin,save,git
from verify_gee import inspect,verify_publication
from verify_remaining_mmrm import inspect as gap_reader
from verify_preservation import inspect as preserve
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import PRIVATE
from baseline_report import clean,python_receipt

def main():
 inspect(publication=True);gap=gap_reader();preservation=preserve()
 save('preservation-closeout-v1.json',preservation)
 commands=[]
 for folder in sorted((PRIVATE/'commands').glob('gee38-v2-*')):
  if (folder/'command.json').is_file():commands.append(dict(lane=folder.name,receipt=clean(json.loads((folder/'command.json').read_bytes()))))
 save('command-receipts-closeout-v1.json',dict(commands=commands,
      manual_source_only_failures=['attempt-trace-preflight-v1.json','attempt-adverse-v1.json','attempt-mmrm-readonly-audit-v1.json'],
      raw_logs='Retained private isolated outputs; byte digests and exact sanitized commands above',
      compiler_failure='gee38-v2-original-Debug-plain-recorder; exit1 retained; corrected recorder uses a separately sealed new source file'))
 reader=json.loads((PRIVATE/'commands/gee38-v2-mmrm-gap-reader/command.json').read_bytes())
 tests=python_receipt('gee38-v2-mmrm-gap-adverse');assert tests['total']==9 and tests['exit_code']==0
 save('validation-closeout-v1.json',dict(mmrm_gap_reader=clean(reader),mmrm_gap_adverse=tests,
      gee_gate_validation=pin(HERE/'gee-gate-validation.json'),new_adverse_tests_total=87,unchanged_prior_adverse_tests_total=98,
      complete_managed_node_python_strict_profile_mkdocs='NOT_RUN: evidence-only work and mandatory missing-original-reference stop',
      inherited_receipts_only=dict(managed_debug='20556 PASS /14 inherited FAIL',managed_release='20556 PASS /14 inherited FAIL',node='916 PASS',
      python='767 PASS /178 FAIL /11 ERROR',strict=1,profile=1,mkdocs=0),
      full_ready_verifier='NOT_COMPLETED_NOT_RUN',native='NOT_RUN',r_oracle='NOT_RUN',office_mac_manual='NOT_RUN',
      packaging_signing_installation_deployment='NOT_RUN',not_ready_reason='New independent MMRM reference coverage decision required; previous ownership/extraction remains unfinished'))
 save('readiness-closeout-v1.json',dict(status='NOT_READY_04A38',gee_supplement='GEE_SUPPLEMENT_GATE_PASS',
      earliest_failed_phase='POST_GEE_GATE_REMAINING_PROFILE_REFERENCE_COVERAGE',blocker_member_id=gap['member_id'],
      missing='Original MMRM valid headered Name/Value profile parser branch; zero candidates among154 public cases/configuration',
      new_mmrm_references='NOT_AUTHORIZED_NOT_EXECUTED',new_production_changes=0,
      remaining_ownership=dict(historical=704,consumer_positive_body_unreviewed=199,needing_review=505,current=719,supporting=317,inline='UNFINISHED'),
      registered_exports=268,final_ownership_closed=False,full_ready_verifier='NOT_COMPLETED_NOT_RUN',
      next_action='Separate owner decision;owner-decision-needed.md. No automatic extra reference execution.',
      gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),native='NOT_RUN',scientific='NOT_GOLDEN',next_batch='04A39_NOT_STARTED',
      predecessor_gate_readiness='readiness.json remains unchanged as a historical pre-gate record'))
 check=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True)
 assert check.returncode==0 and not git('diff','--cached','--name-only')
 save('git-final-closeout-v1.json',dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),
      main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=0,
      status=git('status','--short').decode(),git_writes=False,recorded_utc=datetime.now(timezone.utc).isoformat()))
 save('source-transitions-closeout-v1.json',dict(new_production_transitions=[],new_existing_test_project_transitions=[],new_consumer_edges=[],
      authorized_existing_transitions=pin(ROOT/'evidence/04A38/source-transitions-validation-v2.json'),
      checkpoint_source_bytes='ALL_UNCHANGED',new_files='Only this versioned evidence directory plus ignored isolated build/test outputs',
      old_root_owner_dossier_and_readiness='BYTE_PRESERVED; versioned closeout is the new handover'))
 previous=ROOT/'evidence/04A38/late-archived-entry-supplement/v1/publication-manifest.json'
 paths={r['path'] for r in json.loads(previous.read_bytes())['entries']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and p.name!='entry-inspection.json')
 manifest_path=(HERE/'publication-manifest-closeout-v1.json').relative_to(ROOT).as_posix()
 inventory_path=(HERE/'file-inventory-closeout-v1.json').relative_to(ROOT).as_posix()
 privacy_path=(HERE/'privacy-closeout-v1.json').relative_to(ROOT).as_posix()
 paths.update([manifest_path,inventory_path,privacy_path])
 findings=[]
 for p in sorted(paths):
  if p in [manifest_path,inventory_path,privacy_path] or not p.startswith(HERE.relative_to(ROOT).as_posix()+'/'):continue
  data=(ROOT/p).read_bytes()
  if re.search(rb'(?i)[a-z]:[\\/]+users[\\/]+[^\\/\s"\x27]+',data):findings.append(dict(path=p,kind='OWNER_HOME_PATH'))
  if re.search(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',data):findings.append(dict(path=p,kind='PRIVATE_KEY'))
 assert not findings,findings
 save('privacy-closeout-v1.json',dict(status='PASS',findings=findings,
      scope='Every intended new public text artifact/capture/receipt; all older payloads byte-preserved with their prior privacy qualifications',
      raw_environment_log_excluded=pin(HERE/'entry-inspection.json'),public_view=pin(HERE/'entry-inspection-public.json'),
      retained_private='Raw environment log, command logs, isolated binaries and temporary adversarial views; no deletion',
      source_input_result_trace_redactions=0,workbook_data='Synthetic fixtures only',old_python_privacy_nonpasses='PRESERVED'))
 save('file-inventory-closeout-v1.json',dict(paths=sorted(paths),
      private_exclusions=[dict(path='entry-inspection.json',reason='Raw machine-specific worktree location;public view/disclosure and original private digest preserved')],
      compact_review_copy='NOT_CREATED; no intended payload omitted for upload size',authoritative='This complete inventory and publication-manifest-closeout-v1.json'))
 entries=[dict(path=p,bytes=None,sha256=None) if p==manifest_path else pin(ROOT/p) for p in sorted(paths)]
 save('publication-manifest-closeout-v1.json',dict(status='NOT_READY_04A38',reference_class=CLASS,
      scope='COMPLETE_VERSIONED_GEE_SUPPLEMENT_AND_POST_GATE_MMRM_STOP_HANDOVER;LOCAL_ONLY',
      predecessor_gate_manifest=pin(HERE/'publication-manifest.json'),entries=entries))
 print(json.dumps(verify_publication(ROOT,manifest_path,inventory_path)))
if __name__=='__main__':main()
