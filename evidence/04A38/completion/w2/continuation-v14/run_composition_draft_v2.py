from common import *
from verify_c14_composition_v2 import inspect
draft=PRIVATE/'draft-proof-v1'
def load(name):
 local=draft/Path(name).name
 return json.loads(local.read_bytes()) if local.is_file() and name.startswith(HERE.relative_to(ROOT).as_posix()+'/') else json.loads((ROOT/name).read_bytes())
result=inspect(load_override=load)
with (draft/'source-composition-proof-v1.json').open('x',encoding='utf-8') as f:json.dump(result,f,indent=2)
print('Complete35 parent/property source composites pass draft inspection')
