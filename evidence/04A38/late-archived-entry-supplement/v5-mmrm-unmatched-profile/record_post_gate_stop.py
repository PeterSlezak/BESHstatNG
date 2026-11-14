"""Record the next sourced reference boundary without materializing or executing it."""
import csv
from common import *
def main():
 assert json.loads((HERE/'MMRM_UNMATCHED_PROFILE_GATE_PASS.json').read_bytes())['status']=='MMRM_UNMATCHED_PROFILE_GATE_PASS'
 observation=json.loads((HERE/'group-selection-coverage-observation.json').read_bytes())
 with (ROOT/'evidence/04A38/ownership-progress-validation-v1-v2.csv').open(encoding='utf-8-sig',newline='') as stream:ledger={r['original_member_id']:r for r in csv.DictReader(stream)}
 source=ROOT/'evidence/04A38/preedit/source/Regression/MixedModelUDFs.vb.txt';lines=source.read_text(encoding='utf-8-sig').splitlines()
 assert lines[1476].strip()=='If String.Equals(h.FixedEffectNames(idx), "Intercept", StringComparison.OrdinalIgnoreCase) OrElse'
 for row in observation['configurations'].values():assert row['helper_entry_cases']==34 and row['report_after_helper']==16 and row['intercept_guard_outputs']==0
 save('source-review-console-attempt.json',dict(status='FAILED_CONSOLE_PRESENTATION_AFTER_OBSERVATION_FILE_SAVED',exit_code=1,
  command=['<PYTHON>','-B',HERE.relative_to(ROOT).as_posix()+'/review_group_selection.py'],exception='UnicodeEncodeError: cp1252 cannot encode U+221E in printed historical culture-specific witness text',
  preserved_source=pin(HERE/'review_group_selection.py'),preserved_complete_observation=pin(HERE/'group-selection-coverage-observation.json'),
  observation_file_written_before_failure=True,original_or_current_model_executions=0,new_reference_executions=0,remedy='Independent reader recomputes raw corpus and emits ASCII-safe summary; no source/statistical/evidence-byte correction',
  other_read_only_attempt='An earlier Get-Content request for nonexistent validate_previous.py was corrected by directory inspection to validate_prior.py; no compiler/capture/test failure'))
 save('remaining-intercept-group-coverage.json',dict(status='NOT_READY_04A38',earliest_failed_phase='POST_UNMATCHED_GATE_SCALAR_GROUP_INTERCEPT_REFERENCE_COVERAGE',
  member_id=observation['member_id'],signature=observation['signature'],inherited_ownership_row=ledger[observation['member_id']],source_snapshot=pin(source),
  missing_source_branch=dict(line=1477,continuation_line=1478,rejection_line=1479,condition=lines[1476].strip(),continuation=lines[1477].strip(),source_error_literal='the intercept column cannot be used as a grouping factor.'),
  observations=pin(HERE/'group-selection-coverage-observation.json'),configurations=observation['configurations'],new_reference_executions=0,new_case_packets=0,new_production_changes=0,
  proposed_public_entry='BESH.REGR.MMRM_LSMEANS',proposed_public_argument_order=['handle','group','alpha'],proposed_semantic_input=dict(group_runtime_type='System.String',group_literal='Intercept',alpha='Nothing',handle='Existing same authenticated successful original MMRM factory and fresh fitted session; not a fabricated handle'),
  proposed_cases_per_configuration=2,proposed_cultures=['Invariant','sk-SK'],configurations_proposed=['Debug','Release'],expected_output='UNKNOWN_NOT_CAPTURED',authorization='NOT_AUTHORIZED_NOT_EXECUTED',
  why_essential='Named grouping is a separate public LS-means/contrast path. Lookup succeeds for the intercept, but this mandatory reusable validation must reject it before a grouping vector is copied or a report is built. Other unknown-column errors, ordinary grouping reports, profile duplicate errors and unmatched rectangular-range errors do not establish that boundary or its ByRef/error propagation.',
  late_E_does_not_reach='MMRM_LSMEANS sees Object[,] range and takes its earlier range-profile arm; scalar grouping uses the later Else arm and TryGetDesignColumnValues. No E case calls the scalar helper.',
  next_action='STOP before another original query, new input packet or production change. Separate explicit owner decision for exactly the proposed two cases/configuration is required by owner continuation Section4.5.',
  qualification='Next independently identified essential source gap, not a claim that every later member is reviewed. No HOST_ONLY substitution or deferred mandatory04A38 extraction.',scientific='NOT_GOLDEN',native='NOT_RUN'))
 save('post-gate-member-review.json',dict(status='SOURCE_REVIEWED_NOT_IMPLEMENTATION_CLOSED',new_production_changes=0,new_compiled_consumer_edges=[],
  mask_member_id='BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb|BESHStatNG.WorksheetFunctions.MixedModelUDFs|FunctionBlock|e8eabf0f978b1297@83928:2886',
  mask_statement_review=[dict(behavior='Handle, saved design/visit, initialized profile and dimension guards',disposition='Typed preparation preconditions; native cache acquisition remains Windows. No new validation phase introduced.'),
   dict(behavior='Per-row visit/fitted-column selection, original comparison policy, source-row mask and matched count',disposition='Reusable numerical selection; genuine four positive masks and new two negative masks/configuration now cover report/empty-mask boundary. Real portable extraction/compiled consumer still pending; not HOST_ONLY.'),
   dict(behavior='Empty mask leaves Boolean54False and profileDescriptionNothing, sets original text error and returnsFalse',disposition='Now independently observed by Approval E in both cultures/configurations, including caller ByRef propagation. Preserve allocated mask and failure phase.'),
   dict(behavior='Nonempty profile description and report use of rowMask',disposition='Existing positive original reports preserve source-row alignment; unchanged source and established formatting policy. No algorithm/report correction authorized.')],
  add_mmrm_review_predecessor=pin(V4/'post-gate-member-review.json'),add_mmrm_implementation='Not closed; visit/design mutable selection still requires real portable consumption',
  next_member_id=observation['member_id'],next_inherited_row=ledger[observation['member_id']],
  scalar_group_statement_review=[dict(behavior='Initialize three ByRef outputs',disposition='Preserve Nothing defaults, including on rejection.'),dict(behavior='Fitted matrix and name-width guards',disposition='Typed model preparation preconditions; native fitted-handle/cache ownership remains Windows.'),dict(behavior='Column resolution',disposition='Existing Common metadata normalization/lookup consumed through Windows wrapper; source confirms index resolves before intercept guard.'),dict(behavior='Intercept exclusion',disposition='Reusable validation, independently uncovered; not host-only and not closed by ordinary reports/unknown-name errors.'),dict(behavior='Column projection and canonical resolved name',disposition='Reusable defensive copy with original row order; existing successful reference rows preserved. Portable extraction pending.')],
  whole_batch='704 historical /719 current /317 support and inline figures remain inherited checkpoints, not a fresh complete census. 199 edge-positive bodies and505 remaining reviews are not closed.',
  exports='268 observed in freshly compiled original/current gate harnesses; complete signature/default/attribute verifier for entire changed batch remains unfinished'))
 print('STOP NOT_READY_04A38: original scalar intercept-group guard1477-1479 has zero independent witnesses; no new case or production change')
if __name__=='__main__':main()
