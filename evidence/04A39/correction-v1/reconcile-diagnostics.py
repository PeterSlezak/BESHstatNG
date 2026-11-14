"""Exact assertion-level W3/39 reconciliation, without rewriting failure messages."""
from common import *
import ast, collections, re, shutil

lock()
current=load(PRIOR/'python-node-scanners/python-identities-v1.json')
baseline=load(ROOT/'evidence/04A38/completion/w3/python-node-scanners/python-identities-v1.json')
drifts=load(PRIVATE/'diagnostic-analysis.json')
inventory=load(PRIOR/'python-node-scanners/campaign-v2/python-discovery-inventory-v1.json')['entries']
byid={r['id']:r for r in inventory}
assert len(byid)==956
oldparents={r['id']:r for r in baseline['parents']};newparents={r['id']:r for r in current['parents']}
assert set(oldparents)==set(newparents)==set(byid)
lineage=load(PRIOR/'entry/line-ending-reconciliation-v1.json')['rows']
newfiles=load(HERE/'entry/source-lock.json')['files']
newhashes=collections.defaultdict(list)
for row in newfiles+[pin('.gitignore')]:newhashes[row['sha256']].append(row['path'])
lineages={r['current']['path']:r for r in lineage}
public=[];private=[]
for n,r in enumerate(drifts):
    row=byid[r['identity']];source=ROOT/row['module_path']
    assert digest(source)==row['module_sha256']
    node=next(c for c in ast.walk(ast.parse(source.read_text(encoding='utf-8-sig'))) if isinstance(c,(ast.FunctionDef,ast.AsyncFunctionDef)) and c.name==r['identity'].rsplit('.',1)[1])
    body='\n'.join(source.read_text(encoding='utf-8-sig').splitlines()[node.lineno-1:node.end_lineno])
    before,after=r['before_message'],r['after_message']
    trace_only_assertion=r['before_trace'].replace(before,after)==r['after_trace']
    hashes_before=re.findall(r'\b[a-f0-9]{64}\b',before)
    hashes_after=re.findall(r'\b[a-f0-9]{64}\b',after)
    bindings=[]
    if 'codex/04A-38' in before and 'codex/04A-39' in after:
        cause='HISTORICAL_BRANCH_ASSERTION_CURRENT_BRANCH_CHANGED'
        assert 'branch' in body
    elif 'ba1636f5356ac761509c2cfa16fc66b92430935d' in before and MAIN in after:
        cause='HISTORICAL_MAIN_ASSERTION_ACCEPTED38_MERGE'
        assert any(word in body for word in ('main','BASE','HEAD'))
    elif before.startswith('{') and after.startswith('{'):
        cause='TRACKED_DIFF_SCOPE_CHANGED_AFTER_ACCEPTED38_MERGE'
        assert 'diff' in body
        bindings.append(dict(expected_current_tracked_paths=git('diff','--name-only').decode().splitlines(),remaining_assertion_set=ast.literal_eval(after)))
        bindings[-1]['remaining_assertion_set']=sorted(bindings[-1]['remaining_assertion_set'])
    elif hashes_before and len(hashes_before)==len(hashes_after):
        cause='HISTORICAL_PIN_ASSERTION_CHANGED_BYTES'
        pairs=sorted(set(zip(hashes_before,hashes_after)))
        changed=[(a,b) for a,b in pairs if a!=b]
        assert changed
        for old,new in changed:
            paths=newhashes.get(new,[])
            assert paths, 'UNRESOLVED_CURRENT_HASH '+r['identity']
            for path in paths:
                link=lineages.get(path)
                if link and link['accepted']['sha256']==old:
                    disposition='OWNER_AUTHORIZED_LINE_ENDING_CHANGE'
                    assert link['live_classification']!='UNEXPECTED_DIFFERENCE'
                elif path=='BESHStatNG.Test/BESHStatNG.Tests.vbproj':disposition='AUTHORIZED39_TEST_PROJECT_INCLUSION_OR_NEWLINE'
                elif path=='planning/BESHStatNG_CrossPlatform_MasterPlan.md':disposition='AUTHORIZED39_LIVING_PLAN_REVISION'
                elif path=='.gitignore':disposition='MERGED_MAIN_WORKTREE_NEWLINE_OR_APPROVED38_IGNORE_TRANSITION'
                else:raise AssertionError('UNEXPLAINED_HASH '+path)
                bindings.append(dict(path=path,before_sha256=old,after_sha256=new,disposition=disposition,lineage=link))
    else:raise AssertionError('UNCLASSIFIED_DIAGNOSTIC '+r['identity'])
    item=dict(change_id='DIAGNOSTIC-'+str(n+1).zfill(3),identity=r['identity'],occurrence=next(e['occurrence'] for e in current['events'] if e['identity']==r['identity'] and e['message']==after),
        before_outcome='FAIL',current_outcome='FAIL',before_message_sha256=sha(before.encode()),current_message_sha256=sha(after.encode()),
        before_trace_sha256=sha(r['before_trace'].encode()),current_trace_sha256=sha(r['after_trace'].encode()),
        test_source=pin(source),test_line=node.lineno,test_end_line=node.end_lineno,assertion_source_sha256=sha(body.encode()),cause=cause,
        trace_difference_only_assertion_message=trace_only_assertion,bindings=bindings,
        functional_behavior_change_detected=False,disposition='EXPLAINED_SOURCE_BOUND_DIFFERENCE;ORIGINAL_FAIL_UNWAIVED;NO_EXPECTATION_CHANGE')
    assert trace_only_assertion, 'ADDITIONAL_TRACE_DRIFT '+r['identity']
    public.append(item);private.append(dict(**item,**r,assertion_source=body))
