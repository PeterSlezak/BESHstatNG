import json,os,uuid
from pathlib import Path
from verify_publication import ROOT,W,inspect
view=ROOT/'.test-rationalization-04A38.tmp'/('w1-copy-'+uuid.uuid4().hex[:8]);view.mkdir()
rows=json.loads((ROOT/(W+'publication-manifest-v1.json')).read_bytes())['entries']
for r in rows:
 target=view/r['path'];target.parent.mkdir(parents=True,exist_ok=True);os.link(ROOT/r['path'],target)
print(json.dumps(inspect(view)))
