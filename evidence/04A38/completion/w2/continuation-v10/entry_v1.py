"""Authenticate full authoritative C9 state before C10 production/test edits."""
from common import *
import importlib.util,zipfile,zlib
PRIVATE.mkdir(exist_ok=False);os.environ['GIT_OPTIONAL_LOCKS']='0'
checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('status',['status','--short']),('diff-check',['diff','--check'])]:
    begin=time.monotonic();r=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args],cwd=ROOT,capture_output=True);checks.append(dict(check=name,exit_code=r.returncode,stdout=r.stdout.decode(),stderr=r.stderr.decode(),seconds=time.monotonic()-begin));assert r.returncode==0
assert checks[0]['stdout'].strip()=='codex/04A-38' and all(r['stdout'].strip()==BASE for r in checks[1:4]) and checks[4]['stdout']==''
save('git-entry-v1.json',dict(checks=checks,index='EMPTY',index_file=pin(ROOT/'.git/index'),baseline=BASE))
names=['owner-review.md','NEXT_WAVE_HANDOVER.md','remaining-work-queue-v1.json','new-closures-v3.json','historical-ownership-v2.json','current-ownership-v2.json','ownership-summary-v2.json','source-transitions-final-v1.json','review-seal-v1.json']
external_names=['continuation-v9-publication-receipt-v1.json','continuation-v9-final-git-preservation-v1.json','continuation-v9-postseal-handover-addendum-v1.md','continuation-v9-retained-private-reader-v6.py','continuation-v9-retained-private-preservation-v2.json','continuation-v9-post-publication-artifacts-v1.json','continuation-v9-private-proof-transition-v1.json']
anchors=[pin(PREVIOUS/n) for n in names];external=[pin(W2/n) for n in external_names]
documents={n:json.loads((PREVIOUS/n).read_bytes()) for n in names if n.endswith('.json')};documents.update({n:json.loads((W2/n).read_bytes()) for n in external_names if n.endswith('.json')})
summary=documents['ownership-summary-v2.json'];assert summary['historical']==dict(total=704,closed=343,OPEN=361) and summary['current']==dict(total=726,closed=373,OPEN=353) and summary['support']==dict(raw=325,distinct=303,distinct_closed=244,distinct_OPEN=59)
history=documents['historical-ownership-v2.json']['members'];queue=documents['remaining-work-queue-v1.json'];assert len(history)==704 and len(queue['members'])==361 and len(queue['first_members'])==24
expected=['CT_MCNEMAR_EXACT','CT_ODDS_RATIO','CT_RISK_RATIO','CT_TREND','CT_ORDINAL_ASSOC','CT_MANTEL_HAENSZEL','CT_SINGLE_PROPORTION','CT_TWO_INDEPENDENT_PROPORTIONS','CT_PAIRED_PROPORTIONS','CT_TWO_INDEPENDENT_PROPORTIONS_NI','CT_TWO_INDEPENDENT_PROPORTIONS_EQUIV','PCA_FIT','PCA_SUMMARY','PCA_MATRIX','PCA_EIGEN','PCA_LOADINGS','PCA_SCORES','FA_FIT','FA_SUMMARY','FA_MATRIX','FA_EIGEN','FA_LOADINGS','FA_STRUCTURE','FA_FACTORCORR']
assert [r['name'] for r in queue['first_members']]==expected and queue['first_members']==queue['members'][:24]
by={r['canonical_original_id']:r for r in history};assert all(not by[r['original_id']]['proof'] for r in queue['members'])
sys.path.insert(0,str(PREVIOUS));import verify_c9_publication_v1 as c9
seal=documents['review-seal-v1.json'];assert pin(ROOT/seal['reader']['path'])==seal['reader']
begin=time.monotonic();proof=c9.compute();assert proof['historical']==summary['historical'] and proof['current']==summary['current'];save('v9-source-proof-entry-v1.json',dict(result=proof,reader=seal['reader'],seconds=time.monotonic()-begin))
begin=time.monotonic();protected=c9.preservation(live=True);save('v9-live-preservation-entry-v1.json',dict(result=protected,reader=pin(PREVIOUS/'verify_c9_preservation_v1.py'),seconds=time.monotonic()-begin))
pubpath=W2/'continuation-v9-publication-receipt-v1.json';pub=documents[pubpath.name];manifest=ROOT/pub['manifest']['path'];assert pin(manifest)==pub['manifest'];rows=json.loads(manifest.read_bytes())['entries'];assert len(rows)==6698
for row in rows:
    if row['sha256'] is None:assert row['path']==manifest.relative_to(ROOT).as_posix() and row['bytes'] is None
    else:assert pin(ROOT/row['path'])==row,row['path']
