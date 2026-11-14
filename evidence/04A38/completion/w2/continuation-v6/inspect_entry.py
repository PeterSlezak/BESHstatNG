"""Compact read of canonical evidence and exact changed nonpass traces."""
from common import *
from collections import Counter
v=json.loads((PREVIOUS/'nonpasses-v3.json').read_bytes())
print('DELTA',json.dumps(v['python_occurrence_delta_from_C4']))
for label in ['w2c4-validation-v1-python-scripts','w2c5-validation-v2-python-scripts']:
 p=RUNROOT/'commands'/label/'stderr.log';text=p.read_text(encoding='utf-8-sig')
 sections=text.split('\n======================================================================\n')
 hits=[s for s in sections if s.startswith(('FAIL: test_all_retained_native_methods_and_appearance_remain_exact','ERROR: test_all_retained_native_methods_and_appearance_remain_exact'))]
 print('TRACE',label,clean(hits))
contracts=json.loads((PREVIOUS/'body-contracts-v3.json').read_bytes())
print('CONTRACT_SCHEMA',list(contracts))
for k,val in contracts.items():
 if isinstance(val,list):
  print('LIST',k,len(val))
  for r in val:
   if isinstance(r,dict) and any(n in json.dumps(r)[:1200] for n in ['ANOVA1','WELCH','MCP']):print('ANOVA_CONTRACT',json.dumps(r))
for fn in ['historical-ownership-v3.json','current-ownership-v3.json','remaining-work-queue-v3.json']:
 d=json.loads((PREVIOUS/fn).read_bytes());print('SCHEMA',fn,list(d))
 for k,val in d.items():
  if isinstance(val,list):print('LIST_SCHEMA',k,len(val),json.dumps(val[:1])[:4500])
