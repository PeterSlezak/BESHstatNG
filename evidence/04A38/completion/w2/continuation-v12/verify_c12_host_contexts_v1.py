"""Independent reconstruction of native handle fields, setters, constructors and copies."""
import re
from source_views import view
from proof_kernel_v4 import need,definitions,semantic

def inspect(load,raw,prefix,original,u,contexts):
    host=load(prefix+'host-cache-contexts-v1.json');base='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs'
    expected={'CorrespondenceHandle':('CA_FIT',4),'MultipleCorrespondenceHandle':('MCA_FIT',4),'DiscriminantHandle':('DA_FIT',14)}
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
        previous=load('evidence/04A38/completion/w2/continuation-v11/metadata-all/packet-v2-context-v5/current-'+config+'.json')
        before=definitions(previous)
        def copies(m):return [i for i,x in enumerate(m['instructions']) if x['op']=='call' and isinstance(x['operand'],str) and 'RuntimeHelpers' in x['operand'] and '.GetObjectValue(' in x['operand']]
        need(len(r['boundaries'])==5,'Missing copy/consumer boundary')
        expected_policies={'TryGetCategoricalMatrix':['IsBlankCell','CellToTrimmedText','ResolveImportLabelNames'],'TryGetStringVector':['CellToTrimmedText'],'TryGetDoubleVector':['TryGetDouble'],'MCA_SUMMARY':[],'DA_SUMMARY':[]}
        need({b['native']['member_name'] for b in r['boundaries']}==set(expected_policies),'Unexpected native copy boundary')
        for boundary in r['boundaries']:
            native=defs[boundary['native']['method']];prior=before[native['method']];portable=defs[boundary['portable']['method']]
            need(boundary['native']==native and boundary['previous_native']==prior and boundary['portable']==portable,'Wrong compiled boundary bytes')
            need(boundary['previous_copy_sites']==copies(prior) and boundary['native_copy_sites']==copies(native) and boundary['portable_copy_sites']==copies(portable),'Copy sites forged')
            need(len(copies(prior))==len(copies(native))+len(copies(portable)),'Eager/extra boxed-value copy')
            if native['member_name']=='TryGetCategoricalMatrix':need(portable['parameter_types'][1]=='System.Object&','Categorical metadata Object lost ByRef')
            targets=[defs[x['operand'][7:]] for x in native['instructions'] if x['op']=='ldftn' and isinstance(x['operand'],str) and x['operand'].startswith('METHOD:')]
            need(targets==boundary['named_policy_targets'] and [t['member_name'] for t in targets]==expected_policies[native['member_name']],'Changed named host policy')
    return dict(host_types=3,new_support_types=2,configurations=2,exact_copy_boundaries=5)
