"""Extend, never replace, the immutable C9 preservation/source-transition graph."""
from common import *
source=(PREVIOUS/'verify_c9_preservation_v1.py').read_text(encoding='utf-8')
source=source.replace("C9=W+'continuation-v9/'","C9=W+'continuation-v9/';C10=W+'continuation-v10/'")
source=source.replace("transitions=load(root/(C9+'source-transitions-final-v1.json'))['files']","c9trans=load(root/(C9+'source-transitions-final-v1.json'))['files'];transitions=load(root/(C10+'source-transitions-final-v1.json'))['files']")
source=source.replace('for r in c8trans+c7trans','for r in c9trans+c8trans+c7trans')
point=" for row in load(root/(C9+'entry-v1.json'))['external_receipts']:check(row,transition=False)"
extra=""" c9manifest=(root/(C9+'publication-manifest-v1.json')).read_bytes();need(sha(c9manifest)=='DIGEST','C9 final manifest changed')
 c9rows=json.loads(c9manifest)['entries'];need(len(c9rows)==6698,'C9 publication population changed')
 for row in c9rows:
  if row['sha256'] is not None:check(row)
 for row in load(root/(C10+'entry-v1.json'))['anchors']+load(root/(C10+'entry-v1.json'))['external_receipts']:check(row,transition=False)
""".replace('DIGEST',pin(PREVIOUS/'publication-manifest-v1.json')['sha256'])
assert point in source;source=source.replace(point,extra+point)
source=source.replace("prior_C8_hashed_payloads=6305,","prior_C8_hashed_payloads=6305,prior_C9_hashed_payloads=6697,")
with (HERE/'verify_c10_preservation_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(PREVIOUS/'publication_privacy_c9_v1.py').read_text(encoding='utf-8').replace("authority['C8_publication_receipt']","authority['C9_publication_receipt']").replace('6306','6698')
with (HERE/'publication_privacy_c10_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('publication-privacy-authority-v1.json',dict(base_policy=pin(W2/'continuation-v6/publication_policy.py'),C9_publication_receipt=pin(W2/'continuation-v9-publication-receipt-v1.json'),law='Only exact unchanged policy/path/length/SHA reuses actual C9 physical privacy decisions. Every fresh or changed payload uses the unchanged C6 policy. No new exception; giant failed aggregation stays protected and is explicitly excluded only from review derivatives.',new_privacy_exceptions=0,original_queries=0))
source=(PREVIOUS/'prove_publication_privacy_v1.py').read_text(encoding='utf-8')
begin=source.index("entry=json.loads");end=source.index('import publication_privacy_c9_v1')
source=source[:begin]+source[end:]
source=source.replace('from common_v2 import *','from common import *').replace('publication_privacy_c9_v1','publication_privacy_c10_v1').replace('continuation-v8','continuation-v9').replace('C9 privacy','C10 privacy')
with (HERE/'prove_publication_privacy_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('preservation-reader-crosswalk-v1.json',dict(before=pin(PREVIOUS/'verify_c9_preservation_v1.py'),after=pin(HERE/'verify_c10_preservation_v1.py'),scope='Add exact C9 final manifest/entry authorities and six C10 immediate preimages/successors. Keep all former transitions, owner/lineage/mixed-model/original source checks and live Git predicates. Private v9 final-v2/reader-v6 and archive preservation have a separate additive receipt. No TestResults traversal.'))
