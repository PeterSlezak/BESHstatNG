from common import *
import verify_host_cache_v4
r=command('host-cache-v4-generic-identity-reader',[sys.executable,'-B',str(HERE/'verify_host_cache_v4.py')]);assert r['exit_code']==0
save('host-cache-v4-proof.json',verify_host_cache_v4.inspect())
save('host-cache-v4-reader-command.json',dict(command=clean(r),original_semantic_queries=0,status='FULL_GENERIC_DECLARING_TYPE_METADATA_IDENTITY_PASS'))
