"""Derive C13 queue reader from authenticated predecessor, with explicit owner IDs."""
from common import *
source=PREVIOUS/'entry_queue_v1.py';value=source.read_text(encoding='utf-8')
for before,after in [('proof_kernel_v3','proof_kernel_v4'),('current-universe-packet-v2.json','current-universe-packet-v1.json'),('declaration-shapes-v2.json','declaration-shapes-v1.json'),('packet-v2-context-v5','packet-v1-context-v5'),('==402','==435'),('==302','==269'),('==432','==465'),('==250','==252'),('historical_supported=402,historical_OPEN=302,current_supported=432,current_OPEN=294,support_supported=250,support_OPEN=53','historical_supported=435,historical_OPEN=269,current_supported=465,current_OPEN=261,support_supported=252,support_OPEN=51'),('# C12 authenticated entry queue','# C13 authenticated entry queue')]:value=value.replace(before,after)
value=value.replace("['CA_FIT','CA_DROP','MCA_DROP','DA_DROP']","['DA_FIT','DA_DROP']")
value=value.replace('Actual fit/model/cache publication or drop lifecycle for the selected CA/MCA/DA families; no unrelated family.','Actual original DA fit/model/cache publication and drop lifecycle for selected DA result consumers; no unrelated family.')
prefix1='BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb|BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs|'
prefix2='BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb|BESHStatNG.WorksheetFunctions.PlotDataUDFs|'
declared=[
('DA_MEANS',prefix1+'FunctionBlock|1eae5c2ad10b29ac@174615:1245'),
('DA_PREPROCESS',prefix1+'FunctionBlock|99d21013fcc234b9@176444:1554'),
('DA_COVARIANCE',prefix1+'FunctionBlock|2935743f410b8ccd@178833:2717'),
('DA_FUNCTIONS',prefix1+'FunctionBlock|4ea58f646204f78d@182242:1496'),
('DA_CANONICAL',prefix1+'FunctionBlock|30436481cbb552db@184405:1514'),
('DA_CANONCOEF',prefix1+'FunctionBlock|2f0ff71eb4d77426@186579:1498'),
('DA_CENTROIDS',prefix1+'FunctionBlock|bac6d30c0bd72eba@188673:1510'),
('DA_CONFUSION',prefix1+'FunctionBlock|18f6f8b4387d88c1@191058:1736'),
('DA_CASEWISE',prefix1+'FunctionBlock|35eb02e6f759280a@193741:1884'),
('DA_PREDICT',prefix1+'FunctionBlock|6137f5a46ca70bac@196644:2944'),
('DA_REMOVED',prefix1+'FunctionBlock|d254ceaa9565b763@200240:1194'),
('FindWrappedDiscriminantTable',prefix1+'FunctionBlock|4a8a66567d725d26@239762:1076'),
('BuildDiscriminantMeansTable',prefix1+'FunctionBlock|cf7a03753380b546@241820:1073'),
('GetDiscriminantPredictionResult',prefix1+'FunctionBlock|77f4481228bc3b98@242903:750'),
('HIST_BINS',prefix2+'FunctionBlock|4203524fc1d2fab4@3145:2362'),
('HIST_NORMAL',prefix2+'FunctionBlock|fa28dff62c0b78f5@7186:2811'),
('ROC_POINTS',prefix2+'FunctionBlock|eebf3a7ea1cd9878@12848:2474'),
('ROC_STATS',prefix2+'FunctionBlock|8b3d082e82a70cd2@17066:2383'),
('KM_CURVE',prefix2+'FunctionBlock|3a6a1ef9b6f9232c@21873:1999'),
('CALIB_POINTS',prefix2+'FunctionBlock|5ba0205615e08357@27004:3251'),
('BuildKaplanMeierStepTable',prefix2+'FunctionBlock|0fcb4b292f4645d0@34344:3143'),
('lambda',prefix2+'SingleLineFunctionLambdaExpression|8cbe8cb0546eb818@34810:50'),
('lambda',prefix2+'SingleLineFunctionLambdaExpression|8cbe8cb0546eb818@35801:50'),
('StackHomogeneousTables',prefix2+'FunctionBlock|3226ce518a866845@37497:1060')]
needle="selected=queue['members'][:24]";assert needle in value
value=value.replace(needle,needle+'\nexpected_owner_queue='+repr(declared)+"\nassert [(q['name'],q['original_id']) for q in selected]==expected_owner_queue,'OWNER_QUEUE_MISMATCH_STOP'")
needle="graphs={config:closure(meta,[bind(n,meta,shapes)['method'] for n in roots]) for config,meta in metas.items()}";assert needle in value
value=value.replace(needle,needle+"\nroot_history={resolve(h)['symbol_id']:h for h in history if h['current_ids'] and len(h['current_ids'])==1}\noriginal_graphs={config:closure(meta,[original_method(om[root_history[n['symbol_id']]['git_original_id']],n,config)['method'] for n in roots]) for config,meta in originals.items()}")
needle="if any((defs['Debug'][k]['declaring_type'],defs['Debug'][k]['member_name'])==pair for k in graphs['Debug']['methods']):\n        actual=bind(n,metas['Debug'],shapes)\n        if actual['method'] in graphs['Debug']['methods']:needed.append(n)";assert needle in value
value=value.replace(needle,"if any((defs['Debug'][k]['declaring_type'],defs['Debug'][k]['member_name'])==pair for k in graphs['Debug']['methods']) or any((olddefs['Debug'][k]['declaring_type'],olddefs['Debug'][k]['member_name'])==pair for k in original_graphs['Debug']['methods']):\n        actual=bind(n,metas['Debug'],shapes)\n        original_nodes=[o for o in om.values() if o['parent_member'] is None and (o['owner'],o['name'],o['signature_key'])==(n['owner'],n['name'],n['signature_key'])]\n        was_reached=any(original_method(o,n,'Debug')['method'] in original_graphs['Debug']['methods'] for o in original_nodes)\n        if actual['method'] in graphs['Debug']['methods'] or was_reached:needed.append(n)")
value=value.replace("methods=catalog,graphs=graphs,metadata=", "methods=catalog,graphs=graphs,original_graphs=original_graphs,original_metadata=[pin(PREVIOUS/('metadata-all/packet-v1-context-v5/original-'+c+'.json')) for c in metas],metadata=")
target=HERE/'entry_queue_v1.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('entry-queue-source-crosswalk-v1.json',dict(before=pin(source),after=pin(target),expected_owner_queue=declared,scope='Final v12 input paths/counts; exact v13 owner queue; DA lifecycle only; union of true original/current archived compiled native dependencies, without original execution or capture.',original_queries=0))
print('C13 queue reader prepared from exact owner IDs and full original/current dependency graphs')
