"""Every actual original new-parent/callback fragment, with exact real source matches."""
from common import *
from verify_composition_v2 import tokens,PHASES,unwrap,inspect
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
callbacks=json.loads((HERE/'callback-proofs-v1.json').read_bytes())['rows']
u=json.loads((HERE/'current-universe-packet-v2.json').read_bytes())
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
portable=json.loads((HERE/'portable-api-contracts-v1.json').read_bytes())['contracts']
new={c['original_record']['symbol_id']:c for c in contracts+callbacks if c['new_support'] and c['original_id']}
composition=inspect();save('source-composition-proof-v1.json',composition)
rows=[]
for fragment in original['fragments']:
    if fragment['member_id'] not in new:continue
    c=new[fragment['member_id']];is_callback='current_record' in c and c['current_record']['parent_member'] is not None
    pool=[f for f in u['fragments'] if f['member_id']==c['current_id']]
    if not is_callback:
        graph=set(c['compiled'][0]['transitive']['methods'])
        for p in portable:
            if p['compiled'][0]['method']['method'] in graph:pool.extend(p['fragments'])
    name=c['current_record']['name'];code=fragment['code'];bound=code
    if name in PHASES:
        for a,b in PHASES[name][2].items():bound=bound.replace(a,b)
    matches=[]
    for f in pool:
        if f['kind']!=fragment['kind']:continue
        if any(tokens(f['code'])==tokens(x) or tokens(unwrap(f['code']))==tokens(x) for x in [code,bound]):matches.append(f['fragment_id'])
    rows.append(dict(fragment_id=fragment['fragment_id'],original_parent=c['original_id'],original_source_record=fragment,
        current_parent=c['current_id'],matched_current_fragments=matches,bound_source=bound,
        proof='EXACT_SOURCE_UNDER_ACTUAL_ARGUMENT_BINDINGS' if matches else 'EXPLICIT_WHOLE_PARENT_COMPOSITE_RECONSTRUCTION',
        composition='source-composition-proof-v1.json',dynamic_coverage='NOT_INFERRED_FROM_STATIC_CORRESPONDENCE'))
unmatched=[r for r in rows if not r['matched_current_fragments'] and (r['original_source_record']['is_branch'] or r['original_source_record']['is_inline'])]
save('new-parent-fragment-crosswalk-v1.json',dict(rows=rows,original_fragments=len(rows),
    exact_matches=sum(bool(r['matched_current_fragments']) for r in rows),composite=sum(not r['matched_current_fragments'] for r in rows),
    unmatched_branch_inline=unmatched,original_queries=0))
print('Original fragments',len(rows),'unmatched branch/inline',len(unmatched))
for r in unmatched:print(r['original_source_record']['kind'],r['original_source_record']['code'][:120])
assert not unmatched,'Keep failed ledger; resolve complete real arms, never fabricate fragments'
