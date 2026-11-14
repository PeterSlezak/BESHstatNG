from common import *
from collections import Counter
d=load(HERE/'abi-cil/cross-configuration-v1.json')
def norm(rows):
    return [('valuetuple-constructor' if operand.startswith('METHOD:System.ValueTuple`') and '..ctor(' in operand else op,operand) for op,operand in rows]
rows=[]
for r in d['ordered_call_delegate_dependency_differences']:
    if norm(r['Debug'])==norm(r['Release']) or 'VB$StateMachine_' in r['method'] or any(n in r['method'] for n in ['DiagnosticEmitter @','DiagnosticRunContext @']):continue
    a=Counter(map(tuple,r['Debug']));b=Counter(map(tuple,r['Release']))
    row=dict(method=r['method'],removed=list((a-b).elements()),added=list((b-a).elements()),same_target_sequence=[x[1] for x in r['Debug']]==[x[1] for x in r['Release']])
    rows.append(row)
save('abi-cil/optimization-residual-v1.json',dict(rows=rows))
for r in rows:print(json.dumps(r),flush=True)
