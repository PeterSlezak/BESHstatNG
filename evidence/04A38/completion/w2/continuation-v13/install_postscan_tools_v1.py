from common import *
assert json.loads((HERE/'python-frozen-input-postcheck-v1.json').read_bytes())['status']=='EXACT_FROZEN_INPUTS_UNCHANGED'
rows=[]
for source in sorted((PRIVATE/'bookkeeping-code-v1').glob('*.py')):
    target=W2/source.name if source.name.startswith('continuation-v13-') else HERE/source.name
    assert not target.exists(),'Never overwrite an earlier source/receipt'
    with target.open('xb') as stream:stream.write(source.read_bytes())
    rows.append(dict(private_source=pin(source),published_source=pin(target)))
save('postscan-bookkeeping-source-crosswalk-v1.json',dict(files=rows,original_queries=0,
    scope='Ignored private tooling developed during the immutable Python run; installed only after its exact candidate postcheck. Later evidence is independently hashed/privacy checked and never claimed as an earlier scanner input.'))
print('Append-only postscan tooling installed',len(rows))
