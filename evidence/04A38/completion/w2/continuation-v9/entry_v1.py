"""Live C8 authentication before C9 implementation, without suites or ignored walks."""
from common import *
import zipfile,zlib
PRIVATE.mkdir(exist_ok=False);os.environ['GIT_OPTIONAL_LOCKS']='0'
checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('status',['status','--short']),('diff-check',['diff','--check'])]:
    begin=time.monotonic();r=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args],cwd=ROOT,capture_output=True)
    checks.append(dict(check=name,exit_code=r.returncode,stdout=r.stdout.decode(),stderr=r.stderr.decode(),seconds=time.monotonic()-begin));assert r.returncode==0
assert checks[0]['stdout'].strip()=='codex/04A-38' and all(r['stdout'].strip()==BASE for r in checks[1:4]) and checks[4]['stdout']==''
save('git-entry-v1.json',dict(checks=checks,index='EMPTY',index_file=pin(ROOT/'.git/index'),baseline=BASE))
sys.path.insert(0,str(PREVIOUS));import verify_c8_publication_v4 as c8
seal=json.loads((PREVIOUS/'review-seal-v1.json').read_bytes());assert pin(ROOT/seal['reader']['path'])==seal['reader']
begin=time.monotonic();proof=c8.compute();assert proof['historical']==dict(total=704,closed=298,OPEN=406) and proof['current']==dict(total=726,closed=324,OPEN=402)
save('v8-source-proof-entry-v1.json',dict(result=proof,reader=seal['reader'],seconds=time.monotonic()-begin,exit_code=0))
begin=time.monotonic();protected=c8.preservation(live=True);save('v8-live-preservation-entry-v1.json',dict(result=protected,reader=pin(PREVIOUS/'verify_c8_preservation_v1.py'),seconds=time.monotonic()-begin))
publication_path=W2/'continuation-v8-publication-receipt-v1.json';pub=json.loads(publication_path.read_bytes());manifest_path=ROOT/pub['manifest']['path'];assert pin(manifest_path)==pub['manifest']
rows=json.loads(manifest_path.read_bytes())['entries'];assert len(rows)==6306
for row in rows:
    if row['sha256'] is None:assert row['path']==manifest_path.relative_to(ROOT).as_posix() and row['bytes'] is None
    else:assert pin(ROOT/row['path'])==row,row['path']
assert sum(r['sha256'] is None for r in rows)==1
for item in [pub['archive'],pub['archive']['deterministic_reproduction']]:assert pin(ROOT/item['path'])=={k:item[k] for k in ['path','bytes','sha256']}
stored={r['path']:r for r in pub['physical_zip']['records']}
with zipfile.ZipFile(ROOT/pub['archive']['path']) as archive:
    assert archive.namelist()==[r['path'] for r in rows]
    for row in rows:
        data=archive.read(row['path']);r=stored[row['path']];assert (len(data),sha(data),format(zlib.crc32(data)&0xffffffff,'08x'))==(r['bytes'],r['sha256'],r['stored_CRC32'])
    assert archive.testzip() is None
save('v8-publication-entry-v1.json',dict(status='LIVE6306_MANIFEST_AND_PHYSICAL_ZIP_SHA_CRC_PASS',manifest=pub['manifest'],publication=pin(publication_path),archive=pub['archive'],null_self_entries=1,privacy='Exact authenticated prior physical privacy result, not a new regex scan'))
retained_path=W2/'continuation-v8-retained-private-preservation-v1.json';retained=json.loads(retained_path.read_bytes())
for row in retained['private_pins']+[{k:r[k] for k in ['path','bytes','sha256']} for r in retained['retained_prior_physical_archives']]:assert pin(ROOT/row['path'])==row
save('v8-private-entry-v1.json',dict(receipt=pin(retained_path),pins=retained['private_pins'],prior_archives=retained['retained_prior_physical_archives'],all_exact=True))
anchors=['owner-review.md','NEXT_WAVE_HANDOVER.md','historical-ownership-v2.json','current-ownership-v2.json','remaining-work-queue-v1.json','new-closures-v1.json','whole-body-contracts-v1.json','proof-authority-crosswalk-v1.json','original-fragment-parent-crosswalk-v1.json','new-parent-fragment-crosswalk-v2.json','source-transitions-final-v1.json','review-seal-v1.json','validation-v1.json']
external=[W2/n for n in ['continuation-v8-final-git-preservation-v1.json','continuation-v8-publication-receipt-v1.json','continuation-v8-retained-private-preservation-v1.json']]
queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes());assert len(queue['members'])==406 and len(queue['first_members'])==24
expected=['PASSINGBABLOK_FIT','PASSINGBABLOK_COEF','DEMING_FIT','DEMING_COEF','BLANDALTMAN_FIT','BLANDALTMAN_STATS','BLANDALTMAN_PLOTDATA','BLANDALTMAN_ALLOWABLE_BIAS','BLANDALTMAN_DECISION','LINCCC_FIT','LINCCC_VALUE','KAPPA_FIT','KAPPA_VALUE','ICC_FIT','ICC_VALUE','ICC_RC','CLASS_CONFUSION','CLASS_THRESH','CLASS_CALIB','CLASS_BRIER','CT_CHI2','CT_NOMINAL_ASSOC','CT_FISHER_2X2','CT_FFH_EXACT']
assert [r['name'] for r in queue['first_members']]==expected
save('entry-v1.json',dict(status='LIVE_C8_AUTHENTICATED_BEFORE_C9_IMPLEMENTATION',reader=pin(HERE/'entry_v1.py'),common=pin(HERE/'common.py'),anchors=[pin(PREVIOUS/n) for n in anchors],external_receipts=[pin(p) for p in external],checks=[pin(HERE/n) for n in ['git-entry-v1.json','v8-source-proof-entry-v1.json','v8-live-preservation-entry-v1.json','v8-publication-entry-v1.json','v8-private-entry-v1.json']],instructions=pin(ROOT/'AGENTS.md'),master_plan=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'),first_members=queue['first_members'],original_queries=0,tests='NOT_RUN_ENTRY_AUTHENTICATION_ONLY'))
print('C9 live entry PASS; C8 source, protected files, private pins and physical archive authenticated')
