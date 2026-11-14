"""Add independent CIL-edge/caller checks, retaining the first reader unchanged."""
from pathlib import Path
HERE=Path(__file__).resolve().parent
text=(HERE/'verify_w1.py').read_text(encoding='utf-8')
anchor=" gaps=load(here/'consolidated-reference-gaps.json');"
insert=''' graph=load(here/'compiled-current-closure.json');methods={m['method']:m for m in graph['methods']}
 from collections import defaultdict
 consumers=defaultdict(set)
 for export in compiled['exports']:
  todo=[export['method']];visited=set()
  while todo:
   method=todo.pop()
   if method in visited:continue
   visited.add(method);consumers[method].add(export['effective_excel_name'])
   todo.extend(c['target'] for c in methods.get(method,{}).get('calls',[]) if c['assembly'] in ['BESHStatNG','BESHStat.Core','BESHStat.Excel.Common'])
 with (root/'evidence/04A38/ownership-progress-validation-v1-v2.csv').open(encoding='utf-8-sig',newline='') as stream:ledger={r['original_member_id']:r for r in csv.DictReader(stream)}
 for row in matrix:
  bindings=json.loads(ledger[row['canonical_original_id']]['windows_bindings'])
  need(row['compiled_windows_bindings']==bindings,'Invented Windows binding')
  actual=[methods[m] for m in bindings if m in methods]
  edges=[dict(windows_method=m['method'],**c) for m in actual for c in m['calls'] if c['assembly'] in ['BESHStat.Core','BESHStat.Excel.Common']]
  need(row['actual_portable_edges']==edges,'Fake or missing portable consumer edge')
  callers=sorted(set().union(*(consumers[m['method']] for m in actual))) if actual else []
  need(row['public_callers']==callers,'Invented or omitted registered call path')
  need(row['whole_body_disposition']=='NOT_CLOSED_W1_PLANNING_ONLY','Edge/host label passed off as whole-body closure')
 fragments={f['fragment_id']:f for f in original['fragments']}
 for row in review:
  original_fragment=fragments[row['fragment_id']]
  need(row['source_token_sha256']==original_fragment['token_sha256'] and row['condition']==original_fragment['code'],'Fabricated branch-source evidence')
'''
assert text.count(anchor)==1;text=text.replace(anchor,insert+anchor)
with (HERE/'verify_w1_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
