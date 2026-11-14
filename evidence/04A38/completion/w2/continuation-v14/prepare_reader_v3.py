from common import *
book=PRIVATE/'bookkeeping-code-v1'
v=(book/'verify_c14_checkpoint_template_v2.py').read_text();needle=" need(composition(root,overrides,load)==load(PREFIX+'source-composition-proof-v1.json'),'Whole-source behavioral composite changed');fragment_result=fragment_check(load,PREFIX)"
assert v.count(needle)==1
v=v.replace(needle," from verify_c14_host_v1 import inspect as host_check\n host_result=host_check(raw,load,PREFIX)\n"+needle).replace('summary.update(compiled_retention=retention,','summary.update(host_native=host_result,compiled_retention=retention,')
with (book/'verify_c14_checkpoint_template_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
v=(book/'seal_controls_v1.py').read_text().replace('verify_c14_checkpoint_template_v2.py','verify_c14_checkpoint_template_v3.py')
with (book/'seal_controls_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
print('Independent native storage/default/setter/cache verifier added to final reader template v3')
