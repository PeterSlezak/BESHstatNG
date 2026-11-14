"""Version exact validators and compact the complete fixture index without omissions."""
from common import *
def install(name,value):
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(value)
value=(PREVIOUS/'bind_current_validation_v1.py').read_text().replace('from proof_kernel_v4 import','from proof_kernel_v6 import').replace('w2c12','w2c13').replace('fresh C12','fresh C13')
install('bind_current_validation_v1.py',value)
value=(PREVIOUS/'verify_c12_validation_v1.py').read_text().replace('from proof_kernel_v4 import','from proof_kernel_v6 import').replace('continuation-v11/','continuation-v12/').replace('==20848 and len(prior)==20817','==20888 and len(prior)==20848').replace('==31','==40').replace('Passed=20834','Passed=20874').replace('new_managed_PASS=31','new_managed_PASS=40')
install('verify_c13_validation_v1.py',value)
value=(PREVIOUS/'fixture_crosswalk_v1.py').read_text().replace('from proof_kernel_v4 import','from proof_kernel_v6 import').replace("methods=meta['methods']","methods=meta['methods'];methodkeys=sorted(m['method'] for m in methods);methodindex={k:i for i,k in enumerate(methodkeys)}").replace("compiled_graphs=len(catalog)","compiled_graphs=len(catalog)")
value=value.replace("catalog.append(dict(configuration=config,target_roots=roots,graph=graph,metadata=pin(HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json'))))","catalog.append(dict(configuration=config,target_roots=roots,reached_method_indexes=[methodindex[k] for k in graph['methods']],metadata=pin(HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json'))))")
value=value.replace("compiled_graph_catalog=catalog,counts=counts,","compiled_graph_catalog=catalog,method_catalogs={c:sorted(m['method'] for m in json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+c+'.json')).read_bytes())['methods']) for c in ['Debug','Release']},counts=counts,")
value=value.replace("PREVIOUS/'compatibility-fixture-crosswalk-v2.json'","PREVIOUS/'compatibility-fixture-crosswalk-v1.json'").replace('C12_SHARED','C13_SHARED').replace('old C11','old C12')
install('fixture_crosswalk_v1.py',value)
save('remaining-validator-source-crosswalk-v1.json',dict(sources=[pin(HERE/n) for n in ['bind_current_validation_v1.py','verify_c13_validation_v1.py','fixture_crosswalk_v1.py']],
    scope='Exact current-only shipping and identity checks, measured40 test additions. Fixture catalog retains all operations and complete method reachability using shared integer references; full graph edges remain in exact metadata.',original_queries=0))
