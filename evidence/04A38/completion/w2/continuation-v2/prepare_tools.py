"""Reuse offline isolated build/replay patterns; do not modify old tools."""
from common import *
for name in ['build.py','compatibility.py','inspect_stage.py']:
 s=(PREVIOUS/name).read_text()
 if name=='inspect_stage.py':
  s=s.replace("before=json.loads((HERE/'entry.json').read_bytes())['source_before'];after=[]","before=json.loads((HERE/'entry.json').read_bytes())['source_before'];after=[]")
 with (HERE/name).open('x',encoding='utf-8') as f:f.write(s)
# Focus only changed contracts plus shipping routes; full Debug follows final production.
s=(PREVIOUS/'focused.py').read_text().replace('FullyQualifiedName~UdfModelProfilePreparation38Tests','FullyQualifiedName~UdfModelProfilePreparation38Tests|FullyQualifiedName~UdfPreparation38Tests').replace('"""Focus on changed Common contracts and real shipping formatter adapter."""','"""Current portable/actual shipping preparation contract tests."""')
with (HERE/'focused.py').open('x',encoding='utf-8') as f:f.write(s)
print('Current-only tools prepared with unique w2c2 lanes')
