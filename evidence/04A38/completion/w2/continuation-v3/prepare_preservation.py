from common import *
s=(PREVIOUS/'verify_preservation.py').read_text(encoding='utf-8').replace("C2=W+'continuation-v2/'","C2=W+'continuation-v2/';C3=W+'continuation-v3/'")
s=s.replace("transitions=load(root/(C2+'source-transitions-final.json'))['files'];final={r['after']['path']:r for r in transitions};versions={}","""transitions=load(root/(C3+'source-transitions-final.json'))['files'];c2trans=load(root/(C2+'source-transitions-final.json'))['files'];final={r['after']['path']:r for r in transitions};versions={}
 for r in c2trans:
  if r['after']['path'] not in final:final[r['after']['path']]=r""")
s=s.replace('for r in transitions:\n  version(', 'for r in c2trans+transitions:\n  version(')
s=s.replace("protection=load(root/'evidence/04A38/preservation-validation-v2.json')", """c2manifest=(root/(C2+'publication-manifest-v3.json')).read_bytes();need(sha(c2manifest)=='6428ab41d51fabf062db6a451a51a350428d7cd392f0c70147054ff635f4be86','C2 final manifest changed')
 c2rows=json.loads(c2manifest)['entries'];need(len(c2rows)==3125,'C2 payload count changed')
 for row in c2rows:
  if row['sha256'] is not None:check(row)
 protection=load(root/'evidence/04A38/preservation-validation-v2.json')""")
s=s.replace('prior_C1_hashed_payloads=2644,','prior_C1_hashed_payloads=2644,prior_C2_hashed_payloads=3124,')
with (HERE/'verify_preservation.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
