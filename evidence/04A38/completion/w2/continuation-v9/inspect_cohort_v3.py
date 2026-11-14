"""Derive the real source/dependency cohort before claiming closures."""
from common_v2 import *
from collections import defaultdict,Counter
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());prior=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());history=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members'];queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())['first_members']
meta=json.loads((HERE/'metadata-all/packet-v1-context-v4/current-Debug.json').read_bytes());defs={m['method']:m for m in meta['methods']}
bykey=defaultdict(list);byowner=defaultdict(list)
for n in u['members']:bykey[n['signature_key']].append(n);byowner[n['owner'],n['name']].append(n)
mapping={};exact_ids={n['symbol_id']:n for n in u['members']}
for n in prior['members']:
    if n['symbol_id'] in exact_ids:
        v=exact_ids[n['symbol_id']];assert v['signature_key']==n['signature_key']
        if n['parent_member'] is not None:assert v['body_sha256']==n['body_sha256']
        mapping[n['symbol_id']]=v;continue
    candidates=[v for v in bykey[n['signature_key']] if (v['parent_member'] or '').split('@')[0]==(n['parent_member'] or '').split('@')[0] and (n['parent_member'] is None or v['body_sha256']==n['body_sha256'])];assert len(candidates)==1,n['symbol_id'];mapping[n['symbol_id']]=candidates[0]
roots=[mapping[next(h for h in history if h['canonical_original_id']==r['original_id'])['current_ids'][0]] for r in queue];pending=[m['method'] for m in meta['methods'] if any((m['declaring_type'],m['member_name'])==(n['owner'],n['name']) for n in roots)];seen=set();outside=set()
while pending:
    k=pending.pop()
    if k in seen:continue
    if k not in defs:outside.add(k);continue
    seen.add(k)
    for ins in defs[k]['instructions']:
        if ins['op'] in ['call','callvirt','newobj','ldftn','ldvirtftn'] and isinstance(ins['operand'],str) and ins['operand'].startswith('METHOD:'):pending.append(ins['operand'][7:])
unresolved=[k for k in outside if k.split(' @ ',1)[1].split(',',1)[0] in ['BESHStatNG','BESHStat.Core','BESHStat.Excel.Common']];assert not unresolved,unresolved[:3]
source_ids={n['symbol_id'] for k in seen for n in byowner.get((defs[k]['declaring_type'],defs[k]['member_name']),[]) if n['parent_member'] is None}
changed=[]
for config in ['Debug','Release']:
    now=json.loads((HERE/('metadata-all/packet-v1-context-v4/current-'+config+'.json')).read_bytes());before=json.loads((PREVIOUS/('metadata-all/packet-v1-context-v3/current-'+config+'.json')).read_bytes());nm={r['method']:r for r in now['methods']};old={r['method']:r for r in before['methods']}
    semantic=lambda r:{k:v for k,v in r.items() if k!='raw_IL_sha256'}
    differences=[dict(method=k,owner=r['declaring_type'],name=r['member_name']) for k,r in old.items() if k in nm and semantic(r)!=semantic(nm[k])]
    changed.append(dict(configuration=config,existing_changes=differences,lost=sorted(set(old)-set(nm)),new=sorted(set(nm)-set(old))))
old_by_id={n['symbol_id']:n for n in prior['members']};reverse={v['symbol_id']:old_by_id[k] for k,v in mapping.items()}
needed=[n for n in u['members'] if n['symbol_id'] in source_ids];new=[n for n in needed if reverse[n['symbol_id']]['review_status'].startswith('OPEN')]
save('cohort-diagnostic-v1.json',dict(roots=[n['symbol_id'] for n in roots],reviewed_candidate_ids=[n['symbol_id'] for n in needed],new_open_candidates=new,graph=dict(methods=sorted(seen),external=sorted(outside)),compiled_changes=changed,mapping=[dict(before=k,after=v['symbol_id'],body_equal=old_by_id[k]['body_sha256']==v['body_sha256']) for k,v in mapping.items()],original_queries=0))
print(json.dumps(dict(reached_methods=len(seen),required_source_parents=len(needed),new_open_candidates=[n['name'] for n in new],changes=[dict(configuration=c['configuration'],changed=[(r['owner'],r['name']) for r in c['existing_changes']],lost=len(c['lost']),new=len(c['new'])) for c in changed])))
