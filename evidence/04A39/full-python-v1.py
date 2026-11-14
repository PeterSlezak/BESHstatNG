"""One final frozen Python/scanner campaign. No public evidence writers until workers end."""
from common39 import *
import importlib.metadata

source_lock()
target=HERE/'python-node-scanners/campaign-v2'
prior=load(W3/'python-node-scanners/python-input-freeze-v2.json')
names={x['path'] for x in prior['files'] if x.get('sha256')}
names.update(git('ls-files','--others','--exclude-standard').decode().splitlines())
names.update(git('diff','--name-only').decode().splitlines())
names.update(x['path'] for x in load(HERE/'entry/source-preimages-v1.json')['files'])
assert not any('TestResults' in Path(name).parts for name in names)
rows=[pin(name) for name in sorted(names) if (ROOT/name).is_file()]
save('python-node-scanners/input-freeze-v1.json',dict(utc=utc(),files=rows,
    scope='All W3 full-campaign public input paths, all1350 current source paths and current39 candidates. Exact bounded files, no recursive TestResults traversal.',
    distributions=sorted([dict(name=d.metadata['Name'],version=d.version) for d in importlib.metadata.distributions()],key=lambda x:(x['name'] or '').casefold()),
    interpreter=dict(name=Path(sys.executable).name,sha256=sha(Path(sys.executable).read_bytes())),
    freeze_self='Self and subsequent execution/accounting/dossier bookkeeping excluded to avoid recursive hashes. No source/input mutation allowed during execution.'))
code='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
result=command('python-full',[sys.executable,'-I','-B','-c',code,str(target),str(target/'python_workers_v4.py'),'execute'])
changed=[dict(before=row,after=pin(row['path'])) for row in rows if pin(row['path'])!=row]
save('python-node-scanners/input-postcheck-v1.json',dict(command=result,checked=len(rows),changed=changed,
    input_freeze=pin(HERE/'python-node-scanners/input-freeze-v1.json'),campaigns=1,
    summary=pin(target/'python-execution-summary-v1.json') if (target/'python-execution-summary-v1.json').is_file() else None))
assert not changed and result['exit_code']==0
source_lock()
