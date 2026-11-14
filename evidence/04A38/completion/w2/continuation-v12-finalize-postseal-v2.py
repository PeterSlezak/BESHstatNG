"""Final external additive authorities; never overwrite a sealed payload."""
from common import *
def external(name,doc):
    path=W2/name
    with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(doc,stream,ensure_ascii=False,indent=2);stream.write('\n')
    return pin(path)
def preserve():
    result=command('final-live-preservation-v1',[sys.executable,'-B',str(HERE/'verify_c12_preservation_v1.py')]);assert result['exit_code']==0
    folder=RUNROOT/'commands/w2c12-final-live-preservation-v1';proof=json.loads((folder/'stdout.log').read_bytes())
    assert proof['Git']=='LIVE_PASS' and proof['owner_documents_checked']==16 and proof['lineage_pins']==278 and proof['protected_captures']==100 and proof['original_snapshots']==41
    receipt=external('continuation-v12-live-preservation-v1.json',dict(status='C12_LIVE_ORIGINAL_OWNER_TRACKED_SOURCE_PRESERVATION_PASS_NOT_READY',reader=pin(HERE/'verify_c12_preservation_v1.py'),result=proof,
        execution=clean(result),raw_command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),original_queries=0))
    print(json.dumps(dict(live_preservation=receipt)))
