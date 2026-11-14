"""Two distinct original-only callbacks; source and existing captured Debug IL only."""
from proof_support_v16 import *
o=load(W2.parent/'w1/original-universe.json');common=load(HERE/'final-v4/dependency-full-common.json')
old=load(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json');meta=load(HERE/'final-v5/current-Debug.json')
od=definitions(old);md=definitions(meta)
history=load(HERE/'historical-ownership-v1.json')['members'];hm={h['git_original_id']:h for h in history}
contracts=load(HERE/'whole-body-contracts-v1.json')['contracts'];portable=load(HERE/'portable-api-contracts-v1.json')['contracts']
snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
def target(parent,methods):
 sites=[(i,x) for i,x in enumerate(parent['instructions']) if x['op']=='ldftn' and x['operand'][7:] in methods]
 assert len(sites)==1
 i,x=sites[0];method=methods[x['operand'][7:]]
 construction=parent['instructions'][i:i+2]
 assert construction[1]['op']=='newobj' and 'System.Func`2[[System.Char,' in construction[1]['operand'] and '[System.Boolean,' in construction[1]['operand']
 return method,dict(index=i,instructions=construction)
rows=[]
for native,portable_owner,name in [('BESHStatNG.UdfOutputTables','BESHStatNG.ExcelCommon.UdfOutputTablesOptions38','NormalizeResultTableTitle'),('BESHStatNG.UdfScalarUtilities','BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38','NormalizeMixedModelOptionToken')]:
 parent=next(c for c in contracts if c['original_record'] and c['original_record']['owner']==native and c['current_record']['name']==name)
 children=[n for n in o['members'] if n['parent_member']==parent['original_record']['symbol_id'] and n['kind'].endswith('LambdaExpression')];assert len(children)==1
 a=children[0];h=hm[a['symbol_id']];assert not h['proof'] and h['current_ids']==[]
 p=next(c for c in portable if c['source_record']['owner']==portable_owner and c['source_record']['name']==name)
 children=[n for n in common['members'] if n['parent_member']==p['source_record']['symbol_id'] and n['kind'].endswith('LambdaExpression')];assert len(children)==1
 n=children[0];original_text,ov=body((ROOT/snapshots[a['path']]['path']).read_bytes(),a);current_text,cv=body((ROOT/n['path']).read_bytes(),n)
 assert tokens(original_text)==tokens(current_text)
 original_parent=parent['compiled_Debug']['original_method'];current_parent=p['compiled_Debug'][0]['complete_method']
 original_method,original_binding=target(original_parent,od);current_method,current_binding=target(current_parent,md)
 assert instructions(original_method)==instructions(current_method)
 assert current_method['parameter_types']==original_method['parameter_types']==['System.Char']
 assert current_method['signature']['return_type'].split(' @ ',1)[0]=='System.Boolean'
 assert current_parent['method'] in load(HERE/'cohort-diagnostic-v1.json')['Debug_graph']['methods']
 rows.append(dict(original_id=h['canonical_original_id'],original_record=a,original_parent=parent['original_record'],original_source=snapshots[a['path']],original_whole_body=original_text,original_view=ov,current_ids=[],current_portable_record=n,current_portable_parent=p['source_record'],current_portable_source=pin(ROOT/n['path']),current_whole_body=current_text,current_view=cv,compiled_Debug=dict(original_method=original_method,current_method=current_method,original_typed_parent=original_parent,current_typed_parent=current_parent,original_binding=original_binding,current_binding=current_binding,original_return_authority='Already captured actual Func(Char,Boolean) construction; no invented original measured-return row'),new_support=True,classification='EXACT_SOURCE_ID_DISTINCT_OWNER_AND_SPAN;ORIGINAL_ONLY_NATIVE_CALLBACK_WITH_EXISTING_COMPILED_COMMON_COUNTERPART',Release=DEFERRED,original_queries=0))
assert len({r['original_id'] for r in rows})==2
write_json(HERE,'original-only-callback-addendum-v1.json',dict(rows=rows,original_queries=0,Release=DEFERRED))
print('Two separately owned original-only callback bodies and actual delegate bindings proved',flush=True)
