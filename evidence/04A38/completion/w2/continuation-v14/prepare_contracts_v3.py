from common import *
book=PRIVATE/'bookkeeping-code-v1';s=book/'build_contracts_v2.py';v=s.read_text().replace('from collections import defaultdict','from collections import defaultdict\nimport re')
old="key=n['owner']+'.'+n['name']+'('+','.join(ob['parameter_types'])+')';found="
new="key=(re.sub(r'``[0-9]+(?=\\()', '', re.sub(r' @ [^,]+, Version=[^,]+, Culture=[^,]+, PublicKeyToken=(?:[a-fA-F0-9]+|null)', '', ob['method'])) if None in ob['parameter_types'] else n['owner']+'.'+n['name']+'('+','.join(ob['parameter_types'])+')');found="
assert old in v;v=v.replace(old,new,1)
compile(v,'build_contracts_v3.py','exec')
with (book/'build_contracts_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
with (book/'generic-return-authority-correction-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(before=pin(s),after=pin(book/'build_contracts_v3.py'),reason='The retained original return ledger renders generic parameter Type.ToString instead of null FullName. Derive that exact legacy key from the already bound actual canonical CLR handle, removing only assembly qualification and generic arity display. Require unique recorded original return row and actual current return equality; no original query.',original_queries=0),f,indent=2)
