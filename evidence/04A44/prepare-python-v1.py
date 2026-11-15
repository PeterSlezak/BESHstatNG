from common44 import *
src=ROOT/'evidence/04A43'
s=(src/'python-campaign-v1.py').read_text().replace('from common43','from common44').replace('source=ROOT/\'.test-rationalization-04A42.tmp/python-tools-v4\'','source=ROOT/\'.test-rationalization-04A43.tmp/python-tools-v1\'')
start=s.index('        for a,b in [');end=s.index('\n            if a in text:',start)
s=s[:start]+'''        for a,b in [('.test-rationalization-04A43.tmp/python-private-v1','.test-rationalization-04A44.tmp/python-private-v1'),('.test-rationalization-04A43.tmp/executions/a43-final-Debug-Common-semantic/observations','.test-rationalization-04A44.tmp/executions/a44-final-Debug-Common-semantic/observations'),("BASE='3955a6e9142256df6bc1fd53a924010c58367b51'","BASE='"+MAIN+"'"),("ROOT/'.test-rationalization-04A43.tmp'","ROOT/'.test-rationalization-04A44.tmp'"),('a43pyv1-','a44pyv1-')]:'''+s[end:]
s=s.replace("ROOT/'.test-rationalization-04A42.tmp/python-tools-v4/python-discovery-inventory-v1.json'","ROOT/'.test-rationalization-04A43.tmp/python-tools-v1/python-discovery-inventory-v1.json'").replace('exact_accepted42','exact_accepted43')
with (HERE/'python-campaign-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(src/'reconcile-python-v1.py').read_text().replace('from common43','from common44').replace('evidence/04A42/python/parent-event-reconciliation-v2','evidence/04A43/python/parent-event-reconciliation-v2').replace('accepted42','accepted43')
with (HERE/'reconcile-python-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
