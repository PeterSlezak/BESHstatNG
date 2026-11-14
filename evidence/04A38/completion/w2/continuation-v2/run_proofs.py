from common import *
import verify_slice_v2,verify_host_cache_v2
receipts=[]
for name in ['verify_slice_v2','verify_host_cache_v2']:
 r=command('independent-'+name,[sys.executable,'-B',str(HERE/(name+'.py'))]);receipts.append(clean(r));assert r['exit_code']==0
save('independent-slice-proof.json',verify_slice_v2.inspect())
save('host-cache-proof.json',verify_host_cache_v2.inspect())
save('independent-reader-commands.json',dict(commands=receipts,original_queries=0,status='BOUNDED_PROOFS_PASS_NOT_BATCH_READY'))
