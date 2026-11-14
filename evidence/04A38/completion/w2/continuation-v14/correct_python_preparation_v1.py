from common import *
source=HERE/'common.py';target=HERE/'source-provisional/common-before-library-pin-fix-v1.py'
with target.open('xb') as f:f.write(source.read_bytes())
value=source.read_text();old="def library_pin(path):\n r=pin(path);return dict(name=Path(path).name,bytes=r['bytes'],sha256=r['sha256'])"
new="def library_pin(path):\n path=Path(path)\n with path.open('rb') as s:h=hashlib.file_digest(s,'sha256').hexdigest()\n return dict(name=path.name,bytes=path.stat().st_size,sha256=h)"
assert old in value
with source.open('w',encoding='utf-8',newline='\n') as f:f.write(value.replace(old,new,1))
s=HERE/'prove_parallel_scan_base_v1.py';v=s.read_text().replace("PRIVATE/'parallel-scan-proof-v1'","PRIVATE/'parallel-scan-proof-v2'").replace("HERE/'prove_parallel_scan_base_v1.py'","HERE/'prove_parallel_scan_base_v2.py'")
with (HERE/'prove_parallel_scan_base_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('python-preparation-correction-v1.json',dict(before=pin(target),after=pin(source),failed=pin(HERE/'scanner-base-proof-v1-execution.json'),
 reason='Tooling library_pin must hash external Python/tool binaries without requiring them to be repository-relative payloads. Restore the exact predecessor implementation. No scanner or assertion changes; preserve the failed small fixture directory and use a distinct v2 directory for the corrective proof.',original_queries=0))
