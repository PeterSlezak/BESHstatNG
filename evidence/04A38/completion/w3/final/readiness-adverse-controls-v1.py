"""Positive controls and assertion-preserving adverse specimens; no source mutation."""
from common import *
import copy,importlib.util

spec=importlib.util.spec_from_file_location('w3_gate',HERE/'final/verify-w3-v1.py')
v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v)
controls=[]
def reject(name,fn):
    try:fn()
    except (AssertionError,ValueError,KeyError,FileNotFoundError):controls.append(dict(name=name,status='REJECTED'))
    else:raise AssertionError('ADVERSE_SPECIMEN_ACCEPTED '+name)

row=load(ACCEPTED/'production-test-freeze-v1.json')['sources'][0];raw=(ROOT/row['path']).read_bytes()
v.exact_bytes(row,raw)
reject('changed production bytes',lambda:v.exact_bytes(row,raw+b' '))
reject('missing production file',lambda:v.exact_bytes(row,None))
g=load(HERE/'entry/git-v2.json');v.validate_git(g)
for label,mutate in [('changed branch',lambda x:x['commands']['branch'].update(stdout='main')),
                     ('changed HEAD',lambda x:x['commands']['HEAD'].update(stdout='0'*40)),
                     ('staged changes',lambda x:x['commands']['staged'].update(stdout='a.vb')),
                     ('changed live index bytes',lambda x:x['index'].update(sha256='0'*64))]:
    x=copy.deepcopy(g);mutate(x);reject(label,lambda x=x:v.validate_git(x))
expected=load(ACCEPTED/'managed-Debug-identities-v1.json')['tests']
d=load(HERE/'build-and-tests/Debug-identities-v1.json');v.validate_managed(d,expected)
for label,mutate in [('missing test',lambda x:x['tests'].pop()),
                     ('duplicate occurrence',lambda x:x['tests'].append(copy.deepcopy(x['tests'][0]))),
                     ('changed nonpass message',lambda x:x['nonpasses'][0].update(message_sha256='0'*64))]:
    x=copy.deepcopy(d);mutate(x);reject(label,lambda x=x:v.validate_managed(x,expected))
baseline=load(W2/'continuation-v12/compatibility-packet-v1.json')
c=load(HERE/'compatibility/Release-comparisons-v1.json');v.validate_compat(c,baseline)
for label,mutate in [('missing frozen case',lambda x:x['results'][0].update(cases=x['results'][0]['cases']-1)),
                     ('duplicate corpus',lambda x:x['results'].append(copy.deepcopy(x['results'][0]))),
                     ('mutated typed input',lambda x:x['results'][0]['inputs'].update(sha256='0'*64)),
                     ('replaced original baseline',lambda x:x['results'][0]['original'].update(path='new-original.tsv')),
                     ('supplement promoted to pre-edit',lambda x:next(r for r in x['results'] if r['corpus']=='A').update(provenance='PRE_EDIT_FROZEN'))]:
    x=copy.deepcopy(c);mutate(x);reject(label,lambda x=x:v.validate_compat(x,baseline))
closure=load(HERE/'abi-cil/complete-closure-v4.json');v.validate_closure(closure)
for label,mutate in [('missing whole-body identity',lambda x:x['entries'].pop()),
                     ('incomplete fragment ownership',lambda x:x['fragment_counts'].update(original=20682)),
                     ('removed complete proof',lambda x:x['entries'][0].update(proof=None)),
                     ('unresolved transition',lambda x:x['entries'][0].update(transition='OPEN'))]:
    x=copy.deepcopy(closure);mutate(x);reject(label,lambda x=x:v.validate_closure(x))
inspection=load(HERE/'abi-cil/Release-inspection-v1.json')
actual=load(ROOT/inspection['exports']['path'])['exports'];original=load(ROOT/inspection['sealed_original_export_baseline']['path'])['exports']
v.validate_exports(actual,original)
x=copy.deepcopy(actual);x.pop();reject('missing export',lambda:v.validate_exports(x,original))
x=copy.deepcopy(actual);x[0]['effective_excel_name']+='changed';reject('changed export',lambda:v.validate_exports(x,original))
matrix={c:{k:True for k in ['build','managed','abi-cil','compatibility']} for c in ['Debug','Release']};v.validate_configuration_categories(matrix)
for category in ['build','managed','abi-cil','compatibility']:
    x=copy.deepcopy(matrix);del x['Release'][category];reject('missing Release '+category,lambda x=x:v.validate_configuration_categories(x))
py=load(HERE/'python-node-scanners/python-identities-v1.json');v.validate_python(py)
x=copy.deepcopy(py);x['parents'].pop();reject('missing Python parent',lambda:v.validate_python(x))
x=copy.deepcopy(py);next(r for r in x['parents'] if r['outcome']=='FAIL')['outcome']='PASS';reject('known failing subtests counted as parent PASS',lambda:v.validate_python(x))
x=copy.deepcopy(py);x['events'].append(copy.deepcopy(x['events'][0]));reject('duplicated nonpass event',lambda:v.validate_python(x))
sys.path.append(str(ACCEPTED));from verify_transport_v1 import safe_path,validate_manifest
for p in ['../escape','/absolute','C:/absolute','a\\b','.git/config','a/CON.txt','trailing.']:
    reject('unsafe derivative '+p,lambda p=p:safe_path(p))
safe=dict(scope='GIT_FREE_REVIEW_DERIVATIVE;NOT_A_LIVE_CHECKOUT',private_raw_included=False,files=[dict(path='a.txt',bytes=1,sha256='0'*64,crc32='0'*8)])
validate_manifest(safe)
x=copy.deepcopy(safe);x['files'].append(dict(x['files'][0],path='A.txt'));reject('case insensitive collision',lambda:validate_manifest(x))
sys.path.append(str(W2/'continuation-v6'));import publication_policy as policy
policy.privacy('synthetic-safe.txt',b'bounded software evidence')
for name,data in [('private home',('C:'+chr(92)+'Users'+chr(92)+'synthetic'+chr(92)+'a').encode()),
                  ('private key',('-----BEGIN '+'PRIVATE KEY-----').encode()),
                  ('credential URI',('https:'+'//synthetic:synthetic'+'@example.invalid').encode())]:
    reject('publication privacy '+name,lambda data=data:policy.privacy('new-w3-adverse.txt',data))
save('final/adverse-controls-v1.json',dict(status='ACTUAL_READINESS_AND_PUBLICATION_VALIDATORS_REJECT_ADVERSE_SPECIMENS',
    controls=controls,count=len(controls),source_mutations=0,test_reexecutions=0,
    preserved_report_versions=['complete-closure-v1','complete-closure-v2','complete-closure-v3','reference-authority-audit-v1','debug-compatibility-v1','release-compatibility-v1'],
    assertion_scope='Existing complete measured fields retained; debugger attributes, value-type constructor opcode form and older null-signature schema have explicit source/CIL dispositions, with changed target/export/input/fragment rejection controls.'))
print('Readiness/publication adverse controls',len(controls),'PASS',flush=True)
