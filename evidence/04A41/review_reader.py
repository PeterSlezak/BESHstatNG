"""Standalone04A41 archive reader: no extraction, package installs or host execution."""
import collections,copy,hashlib,io,json,re,sys,unicodedata,zipfile,zlib
PREFIX='evidence/04A41/'
STATUS='READY_FOR_OWNER_04A41_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A42_NOT_AUTHORIZED')
def sha(b):return hashlib.sha256(b).hexdigest()
def safe(n):
    assert n and not n.startswith('/') and '\\' not in n and ':' not in n
    assert all(p not in ('','.','..') and not p.endswith((' ','.')) for p in n.split('/'))
    assert not re.search(r'(?i)(?:^|/)(?:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\.|/|$)',n)
def verify(archive):
    with zipfile.ZipFile(archive) as z:
        names=z.namelist();assert len(names)==len(set(names))==len({unicodedata.normalize('NFKC',n).casefold() for n in names})
        for n in names:safe(n)
        assert z.testzip() is None
        index=json.loads(z.read('.review-index.json'));files={r['path']:r for r in index['files']}
        assert len(files)==len(index['files']) and set(names)==set(files)|{'.review-index.json'}
        for name,r in files.items():
            info=z.getinfo(name);data=z.read(name)
            assert not info.is_dir() and ((info.external_attr>>16)&0o170000)!=0o120000
            assert len(data)==r['bytes'] and sha(data)==r['sha256'] and f'{zlib.crc32(data)&0xffffffff:08x}'==r['crc32']==f'{info.CRC:08x}'
        read=lambda name:json.loads(z.read(PREFIX+name))
        readiness=read('final/readiness-postseal-v1.json')
        assert readiness['status']==STATUS and readiness['gates']==GATES and readiness['effective_only_with_matching_external_receipt']
        assert not readiness['owner_accepted'] and not readiness['merged'] and not readiness['phase04A42_authorized']
        assert readiness['scientific_corrections']==readiness['native_activations']==0
        auth=read('entry/review-authentication-v2.json');assert auth['HEAD']==auth['main']==auth['merge_base']=='c6c22dbc59d6dfc61ed64f6413d8c81e7102e1dc'
        assert auth['branch']=='codex/04A-41' and auth['raw40_boundary_verified'] and auth['plan_ids']==226
        source=read('matrix/source-freeze-v1.json');assert len(source['source_delta'])==10 and len(source['new_tests'])==2
        assert not any(r['after']['path'].startswith(('BESHStat.Core/','BESHstatNG/src/UI/')) for r in source['source_delta'])
        for r in [source['plan'],source['plan_snapshot']]:
            data=z.read(r['path']);assert len(data)==r['bytes'] and sha(data)==r['sha256']
        for r in source['source_delta']:
            data=z.read(r['after']['path']);assert len(data)==r['after']['bytes'] and sha(data)==r['after']['sha256']
        for r in source['new_tests']:
            data=z.read(r['path']);assert len(data)==r['bytes'] and sha(data)==r['sha256']
        plan=read('planning/reconciliation-v1.json');assert len(plan['identities'])==len(set(plan['identities']))==226 and plan['revision']=='2.18'
        delta=read('register/issue-delta-v2.json');assert delta['counts']['canonical_entries']==1169 and delta['counts']['original_obligations']==1514 and delta['counts']['restricted_controls']==802 and delta['counts']['mappings']==2967
        assert len(delta['issues'])==17 and delta['original_controls_discharged']==delta['scientific_corrections_activated']==delta['native_changes']==0
        assert all(r.get('original48_exit_status') is None for r in delta['issues'])
        ownership=read('scope/ownership-parity-v1.json');assert len(ownership['function_matrix'])==76 and ownership['total_exports']==268 and ownership['changed_parent_count']==28
        assert len(ownership['ordinary_coverage_holes'])==5 and all(r['method_unchanged'] for r in ownership['ordinary_coverage_holes'])
        gui=read('scope/gui-current-member-crosswalk-v1.json');assert len(gui['functions'])==76 and not gui['GUI_source_changed'] and not gui['native_executed']
        portable=read('scope/portable-member-lock-v1.json');assert portable['added_public_methods']==22 and portable['existing_Common_services_extended']==5 and portable['new_Common_classes']==0 and not portable['existing_Common_method_bodies_changed']
        debug=read('build-and-tests/Debug-identities-v1.json');tests=debug['tests']
        assert len(tests)==debug['summary']['identities']==21157
        assert collections.Counter(r['outcome'] for r in tests)==debug['summary']['outcomes']==dict(Passed=21143,Failed=14)
        assert not debug['summary']['missing'] and len(debug['added'])==18 and all(r['outcome']=='Passed' for r in debug['added'])
        assert len(debug['nonpasses'])==14 and all(r['classification']=='RETAINED_IDENTICAL' for r in debug['nonpasses'])
        release=read('build-and-tests/Release-identities-v1.json')
        assert release['full_Release']=='NOT_RUN' and release['summary']['outcomes']==dict(Passed=2271,Failed=1)
        assert release['summary']['scope']=='TARGETED_AFFECTED_CONTRACTS;FULL_RELEASE_NOT_RUN'
        trace=read('build-and-tests/Release-trace-disposition-v1.json');assert trace['retained_status']=='FAIL' and not trace['waived']
        assert trace['before_Debug_source_lines']==['88'] and trace['current_Release_source_lines']==['89']
        py=read('python/parent-event-reconciliation-v2.json');assert len(py['parents'])==len({p['id'] for p in py['parents']})==956
        assert collections.Counter(p['outcome'] for p in py['parents'])==py['summary']['parent_outcomes']==dict(PASS=832,FAIL=113,ERROR=11)
        assert py['summary']['event_outcomes']==dict(FAIL=185,ERROR=11) and not py['summary']['unresolved_diagnostic_events']
        dispositions=read('python/diagnostic-dispositions-v1.json');assert dispositions['unresolved']==0 and len(dispositions['rows'])==23 and dispositions['historical_outcomes_retained']
        guards=read('python/source-guard-dispositions-v1.json')['guards'];assert len(guards)==3 and all(r['current_status']=='FAIL' and not r['waived'] for r in guards)
        privacy=read('privacy/repository-candidate-v1.json');assert len(privacy['reports'])==2 and all(r['findings']==0 for r in privacy['reports'])
        assert all(c['detected'] for r in privacy['reports'] for c in r['identity_controls'])
        for config in ['Debug','Release']:
            abi=read('abi-cil/'+config+'-proof-v1.json');assert abi['exports']==268 and abi['exact_export_order_names_signatures_attributes'] and not abi['unexpected'] and not abi['removed']
            exports=read('abi-cil/'+config+'-exports-review-v1.json');assert len(exports['exports'])==268 and exports['exact_accepted40']
            compat=read('compatibility/'+config+'-v1.json');assert compat['cases']==29424 and compat['PRE_EDIT_FROZEN']==28966 and compat['LATE_ARCHIVED_ENTRY_SUPPLEMENT']==458
            assert compat['original_queries']==0 and compat['exact_typed_order_input_schema_identities'] and all(r['exact'] for r in compat['results'])
            f=read('fixtures/'+config+'-41-v1.json');assert f['cases']==390 and f['exact']
            f=read('fixtures/'+config+'-39-v1.json');assert f['cases']==77 and f['exact']
            f=read('fixtures/'+config+'-40-v1.json');assert f['cases']==48 and f['statistical_input_exception_contracts_exact'] and len(f['elapsed_only_cases'])==4
        fixture=read('fixtures/current-main-390-review-v1.json');assert len(fixture['data']['rows'])==len({r['id'] for r in fixture['data']['rows']})==390
        assert fixture['kind']=='REVIEW_COMPATIBILITY_DATASET;NOT_RAW_EXECUTION_TRANSCRIPT_OR_SCIENTIFIC_GOLDEN'
        pr=read('preservation/readback-v1.json');assert pr['entry_tracked_files']==14406 and pr['checkpoint']['files']==pr['checkpoint']['exact_original_copies']==468
        assert pr['checkpoint']['live_exact']==464 and pr['checkpoint']['previous_authorized_newline_renderings']==4
        assert pr['historical_protected']['paths']==8984 and pr['newer40_raw']['paths']==1906 and pr['historical_source_entry_identities']==1353
        assert pr['continuous_physical_identity'].startswith('NOT_VERIFIED') and not pr['W3_union']['fresh_full_union_audit']
        assert pr['Node']['outcome']=='INHERITED_916_PASS;NOT_RERUN'
        for raw in ['evidence/04A40/baseline/Debug-build-v1.json','evidence/04A40/entry/tool-runtime-final-v1.json']:assert raw not in names
        serial=read('matrix/serial-checks-v1.json');assert serial['exits']==[1,1,0]
        final=read('final/git-final-v1.json');assert final['staged_empty'] and final['diff_check_exit']==0 and final['HEAD']==auth['HEAD']
        return dict(status='PASS_STRUCTURAL_AND_SEMANTIC_READBACK',physical_members=len(names),indexed=len(files),SHA_size_CRC='PASS',names='NORMALIZED_NFKC_CASEFOLD_SAFE;NO_COLLISIONS',missing_extra='NONE',readiness=readiness['status'],privacy='External original-scanner receipt required;reader itself is not a privacy scanner.',native_scientific='NOT_RUN;NOT_GOLDEN')
