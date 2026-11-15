from common43 import *
s=(HERE/'scope-v1.py').read_text(encoding='utf-8-sig').replace("if 'ConcurrentDictionary' in p.read_text(encoding='utf-8-sig'):","if b'ConcurrentDictionary' in p.read_bytes():")
with (HERE/'scope-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
