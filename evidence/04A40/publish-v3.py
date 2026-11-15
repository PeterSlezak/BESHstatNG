"""Review-safe selected payload, original stage/physical scanners, external hash receipt."""
from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'evidence/04A40'))
from common40_v3 import *
import ast,collections,re,shutil,unicodedata,zipfile,zlib
sys.path.append(str(ROOT/'evidence/04A38'))
from baseline_report import clean
sys.path.insert(0,str(ROOT/'evidence/04A39/physical-witness-v1'))
import physical

lock();assert load(HERE/'final/readiness-v2.json')['status'] in ['NOT_READY_04A40','READY_FOR_OWNER_04A40_ACCEPTANCE_REVIEW']
context=physical.tokens();assert len(context)>0
expected_guard='ad17f82adca850afb94fa60d3c0ca5866b446860a7724dc77a5e5b3111c9ce5f'
assert pin('evidence/04A38/completion/w2/continuation-v6/privacy_regex_guard.py')['sha256']==expected_guard
folder=PRIVATE/'publication-v2';stage=folder/'stage';stage.mkdir(parents=True)
selected={p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and p.suffix!='.pyc'}
selected.update(r['after']['path'] for r in load(HERE/'matrix/source-freeze-v2.json')['source_delta'])
selected.update(r['path'] for r in load(HERE/'matrix/source-freeze-v2.json')['new_tests'])
selected.update(['BESHstatNG/BESHStatNG.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj','BESHStat.Core/BESHStat.Core.vbproj',
 'BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan2.15.md',
 'evidence/04A39/register/defects-v7.json','evidence/04A39/verify-register-v5.py','evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json',
 'evidence/04A39/correction-v1/privacy/focused-finding-reconciliation.json','evidence/04A39/physical-witness-v1/readiness-v4.json',
 'evidence/04A39/physical-witness-v1/exact-25-fixtures.json','evidence/04A39/physical-witness-v1/sdk-HEAD-proof.json',
 'docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json','docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv',
 'docs/architecture/WindowsMigrationLedger/symbols.csv','evidence/04A38/completion/w3/final/unresolved-exceptions-v2.json',
 'evidence/04A38/completion/w3/python-node-scanners/python-identities-v1.json','scripts/verify-04a13-publication.py','scripts/verify-04a14-publication.py'])
for r in load(HERE/'scope/source-freeze-v1.json')['paths']:selected.add(r['path'])
selected.update(['evidence/04A39/fixtures/frozen-input-manifest-v2.json','evidence/04A39/fixtures/Observe39v2.cs','evidence/04A39/fixtures/before-v2.json',
 'evidence/04A13/R-candidate-reference-v2.csv','evidence/04A13/numerical-comparisons.json','evidence/04A13/issues.json','scripts/reference-04a13.R'])
for config in ['Debug','Release']:
    selected.add('evidence/04A39/abi-cil/'+config+'/exports-v1.json')
    for row in load(HERE/('compatibility/'+config+'-current-only-v4.json'))['results']:
        for field in ['original','current','inputs','recorder']:selected.add(row[field]['path'])
selected.update(['evidence/04A39/physical-witness-v1/physical.py','evidence/04A39/physical-witness-v1/witness_lib.py',
 'evidence/04A39/physical-witness-v1/dependencies/privacy_regex_guard.py','evidence/04A39/correction-v1/entry/source-lock.json'])
renderings=[]
accepted_zip=ROOT/'.test-rationalization-04A39-correction-v1.tmp/BESHStatNG_04A39_CORRECTION_REVIEW_v1.zip'
assert pin(accepted_zip)['sha256']=='6164b6ff1f605997a4a2c482794682ab3e70bb85a48288721f252dec0bb03cab'
accepted=zipfile.ZipFile(accepted_zip)
old_source={r['path']:r for r in load(ROOT/'evidence/04A39/correction-v1/entry/source-lock.json')['files']}
def put(name,data):
    physical.safe_name(name);p=stage/name;p.parent.mkdir(parents=True,exist_ok=True);p.open('xb').write(data)
