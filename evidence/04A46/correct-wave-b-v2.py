from common46 import *
import re
lock();p=ROOT/'BESHStatNG.Test/src/GuiWiring46Projections.vb';s=(PRIVATE/'failed-source-a3/GuiWiring46Projections.vb').read_text(encoding='utf-8')
for owner in ['Ui12SampleSizeTtestSingleProp','Ui82x2','Ui8Proportions']:s=s.replace('Friend Class '+owner+'Projection46','Partial Friend Class '+owner+'Projection46')
p.write_text(s,encoding='utf-8',newline='\n')
p=ROOT/'BESHStatNG.Test/src/GuiWiring46ExecutionTests.vb';s=p.read_text(encoding='utf-8');s=re.sub(r'\bvariant\b','vectorIndex',s,flags=re.I);p.write_text(s,encoding='utf-8',newline='\n')
p=ROOT/'BESHStatNG.Test/src/GuiWiring46ExecutionProjections.vb';s=p.read_text(encoding='utf-8');s=s.replace(' Friend WithEvents getDataMultipleGroups As New FakeControl46()\n','');p.write_text(s,encoding='utf-8',newline='\n')
save('waves/source-correction-b2.json',dict(reason='Generator read its UTF-8 projection using Windows default encoding;restored exact passing A source then changed only Partial modifiers. Reserved Variant test variable renamed. Full failed B1 source preserved.',passing_A_source=pin(PRIVATE/'failed-source-a3/GuiWiring46Projections.vb'),current=pin('BESHStatNG.Test/src/GuiWiring46Projections.vb'),production_changed=False))
