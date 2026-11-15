from common40_v3 import *
lock();s=(HERE/'verify40-v2.py').read_text(encoding='utf-8')
for a,b in [('diagnostic-dispositions-v1.json','diagnostic-dispositions-v3.json'),('readiness-v1.json','readiness-v2.json'),('readiness-adverse-controls-v1.json','readiness-adverse-controls-v2.json')]:s=s.replace(a,b)
(HERE/'verify40-v3.py').open('x',encoding='utf-8').write(s)
s=(HERE/'owner-dossier-v1.py').read_text(encoding='utf-8').replace('readiness-v1.json','readiness-v2.json').replace('diagnostic-dispositions-v1.json','diagnostic-dispositions-v3.json')
s=s.replace('exact test source/assertion sites and explicit unresolved argument differences','exact test source/assertion sites and source-backed branch/main/pin/scope argument explanations; no unexplained functional diagnostic mismatch remains')
(HERE/'owner-dossier-v2.py').open('x',encoding='utf-8').write(s)
