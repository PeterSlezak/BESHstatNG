"""Once-only live C7 authentication before C8 source changes; no tests/restore."""
from common import *
import importlib.util,zipfile,zlib
def load(name,path):
    spec=importlib.util.spec_from_file_location(name,path);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
PRIVATE.mkdir(exist_ok=False)
checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('status',['status','--short']),('diff-check',['diff','--check'])]:
    start=time.monotonic();r=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args],cwd=ROOT,capture_output=True)
    assert r.returncode==0
    checks.append(dict(check=name,exit_code=r.returncode,stdout=r.stdout.decode(),stderr=r.stderr.decode(),seconds=time.monotonic()-start))
assert checks[0]['stdout'].strip()=='codex/04A-38' and all(r['stdout'].strip()==BASE for r in checks[1:4]) and checks[4]['stdout']==''
save('git-entry-v1.json',dict(checks=checks,index='EMPTY',index_file=pin(ROOT/'.git/index'),baseline=BASE))
sys.path.append(str(PREVIOUS));sys.path.append(str(W2/'continuation-v6'))
seal=json.loads((PREVIOUS/'review-seal-v1.json').read_bytes());assert pin(ROOT/seal['reader']['path'])==seal['reader']
start=time.monotonic();proof=load('c7_entry_reader',PREVIOUS/'verify_c7_checkpoint_v1.py').compute()
save('v7-source-proof-entry-v1.json',dict(result=proof,reader=seal['reader'],exit_code=0,seconds=time.monotonic()-start))
assert proof['historical']==dict(total=704,closed=274,OPEN=430) and proof['current']==dict(total=724,closed=299,OPEN=425)
start=time.monotonic();protection=load('c7_entry_preservation',PREVIOUS/'verify_preservation_v1.py').inspect()
save('v7-live-preservation-entry-v1.json',dict(result=protection,reader=pin(PREVIOUS/'verify_preservation_v1.py'),seconds=time.monotonic()-start))
manifest=json.loads((PREVIOUS/'publication-manifest-v1.json').read_bytes());rows=manifest['entries'];assert len(rows)==5915
nulls=[]
for row in rows:
    if row['sha256'] is None:
        assert row['path']==(PREVIOUS/'publication-manifest-v1.json').relative_to(ROOT).as_posix() and row['bytes'] is None;nulls.append(row)
    else:assert pin(ROOT/row['path'])==row,row['path']
assert len(nulls)==1
publication=json.loads((W2/'continuation-v7-publication-receipt-v1.json').read_bytes());archive=ROOT/publication['archive']['path']
assert archive.stat().st_size==publication['archive']['bytes']
with archive.open('rb') as stream:assert hashlib.file_digest(stream,'sha256').hexdigest()==publication['archive']['sha256']
stored={r['path']:r for r in publication['physical_zip']['records']}
with zipfile.ZipFile(archive) as source:
    assert source.namelist()==[r['path'] for r in rows] and len(set(source.namelist()))==len(rows)
    for row in rows:
        data=source.read(row['path']);record=stored[row['path']]
        assert (len(data),sha(data),format(zlib.crc32(data)&0xffffffff,'08x'))==(record['bytes'],record['sha256'],record['stored_CRC32'])
    assert source.testzip() is None
assert all(c['status']=='REJECTED' for c in publication['mutations']) and len(publication['mutations'])==5
save('v7-publication-entry-v1.json',dict(status='LIVE_ALL5914_PAYLOAD_HASH_AND5915_ZIP_SHA_CRC_PASS',null_self_entries=1,archive=publication['archive'],publication=pin(W2/'continuation-v7-publication-receipt-v1.json'),git=pin(W2/'continuation-v7-final-git-preservation-v1.json'),privacy='Authenticated exact final C7 privacy-verified bytes; no repeated regex scan',retained_recovery=publication['recovery']))
anchors=['owner-review.md','NEXT_WAVE_HANDOVER.md','remaining-work-queue-v1.json','new-closures-v1.json','whole-body-contracts-v1.json','proof-authority-crosswalk-v1.json','original-fragment-parent-crosswalk-v1.json','current-ownership-v1.json','historical-ownership-v1.json','source-transitions-final-v1.json','readiness-v1.json','review-seal-v1.json','independent-controls-v1.json','command-receipts-v1.json']
refs={}
def walk(x):
    if isinstance(x,dict):
        if all(k in x for k in ['path','bytes','sha256']) and isinstance(x['path'],str) and x['sha256'] is not None:refs[x['path'],x['bytes'],x['sha256']]=x
        for v in x.values():walk(v)
    elif isinstance(x,list):
        for v in x:walk(v)
for name in anchors:walk(json.loads((PREVIOUS/name).read_bytes())) if name.endswith('.json') else None
for row in refs.values():assert pin(ROOT/row['path'])==row
save('v7-evidence-pins-entry-v1.json',dict(anchors=[pin(PREVIOUS/p) for p in anchors],referenced_pins=list(refs.values()),all_exact=True,original_queries=0))
queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())
save('entry-v1.json',dict(status='LIVE_C7_AUTHENTICATED_BEFORE_C8_SOURCE_EDITS',reader=pin(HERE/'entry_v1.py'),common=pin(HERE/'common.py'),instructions=pin(ROOT/'AGENTS.md'),master_plan=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'),queue=pin(PREVIOUS/'remaining-work-queue-v1.json'),first_members=queue['first_members'],negative_controls='Authenticated unchanged C7 directed controls and publication rejection receipts; no unit suites rerun',original_queries=0))
print('C8 live entry PASS; no source edits, tests, restore or original queries',flush=True)
