from publication_helpers import *
import base64,io

def scan(label):
 reports=candidate_scan(label)
 for r in reports:
  m=physical.scanner(r['scanner']);m.ROOT=ProbeRoot(json.dumps(dict(value='benign portable review control')));found,_=m.scan(['negative-control.json']);assert not found
  r['negative_control']=dict(zero_findings=True,no_identity_tokens_written=True)
  for i,token in enumerate(sorted(physical.tokens())):
   payload=base64.b64encode(('A'*90+' '+token+' '+'Z'*90).encode()).decode()
   m=physical.scanner(r['scanner']);m.ROOT=ProbeRoot(json.dumps(dict(payload=payload)));a,ar=m.scan(['encoded-control.json']);m.re=re;b,br=m.scan(['encoded-control.json'])
   assert a==b and ar==br and any(x['category']=='known_private_identifier' and x['origin']=='decoded_payload' for x in a)
   r['identity_controls'].append(dict(identity_ordinal=i,mode='base64_decoded_payload',detected=True,direct_original_and_guard_exact=True))
 return reports

def prepare():
 check_sources();files=[pin(p) for p in sorted(PRIVATE.rglob('*')) if p.is_file()]
 for r in files:(ROOT/r['path']).chmod(stat.S_IREAD)
 save(PRIVATE/'new-raw-inventory-v1.json',dict(files=files,count=len(files),bytes=sum(r['bytes'] for r in files),scope='All43-owned raw files after executed campaigns/before seal. Later privacy/publication receipts are pinned independently. Older raw stores unchanged.'))
 (PRIVATE/'new-raw-inventory-v1.json').chmod(stat.S_IREAD)
 failures=[]
 for p in sorted((PRIVATE/'failed-attempts').glob('*.json')):
  d=load(p);failures.append(dict(recipe=d['recipe'],type=d['type'],raw=pin(p),status='RETAINED_FAILED_ATTEMPT'))
 save('preservation/new-raw-review-v1.json',dict(kind='SANITIZED_INVENTORY_FACTS;NOT_RAW_LOGS',inventory=pin(PRIVATE/'new-raw-inventory-v1.json'),files=len(files),bytes=sum(r['bytes'] for r in files),read_only=True,failed_attempts=failures,old_stores_mutated=False,discovery_failures='Read-only interactive quoting/discovery errors remain in tool history;no fabricated receipts. Own failed capture process stopped only after executable/PID attribution.'))
 commands=[]
 for p in sorted((PRIVATE/'commands').glob('*/command.json')):
  d=load(p);commands.append(dict(label=p.parent.name,exit_code=d['exit_code'],seconds=d['seconds'],raw=pin(p),stdout_sha256=d['stdout_sha256'],stderr_sha256=d['stderr_sha256']))
 save('matrix/actual-commands-review-v1.json',dict(kind='SANITIZED_EXECUTED_COMMAND_FACTS;NOT_RAW_ARGV_TRANSCRIPT',commands=commands,recipe_directory='evidence/04A43/',interpreter='Existing accepted3.14.7 and packages;no installs',full_Debug_campaigns=1,full_Python_campaigns=1,full_Release='NOT_RUN',Node='INHERITED_916_PASS;NOT_RERUN',direct_verify_script='NOT_RUN_FIXED_PROTECTED_TRX_COLLISION;UNCHANGED_ISOLATED_RUNNER_EQUIVALENT',Excel_COM_UIA_R_Office_WASM_manual='NOT_RUN',failed_attempts_retained=True,stage_commit_merge_rebase_push_release=False,commands_to_reproduce='Use existing interpreter -I -B evidence/04A43/run43.py <recipe> [arguments];append-only writers require fresh version/directory names. Recipes and exact safe command labels are authoritative;raw argv remains controlled.',key_recipes=['authenticate-v1.py','entry-build-v1.py Debug/Release','entry-compiled-v1.py','capture-v3.py','capture-races-v1.py','wave-a-v1.py','wave-b-v1.py','close-wave-b-v4.py','final-managed-v1.py Debug/Release','compiled-and-fixtures-v1.py Debug/Release','compatibility-v1.py Debug/Release','python-campaign-v1.py prepare/execute','reconcile-python-v1.py','python-dispositions-v2.py','preservation-v2.py','serial-checks-v1.py','key-edge-readback-v1.py']))
 reports=scan('repository-initial')
 save('privacy/repository-candidate-v1.json',dict(status='ZERO_FINDINGS_FOR_RECORDED_ACTUAL_FULL_CANDIDATE_INPUTS',reports=reports,waivers=0,rules_ignore_and_scope_unchanged=True,owner_tokens_and_unsalted_identifier_hashes_published=False,history='Accepted39 FAIL1941each(1599+342);W3 privacyERROR;old40 raw failures;54+32+21+25 histories remain. Current privacy PASS is not historical waiver.',later_final_metadata='External receipt pins/rechecks full final candidate including later metadata. No self-hash claim.',gates=GATES))
 print('Protected43 inventory sealed;both unchanged actual-candidate scanner passes complete',flush=True)

