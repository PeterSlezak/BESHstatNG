from common import *
v=(HERE/'seal_controls_v3.py').read_text()
v=v.replace("roots=json.loads((HERE/'proof-inputs-v1.json').read_bytes())['roots']", "roots=json.loads((HERE/'proof-inputs-v1.json').read_bytes())['roots']['Debug'];assert len(closure(meta,roots)['methods'])==1581")
needle="reject('Missing actual source',lambda:verify.compute(overrides={source:b''},check_validation=False))"
assert v.count(needle)==1
v=v.replace(needle,needle+"\nplan=json.loads((HERE/'owner-plan-authorized-transition-v1.json').read_bytes());reject('Changed authorized owner plan successor',lambda:verify.compute(overrides={plan['after']['path']:(ROOT/plan['after']['path']).read_bytes()+b'changed'},check_validation=False));reject('Changed retained entry owner plan',lambda:verify.compute(overrides={plan['preimage']['path']:b'changed'},check_validation=False))")
with (HERE/'seal_controls_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('seal-preparation-root-binding-v1.json',dict(scope='Bind independent internal-dependency mutation to actual Debug root list, not the two-configuration dictionary; require genuine positive graph1581 before deletion. Owner-plan tamper controls added. No source/scientific test modification.',root_authority=pin(HERE/'proof-inputs-v1.json'),source=pin(HERE/'seal_controls_v4.py')))
