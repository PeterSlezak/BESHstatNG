"""Materialize complete dependency closure for source review, without closing it."""
from common import *
import re
history=json.loads((PREVIOUS/'historical-ownership-v3.json').read_bytes())['members']
old=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
now=json.loads((HERE/'current-universe-families-v1.json').read_bytes())
byname={}
for r in now['members']:
 if r['parent_member'] is None:byname.setdefault(r['name'],[]).append(r)
queue=json.loads((PREVIOUS/'remaining-work-queue-v3.json').read_bytes())['first_members']
selected={r['symbol_id']:r for r in now['members'] if r['name'] in {q['name'] for q in queue} or
 r['path'].endswith(('/ParametricUDFs.vb','/NonparametricUDFs.vb')) or
 (r['path'].endswith('/ExcelArgModules.vb') and r['owner'].endswith(('ExcelArgNumeric','ExcelArgPredicates','ExcelArgReaders')))}
pending=list(selected.values())
while pending:
 r=pending.pop()
 for inv in r['invocations']:
  m=re.match(r'^(?:(?:Global\.)?[A-Za-z_][\w.]*\.)?([A-Za-z_]\w*)\(',inv['text'])
  if not m:continue
  candidates=byname.get(m[1],[])
  if len(candidates)==1:
   c=candidates[0]
   if c['symbol_id'] not in selected:selected[c['symbol_id']]=c;pending.append(c)
snap={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
def span(text,r):return text.encode('utf-16-le')[r['span_start']*2:(r['span_start']+r['span_length'])*2].decode('utf-16-le')
rows=[]
for n in selected.values():
 hs=[h for h in history if n['signature_key'] in {i.split('@')[0] for i in h['current_ids']}|{h['git_original_id'].split('@')[0]}]
 assert len(hs)<=1,n['name']
 h=hs[0] if hs else None
 o=next(r for r in old['members'] if r['symbol_id']==h['git_original_id']) if h else None
 current=span((ROOT/n['path']).read_bytes().decode('utf-8-sig'),n)
 original=span((ROOT/snap[n['path']]['path']).read_bytes().decode('utf-8-sig'),o) if o else None
 rows.append(dict(historical=h,original_record=o,current_record=n,original_body=original,current_body=current,
    original_source=snap[n['path']],current_source=pin(ROOT/n['path'])))
 p=HERE/'complete-review-inputs'/n['name'];p.mkdir(parents=True,exist_ok=False)
 for label,text in [('original',original),('current',current)]:
  if text is not None:(p/(label+'.vb.txt')).write_text(text,encoding='utf-8',newline='\n')
save('complete-review-inputs.json',dict(members=rows,scope='Source/caller dependency candidates, not proof or closure labels',original_queries=0))
print(json.dumps(dict(total=len(rows),new_candidates=[r['current_record']['name'] for r in rows if not r['historical'] or not r['historical']['proof']])))
