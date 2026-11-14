"""Derive property cardinalities from the authenticated source declarations."""
from common import *
source=HERE/'prepare_host_contexts_v1.py'
value=source.read_text(encoding='utf-8')
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
count=len([s for s in original['supports'] if s['owner']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs.HierarchicalHandle'])
assert count==9
old="('HierarchicalHandle','HCLUST_FIT',10)";assert value.count(old)==1
value=value.replace(old,"('HierarchicalHandle','HCLUST_FIT',"+str(count)+")")
target=HERE/'prepare_host_contexts_source_v2.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('host-context-count-source-reconciliation-v1.json',dict(unexecuted_provisional_source=pin(source),source_backed_successor=pin(target),
    actual_original_property_count=count,source='Authenticated original-universe.json support declarations for the exact HierarchicalHandle owner.',
    scope='Corrected provisional count before any host-context proof execution; no failed source/model/compiled verification is relabelled.',original_queries=0))
result=command('prepare-host-context-source-v2',[sys.executable,'-B',str(target)]);assert result['exit_code']==0
