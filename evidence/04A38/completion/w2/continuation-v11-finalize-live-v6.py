import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v11'))
"""Additive live receipts after selected publication; no sealed payload is changed."""
from common import *

def external(name,doc):
    path=W2/name
    with path.open('x',encoding='utf-8',newline='\n') as stream:
        json.dump(doc,stream,ensure_ascii=False,indent=2,allow_nan=False);stream.write('\n')
    return pin(path)

def preserve():
    result=command('final-live-preservation-v1',[sys.executable,'-B',str(HERE/'verify_c11_preservation_v1.py')])
    assert result['exit_code']==0
    folder=RUNROOT/'commands/w2c11-final-live-preservation-v1'
    proof=json.loads((folder/'stdout.log').read_bytes())
    assert proof['Git']=='LIVE_PASS' and proof['owner_documents_checked']==16
    assert proof['lineage_pins']==278 and proof['protected_captures']==100 and proof['original_snapshots']==41
    receipt=external('continuation-v11-live-preservation-v1.json',dict(status='LIVE_ORIGINAL_OWNER_TRACKED_SOURCE_TRANSITION_PRESERVATION_PASS_NOT_READY',
        reader=pin(HERE/'verify_c11_preservation_v1.py'),result=proof,execution=clean(result),
        raw_command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),original_queries=0))
    print(json.dumps(dict(live_preservation=receipt)),flush=True)

