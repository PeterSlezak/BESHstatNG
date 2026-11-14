"""Exact prior privacy decisions memoized by policy/path/content; fresh C8 scan.

The two explicit whole-file synthetic control fixtures preserve their original
positive examples. No historical scanner/assertion is changed or waived.
"""
import json,hashlib,sys,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent;C6=HERE.parent/'continuation-v6'
sys.path.append(str(C6))
import publication_policy as policy
need=policy.need;sha=policy.sha;safe_path=policy.safe_path
def _read_pin(row):
    data=(ROOT/row['path']).read_bytes();need((len(data),sha(data))==(row['bytes'],row['sha256']),'Privacy authority pin changed '+row['path']);return data
auth=json.loads((HERE/'privacy-authority-v1.json').read_bytes());_read_pin(auth['policy']);previous=json.loads(_read_pin(auth['C7_publication_receipt']))
need(previous['physical_zip']['status']=='ALL_PHYSICAL_MEMBERS_SHA_STORED_CRC_PRIVACY_PASS','Missing complete prior privacy result')
records=previous['physical_zip']['records'];need(len(records)==5915 and len({r['path'] for r in records})==5915,'Prior decision population changed')
cache={(r['path'],r['bytes'],r['sha256']):[] for r in records}
for finding in previous['physical_zip']['privacy_findings']:
    row=next(r for r in records if r['path']==finding['path']);cache[row['path'],row['bytes'],row['sha256']].append(finding)
fixtures={r['source']['path']:r for r in auth['synthetic_control_sources']};stats=dict(prior_exact=0,fresh=0,synthetic=0)
def synthetic_findings(path,data):
    record=fixtures[path];need((len(data),sha(data))==(record['source']['bytes'],record['source']['sha256']),'Changed synthetic fixture')
    text=data.decode('utf-8-sig').replace(chr(92)*2,chr(92));found=[]
    for kind,pattern in policy.PATTERNS.items():
        matches=pattern.findall(text)
        if not matches:continue
        need(kind in ['home','credential_uri'],'Unexpected synthetic fixture privacy category')
        for value in matches:
            collapsed=value
            while chr(92)*2 in collapsed:collapsed=collapsed.replace(chr(92)*2,chr(92))
            expected=('C:'+chr(92)+'Users'+chr(92)+'synthetic-owner') if kind=='home' else ('https'+'://'+'synthetic'+':'+'example'+'@')
            need(collapsed==expected,'Unattested synthetic fixture literal')
        found.append(dict(path=path,kind=kind,view='raw',whole_file_sha256=sha(data),occurrences=len(matches),fragment_sha256_counts={sha(v.encode()):matches.count(v) for v in matches},classification='EXACT_C8_SYNTHETIC_ADVERSE_CONTROL_SOURCE_NOT_PRIVATE_MACHINE_OR_CREDENTIAL_DATA'))
    need({r['kind'] for r in found}==set(record['categories']),'Synthetic fixture finding coverage changed')
    return found
def privacy(path,data):
    key=(path,len(data),sha(data))
    if key in cache:stats['prior_exact']+=1;return cache[key]
    if path in fixtures and (len(data),sha(data))==(fixtures[path]['source']['bytes'],fixtures[path]['source']['sha256']):
        stats['synthetic']+=1;return synthetic_findings(path,data)
    stats['fresh']+=1;return policy.privacy(path,data)
