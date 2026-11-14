"""Every genuine original new body/callback fragment under actual portable bindings."""
from common import *
from verify_c12_composition_v1 import inspect,tokens,unwrap,PHASES
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
callbacks=json.loads((HERE/'callback-proofs-v1.json').read_bytes())['rows']
current=json.loads((HERE/'current-universe-packet-v1.json').read_bytes())
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
portable=json.loads((HERE/'portable-api-contracts-v1.json').read_bytes())['contracts']
assert inspect()==json.loads((HERE/'source-composition-proof-v1.json').read_bytes())
new={c['original_record']['symbol_id']:c for c in contracts+callbacks if c['new_support'] and c['original_id']};rows=[]
for fragment in original['fragments']:
    if fragment['member_id'] not in new:continue
    c=new[fragment['member_id']];pool=[f for f in current['fragments'] if f['member_id']==c['current_id']]
    if not c['current_record']['parent_member']:
        reached=set(c['compiled'][0]['transitive']['methods'])
        for p in portable:
            if p['compiled'][0]['method']['method'] in reached:pool.extend(p['fragments'])
    code=fragment['code'];bound=code;name=c['current_record']['name']
    if name in PHASES:
        for left,right in PHASES[name]['bindings'].items():bound=bound.replace(left,right)
    matches=[f['fragment_id'] for f in pool if f['kind']==fragment['kind'] and any(tokens(f['code'])==tokens(value) or tokens(unwrap(f['code']))==tokens(value) for value in [code,bound])]
    rows.append(dict(fragment_id=fragment['fragment_id'],original_parent=c['original_id'],original_source_record=fragment,current_parent=c['current_id'],matched_current_fragments=matches,bound_source=bound,
        proof='EXACT_SOURCE_UNDER_ACTUAL_ARGUMENT_BINDINGS' if matches else 'EXPLICIT_WHOLE_PARENT_COMPOSITE_RECONSTRUCTION',composition='source-composition-proof-v1.json',dynamic_coverage='NOT_INFERRED_FROM_STATIC_CORRESPONDENCE'))
unmatched=[r for r in rows if not r['matched_current_fragments'] and (r['original_source_record']['is_branch'] or r['original_source_record']['is_inline'])]
save('new-parent-fragment-crosswalk-v1.json',dict(rows=rows,original_fragments=len(rows),exact_matches=sum(bool(r['matched_current_fragments']) for r in rows),composite=sum(not r['matched_current_fragments'] for r in rows),unmatched_branch_inline=unmatched,original_queries=0))
print(json.dumps(dict(original_fragments=len(rows),exact_matches=sum(bool(r['matched_current_fragments']) for r in rows),composite=sum(not r['matched_current_fragments'] for r in rows),unmatched_branch_inline=len(unmatched))))
for row in unmatched:print(row['original_source_record']['kind'],row['original_source_record']['code'])
assert not unmatched,'Preserve failed fragment ledger; never manufacture an arm match'
