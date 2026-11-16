from common45 import *
import collections,xml.etree.ElementTree as ET
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from baseline_report import index
from validation_report import safe_label
lock();config=sys.argv[1];execution=load(HERE/('build-and-tests/'+config+'-executed-v1.json'))
old=load(ROOT/'evidence/04A44/build-and-tests/Debug-identities-v1.json')
previous={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in old['tests']}
oldnon={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in old['nonpasses']}
NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
tests=[];non=[];raw=[];actual=set()
for part in execution['parts']:
    exact(part['cases']);exact(part['TRX']);traces=collections.defaultdict(list)
    for _,node in ET.iterparse(ROOT/part['TRX']['path'],events=['end']):
        if node.tag==NS+'UnitTestResult':
            if node.attrib['outcome']!='Passed':traces[node.attrib['testName']].append(node.findtext('.//'+NS+'StackTrace',''))
            node.clear()
        elif node.tag==NS+'UnitTest':node.clear()
    for (identity,occurrence),r in index(load(ROOT/part['cases']['path'])['records']).items():
        key=(part['suite'],identity,occurrence);assert key not in actual;actual.add(key);before=previous.get(key)
        row=dict(configuration=config,suite=part['suite'],part=part['part'],identity_sha256=identity,occurrence=occurrence,class_name=r['class_name'],method=r['method'],test_name=safe_label(r['test_name']),outcome=r['outcome'],classification='ADDED45' if before is None else 'RETAINED_IDENTICAL_OUTCOME' if before['outcome']==r['outcome'] else 'CHANGED_OUTCOME')
        tests.append(row)
        if r['outcome']!='Passed':
            trace=traces[r['test_name']].pop(0);a=oldnon.get(key);mh=sha(r['message'].encode());th=sha(trace.encode())
            kind='NEW_NONPASS' if a is None else 'RETAINED_CHANGED_MESSAGE' if mh!=a['message_sha256'] else 'RETAINED_CHANGED_TRACE' if th!=a['traceback_sha256'] else 'RETAINED_IDENTICAL'
            non.append(dict(row,classification=kind,message_sha256=mh,traceback_sha256=th,previous=None if a is None else {k:a[k] for k in ['message_sha256','traceback_sha256']},raw_TRX=part['TRX'],disposition='UNWAIVED_INHERITED' if kind=='RETAINED_IDENTICAL' else 'REVIEW_REQUIRED'))
            raw.append(dict(identity=row,message=r['message'],traceback=trace))
added=[r for r in tests if r['classification']=='ADDED45'];missing=[previous[k] for k in previous.keys()-actual] if config=='Debug' else []
assert all(any(t in r['class_name'] for t in ['WindowsHarness45Tests','WindowsHarness45FormTests']) and r['outcome']=='Passed' for r in added)
assert not missing and not any(r['classification']=='CHANGED_OUTCOME' for r in tests)
save(PRIVATE/(config+'-nonpass-diagnostics-v1.json'),dict(nonpasses=raw))
save('build-and-tests/'+config+'-identities-v1.json',dict(summary=dict(configuration=config,scope=execution['mode'],identities=len(tests),outcomes=dict(collections.Counter(r['outcome'] for r in tests)),added=len(added),missing=missing,nonpass_deltas=dict(collections.Counter(r['classification'] for r in non))),tests=tests,nonpasses=non,added=added,baseline43=pin(ROOT/'evidence/04A44/build-and-tests/Debug-identities-v1.json'),raw_diagnostics=pin(PRIVATE/(config+'-nonpass-diagnostics-v1.json')),full_Release='NOT_RUN',release_scope_note='Affected subset differs from accepted40 targeted subset; aggregate nonpass counts are not a resolution claim.'))
assert not any(r['classification'] in ['NEW_NONPASS','RETAINED_CHANGED_MESSAGE'] for r in non)
print(config,len(tests),'identities;',dict(collections.Counter(r['outcome'] for r in tests)),';nonpasses',dict(collections.Counter(r['classification'] for r in non)),flush=True)
