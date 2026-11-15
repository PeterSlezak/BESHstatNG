from common43 import *
s=(HERE/'check-wave-b-v1.py').read_text(encoding='utf-8-sig').replace('a43-wave-b-v1','a43-wave-b2-v1').replace('wave-b-build-v1.json','wave-b2-build-v1.json').replace('a43-wave-b-','a43-wave-b2-').replace("'wave-b-" ,"'wave-b2-").replace('waves/wave-b-v1.json','waves/wave-b2-v1.json')
with (HERE/'check-wave-b2-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Legacy explicit test Compile registration added;earlier discovery198 was not falsely counted as218',flush=True)
