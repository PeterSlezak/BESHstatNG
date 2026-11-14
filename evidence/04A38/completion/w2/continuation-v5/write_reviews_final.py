from common import *
v=json.loads((HERE/'validation-v3.json').read_bytes())
s=json.loads((HERE/'ownership-summary-v3.json').read_bytes())
f=json.loads((HERE/'fragment-coverage-v3.json').read_bytes())
q=json.loads((HERE/'remaining-work-queue-v3.json').read_bytes())
t=json.loads((HERE/'source-transitions-v4.json').read_bytes())
assert json.loads((HERE/'independent-controls-boundary.json').read_bytes())['passed']
assert json.loads((HERE/'retained-C4-controls-v2.json').read_bytes())['passed']
files='\n'.join('| '+r['path']+' | '+('new' if r['before'] is None else str(r['before']['bytes']))+' | '+str(r['after']['bytes'])+' | `'+r['after']['sha256']+'` |' for r in t['files'])
nexts='\n'.join('- `'+r['original_id']+'` — '+r['name'] for r in q['first_members'][:14])
own='\n'.join('| '+title+' | '+str(s[key]['closed'])+' | '+str(s[key]['OPEN'])+' | '+str(s[key]['total'])+' |' for title,key in [('Historical bodies','historical'),('Current UDF bodies','current')])
source='''# NOT_READY_04A38 — W2 advanced; W3 NOT_STARTED

The final C5 implementation is `cells-v5`. It introduces reusable legacy cell conversion and numeric type-code preparation with eight actual Windows consumer edges: three conversion/classification consumers and five prepared phases. The complete body/dependency review supports 33 additional historical identities, including ANOVA1, ANOVA1_WELCH and ANOVA1_MCP. Two already-supported grouped/nested contracts were reviewed again and are not counted as new. This remains an incomplete 41-source W2 checkpoint; no W3 work or 04A.39 began.

Final authority is `source-transitions-v4.json`, the `*-v3.json` ownership/fragment/validation/readiness ledgers, `body-contracts-v3.json`, `verify_slice_v4.py` and `verify_ownership_final.py`. Earlier C5 cells-v1/v2/v3/v4 attempts, ledgers and provisional proofs remain immutable and are explicitly superseded by these final receipts. Older C4/C3/C2/C1 packets, failures, decisions and all original references remain unchanged.

The entry branch was and remains codex/04A-38; HEAD/main/merge-base are ba1636f5356ac761509c2cfa16fc66b92430935d. The index is empty and diff-check passes. A per-command Git safe.directory setting handles the sandbox account ownership difference; no global config was changed. All 3,777 C4 payload hashes passed before source edits, together with separate preservation, full-body, ownership, retained-context and fragment checks. A combined reader finished its slow privacy scan after editing and rejected the new live source; its failed receipt is retained and the exact authenticated historical-view replay passes. Source of truth is the live worktree, never an uploaded ZIP.

The entry table's70/3678 branch and19/1093 inline fractions describe the C4 CURRENT syntax universe. Its immutable original crosswalk independently gives941/5511 branches and136/1397 inline nodes covered. The label discrepancy and C4 validation's stale text-domain descriptive counter are resolved in entry-discrepancies-v3.json without editing older records.

CellToTrimmedText keeps missing and raw ExcelError handling in Windows. Its six-type numeric formatting whitelist, invariant conversion, finite rejection, strict1e-12 rounding comparison, checked CLng overflow and invariant fallback/Trim semantics live in Common. The string branch uses DirectCast only after TypeOf String. TryGetDouble retains the original ordered missing/error/Boolean/String screening and real Framework catch in Windows; the uncaught Common conversion uses the original current-culture Convert.ToDouble followed by finite rejection. The new portable Object arguments are ByRef only to avoid a second compiler boxed-value copy at the forwarding call; they alias the host local Object and never assign it. IL proves the original single GetObjectValue numeric copy and absence of new VB conversion helpers or writes through that input. IsNumericValue keeps null/DBNull/Boolean and runtime type acquisition native and sends only TypeCode to the exact portable whitelist. Raw ExcelError does not enter these portable APIs.

An intermediate ByVal forwarding implementation added a boxed-copy layer and embedded CStr helper. Its exact source, builds, probes and provisional ledgers remain retained; the final refinement and focused/full reruns are separately versioned. The final current-C4/current-C5 probe gives198 exact observations/configuration in Invariant and sk-SK, including mutable boxed values, custom conversions/providers, call order, exceptions, signed bits, Err state and input arrays. These are current-only tests, never new archived-original cases.

A reentrant-conversion probe found a source-backed extraction defect in TryGetBinary01, TryGetWholeNumber and TryGetOptionalPositiveInteger: a custom converter can write the caller output before failed validation, and the delegated Common API reset erased that write. The original native body has no second reset. Final prepared phases preserve those writes while existing public portable APIs retain their reset wrappers. All10 current-only alias cases/configuration now retain17; adverse pre-fix results of-1,0 or10 remain recorded. No archived-original method was invoked. Grouped/nested native acquisition uses the same no-second-reset boundary. Five whole public API bodies are token-identical after inlining their prepared phase; four prepared bodies also match complete former CIL after removing only the leading reset, including rebased branch and existing For Each disposal-Finally regions. Earlier C5 assertions that no observable operation intervened were wrong; those provisional proofs remain as adverse attempts and are superseded by the final source/CIL/runtime proof.

Full grouped extraction preserves column-major numeric screening, omitted empty groups, lazy header conversion only for retained groups, compact default numbering, input/nonzero-bound behavior, output allocation and ByRef assignment order. An empty selected column still prevents automatic header detection because every selected column must have numeric data below; no header policy correction was made. Existing grouped/nested Common contracts and native callbacks were reused. The no-reset prepared phases also preserve output state after native acquisition; existing public Common APIs retain their original entry-reset behavior. The complete scalar/header/grouped/alpha/format/table contracts, original/current source identities, full CIL and child callbacks are in body-contracts-v3.json and production-consumer-edges-v3.json.

TryParseAlpha retains its fixed 0.05 missing default, conversion-before-validation order, real native catch, invariant NumberStyles.Any parse before current-culture parsing, and failure ByRef values. The p-value adapter still reads AppGlobals settings per cell and uses the unchanged Core formatter. ResultTable construction, header/body/metadata coordinates, column-first then explicit-cell formatting order, duplicate addresses, blank/error behavior and spill shapes are reviewed. Full passive original/current metadata proves209 settings, formatter, ANOVA and result-table dependency methods/configuration unchanged. The final reader also retains1,763 complete C4 method bodies/configuration and all former type/field context, except the13 explicitly changed native/public portable methods. One compiler-generated closure type is rebound by an exact name alias; every field, constructor, callback body and caller matches after that single substitution.

ANOVA1/WELCH/MCP closure covers import, group-count rejection, lazy names, alpha, selector conversion, existing engine construction/compute, report/table selection, formatting/stacking, native real catch and logging. It does not follow merely from the three report edges. The original OneWayANOVA engine and WelshANOVA implementation remain exact; no statistical engine or numerical/p-value/default/output-table correction was introduced. Existing deferred scientific/GUI/UDF candidates remain in the C4 charter and are not fixed here. EqualText/CaseToken policies and their caller-domain conclusions remain unchanged; the eight synthetic out-of-domain differences stay adverse evidence.

| Obligation | Supported | OPEN | Total |
|---|---:|---:|---:|
'''+own+'''
| Distinct support declarations |234|69|303|

Current fragments:1277/15730 covered,172/3655 branches,51/1086 inline nodes. Immutable original fragments:4021/20683 covered,1162/5511 branches,190/1397 inline nodes. Current member population remains717; no new native callback was introduced. The smaller current fragment population reflects relocated native operations, not deletion of original obligations. Every canonical identity, split/relocation mapping, support row and remaining fragment is retained. Module/class headers cannot close their children. Mandatory blockers remain 505 historical,515 current,69 distinct support,14,453 current fragments and 16,662 original fragments. W3 is prohibited by these independently recomputed nonzero counts.

All 24 sealed corpus/configuration pairs match exact bytes after the final refinement:28,966 genuinely pre-edit plus458 separately authorized late cases/configuration. A–G remain exhausted and distinct LATE_ARCHIVED_ENTRY_SUPPLEMENT provenance. No original semantic fit/query/capture/compilation occurred. Original G DLLs were read only as passive metadata and hashes. Both configurations preserve all 268 compiled Excel-DNA signatures/defaults/attributes. No new essential original-reference gap was identified or generic permission requested.

Final validation:

| Lane | Actual outcome |
|---|---|
| Managed Debug |'''+str(v['managed_Debug']['outcomes']['Passed'])+''' PASS /14 inherited FAIL;14 added PASS; no old identity/occurrence/outcome drift |
| Selected Release |'''+str(v['selected_Release']['outcomes']['Passed'])+''' distinct PASS; full Release NOT_RUN_OWNER_PREFERENCE |
| Node |'''+str(sum(r['passed'] for r in v['Node']))+''' PASS |
| Python |'''+str(v['Python']['passed'])+' PASS /'+str(v['Python']['failed'])+' FAIL /'+str(v['Python']['errors'])+''' ERROR |
| Strict / profile / MkDocs |1 /1 /0|
| Final C5 independent controls |32 PASS|
| Exact C4 historical-view controls |30 PASS|

validation-v3.json and nonpasses-v3.json preserve exact names, identities, subtests, outcomes and occurrences, including authorized source-pin failures and any inherited unordered-SPC FAIL/ERROR movement. No historical assertion was changed or waived. Failed compiler fixtures, incorrect header expectation, inspection/build race, reader executable identity rejection, relocated-identity guard and failed tooling setups remain in attempts/complete-attempt-accounting.json, attempts/late-finalization-accounting.json and raw isolated Runner receipts. Only two synthetic diagnostic display labels differ in the public full-Debug derivative; exhaustive JSON comparison proves every other field unchanged. Raw diagnostics remain protected locally and are physically excluded from the review copy.

The raw final Python aggregate was765 PASS/179 FAIL/12 ERROR. One privacy identity hit the early C5 UTF16 diagnostic/cached former path. Its exact bytes were relocated locally, with an identical-value UTF8 equivalent and explicit ledger; the unchanged targeted assertion was rerun and actually FAILS, restoring its inherited outcome. The corrected final aggregate is765/180/11, with the only remaining change from C4 being the same unordered-SPC identity moving FAIL to ERROR. Every raw attempt and occurrence remains in python-encoding-correction-final.json; no privacy assertion was waived. Diagnostic-encoding-relocation.json and local-diagnostic-preservation-v2.json bind the omitted raw bytes.

The unmodified canonical verify-beshstatng.ps1 wrappers were inspected but NOT_RUN because they restore implicitly, share TestResults and may package Legacy. Equivalent explicit offline Core/Common/Windows build/test lanes ran in isolated output directories. Commands are preserved exactly with established public path symbols in command-receipts-final.json; toolchain-final.json records the owner-configured Python and tool versions. Main execution vectors were:

```powershell
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/build.py cells-v5 Debug
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/build.py cells-v5 Release
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/focused.py cells-v5 Debug
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/focused.py cells-v5 Release
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/selected_release.py cells-v5
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/run_full_validation_v4.py managed
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/run_full_validation_v2.py source
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/run_full_validation_v4.py common-python
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/run_full_validation_v4.py serial
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/compatibility.py cells-v5
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/verify_ownership_final.py
& $PythonExe -B evidence/04A38/completion/w2/continuation-v5/verify_preservation_v3.py
```

Additional tools tests run unchanged with audit_solution's exact ignored directory policy applied before descent. Earlier inherited root.rglob runs enumerated ignored directories before filtering; that directory enumeration is not claimed absent. No ignored TestResults payload bytes were rehashed, removed or restored by C5 tooling. The final pruned lane preserves relevant source coverage and temporary-fixture behavior. See tools-pruned-validation.json.

Every existing source/test preimage was retained before editing; the new Common source's creation and exact intermediate refinement preimage are separate transitions. Final changed paths:

| Path | Before bytes | Final bytes | Final SHA-256 |
|---|---:|---:|---|
'''+files+'''

preservation-final-v3.json independently checks all 4,695 tracked paths,278 lineage pins,100 captures,16 owner documents and 41 original UDF snapshots, plus old manifests/cases/adverse records through the explicit retained-preimage/successor graph. Omitted paths in derivative ZIPs never establish live deletion. The publication manifest has a complete explicit inventory and one nonrecursive null self-entry. The external continuation-v5-publication-receipt-v1.json records the actual post-seal live reader, physical ZIP/hash/privacy/CRC, copied executable reader and mutation controls. A compact copy cannot attest omitted private DLLs, owner documents, raw diagnostics, ignored history or live Git/index; those have separate live receipts.

Next work is the exact remaining queue in NEXT_WAVE_HANDOVER.md. This is a Codex-executed compatibility/ownership checkpoint, not owner acceptance, scientific validation, native Excel readiness or Mac parity. P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN. Native Excel/COM/UIA/RefEdit, independent scientific/R, manual Office/Mac, install/restore/network/installer/signing/deployment and full managed Release remain NOT_RUN. No stage, commit, merge, rebase, push, reset, clean, branch switch or owner-document edit occurred.

Two proposed manual checks remain separately unexecuted: compare classical/Welch spill tables and names for full versus empty-middle-column inputs under Invariant/sk-SK; compare MCP ALL output/blank rows and alpha error precedence for omitted,0.05 and locale-style text alpha. No live Excel authorization is inferred.
'''
with (HERE/'owner-review.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
handover='''# NOT_READY_04A38 — W2 advanced; W3 NOT_STARTED

Continue04A.38 only, in codex/04A-38, pinned HEAD/main/merge-base ba1636f5356ac761509c2cfa16fc66b92430935d. Preserve all current changes and every earlier evidence version. Verify live branch/index/diff, source-transitions-v4.json, preservation-final-v3.json and publication-manifest-v1.json before editing. No reset/clean/restore/stage/commit/merge/rebase/push or owner-document edit.

Final shipping libraries: <ISOLATED_RUN>/completion-w2-cont5/cells-v5/<Debug|Release>/lib. Final actual CIL/export/syntax: inspection/final-v4/, dependency-metadata-v9/current-*.json, unchanged original passive v7 metadata, current-universe-final-v4.json. Source/test final preimages and after snapshots are exact; intermediate cells-v1/v2/v3/v4 and old C5 provisional artifacts remain preserved.

Final ledgers are historical-ownership-v3.json, current-ownership-v3.json, original-fragment-parent-crosswalk-v3.json, fragment-coverage-v3.json, per-family-ownership-v3.json and remaining-work-queue-v3.json. Readers are verify_slice_v4.py, verify_ownership_final.py, verify_preservation_v3.py. All 35 reviewed contracts are in body-contracts-v3.json;33 are new closures. CellToTrimmedText/TryGetDouble, scalar/header/grouped import, TryParseAlpha, metadata-bearing output/p-value helpers and ANOVA1/ANOVA1_WELCH/ANOVA1_MCP are supported. Do not reinsert them as the first OPEN queue. Grouped-complete/nested contracts were carried and reviewed again, not new closures.

Remaining 505 historical/515 current/69 distinct support OPEN.1277/15730 current fragments covered. All 704 original identities and 717 current identities remain. Do not infer child/body closure from a portable call, unchanged result, module header or generic HOST_ONLY label. Review complete caller/error/ByRef/static/callback/source dependencies. Keep original/current fragment universes distinct; the old entry branch/inline fractions were current-universe figures.

First exact canonical historical IDs:

'''+nexts+'''

Complete independent/paired/repeated numeric imports and probability-vector policy first, reusing current Common/Core preparation and native physical-alignment/Excel acquisition policies. Then finish RM/nested ANOVA and t-test parent families; continue all 41 sources through the full queue. MaterializePortableReport38 and remaining projection parents require complete metadata/error/source-reference dependency review. Add no duplicate engine or global conversion/text policy. Preserve source row identity, lower bounds, copy/name ownership, first-error/ByRef behavior and real native Framework Err catches. Native scalar conversion and grouped/nested acquisition must call the prepared Common phases after their original entry reset; a second reset can erase aliased callback writes. Existing public Common APIs keep their reset wrappers. See the retained alias-probe-before/after receipts and five whole-body source/four CIL proofs.

The C5 portable ByRef Object parameters are deliberate readonly aliases to the native local Object. Do not restore ByVal forwarding that adds a second boxed-value copy. DirectCast is valid only on the TypeOf String branch. Original Long rounding overflow, unsigned-type text fallback, fixed 0.05 alpha default/invariant-first parsing, empty-column header policy and numeric enum eligibility remain compatibility constraints. Leave scientific/GUI/UDF inconsistencies in the deferred04A.39–48 charter.

Final validation details are validation-v3.json/nonpasses-v3.json: Debug'''+str(v['managed_Debug']['outcomes']['Passed'])+'''PASS/14 inheritedFAIL; selectedRelease'''+str(v['selected_Release']['outcomes']['Passed'])+'''PASS; Node916PASS; Python'''+str(v['Python']['passed'])+'/'+str(v['Python']['failed'])+'/'+str(v['Python']['errors'])+'''; strict/profile/MkDocs1/1/0. Full Release NOT_RUN_OWNER_PREFERENCE.32 final C5 plus30 exact historical-view C4 controls pass. Retain all raw identities/occurrences and unordered-SPC FAIL/ERROR history, every compile/fixture/reader/tool failure and two-label-only diagnostic derivative proof. The pruned final tools audit uses the unchanged ignored-component policy before descent; do not broadly enumerate retained TestResults.

All 24 final current/original pairs match exactly:28,966 genuine pre-edit plus458 separate A–G late/config. No new original fit/query/capture/compilation was authorized or executed. Original DLL activity remains passive metadata/hash only; actual current-only probes are not archived-original references. A–G are exhausted. No new essential gap is identified; ordinary W2 work needs no generic permission. Stop before any genuinely indispensable new original-reference query and request a bounded owner decision.

All 4695 tracked paths/278 pins/100 captures/16 owner documents/41 original snapshots are independently preserved live. Publish/export only the explicit manifest inventory: exclude all raw full-managed Debug diagnostics and Python caches physically, while retaining them locally. The post-seal sibling receipt records actual live/copy/ZIP checks. Omitted payloads cannot be reverified from a compact copy. Keep all older manifests/decisions unchanged.

W3 remains NOT_STARTED. A new fail-closed whole-batch readiness verifier with zero unsupported mandatory OPEN, genuine branch/statement/inline/support proofs and meaningful mutations is required before conditional W3. No READY or owner acceptance is implied. P=NO;N1=NO GO;N2=NO;C2=UNMET;C3=UNAPPROVED;NOT_GOLDEN. Native/manual/scientific/install/restore/signing/deployment and 04A.39 remain outside this checkpoint.
'''
with (HERE/'NEXT_WAVE_HANDOVER.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write(handover)
save('deferred-compatibility-constraints.json',dict(prior_charter=pin(PREVIOUS/'deferred-defect-candidates.json'),
    constraints=['Signed large integer cell text can overflow after Double rounding; unsigned fallback differs by original type domain',
                 'Invariant-first alpha text can reject locale-style comma decimal even under sk-SK; fixed missing default remains.05',
                 'Empty selected group columns prevent automatic header detection; retained names/default numbering stay original',
                 'Numeric TypeCode enum eligibility is preserved, including host ExcelError classification; no new original report query performed'],
    disposition='Compatibility constraints and deferred-source findings only; no scientific correction or 04A.39–48 implementation',original_queries=0))
print('Owner review and exact next-wave handover written')
