from common46 import *
lock();s=(ROOT/'evidence/04A45/preservation-v1.py').read_text(encoding='utf-8-sig').replace('common45','common46').replace('MasterPlan2.21.md','MasterPlan2.22.md').replace('register45_verify','register46_verify')
# Reconcile accepted45 source endpoints;old44 protected raw/source history remains in
# the unchanged readback code and is separately rehashed through all41-44 populations.
s=s.replace("freeze44=load(ROOT/'evidence/04A44/matrix/source-freeze-v1.json')","freeze44=load(ROOT/'evidence/04A45/matrix/source-freeze-v1.json')")
s=s.replace("save('preservation/readback-v1.json'", "save('preservation/readback-base-v1.json'")
with (HERE/'preservation-base-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
