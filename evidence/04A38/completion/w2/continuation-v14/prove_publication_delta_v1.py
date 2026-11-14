from common import *
import publication_privacy_c14_v2 as policy
path='BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb';data=(ROOT/path).read_bytes();a=dict(policy.stats);assert policy.privacy(path,data)==policy.policy.privacy(path,data) and policy.stats['prior_exact']==a['prior_exact']+1
a=dict(policy.stats);assert policy.privacy(path,data+b' ')==policy.policy.privacy(path,data+b' ') and policy.stats['fresh']==a['fresh']+1
controls=[]
for name,fn in [('Changed exact authority',lambda:policy.checked(dict(policy.delta_authority['first_manifest'],sha256='0'*64))),('Fresh private-key marker',lambda:policy.privacy('unattested-delta.txt',('-----'+'BEGIN '+'PRIVATE KEY'+'-----').encode()))]:
 try:fn()
 except ValueError as e:controls.append(dict(name=name,status='REJECTED',reason=clean(str(e))))
 else:raise AssertionError('Delta fallback bypass '+name)
save('publication-delta-equivalence-v1.json',dict(status='EXACT_FIRST_CHECKPOINT_CACHE_AND_UNCHANGED_FRESH_DELTA_POLICY',source=pin(HERE/'publication_privacy_c14_v2.py'),first_manifest=policy.delta_authority['first_manifest'],first_receipt=policy.delta_authority['first_policy_receipt'],prior_exact_equal_base=True,changed_bytes_evaluate_base=True,controls=controls,full_scanner_repeated=False,source_or_assertions_changed=0,original_queries=0))
