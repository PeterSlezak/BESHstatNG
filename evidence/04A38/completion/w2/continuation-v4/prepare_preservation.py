from common import *
s=(PREVIOUS/'verify_preservation.py').read_text(encoding='utf-8')
s=s.replace("C3=W+'continuation-v3/'","C3=W+'continuation-v3/';C4=W+'continuation-v4/'")
needle="transitions=load(root/(C3+'source-transitions-final.json'))['files'];c2trans="
assert needle in s
s=s.replace(needle,"transitions=load(root/(C4+'source-transitions.json'))['files'];c3trans=load(root/(C3+'source-transitions-final.json'))['files'];c2trans=")
s=s.replace('for r in c2trans:\n','for r in c3trans+c2trans:\n').replace('for r in c2trans+transitions:\n','for r in c3trans+c2trans+transitions:\n')
needle=" protection=load(root/'evidence/04A38/preservation-validation-v2.json')"
assert needle in s
extra=" c3manifest=(root/(C3+'publication-manifest-v2.json')).read_bytes();need(sha(c3manifest)=='6c1be0d2bc3e6fbe00968ed1385ad599cab28ad49306756de6085358f98a45dc','C3 final manifest changed')\n c3rows=json.loads(c3manifest)['entries'];need(len(c3rows)==3425,'C3 payload cardinality changed')\n for row in c3rows:\n  if row['sha256'] is not None:check(row)\n"
s=s.replace(needle,extra+needle).replace('prior_C2_hashed_payloads=3124,','prior_C2_hashed_payloads=3124,prior_C3_hashed_payloads=3424,')
with (HERE/'verify_preservation.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(PREVIOUS/'record_tracked_paths.py').read_text(encoding='utf-8').replace('source-transitions-final.json','source-transitions.json').replace('AUTHORIZED_V3_SOURCE_TRANSITION','AUTHORIZED_V4_SOURCE_TRANSITION').replace('EXACT_C2_CHECKPOINT','EXACT_C3_CHECKPOINT')
with (HERE/'record_tracked_paths.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
