"""Standalone small 04A.40 witness reader; no extraction, host execution or writes."""
import copy, hashlib, io, json, re, sys, zipfile, zlib

PREFIX='evidence/04A40/owner-authorized-candidate-v3/'
STATUS='READY_FOR_OWNER_04A40_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A41_NOT_AUTHORIZED')
def sha(data):return hashlib.sha256(data).hexdigest()
def safe(name):
    assert name and not name.startswith('/') and '\\' not in name and ':' not in name
    assert all(p not in ('','.','..') and not p.endswith((' ','.')) for p in name.split('/'))
    assert not re.search(r'(?i)(?:^|/)(?:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\.|/|$)',name)

def verify(archive):
    with zipfile.ZipFile(archive) as z:
        names=z.namelist();assert len(names)==len(set(names))==len({n.casefold() for n in names})
        for n in names:safe(n)
        assert z.testzip() is None
        index=json.loads(z.read('.review-index.json'));expected={r['path']:r for r in index['files']}
        assert len(expected)==len(index['files']) and set(names)==set(expected)|{'.review-index.json'}
        for name,row in expected.items():
            data=z.read(name);info=z.getinfo(name)
            assert not info.is_dir() and ((info.external_attr>>16)&0o170000)!=0o120000
            assert len(data)==row['bytes'] and sha(data)==row['sha256']
            assert f'{zlib.crc32(data)&0xffffffff:08x}'==row['crc32']==f'{info.CRC:08x}'
        read=lambda n:json.loads(z.read(PREFIX+n))
        readiness=read('readiness-postseal-v4.json')
        assert readiness['status']==STATUS and readiness['gates']==GATES and not readiness['accepted']
        assert readiness['effective_only_with_matching_external_delivery_receipt']
        assert readiness['full_Release']=='NOT_RUN' and not readiness['phase04A41_authorized']
        assert readiness['retained_nonpasses']['guard_failures']==3
        assert readiness['retained_nonpasses']['historical_diagnostic_differences']==54
        assert readiness['retained_nonpasses']['current_diagnostic_differences']==32
        for p in ['git-final-v4.json','repository-privacy-witness-v3.json','physical-readback-provenance-v3.json','source-lock-v3.json','owner-review-v3.md','physical-gate-policy-v3.json']:
            assert PREFIX+p in names
        privacy=read('repository-privacy-witness-v3.json')
        assert len(privacy['reports'])==2 and all(r['findings']==r['unresolved_new_04A40']==0 for r in privacy['reports'])
        assert all(c['detected'] for r in privacy['reports'] for c in r['identity_controls'])
        assert privacy['historical']['accepted39_privacy_parents']=='FAIL' and privacy['historical']['findings_each']==1941
        guards=read('scope-guard-dispositions-v3.json');assert guards['tests_run']==guards['FAIL']==3 and guards['PASS']==guards['ERROR']==0
        assert len(guards['guards'])==3 and all(r['status']=='FAIL' and not r['waived'] for r in guards['guards'])
        cross=read('raw-presentation-crosswalk-v3.json');assert len(cross['artifacts'])==2
        for r in cross['artifacts']:
            assert r['original']['path'] not in names and r['raw_failure']=='FAIL_RETAINED'
            assert r['kind']=='SANITIZED_REVIEW_AND_GIT_CANDIDATE_DOCUMENT;NOT_AN_ORIGINAL_EXECUTION_TRANSCRIPT'
            safe_doc=r['representation'];data=z.read(safe_doc['path'])
            assert sha(data)==safe_doc['sha256'] and len(data)==safe_doc['bytes']
            assert safe_doc['sha256']!=r['original']['sha256']
            label=json.loads(z.read(safe_doc['path'].removesuffix('.json')+'-candidate-provenance-v3.json'))
            assert label['representation']==safe_doc and label['kind']==r['kind']
        source=read('source-lock-v3.json')
        assert len(source['source_pins'])==1353 and len(source['authorized_production_files'])==15 and len(source['new_tests'])==2
        assert source['exports_per_configuration']==268 and source['cases_per_configuration']==29424
        assert source['PRE_EDIT_FROZEN']==28966 and source['LATE_ARCHIVED_ENTRY_SUPPLEMENT']==458
        pr=read('preservation-readback-v3.json')
        assert pr['checkpoint_files']==pr['exact_checkpoint_copies']==468 and pr['live_byte_exact']==464 and pr['previous_owner_authorized_newline_renderings']==4
        assert pr['protected_unique_paths']==8984 and pr['source_entry_identities']==1353
        assert pr['continuous_precommit_identity'].startswith('NOT_VERIFIED')
        assert pr['W3_union']['inherited_qualified'] and not pr['W3_union']['fresh_audit']
        g=read('git-final-v4.json')
        assert g['HEAD']==g['main']==g['merge_base']=='bba848305c2c46ff3fc835a1132522ebfec59514'
        assert g['staged_empty'] and g['diff_check_exit']==0 and not g['stage_commit_merge_rebase_push_release']
        prior=read('physical-readback-provenance-v3.json')
        assert prior['original_indexed_paths_contain']=='REVIEW_RENDERED_BYTES;NOT_EXACT_RAW_REPORT_BYTES'
        assert len(prior['rendered_reports'])==2
        return dict(status='STRUCTURAL_AND_SEMANTIC_READBACK_PASS',members=len(names),indexed=len(expected),SHA_size_CRC='PASS',normalized_names='PASS',missing_extra_colliding='NONE',readiness=readiness['status'],physical_privacy='EXTERNAL_ORIGINAL_SCANNER_RECEIPT_REQUIRED;READER_IS_NOT_A_PRIVACY_SCANNER')

