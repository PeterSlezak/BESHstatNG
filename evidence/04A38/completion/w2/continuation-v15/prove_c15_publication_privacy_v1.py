"""Authenticate exact inherited decisions and adverse-cache invalidation."""
from common import *
import publication_privacy_c15_v1 as current
controls=[]
for path in ['evidence/04A38/completion/w2/continuation-v9/owner-review.md','BESHStat.Core/src/DM/NumericGuards.vb']:
 data=(ROOT/path).read_bytes();assert current.privacy(path,data)==current.policy.privacy(path,data)
 controls.append(dict(name='Exact inherited/base policy equality',path=path,status='PASS'))
for path in ['evidence/04A38/completion/w2/continuation-v8/prove_parallel_scan_v1.py','evidence/04A38/completion/w2/continuation-v8/prove_publication_privacy_v1.py']:
 data=(ROOT/path).read_bytes();assert current.privacy(path,data)
 controls.append(dict(name='Exact old bounded adverse-fixture decision',path=path,status='PASS'))
 for label,name,changed in [('changed bytes',path,data+b' '),('changed path',path+'.other',data),('unattested home',path,data+(' C:'+chr(92)+'Users'+chr(92)+'unattested-owner').encode())]:
  try:current.privacy(name,changed)
  except ValueError:controls.append(dict(name=label,status='REJECTED'))
  else:raise AssertionError('Privacy cache bypass accepted '+label)
assert pin(ROOT/current.base['path'])==current.base
save('publication-privacy-equivalence-v1.json',dict(status='EXACT_PRIOR_DECISIONS_AND_FRESH_POLICY_REJECTIONS_PASS',reader=pin(HERE/'publication_privacy_c15_v1.py'),base_policy=current.base,prior_manifest=pin(PREVIOUS/'publication-manifest-v2.json'),controls=controls,new_exceptions=0,original_queries=0,scope='Only exact path/size/content/policy hashes reuse authenticated decisions. All changed/new content uses unchanged base policy. Neither historical Python privacy ERROR is relabelled or waived.'))
print('Selected publication privacy equality/adverse controls',len(controls),'complete')
