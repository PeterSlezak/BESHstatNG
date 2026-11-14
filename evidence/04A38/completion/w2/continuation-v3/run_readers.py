from common import *
from baseline_report import python_receipt
import verify_ownership,verify_preservation,verify_text_domains
receipts=[]
for label,script in [('ownership','verify_ownership.py'),('text','verify_text_domains.py'),('preservation','verify_preservation.py')]:
 r=command('independent-'+label,[sys.executable,'-B',str(HERE/script)]);receipts.append(clean(r));assert r['exit_code']==0
save('independent-ownership-proof.json',verify_ownership.inspect());save('preservation.json',verify_preservation.inspect());save('text-domain-proof.json',verify_text_domains.inspect())
r=command('independent-adverse',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_readers.py','-v'])
save('independent-controls.json',dict(readers=receipts,adverse=clean(r),tests=python_receipt('w2c3-independent-adverse'),original_queries=0));assert r['exit_code']==0