def adverse(archive):
    with zipfile.ZipFile(archive) as z:original={n:z.read(n) for n in z.namelist()}
    def build(files,reindex=False):
        if reindex:
            rows=[dict(path=n,bytes=len(d),sha256=sha(d),crc32=f'{zlib.crc32(d)&0xffffffff:08x}') for n,d in files.items() if n!='.review-index.json']
            files['.review-index.json']=json.dumps(dict(files=rows)).encode()
        output=io.BytesIO()
        with zipfile.ZipFile(output,'w',compression=zipfile.ZIP_DEFLATED) as z:
            for n,d in files.items():z.writestr(n,d)
        output.seek(0);return output
    def change_doc(files,name,change):
        doc=json.loads(files[PREFIX+name]);change(doc);files[PREFIX+name]=json.dumps(doc).encode()
    probes=[]
    for label in ['missing','extra','case_collision','unsafe_name','changed_bytes','false_gate','guard_greenwash','lost_provenance']:
        files=copy.deepcopy(original);reindex=False
        if label=='missing':files.pop(PREFIX+'owner-review-v3.md')
        elif label=='extra':files['extra.txt']=b'extra'
        elif label=='case_collision':files[(PREFIX+'owner-review-v3.md').upper()]=files[PREFIX+'owner-review-v3.md']
        elif label=='unsafe_name':files['../unsafe.txt']=b'unsafe';reindex=True
        elif label=='changed_bytes':files[PREFIX+'owner-review-v3.md']+=b'changed'
        elif label=='false_gate':change_doc(files,'readiness-postseal-v4.json',lambda d:d['gates'].__setitem__('P','YES'));reindex=True
        elif label=='guard_greenwash':change_doc(files,'scope-guard-dispositions-v3.json',lambda d:d['guards'][0].__setitem__('status','PASS'));reindex=True
        elif label=='lost_provenance':change_doc(files,'raw-presentation-crosswalk-v3.json',lambda d:d['artifacts'][0].__setitem__('kind','ORIGINAL_RAW_TRANSCRIPT'));reindex=True
        try:verify(build(files,reindex))
        except (AssertionError,KeyError,ValueError,zipfile.BadZipFile):probes.append(dict(control=label,detected=True))
        else:raise AssertionError('MISSED_ADVERSE_CONTROL:'+label)
    return probes

if __name__=='__main__':print(json.dumps(verify(sys.argv[1]),sort_keys=True))
