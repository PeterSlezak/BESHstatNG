"""Extend immutable protection graph through C7 and only the six C8 transitions."""
from common import *
source=(PREVIOUS/'verify_preservation_v1.py').read_text(encoding='utf-8')
source=source.replace("C7=W+'continuation-v7/'", "C7=W+'continuation-v7/';C8=W+'continuation-v8/'")
needle="c6trans=load(root/(C6+'source-transitions-final-v1.json'))['files'];transitions=load(root/(C7+'source-transitions-final-v1.json'))['files'];"
assert needle in source;source=source.replace(needle,"c6trans=load(root/(C6+'source-transitions-final-v1.json'))['files'];c7trans=load(root/(C7+'source-transitions-final-v1.json'))['files'];transitions=load(root/(C8+'source-transitions-final-v1.json'))['files'];")
source=source.replace('c6trans+c5trans+c4trans+c3trans+c2trans','c7trans+c6trans+c5trans+c4trans+c3trans+c2trans')
needle=" protection=load(root/'evidence/04A38/preservation-validation-v2.json')"
manifest_pin=pin(PREVIOUS/'publication-manifest-v1.json')
addition=""" c7manifest=(root/(C7+'publication-manifest-v1.json')).read_bytes();need(sha(c7manifest)==%r,'C7 final manifest changed')
 c7rows=json.loads(c7manifest)['entries'];need(len(c7rows)==5915,'C7 payload cardinality changed')
 for row in c7rows:
  if row['sha256'] is not None:check(row)
"""%manifest_pin['sha256']
assert needle in source;source=source.replace(needle,addition+needle)
source=source.replace('prior_C6_hashed_payloads=5639,','prior_C6_hashed_payloads=5639,prior_C7_hashed_payloads=5914,')
with (HERE/'verify_c8_preservation_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('preservation-reader-crosswalk-v1.json',dict(before=pin(PREVIOUS/'verify_preservation_v1.py'),after=pin(HERE/'verify_c8_preservation_v1.py'),C7_manifest=manifest_pin,scope='All previous manifest/source transition graphs,4695 tracked paths,278 source pins,100 captures,16 owner documents,41 original snapshots preserved. Add exact C7 manifest and C8 old/new/absent source transitions only. No ignored TestResults traversal.',original_queries=0))
