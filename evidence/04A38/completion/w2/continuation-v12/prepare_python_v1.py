"""Exact inherited scanner/assertion semantics, isolated C12 workers and fresh small controls."""
from common import *
rows=[]
for name in ['parallel_scan_v1.py','python_workers_v1.py','freeze_python_inputs_v1.py','run_python_v1.py','prove_scanner_deferral_v1.py']:
    source=PREVIOUS/name;value=source.read_text(encoding='utf-8')
    if name=='python_workers_v1.py':value=value.replace('w2c11','w2c12').replace('C11 worker','C12 worker')
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
    rows.append(dict(before=pin(source),after=pin(HERE/name),scope='Only worker labels/output roots differ; original tests, scanner kernels, assertions and candidate traversal stay unchanged.'))
source=PREVIOUS/'prove_parallel_scan_base_v4.py';value=source.read_text(encoding='utf-8')
value=value.replace("PRIVATE/'parallel-scan-proof-v3'","PRIVATE/'parallel-scan-proof-v1'").replace('prove_parallel_scan_base_v4.py','prove_parallel_scan_base_v1.py')
old="old=actual[0];(ROOT/old['path']).write_text('changed private proof fixture',encoding='utf-8')"
new="""old=actual[0]
before=pin(ROOT/old['path']);preimage=work/'preimages/0-before.txt';preimage.parent.mkdir(exist_ok=False)
with preimage.open('xb') as stream:stream.write((ROOT/old['path']).read_bytes())
assert pin(preimage)['sha256']==before['sha256']
(ROOT/old['path']).write_text('changed private proof fixture',encoding='utf-8')"""
assert old in value;value=value.replace(old,new)
marker="assert function_ast(POLICY,'scan')==function_ast(ROOT/'scripts/verify-04a14-publication.py','scan')"
phase="""observed_before=next(r['plan'] for r in receipts if any(v['index']==0 for v in json.loads((ROOT/r['plan']['path']).read_bytes())['rows']))
save('private-proof-transition-v1.json',dict(status='CURRENT_SYNTHETIC_CONTROL_PREIMAGE_CAPTURED_BEFORE_DELIBERATE_MUTATION',
    rows=[dict(before=before,preimage=pin(preimage),after=pin(ROOT/old['path']),generator=pin(HERE/'prove_parallel_scan_base_v1.py'),observed_before=observed_before,observed_after=new_receipts[0]['plan'])],
    scope='New C12 stale-cache synthetic control only. Actual before bytes captured before mutation, never an original statistical reference or pre-edit production capture.',original_queries=0))
"""
assert marker in value;value=value.replace(marker,phase+marker)
with (HERE/'prove_parallel_scan_base_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
rows.append(dict(before=pin(source),after=pin(HERE/'prove_parallel_scan_base_v1.py'),scope='Same adverse/equivalence assertions in a fresh C12 directory. Add actual synthetic preimage capture before the deliberate stale-cache mutation; no recovery or historical rewrite.'))
save('python-executor-crosswalk-v1.json',dict(files=rows,original_queries=0))
for label,args in [('scanner-base-proof-v1',[str(HERE/'prove_parallel_scan_base_v1.py')]),('scanner-deferral-proof-v1',[str(HERE/'prove_scanner_deferral_v1.py')]),('python-inventory-v1',[str(HERE/'python_workers_v1.py'),'inventory'])]:
    result=command(label,[sys.executable,'-B',*args],env={'PYTHONHASHSEED':'38'})
    save(label+'-execution.json',clean(result));assert result['exit_code']==0
print('C12 exact discovery, original scanner controls, cache preimages and deferral ready')
