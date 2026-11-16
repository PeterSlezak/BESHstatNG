from common47 import *
import shutil,xml.etree.ElementTree as ET
lock();folder=PRIVATE/'failed-source-b1';folder.mkdir()
for p in ['GuiWiring47ProjectionB.vb','GuiWiring47LongrunTests.vb','GuiWiring47ContractTests.vb','GuiWiring47Fakes.vb']:shutil.copy2(ROOT/'BESHStatNG.Test/src'/p,folder/p)
p=ROOT/'BESHStatNG.Test/src/GuiWiring47ProjectionB.vb';p.write_text(p.read_text().replace(' Friend WithEvents Me As New FakeControl47()\n',''),encoding='utf-8',newline='\n')
d=load(HERE/'fixtures/projections-b-v1.json');events={'Click','CheckedChanged','SelectedIndexChanged','ValueChanged'}
d['bindings']=[r for r in d['bindings'] if r['event'] in events and r['control']!='Me']
for m in d['members']:m['fields']=[f for f in m['fields'] if f!='Me'];m['bindings']=[r for r in m['bindings'] if r['event'] in events and r['control']!='Me']
d['projection']=pin(p);d['binding_correction']='Source regex candidate Me.property references rejected,not event subscriptions. Only actual named event clauses retained;no shipping method changes.'
save('fixtures/projections-b-v2.json',d)
root=ET.Element('bindings')
for r in d['bindings']:ET.SubElement(root,'binding',r)
out=HERE/'fixtures/bindings-b-v2.xml'
with out.open('xb') as f:f.write(ET.tostring(root,encoding='utf-8',xml_declaration=True))
p=ROOT/'BESHStatNG.Test/src/GuiWiring47LongrunTests.vb';s=p.read_text().replace('bindings-b-v1.xml','bindings-b-v2.xml').replace('GuiWiring47StateTests.SourceHandlesAndDynamicSubscriptionsActuallyDeliver(', '(New GuiWiring47StateTests()).SourceHandlesAndDynamicSubscriptionsActuallyDeliver(');p.write_text(s,encoding='utf-8',newline='\n')
p=ROOT/'BESHStatNG.Test/src/GuiWiring47ContractTests.vb';p.write_text(p.read_text().replace('suppliedStart','cancelBeforeWrite'),encoding='utf-8',newline='\n')
