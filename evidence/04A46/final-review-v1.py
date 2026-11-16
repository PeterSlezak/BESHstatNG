from common46 import *
import collections,difflib,xml.etree.ElementTree as ET
lock();d=load(HERE/'scope/coverage-v3.json');census=load(HERE/'scope/family-census-v1.json')
prior=load(HERE/'scope/family-review-v1.json')
columns=prior['member_columns']
prior.update(members=[[r.get(k) for k in columns] for r in d['members']],counts=d['counts'],source_projected_methods=162,
 additional_xyz_event_bindings=11,additional_xyz_observer_cases_per_configuration=44,
 limitations='162 exact copied shipping bodies exercised under supplied fake states:147 managed projections,4 separate helper observers,11 separate XYZ event observers. 334 other members remain explicitly native pending. No native constructors/defaults/clamping/auto-radio/focus/COM/rendering proved.343 new managed identities;22+44 extra observed assertions/config are not extra managed identities.')
save('scope/family-review-v2.json',prior)
scenario=load(HERE/'fixtures/scenario-contract-v1.json');scenario.update(source_projected=162,additional_xyz_bindings=[dict(owner=r['owner'],handler=r['method'],control=r['control'],event=r['event']) for r in load(HERE/'fixtures/xyz-observer-v1.json')['members']],additional_xyz_cases_per_configuration=44,current_cases='343 passing managed identities plus22 helper and44 XYZ observer cases/config;all source-derived,not native or golden.')
save('fixtures/scenario-contract-v2.json',scenario)
state=dict(status='READY_FOR_OWNER_04A46_SAFE_GUI_WIRING_REVIEW__NATIVE_GATE_NOT_AUTHORIZED',effective_only_with_matching_external_receipt=True,HEAD=MAIN,main=MAIN,merge_base=MAIN,branch='codex/04A-46',owner_accepted46=False,live_native='NOT_RUN',actual_HWND_PID_XLL='NOT_VERIFIED',approval='NOT_APPROVED',native_adapter=False,original_controls_discharged=0,original_obligations_discharged=0,next_state='04A46_NATIVE_EXECUTION_PENDING_OWNER_APPROVAL',next_batch='04A47_NOT_AUTHORIZED',gates=GATES)
assert load(HERE/'python/dispositions-v1.json')['unresolved']==0
save('final/readiness-v1.json',state)
tracked={r['path']:r for r in load(PRIVATE/'tracked-entry-inventory-v1.json')['files']}
shipping=sorted({r['path'] for r in census['members']}|{r['path'] for r in census['fields']}|{
 'BESHstatNG/BESHStatAddIn.vb','BESHstatNG/Resources/RibbonXML.xml','BESHstatNG/src/UI/Refedit/RefEditEventLease.vb',
 'BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb','BESHstatNG/src/AppInfrastructure/WindowsExecutionRun.vb',
 'BESHstatNG/src/AppInfrastructure/WindowsDocumentLifecycle.vb','BESHstatNG/src/AppInfrastructure/ExcelDnaHostInfrastructure.vb',
 'BESHStatNG.Test/src/WindowsHarness45.vb'})
for p in shipping:exact(tracked[p])
save('final/source-review-v1.json',dict(shipping_sources=[pin(p) for p in shipping],entry_source_hashes={p:tracked[p]['sha256'] for p in shipping},unchanged_shipping_code=True,production_patch=[],source_kind='CURRENT_MAIN_PRE_EDIT_04A46_SOURCE_DERIVED;NOT_NATIVE;NOT_GOLDEN',transform='Exact original method texts and substitutions separately supplied;numeric CheckNumeric Object becomes FakeControl46. No original branch or message correction.'))
original=PRIVATE/'source-preimages/BESHStatNG.Test/BESHStatNG.Tests.vbproj'
if not original.is_file():original=PRIVATE/'source-preimages-v1/BESHStatNG.Test/BESHStatNG.Tests.vbproj'
assert original.is_file(),'TEST_PROJECT_PREIMAGE_MISSING'
dest=HERE/'source/BESHStatNG.Tests.pre46.vbproj';dest.parent.mkdir(parents=True,exist_ok=True)
with dest.open('xb') as f:f.write(original.read_bytes())
patch=git('diff','--','BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan.md').decode()
for r in load(HERE/'matrix/source-freeze-v1.json')['new_tests']:
 patch+=''.join(difflib.unified_diff([], (ROOT/r['path']).read_text(encoding='utf-8-sig').splitlines(keepends=True),fromfile='/dev/null',tofile='b/'+r['path']))
