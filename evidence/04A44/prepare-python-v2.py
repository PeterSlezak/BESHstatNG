from common44 import *
s=(HERE/'python-campaign-v1.py').read_text().replace("target=PRIVATE/'python-tools-v1'","target=PRIVATE/'python-tools-v2'").replace("(PRIVATE/'python-private-v1').mkdir()","(PRIVATE/'python-private-v2').mkdir()")
# New disjoint43-to44 substitutions,not a mutation of prior failed launch or test assertions.
s=s.replace("'.test-rationalization-04A44.tmp/python-private-v1'","'.test-rationalization-04A44.tmp/python-private-v2'").replace("'a44pyv1-'","'a44pyv2-'").replace("'python1-'","'python2-'").replace('python/discovery-v1.json','python/discovery-v2.json').replace('python-input-freeze-v1.json','python-input-freeze-v2.json').replace('python-input-postcheck-v1.json','python-input-postcheck-v2.json').replace("'python-full-v1'","'python-full-v2'")
with (HERE/'python-campaign-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'reconcile-python-v1.py').read_text().replace("target=PRIVATE/'python-tools-v1'","target=PRIVATE/'python-tools-v2'").replace('python-input-postcheck-v1.json','python-input-postcheck-v2.json').replace('python/discovery-v1.json','python/discovery-v2.json')
with (HERE/'reconcile-python-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('python/failed-prelaunch-v1.json',dict(status='FAILED_BEFORE_ANY_PARENT_EXECUTION;RETAINED',reason='Required final Debug semantic observation directory did not exist yet. Retried only after Debug completion with disjoint v2 outputs;no assertion/source edits.',raw=pin(PRIVATE/'python-input-postcheck-v1.json'),actual_parent_campaigns_in_failed_launch=0,gates=GATES))
