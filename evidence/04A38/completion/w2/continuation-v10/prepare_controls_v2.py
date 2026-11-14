"""Add actual compiled-byte and independent field/fragment mutations without source edits."""
from common import *
source=(HERE/'seal_and_controls_v1.py').read_text(encoding='utf-8')
point="    save('independent-controls-v1.json'"
addition="""    path='.test-rationalization-04A38.tmp/completion-w2-cont10/packet-v2/Debug/lib/BESHStat.Excel.Common.dll'
    reject('actual compiled consumer bytes changed',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'}))
    from verify_host_fragments_v1 import inspect as hostcheck
    universe=load(prefix+'current-universe-packet-v2.json');original=load('evidence/04A38/completion/w1/original-universe.json');contracts=load(prefix+'whole-body-contracts-v1.json')['contracts'];portable=load(prefix+'portable-api-contracts-v1.json')['contracts'];contexts={c:load(prefix+'metadata-all/packet-v2-context-v5/current-'+c+'.json') for c in ['Debug','Release']}
    forged=load(prefix+'host-cache-contexts-v1.json');forged['rows'][0]['configurations'][0]['type']['fields']=[]
    reject('pure host cache field removed',lambda:hostcheck(lambda p:forged if p==prefix+'host-cache-contexts-v1.json' else load(p),raw,prefix,original,universe,contracts,portable,contexts))
    forged_frag=load(prefix+'new-parent-fragment-crosswalk-v1.json');forged_frag['rows'][0]['matched_current_fragments']=['wrong-fragment']
    reject('pure actual fragment target forged',lambda:hostcheck(lambda p:forged_frag if p==prefix+'new-parent-fragment-crosswalk-v1.json' else load(p),raw,prefix,original,universe,contracts,portable,contexts))
"""
assert point in source;source=source.replace(point,addition+point)
with (HERE/'seal_and_controls_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('controls-source-crosswalk-v2.json',dict(before=pin(HERE/'seal_and_controls_v1.py'),after=pin(HERE/'seal_and_controls_v2.py'),scope='Only extend the adverse harness; final executable and production/test/static proof inputs remain unchanged. Actual consumer bytes are virtually mutated, never written; field and fragment checks run independently of outer hash authentication.'))
