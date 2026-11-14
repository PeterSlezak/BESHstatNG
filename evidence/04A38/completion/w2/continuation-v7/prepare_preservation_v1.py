"""Extend immutable C6 protection graph with only explicit C7 transitions."""
from common import *
source=(PREVIOUS/'verify_preservation_v1.py').read_text(encoding='utf-8')
source=source.replace("C6=W+'continuation-v6/'", "C6=W+'continuation-v6/';C7=W+'continuation-v7/'")
needle="transitions=load(root/(C6+'source-transitions-final-v1.json'))['files'];"
assert source.count(needle)==1
source=source.replace(needle,"c6trans=load(root/(C6+'source-transitions-final-v1.json'))['files'];transitions=load(root/(C7+'source-transitions-final-v1.json'))['files'];")
source=source.replace('c5trans+c4trans+c3trans+c2trans','c6trans+c5trans+c4trans+c3trans+c2trans')
needle=" protection=load(root/'evidence/04A38/preservation-validation-v2.json')"
insert=""" c6manifest=(root/(C6+'publication-manifest-v1.json')).read_bytes();need(sha(c6manifest)=='9b5f1ef88e5b4c2e951baaa92d74065d8c127971bc7ca54994b243d4c69b1478','C6 final manifest changed')
 c6rows=json.loads(c6manifest)['entries'];need(len(c6rows)==5640,'C6 payload cardinality changed')
 for row in c6rows:
  if row['sha256'] is not None:check(row)
"""
assert source.count(needle)==1;source=source.replace(needle,insert+needle)
source=source.replace('prior_C5_hashed_payloads=4492,','prior_C5_hashed_payloads=4492,prior_C6_hashed_payloads=5639,')
with (HERE/'verify_preservation_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
save('preservation-reader-crosswalk-v1.json',dict(prior=pin(PREVIOUS/'verify_preservation_v1.py'),current=pin(HERE/'verify_preservation_v1.py'),changes='Add C6 immutable manifest and C6/C7 exact source-version graph. Old manifests, source/capture/owner pins,4695 tracked paths and Git controls unchanged; no ignored TestResults descent.',original_queries=0))
