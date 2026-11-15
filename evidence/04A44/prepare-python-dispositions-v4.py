from common44 import *
s=(HERE/'python-dispositions-v3.py').read_text();start=s.index('# Independently reconstruct');end=s.index('\ndef capture_equal',start)
block='''# Independently reconstruct accepted43 raw/source state from already rehashed authorities.
previous43={}
for path,field in [('evidence/04A39/preservation/new-pins-v1.json','files'),('evidence/04A39/preservation/post-publication-additions-v1.json','added'),('.test-rationalization-04A39-correction-v1.tmp/private-pins-after-postseal.json','files')]:
 for r in load(ROOT/path)[field]:previous43[r['path']]=r['sha256']
for r in load(ROOT/'evidence/04A40/preservation/historical-readback-v4.json')['owner_line_ending_transitions']:previous43[r['committed']['path']]=r['committed']['sha256']
previous43.update({r['path']:r['sha256'] for r in load(ROOT/'.test-rationalization-04A43.tmp/tracked-entry-inventory-v1.json')['files']})
sealed43=load(ROOT/'evidence/04A43/matrix/source-freeze-v1.json')
previous43.update(sealed43['source_pins']);previous43[sealed43['plan']['path']]=sealed43['plan']['sha256'];previous43.update(old43pins);old43pins=previous43
'''
s=s[:start]+block+s[end:]
with (HERE/'python-dispositions-v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
