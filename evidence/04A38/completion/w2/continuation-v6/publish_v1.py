"""Explicit manifest only. No traversal of retained historical TestResults."""
from common import *
from publication_policy import forbidden,safe_path
paths={r['path'] for r in json.loads((PREVIOUS/'publication-manifest-v1.json').read_bytes())['entries']}
paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and not forbidden(p.relative_to(ROOT).as_posix()))
paths.update(r['path'] for r in json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'])
paths.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v5-*') if p.is_file())
delta=json.loads((HERE/'python-source-pin-delta-v1.json').read_bytes())
paths.update(delta[key]['path'] for key in ['original_historical_test','historical_contract','historical_record'])
manifest_path=(HERE/'publication-manifest-v1.json').relative_to(ROOT).as_posix()
inventory_path=(HERE/'file-inventory-v1.json').relative_to(ROOT).as_posix()
paths.update([manifest_path,inventory_path])
paths=sorted(paths)
for path in paths:safe_path(path)
save('file-inventory-v1.json',dict(paths=paths,scope='C5 explicit manifest union C6 explicit local evidence, final source files and C5 external receipts; derivative payload only',
    physically_excluded=['All raw full-managed Debug diagnostics of every version, retained locally','All compiled Python caches',
                         'Private compiled binaries/toolchain/command raw logs','Owner documents omitted from derivative inventory; live16 checked separately',
                         'Unlisted ignored history including retained TestResults; no traversal or cleanup'],
    lineage='Prior manifests and intermediate failed/provisional records remain immutable. Final authority: verify_checkpoint_v5.py, source-transitions-final-v1, families-v2 binaries and review-seal-v5. W2 partial, never READY.'))
entries=[]
for path in paths:
    entries.append(dict(path=path,bytes=None,sha256=None) if path==manifest_path else pin(ROOT/path))
save('publication-manifest-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',inventory='file-inventory-v1.json',
    self_entry='Exactly one nonrecursive null self-entry',entries=entries,new_original_queries=0))
print('Sealed C6 payloads',len(entries)-1)
