"""Append-only technical checkpoint, not whole-batch acceptance."""
from datetime import datetime,timezone
from common import *
o=json.loads((HERE/'ownership-summary.json').read_bytes());v=json.loads((HERE/'validation.json').read_bytes());t=json.loads((HERE/'source-transitions-final.json').read_bytes());coverage=json.loads((HERE/'fragment-coverage-addendum.json').read_bytes());queue=json.loads((HERE/'remaining-work-queue-v2.json').read_bytes())
gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN')
save('readiness.json',dict(status='NOT_READY_04A38',W1_G_GATE='HISTORICAL_APPROVAL_G_COMBINED_GATE_PASS_UNCHANGED',W2='IN_PROGRESS',W3='NOT_STARTED',ownership=o,validation=v,new_original_queries=0,original_metadata_only=True,owner_decision_needed=False,gates=gates,earliest_blocker='573 historical behavioral bodies remain OPEN; 583 current bodies and69 distinct support declarations remain OPEN. Exhaustive in-scope migration/whole-body/branch/inline closure is not complete.',next_action=queue['first_action'],scope='Substantive implemented checkpoint; no W2/W3 completion, owner acceptance, commit or merge',created_UTC=datetime.now(timezone.utc).isoformat()))
table='\n'.join('| '+r['path']+' | '+str(r['before']['bytes'])+' | '+r['before']['sha256']+' | '+str(r['after']['bytes'])+' | '+r['after']['sha256']+' |' for r in t['files'])
nonpasses=json.loads((HERE/'nonpasses.json').read_bytes());failures='\n'.join('- '+r.get('suite','')+': '+r.get('identity',r.get('test_name',''))+'; '+str(r.get('outcome','Failed')) for r in nonpasses['managed'])
review=f'''# Phase04A.38 W2 continuation-v3 — NOT_READY_04A38

W2 IN_PROGRESS; W3 NOT_STARTED. This execution implemented the requested naming seam and removed the two accidental coordinate-class helpers. It adds11 supported historical body dispositions; it does not complete the41-source migration. No new original fit/query/capture was executed. A–G remain historical late supplements, never retroactive pre-edit or scientific controls.

## Actual implementation and boundaries

UdfSpillCell38 lost only its unused StackWithBlankRow and NormalizeBlanks public methods. Its coordinate properties/constructor and both canonical UdfSpillPreparation38 implementations remain unchanged. The prior compiled incoming-call count to the accidental methods is0 in Debug and Release. The removed surface was introduced during this unaccepted batch; source/project and compiled consumer review found no dependency. Arbitrary external reflection consumers cannot be exhaustively enumerated. No registered Excel UDF signature changed. See duplicate-surface-before.json and independent-names-proof.json.

UdfMetadataPreparation38.vb now owns UdfNamePreparation38 and UdfVariableNameRange38. ResolveNames retains native missing-argument priority and GetVariableNames acquisition. Explicit names are filled in-place using actual returned length; detected names are copied; fallback retains original zero/negative allocation and current-culture ordinal behavior. GetVariableNames delegates its exact fallback/comma/cardinality/row-before-column policy to Common. Its native callbacks retain Get2D, missing/blank/Excel-error interpretation and Convert.ToString(cell).Trim() timing. Only typed strings, dimensions and a typed accessor cross the portable boundary. Common receives no raw ExcelError or workbook object.

The preexisting native Get2D/WindowsSpreadsheetTransport path copies a rectangular range into a detached zero-based array, honoring source lower bounds. Scalar values are rejected by this path. Every selected cell is converted before count-mismatch fallback; throwing/null-returning ToString remains exceptional. The original/current transport helpers have exact complete resolved IL in both configurations and are independently pinned to Git/source. Native current-only tests exercise the real shipping methods through typed delegates, both cultures, conversion effects/errors, array identity, source snapshot ownership, range orientation and unusual lower bounds. No new original reference was used.

production-consumer-edges.json lists actual fresh Debug/Release native call/newobj targets, including ResolveNames→FillExplicitNames/ResolveDetectedNames, GetVariableNames→Common.GetVariableNames and its four real callbacks, and the retained StackWithBlankRow/PrepareResultTableForUdf→canonical spill calls. verify_names.py compares full original operations, host callback expressions and exception order. Metadata v5 includes full resolved instructions, locals, exception regions, generic context, fields, initializers, assembly SHA/MVID and passive original provenance. Raw metadata-token IL hashes can differ after adding MemberRefs; resolved instructions/targets/locals/EH/type context are mandatory. No broad instruction or result normalization is allowed.

## Ownership and outstanding work

Historical:131 closed /573 OPEN /704. Current:134 reviewed /583 OPEN /717, including four new naming callbacks. Support:234 closed /69 OPEN /303 distinct,325 raw. Current fragments:707 parent-covered /15,813 total;68/3,692 branch nodes and19/1,093 inline nodes are parent-covered. Original fragments:2,353/20,683 parent-covered;563/5,511 branches and126/1,397 inline nodes. Uncovered and parentless fragments remain OPEN. Headers do not close children. Full populations and canonical original↔current/parent mappings are in historical-ownership.json, current-ownership.json and original-fragment-parent-crosswalk.json. per-family-ownership.json covers all41 sources; remaining-work-queue-v2.json retains every573 OPEN historical identity.

The11 additional proofs are ResolveNames, GetVariableNames, CategoryHeaders, SafeCiLabel, SafeCiText, NormalizeMatchToken, NormalizeKey, CloneStringArray, NormalizeText, NormalizeToken and TryParseSymmetryMethod. Six existing root helpers have complete original→portable resolved IL proofs in both configurations; CI fields/getters/constructors/mutable cached formatting state are also checked. The two normalizers preserve original RuntimeHelpers.GetObjectValue→AsString acquisition prefix and original exception timing. Symmetry preserves original conversion-before-ByRef-write behavior and its entire decision/alias body, with normalizer dependencies and current fixed-RHS text-domain proof. The previous39 whole-IL delegations are retained, and all33 prior host/cache bodies plus16 type contexts and their constructors/initializers are freshly matched in both configurations.

Levene reset-before-conversion remains unchanged from C2 and is exercised by the retained current Framework tests. LegacyUdfText38 has no production change: all59 direct call lines,145 direct calls,63 CaseToken sites/733 ordered choices and76 parents were freshly inspected;1,568,736 source-domain checks/configuration show0 differences. All8 raw synthetic null/ignorable mismatches/configuration remain recorded, outside the fixed non-null RHS caller domains; there is no claim of global null/text equivalence. TryInvertMatrix remains OPEN on VB ProjectData/Err versus embedded Core runtime exceptional state; no early closure or fresh original execution.

## Fresh validation and adverse attempts

Final Core/Common/Windows isolated offline builds passed in Debug and Release. Focused final tests:82 Common +59 Framework PASS/configuration, plus18 selected Core PASS/configuration. Complete managed Debug:20,666 PASS /14 inherited FAIL;13 new passes and0 changes to prior identity/occurrence/outcome pairs. Selected Release:159 distinct PASS; full managed Release NOT_RUN_OWNER_PREFERENCE. Managed identity/occurrence records and all14 failure messages remain in managed-Debug-identities.json and the public diagnostic derivative.

Node916 PASS. Python765 PASS /180 FAIL /11 ERROR; the exact identity/subtest/outcome/multiplicity delta from C2 is empty. Strict/profile/MkDocs exits1/1/0. Source-pin assertions and inherited unordered-SPC nonpasses remain failures/errors. They are not waived or changed to green. nonpasses.json carries the detailed Python identities and all inherited receipt chronology. Canonical PowerShell portable/Legacy wrappers were inspected and NOT_RUN because they can restore/use shared outputs/package XLLs; explicit offline equivalents were used. Toolchain: Python3.14.7, Node24.20.0, .NET SDK10.0.403/MSBuild18.9.15, VS18 Roslyn5.11.0-1.26424.5. Exact commands/output digests and configurations are in command receipts and toolchain.json.

Current-only replay is exact for28,966 genuine pre-edit plus458 separate A–G late rows per configuration,29,424 total each Debug and Release. All24 original/current TSV pairs are hashed in compatibility-names-v3.json. No original fit/query/capture was rerun. Fresh source/compiled export checks preserve268/268 contracts in both configurations; source inventory remains41.

New independent source/IL/ownership/text/preservation adverse tests:26 PASS; scope/static-context/original-fragment tests:7 PASS. Publication and copied-reader results are separately versioned after this document. Historical reader replay: pre-edit28 PASS; checkpoint20; continuation12; A38; B87; C79; D88; E105; F70; G72 PASS. W1 broad historical replay47 PASS /1 inherited ERROR in test_w1.W1Tests.test_intact (the obsolete invalid v1 fixture); canonical later reader outcomes remain separately visible. Exact C2 projection: slice20, ownership9, cache8, publication14 PASS. Three initial C2 reader filename patterns discovered0 tests and exited5; these are retained nonpasses, not counted as passing executions.

Other preserved current attempts: names-v1 Debug compiler BC36641 in a new test lambda; names-v2 five incorrect new Framework fixture assertions/configuration; their source snapshots and raw receipts remain. Corrections were source-backed scalar rejection, enum CObj overload, detached array snapshots and normalized lower bounds; no original oracle changed. Final names-v3 passes. An initial unchanged-helper reader used the wrong CloneStringArray owner, then omitted the compiler's GetObjectValue prefix; corrected v3 proofs retain both prior attempts. A scope reader initially compared raw metadata-token IL hashes; v2 excludes only that raw hash and checks every resolved operation/context. The tentative nonexistent FormatDescriptiveTable queue label is explicitly withdrawn by remaining-work-queue-v2.json. No failed attempt is relabeled PASS.

## Source transitions

Only these six existing current source/test files changed during C3. Exact source-before/after snapshots, bytes and hashes are immutable under this packet; no Core numerical/project/framework change.

| Path | Before bytes | Before SHA-256 | After bytes | After SHA-256 |
|---|---:|---|---:|---|
{table}

## Preservation, publication and limitations

Live preservation independently checks4,695 tracked paths (4,661 original-entry exact +34 authorized transitions),278 lineage pins,100 protected captures,16 owner documents and41 original UDF snapshots with fresh pinned Git CRLF/LF identity. W2/C1/C2 predecessor manifests retain their exact2,134/2,644/3,124 hashed payloads and original nonrecursive null selves; prior changed live source pins resolve only through exact retained preimages plus actual current successors. The ignored historical TestResults tree was not traversed or pruned. Old failed captures, original seals, NOT_READY decisions and source pins remain unchanged. tracked-path-preservation.json and preservation.json distinguish live checks from review-copy claims.

The public derivative excludes every raw full-managed-Debug-v1.json and compiled cache, including the C2 raw diagnostic accidentally present in the owner's313MB upload. Raw diagnostics remain unchanged locally. full-managed-Debug-public-v1.json changes only two synthetic-parameter diagnostic display labels through the previously accepted safe_label policy; identities/outcomes and all scientific/source/branch bytes stay unchanged. diagnostic-public-derivative.json records pointers and hashes. The historical rejected privacy manifest is retained; excluded raw payloads cannot be rehashed from a derivative.

The prior generated220,235,190-byte C2 ZIP (SHA d879ff8ae371839d04adeb5887c366e5a29c06ea98dff412e2a731e5dc5e30ae;3,125 entries) was independently scanned in full: exact physical manifest membership, SHA/CRC and no raw diagnostic/cache. It is distinct from the owner's313,023,652-byte/7,554-entry upload. Its two pattern findings are immutable synthetic adverse fixture/CSV parameters, not actual credentials; source fixtures are included in this C3 derivative as provenance. New verification scans all physical members and decoded/escaped text, rejects extras and raw diagnostics unconditionally, and allows only exact historical fixture file hashes/match classes. This is a bounded pattern audit, not proof against every conceivable secret encoding. The new ZIP/inventory/digests/CRC/copy results are separate observed export receipts after final manifest creation.

The derivative explicitly lists inclusions/exclusions. It cannot establish the live index, omitted owner documents/private binaries/accepted-history files or138GB ignored history. Owner-upload omissions are not live deletion; all corresponding live tracked paths were present. No stale ZIP was restored.

## Next action and restricted gates

Continue ordinary authorized W2, without generic owner approval: first resolve or explicitly preserve TryInvertMatrix exceptional Err-state proof, then ParametricUDFs.ParseEqualVarianceMode and ANOVA1/ANOVA1_WELCH full report/import composition; proceed through the remaining41-source family queue. Every mandatory historical/current/support/branch/inline/statement obligation must close before W3. This checkpoint cannot support READY. No new indispensable original-reference gap was identified and no new owner decision is requested.

P=NO; N1=NO GO; N2=NO; Gate C2=UNMET; Gate C3=UNAPPROVED; NOT_GOLDEN. Native Excel/COM/UIA/RefEdit/in-workbook/manual GUI, independent R/scientific controls, Mac/Office manual smoke, network/restore/install/MSI/signing/deploy, full Release, W3 READY verifier and04A39 are NOT_RUN. Compatibility is not scientific or native validation. Git remains codex/04A-38 with HEAD/main/merge-base ba1636f5356ac761509c2cfa16fc66b92430935d; index empty, diff-check0 at recorded checks. No stage/unstage/commit/merge/push/reset/clean/rebase/branch switch or owner-document edit. Final Git/export receipts are separately versioned; Codex does not accept or merge04A.38.
'''
with (HERE/'owner-review.md').open('x',encoding='utf-8',newline='\n') as f:f.write(review)
with (HERE/'executive-summary.md').open('x',encoding='utf-8',newline='\n') as f:f.write('''# NOT_READY_04A38 — W2 continuation-v3

Implemented portable consumed ResolveNames/GetVariableNames policies with native acquisition/conversion callbacks; removed only the two accidental UdfSpillCell38 helper copies. Fresh complete operation/IL/dependency proofs support11 additional historical closures.

131/704 historical and134/717 current bodies closed;573/583 OPEN. Support234/303 closed,69 OPEN;707/15,813 current fragments covered. W2 IN_PROGRESS; W3 NOT_STARTED.

Debug20,666 PASS/14 unchanged inherited FAIL; selected Release159 PASS; Node916 PASS; Python765 PASS/180 FAIL/11 ERROR, unchanged nonpass identities; strict/profile/MkDocs1/1/0. Exact current replay28,966 genuine pre-edit +458 separate late A–G/configuration;268 compiled exports/configuration. No new original query.

Live4,695 paths/278 pins/100 captures/16 owner documents/41 original snapshots preserved. Raw diagnostics retained locally and excluded from review ZIP with all compiled caches. Actual whole-ZIP membership/privacy/CRC and copied independent checker results accompany the final manifest/export receipt.

Next: TryInvertMatrix Err-state context, then ParseEqualVarianceMode and ANOVA1/ANOVA1_WELCH. Continue authorized W2; no generic approval requested. P=NO;N1=NO GO;N2=NO;C2=UNMET;C3=UNAPPROVED;NOT_GOLDEN. No Git publication/native/scientific/04A39 work.
''')
handover='''# NEXT_WAVE_HANDOVER — C3 substantive implementation checkpoint

Continue04A.38 only, same branch/worktree. HEAD/main/merge-base ba1636f5356ac761509c2cfa16fc66b92430935d; empty index; do not stage/commit/merge/reset/clean/restore ZIPs. Recheck live Git and source-transitions-final.json, final manifest, preservation.json before edits. All6 C3 source/test preimages/successors are retained. Final shipping/current test stage names-v3; current libraries under <ISOLATED_RUN>/completion-w2-cont3/names-v3/<Debug|Release>/lib; metadata-v5/names-v3 passive source/assembly bindings. No new original semantic case, fit, query or capture is authorized.

Completed: naming contracts UdfNamePreparation38/UdfVariableNameRange38; actual native callbacks preserve Get2D/blank/error/conversion order. Both historical naming bodies closed. Duplicate coordinate helpers removed; canonical spill implementations and coordinates unchanged. Additional root/Symmetry proofs and fresh host/cache generic/static context retained. C3 branch/support/fragment readers remain fail-closed, W3 not started.

DO NOT rerun A–G originals, W1 or full managed Release. Current-only comparisons against sealed originals are authorized. No new source-oracle authority is needed for routine W2. If genuinely essential new archived behavior emerges, stop before capture/dependent edits with a bounded consolidated proposal.

Remaining:573 historical/583 current bodies,69 distinct support entries,15,106 current fragments. All exact canonical IDs in remaining-work-queue-v2.json and full ledgers. Original/current counts717 vs prior713 are reconciled by4 new native callback lambdas; all268 exports and41 sources retained. Do not infer closure from edge alone or classify pure parsing/report work HOST_ONLY.

First exact members:
'''
handover+='\n'.join('- '+r['original_id'] for r in queue['first_members'])
handover+='''

TryInvertMatrix is a source/runtime-context proof obligation: original VB ProjectData/Err versus embedded Core shim exceptional-state semantics remain OPEN; inspect source and current-only throw/state probes, never a fresh original query. Then the Parametric ParseEqualVarianceMode/ANOVA1/ANOVA1_WELCH policy/report/import dependencies are a coherent next implementation slice. Use source-before hash/snapshot BEFORE every edit; preserve common statistical engines, native marshaling and row/array/culture/ByRef behavior. Existing W1 schedule/per-family-ownership.json covers other families, not merely these methods.

Fresh final validation: Debug20,666PASS/14 inheritedFAIL, selectedRelease159PASS; Node916; Python765/180/11; strict/profile/MkDocs1/1/0; all29,424 current comparisons/config (pre-edit28,966 separate from458late). Preserve raw failures and source-pin discrepancies. Stage names-v1 compiler failure, names-v2 bad current-only fixtures, initial zero-discovery reader exits5 and scope/raw-token reader mismatch are retained. Corrected successful variants are separately versioned. TryParseSymmetryMethod is now CLOSED on complete source/normalizer/text-domain/exception-prefix proof; TryInvertMatrix stays OPEN. Levene and the59 fixed-RHS text domains remain verified;8 out-of-domain raw synthetic differences/config stay adverse observations.

Publication: exclude C2/C3 raw full-managed-Debug-v1.json and all __pycache__/.pyc/.pyo physically, not just from manifest. Keep them locally unchanged. Use included public derivative and pointer ledger. Whole ZIP scanning must reject unmanifested raw/cache/extra members and verify every actual entry. Exact-hash historical synthetic fixture exceptions include source/CSV provenance; no arbitrary file/prefix exemptions. Final ZIP identity/CRC/copy checks and Git closeout are external export receipts. Omitted live paths/private binaries/owner docs/ignored history cannot be independently verified from compact copy.

Do not create W3 until zero unsupported mandatory ownership OPEN. No owner acceptance, native/scientific control promotion,04A39, staging/commit/merge/push. See owner-review.md, validation.json, nonpasses.json, final manifest and copied verifier receipts for complete commands/counts and limitations.
'''
with (HERE/'NEXT_WAVE_HANDOVER.md').open('x',encoding='utf-8',newline='\n') as f:f.write(handover)
print('NOT_READY checkpoint documents written')
