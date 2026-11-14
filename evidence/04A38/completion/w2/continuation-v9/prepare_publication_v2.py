"""Version proven publication/copy tooling, with explicit inherited helper binding."""
from common_v2 import *
source=(PREVIOUS/'verify_c8_publication_v4.py').read_text(encoding='utf-8')
start=source.index("sys.path.insert(0,str(HERE.parent/'continuation-v7'))");end=source.index('\ndef inspect(',start)
bootstrap="""sys.path.insert(0,str(HERE.parent/'continuation-v8'))
sys.path.insert(0,str(HERE))
# Reuse the corrected, source-pinned C8 bootstrap, including its exact C7 helper.
import verify_c8_publication_v4 as prior_publication
from publication_privacy_c9_v1 import need,sha,safe_path,privacy
from verify_c9_checkpoint_v1 import compute
from verify_c9_preservation_v1 import inspect as preservation
"""
source=source[:start]+bootstrap+source[end:]
source=source.replace('C8_PARTIAL','C9_PARTIAL').replace('publication-manifest-v2.json','publication-manifest-v1.json').replace('file-inventory-v2.json','file-inventory-v1.json')
with (HERE/'verify_c9_publication_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(PREVIOUS/'publish_v5.py').read_text(encoding='utf-8').replace('from common import *','from common_v2 import *').replace('verify_c8_publication_v4','verify_c9_publication_v1').replace('publication-manifest-v2.json','publication-manifest-v1.json').replace('file-inventory-v2.json','file-inventory-v1.json').replace('w2c8','w2c9').replace('cont8','cont9').replace('continuation-v8-publication-receipt-v1.json','continuation-v9-publication-receipt-v1.json').replace('continuation_v8_review.zip','continuation_v9_review.zip').replace('.c8-review-copy-v1.tmp','.c9-review-copy-v1.tmp').replace('C8_PARTIAL','C9_PARTIAL')
source=source.replace("(PREVIOUS/'publication-manifest-v1.json')","(PREVIOUS/'publication-manifest-v2.json')")
source=source.replace("['continuation-v7-publication-receipt-v1.json','continuation-v7-final-git-preservation-v1.json']","['continuation-v8-publication-receipt-v1.json','continuation-v8-final-git-preservation-v1.json','continuation-v8-retained-private-preservation-v1.json','continuation-v8-retained-private-reader-v1.py','continuation-v8-retained-private-reader-v2.py']")
source=source.replace("    # Keep every append-only C8 source/proof version; private runtime outputs stay private.","    paths.add((W2/'continuation-v9-retained-private-reader-v1.py').relative_to(ROOT).as_posix())\n    # Keep every append-only C8 source/proof version; private runtime outputs stay private.")
source=source.replace("    ordered=sorted(paths)","    paths.discard((HERE/'python-first-failures-v1.json').relative_to(ROOT).as_posix())\n    ordered=sorted(paths)")
source=source.replace("excluded=['Private raw command streams/TRX/caches/compiled DLLs'","excluded=['C9 interrupted 4213129746-byte python-first-failures-v1.json, retained unchanged and pinned by failed-aggregation-attempt-v1.json', 'Private raw command streams/TRX/caches/compiled DLLs'")
with (HERE/'publish_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(PREVIOUS/'postseal_v2.py').read_text(encoding='utf-8').replace('from common import *','from common_v2 import *').replace('verify_c8_preservation_v1','verify_c9_preservation_v1').replace('continuation-v8-publication-receipt-v1.json','continuation-v9-publication-receipt-v1.json').replace('continuation-v8-final-git-preservation-v1.json','continuation-v9-final-git-preservation-v1.json')
with (HERE/'postseal_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('publication-tool-crosswalk-v1.json',dict(verifier=dict(before=pin(PREVIOUS/'verify_c8_publication_v4.py'),after=pin(HERE/'verify_c9_publication_v1.py')),publisher=dict(before=pin(PREVIOUS/'publish_v5.py'),after=pin(HERE/'publish_v1.py')),postseal=dict(before=pin(PREVIOUS/'postseal_v2.py'),after=pin(HERE/'postseal_v1.py')),scope='No new privacy exception; correct inherited validation helper is explicitly bootstrapped. Initial final C9 manifest is v1; failed C8 candidates remain ordinary preserved payloads. Separate actual live/copy/physical/private scopes remain mandatory.'))
