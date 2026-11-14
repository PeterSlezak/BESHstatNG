"""Inspect complete queue/dependency/CIL frontier before deciding any body closure."""
from common import *
from collections import defaultdict
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes())
prior=json.loads((PREVIOUS/'current-ownership-v1.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v1.json').read_bytes())['members']
queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())['first_members']
bykey=defaultdict(list)
for n in u['members']:bykey[n['signature_key']].append(n)
rows=[];roots=[]
for q in queue:
    h=next(h for h in history if h['canonical_original_id']==q['original_id'])
    candidates=[n for key in {s.split('@')[0] for s in h['current_ids']} for n in bykey[key]]
    assert len(candidates)==1,(q['name'],len(candidates))
    n=candidates[0];parent=next((p for p in u['members'] if p['symbol_id']==n['parent_member']),None)
    rows.append(dict(original_id=q['original_id'],current=n,parent=parent,previous=[r for r in prior['members'] if r['signature_key']==n['signature_key']]))
    roots.append(parent or n)
roots.extend(n for n in u['members'] if n['name'] in ['MaterializeSampleSizeResult38','ResolveSampleSizeHypothesisKind38'])
definitions={c:json.loads((HERE/('metadata-all/packet-v1-context-v3/current-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
def closure(data,keys):
    defs={r['method']:r for r in data['methods']};pending=list(keys);seen=set();external=set();edges=[]
    while pending:
        key=pending.pop()
        if key in seen:continue
        if key not in defs:external.add(key);continue
        seen.add(key)
        for i,op in enumerate(defs[key]['instructions']):
            if op['op'] in ['call','callvirt','newobj','ldftn','ldvirtftn'] and isinstance(op['operand'],str) and op['operand'].startswith('METHOD:'):
                target=op['operand'][7:];pending.append(target);edges.append(dict(caller=key,instruction=i,op=op['op'],target=target))
    return dict(methods=sorted(seen),external_runtime_or_ExcelDna=sorted(external),edges=edges)
graphs={};source_set=set()
for config,data in definitions.items():
    starts=[r['method'] for r in data['methods'] if any((r['declaring_type'],r['member_name'])==(n['owner'],n['name']) for n in roots)]
    g=closure(data,starts);graphs[config]=g
    defs={r['method']:r for r in data['methods']}
    for key in g['methods']:
        r=defs[key]
        source_set.update(n['symbol_id'] for n in u['members'] if (n['owner'],n['name'])==(r['declaring_type'],r['member_name']))
source_set.update(n['symbol_id'] for n in u['members'] if n['parent_member'] in source_set)
oldbykey=defaultdict(list)
for n in prior['members']:oldbykey[n['signature_key']].append(n)
new=[n for n in u['members'] if n['symbol_id'] in source_set and not any(not r['review_status'].startswith('OPEN') and r['body_token_sha256']==n['body_token_sha256'] for r in oldbykey[n['signature_key']])]
changes=[]
for config,data in definitions.items():
    before=json.loads((PREVIOUS/('metadata-all/packet-v1-context-v3/current-'+config+'.json')).read_bytes());now={r['method']:r for r in data['methods']}
    semantic=lambda r:{k:v for k,v in r.items() if k!='raw_IL_sha256'}
    changed=[dict(owner=r['declaring_type'],name=r['member_name'],method=r['method']) for r in before['methods'] if r['method'] not in now or semantic(r)!=semantic(now[r['method']])]
    changes.append(dict(configuration=config,changed=changed))
save('packet-frontier-inspection-v1.json',dict(queue=rows,root_source_ids=sorted({n['symbol_id'] for n in roots}),graphs=graphs,reviewed_candidates=sorted(source_set),not_previously_supported=new,compiled_changes=changes,census=dict(members=len(u['members']),supports=len(u['supports']),fragments=len(u['fragments'])),original_queries=0))
print(json.dumps(dict(census=dict(members=len(u['members']),supports=len(u['supports']),fragments=len(u['fragments'])),new_candidate_names=[n['name'] for n in new],lambda=[dict(current=r['current']['symbol_id'],parent=r['parent']['name'] if r['parent'] else None,old=r['previous'][0]['review_status']) for r in rows if r['current']['name']=='lambda'],compiled_changes=[dict(configuration=r['configuration'],changes=[(n['owner'],n['name']) for n in r['changed']]) for r in changes])))
