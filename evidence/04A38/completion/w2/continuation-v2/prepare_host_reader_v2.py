from common import *
s=(HERE/'verify_host_cache.py').read_text(encoding='utf-8').replace("'TryGetFactorHandle'","'TryGetFaHandle'")
with (HERE/'verify_host_cache_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('host-reader-scope-addendum.json',dict(previous='verify_host_cache.py',previous_observed_methods=32,expanded_name='TryGetFaHandle',reason='Native method is TryGetFaHandle, not descriptive Factor name. Prior32-body proof retained as a narrower valid scope; new33-body proof also checks this actual method.',original_queries=0))