def adverse(archive):
    with zipfile.ZipFile(archive) as z:original={n:z.read(n) for n in z.namelist()}
    def build(files,reindex=False):
        if reindex:files['.review-index.json']=json.dumps(dict(files=[dict(path=n,bytes=len(d),sha256=sha(d),crc32=f'{zlib.crc32(d)&0xffffffff:08x}') for n,d in files.items() if n!='.review-index.json'])).encode()
        output=io.BytesIO()
        with zipfile.ZipFile(output,'w',compression=zipfile.ZIP_DEFLATED) as z:
            for n,d in files.items():z.writestr(n,d)
        output.seek(0);return output
    def alter(files,path,change):
        d=json.loads(files[PREFIX+path]);change(d);files[PREFIX+path]=json.dumps(d).encode()
    controls=[]
    for label in ['missing','extra','case_collision','unsafe_name','changed_source','greenwash_guard','fake_golden','lost_case','wrong_export_count','missing_inherited_failure','full_release_claim','privacy_scope_loss']:
        files=copy.deepcopy(original);reindex=False
        if label=='missing':files.pop(PREFIX+'final/readiness-postseal-v1.json')
        elif label=='extra':files['extra.txt']=b'extra'
        elif label=='case_collision':files[(PREFIX+'review_reader.py').upper()]=files[PREFIX+'review_reader.py']
        elif label=='unsafe_name':files['../unsafe.txt']=b'unsafe';reindex=True
        elif label=='changed_source':files['BESHStat.Excel.Common/src/Results/UdfInferencePreparation38.vb']+=b'changed'
        elif label=='greenwash_guard':alter(files,'python/source-guard-dispositions-v1.json',lambda d:d['guards'][0].__setitem__('current_status','PASS'));reindex=True
        elif label=='fake_golden':alter(files,'final/readiness-postseal-v1.json',lambda d:d['gates'].__setitem__('P','YES'));reindex=True
        elif label=='lost_case':alter(files,'fixtures/current-main-390-review-v1.json',lambda d:d['data']['rows'].pop());reindex=True
        elif label=='wrong_export_count':alter(files,'abi-cil/Release-exports-review-v1.json',lambda d:d['exports'].pop());reindex=True
        elif label=='missing_inherited_failure':alter(files,'build-and-tests/Debug-identities-v1.json',lambda d:d['nonpasses'].pop());reindex=True
        elif label=='full_release_claim':alter(files,'build-and-tests/Release-identities-v1.json',lambda d:d.__setitem__('full_Release','PASS'));reindex=True
        elif label=='privacy_scope_loss':alter(files,'privacy/repository-candidate-v1.json',lambda d:d['reports'].pop());reindex=True
        try:verify(build(files,reindex))
        except (AssertionError,KeyError,ValueError,zipfile.BadZipFile):controls.append(dict(control=label,detected=True))
        else:raise AssertionError('MISSED_READER_CONTROL:'+label)
    return controls
if __name__=='__main__':print(json.dumps(verify(sys.argv[1]),sort_keys=True))
