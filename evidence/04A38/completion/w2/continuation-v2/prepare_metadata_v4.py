"""Preserve constructed generic declaring-type identities in resolved operands."""
from common import *
s=(HERE/'MethodMetadata-v3.cs').read_text(encoding='utf-8');old='m.DeclaringType.FullName+" @ "+AssemblyNameOf(m.DeclaringType)';assert old in s
s=s.replace(old,'TypeName(m.DeclaringType)').replace('schema="PASSIVE_METADATA_V3"','schema="PASSIVE_METADATA_V4"')
with (HERE/'MethodMetadata-v4.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'run_metadata_v3.py').read_text(encoding='utf-8').replace('metadata-v3','metadata-v4').replace('MethodMetadata-v3.cs','MethodMetadata-v4.cs').replace("HERE/'metadata'/stage","HERE/'metadata-v4'/stage")
with (HERE/'run_metadata_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'verify_host_cache_v2.py').read_text(encoding='utf-8').replace("base+'/metadata/b1-v1/'","base+'/metadata-v4/b1-v1/'")
with (HERE/'verify_host_cache_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('metadata-generic-identity-addendum.json',dict(status='STRONGER_PASSIVE_READER_REQUIRED',old='MethodMetadata-v3.cs',new='MethodMetadata-v4.cs',reason='Constructed generic DeclaringType.FullName can be Nothing; v4 uses the existing TypeName fallback/assembly identity rather than dropping the owner. Actual host method identities/counts unchanged; prior tool outputs retained, fresh original metadata only/current reads required.',original_semantic_queries=0))
