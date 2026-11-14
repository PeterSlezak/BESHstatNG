"""Exact category-list callback identity, caller, fields and constructors; no alias."""
from common_v2 import *
from proof_kernel_v1 import *
rows=[]
for callback in json.loads((HERE/'callback-proofs-v1.json').read_bytes())['rows']:
    if not callback['new_support'] or callback['original_id'] is None:continue
    need(callback['whole_body'].replace('\r\n','\n')==callback['original_whole_body'].replace('\r\n','\n'),'Changed original callback source')
    parent=next(c for c in json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'] if c['current_id']==callback['parent_id']);configs=[]
    for b in callback['configurations']:
        config=b['configuration'];old=json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+config+'.json')).read_bytes());defs=definitions(old);p=next(c['original_method'] for c in parent['compiled'] if c['configuration']==config);targets=[i['operand'][7:] for i in p['instructions'] if i['op']=='ldftn'];need(len(targets)==1,'Original category callback cardinality')
        target=defs[targets[0]];need(semantic(target)==semantic(b['complete_method']),'Original category method CIL drift')
        context=next(t for t in old['types'] if t['type']==target['declaring_type']);need({k:v for k,v in context.items() if k!='fields'}=={k:v for k,v in b['declaring_type'].items() if k!='fields'},'Category declaring type flags drift')
        names={'$I','$I43-0'}
        need([f for f in context['fields'] if f['name'] in names]==[f for f in b['declaring_type']['fields'] if f['name'] in names],'Actual category singleton/delegate field drift')
        prior=json.loads((PREVIOUS/('metadata-all/packet-v1-context-v3/current-'+config+'.json')).read_bytes());prior_type=next(t for t in prior['types'] if t['type']==target['declaring_type']);need(prior_type==b['declaring_type'],'C8 current closure context changed in C9')
        ctors=[m for m in old['methods'] if m['declaring_type']==target['declaring_type'] and m['member_name'] in ['.ctor','.cctor']];need([semantic(m) for m in ctors]==[semantic(m) for m in b['constructors']],'Category ctor/cctor drift')
        now=json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json')).read_bytes());current_parent=definitions(now)[b['parent_method']];need(semantic(p)==semantic(current_parent),'Category caller/cache instruction drift')
        field_names={i['operand'].split('.$')[-1].split(':',1)[0] for m in [p,target,*ctors] for i in m['instructions'] if isinstance(i['operand'],str) and i['operand'].startswith('FIELD:')};need(field_names <= {'I','I43-0'},'Category path reaches unrelated mutable cache')
        configs.append(dict(reached_fields=['$I','$I43-0'],other_fields='Full original/current records retained; older32/33 versus81/90 cache transitions are outside this actual category path and unchanged since C8. UdfDataImport container remains OPEN. No generic field normalization or alias.',configuration=config,original_parent=p,current_parent=current_parent,original_target=target,current_target=b['complete_method'],original_type=context,current_type=b['declaring_type'],original_constructors=ctors,current_constructors=b['constructors'],aliases=[]))
    rows.append(dict(original_id=callback['original_id'],current_id=callback['current_id'],source=callback,configurations=configs))
need(len(rows)==1,'Expected one new historical callback; current-only callbacks remain separate')
save('historical-callback-contracts-v1.json',dict(rows=rows,new_compiler_aliases=[],original_queries=0));print('Category callback/caller/cache/ctor identity exact in both configurations')