def documents():
 freeze=check_sources();debug=load(HERE/'build-and-tests/Debug-identities-v1.json');release=load(HERE/'build-and-tests/Release-identities-v1.json');py=load(HERE/'python/parent-event-reconciliation-v2.json');pres=load(HERE/'preservation/readback-v1.json')
 assert debug['summary']['outcomes']==dict(Passed=21218,Failed=14) and all(r['classification']=='RETAINED_IDENTICAL' for r in debug['nonpasses'])
 assert release['summary']['outcomes']==dict(Passed=373) and not py['summary']['unresolved_diagnostic_events'] and py['summary']['parent_outcomes']==dict(PASS=832,FAIL=113,ERROR=11)
 assert load(HERE/'matrix/serial-checks-v1.json')['exits']==[1,1,0]
 for config in ['Debug','Release']:assert load(HERE/('compatibility/'+config+'-v1.json'))['cases']==29424
 save('final/git-final-v1.json',dict(utc=utc(),branch='codex/04A-43',HEAD=MAIN,main=MAIN,merge_base=MAIN,index=pin('.git/index'),staged_empty=True,diff_check_exit=0,modified=git('diff','--name-only').decode().splitlines(),untracked_at_receipt=git('ls-files','--others','--exclude-standard').decode().splitlines(),final_membership='External receipt binds every actual final candidate path/byte including later metadata.',raw40_paths_absent=True,staging_commit_merge_rebase_push_release=False,preexisting_owner_edits=[]))
 state=dict(status=STATUS,kind='OWNER_REVIEW_READINESS_ONLY',effective_only_with_matching_external_receipt=True,base=MAIN,prior=PRIOR,branch='codex/04A-43',owner_accepted=False,merged=False,phase04A44_authorized=False,scientific_corrections=0,native_activations=0,cached_model_semantics_changed=False,live_Excel='NOT_RUN',implementation=dict(existing_Windows_runtime_files=3,new_Common_source_files=1,new_test_files=2,test_Compile_include=1,changed_public_parents=8,cache_families=18,cache_modules=12,handle_exports=174,total_exports=268,new_PASS_identities=55),software=dict(Debug=debug['summary'],Release=release['summary'],full_Release='NOT_RUN',Python=py['summary'],Node='INHERITED_916_PASS;NOT_RERUN',strict_profile_MkDocs=[1,1,0]),compatibility=dict(sealed_per_configuration=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458,accepted39=77,accepted40=48,accepted41=390,accepted42=1802,current43_operations=1118,real_fit_graphs=36,generated_tokens=74,bounded_race_rows=864,simulated_policy_cases=15,precisely_qualified_runtime_cells=92,late_sealed_entry_key_edges=30,original_queries=0,scientific='NOT_GOLDEN'),privacy='Both unchanged actual-candidate and all physical-member/index scans zero in matching external receipt;no allowances.',historical='14 DebugFAIL;113 PythonFAIL/11ERROR;three historical UDF-diff guards FAIL;39privacy1941each;W3privacyERROR;54+32+21+25 prior and21 current diagnostic differences retained. Accepted41 targeted Release failure/optimized sequence drift remains FAIL outside this subset;42 first-freeze regressions and failed evidence preserved.',preservation=pin(HERE/'preservation/readback-v1.json'),native_limitations='Global session dictionaries are not demonstrated workbook-bound. No new workbook/session epoch acquisition/event hook;simulated verified epochs are policy-only. No automatic cleanup/rebinding/capacity/TTL/LRU/disposal. Dictionary atomics do not prove mutable model safety or serializable Count-Clear.',required_next_gate='Owner review/acceptance of43;any changed workbook invalidation,eviction,error/key grammar or native behavior requires separate exact decision and applicable native gate.44 not authorized.',gates=GATES)
 save('final/readiness-postseal-v1.json',state)
 save('final/decision-matrix-v1.json',dict(state,readiness=pin(HERE/'final/readiness-postseal-v1.json'),gate_evidence=[pin(HERE/p) for p in ['build-and-tests/Debug-identities-v1.json','build-and-tests/Release-identities-v1.json','python/parent-event-reconciliation-v2.json','python/diagnostic-dispositions-v1.json','python/source-guard-dispositions-v1.json','privacy/repository-candidate-v1.json','preservation/readback-v1.json','register/issue-delta-v1.json','scope/portable-ownership-lock-v1.json']]))
 text=f'''# Phase 04A.43 owner review

{STATUS} is effective only with the matching external delivery receipt binding final ZIP bytes, every physical member/index, final full Git-candidate privacy scans and executed adverse controls. The owner has not accepted or merged04A.43.04A.44 is not authorized.

Verified accepted42 merged main/HEAD/merge-base is `{MAIN}`, direct child of historical premerge `{PRIOR}`. Entry was clean. Branch `codex/04A-43` was created from that main;the index remains unchanged with nothing staged. Nine sealed42 source newline transitions in the owner merge were authenticated separately,with exact sealed originals protected;no obsolete preimages replayed. Living entry Rev2.19 is preserved exactly as MasterPlan2.19.md;Rev2.20 records accepted42/current43,retaining all226 original roadmap IDs,the detailed Rev2.12 objective and the binding deferred-defect charter.

The implementation changes three existing Windows files: ExcelArgModules,MultivariateAnalysisUDFs and PropensityScoreUDFs. Their blank-key/blank-or-ALL predicates delegate to one host-free Common contract. One explicit test Compile registration and two new test files add55 passing identities. All18 original Windows dictionaries and all18 FIT method bodies/order remain exact. Eight exported DROP/CLEANUP parent bodies change only those equivalent predicates. All174 handle exports and268 product exports preserve names,argument metadata/defaults/order,return types,attributes and actual reflected volatility/thread flags in Debug and Release. No Core numerical code,GUI/native renderer,JS/Office DTO,framework target,existing test assertion or cache storage/timing operation changed.

Common provides immutable descriptors and pure handle syntax/validity/lifecycle decisions. Family/GUID grammar is diagnostic,never a new rejection rule for legacy stored keys. Host-provided verified epochs can support simulated invalidation decisions;the current Windows adapter acquires no such epochs. Workbook/session binding remains NOT_ESTABLISHED. Native workbook close/reopen,recalc/add-in lifetime and manual FIT/recalculate are NOT_RUN. No new events,automatic rebind/refit,purge,eviction,TTL/LRU/caps,disposal,Office runtime or Custom Functions are activated. Cached-model semantics changed: none.

| Fresh final check | Actual outcome |
|---|---|
| Full Debug,one post-freeze campaign |21218 PASS /14 identical inherited FAIL;21232 identities;21177 accepted42 retained plus55 new PASS |
| Complete Release compilation graph |PASS;373 affected tests PASS;full Release managed suite NOT_RUN |
| Exports/CIL |268 metadata-identical/config;all methods outside declared3 Windows/new Common owners exact |
| Historical frozen compatibility |29424 exact/config:28966 PRE_EDIT_FROZEN +458 approved LATE_ARCHIVED_ENTRY_SUPPLEMENT;zero original queries/rebaseline |
| Accepted cumulative fixtures |77 +48 +390 +1802/config preserved with original exclusions unchanged |
| New actual handle fixtures |1118 operations,36 real-fit graphs across18 families/two cultures,74 actual generated tokens;grammar/uniqueness/A-B object and reference binding checked before symbolization |
| Controlled concurrency |864 bounded get/drop/clear rows/config;allowed invariants/identities preserved,not equal schedules;parallel real PCA FIT/read-read association/copy checks |
| Policy-only simulation |15 compiled Common cases/config;zero native callbacks;fake verified generations explicitly simulated |
| Supplementary key edges |30/config exact against authenticated sealed entry;late measurement,not an invented pre-edit or historical oracle |
| Consolidated Python,one campaign |832 PASS /113 FAIL /11 ERROR;956 exact parents;196 nonpass events:175 identical +21 changed-message dispositions;no lost parents/events |
| Strict/profile/MkDocs |1/1/0;strict/profile NONPASS |
| Node |Inherited916 PASS;NOT_RERUN;73 JS files and compiled Office-facing dependencies unchanged |

Before production edits,actual current-main FIT/query/drop/cleanup executions were captured as CURRENT_MAIN_PRE_EDIT_04A43. They test correct/wrong family,blank/missing/error keys,case/space,unknown/malformed keys,repeated query,caller mutation,drop repeat/query-after-drop,independent identical-data FITs and PSM blank/ALL cleanup. Current successor captures reuse the driver's schema label but are explicitly POST_EDIT_CURRENT,not additional pre-edit references. The comparator validates the real A/B/C token graph and object association before substituting symbolic references,including the derived wrong-family GUID suffix edge. Ninety-two individually listed source-reached clock cells/footnote numbers are qualified;raw values remain protected. No broad generated-handle regex exclusion substitutes for relational proof. The preserved FA_FACTORABILITY source exception is not a successful payload. These are same-engine software compatibility,not independent scientific validation.

Concurrency tests use barriers/bounded timeouts and actual model fits. Public clear exists only for LMM/MMRM/PSM;other family clear interleavings are explicitly controlled host-primitive simulations. Dictionary atomics do not prove concurrent mutation safety of fitted objects. Captured references can survive a subsequent DROP;Count-then-Clear is not serializable removal accounting. Session-global dictionaries may retain models across workbook use;workbook isolation/closure and automatic invalidation are not established. Repeated FIT retains independent old keys. These risks remain restricted rather than receiving unproved native guarantees.

The canonical delta joins accepted39/40/41/42 and preserves1169 entries,1514 original obligations,802 restricted controls and2967 mappings. Three exact canonical entries cover UDF38-D43 cache lifecycle,C25 reentrant snapshot and retained UDF38-D42 FA factorability;no invented issue/control IDs or04A.48 final exit. Proposed workbook isolation/eviction,model mutability/thread-safety and native event changes need exact separate decisions. All scientific/native gates remain. Accepted42 unknown PSM named-table lazy lookup and direct discriminant owner dependency are exact source locks.

All14 Debug failures,the113 Python FAIL/11 ERROR parents,three historical no-UDF-diff guards,the39 privacy1941 findings each(1599+342),W3 privacyERROR and54+32+21+25 historical changed-diagnostic records remain unwaived. The21 current differences are individually explained by literal branch/main/plan,authorized source pin or historical changed-path set context;assertions remain unchanged. Accepted41 targeted Release failure with optimized sequence-point drift remains a FAIL outside this subset. Accepted42 first integration failure21161 PASS/16FAIL and6587 PASS/2FAIL,and its corrected final campaigns,remain protected. New43 failed capture/report/comparator attempts are retained;no production regression was hidden or historical assertion edited.

Fresh preservation rehashes14634 entry tracked files with only the authorized3 runtime/test Compile/plan transitions;468 earlier originals/copies(464 live exact/four prior authorized newline renderings);8984 listed historical paths;1906 raw40 pins;6810 original raw41 and11457 original raw42 pins plus current store recounts({pres['current41_raw_count']} and{pres['current42_raw_count']});1353 original source-history identities and authorized successors;31 sealed40 and10 sealed41 recoveries and nine owner42 newline transitions. Continuous physical identity remains NOT_VERIFIED and788 historical drifts remain. W3's37484/154 union is inherited qualified,not freshly audited. Protected historical TestResults/rawTRX/preimages/failures/scanners/ignore rules remain unchanged. Both unsafe40 raw Git paths remain absent;protected exact originals and safe review-v2 successors remain exact.

Exact command labels/exits/provenance are in matrix/actual-commands-review-v1.json;raw arguments/diagnostics are controlled. Existing WinPython3.14.7/packages and unchanged isolated runner were used;no installs. Direct verify-beshstatng.ps1/-Legacy are NOT_RUN because their fixed Core/Common/Legacy TRX targets collide with protected historical files;the isolated equivalent compiles the complete graph and executes actual managed identities. Reproduction recipes are append-only and require new versioned output paths,not overwriting receipts. The small review archive contains sources/preimages/patch,typed fact/identity manifests and independently executable review_reader.py;no raw model data/tokens/dumps/binaries/giant reverse indices/private tracebacks.

Manual smoke NOT_RUN:fit a cached-model UDF and recalculate;query/drop/cleanup across workbook close/reopen. These require separately attributed native execution and COM-risk gate. R,Excel/COM/UIA/RefEdit,Mac/browser/Office UI,signing/install/release were NOT_RUN. Nothing staged,committed,merged,rebased or pushed. Owner alone accepts/merges43. Next gate is owner review of this restricted software package;any named behavior-changing cache/native correction requires its own exact decision.04A.44 remains unauthorized.

P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A44_NOT_AUTHORIZED
'''
 for p in [HERE/'final/owner-review-v1.md',ROOT/'docs/architecture/batch-04A-43-owner-review.md']:
  with p.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
 save('final/physical-gate-policy-v1.json',dict(kind='EXTERNAL_MATCHING_RECEIPT_REQUIRED',receipt='.test-rationalization-04A43.tmp/publication/external-delivery-receipt-v1.json',index='.review-index.json',index_self_excluded=True,index_externally_inventoried_and_scanned=True,all_members_and_metadata_both_scanners=True,gates=GATES))
 print('Restricted43 owner review/readiness prepared;matching external seal still required',flush=True)

