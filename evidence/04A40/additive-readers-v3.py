"""Preserve stopped readers; use original typed-row hash and report physical drift."""
from pathlib import Path
here=Path(__file__).parent
s=(here/'compatibility-readback-v2.py').read_text(encoding='utf-8')
assert s.count('row_sha256=sha(line.encode())')==1
s=s.replace('row_sha256=sha(line.encode())','row_sha256=sha(json.dumps(row,sort_keys=True).encode())').replace('current-only-v2.json','current-only-v3.json')
(here/'compatibility-readback-v3.py').open('x',encoding='utf-8').write(s)
s=(here/'preservation-v1.py').read_text(encoding='utf-8')
s=s.replace('lock();checkpoint=', 'lock();physical_drift=[];checkpoint=')
s=s.replace("stat=(ROOT/r['path']).stat();assert stat.st_dev==r['device'] and stat.st_ino==r['file_id']", "stat=(ROOT/r['path']).stat()\n    if (stat.st_dev,stat.st_ino)!=(r['device'],r['file_id']):physical_drift.append(dict(path=r['path'],authority='468_CHECKPOINT',before=[r['device'],r['file_id']],after=[stat.st_dev,stat.st_ino],bytes_exact=True))")
s=s.replace("st=(ROOT/r['path']).stat();assert st.st_dev==r['device'] and st.st_ino==r['file_id']", "st=(ROOT/r['path']).stat()\n        if (st.st_dev,st.st_ino)!=(r['device'],r['file_id']):physical_drift.append(dict(path=r['path'],authority='PROTECTED_UNION',before=[r['device'],r['file_id']],after=[st.st_dev,st.st_ino],bytes_exact=True))")
s=s.replace("save('preservation/historical-readback-v1.json'", "save(PRIVATE/'physical-file-id-drift-v2.json',dict(records=physical_drift,disposition='BYTES_IDENTICAL;PHYSICAL_CONTINUITY_NOT_VERIFIED;NO_REBASELINE'))\nsave('preservation/historical-readback-v2.json'")
s=s.replace('original_file_identity_and_bytes=True',"original_bytes_exact=True,physical_identity_unchanged=not physical_drift,physical_identity_drift_count=len(physical_drift),physical_identity_drift=pin(PRIVATE/'physical-file-id-drift-v2.json'),physical_disposition='NOT_VERIFIABLE_FILE_ID_DRIFT;OWNER_PROVENANCE_REVIEW_REQUIRED'")
(here/'preservation-v2.py').open('x',encoding='utf-8').write(s)