with (HERE/'source/full-review-v1.patch').open('x',encoding='utf-8',newline='\n') as f:f.write(patch)
old=load(ROOT/'evidence/04A45/build-and-tests/Debug-identities-v1.json')
save('final/accepted45-test-identities-v1.json',dict(baseline=pin('evidence/04A45/build-and-tests/Debug-identities-v1.json'),tests=[{k:r[k] for k in ['suite','identity_sha256','occurrence','outcome']} for r in old['tests']],nonpasses=old['nonpasses'],historical_scope_status='FAIL_UNWAIVED'))
with (HERE/'entry/accepted45-receipt.review.json').open('xb') as f:f.write((ROOT/'.test-rationalization-04A45.tmp/publication/external-delivery-receipt-v1.json').read_bytes())
save('final/inspector-invocation-v1.json',dict(mode='REFLECTION_ONLY_METADATA_AND_IL_NOT_RUNTIME_BINDING',arguments_contract='LIBRARY_DIRECTORY_AND_NEW_JSON_ONLY',native_or_product_initializers_invoked=False,source=pin('evidence/04A45/fixtures/InspectWiring45.cs'),resource_source=pin('evidence/04A45/fixtures/ResourceBoundary45.cs'),reports=[pin(HERE/'entry/compiled-identities-v1.json'),pin(HERE/'abi-cil/UI-and-attribution-v1.json')],safe_default='Metadata and IL inspection without product instances/static constructors,Excel registration or host invocation;separate observers execute only closed fake types.',real_loaded_XLL_proof='NOT_VERIFIED'))
save('privacy/publication-policy-v1.json',dict(required_scanners=['13','14'],waivers=0,full_candidate_scope='ORIGINAL_UNCHANGED_PATHS_FUNCTIONS',physical_scope='EVERY_MEMBER_INCLUDING_INDEX',success_requires_external_receipt=True,original_scanner_sources=[pin('scripts/verify-04a13-publication.py'),pin('scripts/verify-04a14-publication.py')],guard=pin('evidence/04A39/physical-witness-v1/dependencies/privacy_regex_guard.py'),guard_qualification='Original sources/scan/paths unchanged;accepted regex performance proxy. Owner-local literal/case/NFKC/Base64 positives and clean negative compare decisions with original regex. Neutral replay separate from actual owner identity proof.',raw_private_tokens_published=False))
debug=load(HERE/'build-and-tests/Debug-identities-v1.json');release=load(HERE/'build-and-tests/Release-identities-v1.json')
save('final/software-verification-v1.json',dict(Debug=debug['summary'],Release=release['summary'],full_Release='NOT_RUN',ABI_exports_per_configuration=268,compiled_proof=pin(HERE/'abi-cil/UI-and-attribution-v1.json'),CIL_production_scope='Complete UI/Ribbon/RefEdit emitted metadata and inspected ABI owners exact45;all production source bytes exact authenticated merged main.',compatibility_per_configuration=dict(total=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458,new_original_queries=0),retained_current_cases=dict(batch39=77,batch40=48,batch41=390,batch42=1802,batch43_operations=1118,batch43_graphs=36,batch43_races=864,batch43_policy=15,batch44=591,batch44_fake_progress=72),strict_profile_MkDocs=[1,1,0],Python=dict(PASS=834,FAIL=111,ERROR=11,parents=956,changed_diagnostic_events=13,unresolved=0),Node='INHERITED_916_PASS;NOT_RERUN',managed_Office_contracts='FRESH_FULL_DEBUG_PASS',new_managed_identities=343,additional_observer_cases_per_configuration=66,live_Excel_COM_UIA_RefEdit='NOT_RUN',scientific='NOT_GOLDEN',gates=GATES))
attempts=[]
for p in sorted((PRIVATE/'failed-attempts').glob('*.json')):
 a=load(p);attempts.append(dict(recipe=a.get('recipe'),type=a.get('type'),raw=pin(p)))
save('final/failed-attempts-v1.json',dict(retained=True,attempts=attempts,scope='All failures through preparation;later publication failures additionally pinned in external receipt.',wave_A='a1 compile;a2 reflection121 failures;a3 private-constructor/forwarder14 failures;a4 135PASS.',wave_B='b1 encoding/reserved-name compile;b2 duplicate-field compile;b3 host-alias compile caught before execution;b4 37 expectation failures;b5 312PASS;b6 one Shapiro minimum expectation failure;b7 337PASS;b8 343PASS.',freeze='Failed preparation receipts retained;source-freeze-v1 was first completed operational seal;v2 same generation plus adverse preparation lineage. No test/project/production changes after seal. Evidence-only closed XYZ observer adds11 pure event bodies without altering frozen project.',production_corrections=0,deleted_protected_evidence=0))
save('final/git-final-v1.json',dict(branch='codex/04A-46',HEAD=MAIN,main=MAIN,merge_base=MAIN,index=pin('.git/index'),staged_empty=True,diff_check_exit=0,modified=git('diff','--name-only').decode().splitlines(),added_at_preparation=git('ls-files','--others','--exclude-standard').decode().splitlines(),final_membership='External receipt binds exact final candidate and index after physical readback.',commits_merges_rebases_pushes_releases=False))
print('Final software maps and safe review facts prepared',flush=True)
