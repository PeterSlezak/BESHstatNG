"""Support mapping is independent of contained-body closure. Never hides OPEN parents."""
import re
from common import *
from verify_host_cache_v2 import stable
u=json.loads((HERE/'current-universe-b1-v1.json').read_bytes());configs={}
for c in ['Debug','Release']:
 configs[c]=[json.loads((HERE/'metadata/b1-v1'/(r+'-'+c+'.json')).read_bytes()) for r in ['original','current']]
cache=json.loads((HERE/'host-cache-proof.json').read_bytes());closed_initializers={r['current_id'] for r in cache['initializers']};rows=[]
for s in u['supports']:
 r=dict(s);kind=s['kind'];owner=s['owner'].rstrip('.');declaration=s['declaration']
 if kind=='ModuleBlock':
  name=re.search(r'\bModule (\w+)',declaration)[1];clr=owner+'.'+name
  assert all(any(t['type']==clr for t in pair[1]['types']) for pair in configs.values())
  children=[m['symbol_id'] for m in u['members'] if m['path']==s['path'] and m['span_start']>=s['span_start'] and m['span_start']+m['span_length']<=s['span_start']+s['span_length']]
  r.update(disposition='DECLARATION_CONTEXT_MAPPED_CHILDREN_NOT_CLOSED',clr_owner=clr,children=children,fully_reconciled=False,reason='Exact CLR owner/partial mapping, every contained current identity enumerated separately; module declaration does not close child behavior')
 elif kind=='PropertyStatement':
  owner=next(t['type'] for t in configs['Debug'][1]['types'] if t['type'].replace('+','.')==owner)
  name=re.search(r'\bProperty (\w+)',declaration)[1];proof=[];good=True
  for c,(o,n) in configs.items():
   om={m['method']:m for m in o['methods']};nm={m['method']:m for m in n['methods']};ot=next(t for t in o['types'] if t['type']==owner);nt=next(t for t in n['types'] if t['type']==owner)
   access=[m for m in om.values() if m['declaring_type']==owner and m['member_name'] in ['get_'+name,'set_'+name]];ctors=[m for m in om.values() if m['declaring_type']==owner and m['member_name'] in ['.ctor','.cctor']]
   exact=bool(access) and all(m['method'] in nm and stable(m)==stable(nm[m['method']]) for m in access+ctors) and ot==nt
   good &=exact;proof.append(dict(configuration=c,accessors=[m['method'] for m in access],constructors=[m['method'] for m in ctors],entire_accessor_constructor_and_fields_exact=exact))
  assert good,s['support_id']
  r.update(disposition='NONBEHAVIORAL_SUPPORT_WITH_PROOF',fully_reconciled=True,proof=proof,reason='Auto-property backing field/accessor/constructor/base/metadata exact in both configurations; native cache/session DTO or parent-owned typed import storage. Does not close computed N or fitting/importing parents.')
 elif kind=='FieldDeclaration':
  aliases=[m['symbol_id'] for m in u['members'] if m['path']==s['path'] and m['span_start']>=s['span_start'] and m['span_start']+m['span_length']<=s['span_start']+s['span_length']]
  exact=bool(aliases) and all(m in closed_initializers for m in aliases)
  r.update(disposition='PRIMARY_INITIALIZER_ALIAS_PROVED' if exact else 'OPEN_SUPPORT_BOUNDARY_REVIEW_REQUIRED',aliases=aliases,fully_reconciled=exact,reason='FieldDeclaration aliases primary VariableDeclarator initializer; primary source/full-cctor/literal proof counted once')
 else:
  r.update(disposition='OPEN_SUPPORT_BOUNDARY_REVIEW_REQUIRED',fully_reconciled=False,reason='Class alias/base-constructor and full parent responsibility review remains explicit; no blanket native DTO closure')
 rows.append(r)
save('support-reconciliation.json',dict(raw=len(rows),distinct=sum(not r['already_in_primary_population'] for r in rows),raw_fully_reconciled=sum(r['fully_reconciled'] for r in rows),distinct_fully_reconciled=sum(r['fully_reconciled'] and not r['already_in_primary_population'] for r in rows),rows=rows,all_parent_bodies_closed=False))
