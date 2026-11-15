"""Refine literal handling without changing already computed canonical records."""
from common40_v3 import *
import ast,copy,re,collections
lock();source=(HERE/'compiled-index-v2.py').read_text(encoding='utf-8')
tree=ast.parse(source);defs=[n for n in tree.body if isinstance(n,ast.FunctionDef)]
ns=dict(globals());exec(compile(ast.Module(body=defs,type_ignores=[]),'retained canonical definitions','exec'),ns)
oldcanonical=ns['canonical']
def literal_safe(value):
    if isinstance(value,dict) and value.get('op')=='ldstr':return copy.deepcopy(value)
    if isinstance(value,dict):
        out={k:literal_safe(v) for k,v in value.items() if k!='raw_IL_sha256'}
        if isinstance(value.get('type'),str) and 'fields' in value and '+_Closure$__' in value['type'] and all(f['is_static'] for f in value['fields']):out['fields']=sorted(out['fields'],key=lambda x:json.dumps(x,sort_keys=True))
        return out
    if isinstance(value,list):return [literal_safe(v) for v in value]
    if isinstance(value,str):
        for p,r in [(r'_Closure\$__(\d+)-(\d+)',r'_Closure$__ORD-\2'),(r'_Lambda\$__(\d+)-(\d+)',r'_Lambda$__ORD-\2'),(r'\$I(\d+)-(\d+)',r'$IORD-\2')]:value=re.sub(p,r,value)
    return value
checked=0
for config in ['Debug','Release']:
    for p in [ROOT/('evidence/04A39/abi-cil/'+config+'/current-v1.json'),PRIVATE/('abi-cil-final2/'+config+'/current.json')]:
        for r in load(p)['methods']:
            assert literal_safe(r)==oldcanonical(r),'LITERAL_HANDLING_CHANGES_ACTUAL_RECORD'
            checked+=1
sample=dict(op='ldstr',operand='_Lambda$__77-2');mutated=dict(op='ldstr',operand='_Lambda$__88-2')
assert literal_safe(sample)!=literal_safe(mutated)
record=dict(method='Example',instructions=[dict(op='ldc.i4.1',operand=None)])
changed=copy.deepcopy(record);changed['instructions'][0]['op']='ldc.i4.2';assert literal_safe(record)!=literal_safe(changed)
bad=['netstandard, Version=2.0.0.0','ExcelDna.Integration, Version=1.0.0.0'];assert any('ExcelDna' in s for s in bad)
save('abi-cil/adverse-controls-v1.json',dict(actual_complete_records_unchanged_by_literal_refinement=checked,
    controls=dict(changed_ldstr_generated_name_detected=True,changed_opcode_detected=True,injected_host_assembly_reference_detected=True,static_field_names_types_visibility_retained=True),
    inherited_static_generic_field_order='Only static compiler cache field order normalized; no instance/public layout order normalized.',reference_sources=[pin(PRIVATE/('abi-cil-final2/'+c+'/assembly-references.json')) for c in ['Debug','Release']]))
print('Compiled full-record/ldstr/opcode/host-reference controls PASS',checked,'actual records',flush=True)
