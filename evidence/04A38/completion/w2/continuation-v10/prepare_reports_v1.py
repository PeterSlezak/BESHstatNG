from common import *
source=(PREVIOUS/'report_validation_v4.py').read_text(encoding='utf-8').replace('from common_v2 import *','from common import *').replace('w2c9','w2c10')
source=source.replace("PREVIOUS/'python-first-failures-v1.json'","PREVIOUS/'python-first-failures-v2.json'")
source=source.replace('INHERITED_C8','INHERITED_C9').replace('NEW_C9','NEW_C10').replace('C8_actual_parent_baseline','C9_actual_parent_baseline')
with (HERE/'report_validation_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(PREVIOUS/'run_python_v1.py').read_text(encoding='utf-8').replace('from common_v2 import *','from common import *')
with (HERE/'run_python_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('bounded-report-source-crosswalk-v1.json',dict(before=pin(PREVIOUS/'report_validation_v4.py'),after=pin(HERE/'report_validation_v1.py'),baseline=pin(PREVIOUS/'python-first-failures-v2.json'),law='Bounded v2 event links only; never parse, aggregate, truncate or overwrite the protected failed v9 4GB v1 aggregation. Parent methods and subtest events counted separately; actual message/classification deltas require independent investigation.'))