for name in sorted(selected):
    raw=(ROOT/name).read_bytes();data=raw
    if name in old_source and sha(raw)==old_source[name]['sha256'] and name in accepted.namelist():
        data=accepted.read(name)
    if name.startswith('evidence/04A40/'):
        if name.endswith('.json'):
            doc=json.loads(raw);new=clean(doc)
            if new!=doc:data=(json.dumps(new,ensure_ascii=False,indent=2,allow_nan=False)+'\n').encode()
        elif name.endswith(('.py','.cs','.md')):
            text=raw.decode('utf-8-sig');rendered=clean(text)
            if rendered!=text:data=rendered.encode()
    if raw!=data:renderings.append(dict(path=name,original=pin(name),review_sha256=sha(data),review_bytes=len(data),disposition='DETERMINISTIC_REVIEW_RENDERING;RAW_LOCAL_PRESERVED;NOT_AN_EXECUTION_SOURCE'))
    if name=='evidence/04A40/baseline/Debug-build-v1.json':
        assert raw!=data and data==(PRIVATE/'baseline-Debug-build-review-v1.json').read_bytes(),'PROTECTED_RAW_ENTERED_PAYLOAD'
    put(name,data)
for r in load(HERE/'preservation/historical-readback-v4.json')['authorized_source_transitions']:
    put('preimages-main/'+r['before']['path'],(ROOT/r['preserved_preimage']['path']).read_bytes())
for config in ('Debug','Release'):
    put('evidence/04A40/abi-cil/'+config+'-actual-exports.json',(PRIVATE/('abi-cil-final2/'+config+'/exports.json')).read_bytes())
for label,path in [('before-current-main',PRIVATE/'before-v2.json'),('after-Debug',PRIVATE/'fixtures-final2-Debug/observations.json'),('after-Release',PRIVATE/'fixtures-final2-Release/observations.json')]:
    put('evidence/04A40/fixtures/'+label+'.json',path.read_bytes())
put('review_readback.py',(HERE/'review_readback-v2.py').read_bytes())
put('evidence/04A40/publication/dependencies/privacy_regex_guard.py',(ROOT/'evidence/04A38/completion/w2/continuation-v6/privacy_regex_guard.py').read_bytes())
policy=dict(scope='PHYSICAL_REVIEW_DERIVATIVE_ONLY;REPOSITORY_CANDIDATE_SCOPE_UNCHANGED',owner_context='Username and hostname applied locally; raw tokens/fingerprints remain controlled local evidence.',
  original_scanners=physical.EXPECTED,guard_sha256=expected_guard,synthetic_authority=pin('evidence/04A39/physical-witness-v1/exact-25-fixtures.json'),new_synthetic_allowances=0,
  external_receipt='Required beside ZIP to authenticate this archive and original-scanner outcomes. No archive can embed its own outer SHA.',gates=GATES)
put('.publication-policy.json',(json.dumps(policy,sort_keys=True,indent=2)+'\n').encode())
rows=[]
for p in sorted(stage.rglob('*')):
    if p.is_file():
        b=p.read_bytes();rows.append(dict(path=p.relative_to(stage).as_posix(),bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b):08x}'))
index=dict(schema='04A40_REVIEW_PAYLOAD_V1',active_readiness='evidence/04A40/final/readiness-v2.json',active_diagnostics='evidence/04A40/python/diagnostic-dispositions-v3.json',files=rows,self_excluded='.review-index.json',renderings=renderings,
  authoritative_raw='Local repository/private evidence remain unchanged. Raw TRX/stdout/stderr/tracebacks, binary build outputs, index bytes and historical TestResults omitted from transport; per-execution pins retained.',
  exclusions=load(HERE/'preservation/restricted-new-raw-inventory-v1.json'),reference='Accepted39 main/postseal archive hashes are inherited reference authorities, not re-uploaded or freshly executed.',gates=GATES)
put('.review-index.json',(json.dumps(index,ensure_ascii=False,indent=2)+'\n').encode())
allowed={tuple(r['key']) for r in load(ROOT/'evidence/04A39/physical-witness-v1/exact-25-fixtures.json')['fixtures']}
views=[]
for p in sorted(stage.rglob('*')):
    if not p.is_file():continue
    b=p.read_bytes()
    try:b.decode('utf-8-sig');enc=['utf-8-sig']
    except UnicodeDecodeError:
        enc=['cp1250','cp1252']
        for codec in enc:assert b.decode(codec).encode(codec)==b
    views.extend(dict(path=p.relative_to(stage).as_posix(),encoding=codec) for codec in enc)
