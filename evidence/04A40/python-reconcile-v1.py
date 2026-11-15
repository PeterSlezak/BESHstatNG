"""Exact raw parent/event reconciliation; publication fields contain no raw diagnostics."""
from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'evidence/04A40'))
from common40_v3 import *
import collections,re,ast
sys.path.append(str(ROOT/'evidence/04A38'))
from baseline_report import clean
lock();target=PRIVATE/'python-tools';inv=load(target/'python-discovery-inventory-v1.json')['entries']
receipts=load(target/'python-worker-receipts-v1.json');now=[];parts=[]
for receipt in receipts['receipts']:
    assert receipt['raw'] and pin(receipt['raw']['path'])==receipt['raw']
    raw=load(ROOT/receipt['raw']['path']);assert raw['exit_code']==receipt['command']['exit_code']
    assert pin(raw['parent_journal']['path'])==raw['parent_journal']
    journal=[json.loads(line) for line in (ROOT/raw['parent_journal']['path']).read_text(encoding='utf-8').splitlines()]
    assert [x['value'] for x in journal if x['kind']=='COMPLETE']==raw['records']
    assert collections.Counter(x['value']['id'] for x in journal if x['kind']=='START')==collections.Counter(x['id'] for x in raw['records'])
    now+=raw['records'];parts.append(dict(lane=receipt['lane'],phase=receipt['phase'],raw=receipt['raw'],outcomes=raw['outcomes'],exit_code=raw['exit_code']))
assert len(now)==956 and collections.Counter(x['id'] for x in now)==collections.Counter(x['id'] for x in inv)
old_document=load(ROOT/'evidence/04A39/python-node-scanners/python-identities-v1.json');previous=[]
for part in old_document['parts']:
    assert pin(part['raw']['path'])==part['raw'];previous+=load(ROOT/part['raw']['path'])['records']
assert len(previous)==956 and collections.Counter(x['id'] for x in previous)==collections.Counter(x['id'] for x in now)
def events(records):
    seen=collections.Counter();out={}
    for p in records:
        for e in p['events']:
            assert sha(e['traceback'].encode())==e['traceback_sha256']
            base=(p['id'],e['outcome'],clean((e.get('subtest') or '')[len(p['id']):].strip()));occ=seen[base];seen[base]+=1
            out[base+(occ,)]=dict(e,identity=p['id'],occurrence=occ)
    return out
old=events(previous);current=events(now);parents={r['id']:r for r in previous};inventory={r['id']:r for r in inv}
def source(identity):
    row=inventory[identity];path=ROOT/row['module_path'];tree=ast.parse(path.read_text(encoding='utf-8-sig'))
    name=identity.rsplit('.',1)[-1];matches=[n for n in ast.walk(tree) if isinstance(n,(ast.FunctionDef,ast.AsyncFunctionDef)) and n.name==name]
    return dict(source=pin(path),method=name,lines=[[n.lineno,n.end_lineno] for n in matches])
parent_rows=[dict(id=r['id'],before=parents[r['id']]['outcome'],outcome=r['outcome'],classification='RETAINED_OUTCOME' if parents[r['id']]['outcome']==r['outcome'] else 'CHANGED_OUTCOME_REQUIRES_REVIEW',source=source(r['id'])) for r in now]
def shape(trace):return re.sub(r'File "([^"\n]+)"',lambda m:'File "'+m.group(1).replace('\\','/').rsplit('/',1)[-1]+'"',trace)
cross=[];private=[]
for key in old.keys()|current.keys():
    a=old.get(key);b=current.get(key);stable=sha(json.dumps(key,sort_keys=True).encode())
    before=dict(message_sha256=sha(a.get('message','').encode()),traceback_sha256=a['traceback_sha256']) if a else None
    after=dict(message_sha256=sha(b.get('message','').encode()),traceback_sha256=b['traceback_sha256']) if b else None
    if a is None:kind='NEW_NONPASS'
    elif b is None:kind='MISSING_NONPASS_EVENT'
    elif before==after:kind='RETAINED_IDENTICAL'
    elif before['message_sha256']!=after['message_sha256']:kind='RETAINED_CHANGED_MESSAGE'
    else:kind='RETAINED_CHANGED_TRACE'
    disposition='UNRESOLVED_REQUIRES_REVIEW'
    if kind=='RETAINED_IDENTICAL':disposition='INHERITED_NONPASS_UNWAIVED'
    elif a and b and a.get('message')==b.get('message') and shape(a['traceback'])==shape(b['traceback']):disposition='ENVIRONMENT_FILE_PATH_ONLY;NONPASS_UNWAIVED'
    cross.append(dict(stable_event_id=stable,identity=key[0],outcome=key[1],subtest_identity_sha256=sha(key[2].encode()),occurrence=key[3],before=before,current=after,classification=kind,disposition=disposition,source=source(key[0])))
    private.append(dict(stable_event_id=stable,key=key,before=a,current=b,shape_equal=bool(a and b and shape(a['traceback'])==shape(b['traceback']))))
save(PRIVATE/'python-event-crosswalk-raw-v1.json',dict(events=private))
history=load(ROOT/'evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json');assert len(history['changes'])==54
post=load(PRIVATE/'python-input-postcheck-v1.json');assert not post['changed'] and post['command']['exit_code']==0
summary=dict(parents=956,parent_outcomes=dict(collections.Counter(r['outcome'] for r in now)),before_outcomes=dict(collections.Counter(r['outcome'] for r in previous)),
    event_outcomes=dict(collections.Counter(r['outcome'] for r in current.values())),event_categories=dict(collections.Counter(r['classification'] for r in cross)),
    parent_changes=[r for r in parent_rows if r['classification']!='RETAINED_OUTCOME'],new_or_missing_events=[r['stable_event_id'] for r in cross if r['classification'] in ['NEW_NONPASS','MISSING_NONPASS_EVENT']],
    unexplained_diagnostic_events=[r['stable_event_id'] for r in cross if r['disposition']=='UNRESOLVED_REQUIRES_REVIEW'],
    dropped_parents=[],duplicated_parents=[],campaigns=1,workers_at_peak=3,scanner_helpers=0)
save('python/parent-event-reconciliation-v1.json',dict(summary=summary,parents=parent_rows,events=cross,parts=parts,
    protected_raw=pin(PRIVATE/'python-event-crosswalk-raw-v1.json'),input_freeze=pin(PRIVATE/'python-input-freeze-v1.json'),input_postcheck=pin(PRIVATE/'python-input-postcheck-v1.json'),
    original54=pin(ROOT/'evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json'),original54_events_retained_unwaived=True,source_test_discovery=pin(HERE/'python/discovery-v1.json'),
    historical_privacy=dict(original39_each=1941,earlier_each=1599,added_each=342,W3='TWO_ERROR_PARENTS_UNWAIVED'),repository_privacy='FAIL_UNWAIVED;ACTUAL_CURRENT_FINDINGS_RECONCILED_SEPARATELY'))
print(json.dumps(summary),flush=True)
