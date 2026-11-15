from common44 import *
s=(HERE/'audit-v1.py').read_text().replace("current=(ROOT/r['path']).read_text(encoding='utf-8-sig')","current=(ROOT/r['path']).read_bytes().decode('utf-8-sig')")
with (HERE/'audit-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
