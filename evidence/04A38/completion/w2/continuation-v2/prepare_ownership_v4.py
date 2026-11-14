from common import *
s=(HERE/'verify_ownership_v3.py').read_text(encoding='utf-8').replace('from functools import cache','from functools import cache as json_cache').replace(' @cache\n',' @json_cache\n')
with (HERE/'verify_ownership_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/ownership-reader-v3.json',dict(status='FAILED_SETUP',error='UnboundLocalError: decorator cache name collided with local cache-proof variable; v4 uses explicit json_cache import alias. No verification or source policy changed.',reader=pin(HERE/'verify_ownership_v3.py')))
