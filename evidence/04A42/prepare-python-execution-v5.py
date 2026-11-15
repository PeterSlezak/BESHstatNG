from common42 import *
s=(HERE/'python-campaign-v4.py').read_text(encoding='utf-8-sig');s=s.replace('evidence/04A38/completion/w3/python-node-scanners/python-input-freeze-v4.json','evidence/04A38/completion/w3/python-node-scanners/python-input-freeze-v2.json')
with (HERE/'python-campaign-execution-v5.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Historical W3 input authority restored to actual immutablev2;current disjointv4 paths retained',flush=True)
