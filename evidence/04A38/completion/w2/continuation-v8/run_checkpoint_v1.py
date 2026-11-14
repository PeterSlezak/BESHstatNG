"""Record exact final-run commands; private outputs during frozen Python scan."""
from common import *
mode=sys.argv[1]
if mode=='python':
    frozen=json.loads((HERE/'python-scan-input-freeze-v1.json').read_bytes())
    for r in frozen['files']:
        if r['sha256'] is not None:assert pin(ROOT/r['path'])==r
    result=command('python-complete-population-v1',[sys.executable,'-B',str(HERE/'python_workers_v2.py'),'execute'])
    # Workers have ended; appended result receipts are explicitly post-scan.
    unchanged=[]
    for r in frozen['files']:
        if r['sha256'] is not None:assert pin(ROOT/r['path'])==r;unchanged.append(r['path'])
    save('python-frozen-input-postcheck-v1.json',dict(status='EXACT_FROZEN_INPUTS_UNCHANGED',checked=len(unchanged),command=clean(result),post_scan_scope='Generated validation/reader/review/publication receipts do not purport to have been scanned by the earlier historical test population. They receive separate final publication privacy and integrity checks.'))
    assert result['exit_code']==0
elif mode in ['seal','controls']:
    result=command('reader-'+mode+'-v1',[sys.executable,'-B',str(HERE/'seal_and_controls_v1.py'),mode])
    save(mode+'-execution-v1.json',clean(result));assert result['exit_code']==0
else:raise ValueError(mode)
