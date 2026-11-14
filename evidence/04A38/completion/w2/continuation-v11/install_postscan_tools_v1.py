"""Install only additive post-campaign bookkeeping, after exact frozen-input postcheck."""
from common import *
import shutil

assert json.loads((HERE/'python-frozen-input-postcheck-v1.json').read_bytes())['status']=='EXACT_FROZEN_INPUTS_UNCHANGED'
folder=PRIVATE/'bookkeeping-code-v1';rows=[]
for path in sorted(folder.glob('*.py')):
    target=W2/path.name if path.name.startswith('continuation-v11-') else HERE/path.name
    assert not target.exists(),'Never overwrite a prior tool/receipt'
    with target.open('xb') as stream:stream.write(path.read_bytes())
    rows.append(dict(private_source=pin(path),published_source=pin(target)))
save('postscan-bookkeeping-source-crosswalk-v1.json',dict(files=rows,
    scope='Newly generated bookkeeping after the immutable historical scanner checkpoint. These files were developed only in the ignored private run while public inputs stayed frozen. They receive separate final manifest/privacy/hash verification, not a claim of being earlier scanner inputs.',original_queries=0))
source=(PREVIOUS/'publication_privacy_c10_v1.py').read_text(encoding='utf-8')
source=source.replace("authority['C9_publication_receipt']", "authority['C10_publication_receipt']").replace('6698','7075')
with (HERE/'publication_privacy_c11_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
save('publication-privacy-authority-v1.json',dict(base_policy=pin(W2/'continuation-v6/publication_policy.py'),
    C10_publication_receipt=pin(W2/'continuation-v10-publication-receipt-v1.json'),
    law='Only exact unchanged policy/path/length/SHA may reuse actual v10 physical privacy decisions. Every fresh/changed payload uses the unchanged base policy. No new exception; protected giant exclusion is review-only.',new_privacy_exceptions=0,original_queries=0))
source=(PREVIOUS/'prove_publication_privacy_v2.py').read_text(encoding='utf-8').replace('publication_privacy_c10_v1','publication_privacy_c11_v1').replace('C10 privacy','C11 privacy')
with (HERE/'prove_publication_privacy_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
source=(PREVIOUS/'fixture_crosswalk_v2.py').read_text(encoding='utf-8').replace('proof_kernel_v2','proof_kernel_v3')
source=source.replace("supersedes_limited_v1=pin(HERE/'compatibility-fixture-crosswalk-v1.json')", "predecessor=pin(PREVIOUS/'compatibility-fixture-crosswalk-v2.json')")
source=source.replace('V1 mapped only top-level Debug targets, an honest limited draft retained, not final comprehensive authority.',
    'The prior complete v10 inventory remains retained; this fresh C11 crosswalk uses actual current shipping CIL.')
with (HERE/'fixture_crosswalk_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
for label,args in [('publication-privacy-proof-v1',[str(HERE/'prove_publication_privacy_v1.py')]),
                   ('fixture-crosswalk-v2',[str(HERE/'fixture_crosswalk_v2.py')]),
                   ('postprocess-v1',[str(HERE/'postprocess_v1.py')])]:
    result=command(label,[sys.executable,'-B',*args]);save(label+'-execution.json',clean(result));assert result['exit_code']==0
print('Additive postscan tooling, complete fixture mapping and identity/failure reports ready',flush=True)
