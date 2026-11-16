from common46 import *
lock();p=ROOT/'BESHStatNG.Test/src/GuiWiring46ExecutionProjections.vb'
with (PRIVATE/'failed-source-b3.vb').open('xb') as f:f.write(p.read_bytes())
s=p.read_text(encoding='utf-8')
edits=[['As ExcelDnaResultWriter','As FakeResultWriter46'],['New ExcelDnaResultWriter','New FakeResultWriter46'],['As Range','As FakeRange46'],['New ProcessListofResultTables','New FakeTables46'],['If AreaCheck(','If FakeOutput46.AreaCheck('],['= WorksheetFromRefAdress(','= FakeOutput46.WorksheetFromRefAdress(']]
for a,b in edits:s=s.replace(a,b)
p.write_text(s,encoding='utf-8',newline='\n')
save('fixtures/host-aliases-wave-b-v4.json',dict(edits=edits,reason='VB parent namespace types outrank Imports aliases;explicit fake owner/type substitutions keep source branch bodies and signatures closed to host APIs. Original failed compiler attempt retained;no live COM invocation.',source=pin(p)))
