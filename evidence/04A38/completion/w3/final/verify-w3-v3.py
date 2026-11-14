"""Fail-closed W3 software evidence reader. Never releases native/scientific gates."""
from common import *
from collections import Counter
import csv,re

BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
INDEX='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
RESTRICTIONS='NOT_READY_04A38; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED'


def exact_bytes(row,data):
    assert data is not None and len(data)==row['bytes'] and sha(data)==row['sha256'],'MISSING_OR_CHANGED_BYTES '+row['path']


def validate_git(d):
    c=d['commands']
    assert all(r['exit_code']==0 for r in c.values())
    assert c['branch']['stdout'].strip()=='codex/04A-38'
    assert all(c[k]['stdout'].strip()==BASE for k in ['HEAD','main','merge-base'])
    assert not c['staged']['stdout'] and c['diff-check']['exit_code']==0
    assert d['index']['sha256']==INDEX and d['index']['bytes']==620185


def validate_managed(doc,expected):
    tests=doc['tests']
    key=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'])
    assert Counter(map(key,tests))==Counter(map(key,expected))
    assert len(tests)==len(set(map(key,tests)))==21094
    before={key(r):r for r in expected}
    assert all(r['outcome']==before[key(r)]['outcome'] for r in tests)
    assert Counter(r['outcome'] for r in tests)==dict(Passed=21080,Failed=14)
    assert len({r['suite'] for r in tests})==6
    non=doc['nonpasses'];old=load(ACCEPTED/'managed-Debug-identities-v1.json')['nonpasses']
    messages={key(r):r['message_sha256'] for r in old}
    assert len(non)==14 and all(r['message_sha256']==messages[key(r)] for r in non)
    assert not doc['summary']['missing'] and not doc['summary']['added'] and not doc['summary']['outcome_changes']
    return dict(identities=len(tests),outcomes=dict(Counter(r['outcome'] for r in tests)),
                nonpass_categories=dict(Counter(r['classification'] for r in non)))


def validate_compat(doc,baseline):
    rows=doc['results'];config=doc['configuration']
    assert len(rows)==len({r['corpus'] for r in rows})==12
    expected={r['corpus']:r for r in baseline['results'] if r['configuration']==config}
    assert {r['corpus'] for r in rows}==set(expected)
    for r in rows:
        old=expected[r['corpus']]
        assert r['configuration']==config and r['exact'] and r['command']['exit_code']==0 and r['original_queries']==0
        for k in ['original','inputs','recorder','cases']:assert r[k]==old[k],'ORIGINAL_REFERENCE_CONTAMINATION'
        assert r['provenance']==('LATE_ARCHIVED_ENTRY_SUPPLEMENT' if r['corpus'] in list('ABCDEFG') else 'PRE_EDIT_FROZEN')
        assert r['current']['sha256']==r['original']['sha256'] and r['current']['bytes']==r['original']['bytes']
    assert sum(r['cases'] for r in rows if r['corpus'] in list('ABCDEFG'))==458
    assert sum(r['cases'] for r in rows if r['corpus'] not in list('ABCDEFG'))==28966
    return 29424


def validate_closure(d):
    expected=load(ACCEPTED/'canonical-identity-status-v1.json')['entries']
    keys=lambda rows:Counter((r['population'],r['identity']) for r in rows)
    assert keys(d['entries'])==keys(expected)
    assert all(r['supported'] and r['transition']!='OPEN' for r in d['entries'])
    assert d['fragment_counts']==dict(original=20683,current=12659,predecessor=13695)
    assert not d['missing'] and not d['extra'] and not d['OPEN']
    assert d['physical_Windows']==723 and d['moved_Common_lambdas']==3
    bound={(r['population'],r['identity']):r['proof'] for r in expected}
    assert all(r['proof']==bound[r['population'],r['identity']] for r in d['entries'])
    assert d['preserved_proofs']==load(ACCEPTED/'whole-body-dependency-fragment-reconciliation-v1.json')['proofs']
    return dict(historical=704,current=726,support=303,OPEN=0)


def validate_python(d):
    before=load(ACCEPTED/'python-effective-identities-v1.json')['entries']
    assert Counter(r['id'] for r in d['parents'])==Counter(r['id'] for r in before)
    assert len(d['parents'])==956
    old={r['id']:r['outcome'] for r in before}
    assert all(r['outcome']==old[r['id']] for r in d['parents'])
    assert Counter(r['outcome'] for r in d['parents'])==dict(PASS=830,FAIL=113,ERROR=13)
    assert Counter(r['outcome'] for r in d['events'])==dict(FAIL=185,ERROR=13)
    assert not d['summary']['missing_events'] and not d['summary']['parent_changes']
    assert all(r['classification']!='NEW_NONPASS' for r in d['events'])
    assert d['summary']['two_historical_privacy_errors_retained']==2
    return d['summary']


