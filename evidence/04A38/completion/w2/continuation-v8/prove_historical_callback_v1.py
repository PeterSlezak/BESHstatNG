"""Close only exact queued historical Get2D lambda using real unchanged CIL/cache/ctors."""
from common import *
from source_views import body
history=json.loads((PREVIOUS/'historical-ownership-v1.json').read_bytes())['members'];queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())['first_members']
q=queue[17];assert q['name']=='lambda' and q['original_id'].endswith('|103f86b31077cee4@803:60')
h=next(r for r in history if r['canonical_original_id']==q['original_id']);assert not h['proof']
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
old=next(n for n in original['members'] if n['symbol_id']==h['git_original_id'])
now=next(n for n in u['members'] if n['signature_key'] in {k.split('@')[0] for k in h['current_ids']})
snapshot=next(r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files'] if r['source']['path']==h['path'])
oldtext,oldview=body((ROOT/snapshot['path']).read_bytes(),old);newtext,newview=body((ROOT/now['path']).read_bytes(),now)
assert oldtext==newtext and old['body_token_sha256']==now['body_token_sha256']
contexts=json.loads((HERE/'get2d-original-current-context-inspection-v1.json').read_bytes())['rows'];bindings=[]
semantic=lambda r:{k:v for k,v in r.items() if k!='raw_IL_sha256'}
for row in contexts:
    before,after=row['sides'];assert (before['side'],after['side'])==('original','current')
    assert semantic(before['parent'])==semantic(after['parent']) and semantic(before['target'])==semantic(after['target'])
    assert before['target']['member_name']=='_Lambda$__94-0' and before['target']['method']==after['target']['method']
    assert before['fields']==after['fields']
    assert [semantic(c) for c in before['constructors']]==[semantic(c) for c in after['constructors']]
    fields={f['name']:f for f in before['context']['fields']};newfields={f['name']:f for f in after['context']['fields']}
    required=['$I','$I94-0'];assert all(fields[k]==newfields[k] for k in required)
    assert fields['$I']['is_static'] and fields['$I']['is_readonly'] and fields['$I94-0']['is_static']
    bindings.append(dict(configuration=row['configuration'],original=before,current=after,required_field_names=required,actual_method_aliases=[],qualification='Get2D caller, callback instructions/locals/exceptions, actual cache fields and complete constructors are identical. Other pre-existing cache fields are retained with their existing C7 authority; UdfDataImport remains OPEN. No generic alias/substitution rule or native xlCoerce execution.'))
save('historical-callback-contracts-v1.json',dict(rows=[dict(original_id=h['canonical_original_id'],current_id=now['symbol_id'],original_record=old,current_record=now,original_source=snapshot,current_source=pin(ROOT/now['path']),original_whole_body=oldtext,current_whole_body=newtext,original_source_view=oldview,current_source_view=newview,original_fragments=[f for f in original['fragments'] if f['member_id']==old['symbol_id']],current_fragments=[f for f in u['fragments'] if f['member_id']==now['symbol_id']],configurations=bindings,new_historical_support=True,new_current_support=False)],original_queries=0))
print('Queued historical lambda full source/CIL/cache/ctor/caller proof PASS; current callback was already supported')
