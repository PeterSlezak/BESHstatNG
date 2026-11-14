"""Version the full source/CIL contract builder against C9 and the exact C10 stage."""
from common import *
source=(PREVIOUS/'build_contracts_v1.py').read_text(encoding='utf-8').replace('from common_v2 import *','from common import *').replace('from proof_kernel_v1 import *','from proof_kernel_v2 import *').replace('packet-v1','packet-v2').replace('cohort-diagnostic-v2.json','cohort-diagnostic-v1.json').replace('Retained C8','Retained C9').replace('public_apis=11','public_apis=15').replace('scope=\'Separate new Core/Common declaration census','scope=\'Complete affected Core/Common declaration census, retaining5 previously supported declarations and distinguishing16 new declarations;')
with (HERE/'build_contracts_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('contract-builder-crosswalk-v1.json',dict(before=pin(PREVIOUS/'build_contracts_v1.py'),after=pin(HERE/'build_contracts_v1.py'),source_binding=pin(HERE/'proof_kernel_v2.py'),original_queries=0))
