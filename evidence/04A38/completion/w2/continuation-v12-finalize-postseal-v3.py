"""Correct stale Git-log binding additively; preserve sealed source and first failure."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v12'))
from common import *

def external(name,doc):
    path=W2/name
    with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(doc,stream,ensure_ascii=False,indent=2);stream.write('\n')
    return pin(path)

privatepath=W2/'continuation-v12-retained-private-preservation-v1.json'
verificationpath=W2/'continuation-v12-private-reader-verification-v1.json'
private=json.loads(privatepath.read_bytes());verification=json.loads(verificationpath.read_bytes())
assert verification['exit_code']==0 and verification['receipt']==pin(privatepath)
assert verification['reader']==pin(W2/'continuation-v12-retained-private-reader-v1.py')
assert json.loads(verification['stdout'])['pins']==8801==private['private_pin_count']
pins={r['path']:r for r in private['private_pins']}
stale_path=PRIVATE/'exit-git-receipt-v1.json';stale=json.loads(stale_path.read_bytes())
assert pin(stale_path)==pins[stale_path.relative_to(ROOT).as_posix()]
corrected=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-status']),('status',['status','--porcelain=v1']),('diff-check',['diff','--check'])]:
    folder=RUNROOT/'commands'/('w2c12-exit-git-'+name+'-v1')
    records={key:pin(folder/key) for key in ['command.json','stdout.log','stderr.log']}
    assert all(row==pins[row['path']] for row in records.values())
    command_record=json.loads((folder/'command.json').read_bytes());assert command_record['exit_code']==0
    old=next(r for r in stale['checks'] if r['check']==name)
    corrected.append(dict(check=name,actual_current=records,incorrect_sealed_binding=old['raw_stdout'],
        current_stdout_sha256=records['stdout.log']['sha256'],stale_stdout_sha256=old['raw_stdout']['sha256'],
        bytes_differ=records['stdout.log']['sha256']!=old['raw_stdout']['sha256']))
assert [r['check'] for r in corrected if r['bytes_differ']]==['status']
correction=external('continuation-v12-exit-git-correction-v1.json',dict(status='STALE_V11_LOG_BINDING_REJECTED_ACTUAL_V12_RAW_AUTHORITY_RECONCILED_NOT_READY',
    incorrect_sealed_capture_source=pin(HERE/'capture_exit_git_v1.py'),incorrect_private_receipt=pin(stale_path),
    failed_finalizer=pin(W2/'continuation-v12-finalize-postseal-v2.py'),
    actual_failure='AssertionError at final() raw stdout SHA comparison, line59. The historical status stdout was bound instead of actual current v12 stdout. No final Git receipt was issued by that attempt.',
    earlier_exit_execution=pin(RUNROOT/'commands/w2c12-exit-git-preservation-v1/command.json'),
    reason='capture_exit_git_v1.py retains a w2c11-exit-git- prefix in its folder lookup, while command() actually runs and records w2c12-exit-git-. All seven actual current raw command/stdout/stderr files were already included in the8801 independently verified private pins. The old receipt/source and provisional external artifacts/addendum remain unchanged; only their claim to be current raw Git authority is superseded.',
    checks=corrected,private_verification=pin(verificationpath),current_tool=pin(Path(__file__).resolve()),
    private_pin_population_changed=False,tests_rerun=0,production_edits=0,original_queries=0))
old_artifacts_path=W2/'continuation-v12-post-publication-artifacts-v1.json';artifacts=json.loads(old_artifacts_path.read_bytes())
for key in ['checkpoint_reader','publication_reader','checkpoint_seal','sealed_review','sealed_handover','publication','privacy_checkpoint','parts_index','copied_reader','authoritative_private_reader','authoritative_private_preservation','live_preservation','unsupported_closure_control','accounting_correction']:
    assert pin(ROOT/artifacts[key]['path'])==artifacts[key]
for row in artifacts['source_after_images']:assert pin(ROOT/row['path'])==row
artifacts.update(status='C12_FINAL_SOURCE_COMPILED_AND_CORRECTED_GIT_AUTHORITY_NOT_READY',previous_provisional_artifacts=pin(old_artifacts_path),
    exit_git_correction=correction,finalization_tool=pin(Path(__file__).resolve()),final_git_receipt='continuation-v12-final-git-preservation-v1.json')
final_artifacts=external('continuation-v12-post-publication-artifacts-v2.json',artifacts)
old_addendum=W2/'continuation-v12-postseal-handover-addendum-v1.md'
addendum=W2/'continuation-v12-postseal-handover-addendum-v2.md'
intro=['# W2 continuation-v12 final external authority correction','',
    'This v2 addendum and post-publication-artifacts-v2 supersede v1 only for final Git/provenance authority. The sealed review, six ZIPs, source/proof/test outcomes and8801-pin private preservation remain unchanged. The copied reader and private replay both actually exited0. No historical error/failure becomes PASS.','',
    'The first post-seal finalizer failed closed on a raw Git stdout comparison. Its sealed capture source ran actual v12 commands but selected old v11 log paths when forming the private exit receipt. All actual v12 logs already existed, were private-pinned and independently verified. continuation-v12-exit-git-correction-v1.json records both bindings, the exact status hash mismatch and the preserved first attempt. This is an evidence binding correction, not a change to Git or source.','',
    'Authoritative final receipts: continuation-v12-post-publication-artifacts-v2.json; continuation-v12-final-git-preservation-v1.json; continuation-v12-private-reader-verification-v1.json; continuation-v12-review-parts-index-v1.json. The finalizer uses the actual w2c12 raw receipts and independently repeats branch/HEAD/main/merge-base/index/status/diff checks. No private file was modified or added after the8801-pin freeze; later external receipts are additively pinned by the final Git receipt.','',
    'The earlier accounting rejection and its source-backed successor, additional OPEN-body control wrong-schema attempt and correction, and shell-launch diagnostics are all retained. No unit suite, Python parent, Node suite, compatibility corpus or publication privacy campaign was rerun for these corrections.','',
    'The following retained v1 handover content is correct for implementation, validation, private preservation and package facts; references there to v1 post-publication/finalization authority are superseded above.','',old_addendum.read_text(encoding='utf-8')]
with addendum.open('x',encoding='utf-8',newline='\n') as stream:stream.write('\n'.join(intro))
checks=[]
for entry in corrected:
    name=entry['check'];record=json.loads((ROOT/entry['actual_current']['command.json']['path']).read_bytes())
    argv=record['command'];begin=time.monotonic();result=subprocess.run(argv,cwd=ROOT,capture_output=True);assert result.returncode==0
    assert sha(result.stdout)==entry['current_stdout_sha256']
    value=result.stdout.decode().strip()
    if name=='branch':assert value=='codex/04A-38'
    if name in ['HEAD','main','merge-base']:assert value==BASE
    if name in ['index','diff-check']:assert not result.stdout
    checks.append(dict(check=name,command=clean(argv),exit_code=result.returncode,seconds=time.monotonic()-begin,
        stdout=result.stdout.decode(),stderr=clean(result.stderr.decode()),stdout_sha256=sha(result.stdout),stderr_sha256=sha(result.stderr),
        actual_current_raw_command=entry['actual_current']['command.json'],actual_current_raw_stdout=entry['actual_current']['stdout.log'],
        actual_current_raw_stderr=entry['actual_current']['stderr.log'],exact_actual_v12_stdout_equal=True))
index=pin(ROOT/'.git/index');assert index['sha256']==stale['index_file']['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
for row in json.loads((HERE/'production-test-freeze-v1.json').read_bytes())['sources']:assert pin(ROOT/row['path'])==row
receipt=external('continuation-v12-final-git-preservation-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',
    Git=checks,index_file=index,private_preservation=pin(privatepath),private_verification=pin(verificationpath),
    live_preservation=artifacts['live_preservation'],publication=artifacts['publication'],privacy_checkpoint=artifacts['privacy_checkpoint'],parts_index=artifacts['parts_index'],copied_reader=artifacts['copied_reader'],
    post_publication_artifacts=final_artifacts,postseal_addendum=pin(addendum),postseal_tool=pin(Path(__file__).resolve()),
    superseded_binding=correction,previous_provisional_artifacts=pin(old_artifacts_path),previous_provisional_addendum=pin(old_addendum),original_queries=0))
print(json.dumps(dict(final_git_receipt=receipt,parts=6,files=8015,private_pins=8801,status='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED')))
