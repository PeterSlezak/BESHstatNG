from publication_helpers import *
import base64

def scan(label):
    reports=candidate_scan(label)
    for r in reports:
        m=physical.scanner(r['scanner']);m.ROOT=ProbeRoot(json.dumps(dict(value='benign portable review control')));found,reviewed=m.scan(['negative-control.json']);assert not found
        r['negative_control']=dict(zero_findings=True,no_identity_tokens_written=True)
        encoded=[]
        for i,token in enumerate(sorted(physical.tokens())):
            payload=base64.b64encode(('A'*90+' '+token+' '+'Z'*90).encode()).decode()
            m=physical.scanner(r['scanner']);m.ROOT=ProbeRoot(json.dumps(dict(payload=payload)));a,ar=m.scan(['encoded-control.json']);m.re=re;b,br=m.scan(['encoded-control.json'])
            assert a==b and ar==br and any(x['category']=='known_private_identifier' and x['origin']=='decoded_payload' for x in a)
            encoded.append(dict(identity_ordinal=i,mode='base64_decoded_payload',detected=True,direct_original_and_guard_exact=True))
        r['identity_controls']+=encoded
    return reports

def prepare():
    check_sources();lock();files=[pin(p) for p in sorted(PRIVATE.rglob('*')) if p.is_file()]
    for r in files:(ROOT/r['path']).chmod(stat.S_IREAD)
    save(PRIVATE/'new-raw-inventory-v1.json',dict(files=files,count=len(files),bytes=sum(r['bytes'] for r in files),scope='All42-owned raw files existing after executed campaigns and before privacy/physical seal;new seal receipts are independently pinned externally. Original stores unchanged.'))
    (PRIVATE/'new-raw-inventory-v1.json').chmod(stat.S_IREAD)
    failures=[]
    for p in sorted((PRIVATE/'failed-attempts').glob('*.json')):
        d=load(p);failures.append(dict(recipe=d['recipe'],type=d['type'],raw=pin(p),status='RETAINED_FAILED_ATTEMPT'))
    save('preservation/new-raw-review-v1.json',dict(kind='SANITIZED_PROTECTED_RAW_INVENTORY_FACTS;NOT_RAW_LOGS',inventory=pin(PRIVATE/'new-raw-inventory-v1.json'),files=len(files),bytes=sum(r['bytes'] for r in files),read_only=True,failed_attempts=failures,unwrapped_diagnostic_discovery='Read-only interactive discovery/quoting failures remain in tool history;no fabricated raw execution receipt.',old_stores_mutated=False))
    commands=[]
    for p in sorted((PRIVATE/'commands').glob('*/command.json')):
        d=load(p);commands.append(dict(label=p.parent.name,exit_code=d['exit_code'],seconds=d['seconds'],raw=pin(p),stdout_sha256=d['stdout_sha256'],stderr_sha256=d['stderr_sha256']))
    save('matrix/actual-commands-review-v1.json',dict(kind='SANITIZED_COMMAND_FACTS;NOT_RAW_ARGV_TRANSCRIPT',commands=commands,recipe_directory='evidence/04A42/',interpreter='Existing authenticated accepted3.14.7;no installations',execution_labels='Some orchestration commands inherit a41 prefix from reused command helper;all42 paths/base/source pins are explicit;these are actual labels,not41 executions.',authoritative_recipes=['run42.py authenticate-v1.py','run42.py entry-build-v1.py Debug/Release','run42.py entry-compiled-v1.py','run42.py capture-v1.py','run42.py verify-wave-v1.py a/b2/b3','run42.py check-adapters-v2.py','run42.py final-managed-v2.py Debug/Release','run42.py compiled-and-fixtures-v3.py Debug/Release','run42.py compatibility-v3.py Debug/Release','run42.py python-campaign-v4.py prepare','run42.py python-campaign-execution-v5.py execute','run42.py reconcile-python-v1.py','run42.py python-dispositions-v2.py','run42.py preservation-v2.py','run42.py serial-checks-v2.py'],full_Debug_campaigns=2,reason_for_repeat='First integration freeze exposed2 genuine extraction regressions;exact failed attempt preserved;corrected source refrozen. Full compile/affected Release/ABI/fixtures/frozen compatibility repeated for changed dependencies.',full_Python_campaigns=1,preflight_discovery_failures='Version3 reused a discovery command label;version4 execute initially referenced nonexistent historical W3v4;no full Python tests ran in those failed launches;executedv5 retains actual W3v2.',full_Release='NOT_RUN',Node='INHERITED_916_PASS;NOT_RERUN',direct_verify_script='NOT_RUN_FIXED_PROTECTED_TRX_COLLISION;UNCHANGED_ISOLATED_RUNNER_EQUIVALENT',Excel_COM_UIA_R_Office_WASM_manual='NOT_RUN',stage_commit_merge_rebase_push_release=False))
    reports=scan('repository-initial')
    save('privacy/repository-candidate-v1.json',dict(status='ZERO_FINDINGS_FOR_RECORDED_ACTUAL_FULL_CANDIDATE_INPUTS',reports=reports,waivers=0,rules_ignore_and_scope_unchanged=True,owner_tokens_and_unsalted_identifier_hashes_published=False,history='Accepted39 FAIL1941each;W3 privacyERROR;old40 raw failures;54+32+21 adverse diagnostic histories retained unchanged.',later_final_metadata='External receipt pins and scans full final candidate including all later metadata;no self-hash fiction.',gates=GATES))
    print('Controlled42 raw inventory sealed;actual full original candidate scans PASS',flush=True)

