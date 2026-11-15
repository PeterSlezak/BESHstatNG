from common44 import *
s=(HERE/'python-dispositions-v2.py').read_text()
pos=s.index('\ndef capture_equal')
s=s[:pos]+"\n# Independently reconstruct accepted43 pre-merge bytes for older broad13/14 scope lists.\nprevious43={r['path']:r['sha256'] for r in load(ROOT/'.test-rationalization-04A43.tmp/tracked-entry-inventory-v1.json')['files']}\nsealed43=load(ROOT/'evidence/04A43/matrix/source-freeze-v1.json')\nprevious43.update(sealed43['source_pins']);previous43[sealed43['plan']['path']]=sealed43['plan']['sha256'];previous43.update(old43pins);old43pins=previous43\n"+s[pos:]
with (HERE/'python-dispositions-v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
