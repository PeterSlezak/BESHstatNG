from common import *
result=json.loads((HERE/'static-reader-execution-v3.json').read_bytes());assert result['exit_code']==0
raw=RUNROOT/'commands/w2c10-static-reader-v3/stdout.log';doc=json.loads(raw.read_bytes())
save('readiness-static-v1.json',doc)
source=(HERE/'prove_parallel_scan_base_v1.py').read_text(encoding='utf-8').replace("proof=pin(HERE/'prove_parallel_scan_v1.py')","proof=pin(HERE/'prove_parallel_scan_base_v2.py')").replace("work=PRIVATE/'parallel-scan-proof-v1'","work=PRIVATE/'parallel-scan-proof-v2'")
with (HERE/'prove_parallel_scan_base_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('tool-receipt-corrections-v1.json',dict(static_reader=dict(result=pin(HERE/'static-reader-execution-v3.json'),raw=pin(raw),semantic_exit=0,reporter_error='KeyError stdout: command receipt stores a folder; recover unchanged successful raw stdout without rerunning the reader'),scanner_proof=dict(first=pin(HERE/'prove_parallel_scan_base_v1.py'),corrected=pin(HERE/'prove_parallel_scan_base_v2.py'),reason='First controls completed but final receipt referred to the pre-versioning proof filename. All first helper artifacts retained; repeat only these small synthetic equivalence controls under a fresh private folder.'),production_test_changes=0))
