"""Check every planned payload before making the immutable publication manifest."""
from common import *
from publication_privacy_c10_v1 import privacy,safe_path,stats
previous=json.loads((PREVIOUS/'publication-manifest-v1.json').read_bytes());paths={r['path'] for r in previous['entries']};paths.update(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'])
for name in ['current-universe-packet-v2.json','portable-universe-v1.json','dependency-source-universe-v1.json','declaration-shapes-v1.json']:
    known={p.casefold():p for p in paths}
    for row in json.loads((HERE/name).read_bytes())['sources']:
        if row['path'].casefold() not in known:paths.add(row['path']);known[row['path'].casefold()]=row['path']
paths.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v9-*') if p.is_file());paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts)
assert len(paths)==len({p.casefold() for p in paths})
findings=[]
for p in sorted(paths):safe_path(p);findings.extend(privacy(p,(ROOT/p).read_bytes()))
copied=ROOT/'.c10p.tmp';maximum=max(len(str(copied/p)) for p in paths);assert maximum<260
save('publication-preflight-v1.json',dict(status='ALL_PLANNED_EXISTING_PAYLOADS_POLICY_AND_PATH_PASS',payloads=len(paths),maximum_copy_path_characters=maximum,privacy_findings=findings,policy_stats=stats,authority=pin(HERE/'publication-privacy-authority-v1.json'),scope='Before immutable current manifest; final physical verification checks the manifest and subsequently generated bookkeeping too. Exact unchanged C9 decisions reused only by path/bytes/SHA; all new/changed payloads use unchanged C6 policy. No new exception.'))
print('Publication preflight PASS',len(paths),'max path',maximum,'policy',stats)