def final():
    privatepath=W2/'continuation-v11-retained-private-preservation-v4.json'
    private=json.loads(privatepath.read_bytes());reader=W2/'continuation-v11-retained-private-reader-v4.py'
    begin=time.monotonic();args=[sys.executable,'-B',str(reader),'--verify']
    verification=subprocess.run(args,cwd=ROOT,capture_output=True)
    verification_receipt=external('continuation-v11-private-reader-verification-v1.json',dict(command=clean(args),
        cwd='<REPO_ROOT>',exit_code=verification.returncode,seconds=time.monotonic()-begin,
        stdout=clean(verification.stdout.decode()),stderr=clean(verification.stderr.decode()),
        stdout_sha256=sha(verification.stdout),stderr_sha256=sha(verification.stderr),
        reader=pin(reader),receipt=pin(privatepath),original_queries=0))
    assert verification.returncode==0
    parts_path=W2/'continuation-v11-review-parts-index-v3.json';parts=json.loads(parts_path.read_bytes())
    publication=W2/'continuation-v11-publication-receipt-v3.json'
    copied=W2/'continuation-v11-copied-review-reader-v3.json'
    live=W2/'continuation-v11-live-preservation-v1.json'
    compiled=[]
    for config in ['Debug','Release']:
        for folder in ['lib','udf']:
            for path in sorted((PRIVATE/'packet-v2'/config/folder).glob('*')):
                if path.is_file() and path.suffix.lower() in ['.dll','.exe','.config']:compiled.append(dict(configuration=config,role=folder,**pin(path)))
    artifacts=external('continuation-v11-post-publication-artifacts-v1.json',dict(status='ADDITIVE_FINAL_SOURCE_COMPILED_EXTERNAL_AUTHORITY_INDEX_NOT_READY',
        checkpoint_reader=pin(HERE/'verify_c11_checkpoint_v2.py'),publication_reader=pin(W2/'continuation-v11-publication-reader-v4.py'),sealed_publication_reader=pin(HERE/'verify_c11_publication_v2.py'),complete_manifest=pin(W2/'continuation-v11-logical-publication-manifest-v3.json'),copy_failure_reconciliation=pin(W2/'continuation-v11-copy-failure-reconciliation-v2.json'),publication_supplement_addendum=pin(W2/'continuation-v11-publication-supplement-addendum-v2.md'),checkpoint_seal=pin(HERE/'review-seal-v2.json'),reader_authority=pin(HERE/'final-reader-authority-v1.json'),owner_reader_addendum=pin(HERE/'owner-review-reader-addendum-v1.md'),sealed_review=pin(HERE/'owner-review.md'),sealed_handover=pin(HERE/'NEXT_WAVE_HANDOVER.md'),
        publication=pin(publication),parts_index=pin(parts_path),copied_reader=pin(copied),
        authoritative_private_reader=pin(reader),authoritative_private_preservation=pin(privatepath),current_private_phase=pin(W2/'continuation-v11-private-proof-transition-v1.json'),
        private_reader_verification=verification_receipt,private_pin_count=private['private_pin_count'],
        source_after_images=[r['after'] for r in json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']],
        current_compiled_artifacts=compiled,live_preservation=pin(live),
        final_git_receipt='continuation-v11-final-git-preservation-v1.json',standalone='NOT_CREATED',original_queries=0))
    flags='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED'
    lines=['# W2 continuation-v11 external post-seal handover addendum','',flags,'',
        'The sealed source/evidence/owner review/handover and manifest are unchanged. These are final external additive authorities, not W3, scientific/native acceptance or a merge.','',
        'Authoritative complete publication reader is continuation-v11-publication-reader-v4.py; it verifies the additive union and replays unchanged verify_c11_publication_v2.py / verify_c11_checkpoint_v2.py under review-seal-v2.json. The first copied default reader failed on omitted entry context; the second copied wrapper failed before semantic replay because common unnecessarily imported the repository command runner. Both failed copies and actual exit1 traces remain. Reader-v4 replaces that wrapper import with standard-library hashing; its actual exit1 and unchanged five ZIPs / first copy remain. The versioned sixth ACTIVE supplement supplies exactly three pinned required entry documents plus executable union authorities; optional excluded owner README documents remain omitted. The first supplement preparer failed by counting optional owner reads as required; its exact failure was independently reproduced. The second failed because one required master plan is also an owner document. Both sources and raw execution receipts remain, and the third preparer distinguishes the15 optional README documents from the three required entry documents. The required protected master plan is included byte-exact, superseding the sealed general statement that all16 owner documents were omitted from the initial five parts. The initial shell correction failed to parse before writes; the additive Python correction is retained. No production/test source or historical assertion changed for these publication corrections. The failed sealed v1 and rejected first diagnosis remain unchanged; owner-review-reader-addendum-v1.md and final-reader-authority-v1.json identify the exact module-isolated successor and actual positive/control receipts. No source/test or historical assertion changed to correct that reader-only collision.','',
        f'One logical inventory: **{len(parts["files"])} exact payloads**, one copy each, every file byte SHA/length/CRC and ACTIVE/REFERENCE assignment in continuation-v11-review-parts-index-v3.json. Each physical ZIP is below180MiB and has an actual deterministic second serialization with identical physical SHA/length. No C11 standalone ZIP exists; split parts do not claim to reproduce any standalone physical bytes.','',
        '| Part | Bytes | SHA256 |','|---|---:|---|']
    lines += ['|'+p['name']+'|'+str(p['bytes'])+'|`'+p['sha256']+'`|' for p in parts['parts']]
    lines += ['',
        f'Final private authority: continuation-v11-retained-private-reader-v4.py and continuation-v11-retained-private-preservation-v4.json, **{private["private_pin_count"]} exact pins**, retaining every2545 v10 explicit pin plus{private["new_current_private_count"]} relevant current private files. All enumerated prior/current physical archives and the unchanged4213129746-byte failed v9 aggregation remain verified. The successor reader was independently executed with --verify, exit0; its exact stdout/stderr hashes and duration are in continuation-v11-private-reader-verification-v1.json. Historical root TestResults was neither traversed nor deleted.','',
        'One current synthetic stale-cache fixture was deliberately mutated. Its exact17-byte before state is recovered from its authenticated generator and independently observed before-hash, honestly labelled SYNTHETIC_RECOVERY rather than pre-edit or original statistical capture. The before/after plans, current bytes and recovered bytes remain; continuation-v11-private-proof-transition-v1.json and the authoritative private reader-v2 validate this phase. The earlier private reader-v1/v2/v3 sources are retained drafts with no claimed result; v3 selects the complete seven-part index.','',
        'continuation-v11-live-preservation-v1.json independently verifies all16 protected owner docs,278 lineage pins,100 mixed-model captures,41 original UDF snapshots,4695 tracked paths and the exact source predecessor/successor graph. continuation-v11-copied-review-reader-v3.json verifies the complete physical logical union and executable default reader in a separately extracted copy. continuation-v11-post-publication-artifacts-v1.json pins source/compiled/external after-images. continuation-v11-final-git-preservation-v1.json is the last actual live branch/HEAD/main/merge-base/index/diff receipt. Read all alongside the sealed review.','',
        'Ownership remains402/704 historical supported (302 OPEN),432/726 current supported (294 OPEN),250/303 distinct support (53 OPEN). Debug20803 PASS/14 inherited FAIL, Python830 PASS/113 FAIL/13 ERROR parents and180 FAIL/13 ERROR events, Node916 PASS, strict/profile/MkDocs1/1/0. One current source-pin message changed to the authorized Multi source after-hash; no event identity/outcome was added/lost. Both privacy ERRORs and v9 FAIL histories remain unwaived. Full and selected Release units remain NOT_RUN_OWNER_PREFERENCE; both shipping configurations compiled,268 exports matched and29424 fresh current-only saved comparisons/config matched.','',
        'No staging, commit, merge, rebase, reset, cleaning, push, framework change, original-model query/capture/execution, R/scientific oracle, native/manual Excel/COM/RefEdit/UIA/Office/Mac, restore/install, XLL packaging, signing, deployment, W3 or04A39 work occurred. One approved offline Debug compilation retry accessed the existing SDK sentinel after sandbox denial; no package install/restore was involved.','']
    addendum=W2/'continuation-v11-postseal-handover-addendum-v1.md'
    with addendum.open('x',encoding='utf-8',newline='\n') as stream:stream.write('\n'.join(lines))
    prior=json.loads((PRIVATE/'exit-git-receipt-v1.json').read_bytes());checks=[]
    for name,gitargs in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),
        ('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-status']),
        ('status',['status','--porcelain=v1']),('diff-check',['diff','--check'])]:
        argv=['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*gitargs]
        begin=time.monotonic();result=subprocess.run(argv,cwd=ROOT,capture_output=True)
        assert result.returncode==0
        if name=='branch':assert result.stdout.decode().strip()=='codex/04A-38'
        if name in ['HEAD','main','merge-base']:assert result.stdout.decode().strip()==BASE
        if name=='index':assert not result.stdout
        previous=next(r for r in prior['checks'] if r['check']==name)
        assert sha(result.stdout)==previous['raw_stdout']['sha256'],'Post-publication Git stdout changed'
        checks.append(dict(check=name,command=clean(argv),exit_code=result.returncode,seconds=time.monotonic()-begin,
            stdout=result.stdout.decode(),stderr=clean(result.stderr.decode()),stdout_sha256=sha(result.stdout),stderr_sha256=sha(result.stderr),
            exact_raw_predecessor_command=previous['raw_command'],exact_raw_predecessor_stdout=previous['raw_stdout'],exact_stdout_equal=True))
    index=pin(ROOT/'.git/index');assert index['sha256']==prior['index_file']['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
    for row in json.loads((HERE/'production-test-freeze-v1.json').read_bytes())['sources']:assert pin(ROOT/row['path'])==row
    receipt=external('continuation-v11-final-git-preservation-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',
        P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',
        Git=checks,index_file=index,private_preservation=pin(privatepath),private_verification=verification_receipt,
        live_preservation=pin(live),publication=pin(publication),parts_index=pin(parts_path),copied_reader=pin(copied),
        post_publication_artifacts=artifacts,postseal_addendum=pin(addendum),postseal_tool=pin(Path(__file__)),additive_tools=[pin(W2/name) for name in ['continuation-v11-prepare-private-successor-v2.py','continuation-v11-private-successor-source-crosswalk-v2.json','continuation-v11-adapt-final-authorities-v4.py','continuation-v11-final-chronology-correction-v5.py','continuation-v11-finalize-live-v3.py','continuation-v11-finalize-live-v4.py','continuation-v11-retained-private-reader-v1.py','continuation-v11-retained-private-reader-v2.py','continuation-v11-retained-private-reader-v3.py','continuation-v11-finalize-live-v5.py','continuation-v11-complete-publication-v4.py']],
        original_queries=0,scope='Actual final live checkout and exact source/private/physical preservation. No authoritative history or sealed payload rewritten.'))
    print(json.dumps(dict(final_git_receipt=receipt,parts=len(parts['parts']),files=len(parts['files']),private_pins=private['private_pin_count'],status=flags)),flush=True)

if __name__=='__main__':{'preserve':preserve,'final':final}[sys.argv[1]]()