def documents():
    freeze=check_sources();debug=load(HERE/'build-and-tests/Debug-identities-v2.json');release=load(HERE/'build-and-tests/Release-identities-v2.json');py=load(HERE/'python/parent-event-reconciliation-v2.json')
    assert debug['summary']['outcomes']==dict(Passed=21163,Failed=14) and all(r['classification']=='RETAINED_IDENTICAL' for r in debug['nonpasses'])
    assert release['summary']['outcomes']==dict(Passed=6589) and not py['summary']['unresolved_diagnostic_events'] and py['summary']['parent_outcomes']==dict(PASS=832,FAIL=113,ERROR=11)
    assert load(HERE/'matrix/serial-checks-v2.json')['exits']==[1,1,0]
    for config in ['Debug','Release']:assert load(HERE/('compatibility/'+config+'-v3.json'))['cases']==29424
    save('final/git-final-v1.json',dict(utc=utc(),branch='codex/04A-42',HEAD=MAIN,main=MAIN,merge_base=MAIN,index=pin('.git/index'),staged_empty=True,diff_check_exit=0,modified=git('diff','--name-only').decode().splitlines(),untracked_at_receipt=git('ls-files','--others','--exclude-standard').decode().splitlines(),final_membership='External receipt binds exact later final candidate membership/bytes.',raw40_paths_absent=True,staging_commit_merge_rebase_push_release=False))
    state=dict(status=STATUS,kind='OWNER_ACCEPTANCE_REVIEW_READINESS_ONLY',effective_only_with_matching_external_receipt=True,base=MAIN,prior=PRIOR,branch='codex/04A-42',owner_accepted=False,merged=False,phase04A43_authorized=False,scientific_corrections=0,native_activations=0,implementation=dict(production_files=8,Windows_files=4,Common_files=4,new_Common_public_methods=10,new_Common_classes=0,changed_exported_parents=14,scoped_exports=90,total_exports=268,new_test_files=2,new_passing_test_identities=20),software=dict(Debug=debug['summary'],Release=release['summary'],full_Release='NOT_RUN',Python=py['summary'],Node='INHERITED_916_PASS;NOT_RERUN',strict_profile_MkDocs=[1,1,0],failed_first_freeze=pin(HERE/'matrix/failed-freeze-v1-disposition.json')),compatibility=dict(sealed_per_configuration=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458,accepted39_cases=77,accepted40_contracts=48,GEE_elapsed_only_fields=4,accepted41_probes=390,new_current_main_cases=1802,excluded_new_runtime_fields=['Session GUID text','Created UTC timestamp text'],original_queries=0,scientific='NOT_GOLDEN'),privacy='Both unchanged original full-candidate scanners zero;matching external receipt must prove final metadata and every ZIP member/index zero.',historical='14 DebugFAIL;113 PythonFAIL/11ERROR;three UDF-diff guardsFAIL;39privacy1941each;W3privacyERROR;54+32+21 diagnostic histories remain unwaived. Accepted41 targeted Release failure/message/optimized sequence drift retained,not rerun by the42 family subset.',investigations=dict(count=16,canonical_entries=1169,original_obligations=1514,restricted_controls=802,mappings=2967,original_controls_discharged=0),preservation=pin(HERE/'preservation/readback-v1.json'),limitations='One retained FA_FACTORABILITY source-exception ordinary probe is not successful output. Representative1802 are same-engine compatibility,not exhaustive domain/native/scientific proof. All changed parents have materialized success and adverse cases.',gates=GATES)
    save('final/readiness-postseal-v1.json',state)
    save('final/decision-matrix-v1.json',dict(state,readiness=pin(HERE/'final/readiness-postseal-v1.json'),gate_evidence=[pin(HERE/p) for p in ['build-and-tests/Debug-identities-v2.json','build-and-tests/Release-identities-v2.json','python/parent-event-reconciliation-v2.json','python/diagnostic-dispositions-v1.json','python/source-guard-dispositions-v1.json','privacy/repository-candidate-v1.json','preservation/readback-v1.json','register/issue-delta-v1.json','scope/portable-member-lock-v1.json']]))
    pycounts=py['summary']['parent_outcomes'];changes=len(load(HERE/'python/diagnostic-dispositions-v1.json')['rows'])
    text=f'''# Phase 04A.42 owner review

{STATUS} is effective only with the matching external delivery receipt proving final candidate/privacy/archive readback. The owner has not accepted or merged04A.42.04A.43 is not authorized.

Verified main/HEAD/merge-base `{MAIN}` is the direct child of accepted41 base `{PRIOR}`. Entry was clean; branch is `codex/04A-42`. Index is unchanged and empty of staged changes. The living plan was still2.18; exact2.18 is preserved as a new snapshot and2.19 records owner acceptance of41/authority for42, retaining all226 original IDs, row scopes, deferred-defect charter and chronology. Ten source newline renderings in the owner41 merge are individually authenticated against sealed41 bytes;exact originals remain protected. No obsolete source preimages were replayed.

Eight existing production files changed: four Common services and the four scoped Windows UDF modules. Ten public methods extend existing services;all pre-existing Common method bodies remain exact. Fourteen exported parent bodies changed. All90 scoped exports(70 multivariate,4 classification,10 PSM,6 plot-source),all268 product exports,names/signatures/defaults/order/attributes remain exact per configuration. No Core numerical/geometry algorithm,GUI/native renderer,framework/package,existing test or JS/Office contract changed. FIT/DROP/CLEANUP,cache globals,handle lookup,workbook/session/recalculation/concurrency lifetime remain Windows-owned and byte/method-locked for04A.43. There is no new Office/WASM/Mac UI capability.

WaveA moves factor contribution preparation,hierarchical membership cut dispatch and discriminant training/validation aliases into existing Common services. PCA,CA/MCA,KMeans and other already portable projections are retained without churn. WaveB consolidates PSM named-table payload dispatch,classification engine-backed output preparation and KM/calibration source orchestration. Histogram Pretty behavior and ROC ordering/direction/Core adapters stay unchanged;no04A.75 toggle,new matching rule,bin rule or statistical formula is introduced. Native Excel argument/error/range conversion and header/option/error ordering remain at their actual original adapter positions.

| Executed final verification | Result |
|---|---|
| Full Debug corrected freeze |21163 PASS /14 identical inherited FAIL;21177 identities;all21157 accepted41 identities plus20 new PASS |
| Full Release graph / affected contracts |Nine-step compilation PASS;6589 PASS;full Release managed suite NOT_RUN |
| Existing adapter integration |198 PASS with original assertions unchanged |
| Sealed compatibility |29424 exact per configuration:28966 PRE_EDIT_FROZEN +458 approved LATE_ARCHIVED_ENTRY_SUPPLEMENT;zero original queries/rebaseline |
| Cumulative fixtures |77 accepted39 exact;48 accepted40 contracts exact with four GEE.CompTime fields disclosed;390 accepted41 exact;1802 executed new current-main probes exact except session GUID/Created UTC text |
| Python |832 PASS /113 FAIL /11 ERROR over956 exact parents;all185 FAIL+11 ERROR events reconciled;{changes} current diagnostic differences individually dispositioned |
| Node |Inherited916 PASS;NOT_RERUN;no JS/Office DTO/compiled host-contract dependency changed |
| Strict/profile/MkDocs |1/1/0;strict/profile remain NONPASS |
| Privacy/delivery |Both original actual-candidate scanners and every physical ZIP member/index must be zero in the matching external receipt;standalone reader and16 adverse controls |

The first integration freeze recorded21161 PASS/16 FAIL and targeted Release6587 PASS/2 FAIL. Two new extraction regressions were corrected: unknown PSM table selection read missing handle state eagerly,and a discriminant wrapper changed an existing directly pinned prepared-owner dependency. Exact failed source copies/results and failed reporting/discovery attempts remain. Corrected source was refrozen;full Debug,Release build/affected tests,ABI/fixtures and compatibility were repeated because those demonstrated dependencies changed. There were two full Debug campaigns,not one disguised campaign;one consolidated Python campaign. Existing tests and historical expectations were never edited.

The1802 current-main cases were actually executed against sealed entry Debug assemblies before production edits. They cover all90 exported bodies,real FIT engines/handle reads,cultures Invariant/sk-SK,seed173,ordinary/reversed/filtered/duplicate-label data,weighted/tied thresholds,header variants,nonzero lower bounds,invalid inputs and repeat reads after caller mutation. Complete raw values/IEEE bits/shapes/errors remain protected;review records explicitly replace only generated session GUID and Created UTC strings. These new cases are neither the original38 PRE_EDIT_FROZEN corpus nor an independent scientific reference. FA_FACTORABILITY returns the retained IndexOutOfRangeException in ordinary probes;it is unchanged,canonically linked and not counted as a successful payload. Legacy/Core/adapter tests provide additional finite/empty/row/copy context;no exhaustive native/domain claim follows from same-engine compatibility.

Sixteen interim canonical investigations preserve1169 issues,1514 obligations,802 restricted controls and2967 mappings. Included are C17/C18 CA no-label indexing/activation,PCA caption,MCA category ownership,C19 cluster row display,C20 DA/FA row identity/prior/fold candidates,all five C25 candidates,C14 finite input and the two real UDF38-D42 canonical entries. CA GUI no-label indexing remains dormant;UDF table import is a different route. Physical source IDs,model-local positions,active leaves,category permutations and duplicate labels remain distinct. No row ID is guessed. GUI reentrancy/default intent,Love missing-threshold/all-missing axes,raw-text imports,DA priors/folds and FA factorability bounds require their original exact policy/scientific/native gates. No scientific/native correction,original obligation/control discharge,04A.48 final closure,independent R/golden promotion or live native campaign occurred.

All14 Debug failures,113 Python FAIL/11 ERROR parents,the three historical no-UDF-diff guards,the original39 privacy1941 findings each(1599+342),W3 privacy ERRORs and54+32+21 diagnostic histories remain unwaived. Current candidate privacy PASS does not recolor any old raw/scanner record. The accepted41 targeted Release failure with optimized line-number drift remains a FAIL in preserved evidence;the42 affected subset did not rerun it. Full Release and Node omissions are explicit. Strict/profile exit1 are actual nonpasses.

Fresh preservation verifies14493 entry tracked paths except the eight authorized runtime source transitions and one plan transition;468 checkpoint originals/copies(464 live exact/four earlier authorized newline renderings);8984 listed historical paths;1906 raw40 pins;6810 prior raw41 pins plus a complete current41-store recount;1353 source-history identities with accepted40/41/42 successors;31 sealed40 newline recoveries and ten sealed41 source newline recoveries. Source preimages for all eight changed files are exact entry-byte copies. Continuous physical file identity remains NOT_VERIFIED;788 historical drifts remain. W3's37484-path/154-archive union is inherited qualified,not freshly audited. Protected TestResults,raw reports,old failed attempts and original privacy rules/ignore scope were not overwritten or pruned. The two unsafe40 raw Git paths remain absent;their protected originals and review-v2 successors remain exact.

Exact executed command labels/exits/raw artifact SHA provenance are in matrix/actual-commands-review-v1.json;authoritative successor recipes are listed there. Append-only writers must not be rerun over existing receipts. Direct verify-beshstatng.ps1 and -Legacy were NOT_RUN because their fixed TestResults/Codex/Core.trx,Common.trx andLegacy.trx targets collide with protected historical evidence. The unchanged isolated accepted runner compiled the full graph and ran the actual full Debug/affected Release identities instead. Existing accepted3.14.7 and packages were reused;no install.

Manual smoke checks NOT_RUN: existing PCA/cluster/DA source-row versus worksheet spill comparison;PSM Love/classification/KM/calibration source-table versus native report comparison. Live Excel/COM/UIA/RefEdit,Mac/browser/Office UI,R,signing/install/release were not run. Nothing staged,committed,merged,rebased or pushed. Owner alone accepts/merges42;the next decision is owner review of this restricted software package. Any named scientific/native correction remains a separate exact decision;43 is unauthorized.

P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A43_NOT_AUTHORIZED
'''
    for path in [HERE/'final/owner-review-v1.md',ROOT/'docs/architecture/batch-04A-42-owner-review.md']:
        with path.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
    save('final/physical-gate-policy-v1.json',dict(kind='MATCHING_EXTERNAL_FINAL_SEAL_REQUIRED',external_receipt='.test-rationalization-04A42.tmp/publication/external-delivery-receipt-v1.json',index='.review-index.json',index_self_hash='Excluded from own member map;included in external inventory and both physical scans.',scope='Every physical member/index/metadata;both original scanners,no allowances',gates=GATES))
    print('Restricted42 owner-review/readiness prepared;external seal still required',flush=True)

