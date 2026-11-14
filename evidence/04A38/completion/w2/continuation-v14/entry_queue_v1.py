"""Exact canonical queue and full actual compiled property/callable dependency graph."""
from common import *
sys.path.insert(0,str(PREVIOUS))
from source_views import body
from proof_kernel_v6 import bind,definitions,closure,NESTED_CACHE_DECLARATIONS,semantic
from collections import Counter
assert json.loads((HERE/'entry-executable-authentication-v1.json').read_bytes())['status']=='AUTHENTICATED'
h=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members'];c=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());u=json.loads((PREVIOUS/'current-universe-packet-v1.json').read_bytes());o=json.loads((W2.parent/'w1/original-universe.json').read_bytes());q=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())
assert len(h)==704 and sum(bool(r['proof']) for r in h)==461 and len(c['members'])==726 and sum(not r['review_status'].startswith('OPEN') for r in c['members'])==494
assert len([s for s in c['support'] if not s['already_in_primary_population']])==303 and sum(s['fully_reconciled'] for s in c['support'] if not s['already_in_primary_population'])==253
assert len(q['members'])==243 and {r['original_id'] for r in q['members']}=={r['canonical_original_id'] for r in h if not r['proof']}
assert len(q['current_OPEN'])==232 and {r['current_id'] for r in q['current_OPEN']}=={r['symbol_id'] for r in c['members'] if r['review_status'].startswith('OPEN')}
assert len(q['support_OPEN'])==50 and {s['support_id'] for s in q['support_OPEN']}=={s['support_id'] for s in c['support'] if not s['already_in_primary_population'] and not s['fully_reconciled']}
expected=[('ROC_POINTS','PlotDataUDFs','FunctionBlock','eebf3a7ea1cd9878@12848:2474'),('ROC_STATS','PlotDataUDFs','FunctionBlock','8b3d082e82a70cd2@17066:2383'),('Alpha','PropensityScoreUDFs.PsmUdfHandle','PropertyStatement','1f81b0a18f0e68fe@2870:39'),('PS_FIT','PropensityScoreUDFs','FunctionBlock','aef59c2e00938b4e@11274:5235'),('PS_SUMMARY','PropensityScoreUDFs','FunctionBlock','4bf78642c7f8d435@17851:918'),('PS_SCORES','PropensityScoreUDFs','FunctionBlock','10fcfdbbf4a84769@19965:1153'),('PSM_MATCHES','PropensityScoreUDFs','FunctionBlock','a7a539470a75a65c@22605:1126'),('PS_WEIGHTS','PropensityScoreUDFs','FunctionBlock','8a243b2fc69fd693@25063:1157'),('PS_BALANCE','PropensityScoreUDFs','FunctionBlock','dec5336f087dfd1e@27557:1099'),('PS_EFFECT','PropensityScoreUDFs','FunctionBlock','6b30c11ec79337d7@29889:1101'),('PS_LOVEPLOT_DATA','PropensityScoreUDFs','FunctionBlock','ce4731997b4341b0@32207:1145'),('PS_TABLE','PropensityScoreUDFs','FunctionBlock','8f97597c7bf11b57@34922:1381'),('BuildFitOptions','PropensityScoreUDFs','FunctionBlock','3f491d6b02116d15@38683:1197'),('NormalizeFitOptions','PropensityScoreUDFs','SubBlock','38ce4e9fef3d8a4a@39892:1065'),('ApplyOptionText','PropensityScoreUDFs','SubBlock','ecf499688e2ec662@40969:740'),('ApplyOption','PropensityScoreUDFs','SubBlock','66c51bd9d385759b@41721:5411'),('EnsureCoarseningSpec','PropensityScoreUDFs','SubBlock','528895e7de53e619@47144:413'),('ResolveNamedTable','PropensityScoreUDFs','FunctionBlock','a2bb4382b9645aeb@47569:2926'),('DataImportSummaryTable','PropensityScoreUDFs','FunctionBlock','0198aa4b3fc209b6@50507:1420'),('ParseRunMethod','PropensityScoreUDFs','FunctionBlock','d521fef63ad9e043@51939:1065'),('ParseEstimand','PropensityScoreUDFs','FunctionBlock','ddeb09debb5d238b@53016:211'),('ParseScoreMethod','PropensityScoreUDFs','FunctionBlock','6bbfbb3cd115c227@53239:556'),('ParseDistance','PropensityScoreUDFs','FunctionBlock','675a03c3687b0bda@53807:872'),('ParseCaliperScale','PropensityScoreUDFs','FunctionBlock','2485a76036aa8e22@54691:1012')]
ids=[('BESHstatNG/src/ExcelUDFs/'+owner.split('.')[0]+'.vb|BESHStatNG.WorksheetFunctions.'+owner+'|'+kind+'|'+tail) for _,owner,kind,tail in expected]
assert [(r['name'],r['original_id']) for r in q['members'][:24]]==[(row[0],key) for row,key in zip(expected,ids)]
cm={n['symbol_id']:n for n in c['members']};om={n['symbol_id']:n for n in o['members']};hm={r['canonical_original_id']:r for r in h}
ss={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
shapes={r['symbol_id']:r for r in json.loads((PREVIOUS/'declaration-shapes-v1.json').read_bytes())['rows']}
metas={cfg:json.loads((PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json')).read_bytes()) for cfg in ['Debug','Release']};old={cfg:json.loads((PREVIOUS/('metadata-all/packet-v1-context-v5/original-'+cfg+'.json')).read_bytes()) for cfg in metas}
typesupport=next(s for s in c['support'] if s['kind']=='ClassBlock' and s['declaration']=='Private Class PsmUdfHandle')
assert all(any(t['type']=='BESHStatNG.WorksheetFunctions.PropensityScoreUDFs+PsmUdfHandle' for t in m['types']) for m in metas.values())
NESTED_CACHE_DECLARATIONS[typesupport['owner']]={'PsmUdfHandle'}
def current(r):
 choices=[cm[k] for k in r['current_ids']];assert len(choices)==1,'AMBIGUOUS_ENTRY_ID_STOP '+r['canonical_original_id'];return choices[0]
def methods(node,meta,original_node=None):
 if node['kind']=='PropertyStatement':
  owner=node['owner'].rsplit('.',1);typename=owner[0]+'+'+owner[1]
  found=[m for m in meta['methods'] if m['declaring_type']==typename and m['member_name'] in ['get_'+node['name'],'set_'+node['name'],'.ctor']]
  assert len(found)==3;return found
 shape=dict(shapes)
 if original_node:shape[original_node['symbol_id']]=shapes[node['symbol_id']]
 return [bind(original_node or node,meta,shape)]
rows=[];roots={cfg:[] for cfg in metas};oroots={cfg:[] for cfg in metas}
for ix,key in enumerate(ids,1):
 hist=hm[key];n=current(hist);a=om[hist['git_original_id']];before,bv=body((ROOT/ss[a['path']]['path']).read_bytes(),a);now,nv=body((ROOT/n['path']).read_bytes(),n);bindings=[]
 for cfg,meta in metas.items():
  ms=methods(n,meta);oms=methods(n,old[cfg],a);roots[cfg].extend(m['method'] for m in ms);oroots[cfg].extend(m['method'] for m in oms)
  bindings.append(dict(configuration=cfg,current=ms,original=oms))
 rows.append(dict(ordinal=ix,name=n['name'],historical_id=key,historical_record=hist,original_record=a,current_record=n,original_source=ss[a['path']],current_source=pin(ROOT/n['path']),original_view=bv,current_view=nv,complete_original_body=before,complete_current_body=now,compiled_bindings=bindings,original_fragments=[f for f in o['fragments'] if f['member_id']==a['symbol_id']],current_fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']],closure='OPEN_ENTRY_ONLY'))
dependencies=['BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportPlot.vb|BESHStatNG.UdfDataImport|FunctionBlock|5863e70a6f9b4c6f@6803:2097','BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportPlot.vb|BESHStatNG.UdfDataImport|FunctionBlock|5b63d0ff64a1d271@521:3241']
assert all(key in hm and not hm[key]['proof'] for key in dependencies)
life=next(n for n in cm.values() if n['owner']=='BESHStatNG.WorksheetFunctions.PropensityScoreUDFs' and n['name']=='PS_CLEANUP')
lh=next(r for r in h if life['symbol_id'] in r['current_ids']);lo=om[lh['git_original_id']]
for cfg,meta in metas.items():
 roots[cfg].extend(m['method'] for m in methods(life,meta));oroots[cfg].extend(m['method'] for m in methods(life,old[cfg],lo))
graphs={cfg:closure(meta,roots[cfg]) for cfg,meta in metas.items()};originalgraphs={cfg:closure(meta,oroots[cfg]) for cfg,meta in old.items()}
needed=[]
for n in cm.values():
 if n['kind'] not in ['FunctionBlock','SubBlock','ConstructorBlock']:continue
 if n['parent_member']:continue
 current_family=[m for m in metas['Debug']['methods'] if (m['declaring_type'],m['member_name'])==(n['owner'],n['name']) and m['method'] in graphs['Debug']['methods']]
 original_family=[m for m in old['Debug']['methods'] if (m['declaring_type'],m['member_name'])==(n['owner'],n['name']) and m['method'] in originalgraphs['Debug']['methods']]
 if current_family or original_family:needed.append(dict(source=n,source_shape=shapes[n['symbol_id']],current_actual_targets=current_family,original_actual_targets=original_family))
parents={r['source']['symbol_id'] for r in needed};children=set();pending=list(parents)
while pending:
 p=pending.pop()
 for n in cm.values():
  if n['parent_member']==p and n['symbol_id'] not in children:children.add(n['symbol_id']);pending.append(n['symbol_id'])
reachedtypes={m['declaring_type'] for m in metas['Debug']['methods'] if m['method'] in graphs['Debug']['methods']}
support=[s for s in c['support'] if any(t and (t.replace('+','.')==s['owner'] or t.replace('+','.').startswith(s['owner']+'.')) for t in reachedtypes)]
save('entry-compiled-dependency-catalog-v1.json',dict(graphs=graphs,original_graphs=originalgraphs,current_metadata=[pin(PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json')) for cfg in metas],original_metadata=[pin(PREVIOUS/('metadata-all/packet-v1-context-v5/original-'+cfg+'.json')) for cfg in metas],current_methods={cfg:[m for m in meta['methods'] if m['method'] in graphs[cfg]['methods']] for cfg,meta in metas.items()},original_queries=0))
save('entry-queue-receipt-v1.json',dict(status='AUTHENTICATED_CANONICAL_FIRST24_OPEN_PRE_IMPLEMENTATION',rows=rows,required_ROC_dependency_ids=dependencies,required_native_parents=[r['source'] for r in needed],actual_source_bindings=needed,required_callbacks=[cm[k] for k in sorted(children)],required_support_context=support,property_context=typesupport,supplemental_lifecycle=dict(source=life,historical_id=lh['canonical_original_id'],reason='Complete source-backed cache clear/remove lifecycle for selected PSM fit and result ownership.'),dependencies=pin(HERE/'entry-compiled-dependency-catalog-v1.json'),queue=pin(PREVIOUS/'remaining-work-queue-v1.json'),production_edits=0,original_queries=0,
 obligations='Full original/current source/span/branch/inline, actual generated ldftn and constructed generics, Alpha getter/setter/default constructor, original compiler flags; acquisition/Guid/time/wizard/cache/errors/Err remain native. No whole-parent closure before complete dependency and output proof.'))
save('v13-independent-entry-reconciliation-v1.json',dict(historical=dict(total=704,supported=461,OPEN=243),current=dict(total=726,supported=494,OPEN=232),support=dict(distinct=303,supported=253,OPEN=50),original_fragments=dict(covered=11114,total=20683),current_fragments=dict(covered=6494,total=14492),entry=pin(HERE/'entry-executable-authentication-v1.json'),original_queries=0))
with (HERE/'entry-queue-v1.md').open('x',encoding='utf-8',newline='\n') as f:f.write('# C14 canonical entry queue\n\nOPEN entry candidates only; no closure asserted.\n\n'+'\n'.join(str(r['ordinal'])+'. '+r['name']+' — `'+r['historical_id']+'`' for r in rows)+'\n')
print(json.dumps(dict(first24=[r['name'] for r in rows],native_parents=len(needed),callbacks=len(children),current_reached={cfg:len(g['methods']) for cfg,g in graphs.items()},original_queries=0)))
