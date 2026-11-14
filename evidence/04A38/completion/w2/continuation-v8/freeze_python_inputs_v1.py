"""Freeze the actual historical scanner Git-candidate domain, without ignored walks."""
from common import *
names=set(git('diff','--name-only').decode().splitlines())|set(git('ls-files','--others','--exclude-standard').decode().splitlines())
target=HERE/'python-scan-input-freeze-v1.json';name=target.relative_to(ROOT).as_posix();names.add(name)
rows=[]
for path in sorted(names):
    assert 'TestResults' not in Path(path).parts
    if path==name:rows.append(dict(path=path,bytes=None,sha256=None));continue
    value=ROOT/path
    if value.is_file():rows.append(pin(value))
save(target.name,dict(status='FROZEN_PRE_FULL_PYTHON_ACTUAL_GIT_CANDIDATE_SET',files=rows,self_entry='One nonrecursive null; all other actual bytes pinned before execution.',scope='Exact union of historical scanner diff/untracked candidates at launch. No ignored historical tree traversal. Runtime writes are private; post-run generated evidence is separately publication-checked.',input_commands=['git --no-optional-locks diff --name-only','git --no-optional-locks ls-files --others --exclude-standard']))
print('Frozen scanner candidates',len(rows))
