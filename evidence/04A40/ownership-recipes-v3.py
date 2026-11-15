from common40_v3 import *
lock()
s=(HERE/'ownership-and-issues-v1.py').read_text(encoding='utf-8').split("register=load(ROOT/'evidence/04A39/register/defects-v7.json')")[0]
for a,b in [('source-list-final-v1.txt','source-list-final-v2.txt'),('source-map-final-v1','source-map-final-v2'),('method-parity-v1.json','method-parity-v2.json')]:s=s.replace(a,b)
s+="print('Final source ownership map refreshed without register rewrite',flush=True)\n"
(HERE/'ownership-final-v2.py').open('x',encoding='utf-8').write(s)
s=(HERE/'family-reconciliation-v2.py').read_text(encoding='utf-8')
for a,b in [('source-map-final-v1.json','source-map-final-v2.json'),('method-parity-v1.json','method-parity-v2.json'),('family-parity-v2.json','family-parity-v3.json'),("save('register/issue-delta-v2.json'","save('register/issue-delta-v3.json'")]:s=s.replace(a,b)
(HERE/'family-reconciliation-v3.py').open('x',encoding='utf-8').write(s)
