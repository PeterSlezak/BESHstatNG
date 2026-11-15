from common42 import *
s=(HERE/'python-campaign-v3.py').read_text(encoding='utf-8-sig')
for a,b in [("target=PRIVATE/'python-tools-v3'","target=PRIVATE/'python-tools-v4'"),("(PRIVATE/'python-private-v3').mkdir()","(PRIVATE/'python-private-v4').mkdir()"),('.test-rationalization-04A42.tmp/python-private-v3','.test-rationalization-04A42.tmp/python-private-v4'),("('a41pyv2-','a42pyv2-')","('a41pyv2-','a42pyv4-')"),('python/discovery-v3.json','python/discovery-v4.json'),('python-input-freeze-v3.json','python-input-freeze-v4.json'),('python-input-postcheck-v3.json','python-input-postcheck-v4.json'),("'python3-'","'python4-'"),("'python-full-v3'","'python-full-v4'")]:s=s.replace(a,b)
with (HERE/'python-campaign-v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Version4 isolated discovery labels avoid preflight-only earlier command collisions;no full42 Python campaign yet',flush=True)
