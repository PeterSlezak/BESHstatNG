"""Version unchanged offline tools; record every source/version transition."""
from common import *

rows=[]
for name in ['build_v1.py','inspect_stage_v1.py','compatibility_v1.py','inspect_metadata_v5.py',
             'declaration_shapes_v1.py','return_metadata_v1.py']:
    source=(PREVIOUS/name).read_text(encoding='utf-8')
    source=source.replace("PREVIOUS/'metadata-all/packet-v1-context-v5'", "PREVIOUS/'metadata-all/packet-v2-context-v5'")
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:
        stream.write(source)
    rows.append(dict(before=pin(PREVIOUS/name),after=pin(HERE/name)))
kernel=(HERE/'proof_kernel_v2.py').read_text(encoding='utf-8')
kernel=kernel.replace('{"PcaHandle", "FactorAnalysisHandle"}',
    '{"PcaHandle", "FactorAnalysisHandle", "KMeansHandle", "HierarchicalHandle", "CorrespondenceHandle"}')
assert kernel!=(HERE/'proof_kernel_v2.py').read_text(encoding='utf-8')
with (HERE/'proof_kernel_v3.py').open('x',encoding='utf-8',newline='\n') as stream:
    stream.write(kernel)
save('tooling-carryforward-v1.json',dict(files=rows,kernel_before=pin(HERE/'proof_kernel_v2.py'),
    kernel_after=pin(HERE/'proof_kernel_v3.py'),change='Explicit declared native nested handle parameter types for this cohort. Preserve all rank/ByRef/owner checks. Prior sealed readers and tool bytes remain unchanged.',original_queries=0))
print('C11 offline current-build/metadata/census/frozen tools versioned')
