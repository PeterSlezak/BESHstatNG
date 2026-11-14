from common import *
from source_views import body
import difflib
cs=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];old=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())['members']
ts=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']
for move in json.loads((HERE/'implementation-source-composition-v2.json').read_bytes())['phases']:
    if 'TryGetRocInput' not in move['parent']:continue
    node=next(c for c in cs if c['current_record']['name']=='TryGetRocInput');before=next(n for n in old if n['name']=='TryGetRocInput' and n['parent_member'] is None)
    t=next(t for t in ts if t['path']==move['path']);entry=body((ROOT/t['preimage']['path']).read_bytes(),before)[0]
    print('segment_count',entry.count(move['original_segment']))
    print(''.join(difflib.unified_diff(entry.replace(move['original_segment'],move['actual_native_call'],1).splitlines(True),node['current_whole_body'].splitlines(True))))
