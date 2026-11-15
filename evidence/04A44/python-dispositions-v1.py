from common44 import *
import ast,collections
lock();d=load(HERE/'python/parent-event-reconciliation-v1.json');pairs={r['stable_event_id']:r for r in load(PRIVATE/'python-event-pairs-v1.json')['events']};freeze=load(HERE/'matrix/source-freeze-v1.json');entry=load(HERE/'entry/source-preimages-v1.json');old_modified=set(load(ROOT/'evidence/04A43/final/git-final-v1.json')['modified']);new_modified=set(git('diff','--name-only').decode().splitlines());tests_project='BESHStatNG.Test/BESHStatNG.Tests.vbproj'
guards=['test_no_designer_importer_targets_exports_or_signing_change','test_udf_exports_and_packaging_unchanged','test_no_designer_udf_numerical_packaging_or_framework_change'];rows=[]
old43pins={r['path']:r['sha256'] for r in load(ROOT/'.test-rationalization-04A43.tmp/python-input-freeze-v1.json')['files']}
def capture_equal(case,a,b):
 try:case.assertEqual(a,b)
 except AssertionError as e:return str(e)
 raise AssertionError('EXPECTED_LITERAL_SOURCE_ASSERTION_DID_NOT_FAIL')
for row in d['events']:
 r=dict(row);pair=pairs[r['stable_event_id']];a=pair['before'];b=pair['current'];identity=r['identity']
 if r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW':
  if 'test_removal_delegation_preserves_untyped_lists' in identity:
   test=module('original_08_test',ROOT/'scripts/tests/test_04a08_authoring_successor.py');c=test.c;path='BESHstatNG/src/DM/UIprocedures.vb';old=c.members(c.original(path));pre=c.members((PRIVATE/'source-preimages'/path).read_text(encoding='utf-8-sig'));now=c.members((ROOT/path).read_text(encoding='utf-8-sig'));introduced={n for n in pre if pre[n]!=now[n]};allchanged={n for n in old if old[n]!=now[n]}
   expected={'FormatProgressElapsedSeconds','FormatProgressDouble','TryParseUiDouble','TryParseUiInteger','FormatUiDouble'};assert introduced==expected and {n for n in old if old[n]!=pre[n]}=={'Remove_Item'} and old['Remove_Item']!=pre['Remove_Item'] and pre['Remove_Item']==now['Remove_Item']
   case=test.Authoring08SuccessorTests('test_removal_delegation_preserves_untyped_lists');assert capture_equal(case,{'Remove_Item'},allchanged)==b['message']
   r.update(disposition='NEW_CURRENT_FAIL_LITERAL_04A08_SINGLE_METHOD_SCOPE;UNWAIVED;NOT_BEHAVIORAL_REGRESSION',proof=dict(unchanged_original_expected_set=['Remove_Item'],actual_changed_from04A08_original=sorted(allchanged),authorized44_methods=sorted(introduced),Remove_Item_exact_predecessor=True,source_test=pin(ROOT/'scripts/tests/test_04a08_authoring_successor.py'),scope='Original08 restricts every UIprocedures method outside Remove_Item.44 explicitly authorizes five pure format/parse extractions. Failure remains FAIL;no assertion/baseline rewrite or waiver.',software_evidence=[pin(HERE/'fixtures/Debug-44-v2.json'),pin(HERE/'fixtures/Release-44-v2.json'),pin(HERE/'compatibility/Debug-v1.json'),pin(HERE/'build-and-tests/Debug-identities-v1.json')]))
  elif any(s in identity for s in ['test_all_prechange_protected_bytes_except_exact_approved_scope','test_exact_scope_and_external_owner_plan_are_separate']):
   phase='13' if 'test_04a13_' in identity else '14';test=module('original_scope_'+phase,ROOT/('scripts/tests/test_04a'+phase+('_survival_workflow.py' if phase=='13' else '_mixed_workflow.py')));c=test.c;pins=load(c.RUN/'baseline.json')['source_pins'];allowed=c.CHANGED if phase=='13' else c.CHANGED|{c.OWNER_PLAN};assert not set(pins)-set(old43pins)
   previous=[p for p,h in pins.items() if p not in allowed and old43pins[p]!=h];current=[p for p,h in pins.items() if p not in allowed and pin(p)['sha256']!=h];case=(test.Survival13SourceContracts if phase=='13' else test.Mixed14SourceContracts)()
   assert capture_equal(case,[],previous)==a['message'] and capture_equal(case,[],current)==b['message']
   added=set(current)-set(previous);removed=set(previous)-set(current);assert not removed and added<={r['after']['path'] for r in freeze['source_delta']}
   r.update(disposition='HISTORICAL_PROTECTED_SOURCE_SCOPE_FAIL_RETAINED;AUTHORIZED44_SOURCE_ARGUMENT_ADDITIONS',proof=dict(actual_before_argument_exact=True,actual_current_argument_exact=True,added_paths=sorted(added),removed_paths=[],authorized_transitions=[t for t in freeze['source_delta'] if t['after']['path'] in added],source_test_unchanged=True))
  elif 'test_publication_membership_hashes_empty_index_and_privacy' in identity or 'test_publication_exact_membership_hashes_and_empty_index' in identity:
   left=ast.literal_eval(a['message']);right=ast.literal_eval(b['message']);assert left==old_modified-{tests_project} and right==new_modified-{tests_project}
   r.update(disposition='HISTORICAL_PUBLICATION_ALLOWLIST_SCOPE_FAIL_RETAINED',proof=dict(before_paths=sorted(left),current_paths=sorted(right),unchanged_historical_allowlist_contains_test_project=True,original_assertion_unchanged=True))
  else:
   first=lambda s:[ast.literal_eval(x) for x in s.splitlines()[0].split(' != ',1)];aa=first(a['message']);bb=first(b['message']);positions=[i for i in range(2) if aa[i]!=bb[i]];assert len(positions)==1;i=positions[0];before=aa[i];after=bb[i]
   kind='BRANCH_CONTEXT' if (before,after)==('codex/04A-43','codex/04A-44') else 'MERGED_MAIN_CONTEXT' if (before,after)==(PRIOR,MAIN) else 'AUTHORIZED_PLAN_CONTEXT' if (before,after)==(entry['plan']['sha256'],freeze['plan']['sha256']) else 'AUTHENTICATED_OWNER43_NEWLINE_CONTEXT' if any(t['sealed']['sha256']==before and t['current']['sha256']==after for t in load(HERE/'entry/authentication-v1.json')['merged43_newline_transitions']) else None
   assert kind,'UNEXPLAINED_ASSERTION_ARGUMENT_TRANSITION'
   r.update(disposition='HISTORICAL_'+kind+'_FAIL_RETAINED',proof=dict(before_arguments=aa,current_arguments=bb,unchanged_expected_argument=aa[1-i],source_test_unchanged=True))
 rows.append(r)
