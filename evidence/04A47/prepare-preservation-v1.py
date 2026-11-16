from common47 import *
lock();s=(ROOT/'evidence/04A46/preservation-base-v1.py').read_text().replace('common46','common47').replace('MasterPlan2.22.md','MasterPlan2.23.md').replace('register46_verify','register47_verify').replace("freeze44=load(ROOT/'evidence/04A45/matrix/source-freeze-v1.json')","freeze44=load(ROOT/'evidence/04A46/matrix/source-freeze-v1.json')")
with (HERE/'preservation-base-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
