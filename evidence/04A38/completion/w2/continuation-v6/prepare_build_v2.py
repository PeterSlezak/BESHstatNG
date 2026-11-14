from common import *
source=(HERE/'build.py').read_text(encoding='utf-8').replace("HERE/'entry.json'","HERE/'implementation-plan.json'")
with (HERE/'build_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
