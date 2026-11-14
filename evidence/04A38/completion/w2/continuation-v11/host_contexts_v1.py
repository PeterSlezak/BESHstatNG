"""Original/current cache instance types, all setters and actual publication/copy contexts."""
from common import *
from source_views import view
from proof_kernel_v3 import definitions,semantic,need
import re
u=json.loads((HERE/'current-universe-packet-v2.json').read_bytes())
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
base='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs'
path='BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb'
snapshot=next(r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files'] if r['source']['path']==path)
text,policy=view((ROOT/path).read_bytes(),next(s['sha256'] for s in u['sources'] if s['path']==path))
oldtext,oldpolicy=view((ROOT/snapshot['path']).read_bytes(),next(s['sha256'] for s in original['sources'] if s['path']==path))
def span(text,record):return text.encode('utf-16-le')[record['span_start']*2:(record['span_start']+record['span_length'])*2].decode('utf-16-le')
rows=[]
for name,fit,count in [('FactorAnalysisHandle','FA_FIT',7),('KMeansHandle','KMEANS_FIT',14),('HierarchicalHandle','HCLUST_FIT',9),('CorrespondenceHandle','CA_FIT',4)]:
    current=next(s for s in u['supports'] if s['kind']=='ClassBlock' and s['owner']==base and s['declaration']=='Private Class '+name)
    before=next(s for s in original['supports'] if s['kind']=='ClassBlock' and s['owner']==base and s['declaration']=='Private Class '+name)
    children=[s for s in u['supports'] if s['owner']==base+'.'+name]
    need(len(children)==count and all(s['kind']=='PropertyStatement' for s in children),'Unresolved source handle child')
    need(span(text,current).replace('\r\n','\n')==span(oldtext,before).replace('\r\n','\n'),'Native handle source changed')
    rows.append(dict(source_support=current,original_support=before,current_whole_declaration=span(text,current),
        original_whole_declaration=span(oldtext,before),source=pin(ROOT/path),original_source=snapshot,
        current_view=policy,original_view=oldpolicy,children=children,
        properties=[re.search(r'Property (\w+)',s['declaration'])[1] for s in children],
        fit=fit,configurations=[]))
configs=[]
for config in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v2-context-v5/current-'+config+'.json')).read_bytes())
    old=json.loads((HERE/('metadata-all/packet-v2-context-v5/original-'+config+'.json')).read_bytes())
    defs=definitions(meta);before=definitions(old);types={t['type']:t for t in meta['types'] if t['type']};oldtypes={t['type']:t for t in old['types'] if t['type']}
    need(types[base]==oldtypes[base],'Module cache fields changed')
    cctor=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(base,'.cctor'))
    oldcctor=next(m for m in old['methods'] if (m['declaring_type'],m['member_name'])==(base,'.cctor'))
    need(semantic(cctor)==semantic(oldcctor),'Cache initialization changed')
    for row in rows:
        owner=base+'+'+row['source_support']['declaration'].split()[-1]
        need(types[owner]==oldtypes[owner],'Host cache instance field/type changed')
        need({f['name'] for f in types[owner]['fields']}=={'_'+p for p in row['properties']} and not any(f['is_static'] for f in types[owner]['fields']),'Unresolved instance field')
        methods=[m for m in meta['methods'] if m['declaring_type']==owner]
        previous=[m for m in old['methods'] if m['declaring_type']==owner]
        need(len(methods)==2*len(row['properties'])+1 and {m['method'] for m in methods}=={m['method'] for m in previous},'Missing actual accessor/constructor')
        need(all(semantic(m)==semantic(before[m['method']]) for m in methods),'Native accessor/constructor changed')
        setters={m['method'] for m in methods if m['member_name'].startswith('set_')}
        writers=[dict(caller=m['method'],instruction=i,target=x['operand'][7:]) for m in meta['methods'] for i,x in enumerate(m['instructions'])
            if x['op'] in ['call','callvirt'] and isinstance(x['operand'],str) and x['operand'].startswith('METHOD:') and x['operand'][7:] in setters]
        fit=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(base,row['fit']))
        stores=[i for i,x in enumerate(fit['instructions']) if x['op']=='callvirt' and isinstance(x['operand'],str) and '.set_Item(' in x['operand'] and 'ConcurrentDictionary' in x['operand']]
        need(len(writers)==len(row['properties']) and len(stores)==1 and all(w['caller']==fit['method'] and w['instruction']<stores[0] for w in writers),'Handle mutated after publication or unresolved writer')
        row['configurations'].append(dict(configuration=config,type=types[owner],original_type=oldtypes[owner],
            methods=methods,original_methods=previous,all_shipping_setter_callers=writers,
            fit_method=fit['method'],cache_insertion_instruction=stores[0]))
    prior=json.loads((PREVIOUS/('metadata-all/packet-v2-context-v5/current-'+config+'.json')).read_bytes())
    h=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(base,'HCLUST_SUMMARY'))
    p=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==('BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38','BuildHierarchicalSummary'))
    oldh=next(m for m in prior['methods'] if (m['declaring_type'],m['member_name'])==(base,'HCLUST_SUMMARY'))
    def copies(m):return [i for i,x in enumerate(m['instructions']) if x['op']=='call' and isinstance(x['operand'],str) and 'RuntimeHelpers' in x['operand'] and '.GetObjectValue(' in x['operand']]
    need((len(copies(oldh)),len(copies(h)),len(copies(p)))==(4,3,1) and p['parameter_types'][8]=='System.Object&','Hierarchical final-height copy/slot changed')
    configs.append(dict(configuration=config,module_type=types[base],original_module_type=oldtypes[base],
        cctor=cctor,original_cctor=oldcctor,hierarchical_host=h,hierarchical_portable=p,prior_hierarchical=oldh,
        prior_copies=copies(oldh),native_copies=copies(h),portable_copies=copies(p)))
save('host-cache-contexts-v1.json',dict(rows=rows,configurations=configs,new_cache_policy=False,
    native_execution='NOT_RUN',original_queries=0,scope='Actual source-declared types, every backing field/accessor/constructor, all shipping setter callers before original FIT cache publication, unchanged module initializer, and exact4=3+1 final-height Object-copy boundary. Host module remains OPEN for unrelated children/state.'))
print('Four complete host handle contexts and all shipping writers/copy boundaries authenticated')
