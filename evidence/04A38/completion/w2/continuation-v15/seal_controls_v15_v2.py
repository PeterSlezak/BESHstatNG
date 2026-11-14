"""Read-only final positive replay and adverse controls; no campaign execution."""
from common import *
import importlib.util,copy
seal=json.loads((HERE/'review-seal-v1.json').read_bytes());path=ROOT/seal['reader']['path'];assert pin(path)==seal['reader']
spec=importlib.util.spec_from_file_location('c15_final_checkpoint',path);verify=importlib.util.module_from_spec(spec);spec.loader.exec_module(verify)
positive=verify.compute();save('checkpoint-positive-v1.json',positive)
controls=[]

def reject(name,action):
 try:action()
 except (AssertionError,ValueError,OSError,KeyError,TypeError):controls.append(dict(name=name,status='REJECTED'))
 else:raise AssertionError('Adverse input accepted '+name)

for name,source in [('missing current source',json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'][0]['path']),
 ('changed full parent branch',HERE.relative_to(ROOT).as_posix()+'/whole-body-contracts-v1.json'),
 ('missing exact Cox callback',HERE.relative_to(ROOT).as_posix()+'/callback-proofs-v1.json'),
 ('wrong original property owner',HERE.relative_to(ROOT).as_posix()+'/property-body-contracts-v1.json'),
 ('changed constructed generic accessor/default',HERE.relative_to(ROOT).as_posix()+'/host-cache-contexts-v1.json'),
 ('changed ByRef/return CIL',HERE.relative_to(ROOT).as_posix()+'/final-v1/current-Debug.json'),
 ('changed Release signature',HERE.relative_to(ROOT).as_posix()+'/final-v1/current-Release.json'),
 ('omitted branch fragment',HERE.relative_to(ROOT).as_posix()+'/new-parent-fragment-crosswalk-v3.json'),
 ('unsupported canonical closure',HERE.relative_to(ROOT).as_posix()+'/canonical-identity-status-v1.json'),
 ('changed ordered OPEN queue',HERE.relative_to(ROOT).as_posix()+'/remaining-work-queue-v2.json'),
 ('relabelled inherited test failure',HERE.relative_to(ROOT).as_posix()+'/managed-Debug-identities-v2.json'),
 ('relabelled Python ERROR',HERE.relative_to(ROOT).as_posix()+'/python-effective-identities-v1.json'),
 ('missing owner replacement authority',HERE.relative_to(ROOT).as_posix()+'/owner-replacement-debug-authorization-v1.json')]:
 reject(name,lambda source=source:verify.compute(overrides={source:b''},predecessor=False))
from proof_kernel_v8 import closure
meta=json.loads((HERE/'final-v1/current-Debug.json').read_bytes());diag=json.loads((HERE/'cohort-diagnostic-v1.json').read_bytes());roots=diag['roots']['Debug'];assert len(closure(meta,roots)['methods'])==1053
target=next(m for m in meta['methods'] if m['method'] in diag['graphs']['Debug']['methods'] and m['method'] not in roots)
bad=copy.deepcopy(meta);bad['methods']=[m for m in bad['methods'] if m['method']!=target['method']]
reject('internal transitive method omitted',lambda:verify.need(closure(bad,roots)==diag['graphs']['Debug'],'Missing actual internal method'))
from publication_privacy_c15_v1 import policy
for source in ['../escape','A/../escape','C:/escape','A\\escape','A/CON','A/name.','A//double']:
 reject('unsafe archive path '+source,lambda source=source:policy.safe_path(source))
for name,data in [('key',('-----'+'BEGIN '+'PRIVATE KEY'+'-----').encode()),('credential',('https'+'://'+'owner'+':'+'secret'+'@'+'example.invalid').encode())]:
 reject('fresh privacy '+name,lambda name=name,data=data:policy.privacy('new-adverse-'+name+'.txt',data))
save('adversarial-reader-controls-v1.json',dict(status='ALL_ADVERSE_CONTROLS_REJECTED',reader=seal['reader'],positive=pin(HERE/'checkpoint-positive-v1.json'),controls=controls,original_queries=0))
print('Positive final reader and',len(controls),'adverse controls complete')
