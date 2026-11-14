"""Post-run bookkeeping only; no implementation or historical test mutation."""
from common_v2 import *
def new(old,name,pairs):
    source=(HERE/old).read_text(encoding='utf-8')
    for a,b in pairs:assert a in source,(old,a);source=source.replace(a,b)
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(source)
new('verify_c9_validation_v1.py','verify_c9_validation_v2.py',[("events=load(prefix+'python-first-failures-v1.json')","events=load(prefix+'python-first-failures-v2.json')"),
("need(Counter(map(event_key,events))==Counter(map(event_key,before)),'New/changed Python nonpass requires explicit investigation')","investigation=load(prefix+'python-nonpass-investigation-v1.json')\n    need(investigation['status']=='COMPLETE_SOURCE_BACKED_NONPASS_INVESTIGATION_NOT_WAIVED','Unresolved Python nonpass investigation')\n    need(Counter(map(event_key,events))==Counter(tuple(r) for r in investigation['actual_event_keys']) and Counter(map(event_key,before))==Counter(tuple(r) for r in investigation['baseline_event_keys']),'Investigated actual/baseline nonpass identity drift')\n    need(not investigation['lost_events'],'Historical nonpass event erased')\n    need(all(r['classification']=='AUTHORIZED_C9_SOURCE_PIN_FAIL_NOT_WAIVED' and r['source_transition']['after']['sha256'] in r['message'] for r in investigation['extra_events']),'New nonpass lacks exact source-pin provenance')"),
("counts==Counter(FAIL=181,ERROR=11)","counts==Counter(investigation['actual_event_outcomes'])"),
("need(parents==Counter(PASS=830,FAIL=115,ERROR=11),'Actual parent-method baseline changed')","need(parents==Counter(investigation['actual_parent_outcomes']),'Actual parent-method result differs from investigation')")])
new('verify_c9_checkpoint_template_v2.py','verify_c9_checkpoint_template_v3.py',[
('    def raw(path):return overrides[path] if path in overrides else (root/path).read_bytes()',"    def raw(path):\n        if path in overrides:\n            if overrides[path] is None:raise FileNotFoundError(path)\n            return overrides[path]\n        return (root/path).read_bytes()"),
('from verify_c9_validation_v1 import inspect as validation','from verify_c9_validation_v2 import inspect as validation'),
("    if check_validation:\n        from verify_c9_validation_v2", """    if check_validation:
        comparisons=load(PREFIX+'compatibility-packet-v1.json')['results'];prior={(r['configuration'],r['corpus']):r for r in load(OLD+'compatibility-packet-v1.json')['results']};seen=set();totals=Counter()
        for r in comparisons:
            key=(r['configuration'],r['corpus']);need(key not in seen and key in prior,'Duplicate/unknown frozen pair');seen.add(key);p=prior[key]
            need(all(r[k]==p[k] for k in ['original','inputs','recorder','cases']),'Frozen original/input/recorder chronology changed')
            for k in ['original','inputs','recorder','current']:check(r[k])
            need(r['exact'] and raw(r['current']['path'])==raw(r['original']['path']),'Current frozen output differs')
            need(len([s for s in raw(r['inputs']['path']).splitlines() if s.strip()])==r['cases'],'Frozen input case population changed');totals[r['configuration'],'late' if r['corpus'] in list('ABCDEFG') else 'preedit']+=r['cases']
        need(seen==set(prior) and len(comparisons)==24 and dict(totals)=={(c,p):n for c in ['Debug','Release'] for p,n in [('preedit',28966),('late',458)]},'Incomplete cross-configuration frozen obligation')
        provenance=load(PREFIX+'frozen-provenance-v1.json');need(provenance['new_original_queries']==0,'Unauthorized archived reference capture')
        need({r['configuration'] for r in provenance['configurations']}=={'Debug','Release'} and len(provenance['configurations'])==2,'Dropped provenance configuration')
        for row in provenance['configurations']:
            need(row['genuine_pre_edit']==28966 and row['total']==29424 and {v['authority']:v['cases'] for v in row['late']}==dict(A=432,B=12,C=2,D=4,E=2,F=2,G=4) and all(v['classification']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT' for v in row['late']),'Late/genuine pre-edit provenance changed')
        ready=load(PREFIX+'readiness-v1.json');need(ready['status']=='NOT_READY_04A38' and ready['W2']=='IN_PROGRESS' and ready['W3']=='NOT_STARTED' and ready['mandatory_blockers']==dict(historical_OPEN=summary['historical']['OPEN'],current_OPEN=summary['current']['OPEN'],distinct_support_OPEN=summary['support']['distinct_OPEN']),'False READY or ownership blocker mismatch')
        need((ready['P'],ready['N1'],ready['N2'],ready['C2'],ready['C3'],ready['science'],ready['phase04A39'])==('NO','NO GO','NO','UNMET','UNAPPROVED','NOT_GOLDEN','BLOCKED'),'Unauthorized scientific/native gate change')
        from verify_c9_validation_v2""")])
print('Final frozen/provenance/readiness/nonpass predicates prepared')
