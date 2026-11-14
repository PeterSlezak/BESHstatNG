"""Exact owner-selected58 identities, actual bodies/fragments/Debug dependency graph."""
from common import *
from collections import Counter
sys.path.append(str(PREVIOUS))
from source_views import body
from proof_kernel_v8 import bind,closure,definitions,NESTED_CACHE_DECLARATIONS
assert json.loads((HERE/'entry-authentication-v1.json').read_bytes())['status']=='ENTRY_GATES_PASS'
load=lambda p:json.loads(p.read_bytes())
h=load(PREVIOUS/'historical-ownership-v2.json')['members']
c=load(PREVIOUS/'current-ownership-v3.json')
q=load(PREVIOUS/'remaining-work-queue-v2.json')
u=load(PREVIOUS/'final-v1/current-universe.json')
o=load(W2.parent/'w1/original-universe.json')
assert len(h)==704 and sum(bool(r['proof']) for r in h)==537
assert len(c['members'])==len(u['members'])==726
assert sum(not r['review_status'].startswith('OPEN') for r in c['members'])==566
distinct=[r for r in c['support'] if not r['already_in_primary_population']]
assert len(distinct)==303 and sum(r['fully_reconciled'] for r in distinct)==262
assert [r['original_id'] for r in q['members']]==[r['canonical_original_id'] for r in h if not r['proof']] and len(q['members'])==167
assert [r['current_id'] for r in q['current_OPEN']]==[r['symbol_id'] for r in c['members'] if r['review_status'].startswith('OPEN')] and len(q['current_OPEN'])==160
assert [r['support_id'] for r in q['support_OPEN']]==[r['support_id'] for r in distinct if not r['fully_reconciled']] and len(q['support_OPEN'])==41
names='GEE_LSMESTIMATE ParseGeeLsmEstimateScale AverageGeeFittedDesignRowForLsmProfile GeeFindHeaderIndex GeeFindDesignColumnIndex GeeNormalizeDesignColumnName lambda GLMNB_FIT GLMNB_SUMMARY GLMNB_TESTS GLMNB_RESID GLMNB_PRED BuildParameterNames GLM_FIT GLM_SUMMARY GLM_TESTS GLM_RESID GLM_PRED GLM_CLASS GLM_THRESH GLM_CALIB GLM_BRIER BuildParameterNames ParseGlmResidualType BuildAllResidualOutput LM_FIT LM_SUMMARY LM_TESTS LM_ANOVA LM_VIF LM_RESID LM_PRED ParseLmAnovaScope ParseLmResidualType DropTopRows AppendFootnotes AppendFootnotesInPlace N Get LMM_FIT LMM_RESULTS LMM_COEF LMM_TYPE3 LMM_COVPARMS LMM_G_COV LMM_G_CORR LMM_R_COV LMM_R_CORR LMM_RANEF LMM_FITSTATS LMM_FITTED LMM_RESID LMM_CLEAR_ALL TryGetLmmAlignedInputs TryBuildLmmExpandedDesign ResolveImportedNames NormalizeNameList EnsureLmmResultUsesHandleNames'.split()
tails='''250a00c96963749f@76028:7950 6354f4c72882a598@86334:405 2083976c680ad5d9@103712:1884 9dfa836c95f56a69@105606:588 8b2f24168cad46d9@106204:692 9af7fa1eae0f5e6c@106906:256 428d1e4fd4aba8cd@107092:37
4891274129715627@9810:10706 54b24a79c977b856@22147:3439 44542fceed15653a@27194:3284 2fab7feade1a6597@31945:2706 d79f57da0c4d4f40@36276:5008 6f13aeaf87314c57@42684:937
d7242ffcfa929151@12068:11651 1a28ab1e5f9607c0@25706:3425 2290f3cb868d9afd@30886:3378 dd747d13379e1c4e@36329:2690 1442d93dc14d4bcc@41290:4990 51d3658abb64d4ed@49432:2162 66e4a6ed1d924e62@53874:2242 f559aeaa536e0523@58532:2457 3479c819b534546e@62449:1849 3ee624ae604c621e@67385:935 2e1244d59ae5e84c@68332:977 4aeed379ebe50504@69321:1653
6bd14a263e961f38@8528:11076 fd61cf7bb018104d@20728:3772 7a2e57f7db6976f3@25179:2308 8903ffb4f5c8c7a3@28315:2157 6f248dc0953807fe@31155:1493 c2fe4031501e0169@33741:2983 d7818e0f5fa33c17@37767:4891 fb4d504a88468bdb@43952:529 bc0a8e187bc59f94@44616:929 3353677d5e879cde@45699:759 3a118759717c1f3e@46595:726 bd1e4f68088f9f4e@47486:299
73cbbf0cfbb92765@3110:167 5fb9c3aba7ad5106@3165:86 25db4fbfc139db6c@10103:19201 4afde59f5c2b1b0d@31013:2852 babd7e8e861538f3@34682:728 140aca375a606bc4@36368:759 e5e608768fbbc7c7@37780:579 66a0421262013ff2@38892:600 d9b9866a74436285@40047:604 d7ac96dc18b4b4b1@41227:595 f4bacf4c3fca76b6@42428:599 c282ec120bb0afe1@43648:586 be37d0682a98884c@44758:556 19d3d7a23776ce16@46019:1776 05c0e75d1f0d32de@48547:2112 fb046e57ca52ebe0@52596:472 69c312053e9d59e1@53080:7639 bf3c00f844104327@60731:3932 62e48b22b283476e@64675:887 6346c2d973a92d6d@65574:848 2f1997b24bb50f61@67151:556'''.split()
assert len(names)==len(tails)==58
ids=[]
for i,tail in enumerate(tails,1):
 file,owner=('GeeUDFs','GEEUDFs') if i<=7 else ('GlmNbUDFs','GLMNbUDFs') if i<=13 else ('GlmUDFs','GLMUDFs') if i<=25 else ('LinearModelUDFs','LinearModelUDFs') if i<=37 else ('LmmUDFs','LmmUDFs')
 kind='SingleLineFunctionLambdaExpression' if i==7 else 'PropertyBlock' if i==38 else 'GetAccessorBlock' if i==39 else 'SubBlock' if i in [37,58] else 'FunctionBlock'
 if i in [38,39]:owner+='.LmmImportedInputs'+('.N' if i==39 else '')
 ids.append('BESHstatNG/src/ExcelUDFs/Regression/'+file+'.vb|BESHStatNG.WorksheetFunctions.'+owner+'|'+kind+'|'+tail)
