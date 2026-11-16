"""Standalone read-only post-recovery delivery reader; no extraction or host calls."""
import collections,hashlib,io,json,re,sys,types,unicodedata,zipfile,zlib
from pathlib import Path
P='evidence/04A47/recovery-v2/'
STATUS='READY_FOR_OWNER_04A47_POSTRECOVERY_SAFE_SOFTWARE_REVIEW__NATIVE_GATE_NOT_AUTHORIZED'
PAYLOAD='evidence/04A23/multivariate-spc-same-core-comparisons-v1.json'
SHA='7f20addb9177a48e95b81330aa55acccde4bb6ed8b808cdbad42e09f2b790643'
MAIN='8a8d4ee21f41f1a53072308d4a01faa0b20fdd5a'
def sha(b):return hashlib.sha256(b).hexdigest()
def packed(files):
    b=io.BytesIO()
    with zipfile.ZipFile(b,'w',zipfile.ZIP_STORED) as z:
        for n,v in files.items():z.writestr(n,v)
    b.seek(0);return b
def legacy(files):
    receipt=json.loads(files[P+'sealed-v1-external-receipt.json'])
    assert sha(files[P+'sealed-v1-external-receipt.json'])=='b328b8d298afed783b30cbe14ec98e021252c4a799f2171d2b8f9c92b615539d'
    old={}
    for r in receipt['physical_members']:
        data=files[P+'sealed-v1-index.json'] if r['path']=='.review-index.json' else files[r['path']]
        assert len(data)==r['bytes'] and sha(data)==r['sha256'] and f'{zlib.crc32(data)&0xffffffff:08x}'==r['crc32']
        old[r['path']]=data
    code=old['evidence/04A47/review_reader.py'];m=types.ModuleType('authenticated_legacy47_reader');exec(compile(code,'sealed47-reader.py','exec'),vars(m))
    return m,packed(old)
