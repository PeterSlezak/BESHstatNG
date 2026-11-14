"""Freeze the actual historical Git candidate domain; never walk ignored history."""
from common import *
names=set(git('diff','--name-only').decode().splitlines())|set(git('ls-files','--others','--exclude-standard').decode().splitlines());target=HERE/'python-scan-input-freeze-v1.json';self_name=target.relative_to(ROOT).as_posix();names.add(self_name);rows=[]
for path in sorted(names):
    assert 'TestResults' not in Path(path).parts
    if path==self_name:rows.append(dict(path=path,bytes=None,sha256=None))
    elif (ROOT/path).is_file():rows.append(pin(ROOT/path))
save(target.name,dict(status='FROZEN_PRE_FULL_PYTHON_ACTUAL_GIT_CANDIDATE_SET',files=rows,self_entry='One nonrecursive null self-entry; all other actual bytes pinned before execution.',scope='No public source/evidence writes during the campaign. Private worker outputs are isolated. Later generated aggregate/reader/publication bookkeeping is separately final-hash/privacy verified, not claimed as an earlier historical scanner input.'))
print('Frozen Python candidates',len(rows),'bytes',sum(r['bytes'] or 0 for r in rows))
