"""Standalone archive and optional external-receipt reader;no extraction or host execution."""
import collections,copy,hashlib,io,json,re,sys,unicodedata,zipfile,zlib
PREFIX='evidence/04A42/'
STATUS='READY_FOR_OWNER_04A42_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A43_NOT_AUTHORIZED')
def sha(b):return hashlib.sha256(b).hexdigest()
def safe(n):
    assert n and not n.startswith('/') and chr(92) not in n and ':' not in n
    assert all(p not in ('','.','..') and not p.endswith((' ','.')) for p in n.split('/'))
    assert not re.search(r'(?i)(?:^|/)(?:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\.|/|$)',n)
def verify(archive,external=None):
    with zipfile.ZipFile(archive) as z:
        names=z.namelist();assert len(names)==len(set(names))==len({unicodedata.normalize('NFKC',n).casefold() for n in names})
        for n in names:safe(n)
        assert z.testzip() is None
        index=json.loads(z.read('.review-index.json'));files={r['path']:r for r in index['files']}
        assert len(files)==len(index['files']) and set(names)==set(files)|{'.review-index.json'}
        for name,r in files.items():
            info=z.getinfo(name);data=z.read(name);assert not info.is_dir() and ((info.external_attr>>16)&0o170000)!=0o120000
            assert len(data)==r['bytes'] and sha(data)==r['sha256'] and f'{zlib.crc32(data)&0xffffffff:08x}'==r['crc32']==f'{info.CRC:08x}'
        read=lambda p:json.loads(z.read(PREFIX+p))
        state=read('final/readiness-postseal-v1.json');assert state['status']==STATUS and state['gates']==GATES and state['effective_only_with_matching_external_receipt']
        assert not state['owner_accepted'] and not state['merged'] and not state['phase04A43_authorized'] and state['scientific_corrections']==state['native_activations']==0
        auth=read('entry/review-authentication-v1.json');assert auth['HEAD']==auth['main']==auth['merge_base']=='1df856c47f2342e26de1115f1621722b7c0ac23e' and auth['branch']=='codex/04A-42' and auth['raw40_exclusion_retained']
        source=read('matrix/source-freeze-v2.json');assert len(source['source_delta'])==8 and len(source['new_tests'])==2
        for r in [source['plan'],source['plan_snapshot']]+source['new_tests']+[r['after'] for r in source['source_delta']]:
            data=z.read(r['path']);assert len(data)==r['bytes'] and sha(data)==r['sha256']
        preimages=read('source/preimages-v1.json')['files'];assert len(preimages)==8
        for r in preimages:
            data=z.read(r['public_exact_source_copy']['path']);assert len(data)==r['original']['bytes'] and sha(data)==r['original']['sha256']
        plan=read('planning/reconciliation-v1.json');assert len(plan['identities'])==len(set(plan['identities']))==226 and plan['revision']=='2.19'
        ownership=read('scope/ownership-parity-v1.json');assert ownership['scoped_exports']==len(ownership['function_matrix'])==90 and ownership['total_exports']==268 and ownership['changed_parent_count']==14 and ownership['policy_members']==192
        assert all(r['success_cases'] for r in ownership['function_matrix'] if r['parent_changed']) and all(r['unchanged'] for r in ownership['ordinary_coverage_holes'])
        gui=read('scope/gui-current-member-crosswalk-v1.json');assert len(gui['functions'])==90 and not gui['GUI_source_changed'] and not gui['native_executed']
        portable=read('scope/portable-member-lock-v1.json');assert portable['added_public_methods']==10 and portable['existing_Common_services_extended']==4 and portable['new_Common_classes']==0 and not portable['existing_Common_method_bodies_changed']
        assert not portable['Core_algorithms_changed'] and not portable['Windows_GUI_native_renderers_changed'] and all(r['exact'] for r in portable['session_lifecycle'])
        delta=read('register/issue-delta-v1.json');assert delta['counts']==dict(canonical_entries=1169,original_obligations=1514,restricted_controls=802,mappings=2967) and len(delta['issues'])==16
        assert delta['original_controls_discharged']==delta['original_obligations_discharged']==delta['scientific_corrections_activated']==delta['native_changes']==delta['new_canonical_ids']==0
        assert all(not r['correction_authorized'] and r['original48_exit_status'] is None for r in delta['issues'])
        failed=read('matrix/failed-freeze-v1-disposition.json');assert failed['status'].startswith('FAILED_') and len(failed['new_failures'])==2
        debug=read('build-and-tests/Debug-identities-v2.json');assert len(debug['tests'])==debug['summary']['identities']==21177
        assert collections.Counter(r['outcome'] for r in debug['tests'])==debug['summary']['outcomes']==dict(Passed=21163,Failed=14)
        assert not debug['summary']['missing'] and len(debug['added'])==20 and all(r['outcome']=='Passed' for r in debug['added'])
        assert len(debug['nonpasses'])==14 and all(r['classification']=='RETAINED_IDENTICAL' for r in debug['nonpasses'])
        release=read('build-and-tests/Release-identities-v2.json');assert release['full_Release']=='NOT_RUN' and release['summary']['outcomes']==dict(Passed=6589) and release['summary']['scope']=='TARGETED_AFFECTED_CONTRACTS;FULL_RELEASE_NOT_RUN'
        adapters=read('waves/corrected-adapters-v2.json');assert adapters['outcomes']==dict(Passed=198) and not adapters['existing_tests_modified']
        py=read('python/parent-event-reconciliation-v2.json');assert len(py['parents'])==len({p['id'] for p in py['parents']})==956
        assert collections.Counter(p['outcome'] for p in py['parents'])==py['summary']['parent_outcomes']==dict(PASS=832,FAIL=113,ERROR=11)
        assert py['summary']['event_outcomes']==dict(FAIL=185,ERROR=11) and not py['summary']['unresolved_diagnostic_events'] and not py['summary']['changed_parents']
        disp=read('python/diagnostic-dispositions-v1.json');assert not disp['unresolved'] and disp['historical_outcomes_retained']
        guards=read('python/source-guard-dispositions-v1.json')['guards'];assert len(guards)==3 and all(r['current_status']=='FAIL' and not r['waived'] for r in guards)
        privacy=read('privacy/repository-candidate-v1.json');assert len(privacy['reports'])==2 and all(r['findings']==r['unresolved']==0 for r in privacy['reports'])
        assert all(r['negative_control']['zero_findings'] and all(c['detected'] for c in r['identity_controls']) for r in privacy['reports'])
        for config in ['Debug','Release']:
            abi=read('abi-cil/'+config+'-proof-v3.json');assert abi['exports']==268 and abi['exact_export_order_names_signatures_attributes'] and not abi['unexpected'] and not abi['removed']
            exports=read('abi-cil/'+config+'-exports-review-v1.json');assert len(exports['exports'])==268 and exports['exact_accepted41']
            compat=read('compatibility/'+config+'-v3.json');assert compat['cases']==29424 and compat['PRE_EDIT_FROZEN']==28966 and compat['LATE_ARCHIVED_ENTRY_SUPPLEMENT']==458 and compat['original_queries']==0 and compat['exact_typed_order_input_schema_identities']
            assert all(r['exact'] for r in compat['results'])
            for family,count in [('39',77),('41',390)]:f=read('fixtures/'+config+'-'+family+'-v3.json');assert f['cases']==count and f['exact']
            f=read('fixtures/'+config+'-40-v3.json');assert f['cases']==48 and f['statistical_input_exception_contracts_exact'] and len(f['elapsed_only_cases'])==4
            f=read('fixtures/'+config+'-42-v3.json');assert f['cases']==1802 and f['typed_statistical_shape_error_order_exact'] and len(f['only_excluded_runtime_fields'])==2
        f=read('fixtures/current-main-review-v1.json');assert len(f['data']['rows'])==len({r['id'] for r in f['data']['rows']})==1802 and 'NOT_RAW' in f['kind']
        pr=read('preservation/readback-v1.json');assert pr['entry_tracked_files']==14493 and pr['checkpoint']['files']==pr['checkpoint']['exact_original_copies']==468 and pr['checkpoint']['live_exact']==464
        assert pr['historical_protected']['paths']==8984 and pr['newer40_raw']['paths']==1906 and pr['raw41_prior_pins_exact']==6810 and pr['historical_source_entry_identities']==1353
        assert pr['continuous_physical_identity'].startswith('NOT_VERIFIED') and not pr['W3_union']['fresh_full_union_audit'] and pr['Node']['outcome']=='INHERITED_916_PASS;NOT_RERUN'
        for raw in ['evidence/04A40/baseline/Debug-build-v1.json','evidence/04A40/entry/tool-runtime-final-v1.json']:assert raw not in names
        assert read('matrix/serial-checks-v2.json')['exits']==[1,1,0]
        git=read('final/git-final-v1.json');assert git['staged_empty'] and git['diff_check_exit']==0 and git['HEAD']==auth['HEAD']
        if external is not None:
            from pathlib import Path
            receipt=json.loads(Path(external).read_bytes());data=Path(archive).read_bytes();assert len(data)==receipt['archive']['bytes'] and sha(data)==receipt['archive']['sha256'] and receipt['status']==STATUS
            assert set(names)=={r['path'] for r in receipt['physical_members']}
            for r in receipt['physical_members']:
                data=z.read(r['path']);assert len(data)==r['bytes'] and sha(data)==r['sha256'] and f'{zlib.crc32(data)&0xffffffff:08x}'==r['crc32']
            assert len(receipt['physical_scans'])==len(receipt['full_actual_candidate_scans'])==2 and all(r['findings']==r['unresolved']==0 for r in receipt['physical_scans']+receipt['full_actual_candidate_scans'])
            assert len(receipt['adverse_controls'])>=12 and all(r['detected'] for r in receipt['adverse_controls'])
        return dict(status='PASS_STRUCTURAL_SEMANTIC_AND_EXTERNAL_READBACK' if external else 'PASS_STRUCTURAL_AND_SEMANTIC_READBACK',physical_members=len(names),indexed=len(files),SHA_size_CRC='PASS',names='NFKC_CASEFOLD_SAFE_NO_COLLISIONS',missing_extra='NONE',readiness=STATUS,scientific='NOT_GOLDEN')
