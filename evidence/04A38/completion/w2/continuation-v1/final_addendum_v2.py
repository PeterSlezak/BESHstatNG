from common import *
from datetime import datetime,timezone
def main():
 diagnostic=json.loads((HERE/'inherited-spc-order-diagnostic.json').read_bytes());adverse=json.loads((HERE/'bounded-adverse-addendum-v3.json').read_bytes())
 save('validation-qualification.json',dict(final_source_validation=pin(HERE/'validation.json'),reported_python=dict(passed=766,failed=180,errors=10),previous_first_current_full=dict(passed=766,failed=179,errors=11),historical_checkpoint=dict(passed=767,failed=178,errors=11),source_pin_occurrence='One extra FAIL at test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved,subtest PlotDataUDFs.vb; exact expected/current hashes in transition ledger; not rewritten or promoted.',inherited_outcome_transition=diagnostic,raw_occurrences='Retained old raw logs + targeted privacy reruns reconstruct189 inherited nonpass occurrences. Final has190:one added source-pin failure and one existing ERROR now FAIL, not removed. Identity-only comparison cannot capture repeated subtests.',current_new_managed_nonpasses=0,unresolved_investigative_probe=pin(HERE/'options-policy-diagnostic.json'),actual_P1_adverse_conditions=15,adverse=adverse['receipt']))
 text='''# Closeout addendum: NOT_READY_04A38

Read owner-review.md and readiness-closeout-v2.json for the current source/ownership checkpoint. W2 IN_PROGRESS; W3 NOT_STARTED. No new original reference is authorized or executed.

The final Python result is766 PASS/180 FAIL/10 ERROR, reported exactly. The extra PlotData source-pin failure remains FAIL. The inherited SPC source test iterates an unordered set: the final full run encounters the preexisting ComputeYAxisBounds difference (FAIL); earlier runs encounter a preexisting removed helper (ERROR). Two seed-controlled targeted runs both reproduce missing-helper ERROR; their actual results remain recorded, not substituted for the final full run. Raw-source/test/renderer bytes were not changed. No inherited nonpass occurrence disappeared.

The latest P1 adverse suite is15 PASS. An earlier export-name mutation was mislabeled as a default mutation; its valid name-corruption result is preserved but is not evidence for a default condition. The new version labels the name mutation honestly and separately mutates a real optional argument's default type/value; both conditions are reached and rejected. P0 adverse9 and publication adverse14 also pass. Earlier failed/invalid fixtures remain unchanged.

Review-copy verification proves only included bytes/source/CIL metadata. Private loaded DLLs/package cache and omitted files/Git/ignored history cannot be revalidated from that copy. The final external export receipt provides actual ZIP size/hash/entries/CRC after creation; it is outside the ZIP to avoid self-hashing. No scientific/native or owner acceptance is granted.
'''
 with (HERE/'owner-review-closeout-v2.md').open('x',encoding='utf-8') as f:f.write(text)
 commands=[]
 for folder in sorted((RUNROOT/'commands').glob('w2c1-*')):
  if (folder/'command.json').is_file():commands.append(dict(label=folder.name,receipt=clean(json.loads((folder/'command.json').read_bytes()))))
 save('command-receipts-closeout-v2.json',dict(commands=commands,previous=pin(HERE/'command-receipts.json'),later_final_manifest_and_export_verification='Private immutable Runner receipts and external export receipt; not cyclically included in the ZIP they hash',all_failed_attempts='Retained, including diagnostics and deliberately reproduced invalid-reader fixture'))
 print('Final qualification and command addendum recorded')
if __name__=='__main__':main()
