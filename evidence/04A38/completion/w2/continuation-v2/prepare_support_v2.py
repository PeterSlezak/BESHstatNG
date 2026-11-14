from common import *
s=(HERE/'record_support.py').read_text(encoding='utf-8').replace("elif kind=='PropertyStatement':\n  name=", "elif kind=='PropertyStatement':\n  owner=next(t['type'] for t in configs['Debug'][1]['types'] if t['type'].replace('+','.')==owner)\n  name=")
with (HERE/'record_support_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/support-recorder-v1.json',dict(status='FAILED_METADATA_OWNER_LOOKUP',script=pin(HERE/'record_support.py'),error='StopIteration: Roslyn nested source owner uses period while CLR nested type uses plus. New v2 resolves exact unique CLR type by explicit nested-name mapping. No declaration/body closure was published by v1.'))
