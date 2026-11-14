"""Seal a small sanitized witness dossier; read its physical ZIP with both original scanners."""
from witness_lib import *
from physical import scanner,ZipRoot,inventory
from gate_reader import documents,verify,VERDICT
from readback import readback
import zipfile,zlib

verify_frozen();decision=verify(documents());controls=load(HERE/'adverse-controls.json');assert controls['failed']==0
entry=load(HERE/'entry.json');sdk=load(HERE/'sdk-HEAD-proof.json');pres=load(HERE/'preservation-reconciliation.json')
# Carry the previously absent dependency and its complete authentic provenance.
for n,path in enumerate(entry['guard_provenance']):
    dest=HERE/'dependencies'/('provenance-'+str(n+1)+'.json')
    with dest.open('xb') as f:f.write((ROOT/path['path']).read_bytes())
    assert digest(dest)==path['sha256']
raw=dict(utc=utc(),branch=git('branch','--show-current').decode().strip(),HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),
    staged=git('diff','--cached','--name-only').decode(),status=git('status','--short').decode(),diff_check=git('diff','--check').decode(),index=pin('.git/index'),
    source_lock=entry['source_lock'],old_correction_files_unchanged=True)
assert not raw['staged'] and not raw['diff_check'];save(PRIVATE/'git-final-raw.json',raw)
save('Git-final.json',dict(raw,raw=pin(PRIVATE/'git-final-raw.json'),source_test_plan_count=1353,no_new_existing_source_edits=True,diff_check_exit=0,staged_empty=True,
    commands=entry['actual_commands'],historical_tracked_modifications_preserved=True))
ready=dict(decision,utc=utc(),owner_authority=entry['authority'],prior_readiness=pin(OLD/'final/readiness.json'),prior_status='BLOCKED_04A39_OWNER_DECISION_REQUIRED',
    reconciliation='Subsequent limited owner authority makes the verified physical derivative gate applicable to39 readiness. Original scanner scope and every FAIL/ERROR remain unchanged.',
    input_archives=entry['input_archives'],owner_context=load(HERE/'physical-inputs.json')['owner_context'],
    guard=entry['guard'],original_scanners=entry['scanners'],fixture_count=25,physical_generic_main=27,original_conditional_SDK=2,
    original_scanner_main=25,postseal=0,unresolved_sensitive=0,fixture_proof=pin(HERE/'exact-25-fixtures.json'),sdk_proof=pin(HERE/'sdk-HEAD-proof.json'),
    preservation=pin(HERE/'preservation-reconciliation.json'),adverse_controls=pin(HERE/'adverse-controls.json'),Git=pin(HERE/'Git-final.json'),
    source_pins=1353,old_dossier_files=468,Python_parents=956,changed_diagnostic_FAIL_events=54,canonical_register=1169,original_obligations=1514,restricted_controls=802,
    retained_results=pres['retained_campaigns'],expensive_campaigns_rerun=0,scientific_native_manual='NOT_RUN_UNAUTHORIZED',
    review_limit='Remote reviewers receive exact scanner/guard source and authenticated HEAD snapshot, but no real owner username/hostname values. The fresh owner-local original-scanner executions are machine-bound attestations; generic/per-key/HEAD checks can be independently repeated using pinned ZIP bytes.')
