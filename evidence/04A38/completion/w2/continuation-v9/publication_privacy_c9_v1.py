"""Exact C8 privacy decisions cached by policy/path/bytes; all new bytes use unchanged policy."""
import json,sys,hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
sys.path.append(str(HERE.parent/'continuation-v6'))
import publication_policy as policy
need=policy.need;sha=policy.sha;safe_path=policy.safe_path
def checked(row):
    data=(ROOT/row['path']).read_bytes();need((len(data),sha(data))==(row['bytes'],row['sha256']),'Privacy authority changed '+row['path']);return data
authority=json.loads((HERE/'publication-privacy-authority-v1.json').read_bytes());checked(authority['base_policy']);previous=json.loads(checked(authority['C8_publication_receipt']))
need(Path(policy.__file__).resolve()==(ROOT/authority['base_policy']['path']).resolve() and sha(Path(policy.__file__).read_bytes())==authority['base_policy']['sha256'],'Wrong loaded privacy policy module/source')
need(previous['physical_zip']['status']=='ALL_PHYSICAL_MEMBERS_SHA_STORED_CRC_PRIVACY_PASS','Prior complete physical privacy authority absent')
records=previous['physical_zip']['records'];need(len(records)==6306 and len({r['path'] for r in records})==6306,'Prior privacy population changed')
cache={(r['path'],r['bytes'],r['sha256']):[] for r in records}
for finding in previous['physical_zip']['privacy_findings']:
    r=next(r for r in records if r['path']==finding['path']);cache[r['path'],r['bytes'],r['sha256']].append(finding)
stats=dict(prior_exact=0,fresh=0)
def privacy(path,data):
    key=path,len(data),sha(data)
    if key in cache:stats['prior_exact']+=1;return cache[key]
    stats['fresh']+=1;return policy.privacy(path,data)
