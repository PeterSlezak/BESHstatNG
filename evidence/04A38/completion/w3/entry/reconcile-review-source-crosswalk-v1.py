"""Review snapshots may carry a different transport path; reconcile exact bytes."""
from common import *
from collections import defaultdict
index=load(W2/'continuation-v17-postcorrection-v1-review-parts-index-v1.json')
byhash=defaultdict(list)
for r in index['files']: byhash[(r['bytes'],r['sha256'])].append(r['path'])
rows=[]
for r in load(ACCEPTED/'production-test-freeze-v1.json')['sources']:
    assert pin(ROOT/r['path'])==r
    rows.append(dict(source=r,transport_paths=byhash[r['bytes'],r['sha256']],live_verified=True))
absent=[r for r in rows if not r['transport_paths']]
save('entry/review-source-crosswalk-v1.json',dict(rows=rows,accessible_by_exact_bytes=len(rows)-len(absent),
    live_only=absent,live_only_count=len(absent),source_verified=1350,
    report_correction='entry-manifest-v2 live_only=832 measures absence of identical transport PATH, not absence of source BYTES. Source-lock-v2 verified all1350. This additive crosswalk uses exact bytes/SHA for relocated review snapshots; no source or acceptance assertion changed.'))
print('Review source accessible',len(rows)-len(absent),'live-only',len(absent),flush=True)
assert len(absent)==18
