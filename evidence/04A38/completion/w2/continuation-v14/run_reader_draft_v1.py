from common import *
from verify_c14_checkpoint_template_v1 import compute
prefix=HERE.relative_to(ROOT).as_posix()+'/';draft=PRIVATE/'draft-proof-v1';overrides={prefix+p.name:p.read_bytes() for p in draft.iterdir() if p.is_file()}
result=compute(overrides=overrides,check_validation=False,check_predecessor=False)
with (draft/'independent-draft-reader-v1.json').open('x',encoding='utf-8') as f:json.dump(result,f,indent=2)
print(json.dumps(dict(draft_only=True,historical=result['historical'],current=result['current'],support=result['support'],prior_reader_replay='NOT_REPEATED_IN_PRIVATE_DRAFT_ALREADY_AUTHENTICATED_AT_ENTRY',original_queries=0)))
