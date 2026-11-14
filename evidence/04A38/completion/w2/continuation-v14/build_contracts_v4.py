"""Whole-body facts from actual source spans and recorded original/current CIL."""
from common import *
from proof_kernel_v8 import bind,definitions,closure,semantic,map_current
from source_views import body
from collections import defaultdict
import re
DRAFT='--draft' in sys.argv
OUTPUT=PRIVATE/'draft-proof-v1'
def emit(name,value):
 if not DRAFT:return save(name,value)
 p=OUTPUT/name;p.parent.mkdir(parents=True,exist_ok=True)
 with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False,allow_nan=False);f.write('\n')
old=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());hist=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members'];u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());orig=json.loads((W2.parent/'w1/original-universe.json').read_bytes());entry=json.loads((HERE/'entry-queue-receipt-v1.json').read_bytes())
mapping=map_current(old['members'],u['members']);reverse={n['symbol_id']:k for k,n in mapping.items()};members={n['symbol_id']:n for n in u['members']};prior={n['symbol_id']:n for n in old['members']};om={n['symbol_id']:n for n in orig['members']}
assert len(mapping)==len(u['members'])==726 and all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in old['members'] if not n['review_status'].startswith('OPEN'))
shapes={r['symbol_id']:r for r in json.loads((HERE/'declaration-shapes-v1.json').read_bytes())['rows']};ss={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
meta={cfg:json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json')).read_bytes()) for cfg in ['Debug','Release']};original={cfg:json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+cfg+'.json')).read_bytes()) for cfg in meta};before={cfg:json.loads((PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json')).read_bytes()) for cfg in meta}
defs={cfg:definitions(m) for cfg,m in meta.items()};od={cfg:definitions(m) for cfg,m in original.items()};bd={cfg:definitions(m) for cfg,m in before.items()}
returns={cfg:json.loads((HERE/('return-metadata-'+cfg+'-v1.json')).read_bytes())['methods'] for cfg in meta};oldreturns={cfg:json.loads((ROOT/('evidence/04A38/preedit/compiled-'+cfg+'.json')).read_bytes())['methods'] for cfg in meta}
hm=defaultdict(list)
for h in hist:
 for key in h['current_ids']:hm[mapping[key]['symbol_id']].append(h)
def original_binding(method,cfg):
 rows=[r for r in original[cfg]['methods'] if (r['declaring_type'],r['member_name'],r['parameter_types'])==(method['declaring_type'],method['member_name'],method['parameter_types'])];assert len(rows)==1;return rows[0]
def return_binding(method,cfg):
 rows=[r for r in returns[cfg] if (r['declaring_type'],r['member_name'],r['parameter_types'])==(method['declaring_type'],method['member_name'],method['parameter_types'])];assert len(rows)==1;return rows[0]
