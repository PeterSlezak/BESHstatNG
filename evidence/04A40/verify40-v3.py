"""Fail-closed40 machine-evidence reader. Nonpasses are never converted to green."""
from common40_v3 import *
import collections,copy

def documents():
    return dict(git=load(HERE/'final/git-final-v1.json'),source=load(HERE/'matrix/source-freeze-v2.json'),
      Debug=load(HERE/'build-and-tests/Debug-identities-v2.json'),Release=load(HERE/'build-and-tests/Release-identities-v2.json'),
      compatibility={c:load(HERE/('compatibility/'+c+'-current-only-v4.json')) for c in ['Debug','Release']},
      ABI={c:load(HERE/('abi-cil/'+c+'-proof-v2.json')) for c in ['Debug','Release']},
      Python=load(HERE/'python/parent-event-reconciliation-v2.json'),diagnostics=load(HERE/'python/diagnostic-dispositions-v3.json'),
      Node=load(HERE/'python/Node-fresh-identities-v3.json'),preservation=load(HERE/'preservation/historical-readback-v4.json'),
      privacy=load(HERE/'python/repository-privacy-witness-v1.json'),privacy_controls=load(HERE/'publication/privacy-boundary-controls-v1.json'),
      register=load(HERE/'register/issue-delta-v4.json'))
def verify(d,live=False):
    g=d['git'];assert g['branch']=='codex/04A-40' and g['HEAD']==g['main']==g['merge_base']==MAIN and g['staged_empty'] and g['diff_check_exit']==0
    assert g['index']==load(HERE/'entry/authentication-v3.json')['index'],'INDEX_ANCHOR_CHANGED'
    assert len(d['source']['source_delta'])==16 and len(d['source']['new_tests'])==2
    assert d['source']['source_pins']==load(HERE/'matrix/source-freeze-v2.json')['source_pins'],'FROZEN_SOURCE_MANIFEST_CHANGED'
    for config,counts in [('Debug',dict(Passed=21125,Failed=14)),('Release',dict(Passed=3108,Failed=2))]:
        t=d[config];assert t['summary']['outcomes']==counts and len(t['tests'])==sum(counts.values()) and not t['summary']['missing']
        assert len({(r['suite'],r['identity_sha256'],r['occurrence']) for r in t['tests']})==len(t['tests'])
        assert collections.Counter(r['outcome'] for r in t['tests'])==counts
        assert all(r['classification'] not in ['NEW_NONPASS','RETAINED_CHANGED_MESSAGE'] for r in t['nonpasses'])
        if config=='Debug':assert all(r['classification']=='RETAINED_IDENTICAL' for r in t['nonpasses'])
        a=d['ABI'][config];assert a['exports']==268 and a['exact_export_order_names_signatures_attributes'] and not a['unexpected']
        c=d['compatibility'][config];rows=c['case_identities'];assert len(rows)==29424 and len({(r['corpus'],r['case_id']) for r in rows})==29424
        assert collections.Counter(r['provenance'] for r in rows)==dict(PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458)
        old=load(ROOT/('evidence/04A39/compatibility/'+config+'-retained-v1.json'));assert rows==old['case_identities']
        assert len(c['results'])==12 and c['original_queries']==0
        before={r['corpus']:r for r in old['results']}
        for r in c['results']:assert r['exact'] and r['current']['sha256']==r['original']['sha256'] and r['original']==before[r['corpus']]['original']
        assert load(HERE/('fixtures/'+config+'-39-retained-inputs-fresh-results-v2.json'))['exact']
        assert load(HERE/('fixtures/'+config+'-48-final-v2.json'))['statistical_input_exception_contracts_exact']
    p=d['Python'];assert len(p['parents'])==len({r['id'] for r in p['parents']})==956 and not p['summary']['dropped_parents'] and not p['summary']['duplicated_parents']
    assert collections.Counter(r['outcome'] for r in p['parents'])==p['summary']['parent_outcomes']
    assert d['Node']['passed']==sum(r['passed'] for r in d['Node']['suites'])==916 and all(r['exact_historical_identity_multiset'] for r in d['Node']['suites'])
    pr=d['preservation'];assert pr['original_NOT_READY_files']==468 and pr['original_bytes_preserved'] and pr['source_entry_identities']==1353 and pr['source_unchanged']==1337
    assert len(pr['owner_line_ending_transitions'])==4 and pr['protected_unique_paths']==8984
    assert (pr['register']['canonical_entries'],pr['register']['original_obligations'],pr['register']['restricted_controls'],pr['crosswalk_rows'])==(1169,1514,802,2967)
    assert d['privacy_controls']['raw_unchanged'] and all(r['raw_findings']==6 and r['sanitized_findings']==0 for r in d['privacy_controls']['results'])
    assert d['register']['original_obligations']==1514 and d['register']['restricted_controls']==802 and d['register']['exit_dispositions_reserved_for_04A48']
    if live:
        lock()
        for path,h in d['source']['source_pins'].items():assert pin(path)['sha256']==h,'SOURCE_LOCK_CHANGED'
        for config in ['Debug','Release']:
            proof=d['ABI'][config]
            for item in proof['assembly_hashes']+[proof['current_metadata'],proof['current_exports']]:assert pin(item['path'])==item,'SHIPPING_OR_METADATA_BYTES_CHANGED'
            current=load(PRIVATE/('abi-cil-final2/'+config+'/exports.json'))['exports'];old=load(ROOT/('evidence/04A39/abi-cil/'+config+'/exports-v1.json'))['exports'];assert current==old and len(current)==268
            for r in d['compatibility'][config]['results']:
                assert pin(r['original']['path'])==r['original'] and pin(r['current']['path'])==r['current'] and (ROOT/r['current']['path']).read_bytes()==(ROOT/r['original']['path']).read_bytes()
    blockers=[dict(id='04A40-PRIVACY-ENTRY-RAW-01',reason='Untracked raw entry receipt still has six owner-path rule occurrences. Exact original retained; sanitized derivative is not a repository scanner waiver.')]
    if d['Python']['summary']['parent_changes']:blockers.append(dict(id='04A40-HISTORICAL-SCOPE-GUARDS-01',reason='Historical three diff-scope guards reexpose FAIL under authorized UDF edits; owner applicability disposition remains unwaived.'))
    if d['diagnostics']['unresolved']:blockers.append(dict(id='04A40-DIAGNOSTIC-ARGUMENT-REVIEW-01',reason='Some inherited assertion argument drift remains explicitly owner-review-required; raw pairs and fingerprints retained.'))
    return dict(status='NOT_READY_04A40',software=dict(compilation='VERIFIED_PASS',managed='VERIFIED_NONPASS_INHERITED',exports='VERIFIED_PASS',compatibility='VERIFIED_PASS',Python='NONPASS_UNWAIVED',Node='VERIFIED_PASS'),
      blocking=blockers,scientific_native='NOT_RUN_UNAUTHORIZED;NOT_GOLDEN',publication='EXTERNAL_PHYSICAL_ORIGINAL_SCANNER_RECEIPT_REQUIRED',gates=GATES)
