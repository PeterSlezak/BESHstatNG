from common import *
s=PREVIOUS/'verify_c12_host_contexts_v1.py';v=s.read_text().replace('from proof_kernel_v4 import','from proof_kernel_v6 import')
a=v.index("    expected={");b=v.index('\n    need(',a)
v=v[:a]+"    expected={'PcaHandle':('PCA_FIT',7),'FactorAnalysisHandle':('FA_FIT',7),'KMeansHandle':('KMEANS_FIT',14),'HierarchicalHandle':('HCLUST_FIT',9),'CorrespondenceHandle':('CA_FIT',4),'MultipleCorrespondenceHandle':('MCA_FIT',4),'DiscriminantHandle':('DA_FIT',14)}"+v[b:]
a=v.index('        previous=load(')
v=v[:a]+'''        previous=definitions(load('evidence/04A38/completion/w2/continuation-v12/metadata-all/packet-v1-context-v5/current-'+config+'.json'))
        def copies(m):return [i for i,x in enumerate(m['instructions']) if x['op']=='call' and isinstance(x['operand'],str) and 'RuntimeHelpers' in x['operand'] and '.GetObjectValue(' in x['operand']]
        need(len(r['boundaries'])==8,'Missing exact native copy boundary')
        for b in r['boundaries']:
            n=defs[b['native']['method']];p=defs[b['portable']['method']];old=previous[n['method']]
            need(n==b['native'] and p==b['portable'] and old==b['previous_native'],'Forged compiled host boundary')
            need(b['previous_copy_sites']==copies(old) and b['native_copy_sites']==copies(n) and b['portable_copy_sites']==copies(p) and len(copies(old))==len(copies(n))+len(copies(p)),'Changed VB boxed object copy')
            targets=[defs[x['operand'][7:]] for x in n['instructions'] if x['op']=='ldftn' and isinstance(x['operand'],str) and x['operand'].startswith('METHOD:') and x['operand'][7:] in defs]
            need(targets==b['actual_direct_policy_targets'],'Named/callback host target mismatch')
    return dict(host_types=7,new_support_types=0,configurations=2,exact_copy_boundaries=8)
'''
with (HERE/'verify_c13_host_contexts_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('host-reader-source-crosswalk-v1.json',dict(before=pin(s),after=pin(HERE/'verify_c13_host_contexts_v1.py'),
    correction='Writer stdout retained inherited three/five wording. Its actual receipt and independently rebuilt checks cover seven handles and eight exact copy boundaries. No support class is newly counted.',original_queries=0))
