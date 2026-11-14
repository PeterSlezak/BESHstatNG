"""Use unchanged authenticated C7 actual-handle metadata reader."""
from common import *
rows=[]
for name in ['MethodMetadata-v11.cs','native-metadata-owners-v2.txt']:
    with (HERE/name).open('xb') as f:f.write((PREVIOUS/name).read_bytes())
    rows.append(dict(before=pin(PREVIOUS/name),after=pin(HERE/name)))
save('metadata-tool-crosswalk-v1.json',dict(rows=rows,original_activity='PASSIVE_RETAINED_DLL_METADATA_ONLY_NO_METHOD_OR_TYPE_INITIALIZER_EXECUTION',original_queries=0))