def final():
    privatepath=W2/'continuation-v12-retained-private-preservation-v1.json';private=json.loads(privatepath.read_bytes());reader=W2/'continuation-v12-retained-private-reader-v1.py'
    args=[sys.executable,'-B',str(reader),'--verify'];begin=time.monotonic();result=subprocess.run(args,cwd=ROOT,capture_output=True)
    verification=external('continuation-v12-private-reader-verification-v1.json',dict(command=clean(args),cwd='<REPO_ROOT>',exit_code=result.returncode,seconds=time.monotonic()-begin,
        stdout=clean(result.stdout.decode()),stderr=clean(result.stderr.decode()),stdout_sha256=sha(result.stdout),stderr_sha256=sha(result.stderr),reader=pin(reader),receipt=pin(privatepath),original_queries=0))
    assert result.returncode==0
    index=W2/'continuation-v12-review-parts-index-v1.json';parts=json.loads(index.read_bytes())
    publication=W2/'continuation-v12-publication-receipt-v1.json';privacy=W2/'continuation-v12-publication-privacy-receipt-v1.json'
    copied=W2/'continuation-v12-copied-review-reader-v1.json';live=W2/'continuation-v12-live-preservation-v1.json'
    compiled=[]
    for config in ['Debug','Release']:
        for role in ['lib','udf']:
            for path in sorted((PRIVATE/'packet-v1'/config/role).glob('*')):
                if path.is_file() and path.suffix.lower() in ['.dll','.exe','.config']:compiled.append(dict(configuration=config,role=role,**pin(path)))
    artifacts=external('continuation-v12-post-publication-artifacts-v1.json',dict(status='C12_ADDITIVE_FINAL_SOURCE_COMPILED_AUTHORITY_NOT_READY',
        checkpoint_reader=pin(HERE/'verify_c12_checkpoint_v2.py'),publication_reader=pin(HERE/'verify_c12_publication_v1.py'),checkpoint_seal=pin(HERE/'review-seal-v1.json'),
        sealed_review=pin(HERE/'owner-review.md'),sealed_handover=pin(HERE/'NEXT_WAVE_HANDOVER.md'),
        unsupported_closure_control=pin(W2/'continuation-v12-unsupported-closure-control-v1.json'),accounting_correction=pin(HERE/'postscan-accounting-correction-v1.json'),prior_finalizer_source=pin(HERE/'finalize_live_v1.py'),
        publication=pin(publication),privacy_checkpoint=pin(privacy),parts_index=pin(index),copied_reader=pin(copied),
        authoritative_private_reader=pin(reader),authoritative_private_preservation=pin(privatepath),private_reader_verification=verification,
        private_pin_count=private['private_pin_count'],current_synthetic_phase=pin(HERE/'private-proof-transition-v1.json'),
        source_after_images=[r['after'] for r in json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']],
        current_compiled_artifacts=compiled,live_preservation=pin(live),final_git_receipt='continuation-v12-final-git-preservation-v1.json',standalone='NOT_CREATED',original_queries=0))
    validation=json.loads((HERE/'validation-v1.json').read_bytes());flags='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED'
    lines=['# W2 continuation-v12 external post-seal handover addendum','',flags,'',
        'An actual DA_MEANS OPEN-body false closure was rejected by the sealed reader; continuation-v12-unsupported-closure-control-v1.json retains the first wrong-schema attempt and exact ownership-ledger correction. The first accounting rejection and additive v2 source-backed correction are retained in continuation-v12/postscan-accounting-correction-v1.json. No campaign or test was rerun for either bookkeeping correction.','',
        'The sealed owner review, source/proof evidence, earlier history and manifest remain unchanged. These final external receipts are additive authorities, not owner acceptance, W3, scientific/native validation or a merge.','',
        'Authoritative readers: continuation-v12/verify_c12_checkpoint_v2.py and verify_c12_publication_v1.py, sealed by review-seal-v1.json. The actual independent copied reader result is in continuation-v12-copied-review-reader-v1.json; earlier outcomes are never inferred or substituted.','',
        f'One logical inventory: **{len(parts["files"])} exact payloads** in **{len(parts["parts"])} independently valid complementary ACTIVE/REFERENCE ZIPs**, each below180MiB. Every byte SHA/size/CRC and unique part assignment is in continuation-v12-review-parts-index-v1.json. Every part has an independent deterministic physical second serialization matching its exact SHA/size. No C12 standalone ZIP exists; the parts do not claim to reproduce standalone physical bytes.','',
        '| Part | Bytes | SHA256 |','|---|---:|---|']
    lines+=['|'+part['name']+'|'+str(part['bytes'])+'|`'+part['sha256']+'`|' for part in parts['parts']]
    lines+=['',
        f'Private authority: continuation-v12-retained-private-reader-v1.py and continuation-v12-retained-private-preservation-v1.json contain **{private["private_pin_count"]} exact pins**, all5661 inherited plus{private["new_current_private_count"]} relevant new files, and{len(private["retained_prior_physical_archives"])} retained physical archives. Actual v11 reader-v4 replay and current independent --verify exit0 are recorded. The unchanged4213129746-byte malformed v9 JSON remains at SHA2563ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb. Historical ignored TestResults was never traversed/deleted.','',
        'The current stale-cache control is a new synthetic fixture. Its actual17-byte preimage was copied before deliberate mutation; before/after plans, bytes and source are in private-proof-transition-v1.json and verified by the private reader. It is neither original statistical evidence nor a pre-edit production capture. All old synthetic recovery classifications and failed/provisional histories remain unchanged.','',
        'Live preservation independently verifies16 owner documents,278 lineage pins,100 mixed-model captures,41 original UDF snapshots,4695 tracked paths and the exact full source transition graph. Required AGENTS/master-plan context is present in the logical review;15 optional owner README documents remain omitted only from review derivatives and verified live.','',
        'Ownership:435/704 historical supported (269 OPEN),465/726 current supported (261 OPEN),252/303 distinct support (51 OPEN). Original fragments10457/20683; current6086/14732. All33 new historical/current bodies, seven separate callbacks, two support declarations, retained identities and every OPEN complement are listed in the sealed ledgers. Native module/container and unreviewed dependencies remain OPEN.','',
        f'Debug20834 PASS/14 inherited FAIL, including31 new PASS and exact prior20817 identity/outcome/message preservation. Python parents: {validation["Python"]["methods"]}; nonpass events: {validation["Python"]["nonpass_events"]}. Node916 PASS; strict/profile/MkDocs exits1/1/0. Both historical privacy JSON ERRORs, earlier v9 FAIL histories, source-pin FAILs and unordered-SPC ERROR remain unwaived; exact deltas are source-backed in the sealed nonpass ledger.','',
        'Full and selected Release unit suites remain NOT_RUN_OWNER_PREFERENCE. Debug/Release shipping compilation,268 exact exports and29424 fresh current-only comparisons/configuration passed:28966 genuine pre-edit plus458 separately approved LATE_ARCHIVED_ENTRY_SUPPLEMENT cases. No supplemental original/reference capture or independent scientific truth is asserted.','',
        'The complete expensive historical Python scan ran once on its frozen candidate set. Selected review policy ran once at the publication checkpoint; later artifact/copy readers rehash every payload and authenticate exact policy/content decisions, with a separate small manifest-self check. Review-only exclusions never alter original scanner candidates or turn their errors into PASS.','',
        'continuation-v12-post-publication-artifacts-v1.json pins final source/compiled/reader/external authority. continuation-v12-final-git-preservation-v1.json is the last actual live branch/HEAD/main/merge-base/index/diff receipt. Nothing was staged, committed, merged, rebased, reset or pushed. No original model execution/recompile/query, R/scientific oracle, native Excel/COM/UIA/RefEdit/Office/Mac/manual run, restore/install, XLL packaging, signing, deployment, W3 or04A39 work occurred.','']
    addendum=W2/'continuation-v12-postseal-handover-addendum-v1.md'
    with addendum.open('x',encoding='utf-8',newline='\n') as stream:stream.write('\n'.join(lines))
    prior=json.loads((PRIVATE/'exit-git-receipt-v1.json').read_bytes());checks=[]
    for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-status']),('status',['status','--porcelain=v1']),('diff-check',['diff','--check'])]:
        argv=['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args];begin=time.monotonic();result=subprocess.run(argv,cwd=ROOT,capture_output=True);assert result.returncode==0
        previous=next(r for r in prior['checks'] if r['check']==name);assert sha(result.stdout)==previous['raw_stdout']['sha256']
        if name=='branch':assert result.stdout.decode().strip()=='codex/04A-38'
        if name in ['HEAD','main','merge-base']:assert result.stdout.decode().strip()==BASE
        if name=='index':assert not result.stdout
        checks.append(dict(check=name,command=clean(argv),exit_code=result.returncode,seconds=time.monotonic()-begin,stdout=result.stdout.decode(),stderr=clean(result.stderr.decode()),stdout_sha256=sha(result.stdout),stderr_sha256=sha(result.stderr),exact_raw_predecessor_command=previous['raw_command'],exact_raw_predecessor_stdout=previous['raw_stdout'],exact_stdout_equal=True))
    index_file=pin(ROOT/'.git/index');assert index_file['sha256']==prior['index_file']['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
    for row in json.loads((HERE/'production-test-freeze-v1.json').read_bytes())['sources']:assert pin(ROOT/row['path'])==row
    receipt=external('continuation-v12-final-git-preservation-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',
        Git=checks,index_file=index_file,private_preservation=pin(privatepath),private_verification=verification,live_preservation=pin(live),publication=pin(publication),privacy_checkpoint=pin(privacy),parts_index=pin(index),copied_reader=pin(copied),post_publication_artifacts=artifacts,postseal_addendum=pin(addendum),postseal_tool=pin(Path(__file__)),original_queries=0))
    print(json.dumps(dict(final_git_receipt=receipt,parts=len(parts['parts']),files=len(parts['files']),private_pins=private['private_pin_count'],status=flags)))
if __name__=='__main__':{'preserve':preserve,'final':final}[sys.argv[1]]()
