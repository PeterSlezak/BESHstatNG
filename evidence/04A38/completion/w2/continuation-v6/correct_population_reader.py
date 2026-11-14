from common import *
source=(HERE/'verify_checkpoint_v1.py').read_text(encoding='utf-8')
old="==Counter(members),'Current population omitted/duplicated'"
new="==Counter(members.keys()),'Current population omitted/duplicated'"
assert source.count(old)==1
source=source.replace(old,new)
with (HERE/'verify_checkpoint_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('attempts/population-reader-v1.json',dict(status='STRICT_READER_FALSE_REJECTION_RETAINED',
    reader=pin(HERE/'verify_checkpoint_v1.py'),failure='Counter(dict) interprets dictionary values as counts instead of counting identity keys. Exact pre-ledger recomputation passed; final identity comparison falsely rejected the actual complete724-row population.',
    correction='Count members.keys() explicitly. No ledger, proof, source or expected population changed.',reader_v2=pin(HERE/'verify_checkpoint_v2.py'),original_queries=0))
