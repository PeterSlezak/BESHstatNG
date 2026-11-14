"""Single scheduled full Python campaign, exact pre/post input read-back."""
from common import *

lock()
assert load(HERE/'build-and-tests/Release-identities-v1.json')['summary']['executed']>0
target=HERE/'python-node-scanners/campaign-v1'
frozen=load(HERE/'python-node-scanners/python-input-freeze-v1.json')
rows=[r for r in frozen['files'] if r['sha256'] is not None]
for r in rows:assert pin(ROOT/r['path'])==r,'PRELAUNCH_INPUT_CHANGED '+r['path']
script=target/'python_workers_v4.py'
code='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
r=command('python-full-campaign',[sys.executable,'-I','-B','-c',code,str(target),str(script),'execute'])
post=[dict(expected=row,actual=pin(ROOT/row['path'])) for row in rows if pin(ROOT/row['path'])!=row]
save('python-node-scanners/python-input-postcheck-v1.json',dict(command=r,checked=len(rows),changed=post,
    input_freeze=pin(HERE/'python-node-scanners/python-input-freeze-v1.json'),campaigns=1,
    status='INPUTS_UNCHANGED' if not post else 'SOURCE_OR_INPUT_DRIFT',worker_accounting=pin(target/'python-execution-summary-v1.json') if (target/'python-execution-summary-v1.json').is_file() else None))
assert not post and r['exit_code']==0,'PYTHON_CAMPAIGN_STOP_PRESERVED'
lock()