assert not any(r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW' for r in rows)
parent_transitions=[]
for p in d['summary']['changed_parents']:
 if p['id'].rsplit('.',1)[-1] in guards:
  assert p['before']=='FAIL' and p['outcome']=='PASS' and not any('UDF' in v.upper() or v.endswith('.dna') for v in new_modified)
  proof='Actual44 diff contains no ExcelUDFs/Core algorithms/designer/dna change. Original unchanged assertion now passes for44 scope. Historical43 FAIL/trace remains FAIL/unwaived.'
 else:
  assert p['id'].endswith('.test_removal_delegation_preserves_untyped_lists') and p['before']=='PASS' and p['outcome']=='FAIL'
  proof='Literal08 single-method allowed set now includes five explicitly authorized44 helper extractions. Current FAIL retained;Remove_Item body unchanged;no software behavior defect.'
 parent_transitions.append(dict(p,proof=proof))
assert len(parent_transitions)==4
changed=[r for r in rows if r['classification']!='RETAINED_IDENTICAL']
save('python/diagnostic-dispositions-v1.json',dict(status='ALL_CURRENT_EVENTS_AND_PARENT_TRANSITIONS_ACCOUNTED;FAILS_UNWAIVED',rows=changed,parent_transitions=parent_transitions,counts=dict(collections.Counter(r['disposition'] for r in changed)),unresolved=0,every_parent_transition_explained=True,historical_outcomes_retained=True,tests_and_assertions_modified=False,histories=[pin(p) for p in ['evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json','evidence/04A40/python/diagnostic-dispositions-v3.json','evidence/04A41/python/diagnostic-dispositions-v1.json','evidence/04A42/python/diagnostic-dispositions-v1.json','evidence/04A43/python/diagnostic-dispositions-v1.json']],raw_pairs=pin(PRIVATE/'python-event-pairs-v1.json'),gates=GATES))
guardrows=[]
for name in guards:
 p=next(p for p in d['parents'] if p['id'].endswith('.'+name));e=next(r for r in rows if r['identity']==p['id']);assert p['before']=='FAIL' and p['outcome']=='PASS'
 guardrows.append(dict(identity=p['id'],historical43_status='FAIL',current_status=p['outcome'],source=p['source'],old43_diagnostics=e['before'],current44_failure_diagnostics=e['current'],historical_scope='FAIL_UNWAIVED;NO_OLD_EXPECTATION_REWRITE',current_scope='Actual44 unchanged assertion against current full tracked diff;no UDF/Core/designer/dna changes.',waived=False,prior43=pin(ROOT/'evidence/04A43/python/source-guard-dispositions-v1.json')))
save('python/source-guard-dispositions-v1.json',dict(guards=guardrows,status='HISTORICAL_THREE_FAILS_RETAINED;CURRENT44_SCOPE_PASS_SEPARATE',actual_current_git_diff=sorted(new_modified)))
save('python/parent-event-reconciliation-v2.json',dict(d,summary=dict(d['summary'],unresolved_diagnostic_events=[],diagnostic_dispositions=pin(HERE/'python/diagnostic-dispositions-v1.json')),events=rows,supersedes=pin(HERE/'python/parent-event-reconciliation-v1.json')))
print('All',len(changed),'event differences and4 parent transitions exactly dispositioned;historical FAILs and new literal08 FAIL retained',flush=True)
