"""Render the partial audit report from source-derived ledgers and local run evidence.

Only audit artifacts are written. Run after analyzer/catalog generation and validation.
The run timestamp is isolated in audit-run.json; other source tables are deterministic.
"""
from __future__ import annotations
import collections
import csv
import datetime
import hashlib
import json
import pathlib
import re
import sys
import subprocess
import xml.etree.ElementTree as ET
import catalog
import semantic

ROOT, OUT = catalog.ROOT, catalog.DEFAULT
EVIDENCE = ROOT / "TestResults/04A-01"
CHECK = "--check" in sys.argv


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write(name, text):
    content = text.rstrip() + "\n"
    if CHECK:
        if not (OUT / name).is_file() or (OUT / name).read_text(encoding="utf-8") != content:
            raise RuntimeError("Report drift: " + name)
    else:
        (OUT / name).write_text(content, encoding="utf-8", newline="")


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def git(*args):
    return subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), *args], cwd=ROOT, text=True).rstrip()


def log_text(path):
    data = path.read_bytes()
    return data.decode("utf-16" if data.startswith((b"\xff\xfe", b"\xfe\xff")) else "utf-8-sig", errors="replace")


def main():
    coverage = read_json(OUT / "coverage.json")
    review = read_json(OUT / "review-coverage.json")
    semantic_review = read_json(OUT / "semantic-review-coverage.json")
    authored = read_json(OUT / "catalog-coverage.json")
    files, symbols, edges = (catalog.load(OUT, x) for x in ("files.csv", "symbols.csv", "ui-wiring.csv"))
    run_path = OUT / "audit-run.json"
    if not run_path.exists():
        write("audit-run.json", json.dumps({"audit_utc": datetime.datetime.now(datetime.timezone.utc).isoformat(), "initial_branch": "main",
             "initial_head": coverage["source_head"], "initial_git_status_short": "", "initial_index_diff": "", "initial_submodules": "",
             "baseline_basis": "Observed real local Git preflight; main and local origin/main both at recorded HEAD. No remote fetch. Main contains Revision 2.1, Gate C decision and accepted 04.01-04.07 ancestry.",
             "git_trust": "Invocation-scoped -c safe.directory for checkout ownership mismatch; no global config change",
             "branch_creation": "Requested git switch -c codex/04A-01 succeeded after sandbox .git write approval",
             "ignored_state": "Existing .vs/bin/obj/packages/TestResults (including GateC) and local scripts existed. No unrelated tracked/untracked source changes at preflight. Ignored GateC hashes captured separately.",
             "scope": "Only 04A.01 audit artifacts; no owner acceptance; no staging/commit/push/merge/extraction"}, indent=2))
    family_rows = []
    for family, count in coverage["file_families"].items():
        members = [s for s in symbols if s["family"] == family]
        family_rows.append(f"| {family} | {count} | {len(members)} | {sum(s['disposition']=='needs-investigation' for s in members)} |")
    by_disposition = "\n".join(f"| `{key}` | {coverage['dispositions'].get(key, 0)} |" for key in
        ("already-in-Core", "already-in-Common", "extract-to-Core", "extract-to-Common", "retain-Windows-host-only", "compatibility-facade-to-preserve", "needs-investigation"))
    ribbons = catalog.load(OUT, "ribbon-crosswalk.csv")
    repeated = collections.defaultdict(list)
    for row in ribbons:
        repeated[row["onAction"]].append(row["ribbon_id"])
    repeat_lines = "\n".join(f"- `{action}`: " + ", ".join(f"`{x}`" for x in ids) for action, ids in repeated.items() if len(ids) > 1)
    # Copy exact role/gate/trigger wording; never rewrite or mark these closed.
    old_exceptions = (ROOT / "TestResults/GateC/exception-register.md").read_text(encoding="utf-8-sig")
    exception_table = "\n".join(line for line in old_exceptions.splitlines() if re.match(r"\| (E[1-7]|GC[3-8]) \|", line))
    plan = (ROOT / "planning/BESHStatNG_CrossPlatform_MasterPlan.md").read_text(encoding="utf-8-sig")
    batches = []
    for line in plan.splitlines():
        match = re.match(r"\| \*\*(04A\.(\d+))\*\* \|", line)
        if match and 2 <= int(match[2]) <= 48:
            parts = [part.strip() for part in line.strip("|").split("|")]
            batch = match[1]
            candidates = [s for s in symbols if s["future_batch"] == batch]
            batches.append({"batch_id": batch, "approved_objective": parts[1], "dependencies": parts[2], "risk": parts[-1].strip("*"),
                            "effort_estimate": "high: family decomposition and pinned parity" if parts[-1].strip("*") == "High" else "medium: focused boundary/reconciliation",
                            "ledger_rows_assigned": len(candidates), "candidate_extractions": sum(s["disposition"].startswith("extract-to-") for s in candidates),
                            "qualified_inspected_candidates": sum(semantic.qualified(s) and s["disposition"].startswith("extract-to-") for s in candidates),
                            "unreviewed_provisional_rows": sum(not semantic.qualified(s) for s in candidates),
                            "owner_role": "; ".join(sorted({s["owner_role"] for s in candidates})) or "Existing roadmap family/architecture owner",
                            "status": "PLANNED; requires separate owner authorization; this audit does not implement or authorize it"})
    assert len(batches) == 47 and len({b['batch_id'] for b in batches}) == 47
    write("batch-map.csv", catalog.render(batches))
    write("README.md", f"""# Windows migration ledger — Batch 04A.01

**NOT READY FOR ACCEPTANCE — accurate partial ownership audit.** Source census, syntax enumeration and individual source-member review are complete for the stated profiles; full control contracts and dynamic dispatch coverage remain incomplete. Only the project owner may accept 04A.01 or authorize 04A.02. Gate C2 is not met. No extraction or 04A.02 goldens were implemented.

Baseline: clean local `main`, `{coverage['source_head']}`, then `codex/04A-01`; source HEAD remains unchanged. Master Plan **Revision 2.1, 25 October 2026**, 40/223 accepted and 183 outstanding. Gate C GO WITH DOCUMENTED EXCEPTIONS is owner approval, including four owner-attested final native cases; this audit neither re-approves it nor claims desktop XLL/Mac certification. E1–E7 remain open; GC3–GC8 remain deferred. Audit UTC and observed preflight are in [audit-run.json](audit-run.json); parser/SDK/source history in [source-provenance.json](source-provenance.json).

## Coverage and its limits

| Family | Compiled VB files | Parsed records | Needs investigation |
|---|---:|---:|---:|
{chr(10).join(family_rows)}

{coverage['compile_files']} explicit Compile paths, {coverage['src_files']} under `src`; 29 UI Designers and 32 other UI files. {len(symbols)} source-location records are inventoried with valid dispositions and assigned follow-ups; zero omitted parsed IDs and zero parser diagnostics under the enumerated profiles. This is **syntax-record coverage**, not {len(symbols)} independently reviewed routines: property containers/accessors, initialized state and lambdas are separate records. {review['manually_reviewed_static_records']} records have pinned static body reviews; **{review['unreviewed_static_records']} have no individual manual body review**, across {len(review['files_without_manual_member_review'])} files with no pinned member review. No member's full numerical/native parity is proved by this audit. See [review-by-file.csv](review-by-file.csv).

{authored['methods']} methods / {authored['commands']} commands / {authored['pages']} pages; {authored['method_button_mappings']} method/button mappings. Ribbon: 84 buttons, 84 unique IDs, 81 distinct actions, all 84 action declarations located. Inherited `LoadImage` is verified against installed assembly metadata separately. UDF: 268 annotated callable exports in 23 of 41 files, no duplicate effective names. Common: four typed registrations for **ANOVA and Kruskal–Wallis only**. None is a live Excel registration or wiring test.

UI state: {review['controls']} WithEvents UI declarations; {len(edges)} wiring/configuration/source rows, including 16,863 Designer assignments. **{review['unresolved_edge_rows']} edge rows need target/intent review**, including controls without direct Handles; absence of a handler is not automatically a defect. [ui-controls.csv](ui-controls.csv) distinguishes decorative/container candidates, direct Handles and unresolved intent. Event counts and residual IDs are in [coverage.json](coverage.json) and [review-coverage.json](review-coverage.json).

## Navigate to any source item

- File: filter `path` in [files.csv](files.csv); SHA-256, physical LOC and counts bind the source. [supplemental-files.csv](supplemental-files.csv) covers assets/XML/resx/DNA/config/WiX/binaries and excluded tracked files. Binary assets are deployment boundaries, not portable source.
- Method/accessor/initializer/lambda: filter `path`, `name`, `containing_type` or `signature_key` in [symbols.csv](symbols.csv). `symbol_id` adds syntax span; [types.csv](types.csv), [state.csv](state.csv), [references.csv](references.csv) and [portable-symbols.csv](portable-symbols.csv) retain declarations, state and call evidence. Identical lambda signatures need span IDs to disambiguate movement.
- UI: [ui-wiring.csv](ui-wiring.csv) keys every discovered edge by `edge_id`; [ui-controls.csv](ui-controls.csv) groups declaration/default/configuration and handler evidence by partial type/control. Targets are syntax candidates unless explicitly stated otherwise.
- Ribbon/catalog: [ribbon-crosswalk.csv](ribbon-crosswalk.csv), [catalog-crosswalk.csv](catalog-crosswalk.csv), [xml-callbacks.csv](xml-callbacks.csv), [callback-index.csv](callback-index.csv), [common-registry.csv](common-registry.csv). The four authorities remain separate.
- UDF: filter `effective_name`, `export_id` or `symbol_id` in [udf-exports.csv](udf-exports.csv); explicit Excel names override VB names. The other 18 UDF helper files are included in the member census.
- Open work: [investigations.csv](investigations.csv) has an owner, batch, reason, risk and verification step for every needs-investigation member and unresolved edge. [exception-and-investigation-register.md](exception-and-investigation-register.md) retains gate obligations. [high-risk-review.md](high-risk-review.md) links the reviewed boundary findings.
- Planning/checks: [candidates-and-batch-map.md](candidates-and-batch-map.md), [batch-map.csv](batch-map.csv), [reconciliation.md](reconciliation.md), [validation.md](validation.md), [manual-smoke.md](manual-smoke.md). [index.json](index.json) is the machine-readable path/hash index.

## Dispositions

| Exact enum | Rows |
|---|---:|
{by_disposition}

`already-in-Core` / `already-in-Common` identify a checked portable implementation; `extract-to-Core` proposes numeric/model/geometry ownership; `extract-to-Common` proposes typed policy/report ownership. `retain-Windows-host-only` preserves a visible native/generated boundary. `compatibility-facade-to-preserve` retains the callable wrapper while reusing the located portable service. `needs-investigation` makes no portability assertion; its owner must inspect and verify the specified contract before changing it. Default classifications are static triage, separately labeled from pinned reviews. A reused service listed for a mixed adapter is not a whole-method replacement.

Amendment semantic denominator: **{semantic_review['qualified_source_reviews']}/{semantic_review['denominator']}** detailed member-v1 source reviews; **{semantic_review['unreviewed_for_acceptance']}** remain incomplete for acceptance, including **{semantic_review['legacy_pinned_reviews_requiring_details']}** older pins requiring detailed contract evidence. No generated family rule is counted as manual review. The latest controlled checkpoint is [continuation29.md](continuation29.md), with fresh validation in [continuation29-validation.json](continuation29-validation.json). C29 classifies622/622 frozen wiring intents in a separate sidecar; no full edge qualification or control-v1 credit is added. Original unresolved wiring remains622 and qualified controls1164/2056. C28 risk decisions892 and affected live-golden blockers802 remain unchanged. Historical [continuation28.md](continuation28.md) and the previous [continuation27.md](continuation27.md), [continuation26.md](continuation26.md), [continuation25.md](continuation25.md), [continuation24.md](continuation24.md), [continuation23.md](continuation23.md), [continuation22.md](continuation22.md), [continuation21.md](continuation21.md), [continuation20.md](continuation20.md), [continuation19.md](continuation19.md), [continuation18.md](continuation18.md), [continuation17.md](continuation17.md), [continuation16.md](continuation16.md), [continuation15.md](continuation15.md), [continuation14.md](continuation14.md), [continuation13.md](continuation13.md), [continuation12.md](continuation12.md), [continuation11.md](continuation11.md), [continuation10.md](continuation10.md), [continuation09.md](continuation09.md), [continuation08.md](continuation08.md), [continuation07.md](continuation07.md), [continuation06.md](continuation06.md), [continuation05.md](continuation05.md), [continuation04.md](continuation04.md), [continuation03.md](continuation03.md), [continuation02.md](continuation02.md), [completion-amendment.md](completion-amendment.md) and their validation files are immutable historical checkpoints, not current totals. See [semantic-review-coverage.json](semantic-review-coverage.json) and the immutable pre-amendment audit under [baseline-manifest.json](baseline-manifest.json).

## Reproduce and review

Use [the audit tool instructions](../../../scripts/windows-migration-audit/README.md). `--check` recomputes source expectations and compares every generated source table without writing. `--strict` additionally exits 2 for open ownership/wiring blockers; **it is expected to fail for this partial audit**. Reconciliation self-tests deliberately delete fixture copies of file/member/event/export/Ribbon rows and remove an owner. No network/package installation is needed: SDK-shipped Roslyn, installed Python/PyYAML, and the installed Excel-DNA assembly suffice.

All existing tracked bytes are preserved; [protected-hashes.csv](protected-hashes.csv) records before/after SHA-256 for 2,109 baseline files. Legacy verification regenerated VersionAttributes in each configuration; generated copies were retained under ignored `TestResults/04A-01` and exact originals restored. Gate C ignored evidence has a separate hash comparison; it was not edited or recreated. Full logs/TRX and protected backups remain under `TestResults/04A-01`.

**Git review note:** existing `.gitignore` ignores this new directory. It is deliberately unchanged. `git status --short` therefore lists audit tooling and the new top-level ledger index, while `git status --short --ignored`/`index.json` expose this ledger. When the owner eventually stages after review, the ledger requires explicit force inclusion. Nothing is staged or committed by this batch.
""")
    write("reconciliation.md", f"""# Reconciliation and quantified residuals

Source: `{coverage['source_head']}` on `codex/04A-01`; local `main` and local `origin/main` equal that source. The Gate C decision and Revision 2.1 are in committed main; accepted Phase 04 commits are in its ancestry. Initial tracked/index/untracked source state was clean, submodule status empty. Ignored evidence/build/local files existed. No ZIP merge assumption or remote verification is made.

## Source denominators

- Project XML: 171 distinct explicit Compile items, exactly one files.csv row each; 165 under src. Counts match the 26 October expectations: UI 61 (32+29), Graphics 38, UDF 41, DM 12, AppInfrastructure 6, RegModels 3, Help 3, Update 1; six project-support files.
- Parser `{coverage['parser']}`: {len(symbols)} distinct span IDs, {len(symbols)} ledger rows, zero missing/extra IDs; zero parser diagnostics/unparsed regions for the checked-in source profiles. Fresh SDK/build evidence is separate in continuation29-validation.json; the legacy source-provenance.json field named sdk contains CLR Environment.Version, not an SDK version. Project `_MyType=Windows` plus the generated WindowsForms alternative are parsed; only checked-in conditional directives are the two `_MyType` tests in Settings.Designer.vb. Alternative-only initializers/AutoSaveSettings are explicitly labeled. Shared property containers retain the physical source body including their conditional branch text. This union is broader than the actual active compilation; source records are not a runtime method count.
- 1,231 Sub blocks and 1,540 Function blocks across the entire Windows project are part of this record count, not the historical UI-only regex estimate. Method, accessor, property-container, initializer, lambda, Declare and event kinds are inventoried separately. A field/auto-property without initialization is state, not an executable routine. Type/delegate declarations are in types.csv. Method-body spans and enclosing symbol IDs are retained separately from declaration spans.
- UDF attributes: independent Roslyn attribute denominator 268 = 268 export rows; 23 exporting files, 18 helper files; no duplicate effective names, no unbound annotations. Arrays, error conversion, overload/thread/cache lifetime and actual XLL registration remain semantic/native follow-ups.
- Ribbon: all 84 IDs unique; 84 action references map to 81 distinct declarations in BESHStatAddIn. Three additional reuse occurrences are listed below. The 85th XML callback attribute is inherited LoadImage: installed ExcelDna.Integration 1.9.0 metadata confirms `System.Object LoadImage(System.String)`, assembly SHA-256 in external-callback-evidence.json; method not invoked.
- Catalog: 76 methods, five commands, two pages, 79 method mappings; zero missing Ribbon IDs. Common has four typed handler constructions for two analysis IDs. Registry/catalog/Office editor/host capabilities are distinct; ANOVA/KW alone are current executable Office analyses. Help/form metadata never enables an analysis.

{repeat_lines}

## Historical drift

The 18 October DM headline omitted `ExcelDnaSharedOutputPlanAdapter.vb`, `ExcelDnaCompositeOutputAdapter.vb` and `ExcelDnaOutputTargetAdapter.vb`; current 12 vs historical 9. AppInfrastructure omitted `LegacyPreferenceStore.vb`, `WindowsDiagnostics.vb`, `WindowsDocumentLifecycle.vb`, `WindowsExecutionRun.vb`; current 6 vs historical 2. These are newer accepted host/Common integrations, not seven fresh bulk-extraction candidates. Actual responsibilities are in high-risk-review.md and row-level data.

Current HEAD `11fa5d9` is a further Core forwarding-facade cleanup (20 files; 74 insertions/234 deletions). Its touched member bodies and tests supersede old ZIP/prose impressions; file/registration totals still match the 26 October expected totals. The ZIP's byte/member baseline is unavailable for a direct diff, so no exact ZIP member delta is asserted. Live SHA/signatures are authoritative. There is no Windows `src/StatTests`; migrated statistical code is in `BESHStat.Core/src/StatTests`. The historical 1,232 UI leading-declaration estimate is not a comparable Roslyn baseline and is not used as a denominator. Historical prose's old batch references are superseded by Revision 2.1's unchanged approved IDs (DM 04A.27/28; UDF 04A.38–43; infrastructure 04A.44).

## What is missing

- Missing compiled-file rows: **0**; missing parsed span IDs: **0**; unbound UDF annotations: **0**; missing Ribbon action declarations: **0**; parser errors/unparsed regions: **0** for the explicit profiles.
- **{review['unreviewed_static_records']} records lack individual static body review**, and {len(review['files_without_manual_member_review'])} of 171 files have no pinned member review. **{review['member_investigations']} ownership investigations** remain. This is the material partial-audit limitation even though every record has a valid enum/owner/batch/test proposal.
- **{review['unresolved_edge_rows']} unresolved target/intent rows**, including dynamic/external callbacks and WithEvents controls without direct Handles; see investigations.csv. Designer assignments and syntactically resolved Handles are not runtime wiring proof. Property/field indirection and COM delegate instances require semantic/fake/native host review; local variable delegates are not automatically bound through data flow.
- C29 separately classifies all622 frozen WithEvents intents:458 bounded source-intent resolutions,161 documented deferrals,1 owner decision and2 insufficient-evidence blockers. No old edge/control qualification changes; original unresolved rows remain622. Runtime NOT RUN; C28 risks and802 affected live-control golden blockers remain unchanged. See continuation29.md and continuation29-wiring-intent-triage.csv.
- Call graph is syntax evidence with exact locations; late binding, overload binding, reflection targets, resource-applied defaults, decorative intent and per-option state transitions are not proven. No human-reviewed complete scientific/UX parity finding is made for every high-risk form, all 38 graphics or all 268 UDFs. Exact Core/Common reuse is only claimed where a concrete reviewed/transparent call is listed; other rows say `none verified`.
- No live Excel, XLL, authenticated Office, Windows/Mac UI, chart export, installer/signing/deployment/publishing or new numerical/option goldens. No 04A.75 raw-bin work. Gate C owner-attested checks are historical, not new captures.

`--check` fails on source/table drift, missing rows, stale pinned body decisions, unbound UDF attributes, unknown Ribbon action declarations or duplicate IDs. `--strict` additionally fails on open ownership/parser/event blockers. Python independent project/YAML/XML checks and negative tests are documented in validation.md. New code-bearing constructs that parse outside the recognized kinds still require analyzer maintenance; the AST kinds and fixture tests document the supported grammar. No semantic-model completeness claim is made.

**NOT READY FOR ACCEPTANCE.** Tooling resolves syntax and data integrity, not the remaining semantic/COM decisions. Complete the quantified static reviews and event target/intent resolution in this authorized audit before presenting a complete 04A.01 acceptance request. C2 remains a future 04A.48 closure gate.
""")
    write("candidates-and-batch-map.md", """# Candidate dependencies, effort and risk

04A.02 comes first: owner-authorized representative Windows numerical/table/chart/option/reference capture, never bulk extraction. Capture culture, alpha, seed, physical source rows, output destination and exact defaults before modifying behavior. This audit creates no new goldens.

`batch-map.csv` retains all 47 approved 04A.02–04A.48 IDs, objectives and dependencies directly from Revision 2.1. Effort estimates are audit planning judgments, not amendments to the roadmap or authorization. Zero directly assigned rows does not mean a decomposed batch is unnecessary; broad parent-batch rows name later dependent slices in their proposed tests.

| Priority after separate approval | Existing batches | Dependency / effort / parity risk |
|---|---|---|
| Reference capture | 04A.02 | Highest priority, high effort; fix reproducible data/options/output/geometry baselines before extraction |
| Shared inputs and basic reports | 04A.03–07 | High; UibyID branch matrix, physical paired/blocked alignment, 2x2/RxC orientation, exact CI/alternative, ANOVA/KW supplemental tables; preserve E1/E3 |
| Model effect authoring | 04A.08 | High; split ListBox from TermSpec policy; reuse RegressionDesignCore and existing Common model-input grammar |
| Regression/survival/mixed | 04A.09–16 | High; design/family/link/offset/weights, censoring, KR/Satt/LS-means/reference-grid/optimizer semantics; no second fitter |
| Multivariate/SPC/PSM/planning | 04A.17–26 | High except focused planning slices; scaling/priors/labels/seeds, phases/exclusions/limits/signals and matched row IDs |
| Host input/output reconciliation | 04A.27–28 | High; retain COM conversion and real write adapters; existing Common parsers/planners already exist; E1/E3/E5/E7 |
| Graphics audit/numeric/spec split | 04A.29–37 | Audit then high-risk extraction; ROC/Q-Q mutation/ties/CI, axes/binning/series/geometry; Core histogram/violin/specialty calculations already exist; Excel placement/rendering stays Windows |
| UDF adapters/policy/caches | 04A.38–43 | High; 268 names plus 18 helper files; preserve signatures/scalar/array/native errors and handle/thread/recalc lifetime; Office Custom Functions remain Phase 11 |
| Host infrastructure | 04A.44 | Medium; preserve legacy XML/logging/Help/update/COM lifecycle; Common settings/diagnostics/execution already present; GC8 SimpleFileLogger remains open |
| Wiring and closure | 04A.45–48 | High; fake-host option/event/default/fault harness plus owner native tour; source-to-owner and reference parity closure required for Gate C2 |

The original audit proposed five numeric and four Common extractions; those original decisions remain reviewable in baseline/. The amendment adds detailed member-v1 reviews, separately counted in semantic-review-coverage.json and completion-amendment.md. Candidate allocations must be read as inspected versus provisional: a family tag on an unreviewed row is not an accepted migration allocation. Thousands of records remain open; the candidate count cannot be frozen.

No 04A.49–75 enhancement, Office capability registration or release action is authorized. In particular 04A.75 raw/PrettyBreaks mode is untouched; existing histogram behavior is a baseline obligation.
""")
    write("exception-and-investigation-register.md", f"""# Exceptions, investigations and owner actions

Source of carried authority: [Gate B freeze](../gate-b-common-v1-contract-freeze.md), [Gate C owner decision](../gate-c-owner-decision-20261025.md), and the local ignored Gate C exception register. Exact existing role/gate/trigger wording is retained below. No individual assignee or closure is invented.

| Exception | Current owner disposition | Existing owner role | Existing batch/gate | Existing deadline/trigger |
|---|---|---|---|---|
{exception_table}

E1 legacy renderer semantics need Windows/Common table/p-value/alpha/layout parity; marking a ResultTable is not renderer parity. E3 dense grouped aliases lack full per-observation source coordinates; copying arrays does not restore provenance. E5 Windows guard scope is explicitly limited and unsupported guard consumers remain rejected. E7 family details/chart/transport contracts remain unfrozen even where settings have since been implemented. GC8 `SimpleFileLogger` remains in Core (`src/AppInfrastructure/CoreInfrastructure.vb`, current line 394); Office does not install it. Do not claim a fully filesystem-free Core. E2 serializer/wire, E4 process-local consent and E6 historical checkpoint obligations retain their existing owners.

## Quantified audit open work

`investigations.csv` contains {review['member_investigations']} member and {review['unresolved_edge_rows']} edge investigations, each with source location, reason, owner role, proposed future batch/gate, verification activity and risk. `review-by-file.csv` quantifies {review['unreviewed_static_records']} records lacking a pinned static review; {len(review['files_without_manual_member_review'])} files have no pinned member review. A populated enum is not a reviewed scientific/UX conclusion. All default classifications remain static triage. Pinned reviews say `manually-reviewed-static`; only member-v1 detailed decisions satisfy the separate semantic denominator. `remaining-member-review.csv` includes every unreviewed/legacy-detail record for completion within 04A.01; provisional future batch tags do not postpone that audit obligation.

| Audit finding | Accountable role | Resolution requirement |
|---|---|---|
| Semantic coverage incomplete | Windows portability / Core/Common family maintainers; project owner reviews | Complete per-member body, exact-equivalent and test review within 04A.01; pin decisions by body hash; re-run checks |
| Dynamic/delegate/COM and no-handler intent | Windows UI/RefEdit/Ribbon maintainers | Resolve owning instance/overload/event source and detach lifetime; record intentional decorative/default paths; 04A.45–47 fake/native verification |
| High-risk form branches | Common family / Windows UI maintainers | UibyID grouping and supplementary report matrix; mixed/design/SPC/PSM/contingency branches; 04A.02 reference capture then existing 04A.03–28 slices |
| Core/Common reuse and remaining helpers | Core numeric / Common policy / Windows façade maintainers | Do not re-extract existing matrix/graphics/input/output/settings policies; verify each unresolved helper signature/semantics before future change |
| Source/ledger reproducibility | Audit tooling maintainer | New paths/members/events/exports must fail --check until classified; preserve source hashes and exact IDs |
| Native certification/evidence | Owner / release QA | Owner-run matrix in manual-smoke.md; later Windows/Mac/Office release gates; never relabel simulated/static checks native |

Gate C2 requires the owner-reviewed source-to-owner ledger, no unreviewed reusable routines, stable Windows reference output and retained compatibility boundaries after 04A.48. This partial 04A.01 audit does **not** meet it. Gate C3 and later enhancement/release gates are unchanged. Gate C's final four native checks remain historical owner attestations, not independently rerun evidence. Acceptance remains 40/223 until owner acceptance of a numbered batch.
""")
    write("high-risk-review.md", """# Reviewed boundaries and family review queue

Pinned member decisions live in `scripts/windows-migration-audit/review-decisions.json`; source tables show exact ranges and portable target declarations. The following original observations are supplemented by completion-amendment.md and the detailed member-v1 decisions. Their current qualified versus legacy-detail counts are in semantic-review-coverage.json. All observations are static inspection, not output or runtime validation.

| Located surface | Findings / next evidence |
|---|---|
| Graphics/NormalPlot.vb:57–60 | Constructor sorts arrData in place before retaining it. Preserve and expose this mutation risk; 04A.02 pre/post vector baseline, 04A.31 owner-reviewed copy policy. Blom/Rankit/Van der Waerden score and SPSS/OLS/R line routines are numeric Core candidates; addChart remains Excel COM. |
| Graphics/ROC.vb | compute, DeLongSE and MidRanks contain ROC/AUC/variance/CI/tie calculations; no complete Core ROC equivalent verified. wrapResults assembles summary/cutoff tables and marks body p-value cell (6,2), a Common report candidate (E1/E7). addROCplot stays Windows. Core BinaryClassificationCore already owns classifier summary/calibration/threshold/Brier helpers; these are not proof of ROC equivalence. |
| Graphics/ChartingFunc.vb:201–220; Histogram/CategoricalHistogram | ChartScaling calls ChartAxisScalingCore; HistogramBinsComputation and Gaussian overlay call HistogramBinningCore. Current Core binning calls private PrettyBreaks. Preserve bin edges/counts/midpoints/frequency/overlay; no 04A.75 switch. Histogram rendering still mixes arrays/options/Excel. |
| Graphics/ViolinPlot.vb and specialty wrappers | Core already has ViolinPlotCore, BulletChartCore, CategoricalHistogramCore, ConvexHullPlotCore, CumulativeDistributionPlotCore, DumbbellPlotCore, KiteChartCore, LadderPlotCore, PolarPlotCore, SankeyPlotCore. Exact forwarding rows are located separately; renderers still need label/axis/CI/reference/geometry payload review under E7. All 38 modules are enumerated; complete per-body review is pending. |
| UI/RegressionEffectsController.vb | EnsureTermSpecs/HasCategoricalMainEffect/BaseVarUsedInAnyOtherTerm are neutral dictionary/term policy candidates. SelectedVarsContainCategoricalUsage and add-main/categorical/polynomial/interaction actions read/write ListBox and show messages; split only after typed authoring fixtures. RegressionDesignCore already owns effect-key/name helpers, so no duplicate grammar. |
| UI/UibyID.vb | Full per-member and event census exists. Grouping/orientation, normality, ANOVA/KW/outlier branches and histogram/violin/box/ROC options, supplementary tables and output destination need per-tag source/option review; broad 04A.03 parent assignment names 04A.06/07/30 dependents. No complete parity conclusion claimed. |
| Ui18MMRM / Ui19LMM / UiGLM / Ui13GEE / Ui4Cox | Enumerated model/design/report and native controls remain mixed. Review ML/REML/covariance, KR/Satt, LS-means/reference grids/contrasts, family/link/offset/weights, correlation/cluster/time and censor/strata independently of WinForms. Existing Core fitting and Common typed parsers are not fresh extraction targets. |
| Ui21ControlCharts / Ui22MultivariateControlCharts / Ui11PCA / Ui20PropensityScoreMatching | Review phase/exclusion/historical-limit/signal/covariance masks, PCA/CA/MCA scaling/labels/priors/seeds, PSM match IDs/calipers/trim/weight/balance/Love plots. Exact routines, calls and existing Core candidates are indexed; full semantic comparisons remain open. |
| Ui82x2 / Ui0OneRefeditMulticol / UiTwoInputRefedits / Ui9ANOVA2nested | Preserve worksheet/scalar count orientation, zero cells, exact/mid-p and alternative/CI/report behavior, paired/blocked physical rows and nested/group labels; current member-specific future tests assign 04A.04/05/26. |
| UI/Refedit/Excel2007RefEdit.vb (nested control) | COM selection/worksheet hooks, before/after resize events, range validity and disposal are inventoried. Disposed attempts SelectionChange detach; source references and 8 project-wide RemoveHandler sites are indexed. Static presence does not prove all attach/detach lifetimes; neutral DocumentLifecycleLease already exists in Common. ExcelWindowWrapper, UiUpdateAvailable and RegressionEffectsController are included without a Designer. |
| DM/ExcelDnaSharedApplicationAdapter.vb | COM Range identity/Value2/date-system/error capture stays Windows; Common typed cells/parsers/acquisition budgets already exist. ToGroupedNumericData clones names/groups into shared dense data, but E3 physical provenance is not recreated. |
| DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb | Validates shared output/request identity, bounds/shape/value/p-value metadata and cursor constraints; builds Windows commands, write intent and table/chart actions. Common OutputLayoutPlan/OutputDocument/ResultTableWriteRequest already exist; narrow native preflight/compatibility state remains host policy. SharedOutputPlanTests verifies exact ranges/cursor/consent in fakes, not every legacy renderer (E1). |
| DM/ResultWriter/ExcelDnaCompositeOutputAdapter.vb | Prepare calls Common OfficeCompositeOutputPlanAdapter.Prepare, projects validated table formatting/alpha into Windows commands, and uses literal inactive-link fallback. Common owns composite semantics; ExcelDnaResultWriter and native placement stay host. |
| DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb | Salted IUnknown identity is process-local and releases the acquired pointer; real occupancy/formula snapshots and chart object inspection stay host. InspectWriteCore rejects unsupported Guards and caps cell inspection at 100,000; chart safety uses separate 1,000-shape quota. E4/E5 are not closed. |
| AppInfrastructure/LegacyPreferenceStore.vb | FromLegacy/ToLegacy map existing XML/version schemas to Common normalization/display settings; ReadVersion prohibits DTD and preserves unsupported future versions. Do not create another portable settings engine. |
| AppInfrastructure/WindowsDiagnostics.vb | CoreLoggerDiagnosticSink adapts safe Common DiagnosticRecord string fields into the existing logger; run/stage policy already uses Common DiagnosticEmitter. GC8 Core SimpleFileLogger remains deferred. |
| AppInfrastructure/WindowsDocumentLifecycle.vb | Native AppEvents subscriptions, UI-thread observation, RCW identity and detach belong to Windows; Common lease/snapshot own neutral invalidation. Dispose preserves borrowed RCWs. |
| AppInfrastructure/WindowsExecutionRun.vb | Existing Common ExecutionCoordinator/UserFaultMapper are reused for stages/cancellation/faults while native lifecycle is checked. Write intentionally avoids post-completion cancellation. Preserve this host scope. |
| AppGlobals / ExcelDnaHostInfrastructure / UIprocedures / Help / Update | Shared defaults/preferences/Help identifiers and fault policy already exist in Core/Common. Global application COM, legacy persistence/logging, message/focus behavior, XLL version/registration, native Help launch and updater/deployment remain host boundaries; mixed methods are explicit investigations rather than bulk extraction claims. Generated Help maps/catalog/Ribbon are unchanged. |
| RegModels/LinearAlgebra/Matrix.vb | Every legacy method is separately listed; 18 ambiguous arithmetic/solve overloads have pinned individual Core targets after shape/ByRef review. Other transparent wrappers retain exact source targets. DiagMatFromVector duplicates Core DiagonalMatrixFromVector. Pythag has a private Core counterpart and needs an API/compatibility decision. LU/SVD/QR wrapper result-shape mapping, row orientation, CloneMatrix, formatting and remaining conversions are still investigations; no blanket façade label. |
| All ExcelUDFs, including UDFdataImport and regression helpers | 268 effective names/signatures/attribute spans; 41 files, not only exporting files. Helpers contain normalization, errors, spill/output shape and model/cache ownership. Scalar finite checks, ExcelError/ExcelMissing/ExcelEmpty conversions and session handle lifetime need per-export inspection in 04A.38–43. Runtime export/thread/cache parity is not established by the attribute census. |

Existing test discovery and proposed fixtures are separate columns. `test-index.csv` is a lexical navigation aid, not a claim of member coverage. Important inspected test areas include MatrixTests/Core arithmetic/decomposition/factorization, HistogramBinningFacadeTests/Core histogram tests, SharedOutputPlanTests, CompositeOutputPlanTests, ClassificationReportingTests, PreferenceAdapterTests, DiagnosticAdapterTests, ExecutionAdapterTests and DocumentLifecycleAdapterTests. Further 04A.02 pinned numerical/table/chart/UI goldens require owner authorization.
""")
    chosen = [("Descriptive / assumptions", "DescriptiveStatistics"), ("Normality / outliers", "NormalityTests"),
              ("Parametric", "OneWayANOVA"), ("Nonparametric", "KruskalWallisTest"), ("2×2", "T2x2Table"), ("R×C", "RxCTable"),
              ("Regression", "GeneralizedLinearModelsGLM"), ("Mixed models", "MixedModelsForRepeatedMeasuresMMRM"),
              ("Survival", "CoxRegression"), ("Multivariate", "PrincipalComponentAnalysis"), ("SPC", "ControlCharts"),
              ("Multivariate SPC", "MultivariateControlCharts"), ("PSM", "PropensityScoreMatching"), ("Sample size", "SSicc"),
              ("Graphics", "Histogram")]
    catrows = catalog.load(OUT, "catalog-crosswalk.csv")
    smoke_rows = []
    for label, key in chosen:
        row = next(r for r in catrows if r["catalog_id"] == key)
        smoke_rows.append(f"| {label} | `{row['ribbon_id']}` / `{row['onAction']}` | Owner fixture matching documented method, alpha/seed and physical rows recorded |")
    write("manual-smoke.md", """# Owner-run Windows smoke matrix — NOT EXECUTED by Codex

Record source commit 11fa5d99a040de2e62c75ec201c49852c1d6b289 plus reviewed audit patch hash, Windows build, Excel version/channel and 32/64-bit bitness, .NET Framework 4.8/Excel-DNA 1.9.0, culture/date system, add-in load path, fixture path/SHA-256 and expected results source. Use disposable copies of existing licensed regression fixtures; stage304 ANOVA/KW CSVs and Core/legacy test datasets are reference starting points. Do not expose private workbook data in logs. Repeat native XLL activation for both bitness packages on corresponding Excel environments; a build/packed file is not load certification.

For each representative below: open from its exact Ribbon control, record initial defaults, toggle essential options, reset/enable dependencies and each supported output target; test empty/mixed/error/nonfinite input and invalid option combinations. With RefEdit, change worksheet/workbook/selection after capture, close the source and test stale-source refusal/focus return. Exercise Cancel/busy/progress/Retry where actually supported. Inspect table numeric p-values, display/alpha coloring, headers/footnotes/row labels and one relevant chart's series/axes/CI/reference labels. Record actual observations separately from this instruction list.

| Family | Exact representative Ribbon ID / callback | Fixture |
|---|---|---|
""" + "\n".join(smoke_rows) + """

Include paired/blocked dialog variants (UiTwoInputRefedits/Ui9ANOVA2nested), Ui82x2 worksheet versus scalar counts, MMRM/LMM covariance and LS-means options, PCA labels and SPC phase/exclusion options. Add a normal plot on a saved vector and compare caller order before/after; current constructor sorts its input. Compare ROC table cutoffs/AUC/CI and plotted points using known ties. These observations identify parity needs; do not bless an unreviewed numerical change.

UDF representatives: scalar `BESH.DIST.PRTRNG`; array `BESH.PLOT.HIST_BINS`; model/cache `BESH.REGR.LM_FIT` followed by `BESH.REGR.LM_SUMMARY` and `BESH.REGR.LM_RESID`. Follow the existing Help signatures, record exact formulas and argument order from udf-exports.csv, recalculate, change inputs, test invalid/missing/native-error cells, resize spill areas and close/reopen the workbook. Record stale/invalid handle outcomes and parallel recalc observations without assuming cache lifetime.

Utilities: About/version; Help home and dialog-context topic; settings save/reload/default alpha/seed/p-value display; sanitized log/fault display; update prompt inspection/cancel (no download/install); existing Windows chart export to an owner-approved disposable path (no release claim). Check RefEdit disposal after repeated open/close and workbook/sheet changes. Office/WASM optional owner smoke remains separate: production Office ANOVA/KW input→new-sheet guarded output with native readback; independent WasmSmoke is not production-host evidence.

This is a minimal sampling plan, not all-option numerical qualification. Full pinned numeric/table/chart/option goldens are reserved for separately approved 04A.02. Windows native, Office authenticated, Mac and installer certification remain unexecuted and assigned to their owner/release gates. No manual pass is claimed.
""")
    # Tracked-source and ignored Gate C preservation evidence, without copying its contents.
    before = read_json(EVIDENCE / "protected/before.json")
    protected = [{"path": r["path"], "before_sha256": r["sha256"], "after_sha256": sha(ROOT / r["path"]), "status": "PASS" if r["sha256"] == sha(ROOT / r["path"]) else "FAIL"} for r in before]
    assert all(r["status"] == "PASS" for r in protected), "Protected source bytes differ"
    write("protected-hashes.csv", catalog.render(protected))
    gate_before = read_json(EVIDENCE / "gate-c-hashes-before.json")
    gate_rows = [{"path": r["path"], "before_sha256": r["sha256"], "after_sha256": sha(ROOT / r["path"]), "status": "PASS" if r["sha256"] == sha(ROOT / r["path"]) else "FAIL"} for r in gate_before]
    assert all(r["status"] == "PASS" for r in gate_rows), "Gate C evidence changed"
    write("gate-c-evidence-hashes.csv", catalog.render(gate_rows))
    ignored_vb=[]
    for p in sorted((ROOT / "BESHstatNG").rglob("*.vb")):
        if any(part.lower() in ("bin", "obj") for part in p.relative_to(ROOT / "BESHstatNG").parts):
            ignored_vb.append({"path":p.relative_to(ROOT).as_posix(),"sha256":sha(p),"classification":"ignored build-generated VB output; not an authored explicit Compile source denominator","evidence_status":"environment snapshot; may change with builds or be absent on a clean machine"})
    write("ignored-vb-outputs.csv", catalog.render(ignored_vb))
    trxrows, logrows = [], []
    for config in ("Debug", "Release"):
        for p in sorted((EVIDENCE / config).glob("*.trx")):
            if p.name not in ("Core.trx", "Common.trx", "OfficeAddin.trx", "OfficeExecution.trx", "Legacy.trx", f"OfficeAddin-{config}.trx"):
                continue  # Existing standard result folders may also contain the other configuration.
            tree = ET.parse(p)
            counters = tree.find(".//{*}Counters")
            trxrows.append({"configuration": config, "suite": p.name, "result": "PASS" if counters.get("failed") == "0" else "FAIL", **counters.attrib, "path": p.relative_to(ROOT).as_posix(), "sha256": sha(p)})
        for name in ("full", "foundation"):
            p = EVIDENCE / config / (name + ".log")
            text = log_text(p)
            logrows.append({"configuration": config, "command_kind": name, "python_suite_counts": re.findall(r"Ran (\d+) tests? in", text),
                            "node_suite_counts": re.findall(r"(?:# |ℹ )tests (\d+)", text), "log": p.relative_to(ROOT).as_posix(), "sha256": sha(p),
                            "failure_summary_present": bool(re.search(r"(?m)^FAILED |^FAIL: |^not ok ", text))})
    write("validation-results.json", json.dumps({"script_commands": read_json(EVIDENCE / "validation-runs.json"), "managed_trx": trxrows,
           "python_node_summaries": logrows, "protected_baseline_files_unchanged": len(protected), "gate_c_evidence_files_unchanged": len(gate_rows),
           "not_run": ["live Excel/XLL/UI and native UDF dispatch", "authenticated Office/Edge native scenarios", "Mac", "installer/signing/deploy/publish", "04A.02 goldens"]}, indent=2))
    table = "\n".join(f"| {r['configuration']} | {r['suite']} | {r['result']} | {r['passed']} | {r['failed']} | {r.get('notExecuted','0')} |" for r in trxrows)
    logtable = "\n".join(f"| {r['configuration']} {r['command_kind']} | {', '.join(r['python_suite_counts']) or '—'} | {', '.join(r['node_suite_counts']) or 'see log summary'} |" for r in logrows)
    write("validation.md", f"""# Historical initial-audit validation evidence

These are the original audit's recorded runs under TestResults/04A-01. Current amendment reruns, failures/skips, installed-toolchain differences and checks are reported separately in completion-amendment.md and amendment-validation.json. This historical evidence does not replace current source review.

SDK 10.0.402 / runtime 10.0.13, Roslyn VB 5.9.0.0; Windows VS 2026 installed toolchain; Python 3.11.5 with PyYAML 6.0.3; Node v24.20.0. No installation, network package source, live Excel, installer or signing operation was used. Existing package cache/local `packages` were used with `RestoreSources=<repo>/packages` and `NuGetAudit=false`. Audit-tool restore uses its own cleared NuGet.Config and SDK assemblies only.

Exact commands and file hashes are in [validation-results.json](validation-results.json); full logs/TRX in ignored `TestResults/04A-01/{{Debug,Release}}`. The runner calls:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/windows-migration-audit/run-validation.ps1
# For each Debug and Release, using installed PythonExe:
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1 -Configuration <configuration> -VisualStudio 2026 -Legacy -Wasm -Js -Python -PythonExe C:/Users/peter/Desktop/WPy64-31150/python-3.11.5.amd64/python.exe
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-office-foundation.ps1 -Configuration <configuration>
```

Both full and foundation commands exited **0/PASS** in both configurations. Metadata check subcommands ran through `-Python`, without regeneration: `validate_methods_catalog.py`, `tools/verify_stage304_inventory.py`, `generate_analysis_metadata.py --check`, `generate_help_links.py --methods-yml ... --project-dir ... --check`, and resolver-route check. Existing Python unit tests ran with temporary negative fixtures. WASM builds were builds, not browser/Excel tests. JS suites use mocks, not authenticated Excel.

| Configuration | TRX suite | Outcome | Passed | Failed | Not executed |
|---|---|---|---:|---:|---:|
{table}

OfficeAddin-Configuration.trx is the separate foundation rerun of the same suite as OfficeAddin.trx; do not sum duplicate test identities. Distinct managed full-run counts are 1,024 Core + 742 Common + 505 OfficeAddin + 45 execution + 790 legacy = **3,106 per configuration**. These current counts differ from historical all-evidence totals; no attempt is made to target or reproduce the historical 4,075 or 21 simulated Edge scenarios.

| Command/config | Python suite counts in execution order | Node suite counts in execution order |
|---|---|---|
{logtable}

Read full logs for test identities and warnings; machine counts/hashes are retained above. No existing test was weakened. No unrelated baseline failure was found by these completed commands.

Audit checks: installed-SDK `dotnet build scripts/windows-migration-audit/WindowsMigrationAudit.csproj --configfile scripts/windows-migration-audit/NuGet.Config -v minimal` passed with zero warnings/errors. Roslyn `--self-test` passes ten fixture assertions (multiline/generic/overloaded members, constructors, properties/accessors, custom events, Declare, delegates, operators, lambdas, shared/static/As New initializers and negative set reconciliation). Python `test_reconciliation.py` passes seven tests including deliberately missing file/member/event/export/Ribbon rows and missing owner in copied fixtures. Source `--check` and catalog `--check` must pass on the final ledger; `--check --strict` must return **2/open blockers**, an expected incomplete-audit failure, not a product regression. Final command evidence is retained in `TestResults/04A-01/final-checks.json` and the final report.

The first audit-fixture assertion had an incorrect expected count and failed; it was corrected to count initialized state and the final extended fixture passes. The first validation wrapper stopped because PowerShell 5.1 treated successful Python unittest stderr progress as terminating; the wrapper was corrected and all four final script commands above ran to completion. Those interrupted attempts are not counted as passed full suites. The protected-byte checker also needed a PowerShell 5.1 JSON-array enumeration correction; false change reports did not alter any file. The final-check wrapper initially split its invocation-scoped Git configuration into separate arguments; this was corrected and the direct Git diff check passed before the complete final wrapper rerun. These were audit-tool development failures, not product-suite failures.

Before/after: all **{len(protected)} tracked baseline files** retain exact SHA-256; all **{len(gate_rows)} existing ignored Gate C evidence files** retain exact SHA-256. Existing Codex/04-01 results were backed up before the scripts overwrote their standard result names. New logs/TRX, bin/obj outputs, protected-byte backups and regenerated VersionAttributes copies are under ignored outputs. Only VersionAttributes changed transiently during each legacy build; generated copies were kept and originals restored byte-for-byte. No Help/catalog/README/project/manifest/gate report change remains.

**NOT RUN**: owner manual matrix, Excel desktop XLL/native UDF runtime, authenticated Office, Mac, installer/signing/deployment/publishing and 04A.02 goldens; zero native pass claims. **SKIP**: no missing installed prerequisite required a suite skip. Audit semantic review remains incomplete regardless of green product tests. `git diff --check` and final `git status --short` are recorded after artifact generation; all changes remain unstaged/uncommitted.
""")
    index = {"source_head": coverage["source_head"], "audit_run": "audit-run.json", "acceptance_status": "NOT READY FOR ACCEPTANCE",
             "self_hash_note": "index.json excludes itself; audit-run.json isolates the deliberately volatile timestamp", "artifacts": []}
    paths = [p for p in OUT.rglob("*") if p.is_file() and p != OUT / "index.json"]
    paths += [p for p in (ROOT / "scripts/windows-migration-audit").iterdir() if p.is_file()]
    paths.append(ROOT / "docs/architecture/WindowsMigrationLedger.md")
    for p in sorted(paths):
        index["artifacts"].append({"path": p.relative_to(ROOT).as_posix(), "bytes": p.stat().st_size, "sha256": sha(p)})
    index["authority_inputs"] = [{"path": p, "sha256": sha(ROOT / p)} for p in ("AGENTS.md", "planning/BESHStatNG_CrossPlatform_MasterPlan.md",
        "docs/architecture/gate-c-owner-decision-20261025.md", "docs/architecture/gate-c-office-application-layer-exit.md",
        "docs/architecture/gate-b-common-v1-contract-freeze.md", "planning/BESHStatNG_Phase04A_Windows_Portability_Audit_20261018.md",
        "BESHstatNG/BESHStatNG.vbproj", "BESHstatNG/Resources/RibbonXML.xml", "BESHStatNG_Help_MkDocs/docs/_meta/methods.yml")]
    final_checks=ROOT / 'TestResults/04A-01-continuation-29/publication-checks.json'
    if final_checks.exists():
        index["final_checks"]={"path":final_checks.relative_to(ROOT).as_posix(),"sha256":sha(final_checks)}
    write("index.json", json.dumps(index, indent=2))
    print(json.dumps({"report": "NOT READY FOR ACCEPTANCE", "source_records": len(symbols), "artifacts_indexed": len(index['artifacts']), "protected_unchanged": len(protected), "gate_c_unchanged": len(gate_rows)}, indent=2))


if __name__ == "__main__":
    main()
