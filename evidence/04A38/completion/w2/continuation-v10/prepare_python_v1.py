"""C10 versioned process cache: defer oversized files to unchanged sequential scan."""
from common import *
source=(PREVIOUS/'parallel_scan_v1.py').read_text(encoding='utf-8').replace('from common_v2 import *','from common import *')
source=source.replace('def __init__(self,hook,work):self.hook=hook;self.work=work;self.attempts=[];self.fresh=0;self.invocations=0',"def __init__(self,hook,work,defer_bytes=256*1024*1024):self.hook=hook;self.work=work;self.attempts=[];self.fresh=0;self.invocations=0;self.defer_bytes=defer_bytes;self.deferred=[]")
old="            data=target.read_bytes();key=(auth,str(module.ROOT),os.environ.get('COMPUTERNAME'),path,sha(data))"
new="""            if target.stat().st_size>self.defer_bytes:
                # Scheduling only: retain this candidate and its original position.
                # The unchanged sequential C6 hook evaluates it, including any
                # original parse/exception behavior, without helper translation.
                record=dict(index=index,**pin(target))
                if record not in self.deferred:self.deferred.append(record)
                continue
            data=target.read_bytes();key=(auth,str(module.ROOT),os.environ.get('COMPUTERNAME'),path,sha(data))"""
assert old in source;source=source.replace(old,new)
with (HERE/'parallel_scan_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(PREVIOUS/'prove_parallel_scan_v1.py').read_text(encoding='utf-8').replace('from common_v2 import *','from common import *')
source=source.replace("save('parallel-scanner-equivalence-v1.json'","save('parallel-scanner-base-equivalence-v1.json'")
with (HERE/'prove_parallel_scan_base_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(PREVIOUS/'python_workers_v1.py').read_text(encoding='utf-8').replace('from common_v2 import *','from common import *').replace('w2c9','w2c10').replace('cont9','cont10').replace('C8 worker','C10 worker')
source=source.replace("max_helpers=5)","max_helpers=5,deferred_to_original_sequential=accelerator.deferred)")
with (HERE/'python_workers_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(PREVIOUS/'freeze_python_inputs_v1.py').read_text(encoding='utf-8').replace('from common_v2 import *','from common import *')
with (HERE/'freeze_python_inputs_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('python-executor-crosswalk-v1.json',dict(kernel=dict(before=pin(PREVIOUS/'parallel_scan_v1.py'),after=pin(HERE/'parallel_scan_v1.py')),worker=dict(before=pin(PREVIOUS/'python_workers_v1.py'),after=pin(HERE/'python_workers_v1.py')),change='Only files above 256MiB are deferred from parallel precomputation to the exact unchanged sequential C6 scan/cache at their original candidate position. No file filtering, original scanner/test rewrite, JSON parser replacement or exception translation. Preserve the protected giant failed aggregation; never aggregate it as a result report.',original_queries=0))
