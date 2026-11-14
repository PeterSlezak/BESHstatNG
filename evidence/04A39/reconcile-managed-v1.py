"""Occurrence-level W3→39 managed identity/message/trace comparison, including new tests."""
from common39 import *
from collections import Counter, defaultdict
import xml.etree.ElementTree as ET
sys.path.append(str(ROOT/'evidence/04A38'))
from baseline_report import index, clean
from validation_report import safe_label

source_lock();config=sys.argv[1];mode='full' if config=='Debug' else 'focused'
execution=load(HERE/('build-and-tests/'+config+'-'+mode+'-executed-v4.json'))
prior=load(W3/('build-and-tests/'+config+'-identities-v1.json'))
before={(x['suite'],x['identity_sha256'],x['occurrence']):x for x in prior['tests']}
oldnon={(x['suite'],x['identity_sha256'],x['occurrence']):x for x in prior['nonpasses']}
actual={};tests=[];nonpasses=[];NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
for part in execution['parts']:
    assert pin(part['cases']['path'])==part['cases'] and pin(part['TRX']['path'])==part['TRX']
    trace=defaultdict(list)
    for _,node in ET.iterparse(ROOT/part['TRX']['path'],events=['end']):
        if node.tag==NS+'UnitTestResult':
            if node.attrib['outcome']!='Passed':trace[node.attrib['testName']].append(node.findtext('.//'+NS+'StackTrace',''))
            node.clear()
        elif node.tag==NS+'UnitTest':node.clear()
    for (digest,occurrence),r in index(load(ROOT/part['cases']['path'])['records']).items():
        key=(part['suite'],digest,occurrence);assert key not in actual
        old=before.get(key)
        row=dict(configuration=config,suite=part['suite'],part=part['part'],identity_sha256=digest,occurrence=occurrence,
            class_name=r['class_name'],method=r['method'],test_name=safe_label(r['test_name']),outcome=r['outcome'],
            classification='ADDED_04A39' if old is None else 'RETAINED_IDENTICAL' if old['outcome']==r['outcome'] else 'CHANGED_OUTCOME')
        actual[key]=row;tests.append(row)
        if r['outcome']!='Passed':
            rawtrace=trace[r['test_name']].pop(0);previous=oldnon.get(key)
            mh=sha(r['message'].encode());th=sha(rawtrace.encode())
            delta='NEW_NONPASS' if previous is None else 'RETAINED_CHANGED_MESSAGE' if mh!=previous['message_sha256'] else 'RETAINED_CHANGED_TRACE' if th!=previous['traceback_sha256'] else 'RETAINED_IDENTICAL'
            nonpasses.append(dict(row,classification=delta,message=clean(r['message']),message_sha256=mh,traceback=clean(rawtrace),traceback_sha256=th,raw_TRX=part['TRX'],previous=previous))
missing=[before[k] for k in before.keys()-actual.keys()] if config=='Debug' else []
added=[r for k,r in actual.items() if k not in before]
assert all('UdfInputPolicy39Tests' in r['class_name'] or 'UdfInputBoundary39Tests' in r['class_name'] for r in added)
assert all(r['outcome']=='Passed' for r in added)
summary=dict(configuration=config,scope=mode,executed=len(tests),outcomes=dict(Counter(r['outcome'] for r in tests)),
    retained=len(tests)-len(added),added=len(added),missing=missing,
    outcome_changes=[r for r in tests if r['classification']=='CHANGED_OUTCOME'],
    nonpass_categories=dict(Counter(r['classification'] for r in nonpasses)),
    inherited_not_selected=21094-(len(tests)-len(added)) if config=='Release' else 0,
    full_Release='NOT_RUN_NOT_REQUIRED; targeted policies and77 fixtures plus exact per-config shipping metadata',
    skipped=sum(v for k,v in execution['outcomes'].items() if k not in ['Passed','Failed']))
save('build-and-tests/'+config+'-identities-v1.json',dict(summary=summary,tests=tests,nonpasses=nonpasses,added=added,
    W3_baseline=pin(W3/('build-and-tests/'+config+'-identities-v1.json'))))
assert not missing and not summary['outcome_changes'] and not summary['skipped']
assert all(r['classification'] in ['RETAINED_IDENTICAL','RETAINED_CHANGED_TRACE'] for r in nonpasses)
if config=='Debug':assert len(tests)==21127 and summary['outcomes']==dict(Passed=21113,Failed=14) and len(added)==33
print(json.dumps(summary),flush=True)
