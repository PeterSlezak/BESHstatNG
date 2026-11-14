"""Unique C12 identity validator; old assertions/outcomes are independently reconciled."""
from common import *
source=PREVIOUS/'verify_c11_validation_v1.py';value=source.read_text(encoding='utf-8')
value=value.replace('from proof_kernel_v3 import','from proof_kernel_v4 import')
value=value.replace("OLD='evidence/04A38/completion/w2/continuation-v10/'","OLD='evidence/04A38/completion/w2/continuation-v11/'")
value=value.replace('==20817 and len(prior)==20788','==20848 and len(prior)==20817')
value=value.replace('len(additions)==29','len(additions)==31').replace('Passed=20803','Passed=20834').replace("['new_passes']==29","['new_passes']==31").replace('new_managed_PASS=29','new_managed_PASS=31')
target=HERE/'verify_c12_validation_v1.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('final-validation-source-crosswalk-v1.json',dict(before=pin(source),after=pin(target),original_queries=0,
    scope='Same independent identity/multiset/nonpass/worker/Node/source-freeze checks with measured31 new tests and exact C11 baseline. No test/scanner assertion altered.'))
from verify_c12_validation_v1 import inspect
load=lambda name:json.loads((ROOT/name).read_bytes());raw=lambda name:(ROOT/name).read_bytes()
result=inspect(load,raw,HERE.relative_to(ROOT).as_posix()+'/')
save('validation-independent-replay-v1.json',result)
print('All actual managed/Python/Node identity and frozen-source receipts independently reconciled')
