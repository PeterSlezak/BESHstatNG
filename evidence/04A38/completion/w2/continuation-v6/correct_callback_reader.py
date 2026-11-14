from common import *
source=(HERE/'build_callback_proofs.py').read_text(encoding='utf-8')
source=source.replace("if i['op']=='ldftn']", "if i['op']=='ldftn' and methods[i['operand'][7:]]['member_name'].startswith('_Lambda$__')]")
with (HERE/'build_callback_proofs_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('attempts/callback-reader-v1.json',dict(status='ADVERSE_READER_ATTEMPT_RETAINED',reader=pin(HERE/'build_callback_proofs.py'),
 failure='Strict ldftn/source-lambda count rejected TryGetNestedThreeColumnData because ldftn also represents an ordinary named AddressOf callback.',
 correction='Separate actual compiler-generated _Lambda$__ targets from named AddressOf method targets, then retain exact source-order/count binding. Both named and anonymous edges remain in complete compiled dependency records.',closures_from_failed_attempt=0,original_queries=0))
exec(compile(source,str(HERE/'build_callback_proofs_v2.py'),'exec'))
