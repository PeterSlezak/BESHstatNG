"""Additive passive archived-original graph reconciliation; never an oracle query."""
from common import *
from proof_kernel_v4 import bind,closure,definitions,map_current
sys.path.insert(0,str(PREVIOUS))
from source_views import body
queue=json.loads((HERE/'entry-queue-receipt-v1.json').read_bytes())
prior=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members']
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
current=json.loads((HERE/'current-universe-packet-v1.json').read_bytes())
mapping=map_current(prior['members'],current['members']);oldcm={n['symbol_id']:n for n in prior['members']}
om={n['symbol_id']:n for n in original['members']};hgit={h['git_original_id']:h for h in history}
shapes={r['symbol_id']:r for r in json.loads((PREVIOUS/'declaration-shapes-v2.json').read_bytes())['rows']}
current_shapes={mapping[k]['symbol_id']:v for k,v in shapes.items() if k in mapping}
roots=[r['current_record'] for r in queue['rows'] if r['current_record']['parent_member'] is None]
roots.extend(oldcm[r['current_id']] for r in queue['supplemental_lifecycle'])
graphs={};original_required=set();original_methods={}
for config in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+config+'.json')).read_bytes())
    actual=[]
    for n in roots:
        candidates=[h for h in history if n['symbol_id'] in h['current_ids'] and h['name']!='lambda']
        assert len(candidates)==1
        o=om[candidates[0]['git_original_id']];s=dict(shapes);s[o['symbol_id']]=shapes[n['symbol_id']]
        actual.append(bind(o,meta,s)['method'])
    graph=closure(meta,actual);graphs[config]=graph;defs=definitions(meta)
    pairs={(defs[k]['declaring_type'],defs[k]['member_name']) for k in graph['methods']}
    for o in original['members']:
        if o['parent_member'] is None and (o['owner'],o['name']) in pairs:original_required.add(o['symbol_id'])
    original_methods[config]=dict(metadata=pin(HERE/('metadata-all/packet-v1-context-v5/original-'+config+'.json')),roots=actual)
entry_ids={n['symbol_id'] for n in queue['required_native_parents']};extras=[]
for key in sorted(original_required):
    h=hgit[key];candidates=[oldcm[k] for k in h['current_ids']]
    assert len(candidates)==1,'Unresolved full original dependency '+h['canonical_original_id']
    n=candidates[0]
    if n['symbol_id'] not in entry_ids:
        extras.append(dict(historical_id=h['canonical_original_id'],name=h['name'],current_entry_record=n,current_record=mapping[n['symbol_id']],
            disposition='RETAINED' if h['proof'] else 'OPEN_REQUIRED_ORIGINAL_DEPENDENCY',
            reason='Reached in the actual archived original graph; current named Windows declaration is still measured even when the main current route uses its portable counterpart.'))
current_roots=[mapping[n['symbol_id']] for n in roots]+[r['current_record'] for r in extras]
combined={}
for config in graphs:
    meta=json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json')).read_bytes())
    combined[config]=closure(meta,[bind(n,meta,current_shapes)['method'] for n in current_roots])
save('entry-original-dependency-addendum-v2.json',dict(status='PASSIVE_ENTRY_AUTHORITY_ORIGINAL_GRAPH_COMPLETION_OPEN_ONLY',
    initial_queue=pin(HERE/'entry-queue-receipt-v1.json'),original_graphs=graphs,original_authority=original_methods,
    additional_original_required_native=extras,current_roots=[n['symbol_id'] for n in current_roots],current_graphs=combined,
    chronology='Derived after initial source extraction from the exact immutable archived metadata already authenticated at entry. No new reference, original DLL load, original compiler/model execution, input/query/capture, or retroactive pre-edit claim.',original_queries=0))
print(json.dumps(dict(additional=[dict(name=r['name'],disposition=r['disposition']) for r in extras],current_reached={c:len(g['methods']) for c,g in combined.items()})))
