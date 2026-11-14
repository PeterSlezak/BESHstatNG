"""Keep parent outcomes and individual nonpass events separate."""
from common import *
from collections import Counter
sys.path.append(str(ROOT/'evidence/04A38'))
from baseline_report import clean

target=HERE/'python-node-scanners/campaign-v1'
inventory=load(target/'python-discovery-inventory-v1.json')['entries']
receipts=load(target/'python-worker-receipts-v1.json')['receipts']
records=[];events=[];occurrences=Counter();executions=[]
for r in receipts:
    assert r['raw'] and pin(ROOT/r['raw']['path'])==r['raw']
    raw=load(ROOT/r['raw']['path'])
    assert raw['exit_code']==r['command']['exit_code']
    journal=ROOT/raw['parent_journal']['path'];assert pin(journal)==raw['parent_journal']
    entries=[json.loads(line) for line in journal.read_text(encoding='utf-8').splitlines()]
    assert [x['value'] for x in entries if x['kind']=='COMPLETE']==raw['records']
    assert Counter(x['value']['id'] for x in entries if x['kind']=='START')==Counter(x['id'] for x in raw['records'])
    records.extend(raw['records']);executions.append(dict(lane=r['lane'],phase=r['phase'],raw=r['raw'],parents=raw['testsRun'],outcomes=raw['outcomes'],exit_code=raw['exit_code']))
    for parent in raw['records']:
        for e in parent['events']:
            assert sha(e['traceback'].encode())==e['traceback_sha256']
            key=(parent['id'],e.get('subtest'),e['outcome'])
            ordinal=occurrences[key];occurrences[key]+=1
            events.append(dict(e,identity=parent['id'],occurrence=ordinal))
assert Counter(r['id'] for r in records)==Counter(r['id'] for r in inventory) and len(records)==956
oldparents={r['id']:r for r in load(ACCEPTED/'python-effective-identities-v1.json')['entries']}
parents=[dict(id=r['id'],outcome=r['outcome'],previous=oldparents.get(r['id'],{}).get('outcome'),
    classification='RETAINED_IDENTICAL' if oldparents.get(r['id'],{}).get('outcome')==r['outcome'] else 'NEW_OR_CHANGED_NONPASS') for r in records]
key=lambda r:(r['identity'],r['outcome'],clean((r.get('subtest') or '')[len(r['identity']):].strip()),r['occurrence'])
old={key(r):r for r in load(ACCEPTED/'python-first-failures-v1.json')['records']}
details=[]
for r in events:
    previous=old.get(key(r));message_hash=sha(r.get('message','').encode())
    if not previous:classification='NEW_NONPASS'
    elif previous.get('raw_message_sha256',previous.get('message_sha256'))!=message_hash:classification='RETAINED_CHANGED_MESSAGE'
    elif previous['traceback_sha256']!=r['traceback_sha256']:classification='RETAINED_CHANGED_TRACE'
    else:classification='RETAINED_IDENTICAL'
    details.append(dict(clean(r),message_sha256=message_hash,classification=classification,previous=previous))
lost=[dict(identity=k[0],outcome=k[1],subtest=k[2],occurrence=k[3],classification='MISSING_IDENTITY') for k in old.keys()-{key(r) for r in events}]
summary=dict(parents=len(parents),parent_outcomes=dict(Counter(r['outcome'] for r in parents)),
    nonpass_event_outcomes=dict(Counter(r['outcome'] for r in events)),event_categories=dict(Counter(r['classification'] for r in details)),
    parent_changes=[r for r in parents if r['classification']!='RETAINED_IDENTICAL'],missing_events=lost,
    two_historical_privacy_errors_retained=sum('UnicodeDecodeError' in r.get('traceback','') for r in events),
    prior_message_trace_drift='W2 one changed message and three changed raw traces remain disclosed; no waiver',
    raw_event_count=len(events),campaigns=1,worker_schedule=load(target/'python-worker-receipts-v1.json')['schedule'])
save('python-node-scanners/python-identities-v1.json',dict(summary=summary,parents=parents,events=details,executions=executions))
print(json.dumps(summary),flush=True)
assert not summary['parent_changes'] and not lost
assert summary['parent_outcomes']==dict(PASS=830,FAIL=113,ERROR=13)
assert summary['nonpass_event_outcomes']==dict(FAIL=185,ERROR=13)
assert not any(r['classification']=='NEW_NONPASS' for r in details)
