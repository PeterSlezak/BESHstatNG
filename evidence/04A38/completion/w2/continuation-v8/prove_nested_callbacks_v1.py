"""Bind every nested source callback to its actual immediate compiled parent."""
from common import *
from source_views import body
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
contexts={c:json.loads((HERE/('metadata-all/packet-v1-context-v3/current-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
defs={c:{r['method']:r for r in data['methods']} for c,data in contexts.items()}
rows=[];pending=[(r['current_id'],{c:next(b['roots'] for b in r['compiled'] if b['configuration']==c) for c in contexts}) for r in contracts]
while pending:
    parent,methods=pending.pop(0);children=sorted([n for n in u['members'] if n['parent_member']==parent],key=lambda n:n['span_start'])
    if not children:continue
    assert all(len(keys)==1 for keys in methods.values())
    targets={c:[i['operand'][7:] for i in defs[c][methods[c][0]]['instructions'] if i['op']=='ldftn' and i['operand'][7:] in defs[c] and defs[c][i['operand'][7:]]['member_name'].startswith('_Lambda$__')] for c in contexts}
    assert all(len(keys)==len(children) for keys in targets.values()),parent
    for index,n in enumerate(children):
        text,view=body((ROOT/n['path']).read_bytes(),n);bindings=[];next_methods={}
        for c in contexts:
            target=defs[c][targets[c][index]];context=next(t for t in contexts[c]['types'] if t['type']==target['declaring_type'])
            bindings.append(dict(configuration=c,parent_method=methods[c][0],target=target['method'],complete_method=target,declaring_type=context));next_methods[c]=[target['method']]
        rows.append(dict(current_id=n['symbol_id'],parent_id=parent,current_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=view,configurations=bindings,new_current=False))
        pending.append((n['symbol_id'],next_methods))
assert len(rows)==len({r['current_id'] for r in rows})==11
save('callback-proofs-v2.json',dict(rows=rows,new_current_callbacks=0,scope='All direct and nested source callbacks bind to actual immediate parent ldftn, complete methods/exceptions and field/closure context. The inner GetVariableNames range-reader lambda is explicit; no generic alias or inferred callback closure.'))
inputs=json.loads((HERE/'proof-inputs-v2.json').read_bytes());inputs['expected_callbacks']=11
save('proof-inputs-v3.json',inputs)
source=(HERE/'verify_c8_checkpoint_template_v1.py').read_text(encoding='utf-8')
source=source.replace('proof-inputs-v2.json','proof-inputs-v3.json').replace('callback-proofs-v1.json','callback-proofs-v2.json').replace("==10,'Callback multiplicity differs'","==11,'Callback multiplicity differs'")
before="for t in old['types']:need(types.get(t['type'])==t,'Changed retained C7 field/closure/type context')"
after="""for t in old['types']:
            if t['type'] is not None:need(types.get(t['type'])==t,'Changed retained C7 field/closure/type context')
        need(Counter(json.dumps(t,sort_keys=True) for t in old['types'] if t['type'] is None)==Counter(json.dumps(t,sort_keys=True) for t in now['types'] if t['type'] is None),'Changed retained unused open-constructed raw context')"""
assert before in source;source=source.replace(before,after)
source=source.replace("n['parent_member']==row['parent_id'] and row['parent_id'] in reviewed", "n['parent_member']==row['parent_id'] and row['parent_id'] in (reviewed|{r['current_id'] for r in callbacks})")
before="binding=next(r for r in row['configurations'] if r['configuration']==config);parent=next(c for c in contracts if c['current_id']==row['parent_id']);bound=next(r for r in parent['compiled'] if r['configuration']==config)"
after="""binding=next(r for r in row['configurations'] if r['configuration']==config)
            parent=next((c for c in contracts if c['current_id']==row['parent_id']),None)
            if parent is not None:bound=next(r for r in parent['compiled'] if r['configuration']==config)
            else:
                parent_callback=next(r for r in callbacks if r['current_id']==row['parent_id'])
                bound=dict(roots=[next(r['target'] for r in parent_callback['configurations'] if r['configuration']==config)])"""
assert before in source;source=source.replace(before,after)
source=source.replace("if n['parent_member'] in reviewed},'Missing source callback'", "if n['parent_member'] in (reviewed|{r['current_id'] for r in callbacks})},'Missing source callback'")
with (HERE/'verify_c8_checkpoint_template_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('nested-callback-reader-crosswalk-v1.json',dict(before=pin(HERE/'verify_c8_checkpoint_template_v1.py'),after=pin(HERE/'verify_c8_checkpoint_template_v2.py'),prior_callbacks=pin(HERE/'callback-proofs-v1.json'),complete_callbacks=pin(HERE/'callback-proofs-v2.json'),reason='Fail-closed first draft detected omitted nested GetVariableNames lambda. Recursively bind its actual immediate caller and complete field context. Retain unnamed open-constructed measurements by exact multiset, never by collapsing None dictionary keys; claimed methods must still have concrete owners.',original_queries=0))
print('All11 callbacks, including nested immediate-parent binding, explicit')
