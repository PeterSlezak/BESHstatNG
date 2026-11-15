from common43 import *
for name in ['entry-build-v1.py','entry-compiled-v1.py']:
    s=(ROOT/'evidence/04A42'/name).read_text(encoding='utf-8-sig').replace('from common42','from common43').replace('a42-entry-v1','a43-entry-v1').replace("'evidence/04A41/abi-cil/'","'evidence/04A42/abi-cil/'").replace("config+'-proof-v1.json'","config+'-proof-v3.json'").replace('accepted41','accepted42').replace('accepted41_reader','accepted42_reader')
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
