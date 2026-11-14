"""Occurrence-level managed census and raw-message/trace crosswalk."""
from common import *
from collections import Counter,defaultdict
import xml.etree.ElementTree as ET
sys.path.append(str(ROOT/'evidence/04A38'))
from baseline_report import index,clean
from validation_report import safe_label

config=sys.argv[1]
execution=load(HERE/('build-and-tests/'+config+'-executed-v1.json'))
prior=load(ACCEPTED/'managed-Debug-identities-v1.json')
before={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in prior['tests']}
oldnon={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in prior['nonpasses']}
actual={};nonpasses=[];tests=[];counts=Counter()
NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
for part in execution['parts']:
    assert pin(ROOT/part['cases']['path'])==part['cases'] and pin(ROOT/part['TRX']['path'])==part['TRX']
    d=load(ROOT/part['cases']['path']);traces=defaultdict(list)
    for _,node in ET.iterparse(ROOT/part['TRX']['path'],events=['end']):
        if node.tag==NS+'UnitTestResult':
            if node.attrib['outcome']!='Passed':traces[node.attrib['testName']].append(node.findtext('.//'+NS+'StackTrace',''))
            node.clear()
        elif node.tag==NS+'UnitTest':node.clear()
    counts.update(d['outcomes'])
    for (digest,occurrence),r in index(d['records']).items():
        key=(part['suite'],digest,occurrence)
        assert key not in actual,'DUPLICATED_IDENTITY'
        row=dict(configuration=config,suite=part['suite'],part=part['part'],identity_sha256=digest,occurrence=occurrence,
                 class_name=r['class_name'],method=r['method'],test_name=safe_label(r['test_name']),outcome=r['outcome'])
        actual[key]=row;tests.append(row)
        if r['outcome']!='Passed':
            trace=traces[r['test_name']].pop(0)
            old=oldnon.get(key)
            message_hash=sha(r['message'].encode());trace_hash=sha(trace.encode())
            if not old: classification='NEW_NONPASS'
            elif old['message_sha256']!=message_hash:classification='RETAINED_CHANGED_MESSAGE'
            elif old['traceback_sha256']!=trace_hash:classification='RETAINED_CHANGED_TRACE'
            else:classification='RETAINED_IDENTICAL'
            nonpasses.append(dict(row,classification=classification,message=clean(r['message']),message_sha256=message_hash,
                traceback=clean(trace),traceback_sha256=trace_hash,raw_TRX=part['TRX'],previous=old))
missing=[before[k] for k in before.keys()-actual.keys()]
added=[actual[k] for k in actual.keys()-before.keys()]
changes=[dict(before=before[k],after=actual[k]) for k in before.keys()&actual.keys() if before[k]['outcome']!=actual[k]['outcome']]
summary=dict(configuration=config,discovered_execution_identity_multiset=len(tests),executed=len(tests),outcomes=dict(counts),
    suite_count=len({r['suite'] for r in tests}),partition_count=len(execution['parts']),
    missing=missing,added=added,outcome_changes=changes,nonpass_categories=dict(Counter(r['classification'] for r in nonpasses)),
    source_freeze=pin(ACCEPTED/'production-test-freeze-v1.json'),
    baseline_policy='W2 Debug is the entry comparison. Release outcomes and identity multiset are freshly observed and separately reported.',
    skipped=sum(v for k,v in counts.items() if k not in ['Passed','Failed']),
    discovery_limitation='Execution TRX definitions and occurrences reconciled; no separate discovery-only test-host launch was scheduled.')
save('build-and-tests/'+config+'-identities-v1.json',dict(summary=summary,tests=tests,nonpasses=nonpasses))
print(config,json.dumps(summary),flush=True)
assert not missing and not added and not changes,'UNEXPECTED_MANAGED_IDENTITY_OR_OUTCOME_CHANGE'
assert all(r['classification'] in ['RETAINED_IDENTICAL','RETAINED_CHANGED_TRACE'] for r in nonpasses),'UNEXPECTED_MANAGED_MESSAGE_CHANGE'
assert summary['suite_count']==6 and not summary['skipped']
