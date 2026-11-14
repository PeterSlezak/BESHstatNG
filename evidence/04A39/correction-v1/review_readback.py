"""Standalone archive reader. Supply the SHA from the external owner delivery receipt."""
import argparse,collections,hashlib,json,re,zipfile
from pathlib import Path

def sha(b):return hashlib.sha256(b).hexdigest()
def verify(path,expected_sha):
    with Path(path).open('rb') as f:actual=hashlib.file_digest(f,'sha256').hexdigest()
    assert actual==expected_sha,'ARCHIVE_SHA_MISMATCH'
    with zipfile.ZipFile(path) as z:
        names=z.namelist()
        assert len(names)==len(set(names))==len({n.casefold() for n in names}),'DUPLICATE_COLLIDING_PATH'
        for n in names:
            assert n and not n.startswith('/') and '\\' not in n and ':' not in n and all(p not in ('','.','..') for p in n.split('/')),'UNSAFE_PATH'
        assert z.testzip() is None,'CRC_FAILURE'
        index=json.loads(z.read('.review-inventory.json'));gate=json.loads(z.read('.review-gate.json'))
        files=index['files'];byname={r['path']:r for r in files}
        assert len(byname)==len(files) and set(names)==set(byname)|{'.review-inventory.json'},'MISSING_EXTRA_PAYLOAD'
        for name,row in byname.items():
            data=z.read(name);info=z.getinfo(name)
            assert len(data)==row['bytes'] and sha(data)==row['sha256'] and f'{info.CRC:08x}'==row['crc32'],'PAYLOAD_CHANGED'
        excluded=index['protected_exclusions'];blocked={r['original']['sha256'] for r in excluded}
        assert not set(names)&{r['path'] for r in excluded},'PROTECTED_RAW_PATH'
        assert not blocked&{r['sha256'] for r in files},'RENAMED_PROTECTED_RAW_BYTES'
        coverage=index['source_coverage']
        assert coverage['source_paths']==len(coverage['files'])==1353 and coverage['exact']==1322 and coverage['rendered']==31
        for r in coverage['files']:
            payload=r['payload'];assert byname[payload['path']]['sha256']==payload['sha256']
            if r['classification']=='EXACT_SOURCE':assert r['original']['sha256']==payload['sha256']
        assert gate['unresolved']==0 and gate['scanner_findings']==gate['reviewed_exact_occurrences']==25
        assert gate['repository_candidate_status']=='FAIL_UNWAIVED'
        assert gate['gates']==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A40_NOT_AUTHORIZED')
        prefix='evidence/04A39/correction-v1/'
        parents=json.loads(z.read(prefix+'diagnostics/parent-956-reconciliation.json'))['parents']
        assert len(parents)==len({r['id'] for r in parents})==956
        assert collections.Counter(r['original39_outcome'] for r in parents)==dict(PASS=833,FAIL=112,ERROR=11)
        changes=json.loads(z.read(prefix+'diagnostics/changed-54-crosswalk-v2.json'))['changes']
        assert len(changes)==len({r['stable_event_id'] for r in changes})==54
        assert all(r['trace_difference_only_assertion_message'] for r in changes)
        for name in ('reporting-boundary-tests.json','evidence-reader-controls-v2.json','source-disposition-controls.json'):
            receipt=json.loads(z.read(prefix+'validation/'+name));assert receipt['failures']==receipt['errors']==receipt['exit_code']==0
        register=json.loads(z.read(prefix+'validation/retained-evidence-readback.json'))['register_counts']
        assert (register['canonical_entries'],register['original_obligations'],register['restricted_controls'])==(1169,1514,802)
        return dict(status='PASS_DERIVATIVE_INTEGRITY_READBACK',physical_files=len(names),source_paths=1353,exact_sources=1322,source_renderings=31,
            parent_identities=956,diagnostic_changes=54,privacy='DERIVATIVE_GATE_ONLY;REPOSITORY_NONPASS_UNWAIVED',acceptance='OWNER_DECISION_REQUIRED')
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('archive');parser.add_argument('--sha256',required=True);args=parser.parse_args()
    print(json.dumps(verify(args.archive,args.sha256),sort_keys=True))
