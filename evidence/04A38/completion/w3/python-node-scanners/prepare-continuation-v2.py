"""Preserve genuine prelaunch failure and schedule only the unstarted campaign."""
from common import *

old=load(HERE/'python-node-scanners/python-input-postcheck-v1.json')
assert old['command']['exit_code']==1 and not old['changed']
workerroot=RUNROOT/'w3pyv1'
assert not workerroot.exists(),'UNEXPECTED_STARTED_WORKER_TREE'
save('python-node-scanners/prelaunch-stop-v2.json',dict(
    status='PRELAUNCH_DIRECTORY_INITIALIZATION_FAILURE;ZERO_IDENTITIES_STARTED',
    command=old['command'],postcheck=pin(HERE/'python-node-scanners/python-input-postcheck-v1.json'),
    raw_stderr=pin(RUNROOT/'commands/w3v1-python-full-campaign/stderr.log'),
    absent_worker_root=str(workerroot.relative_to(ROOT)),journals=0,workers_started=0,parents_started=0,
    correction='Initialize only the new private parent directory; unchanged worker/assertion code then executes all unstarted partitions once. No completed identity repeated.'))
workerroot.mkdir(exist_ok=False)
save('matrix/python-campaign-amendment-v2.json',dict(campaign='W3-PYTHON-v1',
    state='ALL956_PARENTS_UNSTARTED_AFTER_PRESERVED_PRELAUNCH_FAILURE',
    driver=pin(HERE/'python-node-scanners/full-python-campaign-v2.py'),
    command=['<OWNER_PYTHON>','-I','-B','evidence/04A38/completion/w3/run-private-w3-v1.py','full-python-campaign-unstarted-v2','python-node-scanners/full-python-campaign-v2.py'],
    inventory=pin(HERE/'python-node-scanners/campaign-v1/python-discovery-inventory-v1.json'),
    parents=956,parent_reference=dict(PASS=830,FAIL=113,ERROR=13),event_reference=dict(FAIL=185,ERROR=13),
    ordinary_workers_at_peak=3,scanner_workers=1,scanner_helpers=0,owner_worker_ceiling=8,
    shared_scanner_schedule='After all ordinary and disposable partitions, scanner alone',
    full_campaign_executions_scheduled=1,completed_identity_repetitions=0))
names=set(m.git('diff','--name-only').splitlines())|set(m.git('ls-files','--others','--exclude-standard').splitlines())
target=HERE/'python-node-scanners/python-input-freeze-v2.json';self_name=target.relative_to(ROOT).as_posix()
previous=load(HERE/'python-node-scanners/python-input-freeze-v1.json')
prior={r['path']:r for r in previous['files'] if r['sha256'] is not None}
rows=[]
for name in sorted(names|{self_name}):
    assert 'TestResults' not in Path(name).parts
    if name==self_name:rows.append(dict(path=name,bytes=None,sha256=None))
    elif name in prior:rows.append(prior[name])
    elif (ROOT/name).is_file():rows.append(pin(ROOT/name))
save('python-node-scanners/python-input-freeze-v2.json',dict(files=rows,previous=pin(HERE/'python-node-scanners/python-input-freeze-v1.json'),
    prelaunch_postcheck=pin(HERE/'python-node-scanners/python-input-postcheck-v1.json'),
    scope='Prior8853 exact hashes retained verbatim after completed postcheck. Only additive bookkeeping/readers separately pinned. Every actual byte is rehashed before and after the one unstarted full execution.',
    new_input_files=len(rows)-len(prior),production_or_test_changes=0))
print('Unstarted campaign parent initialized; additive input freeze',len(rows),flush=True)
