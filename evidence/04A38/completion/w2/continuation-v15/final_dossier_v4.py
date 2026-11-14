"""Post-campaign bookkeeping only; never execute tests or edit production."""
from common import *
from collections import Counter
load=lambda p:json.loads(p.read_bytes())
for row in load(HERE/'production-test-freeze-v3.json')['sources']:assert pin(ROOT/row['path'])==row
comparison=pin(HERE/'compatibility-packet-v3.json');prior=load(PREVIOUS/'frozen-provenance-v1.json')
save('frozen-provenance-v1.json',dict(configurations=prior['configurations'],comparison=comparison,original_authority=pin(PREVIOUS/'frozen-provenance-v1.json'),new_original_queries=0,scientific_truth='NOT_GOLDEN',scope='28,966 genuine pre-edit and458 separately approved late A-G frozen inputs/original outputs/recorders remain exact. Fresh corrected current-only comparison in Debug and Release.'))
save('release-execution-receipt-v1.json',dict(Release_full_unit_suites='NOT_RUN_OWNER_PREFERENCE',Release_selected_unit_suites='NOT_RUN_OWNER_PREFERENCE',Debug_full_campaigns=2,authorized_replacement_Debug_campaigns=1,final_Debug_campaigns=1,
 Debug_shipping=pin(HERE/'build-packet-v3-Debug.json'),Release_shipping=pin(HERE/'build-packet-v3-Release.json'),Release_metadata=pin(HERE/'final-v1/current-Release.json'),Release_exports=pin(HERE/'final-v1/exports-Release.json'),dual_configuration_fresh_compatibility=comparison,restore='NOT_RUN',install='NOT_RUN',native='NOT_RUN',scientific_oracle='NOT_RUN',production_packaging_signing_deployment='NOT_RUN',original_queries=0))
rows=[]
for d in load(HERE/'compiled-method-deltas-v1.json')['configurations']:
 body=[];context=[]
 for r in d['changed']:
  if '<PrivateImplementationDetails>' in r['method']:
   strip=lambda m:{k:v for k,v in m.items() if k not in ['raw_IL_sha256','signature','nonliteral_static_fields']}
   assert strip(r['original'])==strip(r['current'])
   context.append(dict(method=r['method'],old_static_field_count=r['original']['nonliteral_static_fields'],new_static_field_count=r['current']['nonliteral_static_fields'],body_changed=False,reason='Compiler-added static char-array data for extracted text splitter. Exact instructions/locals/parameters/exceptions remain equal.'))
  else:body.append(r['method'])
 assert len(body)==11 and len(context)==1 and len(d['added'])==14 and not d['removed'] and d['Core_changed']==0
 rows.append(dict(configuration=d['configuration'],changed_native_bodies=body,changed_native_body_count=11,compiler_context_only=context,new_portable_public_methods=12,new_private_constructors=2,removed_methods=0,Core_changed=0))
save('compiled-boundary-reconciliation-v1.json',dict(configurations=rows,full_metadata=pin(HERE/'final-inspection-v1.json'),full_constructed_signatures=pin(HERE/'compiled-full-signatures-v2.json'),original_queries=0))
fr=load(HERE/'new-parent-fragment-crosswalk-v3.json');assert not fr['unmatched_branch_inline']
save('fragment-reconciliation-receipt-v1.json',dict(source_crosswalk=pin(HERE/'new-parent-fragment-crosswalk-v3.json'),original_fragments=len(fr['rows']),matched=sum(bool(r['matches']) for r in fr['rows']),whole_parent_source_composite_only=sum(not r['matches'] for r in fr['rows']),unmatched_branch_inline=0,counts=load(HERE/'ownership-summary-v2.json')['fragments'],current_fragment_denominator_before=14450,current_fragment_denominator_after=14178,reduction=272,denominator_reason='Exact original IDs retained. Statement/branch fragments genuinely move out of the Windows726-member census into separately enumerated Common source; four original GEE properties already have inherited constructed Common implementations and no current Windows IDs.',original_queries=0))
old=load(PREVIOUS/'canonical-identity-status-v1.json')['entries'];new=load(HERE/'canonical-identity-status-v1.json')['entries']
assert sum(r['prior_supported'] for r in new)==1281
save('prior-closure-retention-v1.json',dict(previous=pin(PREVIOUS/'canonical-identity-status-v1.json'),current=pin(HERE/'canonical-identity-status-v1.json'),retained=dict(historical=496,current=529,support=256),C14_new=dict(historical=35,current=35,support=3),C14_four_ROC='ALL_RETAINED_EXACT_SUPPORTED_BODY_HASHES',GuessPositiveClassAfterConversion='UNCHANGED_POSTCONVERSION_FALSE_THROW_ALIAS_CONTRACT',old_resetting_Common_API='UNCHANGED_SOURCE_AND_COMPILED_CONTRACT',reopened=0,original_queries=0))
commands=[]
for folder in sorted((RUNROOT/'commands').glob('w2c15-*')):
 path=folder/'command.json'
 if not path.is_file():continue
 doc=load(path)
 if doc.get('exit_code')!=0:
  commands.append(dict(label=folder.name,command=pin(path),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),exit_code=doc.get('exit_code'),interpretation='Raw nonzero retained; test nonpasses remain nonpasses. Tool/reader attempts retain their exact versioned source and successor correction.'))