def package_files():
 rel=['entry/review-authentication-v1.json','matrix/source-freeze-v1.json','matrix/serial-checks-v1.json','matrix/actual-commands-review-v1.json','planning/reconciliation-v1.json','scope/cache-export-inventory-v1.json','scope/cache-export-matrix-v1.json','scope/portable-ownership-lock-v1.json','source/preimages-v1.json','register/issue-delta-v1.json','build-and-tests/Debug-identities-v1.json','build-and-tests/Release-identities-v1.json','build-and-tests/Debug-executed-v1.json','build-and-tests/Release-executed-v1.json','waves/wave-b-v4.json','python/discovery-v1.json','python/parent-event-reconciliation-v2.json','python/diagnostic-dispositions-v1.json','python/source-guard-dispositions-v1.json','privacy/repository-candidate-v1.json','preservation/readback-v1.json','preservation/new-raw-review-v1.json','fixtures/relational-review-v1.json','fixtures/volatile-fields-v1.json','fixtures/reference-normalization-v3.json','fixtures/key-edge-readback-v1.json','fixtures/Observe43V3.cs','fixtures/Race43.cs','fixtures/Policy43.cs','fixtures/KeyEdges43.cs','policy/recalculation-strategy-v1.json','policy/Debug-simulation-v1.json','policy/Release-simulation-v1.json','abi-cil/exports-review-v1.json','abi-cil/effective-registration-flags-v1.json','final/git-final-v1.json','final/readiness-postseal-v1.json','final/decision-matrix-v1.json','final/owner-review-v1.md','final/physical-gate-policy-v1.json','review_reader.py','source-review-v1.patch']
 for c in ['Debug','Release']:rel+=['abi-cil/'+c+'-proof-v1.json','compatibility/'+c+'-v1.json']+['fixtures/'+c+'-'+f+'-v1.json' for f in ['39','40','41','42','43','43-races']]
 freeze=check_sources();files=[HERE/p for p in rel]+[ROOT/r['after']['path'] for r in freeze['source_delta']]+[ROOT/r['path'] for r in freeze['new_production']+freeze['new_tests']]+[ROOT/freeze['plan']['path'],ROOT/freeze['plan_snapshot']['path'],ROOT/'docs/architecture/batch-04A-43-owner-review.md',ROOT/'evidence/04A42/fixtures/Observe42.cs']
 files+=[ROOT/r['public_exact_source_copy']['path'] for r in load(HERE/'source/preimages-v1.json')['files']]
 return sorted(set(files))

