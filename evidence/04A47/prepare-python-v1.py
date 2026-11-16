from common47 import *
lock();rows=[]
for name in ['python-campaign-v1.py','reconcile-python-v1.py','disposition-python-v1.py']:
 s=(ROOT/'evidence/04A46'/name).read_text().replace('common46','common47')
 s=s.replace('04A46','_NEXT47_').replace('04A45','04A46').replace('_NEXT47_','04A47')
 s=s.replace('a46-','_NEXT47_').replace('a45-','a46-').replace('_NEXT47_','a47-')
 s=s.replace('a46py','_NEXT47_').replace('a45py','a46py').replace('_NEXT47_','a47py')
 s=s.replace('accepted45','accepted46').replace('codex/04A-46','_NEXT47_').replace('codex/04A-45','codex/04A-46').replace('_NEXT47_','codex/04A-47')
 s=s.replace("BASE='350ffbc0924b09199249316a3ee2f31bc3dd98e8'","BASE='ebc4a3e9df41ef8588aa7f6b25363f6d044c41a7'")
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
 rows.append(dict(source=pin('evidence/04A46/'+name),driver=pin(HERE/name),scope='Unchanged956 parent bodies;isolated47 disjoint writer roots and authenticated new main/branch/plan context only.'))
save('python/driver-provenance-v1.json',dict(rows=rows,original_test_bodies_modified=False,permitted_ceiling=6,actual_peak=3))
