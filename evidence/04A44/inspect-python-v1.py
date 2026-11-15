from common44 import *
import ast
d=load(HERE/'python/parent-event-reconciliation-v1.json');pairs={r['stable_event_id']:r for r in load(PRIVATE/'python-event-pairs-v1.json')['events']}
for r in d['events']:
 if r['disposition']!='UNRESOLVED_DIAGNOSTIC_REVIEW':continue
 pair=pairs[r['stable_event_id']];print(r['identity'],r['classification'])
 for stage in ['before','current']:
  if pair[stage] is not None:
   value=pair[stage]['message'];first=value.splitlines()[0]
   # Source/branch/hash/set assertions only. Do not print a privacy match or trace.
   if 'authoring_successor' in r['identity']:print(stage,first)
   elif 'publication_' in r['identity']:print(stage,sorted(ast.literal_eval(value)))
   else:print(stage,first)