def seal():
 check_sources();reports=scan('repository-final-full');files=package_files();rows=[];out=PRIVATE/'publication';out.mkdir();archive=out/'BESHStatNG_04A43_OWNER_REVIEW_v1.zip'
 with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
  for p in files:
   n=p.relative_to(ROOT).as_posix();physical.safe_name(n);b=p.read_bytes();rows.append(dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}'));z.writestr(n,b)
  z.writestr('.review-index.json',json.dumps(dict(kind='04A43_SANITIZED_OWNER_REVIEW',files=rows,self_hash_policy='Index excluded from own map,externally hashed/inventoried/scanned as physical member.'),indent=2)+'\n')
 archive.chmod(stat.S_IREAD);physical_reports=[]
 with zipfile.ZipFile(archive) as z:
  members=physical.inventory(z);assert len(members)==len(rows)+1 and len({unicodedata.normalize('NFKC',r['path']).casefold() for r in members})==len(members)
  for phase in ['13','14']:
   m=physical.scanner(phase);m.ROOT=physical.ZipRoot(z,'utf-8-sig');found,reviewed=m.scan(z.namelist());p=out/('physical-scanner-'+phase+'-v1.json');save(p,dict(source=pin(m.__file__),findings=found,reviewed=reviewed,members=len(members)));p.chmod(stat.S_IREAD);assert not found,'UNRESOLVED_PHYSICAL_PRIVACY_FINDING'
   physical_reports.append(dict(scanner=phase,source=pin(m.__file__),scan_AST_sha256=sha(physical.function_ast(m,'scan').encode()),physical_members=len(members),index_included=True,findings=0,unresolved=0,owner_context_applied=True,raw=pin(p)))
 reader=module('reader43_final',HERE/'review_reader.py');readback=reader.verify(archive);controls=reader.adverse(archive);assert len(controls)>=16
 # Real privacy-leak adverse control: altered in-memory physical ZIP through both ORIGINAL scanners, never written/published.
 for phase in ['13','14']:
  buf=io.BytesIO()
  with zipfile.ZipFile(buf,'w') as z:z.writestr('.review-index.json',json.dumps(dict(control='A'*90+' '+sorted(physical.tokens())[0])))
  buf.seek(0)
  with zipfile.ZipFile(buf) as z:
   m=physical.scanner(phase);m.ROOT=physical.ZipRoot(z,'utf-8-sig');f,_=m.scan(z.namelist());assert any(r['category']=='known_private_identifier' for r in f)
  controls.append(dict(control='physical_privacy_leak_original_scanner_'+phase,detected=True,owner_token_published=False))
 check_sources()
 for report in reports:
  m=physical.scanner(report['scanner']);assert m.paths()==[r['path'] for r in report['members']]
  for r in report['members']:exact(r)
 for r in load(PRIVATE/'new-raw-inventory-v1.json')['files']:exact(r)
 primary=[pin(ROOT/'docs/architecture/batch-04A-43-owner-review.md')]+[pin(HERE/p) for p in ['final/decision-matrix-v1.json','scope/cache-export-matrix-v1.json','fixtures/relational-review-v1.json','register/issue-delta-v1.json','preservation/readback-v1.json']]
 receipt=dict(status=STATUS,utc=utc(),archive=pin(archive),primary_reports=primary,readiness=pin(HERE/'final/readiness-postseal-v1.json'),full_actual_candidate_scans=reports,physical_scans=physical_reports,physical_members=members,readback=readback,adverse_controls=controls,index_self_excluded_but_external_full_inventory=True,all_final_candidate_bytes_membership_rechecked_after_zip=True,all43_preseal_raw_inventory_rechecked=True,index=pin('.git/index'),staged_empty=True,diff_check_exit=0,HEAD=MAIN,main=MAIN,merge_base=MAIN,branch='codex/04A-43',owner_accepted=False,merged=False,phase04A44_authorized=False,excluded=['Protected raw logs/TRX/diagnostics','Owner identity tokens and unsalted identifier hashes','Model data and fitted dumps','Binaries/giant reverse indices/full historic TestResults'],gates=GATES)
 rp=out/'external-delivery-receipt-v1.json';save(rp,receipt)
 for phase in ['13','14']:
  m=physical.scanner(phase);f,_=m.scan([rp.relative_to(ROOT).as_posix()]);assert not f,'EXTERNAL_RECEIPT_PRIVACY_FINDING'
 rp.chmod(stat.S_IREAD);assert reader.verify(archive,rp)['SHA_size_CRC']=='PASS'
 print(json.dumps(dict(status=STATUS,archive=pin(archive),external_receipt=pin(rp),members=len(members),adverse_controls=len(controls))),flush=True)

if __name__=='__main__':
 try:{'prepare':prepare,'documents':documents,'seal':seal}[sys.argv[1]]()
 except Exception:
  if sys.argv[1]=='seal':save(PRIVATE/('publication-adverse-'+datetime.datetime.now().strftime('%Y%m%d%H%M%S')+'.json'),dict(status='NOT_READY_04A43',reason='Final candidate/physical/readback gate did not complete;conditional readiness ineffective without matching external PASS receipt.',gates=GATES))
  raise