save('failed-attempt-ledger-v1.json',dict(records=commands,retained_first_full_Debug=pin(HERE/'managed-full-execution-v1.json'),final_replacement=pin(HERE/'managed-full-execution-v2.json'),replacement_authority=pin(HERE/'owner-replacement-debug-authorization-v1.json'),semantic_defect=pin(HERE/'cox-baseline-text-compare-defect-v1.json'),semantic_correction=pin(HERE/'cox-baseline-text-compare-correction-v1.json'),interactive_failures='Early interactive command quoting/import/fragment KeyError failures are preserved in the conversation tool stream where no local Runner record was made; no fabricated local raw capture is claimed. Versioned failed source scripts remain in the review.',older_failures='All inherited failed attempts, original C14 v1 seven-part/index attempts and the .git/index reader issue remain immutable.',original_queries=0))
validation=load(HERE/'validation-v1.json')
save('nonpasses-final-v1.json',dict(managed=load(HERE/'managed-Debug-identities-v2.json')['nonpasses'],python=load(HERE/'python-first-failures-v2.json')['records'],auxiliary=validation['strict_profile_MkDocs'],prior=pin(PREVIOUS/'nonpasses-final-v1.json'),first_and_replacement=pin(HERE/'managed-authorized-replacement-reconciliation-v1.json'),privacy='Both inherited JSON ERROR identities remain ERROR. The malformed4,213,129,746-byte v9 aggregation remains exact and review-only excluded. The separate unordered-SPC ERROR is not relabelled.',original_queries=0))
save('final-source-validation-binding-v1.json',dict(freeze=pin(HERE/'production-test-freeze-v3.json'),managed=pin(HERE/'managed-validation-v2.json'),compatibility=comparison,Python_frozen_pre_post=pin(HERE/'python-frozen-input-postcheck-v1.json'),Node=pin(HERE/'node-identities-v1.json'),all_sources_unchanged=True,original_queries=0))
q=load(HERE/'remaining-work-queue-v2.json')['first_members'];nextlines='\n'.join(str(i+1)+'. `'+r['original_id']+'`' for i,r in enumerate(q))
text=f'''# Continuation v15 owner review — partial W2 checkpoint

NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED.

This checkpoint is not Phase04A.38 acceptance and authorizes neither W3 nor04A.39. No Git staging, commit, reset, merge, rebase or push was performed. The authorized Master Plan remains2.14; exact2.13/2.14 predecessor projections remain versioned and immutable.

## Entry authority and exact closure population

The corrected final C14 v2 seven-part index, postseal additive handover, final Git/index and post-publication receipts were authenticated before implementation. Actual final C14 checkpoint/publication/preservation/private/physical-part readers and the required C13 private reader passed; the interrupted initial publication-reader attempt remains explicit. See entry-authentication-v2.json and entry-queue-receipt-v1.json.

Historical supported537/704 (167OPEN); current supported566/726 (160OPEN); distinct support supported262/303 (41OPEN). Added41 historical,37 current and6 support identities. All496/529/256 prior supported identities remain supported, including the four C14 ROC bodies. Four original GEE profile/time properties have distinct owner-bound constructed Common counterparts and no fabricated current Windows member identities. UdfRegressionOutput remains OPEN because BuildResidualMatrixOutput remains OPEN; GEEUDFs also remains OPEN.

The final current ledger is current-ownership-v3.json and the exact next queue is remaining-work-queue-v2.json. Earlier v2/v1 drafts retain stale inherited child lists; their original bytes remain preserved. The corrected child lists come directly from the actual current census, with no change to closure dispositions, counts or production source. The original/current fragment status ledgers remain exact.

## Approved changed files

- `BESHStat.Excel.Common/src/Analysis/UdfCoxInputPreparation38.vb`
- `BESHStat.Excel.Common/src/Results/UdfCoxGeeReportPreparation38.vb`
- `BESHStat.Excel.Common/src/Analysis/UdfModelImportPreparation38.vb`
- `BESHstatNG/src/ExcelUDFs/Regression/CoxUDFs.vb`
- `BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb`
- `BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImport.vb`
- `BESHStat.Excel.Common.Tests/src/UdfContinuation15Tests.vb`
- `BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb`

## Portable extraction and complete ownership

Existing Cox record preparation and Cox/GEE report assembly moved into typed Common helpers. The GEE loose numeric text branch moved into existing Common input preparation. Existing Core survival/GEE engines and statistical semantics were reused; Core method bodies did not change. Windows retains Excel argument import/evaluation/errors, cache/handle lifetime, workbook/materialization, exception/Err/logging, plotting, conversions and native policies. Twelve public Common methods and two private utility constructors were added. Eleven native compiled bodies changed; a separate compiler static-field census change is explicitly not counted as a changed method body.

All complete102 native parent bodies,13 callbacks, four original properties,91 reached Common contracts, five cache/data class contexts and1053 transitive executable methods per configuration are reconciled. The Cox single-line lambda has its exact original identity, typed Func(Int32,Int32) construction and compiled binding. The display callback is the unchanged actual native ExpForDisplay method. Full original/current source and all real branches/inline fragments are reconciled; source/CIL signatures, generic construction, defaults, ByRef, alias and exception boundaries are retained in the accompanying receipts. No parent was closed merely because it calls a helper.

## Real defect, correction and authorized replacement

Full source/CIL review exposed a v15 extraction defect: six Cox baseline comparisons had inherited binary Common comparison instead of the original native text policy. A failing current-only focused regression was retained, then the six comparisons were corrected through the existing LegacyUdfText38 policy. Width/case/culture regressions pass. First frozen source and first comprehensive Debug campaign remain immutable as PRE_CORRECTION_NONFINAL; their passing results were insufficient to establish this semantic boundary.

The owner explicitly approved one replacement Debug campaign. It was consumed exactly once after corrected source freeze. There are two comprehensive Debug campaigns in history and one final authorized replacement; no third campaign was run. Final managed outcomes:20,990PASS /14 inheritedFAIL among21,004 identities;64 newPASS. Every20,940 C14 identity/outcome/nonpass message remains exact. All first-campaign20,999 identities remain reconciled, with five added text-policy regressions.

Debug and Release compiled offline without restore. Final current-only frozen comparisons are29,424 exact per configuration:28,966 genuine pre-edit cases plus458 separately approved A-G LATE_ARCHIVED_ENTRY_SUPPLEMENT cases, preserving original inputs/outputs/provenance. Actual public268 export ABI/attributes/help/defaults and full constructed signatures/CIL were checked in both configurations. Full and selected Release unit suites are NOT_RUN_OWNER_PREFERENCE.

## Other checkpoint outcomes and limits

Python: {validation['Python']['methods']} among956 complete unique parent identities, at most six worker/helper processes; nonpass subtest events are separately recorded as {validation['Python']['nonpass_events']}. A conversation interruption terminated the first scanner lane after14 starts and13 printed outcomes, before its final40-parent report.916 completed parents were retained without rerunning them. The owner explicitly approved one replacement40-parent scanner lane. There are972 known parent starts,971 completed outcomes including the13 partial headers, and one interrupted unknown outcome. The956 effective complete records reconcile exactly; the13 partial outcomes are separately retained and agree with their replacement outcomes. Six interrupted FAIL messages had not been emitted before termination and remain unknown, rather than being copied from a prior run. Complete current traces/messages, worker/input-freeze and scanner controls remain available. ERROR remains ERROR, including both privacy JSON errors and the separate unordered-SPC error. The owner separately approved two focused privacy corrections after the accelerator translated a helper UTF-8 exception into AssertionError FAILs. Those first raw FAILs remain retained. The correction reuses authenticated successful per-file results, leaves the failed binary candidate uncached, and executes unchanged original scanner/assertions without another bulk scan. Actual focused ERROR type and precedence are recorded separately from the immutable v14 JSON ERROR chronology. No historical assertion was weakened or waived. Node:916PASS (468Office /71resolver /377WASM-node). Strict/profile/MkDocs exit codes are1/1/0, not three passes.

No archived original source/reference execution or recompilation, new compatibility oracle, R/scientific/native Excel/COM/UIA/RefEdit/manual check, production Office/Mac/WASM/browser gate, package restore/install, production XLL/MSI packaging, signing or deployment validation was performed. The requested Excel/WASM manual smoke checks are NOT_RUN_OWNER_PROHIBITION. The general verification wrapper was not run because it would repeat the exactly-once campaigns and includes unauthorized restore/broader workflows; the explicitly authorized offline direct commands and actual raw exits are retained instead.

All historical seals, original41 UDF snapshots,278 lineage pins,100 mixed-model captures,16 owner documents,15,166 inherited private pins and85 inherited physical archives remain protected. No ignored historical TestResults tree was traversed/deleted. The malformed v9 aggregation remains4,213,129,746 bytes /3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb. Review-only exclusions do not authorize deleting any retained private evidence. Review ZIP byte hashes require the actual locally produced bytes; no external upload or independent reviewer execution is claimed.

## Exact next historical queue

The complete167/160/41 ordered OPEN complements are in remaining-work-queue-v2.json. The next24 historical identities are:

{nextlines}

Final additive postseal authority and the external publication/private-preservation receipts bind actual final reader commands, raw streams, Git/index and physical archive hashes without rewriting this review or prior seals.
'''
with (HERE/'owner-review.md').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
print('Final dossier written without source/test changes')
