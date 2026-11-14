"""Explicit manifest only. No traversal of retained historical TestResults."""
from common import *
from publication_policy import forbidden,safe_path
paths={r['path'] for r in json.loads((PREVIOUS/'publication-manifest-v3.json').read_bytes())['entries']}
paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and not forbidden(p.relative_to(ROOT).as_posix()))
paths.update(r['path'] for r in json.loads((HERE/'source-transitions-v4.json').read_bytes())['files'])
manifest_path=(HERE/'publication-manifest-v1.json').relative_to(ROOT).as_posix()
inventory_path=(HERE/'file-inventory-v1.json').relative_to(ROOT).as_posix()
paths.update([manifest_path,inventory_path])
paths=sorted(paths)
for path in paths:safe_path(path)
save('file-inventory-v1.json',dict(paths=paths,scope='C4 explicit manifest union C5 explicit local evidence and final source files; derivative payload only',
    physically_excluded=['All raw full-managed Debug diagnostics of every version, retained locally','All compiled Python caches',
                         'Private compiled binaries/toolchain/command raw logs','Owner documents omitted from derivative inventory; live16 checked separately',
                         'Unlisted ignored history including retained TestResults; no traversal or cleanup'],
    lineage='Prior manifests and all intermediate failed/provisional C5 records remain immutable; final authority is suffix-v3 ledgers plus source-transitions-v4 and cells-v5 binaries.'))
entries=[]
for path in paths:
    entries.append(dict(path=path,bytes=None,sha256=None) if path==manifest_path else pin(ROOT/path))
save('publication-manifest-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',inventory='file-inventory-v1.json',
    self_entry='Exactly one nonrecursive null self-entry',entries=entries,new_original_queries=0))
print('Sealed C5 payloads',len(entries)-1)
