"""Authenticate cached decisions and tightly bound synthetic fixture positives."""
from common import *
sys.path.append(str(W2/'continuation-v6'));import publication_policy as old
entry=json.loads((HERE/'v7-publication-entry-v1.json').read_bytes());assert pin(ROOT/entry['publication']['path'])==entry['publication']
fixtures=[]
for name in ['prove_parallel_scan_v1.py','prepare_parallel_scan_v2.py']:
    data=(HERE/name).read_bytes();text=data.decode('utf-8-sig').replace(chr(92)*2,chr(92));kinds=[kind for kind,p in old.PATTERNS.items() if p.search(text)]
    assert set(kinds)=={'home','credential_uri'},(name,kinds)
    fixtures.append(dict(source=pin(HERE/name),categories=kinds,role='Immutable synthetic positive privacy-control source; whole-file and exact normalized literal bounds, never arbitrary home/URI data'))
save('privacy-authority-v1.json',dict(policy=pin(W2/'continuation-v6/publication_policy.py'),C7_publication_receipt=entry['publication'],synthetic_control_sources=fixtures,law='Deterministic unchanged policy(path,bytes) decisions from the complete authenticated C7 physical ZIP replay only on identical path/length/SHA256. Every changed/new byte uses the original policy. Two exact new synthetic control source files have independently bounded positive literals; no extra path/content/category waiver.',original_queries=0))
import publication_privacy_v1 as reader
controls=[]
prior=json.loads((ROOT/entry['publication']['path']).read_bytes())
for path in ['evidence/04A38/completion/w2/continuation-v7/owner-review.md','BESHStat.Excel.Common.Tests/src/CompositeOutputTests.vb']:
    data=(ROOT/path).read_bytes();expected=old.privacy(path,data);assert reader.privacy(path,data)==expected;controls.append(dict(name='Exact prior decision and original policy equality',path=path,status='PASS'))
for fixture in fixtures:
    path=fixture['source']['path'];data=(ROOT/path).read_bytes();assert reader.privacy(path,data)
    controls.append(dict(name='Exact explicitly synthetic source positive findings retained',path=path,status='PASS'))
    for label,target,changed in [('Changed bytes',path,data+b' '),('Changed path',path+'.other',data),('Injected unrelated profile',path,data+b' C:'+bytes([92])+b'Users'+bytes([92])+b'unattested-owner')]:
        try:reader.privacy(target,changed)
        except ValueError:controls.append(dict(name=label+' rejected',status='REJECTED'))
        else:raise AssertionError('Privacy mutation accepted '+label)
for name,data in [('private_key',b'-----BEGIN PRIVATE KEY-----'),('credential_uri',('https'+'://'+'unattested'+':'+'secret'+'@'+'example.invalid').encode())]:
    try:reader.privacy('new-unattested-'+name+'.txt',data)
    except ValueError:controls.append(dict(name='Fresh '+name+' rejected',status='REJECTED'))
    else:raise AssertionError('Fresh privacy finding accepted')
save('publication-privacy-equivalence-v1.json',dict(status='EXACT_DECISION_CACHE_AND_BOUNDED_SYNTHETIC_CONTROL_POSITIVES_PROVED',reader=pin(HERE/'publication_privacy_v1.py'),authority=pin(HERE/'privacy-authority-v1.json'),controls=controls,scope='Fresh all-payload hashes/CRC remain mandatory. Prior privacy decisions are explicitly historical exact-input cache reuse; C8 new/changed files receive fresh policy evaluation. Original historical Python scanners retain all their actual FAIL/ERROR outcomes.',original_queries=0))
print('Publication privacy cache and12 exact/adverse controls PASS')
