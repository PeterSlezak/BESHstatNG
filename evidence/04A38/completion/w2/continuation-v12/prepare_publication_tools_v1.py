"""Keep unchanged privacy policy; reuse only exact prior physical content decisions."""
from common import *
source=PREVIOUS/'publication_privacy_c11_v1.py'
with (HERE/'publication_privacy_c12_v1.py').open('xb') as stream:stream.write(source.read_bytes())
save('publication-privacy-authority-v1.json',dict(base_policy=pin(W2/'continuation-v6/publication_policy.py'),C10_publication_receipt=pin(W2/'continuation-v10-publication-receipt-v1.json'),
    law='Unchanged policy and exact prior physical path/size/SHA decisions only. Every changed/new selected payload is freshly evaluated once when the checkpoint manifest is built. Later readers hash every payload and authenticate the recorded decision; manifest-self is separately scanned. No new exception or scanner assertion change.',new_privacy_exceptions=0,original_queries=0))
save('publication-privacy-source-crosswalk-v1.json',dict(before=pin(source),after=pin(HERE/'publication_privacy_c12_v1.py'),policy_changed=False,original_queries=0))
print('Unchanged selected-publication privacy policy/source authenticated')
