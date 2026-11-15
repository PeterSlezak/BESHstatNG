from common42 import *
import ast,collections,re
lock();d=load(HERE/'python/parent-event-reconciliation-v1.json');pairs={r['stable_event_id']:r for r in load(PRIVATE/'python-event-pairs-v1.json')['events']}
old_modified=set(load(ROOT/'evidence/04A41/final/git-final-v1.json')['modified'])
new_modified=set(git('diff','--name-only').decode().splitlines())
before_plan=load(HERE/'entry/source-preimages-v1.json')['plan'];after_plan=load(HERE/'matrix/source-freeze-v2.json')['plan'];exact(after_plan)
def scalar_args(message):
    first=message.splitlines()[0];left,right=first.split(' != ',1)
    return [ast.literal_eval(left),ast.literal_eval(right)]
rows=[]
for row in d['events']:
    r=dict(row)
    if r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW':
        pair=pairs[r['stable_event_id']];a=pair['before']['message'];b=pair['current']['message']
        if 'test_publication_membership_hashes_empty_index_and_privacy' in r['identity'] or 'test_publication_exact_membership_hashes_and_empty_index' in r['identity']:
            oldarg=ast.literal_eval(a.strip());newarg=ast.literal_eval(b.strip())
            assert oldarg==old_modified and newarg==new_modified
            r['disposition']='HISTORICAL_CHANGED_PATH_SET_ASSERTION;FAIL_RETAINED'
            r['proof']=dict(original_current41_paths=sorted(oldarg),current42_paths=sorted(newarg),actual_git_diff_exact=True,source_test_unchanged=True)
        else:
            args_a=scalar_args(a);args_b=scalar_args(b);positions=[i for i in range(2) if args_a[i]!=args_b[i]]
            assert len(positions)==1;position=positions[0];before=args_a[position];after=args_b[position]
            if (before,after)==('codex/04A-41','codex/04A-42'):
                disposition='HISTORICAL_BRANCH_LITERAL_ASSERTION;FAIL_RETAINED';authority='Explicit current owner42 task and authenticated dedicated branch'
            elif (before,after)==(PRIOR,MAIN):
                disposition='HISTORICAL_MAIN_COMMIT_LITERAL_ASSERTION;FAIL_RETAINED';authority='Authenticated accepted41 merged-main direct ancestry'
            elif (before,after)==(before_plan['sha256'],after_plan['sha256']):
                disposition='HISTORICAL_PLAN_HASH_ASSERTION;FAIL_RETAINED';authority='Exact2.18 snapshot and authorized2.19 reconciliation preserving226 IDs'
            elif any(r['sealed']['sha256']==before and r['current']['sha256']==after for r in load(HERE/'entry/authentication-v1.json')['merged41_newline_transitions']):
                disposition='AUTHENTICATED_OWNER41_MERGE_NEWLINE_SOURCE_HASH_TRANSITION;FAIL_RETAINED'
                authority='Exact sealed41 original bytes/current owner checkout authenticated before42 edits;not a new42 mutation.'
            else:raise AssertionError('UNEXPLAINED_CURRENT_ASSERTION_ARGUMENT_CHANGE')
            r['disposition']=disposition;r['proof']=dict(assertion_arguments_before=args_a,assertion_arguments_current=args_b,unchanged_historical_expected_argument=args_a[1-position],changed_argument_position=position,authority=authority,source_test_unchanged=True)
    rows.append(r)
assert not any(r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW' for r in rows)
changed=[r for r in rows if r['classification']!='RETAINED_IDENTICAL']
assert all(r['classification'] in ['RETAINED_CHANGED_MESSAGE','RETAINED_CHANGED_TRACE'] for r in changed)
save('python/diagnostic-dispositions-v1.json',dict(status='ALL_CURRENT_EVENT_DIFFERENCES_EXACT_ARGUMENT_OR_SCOPE_EXPLAINED;NONPASSES_UNWAIVED',rows=changed,counts=dict(collections.Counter(r['disposition'] for r in changed)),unresolved=0,actual_tests_and_assertions_modified=False,raw_pairs=pin(PRIVATE/'python-event-pairs-v1.json'),original54=pin(ROOT/'evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json'),original32=pin(ROOT/'evidence/04A40/python/diagnostic-dispositions-v3.json'),original21=pin(ROOT/'evidence/04A41/python/diagnostic-dispositions-v1.json'),historical_outcomes_retained=True,repository_privacy_history='Current candidate2PASS parents do not resolve historical39 FAIL1941each or W3ERROR;old40 raw failure snapshots remain.',gates=GATES))
guardnames=['test_no_designer_importer_targets_exports_or_signing_change','test_udf_exports_and_packaging_unchanged','test_no_designer_udf_numerical_packaging_or_framework_change']
guards=[]
for name in guardnames:
    parent=next(r for r in d['parents'] if r['id'].endswith('.'+name));assert parent['outcome']=='FAIL'
    event=next(r for r in rows if r['identity']==parent['id'])
    paths=sorted(p for p in new_modified if (('UDF' in p.upper() or p.endswith('.dna')) if name=='test_udf_exports_and_packaging_unchanged' else p.startswith('BESHstatNG/src/ExcelUDFs/')))
    guards.append(dict(identity=parent['id'],current_status='FAIL',source=parent['source'],old41_diagnostics=event['before'],current42_diagnostics=event['current'],triggering_authorized_paths=paths,applicability='Literal historical no-UDF-diff assertion is currently violated by authorized42 paths. Separate software/ABI proofs do not change or waive the FAIL.',waived=False,old40_scope_disposition=pin(ROOT/'evidence/04A40/owner-authorized-candidate-v3/scope-guard-dispositions-v3.json')))
save('python/source-guard-dispositions-v1.json',dict(guards=guards,actual_campaign='Full956 current parent/event campaign;no test overrides',status='THREE_FAILS_RETAINED_UNWAIVED'))
save('python/parent-event-reconciliation-v2.json',dict(d,summary=dict(d['summary'],unresolved_diagnostic_events=[],diagnostic_dispositions=pin(HERE/'python/diagnostic-dispositions-v1.json')),events=rows,supersedes=pin(HERE/'python/parent-event-reconciliation-v1.json')))
print('All',len(changed),'current diagnostic differences scoped;3 guards FAIL;54+32+21 historical records retained',flush=True)
