from common import *
s=(HERE/'verify_scope_addendum.py').read_text(encoding='utf-8')
s=s.replace("m==newm[m['method']]","{k:v for k,v in m.items() if k!='raw_IL_sha256'}=={k:v for k,v in newm[m['method']].items() if k!='raw_IL_sha256'}")
s=s.replace("ctor==newm[ctor['method']]","{k:v for k,v in ctor.items() if k!='raw_IL_sha256'}=={k:v for k,v in newm[ctor['method']].items() if k!='raw_IL_sha256'}")
with (HERE/'verify_scope_addendum_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/scope-reader-v1.json',dict(exit_code=1,error='Fresh host/cache full CIL/generic/EH drift',diagnosis='Initial reader also required raw metadata-token-bearing IL bytes to match after adding MemberRefs. Actual first differing field was raw_IL_sha256 only. V2 excludes only raw_IL_sha256; every resolved instruction/operand/offset/local/EH/generic/parameter/type/field and constructor body remains mandatory.',original_reader=pin(HERE/'verify_scope_addendum.py'),corrected_reader=pin(HERE/'verify_scope_addendum_v2.py'),not_a_product_failure=True))
