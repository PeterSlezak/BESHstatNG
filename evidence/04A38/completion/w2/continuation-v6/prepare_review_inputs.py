from common import *
history=json.loads((PREVIOUS/'historical-ownership-v3.json').read_bytes())['members']
old=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
now=json.loads((PREVIOUS/'current-universe-final-v4.json').read_bytes())
snap={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
def span(text,r):return text.encode('utf-16-le')[r['span_start']*2:(r['span_start']+r['span_length'])*2].decode('utf-16-le')
queue=json.loads((PREVIOUS/'remaining-work-queue-v3.json').read_bytes())['first_members']
names={r['name'] for r in queue}
names.update(['TryGetEquivalenceMargins','ParseEqualVarianceMode','TryReadNumericColumn','TryGetNumericColumn','TryGetTrimmedColumnObject','HasTextColumnHeaderForWholeColumnReference'])
selected=[r for r in history if r['name'] in names or r['path'].endswith(('/ParametricUDFs.vb','/NonparametricUDFs.vb'))]
pack=[]
for h in selected:
 o=next(r for r in old['members'] if r['symbol_id']==h['git_original_id'])
 ns=[r for r in now['members'] if r['signature_key']==o['signature_key']]
 assert len(ns)==1,h['name'];n=ns[0]
 row=dict(h,original_record=o,current_record=n,original_source=snap[h['path']],current_source=pin(ROOT/h['path']),
     original_body=span((ROOT/snap[h['path']]['path']).read_text(encoding='utf-8-sig'),o),
     current_body=span((ROOT/h['path']).read_text(encoding='utf-8-sig'),n))
 pack.append(row)
 p=HERE/'review-inputs'/h['name'];p.mkdir(parents=True,exist_ok=False)
 for stage in ['original','current']:
  (p/(stage+'.vb.txt')).write_text(row[stage+'_body'],encoding='utf-8',newline='\n')
save('review-inputs.json',dict(queue=queue,members=pack,original_queries=0))
print(json.dumps(dict(priority=[(r['name'],r['original_id']) for r in queue],selected=[(r['name'],bool(r['proof'])) for r in selected],support_open=[(r['path'],r['kind'],r['declaration']) for r in json.loads((PREVIOUS/'current-ownership-v3.json').read_bytes())['support'] if not r['fully_reconciled'] and not r['already_in_primary_population']]),ensure_ascii=False))
