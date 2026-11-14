"""Independent parent/event multiset and exact W3 nonpass crosswalk."""
from common39 import *
from collections import Counter
sys.path.append(str(ROOT/'evidence/04A38'))
from baseline_report import clean

target=HERE/'python-node-scanners/campaign-v2'
inventory=load(target/'python-discovery-inventory-v1.json')['entries']
receipts=load(target/'python-worker-receipts-v1.json')
records=[];events=[];counts=Counter();parts=[]
for receipt in receipts['receipts']:
    assert receipt['raw'] and pin(receipt['raw']['path'])==receipt['raw']
    raw=load(ROOT/receipt['raw']['path']);assert raw['exit_code']==receipt['command']['exit_code']
    assert pin(raw['parent_journal']['path'])==raw['parent_journal']
    journal=[json.loads(line) for line in (ROOT/raw['parent_journal']['path']).read_text(encoding='utf-8').splitlines()]
    assert [x['value'] for x in journal if x['kind']=='COMPLETE']==raw['records']
    assert Counter(x['value']['id'] for x in journal if x['kind']=='START')==Counter(x['id'] for x in raw['records'])
    records+=raw['records'];parts.append(dict(lane=receipt['lane'],phase=receipt['phase'],raw=receipt['raw'],outcomes=raw['outcomes'],exit_code=raw['exit_code']))
    for parent in raw['records']:
        for e in parent['events']:
            assert sha(e['traceback'].encode())==e['traceback_sha256']
            key=(parent['id'],e.get('subtest'),e['outcome']);ordinal=counts[key];counts[key]+=1
            events.append(dict(e,identity=parent['id'],occurrence=ordinal))
assert Counter(x['id'] for x in records)==Counter(x['id'] for x in inventory) and len(records)==956
old=load(W3/'python-node-scanners/python-identities-v1.json')
parents={x['id']:x for x in old['parents']}
parent_rows=[dict(id=x['id'],outcome=x['outcome'],classification='RETAINED_IDENTICAL' if parents.get(x['id'],{}).get('outcome')==x['outcome'] else 'NEW_OR_CHANGED_NONPASS') for x in records]
key=lambda r:(r['identity'],r['outcome'],clean((r.get('subtest') or '')[len(r['identity']):].strip()),r['occurrence'])
prior={key(x):x for x in old['events']};detail=[]
for row in events:
    previous=prior.get(key(row));message=sha(row.get('message','').encode())
    category='NEW_NONPASS' if previous is None else 'RETAINED_CHANGED_MESSAGE' if message!=previous['message_sha256'] else 'RETAINED_CHANGED_TRACE' if row['traceback_sha256']!=previous['traceback_sha256'] else 'RETAINED_IDENTICAL'
    detail.append(dict(clean(row),message_sha256=message,classification=category,previous=previous))
missing=[dict(identity=k[0],outcome=k[1],subtest=k[2],occurrence=k[3],classification='MISSING_IDENTITY') for k in prior.keys()-{key(x) for x in events}]
summary=dict(parents=len(records),parent_outcomes=dict(Counter(x['outcome'] for x in records)),
    parent_changes=[x for x in parent_rows if x['classification']!='RETAINED_IDENTICAL'],
    events=len(events),event_outcomes=dict(Counter(x['outcome'] for x in events)),
    event_categories=dict(Counter(x['classification'] for x in detail)),missing=missing,
    privacy_UnicodeDecodeError_events=sum('UnicodeDecodeError' in x.get('traceback','') for x in events),
    inherited_W2_drift='One message and three raw traces changed at accepted W2 replacement checkpoint; still disclosed and unwaived.',
    schedule=receipts['schedule'],campaigns=1)
save('python-node-scanners/python-identities-v1.json',dict(summary=summary,parents=parent_rows,events=detail,parts=parts,
    baseline=pin(W3/'python-node-scanners/python-identities-v1.json')))
print(json.dumps(summary),flush=True)
# Preserve and report changed/nonpass results; do not rerun or rebaseline.
assert not summary['parent_changes'] and not missing and not any(x['classification']=='NEW_NONPASS' for x in detail)
assert summary['parent_outcomes']==dict(PASS=830,FAIL=113,ERROR=13)
assert summary['event_outcomes']==dict(FAIL=185,ERROR=13)