assert sum(r['sha256'] is None for r in rows)==1
for item in [pub['archive'],pub['archive']['deterministic_reproduction']]:assert pin(ROOT/item['path'])=={k:item[k] for k in ['path','bytes','sha256']}
stored={r['path']:r for r in pub['physical_zip']['records']}
with zipfile.ZipFile(ROOT/pub['archive']['path']) as archive:
    assert archive.namelist()==[r['path'] for r in rows]
    for row in rows:
        data=archive.read(row['path']);r=stored[row['path']];assert (len(data),sha(data),format(zlib.crc32(data)&0xffffffff,'08x'))==(r['bytes'],r['sha256'],r['stored_CRC32'])
    assert archive.testzip() is None
save('v9-publication-entry-v1.json',dict(status='LIVE6698_MANIFEST_AND_PHYSICAL_ZIP_SHA_CRC_PASS',manifest=pub['manifest'],publication=pin(pubpath),archive=pub['archive'],null_self_entries=1,privacy='Exact prior physical policy result authenticated; no duplicate entry regex scan.'))
retainedpath=W2/'continuation-v9-retained-private-preservation-v2.json';retained=documents[retainedpath.name];assert retained['private_pin_count']==len(retained['private_pins'])==1964 and pin(ROOT/retained['reader']['path'])==retained['reader']
spec=importlib.util.spec_from_file_location('c10_prior_private',W2/'continuation-v9-retained-private-reader-v6.py');private_reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(private_reader)
private_reader.validate_phase(documents['continuation-v9-private-proof-transition-v1.json'])
for row in retained['private_pins']+[{k:r[k] for k in ['path','bytes','sha256']} for r in retained['retained_prior_physical_archives']]:private_reader.check(row)
partial=retained['retained_interrupted_aggregation'];private_reader.check(partial);assert (partial['bytes'],partial['sha256'])==(4213129746,'3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb')
artifact=documents['continuation-v9-post-publication-artifacts-v1.json']
for row in artifact['current_compiled_artifacts']:private_reader.check({k:row[k] for k in ['path','bytes','sha256']})
for row in artifact['source_after_images']:private_reader.check(row)
private_reader.check(artifact['physical_adverse_archive'])
save('v9-private-entry-v1.json',dict(receipt=pin(retainedpath),reader=retained['reader'],pins=retained['private_pins'],prior_archives=retained['retained_prior_physical_archives'],partial=partial,all_exact=True,phase=retained['intentional_synthetic_fixture_transition']))
save('entry-v1.json',dict(status='LIVE_C9_AUTHENTICATED_BEFORE_C10_IMPLEMENTATION',reader=pin(HERE/'entry_v1.py'),common=pin(HERE/'common.py'),anchors=anchors,external_receipts=external,instructions=pin(ROOT/'AGENTS.md'),master_plan=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'),first_members=queue['first_members'],checks=[pin(HERE/n) for n in ['git-entry-v1.json','v9-source-proof-entry-v1.json','v9-live-preservation-entry-v1.json','v9-publication-entry-v1.json','v9-private-entry-v1.json']],original_queries=0,tests='NOT_RUN_ENTRY_AUTHENTICATION_ONLY'))
print('C10 live entry PASS; C9 source, ledgers, private-v2/reader-v6, protected captures and physical ZIP authenticated')
