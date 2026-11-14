"""Extend immutable predecessor lineage through retained C2 source preimages."""
from common import *
s=(PREVIOUS/'verify_preservation_v4.py').read_text(encoding='utf-8')
s=s.replace("C=W+'continuation-v1/'", "C=W+'continuation-v1/';C2=W+'continuation-v2/'")
s=s.replace("transitions=load(root/(C+'source-transitions-p1-v5.json'))['files'];final={r['after']['path']:r for r in transitions};versions={}","""transitions=load(root/(C2+'source-transitions-final.json'))['files'];final={r['after']['path']:r for r in transitions};versions={}
 for r in load(root/(C+'source-transitions-p1-v5.json'))['files']:
  if r['after']['path'] not in final:final[r['after']['path']]=r""")
s=s.replace("checked={};resolved=[]", """# Every former live C1 source version must have an exact retained C2 preimage.
 for r in transitions:
  version(r['path'],dict(r['preimage'],bytes=r['before']['bytes'],sha256=r['before']['sha256']))
  version(r['path'],r['snapshot'])
 checked={};resolved=[]""")
s=s.replace("for row in final.values():check(row['after'],transition=False);check(row['snapshot'],transition=False)","""for row in final.values():check(row['after'],transition=False);check(row['snapshot'],transition=False)
 predecessor=(root/(C+'publication-manifest-v4.json')).read_bytes();need(sha(predecessor)=='726f684cae59a2ace90bfeccd109c2443d1f43a610a51330f34cb891497301d8','C1 manifest changed')
 c1rows=json.loads(predecessor)['entries'];need(len(c1rows)==2645,'C1 count changed')
 for row in c1rows:
  if row['sha256'] is not None:check(row)
 """)
s=s.replace("prior_W2_hashed_payloads=2134", "prior_W2_hashed_payloads=2134,prior_C1_hashed_payloads=2644,continuation_source_transitions=len(transitions)")
with (HERE/'verify_preservation.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('preservation-reader-lineage.json',dict(predecessor_reader=pin(PREVIOUS/'verify_preservation_v4.py'),new_reader=pin(HERE/'verify_preservation.py'),rule='Exact2644C1payload hashes plus old2134W2/4695live hashes through explicit retained-source predecessor/successor graph; frozen evidence never mutable'))