save('readiness.json',ready)
archives=entry['input_archives'];gsha=entry['guard']['sha256']
report=f'''# 04A.39 final privacy witness and owner readiness

`{VERDICT}`

**Publication:** `PHYSICAL_DERIVATIVE_PUBLICATION_PASS`, zero unresolved sensitive findings. **Overall39:** ready for owner acceptance review with documented restrictions; not accepted, committed or merged. The14November2026 limited owner authorization applies only to the physical derivative gate. The old correction-v1 BLOCKED dossier remains byte-identical and records the earlier authority state.

## Live Git and exact physical scope

Branch `codex/04A-39`; HEAD/main/merge-base `{MAIN}`. Index SHA`{entry['Git']['index']['sha256']}` remains unchanged; staged index empty; diff-check0. Actual commands: `git branch --show-current`, `git rev-parse HEAD main`, `git merge-base HEAD main`, `git diff --cached --name-only`, `git diff --check`, `git status --short`, and SHA256 of`.git/index`, with command-scoped safe-directory and optional locks disabled. The pre-existing project/plan modifications are preserved; no production/existing test/plan file was edited here. Live Git inspection is distinct from ZIP checksums.

| Exact reviewed ZIP | Bytes | Physical members | SHA-256 |
|---|---:|---:|---|
|CORRECTION_REVIEW_v1|{archives[0]['bytes']}|2060|`{archives[0]['sha256']}`|
|CORRECTION_POSTSEAL_v2|{archives[1]['bytes']}|19|`{archives[1]['sha256']}`|

Both original scanner13 and14 freshly read every physical member/path, including generated gate/index and postseal metadata. UTF8sig and both strict round-tripping encodings for three pinned legacy sources were covered; the unchanged scanner adds NFKC, JSON-decoded and base64-decoded content. Real owner username and hostname tokens were active and positively tested, with their values retained privately. No stage-only result or previous scanner output substitutes for these four fresh executions. Both scanners report25 main findings and0 postseal findings; every25-key source/line/origin/category/encoding/offset/match hash matches its already justified negative test. No synthetic entries were added. Zero missing, changed or additional occurrences remain.

## Original scanner, guard and SDK predicate

Scanner13 SHA`b26596e5bf9644e81a479fad4f28d3db8d8af23611175b8e582c759007009a03`; scanner14 SHA`b4b4fd3567947d807c73e63022a0128ded37199248d0067f786cb472811d5a25`.

Historical842-byte guard SHA`{gsha}` is authenticated against its original v6 proof, W3 and39 optimization receipts, and included in this dossier. Every email match requires its exact domain suffix; absent suffix proves no match, otherwise the original regex and Match objects run. Other patterns/flags are unchanged. Fresh controls:577 email samples,559 positive samples,18 direct original-scanner comparisons, all exact; original historical12068-case proof retained. No real original-scanner match was suppressed.

The independent generic main count27 decomposes into25 exact synthetic matches plus two public SDK path detections at`BESHstatNG/BESHStatNG.vbproj` lines603/606. Live `git show HEAD:BESHstatNG/BESHStatNG.vbproj` yields **{sdk['HEAD_source']['bytes']} bytes**, SHA`{sdk['HEAD_source']['sha256']}`. Exact SDK literals occur in that actual HEAD source. Both original scanners exercise their unchanged conditional exception. Removing the SDK literal from an adverse-only HEAD reply exposes both findings; there is no wildcard or new synthetic allowance. The authentic HEAD source is transported at its canonical project path in this small dossier, with hashes/location proof.

## Preservation and restricted outcomes

The original468-file NOT_READY checkpoint,1353 live source/test/plan pins, all{pres['protected_unique_paths']} exact listed39/correction private paths, raw Python authorities and source preimages reverify. Current archive source representation remains1322 exact plus31 explicit renderings; this is distinct from1353 live originals. The134 protected upload exclusions remain intact. HistoricalW3 preservation37484/154 remains inherited with its qualified physical scope, not a fresh full union audit. No historical TestResults traversal or cleanup occurred.

All956 Python parents and54 changed diagnostic FAIL events reconcile; three scoped FAIL-to-PASS guards and two privacy ERROR-to-FAIL transitions remain fully disclosed. The retained full39 population is833PASS/112FAIL/11ERROR versus W3's830/113/13. The focused26-parent3PASS/23FAIL/0ERROR replay is separate. All54 FAIL message/trace differences remain source-explained and unwaived. The two retained repository-candidate parents still FAIL with1941 findings each, preserving1599 earlier occurrences plus342 later occurrences per parent. Both W3 historical privacy ERRORs remain unwaived. These parents were not rerun or rescoped here.

The192-member policy map and1169 canonical entries preserve1514 obligations,802 restricted controls,2967 crosswalk rows,33 current source pins, statuses and aliases. Retained software campaigns: Debug21113PASS/14FAIL; targeted Release336PASS;268 exports/configuration;77 exact fixtures/configuration;29424 frozen compatibility cases/configuration (28966pre-edit+458supplement); Node916PASS; strict/profile/MkDocs1/1/0. No original reference was executed, rebaselined or relabeled, and no full campaign was repeated.

| Gate | Current disposition |
|---|---|
|Physical derivative publication|VERIFIED_PASS, zero unresolved|
|Repository-candidate privacy|FAIL_UNWAIVED, scope unchanged|
|W3 privacy|Historical ERRORs retained, unwaived|
|54 changed diagnostics|Explained retained FAILs, not waived|
|Scientific/native/manual/Office-Mac|NOT_RUN_UNAUTHORIZED; restrictions unchanged|
|Overall39|Owner acceptance review WITH_DOCUMENTED_RESTRICTIONS; not accepted/merged|

Independent reviewers can inspect all dependencies and repeat generic,25-key and authenticated-HEAD checks against the pinned ZIPs. Real owner token values intentionally remain private; the owner-local executions are supported by immutable machine-bound journals, token fingerprints and actual detection controls. This is not a repository-wide privacy or scientific validation claim.

The unsafe older v6 raw derivative (102 raw files/1934 source-bound findings) remains preserved and is not for republication. Obtain independent review and an explicit owner39 acceptance decision. No commit, stage, merge, push, release or04A.40 work follows automatically.

`P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A40_NOT_AUTHORIZED`
'''
text('owner-review.md',report)
text('REVIEW.md','Use readback.py <witnessZIP> --sha256 <external receipt SHA> --main <original REVIEW_v1 ZIP> --postseal <original POSTSEAL_v2 ZIP>. It verifies every dossier/input member,25 keys and the authenticated HEAD SDK predicate. This does not reproduce private owner identifiers remotely. The witness ZIP is complementary to the two existing exact ZIPs, which remain unchanged. The external delivery receipt is self-excluded to avoid circular archive hashes. Older blocked/failing dossiers remain authoritative historical evidence. No automatic phase acceptance or next-phase authority.\n')
payload={}
for p in sorted(HERE.rglob('*')):
    if p.is_file() and '__pycache__' not in p.parts:
        payload[p.relative_to(ROOT).as_posix()]=p.read_bytes()