def adverse(archive):
    with zipfile.ZipFile(archive) as z:original={n:z.read(n) for n in z.namelist()}
    def alter(f,p,change):d=json.loads(f[PREFIX+p]);change(d);f[PREFIX+p]=json.dumps(d).encode()
    controls=[]
    labels=['missing','extra','case_collision','unsafe','changed_source','changed_preimage','greenwash_guard','fake_golden','lost_case','wrong_export_count','missing_inherited_failure','full_release_claim','privacy_scope_loss','restricted_correction','next_batch','hidden_failed_attempt']
    for label in labels:
        f=copy.deepcopy(original);reindex=True
        if label=='missing':f.pop(PREFIX+'final/readiness-postseal-v1.json');reindex=False
        elif label=='extra':f['extra.txt']=b'extra';reindex=False
        elif label=='case_collision':f[(PREFIX+'review_reader.py').upper()]=f[PREFIX+'review_reader.py'];reindex=False
        elif label=='unsafe':f['../unsafe.txt']=b'unsafe'
        elif label=='changed_source':f['BESHStat.Excel.Common/src/Analysis/UdfPlotDataPreparation38.vb']+=b'changed'
        elif label=='changed_preimage':p=next(p for p in f if '/source/preimages-v1/' in p);f[p]+=b'changed'
        elif label=='greenwash_guard':alter(f,'python/source-guard-dispositions-v1.json',lambda d:d['guards'][0].__setitem__('current_status','PASS'))
        elif label=='fake_golden':alter(f,'final/readiness-postseal-v1.json',lambda d:d['gates'].__setitem__('P','YES'))
        elif label=='lost_case':alter(f,'fixtures/current-main-review-v1.json',lambda d:d['data']['rows'].pop())
        elif label=='wrong_export_count':alter(f,'abi-cil/Release-exports-review-v1.json',lambda d:d['exports'].pop())
        elif label=='missing_inherited_failure':alter(f,'build-and-tests/Debug-identities-v2.json',lambda d:d['nonpasses'].pop())
        elif label=='full_release_claim':alter(f,'build-and-tests/Release-identities-v2.json',lambda d:d.__setitem__('full_Release','PASS'))
        elif label=='privacy_scope_loss':alter(f,'privacy/repository-candidate-v1.json',lambda d:d['reports'].pop())
        elif label=='restricted_correction':alter(f,'register/issue-delta-v1.json',lambda d:d['issues'][0].__setitem__('correction_authorized',True))
        elif label=='next_batch':alter(f,'final/readiness-postseal-v1.json',lambda d:d.__setitem__('phase04A43_authorized',True))
        elif label=='hidden_failed_attempt':alter(f,'matrix/failed-freeze-v1-disposition.json',lambda d:d.__setitem__('status','PASS'))
        if reindex:f['.review-index.json']=json.dumps(dict(files=[dict(path=n,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data)&0xffffffff:08x}') for n,data in f.items() if n!='.review-index.json'])).encode()
        out=io.BytesIO()
        with zipfile.ZipFile(out,'w',compression=zipfile.ZIP_DEFLATED) as z:
            for n,data in f.items():z.writestr(n,data)
        out.seek(0)
        try:verify(out)
        except (AssertionError,KeyError,ValueError,zipfile.BadZipFile):controls.append(dict(control=label,detected=True))
        else:raise AssertionError('MISSED_READER_CONTROL:'+label)
    return controls
if __name__=='__main__':print(json.dumps(verify(sys.argv[1],sys.argv[2] if len(sys.argv)>2 else None),sort_keys=True))
