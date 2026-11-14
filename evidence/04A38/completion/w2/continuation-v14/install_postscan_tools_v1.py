from common import *
assert json.loads((HERE/'python-frozen-input-postcheck-v1.json').read_bytes())['status']=='EXACT_FROZEN_INPUTS_UNCHANGED'
book=PRIVATE/'bookkeeping-code-v1';rows=[]
for p in sorted(book.glob('*.py')):
 target=W2/p.name if p.name.startswith('continuation-v14-') else HERE/p.name
 assert not target.exists(),'Never overwrite existing source '+target.name
 with target.open('xb') as f:f.write(p.read_bytes())
 rows.append(dict(private_source=pin(p),published_source=pin(target)))
for p in sorted(book.glob('*-correction-*.json')):
 target=HERE/p.name;assert not target.exists()
 with target.open('xb') as f:f.write(p.read_bytes())
 rows.append(dict(private_source=pin(p),published_source=pin(target)))
save('postscan-bookkeeping-source-crosswalk-v1.json',dict(files=rows,scope='Privately developed tools installed append-only only after exact frozen-input postcheck. Draft outputs are not silently promoted; official source/CIL/body/census artifacts are regenerated and independently verified.',original_queries=0))
