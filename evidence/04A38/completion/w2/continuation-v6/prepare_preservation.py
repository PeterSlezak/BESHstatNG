from common import *
source=(PREVIOUS/'verify_preservation_v3.py').read_text(encoding='utf-8')
source=source.replace("C5=W+'continuation-v5/'","C5=W+'continuation-v5/';C6=W+'continuation-v6/'")
source=source.replace("transitions=load(root/(C5+'source-transitions-v4.json'))['files'];", "transitions=load(root/(C6+'source-transitions-final-v1.json'))['files'];c5trans=load(root/(C5+'source-transitions-v4.json'))['files'];")
source=source.replace('for r in c4trans+c3trans+c2trans:', 'for r in c5trans+c4trans+c3trans+c2trans:')
source=source.replace('for r in c4trans+c3trans+c2trans+transitions:', 'for r in c5trans+c4trans+c3trans+c2trans+transitions:')
digest=json.loads((HERE/'v5-publication-hashes-recomputed.json').read_bytes())['manifest']['sha256']
insert=""" c5manifest=(root/(C5+'publication-manifest-v1.json')).read_bytes();need(sha(c5manifest)=='"""+digest+"""','C5 final manifest changed')
 c5rows=json.loads(c5manifest)['entries'];need(len(c5rows)==4493,'C5 payload cardinality changed')
 for row in c5rows:
  if row['sha256'] is not None:check(row)
"""
source=source.replace(" protection=load(root/'evidence/04A38/preservation-validation-v2.json')",insert+" protection=load(root/'evidence/04A38/preservation-validation-v2.json')")
source=source.replace('prior_C4_hashed_payloads=3777,','prior_C4_hashed_payloads=3777,prior_C5_hashed_payloads=4492,')
with (HERE/'verify_preservation_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
from verify_preservation_v1 import inspect
result=inspect();save('preservation-live-v1.json',result)
print(json.dumps({k:result[k] for k in ['status','tracked','lineage_pins','protected_captures','owner_documents_checked','original_snapshots','prior_C5_hashed_payloads','continuation_source_transitions','Git']}))
