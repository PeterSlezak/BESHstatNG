from common import *
from collections import Counter
from baseline_report import index
from validation_report import safe_label
previous=json.loads((PREVIOUS/'managed-Debug-identities-v3.json').read_bytes())['tests']
tests=[];added=[];nonpasses=[];totals=Counter();suites=[]
for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
 data=json.loads((RUNROOT/'executions'/('w2c6-validation-v1-Debug-serial-'+suite)/'cases.json').read_bytes())
 now=index(data['records']);before={(r['identity_sha256'],r['occurrence']):r for r in previous if r['suite']==suite}
 assert all(k in now and now[k]['outcome']==r['outcome'] for k,r in before.items()),'Inherited identity/occurrence/outcome drift '+suite
 totals.update(data['outcomes']);suites.append(dict(suite=suite,outcomes=data['outcomes'],exit_code=data['command']['exit_code'],trx_sha256=data['sha256']))
 for key,row in now.items():
  r=dict(suite=suite,identity_sha256=key[0],occurrence=key[1],class_name=row['class_name'],method=row['method'],test_name=safe_label(row['test_name']),outcome=row['outcome'],population='NEW_CONTINUATION_V6' if key not in before else 'INHERITED_C5')
  tests.append(r)
  if key not in before:added.append(r)
  if row['outcome']!='Passed':nonpasses.append(dict(r,message_sha256=sha(row['message'].encode())))
assert all(r['outcome']=='Passed' for r in added)
old_nonpasses=json.loads((PREVIOUS/'managed-Debug-identities-v3.json').read_bytes())['nonpasses']
fields=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'],r['outcome'],r['message_sha256'])
assert Counter(map(fields,nonpasses))==Counter(map(fields,old_nonpasses))
selected={}
for config in ['Debug','Release']:
 focused=json.loads((HERE/('focused-families-v2-'+config+'.json')).read_bytes());core=json.loads((HERE/'selected-Core-families-v2.json').read_bytes())
 for r in [r for r in focused['results'] if r['kind']=='TEST']+[r for r in core['results'] if r['kind']=='TEST' and r['configuration']==config]:
  for key,row in index(r['tests']['records']).items():selected[config,r['suite'],*key]=row['outcome']
save('managed-Debug-identities-v1.json',dict(tests=tests,new_tests=added,nonpasses=nonpasses))
save('managed-validation-v1.json',dict(Debug=dict(outcomes=dict(totals),new_passes=len(added),new_nonpasses=[],inherited_nonpasses=len(nonpasses),suites=suites,old_identity_occurrence_outcome_message_delta=0),
    selected_Release=dict(outcomes=dict(Counter(v for k,v in selected.items() if k[0]=='Release')),distinct_identities=sum(k[0]=='Release' for k in selected),full_suite='NOT_RUN_OWNER_PREFERENCE'),original_queries=0))

raw=PRIVATE/'full-managed-Debug-v1.json';data=json.loads(raw.read_bytes());changes=[]
def walk(value,path):
 if isinstance(value,dict):
  for key,child in value.items():
   if key=='test_name' and isinstance(child,str):
    label=safe_label(child)
    if label!=child:changes.append(dict(pointer=path+[key],original_label_sha256=sha(child.encode()),public_label_sha256=sha(label.encode()),public_label=label));value[key]=label
   else:walk(child,path+[key])
 elif isinstance(value,list):
  for i,child in enumerate(value):walk(child,path+[i])
walk(data,[]);assert len(changes)==2
save('full-managed-Debug-public-v1.json',data)
save('diagnostic-public-derivative-v1.json',dict(raw_local=pin(raw),public=pin(HERE/'full-managed-Debug-public-v1.json'),changes=changes,excluded_raw_retained_locally=True,scope='Only two synthetic display labels; every other source/input/output/identity/outcome/trace JSON field unchanged'))
source=(PREVIOUS/'verify_derivative_v3.py').read_text(encoding='utf-8').replace('diagnostic-public-derivative-v3.json','diagnostic-public-derivative-v1.json')
with (HERE/'verify_derivative_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
from verify_derivative_v1 import inspect
save('diagnostic-derivative-proof-v1.json',inspect())
print(json.dumps(dict(Debug=dict(totals),new_passes=len(added),inherited_nonpasses=len(nonpasses),selected_Release=dict(Counter(v for k,v in selected.items() if k[0]=='Release')),display_only_derivative_fields=len(changes))))
