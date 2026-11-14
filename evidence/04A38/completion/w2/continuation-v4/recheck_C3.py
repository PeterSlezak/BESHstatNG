"""Historical C3 readers execute against exact immutable C3 source projection."""
import shutil
from common import *
from baseline_report import python_receipt
view=RUNROOT/'c4-C3-exact-view';view.mkdir();aliases={r['path']:ROOT/r['preimage']['path'] for r in json.loads((HERE/'source-transitions.json').read_bytes())['files']};manifest=PREVIOUS/'publication-manifest-v2.json';mapped=[]
for r in json.loads(manifest.read_bytes())['entries']:
 src=ROOT/r['path']
 if r['sha256'] is not None and sha(src.read_bytes())!=r['sha256']:src=aliases[r['path']];mapped.append(dict(path=r['path'],retained=src.relative_to(ROOT).as_posix()))
 if r['sha256'] is not None:assert (src.stat().st_size,sha(src.read_bytes()))==(r['bytes'],r['sha256'])
 dst=view/r['path'];dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dst)
(view/'.test-rationalization-04A38.tmp').mkdir();records=[]
for label,pattern in [('source','test_readers.py'),('scope','test_scope.py'),('publication','test_publication.py')]:
 name='C3-reader-'+label;r=command(name,[sys.executable,'-B','-m','unittest','discover','-s',str(view/PREVIOUS.relative_to(ROOT)),'-p',pattern,'-v'],cwd=view);records.append(dict(command=clean(r),tests=python_receipt('w2c4-'+name)))
save('C3-reader-replay.json',dict(scope='EXACT_3424_HASH_C3_PROJECTION;NOT_CURRENT_C4_SOURCE',source_aliases=mapped,results=records,original_queries=0))
