"""Independent reconstruction of native handle fields, setters, constructors and copies."""
import re
from source_views import view
from proof_kernel_v6 import need,definitions,semantic

def inspect(load,raw,prefix,original,u,contexts):
    host=load(prefix+'host-cache-contexts-v1.json');base='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs'
    expected={'PcaHandle':('PCA_FIT',7),'FactorAnalysisHandle':('FA_FIT',7),'KMeansHandle':('KMEANS_FIT',14),'HierarchicalHandle':('HCLUST_FIT',9),'CorrespondenceHandle':('CA_FIT',4),'MultipleCorrespondenceHandle':('MCA_FIT',4),'DiscriminantHandle':('DA_FIT',14)}
    need({r['source_support']['declaration'].split()[-1] for r in host['rows']}==set(expected),'Wrong complete native handle cohort')
    def span(text,row):return text.encode('utf-16-le')[row['span_start']*2:(row['span_start']+row['span_length'])*2].decode('utf-16-le')
    for r in host['rows']:
        s=r['source_support'];o=r['original_support'];name=s['declaration'].split()[-1];fit,count=expected[name]
        need(s in u['supports'] and o in original['supports'] and s['kind']==o['kind']=='ClassBlock','Wrong source class identity')
        text,policy=view(raw(s['path']),next(x['sha256'] for x in u['sources'] if x['path']==s['path']))
        old,oldpolicy=view(raw(r['original_source']['path']),next(x['sha256'] for x in original['sources'] if x['path']==o['path']))
        need(span(text,s)==r['current_whole_declaration'] and span(old,o)==r['original_whole_declaration'] and policy==r['current_view'] and oldpolicy==r['original_view'],'Forged class/span/source view')
        need(span(text,s).replace('\r\n','\n')==span(old,o).replace('\r\n','\n'),'Native instance declaration changed')
        children=[v for v in u['supports'] if v['owner']==base+'.'+name]
        need(children==r['children'] and len(children)==count and all(v['kind']=='PropertyStatement' for v in children),'Missing handle property declaration')
        need([re.search(r'Property (\w+)',v['declaration'])[1] for v in children]==r['properties'],'Wrong property set')
        for config,meta in contexts.items():
            original_meta=load(prefix+'metadata-all/packet-v1-context-v5/original-'+config+'.json')
            defs=definitions(meta);od=definitions(original_meta);owner=base+'+'+name
            t=next(t for t in meta['types'] if t['type']==owner);ot=next(t for t in original_meta['types'] if t['type']==owner)
            entry=next(v for v in r['configurations'] if v['configuration']==config)
            need(entry['type']==t==entry['original_type']==ot and {f['name'] for f in t['fields']}=={'_'+p for p in r['properties']} and not any(f['is_static'] for f in t['fields']),'Native backing field context forged')
            methods=[m for m in meta['methods'] if m['declaring_type']==owner];before=[m for m in original_meta['methods'] if m['declaring_type']==owner]
            need(entry['methods']==methods and entry['original_methods']==before and len(methods)==2*count+1 and all(semantic(m)==semantic(od[m['method']]) for m in methods),'Accessor/constructor omitted or changed')
            setters={m['method'] for m in methods if m['member_name'].startswith('set_')}
            writers=[dict(caller=m['method'],instruction=i,target=x['operand'][7:]) for m in meta['methods'] for i,x in enumerate(m['instructions']) if x['op'] in ['call','callvirt'] and isinstance(x['operand'],str) and x['operand'].startswith('METHOD:') and x['operand'][7:] in setters]
            parent=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(base,fit))
            stores=[i for i,x in enumerate(parent['instructions']) if x['op']=='callvirt' and isinstance(x['operand'],str) and '.set_Item(' in x['operand'] and 'ConcurrentDictionary' in x['operand']]
            need(writers==entry['all_shipping_setter_callers'] and len(writers)==count and len(stores)==1 and all(w['caller']==parent['method'] and w['instruction']<stores[0] for w in writers),'Unexpected writer/publication order')
    for config,meta in contexts.items():
        old=load(prefix+'metadata-all/packet-v1-context-v5/original-'+config+'.json')
        defs=definitions(meta);od=definitions(old);r=next(v for v in host['configurations'] if v['configuration']==config)
        need(r['module_type']==r['original_module_type']==next(t for t in meta['types'] if t['type']==base)==next(t for t in old['types'] if t['type']==base),'Native module/cache fields changed')
        need(r['cctor']==defs[r['cctor']['method']] and r['original_cctor']==od[r['original_cctor']['method']] and semantic(r['cctor'])==semantic(r['original_cctor']),'Native cache initialization changed')
        previous=definitions(load('evidence/04A38/completion/w2/continuation-v12/metadata-all/packet-v1-context-v5/current-'+config+'.json'))
        def copies(m):return [i for i,x in enumerate(m['instructions']) if x['op']=='call' and isinstance(x['operand'],str) and 'RuntimeHelpers' in x['operand'] and '.GetObjectValue(' in x['operand']]
        need(len(r['boundaries'])==8,'Missing exact native copy boundary')
        for b in r['boundaries']:
            n=defs[b['native']['method']];p=defs[b['portable']['method']];old=previous[n['method']]
            need(n==b['native'] and p==b['portable'] and old==b['previous_native'],'Forged compiled host boundary')
            need(b['previous_copy_sites']==copies(old) and b['native_copy_sites']==copies(n) and b['portable_copy_sites']==copies(p) and len(copies(old))==len(copies(n))+len(copies(p)),'Changed VB boxed object copy')
            targets=[defs[x['operand'][7:]] for x in n['instructions'] if x['op']=='ldftn' and isinstance(x['operand'],str) and x['operand'].startswith('METHOD:') and x['operand'][7:] in defs]
            need(targets==b['actual_direct_policy_targets'],'Named/callback host target mismatch')
    return dict(host_types=7,new_support_types=0,configurations=2,exact_copy_boundaries=8)
