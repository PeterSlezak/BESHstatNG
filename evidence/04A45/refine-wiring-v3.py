from common45 import *
source=(HERE/'refine-wiring-v2.py').read_text().replace("+'-v1.json'","+'-v2.json'").replace("'scope/event-map-v2.json'","'scope/event-map-v3.json'")
needle="  proofs.append(dict(configuration=config,target=name,references=tokens))"
replacement="""  if not tokens and kind in ['AddHandler','RemoveHandler']:
   boundary='AddEventHandler' if kind=='AddHandler' else 'RemoveEventHandler'
   event_name=e['event_or_control'].split('.')[-1]
   for typ in c['types']:
    if typ['owner']!=e['containing_type'] and not typ['owner'].startswith(e['containing_type']+'+'):continue
    for m in typ['methods']:
     if any(t['owner']=='DECLARED_EVENT_NAME' and t['name']==event_name for t in m['tokens']):
      tokens.extend(dict(method=m['name'],method_signature=m['signature'],**t) for t in m['tokens'] if (t['name']==boundary and 'ComAwareEventInfo' in t['owner']) or (t['owner']=='DECLARED_EVENT_NAME' and t['name']==event_name))
   if not any(t['name']==boundary for t in tokens):tokens=[]
  proofs.append(dict(configuration=config,target=name,references=tokens))"""
assert needle in source;source=source.replace(needle,replacement)
exec(compile(source,str(HERE/'refine-wiring-v2.py'),'exec'))
