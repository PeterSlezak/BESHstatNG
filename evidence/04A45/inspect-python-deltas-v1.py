from common45 import *
import ast,re
d=load(HERE/'python/parent-event-reconciliation-v1.json');pairs={p['stable_event_id']:p for p in load(PRIVATE/'python-event-pairs-v1.json')['events']}
for r in d['events']:
 if r['disposition']!='UNRESOLVED_DIAGNOSTIC_REVIEW':continue
 pair=pairs[r['stable_event_id']];a=pair['before']['message'];b=pair['current']['message']
 def parts(s):
  try:return [ast.literal_eval(v) for v in s.splitlines()[0].split(' != ',1)]
  except (ValueError,SyntaxError):return None
 x=parts(a);y=parts(b)
 def display(v):
  if isinstance(v,str) and (re.fullmatch('[a-f0-9]{40}|[a-f0-9]{64}',v) or v.startswith('codex/')):return v
  if isinstance(v,(set,list)):return dict(type=type(v).__name__,length=len(v),relative_source_items=sorted(v) if all(isinstance(s,str) and not re.search('[A-Za-z]:',s) for s in v) else None)
  return dict(type=type(v).__name__)
 print(json.dumps(dict(identity=r['identity'],source=r['source']['source']['path'],before=[display(v) for v in x] if x else 'NON_LITERAL',current=[display(v) for v in y] if y else 'NON_LITERAL')),flush=True)
