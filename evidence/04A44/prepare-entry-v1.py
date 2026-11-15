from common44 import *
s=(ROOT/'evidence/04A43/entry-compiled-v1.py').read_text().replace('common43','common44').replace('a43-','a44-').replace('04A42','04A43').replace('accepted42','accepted43').replace('proof-v3','proof-v1')
with (HERE/'entry-compiled-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