assert [(r['name'],r['original_id']) for r in q['members'][:58]]==list(zip(names,ids))
cm={r['symbol_id']:r for r in c['members']};om={r['symbol_id']:r for r in o['members']};hm={r['canonical_original_id']:r for r in h}
shapes={r['symbol_id']:r for r in load(PREVIOUS/'final-v1/declaration-shapes.json')['rows']}
meta=load(PREVIOUS/'final-v1/current-Debug.json')
original_path=W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json'
original=load(original_path)
snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
for owner,name in [('CoxUDFs','CoxModelHandle'),('GEEUDFs','GeeHandle'),('GLMNbUDFs','GlmNbHandle'),('GLMUDFs','GlmHandle'),('LinearModelUDFs','LinearModelHandle'),('LmmUDFs','LmmHandle')]:NESTED_CACHE_DECLARATIONS['BESHStatNG.WorksheetFunctions.'+owner]={name}
def native_binding(n,m,shape_node=None):
 if n['kind'] in ['PropertyBlock','GetAccessorBlock']:
  prop=n if n['kind']=='PropertyBlock' else (cm.get(n['parent_member']) or om[n['parent_member']])
  owner=prop['owner'].replace('.LmmImportedInputs','+LmmImportedInputs')
  rows=[r for r in m['methods'] if (r['declaring_type'],r['member_name'])==(owner,'get_'+prop['name'])]
  assert len(rows)==1;return rows[0]
 ns={**shapes,n['symbol_id']:shapes[(shape_node or n)['symbol_id']]}
 return bind(n,m,ns)
def lambda_binding(parent,m):
 p=native_binding(parent,m,next(v for v in cm.values() if (v['owner'],v['name'])==(parent['owner'],parent['name']))) if parent['symbol_id'] not in shapes else native_binding(parent,m)
 defs=definitions(m);rows=[defs[x['operand'][7:]] for x in p['instructions'] if x['op']=='ldftn' and isinstance(x['operand'],str) and x['operand'][7:] in defs and defs[x['operand'][7:]]['member_name'].startswith('_Lambda$__')]
 assert len(rows)==1;return p,rows[0]
