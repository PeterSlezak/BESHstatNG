"""Check all newly prepared review payloads before immutable manifest sealing."""
from common_v2 import *
from publication_privacy_c9_v1 import privacy,safe_path,stats
paths={p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts}
paths.update(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'])
paths.add((W2/'continuation-v9-retained-private-reader-v1.py').relative_to(ROOT).as_posix())
rows=[];findings=[]
for path in sorted(paths):
    safe_path(path);data=(ROOT/path).read_bytes();findings.extend(privacy(path,data));rows.append(pin(ROOT/path))
save('prepublication-fresh-privacy-v1.json',dict(status='PRESEAL_CURRENT_PAYLOAD_PRIVACY_PASS',files=rows,findings=findings,policy_calls=stats,scope='Current prepared review payloads before manifest creation. Future inventory/self-manifest and final physical payloads receive separate complete policy checks; no new exception.'))
print('Preseal fresh privacy',len(rows),'files',stats)