def package_files():
    rel=['entry/review-authentication-v1.json','entry/Debug-compiled-v1.json','entry/Release-compiled-v1.json','matrix/source-freeze-v2.json','matrix/failed-freeze-v1-disposition.json','matrix/serial-checks-v2.json','matrix/actual-commands-review-v1.json','planning/reconciliation-v1.json','scope/UDF-inventory-v1.json','scope/ownership-parity-v1.json','scope/gui-current-member-crosswalk-v1.json','scope/portable-member-lock-v1.json','source/preimages-v1.json','register/issue-delta-v1.json','register/executed-context-crosswalk-v1.json','build-and-tests/Debug-identities-v2.json','build-and-tests/Release-identities-v2.json','waves/corrected-adapters-v2.json','python/parent-event-reconciliation-v2.json','python/diagnostic-dispositions-v1.json','python/source-guard-dispositions-v1.json','privacy/repository-candidate-v1.json','preservation/readback-v1.json','preservation/source-history-reconciliation-v2.json','preservation/new-raw-review-v1.json','fixtures/current-main-review-v1.json','fixtures/Observe42.cs','final/git-final-v1.json','final/readiness-postseal-v1.json','final/decision-matrix-v1.json','final/owner-review-v1.md','final/physical-gate-policy-v1.json','review_reader.py','source-review-v1.patch']
    for config in ['Debug','Release']:rel+=['abi-cil/'+config+'-proof-v3.json','abi-cil/'+config+'-exports-review-v1.json','compatibility/'+config+'-v3.json']+['fixtures/'+config+'-'+family+'-v3.json' for family in ['39','40','41','42']]
    source=load(HERE/'matrix/source-freeze-v2.json');files=[HERE/p for p in rel]+[ROOT/r['after']['path'] for r in source['source_delta']]+[ROOT/r['path'] for r in source['new_tests']]+[ROOT/source['plan']['path'],ROOT/source['plan_snapshot']['path'],ROOT/'docs/architecture/batch-04A-42-owner-review.md']
    files += [ROOT/r['public_exact_source_copy']['path'] for r in load(HERE/'source/preimages-v1.json')['files']]
    return sorted(set(files))

