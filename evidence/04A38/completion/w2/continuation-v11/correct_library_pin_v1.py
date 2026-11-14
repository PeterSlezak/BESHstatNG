from common import *
before=pin(HERE/'common.py');data=(HERE/'common.py').read_bytes()
target=HERE/'source-attempts/common-before-library-pin-fix-v1.py.txt';target.parent.mkdir(exist_ok=True)
with target.open('xb') as stream:stream.write(data)
source=data.decode('utf-8')
old="    result = pin(path)\n    return dict(name=Path(path).name, bytes=result['bytes'], sha256=result['sha256'])"
new="    path = Path(path)\n    with path.open('rb') as stream:\n        digest = hashlib.file_digest(stream, 'sha256').hexdigest()\n    return dict(name=path.name, bytes=path.stat().st_size, sha256=digest)"
assert old in source
(HERE/'common.py').write_text(source.replace(old,new),encoding='utf-8',newline='\n')
proof=(HERE/'prove_parallel_scan_base_v2.py').read_text(encoding='utf-8')
proof=proof.replace("PRIVATE/'parallel-scan-proof-v1'", "PRIVATE/'parallel-scan-proof-v2'")
proof=proof.replace("HERE/'prove_parallel_scan_base_v2.py'", "HERE/'prove_parallel_scan_base_v3.py'")
with (HERE/'prove_parallel_scan_base_v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(proof)
save('scanner-bootstrap-first-failure-v1.json',dict(before=before,preimage=pin(target),after=pin(HERE/'common.py'),
    failure=pin(HERE/'scanner-base-proof-v1-execution.json'),stderr=pin(RUNROOT/'commands/w2c11-scanner-base-proof-v1/stderr.log'),
    cause='C11 library_pin incorrectly reused repository-relative pin for the external owner-specified Python executable.',
    correction='Restore the v10 path-independent name/length/SHA library pin contract. No scanner/test/assertion changes. Preserve partial synthetic fixtures, rerun the proof in a new distinct directory.',original_queries=0))
for label,args in [('scanner-base-proof-v2',[str(HERE/'prove_parallel_scan_base_v3.py')]),
                   ('scanner-deferral-proof-v1',[str(HERE/'prove_scanner_deferral_v1.py')]),
                   ('python-inventory-v1',[str(HERE/'python_workers_v1.py'),'inventory'])]:
    result=command(label,[sys.executable,'-B',*args],env={'PYTHONHASHSEED':'38'})
    save(label+'-execution.json',clean(result));assert result['exit_code']==0
