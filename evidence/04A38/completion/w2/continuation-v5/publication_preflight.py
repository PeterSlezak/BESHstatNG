from common import *
from publication_policy import privacy,safe_path,forbidden
paths={r['path'] for r in json.loads((PREVIOUS/'publication-manifest-v3.json').read_bytes())['entries']}
paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and not forbidden(p.relative_to(ROOT).as_posix()))
paths.update(r['path'] for r in json.loads((HERE/'source-transitions-v3.json').read_bytes())['files'])
findings=[];records=[]
for path in sorted(paths):
    safe_path(path);data=(ROOT/path).read_bytes();findings+=privacy(path,data);records.append(dict(path=path,bytes=len(data),sha256=sha(data)))
save('publication-preflight.json',dict(status='PRELIMINARY_CURRENT_INVENTORY_PRIVACY_PASS_NOT_FINAL_SEAL',paths=len(paths),
    findings=findings,records=records,scope='Final validation/owner dossier not yet sealed; final manifest and independent copy reader remain required',
    excluded='Every raw full-managed Debug diagnostic and Python cache remains local; no TestResults traversal'))
print('Preliminary explicit payload privacy PASS',len(paths))
