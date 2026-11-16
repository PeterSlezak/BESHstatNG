from common45 import *
import ast,collections
lock();d=load(HERE/'python/parent-event-reconciliation-v1.json');pairs={p['stable_event_id']:p for p in load(PRIVATE/'python-event-pairs-v1.json')['events']};rows=[]
entry=load(HERE/'entry/source-preimages-v1.json');plan=load(HERE/'planning/authority-v1.json');old_modified=set(load(ROOT/'evidence/04A44/final/git-final-v1.json')['modified']);new_modified=set(git('diff','--name-only').decode().splitlines());test_project='BESHStatNG.Test/BESHStatNG.Tests.vbproj'
for row in d['events']:
 r=dict(row)
 if r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW':
  pair=pairs[r['stable_event_id']];a=pair['before'];b=pair['current'];first=lambda value:[ast.literal_eval(v) for v in value.splitlines()[0].split(' != ',1)]
  aa=first(a['message']);bb=first(b['message']);identity=r['identity']
  if 'test_publication_' in identity:
   assert len(aa)==len(bb)==1 and aa[0]==old_modified-{test_project} and bb[0]==new_modified-{test_project}
   kind='HISTORICAL_ALLOWLIST_CURRENT_DIFF_ARGUMENT_CHANGE';proof=dict(before_paths=sorted(aa[0]),current_paths=sorted(bb[0]),expected_allowlist_unchanged=True)
  else:
   assert len(aa)==len(bb)==2;positions=[i for i in range(2) if aa[i]!=bb[i]];assert len(positions)==1;i=positions[0]
   before,after=aa[i],bb[i]
   if (before,after)==('codex/04A-44','codex/04A-45'):kind='ACTUAL_BRANCH_CONTEXT'
   elif (before,after)==(PRIOR,MAIN):kind='ACTUAL_MERGED_MAIN_CONTEXT'
   elif (before,after)==(plan['entry']['sha256'],plan['current']['sha256']):kind='AUTHORIZED_PLAN_REVISION_CONTEXT'
   else:raise AssertionError('UNEXPLAINED_ARGUMENT_CHANGE')
   proof=dict(before_arguments=aa,current_arguments=bb,unchanged_expected_argument=aa[1-i],exact_source_context_authenticated=True)
  r.update(disposition=kind+'_FAIL_RETAINED_UNWAIVED',proof=dict(proof,source_test_unchanged=True,assertions_modified=False))
 rows.append(r)
assert not any(r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW' for r in rows)
assert not d['summary']['changed_parents'] and len(d['parents'])==956
changes=[r for r in rows if r['classification']!='RETAINED_IDENTICAL'];assert len(changes)==15
guards=load(ROOT/'evidence/04A44/python/source-guard-dispositions-v1.json');scope=load(ROOT/'evidence/04A44/register/issue-delta-v2.json')['Python_validation']['new_source_scope_guard'];assert scope['current_status']=='FAIL'
save('python/dispositions-v1.json',dict(status='ALL15_CHANGED_DIAGNOSTICS_EXACTLY_ACCOUNTED_NO_PARENT_TRANSITIONS',rows=changes,unresolved=0,historical_failures_unwaived=True,historical04A08_scope_status='FAIL_UNWAIVED',current04A08_scope_outcome=next(p['outcome'] for p in d['parents'] if p['id'].endswith('.test_removal_delegation_preserves_untyped_lists')),three_historical_UDF_guards=guards,accepted44_new_scope_failure=scope,changed_parents=[],parents=956,parent_outcomes=d['summary']['parent_outcomes'],historical_1941_privacy_findings_each='RETAINED_UNWAIVED;NOT_NEW_CURRENT_CANDIDATE_FINDINGS',W3_privacy_errors='RETAINED_UNWAIVED',prior_diagnostic_histories=[pin(p) for p in ['evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json','evidence/04A40/python/diagnostic-dispositions-v3.json','evidence/04A41/python/diagnostic-dispositions-v1.json','evidence/04A42/python/diagnostic-dispositions-v1.json','evidence/04A43/python/diagnostic-dispositions-v1.json','evidence/04A44/python/diagnostic-dispositions-v1.json']],actual_peak_workers=3,permitted_ceiling=6,discovery_report_max_workers_is_ceiling=True,original_tests_assertions_and_scanners_unchanged=True,gates=GATES))
save('python/parent-event-reconciliation-v2.json',dict(d,events=rows,summary=dict(d['summary'],unresolved_diagnostic_events=[],diagnostic_dispositions=pin(HERE/'python/dispositions-v1.json')),supersedes=pin(HERE/'python/parent-event-reconciliation-v1.json')))
print('Python956:834PASS/111FAIL/11ERROR;15 exact diagnostic argument changes;zero parent transitions;all historical FAILs unwaived',flush=True)
