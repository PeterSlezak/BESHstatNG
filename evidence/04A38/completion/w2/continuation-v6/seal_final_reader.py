from common import *
from verify_text_domains_v1 import inspect as text_domains
save('text-domain-proof-v1.json',text_domains())
source=(HERE/'verify_checkpoint_v2.py').read_text(encoding='utf-8')
source=source.replace("result=dict(status='INDEPENDENT_C6_PARTIAL_OWNERSHIP_SOURCE_CIL_COMPATIBILITY_PASS_NOT_READY'", "from verify_source_composition_v2 import inspect as compose\n from verify_text_domains_v1 import inspect as text_proof\n composition=compose(root,overrides);text_domains=text_proof(root,overrides)\n result=dict(source_composition=composition,text_domains=text_domains,status='INDEPENDENT_C6_PARTIAL_OWNERSHIP_SOURCE_CIL_COMPATIBILITY_PASS_NOT_READY'")
with (HERE/'verify_checkpoint_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('review-seal-final.json',dict(reader=pin(HERE/'verify_checkpoint_v3.py'),composition_reader=pin(HERE/'verify_source_composition_v2.py'),text_reader=pin(HERE/'verify_text_domains_v1.py'),prior_seal=pin(HERE/'review-seal-v1.json'),status='PARTIAL_W2_NOT_READY',original_queries=0))
print('Final reader includes independent source/CIL composition and full fresh comparison-domain proof')
