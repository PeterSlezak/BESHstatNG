"""Additive post-seal launcher correction; no sealed/private bytes are changed."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v13'))
from common import *
source=HERE/'finalize_external_v1.py'
# The original failure is retained in tool history. Capture an explicitly
# labelled identical diagnostic repeat rather than claim it was the first.
args=[sys.executable,'-I','-B',str(source)];r=subprocess.run(args,cwd=ROOT,capture_output=True)
assert r.returncode==1 and b"ModuleNotFoundError: No module named 'common'" in r.stderr
receipt=W2/'continuation-v13-postseal-launch-correction-v1.json'
doc=dict(status='FAILED_LAUNCH_BEFORE_ANY_WORK_PRESERVED_AND_CORRECTED',
 original_attempt='Initial tool result: isolated finalize_external_v1.py exited1 at line2 importing common. Raw original tool result remains in conversation history; this receipt does not relabel the repeat as the first.',
 diagnostic_repeat=dict(command=clean(args),exit_code=r.returncode,stdout=clean(r.stdout.decode()),stderr=clean(r.stderr.decode()),stdout_bytes=len(r.stdout),stderr_bytes=len(r.stderr),stdout_sha256=sha(r.stdout),stderr_sha256=sha(r.stderr)),
 source=pin(source),preparation_tool=pin(Path(__file__).resolve()),
 remedy='Explicitly insert the documented C13 tool directory before importing common; actual successor private reader --verify remains isolated. No source, test, seal, part, manifest or private file changed.',original_queries=0)
with receipt.open('x',encoding='utf-8',newline='\n') as f:json.dump(doc,f,indent=2);f.write('\n')
prefix="import sys\nfrom pathlib import Path\nsys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v13'))\n"
v=source.read_text();v=v.replace('from common import *',prefix+'from common import *',1)
v=v.replace("sealed_review=pin(HERE/'owner-review.md'),","postseal_launch_correction=pin(W2/'continuation-v13-postseal-launch-correction-v1.json'),sealed_review=pin(HERE/'owner-review.md'),",1)
v=v.replace(" 'Checkpoint reader continuation-v13/", " 'Post-seal launch correction: the initial isolated bookkeeping tool stopped at importing common, before any verification or receipt write. The exact original tool history and explicitly labelled diagnostic repeat are preserved in continuation-v13-postseal-launch-correction-v1.json. Additive continuation-v13-finalize-external-v2.py explicitly inserts the documented C13 tool directory; all sealed/private/archive bytes remain unchanged.',\n 'Checkpoint reader continuation-v13/",1)
target=W2/'continuation-v13-finalize-external-v2.py'
compile(v,str(target),'exec')
with target.open('x',encoding='utf-8',newline='\n') as f:f.write(v)
print(json.dumps(dict(correction=pin(receipt),successor=pin(target))))
