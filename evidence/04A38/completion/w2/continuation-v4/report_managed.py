from collections import Counter
from common import *
from baseline_report import index
from validation_report import safe_label
old=json.loads((PREVIOUS/'managed-Debug-identities.json').read_bytes())['tests'];totals=Counter();tests=[];nonpasses=[];added=[];suites=[]
for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
 data=json.loads((RUNROOT/'executions'/('w2c4-validation-v1-Debug-serial-'+suite)/'cases.json').read_bytes());now=index(data['records']);prev={(r['identity_sha256'],r['occurrence']):r for r in old if r['suite']==suite}
 assert all(k in now and now[k]['outcome']==r['outcome'] for k,r in prev.items()),'Inherited identity/outcome drift '+suite
 totals.update(data['outcomes']);suites.append(dict(suite=suite,outcomes=data['outcomes'],exit_code=data['command']['exit_code'],trx_sha256=data['sha256']))
 for k,r in now.items():
  item=dict(suite=suite,identity_sha256=k[0],occurrence=k[1],class_name=r['class_name'],method=r['method'],test_name=safe_label(r['test_name']),outcome=r['outcome'],population='NEW_CONTINUATION_V3' if k not in prev else 'INHERITED_C2')
  tests.append(item)
  if k not in prev:added.append(item)
  if r['outcome']!='Passed':nonpasses.append(dict(item,message_sha256=sha(r['message'].encode())))
selected={}
for config in ['Debug','Release']:
 focus=json.loads((HERE/('focused-parametric-v1-'+config+'.json')).read_bytes());core=json.loads((HERE/'selected-Core-parametric-v1.json').read_bytes())
 for r in [r for r in focus['results'] if r['kind']=='TEST']+[r for r in core['results'] if r['kind']=='TEST' and r['configuration']==config]:
  for k,row in index(r['tests']['records']).items():selected[(config,r['suite'],*k)]=row['outcome']
save('managed-Debug-identities.json',dict(tests=tests,new_tests=added,nonpasses=nonpasses))
save('managed-validation.json',dict(Debug=dict(outcomes=dict(totals),new_passes=sum(r['outcome']=='Passed' for r in added),new_nonpasses=[r for r in added if r['outcome']!='Passed'],inherited_nonpasses=len(nonpasses),suites=suites,old_identity_occurrence_outcome_delta=0),selected_Release=dict(outcomes=dict(Counter(v for k,v in selected.items() if k[0]=='Release')),distinct_identities=sum(k[0]=='Release' for k in selected),full_suite='NOT_RUN_OWNER_PREFERENCE'),source_pin_changes='Authorized production/test source transitions retain old expected assertions; no historical managed test renamed/removed',original_queries=0))
print(json.dumps(dict(Debug=dict(totals),new_passes=len(added),inherited_nonpasses=len(nonpasses),selected_Release=dict(Counter(v for k,v in selected.items() if k[0]=='Release')))))
