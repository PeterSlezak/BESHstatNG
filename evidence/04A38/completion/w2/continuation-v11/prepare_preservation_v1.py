"""Extend the immutable preservation transition graph; do not change old receipts."""
from common import *
source=(PREVIOUS/'verify_c10_preservation_v1.py').read_text(encoding='utf-8')
source=source.replace("C10=W+'continuation-v10/'", "C10=W+'continuation-v10/';C11=W+'continuation-v11/'")
source=source.replace("transitions=load(root/(C10+'source-transitions-final-v1.json'))['files']", "c10trans=load(root/(C10+'source-transitions-final-v1.json'))['files'];transitions=load(root/(C11+'source-transitions-final-v1.json'))['files']")
source=source.replace('for r in c9trans+c8trans', 'for r in c10trans+c9trans+c8trans')
point=" for row in load(root/(C10+'entry-v1.json'))['anchors']+load(root/(C10+'entry-v1.json'))['external_receipts']:check(row,transition=False)"
extra=""" c10manifest=(root/(C10+'publication-manifest-v1.json')).read_bytes();need(sha(c10manifest)=='DIGEST','C10 final manifest changed')
 c10rows=json.loads(c10manifest)['entries'];need(len(c10rows)==7075,'C10 publication population changed')
 for row in c10rows:
  if row['sha256'] is not None:check(row)
 for row in load(root/(C11+'entry-authority-pins-v1.json'))['authorities']:check(row,transition=False)
""".replace('DIGEST',pin(PREVIOUS/'publication-manifest-v1.json')['sha256'])
assert point in source;source=source.replace(point,extra+point)
source=source.replace('prior_C9_hashed_payloads=6697,','prior_C9_hashed_payloads=6697,prior_C10_hashed_payloads=7074,')
with (HERE/'verify_c11_preservation_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
save('preservation-reader-crosswalk-v1.json',dict(before=pin(PREVIOUS/'verify_c10_preservation_v1.py'),
    after=pin(HERE/'verify_c11_preservation_v1.py'),scope='Add exact C10 final manifest and C11 source preimages/successors, preserving every former manifest/owner/lineage/capture/tracked/source/Git check. No traversal of ignored TestResults.',original_queries=0))
