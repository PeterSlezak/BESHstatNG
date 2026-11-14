"""Independently recount copied managed, Python and Node identity receipts."""
import hashlib,json,re
from collections import Counter
def inspect(load,prefix,raw):
 def need(value,message):
  if not value:raise ValueError(message)
 v=load(prefix+'validation-v1.json');rows=load(prefix+'managed-Debug-identities-v1.json')
 need(dict(Counter(r['outcome'] for r in rows['tests']))==v['managed_Debug']['outcomes'],'Managed outcome recount')
 key=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'])
 current={key(r):r for r in rows['tests']};need(len(current)==len(rows['tests']),'Duplicate managed identity occurrence')
 old=load('evidence/04A38/completion/w2/continuation-v5/managed-Debug-identities-v3.json')
 need(all(k in current and current[k]['outcome']==r['outcome'] for r in old['tests'] for k in [key(r)]),'Managed inherited outcome drift')
 fields=lambda r:(*key(r),r['outcome'],r['message_sha256'])
 need(Counter(map(fields,rows['nonpasses']))==Counter(map(fields,old['nonpasses'])),'Managed inherited message drift')
 need({key(r) for r in rows['new_tests']}==set(current)-{key(r) for r in old['tests']} and all(r['outcome']=='Passed' for r in rows['new_tests']),'New managed test unsupported')
 selected=[]
 groups=[r for r in load(prefix+'focused-families-v2-Release.json')['results'] if r['kind']=='TEST']+[r for r in load(prefix+'selected-Core-families-v2.json')['results'] if r['kind']=='TEST' and r['configuration']=='Release']
 for group in groups:
  cases=group['tests']['records'];need(Counter(r['outcome'] for r in cases)==Counter(group['tests']['outcomes']),'Selected Release case outcomes')
  selected.extend((group['suite'],r['class_name'],r['method'],r['test_name']) for r in cases)
 need(len(selected)==len(set(selected))==v['selected_Release']['distinct_identities']==192,'Selected Release identity recount')
 need(v['selected_Release']['full_suite']=='NOT_RUN_OWNER_PREFERENCE','Full Release falsely claimed')
 p=load(prefix+'nonpasses-v2.json')['python'];po=load('evidence/04A38/completion/w2/continuation-v5/nonpasses-v3.json')['python']
 need(Counter(i for g in p for i in g['identities'])==Counter(i for g in po for i in g['identities']),'Python identity/occurrence drift')
 for g in p:need(len(g['identities'])==g['total']==g['passed']+g['failed']+g['errors'],'Python population recount')
 need({k:sum(g[k] for g in p) for k in ['total','passed','failed','errors']}==v['Python'],'Python outcome sum')
 delta=load(prefix+'python-source-pin-delta-v1.json');historical=load(delta['historical_record']['path'])['current_member_anchors']
 need(delta['outcome']=='FAIL' and delta['occurrences']==1 and len(delta['changed_members'])==2,'New source-pin failure misclassified')
 t=delta['transition'];need(t in load(prefix+'source-transitions-final-v1.json')['files'],'Unrecorded source-pin transition')
 for row in delta['changed_members']:
  need(row['entry'] in historical,'Historical source-pin anchor fabricated')
  pattern=r'^    (?:Public|Private|Friend)\s+(?:Shared\s+)?Function\s+'+re.escape(row['member'])+r'\b.*?^    End Function'
  for source,expected in [(t['preimage']['path'],row['entry']['after_sha256']),(t['after']['path'],row['current']['after_sha256'])]:
   match=re.search(pattern,raw(source).decode('utf-8-sig').replace('\r\n','\n'),re.M|re.S)
   need(match is not None and hashlib.sha256(match[0].encode()).hexdigest()==expected,'Source-pin body/preimage mismatch')
 node=load(prefix+'node-identities-v1.json')['suites'];actual={n['suite']:n for n in v['Node']}
 for g in node:
  record=actual[g['suite']];need(len(g['identities'])==record['total'] and hashlib.sha256(json.dumps(sorted(g['identities'])).encode()).hexdigest()==record['complete_identity_multiset_sha256'],'Node identity recount')
 need(sum(n['passed'] for n in v['Node'])==v['Node_total']==916,'Node total recount')
 return dict(status='COPIED_VALIDATION_IDENTITY_OCCURRENCE_RECOUNT_PASS',managed=v['managed_Debug']['outcomes'],selected_Release=192,Python=v['Python'],Node=916,qualification='Raw private test logs omitted; these are authenticated derived execution receipts, not a new copied execution of production tests')
