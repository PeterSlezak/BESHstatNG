from common import *
s=(HERE/'verify_p1_v2.py').read_text()
s=s.replace("re.search(r'\\) As [\\w.(), ?]+$',l) if kind=='Function'", "(') As ' in l and not l.rstrip().endswith(',')) if kind=='Function'")
with (HERE/'verify_p1_v3.py').open('x',encoding='utf-8') as f:f.write(s)
save('attempts/p1-independent-reader-v2.json',dict(exit_code=1,reader=pin(HERE/'verify_p1_v2.py'),error='Import operation/partial ByRef/order changed TryGetNestedThreeColumnData',diagnosis='Signature reader matched the nonfinal ByRef data(,) As Object, line as the end of the method signature. Token diff contains only the remaining ByRef names declaration. Reader v3 requires a final signature line without a trailing comma; operation checks remain exact.',product_changed=False))
