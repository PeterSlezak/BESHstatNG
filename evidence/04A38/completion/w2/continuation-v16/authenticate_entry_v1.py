"""Actual immutable v15 entry gates, raw provenance, append-only failure receipt."""
from common import *
PRIVATE.mkdir(exist_ok=False)
rawroot=PRIVATE/'entry-commands-v1';rawroot.mkdir()
executions=[];authorities={};identities={}
def check(row):
 actual=pin(ROOT/row['path']);assert actual=={k:row[k] for k in ['path','bytes','sha256']},'ENTRY_PIN_MISMATCH_STOP '+row['path']
 authorities[actual['path']]=actual
 stat=(ROOT/actual['path']).stat();identities[actual['path']]=dict(device=stat.st_dev,file_id=stat.st_ino,bytes=stat.st_size)
 return actual
def read(path):
 p=pin(path);check(p);return json.loads(path.read_bytes())
def execute(label,args,cwd=ROOT):
 folder=rawroot/label;folder.mkdir();start=time.monotonic()
 with (folder/'stdout.log').open('xb') as out,(folder/'stderr.log').open('xb') as err:result=subprocess.run(args,cwd=cwd,stdout=out,stderr=err)
 record=dict(command=args,cwd=str(cwd),exit_code=result.returncode,seconds=time.monotonic()-start)
 with (folder/'command.json').open('x',encoding='utf-8') as f:json.dump(record,f,indent=2)
 executions.append(dict(label=label,receipt=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),result=clean(record)))
 print(label,result.returncode,round(record['seconds'],2),flush=True)
 assert result.returncode==0,'ENTRY_READER_OR_GIT_FAILED_STOP '+label
 return (folder/'stdout.log').read_bytes()
