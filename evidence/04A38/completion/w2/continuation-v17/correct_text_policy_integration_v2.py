"""Preserve the failed development compile and correct only its selector syntax."""
from common import *
import re
p=ROOT/'BESHStat.Excel.Common/src/Analysis/UdfW2CompletionPreparation38.vb'
before=p.read_bytes()
snapshot=HERE/'text-policy-integration-failed-v1.vb.txt'
snapshot.write_bytes(before)
s=before.decode('utf-8')
authority=json.loads((HERE/'portable-text-conversion-preservation-v1.json').read_bytes())
changes=[]
for name in ['GetReferenceCategory','ParseSurvivalWeightText']:
 row=next(x for x in authority['transformations'] if x.get('method')==name)
 original=row['before']
 match=re.search(r'(?m)^[ \t]*Select Case ([^\n]*)\n(.*?)^[ \t]*End Select',original,re.S)
 assert match
 lines=['            Dim key As String = '+match[1]]
 cases=re.findall(r'(?m)^[ \t]*Case ([^\n]*)\n(.*?)(?=^[ \t]*Case |\Z)',match[2],re.S)
 assert len(cases)==(2 if name=='GetReferenceCategory' else 6)
 for case,body in cases:
  if case=='Else':
   lines.append(body.rstrip())
  else:
   choices=[x.strip() for x in case.split(',')]
   condition=' OrElse '.join('LegacyUdfText38.EqualText(key, '+x+')' for x in choices)
   lines.extend(['            If '+condition+' Then',body.rstrip(),'            End If'])
 replacement=original[:match.start()]+'\n'.join(lines)+'\n'+original[match.end():]
 assert s.count(row['after'])==1
 s=s.replace(row['after'],replacement,1)
 changes.append(dict(method=name,before=row['after'],after=replacement))
p.write_bytes(s.encode('utf-8'))
save('text-policy-integration-correction-v2.json',dict(failed_source=pin(snapshot),after=pin(p),changes=changes,failed_build=pin(HERE/'build-development-v4-Debug.json'),tests_executed=False))
old=json.loads((HERE/'development-source-snapshot-v4.json').read_bytes())
old['sources']=[pin(ROOT/r['path']) for r in old['sources']]
save('development-source-snapshot-v5.json',old)
script=(HERE/'build_development_Debug_v4.py').read_text(encoding='utf-8').replace('snapshot-v4.json','snapshot-v5.json')
with (HERE/'build_development_Debug_v5.py').open('x',encoding='utf-8',newline='\n') as f:f.write(script)
print('Text selector integration corrected; failed compile retained')
