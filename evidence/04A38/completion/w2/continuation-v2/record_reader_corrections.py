from common import *
from baseline_report import python_receipt
from verify_a1_dependency import inspect
records=[]
for label,script in [('a1-operations','verify_a1_dependency.py'),('preedit-unique-isolation-v3','recheck_preedit_isolated_v3.py')]:
 r=command(label,[sys.executable,'-B',str(HERE/script)]);records.append(clean(r));assert r['exit_code']==0
save('a1-whole-operation-proof.json',inspect())
save('preedit-isolation-correction.json',dict(commands=records,actual_tests=python_receipt('w2c2-preedit-unique-isolation-v3'),earlier_attempt=dict(label='w2c2-predecessor-preedit',executed_methods=0,setup_errors=1,passed=0,reason='Reused private test destination existed; no previous file removed or overwritten. Baseline receipt formula total-errors yields -1 for setUpClass and is not a valid PASS total; use executed_methods0/setupERROR1. Same immutable source/assertions subsequently28PASS in fresh private root.'),original_queries=0))
