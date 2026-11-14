from common import *
import verify_names,verify_unchanged_v3
receipts=[]
for n in ['verify_names.py','verify_unchanged_v3.py']:
 r=command('independent-'+n[:-3],[sys.executable,'-B',str(HERE/n)]);receipts.append(clean(r));assert r['exit_code']==0
save('independent-names-proof.json',verify_names.inspect())
save('independent-unchanged-proof.json',verify_unchanged_v3.inspect())
save('independent-proof-commands.json',dict(commands=receipts,original_queries=0))
