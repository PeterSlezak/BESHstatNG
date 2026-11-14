"""Document full source dataflow and the single bounded owner decision."""
import json
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
def load(p):return json.loads(p.read_bytes())
def save(n,v):
 with (HERE/n).open('x',encoding='utf-8',newline='\n') as f:json.dump(v,f,ensure_ascii=False,indent=2);f.write('\n')
def write(n,text):
 with (HERE/n).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
def main():
 entry=load(ROOT/'evidence/04A38/entry-members.json')['members'];matrix=load(HERE/'member-behavior-evidence-target-matrix-v3.json')['members'];byid={m['canonical_original_id']:m for m in matrix};rows=[]
 for m in entry:
  p=byid[m['symbol_id']];tokens=m['body_tokens'];rows.append(dict(member_id=m['symbol_id'],path=m['path'],source_lines=[m['start_line'],m['end_line']],signature=m['signature'],body_token_sha256=m['body_token_sha256'],public_callers=p['public_callers'],
   assignment_effects=m['assignments'],created_values=m['creation'],exception_boundaries=m['catches'],
   byref_parameters='ByRef' in m['signature'],source_row_symbols=[s for s in ['SourceRowsUsed','SourceRowsDropped','SourceRow','sourceRows','rowIndices','rowMask','keptRows'] if s in tokens],
   array_boundary_operations=[c for c in m['calls'] if any(s in c['text'] for s in ['GetLength','GetLowerBound','GetUpperBound','Get2D','Clone','Copy'])],
   culture_number_missing_error_operations=[c for c in m['calls'] if any(s in c['text'] for s in ['IsMissingArg','IsBlankCell','TryGetDouble','TryGetFinite','CultureInfo','ToString','LoggedUdf','ExcelError'])],
   target_owner=p['target_owner_plan'],actual_compiled_portable_edges=p['actual_portable_edges'],preedit_witness_set=m['symbol_id'],late_case_index='authorized-late-index-v3.json',
   status='SOURCE_DATAFLOW_ACCOUNTED;W2_SEMANTIC_BOUNDARY_IMPLEMENTATION_AND_WHOLE_BODY_REVIEW_STILL_REQUIRED'))
 save('member-state-row-culture-error-crosswalk.json',dict(status='EXACT_ORIGINAL_ASSIGNMENTS_CREATIONS_CALLS_AND_EXCEPTION_BOUNDARIES;NOT_RUNTIME_COVERAGE',members=rows))
 gaps=load(HERE/'consolidated-reference-gaps.json')['essential_gaps']
 content='''# Consolidated owner decision after Approval F and the whole-universe survey

**MMRM_INTERCEPT_GROUP_GATE_PASS + NOT_READY_04A38. W2/W3 have not started.**

This is the single consolidated identifiable original-reference frontier from
the fresh41-source survey, not an additional capture authority. Original source,
all704 historical behavioral identities,719 current identities,317 distinct
supporting declarations,20,683 original statement/inline fragments and5,511
original branch/short-circuit nodes are indexed. These are overlapping syntax
populations, not additional semantic cases or claims that ownership is closed.
The87 conservative text/error candidates were reviewed individually. Existing
array/caught-error evidence, true A–F traces, native acquisition boundaries,
constructed-model invariants and source-preserving typed composition account for
the other candidates. A missing branch fixture alone was not turned into a case.

The smallest proposed single additional gate has **two case kinds × two cultures
= four cases per configuration**, in Debug and Release: **eight comparison
results total**. These would be new late archived-entry compatibility captures,
not scientific controls. Nothing below is materialized or executed.

'''
 for i,g in enumerate(gaps,1):
  content+=f"## {i}. {g['gap_id']}\n\nCanonical original identity:\n`{g['member_id']}`.\n\nOriginal lines {', '.join(map(str,g['original_lines']))}: {g['condition']}.\n\n{g['rationale']}\n\nPublic order: `{g['public_entry']}({', '.join(g['public_argument_order'])})`.\n\n"
  for k,v in g['prospective_input'].items():content+=f'- **{k}**: {v}.\n'
  content+='\nOne case in Invariant and one in sk-SK per build configuration. Expected public output, error/exception representation and private ByRef observations remain **UNKNOWN_NOT_CAPTURED**.\n\n'
 content+='''For both kinds, authenticate pinned original source and transitive dependencies
again, freeze exact CLR inputs and genuine factory/session context before any
original model/query, prove the particular original source true arm through the
public export, seal plain/repeat/trace outputs before current compilation, then
compare exactly and test independent source/branch/chronology/manifest readers.
Preserve all A–F evidence, previous decisions and failed attempts. Do not add an
alias, unknown-name variant, alternative factory, public contrast-family case,
extra culture or fifth case/configuration without separate authority.

The original GEE fitted names Intercept|X1|X2 are already recorded in ApprovalB;
the proposed GEE query must resolve the real Intercept in that same original
context. The MMRM proposal retains the existing54-row/six-column factory and C–F
spec; its3x1 at must decline the MMRM-only headerless probe before reaching the
wide second-row guard. Source reachability is prospective, never a captured
result or guaranteed PASS. These different public paths cannot borrow each
other's errors as an oracle.

Owner master continuation W1.3.6 and operating rule1 require stopping before W2
production work or further reference execution when these essential gaps remain.
ApprovalF is exhausted. The remaining migration, whole-body/inline/support
dispositions, real portable consumers and W3 validation remain mandatory; they
are not reclassified HOST_ONLY or deferred to04A.39.
'''
 write('owner-decision-needed.md',content)
 changed=sum(not r['body_tokens_unchanged'] for r in matrix);edged=sum(bool(r['actual_portable_edges']) for r in matrix)
 save('readiness.json',dict(status='NOT_READY_04A38',wave='04A38-W1',F_gate='MMRM_INTERCEPT_GROUP_GATE_PASS',W1='REFERENCE_GATE_PASS_AND_EXHAUSTIVE_READ_ONLY_CENSUS_COMPLETE_WITH_CONSOLIDATED_REFERENCE_STOP',W2='NOT_STARTED_AUTHORITY_BLOCKED',W3='NOT_STARTED',
  fresh_counts=dict(original_members=704,current_members=719,support_distinct=317,original_fragments=20683,current_fragments=16946,original_branches=5511,current_branches=4006,original_inline=1397,current_inline=1157,source_files=41,exports=268),
  ownership=dict(historical_whole_bodies_closed=0,historical_whole_bodies_open=704,current_members_pending=719,support_distinct_pending=317,original_members_with_any_current_Core_Common_IL_edge=edged,original_bodies_changed_or_reallocated_since_entry=changed,qualification='An existing edge or syntax review is not whole-body extraction closure; 199/505 inherited progress labels are not counted as fresh completions'),
  essential_gaps=[g['gap_id'] for g in gaps],new_original_queries_beyond_F=0,new_production_changes=0,proposed_additional_cases_per_configuration=4,
  full_ready_verifier='NOT_COMPLETED_NOT_RUN',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),scientific='NOT_GOLDEN',native='NOT_RUN',owner_acceptance=False,git_publication=False,next_batch='04A39_NOT_STARTED'))
 print(json.dumps(dict(status='NOT_READY_04A38',original_bodies_open=704,any_current_portable_edge=edged,changed_or_reallocated_original_bodies=changed,decision='One consolidated4-case/configuration proposal; zero additional executions')))
if __name__=='__main__':main()
