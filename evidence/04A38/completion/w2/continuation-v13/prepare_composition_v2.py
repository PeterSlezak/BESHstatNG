from common import *
source=HERE/'verify_c13_composition_v1.py';v=source.read_text()
v=v.replace("entry=body(raw(transitions[movement['path']]['preimage']['path']),old[movement['path'],name])[0]","entry=body(raw(transitions[movement['path']]['preimage']['path']),old[movement['path'],name])[0].replace('\\r\\n','\\n')")
with (HERE/'verify_c13_composition_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('composition-normalization-correction-v1.json',dict(before=pin(source),after=pin(HERE/'verify_c13_composition_v2.py'),
    reason='Imported source view retains CRLF. Apply the recorded line-ending normalization before exact segment replacement; all token/body/portable boundary assertions remain. Independent byte reversal already passed.',original_queries=0))
