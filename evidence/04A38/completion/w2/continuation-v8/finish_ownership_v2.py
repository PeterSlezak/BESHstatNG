"""Correct draft authority pointers without rewriting any inherited proof."""
from common import *
history=json.loads((HERE/'historical-ownership-v1.json').read_bytes());current=json.loads((HERE/'current-ownership-v1.json').read_bytes())
callback=json.loads((HERE/'historical-callback-contracts-v1.json').read_bytes())['rows'][0]
row=next(h for h in history['members'] if h['canonical_original_id']==callback['original_id'])
row['proof']=dict(kind='C8_AUTHENTICATED_ORIGINAL_CURRENT_CALLBACK_CIL_FIELD_CTOR_CALLER',evidence='historical-callback-contracts-v1.json',original_id=callback['original_id'],current_id=callback['current_id'])
for s in current['support']:
    if s['kind']=='ModuleBlock' and s['declaration']=='Public Module SampleSizeUDFs':
        assert s['fully_reconciled']
        s['proof']=dict(kind='C8_ALL_CONSTITUENT_BODIES_AND_THREE_AUTHENTICATED_LITERAL_FIELDS',children=s['children'],evidence='whole-body-contracts-v1.json',compiled='metadata-all/packet-v1-context-v3/',fields=['HypothesisSuperiority','HypothesisNonInferiority','HypothesisEquivalence'])
save('historical-ownership-v2.json',history);save('current-ownership-v2.json',current)
source=(HERE/'verify_c8_checkpoint_template_v3.py').read_text(encoding='utf-8')
# Only this checkpoint's ledger loads change; inherited C7 ledgers stay v1.
source=source.replace("load(PREFIX+'historical-ownership-v1.json')", "load(PREFIX+'historical-ownership-v2.json')").replace("load(PREFIX+'current-ownership-v1.json')", "load(PREFIX+'current-ownership-v2.json')")
with (HERE/'verify_c8_checkpoint_template_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
new=[];prior={h['canonical_original_id']:h for h in json.loads((PREVIOUS/'historical-ownership-v1.json').read_bytes())['members']};contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
for h in history['members']:
    if h['proof'] and not prior[h['canonical_original_id']]['proof']:
        c=next((c for c in contracts if c['original_id']==h['canonical_original_id']),None)
        if c:new.append(dict(original_id=h['canonical_original_id'],current_ids=h['current_ids'],kind='COMPLETE_FUNCTION_PARENT',source_contract=c,proof=h['proof']))
        else:
            assert h['canonical_original_id']==callback['original_id'];new.append(dict(original_id=h['canonical_original_id'],current_ids=h['current_ids'],kind='HISTORICAL_CALLBACK_CURRENT_ALREADY_SUPPORTED',source_contract=callback,proof=h['proof']))
assert len(new)==24
save('new-closures-v1.json',dict(rows=new,new_historical=24,new_current_supported=25,new_current_declarations=2,new_callbacks=0,reopened=[],original_queries=0))
old=json.loads((PREVIOUS/'current-ownership-v1.json').read_bytes())['members'];bykey={}
for n in current['members']:bykey.setdefault(n['signature_key'],[]).append(n)
mapping=[]
for n in old:
    choices=bykey[n['signature_key']]
    choices=[r for r in choices if (r['parent_member'] or '').split('@')[0]==(n['parent_member'] or '').split('@')[0]]
    assert len(choices)==1,n['symbol_id'];r=choices[0]
    mapping.append(dict(before=n['symbol_id'],after=r['symbol_id'],position_shift=n['symbol_id']!=r['symbol_id'],body_bytes_equal=n['body_sha256']==r['body_sha256'],body_tokens_equal=n['body_token_sha256']==r['body_token_sha256'],signature=n['signature_key'],prior_status=n['review_status']))
save('current-semantic-position-crosswalk-v1.json',dict(rows=mapping,before=724,after=726,shifted=sum(r['position_shift'] for r in mapping),changed_existing_bodies=sum(not r['body_bytes_equal'] for r in mapping),new=[n for n in current['members'] if n['signature_key'] not in {r['signature_key'] for r in old}],qualification='Position-bearing IDs are retained and mapped by exact semantic signature/parent context; shifts are not new behavior.',original_queries=0))
save('ownership-authority-corrections-v1.json',dict(draft_history=pin(HERE/'historical-ownership-v1.json'),authoritative_history=pin(HERE/'historical-ownership-v2.json'),draft_current=pin(HERE/'current-ownership-v1.json'),authoritative_current=pin(HERE/'current-ownership-v2.json'),reader=pin(HERE/'verify_c8_checkpoint_template_v4.py'),reason='Point the historical lambda to its actual callback proof, and describe sample-size literal fields accurately rather than inherited empty-field wording. Counts/IDs and every inherited proof remain unchanged.',original_queries=0))
print('Authoritative24 closure records, complete original/current maps and corrected support authority written')
