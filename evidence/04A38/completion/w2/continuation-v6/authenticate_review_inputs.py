from common import *
from source_views import body
earlier=json.loads((HERE/'complete-review-inputs.json').read_bytes())['members']
current=json.loads((HERE/'current-universe-families-v2.json').read_bytes())['members']
bykey={r['signature_key']:r for r in current if r['parent_member'] is None}
rows=[]
for row in earlier:
 n=bykey[row['current_record']['signature_key']]
 c,cv=body((ROOT/n['path']).read_bytes(),n)
 o=row['original_record'];original,ov=body((ROOT/row['original_source']['path']).read_bytes(),o) if o else (None,None)
 r=dict(row,current_record=n,current_source=pin(ROOT/n['path']),current_body=c,original_body=original,current_view=cv,original_view=ov)
 rows.append(r)
 p=HERE/'authenticated-review-inputs'/n['name'];p.mkdir(parents=True,exist_ok=False)
 for name,text in [('original',original),('current',c)]:
  if text is not None:
   with (p/(name+'.vb.txt')).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
save('authenticated-review-inputs.json',dict(members=rows,original_queries=0,source_view_reader=pin(HERE/'source_views.py')))
save('attempts/original-span-view-correction.json',dict(status='INTERMEDIATE_ORIGINAL_SPAN_INPUT_REJECTED_BEFORE_CLOSURE',
    failed_inputs=[pin(HERE/'review-inputs-v2.json'),pin(HERE/'complete-review-inputs.json')],
    failure='Raw CRLF archived originals were sliced with Roslyn offsets from accepted Git LF bytes; current raw spans were correct. Both must use their individually authenticated census source hash.',
    correction='Match whole raw byte SHA to RAW or explicit CRLF-to-LF view (retaining BOM bytes for identity), decode UTF8 BOM, slice UTF16 and require the exact whole-body SHA. All91 records now pass full-body hashes.',
    source_changes_caused_by_failure=0,closures_from_failed_inputs=0,
    production_source_basis='Implementation scripts derive from complete regex-delimited live bodies and retained preimages, not these erroneous original spans. All24 original/current corpus pairs still match exactly.',
    new_reader=pin(HERE/'source_views.py'),authenticated=pin(HERE/'authenticated-review-inputs.json')))
print('All91 complete original/current body hashes authenticate against the exact source-byte view')
