"""Execute a separate read-only verifier in a copied intended publication root."""
import shutil,uuid
from common import *
from verify_publication import inspect
manifest=sys.argv[1];view=RUNROOT/('c2-export-'+uuid.uuid4().hex[:8]);view.mkdir()
for r in json.loads((ROOT/manifest).read_bytes())['entries']:
 dst=view/r['path'];dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(ROOT/r['path'],dst)
# The copied verifier itself is executed; it does not import live producer logic.
r=command('public-copy-'+uuid.uuid4().hex[:8],[sys.executable,'-B',str(view/HERE.relative_to(ROOT)/'verify_publication.py'),manifest],cwd=view)
assert r['exit_code']==0
print(json.dumps(dict(status='SEPARATE_COPIED_EXECUTABLE_READER_PASS',copied_payloads=len(json.loads((ROOT/manifest).read_bytes())['entries']),command=clean(r),copy='INCLUDED_FILES_ONLY_NOT_LIVE_GIT')))
