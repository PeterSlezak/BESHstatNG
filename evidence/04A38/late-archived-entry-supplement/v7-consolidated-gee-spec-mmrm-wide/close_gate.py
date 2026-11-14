"""Issue bounded Approval G receipt only after independent raw test receipts."""
from datetime import datetime,timezone
from common import *
from baseline_report import python_receipt
from verify_supplement import inspect
from verify_packet import inspect as packet

def main():
 proof=inspect();publication=packet()
 prior=json.loads((HERE/'prior-validation.json').read_bytes());focused=json.loads((HERE/'focused-validation.json').read_bytes())
 assert prior['all_exit_zero'] and focused['all_exit_zero']
 tests=[python_receipt('consolidated38-v7-'+n) for n in ['supplement-adverse','packet-adverse']]
 assert [(r['total'],r['exit_code']) for r in tests]==[(56,0),(16,0)]
 old=[]
 for lane in prior['executed']:
  label='consolidated38-v7-prior-'+lane['lane']
  raw=(RUNROOT/'commands'/label/'stderr.log').read_bytes()
  if b'Ran ' in raw:old.append(python_receipt(label))
 assert all(r['exit_code']==0 and r['failed']==r['errors']==0 for r in old)
 assert sum(r['total'] for r in old)==557
 seal=json.loads((HERE/'original-output-seal.json').read_bytes());sealed=datetime.fromisoformat(seal['sealed_utc'])
 correction=json.loads((HERE/'recorder-correction-seal.json').read_bytes())
 corrected=(HERE/'MmrmCapture38-v2.cs').read_text(encoding='utf-8');old_source=(HERE/'MmrmCapture38.cs').read_text(encoding='utf-8')
 assert corrected==old_source.replace('if(property("Result")==null','if(property(ActiveFamily=="GEE"?"Model":"Result")==null')
 births=[]
 for config in ['Debug','Release']:
  for mode in ['plain','repeat','trace']:
   r=json.loads((HERE/'original-results-v2'/config/mode/'execution.json').read_bytes())
   assert datetime.fromisoformat(correction['recorded_utc'])<datetime.fromisoformat(r['started_utc'])
  for filename in ['BESHStat.Core.Tests.dll','MmrmCurrentCapture38.exe']:
   p=PRIVATE/('current-'+config+'-trace-v2')/filename;stamp=datetime.fromtimestamp(p.stat().st_mtime,timezone.utc);assert stamp>sealed
   births.append(dict(configuration=config,artifact=library_pin(p),modified_utc=stamp.isoformat()))
 save('gate-validation.json',dict(independent_publication=publication,new_tests=tests,new_passes=72,unchanged_previous_tests=old,previous_passes=557,
  corrected_observer='Only GEE Model versus MMRM Result property inspection; source library, factory and case bytes unchanged',
  failed_attempts=['attempt-original-recorder-v1.json','driver-preparation-attempt.json'],current_compilation_after_original_seal=births,
  full_suites='NOT_RUN_EVIDENCE_ONLY_BEFORE_W2',native='NOT_RUN',scientific='NOT_GOLDEN'))
 for kind,name in [('GEE_SPEC_INTERCEPT_ROLE','GEE_SPEC_INTERCEPT_ROLE_GATE_PASS'),('MMRM_WIDE_MULTIROW_REJECTION','MMRM_WIDE_MULTIROW_REJECTION_GATE_PASS')]:
  save(name+'.json',dict(status=name,recorded_utc=datetime.now(timezone.utc).isoformat(),reference_class=CLASS,approval=pin(HERE/'owner-authorization.json'),
   input_seal=pin(HERE/'input-seal.json'),original_output_seal=pin(HERE/'original-output-seal.json'),validation=pin(HERE/'gate-validation.json'),
   cases={c:[r for r in proof['configurations'][c]['cases'] if r['kind']==kind] for c in ['Debug','Release']},batch_status='NOT_READY_04A38'))
 save('APPROVAL_G_GATE_PASS.json',dict(status='APPROVAL_G_COMBINED_GATE_PASS',recorded_utc=datetime.now(timezone.utc).isoformat(),reference_class=CLASS,
  input_seal=pin(HERE/'input-seal.json'),original_output_seal=pin(HERE/'original-output-seal.json'),subgates=[pin(HERE/(n+'.json')) for n in ['GEE_SPEC_INTERCEPT_ROLE_GATE_PASS','MMRM_WIDE_MULTIROW_REJECTION_GATE_PASS']],
  exact_per_configuration=dict(Debug=4,Release=4),true_guard_witnesses=8,accepted_original_executions_per_configuration=12,total_original_executions_per_configuration=24,
  current_executions_per_configuration=4,returned_reports=0,batch_status='NOT_READY_04A38',next_action='Continue W2A/W2B directly; no more original semantic cases authorized'))
 commands=[]
 for folder in sorted((RUNROOT/'commands').glob('consolidated38-v7-*')):
  if (folder/'command.json').is_file():commands.append(dict(label=folder.name,receipt=clean(json.loads((folder/'command.json').read_bytes()))))
 save('command-receipts.json',dict(commands=commands,unreceipted_attempts=['First direct output seal failed on recorder fit classification; retained first original-results and attempt-original-recorder-v1.json','Inline PowerShell quoting failed before current driver creation; driver-preparation-attempt.json','Direct supplement controls56PASS repeated with full raw receipt'],full_suites='NOT_RUN_EVIDENCE_ONLY',old_test_expectations_changed=False))
 with (HERE/'owner-review.md').open('x',encoding='utf-8') as f:f.write('''# Approval G compatibility gate — batch NOT_READY

Both subgates passed: **Debug4/4 EXACT; Release4/4 EXACT**. Each configuration contains one GEE spec-intercept rejection and one MMRM wide-multirow rejection in each of Invariant and sk-SK. Genuine original true guards are the GEE1506–1509 path (resolved fitted Intercept index0) and MMRM592–594 after headerless-column decline652, first wide row1, second row2. All eight public results are scalar System.String errors; no reports, exceptions, native attempts or public argument mutation in accepted observations.

Source isolation independently authenticates pinned main and all41 archived UDF sources, original transitive compile inputs and actual loaded modules. Input seal precedes original execution; accepted original-output seal precedes every G current compilation/execution. Only the predeclared per-session handle symbols differ across sessions. Typed model/design, scalar/error bytes, bounds, IEEE bits, cultures, argument state, branch order and private ByRef observations compare exactly. See compatibility-comparison.json and original-coverage.json.

The first observer erroneously queried the GEE cache for MMRM's Result property. That attempt and its genuine queries are retained, INVALID as accepted fit recording. The corrected recorder uses GEE Model and MMRM Result; original libraries and sealed semantic inputs were unchanged. There are24 original case executions/configuration including12 failed-observer attempts and12 accepted plain/repeat/trace observations, four distinct authorized cases/configuration; four current executions/configuration. No ninth distinct G case exists.

Independent new controls:72 PASS (56 supplement,16 manifest); unchanged predecessor controls557 PASS. Raw identities and command exits are in gate-validation.json and command-receipts.json. Older honest NOT_READY decisions, failed fixtures and manifests remain unchanged. Historical full suites were NOT rerun during this evidence-only gate; no green-suite claim is made.

The previous genuinely pre-edit28966 cases/configuration stay separate from authorized A–G458 late cases/configuration. This gate is LATE_ARCHIVED_ENTRY_SUPPLEMENT and compatibility only. W1 is complete; W2 production extraction/whole-body reconciliation and W3 validation/readiness still remain. Continue W2 under existing authority without a new permission step.

P=NO; N1=NO GO; N2=NO; Gate C2=UNMET; Gate C3=UNAPPROVED; NOT_GOLDEN. Native Excel/COM/UIA/RefEdit, R, Office/Mac manual smoke, packaging/signing/install/deployment remain NOT_RUN. No stage, commit, merge, push or04A.39. Git receipts describe the actual live checkpoint; copied publication readers prove copied bytes only.
''')
 paths={r['path'] for r in json.loads((ROOT/PREVIOUS).read_bytes())['entries']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file())
 manifest=(HERE/'publication-manifest-closeout-v1.json').relative_to(ROOT).as_posix();inventory=(HERE/'file-inventory-closeout-v1.json').relative_to(ROOT).as_posix();paths.update([manifest,inventory])
 save('file-inventory-closeout-v1.json',dict(paths=sorted(paths),scope='Full current v7 and immutable predecessor payload chain; original manifest versions retained',private_binaries='Internal compilation outputs represented by source provenance and assembly hashes; no ignored TestResults enumeration'))
 save('publication-manifest-closeout-v1.json',dict(status='NOT_READY_04A38',reference_class=CLASS,entries=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in sorted(paths)]))
 print(json.dumps(packet(manifest_name=manifest,inventory_name=inventory)))
 print('APPROVAL_G_COMBINED_GATE_PASS; proceed directly W2; batch NOT_READY')
if __name__=='__main__':main()
