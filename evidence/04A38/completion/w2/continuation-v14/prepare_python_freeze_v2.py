from common import *
source=HERE/'freeze_python_inputs_v1.py';v=source.read_text().replace('python-scan-input-freeze-v1.json','python-scan-input-freeze-v2.json')
with (HERE/'freeze_python_inputs_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
source2=HERE/'run_python_v1.py';v=source2.read_text().replace('python-scan-input-freeze-v1.json','python-scan-input-freeze-v2.json')
v=v.replace("checked=len(rows),command=clean(r)","checked=len(rows),inventory=pin(HERE/'python-scan-input-freeze-v2.json'),command=clean(r)")
with (HERE/'run_python_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('python-freeze-launch-correction-v1.json',dict(first_inventory=pin(HERE/'python-scan-input-freeze-v1.json'),source_before=pin(source),source_after=pin(HERE/'freeze_python_inputs_v2.py'),run_before=pin(source2),run_after=pin(HERE/'run_python_v2.py'),
 failed_launch='Direct run_python_v1.py was launched before the asynchronous inventory writer finished. It rejected missing inventory before starting a single Python parent; exact traceback remains in tool history.',
 reason='The outer generic bookkeeping runner also adds an execution receipt after an inventory writer exits. Preserve v1 and freeze v2 only after all pre-campaign public files exist, using the direct writer with no post-freeze public receipt. The actual campaign has not begun and will run once.',parent_tests_executed=0,original_queries=0))
