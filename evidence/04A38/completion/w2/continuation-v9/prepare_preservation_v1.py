"""Extend the immutable source-transition graph by C8/C9, never rewrite old pins."""
from common_v2 import *
source=(PREVIOUS/'verify_c8_preservation_v1.py').read_text(encoding='utf-8')
source=source.replace("C8=W+'continuation-v8/'","C8=W+'continuation-v8/';C9=W+'continuation-v9/'")
source=source.replace("transitions=load(root/(C8+'source-transitions-final-v1.json'))['files']","c8trans=load(root/(C8+'source-transitions-final-v1.json'))['files'];transitions=load(root/(C9+'source-transitions-final-v1.json'))['files']")
source=source.replace('c7trans+c6trans','c8trans+c7trans+c6trans')
needle=" protection=load(root/'evidence/04A38/preservation-validation-v2.json')"
insert=""" c8manifest=(root/(C8+'publication-manifest-v2.json')).read_bytes();need(sha(c8manifest)=='9e92673350e948868d74e80585f730aeea74607f893a4647c2d4c6ea8eb7bd13','C8 final manifest changed')
 c8rows=json.loads(c8manifest)['entries'];need(len(c8rows)==6306,'C8 publication population changed')
 for row in c8rows:
  if row['sha256'] is not None:check(row)
 for row in load(root/(C9+'entry-v1.json'))['external_receipts']:check(row,transition=False)
 if live:
  retained=load(root/(C9+'v8-private-entry-v1.json'))
  for row in retained['pins']+retained['prior_archives']:check(row,transition=False)
  oldzip=load(root/(C9+'v8-publication-entry-v1.json'))['archive'];check(oldzip,transition=False);check(oldzip['deterministic_reproduction'],transition=False)
"""
assert needle in source;source=source.replace(needle,insert+needle)
source=source.replace('prior_C7_hashed_payloads=5914,','prior_C7_hashed_payloads=5914,prior_C8_hashed_payloads=6305,')
source=source.replace("'safe.directory='+root.as_posix(),*args","'safe.directory='+root.as_posix(),'--no-optional-locks',*args").replace("'safe.directory='+root.as_posix(),'diff'","'safe.directory='+root.as_posix(),'--no-optional-locks','diff'")
with (HERE/'verify_c9_preservation_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('preservation-reader-crosswalk-v1.json',dict(before=pin(PREVIOUS/'verify_c8_preservation_v1.py'),after=pin(HERE/'verify_c9_preservation_v1.py'),scope='Exact old manifests/preimages remain immutable. New8 source transitions extend the graph; live scope additionally rehashes all1371 prior private pins and retained physical archives. Copy scope explicitly excludes authoritative omitted private material. No ignored-directory traversal.',original_queries=0))
