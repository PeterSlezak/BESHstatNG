from common import *
rows=[]
source=PREVIOUS/'build_v1.py';value=source.read_text(encoding='utf-8')
target=HERE/'build_v1.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
rows.append(dict(before=pin(source),after=pin(target),scope='Unchanged isolated offline compilation recipe; current common.py supplies C13 command/private paths.'))
source=PREVIOUS/'focused_v1.py';value=source.read_text(encoding='utf-8')
value=value.replace("('Core','BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj',False,'FullyQualifiedName~CaCoreTests|FullyQualifiedName~DiscriminantAnalysisCoreTests'),",'')
value=value.replace('Continuation12','Continuation13').replace('==6 and all','==4 and all')
target=HERE/'focused_v1.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
rows.append(dict(before=pin(source),after=pin(target),scope='Focused current C13 Common and shipping Windows adapter tests only; Core formulas unchanged. Debug only; no Release units.'))
save('focused-validation-recipe-crosswalk-v1.json',dict(rows=rows,original_queries=0))
result=command('packet-v1-Debug-build-driver',[sys.executable,'-B',str(HERE/'build_v1.py'),'packet-v1','Debug']);assert result['exit_code']==0
result=command('packet-v1-Debug-focused-driver',[sys.executable,'-B',str(HERE/'focused_v1.py'),'packet-v1','Debug']);assert result['exit_code']==0
print('First focused Common/actual Windows Debug packet completed')
