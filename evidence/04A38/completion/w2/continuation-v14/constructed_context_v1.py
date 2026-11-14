"""Actual complete CIL delta and all reached constructed owners; PSM state stays native."""
from common import *
from proof_kernel_v8 import definitions,semantic
from source_views import view
draft='--draft' in sys.argv;out=PRIVATE/'draft-proof-v1' if draft else HERE
load=lambda n:json.loads(((out/n) if (out/n).exists() else HERE/n).read_bytes())
diag=load('cohort-diagnostic-v1.json');configs=[];deltas=[];handles=[]
expected={('BESHStatNG.WorksheetFunctions.PropensityScoreUDFs',n) for n in ['BuildFitOptions','ResolveNamedTable','DataImportSummaryTable']}|{('BESHStatNG.UdfDataImport',n) for n in ['GuessPositiveClass','TryGetPsmData']}
u=load('current-universe-packet-v1.json');original=json.loads((W2.parent/'w1/original-universe.json').read_bytes());source='BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb';snap=next(r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files'] if r['source']['path']==source)
current_support=next(s for s in u['supports'] if s['kind']=='ClassBlock' and s['declaration']=='Private Class PsmUdfHandle');old_support=next(s for s in original['supports'] if s['kind']=='ClassBlock' and s['declaration']=='Private Class PsmUdfHandle')
text,cv=view((ROOT/source).read_bytes(),next(s['sha256'] for s in u['sources'] if s['path']==source));before,ov=view((ROOT/snap['path']).read_bytes(),next(s['sha256'] for s in original['sources'] if s['path']==source))
span=lambda t,r:t.encode('utf-16-le')[r['span_start']*2:(r['span_start']+r['span_length'])*2].decode('utf-16-le');assert span(text,current_support).replace('\r\n','\n')==span(before,old_support).replace('\r\n','\n')
for cfg in ['Debug','Release']:
 meta=load('metadata-all/packet-v1-context-v5/current-'+cfg+'.json');prior=json.loads((PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json')).read_bytes());orig=load('metadata-all/packet-v1-context-v5/original-'+cfg+'.json');defs=definitions(meta);bd=definitions(prior);od=definitions(orig);types={t['type']:t for t in meta['types'] if t['type']};pt={t['type']:t for t in prior['types'] if t['type']};ot={t['type']:t for t in orig['types'] if t['type']}
 changed=[k for k in bd if k in defs and semantic(bd[k])!=semantic(defs[k])];added=sorted(set(defs)-set(bd));assert {(defs[k]['declaring_type'],defs[k]['member_name']) for k in changed}==expected and len(added)==8 and not set(bd)-set(defs)
 assert {(defs[k]['declaring_type'],defs[k]['member_name']) for k in added}=={('BESHStatNG.ExcelCommon.UdfClassLabelPreparation38','GuessPositiveClassAfterConversion')}|{('BESHStatNG.ExcelCommon.UdfPsmDataPreparation38',n) for n in ['.ctor','CreateFitOptionsPrepared','CompleteFitOptionsPrepared','ResolveTableKindPrepared','BuildImportSummaryPrepared','BuildModelNamesPrepared','BuildModelInputPrepared']}
 contexts=[]
 for owner in sorted({defs[k]['declaring_type'] for k in diag['graphs'][cfg]['methods']}):
  assert owner in types;ctors=[m for m in meta['methods'] if m['declaring_type']==owner and m['member_name'] in ['.ctor','.cctor']]
  if owner in pt:assert types[owner]==pt[owner] and all(m['method'] in bd and semantic(m)==semantic(bd[m['method']]) for m in ctors)
  else:assert owner=='BESHStatNG.ExcelCommon.UdfPsmDataPreparation38' and not types[owner]['fields']
  contexts.append(dict(owner=owner,type=types[owner],prior_type=pt.get(owner),constructors_and_initializers=ctors,reached_methods=[k for k in diag['graphs'][cfg]['methods'] if defs[k]['declaring_type']==owner]))
 configs.append(dict(configuration=cfg,contexts=contexts,reached_methods=len(diag['graphs'][cfg]['methods']),metadata=pin(HERE/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json'))))
 deltas.append(dict(configuration=cfg,changed=[dict(method=k,original=bd[k],current=defs[k]) for k in changed],added=[defs[k] for k in added],lost=[],new_managed_apis=7,new_private_constructor=1,original_queries=0))
 base='BESHStatNG.WorksheetFunctions.PropensityScoreUDFs';owner=base+'+PsmUdfHandle';assert types[base]==ot[base] and types[owner]==ot[owner];methods=[m for m in meta['methods'] if m['declaring_type']==owner];assert len(methods)==13 and all(m['method'] in od and semantic(m)==semantic(od[m['method']]) for m in methods)
 props=[s for s in u['supports'] if s['owner']==base+'.PsmUdfHandle'];assert len(props)==6;names=[s['declaration'].split('Property ',1)[1].split()[0] for s in props];assert {f['name'] for f in types[owner]['fields']}=={'_'+n for n in names}
 setters={m['method'] for m in methods if m['member_name'].startswith('set_')};writers=[dict(caller=m['method'],instruction=i,target=x['operand'][7:]) for m in meta['methods'] for i,x in enumerate(m['instructions']) if x['op'] in ['call','callvirt'] and x.get('operand') in {'METHOD:'+s for s in setters}]
 fit=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(base,'PS_FIT'));ctor=next(m for m in methods if m['member_name']=='.ctor');stores=[i for i,x in enumerate(fit['instructions']) if x['op']=='callvirt' and '.set_Item(' in (x.get('operand') or '') and 'ConcurrentDictionary' in x['operand']];assert len(stores)==1 and len(writers)==7
 assert sum(w['caller']==ctor['method'] for w in writers)==1 and all(w['caller']==ctor['method'] or w['caller']==fit['method'] and w['instruction']<stores[0] for w in writers)
 cctor=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(base,'.cctor'));assert semantic(cctor)==semantic(od[cctor['method']])
 handles.append(dict(configuration=cfg,current_type=types[owner],original_type=ot[owner],all_methods=methods,original_methods=[od[m['method']] for m in methods],all_shipping_setter_callers=writers,fit_method=fit,cache_insertion_instruction=stores[0],cctor=cctor,original_cctor=od[cctor['method']],module_type=types[base],original_module_type=ot[base]))
for name,value in [('constructed-owner-proofs-v1.json',dict(configurations=configs,original_queries=0)),('compiled-method-deltas-v1.json',dict(configurations=deltas,original_queries=0)),('host-cache-contexts-v1.json',dict(source_support=current_support,original_support=old_support,current_whole_declaration=span(text,current_support),original_whole_declaration=span(before,old_support),source=pin(ROOT/source),original_source=snap,current_view=cv,original_view=ov,properties=props,configurations=handles,Windows_owned=['ConcurrentDictionary','Guid','UtcNow','Alpha default and setter','DataObj/psmData','Excel errors/wizard/Err/context'],original_queries=0))]:
 with (out/name).open('x',encoding='utf-8') as f:json.dump(value,f,indent=2)
print('Exact5 native changes/7 APIs/one private ctor; all reached owners and native PSM state authenticated')
