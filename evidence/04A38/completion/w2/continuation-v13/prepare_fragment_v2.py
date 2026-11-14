from common import *
s=HERE/'fragment_correspondence_v1.py';v=s.read_text().replace("ph['original_source_record']","ph['original_record']").replace("save('source-composition-proof-v1.json',proof)","assert proof==json.loads((HERE/'source-composition-proof-v1.json').read_bytes())")
v=v.replace('new-parent-fragment-crosswalk-v1.json','new-parent-fragment-crosswalk-v2.json')
with (HERE/'fragment_correspondence_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('fragment-schema-correction-v1.json',dict(before=pin(s),after=pin(HERE/'fragment_correspondence_v2.py'),reason='Use actual original_record field in independently proved portable callback; first attempt wrote only the valid source composition receipt before this schema error.',original_queries=0))
