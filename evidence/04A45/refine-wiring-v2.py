from common45 import *
import csv,collections
lock();raw={r['edge_id']:r for r in csv.DictReader((PRIVATE/'syntax-current-v3/ui-wiring.csv').open(encoding='utf-8-sig',newline=''))}
data=load(HERE/'scope/event-map-v1.json');compiled={c:load(HERE/('scope/compiled-wiring-'+c+'-v1.json')) for c in ['Debug','Release']}
resolved=[];unresolved=[]
for edge in data['edges']:
 if edge['classification']!='UNRESOLVED_WIRING':continue
 e=raw[edge['edge_id']];kind=e['kind']
 name=('add_' if kind=='AddHandler' else 'remove_')+e['event_or_control'].split('.')[-1] if kind in ['AddHandler','RemoveHandler'] else 'WireHelp' if e['event_or_control'].endswith('.WireHelp') else 'LogPostEstimation' if 'LogPostEstimation' in e['target'] else 'BeginInvoke' if 'BeginInvoke' in e['event_or_control'] else 'Invoke' if 'Invoke' in e['event_or_control'] else None
 proofs=[]
 for config,c in compiled.items():
  tokens=[dict(method=m['name'],method_signature=m['signature'],**t) for typ in c['types'] if typ['owner']==e['containing_type'] or typ['owner'].startswith(e['containing_type']+'+') for m in typ['methods'] for t in m['tokens'] if t['name']==name]
  proofs.append(dict(configuration=config,target=name,references=tokens))
 if name and all(p['references'] for p in proofs):
  edge.update(classification='WINDOWS_HOST_ONLY_NATIVE_PENDING',reason='Exact source-pinned event/delegate/extension expression has the expected compiled call target in both configurations. Parser-only ambiguity resolved as a host/delegate boundary;actual instance,event subscription and delivery are NOT_VERIFIED.',compiled_evidence=proofs,evidence='evidence/04A45/scope/compiled-wiring-Debug-v1.json;Release counterpart',next_gate='Exact independent approval and same-session loaded-code/native control capture;46/47 family exercise retained')
  resolved.append(edge['edge_id'])
 else:unresolved.append(dict(edge_id=edge['edge_id'],expected=name,proofs=proofs))
data.update(counts=dict(collections.Counter(e['classification'] for e in data['edges'])),parser_ambiguities=82,compiled_target_boundaries=resolved,remaining_unresolved=unresolved,scope_note='Compiled target identification is not actual runtime event binding or native GUI coverage. Every original control remains restricted.')
save('scope/event-map-v2.json',data)
print('82 parser ambiguities:',len(resolved),'compiled host/delegate targets;',len(unresolved),'unresolved',flush=True)
