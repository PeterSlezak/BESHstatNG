"""Seal current proof/source inputs before append-only verification/publication receipts."""
from common import *
names={p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts}
for n in ['current-universe.json','dependency-full-core.json','dependency-full-common.json','dependency-full-adjacent.json']:
 for r in json.loads((HERE/'final-v7'/n).read_bytes())['sources']:names.add(r['path'])
names.update(r['path'] for r in json.loads((HERE/'production-test-freeze-v7.json').read_bytes())['sources'])
names.add('planning/BESHStatNG_CrossPlatform_MasterPlan.md')
assert not any('TestResults' in Path(n).parts or n.endswith('continuation-v9/python-first-failures-v1.json') for n in names)
save('review-seal-v1.json',dict(status='NOT_READY_04A38_SOURCE_BOUND_PROPOSAL',files=[pin(ROOT/n) for n in sorted(names)],source_freeze=pin(HERE/'production-test-freeze-v7.json'),owner_review=pin(HERE/'owner-review.md'),scope='All current source/proof inputs sealed. Later verification/privacy/transport/postseal receipts are additive and separately hash-bound by the final external handover.',original_queries=0))
print('Current source/proof seal created',len(names),'pinned inputs')
