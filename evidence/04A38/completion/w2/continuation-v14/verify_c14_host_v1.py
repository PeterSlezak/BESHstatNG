"""Recompute native PSM constructed storage, default, writer and publication order."""
from proof_kernel_v8 import definitions,semantic,need
from source_views import view

def inspect(raw,load,prefix):
 proof=load(prefix+'host-cache-contexts-v1.json');u=load(prefix+'current-universe-packet-v1.json');o=load('evidence/04A38/completion/w1/original-universe.json')
 source=proof['source'];data=raw(source['path']);text,_=view(data,next(s['sha256'] for s in u['sources'] if s['path']==source['path']))
 original=proof['original_source'];before,_=view(raw(original['path']),next(s['sha256'] for s in o['sources'] if s['path']==source['path']))
 def declaration(t,s):return t.encode('utf-16-le')[s['span_start']*2:(s['span_start']+s['span_length'])*2].decode('utf-16-le')
 for key,pop,t in [('source_support',u,text),('original_support',o,before)]:
  s=proof[key];need(s in pop['supports'],'Wrong PSM source declaration identity');actual=declaration(t,s);need(actual==proof['current_whole_declaration' if key=='source_support' else 'original_whole_declaration'],'PSM whole source declaration mismatch')
 need(proof['current_whole_declaration'].replace('\r\n','\n')==proof['original_whole_declaration'].replace('\r\n','\n'),'PSM constructed source state changed')
 base='BESHStatNG.WorksheetFunctions.PropensityScoreUDFs';owner=base+'+PsmUdfHandle';results=[]
 for cfg in ['Debug','Release']:
  m=load(prefix+'metadata-all/packet-v1-context-v5/current-'+cfg+'.json');old=load(prefix+'metadata-all/packet-v1-context-v5/original-'+cfg+'.json');defs=definitions(m);od=definitions(old);types={t['type']:t for t in m['types']};ot={t['type']:t for t in old['types']};r=next(r for r in proof['configurations'] if r['configuration']==cfg)
  need(types[base]==ot[base]==r['module_type']==r['original_module_type'] and types[owner]==ot[owner]==r['current_type']==r['original_type'],'PSM original/native fields changed')
  methods=[x for x in m['methods'] if x['declaring_type']==owner];need(methods==r['all_methods'] and len(methods)==13 and all(semantic(x)==semantic(od[x['method']]) for x in methods),'PSM accessor/default constructor semantic mismatch')
  props=[s for s in u['supports'] if s['owner']==base+'.PsmUdfHandle'];names=[s['declaration'].split('Property ',1)[1].split()[0] for s in props];need(len(props)==6 and props==proof['properties'] and {f['name'] for f in types[owner]['fields']}=={'_'+n for n in names},'PSM source/storage six-property crosswalk mismatch')
  setters={'METHOD:'+x['method'] for x in methods if x['member_name'].startswith('set_')};writers=[dict(caller=x['method'],instruction=i,target=ins['operand'][7:]) for x in m['methods'] for i,ins in enumerate(x['instructions']) if ins['op'] in ['call','callvirt'] and ins.get('operand') in setters]
  fit=next(x for x in m['methods'] if (x['declaring_type'],x['member_name'])==(base,'PS_FIT'));ctor=next(x for x in methods if x['member_name']=='.ctor');stores=[i for i,x in enumerate(fit['instructions']) if x['op']=='callvirt' and '.set_Item(' in (x.get('operand') or '') and 'ConcurrentDictionary' in x['operand']]
  need(writers==r['all_shipping_setter_callers'] and len(writers)==7 and len(stores)==1 and stores[0]==r['cache_insertion_instruction'] and fit==r['fit_method'],'Native complete writer/cache publication mismatch')
  need(sum(w['caller']==ctor['method'] for w in writers)==1 and sum(w['caller']==fit['method'] for w in writers)==6 and all(w['caller']==ctor['method'] or w['caller']==fit['method'] and w['instruction']<stores[0] for w in writers),'Native PSM partial-state/Alpha initialization/cache order changed')
  cctor=next(x for x in m['methods'] if (x['declaring_type'],x['member_name'])==(base,'.cctor'));need(cctor==r['cctor'] and semantic(cctor)==semantic(od[cctor['method']]),'Native dictionary/session initializer changed')
  results.append(dict(configuration=cfg,fields=6,accessors_and_constructor=13,setter_callers=7,fit_setters_before_cache=6))
 return dict(status='EXACT_NATIVE_SOURCE_CONSTRUCTED_STORAGE_AND_PUBLICATION_ORDER',configurations=results,original_queries=0)
