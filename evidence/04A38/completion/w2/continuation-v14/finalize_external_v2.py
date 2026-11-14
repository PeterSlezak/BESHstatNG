"""Final additive receipts, actual private replay and correctly bound final Git."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent))
from common import *
FLAGS='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED'
def external(name,value):
 path=W2/name
 with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False);f.write('\n')
 return pin(path)
privatepath=W2/'continuation-v14-retained-private-preservation-v1.json';private=json.loads(privatepath.read_bytes());reader=ROOT/private['reader']['path']
args=[sys.executable,'-I','-B',str(reader),'--verify'];start=time.monotonic();r=subprocess.run(args,cwd=ROOT,capture_output=True)
verified=external('continuation-v14-private-reader-verification-v1.json',dict(command=clean(args),exit_code=r.returncode,seconds=time.monotonic()-start,stdout=clean(r.stdout.decode()),stderr=clean(r.stderr.decode()),stdout_sha256=sha(r.stdout),stderr_sha256=sha(r.stderr),reader=pin(reader),receipt=pin(privatepath),original_queries=0));assert r.returncode==0
index=W2/'continuation-v14-review-parts-index-v1.json';parts=json.loads(index.read_bytes());publication=W2/'continuation-v14-publication-receipt-v1.json';copied=W2/'continuation-v14-copied-review-reader-v1.json';live=W2/'continuation-v14-live-preservation-v1.json';privacy=W2/'continuation-v14-publication-privacy-receipt-v1.json'
compiled=[]
for config in ['Debug','Release']:
 for role in ['lib','udf']:
  compiled.extend(dict(configuration=config,role=role,**pin(p)) for p in sorted((PRIVATE/'packet-v1'/config/role).glob('*')) if p.is_file() and p.suffix.lower() in ['.dll','.exe','.config'])
artifacts=external('continuation-v14-post-publication-artifacts-v1.json',dict(status='ADDITIVE_FINAL_AUTHORITY_NOT_READY',checkpoint_reader=pin(HERE/'verify_c14_checkpoint_v2.py'),checkpoint_seal=pin(HERE/'review-seal-v1.json'),publication_reader=pin(HERE/'verify_c14_publication_v2.py'),
 sealed_review=pin(HERE/'owner-review.md'),sealed_handover=pin(HERE/'NEXT_WAVE_HANDOVER.md'),parts_index=pin(index),publication=pin(publication),privacy_checkpoint=pin(privacy),copied_reader=pin(copied),live_preservation=pin(live),
 authoritative_private_reader=pin(reader),authoritative_private_preservation=pin(privatepath),private_reader_verification=verified,private_pin_count=private['private_pin_count'],physical_archive_count=len(private['retained_prior_physical_archives']),
 current_compiled_artifacts=compiled,source_after_images=[t['after'] for t in json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']],final_git_receipt='continuation-v14-final-git-preservation-v1.json',standalone='NOT_CREATED',original_queries=0))
summary=json.loads((HERE/'ownership-summary-v2.json').read_bytes());queue=json.loads((HERE/'remaining-work-queue-v1.json').read_bytes());validation=json.loads((HERE/'validation-independent-replay-v1.json').read_bytes())
lines=['# Continuation-v14 final post-seal authority and handover','',FLAGS,'',
 'Authority order: this final additive handover, final-git-preservation-v1.json and post-publication-artifacts-v1.json bind the unchanged sealed owner review/readers, exact part index, current private reader and actual verification streams. No owner acceptance, merge, W3 or04A39 launch. All earlier failed/provisional attempts remain immutable.','',
 'Actual readers: continuation-v14/verify_c14_checkpoint_v2.py; verify_c14_publication_v2.py; verify_c14_preservation_v2.py; external continuation-v14-retained-private-reader-v1.py. Copied-union and live/private verification receipts bind their exact sources and actual command streams. These are local source-bound Windows receipts. No v14 ZIP upload or independent external reviewer physical verification is claimed.','',
 f'Private pins {private["private_pin_count"]}: all11875 inherited plus{private["new_current_private_count"]} additions; physical archives {len(private["retained_prior_physical_archives"])}: all57 inherited plus new review parts and deterministic reproductions. Actual authoritative v13 reader-v2 replay and current --verify exit0. Protected v9 giant4213129746 bytes remains at SHA2563ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb. Historical TestResults never bulk-traversed/deleted.','',
 f'Logical inventory {len(parts["files"])} unique payloads in{len(parts["parts"])} independently valid ZIPs below180MiB. Every normalized path/SHA/size/CRC/part is indexed exactly once; deterministic physical reproductions match. No standalone ZIP or binary-volume equivalence claim.','',
 '| Part | Bytes | SHA256 |','|---|---:|---|']
lines+=['|'+p['name']+'|'+str(p['bytes'])+'|`'+p['sha256']+'`|' for p in parts['parts']]
lines+=['',f'Ownership and fragment census: {summary}. Full source/callback/property/generic/context contracts and exact NEW/RETAINED/OPEN704+726+303 ledgers are sealed. The four ROC bodies close through complete original-source grammar, preserved old Common contract, actual current-only alias tests and Debug/Release compiled consumers; no new original oracle. Native PSM Alpha constructor/setter/cache lifecycle remains Windows-owned.','',
 f'Actual managed/Python/Node/auxiliary outcomes: {validation}. Both Release full/selected unit suites NOT_RUN_OWNER_PREFERENCE. Debug/Release offline compilation,268 exact exports and29424 fresh frozen current-only comparisons/configuration (28966 genuine plus458 separately LATE_ARCHIVED_ENTRY_SUPPLEMENT) remain distinct from science/native validation.','',
 'The two historical privacy JSON ERRORs and v9 FAIL history, unordered-SPC ERROR, all old source-pin failures, v13 three changed messages and any v14 event/hash deltas remain unwaived in the sealed source-backed nonpass ledger. First/corrective focused compile, prerequisite launches, library-pin, source/generic/property/fragment/reader drafts and any publication attempts are retained. No historic assertion/scanner/protected artifact was modified.','',
 'Final exact next24 historical IDs are filtered only from the final authenticated queue; all remaining current/support IDs and fragments are explicit:','']+[str(i)+'. `'+r['original_id']+'`' for i,r in enumerate(queue['first_members'],1)]
lines+=['',FLAGS,'']
addendum=W2/'continuation-v14-postseal-handover-addendum-v1.md'
with addendum.open('x',encoding='utf-8',newline='\n') as f:f.write('\n'.join(lines))
prior=json.loads((PRIVATE/'exit-git-receipt-v1.json').read_bytes());checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-status']),('status',['status','--porcelain=v1']),('diff-check',['diff','--check'])]:
 argv=['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args];start=time.monotonic();r=subprocess.run(argv,cwd=ROOT,capture_output=True);assert r.returncode==0
 p=next(p for p in prior['checks'] if p['check']==name);assert sha(r.stdout)==p['raw_stdout']['sha256']
 assert '/w2c14-exit-git-' in p['raw_stdout']['path'] and '/w2c14-exit-git-' in p['raw_command']['path']
 if name=='branch':assert r.stdout.decode().strip()=='codex/04A-38'
 if name in ['HEAD','main','merge-base']:assert r.stdout.decode().strip()==BASE
 if name=='index':assert not r.stdout
 checks.append(dict(check=name,command=clean(argv),exit_code=r.returncode,seconds=time.monotonic()-start,stdout=r.stdout.decode(),stderr=clean(r.stderr.decode()),stdout_sha256=sha(r.stdout),stderr_sha256=sha(r.stderr),exact_raw_predecessor_command=p['raw_command'],exact_raw_predecessor_stdout=p['raw_stdout'],exact_stdout_equal=True))
indexfile=pin(ROOT/'.git/index');assert indexfile==prior['index_file'] and indexfile['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
for row in json.loads((HERE/'production-test-freeze-v1.json').read_bytes())['sources']:assert pin(ROOT/row['path'])==row
receipt=external('continuation-v14-final-git-preservation-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',Git=checks,index_file=indexfile,
 private_preservation=pin(privatepath),private_verification=verified,live_preservation=pin(live),publication=pin(publication),privacy_checkpoint=pin(privacy),parts_index=pin(index),copied_reader=pin(copied),post_publication_artifacts=artifacts,postseal_addendum=pin(addendum),postseal_tool=pin(Path(__file__).resolve()),original_queries=0))
print(json.dumps(dict(final_git=receipt,private_pins=private['private_pin_count'],archives=len(private['retained_prior_physical_archives']),parts=len(parts['parts']),files=len(parts['files']),status=FLAGS)))
