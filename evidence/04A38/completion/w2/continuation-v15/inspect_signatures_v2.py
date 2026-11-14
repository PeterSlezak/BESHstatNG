"""Exact current signature/CIL inventory, with explicit reflection field-order evidence."""
from common import *
from collections import Counter
import copy


def types_equal(before, after):
    left={r['type']:r for r in before};right={r['type']:r for r in after}
    assert len(left)==len(before) and len(right)==len(after) and set(left)==set(right)
    deltas=[]
    for key,a in left.items():
        b=right[key]
        assert {k:v for k,v in a.items() if k!='fields'}=={k:v for k,v in b.items() if k!='fields'}
        canonical=lambda fields:Counter(json.dumps(f,sort_keys=True) for f in fields)
        assert len({f['name'] for f in a['fields']})==len(a['fields'])
        assert len({f['name'] for f in b['fields']})==len(b['fields'])
        assert canonical(a['fields'])==canonical(b['fields'])
        if a['fields']!=b['fields']:
            deltas.append(dict(type=key,before_fields=a['fields'],after_fields=b['fields'],
                              interpretation='Same complete field identities/types/flags/literals; only passive reflection enumeration order differs. No source or CIL normalization.'))
    return deltas


folder=PRIVATE/'signatures-v1';exe=folder/'MethodMetadata38.exe'
assert exe.is_file()
commands=[];rows=[];orders=[]
for cfg in ['Debug','Release']:
    if cfg=='Debug':
        target=HERE/'metadata-signatures-v1/current-Debug.json'
        prior=RUNROOT/'commands/w2c15-signatures-current-Debug-v1/command.json'
        assert json.loads(prior.read_bytes())['exit_code']==0
        command_authority=pin(prior)
    else:
        target=HERE/'metadata-signatures-v2/current-Release.json';target.parent.mkdir(exist_ok=True)
        r=command('signatures-current-Release-v2',[str(exe),str(PRIVATE/'packet-v2'/cfg/'lib'),str(target),str(HERE/'native-metadata-owners-v3.txt')]);commands.append(clean(r));assert r['exit_code']==0
        command_authority=pin(RUNROOT/'commands/w2c15-signatures-current-Release-v2/command.json')
    before_path=HERE/('metadata-all/packet-v2-context-v5/current-'+cfg+'.json')
    before=json.loads(before_path.read_bytes());now=json.loads(target.read_bytes())
    stripped=[{k:v for k,v in method.items() if k!='signature'} for method in now['methods']]
    assert stripped==before['methods']
    deltas=types_equal(before['types'],now['types'])
    orders.append(dict(configuration=cfg,rows=deltas))
    assert all(method['signature']['return_type'] is not None and all(p['type'] is not None for p in method['signature']['parameters']) for method in now['methods'])
    controls=[]
    for name,mutate in [('missing field',lambda d:d[0]['fields'].pop()),
                        ('changed field type',lambda d:d[0]['fields'][0].update(type='synthetic_wrong_type')),
                        ('duplicate field',lambda d:d[0]['fields'].append(copy.deepcopy(d[0]['fields'][0])))]:
        selected=next(t for t in now['types'] if t['fields']);original=[copy.deepcopy(selected)];adverse=copy.deepcopy(original);mutate(adverse)
        try:types_equal(original,adverse)
        except AssertionError:controls.append(dict(name=name,status='REJECTED'))
        else:raise AssertionError('Field contract mutation accepted')
    rows.append(dict(configuration=cfg,current=pin(target),same_CIL_authority=pin(before_path),
                     actual_command=command_authority,methods=len(now['methods']),all_return_parameter_types_measured=True,
                     all_CIL_exception_edges_byte_record_equal=True,field_inventory_exact=True,controls=controls))
save('compiled-full-signatures-v2.json',dict(rows=rows,reflection_order_deltas=orders,
    source=pin(HERE/'MethodMetadata-v12.cs'),tool=pin(exe),verifier=pin(Path(__file__)),commands=commands,
    retained_first_attempt=pin(HERE/'inspect-signatures-v1-execution.json'),metadata_only=True,original_queries=0,
    scope='Fresh actual current constructed handles, complete return/parameter/default/attribute/generic contracts, exact method/CIL/exception records and complete field identities. The first strict reflection-order failure remains immutable. Historical readers/assertions are unchanged.'))
print('Current Debug/Release full signatures and exact CIL/field inventory verified; passive enumeration differences explicit')
