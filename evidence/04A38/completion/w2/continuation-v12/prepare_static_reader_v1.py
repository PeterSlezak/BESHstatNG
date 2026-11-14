"""Version the independently executed C11 verifier for C12 exact schemas and ownership."""
from common import *
source=PREVIOUS/'verify_c11_checkpoint_template_v3.py';value=source.read_text(encoding='utf-8')
value=value.replace('from proof_kernel_v3 import','from proof_kernel_v4 import')
value=value.replace('from verify_composition_v2 import inspect as composition,tokens,PHASES,unwrap','from verify_c12_composition_v1 import inspect as composition,tokens,PHASES,unwrap')
value=value.replace("OLD='evidence/04A38/completion/w2/continuation-v10/'","OLD='evidence/04A38/completion/w2/continuation-v11/'")
value=value.replace('from verify_c10_checkpoint_v1 import compute as prior_compute','from verify_c11_checkpoint_v2 import compute as prior_compute')
value=value.replace('from verify_source_transition_v1 import','from verify_c12_source_transition_v1 import').replace('from verify_host_contexts_v1 import','from verify_c12_host_contexts_v1 import').replace('from verify_census_v1 import','from verify_c12_census_v1 import').replace('from verify_c11_validation_v1 import','from verify_c12_validation_v1 import')
value=value.replace('==4,\'Missing/ambiguous source transition\'','==6,\'Missing/ambiguous source transition\'')
value=value.replace('closed=369,OPEN=335','closed=402,OPEN=302').replace('closed=399,OPEN=327','closed=432,OPEN=294').replace('len(inherited)==399','len(inherited)==432')
value=value.replace("PREFIX+'current-universe-packet-v2.json'","PREFIX+'current-universe-packet-v1.json'")
value=value.replace("PREFIX+'declaration-shapes-v2.json'","PREFIX+'declaration-shapes-v1.json'").replace("PREFIX+'cohort-diagnostic-v2.json'","PREFIX+'cohort-diagnostic-v1.json'")
value=value.replace('len(cs)==86 and len(callbacks)==10','len(cs)==82 and len(callbacks)==18').replace('==96,\'Overlapping body contract\'','==100,\'Overlapping body contract\'')
value=value.replace("sum(c['new_support'] for c in callbacks)==2","sum(c['new_support'] for c in callbacks)==7")
value=value.replace("PREFIX+'metadata-all/packet-v2-context-v5/","PREFIX+'metadata-all/packet-v1-context-v5/")
value=value.replace("changed=={('BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs',n) for n in PHASES}","changed=={(members[c['current_id']]['owner'],members[c['current_id']]['name']) for c in cs if members[c['current_id']]['name'] in PHASES}")
value=value.replace("{('BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38',v[0]) for v in PHASES.values()}","{('BESHStatNG.ExcelCommon.'+v['owner'],v['api']) for v in PHASES.values()}")
value=value.replace("len(graph['methods'])==901","len(graph['methods'])==795")
value=value.replace("PREFIX+'return-metadata-'+config+'-v2.json'","PREFIX+'return-metadata-'+config+'-v1.json'")
value=value.replace("PREFIX+'inspection/packet-v2/","PREFIX+'inspection/packet-v1/")
value=value.replace("PREFIX+'compatibility-packet-v2.json'","PREFIX+'compatibility-packet-v1.json'")
value=value.replace("PREFIX+'captures/packet-v2/","PREFIX+'captures/packet-v1/")
value=value.replace('C11 source/ownership','C12 source/ownership').replace('Sealed C11 input','Sealed C12 input')
for name in ['verify_c12_checkpoint_template_v1.py','verify_c12_checkpoint_v1.py']:
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
# Same fully independent cache/source reconstruction; replace only the distinct cohort and five measured copy boundaries.
source2=PREVIOUS/'verify_host_contexts_v1.py';host=source2.read_text(encoding='utf-8')
host=host.replace('from proof_kernel_v3 import','from proof_kernel_v4 import').replace('packet-v2-context-v5','packet-v1-context-v5')
a=host.index("    expected={'FactorAnalysisHandle'");b=host.index('\n    need(',a)
host=host[:a]+"    expected={'CorrespondenceHandle':('CA_FIT',4),'MultipleCorrespondenceHandle':('MCA_FIT',4),'DiscriminantHandle':('DA_FIT',14)}"+host[b:]
a=host.index('        def copies(m):');host=host[:a]+'''        previous=load('evidence/04A38/completion/w2/continuation-v11/metadata-all/packet-v2-context-v5/current-'+config+'.json')
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
'''
with (HERE/'verify_c12_host_contexts_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(host)
save('static-reader-source-crosswalk-v1.json',dict(checkpoint_before=pin(source),checkpoint_after=pin(HERE/'verify_c12_checkpoint_v1.py'),host_before=pin(source2),host_after=pin(HERE/'verify_c12_host_contexts_v1.py'),
    scope='C12 exact stage/count/source/parameter/metadata pointers, two portable classes, five copy boundaries, three handles and unchanged fail-closed predecessor replay. Unique C12 verification module names avoid v11 module collisions.',original_queries=0))