rows=[];roots=[];oroots=[]
portable=load(PREVIOUS/'final-v1/dependency-full-common.json')
for ordinal,key in enumerate(ids,1):
 history=hm[key];a=om[history['git_original_id']]
 oldtext,oldview=body((ROOT/snapshots[a['path']]['path']).read_bytes(),a)
 current=[cm[k] for k in history['current_ids']]
 if ordinal==7:
  assert not current and a['kind']=='SingleLineFunctionLambdaExpression'
  p,oldmethod=lambda_binding(om[a['parent_member']],original)
  parent=next(n for n in portable['members'] if n['owner']=='BESHStatNG.ExcelCommon.GEEUDFsMetadata38' and n['name']=='GeeNormalizeDesignColumnName')
  n=next(n for n in portable['members'] if n['parent_member']==parent['symbol_id'])
  text,source_view=body((ROOT/n['path']).read_bytes(),n);assert text==oldtext
  pp=native_binding(parent,meta);defs=definitions(meta)
  targets=[defs[x['operand'][7:]] for x in pp['instructions'] if x['op']=='ldftn' and x['operand'][7:] in defs and defs[x['operand'][7:]]['member_name'].startswith('_Lambda$__')];assert len(targets)==1
  currentmethod=targets[0];info=dict(kind='HISTORICAL_ONLY_AUTHENTIC_COMMON_CALLBACK',current_ids=[],record=n,parent=parent,complete_body=text,source_view=source_view,source=pin(ROOT/n['path']),original_parent=p,current_parent=pp)
  fragments=[f for f in portable['fragments'] if f['member_id']==n['symbol_id']]
 else:
  assert len(current)==1;n=current[0]
  text,source_view=body((ROOT/n['path']).read_bytes(),n)
  currentmethod=native_binding(n,meta);oldmethod=native_binding(a,original,n)
  info=dict(kind='CURRENT_WINDOWS_COUNTERPART',current_ids=history['current_ids'],record=n,complete_body=text,source_view=source_view,source=pin(ROOT/n['path']))
  fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']]
 roots.append(currentmethod['method']);oroots.append(oldmethod['method'])
 rows.append(dict(ordinal=ordinal,packet='A' if ordinal<=37 else 'B',name=a['name'],historical_id=key,historical_record=history,original_record=a,original_source=snapshots[a['path']],complete_original_body=oldtext,original_view=oldview,current=info,current_fragments=fragments,original_fragments=[f for f in o['fragments'] if f['member_id']==a['symbol_id']],compiled_Debug=dict(original=oldmethod,current=currentmethod),status='OPEN_PRE_IMPLEMENTATION_TARGET_NOT_CLOSURE',portable_candidates=['Reuse actual existing Core numeric/formula/LS-means engines and existing Common options/metadata/profile contracts','Move only reusable report/name/range/design preparation with actual Windows consumer'],host_boundaries=['Excel import/argument evaluation/native error arrays and final materialization','Model handles/session cache/workbook/registration lifetime','Exception/Err logging and current scientific engine policy'],Release=DEFERRED,original_queries=0))
graph=closure(meta,roots);ograph=closure(original,oroots)
needed=[]
for n in cm.values():
 if n['parent_member'] or n['kind'] not in ['FunctionBlock','SubBlock','ConstructorBlock']:continue
 if any(r['declaring_type']==n['owner'] and r['member_name']==n['name'] and r['method'] in graph['methods'] for r in meta['methods']):
  method=native_binding(n,meta);needed.append(dict(source=n,actual_compiled=method))
parents={r['source']['symbol_id'] for r in needed};callbacks=[];pending=list(parents)
while pending:
 parent=pending.pop()
 for n in cm.values():
  if n['parent_member']==parent and n['symbol_id'] not in {v['symbol_id'] for v in callbacks}:callbacks.append(n);pending.append(n['symbol_id'])
types={r['declaring_type'].replace('+','.') for r in meta['methods'] if r['method'] in graph['methods'] and r['declaring_type']}
supports=[s for s in c['support'] if s['owner'] in types or any(t.startswith(s['owner']+'.') for t in types)]
save('entry-compiled-dependency-catalog-v1.json',dict(Debug_graph=graph,original_Debug_graph=ograph,current_methods=[r for r in meta['methods'] if r['method'] in graph['methods']],original_methods=[r for r in original['methods'] if r['method'] in ograph['methods']],current_metadata=pin(PREVIOUS/'final-v1/current-Debug.json'),original_metadata=pin(original_path),Release=DEFERRED,original_queries=0))
save('entry-queue-receipt-v1.json',dict(status='AUTHENTICATED_EXACT_FIRST58_OPEN_PRE_IMPLEMENTATION',rows=rows,full_ordered_OPEN_complements=q,required_native_parents=needed,required_callbacks=callbacks,required_support_context=supports,dependencies=pin(HERE/'entry-compiled-dependency-catalog-v1.json'),entry=pin(HERE/'entry-authentication-v1.json'),queue=pin(PREVIOUS/'remaining-work-queue-v2.json'),production_edits=0,original_queries=0,Release=DEFERRED,obligations='Targets only; entire bodies/fragments/callbacks/Debug CLR and all genuine dependencies required before closure. N/Get remain separately source-identified. Exact original GEE lambda has authentic Common counterpart and no fabricated current Windows ID. Stop before MMRM family by default.'))
print('Exact58 entry queue and Debug graphs authenticated',len(needed),'native parents',len(callbacks),'callbacks',len(graph['methods']),'transitive methods',flush=True)
