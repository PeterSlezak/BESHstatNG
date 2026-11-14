from common import *
save('publication-privacy-authority-v1.json',dict(base_policy=pin(W2/'continuation-v6/publication_policy.py'),
    C10_publication_receipt=pin(W2/'continuation-v10-publication-receipt-v1.json'),C12_complete_manifest=pin(PREVIOUS/'publication-manifest-v1.json'),
    C12_final_git=pin(W2/'continuation-v12-final-git-preservation-v1.json'),
    law='Only exact path/length/SHA and unchanged policy decisions from authenticated complete C12 inventory or prior C10 physical receipts are reused. Every changed/new selected byte receives unchanged base policy evaluation at this checkpoint; no new exception or historical scanner/assertion change.',original_queries=0))
for old,new,replacements in [
    ('prove_publication_privacy_v1.py','prove_publication_privacy_v1.py',{'publication_privacy_c12_v1':'publication_privacy_c13_v1','C12 privacy':'C13 privacy'}),
    ('publication_adverse_v1.py','publication_adverse_v1.py',{'verify_c12_publication_v1':'verify_c13_publication_v1'}),
    ('verify_c12_publication_v1.py','verify_c13_publication_v1.py',{'publication_privacy_c12_v1':'publication_privacy_c13_v1','verify_c12_checkpoint_v2':'verify_c13_checkpoint_v2','verify_c12_preservation_v1':'verify_c13_preservation_v1','C12_':'C13_'})]:
    source=PREVIOUS/old;value=source.read_text(encoding='utf-8')
    for a,b in replacements.items():value=value.replace(a,b)
    with (HERE/new).open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
    save('source-crosswalk-'+new[:-3]+'.json',dict(before=pin(source),after=pin(HERE/new),original_queries=0))
print('Same privacy/adverse/physical reader assertions with authenticated exact C12 policy cache')