try:
 expected={
  'continuation-v15-postseal-handover-addendum-v1.md':'b0582c3a56820d81fd189c350d09caa18dfc930fab13041496dd3c3f45b0dbf7',
  'continuation-v15/owner-review.md':'9b246e08b57b7aa1f082911a00b8845c4dfe8d933f9621243befa144900585ee',
  'continuation-v15/remaining-work-queue-v2.json':'5da815030e7c1eb1dca227101233ce016ff66339b4a5dffc8a88478dabc4db63',
 }
 for name,digest in expected.items():
  row=pin(W2/name);assert row['sha256']==digest,'OWNER_TRANSPORT_HASH_MISMATCH_STOP '+name;check(row)
 for name in ['review-seal-v1.json','historical-ownership-v2.json','current-ownership-v3.json','source-transitions-final-v1.json','production-test-freeze-v3.json','new-closures-final-v1.json','canonical-identity-status-v1.json','fragment-reconciliation-receipt-v1.json','whole-body-contracts-v1.json','callback-proofs-v1.json','compatibility-packet-v3.json']:
  read(PREVIOUS/name)
 art=read(W2/'continuation-v15-post-publication-artifacts-v1.json')
 for row in art['artifacts']:check(row)
 for name in ['continuation-v15-final-verification-v1.json','continuation-v15-final-git-preservation-v1.json','continuation-v15-review-parts-index-v2.json','continuation-v15-publication-receipt-v2.json','continuation-v15-retained-private-preservation-v2.json','continuation-v15-postseal-private-addendum-v2.json','continuation-v15-copied-review-reader-v3.json']:
  read(W2/name)
 finalgit=read(W2/'continuation-v15-final-git-preservation-v1.json');check(finalgit['index'])
 assert finalgit['index']['bytes']==620185 and finalgit['index']['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
 plan=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md');assert plan['sha256']=='0ee45eae70aa35d6c87c636d82c3f33e105b8c12e6e5a4badb69e38fe7ad69dd';check(plan)
 for row in read(PREVIOUS/'production-test-freeze-v3.json')['sources']:check(row)
 post=read(W2/'continuation-v15-postseal-private-addendum-v2.json');assert len(post['postseal_private_pins'])==post['count']==31
 for row in post['postseal_private_pins']:check(row)
 for label,args in [('branch',['branch','--show-current']),('anchors',['rev-parse','HEAD','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('diff-check',['diff','--check']),('status',['status','--short'])]:
  output=execute('git-'+label,['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args])
  if label=='branch':assert output.strip()==b'codex/04A-38'
  if label=='anchors':assert output.decode().splitlines()==[BASE,BASE]
  if label=='merge-base':assert output.decode().strip()==BASE
  if label=='index':assert not output
 # This actual final publication reader invokes the sealed checkpoint reader and
 # predecessor checkpoint graph, verifies every physical member, then invokes
 # the actual final live preservation reader. It does not run any test suite.
 publication=PREVIOUS/'verify_c15_publication_v3.py';check(pin(publication))
 result=execute('c15-publication-checkpoint-physical-live-v3',bootstrap(publication,'--index',W2/'continuation-v15-review-parts-index-v2.json','--parts'))
 actual=json.loads(result);assert actual['files']==9740 and actual['parts']==8 and actual['checkpoint']['historical_supported']==537
 assert actual['preservation']['Git']=='LIVE_PASS'
 private=W2/'continuation-v15-retained-private-reader-v3.py';check(pin(private))
 result=execute('c15-private-v3',bootstrap(private,'--verify'));checked=json.loads(result)
 assert checked['pins']==21208 and checked['archives']==101
 # The final copied-reader source and prior execution are authenticated above;
 # replay that same final reader in the already verified .git-free C15 copy.
 copyroot=RUNROOT/'r3';copyreader=copyroot/publication.relative_to(ROOT)
 assert copyreader.is_file() and not (copyroot/'.git').exists()
 with copyreader.open('rb') as f:assert hashlib.file_digest(f,'sha256').hexdigest()==pin(publication)['sha256']
 result=execute('c15-copied-final-reader-v3',bootstrap(copyreader,'--index',W2/'continuation-v15-review-parts-index-v2.json'),cwd=copyroot)
 assert json.loads(result)['files']==9740
 # Private predecessor receipts are real retained executions, each byte-bound
 # by the actual full inherited21208-pin replay above; no historical replay is
 # invented or attributed to the current configuration.
 entry=read(PREVIOUS/'entry-authentication-v2.json')
 for label in ['c14-private-v2','c13-private-v2']:
  row=next(r for r in entry['executions'] if r['label']==label);assert row['exit_code']==0
  for key in ['reader','receipt','stdout','stderr']:check(row[key])
 check(finalgit['index']);check(plan)
 save('entry-authentication-v1.json',dict(status='ENTRY_GATES_PASS',authorities=list(authorities.values()),filesystem_identities=identities,executions=executions,implementation_edits=0,original_queries=0,historical_supported=537,current_supported=566,support_supported=262,protected_private=21208,protected_archives=101,postseal_evidence=31,historical_release_scope='AUTHENTICATE_IMMUTABLE_V15_RESULTS_ONLY;NOT_V16_RELEASE_EXECUTION'))
 save('owner-debug-only-override-v1.json',dict(status='EXPLICIT_OWNER_V16_INTERMEDIATE_CHECKPOINT_OVERRIDE',owner_authorization_date='2026-11-13',selected_historical_OPEN=58,packet_A_positions=[1,37],packet_B_positions=[38,58],comprehensive_Debug_campaigns_authorized=1,Python_parent_campaigns_authorized=1,Python_process_ceiling=8,Node_campaigns_authorized=1,Debug_frozen_cases=29424,Release_compilation=DEFERRED,Release_full_selected_focused_unit_tests=DEFERRED,Release_ABI_CIL_signatures_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,finalization='Fresh Debug and Release gates required before separately authorized final acceptance; W3_NOT_STARTED;04A39_BLOCKED',historical_assertions='UNCHANGED; exact authenticated predecessor projection only',original_queries=0))
 print('C16 entry authentication PASS; implementation may begin',flush=True)
except Exception as e:
 save('entry-authentication-v1.json',dict(status='ENTRY_AUTHENTICATION_FAILED_STOP',error=type(e).__name__+': '+str(e),authorities=list(authorities.values()),filesystem_identities=identities,executions=executions,implementation_edits=0,original_queries=0))
 print(type(e).__name__+': '+str(e),flush=True);raise SystemExit(1)
