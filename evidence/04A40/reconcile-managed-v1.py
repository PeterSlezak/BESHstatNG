from common40_v3 import *
from collections import Counter,defaultdict
import xml.etree.ElementTree as ET
sys.path.append(str(ROOT/'evidence/04A38'))
from baseline_report import index,clean
from validation_report import safe_label
NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
config=sys.argv[1];execution=load(HERE/('build-and-tests/'+('Debug-full' if config=='Debug' else 'Release-targeted')+'-executed-v1.json'))
prior=load(ROOT/'evidence/04A39/build-and-tests/Debug-identities-v1.json')
old={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in prior['tests']}
oldnon={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in prior['nonpasses']}
tests=[];non=[];actual=set()
for part in execution['parts']:
    assert pin(part['cases']['path'])==part['cases'] and pin(part['TRX']['path'])==part['TRX']
    traces=defaultdict(list)
    for _,node in ET.iterparse(ROOT/part['TRX']['path'],events=['end']):
        if node.tag==NS+'UnitTestResult':
            if node.attrib['outcome']!='Passed':traces[node.attrib['testName']].append(node.findtext('.//'+NS+'StackTrace',''))
            node.clear()
        elif node.tag==NS+'UnitTest':node.clear()
    for (identity,occurrence),r in index(load(ROOT/part['cases']['path'])['records']).items():
        key=(part['suite'],identity,occurrence);assert key not in actual;actual.add(key);previous=old.get(key)
        row=dict(configuration=config,suite=part['suite'],part=part['part'],identity_sha256=identity,occurrence=occurrence,
            class_name=r['class_name'],method=r['method'],test_name=safe_label(r['test_name']),outcome=r['outcome'],
            classification='ADDED40' if previous is None else 'RETAINED_IDENTICAL_OUTCOME' if previous['outcome']==r['outcome'] else 'CHANGED_OUTCOME')
        tests.append(row)
        if r['outcome']!='Passed':
            trace=traces[r['test_name']].pop(0);oldrow=oldnon.get(key);mh=sha(r['message'].encode());th=sha(trace.encode())
            category='NEW_NONPASS' if oldrow is None else 'RETAINED_CHANGED_MESSAGE' if mh!=oldrow['message_sha256'] else 'RETAINED_CHANGED_TRACE' if th!=oldrow['traceback_sha256'] else 'RETAINED_IDENTICAL'
            non.append(dict(row,classification=category,message=clean(r['message']),message_sha256=mh,traceback=clean(trace),traceback_sha256=th,previous=oldrow,raw_TRX=part['TRX']))
missing=[old[k] for k in old.keys()-actual] if config=='Debug' else []
added=[r for r in tests if r['classification']=='ADDED40']
assert all('Regression40' in r['class_name'] for r in added) and all(r['outcome']=='Passed' for r in added)
assert not missing and not any(r['classification']=='CHANGED_OUTCOME' for r in tests)
save('build-and-tests/'+config+'-identities-v1.json',dict(summary=dict(configuration=config,scope=execution['mode'],identities=len(tests),outcomes=dict(Counter(r['outcome'] for r in tests)),
    added=len(added),missing=missing,nonpass_deltas=dict(Counter(r['classification'] for r in non))),tests=tests,nonpasses=non,added=added,baseline39=pin(ROOT/'evidence/04A39/build-and-tests/Debug-identities-v1.json')))
assert not any(r['classification'] in ['NEW_NONPASS','RETAINED_CHANGED_MESSAGE'] for r in non)
if config=='Debug':assert len(tests)==21139 and len(added)==12 and len(non)==14
print(config,'identity multiset reconciled',len(tests),dict(Counter(r['outcome'] for r in tests)),'nonpasses',dict(Counter(r['classification'] for r in non)),flush=True)
