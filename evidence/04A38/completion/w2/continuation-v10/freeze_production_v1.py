"""Freeze final production/test inputs before compatibility and comprehensive validation."""
from common import *
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());rows=[pin(ROOT/p) for p in plan['source_paths']]
save('production-test-freeze-v1.json',dict(status='C10_FINAL_PRODUCTION_TEST_INPUTS_FROZEN',sources=rows,focused_A=pin(HERE/'focused-v3-packet-A-v1-Debug.json'),focused_BC_first=pin(HERE/'focused-bc-v1-packet-v2-Debug.json'),focused_BC_correction=pin(HERE/'focused-copy-corrective-v2.json'),Debug_production=pin(HERE/'build-packet-v2-Debug.json'),Release_production=pin(HERE/'build-packet-v2-Release.json'),new_managed_tests=dict(Common=21,Windows=9,total=30),Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',original_queries=0))
source=(PREVIOUS/'compatibility_v1.py').read_text(encoding='utf-8').replace('from common_v2 import *','from common import *')
with (HERE/'compatibility_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
print('Six final production/test paths frozen; source/proof bookkeeping may continue')
