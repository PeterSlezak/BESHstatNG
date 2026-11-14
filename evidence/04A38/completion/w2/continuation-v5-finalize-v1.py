"""Independent post-publication live closeout; leave the sealed packet unchanged."""
import sys
from pathlib import Path
CHECKPOINT=Path(__file__).resolve().parent/'continuation-v5'
sys.path.insert(0,str(CHECKPOINT))
from common import *
from verify_preservation_v3 import inspect as preservation
from verify_derivative_v3 import inspect as derivative

receipt_path=W2/'continuation-v5-publication-receipt-v1.json'
publication=json.loads(receipt_path.read_bytes())
assert publication['status']=='C5_PARTIAL_W2_NOT_READY_LIVE_COPY_PUBLICATION_VERIFIED'
assert len(publication['mutations'])==6 and all(r['status']=='REJECTED' for r in publication['mutations'])
assert publication['copied_executable']['exit_code']==0
assert publication['copied_result']['ownership']['W3']=='NOT_STARTED'
assert pin(ROOT/publication['manifest']['path'])==publication['manifest']
protected=preservation()
diagnostic=derivative()
relocation=json.loads((HERE/'diagnostic-encoding-relocation.json').read_bytes())
for key in ['retained_local','public_utf8_equivalent']:
    assert pin(ROOT/relocation[key]['path'])==relocation[key]

checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('diff-check',['diff','--check']),('status',['status','--short'])]:
    result=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True)
    assert result.returncode==0
    checks.append(dict(check=name,args=['git','-c','safe.directory=<REPO_ROOT>',*args],exit_code=0,stdout=result.stdout.decode('utf-8'),stderr=result.stderr.decode('utf-8')))
assert next(r for r in checks if r['check']=='branch')['stdout'].strip()=='codex/04A-38'
assert all(r['stdout'].strip()==BASE for r in checks if r['check'] in ['HEAD','main','merge-base'])
assert next(r for r in checks if r['check']=='index')['stdout']==''
result=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',publication=pin(receipt_path),
    live_preservation=protected,private_diagnostic_preservation=diagnostic,encoding_bytes_preserved=True,git=checks,
    source_authority=pin(HERE/'source-transitions-v4.json'),source_changes=8,
    final_readers=['verify_slice_v4.py','verify_ownership_final.py','verify_preservation_v3.py','verify_publication_final.py'],
    owner_review=pin(HERE/'owner-review.md'),handover=pin(HERE/'NEXT_WAVE_HANDOVER.md'),
    manifest_unchanged_since_publication=True,original_queries=0,
    publication_failure=pin(W2/'continuation-v5-publication-copy-attempt-v1.json'),
    benign_tooling_attempt=dict(tool='PowerShell5/Python command-line AST inspection of extended-path literal',exit_code=1,
        failure='SyntaxError: leading zeros in decimal integer literals are not permitted; PowerShell native argument quoting removed nested Python quotes',
        disposition='Read-only diagnostic failed before executing; no source or evidence mutations. File-based recovery avoids shell quote conversion.'),
    commands=[['<PYTHON>','-B','evidence/04A38/completion/w2/continuation-v5/export_and_verify_final.py'],
              ['<PYTHON>','-B','evidence/04A38/completion/w2/continuation-v5-publication-copy-recovery-v1.py'],
              ['<PYTHON>','-B','evidence/04A38/completion/w2/continuation-v5-finalize-v1.py']],
    restrictions='No W3; no original reference queries, native controls, full Release, restore/install/signing/deployment, stage/commit/merge/push, owner-document edits or next numbered batch')
dest=W2/'continuation-v5-final-git-preservation-v1.json'
with dest.open('x',encoding='utf-8',newline='\n') as stream:json.dump(result,stream,ensure_ascii=False,indent=2);stream.write('\n')
print(json.dumps(dict(status='NOT_READY_04A38',publication='LIVE_COPY_HASH_CRC_PRIVACY_MUTATIONS_PASS',preservation=protected['status'],tracked=protected['tracked'],index='EMPTY',diff_check_exit=0,W3='NOT_STARTED')))