def seal():
    check_sources();reports=scan('repository-final-full');files=package_files();rows=[];out=PRIVATE/'publication';out.mkdir();archive=out/'BESHStatNG_04A42_OWNER_REVIEW_v1.zip'
    with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
        for p in files:
            n=p.relative_to(ROOT).as_posix();physical.safe_name(n);data=p.read_bytes();rows.append(dict(path=n,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data)&0xffffffff:08x}'));z.writestr(n,data)
        z.writestr('.review-index.json',json.dumps(dict(kind='04A42_SANITIZED_OWNER_REVIEW',files=rows,self_hash_policy='Index excluded from own map,externally pinned/scanned as physical member.'),indent=2)+'\n')
    archive.chmod(stat.S_IREAD);physical_reports=[]
    with zipfile.ZipFile(archive) as z:
        members=physical.inventory(z);assert len(members)==len(rows)+1 and len({unicodedata.normalize('NFKC',r['path']).casefold() for r in members})==len(members)
        for phase in ['13','14']:
            m=physical.scanner(phase);m.ROOT=physical.ZipRoot(z,'utf-8-sig');found,reviewed=m.scan(z.namelist());p=out/('physical-scanner-'+phase+'-v1.json');save(p,dict(source=pin(m.__file__),findings=found,reviewed=reviewed,members=len(members)));p.chmod(stat.S_IREAD);assert not found,'UNRESOLVED_PHYSICAL_PRIVACY_FINDING'
            physical_reports.append(dict(scanner=phase,source=pin(m.__file__),physical_members=len(members),index_included=True,findings=0,unresolved=0,owner_context_applied=True,scan_AST_sha256=sha(physical.function_ast(m,'scan').encode()),raw=pin(p)))
    reader=module('reader42_final',HERE/'review_reader.py');readback=reader.verify(archive);controls=reader.adverse(archive);assert len(controls)>=12
    check_sources()
    for report in reports:
        m=physical.scanner(report['scanner']);assert m.paths()==[r['path'] for r in report['members']]
        for r in report['members']:exact(r)
    for r in load(PRIVATE/'new-raw-inventory-v1.json')['files']:exact(r)
    receipt=dict(status=STATUS,utc=utc(),archive=pin(archive),readiness=pin(HERE/'final/readiness-postseal-v1.json'),owner_review=pin(HERE/'final/owner-review-v1.md'),architecture_review=pin(ROOT/'docs/architecture/batch-04A-42-owner-review.md'),decision_matrix=pin(HERE/'final/decision-matrix-v1.json'),issue_delta=pin(HERE/'register/issue-delta-v1.json'),source_freeze=pin(HERE/'matrix/source-freeze-v2.json'),full_actual_candidate_scans=reports,physical_scans=physical_reports,physical_members=members,readback=readback,adverse_controls=controls,all_final_candidate_bytes_membership_rechecked_after_zip=True,all42_raw_inventory_rechecked=True,index=pin('.git/index'),staged_empty=True,diff_check_exit=0,HEAD=MAIN,main=MAIN,merge_base=MAIN,branch='codex/04A-42',owner_accepted=False,merged=False,phase04A43_authorized=False,excluded=['Protected raw logs/TRX/private diagnostics','Owner tokens/unsalted identifier fingerprints','Binaries and giant compiled metadata/reverse indices','Complete historic TestResults'],gates=GATES)
    receiptpath=out/'external-delivery-receipt-v1.json';save(receiptpath,receipt)
    for phase in ['13','14']:
        m=physical.scanner(phase);found,_=m.scan([receiptpath.relative_to(ROOT).as_posix()]);assert not found,'EXTERNAL_RECEIPT_PRIVACY_FINDING'
    receiptpath.chmod(stat.S_IREAD);assert reader.verify(archive,receiptpath)['SHA_size_CRC']=='PASS'
    print(json.dumps(dict(status=STATUS,archive=pin(archive),external_receipt=pin(receiptpath),members=len(members),adverse_controls=len(controls))),flush=True)

if __name__=='__main__':
    try:{'prepare':prepare,'documents':documents,'seal':seal}[sys.argv[1]]()
    except Exception:
        if sys.argv[1]=='seal':save(PRIVATE/('publication-adverse-'+datetime.datetime.now().strftime('%Y%m%d%H%M%S')+'.json'),dict(status='NOT_READY_04A42',reason='Final candidate/physical/readback gate did not complete;conditional readiness ineffective without matching PASS external receipt.',gates=GATES))
        raise
