from common import *
source=(HERE/'verify_c10_checkpoint_template_v2.py').read_text(encoding='utf-8')
point="    summary=dict(status='NOT_READY_04A38'"
assert point in source
source=source.replace(point,"    from verify_host_fragments_v1 import inspect as host_fragments\n    host_fragment_proof=host_fragments(load,raw,PREFIX,original,u,cs,portable,contexts)\n"+point)
source=source.replace("original_queries=0)\n    ledger=", "original_queries=0,host_fragment_proof=host_fragment_proof)\n    ledger=")
with (HERE/'verify_c10_checkpoint_template_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
result=command('static-reader-v3',[sys.executable,'-B',str(HERE/'verify_c10_checkpoint_template_v3.py'),'--static'])
save('static-reader-execution-v3.json',clean(result))
if result['exit_code']==0:
    doc=json.loads((ROOT/result['stdout']).read_bytes())
    save('readiness-static-v1.json',doc)
print(json.dumps(clean(result)))
