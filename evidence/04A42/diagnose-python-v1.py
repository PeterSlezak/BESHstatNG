from common42 import *
import ast,re
d=load(HERE/'python/parent-event-reconciliation-v1.json');pairs={r['stable_event_id']:r for r in load(PRIVATE/'python-event-pairs-v1.json')['events']}
def facts(message):
    first=message.splitlines()[0]
    try:
        args=[ast.literal_eval(v) for v in first.split(' != ',1)]
        return [v if isinstance(v,str) and (re.fullmatch(r'[0-9a-f]{40,64}',v) or v.startswith('codex/')) else dict(type=type(v).__name__,sha256=sha(repr(v).encode())) for v in args]
    except (ValueError,SyntaxError):return dict(first_line_sha256=sha(first.encode()),lines=len(message.splitlines()))
for r in d['events']:
    if r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW':
        p=pairs[r['stable_event_id']];print(r['identity'],facts(p['before']['message']),facts(p['current']['message']),flush=True)
