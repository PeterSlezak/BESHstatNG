"""Independent field/cache/late-copy and individual source-fragment predicates."""
import re
from source_views import view
from proof_kernel_v2 import need, definitions, semantic
from verify_c10_composition_v1 import tokens

def inspect(load,raw,prefix,original,u,contracts,portable,contexts):
    host=load(prefix+'host-cache-contexts-v1.json');base='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs'
    def span(text,row):return text.encode('utf-16-le')[row['span_start']*2:(row['span_start']+row['span_length'])*2].decode('utf-16-le')
    need({r['source_support']['declaration'] for r in host['rows']}=={'Private Class PcaHandle','Private Class FactorAnalysisHandle'},'False host class set')
    for r in host['rows']:
        s=r['source_support'];o=r['original_support'];need(s in u['supports'] and o in original['supports'] and s['kind']==o['kind']=='ClassBlock','Wrong source class')
        text=raw(s['path']).decode('utf-8-sig').replace('\r\n','\n');oldtext,normalization=view(raw(r['original_source']['path']),next(v['sha256'] for v in original['sources'] if v['path']==o['path']))
        need(span(text,s)==r['current_whole_declaration'] and span(oldtext,o)==r['original_whole_declaration'] and normalization==r['original_view'],'Class declaration/span forged')
        need(span(text,s)==span(oldtext,o).replace('\r\n','\n'),'Handle declaration changed')
        name=s['declaration'].split()[-1];children=[v for v in u['supports'] if v['owner']==base+'.'+name]
        need(children==r['children'] and len(children)==7 and all(v['kind']=='PropertyStatement' for v in children),'Missing actual handle child')
        need([re.search(r'Property (\w+)',v['declaration'])[1] for v in children]==r['properties'],'Forged handle property set')
        for config,meta in contexts.items():
            defs=definitions(meta);old=load(prefix+'metadata-all/packet-v2-context-v5/original-'+config+'.json');od=definitions(old)
            nt={t['type']:t for t in meta['types'] if t['type']};ot={t['type']:t for t in old['types'] if t['type']};owner=base+'+'+name
            row=next(v for v in r['configurations'] if v['configuration']==config)
            need(row['type']==nt[owner]==row['original_type']==ot[owner],'Handle type/field context forged')
            need({f['name'] for f in nt[owner]['fields']}=={'_'+p for p in r['properties']} and not any(f['is_static'] for f in nt[owner]['fields']),'Unexpected field/initializer')
            methods=[m for m in meta['methods'] if m['declaring_type']==owner];before=[m for m in old['methods'] if m['declaring_type']==owner]
            need(methods==row['methods'] and before==row['original_methods'] and len(methods)==15 and all(semantic(m)==semantic(od[m['method']]) for m in methods),'Missing/changed accessor or constructor')
            setters={m['method'] for m in methods if m['member_name'].startswith('set_')}
            actual=[dict(caller=m['method'],instruction=i,op=x['op'],target=x['operand'][7:]) for m in meta['methods'] for i,x in enumerate(m['instructions']) if x['op'] in ['call','callvirt'] and isinstance(x['operand'],str) and x['operand'].startswith('METHOD:') and x['operand'][7:] in setters]
            fit='PCA_FIT' if name=='PcaHandle' else 'FA_FIT';parent=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(base,fit));stores=[i for i,x in enumerate(parent['instructions']) if x['op']=='callvirt' and isinstance(x['operand'],str) and '.set_Item(' in x['operand'] and 'ConcurrentDictionary' in x['operand']]
            need(actual==row['all_shipping_setter_callers'] and len(actual)==7 and len(stores)==1 and all(c['caller']==parent['method'] and c['instruction']<stores[0] for c in actual),'Handle mutation/publication order changed')
    for config,meta in contexts.items():
        defs=definitions(meta);old=load(prefix+'metadata-all/packet-v2-context-v5/original-'+config+'.json');od=definitions(old);r=next(v for v in host['configurations'] if v['configuration']==config)
        for label,key in [('module_type','original_module_type')]:need(r[label]==r[key]==next(t for t in meta['types'] if t['type']==base)==next(t for t in old['types'] if t['type']==base),'Native cache fields changed')
        need(r['cctor']==defs[r['cctor']['method']] and r['original_cctor']==od[r['original_cctor']['method']] and semantic(r['cctor'])==semantic(r['original_cctor']),'Native cache cctor changed')
        fa=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(base,'FA_EIGEN'));api=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==('BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38','BuildFactorEigen'))
        prior=load('evidence/04A38/completion/w2/continuation-v9/metadata-all/packet-v1-context-v5/current-'+config+'.json');p=next(m for m in prior['methods'] if (m['declaring_type'],m['member_name'])==(base,'FA_EIGEN'))
        def copies(m):return [i for i,x in enumerate(m['instructions']) if x['op']=='call' and isinstance(x['operand'],str) and 'RuntimeHelpers' in x['operand'] and '.GetObjectValue(' in x['operand']]
        need(r['FA_EIGEN']==fa and r['portable_eigen']==api and r['prior_eigen']==p,'Wrong late-header consumer')
        last=max(i for i,x in enumerate(api['instructions']) if isinstance(x['operand'],str) and '.Cumulative(' in x['operand'])
        need(len(copies(p))==2 and len(copies(fa))==len(copies(api))==1 and copies(api)[0]>last and api['parameter_types'][1]=='System.Object&','ByRef or late Object-copy timing drift')
        targets=[defs[x['operand'][7:]] for x in fa['instructions'] if x['op']=='ldftn'];need(targets==r['named_delegate_targets'] and {(m['declaring_type'],m['member_name']) for m in targets}=={('BESHStatNG.Matrix.MatrixArithmeticCore','Trace'),('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','GetOptionalBool')},'Fake callback root')
    # Rebuild exact matched fragment IDs from authentic original/current records,
    # limiting the pool to this parent and its actually reached portable bodies.
    from collections import Counter
    rows=load(prefix+'new-parent-fragment-crosswalk-v1.json')['rows'];cs={c['original_id']:c for c in contracts if c['new_support']};ofs={f['fragment_id']:f for f in original['fragments']}
    for r in rows:
        f=ofs[r['fragment_id']];c=cs[r['original_parent']];need(r['original_source_record']==f and f['member_id']==c['original_record']['symbol_id'] and r['current_parent']==c['current_id'],'Fragment source/parent substitution')
        pool=[v for v in u['fragments'] if v['member_id']==c['current_id']];graph=set(c['compiled'][0]['transitive']['methods'])
        for p in portable:
            if p['compiled'][0]['method']['method'] in graph:pool.extend(p['fragments'])
        # Full authenticated phase reconstruction validates the explicit bound
        # source; this independently checks every advertised target and arm.
        matches=[v['fragment_id'] for v in pool if v['kind']==f['kind'] and any(tokens(v['code'])==tokens(t) for t in [f['code'],r['bound_source']])]
        need(Counter(matches)==Counter(r['matched_current_fragments']),'Fabricated/omitted fragment correspondence')
        need(matches or not (f['is_branch'] or f['is_inline']),'Unproved branch/inline arm')
    return dict(host_classes=2,configurations=2,original_fragments=len(rows),unmatched_branch_inline=0,original_queries=0)
