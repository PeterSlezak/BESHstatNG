"""One planned full Debug campaign over frozen shipping and test sources."""
from common import *
source=(HERE/'validate_managed_v2.py').read_text(encoding='utf-8').replace('focused-BC-packet-v2-Debug.json','focused-bc-v1-packet-v2-Debug.json')
with (HERE/'validate_managed_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
freeze=json.loads((HERE/'production-test-freeze-v1.json').read_bytes())
save('managed-start-source-state-v1.json',dict(freeze=pin(HERE/'production-test-freeze-v1.json'),runner=pin(HERE/'validate_managed_v3.py'),intent='ONE comprehensive six-suite Debug campaign; no Release unit tests; disjoint Common fixture selections; no production/test changes after freeze.'))
result=command('full-managed-supervisor-v1',[sys.executable,'-B',str(HERE/'validate_managed_v3.py')])
save('managed-supervisor-v1.json',clean(result))
print(json.dumps(clean(result)))
