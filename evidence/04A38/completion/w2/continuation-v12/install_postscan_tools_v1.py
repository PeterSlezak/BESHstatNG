"""Publish append-only bookkeeping only after exact comprehensive Python input postcheck."""
from common import *
assert json.loads((HERE/'python-frozen-input-postcheck-v1.json').read_bytes())['status']=='EXACT_FROZEN_INPUTS_UNCHANGED'
rows=[]
for path in sorted((PRIVATE/'bookkeeping-code-v1').glob('*.py')):
    target=W2/path.name if path.name.startswith('continuation-v12-') else HERE/path.name
    assert not target.exists(),'Never overwrite a source or receipt'
    with target.open('xb') as stream:stream.write(path.read_bytes())
    rows.append(dict(private_source=pin(path),published_source=pin(target)))
save('postscan-bookkeeping-source-crosswalk-v1.json',dict(files=rows,scope='Developed only in ignored private tree during the frozen Python run; published after its exact postcheck. Later source/evidence receives independent final hash/privacy/publication checks and is never claimed as an earlier scanner input.',original_queries=0))
print('Additive postscan sources installed',len(rows))
