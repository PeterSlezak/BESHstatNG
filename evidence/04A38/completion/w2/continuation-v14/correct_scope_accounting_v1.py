from common import *
v=(HERE/'postprocess_v2.py').read_text()
v=v.replace("changed_paths={t['path'] for t in trans};extra=[]", "changed_paths={t['path'] for t in trans}|{json.loads((HERE/'owner-plan-authorized-transition-v1.json').read_bytes())['after']['path']};extra=[]")
with (HERE/'postprocess_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
v=(HERE/'finish_proof_v2.py').read_text().replace('nonpass-source-deltas-v2','nonpass-source-deltas-v3').replace('postprocess_v2.py','postprocess_v3.py')
with (HERE/'finish_proof_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('scope-accounting-correction-v1.json',dict(failed_attempt=pin(RUNROOT/'commands/w2c14-nonpass-source-deltas-v2/command.json'),trace=pin(RUNROOT/'commands/w2c14-nonpass-source-deltas-v2/stderr.log'),correction='Include the separately owner-authorized plan transition among actual changed paths in retained historical scope FAIL message deltas. Original assertions and FAIL outcomes unchanged.',authority=pin(HERE/'owner-plan-authorized-transition-v1.json')))
