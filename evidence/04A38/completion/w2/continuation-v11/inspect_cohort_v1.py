"""Derive required complete parents from actual compiled reachability, including callbacks."""
from common import *
from proof_kernel_v3 import map_current, closure, semantic, need

u=json.loads((HERE/'current-universe-packet-v2.json').read_bytes())
old=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members']
entries=json.loads((HERE/'target-entry-bodies-v1.json').read_bytes())['rows']
mapping=map_current(old['members'],u['members']);members={n['symbol_id']:n for n in u['members']}
reverse={n['symbol_id']:k for k,n in mapping.items()};oldm={n['symbol_id']:n for n in old['members']}
roots=[mapping[r['current_record']['symbol_id']] for r in entries if r['current_record']['parent_member'] is None]
supplemental=[]
for name in ['FA_DROP','KMEANS_DROP','HCLUST_DROP','CA_DROP']:
    n=next(n for n in u['members'] if n['name']==name and n['owner']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs')
    roots.append(n);supplemental.append(dict(current_id=n['symbol_id'],reason='Complete native cache lifecycle for this exact cohort.'))
graphs={};changes=[]
for config in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v2-context-v5/current-'+config+'.json')).read_bytes())
    methods=[next(m['method'] for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(n['owner'],n['name'])) for n in roots]
    graph=closure(meta,methods);graphs[config]=graph
    before=json.loads((PREVIOUS/('metadata-all/packet-v2-context-v5/current-'+config+'.json')).read_bytes())
    a={r['method']:r for r in before['methods']};b={r['method']:r for r in meta['methods']}
    changes.append(dict(configuration=config,changed=[dict(method=k,owner=r['declaring_type'],name=r['member_name'])
        for k,r in a.items() if k in b and semantic(r)!=semantic(b[k])],lost=sorted(set(a)-set(b)),new=sorted(set(b)-set(a))))
    if config=='Debug':
        owners={(b[k]['declaring_type'],b[k]['member_name']) for k in graph['methods']}
        needed=[n for n in u['members'] if n['parent_member'] is None and (n['owner'],n['name']) in owners]
pending=list(needed);ids={n['symbol_id'] for n in needed}
while pending:
    parent=pending.pop()
    for n in u['members']:
        if n['parent_member']==parent['symbol_id'] and n['symbol_id'] not in ids:
            ids.add(n['symbol_id']);pending.append(n)
new=[n for n in u['members'] if n['symbol_id'] in ids and oldm[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')]
save('cohort-diagnostic-v1.json',dict(roots=[n['symbol_id'] for n in roots],supplemental=supplemental,
    reviewed_candidate_ids=[n['symbol_id'] for n in needed],all_candidate_ids=sorted(ids),
    new_open_candidates=new,graphs=graphs,compiled_changes=changes,
    mapping=[dict(before=k,after=n['symbol_id'],body_equal=oldm[k]['body_sha256']==n['body_sha256']) for k,n in mapping.items()],original_queries=0))
print(json.dumps(dict(parents=len(needed),callbacks=len(ids)-len(needed),new_open=[n['name'] for n in new],
    reached={c:len(g['methods']) for c,g in graphs.items()},changes=[dict(configuration=r['configuration'],changed=[(m['owner'],m['name']) for m in r['changed']],lost=len(r['lost']),new=len(r['new'])) for r in changes])))
