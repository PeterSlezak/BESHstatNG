"""Freeze actual candidate bytes without traversing ignored history."""
from common import *
import importlib.metadata

lock()
names=set(m.git('diff','--name-only').splitlines())|set(m.git('ls-files','--others','--exclude-standard').splitlines())
rows=[]
target=HERE/'python-node-scanners/python-input-freeze-v1.json'
self_name=target.relative_to(ROOT).as_posix()
for name in sorted(names|{self_name}):
    assert 'TestResults' not in Path(name).parts
    if name==self_name:rows.append(dict(path=name,bytes=None,sha256=None))
    elif (ROOT/name).is_file():rows.append(pin(ROOT/name))
save('python-node-scanners/python-input-freeze-v1.json',dict(status='FROZEN_BEFORE_SINGLE_W3_FULL_PYTHON_CAMPAIGN',files=rows,
    self_entry='One nonrecursive null self-entry; all other candidate bytes pinned.',
    immutable_scope='No external public source/evidence writes during worker execution. Worker accounting emitted after final scanner; post-run bookkeeping independently verified.',
    distributions=sorted([dict(name=d.metadata['Name'],version=d.version) for d in importlib.metadata.distributions()],key=lambda r:(r['name'] or '').casefold()),
    interpreter=pin(Path(sys.executable)) if Path(sys.executable).is_relative_to(ROOT) else dict(bytes=Path(sys.executable).stat().st_size,sha256=sha(Path(sys.executable).read_bytes()),name=Path(sys.executable).name)))
print('Frozen Python candidates',len(rows),'bytes',sum(r['bytes'] or 0 for r in rows),flush=True)
