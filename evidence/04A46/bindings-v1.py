from common46 import *
import re,xml.etree.ElementTree as ET
d=load(HERE/'fixtures/projections-wave-a-v1.json');root=ET.Element('bindings')
for entry in d['members']:
 m=re.search(r'(?s)\bHandles\s+((?:\w+\.\w+\s*,?\s*)+)',entry['original_text'])
 if not m:continue
 for control,event in re.findall(r'(\w+)\.(Click|CheckedChanged|SelectedIndexChanged|ValueChanged)',m[1]):ET.SubElement(root,'binding',dict(owner=entry['owner'],method=entry['method'],source=control,event=event))
with (HERE/'fixtures/bindings-wave-a-v1.xml').open('xb') as f:f.write(ET.tostring(root,encoding='utf-8',xml_declaration=True))
save('fixtures/numeric-source-v2.json',dict(load(HERE/'fixtures/numeric-source-v1.json'),projection=pin('BESHStatNG.Test/src/GuiWiring46Numbers.vb'),transform=[['txtData As Object','txtData As FakeControl46']],qualification='Exact shipping helper body;closed fake parameter type substitution removes late binding and ByRef narrowing while preserving Text/BackColor semantics for supplied fakes. Existing Windows arbitrary-object/COM acquisition remains native pending. Option Strict On.'))
print('Actual Handles clauses:',len(root),'event bindings',flush=True)
