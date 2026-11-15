"""Keep outcome changes and assertion arguments explicit; no diagnostic is a PASS waiver."""
from common40_v3 import *
import collections,re,ast
lock();d=load(HERE/'python/parent-event-reconciliation-v2.json');raw=load(PRIVATE/'python-event-crosswalk-raw-v2.json')['events'];lookup={r['stable_event_id']:r for r in raw}
rows=[]
for e in d['events']:
    if e['classification']=='RETAINED_IDENTICAL':continue
    r=lookup[e['stable_event_id']];a=r['before'];b=r['current'];kind='UNRESOLVED';proof={}
    if a and b:
        frames=lambda x:re.findall(r'File "([^"\n]+)", line (\d+), in ([^\n]+)',x['traceback'])
        af=frames(a);bf=frames(b);same_frame=bool(af and bf and (Path(af[-1][0]).name,af[-1][1:])==(Path(bf[-1][0]).name,bf[-1][1:]))
        proof['same_final_test_frame']=same_frame
        am=a.get('message','');bm=b.get('message','')
        if 'Second list contains' in am and 'Second list contains' in bm and 'private' in e['identity'].lower() or 'test_entire_proposed_publication_set_privacy' in e['identity']:
            before=re.findall(r'Second list contains (\d+) additional elements',am);after=re.findall(r'Second list contains (\d+) additional elements',bm)
            if before and after:kind='REPOSITORY_CANDIDATE_SET_CHANGED;FINDINGS_FAIL_UNWAIVED';proof.update(before_findings=int(before[0]),current_findings=int(after[0]),original_scan_scope_unchanged=True)
        if kind=='UNRESOLVED' and same_frame:
            # Test modules are exactly frozen; retain the actual failed assertion site,
            # both argument fingerprints and any stable asserted missing-token prefix.
            ap=am.split(' not found in ',1);bp=bm.split(' not found in ',1)
            if len(ap)==len(bp)==2 and ap[0]==bp[0]:
                try:needle=ast.literal_eval(ap[0]);left=ast.literal_eval(ap[1]);right=ast.literal_eval(bp[1])
                except (ValueError,SyntaxError):pass
                else:
                    if isinstance(needle,str) and isinstance(left,str) and isinstance(right,str) and needle not in left and needle not in right:
                        kind='SAME_INHERITED_MISSING_TOKEN_ASSERTION;CHANGED_SOURCE_CONTAINER';proof.update(needle_sha256=sha(needle.encode()),before_container_sha256=sha(left.encode()),current_container_sha256=sha(right.encode()))
            if kind=='UNRESOLVED':
                first=lambda s:s.splitlines()[0] if s else ''
                if first(am)==first(bm):kind='SAME_ASSERTION_HEADER;CHANGED_SOURCE_DIAGNOSTIC_BODY'
                else:kind='SAME_SOURCE_TEST_ASSERTION_SITE;ARGUMENT_DRIFT_REQUIRES_REVIEW'
    if a is None and b is not None:
        kind='HISTORICAL_W3_DIFF_SCOPE_FAIL_REEXPOSED;NOT_WAIVED' if any(x in e['identity'] for x in ['test_04a04_alignment.','test_04a05_completion_successor.','test_04a05_contingency.']) else 'NEW_NONPASS_REQUIRES_REVIEW'
    rows.append(dict(stable_event_id=e['stable_event_id'],identity=e['identity'],classification=e['classification'],disposition=kind,proof=proof,source=e['source'],before=e['before'],current=e['current'],waived=False))
base=load(ROOT/'evidence/04A39/register/defects-v7.json');delta=load(HERE/'register/issue-delta-v3.json');extra=[]
for p in d['summary']['parent_changes']:
    canonical='VALIDATION:PYTHON_PARENT:'+p['id'];match=next((r for r in base['issues'] if r['canonical_id']==canonical),None)
    extra.append(dict(canonical_id=canonical,base_status=match['status'] if match else 'UNRESOLVED_CANONICAL_MAPPING',before=p['before'],current=p['outcome'],
       post_status='CURRENT_SCOPE_FAIL_HISTORICAL_NONPASS_REEXPOSED_UNWAIVED',original_obligations=match['original_obligations'] if match else [],original_controls=match['original_controls'] if match else [],
       reason='Original guards reject any changed ExcelUDFs path. All nine changed UDF parents are explicitly authorized40 preparation plumbing, independently compiled/compatibility-tested; guard FAIL is retained, not converted to PASS.',
       required_owner_action='Review scoped guard applicability; no test-source/expected-result rewrite or original control discharge.',exit_disposition_04A48=None))
save('register/issue-delta-v4.json',dict(base=delta['base'],implemented_issue_decisions=delta['entries'],current_Python_scope_changes=extra,
    new_observations=[dict(id='04A40-PRIVACY-ENTRY-RAW-01',status='OPEN_BLOCKING_PRIVACY',source=pin(HERE/'baseline/Debug-build-v1.json'),
       observed='New entry build raw output-directory path retained in local untracked evidence; six original-rule occurrences across two views, not twelve unique findings.',
       implementation='Raw preserved; deterministic sanitized derivative/physical derivative gate. Original repository-candidate FAIL unchanged.',
       remaining_gate='Exact owner decision for protected Git-candidate representation before merge; no protected historical file modified.',scientific_native_change=False)],
    all_other_1169_canonical_entries='UNCHANGED_NO_DISCHARGE',original_obligations=1514,restricted_controls=802,exit_dispositions_reserved_for_04A48=True,gates=GATES))
save('python/diagnostic-dispositions-v1.json',dict(rows=rows,counts=dict(collections.Counter(r['disposition'] for r in rows)),original54=pin(ROOT/'evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json'),
    original54_retained=True,none_waived=True,unresolved=[r['stable_event_id'] for r in rows if 'REQUIRES_REVIEW' in r['disposition'] or r['disposition']=='UNRESOLVED'],
    raw=pin(PRIVATE/'python-event-crosswalk-raw-v2.json'),full_Python_campaigns_per_state=1,stopped_state1_five_new_nonpasses_retained=True))
print('Changed Python diagnostics',collections.Counter(r['disposition'] for r in rows),flush=True)
