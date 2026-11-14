"""Complete source rows, actual Debug methods/delegates and transitive ownership."""
from proof_support_v17_v2 import *
entry=load(HERE/'entry-queue-receipt-v1.json');history=load(PREVIOUS/'historical-ownership-v2.json')['members'];prior=load(PREVIOUS/'current-ownership-v1.json')
u=load(FINAL/'current-universe.json');o=load(W2.parent/'w1/original-universe.json');c=load(FINAL/'dependency-full-common.json')
meta=load(FINAL/'current-Debug.json');original=load(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json');setup(meta);setup(original)
shapes={r['symbol_id']:r for r in load(FINAL/'declaration-shapes.json')['rows']}
mp,moved=map_native(prior['members'],u['members'],c['members'])
members={r['symbol_id']:r for r in u['members']+c['members']};om={r['symbol_id']:r for r in o['members']}
originalshapes={r['symbol_id']:r for r in load(W2/'continuation-v14/declaration-shapes-v1.json').get('rows',[])} if (W2/'continuation-v14/declaration-shapes-v1.json').is_file() else {}
defs=definitions(meta);od=definitions(original)
snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
returnrows=load(ROOT/'evidence/04A38/preedit/compiled-Debug.json')['methods']
roots=[];oroots=[];rows=[];callbacks=[];propertyrows=[]
def original_regular(node,current=None):
 rows=[m for m in original['methods'] if (m['declaring_type'],m['member_name'])==(node['owner'],node['name']) and (current is None or m['parameter_types']==current['parameter_types'])]
 assert len(rows)==1,(node['symbol_id'],len(rows));return rows[0]
def old_callback(n):
 parent=om[n['parent_member']];pm=old_callback(parent)['method'] if parent['kind'].endswith('LambdaExpression') else original_regular(parent)
 siblings=sorted([r for r in o['members'] if r['parent_member']==parent['symbol_id'] and r['kind'].endswith('LambdaExpression')],key=lambda r:r['span_start'])
 sites=[(i,x) for i,x in enumerate(pm['instructions']) if x['op']=='ldftn' and isinstance(x['operand'],str) and x['operand'][7:] in od and od[x['operand'][7:]]['member_name'].startswith('_Lambda$__')]
 assert len(sites)==len(siblings),(parent['name'],len(sites),len(siblings))
 ordinal=siblings.index(n);i,x=sites[ordinal];assert pm['instructions'][i+1]['op']=='newobj'
 return dict(method=od[x['operand'][7:]],parent=pm,ordinal=ordinal,construction=pm['instructions'][i:i+2])
def find_portable_callback(a):
 p=om[a['parent_member']]
 owners={'NormalizeDesignColumnName':'MixedModelUDFsOptions38','MmrmNormalizeDesignColumnName':'UdfDataImportMetadata38','BuildAvailableReferenceSummary':'RegressionFormulaUDFsMetadata38','FormatSampleList':'RegressionFormulaUDFsMetadata38'}
 parent=next(r for r in c['members'] if r['parent_member'] is None and r['name']==p['name'] and r['owner'].endswith('.'+owners[p['name']]))
 siblings=sorted([r for r in o['members'] if r['parent_member']==p['symbol_id'] and r['kind'].endswith('LambdaExpression')],key=lambda r:r['span_start'])
 candidates=sorted([r for r in c['members'] if r['parent_member']==parent['symbol_id'] and r['kind'].endswith('LambdaExpression')],key=lambda r:r['span_start'])
 assert len(candidates)==len(siblings);return candidates[siblings.index(a)]
def clrbinding(node):
 return lambda_bind(meta,node,c if node['path'].startswith('BESHStat.Excel.Common/') else u,shapes) if node['kind'].endswith('LambdaExpression') else dict(method=bind(node,meta,shapes))
for row in entry['rows']:
 a=row['original_record'];h=row['historical'];currents=[]
 if a['kind']=='PropertyStatement':
  native=a['owner'].rsplit('.',1)[0]+'+'+a['owner'].rsplit('.',1)[1]
  nt=next(t for t in meta['types'] if t['type']==native);base=nt['base_type'].split(' @ ',1)[0];base_owner=base.split('`',1)[0]
  target=next(r for r in c['members'] if r['kind']=='PropertyStatement' and r['owner']==base_owner and r['name']==a['name'])
  text,sv=body((ROOT/target['path']).read_bytes(),target)
  same(row['original_whole_body'].replace('MmrmLsmEstimateProfileValue','TValue'),text,'Original-only '+a['owner']+'.'+a['name'])
  oldmethods=[m for m in original['methods'] if m['declaring_type']==native and m['member_name'] in ['get_'+a['name'],'set_'+a['name'],'.ctor']]
  methods=[method(meta,base,'get_'+a['name']),method(meta,base,'set_'+a['name']),method(meta,base,'.ctor'),method(meta,native,'.ctor')]
  oldgetter=next(m for m in oldmethods if m['member_name']=='get_'+a['name']);oldsetter=next(m for m in oldmethods if m['member_name']=='set_'+a['name'])
  assert methods[1]['parameter_types']==oldsetter['parameter_types'] and [x['op'] for x in methods[0]['instructions']].count('ldfld')==1
  assert [x['op'] for x in methods[1]['instructions']]==['ldarg.0','ldarg.1','stfld','ret']
  oldctor=next(m for m in oldmethods if m['member_name']=='.ctor');assert [x['op'] for x in methods[2]['instructions']]==[x['op'] for x in oldctor['instructions']]
  assert [x for x in methods[2]['instructions'] if x['op'].startswith('ldc.')]==[x for x in oldctor['instructions'] if x['op'].startswith('ldc.')]
  assert not nt['fields'];assert methods[3]['instructions'][1]['operand']=='METHOD:'+methods[2]['method']
  propertyrows.append(dict(original_id=row['original_id'],original_record=a,current_ids=[],portable_record=target,source=pin(ROOT/target['path']),whole_declaration=text,original_methods=oldmethods,current_methods=methods,current_native_type=nt,constructed_base=base,mode='EXACT_GENERIC_PROPERTY_INITIALIZER_DEFAULT_AND_CONSTRUCTED_NATIVE_INHERITED_ACCESSORS',Release=DEFERRED))
  roots.extend(m['method'] for m in methods);oroots.extend(m['method'] for m in oldmethods);continue
 if a['kind'].endswith('LambdaExpression'):
  old=old_callback(a)
  if not h['current_ids']:targets=[find_portable_callback(a)]
  else:
   choices=[mp[k] for k in h['current_ids']]
   original_tokens=tokens(row['original_whole_body'])
   targets=[n for n in choices if tokens(untext(body((ROOT/n['path']).read_bytes(),n)[0]))==original_tokens]
   assert len(targets)==1,(a['symbol_id'],len(targets))
  n=targets[0];text,sv=body((ROOT/n['path']).read_bytes(),n);same(row['original_whole_body'],untext(text),'Entire original callback '+a['symbol_id'])
  binding=clrbinding(n);now=binding['method'];assert now['parameter_types']==old['method']['parameter_types']
  callbacks.append(dict(original_id=row['original_id'],original_record=a,current_ids=[k for k in h['current_ids'] if mp[k]['symbol_id']==n['symbol_id']],current_record=n,whole_original_body=row['original_whole_body'],whole_current_body=text,source=pin(ROOT/n['path']),source_view=sv,original_binding=old,current_binding=binding,mode='EXACT_CALLBACK_SOURCE_BODY_PARENT_SIBLING_ORDINAL_AND_ACTUAL_DEBUG_LDFTN_TYPED_DELEGATE',Release=DEFERRED))
  roots.extend([now['method'],binding['parent']['method']]);oroots.extend([old['method']['method'],old['parent']['method']]);continue
 assert len(h['current_ids'])==1,(a['name'],len(h['current_ids']))
 n=mp[h['current_ids'][0]];text,sv=body((ROOT/n['path']).read_bytes(),n);now=bind(n,meta,shapes);old=original_regular(a,now)
 signatures=[r for r in returnrows if (r['declaring_type'],r['name'])==(a['owner'],a['name']) and r['parameter_types']==old['parameter_types']]
 if not signatures:signatures=[r for r in returnrows if (r['declaring_type'],r['name'])==(a['owner'],a['name'])]
 assert len(signatures)==1,(a['name'],len(signatures));assert now['signature']['return_type'].split(' @ ',1)[0]==signatures[0]['return_type']
 rows.append(dict(original_id=row['original_id'],original_record=a,original_source=row['original_source'],original_whole_body=row['original_whole_body'],original_view=row['original_view'],original_fragments=row['original_fragments'],current_ids=h['current_ids'],current_record=n,current_source=pin(ROOT/n['path']),current_whole_body=text,current_view=sv,current_fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']],compiled_Debug=dict(original=old,current=now,original_return=signatures[0]),source_composition_certificate='source-composition-proof-v2.json',Release=DEFERRED))
 roots.append(now['method']);oroots.append(old['method'])
# Reconcile every current-only OPEN callback separately, without inventing history.
current_only=[];covered={k for r in rows+callbacks for k in r['current_ids']}
for n in prior['members']:
 if not n['review_status'].startswith('OPEN') or n['symbol_id'] in covered:continue
 target=mp[n['symbol_id']];binding=clrbinding(target)
 current_only.append(dict(canonical_current_id=n['symbol_id'],current_record=target,whole_body=body((ROOT/target['path']).read_bytes(),target)[0],source=pin(ROOT/target['path']),compiled_Debug=binding,original_id=None,reason='AUTHENTIC_CURRENT_ONLY_TYPED_SELECTOR;NO_SYNTHETIC_ORIGINAL_ID',Release=DEFERRED));roots.append(binding['method']['method'])
graph=closure(meta,roots);og=closure(original,oroots);reachable=set(graph['methods'])
callers={}
for e in graph['edges']:callers.setdefault(e['target'],[]).append(e)
portable=[]
for n in c['members']:
 if n['parent_member'] is not None or n['kind'] not in ['FunctionBlock','SubBlock','ConstructorBlock']:continue
 candidate=[m for m in meta['methods'] if m['method'] in reachable and m['declaring_type'] and m['declaring_type'].split('`',1)[0]==n['owner'] and m['member_name']==('.ctor' if n['kind']=='ConstructorBlock' else n['name'])]
 if not candidate:continue
 actual=portable_bindings(n,meta,shapes,graph);text,sv=body((ROOT/n['path']).read_bytes(),n)
 portable.append(dict(source_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=sv,fragments=[f for f in c['fragments'] if f['member_id']==n['symbol_id']],compiled_Debug=[dict(method=m,actual_consumers=callers.get(m['method'],[])) for m in actual],Release=DEFERRED))
new_portable=[m for m in meta['methods'] if m['method'] in reachable and m['declaring_type'] and m['declaring_type'].startswith(('BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38','BESHStatNG.ExcelCommon.UdfW2CompletionReports38'))]
for m in new_portable:
 assert not any('Microsoft.VisualBasic.' in str(x['operand']) or 'ExcelDna.' in str(x['operand']) or 'System.IO.' in str(x['operand']) for x in m['instructions']),m['method']
save('whole-body-contracts-v1.json',dict(contracts=rows,original_queries=0,Release=DEFERRED))
save('callback-proofs-v1.json',dict(rows=callbacks,current_only=current_only,original_queries=0,Release=DEFERRED))
save('original-only-property-proofs-v1.json',dict(rows=propertyrows,original_queries=0,Release=DEFERRED))
save('portable-api-contracts-v1.json',dict(contracts=portable,new_portable_no_VB_ExcelDna_IO=True,original_queries=0,Release=DEFERRED))
save('complete-Debug-dependency-graph-v1.json',dict(current=graph,original=og,current_methods=[defs[k] for k in graph['methods']],original_methods=[od[k] for k in og['methods']],current_metadata=pin(FINAL/'current-Debug.json'),original_metadata=pin(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json'),original_queries=0,Release=DEFERRED))
print('Entire body contracts',len(rows),'original callbacks',len(callbacks),'original-only properties',len(propertyrows),'current-only',len(current_only),'portable APIs',len(portable),'transitive Debug methods',len(graph['methods']))
