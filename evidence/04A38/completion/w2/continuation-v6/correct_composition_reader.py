from common import *
source=(HERE/'verify_source_composition_v1.py').read_text(encoding='utf-8')
old="block(member(inference,'SplitCorrelationColumns','Sub'),'Sub'))"
new="block(member(inference,'SplitCorrelationColumns','Sub'),'Sub').replace('For i As Integer', 'For i'))"
assert source.count(old)==1;source=source.replace(old,new,1)
with (HERE/'verify_source_composition_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('attempts/composition-reader-v1.json',dict(status='SOURCE_RECONSTRUCTION_GUARD_RETAINED',reader=pin(HERE/'verify_source_composition_v1.py'),
    failure='Exact source-token guard rejected explicit For i As Integer versus original inferred For i; both are Int32 loops over the same Int32 count.',
    correction='Normalize only that explicit Int32 declaration in the pair-column reconstruction. Keep all expressions, array allocation/order and complete metadata checks.',production_changes=0,original_queries=0))
from verify_source_composition_v2 import inspect
result=inspect();save('source-composition-proof-v1.json',result);print(json.dumps(result))
