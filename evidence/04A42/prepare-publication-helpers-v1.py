from common42 import *
s=(ROOT/'evidence/04A41/publication-v1.py').read_text(encoding='utf-8-sig');s=s[:s.index('\ndef prepare():')]
s=s.replace('from common41','from common42').replace('04A41','04A42').replace('04A-41','04A-42').replace('matrix/source-freeze-v1.json','matrix/source-freeze-v2.json')
with (HERE/'publication_helpers.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Original scanner/authenticated AST/real identity/candidate machinery reused without rule changes',flush=True)
