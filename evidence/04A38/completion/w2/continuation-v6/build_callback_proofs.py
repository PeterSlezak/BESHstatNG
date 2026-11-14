from common import *
from source_views import body
u=json.loads((HERE/'current-universe-families-v2.json').read_bytes())
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
byid={r['symbol_id']:r for r in u['members']};parents={r['current_id']:r for r in contracts}
groups={}
for r in u['members']:
 if r['parent_member'] in parents:groups.setdefault(r['parent_member'],[]).append(r)
rows=[]
for parent,children in groups.items():
 c=parents[parent];children=sorted(children,key=lambda r:r['span_start'])
 configs=[]
 for config in ['Debug','Release']:
  meta=json.loads((HERE/('metadata-full/families-v2/current-'+config+'.json')).read_bytes());methods={r['method']:r for r in meta['methods']}
  pc=next(r for r in c['compiled'] if r['configuration']==config)
  assert len(pc['current_methods'])==1,c['name']
  method=methods[pc['current_methods'][0]]
  targets=[i['operand'][7:] for i in method['instructions'] if i['op']=='ldftn']
  assert len(targets)==len(children),(c['name'],len(targets),len(children))
  configs.append(dict(configuration=config,parent_method=method['method'],bindings=[dict(current_id=child['symbol_id'],target=target,complete_method=methods[target],
      declaring_type=next(r for r in meta['types'] if r['type']==methods[target]['declaring_type'])) for child,target in zip(children,targets)]))
 for child in children:
  text,policy=body((ROOT/child['path']).read_bytes(),child)
  rows.append(dict(current_id=child['symbol_id'],parent_id=parent,current_record=child,whole_body=text,source_view=policy,
      source=pin(ROOT/child['path']),configurations=[dict(configuration=r['configuration'],parent_method=r['parent_method'],**next(b for b in r['bindings'] if b['current_id']==child['symbol_id'])) for r in configs],
      review='Entire indexed/header/acquisition callback is included in the reviewed parent source and individually matched to its actual ldftn target in source order. The complete closure method, captured fields, constructor context and native conversion/catch boundary are retained; no status-only callback classification.',original_queries=0))
save('callback-proofs-v1.json',dict(rows=rows,original_queries=0))
print(json.dumps(dict(callbacks=len(rows),parents=[parents[p]['name'] for p in groups])))
