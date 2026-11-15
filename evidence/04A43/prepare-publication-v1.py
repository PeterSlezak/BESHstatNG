from common43 import *
s=(ROOT/'evidence/04A42/publication_helpers.py').read_text().replace('common42','common43').replace('final41','final43').replace('04A42_ACCEPTANCE','04A43_ACCEPTANCE').replace('matrix/source-freeze-v2','matrix/source-freeze-v1')
with (HERE/'publication_helpers.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
