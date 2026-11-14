"""Read exact original/current callback/cache/ctor identity, without generic aliases."""
from common import *
rows=[]
for config in ['Debug','Release']:
    sides=[]
    for side in ['original','current']:
        data=json.loads((HERE/('metadata-all/packet-v1-context-v3/'+side+'-'+config+'.json')).read_bytes());defs={r['method']:r for r in data['methods']}
        parent=next(r for r in data['methods'] if (r['declaring_type'],r['member_name'])==('BESHStatNG.UdfDataImport','Get2D'))
        targets=[i['operand'][7:] for i in parent['instructions'] if i['op']=='ldftn']
        assert len(targets)==1
        target=defs[targets[0]];context=next(t for t in data['types'] if t['type']==target['declaring_type'])
        fields=[i for i in parent['instructions'] if isinstance(i['operand'],str) and i['operand'].startswith('FIELD:')]
        constructors=[r for r in data['methods'] if r['declaring_type']==target['declaring_type'] and r['member_name'] in ['.ctor','.cctor']]
        sides.append(dict(side=side,parent=parent,target=target,context=context,fields=fields,constructors=constructors))
    rows.append(dict(configuration=config,sides=sides))
save('get2d-original-current-context-inspection-v1.json',dict(rows=rows,original_queries=0))
print(json.dumps([dict(configuration=r['configuration'],sides=[dict(side=s['side'],target=s['target']['method'],fields=s['fields'],constructor_methods=[c['method'] for c in s['constructors']],context_fields=s['context']['fields']) for s in r['sides']]) for r in rows]))