def verify(archive,receipt=None):
    with zipfile.ZipFile(archive) as z:
        names=z.namelist();assert len(names)==len(set(names))==len({unicodedata.normalize('NFKC',n).casefold() for n in names})
        assert sum(i.file_size for i in z.infolist())<150_000_000
        for i in z.infolist():
            n=i.filename;assert n and not n.startswith('/') and chr(92) not in n and ':' not in n
            assert all(c not in ('','.','..') and not c.endswith((' ','.')) for c in n.split('/'))
            assert not re.search(r'(?i)(?:^|/)(?:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\.|/|$)',n)
            assert not i.is_dir() and ((i.external_attr>>16)&0o170000)!=0o120000
            assert n.rsplit('/',1)[-1]!=PAYLOAD.rsplit('/',1)[-1],'RAW_PAYLOAD_PUBLICATION_FORBIDDEN'
        files={i.filename:z.read(i) for i in z.infolist()}
    index=json.loads(files['.review-index.json'])['files'];assert len(index)==len({r['path'] for r in index})
    assert {r['path'] for r in index}==set(files)-{'.review-index.json'}
    def pin(r):
        b=files[r['path']];assert len(b)==r['bytes'] and sha(b)==r['sha256']
        if 'crc32' in r:assert f'{zlib.crc32(b)&0xffffffff:08x}'==r['crc32']
    for r in index:pin(r)
    old,oldzip=legacy(files);old.verify(oldzip)
    read=lambda n:json.loads(files[P+n])
    auth=read('payload-authentication-v1.json');assert auth['payload']==dict(path=PAYLOAD,bytes=57758738,sha256=SHA) and auth['expected']==dict(bytes=57758738,sha256=SHA)
    assert auth['source']=='OWNER_RECOVERED_ALREADY_PRESENT_AT_EXPECTED_LOCATION' and auth['regular_nonreparse'] and auth['pre_post_read_exact'] and auth['historical_bytes_written']==0
    assert auth['structure']['records']==3196 and len(auth['structure']['positive_chart_families'])==6 and auth['structure']['payload_keys_exact'] and auth['structure']['rejections_retained']
    assert auth['structure']['status']=='NOT_GOLDEN' and not auth['structure']['promoted'] and not auth['tracked'] and not auth['ignored']
    exception=auth['ignore_policy_exception'];assert exception['owner_statement']=='.gitignore was modified on purpose and is approved. Please continue.' and exception['full_original_candidate_scans_must_include_payload'] and exception['public_zip_must_exclude_payload']
    assert auth['manifest']['bytes']==344343 and auth['manifest']['sha256']=='2d0bbea874e22b2d07c919d930e20377dcd31db14267a0df724a64ce760c241e';pin(auth['manifest']);pin(auth['vb_authority'])
    assert SHA in files[auth['vb_authority']['path']].decode('utf-8-sig')
    manifest=json.loads(files[auth['manifest']['path']])['files'][PAYLOAD];assert all(manifest[k]==auth['payload'][k] for k in ['bytes','sha256'])
    assert auth['v1_archive']['bytes']==6988940 and auth['v1_archive']['sha256']=='0fe14fe9558fadeb4040d2ffc5f28622c920add0bf418aaf9fb7ff7423d05bc8'
    assert auth['v1_receipt']['bytes']==141137 and auth['v1_receipt']['sha256']=='b328b8d298afed783b30cbe14ec98e021252c4a799f2171d2b8f9c92b615539d'
    assert auth['git']['HEAD']==auth['git']['main']==auth['git']['merge_base']==MAIN and auth['git']['staged_empty']
    for p in auth['source_tests']:pin(p)
    focused=read('focused-parent-comparison-v1.json');assert [p['v2'] for p in focused['parents']]==['PASS','FAIL'] and all(p['v1']=='ERROR' for p in focused['parents']) and focused['accepted46_message_exact'] and focused['old_errors_retained']
    py=read('python-reconciliation-v2.json');oldpy=json.loads(files['evidence/04A47/python/parent-event-reconciliation-v2.json'])
    parents=py['parents'];ids=[r['id'] for r in parents];assert len(ids)==len(set(ids))==956 and set(ids)=={r['id'] for r in oldpy['parents']}
    assert collections.Counter(p['outcome'] for p in parents)==dict(PASS=833,FAIL=112,ERROR=11)==py['outcomes']
    assert py['classification']=='FRESH_COMPLETE_CAMPAIGN_NOT_COMPOSITE' and py['campaigns']==1 and py['workers_at_peak']==3 and py['all_source_modules_exact'] and py['all_input_pins_unchanged']
    previous={p['id']:p for p in oldpy['parents']}
    guard='test_04a05_completion_successor.Completion05SuccessorTests.test_no_designer_importer_targets_exports_or_signing_change'
    for p in parents:
        assert p['v1']==previous[p['id']]['outcome'] and p['source']['sha256']==previous[p['id']]['source']['source']['sha256']
        if p['id']==guard:assert p['v1']==p['accepted46']=='PASS' and p['outcome']=='FAIL'
        else:assert p['outcome']==p['accepted46']
    changed=[p for p in parents if p['outcome']!=p['v1']];assert {p['id'] for p in changed}=={p['id'] for p in focused['parents']}|{guard} and len(changed)==3
    assert any(e['id']==guard and e['disposition']=='OWNER_APPROVED_IGNORE_POLICY_DIFF_REACTIVATES_UNCHANGED_SOURCE_GUARD;ACTUAL_FAIL_UNWAIVED' for e in py['events'])
    assert py['unresolved_diagnostic_events']==[] and py['all_historical_nonpasses_unwaived'] and py['old_v1_errors_preserved'] and all(e['disposition']!='UNRESOLVED' for e in py['events'])
    continuity=read('inherited-software-and-preservation-v1.json');assert continuity['Debug']['outcomes']==dict(Passed=21881,Failed=14) and continuity['Release']['outcomes']==dict(Passed=15885,Failed=10) and continuity['full_Release']=='NOT_RUN'
    assert continuity['exports_per_configuration']==268 and continuity['compatibility_per_configuration']==dict(total=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458) and continuity['methods']==101 and continuity['new_managed_tests']==141 and continuity['observer_cases_per_configuration']==147
    assert continuity['canonical_counts']==dict(issues=1169,obligations=1514,restricted_controls=802,mappings=2967) and continuity['discharges']==0
    state=read('readiness-v2.json');assert state['status']==STATUS and state['native']=='NOT_RUN' and state['actual_HWND_PID_XLL']=='NOT_VERIFIED' and state['approval']=='NOT_APPROVED' and not state['MayRunNative'] and state['scientific']=='NOT_GOLDEN' and state['discharges']==0 and state['next']=='04A48_NOT_AUTHORIZED'
    assert state['privacy_candidate_scope']=='ORIGINAL_UNCHANGED_PATHS_INCLUDING_NONIGNORED_PAYLOAD' and state['effective_only_with_matching_external_receipt']
    if receipt:
        d=json.loads(Path(receipt).read_bytes());b=Path(archive).read_bytes();assert d['archive']['bytes']==len(b) and d['archive']['sha256']==sha(b) and d['status']==STATUS
        assert {p['path'] for p in d['physical_members']}==set(files)
        for p in d['physical_members']:pin(p)
        assert len(d['candidate_scans'])==len(d['physical_scans'])==2
        for p in d['candidate_scans']:
            assert p['findings']==p['errors']==0 and PAYLOAD in {m['path'] for m in p['members']} and p['scope']=='ORIGINAL_UNCHANGED_PATHS_FUNCTIONS'
        assert all(p['findings']==p['errors']==0 and p['members']==len(files) and p['includes_index'] for p in d['physical_scans'])
        assert len(d['adverse_controls'])>=60 and all(p['detected'] for p in d['adverse_controls'])
        assert d['postdelivery_git']['staged_empty'] and d['postdelivery_git']['index']['sha256']==auth['git']['index']['sha256']
        for p in d['primary_reports']:pin(p)
    return dict(status='PASS_EXTERNAL_AND_SEMANTIC' if receipt else 'PASS_STRUCTURAL_AND_SEMANTIC',members=len(files),readiness=STATUS,native='NOT_RUN',effective_only_with_matching_external_receipt=True)