required={mapping[n['symbol_id']]['symbol_id'] for n in entry['required_native_parents']}
required.update(mapping[r['current_record']['symbol_id']]['symbol_id'] for r in entry['rows'])
life=entry['supplemental_lifecycle']['source'];required.add(mapping[life['symbol_id']]['symbol_id'])
parents=[n for n in u['members'] if n['symbol_id'] in required and n['parent_member'] is None and n['kind']!='PropertyStatement'];props=[n for n in u['members'] if n['symbol_id'] in required and n['kind']=='PropertyStatement'];assert len(props)==1 and props[0]['name']=='Alpha'
contracts=[];properties=[]
for n in parents+props:
 hs=hm[n['symbol_id']];assert len(hs)<=1,'Overlapping historical source identity';h=hs[0] if hs else None;a=om[h['git_original_id']] if h else None
 now,nv=body((ROOT/n['path']).read_bytes(),n);arch,av=body((ROOT/ss[a['path']]['path']).read_bytes(),a) if a else (None,None);compiled=[]
 for cfg,m in meta.items():
  if n['kind']=='PropertyStatement':
   owner='BESHStatNG.WorksheetFunctions.PropensityScoreUDFs+PsmUdfHandle';ms=[r for r in m['methods'] if r['declaring_type']==owner and r['member_name'] in ['get_Alpha','set_Alpha','.ctor']];assert len(ms)==3
   os=[original_binding(r,cfg) for r in ms];assert all(semantic(r)==semantic(o) for r,o in zip(ms,os))
   t=next(t for t in m['types'] if t['type']==owner);ot=next(t for t in original[cfg]['types'] if t['type']==owner);assert t==ot
   setters={r['method'] for r in ms if r['member_name']=='set_Alpha'};writes=[dict(caller=r['method'],instruction=i,target=x['operand'][7:]) for r in m['methods'] for i,x in enumerate(r['instructions']) if x['op'] in ['call','callvirt'] and x.get('operand') in {'METHOD:'+s for s in setters}]
   expected_writers={r['method'] for r in ms if r['member_name']=='.ctor'}|{r['method'] for r in m['methods'] if r['declaring_type']=='BESHStatNG.WorksheetFunctions.PropensityScoreUDFs' and r['member_name']=='PS_FIT'};assert len(writes)==2 and {w['caller'] for w in writes}==expected_writers
   compiled.append(dict(configuration=cfg,current_methods=ms,original_methods=os,actual_returns=[return_binding(r,cfg) for r in ms if r['member_name']!='.ctor'],current_type=t,original_type=ot,all_shipping_setter_callers=writes,transitive=closure(m,[r['method'] for r in ms]),return_authority='Original actual getter ldfld Double, setter Double parameter and constructor initializer CIL bind the auto-property ABI; no fabricated original reflection row.'))
  else:
   method=bind(n,m,shapes);ob=original_binding(method,cfg) if h else None;ret=return_binding(method,cfg);oldret=None
   if h:
    key=(re.sub(r'``[0-9]+(?=\()', '', re.sub(r' @ [^,]+, Version=[^,]+, Culture=[^,]+, PublicKeyToken=(?:[a-fA-F0-9]+|null)', '', ob['method'])) if None in ob['parameter_types'] else n['owner']+'.'+n['name']+'('+','.join(ob['parameter_types'])+')');found=[r for r in oldreturns[cfg] if r['method']==key];assert len(found)==1;oldret=found[0];assert oldret['return_type']==ret['return_type']
   compiled.append(dict(configuration=cfg,method=method['method'],complete_method=method,source_parameters=shapes[n['symbol_id']],return_declaration=ret,original_return=oldret,original_method=ob,transitive=closure(m,[method['method']])))
 row=dict(original_id=h['canonical_original_id'] if h else None,current_id=n['symbol_id'],current_record=n,original_record=a,current_source=pin(ROOT/n['path']),original_source=ss[a['path']] if a else None,current_whole_body=now,original_whole_body=arch,current_source_view=nv,original_source_view=av,compiled=compiled,new_support=prior[reverse[n['symbol_id']]]['review_status'].startswith('OPEN'),proof_status='SOURCE_COMPILED_FACTS_BEHAVIOR_AND_FRAGMENT_PROOF_REQUIRED',current_fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']],original_fragments=[f for f in orig['fragments'] if a and f['member_id']==a['symbol_id']],original_queries=0)
 (properties if n['kind']=='PropertyStatement' else contracts).append(row)
callbacks=[];pending=[n for n in u['members'] if n['parent_member'] in {c['current_id'] for c in contracts}];callback_methods={cfg:{} for cfg in meta}
while pending:
 n=pending.pop(0);parent=members[n['parent_member']];siblings=sorted([s for s in u['members'] if s['parent_member']==parent['symbol_id']],key=lambda s:s['span_start']);ix=siblings.index(n);bindings=[]
 for cfg,m in meta.items():
  pkey=bind(parent,m,shapes)['method'] if parent['parent_member'] is None else callback_methods[cfg][parent['symbol_id']]
  targets=[i['operand'][7:] for i in defs[cfg][pkey]['instructions'] if i['op']=='ldftn' and i['operand'][7:] in defs[cfg] and defs[cfg][i['operand'][7:]]['member_name'].startswith('_Lambda$__')];assert len(siblings)==len(targets)
  target=defs[cfg][targets[ix]];assert target['method'] in bd[cfg] and semantic(target)==semantic(bd[cfg][target['method']]);owner=next(t for t in m['types'] if t['type']==target['declaring_type']);ctors=[r for r in m['methods'] if r['declaring_type']==owner['type'] and r['member_name'] in ['.ctor','.cctor']]
  callback_methods[cfg][n['symbol_id']]=target['method'];bindings.append(dict(configuration=cfg,parent_method=pkey,target=target['method'],complete_method=target,prior_complete_method=bd[cfg][target['method']],return_declaration=return_binding(target,cfg),declaring_type=owner,constructors=ctors,source_sibling_index=ix))
 text,v=body((ROOT/n['path']).read_bytes(),n);assert n['body_sha256']==prior[reverse[n['symbol_id']]]['body_sha256'];assert not prior[reverse[n['symbol_id']]]['review_status'].startswith('OPEN'),'New callback needs separate historical proof'
 callbacks.append(dict(current_id=n['symbol_id'],parent_id=parent['symbol_id'],current_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=v,new_support=False,configurations=bindings,prior_current_id=reverse[n['symbol_id']],prior_authority='../continuation-v13/current-ownership-v2.json'))
 pending.extend(s for s in u['members'] if s['parent_member']==n['symbol_id'])
roots={cfg:[c['compiled'][ix]['method'] for c in contracts]+[m['method'] for p in properties for m in p['compiled'][ix]['current_methods']] for ix,cfg in enumerate(meta)};graphs={cfg:closure(m,roots[cfg]) for cfg,m in meta.items()}
pu=json.loads((HERE/'portable-universe-v1.json').read_bytes());portable=[]
for n in pu['members']:
 if n['parent_member'] is not None or n['kind'] not in ['FunctionBlock','SubBlock','ConstructorBlock']:continue
 method=bind(n,meta['Debug'],shapes)
 if method['method'] not in graphs['Debug']['methods']:continue
 text,v=body((ROOT/n['path']).read_bytes(),n);bindings=[]
 for cfg,m in meta.items():
  method=bind(n,m,shapes);context=next(t for t in m['types'] if t['type']==n['owner']);assert not context['fields'];bindings.append(dict(configuration=cfg,method=method,context=context,return_declaration=return_binding(method,cfg) if n['name']!='.ctor' else None))
 portable.append(dict(source_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=v,parameters=shapes[n['symbol_id']],fragments=[f for f in pu['fragments'] if f['member_id']==n['symbol_id']],compiled=bindings))
enum=next(s for s in pu['supports'] if s['kind']=='EnumBlock' and 'PsmUdfTableKind38' in s['declaration'])
new=[c for c in contracts+properties if c['new_support']];assert len(new)==35 and all(c['original_id'] for c in new)
emit('whole-body-contracts-v1.json',dict(contracts=contracts,original_queries=0));emit('property-body-contracts-v1.json',dict(contracts=properties,original_queries=0));emit('callback-proofs-v1.json',dict(rows=callbacks,new_declarations=0,original_queries=0));emit('portable-api-contracts-v1.json',dict(contracts=portable,new_public_apis=7,enum_declaration=enum,original_queries=0))
emit('cohort-diagnostic-v1.json',dict(roots=roots,graphs=graphs,reviewed_candidate_ids=[c['current_id'] for c in contracts+properties],new_historical_ids=[c['original_id'] for c in new],new_current_ids=[c['current_id'] for c in new],original_queries=0))
emit('current-semantic-position-crosswalk-v1.json',dict(rows=[dict(before=k,after=n['symbol_id'],body_bytes_equal=prior[k]['body_sha256']==n['body_sha256']) for k,n in mapping.items()],entries=726,original_queries=0))
emit('proof-inputs-v1.json',dict(metadata_prefix='metadata-all/packet-v1-context-v5/',roots=roots,new_historical_ids=[c['original_id'] for c in new],new_current_supported=[c['current_id'] for c in new],original_queries=0))
print(json.dumps(dict(draft=DRAFT,parents=len(contracts),properties=len(properties),callbacks=len(callbacks),portable=len(portable),new=35,reached={cfg:len(g['methods']) for cfg,g in graphs.items()})))
