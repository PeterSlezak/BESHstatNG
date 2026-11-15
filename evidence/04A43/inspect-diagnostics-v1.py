from common43 import *
import ast
d=load(HERE/'python/parent-event-reconciliation-v1.json');pairs={r['stable_event_id']:r for r in load(PRIVATE/'python-event-pairs-v1.json')['events']}
for r in d['events']:
 if r['disposition']!='UNRESOLVED_DIAGNOSTIC_REVIEW':continue
 p=pairs[r['stable_event_id']];a=p['before']['message'];b=p['current']['message']
 print(r['identity'])
 if 'publication' in r['identity']:
  aa=ast.literal_eval(a.strip());bb=ast.literal_eval(b.strip());print('oldset',sorted(aa));print('newset',sorted(bb))
 else:
  print(a.splitlines()[0]);print(b.splitlines()[0])
