"""Seal complete local W1/F dossier; preserve all older manifests byte-for-byte."""
import json,sys,subprocess
from datetime import datetime,timezone
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3];V6=ROOT/'evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group'
sys.path.insert(0,str(V6))
from common import command,clean,RUNROOT,V2,PREVIOUS,pin,library_pin,compiler,git
from baseline_report import python_receipt
def save(n,v):
 with (HERE/n).open('x',encoding='utf-8',newline='\n') as f:json.dump(v,f,ensure_ascii=False,indent=2);f.write('\n')
def main():
 from verify_w1_v2 import inspect
 proof=inspect();save('independent-w1-verification.json',proof)
 result=command('w1-preservation-closeout',[sys.executable,'-B',str(V2/'verify_preservation.py')]);assert result['exit_code']==0
 preserved=json.loads((RUNROOT/'commands/mmrm38-v6-w1-preservation-closeout/stdout.log').read_bytes())
 save('preservation.json',dict(live=preserved,v5_external_hashes=1249,original_sources=41,all_prior_manifests='Byte-preserved; independently rehashed via F and predecessor readers',historical_TestResults='NOT_TRAVERSED_OR_REHASHED',source_transitions='Only inherited32; zero new production/existing test/project/owner-document edits'))
 new=python_receipt('mmrm38-v6-w1-adverse-v2');assert new['total']==18 and new['exit_code']==0
 old_fail=python_receipt('mmrm38-v6-w1-adverse');assert old_fail['exit_code']==1
 save('validation.json',dict(new_F_gate_tests=70,new_W1_tests=new,unchanged_prior_tests=457,total_current_focused_passes=545,
  failed_first_W1_fixture=old_fail,failed_attempt_records=['census-setup-attempt-v1.json','matrix-attempt-v1.json','adverse-attempt-v1.json'],
  correction_policy='Original assertions and frozen outputs unchanged; invalid fixture nominal rejections were not accepted as proof',
  selected_Release='Actual F2/2 and268 compiled metadata comparisons; no full suite',
  full_suites='NOT_RUN_EVIDENCE_ONLY_NO_PRODUCTION_EDIT_AND_MANDATORY_REFERENCE_STOP',
  inherited_full_receipts_only=dict(managed_Debug='20556 PASS /14 inherited FAIL',managed_Release='20556 PASS /14 inherited FAIL',Python='767 PASS /178 FAIL /11 ERROR',Node='916 PASS',strict=1,profile=1,MkDocs=0),
  canonical_wrapper='Inspected;NOT_RUN: implicit restore and shared TestResults, uncontrolled Legacy build effects',native_Excel_COM_UIA_RefEdit='NOT_RUN',R='NOT_RUN',Office_Mac_manual='NOT_RUN',installer_signing_deployment='NOT_RUN',W3_READY_verifier='NOT_COMPLETED_NOT_RUN'))
 commands=[]
 for folder in sorted((RUNROOT/'commands').glob('mmrm38-v6-*')):
  if (folder/'command.json').is_file():commands.append(dict(lane=folder.name,receipt=clean(json.loads((folder/'command.json').read_bytes()))))
 save('command-receipts.json',dict(commands=commands,direct_attempts=['census-setup-attempt-v1.json','matrix-attempt-v1.json'],raw_logs='Preserved privately; exact vectors/cwd/exits and raw digests above',
  later_publication_archive_commands='Separate detached audit/ZIP receipts; no statistical/source/trace redaction',
  read_only_inspection_warnings='Initial Windows rg wildcard-as-path and absent guessed helper path requests were corrected with scoped file discovery; no source or model execution in those inspections'))
 save('toolchain.json',dict(python_version=sys.version,python_name=Path(sys.executable).name,compiler_files=[library_pin(Path(compiler(n))) for n in ['vbc.exe','csc.exe']],
  production_targets='Unchanged Core/Common netstandard2.0; Windows net48',syntax_tool='Existing installed SDK net10; recorded cached no-package assets; no restore/install',module_runtime='SHA/size/MVID and Framework version in F execution.json',network='NOT_RUN'))
 checked=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert checked.returncode==0 and not git('diff','--cached','--name-only')
 save('git-final.json',dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=0,status=git('status','--porcelain=v1','--untracked-files=all').decode(),recorded_utc=datetime.now(timezone.utc).isoformat(),git_mutations=False))
 save('source-transition-audit.json',dict(new_production_changes=[],new_existing_test_project_changes=[],owner_document_changes=[],new_compiled_portable_consumers=[],inherited_transitions=pin(ROOT/'evidence/04A38/source-transitions-validation-v2.json'),new_paths='v6 F packet, completion/w1 and ignored isolated tools/captures/fixtures only',native_scientific='UNCHANGED_NOT_RUN_NOT_GOLDEN'))
 save('privacy.json',dict(status='Independent reader scans new W1/F bytes',private_raw_environment_logs_binaries='Retained privately with old exclusions; not republished',scientific_source_input_output_trace_redactions=0,workbook_data='Previously sealed synthetic input only',old_privacy_nonpasses='Preserved, not reclassified',review_copy='Derivative only; cannot prove live Git/4695 paths/ignored history'))
 paths={r['path'] for r in json.loads((ROOT/PREVIOUS).read_bytes())['entries']}
 for folder in [HERE,V6]:paths.update(p.relative_to(ROOT).as_posix() for p in folder.rglob('*') if p.is_file())
 paths.update(r['path'] for r in json.loads((HERE/'current-universe.json').read_bytes())['sources'])
 manifest=(HERE/'publication-manifest-v1.json').relative_to(ROOT).as_posix();inventory=(HERE/'file-inventory-v1.json').relative_to(ROOT).as_posix();paths.update([manifest,inventory])
 save('file-inventory-v1.json',dict(paths=sorted(paths),scope='Complete intended W1/F and predecessor payloads plus all41 actual current UDF sources',excluded='Existing private raw diagnostic, isolated binaries/raw logs and unrelated historical paths outside predecessor publication; retained TestResults excluded',archive='Derivative ZIP may follow; authoritative bytes unchanged'))
 save('publication-manifest-v1.json',dict(status='NOT_READY_04A38',reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',scope='04A38-W1_APPROVAL_F_AND_FULL_CENSUS_CONSOLIDATED_REFERENCE_STOP',predecessor=pin(ROOT/PREVIOUS),entries=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in sorted(paths)]))
 from verify_publication import inspect as publication
 print(json.dumps(publication()))
if __name__=='__main__':main()
