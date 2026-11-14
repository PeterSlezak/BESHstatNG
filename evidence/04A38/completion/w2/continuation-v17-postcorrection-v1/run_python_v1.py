"""One full Python campaign, with immutable public inputs and exact postchecks."""
from common import *
frozen=json.loads((HERE/'python-scan-input-freeze-v1.json').read_bytes());rows=[r for r in frozen['files'] if r['sha256'] is not None]
for r in rows:assert pin(ROOT/r['path'])==r
r=command('python-complete-population-v1',bootstrap(HERE/'python_workers_v2.py','execute'))
for row in rows:assert pin(ROOT/row['path'])==row
save('python-frozen-input-postcheck-v1.json',dict(status='EXACT_FROZEN_INPUTS_UNCHANGED',checked=len(rows),inventory=pin(HERE/'python-scan-input-freeze-v1.json'),command=clean(r),scope='All frozen public inputs rehashed. No test or implementation edit during the full campaign. Post-run bookkeeping receives separate final publication verification.'));assert r['exit_code']==0
