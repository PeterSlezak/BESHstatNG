"""Finalize source/validation authorities after the one frozen Python campaign."""
from common import *
from verify_c8_checkpoint_template_v6 import compute,closure
proof=compute();save('independent-proof-final-v1.json',proof)
save('readiness-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',mandatory_blockers=dict(historical_OPEN=proof['historical']['OPEN'],current_OPEN=proof['current']['OPEN'],distinct_support_OPEN=proof['support']['distinct_OPEN']),fragments=proof['fragments'],P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',original_queries=0))
scopes=[]
for c in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v1-context-v3/current-'+c+'.json')).read_bytes());defs={r['method']:r for r in meta['methods']};roots=json.loads((HERE/'transitive-compiled-context-v1.json').read_bytes())['configurations'][c]['methods']
    assert all(defs[k]['declaring_type'] is not None for k in roots)
    assert not any(isinstance(i['operand'],str) and i['operand'].startswith('FIELD:.') for k in roots for i in defs[k]['instructions'])
    scopes.append(dict(configuration=c,actual_reached_methods=len(roots),all_reached_owners_concrete=True,unresolved_claimed_fields=0,unused_open_constructed_contexts=sum(t['type'] is None for t in meta['types']),scope='Unnamed open-constructed records remain unchanged raw measurements, outside every claimed path; no generic alias or portable closure inferred.'))
save('constructed-context-scope-v1.json',dict(configurations=scopes,field_order_qualification=pin(HERE/'unrelated-field-source-qualification-v1.json'),new_compiler_aliases=[],original_queries=0))
late=dict(A=432,B=12,C=2,D=4,E=2,F=2,G=4)
save('frozen-provenance-v1.json',dict(configurations=[dict(configuration=c,genuine_pre_edit=28966,late=[dict(authority=k,cases=v,classification='LATE_ARCHIVED_ENTRY_SUPPLEMENT') for k,v in late.items()],total=29424,all_exact=True) for c in ['Debug','Release']],comparison=pin(HERE/'compatibility-packet-v1.json'),original_plain_repeat_trace_typed_branch_and_failed_attempts='Immutable prior manifests/protection graph retained. No original source execution/compile/fit/query/capture.',new_original_queries=0,scientific_truth='NOT_GOLDEN'))
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];meta=json.loads((HERE/'metadata-all/packet-v1-context-v3/current-Debug.json').read_bytes());exports=json.loads((HERE/'inspection/packet-v1/Inspect38-Debug.json').read_bytes())['exports']
export_keys={r['effective_excel_name']:[m['method'] for m in meta['methods'] if m['declaring_type'] is not None and m['declaring_type']+'.'+m['member_name']==r['method'].split('(',1)[0]] for r in exports}
inputs=[];cache={}
for r in json.loads((HERE/'compatibility-packet-v1.json').read_bytes())['results']:
    if r['configuration']!='Debug':continue
    for line in (ROOT/r['inputs']['path']).read_text(encoding='utf-8-sig').splitlines():
        value=json.loads(line);target=value.get('target','');keys=export_keys.get(target,[])
        if not keys:keys=[m['method'] for m in meta['methods'] if m['member_name']==target and (not value.get('type') or m['declaring_type']==value['type'])]
        key=tuple(keys)
        if key not in cache:cache[key]=set(closure(meta,keys)['methods']) if keys else set()
        inputs.append(dict(corpus=r['corpus'],input=r['inputs']['path'],case_id=value.get('case_id'),culture=value.get('culture'),target=target,classification='LATE_ARCHIVED_ENTRY_SUPPLEMENT' if r['corpus'] in late else 'GENUINE_PRE_EDIT',reached=cache[key]))
rows=[]
for c in contracts:
    roots=set(c['compiled'][0]['roots']);matches=[{k:v for k,v in r.items() if k!='reached'} for r in inputs if roots&r['reached']]
    rows.append(dict(original_id=c['original_id'],current_id=c['current_id'],fixtures=matches,qualification='Static real call reachability, not a claim every branch executed. Full authentic source reconstruction/CIL/branch/inline proofs cover unwitnessed branches. No new archived reference.'))
save('compatibility-fixture-crosswalk-v1.json',dict(rows=rows,original_queries=0))
print(json.dumps({k:proof[k] for k in ['historical','current','support','fragments']}))
