"""Exact next24, full source/span authority, and actual archived compiled dependency graph."""
from common import *
from collections import Counter
sys.path.insert(0,str(PREVIOUS))
from source_views import body
from proof_kernel_v4 import bind,closure,definitions,NESTED_CACHE_DECLARATIONS
import re

entry=json.loads((HERE/'entry-executable-authentication-v1.json').read_bytes())
assert entry['status']=='AUTHENTICATED','ENTRY_NOT_AUTHENTICATED_STOP'
queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members']
current=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())
universe=json.loads((PREVIOUS/'current-universe-packet-v1.json').read_bytes())
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
oldh={h['canonical_original_id']:h for h in history};hgit={h['git_original_id']:h for h in history}
cm={n['symbol_id']:n for n in current['members']};om={n['symbol_id']:n for n in original['members']}
assert len(history)==704 and len(cm)==726 and len(current['support'])==325
assert len({h['canonical_original_id'] for h in history})==704
assert len([h for h in history if h['proof']])==435
assert {r['original_id'] for r in queue['members']}=={h['canonical_original_id'] for h in history if not h['proof']}
assert len(queue['members'])==269 and queue['first_members']==queue['members'][:24]
assert {r['current_id'] for r in queue['current_OPEN']}=={n['symbol_id'] for n in cm.values() if n['review_status'].startswith('OPEN')}
assert sum(not n['review_status'].startswith('OPEN') for n in cm.values())==465
distinct=[s for s in current['support'] if not s['already_in_primary_population']]
assert len(distinct)==303 and sum(s['fully_reconciled'] for s in distinct)==252
selected=queue['members'][:24]
expected_owner_queue=[('DA_MEANS', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|1eae5c2ad10b29ac@174615:1245'), ('DA_PREPROCESS', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|99d21013fcc234b9@176444:1554'), ('DA_COVARIANCE', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|2935743f410b8ccd@178833:2717'), ('DA_FUNCTIONS', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|4ea58f646204f78d@182242:1496'), ('DA_CANONICAL', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|30436481cbb552db@184405:1514'), ('DA_CANONCOEF', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|2f0ff71eb4d77426@186579:1498'), ('DA_CENTROIDS', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|bac6d30c0bd72eba@188673:1510'), ('DA_CONFUSION', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|18f6f8b4387d88c1@191058:1736'), ('DA_CASEWISE', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|35eb02e6f759280a@193741:1884'), ('DA_PREDICT', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|6137f5a46ca70bac@196644:2944'), ('DA_REMOVED', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|d254ceaa9565b763@200240:1194'), ('FindWrappedDiscriminantTable', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|4a8a66567d725d26@239762:1076'), ('BuildDiscriminantMeansTable', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|cf7a03753380b546@241820:1073'), ('GetDiscriminantPredictionResult', 'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|FunctionBlock|77f4481228bc3b98@242903:750'), ('HIST_BINS', 'BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|FunctionBlock|4203524fc1d2fab4@3145:2362'), ('HIST_NORMAL', 'BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|FunctionBlock|fa28dff62c0b78f5@7186:2811'), ('ROC_POINTS', 'BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|FunctionBlock|eebf3a7ea1cd9878@12848:2474'), ('ROC_STATS', 'BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|FunctionBlock|8b3d082e82a70cd2@17066:2383'), ('KM_CURVE', 'BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|FunctionBlock|3a6a1ef9b6f9232c@21873:1999'), ('CALIB_POINTS', 'BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|FunctionBlock|5ba0205615e08357@27004:3251'), ('BuildKaplanMeierStepTable', 'BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|FunctionBlock|0fcb4b292f4645d0@34344:3143'), ('lambda', 'BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|SingleLineFunctionLambdaExpression|8cbe8cb0546eb818@34810:50'), ('lambda', 'BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|SingleLineFunctionLambdaExpression|8cbe8cb0546eb818@35801:50'), ('StackHomogeneousTables', 'BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|FunctionBlock|3226ce518a866845@37497:1060')]
assert [(q['name'],q['original_id']) for q in selected]==expected_owner_queue,'OWNER_QUEUE_MISMATCH_STOP'
shapes={r['symbol_id']:r for r in json.loads((PREVIOUS/'declaration-shapes-v1.json').read_bytes())['rows']}
snapshots={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
metas={c:json.loads((PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
originals={c:json.loads((PREVIOUS/('metadata-all/packet-v1-context-v5/original-'+c+'.json')).read_bytes()) for c in metas}
defs={c:definitions(meta) for c,meta in metas.items()};olddefs={c:definitions(meta) for c,meta in originals.items()}
# These are actual existing source declarations, not an assumed type-name alias.
extra=[]
for name in ['MultipleCorrespondenceHandle','DiscriminantHandle']:
    support=[s for s in current['support'] if s['declaration']=='Private Class '+name and s['owner']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs']
    assert len(support)==1
    assert all(any(t['type']==support[0]['owner']+'+'+name for t in meta['types']) for meta in metas.values())
    NESTED_CACHE_DECLARATIONS[support[0]['owner']].add(name);extra.append(support[0])

def resolve(h):
    candidates=[cm[k] for k in h['current_ids']];o=om[h['git_original_id']]
    if o['parent_member']:
        parent=hgit[o['parent_member']];parent_ids=set(parent['current_ids'])
        candidates=[n for n in candidates if n['parent_member'] in parent_ids and n['body_sha256']==o['body_sha256'] and n['body_token_sha256']==o['body_token_sha256']]
    if len(candidates)>1 and o['parent_member']:
        op=om[o['parent_member']]
        original_parent,original_view=body((ROOT/snapshots[op['path']]['path']).read_bytes(),op)
        for parent_id in {n['parent_member'] for n in candidates}:
            cp=cm[parent_id];current_parent,current_view=body((ROOT/cp['path']).read_bytes(),cp)
            assert current_parent==original_parent and cp['body_sha256']==op['body_sha256'],'CHANGED_PARENT_CANNOT_RESOLVE_CALLBACK_STOP'
        relative=(o['span_start']-op['span_start'],o['span_length'])
        candidates=[n for n in candidates if (n['span_start']-cm[n['parent_member']]['span_start'],n['span_length'])==relative]
    assert len(candidates)==1,'AMBIGUOUS_CANONICAL_ID_STOP '+h['canonical_original_id']
    return candidates[0]

def original_method(node,current_node,config):
    original_shapes=dict(shapes);original_shapes[node['symbol_id']]=shapes[current_node['symbol_id']]
    return bind(node,originals[config],original_shapes)

def callback(node,old,config):
    parent=cm[node['parent_member']];parent_method=bind(parent,metas[config],shapes)
    siblings=sorted([n for n in cm.values() if n['parent_member']==parent['symbol_id']],key=lambda n:n['span_start'])
    targets=[i['operand'][7:] for i in parent_method['instructions'] if i['op']=='ldftn' and i['operand'][7:] in defs[config] and defs[config][i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
    assert len(siblings)==len(targets)
    index=next(i for i,n in enumerate(siblings) if n['symbol_id']==node['symbol_id'])
    op=om[old['parent_member']];pm=original_method(op,parent,config)
    os=sorted([n for n in om.values() if n['parent_member']==op['symbol_id']],key=lambda n:n['span_start'])
    ots=[i['operand'][7:] for i in pm['instructions'] if i['op']=='ldftn' and i['operand'][7:] in olddefs[config] and olddefs[config][i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
    assert len(os)==len(ots)
    oi=next(i for i,n in enumerate(os) if n['symbol_id']==old['symbol_id'])
    return dict(current_parent=parent_method['method'],current=defs[config][targets[index]],original_parent=pm['method'],original=olddefs[config][ots[oi]],
        source_sibling_index=index,original_source_sibling_index=oi)

rows=[];roots=[]
for index,q in enumerate(selected,1):
    h=oldh[q['original_id']];assert not h['proof'];n=resolve(h);o=om[h['git_original_id']]
    live,liveview=body((ROOT/n['path']).read_bytes(),n)
    archived,oldview=body((ROOT/snapshots[o['path']]['path']).read_bytes(),o)
    bindings={}
    for config in metas:
        if n['parent_member'] is None:
            method=bind(n,metas[config],shapes);before=original_method(o,n,config)
            bindings[config]=dict(current=method,original=before,graph=closure(metas[config],[method['method']]))
        else:
            bound=callback(n,o,config);bound['graph']=closure(metas[config],[bound['current']['method']]);bindings[config]=bound
    rows.append(dict(ordinal=index,historical_id=q['original_id'],name=q['name'],historical_record=h,original_record=o,
        current_record=n,original_source=snapshots[o['path']],current_source=pin(ROOT/n['path']),original_view=oldview,current_view=liveview,
        complete_original_body=archived,complete_current_body=live,compiled_bindings=bindings,
        original_fragments=[f for f in original['fragments'] if f['member_id']==o['symbol_id']],
        current_fragments=[f for f in universe['fragments'] if f['member_id']==n['symbol_id']],closure='OPEN_ENTRY_ONLY'))
    if n['parent_member'] is None:roots.append(n)
supplemental=[]
for name in ['DA_FIT','DA_DROP']:
    found=[n for n in cm.values() if n['owner']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs' and n['name']==name]
    assert len(found)==1;roots.append(found[0]);supplemental.append(dict(current_id=found[0]['symbol_id'],name=name,reason='Actual original DA fit/model/cache publication and drop lifecycle for selected DA result consumers; no unrelated family.'))
graphs={config:closure(meta,[bind(n,meta,shapes)['method'] for n in roots]) for config,meta in metas.items()}
root_history={resolve(h)['symbol_id']:h for h in history if h['current_ids'] and len(h['current_ids'])==1}
original_graphs={config:closure(meta,[original_method(om[root_history[n['symbol_id']]['git_original_id']],n,config)['method'] for n in roots]) for config,meta in originals.items()}
needed=[]
for n in cm.values():
    if n['parent_member'] is not None:continue
    pair=n['owner'],n['name']
    if any((defs['Debug'][k]['declaring_type'],defs['Debug'][k]['member_name'])==pair for k in graphs['Debug']['methods']) or any((olddefs['Debug'][k]['declaring_type'],olddefs['Debug'][k]['member_name'])==pair for k in original_graphs['Debug']['methods']):
        actual=bind(n,metas['Debug'],shapes)
        original_nodes=[o for o in om.values() if o['parent_member'] is None and (o['owner'],o['name'],o['signature_key'])==(n['owner'],n['name'],n['signature_key'])]
        was_reached=any(original_method(o,n,'Debug')['method'] in original_graphs['Debug']['methods'] for o in original_nodes)
        if actual['method'] in graphs['Debug']['methods'] or was_reached:needed.append(n)
ids={n['symbol_id'] for n in needed};pending=list(ids)
while pending:
    parent=pending.pop()
    for n in cm.values():
        if n['parent_member']==parent and n['symbol_id'] not in ids:ids.add(n['symbol_id']);pending.append(n['symbol_id'])
support=[]
reachedtypes={defs['Debug'][k]['declaring_type'] for k in graphs['Debug']['methods']}
for s in current['support']:
    owner=s['owner'].replace('+','.')
    if s['kind'] in ['ClassBlock','ModuleBlock']:
        match=re.search(r'\b(?:Class|Module)\s+(\w+)',s['declaration']);typed=(owner+'.'+match.group(1)) if match else owner
        relevant=typed in {t.replace('+','.') for t in reachedtypes if t}
    else:relevant=owner in {t.replace('+','.') for t in reachedtypes if t}
    if relevant:support.append(s)
catalog=[]
for config,graph in graphs.items():
    for key in graph['methods']:catalog.append(dict(configuration=config,record=defs[config][key]))
save('entry-compiled-dependency-catalog-v1.json',dict(methods=catalog,graphs=graphs,original_graphs=original_graphs,original_metadata=[pin(PREVIOUS/('metadata-all/packet-v1-context-v5/original-'+c+'.json')) for c in metas],metadata=[pin(PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+c+'.json')) for c in metas],original_queries=0))
save('entry-queue-receipt-v1.json',dict(status='AUTHENTICATED_CANONICAL_FIRST24_OPEN_PRE_IMPLEMENTATION',queue=pin(PREVIOUS/'remaining-work-queue-v1.json'),
    rows=rows,supplemental_lifecycle=supplemental,required_native_parents=needed,required_callbacks=[cm[k] for k in sorted(ids) if cm[k]['parent_member']],
    required_support_context=support,explicit_nested_type_bindings=extra,
    dependencies=pin(HERE/'entry-compiled-dependency-catalog-v1.json'),
    required_reports=[dict(configuration=c,edge=e) for c,g in graphs.items() for e in g['edges'] if any(word in e['target'] for word in ['Report','Table','wrapResults','UdfDataImport'])],
    newly_open_dependency_candidates=[cm[k] for k in sorted(ids) if cm[k]['review_status'].startswith('OPEN')],
    entry_counts=dict(historical_supported=435,historical_OPEN=269,current_supported=465,current_OPEN=261,support_supported=252,support_OPEN=51),
    originals='Authenticated protected snapshots and archived CIL only; no execution, recompilation, oracle or supplemental capture.',production_edits=0,original_queries=0))
lines=['# C13 authenticated entry queue','', 'OPEN entry candidates only; no closure or readiness is asserted.','', '|#|Name|Exact historical identity|Exact current identity|','|---:|---|---|---|']
lines+=['|'+str(r['ordinal'])+'|'+r['name']+'|`'+r['historical_id']+'`|`'+r['current_record']['symbol_id']+'`|' for r in rows]
lines+=['',f'Actual required graph: {len(needed)} native parents, {len(ids)-len(needed)} callbacks. Complete parameter-bound Debug/Release graph, support context, fragments and sources are in entry-queue-receipt-v1.json.']
with (HERE/'entry-queue-v1.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write('\n'.join(lines)+'\n')
print(json.dumps(dict(first24=[r['name'] for r in rows],parents=len(needed),callbacks=len(ids)-len(needed),open_candidates=[cm[k]['name'] for k in sorted(ids) if cm[k]['review_status'].startswith('OPEN')],reached={c:len(g['methods']) for c,g in graphs.items()})))
