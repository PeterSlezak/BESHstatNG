from common import *
s=(HERE/'verify_ownership_v2.py').read_text(encoding='utf-8').replace('import hashlib,json','from functools import cache\nimport hashlib,json').replace(' def load(p):return json.loads(raw(p))',' @cache\n def load(p):return json.loads(raw(p))')
with (HERE/'verify_ownership_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('ownership-reader-performance-v3.json',dict(previous=pin(HERE/'verify_ownership_v2.py'),new=pin(HERE/'verify_ownership_v3.py'),change='Per-inspection JSON memoization only; every immutable source/metadata path checked with same logic and overrides. Avoids reparsing four metadata packets for235auto-properties. No policy/count/assertion change.'))
