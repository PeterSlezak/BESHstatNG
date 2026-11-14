"""Independent set/count/position reconciliation after actual predecessor replays."""
from common import *
entry=json.loads((HERE/'entry-executable-authentication-v1.json').read_bytes());assert entry['status']=='AUTHENTICATED'
history=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members']
current=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());cm={n['symbol_id']:n for n in current['members']}
older=PREVIOUS.parent/'continuation-v11'
oldh=json.loads((older/'historical-ownership-v2.json').read_bytes())['members'];oldc=json.loads((older/'current-ownership-v2.json').read_bytes())
mapping=json.loads((PREVIOUS/'current-semantic-position-crosswalk-v1.json').read_bytes())['rows']
assert len(mapping)==len(cm)==726 and {r['before'] for r in mapping}=={n['symbol_id'] for n in oldc['members']} and {r['after'] for r in mapping}==set(cm)
assert sum(r['before']!=r['after'] for r in mapping)==79 and sum(not r['body_bytes_equal'] for r in mapping)==5
new=json.loads((PREVIOUS/'new-closures-final-v1.json').read_bytes());newids={r['original_id'] for r in new['rows']}
closed={r['canonical_original_id'] for r in history if r['proof']};priorclosed={r['canonical_original_id'] for r in oldh if r['proof']}
assert len(history)==704 and len(closed)==435 and len(priorclosed)==402 and closed-priorclosed==newids and len(newids)==33
newcurrent={r['current_ids'][0] for r in new['rows']};oldmap={r['before']:r['after'] for r in mapping}
closedcurrent={r['symbol_id'] for r in current['members'] if not r['review_status'].startswith('OPEN')}
priorcurrent={oldmap[r['symbol_id']] for r in oldc['members'] if not r['review_status'].startswith('OPEN')}
assert len(closedcurrent)==465 and closedcurrent-priorcurrent==newcurrent and len(newcurrent)==33
assert sum(cm[k]['parent_member'] is None for k in newcurrent)==26 and sum(cm[k]['parent_member'] is not None for k in newcurrent)==7
support=[r for r in current['support'] if not r['already_in_primary_population']]
assert len(current['support'])==325 and len(support)==303 and sum(r['fully_reconciled'] for r in support)==252 and len(new['new_support'])==2
queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())
assert {r['original_id'] for r in queue['members']}=={r['canonical_original_id'] for r in history if not r['proof']} and len(queue['members'])==269
assert {r['current_id'] for r in queue['current_OPEN']}==set(cm)-closedcurrent and len(queue['current_OPEN'])==261
deltas=json.loads((PREVIOUS/'compiled-method-deltas-v1.json').read_bytes())['configurations']
for config in deltas:
    assert len(config['changed'])==5 and len(config['added'])==5 and not config['lost']
    assert all((r.get('current',r))['declaring_type'].startswith('BESHStatNG.ExcelCommon.') for r in config['added'])
actual=json.loads((RUNROOT/'commands/w2c13-prior-sealed-reader-v2/stdout.log').read_bytes())
assert actual['historical']==dict(total=704,closed=435,OPEN=269) and actual['current']==dict(total=726,closed=465,OPEN=261)
assert (actual['fragments']['original']['covered'],actual['fragments']['original']['total'])==(10457,20683)
assert (actual['fragments']['current']['covered'],actual['fragments']['current']['total'])==(6086,14732)
assert all(r['exports']==268 for r in actual['compiled_retention'])
save('v12-independent-entry-reconciliation-v1.json',dict(status='ACTUAL_SEAL_AND_COMPLETE_UNIVERSES_POSITION_SOURCE_ABI_CIL_RECONCILED',
    historical=dict(supported=435,OPEN=269,total=704,new_v12=33),current=dict(supported=465,OPEN=261,total=726,new_v12=33),support=dict(supported=252,OPEN=51,distinct=303,raw=325,new_v12=2),
    semantic_mapping=dict(entries=726,shifted_source_ids=79,changed_native_bodies=5,lost_methods=0),new_v12_parents=26,new_v12_callbacks=7,
    compiled_changes=[dict(configuration=r['configuration'],changed=5,added_common=5,lost=0) for r in deltas],
    original_fragments=dict(covered=10457,total=20683),current_fragments=dict(covered=6086,total=14732),
    actual_sealed_result=pin(RUNROOT/'commands/w2c13-prior-sealed-reader-v2/stdout.log'),canonical_queue=pin(PREVIOUS/'remaining-work-queue-v1.json'),
    full_current_mapping=pin(PREVIOUS/'current-semantic-position-crosswalk-v1.json'),v12_new_closures=pin(PREVIOUS/'new-closures-final-v1.json'),original_queries=0,production_edits=0))
print('V12 full identity/position/body/compiled/export entry reconciliation PASS_NOT_READY')
