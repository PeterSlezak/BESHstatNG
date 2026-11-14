"""Full native/property/callback/portable/adjacent contracts, current Debug only."""
from proof_support_v16 import *
from collections import Counter
OUT=HERE
configure_native()
before=load(PREVIOUS/'current-ownership-v3.json');history=load(PREVIOUS/'historical-ownership-v2.json')['members']
u=load(HERE/'final-v4/current-universe.json');o=load(W2.parent/'w1/original-universe.json');entry=load(HERE/'entry-queue-receipt-v1.json')
mapping=map_current(before['members'],u['members']);reverse={n['symbol_id']:k for k,n in mapping.items()}
members={n['symbol_id']:n for n in u['members']};oldmembers={n['symbol_id']:n for n in before['members']};originals={n['symbol_id']:n for n in o['members']}
hm={key:h for h in history for key in h['current_ids']};hbyoriginal={h['git_original_id']:h for h in history}
snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
shapes={r['symbol_id']:r for r in load(HERE/'final-v4/declaration-shapes.json')['rows']}
meta=load(HERE/'final-v5/current-Debug.json');original=load(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json');prior=load(PREVIOUS/'final-v1/current-Debug.json')
defs=definitions(meta);od=definitions(original);pd=definitions(prior)
returns=load(ROOT/'evidence/04A38/preedit/compiled-Debug.json')['methods']
assert len(mapping)==726 and all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in before['members'] if not n['review_status'].startswith('OPEN'))

def old_method(current):
 matches=[m for m in original['methods'] if (m['declaring_type'],m['member_name'],m['parameter_types'])==(current['declaring_type'],current['member_name'],current['parameter_types'])]
 assert len(matches)==1,'Missing exact original method '+current['method'];return matches[0]
def old_return(method):
 candidates=[r for r in returns if (r['declaring_type'],r['name'])==(method['declaring_type'],method['member_name'])]
 if len(candidates)>1:candidates=[r for r in candidates if r['method'].endswith('('+','.join(method['parameter_types'])+')')]
 assert len(candidates)==1,'Missing exact original measured return '+method['method'];return candidates[0]

required={mapping[r['source']['symbol_id']]['symbol_id'] for r in entry['required_native_parents']}
required.update(mapping[r['current']['record']['symbol_id']]['symbol_id'] for r in entry['rows'] if r['ordinal'] in [38,39])
roots=[];oroots=[]
for row in entry['rows']:
 if row['ordinal']==7:
  roots.append(row['compiled_Debug']['current']['method']);oroots.append(row['compiled_Debug']['original']['method'])
 else:
  n=mapping[row['current']['record']['symbol_id']];method=native_bind(n,meta,shapes,members)
  roots.append(method['method']);oroots.append(row['compiled_Debug']['original']['method'])
graph=closure(meta,roots);oldgraph=closure(original,oroots)
reachable=set(graph['methods']);oldreachable=set(oldgraph['methods'])
callers={}
for edge in graph['edges']:callers.setdefault(edge['target'],[]).append(dict(caller=edge['caller'],instruction=edge['instruction'],op=edge['op']))
portable_index={}
for method in meta['methods']:
 if method['method'] in reachable and method['declaring_type']:portable_index.setdefault((method['declaring_type'].split('`',1)[0],method['member_name']),[]).append(method)
# Source-grounded original-only methods retain their existing proven identities.
original_only=[]
for n in o['members']:
 if n['parent_member'] or n['kind'] not in ['FunctionBlock','SubBlock']:continue
 reached=[m for m in original['methods'] if (m['declaring_type'],m['member_name'])==(n['owner'],n['name']) and m['method'] in oldreachable]
 if not reached:continue
 h=hbyoriginal[n['symbol_id']]
 for key in h['current_ids']:
  current=mapping[key]
  if current['symbol_id'] not in required:
   assert bool(h['proof'])== (not oldmembers[key]['review_status'].startswith('OPEN')),'Original/current retained disposition mismatch' 
   required.add(current['symbol_id']);original_only.append(dict(original_id=h['canonical_original_id'],original_record=n,current_record=current,original_compiled=reached,prior_proof=h['proof'],classification='RETAINED' if h['proof'] else 'NEW_PROPOSAL_REQUIRING_ENTIRE_BODY_SOURCE_COMPOSITE'))
