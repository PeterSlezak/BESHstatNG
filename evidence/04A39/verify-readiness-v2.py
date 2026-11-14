"""Read-only software/readiness gate. Recompute from identities and machine-bound evidence."""
from pathlib import Path
from collections import Counter
import hashlib
import importlib.util
import json
import subprocess
import sys

MAIN='22da2214da6bb1cad4f2934d0b36276324a1432f'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN')
def sha(b):return hashlib.sha256(b).hexdigest()
_CONTENT_CACHE={}


def verify(root, overrides=None, live=True, compiled=True):
    root=Path(root);here=root/'evidence/04A39';w3=root/'evidence/04A38/completion/w3';overrides=overrides or {}
    def read(path):
        path=Path(path);key=path.relative_to(root).as_posix() if path.is_absolute() else path.as_posix()
        if key in overrides:return overrides[key]
        raw=(root/key).read_bytes();cache_key=(str((root/key).resolve()),sha(raw))
        if cache_key not in _CONTENT_CACHE:_CONTENT_CACHE[cache_key]=json.loads(raw)
        return _CONTENT_CACHE[cache_key]
    def pincheck(row):
        b=(root/row['path']).read_bytes();assert len(b)==row['bytes'] and sha(b)==row['sha256'], 'CHANGED_PIN '+row['path']
    entry=read(here/'entry/merge-preflight-v1.json');assert entry['status']=='04A38_OWNER_ACCEPTED_AND_MERGED_VERIFIED' and entry['main']==MAIN
    assert entry['gates']==GATES
    reconciliation=read(here/'entry/line-ending-reconciliation-v1.json')
    assert reconciliation['semantic_differences']==0 and reconciliation['original_hashes_modified'] is False
    assert len(reconciliation['rows'])==1350
    source=read(here/'entry/source-preimages-v1.json')['files']
    assert len(source)==1350 and source==[x['current'] for x in reconciliation['rows']], 'SOURCE_PREIMAGE_UNIVERSE_CHANGED'
    if live:
        for row in source:
            if row['path'].startswith(('BESHstatNG/','BESHStat.Core/','BESHStat.Excel.Common/')):pincheck(row)
    mapping=read(here/'map/method-policy-map-v1.json')
    syntax=read(here/'map/current-source-syntax-v1.json')
    focus=[x for x in syntax['members'] if '/UDFdataImport/' in x['path'] or x['path'].endswith('/ExcelArgModules.vb')]
    assert len(focus)==192 and {x['symbol_id'] for x in focus}=={x['identity'] for x in mapping['methods']}, 'INCOMPLETE_METHOD_OWNERSHIP'
    for member in mapping['methods']:
        original=next(x for x in focus if x['symbol_id']==member['identity'])
        assert member['source_sha256']==original['source_sha256'] and member['body_sha256']==original['body_sha256']
        assert member['all_calls']==original['calls'] and member['assignments']==original['assignments'] and member['catches']==original['catches']
        assert member['production_change'] is False
    spec=importlib.util.spec_from_file_location('register39_independent',here/'verify-register-v5.py')
    register_reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(register_reader)
    register=read(here/'register/defects-v7.json')
    register_summary=register_reader.verify(root,register,check_files=live)
    managed={};identity=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'])
    for config in ['Debug','Release']:
        d=read(here/('build-and-tests/'+config+'-identities-v1.json'))
        rows=d['tests'];assert len({identity(x) for x in rows})==len(rows), 'DUPLICATE_MANAGED_IDENTITY'
        old=read(w3/('build-and-tests/'+config+'-identities-v1.json'));before={identity(x):x for x in old['tests']}
        after={identity(x):x for x in rows}
        if config=='Debug':assert before.keys() <= after.keys(), 'MISSING_MANAGED_IDENTITY'
        added=[x for k,x in after.items() if k not in before]
        assert len(added)==33 and all(x['outcome']=='Passed' and ('UdfInputPolicy39Tests' in x['class_name'] or 'UdfInputBoundary39Tests' in x['class_name']) for x in added)
        assert all(after[k]['outcome']==before[k]['outcome'] for k in before.keys()&after.keys()), 'CHANGED_MANAGED_OUTCOME'
        prior_non={identity(x):x for x in old['nonpasses']}
        for row in d['nonpasses']:
            assert identity(row) in prior_non and row['message_sha256']==prior_non[identity(row)]['message_sha256'], 'NEW_OR_CHANGED_MANAGED_MESSAGE'
            assert row['classification'] in ['RETAINED_IDENTICAL','RETAINED_CHANGED_TRACE']
        outcomes=dict(Counter(x['outcome'] for x in rows))
        if config=='Debug':assert outcomes==dict(Passed=21113,Failed=14) and len(rows)==21127
        else:assert outcomes==dict(Passed=336) and len(rows)==336
        execution=read(here/('build-and-tests/'+config+'-'+('full' if config=='Debug' else 'focused')+'-executed-v4.json'))
        assert sum(sum(p['outcomes'].values()) for p in execution['parts'])==len(rows)
        if live:
            for part in execution['parts']:
                pincheck(part['cases']);pincheck(part['TRX'])
                raw=read(root/part['cases']['path'])
                groups={}
                for row in raw['records']:
                    digest=sha(json.dumps((row['class_name'],row['method'],row['test_name'])).encode())
                    groups.setdefault(digest,[]).append(row)
                slots={(digest,i):row for digest,values in groups.items() for i,row in enumerate(sorted(values,key=lambda r:r['outcome']))}
                selected=[x for x in rows if x['suite']==part['suite'] and x['part']==part['part']]
                assert {(x['identity_sha256'],x['occurrence']):x['outcome'] for x in selected}=={k:v['outcome'] for k,v in slots.items()}, 'RAW_MANAGED_IDENTITY_MISMATCH'
                assert raw['sha256']==part['TRX']['sha256']
        build=read(here/('build-and-tests/'+config+'-build-v4.json'))
        assert len(build['exit_codes'])==9 and not any(build['exit_codes']) and build['source_unchanged'], 'BUILD_ABSENT_OR_FAILED'
        managed[config]=dict(executed=len(rows),outcomes=outcomes,new=33,scope='FULL' if config=='Debug' else 'TARGETED_ONLY')
        abi=read(here/('abi-cil/'+config+'-comparison-'+('v3' if config=='Debug' else 'v2')+'.json'))
        assert abi['exact_exports'] and abi['exports_count']==268 and not abi['normalized_method_changes'], 'ABI_OR_EXPORT_DELTA'
        if compiled:
            current=read(here/('abi-cil/'+config+'/current-v1.json'));previous=read(w3/('abi-cil/review-metadata/'+config+'/current.json'))
            clean=lambda x:{k:v for k,v in x.items() if k!='raw_IL_sha256'}
            assert {x['method']:clean(x) for x in current['methods']}=={x['method']:clean(x) for x in previous['methods']}, 'COMPILED_METHOD_DELTA'
            types_a={x['type']:x for x in current['types']};types_b={x['type']:x for x in previous['types']}
            assert types_a.keys()==types_b.keys(), 'COMPILED_TYPE_DELTA'
            for name in types_a:
                a=types_a[name];b=types_b[name]
                if a==b:continue
                assert config=='Debug' and '_Closure$__' in name and a['assembly']=='BESHStat.Core'
                assert all(x['is_static'] for x in a['fields']+b['fields'])
                assert dict(a,fields=sorted(a['fields'],key=lambda x:x['name']))==dict(b,fields=sorted(b['fields'],key=lambda x:x['name'])), 'TYPE_FIELD_CONTRACT_DELTA'
            exports=read(here/('abi-cil/'+config+'/exports-v1.json'))['exports']
            old_exports=read(w3/('abi-cil/review-metadata/'+config+'/exports.json'))['exports']
            assert len(exports)==268 and sorted(exports,key=lambda x:x['effective_excel_name'])==sorted(old_exports,key=lambda x:x['effective_excel_name'])
        fixtures=read(here/('fixtures/after-'+config+'-v1.json'));baseline=read(here/'fixtures/before-v2.json')
        assert fixtures==baseline and len(fixtures['rows'])==len({x['id'] for x in fixtures['rows']})==77, 'FIXTURE_OR_PRECISION_BEHAVIOR_DELTA'
        retained=read(here/('compatibility/'+config+'-retained-v1.json'))
        assert retained['current_queries_04A39']==retained['original_queries']==0, 'REFERENCE_OR_EXECUTION_RELABEL'
        assert (retained['PRE_EDIT_FROZEN'],retained['LATE_ARCHIVED_ENTRY_SUPPLEMENT'])==(28966,458)
        cases=retained['case_identities'];assert len(cases)==len({(x['corpus'],x['case_id']) for x in cases})==29424, 'FROZEN_CASE_MISSING_DUPLICATE'
        previous_cases=read(w3/('compatibility/'+config+'-case-identities-v2.json'))['entries']
        assert cases==previous_cases, 'FROZEN_INPUT_OR_ORDER_CHANGED'
        if live:
            for result in retained['results']:
                for field in ['original','current','inputs']:pincheck(result[field])
                assert (root/result['original']['path']).read_bytes()==(root/result['current']['path']).read_bytes()
    python=read(here/'python-node-scanners/python-identities-v1.json');parents=python['parents'];events=python['events']
    old_python=read(w3/'python-node-scanners/python-identities-v1.json')
    assert Counter(x['id'] for x in parents)==Counter(x['id'] for x in old_python['parents']), 'PYTHON_PARENT_LOST'
    assert len(parents)==len({x['id'] for x in parents})==956
    assert Counter(x['outcome'] for x in old_python['parents'])==Counter(PASS=830,FAIL=113,ERROR=13), 'ORIGINAL_PYTHON_BASELINE_REPLACED'
    assert Counter(x['outcome'] for x in parents)==Counter(PASS=833,FAIL=112,ERROR=11), 'UNACCOUNTED_CURRENT_PYTHON_OUTCOME'
    assert Counter(x['outcome'] for x in events)==Counter(FAIL=184,ERROR=11) and len(events)==195
    assert Counter(x['outcome'] for x in old_python['events'])==Counter(FAIL=185,ERROR=13) and len(old_python['events'])==198
    delta=read(here/'python-node-scanners/python-delta-v2.json')
    before={x['id']:x['outcome'] for x in old_python['parents']}
    changes={(x['id'],before[x['id']],x['outcome']) for x in parents if before[x['id']]!=x['outcome']}
    assert changes=={(x['id'],x['before'],x['after']) for x in delta['changes']} and len(changes)==5, 'CHANGED_PYTHON_PARENT_HIDDEN'
    assert delta['historical_privacy_errors_waived'] is False, 'PRIVACY_ERROR_WAIVER'
    assert not delta['missing_parent_identities']
    historical_privacy=[x for x in old_python['events'] if x['outcome']=='ERROR' and 'UnicodeDecodeError' in x.get('traceback','')]
    assert len(historical_privacy)==2 and delta['historical_privacy_UnicodeDecodeErrors']==historical_privacy, 'HISTORICAL_PRIVACY_ERRORS_LOST'
    assert Counter(x['classification'] for x in events)==Counter(RETAINED_IDENTICAL=139,RETAINED_CHANGED_MESSAGE=54,NEW_NONPASS=2)
    assert delta['diagnostic_drift_count']==len(delta['diagnostic_drifts'])==54
    assert len(delta['historical_events_not_reproduced_in_current_scope'])==5
    assert delta['findings_per_current_privacy_parent']==1599
    assert delta['status']=='BLOCKING_CHANGED_PRIVACY_AND_DIAGNOSTICS_NOT_WAIVED'
    if live:
        raw_parents=[];raw_events=[]
        for part in python['parts']:
            pincheck(part['raw']);raw=read(root/part['raw']['path']);raw_parents+=raw['records']
            for parent in raw['records']:
                raw_events += [(parent['id'],e['outcome'],sha(e.get('message','').encode()),e['traceback_sha256']) for e in parent['events']]
        assert Counter((x['id'],x['outcome']) for x in raw_parents)==Counter((x['id'],x['outcome']) for x in parents), 'RAW_PYTHON_PARENT_MISMATCH'
        assert Counter(raw_events)==Counter((x['identity'],x['outcome'],x['message_sha256'],x['traceback_sha256']) for x in events), 'RAW_PYTHON_EVENT_MISMATCH'
    post=read(here/'python-node-scanners/input-postcheck-v1.json');assert not post['changed'] and post['campaigns']==1 and post['command']['exit_code']==0
    node=read(here/'python-node-scanners/node-v1.json');oldnode=read(w3/'python-node-scanners/node-v1.json')
    assert node['passed']==916 and len(node['suites'])==3
    for row,oldrow in zip(node['suites'],oldnode['suites']):
        assert row['suite']==oldrow['suite'] and Counter(row['identities'])==Counter(oldrow['identities'])
        assert row['passed']==row['tests']==len(row['identities']) and row['failed']==0 and row['command']['exit_code']==0
    serial=read(here/'python-node-scanners/serial-v1.json')
    assert [x['result']['exit_code'] for x in serial['results']]==[1,1,0], 'SCANNER_EXIT_DRIFT'
    assert all(x['disposition']=='INHERITED_RESTRICTED;EXPLICIT_OWNER_DECISION_REQUIRED' for x in serial['results'][:2])
    if live:
        for row in read(here/'python-node-scanners/input-freeze-v1.json')['files']:pincheck(row)
        commands=['git','-c','safe.directory='+root.as_posix(),'--no-optional-locks']
        call=lambda *a:subprocess.check_output(commands+list(a),cwd=root).decode().strip()
        assert call('branch','--show-current')=='codex/04A-39' and call('rev-parse','HEAD')==call('rev-parse','main')==MAIN, 'GIT_ANCHOR_CHANGED'
        assert not call('diff','--cached','--name-only') and not call('diff','--check')
        pincheck(entry['index'])
    return dict(verdict='NOT_READY_04A39',blockers=['A39-RAW-REVIEW-PRIVACY-01:TWO_CHANGED_PRIVACY_NONPASSES:1599_FINDINGS_EACH','PYTHON_54_CHANGED_DIAGNOSTICS_UNWAIVED'],managed=managed,exports=dict(Debug=268,Release=268),current_fixtures_per_configuration=77,
        retained_compatibility_per_configuration=29424,compatibility_execution='RETAINED_W3;NOT_FRESH39',
        Python=dict(Counter(x['outcome'] for x in parents)),Python_events=dict(Counter(x['outcome'] for x in events)),
        historical_Python=dict(PASS=830,FAIL=113,ERROR=13),historical_Python_events=dict(FAIL=185,ERROR=13),historical_privacy_errors_unwaived=2,Python_parent_outcome_changes=5,Python_changed_messages=54,Node=916,strict_profile_MkDocs=[1,1,0],register=register_summary,gates=GATES,
        scientific_native='NOT_RUN_UNAUTHORIZED',review='No automatic acceptance, merge or04A.40 start.')


if __name__=='__main__':
    root=Path(sys.argv[1]) if len(sys.argv)>1 else Path(__file__).resolve().parents[2]
    result=verify(root);print(json.dumps(result,sort_keys=True));raise SystemExit(2 if result['verdict']=='NOT_READY_04A39' else 0)