def adverse(d):
    specs=[('changed-source-pin',lambda x:x['source']['source_pins'].__setitem__(next(iter(x['source']['source_pins'])),'0'*64)),('branch',lambda x:x['git'].__setitem__('branch','main')),('staged',lambda x:x['git'].__setitem__('staged_empty',False)),
      ('missing-test',lambda x:x['Debug']['tests'].pop()),('changed-test-outcome',lambda x:x['Debug']['tests'][0].__setitem__('outcome','Failed')),
      ('missing-case',lambda x:x['compatibility']['Release']['case_identities'].pop()),('duplicate-case',lambda x:x['compatibility']['Debug']['case_identities'].__setitem__(0,x['compatibility']['Debug']['case_identities'][1])),
      ('baseline-replacement',lambda x:x['compatibility']['Debug']['results'][0]['original'].__setitem__('path','replacement.tsv')),('export-change',lambda x:x['ABI']['Release'].__setitem__('exports',267)),
      ('lost-parent',lambda x:x['Python']['parents'].pop()),('lost-original',lambda x:x['preservation'].__setitem__('original_bytes_preserved',False)),('lost-control',lambda x:x['register'].__setitem__('restricted_controls',801)),
      ('privacy-greenwash',lambda x:x['privacy_controls']['results'][0].__setitem__('raw_findings',0))]
    results=[]
    for name,mutate in specs:
        altered=copy.deepcopy(d);mutate(altered)
        try:verify(altered)
        except (AssertionError,KeyError,IndexError):results.append(dict(control=name,detected=True))
        else:raise AssertionError('MISSED_ADVERSE_CONTROL:'+name)
    return results
if __name__=='__main__':
    d=documents();result=verify(d,live=True);save('final/readiness-v2.json',result);save('final/readiness-adverse-controls-v2.json',dict(controls=adverse(d),assertions_weakened=False));print(result['status'],flush=True)
