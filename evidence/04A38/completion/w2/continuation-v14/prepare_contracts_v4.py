from common import *
book=PRIVATE/'bookkeeping-code-v1';s=book/'build_contracts_v3.py';v=s.read_text();old="assert len(writes)==1 and '.PS_FIT(' in writes[0]['caller'])";assert old not in v
old="assert len(writes)==1 and '.PS_FIT(' in writes[0]['caller']"
new="expected_writers={r['method'] for r in ms if r['member_name']=='.ctor'}|{r['method'] for r in m['methods'] if r['declaring_type']=='BESHStatNG.WorksheetFunctions.PropensityScoreUDFs' and r['member_name']=='PS_FIT'};assert len(writes)==2 and {w['caller'] for w in writes}==expected_writers"
assert old in v;v=v.replace(old,new,1)
with (book/'build_contracts_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
with (book/'alpha-constructor-writer-correction-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(before=pin(s),after=pin(book/'build_contracts_v4.py'),reason='The actual VB auto-property initializer uses set_Alpha in the constructor, in addition to PS_FIT. Record both real writers and require the exact constructor/fit caller set. Original/current ctor CIL is independently exact; never omit the default initializer.',original_queries=0),f,indent=2)
