"""Bind additive post-seal authorities into the final actual live Git receipt."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v10'))
from common import *
source=(W2/'continuation-v10-postseal-v2.py').read_text(encoding='utf-8')
point="target=W2/'continuation-v10-final-git-preservation-v1.json'"
extra="""doc['postseal_reader']=pin(Path(__file__))
doc['postseal_addendum']=pin(W2/'continuation-v10-postseal-handover-addendum-v1.md')
doc['post_publication_artifacts']=pin(W2/'continuation-v10-post-publication-artifacts-v1.json')
artifact_index=json.loads((W2/'continuation-v10-post-publication-artifacts-v1.json').read_bytes())
for row in artifact_index['source_after_images']+artifact_index['current_compiled_artifacts']+artifact_index['external_authorities']:
    assert pin(ROOT/row['path'])=={k:row[k] for k in ['path','bytes','sha256']}
"""
assert point in source;source=source.replace(point,extra+point)
with (W2/'continuation-v10-postseal-v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(W2/'continuation-v10-finalize-handover-v1.py').read_text(encoding='utf-8').replace('continuation-v10-postseal-v2.py','continuation-v10-postseal-v3.py')
with (W2/'continuation-v10-finalize-handover-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
with (W2/'continuation-v10-final-seal-source-crosswalk-v1.json').open('x',encoding='utf-8',newline='\n') as f:json.dump(dict(postseal=dict(before=pin(W2/'continuation-v10-postseal-v2.py'),after=pin(W2/'continuation-v10-postseal-v3.py')),handover=dict(before=pin(W2/'continuation-v10-finalize-handover-v1.py'),after=pin(W2/'continuation-v10-finalize-handover-v2.py')),scope='Add only exact hashes of the external additive handover/artifact index and verify its actual source/runtime/external inputs before the first final live Git receipt. Earlier versions are unexecuted drafts retained; no previous receipt rewritten.',original_queries=0),f,ensure_ascii=False,indent=2);f.write('\n')
print('Final live seal versioned with external source/runtime/addendum bindings')
