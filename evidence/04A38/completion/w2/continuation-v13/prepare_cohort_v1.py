from common import *
from proof_kernel_v5 import bind,definitions,closure,map_current,semantic
from source_views import body
prior=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());universe=json.loads((HERE/'current-universe-packet-v1.json').read_bytes())
context=json.loads((HERE/'callback-identity-context-v1.json').read_bytes())
mapping=map_current(prior['members'],universe['members'],context['mapping']);reverse={n['symbol_id']:k for k,n in mapping.items()}
assert len(mapping)==len(universe['members'])==726
assert all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in prior['members'] if not n['review_status'].startswith('OPEN'))
entry=json.loads((HERE/'entry-queue-receipt-v1.json').read_bytes());shapes={r['symbol_id']:r for r in json.loads((HERE/'declaration-shapes-v1.json').read_bytes())['rows']}
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes());om={n['symbol_id']:n for n in original['members']}
graphs={};roots={};originalgraphs={};originalbindings=[]
original_parent=next(n for n in om.values() if n['name']=='BuildKaplanMeierStepTable')
original_children=sorted([n for n in om.values() if n['parent_member']==original_parent['symbol_id']],key=lambda n:n['span_start'])
assert len(original_children)==2
for config in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json')).read_bytes());defs=definitions(meta)
    originals=json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+config+'.json')).read_bytes());od=definitions(originals)
    root_nodes=[mapping[n['symbol_id']] for n in entry['required_native_parents']]
    roots[config]=[bind(n,meta,shapes)['method'] for n in root_nodes]
    graphs[config]=closure(meta,roots[config]);originalgraphs[config]=json.loads((HERE/'entry-compiled-dependency-catalog-v1.json').read_bytes())['original_graphs'][config]
    oldshape=dict(shapes);cp=next(n for n in root_nodes if n['name']=='BuildKaplanMeierStepTable');oldshape[original_parent['symbol_id']]=shapes[cp['symbol_id']]
    op=bind(original_parent,originals,oldshape)
    targets=[i['operand'][7:] for i in op['instructions'] if i['op']=='ldftn' and i['operand'][7:] in od and od[i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
    assert len(targets)==2
    for index,node in enumerate(original_children):
        current=next(r for r in context['rows'] if r['configuration']==config and r['source_sibling_index']==index)
        actual=od[targets[index]];measured=current['complete_callback']
        assert semantic(actual)==semantic(measured),'Actual archived original KM callback signature/body/exception CIL changed'
        assert node['span_start']-original_parent['span_start']==current['original_relative_utf16']
        originalbindings.append(dict(configuration=config,original_record=node,original_parent=op['method'],actual_original_ldftn=targets[index],original_complete_method=actual,
            current_id=current['current_id'],actual_current_ldftn=current['actual_ldftn'],current_complete_method=measured,role=current['role'],source_context=pin(HERE/'callback-identity-context-v1.json')))
parents=[mapping[n['symbol_id']] for n in entry['required_native_parents']]
ids={n['symbol_id'] for n in parents};pending=list(ids)
while pending:
    p=pending.pop()
    for n in universe['members']:
        if n['parent_member']==p and n['symbol_id'] not in ids:ids.add(n['symbol_id']);pending.append(n['symbol_id'])
oldm={n['symbol_id']:n for n in prior['members']}
save('original-km-callback-bindings-v1.json',dict(status='BOTH_CANONICAL_ORIGINAL_KM_CALLBACKS_EXACT_COMPILED_CURRENT_MATCH',rows=originalbindings,original_queries=0))
save('cohort-diagnostic-v1.json',dict(roots=roots['Debug'],reviewed_candidate_ids=[n['symbol_id'] for n in parents],all_candidate_ids=sorted(ids),graphs=graphs,original_graphs=originalgraphs,
    new_open_candidates=[n for n in universe['members'] if n['symbol_id'] in ids and oldm[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')],
    mapping=[dict(before=k,after=n['symbol_id'],body_equal=oldm[k]['body_sha256']==n['body_sha256']) for k,n in mapping.items()],
    callback_context=pin(HERE/'callback-identity-context-v1.json'),original_only_callback=pin(HERE/'portable-historical-callback-proof-v1.json'),original_queries=0))
save('current-semantic-position-crosswalk-v1.json',dict(rows=[dict(before=k,after=n['symbol_id'],body_bytes_equal=oldm[k]['body_sha256']==n['body_sha256']) for k,n in mapping.items()],
    repeated_callback_context=pin(HERE/'callback-identity-context-v1.json'),qualification='Every726 current identity exactly once; source shifts are not lost methods. Only two ambiguous KM identities use explicit complete source/compiled count/fill parameter proof.'))
print(json.dumps(dict(parents=len(parents),callbacks=len(ids)-len(parents),new_current=len([n for n in universe['members'] if n['symbol_id'] in ids and oldm[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')]),reached={c:len(g['methods']) for c,g in graphs.items()})))