for phase in ('13','14'):
    name='scripts/verify-04a'+phase+'-publication.py';payload[name]=(ROOT/name).read_bytes()
payload['BESHstatNG/BESHStatNG.vbproj']=(ROOT/sdk['HEAD_snapshot']['path']).read_bytes()
rows=[dict(path=name,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data):08x}') for name,data in sorted(payload.items())]
index=dict(schema='04A39-final-physical-privacy-witness-v1',files=rows,input_archives=archives,gates=GATES,
    source_alias=dict(local_snapshot=sdk['HEAD_snapshot']['path'],archive_path='BESHstatNG/BESHStatNG.vbproj',sha256=sdk['HEAD_source']['sha256']),
    self_policy='Inventory excluded only from its own file map; external delivery receipt pins the archive and index. No raw private owner context or traceback transported.')
payload['.witness-inventory.json']=(json.dumps(index,indent=2,ensure_ascii=False)+'\n').encode()
archive=PRIVATE/'BESHStatNG_04A39_FINAL_PRIVACY_WITNESS_v1.zip'
with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6) as z:
    for name,data in sorted(payload.items()):
        info=zipfile.ZipInfo(name,date_time=(2026,11,14,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;info.external_attr=0o600<<16
        z.writestr(info,data)
# Quarantine archive: no publication PASS until these actual physical contents pass both originals.
new_scan=[]
with zipfile.ZipFile(archive) as z:
    actual=inventory(z);assert len(actual)==len(payload)
    for phase in ('13','14'):
        m=scanner(phase);m.ROOT=ZipRoot(z,'utf-8-sig');f,reviewed=m.scan(z.namelist())
        save(PRIVATE/('new-dossier-scanner-'+phase+'.json'),dict(archive=pin(archive),findings=f,reviewed=reviewed,scanner=pin(m.__file__)))
        assert not f,'NEW_DOSSIER_PRIVACY_NOT_VERIFIED_DO_NOT_PUBLISH'
        new_scan.append(dict(scanner=phase,findings=0,reviewed=reviewed,physical_members=len(actual)))
result=readback(archive,digest(archive),ROOT/archives[0]['path'],ROOT/archives[1]['path'])
verify_frozen();original=imported('old_postseal_preservation',OLD/'preservation.py').verify_checkpoint();assert len(original)==468
assert not git('diff','--cached','--name-only') and not git('diff','--check')
receipt=dict(status=VERDICT,publication='PHYSICAL_DERIVATIVE_PUBLICATION_PASS',archive=pin(archive),physical_members=len(payload),per_file_index=rows,index_sha256=sha(payload['.witness-inventory.json']),
    original_reviewed_archives_reverified=[pin(r['path']) for r in archives],physical_contents_readback=result,new_dossier_physical_scanners=new_scan,
    owner_review=pin(HERE/'owner-review.md'),readiness=pin(HERE/'readiness.json'),git_receipt=pin(HERE/'Git-final.json'),
    postseal_source_lock_reverified=1353,postseal_original_dossier_reverified=468,postseal_old_correction_files_unchanged=True,
    branch='codex/04A-39',HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index=pin('.git/index'),
    staged_empty=True,diff_check_exit=0,production_or_existing_test_edits=0,commit_merge_push=False,owner39_acceptance=False,gates=GATES,
    self_policy='External receipt excluded from the small witness ZIP to avoid circular hashes. All necessary source/proofs and older ZIP identities are inside. Original two ZIPs and old blocked dossier remain unchanged.')
# Receipt itself is inspected without changing the scanner's SDK path predicate.
from types import SimpleNamespace
class ReceiptPath:
    def is_file(self):return True
    def read_text(self,encoding):return json.dumps(receipt,ensure_ascii=False)
class ReceiptRoot:
    parents=ROOT.parents;name=ROOT.name
    def __truediv__(self,path):return ReceiptPath()
    def as_posix(self):return ROOT.as_posix()
    def __fspath__(self):return str(ROOT)
for phase in ('13','14'):
    m=scanner(phase);m.ROOT=ReceiptRoot();assert not m.scan(['delivery-receipt-v1.json'])[0]
save('delivery-receipt-v1.json',receipt)
print(json.dumps(dict(status=VERDICT,publication=receipt['publication'],archive=receipt['archive'],members=len(payload),input_members=[2060,19],unresolved=0,postseal='PASS'),sort_keys=True),flush=True)
