from common import *
d=json.loads((HERE/'complete-review-inputs.json').read_bytes())
for name in ['ANOVA1','PrepareWrappedResultTableForUdf','TryGetOptionalProbabilityThresholds','KW_STAT']:
 r=next(r for r in d['members'] if r['current_record']['name']==name)
 for side in ['original','current']:
  record=r[side+'_record'];p=ROOT/(r['original_source']['path'] if side=='original' else r['current_source']['path'])
  raw=p.read_bytes();text=raw.decode('utf-8-sig');expected=record['source_sha256']
  views={'raw':text,'LF':text.replace('\r\n','\n')}
  result=[]
  for label,t in views.items():
   body=t.encode('utf-16-le')[record['span_start']*2:(record['span_start']+record['span_length'])*2].decode('utf-16-le')
   result.append(dict(view=label,source_sha256=sha(t.encode()),source_hash_matches=sha(t.encode())==expected,body_hash_matches=sha(body.encode())==record['body_sha256'],body_start=body[:65],body_end=body[-35:]))
  print(json.dumps(dict(name=name,side=side,raw_bytes_hash=sha(raw),expected_source=expected,variants=result)))
