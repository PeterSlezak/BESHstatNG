"""Authenticate old decisions and reject any changed synthetic-positive source."""
from common import *
import publication_privacy_c10_v1 as current
controls=[]
for path in ['evidence/04A38/completion/w2/continuation-v9/owner-review.md','BESHStat.Core/src/DM/NumericGuards.vb']:
    data=(ROOT/path).read_bytes();assert current.privacy(path,data)==current.policy.privacy(path,data);controls.append(dict(name='Exact prior/base equality',path=path,status='PASS'))
for path in ['evidence/04A38/completion/w2/continuation-v9/prove_parallel_scan_v1.py','evidence/04A38/completion/w2/continuation-v9/prove_publication_privacy_v1.py']:
    data=(ROOT/path).read_bytes();assert current.privacy(path,data);controls.append(dict(name='Exact previously bounded synthetic source decision retained',path=path,status='PASS'))
    for label,changed_path,changed in [('changed bytes',path,data+b' '),('changed path',path+'.other',data),('unattested profile',path,data+(' C:'+chr(92)+'Users'+chr(92)+'unattested-owner').encode())]:
        try:current.privacy(changed_path,changed)
        except ValueError:controls.append(dict(name=label,status='REJECTED'))
        else:raise AssertionError('Privacy cache bypass accepted '+label)
for name,data in [('key',('-----'+'BEGIN '+'PRIVATE KEY'+'-----').encode()),('credential',('https'+'://'+'unattested'+':'+'secret'+'@'+'example.invalid').encode())]:
    try:current.privacy('new-unattested-'+name+'.txt',data)
    except ValueError:controls.append(dict(name='Fresh '+name,status='REJECTED'))
    else:raise AssertionError('Unattested privacy value accepted')
row=dict(current.authority['base_policy'],sha256='0'*64)
try:current.checked(row)
except ValueError:controls.append(dict(name='Wrong policy source pin',status='REJECTED'))
else:raise AssertionError('Wrong policy source accepted')
save('publication-privacy-equivalence-v1.json',dict(status='EXACT_PRIOR_DECISION_CACHE_AND_FRESH_POLICY_REJECTIONS_PASS',reader=pin(HERE/'publication_privacy_c10_v1.py'),authority=pin(HERE/'publication-privacy-authority-v1.json'),controls=controls,new_exceptions=0,scope='Cached decisions are explicitly historical exact-input reuse; hashes/CRC remain fresh. All changed/new payloads receive unchanged base policy evaluation. Historical Python scanner failures are never relabelled or waived.'))
print('C10 privacy cache/source/mutation controls PASS',len(controls))
