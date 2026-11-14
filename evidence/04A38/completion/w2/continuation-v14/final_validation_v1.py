from common import *
from verify_c14_validation_v1 import inspect
r=inspect(lambda p:json.loads((ROOT/p).read_bytes()),lambda p:(ROOT/p).read_bytes(),HERE.relative_to(ROOT).as_posix()+'/');save('validation-independent-replay-v1.json',r);print(json.dumps(r))