def adverse(archive):
    with zipfile.ZipFile(archive) as z:base={n:z.read(n) for n in z.namelist()}
    old,z=legacy(base);results=[dict(r,scope='ORIGINAL_48_ON_EXACT_SEALED_V1_PAYLOAD_WITHIN_V2') for r in old.adverse(z)]
    mutations={
      'fake_original_hash':('payload-authentication-v1.json',lambda d:d['payload'].__setitem__('sha256','0'*64)),
      'fake_original_size':('payload-authentication-v1.json',lambda d:d['payload'].__setitem__('bytes',1519722)),
      'forged_recovery_present':('payload-authentication-v1.json',lambda d:d.__setitem__('pre_post_read_exact',False)),
      'altered_manifest_authority':('payload-authentication-v1.json',lambda d:d['manifest'].__setitem__('sha256','0'*64)),
      'altered_VB_authority':('payload-authentication-v1.json',lambda d:d['vb_authority'].__setitem__('sha256','0'*64)),
      'falsified_04A23_PASS':('focused-parent-comparison-v1.json',lambda d:d['parents'][0].__setitem__('v2','ERROR')),
      'converted_historical_04A24_FAIL':('focused-parent-comparison-v1.json',lambda d:d['parents'][1].__setitem__('v2','PASS')),
      'suppressed_v1_ERROR':('focused-parent-comparison-v1.json',lambda d:d['parents'][0].__setitem__('v1','PASS')),
      'aggregate_only_Python':('python-reconciliation-v2.json',lambda d:d.__setitem__('parents',[])),
      'duplicated_Python_parent':('python-reconciliation-v2.json',lambda d:d['parents'].append(d['parents'][0])),
      'missing_Python_parent':('python-reconciliation-v2.json',lambda d:d['parents'].pop()),
      'fake_composite_as_fresh':('python-reconciliation-v2.json',lambda d:d.__setitem__('classification','COMPOSITE')),
      'suppressed_actual_ignore_guard_FAIL':('python-reconciliation-v2.json',lambda d:next(p for p in d['parents'] if p['id'].startswith('test_04a05_completion_successor.') and p['id'].endswith('test_no_designer_importer_targets_exports_or_signing_change')).__setitem__('outcome','PASS')),
      'changed_v1_archive_identity':('payload-authentication-v1.json',lambda d:d['v1_archive'].__setitem__('sha256','0'*64)),
      'changed_v1_receipt_identity':('payload-authentication-v1.json',lambda d:d['v1_receipt'].__setitem__('bytes',1)),
      'native_PASS':('readiness-v2.json',lambda d:d.__setitem__('native','PASS')),
      'privacy_scope_narrowed':('readiness-v2.json',lambda d:d.__setitem__('privacy_candidate_scope','ZIP_ONLY')),
      'unauthorized_discharge':('readiness-v2.json',lambda d:d.__setitem__('discharges',1)),
      'forged_loaded_XLL':('readiness-v2.json',lambda d:d.__setitem__('actual_HWND_PID_XLL','VERIFIED')),
      'forged_new_full_Release':('inherited-software-and-preservation-v1.json',lambda d:d.__setitem__('full_Release','PASS')),
      'unapproved_ignored_status':('payload-authentication-v1.json',lambda d:d.__setitem__('ignored',True))}
    for label in list(mutations)+['public_inclusion_of_raw_payload','unsafe_filename','case_collision','NFKC_collision']:
        files=dict(base)
        if label in mutations:
            n,f=mutations[label];d=json.loads(files[P+n]);f(d);files[P+n]=json.dumps(d).encode()
        elif label=='public_inclusion_of_raw_payload':files[PAYLOAD]=b'forbidden placeholder'
        elif label=='unsafe_filename':files['../unsafe.txt']=b'unsafe'
        elif label=='case_collision':files[(P+'readiness-v2.json').upper()]=files[P+'readiness-v2.json']
        else:files[P+'\uff52eadiness-v2.json']=files[P+'readiness-v2.json']
        files['.review-index.json']=json.dumps(dict(files=[dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}') for n,b in sorted(files.items()) if n!='.review-index.json'])).encode()
        try:verify(packed(files))
        except (AssertionError,KeyError,ValueError,zipfile.BadZipFile):results.append(dict(control=label,detected=True,scope='NEW_V2_RECOVERY_READER_PHYSICAL_MUTATION_REINDEXED'))
        else:raise AssertionError('MISSED_RECOVERY_CONTROL:'+label)
    return results
if __name__=='__main__':print(json.dumps(verify(sys.argv[1],sys.argv[2] if len(sys.argv)>2 else None),sort_keys=True))
