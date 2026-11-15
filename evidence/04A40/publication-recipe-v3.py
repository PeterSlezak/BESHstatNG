from common40_v3 import *
lock();s=(HERE/'publish-v2.py').read_text(encoding='utf-8')
s=s.replace('final/readiness-v1.json','final/readiness-v2.json')
s=s.replace("renderings=[]", """selected.update(['evidence/04A39/physical-witness-v1/physical.py','evidence/04A39/physical-witness-v1/witness_lib.py',
 'evidence/04A39/physical-witness-v1/dependencies/privacy_regex_guard.py','evidence/04A39/correction-v1/entry/source-lock.json'])
renderings=[]""")
s=s.replace("schema='04A40_REVIEW_PAYLOAD_V1',files=rows", "schema='04A40_REVIEW_PAYLOAD_V1',active_readiness='evidence/04A40/final/readiness-v2.json',active_diagnostics='evidence/04A40/python/diagnostic-dispositions-v3.json',files=rows")
(HERE/'publish-v3.py').open('x',encoding='utf-8').write(s)
