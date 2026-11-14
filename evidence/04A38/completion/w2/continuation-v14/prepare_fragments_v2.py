from common import *
book=PRIVATE/'bookkeeping-code-v1';s=book/'fragment_correspondence_v1.py';v=s.read_text()
v=v.replace(" if name=='GuessPositiveClass':", " if name=='TryGetRocInput':\n  legacy=next(m for m in json.loads((PREVIOUS/'implementation-source-composition-v2.json').read_bytes())['phases'] if 'Function TryGetRocInput(' in m['parent'])\n  for a,b in legacy['bindings'].items():text=text.replace(a,b)\n if name=='GuessPositiveClass':",1)
needle="  text=re.sub(r'\\bs\\b','token',text);"
assert needle in v;v=v.replace(needle,"  text=text.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible(statusCol(i, 0), iv)','binary(i)')\n"+needle,1)
old="  if (v['kind']==f['kind'] or cross_kind) and canonical(name,v['code'])==canonical(name,f['code']):matches.append("
new="  header_composite=name=='GuessPositiveClass' and f['kind']=='IfStatement' and v['kind']=='SingleLineIfStatement' and canonical(name,f['code'])==tokens('If Not binary(i) Then') and canonical(name,v['code'])==tokens('If Not binary(i) Then allBinary01 = False')\n  if header_composite or (v['kind']==f['kind'] or cross_kind) and canonical(name,v['code'])==canonical(name,f['code']):matches.append("
assert old in v;v=v.replace(old,new,1).replace('new-parent-fragment-crosswalk-v1.json','new-parent-fragment-crosswalk-v2.json')
with (book/'fragment_correspondence_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
with (book/'fragment-binding-correction-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(before=pin(s),after=pin(book/'fragment_correspondence_v2.py'),reason='Four real unmatched arms retained. Apply the already independently source/CIL-proved C13 ROC numeric/status delegate bindings. Reconcile the original multiline binary If header with the actual equivalent single-line condition and exact false-flag body, recording a real current fragment composite. No synthetic fragment or parent-only arm claim.',original_queries=0),f,indent=2)
