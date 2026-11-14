from common import *
s=(HERE/'verify_p1_v3.py').read_text()
s=s.replace("oldheader=body(member(base,'LooksLikeHeaderRow')).replace('arr.GetLength(0)','rows')", "oldheader=body(member(base,'LooksLikeHeaderRow')).replace('Dim rows As Integer = arr.GetLength(0)','').replace('arr.GetLength(0)','rows')")
with (HERE/'verify_p1_v4.py').open('x',encoding='utf-8') as f:f.write(s)
save('attempts/p1-independent-reader-v3.json',dict(exit_code=1,reader=pin(HERE/'verify_p1_v3.py'),error='Header dependency changed',diagnosis='Original local rows = arr.GetLength(0) is supplied once at the current native call boundary. Reader v3 retained a spurious Dim rows = rows declaration after substituting the parameter. v4 maps that exact former local into the verified native argument; all branches and call counts remain checked.',product_changed=False))
