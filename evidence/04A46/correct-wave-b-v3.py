from common46 import *
lock();p=ROOT/'BESHStatNG.Test/src/GuiWiring46ExecutionProjections.vb';s=p.read_text(encoding='utf-8')
with (PRIVATE/'failed-source-b2.vb').open('xb') as f:f.write(p.read_bytes())
s=s.replace(' Friend WithEvents ValidateIndependentMargin As New FakeControl46()\n','')
for owner in ['Ui12SampleSizeTtestSingleProp','Ui82x2','Ui8Proportions']:s=s.replace('Partial Friend Class '+owner+'Projection46\n','Partial Friend Class '+owner+'Projection46\n Friend WithEvents btCompute As New FakeControl46()\n')
p.write_text(s,encoding='utf-8',newline='\n')
