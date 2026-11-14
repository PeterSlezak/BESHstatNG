from common import *
text = (PREVIOUS / 'verify_preservation.py').read_text(encoding='utf-8')
text = text.replace("C4=W+'continuation-v4/'", "C4=W+'continuation-v4/';C5=W+'continuation-v5/'")
text = text.replace("transitions=load(root/(C4+'source-transitions.json'))['files'];c3trans=", "transitions=load(root/(C5+'source-transitions.json'))['files'];c4trans=load(root/(C4+'source-transitions.json'))['files'];c3trans=")
text = text.replace('for r in c3trans+c2trans:', 'for r in c4trans+c3trans+c2trans:')
text = text.replace('for r in c3trans+c2trans+transitions:', 'for r in c4trans+c3trans+c2trans+transitions:')
text = text.replace("  version(r['path'],dict(r['preimage'],bytes=r['before']['bytes'],sha256=r['before']['sha256']))", "  if r['before']:version(r['path'],dict(r['preimage'],bytes=r['before']['bytes'],sha256=r['before']['sha256']))")
anchor = sha((PREVIOUS / 'publication-manifest-v3.json').read_bytes())
count = len(json.loads((PREVIOUS / 'publication-manifest-v3.json').read_bytes())['entries'])
insert = """ c4manifest=(root/(C4+'publication-manifest-v3.json')).read_bytes();need(sha(c4manifest)=='ANCHOR','C4 final manifest changed')
 c4rows=json.loads(c4manifest)['entries'];need(len(c4rows)==COUNT,'C4 payload cardinality changed')
 for row in c4rows:
  if row['sha256'] is not None:check(row)
""".replace('ANCHOR',anchor).replace('COUNT',str(count))
text = text.replace(" protection=load(root/'evidence/04A38/preservation-validation-v2.json')", insert + " protection=load(root/'evidence/04A38/preservation-validation-v2.json')")
text = text.replace('prior_C3_hashed_payloads=3424,', 'prior_C3_hashed_payloads=3424,prior_C4_hashed_payloads='+str(count-1)+',')
with (HERE / 'verify_preservation.py').open('x',encoding='utf-8',newline='\n') as stream:
    stream.write(text)