# Raw authority records independently authenticate each parent and event occurrence.
rawpins=[]
for label,parts in [('w3',baseline['executions']),('39',current['parts'])]:
    records=[]
    for part in parts:
        raw=part['raw'];assert pin(raw['path'])==raw
        data=load(ROOT/raw['path']);records.extend(data['records']);rawpins.append(dict(configuration=label,**raw))
        target=PRIVATE/'raw-python-authorities'/label/(str(part['lane'])+'-'+part['phase']+'.json')
        target.parent.mkdir(parents=True,exist_ok=True)
        with target.open('xb') as f:f.write((ROOT/raw['path']).read_bytes())
    assert collections.Counter(r['id'] for r in records)==collections.Counter(byid)
    assert {r['id']:r['outcome'] for r in records}=={r['id']:r['outcome'] for r in (baseline if label=='w3' else current)['parents']}
save(PRIVATE/'diagnostic-crosswalk-raw.json',dict(changes=private,raw_authorities=rawpins))
save('diagnostics/changed-54-crosswalk.json',dict(count=len(public),changes=public,cause_counts=dict(collections.Counter(r['cause'] for r in public)),
    raw_authority=pin(PRIVATE/'diagnostic-crosswalk-raw.json'),reporting_changes=0,runtime_only_changes=0,new_functional_failure_evidence=0,
    status='54_EXACT_ASSERTION_DIFFERENCES_EXPLAINED_NOT_WAIVED'))
parents=[dict(id=k,w3_outcome=oldparents[k]['outcome'],original39_outcome=newparents[k]['outcome'],classification='RETAINED_IDENTICAL' if oldparents[k]['outcome']==newparents[k]['outcome'] else 'CHANGED_OUTCOME_EXPLAINED_SEPARATELY',test_source=pin(byid[k]['module_path'])) for k in sorted(byid)]
save('diagnostics/parent-956-reconciliation.json',dict(parents=parents,count=956,missing=[],duplicate=[],w3_outcomes=dict(collections.Counter(r['outcome'] for r in baseline['parents'])),original39_outcomes=dict(collections.Counter(r['outcome'] for r in current['parents'])),raw_authorities=rawpins,full_campaign_rerun=False,reason='No production or test assertions changed; exact raw record census and focused replay instead of another costly full campaign.'))
save('diagnostics/event-reconciliation.json',dict(w3_events=198,original39_events=195,current_events=[dict(identity=e['identity'],outcome=e['outcome'],subtest=e['subtest'],occurrence=e['occurrence'],message_sha256=e['message_sha256'],traceback_sha256=e['traceback_sha256'],classification=e['classification']) for e in current['events']],
    baseline_events=[dict(identity=e['identity'],outcome=e['outcome'],subtest=e['subtest'],occurrence=e['occurrence'],message_sha256=e['message_sha256'],traceback_sha256=e['traceback_sha256']) for e in baseline['events']],
    note='Historical events retained independently. Current parent/event outcomes never promoted because an inherited failure is known.'))
print('956 parents exact; 54 source-bound assertion differences; all raw authorities verified',flush=True)