contracts=[]
for n in u['members']:
 if n['symbol_id'] not in required or n['kind'] not in ['FunctionBlock','SubBlock','ConstructorBlock','PropertyBlock','GetAccessorBlock']:continue
 key=reverse[n['symbol_id']];h=hm.get(key);a=originals[h['git_original_id']] if h else None
 text,source_view=body((ROOT/n['path']).read_bytes(),n)
 oldtext,oldview=body((ROOT/snapshots[a['path']]['path']).read_bytes(),a) if a else (None,None)
 method=native_bind(n,meta,shapes,members);oldm=old_method(method) if a else None;measured=old_return(oldm) if oldm else None
 if measured:assert method['signature']['return_type'].split(' @ ',1)[0]==measured['return_type']
 reached=[m for m in meta['methods'] if m['declaring_type']==method['declaring_type'] and m['member_name']==method['member_name'] and m['method'] in reachable]
 contracts.append(dict(original_id=h['canonical_original_id'] if h else None,current_id=n['symbol_id'],current_record=n,original_record=a,current_source=pin(ROOT/n['path']),original_source=snapshots[a['path']] if a else None,current_whole_body=text,original_whole_body=oldtext,current_source_view=source_view,original_source_view=oldview,compiled_Debug=dict(complete_method=method,measured_signature=method['signature'],original_method=oldm,original_return=measured,actual_reached_constructions=reached,transitive=closure(meta,[m['method'] for m in reached]) if reached else dict(methods=[],edges=[],external_runtime_or_ExcelDna=[])),new_support=oldmembers[key]['review_status'].startswith('OPEN'),current_fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']],original_fragments=[f for f in o['fragments'] if a and f['member_id']==a['symbol_id']],Release=DEFERRED,original_queries=0))

callbacks=[];callback_methods={}
pending=[n for n in u['members'] if n['parent_member'] in {r['current_id'] for r in contracts} and n['kind'].endswith('LambdaExpression')]
def targets(meta,parent):
 d=definitions(meta)
 return [d[x['operand'][7:]] for x in parent['instructions'] if x['op']=='ldftn' and isinstance(x['operand'],str) and x['operand'][7:] in d and d[x['operand'][7:]]['member_name'].startswith('_Lambda$__')]
while pending:
 n=pending.pop(0);parent=members[n['parent_member']]
 siblings=sorted([s for s in u['members'] if s['parent_member']==parent['symbol_id'] and s['kind'].endswith('LambdaExpression')],key=lambda n:n['span_start']);ordinal=siblings.index(n)
 parentmethod=native_bind(parent,meta,shapes,members) if parent['parent_member'] is None else callback_methods[parent['symbol_id']]
 current_targets=targets(meta,parentmethod);assert len(current_targets)==len(siblings),'Actual callback/ldftn cardinality differs'
 method=current_targets[ordinal];callback_methods[n['symbol_id']]=method
 assert instructions(method)==instructions(pd[method['method']]),'Existing callback CIL changed'
 key=reverse[n['symbol_id']];h=hm.get(key);a=originals[h['git_original_id']] if h else None
 oldm=None;oldtext=None;oldview=None;source=None
 if a:
  oldparent=originals[a['parent_member']];native_parent=next(c['current_record'] for c in contracts if c['original_record'] and c['original_record']['symbol_id']==oldparent['symbol_id']) if oldparent['parent_member'] is None else None
  op=native_bind(oldparent,original,shapes,originals,native_parent) if native_parent else old_method(parentmethod)
  oldsiblings=sorted([s for s in o['members'] if s['parent_member']==oldparent['symbol_id'] and s['kind'].endswith('LambdaExpression')],key=lambda n:n['span_start'])
  ot=targets(original,op);assert len(ot)==len(oldsiblings);oldm=ot[oldsiblings.index(a)]
  assert instructions(method)==instructions(oldm),'Original callback executable body differs'
  oldtext,oldview=body((ROOT/snapshots[a['path']]['path']).read_bytes(),a);source=snapshots[a['path']]
 text,sv=body((ROOT/n['path']).read_bytes(),n)
 if a:assert text==oldtext
 elif oldmembers[key]['review_status'].startswith('OPEN'):
  assert parent['name']=='AverageGeeFittedDesignRowForLsmProfile' and text in ['Function(profile) profile.ColumnIndex','Function(profile) profile.Value']
 callbacks.append(dict(original_id=h['canonical_original_id'] if h else None,current_id=n['symbol_id'],parent_id=parent['symbol_id'],current_record=n,original_record=a,source=pin(ROOT/n['path']),original_source=source,current_whole_body=text,original_whole_body=oldtext,current_source_view=sv,original_source_view=oldview,source_sibling_ordinal=ordinal,compiled_Debug=dict(parent_method=parentmethod,complete_method=method,original_method=oldm,prior_complete_method=pd[method['method']],signature=method['signature']),new_support=oldmembers[key]['review_status'].startswith('OPEN'),current_only_reason=None if a else 'AUTHENTIC_PREEXISTING_TYPED_ADAPTER_SELECTOR;NO_FABRICATED_HISTORICAL_ID',Release=DEFERRED,original_queries=0))
 pending.extend(s for s in u['members'] if s['parent_member']==n['symbol_id'] and s['kind'].endswith('LambdaExpression'))

special=entry['rows'][6];a=special['original_record'];currentmethod=defs[special['compiled_Debug']['current']['method']];oldm=special['compiled_Debug']['original']
assert instructions(currentmethod)==instructions(oldm)
assert currentmethod['signature']['return_type'].split(' @ ',1)[0]=='System.Boolean' and currentmethod['parameter_types']==oldm['parameter_types']==['System.Char']
lambda_proof=dict(original_id=special['historical_id'],original_record=a,original_source=special['original_source'],original_whole_body=special['complete_original_body'],current_ids=[],current_portable_record=special['current']['record'],current_portable_parent=special['current']['parent'],current_portable_source=special['current']['source'],current_whole_body=special['current']['complete_body'],compiled_Debug=dict(original_method=oldm,current_method=currentmethod,original_typed_parent=special['current']['original_parent'],current_typed_parent=special['current']['current_parent'],original_return_authority='Exact original Func(Char,Boolean) ldftn/constructor and original callback CIL; no invented reflection row or original execution'),new_support=True,Release=DEFERRED,original_queries=0)

common=load(HERE/'final-v4/dependency-full-common.json');portable=[]
for n in common['members']:
 if n['parent_member'] is not None or n['kind'] not in ['FunctionBlock','SubBlock','ConstructorBlock']:continue
 name='.ctor' if n['kind']=='ConstructorBlock' else n['name']
 if (n['owner'],name) not in portable_index:continue
 actual=portable_bindings(n,meta,shapes,graph);text,sv=body((ROOT/n['path']).read_bytes(),n)
 portable.append(dict(source_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=sv,parameters=shapes[n['symbol_id']],compiled_Debug=[dict(complete_method=m,signature=m['signature'],actual_callers=callers.get(m['method'],[])) for m in actual],fragments=[f for f in common['fragments'] if f['member_id']==n['symbol_id']],Release=DEFERRED))

changed=[dict(method=k,original=pd[k],current=defs[k]) for k in sorted(pd.keys()&defs.keys()) if cil(pd[k])!=cil(defs[k])]
added=[defs[k] for k in sorted(defs.keys()-pd.keys())];removed=sorted(pd.keys()-defs.keys())
assert not removed,'Existing compiled method disappeared from expanded census'
core_changed=[r for r in changed if r['method'].split(' @ ',1)[1].split(',',1)[0]=='BESHStat.Core'];assert not core_changed
existing_portable=[r for r in changed if r['method'].split(' @ ',1)[1].split(',',1)[0]=='BESHStat.Excel.Common'];assert not existing_portable
adjacent=load(HERE/'final-v4/dependency-full-adjacent.json');adjbefore=load(HERE/'adjacent-LMM-source-before-census-v1.json');adjrows=[]
for n in adjacent['members']:
 if n['parent_member'] or n['kind'] not in ['FunctionBlock','SubBlock']:continue
 ms=[m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(n['owner'],n['name']) and (m['method'] in reachable or n['name']=='DefaultNames')]
 if not ms:continue
 assert len(ms)==1;oldm=old_method(ms[0]);oldn=next(r for r in adjbefore['members'] if r['signature_key']==n['signature_key'])
 text,sv=body((ROOT/n['path']).read_bytes(),n);oldtext,ov=body((ROOT/load(HERE/'adjacent-LMM-helper-authority-v1.json')['source']['preimage']['path']).read_bytes(),oldn)
 adjrows.append(dict(source_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=sv,before_record=oldn,before_whole_body=oldtext,original_Debug_method=oldm,current_Debug_method=ms[0],source_authority=pin(HERE/'adjacent-LMM-helper-authority-v1.json'),scope='NECESSARY_OUTSIDE_CANONICAL_UNIVERSE_DEPENDENCY;NOT_A_RENUMBERED_704_OR_726_MEMBER',Release=DEFERRED,original_queries=0))
contexts=[]
owners={defs[k]['declaring_type'] for k in graph['methods']}
for t in meta['types']:
 if t['type'] in owners:contexts.append(dict(type=t,constructors_and_initializers=[m for m in meta['methods'] if m['declaring_type']==t['type'] and m['member_name'] in ['.ctor','.cctor']]))
outputs={
 'whole-body-contracts-v1.json':dict(contracts=contracts,original_queries=0),
 'callback-proofs-v1.json':dict(rows=callbacks,original_queries=0),
 'historical-only-callback-proof-v1.json':dict(rows=[lambda_proof],original_queries=0),
 'portable-api-contracts-v1.json':dict(contracts=portable,original_queries=0),
 'adjacent-dependency-contracts-v1.json':dict(contracts=adjrows,original_queries=0),
 'constructed-owner-proofs-v1.json':dict(Debug=contexts,Release=DEFERRED,original_queries=0),
 'compiled-method-deltas-v2.json':dict(Debug=dict(changed=changed,added=added,removed=removed,Core_changed=0,existing_Common_changed=0,census_extension=pin(HERE/'final-Debug-metadata-domain-v5.json')),Release=DEFERRED,original_queries=0),
 'current-semantic-position-crosswalk-v1.json':dict(rows=[dict(before=k,after=n['symbol_id'],body_bytes_equal=oldmembers[k]['body_sha256']==n['body_sha256']) for k,n in mapping.items()],entries=726,original_queries=0),
 'original-only-dependencies-v1.json':dict(rows=original_only,original_queries=0),
 'cohort-diagnostic-v1.json':dict(Debug_roots=roots,Debug_graph=graph,original_Debug_graph=oldgraph,reviewed_current_ids=[r['current_id'] for r in contracts+callbacks],proposed_historical_ids=[r['original_id'] for r in contracts+callbacks if r['new_support'] and r['original_id']]+[lambda_proof['original_id']],proposed_current_only=[r['current_id'] for r in callbacks if r['new_support'] and r['original_id'] is None],Release=DEFERRED,original_queries=0),
 'complete-compiled-dependency-catalog-v1.json':dict(Debug_graph=graph,current_methods=[defs[k] for k in graph['methods']],current_metadata=pin(HERE/'final-v5/current-Debug.json'),Release=DEFERRED,original_queries=0),
 'complete-original-compiled-dependency-catalog-v1.json':dict(Debug_graph=oldgraph,original_methods=[od[k] for k in oldgraph['methods']],original_metadata=pin(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json'),Release='IMMUTABLE_HISTORICAL_BYTES_RETAINED;NO_V16_RELEASE_INSPECTION',original_queries=0),
}
for name,doc in outputs.items():write_json(OUT,name,doc)
print('Complete Debug drafts',len(contracts),'native/property contracts',len(callbacks),'callbacks',len(portable),'portable APIs',len(adjrows),'adjacent helpers',len(graph['methods']),'actual transitive methods',flush=True)