class StageMember:
    def __init__(self,p,codec):self.p=p;self.codec=codec
    def is_file(self):return self.p.is_file()
    def read_text(self,encoding):assert encoding=='utf-8-sig';return self.p.read_bytes().decode(self.codec)
class StageRoot:
    parents=ROOT.parents;name=ROOT.name
    def __init__(self,codec):self.codec=codec
    def __truediv__(self,p):return StageMember(stage/p,self.codec)
    def __fspath__(self):return str(ROOT)
    def as_posix(self):return ROOT.as_posix()
def scan(label,archive=None):
    results=[]
    for phase in ('13','14'):
        m=physical.scanner(phase);details=[];reviewed=set()
        for view in views:
            m.ROOT=physical.ZipRoot(archive,view['encoding']) if archive else StageRoot(view['encoding'])
            f,r,d=physical.detailed(m,[view['path']]);reviewed.update(r)
            for row in d:
                b=archive.read(view['path']) if archive else (stage/view['path']).read_bytes()
                k=(row['path'],sha(b),row['origin'],row.get('line'),row['category'],row['offset'],row['match_sha256'],view['encoding'])
                details.append(dict(key=k,known_exact_fixture=k in allowed,raw=row))
        unresolved=[r for r in details if not r['known_exact_fixture']]
        save(PRIVATE/('publication-'+label+'-'+phase+'-raw.json'),dict(findings=details,reviewed=sorted(reviewed)))
        results.append(dict(scanner=phase,scanner_sha256=pin(m.__file__)['sha256'],physical_members=len({v['path'] for v in views}),member_encoding_views=len(views),
           original_findings=len(details),authorized_exact_fixture_matches=sum(r['known_exact_fixture'] for r in details),unresolved=len(unresolved),reviewed=sorted(reviewed),
           scan_AST_sha256=sha(physical.function_ast(m,'scan').encode()),raw=pin(PRIVATE/('publication-'+label+'-'+phase+'-raw.json')),guard_skips=m.re.skips,guard_fallbacks=m.re.fallbacks))
        print('Publication',label,'original scanner',phase,'findings',len(details),'unresolved',len(unresolved),flush=True)
    save(PRIVATE/('publication-'+label+'-results.json'),results)
    assert all(r['unresolved']==0 for r in results),'PUBLICATION_BLOCKED_UNRESOLVED_SENSITIVE_FINDINGS'
    return results
pre=scan('stage')
candidate=folder/'BESHStatNG_04A40_REVIEW_v2.zip'
with zipfile.ZipFile(candidate,'x',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
    for p in sorted(stage.rglob('*')):
        if p.is_file():z.write(p,p.relative_to(stage).as_posix())
with zipfile.ZipFile(candidate) as z:
    physical_rows=physical.inventory(z);assert len(physical_rows)==len(rows)+1 and z.testzip() is None
    actual={r['path']:r for r in physical_rows}
    for r in rows:assert actual[r['path']]==r
    post=scan('physical',z)
assert pre==[{**r,'raw':pre[i]['raw'],'guard_skips':pre[i]['guard_skips'],'guard_fallbacks':pre[i]['guard_fallbacks']} for i,r in enumerate(post)]
reader=module('standalone40',HERE/'review_readback-v2.py');readback=reader.verify(candidate)
save('publication/physical-readback-v1.json',dict(status='PHYSICAL_DERIVATIVE_PUBLICATION_PASS',archive=pin(candidate),physical_members=len(physical_rows),indexed=len(rows),SHA_size_CRC='PASS',
    stage=pre,physical=post,standalone_readback=readback,owner_local_username_and_hostname_applied=True,original_scanner_rules_unchanged=True,new_synthetic_allowances=0,
    repository_privacy='FAIL_UNWAIVED;SEPARATE_SCOPE',no_TestResults_walk=True,renderings=renderings,inventory_sha256=sha((stage/'.review-index.json').read_bytes())))
save('publication/delivery-receipt-v1.json',dict(archive=pin(candidate),physical_members=len(physical_rows),indexed=len(rows),physical_reader=pin(HERE/'publication/physical-readback-v1.json'),
  overall=load(HERE/'final/readiness-v2.json')['status'],accepted=False,merged=False,gates=GATES))
lock();print('Publication physical gate PASS; standalone readback PASS;',pin(candidate),flush=True)