def validate_configuration_categories(d):
    assert set(d)=={'Debug','Release'}
    for config in d:assert set(d[config])=={'build','managed','abi-cil','compatibility'} and all(d[config].values())


def validate_exports(actual,original):
    assert len(actual)==268 and sorted(actual,key=lambda r:r['effective_excel_name'])==sorted(original,key=lambda r:r['effective_excel_name'])


def validate_node(d,baseline):
    old={r['suite']:r['identities'] for r in baseline['suites']}
    assert {r['suite'] for r in d['suites']}==set(old)
    total=0
    for row in d['suites']:
        assert Counter(row['identities'])==Counter(old[row['suite']])
        assert len(row['identities'])==row['tests']==row['passed'] and row['failed']==0 and row['command']['exit_code']==0
        for k in ['stdout','stderr']:assert pin(ROOT/row[k]['path'])==row[k]
        text=(ROOT/row['stdout']['path']).read_bytes().decode('utf-8-sig')
        names=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)\r?$',text,re.M|re.S)
        assert [sha(s.encode()) for s in names]==row['identities']
        total+=len(names)
    assert total==d['passed']==916


def validate_final_receipts(git,publication,preservation,restrictions):
    assert git is not None and publication is not None and preservation is not None
    validate_git(git)
    assert publication['status']=='ALL_LOGICAL_PAYLOAD_SHA_SIZE_CRC_AND_PHYSICAL_PART_SHA_PASS'
    assert preservation['base_reader_exit']==0 and preservation['old_private_union']==31910 and preservation['old_archives']==152
    assert not preservation['private_differences'] and not preservation['source_differences']
    assert restrictions==RESTRICTIONS


