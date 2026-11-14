"""Version bounded review publication; normal short copy paths, exact policy cache."""
from common import *
source=(PREVIOUS/'verify_publication_v1.py').read_text(encoding='utf-8')
source=source.replace("sys.path.append(str(HERE.parent/'continuation-v6'))\nfrom publication_policy import need,sha,safe_path,privacy", "sys.path.append(str(HERE))\nfrom publication_privacy_v1 import need,sha,safe_path,privacy")
source=source.replace('from verify_c7_checkpoint_v1 import compute','from verify_c8_checkpoint_v1 import compute').replace('from verify_preservation_v1 import inspect as preservation','from verify_c8_preservation_v1 import inspect as preservation')
source=source.replace('C7','C8')
with (HERE/'verify_c8_publication_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(PREVIOUS/'publish_v1.py').read_text(encoding='utf-8').replace('verify_publication_v1','verify_c8_publication_v1').replace('C7','C8').replace('c7','c8').replace('continuation-v6','continuation-v7').replace('continuation_v7','continuation_v8').replace('continuation-v7-publication-receipt-v1.json','continuation-v8-publication-receipt-v1.json')
# Keep the predecessor references C7 and new final receipt C8 distinct.
source=source.replace("previous=json.loads((PREVIOUS/'publication-manifest-v1.json').read_bytes())", "previous=json.loads((PREVIOUS/'publication-manifest-v1.json').read_bytes())")
source=source.replace("['continuation-v8-publication-receipt-v1.json','continuation-v7-final-git-preservation-v1.json']", "['continuation-v7-publication-receipt-v1.json','continuation-v7-final-git-preservation-v1.json']")
source=source.replace("copied=Path('\\\\\\\\?\\\\'+str((folder/'copied-root').resolve()));copied.mkdir()", "copied=ROOT/'.c8-review-copy-v1.tmp';assert max(len(str(copied/r['path'])) for r in rows)<260;copied.mkdir(exist_ok=False)")
assert "copied=ROOT/'.c8-review-copy-v1.tmp'" in source
source=source.replace('BESHstatNG_04A38_W2_continuation_v7_review.zip','BESHstatNG_04A38_W2_continuation_v8_review.zip')
with (HERE/'publish_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(PREVIOUS/'postseal_v1.py').read_text(encoding='utf-8').replace('verify_preservation_v1','verify_c8_preservation_v1').replace('continuation-v7','continuation-v8')
with (HERE/'postseal_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('publication-tool-crosswalk-v1.json',dict(verifier=dict(before=pin(PREVIOUS/'verify_publication_v1.py'),after=pin(HERE/'verify_c8_publication_v1.py')),publisher=dict(before=pin(PREVIOUS/'publish_v1.py'),after=pin(HERE/'publish_v1.py')),postseal=dict(before=pin(PREVIOUS/'postseal_v1.py'),after=pin(HERE/'postseal_v1.py')),policy=pin(HERE/'publication_privacy_v1.py'),policy_proof=pin(HERE/'publication-privacy-equivalence-v1.json'),scope='All selected payload SHA/CRC remain fresh; prior exact-input privacy decisions are authenticated cache reuse, not new scans. C8 changed/new bytes scanned; two exact new synthetic fixture sources have bounded positive controls. Copy uses a new short normal path, retaining every C7 long-path failure/copy/receipt. Review ZIP only, no XLL packaging.',original_queries=0))
