from common import *
s=HERE/'verify_c13_checkpoint_template_v3.py';v=s.read_text();old="for k in graph['methods']}}=={c['current_id'] for c in cs}"
assert old in v;v=v.replace(old,"for k in graph['methods']} and bind(n,meta,shapes)['method'] in graph['methods']}=={c['current_id'] for c in cs}")
compile(v,'verify_c13_checkpoint_template_v4.py','exec')
with (HERE/'verify_c13_checkpoint_template_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('reader-overload-binding-correction-v1.json',dict(before=pin(s),after=pin(HERE/'verify_c13_checkpoint_template_v4.py'),
    reason='Owner/name alone falsely included the unreached PrepareResultTableForUdf(ResultTable) overload. Require exact full parameter/ByRef/rank source binding and actual method membership. The real reached overloads are already complete contracts; no dependency omission or new closure is permitted.',original_queries=0))