def compute(final=False):
    lock()
    assert m.git('branch','--show-current')=='codex/04A-38'
    assert all(m.git(*args)==BASE for args in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')])
    assert not m.git('diff','--cached','--name-only') and not m.git('diff','--check')
    assert pin(ROOT/'.git/index')['sha256']==INDEX
    assert pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md')['sha256']=='0ee45eae70aa35d6c87c636d82c3f33e105b8c12e6e5a4badb69e38fe7ad69dd'
    assert pin(ACCEPTED/'production-test-freeze-v1.json')['sha256']=='43cd9af03ab09829c205b694f6650df1779766692159da791144b9f48f7c868e'
    core_index=W2/'continuation-v17-postcorrection-v1-review-parts-index-v1.json'
    assert pin(core_index)['sha256']=='837f9f5e367e6d53465f1fb2f6dd14b8ee13a973deda43c51affd31130e0e358'
    immutable={r['path']:{k:r[k] for k in ['path','bytes','sha256']} for r in load(core_index)['files']}
    for p in [ACCEPTED/'whole-body-dependency-fragment-reconciliation-v1.json',W2/'continuation-v12/compatibility-packet-v1.json']:
        assert pin(p)==immutable[p.relative_to(ROOT).as_posix()]
    validate_git(load(HERE/'entry/git-v2.json'))
    entry=load(HERE/'preservation/live-private-v2.json')
    assert entry['actual_union']==31910 and entry['actual_archives']==152 and not entry['differences'] and not entry['conflicts']
    closure=validate_closure(load(HERE/'abi-cil/complete-closure-v4.json'))
    for r in load(HERE/'abi-cil/complete-closure-v4.json')['preserved_proofs']:
        assert pin(ROOT/r['path'])==r and r==immutable[r['path']]
    old=W2/'continuation-v17'
    assert pin(old/'original-fragment-parent-crosswalk-v4.json')==immutable[(old/'original-fragment-parent-crosswalk-v4.json').relative_to(ROOT).as_posix()]
    fragments=load(old/'original-fragment-parent-crosswalk-v4.json')['rows']
    current=load(old/'current-ownership-v4.json')
    assert len(fragments)==20683 and all(r['covered'] for r in fragments)
    assert len(current['fragments'])==12659 and all(not r['review_status'].startswith('OPEN') for r in current['fragments'])
    expected=load(ACCEPTED/'managed-Debug-identities-v1.json')['tests']
    managed={};compat={};exports={}
    baseline=load(W2/'continuation-v12/compatibility-packet-v1.json')
    for config in ['Debug','Release']:
        build=load(HERE/('build-and-tests/'+config+'-build-v1.json'))
        assert len(build['result']['exit_codes'])==9 and not any(build['result']['exit_codes']) and build['result']['source_unchanged']
        managed[config]=validate_managed(load(HERE/('build-and-tests/'+config+'-identities-v1.json')),expected)
        for part in load(HERE/('build-and-tests/'+config+'-executed-v1.json'))['parts']:
            for k in ['cases','TRX']:assert pin(ROOT/part[k]['path'])==part[k]
        compat[config]=validate_compat(load(HERE/('compatibility/'+config+'-comparisons-v1.json')),baseline)
        for r in load(HERE/('compatibility/'+config+'-comparisons-v1.json'))['results']:
            for k in ['original','inputs','recorder','current']:assert pin(ROOT/r[k]['path'])==r[k]
            assert (ROOT/r['current']['path']).read_bytes()==(ROOT/r['original']['path']).read_bytes()
        inspection=load(HERE/('abi-cil/'+config+'-inspection-v1.json'))
        for k in ['metadata','exports']:assert pin(ROOT/inspection[k]['path'])==inspection[k]
        actual=load(ROOT/inspection['exports']['path'])['exports']
        original=load(ROOT/inspection['sealed_original_export_baseline']['path'])['exports']
        validate_exports(actual,original)
        exports[config]=len(actual)
    py=validate_python(load(HERE/'python-node-scanners/python-identities-v1.json'))
    post=load(HERE/'python-node-scanners/python-input-postcheck-v2.json')
    assert post['command']['exit_code']==0 and post['campaigns']==1 and not post['changed']
    node=load(HERE/'python-node-scanners/node-v1.json')
    baseline_node=W2/'continuation-v17/node-identities-v1.json'
    assert pin(baseline_node)==immutable[baseline_node.relative_to(ROOT).as_posix()]
    validate_node(node,load(baseline_node))
    serial=load(HERE/'python-node-scanners/serial-v1.json')
    exits={r['category']:r['result']['exit_code'] for r in serial['results']}
    assert exits==dict(strict=1,profile=1,docs=0),'AUXILIARY_EXIT_DRIFT'
    for r in serial['results']:assert r['disposition']==('VERIFIED_PASS' if not r['result']['exit_code'] else 'INHERITED_RESTRICTED;EXPLICIT_OWNER_DECISION_REQUIRED')
    if final:
        validate_final_receipts(load(HERE/'final/git-final-v1.json'),load(HERE/'publication/physical-readback-v1.json'),
                                load(HERE/'preservation/post-publication-lock-v1.json'),RESTRICTIONS)
        preservation=load(HERE/'preservation/post-publication-lock-v1.json')
        record=preservation['new_private_pins'];assert pin(ROOT/record['path'])==record
        new=load(ROOT/record['path']);assert new['new_count']==len(new['new_pins'])
        assert new['union']==31910+len(new['new_pins'])
        assert len(new['new_pins'])==len({r['path'].casefold() for r in new['new_pins']})
        for row in new['new_pins']:
            assert pin(ROOT/row['path'])=={k:row[k] for k in ['path','bytes','sha256']}
            s=(ROOT/row['path']).stat()
            assert format(s.st_dev,'x')==row['device_hex'] and format(s.st_ino,'x')==row['file_id_hex']
    validate_configuration_categories({c:{'build':True,'managed':bool(managed[c]),'abi-cil':exports[c]==268,'compatibility':compat[c]==29424} for c in ['Debug','Release']})
    changed_messages=[r['identity'] for r in load(HERE/'python-node-scanners/python-identities-v1.json')['events'] if r['classification']=='RETAINED_CHANGED_MESSAGE']
    return dict(status='W3_BLOCKED_NEEDS_OWNER_DECISION' if changed_messages else 'W3_SOFTWARE_VERIFICATION_COMPLETED_OWNER_REVIEW_PENDING',managed=managed,compatibility=compat,exports=exports,
        closure=closure,Python=py,Node=916,strict_profile_MkDocs=exits,
        inherited_nonpasses='VERIFIED_NONPASS_INHERITED;NOT_WAIVED',
        native_science_manual='NOT_RUN_UNAUTHORIZED',owner_gates='REQUIRES_OWNER_DECISION',
        changed_nonpass_messages=changed_messages,final_receipt_required=not final,restrictions=RESTRICTIONS)


if __name__=='__main__':
    print(json.dumps(compute('--final' in sys.argv),indent=2),flush=True)
