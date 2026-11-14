"""Byte-exact private draft publication only after all frozen Python workers exit."""
import hashlib,json
from pathlib import Path
ROOT=Path.cwd().resolve();assert (ROOT/'.git').is_dir();HERE=ROOT/'evidence/04A38/completion/w2/continuation-v9';PRIVATE=ROOT/'.test-rationalization-04A38.tmp/completion-w2-cont9/postscan-tooling-v1'
def pin(path):
    data=path.read_bytes();return dict(path=path.relative_to(ROOT).as_posix(),bytes=len(data),sha256=hashlib.sha256(data).hexdigest())
post=HERE/'python-frozen-input-postcheck-v1.json';assert json.loads(post.read_bytes())['status']=='EXACT_FROZEN_INPUTS_UNCHANGED'
assert json.loads((HERE/'python-worker-summary-v1.json').read_bytes()) if (HERE/'python-worker-summary-v1.json').is_file() else (HERE/'python-worker-receipts-v1.json').is_file()
rows=[]
for path in sorted(PRIVATE.glob('*.py')):
    compile(path.read_bytes(),str(path),'exec')
    target=HERE.parent/'continuation-v9-retained-private-reader-v1.py' if path.name=='retained_private_reader_v1.py' else HERE/path.name
    with target.open('xb') as stream:stream.write(path.read_bytes())
    before=pin(path);after=pin(target);assert before['sha256']==after['sha256'] and before['bytes']==after['bytes'];rows.append(dict(private_draft=before,published=after))
result=dict(status='BYTE_EXACT_POSTSCAN_BOOKKEEPING_ONLY',files=rows,frozen_campaign_postcheck=pin(post),scope='Private drafts prepared while workers ran; published only after complete worker execution and frozen-input postcheck. No production/test/static-proof edit. These future bookkeeping files were not inputs to the historical scanner campaign; all receive final independent publication privacy/hash controls.')
with (HERE/'postscan-tooling-crosswalk-v1.json').open('x',encoding='utf-8',newline='\n') as stream:json.dump(result,stream,ensure_ascii=False,indent=2);stream.write('\n')
print('Published postscan tools',len(rows))
