from common import *
source=(HERE/'record_context.py').read_text(encoding='utf-8')
old="def key(r):return (r['declaring_type'],r['member_name'],tuple(r['parameter_types']))"
assert source.count(old)==1
source=source.replace(old,"def key(r):return r['method']")
with (HERE/'record_context_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('attempts/context-key-v1.json',dict(status='READER_KEY_COLLISION_REJECTED_BEFORE_CLOSURE',reader=pin(HERE/'record_context.py'),
    failure='Generic parameter FullName is null for different CLR shapes, so (declaring_type,name,parameter_types) collided for Matrix.array2str overloads. The strict comparison rejected an apparent difference.',
    correction='Use complete canonical metadata method identity including generic arity, parameter ranks and TypeName scope. Retain every overload and require exact whole-body context.',
    superseded_inspection=pin(HERE/'inspect_context_deltas.py'),production_changes=0,original_queries=0))
exec(compile(source,str(HERE/'record_context_v2.py'),'exec'))
